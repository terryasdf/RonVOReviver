using CsvHelper;
using CsvHelper.Configuration;
using NLog;
using System.Globalization;
using System.IO;

namespace RonVOReviver.Reviver;

public class SubtitleHandler : IDisposable, IAsyncDisposable
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    private readonly Dictionary<string, Dictionary<string, string>> _subtitles = [];
    private readonly Dictionary<string, CsvWriter> _writers = [];

    public struct Record
    {
        public string Key { get; set; }
        public string Dialogue { get; set; }
        public string Context { get; set; }
    }

    private SubtitleHandler() { }

    public void Dispose()
    {
        foreach (CsvWriter writer in _writers.Values)
        {
            writer.Flush();
            writer.Dispose();
        }
        _writers.Clear();
        GC.SuppressFinalize(this);
    }

    public async ValueTask DisposeAsync()
    {
        foreach (CsvWriter writer in _writers.Values)
        {
            await writer.FlushAsync();
            await writer.DisposeAsync();
        }
        _writers.Clear();
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Loads all old subtitles asynchronously into dictionary and opens a <see cref="CsvWriter"/> for each language.
    /// </summary>
    /// <param name="oldSubtitleFolderPath">The folder path for old files</param>
    /// <param name="outputFolderPath">The folder path for generated files</param>
    /// <param name="progress">Progress report callback</param>
    /// <param name="cancellationToken">Cancellation token</param>
    public static async Task<SubtitleHandler> CreateAsync(
        string oldSubtitleFolderPath,
        string outputFolderPath,
        IProgress<VOProgressReport>? progress = null,
        CancellationToken cancellationToken = default)
    {
        SubtitleHandler handler = new();
        string[] files = Directory.GetFiles(oldSubtitleFolderPath, "sub_*.csv");
        foreach (string file in files)
        {
            cancellationToken.ThrowIfCancellationRequested();
            string fileName = Path.GetFileName(file).ToLower();
            try
            {
                using StreamReader sr = new(file);
                bool hasBadData = false;

                CsvConfiguration csvConfig = new(CultureInfo.InvariantCulture)
                {
                    BadDataFound = args =>
                    {
                        hasBadData = true;
                        Logger.Error($"Skipping bad row in subtitle {file}: {args.RawRecord}");
                    },
                    MissingFieldFound = null
                };
                using CsvReader csvReader = new(sr, csvConfig);

                // Skip the header row
                await csvReader.ReadAsync();
                csvReader.ReadHeader();
                Dictionary<string, string> dict = [];
                while (await csvReader.ReadAsync())
                {
                    try
                    {
                        hasBadData = false;
                        string? key = csvReader.GetField("Key");
                        string? dialogue = csvReader.GetField("Dialogue");
                        if (hasBadData)
                        {
                            continue;
                        }
                        if (key == null || dialogue == null)
                        {
                            continue;
                        }
                        key = key.ToLower();
                        Logger.Debug($"Read from CSV: key = {key}, dialogue = {dialogue}");
                        dict[key] = dialogue;
                    }
                    catch (CsvHelperException e)
                    {
                        Logger.Error($"Bad data in subtitle {file}: {e.Message}");
                    }
                }

                StreamWriter sw = new($"{outputFolderPath}\\{fileName}");
                CsvWriter csvWriter = new(sw, CultureInfo.InvariantCulture);
                handler._writers[fileName] = csvWriter;
                csvWriter.WriteHeader<Record>();
                csvWriter.NextRecord();
                handler._subtitles[fileName] = dict;

                Logger.Debug($"Read subtitle file: {file}");
            }
            catch (UnauthorizedAccessException e)
            {
                progress?.Report(new VOProgressReport(file, VOProgressType.Error));
                Logger.Error($"Unauthorized access to write new subtitle file: {outputFolderPath}\\{fileName}\n{e.Message}");
            }
            catch (FileFormatException e)
            {
                progress?.Report(new VOProgressReport(file, VOProgressType.Error));
                Logger.Error($"Invalid file format: {file}\n{e.Message}");
            }
            catch (IOException e)
            {
                progress?.Report(new VOProgressReport(file, VOProgressType.Error));
                Logger.Error($"Failed to read subtitle file: {file}\n{e.Message}");
            }
        }
        return handler;
    }

    /// <summary>
    /// Writes a line to new subtitle files for each language.
    /// </summary>
    /// <param name="oldKey">The "Key" attribute of old files</param>
    /// <param name="newKey">The "Key" attribute for the new files</param>
    public void WriteLine(string oldKey, string newKey)
    {
        foreach ((string fileName, CsvWriter writer) in _writers)
        {
            if (!_subtitles[fileName].TryGetValue(oldKey, out string? dialogue))
            {
                continue;
            }
            writer.WriteRecord(new Record { Key = newKey, Dialogue = dialogue! });
            writer.NextRecord();
            Logger.Debug($"Written record to {fileName}: Key = {newKey}, Dialogue = {dialogue}");
        }
    }
}
