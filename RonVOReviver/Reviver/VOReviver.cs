﻿using NLog;
using System.Diagnostics;
using System.IO;

namespace RonVOReviver.Reviver;

public class VOReviver
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
    private static readonly string InPakVOPath = "Content\\VO_PC";

    private VOManager _originalVOManager = new();
    private VOManager _moddedVOManager = new();

    private string _destinationFolderPath = string.Empty;

    public int ZeroFillLength { get; set; } = 1;
    public string Character { get; set; } = string.Empty;

    public string ZeroFill(int x) => x.ToString($"D{ZeroFillLength}");

    public void SetOriginalVOFolderPath(string path, Callback progressCallback,
        Callback onFormatExceptionCallback)
    {
        _originalVOManager = new(path, progressCallback, onFormatExceptionCallback);
        ZeroFillLength = _originalVOManager.ZeroFillLength;
        Character = Path.GetFileName(path);
    }

    public void SetModdedVOFolderPath(string path, Callback progressCallback,
        Callback onFormatExceptionCallback)
    {
        _moddedVOManager = new(path, progressCallback, onFormatExceptionCallback);
    }

    public void SetDestionationFolderPath(string path) => _destinationFolderPath = path;

    public void PakVOFiles() => Packer.Pack(_destinationFolderPath);

    public void CopyVOFiles(out List<string> missingVOTypes,
        Callback extraVOTypeFileCallback, Callback progressCallback,
        Callback onIOExceptionCallback)
    {
        missingVOTypes = [];

        // Clear destination directory
        string newVOFolderPath = $"{_destinationFolderPath}\\{InPakVOPath}\\{Character}";
        if (Directory.Exists(_destinationFolderPath))
        {
            Directory.Delete(_destinationFolderPath, true);
            Logger.Debug($"Deleted folder: {_destinationFolderPath}");
        }
        Directory.CreateDirectory(newVOFolderPath);

        int nextTypeCur = 0;
        int numModdedVO = _moddedVOManager.Files.Count;
        string[] moddedVOFiles = [.. _moddedVOManager.Files];

        using SubtitleHandler subtitleHandler = new(_moddedVOManager.FolderPath,
            newVOFolderPath, onIOExceptionCallback);

        for (int i = 0; i < numModdedVO; i = nextTypeCur)
        {
            // Find all files of one voType
            string voType = VOManager.GetVOType(moddedVOFiles[i], out string _);
            while (nextTypeCur < numModdedVO &&
                VOManager.GetVOType(moddedVOFiles[nextTypeCur], out string _).Equals(voType))
            {
                ++nextTypeCur;
            }

            int numOriginal = _originalVOManager.GetMaxIndex(voType);
            int numModded = _moddedVOManager.GetCount(voType);

            Debug.Assert(numModded > 0);
            // Times of reusing modded files to fully replace original files:
            // Ceil{(numOriginal + 1) / numModded}
            int numRepeat = (numOriginal + numModded) / numModded;

            // Copy files for numRepeat times
            int index = 0;
            while (numRepeat-- > 0)
            {
                for (int j = i; j < nextTypeCur; ++j)
                {
                    string oldKey = Path.GetFileNameWithoutExtension(moddedVOFiles[j]);
                    string newKey = $"{voType}_{ZeroFill(index++)}";
                    string dstFile = $"{newVOFolderPath}\\{newKey}.ogg";
                    try
                    {
                        File.Copy(moddedVOFiles[j], dstFile);
                        Logger.Debug($"Copied \"{moddedVOFiles[j]}\" as \"{dstFile}\"");
                        if (numOriginal == 0)
                        {
                            extraVOTypeFileCallback(moddedVOFiles[j]);
                            Logger.Info($"Extra file: \"{moddedVOFiles[j]}\"");
                        }
                        progressCallback(dstFile);
                        subtitleHandler.WriteLine(oldKey, newKey);
                    }
                    catch (UnauthorizedAccessException e)
                    {
                        onIOExceptionCallback(moddedVOFiles[j]);
                        Logger.Error($"Failed to copy due to unauthorized access: " +
                            $"{moddedVOFiles[j]}\n{e.Message}");
                    }
                    catch (IOException e)
                    {
                        onIOExceptionCallback(moddedVOFiles[j]);
                        Logger.Error($"Failed to copy: {moddedVOFiles[j]}\n{e.Message}");
                    }
                }
            }
        }

        foreach (string voType in _originalVOManager.GetVOTypes())
        {
            if (!_moddedVOManager.HasVOType(voType))
            {
                missingVOTypes.Add(voType);
            }
        }
    }
}
