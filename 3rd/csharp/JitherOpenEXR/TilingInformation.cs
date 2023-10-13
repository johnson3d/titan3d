using Jither.OpenEXR.Attributes;
using Jither.OpenEXR.Drawing;
using Jither.OpenEXR.Helpers;

namespace Jither.OpenEXR;

public class TilingInformation
{
    public LevelMode LevelMode { get; }
    public RoundingMode RoundingMode { get; }

    public int XSize { get; }
    public int YSize { get; }

    public IReadOnlyList<TilingLevel> Levels { get; }

    public int LevelXCount { get; private set; }
    public int LevelYCount { get; private set; }

    public int TotalChunkCount { get; private set; }

    internal TilingInformation(EXRPart part) : this(part.Tiles, part.DataWindow.ToBounds(), part.Channels)
    {

    }

    internal TilingInformation(TileDesc? tileDesc, Bounds<int> bounds, ChannelList channels)
    {
        ArgumentNullException.ThrowIfNull(tileDesc);
        LevelMode = tileDesc.LevelMode;
        RoundingMode = tileDesc.RoundingMode;
        XSize = tileDesc.XSize;
        YSize = tileDesc.YSize;

        switch (LevelMode)
        {
            case LevelMode.One:
                LevelXCount = 1;
                LevelYCount = 1;
                break;
            case LevelMode.MipMap:
                LevelXCount = LevelYCount = RoundToInt(Math.Log2(Math.Max(bounds.Width, bounds.Height))) + 1;
                break;
            case LevelMode.RipMap:
                LevelXCount = RoundToInt(Math.Log2(bounds.Width)) + 1;
                LevelYCount = RoundToInt(Math.Log2(bounds.Height)) + 1;
                break;
            default:
                throw new NotSupportedException($"Unsupported level mode: {LevelMode}");
        }
        (Levels, TotalChunkCount) = CalculateLevels(bounds, channels);
    }

    public TilingLevel GetLevel(int levelX, int levelY)
    {
        switch (LevelMode)
        {
            case LevelMode.MipMap:
                if (levelX != levelY)
                {
                    throw new ArgumentException($"For mipmap parts, level number must be {nameof(levelX)} = {nameof(levelY)}");
                }
                if (levelX >= Levels.Count)
                {
                    throw new ArgumentOutOfRangeException($"This mipmap part has {Levels.Count} levels - level number must be between (0,0) and ({LevelXCount},{LevelYCount})");
                }
                return Levels[levelX];
            case LevelMode.One:
            case LevelMode.RipMap:
                int levelIndex = levelY * LevelXCount + levelX;
                if (levelIndex < 0 || levelIndex > Levels.Count)
                {
                    throw new ArgumentOutOfRangeException($"Level number for this part must be between (0,0) and ({LevelXCount},{LevelYCount})");
                }
                return Levels[levelX];
            default:
                throw new NotSupportedException($"Unsupported level mode: {LevelMode}");
        }
    }

    private int RoundToInt(double value)
    {
        return RoundingMode == RoundingMode.Down ? (int)value : (int)Math.Ceiling(value);
    }

    private int DivideWithRounding(int a, int b)
    {
        if (RoundingMode == RoundingMode.Down)
        {
            return a / b;
        }
        return a / b + (a % b > 0 ? 1 : 0);
    }

    private (IReadOnlyList<TilingLevel> levels, int totalChunkCount) CalculateLevels(Bounds<int> bounds, ChannelList channels)
    {
        var levels = new List<TilingLevel>();

        void AddLevel(int levelX, int levelY, int width, int height)
        {
            var levelBounds = new Bounds<int>(bounds.X, bounds.Y, width, height);
            var totalBytes = channels.GetByteCount(levelBounds);
            levels.Add(new TilingLevel(levelX, levelY, levelBounds, totalBytes));
        }

        int width = bounds.Width;
        int height = bounds.Height;
        AddLevel(0, 0, width, height);
        switch (LevelMode)
        {
            case LevelMode.MipMap:
                int index = 1;
                while (width > 1 || height > 1)
                {
                    if (width > 1)
                    {
                        width = DivideWithRounding(width, 2);
                    }
                    if (height > 1)
                    {
                        height = DivideWithRounding(height, 2);
                    }
                    AddLevel(index, index, width, height);
                    index++;
                }
                break;
            case LevelMode.RipMap:
                int levelX = 1;
                int levelY = 0;
                while (height > 1)
                {
                    width = bounds.Width;

                    height = DivideWithRounding(height, 2);

                    while (width > 1)
                    {
                        width = DivideWithRounding(width, 2);
                        AddLevel(levelX, levelY, width, height);
                        levelX++;
                    }
                    levelX = 0;
                    levelY++;
                }
                break;
            default:
            case LevelMode.One:
                break;
        }

        int chunkIndex = 0;
        foreach (var level in levels)
        {
            level.FirstChunkIndex = chunkIndex;
            level.ChunkCount = MathHelpers.DivAndRoundUp(level.DataWindow.Width, XSize) * MathHelpers.DivAndRoundUp(level.DataWindow.Height, YSize);
            chunkIndex += level.ChunkCount;
        }

        return (levels, chunkIndex);
    }
}