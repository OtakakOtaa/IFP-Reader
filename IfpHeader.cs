namespace IFP_reader;


public struct IfpHeader
{
    /// <summary> constant first file value </summary>
    public const string FileMagicDefinition = "ANP3";
    public const int FileMagicLength = 4;
    public const int StringConstantLength = 24;
    public const int FileDefinitionOffset = 8;
    public const float RotationDivisor = 4096;
    public const float TranslationDivisor = 1024;

    public string FileName { init; get; }
    public int HeaderOffset { init; get; }
    public int TailOffset { init; get; }
    public int AnimsCount { init; get; }
}