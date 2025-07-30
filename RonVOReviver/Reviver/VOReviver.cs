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
    private static readonly string InPakVOPath = "Content\\VO_PC";

    private VOManager _originalVOManager = new();
    private VOManager _moddedVOManager = new();
    private SubtitleHandler _subtitleHandler = new();

    private string _destinationFolderPath = string.Empty;

    public int ZeroFillLength { get; set; } = 1;
    public string Character { get; protected set; } = string.Empty;

    public bool SetOriginalVOFolderPath(string path, Callback progressCallback,
        Callback onFormatExceptionCallback)
    {
        try
        {
            _originalVOManager = new(path, progressCallback, onFormatExceptionCallback);
            Character = Path.GetFileName(path);
        }
        catch (UnauthorizedAccessException e)
        {
            Logger.Error($"Failed to read folder due to unauthorized access: " +
                $"{path}\n{e.Message}");
            return false;
        }
        catch (IOException e)
        {
            Logger.Error($"Failed to read folder: {path}\n{e.Message}");
            return false;
        }
        return true;
    }

    public bool SetModdedVOFolderPath(string path, Callback progressCallback,
        Callback onFormatExceptionCallback, Callback onSubtitleIOExceptionCallback)
    {
        try
        {
            _moddedVOManager = new(path, progressCallback, onFormatExceptionCallback);
        }
        catch (UnauthorizedAccessException e)
        {
            Logger.Error($"Failed to read folder due to unauthorized access: " +
                $"{path}\n{e.Message}");
            return false;
        }
        catch (IOException e)
        {
            Logger.Error($"Failed to read folder: {path}\n{e.Message}");
            return false;
        }
        _subtitleHandler = new(path, onSubtitleIOExceptionCallback);
        return true;
    }

    public void SetDestionationFolderPath(string path) => _destinationFolderPath = path;

    public bool CopyVOFiles(out List<string> missingVOTypes,
        Callback extraVOTypeFileCallback, Callback progressCallback,
        Callback onIOExceptionCallback)
    {
        missingVOTypes = [];

        // Clear destination directory
        string newVOFolderPath = $"{_destinationFolderPath}\\{InPakVOPath}\\{Character}";
        try
        {
            if (Directory.Exists(_destinationFolderPath))
            {
                Directory.Delete(_destinationFolderPath, true);
                Logger.Debug($"Deleted folder: {_destinationFolderPath}");
            }
            Directory.CreateDirectory(newVOFolderPath);
        }
        catch (UnauthorizedAccessException e)
        {
            Logger.Error($"Unauthorized access to folder: {_destinationFolderPath}\n{e.Message}");
            return false;
        }
        catch (IOException e)
        {
            Logger.Error($"Failed to clear folder: {_destinationFolderPath}\n{e.Message}");
            return false;
        }

        int nextTypeCur = 0;
        int numModdedVO = _moddedVOManager.Files.Count;
        string[] moddedVOFiles = [.. _moddedVOManager.Files];

        for (int i = 0; i < numModdedVO; i = nextTypeCur)
        {
            // Find all files of one voType
            string voType = VOManager.GetVoType(moddedVOFiles[i], out int _);
            while (nextTypeCur < numModdedVO &&
                VOManager.GetVoType(moddedVOFiles[nextTypeCur], out int _).Equals(voType))
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
                    string dstFile = $"{newVOFolderPath}\\{voType}_{index++.ToString(
                        $"D{ZeroFillLength}")}.ogg";
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

        return true;
    }
}
