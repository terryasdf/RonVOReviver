using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RonVOReviver.Reviver;

public class SubtitleHandler(string originalFilePath, string voFolderPath)
{
    private string _originalFilePath = originalFilePath;
    private string _voFolderPath = voFolderPath;

    public bool GenerateSubtitle()
    {
        return false;
    }
}
