using System;

using System.Globalization;

namespace EngineNS
{
    /// <summary>
    /// 射线结构体
    /// </summary>
	[System.Runtime.InteropServices.StructLayout( System.Runtime.InteropServices.LayoutKind.Sequential )]
    //[System.ComponentModel.TypeConverter( typeof(EngineNS.Design.RayConverter) )]
	public struct Ray : System.IEquatable<Ray>
    {
        /// <summary>
        /// 对象位置
        /// </summary>
        [Rtti.Meta]
        public Vector3 Position;
        /// <summary>
        /// 对象的方向
        /// </summary>
        [Rtti.Meta]
        public Vector3 Direction;
        /// <summary>
        /// 带参构造函数
        /// </summary>
        /// <param name="position">对象位置</param>
        /// <param name="direction">对象的方向</param>
        public Ray(in Vector3 position,in Vector3 direction )
	    {
		    Position = position;
		    Direction = direction;
	    }
        /// <summary>
        /// 射线与平面是否相交
        /// </summary>
        /// <param name="ray">射线对象</param>
        /// <param name="plane">平面对象</param>
        /// <param name="distance">交点距离</param>
        /// <returns>相交返回true，否则返回false</returns>
        [Rtti.Meta]
        public static bool Intersects(in Ray ray, in Plane plane, out float distance)
	    {
		    ray.Direction.Normalize();
		    float dotDirection = (plane.Normal.X * ray.Direction.X) + (plane.Normal.Y * ray.Direction.Y) + (plane.Normal.Z * ray.Direction.Z);

		    if( Math.Abs( dotDirection ) < 0.000001f )
		    {
			    distance = 0;
			    return false;
		    }

		    float dotPosition = (plane.Normal.X * ray.Position.X) + (plane.Normal.Y * ray.Position.Y) + (plane.Normal.Z * ray.Position.Z);
		    float num = ( -plane.D - dotPosition ) / dotDirection;

		    if( num < 0.0f )
		    {
			    if( num < -0.000001f )
			    {
				    distance = 0;
				    return false;
			    }
			    num = 0.0f;
		    }

		    distance = num;
		    return true;
	    }
        /// <summary>
        /// 射线与指定顶点的三角形是否相交
        /// </summary>
        /// <param name="ray">射线对象</param>
        /// <param name="vertex1">顶点坐标</param>
        /// <param name="vertex2">顶点坐标</param>
        /// <param name="vertex3">顶点坐标</param>
        /// <param name="distance">交点距离</param>
        /// <returns>如果相交返回true，否则返回false</returns>
        [Rtti.Meta]
        public static bool Intersects(in Ray ray, in Vector3 vertex1, in Vector3 vertex2, in Vector3 vertex3, out float distance)
	    {
		    float u, v;
		    return Intersects( ray, vertex1, vertex2, vertex3, out distance, out u, out v );
	    }
        /// <summary>
        /// 射线与指定顶点的三角形是否相交
        /// </summary>
        /// <param name="ray">射线对象</param>
        /// <param name="vertex1">顶点坐标</param>
        /// <param name="vertex2">顶点坐标</param>
        /// <param name="vertex3">顶点坐标</param>
        /// <param name="distance">交点距离</param>
        /// <param name="barycentricU">质心的U坐标</param>
        /// <param name="barycentricV">质心的V坐标</param>
        /// <returns>如果相交返回true，否则返回false</returns>
        [Rtti.Meta]
        public static bool Intersects(in Ray ray, in Vector3 vertex1, in Vector3 vertex2, in Vector3 vertex3, out float distance, out float barycentricU, out float barycentricV)
	    {
            unsafe
            {
                fixed(float* pinnedDist = &distance)
                {
                    fixed(float* pinnedU = &barycentricU)
                    {
                        fixed(float* pinnedV = &barycentricV)
                        {
                            fixed (Ray* pinnedRay = &ray)
                            fixed (Vector3* pvertex1 = &vertex1)
                            fixed (Vector3* pvertex2 = &vertex2)
                            fixed (Vector3* pvertex3 = &vertex3)
                            {
                                if (IDllImportApi.v3dxIntersectTri(pvertex1,
                                pvertex2, pvertex3,
                                &pinnedRay->Position, &pinnedRay->Direction,
                                pinnedU, pinnedV, pinnedDist) != 0)
                                    return true;
                                else
                                    return false;
                            }   
                        }
                    }
                }
            }
	    }
        /// <summary>
        /// 射线与包围盒是否相交
        /// </summary>
        /// <param name="ray">射线对象</param>
        /// <param name="box">包围盒对象</param>
        /// <param name="distance">交点距离</param>
        /// <returns>如果相交返回true，否则返回false</returns>
        [Rtti.Meta]
        public static bool Intersects(in Ray ray, in BoundingBox box, out float distance)
	    {
		    float d = 0.0f;
		    float maxValue = float.MaxValue;

		    ray.Direction.Normalize();
		    if( Math.Abs( ray.Direction.X ) < 0.0000001 )
		    {
			    if( ray.Position.X < box.Minimum.X || ray.Position.X > box.Maximum.X )
			    {
				    distance = 0.0f;
				    return false;
			    }
		    }
		    else
		    {
			    float inv = 1.0f / ray.Direction.X;
			    float min = (box.Minimum.X - ray.Position.X) * inv;
			    float max = (box.Maximum.X - ray.Position.X) * inv;

			    if( min > max )
			    {
				    float temp = min;
				    min = max;
				    max = temp;
			    }

			    d = Math.Max( min, d );
			    maxValue = Math.Min( max, maxValue );

			    if( d > maxValue )
			    {
				    distance = 0.0f;
				    return false;
			    }
		    }

		    if( Math.Abs( ray.Direction.Y ) < 0.0000001 )
		    {
			    if( ray.Position.Y < box.Minimum.Y || ray.Position.Y > box.Maximum.Y )
			    {
				    distance = 0.0f;
				    return false;
			    }
		    }
		    else
		    {
			    float inv = 1.0f / ray.Direction.Y;
			    float min = (box.Minimum.Y - ray.Position.Y) * inv;
			    float max = (box.Maximum.Y - ray.Position.Y) * inv;

			    if( min > max )
			    {
				    float temp = min;
				    min = max;
				    max = temp;
			    }

			    d = Math.Max( min, d );
			    maxValue = Math.Min( max, maxValue );

			    if( d > maxValue )
			    {
				    distance = 0.0f;
				    return false;
			    }
		    }

		    if( Math.Abs( ray.Direction.Z ) < 0.0000001 )
		    {
			    if( ray.Position.Z < box.Minimum.Z || ray.Position.Z > box.Maximum.Z )
			    {
				    distance = 0.0f;
				    return false;
			    }
		    }
		    else
		    {
			    float inv = 1.0f / ray.Direction.Z;
			    float min = (box.Minimum.Z - ray.Position.Z) * inv;
			    float max = (box.Maximum.Z - ray.Position.Z) * inv;

			    if( min > max )
			    {
				    float temp = min;
				    min = max;
				    max = temp;
			    }

			    d = Math.Max( min, d );
			    maxValue = Math.Min( max, maxValue );

			    if( d > maxValue )
			    {
				    distance = 0.0f;
				    return false;
			    }
		    }

		    distance = d;
		    return true;
	    }
        /// <summary>
        /// 射线与球体形状的包围盒是否相交
        /// </summary>
        /// <param name="ray">射线对象</param>
        /// <param name="sphere">球体形状的包围盒</param>
        /// <param name="distance">交点距离</param>
        /// <returns>如果相交返回true，否则返回false</returns>
        [Rtti.Meta]
        public static bool Intersects(in Ray ray, in BoundingSphere sphere, out float distance)
	    {
		    float x = sphere.Center.X - ray.Position.X;
		    float y = sphere.Center.Y - ray.Position.Y;
		    float z = sphere.Center.Z - ray.Position.Z;
		    float pyth = (x * x) + (y * y) + (z * z);
		    float rr = sphere.Radius * sphere.Radius;

		    if( pyth <= rr )
		    {
			    distance = 0.0f;
			    return true;
		    }

		    ray.Direction.Normalize();
		    float dot = (x * ray.Direction.X) + (y * ray.Direction.Y) + (z * ray.Direction.Z);
		    if( dot < 0.0f )
		    {
			    distance = 0.0f;
			    return false;
		    }

		    float temp = pyth - (dot * dot);
		    if( temp > rr )
		    {
			    distance = 0.0f;
			    return false;
		    }

		    distance = dot - (float)( Math.Sqrt( (double)( rr - temp ) ) );
		    return true;
	    }
        /// <summary>
        /// 重载"=="操作符
        /// </summary>
        /// <param name="left">射线对象</param>
        /// <param name="right">射线对象</param>
        /// <returns>如果两条射线相同返回true，否则返回false</returns>
        public static bool operator ==(in Ray left, in Ray right)
	    {
            return left.Equals(right);
		    //return Equals( left, right );
	    }
        /// <summary>
        /// 重载"!="操作符
        /// </summary>
        /// <param name="left">射线对象</param>
        /// <param name="right">射线对象</param>
        /// <returns>如果两条射线不相同返回true，否则返回false</returns>
        public static bool operator !=(in Ray left, in Ray right)
	    {
            return !left.Equals(right);
		    //return !Equals( left, right );
	    }
        /// <summary>
        /// 转换到string字符串
        /// </summary>
        /// <returns>返回转换后的string字符串</returns>
        public override String ToString()
	    {
		    return String.Format( CultureInfo.CurrentCulture, "Position:{0} Direction:{1}", Position.ToString(), Direction.ToString() );
	    }
        /// <summary>
        /// 获取对象的哈希值
        /// </summary>
        /// <returns>返回对象的哈希值</returns>
        public override int GetHashCode()
	    {
		    return Position.GetHashCode() + Direction.GetHashCode();
	    }
        /// <summary>
        /// 判断两个对象是否相等
        /// </summary>
        /// <param name="value">可转换成射线的对象</param>
        /// <returns>如果两条射线相同返回true，否则返回false</returns>
        public override bool Equals(Object value)
	    {
		    if( value == null )
			    return false;

		    if( value.GetType() != GetType() )
			    return false;

		    return Equals( (Ray)( value ) );
	    }
        /// <summary>
        /// 判断两条射线是否相等
        /// </summary>
        /// <param name="value">射线对象</param>
        /// <returns>如果两条射线相同返回true，否则返回false</returns>
        public bool Equals(Ray value)
	    {
		    return ( Position == value.Position && Direction == value.Direction );
	    }
        /// <summary>
        /// 判断两条射线是否相等
        /// </summary>
        /// <param name="value1">射线对象</param>
        /// <param name="value2">射线对象</param>
        /// <returns>如果两条射线相同返回true，否则返回false</returns>
        public static bool Equals(in Ray value1, in Ray value2)
	    {
		    return ( value1.Position == value2.Position && value1.Direction == value2.Direction );
	    }
    }

    public struct LinesDistanceHelper
    {
        public void SetLineA(in Vector3 start, in Vector3 end)
        {
            a1_x = start.X;
            a1_y = start.Y;
            a1_z = start.Z;

            a2_x = end.X;
            a2_y = end.Y;
            a2_z = end.Z;
        }
        public void SetLineB(in Vector3 start, in Vector3 end)
        {
            b1_x = start.X;
            b1_y = start.Y;
            b1_z = start.Z;

            b2_x = end.X;
            b2_y = end.Y;
            b2_z = end.Z;
        }
        public void SetLineA(double A1x, double A1y, double A1z, double A2x, double A2y, double A2z)
        {
            a1_x = A1x;
            a1_y = A1y;
            a1_z = A1z;

            a2_x = A2x;
            a2_y = A2y;
            a2_z = A2z;
        }
        public void SetLineB(double B1x, double B1y, double B1z, double B2x, double B2y, double B2z)
        {
            b1_x = B1x;
            b1_y = B1y;
            b1_z = B1z;

            b2_x = B2x;
            b2_y = B2y;
            b2_z = B2z;
        }

        public void GetDistance(out double t1, out double t2)
        {
            //方法来自：http://blog.csdn.net/pi9nc/article/details/11820545

            double d1_x = a2_x - a1_x;
            double d1_y = a2_y - a1_y;
            double d1_z = a2_z - a1_z;

            double d2_x = b2_x - b1_x;
            double d2_y = b2_y - b1_y;
            double d2_z = b2_z - b1_z;

            double e_x = b1_x - a1_x;
            double e_y = b1_y - a1_y;
            double e_z = b1_z - a1_z;


            double cross_e_d2_x, cross_e_d2_y, cross_e_d2_z;
            cross(e_x, e_y, e_z, d2_x, d2_y, d2_z, out cross_e_d2_x, out cross_e_d2_y, out cross_e_d2_z);
            double cross_e_d1_x, cross_e_d1_y, cross_e_d1_z;
            cross(e_x, e_y, e_z, d1_x, d1_y, d1_z, out cross_e_d1_x, out cross_e_d1_y, out cross_e_d1_z);
            double cross_d1_d2_x, cross_d1_d2_y, cross_d1_d2_z;
            cross(d1_x, d1_y, d1_z, d2_x, d2_y, d2_z, out cross_d1_d2_x, out cross_d1_d2_y, out cross_d1_d2_z);

            //double t1, t2;
            t1 = dot(cross_e_d2_x, cross_e_d2_y, cross_e_d2_z, cross_d1_d2_x, cross_d1_d2_y, cross_d1_d2_z);
            t2 = dot(cross_e_d1_x, cross_e_d1_y, cross_e_d1_z, cross_d1_d2_x, cross_d1_d2_y, cross_d1_d2_z);
            double dd = norm(cross_d1_d2_x, cross_d1_d2_y, cross_d1_d2_z);
            t1 /= dd * dd;
            t2 /= dd * dd;

            //得到最近的位置
            PonA_x = (a1_x + (a2_x - a1_x) * t1);
            PonA_y = (a1_y + (a2_y - a1_y) * t1);
            PonA_z = (a1_z + (a2_z - a1_z) * t1);

            PonB_x = (b1_x + (b2_x - b1_x) * t2);
            PonB_y = (b1_y + (b2_y - b1_y) * t2);
            PonB_z = (b1_z + (b2_z - b1_z) * t2);

            distance = norm(PonB_x - PonA_x, PonB_y - PonA_y, PonB_z - PonA_z);
        }

        public Vector3 GetPonA()
        {
            return new Vector3((float)PonA_x, (float)PonA_y, (float)PonA_z);
        }
        public Vector3 GetPonB()
        {
            return new Vector3((float)PonB_x, (float)PonB_y, (float)PonB_z);
        }
        public double PonA_x;//两直线最近点之A线上的点的x坐标
        public double PonA_y;//两直线最近点之A线上的点的y坐标
        public double PonA_z;//两直线最近点之A线上的点的z坐标
        public double PonB_x;//两直线最近点之B线上的点的x坐标
        public double PonB_y;//两直线最近点之B线上的点的y坐标
        public double PonB_z;//两直线最近点之B线上的点的z坐标
        public double distance;//两直线距离
                        //直线A的第一个点
        private double a1_x;
        private double a1_y;
        private double a1_z;
        //直线A的第二个点
        private double a2_x;
        private double a2_y;
        private double a2_z;

        //直线B的第一个点
        private double b1_x;
        private double b1_y;
        private double b1_z;

        //直线B的第二个点
        private double b2_x;
        private double b2_y;
        private double b2_z;

        //点乘
        private double dot(double ax, double ay, double az, double bx, double by, double bz) { return ax * bx + ay * by + az * bz; }
        //向量叉乘得到法向量，最后三个参数为输出参数
        private void cross(double ax, double ay, double az, double bx, double by, double bz, out double x, out double y, out double z)
        {
            x = ay * bz - az * by;
            y = az * bx - ax * bz;
            z = ax * by - ay * bx;
        }
        //向量取模
        private double norm(double ax, double ay, double az) 
        { 
            return Math.Sqrt(dot(ax, ay, az, ax, ay, az)); 
        }
    }
}
