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

    Version _version = new("1.0.0.0");
    string _hash = string.Empty;
    readonly string[] PublishProjects = new[] { "Web" };
    readonly string[] TestsProjects = new[] { "Tests.Business" };

    Target Prepare => d => d
        .Before(Compile)
        .Executes(() =>
        {
            var assemblyInfoVersionFile = Path.Combine(SourceDirectory, ".files", "AssemblyInfo.Version.cs");
            if (File.Exists(assemblyInfoVersionFile))
            {
                Log.Information("Patching: {File}", assemblyInfoVersionFile);

                using (var gitTag = new Process())
                {
                    gitTag.StartInfo = new ProcessStartInfo("git", "tag --sort=-v:refname") { WorkingDirectory = SourceDirectory, RedirectStandardOutput = true, UseShellExecute = false };
                    gitTag.Start();
                    var value = gitTag.StandardOutput.ReadToEnd().Trim();
                    value = new Regex(@"((?:[0-9]{1,}\.{0,}){1,})", RegexOptions.Compiled).Match(value).Captures.LastOrDefault()?.Value;
                    if (value != null)
                    {
                        _version = Version.Parse(value);
                    }

                    gitTag.WaitForExit();
                }

                using (var gitLog = new Process())
                {
                    gitLog.StartInfo = new ProcessStartInfo("git", "rev-parse --verify HEAD") { WorkingDirectory = SourceDirectory, RedirectStandardOutput = true, UseShellExecute = false };
                    gitLog.Start();
                    _hash = gitLog.StandardOutput.ReadLine()?.Trim().Split(" ", StringSplitOptions.RemoveEmptyEntries).LastOrDefault();
                    gitLog.WaitForExit();
                }

                if (_version != null)
                {
                    var content = File.ReadAllText(assemblyInfoVersionFile);
                    var assemblyVersionRegEx = new Regex(@"\[assembly: AssemblyVersion\(.*\)\]", RegexOptions.Compiled);
                    var assemblyFileVersionRegEx = new Regex(@"\[assembly: AssemblyFileVersion\(.*\)\]", RegexOptions.Compiled);
                    var assemblyInformationalVersionRegEx = new Regex(@"\[assembly: AssemblyInformationalVersion\(.*\)\]", RegexOptions.Compiled);

                    content = assemblyVersionRegEx.Replace(content, $"[assembly: AssemblyVersion(\"{_version}\")]");
                    content = assemblyFileVersionRegEx.Replace(content, $"[assembly: AssemblyFileVersion(\"{_version}\")]");
                    content = assemblyInformationalVersionRegEx.Replace(content, $"[assembly: AssemblyInformationalVersion(\"{_version:3}+{_hash}\")]");

                    File.WriteAllText(assemblyInfoVersionFile, content);

                    Log.Information("Version: {Version}", _version);
                    Log.Information("Hash: {Hash}", _hash);
                }
                else
                {
                    Log.Warning("Version was not found");
                }
            }
        });

    Target Clean => d => d
        .Before(Restore)
        .Executes(() =>
        {
            SourceDirectory.GlobDirectories("**/bin", "**/obj").ForEach(DeleteDirectory);
            TestsDirectory.GlobDirectories("**/bin", "**/obj").ForEach(DeleteDirectory);
            EnsureCleanDirectory(PublishDirectory);
            EnsureCleanDirectory(ArtifactsDirectory);
            EnsureCleanDirectory(TestResultsDirectory);
            EnsureCleanDirectory(ScriptsDirectory);
        });

    Target Restore => d => d
        .Executes(() =>
        {
            DotNetToolRestore();
            DotNetRestore(s => s
                .SetProjectFile(Solution));
        });

    Target Compile => d => d
        .DependsOn(Clean)
        .DependsOn(Restore)
        .DependsOn(Prepare)
        .Executes(() =>
        {
            DotNetBuild(s => s
                .SetProjectFile(Solution)
                .SetConfiguration(Configuration)
                .EnableNoRestore());
        });

    Target Test => d => d
        .DependsOn(Compile)
        .Executes(() =>
        {
            var testCombinations =
                from project in TestsProjects.Select(m => Solution.GetProject(m))
                from framework in project.GetTargetFrameworks()
                select new { project, framework };

            DotNetTest(s => s
                .EnableNoRestore()
                .EnableNoBuild()
                .SetConfiguration(Configuration)
                .When(true, x => x
                    .SetLoggers("trx")
                    .SetResultsDirectory(TestResultsDirectory))
                .CombineWith(testCombinations, (x, v) => x
                    .SetProjectFile(v.project.Path)
                    .SetFramework(v.framework)));
        });

    Target Publish => d => d
        .DependsOn(Test)
        .DependsOn(Compile)
        .DependsOn(Clean)
        .Executes(() =>
        {
            var persistence = Solution.GetProject("Persistence")?.Path;
            var startup = Solution.GetProject("Web")?.Path;
            var startupPath = Solution.GetProject("Web")?.Directory ?? string.Empty;

            var config = new ConfigurationBuilder()
                .AddJsonFile(Path.Combine(startupPath, "appsettings.json"), false, true)
                .AddJsonFile(Path.Combine(startupPath, $"appsettings.{Environment}.json"), true, true)
                .Build();

            var connectionStrings = new Dictionary<string, string>();
            config.Bind("ConnectionStrings", connectionStrings);

            var combinations = from item in connectionStrings
                               let split = item.Key.Split(".")
                               where split.Length > 1
                               let context = split.First()
                               let provider = split.Last()
                               where provider != "Sqlite"
                               select new { Context = context, Name = context.Replace("Context", ""), Provider = provider, item.Value };

            foreach (var item in combinations)
            {
                var fileName = Path.Combine(ScriptsDirectory, $"{item.Name}_{item.Provider}_{DateTime.Now:yyyyMMdd}.sql");
                if (File.Exists(fileName))
                {
                    File.Delete(fileName);
                }

                DotNetEf(_ => new MigrationsSettings(Migrations.Script)
                    .EnableIdempotent()
                    .SetProjectFile(Solution.GetProject(persistence))
                    .SetStartupProjectFile(Solution.GetProject(startup))
                    .SetContext(item.Context)
                    .SetOutput(fileName)
                );
            }

            var publishCombinations =
                from project in PublishProjects.Select(m => Solution.GetProject(m))
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
            CopyDirectoryRecursively(ScriptsDirectory, $"{ArtifactsDirectory}/Scripts");
            CopyDirectoryRecursively(TestResultsDirectory, $"{ArtifactsDirectory}/Tests");
            foreach (var project in PublishProjects)
            {
                ZipFile.CreateFromDirectory($"{PublishDirectory}/{project}", $"{ArtifactsDirectory}/{project}.zip");
            }

            EnsureCleanDirectory(PublishDirectory);
            EnsureCleanDirectory(TestResultsDirectory);
            EnsureCleanDirectory(ScriptsDirectory);
            Log.Information($"Output: {ArtifactsDirectory}");
        });
}
#pragma warning restore CA1050 // Declare types in namespaces
#pragma warning restore IDE1006 // Naming Styles
