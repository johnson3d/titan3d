using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS
{
    public struct BoundingCapsule
    {
        public float Length;
        public float Radius;
        public Vector3 P0
        {//胶囊端点球中心0
            get
            {
                return new Vector3(0, Length * 0.5f, 0);
            }
        }
        public Vector3 P1
        {//胶囊端点球中心1
            get
            {
                return new Vector3(0, -Length * 0.5f, 0);
            }
        }
        public bool Intersect(ref BoundingSphere sphere)
        {
            Vector3 particlePosition = sphere.Center;
            float particleRadius = sphere.Radius;
            Vector3 capsuleP0 = this.P0;
            Vector3 capsuleP1 = this.P1;
            float capsuleRadius = Radius;
            float r = capsuleRadius + particleRadius;
            float r2 = r * r;
            Vector3 dir = capsuleP1 - capsuleP0;
            Vector3 d = particlePosition - capsuleP0;
            float t = Vector3.Dot(d, dir);

            if (t <= 0)
            {
                // check sphere1
                float len2 = d.Length();
                if (len2 > 0 && len2 < r2)
                {
                    //float len = (float)Math.Sqrt(len2);
                    //particlePosition = capsuleP0 + d * (r / len);
                    return true;
                }
            }
            else
            {
                float dl = dir.Length();
                if (t >= dl)
                {
                    // check sphere2
                    d = particlePosition - capsuleP1;
                    float len2 = d.Length();
                    if (len2 > 0 && len2 < r2)
                    {
                        //float len = (float)Math.Sqrt(len2);
                        //particlePosition = capsuleP1 + d * (r / len);
                        return true;
                    }
                }
                else if (dl > 0)
                {
                    // check cylinder
                    t /= dl;
                    d -= dir * t;
                    float len2 = d.Length();
                    if (len2 > 0 && len2 < r2)
                    {
                        //float len = (float)Math.Sqrt(len2);
                        //particlePosition += d * ((r - len) / len);
                        return true;
                    }
                }
            }

            return false;
        }
    }
}
