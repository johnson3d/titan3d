using System.Buffers;
using System.Buffers.Binary;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Jither.OpenEXR.Compression;

internal static class HuffmanCoding
{
    private const int HUF_ENCBITS = 16;
    private const int HUF_DECBITS = 14;
    private const int HUF_ENCSIZE = (1 << HUF_ENCBITS) + 1;
    private const int HUF_DECSIZE = 1 << HUF_DECBITS;
    private const int HUF_DECMASK = HUF_DECSIZE - 1;

    private const int SHORT_ZEROCODE_RUN = 59;
    private const int LONG_ZEROCODE_RUN = 63;
    private const int SHORTEST_LONG_RUN = 2 + LONG_ZEROCODE_RUN - SHORT_ZEROCODE_RUN;
    private const int LONGEST_LONG_RUN = 255 + SHORTEST_LONG_RUN;

    public static byte[] Compress(Span<ushort> uncompressed)
    {
        if (uncompressed.Length == 0)
        {
            return Array.Empty<byte>();
        }

        ulong[] frequencies = CountFrequencies(uncompressed);
        (int minIndex, int maxIndex) = BuildEncodingTable(frequencies);

        using (var stream = new MemoryStream())
        {
            const int headerSize = 5 * sizeof(uint);
            var compressed = new BitStreamer(stream);
            stream.Position += headerSize;  // Leaving room for header

            long tableStart = compressed.Position;

            PackEncodingTable(frequencies, minIndex, maxIndex, compressed);

            long dataStart = compressed.Position;

            int bitCount = Encode(frequencies, uncompressed, compressed, maxIndex);

            stream.Position = 0;
            var header = ArrayPool<byte>.Shared.Rent(headerSize);
            try
            {
                uint tableLength = (uint)(dataStart - tableStart);
                var headerSpan = header.AsSpan(0, headerSize);
                BinaryPrimitives.WriteUInt32LittleEndian(headerSpan[..4], (uint)minIndex);
                BinaryPrimitives.WriteUInt32LittleEndian(headerSpan.Slice(4, 4), (uint)maxIndex);
                BinaryPrimitives.WriteUInt32LittleEndian(headerSpan.Slice(8, 4), tableLength);
                BinaryPrimitives.WriteUInt32LittleEndian(headerSpan.Slice(12, 4), (uint)bitCount);
                BinaryPrimitives.WriteUInt32LittleEndian(headerSpan.Slice(16, 4), 0);

                stream.Write(headerSpan);
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(header);
            }

            return stream.ToArray();
        }
    }

    public static void Decompress(byte[] compressed, Span<ushort> decompressed, int compressedByteSize)
    {
        const int headerSize = 5 * sizeof(uint);

        if (compressed.Length < headerSize)
        {
            throw new EXRCompressionException($"PIZ Huffman coding header truncated");
        }
        var headerSpan = compressed.AsSpan(0, headerSize);
        var minIndex = BinaryPrimitives.ReadUInt32LittleEndian(headerSpan[..4]);
        var maxIndex = BinaryPrimitives.ReadUInt32LittleEndian(headerSpan.Slice(4, 4));
        var tableLength = BinaryPrimitives.ReadUInt32LittleEndian(headerSpan.Slice(8, 4));
        var bitCount = BinaryPrimitives.ReadUInt32LittleEndian(headerSpan.Slice(12, 4));
        var _ = BinaryPrimitives.ReadUInt32LittleEndian(headerSpan.Slice(16, 4));

        if (minIndex >= HUF_ENCSIZE || maxIndex >= HUF_ENCSIZE)
        {
            throw new EXRCompressionException($"PIZ invalid Huffman table size ({minIndex}..{maxIndex} exceeds encoding table size of {HUF_ENCSIZE})");
        }

        int compressedDataSize = compressedByteSize - headerSize;
        int byteCount = (int)((bitCount + 7) / 8);
        if (byteCount > compressedDataSize)
        {
            throw new EXRCompressionException($"PIZ declared bitcount ({bitCount} bits = {byteCount} bytes) does not fit in Huffman table+data length ({compressedDataSize} bytes)");
        }

        using (var stream = new MemoryStream(compressed, 0, compressedByteSize))
        {
            stream.Position = headerSize;
            var compressedBits = new BitStreamer(stream);
            var encodingTable = UnpackEncodingTable(compressedBits, minIndex, maxIndex);
            var decodingTable = BuildDecodingTable(encodingTable, minIndex, maxIndex);

            Decode(encodingTable, decodingTable, compressedBits, decompressed, bitCount, maxIndex);
        }
    }

    private static ulong[] CountFrequencies(Span<ushort> data)
    {
        ulong[] result = new ulong[HUF_ENCSIZE];
        for (int i = 0; i < data.Length; i++)
        {
            result[data[i]]++;
        }
        return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int HufLength(ulong code)
    {
        return (int)(code & 0b111111);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ulong HufCode(ulong code)
    {
        return code >> 6;
    }

    // Build a "canonical" Huffman code table:
    //	- for each (uncompressed) symbol, hcode contains the length
    //	  of the corresponding code (in the compressed data)
    //	- canonical codes are computed and stored in hcode
    //	- the rules for constructing canonical codes are as follows:
    //	  * shorter codes (if filled with zeroes to the right)
    //	    have a numerically higher value than longer codes
    //	  * for codes with the same length, numerical values
    //	    increase with numerical symbol values
    //	- because the canonical code table can be constructed from
    //	  symbol lengths alone, the code table can be transmitted
    //	  without sending the actual code values
    //	- see http://www.compressconsult.com/huffman/
    private static void BuildCanonicalCodeTable(ulong[] hcode)
    {
        ulong[] countByCode = ArrayPool<ulong>.Shared.Rent(59);
        try
        {
            Array.Fill(countByCode, 0u);

            // For each i from 0 through 58, count the
            // number of different codes of length i, and
            // store the count in n[i].
            for (int i = 0; i < HUF_ENCSIZE; i++)
            {
                countByCode[hcode[i]]++;
            }

            // For each i from 58 through 1, compute the
            // numerically lowest code with length i, and
            // store that code in n[i].

            ulong code = 0;
            for (int i = 58; i > 0; i--)
            {
                ulong nextCode = (code + countByCode[i]) >> 1;
                countByCode[i] = code;
                code = nextCode;
            }

            // hcode[i] contains the length, l, of the
            // code for symbol i.  Assign the next available
            // code of length l to the symbol and store both
            // l and the code in hcode[i].
            for (int i = 0; i < HUF_ENCSIZE; i++)
            {
                ulong currentLength = hcode[i];
                if (currentLength > 0)
                {
                    hcode[i] = currentLength | (countByCode[currentLength] << 6);
                    countByCode[currentLength]++;
                }
            }
        }
        finally
        {
            ArrayPool<ulong>.Shared.Return(countByCode);
        }
    }

    // This function assumes that when it is called, array frq
    // indicates the frequency of all possible symbols in the data
    // that are to be Huffman-encoded.  (frq[i] contains the number
    // of occurrences of symbol i in the data.)
    //
    // The loop below does three things:
    //
    // 1) Finds the minimum and maximum indices that point
    //    to non-zero entries in frq:
    //
    //     frq[im] != 0, and frq[i] == 0 for all i < im
    //     frq[iM] != 0, and frq[i] == 0 for all i > iM
    //
    // 2) Fills array fHeap with pointers to all non-zero
    //    entries in frq.
    //
    // 3) Initializes array hlink such that hlink[i] == i
    //    for all array entries.
    private static (int min, int max) BuildEncodingTable(ulong[] frequencies)
    {
        int count = 0;
        int min = 0;
        int max = 0;

        int[] links = new int[HUF_ENCSIZE];
        int[] heap = new int[HUF_ENCSIZE];

        while (frequencies[min] == 0)
        {
            min++;
        }

        for (int i = min; i < HUF_ENCSIZE; i++)
        {
            links[i] = i;

            if (frequencies[i] != 0)
            {
                heap[count++] = i;
                max = i;
            }
        }

        // Add a pseudo-symbol, with a frequency count of 1, to frq;
        // adjust the fHeap and hlink array accordingly.  Function
        // Encode() uses the pseudo-symbol for run-length encoding.

        max++;
        frequencies[max] = 1;
        heap[count++] = max;

        // Build an array, scode, such that scode[i] contains the number
        // of bits assigned to symbol i.  Conceptually this is done by
        // constructing a tree whose leaves are the symbols with non-zero
        // frequency:
        //
        //     Make a heap that contains all symbols with a non-zero frequency,
        //     with the least frequent symbol on top.
        //
        //     Repeat until only one symbol is left on the heap:
        //
        //         Take the two least frequent symbols off the top of the heap.
        //         Create a new node that has first two nodes as children, and
        //         whose frequency is the sum of the frequencies of the first
        //         two nodes.  Put the new node back into the heap.
        //
        // The last node left on the heap is the root of the tree.  For each
        // leaf node, the distance between the root and the leaf is the length
        // of the code for the corresponding symbol.
        //
        // The loop below doesn't actually build the tree; instead we compute
        // the distances of the leaves from the root on the fly.  When a new
        // node is added to the heap, then that node's descendants are linked
        // into a single linear list that starts at the new node, and the code
        // lengths of the descendants (that is, their distance from the root
        // of the tree) are incremented by one.

        var binHeap = new MinHeap<HeapFrequency>(heap.Take(count).Select((index) => new HeapFrequency(index, frequencies[index])), count);

        ulong[] scode = new ulong[HUF_ENCSIZE];

        while (count > 1)
        {
            // Find the indices, mm and m, of the two smallest non-zero frq
            // values in fHeap, add the smallest frq to the second-smallest
            // frq, and remove the smallest frq value from fHeap.

            var smallest = binHeap.Pop();
            var smallestIndex = smallest.Index;
            count--;

            var secondSmallest = binHeap.Pop();
            var secondSmallestIndex = secondSmallest.Index;

            binHeap.Push(new HeapFrequency(secondSmallestIndex, smallest.Frequency + secondSmallest.Frequency));

            // The entries in scode are linked into lists with the
            // entries in hlink serving as "next" pointers and with
            // the end of a list marked by hlink[j] == j.
            //
            // Traverse the lists that start at scode[m] and scode[mm].
            // For each element visited, increment the length of the
            // corresponding code by one bit. (If we visit scode[j]
            // during the traversal, then the code for symbol j becomes
            // one bit longer.)
            //
            // Merge the lists that start at scode[m] and scode[mm]
            // into a single list that starts at scode[m].

            // Add a bit to all codes in the first list.

            int index = secondSmallestIndex;
            while (true)
            {
                scode[index]++;

                if (links[index] == index)
                {
                    // Merge the two lists
                    links[index] = smallestIndex;
                    break;
                }

                index = links[index];
            }

            // Add a bit to all codes in the second list.

            index = smallestIndex;
            while (true)
            {
                scode[index]++;
                if (links[index] == index)
                {
                    break;
                }

                index = links[index];
            }
        }

        // Build a canonical Huffman code table, replacing the code
        // lengths in scode with (code, code length) pairs.  Copy the
        // code table from scode into frq.

        BuildCanonicalCodeTable(scode);
        scode.CopyTo(frequencies, 0);

        return (min, max);
    }

    /// Pack an encoding table:
    ///	- only code lengths, not actual codes, are stored
    ///	- runs of zeroes are compressed as follows:
    ///
    ///	  unpacked		packed
    ///	  --------------------------------
    ///	  1 zero		0	(6 bits)
    ///	  2 zeroes		59
    ///	  3 zeroes		60
    ///	  4 zeroes		61
    ///	  5 zeroes		62
    ///	  n zeroes (6 or more)	63 n-6	(6 + 8 bits)
    private static void PackEncodingTable(ulong[] frequencies, int min, int max, BitStreamer bits)
    {
        while (min <= max)
        {
            int codeLength = HufLength(frequencies[min]);

            if (codeLength == 0)
            {
                ulong zeroRun = 1;

                while ((min < max) && (zeroRun < LONGEST_LONG_RUN))
                {
                    if (HufLength(frequencies[min + 1]) > 0)
                    {
                        break;
                    }
                    min++;
                    zeroRun++;
                }

                if (zeroRun >= 2)
                {
                    if (zeroRun >= SHORTEST_LONG_RUN)
                    {
                        bits.WriteBits(6, LONG_ZEROCODE_RUN);
                        bits.WriteBits(8, zeroRun - SHORTEST_LONG_RUN);
                    }
                    else
                    {
                        bits.WriteBits(6, SHORT_ZEROCODE_RUN + zeroRun - 2);
                    }
                    min++;
                    continue;
                }
            }
            bits.WriteBits(6, (ulong)codeLength);
            min++;
        }

        bits.Flush();
    }

    private static ulong[] UnpackEncodingTable(BitStreamer packed, uint min, uint max)
    {
        ulong[] unpacked = new ulong[HUF_ENCSIZE];

        uint position = min;
        while (position <= max)
        {
            unpacked[position] = packed.ReadBits(6);
            int codeLength = (int)unpacked[position];

            if (codeLength == LONG_ZEROCODE_RUN)
            {
                int zeroRun = (int)packed.ReadBits(8) + SHORTEST_LONG_RUN;

                if (position + zeroRun > max + 1)
                {
                    throw new EXRCompressionException($"PIZ corrupt packed encoding table - long zero code run ({zeroRun}) exceeds declared max table index {max}");
                }

                for (int i = 0; i < zeroRun; i++)
                {
                    unpacked[position++] = 0;
                }

                position--;
            }
            else if (codeLength >= SHORT_ZEROCODE_RUN)
            {
                int zeroRun = codeLength - SHORT_ZEROCODE_RUN + 2;

                if (position + zeroRun > max + 1)
                {
                    throw new EXRCompressionException($"PIZ packed encoding table - short zero code run ({zeroRun}) exceeds declared max table index {max}");
                }

                for (int i = 0; i < zeroRun; i++)
                {
                    unpacked[position++] = 0;
                }

                position--;
            }

            position++;
        }

        packed.Reset();

        BuildCanonicalCodeTable(unpacked);
        return unpacked;
    }

    // Build a decoding hash table based on the encoding table hcode:
    //	- short codes (<= HUF_DECBITS) are resolved with a single table access;
    //	- long code entry allocations are not optimized, because long codes are
    //	  unfrequent;
    //	- decoding tables are used by Decode();
    private static HufDec[] BuildDecodingTable(ulong[] frequencies, uint min, uint max)
    {
        // Init hashtable & loop on all codes.
        // Assumes that hufClearDecTable(hdecod) has already been called.
        var result = new HufDec[HUF_DECSIZE];

        for (uint i = min; i <= max; i++)
        {
            ulong frequency = frequencies[i];
            ulong c = HufCode(frequency);
            int codeLength = HufLength(frequency);

            if ((c >> codeLength) != 0)
            {
                // Error: c is supposed to be an l-bit code,
                // but c contains a value that is greater
                // than the largest l-bit number.
                throw new EXRCompressionException($"PIZ corrupt encoding table - invalid Huffman code {c} from frequency {frequency}");
            }

            if (codeLength > HUF_DECBITS)
            {
                int hdecIndex = (int)c >> (codeLength - HUF_DECBITS);
                HufDec hdec = result[hdecIndex] ??= new HufDec();

                if (hdec.ShortLength > 0)
                {
                    // Error: a short code has already
                    // been stored in table entry.
                    throw new EXRCompressionException($"PIZ corrupt encoding table - multiple meanings of Huffman code {c}");
                }

                hdec.LongCode.Add(i);
            }
            else if (codeLength != 0)
            {
                // Short code: init all primary entries

                int hdecIndex = (int)c << (HUF_DECBITS - codeLength);

                ulong start = (ulong)1 << (HUF_DECBITS - codeLength);

                for (ulong iter = start; iter > 0; iter--, hdecIndex++)
                {
                    HufDec hdec = result[hdecIndex] ??= new HufDec();

                    if (hdec.ShortLength > 0 || hdec.LongCode.Count > 0)
                    {
                        // Error: a short code or a long code has
                        // already been stored in table entry.

                        throw new EXRCompressionException($"PIZ corrupt encoding table - multiple meanings of Huffman code {c}");
                    }

                    hdec.ShortLength = codeLength;
                    hdec.ShortCode = i;
                }
            }
        }
        return result;
    }

    private static int Encode(ulong[] frequencies, Span<ushort> uncompressed, BitStreamer output, int runLengthCode)
    {
        long outputStartPosition = output.Position;
        ushort start = uncompressed[0];
        int runLength = 0;
        for (int i = 1; i < uncompressed.Length; i++)
        {
            ushort current = uncompressed[i];
            // Count same values or emit code
            if (start == current && runLength < 255)
            {
                runLength++;
            }
            else
            {
                output.WriteCode(frequencies[start], runLength, frequencies[runLengthCode]);
                runLength = 0;
            }

            start = current;
        }

        // Emit remaining code
        output.WriteCode(frequencies[start], runLength, frequencies[runLengthCode]);

        int bitsWritten = (int)(output.Position - outputStartPosition) * 8 + output.BufferBitCount;
        output.Flush();

        return bitsWritten;
    }

    private static void Decode(ulong[] encodingTable, HufDec[] decodingTable, BitStreamer compressed, Span<ushort> decompressed, ulong bitLength, uint runLengthCode)
    {
        int destIndex = 0;

        while (compressed.RemainingBytes > 0)
        {
            compressed.BufferByte();
            while (compressed.BufferBitCount >= HUF_DECBITS)
            {
                ulong hdecIndex = compressed.ReadDecodingTableIndex();
                HufDec hdec = decodingTable[hdecIndex];

                if (hdec.ShortLength > 0)
                {
                    // Get short code
                    compressed.Advance(hdec.ShortLength);

                    destIndex += compressed.ReadCode(hdec.ShortCode, runLengthCode, decompressed, destIndex);
                }
                else
                {
                    var longCode = hdec.LongCode;

                    int j;
                    for (j = 0; j < longCode.Count; j++)
                    {
                        var code = encodingTable[longCode[j]];
                        int length = HufLength(code);

                        compressed.BufferBits(length);

                        if (HufCode(code) == compressed.PeekBits(length))
                        {
                            compressed.Advance(length);
                            destIndex += compressed.ReadCode(longCode[j], runLengthCode, decompressed, destIndex);
                            break;
                        }
                    }

                    if (j == longCode.Count)
                    {
                        throw new EXRCompressionException($"PIZ corrupt chunk - encoding table didn't contain long code for {hdecIndex}");
                    }
                }

            }
        }

        // Get remaining (short) codes

        int count = (int)(8 - bitLength) & 7;
        compressed.FinalizeRead(count);

        while (compressed.BufferBitCount > 0)
        {
            ulong hdecIndex = compressed.ReadDecodingTableIndex2();
            HufDec hdec = decodingTable[hdecIndex];

            if (hdec.ShortLength > 0)
            {
                compressed.Advance(hdec.ShortLength);
                destIndex += compressed.ReadCode(hdec.ShortCode, runLengthCode, decompressed, destIndex);
            }
            else
            {
                throw new EXRCompressionException($"PIZ corrupt chunk - encoding table didn't contain short code for {hdecIndex}");
            }
        }
    }

    private sealed class BitStreamer
    {
        private readonly Stream stream;
        public long Position => stream.Position;
        public long RemainingBytes => stream.Length - Position;
        public int BufferBitCount { get; private set; }
        private ulong buffer;

        public BitStreamer(Stream stream)
        {
            this.stream = stream;
        }

        public ulong ReadBits(int count)
        {
            while (BufferBitCount < count)
            {
                BufferByte();
            }
            BufferBitCount -= count;
            return (buffer >> BufferBitCount) & ((1u << count) - 1);
        }

        public ulong PeekBits(int count)
        {
            return (buffer >> (BufferBitCount - count)) & ((1u << count) - 1);
        }

        public void WriteBits(int count, ulong bits)
        {
            buffer <<= count;
            BufferBitCount += count;
            buffer |= bits;

            while (BufferBitCount >= 8)
            {
                BufferBitCount -= 8;
                stream.WriteByte((byte)(buffer >> BufferBitCount));
            }
        }

        public int ReadCode(uint code, uint runLengthCode, Span<ushort> dest, int destIndex)
        {
            int startDestIndex = destIndex;
            if (code == runLengthCode)
            {
                if (destIndex == 0)
                {
                    Error("found run length code at start of data.");
                }
                BufferByteIfNeeded();
                BufferBitCount -= 8;
                uint runLength = (byte)(buffer >> BufferBitCount);
                if (destIndex + runLength > dest.Length)
                {
                    Error("run length of ({runLength}) exceeds decompressed length.");
                }
                ushort repeatedCode = dest[destIndex - 1];
                for (ulong i = 0; i < runLength; i++)
                {
                    dest[destIndex++] = repeatedCode;
                }
            }
            else if (destIndex < dest.Length)
            {
                dest[destIndex++] = (ushort)code;
            }
            else
            {
                Error("data exceeds decompressed length.");
            }
            return destIndex - startDestIndex;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ulong ReadDecodingTableIndex()
        {
            return (buffer >> (BufferBitCount - HUF_DECBITS)) & HUF_DECMASK;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ulong ReadDecodingTableIndex2()
        {
            return (buffer << (HUF_DECBITS - BufferBitCount)) & HUF_DECMASK;
        }

        public void WriteCode(ulong code, int runCount, ulong runCode)
        {
            if (HufLength(code) + HufLength(runCode) + 8 < HufLength(code) * runCount)
            {
                EmitCode(code);
                EmitCode(runCode);
                WriteBits(8, (ulong)runCount);
            }
            else
            {
                for (int i = 0; i <= runCount; i++)
                {
                    EmitCode(code);
                }
            }
        }

        public void Advance(int count)
        {
            if (count > BufferBitCount)
            {
                Error("attempt to advance beyond bit buffer.");
            }
            BufferBitCount -= count;
        }

        public void FinalizeRead(int count)
        {
            buffer >>= count;
            BufferBitCount -= count;
        }

        public void Reset()
        {
            BufferBitCount = 0;
            buffer = 0;
        }

        public void Flush()
        {
            while (BufferBitCount > 0)
            {
                stream.WriteByte((byte)(buffer << (8 - BufferBitCount)));
                BufferBitCount -= Math.Min(8, BufferBitCount);
            }
            Reset();
        }

        private void EmitCode(ulong code)
        {
            WriteBits(HufLength(code), HufCode(code));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void BufferByteIfNeeded()
        {
            if (BufferBitCount < 8)
            {
                BufferByte();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void BufferByte()
        {
            buffer = (buffer << 8) | (byte)stream.ReadByte();
            BufferBitCount += 8;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void BufferBits(int count)
        {
            while (BufferBitCount < count)
            {
                BufferByte();
            }
        }

        [DoesNotReturn]
        private static void Error(string message)
        {
            throw new EXRCompressionException($"PIZ corrupt chunk - {message}");
        }
    }

    private class HufDec
    {
        public int ShortLength { get; set; }
        public uint ShortCode { get; set; }
        public List<uint> LongCode { get; } = new(2);

        public override string ToString()
        {
            if (ShortLength != 0)
            {
                return $"Short(ShortCode {{ value: {ShortCode}, len: {ShortLength} }})";
            }
            return $"Long([{String.Join(", ", LongCode)}])";
        }
    }

    private struct HeapFrequency : IComparable<HeapFrequency>
    {
        public int Index { get; }
        public ulong Frequency { get; set; }

        public HeapFrequency(int index, ulong frequency)
        {
            Index = index;
            Frequency = frequency;
        }

        public int CompareTo(HeapFrequency other)
        {
            if (other.Frequency == Frequency)
            {
                return Index - other.Index;
            }
            return Frequency > other.Frequency ? 1 : -1;
        }
    }
}
