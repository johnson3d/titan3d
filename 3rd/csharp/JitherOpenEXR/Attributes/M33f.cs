namespace Jither.OpenEXR.Attributes;

public readonly struct M33f
{
    public IReadOnlyList<float> Values { get; }

    public M33f(params float[] values)
    {
        if (values.Length != 9)
        {
            throw new ArgumentException($"M33f requires 9 values, got: {values.Length}");
        }
        Values = values;
    }
}
