using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace RonVOReviver.Reviver;

public class ModdedVOManager : VOManager
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    public static readonly string[] AllowedPatterns = ["*.ogg", "*.wav", "*.mp3", "*.m4a"];

    public bool IsOgg { get; protected set; } = false;

    public ModdedVOManager() : base() { }

    public ModdedVOManager(string path, Callback progressCallback, Callback onFormatExceptionCallback) :
        base(path, progressCallback, onFormatExceptionCallback) { }

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
