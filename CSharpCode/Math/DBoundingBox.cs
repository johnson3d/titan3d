using System;
using System.Collections.Generic;
using System.Text;
using System.Globalization;
using System.Xml.Schema;

namespace EngineNS
{
    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
    public struct DBoundingBox : System.IEquatable<DBoundingBox>
    {
        public DVector3 Minimum;
        public DVector3 Maximum;
        public static readonly DBoundingBox Empty = new DBoundingBox(DVector3.Zero, float.MinValue);
        public DBoundingBox(in DVector3 minimum, in DVector3 maximum)
        {
            Minimum = minimum;
            Maximum = maximum;
        }
        public DBoundingBox(in DVector3 center, double extent = 1.0)
        {
            Minimum = center - DVector3.UnitXYZ * extent * 0.5;
            Maximum = center + DVector3.UnitXYZ * extent * 0.5;
        }
        public DBoundingBox(in BoundingBox box)
        {
            Minimum.X = box.Minimum.X;
            Minimum.Y = box.Minimum.Y;
            Minimum.Z = box.Minimum.Z;

            Maximum.X = box.Maximum.X;
            Maximum.Y = box.Maximum.Y;
            Maximum.Z = box.Maximum.Z;
        }
        public void FromSingle(in BoundingBox box)
        {
            Minimum.X = box.Minimum.X;
            Minimum.Y = box.Minimum.Y;
            Minimum.Z = box.Minimum.Z;

            Maximum.X = box.Maximum.X;
            Maximum.Y = box.Maximum.Y;
            Maximum.Z = box.Maximum.Z;
        }
        public static void OffsetToSingleBox(in DVector3 offset, in DBoundingBox src, out BoundingBox aabb)
        {
            aabb.Minimum = (src.Minimum - offset).ToSingleVector3();
            aabb.Maximum = (src.Maximum - offset).ToSingleVector3();
        }
        public DBoxCAE AsBoxCAE()
        {
            DBoxCAE result;
            result.Center = (Minimum + Maximum) * 0.5;
            result.Extent = (Minimum - Maximum).AbsVector().ToSingleVector3();
            return result;
        }
        public BoundingBox ToSingleAABB()
        {
            BoundingBox result;
            result.Minimum.X = (float)Minimum.X;
            result.Minimum.Y = (float)Minimum.Y;
            result.Minimum.Z = (float)Minimum.Z;

            result.Maximum.X = (float)Maximum.X;
            result.Maximum.Y = (float)Maximum.Y;
            result.Maximum.Z = (float)Maximum.Z;
            return result;
        }
        public static bool operator ==(DBoundingBox left, DBoundingBox right)
        {
            return Equals(left, right);
        }
        public static bool operator !=(DBoundingBox left, DBoundingBox right)
        {
            return !Equals(left, right);
        }
        public override int GetHashCode()
        {
            return Minimum.GetHashCode() + Maximum.GetHashCode();
        }
        public override bool Equals(object value)
        {
            if (value == null)
                return false;

            if (value.GetType() != GetType())
                return false;

            return Equals((DBoundingBox)(value));
        }
        public bool Equals(DBoundingBox value)
        {
            return (Minimum == value.Minimum && Maximum == value.Maximum);
        }
        public override String ToString()
        {
            return String.Format(CultureInfo.CurrentCulture, "Minimum:{0} Maximum:{1}", Minimum.ToString(), Maximum.ToString());
        }
        public void InitEmptyBox()
        {
            Minimum = new DVector3(double.MaxValue);
            Maximum = new DVector3(-double.MaxValue);
        }
        public bool IsEmpty()
        {
            if (Minimum.X >= Maximum.X ||
               Minimum.Y >= Maximum.Y ||
               Minimum.Z >= Maximum.Z)
                return true;
            return false;
        }
        public static DBoundingBox EmptyBox()
        {
            var bb = new DBoundingBox();
            bb.InitEmptyBox();
            return bb;
        }
        private static void SetVector3Value(ref DVector3 v3, double x, double y, double z)
        {
            v3.X = x;
            v3.Y = y;
            v3.Z = z;
        }
        public DVector3[] GetCorners()
        {
            var results = new DVector3[8];
            SetVector3Value(ref results[0], Minimum.X, Maximum.Y, Maximum.Z);
            SetVector3Value(ref results[1], Maximum.X, Maximum.Y, Maximum.Z);
            SetVector3Value(ref results[2], Maximum.X, Minimum.Y, Maximum.Z);
            SetVector3Value(ref results[3], Minimum.X, Minimum.Y, Maximum.Z);
            SetVector3Value(ref results[4], Minimum.X, Maximum.Y, Minimum.Z);
            SetVector3Value(ref results[5], Maximum.X, Maximum.Y, Minimum.Z);
            SetVector3Value(ref results[6], Maximum.X, Minimum.Y, Minimum.Z);
            SetVector3Value(ref results[7], Minimum.X, Minimum.Y, Minimum.Z);
            return results;
        }
        public unsafe void UnsafeGetCorners(DVector3* verts)
        {
            SetVector3Value(ref verts[0], Minimum.X, Maximum.Y, Maximum.Z);
            SetVector3Value(ref verts[1], Maximum.X, Maximum.Y, Maximum.Z);
            SetVector3Value(ref verts[2], Maximum.X, Minimum.Y, Maximum.Z);
            SetVector3Value(ref verts[3], Minimum.X, Minimum.Y, Maximum.Z);
            SetVector3Value(ref verts[4], Minimum.X, Maximum.Y, Minimum.Z);
            SetVector3Value(ref verts[5], Maximum.X, Maximum.Y, Minimum.Z);
            SetVector3Value(ref verts[6], Maximum.X, Minimum.Y, Minimum.Z);
            SetVector3Value(ref verts[7], Minimum.X, Minimum.Y, Minimum.Z);
        }
        public DVector3 GetCenter()
        {
            return (Maximum + Minimum) * 0.5;
        }
        public DVector3 GetSize()
        {
            return Maximum - Minimum;
        }
        public double GetVolume()
        {
            var sz = GetSize();
            return sz.X * sz.Y * sz.Z;
        }
        public double GetMaxSide()
        {
            var sz = GetSize();
            return Math.Max(sz.X, Math.Max(sz.Y, sz.Z));
        }
        public static ContainmentType Contains(in DBoundingBox box1, in DBoundingBox box2)
        {
            if (box1.Maximum.X < box2.Minimum.X || box1.Minimum.X > box2.Maximum.X)
                return ContainmentType.Disjoint;

            if (box1.Maximum.Y < box2.Minimum.Y || box1.Minimum.Y > box2.Maximum.Y)
                return ContainmentType.Disjoint;

            if (box1.Maximum.Z < box2.Minimum.Z || box1.Minimum.Z > box2.Maximum.Z)
                return ContainmentType.Disjoint;

            if (box1.Minimum.X <= box2.Minimum.X && box2.Maximum.X <= box1.Maximum.X && box1.Minimum.Y <= box2.Minimum.Y &&
                box2.Maximum.Y <= box1.Maximum.Y && box1.Minimum.Z <= box2.Minimum.Z && box2.Maximum.Z <= box1.Maximum.Z)
                return ContainmentType.Contains;

            return ContainmentType.Intersects;
        }
        public static ContainmentType Contains(in DBoundingBox box, in DVector3 vector)
        {
            if (box.Minimum.X <= vector.X && vector.X <= box.Maximum.X && box.Minimum.Y <= vector.Y &&
                vector.Y <= box.Maximum.Y && box.Minimum.Z <= vector.Z && vector.Z <= box.Maximum.Z)
                return ContainmentType.Contains;

            return ContainmentType.Disjoint;
        }
        public void Merge(in DVector3 pos)
        {
            Minimum = DVector3.Minimize(in Minimum, in pos);
            Maximum = DVector3.Maximize(in Maximum, in pos);
        }
        public void Merge(in DBoundingBox box)
        {
            if (IsEmpty())
            {
                Minimum = box.Minimum;
                Maximum = box.Maximum;
            }
            else if (box.IsEmpty())
                return;
            else
            {
                Minimum = DVector3.Minimize(in Minimum, in box.Minimum);
                Maximum = DVector3.Maximize(in Maximum, in box.Maximum);
            }
        }
        public static DBoundingBox Merge(in DBoundingBox box1, in DBoundingBox box2)
        {
            if (box1.IsEmpty())
                return box2;
            else if (box2.IsEmpty())
                return box1;
            else
            {
                DBoundingBox box;
                box.Minimum = DVector3.Minimize(in box1.Minimum, in box2.Minimum);
                box.Maximum = DVector3.Maximize(in box1.Maximum, in box2.Maximum);
                return box;
            }
        }
        public static void And(in DBoundingBox a, in DBoundingBox b, out DBoundingBox box)
        {
            box.Minimum = DVector3.Maximize(in a.Minimum, in b.Minimum);
            box.Maximum = DVector3.Minimize(in a.Maximum, in b.Maximum);
        }
        public static bool Intersects(in DBoundingBox box1, in DBoundingBox box2)
        {
            if (box1.Maximum.X < box2.Minimum.X || box1.Minimum.X > box2.Maximum.X)
                return false;

            if (box1.Maximum.Y < box2.Minimum.Y || box1.Minimum.Y > box2.Maximum.Y)
                return false;

            return (box1.Maximum.Z >= box2.Minimum.Z && box1.Minimum.Z <= box2.Maximum.Z);
        }
        public static DBoundingBox IntersectBox(in DBoundingBox box1, in DBoundingBox box2)
        {
            DBoundingBox result;
            result.Maximum = DVector3.Minimize(box1.Maximum, box2.Maximum);
            result.Minimum = DVector3.Maximize(box1.Minimum, box2.Minimum);
            return result;
        }
        public static void Transform(in DBoundingBox srcBox, in FTransform transform, out DBoundingBox result)
        {
            result.Minimum = new DVector3(double.MaxValue);
            result.Maximum = new DVector3(-double.MaxValue);
            var corners = srcBox.GetCorners();
            for (int i = 1; i < corners.Length; i++)
            {
                corners[i] = transform.TransformPosition(in corners[i]);
                ref var v = ref corners[i];
                result.Minimum = DVector3.Minimize(in result.Minimum, in v);
                result.Maximum = DVector3.Maximize(in result.Maximum, in v);
            }
        }
        public static DBoundingBox TransformNoScale(in DBoundingBox srcBox, in FTransform transform)
        {
            DBoundingBox result;
            TransformNoScale(in srcBox, transform, out result);
            return result;
        }
        public static void TransformNoScale(in DBoundingBox srcBox, in FTransform transform, out DBoundingBox result)
        {
            result.Minimum = new DVector3(double.MaxValue);
            result.Maximum = new DVector3(-double.MaxValue);
            var corners = srcBox.GetCorners();
            for (int i = 1; i < corners.Length; i++)
            {
                corners[i] = transform.TransformPositionNoScale(in corners[i]);
                ref var v = ref corners[i];
                result.Minimum = DVector3.Minimize(in result.Minimum, in v);
                result.Maximum = DVector3.Maximize(in result.Maximum, in v);
            }
        }
        public static bool Intersects(in DBoundingBox box, in DBoundingSphere sphere)
        {
            DVector3 clamped;

            DVector3.Clamp(in sphere.Center, in box.Minimum, in box.Maximum, out clamped);

            double x = sphere.Center.X - clamped.X;
            double y = sphere.Center.Y - clamped.Y;
            double z = sphere.Center.Z - clamped.Z;

            double dist = (x * x) + (y * y) + (z * z);
            var ret = (dist <= (sphere.Radius * sphere.Radius));
            return ret;
        }
    }
    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
    public struct DBoxCAE : System.IEquatable<DBoxCAE>
    {
        public DVector3 Center;
        public Vector3 Extent;
        public DBoundingBox AsAABB()
        {
            DBoundingBox result;
            result.Minimum = Center - Extent;
            result.Maximum = Center + Extent;
            return result;
        }
        public DVector3 Minimum
        {
            get
            {
                return Center - Extent;
            }
        }
        public DVector3 Maximum
        {
            get
            {
                return Center + Extent;
            }
        }
        public Vector3 GetSize()
        {
            return Extent * 2.0f;
        }
        public float GetVolume()
        {
            return Extent.X * Extent.Y * Extent.Z * 8.0f;
        }
        public float GetMaxSide()
        {
            ref var sz = ref Extent;
            if (sz.X >= sz.Y)
            {
                if (sz.X >= sz.Z)
                    return sz.X * 2.0f;
                else
                    return sz.Z * 2.0f;
            }
            else
            {
                if (sz.Y >= sz.Z)
                    return sz.Y * 2.0f;
                else
                    return sz.Z * 2.0f;
            }
        }
        public DBoxCAE(in DVector3 center, in Vector3 extend)
        {
            Center = center;
            Extent = extend;
        }
        public DBoxCAE(in DVector3 minimum, in DVector3 maximum)
        {
            Center = (minimum + maximum) * 0.5;
            Extent = (minimum - maximum).ToSingleVector3();
        }
        public void InitEmptyBox()
        {
            Center = DVector3.Zero;
            Extent = new Vector3(-1);
        }
        public bool IsEmpty()
        {
            if (Extent.X <= 0 ||
               Extent.Y >= 0 ||
               Extent.Z >= 0)
                return true;
            return false;
        }
        public bool Equals(DBoxCAE value)
        {
            return (Center == value.Center && Extent == value.Extent);
        }
        public override bool Equals(object value)
        {
            if (value == null)
                return false;

            if (value.GetType() != GetType())
                return false;

            return Equals((DBoxCAE)(value));
        }
        public override int GetHashCode()
        {
            return Center.GetHashCode() + Extent.GetHashCode();
        }
        public override String ToString()
        {
            return String.Format(CultureInfo.CurrentCulture, "Center:{0} Extend:{1}", Center.ToString(), Extent.ToString());
        }
        public static bool operator ==(in DBoxCAE left, in DBoxCAE right)
        {
            return Equals(left, right);
        }
        public static bool operator !=(in DBoxCAE left, in DBoxCAE right)
        {
            return !Equals(left, right);
        }
        private static void SetVector3Value(ref DVector3 v3, double x, double y, double z)
        {
            v3.X = x;
            v3.Y = y;
            v3.Z = z;
        }
        public DVector3[] GetCorners()
        {
            var tMin = Minimum;
            var tMax = Maximum;
            var results = new DVector3[8];
            SetVector3Value(ref results[0], tMin.X, tMax.Y, tMax.Z);
            SetVector3Value(ref results[1], tMax.X, tMax.Y, tMax.Z);
            SetVector3Value(ref results[2], tMax.X, tMin.Y, tMax.Z);
            SetVector3Value(ref results[3], tMin.X, tMin.Y, tMax.Z);
            SetVector3Value(ref results[4], tMin.X, tMax.Y, tMin.Z);
            SetVector3Value(ref results[5], tMax.X, tMax.Y, tMin.Z);
            SetVector3Value(ref results[6], tMax.X, tMin.Y, tMin.Z);
            SetVector3Value(ref results[7], tMin.X, tMin.Y, tMin.Z);
            return results;
        }
        public static ContainmentType Contains(in DBoxCAE box1, in DBoxCAE box2)
        {
            var distAbs = (box1.Center - box2.Center).AbsVector();
            var extend = box1.Extent + box2.Extent;
            
            if(distAbs.X > extend.X)
                return ContainmentType.Disjoint;

            if (distAbs.Y > extend.Y)
                return ContainmentType.Disjoint;

            if (distAbs.Z > extend.Z)
                return ContainmentType.Disjoint;

            var mv = distAbs + box1.Extent;
            if (mv.X < box2.Extent.X && mv.Y < box2.Extent.Y && mv.Z < box2.Extent.Z)
                return ContainmentType.Contains;
            mv = distAbs + box2.Extent;
            if (mv.X < box1.Extent.X && mv.Y < box1.Extent.Y && mv.Z < box1.Extent.Z)
                return ContainmentType.Contains;

            return ContainmentType.Intersects;

            //if (box1.Maximum.X < box2.Minimum.X || box1.Minimum.X > box2.Maximum.X)
            //    return ContainmentType.Disjoint;

            //if (box1.Maximum.Y < box2.Minimum.Y || box1.Minimum.Y > box2.Maximum.Y)
            //    return ContainmentType.Disjoint;

            //if (box1.Maximum.Z < box2.Minimum.Z || box1.Minimum.Z > box2.Maximum.Z)
            //    return ContainmentType.Disjoint;

            //if (box1.Minimum.X <= box2.Minimum.X && box2.Maximum.X <= box1.Maximum.X && box1.Minimum.Y <= box2.Minimum.Y &&
            //    box2.Maximum.Y <= box1.Maximum.Y && box1.Minimum.Z <= box2.Minimum.Z && box2.Maximum.Z <= box1.Maximum.Z)
            //    return ContainmentType.Contains;

            //return ContainmentType.Intersects;
        }
    }
}
