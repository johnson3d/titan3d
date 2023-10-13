using System.Runtime.CompilerServices;

namespace Jither.OpenEXR.Compression;

internal static class Wavelet
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static (ushort l, ushort h) Encode14(ushort a, ushort b)
    {
        short signedA = (short)a;
        short signedB = (short)b;

        int signedM = (signedA + signedB) >> 1;
        int signedD = signedA - signedB;

        return ((ushort)signedM, (ushort)signedD);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static (ushort a, ushort b) Decode14(ushort l, ushort h)
    {
        short signedL = (short)l;
        short signedH = (short)h;

        int hi = signedH;
        int ai = signedL + (hi & 1) + (hi >> 1);

        int signedA = ai;
        int signedB = ai - hi;

        return ((ushort)signedA, (ushort)signedB);
    }

    private const int NBITS = 16;
    private const int A_OFFSET = 1 << (NBITS - 1);
    private const int M_OFFSET = 1 << (NBITS - 1);
    private const int MOD_MASK = (1 << NBITS) - 1;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static (ushort l, ushort h) Encode16(ushort a, ushort b)
    {
        int ao = (a + A_OFFSET) & MOD_MASK;
        int m = (ao + b) >> 1;
        int d = ao - b;

        if (d < 0)
        {
            m = (m + M_OFFSET) & MOD_MASK;
        }

        d &= MOD_MASK;

        return ((ushort)m, (ushort)d);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static (ushort a, ushort b) Decode16(ushort l, ushort h)
    {
        int bb = (l - (h >> 1)) & MOD_MASK;
        int aa = (h + bb - A_OFFSET) & MOD_MASK;
        return ((ushort)aa, (ushort)bb);
    }

    private const int LIMIT_14 = 1 << 14;

    public static void Encode2D(Span<ushort> data, int xSize, int xOffset, int ySize, int yOffset, ushort maxValue)
    {
        bool is14bit = maxValue < LIMIT_14;
        int size = xSize > ySize ? ySize : xSize;
        int p = 1;
        int p2 = 2;

        while (p2 <= size)
        {
            int py = 0;
            int ey = yOffset * (ySize - p2);
            int oy1 = yOffset * p;
            int oy2 = yOffset * p2;
            int ox1 = xOffset * p;
            int ox2 = xOffset * p2;
            ushort i00, i01, i10, i11;

            while (py <= ey)
            {
                int px = py;
                int ex = py + xOffset * (xSize - p2);

                while (px <= ex)
                {
                    int p01 = px + ox1;
                    int p10 = px + oy1;
                    int p11 = p10 + ox1;

                    if (is14bit)
                    {
                        (i00, i01) = Encode14(data[px], data[p01]);
                        (i10, i11) = Encode14(data[p10], data[p11]);
                        (data[px], data[p10]) = Encode14(i00, i10);
                        (data[p01], data[p11]) = Encode14(i01, i11);
                    }
                    else
                    {
                        (i00, i01) = Encode16(data[px], data[p01]);
                        (i10, i11) = Encode16(data[p10], data[p11]);
                        (data[px], data[p10]) = Encode16(i00, i10);
                        (data[p01], data[p11]) = Encode16(i01, i11);
                    }
                    px += ox2;
                }

                // Encode (1D) odd column (y loop)
                if ((xSize & p) != 0)
                {
                    int p10 = px + oy1;

                    (data[px], data[p10]) = is14bit ? Encode14(data[px], data[p10]) : Encode16(data[px], data[p10]);
                }

                py += oy2;
            }

            // Encode (1D) odd line (x loop)
            if ((ySize & p) != 0)
            {
                int px = py;
                int ex = py + xOffset * (xSize - p2);

                while (px <= ex)
                {
                    int p01 = px + ox1;

                    (data[px], data[p01]) = is14bit ? Encode14(data[px], data[p01]) : Encode16(data[px], data[p01]);

                    px += ox2;
                }
            }

            p = p2;
            p2 <<= 1;
        }
    }

    public static void Decode2D(Span<ushort> data, int xSize, int xOffset, int ySize, int yOffset, ushort maxValue)
    {
        bool is14bit = maxValue < LIMIT_14;
        int size = xSize > ySize ? ySize : xSize;
        int p = 1;
        int p2;

        while (p <= size)
        {
            p <<= 1;
        }

        p >>= 1;
        p2 = p;
        p >>= 1;

        while (p >= 1)
        {
            int py = 0;
            int ey = yOffset * (ySize - p2);
            int oy1 = yOffset * p;
            int oy2 = yOffset * p2;
            int ox1 = xOffset * p;
            int ox2 = xOffset * p2;
            
            ushort i00, i01, i10, i11;

            while (py <= ey)
            {
                int px = py;
                int ex = py + xOffset * (xSize - p2);

                while (px <= ex)
                {
                    int p01 = px + ox1;
                    int p10 = px + oy1;
                    int p11 = p10 + ox1;

                    if (is14bit)
                    {
                        (i00, i10) = Decode14(data[px], data[p10]);
                        (i01, i11) = Decode14(data[p01], data[p11]);
                        (data[px], data[p01]) = Decode14(i00, i01);
                        (data[p10], data[p11]) = Decode14(i10, i11);
                    }
                    else
                    {
                        (i00, i10) = Decode16(data[px], data[p10]);
                        (i01, i11) = Decode16(data[p01], data[p11]);
                        (data[px], data[p01]) = Decode16(i00, i01);
                        (data[p10], data[p11]) = Decode16(i10, i11);
                    }
                    px += ox2;
                }

                // Decode (1D) odd column (y loop)
                if ((xSize & p) != 0)
                {
                    int p10 = px + oy1;

                    (i00, data[p10]) = is14bit ? Decode14(data[px], data[p10]) : Decode16(data[px], data[p10]);

                    data[px] = i00;
                }
                py += oy2;
            }

            // Decode (1D) odd line (x loop)
            if ((ySize & p) != 0)
            {
                int px = py;
                int ex = py + xOffset * (xSize - p2);

                while (px <= ex)
                {
                    int p01 = px + ox1;

                    (i00, data[p01]) = is14bit ? Decode14(data[px], data[p01]) : Decode16(data[px], data[p01]);

                    data[px] = i00;
                    px += ox2;
                }
            }

            p2 = p;
            p >>= 1;
        }
    }
}
