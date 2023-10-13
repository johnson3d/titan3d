using Jither.OpenEXR.Compression;
using Jither.OpenEXR.Drawing;

namespace Jither.OpenEXR;

public abstract class ChunkInfo
{
    protected EXRPart part;

    public int Index { get; }
    // OpenEXR uses ulong, but .NET doesn't, and this ought to be enough...
    public long FileOffset { get; set; }
    public long PixelDataFileOffset { get; set; }
    public int CompressedByteCount { get; set; }
    public int UncompressedByteCount => GetByteCount();
    public int PartNumber => part.PartNumber;

    protected ChunkInfo(EXRPart part, int index)
    {
        this.part = part;
        Index = index;
    }

    private int GetByteCount()
    {
        var bounds = GetBounds();
        try
        {
            return part.Channels.GetByteCount(bounds);
        }
        catch (OverflowException ex)
        {
            throw new EXRFormatException($"Combined byte count of {this} exceeds 2GB", ex);
        }
    }

    public abstract Bounds<int> GetBounds();

    public override string ToString()
    {
        return $"chunk part {PartNumber} index {Index}";
    }
}
