using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace EngineNS
{
    public struct BoundingBox2D : System.IEquatable<BoundingBox2D>
    {
        public static readonly BoundingBox2D Empty = new BoundingBox2D(new Vector2(float.MaxValue), new Vector2(float.MinValue));
        /// <summary>
        /// 最小顶点
        /// </summary>
        public Vector2 Minimum;
        /// <summary>
        /// 最大顶点
        /// </summary>
        public Vector2 Maximum;
        public BoundingBox2D(float minX, float minY, float maxX, float maxY)
        {
            Minimum.X = minX;
            Minimum.Y = minY;
            Maximum.X = maxX;
            Maximum.Y = maxY;
        }
        public BoundingBox2D(float extentX, float extentY)
        {
            Minimum.X = extentX * -0.5f;
            Minimum.Y = extentY * -0.5f;
            Maximum.X = extentX * 0.5f;
            Maximum.Y = extentY * 0.5f;
        }
        public BoundingBox2D(Vector2 center, float extent = 1.0f)
        {
            Minimum = center - Vector2.One * extent * 0.5f;
            Maximum = center + Vector2.One * extent * 0.5f;
        }
        /// <summary>
        /// 带参构造函数
        /// </summary>
        /// <param name="minimum">最小顶点</param>
        /// <param name="maximum">最大顶点</param>
        public BoundingBox2D(Vector2 minimum, Vector2 maximum)
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
            Minimum = (Vector2)System.Runtime.InteropServices.Marshal.PtrToStructure(minimum, typeof(Vector2));
            Maximum = (Vector2)System.Runtime.InteropServices.Marshal.PtrToStructure(maximum, typeof(Vector2));
        }
        /// <summary>
        /// 初始化空的包围盒
        /// </summary>
        public void InitEmptyBox()
        {
            Minimum = new Vector2(float.MaxValue);
            Maximum = new Vector2(-float.MaxValue);
        }
        [Rtti.Meta("")]
        public static BoundingBox2D EmptyBox()
        {
            var bb = new BoundingBox2D();
            bb.InitEmptyBox();
            return bb;
        }

        [Rtti.Meta("")]
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
        public unsafe void UnsafeGetCorners(Vector2* results)
        {
            SetVector2Value(ref results[0], Minimum.X, Minimum.Y);
            SetVector2Value(ref results[1], Maximum.X, Minimum.Y);
            SetVector2Value(ref results[2], Maximum.X, Maximum.Y);
            SetVector2Value(ref results[3], Minimum.X, Maximum.Y);
        }
        public Vector2 GetCorner(int index)
        {
            Vector2 result = new Vector2();
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
        public static void CalculateClosestPointInBox(in Vector2 point, in BoundingBox2D AABB, out Vector2 outPoint, out float outSqrDistance)
        {
            // compute coordinates of point in box coordinate system
            var closest = point - AABB.GetCenter();

            var halfSize = AABB.GetSize() * 0.5f;
            // project test point onto box
            float fSqrDistance = 0.0f;
            float fDelta;

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
        public Vector2 ClosestPoint(in Vector2 pos)
        {
            Vector2 result;
            CalculateClosestPointInBox(in pos, in this, out result, out var sqrDist);
            return result;
        }
        public unsafe Vector2[] GetCorners()
        {
            Vector2[] results = new Vector2[8];
            fixed (Vector2* p = &results[0])
            {
                UnsafeGetCorners(p);
            }
            return results;
        }
        public DVector2[] GetDCorners()
        {
            DVector2[] results = new DVector2[8];
            Vector2 tmp = new Vector2();
            SetVector2Value(ref tmp, Minimum.X, Minimum.Y);
            results[0] = tmp.AsDVector();
            SetVector2Value(ref tmp, Maximum.X, Minimum.Y);
            results[1] = tmp.AsDVector();
            SetVector2Value(ref tmp, Maximum.X, Maximum.Y);
            results[2] = tmp.AsDVector();
            SetVector2Value(ref tmp, Minimum.X, Maximum.Y);

            return results;
        }
        public unsafe void GetCornersUnsafe(Vector2* verts)
        {
            SetVector2Value(ref verts[0], Minimum.X, Minimum.Y);
            SetVector2Value(ref verts[1], Maximum.X, Minimum.Y);
            SetVector2Value(ref verts[2], Maximum.X, Maximum.Y);
            SetVector2Value(ref verts[3], Minimum.X, Maximum.Y);
        }
        private static void SetVector2Value(ref Vector2 v3, float x, float y)
        {
            v3.X = x;
            v3.Y = y;
        }
        /// <summary>
        /// 获取包围盒的中心点
        /// </summary>
        /// <returns>返回包围盒的中心点</returns>
        [Rtti.Meta("")]
        public Vector2 GetCenter()
        {
            return (Maximum + Minimum) * 0.5f;
        }
        public Vector2 GetExtent()
        {
            return (Maximum - Minimum) * 0.5f;
        }
        public Vector2 GetSize()
        {
            return Maximum - Minimum;
        }
        public void Expand(in Vector2 size)
        {
            var oriSize = GetSize();
            var center = GetCenter();
            oriSize = Vector2.Maximize(in oriSize, size);
            Minimum = center - oriSize;
            Maximum = center + oriSize;
        }
        public static BoundingBox2D ExpandBy(BoundingBox2D box, in Vector2 size)
        {
            return new BoundingBox2D(box.Minimum - size, box.Maximum + size);
        }
        public void SetSize(in Vector2 size)
        {
            var center = GetCenter();
            Minimum = center - size;
            Maximum = center + size;
        }
        public float GetVolume()
        {
            var sz = GetSize();
            return sz.X * sz.Y;
        }
        public float GetMaxSide()
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
        [Rtti.Meta("")]
        public ContainmentType Contains(in BoundingBox2D box)
        {
            return Contains(in this, in box);
        }
        [Rtti.Meta("")]
        public static ContainmentType Contains(in BoundingBox2D box1, in BoundingBox2D box2)
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


        [Rtti.Meta("")]
        public ContainmentType Contains(in Vector2 vector)
        {
            return Contains(in this, in vector);
        }
        [Rtti.Meta("")]
        public static ContainmentType Contains(in BoundingBox2D box, in Vector2 vector)
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
        public static unsafe BoundingBox2D FromPoints(Vector2[] points)
        {
            fixed (Vector2* p = &points[0])
            {
                return FromPoints(p, points.Length);
            }
        }
        public static unsafe BoundingBox2D FromPoints(Vector2* points, int count)
        {
            if (points == null || count <= 0)
                throw new ArgumentNullException("points");

            BoundingBox2D result;
            Vector2 min;
            result.Minimum.X = float.MaxValue;
            result.Minimum.Y = float.MaxValue;

            Vector2 max;
            result.Maximum.X = float.MinValue;
            result.Maximum.Y = float.MinValue;

            for (int i = 0; i < count; i++)
            {
                ref Vector2 vector = ref points[i];
                Vector2.Minimize(in result.Minimum, in vector, out min);
                result.Minimum = min;
                Vector2.Maximize(in result.Maximum, in vector, out max);
                result.Maximum = max;
            }

            return result;
        }

        [Rtti.Meta("")]
        public void Merge(in Vector2 pos)
        {
            Vector2.Minimize(in Minimum, in pos, out Minimum);
            Vector2.Maximize(in Maximum, in pos, out Maximum);
        }
        [Rtti.Meta("")]
        public static BoundingBox2D Merge(in BoundingBox2D box1, in BoundingBox2D box2)
        {
            if (box1.IsEmpty())
                return box2;
            else if (box2.IsEmpty())
                return box1;
            else
            {
                BoundingBox2D box;
                Vector2.Minimize(in box1.Minimum, in box2.Minimum, out box.Minimum);
                Vector2.Maximize(in box1.Maximum, in box2.Maximum, out box.Maximum);
                return box;
            }
        }
        [Rtti.Meta("")]
        public static void Merge(in BoundingBox2D box1, in BoundingBox2D box2, out BoundingBox2D box)
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
                Vector2.Minimize(in box1.Minimum, in box2.Minimum, out box.Minimum);
                Vector2.Maximize(in box1.Maximum, in box2.Maximum, out box.Maximum);
            }
        }
        [Rtti.Meta("")]
        public void Merge2(in BoundingBox2D box, out BoundingBox2D outBox)
        {
            if (box.IsEmpty())
            {
                outBox = this;
                return;
            }
            Vector2.Minimize(in this.Minimum, in box.Minimum, out outBox.Minimum);
            Vector2.Maximize(in this.Maximum, in box.Maximum, out outBox.Maximum);
        }
        [Rtti.Meta("")]
        public static BoundingBox2D Merge(in BoundingBox2D box, in Vector2 point)
        {
            BoundingBox2D retBox;
            Vector2.Minimize(in box.Minimum, in point, out retBox.Minimum);
            Vector2.Maximize(in box.Maximum, in point, out retBox.Maximum);
            return retBox;
        }
        public static void And(in BoundingBox2D a, in BoundingBox2D b, out BoundingBox2D box)
        {
            Vector2.Maximize(in a.Minimum, in b.Minimum, out box.Minimum);
            Vector2.Minimize(in a.Maximum, in b.Maximum, out box.Maximum);
        }
        /// <summary>
        /// 判断两个包围盒是否相交
        /// </summary>
        /// <param name="box1">包围盒对象1</param>
        /// <param name="box2">包围盒对象2</param>
        /// <returns>如果相交返回true，否则返回false</returns>
        [Rtti.Meta("")]
        public static bool Intersects(BoundingBox2D box1, BoundingBox2D box2)
        {
            return Intersects(in box1, in box2);
        }
        public static bool Intersects(in BoundingBox2D box1, in BoundingBox2D box2)
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
        [Rtti.Meta("")]
        public static bool Intersects(in BoundingBox2D box, in BoundingSphere2D sphere)
        {
            Vector2 clamped;

            Vector2.Clamp(in sphere.Center, in box.Minimum, in box.Maximum, out clamped);

            float x = sphere.Center.X - clamped.X;
            float y = sphere.Center.Y - clamped.Y;
            
            float dist = (x * x) + (y * y);
            var ret = (dist <= (sphere.Radius * sphere.Radius));
            return ret;
        }
        public static bool Intersects(in BoundingBox2D box, in BoundingSphere2D sphere, out float dist)
        {
            Vector2 clamped;

            Vector2.Clamp(in sphere.Center, in box.Minimum, in box.Maximum, out clamped);

            float x = sphere.Center.X - clamped.X;
            float y = sphere.Center.Y - clamped.Y;
            
            dist = (x * x) + (y * y);
            var ret = (dist <= (sphere.Radius * sphere.Radius));
            dist = Vector2.DistanceSquared(sphere.Center, box.GetCenter());

            return ret;
        }
        /// <summary>
        /// 重载操作符
        /// </summary>
        /// <param name="left">包围盒对象</param>
        /// <param name="right">包围盒对象</param>
        /// <returns>如果相等返回true，否则返回false</returns>
        public static bool operator ==(BoundingBox2D left, BoundingBox2D right)
        {
            return Equals(left, right);
        }
        /// <summary>
        /// 重载操作符！=
        /// </summary>
        /// <param name="left">包围盒对象</param>
        /// <param name="right">包围盒对象</param>
        /// <returns>如果不相等返回true，否则返回false</returns>
        public static bool operator !=(BoundingBox2D left, BoundingBox2D right)
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

            return Equals((BoundingBox2D)(value));
        }
        /// <summary>
        /// 包围盒对象是否相同
        /// </summary>
        /// <param name="value">包围盒对象实例</param>
        /// <returns>如果相同返回true，否则返回false</returns>
        public bool Equals(BoundingBox2D value)
        {
            return (Minimum == value.Minimum && Maximum == value.Maximum);
        }
        /// <summary>
        /// 判断两个包围盒对象是否相同
        /// </summary>
        /// <param name="value1">包围盒对象1</param>
        /// <param name="value2">包围盒对象2</param>
        /// <returns>如果相同返回true，否则返回false</returns>
        public static bool Equals(in BoundingBox2D value1, in BoundingBox2D value2)
        {
            return (value1.Minimum == value2.Minimum && value1.Maximum == value2.Maximum);
        }
    }
}
