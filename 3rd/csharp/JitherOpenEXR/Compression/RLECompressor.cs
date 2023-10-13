using System.Buffers;

namespace Jither.OpenEXR.Compression;

internal sealed class RLECompressor : Compressor
{
    private const int MAX_RUN_LENGTH = 127;
    private const int MIN_RUN_LENGTH = 3;

    public override int ScanLinesPerChunk { get; } = EXRCompression.RLE.GetScanLinesPerChunk();

    public override CompressionResult InternalCompress(ReadOnlySpan<byte> source, Stream dest, PixelDataInfo info)
    {
        int length = info.UncompressedByteSize;
        // We only need a compressed array of the same size as uncompressed. If we're about to overflow it,
        // we'll return CompressionResult.NoGain.
        byte[] uncompressed = ArrayPool<byte>.Shared.Rent(length);
        try
        {
            byte[] compressed = ArrayPool<byte>.Shared.Rent(length);
            int destIndex = 0;
            try
            {
                ReorderAndPredict(source, uncompressed, length);

                int runs = 0;
                int rune = 1;
                while (runs < length)
                {
                    byte runLength = 0;
                    byte value = uncompressed[runs];
                    while (rune < length && uncompressed[rune] == value && runLength < MAX_RUN_LENGTH)
                    {
                        rune++;
                        runLength++;
                    }

                    if (runLength >= MIN_RUN_LENGTH - 1)
                    {
                        if (destIndex + 2 >= compressed.Length)
                        {
                            return CompressionResult.NoGain;
                        }
                        compressed[destIndex++] = runLength;
                        compressed[destIndex++] = value;
                        runs = rune;
                    }
                    else
                    {
                        runLength++;
                        while (rune < length &&
                            ((
                                (rune + 1 >= length) ||
                                (uncompressed[rune] != uncompressed[rune + 1])
                            ) ||
                            (
                                (rune + 2 >= length) ||
                                (uncompressed[rune + 1] != uncompressed[rune + 2])
                            )) &&
                            runLength < MAX_RUN_LENGTH)
                        {
                            runLength++;
                            rune++;
                        }

                        int count = rune - runs;
                        if (destIndex + 1 + count >= compressed.Length)
                        {
                            return CompressionResult.NoGain;
                        }
                        compressed[destIndex++] = (byte)(-runLength);
                        Array.Copy(uncompressed, runs, compressed, destIndex, count);
                        destIndex += count;

                        runs += count;
                    }
                    rune++;
                }

                dest.Write(compressed, 0, destIndex);
                return CompressionResult.Success;
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(compressed);
            }
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(uncompressed);
        }
    }

    public override void InternalDecompress(Stream source, Span<byte> dest, PixelDataInfo info)
    {
        int length = info.UncompressedByteSize;

        byte[] buffer = ArrayPool<byte>.Shared.Rent(length);
        try
        {
            int bufferIndex = 0;
            while (true)
            {
                int b = source.ReadByte();
                if (b < 0)
                {
                    // End of stream
                    break;
                }
                int runCount = (sbyte)b;
                if (runCount < 0)
                {
                    runCount = -runCount;
                    if (source.Length - source.Position < runCount)
                    {
                        throw new EXRCompressionException($"RLE run count of {runCount} would read beyond end of stream");
                    }
                    source.ReadExactly(buffer, bufferIndex, runCount);
                    bufferIndex += runCount;
                }
                else
                {
                    var value = source.ReadByte();
                    if (value < 0)
                    {
                        throw new EXRCompressionException($"Expected RLE value, but end of stream reached");
                    }
                    Array.Fill(buffer, (byte)value, bufferIndex, runCount + 1);
                    bufferIndex += runCount + 1;
                }
            }

            int outputLength = bufferIndex;
            UnpredictAndReorder(buffer, dest, outputLength);
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buffer);
        }
    }
}
