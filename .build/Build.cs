using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
#if USING_7ZIP
using System.Text;
#endif
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Nuke.Common;
using Nuke.Common.Git;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.Coverlet;
using Nuke.Common.Tools.DotNet;
#if USING_EFCORE
using Microsoft.Extensions.Configuration;
using Nuke.Common.Tools.DotNet.EF;
using Nuke.Common.Tools.DotNet.EF.Commands;
using static Nuke.Common.Tools.DotNet.EF.Tasks;
#endif
using Nuke.Common.Tools.Git;
using Nuke.Common.Utilities.Collections;
using Serilog;
using static Nuke.Common.Tools.DotNet.DotNetTasks;

// ReSharper disable InconsistentNaming
// ReSharper disable UnusedMember.Local
#pragma warning disable IDE1006// Naming Styles
#pragma warning disable CA1050// Declare types in namespaces
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

  [Parameter("MAUI Workload Version")]
  public readonly string MauiWorkloadVersion = "8.0.404";

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

  static AbsolutePath TestsDirectory => RootDirectory / "tests";

  static AbsolutePath PublishDirectory => RootDirectory / "publish";

  static AbsolutePath ArtifactsDirectory => RootDirectory / "output";

  static AbsolutePath TestResultsDirectory => RootDirectory / "tests" / "results";

  static AbsolutePath ScriptsDirectory => RootDirectory / "scripts";

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

  Version _version = new Version("1.0.0.0");
  string _hash = string.Empty;
  string _versionTag = string.Empty;

  bool _useMaui;
  string _repoUrl = string.Empty;
  string _androidExt = string.Empty;
  string _iOSExt = string.Empty;

  #endregion

  #region Projects

  [Required]
  [Parameter("Project Name to Build and Deploy")]
  public readonly string Project;

  [Parameter]
  public readonly string[] WebProjects = [];

  [Parameter]
  public readonly string[] ServiceProjects = [];

  [Parameter]
  public readonly string[] DesktopProjects = [];

  [Parameter]
  public readonly string[] MobileProjects = [];

  [Parameter]
  public readonly string[] PackageProjects = [];

  [Parameter]
  public readonly string[] TestsProjects = [];

  static IEnumerable<Project> Projects = Array.Empty<Project>();
  static IEnumerable<Project> Tests = Array.Empty<Project>();

  #endregion

  Target Prepare => d => d
    .Before(Compile)
    .Executes(() =>
    {
      #region Projects

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
          if (build.ImportedProject.HasUnsavedChanges) build.ImportedProject.Save();
          build.ImportedProject.Reload();
        }
        project.ReevaluateIfNecessary();
      }
      Projects = Project switch
      {
        "Web" => Solution.AllProjects.Where(m => WebProjects.Contains(m.Name)).ToArray(),
        "Service" => Solution.AllProjects.Where(m => ServiceProjects.Contains(m.Name)).ToArray(),
        "Desktop" => Solution.AllProjects.Where(m => DesktopProjects.Contains(m.Name)).ToArray(),
        "Mobile" => Solution.AllProjects.Where(m => MobileProjects.Contains(m.Name)).ToArray(),
        "Package" => Solution.AllProjects.Where(m => PackageProjects.Contains(m.Name)).ToArray(),
        _ => Solution.AllProjects.Where(m =>
          Array.Empty<string>().Concat(WebProjects).Concat(ServiceProjects).Concat(DesktopProjects).Concat(MobileProjects).Contains(m.Name)).ToArray()
      };
      Tests = Solution.AllProjects.Where(m => TestsProjects.Contains(m.Name)).ToArray();

      #endregion

      #region Properties

      using (var gitUrl = new Process())
      {
        gitUrl.StartInfo = new ProcessStartInfo(fileName: "git", arguments: "config --get remote.origin.url") { WorkingDirectory = SourceDirectory, RedirectStandardOutput = true, UseShellExecute = false };
        gitUrl.Start();
        _repoUrl = gitUrl.StandardOutput.ReadLine()?.Trim().Split(separator: " ", StringSplitOptions.RemoveEmptyEntries).LastOrDefault();
        gitUrl.WaitForExit();
      }

      _useMaui = Projects.Select(m =>
      {
        var evaluatedValue = m.GetMSBuildProject()?.GetProperty("UseMaui")?.EvaluatedValue;
        return evaluatedValue != null && bool.Parse(evaluatedValue);
      }).Any(m => m);

      #endregion

      #region Specials

      if (_useMaui)
      {
        _androidExt = Environment == Environment.Production ? "aab" : "apk";
        _iOSExt = Platform != null && Platform.Contains("iPhoneSimulator", StringComparison.InvariantCultureIgnoreCase) ? "app" : "ipa";
      }

      #endregion

    });

  Target Clean => d => d
    .Before(Restore)
    .Executes(() =>
    {
      Log.Information("Cleaning Output Directories");
      SourceDirectory.GlobDirectories("**/bin", "**/obj").ForEach((path) => path.DeleteDirectory());
      TestsDirectory.GlobDirectories("**/bin", "**/obj").ForEach((path) => path.DeleteDirectory());
      Log.Information("Cleaning Test Results Directory");
      AbsolutePath.Create(TestResultsDirectory).CreateOrCleanDirectory();
      Log.Information("Cleaning Scripts Directory");
      AbsolutePath.Create(ScriptsDirectory).CreateOrCleanDirectory();
      Log.Information("Cleaning Publish Directory");
      AbsolutePath.Create(PublishDirectory).CreateOrCleanDirectory();
      Log.Information("Cleaning Artifacts Directory");
      AbsolutePath.Create(ArtifactsDirectory).CreateOrCleanDirectory();

      AbsolutePath.Create(TestResultsDirectory).CreateOrCleanDirectory();
      AbsolutePath.Create(TestsDirectory / "coverage").CreateOrCleanDirectory();

      if (DryRun)
      {
        Log.Information("Cleaning NuGet Cache Directory");
        var packages = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.UserProfile), ".nuget", "packages");
        if (Path.Exists(packages)) Directory.Delete(packages, true);
      }
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
          var workloads = DotNet("workload list");
          var isMauiInstalled = workloads.Any(x => x.Text.Contains("maui", StringComparison.OrdinalIgnoreCase));
          if (!isMauiInstalled)
          {
            Log.Information("Installing MAUI workload...");
            var command = $"workload install maui-mobile --version {MauiWorkloadVersion}";
            if (Platform.Contains("Android"))
            {
              command = $"workload install maui-android --version {MauiWorkloadVersion}";
            }
            else if (Platform.Contains("iPhone"))
            {
              command = $"workload install maui-ios --version {MauiWorkloadVersion}";
            }
            DotNet(command);
          }
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
        .SetConfigFile(RootDirectory / ".nuget" / "NuGet.config")
        .CombineWith(Projects, (x, v) => x
          .SetProjectFile(v)));

    });

  Target Compile => d => d
    .DependsOn(Clean)
    .DependsOn(Restore)
    .DependsOn(Prepare)
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
            .When(v.framework.Contains("ios", StringComparison.OrdinalIgnoreCase), c => c.SetProperty("ArchiveOnBuild", true))
            .When(v.framework.Contains("ios", StringComparison.OrdinalIgnoreCase), c => c.SetProperty("RuntimeIdentifier", "ios-arm64"))
            .When(v.framework.Contains("android", StringComparison.OrdinalIgnoreCase), c => c.SetProperty("AndroidPackageFormats", _androidExt))
            .When(v.framework.Contains("android", StringComparison.OrdinalIgnoreCase), c => c.SetProperty("RuntimeIdentifier", "android-arm64"))
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

  Target Test => d => d
    .DependsOn(Compile)
    .Executes(() =>
    {
      if (!Tests.Any()) throw new NullReferenceException("No tests found");
      DotNetTest(s => s
        .EnableNoRestore()
        .EnableNoBuild()
        .SetConfiguration(Configuration)
        .When(true, configurator: x => x
          .SetLoggers("trx")
          .SetResultsDirectory(TestResultsDirectory))
        .CombineWith(Tests, configurator: (x, v) => x
          .SetProjectFile(v.Path)));

      DotNetToolRestore(s => s.SetProcessWorkingDirectory(SourceDirectory));

      var tests = Solution.AllProjects.Where(s => TestsProjects.Contains(s.Name)).ToArray();
      var testsProjects = tests.Where(m => !m.Name.Contains("UI", StringComparison.OrdinalIgnoreCase)).ToArray();

      DotNetTest(s => s
        .SetVerbosity(getDotNetVerbosity())
        .SetConfiguration(Configuration)
        .SetCollectCoverage(true)
        .SetCoverletOutputFormat("cobertura")
        .SetCoverletOutput(TestsDirectory / "coverage")
        .SetLoggers("trx")
        .SetResultsDirectory(TestResultsDirectory)
        .SetDataCollector("XPlat Code Coverage")
        .CombineWith(testsProjects, configurator: (x, v) => x
          .SetProjectFile(v.Path)));

      DotNet($"reportgenerator -reports:\"{TestResultsDirectory}/**/coverage.cobertura.xml\" -targetdir:{TestsDirectory / "coverage"} -reporttypes:\"cobertura\"");

      TestResultsDirectory.GlobFiles("*.trx")
        .ForEach(m =>
        {
          DotNet($"trx2junit {m}");
        });
    });

  Target UITest => d => d
    .DependsOn(Prepare)
    .Executes(() =>
    {
      DotNetToolRestore(s => s.SetProcessWorkingDirectory(SourceDirectory));

      var tests = Solution.AllProjects.Where(s => TestsProjects.Contains(s.Name)).ToArray();
      var testsProjects = tests.Where(m => m.Name.Contains("UI", StringComparison.OrdinalIgnoreCase)).ToArray();

      DotNetTest(s => s
        .SetVerbosity(getDotNetVerbosity())
        .SetConfiguration(Configuration)
        .CombineWith(testsProjects, configurator: (x, v) => x
          .SetProjectFile(v.Path)));

    });

  Target Coverage => d => d
    .Executes(() =>
    {
      DotNetToolRestore(s => s.SetProcessWorkingDirectory(SourceDirectory));
      AbsolutePath.Create(TestsDirectory / "coverage").CreateOrCleanDirectory();
      DotNet($"reportgenerator -reports:\"{TestResultsDirectory}/**/coverage.cobertura.xml\" -targetdir:{TestsDirectory / "coverage"} -reporttypes:\"cobertura;html;teamcitysummary\"");
    });

  Target Versioning => d => d
    .DependsOn(Prepare)
    .After(Prepare)
    .Executes(() =>
    {
      var versionRegex = VersionRegex();
      Tuple<Version, string> tag;

      try
      {
        tag = GitTasks.Git("describe --tags --always --abbrev=0").Select(m => m.Text)
          .Select(m => versionRegex.Match(m))
          .Where(m => m.Success)
          .Select(m => new Tuple<Version, string>(Version.Parse(m.Groups[1].Value), m.Groups[2].Value))
          .FirstOrDefault();
      }
      catch (Exception)
      {
        Log.Warning("Unable to get repository tags using CLI.");
        using var gitTag = new Process();
        gitTag.StartInfo = new ProcessStartInfo(fileName: "git", arguments: "describe --tags --always --abbrev=0") { WorkingDirectory = RootDirectory, RedirectStandardOutput = true, UseShellExecute = false };
        gitTag.Start();
        tag = new[] { gitTag.StandardOutput.ReadToEnd().Trim() }
          .Select(m => versionRegex.Match(m))
          .Where(m => m.Success)
          .Select(m => new Tuple<Version, string>(Version.Parse(m.Groups[1].Value), m.Groups[2].Value))
          .FirstOrDefault();
      }

      _version = tag?.Item1;
      _versionTag = tag?.Item2;
      _hash = Repository.Commit;

      Log.Information("Version: {Version}", _version);
      Log.Information("Tag: {VersionTag}", _versionTag);
      Log.Information("Hash: {Hash}", _hash);

      if (_version == null)
      {
        Log.Warning("Version was not detected");
        _version = new Version("1.0.0");
      }

      if (Environment == Environment.Development)
      {
        return;
      }

      var assemblyInfoFiles = SourceDirectory.GetFiles("AssemblyInfo*.cs", int.MaxValue);
      foreach (var assemblyInfoVersionFile in assemblyInfoFiles)
      {
        Log.Information(messageTemplate: "Patching: {File}", assemblyInfoVersionFile);

        var content = File.ReadAllText(assemblyInfoVersionFile);
        content = AssemblyVersionRegex().Replace(content, $"[assembly: AssemblyVersion(\"{_version}\")]");
        content = AssemblyFileVersionRegex().Replace(content, $"[assembly: AssemblyFileVersion(\"{_version}\")]");
        content = AssemblyInformationalVersionRegex().Replace(content, $"[assembly: AssemblyInformationalVersion(\"{_version:3}{(!string.IsNullOrWhiteSpace(_versionTag) ? $"-{_versionTag}" : "")}+{_hash}\")]");

        File.WriteAllText(assemblyInfoVersionFile, content);
      }

      if (_useMaui)
      {
        var applicationVersion = string.Join("-", new[] { _version?.ToString(3), _versionTag }.Where(r => !string.IsNullOrWhiteSpace(r)));
        var applicationDisplayVersion = ((int)(DateTime.UtcNow.Ticks / 100000000)).ToString();

        if (Verbosity == Verbosity.Minimal)
        {
          Console.WriteLine($"// ApplicationVersion: {applicationVersion}");
          Console.WriteLine($"// ApplicationDisplayVersion: {applicationDisplayVersion}");
        }
        else
        {
          Log.Information($"ApplicationVersion: {applicationVersion}");
          Log.Information($"ApplicationDisplayVersion: {applicationDisplayVersion}");
        }

        var items = loadPublishProjects();
        foreach (var item in items.Where(m => m.useMaui))
        {
          var project = item.project.GetMSBuildProject();

          // Version
          project.SetProperty("ApplicationDisplayVersion", string.Join("-", new[] { _version?.ToString(3), _versionTag }.Where(r => !string.IsNullOrWhiteSpace(r))));
          project.SetProperty("ApplicationVersion", ((int)(DateTime.UtcNow.Ticks / 100000000)).ToString());

          project.Save(item.project.Path);
          project.ReevaluateIfNecessary();
        }
      }

    });

  Target Publish => d => d
    .DependsOn(Clean)
    //.DependsOn(Test)
    .DependsOn(Restore)
    .DependsOn(Compile)
    .DependsOn(Prepare)
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

      if (Project.StartsWith("Web") || Project.StartsWith("Service"))
      {
#if USING_EFCORE
        var startupPath = Startup?.Directory ?? string.Empty;

        var config = new ConfigurationBuilder()
          .AddJsonFile(Path.Combine(startupPath, "appsettings.json"), false, true)
          .AddJsonFile(Path.Combine(startupPath, $"appsettings.{Environment}.json"), true, true)
          .Build();

        var connectionStrings = new Dictionary<string, string>();
        config.Bind(key: "ConnectionStrings", connectionStrings);

        var contexts = from item in connectionStrings
          let split = item.Key.Split(".")
          where split.Length > 1
          let context = split.First()
          let provider = split.Last()
          where provider != "Sqlite"
          select new { Context = context, Name = context.Replace(oldValue: "Context", newValue: ""), Provider = provider, item.Value };

        foreach (var item in contexts)
        {
          var fileName = Path.Combine(ScriptsDirectory, $"{item.Name}_{item.Provider}_{DateTime.Now:yyyyMMdd}.sql");
          if (File.Exists(fileName))
          {
            File.Delete(fileName);
          }
          DotNetEf(_ => new MigrationsSettings(Migrations.Script)
            .EnableIdempotent()
            .SetProjectFile(Persistence.Path)
            .SetStartupProjectFile(Startup.Path)
            .SetContext(item.Context)
            .SetOutput(fileName)
          );
        }
#endif

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
      else if (Project.StartsWith("Mobile"))
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
            .When(v.framework.Contains("ios", StringComparison.OrdinalIgnoreCase), c => c.SetProperty("ArchiveOnBuild", true))
            .When(v.framework.Contains("ios", StringComparison.OrdinalIgnoreCase), c => c.SetProperty("RuntimeIdentifier", "ios-arm64"))
            .When(v.framework.Contains("android", StringComparison.OrdinalIgnoreCase), c => c.SetProperty("AndroidPackageFormats", _androidExt))
            .When(v.framework.Contains("android", StringComparison.OrdinalIgnoreCase), c => c.SetProperty("RuntimeIdentifier", "android-arm64"))
            .When(v.framework.Contains("android", StringComparison.OrdinalIgnoreCase) && PackageSigning, (c) => c
              .SetProperty("AndroidKeyStore", true)
              .SetProperty("AndroidSigningKeyStore", SourceDirectory / ".certs" / v.project.Name + ".keystore")
              .SetProperty("AndroidSigningKeyAlias", AndroidSigningKeyAlias)
              .SetProperty("AndroidSigningKeyPass", AndroidSigningKeyPass)
              .SetProperty("AndroidSigningStorePass", AndroidSigningStorePass)
            )
          )
        );
      }
      else if (Project.StartsWith("Desktop"))
      {
        DotNetPublish(s => s
          .SetWarningLevel(WarningLevel)
          .SetVerbosity(getDotNetVerbosity())
          .SetConfiguration(Configuration)
          .EnableSelfContained()
          .CombineWith(publishProjects, configurator: (x, v) => x
            .When(_useMaui, y => y.SetPlatform(Platform))
            .SetProject(v.project)
            .SetFramework(v.framework)
            .SetOutput($"{PublishDirectory}/{v.project.Name}")));
      }
      else if (Project.StartsWith("Package"))
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
              .SetVersion($"{_version:3}{(!string.IsNullOrWhiteSpace(_versionTag) ? $"-{_versionTag}" : "")}")
              .SetTitle($"{Product} {projectInfo.Name}")
              .SetAuthors(Author)
              .SetDescription($"{Product} {projectInfo.Name}")
              .SetCopyright($"Copyright \u00a9 {Author}")
              .SetVersionSuffix(_version.ToString(3))
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

      Log.Information($"Output: {PublishDirectory}");
    });

  Target Pack => d => d
    .DependsOn(Publish)
    .DependsOn(Prepare)
    .Executes(() =>
    {
      if (ScriptsDirectory.Exists())
      {
        ScriptsDirectory.CopyToDirectory($"{ArtifactsDirectory}", ExistsPolicy.MergeAndOverwrite);
      }

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

      foreach (var item in target)
      {
        if (Project.StartsWith("Desktop"))
        {
          if (OperatingSystem.IsWindows())
          {
#if USING_7ZIP
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
#endif
          }
          // else
          // {
          //   throw new InvalidOperationException("Unable to build in a non-windows machine");
          // }
        }
        else if (Project.StartsWith("Mobile"))
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
        else if (Project.StartsWith("Web") || Project.StartsWith("Service"))
        {
          AbsolutePathExtensions.DeleteFile($"{ArtifactsDirectory}/{item.project.Name}.zip");
          ZipFile.CreateFromDirectory($"{PublishDirectory}/{item.project.Name}", $"{ArtifactsDirectory}/{item.project.Name}.zip");
        }
        else if (Project.StartsWith("Package"))
        {
          var packageId = item.project.GetProperty("PackageId");
          AbsolutePathExtensions.DeleteFile($"{ArtifactsDirectory}/{packageId}.*.nupkg");
          var nugets = AbsolutePathExtensions.GetFiles($"{PublishDirectory}/{item.project.Name}/", $"{packageId}.*.nupkg");
          foreach (var nuget in nugets)
          {
            var fileName = $"{ArtifactsDirectory}/{nuget.Name}";
            if (File.Exists(fileName)) File.Delete(fileName);
            nuget.Move(fileName);
          }
        }
      }
      Log.Information($"Output: {ArtifactsDirectory}");
      return Task.CompletedTask;
    });

  record PublishProjectRecord(Project project, bool useMaui);

  PublishProjectRecord[] loadPublishProjects()
  {
    foreach (var item in Projects)
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

    var internalProjects = (from project in Projects
      let info = project.GetMSBuildProject()
      let useMaui = info.GetPropertyValue("UseMaui")
      select new PublishProjectRecord(
        project, !string.IsNullOrWhiteSpace(useMaui) && bool.Parse(useMaui)
      )).ToArray();

    return internalProjects;
  }

  static DotNetVerbosity getDotNetVerbosity()
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

  [GeneratedRegex(@"v?\=?((?:[0-9]{1,}\.{0,}){1,})\-?(.*)", RegexOptions.Compiled)]
  private static partial Regex VersionRegex();

  [GeneratedRegex(@"\[assembly: AssemblyVersion\(.*\)\]", RegexOptions.Compiled)]
  private static partial Regex AssemblyVersionRegex();

  [GeneratedRegex(@"\[assembly: AssemblyFileVersion\(.*\)\]", RegexOptions.Compiled)]
  private static partial Regex AssemblyFileVersionRegex();

  [GeneratedRegex(@"\[assembly: AssemblyInformationalVersion\(.*\)\]", RegexOptions.Compiled)]
  private static partial Regex AssemblyInformationalVersionRegex();

  string GetReleaseNotes()
  {
    var gitOutput = GitTasks.Git("log -1 --pretty=%B");

    var releaseNotes = new List<string> { $"Environment: {Environment}", System.Environment.NewLine, "Release Notes:", System.Environment.NewLine };
    releaseNotes.AddRange(gitOutput.Where(x => !string.IsNullOrWhiteSpace(x.Text)).Select(x => x.Text).ToList());

    return string.Join(System.Environment.NewLine, releaseNotes);
  }

}
#pragma warning restore CA1050// Declare types in namespaces
#pragma warning restore IDE1006// Naming Styles
