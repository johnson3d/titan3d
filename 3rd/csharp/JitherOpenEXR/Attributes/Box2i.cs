using Jither.OpenEXR.Drawing;

namespace Jither.OpenEXR.Attributes;

public record Box2i(int XMin, int YMin, int XMax, int YMax)
{
    public int Width => checked(XMax - XMin + 1);
    public int Height => checked(YMax - YMin + 1);

    public Box2i(Bounds<int> bounds) : this(bounds.Left, bounds.Top, bounds.Right - 1, bounds.Bottom - 1)
    {
    }

    public Bounds<int> ToBounds()
    {
        return new Bounds<int>(XMin, YMin, Width, Height);
    }

    public void Validate(string name)
    {
        if (Width < 1 || Height < 1)
        {
            throw new EXRFormatException($"{name} has invalid size: {this}");
        }
    }
}
