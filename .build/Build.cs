using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Configuration;
using Nuke.Common;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Tools.DotNet.EF;
using Nuke.Common.Tools.DotNet.EF.Commands;
using Nuke.Common.Utilities.Collections;
using Serilog;
using static Nuke.Common.IO.FileSystemTasks;
using static Nuke.Common.Tools.DotNet.DotNetTasks;
using static Nuke.Common.Tools.DotNet.EF.Tasks;

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

    [Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")]
    readonly Configuration Configuration = IsLocalBuild ? Configuration.Debug : Configuration.Release;

    [Parameter("Platform to build")]
    readonly string Platform = "Web";

    [Parameter("Environment to build - Default is 'Development' (local) or 'Production' (server)")]
    public string Environment = IsLocalBuild ? "Development" : "Production";

    [Solution]
    readonly Solution Solution;

    static AbsolutePath SourceDirectory => RootDirectory / "src";

    static AbsolutePath TestsDirectory => RootDirectory / "tests";

    static AbsolutePath PublishDirectory => RootDirectory / "publish";

    static AbsolutePath ArtifactsDirectory => RootDirectory / "output";

    static AbsolutePath TestResultsDirectory => RootDirectory / "tests" / "results";

    static AbsolutePath ScriptsDirectory => RootDirectory / "scripts";

    static string Author => string.Empty;

    static string Product => string.Empty;

    static string ProjectUrl => string.Empty;

    Version _version = new("1.0.0.0");
    string _hash = string.Empty;

    bool _useMaui = false;
    string _platformOS = string.Empty;
    string _platformName = string.Empty;

    readonly string[] WebProjects = new[]
    {
        "Web"
    };

    readonly string[] DesktopProjects = Array.Empty<string>();

    readonly string[] MobileProjects = new[]
    {
        "App"
    };

    readonly string[] PackageProjects = Array.Empty<string>();

    readonly string[] TestsProjects = new[]
    {
        "Test.Business"
    };

    IEnumerable<Project> Projects = Array.Empty<Project>();
    IEnumerable<Project> Tests = Array.Empty<Project>();

    Target Prepare => d => d
        .Before(Compile)
        .Executes(() =>
        {
            #region Version

            var assemblyInfoVersionFile = Path.Combine(SourceDirectory, path2: ".files", path3: "AssemblyInfo.Version.cs");
            if (File.Exists(assemblyInfoVersionFile))
            {

                Log.Information(messageTemplate: "Patching: {File}", assemblyInfoVersionFile);

                using (var gitTag = new Process())
                {
                    gitTag.StartInfo = new ProcessStartInfo(fileName: "git", arguments: "tag --sort=-v:refname")
                    {
                        WorkingDirectory = SourceDirectory, RedirectStandardOutput = true, UseShellExecute = false
                    };
                    gitTag.Start();
                    var value = gitTag.StandardOutput.ReadToEnd().Trim();
                    value = new Regex(pattern: @"((?:[0-9]{1,}\.{0,}){1,})", RegexOptions.Compiled).Match(value).Captures.LastOrDefault()?.Value;
                    if (value != null)
                    {
                        _version = Version.Parse(value);
                    }

                    gitTag.WaitForExit();
                }

                using (var gitLog = new Process())
                {
                    gitLog.StartInfo = new ProcessStartInfo(fileName: "git", arguments: "rev-parse --verify HEAD")
                    {
                        WorkingDirectory = SourceDirectory, RedirectStandardOutput = true, UseShellExecute = false
                    };
                    gitLog.Start();
                    _hash = gitLog.StandardOutput.ReadLine()?.Trim().Split(separator: " ", StringSplitOptions.RemoveEmptyEntries).LastOrDefault();
                    gitLog.WaitForExit();
                }

                if (_version != null)
                {
                    var content = File.ReadAllText(assemblyInfoVersionFile);
                    var assemblyVersionRegEx = new Regex(pattern: @"\[assembly: AssemblyVersion\(.*\)\]", RegexOptions.Compiled);
                    var assemblyFileVersionRegEx = new Regex(pattern: @"\[assembly: AssemblyFileVersion\(.*\)\]", RegexOptions.Compiled);
                    var assemblyInformationalVersionRegEx = new Regex(pattern: @"\[assembly: AssemblyInformationalVersion\(.*\)\]", RegexOptions.Compiled);

                    content = assemblyVersionRegEx.Replace(content, $"[assembly: AssemblyVersion(\"{_version}\")]");
                    content = assemblyFileVersionRegEx.Replace(content, $"[assembly: AssemblyFileVersion(\"{_version}\")]");
                    content = assemblyInformationalVersionRegEx.Replace(content, $"[assembly: AssemblyInformationalVersion(\"{_version:3}+{_hash}\")]");

                    File.WriteAllText(assemblyInfoVersionFile, content);

                    Log.Information(messageTemplate: "Version: {Version}", _version);
                    Log.Information(messageTemplate: "Hash: {Hash}", _hash);

                }
                else
                {
                    Log.Warning("Version was not found");
                }
            }

            #endregion
            #region Projects

            var platformInfo = Platform.Contains("1:") ? Platform.Split(":") : new[]
            {
                Platform
            };
            _platformName = platformInfo.FirstOrDefault();
            _platformOS = platformInfo.LastOrDefault();

            Projects = _platformName switch
            {
                "Web" => Solution.AllProjects.Where(m => WebProjects.Contains(m.Name)).ToArray(),
                "Desktop" => Solution.AllProjects.Where(m => DesktopProjects.Contains(m.Name)).ToArray(),
                "Mobile" => Solution.AllProjects.Where(m => MobileProjects.Contains(m.Name)).ToArray(),
                "Package" => Solution.AllProjects.Where(m => PackageProjects.Contains(m.Name)).ToArray(),
                _ => Projects
            };
            if (Platform.StartsWith("Mobile")) Projects = Solution.AllProjects.Where(m => MobileProjects.Contains(m.Name)).ToArray();
            Tests = Solution.AllProjects.Where(m => TestsProjects.Contains(m.Name)).ToArray();

            #endregion
            #region Properties

            _useMaui = Projects.Select(m =>
            {
                var evaluatedValue = m.GetMSBuildProject()?.GetProperty("UseMaui")?.EvaluatedValue;
                return evaluatedValue != null && bool.Parse(evaluatedValue);
            }).Any(m => m);

            #endregion

        });

    Target Clean => d => d
        .Before(Restore)
        .Executes(() =>
        {
            SourceDirectory.GlobDirectories("**/bin", "**/obj").ForEach((path) => path.DeleteDirectory());
            TestsDirectory.GlobDirectories("**/bin", "**/obj").ForEach((path) => path.DeleteDirectory());
            AbsolutePath.Create(PublishDirectory).CreateOrCleanDirectory();
            AbsolutePath.Create(ArtifactsDirectory).CreateOrCleanDirectory();
            AbsolutePath.Create(TestResultsDirectory).CreateOrCleanDirectory();
            AbsolutePath.Create(ScriptsDirectory).CreateOrCleanDirectory();
        });

    Target Restore => d => d
        .After(Prepare)
        .Executes(() =>
        {
            DotNetToolRestore();

            if (_useMaui) DotNet($"workload restore");

            DotNetRestore(s => s
                .CombineWith(Projects, configurator: (x, v) => x
                    .SetProjectFile(v)));

        });

    Target Compile => d => d
        .DependsOn(Clean)
        .DependsOn(Restore)
        .DependsOn(Prepare)
        .Executes(() =>
        {
            if (!_useMaui)
            {
                DotNetBuild(s => s
                    .CombineWith(Projects, configurator: (x, v) => x
                        .SetProjectFile(v.Path)
                        .SetConfiguration(Configuration)
                        .EnableNoRestore()));
            }
        });

    Target Test => d => d
        .DependsOn(Compile)
        .Executes(() =>
        {
            var testCombinations =
                from project in TestsProjects.Select(m => Solution.AllProjects.FirstOrDefault(o=> o.Name == m))
                from framework in project.GetTargetFrameworks()
                select new { project, framework };

            DotNetTest(s => s
                .EnableNoRestore()
                .EnableNoBuild()
                .SetConfiguration(Configuration)
                .When(true, configurator: x => x
                    .SetLoggers("trx")
                    .SetResultsDirectory(TestResultsDirectory))
                .CombineWith(Tests, configurator: (x, v) => x
                    .SetProjectFile(v.Path)));
        });

    Target Publish => d => d
        .DependsOn(Test)
        .DependsOn(Compile)
        .DependsOn(Clean)
        .Executes(() =>
        {
            var persistence = Solution.AllProjects.FirstOrDefault(m=> m.Name == "Persistence")?.Path;
            var startup = Solution.AllProjects.FirstOrDefault(m=> m.Name == "Web")?.Path;
            var startupPath = Solution.AllProjects.FirstOrDefault(m=> m.Name == "Web")?.Directory ?? string.Empty;

            var publishProjects =
                (from project in Projects
                 from framework in project.GetTargetFrameworks()
                 select new
                 {
                     project, framework
                 }).Where(m => _platformOS == Platform || m.framework.Contains(_platformOS, StringComparison.InvariantCultureIgnoreCase)).ToArray();

            if (Platform.StartsWith("Web"))
            {
                var startupPath = Startup?.Directory ?? string.Empty;

                var config = new ConfigurationBuilder()
                    .AddJsonFile(Path.Combine(startupPath, path2: "appsettings.json"), false, true)
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
                               select new
                               {
                                   Context = context, Name = context.Replace(oldValue: "Context", newValue: ""), Provider = provider, item.Value
                               };

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

                DotNetEf(_ => new MigrationsSettings(Migrations.Script)
                    .EnableIdempotent()
                    .SetProjectFile(persistence)
                    .SetStartupProjectFile(startup)
                    .SetContext(item.Context)
                    .SetOutput(fileName)
                );
            }
            else if (Platform.StartsWith("Desktop") || Platform.StartsWith("Mobile"))
            {
                foreach (var item in publishProjects)
                {
                    if (_useMaui)
                    {
                        var project = item.project.GetMSBuildProject();
                        project.SetProperty("ApplicationDisplayVersion", _version.ToString(3));
                        project.SetProperty("ApplicationVersion", (_version.Build + (_version.Build == 0 ? 1 : 0)).ToString());
                        project.Save(item.project.Path);
                    }
                }

            var publishCombinations =
                from project in PublishProjects.Select(m => Solution.AllProjects.FirstOrDefault(p=> p.Name == m))
                from framework in project.GetTargetFrameworks()
                select new { project, framework };

            DotNetPublish(s => s
                .EnableNoRestore()
                .EnableNoBuild()
                .SetConfiguration(Configuration)
                .DisablePublishSingleFile()
                .CombineWith(publishCombinations, (x, v) => x
                    .SetProject(v.project)
                    .SetFramework(v.framework)
                    .SetOutput($"{PublishDirectory}/{v.project.Name}")));
        });

    Target Pack => d => d
        .DependsOn(Publish)
        .Executes(() =>
        {
            CopyDirectoryRecursively(ScriptsDirectory, $"{ArtifactsDirectory}", DirectoryExistsPolicy.Merge);
            foreach (var project in Projects)
            {
                AbsolutePathExtensions.DeleteFile($"{ArtifactsDirectory}/{project.Name}.zip");
                ZipFile.CreateFromDirectory($"{PublishDirectory}/{project.Name}", $"{ArtifactsDirectory}/{project.Name}.zip");
            }

            AbsolutePath.Create(PublishDirectory).CreateOrCleanDirectory();
            AbsolutePath.Create(TestResultsDirectory).CreateOrCleanDirectory();
            AbsolutePath.Create(ScriptsDirectory).CreateOrCleanDirectory();
            Log.Information($"Output: {ArtifactsDirectory}");
        });

    Target XamlStyler => d => d
        .Executes(() =>
        {
            DotNetToolRestore();
            SourceDirectory.GetFiles("*.xaml", int.MaxValue)
                .ForEach(m =>
                {
                    DotNet($"xstyler -f {m}");
                });
        });
}
#pragma warning restore CA1050 // Declare types in namespaces
#pragma warning restore IDE1006 // Naming Styles
