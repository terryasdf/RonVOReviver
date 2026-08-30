using NLog;
using System.IO;

namespace RonVOReviver.Reviver;

public delegate void Callback(string path);

public class VOManager
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    private readonly Dictionary<string, List<string>> _voFilesMap = [];
    private readonly List<string> _files = [];

    public string FolderPath { get; protected set; } = string.Empty;
    public IReadOnlyList<string> Files { get => _files; }
    public int ZeroFillLength { get; set; } = 4;

    /// <summary>
    /// Dummy constructor.
    /// </summary>
    public VOManager() { }

    public static string GetVOType(string file)
    {
        string[] components = Path.GetFileName(file).Split('_');
        Array.Resize(ref components, components.Length - 1);
        return string.Join("_", components);
    }

    public static string GetVOType(string file, out string index)
    {
        string[] components = Path.GetFileName(file).Split('_');
        index = components.Last().Split('.')[0];
        Array.Resize(ref components, components.Length - 1);
        return string.Join("_", components);
    }

    public Dictionary<string, List<string>>.KeyCollection GetVOTypes() => _voFilesMap.Keys;

    public bool HasVOType(string voType) => _voFilesMap.ContainsKey(voType);

    public IReadOnlyList<string> GetFiles(string voType)
    {
        return _voFilesMap.TryGetValue(voType, out List<string>? files) ? files : [];
    }

    public virtual string[] GetVOFiles()
    {
        return Directory.GetFiles(FolderPath, "*.ogg");
    }

    /// <summary>
    /// Reads from <paramref name="path"/> and counts types of VO files.
    /// </summary>
    /// <param name="path">The character folder (e.g. SWATJudge)</param>
    /// <param name="progressCallback">
    /// Called upon reading each successful VO file
    /// </param>
    /// <param name="onFormatExceptionCallback">
    /// Called upon unintended naming format (should be like XXXX_1.ogg)
    /// </param>
    public VOManager(string path, Callback progressCallback, Callback onFormatExceptionCallback)
    {
        FolderPath = path;
        string[] filesArray = GetVOFiles();
        _voFilesMap = [];
        for (int i = 0; i < filesArray.Length; ++i)
        {
            // Pak contents are not case-sensitive.
            filesArray[i] = filesArray[i].ToLower();
            string voType = GetVOType(filesArray[i], out string index);
            Logger.Debug($"Found VO under folder: {voType}, file={filesArray[i]}");

            if (!_voFilesMap.TryGetValue(voType, out List<string>? files))
            {
                files = [];
            }

            try
            {
                files.Add(filesArray[i]);
                _files.Add(filesArray[i]);
                _voFilesMap[voType] = files;
                if (ZeroFillLength > index.Length)
                {
                    ZeroFillLength = index.Length;
                }
                progressCallback(filesArray[i]);
            }
            catch (FormatException e)
            {
                Logger.Error($"Parsing failed at {filesArray[i]}: {e.Message}");
                onFormatExceptionCallback(filesArray[i]);
            }
        }
        _files.Sort();
    }
}
