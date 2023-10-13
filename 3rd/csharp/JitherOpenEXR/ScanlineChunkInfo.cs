using Jither.OpenEXR.Drawing;

namespace Jither.OpenEXR;

public class ScanlineChunkInfo : ChunkInfo
{
    public int Y { get; }

    public ScanlineChunkInfo(EXRPart part, int chunkIndex, int y) : base(part, chunkIndex)
    {
        Y = y;
    }

    public override Bounds<int> GetBounds()
    {
        var scanLinesPerChunk = part.Compression.GetScanLinesPerChunk();
        var height = Math.Min(scanLinesPerChunk, part.DataWindow.YMax - Y + 1);
        return new Bounds<int>(0, Y, part.DataWindow.Width, height);
    }
}
