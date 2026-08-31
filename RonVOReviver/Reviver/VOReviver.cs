using NLog;
using System.IO;

namespace RonVOReviver.Reviver;

public class VOReviver(
    VOManager originalVOManager,
    ModdedVOManager moddedVOManager,
    string destinationFolderPath,
    string character)
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
    private static readonly string BlankOggPath = ".\\blank.ogg";
    private static readonly string InPakVOPath = "Content\\VO_PC";

    public async Task PakVOFilesAsync() => await Packer.PackAsync(destinationFolderPath);

    public async Task CopyVOFilesAsync(IProgress<VOProgressReport>? progress = null,
        CancellationToken cancellationToken = default)
    {
        // Clear destination directory
        string newVOFolderPath = $"{destinationFolderPath}\\{InPakVOPath}\\{character}";
        string tempFolderPath = $"{destinationFolderPath}\\temp";
        FileHandler.ClearDirectory(destinationFolderPath);
        Directory.CreateDirectory(newVOFolderPath);

        int numModdedVO = moddedVOManager.Files.Count;
        var moddedVOFiles = moddedVOManager.Files;

        // Convert audio format and save to a temp folder if necessary
        if (!moddedVOManager.IsOgg)
        {
            moddedVOFiles = await FileHandler.ConvertVOFilesAsync(moddedVOManager.Files, tempFolderPath);
        }

        int nextTypeCur = 0;
        await using SubtitleHandler subtitleHandler = await SubtitleHandler.CreateAsync(
            moddedVOManager.FolderPath,
            newVOFolderPath,
            progress,
            cancellationToken);

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

            var originalFiles = originalVOManager.GetFiles(voType);
            bool hasOriginal = originalFiles.Count > 0;

            int j = i;
            string oldKey = Path.GetFileNameWithoutExtension(moddedVOFiles[j]);

            if (!hasOriginal)
            {
                progress?.Report(new VOProgressReport(oldKey, VOProgressType.ExtraVOType));
                while (j < nextTypeCur)
                {
                    Logger.Info($"Extra file: \"{moddedVOFiles[j++]}\"");
                }
                continue;
            }

            foreach (string originalFile in originalFiles)
            {
                cancellationToken.ThrowIfCancellationRequested();
                
                string newKey = Path.GetFileNameWithoutExtension(originalFile);
                string dstFile = $"{newVOFolderPath}\\{Path.GetFileName(originalFile)}";
                try
                {
                    await FileHandler.CopyAsync(moddedVOFiles[j], dstFile, cancellationToken);
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
            if (moddedVOManager.HasVOType(voType))
            {
                continue;
            }

            var originalFiles = originalVOManager.GetFiles(voType);
            foreach (string originalFile in originalFiles)
            {
                cancellationToken.ThrowIfCancellationRequested();
                string dstFile = $"{newVOFolderPath}\\{Path.GetFileName(originalFile)}";
                progress?.Report(new VOProgressReport(Path.GetFileName(originalFile), VOProgressType.MissingVOType));
                try
                {
                    await FileHandler.CopyAsync(BlankOggPath, dstFile, cancellationToken);
                }
                catch (UnauthorizedAccessException e)
                {
                    progress?.Report(new VOProgressReport(BlankOggPath, VOProgressType.Error));
                    Logger.Error($"Failed to copy due to unauthorized access: " +
                        $"{BlankOggPath}\n{e.Message}");
                }
                catch (IOException e)
                {
                    progress?.Report(new VOProgressReport(BlankOggPath, VOProgressType.Error));
                    Logger.Error($"Failed to copy: {BlankOggPath}\n{e.Message}");
                }
            }
        }

        FileHandler.ClearDirectory(tempFolderPath);
    }
}
