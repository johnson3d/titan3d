using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace EngineNS
{
    public struct BoundingSphere2D : System.IEquatable<BoundingSphere2D>
    {
        public static BoundingSphere2D DefaultSphere = new BoundingSphere2D();
        /// <summary>
        /// 球心坐标
        /// </summary>
        public Vector2 Center;
        /// <summary>
        /// 球体半径
        /// </summary>
        public float Radius;
        /// <summary>
        /// 带参构造函数
        /// </summary>
        /// <param name="center">球心坐标</param>
        /// <param name="radius">球体半径</param>
        public BoundingSphere2D(Vector2 center, float radius)
        {
            Center = center;
            Radius = radius;
        }
        /// <summary>
        /// 球体和包围盒是否碰撞
        /// </summary>
        /// <param name="sphere">球体对象</param>
        /// <param name="box">包围盒对象</param>
        /// <returns>如果相交返回true，否则返回false</returns>
        [Rtti.Meta("")]
        public static ContainmentType Contains(BoundingSphere2D sphere, BoundingBox2D box)
        {
            Vector2 vector;

            if (!BoundingBox2D.Intersects(box, sphere))
                return ContainmentType.Disjoint;

            float radius = sphere.Radius * sphere.Radius;
            vector.X = sphere.Center.X - box.Minimum.X;
            vector.Y = sphere.Center.Y - box.Maximum.Y;
            
            if (vector.LengthSquared() > radius)
                return ContainmentType.Intersects;

            vector.X = sphere.Center.X - box.Maximum.X;
            vector.Y = sphere.Center.Y - box.Maximum.Y;
            
            if (vector.LengthSquared() > radius)
                return ContainmentType.Intersects;

            vector.X = sphere.Center.X - box.Maximum.X;
            vector.Y = sphere.Center.Y - box.Minimum.Y;
            
            if (vector.LengthSquared() > radius)
                return ContainmentType.Intersects;

            vector.X = sphere.Center.X - box.Minimum.X;
            vector.Y = sphere.Center.Y - box.Minimum.Y;
            
            if (vector.LengthSquared() > radius)
                return ContainmentType.Intersects;

            return ContainmentType.Contains;
        }
        /// <summary>
        /// 两个球体是否碰撞
        /// </summary>
        /// <param name="sphere1">球体1</param>
        /// <param name="sphere2">球体2</param>
        /// <returns>返回相交类型</returns>
        [Rtti.Meta("")]
        public static ContainmentType Contains(BoundingSphere2D sphere1, BoundingSphere2D sphere2)
        {
            float distance;
            float x = sphere1.Center.X - sphere2.Center.X;
            float y = sphere1.Center.Y - sphere2.Center.Y;

            distance = (float)(Math.Sqrt((x * x) + (y * y)));
            float radius = sphere1.Radius;
            float radius2 = sphere2.Radius;

            if (radius + radius2 < distance)
                return ContainmentType.Disjoint;

            if (radius - radius2 < distance)
                return ContainmentType.Intersects;

            return ContainmentType.Contains;
        }
        /// <summary>
        /// 球体与点的包含类型
        /// </summary>
        /// <param name="sphere">球体对象</param>
        /// <param name="vector">点坐标</param>
        /// <returns>返回相交类型</returns>
        [Rtti.Meta("")]
        public static ContainmentType Contains(BoundingSphere2D sphere, Vector2 vector)
        {
            float x = vector.X - sphere.Center.X;
            float y = vector.Y - sphere.Center.Y;
            
            float distance = (x * x) + (y * y);

            if (distance >= (sphere.Radius * sphere.Radius))
                return ContainmentType.Disjoint;

            return ContainmentType.Contains;
        }

        public bool Intersect(ref BoundingSphere2D sphere, ref BoundingBox2D box)
        {
            bool retCode;
            float s;
            float d = 0.0f;
            //find the square of the distance from the sphere to the box
            for (int i = 0; i < 3; i++)
            {
                if (sphere.Center[i] < box.Minimum[i])
                {
                    s = sphere.Center[i] - box.Minimum[i];
                    d += s * s;
                }
                else if (sphere.Center[i] > box.Maximum[i])
                {
                    s = sphere.Center[i] - box.Maximum[i];
                    d += s * s;
                }
            }

            retCode = (d <= (sphere.Radius * sphere.Radius));
            return retCode;
        }
        /// <summary>
        /// 根据包围盒建立球体包围盒
        /// </summary>
        /// <param name="box">包围盒对象</param>
        /// <returns>返回建立的球体包围盒</returns>
        [Rtti.Meta("")]
        public static BoundingSphere2D FromBox(BoundingBox2D box)
        {
            BoundingSphere2D sphere;
            Vector2.Lerp(in box.Minimum, in box.Maximum, 0.5f, out sphere.Center);

            float x = box.Minimum.X - box.Maximum.X;
            float y = box.Minimum.Y - box.Maximum.Y;

            float distance = (float)(Math.Sqrt((x * x) + (y * y)));

            sphere.Radius = distance * 0.5f;

            return sphere;
        }
        
        /// <summary>
        /// 两个球体的混合
        /// </summary>
        /// <param name="sphere1">球体包围盒1</param>
        /// <param name="sphere2">球体包围盒2</param>
        /// <returns>返回混合后的球体包围盒对象</returns>
        [Rtti.Meta("")]
        public static BoundingSphere2D Merge(BoundingSphere2D sphere1, BoundingSphere2D sphere2)
        {
            BoundingSphere2D sphere;
            Vector2 difference = sphere2.Center - sphere1.Center;

            float length = difference.Length();
            float radius = sphere1.Radius;
            float radius2 = sphere2.Radius;

            if (radius + radius2 >= length)
            {
                if (radius - radius2 >= length)
                    return sphere1;

                if (radius2 - radius >= length)
                    return sphere2;
            }

            Vector2 vector = difference * (1.0f / length);
            float min = Math.Min(-radius, length - radius2);
            float max = (Math.Max(radius, length + radius2) - min) * 0.5f;

            sphere.Center = sphere1.Center + vector * (max + min);
            sphere.Radius = max;

            return sphere;
        }
        /// <summary>
        /// 球体与包围盒是否相交
        /// </summary>
        /// <param name="sphere">球体包围盒对象</param>
        /// <param name="box">包围盒对象</param>
        /// <returns>如果相交返回true，否则返回false</returns>
        [Rtti.Meta("")]
        public static bool Intersects(BoundingSphere2D sphere, BoundingBox2D box)
        {
            return BoundingBox2D.Intersects(box, sphere);
        }
        /// <summary>
        /// 两个球体包围盒是否相交
        /// </summary>
        /// <param name="sphere1">球体包围盒对象</param>
        /// <param name="sphere2">球体包围盒对象</param>
        /// <returns>如果相交返回true，否则返回false</returns>
        [Rtti.Meta("")]
        public static bool Intersects(BoundingSphere2D sphere1, BoundingSphere2D sphere2)
        {
            float distance;
            distance = Vector2.DistanceSquared(sphere1.Center, sphere2.Center);
            float radius = sphere1.Radius;
            float radius2 = sphere2.Radius;

            if ((radius * radius) + (2.0f * radius * radius2) + (radius2 * radius2) <= distance)
                return false;

            return true;
        }

        public static bool operator ==(BoundingSphere2D left, BoundingSphere2D right)
        {
            return Equals(left, right);
        }
        public static bool operator !=(BoundingSphere2D left, BoundingSphere2D right)
        {
            return !Equals(left, right);
        }
        /// <summary>
        /// 转换成string
        /// </summary>
        /// <returns>返回转换后的string</returns>
        public override String ToString()
        {
            return String.Format(CultureInfo.CurrentCulture, "Center:{0} Radius:{1}", Center.ToString(), Radius.ToString(CultureInfo.CurrentCulture));
        }
        /// <summary>
        /// 获取哈希值
        /// </summary>
        /// <returns>返回对象的哈希值</returns>
        public override int GetHashCode()
        {
            return Center.GetHashCode() + Radius.GetHashCode();
        }
        /// <summary>
        /// 判断对象是否相等
        /// </summary>
        /// <param name="value">需要判断的对象</param>
        /// <returns>如果相等返回true，否则返回false</returns>
        public override bool Equals(Object value)
        {
            if (value == null)
                return false;

            if (value.GetType() != GetType())
                return false;

            return Equals((BoundingSphere2D)(value));
        }
        /// <summary>
        /// 判断对象与球体包围盒是否相同
        /// </summary>
        /// <param name="value">球体包围盒</param>
        /// <returns>如果相等返回true，否则返回false</returns>
        public bool Equals(BoundingSphere2D value)
        {
            return (Center == value.Center && Radius == value.Radius);
        }
        /// <summary>
        /// 判断两个球体包围盒是否相等
        /// </summary>
        /// <param name="value1">球体包围盒</param>
        /// <param name="value2">球体包围盒</param>
        /// <returns>如果相等返回true，否则返回false</returns>
        public static bool Equals(ref BoundingSphere2D value1, ref BoundingSphere2D value2)
        {
            return (value1.Center == value2.Center && value1.Radius == value2.Radius);
        }
    }
}
