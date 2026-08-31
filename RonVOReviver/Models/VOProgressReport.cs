namespace RonVOReviver.Models;

public enum VOProgressType
{
    FileCopied,
    ExtraVOType,
    MissingVOType,
    Error
}

public readonly record struct VOProgressReport(string Path, VOProgressType Type = VOProgressType.FileCopied);
