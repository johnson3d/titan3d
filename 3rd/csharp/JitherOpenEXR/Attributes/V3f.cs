namespace Jither.OpenEXR.Attributes;

public record V3f(float V0, float V1, float V2)
{
    public float X => V0;
    public float Y => V1;
    public float Z => V2;
}
