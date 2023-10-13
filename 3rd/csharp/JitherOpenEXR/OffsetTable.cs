using System.Collections;

namespace Jither.OpenEXR;

public class OffsetTable : List<long>
{
    public OffsetTable()
    {
    }

    public OffsetTable(int capacity) : base(capacity)
    {
    }

    public static OffsetTable ReadFrom(EXRReader reader, int count)
    {
        var result = new OffsetTable(count);
        for (int i = 0; i < count; i++)
        {
            // OpenEXR uses ulong - .NET uses long for file positions, so we'll read it as long. Should be enough...
            result.Add((long)reader.ReadULong());
        }
        return result;
    }
}