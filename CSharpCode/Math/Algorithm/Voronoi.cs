using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Algorithm
{
    public class Voronoi
    {
        public struct Edge
        {
            public Vector2 Point1;
            public Vector2 Point2;
            public Edge(in Vector2 p1, in Vector2 p2)
            {
                Point1 = p1;
                Point2 = p2;
            }

            public override bool Equals(object obj)
            {
                if (obj == null) return false;
                if (obj.GetType() != GetType()) return false;
                var edge = (Edge)obj;
                return Equals(edge);
            }
            public bool Equals(in Edge obj)
            {
                return (((Point1 == obj.Point1) && (Point2 == obj.Point2)) ||
                        ((Point1 == obj.Point2) && (Point2 == obj.Point1)));
            }
            public override int GetHashCode()
            {
                return (Point1.ToString() + Point2.ToString()).GetHashCode();
            }
        }

        public static bool Calculate(Vector2[] points, ref HashSet<Edge> edges)
        {
            List<DelaunayTriangle2D.Triangle> triangles = new List<DelaunayTriangle2D.Triangle>();
            Dictionary<int, List<int>> trianglesWithPoint = new Dictionary<int, List<int>>();
            if (!DelaunayTriangle2D.Calculate(points, ref triangles, ref trianglesWithPoint))
                return false;

            for(int i=0; i<triangles.Count; i++)
            {
                GetIndexSameEdgeTriangle(triangles[i].VertexAIndex, i, triangles, trianglesWithPoint, ref edges);
                GetIndexSameEdgeTriangle(triangles[i].VertexBIndex, i, triangles, trianglesWithPoint, ref edges);
                GetIndexSameEdgeTriangle(triangles[i].VertexCIndex, i, triangles, trianglesWithPoint, ref edges);
            }

            return true;
        }
        static void GetIndexSameEdgeTriangle(int vertexIndex, int oriTriangle, List<DelaunayTriangle2D.Triangle> triangles, Dictionary<int, List<int>> trianglesWithPoint, ref HashSet<Edge> edges)
        {
            List<int> pointTriangles;
            if (trianglesWithPoint.TryGetValue(vertexIndex, out pointTriangles))
            {
                for(int triangleIdx = 0; triangleIdx < pointTriangles.Count; triangleIdx++)
                {
                    var tIdx = pointTriangles[triangleIdx];
                    if (tIdx == oriTriangle)
                        continue;
                    if(triangles[tIdx].HasSameEdge(triangles[oriTriangle]))
                    {
                        var edge = new Edge(triangles[tIdx].Circumcenter, triangles[oriTriangle].Circumcenter);
                        edges.Add(edge);
                    }
                }
            }
        }
    }
}
