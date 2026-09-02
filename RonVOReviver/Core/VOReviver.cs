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
        CancellationToken ct = default)
    {
        string oldKey = Path.GetFileNameWithoutExtension(moddedFile);
        string newKey = Path.GetFileNameWithoutExtension(originalFile);
        string dstFile = Path.Combine(dstFolder, Path.GetFileName(originalFile));
        try
        {
            await FileHandler.CopyAsync(moddedFile, dstFile, ct);
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

    private static async Task CopyByRangeAsync(IReadOnlyList<string> moddedFiles,
        IReadOnlyList<string> originalFiles,
        string dstFolder, int start, int end,
        IProgress<VOProgressReport>? progress = null,
        SubtitleHandler? subtitleHandler = null,
        CancellationToken ct = default)
    {
        int i = start;
        bool notFirstRun = false;
        foreach (string originalFile in originalFiles)
        {
            ct.ThrowIfCancellationRequested();
            await CopyVOFileAsync(moddedFiles[i], originalFile, dstFolder,
                progress, subtitleHandler, ct);
            if (++i == end)
            {
                i = start;
                notFirstRun = true;
            }
        }

        if (notFirstRun)
        {
            return;
        }

        // If there are more modded files than original, then keep the remaining.
        for (; i < end; ++i)
        {
            ct.ThrowIfCancellationRequested();
            await CopyVOFileAsync(moddedFiles[i], moddedFiles[i], dstFolder,
                progress, subtitleHandler, ct);
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


            if (originalVOManager.HasVOType(voType))
            {
                var originalFiles = originalVOManager.GetFiles(voType);
                await CopyByRangeAsync(moddedVOFiles, originalFiles, newVOFolderPath,
                    i, nextTypeCur, progress, subtitleHandler, ct);
                continue;
            }

            progress?.Report(
                new VOProgressReport(Path.GetFileNameWithoutExtension(voType),
                    VOProgressType.ExtraVOType));
            for (int j = i; j < nextTypeCur; ++j)
            {
                Logger.Info($"Extra file: \"{moddedVOFiles[j]}\"");
                await CopyVOFileAsync(moddedVOFiles[j], moddedVOFiles[j],
                    newVOFolderPath, progress, subtitleHandler, ct);
            }
        }

        foreach (string voType in originalVOManager.GetVOTypes())
        {
            if (moddedVOManager.HasVOType(voType))
            {
                continue;
            }

            var originalFiles = originalVOManager.GetFiles(voType);

            /* For SWAT characters most VO types have an "S" (silent) variant.
             * If modded VO does not have the "S" variant but has the non-"S" variant,
             * we replace the "S" variant with non-"S" variant instead of blank audio.
             */
            if (voType.EndsWith('s') && moddedVOManager.HasVOType(voType[..^1]))
            {
                moddedVOFiles = moddedVOManager.GetFiles(voType[..^1]);
                await CopyByRangeAsync(moddedVOFiles, originalFiles, newVOFolderPath,
                    0, moddedVOFiles.Count, progress, subtitleHandler, ct);
                continue;
            }

            // Replace with blank audio otherwise.
            progress?.Report(new VOProgressReport(Path.GetFileName(voType), VOProgressType.MissingVOType));
            foreach (string originalFile in originalFiles)
            {
                ct.ThrowIfCancellationRequested();
                await CopyVOFileAsync(BlankOggPath, originalFile, newVOFolderPath,
                    progress, ct: ct);
            }
        }

        FileHandler.ClearDirectory(tempFolderPath);
    }
}
