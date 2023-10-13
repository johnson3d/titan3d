namespace Jither.OpenEXR;

[Flags]
public enum EXRVersionFlags
{
    None = 0,

    IsSinglePartTiled = 0x2,
    LongNames = 0x4,
    NonImageParts = 0x8,
    MultiPart = 0x10
}
