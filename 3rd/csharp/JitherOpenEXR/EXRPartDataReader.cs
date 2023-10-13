using Jither.OpenEXR.Attributes;
using Jither.OpenEXR.Compression;
using Jither.OpenEXR.Converters;
using Jither.OpenEXR.Drawing;
using System.Buffers;

namespace Jither.OpenEXR;

public class EXRPartDataReader : EXRPartDataHandler
{
    private OffsetTable? _offsets;

    private readonly EXRReader reader;
    private readonly long offsetTableOffset;

    private OffsetTable OffsetTable
    {
        get
        {
            if (_offsets != null)
            {
                return _offsets;
            }

            reader.Seek(offsetTableOffset);
            _offsets = OffsetTable.ReadFrom(reader, ChunkCount);
            return _offsets;
        }
    }

    public EXRPartDataReader(EXRPart part, EXRVersion version, EXRReader reader, long offsetTableOffset) : base(part, version)
    {
        this.reader = reader;
        this.offsetTableOffset = offsetTableOffset;
    }

    /// <summary>
    /// Reads the image data from the part into a scanline-interleaved array (the standard OpenEXR image data layout).
    /// </summary>
    /// <remarks>
    /// Scanline-interleaved means that channels are stored separately for each scanline and sorted in alphabetical order.
    /// In other words, a 5x2 pixel RGBA image will be stored as: AAAAA BBBBB GGGGG RRRRR AAAAA BBBBB GGGGG RRRRR.
    /// </remarks>
    public void Read(Span<byte> dest)
    {
        if (part.IsTiled)
        {
            // This methods reads the first level of multi-resolution tiled parts
            Read(dest, 0, 0);
            return;
        }

        if (dest == null)
        {
            throw new ArgumentNullException(nameof(dest));
        }

        part.ValidateAttributes(fileIsMultiPart, fileHasDeepData);

        var totalBytes = GetTotalByteCount();
        if (dest.Length < totalBytes)
        {
            throw new ArgumentException($"Destination array too small ({dest.Length}) to fit pixel data ({totalBytes})");
        }

        int destIndex = 0;
        for (int i = 0; i < ChunkCount; i++)
        {
            var chunkInfo = ReadChunkHeader(i);
            InternalReadChunk(chunkInfo, dest[destIndex..]);
            destIndex += chunkInfo.UncompressedByteCount;
        }
    }

    /// <summary>
    /// Reads the image data from the given multi-resolution level into a scanline-interleaved array (the standard OpenEXR image data layout).
    /// </summary>
    public void Read(Span<byte> dest, int xLevel, int yLevel)
    {
        if (!part.IsTiled || part.Tiles == null)
        {
            throw new InvalidOperationException("Attempt to read tiled level from non-tiled part.");
        }

        var tilingInfo = part.TilingInformation;
        var level = tilingInfo.GetLevel(xLevel, yLevel);
        Read(dest, level);
    }

    /// <summary>
    /// Reads the image data from the given multi-resolution level into a scanline-interleaved array (the standard OpenEXR image data layout).
    /// </summary>
    public void Read(Span<byte> dest, TilingLevel level)
    {
        if (dest == null)
        {
            throw new ArgumentNullException(nameof(dest));
        }

        part.ValidateAttributes(fileIsMultiPart, fileHasDeepData);

        // The position of the bounds doesn't matter here - since tiles do not support sub-sampling, all pixels have the same byte size.
        var totalBytes = level.TotalByteCount;
        if (dest.Length < totalBytes)
        {
            throw new ArgumentException($"Destination array too small ({dest.Length}) to fit pixel data ({totalBytes})");
        }

        if (part.Tiles == null)
        {
            throw new EXRFormatException($"Tiled part is missing 'tiles' attribute");
        }

        byte[] tileDest = ArrayPool<byte>.Shared.Rent(part.Channels.GetByteCount(new Bounds<int>(0, 0, part.Tiles.XSize, part.Tiles.YSize)));
        try
        {
            for (int i = level.FirstChunkIndex; i < level.FirstChunkIndex + level.ChunkCount; i++)
            {
                var chunkInfo = ReadChunkHeader(i, level);
                InternalReadChunk(chunkInfo, tileDest.AsSpan(0, chunkInfo.UncompressedByteCount));
                if (chunkInfo is not TileChunkInfo tileChunkInfo)
                {
                    throw new InvalidOperationException($"{chunkInfo} is not a tile chunk");
                }
                DrawTile(part.Tiles, level, tileChunkInfo, tileDest, dest);
            }
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(tileDest);
        }
    }

    private void DrawTile(TileDesc tiles, TilingLevel level, TileChunkInfo chunkInfo, Span<byte> tile, Span<byte> dest)
    {
        int destX = chunkInfo.X;
        int destY = chunkInfo.Y;
        int destWidth = level.DataWindow.Width;
        int destHeight = level.DataWindow.Height;
        int tileWidth = Math.Min(tiles.XSize, destWidth - destX);
        int tileHeight = Math.Min(tiles.YSize, destHeight - destY);
        int bytesPerPixel = part.Channels.BytesPerPixelNoSubSampling;
        int bytesPerTileScanline = bytesPerPixel * tileWidth;
        int bytesPerDestScanline = bytesPerPixel * destWidth;
        int channelCount = part.Channels.Count;

        int[] bytesPerChannel = new int[channelCount];
        int[] bytesPerTileScanlineChannel = new int[channelCount];
        int[] bytesPerDestScanlineChannel = new int[channelCount];

        int channelIndex = 0;
        foreach (var channel in part.Channels)
        {
            // We don't need to deal with sub-sampling - OpenEXR tiled images don't support sub-sampling
            int byteCount = channel.BytesPerPixelNoSubSampling;
            bytesPerChannel[channelIndex] = byteCount;
            bytesPerTileScanlineChannel[channelIndex] = byteCount * tileWidth;
            bytesPerDestScanlineChannel[channelIndex] = byteCount * destWidth;
            channelIndex++;
        }

        int tileOffset = 0;
        int destOffset = destY * bytesPerDestScanline;

        for (int tileY = 0; tileY < tileHeight; tileY++)
        {
            // Offset from start of tile scanline to start of current channel data
            int tileChannelOffset = 0;
            
            // Offset from start of destination scanline to start of current channel data. Remember that we're dealing with
            // scanline interleaved channels. Example:
            //
            // AAAAAAAAAAAAAAAAAAAAA
            // BBBBBBBBBBBBBBBBBBBBB
            // GGGGGGGGGGGGGGGGGGGGG
            // RRRRRRRRRRRRRRRRRRRRR
            //
            // We'll want to start writing after the first channel's (here, alpha/A) samples of the pixels to the left of the tile.
            // Hence, x coordinate of tile within destination multiplied by the size of the first channel's samples:
            int destChannelOffset = destX * bytesPerChannel[0];
            
            for (int i = 0; i < channelCount; i++)
            {
                // Write 1 channel from 1 scanline:
                int count = bytesPerTileScanlineChannel[i];

                tile.Slice(tileOffset + tileChannelOffset, count).CopyTo(dest[(destOffset + destChannelOffset)..]);
                tileChannelOffset += count;
                // Place next offset at the same x position within the next channel's data:
                destChannelOffset += bytesPerDestScanlineChannel[i];
            }
            destY++;
            // Move 1 scanline forward, for the tile and dest respectively
            tileOffset += bytesPerTileScanline;
            destOffset += bytesPerDestScanline;
        }
    }

    /// <summary>
    /// Reads image data from a single chunk (tile or scanline block) into a raw array (the standard OpenEXR image data layout).
    /// </summary>
    public ChunkInfo ReadChunk(int chunkIndex, Span<byte> dest)
    {
        part.ValidateAttributes(fileIsMultiPart, fileHasDeepData);

        var chunkInfo = ReadChunkHeader(chunkIndex);
        InternalReadChunk(chunkInfo, dest);
        return chunkInfo;
    }

    public void ReadInterleaved(Span<byte> dest, string[] channelOrder)
    {
        part.ValidateAttributes(fileIsMultiPart, fileHasDeepData);

        CheckInterleavedPrerequisites();

        if (dest == null)
        {
            throw new ArgumentNullException(nameof(dest));
        }

        var converter = new PixelInterleaveConverter(part.Channels, channelOrder);
        int destIndex = 0;
        for (int chunkIndex = 0; chunkIndex < ChunkCount; chunkIndex++)
        {
            var chunkInfo = ReadChunkHeader(chunkIndex);
            var pixelData = ArrayPool<byte>.Shared.Rent(chunkInfo.UncompressedByteCount);
            try
            {
                InternalReadChunk(chunkInfo, pixelData);
                converter.FromEXR(chunkInfo.GetBounds(), pixelData.AsSpan(0, chunkInfo.UncompressedByteCount), dest[destIndex..]);
                destIndex += chunkInfo.UncompressedByteCount;
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(pixelData);
            }
        }
    }

    public ChunkInfo ReadChunkInterleaved(int chunkIndex, Span<byte> dest, string[] channelOrder)
    {
        part.ValidateAttributes(fileIsMultiPart, fileHasDeepData);

        CheckInterleavedPrerequisites();

        var converter = new PixelInterleaveConverter(part.Channels, channelOrder);

        var chunkInfo = ReadChunkHeader(chunkIndex);
        var pixelData = ArrayPool<byte>.Shared.Rent(chunkInfo.UncompressedByteCount);
        try
        {
            InternalReadChunk(chunkInfo, pixelData);
            converter.FromEXR(chunkInfo.GetBounds(), pixelData.AsSpan(0, chunkInfo.UncompressedByteCount), dest);
            return chunkInfo;
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(pixelData);
        }
    }

    private ChunkInfo ReadChunkHeader(int chunkIndex, TilingLevel? level = null)
    {
        var offset = OffsetTable[chunkIndex];

        reader.Seek(offset);
        int partNumber = fileIsMultiPart ? reader.ReadInt() : 0;

        if (partNumber != part.PartNumber)
        {
            throw new EXRFormatException($"Read unexpected part number for chunk {chunkIndex}. Reading for part {part.PartNumber} but chunk is for part {partNumber}.");
        }

        ChunkInfo chunkInfo;
        if (part.IsTiled)
        {
            if (reader.Remaining < 16)
            {
                throw new EXRFormatException($"Truncated chunk header - expected at least 16 bytes, was: {reader.Remaining}");
            }
            int x = reader.ReadInt();
            int y = reader.ReadInt();
            int levelX = reader.ReadInt();
            int levelY = reader.ReadInt();

            level ??= part.TilingInformation.GetLevel(0, 0);

            chunkInfo = new TileChunkInfo(part, chunkIndex, level, x, y, levelX, levelY);
        }
        else
        {
            if (reader.Remaining < 4)
            {
                throw new EXRFormatException($"Truncated chunk header - expected at least 4 bytes, was: {reader.Remaining}");
            }
            int y = reader.ReadInt();
            chunkInfo = new ScanlineChunkInfo(part, chunkIndex, y);
        }

        chunkInfo.CompressedByteCount = reader.ReadInt();

        chunkInfo.PixelDataFileOffset = reader.Position;
        chunkInfo.FileOffset = offset;

        return chunkInfo;
    }

    private void InternalReadChunk(ChunkInfo chunkInfo, Span<byte> dest)
    {
        reader.Seek(chunkInfo.PixelDataFileOffset);
        var chunkStream = reader.GetChunkStream(chunkInfo.CompressedByteCount);

        if (chunkInfo.UncompressedByteCount > dest.Length)
        {
            throw new EXRFormatException($"Uncompressed byte count for {chunkInfo} ({chunkInfo.UncompressedByteCount}) exceeds expected size (max {dest.Length}");
        }

        // Yes, compressors could use the length or capacity of the stream rather than
        // an explicit expectedBytes parameter, but not sure if we'll change this
        // implementation in the future.
        var info = new PixelDataInfo(
            part.Channels,
            chunkInfo.GetBounds(),
            chunkInfo.UncompressedByteCount
        );

        compressor.Decompress(chunkStream, dest[..chunkInfo.UncompressedByteCount], info);
    }
}
