using Jither.OpenEXR.Attributes;
using Jither.OpenEXR.Drawing;
using System.Buffers;
using System.Runtime.InteropServices;
using System.Text;

namespace Jither.OpenEXR.Compression;

internal sealed class PizCompressor : Compressor
{
    private const int LUT_SIZE = 1 << 16;
    private const int BITMAP_SIZE = LUT_SIZE >> 3;
    public override int ScanLinesPerChunk { get; } = EXRCompression.PIZ.GetScanLinesPerChunk();

    // PIZ stream layout:
    // - ushort minNonZeroIndex
    // - ushort maxNonZeroIndex
    // - byte[minNonZeroIndex .. maxNonZeroIndex] bitmap
    // - compressed data

    private class ChannelInfo
    {
        public int YSampling { get; }
        public Dimensions<int> Resolution { get; }
        public int UShortsPerPixel { get; }
        public int UShortsPerLine { get; }
        public int BytesPerLine { get; }
        public int UShortsTotal { get; }
        public int StartUShortOffset { get; set; }
        public int NextScanLineByteOffset { get; set; }
        public int NextScanLineUShortOffset { get; set; }

        public ChannelInfo(Channel channel, Bounds<int> bounds)
        {
            YSampling = channel.YSampling;
            Resolution = channel.GetSubsampledResolution(bounds);
            UShortsPerPixel = channel.Type.GetBytesPerPixel() / 2;
            UShortsPerLine =  Resolution.X * UShortsPerPixel;
            BytesPerLine = UShortsPerLine * 2;
            UShortsTotal = Resolution.Area * UShortsPerPixel;
        }
    }

    public override CompressionResult InternalCompress(ReadOnlySpan<byte> source, Stream dest, PixelDataInfo info)
    {
        // TODO: Bit of a nested nightmare here, due to ArrayPool returns.

        var channelInfos = CreateChannelInfos(info);
        int uncompressedUShortSize = info.UncompressedByteSize / 2;

        // PIZ uncompressed data should store all scanlines for each channel consecutively -
        // e.g. A for all scanlines, followed by B for all scanlines, followed by G, followed by R.
        // Here we do the rearrangement:
        var uncompressedArray = ArrayPool<ushort>.Shared.Rent(uncompressedUShortSize);
        try
        {
            // The array may (mostly will) not be the size of the data. We use a span over the exact size for convenience
            var uncompressed = uncompressedArray.AsSpan(0, uncompressedUShortSize);

            int nextByteOffset = 0;
            for (int y = info.Bounds.Top; y < info.Bounds.Bottom; y++)
            {
                foreach (var channel in channelInfos)
                {
                    if (y % channel.YSampling != 0)
                    {
                        continue;
                    }
                    var byteSize = channel.BytesPerLine;
                    var ushortSize = channel.UShortsPerLine;
                    var channelScanline = MemoryMarshal.Cast<byte, ushort>(source.Slice(nextByteOffset, byteSize));
                    var resultSpan = uncompressed.Slice(channel.NextScanLineUShortOffset, ushortSize);
                    channel.NextScanLineUShortOffset += ushortSize;
                    channelScanline.CopyTo(resultSpan);
                    nextByteOffset += byteSize;
                }
            }

            byte[] bitmapArray = ArrayPool<byte>.Shared.Rent(BITMAP_SIZE);
            try
            {
                var bitmap = bitmapArray.AsSpan(0, BITMAP_SIZE);
                // Bitmap is sparsely populated, so need to clear it before use
                bitmap.Clear();

                (var minNonZero, var maxNonZero) = BitmapFromData(uncompressed, bitmap);
                ushort[] lutArray = ArrayPool<ushort>.Shared.Rent(LUT_SIZE);
                try
                {
                    var lut = lutArray.AsSpan(0, LUT_SIZE);

                    var maxValue = ForwardLUTFromBitmap(bitmap, lut);
                    ApplyLUT(lut, uncompressed);

                    foreach (var channelInfo in channelInfos)
                    {
                        var data = uncompressed.Slice(channelInfo.StartUShortOffset, channelInfo.UShortsTotal);
                        // For 32 bit channels, each half is transformed separately:
                        for (int offset = 0; offset < channelInfo.UShortsPerPixel; offset++)
                        {
                            Wavelet.Encode2D(
                                data[offset..],
                                channelInfo.Resolution.X,
                                channelInfo.UShortsPerPixel,
                                channelInfo.Resolution.Y,
                                channelInfo.UShortsPerLine,
                                maxValue
                            );
                        }
                    }
                }
                finally
                {
                    ArrayPool<ushort>.Shared.Return(lutArray);
                }

                var compressed = HuffmanCoding.Compress(uncompressed);

                int bitmapSize = 4 + maxNonZero - minNonZero + 1;
                if (compressed.Length + bitmapSize + 4 >= info.UncompressedByteSize)
                {
                    return CompressionResult.NoGain;
                }

                using (var writer = new BinaryWriter(dest, Encoding.UTF8, leaveOpen: true))
                {
                    writer.Write((ushort)minNonZero);
                    writer.Write((ushort)maxNonZero);
                    // Bitmap is stored only from the first to the last non-zero value
                    // If first is larger than last non-zero value, the bitmap is all 0, and we don't store anything
                    if (minNonZero <= maxNonZero)
                    {
                        writer.Write(bitmap.Slice(minNonZero, maxNonZero - minNonZero + 1));
                    }
                    writer.Write(compressed.Length);
                    writer.Write(compressed);

                    return CompressionResult.Success;
                }
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(bitmapArray);
            }
        }
        finally
        {
            ArrayPool<ushort>.Shared.Return(uncompressedArray);
        }
    }

    public override void InternalDecompress(Stream source, Span<byte> dest, PixelDataInfo info)
    {
        // TODO: Bit of a nested nightmare here, due to ArrayPool returns.
        int uncompressedUShortSize = info.UncompressedByteSize / 2;

        var lutArray = ArrayPool<ushort>.Shared.Rent(LUT_SIZE);
        try
        {
            var decompressedArray = ArrayPool<ushort>.Shared.Rent(uncompressedUShortSize);
            try
            {
                var lut = lutArray.AsSpan(0, LUT_SIZE);
                var decompressed = decompressedArray.AsSpan(0, uncompressedUShortSize);

                // Read data:

                var maxValue = ReadAndDecompress(source, decompressed, lut);

                var channelInfos = CreateChannelInfos(info);

                // Wavelet transform:

                foreach (var channelInfo in channelInfos)
                {
                    var data = decompressed.Slice(channelInfo.StartUShortOffset, channelInfo.UShortsTotal);
                    // For 32 bit channels, each half is transformed seperately:
                    for (int offset = 0; offset < channelInfo.UShortsPerPixel; offset++)
                    {
                        Wavelet.Decode2D(
                            data[offset..],
                            channelInfo.Resolution.X,
                            channelInfo.UShortsPerPixel,
                            channelInfo.Resolution.Y,
                            channelInfo.UShortsPerLine,
                            maxValue
                        );
                    }
                }

                ApplyLUT(lut, decompressed);

                // TODO: Handle Big Endian
                var decompressedAsBytes = MemoryMarshal.AsBytes(decompressed);

                // The uncompressed PIZ data stores all scanlines for each channel consecutively -
                // e.g. A for all scanlines, followed by B for all scanlines, followed by G, followed by R.
                // Split into scanlines, each containing its own (e.g.) ABGR channels.
                int destIndex = 0;
                for (int y = info.Bounds.Top; y < info.Bounds.Bottom; y++)
                {
                    foreach (var channel in channelInfos)
                    {
                        if (y % channel.YSampling != 0)
                        {
                            continue;
                        }
                        var size = channel.BytesPerLine;
                        var channelScanline = decompressedAsBytes.Slice(channel.NextScanLineByteOffset, size);
                        channel.NextScanLineByteOffset += size;
                        var resultSpan = dest.Slice(destIndex, size);
                        channelScanline.CopyTo(resultSpan);
                        destIndex += size;
                    }
                }
            }
            finally
            {
                ArrayPool<ushort>.Shared.Return(decompressedArray);
            }
        }
        finally
        {
            ArrayPool<ushort>.Shared.Return(lutArray);
        }
    }

    private ushort ReadAndDecompress(Stream source, Span<ushort> decompressed, Span<ushort> lut)
    {
        // TODO: Bit of a nested nightmare here, due to ArrayPool returns.

        ushort minNonZeroIndex, maxNonZeroIndex;

        using (var reader = new BinaryReader(source, Encoding.UTF8, leaveOpen: true))
        {
            minNonZeroIndex = reader.ReadUInt16();
            maxNonZeroIndex = reader.ReadUInt16();

            if (minNonZeroIndex >= BITMAP_SIZE || maxNonZeroIndex >= BITMAP_SIZE)
            {
                throw new EXRCompressionException($"Error in PIZ data: min/max non-zero indices exceed bitmap size");
            }

            byte[] bitmap = ArrayPool<byte>.Shared.Rent(BITMAP_SIZE);
            try
            {
                Array.Fill<byte>(bitmap, 0, 0, BITMAP_SIZE);
                // Bitmap is stored only from the first to the last non-zero value
                // If first is larger than last non-zero value, the bitmap is all 0.
                if (minNonZeroIndex <= maxNonZeroIndex)
                {
                    source.ReadExactly(bitmap, minNonZeroIndex, maxNonZeroIndex - minNonZeroIndex + 1);
                }

                int compressedByteSize = reader.ReadInt32();

                byte[] compressed = ArrayPool<byte>.Shared.Rent(compressedByteSize);
                try
                {
                    reader.Read(compressed, 0, compressedByteSize);

                    var maxValue = ReverseLUTFromBitmap(bitmap, lut);

                    HuffmanCoding.Decompress(compressed, decompressed, compressedByteSize);

                    return maxValue;
                }
                finally
                {
                    ArrayPool<byte>.Shared.Return(compressed);
                }
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(bitmap);
            }
        }
    }

    private static List<ChannelInfo> CreateChannelInfos(PixelDataInfo info)
    {
        var channelInfos = info.Channels.Select(c => new ChannelInfo(c, info.Bounds)).ToList();
        int channelStart = 0;
        foreach (var channelInfo in channelInfos)
        {
            channelInfo.StartUShortOffset = channelStart;
            channelInfo.NextScanLineByteOffset = channelStart * 2;
            channelInfo.NextScanLineUShortOffset = channelStart;
            channelStart += channelInfo.UShortsTotal;
        }

        return channelInfos;
    }

    private static (int minIndexNonZero, int maxIndexNonZero) BitmapFromData(Span<ushort> data, Span<byte> bitmap)
    {
        int minIndexNonZero = BITMAP_SIZE - 1;
        int maxIndexNonZero = 0;

        for (int i = 0; i < data.Length; i++)
        {
            ushort value = data[i];
            bitmap[value >> 3] |= (byte)(1 << (value & 0b111));
        }

        bitmap[0] &= 0b11111110; // Clear bit for 0 - "zero is not explicitly stored in the bitmap; we assume that the data always contain zeroes"

        for (ushort i = 0; i < BITMAP_SIZE; i++)
        {
            if (bitmap[i] != 0)
            {
                if (minIndexNonZero > i)
                {
                    minIndexNonZero = i;
                }
                if (maxIndexNonZero < i)
                {
                    maxIndexNonZero = i;
                }
            }
        }

        return (minIndexNonZero, maxIndexNonZero);
    }

    private static ushort ForwardLUTFromBitmap(Span<byte> bitmap, Span<ushort> lut)
    {
        ushort k = 0;

        for (int i = 0; i < LUT_SIZE; i++)
        {
            if (i == 0 || (bitmap[i >> 3] & (1 << (i & 0b111))) != 0)
            {
                lut[i] = k++;
            }
            else
            {
                lut[i] = 0;
            }
        }

        return (ushort)(k - 1);
    }

    private static ushort ReverseLUTFromBitmap(Span<byte> bitmap, Span<ushort> lut)
    {
        int n;
        int k = 0;

        for (int i = 0; i < LUT_SIZE; i++)
        {
            if (i == 0 || (bitmap[i >> 3] & (1 << (i & 0b111))) != 0)
            {
                lut[k++] = (ushort)i;
            }
        }

        n = k - 1;

        while (k < LUT_SIZE)
        {
            lut[k++] = 0;
        }

        return (ushort)n;
    }

    private static void ApplyLUT(Span<ushort> lut, Span<ushort> data)
    {
        for (int i = 0; i < data.Length; i++)
        {
            data[i] = lut[data[i]];
        }
    }
}
