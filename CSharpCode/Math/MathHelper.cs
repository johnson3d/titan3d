using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace EngineNS
{
    public partial class CustomConvert
    {
        #region float
        [Rtti.Meta("")]
        public static string ConvertSingleToString(float val)
        {
            return val.ToString();
        }
        [Rtti.Meta("")]
        public static Byte ConvertSingleToByte(float val)
        {
            return System.Convert.ToByte(val);
        }
        [Rtti.Meta("")]
        public static UInt16 ConvertSingleToUInt16(float val)
        {
            return System.Convert.ToUInt16(val);
        }
        [Rtti.Meta("")]
        public static UInt32 ConvertSingleToUInt32(float val)
        {
            return System.Convert.ToUInt32(val);
        }
        [Rtti.Meta("")]
        public static UInt64 ConvertSingleToUInt64(float val)
        {
            return System.Convert.ToUInt64(val);
        }
        [Rtti.Meta("")]
        public static SByte ConvertSingleToSByte(float val)
        {
            return System.Convert.ToSByte(val);
        }
        [Rtti.Meta("")]
        public static Int16 ConvertSingleToInt16(float val)
        {
            return System.Convert.ToInt16(val);
        }
        [Rtti.Meta("")]
        public static Int32 ConvertSingleToInt32(float val)
        {
            return System.Convert.ToInt32(val);
        }
        [Rtti.Meta("")]
        public static Int64 ConvertSingleToInt64(float val)
        {
            return System.Convert.ToInt64(val);
        }
        [Rtti.Meta("")]
        public static double ConvertSingleToDouble(float val)
        {
            return System.Convert.ToDouble(val);
        }
        [Rtti.Meta("")]
        public static float ConvertStringToSingle(string val)
        {
            try
            {
                return System.Convert.ToSingle(val);
            }
            catch { }
            return 0;
        }
        #endregion
        #region Byte
        [Rtti.Meta("")]
        public static string ConvertByteToString(byte val)
        {
            return val.ToString();
        }
        [Rtti.Meta("")]
        public static UInt16 ConvertByteToUInt16(byte val)
        {
            return System.Convert.ToUInt16(val);
        }
        [Rtti.Meta("")]
        public static UInt32 ConvertByteToUInt32(byte val)
        {
            return System.Convert.ToUInt32(val);
        }
        [Rtti.Meta("")]
        public static UInt64 ConvertByteToUInt64(byte val)
        {
            return System.Convert.ToUInt64(val);
        }
        [Rtti.Meta("")]
        public static SByte ConvertByteToSByte(byte val)
        {
            return System.Convert.ToSByte(val);
        }
        [Rtti.Meta("")]
        public static Int16 ConvertByteToInt16(byte val)
        {
            return System.Convert.ToInt16(val);
        }
        [Rtti.Meta("")]
        public static Int32 ConvertByteToInt32(byte val)
        {
            return System.Convert.ToInt32(val);
        }
        [Rtti.Meta("")]
        public static Int64 ConvertByteToInt64(byte val)
        {
            return System.Convert.ToInt64(val);
        }
        [Rtti.Meta("")]
        public static float ConvertByteToSingle(byte val)
        {
            return System.Convert.ToSingle(val);
        }
        [Rtti.Meta("")]
        public static double ConvertByteToDouble(byte val)
        {
            return System.Convert.ToDouble(val);
        }
        [Rtti.Meta("")]
        public static byte ConvertStringToByte(string val)
        {
            try
            {
                return System.Convert.ToByte(val);
            }
            catch { }
            return 0;
        }
        #endregion
        #region SByte
        [Rtti.Meta("")]
        public static string ConvertSByteToString(sbyte val)
        {
            return val.ToString();
        }
        [Rtti.Meta("")]
        public static Byte ConvertSByteToByte(sbyte val)
        {
            return System.Convert.ToByte(val);
        }
        [Rtti.Meta("")]
        public static UInt16 ConvertSByteToUInt16(sbyte val)
        {
            return System.Convert.ToUInt16(val);
        }
        [Rtti.Meta("")]
        public static UInt32 ConvertSByteToUInt32(sbyte val)
        {
            return System.Convert.ToUInt32(val);
        }
        [Rtti.Meta("")]
        public static UInt64 ConvertSByteToUInt64(sbyte val)
        {
            return System.Convert.ToUInt64(val);
        }
        [Rtti.Meta("")]
        public static Int16 ConvertSByteToInt16(sbyte val)
        {
            return System.Convert.ToInt16(val);
        }
        [Rtti.Meta("")]
        public static Int32 ConvertSByteToInt32(sbyte val)
        {
            return System.Convert.ToInt32(val);
        }
        [Rtti.Meta("")]
        public static Int64 ConvertSByteToInt64(sbyte val)
        {
            return System.Convert.ToInt64(val);
        }
        [Rtti.Meta("")]
        public static double ConvertSByteToDouble(sbyte val)
        {
            return System.Convert.ToDouble(val);
        }
        [Rtti.Meta("")]
        public static sbyte ConvertStringToSByte(string val)
        {
            try
            {
                return System.Convert.ToSByte(val);
            }
            catch { }
            return 0;
        }
        #endregion
        #region UInt16
        [Rtti.Meta("")]
        public static string ConvertUInt16ToString(UInt16 val)
        {
            return val.ToString();
        }
        [Rtti.Meta("")]
        public static Byte ConvertUInt16ToByte(UInt16 val)
        {
            return System.Convert.ToByte(val);
        }
        [Rtti.Meta("")]
        public static UInt32 ConvertUInt16ToUInt32(UInt16 val)
        {
            return System.Convert.ToUInt32(val);
        }
        [Rtti.Meta("")]
        public static UInt64 ConvertUInt16ToUInt64(UInt16 val)
        {
            return System.Convert.ToUInt64(val);
        }
        [Rtti.Meta("")]
        public static SByte ConvertUInt16ToSByte(UInt16 val)
        {
            return System.Convert.ToSByte(val);
        }
        [Rtti.Meta("")]
        public static Int16 ConvertUInt16ToInt16(UInt16 val)
        {
            return System.Convert.ToInt16(val);
        }
        [Rtti.Meta("")]
        public static Int32 ConvertUInt16ToInt32(UInt16 val)
        {
            return System.Convert.ToInt32(val);
        }
        [Rtti.Meta("")]
        public static Int64 ConvertUInt16ToInt64(UInt16 val)
        {
            return System.Convert.ToInt64(val);
        }
        [Rtti.Meta("")]
        public static float ConvertUInt16ToSingle(UInt16 val)
        {
            return System.Convert.ToSingle(val);
        }
        [Rtti.Meta("")]
        public static double ConvertUInt16ToDouble(UInt16 val)
        {
            return System.Convert.ToDouble(val);
        }
        [Rtti.Meta("")]
        public static UInt16 ConvertStringToUInt16(string val)
        {
            try
            {
                return System.Convert.ToUInt16(val);
            }
            catch { }
            return 0;
        }
        #endregion
        #region UInt32
        [Rtti.Meta("")]
        public static string ConvertUInt32ToString(UInt32 val)
        {
            return val.ToString();
        }
        [Rtti.Meta("")]
        public static Byte ConvertUInt32ToByte(UInt32 val)
        {
            return System.Convert.ToByte(val);
        }
        [Rtti.Meta("")]
        public static UInt16 ConvertUInt32ToUInt16(UInt32 val)
        {
            return System.Convert.ToUInt16(val);
        }
        [Rtti.Meta("")]
        public static UInt32 ConvertUInt32ToUInt32(UInt32 val)
        {
            return System.Convert.ToUInt32(val);
        }
        [Rtti.Meta("")]
        public static UInt64 ConvertUInt32ToUInt64(UInt32 val)
        {
            return System.Convert.ToUInt64(val);
        }
        [Rtti.Meta("")]
        public static SByte ConvertUInt32ToSByte(UInt32 val)
        {
            return System.Convert.ToSByte(val);
        }
        [Rtti.Meta("")]
        public static Int16 ConvertUInt32ToInt16(UInt32 val)
        {
            return System.Convert.ToInt16(val);
        }
        [Rtti.Meta("")]
        public static Int32 ConvertUInt32ToInt32(UInt32 val)
        {
            return System.Convert.ToInt32(val);
        }
        [Rtti.Meta("")]
        public static Int64 ConvertUInt32ToInt64(UInt32 val)
        {
            return System.Convert.ToInt64(val);
        }
        [Rtti.Meta("")]
        public static float ConvertUInt32ToSingle(UInt32 val)
        {
            return System.Convert.ToSingle(val);
        }
        [Rtti.Meta("")]
        public static double ConvertUInt32ToDouble(UInt32 val)
        {
            return System.Convert.ToDouble(val);
        }
        [Rtti.Meta("")]
        public static UInt32 ConvertStringToUInt32(string val)
        {
            try
            {
                return System.Convert.ToUInt32(val);
            }
            catch { }
            return 0;
        }
        #endregion
        #region UInt64
        [Rtti.Meta("")]
        public static string ConvertUInt64ToString(UInt64 val)
        {
            return val.ToString();
        }
        [Rtti.Meta("")]
        public static Byte ConvertUInt64ToByte(UInt64 val)
        {
            return System.Convert.ToByte(val);
        }
        [Rtti.Meta("")]
        public static UInt16 ConvertUInt64ToUInt16(UInt64 val)
        {
            return System.Convert.ToUInt16(val);
        }
        [Rtti.Meta("")]
        public static UInt32 ConvertUInt64ToUInt32(UInt64 val)
        {
            return System.Convert.ToUInt32(val);
        }
        [Rtti.Meta("")]
        public static SByte ConvertUInt64ToSByte(UInt64 val)
        {
            return System.Convert.ToSByte(val);
        }
        [Rtti.Meta("")]
        public static Int16 ConvertUInt64ToInt16(UInt64 val)
        {
            return System.Convert.ToInt16(val);
        }
        [Rtti.Meta("")]
        public static Int32 ConvertUInt64ToInt32(UInt64 val)
        {
            return System.Convert.ToInt32(val);
        }
        [Rtti.Meta("")]
        public static Int64 ConvertUInt64ToInt64(UInt64 val)
        {
            return System.Convert.ToInt64(val);
        }
        [Rtti.Meta("")]
        public static float ConvertUInt64ToSingle(UInt64 val)
        {
            return System.Convert.ToSingle(val);
        }
        [Rtti.Meta("")]
        public static double ConvertUInt64ToDouble(UInt64 val)
        {
            return System.Convert.ToDouble(val);
        }
        [Rtti.Meta("")]
        public static UInt64 ConvertStringToUInt64(string val)
        {
            try
            {
                return System.Convert.ToUInt64(val);
            }
            catch { }
            return 0;
        }
        #endregion
        #region Int16
        [Rtti.Meta("")]
        public static string ConvertInt16ToString(Int16 val)
        {
            return val.ToString();
        }
        [Rtti.Meta("")]
        public static Byte ConvertInt16ToByte(Int16 val)
        {
            return System.Convert.ToByte(val);
        }
        [Rtti.Meta("")]
        public static UInt16 ConvertInt16ToUInt16(Int16 val)
        {
            return System.Convert.ToUInt16(val);
        }
        [Rtti.Meta("")]
        public static UInt32 ConvertInt16ToUInt32(Int16 val)
        {
            return System.Convert.ToUInt32(val);
        }
        [Rtti.Meta("")]
        public static UInt64 ConvertInt16ToUInt64(Int16 val)
        {
            return System.Convert.ToUInt64(val);
        }
        [Rtti.Meta("")]
        public static SByte ConvertInt16ToSByte(Int16 val)
        {
            return System.Convert.ToSByte(val);
        }
        [Rtti.Meta("")]
        public static Int32 ConvertInt16ToInt32(Int16 val)
        {
            return System.Convert.ToInt32(val);
        }
        [Rtti.Meta("")]
        public static Int64 ConvertInt16ToInt64(Int16 val)
        {
            return System.Convert.ToInt64(val);
        }
        [Rtti.Meta("")]
        public static float ConvertInt16ToSingle(Int16 val)
        {
            return System.Convert.ToSingle(val);
        }
        [Rtti.Meta("")]
        public static double ConvertInt16ToDouble(Int16 val)
        {
            return System.Convert.ToDouble(val);
        }
        [Rtti.Meta("")]
        public static Int16 ConvertStringToInt16(string val)
        {
            try
            {
                return System.Convert.ToInt16(val);
            }
            catch { }
            return 0;
        }
        #endregion
        #region Int32
        [Rtti.Meta("")]
        public static string ConvertInt32ToString(Int32 val)
        {
            return val.ToString();
        }
        [Rtti.Meta("")]
        public static Byte ConvertInt32ToByte(Int32 val)
        {
            return System.Convert.ToByte(val);
        }
        [Rtti.Meta("")]
        public static UInt16 ConvertInt32ToUInt16(Int32 val)
        {
            return System.Convert.ToUInt16(val);
        }
        [Rtti.Meta("")]
        public static UInt32 ConvertInt32ToUInt32(Int32 val)
        {
            return System.Convert.ToUInt32(val);
        }
        [Rtti.Meta("")]
        public static UInt64 ConvertInt32ToUInt64(Int32 val)
        {
            return System.Convert.ToUInt64(val);
        }
        [Rtti.Meta("")]
        public static SByte ConvertInt32ToSByte(Int32 val)
        {
            return System.Convert.ToSByte(val);
        }
        [Rtti.Meta("")]
        public static Int16 ConvertInt32ToInt16(Int32 val)
        {
            return System.Convert.ToInt16(val);
        }
        [Rtti.Meta("")]
        public static Int64 ConvertInt32ToInt64(Int32 val)
        {
            return System.Convert.ToInt64(val);
        }
        [Rtti.Meta("")]
        public static float ConvertInt32ToSingle(Int32 val)
        {
            return System.Convert.ToSingle(val);
        }
        [Rtti.Meta("")]
        public static double ConvertInt32ToDouble(Int32 val)
        {
            return System.Convert.ToDouble(val);
        }
        [Rtti.Meta("")]
        public static Int32 ConvertStringToInt32(string val)
        {
            try
            {
                return System.Convert.ToInt32(val);
            }
            catch { }
            return 0;
        }
        #endregion
        #region Int64
        [Rtti.Meta("")]
        public static string ConvertInt64ToString(Int64 val)
        {
            return val.ToString();
        }
        [Rtti.Meta("")]
        public static Byte ConvertInt64ToByte(Int64 val)
        {
            return System.Convert.ToByte(val);
        }
        [Rtti.Meta("")]
        public static UInt16 ConvertInt64ToUInt16(Int64 val)
        {
            return System.Convert.ToUInt16(val);
        }
        [Rtti.Meta("")]
        public static UInt32 ConvertInt64ToUInt32(Int64 val)
        {
            return System.Convert.ToUInt32(val);
        }
        [Rtti.Meta("")]
        public static UInt64 ConvertInt64ToUInt64(Int64 val)
        {
            return System.Convert.ToUInt64(val);
        }
        [Rtti.Meta("")]
        public static SByte ConvertInt64ToSByte(Int64 val)
        {
            return System.Convert.ToSByte(val);
        }
        [Rtti.Meta("")]
        public static Int16 ConvertInt64ToInt16(Int64 val)
        {
            return System.Convert.ToInt16(val);
        }
        [Rtti.Meta("")]
        public static Int32 ConvertInt64ToInt32(Int64 val)
        {
            return System.Convert.ToInt32(val);
        }
        [Rtti.Meta("")]
        public static float ConvertInt64ToSingle(Int64 val)
        {
            return System.Convert.ToSingle(val);
        }
        [Rtti.Meta("")]
        public static double ConvertInt64ToDouble(Int64 val)
        {
            return System.Convert.ToDouble(val);
        }
        [Rtti.Meta("")]
        public static Int64 ConvertStringToInt64(string val)
        {
            try
            {
                return System.Convert.ToInt64(val);
            }
            catch { }
            return 0;
        }
        #endregion
        #region Double
        [Rtti.Meta("")]
        public static string ConvertDoubleToString(double val)
        {
            return val.ToString();
        }
        [Rtti.Meta("")]
        public static Byte ConvertDoubleToByte(double val)
        {
            return System.Convert.ToByte(val);
        }
        [Rtti.Meta("")]
        public static UInt16 ConvertDoubleToUInt16(double val)
        {
            return System.Convert.ToUInt16(val);
        }
        [Rtti.Meta("")]
        public static UInt32 ConvertDoubleToUInt32(double val)
        {
            return System.Convert.ToUInt32(val);
        }
        [Rtti.Meta("")]
        public static UInt64 ConvertDoubleToUInt64(double val)
        {
            return System.Convert.ToUInt64(val);
        }
        [Rtti.Meta("")]
        public static SByte ConvertDoubleToSByte(double val)
        {
            return System.Convert.ToSByte(val);
        }
        [Rtti.Meta("")]
        public static Int16 ConvertDoubleToInt16(double val)
        {
            return System.Convert.ToInt16(val);
        }
        [Rtti.Meta("")]
        public static Int32 ConvertDoubleToInt32(double val)
        {
            return System.Convert.ToInt32(val);
        }
        [Rtti.Meta("")]
        public static Int64 ConvertDoubleToInt64(double val)
        {
            return System.Convert.ToInt64(val);
        }
        [Rtti.Meta("")]
        public static float ConvertDoubleToSingle(double val)
        {
            return System.Convert.ToSingle(val);
        }
        [Rtti.Meta("")]
        public static double ConvertStringToDouble(string val)
        {
            try
            {
                return System.Convert.ToDouble(val);
            }
            catch { }
            return 0;
        }
        #endregion
    }
    [Rtti.Meta("")]
    public partial class MathHelper
    {
        public const float Epsilon = ((float)0.00001);
        public const float DEpsilon = 0.0000001f;
        public const float TWO_PI = V_PI * 2.0f;
        public const float V_PI = ((float)3.1415926535);
        [Rtti.Meta("")]
        public static float PI
        {
            get => V_PI;
        }

        [Rtti.Meta("")]
        public static float Invc2PI
        {
            get => (1.0f / TWO_PI);
        }
        // Degrees-to-radians conversion constant (RO).
        public const float V_Deg2Rad = V_PI / 180.0f;
        [Rtti.Meta("")]
        public static float Deg2Rad
        {
            get => V_Deg2Rad;
        }
        // Radians-to-degrees conversion constant (RO).
        public const float V_Rad2Deg = 180.0f / V_PI;
        [Rtti.Meta("")]
        public static float Rad2Deg
        {
            get => V_Rad2Deg;
        }
        public static T MaxSameType<T>(in T a, in T b) where T : IComparable<T>
        {
            return a.CompareTo(b) > 0 ? a : b;
        }
        public static T MinSameType<T>(in T a, in T b) where T : IComparable<T>
        {
            return a.CompareTo(b) < 0 ? a : b;
        }
        public static float Floor(float v)
        {
            return (float)Math.Floor(v);
        }
        public static float Sqrt(float v)
        {
            return (float)System.Math.Sqrt(v);
        }
        [Rtti.Meta("")]
        public static float Abs(float v)
        {
            return (float)System.Math.Abs(v);
        }
        [Rtti.Meta("")]
        public static float Mod(float v1, float v2)
        {
            return (float)v1 % v2;
        }
        [Rtti.Meta("")]
        public static float Sin(float v)
        {
            return (float)System.Math.Sin(v);
        }
        [Rtti.Meta("")]
        public static float Asin(float v)
        {
            return (float)System.Math.Asin(v);
        }
        [Rtti.Meta("")]
        public static float Cos(float v)
        {
            return (float)System.Math.Cos(v);
        }
        [Rtti.Meta("")]
        public static float Acos(float v)
        {
            v = Clamp<float>(v,-1,1);
            return (float)System.Math.Acos(v);
        }
        [Rtti.Meta("")]
        public static float Tan(float v)
        {
            return (float)System.Math.Tan(v);
        }
        [Rtti.Meta("")]
        public static float Atan(float v)
        {
            return (float)System.Math.Atan(v);
        }
        [Rtti.Meta("")]
        public static float Atan2(float y, float x)
        {
            return (float)System.Math.Atan2(y, x);
        }
        [Rtti.Meta("")]
        public static float Pow(float x, float y)
        {
            return (float)System.Math.Pow(x, y);
        }

        private static System.Random sRandom = new Random((int)Support.TtTime.GetTickCount());
        [Rtti.Meta("")]
        public static int Random()
        {
            return sRandom.Next();
        }
        [Rtti.Meta("")]
        public static int RandomRange(int start, int end)
        {
            return sRandom.Next(start, end);
        }
        public static float RandomRange(float start, float end)
        {
            return RandomFloat() * (end - start);
        }
        [Rtti.Meta("")]
        public static float RandomFloat()
        {
            return (float)sRandom.NextDouble();
        }
        [Rtti.Meta("")]
        public static Vector3 RandomDirection(bool bNormalize = true)
        {
            Vector3 result;
            result.X = RandomFloat();
            result.Y = RandomFloat();
            result.Z = RandomFloat();
            if (bNormalize)
                result.Normalize();
            return result;
        }
        [Rtti.Meta("")]
        public static float FClamp(float value, float min, float max)
        {
            if (value > max)
                return max;
            if (value < min)
                return min;
            return value;
        }
        [Rtti.Meta("")]
        public static float FWrap(float value, float min, float max)
        {
            if (value > max)
                return min;
            if (value < min)
                return max;
            return value;
        }
        public static T Clamp<T>(T value, T min, T max) where T : System.IComparable<T>
        {
            T result = value;
            if (value.CompareTo(max) > 0)
                result = max;
            if (value.CompareTo(min) < 0)
                result = min;
            return result;
        }
        [Rtti.Meta("")]
        public static float Lerp(float from, float to, float t)
        {
            return to * t + from * (1.0f - t);
        }
        public static int Lerp(int from, int to, float t)
        {
            return (int)((float)to * t + (float)from * (1.0f - t));
        }
        #region CreateInstance
        [Rtti.Meta("",ShaderName = "CreateVector2f")]
        public static Vector2 CreateVector2f(float x, float y)
        {
            return new Vector2(x, y);
        }
        [Rtti.Meta("",ShaderName = "CreateVector3f")]
        public static Vector3 CreateVector3f(float x, float y, float z)
        {
            return new Vector3(x, y, z);
        }
        [Rtti.Meta("",ShaderName = "CreateVector4f")]
        public static Vector4 CreateVector4f(float x, float y, float z, float w)
        {
            return new Vector4(x, y, z, w);
        }
        [Rtti.Meta("",ShaderName = "CreateColor3f")]
        public static Color3f CreateColor3f(float r, float g, float b)
        {
            return new Color3f(r, g, b);
        }
        [Rtti.Meta("",ShaderName = "CreateColor4f")]
        public static Color4f CreateColor4f(float r, float g, float b, float a)
        {
            return new Color4f(a, r, g, b);
        }
        #endregion
        // 圆与线段碰撞检测
        // 圆心p(x, y), 半径r, 线段两端点p1(x1, y1)和p2(x2, y2)
        [Rtti.Meta("")]
        public static bool IsCircleIntersectLineSeg(Vector2 circleCenter, float r, Vector2 point1, Vector2 point2)
        {
            float vx1 = circleCenter.X - point1.X;
            float vy1 = circleCenter.Y - point1.Y;
            float vx2 = point2.X - point1.X;
            float vy2 = point2.Y- point1.Y;

            //assert(Abs(vx2) > 0.00001f || Abs(vy2) > 0.00001f);

            // len = v2.length()
            float len = (float)Math.Sqrt(vx2 * vx2 + vy2 * vy2);

            // v2.normalize()
            vx2 /= len;
            vy2 /= len;

            // u = v1.dot(v2)
            // u is the vector projection length of vector v1 onto vector v2.
            float u = vx1 * vx2 + vy1 * vy2;

            // determine the nearest point on the lineseg
            float x0 = 0.0f;
            float y0 = 0.0f;
            if (u <= 0)
            {
                // p is on the left of p1, so p1 is the nearest point on lineseg
                x0 = point1.X;
                y0 = point1.Y;
            }
            else if (u >= len)
            {
                // p is on the right of p2, so p2 is the nearest point on lineseg
                x0 = point2.X;
                y0 = point2.Y;
            }
            else
            {
                // p0 = p1 + v2 * u
                // note that v2 is already normalized.
                x0 = point1.X + vx2 * u;
                y0 = point1.Y + vy2 * u;
            }

            return (circleCenter.X - x0) * (circleCenter.X- x0) + (circleCenter.Y - y0) * (circleCenter.Y- y0) <= r * r;

        }

        [Rtti.Meta("")]
        public static bool IsSphereIntersectLineSeg(Vector3 start, Vector3 end,
                                   Vector3 sphereCenter, float sphereRadius
                                  /* ,ref List<Vector3> intersectPoint*/)
        {
            var rayDir = (end - start).NormalizeValue;
            var length = (end - start).Length();
            Vector3 v = start - sphereCenter;
            float b = 2.0f * Vector3.Dot(rayDir,v);
            float c = Vector3.Dot(v,v) - sphereRadius * sphereRadius;
            float discriminant = (b * b) - (4.0f * c);

            if (discriminant < 0.0f) return false;

            discriminant = (float)Math.Sqrt(discriminant);

            float far = (-b + discriminant) / 2.0f;
            float near = (-b - discriminant) / 2.0f;

            Vector3 intersectFarPoint = start + rayDir * far;
            Vector3 intersectNearPoint = start + rayDir * near;
            return (far <= length || near <= length);
            {

            }
            //bool res = (far >= 0.0f || near >= 0.0f);

            //if (res)
            //{
            //    if (near > 0) intersectPoint.Add(intersectNearPoint);
            //    if (far > 0) intersectPoint.Add(intersectFarPoint);
            //}
            //return res;
        }


        /** Spreads bits to every other. */
        [Rtti.Meta("")]
        public static UInt32 MortonCode2(UInt32 x)
        {
            x &= 0x0000ffff;
            x = (x ^ (x << 8)) & 0x00ff00ff;
            x = (x ^ (x << 4)) & 0x0f0f0f0f;
            x = (x ^ (x << 2)) & 0x33333333;
            x = (x ^ (x << 1)) & 0x55555555;
            return x;
        }
        [Rtti.Meta("")]
        public static UInt64 MortonCode2_64(UInt64 x)
        {
            x &= 0x00000000ffffffff;
            x = (x ^ (x << 16)) & 0x0000ffff0000ffff;
            x = (x ^ (x << 8)) & 0x00ff00ff00ff00ff;
            x = (x ^ (x << 4)) & 0x0f0f0f0f0f0f0f0f;
            x = (x ^ (x << 2)) & 0x3333333333333333;
            x = (x ^ (x << 1)) & 0x5555555555555555;
            return x;
        }

        /** Reverses MortonCode2. Compacts every other bit to the right. */
        [Rtti.Meta("")]
        public static UInt32 ReverseMortonCode2(UInt32 x)
        {
            x &= 0x55555555;
            x = (x ^ (x >> 1)) & 0x33333333;
            x = (x ^ (x >> 2)) & 0x0f0f0f0f;
            x = (x ^ (x >> 4)) & 0x00ff00ff;
            x = (x ^ (x >> 8)) & 0x0000ffff;
            return x;
        }
        [Rtti.Meta("")]
        public static UInt64 ReverseMortonCode2_64(UInt64 x)
        {
            x &= 0x5555555555555555;
            x = (x ^ (x >> 1)) & 0x3333333333333333;
            x = (x ^ (x >> 2)) & 0x0f0f0f0f0f0f0f0f;
            x = (x ^ (x >> 4)) & 0x00ff00ff00ff00ff;
            x = (x ^ (x >> 8)) & 0x0000ffff0000ffff;
            x = (x ^ (x >> 16)) & 0x00000000ffffffff;
            return x;
        }

        /** Spreads bits to every 3rd. */
        [Rtti.Meta("")]
        public static UInt32 MortonCode3(UInt32 x)
        {
            x &= 0x000003ff;
            x = (x ^ (x << 16)) & 0xff0000ff;
            x = (x ^ (x << 8)) & 0x0300f00f;
            x = (x ^ (x << 4)) & 0x030c30c3;
            x = (x ^ (x << 2)) & 0x09249249;
            return x;
        }

        /** Reverses MortonCode3. Compacts every 3rd bit to the right. */
        [Rtti.Meta("")]
        public static UInt32 ReverseMortonCode3(UInt32 x)
        {
            x &= 0x09249249;
            x = (x ^ (x >> 2)) & 0x030c30c3;
            x = (x ^ (x >> 4)) & 0x0300f00f;
            x = (x ^ (x >> 8)) & 0xff0000ff;
            x = (x ^ (x >> 16)) & 0x000003ff;
            return x;
        }

        [Rtti.Meta("")]
        public static UInt32 ILog2Const(UInt32 n)
        {
            return (n > 1) ? 1 + ILog2Const(n / 2) : 0;
        }

        [Rtti.Meta("")]
        public static uint DivideAndRoundUp(uint Dividend, uint Divisor)
        {
            return (Dividend + Divisor - 1) / Divisor;
        }

        // Encode for normal map.
        [Rtti.Meta("")]
        public static Vector2 SphericalEncode(Vector3 v3)
        {
            Vector2 v = new Vector2();
            v.X = Atan2(v3.Y, v3.X) * Invc2PI;
            v.Y = v3.Z;

            v.X = v.X * 0.5f + 0.5f;
            v.Y = v.Y * 0.5f + 0.5f;
            return v;
        }

        [Rtti.Meta("")]
        public static Vector3 SphericalDecode(Vector2 v)
        {
            Vector2 ang = new Vector2(v.X * 2.0f - 1.0f, v.Y * 2.0f - 1.0f);

            Vector2 scth = new Vector2(0.0f, 0.0f);

            float r = ang.X * TWO_PI;
            float d2 = 1.0f - ang.Y * ang.Y;

            scth.X = Cos(r);
            scth.Y = Sin(r);

            Vector2 schpi = new Vector2(Sqrt(1.0f - ang.Y * ang.Y), ang.Y);

            Vector3 v3 = new Vector3(scth.X * schpi.X, scth.Y * schpi.X, schpi.Y);

            return v3;
        }

        [Rtti.Meta("")]
        public static Vector2 OctEncode(Vector3 v3)
        {
            float dxyz = Abs(v3.X) + Abs(v3.Y) + Abs(v3.X);
            v3.X = v3.X / dxyz;
            v3.Y = v3.Y / dxyz;
            v3.Z = v3.Z / dxyz;

            Vector2 n = new Vector2(v3.X, v3.Y);

            if (v3.Z < 0)
            {
                float nx = n.X;
                float ny = n.Y;

                n.X = (1.0f - Abs(nx)) * (nx >= 0.0f ? 1.0f : - 1.0f );
                n.Y = (1.0f - Abs(ny)) * (ny >= 0.0f ? 1.0f : - 1.0f );
            }

            n.X = n.X * 0.5f + 0.5f;
            n.Y = n.Y * 0.5f + 0.5f;
            return n;
        }

        [Rtti.Meta("")]
        public static Vector3 OctDecode(Vector2 v)
        {
            v.X = v.X * 2.0f - 1.0f;
            v.Y = v.Y * 2.0f - 1.0f;

            Vector3 n = new Vector3(v.X, v.Y, 1.0f - Abs(v.X) - Abs(v.Y));
            float t = Clamp<float>(-n.Z, 0.0f, 1.0f);
            n.X = n.X + (n.X > 0.0f ? -t : t);
            n.Y = n.Y + (n.Y > 0.0f ? -t : t);
            n.Normalize();
            return n;
        }


        #region SDK
        public const string ModuleNC = CoreSDK.CoreModule;
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static unsafe Vector3* v3dxPlaneIntersectLine(Vector3* pOut, Plane *pP, Vector3* pV1, Vector3* pV2);
        #endregion
    }
}


#if TitanEngine_AutoGen_Macross
#region TitanEngine_AutoGen_Macross


namespace EngineNS
{
	partial class CustomConvert
	{
		private static EngineNS.Macross.TtMacrossBreak macross_break_ConvertSingleToString_2978353149 = new EngineNS.Macross.TtMacrossBreak("EngineNS.CustomConvert->static string ConvertSingleToString(float val)");
		public static unsafe string macross_ConvertSingleToString (string nodeName, float val) 
		{
			using(var stackframe = EngineNS.Macross.TtMacrossStackTracer.CurrentFrame)
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":val", val);
				}
			}
			var _return_value = ConvertSingleToString(val);
			macross_break_ConvertSingleToString_2978353149.TryBreak();
			return _return_value;
		}
		private static EngineNS.Macross.TtMacrossBreak macross_break_ConvertSingleToByte_2978353149 = new EngineNS.Macross.TtMacrossBreak("EngineNS.CustomConvert->static Byte ConvertSingleToByte(float val)");
		public static unsafe Byte macross_ConvertSingleToByte (string nodeName, float val) 
		{
			using(var stackframe = EngineNS.Macross.TtMacrossStackTracer.CurrentFrame)
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":val", val);
				}
			}
			var _return_value = ConvertSingleToByte(val);
			macross_break_ConvertSingleToByte_2978353149.TryBreak();
			return _return_value;
		}
		private static EngineNS.Macross.TtMacrossBreak macross_break_ConvertSingleToUInt16_2978353149 = new EngineNS.Macross.TtMacrossBreak("EngineNS.CustomConvert->static UInt16 ConvertSingleToUInt16(float val)");
		public static unsafe UInt16 macross_ConvertSingleToUInt16 (string nodeName, float val) 
		{
			using(var stackframe = EngineNS.Macross.TtMacrossStackTracer.CurrentFrame)
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":val", val);
				}
			}
			var _return_value = ConvertSingleToUInt16(val);
			macross_break_ConvertSingleToUInt16_2978353149.TryBreak();
			return _return_value;
		}
		private static EngineNS.Macross.TtMacrossBreak macross_break_ConvertSingleToUInt32_2978353149 = new EngineNS.Macross.TtMacrossBreak("EngineNS.CustomConvert->static UInt32 ConvertSingleToUInt32(float val)");
		public static unsafe UInt32 macross_ConvertSingleToUInt32 (string nodeName, float val) 
		{
			using(var stackframe = EngineNS.Macross.TtMacrossStackTracer.CurrentFrame)
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":val", val);
				}
			}
			var _return_value = ConvertSingleToUInt32(val);
			macross_break_ConvertSingleToUInt32_2978353149.TryBreak();
			return _return_value;
		}
		private static EngineNS.Macross.TtMacrossBreak macross_break_ConvertSingleToUInt64_2978353149 = new EngineNS.Macross.TtMacrossBreak("EngineNS.CustomConvert->static UInt64 ConvertSingleToUInt64(float val)");
		public static unsafe UInt64 macross_ConvertSingleToUInt64 (string nodeName, float val) 
		{
			using(var stackframe = EngineNS.Macross.TtMacrossStackTracer.CurrentFrame)
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":val", val);
				}
			}
			var _return_value = ConvertSingleToUInt64(val);
			macross_break_ConvertSingleToUInt64_2978353149.TryBreak();
			return _return_value;
		}
		private static EngineNS.Macross.TtMacrossBreak macross_break_ConvertSingleToSByte_2978353149 = new EngineNS.Macross.TtMacrossBreak("EngineNS.CustomConvert->static SByte ConvertSingleToSByte(float val)");
		public static unsafe SByte macross_ConvertSingleToSByte (string nodeName, float val) 
		{
			using(var stackframe = EngineNS.Macross.TtMacrossStackTracer.CurrentFrame)
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":val", val);
				}
			}
			var _return_value = ConvertSingleToSByte(val);
			macross_break_ConvertSingleToSByte_2978353149.TryBreak();
			return _return_value;
		}
		private static EngineNS.Macross.TtMacrossBreak macross_break_ConvertSingleToInt16_2978353149 = new EngineNS.Macross.TtMacrossBreak("EngineNS.CustomConvert->static Int16 ConvertSingleToInt16(float val)");
		public static unsafe Int16 macross_ConvertSingleToInt16 (string nodeName, float val) 
		{
			using(var stackframe = EngineNS.Macross.TtMacrossStackTracer.CurrentFrame)
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":val", val);
				}
			}
			var _return_value = ConvertSingleToInt16(val);
			macross_break_ConvertSingleToInt16_2978353149.TryBreak();
			return _return_value;
		}
		private static EngineNS.Macross.TtMacrossBreak macross_break_ConvertSingleToInt32_2978353149 = new EngineNS.Macross.TtMacrossBreak("EngineNS.CustomConvert->static Int32 ConvertSingleToInt32(float val)");
		public static unsafe Int32 macross_ConvertSingleToInt32 (string nodeName, float val) 
		{
			using(var stackframe = EngineNS.Macross.TtMacrossStackTracer.CurrentFrame)
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":val", val);
				}
			}
			var _return_value = ConvertSingleToInt32(val);
			macross_break_ConvertSingleToInt32_2978353149.TryBreak();
			return _return_value;
		}
		private static EngineNS.Macross.TtMacrossBreak macross_break_ConvertSingleToInt64_2978353149 = new EngineNS.Macross.TtMacrossBreak("EngineNS.CustomConvert->static Int64 ConvertSingleToInt64(float val)");
		public static unsafe Int64 macross_ConvertSingleToInt64 (string nodeName, float val) 
		{
			using(var stackframe = EngineNS.Macross.TtMacrossStackTracer.CurrentFrame)
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":val", val);
				}
			}
			var _return_value = ConvertSingleToInt64(val);
			macross_break_ConvertSingleToInt64_2978353149.TryBreak();
			return _return_value;
		}
		private static EngineNS.Macross.TtMacrossBreak macross_break_ConvertSingleToDouble_2978353149 = new EngineNS.Macross.TtMacrossBreak("EngineNS.CustomConvert->static double ConvertSingleToDouble(float val)");
		public static unsafe double macross_ConvertSingleToDouble (string nodeName, float val) 
		{
			using(var stackframe = EngineNS.Macross.TtMacrossStackTracer.CurrentFrame)
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":val", val);
				}
			}
			var _return_value = ConvertSingleToDouble(val);
			macross_break_ConvertSingleToDouble_2978353149.TryBreak();
			return _return_value;
		}
		private static EngineNS.Macross.TtMacrossBreak macross_break_ConvertStringToSingle_4132845348 = new EngineNS.Macross.TtMacrossBreak("EngineNS.CustomConvert->static float ConvertStringToSingle(string val)");
		public static unsafe float macross_ConvertStringToSingle (string nodeName, string val) 
		{
			using(var stackframe = EngineNS.Macross.TtMacrossStackTracer.CurrentFrame)
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":val", val);
				}
			}
			var _return_value = ConvertStringToSingle(val);
			macross_break_ConvertStringToSingle_4132845348.TryBreak();
			return _return_value;
		}
		private static EngineNS.Macross.TtMacrossBreak macross_break_ConvertByteToString_1143746743 = new EngineNS.Macross.TtMacrossBreak("EngineNS.CustomConvert->static string ConvertByteToString(byte val)");
		public static unsafe string macross_ConvertByteToString (string nodeName, byte val) 
		{
			using(var stackframe = EngineNS.Macross.TtMacrossStackTracer.CurrentFrame)
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":val", val);
				}
			}
			var _return_value = ConvertByteToString(val);
			macross_break_ConvertByteToString_1143746743.TryBreak();
			return _return_value;
		}
		private static EngineNS.Macross.TtMacrossBreak macross_break_ConvertByteToUInt16_1143746743 = new EngineNS.Macross.TtMacrossBreak("EngineNS.CustomConvert->static UInt16 ConvertByteToUInt16(byte val)");
		public static unsafe UInt16 macross_ConvertByteToUInt16 (string nodeName, byte val) 
		{
			using(var stackframe = EngineNS.Macross.TtMacrossStackTracer.CurrentFrame)
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":val", val);
				}
			}
			var _return_value = ConvertByteToUInt16(val);
			macross_break_ConvertByteToUInt16_1143746743.TryBreak();
			return _return_value;
		}
		private static EngineNS.Macross.TtMacrossBreak macross_break_ConvertByteToUInt32_1143746743 = new EngineNS.Macross.TtMacrossBreak("EngineNS.CustomConvert->static UInt32 ConvertByteToUInt32(byte val)");
		public static unsafe UInt32 macross_ConvertByteToUInt32 (string nodeName, byte val) 
		{
			using(var stackframe = EngineNS.Macross.TtMacrossStackTracer.CurrentFrame)
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":val", val);
				}
			}
			var _return_value = ConvertByteToUInt32(val);
			macross_break_ConvertByteToUInt32_1143746743.TryBreak();
			return _return_value;
		}
		private static EngineNS.Macross.TtMacrossBreak macross_break_ConvertByteToUInt64_1143746743 = new EngineNS.Macross.TtMacrossBreak("EngineNS.CustomConvert->static UInt64 ConvertByteToUInt64(byte val)");
		public static unsafe UInt64 macross_ConvertByteToUInt64 (string nodeName, byte val) 
		{
			using(var stackframe = EngineNS.Macross.TtMacrossStackTracer.CurrentFrame)
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":val", val);
				}
			}
			var _return_value = ConvertByteToUInt64(val);
			macross_break_ConvertByteToUInt64_1143746743.TryBreak();
			return _return_value;
		}
		private static EngineNS.Macross.TtMacrossBreak macross_break_ConvertByteToSByte_1143746743 = new EngineNS.Macross.TtMacrossBreak("EngineNS.CustomConvert->static SByte ConvertByteToSByte(byte val)");
		public static unsafe SByte macross_ConvertByteToSByte (string nodeName, byte val) 
		{
			using(var stackframe = EngineNS.Macross.TtMacrossStackTracer.CurrentFrame)
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":val", val);
				}
			}
			var _return_value = ConvertByteToSByte(val);
			macross_break_ConvertByteToSByte_1143746743.TryBreak();
			return _return_value;
		}
		private static EngineNS.Macross.TtMacrossBreak macross_break_ConvertByteToInt16_1143746743 = new EngineNS.Macross.TtMacrossBreak("EngineNS.CustomConvert->static Int16 ConvertByteToInt16(byte val)");
		public static unsafe Int16 macross_ConvertByteToInt16 (string nodeName, byte val) 
		{
			using(var stackframe = EngineNS.Macross.TtMacrossStackTracer.CurrentFrame)
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":val", val);
				}
			}
			var _return_value = ConvertByteToInt16(val);
			macross_break_ConvertByteToInt16_1143746743.TryBreak();
			return _return_value;
		}
		private static EngineNS.Macross.TtMacrossBreak macross_break_ConvertByteToInt32_1143746743 = new EngineNS.Macross.TtMacrossBreak("EngineNS.CustomConvert->static Int32 ConvertByteToInt32(byte val)");
		public static unsafe Int32 macross_ConvertByteToInt32 (string nodeName, byte val) 
		{
			using(var stackframe = EngineNS.Macross.TtMacrossStackTracer.CurrentFrame)
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":val", val);
				}
			}
			var _return_value = ConvertByteToInt32(val);
			macross_break_ConvertByteToInt32_1143746743.TryBreak();
			return _return_value;
		}
		private static EngineNS.Macross.TtMacrossBreak macross_break_ConvertByteToInt64_1143746743 = new EngineNS.Macross.TtMacrossBreak("EngineNS.CustomConvert->static Int64 ConvertByteToInt64(byte val)");
		public static unsafe Int64 macross_ConvertByteToInt64 (string nodeName, byte val) 
		{
			using(var stackframe = EngineNS.Macross.TtMacrossStackTracer.CurrentFrame)
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":val", val);
				}
			}
			var _return_value = ConvertByteToInt64(val);
			macross_break_ConvertByteToInt64_1143746743.TryBreak();
			return _return_value;
		}
		private static EngineNS.Macross.TtMacrossBreak macross_break_ConvertByteToSingle_1143746743 = new EngineNS.Macross.TtMacrossBreak("EngineNS.CustomConvert->static float ConvertByteToSingle(byte val)");
		public static unsafe float macross_ConvertByteToSingle (string nodeName, byte val) 
		{
			using(var stackframe = EngineNS.Macross.TtMacrossStackTracer.CurrentFrame)
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":val", val);
				}
			}
			var _return_value = ConvertByteToSingle(val);
			macross_break_ConvertByteToSingle_1143746743.TryBreak();
			return _return_value;
		}
		private static EngineNS.Macross.TtMacrossBreak macross_break_ConvertByteToDouble_1143746743 = new EngineNS.Macross.TtMacrossBreak("EngineNS.CustomConvert->static double ConvertByteToDouble(byte val)");
		public static unsafe double macross_ConvertByteToDouble (string nodeName, byte val) 
		{
			using(var stackframe = EngineNS.Macross.TtMacrossStackTracer.CurrentFrame)
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":val", val);
				}
			}
			var _return_value = ConvertByteToDouble(val);
			macross_break_ConvertByteToDouble_1143746743.TryBreak();
			return _return_value;
		}
		private static EngineNS.Macross.TtMacrossBreak macross_break_ConvertStringToByte_4132845348 = new EngineNS.Macross.TtMacrossBreak("EngineNS.CustomConvert->static byte ConvertStringToByte(string val)");
		public static unsafe byte macross_ConvertStringToByte (string nodeName, string val) 
		{
			using(var stackframe = EngineNS.Macross.TtMacrossStackTracer.CurrentFrame)
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":val", val);
				}
			}
			var _return_value = ConvertStringToByte(val);
			macross_break_ConvertStringToByte_4132845348.TryBreak();
			return _return_value;
		}
		private static EngineNS.Macross.TtMacrossBreak macross_break_ConvertSByteToString_4184858054 = new EngineNS.Macross.TtMacrossBreak("EngineNS.CustomConvert->static string ConvertSByteToString(sbyte val)");
		public static unsafe string macross_ConvertSByteToString (string nodeName, sbyte val) 
		{
			using(var stackframe = EngineNS.Macross.TtMacrossStackTracer.CurrentFrame)
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":val", val);
				}
			}
			var _return_value = ConvertSByteToString(val);
			macross_break_ConvertSByteToString_4184858054.TryBreak();
			return _return_value;
		}
		private static EngineNS.Macross.TtMacrossBreak macross_break_ConvertSByteToByte_4184858054 = new EngineNS.Macross.TtMacrossBreak("EngineNS.CustomConvert->static Byte ConvertSByteToByte(sbyte val)");
		public static unsafe Byte macross_ConvertSByteToByte (string nodeName, sbyte val) 
		{
			using(var stackframe = EngineNS.Macross.TtMacrossStackTracer.CurrentFrame)
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":val", val);
				}
			}
			var _return_value = ConvertSByteToByte(val);
			macross_break_ConvertSByteToByte_4184858054.TryBreak();
			return _return_value;
		}
		private static EngineNS.Macross.TtMacrossBreak macross_break_ConvertSByteToUInt16_4184858054 = new EngineNS.Macross.TtMacrossBreak("EngineNS.CustomConvert->static UInt16 ConvertSByteToUInt16(sbyte val)");
		public static unsafe UInt16 macross_ConvertSByteToUInt16 (string nodeName, sbyte val) 
		{
			using(var stackframe = EngineNS.Macross.TtMacrossStackTracer.CurrentFrame)
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":val", val);
				}
			}
			var _return_value = ConvertSByteToUInt16(val);
			macross_break_ConvertSByteToUInt16_4184858054.TryBreak();
			return _return_value;
		}
		private static EngineNS.Macross.TtMacrossBreak macross_break_ConvertSByteToUInt32_4184858054 = new EngineNS.Macross.TtMacrossBreak("EngineNS.CustomConvert->static UInt32 ConvertSByteToUInt32(sbyte val)");
		public static unsafe UInt32 macross_ConvertSByteToUInt32 (string nodeName, sbyte val) 
		{
			using(var stackframe = EngineNS.Macross.TtMacrossStackTracer.CurrentFrame)
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":val", val);
				}
			}
			var _return_value = ConvertSByteToUInt32(val);
			macross_break_ConvertSByteToUInt32_4184858054.TryBreak();
			return _return_value;
		}
		private static EngineNS.Macross.TtMacrossBreak macross_break_ConvertSByteToUInt64_4184858054 = new EngineNS.Macross.TtMacrossBreak("EngineNS.CustomConvert->static UInt64 ConvertSByteToUInt64(sbyte val)");
		public static unsafe UInt64 macross_ConvertSByteToUInt64 (string nodeName, sbyte val) 
		{
			using(var stackframe = EngineNS.Macross.TtMacrossStackTracer.CurrentFrame)
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":val", val);
				}
			}
			var _return_value = ConvertSByteToUInt64(val);
			macross_break_ConvertSByteToUInt64_4184858054.TryBreak();
			return _return_value;
		}
		private static EngineNS.Macross.TtMacrossBreak macross_break_ConvertSByteToInt16_4184858054 = new EngineNS.Macross.TtMacrossBreak("EngineNS.CustomConvert->static Int16 ConvertSByteToInt16(sbyte val)");
		public static unsafe Int16 macross_ConvertSByteToInt16 (string nodeName, sbyte val) 
		{
			using(var stackframe = EngineNS.Macross.TtMacrossStackTracer.CurrentFrame)
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":val", val);
				}
			}
			var _return_value = ConvertSByteToInt16(val);
			macross_break_ConvertSByteToInt16_4184858054.TryBreak();
			return _return_value;
		}
		private static EngineNS.Macross.TtMacrossBreak macross_break_ConvertSByteToInt32_4184858054 = new EngineNS.Macross.TtMacrossBreak("EngineNS.CustomConvert->static Int32 ConvertSByteToInt32(sbyte val)");
		public static unsafe Int32 macross_ConvertSByteToInt32 (string nodeName, sbyte val) 
		{
			using(var stackframe = EngineNS.Macross.TtMacrossStackTracer.CurrentFrame)
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":val", val);
				}
			}
			var _return_value = ConvertSByteToInt32(val);
			macross_break_ConvertSByteToInt32_4184858054.TryBreak();
			return _return_value;
		}
		private static EngineNS.Macross.TtMacrossBreak macross_break_ConvertSByteToInt64_4184858054 = new EngineNS.Macross.TtMacrossBreak("EngineNS.CustomConvert->static Int64 ConvertSByteToInt64(sbyte val)");
		public static unsafe Int64 macross_ConvertSByteToInt64 (string nodeName, sbyte val) 
		{
			using(var stackframe = EngineNS.Macross.TtMacrossStackTracer.CurrentFrame)
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":val", val);
				}
			}
			var _return_value = ConvertSByteToInt64(val);
			macross_break_ConvertSByteToInt64_4184858054.TryBreak();
			return _return_value;
		}
		private static EngineNS.Macross.TtMacrossBreak macross_break_ConvertSByteToDouble_4184858054 = new EngineNS.Macross.TtMacrossBreak("EngineNS.CustomConvert->static double ConvertSByteToDouble(sbyte val)");
		public static unsafe double macross_ConvertSByteToDouble (string nodeName, sbyte val) 
		{
			using(var stackframe = EngineNS.Macross.TtMacrossStackTracer.CurrentFrame)
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":val", val);
				}
			}
			var _return_value = ConvertSByteToDouble(val);
			macross_break_ConvertSByteToDouble_4184858054.TryBreak();
			return _return_value;
		}
		private static EngineNS.Macross.TtMacrossBreak macross_break_ConvertStringToSByte_4132845348 = new EngineNS.Macross.TtMacrossBreak("EngineNS.CustomConvert->static sbyte ConvertStringToSByte(string val)");
		public static unsafe sbyte macross_ConvertStringToSByte (string nodeName, string val) 
		{
			using(var stackframe = EngineNS.Macross.TtMacrossStackTracer.CurrentFrame)
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":val", val);
				}
			}
			var _return_value = ConvertStringToSByte(val);
			macross_break_ConvertStringToSByte_4132845348.TryBreak();
			return _return_value;
		}
		private static EngineNS.Macross.TtMacrossBreak macross_break_ConvertUInt16ToString_3942474206 = new EngineNS.Macross.TtMacrossBreak("EngineNS.CustomConvert->static string ConvertUInt16ToString(UInt16 val)");
		public static unsafe string macross_ConvertUInt16ToString (string nodeName, UInt16 val) 
		{
			using(var stackframe = EngineNS.Macross.TtMacrossStackTracer.CurrentFrame)
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":val", val);
				}
			}
			var _return_value = ConvertUInt16ToString(val);
			macross_break_ConvertUInt16ToString_3942474206.TryBreak();
			return _return_value;
		}
		private static EngineNS.Macross.TtMacrossBreak macross_break_ConvertUInt16ToByte_3942474206 = new EngineNS.Macross.TtMacrossBreak("EngineNS.CustomConvert->static Byte ConvertUInt16ToByte(UInt16 val)");
		public static unsafe Byte macross_ConvertUInt16ToByte (string nodeName, UInt16 val) 
		{
			using(var stackframe = EngineNS.Macross.TtMacrossStackTracer.CurrentFrame)
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":val", val);
				}
			}
			var _return_value = ConvertUInt16ToByte(val);
			macross_break_ConvertUInt16ToByte_3942474206.TryBreak();
			return _return_value;
		}
		private static EngineNS.Macross.TtMacrossBreak macross_break_ConvertUInt16ToUInt32_3942474206 = new EngineNS.Macross.TtMacrossBreak("EngineNS.CustomConvert->static UInt32 ConvertUInt16ToUInt32(UInt16 val)");
		public static unsafe UInt32 macross_ConvertUInt16ToUInt32 (string nodeName, UInt16 val) 
		{
			using(var stackframe = EngineNS.Macross.TtMacrossStackTracer.CurrentFrame)
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":val", val);
				}
			}
			var _return_value = ConvertUInt16ToUInt32(val);
			macross_break_ConvertUInt16ToUInt32_3942474206.TryBreak();
			return _return_value;
		}
		private static EngineNS.Macross.TtMacrossBreak macross_break_ConvertUInt16ToUInt64_3942474206 = new EngineNS.Macross.TtMacrossBreak("EngineNS.CustomConvert->static UInt64 ConvertUInt16ToUInt64(UInt16 val)");
		public static unsafe UInt64 macross_ConvertUInt16ToUInt64 (string nodeName, UInt16 val) 
		{
			using(var stackframe = EngineNS.Macross.TtMacrossStackTracer.CurrentFrame)
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":val", val);
				}
			}
			var _return_value = ConvertUInt16ToUInt64(val);
			macross_break_ConvertUInt16ToUInt64_3942474206.TryBreak();
			return _return_value;
		}
		private static EngineNS.Macross.TtMacrossBreak macross_break_ConvertUInt16ToSByte_3942474206 = new EngineNS.Macross.TtMacrossBreak("EngineNS.CustomConvert->static SByte ConvertUInt16ToSByte(UInt16 val)");
		public static unsafe SByte macross_ConvertUInt16ToSByte (string nodeName, UInt16 val) 
		{
			using(var stackframe = EngineNS.Macross.TtMacrossStackTracer.CurrentFrame)
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":val", val);
				}
			}
			var _return_value = ConvertUInt16ToSByte(val);
			macross_break_ConvertUInt16ToSByte_3942474206.TryBreak();
			return _return_value;
		}
		private static EngineNS.Macross.TtMacrossBreak macross_break_ConvertUInt16ToInt16_3942474206 = new EngineNS.Macross.TtMacrossBreak("EngineNS.CustomConvert->static Int16 ConvertUInt16ToInt16(UInt16 val)");
		public static unsafe Int16 macross_ConvertUInt16ToInt16 (string nodeName, UInt16 val) 
		{
			using(var stackframe = EngineNS.Macross.TtMacrossStackTracer.CurrentFrame)
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":val", val);
				}
			}
			var _return_value = ConvertUInt16ToInt16(val);
			macross_break_ConvertUInt16ToInt16_3942474206.TryBreak();
			return _return_value;
		}
		private static EngineNS.Macross.TtMacrossBreak macross_break_ConvertUInt16ToInt32_3942474206 = new EngineNS.Macross.TtMacrossBreak("EngineNS.CustomConvert->static Int32 ConvertUInt16ToInt32(UInt16 val)");
		public static unsafe Int32 macross_ConvertUInt16ToInt32 (string nodeName, UInt16 val) 
		{
			using(var stackframe = EngineNS.Macross.TtMacrossStackTracer.CurrentFrame)
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":val", val);
				}
			}
			var _return_value = ConvertUInt16ToInt32(val);
			macross_break_ConvertUInt16ToInt32_3942474206.TryBreak();
			return _return_value;
		}
		private static EngineNS.Macross.TtMacrossBreak macross_break_ConvertUInt16ToInt64_3942474206 = new EngineNS.Macross.TtMacrossBreak("EngineNS.CustomConvert->static Int64 ConvertUInt16ToInt64(UInt16 val)");
		public static unsafe Int64 macross_ConvertUInt16ToInt64 (string nodeName, UInt16 val) 
		{
			using(var stackframe = EngineNS.Macross.TtMacrossStackTracer.CurrentFrame)
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":val", val);
				}
			}
			var _return_value = ConvertUInt16ToInt64(val);
			macross_break_ConvertUInt16ToInt64_3942474206.TryBreak();
			return _return_value;
		}
		private static EngineNS.Macross.TtMacrossBreak macross_break_ConvertUInt16ToSingle_3942474206 = new EngineNS.Macross.TtMacrossBreak("EngineNS.CustomConvert->static float ConvertUInt16ToSingle(UInt16 val)");
		public static unsafe float macross_ConvertUInt16ToSingle (string nodeName, UInt16 val) 
		{
			using(var stackframe = EngineNS.Macross.TtMacrossStackTracer.CurrentFrame)
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":val", val);
				}
			}
			var _return_value = ConvertUInt16ToSingle(val);
			macross_break_ConvertUInt16ToSingle_3942474206.TryBreak();
			return _return_value;
		}
		private static EngineNS.Macross.TtMacrossBreak macross_break_ConvertUInt16ToDouble_3942474206 = new EngineNS.Macross.TtMacrossBreak("EngineNS.CustomConvert->static double ConvertUInt16ToDouble(UInt16 val)");
		public static unsafe double macross_ConvertUInt16ToDouble (string nodeName, UInt16 val) 
		{
			using(var stackframe = EngineNS.Macross.TtMacrossStackTracer.CurrentFrame)
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":val", val);
				}
			}
			var _return_value = ConvertUInt16ToDouble(val);
			macross_break_ConvertUInt16ToDouble_3942474206.TryBreak();
			return _return_value;
		}
		private static EngineNS.Macross.TtMacrossBreak macross_break_ConvertStringToUInt16_4132845348 = new EngineNS.Macross.TtMacrossBreak("EngineNS.CustomConvert->static UInt16 ConvertStringToUInt16(string val)");
		public static unsafe UInt16 macross_ConvertStringToUInt16 (string nodeName, string val) 
		{
			using(var stackframe = EngineNS.Macross.TtMacrossStackTracer.CurrentFrame)
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":val", val);
				}
			}
			var _return_value = ConvertStringToUInt16(val);
			macross_break_ConvertStringToUInt16_4132845348.TryBreak();
			return _return_value;
		}
		private static EngineNS.Macross.TtMacrossBreak macross_break_ConvertUInt32ToString_4017056240 = new EngineNS.Macross.TtMacrossBreak("EngineNS.CustomConvert->static string ConvertUInt32ToString(UInt32 val)");
		public static unsafe string macross_ConvertUInt32ToString (string nodeName, UInt32 val) 
		{
			using(var stackframe = EngineNS.Macross.TtMacrossStackTracer.CurrentFrame)
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":val", val);
				}
			}
			var _return_value = ConvertUInt32ToString(val);
			macross_break_ConvertUInt32ToString_4017056240.TryBreak();
			return _return_value;
		}
		private static EngineNS.Macross.TtMacrossBreak macross_break_ConvertUInt32ToByte_4017056240 = new EngineNS.Macross.TtMacrossBreak("EngineNS.CustomConvert->static Byte ConvertUInt32ToByte(UInt32 val)");
		public static unsafe Byte macross_ConvertUInt32ToByte (string nodeName, UInt32 val) 
		{
			using(var stackframe = EngineNS.Macross.TtMacrossStackTracer.CurrentFrame)
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":val", val);
				}
			}
			var _return_value = ConvertUInt32ToByte(val);
			macross_break_ConvertUInt32ToByte_4017056240.TryBreak();
			return _return_value;
		}
		private static EngineNS.Macross.TtMacrossBreak macross_break_ConvertUInt32ToUInt16_4017056240 = new EngineNS.Macross.TtMacrossBreak("EngineNS.CustomConvert->static UInt16 ConvertUInt32ToUInt16(UInt32 val)");
		public static unsafe UInt16 macross_ConvertUInt32ToUInt16 (string nodeName, UInt32 val) 
		{
			using(var stackframe = EngineNS.Macross.TtMacrossStackTracer.CurrentFrame)
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":val", val);
				}
			}
			var _return_value = ConvertUInt32ToUInt16(val);
			macross_break_ConvertUInt32ToUInt16_4017056240.TryBreak();
			return _return_value;
		}
		private static EngineNS.Macross.TtMacrossBreak macross_break_ConvertUInt32ToUInt32_4017056240 = new EngineNS.Macross.TtMacrossBreak("EngineNS.CustomConvert->static UInt32 ConvertUInt32ToUInt32(UInt32 val)");
		public static unsafe UInt32 macross_ConvertUInt32ToUInt32 (string nodeName, UInt32 val) 
		{
			using(var stackframe = EngineNS.Macross.TtMacrossStackTracer.CurrentFrame)
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":val", val);
				}
			}
			var _return_value = ConvertUInt32ToUInt32(val);
			macross_break_ConvertUInt32ToUInt32_4017056240.TryBreak();
			return _return_value;
		}
		private static EngineNS.Macross.TtMacrossBreak macross_break_ConvertUInt32ToUInt64_4017056240 = new EngineNS.Macross.TtMacrossBreak("EngineNS.CustomConvert->static UInt64 ConvertUInt32ToUInt64(UInt32 val)");
		public static unsafe UInt64 macross_ConvertUInt32ToUInt64 (string nodeName, UInt32 val) 
		{
			using(var stackframe = EngineNS.Macross.TtMacrossStackTracer.CurrentFrame)
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":val", val);
				}
			}
			var _return_value = ConvertUInt32ToUInt64(val);
			macross_break_ConvertUInt32ToUInt64_4017056240.TryBreak();
			return _return_value;
		}
		private static EngineNS.Macross.TtMacrossBreak macross_break_ConvertUInt32ToSByte_4017056240 = new EngineNS.Macross.TtMacrossBreak("EngineNS.CustomConvert->static SByte ConvertUInt32ToSByte(UInt32 val)");
		public static unsafe SByte macross_ConvertUInt32ToSByte (string nodeName, UInt32 val) 
		{
			using(var stackframe = EngineNS.Macross.TtMacrossStackTracer.CurrentFrame)
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":val", val);
				}
			}
			var _return_value = ConvertUInt32ToSByte(val);
			macross_break_ConvertUInt32ToSByte_4017056240.TryBreak();
			return _return_value;
		}
		private static EngineNS.Macross.TtMacrossBreak macross_break_ConvertUInt32ToInt16_4017056240 = new EngineNS.Macross.TtMacrossBreak("EngineNS.CustomConvert->static Int16 ConvertUInt32ToInt16(UInt32 val)");
		public static unsafe Int16 macross_ConvertUInt32ToInt16 (string nodeName, UInt32 val) 
		{
			using(var stackframe = EngineNS.Macross.TtMacrossStackTracer.CurrentFrame)
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":val", val);
				}
			}
			var _return_value = ConvertUInt32ToInt16(val);
			macross_break_ConvertUInt32ToInt16_4017056240.TryBreak();
			return _return_value;
		}
		private static EngineNS.Macross.TtMacrossBreak macross_break_ConvertUInt32ToInt32_4017056240 = new EngineNS.Macross.TtMacrossBreak("EngineNS.CustomConvert->static Int32 ConvertUInt32ToInt32(UInt32 val)");
		public static unsafe Int32 macross_ConvertUInt32ToInt32 (string nodeName, UInt32 val) 
		{
			using(var stackframe = EngineNS.Macross.TtMacrossStackTracer.CurrentFrame)
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":val", val);
				}
			}
			var _return_value = ConvertUInt32ToInt32(val);
			macross_break_ConvertUInt32ToInt32_4017056240.TryBreak();
			return _return_value;
		}
		private static EngineNS.Macross.TtMacrossBreak macross_break_ConvertUInt32ToInt64_4017056240 = new EngineNS.Macross.TtMacrossBreak("EngineNS.CustomConvert->static Int64 ConvertUInt32ToInt64(UInt32 val)");
		public static unsafe Int64 macross_ConvertUInt32ToInt64 (string nodeName, UInt32 val) 
		{
			using(var stackframe = EngineNS.Macross.TtMacrossStackTracer.CurrentFrame)
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":val", val);
				}
			}
			var _return_value = ConvertUInt32ToInt64(val);
			macross_break_ConvertUInt32ToInt64_4017056240.TryBreak();
			return _return_value;
		}
		private static EngineNS.Macross.TtMacrossBreak macross_break_ConvertUInt32ToSingle_4017056240 = new EngineNS.Macross.TtMacrossBreak("EngineNS.CustomConvert->static float ConvertUInt32ToSingle(UInt32 val)");
		public static unsafe float macross_ConvertUInt32ToSingle (string nodeName, UInt32 val) 
		{
			using(var stackframe = EngineNS.Macross.TtMacrossStackTracer.CurrentFrame)
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":val", val);
				}
			}
			var _return_value = ConvertUInt32ToSingle(val);
			macross_break_ConvertUInt32ToSingle_4017056240.TryBreak();
			return _return_value;
		}
		private static EngineNS.Macross.TtMacrossBreak macross_break_ConvertUInt32ToDouble_4017056240 = new EngineNS.Macross.TtMacrossBreak("EngineNS.CustomConvert->static double ConvertUInt32ToDouble(UInt32 val)");
		public static unsafe double macross_ConvertUInt32ToDouble (string nodeName, UInt32 val) 
		{
			using(var stackframe = EngineNS.Macross.TtMacrossStackTracer.CurrentFrame)
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":val", val);
				}
			}
			var _return_value = ConvertUInt32ToDouble(val);
			macross_break_ConvertUInt32ToDouble_4017056240.TryBreak();
			return _return_value;
		}
		private static EngineNS.Macross.TtMacrossBreak macross_break_ConvertStringToUInt32_4132845348 = new EngineNS.Macross.TtMacrossBreak("EngineNS.CustomConvert->static UInt32 ConvertStringToUInt32(string val)");
		public static unsafe UInt32 macross_ConvertStringToUInt32 (string nodeName, string val) 
		{
			using(var stackframe = EngineNS.Macross.TtMacrossStackTracer.CurrentFrame)
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":val", val);
				}
			}
			var _return_value = ConvertStringToUInt32(val);
			macross_break_ConvertStringToUInt32_4132845348.TryBreak();
			return _return_value;
		}
		private static EngineNS.Macross.TtMacrossBreak macross_break_ConvertUInt64ToString_1162032717 = new EngineNS.Macross.TtMacrossBreak("EngineNS.CustomConvert->static string ConvertUInt64ToString(UInt64 val)");
		public static unsafe string macross_ConvertUInt64ToString (string nodeName, UInt64 val) 
		{
			using(var stackframe = EngineNS.Macross.TtMacrossStackTracer.CurrentFrame)
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":val", val);
				}
			}
			var _return_value = ConvertUInt64ToString(val);
			macross_break_ConvertUInt64ToString_1162032717.TryBreak();
			return _return_value;
		}
		private static EngineNS.Macross.TtMacrossBreak macross_break_ConvertUInt64ToByte_1162032717 = new EngineNS.Macross.TtMacrossBreak("EngineNS.CustomConvert->static Byte ConvertUInt64ToByte(UInt64 val)");
		public static unsafe Byte macross_ConvertUInt64ToByte (string nodeName, UInt64 val) 
		{
			using(var stackframe = EngineNS.Macross.TtMacrossStackTracer.CurrentFrame)
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":val", val);
				}
			}
			var _return_value = ConvertUInt64ToByte(val);
			macross_break_ConvertUInt64ToByte_1162032717.TryBreak();
			return _return_value;
		}
		private static EngineNS.Macross.TtMacrossBreak macross_break_ConvertUInt64ToUInt16_1162032717 = new EngineNS.Macross.TtMacrossBreak("EngineNS.CustomConvert->static UInt16 ConvertUInt64ToUInt16(UInt64 val)");
		public static unsafe UInt16 macross_ConvertUInt64ToUInt16 (string nodeName, UInt64 val) 
		{
			using(var stackframe = EngineNS.Macross.TtMacrossStackTracer.CurrentFrame)
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":val", val);
				}
			}
			var _return_value = ConvertUInt64ToUInt16(val);
			macross_break_ConvertUInt64ToUInt16_1162032717.TryBreak();
			return _return_value;
		}
		private static EngineNS.Macross.TtMacrossBreak macross_break_ConvertUInt64ToUInt32_1162032717 = new EngineNS.Macross.TtMacrossBreak("EngineNS.CustomConvert->static UInt32 ConvertUInt64ToUInt32(UInt64 val)");
		public static unsafe UInt32 macross_ConvertUInt64ToUInt32 (string nodeName, UInt64 val) 
		{
			using(var stackframe = EngineNS.Macross.TtMacrossStackTracer.CurrentFrame)
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":val", val);
				}
			}
			var _return_value = ConvertUInt64ToUInt32(val);
			macross_break_ConvertUInt64ToUInt32_1162032717.TryBreak();
			return _return_value;
		}
		private static EngineNS.Macross.TtMacrossBreak macross_break_ConvertUInt64ToSByte_1162032717 = new EngineNS.Macross.TtMacrossBreak("EngineNS.CustomConvert->static SByte ConvertUInt64ToSByte(UInt64 val)");
		public static unsafe SByte macross_ConvertUInt64ToSByte (string nodeName, UInt64 val) 
		{
			using(var stackframe = EngineNS.Macross.TtMacrossStackTracer.CurrentFrame)
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":val", val);
				}
			}
			var _return_value = ConvertUInt64ToSByte(val);
			macross_break_ConvertUInt64ToSByte_1162032717.TryBreak();
			return _return_value;
		}
		private static EngineNS.Macross.TtMacrossBreak macross_break_ConvertUInt64ToInt16_1162032717 = new EngineNS.Macross.TtMacrossBreak("EngineNS.CustomConvert->static Int16 ConvertUInt64ToInt16(UInt64 val)");
		public static unsafe Int16 macross_ConvertUInt64ToInt16 (string nodeName, UInt64 val) 
		{
			using(var stackframe = EngineNS.Macross.TtMacrossStackTracer.CurrentFrame)
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":val", val);
				}
			}
			var _return_value = ConvertUInt64ToInt16(val);
			macross_break_ConvertUInt64ToInt16_1162032717.TryBreak();
			return _return_value;
		}
		private static EngineNS.Macross.TtMacrossBreak macross_break_ConvertUInt64ToInt32_1162032717 = new EngineNS.Macross.TtMacrossBreak("EngineNS.CustomConvert->static Int32 ConvertUInt64ToInt32(UInt64 val)");
		public static unsafe Int32 macross_ConvertUInt64ToInt32 (string nodeName, UInt64 val) 
		{
			using(var stackframe = EngineNS.Macross.TtMacrossStackTracer.CurrentFrame)
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":val", val);
				}
			}
			var _return_value = ConvertUInt64ToInt32(val);
			macross_break_ConvertUInt64ToInt32_1162032717.TryBreak();
			return _return_value;
		}
		private static EngineNS.Macross.TtMacrossBreak macross_break_ConvertUInt64ToInt64_1162032717 = new EngineNS.Macross.TtMacrossBreak("EngineNS.CustomConvert->static Int64 ConvertUInt64ToInt64(UInt64 val)");
		public static unsafe Int64 macross_ConvertUInt64ToInt64 (string nodeName, UInt64 val) 
		{
			using(var stackframe = EngineNS.Macross.TtMacrossStackTracer.CurrentFrame)
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":val", val);
				}
			}
			var _return_value = ConvertUInt64ToInt64(val);
			macross_break_ConvertUInt64ToInt64_1162032717.TryBreak();
			return _return_value;
		}
		private static EngineNS.Macross.TtMacrossBreak macross_break_ConvertUInt64ToSingle_1162032717 = new EngineNS.Macross.TtMacrossBreak("EngineNS.CustomConvert->static float ConvertUInt64ToSingle(UInt64 val)");
		public static unsafe float macross_ConvertUInt64ToSingle (string nodeName, UInt64 val) 
		{
			using(var stackframe = EngineNS.Macross.TtMacrossStackTracer.CurrentFrame)
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":val", val);
				}
			}
			var _return_value = ConvertUInt64ToSingle(val);
			macross_break_ConvertUInt64ToSingle_1162032717.TryBreak();
			return _return_value;
		}
		private static EngineNS.Macross.TtMacrossBreak macross_break_ConvertUInt64ToDouble_1162032717 = new EngineNS.Macross.TtMacrossBreak("EngineNS.CustomConvert->static double ConvertUInt64ToDouble(UInt64 val)");
		public static unsafe double macross_ConvertUInt64ToDouble (string nodeName, UInt64 val) 
		{
			using(var stackframe = EngineNS.Macross.TtMacrossStackTracer.CurrentFrame)
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":val", val);
				}
			}
			var _return_value = ConvertUInt64ToDouble(val);
			macross_break_ConvertUInt64ToDouble_1162032717.TryBreak();
			return _return_value;
		}
		private static EngineNS.Macross.TtMacrossBreak macross_break_ConvertStringToUInt64_4132845348 = new EngineNS.Macross.TtMacrossBreak("EngineNS.CustomConvert->static UInt64 ConvertStringToUInt64(string val)");
		public static unsafe UInt64 macross_ConvertStringToUInt64 (string nodeName, string val) 
		{
			using(var stackframe = EngineNS.Macross.TtMacrossStackTracer.CurrentFrame)
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":val", val);
				}
			}
			var _return_value = ConvertStringToUInt64(val);
			macross_break_ConvertStringToUInt64_4132845348.TryBreak();
			return _return_value;
		}
		private static EngineNS.Macross.TtMacrossBreak macross_break_ConvertInt16ToString_546961797 = new EngineNS.Macross.TtMacrossBreak("EngineNS.CustomConvert->static string ConvertInt16ToString(Int16 val)");
		public static unsafe string macross_ConvertInt16ToString (string nodeName, Int16 val) 
		{
			using(var stackframe = EngineNS.Macross.TtMacrossStackTracer.CurrentFrame)
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":val", val);
				}
			}
			var _return_value = ConvertInt16ToString(val);
			macross_break_ConvertInt16ToString_546961797.TryBreak();
			return _return_value;
		}
		private static EngineNS.Macross.TtMacrossBreak macross_break_ConvertInt16ToByte_546961797 = new EngineNS.Macross.TtMacrossBreak("EngineNS.CustomConvert->static Byte ConvertInt16ToByte(Int16 val)");
		public static unsafe Byte macross_ConvertInt16ToByte (string nodeName, Int16 val) 
		{
			using(var stackframe = EngineNS.Macross.TtMacrossStackTracer.CurrentFrame)
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":val", val);
				}
			}
			var _return_value = ConvertInt16ToByte(val);
			macross_break_ConvertInt16ToByte_546961797.TryBreak();
			return _return_value;
		}
		private static EngineNS.Macross.TtMacrossBreak macross_break_ConvertInt16ToUInt16_546961797 = new EngineNS.Macross.TtMacrossBreak("EngineNS.CustomConvert->static UInt16 ConvertInt16ToUInt16(Int16 val)");
		public static unsafe UInt16 macross_ConvertInt16ToUInt16 (string nodeName, Int16 val) 
		{
			using(var stackframe = EngineNS.Macross.TtMacrossStackTracer.CurrentFrame)
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":val", val);
				}
			}
			var _return_value = ConvertInt16ToUInt16(val);
			macross_break_ConvertInt16ToUInt16_546961797.TryBreak();
			return _return_value;
		}
		private static EngineNS.Macross.TtMacrossBreak macross_break_ConvertInt16ToUInt32_546961797 = new EngineNS.Macross.TtMacrossBreak("EngineNS.CustomConvert->static UInt32 ConvertInt16ToUInt32(Int16 val)");
		public static unsafe UInt32 macross_ConvertInt16ToUInt32 (string nodeName, Int16 val) 
		{
			using(var stackframe = EngineNS.Macross.TtMacrossStackTracer.CurrentFrame)
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":val", val);
				}
			}
			var _return_value = ConvertInt16ToUInt32(val);
			macross_break_ConvertInt16ToUInt32_546961797.TryBreak();
			return _return_value;
		}
		private static EngineNS.Macross.TtMacrossBreak macross_break_ConvertInt16ToUInt64_546961797 = new EngineNS.Macross.TtMacrossBreak("EngineNS.CustomConvert->static UInt64 ConvertInt16ToUInt64(Int16 val)");
		public static unsafe UInt64 macross_ConvertInt16ToUInt64 (string nodeName, Int16 val) 
		{
			using(var stackframe = EngineNS.Macross.TtMacrossStackTracer.CurrentFrame)
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":val", val);
				}
			}
			var _return_value = ConvertInt16ToUInt64(val);
			macross_break_ConvertInt16ToUInt64_546961797.TryBreak();
			return _return_value;
		}
		private static EngineNS.Macross.TtMacrossBreak macross_break_ConvertInt16ToSByte_546961797 = new EngineNS.Macross.TtMacrossBreak("EngineNS.CustomConvert->static SByte ConvertInt16ToSByte(Int16 val)");
		public static unsafe SByte macross_ConvertInt16ToSByte (string nodeName, Int16 val) 
		{
			using(var stackframe = EngineNS.Macross.TtMacrossStackTracer.CurrentFrame)
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":val", val);
				}
			}
			var _return_value = ConvertInt16ToSByte(val);
			macross_break_ConvertInt16ToSByte_546961797.TryBreak();
			return _return_value;
		}
		private static EngineNS.Macross.TtMacrossBreak macross_break_ConvertInt16ToInt32_546961797 = new EngineNS.Macross.TtMacrossBreak("EngineNS.CustomConvert->static Int32 ConvertInt16ToInt32(Int16 val)");
		public static unsafe Int32 macross_ConvertInt16ToInt32 (string nodeName, Int16 val) 
		{
			using(var stackframe = EngineNS.Macross.TtMacrossStackTracer.CurrentFrame)
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":val", val);
				}
			}
			var _return_value = ConvertInt16ToInt32(val);
			macross_break_ConvertInt16ToInt32_546961797.TryBreak();
			return _return_value;
		}
		private static EngineNS.Macross.TtMacrossBreak macross_break_ConvertInt16ToInt64_546961797 = new EngineNS.Macross.TtMacrossBreak("EngineNS.CustomConvert->static Int64 ConvertInt16ToInt64(Int16 val)");
		public static unsafe Int64 macross_ConvertInt16ToInt64 (string nodeName, Int16 val) 
		{
			using(var stackframe = EngineNS.Macross.TtMacrossStackTracer.CurrentFrame)
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":val", val);
				}
			}
			var _return_value = ConvertInt16ToInt64(val);
			macross_break_ConvertInt16ToInt64_546961797.TryBreak();
			return _return_value;
		}
		private static EngineNS.Macross.TtMacrossBreak macross_break_ConvertInt16ToSingle_546961797 = new EngineNS.Macross.TtMacrossBreak("EngineNS.CustomConvert->static float ConvertInt16ToSingle(Int16 val)");
		public static unsafe float macross_ConvertInt16ToSingle (string nodeName, Int16 val) 
		{
			using(var stackframe = EngineNS.Macross.TtMacrossStackTracer.CurrentFrame)
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":val", val);
				}
			}
			var _return_value = ConvertInt16ToSingle(val);
			macross_break_ConvertInt16ToSingle_546961797.TryBreak();
			return _return_value;
		}
		private static EngineNS.Macross.TtMacrossBreak macross_break_ConvertInt16ToDouble_546961797 = new EngineNS.Macross.TtMacrossBreak("EngineNS.CustomConvert->static double ConvertInt16ToDouble(Int16 val)");
		public static unsafe double macross_ConvertInt16ToDouble (string nodeName, Int16 val) 
		{
			using(var stackframe = EngineNS.Macross.TtMacrossStackTracer.CurrentFrame)
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":val", val);
				}
			}
			var _return_value = ConvertInt16ToDouble(val);
			macross_break_ConvertInt16ToDouble_546961797.TryBreak();
			return _return_value;
		}
		private static EngineNS.Macross.TtMacrossBreak macross_break_ConvertStringToInt16_4132845348 = new EngineNS.Macross.TtMacrossBreak("EngineNS.CustomConvert->static Int16 ConvertStringToInt16(string val)");
		public static unsafe Int16 macross_ConvertStringToInt16 (string nodeName, string val) 
		{
			using(var stackframe = EngineNS.Macross.TtMacrossStackTracer.CurrentFrame)
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":val", val);
				}
			}
			var _return_value = ConvertStringToInt16(val);
			macross_break_ConvertStringToInt16_4132845348.TryBreak();
			return _return_value;
		}
		private static EngineNS.Macross.TtMacrossBreak macross_break_ConvertInt32ToString_2747689535 = new EngineNS.Macross.TtMacrossBreak("EngineNS.CustomConvert->static string ConvertInt32ToString(Int32 val)");
		public static unsafe string macross_ConvertInt32ToString (string nodeName, Int32 val) 
		{
			using(var stackframe = EngineNS.Macross.TtMacrossStackTracer.CurrentFrame)
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":val", val);
				}
			}
			var _return_value = ConvertInt32ToString(val);
			macross_break_ConvertInt32ToString_2747689535.TryBreak();
			return _return_value;
		}
		private static EngineNS.Macross.TtMacrossBreak macross_break_ConvertInt32ToByte_2747689535 = new EngineNS.Macross.TtMacrossBreak("EngineNS.CustomConvert->static Byte ConvertInt32ToByte(Int32 val)");
		public static unsafe Byte macross_ConvertInt32ToByte (string nodeName, Int32 val) 
		{
			using(var stackframe = EngineNS.Macross.TtMacrossStackTracer.CurrentFrame)
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":val", val);
				}
			}
			var _return_value = ConvertInt32ToByte(val);
			macross_break_ConvertInt32ToByte_2747689535.TryBreak();
			return _return_value;
		}
		private static EngineNS.Macross.TtMacrossBreak macross_break_ConvertInt32ToUInt16_2747689535 = new EngineNS.Macross.TtMacrossBreak("EngineNS.CustomConvert->static UInt16 ConvertInt32ToUInt16(Int32 val)");
		public static unsafe UInt16 macross_ConvertInt32ToUInt16 (string nodeName, Int32 val) 
		{
			using(var stackframe = EngineNS.Macross.TtMacrossStackTracer.CurrentFrame)
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":val", val);
				}
			}
			var _return_value = ConvertInt32ToUInt16(val);
			macross_break_ConvertInt32ToUInt16_2747689535.TryBreak();
			return _return_value;
		}
		private static EngineNS.Macross.TtMacrossBreak macross_break_ConvertInt32ToUInt32_2747689535 = new EngineNS.Macross.TtMacrossBreak("EngineNS.CustomConvert->static UInt32 ConvertInt32ToUInt32(Int32 val)");
		public static unsafe UInt32 macross_ConvertInt32ToUInt32 (string nodeName, Int32 val) 
		{
			using(var stackframe = EngineNS.Macross.TtMacrossStackTracer.CurrentFrame)
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":val", val);
				}
			}
			var _return_value = ConvertInt32ToUInt32(val);
			macross_break_ConvertInt32ToUInt32_2747689535.TryBreak();
			return _return_value;
		}
		private static EngineNS.Macross.TtMacrossBreak macross_break_ConvertInt32ToUInt64_2747689535 = new EngineNS.Macross.TtMacrossBreak("EngineNS.CustomConvert->static UInt64 ConvertInt32ToUInt64(Int32 val)");
		public static unsafe UInt64 macross_ConvertInt32ToUInt64 (string nodeName, Int32 val) 
		{
			using(var stackframe = EngineNS.Macross.TtMacrossStackTracer.CurrentFrame)
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":val", val);
				}
			}
			var _return_value = ConvertInt32ToUInt64(val);
			macross_break_ConvertInt32ToUInt64_2747689535.TryBreak();
			return _return_value;
		}
		private static EngineNS.Macross.TtMacrossBreak macross_break_ConvertInt32ToSByte_2747689535 = new EngineNS.Macross.TtMacrossBreak("EngineNS.CustomConvert->static SByte ConvertInt32ToSByte(Int32 val)");
		public static unsafe SByte macross_ConvertInt32ToSByte (string nodeName, Int32 val) 
		{
			using(var stackframe = EngineNS.Macross.TtMacrossStackTracer.CurrentFrame)
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":val", val);
				}
			}
			var _return_value = ConvertInt32ToSByte(val);
			macross_break_ConvertInt32ToSByte_2747689535.TryBreak();
			return _return_value;
		}
		private static EngineNS.Macross.TtMacrossBreak macross_break_ConvertInt32ToInt16_2747689535 = new EngineNS.Macross.TtMacrossBreak("EngineNS.CustomConvert->static Int16 ConvertInt32ToInt16(Int32 val)");
		public static unsafe Int16 macross_ConvertInt32ToInt16 (string nodeName, Int32 val) 
		{
			using(var stackframe = EngineNS.Macross.TtMacrossStackTracer.CurrentFrame)
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":val", val);
				}
			}
			var _return_value = ConvertInt32ToInt16(val);
			macross_break_ConvertInt32ToInt16_2747689535.TryBreak();
			return _return_value;
		}
		private static EngineNS.Macross.TtMacrossBreak macross_break_ConvertInt32ToInt64_2747689535 = new EngineNS.Macross.TtMacrossBreak("EngineNS.CustomConvert->static Int64 ConvertInt32ToInt64(Int32 val)");
		public static unsafe Int64 macross_ConvertInt32ToInt64 (string nodeName, Int32 val) 
		{
			using(var stackframe = EngineNS.Macross.TtMacrossStackTracer.CurrentFrame)
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":val", val);
				}
			}
			var _return_value = ConvertInt32ToInt64(val);
			macross_break_ConvertInt32ToInt64_2747689535.TryBreak();
			return _return_value;
		}
		private static EngineNS.Macross.TtMacrossBreak macross_break_ConvertInt32ToSingle_2747689535 = new EngineNS.Macross.TtMacrossBreak("EngineNS.CustomConvert->static float ConvertInt32ToSingle(Int32 val)");
		public static unsafe float macross_ConvertInt32ToSingle (string nodeName, Int32 val) 
		{
			using(var stackframe = EngineNS.Macross.TtMacrossStackTracer.CurrentFrame)
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":val", val);
				}
			}
			var _return_value = ConvertInt32ToSingle(val);
			macross_break_ConvertInt32ToSingle_2747689535.TryBreak();
			return _return_value;
		}
		private static EngineNS.Macross.TtMacrossBreak macross_break_ConvertInt32ToDouble_2747689535 = new EngineNS.Macross.TtMacrossBreak("EngineNS.CustomConvert->static double ConvertInt32ToDouble(Int32 val)");
		public static unsafe double macross_ConvertInt32ToDouble (string nodeName, Int32 val) 
		{
			using(var stackframe = EngineNS.Macross.TtMacrossStackTracer.CurrentFrame)
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":val", val);
				}
			}
			var _return_value = ConvertInt32ToDouble(val);
			macross_break_ConvertInt32ToDouble_2747689535.TryBreak();
			return _return_value;
		}
		private static EngineNS.Macross.TtMacrossBreak macross_break_ConvertStringToInt32_4132845348 = new EngineNS.Macross.TtMacrossBreak("EngineNS.CustomConvert->static Int32 ConvertStringToInt32(string val)");
		public static unsafe Int32 macross_ConvertStringToInt32 (string nodeName, string val) 
		{
			using(var stackframe = EngineNS.Macross.TtMacrossStackTracer.CurrentFrame)
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":val", val);
				}
			}
			var _return_value = ConvertStringToInt32(val);
			macross_break_ConvertStringToInt32_4132845348.TryBreak();
			return _return_value;
		}
		private static EngineNS.Macross.TtMacrossBreak macross_break_ConvertInt64ToString_2285206386 = new EngineNS.Macross.TtMacrossBreak("EngineNS.CustomConvert->static string ConvertInt64ToString(Int64 val)");
		public static unsafe string macross_ConvertInt64ToString (string nodeName, Int64 val) 
		{
			using(var stackframe = EngineNS.Macross.TtMacrossStackTracer.CurrentFrame)
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":val", val);
				}
			}
			var _return_value = ConvertInt64ToString(val);
			macross_break_ConvertInt64ToString_2285206386.TryBreak();
			return _return_value;
		}
		private static EngineNS.Macross.TtMacrossBreak macross_break_ConvertInt64ToByte_2285206386 = new EngineNS.Macross.TtMacrossBreak("EngineNS.CustomConvert->static Byte ConvertInt64ToByte(Int64 val)");
		public static unsafe Byte macross_ConvertInt64ToByte (string nodeName, Int64 val) 
		{
			using(var stackframe = EngineNS.Macross.TtMacrossStackTracer.CurrentFrame)
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":val", val);
				}
			}
			var _return_value = ConvertInt64ToByte(val);
			macross_break_ConvertInt64ToByte_2285206386.TryBreak();
			return _return_value;
		}
		private static EngineNS.Macross.TtMacrossBreak macross_break_ConvertInt64ToUInt16_2285206386 = new EngineNS.Macross.TtMacrossBreak("EngineNS.CustomConvert->static UInt16 ConvertInt64ToUInt16(Int64 val)");
		public static unsafe UInt16 macross_ConvertInt64ToUInt16 (string nodeName, Int64 val) 
		{
			using(var stackframe = EngineNS.Macross.TtMacrossStackTracer.CurrentFrame)
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":val", val);
				}
			}
			var _return_value = ConvertInt64ToUInt16(val);
			macross_break_ConvertInt64ToUInt16_2285206386.TryBreak();
			return _return_value;
		}
		private static EngineNS.Macross.TtMacrossBreak macross_break_ConvertInt64ToUInt32_2285206386 = new EngineNS.Macross.TtMacrossBreak("EngineNS.CustomConvert->static UInt32 ConvertInt64ToUInt32(Int64 val)");
		public static unsafe UInt32 macross_ConvertInt64ToUInt32 (string nodeName, Int64 val) 
		{
			using(var stackframe = EngineNS.Macross.TtMacrossStackTracer.CurrentFrame)
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":val", val);
				}
			}
			var _return_value = ConvertInt64ToUInt32(val);
			macross_break_ConvertInt64ToUInt32_2285206386.TryBreak();
			return _return_value;
		}
		private static EngineNS.Macross.TtMacrossBreak macross_break_ConvertInt64ToUInt64_2285206386 = new EngineNS.Macross.TtMacrossBreak("EngineNS.CustomConvert->static UInt64 ConvertInt64ToUInt64(Int64 val)");
		public static unsafe UInt64 macross_ConvertInt64ToUInt64 (string nodeName, Int64 val) 
		{
			using(var stackframe = EngineNS.Macross.TtMacrossStackTracer.CurrentFrame)
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":val", val);
				}
			}
			var _return_value = ConvertInt64ToUInt64(val);
			macross_break_ConvertInt64ToUInt64_2285206386.TryBreak();
			return _return_value;
		}
		private static EngineNS.Macross.TtMacrossBreak macross_break_ConvertInt64ToSByte_2285206386 = new EngineNS.Macross.TtMacrossBreak("EngineNS.CustomConvert->static SByte ConvertInt64ToSByte(Int64 val)");
		public static unsafe SByte macross_ConvertInt64ToSByte (string nodeName, Int64 val) 
		{
			using(var stackframe = EngineNS.Macross.TtMacrossStackTracer.CurrentFrame)
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":val", val);
				}
			}
			var _return_value = ConvertInt64ToSByte(val);
			macross_break_ConvertInt64ToSByte_2285206386.TryBreak();
			return _return_value;
		}
		private static EngineNS.Macross.TtMacrossBreak macross_break_ConvertInt64ToInt16_2285206386 = new EngineNS.Macross.TtMacrossBreak("EngineNS.CustomConvert->static Int16 ConvertInt64ToInt16(Int64 val)");
		public static unsafe Int16 macross_ConvertInt64ToInt16 (string nodeName, Int64 val) 
		{
			using(var stackframe = EngineNS.Macross.TtMacrossStackTracer.CurrentFrame)
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":val", val);
				}
			}
			var _return_value = ConvertInt64ToInt16(val);
			macross_break_ConvertInt64ToInt16_2285206386.TryBreak();
			return _return_value;
		}
		private static EngineNS.Macross.TtMacrossBreak macross_break_ConvertInt64ToInt32_2285206386 = new EngineNS.Macross.TtMacrossBreak("EngineNS.CustomConvert->static Int32 ConvertInt64ToInt32(Int64 val)");
		public static unsafe Int32 macross_ConvertInt64ToInt32 (string nodeName, Int64 val) 
		{
			using(var stackframe = EngineNS.Macross.TtMacrossStackTracer.CurrentFrame)
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":val", val);
				}
			}
			var _return_value = ConvertInt64ToInt32(val);
			macross_break_ConvertInt64ToInt32_2285206386.TryBreak();
			return _return_value;
		}
		private static EngineNS.Macross.TtMacrossBreak macross_break_ConvertInt64ToSingle_2285206386 = new EngineNS.Macross.TtMacrossBreak("EngineNS.CustomConvert->static float ConvertInt64ToSingle(Int64 val)");
		public static unsafe float macross_ConvertInt64ToSingle (string nodeName, Int64 val) 
		{
			using(var stackframe = EngineNS.Macross.TtMacrossStackTracer.CurrentFrame)
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":val", val);
				}
			}
			var _return_value = ConvertInt64ToSingle(val);
			macross_break_ConvertInt64ToSingle_2285206386.TryBreak();
			return _return_value;
		}
		private static EngineNS.Macross.TtMacrossBreak macross_break_ConvertInt64ToDouble_2285206386 = new EngineNS.Macross.TtMacrossBreak("EngineNS.CustomConvert->static double ConvertInt64ToDouble(Int64 val)");
		public static unsafe double macross_ConvertInt64ToDouble (string nodeName, Int64 val) 
		{
			using(var stackframe = EngineNS.Macross.TtMacrossStackTracer.CurrentFrame)
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":val", val);
				}
			}
			var _return_value = ConvertInt64ToDouble(val);
			macross_break_ConvertInt64ToDouble_2285206386.TryBreak();
			return _return_value;
		}
		private static EngineNS.Macross.TtMacrossBreak macross_break_ConvertStringToInt64_4132845348 = new EngineNS.Macross.TtMacrossBreak("EngineNS.CustomConvert->static Int64 ConvertStringToInt64(string val)");
		public static unsafe Int64 macross_ConvertStringToInt64 (string nodeName, string val) 
		{
			using(var stackframe = EngineNS.Macross.TtMacrossStackTracer.CurrentFrame)
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":val", val);
				}
			}
			var _return_value = ConvertStringToInt64(val);
			macross_break_ConvertStringToInt64_4132845348.TryBreak();
			return _return_value;
		}
		private static EngineNS.Macross.TtMacrossBreak macross_break_ConvertDoubleToString_1143615832 = new EngineNS.Macross.TtMacrossBreak("EngineNS.CustomConvert->static string ConvertDoubleToString(double val)");
		public static unsafe string macross_ConvertDoubleToString (string nodeName, double val) 
		{
			using(var stackframe = EngineNS.Macross.TtMacrossStackTracer.CurrentFrame)
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":val", val);
				}
			}
			var _return_value = ConvertDoubleToString(val);
			macross_break_ConvertDoubleToString_1143615832.TryBreak();
			return _return_value;
		}
		private static EngineNS.Macross.TtMacrossBreak macross_break_ConvertDoubleToByte_1143615832 = new EngineNS.Macross.TtMacrossBreak("EngineNS.CustomConvert->static Byte ConvertDoubleToByte(double val)");
		public static unsafe Byte macross_ConvertDoubleToByte (string nodeName, double val) 
		{
			using(var stackframe = EngineNS.Macross.TtMacrossStackTracer.CurrentFrame)
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":val", val);
				}
			}
			var _return_value = ConvertDoubleToByte(val);
			macross_break_ConvertDoubleToByte_1143615832.TryBreak();
			return _return_value;
		}
		private static EngineNS.Macross.TtMacrossBreak macross_break_ConvertDoubleToUInt16_1143615832 = new EngineNS.Macross.TtMacrossBreak("EngineNS.CustomConvert->static UInt16 ConvertDoubleToUInt16(double val)");
		public static unsafe UInt16 macross_ConvertDoubleToUInt16 (string nodeName, double val) 
		{
			using(var stackframe = EngineNS.Macross.TtMacrossStackTracer.CurrentFrame)
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":val", val);
				}
			}
			var _return_value = ConvertDoubleToUInt16(val);
			macross_break_ConvertDoubleToUInt16_1143615832.TryBreak();
			return _return_value;
		}
		private static EngineNS.Macross.TtMacrossBreak macross_break_ConvertDoubleToUInt32_1143615832 = new EngineNS.Macross.TtMacrossBreak("EngineNS.CustomConvert->static UInt32 ConvertDoubleToUInt32(double val)");
		public static unsafe UInt32 macross_ConvertDoubleToUInt32 (string nodeName, double val) 
		{
			using(var stackframe = EngineNS.Macross.TtMacrossStackTracer.CurrentFrame)
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":val", val);
				}
			}
			var _return_value = ConvertDoubleToUInt32(val);
			macross_break_ConvertDoubleToUInt32_1143615832.TryBreak();
			return _return_value;
		}
		private static EngineNS.Macross.TtMacrossBreak macross_break_ConvertDoubleToUInt64_1143615832 = new EngineNS.Macross.TtMacrossBreak("EngineNS.CustomConvert->static UInt64 ConvertDoubleToUInt64(double val)");
		public static unsafe UInt64 macross_ConvertDoubleToUInt64 (string nodeName, double val) 
		{
			using(var stackframe = EngineNS.Macross.TtMacrossStackTracer.CurrentFrame)
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":val", val);
				}
			}
			var _return_value = ConvertDoubleToUInt64(val);
			macross_break_ConvertDoubleToUInt64_1143615832.TryBreak();
			return _return_value;
		}
		private static EngineNS.Macross.TtMacrossBreak macross_break_ConvertDoubleToSByte_1143615832 = new EngineNS.Macross.TtMacrossBreak("EngineNS.CustomConvert->static SByte ConvertDoubleToSByte(double val)");
		public static unsafe SByte macross_ConvertDoubleToSByte (string nodeName, double val) 
		{
			using(var stackframe = EngineNS.Macross.TtMacrossStackTracer.CurrentFrame)
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":val", val);
				}
			}
			var _return_value = ConvertDoubleToSByte(val);
			macross_break_ConvertDoubleToSByte_1143615832.TryBreak();
			return _return_value;
		}
		private static EngineNS.Macross.TtMacrossBreak macross_break_ConvertDoubleToInt16_1143615832 = new EngineNS.Macross.TtMacrossBreak("EngineNS.CustomConvert->static Int16 ConvertDoubleToInt16(double val)");
		public static unsafe Int16 macross_ConvertDoubleToInt16 (string nodeName, double val) 
		{
			using(var stackframe = EngineNS.Macross.TtMacrossStackTracer.CurrentFrame)
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":val", val);
				}
			}
			var _return_value = ConvertDoubleToInt16(val);
			macross_break_ConvertDoubleToInt16_1143615832.TryBreak();
			return _return_value;
		}
		private static EngineNS.Macross.TtMacrossBreak macross_break_ConvertDoubleToInt32_1143615832 = new EngineNS.Macross.TtMacrossBreak("EngineNS.CustomConvert->static Int32 ConvertDoubleToInt32(double val)");
		public static unsafe Int32 macross_ConvertDoubleToInt32 (string nodeName, double val) 
		{
			using(var stackframe = EngineNS.Macross.TtMacrossStackTracer.CurrentFrame)
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":val", val);
				}
			}
			var _return_value = ConvertDoubleToInt32(val);
			macross_break_ConvertDoubleToInt32_1143615832.TryBreak();
			return _return_value;
		}
		private static EngineNS.Macross.TtMacrossBreak macross_break_ConvertDoubleToInt64_1143615832 = new EngineNS.Macross.TtMacrossBreak("EngineNS.CustomConvert->static Int64 ConvertDoubleToInt64(double val)");
		public static unsafe Int64 macross_ConvertDoubleToInt64 (string nodeName, double val) 
		{
			using(var stackframe = EngineNS.Macross.TtMacrossStackTracer.CurrentFrame)
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":val", val);
				}
			}
			var _return_value = ConvertDoubleToInt64(val);
			macross_break_ConvertDoubleToInt64_1143615832.TryBreak();
			return _return_value;
		}
		private static EngineNS.Macross.TtMacrossBreak macross_break_ConvertDoubleToSingle_1143615832 = new EngineNS.Macross.TtMacrossBreak("EngineNS.CustomConvert->static float ConvertDoubleToSingle(double val)");
		public static unsafe float macross_ConvertDoubleToSingle (string nodeName, double val) 
		{
			using(var stackframe = EngineNS.Macross.TtMacrossStackTracer.CurrentFrame)
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":val", val);
				}
			}
			var _return_value = ConvertDoubleToSingle(val);
			macross_break_ConvertDoubleToSingle_1143615832.TryBreak();
			return _return_value;
		}
		private static EngineNS.Macross.TtMacrossBreak macross_break_ConvertStringToDouble_4132845348 = new EngineNS.Macross.TtMacrossBreak("EngineNS.CustomConvert->static double ConvertStringToDouble(string val)");
		public static unsafe double macross_ConvertStringToDouble (string nodeName, string val) 
		{
			using(var stackframe = EngineNS.Macross.TtMacrossStackTracer.CurrentFrame)
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":val", val);
				}
			}
			var _return_value = ConvertStringToDouble(val);
			macross_break_ConvertStringToDouble_4132845348.TryBreak();
			return _return_value;
		}
	}
}


namespace EngineNS
{
	partial class MathHelper
	{
		private static EngineNS.Macross.TtMacrossBreak macross_break_Abs_650818214 = new EngineNS.Macross.TtMacrossBreak("EngineNS.MathHelper->static float Abs(float v)");
		public static unsafe float macross_Abs (string nodeName, float v) 
		{
			using(var stackframe = EngineNS.Macross.TtMacrossStackTracer.CurrentFrame)
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":v", v);
				}
			}
			var _return_value = Abs(v);
			macross_break_Abs_650818214.TryBreak();
			return _return_value;
		}
		private static EngineNS.Macross.TtMacrossBreak macross_break_Mod_3608637309 = new EngineNS.Macross.TtMacrossBreak("EngineNS.MathHelper->static float Mod(float v1, float v2)");
		public static unsafe float macross_Mod (string nodeName, float v1, float v2) 
		{
			using(var stackframe = EngineNS.Macross.TtMacrossStackTracer.CurrentFrame)
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":v1", v1);
					stackframe.SetWatchVariable(nodeName + ":v2", v2);
				}
			}
			var _return_value = Mod(v1, v2);
			macross_break_Mod_3608637309.TryBreak();
			return _return_value;
		}
		private static EngineNS.Macross.TtMacrossBreak macross_break_Sin_650818214 = new EngineNS.Macross.TtMacrossBreak("EngineNS.MathHelper->static float Sin(float v)");
		public static unsafe float macross_Sin (string nodeName, float v) 
		{
			using(var stackframe = EngineNS.Macross.TtMacrossStackTracer.CurrentFrame)
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":v", v);
				}
			}
			var _return_value = Sin(v);
			macross_break_Sin_650818214.TryBreak();
			return _return_value;
		}
		private static EngineNS.Macross.TtMacrossBreak macross_break_Asin_650818214 = new EngineNS.Macross.TtMacrossBreak("EngineNS.MathHelper->static float Asin(float v)");
		public static unsafe float macross_Asin (string nodeName, float v) 
		{
			using(var stackframe = EngineNS.Macross.TtMacrossStackTracer.CurrentFrame)
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":v", v);
				}
			}
			var _return_value = Asin(v);
			macross_break_Asin_650818214.TryBreak();
			return _return_value;
		}
		private static EngineNS.Macross.TtMacrossBreak macross_break_Cos_650818214 = new EngineNS.Macross.TtMacrossBreak("EngineNS.MathHelper->static float Cos(float v)");
		public static unsafe float macross_Cos (string nodeName, float v) 
		{
			using(var stackframe = EngineNS.Macross.TtMacrossStackTracer.CurrentFrame)
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":v", v);
				}
			}
			var _return_value = Cos(v);
			macross_break_Cos_650818214.TryBreak();
			return _return_value;
		}
		private static EngineNS.Macross.TtMacrossBreak macross_break_Acos_650818214 = new EngineNS.Macross.TtMacrossBreak("EngineNS.MathHelper->static float Acos(float v)");
		public static unsafe float macross_Acos (string nodeName, float v) 
		{
			using(var stackframe = EngineNS.Macross.TtMacrossStackTracer.CurrentFrame)
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":v", v);
				}
			}
			var _return_value = Acos(v);
			macross_break_Acos_650818214.TryBreak();
			return _return_value;
		}
		private static EngineNS.Macross.TtMacrossBreak macross_break_Tan_650818214 = new EngineNS.Macross.TtMacrossBreak("EngineNS.MathHelper->static float Tan(float v)");
		public static unsafe float macross_Tan (string nodeName, float v) 
		{
			using(var stackframe = EngineNS.Macross.TtMacrossStackTracer.CurrentFrame)
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":v", v);
				}
			}
			var _return_value = Tan(v);
			macross_break_Tan_650818214.TryBreak();
			return _return_value;
		}
		private static EngineNS.Macross.TtMacrossBreak macross_break_Atan_650818214 = new EngineNS.Macross.TtMacrossBreak("EngineNS.MathHelper->static float Atan(float v)");
		public static unsafe float macross_Atan (string nodeName, float v) 
		{
			using(var stackframe = EngineNS.Macross.TtMacrossStackTracer.CurrentFrame)
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":v", v);
				}
			}
			var _return_value = Atan(v);
			macross_break_Atan_650818214.TryBreak();
			return _return_value;
		}
		private static EngineNS.Macross.TtMacrossBreak macross_break_Atan2_2642596091 = new EngineNS.Macross.TtMacrossBreak("EngineNS.MathHelper->static float Atan2(float y, float x)");
		public static unsafe float macross_Atan2 (string nodeName, float y, float x) 
		{
			using(var stackframe = EngineNS.Macross.TtMacrossStackTracer.CurrentFrame)
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":y", y);
					stackframe.SetWatchVariable(nodeName + ":x", x);
				}
			}
			var _return_value = Atan2(y, x);
			macross_break_Atan2_2642596091.TryBreak();
			return _return_value;
		}
		private static EngineNS.Macross.TtMacrossBreak macross_break_Pow_2776435935 = new EngineNS.Macross.TtMacrossBreak("EngineNS.MathHelper->static float Pow(float x, float y)");
		public static unsafe float macross_Pow (string nodeName, float x, float y) 
		{
			using(var stackframe = EngineNS.Macross.TtMacrossStackTracer.CurrentFrame)
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":x", x);
					stackframe.SetWatchVariable(nodeName + ":y", y);
				}
			}
			var _return_value = Pow(x, y);
			macross_break_Pow_2776435935.TryBreak();
			return _return_value;
		}
		private static EngineNS.Macross.TtMacrossBreak macross_break_Random_3323264318 = new EngineNS.Macross.TtMacrossBreak("EngineNS.MathHelper->static int Random()");
		public static unsafe int macross_Random (string nodeName) 
		{
			using(var stackframe = EngineNS.Macross.TtMacrossStackTracer.CurrentFrame)
			{
				if(stackframe != null)
				{
				}
			}
			var _return_value = Random();
			macross_break_Random_3323264318.TryBreak();
			return _return_value;
		}
		private static EngineNS.Macross.TtMacrossBreak macross_break_RandomRange_3495035793 = new EngineNS.Macross.TtMacrossBreak("EngineNS.MathHelper->static int RandomRange(int start, int end)");
		public static unsafe int macross_RandomRange (string nodeName, int start, int end) 
		{
			using(var stackframe = EngineNS.Macross.TtMacrossStackTracer.CurrentFrame)
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":start", start);
					stackframe.SetWatchVariable(nodeName + ":end", end);
				}
			}
			var _return_value = RandomRange(start, end);
			macross_break_RandomRange_3495035793.TryBreak();
			return _return_value;
		}
		private static EngineNS.Macross.TtMacrossBreak macross_break_RandomFloat_3323264318 = new EngineNS.Macross.TtMacrossBreak("EngineNS.MathHelper->static float RandomFloat()");
		public static unsafe float macross_RandomFloat (string nodeName) 
		{
			using(var stackframe = EngineNS.Macross.TtMacrossStackTracer.CurrentFrame)
			{
				if(stackframe != null)
				{
				}
			}
			var _return_value = RandomFloat();
			macross_break_RandomFloat_3323264318.TryBreak();
			return _return_value;
		}
		private static EngineNS.Macross.TtMacrossBreak macross_break_RandomDirection_845820099 = new EngineNS.Macross.TtMacrossBreak("EngineNS.MathHelper->static Vector3 RandomDirection(bool bNormalize)");
		public static unsafe Vector3 macross_RandomDirection (string nodeName, bool bNormalize) 
		{
			using(var stackframe = EngineNS.Macross.TtMacrossStackTracer.CurrentFrame)
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":bNormalize", bNormalize);
				}
			}
			var _return_value = RandomDirection(bNormalize);
			macross_break_RandomDirection_845820099.TryBreak();
			return _return_value;
		}
		private static EngineNS.Macross.TtMacrossBreak macross_break_FClamp_3533399359 = new EngineNS.Macross.TtMacrossBreak("EngineNS.MathHelper->static float FClamp(float value, float min, float max)");
		public static unsafe float macross_FClamp (string nodeName, float value, float min, float max) 
		{
			using(var stackframe = EngineNS.Macross.TtMacrossStackTracer.CurrentFrame)
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":value", value);
					stackframe.SetWatchVariable(nodeName + ":min", min);
					stackframe.SetWatchVariable(nodeName + ":max", max);
				}
			}
			var _return_value = FClamp(value, min, max);
			macross_break_FClamp_3533399359.TryBreak();
			return _return_value;
		}
		private static EngineNS.Macross.TtMacrossBreak macross_break_FWrap_3533399359 = new EngineNS.Macross.TtMacrossBreak("EngineNS.MathHelper->static float FWrap(float value, float min, float max)");
		public static unsafe float macross_FWrap (string nodeName, float value, float min, float max) 
		{
			using(var stackframe = EngineNS.Macross.TtMacrossStackTracer.CurrentFrame)
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":value", value);
					stackframe.SetWatchVariable(nodeName + ":min", min);
					stackframe.SetWatchVariable(nodeName + ":max", max);
				}
			}
			var _return_value = FWrap(value, min, max);
			macross_break_FWrap_3533399359.TryBreak();
			return _return_value;
		}
		private static EngineNS.Macross.TtMacrossBreak macross_break_Lerp_232143363 = new EngineNS.Macross.TtMacrossBreak("EngineNS.MathHelper->static float Lerp(float from, float to, float t)");
		public static unsafe float macross_Lerp (string nodeName, float from, float to, float t) 
		{
			using(var stackframe = EngineNS.Macross.TtMacrossStackTracer.CurrentFrame)
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":from", from);
					stackframe.SetWatchVariable(nodeName + ":to", to);
					stackframe.SetWatchVariable(nodeName + ":t", t);
				}
			}
			var _return_value = Lerp(from, to, t);
			macross_break_Lerp_232143363.TryBreak();
			return _return_value;
		}
		private static EngineNS.Macross.TtMacrossBreak macross_break_CreateVector2f_2776435935 = new EngineNS.Macross.TtMacrossBreak("EngineNS.MathHelper->static Vector2 CreateVector2f(float x, float y)");
		public static unsafe Vector2 macross_CreateVector2f (string nodeName, float x, float y) 
		{
			using(var stackframe = EngineNS.Macross.TtMacrossStackTracer.CurrentFrame)
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":x", x);
					stackframe.SetWatchVariable(nodeName + ":y", y);
				}
			}
			var _return_value = CreateVector2f(x, y);
			macross_break_CreateVector2f_2776435935.TryBreak();
			return _return_value;
		}
		private static EngineNS.Macross.TtMacrossBreak macross_break_CreateVector3f_2005859565 = new EngineNS.Macross.TtMacrossBreak("EngineNS.MathHelper->static Vector3 CreateVector3f(float x, float y, float z)");
		public static unsafe Vector3 macross_CreateVector3f (string nodeName, float x, float y, float z) 
		{
			using(var stackframe = EngineNS.Macross.TtMacrossStackTracer.CurrentFrame)
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":x", x);
					stackframe.SetWatchVariable(nodeName + ":y", y);
					stackframe.SetWatchVariable(nodeName + ":z", z);
				}
			}
			var _return_value = CreateVector3f(x, y, z);
			macross_break_CreateVector3f_2005859565.TryBreak();
			return _return_value;
		}
		private static EngineNS.Macross.TtMacrossBreak macross_break_CreateVector4f_3044319830 = new EngineNS.Macross.TtMacrossBreak("EngineNS.MathHelper->static Vector4 CreateVector4f(float x, float y, float z, float w)");
		public static unsafe Vector4 macross_CreateVector4f (string nodeName, float x, float y, float z, float w) 
		{
			using(var stackframe = EngineNS.Macross.TtMacrossStackTracer.CurrentFrame)
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":x", x);
					stackframe.SetWatchVariable(nodeName + ":y", y);
					stackframe.SetWatchVariable(nodeName + ":z", z);
					stackframe.SetWatchVariable(nodeName + ":w", w);
				}
			}
			var _return_value = CreateVector4f(x, y, z, w);
			macross_break_CreateVector4f_3044319830.TryBreak();
			return _return_value;
		}
		private static EngineNS.Macross.TtMacrossBreak macross_break_CreateColor3f_1155057533 = new EngineNS.Macross.TtMacrossBreak("EngineNS.MathHelper->static Color3f CreateColor3f(float r, float g, float b)");
		public static unsafe Color3f macross_CreateColor3f (string nodeName, float r, float g, float b) 
		{
			using(var stackframe = EngineNS.Macross.TtMacrossStackTracer.CurrentFrame)
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":r", r);
					stackframe.SetWatchVariable(nodeName + ":g", g);
					stackframe.SetWatchVariable(nodeName + ":b", b);
				}
			}
			var _return_value = CreateColor3f(r, g, b);
			macross_break_CreateColor3f_1155057533.TryBreak();
			return _return_value;
		}
		private static EngineNS.Macross.TtMacrossBreak macross_break_CreateColor4f_2529872060 = new EngineNS.Macross.TtMacrossBreak("EngineNS.MathHelper->static Color4f CreateColor4f(float r, float g, float b, float a)");
		public static unsafe Color4f macross_CreateColor4f (string nodeName, float r, float g, float b, float a) 
		{
			using(var stackframe = EngineNS.Macross.TtMacrossStackTracer.CurrentFrame)
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":r", r);
					stackframe.SetWatchVariable(nodeName + ":g", g);
					stackframe.SetWatchVariable(nodeName + ":b", b);
					stackframe.SetWatchVariable(nodeName + ":a", a);
				}
			}
			var _return_value = CreateColor4f(r, g, b, a);
			macross_break_CreateColor4f_2529872060.TryBreak();
			return _return_value;
		}
		private static EngineNS.Macross.TtMacrossBreak macross_break_IsCircleIntersectLineSeg_3156159405 = new EngineNS.Macross.TtMacrossBreak("EngineNS.MathHelper->static bool IsCircleIntersectLineSeg(Vector2 circleCenter, float r, Vector2 point1, Vector2 point2)");
		public static unsafe bool macross_IsCircleIntersectLineSeg (string nodeName, Vector2 circleCenter, float r, Vector2 point1, Vector2 point2) 
		{
			using(var stackframe = EngineNS.Macross.TtMacrossStackTracer.CurrentFrame)
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":circleCenter", circleCenter);
					stackframe.SetWatchVariable(nodeName + ":r", r);
					stackframe.SetWatchVariable(nodeName + ":point1", point1);
					stackframe.SetWatchVariable(nodeName + ":point2", point2);
				}
			}
			var _return_value = IsCircleIntersectLineSeg(circleCenter, r, point1, point2);
			macross_break_IsCircleIntersectLineSeg_3156159405.TryBreak();
			return _return_value;
		}
		private static EngineNS.Macross.TtMacrossBreak macross_break_IsSphereIntersectLineSeg_1236205554 = new EngineNS.Macross.TtMacrossBreak("EngineNS.MathHelper->static bool IsSphereIntersectLineSeg(Vector3 start, Vector3 end, Vector3 sphereCenter, float sphereRadius)");
		public static unsafe bool macross_IsSphereIntersectLineSeg (string nodeName, Vector3 start, Vector3 end, Vector3 sphereCenter, float sphereRadius) 
		{
			using(var stackframe = EngineNS.Macross.TtMacrossStackTracer.CurrentFrame)
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":start", start);
					stackframe.SetWatchVariable(nodeName + ":end", end);
					stackframe.SetWatchVariable(nodeName + ":sphereCenter", sphereCenter);
					stackframe.SetWatchVariable(nodeName + ":sphereRadius", sphereRadius);
				}
			}
			var _return_value = IsSphereIntersectLineSeg(start, end, sphereCenter, sphereRadius);
			macross_break_IsSphereIntersectLineSeg_1236205554.TryBreak();
			return _return_value;
		}
		private static EngineNS.Macross.TtMacrossBreak macross_break_MortonCode2_2388649221 = new EngineNS.Macross.TtMacrossBreak("EngineNS.MathHelper->static UInt32 MortonCode2(UInt32 x)");
		public static unsafe UInt32 macross_MortonCode2 (string nodeName, UInt32 x) 
		{
			using(var stackframe = EngineNS.Macross.TtMacrossStackTracer.CurrentFrame)
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":x", x);
				}
			}
			var _return_value = MortonCode2(x);
			macross_break_MortonCode2_2388649221.TryBreak();
			return _return_value;
		}
		private static EngineNS.Macross.TtMacrossBreak macross_break_MortonCode2_64_2198230936 = new EngineNS.Macross.TtMacrossBreak("EngineNS.MathHelper->static UInt64 MortonCode2_64(UInt64 x)");
		public static unsafe UInt64 macross_MortonCode2_64 (string nodeName, UInt64 x) 
		{
			using(var stackframe = EngineNS.Macross.TtMacrossStackTracer.CurrentFrame)
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":x", x);
				}
			}
			var _return_value = MortonCode2_64(x);
			macross_break_MortonCode2_64_2198230936.TryBreak();
			return _return_value;
		}
		private static EngineNS.Macross.TtMacrossBreak macross_break_ReverseMortonCode2_2388649221 = new EngineNS.Macross.TtMacrossBreak("EngineNS.MathHelper->static UInt32 ReverseMortonCode2(UInt32 x)");
		public static unsafe UInt32 macross_ReverseMortonCode2 (string nodeName, UInt32 x) 
		{
			using(var stackframe = EngineNS.Macross.TtMacrossStackTracer.CurrentFrame)
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":x", x);
				}
			}
			var _return_value = ReverseMortonCode2(x);
			macross_break_ReverseMortonCode2_2388649221.TryBreak();
			return _return_value;
		}
		private static EngineNS.Macross.TtMacrossBreak macross_break_ReverseMortonCode2_64_2198230936 = new EngineNS.Macross.TtMacrossBreak("EngineNS.MathHelper->static UInt64 ReverseMortonCode2_64(UInt64 x)");
		public static unsafe UInt64 macross_ReverseMortonCode2_64 (string nodeName, UInt64 x) 
		{
			using(var stackframe = EngineNS.Macross.TtMacrossStackTracer.CurrentFrame)
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":x", x);
				}
			}
			var _return_value = ReverseMortonCode2_64(x);
			macross_break_ReverseMortonCode2_64_2198230936.TryBreak();
			return _return_value;
		}
		private static EngineNS.Macross.TtMacrossBreak macross_break_MortonCode3_2388649221 = new EngineNS.Macross.TtMacrossBreak("EngineNS.MathHelper->static UInt32 MortonCode3(UInt32 x)");
		public static unsafe UInt32 macross_MortonCode3 (string nodeName, UInt32 x) 
		{
			using(var stackframe = EngineNS.Macross.TtMacrossStackTracer.CurrentFrame)
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":x", x);
				}
			}
			var _return_value = MortonCode3(x);
			macross_break_MortonCode3_2388649221.TryBreak();
			return _return_value;
		}
		private static EngineNS.Macross.TtMacrossBreak macross_break_ReverseMortonCode3_2388649221 = new EngineNS.Macross.TtMacrossBreak("EngineNS.MathHelper->static UInt32 ReverseMortonCode3(UInt32 x)");
		public static unsafe UInt32 macross_ReverseMortonCode3 (string nodeName, UInt32 x) 
		{
			using(var stackframe = EngineNS.Macross.TtMacrossStackTracer.CurrentFrame)
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":x", x);
				}
			}
			var _return_value = ReverseMortonCode3(x);
			macross_break_ReverseMortonCode3_2388649221.TryBreak();
			return _return_value;
		}
		private static EngineNS.Macross.TtMacrossBreak macross_break_ILog2Const_3628127243 = new EngineNS.Macross.TtMacrossBreak("EngineNS.MathHelper->static UInt32 ILog2Const(UInt32 n)");
		public static unsafe UInt32 macross_ILog2Const (string nodeName, UInt32 n) 
		{
			using(var stackframe = EngineNS.Macross.TtMacrossStackTracer.CurrentFrame)
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":n", n);
				}
			}
			var _return_value = ILog2Const(n);
			macross_break_ILog2Const_3628127243.TryBreak();
			return _return_value;
		}
		private static EngineNS.Macross.TtMacrossBreak macross_break_DivideAndRoundUp_1445672579 = new EngineNS.Macross.TtMacrossBreak("EngineNS.MathHelper->static uint DivideAndRoundUp(uint Dividend, uint Divisor)");
		public static unsafe uint macross_DivideAndRoundUp (string nodeName, uint Dividend, uint Divisor) 
		{
			using(var stackframe = EngineNS.Macross.TtMacrossStackTracer.CurrentFrame)
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":Dividend", Dividend);
					stackframe.SetWatchVariable(nodeName + ":Divisor", Divisor);
				}
			}
			var _return_value = DivideAndRoundUp(Dividend, Divisor);
			macross_break_DivideAndRoundUp_1445672579.TryBreak();
			return _return_value;
		}
		private static EngineNS.Macross.TtMacrossBreak macross_break_SphericalEncode_2321709733 = new EngineNS.Macross.TtMacrossBreak("EngineNS.MathHelper->static Vector2 SphericalEncode(Vector3 v3)");
		public static unsafe Vector2 macross_SphericalEncode (string nodeName, Vector3 v3) 
		{
			using(var stackframe = EngineNS.Macross.TtMacrossStackTracer.CurrentFrame)
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":v3", v3);
				}
			}
			var _return_value = SphericalEncode(v3);
			macross_break_SphericalEncode_2321709733.TryBreak();
			return _return_value;
		}
		private static EngineNS.Macross.TtMacrossBreak macross_break_SphericalDecode_2954637647 = new EngineNS.Macross.TtMacrossBreak("EngineNS.MathHelper->static Vector3 SphericalDecode(Vector2 v)");
		public static unsafe Vector3 macross_SphericalDecode (string nodeName, Vector2 v) 
		{
			using(var stackframe = EngineNS.Macross.TtMacrossStackTracer.CurrentFrame)
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":v", v);
				}
			}
			var _return_value = SphericalDecode(v);
			macross_break_SphericalDecode_2954637647.TryBreak();
			return _return_value;
		}
		private static EngineNS.Macross.TtMacrossBreak macross_break_OctEncode_2321709733 = new EngineNS.Macross.TtMacrossBreak("EngineNS.MathHelper->static Vector2 OctEncode(Vector3 v3)");
		public static unsafe Vector2 macross_OctEncode (string nodeName, Vector3 v3) 
		{
			using(var stackframe = EngineNS.Macross.TtMacrossStackTracer.CurrentFrame)
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":v3", v3);
				}
			}
			var _return_value = OctEncode(v3);
			macross_break_OctEncode_2321709733.TryBreak();
			return _return_value;
		}
		private static EngineNS.Macross.TtMacrossBreak macross_break_OctDecode_2954637647 = new EngineNS.Macross.TtMacrossBreak("EngineNS.MathHelper->static Vector3 OctDecode(Vector2 v)");
		public static unsafe Vector3 macross_OctDecode (string nodeName, Vector2 v) 
		{
			using(var stackframe = EngineNS.Macross.TtMacrossStackTracer.CurrentFrame)
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":v", v);
				}
			}
			var _return_value = OctDecode(v);
			macross_break_OctDecode_2954637647.TryBreak();
			return _return_value;
		}
	}
}
#endregion//TitanEngine_AutoGen_Macross
#endif//TitanEngine_AutoGen_Macross