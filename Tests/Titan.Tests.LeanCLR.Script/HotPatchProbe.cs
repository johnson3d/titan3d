namespace Titan.HotPatch;

public static class HotPatchProbe
{
    public static int Add(int left, int right)
    {
        return IntrinsicAdd(left, right);
    }
    public static int IntrinsicAdd(int left, int right) => left + right;
    public static void ThrowProbe() => throw new InvalidOperationException("LeanCLR netstandard probe");
    public static string Echo(string value) => value;
    public static string Repeat(string text, int count) => text == null ? null : string.Concat(Enumerable.Repeat(text, count));
    public static byte[] EchoBytes(byte[] value) => value;
}

public sealed class HotPatchInstanceProbe
{
    public int Add(int left, int right)
    {
        return IntrinsicAdd(left, right);
    }

    public int IntrinsicAdd(int left, int right) => left + right;
    public int Measure(string text) => text == null ? -1 : text.Length;
    public string Greet(string name) => "Hi " + name;
}
