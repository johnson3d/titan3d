using System;

using System.Globalization;

namespace EngineNS
{
    /// <summary>
    /// 旋转四元数结构体
    /// </summary>
    [System.Serializable]
    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
    //[System.ComponentModel.TypeConverter( typeof(EngineNS.Design.QuaternionConverter) )]
    public struct Quaternion : System.IEquatable<Quaternion>
    {
        #region Member
        /// <summary>
        /// X值
        /// </summary>
        [Rtti.Meta]
        public float X;
        /// <summary>
        /// Y值
        /// </summary>
        [Rtti.Meta]
        public float Y;
        /// <summary>
        /// Z值
        /// </summary>
        [Rtti.Meta]
        public float Z;
        /// <summary>
        /// W值
        /// </summary>
        [Rtti.Meta]
        public float W;
        #endregion

        #region Equal override
        /// <summary>
        /// 转换到string类型
        /// </summary>
        /// <returns>返回转换到string类型的对象</returns>
        public override string ToString()
        {
            return $"{X},{Y},{Z},{W}";
            //return String.Format(CultureInfo.CurrentCulture, "X:{0} Y:{1} Z:{2} W:{3}", X.ToString(CultureInfo.CurrentCulture),
            //    Y.ToString(CultureInfo.CurrentCulture), Z.ToString(CultureInfo.CurrentCulture),
            //    W.ToString(CultureInfo.CurrentCulture));
        }
        /// <summary>
        /// 获取对象的哈希值
        /// </summary>
        /// <returns>返回该对象的哈希值</returns>
	    public override int GetHashCode()
        {
            return X.GetHashCode() + Y.GetHashCode() + Z.GetHashCode() + W.GetHashCode();
        }
        /// <summary>
        /// 判断两个对象是否相等
        /// </summary>
        /// <param name="value">可转换成旋转四元数的对象</param>
        /// <returns>如果两个对象相等返回true，否则返回false</returns>
	    public override bool Equals(object value)
        {
            if (value == null)
                return false;

            if (value.GetType() != GetType())
                return false;

            return Equals((Quaternion)(value));
        }
        /// <summary>
        /// 判断两个对象是否相等
        /// </summary>
        /// <param name="value">旋转四元数</param>
        /// <returns>如果两个对象相等返回true，否则返回false</returns>
	    public bool Equals(Quaternion value)
        {
            bool reX = (Math.Abs(X - value.X) < CoreDefine.Epsilon);
            bool reY = (Math.Abs(Y - value.Y) < CoreDefine.Epsilon);
            bool reZ = (Math.Abs(Z - value.Z) < CoreDefine.Epsilon);
            bool reW = (Math.Abs(W - value.W) < CoreDefine.Epsilon);
            return (reX && reY && reZ && reW);
        }
        /// <summary>
        /// 判断两个对象是否相等
        /// </summary>
        /// <param name="value1">旋转四元数</param>
        /// <param name="value2">旋转四元数</param>
        /// <returns>如果两个对象相等返回true，否则返回false</returns>
	    public static bool Equals(in Quaternion value1, in Quaternion value2)
        {
            bool reX = (Math.Abs(value1.X - value2.X) < CoreDefine.Epsilon);
            bool reY = (Math.Abs(value1.Y - value2.Y) < CoreDefine.Epsilon);
            bool reZ = (Math.Abs(value1.Z - value2.Z) < CoreDefine.Epsilon);
            bool reW = (Math.Abs(value1.W - value2.W) < CoreDefine.Epsilon);
            return (reX && reY && reZ && reW);
        }
        #endregion
        /// <summary>
        /// 带参构造函数
        /// </summary>
        /// <param name="x">X值</param>
        /// <param name="y">Y值</param>
        /// <param name="z">Z值</param>
        /// <param name="w">W值</param>
        public Quaternion(float x, float y, float z, float w)
        {
            X = x;
            Y = y;
            Z = z;
            W = w;
        }
        /// <summary>
        /// 带参构造函数
        /// </summary>
        /// <param name="value">三维向量</param>
        /// <param name="w">W值</param>
	    public Quaternion(Vector3 value, float w)
        {
            X = value.X;
            Y = value.Y;
            Z = value.Z;
            W = w;
        }
        /// <summary>
        /// 只读属性，标准四元数
        /// </summary>
        public readonly static Quaternion mIdentity = new Quaternion(0, 0, 0, 1);
        [Rtti.Meta]
        public static Quaternion Identity
        {
            get
            {
                return mIdentity;
            }
        }
        public readonly static Quaternion mZero = new Quaternion(0, 0, 0, 0);
        [Rtti.Meta]
        public static Quaternion Zero
        {
            get
            {
                return mZero;
            }
        }
        /// <summary>
        /// 只读属性，是否为标准四元数
        /// </summary>
        [Rtti.Meta]
        public bool IsIdentity
        {
            get
            {
                if (X != 0.0f || Y != 0.0f || Z != 0.0f)
                    return false;

                return (W == 1.0f);
            }
        }

        [Rtti.Meta]
        public bool IsValid
        {
            get
            {
                if (X == 0.0f && Y == 0.0f && Z == 0.0f && W == 0.0f)
                    return false;

                return true;
            }
        }

        /// <summary>
        /// 只读属性，旋转轴对象
        /// </summary>
        [Rtti.Meta]
        public Vector3 Axis
        {
            get
            {
                unsafe
                {
                    fixed (Quaternion* pinThis = &this)
                    {
                        float angle;
                        Vector3 axis;

                        IDllImportApi.v3dxQuaternionToAxisAngle(pinThis, (Vector3*)&axis, &angle);
                        return axis;
                    }
                }
            }
        }
        /// <summary>
        /// 只读属性，旋转角度
        /// </summary>
        [Rtti.Meta]
        public float Angle
        {
            get
            {
                unsafe
                {
                    fixed (Quaternion* pinThis = &this)
                    {
                        float angle;
                        Vector3 axis;

                        IDllImportApi.v3dxQuaternionToAxisAngle(pinThis, (Vector3*)&axis, &angle);
                        return angle;
                    }
                }
            }
        }
        /// <summary>
        /// 获取沿轴旋转的角度
        /// </summary>
        /// <param name="axis">旋转轴向量</param>
        /// <returns>返回沿轴旋转的角度</returns>
        [Rtti.Meta]
        public float GetAngleWithAxis(Vector3 axis)
        {
            unsafe
            {
                fixed (Quaternion* pinThis = &this)
                {
                    float angle;

                    IDllImportApi.v3dxQuaternionToAxisAngle(pinThis, (Vector3*)&axis, &angle);
                    return angle;
                }
            }
        }

        //#define CLAMP(x , min , max) ((x) > (max) ? (max) : ((x) < (min) ? (min) : x))
        /// <summary>
        /// 获取欧拉角
        /// </summary>
        /// <param name="Yaw">航向角</param>
        /// <param name="Pitch">俯仰角</param>
        /// <param name="Roll">翻转角</param>
        [Rtti.Meta]
        public void GetYawPitchRoll(out float Yaw, out float Pitch, out float Roll)
        {
            //double d0 = X * X + Y * Y - Z * Z - W * W;
            //double d1 = 2 * (Y * Z + X * W);
            //double d2 = X * X - Y * Y - Z * Z + W * W;
            //Yaw = (float)(Math.Atan(d1 / d0));
            //Pitch = (float)(Math.Asin(-2 * (Y * W - X * Z)));
            //Roll = (float)(Math.Atan( 2 * (Z * W + X * Y) / d2));

            //if(d2 < 0)
            //{
            //	if(Roll < 0)
            //		Roll += Math.PI;
            //	else
            //		Roll -= Math.PI;
            //}

            //if(d0 < 0)
            //{
            //	if(d1 > 0)
            //		Yaw += Math.PI;
            //	else
            //		Yaw -= Math.PI;
            //}

            Yaw = (float)(Math.Atan2(2 * (W * Y + Z * X), 1 - 2 * (X * X + Y * Y)));
            float value = 2 * (W * X - Y * Z);
            value = ((value) > (1.0f) ? (1.0f) : ((value) < (-1.0f) ? (-1.0f) : value));
            Pitch = (float)(Math.Asin(value));
            Roll = (float)(Math.Atan2(2 * (W * Z + X * Y), 1 - 2 * (Z * Z + X * X)));
        }
        [Rtti.Meta]
        public Vector3 ToEuler()
        {
            Vector3 result = Vector3.Zero;
            GetYawPitchRoll(out result.Y, out result.X, out result.Z);
            return result;
        }
        /// <summary>
        /// 长度
        /// </summary>
        /// <returns>返回四元数的长度</returns>
        [Rtti.Meta]
        public float Length()
        {
            return (float)(Math.Sqrt((X * X) + (Y * Y) + (Z * Z) + (W * W)));
        }
        /// <summary>
        /// 长度的平方
        /// </summary>
        /// <returns>返回四元数的长度的平方</returns>
        [Rtti.Meta]
        public float LengthSquared()
        {
            return (X * X) + (Y * Y) + (Z * Z) + (W * W);
        }
        /// <summary>
        /// 四元数的单位向量
        /// </summary>
        [Rtti.Meta]
        public void Normalize()
        {
            float length = 1.0f / Length();
            X *= length;
            Y *= length;
            Z *= length;
            W *= length;
        }
        /// <summary>
        /// 共轭四元数
        /// </summary>
        [Rtti.Meta]
        public void Conjugate()
        {
            X = -X;
            Y = -Y;
            Z = -Z;
        }
        /// <summary>
        /// 四元数的逆矩阵
        /// </summary>
        [Rtti.Meta]
        public void Invert()
        {
            float lengthSq = 1.0f / ((X * X) + (Y * Y) + (Z * Z) + (W * W));
            X = -X * lengthSq;
            Y = -Y * lengthSq;
            Z = -Z * lengthSq;
            W = W * lengthSq;
        }
        [Rtti.Meta]
        public Quaternion Inverse()
        {
            float fNorm = W * W + X * X + Y * Y + Z * Z;
            if (fNorm > 0.0f)
            {
                float fInvNorm = 1.0f / fNorm;
                return new Quaternion(-X * fInvNorm, -Y * fInvNorm, -Z * fInvNorm, W * fInvNorm);
            }
            else
            {
                // return an invalid result to flag the error
                System.Diagnostics.Debug.Assert(false);
                return Quaternion.Identity;
            }
        }
        /// <summary>
        /// 四元数相加
        /// </summary>
        /// <param name="left">四元数对象</param>
        /// <param name="right">四元数对象</param>
        /// <returns>返回计算后的四元数</returns>
        [Rtti.Meta]
        public static Quaternion Add(Quaternion left, Quaternion right)
        {
            Quaternion result;
            result.X = left.X + right.X;
            result.Y = left.Y + right.Y;
            result.Z = left.Z + right.Z;
            result.W = left.W + right.W;
            return result;
        }
        /// <summary>
        /// 四元数相加
        /// </summary>
        /// <param name="left">四元数对象</param>
        /// <param name="right">四元数对象</param>
        /// <param name="result">计算后的四元数</param>
        [Rtti.Meta]
        public static void Add(ref Quaternion left, ref Quaternion right, out Quaternion result)
        {
            Quaternion r;
            r.X = left.X + right.X;
            r.Y = left.Y + right.Y;
            r.Z = left.Z + right.Z;
            r.W = left.W + right.W;

            result = r;
        }
        /// <summary>
        /// 计算重心
        /// </summary>
        /// <param name="q1">四元数对象</param>
        /// <param name="q2">四元数对象</param>
        /// <param name="q3">四元数对象</param>
        /// <param name="f">力大小</param>
        /// <param name="g">重力值</param>
        /// <returns>返回计算后的四元数</returns>
        [Rtti.Meta]
        public static Quaternion Barycentric(Quaternion q1, Quaternion q2, Quaternion q3, float f, float g)
        {
            Quaternion result;

            unsafe
            {
                IDllImportApi.v3dxQuaternionBaryCentric((Quaternion*)&result, (Quaternion*)&q1,
                (Quaternion*)&q2, (Quaternion*)&q3, f, g);
            }

            return result;
        }
        /// <summary>
        /// 计算重心
        /// </summary>
        /// <param name="q1">四元数对象</param>
        /// <param name="q2">四元数对象</param>
        /// <param name="q3">四元数对象</param>
        /// <param name="f">力大小</param>
        /// <param name="g">重力值</param>
        /// <param name="result">计算后的四元数</param>
        [Rtti.Meta]
        public static void Barycentric(ref Quaternion q1, ref Quaternion q2, ref Quaternion q3, float f, float g, out Quaternion result)
        {
            unsafe
            {
                fixed (Quaternion* pinResult = &result)
                {
                    fixed (Quaternion* pin1 = &q1)
                    {
                        fixed (Quaternion* pin2 = &q2)
                        {
                            fixed (Quaternion* pin3 = &q3)
                            {
                                IDllImportApi.v3dxQuaternionBaryCentric(pinResult, pin1, pin2, pin3, f, g);
                            }
                        }
                    }
                }
            }
        }
        /// <summary>
        /// 计算共轭四元数
        /// </summary>
        /// <param name="quat">四元数对象</param>
        /// <returns>返回计算后的四元数</returns>
        [Rtti.Meta]
        public static Quaternion Conjugate(Quaternion quat)
        {
            Quaternion result;
            result.X = -quat.X;
            result.Y = -quat.Y;
            result.Z = -quat.Z;
            result.W = quat.W;
            return result;
        }
        /// <summary>
        /// 计算共轭四元数
        /// </summary>
        /// <param name="quat">四元数对象</param>
        /// <param name="result">计算后的四元数</param>
        [Rtti.Meta]
        public static void Conjugate(ref Quaternion quat, out Quaternion result)
        {
            result.X = -quat.X;
            result.Y = -quat.Y;
            result.Z = -quat.Z;
            result.W = quat.W;
        }
        /// <summary>
        /// 两个四元数的商
        /// </summary>
        /// <param name="left">四元数对象</param>
        /// <param name="right">四元数对象</param>
        /// <returns>返回计算后的四元数</returns>
        [Rtti.Meta]
        public static Quaternion Divide(Quaternion left, Quaternion right)
        {
            Quaternion result;
            result.X = left.X / right.X;
            result.Y = left.Y / right.Y;
            result.Z = left.Z / right.Z;
            result.W = left.W / right.W;
            return result;
        }
        /// <summary>
        /// 两个四元数的商
        /// </summary>
        /// <param name="left">四元数对象</param>
        /// <param name="right">四元数对象</param>
        /// <param name="result">计算后的四元数</param>
        [Rtti.Meta]
        public static void Divide(ref Quaternion left, ref Quaternion right, out Quaternion result)
        {
            result.X = left.X / right.X;
            result.Y = left.Y / right.Y;
            result.Z = left.Z / right.Z;
            result.W = left.W / right.W;
        }
        /// <summary>
        /// 两个四元数的点积
        /// </summary>
        /// <param name="left">四元数对象</param>
        /// <param name="right">四元数对象</param>
        /// <returns>返回计算后的四元数</returns>
        [Rtti.Meta]
        public static float Dot(Quaternion left, Quaternion right)
        {
            return (left.X * right.X) + (left.Y * right.Y) + (left.Z * right.Z) + (left.W * right.W);
        }
        [Rtti.Meta]
        public static float AngleBetween(Quaternion a, Quaternion b)
        {
            float dot = Dot(a, b);
            return (float)MathHelper.Acos(Math.Min(Math.Abs(dot), 1.0F)) * 2.0F * MathHelper.Rad2Deg;
        }
        /// <summary>
        /// 四元数的指数计算
        /// </summary>
        /// <param name="quat">四元数对象</param>
        /// <returns>返回计算后的四元数</returns>
        [Rtti.Meta]
        public static Quaternion Exponential(Quaternion quat)
        {
            Quaternion result;
            unsafe
            {
                IDllImportApi.v3dxQuaternionExp((Quaternion*)&result, (Quaternion*)&quat);
            }
            return result;
        }
        /// <summary>
        /// 四元数的指数计算
        /// </summary>
        /// <param name="quat">四元数对象</param>
        /// <param name="result">计算后的四元数</param>
        [Rtti.Meta]
        public static void Exponential(ref Quaternion quat, out Quaternion result)
        {
            unsafe
            {
                fixed (Quaternion* pinQuat = &quat)
                {
                    fixed (Quaternion* pinResult = &result)
                    {
                        IDllImportApi.v3dxQuaternionExp(pinResult, pinQuat);
                    }
                }
            }
        }
        /// <summary>
        /// 求四元数的逆矩阵
        /// </summary>
        /// <param name="quaternion">四元数对象</param>
        /// <returns>返回计算后的四元数</returns>
        [Rtti.Meta]
        public static Quaternion Invert(Quaternion quaternion)
        {
            Quaternion result;
            float lengthSq = 1.0f / ((quaternion.X * quaternion.X) + (quaternion.Y * quaternion.Y) + (quaternion.Z * quaternion.Z) + (quaternion.W * quaternion.W));

            result.X = -quaternion.X * lengthSq;
            result.Y = -quaternion.Y * lengthSq;
            result.Z = -quaternion.Z * lengthSq;
            result.W = quaternion.W * lengthSq;

            return result;
        }
        /// <summary>
        /// 求四元数的逆矩阵
        /// </summary>
        /// <param name="quaternion">四元数对象</param>
        /// <param name="result">计算后的四元数</param>
        [Rtti.Meta]
        public static void Invert(ref Quaternion quaternion, out Quaternion result)
        {
            float lengthSq = 1.0f / ((quaternion.X * quaternion.X) + (quaternion.Y * quaternion.Y) + (quaternion.Z * quaternion.Z) + (quaternion.W * quaternion.W));

            result.X = -quaternion.X * lengthSq;
            result.Y = -quaternion.Y * lengthSq;
            result.Z = -quaternion.Z * lengthSq;
            result.W = quaternion.W * lengthSq;
        }
        /// <summary>
        /// 四元数的插值计算
        /// </summary>
        /// <param name="left">四元数对象</param>
        /// <param name="right">四元数对象</param>
        /// <param name="amount">插值</param>
        /// <returns>返回计算后的四元数</returns>
        [Rtti.Meta]
        public static Quaternion Lerp(Quaternion left, Quaternion right, float amount)
        {
            Quaternion result;
            float inverse = 1.0f - amount;
            float dot = (left.X * right.X) + (left.Y * right.Y) + (left.Z * right.Z) + (left.W * right.W);

            if (dot >= 0.0f)
            {
                result.X = (inverse * left.X) + (amount * right.X);
                result.Y = (inverse * left.Y) + (amount * right.Y);
                result.Z = (inverse * left.Z) + (amount * right.Z);
                result.W = (inverse * left.W) + (amount * right.W);
            }
            else
            {
                result.X = (inverse * left.X) - (amount * right.X);
                result.Y = (inverse * left.Y) - (amount * right.Y);
                result.Z = (inverse * left.Z) - (amount * right.Z);
                result.W = (inverse * left.W) - (amount * right.W);
            }

            float invLength = 1.0f / result.Length();

            result.X *= invLength;
            result.Y *= invLength;
            result.Z *= invLength;
            result.W *= invLength;

            return result;
        }
        /// <summary>
        /// 四元数的插值计算
        /// </summary>
        /// <param name="left">四元数对象</param>
        /// <param name="right">四元数对象</param>
        /// <param name="amount">插值</param>
        /// <param name="result">计算后的四元数</param>
        [Rtti.Meta]
        public static void Lerp(ref Quaternion left, ref Quaternion right, float amount, out Quaternion result)
        {
            float inverse = 1.0f - amount;
            float dot = (left.X * right.X) + (left.Y * right.Y) + (left.Z * right.Z) + (left.W * right.W);

            if (dot >= 0.0f)
            {
                result.X = (inverse * left.X) + (amount * right.X);
                result.Y = (inverse * left.Y) + (amount * right.Y);
                result.Z = (inverse * left.Z) + (amount * right.Z);
                result.W = (inverse * left.W) + (amount * right.W);
            }
            else
            {
                result.X = (inverse * left.X) - (amount * right.X);
                result.Y = (inverse * left.Y) - (amount * right.Y);
                result.Z = (inverse * left.Z) - (amount * right.Z);
                result.W = (inverse * left.W) - (amount * right.W);
            }

            float invLength = 1.0f / result.Length();

            result.X *= invLength;
            result.Y *= invLength;
            result.Z *= invLength;
            result.W *= invLength;
        }
        /// <summary>
        /// 四元数的对数计算
        /// </summary>
        /// <param name="quat">四元数对象</param>
        /// <returns>返回计算后的四元数</returns>
        [Rtti.Meta]
        public static Quaternion Logarithm(Quaternion quat)
        {
            Quaternion result;
            unsafe
            {
                IDllImportApi.v3dxQuaternionLn((Quaternion*)&result, (Quaternion*)&quat);
            }
            return result;
        }
        /// <summary>
        /// 四元数的对数计算
        /// </summary>
        /// <param name="quat">四元数对象</param>
        /// <param name="result">计算后的四元数</param>
        [Rtti.Meta]
        public static void Logarithm(ref Quaternion quat, out Quaternion result)
        {
            unsafe
            {
                fixed (Quaternion* pinQuat = &quat)
                {
                    fixed (Quaternion* pinResult = &result)
                    {
                        IDllImportApi.v3dxQuaternionLn((Quaternion*)pinResult, (Quaternion*)pinQuat);
                    }
                }
            }
        }
        /// <summary>
        /// 四元数的乘法
        /// </summary>
        /// <param name="left">四元数对象</param>
        /// <param name="right">四元数对象</param>
        /// <returns>返回计算后的四元数</returns>
        [Rtti.Meta]
        public static Quaternion Multiply(Quaternion left, Quaternion right)
        {
            Quaternion quaternion;
            float lx = left.X;
            float ly = left.Y;
            float lz = left.Z;
            float lw = left.W;
            float rx = right.X;
            float ry = right.Y;
            float rz = right.Z;
            float rw = right.W;

            quaternion.X = (rx * lw + lx * rw + ry * lz) - (rz * ly);
            quaternion.Y = (ry * lw + ly * rw + rz * lx) - (rx * lz);
            quaternion.Z = (rz * lw + lz * rw + rx * ly) - (ry * lx);
            quaternion.W = (rw * lw) - (rx * lx + ry * ly + rz * lz);

            return quaternion;
        }
        /// <summary>
        /// 四元数的乘法
        /// </summary>
        /// <param name="left">四元数对象</param>
        /// <param name="right">四元数对象</param>
        /// <param name="result">计算后的四元数</param>
        //[Rtti.Meta]
        public static void Multiply(ref Quaternion left, ref Quaternion right, out Quaternion result)
        {
            float lx = left.X;
            float ly = left.Y;
            float lz = left.Z;
            float lw = left.W;
            float rx = right.X;
            float ry = right.Y;
            float rz = right.Z;
            float rw = right.W;

            result.X = (rx * lw + lx * rw + ry * lz) - (rz * ly);
            result.Y = (ry * lw + ly * rw + rz * lx) - (rx * lz);
            result.Z = (rz * lw + lz * rw + rx * ly) - (ry * lx);
            result.W = (rw * lw) - (rx * lx + ry * ly + rz * lz);
        }
        /// <summary>
        /// 四元数的乘法
        /// </summary>
        /// <param name="quaternion">四元数对象</param>
        /// <param name="scale">缩放大小</param>
        /// <returns>返回计算后的四元数</returns>
        [Rtti.Meta]
        public static Vector3 RotateVector3(Quaternion quaternion, Vector3 vec)
        {
            return quaternion * vec;
        }
        /// <summary>
        /// 四元数的乘法
        /// </summary>
        /// <param name="quaternion">四元数对象</param>
        /// <param name="scale">缩放大小</param>
        /// <returns>返回计算后的四元数</returns>
        [Rtti.Meta]
        public static Quaternion MultiplyFloat(Quaternion quaternion, float scale)
        {
            Quaternion result;
            result.X = quaternion.X * scale;
            result.Y = quaternion.Y * scale;
            result.Z = quaternion.Z * scale;
            result.W = quaternion.W * scale;
            return result;
        }
        /// <summary>
        /// 四元数的乘法
        /// </summary>
        /// <param name="quaternion">四元数对象</param>
        /// <param name="scale">缩放大小</param>
        /// <param name="result">计算后的四元数</param>
        //[Rtti.Meta]
        public static void Multiply(ref Quaternion quaternion, float scale, out Quaternion result)
        {
            result.X = quaternion.X * scale;
            result.Y = quaternion.Y * scale;
            result.Z = quaternion.Z * scale;
            result.W = quaternion.W * scale;
        }
        /// <summary>
        /// 四元数的取反运算
        /// </summary>
        /// <param name="quat">四元数对象</param>
        /// <returns>返回计算后的四元数</returns>
        [Rtti.Meta]
        public static Quaternion Negate(Quaternion quat)
        {
            Quaternion result;
            result.X = -quat.X;
            result.Y = -quat.Y;
            result.Z = -quat.Z;
            result.W = -quat.W;
            return result;
        }
        /// <summary>
        /// 四元数的取反运算
        /// </summary>
        /// <param name="quat">四元数对象</param>
        /// <param name="result">计算后的四元数</param>
        [Rtti.Meta]
        public static void Negate(ref Quaternion quat, out Quaternion result)
        {
            result.X = -quat.X;
            result.Y = -quat.Y;
            result.Z = -quat.Z;
            result.W = -quat.W;
        }
        /// <summary>
        /// 四元数的单位化
        /// </summary>
        /// <param name="quat">四元数对象</param>
        /// <returns>返回计算后的四元数</returns>
        [Rtti.Meta]
        public static Quaternion Normalize(Quaternion quat)
        {
            quat.Normalize();
            return quat;
        }
        /// <summary>
        /// 四元数的单位化
        /// </summary>
        /// <param name="quat">四元数对象</param>
        /// <param name="result">计算后的四元数</param>
        [Rtti.Meta]
        public static void Normalize(ref Quaternion quat, out Quaternion result)
        {
            float length = 1.0f / quat.Length();
            result.X = quat.X * length;
            result.Y = quat.Y * length;
            result.Z = quat.Z * length;
            result.W = quat.W * length;
        }
        /// <summary>
        /// 计算按轴旋转后的四元数
        /// </summary>
        /// <param name="axis">旋转轴</param>
        /// <param name="angle">旋转角度</param>
        /// <returns>返回计算后的四元数</returns>
        [Rtti.Meta]
        public static Quaternion RotationAxis(Vector3 axis, float angle)
        {
            Quaternion result;

            Vector3.Normalize(ref axis, out axis);

            float half = angle * 0.5f;
            float sin = (float)(Math.Sin((double)(half)));
            float cos = (float)(Math.Cos((double)(half)));

            result.X = axis.X * sin;
            result.Y = axis.Y * sin;
            result.Z = axis.Z * sin;
            result.W = cos;

            return result;
        }
        /// <summary>
        /// 计算按轴旋转后的四元数
        /// </summary>
        /// <param name="axis">旋转轴</param>
        /// <param name="angle">旋转角度</param>
        /// <param name="result">计算后的四元数</param>
        [Rtti.Meta]
        public static void RotationAxis(ref Vector3 axis, float angle, out Quaternion result)
        {
            Vector3.Normalize(ref axis, out axis);

            float half = angle * 0.5f;
            float sin = (float)(Math.Sin((double)(half)));
            float cos = (float)(Math.Cos((double)(half)));

            result.X = axis.X * sin;
            result.Y = axis.Y * sin;
            result.Z = axis.Z * sin;
            result.W = cos;
        }
        /// <summary>
        /// 计算按旋转矩阵旋转后的四元数
        /// </summary>
        /// <param name="matrix">旋转矩阵</param>
        /// <returns>返回计算后的四元数</returns>
        [Rtti.Meta]
        public static Quaternion RotationMatrix(Matrix matrix)
        {
            Quaternion result;
            //float scale = matrix.M11 + matrix.M22 + matrix.M33;

            //if( scale > 0.0f )
            //{
            //	float sqrt = (float)( Math.Sqrt( (double)(scale + 1.0f) ) );

            //	result.W = sqrt * 0.5f;
            //	sqrt = 0.5f / sqrt;

            //	result.X = (matrix.M23 - matrix.M32) * sqrt;
            //	result.Y = (matrix.M31 - matrix.M13) * sqrt;
            //	result.Z = (matrix.M12 - matrix.M21) * sqrt;

            //	return result;
            //}

            //if( (matrix.M11 >= matrix.M22) && (matrix.M11 >= matrix.M33) )
            //{
            //	float sqrt = (float)( Math.Sqrt( (double)(1.0f + matrix.M11 - matrix.M22 - matrix.M33) ) );
            //	float half = 0.5f / sqrt;

            //	result.X = 0.5f * sqrt;
            //	result.Y = (matrix.M12 + matrix.M21) * half;
            //	result.Z = (matrix.M13 + matrix.M31) * half;
            //	result.W = (matrix.M23 - matrix.M32) * half;

            //	return result;
            //}

            //if( matrix.M22 > matrix.M33 )
            //{
            //	float sqrt = (float)( Math.Sqrt( (double)(1.0f + matrix.M22 - matrix.M11 - matrix.M33) ) );
            //	float half = 0.5f / sqrt;

            //	result.X = (matrix.M21 + matrix.M12) * half;
            //	result.Y = 0.5f * sqrt;
            //	result.Z = (matrix.M32 + matrix.M23) * half;
            //	result.W = (matrix.M31 - matrix.M13) * half;

            //	return result;
            //}

            //float sqrt = (float)( Math.Sqrt( (double)(1.0f + matrix.M33 - matrix.M11 - matrix.M22) ) );
            //float half = 0.5f / sqrt;

            //result.X = (matrix.M31 + matrix.M13) * half;
            //result.Y = (matrix.M32 + matrix.M23) * half;
            //result.Z = 0.5f * sqrt;
            //result.W = (matrix.M12 - matrix.M21) * half;

            //Quaternion result2;
            var tempMatrix = matrix;
            tempMatrix.NoScale();
            unsafe
            {
                IDllImportApi.v3dxQuaternionRotationMatrix((Quaternion*)&result, (Matrix*)&tempMatrix);
            }

            return result;
        }
        /// <summary>
        /// 计算按旋转矩阵旋转后的四元数
        /// </summary>
        /// <param name="matrix">旋转矩阵</param>
        /// <param name="result">计算后的四元数</param>
        [Rtti.Meta]
        public static void RotationMatrix(ref Matrix matrix, out Quaternion result)
        {
            //float scale = matrix.M11 + matrix.M22 + matrix.M33;

            //if( scale > 0.0f )
            //{
            //	float sqrt = (float)( Math.Sqrt( (double)(scale + 1.0f) ) );

            //	result.W = sqrt * 0.5f;
            //	sqrt = 0.5f / sqrt;

            //	result.X = (matrix.M23 - matrix.M32) * sqrt;
            //	result.Y = (matrix.M31 - matrix.M13) * sqrt;
            //	result.Z = (matrix.M12 - matrix.M21) * sqrt;
            //	return;
            //}

            //if( (matrix.M11 >= matrix.M22) && (matrix.M11 >= matrix.M33) )
            //{
            //	float sqrt = (float)( Math.Sqrt( (double)(1.0f + matrix.M11 - matrix.M22 - matrix.M33) ) );
            //	float half = 0.5f / sqrt;

            //	result.X = 0.5f * sqrt;
            //	result.Y = (matrix.M12 + matrix.M21) * half;
            //	result.Z = (matrix.M13 + matrix.M31) * half;
            //	result.W = (matrix.M23 - matrix.M32) * half;
            //	return;
            //}

            //if( matrix.M22 > matrix.M33 )
            //{
            //	float sqrt = (float)( Math.Sqrt( (double)(1.0f + matrix.M22 - matrix.M11 - matrix.M33) ) );
            //	float half = 0.5f / sqrt;

            //	result.X = (matrix.M21 + matrix.M12) * half;
            //	result.Y = 0.5f * sqrt;
            //	result.Z = (matrix.M32 + matrix.M23) * half;
            //	result.W = (matrix.M31 - matrix.M13) * half;
            //	return;
            //}

            //float sqrt = (float)( Math.Sqrt( (double)(1.0f + matrix.M33 - matrix.M11 - matrix.M22) ) );
            //float half = 0.5f / sqrt;

            //result.X = (matrix.M31 + matrix.M13) * half;
            //result.Y = (matrix.M32 + matrix.M23) * half;
            //result.Z = 0.5f * sqrt;
            //result.W = (matrix.M12 - matrix.M21) * half;
            var tempMatrix = matrix;
            tempMatrix.NoScale();
            unsafe
            {
                fixed (Quaternion* pin_result = &result)
                {
                    IDllImportApi.v3dxQuaternionRotationMatrix(pin_result, &tempMatrix);
                }
            }
        }
        /// <summary>
        /// 根据旋转欧拉角计算旋转四元数
        /// </summary>
        /// <param name="yaw">俯仰角</param>
        /// <param name="pitch">航向角</param>
        /// <param name="roll">翻转角</param>
        /// <returns>返回计算后的旋转四元数</returns>
        [Rtti.Meta]
        public static Quaternion RotationYawPitchRoll(float yaw, float pitch, float roll)
        {
            Quaternion result;

            float halfRoll = roll * 0.5f;
            float sinRoll = (float)(Math.Sin((double)(halfRoll)));
            float cosRoll = (float)(Math.Cos((double)(halfRoll)));
            float halfPitch = pitch * 0.5f;
            float sinPitch = (float)(Math.Sin((double)(halfPitch)));
            float cosPitch = (float)(Math.Cos((double)(halfPitch)));
            float halfYaw = yaw * 0.5f;
            float sinYaw = (float)(Math.Sin((double)(halfYaw)));
            float cosYaw = (float)(Math.Cos((double)(halfYaw)));

            result.X = (cosYaw * sinPitch * cosRoll) + (sinYaw * cosPitch * sinRoll);
            result.Y = (sinYaw * cosPitch * cosRoll) - (cosYaw * sinPitch * sinRoll);
            result.Z = (cosYaw * cosPitch * sinRoll) - (sinYaw * sinPitch * cosRoll);
            result.W = (cosYaw * cosPitch * cosRoll) + (sinYaw * sinPitch * sinRoll);

            return result;
        }
        [Rtti.Meta]
        public static Quaternion FromEuler(Vector3 euler)
        {
            var yaw = euler.Y;
            var pitch = euler.X;
            var roll = euler.Z;
            Quaternion result;

            float halfRoll = roll * 0.5f;
            float sinRoll = (float)(Math.Sin((double)(halfRoll)));
            float cosRoll = (float)(Math.Cos((double)(halfRoll)));
            float halfPitch = pitch * 0.5f;
            float sinPitch = (float)(Math.Sin((double)(halfPitch)));
            float cosPitch = (float)(Math.Cos((double)(halfPitch)));
            float halfYaw = yaw * 0.5f;
            float sinYaw = (float)(Math.Sin((double)(halfYaw)));
            float cosYaw = (float)(Math.Cos((double)(halfYaw)));

            result.X = (cosYaw * sinPitch * cosRoll) + (sinYaw * cosPitch * sinRoll);
            result.Y = (sinYaw * cosPitch * cosRoll) - (cosYaw * sinPitch * sinRoll);
            result.Z = (cosYaw * cosPitch * sinRoll) - (sinYaw * sinPitch * cosRoll);
            result.W = (cosYaw * cosPitch * cosRoll) + (sinYaw * sinPitch * sinRoll);

            return result;
        }
        /// <summary>
        /// 根据旋转欧拉角计算旋转四元数
        /// </summary>
        /// <param name="yaw">俯仰角</param>
        /// <param name="pitch">航向角</param>
        /// <param name="roll">翻转角</param>
        /// <param name="result">计算后的旋转四元数</param>
        [Rtti.Meta]
        public static void RotationYawPitchRoll(float yaw, float pitch, float roll, out Quaternion result)
        {
            float halfRoll = roll * 0.5f;
            float sinRoll = (float)(Math.Sin((double)(halfRoll)));
            float cosRoll = (float)(Math.Cos((double)(halfRoll)));
            float halfPitch = pitch * 0.5f;
            float sinPitch = (float)(Math.Sin((double)(halfPitch)));
            float cosPitch = (float)(Math.Cos((double)(halfPitch)));
            float halfYaw = yaw * 0.5f;
            float sinYaw = (float)(Math.Sin((double)(halfYaw)));
            float cosYaw = (float)(Math.Cos((double)(halfYaw)));

            result.X = (cosYaw * sinPitch * cosRoll) + (sinYaw * cosPitch * sinRoll);
            result.Y = (sinYaw * cosPitch * cosRoll) - (cosYaw * sinPitch * sinRoll);
            result.Z = (cosYaw * cosPitch * sinRoll) - (sinYaw * sinPitch * cosRoll);
            result.W = (cosYaw * cosPitch * cosRoll) + (sinYaw * sinPitch * sinRoll);
        }
        /// <summary>
        /// 球形插值
        /// </summary>
        /// <param name="q1">旋转四元数</param>
        /// <param name="q2">旋转四元数</param>
        /// <param name="t">插值</param>
        /// <returns>返回计算后的旋转四元数</returns>
        [Rtti.Meta]
        public static Quaternion Slerp(Quaternion q1, Quaternion q2, float t)
        {
            Quaternion result;

            float opposite;
            float inverse;
            float dot = (q1.X * q2.X) + (q1.Y * q2.Y) + (q1.Z * q2.Z) + (q1.W * q2.W);
            bool flag = false;

            if (dot < 0.0f)
            {
                flag = true;
                dot = -dot;
            }

            if (dot > 0.999999f)
            {
                inverse = 1.0f - t;
                opposite = flag ? -t : t;
            }
            else
            {
                float acos = (float)(MathHelper.Acos(dot));
                float invSin = (float)((1.0f / Math.Sin((double)(acos))));

                inverse = ((float)(Math.Sin((double)((1.0f - t) * acos)))) * invSin;
                opposite = flag ? (((float)(-Math.Sin((double)(t * acos)))) * invSin) : (((float)(Math.Sin((double)(t * acos)))) * invSin);
            }

            result.X = (inverse * q1.X) + (opposite * q2.X);
            result.Y = (inverse * q1.Y) + (opposite * q2.Y);
            result.Z = (inverse * q1.Z) + (opposite * q2.Z);
            result.W = (inverse * q1.W) + (opposite * q2.W);

            return result;
        }
        /// <summary>
        /// 球形插值
        /// </summary>
        /// <param name="q1">旋转四元数</param>
        /// <param name="q2">旋转四元数</param>
        /// <param name="t">插值</param>
        /// <param name="result">计算后的旋转四元数</param>
        [Rtti.Meta]
        public static void Slerp(ref Quaternion q1, ref Quaternion q2, float t, out Quaternion result)
        {
            float opposite;
            float inverse;
            float dot = (q1.X * q2.X) + (q1.Y * q2.Y) + (q1.Z * q2.Z) + (q1.W * q2.W);
            bool flag = false;

            if (dot < 0.0f)
            {
                flag = true;
                dot = -dot;
            }

            if (dot > 0.999999f)
            {
                inverse = 1.0f - t;
                opposite = flag ? -t : t;
            }
            else
            {
                float acos = (float)(MathHelper.Acos(dot));
                float invSin = (float)((1.0f / Math.Sin((double)(acos))));

                inverse = ((float)(Math.Sin((double)((1.0f - t) * acos)))) * invSin;
                opposite = flag ? (((float)(-Math.Sin((double)(t * acos)))) * invSin) : (((float)(Math.Sin((double)(t * acos)))) * invSin);
            }

            result.X = (inverse * q1.X) + (opposite * q2.X);
            result.Y = (inverse * q1.Y) + (opposite * q2.Y);
            result.Z = (inverse * q1.Z) + (opposite * q2.Z);
            result.W = (inverse * q1.W) + (opposite * q2.W);
        }
        /// <summary>
        /// 四元数编组
        /// </summary>
        /// <param name="q1">旋转四元数</param>
        /// <param name="a">旋转四元数</param>
        /// <param name="b">旋转四元数</param>
        /// <param name="c">旋转四元数</param>
        /// <param name="t">组名</param>
        /// <returns>返回计算后的旋转四元数</returns>
        [Rtti.Meta]
        public static Quaternion Squad(Quaternion q1, Quaternion a, Quaternion b, Quaternion c, float t)
        {
            Quaternion result;

            unsafe
            {
                IDllImportApi.v3dxQuaternionSquad((Quaternion*)&result, (Quaternion*)&q1, (Quaternion*)&a,
                    (Quaternion*)&b, (Quaternion*)&c, t);
            }

            return result;
        }
        /// <summary>
        /// 四元数编组
        /// </summary>
        /// <param name="q1">旋转四元数</param>
        /// <param name="a">旋转四元数</param>
        /// <param name="b">旋转四元数</param>
        /// <param name="c">旋转四元数</param>
        /// <param name="t">组名</param>
        /// <param name="result">计算后的旋转四元数</param>
        [Rtti.Meta]
        public static void Squad(ref Quaternion q1, ref Quaternion a, ref Quaternion b, ref Quaternion c, float t, out Quaternion result)
        {
            unsafe
            {
                fixed (Quaternion* pin1 = &q1)
                {
                    fixed (Quaternion* pinA = &a)
                    {
                        fixed (Quaternion* pinB = &b)
                        {
                            fixed (Quaternion* pinC = &c)
                            {
                                fixed (Quaternion* pinResult = &result)
                                {
                                    IDllImportApi.v3dxQuaternionSquad(pinResult, pin1, pinA, pinB, pinC, t);
                                }
                            }
                        }
                    }
                }
            }
        }

        //public static Quaternion[] SquadSetup( Quaternion source1, Quaternion source2, Quaternion source3, Quaternion source4 )
        //{
        // Quaternion result1 = new Quaternion();
        // Quaternion result2 = new Quaternion();
        // Quaternion result3 = new Quaternion();
        //       Quaternion[] results = new Quaternion[3];

        //       unsafe
        //       {
        //           IDllImportApi.D3DXQuaternionSquadSetup((Quaternion*)&result1, (Quaternion*)&result2, (Quaternion*)&result3,
        //                  (Quaternion*)&source1, (Quaternion*)&source2, (Quaternion*)&source3, (Quaternion*)&source4);
        //       }

        // results[0] = result1;
        // results[1] = result2;
        // results[2] = result3;
        // return results;
        //}
        /// <summary>
        /// 两个四元数相减
        /// </summary>
        /// <param name="left">旋转四元数</param>
        /// <param name="right">旋转四元数</param>
        /// <returns>返回计算后的旋转四元数</returns>
        [Rtti.Meta]
        public static Quaternion Subtract(Quaternion left, Quaternion right)
        {
            Quaternion result;
            result.X = left.X - right.X;
            result.Y = left.Y - right.Y;
            result.Z = left.Z - right.Z;
            result.W = left.W - right.W;
            return result;
        }
        /// <summary>
        /// 两个四元数相减
        /// </summary>
        /// <param name="left">旋转四元数</param>
        /// <param name="right">旋转四元数</param>
        /// <param name="result">计算后的旋转四元数</param>
        [Rtti.Meta]
        public static void Subtract(ref Quaternion left, ref Quaternion right, out Quaternion result)
        {
            result.X = left.X - right.X;
            result.Y = left.Y - right.Y;
            result.Z = left.Z - right.Z;
            result.W = left.W - right.W;
        }
        /// <summary>
        /// 获取两个向量的旋转四元数
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <returns></returns>
        [Rtti.Meta]
        public static Quaternion GetQuaternion(EngineNS.Vector3 from, EngineNS.Vector3 to)
        {
            from.Normalize();
            to.Normalize();
            var normal = EngineNS.Vector3.Cross(from, to);
            normal.Normalize();
            var angle = MathHelper.Acos(EngineNS.Vector3.Dot(from, to));
            return RotationAxis(normal, (float)angle);
        }
        //[Rtti.Meta]
        //public static Quaternion GetQuaternionWithAxis(EngineNS.Vector3 axis,EngineNS.Vector3 from, EngineNS.Vector3 to)
        //{
        //    from.Normalize();
        //    to.Normalize();
        //    var normal1 = EngineNS.Vector3.Cross(from, to);
        //    var angle1 = MathHelper.Acos(EngineNS.Vector3.Dot(from, to));

        //    var rot1 = Quaternion.RotationAxis(normal1, angle1);

        //    var newUp = Quaternion.RotateVector3(rot1, axis);

        //    var normal2 = EngineNS.Vector3.Cross(newUp, to);
        //    float invFactor = 1;
        //    var normal3 = EngineNS.Vector3.Cross(axis, to);

        //    normal2.Normalize();
        //    normal3.Normalize();
        //    var angle2 = MathHelper.Acos(EngineNS.Vector3.Dot(normal2, normal3));
        //    Quaternion rot2;
        //    if (newUp.X < 0)
        //        rot2 = Quaternion.RotationAxis(to, angle2 * invFactor);
        //    else
        //        rot2 = Quaternion.RotationAxis(to, -angle2 * invFactor);

        //    return Quaternion.Multiply(rot1, rot2);
        //}
        [Rtti.Meta]
        public static Quaternion GetQuaternionUp(EngineNS.Vector3 from, EngineNS.Vector3 to)
        {
            var m1 = Matrix.MakeFromZ(from);
            var m2 = Matrix.MakeFromZ(to);

            m1.Inverse();
            var m3 = m1 * m2;
            return Quaternion.RotationMatrix(m3);
            //from.Normalize();
            //to.Normalize();
            //var normal1 = EngineNS.Vector3.Cross(from, to);
            //var angle1 = MathHelper.Acos(EngineNS.Vector3.Dot(from, to));

            //var rot1 = Quaternion.RotationAxis(normal1, angle1);

            //var newUp = Quaternion.RotateVector3(rot1, Vector3.UnitY);

            //var normal2 = EngineNS.Vector3.Cross(newUp, to);
            //float invFactor = 1;
            //var normal3 = EngineNS.Vector3.Cross(Vector3.UnitY, to);

            //normal2.Normalize();
            //normal3.Normalize();
            //var angle2 = MathHelper.Acos(EngineNS.Vector3.Dot(normal2, normal3));
            //Quaternion rot2;
            //if (newUp.X < 0)
            //    rot2 = Quaternion.RotationAxis(to, angle2 * invFactor);
            //else
            //    rot2 = Quaternion.RotationAxis(to, -angle2 * invFactor);

            //return Quaternion.Multiply(rot1, rot2);
        }
        /// <summary>
        /// 重载"*"号运算符
        /// </summary>
        /// <param name="left">旋转四元数</param>
        /// <param name="right">旋转四元数</param>
        /// <returns>返回计算后的旋转四元数</returns>
	    public static Quaternion operator *(Quaternion left, Quaternion right)
        {
            Quaternion quaternion;
            float lx = left.X;
            float ly = left.Y;
            float lz = left.Z;
            float lw = left.W;
            float rx = right.X;
            float ry = right.Y;
            float rz = right.Z;
            float rw = right.W;

            quaternion.X = (rx * lw + lx * rw + ry * lz) - (rz * ly);
            quaternion.Y = (ry * lw + ly * rw + rz * lx) - (rx * lz);
            quaternion.Z = (rz * lw + lz * rw + rx * ly) - (ry * lx);
            quaternion.W = (rw * lw) - (rx * lx + ry * ly + rz * lz);

            return quaternion;
        }
        /// <summary>
        /// 重载"*"号运算符
        /// </summary>
        /// <param name="quaternion">旋转四元数</param>
        /// <param name="scale">缩放值</param>
        /// <returns>计算后的旋转四元数</returns>
	    public static Quaternion operator *(Quaternion quaternion, float scale)
        {
            Quaternion result;
            result.X = quaternion.X * scale;
            result.Y = quaternion.Y * scale;
            result.Z = quaternion.Z * scale;
            result.W = quaternion.W * scale;
            return result;
        }
        public static Vector3 operator *(Quaternion rotation, Vector3 point)
        {
            float x = rotation.X * 2F;
            float y = rotation.Y * 2F;
            float z = rotation.Z * 2F;
            float xx = rotation.X * x;
            float yy = rotation.Y * y;
            float zz = rotation.Z * z;
            float xy = rotation.X * y;
            float xz = rotation.X * z;
            float yz = rotation.Y * z;
            float wx = rotation.W * x;
            float wy = rotation.W * y;
            float wz = rotation.W * z;

            Vector3 res;
            res.X = (1F - (yy + zz)) * point.X + (xy - wz) * point.Y + (xz + wy) * point.Z;
            res.Y = (xy + wz) * point.X + (1F - (xx + zz)) * point.Y + (yz - wx) * point.Z;
            res.Z = (xz - wy) * point.X + (yz + wx) * point.Y + (1F - (xx + yy)) * point.Z;
            return res;
        }
        /// <summary>
        /// 重载"*"号运算符
        /// </summary>
        /// <param name="scale">缩放值</param>
        /// <param name="quaternion">旋转四元数</param>
        /// <returns>返回计算后的旋转四元数</returns>
	    public static Quaternion operator *(float scale, Quaternion quaternion)
        {
            Quaternion result;
            result.X = quaternion.X * scale;
            result.Y = quaternion.Y * scale;
            result.Z = quaternion.Z * scale;
            result.W = quaternion.W * scale;
            return result;
        }
        /// <summary>
        /// 重载"/"号运算符
        /// </summary>
        /// <param name="lhs">旋转四元数</param>
        /// <param name="rhs">除数</param>
        /// <returns>返回计算后的旋转四元数</returns>
	    public static Quaternion operator /(Quaternion lhs, float rhs)
        {
            Quaternion result;
            result.X = lhs.X / rhs;
            result.Y = lhs.Y / rhs;
            result.Z = lhs.Z / rhs;
            result.W = lhs.W / rhs;
            return result;
        }
        /// <summary>
        /// 重载"+"号运算符
        /// </summary>
        /// <param name="lhs">旋转四元数</param>
        /// <param name="rhs">旋转四元数</param>
        /// <returns>返回计算后的旋转四元数</returns>
	    public static Quaternion operator +(Quaternion lhs, Quaternion rhs)
        {
            Quaternion result;
            result.X = lhs.X + rhs.X;
            result.Y = lhs.Y + rhs.Y;
            result.Z = lhs.Z + rhs.Z;
            result.W = lhs.W + rhs.W;
            return result;
        }
        /// <summary>
        /// 重载"-"号运算符
        /// </summary>
        /// <param name="lhs">旋转四元数</param>
        /// <param name="rhs">旋转四元数</param>
        /// <returns>返回计算后的旋转四元数</returns>
	    public static Quaternion operator -(Quaternion lhs, Quaternion rhs)
        {
            Quaternion result;
            result.X = lhs.X - rhs.X;
            result.Y = lhs.Y - rhs.Y;
            result.Z = lhs.Z - rhs.Z;
            result.W = lhs.W - rhs.W;
            return result;
        }
        /// <summary>
        /// 重载"-"号运算符，用于取反
        /// </summary>
        /// <param name="quaternion">旋转四元数</param>
        /// <returns>返回计算后的旋转四元数</returns>
	    public static Quaternion operator -(Quaternion quaternion)
        {
            Quaternion result;
            result.X = -quaternion.X;
            result.Y = -quaternion.Y;
            result.Z = -quaternion.Z;
            result.W = -quaternion.W;
            return result;
        }
        /// <summary>
        /// 重载"=="号运算符
        /// </summary>
        /// <param name="left">旋转四元数</param>
        /// <param name="right">旋转四元数</param>
        /// <returns>如果两个对象相等返回true，否则返回false</returns>
	    public static bool operator ==(Quaternion left, Quaternion right)
        {
            return left.Equals(right);
            //return Equals( left, right );
        }
        /// <summary>
        /// 重载"!="号运算符
        /// </summary>
        /// <param name="left">旋转四元数</param>
        /// <param name="right">旋转四元数</param>
        /// <returns>如果两个对象不相等返回true，否则返回false</returns>
	    public static bool operator !=(Quaternion left, Quaternion right)
        {
            return !left.Equals(right);
            //return !Equals( left, right );
        }
    }
}
