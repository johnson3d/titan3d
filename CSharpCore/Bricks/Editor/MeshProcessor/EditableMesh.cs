using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.MeshProcessor
{
    public class EditableMesh
    {
        public List<Vertex> Vertices = new List<Vertex>();
        public List<Face> Faces = new List<Face>();
        public List<Edge> Edges = new List<Edge>();
        public List<MeshAtom> Atoms;
        public BoundingBox AABB;
        internal List<float> Lods = new List<float>();
        public void ClearLods()
        {
            Lods.Clear();
            Lods.Add(1.0f);
        }
        public bool PushLod(float rate)
        {
            if (rate > 1.0f || rate < 0)
                return false;
            if (Lods[Lods.Count - 1] < rate)
                return false;

            Lods.Add(rate);
            return true;
        }
        public Vertex GetVertex(int index)
        {
            return Vertices[index];
        }
        public Face GetFace(int index)
        {
            return Faces[index];
        }
        public bool IsEdgeVertex(int vtx, int target)
        {
            foreach(var i in Vertices[vtx].Linker.Faces)
            {
                if(GetFace(i).Contain(target)==false)
                {
                    return false;
                }
            }
            return true;
        }
        public bool HasAloneEdge(int face)
        {
            var f = GetFace(face);
            var e = FindEdge(f.A, f.B);
            if (Edges[e].IsAloneEdge())
                return true;
            e = FindEdge(f.A, f.C);
            if (Edges[e].IsAloneEdge())
                return true;
            e = FindEdge(f.B, f.C);
            if (Edges[e].IsAloneEdge())
                return true;
            return false;
        }
        public int GetValidFaceNumber()
        {
            int num = 0;
            foreach(var i in Faces)
            {
                if (i.Deleted == false)
                    num++;
            }
            return num;
        }
        public int AloneEdgeNumber
        {
            get
            {
                int num = 0;
                foreach (var i in Edges)
                {
                    if (i.IsAloneEdge())
                        num++;
                }
                return num;
            }
        }
        public bool EnablePosition;
        public bool EnableNormal;
        public bool EnableTangent;
        public bool EnableColor;
        public bool EnableUV;
        public bool EnableLightMapUV;
        public bool EnableSkinIndex;
        public bool EnableSkinWeight;
        public bool EnableTerrainIndex;
        public bool EnableTerrainGradient;

        internal int AddEdge(int face,int a, int b)
        {
            var e = Edge.CreateEdge(a, b);

            int idx;
            if(Edge.QuickFind(Edges, e, out idx))
            {
                Edges[idx].AddFace(face);
                return idx;
            }
            else
            {
                e.AddFace(face);
                if (Edges.Count == 0)
                {
                    Edges.Add(e);
                    return 0;
                }
                if (Edges[idx].CmpValue<=e.CmpValue)
                    Edges.Insert(idx + 1, e);
                else
                    Edges.Insert(idx, e);
                return idx;
            }
        }

        internal int FindEdge(int a, int b)
        {
            var e = Edge.CreateEdge(a, b);
            int idx;
            if (Edge.QuickFind(Edges, e, out idx))
            {
                return idx;
            }
            return -1;
        }

        protected Graphics.Mesh.CGfxMeshPrimitives mMesh;
        public bool InitMesh(CRenderContext rc, Graphics.Mesh.CGfxMeshPrimitives mesh)
        {
            mMesh = mesh;
            mesh.PreUse();
            EnablePosition = false;
            EnableNormal = false;
            EnableTangent = false;
            EnableColor = false;
            EnableUV = false;
            EnableLightMapUV = false;
            EnableSkinIndex = false;
            EnableSkinWeight = false;
            EnableTerrainIndex = false;
            EnableTerrainGradient = false;

            AABB = mesh.AABB;

            int vertNum = 0;
            Vertices.Clear();

            #region Position
            var vb = mesh.GeometryMesh.GetVertexBuffer(EVertexSteamType.VST_Position);
            if(vb!=null)
            {
                EnablePosition = true;
                var blob = new Support.CBlobObject();
                vb.GetBufferData(rc, blob);
                unsafe
                {
                    vertNum = (int)blob.Size / sizeof(Vector3);
                    var ptr = (Vector3*)blob.Data.ToPointer();
                    for (int i = 0; i < vertNum; i++)
                    {
                        var vert = new Vertex();
                        vert.Position = ptr[i];
                        Vertices.Add(vert);
                    }
                }
            }
            else
            {
                return false;
            }
            #endregion

            #region Normal
            vb = mesh.GeometryMesh.GetVertexBuffer(EVertexSteamType.VST_Normal);
            if (vb != null)
            {
                EnableNormal = true;
                var blob = new Support.CBlobObject();
                vb.GetBufferData(rc, blob);
                unsafe
                {
                    var ptr = (Vector3*)blob.Data.ToPointer();
                    for (int i = 0; i < vertNum; i++)
                    {
                        Vertices[i].Normal = ptr[i];
                    }
                }
            }
            #endregion

            #region Tangent
            vb = mesh.GeometryMesh.GetVertexBuffer(EVertexSteamType.VST_Tangent);
            if (vb != null)
            {
                EnableTangent = true;
                var blob = new Support.CBlobObject();
                vb.GetBufferData(rc, blob);
                unsafe
                {
                    var ptr = (Vector4*)blob.Data.ToPointer();
                    for (int i = 0; i < vertNum; i++)
                    {
                        Vertices[i].Tangent = ptr[i];
                    }
                }
            }
            #endregion

            #region Color
            vb = mesh.GeometryMesh.GetVertexBuffer(EVertexSteamType.VST_Color);
            if (vb != null)
            {
                EnableColor = true;
                var blob = new Support.CBlobObject();
                vb.GetBufferData(rc, blob);
                unsafe
                {
                    var ptr = (Byte4*)blob.Data.ToPointer();
                    for (int i = 0; i < vertNum; i++)
                    {
                        Vertices[i].Color = ptr[i];
                    }
                }
            }
            #endregion

            #region UV
            vb = mesh.GeometryMesh.GetVertexBuffer(EVertexSteamType.VST_UV);
            if (vb != null)
            {
                EnableUV = true;
                var blob = new Support.CBlobObject();
                vb.GetBufferData(rc, blob);
                unsafe
                {
                    var ptr = (Vector2*)blob.Data.ToPointer();
                    for (int i = 0; i < vertNum; i++)
                    {
                        Vertices[i].UV = ptr[i];
                    }
                }
            }
            #endregion

            #region LightMapUV
            vb = mesh.GeometryMesh.GetVertexBuffer(EVertexSteamType.VST_LightMap);
            if (vb != null)
            {
                EnableLightMapUV = true;
                var blob = new Support.CBlobObject();
                vb.GetBufferData(rc, blob);
                unsafe
                {
                    var ptr = (Vector2*)blob.Data.ToPointer();
                    for (int i = 0; i < vertNum; i++)
                    {
                        Vertices[i].LightMapUV = ptr[i];
                    }
                }
            }
            #endregion

            #region SkinIndex
            vb = mesh.GeometryMesh.GetVertexBuffer(EVertexSteamType.VST_SkinIndex);
            if (vb != null)
            {
                EnableSkinIndex = true;
                var blob = new Support.CBlobObject();
                vb.GetBufferData(rc, blob);
                unsafe
                {
                    var ptr = (Byte4*)blob.Data.ToPointer();
                    for (int i = 0; i < vertNum; i++)
                    {
                        Vertices[i].SkinIndex = ptr[i];
                    }
                }
            }
            #endregion

            #region SkinWeight
            vb = mesh.GeometryMesh.GetVertexBuffer(EVertexSteamType.VST_SkinWeight);
            if (vb != null)
            {
                EnableSkinWeight = true;
                var blob = new Support.CBlobObject();
                vb.GetBufferData(rc, blob);
                unsafe
                {
                    var ptr = (Vector4*)blob.Data.ToPointer();
                    for (int i = 0; i < vertNum; i++)
                    {
                        Vertices[i].SkinWeight = ptr[i];
                    }
                }
            }
            #endregion

            #region TerrainIndex
            vb = mesh.GeometryMesh.GetVertexBuffer(EVertexSteamType.VST_TerrainIndex);
            if (vb != null)
            {
                EnableTerrainIndex = true;
                var blob = new Support.CBlobObject();
                vb.GetBufferData(rc, blob);
                unsafe
                {
                    var ptr = (Byte4*)blob.Data.ToPointer();
                    for (int i = 0; i < vertNum; i++)
                    {
                        Vertices[i].TerrainIndex = ptr[i];
                    }
                }
            }
            #endregion

            #region TerrainGradient
            vb = mesh.GeometryMesh.GetVertexBuffer(EVertexSteamType.VST_TerrainGradient);
            if (vb != null)
            {
                EnableTerrainGradient = true;
                var blob = new Support.CBlobObject();
                vb.GetBufferData(rc, blob);
                unsafe
                {
                    var ptr = (Byte4*)blob.Data.ToPointer();
                    for (int i = 0; i < vertNum; i++)
                    {
                        Vertices[i].TerrainGradient = ptr[i];
                    }
                }
            }
            #endregion

            List<int> OriIndices = new List<int>();
            #region Face
            var ib = mesh.GeometryMesh.GetIndexBuffer();
            if(ib!=null)
            {
                var blob = new Support.CBlobObject();
                ib.GetBufferData(rc, blob);
                unsafe
                {
                    if (ib.Desc.Type == EIndexBufferType.IBT_Int16)
                    {
                        int indexNum = (int)blob.Size / (sizeof(UInt16));
                        var ptr = (UInt16*)blob.Data.ToPointer();
                        for (int i = 0; i < indexNum; i++)
                        {
                            OriIndices.Add(ptr[i]);
                        }
                    }
                    else
                    {
                        int indexNum = (int)blob.Size / (sizeof(int));
                        var ptr = (int*)blob.Data.ToPointer();
                        for (int i = 0; i < indexNum; i++)
                        {
                            OriIndices.Add(ptr[i]);
                        }
                    }
                }
            }
            #endregion

            #region Mtl
            Faces.Clear();
            Atoms = new List<MeshAtom>();
            for (UInt32 i = 0; i < mesh.AtomNumber; i++)
            {
                var atom = new MeshAtom();
                atom.Mtl = (int)i;
                mesh.GetAtom(i, 0, ref atom.OriginDesc);
                Atoms.Add(atom);

                int index = (int)atom.OriginDesc.StartIndex/3;
                for (int j = 0; j < atom.OriginDesc.NumPrimitives; j++)
                {
                    var f = new Face();
                    f.A = OriIndices[(int)atom.OriginDesc.StartIndex + j * 3 + 0];
                    f.B = OriIndices[(int)atom.OriginDesc.StartIndex + j * 3 + 1];
                    f.C = OriIndices[(int)atom.OriginDesc.StartIndex + j * 3 + 2];
                    f.Mtl = (int)i;
                    atom.Faces.Add(Faces.Count);
                    Faces.Add(f);
                }
            }
            #endregion

            BuildFixedVertices(0.000001f);
            BuildFaces();

            BuildEdges();
            BuildLinkers();

            for (int i = 0; i < Atoms.Count; i++)
            {
                Atoms[i].FaceLods.Clear();
                Atoms[i].FaceLods.Add(Atoms[i].Faces);
            }

            return true;
        }
        private bool SamePosition(ref Vector3 a, ref Vector3 b, float epsilon)
        {
            if (Math.Abs(a.X - b.X) < epsilon &&
                Math.Abs(a.Y - b.Y) < epsilon &&
                Math.Abs(a.Z - b.Z) < epsilon)
                return true;
            return false;
        }
        public void BuildFixedVertices(float epsilon)
        {
            for (int i = 0; i < Vertices.Count; i++)
            {
                Vertices[i].Linker.FixedVertices.Add(i);
                var pos = Vertices[i].Position;
                for (int j = 0; j < Vertices.Count; j++)
                {
                    if (i == j)
                    {
                        continue;
                    }
                    if(SamePosition(ref pos, ref Vertices[j].Position, epsilon))
                    {
                        Vertices[i].Linker.FixedVertices.Add(j);
                    }
                }
            }
        }
        public float AvgFaceArea = 0;
        public void BuildFaces()
        {
            for (int i = 0; i < Faces.Count; i++)
            {
                Faces[i].FaceNormal = Vector3.CalcFaceNormal(ref Vertices[Faces[i].A].Position,
                    ref Vertices[Faces[i].B].Position,
                    ref Vertices[Faces[i].C].Position);
                Faces[i].Area = Vector3.CalcArea3(ref Vertices[Faces[i].A].Position,
                    ref Vertices[Faces[i].B].Position,
                    ref Vertices[Faces[i].C].Position);
                Faces[i].Area = Math.Abs(Faces[i].Area) * 0.5f;
            }
            GetAvgFaceArea();
        }
        public float GetAvgFaceArea()
        {
            AvgFaceArea = 0;
            int fcount = 0;
            for (int i = 0; i < Faces.Count; i++)
            {
                var f = GetFace(i);
                if (f.Deleted)
                    continue;
                if (f.Area < 0)
                    continue;
                AvgFaceArea += f.Area;
                fcount++;
            }
            if (fcount == 0)
                return 0;
            AvgFaceArea = AvgFaceArea / (float)fcount;
            return AvgFaceArea;
        }
        public void BuildEdges()
        {
            if(Edges==null)
                Edges = new List<Edge>();
            else
                Edges = new List<Edge>(Edges.Capacity);
            for (int i = 0; i < Faces.Count; i++)
            {
                if (Faces[i].Deleted)
                    continue;
                AddEdge(i, Faces[i].A, Faces[i].B);
                AddEdge(i, Faces[i].A, Faces[i].C);
                AddEdge(i, Faces[i].C, Faces[i].B);
            }
        }
        public void BuildLinkers()
        {
            for (int i = 0; i < Vertices.Count; i++)
            {
                Vertices[i].Linker.Reset();
            }
            for (int i = 0; i < Faces.Count; i++)
            {
                if (Faces[i].Deleted)
                    continue;

                Vertices[Faces[i].A].Linker.AddFace(i);
                Vertices[Faces[i].B].Linker.AddFace(i);
                Vertices[Faces[i].C].Linker.AddFace(i);
                
                Vertices[Faces[i].A].Linker.AddVertex(Faces[i].B);
                Vertices[Faces[i].A].Linker.AddVertex(Faces[i].C);
                
                Vertices[Faces[i].B].Linker.AddVertex(Faces[i].A);
                Vertices[Faces[i].B].Linker.AddVertex(Faces[i].C);
                
                Vertices[Faces[i].C].Linker.AddVertex(Faces[i].A);
                Vertices[Faces[i].C].Linker.AddVertex(Faces[i].B);

                int e = this.FindEdge(Faces[i].A, Faces[i].B);
                Vertices[Faces[i].A].Linker.AddEdge(e);
                e = this.FindEdge(Faces[i].A, Faces[i].C);
                Vertices[Faces[i].A].Linker.AddEdge(e);

                e = this.FindEdge(Faces[i].B, Faces[i].A);
                Vertices[Faces[i].B].Linker.AddEdge(e);
                e = this.FindEdge(Faces[i].B, Faces[i].C);
                Vertices[Faces[i].B].Linker.AddEdge(e);

                e = this.FindEdge(Faces[i].C, Faces[i].A);
                Vertices[Faces[i].C].Linker.AddEdge(e);
                e = this.FindEdge(Faces[i].C, Faces[i].B);
                Vertices[Faces[i].C].Linker.AddEdge(e);
            }
        }

        public Graphics.Mesh.CGfxMeshPrimitives CookMesh(CRenderContext rc)
        {
            var result = CEngine.Instance.MeshPrimitivesManager.CreateMeshPrimitives(rc, (UInt32)Atoms.Count);

            var mesh = result.GeometryMesh;
            UInt32 resourceSize = 0;

            #region Position
            if(EnablePosition)
            {
                unsafe
                {
                    var vbDesc = new CVertexBufferDesc();
                    Vector3[] data = new Vector3[Vertices.Count];
                    for (int i = 0; i < data.Length; i++)
                    {
                        data[i] = Vertices[i].Position;
                    }
                    fixed (Vector3* ptr = &data[0])
                    {
                        vbDesc.InitData = (IntPtr)ptr;
                        vbDesc.Stride = (UInt32)sizeof(Vector3);
                        vbDesc.ByteWidth = (UInt32)(sizeof(Vector3) * data.Length);
                        var vb = rc.CreateVertexBuffer(vbDesc);
                        mesh.BindVertexBuffer(EVertexSteamType.VST_Position, vb);
                        resourceSize += vbDesc.ByteWidth;
                    }
                }
            }
            #endregion

            #region Normal
            if(EnableNormal)
            {
                unsafe
                {
                    var vbDesc = new CVertexBufferDesc();
                    Vector3[] data = new Vector3[Vertices.Count];
                    for (int i = 0; i < data.Length; i++)
                    {
                        data[i] = Vertices[i].Normal;
                    }
                    fixed (Vector3* ptr = &data[0])
                    {
                        vbDesc.InitData = (IntPtr)ptr;
                        vbDesc.Stride = (UInt32)sizeof(Vector3);
                        vbDesc.ByteWidth = (UInt32)(sizeof(Vector3) * data.Length);
                        var vb = rc.CreateVertexBuffer(vbDesc);
                        mesh.BindVertexBuffer(EVertexSteamType.VST_Normal, vb);
                        resourceSize += vbDesc.ByteWidth;
                    }
                }
            }
            #endregion

            #region Tangent
            if (EnableTangent)
            {
                unsafe
                {
                    var vbDesc = new CVertexBufferDesc();
                    Vector4[] data = new Vector4[Vertices.Count];
                    for (int i = 0; i < data.Length; i++)
                    {
                        data[i] = Vertices[i].Tangent;
                    }
                    fixed (Vector4* ptr = &data[0])
                    {
                        vbDesc.InitData = (IntPtr)ptr;
                        vbDesc.Stride = (UInt32)sizeof(Vector4);
                        vbDesc.ByteWidth = (UInt32)(sizeof(Vector4) * data.Length);
                        var vb = rc.CreateVertexBuffer(vbDesc);
                        mesh.BindVertexBuffer(EVertexSteamType.VST_Tangent, vb);
                        resourceSize += vbDesc.ByteWidth;
                    }
                }
            }
            #endregion

            #region Color
            if (EnableColor)
            {
                unsafe
                {
                    var vbDesc = new CVertexBufferDesc();
                    Byte4[] data = new Byte4[Vertices.Count];
                    for (int i = 0; i < data.Length; i++)
                    {
                        data[i] = Vertices[i].Color;
                    }
                    fixed (Byte4* ptr = &data[0])
                    {
                        vbDesc.InitData = (IntPtr)ptr;
                        vbDesc.Stride = (UInt32)sizeof(Byte4);
                        vbDesc.ByteWidth = (UInt32)(sizeof(Byte4) * data.Length);
                        var vb = rc.CreateVertexBuffer(vbDesc);
                        mesh.BindVertexBuffer(EVertexSteamType.VST_Color, vb);
                        resourceSize += vbDesc.ByteWidth;
                    }
                }
            }
            #endregion

            #region UV
            if (EnableUV)
            {
                unsafe
                {
                    var vbDesc = new CVertexBufferDesc();
                    Vector2[] data = new Vector2[Vertices.Count];
                    for (int i = 0; i < data.Length; i++)
                    {
                        data[i] = Vertices[i].UV;
                    }
                    fixed (Vector2* ptr = &data[0])
                    {
                        vbDesc.InitData = (IntPtr)ptr;
                        vbDesc.Stride = (UInt32)sizeof(Vector2);
                        vbDesc.ByteWidth = (UInt32)(sizeof(Vector2) * data.Length);
                        var vb = rc.CreateVertexBuffer(vbDesc);
                        mesh.BindVertexBuffer(EVertexSteamType.VST_UV, vb);
                        resourceSize += vbDesc.ByteWidth;
                    }
                }
            }
            #endregion

            #region LightMapUV
            if (EnableLightMapUV)
            {
                unsafe
                {
                    var vbDesc = new CVertexBufferDesc();
                    Vector2[] data = new Vector2[Vertices.Count];
                    for (int i = 0; i < data.Length; i++)
                    {
                        data[i] = Vertices[i].LightMapUV;
                    }
                    fixed (Vector2* ptr = &data[0])
                    {
                        vbDesc.InitData = (IntPtr)ptr;
                        vbDesc.Stride = (UInt32)sizeof(Vector2);
                        vbDesc.ByteWidth = (UInt32)(sizeof(Vector2) * data.Length);
                        var vb = rc.CreateVertexBuffer(vbDesc);
                        mesh.BindVertexBuffer(EVertexSteamType.VST_LightMap, vb);
                        resourceSize += vbDesc.ByteWidth;
                    }
                }
            }
            #endregion

            #region LightSkinIndex
            if (EnableSkinIndex)
            {
                unsafe
                {
                    var vbDesc = new CVertexBufferDesc();
                    Byte4[] data = new Byte4[Vertices.Count];
                    for (int i = 0; i < data.Length; i++)
                    {
                        data[i] = Vertices[i].SkinIndex;
                    }
                    fixed (Byte4* ptr = &data[0])
                    {
                        vbDesc.InitData = (IntPtr)ptr;
                        vbDesc.Stride = (UInt32)sizeof(Byte4);
                        vbDesc.ByteWidth = (UInt32)(sizeof(Byte4) * data.Length);
                        var vb = rc.CreateVertexBuffer(vbDesc);
                        mesh.BindVertexBuffer(EVertexSteamType.VST_SkinIndex, vb);
                        resourceSize += vbDesc.ByteWidth;
                    }
                }
            }
            #endregion

            #region LightSkinWeight
            if (EnableSkinWeight)
            {
                unsafe
                {
                    var vbDesc = new CVertexBufferDesc();
                    Vector4[] data = new Vector4[Vertices.Count];
                    for (int i = 0; i < data.Length; i++)
                    {
                        data[i] = Vertices[i].SkinWeight;
                    }
                    fixed (Vector4* ptr = &data[0])
                    {
                        vbDesc.InitData = (IntPtr)ptr;
                        vbDesc.Stride = (UInt32)sizeof(Vector4);
                        vbDesc.ByteWidth = (UInt32)(sizeof(Vector4) * data.Length);
                        var vb = rc.CreateVertexBuffer(vbDesc);
                        mesh.BindVertexBuffer(EVertexSteamType.VST_SkinWeight, vb);
                        resourceSize += vbDesc.ByteWidth;
                    }
                }
            }
            #endregion

            #region LightTerrainIndex
            if (EnableTerrainIndex)
            {
                unsafe
                {
                    var vbDesc = new CVertexBufferDesc();
                    Byte4[] data = new Byte4[Vertices.Count];
                    for (int i = 0; i < data.Length; i++)
                    {
                        data[i] = Vertices[i].TerrainIndex;
                    }
                    fixed (Byte4* ptr = &data[0])
                    {
                        vbDesc.InitData = (IntPtr)ptr;
                        vbDesc.Stride = (UInt32)sizeof(Byte4);
                        vbDesc.ByteWidth = (UInt32)(sizeof(Byte4) * data.Length);
                        var vb = rc.CreateVertexBuffer(vbDesc);
                        mesh.BindVertexBuffer(EVertexSteamType.VST_TerrainIndex, vb);
                        resourceSize += vbDesc.ByteWidth;
                    }
                }
            }
            #endregion

            #region LightTerrainGradient
            if (EnableTerrainGradient)
            {
                unsafe
                {
                    var vbDesc = new CVertexBufferDesc();
                    Byte4[] data = new Byte4[Vertices.Count];
                    for (int i = 0; i < data.Length; i++)
                    {
                        data[i] = Vertices[i].TerrainGradient;
                    }
                    fixed (Byte4* ptr = &data[0])
                    {
                        vbDesc.InitData = (IntPtr)ptr;
                        vbDesc.Stride = (UInt32)sizeof(Byte4);
                        vbDesc.ByteWidth = (UInt32)(sizeof(Byte4) * data.Length);
                        var vb = rc.CreateVertexBuffer(vbDesc);
                        mesh.BindVertexBuffer(EVertexSteamType.VST_TerrainGradient, vb);
                        resourceSize += vbDesc.ByteWidth;
                    }
                }
            }
            #endregion

            #region IndexBuffer & Atoms
            {
                if((UInt32)Vertices.Count>=UInt32.MaxValue)
                {
                    List<int> indices = new List<int>();
                    for (int i = 0; i < Atoms.Count; i++)
                    {
                        int mtlStart = indices.Count;
                        Atoms[i].BuildIndexBuffer(this, indices);
                        var desc = Atoms[i].Lods[0];
                        desc.StartIndex = desc.StartIndex + (UInt32)mtlStart;
                        result.PushAtomLOD((UInt32)i, ref desc);
                    }
                    unsafe
                    {
                        var ibDesc = new CIndexBufferDesc();
                        var data = indices.ToArray();
                        fixed (Int32* ptr = &data[0])
                        {
                            ibDesc.InitData = (IntPtr)ptr;
                            ibDesc.Type = EIndexBufferType.IBT_Int32;
                            ibDesc.ByteWidth = (UInt32)(sizeof(Int32) * data.Length);
                            var ib = rc.CreateIndexBuffer(ibDesc);
                            mesh.BindIndexBuffer(ib);
                            resourceSize += ibDesc.ByteWidth;
                        }
                    }
                }
                else
                {
                    List<UInt16> indices = new List<UInt16>();
                    for (int i = 0; i < Atoms.Count; i++)
                    {
                        int mtlStart = indices.Count;
                        Atoms[i].BuildIndexBuffer(this, indices);
                        var lod = Atoms[i].Lods;
                        if(Atoms[i].FinalLods.Count>0)
                        {
                            lod = Atoms[i].FinalLods;
                        }
                        for (int j = 0; j < lod.Count; j++)
                        {
                            var desc = lod[j];
                            if (desc.NumPrimitives > 0)
                            {
                                desc.StartIndex = desc.StartIndex + (UInt32)mtlStart;
                                result.PushAtomLOD((UInt32)i, ref desc);
                            }
                        }
                    }
                    if(indices.Count>0)
                    {
                        unsafe
                        {
                            var ibDesc = new CIndexBufferDesc();
                            var data = indices.ToArray();
                            fixed (UInt16* ptr = &data[0])
                            {
                                ibDesc.InitData = (IntPtr)ptr;
                                ibDesc.Type = EIndexBufferType.IBT_Int16;
                                ibDesc.ByteWidth = (UInt32)(sizeof(Int16) * data.Length);
                                var ib = rc.CreateIndexBuffer(ibDesc);
                                mesh.BindIndexBuffer(ib);
                                resourceSize += ibDesc.ByteWidth;
                            }
                        }
                    }
                }
            }
            #endregion

            result.AABB = AABB;

            result.ResourceState.ResourceSize = resourceSize;
            result.ResourceState.StreamState = EStreamingState.SS_Valid;
            result.ResourceState.KeepValid = true;

            return result;
        }
        public int GetValidLinkFace(int vert)
        {
            var cur = Vertices[vert];
            int num = 0;
            foreach (var i in cur.Linker.Faces)
            {
                if(GetFace(i).Deleted == false)
                {
                    num++;
                }
            }
            return num;
        }
        public void GetContainVertices(Rectangle rect, Graphics.CGfxCamera camera, List<int> verts, bool cullback=true)
        {
            //verts.Clear();
            for (int i=0; i<Vertices.Count; i++)
            {
                var cur = Vertices[i];
                if (GetValidLinkFace(i)==0)
                    continue;
                var screenPos = camera.CameraData.Trans2ViewPort(ref cur.Position);
                if(rect.Contains((int)screenPos.X, (int)screenPos.Y))
                {
                    var viewNorm = Vector3.TransformNormal(cur.Normal, camera.CameraData.ViewMatrix);
                    if (viewNorm.Z<=0 || cullback==false)
                        verts.Add(i);
                }
            }
        }
    }
}
