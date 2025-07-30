using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RonVOReviver.Reviver;

public class SubtitleHandler
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    private readonly Dictionary<string, Dictionary<string, string>> _subtitles = [];

    /// <summary>
    /// Dummy constructor.
    /// </summary>
    public SubtitleHandler() { }

    public SubtitleHandler(string folderPath, Callback onIOExceptionCallback)
    {
        string[] files = Directory.GetFiles(folderPath, "sub_*.csv");
        foreach (string file in files)
        {
            try
            {
                using StreamReader sr = new(file);
                // Skip the first row, which stores the indices
                sr.ReadLine();
                Dictionary<string, string> dict = [];
                while (sr.Peek() >= 0)
                {
                    string row = sr.ReadLine()!;
                    string[] cells = row.Split(',');
                    dict[cells[0]] = cells[1];
                }
                _subtitles[Path.GetFileName(file).ToLower()] = dict;
                Logger.Debug($"Read subtitle file: {file}");
            }
            catch (IOException e)
            {
                onIOExceptionCallback(file);
                Logger.Error($"Failed to read subtitle file: {file}\n{e.Message}");
            }
        }
    }
}
