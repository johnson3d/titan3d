using System;


namespace EngineNS
{
    /// <summary>
    /// 4*4矩阵结构体
    /// </summary>
    [System.Serializable]
    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential, Pack = 4)]
    //[System.ComponentModel.TypeConverter( typeof(EngineNS.Design.MatrixConverter) )]
    public struct Matrix : System.IEquatable<Matrix>
    {
        #region Member
        public float M11;

        public float M12;

        public float M13;

        public float M14;

        public float M21;

        public float M22;

        public float M23;

        public float M24;

        public float M31;

        public float M32;

        public float M33;

        public float M34;

        public float M41;

        public float M42;

        public float M43;

        public float M44;
        #endregion
        /// <summary>
        /// 矩阵的元素
        /// </summary>
        /// <param name="row">行值</param>
        /// <param name="column">列值</param>
        /// <returns>矩阵的相应的行和列的值</returns>
        public float this[int row, int column]
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
        public Vector3 Left
        {
            get
            {
                return new Vector3(-this.M11, -this.M12, -this.M13);
            }
            set
            {
                this.M11 = -value.X;
                this.M12 = -value.Y;
                this.M13 = -value.Z;
            }
        }
        public Vector3 Right
        {
            get
            {
                return new Vector3(M11, M21, M31);
            }
            set
            {
                this.M11 = value.X;
                this.M12 = value.Y;
                this.M13 = value.Z;
            }
        }
        public Vector3 Up
        {
            get
            {
                return new Vector3(M12, M22, M32);
            }
            set
            {
                this.M21 = value.X;
                this.M22 = value.Y;
                this.M23 = value.Z;
            }
        }
        public Vector3 Down
        {
            get
            {
                return new Vector3(-this.M21, -this.M22, -this.M23);
            }
            set
            {
                this.M21 = -value.X;
                this.M22 = -value.Y;
                this.M23 = -value.Z;
            }
        }
        public Vector3 Forward
        {
            get
            {
                return new Vector3(M13, M23, M33);
            }
            set
            {
                this.M31 = -value.X;
                this.M32 = -value.Y;
                this.M33 = -value.Z;
            }
        }
        public Vector3 Backward
        {
            get
            {
                return new Vector3(this.M31, this.M32, this.M33);
            }
            set
            {
                this.M31 = value.X;
                this.M32 = value.Y;
                this.M33 = value.Z;
            }
        }
        public Vector3 Colume3
        {
            get
            {
                return new Vector3(M14, M24, M34);
            }
        }
        /// <summary>
        /// 标准矩阵
        /// </summary>
        public static Matrix mIdentity = InitStaticMatrix();
        /// <summary>
        /// 初始化矩阵为单位矩阵
        /// </summary>
        /// <returns>返回单位矩阵</returns>
        public static Matrix InitStaticMatrix()
        {
            Matrix matrix;
            unsafe
            {
                Matrix* pMatrix = &matrix;
                float* pFloat = (float*)pMatrix;
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
        public static Matrix Identity
        {
            get
            {
                return mIdentity;
            }
        }

        #region Equal Overrride
        /// <summary>
        /// 转换到string字符串
        /// </summary>
        /// <returns>转换后的String字符串</returns>
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
        /// <summary>
        /// 获取对象的哈希值
        /// </summary>
        /// <returns>返回对象的哈希值</returns>
	    public override int GetHashCode()
        {
            return M11.GetHashCode() + M12.GetHashCode() + M13.GetHashCode() + M14.GetHashCode() +
                   M21.GetHashCode() + M22.GetHashCode() + M23.GetHashCode() + M24.GetHashCode() +
                   M31.GetHashCode() + M32.GetHashCode() + M33.GetHashCode() + M34.GetHashCode() +
                   M41.GetHashCode() + M42.GetHashCode() + M43.GetHashCode() + M44.GetHashCode();
        }
        /// <summary>
        /// 判断两个对象是否相等
        /// </summary>
        /// <param name="value">可转换成矩阵的对象</param>
        /// <returns>如果相等返回true，否则返回false</returns>
	    public override bool Equals(object value)
        {
            if (value == null)
                return false;

            if (value.GetType() != GetType())
                return false;

            return Equals((Matrix)(value));
        }
        /// <summary>
        /// 判断两个对象是否相等
        /// </summary>
        /// <param name="value">矩阵对象</param>
        /// <returns>如果相等返回true，否则返回false</returns>
	    public bool Equals(Matrix value)
        {
            return (M11 == value.M11 && M12 == value.M12 && M13 == value.M13 && M14 == value.M14 &&
                     M21 == value.M21 && M22 == value.M22 && M23 == value.M23 && M24 == value.M24 &&
                     M31 == value.M31 && M32 == value.M32 && M33 == value.M33 && M34 == value.M34 &&
                     M41 == value.M41 && M42 == value.M42 && M43 == value.M43 && M44 == value.M44);
        }
        /// <summary>
        /// 判断两个对象是否相等
        /// </summary>
        /// <param name="value1">矩阵对象</param>
        /// <param name="value2">矩阵对象</param>
        /// <returns>如果value1与value2相等返回true，否则返回false</returns>
	    public static bool Equals(ref Matrix value1, ref Matrix value2)
        {
            return (value1.M11 == value2.M11 && value1.M12 == value2.M12 && value1.M13 == value2.M13 && value1.M14 == value2.M14 &&
                     value1.M21 == value2.M21 && value1.M22 == value2.M22 && value1.M23 == value2.M23 && value1.M24 == value2.M24 &&
                     value1.M31 == value2.M31 && value1.M32 == value2.M32 && value1.M33 == value2.M33 && value1.M34 == value2.M34 &&
                     value1.M41 == value2.M41 && value1.M42 == value2.M42 && value1.M43 == value2.M43 && value1.M44 == value2.M44);
        }
        #endregion
        /// <summary>
        /// 将矩阵对象转换成数组
        /// </summary>
        /// <returns>返回转换后的数组</returns>
        public float[] ToArray()
        {
            float[] result = new float[16];
            result[0] = M11;
            result[1] = M12;
            result[2] = M13;
            result[3] = M14;
            result[4] = M21;
            result[5] = M22;
            result[6] = M23;
            result[7] = M24;
            result[8] = M31;
            result[9] = M32;
            result[10] = M33;
            result[11] = M34;
            result[12] = M41;
            result[13] = M42;
            result[14] = M43;
            result[15] = M44;

            return result;
        }
        public static void CreateWorld(ref Vector3 position, ref Vector3 forward, ref Vector3 up, out Matrix result)
        {
            Vector3 x, y, z;
            Vector3.Normalize(ref forward, out z);
            Vector3.Cross(ref forward, ref up, out x);
            Vector3.Cross(ref x, ref forward, out y);
            x.Normalize();
            y.Normalize();

            result = new Matrix();
            result.Right = x;
            result.Up = y;
            result.Forward = z;
            result.Translation = position;
            result.M44 = 1f;
        }

        public static Matrix CreateBillboard(Vector3 objectPosition, Vector3 cameraPosition,
           Vector3 cameraUpVector, Nullable<Vector3> cameraForwardVector)
        {
            var diff = cameraPosition - objectPosition;

            Matrix matrix = Matrix.Identity;

            diff.Normalize();
            matrix.Forward = diff;
            matrix.Left = Vector3.Cross(diff, cameraUpVector);
            matrix.Up = cameraUpVector;
            matrix.Translation = objectPosition;

            return matrix;
        }
        public static void CreateBillboard(ref Vector3 objectPosition, ref Vector3 cameraPosition,
            ref Vector3 cameraUpVector, Vector3? cameraForwardVector, out Matrix result)
        {
            Vector3 vector;
            Vector3 vector2;
            Vector3 vector3;
            vector.X = objectPosition.X - cameraPosition.X;
            vector.Y = objectPosition.Y - cameraPosition.Y;
            vector.Z = objectPosition.Z - cameraPosition.Z;
            float num = vector.LengthSquared();
            if (num < 0.0001f)
            {
                vector = cameraForwardVector.HasValue ? -cameraForwardVector.Value : Vector3.Forward;
            }
            else
            {
                Vector3.Multiply(ref vector, (float)(1f / ((float)Math.Sqrt((double)num))), out vector);
            }
            Vector3.Cross(ref cameraUpVector, ref vector, out vector3);
            vector3.Normalize();
            Vector3.Cross(ref vector, ref vector3, out vector2);
            result.M11 = vector3.X;
            result.M12 = vector3.Y;
            result.M13 = vector3.Z;
            result.M14 = 0f;
            result.M21 = vector2.X;
            result.M22 = vector2.Y;
            result.M23 = vector2.Z;
            result.M24 = 0f;
            result.M31 = vector.X;
            result.M32 = vector.Y;
            result.M33 = vector.Z;
            result.M34 = 0f;
            result.M41 = objectPosition.X;
            result.M42 = objectPosition.Y;
            result.M43 = objectPosition.Z;
            result.M44 = 1f;
        }

        public static Matrix CreateConstrainedBillboard(Vector3 objectPosition, Vector3 cameraPosition,
            Vector3 rotateAxis, Nullable<Vector3> cameraForwardVector, Nullable<Vector3> objectForwardVector)
        {
            float num;
            Vector3 vector;
            Matrix matrix;
            Vector3 vector2;
            Vector3 vector3;
            vector2.X = objectPosition.X - cameraPosition.X;
            vector2.Y = objectPosition.Y - cameraPosition.Y;
            vector2.Z = objectPosition.Z - cameraPosition.Z;
            float num2 = vector2.LengthSquared();
            if (num2 < 0.0001f)
            {
                vector2 = cameraForwardVector.HasValue ? -cameraForwardVector.Value : Vector3.Forward;
            }
            else
            {
                Vector3.Multiply(ref vector2, (float)(1f / ((float)Math.Sqrt((double)num2))), out vector2);
            }
            Vector3 vector4 = rotateAxis;
            Vector3.Dot(ref rotateAxis, ref vector2, out num);
            if (Math.Abs(num) > 0.9982547f)
            {
                if (objectForwardVector.HasValue)
                {
                    vector = objectForwardVector.Value;
                    Vector3.Dot(ref rotateAxis, ref vector, out num);
                    if (Math.Abs(num) > 0.9982547f)
                    {
                        num = ((rotateAxis.X * Vector3.Forward.X) + (rotateAxis.Y * Vector3.Forward.Y)) + (rotateAxis.Z * Vector3.Forward.Z);
                        vector = (Math.Abs(num) > 0.9982547f) ? Vector3.Right : Vector3.Forward;
                    }
                }
                else
                {
                    num = ((rotateAxis.X * Vector3.Forward.X) + (rotateAxis.Y * Vector3.Forward.Y)) + (rotateAxis.Z * Vector3.Forward.Z);
                    vector = (Math.Abs(num) > 0.9982547f) ? Vector3.Right : Vector3.Forward;
                }
                Vector3.Cross(ref rotateAxis, ref vector, out vector3);
                vector3.Normalize();
                Vector3.Cross(ref vector3, ref rotateAxis, out vector);
                vector.Normalize();
            }
            else
            {
                Vector3.Cross(ref rotateAxis, ref vector2, out vector3);
                vector3.Normalize();
                Vector3.Cross(ref vector3, ref vector4, out vector);
                vector.Normalize();
            }
            matrix.M11 = vector3.X;
            matrix.M12 = vector3.Y;
            matrix.M13 = vector3.Z;
            matrix.M14 = 0f;
            matrix.M21 = vector4.X;
            matrix.M22 = vector4.Y;
            matrix.M23 = vector4.Z;
            matrix.M24 = 0f;
            matrix.M31 = vector.X;
            matrix.M32 = vector.Y;
            matrix.M33 = vector.Z;
            matrix.M34 = 0f;
            matrix.M41 = objectPosition.X;
            matrix.M42 = objectPosition.Y;
            matrix.M43 = objectPosition.Z;
            matrix.M44 = 1f;
            return matrix;

        }


        public static void CreateConstrainedBillboard(ref Vector3 objectPosition, ref Vector3 cameraPosition,
            ref Vector3 rotateAxis, Vector3? cameraForwardVector, Vector3? objectForwardVector, out Matrix result)
        {
            float num;
            Vector3 vector;
            Vector3 vector2;
            Vector3 vector3;
            vector2.X = objectPosition.X - cameraPosition.X;
            vector2.Y = objectPosition.Y - cameraPosition.Y;
            vector2.Z = objectPosition.Z - cameraPosition.Z;
            float num2 = vector2.LengthSquared();
            if (num2 < 0.0001f)
            {
                vector2 = cameraForwardVector.HasValue ? -cameraForwardVector.Value : Vector3.Forward;
            }
            else
            {
                Vector3.Multiply(ref vector2, (float)(1f / ((float)Math.Sqrt((double)num2))), out vector2);
            }
            Vector3 vector4 = rotateAxis;
            Vector3.Dot(ref rotateAxis, ref vector2, out num);
            if (Math.Abs(num) > 0.9982547f)
            {
                if (objectForwardVector.HasValue)
                {
                    vector = objectForwardVector.Value;
                    Vector3.Dot(ref rotateAxis, ref vector, out num);
                    if (Math.Abs(num) > 0.9982547f)
                    {
                        num = ((rotateAxis.X * Vector3.Forward.X) + (rotateAxis.Y * Vector3.Forward.Y)) + (rotateAxis.Z * Vector3.Forward.Z);
                        vector = (Math.Abs(num) > 0.9982547f) ? Vector3.Right : Vector3.Forward;
                    }
                }
                else
                {
                    num = ((rotateAxis.X * Vector3.Forward.X) + (rotateAxis.Y * Vector3.Forward.Y)) + (rotateAxis.Z * Vector3.Forward.Z);
                    vector = (Math.Abs(num) > 0.9982547f) ? Vector3.Right : Vector3.Forward;
                }
                Vector3.Cross(ref rotateAxis, ref vector, out vector3);
                vector3.Normalize();
                Vector3.Cross(ref vector3, ref rotateAxis, out vector);
                vector.Normalize();
            }
            else
            {
                Vector3.Cross(ref rotateAxis, ref vector2, out vector3);
                vector3.Normalize();
                Vector3.Cross(ref vector3, ref vector4, out vector);
                vector.Normalize();
            }
            result.M11 = vector3.X;
            result.M12 = vector3.Y;
            result.M13 = vector3.Z;
            result.M14 = 0f;
            result.M21 = vector4.X;
            result.M22 = vector4.Y;
            result.M23 = vector4.Z;
            result.M24 = 0f;
            result.M31 = vector.X;
            result.M32 = vector.Y;
            result.M33 = vector.Z;
            result.M34 = 0f;
            result.M41 = objectPosition.X;
            result.M42 = objectPosition.Y;
            result.M43 = objectPosition.Z;
            result.M44 = 1f;

        }

        /// <summary>
        /// 逆矩阵
        /// </summary>
	    public void Inverse()
        {
            unsafe
            {
                fixed (Matrix* pinnedThis = &this)
                {
                    IDllImportApi.v3dxMatrix4Inverse(pinnedThis, pinnedThis, (float*)0);
                }
            }
        }
        /// <summary>
        /// 转置矩阵
        /// </summary>
        public void Transpose()
        {
            unsafe
            {
                fixed (Matrix* pinnedThis = &this)
                {
                    IDllImportApi.v3dxMatrix4Transpose_extern(pinnedThis, pinnedThis);
                }
            }
        }
        public static void Transpose(ref Matrix matrix, out Matrix result)
        {
            result.M11 = matrix.M11;
            result.M12 = matrix.M21;
            result.M13 = matrix.M31;
            result.M14 = matrix.M41;

            result.M21 = matrix.M12;
            result.M22 = matrix.M22;
            result.M23 = matrix.M32;
            result.M24 = matrix.M42;

            result.M31 = matrix.M13;
            result.M32 = matrix.M23;
            result.M33 = matrix.M33;
            result.M34 = matrix.M43;

            result.M41 = matrix.M14;
            result.M42 = matrix.M24;
            result.M43 = matrix.M34;
            result.M44 = matrix.M44;
        }
        /// <summary>
        /// 矩阵分解
        /// </summary>
        /// <param name="scale">缩放值</param>
        /// <param name="rotation">旋转值，弧度</param>
        /// <param name="translation">平移值</param>
        /// <returns>分解成功返回true，否则返回false</returns>
	    public bool Decompose(out Vector3 scale, out Quaternion rotation, out Vector3 translation)
        {
            unsafe
            {
                fixed (Vector3* plocalScale = &scale)
                {
                    fixed (Quaternion* plocalRot = &rotation)
                    {
                        fixed (Vector3* plocalTrans = &translation)
                        {
                            fixed (Matrix* pinnedThis = &this)
                            {
                                int hr = IDllImportApi.v3dxMatrixDecompose(plocalScale, plocalRot, plocalTrans, pinnedThis);
                                return hr == 0;
                            }
                        }
                    }
                }
            }
        }
        /// <summary>
        /// 行列式因子
        /// </summary>
        /// <returns>返回该行列式的值</returns>
        public float Determinant()
        {
            float temp1 = (M33 * M44) - (M34 * M43);
            float temp2 = (M32 * M44) - (M34 * M42);
            float temp3 = (M32 * M43) - (M33 * M42);
            float temp4 = (M31 * M44) - (M34 * M41);
            float temp5 = (M31 * M43) - (M33 * M41);
            float temp6 = (M31 * M42) - (M32 * M41);

            return ((((M11 * (((M22 * temp1) - (M23 * temp2)) + (M24 * temp3))) - (M12 * (((M21 * temp1) -
                (M23 * temp4)) + (M24 * temp5)))) + (M13 * (((M21 * temp2) - (M22 * temp4)) + (M24 * temp6)))) -
                (M14 * (((M21 * temp3) - (M22 * temp5)) + (M23 * temp6))));
        }

        public void SetTrans(Vector3 v)
        {
            M41 = v.X;
            M42 = v.Y;
            M43 = v.Z;
        }
        public void NoRotation()
        {
            var temp = Scale;
            M11 = temp.X;
            M12 = 0;
            M13 = 0;
            M21 = 0;
            M22 = temp.Y;
            M23 = 0;
            M31 = 0;
            M32 = 0;
            M33 = temp.Z;
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
        public Vector3 Translation
        {
            get { return new Vector3(M41, M42, M43); }
            set
            {
                this.M41 = value.X;
                this.M42 = value.Y;
                this.M43 = value.Z;
            }
        }
        public Vector3 Scale
        {
            get
            {
                var temp = Vector3.Zero;
                temp.X = MathHelper.Sqrt(M11 * M11 + M12 * M12 + M13 * M13); // getRow1().getLength();
                temp.Y = MathHelper.Sqrt(M21 * M21 + M22 * M22 + M23 * M23);
                temp.Z = MathHelper.Sqrt(M31 * M31 + M32 * M32 + M33 * M33);
                return temp;
            }
        }
        public Quaternion Rotation
        {
            get
            {
                return Quaternion.RotationMatrix(this);
            }
        }

        /// <summary>
        /// 矩阵相加
        /// </summary>
        /// <param name="left">矩阵对象</param>
        /// <param name="right">矩阵对象</param>
        /// <returns>返回计算后的矩阵</returns>
        public static Matrix Add(Matrix left, Matrix right)
        {
            Matrix result;
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
        /// <summary>
        /// 矩阵相加
        /// </summary>
        /// <param name="left">矩阵对象</param>
        /// <param name="right">矩阵对象</param>
        /// <param name="result">计算后的矩阵</param>
        public static void Add(ref Matrix left, ref Matrix right, out Matrix result)
        {
            Matrix r;
            r.M11 = left.M11 + right.M11;
            r.M12 = left.M12 + right.M12;
            r.M13 = left.M13 + right.M13;
            r.M14 = left.M14 + right.M14;
            r.M21 = left.M21 + right.M21;
            r.M22 = left.M22 + right.M22;
            r.M23 = left.M23 + right.M23;
            r.M24 = left.M24 + right.M24;
            r.M31 = left.M31 + right.M31;
            r.M32 = left.M32 + right.M32;
            r.M33 = left.M33 + right.M33;
            r.M34 = left.M34 + right.M34;
            r.M41 = left.M41 + right.M41;
            r.M42 = left.M42 + right.M42;
            r.M43 = left.M43 + right.M43;
            r.M44 = left.M44 + right.M44;

            result = r;
        }
        /// <summary>
        /// 两个矩阵相减
        /// </summary>
        /// <param name="left">矩阵对象</param>
        /// <param name="right">矩阵对象</param>
        /// <returns>返回计算后的矩阵</returns>
        public static Matrix Subtract(Matrix left, Matrix right)
        {
            Matrix result;
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
        /// <summary>
        /// 两个矩阵相减
        /// </summary>
        /// <param name="left">矩阵对象</param>
        /// <param name="right">矩阵对象</param>
        /// <param name="result">计算后的矩阵</param>
        public static void Subtract(ref Matrix left, ref Matrix right, out Matrix result)
        {
            Matrix r;
            r.M11 = left.M11 - right.M11;
            r.M12 = left.M12 - right.M12;
            r.M13 = left.M13 - right.M13;
            r.M14 = left.M14 - right.M14;
            r.M21 = left.M21 - right.M21;
            r.M22 = left.M22 - right.M22;
            r.M23 = left.M23 - right.M23;
            r.M24 = left.M24 - right.M24;
            r.M31 = left.M31 - right.M31;
            r.M32 = left.M32 - right.M32;
            r.M33 = left.M33 - right.M33;
            r.M34 = left.M34 - right.M34;
            r.M41 = left.M41 - right.M41;
            r.M42 = left.M42 - right.M42;
            r.M43 = left.M43 - right.M43;
            r.M44 = left.M44 - right.M44;

            result = r;
        }
        /// <summary>
        /// 矩阵取反
        /// </summary>
        /// <param name="matrix">矩阵对象</param>
        /// <returns>返回计算后的矩阵</returns>
        public static Matrix Negate(Matrix matrix)
        {
            Matrix result;
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
        public static Matrix NoRotation(Matrix matrix)
        {
            var result = matrix;
            result.NoRotation();
            return result;
        }
        public static Matrix NoScale(Matrix matrix)
        {
            var result = matrix;
            result.NoScale();
            return result;
        }
        /// <summary>
        /// 矩阵取反
        /// </summary>
        /// <param name="matrix">矩阵对象</param>
        /// <param name="result">计算后的矩阵</param>
        public static void Negate(ref Matrix matrix, out Matrix result)
        {
            Matrix r;
            r.M11 = -matrix.M11;
            r.M12 = -matrix.M12;
            r.M13 = -matrix.M13;
            r.M14 = -matrix.M14;
            r.M21 = -matrix.M21;
            r.M22 = -matrix.M22;
            r.M23 = -matrix.M23;
            r.M24 = -matrix.M24;
            r.M31 = -matrix.M31;
            r.M32 = -matrix.M32;
            r.M33 = -matrix.M33;
            r.M34 = -matrix.M34;
            r.M41 = -matrix.M41;
            r.M42 = -matrix.M42;
            r.M43 = -matrix.M43;
            r.M44 = -matrix.M44;

            result = r;
        }
        /// <summary>
        /// 两个矩阵相乘
        /// </summary>
        /// <param name="left">矩阵对象</param>
        /// <param name="right">矩阵对象</param>
        /// <returns>返回计算后的矩阵</returns>
        public static Matrix Multiply(Matrix left, Matrix right)
        {
            Matrix result;
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
        /// <summary>
        /// 两个矩阵相乘
        /// </summary>
        /// <param name="left">矩阵对象</param>
        /// <param name="right">矩阵对象</param>
        /// <param name="result">计算后的矩阵</param>
        public static void Multiply(ref Matrix left, ref Matrix right, out Matrix result)
        {
            Matrix r;
            r.M11 = (left.M11 * right.M11) + (left.M12 * right.M21) + (left.M13 * right.M31) + (left.M14 * right.M41);
            r.M12 = (left.M11 * right.M12) + (left.M12 * right.M22) + (left.M13 * right.M32) + (left.M14 * right.M42);
            r.M13 = (left.M11 * right.M13) + (left.M12 * right.M23) + (left.M13 * right.M33) + (left.M14 * right.M43);
            r.M14 = (left.M11 * right.M14) + (left.M12 * right.M24) + (left.M13 * right.M34) + (left.M14 * right.M44);
            r.M21 = (left.M21 * right.M11) + (left.M22 * right.M21) + (left.M23 * right.M31) + (left.M24 * right.M41);
            r.M22 = (left.M21 * right.M12) + (left.M22 * right.M22) + (left.M23 * right.M32) + (left.M24 * right.M42);
            r.M23 = (left.M21 * right.M13) + (left.M22 * right.M23) + (left.M23 * right.M33) + (left.M24 * right.M43);
            r.M24 = (left.M21 * right.M14) + (left.M22 * right.M24) + (left.M23 * right.M34) + (left.M24 * right.M44);
            r.M31 = (left.M31 * right.M11) + (left.M32 * right.M21) + (left.M33 * right.M31) + (left.M34 * right.M41);
            r.M32 = (left.M31 * right.M12) + (left.M32 * right.M22) + (left.M33 * right.M32) + (left.M34 * right.M42);
            r.M33 = (left.M31 * right.M13) + (left.M32 * right.M23) + (left.M33 * right.M33) + (left.M34 * right.M43);
            r.M34 = (left.M31 * right.M14) + (left.M32 * right.M24) + (left.M33 * right.M34) + (left.M34 * right.M44);
            r.M41 = (left.M41 * right.M11) + (left.M42 * right.M21) + (left.M43 * right.M31) + (left.M44 * right.M41);
            r.M42 = (left.M41 * right.M12) + (left.M42 * right.M22) + (left.M43 * right.M32) + (left.M44 * right.M42);
            r.M43 = (left.M41 * right.M13) + (left.M42 * right.M23) + (left.M43 * right.M33) + (left.M44 * right.M43);
            r.M44 = (left.M41 * right.M14) + (left.M42 * right.M24) + (left.M43 * right.M34) + (left.M44 * right.M44);

            result = r;
        }
        /// <summary>
        /// 两个矩阵相乘
        /// </summary>
        /// <param name="left">矩阵指针</param>
        /// <param name="right">矩阵指针</param>
        /// <param name="result">计算后的结果</param>
        /// <param name="count">索引</param>
	    public unsafe static void Multiply(Matrix* left, Matrix* right, Matrix* result, int count)
        {
            for (int i = 0; i < count; ++i)
            {
                IDllImportApi.v3dxMatrixMultiply(&left[i],
                    &right[i],
                    &result[i]);
            }
        }
        /// <summary>
        /// 两个矩阵数组相乘
        /// </summary>
        /// <param name="left">矩阵数组</param>
        /// <param name="right">矩阵数组</param>
        /// <param name="result">计算后的矩阵数组</param>
        /// <param name="offset">矩阵数组的索引值</param>
        /// <param name="count">矩阵的维数</param>
        public static void Multiply(Matrix[] left, Matrix[] right, Matrix[] result, int offset, int count)
        {
            if (left.Length != right.Length)
                throw new ArgumentException("Left and right arrays must be the same size.", "right");
            if (right.Length != result.Length)
                throw new ArgumentException("Result array must be the same size as input arrays.", "result");
            //Utilities.CheckArrayBounds( left, offset, count );

            unsafe
            {
                fixed (Matrix* pinnedLeft = &left[offset])
                {
                    fixed (Matrix* pinnedRight = &right[offset])
                    {
                        fixed (Matrix* pinnedResult = &result[offset])
                        {
                            Multiply(pinnedLeft, pinnedRight, pinnedResult, count);
                        }
                    }
                }
            }
        }
        /// <summary>
        /// 矩阵数组与矩阵相乘
        /// </summary>
        /// <param name="left">矩阵数组</param>
        /// <param name="right">矩阵对象</param>
        /// <param name="result">计算后的矩阵数组</param>
        /// <param name="offset">矩阵数组的索引值</param>
        /// <param name="count">矩阵的维数</param>
	    public static void Multiply(Matrix[] left, Matrix right, Matrix[] result, int offset, int count)
        {
            if (left.Length != result.Length)
                throw new ArgumentException("Result array must be the same size as the input array.", "result");
            //Utilities.CheckArrayBounds( left, offset, count );

            unsafe
            {
                fixed (Matrix* pinnedLeft = &left[offset])
                {
                    fixed (Matrix* pinnedResult = &result[offset])
                    {
                        for (int i = 0; i < count; ++i)
                        {
                            IDllImportApi.v3dxMatrixMultiply(&pinnedLeft[i],
                                &right,
                                &pinnedResult[i]);
                        }
                    }
                }
            }
        }
        /// <summary>
        /// 矩阵的数乘
        /// </summary>
        /// <param name="left">矩阵对象</param>
        /// <param name="right">常数</param>
        /// <returns>返回计算后的矩阵</returns>
        public static Matrix Multiply(Matrix left, float right)
        {
            Matrix result;
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
        /// <summary>
        /// 矩阵的数乘
        /// </summary>
        /// <param name="left">矩阵对象</param>
        /// <param name="right">常数</param>
        /// <param name="result">计算后的矩阵</param>
        public static void Multiply(ref Matrix left, float right, out Matrix result)
        {
            Matrix r;
            r.M11 = left.M11 * right;
            r.M12 = left.M12 * right;
            r.M13 = left.M13 * right;
            r.M14 = left.M14 * right;
            r.M21 = left.M21 * right;
            r.M22 = left.M22 * right;
            r.M23 = left.M23 * right;
            r.M24 = left.M24 * right;
            r.M31 = left.M31 * right;
            r.M32 = left.M32 * right;
            r.M33 = left.M33 * right;
            r.M34 = left.M34 * right;
            r.M41 = left.M41 * right;
            r.M42 = left.M42 * right;
            r.M43 = left.M43 * right;
            r.M44 = left.M44 * right;

            result = r;
        }
        /// <summary>
        /// 两个矩阵相除
        /// </summary>
        /// <param name="left">矩阵对象</param>
        /// <param name="right">矩阵对象</param>
        /// <returns>返回计算后的矩阵</returns>
        public static Matrix Divide(Matrix left, Matrix right)
        {
            Matrix result;
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
        /// <summary>
        /// 两个矩阵相除
        /// </summary>
        /// <param name="left">矩阵对象</param>
        /// <param name="right">矩阵对象</param>
        /// <param name="result">计算后的矩阵</param>
        public static void Divide(ref Matrix left, ref Matrix right, out Matrix result)
        {
            Matrix r;
            r.M11 = left.M11 / right.M11;
            r.M12 = left.M12 / right.M12;
            r.M13 = left.M13 / right.M13;
            r.M14 = left.M14 / right.M14;
            r.M21 = left.M21 / right.M21;
            r.M22 = left.M22 / right.M22;
            r.M23 = left.M23 / right.M23;
            r.M24 = left.M24 / right.M24;
            r.M31 = left.M31 / right.M31;
            r.M32 = left.M32 / right.M32;
            r.M33 = left.M33 / right.M33;
            r.M34 = left.M34 / right.M34;
            r.M41 = left.M41 / right.M41;
            r.M42 = left.M42 / right.M42;
            r.M43 = left.M43 / right.M43;
            r.M44 = left.M44 / right.M44;

            result = r;
        }
        /// <summary>
        /// 矩阵与常数相除
        /// </summary>
        /// <param name="left">矩阵对象</param>
        /// <param name="right">常数值</param>
        /// <returns>返回计算后的矩阵</returns>
        public static Matrix Divide(Matrix left, float right)
        {
            Matrix result;
            float inv = 1.0f / right;

            result.M11 = left.M11 * inv;
            result.M12 = left.M12 * inv;
            result.M13 = left.M13 * inv;
            result.M14 = left.M14 * inv;
            result.M21 = left.M21 * inv;
            result.M22 = left.M22 * inv;
            result.M23 = left.M23 * inv;
            result.M24 = left.M24 * inv;
            result.M31 = left.M31 * inv;
            result.M32 = left.M32 * inv;
            result.M33 = left.M33 * inv;
            result.M34 = left.M34 * inv;
            result.M41 = left.M41 * inv;
            result.M42 = left.M42 * inv;
            result.M43 = left.M43 * inv;
            result.M44 = left.M44 * inv;
            return result;
        }
        /// <summary>
        /// 矩阵与常数相除
        /// </summary>
        /// <param name="left">矩阵对象</param>
        /// <param name="right">常数</param>
        /// <param name="result">计算后的矩阵</param>
        public static void Divide(ref Matrix left, float right, out Matrix result)
        {
            float inv = 1.0f / right;

            Matrix r;
            r.M11 = left.M11 * inv;
            r.M12 = left.M12 * inv;
            r.M13 = left.M13 * inv;
            r.M14 = left.M14 * inv;
            r.M21 = left.M21 * inv;
            r.M22 = left.M22 * inv;
            r.M23 = left.M23 * inv;
            r.M24 = left.M24 * inv;
            r.M31 = left.M31 * inv;
            r.M32 = left.M32 * inv;
            r.M33 = left.M33 * inv;
            r.M34 = left.M34 * inv;
            r.M41 = left.M41 * inv;
            r.M42 = left.M42 * inv;
            r.M43 = left.M43 * inv;
            r.M44 = left.M44 * inv;

            result = r;
        }
        /// <summary>
        /// 矩阵插值计算
        /// </summary>
        /// <param name="value1">矩阵对象</param>
        /// <param name="value2">矩阵对象</param>
        /// <param name="amount">插值</param>
        /// <returns>返回计算后的矩阵</returns>
        public static Matrix Lerp(Matrix value1, Matrix value2, float amount)
        {
            Matrix result;
            result.M11 = value1.M11 + ((value2.M11 - value1.M11) * amount);
            result.M12 = value1.M12 + ((value2.M12 - value1.M12) * amount);
            result.M13 = value1.M13 + ((value2.M13 - value1.M13) * amount);
            result.M14 = value1.M14 + ((value2.M14 - value1.M14) * amount);
            result.M21 = value1.M21 + ((value2.M21 - value1.M21) * amount);
            result.M22 = value1.M22 + ((value2.M22 - value1.M22) * amount);
            result.M23 = value1.M23 + ((value2.M23 - value1.M23) * amount);
            result.M24 = value1.M24 + ((value2.M24 - value1.M24) * amount);
            result.M31 = value1.M31 + ((value2.M31 - value1.M31) * amount);
            result.M32 = value1.M32 + ((value2.M32 - value1.M32) * amount);
            result.M33 = value1.M33 + ((value2.M33 - value1.M33) * amount);
            result.M34 = value1.M34 + ((value2.M34 - value1.M34) * amount);
            result.M41 = value1.M41 + ((value2.M41 - value1.M41) * amount);
            result.M42 = value1.M42 + ((value2.M42 - value1.M42) * amount);
            result.M43 = value1.M43 + ((value2.M43 - value1.M43) * amount);
            result.M44 = value1.M44 + ((value2.M44 - value1.M44) * amount);
            return result;
        }
        /// <summary>
        /// 矩阵插值计算
        /// </summary>
        /// <param name="value1">矩阵对象</param>
        /// <param name="value2">矩阵对象</param>
        /// <param name="amount">插值</param>
        /// <param name="result">计算后的矩阵</param>
        public static void Lerp(ref Matrix value1, ref Matrix value2, float amount, out Matrix result)
        {
            Matrix r;
            r.M11 = value1.M11 + ((value2.M11 - value1.M11) * amount);
            r.M12 = value1.M12 + ((value2.M12 - value1.M12) * amount);
            r.M13 = value1.M13 + ((value2.M13 - value1.M13) * amount);
            r.M14 = value1.M14 + ((value2.M14 - value1.M14) * amount);
            r.M21 = value1.M21 + ((value2.M21 - value1.M21) * amount);
            r.M22 = value1.M22 + ((value2.M22 - value1.M22) * amount);
            r.M23 = value1.M23 + ((value2.M23 - value1.M23) * amount);
            r.M24 = value1.M24 + ((value2.M24 - value1.M24) * amount);
            r.M31 = value1.M31 + ((value2.M31 - value1.M31) * amount);
            r.M32 = value1.M32 + ((value2.M32 - value1.M32) * amount);
            r.M33 = value1.M33 + ((value2.M33 - value1.M33) * amount);
            r.M34 = value1.M34 + ((value2.M34 - value1.M34) * amount);
            r.M41 = value1.M41 + ((value2.M41 - value1.M41) * amount);
            r.M42 = value1.M42 + ((value2.M42 - value1.M42) * amount);
            r.M43 = value1.M43 + ((value2.M43 - value1.M43) * amount);
            r.M44 = value1.M44 + ((value2.M44 - value1.M44) * amount);

            result = r;
        }
        /// <summary>
        /// 面板显示
        /// </summary>
        /// <param name="objectPosition">对象位置坐标</param>
        /// <param name="cameraPosition">摄像机坐标</param>
        /// <param name="cameraUpVector">摄像机的UP向量</param>
        /// <param name="cameraForwardVector">摄像机Forward向量</param>
        /// <returns>返回计算后的矩阵</returns>
	    public static Matrix Billboard(Vector3 objectPosition, Vector3 cameraPosition, Vector3 cameraUpVector, Vector3 cameraForwardVector)
        {
            Matrix result;
            Vector3 difference = objectPosition - cameraPosition;
            Vector3 crossed;
            Vector3 final;

            float lengthSq = difference.LengthSquared();
            if (lengthSq < 0.0001f)
                difference = -cameraForwardVector;
            else
                difference *= (float)(1.0f / Math.Sqrt(lengthSq));

            Vector3.Cross(ref cameraUpVector, ref difference, out crossed);
            crossed.Normalize();
            Vector3.Cross(ref difference, ref crossed, out final);

            result.M11 = crossed.X;
            result.M12 = crossed.Y;
            result.M13 = crossed.Z;
            result.M14 = 0.0f;
            result.M21 = final.X;
            result.M22 = final.Y;
            result.M23 = final.Z;
            result.M24 = 0.0f;
            result.M31 = difference.X;
            result.M32 = difference.Y;
            result.M33 = difference.Z;
            result.M34 = 0.0f;
            result.M41 = objectPosition.X;
            result.M42 = objectPosition.Y;
            result.M43 = objectPosition.Z;
            result.M44 = 1.0f;

            return result;
        }
        /// <summary>
        /// 面板显示
        /// </summary>
        /// <param name="objectPosition">对象位置坐标</param>
        /// <param name="cameraPosition">摄像机坐标</param>
        /// <param name="cameraUpVector">摄像机的UP向量</param>
        /// <param name="cameraForwardVector">摄像机Forward向量</param>
        /// <param name="result">计算后的矩阵</param>
	    public static void Billboard(ref Vector3 objectPosition, ref Vector3 cameraPosition, ref Vector3 cameraUpVector, ref Vector3 cameraForwardVector, out Matrix result)
        {
            Vector3 difference = objectPosition - cameraPosition;
            Vector3 crossed;
            Vector3 final;

            float lengthSq = difference.LengthSquared();
            if (lengthSq < 0.0001f)
                difference = -cameraForwardVector;
            else
                difference *= (float)(1.0f / Math.Sqrt(lengthSq));

            Vector3.Cross(ref cameraUpVector, ref difference, out crossed);
            crossed.Normalize();
            Vector3.Cross(ref difference, ref crossed, out final);

            result.M11 = crossed.X;
            result.M12 = crossed.Y;
            result.M13 = crossed.Z;
            result.M14 = 0.0f;
            result.M21 = final.X;
            result.M22 = final.Y;
            result.M23 = final.Z;
            result.M24 = 0.0f;
            result.M31 = difference.X;
            result.M32 = difference.Y;
            result.M33 = difference.Z;
            result.M34 = 0.0f;
            result.M41 = objectPosition.X;
            result.M42 = objectPosition.Y;
            result.M43 = objectPosition.Z;
            result.M44 = 1.0f;
        }
        /// <summary>
        /// 沿X轴旋转
        /// </summary>
        /// <param name="angle">旋转角度</param>
        /// <returns>计算后的矩阵</returns>
        public static Matrix RotationX(float angle)
        {
            Matrix result;
            float cos = (float)(Math.Cos((double)(angle)));
            float sin = (float)(Math.Sin((double)(angle)));

            result.M11 = 1.0f;
            result.M12 = 0.0f;
            result.M13 = 0.0f;
            result.M14 = 0.0f;
            result.M21 = 0.0f;
            result.M22 = cos;
            result.M23 = sin;
            result.M24 = 0.0f;
            result.M31 = 0.0f;
            result.M32 = -sin;
            result.M33 = cos;
            result.M34 = 0.0f;
            result.M41 = 0.0f;
            result.M42 = 0.0f;
            result.M43 = 0.0f;
            result.M44 = 1.0f;

            //D3DXMatrixRotationX((D3DXMATRIX*)&result, angle);

            return result;
        }
        /// <summary>
        /// 沿X轴旋转
        /// </summary>
        /// <param name="angle">旋转角度</param>
        /// <param name="result">计算后的矩阵</param>
        public static void RotationX(float angle, out Matrix result)
        {
            float cos = (float)(Math.Cos((double)(angle)));
            float sin = (float)(Math.Sin((double)(angle)));

            result.M11 = 1.0f;
            result.M12 = 0.0f;
            result.M13 = 0.0f;
            result.M14 = 0.0f;
            result.M21 = 0.0f;
            result.M22 = cos;
            result.M23 = sin;
            result.M24 = 0.0f;
            result.M31 = 0.0f;
            result.M32 = -sin;
            result.M33 = cos;
            result.M34 = 0.0f;
            result.M41 = 0.0f;
            result.M42 = 0.0f;
            result.M43 = 0.0f;
            result.M44 = 1.0f;
        }
        /// <summary>
        /// 沿Y轴旋转
        /// </summary>
        /// <param name="angle">旋转角度</param>
        /// <returns>计算后的矩阵</returns>
        public static Matrix RotationY(float angle)
        {
            Matrix result;
            float cos = (float)(Math.Cos((double)(angle)));
            float sin = (float)(Math.Sin((double)(angle)));

            result.M11 = cos;
            result.M12 = 0.0f;
            result.M13 = -sin;
            result.M14 = 0.0f;
            result.M21 = 0.0f;
            result.M22 = 1.0f;
            result.M23 = 0.0f;
            result.M24 = 0.0f;
            result.M31 = sin;
            result.M32 = 0.0f;
            result.M33 = cos;
            result.M34 = 0.0f;
            result.M41 = 0.0f;
            result.M42 = 0.0f;
            result.M43 = 0.0f;
            result.M44 = 1.0f;

            //D3DXMatrixRotationY((D3DXMATRIX*)&result, angle);

            return result;
        }
        /// <summary>
        /// 沿Y轴旋转
        /// </summary>
        /// <param name="angle">旋转角度</param>
        /// <param name="result">计算后的矩阵</param>
        public static void RotationY(float angle, out Matrix result)
        {
            float cos = (float)(Math.Cos((double)(angle)));
            float sin = (float)(Math.Sin((double)(angle)));

            result.M11 = cos;
            result.M12 = 0.0f;
            result.M13 = -sin;
            result.M14 = 0.0f;
            result.M21 = 0.0f;
            result.M22 = 1.0f;
            result.M23 = 0.0f;
            result.M24 = 0.0f;
            result.M31 = sin;
            result.M32 = 0.0f;
            result.M33 = cos;
            result.M34 = 0.0f;
            result.M41 = 0.0f;
            result.M42 = 0.0f;
            result.M43 = 0.0f;
            result.M44 = 1.0f;
        }
        /// <summary>
        /// 沿Z轴旋转
        /// </summary>
        /// <param name="angle">旋转角度</param>
        /// <returns>返回旋转后的矩阵</returns>
        public static Matrix RotationZ(float angle)
        {
            Matrix result;
            float cos = (float)(Math.Cos((double)(angle)));
            float sin = (float)(Math.Sin((double)(angle)));

            result.M11 = cos;
            result.M12 = sin;
            result.M13 = 0.0f;
            result.M14 = 0.0f;
            result.M21 = -sin;
            result.M22 = cos;
            result.M23 = 0.0f;
            result.M24 = 0.0f;
            result.M31 = 0.0f;
            result.M32 = 0.0f;
            result.M33 = 1.0f;
            result.M34 = 0.0f;
            result.M41 = 0.0f;
            result.M42 = 0.0f;
            result.M43 = 0.0f;
            result.M44 = 1.0f;

            //D3DXMatrixRotationZ((D3DXMATRIX*)&result, angle);

            return result;
        }
        /// <summary>
        /// 沿Z轴旋转
        /// </summary>
        /// <param name="angle">旋转角度</param>
        /// <param name="result">旋转后的矩阵</param>
        public static void RotationZ(float angle, out Matrix result)
        {
            float cos = (float)(Math.Cos((double)(angle)));
            float sin = (float)(Math.Sin((double)(angle)));

            result.M11 = cos;
            result.M12 = sin;
            result.M13 = 0.0f;
            result.M14 = 0.0f;
            result.M21 = -sin;
            result.M22 = cos;
            result.M23 = 0.0f;
            result.M24 = 0.0f;
            result.M31 = 0.0f;
            result.M32 = 0.0f;
            result.M33 = 1.0f;
            result.M34 = 0.0f;
            result.M41 = 0.0f;
            result.M42 = 0.0f;
            result.M43 = 0.0f;
            result.M44 = 1.0f;
        }
        public static Matrix RotationQuaternion(Quaternion quaternion)
        {
            Matrix result;

            float xx = quaternion.X * quaternion.X;
            float yy = quaternion.Y * quaternion.Y;
            float zz = quaternion.Z * quaternion.Z;
            float xy = quaternion.X * quaternion.Y;
            float zw = quaternion.Z * quaternion.W;
            float zx = quaternion.Z * quaternion.X;
            float yw = quaternion.Y * quaternion.W;
            float yz = quaternion.Y * quaternion.Z;
            float xw = quaternion.X * quaternion.W;
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

            //D3DXMatrixRotationQuaternion((D3DXMATRIX*)&result, (D3DXQUATERNION*)&quaternion);

            return result;
        }
        /// <summary>
        /// 根据旋转四元数计算的矩阵
        /// </summary>
        /// <param name="rotation">旋转四元数</param>
        /// <param name="result">计算后的矩阵</param>
        public static void RotationQuaternion(ref Quaternion rotation, out Matrix result)
        {
            float xx = rotation.X * rotation.X;
            float yy = rotation.Y * rotation.Y;
            float zz = rotation.Z * rotation.Z;
            float xy = rotation.X * rotation.Y;
            float zw = rotation.Z * rotation.W;
            float zx = rotation.Z * rotation.X;
            float yw = rotation.Y * rotation.W;
            float yz = rotation.Y * rotation.Z;
            float xw = rotation.X * rotation.W;
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
        /// <summary>
        /// 根据轴进行旋转
        /// </summary>
        /// <param name="axis">旋转轴向量</param>
        /// <param name="angle">旋转角度</param>
        /// <returns>返回旋转后的矩阵</returns>
        public static Matrix RotationAxis(Vector3 axis, float angle)
        {
            if (axis.LengthSquared() != 1.0f)
                axis.Normalize();

            Matrix result;
            float x = axis.X;
            float y = axis.Y;
            float z = axis.Z;
            float cos = (float)(Math.Cos((double)(angle)));
            float sin = (float)(Math.Sin((double)(angle)));
            float xx = x * x;
            float yy = y * y;
            float zz = z * z;
            float xy = x * y;
            float xz = x * z;
            float yz = y * z;

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

            return result;
        }
        /// <summary>
        /// 根据轴进行旋转
        /// </summary>
        /// <param name="axis">旋转轴向量</param>
        /// <param name="angle">旋转角度</param>
        /// <param name="result">旋转后的矩阵</param>
        public static void RotationAxis(ref Vector3 axis, float angle, out Matrix result)
        {
            if (axis.LengthSquared() != 1.0f)
                axis.Normalize();

            float x = axis.X;
            float y = axis.Y;
            float z = axis.Z;
            float cos = (float)(Math.Cos((double)(angle)));
            float sin = (float)(Math.Sin((double)(angle)));
            float xx = x * x;
            float yy = y * y;
            float zz = z * z;
            float xy = x * y;
            float xz = x * z;
            float yz = y * z;

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
        /// <summary>
        /// 根据欧拉角设置矩阵
        /// </summary>
        /// <param name="yaw">航向角</param>
        /// <param name="pitch">俯仰角</param>
        /// <param name="roll">翻滚角</param>
        /// <returns>返回计算后的矩阵</returns>
        public static Matrix RotationYawPitchRoll(float yaw, float pitch, float roll)
        {
            Matrix result;
            Quaternion quaternion;
            Quaternion.RotationYawPitchRoll(yaw, pitch, roll, out quaternion);
            RotationQuaternion(ref quaternion, out result);
            return result;
        }
        /// <summary>
        /// 根据欧拉角设置矩阵
        /// </summary>
        /// <param name="yaw">航向角</param>
        /// <param name="pitch">俯仰角</param>
        /// <param name="roll">翻滚角</param>
        /// <param name="result">计算后的矩阵</param>
        public static void RotationYawPitchRoll(float yaw, float pitch, float roll, out Matrix result)
        {
            Quaternion quaternion;
            Quaternion.RotationYawPitchRoll(yaw, pitch, roll, out quaternion);
            RotationQuaternion(ref quaternion, out result);
        }
        /// <summary>
        /// 平移矩阵
        /// </summary>
        /// <param name="x">X方向移动的距离</param>
        /// <param name="y">Y方向移动的距离</param>
        /// <param name="z">Z方向移动的距离</param>
        /// <returns>平移后的矩阵</returns>
        public static Matrix Translate(float x, float y, float z)
        {
            Matrix result;
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
            result.M41 = x;
            result.M42 = y;
            result.M43 = z;
            result.M44 = 1.0f;
            return result;
        }
        /// <summary>
        /// 平移矩阵
        /// </summary>
        /// <param name="x">X方向移动的距离</param>
        /// <param name="y">Y方向移动的距离</param>
        /// <param name="z">Z方向移动的距离</param>
        /// <param name="result">平移后的矩阵</param>
        public static void Translate(float x, float y, float z, out Matrix result)
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
            result.M41 = x;
            result.M42 = y;
            result.M43 = z;
            result.M44 = 1.0f;
        }
        /// <summary>
        /// 平移矩阵
        /// </summary>
        /// <param name="translation">移动的三维向量</param>
        /// <returns>返回平移后的矩阵</returns>
        public static Matrix Translate(Vector3 translation)
        {
            Matrix result;
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
            return result;
        }
        /// <summary>
        /// 平移矩阵
        /// </summary>
        /// <param name="translation">移动的三维向量</param>
        /// <param name="result">平移后的矩阵</param>
        public static void Translate(ref Vector3 translation, out Matrix result)
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
        public static Matrix Scaling(float value)
        {
            return Scaling(value, value, value);
        }
        /// <summary>
        /// 缩放矩阵
        /// </summary>
        /// <param name="x">X方向的缩放值</param>
        /// <param name="y">Y方向的缩放值</param>
        /// <param name="z">Z方向的缩放值</param>
        /// <returns>返回计算后的矩阵</returns>
        public static Matrix Scaling(float x, float y, float z)
        {
            Matrix result;
            result.M11 = x;
            result.M12 = 0.0f;
            result.M13 = 0.0f;
            result.M14 = 0.0f;
            result.M21 = 0.0f;
            result.M22 = y;
            result.M23 = 0.0f;
            result.M24 = 0.0f;
            result.M31 = 0.0f;
            result.M32 = 0.0f;
            result.M33 = z;
            result.M34 = 0.0f;
            result.M41 = 0.0f;
            result.M42 = 0.0f;
            result.M43 = 0.0f;
            result.M44 = 1.0f;
            return result;
        }
        /// <summary>
        /// 缩放矩阵
        /// </summary>
        /// <param name="x">X方向的缩放值</param>
        /// <param name="y">Y方向的缩放值</param>
        /// <param name="z">Z方向的缩放值</param>
        /// <param name="result">计算后的矩阵</param>
        public static void Scaling(float x, float y, float z, out Matrix result)
        {
            result.M11 = x;
            result.M12 = 0.0f;
            result.M13 = 0.0f;
            result.M14 = 0.0f;
            result.M21 = 0.0f;
            result.M22 = y;
            result.M23 = 0.0f;
            result.M24 = 0.0f;
            result.M31 = 0.0f;
            result.M32 = 0.0f;
            result.M33 = z;
            result.M34 = 0.0f;
            result.M41 = 0.0f;
            result.M42 = 0.0f;
            result.M43 = 0.0f;
            result.M44 = 1.0f;
        }
        /// <summary>
        /// 缩放矩阵
        /// </summary>
        /// <param name="scaling">三维缩放向量</param>
        /// <returns>返回计算后的矩阵</returns>
        public static Matrix Scaling(Vector3 scaling)
        {
            Matrix result;
            result.M11 = scaling.X;
            result.M12 = 0.0f;
            result.M13 = 0.0f;
            result.M14 = 0.0f;
            result.M21 = 0.0f;
            result.M22 = scaling.Y;
            result.M23 = 0.0f;
            result.M24 = 0.0f;
            result.M31 = 0.0f;
            result.M32 = 0.0f;
            result.M33 = scaling.Z;
            result.M34 = 0.0f;
            result.M41 = 0.0f;
            result.M42 = 0.0f;
            result.M43 = 0.0f;
            result.M44 = 1.0f;
            return result;
        }
        /// <summary>
        /// 缩放矩阵
        /// </summary>
        /// <param name="scaling">三维缩放向量</param>
        /// <param name="result">计算后的矩阵</param>
        public static void Scaling(ref Vector3 scaling, out Matrix result)
        {
            result.M11 = scaling.X;
            result.M12 = 0.0f;
            result.M13 = 0.0f;
            result.M14 = 0.0f;
            result.M21 = 0.0f;
            result.M22 = scaling.Y;
            result.M23 = 0.0f;
            result.M24 = 0.0f;
            result.M31 = 0.0f;
            result.M32 = 0.0f;
            result.M33 = scaling.Z;
            result.M34 = 0.0f;
            result.M41 = 0.0f;
            result.M42 = 0.0f;
            result.M43 = 0.0f;
            result.M44 = 1.0f;
        }
        /// <summary>
        /// 仿射移动
        /// </summary>
        /// <param name="scaling">放大倍数</param>
        /// <param name="rotationCenter">旋转的中心点</param>
        /// <param name="rotation">旋转四元数</param>
        /// <param name="translation">平移向量</param>
        /// <returns>返回平移后的矩阵</returns>
        public static Matrix AffineTransformation(float scaling, Vector3 rotationCenter, Quaternion rotation, Vector3 translation)
        {
            Matrix result;
            unsafe
            {
                IDllImportApi.v3dxMatrixAffineTransformation((Matrix*)&result, scaling, (Vector3*)&rotationCenter, (Quaternion*)&rotation, (Vector3*)&translation);
            }
            return result;
        }
        /// <summary>
        /// 仿射移动
        /// </summary>
        /// <param name="scaling">放大倍数</param>
        /// <param name="rotationCenter">旋转的中心点</param>
        /// <param name="rotation">旋转四元数</param>
        /// <param name="translation">移动的向量</param>
        /// <param name="result">平移后的矩阵</param>
        public static void AffineTransformation(float scaling, ref Vector3 rotationCenter, ref Quaternion rotation, ref Vector3 translation, out Matrix result)
        {
            unsafe
            {
                fixed (Vector3* pinRotationCenter = &rotationCenter)
                {
                    fixed (Quaternion* pinRotation = &rotation)
                    {
                        fixed (Vector3* pinTranslation = &translation)
                        {
                            fixed (Matrix* pinResult = &result)
                            {
                                IDllImportApi.v3dxMatrixAffineTransformation(pinResult, scaling, pinRotationCenter, pinRotation, pinTranslation);
                            }
                        }
                    }
                }
            }
        }

        //public static Matrix AffineTransformation2D(float scaling, Vector2 rotationCenter, float rotation, Vector2 translation)
        //{
        //    Matrix result;
        //    unsafe
        //    {
        //        IDllImportApi.D3DXMatrixAffineTransformation2D((Matrix*)&result, scaling, (Vector2*)&rotationCenter, rotation, (Vector2*)&translation);
        //    }
        //    return result;
        //}

        //public static void AffineTransformation2D(float scaling, ref Vector2 rotationCenter, float rotation, ref Vector2 translation, out Matrix result)
        //{
        //    unsafe
        //    {
        //        fixed (Vector2* pinRotationCenter = &rotationCenter)
        //        {
        //            fixed(Vector2* pinTranslation = &translation)
        //            {
        //                fixed (Matrix* pinResult = &result)
        //                {
        //                    IDllImportApi.D3DXMatrixAffineTransformation2D(pinResult, scaling, pinRotationCenter, rotation, pinTranslation);
        //                }
        //            }
        //        }
        //    }
        //}
        /// <summary>
        /// 变换矩阵
        /// </summary>
        /// <param name="scaling">缩放向量</param>
        /// <param name="rotation">旋转四元数</param>
        /// <param name="translation">平移向量</param>
        /// <returns>返回变换后的矩阵</returns>
        public static Matrix Transformation(Vector3 scaling, Quaternion rotation, Vector3 translation)
        {
            Matrix result;
            unsafe
            {
                IDllImportApi.v3dxMatrixTransformationOrigin((Matrix*)&result, (Vector3*)&scaling, (Quaternion*)&rotation, (Vector3*)&translation);
            }
            return result;
        }
        public static Matrix Transformation(Quaternion rotation, Vector3 translation)
        {
            Matrix result;
            unsafe
            {
                IDllImportApi.v3dxMatrixTransformationOrigin((Matrix*)&result, (Vector3*)0, (Quaternion*)&rotation, (Vector3*)&translation);
            }
            return result;
        }
        /// <summary>
        /// 变换矩阵
        /// </summary>
        /// <param name="scaling">缩放向量</param>
        /// <param name="rotation">旋转四元数</param>
        /// <param name="translation">平移向量</param>
        /// <param name="result">变换后的矩阵</param>
        /// <returns>返回变换后的矩阵</returns>
        public static Matrix Transformation(Vector3 scaling, Quaternion rotation, Vector3 translation, out Matrix result)
        {
            unsafe
            {
                fixed (Matrix* pinResult = &result)
                {
                    IDllImportApi.v3dxMatrixTransformationOrigin(pinResult, (Vector3*)&scaling, (Quaternion*)&rotation, (Vector3*)&translation);
                }
            }
            return result;
        }
        /// <summary>
        /// 变换矩阵
        /// </summary>
        /// <param name="scalingCenter">缩放中心点坐标</param>
        /// <param name="scalingRotation">带缩放的旋转四元数</param>
        /// <param name="scaling">缩放向量</param>
        /// <param name="rotationCenter">旋转中心</param>
        /// <param name="rotation">旋转四元数</param>
        /// <param name="translation">平移向量</param>
        /// <returns>返回变换后的矩阵</returns>
        public static Matrix Transformation(Vector3 scalingCenter, Quaternion scalingRotation, Vector3 scaling, Vector3 rotationCenter, Quaternion rotation, Vector3 translation)
        {
            Matrix result;
            unsafe
            {
                IDllImportApi.v3dxMatrixTransformation((Matrix*)&result, (Vector3*)&scalingCenter, (Quaternion*)&scalingRotation, (Vector3*)&scaling, (Vector3*)&rotationCenter, (Quaternion*)&rotation, (Vector3*)&translation);
            }
            return result;
        }
        /// <summary>
        /// 变换矩阵
        /// </summary>
        /// <param name="scalingCenter">缩放中心点坐标</param>
        /// <param name="scalingRotation">带缩放的旋转四元数</param>
        /// <param name="scaling">缩放向量</param>
        /// <param name="rotationCenter">旋转中心</param>
        /// <param name="rotation">旋转四元数</param>
        /// <param name="translation">平移向量</param>
        /// <param name="result">变换后的矩阵</param>
        public static void Transformation(ref Vector3 scalingCenter, ref Quaternion scalingRotation, ref Vector3 scaling, ref Vector3 rotationCenter, ref Quaternion rotation, ref Vector3 translation, out Matrix result)
        {
            unsafe
            {
                fixed (Vector3* pinScalingCenter = &scalingCenter)
                {
                    fixed (Quaternion* pinScalingRotation = &scalingRotation)
                    {
                        fixed (Vector3* pinScaling = &scaling)
                        {
                            fixed (Vector3* pinRotationCenter = &rotationCenter)
                            {
                                fixed (Quaternion* pinRotation = &rotation)
                                {
                                    fixed (Vector3* pinTranslation = &translation)
                                    {
                                        fixed (Matrix* pinResult = &result)
                                        {
                                            IDllImportApi.v3dxMatrixTransformation(pinResult, pinScalingCenter, pinScalingRotation, pinScaling, pinRotationCenter, pinRotation, pinTranslation);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }


        }

        //public static Matrix Transformation2D(Vector2 scalingCenter, float scalingRotation, Vector2 scaling, Vector2 rotationCenter, float rotation, Vector2 translation)
        //{
        //    Matrix result;
        //    unsafe
        //    {
        //        IDllImportApi.D3DXMatrixTransformation2D((Matrix*)&result, (Vector2*)&scalingCenter, scalingRotation, (Vector2*)&scaling, (Vector2*)&rotationCenter, rotation, (Vector2*)&translation);
        //    }

        //    return result;
        //}

        //public static void Transformation2D(ref Vector2 scalingCenter, float scalingRotation, ref Vector2 scaling, ref Vector2 rotationCenter, float rotation, ref Vector2 translation, out Matrix result)
        //{
        //    unsafe
        //    {
        //        fixed(Vector2* pinScalingCenter = &scalingCenter)
        //        {
        //            fixed(Vector2* pinScaling = &scaling)
        //            {
        //                fixed(Vector2* pinRotationCenter = &rotationCenter)
        //                {
        //                    fixed(Vector2* pinTranslation = &translation)
        //                    {
        //                        fixed (Matrix* pinResult = &result)
        //                        {
        //                            IDllImportApi.D3DXMatrixTransformation2D(pinResult, pinScalingCenter, scalingRotation,
        //                                pinScaling, pinRotationCenter, rotation, pinTranslation);                        
        //                        }
        //                    }
        //                }
        //            }
        //        }
        //    }
        //}
        /// <summary>
        /// 视野矩阵
        /// </summary>
        /// <param name="eye">摄像机的位置坐标</param>
        /// <param name="target">目标对象的坐标</param>
        /// <param name="up">相机向上的方向在世界坐标中的方向</param>
        /// <returns>返回计算后的矩阵</returns>
        public static Matrix LookAtLH(Vector3 eye, Vector3 target, Vector3 up)
        {
            Matrix result;
            unsafe
            {
                IDllImportApi.v3dxMatrixLookAtLH((Matrix*)&result, (Vector3*)&eye, (Vector3*)&target, (Vector3*)&up);
            }
            return result;
        }
        /// <summary>
        /// 视野矩阵
        /// </summary>
        /// <param name="eye">摄像机的位置坐标</param>
        /// <param name="target">目标对象的坐标</param>
        /// <param name="up">相机向上的方向在世界坐标中的方向</param>
        /// <param name="result">计算后的矩阵</param>
        public static void LookAtLH(ref Vector3 eye, ref Vector3 target, ref Vector3 up, out Matrix result)
        {
            unsafe
            {
                fixed (Vector3* pinCamera = &eye)
                {
                    fixed (Vector3* pinTarget = &target)
                    {
                        fixed (Vector3* pinUp = &up)
                        {
                            fixed (Matrix* pinResult = &result)
                            {
                                IDllImportApi.v3dxMatrixLookAtLH(pinResult, pinCamera, pinTarget, pinUp);
                            }
                        }
                    }
                }
            }
        }
        public static Matrix CreateOrthographicOffCenter(float left, float right, float bottom, float top, float zNearPlane, float zFarPlane)
        {
            Matrix matrix;
            matrix.M11 = (float)(2.0 / ((double)right - (double)left));
            matrix.M12 = 0.0f;
            matrix.M13 = 0.0f;
            matrix.M14 = 0.0f;
            matrix.M21 = 0.0f;
            matrix.M22 = (float)(2.0 / ((double)top - (double)bottom));
            matrix.M23 = 0.0f;
            matrix.M24 = 0.0f;
            matrix.M31 = 0.0f;
            matrix.M32 = 0.0f;
            matrix.M33 = (float)(1.0 / ((double)zNearPlane - (double)zFarPlane));
            matrix.M34 = 0.0f;
            matrix.M41 = (float)(((double)left + (double)right) / ((double)left - (double)right));
            matrix.M42 = (float)(((double)top + (double)bottom) / ((double)bottom - (double)top));
            matrix.M43 = (float)((double)zNearPlane / ((double)zNearPlane - (double)zFarPlane));
            matrix.M44 = 1.0f;
            return matrix;
        }
        public static void CreateOrthographicOffCenter(float left, float right, float bottom, float top, float zNearPlane, float zFarPlane, out Matrix result)
        {
            result.M11 = (float)(2.0 / ((double)right - (double)left));
            result.M12 = 0.0f;
            result.M13 = 0.0f;
            result.M14 = 0.0f;
            result.M21 = 0.0f;
            result.M22 = (float)(2.0 / ((double)top - (double)bottom));
            result.M23 = 0.0f;
            result.M24 = 0.0f;
            result.M31 = 0.0f;
            result.M32 = 0.0f;
            result.M33 = (float)(1.0 / ((double)zNearPlane - (double)zFarPlane));
            result.M34 = 0.0f;
            result.M41 = (float)(((double)left + (double)right) / ((double)left - (double)right));
            result.M42 = (float)(((double)top + (double)bottom) / ((double)bottom - (double)top));
            result.M43 = (float)((double)zNearPlane / ((double)zNearPlane - (double)zFarPlane));
            result.M44 = 1.0f;
        }
        /// <summary>
        /// 平行投影
        /// </summary>
        /// <param name="width">宽</param>
        /// <param name="height">高</param>
        /// <param name="znear">最近的值</param>
        /// <param name="zfar">最远的值</param>
        /// <returns>返回投影矩阵</returns>
        public static Matrix OrthoLH(float width, float height, float znear, float zfar)
        {
            Matrix result;
            unsafe
            {
                IDllImportApi.v3dxMatrix4Ortho((Matrix*)&result, width, height, znear, zfar);
            }

            return result;
        }
        /// <summary>
        /// 平行投影
        /// </summary>
        /// <param name="width">宽</param>
        /// <param name="height">高</param>
        /// <param name="znear">最近的值</param>
        /// <param name="zfar">最远的值</param>
        /// <param name="result">投影矩阵</param>
        public static void OrthoLH(float width, float height, float znear, float zfar, out Matrix result)
        {
            unsafe
            {
                fixed (Matrix* pinResult = &result)
                {
                    IDllImportApi.v3dxMatrix4Ortho(pinResult, width, height, znear, zfar);
                }
            }
        }
        /// <summary>
        /// 透视投影
        /// </summary>
        /// <param name="fov">FOV值</param>
        /// <param name="aspect">方向</param>
        /// <param name="znear">最近的值</param>
        /// <param name="zfar">最远的值</param>
        /// <returns>返回透视投影矩阵</returns>
        public static Matrix PerspectiveFovLH(float fov, float aspect, float znear, float zfar)
        {
            Matrix result;
            unsafe
            {
                IDllImportApi.v3dxMatrix4Perspective((Matrix*)&result, fov, aspect, znear, zfar);
            }
            return result;
        }
        /// <summary>
        /// 透视投影
        /// </summary>
        /// <param name="fov">FOV值</param>
        /// <param name="aspect">方向</param>
        /// <param name="znear">最近的值</param>
        /// <param name="zfar">最远的值</param>
        /// <param name="result">透视投影矩阵</param>
        public static void PerspectiveFovLH(float fov, float aspect, float znear, float zfar, out Matrix result)
        {
            unsafe
            {
                fixed (Matrix* pinResult = &result)
                {
                    IDllImportApi.v3dxMatrix4Perspective(pinResult, fov, aspect, znear, zfar);
                }
            }
        }
        /// <summary>
        /// 镜面反射
        /// </summary>
        /// <param name="plane">面对象</param>
        /// <returns>返回反射矩阵</returns>
        public static Matrix Reflection(Plane plane)
        {
            Matrix result;
            plane.Normalize();
            float x = plane.Normal.X;
            float y = plane.Normal.Y;
            float z = plane.Normal.Z;
            float x2 = -2.0f * x;
            float y2 = -2.0f * y;
            float z2 = -2.0f * z;
            result.M11 = (x2 * x) + 1.0f;
            result.M12 = y2 * x;
            result.M13 = z2 * x;
            result.M14 = 0.0f;
            result.M21 = x2 * y;
            result.M22 = (y2 * y) + 1.0f;
            result.M23 = z2 * y;
            result.M24 = 0.0f;
            result.M31 = x2 * z;
            result.M32 = y2 * z;
            result.M33 = (z2 * z) + 1.0f;
            result.M34 = 0.0f;
            result.M41 = x2 * plane.D;
            result.M42 = y2 * plane.D;
            result.M43 = z2 * plane.D;
            result.M44 = 1.0f;
            return result;
        }
        /// <summary>
        /// 镜面反射
        /// </summary>
        /// <param name="plane">面对象</param>
        /// <param name="result">反射矩阵</param>
        public static void Reflection(ref Plane plane, out Matrix result)
        {
            plane.Normalize();
            float x = plane.Normal.X;
            float y = plane.Normal.Y;
            float z = plane.Normal.Z;
            float x2 = -2.0f * x;
            float y2 = -2.0f * y;
            float z2 = -2.0f * z;
            result.M11 = (x2 * x) + 1.0f;
            result.M12 = y2 * x;
            result.M13 = z2 * x;
            result.M14 = 0.0f;
            result.M21 = x2 * y;
            result.M22 = (y2 * y) + 1.0f;
            result.M23 = z2 * y;
            result.M24 = 0.0f;
            result.M31 = x2 * z;
            result.M32 = y2 * z;
            result.M33 = (z2 * z) + 1.0f;
            result.M34 = 0.0f;
            result.M41 = x2 * plane.D;
            result.M42 = y2 * plane.D;
            result.M43 = z2 * plane.D;
            result.M44 = 1.0f;
        }
        /// <summary>
        /// 阴影矩阵
        /// </summary>
        /// <param name="light">光源坐标</param>
        /// <param name="plane">面对象</param>
        /// <returns>返回阴影矩阵</returns>
        public static Matrix Shadow(Vector4 light, Plane plane)
        {
            Matrix result;
            plane.Normalize();
            float dot = ((plane.Normal.X * light.X) + (plane.Normal.Y * light.Y)) + (plane.Normal.Z * light.Z);
            float x = -plane.Normal.X;
            float y = -plane.Normal.Y;
            float z = -plane.Normal.Z;
            float d = -plane.D;
            result.M11 = (x * light.X) + dot;
            result.M21 = y * light.X;
            result.M31 = z * light.X;
            result.M41 = d * light.X;
            result.M12 = x * light.Y;
            result.M22 = (y * light.Y) + dot;
            result.M32 = z * light.Y;
            result.M42 = d * light.Y;
            result.M13 = x * light.Z;
            result.M23 = y * light.Z;
            result.M33 = (z * light.Z) + dot;
            result.M43 = d * light.Z;
            result.M14 = 0.0f;
            result.M24 = 0.0f;
            result.M34 = 0.0f;
            result.M44 = dot;
            return result;
        }
        /// <summary>
        /// 阴影矩阵
        /// </summary>
        /// <param name="light">光源坐标</param>
        /// <param name="plane">面对象</param>
        /// <param name="result">阴影矩阵</param>
        public static void Shadow(ref Vector4 light, ref Plane plane, out Matrix result)
        {
            plane.Normalize();
            float dot = ((plane.Normal.X * light.X) + (plane.Normal.Y * light.Y)) + (plane.Normal.Z * light.Z);
            float x = -plane.Normal.X;
            float y = -plane.Normal.Y;
            float z = -plane.Normal.Z;
            float d = -plane.D;
            result.M11 = (x * light.X) + dot;
            result.M21 = y * light.X;
            result.M31 = z * light.X;
            result.M41 = d * light.X;
            result.M12 = x * light.Y;
            result.M22 = (y * light.Y) + dot;
            result.M32 = z * light.Y;
            result.M42 = d * light.Y;
            result.M13 = x * light.Z;
            result.M23 = y * light.Z;
            result.M33 = (z * light.Z) + dot;
            result.M43 = d * light.Z;
            result.M14 = 0.0f;
            result.M24 = 0.0f;
            result.M34 = 0.0f;
            result.M44 = dot;
        }
        /// <summary>
        /// 逆矩阵
        /// </summary>
        /// <param name="mat">矩阵对象</param>
        /// <returns>返回逆矩阵</returns>
        public static Matrix Invert(ref Matrix mat)
        {
            Matrix result;
            Invert(ref mat, out result);

            return result;
        }
        /// <summary>
        /// 逆矩阵
        /// </summary>
        /// <param name="mat">矩阵对象</param>
        /// <param name="result">逆矩阵</param>
        public static void Invert(ref Matrix mat, out Matrix result)
        {
            unsafe
            {
                fixed (Matrix* pResult = &result)
                {
                    fixed (Matrix* pMat = &mat)
                    {
                        IDllImportApi.v3dxMatrix4Inverse(pResult, pMat, (float*)0);
                    }
                }
            }
        }
        /// <summary>
        /// 转置矩阵
        /// </summary>
        /// <param name="mat">矩阵对象</param>
        /// <returns>返回转置后的矩阵</returns>
        public static Matrix Transpose(ref Matrix mat)
        {
            Matrix result;
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
        /// <summary>
        /// 重载"*"运算符
        /// </summary>
        /// <param name="left">矩阵对象</param>
        /// <param name="right">矩阵对象</param>
        /// <returns>返回计算后的矩阵对象</returns>
        public static Matrix operator *(Matrix left, Matrix right)
        {
            Matrix result;
            unsafe
            {
                EngineNS.IDllImportApi.v3dxMatrix4Mul_CSharp(&result, &left, &right);
                return result;
            }

            //Matrix1Check.Begin();
            //result.M11 = (left.M11 * right.M11) + (left.M12 * right.M21) + (left.M13 * right.M31) + (left.M14 * right.M41);
            //result.M12 = (left.M11 * right.M12) + (left.M12 * right.M22) + (left.M13 * right.M32) + (left.M14 * right.M42);
            //result.M13 = (left.M11 * right.M13) + (left.M12 * right.M23) + (left.M13 * right.M33) + (left.M14 * right.M43);
            //result.M14 = (left.M11 * right.M14) + (left.M12 * right.M24) + (left.M13 * right.M34) + (left.M14 * right.M44);
            //result.M21 = (left.M21 * right.M11) + (left.M22 * right.M21) + (left.M23 * right.M31) + (left.M24 * right.M41);
            //result.M22 = (left.M21 * right.M12) + (left.M22 * right.M22) + (left.M23 * right.M32) + (left.M24 * right.M42);
            //result.M23 = (left.M21 * right.M13) + (left.M22 * right.M23) + (left.M23 * right.M33) + (left.M24 * right.M43);
            //result.M24 = (left.M21 * right.M14) + (left.M22 * right.M24) + (left.M23 * right.M34) + (left.M24 * right.M44);
            //result.M31 = (left.M31 * right.M11) + (left.M32 * right.M21) + (left.M33 * right.M31) + (left.M34 * right.M41);
            //result.M32 = (left.M31 * right.M12) + (left.M32 * right.M22) + (left.M33 * right.M32) + (left.M34 * right.M42);
            //result.M33 = (left.M31 * right.M13) + (left.M32 * right.M23) + (left.M33 * right.M33) + (left.M34 * right.M43);
            //result.M34 = (left.M31 * right.M14) + (left.M32 * right.M24) + (left.M33 * right.M34) + (left.M34 * right.M44);
            //result.M41 = (left.M41 * right.M11) + (left.M42 * right.M21) + (left.M43 * right.M31) + (left.M44 * right.M41);
            //result.M42 = (left.M41 * right.M12) + (left.M42 * right.M22) + (left.M43 * right.M32) + (left.M44 * right.M42);
            //result.M43 = (left.M41 * right.M13) + (left.M42 * right.M23) + (left.M43 * right.M33) + (left.M44 * right.M43);
            //result.M44 = (left.M41 * right.M14) + (left.M42 * right.M24) + (left.M43 * right.M34) + (left.M44 * right.M44);
            //Matrix1Check.End();
            //return result;
        }
        /// <summary>
        /// 重载"*"号运算符
        /// </summary>
        /// <param name="left">矩阵对象</param>
        /// <param name="right">常数</param>
        /// <returns>返回计算后的矩阵对象</returns>
        public static Matrix operator *(Matrix left, float right)
        {
            Matrix result;
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
        /// <summary>
        /// 重载"*"号运算符
        /// </summary>
        /// <param name="right">常数</param>
        /// <param name="left">矩阵对象</param>
        /// <returns>返回计算后的矩阵对象</returns>
        public static Matrix operator *(float right, Matrix left)
        {
            return left * right;
        }
        /// <summary>
        /// 重载"/"号运算符
        /// </summary>
        /// <param name="left">矩阵对象</param>
        /// <param name="right">矩阵对象</param>
        /// <returns>返回计算后的矩阵对象</returns>
        public static Matrix operator /(Matrix left, Matrix right)
        {
            Matrix result;
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
        /// <summary>
        /// 重载"/"号运算符
        /// </summary>
        /// <param name="left">矩阵对象</param>
        /// <param name="right">常数</param>
        /// <returns>返回计算后的矩阵对象</returns>
        public static Matrix operator /(Matrix left, float right)
        {
            Matrix result;
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
        /// <summary>
        /// 重载"+"号运算符
        /// </summary>
        /// <param name="left">矩阵对象</param>
        /// <param name="right">矩阵对象</param>
        /// <returns>返回计算后的矩阵对象</returns>
        public static Matrix operator +(Matrix left, Matrix right)
        {
            Matrix result;
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
        /// <summary>
        /// 重载"-"号运算符
        /// </summary>
        /// <param name="left">矩阵对象</param>
        /// <param name="right">矩阵对象</param>
        /// <returns>返回计算后的矩阵对象</returns>
        public static Matrix operator -(Matrix left, Matrix right)
        {
            Matrix result;
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
        /// <summary>
        /// 重载"-"号运算符
        /// </summary>
        /// <param name="matrix">矩阵对象</param>
        /// <returns>返回计算后的矩阵对象</returns>
        public static Matrix operator -(Matrix matrix)
        {
            Matrix result;
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
        /// <summary>
        /// 重载"=="号运算符
        /// </summary>
        /// <param name="left">矩阵对象</param>
        /// <param name="right">矩阵对象</param>
        /// <returns>两个矩阵相等返回true，否则返回false</returns>
        public static bool operator ==(Matrix left, Matrix right)
        {
            return left.Equals(right);
            //return Equals(left, right);
        }
        /// <summary>
        /// 重载"!="号运算符
        /// </summary>
        /// <param name="left">矩阵对象</param>
        /// <param name="right">矩阵对象</param>
        /// <returns>两个矩阵不相等返回true，否则返回false</returns>
        public static bool operator !=(Matrix left, Matrix right)
        {
            return !left.Equals(right);
            //return !Matrix.Equals(left, right);
        }

        public static Matrix MakeMatrix(Vector3 InX, Vector3 InY, Vector3 InZ, Vector3 InW)
        {
            Matrix result;
            result.M11 = InX.X; result.M12 = InX.Y; result.M13 = InX.Z; result.M14 = 0.0f;
            result.M21 = InY.X; result.M22 = InY.Y; result.M23 = InY.Z; result.M24 = 0.0f;
            result.M31 = InZ.X; result.M32 = InZ.Y; result.M33 = InZ.Z; result.M34 = 0.0f;
            result.M41 = InW.X; result.M42 = InW.Y; result.M43 = InW.Z; result.M44 = 1.0f;
            return result;
        }
        const float KINDA_SMALL_NUMBER = 0.0001f;
        public static Matrix MakeFromZ(Vector3 NewZ)
        {
            NewZ.Normalize();

            // try to use up if possible
            var UpVector = (Math.Abs(NewZ.Y) < (1.0f - KINDA_SMALL_NUMBER)) ? Vector3.UnitY : Vector3.UnitX;

            var NewX = Vector3.Cross(UpVector, NewZ);
            NewX.Normalize();
            var NewY = Vector3.Cross(NewZ, NewX);

            return Matrix.MakeMatrix(NewX, NewY, NewZ, Vector3.Zero);
        }
        public static Matrix MakeFromY(Vector3 NewY)
        {
            NewY.Normalize();

            // try to use up if possible
            var UpVector = (Math.Abs(NewY.Y) < (1.0f - KINDA_SMALL_NUMBER)) ? Vector3.UnitY : Vector3.UnitX;

            var NewX = Vector3.Cross(NewY, UpVector);
            NewX.Normalize();
            var NewZ = Vector3.Cross(NewX, NewY);

            return Matrix.MakeMatrix(NewX, NewY, NewZ, Vector3.Zero);
        }
        public static Matrix MakeFromX(Vector3 NewX)
        {
            NewX.Normalize();

            // try to use up if possible
            var UpVector = (Math.Abs(NewX.Y) < (1.0f - KINDA_SMALL_NUMBER)) ? Vector3.UnitY : Vector3.UnitX;

            var NewZ = Vector3.Cross(NewX, UpVector);
            NewX.Normalize();
            var NewY = Vector3.Cross(NewZ, NewX);

            return Matrix.MakeMatrix(NewX, NewY, NewZ, Vector3.Zero);
        }
    }
}
