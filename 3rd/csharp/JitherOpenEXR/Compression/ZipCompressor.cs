using System.Buffers;
using System.IO.Compression;

namespace Jither.OpenEXR.Compression;

internal class ZipCompressor : Compressor
{
    public override int ScanLinesPerChunk { get; } = EXRCompression.ZIP.GetScanLinesPerChunk();

    public override CompressionResult InternalCompress(ReadOnlySpan<byte> source, Stream dest, PixelDataInfo info)
    {
        int length = info.UncompressedByteSize;
        byte[] buffer = ArrayPool<byte>.Shared.Rent(length);
        try
        {
            ReorderAndPredict(source, buffer, length);
            using (var intermediary = new MemoryStream())
            {
                using (var zlib = new ZLibStream(intermediary, CompressionLevel.Optimal, leaveOpen: true))
                {
                    zlib.Write(buffer, 0, length);
                }

                if (intermediary.Position >= info.UncompressedByteSize)
                {
                    return CompressionResult.NoGain;
                }
                intermediary.Position = 0;
                intermediary.CopyTo(dest);
                return CompressionResult.Success;
            }
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buffer);
        }
    }

    public override void InternalDecompress(Stream source, Span<byte> dest, PixelDataInfo info)
    {
        int length = info.UncompressedByteSize;
        byte[] buffer = ArrayPool<byte>.Shared.Rent(length);
        try
        {
            using (var zlib = new ZLibStream(source, CompressionMode.Decompress, leaveOpen: true))
            {
                try
                {
                    zlib.ReadExactly(buffer, 0, length);
                }
                catch (Exception ex) when (ex is InvalidDataException or ArgumentOutOfRangeException)
                {
                    throw new EXRCompressionException($"Invalid zlib compressed data", ex);
                }
            }

            UnpredictAndReorder(buffer, dest, length);
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buffer);
        }
    }
}
