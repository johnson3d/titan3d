using System;
using System.Collections.Generic;

using System.Globalization;

namespace EngineNS
{
    /// <summary>
    /// 二维向量结构体
    /// </summary>
    [Vector2.Vector2Editor]
	[System.Runtime.InteropServices.StructLayout( System.Runtime.InteropServices.LayoutKind.Sequential, Pack = 4 )]
    [Vector2.TypeConverterAttribute]
    public struct Vector2 : System.IEquatable<Vector2>
    {
        public class TypeConverterAttribute : Support.TypeConverterAttributeBase
        {
            public override bool ConvertFromString(ref object obj, string text)
            {
                obj = Vector2.FromString(text);
                return true;
            }
        }
        public class Vector2EditorAttribute : EGui.Controls.PropertyGrid.PGCustomValueEditorAttribute
        {
            public override unsafe bool OnDraw(in EditorInfo info, out object newValue)
            {
                newValue = info.Value;
                var v = (Vector2)info.Value;
                //var saved = v;
                var index = ImGuiAPI.TableGetColumnIndex();
                var width = ImGuiAPI.GetColumnWidth(index);
                ImGuiAPI.SetNextItemWidth(width - EGui.UIProxy.StyleConfig.Instance.PGCellPadding.X);
                //ImGuiAPI.AlignTextToFramePadding();
                //ImGuiAPI.PushStyleVar(ImGuiStyleVar_.ImGuiStyleVar_FramePadding, ref EGui.UIProxy.StyleConfig.Instance.PGInputFramePadding);
                var minValue = float.MinValue;
                var maxValue = float.MaxValue;
                var changed = ImGuiAPI.DragScalarN2(TName.FromString2("##", info.Name).ToString(), ImGuiDataType_.ImGuiDataType_Float, (float*)&v, 2, 0.1f, &minValue, &maxValue, "%0.6f", ImGuiSliderFlags_.ImGuiSliderFlags_None);
                //ImGuiAPI.InputFloat2(, (float*)&v, "%.6f", ImGuiInputTextFlags_.ImGuiInputTextFlags_CharsDecimal);
                //ImGuiAPI.PopStyleVar(1);
                if (changed)//(v != saved)
                {
                    newValue = v;
                    return true;
                }
                return false;
            }
        }
        public override string ToString()
        {
            return $"{X},{Y}";
        }
        public static Vector2 FromString(string text)
        {
            try
            {
                var segs = text.Split(',');
                return new Vector2(System.Convert.ToSingle(segs[0]),
                    System.Convert.ToSingle(segs[1]));
            }
            catch
            {
                return new Vector2();
            }
        }
        public void SetValue(float x, float y)
        {
            X = x;
            Y = y;
        }
        /// <summary>
        /// X的值
        /// </summary>
        [Rtti.Meta]
        public float X;
        /// <summary>
        /// Y的值
        /// </summary>
        [Rtti.Meta]
        public float Y;
        /// <summary>
        /// 带参构造函数
        /// </summary>
        /// <param name="value">值</param>
        public Vector2( float value )
	    {
		    X = value;
		    Y = value;
	    }
        /// <summary>
        /// 带参构造函数
        /// </summary>
        /// <param name="x">X的值</param>
        /// <param name="y">Y的值</param>
        public Vector2(float x, float y)
	    {
		    X = x;
		    Y = y;
	    }
        /// <summary>
        /// 根据索引值设置对象的值
        /// </summary>
        /// <param name="index">索引</param>
        /// <returns>返回设置的对象</returns>
        public float this[int index]
        {
            get
	        {
		        switch( index )
		        {
		        case 0:
			        return X;
		        case 1:
			        return Y;
		        default:
			        throw new ArgumentOutOfRangeException( "index", "Indices for Vector2 run from 0 to 1, inclusive." );
		        }
	        }
            set
	        {
		        switch( index )
		        {
		        case 0:
			        X = value;
			        break;
		        case 1:
			        Y = value;
			        break;
		        default:
			        throw new ArgumentOutOfRangeException( "index", "Indices for Vector2 run from 0 to 1, inclusive." );
		        }
	        }
        }
        public static Vector2 Zero
        {
            get { return mZero; }
        }
        static Vector2 mZero = new Vector2(0, 0);
        [Rtti.Meta]
        public static Vector2 UnitX { get { return mUnitX; } }
        public static Vector2 mUnitX = new Vector2(1, 0);
        [Rtti.Meta]
        public static Vector2 UnitY { get{ return mUnitY; } }
        public static Vector2 mUnitY = new Vector2(0, 1);
        [Rtti.Meta]
        public static Vector2 UnitXY { get { return mUnitXY; } }
        public static Vector2 mUnitXY = new Vector2(1, 1);
        [Rtti.Meta]
        public static Vector2 InvUnitXY { get { return InvmUnitXY; } }
        public static Vector2 InvmUnitXY = new Vector2(-1, -1);
        [Rtti.Meta]
        public static int SizeInBytes { get{ return System.Runtime.InteropServices.Marshal.SizeOf(typeof(Vector2)); } }

        #region Equal Override
        /// <summary>
        /// 获取对象的哈希值
        /// </summary>
        /// <returns>返回对象的哈希值</returns>
	    public override int GetHashCode()
	    {
		    return X.GetHashCode() + Y.GetHashCode();
	    }
        /// <summary>
        /// 判断两个对象是否相同
        /// </summary>
        /// <param name="value">可以转换到Vector2类型的对象</param>
        /// <returns>如果两个对象相同返回true，否则返回false</returns>
	    public override bool Equals( object value )
	    {
		    if( value == null )
			    return false;

		    if( value.GetType() != GetType() )
			    return false;

		    return Equals( (Vector2)( value ) );
	    }
        /// <summary>
        /// 判断两个对象是否相同
        /// </summary>
        /// <param name="value">二维向量对象</param>
        /// <returns>如果两个对象相同返回true，否则返回false</returns>
	    public bool Equals( Vector2 value )
	    {
		    return ( X == value.X && Y == value.Y );
	    }
        /// <summary>
        /// 判断两个对象是否相同
        /// </summary>
        /// <param name="value1">二维向量对象</param>
        /// <param name="value2">二维向量对象</param>
        /// <returns>如果两个对象相同返回true，否则返回false</returns>
	    public static bool Equals( ref Vector2 value1, ref Vector2 value2 )
	    {
		    return ( value1.X == value2.X && value1.Y == value2.Y );
	    }
        #endregion
        /// <summary>
        /// 向量的长度
        /// </summary>
        /// <returns>返回向量的长度</returns>
        [Rtti.Meta]
        public float Length()
	    {
		    return (float)( Math.Sqrt( (X * X) + (Y * Y) ) );
	    }
        /// <summary>
        /// 长度的平方
        /// </summary>
        /// <returns>返回长度的平方</returns>
        [Rtti.Meta]
        public float LengthSquared()
        {
            return (X * X) + (Y * Y);
        }
        /// <summary>
        /// 二维向量的单位化
        /// </summary>
        [Rtti.Meta]
        public void Normalize()
        {
            float length = Length();
		    if( length == 0 )
			    return;
		    float num = 1 / length;
		    X *= num;
		    Y *= num;
	    }
        /// <summary>
        /// 两个向量的和
        /// </summary>
        /// <param name="left">二维向量对象</param>
        /// <param name="right">二维向量对象</param>
        /// <returns>返回两个向量的和</returns>
        [Rtti.Meta]
        public static Vector2 Add( Vector2 left, Vector2 right )
	    {
            Vector2 ret;
            ret.X = left.X + right.X;
            ret.Y = left.Y + right.Y;
            return ret;
	    }
        /// <summary>
        /// 两个向量的和
        /// </summary>
        /// <param name="left">二维向量对象</param>
        /// <param name="right">二维向量对象</param>
        /// <param name="result">两个向量的和</param>
        [Rtti.Meta]
        public static void Add( ref Vector2 left, ref Vector2 right, out Vector2 result )
	    {
            result.X = left.X + right.X;
            result.Y = left.Y + right.Y;
	    }
        /// <summary>
        /// 两个向量的差
        /// </summary>
        /// <param name="left">二维向量对象</param>
        /// <param name="right">二维向量对象</param>
        /// <returns>返回两个向量的差</returns>
        [Rtti.Meta]
        public static Vector2 Subtract( Vector2 left, Vector2 right )
	    {
            Vector2 ret;
            ret.X = left.X - right.X;
            ret.Y = left.Y - right.Y;
            return ret;
	    }
        /// <summary>
        /// 两个向量的差
        /// </summary>
        /// <param name="left">二维向量对象</param>
        /// <param name="right">二维向量对象</param>
        /// <param name="result">两个向量的差</param>
        [Rtti.Meta]
        public static void Subtract( ref Vector2 left, ref Vector2 right, out Vector2 result )
	    {
            result.X = left.X - right.X;
            result.Y = left.Y - right.Y;
	    }
        [Rtti.Meta]
        public static Vector2 Modulate( Vector2 left, Vector2 right )
	    {
            Vector2 result;
            result.X = left.X * right.X;
            result.Y = left.Y * right.Y;
            return result;
	    }
        /// <summary>
        /// 两个向量的积
        /// </summary>
        /// <param name="left">二维向量对象</param>
        /// <param name="right">二维向量对象</param>
        /// <param name="result">两个向量的积</param>
        [Rtti.Meta]
        public static void Modulate( ref Vector2 left, ref Vector2 right, out Vector2 result )
	    {
            result.X = left.X * right.X;
            result.Y = left.Y * right.Y;
        }
        [Rtti.Meta]
        public static Vector2 Multiply( Vector2 value, float scale )
	    {
            Vector2 result;
            result.X = value.X * scale;
            result.Y = value.Y * scale;
            return result;
	    }
        /// <summary>
        /// 向量与常数的积
        /// </summary>
        /// <param name="value">二维向量对象</param>
        /// <param name="scale">常数</param>
        /// <param name="result">向量与常数的积</param>
        [Rtti.Meta]
        public static void Multiply( ref Vector2 value, float scale, out Vector2 result )
	    {
            result.X = value.X * scale;
            result.Y = value.Y * scale;
	    }
        /// <summary>
        /// 向量与常数的商
        /// </summary>
        /// <param name="value">二维向量对象</param>
        /// <param name="scale">常数</param>
        /// <returns>返回向量与常数的商</returns>
        [Rtti.Meta]
        public static Vector2 Divide( Vector2 value, float scale )
	    {
            Vector2 result;
            result.X = value.X / scale;
            result.Y = value.Y / scale;
            return result;
	    }
        /// <summary>
        /// 向量与常数的商
        /// </summary>
        /// <param name="value">二维向量对象</param>
        /// <param name="scale">常数</param>
        /// <param name="result">向量与常数的商</param>
        [Rtti.Meta]
        public static void Divide( ref Vector2 value, float scale, out Vector2 result )
	    {
            result.X = value.X / scale;
            result.Y = value.Y / scale;
        }
        /// <summary>
        /// 向量取反
        /// </summary>
        /// <param name="value">二维向量对象</param>
        /// <returns>返回向量取反的结果</returns>
        [Rtti.Meta]
        public static Vector2 Negate( Vector2 value )
	    {
            Vector2 result;
            result.X = -value.X;
            result.Y = -value.Y;
            return result;
	    }
        /// <summary>
        /// 向量取反
        /// </summary>
        /// <param name="value">二维向量对象</param>
        /// <param name="result">向量取反的结果</param>
        [Rtti.Meta]
        public static void Negate( ref Vector2 value, out Vector2 result )
	    {
            result.X = -value.X;
            result.Y = -value.Y;
        }
        /// <summary>
        /// 求对象的质心
        /// </summary>
        /// <param name="value1">二维向量对象</param>
        /// <param name="value2">二维向量对象</param>
        /// <param name="value3">二维向量对象</param>
        /// <param name="amount1">参数</param>
        /// <param name="amount2">参数</param>
        /// <returns>返回对象的质心坐标</returns>
        [Rtti.Meta]
        public static Vector2 Barycentric( Vector2 value1, Vector2 value2, Vector2 value3, float amount1, float amount2 )
	    {
		    Vector2 vector;
		    vector.X = (value1.X + (amount1 * (value2.X - value1.X))) + (amount2 * (value3.X - value1.X));
		    vector.Y = (value1.Y + (amount1 * (value2.Y - value1.Y))) + (amount2 * (value3.Y - value1.Y));
		    return vector;
	    }
        /// <summary>
        /// 求对象的质心
        /// </summary>
        /// <param name="value1">二维向量对象</param>
        /// <param name="value2">二维向量对象</param>
        /// <param name="value3">二维向量对象</param>
        /// <param name="amount1">参数</param>
        /// <param name="amount2">参数</param>
        /// <param name="result">对象的质心坐标</param>
        [Rtti.Meta]
        public static void Barycentric( ref Vector2 value1, ref Vector2 value2, ref Vector2 value3, float amount1, float amount2, out Vector2 result )
	    {
            result.X = (value1.X + (amount1 * (value2.X - value1.X))) + (amount2 * (value3.X - value1.X));
            result.Y = (value1.Y + (amount1 * (value2.Y - value1.Y))) + (amount2 * (value3.Y - value1.Y));
	    }
        /// <summary>
        /// 插值计算
        /// </summary>
        /// <param name="value1">二维坐标点</param>
        /// <param name="value2">二维坐标点</param>
        /// <param name="value3">二维坐标点</param>
        /// <param name="value4">二维坐标点</param>
        /// <param name="amount">插值数据</param>
        /// <returns>返回计算后的坐标点</returns>
        [Rtti.Meta]
        public static Vector2 CatmullRom( Vector2 value1, Vector2 value2, Vector2 value3, Vector2 value4, float amount )
	    {
            Vector2 vector;
		    float squared = amount * amount;
		    float cubed = amount * squared;

		    vector.X = 0.5f * ((((2.0f * value2.X) + ((-value1.X + value3.X) * amount)) + 
			    (((((2.0f * value1.X) - (5.0f * value2.X)) + (4.0f * value3.X)) - value4.X) * squared)) + 
			    ((((-value1.X + (3.0f * value2.X)) - (3.0f * value3.X)) + value4.X) * cubed));

		    vector.Y = 0.5f * ((((2.0f * value2.Y) + ((-value1.Y + value3.Y) * amount)) + 
			    (((((2.0f * value1.Y) - (5.0f * value2.Y)) + (4.0f * value3.Y)) - value4.Y) * squared)) + 
			    ((((-value1.Y + (3.0f * value2.Y)) - (3.0f * value3.Y)) + value4.Y) * cubed));

		    return vector;
	    }
        /// <summary>
        /// 插值计算
        /// </summary>
        /// <param name="value1">二维坐标点</param>
        /// <param name="value2">二维坐标点</param>
        /// <param name="value3">二维坐标点</param>
        /// <param name="value4">二维坐标点</param>
        /// <param name="amount">插值数据</param>
        /// <param name="result">计算后的坐标点</param>
        [Rtti.Meta]
        public static void CatmullRom( ref Vector2 value1, ref Vector2 value2, ref Vector2 value3, ref Vector2 value4, float amount, out Vector2 result )
	    {
		    float squared = amount * amount;
		    float cubed = amount * squared;
            Vector2 r;

		    r.X = 0.5f * ((((2.0f * value2.X) + ((-value1.X + value3.X) * amount)) + 
			    (((((2.0f * value1.X) - (5.0f * value2.X)) + (4.0f * value3.X)) - value4.X) * squared)) + 
			    ((((-value1.X + (3.0f * value2.X)) - (3.0f * value3.X)) + value4.X) * cubed));

		    r.Y = 0.5f * ((((2.0f * value2.Y) + ((-value1.Y + value3.Y) * amount)) + 
			    (((((2.0f * value1.Y) - (5.0f * value2.Y)) + (4.0f * value3.Y)) - value4.Y) * squared)) + 
			    ((((-value1.Y + (3.0f * value2.Y)) - (3.0f * value3.Y)) + value4.Y) * cubed));

		    result = r;
	    }
        /// <summary>
        /// 载体计算
        /// </summary>
        /// <param name="value">二维坐标点</param>
        /// <param name="min">二维坐标点的最小值</param>
        /// <param name="max">二维坐标点的最大值</param>
        /// <returns>返回计算后的坐标</returns>
        [Rtti.Meta]
        public static Vector2 Clamp( Vector2 value, Vector2 min, Vector2 max )
	    {
            Vector2 result;

            float x = value.X;
		    x = (x > max.X) ? max.X : x;
		    x = (x < min.X) ? min.X : x;

		    float y = value.Y;
		    y = (y > max.Y) ? max.Y : y;
		    y = (y < min.Y) ? min.Y : y;

            result.X = x;
            result.Y = y;
            return result;
	    }
        /// <summary>
        /// 载体计算
        /// </summary>
        /// <param name="value">二维坐标点</param>
        /// <param name="min">二维坐标点的最小值</param>
        /// <param name="max">二维坐标点的最大值</param>
        /// <param name="result">计算后的坐标</param>
        [Rtti.Meta]
        public static void Clamp( ref Vector2 value, ref Vector2 min, ref Vector2 max, out Vector2 result )
	    {
		    float x = value.X;
		    x = (x > max.X) ? max.X : x;
		    x = (x < min.X) ? min.X : x;

		    float y = value.Y;
		    y = (y > max.Y) ? max.Y : y;
		    y = (y < min.Y) ? min.Y : y;

            result.X = x;
            result.Y = y;
        }
        /// <summary>
        /// 艾米插值计算
        /// </summary>
        /// <param name="value1">二维向量</param>
        /// <param name="tangent1">二维向量的正切点坐标</param>
        /// <param name="value2">二维向量</param>
        /// <param name="tangent2">二维向量的正切点坐标</param>
        /// <param name="amount">插值</param>
        /// <returns>返回计算后的向量</returns>
        [Rtti.Meta]
        public static Vector2 Hermite( Vector2 value1, Vector2 tangent1, Vector2 value2, Vector2 tangent2, float amount )
	    {
            Vector2 vector;
		    float squared = amount * amount;
		    float cubed = amount * squared;
		    float part1 = ((2.0f * cubed) - (3.0f * squared)) + 1.0f;
		    float part2 = (-2.0f * cubed) + (3.0f * squared);
		    float part3 = (cubed - (2.0f * squared)) + amount;
		    float part4 = cubed - squared;

		    vector.X = (((value1.X * part1) + (value2.X * part2)) + (tangent1.X * part3)) + (tangent2.X * part4);
		    vector.Y = (((value1.Y * part1) + (value2.Y * part2)) + (tangent1.Y * part3)) + (tangent2.Y * part4);

		    return vector;
	    }
        /// <summary>
        /// 艾米插值计算
        /// </summary>
        /// <param name="value1">二维向量</param>
        /// <param name="tangent1">二维向量的正切点坐标</param>
        /// <param name="value2">二维向量</param>
        /// <param name="tangent2">二维向量的正切点坐标</param>
        /// <param name="amount">插值</param>
        /// <param name="result">计算后的向量</param>
        [Rtti.Meta]
        public static void Hermite( ref Vector2 value1, ref Vector2 tangent1, ref Vector2 value2, ref Vector2 tangent2, float amount, out Vector2 result )
	    {
		    float squared = amount * amount;
		    float cubed = amount * squared;
		    float part1 = ((2.0f * cubed) - (3.0f * squared)) + 1.0f;
		    float part2 = (-2.0f * cubed) + (3.0f * squared);
		    float part3 = (cubed - (2.0f * squared)) + amount;
		    float part4 = cubed - squared;

            Vector2 r;
		    r.X = (((value1.X * part1) + (value2.X * part2)) + (tangent1.X * part3)) + (tangent2.X * part4);
		    r.Y = (((value1.Y * part1) + (value2.Y * part2)) + (tangent1.Y * part3)) + (tangent2.Y * part4);

		    result = r;
        }
        /// <summary>
        /// 计算线性插值
        /// </summary>
        /// <param name="start">起点坐标</param>
        /// <param name="end">终点坐标</param>
        /// <param name="factor">插值因子</param>
        /// <returns>返回计算后的向量</returns>
        [Rtti.Meta]
        public static Vector2 Lerp(Vector2 start, Vector2 end, float factor)
        {
            Vector2 vector;

            vector.X = start.X + ((end.X - start.X) * factor);
            vector.Y = start.Y + ((end.Y - start.Y) * factor);

            return vector;
	    }
        /// <summary>
        /// 计算线性插值
        /// </summary>
        /// <param name="start">起点坐标</param>
        /// <param name="end">终点坐标</param>
        /// <param name="factor">插值因子</param>
        /// <param name="result">计算后的向量</param>
        [Rtti.Meta]
        public static void Lerp( ref Vector2 start, ref Vector2 end, float factor, out Vector2 result )
	    {
		    Vector2 r;
		    r.X = start.X + ((end.X - start.X) * factor);
		    r.Y = start.Y + ((end.Y - start.Y) * factor);

		    result = r;
	    }
        /// <summary>
        /// 平滑插值计算
        /// </summary>
        /// <param name="start">起点坐标</param>
        /// <param name="end">终点坐标</param>
        /// <param name="amount">插值因子</param>
        /// <returns>返回计算后的向量</returns>
        [Rtti.Meta]
        public static Vector2 SmoothStep( Vector2 start, Vector2 end, float amount )
	    {
            Vector2 vector;

		    amount = (amount > 1.0f) ? 1.0f : ((amount < 0.0f) ? 0.0f : amount);
		    amount = (amount * amount) * (3.0f - (2.0f * amount));

		    vector.X = start.X + ((end.X - start.X) * amount);
		    vector.Y = start.Y + ((end.Y - start.Y) * amount);

		    return vector;
	    }
        /// <summary>
        /// 平滑插值计算
        /// </summary>
        /// <param name="start">起点坐标</param>
        /// <param name="end">终点坐标</param>
        /// <param name="amount">插值因子</param>
        /// <param name="result">计算后的向量</param>
        [Rtti.Meta]
        public static void SmoothStep( ref Vector2 start, ref Vector2 end, float amount, out Vector2 result )
	    {
		    amount = (amount > 1.0f) ? 1.0f : ((amount < 0.0f) ? 0.0f : amount);
		    amount = (amount * amount) * (3.0f - (2.0f * amount));

            Vector2 r;
		    r.X = start.X + ((end.X - start.X) * amount);
		    r.Y = start.Y + ((end.Y - start.Y) * amount);

		    result = r;
	    }
        /// <summary>
        /// 计算两点间的距离
        /// </summary>
        /// <param name="value1">坐标点</param>
        /// <param name="value2">坐标点</param>
        /// <returns>返回两点间的距离</returns>
        [Rtti.Meta]
        public static float Distance( Vector2 value1, Vector2 value2 )
	    {
		    float x = value1.X - value2.X;
		    float y = value1.Y - value2.Y;

		    return (float)( Math.Sqrt( (x * x) + (y * y) ) );
	    }
        /// <summary>
        /// 计算两点间的距离的平方
        /// </summary>
        /// <param name="value1">坐标点</param>
        /// <param name="value2">坐标点</param>
        /// <returns>返回两点间的距离的平方</returns>
        [Rtti.Meta]
        public static float DistanceSquared( Vector2 value1, Vector2 value2 )
	    {
		    float x = value1.X - value2.X;
		    float y = value1.Y - value2.Y;

		    return (x * x) + (y * y);
	    }
        /// <summary>
        /// 向量的点积
        /// </summary>
        /// <param name="left">二维向量</param>
        /// <param name="right">二维向量</param>
        /// <returns>返回点积值</returns>
        [Rtti.Meta]
        public static float Dot( Vector2 left, Vector2 right )
	    {
		    return (left.X * right.X + left.Y * right.Y);
	    }
        /// <summary>
        /// 向量的单位化
        /// </summary>
        /// <param name="vector">二维向量</param>
        /// <returns>返回单位化后的向量</returns>
        [Rtti.Meta]
        public static Vector2 Normalize( Vector2 vector )
	    {
		    vector.Normalize();
		    return vector;
	    }
        /// <summary>
        /// 向量的单位化
        /// </summary>
        /// <param name="vector">二维向量</param>
        /// <param name="result">单位化后的向量</param>
        [Rtti.Meta]
        public static void Normalize( ref Vector2 vector, out Vector2 result )
	    {
		    result = Normalize(vector);
	    }
        /// <summary>
        /// 二维向量的坐标转换运算
        /// </summary>
        /// <param name="vector">二维向量</param>
        /// <param name="transform">转换矩阵</param>
        /// <returns>返回转换后的向量</returns>
        [Rtti.Meta]
        public static Vector4 Transform( Vector2 vector, Matrix transform )
	    {
            Vector4 result;

		    result.X = (vector.X * transform.M11) + (vector.Y * transform.M21) + transform.M41;
		    result.Y = (vector.X * transform.M12) + (vector.Y * transform.M22) + transform.M42;
		    result.Z = (vector.X * transform.M13) + (vector.Y * transform.M23) + transform.M43;
		    result.W = (vector.X * transform.M14) + (vector.Y * transform.M24) + transform.M44;

		    return result;
	    }
        /// <summary>
        /// 二维向量的坐标转换运算
        /// </summary>
        /// <param name="vector">二维向量</param>
        /// <param name="transform">转换矩阵</param>
        /// <param name="result">转换后的向量</param>
        [Rtti.Meta]
        public static void Transform( ref Vector2 vector, ref Matrix transform, out Vector4 result )
	    {
            Vector4 r;
		    r.X = (vector.X * transform.M11) + (vector.Y * transform.M21) + transform.M41;
		    r.Y = (vector.X * transform.M12) + (vector.Y * transform.M22) + transform.M42;
		    r.Z = (vector.X * transform.M13) + (vector.Y * transform.M23) + transform.M43;
		    r.W = (vector.X * transform.M14) + (vector.Y * transform.M24) + transform.M44;

		    result = r;
	    }
        /// <summary>
        /// 二维向量的坐标转换运算
        /// </summary>
        /// <param name="vectors">二维向量列表</param>
        /// <param name="transform">转换矩阵</param>
        /// <returns>返回转换后的向量列表</returns>
        public static Vector4[] Transform( Vector2[] vectors, ref Matrix transform )
	    {
		    if( vectors == null )
			    throw new ArgumentNullException( "vectors" );

		    int count = vectors.Length;
		    Vector4[] results = new Vector4[ count ];

		    for( int i = 0; i < count; i++ )
		    {
                Vector4 r;
			    r.X = (vectors[i].X * transform.M11) + (vectors[i].Y * transform.M21) + transform.M41;
			    r.Y = (vectors[i].X * transform.M12) + (vectors[i].Y * transform.M22) + transform.M42;
			    r.Z = (vectors[i].X * transform.M13) + (vectors[i].Y * transform.M23) + transform.M43;
			    r.W = (vectors[i].X * transform.M14) + (vectors[i].Y * transform.M24) + transform.M44;

			    results[i] = r;
		    }

		    return results;
	    }
        /// <summary>
        /// 二维向量的坐标转换运算
        /// </summary>
        /// <param name="value">二维向量</param>
        /// <param name="rotation">旋转四元数</param>
        /// <returns>返回转换后的向量</returns>
        [Rtti.Meta]
        public static Vector4 Transform( Vector2 value, Quaternion rotation )
	    {
            Vector4 vector;
		    float x = rotation.X + rotation.X;
		    float y = rotation.Y + rotation.Y;
		    float z = rotation.Z + rotation.Z;
		    float wx = rotation.W * x;
		    float wy = rotation.W * y;
		    float wz = rotation.W * z;
		    float xx = rotation.X * x;
		    float xy = rotation.X * y;
		    float xz = rotation.X * z;
		    float yy = rotation.Y * y;
		    float yz = rotation.Y * z;
		    float zz = rotation.Z * z;

		    vector.X = ((value.X * ((1.0f - yy) - zz)) + (value.Y * (xy - wz)));
		    vector.Y = ((value.X * (xy + wz)) + (value.Y * ((1.0f - xx) - zz)));
		    vector.Z = ((value.X * (xz - wy)) + (value.Y * (yz + wx)));
		    vector.W = 1.0f;

		    return vector;
	    }
        /// <summary>
        /// 二维向量的坐标转换运算
        /// </summary>
        /// <param name="value">二维向量</param>
        /// <param name="rotation">旋转四元数</param>
        /// <param name="result">转换后的向量</param>
        [Rtti.Meta]
        public static void Transform( ref Vector2 value, ref Quaternion rotation, out Vector4 result )
	    {
		    float x = rotation.X + rotation.X;
		    float y = rotation.Y + rotation.Y;
		    float z = rotation.Z + rotation.Z;
		    float wx = rotation.W * x;
		    float wy = rotation.W * y;
		    float wz = rotation.W * z;
		    float xx = rotation.X * x;
		    float xy = rotation.X * y;
		    float xz = rotation.X * z;
		    float yy = rotation.Y * y;
		    float yz = rotation.Y * z;
		    float zz = rotation.Z * z;

            Vector4 r;
		    r.X = ((value.X * ((1.0f - yy) - zz)) + (value.Y * (xy - wz)));
		    r.Y = ((value.X * (xy + wz)) + (value.Y * ((1.0f - xx) - zz)));
		    r.Z = ((value.X * (xz - wy)) + (value.Y * (yz + wx)));
		    r.W = 1.0f;

		    result = r;
	    }
        /// <summary>
        /// 二维向量的坐标转换运算
        /// </summary>
        /// <param name="vectors">二维向量列表</param>
        /// <param name="rotation">旋转四元数</param>
        /// <returns>返回转换后的向量列表</returns>
        public static Vector4[] Transform( Vector2[] vectors, ref Quaternion rotation )
	    {
		    if( vectors == null )
			    throw new ArgumentNullException( "vectors" );

		    int count = vectors.Length;
		    Vector4[] results = new Vector4[ count ];

		    float x = rotation.X + rotation.X;
		    float y = rotation.Y + rotation.Y;
		    float z = rotation.Z + rotation.Z;
		    float wx = rotation.W * x;
		    float wy = rotation.W * y;
		    float wz = rotation.W * z;
		    float xx = rotation.X * x;
		    float xy = rotation.X * y;
		    float xz = rotation.X * z;
		    float yy = rotation.Y * y;
		    float yz = rotation.Y * z;
		    float zz = rotation.Z * z;

		    for( int i = 0; i < count; i++ )
		    {
                Vector4 r;
			    r.X = ((vectors[i].X * ((1.0f - yy) - zz)) + (vectors[i].Y * (xy - wz)));
			    r.Y = ((vectors[i].X * (xy + wz)) + (vectors[i].Y * ((1.0f - xx) - zz)));
			    r.Z = ((vectors[i].X * (xz - wy)) + (vectors[i].Y * (yz + wx)));
			    r.W = 1.0f;

			    results[i] = r;
		    }

		    return results;
	    }
        /// <summary>
        /// 坐标轴转换
        /// </summary>
        /// <param name="coord">坐标轴向量</param>
        /// <param name="transform">转换矩阵</param>
        /// <returns>返回转换后的向量</returns>
        [Rtti.Meta]
        public static Vector2 TransformCoordinate( Vector2 coord, Matrix transform )
	    {
            Vector4 vector;

		    vector.X = (coord.X * transform.M11) + (coord.Y * transform.M21) + transform.M41;
		    vector.Y = (coord.X * transform.M12) + (coord.Y * transform.M22) + transform.M42;
		    vector.Z = (coord.X * transform.M13) + (coord.Y * transform.M23) + transform.M43;
		    vector.W = 1 / ((coord.X * transform.M14) + (coord.Y * transform.M24) + transform.M44);

            Vector2 result;
            result.X = vector.X * vector.W;
            result.Y = vector.Y * vector.W;
            return result;
	    }
        [Rtti.Meta]
        public static void TransformCoordinate( ref Vector2 coord, ref Matrix transform, out Vector2 result )
	    {
            Vector4 vector;

		    vector.X = (coord.X * transform.M11) + (coord.Y * transform.M21) + transform.M41;
		    vector.Y = (coord.X * transform.M12) + (coord.Y * transform.M22) + transform.M42;
		    vector.Z = (coord.X * transform.M13) + (coord.Y * transform.M23) + transform.M43;
		    vector.W = 1 / ((coord.X * transform.M14) + (coord.Y * transform.M24) + transform.M44);
            
            result.X = vector.X * vector.W;
            result.Y = vector.Y * vector.W;
	    }
        public static Vector2[] TransformCoordinate( Vector2[] coords, ref Matrix transform )
	    {
		    if( coords == null )
			    throw new ArgumentNullException( "coordinates" );

            Vector4 vector;
		    int count = coords.Length;
		    Vector2[] results = new Vector2[ count ];

		    for( int i = 0; i < count; i++ )
		    {
			    vector.X = (coords[i].X * transform.M11) + (coords[i].Y * transform.M21) + transform.M41;
			    vector.Y = (coords[i].X * transform.M12) + (coords[i].Y * transform.M22) + transform.M42;
			    vector.Z = (coords[i].X * transform.M13) + (coords[i].Y * transform.M23) + transform.M43;
			    vector.W = 1 / ((coords[i].X * transform.M14) + (coords[i].Y * transform.M24) + transform.M44);
                results[i].X = vector.X * vector.W;
                results[i].Y = vector.Y * vector.W;
		    }

		    return results;
	    }
        [Rtti.Meta]
        public static Vector2 TransformNormal( Vector2 normal, Matrix transform )
	    {
            Vector2 vector;

		    vector.X = (normal.X * transform.M11) + (normal.Y * transform.M21);
		    vector.Y = (normal.X * transform.M12) + (normal.Y * transform.M22);

		    return vector;
	    }
        /// <summary>
        /// 单位向量转换
        /// </summary>
        /// <param name="normal">单位向量</param>
        /// <param name="transform">转换矩阵</param>
        /// <param name="result">转换后的向量</param>
        [Rtti.Meta]
        public static void TransformNormal( ref Vector2 normal, ref Matrix transform, out Vector2 result )
	    {
            Vector2 r;
		    r.X = (normal.X * transform.M11) + (normal.Y * transform.M21);
		    r.Y = (normal.X * transform.M12) + (normal.Y * transform.M22);

		    result = r;
	    }
        /// <summary>
        /// 单位向量转换
        /// </summary>
        /// <param name="normals">单位向量列表</param>
        /// <param name="transform">转换矩阵</param>
        /// <returns>返回转换后的向量列表</returns>
        public static Vector2[] TransformNormal( Vector2[] normals, ref Matrix transform )
	    {
		    if( normals == null )
			    throw new ArgumentNullException( "normals" );

		    int count = normals.Length;
		    Vector2[] results = new Vector2[ count ];

		    for( int i = 0; i < count; i++ )
		    {
                Vector2 r;
			    r.X = (normals[i].X * transform.M11) + (normals[i].Y * transform.M21);
			    r.Y = (normals[i].X * transform.M12) + (normals[i].Y * transform.M22);

			    results[i] = r;
		    }

		    return results;
	    }
        /// <summary>
        /// 向量的最小化
        /// </summary>
        /// <param name="left">二维向量</param>
        /// <param name="right">二维向量</param>
        /// <returns>返回最小化后的向量</returns>
        [Rtti.Meta]
        public static Vector2 Minimize( Vector2 left, Vector2 right )
	    {
            Vector2 vector;
		    vector.X = (left.X < right.X) ? left.X : right.X;
		    vector.Y = (left.Y < right.Y) ? left.Y : right.Y;
		    return vector;
	    }
        /// <summary>
        /// 向量的最小化
        /// </summary>
        /// <param name="left">二维向量</param>
        /// <param name="right">二维向量</param>
        /// <param name="result">最小化后的向量</param>
        [Rtti.Meta]
        public static void Minimize( ref Vector2 left, ref Vector2 right, out Vector2 result )
	    {
            Vector2 r;
		    r.X = (left.X < right.X) ? left.X : right.X;
		    r.Y = (left.Y < right.Y) ? left.Y : right.Y;

		    result = r;
	    }
        /// <summary>
        /// 向量的最大化
        /// </summary>
        /// <param name="left">二维向量</param>
        /// <param name="right">二维向量</param>
        /// <returns>返回最大化后的向量</returns>
        [Rtti.Meta]
        public static Vector2 Maximize( Vector2 left, Vector2 right )
	    {
            Vector2 vector;
		    vector.X = (left.X > right.X) ? left.X : right.X;
		    vector.Y = (left.Y > right.Y) ? left.Y : right.Y;
		    return vector;
	    }
        /// <summary>
        /// 向量的最大化
        /// </summary>
        /// <param name="left">二维向量</param>
        /// <param name="right">二维向量</param>
        /// <param name="result">最大化后的向量</param>
        [Rtti.Meta]
        public static void Maximize( ref Vector2 left, ref Vector2 right, out Vector2 result )
	    {
            Vector2 r;
		    r.X = (left.X > right.X) ? left.X : right.X;
		    r.Y = (left.Y > right.Y) ? left.Y : right.Y;

		    result = r;
	    }
        /// <summary>
        /// 重载"+"号运算符
        /// </summary>
        /// <param name="left">二维向量</param>
        /// <param name="right">二维向量</param>
        /// <returns>返回计算后的向量</returns>
        public static Vector2 operator + ( Vector2 left, Vector2 right )
	    {
            Vector2 result;
            result.X = left.X + right.X;
            result.Y = left.Y + right.Y;
            return result;
	    }
        /// <summary>
        /// 重载"-"号运算符
        /// </summary>
        /// <param name="left">二维向量</param>
        /// <param name="right">二维向量</param>
        /// <returns>返回计算后的向量</returns>
	    public static Vector2 operator - ( Vector2 left, Vector2 right )
	    {
            Vector2 result;
            result.X = left.X - right.X;
            result.Y = left.Y - right.Y;
            return result;
	    }
        /// <summary>
        /// 重载"-"号运算符
        /// </summary>
        /// <param name="value">二维向量</param>
        /// <returns>返回计算后的向量</returns>
        public static Vector2 operator - ( Vector2 value )
	    {
            Vector2 result;
            result.X = -value.X;
            result.Y = -value.Y;
            return result;
	    }
        /// <summary>
        /// 重载"*"号运算符
        /// </summary>
        /// <param name="value">二维向量</param>
        /// <param name="scale">乘数</param>
        /// <returns>返回计算后的向量</returns>
        public static Vector2 operator * ( Vector2 value, float scale )
	    {
            Vector2 result;
            result.X = value.X * scale;
            result.Y = value.Y * scale;
            return result;
	    }
        /// <summary>
        /// 重载"*"号运算符
        /// </summary>
        /// <param name="scale">乘数</param>
        /// <param name="vec">二维向量</param>
        /// <returns>返回计算后的向量</returns>
        public static Vector2 operator * ( float scale, Vector2 vec )
	    {
		    return vec * scale;
	    }
        /// <summary>
        /// 重载"/"号运算符
        /// </summary>
        /// <param name="value">二维向量</param>
        /// <param name="scale">除数</param>
        /// <returns>返回计算后的向量</returns>
        public static Vector2 operator / ( Vector2 value, float scale )
	    {
            Vector2 result;
            result.X = value.X / scale;
            result.Y = value.Y / scale;
            return result;
	    }
        /// <summary>
        /// 重载"=="号运算符
        /// </summary>
        /// <param name="left">二维向量</param>
        /// <param name="right">二维向量</param>
        /// <returns>如果两个向量相等返回true，否则返回false</returns>
        public static bool operator == ( Vector2 left, Vector2 right )
	    {
            return left.Equals(right);
            //return Equals( left, right );
        }
        /// <summary>
        /// 重载"!="号运算符
        /// </summary>
        /// <param name="left">二维向量</param>
        /// <param name="right">二维向量</param>
        /// <returns>如果两个向量不相等返回true，否则返回false</returns>
        public static bool operator != ( Vector2 left, Vector2 right )
	    {
            return !left.Equals(right);
		    //return !Equals( left, right );
	    }
    }
}
