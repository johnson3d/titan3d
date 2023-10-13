using Jither.OpenEXR.Drawing;

namespace Jither.OpenEXR.Compression;

public sealed class PixelDataInfo
{
    public ChannelList Channels { get; }
    public Bounds<int> Bounds { get; }
    public int UncompressedByteSize { get; }

    public PixelDataInfo(ChannelList channels, Bounds<int> bounds, int expectedByteSize)
    {
        Channels = channels;
        Bounds = bounds;
        UncompressedByteSize = expectedByteSize;
    }
}
