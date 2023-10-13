using Jither.OpenEXR.Compression;
using static System.Net.Mime.MediaTypeNames;
using static System.Reflection.Metadata.BlobBuilder;
using System;

namespace Jither.OpenEXR;

public enum EXRCompression
{
    /// <summary>
    /// No compression.
    /// </summary>
    None = 0,

    /// <summary>
    /// Lossless.
    /// Differences between horizontally adjacent pixels are run-length encoded.
    /// </summary>
    /// <remarks>
    /// This method is fast, and works well for images with large flat areas,
    /// but for photographic images, the compressed file size is usually between 60 and 75 percent of the uncompressed size.
    /// </remarks>
    RLE = 1,

    /// <summary>
    /// Lossless.
    /// Uses zlib for compression, similarly to the open source zlib library. Unlike ZIP compression, this operates one scan line at a time.
    /// </summary>
    ZIPS = 2,

    /// <summary>
    /// Lossless.
    /// Differences between horizontally adjacent pixels are compressed using the open source zlib library.ZIP decompression is faster than PIZ decompression,
    /// but ZIP compression is significantly slower.
    /// Unlike ZIPS compression, this operates in in blocks of 16 scan lines.
    /// </summary>
    /// <remarks>
    /// Photographic images tend to shrink to between 45 and 55 percent of their uncompressed size. Multi-resolution files are often used as texture maps for 3D renderers.
    /// For this application, fast read accesses are usually more important than fast writes, or maximum compression.For texture maps, ZIP is probably the best compression method.
    /// </remarks>
    ZIP = 3,

    /// <summary>
    /// Lossless.
    /// A wavelet transform is applied to the pixel data, and the result is Huffman-encoded. Files are compressed and decompressed at roughly the same speed.
    /// <remarks>
    /// This scheme tends to provide the best compression ratio for the types of images that are typically processed at Industrial Light & Magic.
    /// For photographic images with film grain, the files are reduced to between 35 and 55 percent of their uncompressed size.
    /// PIZ compression works well for scan line based files, and also for tiled files with large tiles, but small tiles do not shrink much.
    /// (PIZ-compressed data start with a relatively long header; if the input to the compressor is short, adding the header tends to offset any size reduction of the input.)
    /// </remarks>
    PIZ = 4,

    /// <summary>
    /// Lossy for float, lossless for uint and half.
    /// After reducing 32-bit floating-point data to 24 bits by rounding, differences between horizontally adjacent pixels are compressed with zlib, similar to ZIP.
    /// PXR24 compression preserves image channels of type HALF and UINT exactly, but the relative error of FLOAT data increases to about 3e-5.
    /// </summary>
    /// <remarks>
    /// This compression method works well for depth buffers and similar images, where the possible range of values is very large, but where full 32-bit floating-point
    /// accuracy is not necessary. Rounding improves compression significantly by eliminating the pixels’ 8 least significant bits, which tend to be very noisy, and
    /// difficult to compress.
    /// Note: This lossy compression scheme is not supported in deep files.
    /// </remarks>
    PXR24 = 5,

    /// <summary>
    /// Lossy for half, uint and float are not compressed.
    /// Channels of type HALF are split into blocks of four by four pixels or 32 bytes. Each block is then packed into 14 bytes, reducing the data to 44 percent of their
    /// uncompressed size. When B44 compression is applied to RGB images in combination with luminance/chroma encoding (see below), the size of the compressed pixels
    /// is about 22 percent of the size of the original RGB data. Channels of type UINT or FLOAT are not compressed.
    /// </summary>
    /// <remarks>
    /// Decoding is fast enough to allow real-time playback of B44-compressed OpenEXR image sequences on commodity hardware. The size of a B44-compressed file depends on
    /// the number of pixels in the image, but not on the data in the pixels. All files with the same resolution and the same set of channels have the same size. This can be
    /// advantageous for systems that support real-time playback of image sequences; the predictable file size makes it easier to allocate space on storage media efficiently.
    /// Note: This lossy compression scheme is not supported in deep files.
    /// </remarks>
    B44 = 6,

    /// <summary>
    /// Lossy for half, uint and float are not compressed.
    /// Like B44, except for blocks of four by four pixels where all pixels have the same value, which are packed into 3 instead of 14 bytes. For images with large uniform areas,
    /// B44A produces smaller files than B44 compression.
    /// </summary>
    /// <remarks>
    /// Note: This lossy compression scheme is not supported in deep files.
    /// </remarks>
    B44A = 7,

    /// <summary>
    /// Lossy for RGB data.
    /// Contributed by Dreamworks Animation. Compresses RGB channels using DCT quantization somewhat similar to JPEG.
    /// </summary>
    DWAA = 8,

    /// <summary>
    /// Lossy for RGB data.
    /// Contributed by Dreamworks Animation. Compresses RGB channels using DCT quantization somewhat similar to JPEG.
    /// </summary>
    DWAB = 9,
}

public static class EXRCompressionExtensions
{
    public static int GetScanLinesPerChunk(this EXRCompression compression)
    {
        return compression switch
        {
            EXRCompression.None => 1,
            EXRCompression.RLE => 1,
            EXRCompression.ZIPS => 1,
            EXRCompression.ZIP => 16,
            EXRCompression.PIZ => 32,
            EXRCompression.PXR24 => 16,
            EXRCompression.B44 => 32,
            EXRCompression.B44A => 32,
            EXRCompression.DWAA => 32,
            EXRCompression.DWAB => 256,
            _ => throw new NotImplementedException($"{nameof(GetScanLinesPerChunk)} not implemented for {compression}")
        }; ;
    }
}