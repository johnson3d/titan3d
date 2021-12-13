using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS
{
    public struct DMatrix : System.IEquatable<DMatrix>
    {
        #region Member
        public double M11;
        public double M12;
        public double M13;
        public double M14;
        public double M21;
        public double M22;
        public double M23;
        public double M24;
        public double M31;
        public double M32;
        public double M33;
        public double M34;
        public double M41;
        public double M42;
        public double M43;
        public double M44;
        #endregion
        #region Equal Overrride
        public override string ToString()
        {
            return string.Format(System.Globalization.CultureInfo.CurrentCulture, "[[M11:{0} M12:{1} M13:{2} M14:{3}] [M21:{4} M22:{5} M23:{6} M24:{7}] [M31:{8} M32:{9} M33:{10} M34:{11}] [M41:{12} M42:{13} M43:{14} M44:{15}]]",
                M11.ToString(System.Globalization.CultureInfo.CurrentCulture),
                M12.ToString(System.Globalization.CultureInfo.CurrentCulture),
                M13.ToString(System.Globalization.CultureInfo.CurrentCulture),
                M14.ToString(System.Globalization.CultureInfo.CurrentCulture),
                M21.ToString(System.Globalization.CultureInfo.CurrentCulture),
                M22.ToString(System.Globalization.CultureInfo.CurrentCulture),
                M23.ToString(System.Globalization.CultureInfo.CurrentCulture),
                M24.ToString(System.Globalization.CultureInfo.CurrentCulture),
                M31.ToString(System.Globalization.CultureInfo.CurrentCulture),
                M32.ToString(System.Globalization.CultureInfo.CurrentCulture),
                M33.ToString(System.Globalization.CultureInfo.CurrentCulture),
                M34.ToString(System.Globalization.CultureInfo.CurrentCulture),
                M41.ToString(System.Globalization.CultureInfo.CurrentCulture),
                M42.ToString(System.Globalization.CultureInfo.CurrentCulture),
                M43.ToString(System.Globalization.CultureInfo.CurrentCulture),
                M44.ToString(System.Globalization.CultureInfo.CurrentCulture));
        }
	    public override int GetHashCode()
        {
            return M11.GetHashCode() + M12.GetHashCode() + M13.GetHashCode() + M14.GetHashCode() +
                   M21.GetHashCode() + M22.GetHashCode() + M23.GetHashCode() + M24.GetHashCode() +
                   M31.GetHashCode() + M32.GetHashCode() + M33.GetHashCode() + M34.GetHashCode() +
                   M41.GetHashCode() + M42.GetHashCode() + M43.GetHashCode() + M44.GetHashCode();
        }
	    public override bool Equals(object value)
        {
            if (value == null)
                return false;

            if (value.GetType() != GetType())
                return false;

            return Equals((DMatrix)(value));
        }
	    public bool Equals(DMatrix value)
        {
            return (M11 == value.M11 && M12 == value.M12 && M13 == value.M13 && M14 == value.M14 &&
                     M21 == value.M21 && M22 == value.M22 && M23 == value.M23 && M24 == value.M24 &&
                     M31 == value.M31 && M32 == value.M32 && M33 == value.M33 && M34 == value.M34 &&
                     M41 == value.M41 && M42 == value.M42 && M43 == value.M43 && M44 == value.M44);
        }
        public static bool Equals(in DMatrix value1, in DMatrix value2, double epsilon = CoreDefine.DEpsilon)
        {
            return CoreDefine.DoubleEuqal(value1.M11, value2.M11, epsilon) &&
                    CoreDefine.DoubleEuqal(value1.M12, value2.M12, epsilon) &&
                    CoreDefine.DoubleEuqal(value1.M13, value2.M13, epsilon) &&
                    CoreDefine.DoubleEuqal(value1.M14, value2.M14, epsilon) &&
                    CoreDefine.DoubleEuqal(value1.M21, value2.M21, epsilon) &&
                    CoreDefine.DoubleEuqal(value1.M22, value2.M22, epsilon) &&
                    CoreDefine.DoubleEuqal(value1.M23, value2.M23, epsilon) &&
                    CoreDefine.DoubleEuqal(value1.M24, value2.M24, epsilon) &&
                    CoreDefine.DoubleEuqal(value1.M31, value2.M31, epsilon) &&
                    CoreDefine.DoubleEuqal(value1.M32, value2.M32, epsilon) &&
                    CoreDefine.DoubleEuqal(value1.M33, value2.M33, epsilon) &&
                    CoreDefine.DoubleEuqal(value1.M34, value2.M34, epsilon) &&
                    CoreDefine.DoubleEuqal(value1.M41, value2.M41, epsilon) &&
                    CoreDefine.DoubleEuqal(value1.M42, value2.M42, epsilon) &&
                    CoreDefine.DoubleEuqal(value1.M43, value2.M43, epsilon) &&
                    CoreDefine.DoubleEuqal(value1.M44, value2.M44, epsilon);
        }
        #endregion

        public double this[int row, int column]
        {
            get
            {
                if (row < 0 || row > 3)
                    throw new ArgumentOutOfRangeException("row", "Rows and columns for matrices run from 0 to 3, inclusive.");

                if (column < 0 || column > 3)
                    throw new ArgumentOutOfRangeException("column", "Rows and columns for matrices run from 0 to 3, inclusive.");

                int index = row * 4 + column;
                switch (index)
                {
                    case 0: return M11;
                    case 1: return M12;
                    case 2: return M13;
                    case 3: return M14;
                    case 4: return M21;
                    case 5: return M22;
                    case 6: return M23;
                    case 7: return M24;
                    case 8: return M31;
                    case 9: return M32;
                    case 10: return M33;
                    case 11: return M34;
                    case 12: return M41;
                    case 13: return M42;
                    case 14: return M43;
                    case 15: return M44;
                }

                return 0.0f;
            }

            set
            {
                if (row < 0 || row > 3)
                    throw new ArgumentOutOfRangeException("row", "Rows and columns for matrices run from 0 to 3, inclusive.");

                if (column < 0 || column > 3)
                    throw new ArgumentOutOfRangeException("column", "Rows and columns for matrices run from 0 to 3, inclusive.");

                int index = row * 4 + column;
                switch (index)
                {
                    case 0: M11 = value; break;
                    case 1: M12 = value; break;
                    case 2: M13 = value; break;
                    case 3: M14 = value; break;
                    case 4: M21 = value; break;
                    case 5: M22 = value; break;
                    case 6: M23 = value; break;
                    case 7: M24 = value; break;
                    case 8: M31 = value; break;
                    case 9: M32 = value; break;
                    case 10: M33 = value; break;
                    case 11: M34 = value; break;
                    case 12: M41 = value; break;
                    case 13: M42 = value; break;
                    case 14: M43 = value; break;
                    case 15: M44 = value; break;
                }
            }
        }

        public DVector3 Translation
        {
            get { return new DVector3(M41, M42, M43); }
            set
            {
                this.M41 = value.X;
                this.M42 = value.Y;
                this.M43 = value.Z;
            }
        }
        public DVector3 Scale
        {
            get
            {
                var temp = DVector3.Zero;
                temp.X = System.Math.Sqrt(M11 * M11 + M12 * M12 + M13 * M13); // getRow1().getLength();
                temp.Y = System.Math.Sqrt(M21 * M21 + M22 * M22 + M23 * M23);
                temp.Z = System.Math.Sqrt(M31 * M31 + M32 * M32 + M33 * M33);
                return temp;
            }
        }
        public Quaternion Rotation
        {
            get
            {
                Quaternion result;
                DMatrix.RotationMatrix(in this, out result);
                return result;
            }
        }
        public DVector3 GetScaledAxis(Matrix.EAxisType InAxis)
        {
            switch (InAxis)
            {
                case Matrix.EAxisType.X:
                    return new DVector3(M11, M12, M13);
                case Matrix.EAxisType.Y:
                    return new DVector3(M21, M22, M23);
                case Matrix.EAxisType.Z:
                    return new DVector3(M31, M32, M33);
                default:
                    return DVector3.Zero;
            }
        }
        public void NoScale()
        {
            var temp = Scale;
            M11 /= temp.X;
            M12 /= temp.X;
            M13 /= temp.X;
            M21 /= temp.Y;
            M22 /= temp.Y;
            M23 /= temp.Y;
            M31 /= temp.Z;
            M32 /= temp.Z;
            M33 /= temp.Z;
        }

        public DVector3 Left
        {
            get
            {
                return new DVector3(-this.M11, -this.M12, -this.M13);
            }
            set
            {
                this.M11 = -value.X;
                this.M12 = -value.Y;
                this.M13 = -value.Z;
            }
        }
        public DVector3 Right
        {
            get
            {
                return new DVector3(M11, M21, M31);
            }
            set
            {
                this.M11 = value.X;
                this.M12 = value.Y;
                this.M13 = value.Z;
            }
        }
        public DVector3 Up
        {
            get
            {
                return new DVector3(M12, M22, M32);
            }
            set
            {
                this.M21 = value.X;
                this.M22 = value.Y;
                this.M23 = value.Z;
            }
        }
        public DVector3 Down
        {
            get
            {
                return new DVector3(-this.M21, -this.M22, -this.M23);
            }
            set
            {
                this.M21 = -value.X;
                this.M22 = -value.Y;
                this.M23 = -value.Z;
            }
        }
        public DVector3 Forward
        {
            get
            {
                return new DVector3(M13, M23, M33);
            }
            set
            {
                this.M31 = -value.X;
                this.M32 = -value.Y;
                this.M33 = -value.Z;
            }
        }
        public DVector3 Backward
        {
            get
            {
                return new DVector3(this.M31, this.M32, this.M33);
            }
            set
            {
                this.M31 = value.X;
                this.M32 = value.Y;
                this.M33 = value.Z;
            }
        }
        public DVector3 Colume3
        {
            get
            {
                return new DVector3(M14, M24, M34);
            }
        }
        public readonly static DMatrix Identity = InitStaticMatrix();
        

        public static DMatrix InitStaticMatrix()
        {
            DMatrix matrix;
            unsafe
            {
                DMatrix* pMatrix = &matrix;
                double* pFloat = (double*)pMatrix;
                for (int i = 0; i < 16; i++)
                {
                    pFloat[i] = 0;
                }
            }
            matrix.M11 = 1.0f;
            matrix.M22 = 1.0f;
            matrix.M33 = 1.0f;
            matrix.M44 = 1.0f;
            return matrix;
        }

        public static void CreateWorld(in DVector3 position, in DVector3 forward, in DVector3 up, out DMatrix result)
        {
            DVector3 x, y, z;
            DVector3.Normalize(in forward, out z);
            DVector3.Cross(in forward, in up, out x);
            DVector3.Cross(in x, in forward, out y);
            x.Normalize();
            y.Normalize();

            result = new DMatrix();
            result.Right = x;
            result.Up = y;
            result.Forward = z;
            result.Translation = position;
            result.M44 = 1f;
        }
        public static Quaternion RotationMatrix(in DMatrix matrix)
        {
            Quaternion result;
            RotationMatrix(in matrix, out result);
            return result;
        }
        public static void RotationMatrix(in DMatrix matrix, out Quaternion result)
        {
            var scale = matrix.M11 + matrix.M22 + matrix.M33;

            if (scale > 0.0f)
            {
                var sqrt = Math.Sqrt(scale + 1.0f);

                result.W = (float)sqrt * 0.5f;
                sqrt = 0.5f / sqrt;

                result.X = (float)((matrix.M23 - matrix.M32) * sqrt);
                result.Y = (float)((matrix.M31 - matrix.M13) * sqrt);
                result.Z = (float)((matrix.M12 - matrix.M21) * sqrt);
                return;
            }

            if ((matrix.M11 >= matrix.M22) && (matrix.M11 >= matrix.M33))
            {
                var sqrt = Math.Sqrt(1.0f + matrix.M11 - matrix.M22 - matrix.M33);
                var half = 0.5d / sqrt;

                result.X = (float)(0.5d * sqrt);
                result.Y = (float)((matrix.M12 + matrix.M21) * half);
                result.Z = (float)((matrix.M13 + matrix.M31) * half);
                result.W = (float)((matrix.M23 - matrix.M32) * half);
                return;
            }

            if (matrix.M22 > matrix.M33)
            {
                var sqrt = Math.Sqrt(1.0f + matrix.M22 - matrix.M11 - matrix.M33);
                var half = 0.5d / sqrt;

                result.X = (float)((matrix.M21 + matrix.M12) * half);
                result.Y = (float)(0.5d * sqrt);
                result.Z = (float)((matrix.M32 + matrix.M23) * half);
                result.W = (float)((matrix.M31 - matrix.M13) * half);
                return;
            }

            {
                var sqrt = Math.Sqrt(1.0f + matrix.M33 - matrix.M11 - matrix.M22);
                var half = 0.5d / sqrt;

                result.X = (float)((matrix.M31 + matrix.M13) * half);
                result.Y = (float)((matrix.M32 + matrix.M23) * half);
                result.Z = (float)(0.5d * sqrt);
                result.W = (float)((matrix.M12 - matrix.M21) * half);
            }
        }
        public void Inverse()
        {
            unsafe
            {
                fixed (DMatrix* pinnedThis = &this)
                {
                    IDllImportApi.v3dxDMatrix4Inverse(pinnedThis, pinnedThis, (double*)0);
                }
            }
        }
        public static void Invert(in DMatrix matrix, out DMatrix outMatrix)
        {
            unsafe
            {
                fixed (DMatrix* pinnedThis = &matrix)
                fixed (DMatrix* pinnedOut = &outMatrix)
                {
                    IDllImportApi.v3dxDMatrix4Inverse(pinnedOut, pinnedThis, (double*)0);
                }
            }
        }
        public static DMatrix Transpose(in DMatrix mat)
        {
            DMatrix result;
            result.M11 = mat.M11;
            result.M12 = mat.M21;
            result.M13 = mat.M31;
            result.M14 = mat.M41;
            result.M21 = mat.M12;
            result.M22 = mat.M22;
            result.M23 = mat.M32;
            result.M24 = mat.M42;
            result.M31 = mat.M13;
            result.M32 = mat.M23;
            result.M33 = mat.M33;
            result.M34 = mat.M43;
            result.M41 = mat.M14;
            result.M42 = mat.M24;
            result.M43 = mat.M34;
            result.M44 = mat.M44;
            return result;
        }
        public static DMatrix Translate(in DVector3 translation)
        {
            DMatrix result;
            Translate(in translation, out result);
            return result;
        }
        public static void Translate(in DVector3 translation, out DMatrix result)
        {
            result.M11 = 1.0f;
            result.M12 = 0.0f;
            result.M13 = 0.0f;
            result.M14 = 0.0f;
            result.M21 = 0.0f;
            result.M22 = 1.0f;
            result.M23 = 0.0f;
            result.M24 = 0.0f;
            result.M31 = 0.0f;
            result.M32 = 0.0f;
            result.M33 = 1.0f;
            result.M34 = 0.0f;
            result.M41 = translation.X;
            result.M42 = translation.Y;
            result.M43 = translation.Z;
            result.M44 = 1.0f;
        }
        public static DMatrix RotationAxis(in DVector3 axis, double angle)
        {
            DMatrix result;
            RotationAxis(in axis, angle, out result);
            return result;
        }

        public static void RotationAxis(in DVector3 axis, double angle, out DMatrix result)
        {
            if (axis.LengthSquared() != 1.0f)
                axis.Normalize();

            double x = axis.X;
            double y = axis.Y;
            double z = axis.Z;
            double cos = Math.Cos(angle);
            double sin = Math.Sin(angle);
            double xx = x * x;
            double yy = y * y;
            double zz = z * z;
            double xy = x * y;
            double xz = x * z;
            double yz = y * z;

            result.M11 = xx + (cos * (1.0f - xx));
            result.M12 = (xy - (cos * xy)) + (sin * z);
            result.M13 = (xz - (cos * xz)) - (sin * y);
            result.M14 = 0.0f;
            result.M21 = (xy - (cos * xy)) - (sin * z);
            result.M22 = yy + (cos * (1.0f - yy));
            result.M23 = (yz - (cos * yz)) + (sin * x);
            result.M24 = 0.0f;
            result.M31 = (xz - (cos * xz)) + (sin * y);
            result.M32 = (yz - (cos * yz)) - (sin * x);
            result.M33 = zz + (cos * (1.0f - zz));
            result.M34 = 0.0f;
            result.M41 = 0.0f;
            result.M42 = 0.0f;
            result.M43 = 0.0f;
            result.M44 = 1.0f;
        }
        public static void RotationQuaternion(ref Quaternion rotation, out DMatrix result)
        {
            double xx = rotation.X * rotation.X;
            double yy = rotation.Y * rotation.Y;
            double zz = rotation.Z * rotation.Z;
            double xy = rotation.X * rotation.Y;
            double zw = rotation.Z * rotation.W;
            double zx = rotation.Z * rotation.X;
            double yw = rotation.Y * rotation.W;
            double yz = rotation.Y * rotation.Z;
            double xw = rotation.X * rotation.W;
            result.M11 = 1.0f - (2.0f * (yy + zz));
            result.M12 = 2.0f * (xy + zw);
            result.M13 = 2.0f * (zx - yw);
            result.M14 = 0.0f;
            result.M21 = 2.0f * (xy - zw);
            result.M22 = 1.0f - (2.0f * (zz + xx));
            result.M23 = 2.0f * (yz + xw);
            result.M24 = 0.0f;
            result.M31 = 2.0f * (zx + yw);
            result.M32 = 2.0f * (yz - xw);
            result.M33 = 1.0f - (2.0f * (yy + xx));
            result.M34 = 0.0f;
            result.M41 = 0.0f;
            result.M42 = 0.0f;
            result.M43 = 0.0f;
            result.M44 = 1.0f;
        }
        public static DMatrix operator *(DMatrix left, DMatrix right)
        {
            DMatrix result;

            result.M11 = (left.M11 * right.M11) + (left.M12 * right.M21) + (left.M13 * right.M31) + (left.M14 * right.M41);
            result.M12 = (left.M11 * right.M12) + (left.M12 * right.M22) + (left.M13 * right.M32) + (left.M14 * right.M42);
            result.M13 = (left.M11 * right.M13) + (left.M12 * right.M23) + (left.M13 * right.M33) + (left.M14 * right.M43);
            result.M14 = (left.M11 * right.M14) + (left.M12 * right.M24) + (left.M13 * right.M34) + (left.M14 * right.M44);
            result.M21 = (left.M21 * right.M11) + (left.M22 * right.M21) + (left.M23 * right.M31) + (left.M24 * right.M41);
            result.M22 = (left.M21 * right.M12) + (left.M22 * right.M22) + (left.M23 * right.M32) + (left.M24 * right.M42);
            result.M23 = (left.M21 * right.M13) + (left.M22 * right.M23) + (left.M23 * right.M33) + (left.M24 * right.M43);
            result.M24 = (left.M21 * right.M14) + (left.M22 * right.M24) + (left.M23 * right.M34) + (left.M24 * right.M44);
            result.M31 = (left.M31 * right.M11) + (left.M32 * right.M21) + (left.M33 * right.M31) + (left.M34 * right.M41);
            result.M32 = (left.M31 * right.M12) + (left.M32 * right.M22) + (left.M33 * right.M32) + (left.M34 * right.M42);
            result.M33 = (left.M31 * right.M13) + (left.M32 * right.M23) + (left.M33 * right.M33) + (left.M34 * right.M43);
            result.M34 = (left.M31 * right.M14) + (left.M32 * right.M24) + (left.M33 * right.M34) + (left.M34 * right.M44);
            result.M41 = (left.M41 * right.M11) + (left.M42 * right.M21) + (left.M43 * right.M31) + (left.M44 * right.M41);
            result.M42 = (left.M41 * right.M12) + (left.M42 * right.M22) + (left.M43 * right.M32) + (left.M44 * right.M42);
            result.M43 = (left.M41 * right.M13) + (left.M42 * right.M23) + (left.M43 * right.M33) + (left.M44 * right.M43);
            result.M44 = (left.M41 * right.M14) + (left.M42 * right.M24) + (left.M43 * right.M34) + (left.M44 * right.M44);
            return result;
        }
        public static DMatrix operator *(DMatrix left, double right)
        {
            DMatrix result;
            result.M11 = left.M11 * right;
            result.M12 = left.M12 * right;
            result.M13 = left.M13 * right;
            result.M14 = left.M14 * right;
            result.M21 = left.M21 * right;
            result.M22 = left.M22 * right;
            result.M23 = left.M23 * right;
            result.M24 = left.M24 * right;
            result.M31 = left.M31 * right;
            result.M32 = left.M32 * right;
            result.M33 = left.M33 * right;
            result.M34 = left.M34 * right;
            result.M41 = left.M41 * right;
            result.M42 = left.M42 * right;
            result.M43 = left.M43 * right;
            result.M44 = left.M44 * right;
            return result;
        }
        public static DMatrix operator *(double right, DMatrix left)
        {
            return left * right;
        }
        public static DMatrix operator /(DMatrix left, DMatrix right)
        {
            DMatrix result;
            result.M11 = left.M11 / right.M11;
            result.M12 = left.M12 / right.M12;
            result.M13 = left.M13 / right.M13;
            result.M14 = left.M14 / right.M14;
            result.M21 = left.M21 / right.M21;
            result.M22 = left.M22 / right.M22;
            result.M23 = left.M23 / right.M23;
            result.M24 = left.M24 / right.M24;
            result.M31 = left.M31 / right.M31;
            result.M32 = left.M32 / right.M32;
            result.M33 = left.M33 / right.M33;
            result.M34 = left.M34 / right.M34;
            result.M41 = left.M41 / right.M41;
            result.M42 = left.M42 / right.M42;
            result.M43 = left.M43 / right.M43;
            result.M44 = left.M44 / right.M44;
            return result;
        }
        public static DMatrix operator /(DMatrix left, double right)
        {
            DMatrix result;
            result.M11 = left.M11 / right;
            result.M12 = left.M12 / right;
            result.M13 = left.M13 / right;
            result.M14 = left.M14 / right;
            result.M21 = left.M21 / right;
            result.M22 = left.M22 / right;
            result.M23 = left.M23 / right;
            result.M24 = left.M24 / right;
            result.M31 = left.M31 / right;
            result.M32 = left.M32 / right;
            result.M33 = left.M33 / right;
            result.M34 = left.M34 / right;
            result.M41 = left.M41 / right;
            result.M42 = left.M42 / right;
            result.M43 = left.M43 / right;
            result.M44 = left.M44 / right;
            return result;
        }
        public static DMatrix operator +(DMatrix left, DMatrix right)
        {
            DMatrix result;
            result.M11 = left.M11 + right.M11;
            result.M12 = left.M12 + right.M12;
            result.M13 = left.M13 + right.M13;
            result.M14 = left.M14 + right.M14;
            result.M21 = left.M21 + right.M21;
            result.M22 = left.M22 + right.M22;
            result.M23 = left.M23 + right.M23;
            result.M24 = left.M24 + right.M24;
            result.M31 = left.M31 + right.M31;
            result.M32 = left.M32 + right.M32;
            result.M33 = left.M33 + right.M33;
            result.M34 = left.M34 + right.M34;
            result.M41 = left.M41 + right.M41;
            result.M42 = left.M42 + right.M42;
            result.M43 = left.M43 + right.M43;
            result.M44 = left.M44 + right.M44;
            return result;
        }
        public static DMatrix operator -(DMatrix left, DMatrix right)
        {
            DMatrix result;
            result.M11 = left.M11 - right.M11;
            result.M12 = left.M12 - right.M12;
            result.M13 = left.M13 - right.M13;
            result.M14 = left.M14 - right.M14;
            result.M21 = left.M21 - right.M21;
            result.M22 = left.M22 - right.M22;
            result.M23 = left.M23 - right.M23;
            result.M24 = left.M24 - right.M24;
            result.M31 = left.M31 - right.M31;
            result.M32 = left.M32 - right.M32;
            result.M33 = left.M33 - right.M33;
            result.M34 = left.M34 - right.M34;
            result.M41 = left.M41 - right.M41;
            result.M42 = left.M42 - right.M42;
            result.M43 = left.M43 - right.M43;
            result.M44 = left.M44 - right.M44;
            return result;
        }
        public static DMatrix operator -(DMatrix matrix)
        {
            DMatrix result;
            result.M11 = -matrix.M11;
            result.M12 = -matrix.M12;
            result.M13 = -matrix.M13;
            result.M14 = -matrix.M14;
            result.M21 = -matrix.M21;
            result.M22 = -matrix.M22;
            result.M23 = -matrix.M23;
            result.M24 = -matrix.M24;
            result.M31 = -matrix.M31;
            result.M32 = -matrix.M32;
            result.M33 = -matrix.M33;
            result.M34 = -matrix.M34;
            result.M41 = -matrix.M41;
            result.M42 = -matrix.M42;
            result.M43 = -matrix.M43;
            result.M44 = -matrix.M44;
            return result;
        }
        public static bool operator ==(DMatrix left, DMatrix right)
        {
            return left.Equals(right);
            //return Equals(left, right);
        }
        public static bool operator !=(DMatrix left, DMatrix right)
        {
            return !left.Equals(right);
            //return !Matrix.Equals(left, right);
        }

        public static DMatrix Transformation(in Vector3 scaling, in Quaternion rotation, in DVector3 translation, out DMatrix result)
        {
            unsafe
            {
                fixed (Vector3* pinScaling = &scaling)
                fixed (Quaternion* pinRot = &rotation)
                fixed (DVector3* pinTrans = &translation)
                fixed (DMatrix* pinResult = &result)
                {
                    IDllImportApi.v3dxDMatrixTransformationOrigin(pinResult, pinScaling, pinRot, pinTrans);
                }
            }
            return result;
        }
        public static void Transformation(in DVector3 scalingCenter, in Quaternion scalingRotation, in Vector3 scaling, in DVector3 rotationCenter, in Quaternion rotation, in DVector3 translation, out DMatrix result)
        {
            unsafe
            {
                fixed (DVector3* pinScalingCenter = &scalingCenter)
                {
                    fixed (Quaternion* pinScalingRotation = &scalingRotation)
                    {
                        fixed (Vector3* pinScaling = &scaling)
                        {
                            fixed (DVector3* pinRotationCenter = &rotationCenter)
                            {
                                fixed (Quaternion* pinRotation = &rotation)
                                {
                                    fixed (DVector3* pinTranslation = &translation)
                                    {
                                        fixed (DMatrix* pinResult = &result)
                                        {
                                            IDllImportApi.v3dxDMatrixTransformation(pinResult, pinScalingCenter, pinScalingRotation, pinScaling, pinRotationCenter, pinRotation, pinTranslation);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
        public void Decompose(out Vector3 scale, out Quaternion rotation, out DVector3 translation)
        {
            unsafe
            {
                fixed (Vector3* plocalScale = &scale)
                {
                    fixed (Quaternion* plocalRot = &rotation)
                    {
                        fixed (DVector3* plocalTrans = &translation)
                        {
                            fixed (DMatrix* pinnedThis = &this)
                            {
                                IDllImportApi.v3dxDMatrixDecompose(plocalScale, plocalRot, plocalTrans, pinnedThis);
                            }
                        }
                    }
                }
            }
        }
    }
}
