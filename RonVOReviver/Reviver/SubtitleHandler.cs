using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RonVOReviver.Reviver;

public class SubtitleHandler : IDisposable
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
    // TODO: Check header content
    private static readonly string HeaderContent = "Key,Content,";

    private readonly Dictionary<string, Dictionary<string, string>> _subtitles = [];

    private Dictionary<string, StreamWriter> _writers = [];

    /// <summary>
    /// Dummy constructor.
    /// </summary>
    public SubtitleHandler() { }

    public void Dispose()
    {
        foreach (StreamWriter writer in _writers.Values)
        {
            writer.Dispose();
        }
        GC.SuppressFinalize(this);
    }

    public SubtitleHandler(string oldSubtitleFolderPath, string outputFolderPath,
        Callback onIOExceptionCallback)
    {
        string[] files = Directory.GetFiles(oldSubtitleFolderPath, "sub_*.csv");
        foreach (string file in files)
        {
            string fileName = Path.GetFileName(file).ToLower();
            try
            {
                using StreamReader sr = new(file);
                // Skip the header row
                sr.ReadLine();
                Dictionary<string, string> dict = [];
                string? row;
                while ((row = sr.ReadLine()) is not null)
                {
                    string[] cells = row.Trim().Split(',');
                    if (cells.Length < 2)
                    {
                        continue;
                    }
                    dict[cells[0]] = cells[1];
                }
                _writers[fileName] = new StreamWriter($"{outputFolderPath}\\{fileName}");
                _writers[fileName].Write(HeaderContent);
                _subtitles[fileName] = dict;
                Logger.Debug($"Read subtitle file: {file}");
            }
            catch (UnauthorizedAccessException e)
            {
                onIOExceptionCallback(file);
                Logger.Error($"Unauthorized access to write new subtitle file: {
                    outputFolderPath}\\{fileName}\n{e.Message}");
            }
            catch (IOException e)
            {
                onIOExceptionCallback(file);
                Logger.Error($"Failed to read subtitle file: {file}\n{e.Message}");
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="oldKey"></param>
    /// <param name="newKey"></param>
    public void WriteLine(string oldKey, string newKey)
    {
        foreach ((string fileName, StreamWriter writer) in _writers)
        {
            if (!_subtitles[fileName].TryGetValue(oldKey, out string? content))
            {
                continue;
            }
            string line = $"{newKey},{content},";
            writer.WriteLine(line);
        }
    }
}
