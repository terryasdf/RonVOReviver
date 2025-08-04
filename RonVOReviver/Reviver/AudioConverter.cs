using FFMpegCore;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
