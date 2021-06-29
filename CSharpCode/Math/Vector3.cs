using System;
using System.Collections.Generic;

//using System.Runtime.InteropServices.OutAttribute;

namespace EngineNS
{
    /// <summary>
    /// 三维向量对象
    /// </summary>
    [Vector3.Vector3Editor]
    [Vector3.TypeConverter]
	[System.Runtime.InteropServices.StructLayout( System.Runtime.InteropServices.LayoutKind.Sequential, Pack = 4 )]
    public struct Vector3 : System.IEquatable<Vector3>
    {
        public class TypeConverterAttribute : Support.TypeConverterAttributeBase
        {
            public override bool ConvertFromString(ref object obj, string text)
            {
                obj = Vector3.FromString(text);
                return true;
            }
        }
        public class Vector3EditorAttribute : EGui.Controls.PropertyGrid.PGCustomValueEditorAttribute
        {
            public override unsafe bool OnDraw(in EditorInfo info, out object newValue)
            {
                newValue = info.Value;
                //var saved = v;
                var index = ImGuiAPI.TableGetColumnIndex();
                var width = ImGuiAPI.GetColumnWidth(index);
                ImGuiAPI.SetNextItemWidth(width - EGui.UIProxy.StyleConfig.Instance.PGCellPadding.X);
                //ImGuiAPI.PushStyleVar(ImGuiStyleVar_.ImGuiStyleVar_FramePadding, ref EGui.UIProxy.StyleConfig.Instance.PGInputFramePadding);
                var minValue = float.MinValue;
                var maxValue = float.MaxValue;
                var multiValue = info.Value as EGui.Controls.PropertyGrid.PropertyMultiValue;
                if (multiValue != null && multiValue.HasDifferentValue())
                {
                    ImGuiAPI.Text(multiValue.MultiValueString);
                }
                else
                {
                    var v = (Vector3)info.Value;
                    var changed = ImGuiAPI.DragScalarN2(TName.FromString2("##", info.Name).ToString(), ImGuiDataType_.ImGuiDataType_Float, (float*)&v, 3, 0.1f, &minValue, &maxValue, "%0.6f", ImGuiSliderFlags_.ImGuiSliderFlags_None);
                    //ImGuiAPI.InputFloat3(TName.FromString2("##", info.Name).ToString(), (float*)&v, "%.6f", ImGuiInputTextFlags_.ImGuiInputTextFlags_CharsDecimal);
                    //ImGuiAPI.PopStyleVar(1);
                    if (changed)//(v != saved)
                    {
                        newValue = v;
                        return true;
                    }
                }
                return false;
            }
        }
        public override string ToString()
        {
            return $"{X},{Y},{Z}";
            //return string.Format(System.Globalization.CultureInfo.CurrentCulture, "X:{0} Y:{1} Z:{2}", 
            //    X.ToString(System.Globalization.CultureInfo.CurrentCulture), 
            //    Y.ToString(System.Globalization.CultureInfo.CurrentCulture), 
            //    Z.ToString(System.Globalization.CultureInfo.CurrentCulture));
        }
        public static Vector3 FromString(string text)
        {
            try
            {
                var segs = text.Split(',');
                return new Vector3(System.Convert.ToSingle(segs[0]),
                    System.Convert.ToSingle(segs[1]),
                    System.Convert.ToSingle(segs[2]));
            }
            catch
            {
                return new Vector3();
            }
        }
        public void SetValue(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }
        public bool IsValid()
        {
            return !float.IsNaN(X) && !float.IsNaN(Y) && !float.IsNaN(Z);
        }
        #region Def Struct
        /// <summary>
        /// Gets or sets the X component of the vector.
        /// </summary>
        /// <value>The X component of the vector.</value>
        [Rtti.Meta]
        public float X;

        /// <summary>
        /// Gets or sets the Y component of the vector.
        /// </summary>
        /// <value>The Y component of the vector.</value>
        [Rtti.Meta]
        public float Y;
        /// <summary>
        /// Gets or sets the Z component of the vector.
        /// </summary>
        /// <value>The Z component of the vector.</value>
        [Rtti.Meta]
        public float Z;
        public float this[int index]
        {
            get
            {
                switch (index)
                {
                    case 0:
                        return X;
                    case 1:
                        return Y;
                    case 2:
                        return Z;
                    default:
                        throw new ArgumentOutOfRangeException("index", "Indices for Vector3 run from 0 to 2, inclusive.");
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
		        case 2:
			        Z = value;
			        break;
		        default:
			        throw new ArgumentOutOfRangeException( "index", "Indices for Vector3 run from 0 to 2, inclusive." );
		        }
            }
        }
        #endregion

        #region Static Member
        public bool IsValid(ref Vector3 val)
        {
            return !float.IsNaN(val.X) && !float.IsNaN(val.Y) && !float.IsNaN(val.Z);
        }
        /// <summary>
        /// (1,1,1)向量
        /// </summary>
        [Rtti.Meta]
        public static Vector3 UnitXYZ = new Vector3(1, 1, 1);
        /// <summary>
        /// 零向量
        /// </summary>
        [Rtti.Meta]
        public static Vector3 Zero = new Vector3(0, 0, 0);
        /// <summary>
        /// X轴的单位向量
        /// </summary>
        [Rtti.Meta]
        public static Vector3 UnitX = new Vector3(1, 0, 0);
        /// <summary>
        /// Y轴的单位向量
        /// </summary>
        [Rtti.Meta]
        public static Vector3 UnitY = new Vector3(0, 1, 0);
        /// <summary>
        /// Z轴的单位向量
        /// </summary>
        [Rtti.Meta]
        public static Vector3 UnitZ = new Vector3(0, 0, 1);
        
        public static Vector3 One = new Vector3(1f, 1f, 1f);
        public static Vector3 Up = new Vector3(0f, 1f, 0f);
        public static Vector3 Down = new Vector3(0f, -1f, 0f);
        public static Vector3 Right = new Vector3(1f, 0f, 0f);
        public static Vector3 Left = new Vector3(-1f, 0f, 0f);
        public static Vector3 Forward = new Vector3(0f, 0f, -1f);
        public static Vector3 Backward = new Vector3(0f, 0f, 1f);
        #endregion

        #region Constructure
        /// <summary>
        /// 带参构造函数
        /// </summary>
        /// <param name="value">Vector3对象</param>
        public Vector3(Vector3 value)
        {
            X = value.X;
            Y = value.Y;
            Z = value.Z;
        }
        /// <summary>
        /// 带参构造函数
        /// </summary>
        /// <param name="value">值</param>
        public Vector3( float value )
	    {
		    X = value;
		    Y = value;
		    Z = value;
	    }
        //public Vector3( Vector2 value, float z )
        //{
        //    X = value.X;
        //    Y = value.Y;
        //    Z = z;
        //}
        /// <summary>
        /// 带参构造函数
        /// </summary>
        /// <param name="x">X的值</param>
        /// <param name="y">Y的值</param>
        /// <param name="z">Z的值</param>
        public Vector3(float x, float y, float z)
	    {
		    X = x;
		    Y = y;
		    Z = z;
	    }
        #endregion
        /// <summary>
        /// 长度
        /// </summary>
        /// <returns>返回向量的长度</returns>
        [Rtti.Meta]
        public float Length()
	    {
		    return (float)( System.Math.Sqrt( (X * X) + (Y * Y) + (Z * Z) ) );
	    }
        /// <summary>
        /// 长度的平方
        /// </summary>
        /// <returns>返回向量长度的平方</returns>
        [Rtti.Meta]
        public float LengthSquared()
	    {
		    return (X * X) + (Y * Y) + (Z * Z);
	    }
        /// <summary>
        /// 向量的单位化
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
		    Z *= num;
	    }
        /// <summary>
        /// 向量的单位向量，不改变原向量
        /// </summary>
        [Rtti.Meta]
        public Vector3 NormalizeValue
        {
            get {
                var temp = this;
                temp.Normalize();
                return temp;
                }
        }
        /// <summary>
        /// 两个三维向量的和
        /// </summary>
        /// <param name="left">三维向量</param>
        /// <param name="right">三维向量</param>
        /// <returns>返回两个三维向量的和</returns>
        [Rtti.Meta]
        public static Vector3 Add( Vector3 left, Vector3 right )
	    {
            Vector3 result;
            result.X = left.X + right.X;
            result.Y = left.Y + right.Y;
            result.Z = left.Z + right.Z;
            return result;
	    }
        /// <summary>
        /// 两个三维向量的和
        /// </summary>
        /// <param name="left">三维向量</param>
        /// <param name="right">三维向量</param>
        /// <param name="result">两个三维向量的和</param>
        [Rtti.Meta]
        public static void Add(ref Vector3 left, ref Vector3 right, out Vector3 result)
	    {
            result.X = left.X + right.X;
            result.Y = left.Y + right.Y;
            result.Z = left.Z + right.Z;
        }
        /// <summary>
        /// 两个三维向量的差
        /// </summary>
        /// <param name="left">三维向量</param>
        /// <param name="right">三维向量</param>
        /// <returns>返回两个三维向量的差</returns>
        [Rtti.Meta]
        public static Vector3 Subtract(Vector3 left, Vector3 right)
	    {
            EngineNS.Vector3 result;
            result.X = left.X - right.X;
            result.Y = left.Y - right.Y;
            result.Z = left.Z - right.Z;
            return result;
	    }
        /// <summary>
        /// 两个三维向量的差
        /// </summary>
        /// <param name="left">三维向量</param>
        /// <param name="right">三维向量</param>
        /// <param name="result">两个三维向量的差</param>
        [Rtti.Meta]
        public static void Subtract(ref Vector3 left, ref Vector3 right, out Vector3 result)
	    {
            result.X = left.X - right.X;
            result.Y = left.Y - right.Y;
            result.Z = left.Z - right.Z;
        }
        /// <summary>
        /// 两个三维向量的积
        /// </summary>
        /// <param name="left">三维向量</param>
        /// <param name="right">三维向量</param>
        /// <returns>返回两个三维向量的积</returns>
        [Rtti.Meta]
        public static Vector3 Modulate(Vector3 left, Vector3 right)
	    {
            Vector3 result;
            result.X = left.X * right.X;
            result.Y = left.Y * right.Y;
            result.Z = left.Z * right.Z;
            return result;
	    }
        /// <summary>
        /// 两个三维向量的积
        /// </summary>
        /// <param name="left">三维向量</param>
        /// <param name="right">三维向量</param>
        /// <param name="result">两个三维向量的积</param>
        [Rtti.Meta]
        public static void Modulate(ref Vector3 left, ref Vector3 right, out Vector3 result)
	    {
            result.X = left.X * right.X;
            result.Y = left.Y * right.Y;
            result.Z = left.Z * right.Z;
        }
        /// <summary>
        /// 向量的数乘
        /// </summary>
        /// <param name="value">三维向量</param>
        /// <param name="scale">常数</param>
        /// <returns>返回计算结果</returns>
        [Rtti.Meta]
        public static Vector3 Multiply(Vector3 value, float scale)
	    {
            Vector3 result;
            result.X = value.X * scale;
            result.Y = value.Y * scale;
            result.Z = value.Z * scale;
            return result;
	    }
        /// <summary>
        /// 向量的数乘
        /// </summary>
        /// <param name="value">三维向量</param>
        /// <param name="scale">常数</param>
        /// <param name="result">计算结果</param>
        [Rtti.Meta]
        public static void Multiply(ref Vector3 value, float scale, out Vector3 result)
	    {
            result.X = value.X * scale;
            result.Y = value.Y * scale;
            result.Z = value.Z * scale;
        }
        /// <summary>
        /// 向量的除法
        /// </summary>
        /// <param name="value">三维向量</param>
        /// <param name="scale">常数</param>
        /// <returns>返回计算结果</returns>
        [Rtti.Meta]
        public static Vector3 Divide(Vector3 value, float scale)
	    {
            Vector3 result;
            result.X = value.X / scale;
            result.Y = value.Y / scale;
            result.Z = value.Z / scale;
            return result;
	    }
        /// <summary>
        /// 向量的除法
        /// </summary>
        /// <param name="value">三维向量</param>
        /// <param name="scale">常数</param>
        /// <param name="result">计算结果</param>
        [Rtti.Meta]
        public static void Divide(ref Vector3 value, float scale, out Vector3 result)
	    {
            result.X = value.X / scale;
            result.Y = value.Y / scale;
            result.Z = value.Z / scale;
        }
        /// <summary>
        /// 向量的取反
        /// </summary>
        /// <param name="value">三维向量</param>
        /// <returns>返回计算结果</returns>
        [Rtti.Meta]
        public static Vector3 Negate(Vector3 value)
	    {
            Vector3 result;
            result.X = -value.X;
            result.Y = -value.Y;
            result.Z = -value.Z;
            return result;
	    }
        /// <summary>
        /// 向量的取反
        /// </summary>
        /// <param name="value">三维向量</param>
        /// <param name="result">计算结果</param>
        [Rtti.Meta]
        public static void Negate(ref Vector3 value, out Vector3 result)
	    {
            result.X = -value.X;
            result.Y = -value.Y;
            result.Z = -value.Z;
        }
        /// <summary>
        /// 计算质心坐标
        /// </summary>
        /// <param name="value1">三维坐标点</param>
        /// <param name="value2">三维坐标点</param>
        /// <param name="value3">三维坐标点</param>
        /// <param name="amount1">参数</param>
        /// <param name="amount2">参数</param>
        /// <returns>返回计算结果</returns>
        [Rtti.Meta]
        public static Vector3 Barycentric(Vector3 value1, Vector3 value2, Vector3 value3, float amount1, float amount2)
	    {
		    Vector3 vector;
		    vector.X = (value1.X + (amount1 * (value2.X - value1.X))) + (amount2 * (value3.X - value1.X));
		    vector.Y = (value1.Y + (amount1 * (value2.Y - value1.Y))) + (amount2 * (value3.Y - value1.Y));
		    vector.Z = (value1.Z + (amount1 * (value2.Z - value1.Z))) + (amount2 * (value3.Z - value1.Z));
		    return vector;
	    }
        /// <summary>
        /// 计算质心坐标
        /// </summary>
        /// <param name="value1">三维坐标点</param>
        /// <param name="value2">三维坐标点</param>
        /// <param name="value3">三维坐标点</param>
        /// <param name="amount1">参数</param>
        /// <param name="amount2">参数</param>
        /// <param name="result">计算结果</param>
        [Rtti.Meta]
        public static void Barycentric(ref Vector3 value1, ref Vector3 value2, ref Vector3 value3, float amount1, float amount2, out Vector3 result)
	    {
            result.X = (value1.X + (amount1 * (value2.X - value1.X))) + (amount2 * (value3.X - value1.X));
            result.Y = (value1.Y + (amount1 * (value2.Y - value1.Y))) + (amount2 * (value3.Y - value1.Y));
            result.Z = (value1.Z + (amount1 * (value2.Z - value1.Z))) + (amount2 * (value3.Z - value1.Z));
	    }
        /// <summary>
        /// CatmullRom插值计算
        /// </summary>
        /// <param name="value1">三维坐标点</param>
        /// <param name="value2">三维坐标点</param>
        /// <param name="value3">三维坐标点</param>
        /// <param name="value4">三维坐标点</param>
        /// <param name="amount">参数因子</param>
        /// <returns>返回计算结果</returns>
        [Rtti.Meta]
        public static Vector3 CatmullRom(Vector3 value1, Vector3 value2, Vector3 value3, Vector3 value4, float amount)
	    {
		    Vector3 vector;
		    float squared = amount * amount;
		    float cubed = amount * squared;

		    vector.X = 0.5f * ((((2.0f * value2.X) + ((-value1.X + value3.X) * amount)) + 
			    (((((2.0f * value1.X) - (5.0f * value2.X)) + (4.0f * value3.X)) - value4.X) * squared)) + 
			    ((((-value1.X + (3.0f * value2.X)) - (3.0f * value3.X)) + value4.X) * cubed));

		    vector.Y = 0.5f * ((((2.0f * value2.Y) + ((-value1.Y + value3.Y) * amount)) + 
			    (((((2.0f * value1.Y) - (5.0f * value2.Y)) + (4.0f * value3.Y)) - value4.Y) * squared)) + 
			    ((((-value1.Y + (3.0f * value2.Y)) - (3.0f * value3.Y)) + value4.Y) * cubed));

		    vector.Z = 0.5f * ((((2.0f * value2.Z) + ((-value1.Z + value3.Z) * amount)) + 
			    (((((2.0f * value1.Z) - (5.0f * value2.Z)) + (4.0f * value3.Z)) - value4.Z) * squared)) + 
			    ((((-value1.Z + (3.0f * value2.Z)) - (3.0f * value3.Z)) + value4.Z) * cubed));

		    return vector;
	    }
        /// <summary>
        /// CatmullRom插值计算
        /// </summary>
        /// <param name="value1">三维坐标点</param>
        /// <param name="value2">三维坐标点</param>
        /// <param name="value3">三维坐标点</param>
        /// <param name="value4">三维坐标点</param>
        /// <param name="amount">参数因子</param>
        /// <param name="result">计算结果</param>
        [Rtti.Meta]
        public static void CatmullRom(ref Vector3 value1, ref Vector3 value2, ref Vector3 value3, ref Vector3 value4, float amount, out Vector3 result)
	    {
		    float squared = amount * amount;
		    float cubed = amount * squared;
		
		    Vector3 r;

		    r.X = 0.5f * ((((2.0f * value2.X) + ((-value1.X + value3.X) * amount)) + 
			    (((((2.0f * value1.X) - (5.0f * value2.X)) + (4.0f * value3.X)) - value4.X) * squared)) + 
			    ((((-value1.X + (3.0f * value2.X)) - (3.0f * value3.X)) + value4.X) * cubed));

		    r.Y = 0.5f * ((((2.0f * value2.Y) + ((-value1.Y + value3.Y) * amount)) + 
			    (((((2.0f * value1.Y) - (5.0f * value2.Y)) + (4.0f * value3.Y)) - value4.Y) * squared)) + 
			    ((((-value1.Y + (3.0f * value2.Y)) - (3.0f * value3.Y)) + value4.Y) * cubed));

		    r.Z = 0.5f * ((((2.0f * value2.Z) + ((-value1.Z + value3.Z) * amount)) + 
			    (((((2.0f * value1.Z) - (5.0f * value2.Z)) + (4.0f * value3.Z)) - value4.Z) * squared)) + 
			    ((((-value1.Z + (3.0f * value2.Z)) - (3.0f * value3.Z)) + value4.Z) * cubed));

		    result = r;
	    }
        /// <summary>
        /// 载体计算
        /// </summary>
        /// <param name="value">三维坐标点</param>
        /// <param name="min">三维坐标点的最小值</param>
        /// <param name="max">三维坐标点的最大值</param>
        /// <returns>返回计算结果</returns>
        [Rtti.Meta]
        public static Vector3 Clamp(Vector3 value, Vector3 min, Vector3 max)
	    {
		    float x = value.X;
		    x = (x > max.X) ? max.X : x;
		    x = (x < min.X) ? min.X : x;

		    float y = value.Y;
		    y = (y > max.Y) ? max.Y : y;
		    y = (y < min.Y) ? min.Y : y;

		    float z = value.Z;
		    z = (z > max.Z) ? max.Z : z;
		    z = (z < min.Z) ? min.Z : z;

            Vector3 result;
            result.X = x;
            result.Y = y;
            result.Z = z;
            return result;
	    }
        /// <summary>
        /// 载体计算
        /// </summary>
        /// <param name="value">三维坐标点</param>
        /// <param name="min">三维坐标点的最小值</param>
        /// <param name="max">三维坐标点的最大值</param>
        /// <param name="result">计算结果</param>
        [Rtti.Meta]
        public static void Clamp(ref Vector3 value, ref Vector3 min, ref Vector3 max, out Vector3 result)
	    {
		    float x = value.X;
		    x = (x > max.X) ? max.X : x;
		    x = (x < min.X) ? min.X : x;

		    float y = value.Y;
		    y = (y > max.Y) ? max.Y : y;
		    y = (y < min.Y) ? min.Y : y;

		    float z = value.Z;
		    z = (z > max.Z) ? max.Z : z;
		    z = (z < min.Z) ? min.Z : z;

            result.X = x;
            result.Y = y;
            result.Z = z;
        }
        /// <summary>
        /// 艾米插值计算
        /// </summary>
        /// <param name="value1">三维向量</param>
        /// <param name="tangent1">三维向量的正切点坐标</param>
        /// <param name="value2">三维向量</param>
        /// <param name="tangent2">三维向量的正切点坐标</param>
        /// <param name="amount">插值</param>
        /// <returns>返回计算后的向量</returns>
        [Rtti.Meta]
        public static Vector3 Hermite(Vector3 value1, Vector3 tangent1, Vector3 value2, Vector3 tangent2, float amount)
	    {
		    Vector3 vector;
		    float squared = amount * amount;
		    float cubed = amount * squared;
		    float part1 = ((2.0f * cubed) - (3.0f * squared)) + 1.0f;
		    float part2 = (-2.0f * cubed) + (3.0f * squared);
		    float part3 = (cubed - (2.0f * squared)) + amount;
		    float part4 = cubed - squared;

		    vector.X = (((value1.X * part1) + (value2.X * part2)) + (tangent1.X * part3)) + (tangent2.X * part4);
		    vector.Y = (((value1.Y * part1) + (value2.Y * part2)) + (tangent1.Y * part3)) + (tangent2.Y * part4);
		    vector.Z = (((value1.Z * part1) + (value2.Z * part2)) + (tangent1.Z * part3)) + (tangent2.Z * part4);

		    return vector;
	    }
        /// <summary>
        /// 艾米插值计算
        /// </summary>
        /// <param name="value1">三维向量</param>
        /// <param name="tangent1">三维向量的正切点坐标</param>
        /// <param name="value2">三维向量</param>
        /// <param name="tangent2">三维向量的正切点坐标</param>
        /// <param name="amount">插值</param>
        /// <param name="result">计算后的向量</param>
        [Rtti.Meta]
        public static void Hermite(ref Vector3 value1, ref Vector3 tangent1, ref Vector3 value2, ref Vector3 tangent2, float amount, out Vector3 result)
	    {
		    float squared = amount * amount;
		    float cubed = amount * squared;
		    float part1 = ((2.0f * cubed) - (3.0f * squared)) + 1.0f;
		    float part2 = (-2.0f * cubed) + (3.0f * squared);
		    float part3 = (cubed - (2.0f * squared)) + amount;
		    float part4 = cubed - squared;

		    result.X = (((value1.X * part1) + (value2.X * part2)) + (tangent1.X * part3)) + (tangent2.X * part4);
		    result.Y = (((value1.Y * part1) + (value2.Y * part2)) + (tangent1.Y * part3)) + (tangent2.Y * part4);
		    result.Z = (((value1.Z * part1) + (value2.Z * part2)) + (tangent1.Z * part3)) + (tangent2.Z * part4);
	    }
        /// <summary>
        /// 计算线性插值
        /// </summary>
        /// <param name="start">起点坐标</param>
        /// <param name="end">终点坐标</param>
        /// <param name="factor">插值因子</param>
        /// <returns>返回计算后的向量</returns>
        [Rtti.Meta]
        public static Vector3 Lerp(Vector3 start, Vector3 end, float factor)
	    {
		    Vector3 vector;

		    vector.X = start.X + ((end.X - start.X) * factor);
		    vector.Y = start.Y + ((end.Y - start.Y) * factor);
		    vector.Z = start.Z + ((end.Z - start.Z) * factor);

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
        public static void Lerp(ref Vector3 start, ref Vector3 end, float factor, out Vector3 result)
	    {
		    result.X = start.X + ((end.X - start.X) * factor);
		    result.Y = start.Y + ((end.Y - start.Y) * factor);
		    result.Z = start.Z + ((end.Z - start.Z) * factor);
	    }
        /// <summary>
        /// 平滑插值计算
        /// </summary>
        /// <param name="start">起点坐标</param>
        /// <param name="end">终点坐标</param>
        /// <param name="amount">插值因子</param>
        /// <returns>返回计算后的向量</returns>
        [Rtti.Meta]
        public static Vector3 SmoothStep(Vector3 start, Vector3 end, float amount)
	    {
		    Vector3 vector;

		    amount = (amount > 1.0f) ? 1.0f : ((amount < 0.0f) ? 0.0f : amount);
		    amount = (amount * amount) * (3.0f - (2.0f * amount));

		    vector.X = start.X + ((end.X - start.X) * amount);
		    vector.Y = start.Y + ((end.Y - start.Y) * amount);
		    vector.Z = start.Z + ((end.Z - start.Z) * amount);

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
        public static void SmoothStep(ref Vector3 start, ref Vector3 end, float amount, out Vector3 result)
	    {
		    amount = (amount > 1.0f) ? 1.0f : ((amount < 0.0f) ? 0.0f : amount);
		    amount = (amount * amount) * (3.0f - (2.0f * amount));

		    result.X = start.X + ((end.X - start.X) * amount);
		    result.Y = start.Y + ((end.Y - start.Y) * amount);
		    result.Z = start.Z + ((end.Z - start.Z) * amount);
	    }
        [Rtti.Meta]
        public static Vector3 Slerp(Vector3 start, Vector3 end, float factor)
        {
            float lhsMag = start.Length();
            float rhsMag = end.Length();

            if (lhsMag < MathHelper.Epsilon|| rhsMag < MathHelper.Epsilon)
                return Lerp(start, end, factor);

            float lerpedMagnitude = MathHelper.FloatLerp(lhsMag, rhsMag, factor);

            float dot = Dot(start, end) / (lhsMag * rhsMag);
            // direction is almost the same
            if (dot > 1.0F - MathHelper.Epsilon)
            {
                return Lerp(start, end, factor);
            }
            // directions are almost opposite
            else if (dot < -1.0F + MathHelper.Epsilon)
            {
                Vector3 lhsNorm = start / lhsMag;
                Vector3 axis = OrthoNormalVectorFast(lhsNorm);
                Matrix3x3 m = Matrix3x3.RotationAxis(axis, MathHelper.PI * factor);
                
                Vector3 slerped = lhsNorm* m;
                slerped *= lerpedMagnitude;
                return slerped;
            }
            // normal case
            else
            {
                Vector3 axis = Cross(start, end);
                Vector3 lhsNorm = start / lhsMag;
                axis = Normalize(axis);
                float angle =  MathHelper.Acos(dot) * factor;

                Matrix3x3 m = Matrix3x3.RotationAxis(axis, angle);
                Vector3 slerped = lhsNorm * m;
                slerped *= lerpedMagnitude;
                return slerped;
            }
        }
        const float k1OverSqrt2 = 0.7071067811865475244008443621048490f;

        public static Vector3 OrthoNormalVectorFast(Vector3 n)
        {
            Vector3 res = Vector3.Zero;
            if (Math.Abs(n.Z) > k1OverSqrt2)
            {
                // choose p in y-z plane
                float a = n.Y * n.Y + n.Z * n.Z;
                float k = 1.0f / (float)Math.Sqrt(a);
                res.X = 0;
                res.Y = -n.Z* k;
                res.Z = n.Y* k;
            }
            else
            {
                // choose p in x-y plane
                float a = n.X * n.X + n.Y * n.Y;
            float k = 1.0F / (float)Math.Sqrt(a);
            res.X= -n.Y* k;
            res.Y = n.X* k;
            res.Z = 0;
            }
            return res;
        }
        /// <summary>
        /// 计算两点间的距离
        /// </summary>
        /// <param name="value1">坐标点</param>
        /// <param name="value2">坐标点</param>
        /// <returns>返回两点间的距离</returns>
        [Rtti.Meta]
        public static float Distance(ref Vector3 value1, ref Vector3 value2)
	    {
		    float x = value1.X - value2.X;
		    float y = value1.Y - value2.Y;
		    float z = value1.Z - value2.Z;

		    return (float)( Math.Sqrt( (x * x) + (y * y) + (z * z) ) );
	    }
        [Rtti.Meta]
        public static float DistanceSquared(Vector3 value1, Vector3 value2)
        {
            return DistanceSquared(ref value1, ref value2);
        }
        /// <summary>
        /// 计算两点间的距离的平方
        /// </summary>
        /// <param name="value1">坐标点</param>
        /// <param name="value2">坐标点</param>
        /// <returns>返回两点间的距离的平方</returns>
        [Rtti.Meta]
        public static float DistanceSquared(ref Vector3 value1, ref Vector3 value2)
	    {
		    float x = value1.X - value2.X;
		    float y = value1.Y - value2.Y;
		    float z = value1.Z - value2.Z;

		    return (x * x) + (y * y) + (z * z);
	    }
        /// <summary>
        /// 向量的点积
        /// </summary>
        /// <param name="left">三维向量</param>
        /// <param name="right">三维向量</param>
        /// <returns>返回点积值</returns>
        [Rtti.Meta]
        public static float Dot(Vector3 left, Vector3 right)
	    {
		    return (left.X * right.X + left.Y * right.Y + left.Z * right.Z);
	    }
        public static void Dot(ref Vector3 left, ref Vector3 right, out float num)
        {
            num = (left.X * right.X + left.Y * right.Y + left.Z * right.Z);
        }
        /// <summary>
        /// 向量的叉积
        /// </summary>
        /// <param name="left">三维向量</param>
        /// <param name="right">三维向量</param>
        /// <returns>返回叉积值</returns>
        [Rtti.Meta]
        public static Vector3 Cross(Vector3 left, Vector3 right)
	    {
		    Vector3 result;
		    result.X = left.Y * right.Z - left.Z * right.Y;
		    result.Y = left.Z * right.X - left.X * right.Z;
		    result.Z = left.X * right.Y - left.Y * right.X;
		    return result;
	    }
        /// <summary>
        /// 向量的叉积
        /// </summary>
        /// <param name="left">三维向量</param>
        /// <param name="right">三维向量</param>
        /// <param name="result">叉积结果</param>
        [Rtti.Meta]
        public static void Cross(ref Vector3 left, ref Vector3 right, out Vector3 result)
	    {
		    Vector3 r;
		    r.X = left.Y * right.Z - left.Z * right.Y;
		    r.Y = left.Z * right.X - left.X * right.Z;
		    r.Z = left.X * right.Y - left.Y * right.X; 

		    result = r;
	    }
        /// <summary>
        /// 向量的投影
        /// </summary>
        /// <param name="vector">三维向量</param>
        /// <param name="normal">投影到的单位向量</param>
        /// <returns>返回投影向量</returns>
        [Rtti.Meta]
        public static Vector3 Reflect(Vector3 vector, Vector3 normal)
	    {
		    Vector3 result;
		    float dot = ((vector.X * normal.X) + (vector.Y * normal.Y)) + (vector.Z * normal.Z);

		    result.X = vector.X - ((2.0f * dot) * normal.X);
		    result.Y = vector.Y - ((2.0f * dot) * normal.Y);
		    result.Z = vector.Z - ((2.0f * dot) * normal.Z);

		    return result;
	    }
        public static Vector3 CalcFaceNormal(ref Vector3 a, ref Vector3 b, ref Vector3 c)
        {
            var t1 = a - c;
            var t2 = b - c;
            Vector3 result;
            Cross(ref t1, ref t2, out result);
            result.Normalize();
            return result;
        }
        public static float CalcArea3(ref Vector3 a, ref Vector3 b, ref Vector3 c)
	    {
            //此处是向量叉积的几何意义的应用
            //没处以2，所以出来的是平行四边形面积，并且有正负数的问题，
            //正数说明夹角是负角度
            //计算面积，外面要用abs * 0.5
            Vector3 v1 = b - a;
            Vector3 v2 = c - a;
            return ((v1.Y * v2.Z + v1.Z * v2.X + v1.X * v2.Y) -
                    (v1.Y * v2.X + v1.X * v2.Z + v1.X * v2.Y));
        }
        /// <summary>
        /// 向量的投影
        /// </summary>
        /// <param name="vector">三维向量</param>
        /// <param name="normal">投影到的单位向量</param>
        /// <param name="result">投影向量</param>
        [Rtti.Meta]
        public static void Reflect(ref Vector3 vector, ref Vector3 normal, out Vector3 result)
	    {
		    float dot = ((vector.X * normal.X) + (vector.Y * normal.Y)) + (vector.Z * normal.Z);

		    result.X = vector.X - ((2.0f * dot) * normal.X);
		    result.Y = vector.Y - ((2.0f * dot) * normal.Y);
		    result.Z = vector.Z - ((2.0f * dot) * normal.Z);
	    }
        /// <summary>
        /// 向量的单位化
        /// </summary>
        /// <param name="vector">三维向量</param>
        /// <returns>返回单位向量</returns>
        [Rtti.Meta]
        public static Vector3 Normalize(Vector3 vector)
	    {
		    vector.Normalize();
		    return vector;
	    }
        /// <summary>
        /// 向量的单位化
        /// </summary>
        /// <param name="vector">三维向量</param>
        /// <param name="result">单位向量</param>
        [Rtti.Meta]
        public static void Normalize(ref Vector3 vector, out Vector3 result)
	    {
		    result = vector;
		    result.Normalize();
	    }
        [Rtti.Meta]
        public static Vector4 Transform(Vector3 vector, Matrix transform)
        {
            Vector4 result;

            result.X = (((vector.X * transform.M11) + (vector.Y * transform.M21)) + (vector.Z * transform.M31)) + transform.M41;
            result.Y = (((vector.X * transform.M12) + (vector.Y * transform.M22)) + (vector.Z * transform.M32)) + transform.M42;
            result.Z = (((vector.X * transform.M13) + (vector.Y * transform.M23)) + (vector.Z * transform.M33)) + transform.M43;
            result.W = (((vector.X * transform.M14) + (vector.Y * transform.M24)) + (vector.Z * transform.M34)) + transform.M44;

            return result;
        }
        /// <summary>
        /// 三维向量的坐标转换运算
        /// </summary>
        /// <param name="vector">三维向量</param>
        /// <param name="transform">转换矩阵</param>
        /// <param name="result">转换后的向量</param>
        [Rtti.Meta]
        public static void Transform(ref Vector3 vector, ref Matrix transform, out Vector4 result)
        {
            result.X = (((vector.X * transform.M11) + (vector.Y * transform.M21)) + (vector.Z * transform.M31)) + transform.M41;
            result.Y = (((vector.X * transform.M12) + (vector.Y * transform.M22)) + (vector.Z * transform.M32)) + transform.M42;
            result.Z = (((vector.X * transform.M13) + (vector.Y * transform.M23)) + (vector.Z * transform.M33)) + transform.M43;
            result.W = (((vector.X * transform.M14) + (vector.Y * transform.M24)) + (vector.Z * transform.M34)) + transform.M44;
        }
        /// <summary>
        /// 三维向量的坐标转换运算
        /// </summary>
        /// <param name="vectorsIn">需要转换的三维向量指针</param>
        /// <param name="inputStride">步数</param>
        /// <param name="transformation">转换矩阵指针</param>
        /// <param name="vectorsOut">计算后的向量</param>
        /// <param name="outputStride">输出的步数</param>
        /// <param name="count">次数</param>
        [Rtti.Meta]
        public static void Transform(IntPtr vectorsIn, int inputStride, IntPtr transformation, IntPtr vectorsOut, int outputStride, int count)
        {
            IDllImportApi.v3dxVec3TransformArray((vectorsOut), (UInt32)outputStride,
                (vectorsIn), (UInt32)inputStride,
                (transformation), (UInt32)count);
        }
        /// <summary>
        /// 三维向量的坐标转换运算
        /// </summary>
        /// <param name="vectorsIn">需要转换的三维向量指针</param>
        /// <param name="transformation">转换矩阵指针</param>
        /// <param name="vectorsOut">计算后的向量</param>
        /// <param name="count">次数</param>
        [Rtti.Meta]
        public static void Transform(IntPtr vectorsIn, IntPtr transformation, IntPtr vectorsOut, int count) 
        { 
            Transform(vectorsIn, sizeof(float)*3, transformation, vectorsOut, sizeof(float) * 4, count); 
        }
        /// <summary>
        /// 三维向量的坐标转换运算
        /// </summary>
        /// <param name="vectorsIn">需要转换的三维向量列表</param>
        /// <param name="transformation">转换矩阵</param>
        /// <param name="vectorsOut">计算后的向量列表</param>
        /// <param name="offset">偏移值</param>
        /// <param name="count">次数</param>
        public static void Transform(Vector3[] vectorsIn, ref Matrix transformation, Vector4[] vectorsOut, int offset, int count)
        {
            if(vectorsIn.Length != vectorsOut.Length)
                throw new ArgumentException( "Input and output arrays must be the same size.", "vectorsOut" );

            var pinnedIn = System.Runtime.InteropServices.Marshal.UnsafeAddrOfPinnedArrayElement(vectorsIn, offset);
            var pinnedOut = System.Runtime.InteropServices.Marshal.UnsafeAddrOfPinnedArrayElement(vectorsOut, offset);
            unsafe
            {   
                fixed (Matrix* pinnedMatrix = &transformation)
                {
                    Transform((IntPtr)pinnedIn, (IntPtr)pinnedMatrix, (IntPtr)pinnedOut, count);
                }
            }
        }
        /// <summary>
        /// 三维向量的坐标转换运算
        /// </summary>
        /// <param name="vectorsIn">需要转换的三维向量列表</param>
        /// <param name="transformation">转换矩阵</param>
        /// <param name="vectorsOut">计算后的向量列表</param>
        public static void Transform( Vector3[] vectorsIn, ref Matrix transformation, Vector4[] vectorsOut ) 
        { 
            Transform( vectorsIn, ref transformation, vectorsOut, 0, 0 ); 
        }
        /// <summary>
        /// 三维向量的坐标转换运算
        /// </summary>
        /// <param name="vectors">需要转换的三维向量列表</param>
        /// <param name="transform">转换矩阵</param>
        /// <returns>返回计算后的向量列表</returns>
        public static Vector4[] Transform(Vector3[] vectors, ref Matrix transform)
        {
            int count = vectors.Length;
            Vector4[] results = new Vector4[ count ];

            /*for( int i = 0; i < count; i++ )
            {
                Vector4 r;
                r.X = (((vectors[i].X * transform.M11) + (vectors[i].Y * transform.M21)) + (vectors[i].Z * transform.M31)) + transform.M41;
                r.Y = (((vectors[i].X * transform.M12) + (vectors[i].Y * transform.M22)) + (vectors[i].Z * transform.M32)) + transform.M42;
                r.Z = (((vectors[i].X * transform.M13) + (vectors[i].Y * transform.M23)) + (vectors[i].Z * transform.M33)) + transform.M43;
                r.W = (((vectors[i].X * transform.M14) + (vectors[i].Y * transform.M24)) + (vectors[i].Z * transform.M34)) + transform.M44;
		
                results[i] = r;
            }*/
            Transform( vectors, ref transform, results );
            return results;
        }
        /// <summary>
        /// 三维向量的坐标转换运算
        /// </summary>
        /// <param name="value">需要转换的三维向量</param>
        /// <param name="rotation">旋转四元数</param>
        /// <returns>返回计算后的向量</returns>
        [Rtti.Meta]
        public static Vector4 Transform(Vector3 value, Quaternion rotation)
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

            vector.X = ((value.X * ((1.0f - yy) - zz)) + (value.Y * (xy - wz))) + (value.Z * (xz + wy));
            vector.Y = ((value.X * (xy + wz)) + (value.Y * ((1.0f - xx) - zz))) + (value.Z * (yz - wx));
            vector.Z = ((value.X * (xz - wy)) + (value.Y * (yz + wx))) + (value.Z * ((1.0f - xx) - yy));
            vector.W = 1.0f;

            return vector;
        }
        /// <summary>
        /// 三维向量的坐标转换运算
        /// </summary>
        /// <param name="value">需要转换的三维向量</param>
        /// <param name="rotation">旋转四元数</param>
        /// <param name="result">计算后的向量</param>
        [Rtti.Meta]
        public static void Transform(ref Vector3 value, ref Quaternion rotation, out Vector4 result)
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

            result.X = ((value.X * ((1.0f - yy) - zz)) + (value.Y * (xy - wz))) + (value.Z * (xz + wy));
            result.Y = ((value.X * (xy + wz)) + (value.Y * ((1.0f - xx) - zz))) + (value.Z * (yz - wx));
            result.Z = ((value.X * (xz - wy)) + (value.Y * (yz + wx))) + (value.Z * ((1.0f - xx) - yy));
            result.W = 1.0f;
        }
        /// <summary>
        /// 三维向量的坐标转换运算
        /// </summary>
        /// <param name="vectors">需要转换的三维向量列表</param>
        /// <param name="rotation">旋转四元数</param>
        /// <returns>返回计算后的向量列表</returns>
        public static Vector4[] Transform(Vector3[] vectors, ref Quaternion rotation)
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
                r.X = ((vectors[i].X * ((1.0f - yy) - zz)) + (vectors[i].Y * (xy - wz))) + (vectors[i].Z * (xz + wy));
                r.Y = ((vectors[i].X * (xy + wz)) + (vectors[i].Y * ((1.0f - xx) - zz))) + (vectors[i].Z * (yz - wx));
                r.Z = ((vectors[i].X * (xz - wy)) + (vectors[i].Y * (yz + wx))) + (vectors[i].Z * ((1.0f - xx) - yy));
                r.W = 1.0f;

                results[i] = r;
            }

            return results;
        }
        /// <summary>
        /// 坐标轴变换
        /// </summary>
        /// <param name="coord">坐标轴对象</param>
        /// <param name="transform">转换矩阵</param>
        /// <returns>返回计算后的向量</returns>
        [Rtti.Meta]
        public static Vector3 TransformCoordinate(Vector3 coord, Matrix transform)
        {
            Vector4 vector;

            vector.X = (((coord.X * transform.M11) + (coord.Y * transform.M21)) + (coord.Z * transform.M31)) + transform.M41;
            vector.Y = (((coord.X * transform.M12) + (coord.Y * transform.M22)) + (coord.Z * transform.M32)) + transform.M42;
            vector.Z = (((coord.X * transform.M13) + (coord.Y * transform.M23)) + (coord.Z * transform.M33)) + transform.M43;
            vector.W = 1 / ((((coord.X * transform.M14) + (coord.Y * transform.M24)) + (coord.Z * transform.M34)) + transform.M44);

            Vector3 result;
            result.X = vector.X * vector.W;
            result.Y = vector.Y * vector.W;
            result.Z = vector.Z* vector.W;
            return result;
        }
        /// <summary>
        /// 坐标轴变换
        /// </summary>
        /// <param name="coord">坐标轴对象</param>
        /// <param name="transform">转换矩阵</param>
        /// <param name="result">计算后的向量</param>
        [Rtti.Meta]
        public static float TransformCoordinate(ref Vector3 coord, ref Matrix transform, out Vector3 result)
        {
            Vector4 vector;

            vector.X = (((coord.X * transform.M11) + (coord.Y * transform.M21)) + (coord.Z * transform.M31)) + transform.M41;
            vector.Y = (((coord.X * transform.M12) + (coord.Y * transform.M22)) + (coord.Z * transform.M32)) + transform.M42;
            vector.Z = (((coord.X * transform.M13) + (coord.Y * transform.M23)) + (coord.Z * transform.M33)) + transform.M43;
            vector.W = 1 / ((((coord.X * transform.M14) + (coord.Y * transform.M24)) + (coord.Z * transform.M34)) + transform.M44);

            result.X = vector.X * vector.W;
            result.Y = vector.Y * vector.W;
            result.Z = vector.Z * vector.W;
            return vector.W;
        }
        /// <summary>
        /// 坐标轴变换
        /// </summary>
        /// <param name="value">三维向量</param>
        /// <param name="rotation">旋转四元数</param>
        /// <returns>返回计算后的向量</returns>
        [Rtti.Meta]
        public static Vector3 TransformCoordinate(Vector3 value, Quaternion rotation)
        {
            var v4 = Transform(value, rotation);
            Vector3 result;
            result.X = v4.X;
            result.Y = v4.Y;
            result.Z = v4.Z;
            return result;
        }
        /// <summary>
        /// 坐标轴变换
        /// </summary>
        /// <param name="coordsIn">坐标轴指针</param>
        /// <param name="inputStride">输入的步数</param>
        /// <param name="transformation">转换矩阵指针</param>
        /// <param name="coordsOut">输出的坐标轴指针</param>
        /// <param name="outputStride">输出的步数</param>
        /// <param name="count">计数</param>
        public unsafe static void TransformCoordinate( Vector3* coordsIn, int inputStride, Matrix* transformation, Vector3* coordsOut, int outputStride, int count )
        {
            IDllImportApi.v3dxVec3TransformCoordArray( coordsOut , (UInt32)outputStride,
                    coordsIn, (UInt32)inputStride,
                    transformation, (UInt32)count);
        }
        /// <summary>
        /// 坐标轴变换
        /// </summary>
        /// <param name="coordinatesIn">坐标轴指针</param>
        /// <param name="transformation">转换矩阵指针</param>
        /// <param name="coordinatesOut">输出的坐标轴指针</param>
        /// <param name="count">计数</param>
        public unsafe static void TransformCoordinate(Vector3* coordinatesIn, Matrix* transformation, Vector3* coordinatesOut, int count) 
        { 
            TransformCoordinate(coordinatesIn, (int)sizeof(Vector3), transformation, coordinatesOut, (int)sizeof(Vector3), count); 
        }
        /// <summary>
        /// 坐标轴变换
        /// </summary>
        /// <param name="coordsIn">坐标轴指针</param>
        /// <param name="transformation">转换矩阵指针</param>
        /// <param name="coordsOut">输出的坐标轴指针</param>
        /// <param name="offset">偏移值</param>
        /// <param name="count">计数</param>
        public static void TransformCoordinate( Vector3[] coordsIn, ref Matrix transformation, Vector3[] coordsOut, int offset, int count )
        {
            if(coordsIn.Length != coordsOut.Length)
                throw new ArgumentException( "Input and output arrays must be the same size.", "coordinatesOut" );
            //Utilities::CheckArrayBounds( coordsIn, offset, count );

            unsafe
            {
                fixed (Vector3* pinnedIn = &coordsIn[offset])
                {
                    fixed (Matrix* pinnedMatrix = &transformation)
                    {
                        fixed (Vector3* pinnedOut = &coordsOut[offset])
                        {
                            TransformCoordinate(pinnedIn, pinnedMatrix, pinnedOut, count);
                        }
                    }
                }
            }
        }
        /// <summary>
        /// 坐标轴变换
        /// </summary>
        /// <param name="coordinatesIn">坐标轴列表</param>
        /// <param name="transformation">转换矩阵</param>
        /// <param name="coordinatesOut">输出的坐标轴列表</param>
        public static void TransformCoordinate(Vector3[] coordinatesIn, ref Matrix transformation, Vector3[] coordinatesOut) 
        { 
            TransformCoordinate(coordinatesIn, ref transformation, coordinatesOut, 0, 0); 
        }
        /// <summary>
        /// 坐标轴变换
        /// </summary>
        /// <param name="coords">坐标轴列表</param>
        /// <param name="transform">转换矩阵</param>
        /// <returns>返回转换后的坐标轴列表</returns>
        public static Vector3[] TransformCoordinate(Vector3[] coords, ref Matrix transform)
        {
            if( coords == null )
                throw new ArgumentNullException( "coordinates" );

            //Vector4 vector = new Vector4();
            int count = coords.Length;
            Vector3[] results = new Vector3[ count ];

            /*for( int i = 0; i < count; i++ )
            {
                vector.X = (((coords[i].X * transform.M11) + (coords[i].Y * transform.M21)) + (coords[i].Z * transform.M31)) + transform.M41;
                vector.Y = (((coords[i].X * transform.M12) + (coords[i].Y * transform.M22)) + (coords[i].Z * transform.M32)) + transform.M42;
                vector.Z = (((coords[i].X * transform.M13) + (coords[i].Y * transform.M23)) + (coords[i].Z * transform.M33)) + transform.M43;
                vector.W = 1 / ((((coords[i].X * transform.M14) + (coords[i].Y * transform.M24)) + (coords[i].Z * transform.M34)) + transform.M44);
                results[i] = Vector3( vector.X * vector.W, vector.Y * vector.W, vector.Z * vector.W );
            }*/
            TransformCoordinate( coords, ref transform, results );
            return results;
        }
        /// <summary>
        /// 单位向量的坐标变换
        /// </summary>
        /// <param name="normal">单位向量</param>
        /// <param name="transform">转换矩阵</param>
        /// <returns>返回转换后的向量</returns>
        [Rtti.Meta]
        public static Vector3 TransformNormal(Vector3 normal, Matrix transform)
        {
            Vector3 vector;

            vector.X = ((normal.X * transform.M11) + (normal.Y * transform.M21)) + (normal.Z * transform.M31);
            vector.Y = ((normal.X * transform.M12) + (normal.Y * transform.M22)) + (normal.Z * transform.M32);
            vector.Z = ((normal.X * transform.M13) + (normal.Y * transform.M23)) + (normal.Z * transform.M33);

            return vector;
        }
        [Rtti.Meta]
        public static Vector3 TransposeTransformNormal(Vector3 normal, Matrix transform)
        {
            Vector3 vector;

            vector.X = ((normal.X * transform.M11) + (normal.Y * transform.M12)) + (normal.Z * transform.M13);
            vector.Y = ((normal.X * transform.M21) + (normal.Y * transform.M22)) + (normal.Z * transform.M23);
            vector.Z = ((normal.X * transform.M31) + (normal.Y * transform.M32)) + (normal.Z * transform.M33);

            return vector;
        }
        /// <summary>
        /// 单位向量的坐标变换
        /// </summary>
        /// <param name="normal">单位向量</param>
        /// <param name="transform">转换矩阵</param>
        /// <param name="result">转换后的向量</param>
        [Rtti.Meta]
        public static void TransformNormal(ref Vector3 normal, ref Matrix transform, out Vector3 result)
        {
            result.X = ((normal.X * transform.M11) + (normal.Y * transform.M21)) + (normal.Z * transform.M31);
            result.Y = ((normal.X * transform.M12) + (normal.Y * transform.M22)) + (normal.Z * transform.M32);
            result.Z = ((normal.X * transform.M13) + (normal.Y * transform.M23)) + (normal.Z * transform.M33);
        }
        /// <summary>
        /// 单位向量的坐标变换
        /// </summary>
        /// <param name="normalsIn">需要转换的单位向量指针</param>
        /// <param name="inputStride">输入步数</param>
        /// <param name="transformation">转换矩阵指针</param>
        /// <param name="normalsOut">转换后的单位向量指针</param>
        /// <param name="outputStride">输出的步数</param>
        /// <param name="count">计数</param>
        public unsafe static void TransformNormal( Vector3* normalsIn, int inputStride, Matrix* transformation, Vector3* normalsOut, int outputStride, int count )
        {
            IDllImportApi.v3dxVec3TransformNormalArray( normalsOut , (UInt32)outputStride,
                normalsIn, (UInt32)inputStride,
                transformation, (UInt32)count);
        }
        /// <summary>
        /// 单位向量的坐标变换
        /// </summary>
        /// <param name="normalsIn">需要转换的单位向量指针</param>
        /// <param name="transformation">转换矩阵指针</param>
        /// <param name="normalsOut">转换后的单位向量指针</param>
        /// <param name="count">计数</param>
        public unsafe static void TransformNormal(Vector3* normalsIn, Matrix* transformation, Vector3* normalsOut, int count) 
        {
            TransformNormal(normalsIn, (int)sizeof(Vector3), transformation, normalsOut, (int)sizeof(Vector3), count); 
        }
        /// <summary>
        /// 单位向量的坐标变换
        /// </summary>
        /// <param name="normalsIn">需要转换的单位向量列表</param>
        /// <param name="transformation">转换矩阵</param>
        /// <param name="normalsOut">转换后的单位向量列表</param>
        public static void TransformNormal(Vector3[] normalsIn, ref Matrix transformation, Vector3[] normalsOut) 
        { 
            TransformNormal(normalsIn, ref transformation, normalsOut, 0, 0); 
        }
        /// <summary>
        /// 单位向量的坐标变换
        /// </summary>
        /// <param name="normalsIn">需要转换的单位向量列表</param>
        /// <param name="transformation">转换矩阵</param>
        /// <param name="normalsOut">转换后的单位向量列表</param>
        /// <param name="offset">偏移值</param>
        /// <param name="count">计数</param>
        public static void TransformNormal(Vector3[] normalsIn, ref Matrix transformation, Vector3[] normalsOut, int offset, int count)
        {
            if(normalsIn.Length != normalsOut.Length)
                throw new ArgumentException( "Input and output arrays must be the same size.", "normalsOut" );
            //Utilities::CheckArrayBounds( normalsOut, offset, count );

            unsafe
            {
                fixed (Vector3* pinnedIn = &normalsIn[offset])
                {
                    fixed (Matrix* pinnedMatrix = &transformation)
                    {
                        fixed (Vector3* pinnedOut = &normalsOut[offset])
                        {
                            TransformNormal(pinnedIn, pinnedMatrix, pinnedOut, count);
                        }
                    }
                }
            }
        }
        /// <summary>
        /// 单位向量的坐标变换
        /// </summary>
        /// <param name="normals">需要转换的单位向量列表</param>
        /// <param name="transform">转换矩阵</param>
        /// <returns>返回转换后的单位向量列表</returns>
        public static Vector3[] TransformNormal(Vector3[] normals, ref Matrix transform)
        {
            if( normals == null )
                throw new ArgumentNullException( "normals" );

            int count = normals.Length;
            Vector3[] results = new Vector3[count];

            /*for( int i = 0; i < count; i++ )
            {
                Vector3 r;
                r.X = ((normals[i].X * transform.M11) + (normals[i].Y * transform.M21)) + (normals[i].Z * transform.M31);
                r.Y = ((normals[i].X * transform.M12) + (normals[i].Y * transform.M22)) + (normals[i].Z * transform.M32);
                r.Z = ((normals[i].X * transform.M13) + (normals[i].Y * transform.M23)) + (normals[i].Z * transform.M33);
		
                results[i] = r;
            }*/
            TransformNormal( normals, ref transform, results );
            return results;
        }
        /// <summary>
        /// 投影,将一个坐标从视图空间投影到投影空间中
        /// </summary>
        /// <param name="vector">坐标点</param>
        /// <param name="x">屏幕坐标的X值</param>
        /// <param name="y">屏幕坐标的Y值</param>
        /// <param name="width">宽</param>
        /// <param name="height">高</param>
        /// <param name="minZ">最小的Z值</param>
        /// <param name="maxZ">最大的Z值</param>
        /// <param name="worldViewProjection">世界坐标下的转换矩阵</param>
        /// <returns>返回转换后的三维坐标</returns>
        [Rtti.Meta]
        public static Vector3 Project(Vector3 vector, float x, float y, float width, float height, float minZ, float maxZ, Matrix worldViewProjection)
        {
            TransformCoordinate( ref vector, ref worldViewProjection, out vector );
            Vector3 result;
            result.X = ((1.0f + vector.X) * 0.5f * width) + x;
            result.Y = ((1.0f - vector.Y) * 0.5f * height) + y;
            result.Z = (vector.Z * (maxZ - minZ)) + minZ;
            return result;
        }
        /// <summary>
        /// 投影,将一个坐标从视图空间投影到投影空间中
        /// </summary>
        /// <param name="vector">坐标点</param>
        /// <param name="x">屏幕坐标的X值</param>
        /// <param name="y">屏幕坐标的Y值</param>
        /// <param name="width">宽</param>
        /// <param name="height">高</param>
        /// <param name="minZ">最小的Z值</param>
        /// <param name="maxZ">最大的Z值</param>
        /// <param name="worldViewProjection">世界坐标下的转换矩阵</param>
        /// <param name="result">转换后的三维坐标</param>
        [Rtti.Meta]
        public static void Project(ref Vector3 vector, float x, float y, float width, float height, float minZ, float maxZ, ref Matrix worldViewProjection, out Vector3 result)
        {
            Vector3 v;
            TransformCoordinate( ref vector, ref worldViewProjection, out v );
            
            result.X = ((1.0f + vector.X) * 0.5f * width) + x;
            result.Y = ((1.0f - vector.Y) * 0.5f * height) + y;
            result.Z = (vector.Z * (maxZ - minZ)) + minZ;
            //result = new Vector3( ( ( 1.0f + v.X ) * 0.5f * width ) + x, ( ( 1.0f - v.Y ) * 0.5f * height ) + y, ( v.Z * ( maxZ - minZ ) ) + minZ );
        }
        /// <summary>
        /// 反投影,将一个坐标从投影空间中反投影到视图空间
        /// </summary>
        /// <param name="vector">坐标点</param>
        /// <param name="x">屏幕坐标的X值</param>
        /// <param name="y">屏幕坐标的Y值</param>
        /// <param name="width">宽</param>
        /// <param name="height">高</param>
        /// <param name="minZ">最小的Z值</param>
        /// <param name="maxZ">最大的Z值</param>
        /// <param name="worldViewProjection">世界坐标下的转换矩阵</param>
        /// <returns>返回转换后的三维坐标</returns>
        [Rtti.Meta]
        public static Vector3 Unproject( Vector3 vector, float x, float y, float width, float height, float minZ, float maxZ, Matrix worldViewProjection )
        {
            Vector3 v;
            Matrix matrix;
            Matrix.Invert( ref worldViewProjection, out matrix );

            v.X = ( ( ( vector.X - x ) / width ) * 2.0f ) - 1.0f;
            v.Y = -( ( ( ( vector.Y - y ) / height ) * 2.0f ) - 1.0f );
            v.Z = ( vector.Z - minZ ) / ( maxZ - minZ );

            TransformCoordinate( ref v, ref matrix, out v );
            return v;
        }
        /// <summary>
        /// 反投影,将一个坐标从投影空间中反投影到视图空间
        /// </summary>
        /// <param name="vector">坐标点</param>
        /// <param name="x">屏幕坐标的X值</param>
        /// <param name="y">屏幕坐标的Y值</param>
        /// <param name="width">宽</param>
        /// <param name="height">高</param>
        /// <param name="minZ">最小的Z值</param>
        /// <param name="maxZ">最大的Z值</param>
        /// <param name="worldViewProjection">世界坐标下的转换矩阵</param>
        /// <param name="result">转换后的三维坐标</param>
        [Rtti.Meta]
        public static void Unproject( ref Vector3 vector, float x, float y, float width, float height, float minZ, float maxZ, ref Matrix worldViewProjection, out Vector3 result )
        {
            Vector3 v;
            Matrix matrix;
            Matrix.Invert( ref worldViewProjection, out matrix );

            v.X = ( ( ( vector.X - x ) / width ) * 2.0f ) - 1.0f;
            v.Y = -( ( ( ( vector.Y - y ) / height ) * 2.0f ) - 1.0f );
            v.Z = ( vector.Z - minZ ) / ( maxZ - minZ );

            TransformCoordinate( ref v, ref matrix, out v );
            result = v;
        }
        /// <summary>
        /// 最小化
        /// </summary>
        /// <param name="left">三维坐标</param>
        /// <param name="right">三维坐标</param>
        /// <returns>返回计算后的三维坐标</returns>
        [Rtti.Meta]
        public static Vector3 Minimize( Vector3 left, Vector3 right )
        {
            Vector3 vector;
            vector.X = (left.X < right.X) ? left.X : right.X;
            vector.Y = (left.Y < right.Y) ? left.Y : right.Y;
            vector.Z = (left.Z < right.Z) ? left.Z : right.Z;
            return vector;
        }
        /// <summary>
        /// 最小化
        /// </summary>
        /// <param name="left">三维坐标</param>
        /// <param name="right">三维坐标</param>
        /// <param name="result">计算后的三维坐标</param>
        [Rtti.Meta]
        public static void Minimize( ref Vector3 left, ref Vector3 right, out Vector3 result )
        {
            result.X = (left.X < right.X) ? left.X : right.X;
            result.Y = (left.Y < right.Y) ? left.Y : right.Y;
            result.Z = (left.Z < right.Z) ? left.Z : right.Z;
        }
        /// <summary>
        /// 最大化
        /// </summary>
        /// <param name="left">三维坐标</param>
        /// <param name="right">三维坐标</param>
        /// <returns>返回计算后的三维坐标</returns>
        [Rtti.Meta]
        public static Vector3 Maximize( Vector3 left, Vector3 right )
        {
            Vector3 vector;
            vector.X = (left.X > right.X) ? left.X : right.X;
            vector.Y = (left.Y > right.Y) ? left.Y : right.Y;
            vector.Z = (left.Z > right.Z) ? left.Z : right.Z;
            return vector;
        }
        /// <summary>
        /// 最大化
        /// </summary>
        /// <param name="left">三维坐标</param>
        /// <param name="right">三维坐标</param>
        /// <param name="result">计算后的三维坐标</param>
        [Rtti.Meta]
        public static void Maximize( ref Vector3 left, ref Vector3 right, out Vector3 result )
        {
            result.X = (left.X > right.X) ? left.X : right.X;
            result.Y = (left.Y > right.Y) ? left.Y : right.Y;
            result.Z = (left.Z > right.Z) ? left.Z : right.Z;
        }
        /// <summary>
        /// 重载"+"号运算符
        /// </summary>
        /// <param name="left">三维坐标</param>
        /// <param name="right">三维坐标</param>
        /// <returns>返回计算后的三维坐标</returns>
        public static Vector3 operator + ( Vector3 left, Vector3 right )
        {
            Vector3 result;
            result.X = left.X + right.X;
            result.Y = left.Y + right.Y;
            result.Z = left.Z + right.Z;
            return result;
        }
        /// <summary>
        /// 重载"-"号运算符
        /// </summary>
        /// <param name="left">三维坐标</param>
        /// <param name="right">三维坐标</param>
        /// <returns>返回计算后的三维坐标</returns>
        public static Vector3 operator - ( Vector3 left, Vector3 right )
        {
            Vector3 result;
            result.X = left.X - right.X;
            result.Y = left.Y - right.Y;
            result.Z = left.Z - right.Z;
            return result;
        }
        /// <summary>
        /// 重载"-"号运算符，取向量的反方向
        /// </summary>
        /// <param name="value">三维坐标</param>
        /// <returns>返回计算后的三维坐标</returns>
        public static Vector3 operator - ( Vector3 value )
        {
            Vector3 result;
            result.X = -value.X;
            result.Y = -value.Y;
            result.Z = -value.Z;
            return result;
        }
        /// <summary>
        /// 重载"*"号运算符
        /// </summary>
        /// <param name="value">三维坐标</param>
        /// <param name="scale">放大倍数</param>
        /// <returns>返回计算后的三维坐标</returns>
        public static Vector3 operator * ( Vector3 value, float scale )
        {
            Vector3 result;
            result.X = value.X * scale;
            result.Y = value.Y * scale;
            result.Z = value.Z * scale;
            return result;
        }
        public static Vector3 operator *(Vector3 left, Vector3 right)
        {
            Vector3 result;
            result.X = left.X * right.X;
            result.Y = left.Y * right.Y;
            result.Z = left.Z * right.Z;
            return result;
        }
        /// <summary>
        /// 重载"*"号运算符
        /// </summary>
        /// <param name="scale">三维坐标</param>
        /// <param name="vec">放大倍数</param>
        /// <returns>返回计算后的三维坐标</returns>
        public static Vector3 operator * ( float scale, Vector3 vec )
        {
            return vec * scale;
        }
        /// <summary>
        /// 重载"/"号运算符
        /// </summary>
        /// <param name="value">三维坐标</param>
        /// <param name="scale">缩小倍数</param>
        /// <returns>返回计算后的三维坐标</returns>
        public static Vector3 operator / ( Vector3 value, float scale )
        {
            Vector3 result;
            result.X = value.X / scale;
            result.Y = value.Y / scale;
            result.Z = value.Z / scale;
            return result;
        }
        public static Vector3 operator /(Vector3 left, Vector3 right)
        {
            Vector3 result;
            result.X = left.X / right.X;
            result.Y = left.Y / right.Y;
            result.Z = left.Z / right.Z;
            return result;
        }
        /// <summary>
        /// 重载"=="号运算符，判断两个向量是否相等
        /// </summary>
        /// <param name="left">三维坐标</param>
        /// <param name="right">三维坐标</param>
        /// <returns>如果两个向量相等返回true，否则返回false</returns>
        public static bool operator == ( Vector3 left, Vector3 right )
        {
            return left.Equals(right);
            //return Equals(ref left, ref right );
        }
        /// <summary>
        /// 重载"!="号运算符，判断两个向量是否相等
        /// </summary>
        /// <param name="left">三维坐标</param>
        /// <param name="right">三维坐标</param>
        /// <returns>如果两个向量不相等返回true，否则返回false</returns>
        public static bool operator != ( Vector3 left, Vector3 right )
        {
            return !left.Equals(right);
            //return !Equals(ref left, ref right );
        }
        /// <summary>
        /// 获取对象的哈希值
        /// </summary>
        /// <returns>返回对象的哈希值</returns>
        public override int GetHashCode()
        {
            return X.GetHashCode() + Y.GetHashCode() + Z.GetHashCode();
        }
        /// <summary>
        /// 判断两个对象是否相等
        /// </summary>
        /// <param name="value">可转换成Vector3的对象</param>
        /// <returns>如果两个向量相等返回true，否则返回false</returns>
        public override bool Equals( object value )
        {
            if( value == null )
                return false;

            if( value.GetType() != GetType() )
                return false;

            return Equals((Vector3)value);
        }
        /// <summary>
        /// 判断两个对象是否相等
        /// </summary>
        /// <param name="value">Vector3对象</param>
        /// <returns>如果两个向量相等返回true，否则返回false</returns>
        public bool Equals(Vector3 value)
        {
            bool reX = (Math.Abs(X - value.X) < CoreDefine.Epsilon);
            bool reY = (Math.Abs(Y - value.Y) < CoreDefine.Epsilon);
            bool reZ = (Math.Abs(Z - value.Z) < CoreDefine.Epsilon);
            return (reX && reY && reZ);
        }
        /// <summary>
        /// 判断两个对象是否相等
        /// </summary>
        /// <param name="value1">Vector3对象</param>
        /// <param name="value2">Vector3对象</param>
        /// <returns>如果两个向量相等返回true，否则返回false</returns>
        public static bool Equals( ref Vector3 value1, ref Vector3 value2 )
        {
            bool reX = (Math.Abs(value1.X - value2.X) < CoreDefine.Epsilon);
            bool reY = (Math.Abs(value1.Y - value2.Y) < CoreDefine.Epsilon);
            bool reZ = (Math.Abs(value1.Z - value2.Z) < CoreDefine.Epsilon);
            return (reX && reY && reZ);
        }
    }
}
