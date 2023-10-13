using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jither.OpenEXR.Compression;

internal sealed class UnsupportedCompressor : Compressor
{
    private readonly EXRCompression type;

    public override int ScanLinesPerChunk => type.GetScanLinesPerChunk();

    public UnsupportedCompressor(EXRCompression type)
    {
        this.type = type;
    }

    public override CompressionResult InternalCompress(ReadOnlySpan<byte> source, Stream dest, PixelDataInfo info)
    {
        throw new NotSupportedException($"{type} compression is not supported.");
    }

    public override void InternalDecompress(Stream source, Span<byte> dest, PixelDataInfo info)
    {
        throw new NotSupportedException($"{type} compression is not supported.");
    }
}
