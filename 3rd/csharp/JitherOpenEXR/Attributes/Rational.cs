namespace Jither.OpenEXR.Attributes;

public record Rational(int Numerator, uint Denominator)
{
    public double Value => (double)Numerator / Denominator;
}
