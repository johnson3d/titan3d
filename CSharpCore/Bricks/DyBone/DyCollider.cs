using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.DyBone
{
    public enum EColliderType
    {
        Unknonw,
        OutSideSphere,
        InSideSphere,
        OutSideCapsule,
        InSideCapsule,
    }
    public struct DyCollider
    {
        public void SetDefault()
        {
            Transform = Matrix.Identity;
            InvertTransform = Matrix.Identity;
            Flags = EColliderType.Unknonw;
        }
        public EColliderType Flags;
        public BoundingSphere BVSphere;
        public BoundingCapsule BVCapsule;
        public Matrix Transform;
        public Matrix InvertTransform;
        public void SetTransform(Matrix mt)
        {
            Transform = mt;
            InvertTransform = Matrix.Invert(ref mt);
        }
        public void Collide(ref DyBone.Particle p)
        {
            //p transform Collider space
            p.Position = Vector3.TransformCoordinate(p.Position, InvertTransform);
            
            switch (Flags)
            {
                case EColliderType.OutSideCapsule:
                    {
                        OutsideCapsule(ref p.Position, p.Radius, BVCapsule.P0, BVCapsule.P1, BVCapsule.Radius);
                    }
                    break;
                case EColliderType.InSideCapsule:
                    {
                        InsideCapsule(ref p.Position, p.Radius, BVCapsule.P0, BVCapsule.P1, BVCapsule.Radius);
                    }
                    break;
                case EColliderType.OutSideSphere:
                    {
                        var dir = p.Position - BVSphere.Center;
                        var dist = dir.Length();
                        float touchDist = p.Radius + BVSphere.Radius;
                        if (dist > touchDist)
                            break;
                        dir /= dist;
                        p.Position = BVSphere.Center + dir * touchDist;
                    }
                    break;
                case EColliderType.InSideSphere:
                    {
                        var dir = p.Position - BVSphere.Center;
                        var dist = dir.Length();
                        float touchDist = BVSphere.Radius - p.Radius;
                        if (dist < touchDist)
                            break;
                        dir /= dist;
                        p.Position = BVSphere.Center + dir * touchDist;
                    }
                    break;
            }

            //p transform world space
            p.Position = Vector3.TransformCoordinate(p.Position, Transform);
        }

        static bool OutsideCapsule(ref Vector3 particlePosition, float particleRadius, Vector3 capsuleP0, Vector3 capsuleP1, float capsuleRadius)
        {
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
                    float len = (float)Math.Sqrt(len2);
                    particlePosition = capsuleP0 + d * (r / len);
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
                        float len = (float)Math.Sqrt(len2);
                        particlePosition = capsuleP1 + d * (r / len);
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
                        float len = (float)Math.Sqrt(len2);
                        particlePosition += d * ((r - len) / len);
                        return true;
                    }
                }
            }

            return false;
        }
        static bool InsideCapsule(ref Vector3 particlePosition, float particleRadius, Vector3 capsuleP0, Vector3 capsuleP1, float capsuleRadius)
        {
            float r = capsuleRadius - particleRadius;
            float r2 = r * r;
            Vector3 dir = capsuleP1 - capsuleP0;
            Vector3 d = particlePosition - capsuleP0;
            float t = Vector3.Dot(d, dir);

            if (t <= 0)
            {
                // check sphere1
                float len2 = d.Length();
                if (len2 > r2)
                {
                    float len = (float)Math.Sqrt(len2);
                    particlePosition = capsuleP0 + d * (r / len);
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
                    if (len2 > r2)
                    {
                        float len = (float)Math.Sqrt(len2);
                        particlePosition = capsuleP1 + d * (r / len);
                        return true;
                    }
                }
                else if (dl > 0)
                {
                    // check cylinder
                    t /= dl;
                    d -= dir * t;
                    float len2 = d.Length();
                    if (len2 > r2)
                    {
                        float len = (float)Math.Sqrt(len2);
                        particlePosition += d * ((r - len) / len);
                        return true;
                    }
                }
            }

            return false;
        }
    }
    
    public class DyColliderSet
    {
        public DyCollider[] Colliders = new DyCollider[64];
        public void Collide(ref DyBone.Particle p)
        {
            for(int i=0; i<64; i++)
            {
                if (Colliders[i].Flags == EColliderType.Unknonw)
                    continue;

                if(p.PVS!=0)
                {
                    if ((p.PVS & ((UInt64)(1 << i))) == 0)
                        continue;
                }

                Colliders[i].Collide(ref p);
            }
        }
    }
}
