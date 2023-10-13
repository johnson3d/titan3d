namespace Jither.OpenEXR.Attributes;

public readonly struct M44f
{
    public IReadOnlyList<float> Values { get; }

    public M44f(params float[] values)
    {
        if (values.Length != 16)
        {
            throw new ArgumentException($"M44F requires 16 values, got: {values.Length}");
        }
        Values = values;
    }
}
