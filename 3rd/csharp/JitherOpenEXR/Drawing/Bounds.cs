using System.Numerics;

namespace Jither.OpenEXR.Drawing;

public record Bounds<T>(T X, T Y, T Width, T Height) where T : IBinaryNumber<T>
{
    public T Area => Width * Height;
    public Dimensions<T> Dimensions => new(Width, Height);

    public T Left => X;
    public T Top => Y;
    public T Right => X + Width;
    public T Bottom => Y + Height;
}

