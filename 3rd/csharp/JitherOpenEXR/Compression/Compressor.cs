using System.Buffers;

namespace Jither.OpenEXR.Compression;

public abstract class Compressor
{
    public abstract int ScanLinesPerChunk { get; }

    public void Compress(ReadOnlySpan<byte> source, Stream dest, PixelDataInfo info)
    {
        if (InternalCompress(source, dest, info) != CompressionResult.Success)
        {
            if (this is not NullCompressor)
            {
                // Write uncompressed, e.g. if compression turns out to actually inflate the data.
                NullCompressor.Instance.Compress(source, dest, info);
            }
        }
    }

    public void Decompress(Stream source, Span<byte> dest, PixelDataInfo info)
    {
        if (this is not NullCompressor && source.Length == info.UncompressedByteSize)
        {
            // If compression actually turns out to inflate the data, the chunk will be written uncompressed instead.
            // There is no indication of this, other than the compressed data being the same size as the decompressed
            // data. Note that although uncompressed data is conceptually handled 1 scanline at a time, as opposed to
            // some of the compression methods, e.g. 16 scanlines of combined uncompressed data will look identical
            // to 16 "individually uncompressed" scanlines.
            NullCompressor.Instance.Decompress(source, dest, info);
        }
        else
        {
            InternalDecompress(source, dest, info);
        }
    }

    public abstract CompressionResult InternalCompress(ReadOnlySpan<byte> source, Stream dest, PixelDataInfo info);

    public abstract void InternalDecompress(Stream source, Span<byte> dest, PixelDataInfo info);
    
    protected static void UnpredictAndReorder(ReadOnlySpan<byte> source, Span<byte> dest, int length)
    {
        byte[] temp = ArrayPool<byte>.Shared.Rent(length);
        try
        {
            // Convert deltas to actual values
            int t1 = 0;
            byte previous = temp[1] = source[t1];
            t1++;
            while (t1 < length)
            {
                sbyte delta = (sbyte)source[t1];
                temp[t1] = previous = (byte)(previous + delta - 128);
                t1++;
            }

            // Data is split into two parts containing the bytes at odd and even offsets, respectively. Reorder them:
            t1 = 0;
            int t2 = (length + 1) / 2;
            int s = 0;
            while (s < length)
            {
                dest[s++] = temp[t1++];
                if (s < length)
                {
                    dest[s++] = temp[t2++];
                }
            }
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(temp);
        }
    }

    protected static void ReorderAndPredict(ReadOnlySpan<byte> source, Span<byte> dest, int length)
    {
        // Split data into two parts containing the bytes at odd and even offsets, respectively.
        int t1 = 0;
        int t2 = (length + 1) / 2;
        int s = 0;

        while (s < length)
        {
            dest[t1++] = source[s++];
            if (s < length)
            {
                dest[t2++] = source[s++];
            }
        }

        // Convert values to deltas inplace
        // index 0 holds the starting value, so unchanged
        t1 = 1;
        byte previous = dest[0];
        while (t1 < length)
        {
            byte current = dest[t1];
            int d = (sbyte)(current - previous + 128 + 256);
            previous = current;
            dest[t1++] = (byte)d;
        }
    }

}
