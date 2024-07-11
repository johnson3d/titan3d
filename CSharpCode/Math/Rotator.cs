using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS
{
    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
    public struct FRotator : System.IEquatable<FRotator>
    {
        public float Yaw;
        public float Pitch;
        public float Roll;
        public FRotator()
        {

        }
        public FRotator(float yaw, float pitch, float roll)
        {
            Yaw = yaw;
            Pitch = pitch;
            Roll = roll;
        }
        public FRotator(in Vector3 rh)
        {
            Yaw = rh.X;
            Pitch = rh.Y;
            Roll = rh.Z;
        }
        public float this[int index]
        {
            get
            {
                switch (index)
                {
                    case 0:
                        return Yaw;
                    case 1:
                        return Pitch;
                    case 2:
                        return Roll;
                    default:
                        throw new ArgumentOutOfRangeException("index", "Indices for FRotator run from 0 to 2, inclusive.");
                }
            }
            set
            {
                switch (index)
                {
                    case 0:
                        Yaw = value;
                        break;
                    case 1:
                        Pitch = value;
                        break;
                    case 2:
                        Roll = value;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException("index", "Indices for FRotator run from 0 to 2, inclusive.");
                }
            }
        }
        public override string ToString()
        {
            return $"{Yaw},{Pitch},{Roll}";
        }
        public static FRotator FromString(string text)
        {
            try
            {
                var result = new FRotator();
                ReadOnlySpan<char> chars = text.ToCharArray();
                int iStart = 0;
                int j = 0;
                for (int i = 0; i < chars.Length; i++)
                {
                    if (chars[i] == ',')
                    {
                        result[j] = float.Parse(chars.Slice(iStart, i - iStart));
                        iStart = i + 1;
                        j++;
                        if (j == 2)
                            break;
                    }
                }
                result[j] = float.Parse(chars.Slice(iStart, chars.Length - iStart));
                return result;
                //var segs = text.Split(',');
                //return new FRotator(System.Convert.ToSingle(segs[0]),
                //    System.Convert.ToSingle(segs[1]),
                //    System.Convert.ToSingle(segs[2]));
            }
            catch
            {
                return new FRotator(0,0,0);
            }
        }

        public static FRotator operator +(in FRotator left, in FRotator right)
        {
            FRotator result;
            result.Yaw = left.Yaw + right.Yaw;
            result.Pitch = left.Pitch + right.Pitch;
            result.Roll = left.Roll + right.Roll;
            return result;
        }
        public static FRotator operator +(in FRotator left, float right)
        {
            FRotator result;
            result.Yaw = left.Yaw + right;
            result.Pitch = left.Pitch + right;
            result.Roll = left.Roll + right;
            return result;
        }
        public static FRotator operator -(in FRotator left, in FRotator right)
        {
            FRotator result;
            result.Yaw = left.Yaw - right.Yaw;
            result.Pitch = left.Pitch - right.Pitch;
            result.Roll = left.Roll - right.Roll;
            return result;
        }
        public static FRotator operator -(in FRotator value)
        {
            FRotator result;
            result.Yaw = -value.Yaw;
            result.Pitch = -value.Pitch;
            result.Roll = -value.Roll;
            return result;
        }
        public static FRotator operator *(in FRotator value, float scale)
        {
            FRotator result;
            result.Yaw = value.Yaw * scale;
            result.Pitch = value.Pitch * scale;
            result.Roll = value.Roll * scale;
            return result;
        }
        public static FRotator operator *(in FRotator left, in FRotator right)
        {
            FRotator result;
            result.Yaw = left.Yaw * right.Yaw;
            result.Pitch = left.Pitch * right.Pitch;
            result.Roll = left.Roll * right.Roll;
            return result;
        }
        public static FRotator operator *(float scale, in FRotator vec)
        {
            return vec * scale;
        }
        public static FRotator operator /(in FRotator value, float scale)
        {
            FRotator result;
            result.Yaw = value.Yaw / scale;
            result.Pitch = value.Pitch / scale;
            result.Roll = value.Roll / scale;
            return result;
        }
        public static FRotator operator /(in FRotator left, in FRotator right)
        {
            FRotator result;
            result.Yaw = left.Yaw / right.Yaw;
            result.Pitch = left.Pitch / right.Pitch;
            result.Roll = left.Roll / right.Roll;
            return result;
        }
        public override bool Equals(object value)
        {
            if (value == null)
                return false;

            if (value.GetType() != GetType())
                return false;

            return Equals((FRotator)value);
        }
        public override int GetHashCode()
        {
            return Yaw.GetHashCode() + Pitch.GetHashCode() + Roll.GetHashCode();
        }
        public bool Equals(FRotator value)
        {
            bool reX = (Math.Abs(Yaw - value.Yaw) < MathHelper.Epsilon);
            bool reY = (Math.Abs(Pitch - value.Pitch) < MathHelper.Epsilon);
            bool reZ = (Math.Abs(Roll - value.Roll) < MathHelper.Epsilon);
            return (reX && reY && reZ);
        }
        public static bool operator ==(in FRotator left, in FRotator right)
        {
            return left.Equals(right);
            //return Equals(ref left, ref right );
        }
        public static bool operator !=(in FRotator left, in FRotator right)
        {
            return !left.Equals(right);
            //return !Equals(ref left, ref right );
        }
    }
}
