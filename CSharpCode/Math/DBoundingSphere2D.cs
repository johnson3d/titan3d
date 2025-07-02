using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace EngineNS
{
    public struct DBoundingSphere2D : System.IEquatable<DBoundingSphere2D>
    {
        public static DBoundingSphere2D DefaultSphere = new DBoundingSphere2D();
        /// <summary>
        /// 球心坐标
        /// </summary>
        public DVector2 Center;
        /// <summary>
        /// 球体半径
        /// </summary>
        public double Radius;
        /// <summary>
        /// 带参构造函数
        /// </summary>
        /// <param name="center">球心坐标</param>
        /// <param name="radius">球体半径</param>
        public DBoundingSphere2D(DVector2 center, double radius)
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
        public static ContainmentType Contains(DBoundingSphere2D sphere, DBoundingBox2D box)
        {
            DVector2 vector;

            if (!DBoundingBox2D.Intersects(box, sphere))
                return ContainmentType.Disjoint;

            double radius = sphere.Radius * sphere.Radius;
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
        public static ContainmentType Contains(DBoundingSphere2D sphere1, DBoundingSphere2D sphere2)
        {
            double distance;
            double x = sphere1.Center.X - sphere2.Center.X;
            double y = sphere1.Center.Y - sphere2.Center.Y;

            distance = (double)(Math.Sqrt((x * x) + (y * y)));
            double radius = sphere1.Radius;
            double radius2 = sphere2.Radius;

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
        public static ContainmentType Contains(DBoundingSphere2D sphere, DVector2 vector)
        {
            double x = vector.X - sphere.Center.X;
            double y = vector.Y - sphere.Center.Y;
            
            double distance = (x * x) + (y * y);

            if (distance >= (sphere.Radius * sphere.Radius))
                return ContainmentType.Disjoint;

            return ContainmentType.Contains;
        }

        public bool Intersect(ref DBoundingSphere2D sphere, ref DBoundingBox2D box)
        {
            bool retCode;
            double s;
            double d = 0.0f;
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
        public static DBoundingSphere2D FromBox(DBoundingBox2D box)
        {
            DBoundingSphere2D sphere;
            DVector2.Lerp(in box.Minimum, in box.Maximum, 0.5f, out sphere.Center);

            double x = box.Minimum.X - box.Maximum.X;
            double y = box.Minimum.Y - box.Maximum.Y;

            double distance = (double)(Math.Sqrt((x * x) + (y * y)));

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
        public static DBoundingSphere2D Merge(DBoundingSphere2D sphere1, DBoundingSphere2D sphere2)
        {
            DBoundingSphere2D sphere;
            DVector2 difference = sphere2.Center - sphere1.Center;

            double length = difference.Length();
            double radius = sphere1.Radius;
            double radius2 = sphere2.Radius;

            if (radius + radius2 >= length)
            {
                if (radius - radius2 >= length)
                    return sphere1;

                if (radius2 - radius >= length)
                    return sphere2;
            }

            DVector2 vector = difference * (1.0f / length);
            double min = Math.Min(-radius, length - radius2);
            double max = (Math.Max(radius, length + radius2) - min) * 0.5f;

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
        public static bool Intersects(DBoundingSphere2D sphere, DBoundingBox2D box)
        {
            return DBoundingBox2D.Intersects(box, sphere);
        }
        /// <summary>
        /// 两个球体包围盒是否相交
        /// </summary>
        /// <param name="sphere1">球体包围盒对象</param>
        /// <param name="sphere2">球体包围盒对象</param>
        /// <returns>如果相交返回true，否则返回false</returns>
        [Rtti.Meta("")]
        public static bool Intersects(DBoundingSphere2D sphere1, DBoundingSphere2D sphere2)
        {
            double distance;
            distance = DVector2.DistanceSquared(sphere1.Center, sphere2.Center);
            double radius = sphere1.Radius;
            double radius2 = sphere2.Radius;

            if ((radius * radius) + (2.0f * radius * radius2) + (radius2 * radius2) <= distance)
                return false;

            return true;
        }

        public static bool operator ==(DBoundingSphere2D left, DBoundingSphere2D right)
        {
            return Equals(left, right);
        }
        public static bool operator !=(DBoundingSphere2D left, DBoundingSphere2D right)
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

            return Equals((DBoundingSphere2D)(value));
        }
        /// <summary>
        /// 判断对象与球体包围盒是否相同
        /// </summary>
        /// <param name="value">球体包围盒</param>
        /// <returns>如果相等返回true，否则返回false</returns>
        public bool Equals(DBoundingSphere2D value)
        {
            return (Center == value.Center && Radius == value.Radius);
        }
        /// <summary>
        /// 判断两个球体包围盒是否相等
        /// </summary>
        /// <param name="value1">球体包围盒</param>
        /// <param name="value2">球体包围盒</param>
        /// <returns>如果相等返回true，否则返回false</returns>
        public static bool Equals(ref DBoundingSphere2D value1, ref DBoundingSphere2D value2)
        {
            return (value1.Center == value2.Center && value1.Radius == value2.Radius);
        }
    }
}
