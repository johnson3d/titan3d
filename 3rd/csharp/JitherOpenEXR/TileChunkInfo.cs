using Jither.OpenEXR.Attributes;
using Jither.OpenEXR.Drawing;

namespace Jither.OpenEXR;

public class TileChunkInfo : ChunkInfo
{
    private readonly TilingLevel level;

    public int X { get; }
    public int Y { get; }
    public int LevelX { get; }
    public int LevelY { get; }

    protected TileDesc Tiles => part.Tiles ?? throw new InvalidOperationException($"Expected part to have a tiles attribute.");

    public TileChunkInfo(EXRPart part, int index, TilingLevel level, int x, int y, int levelX, int levelY) : base(part, index)
    {
        this.level = level;
        X = x * Tiles.XSize;
        Y = y * Tiles.YSize;
        LevelX = levelX;
        LevelY = levelY;
    }

    public override Bounds<int> GetBounds()
    {
        var dataWindow = level.DataWindow;
        int width = Math.Min(Tiles.XSize, dataWindow.Width - X);
        int height = Math.Min(Tiles.YSize, dataWindow.Height - Y);
        return new Bounds<int>(X, Y, width, height);
    }
}
