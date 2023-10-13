namespace Jither.OpenEXR.Attributes;

public record Box2f(float XMin, float YMin, float XMax, float YMax)
{
    public float Width => XMax - XMin + 1;
    public float Height => YMax - YMin + 1;
}
