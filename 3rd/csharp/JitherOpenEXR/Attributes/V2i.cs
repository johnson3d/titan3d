namespace Jither.OpenEXR.Attributes;

public record V2i(int V0, int V1)
{
    public int X => V0;
    public int Y => V1;

    public int Area => V0 * V1;
}
