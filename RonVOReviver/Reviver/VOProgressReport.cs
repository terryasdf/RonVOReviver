namespace RonVOReviver.Reviver;

public enum VOProgressType
{
    FileCopied,
    ExtraVOType,
    Error
}

public readonly record struct VOProgressReport(string Path, VOProgressType Type = VOProgressType.FileCopied);