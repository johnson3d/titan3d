using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.MeshProcessor
{
    public class ProgressMeshProcessor : EditableMesh
    {
        public delegate float FGetVertexValue(int index, VertexLinker linker);
        public int AddFace(int a, int b, int c, int mtl, int gen)
        {//不排重了
            if (a == b || b == c || a == c)
                return -1;
            var f = new Face();
            f.A = a;
            f.B = b;
            f.C = c;
            f.Mtl = mtl;
            f.Gen = gen;
            f.FaceNormal = Vector3.CalcFaceNormal(ref Vertices[a].Position,
                    ref Vertices[b].Position,
                    ref Vertices[c].Position);
            f.Area = Vector3.CalcArea3(ref Vertices[a].Position,
                ref Vertices[b].Position,
                ref Vertices[c].Position);
            f.Area = Math.Abs(f.Area) * 0.5f;
            Faces.Add(f);

            var index = Faces.Count - 1;
            Atoms[mtl].AddFace(index);
            return index;
        }
        public float DftGetVertexValue(int index, VertexLinker linker)
        {
            var center = Vertices[index];
            
            linker.VertexValue = 0;
            if(linker.Vertices.Count==0)
            {
                foreach(var i in linker.Faces)
                {
                    GetFace(i).Deleted = true;
                }
                return -1;
            }
            float TotalDistance = 0;
            var refFaces = new List<int>();
            foreach(var i in linker.FixedVertices)
            {
                foreach (var j in GetVertex(i).Linker.Faces)
                {
                    if (refFaces.Contains(j) == false)
                        refFaces.Add(j);
                }

                foreach (var j in GetVertex(i).Linker.Vertices)
                {
                    TotalDistance += Vector3.Distance(ref center.Position, ref GetVertex(j).Position);
                    foreach (var k in GetVertex(j).Linker.Vertices)
                    {
                        TotalDistance += Vector3.Distance(ref center.Position, ref GetVertex(k).Position);
                        foreach (var m in GetVertex(k).Linker.Faces)
                        {
                            if (refFaces.Contains(m) == false)
                                refFaces.Add(m);
                        }
                    }
                }
            }

            if (refFaces.Count == 0)
                return -1;

            foreach(var i in refFaces)
            {
                linker.VertexValue += GetFace(i).Area;// * (1 + GetFace(i).Gen * 100);
                linker.VertexValue += (float)GetFace(i).Gen * AvgFaceArea * 100.0f;
            }
            linker.VertexValue += TotalDistance;
            linker.VertexValue += linker.ExtraValue;

            return linker.VertexValue;
        }
        public int FindMinor(FGetVertexValue fun)
        {
            if(fun == null)
            {
                fun = DftGetVertexValue;
            }

            float vertValue = float.MaxValue;
            int result = -1;
            for (int i = 0; i < Vertices.Count; i++)
            {
                var linker = Vertices[i].Linker;
                var ret = fun(i, linker);
                if(ret<0)
                {
                    continue;
                }
                if (linker.VertexValue <= vertValue)
                {
                    vertValue = linker.VertexValue;
                    result = i;
                }
            }
            return result;
        }
        public void MergeVertices(int idx)
        {
            int addNum = 0;
            int delNum = 0;
            foreach (var i in GetVertex(idx).Linker.FixedVertices)
            {
                List<int> verts = GetVertex(i).Linker.Vertices;
                foreach (var j in verts)
                {
                    foreach(var k in GetVertex(j).Linker.FixedVertices)
                    {
                        var faces = GetVertex(k).Linker.Faces;
                        foreach (var m in faces)
                        {
                            var f = GetFace(m);
                            if (f.Deleted == true)
                            {
                                continue;
                            }
                            Atoms[f.Mtl].FaceDeleted(this, m);
                            f.Deleted = true;
                            delNum++;
                            if (f.Contain(i))
                            {
                                continue;
                            }
                            if (f.IsA(k))
                            {
                                if (AddFace(idx, f.B, f.C, f.Mtl, f.Gen + 1) > 0)
                                {
                                    addNum++;
                                }
                            }
                            else if (f.IsB(k))
                            {
                                if (AddFace(f.A, idx, f.C, f.Mtl, f.Gen + 1) > 0)
                                {
                                    addNum++;
                                }
                            }
                            else if (f.IsC(k))
                            {
                                if (AddFace(f.A, f.B, idx, f.Mtl, f.Gen + 1) > 0)
                                {
                                    addNum++;
                                }
                            }
                        }
                    }
                }
            }

            if(addNum>delNum)
            {
               
            }

            BuildEdges();
            BuildLinkers();
            GetAvgFaceArea();
        }

        public void FixDeletedFaces()
        {
            foreach(var i in Atoms)
            {
                i.BuildIndexBuffer(this);
            }
        }

        public void BuildLodInfos()
        {
            foreach (var i in Atoms)
            {
                i.BuildLodInfos(Lods);
            }
        }
    }
}
