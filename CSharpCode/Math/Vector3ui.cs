using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS
{
    [Vector3ui.Vector3Editor]
    [Vector3ui.TypeConverter]
    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential, Pack = 4)]
    public struct Vector3ui : System.IEquatable<Vector3ui>
    {
        public uint X;
        public uint Y;
        public uint Z;

        public class TypeConverterAttribute : Support.TypeConverterAttributeBase
        {
            public override bool ConvertFromString(ref object obj, string text)
            {
                obj = Vector3ui.FromString(text);
                return true;
            }
        }
        public class Vector3EditorAttribute : EGui.Controls.PropertyGrid.PGCustomValueEditorAttribute
        {
            public override unsafe bool OnDraw(in EditorInfo info, out object newValue)
            {
                this.Expandable = true;
                bool retValue = false;
                newValue = info.Value;
                //var saved = v;
                var index = ImGuiAPI.TableGetColumnIndex();
                var width = ImGuiAPI.GetColumnWidth(index);
                ImGuiAPI.SetNextItemWidth(width - EGui.UIProxy.StyleConfig.Instance.PGCellPadding.X);
                //ImGuiAPI.PushStyleVar(ImGuiStyleVar_.ImGuiStyleVar_FramePadding, ref EGui.UIProxy.StyleConfig.Instance.PGInputFramePadding);
                var minValue = uint.MinValue;
                var maxValue = uint.MaxValue;
                var multiValue = info.Value as EGui.Controls.PropertyGrid.PropertyMultiValue;
                if (multiValue != null && multiValue.HasDifferentValue())
                {
                    ImGuiAPI.Text(multiValue.MultiValueString);
                    if (multiValue.DrawVector<Vector3ui>(in info) && !info.Readonly)
                    {
                        newValue = multiValue;
                        retValue = true;
                    }
                }
                else
                {
                    var v = (Vector3ui)info.Value;
                    float speed = 1.0f;
                    var format = "%.6f";
                    if (info.HostProperty != null)
                    {
                        var vR = info.HostProperty.GetAttribute<EGui.Controls.PropertyGrid.PGValueRange>();
                        if (vR != null)
                        {
                            minValue = (uint)vR.Min;
                            maxValue = (uint)vR.Max;
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
                    var changed = ImGuiAPI.DragScalarN2(TName.FromString2("##", info.Name).ToString(), ImGuiDataType_.ImGuiDataType_Float, (uint*)&v, 3, speed, &minValue, &maxValue, format, ImGuiSliderFlags_.ImGuiSliderFlags_None);
                    //ImGuiAPI.InputFloat3(TName.FromString2("##", info.Name).ToString(), (uint*)&v, "%.6f", ImGuiInputTextFlags_.ImGuiInputTextFlags_CharsDecimal);
                    //ImGuiAPI.PopStyleVar(1);
                    if (changed && !info.Readonly)//(v != saved)
                    {
                        newValue = v;
                        retValue = true;
                    }

                    if (Vector4.Vector4EditorAttribute.OnDrawVectorValue<Vector3ui>(in info, ref v, ref v) && !info.Readonly)
                    {
                        newValue = v;
                        retValue = true;
                    }

                }
                return retValue;
            }
        }
        public override string ToString()
        {
            return $"{X},{Y},{Z}";
        }
        public Vector2 GetXY()
        {
            return new Vector2(X, Y);
        }
        public static Vector3ui FromString(string text)
        {
            try
            {
                var result = new Vector3ui();
                ReadOnlySpan<char> chars = text.ToCharArray();
                int iStart = 0;
                int j = 0;
                for (int i = 0; i < chars.Length; i++)
                {
                    if (chars[i] == ',')
                    {
                        result[j] = uint.Parse(chars.Slice(iStart, i - iStart));
                        iStart = i + 1;
                        j++;
                        if (j == 2)
                            break;
                    }
                }
                result[j] = uint.Parse(chars.Slice(iStart, chars.Length - iStart));
                return result;
                //var segs = text.Split(',');
                //return new Vector3ui(System.Convert.ToSingle(segs[0]),
                //    System.Convert.ToSingle(segs[1]),
                //    System.Convert.ToSingle(segs[2]));
            }
            catch
            {
                return Vector3ui.Zero;
            }
        }
        public void SetValue(uint x, uint y, uint z)
        {
            X = x;
            Y = y;
            Z = z;
        }
        public static uint Select(uint Comparand, uint ValueGEZero, uint ValueLTZero)
        {
            return Comparand >= 0.0f ? ValueGEZero : ValueLTZero;
        }
        public uint GetMaxValue()
        {
            return Math.Max(X, Math.Max(Y, Z));
        }
        public uint GetMinValue()
        {
            return Math.Min(X, Math.Min(Y, Z));
        }
        public uint this[int index]
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
                        throw new ArgumentOutOfRangeException("index", "Indices for Vector3ui run from 0 to 2, inclusive.");
                }
            }
            set
            {
                switch (index)
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
                        throw new ArgumentOutOfRangeException("index", "Indices for Vector3ui run from 0 to 2, inclusive.");
                }
            }
        }

        #region Static Member
        public readonly static Vector3ui Zero = new Vector3ui(0, 0, 0);
        public readonly static Vector3ui UnitX = new Vector3ui(1, 0, 0);
        public readonly static Vector3ui UnitY = new Vector3ui(0, 1, 0);
        public readonly static Vector3ui UnitZ = new Vector3ui(0, 0, 1);
        public readonly static Vector3ui One = new Vector3ui(1, 1, 1);
        public readonly static Vector3ui Up = new Vector3ui(0, 1, 0);
        public readonly static Vector3ui Right = new Vector3ui(1, 0, 0);
        public readonly static Vector3ui Forward = new Vector3ui(0, 0, 1);
        
        public readonly static Vector3ui MaxValue = new Vector3ui(uint.MaxValue, uint.MaxValue, uint.MaxValue);
        public readonly static Vector3ui MinValue = new Vector3ui(uint.MinValue, uint.MinValue, uint.MinValue);
        #endregion

        #region Constructure
        /// <summary>
        /// 带参构造函数
        /// </summary>
        /// <param name="value">Vector3ui对象</param>
        public Vector3ui(in Vector3ui value)
        {
            X = value.X;
            Y = value.Y;
            Z = value.Z;
        }
        /// <summary>
        /// 带参构造函数
        /// </summary>
        /// <param name="value">值</param>
        public Vector3ui(uint value)
        {
            X = value;
            Y = value;
            Z = value;
        }
        //public Vector3ui( Vector2 value, uint z )
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
        public Vector3ui(uint x, uint y, uint z)
        {
            X = x;
            Y = y;
            Z = z;
        }
        #endregion
        public bool HasNagative()
        {
            if (X < 0 || Y < 0 || Z < 0)
                return true;
            return false;
        }
        /// <summary>
        /// 长度
        /// </summary>
        /// <returns>返回向量的长度</returns>
        [Rtti.Meta]
        public uint Length()
        {
            return (uint)(System.Math.Sqrt((X * X) + (Y * Y) + (Z * Z)));
        }
        /// <summary>
        /// 长度的平方
        /// </summary>
        /// <returns>返回向量长度的平方</returns>
        [Rtti.Meta]
        public uint LengthSquared()
        {
            return (X * X) + (Y * Y) + (Z * Z);
        }
        /// <summary>
        /// 向量的单位化
        /// </summary>
        [Rtti.Meta]
        public uint Normalize()
        {
            uint length = Length();
            if (length == 0)
                return length;
            uint num = 1 / length;
            X *= num;
            Y *= num;
            Z *= num;
            return length;
        }
        /// <summary>
        /// 向量的单位向量，不改变原向量
        /// </summary>
        [Rtti.Meta]
        public Vector3ui NormalizeValue
        {
            get
            {
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
        public static Vector3ui Add(in Vector3ui left, in Vector3ui right)
        {
            Vector3ui result;
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
        public static void Add(in Vector3ui left, in Vector3ui right, out Vector3ui result)
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
        public static Vector3ui Subtract(in Vector3ui left, in Vector3ui right)
        {
            EngineNS.Vector3ui result;
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
        public static void Subtract(in Vector3ui left, in Vector3ui right, out Vector3ui result)
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
        public static Vector3ui Modulate(in Vector3ui left, in Vector3ui right)
        {
            Vector3ui result;
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
        public static void Modulate(in Vector3ui left, in Vector3ui right, out Vector3ui result)
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
        public static Vector3ui Multiply(in Vector3ui value, uint scale)
        {
            Vector3ui result;
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
        public static void Multiply(in Vector3ui value, uint scale, out Vector3ui result)
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
        public static Vector3ui Divide(in Vector3ui value, uint scale)
        {
            Vector3ui result;
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
        public static void Divide(in Vector3ui value, uint scale, out Vector3ui result)
        {
            result.X = value.X / scale;
            result.Y = value.Y / scale;
            result.Z = value.Z / scale;
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
        public static Vector3ui Barycentric(in Vector3ui value1, in Vector3ui value2, in Vector3ui value3, uint amount1, uint amount2)
        {
            Vector3ui vector;
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
        public static void Barycentric(in Vector3ui value1, in Vector3ui value2, in Vector3ui value3, uint amount1, uint amount2, out Vector3ui result)
        {
            result.X = (value1.X + (amount1 * (value2.X - value1.X))) + (amount2 * (value3.X - value1.X));
            result.Y = (value1.Y + (amount1 * (value2.Y - value1.Y))) + (amount2 * (value3.Y - value1.Y));
            result.Z = (value1.Z + (amount1 * (value2.Z - value1.Z))) + (amount2 * (value3.Z - value1.Z));
        }
        /// <summary>
        /// 载体计算
        /// </summary>
        /// <param name="value">三维坐标点</param>
        /// <param name="min">三维坐标点的最小值</param>
        /// <param name="max">三维坐标点的最大值</param>
        /// <returns>返回计算结果</returns>
        [Rtti.Meta]
        public static Vector3ui Clamp(in Vector3ui value, in Vector3ui min, in Vector3ui max)
        {
            uint x = value.X;
            x = (x > max.X) ? max.X : x;
            x = (x < min.X) ? min.X : x;

            uint y = value.Y;
            y = (y > max.Y) ? max.Y : y;
            y = (y < min.Y) ? min.Y : y;

            uint z = value.Z;
            z = (z > max.Z) ? max.Z : z;
            z = (z < min.Z) ? min.Z : z;

            Vector3ui result;
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
        public static void Clamp(in Vector3ui value, in Vector3ui min, in Vector3ui max, out Vector3ui result)
        {
            uint x = value.X;
            x = (x > max.X) ? max.X : x;
            x = (x < min.X) ? min.X : x;

            uint y = value.Y;
            y = (y > max.Y) ? max.Y : y;
            y = (y < min.Y) ? min.Y : y;

            uint z = value.Z;
            z = (z > max.Z) ? max.Z : z;
            z = (z < min.Z) ? min.Z : z;

            result.X = x;
            result.Y = y;
            result.Z = z;
        }
        /// <summary>
        /// 计算线性插值
        /// </summary>
        /// <param name="start">起点坐标</param>
        /// <param name="end">终点坐标</param>
        /// <param name="factor">插值因子</param>
        /// <returns>返回计算后的向量</returns>
        [Rtti.Meta]
        public static Vector3ui Lerp(in Vector3ui start, in Vector3ui end, uint factor)
        {
            Vector3ui vector;

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
        public static void Lerp(in Vector3ui start, in Vector3ui end, uint factor, out Vector3ui result)
        {
            result.X = start.X + ((end.X - start.X) * factor);
            result.Y = start.Y + ((end.Y - start.Y) * factor);
            result.Z = start.Z + ((end.Z - start.Z) * factor);
        }
        /// <summary>
        /// 计算两点间的距离
        /// </summary>
        /// <param name="value1">坐标点</param>
        /// <param name="value2">坐标点</param>
        /// <returns>返回两点间的距离</returns>
        [Rtti.Meta]
        public static uint Distance(in Vector3ui value1, in Vector3ui value2)
        {
            uint x = value1.X - value2.X;
            uint y = value1.Y - value2.Y;
            uint z = value1.Z - value2.Z;

            return (uint)(Math.Sqrt((x * x) + (y * y) + (z * z)));
        }
        [Rtti.Meta]
        public static uint RayDistanceSquared(in Vector3ui point, in Vector3ui start, in Vector3ui dirNormalized, out uint len)
        {
            var v = point - start;
            len = Vector3ui.Dot(in v, in dirNormalized);
            return v.LengthSquared() - (len * len);
        }
        /// <summary>
        /// 计算两点间的距离的平方
        /// </summary>
        /// <param name="value1">坐标点</param>
        /// <param name="value2">坐标点</param>
        /// <returns>返回两点间的距离的平方</returns>
        [Rtti.Meta]
        public static uint DistanceSquared(in Vector3ui value1, in Vector3ui value2)
        {
            uint x = value1.X - value2.X;
            uint y = value1.Y - value2.Y;
            uint z = value1.Z - value2.Z;

            return (x * x) + (y * y) + (z * z);
        }
        /// <summary>
        /// 向量的点积
        /// </summary>
        /// <param name="left">三维向量</param>
        /// <param name="right">三维向量</param>
        /// <returns>返回点积值</returns>        
        public static uint Dot(in Vector3ui left, in Vector3ui right)
        {
            uint num;
            Dot(in left, in right, out num);
            return num;
        }
        public static void Dot(in Vector3ui left, in Vector3ui right, out uint num)
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
        public static Vector3ui Cross(in Vector3ui left, in Vector3ui right)
        {
            Vector3ui result;
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
        public static void Cross(in Vector3ui left, in Vector3ui right, out Vector3ui result)
        {
            Vector3ui r;
            r.X = left.Y * right.Z - left.Z * right.Y;
            r.Y = left.Z * right.X - left.X * right.Z;
            r.Z = left.X * right.Y - left.Y * right.X;

            result = r;
        }
        public static Vector3ui CalcFaceNormal(in Vector3ui a, in Vector3ui b, in Vector3ui c)
        {
            var t1 = a - c;
            var t2 = b - c;
            Vector3ui result;
            Cross(in t1, in t2, out result);
            result.Normalize();
            return result;
        }
        public static uint CalcArea3(in Vector3ui a, in Vector3ui b, in Vector3ui c)
        {
            //此处是向量叉积的几何意义的应用
            //没处以2，所以出来的是平行四边形面积，并且有正负数的问题，
            //正数说明夹角是负角度
            //计算面积，外面要用abs * 0.5
            Vector3ui v1 = b - a;
            Vector3ui v2 = c - a;
            return ((v1.Y * v2.Z + v1.Z * v2.X + v1.X * v2.Y) -
                    (v1.Y * v2.X + v1.X * v2.Z + v1.X * v2.Y));
        }
        /// <summary>
        /// 向量的单位化
        /// </summary>
        /// <param name="vector">三维向量</param>
        /// <returns>返回单位向量</returns>
        [Rtti.Meta]
        public static Vector3ui Normalize(in Vector3ui vector)
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
        public static void Normalize(in Vector3ui vector, out Vector3ui result)
        {
            result = vector;
            result.Normalize();
        }
        [Rtti.Meta]
        public static Vector4 Transform(in Vector3ui vector, in Matrix transform)
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
        public static void Transform(in Vector3ui vector, in Matrix transform, out Vector4 result)
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
        public static void Transform(IntPtr vectorsIn, uint inputStride, IntPtr transformation, IntPtr vectorsOut, uint outputStride, uint count)
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
        public static void Transform(IntPtr vectorsIn, IntPtr transformation, IntPtr vectorsOut, uint count)
        {
            Transform(vectorsIn, sizeof(uint) * 3, transformation, vectorsOut, sizeof(uint) * 4, count);
        }
        /// <summary>
        /// 最小化
        /// </summary>
        /// <param name="left">三维坐标</param>
        /// <param name="right">三维坐标</param>
        /// <returns>返回计算后的三维坐标</returns>
        [Rtti.Meta]
        public static Vector3ui Minimize(in Vector3ui left, in Vector3ui right)
        {
            Vector3ui vector;
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
        public static void Minimize(in Vector3ui left, in Vector3ui right, out Vector3ui result)
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
        public static Vector3ui Maximize(in Vector3ui left, in Vector3ui right)
        {
            Vector3ui vector;
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
        public static void Maximize(in Vector3ui left, in Vector3ui right, out Vector3ui result)
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
        public static Vector3ui operator +(in Vector3ui left, in Vector3ui right)
        {
            Vector3ui result;
            result.X = left.X + right.X;
            result.Y = left.Y + right.Y;
            result.Z = left.Z + right.Z;
            return result;
        }
        public static Vector3ui operator +(in Vector3ui left, uint right)
        {
            Vector3ui result;
            result.X = left.X + right;
            result.Y = left.Y + right;
            result.Z = left.Z + right;
            return result;
        }
        /// <summary>
        /// 重载"-"号运算符
        /// </summary>
        /// <param name="left">三维坐标</param>
        /// <param name="right">三维坐标</param>
        /// <returns>返回计算后的三维坐标</returns>
        public static Vector3ui operator -(in Vector3ui left, in Vector3ui right)
        {
            Vector3ui result;
            result.X = left.X - right.X;
            result.Y = left.Y - right.Y;
            result.Z = left.Z - right.Z;
            return result;
        }
        /// <summary>
        /// 重载"*"号运算符
        /// </summary>
        /// <param name="value">三维坐标</param>
        /// <param name="scale">放大倍数</param>
        /// <returns>返回计算后的三维坐标</returns>
        public static Vector3ui operator *(in Vector3ui value, uint scale)
        {
            Vector3ui result;
            result.X = value.X * scale;
            result.Y = value.Y * scale;
            result.Z = value.Z * scale;
            return result;
        }
        public static Vector3ui operator *(in Vector3ui left, in Vector3ui right)
        {
            Vector3ui result;
            result.X = left.X * right.X;
            result.Y = left.Y * right.Y;
            result.Z = left.Z * right.Z;
            return result;
        }
        public static DVector3 operator *(in Vector3ui left, in DVector3 right)
        {
            DVector3 result;
            result.X = right.X * left.X;
            result.Y = right.Y * left.Y;
            result.Z = right.Z * left.Z;
            return result;
        }
        /// <summary>
        /// 重载"*"号运算符
        /// </summary>
        /// <param name="scale">三维坐标</param>
        /// <param name="vec">放大倍数</param>
        /// <returns>返回计算后的三维坐标</returns>
        public static Vector3ui operator *(uint scale, in Vector3ui vec)
        {
            return vec * scale;
        }
        /// <summary>
        /// 重载"/"号运算符
        /// </summary>
        /// <param name="value">三维坐标</param>
        /// <param name="scale">缩小倍数</param>
        /// <returns>返回计算后的三维坐标</returns>
        public static Vector3ui operator /(in Vector3ui value, uint scale)
        {
            Vector3ui result;
            result.X = value.X / scale;
            result.Y = value.Y / scale;
            result.Z = value.Z / scale;
            return result;
        }
        public static Vector3ui operator /(in Vector3ui left, in Vector3ui right)
        {
            Vector3ui result;
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
        public static bool operator ==(in Vector3ui left, in Vector3ui right)
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
        public static bool operator !=(in Vector3ui left, in Vector3ui right)
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
        /// <param name="value">可转换成Vector3ui的对象</param>
        /// <returns>如果两个向量相等返回true，否则返回false</returns>
        public override bool Equals(object value)
        {
            if (value == null)
                return false;

            if (value.GetType() != GetType())
                return false;

            return Equals((Vector3ui)value);
        }
        /// <summary>
        /// 判断两个对象是否相等
        /// </summary>
        /// <param name="value">Vector3ui对象</param>
        /// <returns>如果两个向量相等返回true，否则返回false</returns>
        public bool Equals(Vector3ui value)
        {
            bool reX = (Math.Abs(X - value.X) < MathHelper.Epsilon);
            bool reY = (Math.Abs(Y - value.Y) < MathHelper.Epsilon);
            bool reZ = (Math.Abs(Z - value.Z) < MathHelper.Epsilon);
            return (reX && reY && reZ);
        }
        /// <summary>
        /// 判断两个对象是否相等
        /// </summary>
        /// <param name="value1">Vector3ui对象</param>
        /// <param name="value2">Vector3ui对象</param>
        /// <returns>如果两个向量相等返回true，否则返回false</returns>        
        public static Bool3 Equals(in Vector3ui value1, in Vector3ui value2)
        {
            Bool3 result;
            result.X = value1.X == value2.X;
            result.Y = value1.Y == value2.Y;
            result.Z = value1.Z == value2.Z;
            return result;
        }
        public static Bool3 Less(in Vector3ui value1, in Vector3ui value2)
        {
            Bool3 result;
            result.X = value1.X < value2.X;
            result.Y = value1.Y < value2.Y;
            result.Z = value1.Z < value2.Z;
            return result;
        }
        public static Bool3 LessEqual(in Vector3ui value1, in Vector3ui value2)
        {
            Bool3 result;
            result.X = value1.X <= value2.X;
            result.Y = value1.Y <= value2.Y;
            result.Z = value1.Z <= value2.Z;
            return result;
        }
        public static Bool3 Great(in Vector3ui value1, in Vector3ui value2)
        {
            Bool3 result;
            result.X = value1.X > value2.X;
            result.Y = value1.Y > value2.Y;
            result.Z = value1.Z > value2.Z;
            return result;
        }
        public static Bool3 GreatEqual(in Vector3ui value1, in Vector3ui value2)
        {
            Bool3 result;
            result.X = value1.X >= value2.X;
            result.Y = value1.Y >= value2.Y;
            result.Z = value1.Z >= value2.Z;
            return result;
        }
        public static Vector3ui Mod(in Vector3ui value1, in Vector3ui value2)
        {
            Vector3ui result;
            result.X = value1.X % value2.X;
            result.Y = value1.Y % value2.Y;
            result.Z = value1.Z % value2.Z;
            return result;
        }
    }
}
