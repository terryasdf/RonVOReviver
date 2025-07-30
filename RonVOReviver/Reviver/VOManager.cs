using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RonVOReviver.Reviver;

public delegate void Callback(string path);

public class VOManager
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    private readonly Dictionary<string, List<int>> _voIndicesMap = [];
    public List<string> Files { get; } = [];

    /// <summary>
    /// Dummy constructor.
    /// </summary>
    public VOManager() { }


    public static string GetVoType(string file, out int index)
    {
        string[] components = Path.GetFileName(file).Split('_');
        string strIndex = components.Last().Split('.')[0];
        index = int.Parse(strIndex);
        Array.Resize(ref components, components.Length - 1);
        return string.Concat(components);
    }

    public Dictionary<string, List<int>>.KeyCollection GetVOTypes() => _voIndicesMap.Keys;

    public bool HasVOType(string voType) => _voIndicesMap.ContainsKey(voType);

    public int GetMaxIndex(string voType)
    {
        return _voIndicesMap.TryGetValue(voType, out List<int>? indices) ? indices.Max() : 0;
    }

    public int GetCount(string voType)
    {
        return _voIndicesMap.TryGetValue(voType, out List<int>? indices) ? indices.Count : 0;
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
        _voIndicesMap = [];
        if (!Directory.Exists(path))
        {
            return;
        }

        string[] filesArray = Directory.GetFiles(path, "*.ogg");
        for (int i = 0; i < filesArray.Length; ++i)
        {
            // Pak contents are not case-sensitive.
            filesArray[i] = filesArray[i].ToLower();
            try
            {
                string voType = GetVoType(filesArray[i], out int id);
                Logger.Debug($"Found .ogg under folder: {voType}, id={id}");

                if (!_voIndicesMap.TryGetValue(voType, out List<int>? indices))
                {
                    indices = [];
                    _voIndicesMap[voType] = indices;
                }
                Files.Add(filesArray[i]);
                indices.Add(id);
                progressCallback(filesArray[i]);
            }
            catch (FormatException e)
            {
                Logger.Error($"Parsing failed at {filesArray[i]}: {e.Message}");
                onFormatExceptionCallback(filesArray[i]);
            }
        }
        Files.Sort();
    }
}
