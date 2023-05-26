using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace EngineNS
{
    [Vector2i.Vector3Editor]
    [Vector2i.TypeConverter]
    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential, Pack = 4)]
    public struct Vector2i : System.IEquatable<Vector2i>
    {
        public int X;
        public int Y;

        public class TypeConverterAttribute : Support.TypeConverterAttributeBase
        {
            public override bool ConvertFromString(ref object obj, string text)
            {
                obj = Vector2i.FromString(text);
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
                var minValue = int.MinValue;
                var maxValue = int.MaxValue;
                var multiValue = info.Value as EGui.Controls.PropertyGrid.PropertyMultiValue;
                if (multiValue != null && multiValue.HasDifferentValue())
                {
                    ImGuiAPI.Text(multiValue.MultiValueString);
                    if (multiValue.DrawVector<Vector2i>(in info) && !info.Readonly)
                    {
                        newValue = multiValue;
                        retValue = true;
                    }
                }
                else
                {
                    var v = (Vector2i)info.Value;
                    float speed = 1.0f;
                    var format = "%.6f";
                    if (info.HostProperty != null)
                    {
                        var vR = info.HostProperty.GetAttribute<EGui.Controls.PropertyGrid.PGValueRange>();
                        if (vR != null)
                        {
                            minValue = (int)vR.Min;
                            maxValue = (int)vR.Max;
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
                    var changed = ImGuiAPI.DragScalarN2(TName.FromString2("##", info.Name).ToString(), ImGuiDataType_.ImGuiDataType_Float, (int*)&v, 3, speed, &minValue, &maxValue, format, ImGuiSliderFlags_.ImGuiSliderFlags_None);
                    //ImGuiAPI.InputFloat3(TName.FromString2("##", info.Name).ToString(), (int*)&v, "%.6f", ImGuiInputTextFlags_.ImGuiInputTextFlags_CharsDecimal);
                    //ImGuiAPI.PopStyleVar(1);
                    if (changed && !info.Readonly)//(v != saved)
                    {
                        newValue = v;
                        retValue = true;
                    }

                    if (Vector4.Vector4EditorAttribute.OnDrawVectorValue<Vector2i>(in info, ref v, ref v) && !info.Readonly)
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
            return $"{X},{Y}";
        }
        public Vector2 GetXY()
        {
            return new Vector2(X, Y);
        }
        public static Vector2i FromString(string text)
        {
            try
            {
                var result = new Vector2i();
                ReadOnlySpan<char> chars = text.ToCharArray();
                int iStart = 0;
                int j = 0;
                for (int i = 0; i < chars.Length; i++)
                {
                    if (chars[i] == ',')
                    {
                        result[j] = int.Parse(chars.Slice(iStart, i - iStart));
                        iStart = i + 1;
                        j++;
                        if (j == 2)
                            break;
                    }
                }
                result[j] = int.Parse(chars.Slice(iStart, chars.Length - iStart));
                return result;
                //var segs = text.Split(',');
                //return new Vector2i(System.Convert.ToSingle(segs[0]),
                //    System.Convert.ToSingle(segs[1]),
                //    System.Convert.ToSingle(segs[2]));
            }
            catch
            {
                return Vector2i.Zero;
            }
        }
        public void SetValue(int x, int y)
        {
            X = x;
            Y = y;
        }
        public static int Select(int Comparand, int ValueGEZero, int ValueLTZero)
        {
            return Comparand >= 0.0f ? ValueGEZero : ValueLTZero;
        }
        public Vector2i GetSignVector()
        {
            return new Vector2i
                (
                Select(X, 1, -1),
                Select(Y, 1, -1)
                );
        }
        public int GetMaxValue()
        {
            return Math.Max(X, Y);
        }
        public int GetMinValue()
        {
            return Math.Min(X, Y);
        }
        public int this[int index]
        {
            get
            {
                switch (index)
                {
                    case 0:
                        return X;
                    case 1:
                        return Y;
                    default:
                        throw new ArgumentOutOfRangeException("index", "Indices for Vector2i run from 0 to 2, inclusive.");
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
                    default:
                        throw new ArgumentOutOfRangeException("index", "Indices for Vector2i run from 0 to 2, inclusive.");
                }
            }
        }

        #region Static Member
        public readonly static Vector2i Zero = new Vector2i(0, 0);
        public readonly static Vector2i UnitX = new Vector2i(1, 0);
        public readonly static Vector2i UnitY = new Vector2i(0, 1);
        public readonly static Vector2i One = new Vector2i(1, 1);
        public readonly static Vector2i MinusOne = new Vector2i(-1, -1);
        public readonly static Vector2i Up = new Vector2i(0, 1);
        public readonly static Vector2i Down = new Vector2i(0, -1);
        public readonly static Vector2i Right = new Vector2i(1, 0);
        public readonly static Vector2i Left = new Vector2i(-1, 0);
        
        public readonly static Vector2i MaxValue = new Vector2i(int.MaxValue, int.MaxValue);
        public readonly static Vector2i MinValue = new Vector2i(int.MinValue, int.MinValue);
        #endregion

        #region Constructure
        /// <summary>
        /// 带参构造函数
        /// </summary>
        /// <param name="value">Vector2i对象</param>
        public Vector2i(in Vector2i value)
        {
            X = value.X;
            Y = value.Y;
        }
        /// <summary>
        /// 带参构造函数
        /// </summary>
        /// <param name="value">值</param>
        public Vector2i(int value)
        {
            X = value;
            Y = value;
        }
        //public Vector2i( Vector2 value, int z )
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
        public Vector2i(int x, int y)
        {
            X = x;
            Y = y;
        }
        #endregion
        public bool HasNagative()
        {
            if (X < 0 || Y < 0)
                return true;
            return false;
        }
        /// <summary>
        /// 长度
        /// </summary>
        /// <returns>返回向量的长度</returns>
        [Rtti.Meta]
        public int Length()
        {
            return (int)(System.Math.Sqrt((X * X) + (Y * Y)));
        }
        /// <summary>
        /// 长度的平方
        /// </summary>
        /// <returns>返回向量长度的平方</returns>
        [Rtti.Meta]
        public int LengthSquared()
        {
            return (X * X) + (Y * Y);
        }
        /// <summary>
        /// 向量的单位化
        /// </summary>
        [Rtti.Meta]
        public int Normalize()
        {
            int length = Length();
            if (length == 0)
                return length;
            int num = 1 / length;
            X *= num;
            Y *= num;
            return length;
        }
        /// <summary>
        /// 向量的单位向量，不改变原向量
        /// </summary>
        [Rtti.Meta]
        public Vector2i NormalizeValue
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
        public static Vector2i Add(in Vector2i left, in Vector2i right)
        {
            Vector2i result;
            result.X = left.X + right.X;
            result.Y = left.Y + right.Y;
            return result;
        }
        /// <summary>
        /// 两个三维向量的和
        /// </summary>
        /// <param name="left">三维向量</param>
        /// <param name="right">三维向量</param>
        /// <param name="result">两个三维向量的和</param>
        [Rtti.Meta]
        public static void Add(in Vector2i left, in Vector2i right, out Vector2i result)
        {
            result.X = left.X + right.X;
            result.Y = left.Y + right.Y;
        }
        /// <summary>
        /// 两个三维向量的差
        /// </summary>
        /// <param name="left">三维向量</param>
        /// <param name="right">三维向量</param>
        /// <returns>返回两个三维向量的差</returns>
        [Rtti.Meta]
        public static Vector2i Subtract(in Vector2i left, in Vector2i right)
        {
            EngineNS.Vector2i result;
            result.X = left.X - right.X;
            result.Y = left.Y - right.Y;
            return result;
        }
        /// <summary>
        /// 两个三维向量的差
        /// </summary>
        /// <param name="left">三维向量</param>
        /// <param name="right">三维向量</param>
        /// <param name="result">两个三维向量的差</param>
        [Rtti.Meta]
        public static void Subtract(in Vector2i left, in Vector2i right, out Vector2i result)
        {
            result.X = left.X - right.X;
            result.Y = left.Y - right.Y;
        }
        /// <summary>
        /// 两个三维向量的积
        /// </summary>
        /// <param name="left">三维向量</param>
        /// <param name="right">三维向量</param>
        /// <returns>返回两个三维向量的积</returns>
        [Rtti.Meta]
        public static Vector2i Modulate(in Vector2i left, in Vector2i right)
        {
            Vector2i result;
            result.X = left.X * right.X;
            result.Y = left.Y * right.Y;
            return result;
        }
        /// <summary>
        /// 两个三维向量的积
        /// </summary>
        /// <param name="left">三维向量</param>
        /// <param name="right">三维向量</param>
        /// <param name="result">两个三维向量的积</param>
        [Rtti.Meta]
        public static void Modulate(in Vector2i left, in Vector2i right, out Vector2i result)
        {
            result.X = left.X * right.X;
            result.Y = left.Y * right.Y;
        }
        /// <summary>
        /// 向量的数乘
        /// </summary>
        /// <param name="value">三维向量</param>
        /// <param name="scale">常数</param>
        /// <returns>返回计算结果</returns>
        [Rtti.Meta]
        public static Vector2i Multiply(in Vector2i value, int scale)
        {
            Vector2i result;
            result.X = value.X * scale;
            result.Y = value.Y * scale;
            return result;
        }
        /// <summary>
        /// 向量的数乘
        /// </summary>
        /// <param name="value">三维向量</param>
        /// <param name="scale">常数</param>
        /// <param name="result">计算结果</param>
        [Rtti.Meta]
        public static void Multiply(in Vector2i value, int scale, out Vector2i result)
        {
            result.X = value.X * scale;
            result.Y = value.Y * scale;
        }
        /// <summary>
        /// 向量的除法
        /// </summary>
        /// <param name="value">三维向量</param>
        /// <param name="scale">常数</param>
        /// <returns>返回计算结果</returns>
        [Rtti.Meta]
        public static Vector2i Divide(in Vector2i value, int scale)
        {
            Vector2i result;
            result.X = value.X / scale;
            result.Y = value.Y / scale;
            return result;
        }
        /// <summary>
        /// 向量的除法
        /// </summary>
        /// <param name="value">三维向量</param>
        /// <param name="scale">常数</param>
        /// <param name="result">计算结果</param>
        [Rtti.Meta]
        public static void Divide(in Vector2i value, int scale, out Vector2i result)
        {
            result.X = value.X / scale;
            result.Y = value.Y / scale;
        }
        /// <summary>
        /// 向量的取反
        /// </summary>
        /// <param name="value">三维向量</param>
        /// <returns>返回计算结果</returns>
        [Rtti.Meta]
        public static Vector2i Negate(in Vector2i value)
        {
            Vector2i result;
            result.X = -value.X;
            result.Y = -value.Y;
            return result;
        }
        /// <summary>
        /// 向量的取反
        /// </summary>
        /// <param name="value">三维向量</param>
        /// <param name="result">计算结果</param>
        [Rtti.Meta]
        public static void Negate(in Vector2i value, out Vector2i result)
        {
            result.X = -value.X;
            result.Y = -value.Y;
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
        public static Vector2i Barycentric(in Vector2i value1, in Vector2i value2, in Vector2i value3, int amount1, int amount2)
        {
            Vector2i vector;
            vector.X = (value1.X + (amount1 * (value2.X - value1.X))) + (amount2 * (value3.X - value1.X));
            vector.Y = (value1.Y + (amount1 * (value2.Y - value1.Y))) + (amount2 * (value3.Y - value1.Y));
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
        public static void Barycentric(in Vector2i value1, in Vector2i value2, in Vector2i value3, int amount1, int amount2, out Vector2i result)
        {
            result.X = (value1.X + (amount1 * (value2.X - value1.X))) + (amount2 * (value3.X - value1.X));
            result.Y = (value1.Y + (amount1 * (value2.Y - value1.Y))) + (amount2 * (value3.Y - value1.Y));
        }
        /// <summary>
        /// 载体计算
        /// </summary>
        /// <param name="value">三维坐标点</param>
        /// <param name="min">三维坐标点的最小值</param>
        /// <param name="max">三维坐标点的最大值</param>
        /// <returns>返回计算结果</returns>
        [Rtti.Meta]
        public static Vector2i Clamp(in Vector2i value, in Vector2i min, in Vector2i max)
        {
            int x = value.X;
            x = (x > max.X) ? max.X : x;
            x = (x < min.X) ? min.X : x;

            int y = value.Y;
            y = (y > max.Y) ? max.Y : y;
            y = (y < min.Y) ? min.Y : y;

            Vector2i result;
            result.X = x;
            result.Y = y;
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
        public static void Clamp(in Vector2i value, in Vector2i min, in Vector2i max, out Vector2i result)
        {
            int x = value.X;
            x = (x > max.X) ? max.X : x;
            x = (x < min.X) ? min.X : x;

            int y = value.Y;
            y = (y > max.Y) ? max.Y : y;
            y = (y < min.Y) ? min.Y : y;

            result.X = x;
            result.Y = y;
        }
        /// <summary>
        /// 计算线性插值
        /// </summary>
        /// <param name="start">起点坐标</param>
        /// <param name="end">终点坐标</param>
        /// <param name="factor">插值因子</param>
        /// <returns>返回计算后的向量</returns>
        [Rtti.Meta]
        public static Vector2i Lerp(in Vector2i start, in Vector2i end, int factor)
        {
            Vector2i vector;

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
        public static void Lerp(in Vector2i start, in Vector2i end, int factor, out Vector2i result)
        {
            result.X = start.X + ((end.X - start.X) * factor);
            result.Y = start.Y + ((end.Y - start.Y) * factor);
        }
        /// <summary>
        /// 计算两点间的距离
        /// </summary>
        /// <param name="value1">坐标点</param>
        /// <param name="value2">坐标点</param>
        /// <returns>返回两点间的距离</returns>
        [Rtti.Meta]
        public static int Distance(in Vector2i value1, in Vector2i value2)
        {
            int x = value1.X - value2.X;
            int y = value1.Y - value2.Y;
            
            return (int)(Math.Sqrt((x * x) + (y * y)));
        }
        [Rtti.Meta]
        public static int RayDistanceSquared(in Vector2i point, in Vector2i start, in Vector2i dirNormalized, out int len)
        {
            var v = point - start;
            len = Vector2i.Dot(in v, in dirNormalized);
            return v.LengthSquared() - (len * len);
        }
        /// <summary>
        /// 计算两点间的距离的平方
        /// </summary>
        /// <param name="value1">坐标点</param>
        /// <param name="value2">坐标点</param>
        /// <returns>返回两点间的距离的平方</returns>
        [Rtti.Meta]
        public static int DistanceSquared(in Vector2i value1, in Vector2i value2)
        {
            int x = value1.X - value2.X;
            int y = value1.Y - value2.Y;
            
            return (x * x) + (y * y);
        }
        /// <summary>
        /// 向量的点积
        /// </summary>
        /// <param name="left">三维向量</param>
        /// <param name="right">三维向量</param>
        /// <returns>返回点积值</returns>        
        public static int Dot(in Vector2i left, in Vector2i right)
        {
            int num;
            Dot(in left, in right, out num);
            return num;
        }
        public static void Dot(in Vector2i left, in Vector2i right, out int num)
        {
            num = (left.X * right.X + left.Y * right.Y);
        }
        /// <summary>
        /// 向量的单位化
        /// </summary>
        /// <param name="vector">三维向量</param>
        /// <returns>返回单位向量</returns>
        [Rtti.Meta]
        public static Vector2i Normalize(in Vector2i vector)
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
        public static void Normalize(in Vector2i vector, out Vector2i result)
        {
            result = vector;
            result.Normalize();
        }
        
        /// <summary>
        /// 最小化
        /// </summary>
        /// <param name="left">三维坐标</param>
        /// <param name="right">三维坐标</param>
        /// <returns>返回计算后的三维坐标</returns>
        [Rtti.Meta]
        public static Vector2i Minimize(in Vector2i left, in Vector2i right)
        {
            Vector2i vector;
            vector.X = (left.X < right.X) ? left.X : right.X;
            vector.Y = (left.Y < right.Y) ? left.Y : right.Y;
            return vector;
        }
        /// <summary>
        /// 最小化
        /// </summary>
        /// <param name="left">三维坐标</param>
        /// <param name="right">三维坐标</param>
        /// <param name="result">计算后的三维坐标</param>
        [Rtti.Meta]
        public static void Minimize(in Vector2i left, in Vector2i right, out Vector2i result)
        {
            result.X = (left.X < right.X) ? left.X : right.X;
            result.Y = (left.Y < right.Y) ? left.Y : right.Y;
        }
        /// <summary>
        /// 最大化
        /// </summary>
        /// <param name="left">三维坐标</param>
        /// <param name="right">三维坐标</param>
        /// <returns>返回计算后的三维坐标</returns>
        [Rtti.Meta]
        public static Vector2i Maximize(in Vector2i left, in Vector2i right)
        {
            Vector2i vector;
            vector.X = (left.X > right.X) ? left.X : right.X;
            vector.Y = (left.Y > right.Y) ? left.Y : right.Y;
            return vector;
        }
        /// <summary>
        /// 最大化
        /// </summary>
        /// <param name="left">三维坐标</param>
        /// <param name="right">三维坐标</param>
        /// <param name="result">计算后的三维坐标</param>
        [Rtti.Meta]
        public static void Maximize(in Vector2i left, in Vector2i right, out Vector2i result)
        {
            result.X = (left.X > right.X) ? left.X : right.X;
            result.Y = (left.Y > right.Y) ? left.Y : right.Y;
        }
        /// <summary>
        /// 重载"+"号运算符
        /// </summary>
        /// <param name="left">三维坐标</param>
        /// <param name="right">三维坐标</param>
        /// <returns>返回计算后的三维坐标</returns>
        public static Vector2i operator +(in Vector2i left, in Vector2i right)
        {
            Vector2i result;
            result.X = left.X + right.X;
            result.Y = left.Y + right.Y;
            return result;
        }
        public static Vector2i operator +(in Vector2i left, int right)
        {
            Vector2i result;
            result.X = left.X + right;
            result.Y = left.Y + right;
            return result;
        }
        /// <summary>
        /// 重载"-"号运算符
        /// </summary>
        /// <param name="left">三维坐标</param>
        /// <param name="right">三维坐标</param>
        /// <returns>返回计算后的三维坐标</returns>
        public static Vector2i operator -(in Vector2i left, in Vector2i right)
        {
            Vector2i result;
            result.X = left.X - right.X;
            result.Y = left.Y - right.Y;
            return result;
        }
        /// <summary>
        /// 重载"-"号运算符，取向量的反方向
        /// </summary>
        /// <param name="value">三维坐标</param>
        /// <returns>返回计算后的三维坐标</returns>
        public static Vector2i operator -(in Vector2i value)
        {
            Vector2i result;
            result.X = -value.X;
            result.Y = -value.Y;
            return result;
        }
        /// <summary>
        /// 重载"*"号运算符
        /// </summary>
        /// <param name="value">三维坐标</param>
        /// <param name="scale">放大倍数</param>
        /// <returns>返回计算后的三维坐标</returns>
        public static Vector2i operator *(in Vector2i value, int scale)
        {
            Vector2i result;
            result.X = value.X * scale;
            result.Y = value.Y * scale;
            return result;
        }
        public static Vector2i operator *(in Vector2i left, in Vector2i right)
        {
            Vector2i result;
            result.X = left.X * right.X;
            result.Y = left.Y * right.Y;
            return result;
        }
        /// <summary>
        /// 重载"*"号运算符
        /// </summary>
        /// <param name="scale">三维坐标</param>
        /// <param name="vec">放大倍数</param>
        /// <returns>返回计算后的三维坐标</returns>
        public static Vector2i operator *(int scale, in Vector2i vec)
        {
            return vec * scale;
        }
        /// <summary>
        /// 重载"/"号运算符
        /// </summary>
        /// <param name="value">三维坐标</param>
        /// <param name="scale">缩小倍数</param>
        /// <returns>返回计算后的三维坐标</returns>
        public static Vector2i operator /(in Vector2i value, int scale)
        {
            Vector2i result;
            result.X = value.X / scale;
            result.Y = value.Y / scale;
            return result;
        }
        public static Vector2i operator /(in Vector2i left, in Vector2i right)
        {
            Vector2i result;
            result.X = left.X / right.X;
            result.Y = left.Y / right.Y;
            return result;
        }
        /// <summary>
        /// 重载"=="号运算符，判断两个向量是否相等
        /// </summary>
        /// <param name="left">三维坐标</param>
        /// <param name="right">三维坐标</param>
        /// <returns>如果两个向量相等返回true，否则返回false</returns>
        public static bool operator ==(in Vector2i left, in Vector2i right)
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
        public static bool operator !=(in Vector2i left, in Vector2i right)
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
            return X.GetHashCode() + Y.GetHashCode();
        }
        /// <summary>
        /// 判断两个对象是否相等
        /// </summary>
        /// <param name="value">可转换成Vector2i的对象</param>
        /// <returns>如果两个向量相等返回true，否则返回false</returns>
        public override bool Equals(object value)
        {
            if (value == null)
                return false;

            if (value.GetType() != GetType())
                return false;

            return Equals((Vector2i)value);
        }
        /// <summary>
        /// 判断两个对象是否相等
        /// </summary>
        /// <param name="value">Vector2i对象</param>
        /// <returns>如果两个向量相等返回true，否则返回false</returns>
        public bool Equals(Vector2i value)
        {
            bool reX = (Math.Abs(X - value.X) < MathHelper.Epsilon);
            bool reY = (Math.Abs(Y - value.Y) < MathHelper.Epsilon);
            return (reX && reY);
        }
        /// <summary>
        /// 判断两个对象是否相等
        /// </summary>
        /// <param name="value1">Vector2i对象</param>
        /// <param name="value2">Vector2i对象</param>
        /// <returns>如果两个向量相等返回true，否则返回false</returns>        
        public static Bool2 Equals(in Vector2i value1, in Vector2i value2)
        {
            Bool2 result;
            result.X = value1.X == value2.X;
            result.Y = value1.Y == value2.Y;
            return result;
        }
        public static Bool2 Less(in Vector2i value1, in Vector2i value2)
        {
            Bool2 result;
            result.X = value1.X < value2.X;
            result.Y = value1.Y < value2.Y;
            return result;
        }
        public static Bool2 LessEqual(in Vector2i value1, in Vector2i value2)
        {
            Bool2 result;
            result.X = value1.X <= value2.X;
            result.Y = value1.Y <= value2.Y;
            return result;
        }
        public static Bool2 Great(in Vector2i value1, in Vector2i value2)
        {
            Bool2 result;
            result.X = value1.X > value2.X;
            result.Y = value1.Y > value2.Y;
            return result;
        }
        public static Bool2 GreatEqual(in Vector2i value1, in Vector2i value2)
        {
            Bool2 result;
            result.X = value1.X >= value2.X;
            result.Y = value1.Y >= value2.Y;
            return result;
        }
        public static Vector2i Mod(in Vector2i value1, in Vector2i value2)
        {
            Vector2i result;
            result.X = value1.X % value2.X;
            result.Y = value1.Y % value2.Y;
            return result;
        }
    }
}
