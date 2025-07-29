using NLog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RonVOReviver.Reviver;

public class VOReviver
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    private VOManager _originalVoManager = new();
    private VOManager _moddedVoManager = new();

    public int ZeroFillLength { get; set; } = 1;     
    public string OriginalVoFolderPath { get; set; } = string.Empty;
    public string ModdedVoFolderPath { get; set; } = string.Empty;
    public string DestionationFolderPath { get; set; } = string.Empty;

    public void CopyVoFiles(string dstPath,
        out List<string> missingInModVoTypes, out List<string> extraVoTypeFiles, Callback callback)
    {
        // Clear destination directory
        if (Directory.Exists(dstPath))
        {
            Directory.Delete(dstPath, true);
        }
        Directory.CreateDirectory(dstPath);

        missingInModVoTypes = [];
        extraVoTypeFiles = [];

        //int nextTypeCur = 0, numModdedVo = _moddedVoManager.;
        //for (int i = 0; i < numModdedVo; i = nextTypeCur)
        //{
        //    string file = ModdedVoFiles[i];
        //    string voType = VOManager.GetVoType(file, out int id);
        //    while (nextTypeCur < numModdedVo &&
        //            VOManager.GetVoType(ModdedVoFiles[i+1], out int _).Equals(voType))
        //    {
        //        ++nextTypeCur;
        //    }

        //    int numRepeat = ( + );

        //    if (!_manager.ContainsVoType(voType))
        //    {
        //        Debug.Assert(File.Exists(dstPath));
        //        string dstFile = $"{dstPath}\\{voType}_{id.ToString($"D{ZeroFillLength}")}.ogg";
        //        File.Copy(file, dstFile);
        //        extraVoTypeFiles.Add(file);

        //        Logger.Info($"Copied \"{file}\" as \"{dstFile}\"");
        //        callback(dstPath);
        //        continue;
        //    }

        //    ;
        //}
    }
}
