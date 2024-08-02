using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Editor
{
    [FloatRange.FloatRangeEditor]
    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential, Pack = 4)]
    public struct FloatRange : System.IEquatable<FloatRange>
    {
        #region attribute
        public class TypeConverterAttribute : Support.TypeConverterAttributeBase
        {
            public override bool ConvertFromString(ref object obj, string text)
            {
                obj = Vector2.FromString(text);
                return true;
            }
        }
        public class FloatRangeEditorAttribute : EGui.Controls.PropertyGrid.PGCustomValueEditorAttribute
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
                var minValue = float.MinValue;
                var maxValue = float.MaxValue;
                var name = TName.FromString2("##", info.Name).ToString();
                var multiValue = info.Value as EGui.Controls.PropertyGrid.PropertyMultiValue;
                bool retValue = false;
                if (multiValue != null && multiValue.HasDifferentValue())
                {
                    ImGuiAPI.Text(multiValue.MultiValueString);
                    if (multiValue.DrawVector<Vector2>(in info, "Min", "Range") && !info.Readonly)
                    {
                        newValue = multiValue;
                        retValue = true;
                    }
                }
                else
                {
                    var v = (Vector2)info.Value;
                    float speed = 0.1f;
                    var format = "%.6f";
                    if (info.HostProperty != null)
                    {
                        var vR = info.HostProperty.GetAttribute<EGui.Controls.PropertyGrid.PGValueRange>();
                        if (vR != null)
                        {
                            minValue = (float)vR.Min;
                            maxValue = (float)vR.Max;
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
                    var changed = ImGuiAPI.DragScalarN2(name, ImGuiDataType_.ImGuiDataType_Float, (float*)&v, 2, speed, &minValue, &maxValue, format, ImGuiSliderFlags_.ImGuiSliderFlags_None);
                    //ImGuiAPI.InputFloat2(, (float*)&v, "%.6f", ImGuiInputTextFlags_.ImGuiInputTextFlags_CharsDecimal);
                    //ImGuiAPI.PopStyleVar(1);
                    if (changed && !info.Readonly)//(v != saved)
                    {
                        newValue = v;
                        retValue = true;
                    }

                    if (Vector4.Vector4EditorAttribute.OnDrawVectorValue<Vector2>(in info, ref v, ref v, "Min", "Range") && !info.Readonly)
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
            return $"{Min},{Range}";
        }
        public static FloatRange FromString(string text)
        {
            try
            {
                FloatRange result = new FloatRange();
                ReadOnlySpan<char> chars = text.ToCharArray();
                var pos = chars.IndexOf(',');
                result.Min = float.Parse(chars.Slice(0, pos));
                result.Range = float.Parse(chars.Slice(pos + 1, chars.Length - pos - 1));
                return result;
                //var segs = text.Split(',');
                //return new Vector2(System.Convert.ToSingle(segs[0]),
                //    System.Convert.ToSingle(segs[1]));
            }
            catch
            {
                return new FloatRange();
            }
        }
        #endregion
        public float Min;
        public float Range;
        public float GetValue(float factor)
        {
            return Min + Range * factor;
        }
        #region Equal Override
        /// <summary>
        /// 获取对象的哈希值
        /// </summary>
        /// <returns>返回对象的哈希值</returns>
	    public override int GetHashCode()
        {
            return Min.GetHashCode() + Range.GetHashCode();
        }
	    public override bool Equals(object value)
        {
            if (value == null)
                return false;

            if (value.GetType() != GetType())
                return false;

            return Equals((FloatRange)(value));
        }
	    public bool Equals(FloatRange value)
        {
            return (Min == value.Min && Range == value.Range);
        }
	    public static bool Equals(in FloatRange value1, in FloatRange value2, float epsilon = MathHelper.Epsilon)
        {
            bool reX = (Math.Abs(value1.Min - value2.Min) < MathHelper.Epsilon);
            bool reY = (Math.Abs(value1.Range - value2.Range) < MathHelper.Epsilon);
            return (reX && reY);
        }
        public static bool operator ==(in FloatRange left, in FloatRange right)
        {
            return left.Equals(right);
            //return Equals( left, right );
        }
        public static bool operator !=(in FloatRange left, in FloatRange right)
        {
            return !left.Equals(right);
            //return !Equals( left, right );
        }
        #endregion
    }
}
