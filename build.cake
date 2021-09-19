//////////////////////////////////////////////////////////////////////
// ARGUMENTS
//////////////////////////////////////////////////////////////////////

var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");

//////////////////////////////////////////////////////////////////////
// PREPARATION
//////////////////////////////////////////////////////////////////////

// Define directories.
var outputDirectory = $"build/{configuration}";
var publishDirectory = "build/Publish";

//////////////////////////////////////////////////////////////////////
// TASKS
//////////////////////////////////////////////////////////////////////

Task("Clean")
    .WithCriteria(c => HasArgument("rebuild"))
    .Does(() =>
{
    CleanDirectory($"./src/Domain/bin/{configuration}");
    CleanDirectory($"./src/Infrastructure/bin/{configuration}");
    CleanDirectory($"./src/Persistence/bin/{configuration}");
    CleanDirectory($"./src/Web/bin/{configuration}");
});

Task("Build")
    .IsDependentOn("Clean")
    .Does(() =>
{
    DotNetCoreBuild("./src/Solution.sln", new DotNetCoreBuildSettings
    {
        Configuration = configuration,
        NoRestore = true,
        OutputDirectory = outputDirectory,
        ArgumentCustomization = args => args.Append($@"/p:PublishProfile={configuration} /p:PackageLocation=""../../{publishDirectory}"" /p:DeployOnBuild=true /p:WebPublishMethod=Package /p:PackageAsSingleFile=true"),
    });
});

Task("Test")
    .IsDependentOn("Build")
    .Does(() =>
{
    DotNetCoreTest("./src/Solution.sln", new DotNetCoreTestSettings
    {
        Configuration = configuration,
        NoBuild = true,
    });
});

Task("Default")
    .IsDependentOn("Test");

//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////

RunTarget(target);