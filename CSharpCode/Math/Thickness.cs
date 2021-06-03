using System;

namespace EngineNS
{
    // Summary:
    //     Describes the thickness of a frame around a rectangle. Four System.Double
    //     values describe the CSUtility.Support.Thickness.Left, CSUtility.Support.Thickness.Top,
    //     CSUtility.Support.Thickness.Right, and CSUtility.Support.Thickness.Bottom sides
    //     of the rectangle, respectively.
    public struct Thickness : IEquatable<Thickness>
    {
        [Rtti.Meta]
        public static Thickness Empty = new Thickness(0);
        //
        // Summary:
        //     Initializes a new instance of the CSUtility.Support.Thickness structure that
        //     has the specified uniform length on each side.
        //
        // Parameters:
        //   uniformLength:
        //     The uniform length applied to all four sides of the bounding rectangle.
        public Thickness(float uniformLength)
        {
            mLeft = mBottom = mRight = mTop = uniformLength;
        }
        //
        // Summary:
        //     Initializes a new instance of the CSUtility.Support.Thickness structure that
        //     has specific lengths (supplied as a System.Double) applied to each side of
        //     the rectangle.
        //
        // Parameters:
        //   left:
        //     The thickness for the left side of the rectangle.
        //
        //   top:
        //     The thickness for the upper side of the rectangle.
        //
        //   right:
        //     The thickness for the right side of the rectangle
        //
        //   bottom:
        //     The thickness for the lower side of the rectangle.
        public Thickness(double left, double top, double right, double bottom)
        {
            mLeft = (float)left;
            mTop = (float)top;
            mRight = (float)right;
            mBottom = (float)bottom;
        }
        public Thickness(float left, float top, float right, float bottom)
        {
            mLeft = left;
            mTop = top;
            mRight = right;
            mBottom = bottom;
        }

        // Summary:
        //     Compares two CSUtility.Support.Thickness structures for inequality.
        //
        // Parameters:
        //   t1:
        //     The first structure to compare.
        //
        //   t2:
        //     The other structure to compare.
        //
        // Returns:
        //     true if the two instances of CSUtility.Support.Thickness are not equal; otherwise,
        //     false.
        public static bool operator !=(Thickness t1, Thickness t2)
        {
            return ((t1.Left != t2.Left) || (t1.Top != t2.Top) || (t1.Right != t2.Right) || (t1.Bottom != t2.Bottom));
        }
        //
        // Summary:
        //     Compares the value of two CSUtility.Support.Thickness structures for equality.
        //
        // Parameters:
        //   t1:
        //     The first structure to compare.
        //
        //   t2:
        //     The other structure to compare.
        //
        // Returns:
        //     true if the two instances of CSUtility.Support.Thickness are equal; otherwise,
        //     false.
        public static bool operator ==(Thickness t1, Thickness t2)
        {
            return ((t1.Left == t2.Left) && (t1.Top == t2.Top) && (t1.Right == t2.Right) && (t1.Bottom == t2.Bottom));
        }

        // Summary:
        //     Gets or sets the width, in pixels, of the lower side of the bounding rectangle.
        //
        // Returns:
        //     A System.Double that represents the width, in pixels, of the lower side of
        //     the bounding rectangle for this instance of CSUtility.Support.Thickness. A pixel
        //     is equal to 1/96 of an inch. The default is 0.
        float mBottom;
        [Rtti.Meta]
        public float Bottom
        {
            get { return mBottom; }
            set { mBottom = value; }
        }
        //
        // Summary:
        //     Gets or sets the width, in pixels, of the left side of the bounding rectangle.
        //
        // Returns:
        //     A System.Double that represents the width, in pixels, of the left side of
        //     the bounding rectangle for this instance of CSUtility.Support.Thickness. a pixel
        //     is equal to 1/96 on an inch. The default is 0.
        float mLeft;
        [Rtti.Meta]
        public float Left
        {
            get { return mLeft; }
            set { mLeft = value; }
        }
        //
        // Summary:
        //     Gets or sets the width, in pixels, of the right side of the bounding rectangle.
        //
        // Returns:
        //     A System.Double that represents the width, in pixels, of the right side of
        //     the bounding rectangle for this instance of CSUtility.Support.Thickness. A pixel
        //     is equal to 1/96 of an inch. The default is 0.
        float mRight;
        [Rtti.Meta]
        public float Right
        {
            get { return mRight; }
            set { mRight = value; }
        }
        //
        // Summary:
        //     Gets or sets the width, in pixels, of the upper side of the bounding rectangle.
        //
        // Returns:
        //     A System.Double that represents the width, in pixels, of the upper side of
        //     the bounding rectangle for this instance of CSUtility.Support.Thickness. A pixel
        //     is equal to 1/96 of an inch. The default is 0.
        float mTop;
        [Rtti.Meta]
        public float Top
        {
            get { return mTop; }
            set { mTop = value; }
        }

        // Summary:
        //     Compares this CSUtility.Support.Thickness structure to another System.Object
        //     for equality.
        //
        // Parameters:
        //   obj:
        //     The object to compare.
        //
        // Returns:
        //     true if the two objects are equal; otherwise, false.
        public override bool Equals(object obj)
        {
            if (!(obj is Thickness))
                return false;

            return Equals((Thickness)obj);
        }
        //
        // Summary:
        //     Compares this CSUtility.Support.Thickness structure to another CSUtility.Support.Thickness
        //     structure for equality.
        //
        // Parameters:
        //   thickness:
        //     An instance of CSUtility.Support.Thickness to compare for equality.
        //
        // Returns:
        //     true if the two instances of CSUtility.Support.Thickness are equal; otherwise,
        //     false.
        public bool Equals(Thickness thickness)
        {
            return this == thickness;
        }
        //
        // Summary:
        //     Returns the hash code of the structure.
        //
        // Returns:
        //     A hash code for this instance of CSUtility.Support.Thickness.
        public override int GetHashCode()
        {
            return (int)EngineNS.UniHash.DefaultHash(this.ToString());// ToString().GetHashCode();
        }
        //
        // Summary:
        //     Returns the string representation of the CSUtility.Support.Thickness structure.
        //
        // Returns:
        //     A System.String that represents the CSUtility.Support.Thickness value.
        public override string ToString()
        {
            return Left + "," + Top + "," + Right + "," + Bottom;
        }
    }
}
