using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS
{
    [DVector3.DVector3Editor]
    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential, Pack = 8)]
    public struct DVector3 : System.IEquatable<DVector3>
    {
        public class DVector3EditorAttribute : EGui.Controls.PropertyGrid.PGCustomValueEditorAttribute
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
                var minValue = double.MinValue;
                var maxValue = double.MaxValue;
                var multiValue = info.Value as EGui.Controls.PropertyGrid.PropertyMultiValue;
                if (multiValue != null && multiValue.HasDifferentValue())
                {
                    ImGuiAPI.Text(multiValue.MultiValueString);
                    if (multiValue.DrawDVector<DVector3>(in info) && !info.Readonly)
                    {
                        newValue = multiValue;
                        retValue = true;
                    }
                }
                else
                {
                    var v = (DVector3)info.Value;
                    var changed = ImGuiAPI.DragScalarN2(TName.FromString2("##", info.Name).ToString(), ImGuiDataType_.ImGuiDataType_Double, (double*)&v, 3, 0.1f, &minValue, &maxValue, "%0.6lf", ImGuiSliderFlags_.ImGuiSliderFlags_None);
                    //ImGuiAPI.InputFloat3(TName.FromString2("##", info.Name).ToString(), (float*)&v, "%.6f", ImGuiInputTextFlags_.ImGuiInputTextFlags_CharsDecimal);
                    //ImGuiAPI.PopStyleVar(1);
                    if (changed && !info.Readonly)//(v != saved)
                    {
                        newValue = new DVector3(v.X, v.Y, v.Z);
                        retValue = true;
                    }

                    if (Vector4.Vector4EditorAttribute.OnDrawDVectorValue<DVector3>(in info, ref v, ref v) && !info.Readonly)
                    {
                        newValue = v;
                        retValue = true;
                    }
                }
                return retValue;
            }
        }

        public readonly static DVector3 Zero = new DVector3(0, 0, 0);
        public readonly static DVector3 UnitXYZ = new DVector3(1, 1, 1);
        public readonly static DVector3 UnitX = new DVector3(1, 0, 0);
        public readonly static DVector3 UnitY = new DVector3(0, 1, 0);
        public readonly static DVector3 UnitZ = new DVector3(0, 0, 1);

        public readonly static DVector3 One = new DVector3(1f, 1f, 1f);
        public readonly static DVector3 Up = new DVector3(0f, 1f, 0f);
        public readonly static DVector3 Down = new DVector3(0f, -1f, 0f);
        public readonly static DVector3 Right = new DVector3(1f, 0f, 0f);
        public readonly static DVector3 Left = new DVector3(-1f, 0f, 0f);
        public readonly static DVector3 Forward = new DVector3(0f, 0f, -1f);
        public readonly static DVector3 Backward = new DVector3(0f, 0f, 1f);

        public readonly static DVector3 MaxValue = new DVector3(double.MaxValue, double.MaxValue, double.MaxValue);
        public readonly static DVector3 MinValue = new DVector3(double.MinValue, double.MinValue, double.MinValue);
        public double X;
        public double Y;
        public double Z;
        public DVector3(double x, double y, double z)
        {
            X = x;
            Y = y;
            Z = z;
        }
        public DVector3(in DVector3 value)
        {
            X = value.X;
            Y = value.Y;
            Z = value.Z;
        }
        public DVector3(double value)
        {
            X = value;
            Y = value;
            Z = value;
        }
        public Vector3 ToSingleVector3()
        {
            return new Vector3((float)X, (float)Y, (float)Z);
        }
        public DVector3 AbsVector()
        {
            return new DVector3(Math.Abs(X), Math.Abs(Y), Math.Abs(Z));
        }
        public override string ToString()
        {
            return $"{X},{Y},{Z}";
            //return string.Format(System.Globalization.CultureInfo.CurrentCulture, "X:{0} Y:{1} Z:{2}", 
            //    X.ToString(System.Globalization.CultureInfo.CurrentCulture), 
            //    Y.ToString(System.Globalization.CultureInfo.CurrentCulture), 
            //    Z.ToString(System.Globalization.CultureInfo.CurrentCulture));
        }
        public static DVector3 FromString(string text)
        {
            try
            {
                var segs = text.Split(',');
                return new DVector3(System.Convert.ToDouble(segs[0]),
                    System.Convert.ToDouble(segs[1]),
                    System.Convert.ToDouble(segs[2]));
            }
            catch
            {
                return DVector3.Zero;
            }
        }
        public override int GetHashCode()
        {
            return X.GetHashCode() + Y.GetHashCode() + Z.GetHashCode();
        }
        public override bool Equals(object value)
        {
            if (value == null)
                return false;

            if (value.GetType() != GetType())
                return false;

            return Equals((DVector3)value);
        }
        public bool HasNagative()
        {
            if (X < 0 || Y < 0 || Z < 0)
                return true;
            return false;
        }
        public double Length()
        {
            return (System.Math.Sqrt((X * X) + (Y * Y) + (Z * Z)));
        }
        public double LengthSquared()
        {
            return (X * X) + (Y * Y) + (Z * Z);
        }
        public void Normalize()
        {
            var length = Length();
            if (length == 0)
                return;
            var num = 1 / length;
            X *= num;
            Y *= num;
            Z *= num;
        }
        public static DVector3 operator +(in DVector3 left, in DVector3 right)
        {
            DVector3 result;
            result.X = left.X + right.X;
            result.Y = left.Y + right.Y;
            result.Z = left.Z + right.Z;
            return result;
        }
        public static DVector3 operator +(in DVector3 left, in Vector3 right)
        {
            DVector3 result;
            result.X = left.X + right.X;
            result.Y = left.Y + right.Y;
            result.Z = left.Z + right.Z;
            return result;
        }
        public static DVector3 operator -(in DVector3 left, in DVector3 right)
        {
            DVector3 result;
            result.X = left.X - right.X;
            result.Y = left.Y - right.Y;
            result.Z = left.Z - right.Z;
            return result;
        }
        public static DVector3 operator -(in DVector3 left, in Vector3 right)
        {
            DVector3 result;
            result.X = left.X - right.X;
            result.Y = left.Y - right.Y;
            result.Z = left.Z - right.Z;
            return result;
        }
        public static DVector3 operator -(DVector3 value)
        {
            DVector3 result;
            result.X = -value.X;
            result.Y = -value.Y;
            result.Z = -value.Z;
            return result;
        }
        public static DVector3 operator *(in DVector3 value, double scale)
        {
            DVector3 result;
            result.X = value.X * scale;
            result.Y = value.Y * scale;
            result.Z = value.Z * scale;
            return result;
        }
        public static DVector3 operator *(in DVector3 left, in DVector3 right)
        {
            DVector3 result;
            result.X = left.X * right.X;
            result.Y = left.Y * right.Y;
            result.Z = left.Z * right.Z;
            return result;
        }
        public static DVector3 operator *(in DVector3 left, in Vector3 right)
        {
            DVector3 result;
            result.X = left.X * right.X;
            result.Y = left.Y * right.Y;
            result.Z = left.Z * right.Z;
            return result;
        }
        public static DVector3 operator *(double scale, in DVector3 vec)
        {
            return vec * scale;
        }
        public static DVector3 operator /(in DVector3 value, double scale)
        {
            DVector3 result;
            result.X = value.X / scale;
            result.Y = value.Y / scale;
            result.Z = value.Z / scale;
            return result;
        }
        public static DVector3 operator /(in DVector3 left, in DVector3 right)
        {
            DVector3 result;
            result.X = left.X / right.X;
            result.Y = left.Y / right.Y;
            result.Z = left.Z / right.Z;
            return result;
        }
        public static DVector3 operator /(in DVector3 left, in Vector3 right)
        {
            DVector3 result;
            result.X = left.X / right.X;
            result.Y = left.Y / right.Y;
            result.Z = left.Z / right.Z;
            return result;
        }
        public static bool operator ==(in DVector3 left, in DVector3 right)
        {
            return left.Equals(right);
            //return Equals(ref left, ref right );
        }
        public static bool operator !=(in DVector3 left, in DVector3 right)
        {
            return !left.Equals(right);
            //return !Equals(ref left, ref right );
        }
        public static DVector3 Minimize(in DVector3 left, in DVector3 right)
        {
            DVector3 vector;
            vector.X = (left.X < right.X) ? left.X : right.X;
            vector.Y = (left.Y < right.Y) ? left.Y : right.Y;
            vector.Z = (left.Z < right.Z) ? left.Z : right.Z;
            return vector;
        }
        public static DVector3 Maximize(in DVector3 left, in DVector3 right)
        {
            DVector3 vector;
            vector.X = (left.X > right.X) ? left.X : right.X;
            vector.Y = (left.Y > right.Y) ? left.Y : right.Y;
            vector.Z = (left.Z > right.Z) ? left.Z : right.Z;
            return vector;
        }
        public static DVector3 Add(in DVector3 left, in DVector3 right)
        {
            DVector3 result;
            result.X = left.X + right.X;
            result.Y = left.Y + right.Y;
            result.Z = left.Z + right.Z;
            return result;
        }
        public static void Add(in DVector3 left, in DVector3 right, out DVector3 result)
        {
            result.X = left.X + right.X;
            result.Y = left.Y + right.Y;
            result.Z = left.Z + right.Z;
        }
        public static DVector3 Subtract(in DVector3 left, in DVector3 right)
        {
            DVector3 result;
            result.X = left.X - right.X;
            result.Y = left.Y - right.Y;
            result.Z = left.Z - right.Z;
            return result;
        }
        public static void Subtract(in DVector3 left, in DVector3 right, out DVector3 result)
        {
            result.X = left.X - right.X;
            result.Y = left.Y - right.Y;
            result.Z = left.Z - right.Z;
        }
        public static DVector3 Modulate(in DVector3 left, in DVector3 right)
        {
            DVector3 result;
            result.X = left.X * right.X;
            result.Y = left.Y * right.Y;
            result.Z = left.Z * right.Z;
            return result;
        }
        public static void Modulate(in DVector3 left, in DVector3 right, out DVector3 result)
        {
            result.X = left.X * right.X;
            result.Y = left.Y * right.Y;
            result.Z = left.Z * right.Z;
        }
        public static DVector3 Multiply(in DVector3 value, float scale)
        {
            DVector3 result;
            result.X = value.X * scale;
            result.Y = value.Y * scale;
            result.Z = value.Z * scale;
            return result;
        }
        public static void Multiply(in DVector3 value, double scale, out DVector3 result)
        {
            result.X = value.X * scale;
            result.Y = value.Y * scale;
            result.Z = value.Z * scale;
        }
        public static DVector3 Divide(in DVector3 value, double scale)
        {
            DVector3 result;
            result.X = value.X / scale;
            result.Y = value.Y / scale;
            result.Z = value.Z / scale;
            return result;
        }
        public static void Divide(in DVector3 value, double scale, out DVector3 result)
        {
            result.X = value.X / scale;
            result.Y = value.Y / scale;
            result.Z = value.Z / scale;
        }
        public static DVector3 Negate(in DVector3 value)
        {
            DVector3 result;
            result.X = -value.X;
            result.Y = -value.Y;
            result.Z = -value.Z;
            return result;
        }
        public static void Negate(in DVector3 value, out DVector3 result)
        {
            result.X = -value.X;
            result.Y = -value.Y;
            result.Z = -value.Z;
        }
        public static DVector3 Barycentric(in DVector3 value1, in DVector3 value2, in DVector3 value3, double amount1, double amount2)
        {
            DVector3 vector;
            vector.X = (value1.X + (amount1 * (value2.X - value1.X))) + (amount2 * (value3.X - value1.X));
            vector.Y = (value1.Y + (amount1 * (value2.Y - value1.Y))) + (amount2 * (value3.Y - value1.Y));
            vector.Z = (value1.Z + (amount1 * (value2.Z - value1.Z))) + (amount2 * (value3.Z - value1.Z));
            return vector;
        }
        public static void Barycentric(in DVector3 value1, in DVector3 value2, in DVector3 value3, double amount1, double amount2, out DVector3 result)
        {
            result.X = (value1.X + (amount1 * (value2.X - value1.X))) + (amount2 * (value3.X - value1.X));
            result.Y = (value1.Y + (amount1 * (value2.Y - value1.Y))) + (amount2 * (value3.Y - value1.Y));
            result.Z = (value1.Z + (amount1 * (value2.Z - value1.Z))) + (amount2 * (value3.Z - value1.Z));
        }
        public static DVector3 Clamp(in DVector3 value, in DVector3 min, in DVector3 max)
        {
            var x = value.X;
            x = (x > max.X) ? max.X : x;
            x = (x < min.X) ? min.X : x;

            var y = value.Y;
            y = (y > max.Y) ? max.Y : y;
            y = (y < min.Y) ? min.Y : y;

            var z = value.Z;
            z = (z > max.Z) ? max.Z : z;
            z = (z < min.Z) ? min.Z : z;

            DVector3 result;
            result.X = x;
            result.Y = y;
            result.Z = z;
            return result;
        }
        public static void Clamp(in DVector3 value, in DVector3 min, in DVector3 max, out DVector3 result)
        {
            var x = value.X;
            x = (x > max.X) ? max.X : x;
            x = (x < min.X) ? min.X : x;

            var y = value.Y;
            y = (y > max.Y) ? max.Y : y;
            y = (y < min.Y) ? min.Y : y;

            var z = value.Z;
            z = (z > max.Z) ? max.Z : z;
            z = (z < min.Z) ? min.Z : z;

            result.X = x;
            result.Y = y;
            result.Z = z;
        }
        public static DVector3 Hermite(in DVector3 value1, in DVector3 tangent1, in DVector3 value2, in DVector3 tangent2, float amount)
        {
            DVector3 vector;
            var squared = amount * amount;
            var cubed = amount * squared;
            var part1 = ((2.0d * cubed) - (3.0d * squared)) + 1.0d;
            var part2 = (-2.0d * cubed) + (3.0d * squared);
            var part3 = (cubed - (2.0d * squared)) + amount;
            var part4 = cubed - squared;

            vector.X = (((value1.X * part1) + (value2.X * part2)) + (tangent1.X * part3)) + (tangent2.X * part4);
            vector.Y = (((value1.Y * part1) + (value2.Y * part2)) + (tangent1.Y * part3)) + (tangent2.Y * part4);
            vector.Z = (((value1.Z * part1) + (value2.Z * part2)) + (tangent1.Z * part3)) + (tangent2.Z * part4);

            return vector;
        }
        public static DVector3 Lerp(in DVector3 start, in DVector3 end, double factor)
        {
            DVector3 vector;

            vector.X = start.X + ((end.X - start.X) * factor);
            vector.Y = start.Y + ((end.Y - start.Y) * factor);
            vector.Z = start.Z + ((end.Z - start.Z) * factor);

            return vector;
        }
        public static void Lerp(in DVector3 start, in DVector3 end, double factor, out DVector3 result)
        {
            result.X = start.X + ((end.X - start.X) * factor);
            result.Y = start.Y + ((end.Y - start.Y) * factor);
            result.Z = start.Z + ((end.Z - start.Z) * factor);
        }
        public static DVector3 SmoothStep(in DVector3 start, in DVector3 end, float amount)
        {
            DVector3 vector;

            amount = (amount > 1.0f) ? 1.0f : ((amount < 0.0f) ? 0.0f : amount);
            amount = (amount * amount) * (3.0f - (2.0f * amount));

            vector.X = start.X + ((end.X - start.X) * amount);
            vector.Y = start.Y + ((end.Y - start.Y) * amount);
            vector.Z = start.Z + ((end.Z - start.Z) * amount);

            return vector;
        }
        public static double Distance(in DVector3 value1, in DVector3 value2)
        {
            var x = value1.X - value2.X;
            var y = value1.Y - value2.Y;
            var z = value1.Z - value2.Z;

            return (Math.Sqrt((x * x) + (y * y) + (z * z)));
        }
        public static double DistanceSquared(in DVector3 value1, in DVector3 value2)
        {
            var x = value1.X - value2.X;
            var y = value1.Y - value2.Y;
            var z = value1.Z - value2.Z;

            return (x * x) + (y * y) + (z * z);
        }
        public static double Dot(in DVector3 left, in DVector3 right)
        {
            double num;
            Dot(in left, in right, out num);
            return num;
        }
        public static double Dot(in DVector3 left, in Vector3 right)
        {
            double num;
            Dot(in left, in right, out num);
            return num;
        }
        public static void Dot(in DVector3 left, in DVector3 right, out double num)
        {
            num = (left.X * right.X + left.Y * right.Y + left.Z * right.Z);
        }
        public static void Dot(in DVector3 left, in Vector3 right, out double num)
        {
            num = (left.X * right.X + left.Y * right.Y + left.Z * right.Z);
        }
        public static DVector3 Cross(in DVector3 left, in DVector3 right)
        {
            DVector3 result;
            result.X = left.Y * right.Z - left.Z * right.Y;
            result.Y = left.Z * right.X - left.X * right.Z;
            result.Z = left.X * right.Y - left.Y * right.X;
            return result;
        }
        public static void Cross(in DVector3 left, in DVector3 right, out DVector3 result)
        {
            DVector3 r;
            r.X = left.Y * right.Z - left.Z * right.Y;
            r.Y = left.Z * right.X - left.X * right.Z;
            r.Z = left.X * right.Y - left.Y * right.X;

            result = r;
        }
        public static DVector3 Reflect(in DVector3 vector, in DVector3 normal)
        {
            DVector3 result;
            var dot = ((vector.X * normal.X) + (vector.Y * normal.Y)) + (vector.Z * normal.Z);

            result.X = vector.X - ((2.0f * dot) * normal.X);
            result.Y = vector.Y - ((2.0f * dot) * normal.Y);
            result.Z = vector.Z - ((2.0f * dot) * normal.Z);

            return result;
        }
        public static void Reflect(in DVector3 vector, in DVector3 normal, out DVector3 result)
        {
            var dot = ((vector.X * normal.X) + (vector.Y * normal.Y)) + (vector.Z * normal.Z);

            result.X = vector.X - ((2.0f * dot) * normal.X);
            result.Y = vector.Y - ((2.0f * dot) * normal.Y);
            result.Z = vector.Z - ((2.0f * dot) * normal.Z);
        }
        public static DVector3 CalcFaceNormal(in DVector3 a, in DVector3 b, in DVector3 c)
        {
            var t1 = a - c;
            var t2 = b - c;
            DVector3 result;
            Cross(in t1, in t2, out result);
            result.Normalize();
            return result;
        }
        public static double CalcArea3(in DVector3 a, in DVector3 b, in DVector3 c)
        {
            //此处是向量叉积的几何意义的应用
            //没处以2，所以出来的是平行四边形面积，并且有正负数的问题，
            //正数说明夹角是负角度
            //计算面积，外面要用abs * 0.5
            DVector3 v1 = b - a;
            DVector3 v2 = c - a;
            return ((v1.Y * v2.Z + v1.Z * v2.X + v1.X * v2.Y) -
                    (v1.Y * v2.X + v1.X * v2.Z + v1.X * v2.Y));
        }
        public static DVector3 Normalize(DVector3 vector)
        {
            vector.Normalize();
            return vector;
        }
        public static void Normalize(in DVector3 vector, out DVector3 result)
        {
            result = vector;
            result.Normalize();
        }
        public static DVector4 Transform(in DVector3 vector, in Matrix transform)
        {
            DVector4 result;

            result.X = (((vector.X * transform.M11) + (vector.Y * transform.M21)) + (vector.Z * transform.M31)) + transform.M41;
            result.Y = (((vector.X * transform.M12) + (vector.Y * transform.M22)) + (vector.Z * transform.M32)) + transform.M42;
            result.Z = (((vector.X * transform.M13) + (vector.Y * transform.M23)) + (vector.Z * transform.M33)) + transform.M43;
            result.W = (((vector.X * transform.M14) + (vector.Y * transform.M24)) + (vector.Z * transform.M34)) + transform.M44;

            return result;
        }
        public static void Transform(in DVector3 vector, in Matrix transform, out DVector4 result)
        {
            result.X = (((vector.X * transform.M11) + (vector.Y * transform.M21)) + (vector.Z * transform.M31)) + transform.M41;
            result.Y = (((vector.X * transform.M12) + (vector.Y * transform.M22)) + (vector.Z * transform.M32)) + transform.M42;
            result.Z = (((vector.X * transform.M13) + (vector.Y * transform.M23)) + (vector.Z * transform.M33)) + transform.M43;
            result.W = (((vector.X * transform.M14) + (vector.Y * transform.M24)) + (vector.Z * transform.M34)) + transform.M44;
        }
        public static DVector4 Transform(DVector3 value, Quaternion rotation)
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

            vector.X = ((value.X * ((1.0f - yy) - zz)) + (value.Y * (xy - wz))) + (value.Z * (xz + wy));
            vector.Y = ((value.X * (xy + wz)) + (value.Y * ((1.0f - xx) - zz))) + (value.Z * (yz - wx));
            vector.Z = ((value.X * (xz - wy)) + (value.Y * (yz + wx))) + (value.Z * ((1.0f - xx) - yy));
            vector.W = 1.0f;

            return vector;
        }
        public static void Transform(in DVector3 value, in Quaternion rotation, out DVector4 result)
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

            result.X = ((value.X * ((1.0f - yy) - zz)) + (value.Y * (xy - wz))) + (value.Z * (xz + wy));
            result.Y = ((value.X * (xy + wz)) + (value.Y * ((1.0f - xx) - zz))) + (value.Z * (yz - wx));
            result.Z = ((value.X * (xz - wy)) + (value.Y * (yz + wx))) + (value.Z * ((1.0f - xx) - yy));
            result.W = 1.0f;
        }
        public static DVector3 TransformCoordinate(in DVector3 coord, in DMatrix transform)
        {
            DVector4 vector;

            vector.X = (((coord.X * transform.M11) + (coord.Y * transform.M21)) + (coord.Z * transform.M31)) + transform.M41;
            vector.Y = (((coord.X * transform.M12) + (coord.Y * transform.M22)) + (coord.Z * transform.M32)) + transform.M42;
            vector.Z = (((coord.X * transform.M13) + (coord.Y * transform.M23)) + (coord.Z * transform.M33)) + transform.M43;
            vector.W = 1 / ((((coord.X * transform.M14) + (coord.Y * transform.M24)) + (coord.Z * transform.M34)) + transform.M44);

            DVector3 result;
            result.X = vector.X * vector.W;
            result.Y = vector.Y * vector.W;
            result.Z = vector.Z * vector.W;
            return result;
        }
        public static DVector3 TransformCoordinate(in DVector3 oriCoord, in DVector3 offset, in Matrix transform)
        {
            var coord = (oriCoord - offset).ToSingleVector3();
            Vector4 vector;

            vector.X = (((coord.X * transform.M11) + (coord.Y * transform.M21)) + (coord.Z * transform.M31)) + transform.M41;
            vector.Y = (((coord.X * transform.M12) + (coord.Y * transform.M22)) + (coord.Z * transform.M32)) + transform.M42;
            vector.Z = (((coord.X * transform.M13) + (coord.Y * transform.M23)) + (coord.Z * transform.M33)) + transform.M43;
            vector.W = 1 / ((((coord.X * transform.M14) + (coord.Y * transform.M24)) + (coord.Z * transform.M34)) + transform.M44);

            DVector3 result;
            result.X = offset.X + vector.X * vector.W;
            result.Y = offset.X + vector.Y * vector.W;
            result.Z = offset.X + vector.Z * vector.W;
            return result;
        }

        public static double TransformCoordinate(in DVector3 coord, in DMatrix transform,  out DVector3 result)
        {
            DVector4 vector;

            vector.X = (((coord.X * transform.M11) + (coord.Y * transform.M21)) + (coord.Z * transform.M31)) + transform.M41;
            vector.Y = (((coord.X * transform.M12) + (coord.Y * transform.M22)) + (coord.Z * transform.M32)) + transform.M42;
            vector.Z = (((coord.X * transform.M13) + (coord.Y * transform.M23)) + (coord.Z * transform.M33)) + transform.M43;
            vector.W = 1 / ((((coord.X * transform.M14) + (coord.Y * transform.M24)) + (coord.Z * transform.M34)) + transform.M44);

            result.X = vector.X * vector.W;
            result.Y = vector.Y * vector.W;
            result.Z = vector.Z * vector.W;
            return vector.W;
        }
        public static double TransformCoordinate(in DVector3 oriCoord, in DVector3 offset, in Matrix transform, out DVector3 result)
        {
            var coord = (oriCoord - offset).ToSingleVector3();
            Vector4 vector;

            vector.X = (((coord.X * transform.M11) + (coord.Y * transform.M21)) + (coord.Z * transform.M31)) + transform.M41;
            vector.Y = (((coord.X * transform.M12) + (coord.Y * transform.M22)) + (coord.Z * transform.M32)) + transform.M42;
            vector.Z = (((coord.X * transform.M13) + (coord.Y * transform.M23)) + (coord.Z * transform.M33)) + transform.M43;
            vector.W = 1 / ((((coord.X * transform.M14) + (coord.Y * transform.M24)) + (coord.Z * transform.M34)) + transform.M44);

            result.X = offset.X + vector.X * vector.W;
            result.Y = offset.Y + vector.Y * vector.W;
            result.Z = offset.Z + vector.Z * vector.W;
            return vector.W;
        }
        public static DVector3 TransformCoordinate(in DVector3 value, in Quaternion rotation)
        {
            var v4 = Transform(value, rotation);
            DVector3 result;
            result.X = v4.X;
            result.Y = v4.Y;
            result.Z = v4.Z;
            return result;
        }
        public static DVector3 TransformNormal(in DVector3 normal, in Matrix transform)
        {
            DVector3 vector;

            vector.X = ((normal.X * transform.M11) + (normal.Y * transform.M21)) + (normal.Z * transform.M31);
            vector.Y = ((normal.X * transform.M12) + (normal.Y * transform.M22)) + (normal.Z * transform.M32);
            vector.Z = ((normal.X * transform.M13) + (normal.Y * transform.M23)) + (normal.Z * transform.M33);

            return vector;
        }
        public static void TransformNormal(in DVector3 normal, in Matrix transform, out DVector3 result)
        {
            result.X = ((normal.X * transform.M11) + (normal.Y * transform.M21)) + (normal.Z * transform.M31);
            result.Y = ((normal.X * transform.M12) + (normal.Y * transform.M22)) + (normal.Z * transform.M32);
            result.Z = ((normal.X * transform.M13) + (normal.Y * transform.M23)) + (normal.Z * transform.M33);
        }
        public static DVector3 TransposeTransformNormal(in DVector3 normal, in Matrix transform)
        {
            DVector3 vector;

            vector.X = ((normal.X * transform.M11) + (normal.Y * transform.M12)) + (normal.Z * transform.M13);
            vector.Y = ((normal.X * transform.M21) + (normal.Y * transform.M22)) + (normal.Z * transform.M23);
            vector.Z = ((normal.X * transform.M31) + (normal.Y * transform.M32)) + (normal.Z * transform.M33);

            return vector;
        }
        public static DVector3 Project(DVector3 vector, float x, float y, float width, float height, float minZ, float maxZ, in DMatrix worldViewProjection)
        {
            TransformCoordinate(in vector, in worldViewProjection, out vector);
            DVector3 result;
            result.X = ((1.0f + vector.X) * 0.5f * width) + x;
            result.Y = ((1.0f - vector.Y) * 0.5f * height) + y;
            result.Z = (vector.Z * (maxZ - minZ)) + minZ;
            return result;
        }
        public static DVector3 Unproject(in DVector3 vector, float x, float y, float width, float height, float minZ, float maxZ, in DMatrix worldViewProjection)
        {
            DVector3 v;
            DMatrix matrix;
            DMatrix.Invert(in worldViewProjection, out matrix);

            v.X = (((vector.X - x) / width) * 2.0f) - 1.0f;
            v.Y = -((((vector.Y - y) / height) * 2.0f) - 1.0f);
            v.Z = (vector.Z - minZ) / (maxZ - minZ);

            TransformCoordinate(in v, in matrix, out v);
            return v;
        }
        public static bool Equals(in DVector3 value1, in DVector3 value2, double epsilon = CoreDefine.DEpsilon)
        {
            bool reX = (Math.Abs(value1.X - value2.X) < epsilon);
            bool reY = (Math.Abs(value1.Y - value2.Y) < epsilon);
            bool reZ = (Math.Abs(value1.Z - value2.Z) < epsilon);
            return (reX && reY && reZ);
        }
        public bool Equals(DVector3 value)
        {
            bool reX = (Math.Abs(X - value.X) < CoreDefine.DEpsilon);
            bool reY = (Math.Abs(Y - value.Y) < CoreDefine.DEpsilon);
            bool reZ = (Math.Abs(Z - value.Z) < CoreDefine.DEpsilon);
            return (reX && reY && reZ);
        }
    }
}
