using Jither.OpenEXR.Attributes;

namespace Jither.OpenEXR;

using Rational = Attributes.Rational;

public abstract class EXRAttribute
{
    public string Name { get; private set; }
    public string Type => this.UntypedValue switch
    {
        double => "double",
        float => "float",
        int => "int",
        string => "string",
        Box2i => "box2i",
        Box2f => "box2f",
        Chromaticities => "chromaticities",
        EXRCompression => "compression",
        EnvironmentMap => "envmap",
        KeyCode => "keycode",
        LineOrder => "lineOrder",
        M33f => "m33f",
        M44f => "m44f",
        Rational => "rational",
        List<string> => "stringvector",
        TileDesc => "tiledesc",
        TimeCode => "timecode",
        V2i => "v2i",
        V2f => "v2f",
        V3i => "v3i",
        V3f => "v3f",
        ChannelList => "chlist",
        Preview => "preview",
        UnknownValue uv => uv.Type,
        _ => throw new NotImplementedException($"Type name for {UntypedValue?.GetType()} not implemented.")
    };

    public abstract object? UntypedValue { get; }

    protected EXRAttribute(string name)
    {
        if (String.IsNullOrEmpty(name)) throw new ArgumentNullException(nameof(name), "Attribute names cannot be null or empty.");
        Name = name;
    }

    public abstract void WriteTo(EXRWriter writer);

    public static EXRAttribute? ReadFrom(EXRReader reader, int maxNameLength)
    {
        string name, type;
        int size;

        try
        {
            name = reader.ReadStringZ(maxNameLength);
        }
        catch (Exception ex)
        {
            throw new EXRFormatException("Error reading header attribute name.", ex);
        }

        if (name == String.Empty)
        {
            return null;
        }

        try
        {
            type = reader.ReadStringZ(maxNameLength);
        }
        catch (Exception ex)
        {
            throw new EXRFormatException("Error reading header attribute type.", ex);
        }

        if (type == String.Empty)
        {
            throw new EXRFormatException("Empty header attribute type.");
        }

        size = reader.ReadInt();
        void CheckSize(int expectedSize)
        {
            if (size != expectedSize)
            {
                throw new EXRFormatException($"Expected size of header attribute {name} (type: {type}) to be {expectedSize} bytes, but was {size}.");
            }
        }
        switch (type)
        {
            case "box2i":
                CheckSize(16);
                return new EXRAttribute<Box2i>(name, new Box2i(reader.ReadInt(), reader.ReadInt(), reader.ReadInt(), reader.ReadInt()));
            case "box2f":
                CheckSize(16);
                return new EXRAttribute<Box2f>(name, new Box2f(reader.ReadFloat(), reader.ReadFloat(), reader.ReadFloat(), reader.ReadFloat()));
            case "chromaticities":
                CheckSize(32);
                return new EXRAttribute<Chromaticities>(name, new Chromaticities(reader.ReadFloat(), reader.ReadFloat(), reader.ReadFloat(), reader.ReadFloat(), reader.ReadFloat(), reader.ReadFloat(), reader.ReadFloat(), reader.ReadFloat()));
            case "compression":
                CheckSize(1);
                return new EXRAttribute<EXRCompression>(name, (EXRCompression)reader.ReadByte());
            case "double":
                CheckSize(4);
                return new EXRAttribute<double>(name, reader.ReadDouble());
            case "envmap":
                CheckSize(1);
                return new EXRAttribute<EnvironmentMap>(name, (EnvironmentMap)reader.ReadByte());
            case "float":
                CheckSize(4);
                return new EXRAttribute<float>(name, reader.ReadFloat());
            case "int":
                CheckSize(4);
                return new EXRAttribute<int>(name, reader.ReadInt());
            case "keycode":
                CheckSize(28);
                return new EXRAttribute<KeyCode>(name, new KeyCode(reader.ReadInt(), reader.ReadInt(), reader.ReadInt(), reader.ReadInt(), reader.ReadInt(), reader.ReadInt(), reader.ReadInt()));
            case "lineOrder":
                CheckSize(1);
                return new EXRAttribute<LineOrder>(name, (LineOrder)reader.ReadByte());
            case "m33f":
                CheckSize(36);
                return new EXRAttribute<M33f>(name, new M33f(reader.ReadFloatArray(9)));
            case "m44f":
                CheckSize(64);
                return new EXRAttribute<M44f>(name, new M44f(reader.ReadFloatArray(16)));
            case "rational":
                CheckSize(8);
                return new EXRAttribute<Rational>(name, new Rational(reader.ReadInt(), reader.ReadUInt()));
            case "string":
                return new EXRAttribute<string>(name, reader.ReadString(size));
            case "stringvector":
                var list = new List<string>();
                if (size != 0 && size < 4)
                {
                    throw new EXRFormatException($"Expected size of 0 or 4+ for size of attribute {name} (type: {type}), but was: {size}");
                }
                long totalRead = 0;

                while (totalRead < size)
                {
                    var start = reader.Position;
                    var str = reader.ReadString();
                    list.Add(str);
                    totalRead += reader.Position - start;
                }
                if (totalRead != size)
                {
                    throw new EXRFormatException($"Attribute {name} (type: {type}) declared size to be {size} bytes, but read {totalRead}");
                }
                return new EXRAttribute<List<string>>(name, list);
            case "tiledesc":
                CheckSize(9);
                // We're ignoring that tiledesc X/Y are actually unsigned - it's inconsistent with every other part of OpenEXR, e.g.
                // DataWindow and DisplayWindow which are signed.
                return new EXRAttribute<TileDesc>(name, new TileDesc(reader.ReadInt(), reader.ReadInt(), reader.ReadByte()));
            case "timecode":
                CheckSize(8);
                return new EXRAttribute<TimeCode>(name, new TimeCode(reader.ReadUInt(), reader.ReadUInt()));
            case "v2i":
                CheckSize(8);
                return new EXRAttribute<V2i>(name, new V2i(reader.ReadInt(), reader.ReadInt()));
            case "v2f":
                CheckSize(8);
                return new EXRAttribute<V2f>(name, new V2f(reader.ReadFloat(), reader.ReadFloat()));
            case "v3i":
                CheckSize(12);
                return new EXRAttribute<V3i>(name, new V3i(reader.ReadInt(), reader.ReadInt(), reader.ReadInt()));
            case "v3f":
                CheckSize(12);
                return new EXRAttribute<V3f>(name, new V3f(reader.ReadFloat(), reader.ReadFloat(), reader.ReadFloat()));
            case "chlist":
                try
                {
                    var channelList = ChannelList.ReadFrom(reader, size);
                    return new EXRAttribute<ChannelList>(name, channelList);
                }
                catch (Exception ex)
                {
                    throw new EXRFormatException($"Failed reading channel list '{name}'.", ex);
                }
            case "preview":
                if (size < 9)
                {
                    throw new EXRFormatException($"Failed reading preview: Contained no preview image data");
                }
                return new EXRAttribute<Preview>(name, new Preview(reader.ReadUInt(), reader.ReadUInt(), reader.ReadBytes(size - 8)));
            default:
                return new EXRAttribute<UnknownValue>(name, new UnknownValue(type, reader.ReadBytes(size)));
        }
    }
}

public class EXRAttribute<T> : EXRAttribute
{
    public T Value { get; private set; }
    public override object? UntypedValue => Value;

    public EXRAttribute(string name, T value) : base(name)
    {
        Value = value;
    }

    public override void WriteTo(EXRWriter writer)
    {
        if (Name == null)
        {
            return;
        }
        writer.WriteStringZ(Name);

        writer.WriteStringZ(Type);

        void WriteSize(int size)
        {
            writer.WriteInt(size);
        }

        switch (Value)
        {
            case Box2i box2i:
                WriteSize(16);
                writer.WriteInt(box2i.XMin);
                writer.WriteInt(box2i.YMin);
                writer.WriteInt(box2i.XMax);
                writer.WriteInt(box2i.YMax);
                break;
            case Box2f box2f:
                WriteSize(16);
                writer.WriteFloat(box2f.XMin);
                writer.WriteFloat(box2f.YMin);
                writer.WriteFloat(box2f.XMax);
                writer.WriteFloat(box2f.YMax);
                break;
            case Chromaticities chromaticities:
                WriteSize(32);
                writer.WriteFloat(chromaticities.RedX);
                writer.WriteFloat(chromaticities.RedY);
                writer.WriteFloat(chromaticities.GreenX);
                writer.WriteFloat(chromaticities.GreenY);
                writer.WriteFloat(chromaticities.BlueX);
                writer.WriteFloat(chromaticities.BlueY);
                writer.WriteFloat(chromaticities.WhiteX);
                writer.WriteFloat(chromaticities.WhiteY);
                break;
            case EXRCompression compression:
                WriteSize(1);
                writer.WriteByte((byte)compression);
                break;
            case double d:
                WriteSize(4);
                writer.WriteDouble(d);
                break;
            case EnvironmentMap envMap:
                WriteSize(1);
                writer.WriteByte((byte)envMap);
                break;
            case float f:
                WriteSize(4);
                writer.WriteFloat(f);
                break;
            case int i:
                WriteSize(4);
                writer.WriteInt(i);
                break;
            case KeyCode keyCode:
                WriteSize(28);
                writer.WriteInt(keyCode.FilmMFCCode);
                writer.WriteInt(keyCode.FilmType);
                writer.WriteInt(keyCode.Prefix);
                writer.WriteInt(keyCode.Count);
                writer.WriteInt(keyCode.PerfOffset);
                writer.WriteInt(keyCode.PerfsPerFrame);
                writer.WriteInt(keyCode.PerfsPerCount);
                break;
            case LineOrder lineOrder:
                WriteSize(1);
                writer.WriteByte((byte)lineOrder);
                break;
            case M33f m33f:
                WriteSize(36);
                writer.WriteFloatArray(m33f.Values);
                break;
            case M44f m44f:
                WriteSize(64);
                writer.WriteFloatArray(m44f.Values);
                break;
            case Attributes.Rational rational:
                WriteSize(8);
                writer.WriteInt(rational.Numerator);
                writer.WriteUInt(rational.Denominator);
                break;
            case string str:
                writer.WriteString(str);
                break;
            case List<string> stringVector:
                var sizePosition = writer.Position;
                writer.WriteInt(0); // Placeholder
                foreach (var str in stringVector)
                {
                    writer.WriteString(str);
                }
                var endPosition = writer.Position;
                writer.Seek(sizePosition, SeekOrigin.Begin);
                WriteSize((int)(endPosition - sizePosition - 4));
                writer.Seek(0, SeekOrigin.End);
                break;
            case TileDesc tileDesc:
                WriteSize(9);
                // We're ignoring that tiledesc X/Y are actually unsigned - it's inconsistent with every other part of OpenEXR, e.g.
                // DataWindow and DisplayWindow, which are signed.
                writer.WriteInt(tileDesc.XSize);
                writer.WriteInt(tileDesc.YSize);
                writer.WriteByte(tileDesc.Mode);
                break;
            case TimeCode timeCode:
                WriteSize(8);
                writer.WriteUInt(timeCode.TimeAndFlags);
                writer.WriteUInt(timeCode.UserData);
                break;
            case V2i v2i:
                WriteSize(8);
                writer.WriteInt(v2i.V0);
                writer.WriteInt(v2i.V1);
                break;
            case V2f v2f:
                WriteSize(8);
                writer.WriteFloat(v2f.V0);
                writer.WriteFloat(v2f.V1);
                break;
            case V3i v3i:
                WriteSize(12);
                writer.WriteInt(v3i.V0);
                writer.WriteInt(v3i.V1);
                writer.WriteInt(v3i.V2);
                break;
            case V3f v3f:
                WriteSize(12);
                writer.WriteFloat(v3f.V0);
                writer.WriteFloat(v3f.V1);
                writer.WriteFloat(v3f.V2);
                break;
            case ChannelList chlist:
                chlist.WriteTo(writer);
                break;
            case Preview preview:
                WriteSize(preview.RGBAData.Length + 8);
                writer.WriteUInt(preview.Width);
                writer.WriteUInt(preview.Height);
                writer.WriteBytes(preview.RGBAData);
                break;
            case UnknownValue unknown:
                WriteSize(unknown.Bytes.Length);
                writer.WriteBytes(unknown.Bytes);
                break;
            default:
                throw new NotImplementedException($"Writing of attribute {Name} (type: {Type}) not implemented.");
        }
    }
}
