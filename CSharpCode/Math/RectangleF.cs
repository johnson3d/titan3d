using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS
{
    [System.ComponentModel.TypeConverterAttribute("System.ComponentModel.ExpandableObjectConverter")]
    public struct RectangleF
    {
        // 考虑到Width、Height有小于0的情况
        [Rtti.Meta]
        public float Left
        {
            get { return System.Math.Min(X, X + Width); }
        }
        [Rtti.Meta]
        public float Top
        {
            get { return System.Math.Min(Y, Y + Height); }
        }
        [Rtti.Meta]
        public float Right
        {
            get { return System.Math.Max(X, X + Width); }
        }
        [Rtti.Meta]
        public float Bottom
        {
            get { return System.Math.Max(Y, Y + Height); }
        }

        [Rtti.Meta]
        public bool IsEmpty
        {
            get
            {
                if (Width == 0 && Height == 0)
                    return true;

                return false;
            }
        }
        [Rtti.Meta]
        public SizeF Size
        {
            get { return new SizeF(Width, Height); }
            set
            {
                Width = value.Width;
                Height = value.Height;
            }
        }
        [Rtti.Meta]
        public Point2f Location
        {
            get { return new Point2f(X, Y); }
            set
            {
                X = value.X;
                Y = value.Y;
            }
        }
        [Rtti.Meta]
        public float X
        {
            get;
            set;
        }
        [Rtti.Meta]
        public float Y
        {
            get;
            set;
        }
        [Rtti.Meta]
        public float Width
        {
            get;
            set;
        }
        [Rtti.Meta]
        public float Height
        {
            get;
            set;
        }

        static RectangleF mEmpty = new RectangleF(0, 0, 0, 0);
        [Rtti.Meta]
        public static RectangleF Empty
        {
            get { return mEmpty; }
        }
        static RectangleF mInfinity = new RectangleF(0, 0, float.PositiveInfinity, float.PositiveInfinity);
        public static RectangleF Infinity => mInfinity;

        public RectangleF(float x, float y, float width, float height)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }

        public RectangleF(Point2f pt, SizeF size)
        {
            X = pt.X;
            Y = pt.Y;
            Width = size.Width;
            Height = size.Height;
        }
        public bool Contains(in Point2f pt)
        {
            if ((pt.X >= Left) && (pt.Y >= Top) && (pt.X < Right) && (pt.Y < Bottom))
                return true;

            return false;
        }
        [Rtti.Meta]
        public bool Contains(Point2f pt)
        {
            return Contains(in pt);
        }
        [Rtti.Meta]
        public bool Contains(float x, float y)
        {
            if ((x >= Left) && (y >= Top) && (x < Right) && (y < Bottom))
                return true;

            return false;
        }

        [Rtti.Meta]
        public static RectangleF Intersect( RectangleF a, RectangleF b )
        {
            return Intersect(in a, in b);
        }
        public static RectangleF Intersect(in RectangleF a, in RectangleF b)
        {
            if (a.Left > b.Right || a.Top > b.Bottom || a.Right < b.Left || a.Bottom < b.Top)
                return RectangleF.Empty;

            var left = System.Math.Max(a.Left, b.Left);
            var top = System.Math.Max(a.Top, b.Top);
            var right = System.Math.Min(a.Right, b.Right);
            var bottom = System.Math.Min(a.Bottom, b.Bottom);

            return new RectangleF(left, top, right - left, bottom - top);
        }
        [Rtti.Meta]
        public RectangleF Intersect(in RectangleF rect)
        {
            return Intersect(in this, in rect);
        }

        public override bool Equals(object obj)
        {
            var tag = (RectangleF)obj;
            if ((System.Math.Abs(X - tag.X) <= MathHelper.Epsilon) &&
                (System.Math.Abs(Y - tag.Y) <= MathHelper.Epsilon) &&
                (System.Math.Abs(Width - tag.Width) <= MathHelper.Epsilon) &&
                (System.Math.Abs(Height - tag.Height) <= MathHelper.Epsilon))
                return true;
            return false;
        }
        public bool Equals(in RectangleF tag)
        {
            if ((System.Math.Abs(X - tag.X) <= MathHelper.Epsilon) &&
                (System.Math.Abs(Y - tag.Y) <= MathHelper.Epsilon) &&
                (System.Math.Abs(Width - tag.Width) <= MathHelper.Epsilon) &&
                (System.Math.Abs(Height - tag.Height) <= MathHelper.Epsilon))
                return true;
            return false;
        }

        public override int GetHashCode()
        {
            //return (int)(X + Y + Width + Height);
            return base.GetHashCode();
        }
    }

    public struct Interpolation
    {
        public static float BarycentricInterpolation(in Vector3 uvw, float v0, float v1, float v2)
        {
            return uvw.X * v0 + uvw.Y * v1 + uvw.Z * v2;
        }
        public static Vector2 BarycentricInterpolation(in Vector3 uvw, in Vector2 v0, in Vector2 v1, in Vector2 v2)
        {
            return v0 * uvw.X + v1 * uvw.Y + v2 * uvw.Z;
        }
        public static Vector3 BarycentricInterpolation(in Vector3 uvw, in Vector3 v0, in Vector3 v1, in Vector3 v2) 
        {
            return uvw.X * v0 + uvw.Y * v1 + uvw.Z * v2;
        }
        public static Vector4 BarycentricInterpolation(in Vector3 uvw, in Vector4 v0, in Vector4 v1, in Vector4 v2)
        {
            return uvw.X * v0 + uvw.Y * v1 + uvw.Z * v2;
        }
    }
    public struct FSquareSurface
    {
        public readonly static FSquareSurface Identity = new FSquareSurface();
        public Vector4 UV_0_0;//nor:xyz,height:w
        public Vector4 UV_1_0;
        public Vector4 UV_0_1;
        public Vector4 UV_1_1;
        public static FSquareSurface Maximize(in FSquareSurface left, in FSquareSurface right)
        {
            FSquareSurface result;
            result.UV_0_0 = Vector4.Maximize(in left.UV_0_0, in right.UV_0_0);
            result.UV_1_0 = Vector4.Maximize(in left.UV_1_0, in right.UV_1_0);
            result.UV_0_1 = Vector4.Maximize(in left.UV_0_1, in right.UV_0_1);
            result.UV_1_1 = Vector4.Maximize(in left.UV_1_1, in right.UV_1_1);
            return result;
        }
        public static FSquareSurface Minimize(in FSquareSurface left, in FSquareSurface right)
        {
            FSquareSurface result;
            result.UV_0_0 = Vector4.Minimize(in left.UV_0_0, in right.UV_0_0);
            result.UV_1_0 = Vector4.Minimize(in left.UV_1_0, in right.UV_1_0);
            result.UV_0_1 = Vector4.Minimize(in left.UV_0_1, in right.UV_0_1);
            result.UV_1_1 = Vector4.Minimize(in left.UV_1_1, in right.UV_1_1);
            return result;
        }
        public Vector4 GetPoint(float u, float v)//u,v [0-1]
        {
            if (v >= u)
            {
                Vector3 uvw;
                uvw.X = u;
                uvw.Y = (1 - v);
                uvw.Z = 1 - uvw.X - uvw.Y;
                return Interpolation.BarycentricInterpolation(in uvw, in UV_1_1, in UV_0_0, in UV_0_1);
            }
            else
            {
                Vector3 uvw;
                uvw.X = 1 - u;
                uvw.Y = v;
                uvw.Z = 1 - uvw.X - uvw.Y;
                return Interpolation.BarycentricInterpolation(in uvw, in UV_0_0, in UV_1_1, in UV_1_0);
            }
        }
    }
}
