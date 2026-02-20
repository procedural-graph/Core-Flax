using Flax.Build;
using Flax.Build.NativeCpp;
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace ProceduralGraph.FlaxEngine;

/// <summary>
/// ProceduralGraphCore EditorTarget.
/// </summary>
public class ProceduralGraphCoreEditorTarget : GameProjectEditorTarget
{
    /// <inheritdoc/>
    public override void SetupTargetEnvironment(BuildOptions options)
    {
        options.ScriptingAPI.SystemReferences.Add("System.Diagnostics");
    }

    /// <inheritdoc />
    public override void Init()
    {
        base.Init();   
        Modules.Add(nameof(CommonModule));
    }

    /// <inheritdoc/>
    public override void PreBuild()
    {
        base.PreBuild();
        Exec("dotnet build \"..\\..\\Generic\\Generic.csproj\" -c Release -f net9.0 -o \"..\\Content\\Assemblies\\Public\" -p:GenerateDocumentationFile=true -p:DebugType=portable -p:GenerateDependencyFile=false -p:CopyLocalLockFileAssemblies=true");
    }

    private static void Exec(string command, [CallerFilePath] string workingDirectory = "")
    {
        string fileName;
        string arguments;

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            fileName = "cmd.exe";
            arguments = $"/c {command}";
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux) || RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            fileName = "/bin/bash";
            arguments = $"-c \"{command}\"";
        }
        else
        {
            throw new PlatformNotSupportedException("Unsupported operating system.");
        }

        ProcessStartInfo startInfo = new()
        {
            WorkingDirectory = Path.GetDirectoryName(workingDirectory),
            FileName = fileName,
            Arguments = arguments,
            CreateNoWindow = true,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };

        Process process = Process.Start(startInfo) ?? throw new InvalidOperationException("Failed to start process.");

        Task.WaitAll(LogStandardOutputAsync(process.StandardOutput), LogErrorOutputAsync(process.StandardError));

        if (process.ExitCode != 0)
        {
            throw new InvalidOperationException($"Command execution failed with exit code {process.ExitCode}.");
        }
    }

    private static async Task LogStandardOutputAsync(StreamReader streamReader)
    {
        while (await streamReader.ReadLineAsync() is string line)
        {
            Log.Info(line);
        }
    }

    private static async Task LogErrorOutputAsync(StreamReader streamReader)
    {
        while (await streamReader.ReadLineAsync() is string line)
        {
            Log.Error(line);
        }
    }
}
