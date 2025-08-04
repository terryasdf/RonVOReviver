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

    public static void Copy(string srcFile, string dstFile)
    {
        File.Copy(srcFile, dstFile);
        Logger.Debug($"Copied \"{srcFile}\" as \"{dstFile}\"");
    }
}
