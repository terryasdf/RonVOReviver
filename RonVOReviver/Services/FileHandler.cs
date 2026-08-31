using NLog;
using System.IO;

namespace RonVOReviver.Services;

public class FileHandler
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    public static void ClearDirectory(string path)
    {
        if (Directory.Exists(path))
        {
            Directory.Delete(path, true);
            Logger.Debug($"Deleted folder: {path}");
        }
    }

    /// <summary>
    /// Converts all the given <paramref name="files"/> into .ogg and saves to <paramref name="dstFolder"/>
    /// </summary>
    /// <param name="files"></param>
    /// <param name="dstFolder"></param>
    /// <returns></returns>
    public static async Task<List<string>> ConvertVOFilesAsync(IReadOnlyList<string> files, string dstFolder)
    {
        ClearDirectory(dstFolder);
        Directory.CreateDirectory(dstFolder);

        string[] dstFiles = new string[files.Count];
        await Parallel.ForEachAsync(Enumerable.Range(0, files.Count),
            new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount },
            async (i, ct) =>
            {
                string dst = Path.Combine(dstFolder, $"{Path.GetFileNameWithoutExtension(files[i])}.ogg");
                await AudioConverter.ConvertToOggAsync(files[i], dst);
                dstFiles[i] = dst;
            });

        return [.. dstFiles];
    }

    public static async Task CopyAsync(string srcFile, string dstFile, CancellationToken cancellationToken = default)
    {
        using FileStream sourceStream = new(srcFile, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize: 4096, useAsync: true);
        using FileStream destinationStream = new(dstFile, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize: 4096, useAsync: true);
        await sourceStream.CopyToAsync(destinationStream, cancellationToken);
        Logger.Debug($"Copied \"{srcFile}\" as \"{dstFile}\"");
    }
}
