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
	public static int Main() => Execute<Build>(x => x.Pack);

	[Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")]
	readonly Configuration Configuration = IsLocalBuild ? Configuration.Debug : Configuration.Release;

	[Solution] readonly Solution Solution;

	AbsolutePath SourceDirectory => RootDirectory / "src";
	AbsolutePath TestsDirectory => RootDirectory / "tests";

	AbsolutePath PublishDirectory => RootDirectory / "publish";
	AbsolutePath ArtifactsDirectory => RootDirectory / "output";

	Target Clean => _ => _
		.Before(Restore)
		.Executes(() =>
		{
			SourceDirectory.GlobDirectories("**/bin", "**/obj").ForEach(DeleteDirectory);
			TestsDirectory.GlobDirectories("**/bin", "**/obj").ForEach(DeleteDirectory);
			EnsureCleanDirectory(PublishDirectory);
			EnsureCleanDirectory(ArtifactsDirectory);
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
				.SetProject(Solution.GetProject("Web"))
				.SetConfiguration(Configuration)
				.SetOutput(PublishDirectory)
				.EnableNoRestore()
				.EnableNoBuild());
		});

	Target Pack => _ => _
		.DependsOn(Publish)
		.Executes(() =>
		{
			ZipFile.CreateFromDirectory(PublishDirectory, ArtifactsDirectory / "Release.zip");
		});

}
