using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS
{
    public struct DMatrix2x2 : System.IEquatable<DMatrix2x2>
    {
        #region Member
        public double M11;
        public double M12;
        public double M21;
        public double M22;
        #endregion
        public bool Equals(DMatrix2x2 value)
        {
            return (M11 == value.M11 && M12 == value.M12 &&
                M21 == value.M21 && M22 == value.M22);
        }
        public static DMatrix2x2 operator *(DMatrix2x2 left, DMatrix2x2 right)
        {
            DMatrix2x2 result;

            result.M11 = (left.M11 * right.M11) + (left.M12 * right.M21);
            result.M12 = (left.M11 * right.M12) + (left.M12 * right.M22);
            result.M21 = (left.M21 * right.M11) + (left.M22 * right.M21);
            result.M22 = (left.M21 * right.M12) + (left.M22 * right.M22);
            return result;
        }
    }
}
