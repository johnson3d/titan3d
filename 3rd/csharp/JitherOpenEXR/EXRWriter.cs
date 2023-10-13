using System.Text;

namespace Jither.OpenEXR;

public class EXRWriter : IDisposable
{
    private readonly BinaryWriter writer;
    private bool disposed = false;
    private readonly int maxNameLength;

    public EXRWriter(BinaryWriter writer, int maxNameLength)
    {
        this.writer = writer;
        this.maxNameLength = maxNameLength;
    }

    public EXRWriter(Stream stream, int maxNameLength, bool leaveOpen = false) : this(new BinaryWriter(stream, Encoding.Latin1, leaveOpen), maxNameLength)
    {
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    public long Position => writer.BaseStream.Position;

    public void WriteByte(byte value) => writer.Write(value);
    public void WriteShort(short value) => writer.Write(value);
    public void WriteUShort(ushort value) => writer.Write(value);
    public void WriteInt(int value) => writer.Write(value);
    public void WriteUInt(uint value) => writer.Write(value);
    public void WriteULong(ulong value) => writer.Write(value);
    public void WriteHalf(Half value) => writer.Write(value);
    public void WriteFloat(float value) => writer.Write(value);
    public void WriteDouble(double value) => writer.Write(value);

    public void Seek(long offset, SeekOrigin origin = SeekOrigin.Begin)
    {
        // OpenEXR uses unsigned long offsets - we have to settle with signed. "8.589.934.592 gigabytes ought to be enough for everybody".
        writer.BaseStream.Seek(offset, origin);
    }

    public Stream GetStream()
    {
        return writer.BaseStream;
    }

    public void WriteFloatArray(IEnumerable<float> value)
    {
        foreach (var val in value)
        {
            writer.Write(val);
        }
    }

    public void WriteStringZ(string value)
    {
        if (value.Length > maxNameLength)
        {
            throw new EXRFormatException($"Name length ({value.Length}) exceeds maximum length ({maxNameLength}) for this EXR file.");
        }
        var bytes = Encoding.Latin1.GetBytes(value);
        writer.Write(bytes);
        WriteByte(0);
    }

    public void WriteBytes(byte[] bytes)
    {
        writer.Write(bytes);
    }

    public void WriteString(string value, bool writeSize = true)
    {
        if (writeSize)
        {
            WriteInt(value.Length);
        }
        var bytes = Encoding.Latin1.GetBytes(value);
        writer.Write(bytes);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposed)
        {
            return;
        }

        if (disposing)
        {
            try
            {
                writer.Dispose();
            }
            catch
            {

            }
        }
        disposed = true;
    }
}
