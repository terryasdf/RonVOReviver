using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RonVOReviver.Reviver;

public class ModdedVOManager : VOManager
{
    public static readonly string[] AllowedPatterns = ["*.ogg", "*.wav", "*.mp3"];

    public ModdedVOManager() : base() { }

    public ModdedVOManager(string path, Callback progressCallback, Callback onFormatExceptionCallback) :
        base(path, progressCallback, onFormatExceptionCallback) { }

    public override string[] GetVOFiles()
    {
        foreach (string ext in AllowedPatterns)
        {
            string[] files = Directory.GetFiles(FolderPath, ext);
            if (files.Length > 0)
            {
                return files;
            }
        }
        return [];
    }
}
