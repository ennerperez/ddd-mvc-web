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
    CleanDirectory(publishDirectory);
});

Task("Build")
    .IsDependentOn("Clean")
    .Does(() =>
{
    var settings = new DotNetPublishSettings
    {
        Configuration = configuration,
        OutputDirectory = outputDirectory
    };
    
    DotNetPublish("./src/Web/Web.csproj", settings);

});

Task("Zip")
    .IsDependentOn("Build")
    .Does(() =>
{
    Zip(outputDirectory, System.IO.Path.Combine(outputDirectory,"..",configuration+".zip"));    
});

Task("Test")
    .IsDependentOn("Zip")
    .Does(() =>
{
    DotNetTest("./Solution.sln", new DotNetTestSettings
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