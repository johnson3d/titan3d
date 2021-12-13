using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS
{
    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential, Pack = 4)]
    public struct DVector4 : System.IEquatable<DVector4>
    {
        public double X;
        public double Y;
        public double Z;
        public double W;
        public DVector4(DVector4 value)
        {
            X = value.X;
            Y = value.Y;
            Z = value.Z;
            W = value.W;
        }
        public DVector4(double value)
        {
            X = value;
            Y = value;
            Z = value;
            W = value;
        }
        public DVector4(DVector3 value, double w)
        {
            X = value.X;
            Y = value.Y;
            Z = value.Z;
            W = w;
        }
        /// <summary>
        /// 带参构造函数
        /// </summary>
        /// <param name="x">X值</param>
        /// <param name="y">Y值</param>
        /// <param name="z">Z值</param>
        /// <param name="w">W值</param>
        public DVector4(double x, double y, double z, double w)
        {
            X = x;
            Y = y;
            Z = z;
            W = w;
        }
        public DVector3 AsVector3()
        {
            return new DVector3(X, Y, Z);
        }
        public override string ToString()
        {
            return $"{X},{Y},{Z},{W}";
        }
        public override int GetHashCode()
        {
            return X.GetHashCode() + Y.GetHashCode() + Z.GetHashCode() + W.GetHashCode();
        }
        public double Length()
        {
            return (Math.Sqrt((X * X) + (Y * Y) + (Z * Z) + (W * W)));
        }
        public double LengthSquared()
        {
            return (X * X) + (Y * Y) + (Z * Z) + (W * W);
        }
        public void Normalize()
        {
            var length = Length();
            if (length == 0)
                return;
            var num = 1 / length;
            X *= num;
            Y *= num;
            Z *= num;
            W *= num;
        }
        public static DVector4 Add(in DVector4 left, in DVector4 right)
        {
            DVector4 result;
            result.X = left.X + right.X;
            result.Y = left.Y + right.Y;
            result.Z = left.Z + right.Z;
            result.W = left.W + right.W;
            return result;
        }
        public static void Add(in DVector4 left, in DVector4 right, out DVector4 result)
        {
            result.X = left.X + right.X;
            result.Y = left.Y + right.Y;
            result.Z = left.Z + right.Z;
            result.W = left.W + right.W;
        }
        public static DVector4 Subtract(in DVector4 left, in DVector4 right)
        {
            DVector4 result;
            result.X = left.X - right.X;
            result.Y = left.Y - right.Y;
            result.Z = left.Z - right.Z;
            result.W = left.W - right.W;
            return result;
        }
        public static void Subtract(in DVector4 left, in DVector4 right, out DVector4 result)
        {
            result.X = left.X - right.X;
            result.Y = left.Y - right.Y;
            result.Z = left.Z - right.Z;
            result.W = left.W - right.W;
        }
        public static DVector4 Modulate(in DVector4 left, in DVector4 right)
        {
            DVector4 result;
            result.X = left.X * right.X;
            result.Y = left.Y * right.Y;
            result.Z = left.Z * right.Z;
            result.W = left.W * right.W;
            return result;
        }
        public static void Modulate(in DVector4 left, in DVector4 right, out DVector4 result)
        {
            result.X = left.X * right.X;
            result.Y = left.Y * right.Y;
            result.Z = left.Z * right.Z;
            result.W = left.W * right.W;
        }
        public static DVector4 Multiply(in DVector4 value, double scale)
        {
            DVector4 result;
            result.X = value.X * scale;
            result.Y = value.Y * scale;
            result.Z = value.Z * scale;
            result.W = value.W * scale;
            return result;
        }
        public static void Multiply(in DVector4 value, double scale, out DVector4 result)
        {
            result.X = value.X * scale;
            result.Y = value.Y * scale;
            result.Z = value.Z * scale;
            result.W = value.W * scale;
        }
        public static DVector4 Divide(in DVector4 value, double scale)
        {
            DVector4 result;
            result.X = value.X / scale;
            result.Y = value.Y / scale;
            result.Z = value.Z / scale;
            result.W = value.W / scale;
            return result;
        }
        public static void Divide(in DVector4 value, double scale, out DVector4 result)
        {
            result.X = value.X / scale;
            result.Y = value.Y / scale;
            result.Z = value.Z / scale;
            result.W = value.W / scale;
        }
        public static DVector4 Negate(in DVector4 value)
        {
            DVector4 result;
            result.X = -value.X;
            result.Y = -value.Y;
            result.Z = -value.Z;
            result.W = -value.W;
            return result;
        }
        public static void Negate(in DVector4 value, out DVector4 result)
        {
            result.X = -value.X;
            result.Y = -value.Y;
            result.Z = -value.Z;
            result.W = -value.W;
        }
        public static DVector4 Barycentric(in DVector4 value1, in DVector4 value2, in DVector4 value3, double amount1, double amount2)
        {
            DVector4 vector;
            vector.X = (value1.X + (amount1 * (value2.X - value1.X))) + (amount2 * (value3.X - value1.X));
            vector.Y = (value1.Y + (amount1 * (value2.Y - value1.Y))) + (amount2 * (value3.Y - value1.Y));
            vector.Z = (value1.Z + (amount1 * (value2.Z - value1.Z))) + (amount2 * (value3.Z - value1.Z));
            vector.W = (value1.W + (amount1 * (value2.W - value1.W))) + (amount2 * (value3.W - value1.W));
            return vector;
        }
        public static void Barycentric(in DVector4 value1, in DVector4 value2, in DVector4 value3, double amount1, double amount2, out DVector4 result)
        {
            result.X = (value1.X + (amount1 * (value2.X - value1.X))) + (amount2 * (value3.X - value1.X));
            result.Y = (value1.Y + (amount1 * (value2.Y - value1.Y))) + (amount2 * (value3.Y - value1.Y));
            result.Z = (value1.Z + (amount1 * (value2.Z - value1.Z))) + (amount2 * (value3.Z - value1.Z));
            result.W = (value1.W + (amount1 * (value2.W - value1.W))) + (amount2 * (value3.W - value1.W));
        }
        public static DVector4 Clamp(in DVector4 value, in DVector4 min, in DVector4 max)
        {
            var x = value.X;
            x = (x > max.X) ? max.X : x;
            x = (x < min.X) ? min.X : x;

            var y = value.Y;
            y = (y > max.Y) ? max.Y : y;
            y = (y < min.Y) ? min.Y : y;

            var z = value.Z;
            z = (z > max.Z) ? max.Z : z;
            z = (z < min.Z) ? min.Z : z;

            var w = value.W;
            w = (w > max.W) ? max.W : w;
            w = (w < min.W) ? min.W : w;

            DVector4 result;
            result.X = x;
            result.Y = y;
            result.Z = z;
            result.W = w;
            return result;
        }
        public static void Clamp(in DVector4 value, in DVector4 min, in DVector4 max, out DVector4 result)
        {
            var x = value.X;
            x = (x > max.X) ? max.X : x;
            x = (x < min.X) ? min.X : x;

            var y = value.Y;
            y = (y > max.Y) ? max.Y : y;
            y = (y < min.Y) ? min.Y : y;

            var z = value.Z;
            z = (z > max.Z) ? max.Z : z;
            z = (z < min.Z) ? min.Z : z;

            var w = value.W;
            w = (w > max.W) ? max.W : w;
            w = (w < min.W) ? min.W : w;

            result.X = x;
            result.Y = y;
            result.Z = z;
            result.W = w;
        }
        public static DVector4 Lerp(in DVector4 start, in DVector4 end, double factor)
        {
            DVector4 vector;

            vector.X = start.X + ((end.X - start.X) * factor);
            vector.Y = start.Y + ((end.Y - start.Y) * factor);
            vector.Z = start.Z + ((end.Z - start.Z) * factor);
            vector.W = start.W + ((end.W - start.W) * factor);

            return vector;
        }
        public static void Lerp(in DVector4 start, in DVector4 end, double factor, out DVector4 result)
        {
            result.X = start.X + ((end.X - start.X) * factor);
            result.Y = start.Y + ((end.Y - start.Y) * factor);
            result.Z = start.Z + ((end.Z - start.Z) * factor);
            result.W = start.W + ((end.W - start.W) * factor);
        }
        public static DVector4 SmoothStep(in DVector4 start, in DVector4 end, double amount)
        {
            DVector4 vector;

            amount = (amount > 1.0f) ? 1.0f : ((amount < 0.0f) ? 0.0f : amount);
            amount = (amount * amount) * (3.0f - (2.0f * amount));

            vector.X = start.X + ((end.X - start.X) * amount);
            vector.Y = start.Y + ((end.Y - start.Y) * amount);
            vector.Z = start.Z + ((end.Z - start.Z) * amount);
            vector.W = start.W + ((end.W - start.W) * amount);

            return vector;
        }
        public static double Distance(in DVector4 value1, in DVector4 value2)
        {
            var x = value1.X - value2.X;
            var y = value1.Y - value2.Y;
            var z = value1.Z - value2.Z;
            var w = value1.W - value2.W;

            return (Math.Sqrt((x * x) + (y * y) + (z * z) + (w * w)));
        }
        public static double DistanceSquared(in DVector4 value1, in DVector4 value2)
        {
            var x = value1.X - value2.X;
            var y = value1.Y - value2.Y;
            var z = value1.Z - value2.Z;
            var w = value1.W - value2.W;

            return (x * x) + (y * y) + (z * z) + (w * w);
        }
        public static double Dot(in DVector4 left, in DVector4 right)
        {
            return (left.X * right.X + left.Y * right.Y + left.Z * right.Z + left.W * right.W);
        }
        public static DVector4 Normalize(in DVector4 vector)
        {
            vector.Normalize();
            return vector;
        }
        public static void Normalize(in DVector4 vector, out DVector4 result)
        {
            result = vector;
            result.Normalize();
        }
        public static DVector4 Transform(in DVector4 vector, in Matrix transform)
        {
            DVector4 result;

            result.X = (vector.X * transform.M11) + (vector.Y * transform.M21) + (vector.Z * transform.M31) + (vector.W * transform.M41);
            result.Y = (vector.X * transform.M12) + (vector.Y * transform.M22) + (vector.Z * transform.M32) + (vector.W * transform.M42);
            result.Z = (vector.X * transform.M13) + (vector.Y * transform.M23) + (vector.Z * transform.M33) + (vector.W * transform.M43);
            result.W = (vector.X * transform.M14) + (vector.Y * transform.M24) + (vector.Z * transform.M34) + (vector.W * transform.M44);

            return result;
        }
        public static void Transform(in DVector4 vector, in Matrix transform, out DVector4 result)
        {
            DVector4 r;
            r.X = (vector.X * transform.M11) + (vector.Y * transform.M21) + (vector.Z * transform.M31) + (vector.W * transform.M41);
            r.Y = (vector.X * transform.M12) + (vector.Y * transform.M22) + (vector.Z * transform.M32) + (vector.W * transform.M42);
            r.Z = (vector.X * transform.M13) + (vector.Y * transform.M23) + (vector.Z * transform.M33) + (vector.W * transform.M43);
            r.W = (vector.X * transform.M14) + (vector.Y * transform.M24) + (vector.Z * transform.M34) + (vector.W * transform.M44);

            result = r;
        }
        public static DVector4 Transform(in DVector4 value, in Quaternion rotation)
        {
            DVector4 vector;
            double x = rotation.X + rotation.X;
            double y = rotation.Y + rotation.Y;
            double z = rotation.Z + rotation.Z;
            double wx = rotation.W * x;
            double wy = rotation.W * y;
            double wz = rotation.W * z;
            double xx = rotation.X * x;
            double xy = rotation.X * y;
            double xz = rotation.X * z;
            double yy = rotation.Y * y;
            double yz = rotation.Y * z;
            double zz = rotation.Z * z;

            vector.X = ((value.X * ((1.0f - yy) - zz)) + (value.Y * (xy - wz))) + (value.Z * (xz + wy));
            vector.Y = ((value.X * (xy + wz)) + (value.Y * ((1.0f - xx) - zz))) + (value.Z * (yz - wx));
            vector.Z = ((value.X * (xz - wy)) + (value.Y * (yz + wx))) + (value.Z * ((1.0f - xx) - yy));
            vector.W = value.W;

            return vector;
        }
        public static void Transform(in DVector4 value, in Quaternion rotation, out DVector4 result)
        {
            double x = rotation.X + rotation.X;
            double y = rotation.Y + rotation.Y;
            double z = rotation.Z + rotation.Z;
            double wx = rotation.W * x;
            double wy = rotation.W * y;
            double wz = rotation.W * z;
            double xx = rotation.X * x;
            double xy = rotation.X * y;
            double xz = rotation.X * z;
            double yy = rotation.Y * y;
            double yz = rotation.Y * z;
            double zz = rotation.Z * z;

            DVector4 r;
            r.X = ((value.X * ((1.0f - yy) - zz)) + (value.Y * (xy - wz))) + (value.Z * (xz + wy));
            r.Y = ((value.X * (xy + wz)) + (value.Y * ((1.0f - xx) - zz))) + (value.Z * (yz - wx));
            r.Z = ((value.X * (xz - wy)) + (value.Y * (yz + wx))) + (value.Z * ((1.0f - xx) - yy));
            r.W = value.W;

            result = r;
        }
        public static DVector4 Minimize(in DVector4 left, in DVector4 right)
        {
            DVector4 vector;
            vector.X = (left.X < right.X) ? left.X : right.X;
            vector.Y = (left.Y < right.Y) ? left.Y : right.Y;
            vector.Z = (left.Z < right.Z) ? left.Z : right.Z;
            vector.W = (left.W < right.W) ? left.W : right.W;
            return vector;
        }
        public static DVector4 Maximize(in DVector4 left, in DVector4 right)
        {
            DVector4 vector;
            vector.X = (left.X > right.X) ? left.X : right.X;
            vector.Y = (left.Y > right.Y) ? left.Y : right.Y;
            vector.Z = (left.Z > right.Z) ? left.Z : right.Z;
            vector.W = (left.W > right.W) ? left.W : right.W;
            return vector;
        }
        public static DVector4 operator +(in DVector4 left, in DVector4 right)
        {
            DVector4 result;
            result.X = left.X + right.X;
            result.Y = left.Y + right.Y;
            result.Z = left.Z + right.Z;
            result.W = left.W + right.W;
            return result;
        }
        public static DVector4 operator -(in DVector4 left, in DVector4 right)
        {
            DVector4 result;
            result.X = left.X - right.X;
            result.Y = left.Y - right.Y;
            result.Z = left.Z - right.Z;
            result.W = left.W - right.W;
            return result;
        }
        public static DVector4 operator -(in DVector4 value)
        {
            DVector4 result;
            result.X = -value.X;
            result.Y = -value.Y;
            result.Z = -value.Z;
            result.W = -value.W;
            return result;
        }
        public static DVector4 operator *(in DVector4 value, double scale)
        {
            DVector4 result;
            result.X = value.X * scale;
            result.Y = value.Y * scale;
            result.Z = value.Z * scale;
            result.W = value.W * scale;
            return result;
        }
        public static DVector4 operator *(double scale, DVector4 vec)
        {
            return vec * scale;
        }
        public static DVector4 operator /(DVector4 value, double scale)
        {
            DVector4 result;
            result.X = value.X / scale;
            result.Y = value.Y / scale;
            result.Z = value.Z / scale;
            result.W = value.W / scale;
            return result;
        }
        public static bool operator ==(in DVector4 left, in DVector4 right)
        {
            return Equals(left, right);
        }
        public static bool operator !=(in DVector4 left, in DVector4 right)
        {
            return !Equals(left, right);
        }
        public static bool Equals(in DVector4 value1, in DVector4 value2)
        {
            return (value1.X == value2.X && value1.Y == value2.Y && value1.Z == value2.Z && value1.W == value2.W);
        }
        public override bool Equals(object value)
        {
            if (value == null)
                return false;

            if (value.GetType() != GetType())
                return false;

            return Equals((DVector4)(value));
        }
        public bool Equals(DVector4 value)
        {
            return (X == value.X && Y == value.Y && Z == value.Z && W == value.W);
        }
    }
}
