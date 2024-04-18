using System;
using System.Globalization;

namespace EngineNS
{
    /// <summary>
    /// 颜色结构体
    /// </summary>
    [System.Serializable]
	[System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
    //[System.ComponentModel.TypeConverter( typeof(EngineNS.Design.Color3Converter))]
	public struct Color3f : System.IEquatable<Color3f>
    {
        public float Red;
        public float Green;
        public float Blue;
        public Color3f(in Vector3 v)
        {
            Red = v.X;
            Green = v.Y;
            Blue = v.Z;
        }
        public static implicit operator Color3f(Vector3 d) => new Color3f(d.X, d.Y, d.Z);
        public static Color3f FromObject(object obj)
        {
            if (obj.GetType() == typeof(Color3f))
                return (Color3f)obj;
            else if (obj.GetType() == typeof(Vector3))
                return (Vector3)obj;
            else
            {
                System.Diagnostics.Debug.Assert(false);
                return new Color3f();
            }
        }
        public float this[int index]
        {
            get
            {
                switch (index)
                {
                    case 0:
                        return Red;
                    case 1:
                        return Green;
                    case 2:
                        return Blue;
                    default:
                        throw new ArgumentOutOfRangeException("index", "Indices for Vector3 run from 0 to 2, inclusive.");
                }
            }
            set
            {
                switch (index)
                {
                    case 0:
                        Red = value;
                        break;
                    case 1:
                        Green = value;
                        break;
                    case 2:
                        Blue = value;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException("index", "Indices for Vector3 run from 0 to 2, inclusive.");
                }
            }
        }
        public static Color3f FromString(string text)
        {
            try
            {
                var result = new Color3f();
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
            }
            catch
            {
                return new Color3f(0,0,0);
            }
        }
        public override string ToString()
        {
            return $"{Red},{Green},{Blue}";
            //return string.Format(CultureInfo.CurrentCulture, $"R:{Red.ToString(CultureInfo.CurrentCulture)} G:{Green.ToString(CultureInfo.CurrentCulture)} B:{Blue.ToString(CultureInfo.CurrentCulture)}");
        }
        /// <summary>
        /// 带参构造函数
        /// </summary>
        /// <param name="red">红色值</param>
        /// <param name="green">绿色值</param>
        /// <param name="blue">蓝色值</param>
        public Color3f( float red, float green, float blue )
	    {
            Red = red;
            Green = green;
            Blue = blue;
	    }
        public Color3f(double red, double green, double blue)
        {
            Red = (float)red;
            Green = (float)green;
            Blue = (float)blue;
        }
        public Vector3 ToVector3()
        {
            return new Vector3(Red, Green, Blue);
        }
        public static Color3f FromColor(Color color)
        {
            Color3f retValue = new Color3f();
            retValue.Red = ((float)color.R) / 255.0f;
            retValue.Green = ((float)color.G) / 255.0f;
            retValue.Blue = ((float)color.B) / 255.0f;
            return retValue;
        }
        /// <summary>
        /// 重载操作符==
        /// </summary>
        /// <param name="left">颜色值</param>
        /// <param name="right">颜色值</param>
        /// <returns>如果两个颜色相同返回true，否则返回false</returns>
	    public static bool operator == ( Color3f left, Color3f right )
	    {
            return left.Equals(right);
		    //return Color3f.Equals( left, right );
	    }
        /// <summary>
        /// 重载操作符!=
        /// </summary>
        /// <param name="left">颜色值</param>
        /// <param name="right">颜色值</param>
        /// <returns>如果两个颜色不相同返回true，否则返回false</returns>
        public static bool operator !=(Color3f left, Color3f right)
	    {
            return !left.Equals(right);
		    //return !Equals( left, right );
	    }
        /// <summary>
        /// 获取哈希值
        /// </summary>
        /// <returns>返回对象的哈希值</returns>
	    public override int GetHashCode()
	    {
		    return Red.GetHashCode() + Green.GetHashCode() + Blue.GetHashCode();
	    }
        /// <summary>
        /// 判断两个对象是否相等
        /// </summary>
        /// <param name="value">需要判断的对象</param>
        /// <returns>如果相同返回true，否则返回false</returns>
	    public override bool Equals( object value )
	    {
		    if( value == null )
			    return false;

		    if( value.GetType() != GetType() )
			    return false;

		    return Equals( (Color3f)( value ) );
	    }
        /// <summary>
        /// 判断两个颜色是否相同
        /// </summary>
        /// <param name="value">颜色值</param>
        /// <returns>如果相同返回true，否则返回false</returns>
	    public bool Equals( Color3f value )
	    {
		    return ( Red == value.Red && Green == value.Green && Blue == value.Blue );
	    }
        /// <summary>
        /// 判断两个颜色是否相同
        /// </summary>
        /// <param name="value1">颜色值</param>
        /// <param name="value2">颜色值</param>
        /// <returns>如果相同返回true，否则返回false</returns>
        public static bool Equals(ref Color3f value1, ref Color3f value2)
	    {
		    return ( value1.Red == value2.Red && value1.Green == value2.Green && value1.Blue == value2.Blue );
	    }
        public static UInt32 ToAbgr(Vector3 color)
        {
            UInt32 value = (UInt32)(color.X * 255);
            value += (UInt32)(color.Y * 255) << 8;
            value += (UInt32)(color.Z * 255) << 16;
            value += (UInt32)(255) << 24;
            return value;
        }
        public static UInt32 ToAbgr(Color3f color)
        {
            UInt32 value = (UInt32)(color.Red * 255);
            value += (UInt32)(color.Green * 255) << 8;
            value += (UInt32)(color.Blue * 255) << 16;
            value += (UInt32)(255) << 24;
            return value;
        }
    }
}
