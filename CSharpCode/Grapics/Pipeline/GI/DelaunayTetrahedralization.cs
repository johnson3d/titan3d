using System;
using System.Collections.Generic;
using System.Linq;

namespace EngineNS.Graphics.Pipeline.GI
{
    //todo: https://github.com/CGAL/cgal
    public class TtDelaunayTetrahedralization
    {
        #region Data Structures

        [Serializable]
        public class TetrahedronData
        {
            public int[] ProbeIndices { get; set; } = new int[4];
            public Vector3[] Vertices { get; set; } = new Vector3[4];
            public Matrix BarycentricMatrix { get; set; }
            public float Volume { get; set; }
            public BoundingBox Bounds { get; set; }

            public void Precompute()
            {
                // 计算重心坐标转换矩阵
                var m = new Matrix(
                    Vertices[0].x, Vertices[1].x, Vertices[2].x, Vertices[3].x,
                    Vertices[0].y, Vertices[1].y, Vertices[2].y, Vertices[3].y,
                    Vertices[0].z, Vertices[1].z, Vertices[2].z, Vertices[3].z,
                    1f, 1f, 1f, 1f
                );

                Volume = MathF.Abs(m.Determinant()) / 6f;

                if (MathF.Abs(m.Determinant()) > 1e-10f)
                {
                    BarycentricMatrix = Matrix.Invert(m);
                }
                else
                {
                    BarycentricMatrix = Matrix.Identity;
                }

                // 计算包围盒
                Bounds = new BoundingBox(Vertices[0], Vector3.Zero);
                for (int i = 1; i < 4; i++)
                {
                    Bounds.Merge(Vertices[i]);
                }
            }

            public bool IsValid() => Volume > 1e-10f;

            public Vector4 GetBarycentricCoords(Vector3 worldPos)
            {
                var homogeneousPos = new Vector4(worldPos.x, worldPos.y, worldPos.z, 1f);
                return Vector4.Transform(homogeneousPos, BarycentricMatrix);
            }

            public bool ContainsPoint(Vector3 worldPos)
            {
                var baryCoords = GetBarycentricCoords(worldPos);
                return baryCoords.X >= -1e-6f && baryCoords.Y >= -1e-6f &&
                       baryCoords.Z >= -1e-6f && baryCoords.W >= -1e-6f;
            }
        }

        public class Face
        {
            public int[] Vertices { get; set; } = new int[3];
            public Vector3 Normal { get; set; }
            public Vector3 Center { get; set; }
            public int TetrahedronIndex { get; set; } = -1;

            public Face(int v1, int v2, int v3, int tetraIndex = -1)
            {
                Vertices = new[] { v1, v2, v3 };
                TetrahedronIndex = tetraIndex;
            }

            public void ComputeNormalAndCenter(List<Vector3> points)
            {
                var p1 = points[Vertices[0]];
                var p2 = points[Vertices[1]];
                var p3 = points[Vertices[2]];

                // 计算面的中心
                Center = (p1 + p2 + p3) / 3f;

                // 计算法向量
                var edge1 = p2 - p1;
                var edge2 = p3 - p1;
                Normal = Vector3.Cross(edge1, edge2);
                Normal.Normalize();
            }

            public override bool Equals(object obj)
            {
                if (obj is Face other)
                {
                    var sortedThis = Vertices.OrderBy(v => v).ToArray();
                    var sortedOther = other.Vertices.OrderBy(v => v).ToArray();
                    return sortedThis.SequenceEqual(sortedOther);
                }
                return false;
            }

            public override int GetHashCode()
            {
                var sorted = Vertices.OrderBy(v => v).ToArray();
                return sorted[0].GetHashCode() ^ (sorted[1].GetHashCode() << 1) ^ (sorted[2].GetHashCode() >> 1);
            }
        }

        public struct Edge
        {
            public int v1, v2;

            public Edge(int vertex1, int vertex2)
            {
                if (vertex1 < vertex2)
                {
                    v1 = vertex1;
                    v2 = vertex2;
                }
                else
                {
                    v1 = vertex2;
                    v2 = vertex1;
                }
            }

            public override bool Equals(object obj)
            {
                return obj is Edge other && v1 == other.v1 && v2 == other.v2;
            }

            public override int GetHashCode()
            {
                return v1.GetHashCode() ^ (v2.GetHashCode() << 1);
            }
        }

        private class ConflictGraph
        {
            private Dictionary<int, HashSet<Face>> pointToFaces = new Dictionary<int, HashSet<Face>>();
            private Dictionary<Face, HashSet<int>> faceToPoints = new Dictionary<Face, HashSet<int>>();

            public void AddConflict(int pointIndex, Face face)
            {
                if (!pointToFaces.ContainsKey(pointIndex))
                    pointToFaces[pointIndex] = new HashSet<Face>();
                pointToFaces[pointIndex].Add(face);

                if (!faceToPoints.ContainsKey(face))
                    faceToPoints[face] = new HashSet<int>();
                faceToPoints[face].Add(pointIndex);
            }

            public HashSet<Face> GetConflictingFaces(int pointIndex)
            {
                return pointToFaces.TryGetValue(pointIndex, out var faces) ? faces : new HashSet<Face>();
            }

            public HashSet<int> GetConflictingPoints(Face face)
            {
                return faceToPoints.TryGetValue(face, out var points) ? points : new HashSet<int>();
            }

            public void RemoveFace(Face face)
            {
                if (faceToPoints.TryGetValue(face, out var points))
                {
                    foreach (var point in points)
                    {
                        if (pointToFaces.TryGetValue(point, out var faces))
                        {
                            faces.Remove(face);
                        }
                    }
                    faceToPoints.Remove(face);
                }
            }

            public void Clear()
            {
                pointToFaces.Clear();
                faceToPoints.Clear();
            }
        }

        #endregion

        #region Main Algorithm

        public static List<TetrahedronData> ComputeDelaunayTetrahedralization(List<Vector3> points)
        {
            if (points.Count < 4)
            {
                //Debug.LogWarning("Need at least 4 points for tetrahedralization");
                return new List<TetrahedronData>();
            }

            var instance = new TtDelaunayTetrahedralization();
            return instance.BuildTetrahedralization(points);
        }

        private List<TetrahedronData> BuildTetrahedralization(List<Vector3> points)
        {
            var tetrahedra = new List<TetrahedronData>();
            var hull = new List<Face>();
            var conflictGraph = new ConflictGraph();

            try
            {
                // 1. 找到初始四面体
                var initialTetra = FindInitialTetrahedron(points);
                if (initialTetra == null)
                {
                    Profiler.Log.WriteLine<Profiler.TtGraphicsGategory>(Profiler.ELogTag.Error, "Could not find valid initial tetrahedron");
                    return tetrahedra;
                }

                // 2. 初始化凸包面
                var initialFaces = GetTetrahedronFaces(initialTetra);
                foreach (var face in initialFaces)
                {
                    face.ComputeNormalAndCenter(points);
                    hull.Add(face);
                }

                tetrahedra.Add(initialTetra);
                var usedPoints = new HashSet<int>(initialTetra.ProbeIndices);

                // 3. 构建初始冲突图
                BuildInitialConflictGraph(points, hull, usedPoints, conflictGraph);

                // 4. 逐个添加剩余点
                var remainingPoints = Enumerable.Range(0, points.Count)
                    .Where(i => !usedPoints.Contains(i))
                    .OrderBy(i => conflictGraph.GetConflictingFaces(i).Count) // 优先处理冲突少的点
                    .ToList();

                foreach (int pointIndex in remainingPoints)
                {
                    var conflictingFaces = conflictGraph.GetConflictingFaces(pointIndex);
                    if (conflictingFaces.Count == 0) 
                        continue;

                    AddPointToHull(pointIndex, points, hull, conflictingFaces, conflictGraph, tetrahedra);
                    usedPoints.Add(pointIndex);
                }

                Profiler.Log.WriteLine<Profiler.TtGraphicsGategory>(Profiler.ELogTag.Info, $"Successfully built {tetrahedra.Count} tetrahedra from {points.Count} points");
            }
            catch (Exception e)
            {
                Profiler.Log.WriteException(e);
            }
            finally
            {
                conflictGraph.Clear();
            }

            return tetrahedra;
        }

        #endregion

        #region Helper Methods

        private void BuildInitialConflictGraph(List<Vector3> points, List<Face> hull,
                                             HashSet<int> usedPoints, ConflictGraph conflictGraph)
        {
            for (int i = 0; i < points.Count; i++)
            {
                if (usedPoints.Contains(i)) 
                    continue;

                foreach (var face in hull)
                {
                    if (IsPointAboveFace(face, points[i], points))
                    {
                        conflictGraph.AddConflict(i, face);
                    }
                }
            }
        }

        private void AddPointToHull(int pointIndex, List<Vector3> points, List<Face> hull,
                          HashSet<Face> conflictingFaces, ConflictGraph conflictGraph,
                          List<TetrahedronData> tetrahedra)
        {
            var point = points[pointIndex];

            // 1. 找到边界边
            var boundaryEdges = FindBoundaryEdges(conflictingFaces.ToList());
            if (boundaryEdges.Count == 0)
            {
                return;
            }

            // 2. 在删除冲突面之前，先为每条边界边找到第三个顶点
            var edgeToThirdVertex = new Dictionary<Edge, int>();
            foreach (var edge in boundaryEdges)
            {
                var thirdVertex = FindThirdVertexOfEdge(edge, conflictingFaces, points);
                if (thirdVertex != -1)
                {
                    edgeToThirdVertex[edge] = thirdVertex;
                }
            }

            // 3. 收集需要重新分配的点
            var orphanedPoints = new HashSet<int>();
            foreach (var face in conflictingFaces)
            {
                var conflictingPoints = conflictGraph.GetConflictingPoints(face);
                foreach (var p in conflictingPoints)
                {
                    if (p != pointIndex)
                        orphanedPoints.Add(p);
                }
            }

            // 4. 现在可以安全地移除冲突面
            foreach (var face in conflictingFaces.ToList())
            {
                hull.Remove(face);
                conflictGraph.RemoveFace(face);
            }

            // 5. 为每条边界边创建新面和四面体
            var newFaces = new List<Face>();
            foreach (var edge in boundaryEdges)
            {
                var newFace = new Face(edge.v1, edge.v2, pointIndex);
                newFace.ComputeNormalAndCenter(points);

                hull.Add(newFace);
                newFaces.Add(newFace);

                // 使用之前保存的第三个顶点信息创建四面体
                if (edgeToThirdVertex.TryGetValue(edge, out int thirdVertex))
                {
                    var tetra = CreateTetrahedron(edge.v1, edge.v2, thirdVertex, pointIndex, points);
                    if (tetra != null && tetra.IsValid())
                    {
                        tetrahedra.Add(tetra);
                    }
                }
            }

            // 6. 重新分配孤立的点
            foreach (var orphanPoint in orphanedPoints)
            {
                foreach (var newFace in newFaces)
                {
                    if (IsPointAboveFace(newFace, points[orphanPoint], points))
                    {
                        conflictGraph.AddConflict(orphanPoint, newFace);
                    }
                }
            }
        }


        private TetrahedronData CreateTetrahedron(int v1, int v2, int v3, int v4, List<Vector3> points)
        {
            var tetra = new TetrahedronData
            {
                ProbeIndices = new[] { v1, v2, v3, v4 },
                Vertices = new[] { points[v1], points[v2], points[v3], points[v4] }
            };

            tetra.Precompute();
            return tetra;
        }

        private List<Face> GetTetrahedronFaces(TetrahedronData tetrahedron)
        {
            var faces = new List<Face>();
            var indices = tetrahedron.ProbeIndices;

            // 四面体有4个面，注意顶点顺序确保法向量朝外
            faces.Add(new Face(indices[0], indices[1], indices[2])); // 面 ABC
            faces.Add(new Face(indices[0], indices[1], indices[3])); // 面 ABD  
            faces.Add(new Face(indices[0], indices[2], indices[3])); // 面 ACD
            faces.Add(new Face(indices[1], indices[2], indices[3])); // 面 BCD

            return faces;
        }

        private List<Edge> FindBoundaryEdges(List<Face> faces)
        {
            var edgeCount = new Dictionary<Edge, int>();

            // 统计每条边的出现次数
            foreach (var face in faces)
            {
                var edges = new[]
                {
                    new Edge(face.Vertices[0], face.Vertices[1]),
                    new Edge(face.Vertices[1], face.Vertices[2]),
                    new Edge(face.Vertices[2], face.Vertices[0])
                };

                foreach (var edge in edges)
                {
                    edgeCount[edge] = edgeCount.TryGetValue(edge, out var count) ? count + 1 : 1;
                }
            }

            // 边界边只出现一次
            return edgeCount.Where(kvp => kvp.Value == 1).Select(kvp => kvp.Key).ToList();
        }

        private bool IsPointAboveFace(Face face, Vector3 point, List<Vector3> allPoints)
        {
            if (face.Normal.Length() < 1e-6f) 
                return false;

            var faceCenter = face.Center;
            var toPoint = point - faceCenter;
            return Vector3.Dot(face.Normal, toPoint) > 1e-6f;
        }

        private int FindThirdVertexOfEdge(Edge edge, HashSet<Face> faces, List<Vector3> points)
        {
            foreach (var face in faces)
            {
                var vertices = face.Vertices;
                bool hasV1 = vertices.Contains(edge.v1);
                bool hasV2 = vertices.Contains(edge.v2);

                if (hasV1 && hasV2)
                {
                    // 找到包含这条边的面，返回第三个顶点
                    foreach (var vertex in vertices)
                    {
                        if (vertex != edge.v1 && vertex != edge.v2)
                            return vertex;
                    }
                }
            }
            return -1;
        }

        private TetrahedronData FindInitialTetrahedron(List<Vector3> points)
        {
            // 找到体积最大的初始四面体，限制搜索范围提高性能
            float maxVolume = 0f;
            TetrahedronData bestTetra = null;
            int searchLimit = Math.Min(points.Count, 50); // 限制搜索范围

            for (int i = 0; i < searchLimit - 3; i++)
            {
                for (int j = i + 1; j < searchLimit - 2; j++)
                {
                    for (int k = j + 1; k < searchLimit - 1; k++)
                    {
                        for (int l = k + 1; l < searchLimit; l++)
                        {
                            var tetra = new TetrahedronData
                            {
                                ProbeIndices = new[] { i, j, k, l },
                                Vertices = new[] { points[i], points[j], points[k], points[l] }
                            };

                            tetra.Precompute();

                            if (tetra.IsValid() && tetra.Volume > maxVolume)
                            {
                                maxVolume = tetra.Volume;
                                bestTetra = tetra;
                            }
                        }
                    }
                }
            }

            return bestTetra;
        }

        #endregion

        #region Public Utility Methods

        public static int FindBestTetrahedron(Vector3 queryPoint, List<TetrahedronData> tetrahedra)
        {
            // 首先查找包含该点的四面体
            for (int i = 0; i < tetrahedra.Count; i++)
            {
                if (tetrahedra[i].ContainsPoint(queryPoint))
                {
                    return i;
                }
            }

            // 如果没找到包含的，返回最近的
            float minDistance = float.MaxValue;
            int bestIndex = -1;

            for (int i = 0; i < tetrahedra.Count; i++)
            {
                float outSqrDistance;
                BoundingBox.CalculateClosestPointInBox(queryPoint, tetrahedra[i].Bounds, out Vector3 closestPoint, out outSqrDistance);
                float distance = outSqrDistance;
                if (distance < minDistance)
                {
                    minDistance = distance;
                    bestIndex = i;
                }
            }

            return bestIndex;
        }

        /// <summary>
        /// 使用四面体重心坐标插值球谐系数
        /// </summary>
        /// <param name="queryPoint">查询点的世界坐标</param>
        /// <param name="tetrahedronIndex">四面体索引</param>
        /// <param name="tetrahedra">四面体列表</param>
        /// <param name="probeSHCoefficients">每个probe的球谐系数（每个probe有9个SH系数用于RGB）</param>
        /// <returns>插值后的球谐系数</returns>
        //public static Vector4[] InterpolateSH(Vector3 queryPoint, int tetrahedronIndex,
        //                                    List<TetrahedronData> tetrahedra,
        //                                    List<Vector4[]> probeSHCoefficients)
        //{
        //    if (tetrahedronIndex < 0 || tetrahedronIndex >= tetrahedra.Count)
        //        return CreateZeroSH();

        //    var tetrahedron = tetrahedra[tetrahedronIndex];
        //    var barycentricCoords = tetrahedron.GetBarycentricCoords(queryPoint);

        //    // 确保重心坐标有效
        //    if (!IsValidBarycentricCoords(barycentricCoords))
        //    {
        //        // 如果重心坐标无效，使用最近点插值
        //        barycentricCoords = ClampBarycentricCoords(barycentricCoords);
        //    }

        //    // 获取四个probe的SH系数
        //    var sh0 = probeSHCoefficients[tetrahedron.ProbeIndices[0]];
        //    var sh1 = probeSHCoefficients[tetrahedron.ProbeIndices[1]];
        //    var sh2 = probeSHCoefficients[tetrahedron.ProbeIndices[2]];
        //    var sh3 = probeSHCoefficients[tetrahedron.ProbeIndices[3]];

        //    // 插值每个SH系数
        //    var interpolatedSH = new Vector4[9]; // L0(1) + L1(3) + L2(5) = 9个系数

        //    for (int i = 0; i < 9; i++)
        //    {
        //        interpolatedSH[i] = sh0[i] * barycentricCoords.x +
        //                           sh1[i] * barycentricCoords.y +
        //                           sh2[i] * barycentricCoords.z +
        //                           sh3[i] * barycentricCoords.w;
        //    }

        //    return interpolatedSH;
        //}
        #endregion
    }
}