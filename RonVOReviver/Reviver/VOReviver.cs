using NLog;
using System.Diagnostics;
using System.IO;

namespace RonVOReviver.Reviver;

public class VOReviver
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
    private static readonly string InPakVOPath = "Content\\VO_PC";

    private VOManager _originalVOManager = new();
    private ModdedVOManager _moddedVOManager = new();

    private string _destinationFolderPath = string.Empty;

    public string Character { get; set; } = string.Empty;

    public void SetOriginalVOFolderPath(string path, IProgress<VOManagerProgressReport>? progress = null)
    {
        _originalVOManager = new(path, progress);
        Character = Path.GetFileName(path);
    }

    public void SetModdedVOFolderPath(string path, IProgress<VOManagerProgressReport>? progress = null)
    {
        _moddedVOManager = new(path, progress);
    }

    public void SetDestionationFolderPath(string path) => _destinationFolderPath = path;

    public async Task PakVOFilesAsync() => await Packer.PackAsync(_destinationFolderPath);

    public async Task CopyVOFiles(IProgress<VOProgressReport>? progress = null,
        CancellationToken cancellationToken = default)
    {
        await Task.Run(async () =>
        {
            // Clear destination directory
            string newVOFolderPath = $"{_destinationFolderPath}\\{InPakVOPath}\\{Character}";
            string tempFolderPath = $"{_destinationFolderPath}\\temp";
            FileHandler.ClearDirectory(_destinationFolderPath);
            Directory.CreateDirectory(newVOFolderPath);
            
            int numModdedVO = _moddedVOManager.Files.Count;
            IReadOnlyList<string> moddedVOFiles = _moddedVOManager.Files;

            // Convert audio format and save to a temp folder if necessary
            if (!_moddedVOManager.IsOgg)
            {
                moddedVOFiles = await FileHandler.ConvertVOFilesAsync(_moddedVOManager.Files, tempFolderPath);
            }

            int nextTypeCur = 0;
            using SubtitleHandler subtitleHandler = new(_moddedVOManager.FolderPath,
                newVOFolderPath, progress);
            for (int i = 0; i < numModdedVO; i = nextTypeCur)
            {
                cancellationToken.ThrowIfCancellationRequested();
                // Find all files of one voType
                string voType = VOManager.GetVOType(moddedVOFiles[i]);
                while (nextTypeCur < numModdedVO &&
                    VOManager.GetVOType(moddedVOFiles[nextTypeCur]).Equals(voType))
                {
                    ++nextTypeCur;
                }

                // Copy files for numRepeat times
                IReadOnlyList<string> originalFiles = _originalVOManager.GetFiles(voType);
                bool hasOriginal = originalFiles.Count > 0;

                int j = i;
                foreach (string originalFile in originalFiles)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    string oldKey = Path.GetFileNameWithoutExtension(moddedVOFiles[j]);
                    string newKey = Path.GetFileNameWithoutExtension(originalFile);
                    string dstFile = $"{newVOFolderPath}\\{Path.GetFileName(originalFile)}";
                    try
                    {
                        FileHandler.Copy(moddedVOFiles[j], dstFile);
                        if (!hasOriginal)
                        {
                            progress?.Report(new VOProgressReport(moddedVOFiles[j], VOProgressType.ExtraVOType));
                            Logger.Info($"Extra file: \"{moddedVOFiles[j]}\"");
                        }
                        progress?.Report(new VOProgressReport(dstFile, VOProgressType.FileCopied));
                        subtitleHandler.WriteLine(oldKey, newKey);
                    }
                    catch (UnauthorizedAccessException e)
                    {
                        progress?.Report(new VOProgressReport(moddedVOFiles[j], VOProgressType.Error));
                        Logger.Error($"Failed to copy due to unauthorized access: " +
                            $"{moddedVOFiles[j]}\n{e.Message}");
                    }
                    catch (IOException e)
                    {
                        progress?.Report(new VOProgressReport(moddedVOFiles[j], VOProgressType.Error));
                        Logger.Error($"Failed to copy: {moddedVOFiles[j]}\n{e.Message}");
                    }

                    if (++j == nextTypeCur)
                    {
                        j = i;
                    }
                }
            }

            FileHandler.ClearDirectory(tempFolderPath);
        }, cancellationToken);
    }
}
