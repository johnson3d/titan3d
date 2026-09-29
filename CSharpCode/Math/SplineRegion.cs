using System;
using System.Collections.Generic;

namespace EngineNS
{
    /// <summary>
    /// 闭合 Bézier 样条在世界 XZ 平面上的不可变区域快照。
    /// TitanEngine 使用 Y-up，因此 Y 只作为区域表面的高度。
    /// </summary>
    public sealed class TtSplineRegionSnapshot
    {
        readonly Vector3[] mBoundaryPoints;
        readonly Vector2[] mPolygonPoints;

        public bool IsValid { get; }
        public string InvalidReason { get; }
        public ulong SourceRevision { get; }
        public IReadOnlyList<Vector3> BoundaryPoints => mBoundaryPoints;
        public IReadOnlyList<Vector2> PolygonPoints => mPolygonPoints;
        public Vector2 MinimumXZ { get; }
        public Vector2 MaximumXZ { get; }
        public float MinimumY { get; }
        public float MaximumY { get; }
        public float AverageY { get; }

        TtSplineRegionSnapshot(Vector3[] boundaryPoints, ulong revision, bool isValid,
            string invalidReason, in Vector2 minimumXZ, in Vector2 maximumXZ,
            float minimumY, float maximumY, float averageY)
        {
            mBoundaryPoints = boundaryPoints ?? Array.Empty<Vector3>();
            mPolygonPoints = new Vector2[mBoundaryPoints.Length];
            for (int i = 0; i < mBoundaryPoints.Length; i++)
                mPolygonPoints[i] = new Vector2(mBoundaryPoints[i].X, mBoundaryPoints[i].Z);
            SourceRevision = revision;
            IsValid = isValid;
            InvalidReason = invalidReason;
            MinimumXZ = minimumXZ;
            MaximumXZ = maximumXZ;
            MinimumY = minimumY;
            MaximumY = maximumY;
            AverageY = averageY;
        }

        public static TtSplineRegionSnapshot Create(TtBezier3DSplineSnapshot source,
            in FTransform localToWorld, float borderSampleSpacing = 0.25f,
            float epsilon = 1e-5f)
        {
            if (source == null || !source.IsClosed)
                return Invalid(source?.Revision ?? 0, "Spline is not closed");
            if (source.Points.Count < 3)
                return Invalid(source.Revision, "A closed region requires at least three points");

            var spline = new TtBezier3DSpline
            {
                Segments = source.Segments,
            };
            spline.RestoreSnapshot(source);
            if (spline.SegmentCount < 3 || spline.Length <= epsilon)
                return Invalid(source.Revision, "Spline has no valid closed boundary");

            borderSampleSpacing = Math.Max(epsilon, borderSampleSpacing);
            var boundary = new List<Vector3>();
            for (int curveIndex = 0; curveIndex < spline.Curves.Count; curveIndex++)
            {
                var curve = spline.Curves[curveIndex];
                int steps = Math.Max(1, (int)Math.Ceiling(curve.Length / borderSampleSpacing));
                for (int step = 0; step < steps; step++)
                {
                    var local = curve.GetValue((float)step / steps).AsDVector();
                    var world = localToWorld.TransformPosition(in local).ToSingleVector3();
                    AddDistinct(boundary, in world, epsilon);
                }
            }

            if (boundary.Count > 1)
            {
                var firstPoint = boundary[0];
                var lastPoint = boundary[boundary.Count - 1];
                if (DistanceSquaredXZ(in firstPoint, in lastPoint) <= epsilon * epsilon)
                    boundary.RemoveAt(boundary.Count - 1);
            }
            if (boundary.Count < 3)
                return Invalid(source.Revision, "Spline projection contains fewer than three distinct points");

            float signedArea = CalculateSignedArea(boundary);
            if (!float.IsFinite(signedArea) || Math.Abs(signedArea) <= epsilon * epsilon)
                return Invalid(source.Revision, "Spline projection has zero area");
            if (signedArea < 0.0f)
                boundary.Reverse();
            if (HasSelfIntersection(boundary, epsilon))
                return Invalid(source.Revision, "Self-intersecting spline regions are not supported");

            var minXZ = new Vector2(float.MaxValue, float.MaxValue);
            var maxXZ = new Vector2(float.MinValue, float.MinValue);
            float minY = float.MaxValue;
            float maxY = float.MinValue;
            float sumY = 0.0f;
            for (int i = 0; i < boundary.Count; i++)
            {
                var point = boundary[i];
                minXZ.X = Math.Min(minXZ.X, point.X);
                minXZ.Y = Math.Min(minXZ.Y, point.Z);
                maxXZ.X = Math.Max(maxXZ.X, point.X);
                maxXZ.Y = Math.Max(maxXZ.Y, point.Z);
                minY = Math.Min(minY, point.Y);
                maxY = Math.Max(maxY, point.Y);
                sumY += point.Y;
            }

            return new TtSplineRegionSnapshot(boundary.ToArray(), source.Revision, true, null,
                in minXZ, in maxXZ, minY, maxY, sumY / boundary.Count);
        }

        static TtSplineRegionSnapshot Invalid(ulong revision, string reason)
        {
            var zero = Vector2.Zero;
            return new TtSplineRegionSnapshot(Array.Empty<Vector3>(), revision, false, reason,
                in zero, in zero, 0.0f, 0.0f, 0.0f);
        }

        static void AddDistinct(List<Vector3> points, in Vector3 point, float epsilon)
        {
            if (!float.IsFinite(point.X) || !float.IsFinite(point.Y) || !float.IsFinite(point.Z))
                return;
            if (points.Count == 0)
            {
                points.Add(point);
                return;
            }
            var lastPoint = points[points.Count - 1];
            if (DistanceSquaredXZ(in lastPoint, in point) > epsilon * epsilon)
                points.Add(point);
        }

        static float DistanceSquaredXZ(in Vector3 left, in Vector3 right)
        {
            float dx = left.X - right.X;
            float dz = left.Z - right.Z;
            return dx * dx + dz * dz;
        }

        static float CalculateSignedArea(List<Vector3> points)
        {
            double area = 0.0;
            for (int i = 0; i < points.Count; i++)
            {
                var current = points[i];
                var next = points[(i + 1) % points.Count];
                area += (double)current.X * next.Z - (double)next.X * current.Z;
            }
            return (float)(area * 0.5);
        }

        static bool HasSelfIntersection(List<Vector3> points, float epsilon)
        {
            int count = points.Count;
            for (int i = 0; i < count; i++)
            {
                var a0 = new Vector2(points[i].X, points[i].Z);
                var a1 = new Vector2(points[(i + 1) % count].X, points[(i + 1) % count].Z);
                for (int j = i + 1; j < count; j++)
                {
                    if (j == i || j == i + 1 || (i == 0 && j == count - 1))
                        continue;
                    var b0 = new Vector2(points[j].X, points[j].Z);
                    var b1 = new Vector2(points[(j + 1) % count].X, points[(j + 1) % count].Z);
                    if (SegmentsIntersect(in a0, in a1, in b0, in b1, epsilon))
                        return true;
                }
            }
            return false;
        }

        static bool SegmentsIntersect(in Vector2 a0, in Vector2 a1, in Vector2 b0, in Vector2 b1, float epsilon)
        {
            float o1 = Cross(in a0, in a1, in b0);
            float o2 = Cross(in a0, in a1, in b1);
            float o3 = Cross(in b0, in b1, in a0);
            float o4 = Cross(in b0, in b1, in a1);
            if (((o1 > epsilon && o2 < -epsilon) || (o1 < -epsilon && o2 > epsilon)) &&
                ((o3 > epsilon && o4 < -epsilon) || (o3 < -epsilon && o4 > epsilon)))
                return true;
            return (Math.Abs(o1) <= epsilon && IsPointOnSegment(in b0, in a0, in a1, epsilon)) ||
                (Math.Abs(o2) <= epsilon && IsPointOnSegment(in b1, in a0, in a1, epsilon)) ||
                (Math.Abs(o3) <= epsilon && IsPointOnSegment(in a0, in b0, in b1, epsilon)) ||
                (Math.Abs(o4) <= epsilon && IsPointOnSegment(in a1, in b0, in b1, epsilon));
        }

        static bool IsPointOnSegment(in Vector2 point, in Vector2 start, in Vector2 end, float epsilon)
        {
            return point.X >= Math.Min(start.X, end.X) - epsilon &&
                point.X <= Math.Max(start.X, end.X) + epsilon &&
                point.Y >= Math.Min(start.Y, end.Y) - epsilon &&
                point.Y <= Math.Max(start.Y, end.Y) + epsilon;
        }

        static float Cross(in Vector2 a, in Vector2 b, in Vector2 c)
        {
            return (b.X - a.X) * (c.Y - a.Y) - (b.Y - a.Y) * (c.X - a.X);
        }

        public bool Contains(in Vector2 point, float boundaryEpsilon = 1e-4f)
        {
            if (!IsValid || point.X < MinimumXZ.X - boundaryEpsilon ||
                point.X > MaximumXZ.X + boundaryEpsilon ||
                point.Y < MinimumXZ.Y - boundaryEpsilon || point.Y > MaximumXZ.Y + boundaryEpsilon)
                return false;

            float epsilonSquared = boundaryEpsilon * boundaryEpsilon;
            bool inside = false;
            for (int i = 0, j = mPolygonPoints.Length - 1; i < mPolygonPoints.Length; j = i++)
            {
                var a = mPolygonPoints[j];
                var b = mPolygonPoints[i];
                if (DistanceSquaredToSegment(in point, in a, in b) <= epsilonSquared)
                    return true;
                if ((a.Y > point.Y) != (b.Y > point.Y) &&
                    point.X < (b.X - a.X) * (point.Y - a.Y) / (b.Y - a.Y) + a.X)
                    inside = !inside;
            }
            return inside;
        }

        public float DistanceToBoundary(in Vector2 point)
        {
            if (!IsValid)
                return float.MaxValue;
            float minDistanceSquared = float.MaxValue;
            for (int i = 0; i < mPolygonPoints.Length; i++)
            {
                var a = mPolygonPoints[i];
                var b = mPolygonPoints[(i + 1) % mPolygonPoints.Length];
                minDistanceSquared = Math.Min(minDistanceSquared, DistanceSquaredToSegment(in point, in a, in b));
            }
            return (float)Math.Sqrt(minDistanceSquared);
        }

        static float DistanceSquaredToSegment(in Vector2 point, in Vector2 start, in Vector2 end)
        {
            var edge = end - start;
            float lengthSquared = edge.LengthSquared();
            if (lengthSquared <= 1e-20f)
                return (point - start).LengthSquared();
            float t = Vector2.Dot(point - start, edge) / lengthSquared;
            t = MathHelper.Clamp(t, 0.0f, 1.0f);
            var nearest = start + edge * t;
            return (point - nearest).LengthSquared();
        }

        public float ProjectHeight(in Vector2 point)
        {
            if (!IsValid || mBoundaryPoints.Length == 0)
                return 0.0f;
            double weightedHeight = 0.0;
            double weightSum = 0.0;
            for (int i = 0; i < mBoundaryPoints.Length; i++)
            {
                var sample = mBoundaryPoints[i];
                double dx = point.X - sample.X;
                double dz = point.Y - sample.Z;
                double distanceSquared = dx * dx + dz * dz;
                if (distanceSquared <= 1e-12)
                    return sample.Y;
                double weight = 1.0 / distanceSquared;
                weightedHeight += sample.Y * weight;
                weightSum += weight;
            }
            return weightSum > 0.0 ? (float)(weightedHeight / weightSum) : AverageY;
        }
    }

    [UnitTest.TtTest]
    public class TtTest_SplineRegionSnapshot
    {
        static TtBezier3DSpline CreatePolygon(params Vector3[] points)
        {
            var spline = new TtBezier3DSpline();
            for (int i = 0; i < points.Length; i++)
                spline.AppendPoint(in points[i], in points[i], in points[i]);
            spline.IsClosed = true;
            return spline;
        }

        public void UnitTestEntrance()
        {
            var identity = FTransform.Identity;
            var square = CreatePolygon(new Vector3(0, 1, 0), new Vector3(4, 2, 0),
                new Vector3(4, 3, 4), new Vector3(0, 4, 4));
            var region = TtSplineRegionSnapshot.Create(square.CreateSnapshot(), in identity, 0.25f);
            UnitTest.TtUnitTestManager.TAssert(region.IsValid, region.InvalidReason);
            var inside = new Vector2(2, 2);
            var outside = new Vector2(5, 2);
            var boundary = new Vector2(0, 2);
            UnitTest.TtUnitTestManager.TAssert(region.Contains(in inside) &&
                !region.Contains(in outside) && region.Contains(in boundary),
                "Spline region containment failed");
            UnitTest.TtUnitTestManager.TAssert(region.DistanceToBoundary(in inside) > 1.9f &&
                region.DistanceToBoundary(in inside) < 2.1f,
                "Spline region boundary distance is incorrect");
            UnitTest.TtUnitTestManager.TAssert(region.ProjectHeight(in inside) >= region.MinimumY &&
                region.ProjectHeight(in inside) <= region.MaximumY,
                "Spline region projected height is outside the sampled range");

            var clockwise = CreatePolygon(new Vector3(0, 0, 0), new Vector3(0, 0, 4),
                new Vector3(4, 0, 4), new Vector3(4, 0, 0));
            var clockwiseRegion = TtSplineRegionSnapshot.Create(clockwise.CreateSnapshot(), in identity, 0.25f);
            UnitTest.TtUnitTestManager.TAssert(clockwiseRegion.IsValid &&
                clockwiseRegion.Contains(in inside), "Clockwise spline region was not normalized");

            var crossing = CreatePolygon(new Vector3(0, 0, 0), new Vector3(4, 0, 4),
                new Vector3(0, 0, 4), new Vector3(4, 0, 0));
            var invalid = TtSplineRegionSnapshot.Create(crossing.CreateSnapshot(), in identity, 0.25f);
            UnitTest.TtUnitTestManager.TAssert(!invalid.IsValid,
                "Self-intersecting spline region was accepted");
        }
    }
}
