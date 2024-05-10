using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Animation.Asset.BlendSpace
{
    enum ECircumCircleState
    {
        Outside = -1,
        On = 0,
        Inside = 1,
    };
    public struct FBSIndexPoint
    {
        public TtBSPoint Point;
        public int OriginalIndex;
        public FBSIndexPoint(TtBSPoint P, int InOriginalIndex)
        {
            Point = P;
            OriginalIndex = InOriginalIndex;
        }
    };
    public class TtBSSortByDistance
    {
        public int Index = 0;
        public float Distance = 0;
        public TtBSSortByDistance(int index, float distance)
        {
            Index = index;
            Distance = distance;
        }
    };
    public struct FBSBox
    {
        /** Holds the box's minimum point. */
        public Vector3 Min;

        /** Holds the box's maximum point. */
        public Vector3 Max;

        /** Holds a flag indicating whether this box is valid. */
        public int IsValid;
        public Vector3 GetSize()
        {
            return (Max - Min);
        }
    }


    public class TtBSPoint
    {
        // position of Point
        public Vector3 Position;
        // Triangles this point belongs to
        public List<TtBSTriangle> Triangles;

        public TtBSPoint(Vector3 Pos)
        {
            Position = Pos;
            Triangles = new List<TtBSTriangle>();
        }
        public bool Equals(TtBSPoint right)
        {
            if (right == null)
                return false;
            return (Position == right.Position);
        }
        //public static bool operator ==(BSPoint left, BSPoint right)
        //{
        //    // if same position, it's same point
        //    return (left.Position == right?.Position);
        //}
        //public static bool operator !=(BSPoint left, BSPoint right)
        //{
        //    // if same position, it's same point
        //    return (left.Position != right.Position);
        //}

        public void AddTriangle(TtBSTriangle NewTriangle)
        {
            // Triangles.AddUnique(NewTriangle);
        }

        public void RemoveTriangle(TtBSTriangle TriangleToRemove)
        {
            //Triangles.Remove(TriangleToRemove);
        }

        public float GetDistance(TtBSPoint Other)
        {
            return (Other.Position - Position).Length();
        }
    };

    public class TtBSHalfEdge
    {
        // 3 vertices in CCW order
        public TtBSPoint[] Vertices = new TtBSPoint[2];

        public TtBSHalfEdge() { }
        public TtBSHalfEdge(TtBSPoint A, TtBSPoint B)
        {
            Vertices[0] = A;
            Vertices[1] = B;
        }

        public bool DoesShare(TtBSHalfEdge A)
        {
            return (Vertices[0] == A.Vertices[1] && Vertices[1] == A.Vertices[0]);
        }
        public bool Equals(TtBSHalfEdge right)
        {
            if (right == null)
                return false;
            return (Vertices[0] == right.Vertices[0] && Vertices[1] == right.Vertices[1]);
        }
        //public static bool operator ==(BSHalfEdge left, BSHalfEdge right)
        //{
        //    return (left.Vertices[0] == right.Vertices[0] && left.Vertices[1] == right.Vertices[1]);

        //}
        //public static bool operator !=(BSHalfEdge left, BSHalfEdge right)
        //{
        //    return !(left.Vertices[0] == right.Vertices[0] && left.Vertices[1] == right.Vertices[1]);
        //}
    };

    public class TtBSTriangle
    {
        // 3 vertices in CCW order
        public TtBSPoint[] Vertices = new TtBSPoint[3];
        // average points for Vertices
        public Vector3 Center;
        // FEdges
        public TtBSHalfEdge[] Edges = new TtBSHalfEdge[3];
        public bool Equals(TtBSHalfEdge right)
        {
            if (right == null)
                return false;
            return (Vertices[0] == right.Vertices[0] && Vertices[1] == right.Vertices[1] && Vertices[2] == right.Vertices[2]);
        }

        //public static bool operator ==(BSTriangle left, BSTriangle right)
        //{
        //    return (left.Vertices[0] == right.Vertices[0] && left.Vertices[1] == right.Vertices[1] && left.Vertices[2] == right.Vertices[2]);

        //}
        //public static bool operator !=(BSTriangle left, BSTriangle right)
        //{
        //    return !(left.Vertices[0] == right.Vertices[0] && left.Vertices[1] == right.Vertices[1] && left.Vertices[2] == right.Vertices[2]);
        //}

        //public BSTriangle(BSTriangle Copy)
        //{
        //    //FMemory::Memcpy(Vertices, Copy.Vertices);
        //    //FMemory::Memcpy(Edges, Copy.Edges);
        //    //Center = Copy.Center;

        //    //Vertices[0]->AddTriangle(this);
        //    //Vertices[1]->AddTriangle(this);
        //    //Vertices[2]->AddTriangle(this);
        //}

        public TtBSTriangle(TtBSPoint A, TtBSPoint B, TtBSPoint C)
        {
            Vertices[0] = A;
            Vertices[1] = B;
            Vertices[2] = C;
            Center = (A.Position + B.Position + C.Position) / 3.0f;

            Vertices[0].AddTriangle(this);
            Vertices[1].AddTriangle(this);
            Vertices[2].AddTriangle(this);
            // when you make triangle first time, make sure it stays in CCW
            MakeCCW();

            // now create edges, this should be in the CCW order
            Edges[0] = new TtBSHalfEdge(Vertices[0], Vertices[1]);
            Edges[1] = new TtBSHalfEdge(Vertices[1], Vertices[2]);
            Edges[2] = new TtBSHalfEdge(Vertices[2], Vertices[0]);
        }

        public TtBSTriangle(TtBSPoint A)
        {
            Vertices[0] = A;
            Vertices[1] = A;
            Vertices[2] = A;
            Center = A.Position;

            Vertices[0].AddTriangle(this);
            Vertices[1].AddTriangle(this);
            Vertices[2].AddTriangle(this);

            // now create edges, this should be in the CCW order
            Edges[0] = new TtBSHalfEdge(Vertices[0], Vertices[1]);
            Edges[1] = new TtBSHalfEdge(Vertices[1], Vertices[2]);
            Edges[2] = new TtBSHalfEdge(Vertices[2], Vertices[0]);
        }

        public TtBSTriangle(TtBSPoint A, TtBSPoint B)
        {
            Vertices[0] = A;
            Vertices[1] = B;
            Vertices[2] = B;
            Center = (A.Position + B.Position) / 2.0f;

            Vertices[0].AddTriangle(this);
            Vertices[1].AddTriangle(this);
            Vertices[2].AddTriangle(this);

            // now create edges, this should be in the CCW order
            Edges[0] = new TtBSHalfEdge(Vertices[0], Vertices[1]);
            Edges[1] = new TtBSHalfEdge(Vertices[1], Vertices[2]);
            Edges[2] = new TtBSHalfEdge(Vertices[2], Vertices[0]);
        }

        public TtBSTriangle()
        {
            Vertices[0] = null;
            Vertices[1] = null;
            Vertices[2] = null;
        }

        ~TtBSTriangle()
        {
            for (int VertexIndex = 0; VertexIndex < 3; ++VertexIndex)
            {
                if (Vertices[VertexIndex] != null)
                {
                    Vertices[VertexIndex].RemoveTriangle(this);
                }
            }
        }

        bool Contains(TtBSPoint Other)
        {
            return (Other == Vertices[0] || Other == Vertices[1] || Other == Vertices[2]);
        }

        public float GetDistance(TtBSPoint Other)
        {
            return (Other.Position - Center).Length();
        }

        public float GetDistance(Vector3 Other)
        {
            return (Other - Center).Length();
        }

        public bool HasSameHalfEdge(TtBSTriangle Other)
        {
            for (int I = 0; I < 3; ++I)
            {
                for (int J = 0; J < 3; ++J)
                {
                    if (Other.Edges[I].Equals(Edges[J]))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public bool DoesShareSameEdge(TtBSTriangle Other)
        {
            for (int I = 0; I < 3; ++I)
            {
                for (int J = 0; J < 3; ++J)
                {
                    if (Other.Edges[I].DoesShare(Edges[J]))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        // find point that doesn't share with this
        // this should only get called if it shares same edge
        public TtBSPoint FindNonSharingPoint(TtBSTriangle Other)
        {
            if (!Contains(Other.Vertices[0]))
            {
                return Other.Vertices[0];
            }

            if (!Contains(Other.Vertices[1]))
            {
                return Other.Vertices[1];
            }

            if (!Contains(Other.Vertices[2]))
            {
                return Other.Vertices[2];
            }

            return null;
        }


        private void MakeCCW()
        {
            // this eventually has to happen on the plane that contains this 3 points
            // for now we ignore Z
            Vector3 Diff1 = Vertices[1].Position - Vertices[0].Position;
            Vector3 Diff2 = Vertices[2].Position - Vertices[0].Position;

            float Result = Diff1.X * Diff2.Y - Diff1.Y * Diff2.X;

            //check(Result != 0.0f);

            // it's in left side, we need this to be right side
            if (Result < 0.0f)
            {
                // swap 1&2 
                TtBSPoint TempPt = Vertices[2];
                Vertices[2] = Vertices[1];
                Vertices[1] = TempPt;
            }
        }
    }
    public class TtDelaunayTriangleMaker
    {
        public void Reset()
        {
            ClearTriangles();
            ClearSamples();
            Sorted2Indices.Clear();
        }
        public void ClearTriangles()
        {
            TriangleList.Clear();
        }
        public void ClearSamples()
        {
            SamplesList.Clear();
        }
        public void Triangulate()
        {
            if (SamplesList.Count == 0)
            {
                return;
            }
            else if (SamplesList.Count == 1)
            {
                // degenerate case 1
                TtBSTriangle triangle = new TtBSTriangle(SamplesList[0]);
                AddTriangle(triangle);
            }
            else if (SamplesList.Count == 2)
            {
                // degenerate case 2
                TtBSTriangle triangle = new TtBSTriangle(SamplesList[0], SamplesList[1]);
                AddTriangle(triangle);
            }
            else
            {
                Sort();
                // first choose first 3 points
                for (int i = 2; i < SamplesList.Count; ++i)
                {
                    GenerateTriangles(SamplesList, i + 1);
                }
                if (TriangleList.Count == 0)
                {
                    if (AllCoincident(SamplesList))
                    {
                        // coincident case - just create one triangle
                        TtBSTriangle triangle = new TtBSTriangle(SamplesList[0]);
                        AddTriangle(triangle);
                    }
                    else
                    {
                        // collinear case: create degenerate triangles between pairs of points
                        for (int pointIndex = 0; pointIndex < SamplesList.Count - 1; ++pointIndex)
                        {
                            TtBSTriangle triangle = new TtBSTriangle(SamplesList[pointIndex], SamplesList[pointIndex + 1]);
                            AddTriangle(triangle);
                        }
                    }
                }
            }
        }

        public void AddSample(Vector3 position, int index)
        {
            if(SamplesList.Count == 0)
            {
                SamplesList.Add(new TtBSPoint(position));
                Sorted2Indices.Add(index);
            }
            if (SamplesList.Find((a) => 
            {
                return a.Position == position;
            }) == null)
            {
                SamplesList.Add(new TtBSPoint(position));
                Sorted2Indices.Add(index);
            }
        }

        public void Step(int StartIndex)
        {

        }
        void Sort()
        {
            List<FBSIndexPoint> sortedPoints = new List<FBSIndexPoint>();
            for (int i = 0; i < SamplesList.Count; ++i)
            {
                sortedPoints.Add(new FBSIndexPoint(SamplesList[i], Sorted2Indices[i]));
            }
            sortedPoints.Sort((A, B) =>
            {
                if (A.Point.Position.X == B.Point.Position.X) // same, then compare Y
                {
                    if (A.Point.Position.Y == B.Point.Position.Y)
                    {
                        return A.Point.Position.Z < B.Point.Position.Z ? 1 : -1;
                    }
                    else
                    {
                        return A.Point.Position.Y < B.Point.Position.Y ? 1 : -1;
                    }
                }

                return A.Point.Position.X < B.Point.Position.X ? 1 : -1;
            });

            Sorted2Indices.Clear();
            for (int i = 0; i < SamplesList.Count; ++i)
            {
                Sorted2Indices.Add(-1);
            }
            for (int sampleIndex = 0; sampleIndex < SamplesList.Count; ++sampleIndex)
            {
                SamplesList[sampleIndex] = sortedPoints[sampleIndex].Point;
                Sorted2Indices[sampleIndex] = sortedPoints[sampleIndex].OriginalIndex;
            }

        }

        ~TtDelaunayTriangleMaker()
        {

        }

        /** 
         * Get TriangleList
         */
        public List<TtBSTriangle> GetTriangleList() { return TriangleList; }

        public void EditPointValue(int SamplePointIndex, Vector3 NewValue)
        {
            SamplesList[SamplePointIndex].Position = NewValue;
        }

        /** Original index - before sorted to match original data **/
        public int GetOriginalIndex(int NewSortedSamplePointList)
        {
            return Sorted2Indices[NewSortedSamplePointList];
        }

        public List<int> GetIndiceMapping() { return Sorted2Indices; }

        /* Set the grid box, so we can normalize the sample points */
        public void SetGridBox(TtBlendSpace_Axis BlendParamX, TtBlendSpace_Axis BlendParamY)
        {
            FBSBox GridBox = new FBSBox();
            GridBox.Min.X = BlendParamX.Min;
            GridBox.Max.X = BlendParamX.Max;
            GridBox.Min.Y = BlendParamY.Min;
            GridBox.Max.Y = BlendParamY.Max;

            Vector3 Size = GridBox.GetSize();

            Size.X = Math.Max(Size.X, 0.00001f);
            Size.Y = Math.Max(Size.Y, 0.00001f);
            Size.Z = Math.Max(Size.Z, 0.00001f);

            GridMin = GridBox.Min;
            RecipGridSize.X = 1.0f / Size.X;
            RecipGridSize.Y = 1.0f / Size.Y;
            RecipGridSize.Z = 1.0f / Size.Z;
        }

        /** 
         * The key function in Delaunay Triangulation
         * return true if the TestPoint is WITHIN the triangle circumcircle
         *	http://en.wikipedia.org/wiki/Delaunay_triangulation 
         */
        private ECircumCircleState GetCircumcircleState(TtBSTriangle triangle, TtBSPoint testPoint)
        {
            int NumPointsPerTriangle = 3;

            // First off, normalize all the points
            Vector3[] NormalizedPositions = new Vector3[NumPointsPerTriangle];

            // Unrolled loop
            NormalizedPositions[0].X = (triangle.Vertices[0].Position - GridMin).X * RecipGridSize.X;
            NormalizedPositions[0].Y = (triangle.Vertices[0].Position - GridMin).Y * RecipGridSize.Y;
            NormalizedPositions[0].Z = (triangle.Vertices[0].Position - GridMin).Z * RecipGridSize.Z;
            NormalizedPositions[1].X = (triangle.Vertices[1].Position - GridMin).X * RecipGridSize.X;
            NormalizedPositions[1].Y = (triangle.Vertices[1].Position - GridMin).Y * RecipGridSize.Y;
            NormalizedPositions[1].Z = (triangle.Vertices[1].Position - GridMin).Z * RecipGridSize.Z;
            NormalizedPositions[2].X = (triangle.Vertices[2].Position - GridMin).X * RecipGridSize.X;
            NormalizedPositions[2].Y = (triangle.Vertices[2].Position - GridMin).Y * RecipGridSize.Y;
            NormalizedPositions[2].Z = (triangle.Vertices[2].Position - GridMin).Z * RecipGridSize.Z;

            Vector3 NormalizedTestPoint = Vector3.Zero;
            NormalizedTestPoint.X = (testPoint.Position - GridMin).X * RecipGridSize.X;
            NormalizedTestPoint.Y = (testPoint.Position - GridMin).Y * RecipGridSize.Y;
            NormalizedTestPoint.Z = (testPoint.Position - GridMin).Z * RecipGridSize.Z;

            // ignore Z, eventually this has to be on plane
            // http://en.wikipedia.org/wiki/Delaunay_triangulation - determinant
            float M00 = NormalizedPositions[0].X - NormalizedTestPoint.X;
            float M01 = NormalizedPositions[0].Y - NormalizedTestPoint.Y;
            float M02 = NormalizedPositions[0].X * NormalizedPositions[0].X - NormalizedTestPoint.X * NormalizedTestPoint.X
                + NormalizedPositions[0].Y * NormalizedPositions[0].Y - NormalizedTestPoint.Y * NormalizedTestPoint.Y;

            float M10 = NormalizedPositions[1].X - NormalizedTestPoint.X;
            float M11 = NormalizedPositions[1].Y - NormalizedTestPoint.Y;
            float M12 = NormalizedPositions[1].X * NormalizedPositions[1].X - NormalizedTestPoint.X * NormalizedTestPoint.X
                + NormalizedPositions[1].Y * NormalizedPositions[1].Y - NormalizedTestPoint.Y * NormalizedTestPoint.Y;

            float M20 = NormalizedPositions[2].X - NormalizedTestPoint.X;
            float M21 = NormalizedPositions[2].Y - NormalizedTestPoint.Y;
            float M22 = NormalizedPositions[2].X * NormalizedPositions[2].X - NormalizedTestPoint.X * NormalizedTestPoint.X
                + NormalizedPositions[2].Y * NormalizedPositions[2].Y - NormalizedTestPoint.Y * NormalizedTestPoint.Y;

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

        /**
         * return true if they can make triangle
         */
        private bool IsEligibleForTriangulation(TtBSPoint A, TtBSPoint B, TtBSPoint C)
        {
            return (IsCollinear(A, B, C) == false);
        }

        /** 
         * return true if 3 points are collinear
         * by that if those 3 points create straight line
         */
        private bool IsCollinear(TtBSPoint A, TtBSPoint B, TtBSPoint C)
        {
            // this eventually has to happen on the plane that contains this 3 pages
            // for now we ignore Z
            Vector3 Diff1 = B.Position - A.Position;
            Vector3 Diff2 = C.Position - A.Position;

            float Result = Diff1.X * Diff2.Y - Diff1.Y * Diff2.X;

            return (Result == 0.0f);
        }

        /** 
         * return true if all points are coincident
         * (i.e. if all points are the same)
         */
        private bool AllCoincident(List<TtBSPoint> InPoints)
        {
            if (InPoints.Count > 0)
            {
                TtBSPoint FirstPoint = InPoints[0];
                for (int PointIndex = 0; PointIndex < InPoints.Count; ++PointIndex)
                {
                    TtBSPoint Point = InPoints[PointIndex];
                    if (Point.Position != FirstPoint.Position)
                    {
                        return false;
                    }
                }

                return true;
            }

            return false;
        }

        /**
         * Flip TriangleList(I) with TriangleList(J). 
         */
        private bool FlipTriangles(int TriangleIndexOne, int TriangleIndexTwo)
        {
            TtBSTriangle A = TriangleList[TriangleIndexOne];
            TtBSTriangle B = TriangleList[TriangleIndexTwo];

            // if already optimized, don't have to do any
            TtBSPoint TestPt = A.FindNonSharingPoint(B);

            // If it's not inside, we don't have to do any
            if (GetCircumcircleState(A, TestPt) != ECircumCircleState.Inside)
            {
                return false;
            }

            TtBSTriangle[] NewTriangles = new TtBSTriangle[2];
            int TrianglesMade = 0;

            for (int VertexIndexOne = 0; VertexIndexOne < 2; ++VertexIndexOne)
            {
                for (int VertexIndexTwo = VertexIndexOne + 1; VertexIndexTwo < 3; ++VertexIndexTwo)
                {
                    // Check if these vertices form a valid triangle (should be non-colinear)
                    if (IsEligibleForTriangulation(A.Vertices[VertexIndexOne], A.Vertices[VertexIndexTwo], TestPt))
                    {
                        // Create the new triangle and check if the final (original) vertex falls inside or outside of it's circumcircle
                        TtBSTriangle NewTriangle = new TtBSTriangle(A.Vertices[VertexIndexOne], A.Vertices[VertexIndexTwo], TestPt);
                        int VertexIndexThree = 3 - (VertexIndexTwo + VertexIndexOne);
                        if (GetCircumcircleState(NewTriangle, A.Vertices[VertexIndexThree]) == ECircumCircleState.Outside)
                        {
                            // If so store the triangle and increment the number of triangles
                            //checkf(TrianglesMade < 2, TEXT("Incorrect number of triangles created"));
                            NewTriangles[TrianglesMade] = NewTriangle;
                            ++TrianglesMade;
                        }
                    }
                }
            }

            // In case two triangles were generated the flip was successful so we can add them to the list
            if (TrianglesMade == 2)
            {
                AddTriangle(NewTriangles[0], false);
                AddTriangle(NewTriangles[1], false);
            }

            return TrianglesMade == 2;
        }

        /** 
         * Add new Triangle
         */
        private void AddTriangle(TtBSTriangle triangle, bool bCheckHalfEdge = true)
        {
            // see if it's same vertices
            for (int i = 0; i < TriangleList.Count; ++i)
            {
                if (triangle == TriangleList[i])
                {
                    return;
                }

                if (bCheckHalfEdge && triangle.HasSameHalfEdge(TriangleList[i]))
                {
                    return;
                }
            }

            TriangleList.Add(new TtBSTriangle(triangle.Vertices[0], triangle.Vertices[1], triangle.Vertices[2]));
        }

        /** 
         * Used as incremental step to triangulate all points
         * Create triangles TotalNum of PointList
         */
        private int GenerateTriangles(List<TtBSPoint> PointList, int TotalNum)
        {
            if (TotalNum == 3)
            {
                if (IsEligibleForTriangulation(PointList[0], PointList[1], PointList[2]))
                {
                    TtBSTriangle Triangle = new TtBSTriangle(PointList[0], PointList[1], PointList[2]);
                    AddTriangle(Triangle);
                }
            }
            else if (TriangleList.Count == 0)
            {
                TtBSPoint TestPoint = PointList[TotalNum - 1];

                // so far no triangle is made, try to make it with new points that are just entered
                for (int I = 0; I < TotalNum - 2; ++I)
                {
                    if (IsEligibleForTriangulation(PointList[I], PointList[I + 1], TestPoint))
                    {
                        TtBSTriangle NewTriangle = new TtBSTriangle(PointList[I], PointList[I + 1], TestPoint);
                        AddTriangle(NewTriangle);
                    }
                }
            }
            else
            {
                // get the last addition
                TtBSPoint TestPoint = PointList[TotalNum - 1];
                int TriangleNum = TriangleList.Count;

                for (int I = 0; I < TriangleList.Count; ++I)
                {
                    TtBSTriangle Triangle = TriangleList[I];
                    if (IsEligibleForTriangulation(Triangle.Vertices[0], Triangle.Vertices[1], TestPoint))
                    {
                        TtBSTriangle NewTriangle = new TtBSTriangle(Triangle.Vertices[0], Triangle.Vertices[1], TestPoint);
                        AddTriangle(NewTriangle);
                    }

                    if (IsEligibleForTriangulation(Triangle.Vertices[0], Triangle.Vertices[2], TestPoint))
                    {
                        TtBSTriangle NewTriangle = new TtBSTriangle(Triangle.Vertices[0], Triangle.Vertices[2], TestPoint);
                        AddTriangle(NewTriangle);
                    }

                    if (IsEligibleForTriangulation(Triangle.Vertices[1], Triangle.Vertices[2], TestPoint))
                    {
                        TtBSTriangle NewTriangle = new TtBSTriangle(Triangle.Vertices[1], Triangle.Vertices[2], TestPoint);
                        AddTriangle(NewTriangle);
                    }
                }

                // this is locally optimization part
                // we need to make sure all triangles are locally optimized. If not optimize it. 
                for (int I = 0; I < TriangleList.Count; ++I)
                {
                    TtBSTriangle A = TriangleList[I];
                    for (int J = I + 1; J < TriangleList.Count; ++J)
                    {
                        TtBSTriangle B = TriangleList[J];

                        // does share same edge
                        if (A.DoesShareSameEdge(B))
                        {
                            // then test to see if locally optimized
                            if (FlipTriangles(I, J))
                            {
                                //// if this flips, remove current triangle
                                //delete TriangleList[I];
                                //delete TriangleList[J];
                                //I need to remove J first because other wise, 
                                //  index J isn't valid anymore
                                TriangleList.RemoveAt(J);
                                TriangleList.RemoveAt(I);
                                // start over since we modified triangle
                                // once we don't have any more to flip, we're good to go!
                                I = -1;
                                break;
                            }
                        }
                    }
                }
            }

            return TriangleList.Count;
        }

        public List<TtBSPoint> GetSamplePointList() { return SamplesList; }
        List<TtBSPoint> SamplesList = new List<TtBSPoint>();
        List<int> Sorted2Indices = new List<int>();
        List<TtBSTriangle> TriangleList = new List<TtBSTriangle>();
        Vector3 GridMin = Vector3.Zero;
        Vector3 RecipGridSize = Vector3.Zero;
    }
}
