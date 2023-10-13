namespace Jither.OpenEXR;

public class EXRVersion
{
    public byte Number { get; }
    public EXRVersionFlags Flags { get; }

    public bool IsSinglePartTiled => Flags.HasFlag(EXRVersionFlags.IsSinglePartTiled);
    public bool HasLongNames => Flags.HasFlag(EXRVersionFlags.LongNames);
    public bool HasNonImageParts => Flags.HasFlag(EXRVersionFlags.NonImageParts);
    public bool IsMultiPart => Flags.HasFlag(EXRVersionFlags.MultiPart);

    public int MaxNameLength => HasLongNames ? 255 : 31;

    public EXRVersion(int version)
    {
        Number = (byte)(version & 0xff);
        Flags = (EXRVersionFlags)(version >> 8);
        Check();
    }

    public EXRVersion(byte number, EXRVersionFlags flags)
    {
        Number = number;
        Flags = flags;
        Check();
    }

    public static EXRVersion ReadFrom(EXRReader reader)
    {
        return new EXRVersion(reader.ReadInt());
    }

    public void WriteTo(EXRWriter writer)
    {
        writer.WriteInt(((int)Flags << 8) | Number);
    }

    public void Check()
    {
        if (Number != 1 && Number != 2)
        {
            throw new EXRFormatException($"Expected OpenEXR version 1 or 2, but {Number} found.");
        }
        if (Number == 1)
        {
            if (IsMultiPart)
            {
                throw new EXRFormatException($"Version 1 file with 'multi part' bit set");
            }
            if (HasNonImageParts)
            {
                throw new EXRFormatException($"Version 1 file with 'non image parts' bit set");
            }
        }
        if (IsSinglePartTiled && (IsMultiPart || HasNonImageParts))
        {
            throw new EXRFormatException($"Single part tiled files cannot also be multi part or have non image parts");
        }
    }
}
