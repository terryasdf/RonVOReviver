using NLog;
using System.IO;

namespace RonVOReviver.Reviver;

public class VOManager
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    private readonly Dictionary<string, List<string>> _voFilesMap = [];
    private readonly List<string> _files = [];

    public string FolderPath { get; protected set; } = string.Empty;
    public IReadOnlyList<string> Files { get => _files; }

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
    /// <param name="progress">
    /// Called upon reading each VO file (reporting success or format error)
    /// </param>
    public VOManager(string path, IProgress<VOManagerProgressReport>? progress = null)
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
                progress?.Report(new VOManagerProgressReport(filesArray[i], VOManagerProgressType.Success));
            }
            catch (FormatException e)
            {
                Logger.Error($"Parsing failed at {filesArray[i]}: {e.Message}");
                progress?.Report(new VOManagerProgressReport(filesArray[i], VOManagerProgressType.FormatError));
            }
        }
        _files.Sort();
    }
}
