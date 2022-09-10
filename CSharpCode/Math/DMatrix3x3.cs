using System;
using System.Globalization;


namespace EngineNS
{
    [System.Serializable]
    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential, Pack = 4)]
    public struct DMatrix3x3 : System.IEquatable<DMatrix3x3>
    {
        #region Member
        public double M11;
        public double M12;
        public double M13;

        public double M21;
        public double M22;
        public double M23;

        public double M31;
        public double M32;
        public double M33;
        #endregion
        /// <summary>
        /// 只读属性，标准矩阵
        /// </summary>
        public static DMatrix3x3 Identity
        {
            get
            {
                return mIdentity;
            }
        }
        static DMatrix3x3 mIdentity = new DMatrix3x3();
        static DMatrix3x3()
        {
            mIdentity.M11 = 1.0f;
            mIdentity.M22 = 1.0f;
            mIdentity.M33 = 1.0f;
        }

        #region Equal Overrride
        /// <summary>
        /// 转换到string字符串
        /// </summary>
        /// <returns>转换后的String字符串</returns>
        public override string ToString()
        {
            return string.Format(System.Globalization.CultureInfo.CurrentCulture, "[[M11:{0} M12:{1} M13:{2}] [M21:{4} M22:{5} M23:{6}] [M31:{8} M32:{9} M33:{10}] ]",
                M11.ToString(System.Globalization.CultureInfo.CurrentCulture),
                M12.ToString(System.Globalization.CultureInfo.CurrentCulture),
                M13.ToString(System.Globalization.CultureInfo.CurrentCulture),
                M21.ToString(System.Globalization.CultureInfo.CurrentCulture),
                M22.ToString(System.Globalization.CultureInfo.CurrentCulture),
                M23.ToString(System.Globalization.CultureInfo.CurrentCulture),
                M31.ToString(System.Globalization.CultureInfo.CurrentCulture),
                M32.ToString(System.Globalization.CultureInfo.CurrentCulture),
                M33.ToString(System.Globalization.CultureInfo.CurrentCulture));
        }
        /// <summary>
        /// 获取对象的哈希值
        /// </summary>
        /// <returns>返回对象的哈希值</returns>
	    public override int GetHashCode()
        {
            return M11.GetHashCode() + M12.GetHashCode() + M13.GetHashCode() +
                   M21.GetHashCode() + M22.GetHashCode() + M23.GetHashCode() +
                   M31.GetHashCode() + M32.GetHashCode() + M33.GetHashCode();

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

            return Equals((DMatrix3x3)(value));
        }
        /// <summary>
        /// 判断两个对象是否相等
        /// </summary>
        /// <param name="value">矩阵对象</param>
        /// <returns>如果相等返回true，否则返回false</returns>
	    public bool Equals(DMatrix3x3 value)
        {
            return (M11 == value.M11 && M12 == value.M12 && M13 == value.M13 &&
                     M21 == value.M21 && M22 == value.M22 && M23 == value.M23 &&
                     M31 == value.M31 && M32 == value.M32 && M33 == value.M33);

        }
        /// <summary>
        /// 判断两个对象是否相等
        /// </summary>
        /// <param name="value1">矩阵对象</param>
        /// <param name="value2">矩阵对象</param>
        /// <returns>如果value1与value2相等返回true，否则返回false</returns>
	    public static bool Equals(ref DMatrix3x3 value1, ref DMatrix3x3 value2)
        {
            return (value1.M11 == value2.M11 && value1.M12 == value2.M12 && value1.M13 == value2.M13 &&
                     value1.M21 == value2.M21 && value1.M22 == value2.M22 && value1.M23 == value2.M23 &&
                     value1.M31 == value2.M31 && value1.M32 == value2.M32 && value1.M33 == value2.M33);
        }
        #endregion

        #region Rotation
        /// <summary>
        /// 沿X轴旋转
        /// </summary>
        /// <param name="angle">旋转角度</param>
        /// <returns>计算后的矩阵</returns>
        public static DMatrix3x3 RotationX(double angle)
        {
            DMatrix3x3 result;
            double cos = (double)(Math.Cos((double)(angle)));
            double sin = (double)(Math.Sin((double)(angle)));

            result.M11 = 1.0f;
            result.M12 = 0.0f;
            result.M13 = 0.0f;
            result.M21 = 0.0f;
            result.M22 = cos;
            result.M23 = sin;
            result.M31 = 0.0f;
            result.M32 = -sin;
            result.M33 = cos;

            //D3DXMatrixRotationX((D3DXMATRIX*)&result, angle);

            return result;
        }
        /// <summary>
        /// 沿X轴旋转
        /// </summary>
        /// <param name="angle">旋转角度</param>
        /// <param name="result">计算后的矩阵</param>
        public static void RotationX(double angle, out DMatrix3x3 result)
        {
            double cos = (double)(Math.Cos((double)(angle)));
            double sin = (double)(Math.Sin((double)(angle)));

            result.M11 = 1.0f;
            result.M12 = 0.0f;
            result.M13 = 0.0f;
            result.M21 = 0.0f;
            result.M22 = cos;
            result.M23 = sin;
            result.M31 = 0.0f;
            result.M32 = -sin;
            result.M33 = cos;
        }
        /// <summary>
        /// 沿Y轴旋转
        /// </summary>
        /// <param name="angle">旋转角度</param>
        /// <returns>计算后的矩阵</returns>
        public static DMatrix3x3 RotationY(double angle)
        {
            DMatrix3x3 result;
            double cos = (double)(Math.Cos((double)(angle)));
            double sin = (double)(Math.Sin((double)(angle)));

            result.M11 = cos;
            result.M12 = 0.0f;
            result.M13 = -sin;
            result.M21 = 0.0f;
            result.M22 = 1.0f;
            result.M23 = 0.0f;
            result.M31 = sin;
            result.M32 = 0.0f;
            result.M33 = cos;
            return result;
        }
        /// <summary>
        /// 沿Y轴旋转
        /// </summary>
        /// <param name="angle">旋转角度</param>
        /// <param name="result">计算后的矩阵</param>
        public static void RotationY(double angle, out DMatrix3x3 result)
        {
            double cos = (double)(Math.Cos((double)(angle)));
            double sin = (double)(Math.Sin((double)(angle)));

            result.M11 = cos;
            result.M12 = 0.0f;
            result.M13 = -sin;
            result.M21 = 0.0f;
            result.M22 = 1.0f;
            result.M23 = 0.0f;
            result.M31 = sin;
            result.M32 = 0.0f;
            result.M33 = cos;
        }
        /// <summary>
        /// 沿Z轴旋转
        /// </summary>
        /// <param name="angle">旋转角度</param>
        /// <returns>返回旋转后的矩阵</returns>
        public static DMatrix3x3 RotationZ(double angle)
        {
            DMatrix3x3 result;
            double cos = (double)(Math.Cos((double)(angle)));
            double sin = (double)(Math.Sin((double)(angle)));

            result.M11 = cos;
            result.M12 = sin;
            result.M13 = 0.0f;
            result.M21 = -sin;
            result.M22 = cos;
            result.M23 = 0.0f;
            result.M31 = 0.0f;
            result.M32 = 0.0f;
            result.M33 = 1.0f;
            return result;
        }
        /// <summary>
        /// 沿Z轴旋转
        /// </summary>
        /// <param name="angle">旋转角度</param>
        /// <param name="result">旋转后的矩阵</param>
        public static void RotationZ(double angle, out DMatrix3x3 result)
        {
            double cos = (double)(Math.Cos((double)(angle)));
            double sin = (double)(Math.Sin((double)(angle)));

            result.M11 = cos;
            result.M12 = sin;
            result.M13 = 0.0f;
            result.M21 = -sin;
            result.M22 = cos;
            result.M23 = 0.0f;
            result.M31 = 0.0f;
            result.M32 = 0.0f;
            result.M33 = 1.0f;
        }
        public static DMatrix3x3 RotationQuaternion(Quaternion quaternion)
        {
            DMatrix3x3 result;

            double xx = quaternion.X * quaternion.X;
            double yy = quaternion.Y * quaternion.Y;
            double zz = quaternion.Z * quaternion.Z;
            double xy = quaternion.X * quaternion.Y;
            double zw = quaternion.Z * quaternion.W;
            double zx = quaternion.Z * quaternion.X;
            double yw = quaternion.Y * quaternion.W;
            double yz = quaternion.Y * quaternion.Z;
            double xw = quaternion.X * quaternion.W;
            result.M11 = 1.0f - (2.0f * (yy + zz));
            result.M12 = 2.0f * (xy + zw);
            result.M13 = 2.0f * (zx - yw);
            result.M21 = 2.0f * (xy - zw);
            result.M22 = 1.0f - (2.0f * (zz + xx));
            result.M23 = 2.0f * (yz + xw);
            result.M31 = 2.0f * (zx + yw);
            result.M32 = 2.0f * (yz - xw);
            result.M33 = 1.0f - (2.0f * (yy + xx));
            return result;
        }
        /// <summary>
        /// 根据旋转四元数计算的矩阵
        /// </summary>
        /// <param name="rotation">旋转四元数</param>
        /// <param name="result">计算后的矩阵</param>
        public static void RotationQuaternion(ref Quaternion rotation, out DMatrix3x3 result)
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
            result.M21 = 2.0f * (xy - zw);
            result.M22 = 1.0f - (2.0f * (zz + xx));
            result.M23 = 2.0f * (yz + xw);
            result.M31 = 2.0f * (zx + yw);
            result.M32 = 2.0f * (yz - xw);
            result.M33 = 1.0f - (2.0f * (yy + xx));
        }
        /// <summary>
        /// 根据轴进行旋转
        /// </summary>
        /// <param name="axis">旋转轴向量</param>
        /// <param name="angle">旋转角度</param>
        /// <returns>返回旋转后的矩阵</returns>
        public static DMatrix3x3 RotationAxis(DVector3 axis, double angle)
        {
            if (axis.LengthSquared() != 1.0f)
                axis.Normalize();

            DMatrix3x3 result;
            double x = axis.X;
            double y = axis.Y;
            double z = axis.Z;
            double cos = (double)(Math.Cos((double)(angle)));
            double sin = (double)(Math.Sin((double)(angle)));
            double xx = x * x;
            double yy = y * y;
            double zz = z * z;
            double xy = x * y;
            double xz = x * z;
            double yz = y * z;

            result.M11 = xx + (cos * (1.0f - xx));
            result.M12 = (xy - (cos * xy)) + (sin * z);
            result.M13 = (xz - (cos * xz)) - (sin * y);
            result.M21 = (xy - (cos * xy)) - (sin * z);
            result.M22 = yy + (cos * (1.0f - yy));
            result.M23 = (yz - (cos * yz)) + (sin * x);
            result.M31 = (xz - (cos * xz)) + (sin * y);
            result.M32 = (yz - (cos * yz)) - (sin * x);
            result.M33 = zz + (cos * (1.0f - zz));
            return result;
        }
        /// <summary>
        /// 根据轴进行旋转
        /// </summary>
        /// <param name="axis">旋转轴向量</param>
        /// <param name="angle">旋转角度</param>
        /// <param name="result">旋转后的矩阵</param>
        public static void RotationAxis(ref DVector3 axis, double angle, out DMatrix3x3 result)
        {
            if (axis.LengthSquared() != 1.0f)
                axis.Normalize();

            double x = axis.X;
            double y = axis.Y;
            double z = axis.Z;
            double cos = (double)(Math.Cos((double)(angle)));
            double sin = (double)(Math.Sin((double)(angle)));
            double xx = x * x;
            double yy = y * y;
            double zz = z * z;
            double xy = x * y;
            double xz = x * z;
            double yz = y * z;

            result.M11 = xx + (cos * (1.0f - xx));
            result.M12 = (xy - (cos * xy)) + (sin * z);
            result.M13 = (xz - (cos * xz)) - (sin * y);
            result.M21 = (xy - (cos * xy)) - (sin * z);
            result.M22 = yy + (cos * (1.0f - yy));
            result.M23 = (yz - (cos * yz)) + (sin * x);
            result.M31 = (xz - (cos * xz)) + (sin * y);
            result.M32 = (yz - (cos * yz)) - (sin * x);
            result.M33 = zz + (cos * (1.0f - zz));
        }
        /// <summary>
        /// 根据欧拉角设置矩阵
        /// </summary>
        /// <param name="yaw">航向角</param>
        /// <param name="pitch">俯仰角</param>
        /// <param name="roll">翻滚角</param>
        /// <returns>返回计算后的矩阵</returns>
        public static DMatrix3x3 RotationYawPitchRoll(float yaw, float pitch, float roll)
        {
            DMatrix3x3 result;
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
        public static void RotationYawPitchRoll(float yaw, float pitch, float roll, out DMatrix3x3 result)
        {
            Quaternion quaternion;
            Quaternion.RotationYawPitchRoll(yaw, pitch, roll, out quaternion);
            RotationQuaternion(ref quaternion, out result);
        }
        #endregion

        #region overrid operator

        public static DVector3 operator *(DVector3 left, DMatrix3x3 rith)
        {
            DVector3 vector3;
            vector3.X = rith.M11 * left.X + rith.M21 * left.Y + rith.M31 * left.Z;
            vector3.Y = rith.M12 * left.X + rith.M22 * left.Y + rith.M32 * left.Z;
            vector3.Z = rith.M13 * left.X + rith.M23 * left.Y + rith.M33 * left.Z;
            return vector3;
        }
        #endregion
    }
}
