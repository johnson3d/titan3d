using System;

using System.Globalization;

namespace EngineNS
{
    /// <summary>
    /// 包围盒结构体
    /// </summary>
    [System.Serializable]
	[System.Runtime.InteropServices.StructLayout( System.Runtime.InteropServices.LayoutKind.Sequential )]
    //[System.ComponentModel.TypeConverter( typeof(EngineNS.Design.BoundingBoxConverter) )]
	public struct BoundingBox : System.IEquatable<BoundingBox>
    {
        public static readonly BoundingBox Empty = new BoundingBox(new Vector3(float.MaxValue), new Vector3(float.MinValue));
        /// <summary>
        /// 最小顶点
        /// </summary>
        public Vector3 Minimum;
        /// <summary>
        /// 最大顶点
        /// </summary>
        public Vector3 Maximum;
        public BoundingBox(float minX, float minY, float minZ, float maxX, float maxY, float maxZ)
        {
            Minimum.X = minX;
            Minimum.Y = minY;
            Minimum.Z = minZ;
            Maximum.X = maxX;
            Maximum.Y = maxY;
            Maximum.Z = maxZ;
        }
        public BoundingBox(float extentX, float extentY, float extentZ)
        {
            Minimum.X = extentX * -0.5f;
            Minimum.Y = extentY * -0.5f;
            Minimum.Z = extentZ * -0.5f;
            Maximum.X = extentX * 0.5f;
            Maximum.Y = extentY * 0.5f;
            Maximum.Z = extentZ * 0.5f;
        }
        public BoundingBox(Vector3 center, float extent = 1.0f)
        {
            Minimum = center - Vector3.One * extent * 0.5f;
            Maximum = center + Vector3.One * extent * 0.5f;
        }
        /// <summary>
        /// 带参构造函数
        /// </summary>
        /// <param name="minimum">最小顶点</param>
        /// <param name="maximum">最大顶点</param>
        public BoundingBox( Vector3 minimum, Vector3 maximum ) 
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
            Minimum = (Vector3)System.Runtime.InteropServices.Marshal.PtrToStructure(minimum, typeof(Vector3));
            Maximum = (Vector3)System.Runtime.InteropServices.Marshal.PtrToStructure(maximum, typeof(Vector3));
        }
        /// <summary>
        /// 初始化空的包围盒
        /// </summary>
        public void InitEmptyBox()
        {
            Minimum = new Vector3(float.MaxValue);
            Maximum = new Vector3(-float.MaxValue);
        }
        [Rtti.Meta]
        public static BoundingBox EmptyBox()
        {
            var bb = new BoundingBox();
            bb.InitEmptyBox();
            return bb;
        }

        [Rtti.Meta]
        public bool IsEmpty()
        {
            if (Minimum.X >= Maximum.X ||
               Minimum.Y >= Maximum.Y ||
               Minimum.Z >= Maximum.Z)
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
        ///    0------1
        ///   /|     /|
        ///  / |3   / 2
        /// 4------5  /
        /// | /    | /
        /// |/     |/
        /// 7------6
        public Vector3[] GetCorners()
	    {
		    Vector3[] results = new Vector3[8];
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
        public DVector3[] GetDCorners()
        {
            DVector3[] results = new DVector3[8];
            Vector3 tmp = new Vector3();
            SetVector3Value(ref tmp, Minimum.X, Maximum.Y, Maximum.Z);
            results[0] = tmp.AsDVector();
            SetVector3Value(ref tmp, Maximum.X, Maximum.Y, Maximum.Z);
            results[1] = tmp.AsDVector();
            SetVector3Value(ref tmp, Maximum.X, Minimum.Y, Maximum.Z);
            results[2] = tmp.AsDVector();
            SetVector3Value(ref tmp, Minimum.X, Minimum.Y, Maximum.Z);
            results[3] = tmp.AsDVector();
            SetVector3Value(ref tmp, Minimum.X, Maximum.Y, Minimum.Z);
            results[4] = tmp.AsDVector();
            SetVector3Value(ref tmp, Maximum.X, Maximum.Y, Minimum.Z);
            results[5] = tmp.AsDVector();
            SetVector3Value(ref tmp, Maximum.X, Minimum.Y, Minimum.Z);
            results[6] = tmp.AsDVector();
            SetVector3Value(ref tmp, Minimum.X, Minimum.Y, Minimum.Z);
            results[7] = tmp.AsDVector();
            return results;
        }
        public unsafe void GetCornersUnsafe(Vector3* verts)
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
        private static void SetVector3Value(ref Vector3 v3, float x,float y,float z)
        {
            v3.X = x;
            v3.Y = y;
            v3.Z = z;
        }
        /// <summary>
        /// 获取包围盒的中心点
        /// </summary>
        /// <returns>返回包围盒的中心点</returns>
        [Rtti.Meta]
        public Vector3 GetCenter()
	    {
		    return (Maximum + Minimum) * 0.5f;
	    }
        public Vector3 GetSize()
        {
            return Maximum - Minimum;
        }
        public float GetVolume()
        {
            var sz = GetSize();
            return sz.X * sz.Y * sz.Z;
        }
        public float GetMaxSide()
        {
            var sz = GetSize();
            if(sz.X>=sz.Y)
            {
                if (sz.X >= sz.Z)
                    return sz.X;
                else
                    return sz.Z;
            }
            else
            {
                if (sz.Y >= sz.Z)
                    return sz.Y;
                else
                    return sz.Z;
            }
        }
        [Rtti.Meta]
        public ContainmentType Contains(in BoundingBox box)
        {
            return Contains(in this, in box);
        }
        [Rtti.Meta]
        public static ContainmentType Contains(in BoundingBox box1, in BoundingBox box2)
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

        //ContainmentType Contains( BoundingBox box, BoundingSphere sphere )
        //{
        //    float dist;
        //    Vector3 clamped;

        //    Vector3.Clamp( sphere.Center, box.Minimum, box.Maximum, clamped );

        //    float x = sphere.Center.X - clamped.X;
        //    float y = sphere.Center.Y - clamped.Y;
        //    float z = sphere.Center.Z - clamped.Z;

        //    dist = (x * x) + (y * y) + (z * z);
        //    float radius = sphere.Radius;

        //    if( dist > (radius * radius) )
        //        return ContainmentType.Disjoint;

        //    if( box.Minimum.X + radius <= sphere.Center.X && sphere.Center.X <= box.Maximum.X - radius && 
        //        box.Maximum.X - box.Minimum.X > radius && box.Minimum.Y + radius <= sphere.Center.Y && 
        //        sphere.Center.Y <= box.Maximum.Y - radius && box.Maximum.Y - box.Minimum.Y > radius && 
        //        box.Minimum.Z + radius <= sphere.Center.Z && sphere.Center.Z <= box.Maximum.Z - radius &&
        //        box.Maximum.X - box.Minimum.X > radius )
        //        return ContainmentType.Contains;

        //    return ContainmentType.Intersects;
        //}

        [Rtti.Meta]
        public ContainmentType Contains(in Vector3 vector)
        {
            return Contains(in this, in vector);
        }
        [Rtti.Meta]
        public static ContainmentType Contains(in BoundingBox box, in Vector3 vector)
        {
            if (box.Minimum.X <= vector.X && vector.X <= box.Maximum.X && box.Minimum.Y <= vector.Y &&
                vector.Y <= box.Maximum.Y && box.Minimum.Z <= vector.Z && vector.Z <= box.Maximum.Z)
                return ContainmentType.Contains;

            return ContainmentType.Disjoint;
        }
        /// <summary>
        /// 根据点创建包围盒对象
        /// </summary>
        /// <param name="points">点列表</param>
        /// <returns>返回创建的包围盒</returns>
        public static BoundingBox FromPoints(Vector3[] points)
	    {
		    if( points == null || points.Length <= 0 )
			    throw new ArgumentNullException( "points" );

            BoundingBox result;
            Vector3 min;
            result.Minimum.X = float.MaxValue;
            result.Minimum.Y = float.MaxValue;
            result.Minimum.Z = float.MaxValue;

            Vector3 max;
            result.Maximum.X = float.MinValue;
            result.Maximum.Y = float.MinValue;
            result.Maximum.Z = float.MinValue;

            foreach ( var i in points )
		    {
                Vector3 vector = i;
			    Vector3.Minimize( in result.Minimum, in vector, out min );
                result.Minimum = min;
			    Vector3.Maximize(in result.Maximum, in vector, out max );
                result.Maximum = max;
		    }

		    return result;
	    }

        //BoundingBox FromPoints( DataStream points, int count, int stride )
        //{
        //    BoundingBox box;

        //    HRESULT hr = D3DXComputeBoundingBox( reinterpret_cast<D3DXVECTOR3*>( points.PositionPointer ), count, stride, 
        //        reinterpret_cast<D3DXVECTOR3*>( &box.Minimum ), reinterpret_cast<D3DXVECTOR3*>( &box.Maximum ) );

        //    if( RECORD_SDX( hr ).IsFailure )
        //        return BoundingBox();

        //    return box;
        //}

        //BoundingBox FromSphere( BoundingSphere sphere )
        //{
        //    BoundingBox box;
        //    box.Minimum = new Vector3( sphere.Center.X - sphere.Radius, sphere.Center.Y - sphere.Radius, sphere.Center.Z - sphere.Radius );
        //    box.Maximum = new Vector3( sphere.Center.X + sphere.Radius, sphere.Center.Y + sphere.Radius, sphere.Center.Z + sphere.Radius );
        //    return box;
        //}
        /// <summary>
        /// 包围盒与包围盒融合，新的包围盒取两个包围盒的最大值点和最小值点
        /// </summary>
        /// <param name="box1">包围盒对象1</param>
        /// <param name="box2">包围盒对象2</param>
        /// <returns>返回混合后的包围盒</returns>

        [Rtti.Meta]
        public void Merge(in Vector3 pos)
        {
            Vector3.Minimize(in Minimum, in pos, out Minimum);
            Vector3.Maximize(in Maximum, in pos, out Maximum);
        }
        [Rtti.Meta]
        public static BoundingBox Merge(in BoundingBox box1, in BoundingBox box2)
	    {
            if (box1.IsEmpty())
                return box2;
            else if (box2.IsEmpty())
                return box1;
            else
            {
                BoundingBox box;
                Vector3.Minimize(in box1.Minimum, in box2.Minimum, out box.Minimum);
                Vector3.Maximize(in box1.Maximum, in box2.Maximum, out box.Maximum);
                return box;
            }
	    }
        [Rtti.Meta]
        public static void Merge(in BoundingBox box1, in BoundingBox box2, out BoundingBox box)
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
                Vector3.Minimize(in box1.Minimum, in box2.Minimum, out box.Minimum);
                Vector3.Maximize(in box1.Maximum, in box2.Maximum, out box.Maximum);
            }
        }
        [Rtti.Meta]
        public void Merge2(in BoundingBox box, out BoundingBox outBox)
        {
            if (box.IsEmpty())
            {
                outBox = this;
                return;
            }
            Vector3.Minimize(in this.Minimum, in box.Minimum, out outBox.Minimum);
            Vector3.Maximize(in this.Maximum, in box.Maximum, out outBox.Maximum);
        }
        [Rtti.Meta]
        public static BoundingBox Merge(in BoundingBox box, in Vector3 point)
        {
            BoundingBox retBox;
            Vector3.Minimize(in box.Minimum, in point, out retBox.Minimum);
            Vector3.Maximize(in box.Maximum, in point, out retBox.Maximum);
            return retBox;
        }
        public static void And(in BoundingBox a, in BoundingBox b, out BoundingBox box)
        {
            Vector3.Maximize(in a.Minimum, in b.Minimum, out box.Minimum);
            Vector3.Minimize(in a.Maximum, in b.Maximum, out box.Maximum);
        }
        /// <summary>
        /// 判断两个包围盒是否相交
        /// </summary>
        /// <param name="box1">包围盒对象1</param>
        /// <param name="box2">包围盒对象2</param>
        /// <returns>如果相交返回true，否则返回false</returns>
        [Rtti.Meta]
        public static bool Intersects(BoundingBox box1, BoundingBox box2)
	    {
            return Intersects(in box1, in box2);
        }
        public static bool Intersects(in BoundingBox box1, in BoundingBox box2)
        {
            if (box1.Maximum.X < box2.Minimum.X || box1.Minimum.X > box2.Maximum.X)
                return false;

            if (box1.Maximum.Y < box2.Minimum.Y || box1.Minimum.Y > box2.Maximum.Y)
                return false;

            return (box1.Maximum.Z >= box2.Minimum.Z && box1.Minimum.Z <= box2.Maximum.Z);
        }
        /// <summary>
        /// 判断包围盒与球体是否相交
        /// </summary>
        /// <param name="box">包围盒对象</param>
        /// <param name="sphere">球体对象</param>
        /// <returns>如果相交返回true，否则返回false</returns>
        [Rtti.Meta]
        public static bool Intersects(in BoundingBox box, in BoundingSphere sphere)
        {
            Vector3 clamped;

            Vector3.Clamp(in sphere.Center, in box.Minimum, in box.Maximum, out clamped);

            float x = sphere.Center.X - clamped.X;
            float y = sphere.Center.Y - clamped.Y;
            float z = sphere.Center.Z - clamped.Z;

            float dist = (x * x) + (y * y) + (z * z);
            var ret = (dist <= (sphere.Radius * sphere.Radius));            
            return ret;
        }
        public static bool Intersects(in BoundingBox box, in BoundingSphere sphere, out float dist)
        {
            Vector3 clamped;

            Vector3.Clamp(in sphere.Center, in box.Minimum, in box.Maximum, out clamped);

            float x = sphere.Center.X - clamped.X;
            float y = sphere.Center.Y - clamped.Y;
            float z = sphere.Center.Z - clamped.Z;

            dist = (x * x) + (y * y) + (z * z);
            var ret = (dist <= (sphere.Radius * sphere.Radius));
            dist = Vector3.DistanceSquared(sphere.Center, box.GetCenter());

            return ret;
        }
        //bool Intersects( BoundingBox box, Ray ray, out float distance )
        //{
        //    return Ray.Intersects( ray, box, distance );
        //}
        /// <summary>
        /// 判断包围盒和面的碰撞
        /// </summary>
        /// <param name="box">包围盒对象</param>
        /// <param name="plane">面对象</param>
        /// <returns>发生碰撞返回true，否则返回false</returns>
        [Rtti.Meta]
        public static PlaneIntersectionType Intersects(BoundingBox box, Plane plane)
	    {
		    return Plane.Intersects( plane, box);
	    }
        /// <summary>
        /// 重载操作符
        /// </summary>
        /// <param name="left">包围盒对象</param>
        /// <param name="right">包围盒对象</param>
        /// <returns>如果相等返回true，否则返回false</returns>
        public static bool operator ==(BoundingBox left, BoundingBox right)
	    {
		    return Equals( left, right );
	    }
        /// <summary>
        /// 重载操作符！=
        /// </summary>
        /// <param name="left">包围盒对象</param>
        /// <param name="right">包围盒对象</param>
        /// <returns>如果不相等返回true，否则返回false</returns>
        public static bool operator !=(BoundingBox left, BoundingBox right)
	    {
		    return !Equals( left, right );
	    }
        /// <summary>
        /// 转换成string类型
        /// </summary>
        /// <returns>返回转换成的string</returns>
        public override String ToString()
	    {
		    return String.Format( CultureInfo.CurrentCulture, "Minimum:{0} Maximum:{1}", Minimum.ToString(), Maximum.ToString() );
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
		    if( value == null )
			    return false;

		    if( value.GetType() != GetType() )
			    return false;

		    return Equals( (BoundingBox)( value ) );
	    }
        /// <summary>
        /// 包围盒对象是否相同
        /// </summary>
        /// <param name="value">包围盒对象实例</param>
        /// <returns>如果相同返回true，否则返回false</returns>
        public bool Equals(BoundingBox value)
	    {
		    return ( Minimum == value.Minimum && Maximum == value.Maximum );
	    }
        /// <summary>
        /// 判断两个包围盒对象是否相同
        /// </summary>
        /// <param name="value1">包围盒对象1</param>
        /// <param name="value2">包围盒对象2</param>
        /// <returns>如果相同返回true，否则返回false</returns>
        public static bool Equals(in BoundingBox value1, in BoundingBox value2)
	    {
		    return ( value1.Minimum == value2.Minimum && value1.Maximum == value2.Maximum );
	    }
        public static BoundingBox Transform(in BoundingBox srcBox, in Matrix matrix)
        {
            BoundingBox result;
            Transform(in srcBox, in matrix, out result);
            return result;
        }
        public static BoundingBox Transform(in BoundingBox srcBox, in FTransform matrix)
        {
            BoundingBox result;
            Transform(in srcBox, in matrix, out result);
            return result;
        }
        public static BoundingBox TransformNoScale(in BoundingBox srcBox, in FTransform matrix)
        {
            BoundingBox result;
            TransformNoScale(in srcBox, in matrix, out result);
            return result;
        }
        //http://dev.theomader.com/transform-bounding-boxes/
        public static void Transform(in BoundingBox srcBox, in Matrix matrix, out BoundingBox result)
        {
            //var right = matrix.Right;
            //var up = matrix.Up;
            //var forward = matrix.Forward;
            //var trans = matrix.Translation;

            //var xa = right * srcBox.Minimum.X;
            //var xb = right * srcBox.Maximum.X;

            //var ya = up * srcBox.Minimum.Y;
            //var yb = up * srcBox.Maximum.Y;

            //var za = forward * srcBox.Minimum.Z;
            //var zb = forward * srcBox.Maximum.Z;

            //result.Minimum = Vector3.Minimize(xa, xb) + Vector3.Minimize(ya, yb) + Vector3.Minimize(za, zb) + trans;
            //result.Maximum = Vector3.Maximize(xa, xb) + Vector3.Maximize(ya, yb) + Vector3.Maximize(za, zb) + trans;

            result.Minimum = new Vector3(float.MaxValue);
            result.Maximum = new Vector3(-float.MaxValue);
            var corners = srcBox.GetCorners();
            for(int i=1; i<corners.Length; i++)
            {
                Vector3.TransformCoordinate(in corners[i], in matrix, out corners[i]);
                Vector3.Minimize(in result.Minimum, in corners[i], out result.Minimum);
                Vector3.Maximize(in result.Maximum, in corners[i], out result.Maximum);
            }
        }
        public static void Transform(in BoundingBox srcBox, in FTransform transform, out BoundingBox result)
        {
            result.Minimum = new Vector3(float.MaxValue);
            result.Maximum = new Vector3(-float.MaxValue);
            var corners = srcBox.GetDCorners();
            for (int i = 1; i < corners.Length; i++)
            {
                corners[i] = transform.TransformPosition(in corners[i]);
                var v = corners[i].ToSingleVector3();
                Vector3.Minimize(in result.Minimum, in v, out result.Minimum);
                Vector3.Maximize(in result.Maximum, in v, out result.Maximum);
            }
        }
        public static void TransformNoScale(in BoundingBox srcBox, in FTransform transform, out BoundingBox result)
        {
            result.Minimum = new Vector3(float.MaxValue);
            result.Maximum = new Vector3(-float.MaxValue);
            var corners = srcBox.GetDCorners();
            for (int i = 1; i < corners.Length; i++)
            {
                corners[i] = transform.TransformPositionNoScale(in corners[i]);
                var v = corners[i].ToSingleVector3();
                Vector3.Minimize(in result.Minimum, in v, out result.Minimum);
                Vector3.Maximize(in result.Maximum, in v, out result.Maximum);
            }
        }        
    }
}
