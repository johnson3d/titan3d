using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace EngineNS
{
    public struct DBoundingBox2D : System.IEquatable<DBoundingBox2D>
    {
        public static readonly DBoundingBox2D Empty = new DBoundingBox2D(new DVector2(double.MaxValue), new DVector2(double.MinValue));
        /// <summary>
        /// 最小顶点
        /// </summary>
        public DVector2 Minimum;
        /// <summary>
        /// 最大顶点
        /// </summary>
        public DVector2 Maximum;
        public DBoundingBox2D(double minX, double minY, double maxX, double maxY)
        {
            Minimum.X = minX;
            Minimum.Y = minY;
            Maximum.X = maxX;
            Maximum.Y = maxY;
        }
        public DBoundingBox2D(double extentX, double extentY)
        {
            Minimum.X = extentX * -0.5f;
            Minimum.Y = extentY * -0.5f;
            Maximum.X = extentX * 0.5f;
            Maximum.Y = extentY * 0.5f;
        }
        public DBoundingBox2D(DVector2 center, double extent = 1.0f)
        {
            Minimum = center - DVector2.One * extent * 0.5f;
            Maximum = center + DVector2.One * extent * 0.5f;
        }
        /// <summary>
        /// 带参构造函数
        /// </summary>
        /// <param name="minimum">最小顶点</param>
        /// <param name="maximum">最大顶点</param>
        public DBoundingBox2D(DVector2 minimum, DVector2 maximum)
        {
            Minimum = minimum;
            Maximum = maximum;
        }
        /// <summary>
        /// 初始化空的包围盒
        /// </summary>
        /// <param name="minimum">最小顶点指针</param>
        /// <param name="maximum">最大顶点指针</param>
        public void InitEmptyBox(IntPtr minimum, IntPtr maximum)
        {
            Minimum = (DVector2)System.Runtime.InteropServices.Marshal.PtrToStructure(minimum, typeof(DVector2));
            Maximum = (DVector2)System.Runtime.InteropServices.Marshal.PtrToStructure(maximum, typeof(DVector2));
        }
        /// <summary>
        /// 初始化空的包围盒
        /// </summary>
        public void InitEmptyBox()
        {
            Minimum = new DVector2(double.MaxValue);
            Maximum = new DVector2(-double.MaxValue);
        }
        [Rtti.Meta]
        public static DBoundingBox2D EmptyBox()
        {
            var bb = new DBoundingBox2D();
            bb.InitEmptyBox();
            return bb;
        }

        [Rtti.Meta]
        public bool IsEmpty()
        {
            if (Minimum.X >= Maximum.X ||
               Minimum.Y >= Maximum.Y)
                return true;
            return false;
        }
        /// <summary>
        /// 获取包围盒的顶点列表
        /// </summary>
        /// <returns>返回包围盒的顶点列表</returns>
        /// y  z
        /// | /
        /// |/
        /// -----x
        /// 3------2
        /// |      | 
        /// |      |
        /// 0------1
        public unsafe void UnsafeGetCorners(DVector2* results)
        {
            SetVector2Value(ref results[0], Minimum.X, Minimum.Y);
            SetVector2Value(ref results[1], Maximum.X, Minimum.Y);
            SetVector2Value(ref results[2], Maximum.X, Maximum.Y);
            SetVector2Value(ref results[3], Minimum.X, Maximum.Y);
        }
        public DVector2 GetCorner(int index)
        {
            DVector2 result = new DVector2();
            switch (index)
            {
                case 0:
                    SetVector2Value(ref result, Minimum.X, Minimum.Y);
                    break;
                case 1:
                    SetVector2Value(ref result, Minimum.X, Minimum.Y);
                    break;
                case 2:
                    SetVector2Value(ref result, Maximum.X, Maximum.Y);
                    break;
                case 3:
                    SetVector2Value(ref result, Minimum.X, Maximum.Y);
                    break;
            }
            return result;
        }
        public static void CalculateClosestPointInBox(in DVector2 point, in DBoundingBox2D AABB, out DVector2 outPoint, out double outSqrDistance)
        {
            // compute coordinates of point in box coordinate system
            var closest = point - AABB.GetCenter();

            var halfSize = AABB.GetSize() * 0.5f;
            // project test point onto box
            double fSqrDistance = 0.0f;
            double fDelta;

            for (int i = 0; i < 3; i++)
            {
                if (closest[i] < -halfSize[i])
                {
                    fDelta = closest[i] + halfSize[i];
                    fSqrDistance += fDelta * fDelta;
                    closest[i] = -halfSize[i];
                }
                else if (closest[i] > halfSize[i])
                {
                    fDelta = closest[i] - halfSize[i];
                    fSqrDistance += fDelta * fDelta;
                    closest[i] = halfSize[i];
                }
            }

            // Inside
            if (fSqrDistance == 0.0F)
            {
                outPoint = point;
                outSqrDistance = 0.0F;
            }
            // Outside
            else
            {
                outPoint = closest + AABB.GetCenter();
                outSqrDistance = fSqrDistance;
            }
        }
        public DVector2 ClosestPoint(in DVector2 pos)
        {
            DVector2 result;
            CalculateClosestPointInBox(in pos, in this, out result, out var sqrDist);
            return result;
        }
        public unsafe DVector2[] GetCorners()
        {
            DVector2[] results = new DVector2[8];
            fixed (DVector2* p = &results[0])
            {
                UnsafeGetCorners(p);
            }
            return results;
        }
        public unsafe void GetCornersUnsafe(DVector2* verts)
        {
            SetVector2Value(ref verts[0], Minimum.X, Minimum.Y);
            SetVector2Value(ref verts[1], Maximum.X, Minimum.Y);
            SetVector2Value(ref verts[2], Maximum.X, Maximum.Y);
            SetVector2Value(ref verts[3], Minimum.X, Maximum.Y);
        }
        private static void SetVector2Value(ref DVector2 v3, double x, double y)
        {
            v3.X = x;
            v3.Y = y;
        }
        /// <summary>
        /// 获取包围盒的中心点
        /// </summary>
        /// <returns>返回包围盒的中心点</returns>
        [Rtti.Meta]
        public DVector2 GetCenter()
        {
            return (Maximum + Minimum) * 0.5f;
        }
        public DVector2 GetExtent()
        {
            return (Maximum - Minimum) * 0.5f;
        }
        public DVector2 GetSize()
        {
            return Maximum - Minimum;
        }
        public void Expand(in DVector2 size)
        {
            var oriSize = GetSize();
            var center = GetCenter();
            oriSize = DVector2.Maximize(in oriSize, size);
            Minimum = center - oriSize;
            Maximum = center + oriSize;
        }
        public static DBoundingBox2D ExpandBy(DBoundingBox2D box, in DVector2 size)
        {
            return new DBoundingBox2D(box.Minimum - size, box.Maximum + size);
        }
        public void SetSize(in DVector2 size)
        {
            var center = GetCenter();
            Minimum = center - size;
            Maximum = center + size;
        }
        public double GetVolume()
        {
            var sz = GetSize();
            return sz.X * sz.Y;
        }
        public double GetMaxSide()
        {
            var sz = GetSize();
            if (sz.X >= sz.Y)
            {
                return sz.X;
            }
            else
            {
                return sz.Y;
            }
        }
        [Rtti.Meta]
        public ContainmentType Contains(in DBoundingBox2D box)
        {
            return Contains(in this, in box);
        }
        [Rtti.Meta]
        public static ContainmentType Contains(in DBoundingBox2D box1, in DBoundingBox2D box2)
        {
            if (box1.Maximum.X < box2.Minimum.X || box1.Minimum.X > box2.Maximum.X)
                return ContainmentType.Disjoint;

            if (box1.Maximum.Y < box2.Minimum.Y || box1.Minimum.Y > box2.Maximum.Y)
                return ContainmentType.Disjoint;

            if (box1.Minimum.X <= box2.Minimum.X && box2.Maximum.X <= box1.Maximum.X && box1.Minimum.Y <= box2.Minimum.Y &&
                box2.Maximum.Y <= box1.Maximum.Y)
                return ContainmentType.Contains;

            return ContainmentType.Intersects;
        }


        [Rtti.Meta]
        public ContainmentType Contains(in DVector2 vector)
        {
            return Contains(in this, in vector);
        }
        [Rtti.Meta]
        public static ContainmentType Contains(in DBoundingBox2D box, in DVector2 vector)
        {
            if (box.Minimum.X <= vector.X && vector.X <= box.Maximum.X && box.Minimum.Y <= vector.Y &&
                vector.Y <= box.Maximum.Y)
                return ContainmentType.Contains;

            return ContainmentType.Disjoint;
        }
        /// <summary>
        /// 根据点创建包围盒对象
        /// </summary>
        /// <param name="points">点列表</param>
        /// <returns>返回创建的包围盒</returns>
        public static unsafe DBoundingBox2D FromPoints(DVector2[] points)
        {
            fixed (DVector2* p = &points[0])
            {
                return FromPoints(p, points.Length);
            }
        }
        public static unsafe DBoundingBox2D FromPoints(DVector2* points, int count)
        {
            if (points == null || count <= 0)
                throw new ArgumentNullException("points");

            DBoundingBox2D result;
            DVector2 min;
            result.Minimum.X = double.MaxValue;
            result.Minimum.Y = double.MaxValue;

            DVector2 max;
            result.Maximum.X = double.MinValue;
            result.Maximum.Y = double.MinValue;

            for (int i = 0; i < count; i++)
            {
                ref DVector2 vector = ref points[i];
                DVector2.Minimize(in result.Minimum, in vector, out min);
                result.Minimum = min;
                DVector2.Maximize(in result.Maximum, in vector, out max);
                result.Maximum = max;
            }

            return result;
        }

        [Rtti.Meta]
        public void Merge(in DVector2 pos)
        {
            DVector2.Minimize(in Minimum, in pos, out Minimum);
            DVector2.Maximize(in Maximum, in pos, out Maximum);
        }
        public void Merge(in DBoundingBox2D box)
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
                Minimum = DVector2.Minimize(in Minimum, in box.Minimum);
                Maximum = DVector2.Maximize(in Maximum, in box.Maximum);
            }
        }
        [Rtti.Meta]
        public static DBoundingBox2D Merge(in DBoundingBox2D box1, in DBoundingBox2D box2)
        {
            if (box1.IsEmpty())
                return box2;
            else if (box2.IsEmpty())
                return box1;
            else
            {
                DBoundingBox2D box;
                DVector2.Minimize(in box1.Minimum, in box2.Minimum, out box.Minimum);
                DVector2.Maximize(in box1.Maximum, in box2.Maximum, out box.Maximum);
                return box;
            }
        }
        [Rtti.Meta]
        public static void Merge(in DBoundingBox2D box1, in DBoundingBox2D box2, out DBoundingBox2D box)
        {
            if (box1.IsEmpty())
            {
                box = box2;
            }
            else if (box2.IsEmpty())
            {
                box = box1;
            }
            else
            {
                DVector2.Minimize(in box1.Minimum, in box2.Minimum, out box.Minimum);
                DVector2.Maximize(in box1.Maximum, in box2.Maximum, out box.Maximum);
            }
        }
        [Rtti.Meta]
        public void Merge2(in DBoundingBox2D box, out DBoundingBox2D outBox)
        {
            if (box.IsEmpty())
            {
                outBox = this;
                return;
            }
            DVector2.Minimize(in this.Minimum, in box.Minimum, out outBox.Minimum);
            DVector2.Maximize(in this.Maximum, in box.Maximum, out outBox.Maximum);
        }
        [Rtti.Meta]
        public static DBoundingBox2D Merge(in DBoundingBox2D box, in DVector2 point)
        {
            DBoundingBox2D retBox;
            DVector2.Minimize(in box.Minimum, in point, out retBox.Minimum);
            DVector2.Maximize(in box.Maximum, in point, out retBox.Maximum);
            return retBox;
        }
        public static void And(in DBoundingBox2D a, in DBoundingBox2D b, out DBoundingBox2D box)
        {
            DVector2.Maximize(in a.Minimum, in b.Minimum, out box.Minimum);
            DVector2.Minimize(in a.Maximum, in b.Maximum, out box.Maximum);
        }
        /// <summary>
        /// 判断两个包围盒是否相交
        /// </summary>
        /// <param name="box1">包围盒对象1</param>
        /// <param name="box2">包围盒对象2</param>
        /// <returns>如果相交返回true，否则返回false</returns>
        [Rtti.Meta]
        public static bool Intersects(DBoundingBox2D box1, DBoundingBox2D box2)
        {
            return Intersects(in box1, in box2);
        }
        public static bool Intersects(in DBoundingBox2D box1, in DBoundingBox2D box2)
        {
            if (box1.Maximum.X < box2.Minimum.X || box1.Minimum.X > box2.Maximum.X)
                return false;

            return (box1.Maximum.Y >= box2.Minimum.Y && box1.Minimum.Y <= box2.Maximum.Y);
        }
        /// <summary>
        /// 判断包围盒与球体是否相交
        /// </summary>
        /// <param name="box">包围盒对象</param>
        /// <param name="sphere">球体对象</param>
        /// <returns>如果相交返回true，否则返回false</returns>
        [Rtti.Meta]
        public static bool Intersects(in DBoundingBox2D box, in DBoundingSphere2D sphere)
        {
            DVector2 clamped;

            DVector2.Clamp(in sphere.Center, in box.Minimum, in box.Maximum, out clamped);

            double x = sphere.Center.X - clamped.X;
            double y = sphere.Center.Y - clamped.Y;
            
            double dist = (x * x) + (y * y);
            var ret = (dist <= (sphere.Radius * sphere.Radius));
            return ret;
        }
        public static bool Intersects(in DBoundingBox2D box, in DBoundingSphere2D sphere, out double dist)
        {
            DVector2 clamped;

            DVector2.Clamp(in sphere.Center, in box.Minimum, in box.Maximum, out clamped);

            double x = sphere.Center.X - clamped.X;
            double y = sphere.Center.Y - clamped.Y;
            
            dist = (x * x) + (y * y);
            var ret = (dist <= (sphere.Radius * sphere.Radius));
            dist = DVector2.DistanceSquared(sphere.Center, box.GetCenter());

            return ret;
        }
        /// <summary>
        /// 重载操作符
        /// </summary>
        /// <param name="left">包围盒对象</param>
        /// <param name="right">包围盒对象</param>
        /// <returns>如果相等返回true，否则返回false</returns>
        public static bool operator ==(DBoundingBox2D left, DBoundingBox2D right)
        {
            return Equals(left, right);
        }
        /// <summary>
        /// 重载操作符！=
        /// </summary>
        /// <param name="left">包围盒对象</param>
        /// <param name="right">包围盒对象</param>
        /// <returns>如果不相等返回true，否则返回false</returns>
        public static bool operator !=(DBoundingBox2D left, DBoundingBox2D right)
        {
            return !Equals(left, right);
        }
        /// <summary>
        /// 转换成string类型
        /// </summary>
        /// <returns>返回转换成的string</returns>
        public override String ToString()
        {
            return String.Format(CultureInfo.CurrentCulture, "Minimum:{0} Maximum:{1}", Minimum.ToString(), Maximum.ToString());
        }
        /// <summary>
        /// 获取哈希值
        /// </summary>
        /// <returns>返回相应的哈希值</returns>
        public override int GetHashCode()
        {
            return Minimum.GetHashCode() + Maximum.GetHashCode();
        }
        /// <summary>
        /// 对象实例是否相同
        /// </summary>
        /// <param name="value">对象实例</param>
        /// <returns>如果相等返回true，否则返回false</returns>
        public override bool Equals(object value)
        {
            if (value == null)
                return false;

            if (value.GetType() != GetType())
                return false;

            return Equals((DBoundingBox2D)(value));
        }
        /// <summary>
        /// 包围盒对象是否相同
        /// </summary>
        /// <param name="value">包围盒对象实例</param>
        /// <returns>如果相同返回true，否则返回false</returns>
        public bool Equals(DBoundingBox2D value)
        {
            return (Minimum == value.Minimum && Maximum == value.Maximum);
        }
        /// <summary>
        /// 判断两个包围盒对象是否相同
        /// </summary>
        /// <param name="value1">包围盒对象1</param>
        /// <param name="value2">包围盒对象2</param>
        /// <returns>如果相同返回true，否则返回false</returns>
        public static bool Equals(in DBoundingBox2D value1, in DBoundingBox2D value2)
        {
            return (value1.Minimum == value2.Minimum && value1.Maximum == value2.Maximum);
        }
    }
}
