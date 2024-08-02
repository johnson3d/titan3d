using System;
using System.Globalization;

namespace EngineNS
{
    /// <summary>
    /// 带透明通道的颜色值
    /// </summary>
    [System.Serializable]
	[System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
	public struct Color4f
    {
        /// <summary>
        /// 红色值
        /// </summary>
        public float Red;
        /// <summary>
        /// 绿色值
        /// </summary>
        public float Green;
        /// <summary>
        /// 蓝色值
        /// </summary>
        public float Blue;
        /// <summary>
        /// Alpha通道值
        /// </summary>
        public float Alpha;
        public static Color4f FromObject(object obj)
        {
            if (obj.GetType() == typeof(Color4f))
                return (Color4f)obj;
            else if (obj.GetType() == typeof(Vector4))
                return (Vector4)obj;
            else
            {
                System.Diagnostics.Debug.Assert(false);
                return new Color4f();
            }
        }
        /// <summary>
        /// 带参构造函数
        /// </summary>
        /// <param name="alpha">Alpha通道值</param>
        /// <param name="red">红色值</param>
        /// <param name="green">绿色值</param>
        /// <param name="blue">蓝色值</param>
	    public Color4f(float alpha, float red, float green, float blue)
        {
		    Alpha = alpha;
		    Red = red;
		    Green = green;
		    Blue = blue;
	    }
        public Color4f(double alpha, double red, double green, double blue)
        {
            Alpha = (float)alpha;
            Red = (float)red;
            Green = (float)green;
            Blue = (float)blue;
        }
        public Vector3 ToVector3()
        {
            Vector3 result;
            result.X = Red;
            result.Y = Green;
            result.Z = Blue;
            return result;
        }
        public Vector4 ToVector4()
        {
            return new Vector4(Red, Green, Blue, Alpha);
        }
        //public Color4f(Vector4 src)
        //{
        //    Alpha = src.W;
        //    Red = src.X;
        //    Green = src.Y;
        //    Blue = src.Z;
        //}
        /// <summary>
        /// 带参构造函数
        /// </summary>
        /// <param name="red">红色值</param>
        /// <param name="green">绿色值</param>
        /// <param name="blue">蓝色值</param>
	    public Color4f( float red, float green, float blue )
	    {
		    Alpha = 1.0f;
		    Red = red;
		    Green = green;
		    Blue = blue;
	    }
        public Color4f(double red, double green, double blue)
        {
            Alpha = 1.0f;
            Red = (float)red;
            Green = (float)green;
            Blue = (float)blue;
        }
        /// <summary>
        /// 带参构造函数
        /// </summary>
        /// <param name="color">颜色</param>
        public Color4f(Color4b color)
	    {
		    Alpha = color.A / 255.0f;
		    Red = color.R / 255.0f;
		    Green = color.G / 255.0f;
		    Blue = color.B / 255.0f;
	    }
        /// <summary>
        /// 带参构造函数
        /// </summary>
        /// <param name="color">颜色对象</param>
	    public Color4f( Color3f color )
	    {
		    Alpha = 1.0f;
		    Red = color.Red;
		    Green = color.Green;
		    Blue = color.Blue;
	    }
        /// <summary>
        /// 带参构造函数
        /// </summary>
        /// <param name="color">颜色值，使用Vector3表示</param>
	    public Color4f( Vector3 color )
	    {
		    Alpha = 1.0f;
		    Red = color.X;
		    Green = color.Y;
		    Blue = color.Z;
	    }
        /// <summary>
        /// 带参构造函数
        /// </summary>
        /// <param name="color">颜色值，使用Vector4表示</param>
	    public Color4f( Vector4 color )
	    {
		    Alpha = color.W;
		    Red = color.X;
		    Green = color.Y;
		    Blue = color.Z;
	    }
        /// <summary>
        /// 带参构造函数
        /// </summary>
        /// <param name="argb">使用int表示的颜色值</param>
	    public Color4f( uint argb )
	    {
		    Alpha = ( ( argb >> 24 ) & 255 ) / 255.0f;
		    Red = ( ( argb >> 16 ) & 255 ) / 255.0f;
		    Green = ( ( argb >> 8 ) & 255 ) / 255.0f;
		    Blue = ( argb & 255 ) / 255.0f;
	    }

        public bool IsAlmostBlack()
	    {
		    return Red*Red < MathHelper.Epsilon && Green*Green < MathHelper.Epsilon && Red*Red < MathHelper.Epsilon;
	    }

    /// <summary>
    /// 颜色转换
    /// </summary>
    /// <returns>返回转换后的颜色</returns>
    public Color4b ToColor()
	    {
		    return Color4b.FromArgb( (int)(Alpha * 255), (int)(Red * 255), (int)(Green * 255), (int)(Blue * 255) );
	    }
        /// <summary>
        /// 转换成不带Alpha通道值的颜色值
        /// </summary>
        /// <returns>返回转换后的颜色值</returns>
	    public Color3f ToColor3()
	    {
            Color3f result;
            result.Red = Red;
            result.Green = Green;
            result.Blue = Blue;
            return result;
	    }
        /// <summary>
        /// 转换成带Alpha通道值的颜色值
        /// </summary>
        /// <returns>返回转换后的颜色值</returns>
	    public UInt32 ToArgb()
	    {
		    UInt32 a, r, g, b;

		    a = (UInt32)(Alpha * 255.0f);
		    r = (UInt32)(Red * 255.0f);
		    g = (UInt32)(Green * 255.0f);
		    b = (UInt32)(Blue * 255.0f);

		    UInt32 value = b;
		    value += g << 8;
		    value += r << 16;
		    value += a << 24;

		    return value;
	    }
        public UInt32 ToAbgr()
        {
		    UInt32 a, r, g, b;

		    a = (UInt32)(Alpha * 255.0f);
		    r = (UInt32)(Red * 255.0f);
		    g = (UInt32)(Green * 255.0f);
		    b = (UInt32)(Blue * 255.0f);

		    UInt32 value = r;
		    value += g << 8;
		    value += b << 16;
		    value += a << 24;

		    return value;
        }
        public static Color4f FromABGR(Color4b color)
        {
            Color4f retValue = new Color4f();
            retValue.Alpha = ((float)color.A) / 255.0f;
            retValue.Red = ((float)color.R) / 255.0f;
            retValue.Green = ((float)color.G) / 255.0f;
            retValue.Blue = ((float)color.B) / 255.0f;
            return retValue;
        }
        public static UInt32 Argb2Abgr(UInt32 color)
        {
            var a = ((color >> 24) & 255);
            var r = ((color >> 16) & 255);
            var g = ((color >> 8) & 255);
            var b = (color & 255);

            UInt32 value = r;
            value += g << 8;
            value += b << 16;
            value += a << 24;

            return value;
        }
        public static UInt32 ToAbgr(Color4f color)
        {
            UInt32 value = (UInt32)(color.Red * 255);
            value += (UInt32)(color.Green * 255) << 8;
            value += (UInt32)(color.Blue * 255) << 16;
            value += (UInt32)(color.Alpha * 255) << 24;
            return value;
        }
        public static UInt32 ToAbgr(Vector4 color)
        {
            UInt32 value = (UInt32)(color.X * 255);
            value += (UInt32)(color.Y * 255) << 8;
            value += (UInt32)(color.Z * 255) << 16;
            value += (UInt32)(color.W * 255) << 24;
            return value;
        }
        public UInt32 ToRgb()
        {
            UInt32 a, r, g, b;

            a = 255;
            r = (UInt32)(Red * 255.0f);
            g = (UInt32)(Green * 255.0f);
            b = (UInt32)(Blue * 255.0f);

            UInt32 value = b;
            value += g << 8;
            value += r << 16;
            value += a << 24;

            return value;
        }
        public UInt32 ToBgr()
        {
            UInt32 a, r, g, b;

            a = 255;
            r = (UInt32)(Red * 255.0f);
            g = (UInt32)(Green * 255.0f);
            b = (UInt32)(Blue * 255.0f);

            UInt32 value = r;
            value += g << 8;
            value += b << 16;
            value += a << 24;

            return value;
        }
        /// <summary>
        /// 两个颜色值相加
        /// </summary>
        /// <param name="color1">颜色1</param>
        /// <param name="color2">颜色2</param>
        /// <returns>返回相加后的颜色</returns>
	    public static Color4f Add( Color4f color1, Color4f color2 )
	    {
            Color4f result;
            result.Alpha = color1.Alpha + color2.Alpha;
            result.Red = color1.Red + color2.Red;
            result.Green = color1.Green + color2.Green;
            result.Blue = color1.Blue + color2.Blue;
            return result;
	    }
        /// <summary>
        /// 两个颜色值相加
        /// </summary>
        /// <param name="color1">颜色1</param>
        /// <param name="color2">颜色2</param>
        /// <param name="result">相加后的颜色</param>
	    public static void Add( ref Color4f color1, ref Color4f color2, out Color4f result )
	    {
            result.Alpha = color1.Alpha + color2.Alpha;
            result.Red = color1.Red + color2.Red;
            result.Green = color1.Green + color2.Green;
            result.Blue = color1.Blue + color2.Blue;
        }
        /// <summary>
        /// 两个颜色相减
        /// </summary>
        /// <param name="color1">颜色1</param>
        /// <param name="color2">颜色2</param>
        /// <returns>返回相减后的颜色</returns>
	    public static Color4f Subtract( Color4f color1, Color4f color2 )
	    {
            Color4f result;
            result.Alpha = color1.Alpha - color2.Alpha;
            result.Red = color1.Red - color2.Red;
            result.Green = color1.Green - color2.Green;
            result.Blue = color1.Blue - color2.Blue;
            return result;
	    }
        /// <summary>
        /// 两个颜色相减
        /// </summary>
        /// <param name="color1">颜色1</param>
        /// <param name="color2">颜色2</param>
        /// <param name="result">相减后的颜色</param>
	    public static void Subtract( ref Color4f color1, ref Color4f color2, out Color4f result )
	    {
            result.Alpha = color1.Alpha - color2.Alpha;
            result.Red = color1.Red - color2.Red;
            result.Green = color1.Green - color2.Green;
            result.Blue = color1.Blue - color2.Blue;
        }
        /// <summary>
        /// 两个颜色相乘
        /// </summary>
        /// <param name="color1">颜色1</param>
        /// <param name="color2">颜色2</param>
        /// <returns>返回相乘后的颜色</returns>
	    public static Color4f Modulate( Color4f color1, Color4f color2 )
	    {
            Color4f result;
            result.Alpha = color1.Alpha * color2.Alpha;
            result.Red = color1.Red * color2.Red;
            result.Green = color1.Green * color2.Green;
            result.Blue = color1.Blue * color2.Blue;
            return result;
	    }
        /// <summary>
        /// 两个颜色相乘
        /// </summary>
        /// <param name="color1">颜色1</param>
        /// <param name="color2">颜色2</param>
        /// <param name="result">相乘后的颜色</param>
	    public static void Modulate( ref Color4f color1, ref Color4f color2, out Color4f result )
	    {
            result.Alpha = color1.Alpha * color2.Alpha;
            result.Red = color1.Red * color2.Red;
            result.Green = color1.Green * color2.Green;
            result.Blue = color1.Blue * color2.Blue;
	    }
        /// <summary>
        /// 颜色的线性插值计算
        /// </summary>
        /// <param name="color1">颜色1</param>
        /// <param name="color2">颜色2</param>
        /// <param name="amount">插值</param>
        /// <returns>返回计算后的颜色</returns>
	    public static Color4f Lerp( Color4f color1, Color4f color2, float amount )
	    {
            Color4f result;
            result.Alpha = color1.Alpha + amount * ( color2.Alpha - color1.Alpha );
            result.Red = color1.Red + amount * ( color2.Red - color1.Red );
            result.Green = color1.Green + amount * ( color2.Green - color1.Green );
            result.Blue = color1.Blue + amount * ( color2.Blue - color1.Blue );

            return result;
	    }
        /// <summary>
        /// 颜色的线性插值计算
        /// </summary>
        /// <param name="color1">颜色1</param>
        /// <param name="color2">颜色2</param>
        /// <param name="amount">插值</param>
        /// <param name="result">计算后的颜色</param>
	    public static void Lerp( ref Color4f color1, ref Color4f color2, float amount, out Color4f result )
	    {
            result.Alpha = color1.Alpha + amount * ( color2.Alpha - color1.Alpha );
            result.Red = color1.Red + amount * ( color2.Red - color1.Red );
            result.Green = color1.Green + amount * ( color2.Green - color1.Green );
            result.Blue = color1.Blue + amount * ( color2.Blue - color1.Blue );
	    }
        /// <summary>
        /// 颜色的补植
        /// </summary>
        /// <param name="color">颜色对象</param>
        /// <returns>返回计算后的颜色</returns>
	    public static Color4f Negate( Color4f color )
	    {
            Color4f result;
            result.Alpha = 1.0f - color.Alpha;
            result.Red = 1.0f - color.Red;
            result.Green = 1.0f - color.Green;
            result.Blue = 1.0f - color.Blue;

            return result;
	    }
        /// <summary>
        /// 颜色的补植
        /// </summary>
        /// <param name="color">颜色对象</param>
        /// <param name="result">计算后的颜色</param>
	    public static void Negate( ref Color4f color, out Color4f result )
	    {
            result.Alpha = 1.0f - color.Alpha;
            result.Red = 1.0f - color.Red;
            result.Green = 1.0f - color.Green;
            result.Blue = 1.0f - color.Blue;
        }
        /// <summary>
        /// 调整对比度
        /// </summary>
        /// <param name="color">颜色</param>
        /// <param name="contrast">对比度</param>
        /// <returns>返回调整后的颜色</returns>
	    public static Color4f AdjustContrast( Color4f color, float contrast )
	    {
            Color4f result;
            result.Red = 0.5f + contrast * ( color.Red - 0.5f );
            result.Green = 0.5f + contrast * ( color.Green - 0.5f );
            result.Blue = 0.5f + contrast * ( color.Blue - 0.5f );
            result.Alpha = color.Alpha;
            return result;
	    }
        /// <summary>
        /// 调整对比度
        /// </summary>
        /// <param name="color">颜色</param>
        /// <param name="contrast">对比度</param>
        /// <param name="result">调整后的颜色</param>
	    public static void AdjustContrast( ref Color4f color, float contrast, out Color4f result )
	    {
            result.Red = 0.5f + contrast * ( color.Red - 0.5f );
            result.Green = 0.5f + contrast * ( color.Green - 0.5f );
            result.Blue = 0.5f + contrast * ( color.Blue - 0.5f );

            result.Alpha = color.Alpha;
        }
        /// <summary>
        /// 调整饱和度
        /// </summary>
        /// <param name="color">颜色值</param>
        /// <param name="saturation">饱和度</param>
        /// <returns>返回调整后的颜色值</returns>
	    public static Color4f AdjustSaturation( Color4f color, float saturation )
	    {
            Color4f result;
            float grey = color.Red * 0.2125f + color.Green * 0.7154f + color.Blue * 0.0721f;
            result.Red = grey + saturation * ( color.Red - grey );
            result.Green = grey + saturation * ( color.Green - grey );
            result.Blue = grey + saturation * ( color.Blue - grey );

            result.Alpha = color.Alpha;
            return result;
	    }
        /// <summary>
        /// 调整饱和度
        /// </summary>
        /// <param name="color">颜色值</param>
        /// <param name="saturation">饱和度</param>
        /// <param name="result">调整后的颜色值</param>
	    public static void AdjustSaturation( ref Color4f color, float saturation, out Color4f result )
	    {
            float grey = color.Red * 0.2125f + color.Green * 0.7154f + color.Blue * 0.0721f;
            result.Red = grey + saturation * (color.Red - grey);
            result.Green = grey + saturation * (color.Green - grey);
            result.Blue = grey + saturation * (color.Blue - grey);

            result.Alpha = color.Alpha;
        }
        /// <summary>
        /// 颜色值加深
        /// </summary>
        /// <param name="color">颜色值</param>
        /// <param name="scale">深度</param>
        /// <returns>返回计算后的颜色</returns>
	    public static Color4f Scale( Color4f color, float scale )
	    {
            Color4f result;
            result.Red = color.Red * scale;
            result.Green = color.Green * scale;
            result.Blue = color.Blue * scale;

            result.Alpha = color.Alpha;
            return result;
	    }
        /// <summary>
        /// 颜色值加深
        /// </summary>
        /// <param name="color">颜色值</param>
        /// <param name="scale">深度</param>
        /// <param name="result">计算后的颜色</param>
	    public static void Scale( ref Color4f color, float scale, out Color4f result )
	    {
            result.Red = color.Red * scale;
            result.Green = color.Green * scale;
            result.Blue = color.Blue * scale;

            result.Alpha = color.Alpha;
	    }
        /// <summary>
        /// 重载"+"号操作符
        /// </summary>
        /// <param name="color1">颜色值</param>
        /// <param name="color2">颜色值</param>
        /// <returns>返回计算后的颜色值</returns>
	    public static Color4f operator + ( Color4f color1, Color4f color2 )
	    {
            Color4f result;
            result.Red = color1.Red + color2.Red;
            result.Green = color1.Green + color2.Green;
            result.Blue = color1.Blue + color2.Blue;
            result.Alpha = color1.Alpha + color2.Alpha;
            return result;
	    }
        /// <summary>
        /// 重载"-"号操作符
        /// </summary>
        /// <param name="color1">颜色值</param>
        /// <param name="color2">颜色值</param>
        /// <returns>返回计算后的颜色值</returns>
	    public static Color4f operator - ( Color4f color1, Color4f color2 )
	    {
            Color4f result;
            result.Red = color1.Red - color2.Red;
            result.Green = color1.Green - color2.Green;
            result.Blue = color1.Blue - color2.Blue;
            result.Alpha = color1.Alpha - color2.Alpha;
            return result;
        }
        /// <summary>
        /// 重载"-"号操作符
        /// </summary>
        /// <param name="color">颜色值</param>
        /// <returns>返回计算后的颜色值</returns>
	    public static Color4f operator - ( Color4f color )
	    {
            Color4f result;
            result.Red = 1.0f - color.Red;
            result.Green = 1.0f - color.Green;
            result.Blue = 1.0f - color.Blue;
            result.Alpha = 1.0f - color.Alpha;
            return result;
	    }
        /// <summary>
        /// 重载"*"号操作符
        /// </summary>
        /// <param name="color1">颜色值</param>
        /// <param name="color2">颜色值</param>
        /// <returns>返回计算后的颜色值</returns>
	    public static Color4f operator * ( Color4f color1, Color4f color2 )
	    {
            Color4f result;
            result.Red = color1.Red * color2.Red;
            result.Green = color1.Green * color2.Green;
            result.Blue = color1.Blue * color2.Blue;
            result.Alpha = color1.Alpha * color2.Alpha;
            return result;
	    }
        /// <summary>
        /// 重载"*"号操作符
        /// </summary>
        /// <param name="color">颜色值</param>
        /// <param name="scale">放大倍数</param>
        /// <returns>返回计算后的颜色值</returns>
        public static Color4f operator *(Color4f color, float scale)
	    {
            Color4f result;
            result.Red = color.Red * scale;
            result.Green = color.Green * scale;
            result.Blue = color.Blue * scale;
            result.Alpha = color.Alpha * scale;
            return result;
	    }
        /// <summary>
        /// 重载"*"号操作符
        /// </summary>
        /// <param name="scale">放大倍数</param>
        /// <param name="value">颜色值</param>
        /// <returns>返回计算后的颜色值</returns>
        public static Color4f operator * ( float scale, Color4f value )
	    {
		    return value * scale;
	    }
        /// <summary>
        /// 重载"=="号操作符
        /// </summary>
        /// <param name="left">颜色值</param>
        /// <param name="right">颜色值</param>
        /// <returns>两个颜色对象相等返回true，否则返回false</returns>
	    public static bool operator == ( Color4f left, Color4f right )
	    {
            return left.Equals(right);
            //return Equals( left, right );
        }
        /// <summary>
        /// 重载"!="号操作符
        /// </summary>
        /// <param name="left">颜色值</param>
        /// <param name="right">颜色值</param>
        /// <returns>两个颜色对象不相等返回true，否则返回false</returns>
	    public static bool operator != ( Color4f left, Color4f right )
	    {
            return !left.Equals(right);
		    //return !Equals( left, right );
	    }
        /// <summary>
        /// 自定义int类型转换方式
        /// </summary>
        /// <param name="value">颜色对象</param>
	    public static implicit operator uint( Color4f value )
	    {
		    return value.ToArgb();
	    }
        /// <summary>
        /// 自定义Color3类型转换方式
        /// </summary>
        /// <param name="value">颜色对象</param>
	    public static implicit operator Color3f( Color4f value )
	    {
		    return value.ToColor3();
	    }
        /// <summary>
        /// 自定义CSUtility.Support.Color类型转换方式
        /// </summary>
        /// <param name="value">颜色对象</param>
	    public static implicit operator Color4b( Color4f value )
	    {
		    return value.ToColor();
	    }
        /// <summary>
        /// 自定义Vector3类型转换方式
        /// </summary>
        /// <param name="value">颜色对象</param>
	    public static implicit operator Vector3( Color4f value )
	    {
		    return value.ToVector3();
	    }
        /// <summary>
        /// 自定义Vector4类型转换方式
        /// </summary>
        /// <param name="value">颜色对象</param>
	    public static implicit operator Vector4( Color4f value )
	    {
		    return value.ToVector4();
	    }
        /// <summary>
        /// 自定义Color4类型转换方式
        /// </summary>
        /// <param name="value">int类型的值</param>
	    public static implicit operator Color4f( int argb)
	    {
            Color4f result;
            result.Alpha = ((argb >> 24) & 255) / 255.0f;
            result.Red = ((argb >> 16) & 255) / 255.0f;
            result.Green = ((argb >> 8) & 255) / 255.0f;
            result.Blue = (argb & 255) / 255.0f;
            return result;
	    }
        /// <summary>
        /// 自定义Color4类型转换方式
        /// </summary>
        /// <param name="value">Color3类型的对象</param>
	    public static implicit operator Color4f( Color3f value )
	    {
            Color4f result;
            result.Alpha = 1.0f;
            result.Red = value.Red;
            result.Green = value.Green;
            result.Blue = value.Blue;
            return result;
	    }
        /// <summary>
        /// 自定义Color4类型转换方式
        /// </summary>
        /// <param name="value">CSUtility.Support.Color类型的对象</param>
	    public static implicit operator Color4f( Color4b value )
	    {
            Color4f result;
            result.Red = value.R;
            result.Green = value.G;
            result.Blue = value.B;
            result.Alpha = value.A;
            return result;
	    }
        /// <summary>
        /// 自定义Color4类型转换方式
        /// </summary>
        /// <param name="value">Vector3类型的对象</param>
	    public static implicit operator Color4f( Vector3 value )
	    {
            Color4f result;
            result.Red = value.X;
            result.Green = value.Y;
            result.Blue = value.Z;
            result.Alpha = 1;
            return result;
        }
        /// <summary>
        /// 自定义Color4类型转换方式
        /// </summary>
        /// <param name="value">Vector4类型的对象</param>
        public static implicit operator Color4f(Vector4 value)
	    {
            Color4f result;
            result.Red = value.X;
            result.Green = value.Y;
            result.Blue = value.Z;
            result.Alpha = value.W;
            return result;
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
                    case 3:
                        return Alpha;
                    default:
                        throw new ArgumentOutOfRangeException("index", "Indices for Vector4f run from 0 to 2, inclusive.");
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
                    case 3:
                        Alpha = value;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException("index", "Indices for Vector4f run from 0 to 2, inclusive.");
                }
            }
        }
        public static Color4f FromString(string text)
        {
            try
            {
                var result = new Color4f();
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
                        if (j == 3)
                            break;
                    }
                }
                result[j] = float.Parse(chars.Slice(iStart, chars.Length - iStart));
                return result;
            }
            catch
            {
                return new Color4f(0,0,0);
            }
        }
        public override string ToString()
	    {
            return $"{Red},{Green},{Blue},{Alpha}";
            //return string.Format( CultureInfo.CurrentCulture, "A:{0} R:{1} G:{2} B:{3}", 
            // Alpha.ToString(CultureInfo.CurrentCulture), Red.ToString(CultureInfo.CurrentCulture), 
            // Green.ToString(CultureInfo.CurrentCulture), Blue.ToString(CultureInfo.CurrentCulture) );
        }
        /// <summary>
        /// 获取该对象的哈希值
        /// </summary>
        /// <returns>返回该对象的哈希值</returns>
	    public override int GetHashCode()
	    {
		    return Alpha.GetHashCode() + Red.GetHashCode() + Green.GetHashCode() + Blue.GetHashCode();
	    }
        /// <summary>
        /// 判断是否相等
        /// </summary>
        /// <param name="value">实例对象</param>
        /// <returns>如果相等返回true，否则返回false</returns>
	    public override bool Equals( object value )
	    {
		    if( value == null )
			    return false;

		    if( value.GetType() != GetType() )
			    return false;

		    return Equals( (Color4f)( value ) );
	    }
        /// <summary>
        /// 判断两个对象是否相等
        /// </summary>
        /// <param name="value">Color4实例对象</param>
        /// <returns>如果相等返回true，否则返回false</returns>
	    public bool Equals( Color4f value )
	    {
		    return ( Alpha == value.Alpha && Red == value.Red && Green == value.Green && Blue == value.Blue );
	    }
        /// <summary>
        /// 判断两个对象是否相等
        /// </summary>
        /// <param name="value1">Color4实例对象</param>
        /// <param name="value2">Color4实例对象</param>
        /// <returns>如果相等返回true，否则返回false</returns>
	    public static bool Equals( ref Color4f value1, ref Color4f value2 )
	    {
		    return ( value1.Alpha == value2.Alpha && value1.Red == value2.Red && value1.Green == value2.Green && value1.Blue == value2.Blue );
	    }
    }
}
