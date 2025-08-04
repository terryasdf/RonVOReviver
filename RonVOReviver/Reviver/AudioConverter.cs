using FFMpegCore;
using NLog;

namespace RonVOReviver.Reviver;

public class AudioConverter
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    public static void ConvertToOgg(string srcFile, string dstFile)
    {
        Logger.Info($"Converting {srcFile} to {dstFile}");
        FFMpegArguments.FromFileInput(srcFile).OutputToFile(dstFile).ProcessSynchronously();
    }
}
