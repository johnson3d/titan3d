using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS
{
    public struct SizeF
    {
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

        static SizeF mEmpty = new SizeF(0.0f, 0.0f);
        [Rtti.Meta]
        public static SizeF Empty
        {
            get { return mEmpty; }
        }
        static SizeF sInfinity = new SizeF(float.PositiveInfinity, float.PositiveInfinity);
        public static SizeF Infinity => sInfinity;

        public SizeF(float width, float height)
        {
            Width = width;
            Height = height;
        }

        public static bool operator ==(SizeF left, SizeF right)
        {
            return ((left.Width == right.Width) && (left.Height == right.Height));
        }
        public static bool operator !=(SizeF left, SizeF right)
        {
            return ((left.Width != right.Width) || (left.Height != right.Height));
        }

        public override bool Equals(object obj)
        {
            var size = (SizeF)obj;
            return ((Math.Abs(this.Width - size.Width) < EngineNS.MathHelper.Epsilon) &&
                    (Math.Abs(this.Height - size.Height) < EngineNS.MathHelper.Epsilon));
        }
        public bool Equals(in SizeF size)
        {
            return ((Math.Abs(this.Width - size.Width) < EngineNS.MathHelper.Epsilon) &&
                    (Math.Abs(this.Height - size.Height) < EngineNS.MathHelper.Epsilon));
        }

        public override int GetHashCode()
        {
            return (Width.ToString() + Height.ToString()).GetHashCode();
        }
    }
}
