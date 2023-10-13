using Jither.OpenEXR.Compression;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jither.OpenEXR;

public class EXRReader : IDisposable
{
    private readonly BinaryReader reader;
    private bool disposed = false;

    public EXRReader(BinaryReader reader)
    {
        this.reader = reader;
    }

    public EXRReader(Stream stream, bool leaveOpen = false) : this(new BinaryReader(stream, Encoding.Latin1, leaveOpen))
    {
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    public long Position => reader.BaseStream.Position;
    public long Length => reader.BaseStream.Length;
    public long Remaining => reader.BaseStream.Length - reader.BaseStream.Position;

    public byte ReadByte() => reader.ReadByte();
    public short ReadShort() => reader.ReadInt16();
    public ushort ReadUShort() => reader.ReadUInt16();
    public int ReadInt() => reader.ReadInt32();
    public uint ReadUInt() => reader.ReadUInt32();
    public ulong ReadULong() => reader.ReadUInt64();
    public Half ReadHalf() => reader.ReadHalf();
    public float ReadFloat() => reader.ReadSingle();
    public double ReadDouble() => reader.ReadDouble();

    public void Seek(long offset)
    {
        // OpenEXR uses unsigned long offsets - we have to settle with signed. "8.589.934.592 gigabytes ought to be enough for everybody".
        reader.BaseStream.Seek(offset, SeekOrigin.Begin);
    }

    public Stream GetChunkStream(int length)
    {
        return new SubStream(reader.BaseStream, Position, length);
    }

    public float[] ReadFloatArray(int size)
    {
        float[] result = new float[size];
        for (int i = 0; i < size; i++)
        {
            result[i] = ReadFloat();
        }
        return result;
    }

    public string ReadStringZ(long max = 0)
    {
        var builder = new StringBuilder();
        max = max > 0 ? max : reader.BaseStream.Length - reader.BaseStream.Position;
        int position = 0;
        while (position < max)
        {
            char c = reader.ReadChar();
            if (c == '\0')
            {
                break;
            }
            builder.Append(c);
        }
        return builder.ToString();
    }

    public string ReadString(int length)
    {
        long remaining = reader.BaseStream.Length - reader.BaseStream.Position;
        if (length <= 0)
        {
            throw new EXRFormatException($"Error reading text/string: Specified length wasn't a positive integer (was: {length})");
        }
        if (length > remaining)
        {
            throw new EXRFormatException($"Error reading text/string: Length ({length}) is larger than remaining file size ({remaining})");
        }
        char[] result = ArrayPool<char>.Shared.Rent(length);
        try
        {
            reader.Read(result, 0, length);
            return new String(result, 0, length);
        }
        finally
        {
            ArrayPool<char>.Shared.Return(result);
        }
    }

    public byte[] ReadBytes(int length)
    {
        return reader.ReadBytes(length);
    }

    public string ReadString()
    {
        int length = ReadInt();
        return ReadString(length);
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
                reader.Dispose();
            }
            catch
            {

            }
        }
        disposed = true;
    }
}
