using NLog;
using System.IO;

namespace RonVOReviver.Reviver;

public class ModdedVOManager(string path, IProgress<VOManagerProgressReport>? progress = null) : VOManager(path, progress)
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    public static readonly string[] AllowedPatterns = ["*.ogg", "*.wav", "*.mp3", "*.m4a"];

    public bool IsOgg { get; protected set; } = false;

    public override string[] GetVOFiles()
    {
        for (int i = 0; i < AllowedPatterns.Length; ++i)
        {
            string[] files = Directory.GetFiles(FolderPath, AllowedPatterns[i]);
            if (files.Length > 0)
            {
                Logger.Info($"Found modded files with {AllowedPatterns[i]} type in {FolderPath}");
                IsOgg = i == 0;
                return files;
            }
            Logger.Info($"Didn't find any modded files with {AllowedPatterns[i]} type in {FolderPath}");
        }
        return [];
    }
}
