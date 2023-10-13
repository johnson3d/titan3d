using System.Numerics;

namespace Jither.OpenEXR.Drawing;

public record Dimensions<T>(T Width, T Height) where T : IBinaryNumber<T>
{
    public T Area => checked(Width * Height);

    public T X => Width;
    public T Y => Height;
}
