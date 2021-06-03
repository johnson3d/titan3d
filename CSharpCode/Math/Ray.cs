using System;

using System.Globalization;

namespace EngineNS
{
    /// <summary>
    /// 射线结构体
    /// </summary>
    [System.Serializable]
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
        public Ray( Vector3 position, Vector3 direction )
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
        public static bool Intersects(Ray ray, Plane plane, out float distance)
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
        public static bool Intersects(Ray ray, Vector3 vertex1, Vector3 vertex2, Vector3 vertex3, out float distance)
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
        public static bool Intersects(Ray ray, Vector3 vertex1, Vector3 vertex2, Vector3 vertex3, out float distance, out float barycentricU, out float barycentricV)
	    {
            unsafe
            {
                fixed(float* pinnedDist = &distance)
                {
                    fixed(float* pinnedU = &barycentricU)
                    {
                        fixed(float* pinnedV = &barycentricV)
                        {
                            if( IDllImportApi.v3dxIntersectTri( &vertex1 , 
			                    &vertex2 , &vertex3 ,
			                    &ray.Position , &ray.Direction ,
			                    pinnedU, pinnedV, pinnedDist )!=0 )
			                    return true;
		                    else
			                    return false;
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
        public static bool Intersects(Ray ray, BoundingBox box, out float distance)
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
        public static bool Intersects(Ray ray, BoundingSphere sphere, out float distance)
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
        public static bool operator ==(Ray left, Ray right)
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
        public static bool operator !=(Ray left, Ray right)
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
        public static bool Equals(ref Ray value1, ref Ray value2)
	    {
		    return ( value1.Position == value2.Position && value1.Direction == value2.Direction );
	    }
    }
}
