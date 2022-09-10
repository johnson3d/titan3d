using System;
using System.Collections.Generic;
using EngineNS.Bricks.NodeGraph;

namespace EngineNS.Bricks.Procedure.Node
{
    [Bricks.CodeBuilder.ContextMenu("MeshLoader", "Mesh\\Loader", UPgcGraph.PgcEditorKeyword)]
    public class UMeshLoader : UPgcNodeBase
    {
        [EGui.Controls.PropertyGrid.PGCustomValueEditor(HideInPG = true)]
        public PinOut IndicesPin { get; set; } = new PinOut();
        [EGui.Controls.PropertyGrid.PGCustomValueEditor(HideInPG = true)]
        public PinOut PosPin { get; set; } = new PinOut();
        [EGui.Controls.PropertyGrid.PGCustomValueEditor(HideInPG = true)]
        public PinOut NorPin { get; set; } = new PinOut();
        [EGui.Controls.PropertyGrid.PGCustomValueEditor(HideInPG = true)]
        public PinOut UVPin { get; set; } = new PinOut();
        public override int PreviewResultIndex
        {
            get => mPreviewResultIndex;
            set
            {
                mPreviewResultIndex = -1;
            }
        }
        public UBufferCreator IndexBufferCreator { get; } = UBufferCreator.CreateInstance<USuperBuffer<Int32_3, FInt3Operator>>(-1, -1, -1);
        public UBufferCreator Vec3BufferCreator { get; } = UBufferCreator.CreateInstance<USuperBuffer<Vector3, FFloat3Operator>>(-1, -1, -1);

        public Graphics.Mesh.UMaterialMesh PreviewMesh;
        public UMeshLoader()
        {
            PrevSize = new Vector2(100, 100);

            Icon.Size = new Vector2(25, 25);
            Icon.Color = 0xFF00FF00;
            TitleColor = 0xFF204020;
            BackColor = 0x80808080;

            AddOutput(IndicesPin, "Indices", IndexBufferCreator);
            AddOutput(PosPin, "Pos", Vec3BufferCreator);
            AddOutput(NorPin, "Nor", Vec3BufferCreator);
            AddOutput(UVPin, "UV", Vec3BufferCreator);
        }
        ~UMeshLoader()
        {
            if (Task != null && Task.Result != null)
            {
                Task.Result.mTextureRSV?.Dispose();
                Task = null;
            }
        }
        public Graphics.Mesh.UMaterialMesh Mesh;
        RName mMeshName;
        [Rtti.Meta]
        [RName.PGRName(FilterExts = Graphics.Mesh.UMaterialMesh.AssetExt)]
        public RName MeshName
        {
            get => mMeshName;
            set
            {
                mMeshName = value;
                System.Action exec = async () =>
                {
                    Mesh = await UEngine.Instance.GfxDevice.MaterialMeshManager.GetMaterialMesh(value);
                    await Mesh.Mesh.LoadMeshDataProvider();
                };
                exec();
            }
        }
        public override UBufferCreator GetOutBufferCreator(PinOut pin)
        {
            if (pin == IndicesPin)
                return IndexBufferCreator;
            return Vec3BufferCreator;
        }
        public unsafe override bool InitProcedure(UPgcGraph graph)
        {
            if (Mesh == null)
                return false;
            var builder = Mesh.Mesh.MeshDataProvider.mCoreObject;
            var pPos = (Vector3*)builder.GetStream(NxRHI.EVertexStreamType.VST_Position).GetData();
            var pNor = (Vector3*)builder.GetStream(NxRHI.EVertexStreamType.VST_Normal).GetData();
            var pUV = (Vector2*)builder.GetStream(NxRHI.EVertexStreamType.VST_UV).GetData();

            int NunPfTrian = 0;
            for (uint i = 0; i < builder.GetAtomNumber(); i++)
            {
                var desc = new NxRHI.FMeshAtomDesc();

                desc = *builder.GetAtom(i, 0);
                
                NunPfTrian += (int)desc.NumPrimitives;
            }
            
            var idxBuffer = UBufferConponent.CreateInstance(UBufferCreator.CreateInstance<USuperBuffer<Int32_3, FInt3Operator>>(NunPfTrian, 1, 1));
            
            if (Mesh.Mesh.MeshDataProvider.mCoreObject.IsIndex32)
            {
                var pIndices = (int*)Mesh.Mesh.MeshDataProvider.mCoreObject.GetIndices().GetData();
                for (int i = 0; i < (int)NunPfTrian; i++)
                {
                    Int32_3 tmp;
                    tmp.X = pIndices[3 * i];
                    tmp.Y = pIndices[3 * i + 1];
                    tmp.Z = pIndices[3 * i + 2];
                    idxBuffer.SetPixel<Int32_3>(i, in tmp);
                }
            }
            else
            {
                var pIndices = (ushort*)Mesh.Mesh.MeshDataProvider.mCoreObject.GetIndices().GetData();
                for (int i = 0; i < (int)NunPfTrian; i++)
                {
                    Int32_3 tmp;
                    tmp.X = pIndices[3 * i];
                    tmp.Y = pIndices[3 * i + 1];
                    tmp.Z = pIndices[3 * i + 2];
                    idxBuffer.SetPixel<Int32_3>(i, in tmp);
                }
            }

            var posBuffer = UBufferConponent.CreateInstance(UBufferCreator.CreateInstance<USuperBuffer<Vector3, FFloat3Operator>>((int)builder.VertexNumber, 1, 1));
            var norBuffer = UBufferConponent.CreateInstance(UBufferCreator.CreateInstance<USuperBuffer<Vector3, FFloat3Operator>>((int)builder.VertexNumber, 1, 1));
            var uvBuffer = UBufferConponent.CreateInstance(UBufferCreator.CreateInstance<USuperBuffer<Vector2, FFloat2Operator>>((int)builder.VertexNumber, 1, 1));

            graph.BufferCache.RegBuffer(IndicesPin, idxBuffer);
            graph.BufferCache.RegBuffer(PosPin, posBuffer);
            graph.BufferCache.RegBuffer(NorPin, norBuffer);
            graph.BufferCache.RegBuffer(UVPin, uvBuffer);

            for (int i = 0; i < (int)builder.VertexNumber; i++)
            {
                posBuffer.SetPixel<Vector3>(i, in pPos[i]);
                if (pNor == (Vector3*)0)
                {
                    norBuffer.SetPixel<Vector3>(i, in pNor[i]);
                }
                else
                {
                    norBuffer.SetPixel<Vector3>(i, in Vector3.UnitY);
                }
                if (pNor == (Vector2*)0)
                {
                    uvBuffer.SetPixel<Vector2>(i, in pUV[i]);
                }
                else
                {
                    uvBuffer.SetPixel<Vector2>(i, in Vector2.One);
                }
            }

            return true;
        }
        internal System.Threading.Tasks.Task<Editor.USnapshot> Task;
        public unsafe override void OnPreviewDraw(in Vector2 prevStart, in Vector2 prevEnd, ImDrawList cmdlist)
        {
            if (MeshName == null)
                return;
            if (Task == null)
            {
                Task = Editor.USnapshot.Load(MeshName.Address + ".snap");
                return;
            }
            else if (Task.IsCompleted == true)
            {
                if (Task.Result == null)
                {
                    Task = null;
                }
                else
                {
                    cmdlist.AddImage(Task.Result.mTextureRSV.GetTextureHandle().ToPointer(), in prevStart, in prevEnd, in Vector2.Zero, in Vector2.One, 0xFFFFFFFF);
                }
            }
        }
    }

    [Bricks.CodeBuilder.ContextMenu("PreviewMesh", "Mesh\\Preview", UPgcGraph.PgcEditorKeyword)]
    public class UPreviewMesh : UPgcNodeBase
    {
        [EGui.Controls.PropertyGrid.PGCustomValueEditor(HideInPG = true)]
        public PinIn InIndices { get; set; } = new PinIn();
        [EGui.Controls.PropertyGrid.PGCustomValueEditor(HideInPG = true)]
        public PinIn InPos { get; set; } = new PinIn();
        [EGui.Controls.PropertyGrid.PGCustomValueEditor(HideInPG = true)]
        public PinIn InNor { get; set; } = new PinIn();
        [EGui.Controls.PropertyGrid.PGCustomValueEditor(HideInPG = true)]
        public PinOut IndicesPin { get; set; } = new PinOut();
        [EGui.Controls.PropertyGrid.PGCustomValueEditor(HideInPG = true)]
        public PinOut PosPin { get; set; } = new PinOut();
        [EGui.Controls.PropertyGrid.PGCustomValueEditor(HideInPG = true)]
        public PinOut NorPin { get; set; } = new PinOut();

        public UBufferCreator IndexBufferCreator { get; } = UBufferCreator.CreateInstance<USuperBuffer<Int32_3, FInt3Operator>>(-1, -1, -1);
        public UBufferCreator XYZBufferCreator { get; } = UBufferCreator.CreateInstance<USuperBuffer<Vector3, FFloat3Operator>>(-1, -1, -1);
        public UPreviewMesh()
        {
            PrevSize = new Vector2(100, 100);

            Icon.Size = new Vector2(25, 25);
            Icon.Color = 0xFF00FF00;
            TitleColor = 0xFF204020;
            BackColor = 0x80808080;

            AddInput(InIndices, "InIndices", IndexBufferCreator);
            AddInput(InPos, "InPos", XYZBufferCreator);
            AddInput(InNor, "InNor", XYZBufferCreator);

            AddOutput(IndicesPin, "Indices", IndexBufferCreator);
            AddOutput(PosPin, "Pos", XYZBufferCreator);
            AddOutput(NorPin, "Nor", XYZBufferCreator);
        }
        public override int PreviewResultIndex
        {
            get => mPreviewResultIndex;
            set
            {
                mPreviewResultIndex = -1;
            }
        }
        protected unsafe void CreatePreviewMesh(RName rn, UPgcGraph graph)
        {
            var meshBuilder = new Graphics.Mesh.UMeshDataProvider();
            {
                var indices = graph.BufferCache.FindBuffer(InIndices) as USuperBuffer<Int32_3, FInt3Operator>;
                var pos = graph.BufferCache.FindBuffer(InPos) as USuperBuffer<Vector3, FFloat3Operator>;
                var nor = graph.BufferCache.FindBuffer(InNor) as USuperBuffer<Vector3, FFloat3Operator>;

                var builder = meshBuilder.mCoreObject;
                uint streams = (uint)((1 << (int)NxRHI.EVertexStreamType.VST_Position) |
                (1 << (int)NxRHI.EVertexStreamType.VST_Normal));
                builder.Init((uint)streams, true, 1);

                var dpDesc = new NxRHI.FMeshAtomDesc();
                dpDesc.SetDefault();
                dpDesc.PrimitiveType = NxRHI.EPrimitiveType.EPT_TriangleList;
                dpDesc.NumPrimitives = 0;

                var aabb = BoundingBox.Empty;
                for (int i = 0; i < pos.Width; i++)
                {
                    ref var vPos = ref pos.GetPixel<Vector3>(i, 0, 0);
                    aabb.Merge(in vPos);
                    Vector3 vNor = Vector3.Up;
                    if (nor != null && nor.IsValidPixel(i, 0, 0))
                    {
                        vNor = nor.GetPixel<Vector3>(i, 0, 0);
                    }
                    builder.AddVertex(in vPos, in vNor, in Vector2.Zero, 0xFFFFFFFF);
                }
                builder.SetAABB(ref aabb);
                dpDesc.NumPrimitives = (uint)indices.Width;
                for (int i = 0; i < indices.Width; i++)
                {
                    var index = indices.GetPixel<Int32_3>(i, 0, 0);
                    builder.AddTriangle((uint)(index.X % pos.Width), (uint)(index.Y % pos.Width), (uint)(index.Z % pos.Width));
                    //builder.AddTriangle((uint)index.X, (uint)index.Y, (uint)index.Z);
                    //dpDesc.NumPrimitives++;
                }
                builder.PushAtomLOD(0, in dpDesc);
            }

            PreviewMesh = new Graphics.Mesh.UMaterialMesh();
            PreviewMesh.AssetName = rn;
            var meshPrimitve = meshBuilder.ToMesh();
            var matrials = new Graphics.Pipeline.Shader.UMaterialInstance[1];
            matrials[0] = UEngine.Instance.GfxDevice.MaterialInstanceManager.WireColorMateria;
            matrials[0].RenderLayer = Graphics.Pipeline.ERenderLayer.RL_Translucent;
            var rast = matrials[0].Rasterizer;
            rast.FillMode = NxRHI.EFillMode.FMD_WIREFRAME;
            matrials[0].Rasterizer = rast;
            PreviewMesh.Initialize(meshPrimitve, matrials);
        }
        public unsafe override void OnPreviewDraw(in Vector2 prevStart, in Vector2 prevEnd, ImDrawList cmdlist)
        {
            //var ctrlPos = ParentGraph.CanvasToViewport(in prevStart);
            var ctrlPos = prevStart;
            ctrlPos -= ImGuiAPI.GetWindowPos();
            ImGuiAPI.SetCursorPos(in ctrlPos);
            ImGuiAPI.PushID($"{this.NodeId.ToString()}");
            if (ImGuiAPI.Button("ShowMesh"))
            {
                var task = DoPreview();
            }
            ImGuiAPI.PopID();
            //if (PreviewSRV == null)
            //    return;

            //var uv0 = new Vector2(0, 0);
            //var uv1 = new Vector2(1, 1);
            //unsafe
            //{
            //    cmdlist.AddImage(PreviewSRV.GetTextureHandle().ToPointer(), in prevStart, in prevEnd, in uv0, in uv1, 0xFFFFFFFF);
            //}
        }
        public async System.Threading.Tasks.Task DoPreview()
        {
            //var mainEditor = UEngine.Instance.GfxDevice.MainWindow as Editor.UMainEditorApplication;
            //if (mainEditor != null)
            //{
            //    var rn = RName.GetRName(this.ParentGraph.GraphName, RName.ERNameType.Transient);
            //    if (PreviewMesh == null)
            //        return;

            //    var task = mainEditor.AssetEditorManager.OpenEditor(mainEditor, typeof(Editor.Forms.UMeshEditor), rn, PreviewMesh);
            //}

            var graph = this.ParentGraph as UPgcGraph;
            graph.Compile(this);

            graph.GraphEditor.PreviewRoot.ClearChildren();
            var viewport = graph.GraphEditor.PreviewViewport;

            var mesh = new Graphics.Mesh.UMesh();
            var ok = mesh.Initialize(PreviewMesh, Rtti.UTypeDescGetter<Graphics.Mesh.UMdfStaticMesh>.TypeDesc);
            if (ok)
            {
                var meshNode = await GamePlay.Scene.UMeshNode.AddMeshNode(viewport.World, graph.GraphEditor.PreviewRoot, new GamePlay.Scene.UMeshNode.UMeshNodeData(), typeof(GamePlay.UPlacement), mesh, DVector3.Zero, Vector3.One, Quaternion.Identity);
                meshNode.HitproxyType = Graphics.Pipeline.UHitProxy.EHitproxyType.Root;
                meshNode.NodeData.Name = "PreviewMesh";
                meshNode.IsAcceptShadow = false;
                meshNode.IsCastShadow = true;
            }
        }
        public override UBufferCreator GetOutBufferCreator(PinOut pin)
        {
            return null;
        }
        public override bool OnProcedure(UPgcGraph graph)
        {
            var indices = graph.BufferCache.FindBuffer(InIndices) as USuperBuffer<Int32_3, FInt3Operator>;
            var pos = graph.BufferCache.FindBuffer(InPos) as USuperBuffer<Vector3, FFloat3Operator>;
            var nor = graph.BufferCache.FindBuffer(InNor) as USuperBuffer<Vector3, FFloat3Operator>;

            if (indices != null)
            {
                graph.BufferCache.RegBuffer(PosPin, indices);
            }
            if (pos != null)
            {
                graph.BufferCache.RegBuffer(PosPin, pos);
            }
            if (nor != null)
            {
                graph.BufferCache.RegBuffer(NorPin, nor);
            }

            if (graph.GraphEditor != null)
            {
                if (PreviewSRV != null)
                {
                    PreviewSRV?.FreeTextureHandle();
                    PreviewSRV = null;
                }

                var rn = RName.GetRName(this.ParentGraph.GraphName, RName.ERNameType.Transient);
                CreatePreviewMesh(rn, graph);
                //System.Action action = async () =>
                //{
                //    var TriMesh = Graphics.Mesh.UMeshDataProvider.MakeCylinder(2.0f, 0.5f, 3.0f, 100, 100, 0xfffffff);
                //    PreviewMesh = new Graphics.Mesh.UMaterialMesh();
                //    var meshPrimitve = TriMesh.ToMesh();
                //    var matrials = new Graphics.Pipeline.Shader.UMaterialInstance[1];
                //    matrials[0] = UEngine.Instance.GfxDevice.MaterialInstanceManager.WireColorMateria;
                //    matrials[0].RenderLayer = Graphics.Pipeline.ERenderLayer.RL_Translucent;
                //    var rast = matrials[0].Rasterizer;
                //    rast.FillMode = EFillMode.FMD_WIREFRAME;
                //    matrials[0].Rasterizer = rast;
                //    PreviewMesh.Initialize(meshPrimitve, matrials);

                //    //PreviewMesh = new Graphics.Mesh.UMesh();
                //    //PreviewMesh.Initialize(ShowMesh, Rtti.UTypeDescGetter<Graphics.Mesh.UMdfStaticMesh>.TypeDesc);
                //    //if (ok)
                //    //{
                //    //    PreviewMeshNode = await GamePlay.Scene.UMeshNode.AddMeshNode(PreviewViewport.World, PreviewViewport.World.Root, 
                //    //        new GamePlay.Scene.UMeshNode.UMeshNodeData(), typeof(GamePlay.UPlacement), PreviewMesh, DVector3.Zero, Vector3.One, Quaternion.Identity);
                //    //    PreviewMeshNode.HitproxyType = Graphics.Pipeline.UHitProxy.EHitproxyType.Root;
                //    //    PreviewMeshNode.NodeData.Name = "PreviewObject";
                //    //    PreviewMeshNode.IsAcceptShadow = false;
                //    //    PreviewMeshNode.IsCastShadow = true;
                //    //}
                //};
                
                //action();
            }
            return true;
        }

        public Graphics.Mesh.UMaterialMesh PreviewMesh;
    }
}
