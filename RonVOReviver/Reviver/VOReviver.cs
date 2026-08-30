using NLog;
using System.Diagnostics;
using System.IO;

namespace RonVOReviver.Reviver;

public class VOReviver
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
    private static readonly string InPakVOPath = "Content\\VO_PC";

    private VOManager _originalVOManager = new();
    private ModdedVOManager _moddedVOManager = new();

    private string _destinationFolderPath = string.Empty;

    public int ZeroFillLength { get; set; } = 1;
    public string Character { get; set; } = string.Empty;

    public string ZeroFill(int x) => x.ToString($"D{ZeroFillLength}");

    public void SetOriginalVOFolderPath(string path, Callback progressCallback,
        Callback onFormatExceptionCallback)
    {
        _originalVOManager = new(path, progressCallback, onFormatExceptionCallback);
        ZeroFillLength = _originalVOManager.ZeroFillLength;
        Character = Path.GetFileName(path);
    }

    public void SetModdedVOFolderPath(string path, Callback progressCallback,
        Callback onFormatExceptionCallback)
    {
        _moddedVOManager = new(path, progressCallback, onFormatExceptionCallback);
    }

    public void SetDestionationFolderPath(string path) => _destinationFolderPath = path;

    public void PakVOFiles() => Packer.Pack(_destinationFolderPath);

    public async Task CopyVOFiles(Callback extraVOTypeFileCallback, Callback progressCallback,
        Callback onIOExceptionCallback)
    {
        // Clear destination directory
        string newVOFolderPath = $"{_destinationFolderPath}\\{InPakVOPath}\\{Character}";
        string tempFolderPath = $"{_destinationFolderPath}\\temp";
        FileHandler.ClearDirectory(_destinationFolderPath);
        Directory.CreateDirectory(newVOFolderPath);
        
        int numModdedVO = _moddedVOManager.Files.Count;
        IReadOnlyList<string> moddedVOFiles = _moddedVOManager.Files;

        // Convert audio format and save to a temp folder if necessary
        if (!_moddedVOManager.IsOgg)
        {
            moddedVOFiles = await FileHandler.ConvertVOFilesAsync(_moddedVOManager.Files, tempFolderPath);
        }

        int nextTypeCur = 0;
        using SubtitleHandler subtitleHandler = new(_moddedVOManager.FolderPath,
            newVOFolderPath, onIOExceptionCallback);
        for (int i = 0; i < numModdedVO; i = nextTypeCur)
        {
            // Find all files of one voType
            string voType = VOManager.GetVOType(moddedVOFiles[i]);
            while (nextTypeCur < numModdedVO &&
                VOManager.GetVOType(moddedVOFiles[nextTypeCur]).Equals(voType))
            {
                ++nextTypeCur;
            }

            // Copy files for numRepeat times
            IReadOnlyList<string> originalFiles = _originalVOManager.GetFiles(voType);
            bool hasOriginal = originalFiles.Count > 0;

            int j = i;
            foreach (string originalFile in originalFiles)
            {
                string oldKey = Path.GetFileNameWithoutExtension(moddedVOFiles[j]);
                string newKey = Path.GetFileNameWithoutExtension(originalFile);
                string dstFile = $"{newVOFolderPath}\\{Path.GetFileName(originalFile)}";
                try
                {
                    FileHandler.Copy(moddedVOFiles[j], dstFile);
                    if (!hasOriginal)
                    {
                        extraVOTypeFileCallback(moddedVOFiles[j]);
                        Logger.Info($"Extra file: \"{moddedVOFiles[j]}\"");
                    }
                    progressCallback(dstFile);
                    subtitleHandler.WriteLine(oldKey, newKey);
                }
                catch (UnauthorizedAccessException e)
                {
                    onIOExceptionCallback(moddedVOFiles[j]);
                    Logger.Error($"Failed to copy due to unauthorized access: " +
                        $"{moddedVOFiles[j]}\n{e.Message}");
                }
                catch (IOException e)
                {
                    onIOExceptionCallback(moddedVOFiles[j]);
                    Logger.Error($"Failed to copy: {moddedVOFiles[j]}\n{e.Message}");
                }

                if (++j == nextTypeCur)
                {
                    j = i;
                }
            }
        }

        FileHandler.ClearDirectory(tempFolderPath);
    }
}
