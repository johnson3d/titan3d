namespace Jither.OpenEXR.Attributes;

// Ignoring that this is a uint - it's inconsistent with everything else - e.g. DataWindow and DisplayWindow, which use signed integers
public record TileDesc(int XSize, int YSize, LevelMode LevelMode, RoundingMode RoundingMode)
{
    public byte Mode => (byte)(((int)RoundingMode << 4) | (int)LevelMode);

    public TileDesc(int xSize, int ySize, byte mode) : this(xSize, ySize, (LevelMode)(mode & 0xf), (RoundingMode)((mode & 0xf0) >> 4))
    {
    }
}
