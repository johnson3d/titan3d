using EngineNS.Bricks.Animation.AnimNode;
using EngineNS.Bricks.Animation.Utility;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.Animation.Utility
{
    public class BlendSpaceGrid
    {
        /**
	 * Find Triangle this TestPoint is within
	 * 
	 * @param	TestPoint				Point to test
	 * @param	OutBaryCentricCoords	Output BaryCentricCoords2D of the point in the triangle // for now it's only 2D
	 * @param	OutTriangle				The triangle that this point is within or lie
	 * @param	TriangleList			TriangleList to test
	 * 
	 * @return	true if successfully found the triangle this point is within
	 */
        public bool FindTriangleThisPointBelongsTo(Vector3 TestPoint, ref Vector3 OutBaryCentricCoords, ref BSTriangle OutTriangle, List<BSTriangle> TriangleList)
        {
            // Calculate distance from point to triangle and sort the triangle list accordingly
            List<BSSortByDistance> SortedTriangles = new List<BSSortByDistance>();
            for (int i = 0; i < TriangleList.Count; ++i)
            {
                SortedTriangles.Add(new BSSortByDistance(-1, 0));
            }
            for (int TriangleIndex = 0; TriangleIndex < TriangleList.Count; ++TriangleIndex)
            {
                SortedTriangles[TriangleIndex].Index = TriangleIndex;
                SortedTriangles[TriangleIndex].Distance = TriangleList[TriangleIndex].GetDistance(TestPoint);
            }
            SortedTriangles.Sort((A, B) =>
            {
                if (A.Distance == B.Distance)
                    return 0;
                return A.Distance < B.Distance ? 1 : -1;
            });

            // Now loop over the sorted triangles and test the barycentric coordinates with the point
            for (int i = 0; i < SortedTriangles.Count; ++i)
            {
                BSTriangle Triangle = TriangleList[SortedTriangles[i].Index];

                Vector3 Coords = GetBaryCentric2D(TestPoint, Triangle.Vertices[0].Position, Triangle.Vertices[1].Position, Triangle.Vertices[2].Position);

                // Z coords often has precision error because it's derived from 1-A-B, so do more precise check
                if (Math.Abs(Coords.Z) < 0.00001f)
                {
                    Coords.Z = 0.0f;
                }

                // Is the point inside of the triangle, or on it's edge (Z coordinate should always match since the blend samples are set in 2D)
                if (0.0f <= Coords.X && Coords.X <= 1.0 && 0.0f <= Coords.Y && Coords.Y <= 1.0 && 0.0f <= Coords.Z && Coords.Z <= 1.0)
                {
                    OutBaryCentricCoords = Coords;
                    OutTriangle = Triangle;
                    return true;
                }
            }

            return false;
        }
        private Vector3 GetBaryCentric2D(Vector3 Point, Vector3 A, Vector3 B, Vector3 C)
        {
            float a = ((B.Y - C.Y) * (Point.X - C.X) + (C.X - B.X) * (Point.Y - C.Y)) / ((B.Y - C.Y) * (A.X - C.X) + (C.X - B.X) * (A.Y - C.Y));
            float b = ((C.Y - A.Y) * (Point.X - C.X) + (A.X - C.X) * (Point.Y - C.Y)) / ((B.Y - C.Y) * (A.X - C.X) + (C.X - B.X) * (A.Y - C.Y));

            return new Vector3(a, b, 1.0f - a - b);
        }
        private Vector3 ClosestPointOnLine(Vector3 LineStart, Vector3 LineEnd, Vector3 Point)
        {
            // Solve to find alpha along line that is closest point
            // Weisstein, Eric W. "Point-Line Distance--3-Dimensional." From MathWorld--A Switchram Web Resource. http://mathworld.wolfram.com/Point-LineDistance3-Dimensional.html 
            var sp = LineStart - Point;
            var es = LineEnd - LineStart;
            float A = sp.X * es.X + sp.Y * es.Y + sp.Z * es.Z;
            float B = (LineEnd - LineStart).LengthSquared();
            // This should be robust to B == 0 (resulting in NaN) because clamp should return 1.
            float T = MathHelper.Clamp(-A / B, 0.0f, 1.0f);

            // Generate closest point
            Vector3 ClosestPoint = LineStart + (T * (LineEnd - LineStart));

            return ClosestPoint;
        }
        /**  
         * Fill up Grid GridPoints using TriangleList input - Grid information should have been set by SetGridInfo
         * 
         * @param	SamplePoints		: Sample Point List
         * @param	TriangleList		: List of triangles
         */
        public void GenerateGridElements(List<BSPoint> SamplePoints, List<BSTriangle> TriangleList)
        {
            if (!(NumGridDivisions.X > 0 && NumGridDivisions.Y > 0))
                return;
            //if(!(GridDimensions.IsValid))

            int TotalNumGridPoints = (int)(NumGridPointsForAxis.X * NumGridPointsForAxis.Y);

            GridPoints.Clear();


            if (SamplePoints.Count == 0 || TriangleList.Count == 0)
            {
                return;
            }

            for (int i = 0; i < TotalNumGridPoints; ++i)
            {
                GridPoints.Add(new GridElement());
            }
            Vector3 GridPointPosition;
            for (int GridPositionX = 0; GridPositionX < NumGridPointsForAxis.X; ++GridPositionX)
            {
                for (int GridPositionY = 0; GridPositionY < NumGridPointsForAxis.Y; ++GridPositionY)
                {
                    BSTriangle SelectedTriangle = null;
                    GridElement GridPoint = GridPoints[GridPositionX * (int)NumGridPointsForAxis.Y + GridPositionY];

                    GridPointPosition = GetPosFromIndex(GridPositionX, GridPositionY);

                    Vector3 Weights = Vector3.Zero;
                    if (FindTriangleThisPointBelongsTo(GridPointPosition, ref Weights, ref SelectedTriangle, TriangleList))
                    {
                        // found it
                        GridPoint.Weights[0] = Weights.X;
                        GridPoint.Weights[1] = Weights.Y;
                        GridPoint.Weights[2] = Weights.Z;
                        // need to find sample point index
                        // @todo fix this with better solution
                        // lazy me
                        GridPoint.Indices[0] = SamplePoints.FindIndex((point) => { return point == SelectedTriangle.Vertices[0]; });
                        GridPoint.Indices[1] = SamplePoints.FindIndex((point) => { return point == SelectedTriangle.Vertices[1]; });
                        GridPoint.Indices[2] = SamplePoints.FindIndex((point) => { return point == SelectedTriangle.Vertices[2]; });

                        //check(GridPoint.Indices[0] != INDEX_NONE);
                        //check(GridPoint.Indices[1] != INDEX_NONE);
                        //check(GridPoint.Indices[2] != INDEX_NONE);
                    }
                    else
                    {
                        List<BSSortByDistance> SortedTriangles = new List<BSSortByDistance>();
                        for (int TriangleIndex = 0; TriangleIndex < TriangleList.Count; ++TriangleIndex)
                        {
                            // Check if points are collinear
                            BSTriangle Triangle = TriangleList[TriangleIndex];
                            Vector3 EdgeA = Triangle.Vertices[1].Position - Triangle.Vertices[0].Position;
                            Vector3 EdgeB = Triangle.Vertices[2].Position - Triangle.Vertices[0].Position;
                            float Result = EdgeA.X * EdgeB.Y - EdgeA.Y * EdgeB.X;
                            // Only add valid triangles
                            if (Result > 0.0f)
                            {
                                SortedTriangles.Add(new BSSortByDistance(TriangleIndex, Triangle.GetDistance(GridPointPosition)));
                            }
                        }

                        if (SortedTriangles.Count > 0)
                        {
                            // SortedTriangles.Sort([](FSortByDistance A, FSortByDistance B) { return A.Distance < B.Distance; });
                            BSTriangle ClosestTriangle = TriangleList[SortedTriangles[0].Index];

                            // For the closest triangle, determine which of its edges is closest to the grid point
                            List<BSSortByDistance> Edges = new List<BSSortByDistance>();
                            List<Vector3> PointsOnEdges = new List<Vector3>();
                            for (int EdgeIndex = 0; EdgeIndex < 3; ++EdgeIndex)
                            {
                                Vector3 ClosestPoint = ClosestPointOnLine(ClosestTriangle.Edges[EdgeIndex].Vertices[0].Position, ClosestTriangle.Edges[EdgeIndex].Vertices[1].Position, GridPointPosition);
                                Edges.Add(new BSSortByDistance(EdgeIndex, (ClosestPoint - GridPointPosition).LengthSquared()));
                                PointsOnEdges.Add(ClosestPoint);
                            }
                            //Edges.Sort([](FSortByDistance A, FSortByDistance B) { return A.Distance < B.Distance; });

                            // Calculate weighting using the closest edge points and the clamped grid position on the line
                            Vector3 GridWeights = GetBaryCentric2D(PointsOnEdges[Edges[0].Index], ClosestTriangle.Vertices[0].Position, ClosestTriangle.Vertices[1].Position, ClosestTriangle.Vertices[2].Position);

                            for (int Index = 0; Index < 3; ++Index)
                            {
                                GridPoint.Weights[Index] = GridWeights[Index];
                                GridPoint.Indices[Index] = SamplePoints.FindIndex((point) => { return point == ClosestTriangle.Vertices[Index]; });
                            }
                        }
                        else
                        {
                            // This means that there is either one point, two points or collinear triangles on the grid
                            if (SamplePoints.Count == 1)
                            {
                                // Just one, fill all grid points to the single sample
                                GridPoint.Weights[0] = 1.0f;
                                GridPoint.Indices[0] = 0;
                            }
                            else
                            {
                                // Two points or co-linear triangles, first find the two closest samples
                                List<BSSortByDistance> SampleDistances = new List<BSSortByDistance>();
                                for (int PointIndex = 0; PointIndex < SamplePoints.Count; ++PointIndex)
                                {
                                    var vec = (SamplePoints[PointIndex].Position - GridPointPosition);
                                    Vector2 vector2 = new Vector2(vec.X, vec.Y);
                                    float DistanceFromSampleToPoint = vector2.LengthSquared();
                                    SampleDistances.Add(new BSSortByDistance(PointIndex, DistanceFromSampleToPoint));
                                }
                                SampleDistances.Sort((A, B) =>
                                {
                                    if (A.Distance == B.Distance)
                                        return 0;
                                    return A.Distance > B.Distance ? 1 : -1;
                                });

                                // Find closest point on line between the two samples (clamping the grid position to the line, just like clamping to the triangle edges)
                                BSPoint[] Samples = new BSPoint[2];
                                Samples[0] = SamplePoints[SampleDistances[0].Index];
                                Samples[1] = SamplePoints[SampleDistances[1].Index];
                                Vector3 ClosestPointOnTheLine = ClosestPointOnLine(Samples[0].Position, Samples[1].Position, GridPointPosition);

                                var temp = (Samples[0].Position - Samples[1].Position);
                                Vector2 tempVector2 = new Vector2(temp.X, temp.Y);
                                float LineLength = tempVector2.LengthSquared();

                                // Weight the samples according to the distance from the grid point on the line to the samples
                                for (int SampleIndex = 0; SampleIndex < 2; ++SampleIndex)
                                {
                                    var thelength3D = (Samples[SampleIndex].Position - ClosestPointOnTheLine);
                                    var thelength2D = new Vector2(thelength3D.X, thelength3D.Y);
                                    GridPoint.Weights[SampleIndex] = (LineLength - thelength2D.LengthSquared()) / LineLength;
                                    GridPoint.Indices[SampleIndex] = SamplePoints.FindIndex((point) => { return point == Samples[SampleIndex]; });
                                }
                            }
                        }
                    }
                }
            }
        }

        /** 
         * default value 
         */
        public BlendSpaceGrid()
        {
            GridDimensions = new BSBox();
            GridDimensions.Min = new Vector3(0, 0, 0);
            GridDimensions.Max = new Vector3(100, 100, 0);
            NumGridPointsForAxis = new Vector2(5, 5);
        }

        public void Reset()
        {
            GridPoints.Clear();
        }

        public void SetGridInfo(BlendAxis BlendParamX, BlendAxis BlendParamY)
        {
            NumGridPointsForAxis.X = (int)BlendParamX.GridNum + 1;
            NumGridPointsForAxis.Y = (int)BlendParamY.GridNum + 1;

            NumGridDivisions.X = BlendParamX.GridNum;
            NumGridDivisions.Y = BlendParamY.GridNum;

            GridDimensions.Min.X = BlendParamX.Min;
            GridDimensions.Max.X = BlendParamX.Max;

            GridDimensions.Min.Y = BlendParamY.Min;
            GridDimensions.Max.Y = BlendParamY.Max;
        }

        public GridElement GetElement(int GridX, int GridY)
        {
            //check(NumGridPointsForAxis.X >= GridX);
            //check(NumGridPointsForAxis.Y >= GridY);

            //check(GridPoints.Num() > 0);
            return GridPoints[GridX * (int)NumGridPointsForAxis.Y + GridY];
        }
        public List<GridElement> GetElements() { return GridPoints; }

        /** 
         * Convert grid index (GridX, GridY) to triangle coords and returns Vector3
*/
        public Vector3 GetPosFromIndex(int GridX, int GridY)
        {
            Vector2 CoordDim = new Vector2(GridDimensions.GetSize().X, GridDimensions.GetSize().Y);
            Vector2 EachGridSize = Vector2.Zero;
            EachGridSize.X = CoordDim.X / NumGridDivisions.X;
            EachGridSize.Y = CoordDim.Y / NumGridDivisions.Y;

            // for now only 2D
            return new Vector3(GridX * EachGridSize.X + GridDimensions.Min.X, GridY * EachGridSize.Y + GridDimensions.Min.Y, 0.0f);
        }
        BSBox GridDimensions;

        // how many rows/cols for each axis
        Vector2 NumGridPointsForAxis;
        Vector2 NumGridDivisions;

        // Each point data -output data
        List<GridElement> GridPoints = new List<GridElement>(); // 2D array saved in 1D array, to search (x, y), x*GridSizeX+y;
    }
}
