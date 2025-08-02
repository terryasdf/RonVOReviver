using CsvHelper;
using NLog;
using System.Globalization;
using System.IO;

namespace RonVOReviver.Reviver;

public class SubtitleHandler : IDisposable
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    private readonly Dictionary<string, Dictionary<string, string>> _subtitles = [];
    private Dictionary<string, CsvWriter> _writers = [];

    public class Record
    {
        public required string Key { get; set; }
        public required string Dialogue { get; set; }
        public required string Context { get; set; }
    }

    /// <summary>
    /// Dummy constructor.
    /// </summary>
    public SubtitleHandler() { }

    public void Dispose()
    {
        foreach (CsvWriter writer in _writers.Values)
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
                using CsvReader csvReader = new(sr, CultureInfo.InvariantCulture);

                // Skip the header row
                csvReader.Read();
                csvReader.ReadHeader();
                Dictionary<string, string> dict = [];
                while (csvReader.Read())
                {
                    string? key = csvReader.GetField("Key");
                    string? dialogue = csvReader.GetField("Dialogue");
                    if (key == null || dialogue == null)
                    {
                        throw new FileFormatException($"Invalid csv format in {file}");
                    }
                    key = key.ToLower();
                    Logger.Debug($"Read from CSV: key = {key}, dialogue = {dialogue}");
                    dict[key] = dialogue;
                }

                StreamWriter sw = new($"{outputFolderPath}\\{fileName}");
                CsvWriter csvWriter = new(sw, CultureInfo.InvariantCulture);
                _writers[fileName] = csvWriter;
                csvWriter.WriteHeader<Record>();
                csvWriter.NextRecord();
                _subtitles[fileName] = dict;

                Logger.Debug($"Read subtitle file: {file}");
            }
            catch (UnauthorizedAccessException e)
            {
                onIOExceptionCallback(file);
                Logger.Error($"Unauthorized access to write new subtitle file: {outputFolderPath}\\{fileName}\n{e.Message}");
            }
            catch (FileFormatException e)
            {
                onIOExceptionCallback(file);
                Logger.Error($"Invalid file format: {file}\n{e.Message}");
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
        foreach ((string fileName, CsvWriter writer) in _writers)
        {
            if (!_subtitles[fileName].TryGetValue(oldKey, out string? dialogue))
            {
                continue;
            }
            writer.WriteRecord(new Record { Key = newKey, Dialogue = dialogue!, Context = string.Empty });
            writer.NextRecord();
            Logger.Debug($"Written record to {fileName}: Key = {newKey}, Dialogue = {dialogue}");
        }
    }
}
