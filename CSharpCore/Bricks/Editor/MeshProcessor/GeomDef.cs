using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace EngineNS.Bricks.MeshProcessor
{
    public class MeshAtom
    {
        public CDrawPrimitiveDesc OriginDesc;
        public int Mtl;
        public List<int> Faces = new List<int>();
        public List<CDrawPrimitiveDesc> Lods = new List<CDrawPrimitiveDesc>();
        public List<int> OriFaces;
        
        public List<List<int>> FaceLods = new List<List<int>>();

        public List<int> FinalFaces = new List<int>();
        public List<CDrawPrimitiveDesc> FinalLods = new List<CDrawPrimitiveDesc>();
        public void BuildLodInfos(List<float> lods)
        {
            int FullFace = (int)OriginDesc.NumPrimitives;
            if(FaceLods[0].Count - FullFace>0)
            {
                int firstAdd = FaceLods[0].Count - FullFace;
                FaceLods[0].RemoveRange(FullFace, firstAdd);
            }
            int curLod = 0;
            int limit = (int)((float)FullFace * lods[curLod]);
            List<List<int>> finalLods = new List<List<int>>();
            for (int i = 0; i < FaceLods.Count; i++)
            {
                if(FaceLods[i].Count > limit)
                {
                    continue;
                }
                finalLods.Add(FaceLods[i]);
                curLod++;
                if (curLod >= lods.Count)
                    break;
                limit = (int)((float)FullFace * lods[curLod]);
            }
            
            //BuildFinalFaces(finalLods);
            //int f1 = FinalFaces.Count;
            BuildFinalFaces2(finalLods);
            //int f2 = FinalFaces.Count;
            //System.Diagnostics.Debug.WriteLine(f2 - f1);
        }
        public void BuildFinalFaces(List<List<int>> finalLods)
        {
            FinalFaces.Clear();
            FinalLods.Clear();
            int curStartFace = 0;
            for (int i = 0; i < finalLods.Count; i++)
            {
                FinalFaces.AddRange(finalLods[i]);

                var desc = new CDrawPrimitiveDesc();
                desc.SetDefault();
                desc.StartIndex = (UInt32)curStartFace * 3;
                desc.NumPrimitives = (UInt32)finalLods[i].Count;
                FinalLods.Add(desc);
                
                curStartFace += finalLods[i].Count;
            }
        }
        public void BuildFinalFaces2(List<List<int>> finalLods)
        {
            FinalFaces.Clear();
            FinalLods.Clear();
            int curStart = 0;
            for (int i = 0; i < finalLods.Count; i++)
            {
                var both = new List<int>();
                for(int j=curStart; j < FinalFaces.Count; j++)
                {
                    var at = finalLods[i].IndexOf(FinalFaces[j]);
                    if (at>=0)
                    {
                        both.Add(FinalFaces[j]);
                        FinalFaces.RemoveAt(j);
                        finalLods[i].RemoveAt(at);
                    }
                }
                
                var desc = new CDrawPrimitiveDesc();
                desc.SetDefault();
                desc.StartIndex = (UInt32)FinalFaces.Count * 3;
                desc.NumPrimitives = (UInt32)(finalLods[i].Count + both.Count);
                FinalLods.Add(desc);
                FinalFaces.AddRange(both);
                curStart = FinalFaces.Count;
                FinalFaces.AddRange(finalLods[i]);
            }
        }
        public void BuildIndexBuffer(EditableMesh mesh)
        {
            var curFaces = new List<int>();
            for (var i = 0; i < Faces.Count; i++)
            {
                var f = mesh.GetFace(Faces[i]);
                if (f.Deleted)
                    continue;
                curFaces.Add(Faces[i]);
            }

            Faces = curFaces;

            FaceLods.Add(curFaces);

            var desc = new CDrawPrimitiveDesc();
            desc.SetDefault();
            desc.StartIndex = 0;
            desc.NumPrimitives = (UInt32)Faces.Count;

            Lods.Clear();
            Lods.Add(desc);

            System.Diagnostics.Debug.WriteLine($"Face({Mtl}) = {desc.NumPrimitives}");
        }
        public void AddFace(int face)
        {
            if (Faces.Contains(face))
                return;
            Faces.Add(face);
        }
        public int GetDeletedFaceNum(EditableMesh mesh)
        {
            int num = 0;
            for (int i = 0; i < Faces.Count; i++)
            {
                if (mesh.Faces[Faces[i]].Deleted)
                    num++;
                else
                    break;
            }
            return num;
        }
        public void FaceDeleted(EditableMesh mesh, int face)
        {
            int fIdx = -1;
            for (int i = 0; i < Faces.Count; i++)
            {
                if (Faces[i] == face)
                {
                    fIdx = i;
                    break;
                }
            }
            if (fIdx<0)
            {
                return;
            }
            int num = 0;
            for (int i = 0; i < Faces.Count; i++)
            {
                if (mesh.Faces[Faces[i]].Deleted)
                    num++;
                else
                    break;
            }
            //if(num== TestFace)
            //{
            //    int xxx = 0;
            //}
            //最后一个面被删除的情况
            if (num == Faces.Count)
                return;
            var save = Faces[num];
            Faces[num] = Faces[fIdx];
            Faces[fIdx] = save;
        }

        public void BuildIndexBuffer(EditableMesh mesh, List<int> indices)
        {
            for(int i=0; i<Faces.Count; i++)
            {
                var f = mesh.Faces[Faces[i]];
                indices.Add(f.A);
                indices.Add(f.B);
                indices.Add(f.C);
            }
        }
        public void BuildIndexBuffer(EditableMesh mesh, List<UInt16> indices)
        {
            if (FinalFaces.Count > 0)
            {
                for (int i = 0; i < FinalFaces.Count; i++)
                {
                    var f = mesh.Faces[FinalFaces[i]];
                    indices.Add((UInt16)f.A);
                    indices.Add((UInt16)f.B);
                    indices.Add((UInt16)f.C);
                }
            }
            else
            {
                for (int i = 0; i < Faces.Count; i++)
                {
                    var f = mesh.Faces[Faces[i]];
                    indices.Add((UInt16)f.A);
                    indices.Add((UInt16)f.B);
                    indices.Add((UInt16)f.C);
                }
            }
        }
    }
    public class Vertex
    {
        public Vector3 Position;
        public Vector3 Normal;
        public Vector4 Tangent;
        public Byte4 Color;
        public Vector2 UV;
        public Vector2 LightMapUV;
        public Byte4 SkinIndex;
        public Vector4 SkinWeight;
        public Byte4 TerrainIndex;
        public Byte4 TerrainGradient;
        public VertexLinker Linker = new VertexLinker();
    }

    [StructLayout(LayoutKind.Explicit, Pack = 4)]
    public class Edge
    {
        [FieldOffset(0)]
        public UInt32 A;
        [FieldOffset(4)]
        public UInt32 B;
        [FieldOffset(0)]
        public UInt64 CmpValue;

        [FieldOffset(8)]
        public List<int> Faces = new List<int>();
        protected Edge()
        {

        }
        public static Edge CreateEdge(int a, int b)
        {
            var result = new Edge();
            if (a < b)
            {
                result.A = (UInt32)a;
                result.B = (UInt32)b;
            }
            else
            {
                result.A = (UInt32)b;
                result.B = (UInt32)a;
            }
            return result;
        }
        public static bool QuickFind(List<Edge> edges, Edge e, out int index)
        {
            int start = 0;
            int end = edges.Count - 1;
            int mid = 0;
            while (start<=end)
            {
                mid = (end + start) / 2;
                if (edges[mid].CmpValue == e.CmpValue)
                {
                    index = mid;
                    return true;
                }
                else if (edges[mid].CmpValue < e.CmpValue)
                {
                    start = mid + 1;
                }
                else
                {
                    end = mid - 1;
                }
            }
            index = mid;
            return false;
        }
        //public bool IsSame(int a, int b)
        //{
        //    if (a == A && b == B)
        //        return true;
        //    if (a == B && b == A)
        //        return true;
        //    return false;
        //}
        internal void AddFace(int face)
        {
            for(int i=0;i< Faces.Count; i++)
            {
                if (Faces[i] == face)
                    return;
            }
            Faces.Add(face);
        }
        public bool IsAloneEdge()
        {
            return Faces.Count == 1;
        }
    }
    public class VertexLinker
    {
        public void Reset()
        {
            VertexValue = 0;
            Vertices.Clear();
            Edges.Clear();
            Faces.Clear();
        }
        public float ExtraValue = 0;
        public float VertexValue;
        public List<int> Vertices = new List<int>();
        public List<int> Edges = new List<int>();
        public List<int> Faces = new List<int>();
        public List<int> FixedVertices = new List<int>();
        public void AddFace(int face)
        {
            for(int i=0; i< Faces.Count; i++)
            {
                if (Faces[i] == face)
                    return;
            }
            Faces.Add(face);
        }
        public void AddVertex(int vtx)
        {
            for (int i = 0; i < Vertices.Count; i++)
            {
                if (Vertices[i] == vtx)
                    return;
            }
            Vertices.Add(vtx);
        }
        public void AddEdge(int e)
        {
            for (int i = 0; i < Edges.Count; i++)
            {
                if (Edges[i] == e)
                    return;
            }
            Edges.Add(e);
        }
    }

    public class Face
    {
        public bool Deleted = false;
        public int A;
        public int B;
        public int C;
        public int Gen;
        public int Mtl;
        public float Area;
        public Vector3 FaceNormal;
        public object ExtData;
        public bool Contain(int v)
        {
            return (A == v) || (B == v) || (C == v);
        }
        public bool IsA(int v)
        {
            return A == v;
        }
        public bool IsB(int v)
        {
            return B == v;
        }
        public bool IsC(int v)
        {
            return C == v;
        }
    }
}
