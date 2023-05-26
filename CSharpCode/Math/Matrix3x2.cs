using System;
using System.Globalization;

namespace EngineNS
{
    /// <summary>
    /// 3*2矩阵结构体
    /// </summary>
    [System.Serializable]
	[System.Runtime.InteropServices.StructLayout( System.Runtime.InteropServices.LayoutKind.Sequential, Pack = 4 )]
	public struct Matrix3x2 : System.IEquatable<Matrix3x2>
	{
        public float M11;
		public float M12;
		public float M21;
		public float M22;
		public float M31;
		public float M32;
        /// <summary>
        /// 只读属性，标准矩阵
        /// </summary>
        public static Matrix3x2 Identity
	    {
            get
            {
		        return mIdentity;
            }
	    }
        static Matrix3x2 mIdentity = new Matrix3x2();
        static Matrix3x2()
        {
            mIdentity.M11 = 1.0f;
            mIdentity.M22 = 1.0f;
        }
        //Matrix3x2 Invert( Matrix3x2 mat )
        //{
        //    Matrix3x2 result = mat;
        //    (D2D1.Matrix3x2F.ReinterpretBaseType((D2D1_MATRIX_3X2_F*)&result)).Invert();
        //    return result;
        //}

        //void Invert( ref Matrix3x2 mat, out Matrix3x2 result )
        //{
        //    result = mat;

        //    pin_ptr<Matrix3x2> pinResult = &result;

        //    (D2D1.Matrix3x2F.ReinterpretBaseType((D2D1_MATRIX_3X2_F*)pinResult)).Invert();
        //}
        /// <summary>
        /// 重载"*"号运算符
        /// </summary>
        /// <param name="left">矩阵对象</param>
        /// <param name="right">矩阵对象</param>
        /// <returns>返回计算后的矩阵对象</returns>
        public static Matrix3x2 operator *(Matrix3x2 left, Matrix3x2 right)
	    {
		    Matrix3x2 r;

		    r.M11 = (left.M11 * right.M11) + (left.M12 * right.M21);
            r.M12 = (left.M11 * right.M12) + (left.M12 * right.M22);
            r.M21 = (left.M21 * right.M11) + (left.M22 * right.M21);
            r.M22 = (left.M21 * right.M12) + (left.M22 * right.M22);
            r.M31 = (left.M31 * right.M11) + (left.M32 * right.M21) + right.M31;
            r.M32 = (left.M31 * right.M12) + (left.M32 * right.M22) + right.M32;

		    return r;
	    }
        /// <summary>
        /// 两个矩阵相乘
        /// </summary>
        /// <param name="left">矩阵对象</param>
        /// <param name="right">矩阵对象</param>
        /// <returns>返回计算后的矩阵对象</returns>
        public static Matrix3x2 Multiply(Matrix3x2 left, Matrix3x2 right)
	    {
		    Matrix3x2 r;

		    r.M11 = (left.M11 * right.M11) + (left.M12 * right.M21);
            r.M12 = (left.M11 * right.M12) + (left.M12 * right.M22);
            r.M21 = (left.M21 * right.M11) + (left.M22 * right.M21);
            r.M22 = (left.M21 * right.M12) + (left.M22 * right.M22);
            r.M31 = (left.M31 * right.M11) + (left.M32 * right.M21) + right.M31;
            r.M32 = (left.M31 * right.M12) + (left.M32 * right.M22) + right.M32;

		    return r;
	    }
        /// <summary>
        /// 两个矩阵相乘
        /// </summary>
        /// <param name="left">矩阵对象</param>
        /// <param name="right">矩阵对象</param>
        /// <param name="result">计算后的矩阵对象</param>
        public static void Multiply(ref Matrix3x2 left, ref Matrix3x2 right, out Matrix3x2 result)
	    {
		    Matrix3x2 r;

		    r.M11 = (left.M11 * right.M11) + (left.M12 * right.M21);
            r.M12 = (left.M11 * right.M12) + (left.M12 * right.M22);
            r.M21 = (left.M21 * right.M11) + (left.M22 * right.M21);
            r.M22 = (left.M21 * right.M12) + (left.M22 * right.M22);
            r.M31 = (left.M31 * right.M11) + (left.M32 * right.M21) + right.M31;
            r.M32 = (left.M31 * right.M12) + (left.M32 * right.M22) + right.M32;

		    result = r;
	    }
        /// <summary>
        /// 3*2矩阵的旋转计算
        /// </summary>
        /// <param name="angle">旋转角度</param>
        /// <param name="centerPoint">旋转的中心点</param>
        /// <returns>返回计算后的矩阵</returns>
        public static Matrix3x2 Rotation(float angle, Point2f centerPoint)
	    {
		    return Translation( -centerPoint.X, -centerPoint.Y ) *
			       Rotation( angle ) *
			       Translation( centerPoint.X, centerPoint.Y );
	    }
        /// <summary>
        /// 3*2矩阵的旋转计算
        /// </summary>
        /// <param name="angle">旋转角度</param>
        /// <param name="centerPoint">旋转的中心点</param>
        /// <param name="result">计算后的矩阵</param>
        public static void Rotation(float angle, ref Point2f centerPoint, out Matrix3x2 result)
	    {
		    result = Translation( -centerPoint.X, -centerPoint.Y ) *
				     Rotation( angle ) *
				     Translation( centerPoint.X, centerPoint.Y );
	    }
        /// <summary>
        /// 3*2矩阵的旋转计算
        /// </summary>
        /// <param name="angle">旋转角度</param>
        /// <param name="result">计算后的矩阵</param>
        public static void Rotation(float angle, out Matrix3x2 result)
	    {
		    Matrix3x2 r;

		    double radians = (Math.PI * angle) / 180.0;

		    float cos = (float)( Math.Cos( radians ) );
		    float sin = (float)( Math.Sin( radians ) );

		    r.M11 = cos;
		    r.M12 = sin;
		    r.M21 = -sin;
		    r.M22 = cos;
		    r.M31 = 0.0f;
		    r.M32 = 0.0f;

		    result = r;
	    }
        /// <summary>
        /// 3*2矩阵的旋转计算
        /// </summary>
        /// <param name="angle">旋转角度</param>
        /// <returns>返回计算后的矩阵</returns>
        public static Matrix3x2 Rotation(float angle)
	    {
		    Matrix3x2 result;

		    double radians = (Math.PI * angle) / 180.0;

		    float cos = (float)( Math.Cos( radians ) );
		    float sin = (float)( Math.Sin( radians ) );

		    result.M11 = cos;
		    result.M12 = sin;
		    result.M21 = -sin;
		    result.M22 = cos;
		    result.M31 = 0.0f;
		    result.M32 = 0.0f;

		    return result;
	    }
        /// <summary>
        /// 3*2矩阵的缩放计算
        /// </summary>
        /// <param name="size">缩放尺寸</param>
        /// <returns>返回计算后的矩阵</returns>
        public static Matrix3x2 Scale(SizeF size)
	    {
		    Matrix3x2 r;

		    r.M11 = size.Width;	r.M12 = 0.0f;
		    r.M21 = 0.0f;		r.M22 = size.Height;
		    r.M31 = 0.0f;		r.M32 = 0.0f;

		    return r;
	    }
        /// <summary>
        /// 3*2矩阵的缩放计算
        /// </summary>
        /// <param name="size">缩放尺寸</param>
        /// <param name="result">计算后的矩阵</param>
        public static void Scale(ref SizeF size, out Matrix3x2 result)
	    {
		    Matrix3x2 r;

		    r.M11 = size.Width;	r.M12 = 0.0f;
		    r.M21 = 0.0f;		r.M22 = size.Height;
		    r.M31 = 0.0f;		r.M32 = 0.0f;

		    result = r;
	    }
        /// <summary>
        /// 3*2矩阵的缩放计算
        /// </summary>
        /// <param name="x">X方向的缩放值</param>
        /// <param name="y">Y方向的缩放值</param>
        /// <returns>返回计算后的矩阵</returns>
        public static Matrix3x2 Scale(float x, float y)
	    {
		    Matrix3x2 r;

		    r.M11 = x;			r.M12 = 0.0f;
		    r.M21 = 0.0f;		r.M22 = y;
		    r.M31 = 0.0f;		r.M32 = 0.0f;

		    return r;
	    }
        /// <summary>
        /// 3*2矩阵的缩放计算
        /// </summary>
        /// <param name="x">X方向的缩放值</param>
        /// <param name="y">Y方向的缩放值</param>
        /// <param name="result">计算后的矩阵</param>
        public static void Scale(float x, float y, out Matrix3x2 result)
	    {
		    Matrix3x2 r;

		    r.M11 = x;			r.M12 = 0.0f;
		    r.M21 = 0.0f;		r.M22 = y;
		    r.M31 = 0.0f;		r.M32 = 0.0f;

		    result = r;
	    }
        /// <summary>
        /// 3*2矩阵的缩放计算
        /// </summary>
        /// <param name="size">缩放尺寸</param>
        /// <param name="centerPoint">缩放中心</param>
        /// <returns>返回计算后的矩阵</returns>
        public static Matrix3x2 Scale(SizeF size, Point2f centerPoint)
	    {
		    Matrix3x2 r;

		    r.M11 = size.Width;	r.M12 = 0.0f;
		    r.M21 = 0.0f;		r.M22 = size.Height;

		    r.M31 = centerPoint.X - (size.Width * centerPoint.X);
		    r.M32 = centerPoint.Y - (size.Height * centerPoint.Y);

		    return r;
	    }
        /// <summary>
        /// 3*2矩阵的缩放计算
        /// </summary>
        /// <param name="size">缩放尺寸</param>
        /// <param name="centerPoint">缩放中心</param>
        /// <param name="result">计算后的矩阵</param>
        public static void Scale(ref SizeF size, ref Point2f centerPoint, out Matrix3x2 result)
	    {
		    Matrix3x2 r;

		    r.M11 = size.Width;	r.M12 = 0.0f;
		    r.M21 = 0.0f;		r.M22 = size.Height;

		    r.M31 = centerPoint.X - (size.Width * centerPoint.X);
		    r.M32 = centerPoint.Y - (size.Height * centerPoint.Y);

		    result = r;
	    }
        /// <summary>
        /// 3*2矩阵的缩放计算
        /// </summary>
        /// <param name="x">X方向的缩放值</param>
        /// <param name="y">Y方向的缩放值</param>
        /// <param name="centerPoint">缩放中心点</param>
        /// <returns>返回计算后的矩阵</returns>
        public static Matrix3x2 Scale(float x, float y, Point2f centerPoint)
	    {
		    Matrix3x2 r;

		    r.M11 = x;		r.M12 = 0.0f;
		    r.M21 = 0.0f;	r.M22 = y;

		    r.M31 = centerPoint.X - (x * centerPoint.X);
		    r.M32 = centerPoint.Y - (y * centerPoint.Y);

		    return r;
	    }
        /// <summary>
        /// 3*2矩阵的缩放计算
        /// </summary>
        /// <param name="x">X方向的缩放值</param>
        /// <param name="y">Y方向的缩放值</param>
        /// <param name="centerPoint">缩放中心点</param>
        /// <param name="result">计算后的矩阵</param>
        public static void Scale(float x, float y, ref Point2f centerPoint, out Matrix3x2 result)
	    {
		    Matrix3x2 r;

		    r.M11 = x;		r.M12 = 0.0f;
		    r.M21 = 0.0f;	r.M22 = y;

		    r.M31 = centerPoint.X - (x * centerPoint.X);
		    r.M32 = centerPoint.Y - (y * centerPoint.Y);

		    result = r;
	    }

        //public static Matrix3x2 Skew(float angleX, float angleY, CSUtility.Support.Point2f centerPoint)
        //{
        //    D2D1_POINT_2F centerPoint_n = D2D1.Point2F( centerPoint.X, centerPoint.Y );

        //    Matrix3x2 r;

        //    D2D1.Matrix3x2F mat = D2D1.Matrix3x2F.Skew( angleX, angleY, centerPoint_n );

        //    r.M11 = mat._11; r.M12 = mat._12;
        //    r.M21 = mat._21; r.M22 = mat._22;
        //    r.M31 = mat._31; r.M32 = mat._32;

        //    return r;
        //}

        //public static void Skew(float angleX, float angleY, ref CSUtility.Support.Point2f centerPoint, out Matrix3x2 result)
        //{
        //    D2D1_POINT_2F centerPoint_n = D2D1.Point2F( centerPoint.X, centerPoint.Y );

        //    Matrix3x2 r;

        //    D2D1.Matrix3x2F mat = D2D1.Matrix3x2F.Skew( angleX, angleY, centerPoint_n );

        //    r.M11 = mat._11; r.M12 = mat._12;
        //    r.M21 = mat._21; r.M22 = mat._22;
        //    r.M31 = mat._31; r.M32 = mat._32;

        //    result = r;
        //}
        /// <summary>
        /// 3*2矩阵的平移计算
        /// </summary>
        /// <param name="point">平移向量</param>
        /// <returns>返回计算后的矩阵</returns>
        public static Matrix3x2 Translation(Point2f point)
	    {
		    Matrix3x2 r;

		    r.M11 = 1.0f;		r.M12 = 0.0f;
		    r.M21 = 0.0f;		r.M22 = 1.0f;
		    r.M31 = point.X;	r.M32 = point.Y;

		    return r;
	    }
        /// <summary>
        /// 3*2矩阵的平移计算
        /// </summary>
        /// <param name="point">平移向量</param>
        /// <param name="result">计算后的矩阵</param>
        public static void Translation(ref Point2f point, out Matrix3x2 result)
	    {
		    Matrix3x2 r;

		    r.M11 = 1.0f;		r.M12 = 0.0f;
		    r.M21 = 0.0f;		r.M22 = 1.0f;
		    r.M31 = point.X;	r.M32 = point.Y;

		    result = r;
	    }
        /// <summary>
        /// 3*2矩阵的平移计算
        /// </summary>
        /// <param name="x">X方向的值</param>
        /// <param name="y">Y方向的值</param>
        /// <returns>返回计算后的矩阵</returns>
        public static Matrix3x2 Translation(float x, float y)
	    {
		    Matrix3x2 r;

		    r.M11 = 1.0f;	r.M12 = 0.0f;
		    r.M21 = 0.0f;	r.M22 = 1.0f;
		    r.M31 = x;		r.M32 = y;

		    return r;
	    }
        /// <summary>
        /// 3*2矩阵的平移计算
        /// </summary>
        /// <param name="x">X方向的值</param>
        /// <param name="y">Y方向的值</param>
        /// <param name="result">计算后的矩阵</param>
        public static void Translation(float x, float y, out Matrix3x2 result)
	    {
		    Matrix3x2 r;

		    r.M11 = 1.0f;	r.M12 = 0.0f;
		    r.M21 = 0.0f;	r.M22 = 1.0f;
		    r.M31 = x;		r.M32 = y;

		    result = r;
	    }
        /// <summary>
        /// 位置变换
        /// </summary>
        /// <param name="mat">变换矩阵</param>
        /// <param name="point">位置坐标</param>
        /// <returns>返回变换后的位置</returns>
        public static Point2f TransformPoint(Matrix3x2 mat, Point2f point)
	    {
		    Point2f result;

            result._X = (point.X * mat.M11) + (point.Y * mat.M21) + mat.M31;
            result._Y = (point.X * mat.M12) + (point.Y * mat.M22) + mat.M32;

		    return result;
	    }
        /// <summary>
        /// 位置变换
        /// </summary>
        /// <param name="mat">变换矩阵</param>
        /// <param name="point">位置坐标</param>
        /// <param name="result">变换后的位置</param>
        public static void TransformPoint(ref Matrix3x2 mat, ref Point2f point, out Point2f result)
	    {
            Point2f r;

		    r._X = (point.X * mat.M11) + (point.Y * mat.M21) + mat.M31;
		    r._Y = (point.X * mat.M12) + (point.Y * mat.M22) + mat.M32;

		    result = r;
	    }
        /// <summary>
        /// 只读属性，是否为单位矩阵
        /// </summary>
        public bool IsIdentity
	    {
            get
            {
                return (M11 == 1.0f && M12 == 0.0f &&
                          M21 == 0.0f && M22 == 1.0f &&
                          M31 == 0.0f && M32 == 0.0f);
            }
	    }
        /// <summary>
        /// 行列式的值
        /// </summary>
        /// <returns>返回行列式的值</returns>
        public float Determinant()
	    {
		    return (M11 * M22) - (M12 * M21);
	    }

        //bool Invert()
        //{
        //    pin_ptr<Matrix3x2> pinnedThis = this;

        //    return (D2D1.Matrix3x2F.ReinterpretBaseType((D2D1_MATRIX_3X2_F*)pinnedThis)).Invert();
        //}
        /// <summary>
        /// 重载"=="号运算符
        /// </summary>
        /// <param name="left">3x2矩阵</param>
        /// <param name="right">3x2矩阵</param>
        /// <returns>如果两个矩阵相等返回true，否则返回false</returns>
        public static bool operator ==(Matrix3x2 left, Matrix3x2 right)
	    {
            return left.Equals(right);
		    //return Equals( left, right );
	    }
        /// <summary>
        /// 重载"!="号运算符
        /// </summary>
        /// <param name="left">3x2矩阵</param>
        /// <param name="right">3x2矩阵</param>
        /// <returns>如果两个矩阵不相等返回true，否则返回false</returns>
        public static bool operator !=(Matrix3x2 left, Matrix3x2 right)
	    {
            return !left.Equals(right);
		    //return !Equals( left, right );
	    }
        /// <summary>
        /// 变换成String类型
        /// </summary>
        /// <returns>返回变换后的String类型对象</returns>
        public override System.String ToString()
	    {
		    return String.Format( CultureInfo.CurrentCulture, "[[M11:{0} M12:{1}] [M21:{2} M22:{3}] [M31:{4} M32:{5}]]",
			    M11.ToString(CultureInfo.CurrentCulture), M12.ToString(CultureInfo.CurrentCulture),
			    M21.ToString(CultureInfo.CurrentCulture), M22.ToString(CultureInfo.CurrentCulture),
			    M31.ToString(CultureInfo.CurrentCulture), M32.ToString(CultureInfo.CurrentCulture));
	    }
        /// <summary>
        /// 获取对象的哈希值
        /// </summary>
        /// <returns>返回对象的哈希值</returns>
        public override int GetHashCode()
	    {
		    return M11.GetHashCode() + M12.GetHashCode() +
			       M21.GetHashCode() + M22.GetHashCode() + 
			       M31.GetHashCode() + M32.GetHashCode();
	    }
        /// <summary>
        /// 判断两个对象是否相等
        /// </summary>
        /// <param name="obj">可转换成3x2矩阵的对象</param>
        /// <returns>如果两个矩阵相等返回true，否则返回false</returns>
        public override bool Equals(System.Object obj)
	    {
		    if( obj == null )
			    return false;

		    if( obj.GetType() != GetType() )
			    return false;

		    return Equals( (Matrix3x2)( obj ) );
	    }
        /// <summary>
        /// 判断两个对象是否相等
        /// </summary>
        /// <param name="other">3x2矩阵的对象</param>
        /// <returns>如果两个矩阵相等返回true，否则返回false</returns>
	    public bool Equals( Matrix3x2 other )
	    {
		    return ( M11 == other.M11 && M12 == other.M12 &&
				     M21 == other.M21 && M22 == other.M22 &&
				     M31 == other.M31 && M32 == other.M32 );
	    }
        /// <summary>
        /// 判断两个对象是否相等
        /// </summary>
        /// <param name="value1">3x2矩阵的对象</param>
        /// <param name="value2">3x2矩阵的对象</param>
        /// <returns>如果两个矩阵相等返回true，否则返回false</returns>
        public static bool Equals(ref Matrix3x2 value1, ref Matrix3x2 value2)
	    {
		    return ( value1.M11 == value2.M11 && value1.M12 == value2.M12 &&
				     value1.M21 == value2.M21 && value1.M22 == value2.M22 &&
				     value1.M31 == value2.M31 && value1.M32 == value2.M32 );
	    }
    }
}
