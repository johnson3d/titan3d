using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jither.OpenEXR.Compression;

internal sealed class SubStream : Stream
{
    private readonly Stream baseStream;
    private readonly long length;
    private readonly long baseOffset;
    private long position;

    public SubStream(Stream baseStream, long offset, long length)
    {
        this.baseStream = baseStream;
        this.baseOffset = offset;
        this.length = length;
    }

    public override bool CanRead => true;
    public override bool CanWrite => true;
    public override bool CanSeek => true;

    public override long Length => length;


    public override void Flush()
    {
        // Do nothing
    }

    public override long Position {
        get => position;
        set
        {
            if (value < 0 || value > length)
            {
                throw new ArgumentOutOfRangeException(nameof(Position), $"SubStream position out of range 0-{length}. Was: {value}");
            }
            position = value;
        }
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
        if (count < 0)
        {
            // Why is .NET's Stream.Read count parameter even a signed integer? 
            throw new ArgumentOutOfRangeException(nameof(count), "count cannot be negative");
        }
        baseStream.Position = baseOffset + position;
        count = (int)Math.Min(count, length - position);
        int bytesRead = baseStream.Read(buffer, offset, count);
        position += bytesRead;
        return bytesRead;
    }

    public override long Seek(long offset, SeekOrigin origin)
    {
        switch (origin)
        {
            case SeekOrigin.Begin:
                position = Math.Min(offset, length - 1);
                break;
            case SeekOrigin.End:
                position = Math.Max(length - offset, 0);
                break;
            case SeekOrigin.Current:
                position += offset;
                if (position < 0)
                {
                    position = 0;
                }
                if (position > length - 1)
                {
                    position = length - 1;
                }
                break;
        }
        return position;
    }

    public override void SetLength(long value)
    {
        throw new NotSupportedException();
    }

    public override void Write(byte[] buffer, int offset, int count)
    {
        throw new NotSupportedException();
    }
}
