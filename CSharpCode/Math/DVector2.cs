using System;
using System.Collections.Generic;

using System.Globalization;

namespace EngineNS
{
    /// <summary>
    /// 二维向量结构体
    /// </summary>
    [DVector2.Vector2Editor]
	[System.Runtime.InteropServices.StructLayout( System.Runtime.InteropServices.LayoutKind.Sequential, Pack = 4 )]
    [DVector2.TypeConverterAttribute]
    public struct DVector2 : System.IEquatable<DVector2>
    {
        public class TypeConverterAttribute : Support.TypeConverterAttributeBase
        {
            public override bool ConvertFromString(ref object obj, string text)
            {
                obj = DVector2.FromString(text);
                return true;
            }
        }
        public class Vector2EditorAttribute : EGui.Controls.PropertyGrid.PGCustomValueEditorAttribute
        {
            public override unsafe bool OnDraw(in EditorInfo info, out object newValue)
            {
                this.Expandable = true;
                newValue = info.Value;
                //var saved = v;
                var index = ImGuiAPI.TableGetColumnIndex();
                var width = ImGuiAPI.GetColumnWidth(index);
                ImGuiAPI.SetNextItemWidth(width - EGui.UIProxy.StyleConfig.Instance.PGCellPadding.X);
                //ImGuiAPI.AlignTextToFramePadding();
                //ImGuiAPI.PushStyleVar(ImGuiStyleVar_.ImGuiStyleVar_FramePadding, ref EGui.UIProxy.StyleConfig.Instance.PGInputFramePadding);
                var minValue = double.MinValue;
                var maxValue = double.MaxValue;
                var name = TName.FromString2("##", info.Name).ToString();
                var multiValue = info.Value as EGui.Controls.PropertyGrid.PropertyMultiValue;
                bool retValue = false;
                if (multiValue != null && multiValue.HasDifferentValue())
                {
                    ImGuiAPI.Text(multiValue.MultiValueString);
                    if (multiValue.DrawVector<DVector2>(in info) && !info.Readonly)
                    {
                        newValue = multiValue;
                        retValue = true;
                    }
                }
                else
                {
                    var v = (DVector2)info.Value;
                    float speed = 0.1f;
                    var format = "%.9lf";
                    if (info.HostProperty != null)
                    {
                        var vR = info.HostProperty.GetAttribute<EGui.Controls.PropertyGrid.PGValueRange>();
                        if (vR != null)
                        {
                            minValue = (double)vR.Min;
                            maxValue = (double)vR.Max;
                        }
                        var vStep = info.HostProperty.GetAttribute<EGui.Controls.PropertyGrid.PGValueChangeStep>();
                        if (vStep != null)
                        {
                            speed = vStep.Step;
                        }
                        var vFormat = info.HostProperty.GetAttribute<EGui.Controls.PropertyGrid.PGValueFormat>();
                        if (vFormat != null)
                            format = vFormat.Format;
                    }
                    var changed = ImGuiAPI.DragScalarN2(name, ImGuiDataType_.ImGuiDataType_Double, (double*)&v, 2, speed, &minValue, &maxValue, format, ImGuiSliderFlags_.ImGuiSliderFlags_None);
                    //ImGuiAPI.InputFloat2(, (double*)&v, "%.6f", ImGuiInputTextFlags_.ImGuiInputTextFlags_CharsDecimal);
                    //ImGuiAPI.PopStyleVar(1);
                    if (changed && !info.Readonly)//(v != saved)
                    {
                        newValue = v;
                        retValue = true;
                    }

                    if (Vector4.Vector4EditorAttribute.OnDrawDVectorValue<DVector2>(in info, ref v, ref v) && !info.Readonly)
                    {
                        newValue = v;
                        retValue = true;
                    }
                }
                return retValue;
            }
        }
        public Vector2 AsSingleVector()
        {
            return new Vector2((float)X, (float)Y);
        }
        public override string ToString()
        {
            return $"{X},{Y}";
        }
        public static DVector2 FromString(string text)
        {
            try
            {
                var result = new DVector2();
                ReadOnlySpan<char> chars = text.ToCharArray();
                var pos = chars.IndexOf(',');
                result.X = float.Parse(chars.Slice(0, pos));
                result.Y = float.Parse(chars.Slice(pos + 1, chars.Length - pos - 1));
                return result;
                //var segs = text.Split(',');
                //return new DVector2(System.Convert.ToSingle(segs[0]),
                //    System.Convert.ToSingle(segs[1]));
            }
            catch
            {
                return new DVector2();
            }
        }
        public void SetValue(double x, double y)
        {
            X = x;
            Y = y;
        }
        /// <summary>
        /// X的值
        /// </summary>
        [Rtti.Meta]
        public double X;
        /// <summary>
        /// Y的值
        /// </summary>
        [Rtti.Meta]
        public double Y;
        /// <summary>
        /// 带参构造函数
        /// </summary>
        /// <param name="value">值</param>
        public DVector2( double value )
	    {
		    X = value;
		    Y = value;
	    }
        /// <summary>
        /// 带参构造函数
        /// </summary>
        /// <param name="x">X的值</param>
        /// <param name="y">Y的值</param>
        public DVector2(double x, double y)
	    {
		    X = x;
		    Y = y;
	    }
        /// <summary>
        /// 根据索引值设置对象的值
        /// </summary>
        /// <param name="index">索引</param>
        /// <returns>返回设置的对象</returns>
        public double this[int index]
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
			        throw new ArgumentOutOfRangeException( "index", "Indices for DVector2 run from 0 to 1, inclusive." );
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
			        throw new ArgumentOutOfRangeException( "index", "Indices for DVector2 run from 0 to 1, inclusive." );
		        }
	        }
        }
        public readonly static DVector2 MaxValue = new DVector2(double.MaxValue, double.MaxValue);
        public readonly static DVector2 MinValue = new DVector2(double.MinValue, double.MinValue);
        public readonly static DVector2 Zero = new DVector2(0, 0);
        public readonly static DVector2 One = new DVector2(1, 1);
        public readonly static DVector2 NegativeOne = new DVector2(-1, -1);
        [Rtti.Meta]
        public static DVector2 UnitX { get { return mUnitX; } }
        public readonly static DVector2 mUnitX = new DVector2(1, 0);
        [Rtti.Meta]
        public static DVector2 UnitY { get{ return mUnitY; } }
        public readonly static DVector2 mUnitY = new DVector2(0, 1);
        [Rtti.Meta]
        public static DVector2 UnitXY { get { return mUnitXY; } }
        public readonly static DVector2 mUnitXY = new DVector2(1, 1);
        [Rtti.Meta]
        public readonly static DVector2 InvmUnitXY = new DVector2(-1, -1);
        [Rtti.Meta]
        public static int SizeInBytes 
        { 
            get
            {
                unsafe
                {
                    return sizeof(DVector2);
                }
            }
        }

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
        /// <param name="value">可以转换到DVector2类型的对象</param>
        /// <returns>如果两个对象相同返回true，否则返回false</returns>
	    public override bool Equals( object value )
	    {
		    if( value == null )
			    return false;

		    if( value.GetType() != GetType() )
			    return false;

		    return Equals( (DVector2)( value ) );
	    }
        /// <summary>
        /// 判断两个对象是否相同
        /// </summary>
        /// <param name="value">二维向量对象</param>
        /// <returns>如果两个对象相同返回true，否则返回false</returns>
	    public bool Equals( DVector2 value )
	    {
		    return ( X == value.X && Y == value.Y );
	    }
        /// <summary>
        /// 判断两个对象是否相同
        /// </summary>
        /// <param name="value1">二维向量对象</param>
        /// <param name="value2">二维向量对象</param>
        /// <returns>如果两个对象相同返回true，否则返回false</returns>
	    public static bool Equals( ref DVector2 value1, ref DVector2 value2 )
	    {
		    return ( value1.X == value2.X && value1.Y == value2.Y );
	    }
        #endregion
        /// <summary>
        /// 向量的长度
        /// </summary>
        /// <returns>返回向量的长度</returns>
        [Rtti.Meta]
        public double Length()
	    {
		    return (double)( Math.Sqrt( (X * X) + (Y * Y) ) );
	    }
        /// <summary>
        /// 长度的平方
        /// </summary>
        /// <returns>返回长度的平方</returns>
        [Rtti.Meta]
        public double LengthSquared()
        {
            return (X * X) + (Y * Y);
        }
        /// <summary>
        /// 二维向量的单位化
        /// </summary>
        [Rtti.Meta]
        public double Normalize()
        {
            double length = Length();
		    if( length == 0 )
			    return 0;
		    double num = 1 / length;
		    X *= num;
		    Y *= num;
            return length;
        }
        /// <summary>
        /// 两个向量的和
        /// </summary>
        /// <param name="left">二维向量对象</param>
        /// <param name="right">二维向量对象</param>
        /// <returns>返回两个向量的和</returns>
        [Rtti.Meta]
        public static DVector2 Add( DVector2 left, DVector2 right )
	    {
            DVector2 ret;
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
        public static void Add( ref DVector2 left, ref DVector2 right, out DVector2 result )
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
        public static DVector2 Subtract( DVector2 left, DVector2 right )
	    {
            DVector2 ret;
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
        public static void Subtract( ref DVector2 left, ref DVector2 right, out DVector2 result )
	    {
            result.X = left.X - right.X;
            result.Y = left.Y - right.Y;
	    }
        [Rtti.Meta]
        public static DVector2 Modulate( DVector2 left, DVector2 right )
	    {
            DVector2 result;
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
        public static void Modulate( ref DVector2 left, ref DVector2 right, out DVector2 result )
	    {
            result.X = left.X * right.X;
            result.Y = left.Y * right.Y;
        }
        [Rtti.Meta]
        public static DVector2 Multiply( DVector2 value, double scale )
	    {
            DVector2 result;
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
        public static void Multiply( ref DVector2 value, double scale, out DVector2 result )
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
        public static DVector2 Divide( DVector2 value, double scale )
	    {
            DVector2 result;
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
        public static void Divide( ref DVector2 value, double scale, out DVector2 result )
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
        public static DVector2 Negate( DVector2 value )
	    {
            DVector2 result;
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
        public static void Negate( ref DVector2 value, out DVector2 result )
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
        public static DVector2 Barycentric(in DVector2 value1, in DVector2 value2, in DVector2 value3, double amount1, double amount2 )
	    {
		    DVector2 vector;
		    vector.X = (value1.X + (amount1 * (value2.X - value1.X))) + (amount2 * (value3.X - value1.X));
		    vector.Y = (value1.Y + (amount1 * (value2.Y - value1.Y))) + (amount2 * (value3.Y - value1.Y));
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
        /// <returns>返回计算后的坐标点</returns>
        [Rtti.Meta]
        public static DVector2 CatmullRom( DVector2 value1, DVector2 value2, DVector2 value3, DVector2 value4, double amount )
	    {
            DVector2 vector;
		    double squared = amount * amount;
		    double cubed = amount * squared;

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
        public static void CatmullRom( ref DVector2 value1, ref DVector2 value2, ref DVector2 value3, ref DVector2 value4, double amount, out DVector2 result )
	    {
		    double squared = amount * amount;
		    double cubed = amount * squared;
            DVector2 r;

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
        public static DVector2 Clamp( DVector2 value, DVector2 min, DVector2 max )
	    {
            DVector2 result;

            double x = value.X;
		    x = (x > max.X) ? max.X : x;
		    x = (x < min.X) ? min.X : x;

		    double y = value.Y;
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
        public static void Clamp( in DVector2 value, in DVector2 min, in DVector2 max, out DVector2 result )
	    {
		    double x = value.X;
		    x = (x > max.X) ? max.X : x;
		    x = (x < min.X) ? min.X : x;

		    double y = value.Y;
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
        public static DVector2 Hermite( DVector2 value1, DVector2 tangent1, DVector2 value2, DVector2 tangent2, double amount )
	    {
            DVector2 vector;
		    double squared = amount * amount;
		    double cubed = amount * squared;
		    double part1 = ((2.0f * cubed) - (3.0f * squared)) + 1.0f;
		    double part2 = (-2.0f * cubed) + (3.0f * squared);
		    double part3 = (cubed - (2.0f * squared)) + amount;
		    double part4 = cubed - squared;

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
        public static void Hermite( ref DVector2 value1, ref DVector2 tangent1, ref DVector2 value2, ref DVector2 tangent2, double amount, out DVector2 result )
	    {
		    double squared = amount * amount;
		    double cubed = amount * squared;
		    double part1 = ((2.0f * cubed) - (3.0f * squared)) + 1.0f;
		    double part2 = (-2.0f * cubed) + (3.0f * squared);
		    double part3 = (cubed - (2.0f * squared)) + amount;
		    double part4 = cubed - squared;

            DVector2 r;
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
        public static DVector2 Lerp(DVector2 start, DVector2 end, double factor)
        {
            DVector2 vector;

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
        public static void Lerp( ref DVector2 start, ref DVector2 end, double factor, out DVector2 result )
	    {
		    DVector2 r;
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
        public static DVector2 SmoothStep( DVector2 start, DVector2 end, double amount )
	    {
            DVector2 vector;

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
        public static void SmoothStep( ref DVector2 start, ref DVector2 end, double amount, out DVector2 result )
	    {
		    amount = (amount > 1.0f) ? 1.0f : ((amount < 0.0f) ? 0.0f : amount);
		    amount = (amount * amount) * (3.0f - (2.0f * amount));

            DVector2 r;
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
        public static double Distance( DVector2 value1, DVector2 value2 )
	    {
		    double x = value1.X - value2.X;
		    double y = value1.Y - value2.Y;

		    return (double)( Math.Sqrt( (x * x) + (y * y) ) );
	    }
        /// <summary>
        /// 计算两点间的距离的平方
        /// </summary>
        /// <param name="value1">坐标点</param>
        /// <param name="value2">坐标点</param>
        /// <returns>返回两点间的距离的平方</returns>
        [Rtti.Meta]
        public static double DistanceSquared( DVector2 value1, DVector2 value2 )
	    {
		    double x = value1.X - value2.X;
		    double y = value1.Y - value2.Y;

		    return (x * x) + (y * y);
	    }
        /// <summary>
        /// 向量的点积
        /// </summary>
        /// <param name="left">二维向量</param>
        /// <param name="right">二维向量</param>
        /// <returns>返回点积值</returns>
        [Rtti.Meta]
        public static double Dot( DVector2 left, DVector2 right )
	    {
		    return (left.X * right.X + left.Y * right.Y);
	    }
        /// <summary>
        /// 向量的单位化
        /// </summary>
        /// <param name="vector">二维向量</param>
        /// <returns>返回单位化后的向量</returns>
        [Rtti.Meta]
        public static DVector2 Normalize( DVector2 vector )
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
        public static void Normalize( ref DVector2 vector, out DVector2 result )
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
        public static DVector4 Transform( DVector2 vector, Matrix transform )
	    {
            DVector4 result;

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
        public static void Transform( ref DVector2 vector, ref Matrix transform, out DVector4 result )
	    {
            DVector4 r;
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
        public static DVector4[] Transform( DVector2[] vectors, ref Matrix transform )
	    {
		    if( vectors == null )
			    throw new ArgumentNullException( "vectors" );

		    int count = vectors.Length;
		    DVector4[] results = new DVector4[ count ];

		    for( int i = 0; i < count; i++ )
		    {
                DVector4 r;
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
        public static DVector4 Transform( DVector2 value, Quaternion rotation )
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
        public static void Transform( ref DVector2 value, ref Quaternion rotation, out DVector4 result )
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
        public static DVector4[] Transform( DVector2[] vectors, ref Quaternion rotation )
	    {
		    if( vectors == null )
			    throw new ArgumentNullException( "vectors" );

		    int count = vectors.Length;
		    DVector4[] results = new DVector4[ count ];

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

		    for( int i = 0; i < count; i++ )
		    {
                DVector4 r;
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
        public static DVector2 TransformCoordinate(in DVector2 coord, in DMatrix3x3 transform )
	    {
            DVector3 vector;

		    vector.X = (coord.X * transform.M11) + (coord.Y * transform.M21) + transform.M31;
		    vector.Y = (coord.X * transform.M12) + (coord.Y * transform.M22) + transform.M32;
		    vector.Z = 1 / ((coord.X * transform.M13) + (coord.Y * transform.M23) + transform.M33);

            DVector2 result;
            result.X = vector.X * vector.Z;
            result.Y = vector.Y * vector.Z;
            return result;
	    }
        [Rtti.Meta]
        public static void TransformCoordinate(in DVector2 coord, in DMatrix3x3 transform, out DVector2 result )
	    {
            DVector3 vector;

		    vector.X = (coord.X * transform.M11) + (coord.Y * transform.M21) + transform.M31;
		    vector.Y = (coord.X * transform.M12) + (coord.Y * transform.M22) + transform.M32;
		    vector.Z = 1 / ((coord.X * transform.M13) + (coord.Y * transform.M23) + transform.M33);
            
            result.X = vector.X * vector.Z;
            result.Y = vector.Y * vector.Z;
	    }
        public static DVector2[] TransformCoordinate(DVector2[] coords, in DMatrix3x3 transform )
	    {
		    if( coords == null )
			    throw new ArgumentNullException( "coordinates" );

            DVector3 vector;
		    int count = coords.Length;
		    DVector2[] results = new DVector2[ count ];

		    for( int i = 0; i < count; i++ )
		    {
			    vector.X = (coords[i].X * transform.M11) + (coords[i].Y * transform.M21) + transform.M31;
			    vector.Y = (coords[i].X * transform.M12) + (coords[i].Y * transform.M22) + transform.M32;
			    vector.Z = 1 / ((coords[i].X * transform.M13) + (coords[i].Y * transform.M23) + transform.M33);
                results[i].X = vector.X * vector.Z;
                results[i].Y = vector.Y * vector.Z;
		    }

		    return results;
	    }
        [Rtti.Meta]
        public static DVector2 TransformNormal( DVector2 normal, DMatrix3x3 transform )
	    {
            DVector2 vector;

		    vector.X = (normal.X * transform.M11) + (normal.Y * transform.M21);
		    vector.Y = (normal.X * transform.M12) + (normal.Y * transform.M22);

		    return vector;
	    }
        [Rtti.Meta]
        public static DVector2 TransformNormal(DVector2 normal, DMatrix2x2 transform)
        {
            DVector2 vector;

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
        public static void TransformNormal(in DVector2 normal, in DMatrix3x3 transform, out DVector2 result )
	    {
            DVector2 r;
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
        public static DVector2[] TransformNormal( DVector2[] normals, ref DMatrix3x3 transform )
	    {
		    if( normals == null )
			    throw new ArgumentNullException( "normals" );

		    int count = normals.Length;
		    DVector2[] results = new DVector2[ count ];

		    for( int i = 0; i < count; i++ )
		    {
                DVector2 r;
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
        public static DVector2 Minimize(in DVector2 left,in DVector2 right )
	    {
            DVector2 vector;
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
        public static void Minimize(in DVector2 left, in DVector2 right, out DVector2 result )
	    {
            DVector2 r;
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
        public static DVector2 Maximize(in DVector2 left, in DVector2 right )
	    {
            DVector2 vector;
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
        public static void Maximize(in DVector2 left, in DVector2 right, out DVector2 result )
	    {
            DVector2 r;
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
        public static DVector2 operator + (in DVector2 left, in DVector2 right )
	    {
            DVector2 result;
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
	    public static DVector2 operator - (in DVector2 left, in DVector2 right )
	    {
            DVector2 result;
            result.X = left.X - right.X;
            result.Y = left.Y - right.Y;
            return result;
	    }
        /// <summary>
        /// 重载"-"号运算符
        /// </summary>
        /// <param name="value">二维向量</param>
        /// <returns>返回计算后的向量</returns>
        public static DVector2 operator - (in DVector2 value )
	    {
            DVector2 result;
            result.X = -value.X;
            result.Y = -value.Y;
            return result;
	    }
        public static DVector2 operator *(in DVector2 value, in DVector2 scale)
        {
            DVector2 result;
            result.X = value.X * scale.X;
            result.Y = value.Y * scale.Y;
            return result;
        }
        public static DVector2 operator * (in DVector2 value, double scale )
	    {
            DVector2 result;
            result.X = value.X * scale;
            result.Y = value.Y * scale;
            return result;
	    }
        public static DVector2 operator /(in DVector2 value, in DVector2 scale)
        {
            DVector2 result;
            result.X = value.X / scale.X;
            result.Y = value.Y / scale.Y;
            return result;
        }
        public static DVector2 operator / (in DVector2 value, double scale )
	    {
            DVector2 result;
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
        public static bool operator == (in DVector2 left, in DVector2 right )
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
        public static bool operator != (in DVector2 left, in DVector2 right )
	    {
            return !left.Equals(right);
		    //return !Equals( left, right );
	    }

        public static bool Intersects(in DVector2 minbox, in DVector2 maxbox, in DVector2 center, double radius)
        {
            DVector2 clamped;

            DVector2.Clamp(in center, in minbox, in maxbox, out clamped);

            double x = center.X - clamped.X;
            double y = center.Y - clamped.Y;

            double dist = (x * x) + (y * y);
            var ret = (dist <= (radius * radius));

            return ret;
        }
    }
}
