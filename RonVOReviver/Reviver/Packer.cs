using NLog;
using System.Diagnostics;
using System.IO;

namespace RonVOReviver.Reviver;

public static class Packer
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    private const string PakDirectory = ".\\paking";
    private static readonly string PakExecutable = Path.GetFullPath($"{PakDirectory}\\ron_pak.bat");

    private static void OpenExplorer(string path)
    {
        ProcessStartInfo? processInfo = new("explorer", $"\"{path}\"");
        Process.Start(processInfo);
    }

    public static async Task PackAsync(string pakPath)
    {
        if (!Directory.Exists(pakPath))
        {
            Logger.Error($"The pak folder does not exist: {pakPath}");
            throw new DirectoryNotFoundException($"The folder does not exist:\n{pakPath}");
        }

        ProcessStartInfo processInfo = new(PakExecutable, $"\"{pakPath}\"");

        Logger.Info($"Starting paking process: {pakPath}");

        using Process? p = Process.Start(processInfo);
        if (p == null)
        {
            Logger.Info($"Paking process not started");
            return;
        }
        await p.WaitForExitAsync();
        Logger.Info($"Paking process finished");

        OpenExplorer($"{pakPath}\\..");
    }
}