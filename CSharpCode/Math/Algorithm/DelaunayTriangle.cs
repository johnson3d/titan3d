using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Algorithm
{
    public class DelaunayTriangle2D
    {
        public struct Triangle
        {
            public Vector2 Circumcenter;
            public int VertexAIndex;
            public int VertexBIndex;
            public int VertexCIndex;

            public Triangle(int vertexAIndex, int vertexBIndex, int vertexCIndex, Vector2[] points)
            {
                VertexAIndex = vertexAIndex;
                VertexBIndex = vertexBIndex;
                VertexCIndex = vertexCIndex;

                var p0 = points[VertexAIndex];
                var p1 = points[VertexBIndex];
                var p2 = points[VertexCIndex];
                var dA = p0.LengthSquared();
                var dB = p1.LengthSquared();
                var dC = p2.LengthSquared();

                var aux1 = (dA * (p2.Y - p1.Y) + dB * (p0.Y - p2.Y) + dC * (p1.Y - p0.Y));
                var aux2 = -(dA * (p2.X - p1.X) + dB * (p0.X - p2.X) + dC * (p1.X - p0.X));
                var div = (2 * (p0.X * (p2.Y - p1.Y) + p1.X * (p0.Y - p2.Y) + p2.X * (p1.Y - p0.Y)));

                if (div == 0)
                    throw new DivideByZeroException();

                Circumcenter = new Vector2(aux1 / div, aux2 / div);
                MakeCCW(points);
            }
            private void MakeCCW(Vector2[] points)
            {
                if (VertexAIndex >= points.Length || VertexBIndex >= points.Length || VertexCIndex >= points.Length)
                    return;

                var diff1 = points[VertexBIndex] - points[VertexAIndex];
                var diff2 = points[VertexCIndex] - points[VertexAIndex];
                float result = diff1.X * diff2.Y - diff1.Y * diff2.X;
                if(result < 0.0f)
                {
                    var tempPt = VertexCIndex;
                    VertexCIndex = VertexBIndex;
                    VertexBIndex = tempPt;
                }
            }
            public bool HasSameEdge(in Triangle other)
            {
                if ((other.VertexAIndex == VertexAIndex && other.VertexBIndex == VertexBIndex) ||
                    (other.VertexBIndex == VertexAIndex && other.VertexCIndex == VertexBIndex) ||
                    (other.VertexCIndex == VertexAIndex && other.VertexAIndex == VertexBIndex))
                    return true;
                if ((other.VertexAIndex == VertexBIndex && other.VertexBIndex == VertexCIndex) ||
                    (other.VertexBIndex == VertexBIndex && other.VertexCIndex == VertexCIndex) ||
                    (other.VertexCIndex == VertexBIndex && other.VertexAIndex == VertexCIndex))
                    return true;
                if ((other.VertexAIndex == VertexCIndex && other.VertexBIndex == VertexAIndex) ||
                    (other.VertexBIndex == VertexCIndex && other.VertexCIndex == VertexAIndex) ||
                    (other.VertexCIndex == VertexCIndex && other.VertexAIndex == VertexAIndex))
                    return true;
                return false;
            }
            public bool DoesShareSameEdge(in Triangle other)
            {
                if ((other.VertexAIndex == VertexBIndex && other.VertexBIndex == VertexAIndex) ||
                    (other.VertexBIndex == VertexBIndex && other.VertexCIndex == VertexAIndex) ||
                    (other.VertexCIndex == VertexBIndex && other.VertexAIndex == VertexAIndex))
                    return true;
                if ((other.VertexAIndex == VertexCIndex && other.VertexBIndex == VertexBIndex) ||
                    (other.VertexBIndex == VertexCIndex && other.VertexCIndex == VertexBIndex) ||
                    (other.VertexCIndex == VertexCIndex && other.VertexAIndex == VertexBIndex))
                    return true;
                if ((other.VertexAIndex == VertexAIndex && other.VertexBIndex == VertexCIndex) ||
                    (other.VertexBIndex == VertexAIndex && other.VertexCIndex == VertexCIndex) ||
                    (other.VertexCIndex == VertexAIndex && other.VertexAIndex == VertexCIndex))
                    return true;
                return false;
            }
            public int FindNonSharingPoint(in Triangle other)
            {
                if (!Contains(other.VertexAIndex))
                    return other.VertexAIndex;
                if(!Contains(other.VertexBIndex))
                    return other.VertexBIndex;
                if(!Contains(other.VertexCIndex))
                    return other.VertexCIndex;
                return -1;
            }
            public bool Contains(int pointIdx)
            {
                return ((VertexAIndex == pointIdx) || (VertexBIndex == pointIdx) || (VertexCIndex == pointIdx));
            }
            public static bool operator == (in Triangle left, in Triangle right)
            {
                return ((left.VertexAIndex == right.VertexAIndex) &&
                        (left.VertexBIndex == right.VertexBIndex) &&
                        (left.VertexCIndex == right.VertexCIndex));
            }
            public static bool operator !=(in Triangle left, in Triangle right)
            {
                return ((left.VertexAIndex != right.VertexAIndex) ||
                        (left.VertexBIndex != right.VertexBIndex) ||
                        (left.VertexCIndex != right.VertexCIndex));
            }
            public override bool Equals(object obj)
            {
                if(obj == null)
                    return false;
                if (obj.GetType() != GetType())
                    return false;
                return Equals((Triangle)obj);
            }
            public bool Equals(Triangle val)
            {
                return ((val.VertexAIndex == VertexAIndex) &&
                        (val.VertexBIndex == VertexBIndex) &&
                        (val.VertexCIndex == VertexCIndex));
            }
            public override int GetHashCode()
            {
                return ToString().GetHashCode();
            }
            public override string ToString()
            {
                return ("A:" + VertexAIndex + " B:" + VertexBIndex + " C:" + VertexCIndex);
            }
        }
        public static bool Calculate(Vector2[] points, ref List<Triangle> triangleList, ref Dictionary<int, List<int>> trianglesWithPoint)
        {
            triangleList.Clear();

            if (points == null || points.Length == 0)
                return false;

            if(points.Length == 1)
            {
                var triangle = new Triangle(0, 0, 0, points);
                AddTriangle(triangle, ref triangleList, ref trianglesWithPoint);
            }
            else if(points.Length == 2)
            {
                var triangle = new Triangle(0, 1, 1, points);
                AddTriangle(triangle, ref triangleList, ref trianglesWithPoint);
            }
            else
            {
                Sort(ref points);
                for(int i=2; i<points.Length; ++i)
                {
                    GenerateTriangles(points, i + 1, ref triangleList, ref trianglesWithPoint);
                }
                if(triangleList.Count == 0)
                {
                    if(AllCoincident(points))
                    {
                        // coincident case - just create one triangle
                        var triangle = new Triangle(0, 0, 0, points);
                        AddTriangle(triangle, ref triangleList, ref trianglesWithPoint);
                    }
                    else
                    {
                        // collinear case: create degenerate triangles between pairs of points
                        for(int i=0; i<points.Length; ++i)
                        {
                            var triangle = new Triangle(i, i + 1, i + 1, points);
                            AddTriangle(triangle, ref triangleList, ref trianglesWithPoint);
                        }
                    }
                }
            }

            return true;
        }

        static bool AllCoincident(Vector2[] inPoints)
        {
            if(inPoints.Length > 0)
            {
                for(int i=1; i<inPoints.Length; ++i)
                {
                    if (inPoints[0] != inPoints[i])
                        return false;
                }

                return true;
            }

            return false;
        }

        static void AddTriangle(in Triangle triangle, ref List<Triangle> triangleList, ref Dictionary<int, List<int>> trianglesWithPoint, bool checkEdge = true)
        {
            for(int i=0; i<triangleList.Count; ++i)
            {
                if (triangle == triangleList[i])
                    return;

                if (checkEdge && triangle.HasSameEdge(triangleList[i]))
                    return;
            }

            SetTrianglePointRef(triangleList.Count, triangle, ref trianglesWithPoint);
            triangleList.Add(triangle);
        }
        static void Sort(ref Vector2[] points)
        {
            for(int i=0; i<points.Length-1; ++i)
            {
                for(int j=i+1; j<points.Length; ++j)
                {
                    bool bChange = false;
                    if(Math.Abs(points[i].X - points[j].X) < MathHelper.Epsilon)
                        bChange = (points[i].Y < points[j].Y) ? true : false;
                    else
                        bChange = (points[i].X < points[j].X) ? true : false;

                    if(bChange)
                    {
                        Vector2 temp = points[i];
                        points[i] = points[j];
                        points[j] = temp;
                    }
                }
            }
        }
        static int GenerateTriangles(Vector2[] pointList, int totalNum, ref List<Triangle> triangleList, ref Dictionary<int, List<int>> trianglesWithPoint)
        {
            if(totalNum == 3)
            {
                if(IsEligibleForTriangulation(pointList[0], pointList[1], pointList[2]))
                {
                    var triangle = new Triangle(0, 1, 2, pointList);
                    AddTriangle(triangle, ref triangleList, ref trianglesWithPoint);
                }
            }
            else if(triangleList.Count == 0)
            {
                Vector2 testPoint = pointList[totalNum - 1];
                for(int i=0; i<totalNum - 2; ++i)
                {
                    if(IsEligibleForTriangulation(pointList[i], pointList[i + 1], testPoint))
                    {
                        var triangle = new Triangle(i, i + 1, totalNum - 1, pointList);
                        AddTriangle(triangle, ref triangleList, ref trianglesWithPoint);
                    }
                }
            }
            else
            {
                var testPos = pointList[totalNum - 1];
                var testIndex = (totalNum - 1);
                for(int i=0; i<triangleList.Count; ++i)
                {
                    var triangle = triangleList[i];
                    if(IsEligibleForTriangulation(pointList[triangle.VertexAIndex], pointList[triangle.VertexBIndex], testPos))
                    {
                        var newTriangle = new Triangle(triangle.VertexAIndex, triangle.VertexBIndex, testIndex, pointList);
                        AddTriangle(newTriangle, ref triangleList, ref trianglesWithPoint);
                    }
                    if(IsEligibleForTriangulation(pointList[triangle.VertexAIndex], pointList[triangle.VertexCIndex], testPos))
                    {
                        var newTriangle = new Triangle(triangle.VertexAIndex, triangle.VertexCIndex, testIndex, pointList);
                        AddTriangle(newTriangle, ref triangleList, ref trianglesWithPoint);
                    }
                    if(IsEligibleForTriangulation(pointList[triangle.VertexBIndex], pointList[triangle.VertexCIndex], testPos))
                    {
                        var newTriangle = new Triangle(triangle.VertexBIndex, triangle.VertexCIndex, testIndex, pointList);
                        AddTriangle(newTriangle, ref triangleList, ref trianglesWithPoint);
                    }
                }

                for(int i=0; i<triangleList.Count; ++i)
                {
                    for(int j=i+1; j<triangleList.Count; ++j)
                    {
                        if(triangleList[i].DoesShareSameEdge(triangleList[j]))
                        {
                            if(FlipTriangles(i, j, ref triangleList, pointList, ref trianglesWithPoint))
                            {
                                i = -1;
                                break;
                            }
                        }
                    }
                }
            }

            return triangleList.Count;
        }
        static bool IsEligibleForTriangulation(in Vector2 a, in Vector2 b, in Vector2 c)
        {
            return !IsCollinear(a, b, c);
        }
        static bool IsCollinear(in Vector2 a, in Vector2 b, in Vector2 c)
        {
            var diff1 = b - a;
            var diff2 = c - a;
            var result = diff1.X * diff2.Y - diff1.Y * diff2.X;
            return (result == 0.0f);
        }
        static unsafe bool FlipTriangles(int triangleAIdx, int triangleBIdx, ref List<Triangle> triangleList, Vector2[] points, ref Dictionary<int, List<int>> trianglesWithPoint)
        {
            var gridMin = Vector2.Zero;
            var recipGridSize = Vector2.One;
            var testPt = triangleList[triangleAIdx].FindNonSharingPoint(triangleList[triangleBIdx]);
            if (GetCircumcircleState(triangleList[triangleAIdx], testPt, points, gridMin, recipGridSize) != ECircumCircleState.Inside)
                return false;

            var newTriangles = stackalloc Triangle[2];
            int trianglesMade = 0;
            if(IsEligibleForTriangulation(points[triangleList[triangleAIdx].VertexAIndex], points[triangleList[triangleAIdx].VertexBIndex], points[testPt]))
            {
                var newTriangle = new Triangle(triangleList[triangleAIdx].VertexAIndex, triangleList[triangleAIdx].VertexBIndex, testPt, points);
                if(GetCircumcircleState(newTriangle, triangleList[triangleAIdx].VertexCIndex, points, gridMin, recipGridSize) == ECircumCircleState.Outside)
                {
                    newTriangles[trianglesMade++] = newTriangle;
                }
            }
            if(IsEligibleForTriangulation(points[triangleList[triangleAIdx].VertexBIndex], points[triangleList[triangleAIdx].VertexCIndex], points[testPt]))
            {
                var newTriangle = new Triangle(triangleList[triangleAIdx].VertexBIndex, triangleList[triangleAIdx].VertexCIndex, testPt, points);
                if(GetCircumcircleState(newTriangle, triangleList[triangleAIdx].VertexAIndex, points, gridMin, recipGridSize) == ECircumCircleState.Outside)
                {
                    newTriangles[trianglesMade++] = newTriangle;
                }
            }
            if(IsEligibleForTriangulation(points[triangleList[triangleAIdx].VertexCIndex], points[triangleList[triangleAIdx].VertexAIndex], points[testPt]))
            {
                var newTriangle = new Triangle(triangleList[triangleAIdx].VertexCIndex, triangleList[triangleAIdx].VertexAIndex, testPt, points);
                if(GetCircumcircleState(newTriangle, triangleList[triangleAIdx].VertexBIndex, points, gridMin, recipGridSize) == ECircumCircleState.Outside)
                {
                    newTriangles[trianglesMade++] = newTriangle;
                }
            }

            if(trianglesMade == 2)
            {
                List<int> pointTriangles;
                if (trianglesWithPoint.TryGetValue(triangleList[triangleAIdx].VertexAIndex, out pointTriangles))
                {
                    pointTriangles.Remove(triangleAIdx);
                }
                if (trianglesWithPoint.TryGetValue(triangleList[triangleAIdx].VertexBIndex, out pointTriangles))
                {
                    pointTriangles.Remove(triangleAIdx);
                }
                if (trianglesWithPoint.TryGetValue(triangleList[triangleAIdx].VertexCIndex, out pointTriangles))
                {
                    pointTriangles.Remove(triangleAIdx);
                }

                if (trianglesWithPoint.TryGetValue(triangleList[triangleBIdx].VertexAIndex, out pointTriangles))
                {
                    pointTriangles.Remove(triangleAIdx);
                }
                if (trianglesWithPoint.TryGetValue(triangleList[triangleBIdx].VertexBIndex, out pointTriangles))
                {
                    pointTriangles.Remove(triangleAIdx);
                }
                if (trianglesWithPoint.TryGetValue(triangleList[triangleBIdx].VertexCIndex, out pointTriangles))
                {
                    pointTriangles.Remove(triangleAIdx);
                }

                triangleList[triangleAIdx] = newTriangles[0];
                triangleList[triangleBIdx] = newTriangles[1];

                SetTrianglePointRef(triangleAIdx, newTriangles[0], ref trianglesWithPoint);
                SetTrianglePointRef(triangleBIdx, newTriangles[1], ref trianglesWithPoint);
            }

            return trianglesMade == 2;
        }

        static void SetTrianglePointRef(int triangleIdx, in Triangle triangle, ref Dictionary<int, List<int>> trianglesWithPoint)
        {
            List<int> pointTriangles;
            if (!trianglesWithPoint.TryGetValue(triangle.VertexAIndex, out pointTriangles))
            {
                pointTriangles = new List<int>();
                trianglesWithPoint[triangle.VertexAIndex] = pointTriangles;
            }
            if (!pointTriangles.Contains(triangleIdx))
                pointTriangles.Add(triangleIdx);

            if (!trianglesWithPoint.TryGetValue(triangle.VertexBIndex, out pointTriangles))
            {
                pointTriangles = new List<int>();
                trianglesWithPoint[triangle.VertexBIndex] = pointTriangles;
            }
            if (!pointTriangles.Contains(triangleIdx))
                pointTriangles.Add(triangleIdx);

            if (!trianglesWithPoint.TryGetValue(triangle.VertexCIndex, out pointTriangles))
            {
                pointTriangles = new List<int>();
                trianglesWithPoint[triangle.VertexCIndex] = pointTriangles;
            }
            if (!pointTriangles.Contains(triangleIdx))
                pointTriangles.Add(triangleIdx);
        }

        enum ECircumCircleState
        {
            Outside = -1,
            On = 0,
            Inside = 1,
        }
        /** 
         * The key function in Delaunay Triangulation
         * return true if the TestPoint is WITHIN the triangle circumcircle
         *	http://en.wikipedia.org/wiki/Delaunay_triangulation 
         */
        static unsafe ECircumCircleState GetCircumcircleState(in Triangle triangle, int testPoint, Vector2[] points, in Vector2 gridMin, in Vector2 recipGridSize)
        {
            int numPointsPerTriangle = 3;
            // First off, normalize all the points
            var normalizedPositions = stackalloc Vector2[numPointsPerTriangle];
            normalizedPositions[0].X = (points[triangle.VertexAIndex].X - gridMin.X) * recipGridSize.X;
            normalizedPositions[0].Y = (points[triangle.VertexAIndex].Y - gridMin.Y) * recipGridSize.Y;
            normalizedPositions[1].X = (points[triangle.VertexBIndex].X - gridMin.X) * recipGridSize.X;
            normalizedPositions[1].Y = (points[triangle.VertexBIndex].Y - gridMin.Y) * recipGridSize.Y;
            normalizedPositions[2].X = (points[triangle.VertexCIndex].X - gridMin.X) * recipGridSize.X;
            normalizedPositions[2].Y = (points[triangle.VertexCIndex].Y - gridMin.Y) * recipGridSize.Y;

            var normalizedTestPoint = Vector2.Zero;
            normalizedTestPoint.X = (points[testPoint].X - gridMin.X) * recipGridSize.X;
            normalizedTestPoint.Y = (points[testPoint].Y - gridMin.Y) * recipGridSize.Y;

            // http://en.wikipedia.org/wiki/Delaunay_triangulation - determinant
            float M00 = normalizedPositions[0].X - normalizedTestPoint.X;
            float M01 = normalizedPositions[0].Y - normalizedTestPoint.Y;
            float M02 = normalizedPositions[0].X * normalizedPositions[0].X - normalizedTestPoint.X * normalizedTestPoint.X
                + normalizedPositions[0].Y * normalizedPositions[0].Y - normalizedTestPoint.Y * normalizedTestPoint.Y;

            float M10 = normalizedPositions[1].X - normalizedTestPoint.X;
            float M11 = normalizedPositions[1].Y - normalizedTestPoint.Y;
            float M12 = normalizedPositions[1].X * normalizedPositions[1].X - normalizedTestPoint.X * normalizedTestPoint.X
                + normalizedPositions[1].Y * normalizedPositions[1].Y - normalizedTestPoint.Y * normalizedTestPoint.Y;

            float M20 = normalizedPositions[2].X - normalizedTestPoint.X;
            float M21 = normalizedPositions[2].Y - normalizedTestPoint.Y;
            float M22 = normalizedPositions[2].X * normalizedPositions[2].X - normalizedTestPoint.X * normalizedTestPoint.X
                + normalizedPositions[2].Y * normalizedPositions[2].Y - normalizedTestPoint.Y * normalizedTestPoint.Y;

            float Det = M00 * M11 * M22 + M01 * M12 * M20 + M02 * M10 * M21 - (M02 * M11 * M20 + M01 * M10 * M22 + M00 * M12 * M21);

            // When the vertices are sorted in a counterclockwise order, the determinant is positive if and only if Testpoint lies inside the circumcircle of T.
            if (Det < 0.0f)
            {
                return ECircumCircleState.Outside;
            }
            else
            {
                // On top of the triangle edge
                if (Math.Abs(Det) < 0.00001f)
                {
                    return ECircumCircleState.On;
                }
                else
                {
                    return ECircumCircleState.Inside;
                }
            }
        }
    }
    public struct TriangleTetrahedronMesh
    {
        public struct Triangle
        {
            public int V0;
            public int V1;
            public int V2;
            public int Tet0;
            public int Tet1;
            public int HashCode;

            public Triangle(int v0, int v1, int v2)
            {
                V0 = v0;
                V1 = v1;
                V2 = v2;
                Tet0 = -1;
                Tet1 = -1;

                unsafe
                {
                    var list = stackalloc int[3];
                    list[0] = V0;
                    list[1] = V1;
                    list[2] = V2;

                    for (int i = 0; i < 2; i++)
                    {
                        for (int j = i + 1; j < 3; j++)
                        {
                            if (list[i] > list[j])
                                list[i] = list[j];
                        }
                    }
                    HashCode = list[0].GetHashCode() + list[1].GetHashCode() + list[2].GetHashCode();
                }
            }

            public override bool Equals(object obj)
            {
                if (obj == null)
                    return false;
                return Equals((Triangle)obj);
            }
            public bool Equals(in Triangle tri)
            {
                return (HashCode == tri.HashCode);
            }
            public static bool operator ==(in Triangle left, in Triangle right)
            {
                return left.Equals(right);
            }
            public static bool operator !=(in Triangle left, in Triangle right)
            {
                return !left.Equals(right);
            }
            public override int GetHashCode()
            {
                return HashCode;
            }
            public override string ToString()
            {
                return V0 + "," + V1 + "," + V2;
            }
        }
        public struct Tetrahedron
        {
            public int V0;
            public int V1;
            public int V2;
            public int V3;

            public int Tri0;
            public int Tri1;
            public int Tri2;
            public int Tri3;

            public int HashCode;

            public Tetrahedron(int v0, int v1, int v2, int v3)
            {
                V0 = v0;
                V1 = v1;
                V2 = v2;
                V3 = v3;
                Tri0 = -1;
                Tri1 = -1;
                Tri2 = -1;
                Tri3 = -1;

                unsafe
                {
                    var list = stackalloc int[4];
                    list[0] = V0;
                    list[1] = V1;
                    list[2] = V2;
                    list[3] = V3;

                    for (int i = 0; i < 3; i++)
                    {
                        for (int j = i + 1; j < 4; j++)
                        {
                            if (list[i] > list[j])
                                list[i] = list[j];
                        }
                    }
                    HashCode = list[0].GetHashCode() + list[1].GetHashCode() + list[2].GetHashCode() + list[3].GetHashCode();
                }
            }
            public override bool Equals(object obj)
            {
                if (obj == null)
                    return false;
                return Equals((Tetrahedron)obj);
            }
            public bool Equal(in Tetrahedron tet)
            {
                return (HashCode == tet.HashCode);
            }
            public static bool operator ==(in Tetrahedron left, in Tetrahedron right)
            {
                return left.Equals(right);
            }
            public static bool operator !=(in Tetrahedron left, in Tetrahedron right)
            {
                return !left.Equals(right);
            }
            public override int GetHashCode()
            {
                return HashCode;
            }
            public override string ToString()
            {
                return V0 + "," + V1 + "," + V2;
            }
        }

        public Support.UNativeArray<Triangle> Triangles;
        public Support.UNativeArray<Tetrahedron> Tetrahedrons;

        public void Reset()
        {
            Triangles = Support.UNativeArray<Triangle>.CreateInstance();
            Tetrahedrons = Support.UNativeArray<Tetrahedron>.CreateInstance();
        }
        public void Clear()
        {
            Triangles.Clear();
            Tetrahedrons.Clear();
        }
        public int Insert(int v0, int v1, int v2, int v3)
        {
            unsafe
            {
                var tet = new Tetrahedron(v0, v1, v2, v3);
                if (Tetrahedrons.Contains(tet))
                    return -1;

                var tetIdx = Tetrahedrons.Count;

                var tri = new Triangle(tet.V0, tet.V1, tet.V2);
                var triIdx = Triangles.IndexOf(tri);
                if (triIdx < 0)
                {
                    tri.Tet0 = tetIdx;
                    tet.Tri0 = Triangles.Count;
                    Triangles.Add(tri);
                }
                else
                {
                    var triPtr = Triangles.UnsafeGetElementAddress(triIdx);
                    if((*triPtr).Tet1 != -1)
                    {
                        Profiler.Log.WriteLine(Profiler.ELogTag.Fatal, "EdgeMesh", "Try to create edge mesh");
                        return -1;
                    }
                    (*triPtr).Tet1 = tetIdx;
                    tet.Tri0 = triIdx;
                }

                tri = new Triangle(tet.V0, tet.V3, tet.V2);
                triIdx = Triangles.IndexOf(tri);
                if(triIdx < 0)
                {
                    tri.Tet0 = tetIdx;
                    tet.Tri1 = Triangles.Count;
                    Triangles.Add(tri);
                }
                else
                {
                    var triPtr = Triangles.UnsafeGetElementAddress(triIdx);
                    if((*triPtr).Tet1 != -1)
                    {
                        Profiler.Log.WriteLine(Profiler.ELogTag.Fatal, "EdgeMesh", "Try to create edge mesh");
                        return -1;
                    }
                    (*triPtr).Tet1 = tetIdx;
                    tet.Tri1 = triIdx;
                }

                tri = new Triangle(tet.V0, tet.V1, tet.V3);
                triIdx = Triangles.IndexOf(tri);
                if (triIdx < 0)
                {
                    tri.Tet0 = tetIdx;
                    tet.Tri2 = Triangles.Count;
                    Triangles.Add(tri);
                }
                else
                {
                    var triPtr = Triangles.UnsafeGetElementAddress(triIdx);
                    if ((*triPtr).Tet1 != -1)
                    {
                        Profiler.Log.WriteLine(Profiler.ELogTag.Fatal, "EdgeMesh", "Try to create edge mesh");
                        return -1;
                    }
                    (*triPtr).Tet1 = tetIdx;
                    tet.Tri2 = triIdx;
                }

                tri = new Triangle(tet.V0, tet.V2, tet.V1);
                triIdx = Triangles.IndexOf(tri);
                if (triIdx < 0)
                {
                    tri.Tet0 = tetIdx;
                    tet.Tri3 = Triangles.Count;
                    Triangles.Add(tri);
                }
                else
                {
                    var triPtr = Triangles.UnsafeGetElementAddress(triIdx);
                    if ((*triPtr).Tet1 != -1)
                    {
                        Profiler.Log.WriteLine(Profiler.ELogTag.Fatal, "EdgeMesh", "Try to create edge mesh");
                        return -1;
                    }
                    (*triPtr).Tet1 = tetIdx;
                    tet.Tri3 = triIdx;
                }

                Tetrahedrons.Add(tet);
                return tetIdx;
            }
        }
        public bool Remove(int v0, int v1, int v2, int v3)
        {
            var tet = new Tetrahedron(v0, v1, v2, v3);
            var tetIdx = Tetrahedrons.IndexOf(tet);
            if (tetIdx < 0)
                return false;

            unsafe
            {
                var tetPtr = Tetrahedrons.UnsafeGetElementAddress(tetIdx);
                var removeTriIdxes = Support.UNativeArray<int>.CreateInstance();
                if(Triangles.Count > 0)
                {
                    var triPtr = Triangles.UnsafeGetElementAddress(0);
                    for (int i = 0; i < Triangles.Count; i++)
                    {
                        var curPtr = triPtr + (i * sizeof(Triangle));
                        if ((*curPtr).Tet0 == tetIdx)
                            (*curPtr).Tet0 = -1;
                        else if ((*curPtr).Tet0 > tetIdx)
                            (*curPtr).Tet0--;
                        if ((*curPtr).Tet1 == tetIdx)
                            (*curPtr).Tet1 = -1;
                        else if ((*curPtr).Tet1 > tetIdx)
                            (*curPtr).Tet1--;

                        if ((*curPtr).Tet0 == -1 && (*curPtr).Tet1 == -1)
                            removeTriIdxes.Add(i);
                    }

                    foreach(var removeTriIdx in removeTriIdxes)
                    {
                        for(int ti=0; ti < Tetrahedrons.Count; ti++)
                        {
                            var tt = Tetrahedrons.UnsafeGetElementAddress(ti);
                            if ((*tt).Tri0 > removeTriIdx)
                                (*tt).Tri0--;
                            if ((*tt).Tri1 > removeTriIdx)
                                (*tt).Tri1--;
                            if ((*tt).Tri2 > removeTriIdx)
                                (*tt).Tri2--;
                            if ((*tt).Tri3 > removeTriIdx)
                                (*tt).Tri3--;
                        }
                        Triangles.RemoveAt(removeTriIdx);
                    }
                }
                removeTriIdxes.Dispose();
                Tetrahedrons.RemoveAt(tetIdx);
            }
            return true;
        }
        public static int ToPlane(in Vector3 test, in Vector3 v0, in Vector3 v1, in Vector3 v2)
        {
            float x0 = test.X - v0.X;
            float y0 = test.Y - v0.Y;
            float z0 = test.Z - v0.Z;
            float x1 = v1.X - v0.X;
            float y1 = v1.Y - v0.Y;
            float z1 = v1.Z - v0.Z;
            float x2 = v2.X - v0.X;
            float y2 = v2.Y - v0.Y;
            float z2 = v2.Z - v0.Z;
            float y1z2 = y1 * z2;
            float y2z1 = y2 * z1;
            float y2z0 = y2 * z0;
            float y0z2 = y0 * z2;
            float y0z1 = y0 * z1;
            float y1z0 = y1 * z0;
            float c0 = y1z2 - y2z1;
            float c1 = y2z0 - y0z2;
            float c2 = y0z1 - y1z0;
            float x0c0 = x0 * c0;
            float x1c1 = x1 * c1;
            float x2c2 = x2 * c2;
            float term = x0c0 + x1c1;
            float det = term + x2c2;

            return det > 0.0f ? 1 : (det < 0.0f ? -1 : 0);
        }
        public static int ToCircumsphere(in Vector3 test, in Vector3 v0, in Vector3 v1, in Vector3 v2, in Vector3 v3)
        {
            float x0 = v0.X - test.X;
            float y0 = v0.Y - test.Y;
            float z0 = v0.Z - test.Z;
            float s00 = v0.X + test.X;
            float s01 = v0.Y + test.Y;
            float s02 = v0.Z + test.Z;
            float t00 = s00 * x0;
            float t01 = s01 * y0;
            float t02 = s02 * z0;
            float t00pt01 = t00 + t01;
            float w0 = t00pt01 + t02;

            float x1 = v1.X - test.X;
            float y1 = v1.Y - test.Y;
            float z1 = v1.Z - test.Z;
            float s10 = v1.X + test.X;
            float s11 = v1.Y + test.Y;
            float s12 = v1.Z + test.Z;
            float t10 = s10 * x1;
            float t11 = s11 * y1;
            float t12 = s12 * z1;
            float t10pt11 = t10 + t11;
            float w1 = t10pt11 + t12;

            float x2 = v2.X - test.X;
            float y2 = v2.Y - test.Y;
            float z2 = v2.Z - test.Z;
            float s20 = v2.X + test.X;
            float s21 = v2.Y + test.Y;
            float s22 = v2.Z + test.Z;
            float t20 = s20 * x2;
            float t21 = s21 * y2;
            float t22 = s22 * z2;
            float t20pt21 = t20 + t21;
            float w2 = t20pt21 + t22;

            float x3 = v3.X - test.X;
            float y3 = v3.X - test.Y;
            float z3 = v3.Z - test.Z;
            float s30 = v3.X + test.X;
            float s31 = v3.Y + test.Y;
            float s32 = v3.Z + test.Z;
            float t30 = s30 * x3;
            float t31 = s31 * y3;
            float t32 = s32 * z3;
            float t30pt31 = t30 + t31;
            float w3 = t30pt31 + t32;

            float x0y1 = x0 * y1;
            float x0y2 = x0 * y2;
            float x0y3 = x0 * y3;
            float x1y0 = x1 * y0;
            float x1y2 = x1 * y2;
            float x1y3 = x1 * y3;
            float x2y0 = x2 * y0;
            float x2y1 = x2 * y1;
            float x2y3 = x2 * y3;
            float x3y0 = x3 * y0;
            float x3y1 = x3 * y1;
            float x3y2 = x3 * y2;
            float a0 = x0y1 - x1y0;
            float a1 = x0y2 - x2y0;
            float a2 = x0y3 - x3y0;
            float a3 = x1y2 - x2y1;
            float a4 = x1y3 - x3y1;
            float a5 = x2y3 - x3y2;

            float z0w1 = z0 * w1;
            float z0w2 = z0 * w2;
            float z0w3 = z0 * w3;
            float z1w0 = z1 * w0;
            float z1w2 = z1 * w2;
            float z1w3 = z1 * w3;
            float z2w0 = z2 * w0;
            float z2w1 = z2 * w1;
            float z2w3 = z2 * w3;
            float z3w0 = z3 * w0;
            float z3w1 = z3 * w1;
            float z3w2 = z3 * w2;
            float b0 = z0w1 - z1w0;
            float b1 = z0w2 - z2w0;
            float b2 = z0w3 - z3w0;
            float b3 = z1w2 - z2w1;
            float b4 = z1w3 - z3w1;
            float b5 = z2w3 - z3w2;
            float a0b5 = a0 * b5;
            float a1b4 = a1 * b4;
            float a2b3 = a2 * b3;
            float a3b2 = a3 * b2;
            float a4b1 = a4 * b1;
            float a5b0 = a5 * b0;
            float term0 = a0b5 - a1b4;
            float term1 = term0 + a2b3;
            float term2 = term1 + a3b2;
            float term3 = term2 - a4b1;
            float det = term3 + a5b0;

            return (det > 0.0f ? 1 : (det < 0.0f ? -1 : 0));
        }
        public int GetContainingTetrahedron(int pointIdx, Vector3[] points)
        {
            int retValue = -1;
            unsafe
            {
                var tet = Tetrahedrons.UnsafeGetElementAddress(0);
                for (int i = 0; i < Tetrahedrons.Count; i++)
                {
                    if(ToPlane(points[pointIdx], points[(*tet).V1], points[(*tet).V2], points[(*tet).V3]) > 0)
                    {
                        var tri = Triangles.UnsafeGetElementAddress((*tet).Tri0);
                        var opTetIdx = ((*tri).Tet0 == i) ? (*tri).Tet1 : (*tri).Tet0;
                        if (opTetIdx >= 0)
                        {
                            tet = Tetrahedrons.UnsafeGetElementAddress(opTetIdx);
                            retValue = opTetIdx;
                            continue;
                        }
                        else
                            return -1;
                    }
                    if (ToPlane(points[pointIdx], points[(*tet).V0], points[(*tet).V3], points[(*tet).V2]) > 0)
                    {
                        var tri = Triangles.UnsafeGetElementAddress((*tet).Tri1);
                        var opTetIdx = ((*tri).Tet0 == i) ? (*tri).Tet1 : (*tri).Tet0;
                        if (opTetIdx >= 0)
                        {
                            tet = Tetrahedrons.UnsafeGetElementAddress(opTetIdx);
                            retValue = opTetIdx;
                            continue;
                        }
                        else
                            return -1;
                    }
                    if (ToPlane(points[pointIdx], points[(*tet).V0], points[(*tet).V1], points[(*tet).V3]) > 0)
                    {
                        var tri = Triangles.UnsafeGetElementAddress((*tet).Tri2);
                        var opTetIdx = ((*tri).Tet0 == i) ? (*tri).Tet1 : (*tri).Tet0;
                        if (opTetIdx >= 0)
                        {
                            tet = Tetrahedrons.UnsafeGetElementAddress(opTetIdx);
                            retValue = opTetIdx;
                            continue;
                        }
                        else
                            return -1;
                    }
                    if (ToPlane(points[pointIdx], points[(*tet).V0], points[(*tet).V2], points[(*tet).V1]) > 0)
                    {
                        var tri = Triangles.UnsafeGetElementAddress((*tet).Tri3);
                        var opTetIdx = ((*tri).Tet0 == i) ? (*tri).Tet1 : (*tri).Tet0;
                        if (opTetIdx >= 0)
                        {
                            tet = Tetrahedrons.UnsafeGetElementAddress(opTetIdx);
                            retValue = opTetIdx;
                            continue;
                        }
                        else
                            return -1;
                    }

                    return retValue;
                }
            }
            return -1;
        }
        public int GetAdjanceTetrahedron(int triIdx, int tetIdx)
        {
            unsafe
            {
                if (tetIdx < 0 || tetIdx >= Tetrahedrons.Count)
                    return -1;
                if (triIdx < 0 || triIdx >= Triangles.Count)
                    return -1;
                var tet = Tetrahedrons.UnsafeGetElementAddress(tetIdx);
                Triangle* tri = null;
                switch(triIdx)
                {
                    case 0:
                        tri = Triangles.UnsafeGetElementAddress((*tet).Tri0);
                        break;
                    case 1:
                        tri = Triangles.UnsafeGetElementAddress((*tet).Tri1);
                        break;
                    case 2:
                        tri = Triangles.UnsafeGetElementAddress((*tet).Tri2);
                        break;
                    case 3:
                        tri = Triangles.UnsafeGetElementAddress((*tet).Tri3);
                        break;
                }
                if(tri != null)
                {
                    return ((*tri).Tet0 == tetIdx) ? (*tri).Tet1 : (*tri).Tet0;
                }
                return -1;
            }
        }
    }
    public struct EdgeTriangleMesh
    {
        public struct Edge
        {
            public int V0;
            public int V1;
            public int Tri0;
            public int Tri1;

            public Edge(int v0, int v1)
            {
                V0 = v0;
                V1 = v1;
                Tri0 = -1;
                Tri1 = -1;
            }

            public override bool Equals(object obj)
            {
                if (obj == null)
                    return false;
                return Equals((Edge)obj);
            }
            public bool Equals(in Edge edge)
            {
                if (((edge.V0 == V0) && (edge.V1 == V1)) ||
                   ((edge.V0 == V1) && (edge.V1 == V0)))
                    return true;
                return false;
            }
            public override int GetHashCode()
            {
                var vmin = Math.Min(V0, V1);
                var vmax = Math.Max(V0, V1);
                return (vmin + "," + vmax).GetHashCode();
            }
            public override string ToString()
            {
                return V0 + "," + V1;
            }
            public static bool operator ==(in Edge left, in Edge right)
            {
                return left.Equals(right);
            }
            public static bool operator !=(in Edge left, in Edge right)
            {
                return !left.Equals(right);
            }
        }
        public struct Triangle
        {
            public int V0;
            public int V1;
            public int V2;
            public int EdgeIndex0;
            public int EdgeIndex1;
            public int EdgeIndex2;
            public int HashCode;

            public Triangle(int v0, int v1, int v2)
            {
                V0 = v0;
                V1 = v1;
                V2 = v2;
                EdgeIndex0 = -1;
                EdgeIndex1 = -1;
                EdgeIndex2 = -1;

                unsafe
                {
                    var list = stackalloc int[3];
                    list[0] = V0;
                    list[1] = V1;
                    list[2] = V2;

                    for (int i = 0; i < 2; i++)
                    {
                        for (int j = i + 1; j < 3; j++)
                        {
                            if (list[i] > list[j])
                                list[i] = list[j];
                        }
                    }
                    HashCode = list[0].GetHashCode() + list[1].GetHashCode() + list[2].GetHashCode();
                }
            }

            public override bool Equals(object obj)
            {
                if (obj == null)
                    return false;
                return Equals((Triangle)obj);
            }
            public bool Equals(in Triangle tri)
            {
                return (HashCode == tri.HashCode);
            }
            public static bool operator ==(in Triangle left, in Triangle right)
            {
                return left.Equals(right);
            }
            public static bool operator !=(in Triangle left, in Triangle right)
            {
                return !left.Equals(right);
            }
            public override int GetHashCode()
            {
                return HashCode;
            }
            public override string ToString()
            {
                return V0 + "," + V1 + "," + V2;
            }
        }

        public Support.UNativeArray<Edge> Edges;
        public Support.UNativeArray<Triangle> Triangles;

        public void Reset()
        {
            Edges = Support.UNativeArray<Edge>.CreateInstance();
            Triangles = Support.UNativeArray<Triangle>.CreateInstance();
        }
        public void Clear()
        {
            Edges.Clear();
            Triangles.Clear();
        }
        static unsafe int FindEdge(in Support.UNativeArray<Edge> edges, in Edge srcEdge)
        {
            for(int i=0; i<edges.Count; i++)
            {
                Edge* edge = edges.UnsafeGetElementAddress(i);
                if(*edge == srcEdge)
                    return i;
            }
            return -1;
        }
        public int Insert(int v0, int v1, int v2)
        {
            unsafe
            {
                Triangle tri = new Triangle(v0, v1, v2);
                if (Triangles.Contains(tri))
                    return -1;

                var triIdx = Triangles.Count;
                var edge = new Edge(tri.V2, tri.V0);
                var tempEdgeIdx = FindEdge(Edges, edge);
                if (tempEdgeIdx >= 0)
                {
                    Edge* tempEdge = Edges.UnsafeGetElementAddress(tempEdgeIdx);
                    if ((*tempEdge).Tri1 != -1)
                    {
                        Profiler.Log.WriteLine(Profiler.ELogTag.Fatal, "EdgeMesh", "Try to create edge mesh");
                        return -1;
                    }
                    (*tempEdge).Tri1 = triIdx;
                    tri.EdgeIndex2 = tempEdgeIdx;
                }
                else
                {
                    edge.Tri0 = triIdx;
                    tri.EdgeIndex2 = Edges.Count;
                    Edges.Add(edge);
                }

                edge = new Edge(tri.V0, tri.V1);
                tempEdgeIdx = FindEdge(Edges, edge);
                if (tempEdgeIdx >= 0)
                {
                    Edge* tempEdge = Edges.UnsafeGetElementAddress(tempEdgeIdx);
                    if ((*tempEdge).Tri1 != -1)
                    {
                        Profiler.Log.WriteLine(Profiler.ELogTag.Fatal, "EdgeMesh", "Try to create edge mesh");
                        return -1;
                    }
                    (*tempEdge).Tri1 = triIdx;
                    tri.EdgeIndex0 = tempEdgeIdx;
                }
                else
                {
                    edge.Tri0 = triIdx;
                    tri.EdgeIndex0 = Edges.Count;
                    Edges.Add(edge);
                }

                edge = new Edge(tri.V1, tri.V2);
                tempEdgeIdx = FindEdge(Edges, edge);
                if (tempEdgeIdx >= 0)
                {
                    Edge* tempEdge = Edges.UnsafeGetElementAddress(tempEdgeIdx);
                    if ((*tempEdge).Tri1 != -1)
                    {
                        Profiler.Log.WriteLine(Profiler.ELogTag.Fatal, "EdgeMesh", "Try to create edge mesh");
                        return -1;
                    }
                    (*tempEdge).Tri1 = triIdx;
                    tri.EdgeIndex1 = tempEdgeIdx;
                }
                else
                {
                    edge.Tri0 = triIdx;
                    tri.EdgeIndex1 = Edges.Count;
                    Edges.Add(edge);
                }

                Triangles.Add(tri);
                return triIdx;
            }
        }
        public bool Remove(int v0, int v1, int v2)
        {
            var tri = new Triangle(v0, v1, v2);
            var triIdx = Triangles.IndexOf(tri);
            if (triIdx < 0)
                return false;

            unsafe
            {
                var triPtr = Triangles.UnsafeGetElementAddress(triIdx);
                var removeEdgeIdxes = Support.UNativeArray<int>.CreateInstance();
                if(Edges.Count > 0)
                {
                    var edgePtr = Edges.UnsafeGetElementAddress(0);
                    for(int i=0; i < Edges.Count; i++)
                    {
                        var curPtr = edgePtr + (i * sizeof(Edge));
                        if ((*curPtr).Tri0 == triIdx)
                            (*curPtr).Tri0 = -1;
                        else if ((*curPtr).Tri0 > triIdx)
                            (*curPtr).Tri0--;
                        if ((*curPtr).Tri1 == triIdx)
                            (*curPtr).Tri1 = -1;
                        else if ((*curPtr).Tri1 > triIdx)
                            (*curPtr).Tri1--;

                        if((*curPtr).Tri0 == -1 && (*curPtr).Tri1 == -1)
                        {
                            removeEdgeIdxes.Add(i);
                        }
                    }

                    for(int i=0; i<removeEdgeIdxes.Count; i++)
                    {
                        var removeEdgeIdx = removeEdgeIdxes[i];
                        for (int ti=0; ti<Triangles.Count; ti++)
                        {
                            var tt = Triangles.UnsafeGetElementAddress(ti);
                            if ((*tt).EdgeIndex0 > removeEdgeIdx)
                                (*tt).EdgeIndex0--;
                            if ((*tt).EdgeIndex1 > removeEdgeIdx)
                                (*tt).EdgeIndex1--;
                            if ((*tt).EdgeIndex2 > removeEdgeIdx)
                                (*tt).EdgeIndex2--;
                        }
                        Edges.RemoveAt(removeEdgeIdx);
                    }
                }
                removeEdgeIdxes.Dispose();
                Triangles.RemoveAt(triIdx);
            }
            return true;
        }

        public static int ToLine(in Vector2 test, in Vector2 v0, in Vector2 v1)
        {
            float x0 = test.X - v0.X;
            float y0 = test.Y - v0.Y;
            float x1 = v1.X - v0.X;
            float y1 = v1.Y - v0.Y;
            float x0y1 = x0 * y1;
            float x1y0 = x1 * y0;
            float det = x0y1 - x1y0;

            return (det > 0.0f ? +1 : (det < 0.0f ? -1 : 0));
        }
        public static int ToCircumcircle(in Vector2 test, in Vector2 v0, in Vector2 v1, in Vector2 v2)
        {
            float x0 = v0.X - test.X;
            float y0 = v0.Y - test.Y;
            float s00 = v0.X + test.X;
            float s01 = v0.Y + test.Y;
            float t00 = s00 * x0;
            float t01 = s01 * y0;
            float z0 = t00 + t01;

            float x1 = v1.X - test.X;
            float y1 = v1.Y - test.Y;
            float s10 = v1.X + test.X;
            float s11 = v1.Y + test.Y;
            float t10 = s10 * x1;
            float t11 = s11 * y1;
            float z1 = t10 + t11;

            float x2 = v2.X - test.X;
            float y2 = v2.Y - test.Y;
            float s20 = v2.X + test.X;
            float s21 = v2.Y + test.Y;
            float t20 = s20 * x2;
            float t21 = s21 * y2;
            float z2 = t20 + t21;

            float y0z1 = y0 * z1;
            float y0z2 = y0 * z2;
            float y1z0 = y1 * z0;
            float y1z2 = y1 * z2;
            float y2z0 = y2 * z0;
            float y2z1 = y2 * z1;
            float c0 = y1z2 - y2z1;
            float c1 = y2z0 - y0z2;
            float c2 = y0z1 - y1z0;
            float x0c0 = x0 * c0;
            float x1c1 = x1 * c1;
            float x2c2 = x2 * c2;
            float term = x0c0 + x1c1;
            float det = term + x2c2;

            return (det < 0.0f ? 1 : (det > 0.0f ? -1 : 0));
        }

        public int GetContainingTriangle(int pointIdx, Vector2[] points)
        {
            int retValue = -1;
            unsafe
            {
                Triangle* tri = Triangles.UnsafeGetElementAddress(0);
                for (int i = 0; i < Triangles.Count; i++)
                {
                    if(ToLine(points[pointIdx], points[(*tri).V0], points[(*tri).V1]) > 0)
                    {
                        var edge = Edges.UnsafeGetElementAddress((*tri).EdgeIndex0);
                        var opTriIdx = ((*edge).Tri0 == i) ? (*edge).Tri1 : (*edge).Tri0;
                        if (opTriIdx >= 0)
                        {
                            tri = Triangles.UnsafeGetElementAddress(opTriIdx);
                            retValue = opTriIdx;
                            continue;
                        }
                        else
                            return -1;
                    }
                    if(ToLine(points[pointIdx], points[(*tri).V1], points[(*tri).V2]) > 0)
                    {
                        var edge = Edges.UnsafeGetElementAddress((*tri).EdgeIndex1);
                        var opTriIdx = ((*edge).Tri0 == i) ? (*edge).Tri1 : (*edge).Tri0;
                        if (opTriIdx >= 0)
                        {
                            tri = Triangles.UnsafeGetElementAddress(opTriIdx);
                            retValue = opTriIdx;
                            continue;
                        }
                        else
                            return -1;
                    }
                    if (ToLine(points[pointIdx], points[(*tri).V2], points[(*tri).V1]) > 0)
                    {
                        var edge = Edges.UnsafeGetElementAddress((*tri).EdgeIndex2);
                        var opTriIdx = ((*edge).Tri0 == i) ? (*edge).Tri1 : (*edge).Tri0;
                        if (opTriIdx >= 0)
                        {
                            tri = Triangles.UnsafeGetElementAddress(opTriIdx);
                            retValue = opTriIdx;
                            continue;
                        }
                        else
                            return -1;
                    }

                    // The point is inside all four edges, so the point is inside a triangle.
                    return retValue;
                }
            }

            return -1;
        }
        public int GetAdjanceTriangle(int edgeIdx, int triIdx)
        {
            unsafe
            {
                if (triIdx < 0 || triIdx >= Triangles.Count)
                    return -1;
                if (edgeIdx < 0 || edgeIdx >= Edges.Count)
                    return -1;
                var tri = Triangles.UnsafeGetElementAddress(triIdx);
                Edge* edge = null;
                switch (edgeIdx)
                {
                    case 0:
                        edge = Edges.UnsafeGetElementAddress((*tri).EdgeIndex0);
                        break;
                    case 1:
                        edge = Edges.UnsafeGetElementAddress((*tri).EdgeIndex1);
                        break;
                    case 2:
                        edge = Edges.UnsafeGetElementAddress((*tri).EdgeIndex2);
                        break;
                }
                if(edge != null)
                {
                    return ((*edge).Tri0 == triIdx) ? (*edge).Tri1 : (*edge).Tri0;
                }
                return -1;
            }
        }
    }

    struct PointInfo2D
    {
        public int Dimension;
        public int Extreme0;
        public int Extreme1;
        public int Extreme2;
        public Vector2 Min;
        public Vector2 Max;
        public float MaxRange;

        public Vector2 Origin;
        public Vector2 Direction0;
        public Vector2 Direction1;

        public bool ExtremeCCW;

        public unsafe PointInfo2D(Vector2[] points, float epsilon)
        {
            Dimension = 0;
            Extreme0 = 0;
            Extreme1 = 0;
            Extreme2 = 0;
            Min = Vector2.Zero;
            Max = Vector2.Zero;
            MaxRange = 0;
            Origin = Vector2.Zero;
            Direction0 = Vector2.Zero;
            Direction1 = Vector2.Zero;
            ExtremeCCW = false;
            Initialize(points, epsilon);
        }
        void Initialize(Vector2[] points, float epsilon)
        {
            var eps = Math.Max(epsilon, 0.0f);

            if (points != null && points.Length > 0)
            {
                Int32_2 indexMin = Int32_2.Zero;
                Int32_2 indexMax = Int32_2.Zero;
                Min = points[0];
                Max = points[0];
                for (int i = 0; i < points.Length; i++)
                {
                    if (points[i].X < Min.X)
                    {
                        Min.X = points[i].X;
                        indexMin.X = i;
                    }
                    if (points[i].X > Max.X)
                    {
                        Max.X = points[i].X;
                        indexMax.X = i;
                    }
                    if (points[i].Y < Min.Y)
                    {
                        Min.Y = points[i].Y;
                        indexMin.Y = i;
                    }
                    if (points[i].Y > Max.Y)
                    {
                        Max.Y = points[i].Y;
                        indexMax.Y = i;
                    }
                }

                if ((Max.X - Min.X) > (Max.Y - Min.Y))
                {
                    Extreme0 = indexMin.X;
                    Extreme1 = indexMax.X;
                    MaxRange = Max.X - Min.X;
                }
                else
                {
                    Extreme0 = indexMin.Y;
                    Extreme1 = indexMax.Y;
                    MaxRange = Max.Y - Min.Y;
                }

                Origin = points[Extreme0];
                if (MaxRange <= eps)
                {
                    Dimension = 0;
                    Extreme1 = Extreme2 = Extreme0;
                    return;
                }

                // Test whether the vector set is nearly a line segment.
                Direction0 = points[Extreme1] - Origin;
                Direction0.Normalize();
                Direction1 = new Vector2(-Direction0.Y, Direction0.X);

                // Compute the maximum distance of the points from the line origin + t * direction[0].
                float maxDistance = 0.0f;
                float maxSign = 0.0f;
                Extreme2 = Extreme0;
                for(int i=0; i< points.Length; i++)
                {
                    var diff = points[i] - Origin;
                    float distance = (Direction0 - diff).LengthSquared();
                    float sign = (distance > 0.0f ? 1.0f : (distance < 0.0f ? -1.0f : 0.0f));
                    distance = Math.Abs(distance);
                    if(distance > maxDistance)
                    {
                        maxDistance = distance;
                        maxSign = sign;
                        Extreme2 = i;
                    }
                }

                if(maxDistance <= eps * MaxRange)
                {
                    Dimension = 1;
                    Extreme2 = Extreme1;
                    return;
                }

                Dimension = 2;
                ExtremeCCW = maxSign > 0.0f;
            }
        }
    }
    struct PointInfo3D
    {
        public int Dimension;
        public int Extreme0;
        public int Extreme1;
        public int Extreme2;
        public int Extreme3;
        public Vector3 Min;
        public Vector3 Max;
        public float MaxRange;

        public Vector3 Origin;
        public Vector3 Direction0;
        public Vector3 Direction1;
        public Vector3 Direction2;

        public bool ExtremeCCW;

        public unsafe PointInfo3D(Vector3[] points, float epsilon)
        {
            Dimension = 0;
            Extreme0 = 0;
            Extreme1 = 0;
            Extreme2 = 0;
            Extreme3 = 0;
            Min = Vector3.Zero;
            Max = Vector3.Zero;
            MaxRange = 0;
            Origin = Vector3.Zero;
            Direction0 = Vector3.Zero;
            Direction1 = Vector3.Zero;
            Direction2 = Vector3.Zero;
            ExtremeCCW = false;
            Initialize(points, epsilon);
        }
        void Initialize(Vector3[] points, float epsilon)
        {
            var eps = Math.Max(epsilon, 0.0f);

            if(points != null && points.Length > 0)
            {
                Int32_3 indexMin = Int32_3.Zero;
                Int32_3 indexMax = Int32_3.Zero;
                Min = points[0];
                Max = points[0];
                for(int i=0; i<points.Length; i++)
                {
                    if(points[i].X < Min.X)
                    {
                        Min.X = points[i].X;
                        indexMin.X = i;
                    }
                    if(points[i].X > Max.X)
                    {
                        Max.X = points[i].X;
                        indexMax.X = i;
                    }
                    if (points[i].Y < Min.Y)
                    {
                        Min.Y = points[i].Y;
                        indexMin.Y = i;
                    }
                    if (points[i].Y > Max.Y)
                    {
                        Max.Y = points[i].Y;
                        indexMax.Y = i;
                    }
                    if (points[i].Z < Min.Z)
                    {
                        Min.Z = points[i].Z;
                        indexMin.Z = i;
                    }
                    if (points[i].Z > Max.Z)
                    {
                        Max.Z = points[i].Z;
                        indexMax.Z = i;
                    }
                }

                Extreme0 = indexMin.X;
                Extreme1 = indexMax.X;
                MaxRange = Max.X - Min.X;
                if((Max.Y - Min.Y) > MaxRange)
                {
                    MaxRange = Max.Y - Min.Y;
                    Extreme0 = indexMin.Y;
                    Extreme1 = indexMax.Y;
                }
                if((Max.Z - Min.Z) > MaxRange)
                {
                    MaxRange = Max.Z - Min.Z;
                    Extreme0 = indexMin.Z;
                    Extreme1 = indexMax.Z;
                }

                Origin = points[Extreme0];
                if(MaxRange <= eps)
                {
                    Dimension = 0;
                    Extreme1 = Extreme2 = Extreme3 = Extreme0;
                    return;
                }

                // Test whether the vector set is nearly a line segment.
                Direction0 = points[Extreme1] - Origin;
                Direction0.Normalize();
                if(Math.Abs(Direction0.X) > Math.Abs(Direction0.Y))
                {
                    Direction1.X = -Direction0.Z;
                    Direction1.Y = 0.0f;
                    Direction1.Z = Direction0.X;
                }
                else
                {
                    Direction1.X = 0.0f;
                    Direction1.Y = Direction0.Z;
                    Direction1.Z = -Direction0.Y;
                }
                Direction1.Normalize();
                Direction2 = Vector3.Normalize(Vector3.Cross(in Direction0, in Direction1));

                // Compute the maximum distance of the points from the line origin + t * direction[0].
                float maxDistance = 0.0f;
                Extreme2 = Extreme0;
                for(int i=0; i<points.Length; i++)
                {
                    var diff = points[i] - Origin;
                    var dot = Vector3.Dot(in Direction0, diff);
                    var proj = diff - dot * Direction0;
                    var distance = proj.Length();
                    if(distance > maxDistance)
                    {
                        maxDistance = distance;
                        Extreme2 = i;
                    }
                }

                if(maxDistance <= eps * MaxRange)
                {
                    Dimension = 1;
                    Extreme2 = Extreme1;
                    Extreme3 = Extreme1;
                    return;
                }

                // Test whether the vector set is nearly a plane.
                Direction1 = points[Extreme2] - Origin;
                Direction1 -= Vector3.Dot(Direction0, Direction1) * Direction0;
                Direction1.Normalize();
                Direction2 = Vector3.Normalize(Vector3.Cross(Direction0, Direction1));

                // Compute the maximum distance of the points from the plane origin + t0 * direction[0] + t1 * direction[1].
                maxDistance = 0.0f;
                float maxSign = 0.0f;
                Extreme3 = Extreme0;
                for(int i=0; i<points.Length; ++i)
                {
                    var diff = points[i] - Origin;
                    var distance = Vector3.Dot(Direction2, diff);
                    var sign = (distance > 0.0f ? 1.0f : (distance < 0.0f ? -1.0f : 0.0f));
                    distance = Math.Abs(distance);
                    if(distance > maxDistance)
                    {
                        maxDistance = distance;
                        maxSign = sign;
                        Extreme3 = i;
                    }
                }

                if(maxDistance <= eps * MaxRange)
                {
                    Dimension = 2;
                    Extreme3 = Extreme2;
                    return;
                }

                Dimension = 3;
                ExtremeCCW = maxSign > 0.0f;
            }
        }
    }

    public struct Delaunay2D
    {
        public EdgeTriangleMesh Graph;
        public int NumUniqueVertices;
        public int Dimension;

        public unsafe bool Calculate(Vector2[] points, float epsilon = MathHelper.Epsilon)
        {
            Graph.Clear();

            if(points.Length < 3)
                return false;

            var ptInfo = new PointInfo2D(points, epsilon);
            Dimension = ptInfo.Dimension;
            if (ptInfo.Dimension != 2)
                return false;

            if(!ptInfo.ExtremeCCW)
            {
                var temp = ptInfo.Extreme1;
                ptInfo.Extreme1 = ptInfo.Extreme2;
                ptInfo.Extreme2 = temp;
            }
            if (Graph.Insert(ptInfo.Extreme0, ptInfo.Extreme1, ptInfo.Extreme2) < 0)
                return false;

            NumUniqueVertices = 0;
            var processed = Support.UNativeArray<byte>.CreateInstance();
            var byteCount = points.Length / 8 + 1;
            processed.mCoreObject.SetSize(byteCount);
            CoreSDK.MemorySet(processed.UnsafeGetElementAddress(0), 0, (uint)byteCount);

            var idx = ptInfo.Extreme0 / 8;
            var subIdx = ptInfo.Extreme0 - idx * 8;
            var processedData = processed.UnsafeGetElementAddress(idx);
            *processedData = (byte)((*processedData) | (1 << subIdx));

            idx = ptInfo.Extreme1 / 8;
            subIdx = ptInfo.Extreme1 - idx * 8;
            processedData = processed.UnsafeGetElementAddress(idx);
            *processedData = (byte)((*processedData) | (1 << subIdx));

            idx = ptInfo.Extreme2 / 8;
            subIdx = ptInfo.Extreme2 - idx * 8;
            processedData = processed.UnsafeGetElementAddress(idx);
            *processedData = (byte)((*processedData) | (1 << subIdx));

            for(int i = 0; i < points.Length; i++)
            {
                idx = i / 8;
                subIdx = i - idx * 8;
                processedData = processed.UnsafeGetElementAddress(idx);
                if (((*processedData) & (1 << subIdx)) != 0)
                    continue;

                if (!Update(i, points))
                    return false;

                *processedData = (byte)((*processedData) | (1 << subIdx));
                NumUniqueVertices++;
            }

            processed.Dispose();

            return true;
        }
        unsafe bool GetAndRemoveInsertionPolygon(int idx, Vector2[] points, ref Support.UNativeArray<int> candidates, ref Support.UNativeArray<int> boundary)
        {
            // Locate the triangles that make up the insertion polygon.
            var polygon = new EdgeTriangleMesh();
            while(candidates.Count > 0)
            {
                var triIdx = candidates[0];
                candidates.RemoveAt(0);

                for(int edgeIdx = 0; edgeIdx < 3; edgeIdx++)
                {
                    var adjTriIdx = Graph.GetAdjanceTriangle(edgeIdx, triIdx);
                    if(adjTriIdx >= 0 && !candidates.Contains(adjTriIdx))
                    {
                        var adjTri = Graph.Triangles.UnsafeGetElementAddress(adjTriIdx);
                        if (EdgeTriangleMesh.ToCircumcircle(points[idx], points[(*adjTri).V0], points[(*adjTri).V1], points[(*adjTri).V2]) <= 0)
                            candidates.Add(adjTriIdx);
                    }
                }

                var tri = Graph.Triangles.UnsafeGetElementAddress(triIdx);
                if (polygon.Insert((*tri).V0, (*tri).V1, (*tri).V2) < 0)
                    return false;
                if (!Graph.Remove((*tri).V0, (*tri).V1, (*tri).V2))
                    return false;
            }

            // Get the boundary edges of the insertion polygon.
            var polygonTriCount = polygon.Triangles.Count;
            if(polygonTriCount > 0)
            {
                var triPtr = polygon.Triangles.UnsafeGetElementAddress(0);
                for(int i = 0; i < polygonTriCount; i++)
                {
                    var curPtr = triPtr[i];
                    var adjTriIdx = polygon.GetAdjanceTriangle(0, i);
                    if(adjTriIdx >= 0)
                    {
                        var bdEdge = new EdgeTriangleMesh.Edge(curPtr.V0, curPtr.V1);
                        var bdEdgeIdx = Graph.Edges.IndexOf(bdEdge);
                        boundary.Add(bdEdgeIdx);
                    }
                    adjTriIdx = polygon.GetAdjanceTriangle(1, i);
                    if(adjTriIdx >= 0)
                    {
                        var bdEdge = new EdgeTriangleMesh.Edge(curPtr.V1, curPtr.V2);
                        var bdEdgeIdx = Graph.Edges.IndexOf(bdEdge);
                        boundary.Add(bdEdgeIdx);
                    }
                    adjTriIdx = polygon.GetAdjanceTriangle(2, i);
                    if (adjTriIdx >= 0)
                    {
                        var bdEdge = new EdgeTriangleMesh.Edge(curPtr.V2, curPtr.V0);
                        var bdEdgeIdx = Graph.Edges.IndexOf(bdEdge);
                        boundary.Add(bdEdgeIdx);
                    }
                }
            }

            return true;
        }
        unsafe bool Update(int idx, Vector2[] points)
        {
            var triIdx = Graph.GetContainingTriangle(idx, points);
            if(triIdx >= 0)
            {
                // The point is inside the convex hull. The insertion polygon contains only triangles in the current triangulation.
                // The hull does not change.

                // Use a depth-first search for those triangles whose circumcircles contain point i.
                var candidates = Support.UNativeArray<int>.CreateInstance();
                candidates.Add(triIdx);

                // Get the boundary of the insertion polygon C that contains the triangles whose circumcircles contain point i.
                // Polygon C contains the point i.
                var boundary = Support.UNativeArray<int>.CreateInstance();
                if (!GetAndRemoveInsertionPolygon(idx, points, ref candidates, ref boundary))
                    return false;

                // The insertion polygon consists of the triangles formed by point i and the faces of C.
                foreach (var bdIdx in boundary)
                {
                    var bd = Graph.Edges.UnsafeGetElementAddress(bdIdx);
                    if(EdgeTriangleMesh.ToLine(points[idx], points[(*bd).V0], points[(*bd).V1]) < 0)
                    {
                        if (Graph.Insert(idx, (*bd).V0, (*bd).V1) < 0)
                            return false;
                    }
                }

                boundary.Dispose();
                candidates.Dispose();
            }
            else
            {
                // The point is outside the convex hull. The insertion polygon is formed by point i and any triangles in the
                // current triangulation whose circumcircles contain point i.

                // Locate the convex hull of the triangles.
                Support.UNativeArray<int> hull = Support.UNativeArray<int>.CreateInstance();
                var triPtr = Graph.Triangles.UnsafeGetElementAddress(0);
                var edgePtr = Graph.Edges.UnsafeGetElementAddress(0);
                for(int i=0; i<Graph.Triangles.Count; i++)
                {
                    var opTri = (edgePtr[triPtr[i].EdgeIndex0].Tri0 == i) ? edgePtr[triPtr[i].EdgeIndex0].Tri1 : edgePtr[triPtr[i].EdgeIndex0].Tri0;
                    if(opTri < 0)
                        hull.Add(triPtr[i].EdgeIndex0);

                    opTri = (edgePtr[triPtr[i].EdgeIndex1].Tri0 == i) ? edgePtr[triPtr[i].EdgeIndex1].Tri1 : edgePtr[triPtr[i].EdgeIndex1].Tri0;
                    if (opTri < 0)
                        hull.Add(triPtr[i].EdgeIndex1);

                    opTri = (edgePtr[triPtr[i].EdgeIndex2].Tri0 == i) ? edgePtr[triPtr[i].EdgeIndex2].Tri1 : edgePtr[triPtr[i].EdgeIndex2].Tri0;
                    if (opTri < 0)
                        hull.Add(triPtr[i].EdgeIndex2);
                }

                Support.UNativeArray<int> candidates = Support.UNativeArray<int>.CreateInstance();
                Support.UNativeArray<int> visible = Support.UNativeArray<int>.CreateInstance();
                foreach(var edgeIdx in hull)
                {
                    var edge = edgePtr[edgeIdx];
                    if(EdgeTriangleMesh.ToLine(points[idx], points[edge.V0], points[edge.V1]) > 0)
                    {
                        if (edge.Tri1 < 0)
                        {
                            if (candidates.Contains(edge.Tri0))
                            {
                                var a0 = triPtr[edge.Tri0].V0;
                                var a1 = triPtr[edge.Tri0].V1;
                                var a2 = triPtr[edge.Tri0].V2;
                                if (EdgeTriangleMesh.ToCircumcircle(points[idx], points[a0], points[a1], points[a2]) <= 0)
                                    candidates.Add(edge.Tri0); // Point i is in the circumcircle.
                                else
                                    visible.Add(edgeIdx); // Point i is not in the circumcircle but the hull edge is visible.
                            }
                        }
                        else
                            return false;
                    }
                }

                // Get the boundary of the insertion subpolygon C that contains the triangles whose circumcircles contain point i.
                Support.UNativeArray<int> boundary = Support.UNativeArray<int>.CreateInstance();
                if(!GetAndRemoveInsertionPolygon(idx, points, ref candidates, ref boundary))
                    return false;

                // The insertion polygon P consists of the triangles formed by point i and the back edges of C and the visible edges of Graph-C.
                foreach (var bdIdx in boundary)
                {
                    var edge = edgePtr[bdIdx];
                    if(EdgeTriangleMesh.ToLine(points[idx], points[edge.V0], points[edge.V1]) < 0)
                    {
                        // This is a back edge of the boundary.
                        if (Graph.Insert(idx, edge.V0, edge.V1) < 0)
                            return false;
                    }
                }
                foreach(var visIdx in visible)
                {
                    var edge = edgePtr[visIdx];
                    if (Graph.Insert(idx, edge.V1, edge.V0) < 0)
                        return false;
                }

                candidates.Dispose();
                visible.Dispose();
                boundary.Dispose();
            }

            return true;
        }
    }

    public struct Delaunay3D
    {
        public TriangleTetrahedronMesh Graph;
        public int NumUniqueVertices;
        public int Dimension;

        public unsafe bool Calculate(Vector3[] points, float epsilon = MathHelper.Epsilon)
        {
            Graph.Clear();

            if (points.Length < 4)
                return false;

            var ptInfo = new PointInfo3D(points, epsilon);
            Dimension = ptInfo.Dimension;
            if (ptInfo.Dimension != 3)
                return false;

            if(!ptInfo.ExtremeCCW)
            {
                var temp = ptInfo.Extreme2;
                ptInfo.Extreme2 = ptInfo.Extreme3;
                ptInfo.Extreme3 = temp;
            }
            if (Graph.Insert(ptInfo.Extreme0, ptInfo.Extreme1, ptInfo.Extreme2, ptInfo.Extreme3) < 0)
                return false;

            var processed = Support.UNativeArray<byte>.CreateInstance();
            var byteCount = points.Length / 8 + 1;
            processed.mCoreObject.SetSize(byteCount);
            CoreSDK.MemorySet(processed.UnsafeGetElementAddress(0), 0, (uint)byteCount);

            var idx = ptInfo.Extreme0 / 8;
            var subIdx = ptInfo.Extreme0 - idx * 8;
            var processedData = processed.UnsafeGetElementAddress(idx);
            *processedData = (byte)((*processedData) | (1 << subIdx));

            idx = ptInfo.Extreme1 / 8;
            subIdx = ptInfo.Extreme1 - idx * 8;
            processedData = processed.UnsafeGetElementAddress(idx);
            *processedData = (byte)((*processedData) | (1 << subIdx));

            idx = ptInfo.Extreme2 / 8;
            subIdx = ptInfo.Extreme2 - idx * 8;
            processedData = processed.UnsafeGetElementAddress(idx);
            *processedData = (byte)((*processedData) | (1 << subIdx));

            idx = ptInfo.Extreme3 / 8;
            subIdx = ptInfo.Extreme3 - idx * 8;
            processedData = processed.UnsafeGetElementAddress(idx);
            *processedData = (byte)((*processedData) | (1 << subIdx));

            for(int i=0; i<points.Length; i++)
            {
                idx = i / 8;
                subIdx = i - idx * 8;
                processedData = processed.UnsafeGetElementAddress(idx);
                if (((*processedData) & (1 << subIdx)) != 0)
                    continue;

                if (!Update(i, points))
                    return false;

                *processedData = (byte)((*processedData) | (1 << subIdx));
                NumUniqueVertices++;
            }

            processed.Dispose();

            return true;
        }
        unsafe bool GetAndRemoveInsertionPolyhedron(int idx, Vector3[] points, ref Support.UNativeArray<int> candidates, ref Support.UNativeArray<int> boundary)
        {
            var polygon = new TriangleTetrahedronMesh();
            while(candidates.Count > 0)
            {
                var tetIdx = candidates[0];
                candidates.RemoveAt(0);

                for(int triIdx = 0; triIdx < 4; ++triIdx)
                {
                    var adjTetIdx = Graph.GetAdjanceTetrahedron(triIdx, tetIdx);
                    if(adjTetIdx >= 0 && !candidates.Contains(adjTetIdx))
                    {
                        var adjTet = Graph.Tetrahedrons.UnsafeGetElementAddress(adjTetIdx);
                        if (TriangleTetrahedronMesh.ToCircumsphere(points[idx], points[(*adjTet).V0], points[(*adjTet).V1], points[(*adjTet).V2], points[(*adjTet).V3]) <= 0)
                            candidates.Add(adjTetIdx);
                    }
                }

                var tet = Graph.Tetrahedrons.UnsafeGetElementAddress(tetIdx);
                if (polygon.Insert((*tet).V0, (*tet).V1, (*tet).V2, (*tet).V3) < 0)
                    return false;
                if (!Graph.Remove((*tet).V0, (*tet).V1, (*tet).V2, (*tet).V3))
                    return false;
            }

            var polygonTetCount = polygon.Tetrahedrons.Count;
            if(polygonTetCount > 0)
            {
                var tetPtr = polygon.Tetrahedrons.UnsafeGetElementAddress(0);
                for(int i=0; i<polygonTetCount; i++)
                {
                    var curPtr = tetPtr[i];
                    var adjTetIdx = polygon.GetAdjanceTetrahedron(0, i);
                    if(adjTetIdx >= 0)
                    {
                        var bdTri = new TriangleTetrahedronMesh.Triangle(curPtr.V1, curPtr.V2, curPtr.V3);
                        var bdTriIdx = Graph.Triangles.IndexOf(bdTri);
                        boundary.Add(bdTriIdx);
                    }
                    adjTetIdx = polygon.GetAdjanceTetrahedron(1, i);
                    if (adjTetIdx >= 0)
                    {
                        var bdTri = new TriangleTetrahedronMesh.Triangle(curPtr.V0, curPtr.V3, curPtr.V2);
                        var bdTriIdx = Graph.Triangles.IndexOf(bdTri);
                        boundary.Add(bdTriIdx);
                    }
                    adjTetIdx = polygon.GetAdjanceTetrahedron(2, i);
                    if (adjTetIdx >= 0)
                    {
                        var bdTri = new TriangleTetrahedronMesh.Triangle(curPtr.V0, curPtr.V1, curPtr.V3);
                        var bdTriIdx = Graph.Triangles.IndexOf(bdTri);
                        boundary.Add(bdTriIdx);
                    }
                    adjTetIdx = polygon.GetAdjanceTetrahedron(3, i);
                    if (adjTetIdx >= 0)
                    {
                        var bdTri = new TriangleTetrahedronMesh.Triangle(curPtr.V0, curPtr.V2, curPtr.V1);
                        var bdTriIdx = Graph.Triangles.IndexOf(bdTri);
                        boundary.Add(bdTriIdx);
                    }
                }
            }

            return true;
        }
        unsafe bool Update(int idx, Vector3[] points)
        {
            var tetIdx = Graph.GetContainingTetrahedron(idx, points);
            if(tetIdx >= 0)
            {
                var candidates = Support.UNativeArray<int>.CreateInstance();
                candidates.Add(tetIdx);

                var boundary = Support.UNativeArray<int>.CreateInstance();
                if (!GetAndRemoveInsertionPolyhedron(idx, points, ref candidates, ref boundary))
                    return false;

                foreach(var bdIdx in boundary)
                {
                    var bd = Graph.Triangles.UnsafeGetElementAddress(bdIdx);
                    if(TriangleTetrahedronMesh.ToPlane(points[idx], points[(*bd).V0], points[(*bd).V1], points[(*bd).V2]) < 0)
                    {
                        if (Graph.Insert(idx, (*bd).V0, (*bd).V1, (*bd).V2) < 0)
                            return false;
                    }
                }

                boundary.Dispose();
                candidates.Dispose();
            }
            else
            {
                Support.UNativeArray<int> hull = Support.UNativeArray<int>.CreateInstance();
                var tetPtr = Graph.Tetrahedrons.UnsafeGetElementAddress(0);
                var triPtr = Graph.Triangles.UnsafeGetElementAddress(0);
                for(int i=0; i<Graph.Tetrahedrons.Count; i++)
                {
                    var opTet = (triPtr[tetPtr[i].Tri0].Tet0 == i) ? triPtr[tetPtr[i].Tri0].Tet1 : triPtr[tetPtr[i].Tri0].Tet0;
                    if (opTet < 0)
                        hull.Add(tetPtr[i].Tri0);
                    opTet = (triPtr[tetPtr[i].Tri1].Tet0 == i) ? triPtr[tetPtr[i].Tri1].Tet1 : triPtr[tetPtr[i].Tri1].Tet0;
                    if (opTet < 0)
                        hull.Add(tetPtr[i].Tri1);
                    opTet = (triPtr[tetPtr[i].Tri2].Tet0 == i) ? triPtr[tetPtr[i].Tri2].Tet1 : triPtr[tetPtr[i].Tri2].Tet0;
                    if (opTet < 0)
                        hull.Add(tetPtr[i].Tri2);
                    opTet = (triPtr[tetPtr[i].Tri3].Tet0 == i) ? triPtr[tetPtr[i].Tri3].Tet1 : triPtr[tetPtr[i].Tri3].Tet0;
                    if (opTet < 0)
                        hull.Add(tetPtr[i].Tri3);
                }

                var candidates = Support.UNativeArray<int>.CreateInstance();
                var visible = Support.UNativeArray<int>.CreateInstance();
                foreach(var triIdx in hull)
                {
                    var tri = triPtr[triIdx];
                    if(TriangleTetrahedronMesh.ToPlane(points[idx], points[tri.V0], points[tri.V1], points[tri.V2]) > 0)
                    {
                        if (tri.Tet1 < 0)
                        {
                            if (candidates.Contains(tri.Tet0))
                            {
                                var a0 = tetPtr[tri.Tet0].V0;
                                var a1 = tetPtr[tri.Tet0].V1;
                                var a2 = tetPtr[tri.Tet0].V2;
                                var a3 = tetPtr[tri.Tet0].V3;
                                if (TriangleTetrahedronMesh.ToCircumsphere(points[idx], points[a0], points[a1], points[a2], points[a3]) <= 0)
                                    candidates.Add(tri.Tet0);
                                else
                                    visible.Add(triIdx);
                            }
                        }
                        else
                            return false;
                    }
                }

                var boundary = Support.UNativeArray<int>.CreateInstance();
                if (!GetAndRemoveInsertionPolyhedron(idx, points, ref candidates, ref boundary))
                    return false;

                foreach(var bdIdx in boundary)
                {
                    var tri = triPtr[bdIdx];
                    if(TriangleTetrahedronMesh.ToPlane(points[idx], points[tri.V0], points[tri.V1], points[tri.V2]) < 0)
                    {
                        if (Graph.Insert(idx, tri.V0, tri.V1, tri.V2) < 0)
                            return false;
                    }
                }
                foreach(var visIdx in visible)
                {
                    var tri = triPtr[visIdx];
                    if (Graph.Insert(idx, tri.V0, tri.V2, tri.V1) < 0)
                        return false;
                }

                candidates.Dispose();
                visible.Dispose();
                boundary.Dispose();
            }

            return true;
        }
    }
}
