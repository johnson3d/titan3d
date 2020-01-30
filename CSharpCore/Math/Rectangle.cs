using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS
{
    [System.ComponentModel.TypeConverterAttribute("System.ComponentModel.ExpandableObjectConverter")]
    public struct Rectangle : IEquatable<Rectangle>
    {
        [Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public int Left
        {
            get { return X; }
        }
        [Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public int Top
        {
            get { return Y; }
        }
        [Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public int Right
        {
            get { return X + Width; }
        }
        [Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public int Bottom
        {
            get { return Y + Height; }
        }

        [Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public Point Center
        {
            get
            {
                return new Point(Left + Width / 2, Top + Height / 2);
            }
        }

        [Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable | Editor.MacrossMemberAttribute.enMacrossType.NotShowInBreak)]
        public bool IsEmpty
        {
            get
            {
                if (Width == 0 && Height == 0)
                    return true;

                return false;
            }
        }

        public bool Equals(Rectangle rhs)
        {
            return (Left == rhs.Left && Top == rhs.Top && Width == rhs.Width && Height == rhs.Height);
        }
        public override bool Equals(object o)
        {
            return false;
        }

        public override int GetHashCode()
        {
            return (Left + Top + Width + Height).GetHashCode();
        }
        
        public static bool operator == (Rectangle lhs, Rectangle rhs)
        {
            return (lhs.Left==rhs.Left && lhs.Top==rhs.Top && lhs.Width==rhs.Width && lhs.Height==rhs.Height);
        }

        public static bool operator !=(Rectangle lhs, Rectangle rhs)
        {
            return !(lhs.Left == rhs.Left && lhs.Top == rhs.Top && lhs.Width == rhs.Width && lhs.Height == rhs.Height);
        }
        [Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public Size Size
        {
            get { return new Size(Width, Height); }
            set
            {
                Width = value.Width;
                Height = value.Height;
            }
        }
        [Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public Point Location
        {
            get { return new Point(X, Y); }
            set
            {
                X = value.X;
                Y = value.Y;
            }
        }
        [Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public int X
        {
            get;
            set;
        }
        [Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public int Y
        {
            get;
            set;
        }
        [Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public int Width
        {
            get;
            set;
        }
        [Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public int Height
        {
            get;
            set;
        }
        static Rectangle mEmpty = new Rectangle(0, 0, 0, 0);
        [Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public static Rectangle Empty
        {
            get { return mEmpty; }
        }

        public Rectangle(int x, int y, int width, int height)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }

        public Rectangle(Point pt, Size size)
        {
            X = pt.X;
            Y = pt.Y;
            Width = size.Width;
            Height = size.Height;
        }

        [Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public bool Contains(Point pt)
        {
            if ((pt.X >= X) && (pt.Y >= Y) && (pt.X < Right) && (pt.Y < Bottom))
                return true;

            return false;
        }
        [Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public bool Contains(int x, int y)
        {
            if ((x >= X) && (y >= Y) && (x < Right) && (y < Bottom))
                return true;

            return false;
        }

        [Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public static Rectangle Intersect(Rectangle a, Rectangle b)
        {
            if (a.Left > b.Right || a.Top > b.Bottom || a.Right < b.Left || a.Bottom < b.Top)
                return Rectangle.Empty;

            var left = System.Math.Max(a.Left, b.Left);
            var top = System.Math.Max(a.Top, b.Top);
            var right = System.Math.Min(a.Right, b.Right);
            var bottom = System.Math.Min(a.Bottom, b.Bottom);

            return new Rectangle(left, top, right - left, bottom - top);
        }
    }
}
