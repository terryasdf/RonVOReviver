using FFMpegCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RonVOReviver.Reviver;

public class AudioConverter
{
    public static void ConvertToOgg(string srcFile, string dstFile)
    {
        FFMpegArguments.FromFileInput(srcFile).OutputToFile(dstFile).ProcessSynchronously();
    }
}
