using System.IO;
using System.IO.Compression;
using Nuke.Common;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Utilities.Collections;
using static Nuke.Common.IO.FileSystemTasks;
using static Nuke.Common.Tools.DotNet.DotNetTasks;
// ReSharper disable UnusedMember.Local

class Build : NukeBuild
{
	/// Support plugins are available for:
	///   - JetBrains ReSharper        https://nuke.build/resharper
	///   - JetBrains Rider            https://nuke.build/rider
	///   - Microsoft VisualStudio     https://nuke.build/visualstudio
	///   - Microsoft VSCode           https://nuke.build/vscode
	public static int Main() => Execute<Build>(x => x.Compile);

	[Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")]
	readonly Configuration Configuration = Configuration.Release; // IsLocalBuild ? Configuration.Debug : Configuration.Release;

	[Solution] readonly Solution Solution;

	AbsolutePath SourceDirectory => RootDirectory / "src";
	AbsolutePath TestsDirectory => RootDirectory / "tests";
	AbsolutePath OutputDirectory => RootDirectory / "output";
	AbsolutePath BuildDirectory => RootDirectory / "build";

	Target Clean => _ => _
		.Before(Restore)
		.Executes(() =>
		{
			SourceDirectory.GlobDirectories("**/bin", "**/obj").ForEach(DeleteDirectory);
			TestsDirectory.GlobDirectories("**/bin", "**/obj").ForEach(DeleteDirectory);
			EnsureCleanDirectory(OutputDirectory);
			EnsureCleanDirectory(BuildDirectory);
		});

	Target Restore => _ => _
		.Executes(() =>
		{
			DotNetRestore(s => s
				.SetProjectFile(Solution));
		});

	Target Compile => _ => _
		.DependsOn(Restore)
		.Executes(() =>
		{
			DotNetBuild(s => s
				.SetProjectFile(Solution)
				.SetConfiguration(Configuration)
				.EnableNoRestore());
		});

	Target Publish => _ => _
		.DependsOn(Compile)
		.DependsOn(Clean)
		.Executes(() =>
		{
			DotNetPublish(s => s
				.SetProject(Solution)
				.SetConfiguration(Configuration)
				.SetOutput(OutputDirectory)
				.EnableNoRestore()
				.EnableNoBuild());
		});

	Target Zip => _ => _
		.DependsOn(Publish)
		.Executes(() =>
		{
			if (!Directory.Exists(BuildDirectory)) Directory.CreateDirectory(BuildDirectory);
			ZipFile.CreateFromDirectory(OutputDirectory, BuildDirectory / "Release.zip");
			EnsureCleanDirectory(OutputDirectory);
		});

}
