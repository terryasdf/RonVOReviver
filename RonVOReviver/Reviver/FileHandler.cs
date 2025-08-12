using NLog;
using System.IO;

namespace RonVOReviver.Reviver;

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

        List<string> dstFiles = [];
        int numFiles = files.Count;
        int i = 0;
        for (; i + 3 < numFiles; i += 4)
        {
            dstFiles.Add($"{dstFolder}\\{Path.GetFileNameWithoutExtension(files[i])}.ogg");
            dstFiles.Add($"{dstFolder}\\{Path.GetFileNameWithoutExtension(files[i + 1])}.ogg");
            dstFiles.Add($"{dstFolder}\\{Path.GetFileNameWithoutExtension(files[i + 2])}.ogg");
            dstFiles.Add($"{dstFolder}\\{Path.GetFileNameWithoutExtension(files[i + 3])}.ogg");
            Task t1 = AudioConverter.ConvertToOggAsync(files[i], dstFiles[i]);
            Task t2 = AudioConverter.ConvertToOggAsync(files[i + 1], dstFiles[i + 1]);
            Task t3 = AudioConverter.ConvertToOggAsync(files[i + 2], dstFiles[i + 2]);
            Task t4 = AudioConverter.ConvertToOggAsync(files[i + 3], dstFiles[i + 3]);
            await Task.WhenAll(t1, t2, t3, t4);
        }

        List<Task> tasks = [];
        if (i < numFiles)
        {
            dstFiles.Add($"{dstFolder}\\{Path.GetFileNameWithoutExtension(files[i])}.ogg");
            tasks.Add(AudioConverter.ConvertToOggAsync(files[i], dstFiles.Last()));
        }
        if (++i < numFiles)
        {
            dstFiles.Add($"{dstFolder}\\{Path.GetFileNameWithoutExtension(files[i])}.ogg");
            tasks.Add(AudioConverter.ConvertToOggAsync(files[i], dstFiles.Last()));
        }
        if (++i < numFiles)
        {
            dstFiles.Add($"{dstFolder}\\{Path.GetFileNameWithoutExtension(files[i])}.ogg");
            tasks.Add(AudioConverter.ConvertToOggAsync(files[i], dstFiles.Last()));
        }
        await Task.WhenAll(tasks);

        return dstFiles;
    }

    public static void Copy(string srcFile, string dstFile)
    {
        File.Copy(srcFile, dstFile);
        Logger.Debug($"Copied \"{srcFile}\" as \"{dstFile}\"");
    }
}
