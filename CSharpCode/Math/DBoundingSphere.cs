using System;

using System.Globalization;

namespace EngineNS
{
    /// <summary>
    /// 球体包围盒结构体
    /// </summary>
    [System.Serializable]
    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
    public struct DBoundingSphere : System.IEquatable<DBoundingSphere>
    {
        public static DBoundingSphere DefaultSphere = new DBoundingSphere();
        /// <summary>
        /// 球心坐标
        /// </summary>
        public DVector3 Center;
        /// <summary>
        /// 球体半径
        /// </summary>
        public double Radius;
        /// <summary>
        /// 带参构造函数
        /// </summary>
        /// <param name="center">球心坐标</param>
        /// <param name="radius">球体半径</param>
        public DBoundingSphere(DVector3 center, double radius)
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
        [Rtti.Meta]
        public static ContainmentType Contains(DBoundingSphere sphere, DBoundingBox box)
        {
            DVector3 vector;

            if (!DBoundingBox.Intersects(box, sphere))
                return ContainmentType.Disjoint;

            double radius = sphere.Radius * sphere.Radius;
            vector.X = sphere.Center.X - box.Minimum.X;
            vector.Y = sphere.Center.Y - box.Maximum.Y;
            vector.Z = sphere.Center.Z - box.Maximum.Z;

            if (vector.LengthSquared() > radius)
                return ContainmentType.Intersects;

            vector.X = sphere.Center.X - box.Maximum.X;
            vector.Y = sphere.Center.Y - box.Maximum.Y;
            vector.Z = sphere.Center.Z - box.Maximum.Z;

            if (vector.LengthSquared() > radius)
                return ContainmentType.Intersects;

            vector.X = sphere.Center.X - box.Maximum.X;
            vector.Y = sphere.Center.Y - box.Minimum.Y;
            vector.Z = sphere.Center.Z - box.Maximum.Z;

            if (vector.LengthSquared() > radius)
                return ContainmentType.Intersects;

            vector.X = sphere.Center.X - box.Minimum.X;
            vector.Y = sphere.Center.Y - box.Minimum.Y;
            vector.Z = sphere.Center.Z - box.Maximum.Z;

            if (vector.LengthSquared() > radius)
                return ContainmentType.Intersects;

            vector.X = sphere.Center.X - box.Minimum.X;
            vector.Y = sphere.Center.Y - box.Maximum.Y;
            vector.Z = sphere.Center.Z - box.Minimum.Z;

            if (vector.LengthSquared() > radius)
                return ContainmentType.Intersects;

            vector.X = sphere.Center.X - box.Maximum.X;
            vector.Y = sphere.Center.Y - box.Maximum.Y;
            vector.Z = sphere.Center.Z - box.Minimum.Z;

            if (vector.LengthSquared() > radius)
                return ContainmentType.Intersects;

            vector.X = sphere.Center.X - box.Maximum.X;
            vector.Y = sphere.Center.Y - box.Minimum.Y;
            vector.Z = sphere.Center.Z - box.Minimum.Z;

            if (vector.LengthSquared() > radius)
                return ContainmentType.Intersects;

            vector.X = sphere.Center.X - box.Minimum.X;
            vector.Y = sphere.Center.Y - box.Minimum.Y;
            vector.Z = sphere.Center.Z - box.Minimum.Z;

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
        [Rtti.Meta]
        public static ContainmentType Contains(DBoundingSphere sphere1, DBoundingSphere sphere2)
        {
            double distance;
            double x = sphere1.Center.X - sphere2.Center.X;
            double y = sphere1.Center.Y - sphere2.Center.Y;
            double z = sphere1.Center.Z - sphere2.Center.Z;

            distance = (double)(Math.Sqrt((x * x) + (y * y) + (z * z)));
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
        [Rtti.Meta]
        public static ContainmentType Contains(DBoundingSphere sphere, DVector3 vector)
        {
            double x = vector.X - sphere.Center.X;
            double y = vector.Y - sphere.Center.Y;
            double z = vector.Z - sphere.Center.Z;

            double distance = (x * x) + (y * y) + (z * z);

            if (distance >= (sphere.Radius * sphere.Radius))
                return ContainmentType.Disjoint;

            return ContainmentType.Contains;
        }
        [Rtti.Meta]
        public static DBoundingSphere FromBox(DBoundingBox box)
        {
            DBoundingSphere sphere;
            DVector3.Lerp(in box.Minimum, in box.Maximum, 0.5f, out sphere.Center);

            double x = box.Minimum.X - box.Maximum.X;
            double y = box.Minimum.Y - box.Maximum.Y;
            double z = box.Minimum.Z - box.Maximum.Z;

            double distance = (double)(Math.Sqrt((x * x) + (y * y) + (z * z)));

            sphere.Radius = distance * 0.5f;

            return sphere;
        }
        public static DBoundingSphere FromPoints(DVector3[] points)
        {
            DBoundingSphere sphere = new DBoundingSphere();
            
            return sphere;
        }
        /// <summary>
        /// 两个球体的混合
        /// </summary>
        /// <param name="sphere1">球体包围盒1</param>
        /// <param name="sphere2">球体包围盒2</param>
        /// <returns>返回混合后的球体包围盒对象</returns>
        [Rtti.Meta]
        public static DBoundingSphere Merge(DBoundingSphere sphere1, DBoundingSphere sphere2)
        {
            DBoundingSphere sphere;
            DVector3 difference = sphere2.Center - sphere1.Center;

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

            DVector3 vector = difference * (1.0f / length);
            double min = Math.Min(-radius, length - radius2);
            double max = (Math.Max(radius, length + radius2) - min) * 0.5f;

            sphere.Center = sphere1.Center + vector * (max + min);
            sphere.Radius = max;

            return sphere;
        }                
        public static bool Intersects(DBoundingSphere sphere1, DBoundingSphere sphere2)
        {
            double distance;
            distance = DVector3.DistanceSquared(sphere1.Center, sphere2.Center);
            double radius = sphere1.Radius;
            double radius2 = sphere2.Radius;

            if ((radius * radius) + (2.0f * radius * radius2) + (radius2 * radius2) <= distance)
                return false;

            return true;
        }

        [Rtti.Meta]
        public static PlaneIntersectionType Intersects(DBoundingSphere sphere, DPlane plane)
        {
            return DPlane.Intersects(plane, sphere);
        }
        /// <summary>
        /// 重载==操作符
        /// </summary>
        /// <param name="left">球体包围盒对象</param>
        /// <param name="right">球体包围盒对象</param>
        /// <returns>如果相等返回true，否则返回false</returns>
        public static bool operator ==(DBoundingSphere left, DBoundingSphere right)
        {
            return Equals(left, right);
        }
        /// <summary>
        /// 重载!=操作符
        /// </summary>
        /// <param name="left">球体包围盒对象</param>
        /// <param name="right">球体包围盒对象</param>
        /// <returns>如果不相等返回true，否则返回false</returns>
        public static bool operator !=(DBoundingSphere left, DBoundingSphere right)
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
        public override bool Equals(object value)
        {
            if (value == null)
                return false;

            if (value.GetType() != GetType())
                return false;

            return Equals((DBoundingSphere)(value));
        }
        /// <summary>
        /// 判断对象与球体包围盒是否相同
        /// </summary>
        /// <param name="value">球体包围盒</param>
        /// <returns>如果相等返回true，否则返回false</returns>
        public bool Equals(DBoundingSphere value)
        {
            return (Center == value.Center && Radius == value.Radius);
        }
        /// <summary>
        /// 判断两个球体包围盒是否相等
        /// </summary>
        /// <param name="value1">球体包围盒</param>
        /// <param name="value2">球体包围盒</param>
        /// <returns>如果相等返回true，否则返回false</returns>
        public static bool Equals(ref DBoundingSphere value1, ref DBoundingSphere value2)
        {
            return (value1.Center == value2.Center && value1.Radius == value2.Radius);
        }
    }
}
