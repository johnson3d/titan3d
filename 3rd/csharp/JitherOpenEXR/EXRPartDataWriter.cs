using Jither.OpenEXR.Compression;
using Jither.OpenEXR.Converters;
using System.Buffers;

namespace Jither.OpenEXR;

public class EXRPartDataWriter : EXRPartDataHandler
{
    private readonly EXRWriter writer;
    private readonly long offsetTableOffset;

    internal EXRPartDataWriter(EXRPart part, EXRVersion version, EXRWriter writer) : base(part, version)
    {
        this.writer = writer;
        offsetTableOffset = writer.Position;
    }

    public void WriteOffsetPlaceholders()
    {
        for (int i = 0; i < ChunkCount; i++)
        {
            writer.WriteULong(0xffffffffffffffffUL);
        }
    }

    public void Write(ReadOnlySpan<byte> source)
    {
        int sourceOffset = 0;
        for (int chunkIndex = 0; chunkIndex < ChunkCount; chunkIndex++)
        {
            int y = chunkIndex * compressor.ScanLinesPerChunk + part.DataWindow.YMin;
            var chunkInfo = new ScanlineChunkInfo(part, chunkIndex, y);
            int bytesWritten = InternalWriteChunk(chunkInfo, source[sourceOffset..]);
            sourceOffset += bytesWritten;
        }
    }

    public void WriteChunk(ChunkInfo chunkInfo, ReadOnlySpan<byte> source)
    {
        CheckWriteCount(chunkInfo, source);
        InternalWriteChunk(chunkInfo, source);
    }

    public void WriteInterleaved(ReadOnlySpan<byte> source, string[] channelOrder)
    {
        int sourceOffset = 0;
        var converter = new PixelInterleaveConverter(part.Channels, channelOrder);
        for (int chunkIndex = 0; chunkIndex < ChunkCount; chunkIndex++)
        {
            int y = chunkIndex * compressor.ScanLinesPerChunk + part.DataWindow.YMin;
            var chunkInfo = new ScanlineChunkInfo(part, chunkIndex, y);
            var pixelData = ArrayPool<byte>.Shared.Rent(chunkInfo.UncompressedByteCount);
            try
            {
                converter.ToEXR(chunkInfo.GetBounds(), source[sourceOffset..], pixelData);
                int bytesWritten = InternalWriteChunk(chunkInfo, pixelData);
                sourceOffset += bytesWritten;
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(pixelData);
            }
        }
    }

    public int WriteChunkInterleaved(ChunkInfo chunkInfo, ReadOnlySpan<byte> source, string[] channelOrder)
    {
        CheckWriteCount(chunkInfo, source);
        CheckInterleavedPrerequisites();

        var converter = new PixelInterleaveConverter(part.Channels, channelOrder);
        var pixelData = ArrayPool<byte>.Shared.Rent(chunkInfo.UncompressedByteCount);
        try
        {
            converter.ToEXR(chunkInfo.GetBounds(), source, pixelData);
            return InternalWriteChunk(chunkInfo, pixelData);
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(pixelData);
        }
    }

    private int InternalWriteChunk(ChunkInfo chunkInfo, ReadOnlySpan<byte> source)
    {
        chunkInfo.FileOffset = writer.Position;

        long sizeOffset = WriteChunkHeader(chunkInfo);

        chunkInfo.PixelDataFileOffset = writer.Position;
        var destStream = writer.GetStream();

        var info = new PixelDataInfo(
            part.Channels,
            chunkInfo.GetBounds(),
            chunkInfo.UncompressedByteCount
        );
        compressor.Compress(source[..chunkInfo.UncompressedByteCount], destStream, info);

        var size = (int)(writer.Position - sizeOffset - 4);
        writer.Seek(sizeOffset);
        writer.WriteInt(size);

        writer.Seek(offsetTableOffset + chunkInfo.Index * 8);
        writer.WriteULong((ulong)chunkInfo.FileOffset);

        writer.Seek(0, SeekOrigin.End);

        return chunkInfo.UncompressedByteCount;
    }

    private long WriteChunkHeader(ChunkInfo chunkInfo)
    {
        if (fileIsMultiPart)
        {
            writer.WriteInt(chunkInfo.PartNumber);
        }

        if (part.IsTiled)
        {
            if (chunkInfo is not TileChunkInfo tileInfo)
            {
                throw new EXRFormatException($"Expected tile chunk info for {chunkInfo}");
            }
            writer.WriteInt(tileInfo.X);
            writer.WriteInt(tileInfo.Y);
            writer.WriteInt(tileInfo.LevelX);
            writer.WriteInt(tileInfo.LevelY);
        }
        else
        {
            if (chunkInfo is not ScanlineChunkInfo scanlineInfo)
            {
                throw new EXRFormatException($"Expected scanline chunk info for {chunkInfo}");
            }
            writer.WriteInt(scanlineInfo.Y);
        }
        long sizeOffset = writer.Position;
        writer.WriteInt(0); // Placeholder
        return sizeOffset;
    }

    private static void CheckWriteCount(ChunkInfo chunkInfo, ReadOnlySpan<byte> source)
    {
        int actual = source.Length;
        int expected = chunkInfo.UncompressedByteCount;
        if (actual < expected)
        {
            throw new ArgumentException($"Expected chunk to write to be {expected} bytes, but got span with {actual} bytes", nameof(source));
        }
    }
}
