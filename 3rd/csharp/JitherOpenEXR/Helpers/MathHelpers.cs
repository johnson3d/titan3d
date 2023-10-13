using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Jither.OpenEXR.Helpers;

internal static class MathHelpers
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T DivAndRoundUp<T>(T a, T b) where T: IBinaryInteger<T>
    {
        return a / b + ((!T.IsZero(a % b)) ? T.One : T.Zero);
    }
}
