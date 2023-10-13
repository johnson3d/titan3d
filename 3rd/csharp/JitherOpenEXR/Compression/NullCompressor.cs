namespace Jither.OpenEXR.Compression;

internal sealed class NullCompressor : Compressor
{
    public static NullCompressor Instance { get; } = new NullCompressor();

    public override int ScanLinesPerChunk { get; } = EXRCompression.None.GetScanLinesPerChunk();

    public override CompressionResult InternalCompress(ReadOnlySpan<byte> source, Stream dest, PixelDataInfo info)
    {
        dest.Write(source);
        return CompressionResult.Success;
    }

    public override void InternalDecompress(Stream source, Span<byte> dest, PixelDataInfo info)
    {
        source.ReadExactly(dest);
    }
}
