using System.Buffers;
using System.IO.Compression;

namespace Jither.OpenEXR.Compression;

internal sealed class ZipSCompressor : ZipCompressor
{
    public override int ScanLinesPerChunk { get; } = EXRCompression.ZIPS.GetScanLinesPerChunk();
}
