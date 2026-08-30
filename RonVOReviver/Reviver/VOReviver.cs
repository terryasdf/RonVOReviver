using NLog;
using System.Diagnostics;
using System.IO;

namespace RonVOReviver.Reviver;

public class VOReviver(
    VOManager originalVOManager,
    ModdedVOManager moddedVOManager,
    string destinationFolderPath,
    string character)
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
    private static readonly string InPakVOPath = "Content\\VO_PC";

    public async Task PakVOFilesAsync() => await Packer.PackAsync(destinationFolderPath);

    public async Task CopyVOFiles(IProgress<VOProgressReport>? progress = null,
        CancellationToken cancellationToken = default)
    {
        await Task.Run(async () =>
        {
            // Clear destination directory
            string newVOFolderPath = $"{destinationFolderPath}\\{InPakVOPath}\\{character}";
            string tempFolderPath = $"{destinationFolderPath}\\temp";
            FileHandler.ClearDirectory(destinationFolderPath);
            Directory.CreateDirectory(newVOFolderPath);
            
            int numModdedVO = moddedVOManager.Files.Count;
            IReadOnlyList<string> moddedVOFiles = moddedVOManager.Files;

            // Convert audio format and save to a temp folder if necessary
            if (!moddedVOManager.IsOgg)
            {
                moddedVOFiles = await FileHandler.ConvertVOFilesAsync(moddedVOManager.Files, tempFolderPath);
            }

            int nextTypeCur = 0;
            using SubtitleHandler subtitleHandler = new(moddedVOManager.FolderPath,
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

                IReadOnlyList<string> originalFiles = originalVOManager.GetFiles(voType);
                bool hasOriginal = originalFiles.Count > 0;

                int j = i;

                if (!hasOriginal)
                {
                    while (j < nextTypeCur)
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        string dstFile = $"{newVOFolderPath}\\{Path.GetFileName(moddedVOFiles[j])}";

                        progress?.Report(new VOProgressReport(moddedVOFiles[j], VOProgressType.ExtraVOType));
                        Logger.Info($"Extra file: \"{moddedVOFiles[j++]}\"");
                    }
                    continue;
                }

                foreach (string originalFile in originalFiles)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    string oldKey = Path.GetFileNameWithoutExtension(moddedVOFiles[j]);
                    string newKey = Path.GetFileNameWithoutExtension(originalFile);
                    string dstFile = $"{newVOFolderPath}\\{Path.GetFileName(originalFile)}";
                    try
                    {
                        FileHandler.Copy(moddedVOFiles[j], dstFile);
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

            foreach (string voType in originalVOManager.GetVOTypes())
            {
                if (!moddedVOManager.HasVOType(voType))
                {
                    progress?.Report(new VOProgressReport(voType, VOProgressType.MissingVOType));
                }
            }

            FileHandler.ClearDirectory(tempFolderPath);
        }, cancellationToken);
    }
}
