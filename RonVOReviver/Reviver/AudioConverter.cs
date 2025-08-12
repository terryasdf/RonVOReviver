using FFMpegCore;
using NLog;

namespace RonVOReviver.Reviver;

public class AudioConverter
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    public static async Task ConvertToOggAsync(string srcFile, string dstFile)
    {
        Logger.Debug($"Converting {srcFile} to {dstFile}");
        if (await FFMpegArguments.FromFileInput(srcFile).OutputToFile(dstFile).ProcessAsynchronously())
            return;
        Logger.Error($"Failed to convert {srcFile} to {dstFile}");
    }
}
