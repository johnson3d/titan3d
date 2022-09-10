using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS
{
    public struct Matrix2x2 : System.IEquatable<Matrix2x2>
    {
        #region Member
        public float M11;
        public float M12;
        public float M21;
        public float M22;
        #endregion
        public bool Equals(Matrix2x2 value)
        {
            return (M11 == value.M11 && M12 == value.M12 &&
                M21 == value.M21 && M22 == value.M22);
        }
        public static Matrix2x2 operator *(Matrix2x2 left, Matrix2x2 right)
        {
            Matrix2x2 result;

            result.M11 = (left.M11 * right.M11) + (left.M12 * right.M21);
            result.M12 = (left.M11 * right.M12) + (left.M12 * right.M22);
            result.M21 = (left.M21 * right.M11) + (left.M22 * right.M21);
            result.M22 = (left.M21 * right.M12) + (left.M22 * right.M22);
            return result;
        }
    }
}
