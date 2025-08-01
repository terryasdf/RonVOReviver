using NLog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RonVOReviver.Reviver;

public static class Packer
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    private const string PakExecutable = "ron_pak.bat";
    private const string PakDirectory = "paking";

    public static void Pack(string pakPath)
    {
        if (!Directory.Exists(pakPath))
        {
            throw new DirectoryNotFoundException($"The folder does not exist:\n{pakPath}");
        }

        ProcessStartInfo processInfo = new(PakExecutable, pakPath)
        {
            UseShellExecute = false,
            WorkingDirectory = PakDirectory,
        };

        Process? p = Process.Start(processInfo);
        p?.WaitForExit();
    }
}