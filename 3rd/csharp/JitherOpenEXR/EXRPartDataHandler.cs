using Jither.OpenEXR.Attributes;
using Jither.OpenEXR.Compression;
using Jither.OpenEXR.Drawing;
using Jither.OpenEXR.Helpers;
using System.Drawing;

namespace Jither.OpenEXR;

public abstract class EXRPartDataHandler
{
    protected readonly EXRPart part;
    protected readonly Compressor compressor;
    public int ChunkCount { get; }
    protected readonly bool fileIsMultiPart;
    protected readonly bool fileHasDeepData;

    /// <summary>
    /// Returns the number of bytes needed to contain the part's complete pixel data. 
    /// </summary>
    /// <remarks>
    /// This will throw <see cref="EXRFormatException"/> for byte counts above 2GB. A .NET array wouldn't be able to hold the complete image.
    /// OpenEXR limits individual chunks to 2GB, but has no such limitation for the complete image. In order to read an image larger than this, use
    /// <seealso cref="GetTotalByteCountLarge"/> and process each chunk individually.
    /// </remarks>
    public int GetTotalByteCount()
    {
        try
        {
            var bounds = part.DataWindow.ToBounds();
            return part.Channels.GetByteCount(bounds);
        }
        catch (OverflowException ex)
        {
            throw new EXRFormatException($"Byte count of part '{part.Name}' exceeds 2GB", ex);
        }
    }

    /// <summary>
    /// Returns the number of bytes needed to contain the part's complete pixel data. Unlike <see cref="GetTotalByteCount"/>, this allows computing
    /// sizes (way) larger than 2GB. For multi-resolution, this returns the bytes needed for the first level (0, 0).
    /// </summary>
    public ulong GetTotalByteCountLarge()
    {
        var bounds = part.DataWindow.ToBounds();
        return part.Channels.GetByteCountLarge(bounds);
    }

    protected void CheckInterleavedPrerequisites()
    {
        if (part.Channels.AreSubsampled)
        {
            throw new NotSupportedException($"Interleaved read/write is not supported with subsampled channels.");
        }
    }

    protected EXRPartDataHandler(EXRPart part, EXRVersion version)
    {
        this.part = part;

        fileIsMultiPart = version.IsMultiPart;
        fileHasDeepData = version.HasNonImageParts;

        compressor = part.Compression switch
        {
            EXRCompression.None => new NullCompressor(),
            EXRCompression.RLE => new RLECompressor(),
            EXRCompression.ZIPS => new ZipSCompressor(),
            EXRCompression.ZIP => new ZipCompressor(),
            EXRCompression.PIZ => new PizCompressor(),
            _ => new UnsupportedCompressor(part.Compression)
        };
        
        if (fileIsMultiPart)
        {
            ChunkCount = part.GetAttributeOrThrow<int>("chunkCount");
        }
        else
        {
            ChunkCount = part.GetCalculatedChunkCount();
        }
    }
}
