using Jither.OpenEXR.Drawing;

namespace Jither.OpenEXR.Converters;

/// <summary>
/// PixelConverters handle conversion from/to OpenEXR's pixel data structure.
/// </summary>
internal abstract class PixelConverter
{
    public abstract void ToEXR(Bounds<int> bounds, ReadOnlySpan<byte> source, Span<byte> dest);
    public abstract void FromEXR(Bounds<int> bounds, ReadOnlySpan<byte> source, Span<byte> dest);
}
