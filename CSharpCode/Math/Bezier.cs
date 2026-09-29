using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS
{
    /// <summary>
    /// 三次 Bézier 样条的一个锚点及其左右控制柄。
    /// 该类型是样条的持久化数据单元，曲线段与采样缓存均由它派生。
    /// </summary>
    [Rtti.Meta("")]
    public class TtBezier3DPoint : IO.BaseSerializer
    {
        [Rtti.Meta("")]
        public Vector3 Position { get; set; }
        [Rtti.Meta("")]
        public Vector3 LeftControl { get; set; }
        [Rtti.Meta("")]
        public Vector3 RightControl { get; set; }

        public TtBezier3DPoint()
        {
        }

        public TtBezier3DPoint(in Vector3 position, in Vector3 leftControl, in Vector3 rightControl)
        {
            Position = position;
            LeftControl = leftControl;
            RightControl = rightControl;
        }

        public TtBezier3DPoint ClonePoint()
        {
            return new TtBezier3DPoint(Position, LeftControl, RightControl);
        }
    }

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
                public float Distance;
            }
            public FBZPoint[] CachedPoints;
            public BoundingBox AABB;
            public int Segments { get; private set; }
            public void BuildCache(Bezier3D curve, int segments = 100)
            {
                Segments = Math.Max(1, segments);
                Vector3 prevPos = curve.GetValue(0);
                curve.mLength = 0f;
                CachedPoints = new FBZPoint[Segments + 1];
                CachedPoints[0].Position = prevPos;
                CachedPoints[0].Forward = curve.GetForward(0);
                CachedPoints[0].Distance = 0.0f;
                AABB.InitEmptyBox();
                AABB.Merge(in prevPos);
                for (int i = 1; i <= Segments; i++)
                {
                    float t = (float)i / Segments;
                    Vector3 newPos = curve.GetValue(t);
                    curve.mLength += Vector3.Distance(prevPos, newPos);
                    prevPos = newPos;
                    CachedPoints[i].Position = newPos;
                    CachedPoints[i].Forward = curve.GetForward(t);
                    CachedPoints[i].Distance = curve.mLength;
                    AABB.Merge(in prevPos);
                }
            }
        }

        private UPointCache PointCache = null;
        public UPointCache GetPointCache(int segments = 100)
        {
            if (PointCache == null)
            {
                PointCache = new UPointCache();
                PointCache.BuildCache(this, Math.Max(1, segments));
            }
            return PointCache;
        }
        public void Set(in Vector3 start, in Vector3 end, in Vector3 sCtrl, in Vector3 eCtrl, int Segments = 100)
        {
            mStart = start;
            mEnd = end;
            mStartCtrl = sCtrl;
            mEndCtrl = eCtrl;
            PointCache = null;

            if (Segments > 0)
                GetPointCache(Segments);
            else
                mLength = 0.0f;
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

        public float GetParameterAtDistance(float distance)
        {
            var cache = PointCache ?? GetPointCache();
            if (cache.CachedPoints.Length <= 1 || mLength <= 0.0f)
                return 0.0f;
            distance = MathHelper.Clamp(distance, 0.0f, mLength);
            if (distance <= 0.0f)
                return 0.0f;
            if (distance >= mLength)
                return 1.0f;

            int low = 1;
            int high = cache.CachedPoints.Length - 1;
            while (low < high)
            {
                int middle = low + (high - low) / 2;
                if (cache.CachedPoints[middle].Distance < distance)
                    low = middle + 1;
                else
                    high = middle;
            }

            int upperIndex = low;
            int lowerIndex = upperIndex - 1;
            float lowerDistance = cache.CachedPoints[lowerIndex].Distance;
            float upperDistance = cache.CachedPoints[upperIndex].Distance;
            float interval = upperDistance - lowerDistance;
            float fraction = interval > 1e-12f ? (distance - lowerDistance) / interval : 0.0f;
            return (lowerIndex + fraction) / cache.Segments;
        }
    }

    public sealed class TtBezier3DSplineSnapshot
    {
        public readonly List<TtBezier3DPoint> Points;
        public readonly bool IsClosed;
        public readonly int Segments;
        public readonly ulong Revision;

        public TtBezier3DSplineSnapshot(IReadOnlyList<TtBezier3DPoint> points, bool isClosed,
            int segments, ulong revision)
        {
            Points = new List<TtBezier3DPoint>(points?.Count ?? 0);
            if (points != null)
            {
                for (int i = 0; i < points.Count; i++)
                    Points.Add(points[i].ClonePoint());
            }
            IsClosed = isClosed;
            Segments = Math.Max(1, segments);
            Revision = revision;
        }
    }

    [Rtti.Meta("")]
    public class TtBezier3DSpline : IO.BaseSerializer
    {
        int mSegments = 100;
        bool mIsClosed;
        ulong mRevision;
        List<TtBezier3DPoint> mPoints = new List<TtBezier3DPoint>();
        readonly List<Bezier3D> mCurves = new List<Bezier3D>();
        bool mDerivedDataDirty = true;

        [System.ComponentModel.Browsable(false)]
        public bool IsDirty { get; set; } = true;

        [Rtti.Meta("")]
        public int Segments
        {
            get => mSegments;
            set
            {
                var newValue = Math.Max(1, value);
                if (mSegments == newValue)
                    return;
                mSegments = newValue;
                MarkDirty();
            }
        }

        [Rtti.Meta("")]
        public bool IsClosed
        {
            get => mIsClosed;
            set
            {
                if (mIsClosed == value)
                    return;
                mIsClosed = value;
                MarkDirty();
            }
        }

        [Rtti.Meta("")]
        public List<TtBezier3DPoint> Points
        {
            get => mPoints;
            set
            {
                mPoints = value ?? new List<TtBezier3DPoint>();
                MarkDirty();
            }
        }

        [System.ComponentModel.Browsable(false)]
        public List<Bezier3D> Curves
        {
            get
            {
                EnsureDerivedData();
                return mCurves;
            }
        }

        [System.ComponentModel.Browsable(false)]
        public int PointCount => mPoints?.Count ?? 0;

        [System.ComponentModel.Browsable(false)]
        public int SegmentCount
        {
            get
            {
                EnsureDerivedData();
                return mCurves.Count;
            }
        }

        [System.ComponentModel.Browsable(false)]
        public float Length
        {
            get
            {
                EnsureDerivedData();
                return mLength;
            }
        }
        float mLength;

        [System.ComponentModel.Browsable(false)]
        public ulong Revision => mRevision;

        void MarkDirty()
        {
            mDerivedDataDirty = true;
            IsDirty = true;
            mRevision++;
        }

        void EnsureDerivedData()
        {
            if (mDerivedDataDirty)
                RebuildDerivedData();
        }

        void RebuildDerivedData()
        {
            mCurves.Clear();
            mLength = 0.0f;
            if (mPoints == null || mPoints.Count == 0)
            {
                mDerivedDataDirty = false;
                IsDirty = true;
                return;
            }

            for (int i = 1; i < mPoints.Count; i++)
                AddDerivedCurve(mPoints[i - 1], mPoints[i]);
            if (mIsClosed && mPoints.Count >= 3)
                AddDerivedCurve(mPoints[mPoints.Count - 1], mPoints[0]);
            mDerivedDataDirty = false;
            IsDirty = true;
        }

        void AddDerivedCurve(TtBezier3DPoint start, TtBezier3DPoint end)
        {
            var curve = new Bezier3D(start.Position, end.Position,
                start.RightControl, end.LeftControl, mSegments);
            mCurves.Add(curve);
            mLength += curve.Length;
        }

        public override void OnPostRead(object tagObj, object hostObj, bool fromXml)
        {
            base.OnPostRead(tagObj, hostObj, fromXml);
            MarkDirty();
            EnsureDerivedData();
        }

        public TtBezier3DSplineSnapshot CreateSnapshot()
        {
            return new TtBezier3DSplineSnapshot(mPoints, mIsClosed, mSegments, mRevision);
        }

        public void RestoreSnapshot(TtBezier3DSplineSnapshot snapshot)
        {
            if (snapshot == null)
                return;
            mPoints = ClonePoints(snapshot.Points);
            mIsClosed = snapshot.IsClosed;
            mSegments = Math.Max(1, snapshot.Segments);
            MarkDirty();
            EnsureDerivedData();
        }

        public void RestoreSnapshot(IReadOnlyList<TtBezier3DPoint> points)
        {
            mPoints = ClonePoints(points);
            MarkDirty();
            EnsureDerivedData();
        }

        static List<TtBezier3DPoint> ClonePoints(IReadOnlyList<TtBezier3DPoint> points)
        {
            var result = new List<TtBezier3DPoint>(points?.Count ?? 0);
            if (points != null)
            {
                for (int i = 0; i < points.Count; i++)
                    result.Add(points[i].ClonePoint());
            }
            return result;
        }

        public bool TryGetPoint(int index, out TtBezier3DPoint point)
        {
            if (index < 0 || index >= PointCount)
            {
                point = null;
                return false;
            }
            point = mPoints[index];
            return true;
        }

        public void AppendPoint(in Vector3 pos, in Vector3 leftCtrl, in Vector3 rightCtrl)
        {
            mPoints.Add(new TtBezier3DPoint(in pos, in leftCtrl, in rightCtrl));
            MarkDirty();
            EnsureDerivedData();
        }

        public void AppendPoint(in Vector3 pos, in Vector3 leftCtrl)
        {
            var rightCtrl = pos * 2.0f - leftCtrl;
            AppendPoint(in pos, in leftCtrl, in rightCtrl);
        }

        public bool InsertPoint(int index, in Vector3 pos, in Vector3 leftCtrl, in Vector3 rightCtrl)
        {
            if (index < 0 || index > PointCount)
                return false;
            mPoints.Insert(index, new TtBezier3DPoint(in pos, in leftCtrl, in rightCtrl));
            MarkDirty();
            EnsureDerivedData();
            return true;
        }

        public bool RemovePoint(int index, int segments = 100)
        {
            if (index < 0 || index >= PointCount)
                return false;
            mPoints.RemoveAt(index);
            MarkDirty();
            EnsureDerivedData();
            return true;
        }

        public bool SetPoint(int index, in Vector3 pos, in Vector3 leftCtrl, in Vector3 rightCtrl)
        {
            if (!TryGetPoint(index, out var point))
                return false;
            point.Position = pos;
            point.LeftControl = leftCtrl;
            point.RightControl = rightCtrl;
            MarkDirty();
            EnsureDerivedData();
            return true;
        }

        public bool SetPointPosition(int index, in Vector3 pos, bool moveControls = true)
        {
            if (!TryGetPoint(index, out var point))
                return false;
            var delta = pos - point.Position;
            point.Position = pos;
            if (moveControls)
            {
                point.LeftControl += delta;
                point.RightControl += delta;
            }
            MarkDirty();
            EnsureDerivedData();
            return true;
        }

        public bool SetPointLeftCtrl(int index, in Vector3 value)
        {
            if (!TryGetPoint(index, out var point))
                return false;
            point.LeftControl = value;
            MarkDirty();
            EnsureDerivedData();
            return true;
        }

        public bool SetPointRightCtrl(int index, in Vector3 value)
        {
            if (!TryGetPoint(index, out var point))
                return false;
            point.RightControl = value;
            MarkDirty();
            EnsureDerivedData();
            return true;
        }

        public bool SetPointTangent(int index, bool isLeftTangent, in Vector3 value)
        {
            if (!TryGetPoint(index, out var point))
                return false;
            if (isLeftTangent)
            {
                point.LeftControl = value;
                point.RightControl = point.Position * 2.0f - value;
            }
            else
            {
                point.RightControl = value;
                point.LeftControl = point.Position * 2.0f - value;
            }
            MarkDirty();
            EnsureDerivedData();
            return true;
        }

        public Vector3 GetPointPos(int index) => mPoints[index].Position;
        public Vector3 GetPointLeftCtrl(int index) => mPoints[index].LeftControl;
        public Vector3 GetPointRightCtrl(int index) => mPoints[index].RightControl;

        public bool SplitSegmentAfter(int pointIndex, float t = 0.5f)
        {
            if (pointIndex < 0 || pointIndex >= PointCount)
                return false;
            var rightIndex = pointIndex + 1;
            if (rightIndex >= PointCount)
            {
                if (!mIsClosed || PointCount < 3)
                    return false;
                rightIndex = 0;
            }
            t = MathHelper.Clamp(t, 0.0f, 1.0f);
            var leftPoint = mPoints[pointIndex];
            var rightPoint = mPoints[rightIndex];
            var a = Vector3.Lerp(leftPoint.Position, leftPoint.RightControl, t);
            var b = Vector3.Lerp(leftPoint.RightControl, rightPoint.LeftControl, t);
            var c = Vector3.Lerp(rightPoint.LeftControl, rightPoint.Position, t);
            var d = Vector3.Lerp(a, b, t);
            var e = Vector3.Lerp(b, c, t);
            var position = Vector3.Lerp(d, e, t);

            leftPoint.RightControl = a;
            rightPoint.LeftControl = c;
            mPoints.Insert(pointIndex + 1, new TtBezier3DPoint(in position, in d, in e));
            MarkDirty();
            EnsureDerivedData();
            return true;
        }

        public bool Intesect(in Vector3 pos, out Vector3 hitPos, out Vector3 forward, float tolerance = 0.01f)
        {
            foreach (var curve in Curves)
            {
                if (curve.Start == curve.End)
                    continue;
                var cache = curve.GetPointCache(Segments);
                if (cache.AABB.Contains(in pos) == ContainmentType.Disjoint)
                    continue;
                for (int j = 0; j < cache.CachedPoints.Length - 1; j++)
                {
                    ref var start = ref cache.CachedPoints[j].Position;
                    ref var end = ref cache.CachedPoints[j + 1].Position;
                    var dir = end - start;
                    var dirLen = dir.Normalize();
                    var distSq = Vector3.RayDistanceSquared(in pos, in start, in dir, out var len);
                    if (len >= 0 && len <= dirLen && distSq <= tolerance)
                    {
                        hitPos = start + dir * len;
                        forward = dir;
                        return true;
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
            foreach (var curve in Curves)
            {
                if (curve.Start == curve.End)
                    continue;
                var cache = curve.GetPointCache(Segments);
                if (!Ray.Intersects(in ray, in cache.AABB, out var intersectDist) || intersectDist < 0 || intersectDist > rayLen)
                    continue;
                for (int j = 0; j < cache.CachedPoints.Length - 1; j++)
                {
                    ref var start = ref cache.CachedPoints[j].Position;
                    var end = cache.CachedPoints[j + 1].Position;
                    var segDir = end - start;
                    var segLen = segDir.Length();
                    helper.SetLineB(in start, in end);
                    helper.GetDistance(out var t1, out var t2);
                    if (helper.distance < tolerance && t1 >= 0 && t2 >= 0 && t1 <= rayLen && t2 <= segLen)
                    {
                        hitPos = helper.GetPonB();
                        forward = hitPos - start;
                        forward.Normalize();
                        return true;
                    }
                }
            }
            hitPos = Vector3.Zero;
            forward = Vector3.Zero;
            return false;
        }

        public Vector3 GetValue(float distance)
        {
            return TryGetValueAndForward(distance, out var pos, out _) ? pos : Vector3.Zero;
        }

        public Vector3 GetForword(float distance)
        {
            return TryGetValueAndForward(distance, out _, out var dir) ? dir : Vector3.Zero;
        }

        public bool GetValueAndForword(float distance, out Vector3 pos, out Vector3 dir)
        {
            return TryGetValueAndForward(distance, out pos, out dir);
        }

        public Vector3 GetValueAtAlpha(float alpha)
        {
            return TryGetValueAndForwardAtAlpha(alpha, out var pos, out _) ? pos : Vector3.Zero;
        }

        public bool TryGetValueAndForwardAtAlpha(float alpha, out Vector3 pos, out Vector3 dir)
        {
            EnsureDerivedData();
            if (mCurves.Count == 0 || mLength <= 0.0f || float.IsNaN(alpha) || float.IsInfinity(alpha))
            {
                pos = Vector3.Zero;
                dir = Vector3.Zero;
                return false;
            }
            if (mIsClosed)
                alpha -= (float)Math.Floor(alpha);
            else
                alpha = MathHelper.Clamp(alpha, 0.0f, 1.0f);
            return TryGetValueAndForward(alpha * mLength, out pos, out dir);
        }

        public bool TryGetSegmentValue(int segmentIndex, float t, out Vector3 pos, out Vector3 dir)
        {
            EnsureDerivedData();
            if (segmentIndex < 0 || segmentIndex >= mCurves.Count)
            {
                pos = Vector3.Zero;
                dir = Vector3.Zero;
                return false;
            }
            t = MathHelper.Clamp(t, 0.0f, 1.0f);
            pos = mCurves[segmentIndex].GetValue(t);
            dir = mCurves[segmentIndex].GetForward(t);
            return true;
        }

        bool TryGetValueAndForward(float distance, out Vector3 pos, out Vector3 dir)
        {
            EnsureDerivedData();
            if (PointCount == 0 || mLength <= 0.0f || float.IsNaN(distance) || float.IsInfinity(distance))
            {
                pos = Vector3.Zero;
                dir = Vector3.Zero;
                return false;
            }
            if (mIsClosed)
            {
                distance %= mLength;
                if (distance < 0.0f)
                    distance += mLength;
            }
            else
            {
                distance = MathHelper.Clamp(distance, 0.0f, mLength);
            }
            if (!mIsClosed && distance == mLength)
            {
                var lastCurve = mCurves[mCurves.Count - 1];
                pos = lastCurve.End;
                dir = lastCurve.GetForward(1.0f);
                return true;
            }
            foreach (var curve in mCurves)
            {
                if (curve.Length <= 0.0f)
                    continue;
                if (distance <= curve.Length)
                {
                    var t = curve.GetParameterAtDistance(distance);
                    pos = curve.GetValue(t);
                    dir = curve.GetForward(t);
                    if (!float.IsFinite(dir.X) || !float.IsFinite(dir.Y) || !float.IsFinite(dir.Z))
                        dir = Vector3.Zero;
                    return true;
                }
                distance -= curve.Length;
            }
            pos = Vector3.Zero;
            dir = Vector3.Zero;
            return false;
        }
    }

    [UnitTest.TtTest]
    public class UTest_Bezier3DSpline
    {
        const float Tolerance = 1e-4f;

        static bool NearlyEqual(in Vector3 left, in Vector3 right)
        {
            return Vector3.Distance(left, right) <= Tolerance;
        }

        static TtBezier3DSpline CreateCurvedSpline()
        {
            var spline = new TtBezier3DSpline();
            spline.AppendPoint(new Vector3(0, 0, 0), new Vector3(-1, 0, 0), new Vector3(1, 1, 0));
            spline.AppendPoint(new Vector3(3, 0, 0), new Vector3(2, 1, 0), new Vector3(4, 0, 0));
            return spline;
        }

        public void UnitTestEntrance()
        {
            TestStraightLine();
            TestZeroBasedMutation();
            TestDerivedDataRefresh();
            TestBoundTangents();
            TestSplitPreservesCurve();
            TestSnapshotIsolation();
            TestClosedSpline();
            TestArcLengthParameterization();
            TestDistanceEndpointsAndDegenerateSegments();
        }

        void TestStraightLine()
        {
            var spline = new TtBezier3DSpline();
            spline.AppendPoint(new Vector3(-0.5f, 0, 0), new Vector3(-5.0f / 6.0f, 0, 0), new Vector3(-1.0f / 6.0f, 0, 0));
            spline.AppendPoint(new Vector3(0.5f, 0, 0), new Vector3(1.0f / 6.0f, 0, 0), new Vector3(5.0f / 6.0f, 0, 0));

            UnitTest.TtUnitTestManager.TAssert(Math.Abs(spline.Length - 1.0f) <= Tolerance,
                $"Bezier straight-line length mismatch: {spline.Length}");
            UnitTest.TtUnitTestManager.TAssert(NearlyEqual(spline.GetValue(0.5f), Vector3.Zero),
                $"Bezier straight-line midpoint mismatch: {spline.GetValue(0.5f)}");
        }

        void TestZeroBasedMutation()
        {
            var spline = CreateCurvedSpline();
            var inserted = new Vector3(1.5f, 0.5f, 0);
            UnitTest.TtUnitTestManager.TAssert(NearlyEqual(spline.GetPointPos(0), Vector3.Zero),
                "Bezier point indexing must be 0-based");
            UnitTest.TtUnitTestManager.TAssert(spline.InsertPoint(1, inserted, inserted - Vector3.UnitX, inserted + Vector3.UnitX),
                "Bezier insertion at a 0-based index failed");
            UnitTest.TtUnitTestManager.TAssert(spline.PointCount == 3 && NearlyEqual(spline.GetPointPos(1), inserted),
                "Bezier insertion produced an unexpected point sequence");
            UnitTest.TtUnitTestManager.TAssert(!spline.InsertPoint(-1, inserted, inserted, inserted) &&
                !spline.RemovePoint(spline.PointCount), "Bezier mutation accepted an out-of-range index");

            var oldLeft = spline.GetPointLeftCtrl(1);
            var oldRight = spline.GetPointRightCtrl(1);
            var moved = inserted + Vector3.UnitY;
            UnitTest.TtUnitTestManager.TAssert(spline.SetPointPosition(1, moved), "Bezier anchor move failed");
            UnitTest.TtUnitTestManager.TAssert(NearlyEqual(spline.GetPointLeftCtrl(1), oldLeft + Vector3.UnitY) &&
                NearlyEqual(spline.GetPointRightCtrl(1), oldRight + Vector3.UnitY),
                "Bezier anchor move did not preserve tangent offsets");
            UnitTest.TtUnitTestManager.TAssert(spline.RemovePoint(1) && spline.PointCount == 2,
                "Bezier point removal failed");
        }

        void TestDerivedDataRefresh()
        {
            var spline = CreateCurvedSpline();
            var oldLength = spline.Length;
            var newControl = new Vector3(1, 3, 0);
            UnitTest.TtUnitTestManager.TAssert(spline.SetPointRightCtrl(0, newControl),
                "Bezier tangent update failed");
            UnitTest.TtUnitTestManager.TAssert(NearlyEqual(spline.Curves[0].StartCtrl, newControl),
                "Bezier curve cache did not refresh after tangent update");
            UnitTest.TtUnitTestManager.TAssert(Math.Abs(spline.Length - oldLength) > Tolerance,
                "Bezier length cache did not refresh after tangent update");
        }

        void TestBoundTangents()
        {
            var spline = CreateCurvedSpline();
            var tangent = new Vector3(2, -3, 1);
            UnitTest.TtUnitTestManager.TAssert(spline.SetPointTangent(1, true, tangent),
                "Bezier bound tangent update failed");
            var anchor = spline.GetPointPos(1);
            var leftDerivative = anchor - spline.GetPointLeftCtrl(1);
            var rightDerivative = spline.GetPointRightCtrl(1) - anchor;
            UnitTest.TtUnitTestManager.TAssert(NearlyEqual(leftDerivative, rightDerivative),
                "Bezier bound tangents do not preserve C1 continuity");
        }

        void TestSplitPreservesCurve()
        {
            var spline = CreateCurvedSpline();
            var original = spline.Curves[0];
            var samples = new Vector3[17];
            for (int i = 0; i < samples.Length; i++)
                samples[i] = original.GetValue((float)i / (samples.Length - 1));

            UnitTest.TtUnitTestManager.TAssert(spline.SplitSegmentAfter(0), "Bezier segment split failed");
            UnitTest.TtUnitTestManager.TAssert(spline.PointCount == 3 && spline.Curves.Count == 2,
                "Bezier segment split produced an unexpected topology");
            for (int i = 0; i < samples.Length; i++)
            {
                var t = (float)i / (samples.Length - 1);
                var actual = t <= 0.5f ? spline.Curves[0].GetValue(t * 2.0f) :
                    spline.Curves[1].GetValue(t * 2.0f - 1.0f);
                UnitTest.TtUnitTestManager.TAssert(NearlyEqual(samples[i], actual),
                    $"Bezier split changed curve geometry at t={t}: expected={samples[i]}, actual={actual}");
            }
        }

        void TestSnapshotIsolation()
        {
            var spline = CreateCurvedSpline();
            var snapshot = spline.CreateSnapshot();
            var original = snapshot.Points[0].Position;
            spline.SetPointPosition(0, new Vector3(7, 8, 9));
            UnitTest.TtUnitTestManager.TAssert(NearlyEqual(snapshot.Points[0].Position, original),
                "Bezier snapshot shares mutable point state with its source");

            spline.RestoreSnapshot(snapshot);
            snapshot.Points[0].Position = new Vector3(-7, -8, -9);
            UnitTest.TtUnitTestManager.TAssert(NearlyEqual(spline.GetPointPos(0), original),
                "Bezier snapshot restore did not clone point state");
        }

        void TestArcLengthParameterization()
        {
            var curve = new Bezier3D(new Vector3(0, 0, 0), new Vector3(4, 0, 0),
                new Vector3(0, 8, 0), new Vector3(4, -8, 0), 512);
            float expectedStep = curve.Length / 8.0f;
            float previousT = 0.0f;
            for (int i = 1; i <= 8; i++)
            {
                float currentT = curve.GetParameterAtDistance(expectedStep * i);
                float measuredLength = MeasureCurveLength(curve, previousT, currentT, 256);
                UnitTest.TtUnitTestManager.TAssert(Math.Abs(measuredLength - expectedStep) <= expectedStep * 0.025f,
                    $"Bezier arc-length inversion is uneven at sample {i}: expected={expectedStep}, actual={measuredLength}");
                previousT = currentT;
            }
        }

        static float MeasureCurveLength(Bezier3D curve, float startT, float endT, int steps)
        {
            var previous = curve.GetValue(startT);
            float result = 0.0f;
            for (int i = 1; i <= steps; i++)
            {
                float t = startT + (endT - startT) * i / steps;
                var current = curve.GetValue(t);
                result += Vector3.Distance(previous, current);
                previous = current;
            }
            return result;
        }

        void TestDistanceEndpointsAndDegenerateSegments()
        {
            var spline = new TtBezier3DSpline();
            spline.AppendPoint(Vector3.Zero, Vector3.Zero, Vector3.Zero);
            spline.AppendPoint(Vector3.Zero, Vector3.Zero, Vector3.Zero);
            spline.AppendPoint(new Vector3(2, 0, 0), new Vector3(1, 0, 0), new Vector3(3, 0, 0));

            UnitTest.TtUnitTestManager.TAssert(NearlyEqual(spline.GetValue(-1.0f), Vector3.Zero),
                "Open spline did not clamp a negative distance to its start");
            UnitTest.TtUnitTestManager.TAssert(NearlyEqual(spline.GetValue(spline.Length + 1.0f), new Vector3(2, 0, 0)),
                "Open spline did not clamp an excessive distance to its end");
            UnitTest.TtUnitTestManager.TAssert(spline.GetValueAndForword(0.0f, out _, out var direction) &&
                float.IsFinite(direction.X) && float.IsFinite(direction.Y) && float.IsFinite(direction.Z),
                "Degenerate spline segment produced a non-finite tangent");

            spline.IsClosed = true;
            UnitTest.TtUnitTestManager.TAssert(NearlyEqual(spline.GetValue(-0.25f),
                spline.GetValue(spline.Length - 0.25f)), "Closed spline did not wrap a negative distance");
        }

        void TestClosedSpline()
        {
            var spline = new TtBezier3DSpline();
            spline.AppendPoint(new Vector3(0, 0, 0), new Vector3(0, 0, -1), new Vector3(1, 0, 0));
            spline.AppendPoint(new Vector3(3, 0, 0), new Vector3(2, 0, 0), new Vector3(3, 0, 1));
            spline.AppendPoint(new Vector3(0, 0, 3), new Vector3(1, 0, 3), new Vector3(0, 0, 2));
            float openLength = spline.Length;
            spline.IsClosed = true;

            UnitTest.TtUnitTestManager.TAssert(spline.SegmentCount == 3 && spline.Length > openLength,
                "Closed spline did not add the last-to-first segment");
            var closing = spline.Curves[2];
            UnitTest.TtUnitTestManager.TAssert(NearlyEqual(closing.Start, spline.GetPointPos(2)) &&
                NearlyEqual(closing.StartCtrl, spline.GetPointRightCtrl(2)) &&
                NearlyEqual(closing.EndCtrl, spline.GetPointLeftCtrl(0)) &&
                NearlyEqual(closing.End, spline.GetPointPos(0)),
                "Closed spline uses incorrect closing-segment control points");
            UnitTest.TtUnitTestManager.TAssert(NearlyEqual(spline.GetValueAtAlpha(0.0f),
                spline.GetValueAtAlpha(1.0f)), "Closed spline alpha did not wrap");
            UnitTest.TtUnitTestManager.TAssert(NearlyEqual(spline.GetValue(0.25f),
                spline.GetValue(spline.Length + 0.25f)), "Closed spline distance did not wrap");

            var beforeSplit = closing.GetValue(0.5f);
            UnitTest.TtUnitTestManager.TAssert(spline.SplitSegmentAfter(2),
                "Closing segment split failed");
            UnitTest.TtUnitTestManager.TAssert(spline.PointCount == 4 && spline.SegmentCount == 4 &&
                NearlyEqual(spline.GetPointPos(3), beforeSplit),
                "Closing segment split changed its midpoint");

            var snapshot = spline.CreateSnapshot();
            spline.IsClosed = false;
            spline.RestoreSnapshot(snapshot);
            UnitTest.TtUnitTestManager.TAssert(spline.IsClosed && spline.SegmentCount == 4,
                "Closed state was not preserved by a full spline snapshot");
        }
    }
}
