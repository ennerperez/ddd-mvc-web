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

// ReSharper disable UnusedMember.Local

partial class Build : NukeBuild
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

	[Solution] readonly Solution Solution;

	AbsolutePath SourceDirectory => RootDirectory / "src";
	AbsolutePath TestsDirectory => RootDirectory / "tests";

	AbsolutePath PublishDirectory => RootDirectory / "publish";
	AbsolutePath ArtifactsDirectory => RootDirectory / "output";
	AbsolutePath TestResultsDirectory => RootDirectory / "testresults";
	AbsolutePath ScriptsDirectory => RootDirectory / "scripts";

	Version _version = new Version("1.0.0.0");
	string _hash = string.Empty;

	string[] PublishProjects = new[] {"Web"};
	string[] TestsProjects = new[] {"Tests.Business"};

	Target Prepare => _ => _
		.Before(Compile)
		.Executes(() =>
		{

			var assemblyInfoVersionFile = Path.Combine(SourceDirectory, path2: ".files", path3: "AssemblyInfo.Version.cs");
			if (File.Exists(assemblyInfoVersionFile))
			{

				Log.Information(messageTemplate: "Patching: {File}", assemblyInfoVersionFile);

				using (var gitTag = new Process() {StartInfo = new ProcessStartInfo(fileName: "git", arguments: "tag --sort=-v:refname") {WorkingDirectory = SourceDirectory, RedirectStandardOutput = true, UseShellExecute = false}})
				{
					gitTag.Start();
					var value = gitTag.StandardOutput.ReadToEnd().Trim();
					value = new Regex(pattern: @"((?:[0-9]{1,}\.{0,}){1,})", RegexOptions.Compiled).Match(value).Captures.LastOrDefault()?.Value;
					Version.TryParse(value, out _version);
					gitTag.WaitForExit();
				}

				using (var gitLog = new Process() {StartInfo = new ProcessStartInfo(fileName: "git", arguments: "rev-parse --verify HEAD") {WorkingDirectory = SourceDirectory, RedirectStandardOutput = true, UseShellExecute = false}})
				{
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

		});

	Target Clean => _ => _
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

	Target Restore => _ => _
		.Executes(() =>
		{
			DotNetToolRestore();
			DotNetRestore(s => s
				.SetProjectFile(Solution));
		});

	Target Compile => _ => _
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

	Target Test => _ => _
		.DependsOn(Compile)
		.Executes(() =>
		{
			var testCombinations =
				from project in TestsProjects.Select(m => Solution.GetProject(m))
				from framework in project.GetTargetFrameworks()
				select new {project, framework};

			DotNetTest(s => s
				.EnableNoRestore()
				.EnableNoBuild()
				.SetConfiguration(Configuration)
				.When(true, _ => _
					.SetLoggers("trx")
					.SetResultsDirectory(TestResultsDirectory))
				.CombineWith(testCombinations, configurator: (_, v) => _
					.SetProjectFile(v.project.Path)
					.SetFramework(v.framework)));

		});

	Target Publish => _ => _
		//.DependsOn(Test)
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
				let split = item.Key.Split("_")
				where split.Length > 1
				let context = split.First()
				let provider = split.Last()
				where provider != "Sqlite"
				select new {Context = context, Name = context.Replace("Context", ""), Provider = provider, item.Value};

			foreach (var item in combinations)
			{
				var fileName = Path.Combine(ScriptsDirectory, $"{item.Name}_{item.Provider}_{DateTime.Now:yyyyMMdd}.sql");
				if (File.Exists(fileName)) File.Delete(fileName);
				DotNetEf(_ => new MigrationsSettings(Migrations.Script)
					.SetProjectFile(Solution.GetProject(persistence))
					.SetStartupProjectFile(Solution.GetProject(startup))
					.SetContext(item.Context)
					.SetOutput(fileName)
				);
			}

			var publishCombinations =
				from project in PublishProjects.Select(m => Solution.GetProject(m))
				from framework in project.GetTargetFrameworks()
				select new {project, framework};

			DotNetPublish(s => s
				.EnableNoRestore()
				.EnableNoBuild()
				.SetConfiguration(Configuration)
				.DisablePublishSingleFile()
				.CombineWith(publishCombinations, configurator: (_, v) => _
					.SetProject(v.project)
					.SetFramework(v.framework)
					.SetOutput($"{PublishDirectory}/{v.project.Name}")));
		});

	Target Pack => _ => _
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
