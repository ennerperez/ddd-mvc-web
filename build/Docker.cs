using System;
using System.Diagnostics;
using Nuke.Common;
using Serilog;

// ReSharper disable UsageOfDefaultStructEquality

// ReSharper disable UnusedMember.Local
#pragma warning disable IDE0051 // Remove unused private members
public partial class Build
{
    #region Registry

    [Parameter("Registry Name")]
    public readonly string RegistryName;

    [Parameter("Registry Host")]
    public readonly string RegistryHost;

    [Secret]
    [Parameter("Registry Username")]
    public readonly string RegistryUsername;

    [Secret]
    [Parameter("Registry Password")]
    public readonly string RegistryPassword;

    #endregion

    #region Docker

    [Parameter("Container Name")]
    public readonly string ContainerName;

    [Parameter("Project Name")]
    public readonly string ProjectName;

    #endregion

    string ContainerTag => $"v{_version:3}{(!string.IsNullOrWhiteSpace(_versionTag) ? $"-{_versionTag}" : "")}";

    Target DockerBuild => d => d
        .DependsOn(Versioning)
        .DependsOn(Publish)
        .Executes(() =>
        {
            var containerName = !string.IsNullOrEmpty(ContainerName) ? ContainerName : Solution.Name;
            var registryName = !string.IsNullOrEmpty(RegistryName) ? RegistryName : !string.IsNullOrEmpty(RegistryHost) ? new Uri(RegistryHost).Host : string.Empty;
            var imageName = !string.IsNullOrWhiteSpace(registryName) ? $"{registryName}/{containerName}" : $"{containerName}";
            var projectName = !string.IsNullOrEmpty(ProjectName) ? ProjectName : Project;
            Log.Information($"Building {imageName} image");
            using var dockerBuild = new Process();
            dockerBuild.StartInfo = new ProcessStartInfo(fileName: "docker", arguments: $"build --build-arg configuration={Configuration} -f src/{projectName}/Dockerfile -t {imageName}:latest -t {imageName}:{ContainerTag} .")
            {
                WorkingDirectory = RootDirectory,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
                UseShellExecute = false
            };

            dockerBuild.OutputDataReceived += OnEventHandler;
            dockerBuild.ErrorDataReceived += OnEventHandler;

            dockerBuild.Start();
            dockerBuild.BeginOutputReadLine();
            dockerBuild.BeginErrorReadLine();
            dockerBuild.WaitForExit();
            dockerBuild.CancelOutputRead();
            dockerBuild.CancelErrorRead();
            return;

            void OnEventHandler(object sender, DataReceivedEventArgs e)
            {
                if (e.Data == null) { return; }

                if (e.Data.Contains("WARNING", StringComparison.OrdinalIgnoreCase)) { Log.Warning(e.Data); }
                else if (e.Data.Contains("ERROR", StringComparison.OrdinalIgnoreCase) ||
                         e.Data.Contains("FAILED", StringComparison.OrdinalIgnoreCase) ||
                         e.Data.Contains("INVALID", StringComparison.OrdinalIgnoreCase))
                {
                    var ex = new Exception(e.Data);
                    Log.Error(ex, "{Message}", ex.Message);
                    throw ex;
                }
            }
        });

    Target DockerLogin => d => d
        .After(Publish)
        .Before(DockerPush)
        .Executes(() =>
        {
            if (string.IsNullOrWhiteSpace(RegistryHost)) { throw new ArgumentNullException(nameof(RegistryHost)); }

            if (string.IsNullOrWhiteSpace(RegistryUsername)) { throw new ArgumentNullException(nameof(RegistryUsername)); }

            if (string.IsNullOrWhiteSpace(RegistryPassword)) { throw new ArgumentNullException(nameof(RegistryPassword)); }

            Log.Information($"Login into {RegistryHost}");
            using var dockerLogin = new Process();
            dockerLogin.StartInfo = new ProcessStartInfo(fileName: "docker", arguments: $"login {RegistryHost} --username {RegistryUsername} --password {RegistryPassword}")
            {
                WorkingDirectory = RootDirectory,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
                UseShellExecute = false
            };

            dockerLogin.OutputDataReceived += OnEventHandler;
            dockerLogin.ErrorDataReceived += OnEventHandler;

            dockerLogin.Start();
            dockerLogin.BeginOutputReadLine();
            dockerLogin.BeginErrorReadLine();
            dockerLogin.WaitForExit(15000);
            dockerLogin.CancelOutputRead();
            dockerLogin.CancelErrorRead();
            return;

            void OnEventHandler(object sender, DataReceivedEventArgs e)
            {
                if (e.Data == null) { return; }

                if (e.Data.Contains("WARNING", StringComparison.OrdinalIgnoreCase)) { Log.Warning(e.Data); }
                else if (e.Data.Contains("ERROR", StringComparison.OrdinalIgnoreCase) ||
                         e.Data.Contains("FAILED", StringComparison.OrdinalIgnoreCase) ||
                         e.Data.Contains("INVALID", StringComparison.OrdinalIgnoreCase))
                {
                    var ex = new Exception(e.Data);
                    Log.Error(ex, "{Message}", ex.Message);
                    throw ex;
                }
            }
        });

    Target DockerPush => d => d
        .DependsOn(Versioning)
        .DependsOn(DockerBuild)
        .DependsOn(DockerLogin)
        .Executes(() =>
        {
            var tags = new[] { ContainerTag, "latest" };
            var containerName = !string.IsNullOrEmpty(ContainerName) ? ContainerName : Solution.Name;
            var registryName = !string.IsNullOrEmpty(RegistryName) ? RegistryName : new Uri(RegistryHost).Host;
            var imageName = !string.IsNullOrWhiteSpace(registryName) ? $"{registryName}/{containerName}" : $"{containerName}";
            foreach (var tag in tags)
            {
                Log.Information($"Pushing {imageName}:{tag}");
                using var dockerPush = new Process();
                dockerPush.StartInfo = new ProcessStartInfo(fileName: "docker", arguments: $"push {imageName}:{tag}")
                {
                    WorkingDirectory = RootDirectory,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true,
                    UseShellExecute = false
                };

                dockerPush.OutputDataReceived += OnEventHandler;
                dockerPush.ErrorDataReceived += OnEventHandler;

                dockerPush.Start();
                dockerPush.BeginOutputReadLine();
                dockerPush.BeginErrorReadLine();
                dockerPush.WaitForExit();
                dockerPush.CancelOutputRead();
                dockerPush.CancelErrorRead();
            }

            return;

            void OnEventHandler(object sender, DataReceivedEventArgs e)
            {
                if (e.Data == null) { return; }

                if (e.Data.Contains("WARNING", StringComparison.OrdinalIgnoreCase)) { Log.Warning(e.Data); }
                else if (e.Data.Contains("ERROR", StringComparison.OrdinalIgnoreCase) ||
                         e.Data.Contains("FAILED", StringComparison.OrdinalIgnoreCase) ||
                         e.Data.Contains("INVALID", StringComparison.OrdinalIgnoreCase))
                {
                    var ex = new Exception(e.Data);
                    Log.Error(ex, "{Message}", ex.Message);
                    throw ex;
                }
            }
        });

    Target DockerPrune => d => d
        .After(DockerBuild)
        .Executes(() =>
        {
            using var dockerBuilderPrune = new Process();
            Log.Information($"Pruning builder");
            dockerBuilderPrune.StartInfo = new ProcessStartInfo(fileName: "docker", arguments: $"builder prune -f")
            {
                WorkingDirectory = RootDirectory,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
                UseShellExecute = false
            };

            dockerBuilderPrune.OutputDataReceived += OnEventHandler;
            dockerBuilderPrune.ErrorDataReceived += OnEventHandler;

            dockerBuilderPrune.Start();
            dockerBuilderPrune.BeginOutputReadLine();
            dockerBuilderPrune.BeginErrorReadLine();
            dockerBuilderPrune.WaitForExit();
            dockerBuilderPrune.CancelOutputRead();
            dockerBuilderPrune.CancelErrorRead();
            return;

            void OnEventHandler(object sender, DataReceivedEventArgs e)
            {
                if (e.Data == null) { return; }

                if (e.Data.Contains("WARNING", StringComparison.OrdinalIgnoreCase)) { Log.Warning(e.Data); }
                else if (e.Data.Contains("ERROR", StringComparison.OrdinalIgnoreCase) ||
                         e.Data.Contains("FAILED", StringComparison.OrdinalIgnoreCase) ||
                         e.Data.Contains("INVALID", StringComparison.OrdinalIgnoreCase))
                {
                    var ex = new Exception(e.Data);
                    Log.Error(ex, "{Message}", ex.Message);
                    throw ex;
                }
            }
        });
}

#pragma warning restore CA1050 // Declare types in namespaces
#pragma warning restore IDE1006 // Naming Styles
