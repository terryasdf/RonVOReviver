using NLog;
using RonVOReviver.Models;
using RonVOReviver.Services;
using System.IO;

namespace RonVOReviver.Core;

public class VOReviver(
    VOManager originalVOManager,
    ModdedVOManager moddedVOManager,
    string pakFolderPath,
    string character)
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
    private static readonly string BlankOggPath = Path.Combine(AppContext.BaseDirectory, "Assets", "Audio", "blank.ogg");
    private static readonly string InPakVOPath = Path.Combine("Content", "VO_PC");

    public async Task PakVOFilesAsync() => await Packer.PackAsync(pakFolderPath);

    private static async Task CopyVOFileAsync(string moddedFile, string originalFile, string dstFolder,
        IProgress<VOProgressReport>? progress = null,
        SubtitleHandler? subtitleHandler = null,
        CancellationToken cancellationToken = default)
    {
        string oldKey = Path.GetFileNameWithoutExtension(moddedFile);
        string newKey = Path.GetFileNameWithoutExtension(originalFile);
        string dstFile = Path.Combine(dstFolder, Path.GetFileName(originalFile));
        try
        {
            await FileHandler.CopyAsync(moddedFile, dstFile, cancellationToken);
            progress?.Report(new VOProgressReport(dstFile, VOProgressType.FileCopied));
            subtitleHandler?.WriteLine(oldKey, newKey);
        }
        catch (UnauthorizedAccessException e)
        {
            progress?.Report(new VOProgressReport(moddedFile, VOProgressType.Error));
            Logger.Error($"Failed to copy due to unauthorized access: " +
                $"{moddedFile}\n{e.Message}");
        }
        catch (IOException e)
        {
            progress?.Report(new VOProgressReport(moddedFile, VOProgressType.Error));
            Logger.Error($"Failed to copy: {moddedFile}\n{e.Message}");
        }
    }

    public async Task CopyVOFilesAsync(IProgress<VOProgressReport>? progress = null,
        CancellationToken ct = default)
    {
        // Clear destination directory
        string newVOFolderPath = Path.Combine(pakFolderPath, InPakVOPath, character);
        string tempFolderPath = Path.Combine(pakFolderPath, "temp");
        FileHandler.ClearDirectory(pakFolderPath);
        Directory.CreateDirectory(newVOFolderPath);

        int numModdedVO = moddedVOManager.Files.Count;
        var moddedVOFiles = moddedVOManager.Files;

        // Convert audio format and save to a temp folder if necessary
        if (!moddedVOManager.IsOgg)
        {
            moddedVOFiles = await FileHandler.ConvertVOFilesAsync(moddedVOManager.Files, tempFolderPath);
            numModdedVO = moddedVOFiles.Count;
        }

        int nextTypeCur = 0;
        await using SubtitleHandler subtitleHandler = await SubtitleHandler.CreateAsync(
            moddedVOManager.FolderPath, newVOFolderPath, progress, ct);

        for (int i = 0; i < numModdedVO; i = nextTypeCur)
        {
            ct.ThrowIfCancellationRequested();
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

            if (!hasOriginal)
            {
                progress?.Report(
                    new VOProgressReport(Path.GetFileNameWithoutExtension(moddedVOFiles[j]),
                    VOProgressType.ExtraVOType));
                for (; j < nextTypeCur; ++j)
                {
                    Logger.Info($"Extra file: \"{moddedVOFiles[j]}\"");
                    await CopyVOFileAsync(moddedVOFiles[j], moddedVOFiles[j],
                        newVOFolderPath, progress, subtitleHandler, ct);
                }
                continue;
            }

            foreach (string originalFile in originalFiles)
            {
                ct.ThrowIfCancellationRequested();
                await CopyVOFileAsync(moddedVOFiles[j], originalFile, newVOFolderPath,
                    progress, subtitleHandler, ct);
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

            progress?.Report(new VOProgressReport(Path.GetFileName(voType), VOProgressType.MissingVOType));

            var originalFiles = originalVOManager.GetFiles(voType);
            foreach (string originalFile in originalFiles)
            {
                ct.ThrowIfCancellationRequested();
                await CopyVOFileAsync(BlankOggPath, originalFile, newVOFolderPath, progress, cancellationToken: ct);
            }
        }

        FileHandler.ClearDirectory(tempFolderPath);
    }
}
