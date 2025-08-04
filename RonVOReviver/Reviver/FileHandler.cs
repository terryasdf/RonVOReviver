using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
}
