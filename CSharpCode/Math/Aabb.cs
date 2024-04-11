using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS
{
    public struct Aabb
    {
        public DVector3 Center;
        public Vector3 Extent;
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
        public Aabb(in DVector3 center, in Vector3 extent)
        {
            Center = center;
            Extent = extent;
        }
        public Aabb(in DVector3 min, in DVector3 max)
        {
            Center = (min + max) * 0.5f;
            Extent = (max - min).ToSingleVector3() * 0.5f;
        }
        public Aabb(in DBoundingBox box)
        {
            Center = box.GetCenter();
            Extent = box.GetSize().ToSingleVector3() * 0.5f;
        }
        public DBoundingBox AsBoundingBox()
        {
            return new DBoundingBox(Center + Extent, Center - Extent);
        }
        public static void CalculateClosestPointInBox(in DVector3 point, in Aabb AABB, out DVector3 outPoint, out double outSqrDistance)
        {
            // compute coordinates of point in box coordinate system
            var closest = point - AABB.Center;

            var halfSize = AABB.Extent.AsDVector();
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
                outPoint = closest + AABB.Center;
                outSqrDistance = fSqrDistance;
            }
        }
        public DVector3 ClosestPoint(in DVector3 pos)
        {
            DVector3 result;
            CalculateClosestPointInBox(in pos, in this, out result, out var sqrDist);
            return result;
        }
        public void Expand(in Vector3 size)
        {
            Extent = Vector3.Maximize(in Extent, size);
        }
        public static bool IsIntersect(in Aabb box1, in Aabb box2)
        {
            if (MathHelper.Abs(box1.Center.X - box2.Center.X) > box1.Extent.X + box2.Extent.Y)
            {
                return false;
            }
            if (MathHelper.Abs(box1.Center.Y - box2.Center.Y) > box1.Extent.Y + box2.Extent.Y)
            {
                return false;
            }
            if (MathHelper.Abs(box1.Center.Z - box2.Center.Z) > box1.Extent.Z + box2.Extent.Z)
            {
                return false;
            }

            return true;
        }
        public static bool IsContain(in Aabb box1, in DVector3 pos)
        {
            return true;
        }
        public bool IsContain(in DVector3 pos)
        {
            return IsContain(in this, pos);
        }
    }
}
