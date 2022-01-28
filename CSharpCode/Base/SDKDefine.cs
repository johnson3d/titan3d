using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS
{
    public class CoreDefine
    {
        public const float Epsilon = 0.00001f;
        public const float DEpsilon = 0.00001f;
        public static bool FloatEuqal(float f1, float f2, float epsilon)
        {
            return Math.Abs(f2 - f1) < epsilon;
        }
        public static bool DoubleEuqal(double f1, double f2, double epsilon)
        {
            return Math.Abs(f2 - f1) < epsilon;
        }
        public static float FloatLerp(float f1, float f2, float lp)
        {
            return f1 * lp + (1.0f - lp) * f2;
        }
        public static double DoubleLerp(double f1, double f2, double lp)
        {
            return f1 * lp + (1.0 - lp) * f2;
        }
        public static int Clamp(int a, int min, int max)
        {
            if (a > max)
                return max;
            else if (a < min)
                return min;
            return a;
        }
        public static uint Roundup(uint a, uint b)
        {
            uint result = a / b;
            if (a % b != 0)
                result += 1;
            return result;
        }
        public static int RoundUpPow2(int numToRound, int multiple)
        {
            System.Diagnostics.Debug.Assert(multiple!=0 && ((multiple & (multiple - 1)) == 0));
            return (numToRound + multiple - 1) & -multiple;
        }
        public static void Swap<T>(ref T lh, ref T rh)
        {
            T tmp = lh;
            lh = rh;
            rh = tmp;
        }
    }
}
