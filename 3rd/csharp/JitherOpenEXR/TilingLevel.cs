using Jither.OpenEXR.Drawing;

namespace Jither.OpenEXR;

public class TilingLevel
{
    public Bounds<int> DataWindow { get; }

    public int LevelX { get; }
    public int LevelY { get; }

    public int FirstChunkIndex { get; internal set; }
    public int ChunkCount { get; internal set; }
    public int TotalByteCount { get; }

    public TilingLevel(int levelX, int levelY, Bounds<int> dataWindow, int totalByteCount)
    {
        LevelX = levelX;
        LevelY = levelY;
        DataWindow = dataWindow;
        TotalByteCount = totalByteCount;
    }
}
