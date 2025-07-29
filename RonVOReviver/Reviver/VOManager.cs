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

    private string _folderPath = string.Empty;
    private Dictionary<string, List<int>> _voIndicesMap = [];
    public string[] Files = [];
    public int NumVo { get; protected set; }  = 0;

    public static string GetVoType(string file, out int index)
    {
        string[] components = Path.GetFileName(file).Split('_');
        string strIndex = components.Last().Split('.')[0];
        index = int.Parse(strIndex);
        Array.Resize(ref components, components.Length - 1);
        return string.Concat(components);
    }

    public bool ContainsVoType(string voType) => _voIndicesMap.ContainsKey(voType.ToLower());

    public List<int> GetVoIndices(string voType)
    {
        return _voIndicesMap[voType];
    }

    public bool SetFolderPath(string path, Callback progressCallback, Callback onFormatExceptionCallback)
    {
        _folderPath = path;
        _voIndicesMap = [];
        if (!File.Exists(path))
        {
            return false;
        }

        Files = Directory.GetFiles(path, "*.ogg");
        NumVo = Files.Length;
        for (int i = 0; i < NumVo; ++i)
        {
            Files[i] = Files[i].ToLower();
            try
            {
                string voType = GetVoType(Files[i], out int id);

                // Pak contents are not case-sensitive.
                voType = voType.ToLower();
                Logger.Info($"Found .ogg under folder: {voType}, id={id}");

                if (_voIndicesMap[voType] is null)
                {
                    _voIndicesMap[voType] = [];
                }
                _voIndicesMap[voType].Add(id);
                progressCallback(Files[i]);
            }
            catch (FormatException e)
            {
                Logger.Error($"Parsing failed at {Files[i]}: {e.Message}");
                onFormatExceptionCallback(Files[i]);
            }
        }
        Array.Sort(Files);
        return true;
    }
}
