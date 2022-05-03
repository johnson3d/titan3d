using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS
{
    public class Bezier3D
    {
        public Vector3 mStart;//a
        public Vector3 mStartCtrl;//b
        public Vector3 mEndCtrl;//c
        public Vector3 mEnd;//d
        public Vector3 Start { get => mStart; }
        public Vector3 StartCtrl { get => mStartCtrl; }
        public Vector3 EndCtrl { get => mEndCtrl; }
        public Vector3 End { get => mEnd; }

        public float mLength;
        public float Length { get => mLength; }
        public Bezier3D(in Vector3 start, in Vector3 end, in Vector3 sCtrl, in Vector3 eCtrl, int Segments = 100)
        {
            Set(start, end, sCtrl, eCtrl, Segments);
        }
        public class UPointCache
        {
            public struct FBZPoint
            {
                public Vector3 Position;
                public Vector3 Forward;
            }
            public FBZPoint[] CachedPoints;
            public BoundingBox AABB;
            public void BuildCache(Bezier3D curve, int Segments = 100)
            {
                Vector3 prevPos = curve.GetValue(0);
                curve.mLength = 0f;
                //float step = 1 / (float)Segments;            
                CachedPoints = new FBZPoint[Segments + 1];
                CachedPoints[0].Position = prevPos;
                CachedPoints[0].Forward = curve.GetForward(0);
                AABB.InitEmptyBox();
                AABB.Merge(in prevPos);
                for (int i = 1; i <= Segments; i++)
                {
                    float t = (float)i / (float)Segments;
                    Vector3 newPos = curve.GetValue(t);
                    float segmentLength = Vector3.Distance(prevPos, newPos);
                    curve.mLength += segmentLength;
                    prevPos = newPos;
                    CachedPoints[i].Position = newPos;
                    CachedPoints[i].Forward = curve.GetForward(t);
                    AABB.Merge(in prevPos);
                }
            }
        }

        private UPointCache PointCache = null;
        public UPointCache GetPointCache(int Segments = 100)
        {
            if (PointCache == null)
            {
                PointCache = new UPointCache();
                PointCache.BuildCache(this, Segments);
            }
            return PointCache;
        }
        public void Set(in Vector3 start, in Vector3 end, in Vector3 sCtrl, in Vector3 eCtrl, int Segments = 100)
        {
            mStart = start;
            mEnd = end;
            mStartCtrl = sCtrl;
            mEndCtrl = eCtrl;

            mLength = CalculateLength(Segments);
        }

        //t = [0 - 1]
        public Vector3 GetValue(float t)
        {
            var inv_t =  1f - t;
            float inv_t_2 = inv_t * inv_t;
            float inv_t_3 = inv_t_2 * inv_t;
            float t_2 = t * t;
            return (inv_t_3) * mStart + (3 * t * inv_t_2) * mStartCtrl + (3 * inv_t * t_2) * mEndCtrl  + (t_2 * t) * mEnd;
        }
        //t = [0 - 1]
        public Vector3 GetForward(float t)
        { 
            float oneMinusT = 1f - t;
            return 3f * oneMinusT * oneMinusT * (mStartCtrl - mStart) +
                6f * oneMinusT * t * (mEndCtrl - mStartCtrl) +
                3f * t * t * (mEnd - mEndCtrl);
        }
        public float CalculateLength(int Segments = 100)
        {
            Vector3 prevPos = GetValue(0);
            float totalLength = 0f;
            //float step = 1 / (float)Segments;            
            for (int i = 1; i <= Segments; i++)
            {
                float t = (float)i / (float)Segments;
                Vector3 newPos = GetValue(t);
                float segmentLength = Vector3.Distance(prevPos, newPos);
                totalLength += segmentLength;
                prevPos = newPos;
            }
            return totalLength;
        }
    }

    public class UBezier3DSpline : IO.BaseSerializer
    {
        public bool IsDirty = true;
        public int Segments { get; set; } = 100;
        public List<Bezier3D> Curves { get; set; } = new List<Bezier3D>();
        public float Length
        {
            get;
            private set;
        }
        private void UpdateLength()
        {
            Length = 0;
            foreach (var i in Curves)
            {
                Length += i.Length;
            }
            IsDirty = true;
        }
        public bool Intesect(in Vector3 pos, out Vector3 hitPos, out Vector3 forward, float tolerance = 0.01f)
        {
            foreach (var i in Curves)
            {
                var cache = i.GetPointCache(Segments);
                if (cache.AABB.Contains(in pos) != ContainmentType.Disjoint)
                {
                    for (int j = 0; j < cache.CachedPoints.Length - 1; j++)
                    {
                        ref var start = ref cache.CachedPoints[j].Position;
                        ref var end = ref cache.CachedPoints[j+1].Position;
                        var dir = end - start;
                        var dirLen = dir.Normalize();
                        float len;
                        var distSq = Vector3.RayDistanceSquared(in pos, in start, in dir, out len);
                        if (len >= 0 && len <= dirLen && distSq <= tolerance)
                        {
                            hitPos = start + dir * len;
                            forward = dir;
                            return true;
                        }
                    }
                }
            }
            hitPos = pos;
            forward = Vector3.Zero;
            return false;
        }
        public bool Intesect(in Vector3 rayStart, in Vector3 rayEnd, out Vector3 hitPos, out Vector3 forward, float tolerance = 0.01f)
        {
            var rayDir = rayEnd - rayStart;
            var rayLen = rayDir.Normalize();
            var ray = new Ray(rayStart, rayDir);
            var helper = new LinesDistanceHelper();
            helper.SetLineA(in rayStart, in rayEnd);
            foreach (var i in Curves)
            {
                var cache = i.GetPointCache(Segments);

                float intersectDist;
                if (Ray.Intersects(in ray, in cache.AABB, out intersectDist) && intersectDist >= 0 && intersectDist <= rayLen)
                {
                    for (int j = 0; j < cache.CachedPoints.Length - 1; j++)
                    {
                        ref var start = ref cache.CachedPoints[j].Position;
                        var end = cache.CachedPoints[j + 1].Position;
                        var segDir = end - start;
                        var segLen = segDir.Length();

                        helper.SetLineB(in start, in end);

                        double t1, t2;
                        helper.GetDistance(out t1, out t2);
                        //var ponA = helper.GetPonA();
                        //var ponB = helper.GetPonB();
                        if (helper.distance < tolerance && t1 >=0 && t2 >= 0 && t1 <= rayLen && t2 <= segLen)
                        {
                            hitPos = helper.GetPonB();
                            forward = hitPos - start;
                            forward.Normalize();
                            return true;
                        }
                    }
                }
            }
            hitPos = Vector3.Zero;
            forward = Vector3.Zero;
            return false;
        }
        public void AppendPoint(in Vector3 pos, in Vector3 sCtrl, in Vector3 eCtrl)
        {
            if (Curves.Count == 0)
            {
                Bezier3D tmp = new Bezier3D(in pos, in pos, in sCtrl, in eCtrl, 0);
                Curves.Add(tmp);
            }
            else
            {
                Bezier3D tmp = new Bezier3D(in Curves[Curves.Count - 1].mEnd, in pos, in sCtrl, in eCtrl, Segments);
                Curves.Add(tmp);
            }
            UpdateLength();
        }
        public void AppendPoint(in Vector3 pos, in Vector3 eCtrl)
        {
            if (Curves.Count == 0)
            {
                Bezier3D tmp = new Bezier3D(in pos, in pos, in eCtrl, in eCtrl, 0);
                Curves.Add(tmp);
            }
            else
            {
                var prevCurve = Curves[Curves.Count - 1];
                var delta = prevCurve.End * 2 - prevCurve.EndCtrl;
                Bezier3D tmp = new Bezier3D(in prevCurve.mEnd, in pos, in delta, in eCtrl, Segments);
                Curves.Add(tmp);
            }
            UpdateLength();
        }
        public bool InsertPoint(int index, in Vector3 pos, in Vector3 leftCtrl, in Vector3 rightCtrl)
        {
            if (index < 0 || index >= Curves.Count - 1)
                return false;
            var cur = Curves[index];
            Bezier3D left = new Bezier3D(in cur.mStart, in pos, in cur.mStartCtrl, in leftCtrl, Segments);
            Bezier3D right = new Bezier3D(in pos, in cur.mEnd, in rightCtrl, in cur.mEndCtrl, Segments);
            Curves[index] = left;
            Curves.Insert(index, right);

            UpdateLength();
            return true;
        }
        public bool RemovePoint(int index, int Segments = 100)
        {
            if (index <= 0 || index >= Curves.Count - 1)
                return false;

            var left = Curves[index];
            var right = Curves[index + 1];

            right.Set(left.Start, right.End, left.StartCtrl, right.EndCtrl, Segments);
            Curves.RemoveAt(index);

            UpdateLength();
            return true;
        }
        public bool SetPoint(int index, in Vector3 pos, in Vector3 leftCtrl, in Vector3 rightCtrl)
        {
            if (index < 0 || index > Curves.Count - 1)
                return false;

            if (index == 0)
            {
                Curves[index].Set(in pos, in pos, in leftCtrl, in rightCtrl, Segments);
                UpdateLength();
                return true;
            }

            var left = Curves[index - 1];
            var right = Curves[index];

            left.Set(in left.mStart, in pos, in left.mStartCtrl, in leftCtrl, Segments);
            right.Set(in pos, in right.mEnd, in rightCtrl, in right.mEndCtrl, Segments);

            UpdateLength();
            return true;
        }
        //index start from 1;
        public Vector3 GetPointPos(int index)
        {
            return Curves[index - 1].mEnd;
        }
        public Vector3 GetPointLeftCtrl(int index)
        {
            if (index == 0)
                return Curves[0].mStart;
            return Curves[index - 1].mEndCtrl;
        }
        public Vector3 GetPointRightCtrl(int index)
        {
            if (index >= Curves.Count)
            {
                return Curves[Curves.Count - 1].mEndCtrl;
            }
            return Curves[index].mStartCtrl;
        }
        public Vector3 GetValue(float distance)
        {
            if (distance < 0 || distance >= Length)
                return Vector3.Zero;
            foreach (var i in Curves)
            {
                if (i.Length > distance)
                {
                    return i.GetValue(distance / i.Length);
                }
                else
                {
                    distance -= i.Length;
                }
            }
            return Vector3.Zero;
        }
        public Vector3 GetForword(float distance)
        {
            if (distance < 0 || distance >= Length)
                return Vector3.Zero;
            foreach (var i in Curves)
            {
                if (i.Length > distance)
                {
                    return i.GetForward(distance / i.Length);
                }
                else
                {
                    distance -= i.Length;
                }
            }
            return Vector3.Zero;
        }
        public bool GetValueAndForword(float distance, out Vector3 pos, out Vector3 dir)
        {
            if (distance < 0 || distance >= Length)
            {
                pos = Vector3.Zero;
                dir = Vector3.Zero;
                return false;
            }
            foreach (var i in Curves)
            {
                if (i.Length > distance)
                {
                    var t = distance / i.Length;
                    pos = i.GetValue(t);
                    dir = i.GetForward(t);
                    return true;
                }
                else
                {
                    distance -= i.Length;
                }
            }
            pos = Vector3.Zero;
            dir = Vector3.Zero;
            return false;
        }
    }
}
