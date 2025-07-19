#if USING_7ZIP
using System.Text;
#endif
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Nuke.Common;
using Nuke.Common.Git;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.Coverlet;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Tools.Git;
using Nuke.Common.Tools.GitVersion;
using Nuke.Common.Tools.MauiCheck;
using Nuke.Common.Tools.SonarScanner;
using Nuke.Common.Utilities.Collections;
using Serilog;
using static Nuke.Common.Tools.DotNet.DotNetTasks;

// ReSharper disable InconsistentNaming
// ReSharper disable UnusedMember.Local
#pragma warning disable IDE1006 // Naming Styles
#pragma warning disable CA1050 // Declare types in namespaces

public partial class Build : NukeBuild
{
    /// Support plugins are available for:
    ///   - JetBrains ReSharper        https://nuke.build/resharper
    ///   - JetBrains Rider            https://nuke.build/rider
    ///   - Microsoft VisualStudio     https://nuke.build/visualstudio
    ///   - Microsoft VSCode           https://nuke.build/vscode
    public static int Main() => Execute<Build>(x => x.Pack);

    #region Options

    [Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")]
    public readonly Configuration Configuration = IsLocalBuild ? Configuration.Debug : Configuration.Release;

    [Parameter("Environment to build - Default is 'Development' (local) or 'Production' (server)")]
    public readonly Environment Environment = IsLocalBuild ? Environment.Development : Environment.Production;

    [Parameter("Platform to build - Default is 'AnyCPU'")]
    public string Platform = "AnyCPU";

    [Parameter("Warning Level")]
    public readonly int WarningLevel;

    [Parameter("Dry Run")]
    public readonly bool DryRun;

    #endregion

    #region MAUI

    [Parameter("Package Signing", Name = "Signing")]
    public readonly bool PackageSigning;

    [Secret]
    [Parameter("Android Signing Key Alias")]
    public readonly string AndroidSigningKeyAlias;

    [Secret]
    [Parameter("Android Signing Key Pass")]
    public readonly string AndroidSigningKeyPass;

    [Secret]
    [Parameter("Android Signing Store Pass")]
    public readonly string AndroidSigningStorePass;

    #endregion

    #region Locations

    [Solution]
    public readonly Solution Solution;

    [GitRepository]
    public readonly GitRepository Repository;

    static AbsolutePath SourceDirectory => RootDirectory / "src";

    static AbsolutePath TestsDirectory => RootDirectory / "test" / "results";

    static AbsolutePath PublishDirectory => RootDirectory / "publish";

    static AbsolutePath ArtifactsDirectory => RootDirectory / "output";

    #endregion

    #region Information

    [Parameter]
    public readonly string Author;

    [Parameter]
    public readonly string Product;

    [Parameter]
    public readonly string ProjectUrl;

    [Parameter]
    public readonly string PackageId;

    GitVersion _version = null;

    bool _useMaui;
    string _repoUrl = string.Empty;
    string _androidExt = string.Empty;
    string _iOSExt = string.Empty;

    #endregion

    #region Projects

    [Required, Parameter("Projects to Build and Deploy")]
    public readonly string[] Projects;

    private Dictionary<string, string[]> _projects;
    private Project[] _webProjects;
    private Project[] _serviceProjects;
    private Project[] _desktopProjects;
    private Project[] _mobileProjects;
    private Project[] _packageProjects;
    private Project[] _testProjects;

    #endregion

    Target Prepare => d => d
        .Before(Restore)
        .Executes(() =>
        {
            #region Projects

            _projects = Projects.Select(m => new
                {
                    key = m.Split(":").FirstOrDefault(),
                    value = m.Split(":").LastOrDefault()?.Split(",").ToArray()
                })
                .ToDictionary(k => k.key, v => v.value);

            Log.Information("Reading projects");
            foreach (var item in Solution.AllProjects)
            {
                var project = item.GetMSBuildProject();
                var build = project.Imports.FirstOrDefault(m => m.ImportedProject.EscapedFullPath.EndsWith("Directory.Build.props"));
                if (build.ImportedProject != null)
                {
                    // Variables
                    var pg1 = build.ImportedProject.PropertyGroups.FirstOrDefault(m => m.Properties.Any(p => p.Name == "SolutionDir"));
                    pg1?.SetProperty("SolutionDir", Solution.Directory);
                    if (build.ImportedProject.HasUnsavedChanges)
                    {
                        build.ImportedProject.Save();
                    }

                    build.ImportedProject.Reload();
                }

                project.ReevaluateIfNecessary();
            }

            _webProjects = _projects.ContainsKey("Web") ? Solution.AllProjects.Where(m => _projects["Web"].Contains(m.Name)).ToArray() : Array.Empty<Project>();
            _serviceProjects = _projects.ContainsKey("Service") ? Solution.AllProjects.Where(m => _projects["Service"].Contains(m.Name)).ToArray() : Array.Empty<Project>();
            _desktopProjects = _projects.ContainsKey("Desktop") ? Solution.AllProjects.Where(m => _projects["Desktop"].Contains(m.Name)).ToArray() : Array.Empty<Project>();
            _mobileProjects = _projects.ContainsKey("Mobile") ? Solution.AllProjects.Where(m => _projects["Mobile"].Contains(m.Name)).ToArray() : Array.Empty<Project>();
            _packageProjects = _projects.ContainsKey("Package") ? Solution.AllProjects.Where(m => _projects["Package"].Contains(m.Name)).ToArray() : Array.Empty<Project>();
            _testProjects = _projects.ContainsKey("Test") ? Solution.AllProjects.Where(m => _projects["Test"].Contains(m.Name)).ToArray() : Array.Empty<Project>();

            #endregion

            #region Properties

            using (var gitUrl = new Process())
            {
                gitUrl.StartInfo = new ProcessStartInfo(fileName: "git", arguments: "config --get remote.origin.url") { WorkingDirectory = SourceDirectory, RedirectStandardOutput = true, UseShellExecute = false };
                gitUrl.Start();
                _repoUrl = gitUrl.StandardOutput.ReadLine()?.Trim().Split(separator: " ", StringSplitOptions.RemoveEmptyEntries).LastOrDefault();
                gitUrl.WaitForExit();
            }

            _useMaui = Solution.AllProjects.Select(m =>
            {
                var evaluatedValue = m.GetMSBuildProject()?.GetProperty("UseMaui")?.EvaluatedValue;
                return evaluatedValue != null && bool.Parse(evaluatedValue);
            }).Any(m => m);

            #endregion

            #region Specials

            if (!_useMaui)
            {
                return;
            }

            _androidExt = Environment == Environment.Production ? "aab" : "apk";
            _iOSExt = Platform != null && Platform.Contains("iPhoneSimulator", StringComparison.InvariantCultureIgnoreCase) ? "app" : "ipa";

            #endregion
        });

    Target Restore => d => d
        .DependsOn(Prepare)
        .Executes(() =>
        {
            Log.Information("Restoring tools");
            DotNetToolRestore(s => s.SetProcessWorkingDirectory(SourceDirectory));

            try
            {
                if (_useMaui)
                {
                    Log.Information("Checking for MAUI workload installation");
                    MauiCheckTasks.MauiCheck(c => c.SetNonInteractive(true));

                    // var workloads = DotNet("workload list");
                    // var isMauiInstalled = workloads.Any(x => x.Text.Contains("maui", StringComparison.OrdinalIgnoreCase));
                    // if (!isMauiInstalled)
                    // {
                    //     Log.Information("Installing MAUI workload...");
                    //     var workloadId = "maui-mobile";
                    //     if (Platform.Contains("Android"))
                    //     {
                    //         workloadId = "maui-android";
                    //     }
                    //     else if (Platform.Contains("iPhone"))
                    //     {
                    //         workloadId = "maui-ios";
                    //     }
                    //
                    //     DotNetWorkloadInstall(options =>
                    //         options.SetWorkloadId(workloadId)
                    //     );
                    // }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "{Message}", ex.Message);
                throw;
            }

            Log.Information("Restoring nugets");
            DotNetRestore(s => s
                .SetWarningLevel(WarningLevel)
                .SetVerbosity(getDotNetVerbosity())
                .SetPlatform(Platform)
                .SetConfigFile(RootDirectory / "nuget.config")
                .CombineWith(Solution.AllProjects, (x, v) => x
                    .SetProjectFile(v)));
        });

    Target Clean => d => d
        .Before(Prepare)
        .Executes(() =>
        {
            Log.Information("Cleaning Output Directories");
            SourceDirectory.GlobDirectories("**/bin", "**/obj").ForEach((path) => path.DeleteDirectory());
            TestsDirectory.GlobDirectories("**/bin", "**/obj").ForEach((path) => path.DeleteDirectory());
            Log.Information("Cleaning Publish Directory");
            AbsolutePath.Create(PublishDirectory).CreateOrCleanDirectory();
            Log.Information("Cleaning Artifacts Directory");
            AbsolutePath.Create(ArtifactsDirectory).CreateOrCleanDirectory();

            if (!DryRun)
            {
                return;
            }

            Log.Information("Cleaning NuGet Cache Directory");
            var packages = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.UserProfile), ".nuget", "packages");
            if (Path.Exists(packages))
            {
                Directory.Delete(packages, true);
            }
        });

    Target Versioning => d => d
        .DependsOn(Prepare)
        .Executes(() =>
        {
            _version = GitVersionTasks.GitVersion().Result;

            if (Environment == Environment.Development)
            {
                return;
            }

            var assemblyInfoFiles = SourceDirectory.GetFiles("AssemblyInfo*.cs", int.MaxValue);
            foreach (var assemblyInfoVersionFile in assemblyInfoFiles)
            {
                Log.Information(messageTemplate: "Patching: {File}", assemblyInfoVersionFile);

                var content = File.ReadAllText(assemblyInfoVersionFile);
                content = AssemblyVersionRegex().Replace(content, $"[assembly: AssemblyVersion(\"{_version.AssemblySemVer}\")]");
                content = AssemblyFileVersionRegex().Replace(content, $"[assembly: AssemblyFileVersion(\"{_version.AssemblySemFileVer}\")]");
                content = AssemblyInformationalVersionRegex().Replace(content, $"[assembly: AssemblyInformationalVersion(\"{_version.InformationalVersion}\")]");

                File.WriteAllText(assemblyInfoVersionFile, content);
            }

            if (!_useMaui)
            {
                return;
            }

            if (Verbosity == Verbosity.Minimal)
            {
                Console.WriteLine($"// ApplicationVersion: {_version.AssemblySemVer}");
                Console.WriteLine($"// ApplicationDisplayVersion: {_version.FullSemVer}");
            }
            else
            {
                Log.Information($"ApplicationVersion: {_version.AssemblySemVer}");
                Log.Information($"ApplicationDisplayVersion: {_version.FullSemVer}");
            }

            var items = loadPublishProjects();
            foreach (var item in items.Where(m => m.useMaui))
            {
                var project = item.project.GetMSBuildProject();

                // Version
                project.SetProperty("ApplicationVersion", _version.AssemblySemVer);
                project.SetProperty("ApplicationDisplayVersion", _version.FullSemVer);

                project.Save(item.project.Path);
                project.ReevaluateIfNecessary();
            }
        });

    Target Compile => d => d
        .DependsOn(Clean)
        .DependsOn(Prepare)
        .DependsOn(Restore)
        .DependsOn(Versioning)
        .Executes(() =>
        {
            var items = loadPublishProjects();
            var target = from item in items
                from framework in item.project.GetTargetFrameworks()?.Where(m => !string.IsNullOrEmpty(m))
                select new { framework, item.project };

            if (_useMaui)
            {
                target = from item in target
                    where (item.framework.Contains("ios", StringComparison.OrdinalIgnoreCase) && Platform.Contains("iPhone", StringComparison.InvariantCultureIgnoreCase)) ||
                          item.framework.Contains("android", StringComparison.OrdinalIgnoreCase) && Platform.Contains("Android", StringComparison.InvariantCultureIgnoreCase)
                    select new { item.framework, item.project };

                DotNetBuild(s => s
                    .SetProcessWorkingDirectory(Solution.Directory)
                    .SetWarningLevel(WarningLevel)
                    .SetVerbosity(getDotNetVerbosity())
                    .SetConfiguration(Configuration)
                    .SetProperty("Environment", Environment.ToString())
                    .EnableNoRestore()
                    .CombineWith(target, configurator: (x, v) => x
                        .SetProjectFile(v.project.Path)
                        .When(w => w.Framework.Contains("ios", StringComparison.OrdinalIgnoreCase), c => c.SetProperty("ArchiveOnBuild", true))
                        .When(w => w.Framework.Contains("ios", StringComparison.OrdinalIgnoreCase), c => c.SetProperty("RuntimeIdentifier", "ios-arm64"))
                        .When(w => w.Framework.Contains("android", StringComparison.OrdinalIgnoreCase), c => c.SetProperty("AndroidPackageFormats", _androidExt))
                        .When(w => w.Framework.Contains("android", StringComparison.OrdinalIgnoreCase), c => c.SetProperty("RuntimeIdentifier", "android-arm64"))
                    ));
                return;
            }

            DotNetBuild(s => s
                .SetProcessWorkingDirectory(Solution.Directory)
                .SetWarningLevel(WarningLevel)
                .SetVerbosity(getDotNetVerbosity())
                .SetConfiguration(Configuration)
                .SetProperty("Environment", Environment.ToString())
                .EnableNoRestore()
                .CombineWith(target, configurator: (x, v) => x
                    .SetProjectFile(v.project.Path)
                ));
        });

    Target Tests => d => d
        .DependsOn(Compile)
        .Executes(() =>
        {
            if (!_testProjects.Any())
            {
                throw new OperationCanceledException("No tests found");
            }

            AbsolutePath.Create(TestsDirectory).CreateOrCleanDirectory();

            DotNetBuild(c => c.SetProjectFile(Solution.Path));

            DotNetTest(s => s
                .EnableNoRestore()
                .EnableNoBuild()
                .SetConfiguration(Configuration)
                .SetResultsDirectory(TestsDirectory)
                .SetCollectCoverage(true)
                .SetCoverletOutputFormat(CoverletOutputFormat.cobertura)
                .SetLoggers("trx", "html")
                .SetDataCollector("XPlat Code Coverage")
                .CombineWith(_testProjects, configurator: (x, v) => x
                    .SetProjectFile(v.Path)));

            // var covertura = TestsDirectory.GlobFiles("**/coverage.cobertura.xml");
            //
            // ReportGeneratorTasks.ReportGenerator(r => r
            //     .CombineWith(covertura, (s, p) => s
            //         .SetReports(p)
            //         .SetTargetDirectory(TestsDirectory)
            //         .SetReportTypes(ReportTypes.Html, ReportTypes.Cobertura, ReportTypes.MarkdownSummary, ReportTypes.SonarQube)
            //     ));
        });

    Target UITest => d => d
        .DependsOn(Compile)
        .Executes(() =>
        {
            DotNetToolRestore(s => s.SetProcessWorkingDirectory(SourceDirectory));

            var testsProjects = _testProjects.Where(m => m.Name.Contains("UI", StringComparison.OrdinalIgnoreCase)).ToArray();

            DotNetTest(s => s
                .SetVerbosity(getDotNetVerbosity())
                .SetConfiguration(Configuration)
                .CombineWith(testsProjects, configurator: (x, v) => x
                    .SetProjectFile(v.Path)));
        });

    Target Publish => d => d
        .DependsOn(Clean)
        .DependsOn(Prepare)
        .DependsOn(Restore)
        .DependsOn(Versioning)
        .Executes(() =>
        {
            var items = loadPublishProjects();
            var publishProjects = from item in items
                from framework in item.project.GetTargetFrameworks()?.Where(m => !string.IsNullOrEmpty(m))
                select new { item.project, framework };

            if (_useMaui)
            {
                publishProjects = from item in publishProjects
                    where (item.framework.Contains("ios", StringComparison.OrdinalIgnoreCase) && Platform.Contains("iPhone", StringComparison.InvariantCultureIgnoreCase)) ||
                          item.framework.Contains("android", StringComparison.OrdinalIgnoreCase) && Platform.Contains("Android", StringComparison.InvariantCultureIgnoreCase)
                    select new { item.project, item.framework };
            }

            var hasWeb = (_projects.ContainsKey("Web") && _projects["Web"].Any());
            var hasService = (_projects.ContainsKey("Service") && _projects["Service"].Any());
            var hasMobile = (_projects.ContainsKey("Mobile") && _projects["Mobile"].Any());
            var hasDesktop = (_projects.ContainsKey("Desktop") && _projects["Desktop"].Any());
            var hasPackage = (_projects.ContainsKey("Package") && _projects["Package"].Any());

            if (hasWeb || hasService)
            {
                DotNetPublish(s => s
                    .SetWarningLevel(WarningLevel)
                    .SetVerbosity(getDotNetVerbosity())
                    .EnableNoRestore()
                    .EnableNoBuild()
                    .SetConfiguration(Configuration)
                    .DisablePublishSingleFile()
                    .CombineWith(publishProjects, configurator: (x, v) => x
                        .SetProject(v.project)
                        .SetFramework(v.framework)
                        .SetOutput($"{PublishDirectory}/{v.project.Name}")));
            }
            else if (hasMobile)
            {
                DotNetPublish(s => s
                    .SetWarningLevel(WarningLevel)
                    .SetVerbosity(getDotNetVerbosity())
                    .SetConfiguration(Configuration)
                    .SetPlatform(Platform)
                    .CombineWith(publishProjects, configurator: (x, v) => x
                        .SetProject(v.project)
                        .SetOutput(PublishDirectory / v.project.Name / v.framework)
                        .SetFramework(v.framework)
                        .When(w => w.Framework.Contains("ios", StringComparison.OrdinalIgnoreCase), c => c.SetProperty("ArchiveOnBuild", true))
                        .When(w => w.Framework.Contains("ios", StringComparison.OrdinalIgnoreCase), c => c.SetProperty("RuntimeIdentifier", "ios-arm64"))
                        .When(w => w.Framework.Contains("android", StringComparison.OrdinalIgnoreCase), c => c.SetProperty("AndroidPackageFormats", _androidExt))
                        .When(w => w.Framework.Contains("android", StringComparison.OrdinalIgnoreCase), c => c.SetProperty("RuntimeIdentifier", "android-arm64"))
                        .When(w => w.Framework.Contains("android", StringComparison.OrdinalIgnoreCase) && PackageSigning, (c) => c
                            .SetProperty("AndroidKeyStore", true)
                            .SetProperty("AndroidSigningKeyStore", SourceDirectory / ".certs" / v.project.Name + ".keystore")
                            .SetProperty("AndroidSigningKeyAlias", AndroidSigningKeyAlias)
                            .SetProperty("AndroidSigningKeyPass", AndroidSigningKeyPass)
                            .SetProperty("AndroidSigningStorePass", AndroidSigningStorePass)
                        )
                    )
                );
            }
            else if (hasDesktop)
            {
                DotNetPublish(s => s
                    .SetWarningLevel(WarningLevel)
                    .SetVerbosity(getDotNetVerbosity())
                    .SetConfiguration(Configuration)
                    .EnableSelfContained()
                    .CombineWith(publishProjects, configurator: (x, v) => x
                        .When(_ => _useMaui, y => y.SetPlatform(Platform))
                        .SetProject(v.project)
                        .SetFramework(v.framework)
                        .SetOutput($"{PublishDirectory}/{v.project.Name}")));
            }
            else if (hasPackage)
            {
                foreach (var item in publishProjects)
                {
                    var projectInfo = Solution.AllProjects.FirstOrDefault(p => p.Name == item.project.Name);
                    if (projectInfo != null)
                    {
                        DotNetPack(s => s
                            .SetWarningLevel(WarningLevel)
                            .SetVerbosity(getDotNetVerbosity())
                            .SetProject(projectInfo)
                            .SetConfiguration(Configuration)
                            .AddProperty("Icon", ".editoricon.png")
                            //.SetVersion($"{_version:3}{(!string.IsNullOrWhiteSpace(_versionTag) ? $"-{_versionTag}" : "")}")
                            .SetTitle($"{Product} {projectInfo.Name}")
                            .SetAuthors(Author)
                            .SetDescription($"{Product} {projectInfo.Name}")
                            .SetCopyright($"Copyright \u00a9 {Author}")
                            //.SetVersionSuffix(_version.ToString(3))
                            .SetRepositoryUrl(_repoUrl)
                            .SetRepositoryType("git")
                            .SetOutputDirectory($"{PublishDirectory}/{item.project.Name}"));
                    }
                    else
                    {
                        throw new NotSupportedException("Project not supported");
                    }
                }
            }

            Log.Information("Output: {PublishDirectory}", PublishDirectory);
        });

    Target Pack => d => d
        .DependsOn(Publish)
        .Executes(() =>
        {
            var items = loadPublishProjects();
            var target = from item in items
                from framework in item.project.GetTargetFrameworks()?.Where(m => !string.IsNullOrEmpty(m))
                select new { framework, item.project };

            if (_useMaui)
            {
                target = from item in target
                    where (item.framework.Contains("ios", StringComparison.OrdinalIgnoreCase) && Platform.Contains("iPhone", StringComparison.InvariantCultureIgnoreCase)) ||
                          item.framework.Contains("android", StringComparison.OrdinalIgnoreCase) && Platform.Contains("Android", StringComparison.InvariantCultureIgnoreCase)
                    select new { item.framework, item.project };

                if (!PackageSigning)
                {
                    ArtifactsDirectory.GlobFiles($"**/*-android/*Signed.{_androidExt}").DeleteFiles();
                }
            }

#if USING_EFCORE
            if (Project.StartsWith("Web") || Project.StartsWith("Service") || Project.StartsWith("Package"))
            {
                var startupPath = Startup?.Directory ?? string.Empty;

                var config = new ConfigurationBuilder()
                    .AddJsonFile(Path.Combine(startupPath, "appsettings.json"), false, true)
                    .AddJsonFile(Path.Combine(startupPath, $"appsettings.{Environment}.json"), true, true)
                    .Build();

                var connectionStrings = new Dictionary<string, string>();
                config.Bind(key: "ConnectionStrings", connectionStrings);

                var contexts = (from item in connectionStrings
                        let split = item.Key.Split(".")
                        where split.Length > 1
                        let context = split.First()
                        let provider = split.Last()
                        select new { Context = context, Name = context.Replace(oldValue: "Context", newValue: ""), Provider = provider, item.Value })
                    .ToArray();

                var scriptDir = Path.Combine(ArtifactsDirectory, "scripts");
                if (!Directory.Exists(scriptDir))
                {
                    Directory.CreateDirectory(scriptDir);
                }

                foreach (var item in contexts.Where(m => m.Provider != "Sqlite"))
                {
                    if (Startup == null || Persistence == null) continue;
                    var scripfile = Path.Combine(ArtifactsDirectory, "scripts", $"{item.Name}_{item.Provider}_{DateTime.Now:yyyyMMdd}.sql");
                    if (File.Exists(scripfile))
                    {
                        File.Delete(scripfile);
                    }

                    DotNetEf(_ => new MigrationsSettings(Migrations.Script)
                        .EnableIdempotent()
                        .SetProjectFile(Persistence.Path)
                        .SetStartupProjectFile(Startup.Path)
                        .SetContext(item.Context)
                        .SetOutput(scripfile)
                    );
                }

                var bundlesDir = Path.Combine(ArtifactsDirectory, "bundles");
                if (!Directory.Exists(bundlesDir))
                {
                    Directory.CreateDirectory(bundlesDir);
                }

                foreach (var item in contexts.Where(m => m.Provider == "Sqlite"))
                {
                    if (Startup == null || Persistence == null) continue;
                    var databasefile = Path.Combine(bundlesDir, $"{item.Name}_{item.Provider}_{DateTime.Now:yyyyMMdd}.db");
                    DotNetEf(_ => new DatabaseSettings(Database.Update)
                        .SetConnection($"Data Source={databasefile}")
                        .SetProjectFile(Persistence.Path)
                        .SetStartupProjectFile(Startup.Path)
                        .SetContext(item.Context)
                    );
                }
            }
#endif

            var hasWeb = (_projects.ContainsKey("Web") && _projects["Web"].Any());
            var hasService = (_projects.ContainsKey("Service") && _projects["Service"].Any());
            var hasMobile = (_projects.ContainsKey("Mobile") && _projects["Mobile"].Any());
            var hasDesktop = (_projects.ContainsKey("Desktop") && _projects["Desktop"].Any());
            var hasPackage = (_projects.ContainsKey("Package") && _projects["Package"].Any());

            foreach (var item in target)
            {
                if (hasDesktop)
                {
#if USING_7ZIP
                    if (OperatingSystem.IsWindows())
                    {
                        AbsolutePathExtensions.DeleteFile($"{ArtifactsDirectory}/{item.project.Name}.exe");
                        SevenZip.SevenZipBase.SetLibraryPath(Solution.Directory / ".build" / "7za.bin");
                        var compressor = new SevenZip.SevenZipCompressor();
                        compressor.CompressDirectory(Path.Combine(PublishDirectory, item.project.Name), Path.Combine(ArtifactsDirectory, $"{item.project.Name}.7z"));

                        var configs = new[] { ";!@Install@!UTF-8!", $"Title=\"{Product} {item.project.Name}\"", $"ExecuteFile=\"{item.project.Name}.exe\"", ";!@InstallEnd@!" };
                        var array1 = File.ReadAllBytes(Solution.Directory / ".build" / $"{Product}.sfx");
                        var array2 = Encoding.UTF8.GetBytes(string.Join(System.Environment.NewLine, configs));
                        var array3 = File.ReadAllBytes($"{ArtifactsDirectory / item.project.Name}.7z");
                        var data = array1.Concat(array2).Concat(array3).ToArray();
                        File.WriteAllBytes($"{ArtifactsDirectory}/{item.project.Name}.exe", data);
                        AbsolutePathExtensions.DeleteFile($"{ArtifactsDirectory}/{item.project.Name}.7z");
                    }
                    else
                    {
                      throw new InvalidOperationException("Unable to build in a non-windows machine");
                    }
#endif
                }
                else if (hasMobile)
                {
                    var change = AbsolutePath.Create(item.project.Directory / "Content").GlobFiles("CHANGES").FirstOrDefault();
                    var files = AbsolutePath.Create(item.project.Directory / "Content").GlobFiles("CHANGES.*.txt").ToArray();
                    files = files.Append(change).ToArray();
                    var notesPath = PublishDirectory / item.project.Name / item.framework / "Notes";
                    notesPath.CreateOrCleanDirectory();
                    foreach (var file in files)
                    {
                        File.Copy(file.ToString(), notesPath / file.Name);
                    }

                    AbsolutePath.Create(ArtifactsDirectory / "Android").CreateOrCleanDirectory();
                    AbsolutePath.Create(ArtifactsDirectory / "iOS").CreateOrCleanDirectory();

                    PublishDirectory.GlobFiles($"**/*-android/*{(PackageSigning ? "-Signed" : string.Empty)}.{_androidExt}").ForEach(x => x.MoveToDirectory(ArtifactsDirectory / "Android"));
                    PublishDirectory.GlobFiles($"**/*-ios/*.{_iOSExt}").ForEach(x => x.MoveToDirectory(ArtifactsDirectory / "iOS"));

                    // R8 Mapping
                    PublishDirectory.GlobFiles($"**/*-android/mapping.txt").ForEach(x => x.MoveToDirectory(ArtifactsDirectory / "Android"));

                    // Changes
                    PublishDirectory.GlobFiles($"**/*-android/Notes/CHANGES").ForEach(x =>
                    {
                        (ArtifactsDirectory / "Android" / "Notes").CreateDirectory();
                        x.MoveToDirectory(ArtifactsDirectory / "Android" / "Notes");
                    });
                    PublishDirectory.GlobFiles($"**/*-ios/Notes/*.*.txt").ForEach(x =>
                    {
                        (ArtifactsDirectory / "iOS" / "Notes").CreateDirectory();
                        x.MoveToDirectory(ArtifactsDirectory / "iOS" / "Notes");
                    });

                    //Rename Packages
                    (ArtifactsDirectory / "Android").GlobFiles($"*.{_androidExt}").FirstOrDefault()?.RenameWithoutExtension($"{PackageId}", ExistsPolicy.FileOverwrite);
                    (ArtifactsDirectory / "iOS" / "").GlobFiles($"*.{_iOSExt}").FirstOrDefault()?.RenameWithoutExtension($"{PackageId}", ExistsPolicy.FileOverwrite);
                }
                else if (hasWeb || hasService)
                {
                    AbsolutePathExtensions.DeleteFile($"{ArtifactsDirectory}/{item.project.Name}.zip");
                    ZipFile.CreateFromDirectory($"{PublishDirectory}/{item.project.Name}", $"{ArtifactsDirectory}/{item.project.Name}.zip");
                }
                else if (hasPackage)
                {
                    var packageId = item.project.GetProperty("PackageId");
                    AbsolutePathExtensions.DeleteFile($"{ArtifactsDirectory}/{packageId}.*.nupkg");
                    var nugets = AbsolutePathExtensions.GetFiles($"{PublishDirectory}/{item.project.Name}/", $"{packageId}.*.nupkg");
                    foreach (var nuget in nugets)
                    {
                        var fileName = $"{ArtifactsDirectory}/{nuget.Name}";
                        if (File.Exists(fileName))
                        {
                            File.Delete(fileName);
                        }

                        nuget.Move(fileName);
                    }
                }
            }

            Log.Information("Output: {ArtifactsDirectory}", ArtifactsDirectory);
            return Task.CompletedTask;
        });

    Target Analyze => d => d
        .After(Restore)
        .Executes(() =>
        {
#if USING_SONARQUBE
            var lintFile = Path.Combine(RootDirectory, ".sonarlint", Solution.Name + ".json");
            SonarLint lint;
            if (File.Exists(lintFile))
            {
                lint = JsonSerializer.Deserialize<SonarLint>(File.ReadAllText(lintFile));
            }
            else
            {
                lint = new SonarLint(
                    System.Environment.GetEnvironmentVariable("SONAR_HOST_URL"),
                    System.Environment.GetEnvironmentVariable("SONAR_TOKEN"),
                    System.Environment.GetEnvironmentVariable("SONAR_PROJECT_KEY")
                );
            }

            if (lint != null)
            {
                SonarScannerTasks.SonarScannerBegin(s => s
                    .SetProjectKey(lint.projectKey)
                    .SetToken(lint.sonarQubeToken)
                    .SetAdditionalParameter("sonar.host.url", lint.sonarQubeUri)
                    .SetAdditionalParameter("sonar.exclusions", "**/.sonarlint/*.*")
                    .SetAdditionalParameter("sonar.cs.vscoveragexml.reportsPaths", TestsDirectory / "coverage" / "coverage.xml")
                );
                DotNetBuild(s => s.SetProjectFile(Solution));
                // DotCoverTasks.DotCoverCover(c => c
                //     .SetReportType(DotCoverReportType.Xml)
                //     .SetOutputFile(TestsDirectory / "coverage" / "coverage.xml")
                // );
                SonarScannerTasks.SonarScannerEnd(s => s
                    .SetToken(lint.sonarQubeToken)
                );
            }
            else
            {
                Log.Warning("SonarQube Lint file not found: {LintFile}", lintFile);
            }
#endif
        });

    record PublishProjectRecord(Project project, bool useMaui);

    private PublishProjectRecord[] loadPublishProjects()
    {
        foreach (var item in Solution.Projects)
        {
            var project = item.GetMSBuildProject();
            var options = project.Imports.FirstOrDefault(m => m.ImportedProject.EscapedFullPath.EndsWith("Options.props"));
            if (options.ImportedProject != null)
            {
                // Variables
                var pg1 = options.ImportedProject.PropertyGroups.FirstOrDefault(m => m.Label == "Environment");
                if (pg1 != null)
                {
                    pg1.SetProperty("Environment", Environment.ToString());
                    pg1.SetProperty("Platform", Platform);
                }
                else
                {
                    options.ImportedProject.AddProperty("Environment", Environment.ToString());
                    options.ImportedProject.AddProperty("Platform", Platform);
                }

                if (options.ImportedProject.HasUnsavedChanges)
                {
                    options.ImportedProject.Save();
                }

                options.ImportedProject.Reload();
            }
            else
            {
                project.SetProperty("Environment", Environment.ToString());
                project.SetProperty("Platform", Platform);
                project.Save();
            }

            project.ReevaluateIfNecessary();
        }

        var internalProjects = (from project in Solution.Projects
            let info = project.GetMSBuildProject()
            let useMaui = info.GetPropertyValue("UseMaui")
            select new PublishProjectRecord(
                project, !string.IsNullOrWhiteSpace(useMaui) && bool.Parse(useMaui)
            )).ToArray();

        return internalProjects;
    }

#if USING_SONARQUBE
    private record SonarLint(string sonarQubeUri, string sonarQubeToken, string projectKey);
#endif

    private DotNetVerbosity getDotNetVerbosity()
    {
        return Verbosity switch
        {
            Verbosity.Minimal => DotNetVerbosity.minimal,
            Verbosity.Verbose => DotNetVerbosity.detailed,
            Verbosity.Quiet => DotNetVerbosity.quiet,
            Verbosity.Normal => DotNetVerbosity.normal,
            _ => DotNetVerbosity.diagnostic
        };
    }

    private string getReleaseNotes()
    {
        var gitOutput = GitTasks.Git("log -1 --pretty=%B");

        var releaseNotes = new List<string> { $"Environment: {Environment}", System.Environment.NewLine, "Release Notes:", System.Environment.NewLine };
        releaseNotes.AddRange(gitOutput.Where(x => !string.IsNullOrWhiteSpace(x.Text)).Select(x => x.Text).ToList());

        return string.Join(System.Environment.NewLine, releaseNotes);
    }

    [GeneratedRegex(@"\[assembly: AssemblyVersion\(.*\)\]", RegexOptions.Compiled)]
    private static partial Regex AssemblyVersionRegex();

    [GeneratedRegex(@"\[assembly: AssemblyFileVersion\(.*\)\]", RegexOptions.Compiled)]
    private static partial Regex AssemblyFileVersionRegex();

    [GeneratedRegex(@"\[assembly: AssemblyInformationalVersion\(.*\)\]", RegexOptions.Compiled)]
    private static partial Regex AssemblyInformationalVersionRegex();
}
#pragma warning restore CA1050 // Declare types in namespaces
#pragma warning restore IDE1006 // Naming Styles
