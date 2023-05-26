using System;
using System.Collections.Generic;

namespace EngineNS
{
    /// <summary>
    /// 四维向量结构体
    /// </summary>
    [Vector4ui.Vector4Editor]
    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential, Pack = 4)]
    [Vector4ui.TypeConverter]
    public struct Vector4ui : System.IEquatable<Vector4ui>
    {
        public class TypeConverterAttribute : Support.TypeConverterAttributeBase
        {
            public override bool ConvertFromString(ref object obj, string text)
            {
                obj = Vector4ui.FromString(text);
                return true;
            }
        }
        public class Vector4EditorAttribute : EGui.Controls.PropertyGrid.PGCustomValueEditorAttribute
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
                //ImGuiAPI.InputFloat3(TName.FromString2("##", info.Name).ToString(), (uint*)&v, "%.6f", ImGuiInputTextFlags_.ImGuiInputTextFlags_CharsDecimal);
                var minValue = uint.MinValue;
                var maxValue = uint.MaxValue;
                var multiValue = info.Value as EGui.Controls.PropertyGrid.PropertyMultiValue;
                if (multiValue != null && multiValue.HasDifferentValue())
                {
                    ImGuiAPI.Text(multiValue.MultiValueString);
                    if (multiValue.DrawVector<Vector4ui>(in info) && !info.Readonly)
                    {
                        newValue = multiValue;
                        retValue = true;
                    }
                }
                else
                {
                    var v = (Vector4ui)info.Value;
                    float speed = 0.1f;
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
                    var changed = ImGuiAPI.DragScalarN2(TName.FromString2("##", info.Name).ToString(), ImGuiDataType_.ImGuiDataType_Float, (uint*)&v, 4, speed, &minValue, &maxValue, format, ImGuiSliderFlags_.ImGuiSliderFlags_None);
                    //ImGuiAPI.PopStyleVar(1);
                    if (changed && !info.Readonly)//(v != saved)
                    {
                        newValue = v;
                        retValue = true;
                    }

                    if (OnDrawVectorValue<Vector4ui>(in info, ref v, ref v) && !info.Readonly)
                    {
                        newValue = v;
                        retValue = true;
                    }
                }
                return retValue;
            }
            public unsafe static bool OnDrawDVectorValue<T>(in EditorInfo info, ref T v, ref T newValue) where T : unmanaged
            {
                bool retValue = false;
                if (info.Expand)
                {
                    var minValue = double.MinValue;
                    var maxValue = double.MaxValue;

                    ImGuiTableRowData rowData = new ImGuiTableRowData()
                    {
                        IndentTextureId = info.HostPropertyGrid.IndentDec.GetImagePtrPointer().ToPointer(),
                        MinHeight = 0,
                        CellPaddingYEnd = info.HostPropertyGrid.EndRowPadding,
                        CellPaddingYBegin = info.HostPropertyGrid.BeginRowPadding,
                        IndentImageWidth = info.HostPropertyGrid.Indent,
                        IndentTextureUVMin = Vector2.Zero,
                        IndentTextureUVMax = Vector2.One,
                        IndentColor = info.HostPropertyGrid.IndentColor,
                        HoverColor = EGui.UIProxy.StyleConfig.Instance.PGItemHoveredColor,
                        Flags = ImGuiTableRowFlags_.ImGuiTableRowFlags_None,
                    };
                    for (var dimIdx = 0; dimIdx < sizeof(T) / sizeof(double); dimIdx++)
                    {
                        ImGuiAPI.TableNextRow(in rowData);
                        ImGuiAPI.TableSetColumnIndex(0);
                        ImGuiAPI.AlignTextToFramePadding();
                        string dimName = "";
                        switch (dimIdx)
                        {
                            case 0:
                                dimName = "X";
                                break;
                            case 1:
                                dimName = "Y";
                                break;
                            case 2:
                                dimName = "Z";
                                break;
                            case 3:
                                dimName = "W";
                                break;
                        }
                        ImGuiAPI.Indent(15);
                        ImGuiAPI.Text(dimName);
                        ImGuiAPI.Unindent(15);
                        ImGuiAPI.TableNextColumn();
                        ImGuiAPI.SetNextItemWidth(-1);
                        fixed (T* vPtr = &v)
                        {
                            var dimV = ((double*)vPtr)[dimIdx];
                            var dimVChanged = ImGuiAPI.DragScalar2(dimName, ImGuiDataType_.ImGuiDataType_Double, &dimV, 0.1f, &minValue, &maxValue, "%0.6lf", ImGuiSliderFlags_.ImGuiSliderFlags_None);
                            if (dimVChanged)
                            {
                                ((double*)vPtr)[dimIdx] = dimV;
                                newValue = v;
                                retValue = true;
                            }
                        }
                    }
                }
                return retValue;
            }

            public unsafe static bool OnDrawVectorValue<T>(in EditorInfo info, ref T v, ref T newValue) where T : unmanaged
            {
                bool retValue = false;
                if (info.Expand)
                {
                    var minValue = uint.MinValue;
                    var maxValue = uint.MaxValue;

                    ImGuiTableRowData rowData = new ImGuiTableRowData()
                    {
                        IndentTextureId = info.HostPropertyGrid.IndentDec.GetImagePtrPointer().ToPointer(),
                        MinHeight = 0,
                        CellPaddingYEnd = info.HostPropertyGrid.EndRowPadding,
                        CellPaddingYBegin = info.HostPropertyGrid.BeginRowPadding,
                        IndentImageWidth = info.HostPropertyGrid.Indent,
                        IndentTextureUVMin = Vector2.Zero,
                        IndentTextureUVMax = Vector2.One,
                        IndentColor = info.HostPropertyGrid.IndentColor,
                        HoverColor = EGui.UIProxy.StyleConfig.Instance.PGItemHoveredColor,
                        Flags = ImGuiTableRowFlags_.ImGuiTableRowFlags_None,
                    };
                    for (var dimIdx = 0; dimIdx < sizeof(T) / sizeof(uint); dimIdx++)
                    {
                        ImGuiAPI.TableNextRow(in rowData);
                        ImGuiAPI.TableSetColumnIndex(0);
                        ImGuiAPI.AlignTextToFramePadding();
                        string dimName = "";
                        switch (dimIdx)
                        {
                            case 0:
                                dimName = "X";
                                break;
                            case 1:
                                dimName = "Y";
                                break;
                            case 2:
                                dimName = "Z";
                                break;
                            case 3:
                                dimName = "W";
                                break;
                        }
                        ImGuiAPI.Indent(15);
                        ImGuiAPI.Text(dimName);
                        ImGuiAPI.Unindent(15);
                        ImGuiAPI.TableNextColumn();
                        ImGuiAPI.SetNextItemWidth(-1);
                        fixed (T* vPtr = &v)
                        {
                            var dimV = ((uint*)vPtr)[dimIdx];
                            var dimVChanged = ImGuiAPI.DragScalar2(dimName, ImGuiDataType_.ImGuiDataType_S64, &dimV, 0.1f, &minValue, &maxValue, null, ImGuiSliderFlags_.ImGuiSliderFlags_None);
                            if (dimVChanged)
                            {
                                ((uint*)vPtr)[dimIdx] = dimV;
                                newValue = v;
                                retValue = true;
                            }
                        }
                    }
                }
                return retValue;
            }
        }
        public override string ToString()
        {
            return $"{X},{Y},{Z},{W}";
        }
        public static Vector4ui FromString(string text)
        {
            try
            {
                var result = new Vector4ui();
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
                        if (j == 3)
                            break;
                    }
                }
                result[j] = uint.Parse(chars.Slice(iStart, chars.Length - iStart));
                return result;
                //var segs = text.Split(',');
                //return new Vector4ui(System.Convert.ToSingle(segs[0]),
                //    System.Convert.ToSingle(segs[1]),
                //    System.Convert.ToSingle(segs[2]),
                //    System.Convert.ToSingle(segs[3]));
            }
            catch
            {
                return new Vector4ui();
            }
        }
        public void SetValue(uint x, uint y, uint z, uint w)
        {
            X = x;
            Y = y;
            Z = z;
            W = w;
        }
        #region Member
        /// <summary>
        /// X的坐标
        /// </summary>
        [Rtti.Meta]
        public uint X;
        /// <summary>
        /// Y的坐标
        /// </summary>

        [Rtti.Meta]
        public uint Y;
        /// <summary>
        /// Z的坐标
        /// </summary>
        [Rtti.Meta]
        public uint Z;
        /// <summary>
        /// W的值
        /// </summary>
        [Rtti.Meta]
        public uint W;
        #endregion
        /// <summary>
        /// 按索引获取对象的值
        /// </summary>
        /// <param name="index">索引值</param>
        /// <returns>返回相应的对象值</returns>
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
                    case 3:
                        return W;
                    default:
                        throw new ArgumentOutOfRangeException("index", "Indices for Vector4ui run from 0 to 3, inclusive.");
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
                    case 3:
                        W = value;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException("index", "Indices for Vector4ui run from 0 to 3, inclusive.");
                }
            }
        }
        public Vector4ui(in Vector3ui v, uint w)
        {
            W = w;
            X = v.X;
            Y = v.Y;
            Z = v.Z;
        }

        #region StaticMember
        /// <summary>
        /// 只读属性，0向量
        /// </summary>
        [Rtti.Meta]
        public readonly static Vector4ui Zero = new Vector4ui(0, 0, 0, 0);
        [Rtti.Meta]
        public readonly static Vector4ui UnitX = new Vector4ui(1, 0, 0, 0);

        [Rtti.Meta]
        public readonly static Vector4ui UnitY = new Vector4ui(0, 1, 0, 0);
        [Rtti.Meta]
        public readonly static Vector4ui UnitZ = new Vector4ui(0, 0, 1, 0);
        [Rtti.Meta]
        public readonly static Vector4ui UnitW = new Vector4ui(0, 0, 0, 1);
        [Rtti.Meta]
        public readonly static Vector4ui One = new Vector4ui(1, 1, 1, 1);
        public readonly static Vector4ui MaxValue = new Vector4ui(uint.MaxValue, uint.MaxValue, uint.MaxValue, uint.MaxValue);
        public readonly static Vector4ui MinValue = new Vector4ui(uint.MinValue, uint.MinValue, uint.MinValue, uint.MinValue);
        /// <summary>
        /// 只读属性，该对象的所占内存大小
        /// </summary>
        [Rtti.Meta]
        public static int SizeInBytes
        {
            get
            {
                unsafe
                {
                    return sizeof(Vector4ui);
                }
            }
        }
        #endregion

        #region Constructure
        /// <summary>
        /// 带参构造函数
        /// </summary>
        /// <param name="value">Vector4ui对象</param>
        public Vector4ui(Vector4ui value)
        {
            X = value.X;
            Y = value.Y;
            Z = value.Z;
            W = value.W;
        }
        /// <summary>
        /// 带参构造函数
        /// </summary>
        /// <param name="value">值</param>
        public Vector4ui(uint value)
        {
            X = value;
            Y = value;
            Z = value;
            W = value;
        }

        //public Vector4ui( Vector2 value, uint z, uint w )
        //{
        //    X = value.X;
        //    Y = value.Y;
        //    Z = z;
        //    W = w;
        //}
        /// <summary>
        /// 带参构造函数
        /// </summary>
        /// <param name="value">Vector3ui对象</param>
        /// <param name="w">W值</param>
        public Vector4ui(Vector3ui value, uint w)
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
        public Vector4ui(uint x, uint y, uint z, uint w)
        {
            X = x;
            Y = y;
            Z = z;
            W = w;
        }
        #endregion

        #region Equal Override
        /// <summary>
        /// 获取哈希值
        /// </summary>
        /// <returns>返回对象的哈希值</returns>
	    public override int GetHashCode()
        {
            return X.GetHashCode() + Y.GetHashCode() + Z.GetHashCode();
        }
        /// <summary>
        /// 判断两个对象是否相等
        /// </summary>
        /// <param name="value">可转换成Vector4ui的对象</param>
        /// <returns>如果两个对象相等返回true，否则返回false</returns>
	    public override bool Equals(object value)
        {
            if (value == null)
                return false;

            if (value.GetType() != GetType())
                return false;

            return Equals((Vector4ui)(value));
        }
        /// <summary>
        /// 判断两个对象是否相等
        /// </summary>
        /// <param name="value">Vector4ui对象</param>
        /// <returns>如果两个对象相等返回true，否则返回false</returns>
	    public bool Equals(Vector4ui value)
        {
            return (X == value.X && Y == value.Y && Z == value.Z && W == value.W);
        }
        /// <summary>
        /// 判断两个对象是否相等
        /// </summary>
        /// <param name="value1">Vector4ui对象</param>
        /// <param name="value2">Vector4ui对象</param>
        /// <returns>如果两个对象相等返回true，否则返回false</returns>
	    public static Bool4 Equals(in Vector4ui value1, in Vector4ui value2)
        {
            Bool4 result;
            result.X = (Math.Abs(value1.X - value2.X) < 0);
            result.Y = (Math.Abs(value1.Y - value2.Y) < 0);
            result.Z = (Math.Abs(value1.Z - value2.Z) < 0);
            result.W = (Math.Abs(value1.W - value2.W) < 0);
            return result;
        }
        #endregion
        public static Vector4ui Minimize(in Vector4ui left, in Vector4ui right)
        {
            Vector4ui vector;
            vector.X = (left.X < right.X) ? left.X : right.X;
            vector.Y = (left.Y < right.Y) ? left.Y : right.Y;
            vector.Z = (left.Z < right.Z) ? left.Z : right.Z;
            vector.W = (left.W < right.W) ? left.W : right.W;
            return vector;
        }
        public static void Minimize(in Vector4ui left, in Vector4ui right, out Vector4ui result)
        {
            result.X = (left.X < right.X) ? left.X : right.X;
            result.Y = (left.Y < right.Y) ? left.Y : right.Y;
            result.Z = (left.Z < right.Z) ? left.Z : right.Z;
            result.W = (left.W < right.W) ? left.W : right.W;
        }
        public static Vector4ui Maximize(in Vector4ui left, in Vector4ui right)
        {
            Vector4ui vector;
            vector.X = (left.X > right.X) ? left.X : right.X;
            vector.Y = (left.Y > right.Y) ? left.Y : right.Y;
            vector.Z = (left.Z > right.Z) ? left.Z : right.Z;
            vector.W = (left.W > right.W) ? left.W : right.W;
            return vector;
        }
        public static void Maximize(in Vector4ui left, in Vector4ui right, out Vector4ui result)
        {
            result.X = (left.X > right.X) ? left.X : right.X;
            result.Y = (left.Y > right.Y) ? left.Y : right.Y;
            result.Z = (left.Z > right.Z) ? left.Z : right.Z;
            result.W = (left.W > right.W) ? left.W : right.W;
        }
        public static Vector4ui operator *(in Vector4ui left, in Vector4ui right)
        {
            Vector4ui result;
            result.X = left.X * right.X;
            result.Y = left.Y * right.Y;
            result.Z = left.Z * right.Z;
            result.W = left.W * right.W;
            return result;
        }
        public static Vector4ui operator /(in Vector4ui left, in Vector4ui right)
        {
            Vector4ui result;
            result.X = left.X / right.X;
            result.Y = left.Y / right.Y;
            result.Z = left.Z / right.Z;
            result.W = left.W / right.W;
            return result;
        }
        /// <summary>
        /// 对象的长度
        /// </summary>
        /// <returns>返回对象的长度</returns>
        [Rtti.Meta]
        public uint Length()
        {
            return (uint)(Math.Sqrt((X * X) + (Y * Y) + (Z * Z) + (W * W)));
        }
        /// <summary>
        /// 对象长度平方
        /// </summary>
        /// <returns>返回对象长度的平方</returns>
        [Rtti.Meta]
        public uint LengthSquared()
        {
            return (X * X) + (Y * Y) + (Z * Z) + (W * W);
        }
        /// <summary>
        /// 对象的单位化
        /// </summary>
        [Rtti.Meta]
        public void Normalize()
        {
            uint length = Length();
            if (length == 0)
                return;
            uint num = 1 / length;
            X *= num;
            Y *= num;
            Z *= num;
            W *= num;
        }
        /// <summary>
        /// 两个对象相加
        /// </summary>
        /// <param name="left">Vector4ui对象</param>
        /// <param name="right">Vector4ui对象</param>
        /// <returns>返回相加的结果</returns>
        [Rtti.Meta]
        public static Vector4ui Add(in Vector4ui left, in Vector4ui right)
        {
            Vector4ui result;
            result.X = left.X + right.X;
            result.Y = left.Y + right.Y;
            result.Z = left.Z + right.Z;
            result.W = left.W + right.W;
            return result;
        }
        /// <summary>
        /// 两个对象相加
        /// </summary>
        /// <param name="left">Vector4ui对象</param>
        /// <param name="right">Vector4ui对象</param>
        /// <param name="result">相加的结果</param>
        [Rtti.Meta]
        public static void Add(in Vector4ui left, in Vector4ui right, out Vector4ui result)
        {
            result.X = left.X + right.X;
            result.Y = left.Y + right.Y;
            result.Z = left.Z + right.Z;
            result.W = left.W + right.W;
        }
        /// <summary>
        /// 两个对象的差
        /// </summary>
        /// <param name="left">Vector4ui对象</param>
        /// <param name="right">Vector4ui对象</param>
        /// <returns>返回计算后的结果</returns>
        [Rtti.Meta]
        public static Vector4ui Subtract(in Vector4ui left, in Vector4ui right)
        {
            Vector4ui result;
            result.X = left.X - right.X;
            result.Y = left.Y - right.Y;
            result.Z = left.Z - right.Z;
            result.W = left.W - right.W;
            return result;
        }
        /// <summary>
        /// 两个对象的差
        /// </summary>
        /// <param name="left">Vector4ui对象</param>
        /// <param name="right">Vector4ui对象</param>
        /// <param name="result">计算后的结果</param>
        [Rtti.Meta]
        public static void Subtract(in Vector4ui left, in Vector4ui right, out Vector4ui result)
        {
            result.X = left.X - right.X;
            result.Y = left.Y - right.Y;
            result.Z = left.Z - right.Z;
            result.W = left.W - right.W;
        }
        /// <summary>
        /// 两个对象的积
        /// </summary>
        /// <param name="left">Vector4ui对象</param>
        /// <param name="right">Vector4ui对象</param>
        /// <returns>返回计算后的结果</returns>
        [Rtti.Meta]
        public static Vector4ui Modulate(in Vector4ui left, in Vector4ui right)
        {
            Vector4ui result;
            result.X = left.X * right.X;
            result.Y = left.Y * right.Y;
            result.Z = left.Z * right.Z;
            result.W = left.W * right.W;
            return result;
        }
        /// <summary>
        /// 两个对象的积
        /// </summary>
        /// <param name="left">Vector4ui对象</param>
        /// <param name="right">Vector4ui对象</param>
        /// <param name="result">计算后的结果</param>
        [Rtti.Meta]
        public static void Modulate(in Vector4ui left, in Vector4ui right, out Vector4ui result)
        {
            result.X = left.X * right.X;
            result.Y = left.Y * right.Y;
            result.Z = left.Z * right.Z;
            result.W = left.W * right.W;
        }
        /// <summary>
        /// 向量的数乘
        /// </summary>
        /// <param name="value">Vector4ui对象</param>
        /// <param name="scale">缩放值</param>
        /// <returns>返回计算后的结果</returns>
        [Rtti.Meta]
        public static Vector4ui Multiply(in Vector4ui value, uint scale)
        {
            Vector4ui result;
            result.X = value.X * scale;
            result.Y = value.Y * scale;
            result.Z = value.Z * scale;
            result.W = value.W * scale;
            return result;
        }
        /// <summary>
        /// 向量的数乘
        /// </summary>
        /// <param name="value">Vector4ui对象</param>
        /// <param name="scale">缩放值</param>
        /// <param name="result">计算后的结果</param>
        [Rtti.Meta]
        public static void Multiply(in Vector4ui value, uint scale, out Vector4ui result)
        {
            result.X = value.X * scale;
            result.Y = value.Y * scale;
            result.Z = value.Z * scale;
            result.W = value.W * scale;
        }
        /// <summary>
        /// 向量与常数的商
        /// </summary>
        /// <param name="value">Vector4ui对象</param>
        /// <param name="scale">缩放值</param>
        /// <returns>返回计算后的结果</returns>
        [Rtti.Meta]
        public static Vector4ui Divide(in Vector4ui value, uint scale)
        {
            Vector4ui result;
            result.X = value.X / scale;
            result.Y = value.Y / scale;
            result.Z = value.Z / scale;
            result.W = value.W / scale;
            return result;
        }
        /// <summary>
        /// 向量与常数的商
        /// </summary>
        /// <param name="value">Vector4ui对象</param>
        /// <param name="scale">缩放值</param>
        /// <param name="result">计算后的结果</param>
        [Rtti.Meta]
        public static void Divide(in Vector4ui value, uint scale, out Vector4ui result)
        {
            result.X = value.X / scale;
            result.Y = value.Y / scale;
            result.Z = value.Z / scale;
            result.W = value.W / scale;
        }
        /// <summary>
        /// 计算质心
        /// </summary>
        /// <param name="value1">Vector4ui对象坐标</param>
        /// <param name="value2">Vector4ui对象坐标</param>
        /// <param name="value3">Vector4ui对象坐标</param>
        /// <param name="amount1">参数</param>
        /// <param name="amount2">参数</param>
        /// <returns>返回计算后的结果</returns>
        [Rtti.Meta]
        public static Vector4ui Barycentric(in Vector4ui value1, in Vector4ui value2, in Vector4ui value3, uint amount1, uint amount2)
        {
            Vector4ui vector;
            vector.X = (value1.X + (amount1 * (value2.X - value1.X))) + (amount2 * (value3.X - value1.X));
            vector.Y = (value1.Y + (amount1 * (value2.Y - value1.Y))) + (amount2 * (value3.Y - value1.Y));
            vector.Z = (value1.Z + (amount1 * (value2.Z - value1.Z))) + (amount2 * (value3.Z - value1.Z));
            vector.W = (value1.W + (amount1 * (value2.W - value1.W))) + (amount2 * (value3.W - value1.W));
            return vector;
        }
        /// <summary>
        /// 计算质心
        /// </summary>
        /// <param name="value1">Vector4ui对象坐标</param>
        /// <param name="value2">Vector4ui对象坐标</param>
        /// <param name="value3">Vector4ui对象坐标</param>
        /// <param name="amount1">参数</param>
        /// <param name="amount2">参数</param>
        /// <param name="result">计算后的结果</param>
        [Rtti.Meta]
        public static void Barycentric(in Vector4ui value1, in Vector4ui value2, in Vector4ui value3, uint amount1, uint amount2, out Vector4ui result)
        {
            result.X = (value1.X + (amount1 * (value2.X - value1.X))) + (amount2 * (value3.X - value1.X));
            result.Y = (value1.Y + (amount1 * (value2.Y - value1.Y))) + (amount2 * (value3.Y - value1.Y));
            result.Z = (value1.Z + (amount1 * (value2.Z - value1.Z))) + (amount2 * (value3.Z - value1.Z));
            result.W = (value1.W + (amount1 * (value2.W - value1.W))) + (amount2 * (value3.W - value1.W));
        }

        /// <summary>
        /// 载体计算
        /// </summary>
        /// <param name="value">Vector4ui对象坐标</param>
        /// <param name="min">Vector4ui对象的最小值</param>
        /// <param name="max">Vector4ui对象的最大值</param>
        /// <returns>返回计算后的结果</returns>
        [Rtti.Meta]
        public static Vector4ui Clamp(in Vector4ui value, in Vector4ui min, in Vector4ui max)
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

            uint w = value.W;
            w = (w > max.W) ? max.W : w;
            w = (w < min.W) ? min.W : w;

            Vector4ui result;
            result.X = x;
            result.Y = y;
            result.Z = z;
            result.W = w;
            return result;
        }
        /// <summary>
        /// 载体计算
        /// </summary>
        /// <param name="value">Vector4ui对象坐标</param>
        /// <param name="min">Vector4ui对象的最小值</param>
        /// <param name="max">Vector4ui对象的最大值</param>
        /// <param name="result">计算后的结果</param>
        [Rtti.Meta]
        public static void Clamp(in Vector4ui value, in Vector4ui min, in Vector4ui max, out Vector4ui result)
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

            uint w = value.W;
            w = (w > max.W) ? max.W : w;
            w = (w < min.W) ? min.W : w;

            result.X = x;
            result.Y = y;
            result.Z = z;
            result.W = w;
        }

        /// <summary>
        /// 计算线性插值
        /// </summary>
        /// <param name="start">起点坐标</param>
        /// <param name="end">终点坐标</param>
        /// <param name="factor">插值因子</param>
        /// <returns>返回计算后的向量</returns>
        [Rtti.Meta]
        public static Vector4ui Lerp(in Vector4ui start, in Vector4ui end, uint factor)
        {
            Vector4ui vector;

            vector.X = start.X + ((end.X - start.X) * factor);
            vector.Y = start.Y + ((end.Y - start.Y) * factor);
            vector.Z = start.Z + ((end.Z - start.Z) * factor);
            vector.W = start.W + ((end.W - start.W) * factor);

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
        public static void Lerp(in Vector4ui start, in Vector4ui end, uint factor, out Vector4ui result)
        {
            result.X = start.X + ((end.X - start.X) * factor);
            result.Y = start.Y + ((end.Y - start.Y) * factor);
            result.Z = start.Z + ((end.Z - start.Z) * factor);
            result.W = start.W + ((end.W - start.W) * factor);
        }

        /// <summary>
        /// 计算两点间的距离
        /// </summary>
        /// <param name="value1">坐标点</param>
        /// <param name="value2">坐标点</param>
        /// <returns>返回两点间的距离</returns>
        [Rtti.Meta]
        public static uint Distance(in Vector4ui value1, in Vector4ui value2)
        {
            uint x = value1.X - value2.X;
            uint y = value1.Y - value2.Y;
            uint z = value1.Z - value2.Z;
            uint w = value1.W - value2.W;

            return (uint)(Math.Sqrt((x * x) + (y * y) + (z * z) + (w * w)));
        }
        /// <summary>
        /// 计算两点间的距离的平方
        /// </summary>
        /// <param name="value1">坐标点</param>
        /// <param name="value2">坐标点</param>
        /// <returns>返回两点间的距离的平方</returns>
        [Rtti.Meta]
        public static uint DistanceSquared(in Vector4ui value1, in Vector4ui value2)
        {
            uint x = value1.X - value2.X;
            uint y = value1.Y - value2.Y;
            uint z = value1.Z - value2.Z;
            uint w = value1.W - value2.W;

            return (x * x) + (y * y) + (z * z) + (w * w);
        }
        /// <summary>
        /// 向量的点积
        /// </summary>
        /// <param name="left">向量对象</param>
        /// <param name="right">对象向量</param>
        /// <returns>返回点积值</returns>
        [Rtti.Meta]
        public static uint Dot(in Vector4ui left, in Vector4ui right)
        {
            return (left.X * right.X + left.Y * right.Y + left.Z * right.Z + left.W * right.W);
        }
        /// <summary>
        /// 向量的单位化
        /// </summary>
        /// <param name="vector">向量对象</param>
        /// <returns>返回单位向量</returns>
        [Rtti.Meta]
        public static Vector4ui Normalize(in Vector4ui vector)
        {
            vector.Normalize();
            return vector;
        }
        /// <summary>
        /// 向量的单位化
        /// </summary>
        /// <param name="vector">向量对象</param>
        /// <param name="result">单位向量</param>
        [Rtti.Meta]
        public static void Normalize(in Vector4ui vector, out Vector4ui result)
        {
            result = vector;
            result.Normalize();
        }

        /// <summary>
        /// 重载"+"号运算符
        /// </summary>
        /// <param name="left">向量对象</param>
        /// <param name="right">向量对象</param>
        /// <returns>返回计算后的向量对象</returns>
        public static Vector4ui operator +(in Vector4ui left, in Vector4ui right)
        {
            Vector4ui result;
            result.X = left.X + right.X;
            result.Y = left.Y + right.Y;
            result.Z = left.Z + right.Z;
            result.W = left.W + right.W;
            return result;
        }
        /// <summary>
        /// 重载"-"号运算符
        /// </summary>
        /// <param name="left">向量对象</param>
        /// <param name="right">向量对象</param>
        /// <returns>返回计算后的向量对象</returns>
        public static Vector4ui operator -(in Vector4ui left, in Vector4ui right)
        {
            Vector4ui result;
            result.X = left.X - right.X;
            result.Y = left.Y - right.Y;
            result.Z = left.Z - right.Z;
            result.W = left.W - right.W;
            return result;
        }
        /// <summary>
        /// 重载"*"号运算符
        /// </summary>
        /// <param name="value">向量对象</param>
        /// <param name="scale">缩放常数</param>
        /// <returns>返回计算后的向量对象</returns>
        public static Vector4ui operator *(in Vector4ui value, uint scale)
        {
            Vector4ui result;
            result.X = value.X * scale;
            result.Y = value.Y * scale;
            result.Z = value.Z * scale;
            result.W = value.W * scale;
            return result;
        }
        /// <summary>
        /// 重载"*"号运算符
        /// </summary>
        /// <param name="scale">缩放常数</param>
        /// <param name="vec">向量对象</param>
        /// <returns>返回计算后的向量对象</returns>
        public static Vector4ui operator *(uint scale, Vector4ui vec)
        {
            return vec * scale;
        }
        /// <summary>
        /// 重载"/"号运算符
        /// </summary>
        /// <param name="value">向量对象</param>
        /// <param name="scale">缩放常数</param>
        /// <returns>返回计算后的向量对象</returns>
        public static Vector4ui operator /(Vector4ui value, uint scale)
        {
            Vector4ui result;
            result.X = value.X / scale;
            result.Y = value.Y / scale;
            result.Z = value.Z / scale;
            result.W = value.W / scale;
            return result;
        }
        /// <summary>
        /// 重载"=="号运算符
        /// </summary>
        /// <param name="left">向量对象</param>
        /// <param name="right">向量对象</param>
        /// <returns>如果两个向量相等返回true，否则返回false</returns>
	    public static bool operator ==(in Vector4ui left, in Vector4ui right)
        {
            return Equals(left, right).All();
        }
        /// <summary>
        /// 重载"!="号运算符
        /// </summary>
        /// <param name="left">向量对象</param>
        /// <param name="right">向量对象</param>
        /// <returns>如果两个向量不相等返回true，否则返回false</returns>
	    public static bool operator !=(in Vector4ui left, in Vector4ui right)
        {
            return !Equals(left, right).All();
        }
        public static Bool4 Less(in Vector4ui value1, in Vector4ui value2)
        {
            Bool4 result;
            result.X = value1.X < value2.X;
            result.Y = value1.Y < value2.Y;
            result.Z = value1.Z < value2.Z;
            result.W = value1.W < value2.W;
            return result;
        }
        public static Bool4 LessEqual(in Vector4ui value1, in Vector4ui value2)
        {
            Bool4 result;
            result.X = value1.X <= value2.X;
            result.Y = value1.Y <= value2.Y;
            result.Z = value1.Z <= value2.Z;
            result.W = value1.W <= value2.W;
            return result;
        }
        public static Bool4 Great(in Vector4ui value1, in Vector4ui value2)
        {
            Bool4 result;
            result.X = value1.X > value2.X;
            result.Y = value1.Y > value2.Y;
            result.Z = value1.Z > value2.Z;
            result.W = value1.W > value2.W;
            return result;
        }
        public static Bool4 GreatEqual(in Vector4ui value1, in Vector4ui value2)
        {
            Bool4 result;
            result.X = value1.X >= value2.X;
            result.Y = value1.Y >= value2.Y;
            result.Z = value1.Z >= value2.Z;
            result.W = value1.W >= value2.W;
            return result;
        }
    }
}
