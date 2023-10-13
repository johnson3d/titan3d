namespace Jither.OpenEXR;

public enum EXRDataType
{
    /// <summary>
    /// 32-bit unsigned integers; for discrete per-pixel data such as object identifiers.
    /// </summary>
    UInt = 0,

    /// <summary>
    /// 16-bit floating-point numbers; for regular image data.
    /// </summary>
    Half = 1,

    /// <summary>
    /// 32-bit IEEE-754 floating-point numbers; used where the range or precision of 16-bit number is not sufficient (for example, depth channels).
    /// </summary>
    Float = 2
}

public static class EXRDataTypeExtensions
{
    public static int GetBytesPerPixel(this EXRDataType pixelType)
    {
        return pixelType switch
        {
            EXRDataType.UInt => 4,
            EXRDataType.Half => 2,
            EXRDataType.Float => 4,
            _ => throw new NotImplementedException($"GetBytesPerPixel not implemented for {pixelType}")
        };
    }
}