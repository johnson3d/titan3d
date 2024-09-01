using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using NPOI.SS.Formula.Functions;

namespace EngineNS
{
    public partial class CustomConvert
    {
        #region float
        [Rtti.Meta]
        public static string ConvertSingleToString(float val)
        {
            return val.ToString();
        }
        [Rtti.Meta]
        public static Byte ConvertSingleToByte(float val)
        {
            return System.Convert.ToByte(val);
        }
        [Rtti.Meta]
        public static UInt16 ConvertSingleToUInt16(float val)
        {
            return System.Convert.ToUInt16(val);
        }
        [Rtti.Meta]
        public static UInt32 ConvertSingleToUInt32(float val)
        {
            return System.Convert.ToUInt32(val);
        }
        [Rtti.Meta]
        public static UInt64 ConvertSingleToUInt64(float val)
        {
            return System.Convert.ToUInt64(val);
        }
        [Rtti.Meta]
        public static SByte ConvertSingleToSByte(float val)
        {
            return System.Convert.ToSByte(val);
        }
        [Rtti.Meta]
        public static Int16 ConvertSingleToInt16(float val)
        {
            return System.Convert.ToInt16(val);
        }
        [Rtti.Meta]
        public static Int32 ConvertSingleToInt32(float val)
        {
            return System.Convert.ToInt32(val);
        }
        [Rtti.Meta]
        public static Int64 ConvertSingleToInt64(float val)
        {
            return System.Convert.ToInt64(val);
        }
        [Rtti.Meta]
        public static double ConvertSingleToDouble(float val)
        {
            return System.Convert.ToDouble(val);
        }
        [Rtti.Meta]
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
        [Rtti.Meta]
        public static string ConvertByteToString(byte val)
        {
            return val.ToString();
        }
        [Rtti.Meta]
        public static UInt16 ConvertByteToUInt16(byte val)
        {
            return System.Convert.ToUInt16(val);
        }
        [Rtti.Meta]
        public static UInt32 ConvertByteToUInt32(byte val)
        {
            return System.Convert.ToUInt32(val);
        }
        [Rtti.Meta]
        public static UInt64 ConvertByteToUInt64(byte val)
        {
            return System.Convert.ToUInt64(val);
        }
        [Rtti.Meta]
        public static SByte ConvertByteToSByte(byte val)
        {
            return System.Convert.ToSByte(val);
        }
        [Rtti.Meta]
        public static Int16 ConvertByteToInt16(byte val)
        {
            return System.Convert.ToInt16(val);
        }
        [Rtti.Meta]
        public static Int32 ConvertByteToInt32(byte val)
        {
            return System.Convert.ToInt32(val);
        }
        [Rtti.Meta]
        public static Int64 ConvertByteToInt64(byte val)
        {
            return System.Convert.ToInt64(val);
        }
        [Rtti.Meta]
        public static float ConvertByteToSingle(byte val)
        {
            return System.Convert.ToSingle(val);
        }
        [Rtti.Meta]
        public static double ConvertByteToDouble(byte val)
        {
            return System.Convert.ToDouble(val);
        }
        [Rtti.Meta]
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
        [Rtti.Meta]
        public static string ConvertSByteToString(sbyte val)
        {
            return val.ToString();
        }
        [Rtti.Meta]
        public static Byte ConvertSByteToByte(sbyte val)
        {
            return System.Convert.ToByte(val);
        }
        [Rtti.Meta]
        public static UInt16 ConvertSByteToUInt16(sbyte val)
        {
            return System.Convert.ToUInt16(val);
        }
        [Rtti.Meta]
        public static UInt32 ConvertSByteToUInt32(sbyte val)
        {
            return System.Convert.ToUInt32(val);
        }
        [Rtti.Meta]
        public static UInt64 ConvertSByteToUInt64(sbyte val)
        {
            return System.Convert.ToUInt64(val);
        }
        [Rtti.Meta]
        public static Int16 ConvertSByteToInt16(sbyte val)
        {
            return System.Convert.ToInt16(val);
        }
        [Rtti.Meta]
        public static Int32 ConvertSByteToInt32(sbyte val)
        {
            return System.Convert.ToInt32(val);
        }
        [Rtti.Meta]
        public static Int64 ConvertSByteToInt64(sbyte val)
        {
            return System.Convert.ToInt64(val);
        }
        [Rtti.Meta]
        public static double ConvertSByteToDouble(sbyte val)
        {
            return System.Convert.ToDouble(val);
        }
        [Rtti.Meta]
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
        [Rtti.Meta]
        public static string ConvertUInt16ToString(UInt16 val)
        {
            return val.ToString();
        }
        [Rtti.Meta]
        public static Byte ConvertUInt16ToByte(UInt16 val)
        {
            return System.Convert.ToByte(val);
        }
        [Rtti.Meta]
        public static UInt32 ConvertUInt16ToUInt32(UInt16 val)
        {
            return System.Convert.ToUInt32(val);
        }
        [Rtti.Meta]
        public static UInt64 ConvertUInt16ToUInt64(UInt16 val)
        {
            return System.Convert.ToUInt64(val);
        }
        [Rtti.Meta]
        public static SByte ConvertUInt16ToSByte(UInt16 val)
        {
            return System.Convert.ToSByte(val);
        }
        [Rtti.Meta]
        public static Int16 ConvertUInt16ToInt16(UInt16 val)
        {
            return System.Convert.ToInt16(val);
        }
        [Rtti.Meta]
        public static Int32 ConvertUInt16ToInt32(UInt16 val)
        {
            return System.Convert.ToInt32(val);
        }
        [Rtti.Meta]
        public static Int64 ConvertUInt16ToInt64(UInt16 val)
        {
            return System.Convert.ToInt64(val);
        }
        [Rtti.Meta]
        public static float ConvertUInt16ToSingle(UInt16 val)
        {
            return System.Convert.ToSingle(val);
        }
        [Rtti.Meta]
        public static double ConvertUInt16ToDouble(UInt16 val)
        {
            return System.Convert.ToDouble(val);
        }
        [Rtti.Meta]
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
        [Rtti.Meta]
        public static string ConvertUInt32ToString(UInt32 val)
        {
            return val.ToString();
        }
        [Rtti.Meta]
        public static Byte ConvertUInt32ToByte(UInt32 val)
        {
            return System.Convert.ToByte(val);
        }
        [Rtti.Meta]
        public static UInt16 ConvertUInt32ToUInt16(UInt32 val)
        {
            return System.Convert.ToUInt16(val);
        }
        [Rtti.Meta]
        public static UInt32 ConvertUInt32ToUInt32(UInt32 val)
        {
            return System.Convert.ToUInt32(val);
        }
        [Rtti.Meta]
        public static UInt64 ConvertUInt32ToUInt64(UInt32 val)
        {
            return System.Convert.ToUInt64(val);
        }
        [Rtti.Meta]
        public static SByte ConvertUInt32ToSByte(UInt32 val)
        {
            return System.Convert.ToSByte(val);
        }
        [Rtti.Meta]
        public static Int16 ConvertUInt32ToInt16(UInt32 val)
        {
            return System.Convert.ToInt16(val);
        }
        [Rtti.Meta]
        public static Int32 ConvertUInt32ToInt32(UInt32 val)
        {
            return System.Convert.ToInt32(val);
        }
        [Rtti.Meta]
        public static Int64 ConvertUInt32ToInt64(UInt32 val)
        {
            return System.Convert.ToInt64(val);
        }
        [Rtti.Meta]
        public static float ConvertUInt32ToSingle(UInt32 val)
        {
            return System.Convert.ToSingle(val);
        }
        [Rtti.Meta]
        public static double ConvertUInt32ToDouble(UInt32 val)
        {
            return System.Convert.ToDouble(val);
        }
        [Rtti.Meta]
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
        [Rtti.Meta]
        public static string ConvertUInt64ToString(UInt64 val)
        {
            return val.ToString();
        }
        [Rtti.Meta]
        public static Byte ConvertUInt64ToByte(UInt64 val)
        {
            return System.Convert.ToByte(val);
        }
        [Rtti.Meta]
        public static UInt16 ConvertUInt64ToUInt16(UInt64 val)
        {
            return System.Convert.ToUInt16(val);
        }
        [Rtti.Meta]
        public static UInt32 ConvertUInt64ToUInt32(UInt64 val)
        {
            return System.Convert.ToUInt32(val);
        }
        [Rtti.Meta]
        public static SByte ConvertUInt64ToSByte(UInt64 val)
        {
            return System.Convert.ToSByte(val);
        }
        [Rtti.Meta]
        public static Int16 ConvertUInt64ToInt16(UInt64 val)
        {
            return System.Convert.ToInt16(val);
        }
        [Rtti.Meta]
        public static Int32 ConvertUInt64ToInt32(UInt64 val)
        {
            return System.Convert.ToInt32(val);
        }
        [Rtti.Meta]
        public static Int64 ConvertUInt64ToInt64(UInt64 val)
        {
            return System.Convert.ToInt64(val);
        }
        [Rtti.Meta]
        public static float ConvertUInt64ToSingle(UInt64 val)
        {
            return System.Convert.ToSingle(val);
        }
        [Rtti.Meta]
        public static double ConvertUInt64ToDouble(UInt64 val)
        {
            return System.Convert.ToDouble(val);
        }
        [Rtti.Meta]
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
        [Rtti.Meta]
        public static string ConvertInt16ToString(Int16 val)
        {
            return val.ToString();
        }
        [Rtti.Meta]
        public static Byte ConvertInt16ToByte(Int16 val)
        {
            return System.Convert.ToByte(val);
        }
        [Rtti.Meta]
        public static UInt16 ConvertInt16ToUInt16(Int16 val)
        {
            return System.Convert.ToUInt16(val);
        }
        [Rtti.Meta]
        public static UInt32 ConvertInt16ToUInt32(Int16 val)
        {
            return System.Convert.ToUInt32(val);
        }
        [Rtti.Meta]
        public static UInt64 ConvertInt16ToUInt64(Int16 val)
        {
            return System.Convert.ToUInt64(val);
        }
        [Rtti.Meta]
        public static SByte ConvertInt16ToSByte(Int16 val)
        {
            return System.Convert.ToSByte(val);
        }
        [Rtti.Meta]
        public static Int32 ConvertInt16ToInt32(Int16 val)
        {
            return System.Convert.ToInt32(val);
        }
        [Rtti.Meta]
        public static Int64 ConvertInt16ToInt64(Int16 val)
        {
            return System.Convert.ToInt64(val);
        }
        [Rtti.Meta]
        public static float ConvertInt16ToSingle(Int16 val)
        {
            return System.Convert.ToSingle(val);
        }
        [Rtti.Meta]
        public static double ConvertInt16ToDouble(Int16 val)
        {
            return System.Convert.ToDouble(val);
        }
        [Rtti.Meta]
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
        [Rtti.Meta]
        public static string ConvertInt32ToString(Int32 val)
        {
            return val.ToString();
        }
        [Rtti.Meta]
        public static Byte ConvertInt32ToByte(Int32 val)
        {
            return System.Convert.ToByte(val);
        }
        [Rtti.Meta]
        public static UInt16 ConvertInt32ToUInt16(Int32 val)
        {
            return System.Convert.ToUInt16(val);
        }
        [Rtti.Meta]
        public static UInt32 ConvertInt32ToUInt32(Int32 val)
        {
            return System.Convert.ToUInt32(val);
        }
        [Rtti.Meta]
        public static UInt64 ConvertInt32ToUInt64(Int32 val)
        {
            return System.Convert.ToUInt64(val);
        }
        [Rtti.Meta]
        public static SByte ConvertInt32ToSByte(Int32 val)
        {
            return System.Convert.ToSByte(val);
        }
        [Rtti.Meta]
        public static Int16 ConvertInt32ToInt16(Int32 val)
        {
            return System.Convert.ToInt16(val);
        }
        [Rtti.Meta]
        public static Int64 ConvertInt32ToInt64(Int32 val)
        {
            return System.Convert.ToInt64(val);
        }
        [Rtti.Meta]
        public static float ConvertInt32ToSingle(Int32 val)
        {
            return System.Convert.ToSingle(val);
        }
        [Rtti.Meta]
        public static double ConvertInt32ToDouble(Int32 val)
        {
            return System.Convert.ToDouble(val);
        }
        [Rtti.Meta]
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
        [Rtti.Meta]
        public static string ConvertInt64ToString(Int64 val)
        {
            return val.ToString();
        }
        [Rtti.Meta]
        public static Byte ConvertInt64ToByte(Int64 val)
        {
            return System.Convert.ToByte(val);
        }
        [Rtti.Meta]
        public static UInt16 ConvertInt64ToUInt16(Int64 val)
        {
            return System.Convert.ToUInt16(val);
        }
        [Rtti.Meta]
        public static UInt32 ConvertInt64ToUInt32(Int64 val)
        {
            return System.Convert.ToUInt32(val);
        }
        [Rtti.Meta]
        public static UInt64 ConvertInt64ToUInt64(Int64 val)
        {
            return System.Convert.ToUInt64(val);
        }
        [Rtti.Meta]
        public static SByte ConvertInt64ToSByte(Int64 val)
        {
            return System.Convert.ToSByte(val);
        }
        [Rtti.Meta]
        public static Int16 ConvertInt64ToInt16(Int64 val)
        {
            return System.Convert.ToInt16(val);
        }
        [Rtti.Meta]
        public static Int32 ConvertInt64ToInt32(Int64 val)
        {
            return System.Convert.ToInt32(val);
        }
        [Rtti.Meta]
        public static float ConvertInt64ToSingle(Int64 val)
        {
            return System.Convert.ToSingle(val);
        }
        [Rtti.Meta]
        public static double ConvertInt64ToDouble(Int64 val)
        {
            return System.Convert.ToDouble(val);
        }
        [Rtti.Meta]
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
        [Rtti.Meta]
        public static string ConvertDoubleToString(double val)
        {
            return val.ToString();
        }
        [Rtti.Meta]
        public static Byte ConvertDoubleToByte(double val)
        {
            return System.Convert.ToByte(val);
        }
        [Rtti.Meta]
        public static UInt16 ConvertDoubleToUInt16(double val)
        {
            return System.Convert.ToUInt16(val);
        }
        [Rtti.Meta]
        public static UInt32 ConvertDoubleToUInt32(double val)
        {
            return System.Convert.ToUInt32(val);
        }
        [Rtti.Meta]
        public static UInt64 ConvertDoubleToUInt64(double val)
        {
            return System.Convert.ToUInt64(val);
        }
        [Rtti.Meta]
        public static SByte ConvertDoubleToSByte(double val)
        {
            return System.Convert.ToSByte(val);
        }
        [Rtti.Meta]
        public static Int16 ConvertDoubleToInt16(double val)
        {
            return System.Convert.ToInt16(val);
        }
        [Rtti.Meta]
        public static Int32 ConvertDoubleToInt32(double val)
        {
            return System.Convert.ToInt32(val);
        }
        [Rtti.Meta]
        public static Int64 ConvertDoubleToInt64(double val)
        {
            return System.Convert.ToInt64(val);
        }
        [Rtti.Meta]
        public static float ConvertDoubleToSingle(double val)
        {
            return System.Convert.ToSingle(val);
        }
        [Rtti.Meta]
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
    [Rtti.Meta]
    public partial class MathHelper
    {
        public const float Epsilon = ((float)0.00001);
        public const float DEpsilon = 0.0000001f;
        public const float TWO_PI = V_PI * 2.0f;
        public const float V_PI = ((float)3.1415926535);
        [Rtti.Meta]
        public static float PI
        {
            get => V_PI;
        }

        [Rtti.Meta]
        public static float Invc2PI
        {
            get => (1.0f / TWO_PI);
        }
        // Degrees-to-radians conversion constant (RO).
        public const float V_Deg2Rad = V_PI / 180.0f;
        [Rtti.Meta]
        public static float Deg2Rad
        {
            get => V_Deg2Rad;
        }
        // Radians-to-degrees conversion constant (RO).
        public const float V_Rad2Deg = 180.0f / V_PI;
        [Rtti.Meta]
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
        [Rtti.Meta]
        public static float Abs(float v)
        {
            return (float)System.Math.Abs(v);
        }
        [Rtti.Meta]
        public static float Mod(float v1, float v2)
        {
            return (float)v1 % v2;
        }
        [Rtti.Meta]
        public static float Sin(float v)
        {
            return (float)System.Math.Sin(v);
        }
        [Rtti.Meta]
        public static float Asin(float v)
        {
            return (float)System.Math.Asin(v);
        }
        [Rtti.Meta]
        public static float Cos(float v)
        {
            return (float)System.Math.Cos(v);
        }
        [Rtti.Meta]
        public static float Acos(float v)
        {
            v = Clamp<float>(v,-1,1);
            return (float)System.Math.Acos(v);
        }
        [Rtti.Meta]
        public static float Tan(float v)
        {
            return (float)System.Math.Tan(v);
        }
        [Rtti.Meta]
        public static float Atan(float v)
        {
            return (float)System.Math.Atan(v);
        }
        [Rtti.Meta]
        public static float Atan2(float y, float x)
        {
            return (float)System.Math.Atan2(y, x);
        }
        [Rtti.Meta]
        public static float Pow(float x, float y)
        {
            return (float)System.Math.Pow(x, y);
        }

        private static System.Random sRandom = new Random((int)Support.TtTime.GetTickCount());
        [Rtti.Meta]
        public static int Random()
        {
            return sRandom.Next();
        }
        [Rtti.Meta]
        public static int RandomRange(int start, int end)
        {
            return sRandom.Next(start, end);
        }
        public static float RandomRange(float start, float end)
        {
            return RandomDouble() * (end - start);
        }
        [Rtti.Meta]
        public static float RandomDouble()
        {
            return (float)sRandom.NextDouble();
        }
        [Rtti.Meta]
        public static Vector3 RandomDirection(bool bNormalize = true)
        {
            Vector3 result;
            result.X = RandomDouble();
            result.Y = RandomDouble();
            result.Z = RandomDouble();
            if (bNormalize)
                result.Normalize();
            return result;
        }
        [Rtti.Meta]
        public static float FClamp(float value, float min, float max)
        {
            if (value > max)
                return max;
            if (value < min)
                return min;
            return value;
        }
        [Rtti.Meta]
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
        [Rtti.Meta]
        public static float Lerp(float from, float to, float t)
        {
            return to * t + from * (1.0f - t);
        }
        public static int Lerp(int from, int to, float t)
        {
            return (int)((float)to * t + (float)from * (1.0f - t));
        }
        #region CreateInstance
        [Rtti.Meta(ShaderName = "CreateVector2f")]
        public static Vector2 CreateVector2f(float x, float y)
        {
            return new Vector2(x, y);
        }
        [Rtti.Meta(ShaderName = "CreateVector3f")]
        public static Vector3 CreateVector3f(float x, float y, float z)
        {
            return new Vector3(x, y, z);
        }
        [Rtti.Meta(ShaderName = "CreateVector4f")]
        public static Vector4 CreateVector4f(float x, float y, float z, float w)
        {
            return new Vector4(x, y, z, w);
        }
        [Rtti.Meta(ShaderName = "CreateColor3f")]
        public static Color3f CreateColor3f(float r, float g, float b)
        {
            return new Color3f(r, g, b);
        }
        [Rtti.Meta(ShaderName = "CreateColor4f")]
        public static Color4f CreateColor4f(float r, float g, float b, float a)
        {
            return new Color4f(a, r, g, b);
        }
        #endregion
        // 圆与线段碰撞检测
        // 圆心p(x, y), 半径r, 线段两端点p1(x1, y1)和p2(x2, y2)
        [Rtti.Meta]
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

        [Rtti.Meta]
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
        [Rtti.Meta]
        public static UInt32 MortonCode2(UInt32 x)
        {
            x &= 0x0000ffff;
            x = (x ^ (x << 8)) & 0x00ff00ff;
            x = (x ^ (x << 4)) & 0x0f0f0f0f;
            x = (x ^ (x << 2)) & 0x33333333;
            x = (x ^ (x << 1)) & 0x55555555;
            return x;
        }
        [Rtti.Meta]
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
        [Rtti.Meta]
        public static UInt32 ReverseMortonCode2(UInt32 x)
        {
            x &= 0x55555555;
            x = (x ^ (x >> 1)) & 0x33333333;
            x = (x ^ (x >> 2)) & 0x0f0f0f0f;
            x = (x ^ (x >> 4)) & 0x00ff00ff;
            x = (x ^ (x >> 8)) & 0x0000ffff;
            return x;
        }
        [Rtti.Meta]
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
        [Rtti.Meta]
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
        [Rtti.Meta]
        public static UInt32 ReverseMortonCode3(UInt32 x)
        {
            x &= 0x09249249;
            x = (x ^ (x >> 2)) & 0x030c30c3;
            x = (x ^ (x >> 4)) & 0x0300f00f;
            x = (x ^ (x >> 8)) & 0xff0000ff;
            x = (x ^ (x >> 16)) & 0x000003ff;
            return x;
        }

        [Rtti.Meta]
        public static UInt32 ILog2Const(UInt32 n)
        {
            return (n > 1) ? 1 + ILog2Const(n / 2) : 0;
        }

        [Rtti.Meta]
        public static uint DivideAndRoundUp(uint Dividend, uint Divisor)
        {
            return (Dividend + Divisor - 1) / Divisor;
        }

        // Encode for normal map.
        [Rtti.Meta]
        public static Vector2 SphericalEncode(Vector3 v3)
        {
            Vector2 v = new Vector2();
            v.X = Atan2(v3.Y, v3.X) * Invc2PI;
            v.Y = v3.Z;

            v.X = v.X * 0.5f + 0.5f;
            v.Y = v.Y * 0.5f + 0.5f;
            return v;
        }

        [Rtti.Meta]
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

        [Rtti.Meta]
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

        [Rtti.Meta]
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
