namespace RonVOReviver.Models;

public enum VOManagerProgressType
{
    Success,
    FormatError
}

public readonly record struct VOManagerProgressReport(string Path, VOManagerProgressType Type = VOManagerProgressType.Success);
