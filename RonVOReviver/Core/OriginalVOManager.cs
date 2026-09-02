using NLog;
using RonVOReviver.Models;
using System.IO;

namespace RonVOReviver.Core;

public class OriginalVOManager(string path, IProgress<VOManagerProgressReport>? progress = null) : VOManager(path, progress)
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    public override string[] GetVOFiles()
    {
        if (!File.Exists(FolderPath))
        {
            Logger.Warn($"Vanilla VO list file not found: {FolderPath}");
            return [];
        }

        try
        {
            string[] lines = File.ReadAllLines(FolderPath);
            return [.. lines
                .Select(line => line.Trim())
                .Where(line => !string.IsNullOrWhiteSpace(line))];
        }
        catch (Exception ex)
        {
            Logger.Error($"Failed to read vanilla VO list file {FolderPath}: {ex.Message}");
            return [];
        }
    }
}

