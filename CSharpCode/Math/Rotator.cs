using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS
{
    [FRotator.FRotatorEditor]
    [FRotator.TypeConverter]
    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
    public struct FRotator : System.IEquatable<FRotator>
    {
        public float Yaw;
        public float Pitch;
        public float Roll;

        public const float R2D = 180.0f / MathF.PI;
        public const float D2R = MathF.PI / 180.0f;
        public float YawDegree
        {
            get
            {
                return Yaw * R2D;
            }
            set
            {
                Yaw = value * D2R;
            }
        }
        public float PitchDegree
        {
            get
            {
                return Pitch * R2D;
            }
            set
            {
                Pitch = value * D2R;
            }
        }
        public float RollDegree
        {
            get
            {
                return Roll * R2D;
            }
            set
            {
                Roll = value * D2R;
            }
        }

        #region editor attributes
        public class FRotatorEditorAttribute : EGui.Controls.PropertyGrid.PGCustomValueEditorAttribute
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
                var minValue = float.MinValue;
                var maxValue = float.MaxValue;
                var multiValue = info.Value as EGui.Controls.PropertyGrid.PropertyMultiValue;
                if (multiValue != null && multiValue.HasDifferentValue())
                {
                    ImGuiAPI.Text(multiValue.MultiValueString);
                    if (multiValue.DrawVector<FRotator>(in info, "Yaw", "Pitch", "Roll", "W", (ref FRotator target, int index, ref float v, bool bSet) =>
                    {
                        if(bSet)
                        {
                            switch (index)
                            {
                                case 0:
                                    target.YawDegree = v;
                                    break;
                                case 1:
                                    target.PitchDegree = v;
                                    break;
                                case 2:
                                    target.RollDegree = v;
                                    break;
                            }
                        }
                        else
                        {
                            switch (index)
                            {
                                case 0:
                                    v = target.YawDegree;
                                    break;
                                case 1:
                                    v = target.PitchDegree;
                                    break;
                                case 2:
                                    v = target.RollDegree;
                                    break;
                            }
                        }
                    }) && !info.Readonly)
                    {
                        newValue = multiValue;
                        retValue = true;
                    }
                }
                else
                {
                    var v = (FRotator)info.Value;
                    Vector3 rv;
                    rv.X = v.YawDegree;
                    rv.Y = v.PitchDegree;
                    rv.Z = v.RollDegree;
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
                    var changed = ImGuiAPI.DragScalarN2(TName.FromString2("##", info.Name).ToString(), ImGuiDataType_.ImGuiDataType_Float, (float*)&rv, 3, speed, &minValue, &maxValue, format, ImGuiSliderFlags_.ImGuiSliderFlags_None);
                    //ImGuiAPI.InputFloat3(TName.FromString2("##", info.Name).ToString(), (float*)&v, "%.6f", ImGuiInputTextFlags_.ImGuiInputTextFlags_CharsDecimal);
                    //ImGuiAPI.PopStyleVar(1);
                    if (changed && !info.Readonly)//(v != saved)
                    {
                        v.YawDegree = rv.X;
                        v.PitchDegree = rv.Y;
                        v.Roll = rv.Z;
                        newValue = v;
                        retValue = true;
                    }

                    if (Vector4.Vector4EditorAttribute.OnDrawVectorValue<FRotator>(in info, ref v, ref v, "Yaw", "Pitch", "Roll", "W", 
                        (ref FRotator target, int index, ref float v, bool bSet) =>
                        {
                            if (bSet)
                            {
                                switch (index)
                                {
                                    case 0:
                                        target.YawDegree = v;
                                        break;
                                    case 1:
                                        target.PitchDegree = v;
                                        break;
                                    case 2:
                                        target.RollDegree = v;
                                        break;
                                }
                            }
                            else
                            {
                                switch (index)
                                {
                                    case 0:
                                        v = target.YawDegree;
                                        break;
                                    case 1:
                                        v = target.PitchDegree;
                                        break;
                                    case 2:
                                        v = target.RollDegree;
                                        break;
                                }
                            }
                        }) && !info.Readonly)
                    {
                        newValue = v;
                        retValue = true;
                    }

                }
                return retValue;
            }
        }
        public class TypeConverterAttribute : Support.TypeConverterAttributeBase
        {
            public override bool ConvertFromString(ref object obj, string text)
            {
                obj = FRotator.FromString(text);
                return true;
            }
        }
        #endregion
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
