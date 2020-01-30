using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS
{
    public struct Point
    {
        [Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable | Editor.MacrossMemberAttribute.enMacrossType.NotShowInBreak)]
        public bool IsEmpty
        {
            get
            {
                if (X == 0 && Y == 0)
                    return true;

                return false;
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

        static Point mEmpty = new Point(0, 0);

        [Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable | Editor.MacrossMemberAttribute.enMacrossType.NotShowInBreak)]
        public static Point Empty
        {
            get { return mEmpty; }
        }

        public Point(int x, int y)
        {
            X = x;
            Y = y;
        }

        public static Point operator -(Point left, Point right)
        {
            return new Point(left.X - right.X, left.Y - right.Y);
        }
        public static Point operator +(Point left, Point right)
        {
            return new Point(left.X + right.X, left.Y + right.Y);
        }
        public static Point operator * (Point vec, float scale)
        {
            return new Point((int)(vec.X * scale), (int)(vec.Y * scale));
        }
        public static Point operator * (float scale, Point vec)
        {
            return vec * scale;
        }
        [Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public int Length
        {
            get { return (int)System.Math.Sqrt((X * X) + (Y * Y)); }
        }
    }
}
