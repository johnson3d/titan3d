using EngineNS.Animation.Asset;
using EngineNS.Animation.SkeletonAnimation.Skeleton.Limb;
using EngineNS.Bricks.GpuDriven;
using EngineNS.GamePlay.Camera;
using EngineNS.Graphics.Mesh;
using EngineNS.Graphics.Pipeline;
using EngineNS.Graphics.Pipeline.Shader;
using EngineNS.NxRHI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

//using EngineNS.Graphics.Canvas;

namespace EngineNS.Editor.Forms
{
    public class TtDebugShowTool
    {
        bool mShowNormal = false;
        public bool ShowNormal
        {
            get {return mShowNormal;}
            set 
            {
                mShowNormal = value;
                if (value)
                    NormalNode?.UnsetStyle(GamePlay.Scene.TtNode.ENodeStyles.Invisible);
                else
                    NormalNode?.SetStyle(GamePlay.Scene.TtNode.ENodeStyles.Invisible);
            }
        }

        bool mShowTangent = false;
        public bool ShowTangent
        {
            get { return mShowTangent; }
            set
            {
                mShowTangent = value;
                if (value)
                    TangentNode?.UnsetStyle(GamePlay.Scene.TtNode.ENodeStyles.Invisible);
                else
                    TangentNode?.SetStyle(GamePlay.Scene.TtNode.ENodeStyles.Invisible);
            }
        }

        public EngineNS.GamePlay.Scene.TtMeshNode NormalNode;
        public Graphics.Mesh.TtRenderMesh NormalMesh;
        public EngineNS.GamePlay.Scene.TtMeshNode TangentNode;
        public Graphics.Mesh.TtRenderMesh TangentMesh;

        public async System.Threading.Tasks.Task Initialize(List<Graphics.Mesh.TtMeshPrimitives> MeshPrimitivesList, GamePlay.TtWorld world)
        {
            List<Vector3> PositionList = new List<Vector3>();
            List<Vector3> NormalList = new List<Vector3>();
            List<Vector3> TangentList = new List<Vector3>();
            unsafe
            {
                foreach( var Mesh in MeshPrimitivesList)
                {
                    TtMeshDataProvider meshProvider = new TtMeshDataProvider();
                    if (meshProvider.InitFrom(Mesh))
                    {
                        var builder = meshProvider.mCoreObject;
                        var pPos = (Vector3*)builder.GetStream(NxRHI.EVertexStreamType.VST_Position).GetData();
                        var pNor = (Vector3*)builder.GetStream(NxRHI.EVertexStreamType.VST_Normal).GetData();
                        var pTangent = (Vector4*)builder.GetStream(NxRHI.EVertexStreamType.VST_Tangent).GetData();

                        for (int i = 0; i < (int)builder.VertexNumber; i++)
                        {
                            if (pPos != null)
                            {
                                PositionList.Add(pPos[i]);
                                if (pNor != null)
                                {
                                    NormalList.Add(pPos[i]);
                                    NormalList.Add(pPos[i] + pNor[i] * 0.05f);
                                }
                                if (pTangent != null)
                                {
                                    TangentList.Add(pPos[i]);
                                    Vector3 tangent = new Vector3(pTangent[i].X, pTangent[i].Y, pTangent[i].Z);
                                    TangentList.Add(pPos[i] + tangent * 0.05f);
                                }
                            }
                        }
                    }
                }
            }

            var mtl = await RName.GetRName("material/line_color.material", RName.ERNameType.Engine).GetAsset<Graphics.Pipeline.Shader.TtMaterial>();// TtEngine.Instance.GfxDevice.MaterialManager.GetMaterial(RName.GetRName("material/line_color.material", RName.ERNameType.Engine));
            var materials = new Graphics.Pipeline.Shader.TtMaterial[1];
            materials[0] = mtl;

            if(NormalList.Count>0)
            {
                NormalMesh = new Graphics.Mesh.TtRenderMesh();
                var normalProvider = Graphics.Mesh.TtMeshDataProvider.MakeLines(in NormalList, 0xFF00FF00);
                NormalMesh.Initialize(normalProvider.ToMesh(), materials, Rtti.TtTypeDescGetter<Graphics.Mesh.TtMdfStaticMesh>.TypeDesc);
                NormalMesh.MdfQueue.MdfDatas = this;

                NormalNode = await GamePlay.Scene.TtMeshNode.AddMeshNode(world, world.Root, new GamePlay.Scene.TtMeshNode.TtMeshNodeData(), typeof(GamePlay.TtPlacement), NormalMesh, DVector3.Zero, Vector3.One, Quaternion.Identity);
                NormalNode.SetStyle(GamePlay.Scene.TtNode.ENodeStyles.Invisible);
                //NormalNode.SetStyle(GamePlay.Scene.UNode.ENodeStyles.VisibleFollowParent);
                NormalNode.NodeData.Name = "Debug_NormalNode";
                NormalNode.IsAcceptShadow = false;
                NormalNode.IsCastShadow = false;
                NormalNode.HitproxyType = Graphics.Pipeline.TtHitProxy.EHitproxyType.None;
            }

            if(TangentList.Count>0)
            {
                TangentMesh = new Graphics.Mesh.TtRenderMesh();
                var tangentProvider = Graphics.Mesh.TtMeshDataProvider.MakeLines(in TangentList, 0xFF0000FF);
                TangentMesh.Initialize(tangentProvider.ToMesh(), materials, Rtti.TtTypeDescGetter<Graphics.Mesh.TtMdfStaticMesh>.TypeDesc);
                TangentMesh.MdfQueue.MdfDatas = this;

                TangentNode = await GamePlay.Scene.TtMeshNode.AddMeshNode(world, world.Root, new GamePlay.Scene.TtMeshNode.TtMeshNodeData(), typeof(GamePlay.TtPlacement), TangentMesh, DVector3.Zero, Vector3.One, Quaternion.Identity);
                TangentNode.SetStyle(GamePlay.Scene.TtNode.ENodeStyles.Invisible);
                //TangentNode.SetStyle(GamePlay.Scene.UNode.ENodeStyles.VisibleFollowParent);
                TangentNode.NodeData.Name = "Debug_TangentMeshNode";
                TangentNode.IsAcceptShadow = false;
                TangentNode.IsCastShadow = false;
                TangentNode.HitproxyType = Graphics.Pipeline.TtHitProxy.EHitproxyType.None;
            }

        }
    }

    [IO.TtConfig(Path = "vms_editor.jscfg")]
    public class TtMeshPrimitiveEditorConfig : IO.IConfig
    {
        public TtMeshPrimitiveEditorConfig()
        {
            MaterialName = RName.GetRName("material/sysdft.material", RName.ERNameType.Engine);
            PlaneMaterialName = RName.GetRName("material/whitecolor.uminst", RName.ERNameType.Engine);
            ImportBaseMaterial = RName.GetRName("material/pbr.material", RName.ERNameType.Engine);
        }
        public RName MaterialName { get; set; }
        public RName PlaneMaterialName { get; set; }
        public RName ImportBaseMaterial { get; set; }

        // ─── Skeleton & PhysicsAsset Colors ──────────────────
        [Rtti.Meta("")]
        [System.ComponentModel.Category("Skeleton Colors")]
        public Color4b BoneSphereColor { get; set; } = Color4b.Green;

        [Rtti.Meta("")]
        [System.ComponentModel.Category("Skeleton Colors")]
        public Color4b BoneSphereHighlightColor { get; set; } = Color4b.Red;

        [Rtti.Meta("")]
        [System.ComponentModel.Category("Skeleton Colors")]
        public Color4b BoneLineColor { get; set; } = Color4b.Green;

        [Rtti.Meta("")]
        [System.ComponentModel.Category("PhysicsAsset Colors")]
        public Color4b PhysicsShapeColor { get; set; } = Color4b.FromArgb(0x40, Color4b.Green);

        [Rtti.Meta("")]
        [System.ComponentModel.Category("PhysicsAsset Colors")]
        public Color4b ConstraintConeColor { get; set; } = Color4b.Goldenrod;
    }
    public class TtMeshPrimitiveEditor : TtLightEnvironemnt, Editor.IAssetEditor, IRootForm, ISkeletonTreeHost
    {
        public int GetTickOrder()
        {
            return 0;
        }
        public RName AssetName { get; set; }
        protected bool mVisible = true;
        public bool Visible { get => mVisible; set => mVisible = value; }
        public uint DockId { get; set; }
        ImGuiWindowClass mDockKeyClass;
        public ImGuiWindowClass DockKeyClass => mDockKeyClass;
        public ImGuiCond_ DockCond { get; set; } = ImGuiCond_.ImGuiCond_FirstUseEver;

        //public Graphics.Canvas.TtCanvas TestCanvas { get; set; }

        public Graphics.Mesh.TtMeshPrimitives Mesh;
        public Editor.TtPreviewViewport PreviewViewport = new Editor.TtPreviewViewport();
        public EGui.Controls.PropertyGrid.TtPropertyGrid MeshPropGrid = new EGui.Controls.PropertyGrid.TtPropertyGrid();
        public EGui.Controls.PropertyGrid.TtPropertyGrid EditorPropGrid = new EGui.Controls.PropertyGrid.TtPropertyGrid();
        EngineNS.GamePlay.Scene.TtMeshNode mCurrentMeshNode;
        //EngineNS.GamePlay.Scene.TtMeshNode mArrowMeshNode;
        int mLastPickedProxyCount = 0;
        Graphics.Pipeline.IProxiable mLastPickedBoneOrShape = null;
        float mCurrentMeshRadius = 1.0f;
        public float PlaneScale = 5.0f;
        EngineNS.GamePlay.Scene.TtMeshNode PlaneMeshNode;
        [Category("Option")]
        public TtRenderPolicy RenderPolicy
        {
            get
            {
                if (PreviewViewport == null)
                    return null;
                return PreviewViewport.RenderPolicy;
            }
        }
        [Category("Option")]
        public bool IsCastShadow
        {
            get
            {
                if (mCurrentMeshNode == null)
                    return false;
                return mCurrentMeshNode.IsCastShadow;
            }
            set
            {
                if (mCurrentMeshNode == null)
                    return;
                mCurrentMeshNode.IsCastShadow = value;
            }
        }
        [Category("Option")]
        public bool IsAcceptShadow
        {
            get
            {
                if (mCurrentMeshNode == null)
                    return false;
                return mCurrentMeshNode.IsAcceptShadow;
            }
            set
            {
                if (mCurrentMeshNode == null)
                    return;
                mCurrentMeshNode.IsAcceptShadow = value;
            }
        }
        TtDebugShowTool DebugShowTool;
        TtSkeletonTreePanel SkeletonTreePanel = new TtSkeletonTreePanel();
        bool mShowSkeletonPanel = true;
        bool mShowNormal = false;
        bool mShowTangent = false;

        #region QuarkDAG Debug
        bool mShowQuarkDAGPanel = false;
        bool mQuarkDAGBuilt = false;
        TtPreviewViewport QuarkPreviewViewport;
        uint mQuarkClusterCount = 0;
        uint mQuarkMipLevels = 0;
        int mQuarkLODLevel = 0;
        int mQuarkLODLevelPrev = -1; // force initial build
        int mQuarkMaxGroupSize = 32;
        int mQuarkClusterSize = 128;
        bool mShowQuarkOverlay = true;
        uint mQuarkLODTriCount = 0;
        uint mQuarkLODClusterCount = 0;
        uint mQuarkOrigTriCount = 0;
        GamePlay.Scene.TtMeshNode mQuarkMeshNode;
        TtMaterial mQuarkMaterial;
        #endregion
        ~TtMeshPrimitiveEditor()
        {
            Dispose();
        }
        public void Dispose()
        {
            Mesh = null;
            MeshMaterial = null;
            CoreSDK.DisposeObject(ref QuarkPreviewViewport);
            CoreSDK.DisposeObject(ref PreviewViewport);
            MeshPropGrid.Target = null;
            EditorPropGrid.Target = null;
        }
        public async Thread.Async.TtTask<bool> Initialize()
        {
            await MeshPropGrid.Initialize();
            await EditorPropGrid.Initialize();
            return true;
        }
        public IRootForm GetRootForm()
        {
            return this;
        }
        public void SetMeshWireFrame(bool wireframe)
        {
            if (Mesh == null || MeshMaterial == null)
                return;
            var rast = MeshMaterial.Rasterizer;
            rast.FillMode = wireframe ? NxRHI.EFillMode.FMD_WIREFRAME : NxRHI.EFillMode.FMD_SOLID;
            MeshMaterial.Rasterizer = rast;
        }
        TtMaterial MeshMaterial = null;
        protected async Thread.Async.TtTask<bool> Initialize_PreviewMaterialInstance(Graphics.Pipeline.TtViewportSlate viewport, TtSlateApplication application, Graphics.Pipeline.TtRenderPolicy policy, float zMin, float zMax)
        {
            viewport.RenderPolicy = policy;

            await viewport.World.InitWorld();

            (viewport as Editor.TtPreviewViewport).CameraController.ControlCamera(viewport.RenderPolicy.DefaultCamera);

            var config = TtEngine.Instance.ConfigManager.GetConfig<Editor.Forms.TtMeshPrimitiveEditorConfig>();
            MeshMaterial = await TtEngine.Instance.GfxDevice.MaterialManager.CreateMaterial(config.MaterialName);
            var materials = new Graphics.Pipeline.Shader.TtMaterial[Mesh.mCoreObject.GetAtomNumber()];
            for (int i = 0; i < materials.Length; i++)
            {
                materials[i] = MeshMaterial;
            }
            var mesh = new Graphics.Mesh.TtRenderMesh();
            var meshNodeData = new GamePlay.Scene.TtMeshNode.TtMeshNodeData();
            if (Mesh.PartialSkeleton != null)
            {
                mesh.Initialize(Mesh, materials, Rtti.TtTypeDescGetter<Graphics.Mesh.TtMdfSkinMesh>.TypeDesc);
                meshNodeData.MdfQueueType = EngineNS.Rtti.TtTypeDesc.TypeStr(typeof(EngineNS.Graphics.Mesh.TtMdfSkinMesh));
            }
            else
            {
                mesh.Initialize(Mesh, materials, Rtti.TtTypeDescGetter<Graphics.Mesh.TtMdfStaticMesh>.TypeDesc);
                meshNodeData.MdfQueueType = EngineNS.Rtti.TtTypeDesc.TypeStr(typeof(EngineNS.Graphics.Mesh.TtMdfStaticMesh));
            }
            meshNodeData.MeshName = Mesh.AssetName;
            var meshNode = await GamePlay.Scene.TtMeshNode.AddMeshNode(viewport.World, viewport.World.Root, meshNodeData, typeof(GamePlay.TtPlacement), mesh,
                        DVector3.Zero, Vector3.One, Quaternion.Identity);
            meshNode.HitproxyType = Graphics.Pipeline.TtHitProxy.EHitproxyType.None;
            meshNode.NodeData.Name = "PreviewObject";
            meshNode.IsAcceptShadow = true;
            meshNode.IsCastShadow = true;

            mCurrentMeshNode = meshNode;

            if (Mesh.PartialSkeleton != null)
            {
                SkeletonTreePanel.SetSkeleton(Mesh.PartialSkeleton, this);
                SkeletonTreePanel.SetMeshAssetName(Mesh.AssetName, PreviewViewport.World);
            }

            DebugShowTool = new TtDebugShowTool();
            List<Graphics.Mesh.TtMeshPrimitives> MeshPrimitivesList = new List<Graphics.Mesh.TtMeshPrimitives>();
            MeshPrimitivesList.Add(Mesh);
            await DebugShowTool.Initialize(MeshPrimitivesList, PreviewViewport.World);

            var aabb = mesh.MaterialMesh.AABB;
            float radius = aabb.GetMaxSide();
            DBoundingSphere sphere;
            sphere.Center = aabb.GetCenter().AsDVector();
            sphere.Radius = mCurrentMeshRadius;
            policy.DefaultCamera.AutoZoom(in sphere);

            var planeMaterialName = TtEngine.Instance.ConfigManager.GetConfig<Editor.Forms.TtMeshPrimitiveEditorConfig>().PlaneMaterialName;
            var studioContext = await PreviewViewport.CreateStudioEnvironment(aabb, PlaneScale, planeMaterialName);
            PlaneMeshNode = studioContext?.FloorNode;

            await InitializeLightEnv(PreviewViewport, studioContext?.Radius ?? radius);

            return true;
        }

        public Graphics.Mesh.TtRenderMesh SdfDebugMesh;
        EngineNS.GamePlay.Scene.TtMeshNode SdfMeshNode;
        public void CalcVoxelsInBrick(Vector3i BrickCoordinate, BoundingBox DistanceFieldVolumeBounds,
            DistanceField.TtSparseSdfMip SdfData, DistanceField.DistanceFieldConfig SdfConfig,
            List<Byte> BrickVoxelSdfList, ref List<Vector3> OutVoxelPositions, ref List<Byte> OutVoxelDistance)
        {
            if (OutVoxelPositions == null || OutVoxelDistance == null)
                return;

            Vector3 IndirectionVoxelSize = DistanceFieldVolumeBounds.GetSize() / new Vector3(SdfData.IndirectionDimensions);
            Vector3 DistanceFieldVoxelSize = IndirectionVoxelSize / new Vector3(SdfConfig.UniqueDataBrickSize);
            Vector3 BrickMinPosition = DistanceFieldVolumeBounds.Minimum + new Vector3(BrickCoordinate) * IndirectionVoxelSize;

            for (int YIndex = 0; YIndex < SdfConfig.BrickSize; YIndex++)
            {
                for (int ZIndex = 0; ZIndex < SdfConfig.BrickSize; ZIndex++)
                {
                    for (int XIndex = 0; XIndex < SdfConfig.BrickSize; XIndex++)
                    {
                        Vector3 VoxelPosition = new Vector3(XIndex, YIndex, ZIndex) * DistanceFieldVoxelSize + BrickMinPosition;
                        int Index = (ZIndex * SdfConfig.BrickSize * SdfConfig.BrickSize + YIndex * SdfConfig.BrickSize + XIndex);

                        Byte QuantizedDistance = BrickVoxelSdfList[Index];
                        if (QuantizedDistance == 255)
                            continue;

                        OutVoxelPositions.Add(VoxelPosition);
                        OutVoxelDistance.Add(QuantizedDistance);
                    }
                }
            }
        }
        public async Thread.Async.TtTask CreateSdfDebugMesh(GamePlay.TtWorld world, DistanceField.TtSdfAsset sdfAsset)
        {
            if (sdfAsset == null || sdfAsset.Mips.Count < 0)
                return;

            if (SdfMeshNode == null)
            {
                var material = await RName.GetRName("material/sdfcolor.uminst", RName.ERNameType.Engine).CreateAsset<Graphics.Pipeline.Shader.TtMaterialInstance>();
                SdfDebugMesh = new Graphics.Mesh.TtRenderMesh();
                var rect = Graphics.Mesh.TtMeshDataProvider.MakeBox(-0.5f, -0.5f, -0.5f, 1, 1, 1, 0xffffffff);
                var rectMesh = rect.ToMesh();
                var materials = new Graphics.Pipeline.Shader.TtMaterial[1];
                materials[0] = material;
                SdfDebugMesh.Initialize(rectMesh, materials, Rtti.TtTypeDescGetter<Graphics.Mesh.TtMdfInstanceStaticMesh>.TypeDesc);
                SdfDebugMesh.MdfQueue.MdfDatas = this;

                var meshNode = await GamePlay.Scene.TtMeshNode.AddMeshNode(world, world.Root, new GamePlay.Scene.TtMeshNode.TtMeshNodeData(), typeof(GamePlay.TtPlacement), SdfDebugMesh, DVector3.Zero, Vector3.One, Quaternion.Identity);
                meshNode.SetStyle(GamePlay.Scene.TtNode.ENodeStyles.VisibleAlways);
                meshNode.NodeData.Name = "Debug_SdfMeshNode";
                meshNode.IsAcceptShadow = false;
                meshNode.IsCastShadow = false;
                meshNode.HitproxyType = Graphics.Pipeline.TtHitProxy.EHitproxyType.None;

                SdfMeshNode = meshNode;
                //SdfMeshNode.Parent = viewport.World.Root;
            }

            var sdfData = sdfAsset.Mips[0];

            DistanceField.DistanceFieldConfig sdfConfig = new DistanceField.DistanceFieldConfig();
            BoundingBox DistanceFieldVolumeBounds = sdfAsset.LocalSpaceMeshBounds;
            // Expand to guarantee one voxel border for gradient reconstruction using bilinear filtering
            if (sdfConfig.MeshDistanceFieldObjectBorder != 0)
            {
                Vector3 TexelObjectSpaceSize = sdfAsset.LocalSpaceMeshBounds.GetSize() / new Vector3(sdfData.IndirectionDimensions * sdfConfig.UniqueDataBrickSize - new Vector3i(2 * sdfConfig.MeshDistanceFieldObjectBorder));
                DistanceFieldVolumeBounds = BoundingBox.ExpandBy(sdfAsset.LocalSpaceMeshBounds, TexelObjectSpaceSize);
            }
            Vector3 IndirectionVoxelSize = DistanceFieldVolumeBounds.GetSize() / new Vector3(sdfData.IndirectionDimensions);
            Vector3 VoxelSize = IndirectionVoxelSize / new Vector3(sdfConfig.UniqueDataBrickSize);

            int VoxelCountInBrick = sdfConfig.BrickSize * sdfConfig.BrickSize * sdfConfig.BrickSize;
            var OutVoxelPositions = new List<Vector3>(VoxelCountInBrick);
            var OutVoxelDistance = new List<Byte>(VoxelCountInBrick);
            int validBrickIndex = 0;
            for (int YIndex = 0; YIndex < sdfData.IndirectionDimensions.Y; YIndex++)
            {
                for (int ZIndex = 0; ZIndex < sdfData.IndirectionDimensions.Z; ZIndex++)
                {
                    for (int XIndex = 0; XIndex < sdfData.IndirectionDimensions.X; XIndex++)
                    {
                        int IndirectionIndex = (ZIndex * sdfData.IndirectionDimensions.Y + YIndex) * sdfData.IndirectionDimensions.X + XIndex;
                        if (sdfData.IndirectionTable[IndirectionIndex] != sdfConfig.InvalidBrickIndex)
                        {
                            var BrickVoxelSdfList = sdfData.DistanceFieldBrickData.GetRange(validBrickIndex * VoxelCountInBrick, VoxelCountInBrick);
                            Vector3i BrickCoordinate = new Vector3i(XIndex, YIndex, ZIndex);

                            CalcVoxelsInBrick(BrickCoordinate, DistanceFieldVolumeBounds, sdfData, sdfConfig, BrickVoxelSdfList, ref OutVoxelPositions, ref OutVoxelDistance);

                            validBrickIndex++;
                            if (validBrickIndex == sdfData.NumDistanceFieldBricks)
                                break;
                        }
                    }
                }
            }

            var instanceMdf = SdfDebugMesh.MdfQueue as Graphics.Mesh.TtMdfInstanceStaticMesh;
            instanceMdf.InstanceModifier.InstanceBuffers.ResetInstance();
            instanceMdf.InstanceModifier.SetCapacity((uint)OutVoxelPositions.Count, false);
            for (int i = 0; i < OutVoxelPositions.Count; ++i)
            {
                var voxelPos = OutVoxelPositions[i];
                var QuantizedDistance = OutVoxelDistance[i];
                var instance = new Graphics.Pipeline.Shader.FVSInstanceData();
                instance.Position = mCurrentMeshNode.Location.ToSingleVector3() + voxelPos;
                instance.Scale = VoxelSize/**0.2f*/;
                instance.Quat = Quaternion.Identity;
                instance.UserData.X = (uint)Byte.MaxValue - (uint)QuantizedDistance;

                // decode to volume space distance
                float RescaledDistance = (float)QuantizedDistance / 255.0f;
                float VolumeSpaceDistance = RescaledDistance * sdfData.DistanceFieldToVolumeScaleBias.X + sdfData.DistanceFieldToVolumeScaleBias.Y;
                // encode 
                //float RescaledDistance = (VolumeSpaceDistance - DistanceFieldToVolumeScaleBias.Y) / DistanceFieldToVolumeScaleBias.X;
                //Byte QuantizedDistance = (Byte)Math.Clamp((int)Math.Floor(RescaledDistance * 255.0f + .5f), 0, 255);


                instanceMdf.InstanceModifier.PushInstance(in instance, new Graphics.Mesh.Modifier.FCullBounding());
            }
        }
        public float LoadingPercent { get; set; } = 1.0f;
        public string ProgressText { get; set; } = "Loading";
        public async Thread.Async.TtTask<bool> OpenEditor(TtMainEditorApplication mainEditor, RName name, object arg, bool saveLayout)
        {
            AssetName = name;
            Mesh = arg as Graphics.Mesh.TtMeshPrimitives;
            if (Mesh == null)
            {
                Mesh = await name.CreateAsset<Graphics.Mesh.TtMeshPrimitives>();
                if (Mesh == null)
                    return false;
                await Mesh.LoadMeshDataProvider();
            }

            PreviewViewport.PreviewAsset = AssetName;
            PreviewViewport.Title = $"Mesh:{name}";
            PreviewViewport.OnInitialize = Initialize_PreviewMaterialInstance;
            await PreviewViewport.Initialize(TtEngine.Instance.GfxDevice.SlateApplication, TtEngine.Instance.Config.MainRPolicyName, 0, 1);

            MeshPropGrid.Target = Mesh;
            EditorPropGrid.Target = this;
            TtEngine.Instance.TickableManager.AddTickable(this);

            return true;
        }
        public void OnCloseEditor()
        {
            TtEngine.Instance.TickableManager.RemoveTickable(this);
            Dispose();
        }

        #region DrawUI
        public Vector2 WindowPos;
        public Vector2 WindowSize = new Vector2(800, 600);
        public bool IsDrawing { get; set; }
        public unsafe void OnDraw()
        {
            if (Visible == false)
                return;

            var pivot = new Vector2(0);
            ImGuiAPI.SetNextWindowSize(in WindowSize, ImGuiCond_.ImGuiCond_FirstUseEver);
            IsDrawing = EGui.UIProxy.DockProxy.BeginMainForm(GetWindowsName(), this, ImGuiWindowFlags_.ImGuiWindowFlags_None);
            if (IsDrawing)
            {
                if (ImGuiAPI.IsWindowFocused(ImGuiFocusedFlags_.ImGuiFocusedFlags_RootAndChildWindows))
                {
                    var mainEditor = TtEngine.Instance.GfxDevice.SlateApplication as Editor.TtMainEditorApplication;
                    if (mainEditor != null)
                        mainEditor.AssetEditorManager.CurrentActiveEditor = this;
                }
                WindowPos = ImGuiAPI.GetWindowPos();
                WindowSize = ImGuiAPI.GetWindowSize();
                DrawToolBar();
                //var sz = new Vector2(-1);
                //ImGuiAPI.BeginChild("Client", ref sz, false, ImGuiWindowFlags_.)
                ImGuiAPI.Separator();
            }
            ResetDockspace();
            EGui.UIProxy.DockProxy.EndMainForm(IsDrawing);

            DrawPreview();
            DrawEditorDetails();
            DrawMeshDetails();
            DrawSkeleton();
            DrawQuarkDAG();
        }
        bool mDockInitialized = false;
        protected void ResetDockspace(bool force = false)
        {
            var pos = ImGuiAPI.GetCursorPos();
            var id = ImGuiAPI.GetID(AssetName.Name + "_Dockspace");
            mDockKeyClass.ClassId = id;
            ImGuiAPI.DockSpace(id, Vector2.Zero, ImGuiDockNodeFlags_.ImGuiDockNodeFlags_None, mDockKeyClass);
            if (mDockInitialized && !force)
                return;
            ImGuiAPI.DockBuilderRemoveNode(id);
            ImGuiAPI.DockBuilderAddNode(id, ImGuiDockNodeFlags_.ImGuiDockNodeFlags_None);
            ImGuiAPI.DockBuilderSetNodePos(id, pos);
            ImGuiAPI.DockBuilderSetNodeSize(id, Vector2.One);
            mDockInitialized = true;

            var rightId = id;
            uint middleId = 0;
            uint downId = 0;
            uint leftId = 0;
            uint rightUpId = 0;
            uint rightDownId = 0;
            ImGuiAPI.DockBuilderSplitNode(rightId, ImGuiDir.ImGuiDir_Left, 0.8f, ref middleId, ref rightId);
            ImGuiAPI.DockBuilderSplitNode(rightId, ImGuiDir.ImGuiDir_Down, 0.5f, ref rightDownId, ref rightUpId);
            ImGuiAPI.DockBuilderSplitNode(middleId, ImGuiDir.ImGuiDir_Down, 0.3f, ref downId, ref middleId);
            ImGuiAPI.DockBuilderSplitNode(middleId, ImGuiDir.ImGuiDir_Left, 0.2f, ref leftId, ref middleId);

            ImGuiAPI.DockBuilderDockWindow(EGui.UIProxy.DockProxy.GetDockWindowName("Skeleton", mDockKeyClass), leftId);
            ImGuiAPI.DockBuilderDockWindow(EGui.UIProxy.DockProxy.GetDockWindowName("Preview", mDockKeyClass), middleId);
            ImGuiAPI.DockBuilderDockWindow(EGui.UIProxy.DockProxy.GetDockWindowName("QuarkDAG", mDockKeyClass), middleId);
            ImGuiAPI.DockBuilderDockWindow(EGui.UIProxy.DockProxy.GetDockWindowName("EditorDetails", mDockKeyClass), rightUpId);
            ImGuiAPI.DockBuilderDockWindow(EGui.UIProxy.DockProxy.GetDockWindowName("MeshDetails", mDockKeyClass), rightUpId);
            ImGuiAPI.DockBuilderDockWindow(EGui.UIProxy.DockProxy.GetDockWindowName("BoneDetails", mDockKeyClass), rightUpId);

            ImGuiAPI.DockBuilderFinish(id);
        }
        protected unsafe void DrawToolBar()
        {
            var btSize = Vector2.Zero;
            if (EGui.UIProxy.CustomButton.ToolButton("Save", in btSize))
            {
                Mesh.SaveAssetTo(Mesh.AssetName);
                //var unused = TtEngine.Instance.GfxDevice.MaterialInstanceManager.ReloadMaterialInstance(Mesh.AssetName);

                //USnapshot.Save(Mesh.AssetName, Mesh.GetAMeta(), PreviewViewport.RenderPolicy.GetFinalShowRSV(), TtEngine.Instance.GfxDevice.RenderContext.mCoreObject.GetImmCommandList());
            }
            ImGuiAPI.SameLine(0, -1);
            if (EGui.UIProxy.CustomButton.ToolButton("Reload", in btSize))
            {

            }
            ImGuiAPI.SameLine(0, -1);
            if (EGui.UIProxy.CustomButton.ToolButton("Undo", in btSize))
            {
                
            }
            ImGuiAPI.SameLine(0, -1);
            if (EGui.UIProxy.CustomButton.ToolButton("Redo", in btSize))
            {
                
            }
            //ImGuiAPI.SameLine(0, -1);
            //if (EGui.UIProxy.CustomButton.ToolButton("BuildCluster", in btSize))
            //{
            //    var meshMeta = Mesh.GetAMeta() as EngineNS.Graphics.Mesh.TtMeshPrimitivesAMeta;
            //    meshMeta.IsClustered = true;
            //    meshMeta.AddReferenceAsset(RName.GetRName(Mesh.AssetName + ".clusteremesh", Mesh.AssetName.RNameType));
            //    meshMeta.SaveAMeta((IO.IAsset)null);
            //    Mesh.BuildClusteredMesh();
            //}
            //ImGuiAPI.SameLine(0, -1);
            //if (EGui.UIProxy.CustomButton.ToolButton("LoadCluster", in btSize))
            //{
            //    Mesh.LoadClusterMesh();
            //}
            ImGuiAPI.SameLine(0, -1);
            if (ImGuiAPI.ToggleButton("N", ref mShowNormal, in btSize, 0))
            {
                DebugShowTool.ShowNormal = mShowNormal;
            }
            ImGuiAPI.SameLine(0, -1);
            if (ImGuiAPI.ToggleButton("T", ref mShowTangent, in btSize, 0))
            {
                DebugShowTool.ShowTangent = mShowTangent;
            }
            ImGuiAPI.SameLine(0, -1);
            if (EGui.UIProxy.CustomButton.ToolButton("BuildMeshlets", in btSize))
            {
                Mesh.BuildMeshlets();
            }
            ImGuiAPI.SameLine(0, -1);
            if (EGui.UIProxy.CustomButton.ToolButton("BuildTangent", in btSize))
            {
                var mdp = new TtMeshDataProvider();
                mdp.InitFrom(Mesh);
                mdp.mCoreObject.BuildTangent();
                mdp.ToMesh(Mesh);
            }
            ImGuiAPI.SameLine(0, -1);
            if (EGui.UIProxy.CustomButton.ToolButton("BuildLightMap", in btSize))
            {
                var mdp = new TtMeshDataProvider();
                mdp.InitFrom(Mesh);
                float aspect = 1.0f;
                mdp.mCoreObject.BuildLightMap(ref aspect);
                mdp.ToMesh(Mesh);
            }
            ImGuiAPI.SameLine(0, -1);
            if (EGui.UIProxy.CustomButton.ToolButton("ApplyTransform", in btSize))
            {
                ApplyNodeTransformToMesh();
            }
            ImGuiAPI.SameLine(0, -1);
            if (EGui.UIProxy.CustomButton.ToolButton("ExportSkeleton", in btSize))
            {
                ExportSkeletonAsset();
            }
            ImGuiAPI.SameLine(0, -1);
            if (EGui.UIProxy.CustomButton.ToolButton("BuildTexelFactor", in btSize))
            {
                Mesh?.ComputeAndApplyTexelFactor();
            }
            ImGuiAPI.SameLine(0, -1);
            if (ImGuiAPI.Checkbox("Wireframe", ref mWireframe))
            {
                SetMeshWireFrame(mWireframe);
            }
            ImGuiAPI.SameLine(0, -1);
            if (EGui.UIProxy.CustomButton.ToolButton("BuildNaniteDAG", in btSize))
            {
                BuildNaniteDAG();
            }
            ImGuiAPI.SameLine(0, -1);
            if (ImGuiAPI.ToggleButton("DAG", ref mShowQuarkDAGPanel, in btSize, 0))
            {
            }
        }
        bool mWireframe = false;



        /// <summary>
        /// 将当前 meshNode 的 Placement 变换（位移/旋转/缩放）烘焙到顶点数据中，
        /// 重置节点变换为 Identity，然后保存 vms。
        /// </summary>
        unsafe void ApplyNodeTransformToMesh()
        {
            if (mCurrentMeshNode == null || Mesh == null)
                return;

            var placement = mCurrentMeshNode.Placement;
            var position = placement.Position;
            var rotation = placement.Quat;
            var scale = placement.Scale;

            var translationF = new Vector3((float)position.X, (float)position.Y, (float)position.Z);
            var transformMatrix = Matrix.Transformation(scale, rotation, translationF);

            // 法线/切线需要用逆转置矩阵来保证非均匀缩放时方向正确
            var noTranslation = Matrix.Transformation(scale, rotation, Vector3.Zero);
            Matrix.Invert(in noTranslation, out var invMat);
            Matrix.Transpose(in invMat, out var inverseTranspose);

            var meshProvider = new TtMeshDataProvider();
            if (!meshProvider.InitFrom(Mesh))
                return;

            var builder = meshProvider.mCoreObject;
            int vertexCount = (int)builder.VertexNumber;

            // Position
            var pPos = (Vector3*)builder.GetStream(NxRHI.EVertexStreamType.VST_Position).GetData();
            if (pPos != null)
            {
                for (int i = 0; i < vertexCount; i++)
                {
                    pPos[i] = Vector3.TransformCoordinate(in pPos[i], in transformMatrix);
                }
            }

            // Normal
            var pNor = (Vector3*)builder.GetStream(NxRHI.EVertexStreamType.VST_Normal).GetData();
            if (pNor != null)
            {
                for (int i = 0; i < vertexCount; i++)
                {
                    pNor[i] = Vector3.TransformNormal(in pNor[i], in inverseTranspose);
                    pNor[i].Normalize();
                }
            }

            // Tangent (Vector4 — w 分量保留手性符号)
            var pTan = (Vector4*)builder.GetStream(NxRHI.EVertexStreamType.VST_Tangent).GetData();
            if (pTan != null)
            {
                for (int i = 0; i < vertexCount; i++)
                {
                    var tangentDir = new Vector3(pTan[i].X, pTan[i].Y, pTan[i].Z);
                    tangentDir = Vector3.TransformNormal(in tangentDir, in inverseTranspose);
                    tangentDir.Normalize();
                    pTan[i] = new Vector4(tangentDir.X, tangentDir.Y, tangentDir.Z, pTan[i].W);
                }
            }

            meshProvider.ToMesh(Mesh);

            // 重置节点变换为 Identity
            placement.Position = DVector3.Zero;
            placement.Quat = Quaternion.Identity;
            placement.Scale = Vector3.One;

            // 重新对齐 Axis
            PreviewViewport.Axis?.SetSelectedNodes(mCurrentMeshNode);

            // 保存 vms
            Mesh.SaveAssetTo(Mesh.AssetName);
        }

        void ExportSkeletonAsset()
        {
            if (Mesh?.PartialSkeleton == null)
            {
                Profiler.Log.WriteLine<Profiler.TtLogCategory>(Profiler.ELogTag.Warning, "ExportSkeleton", "Mesh has no PartialSkeleton to export.");
                return;
            }

            var noExtName = Mesh.AssetName.NoExtName;
            var skeletonRName = RName.GetRName(noExtName + TtSkeletonAsset.AssetExt, Mesh.AssetName.RNameType);

            var skeletonAsset = new TtSkeletonAsset();
            skeletonAsset.Skeleton = Mesh.PartialSkeleton;

            var ameta = skeletonRName.AMeta;
            if (ameta == null)
            {
                ameta = new TtSkeletonAssetAMeta();
                ameta.SetAssetName(skeletonRName);
                ameta.AssetId = Guid.NewGuid();
                ameta.TypeStr = Rtti.TtTypeDesc.TypeOf(typeof(TtSkeletonAsset)).TypeString;
                ameta.Description = $"Exported from {Mesh.AssetName}";
                ameta.SaveAMeta((IO.IAsset)null);
                TtEngine.Instance.AssetMetaManager.RegAsset(ameta);
            }
            
            skeletonAsset.SaveAssetTo(skeletonRName);

            Profiler.Log.WriteLine<Profiler.TtLogCategory>(Profiler.ELogTag.Info, "ExportSkeleton", $"Skeleton saved to {skeletonRName}");
        }

        public void OnBoneSelected(ILimb selectedBone)
        {
        }

        public void OnShapeSelected(Graphics.Mesh.PhysicsAsset.TtCollisionShape shape, GamePlay.Scene.TtNode proxyNode)
        {
            if (shape == null || proxyNode == null)
            {
                // 清除 Axis 选中，不绑定任何节点
                PreviewViewport.Axis?.SetSelectedNodes((System.Collections.Generic.List<GamePlay.Scene.TtNode>)null);
                return;
            }
            // 清除旧选中，Axis 绑定到代理节点
            PreviewViewport.Axis?.SetSelectedNodes(new System.Collections.Generic.List<GamePlay.Scene.TtNode> { proxyNode });
        }

        bool mShowBoneDetails = true;
        protected void DrawSkeleton()
        {
            if (!SkeletonTreePanel.HasSkeleton)
                return;
            var show = EGui.UIProxy.DockProxy.BeginPanel(mDockKeyClass, "Skeleton", ref mShowSkeletonPanel, ImGuiWindowFlags_.ImGuiWindowFlags_None);
            if (show)
            {
                SkeletonTreePanel.OnDrawTree();
            }
            EGui.UIProxy.DockProxy.EndPanel(show);

            var showDetails = EGui.UIProxy.DockProxy.BeginPanel(mDockKeyClass, "BoneDetails", ref mShowBoneDetails, ImGuiWindowFlags_.ImGuiWindowFlags_None);
            if (showDetails)
            {
                SkeletonTreePanel.OnDrawBoneDetails();
            }
            EGui.UIProxy.DockProxy.EndPanel(showDetails);
        }

        bool ShowEditorPropGrid = true;
        [Category("Editing")]
        public Color4b MeshColor { get; set; } = Color4b.FromArgb(255, 255, 255, 255);
        protected void DrawEditorDetails()
        {
            var sz = new Vector2(-1);
            var show = EGui.UIProxy.DockProxy.BeginPanel(mDockKeyClass, "EditorDetails", ref ShowEditorPropGrid, ImGuiWindowFlags_.ImGuiWindowFlags_None);
            if (show)
            {
                if (ImGuiAPI.Button("SetMeshColor"))
                {
                    TtEngine.Instance.EventPoster.RunOn((state) =>
                    {
                        //TtEngine.Instance.StopOperation($"{Mesh.AssetName}:SetMeshColor");
                        var vb = Mesh.MeshDataProvider.CreateStream(NxRHI.EVertexStreamType.VST_Color);
                        unsafe
                        {
                            Color4b* pVertices = (Color4b*)vb.GetData();
                            for (uint i = 0; i < this.Mesh.MeshDataProvider.NumOfVertex; i++)
                            {
                                pVertices[i] = MeshColor;
                            }
                        }
                        Mesh.MeshDataProvider.ToMesh(Mesh);
                        Mesh.SaveAssetTo(Mesh.AssetName);
                        //TtEngine.Instance.ResumeOperation();
                        return true;
                    }, Thread.Async.EAsyncTarget.Main);
                }
                EditorPropGrid.OnDraw(true, false, false);
            }
            EGui.UIProxy.DockProxy.EndPanel(show);
        }
        DistanceField.TtSdfAsset MeshSdfAsset = new DistanceField.TtSdfAsset();
        bool ShowMeshPropGrid = true;
        protected void DrawMeshDetails()
        {
            var sz = new Vector2(-1);
            var show = EGui.UIProxy.DockProxy.BeginPanel(mDockKeyClass, "MeshDetails", ref ShowMeshPropGrid, ImGuiWindowFlags_.ImGuiWindowFlags_None);
            if (show)
            {
                if (ImGuiAPI.Button("Build SDF"))
                {
                    var meshProvider = new TtMeshDataProvider();
                    if (meshProvider.InitFrom(Mesh))
                    {
                        var sdfConfig = new DistanceField.DistanceFieldConfig();
                        var outSDF = MeshSdfAsset;
                        DistanceField.UMeshUtilities.GenerateSignedDistanceFieldVolumeData(Mesh.AssetName.ToString(), meshProvider, sdfConfig, 1.0f, false, ref outSDF);

                        //var noExtName = Mesh.AssetName.Name.Substring(0, Mesh.AssetName.Name.Length - Mesh.AssetName.ExtName.Length);
                        var noExtName = Mesh.AssetName.NoExtName;
                        var rn = RName.GetRName(noExtName + DistanceField.TtSdfAsset.AssetExt, Mesh.AssetName.RNameType);
                        var ameta = new DistanceField.TtSdfAssetAMeta();
                        ameta.SetAssetName(rn);
                        ameta.AssetId = Guid.NewGuid();
                        ameta.TypeStr = Rtti.TtTypeDesc.TypeOf(typeof(DistanceField.TtSdfAsset)).TypeString;
                        ameta.Description = $"This is a {typeof(DistanceField.TtSdfAssetAMeta).FullName}\n";
                        ameta.SaveAMeta((IO.IAsset)null);
                        TtEngine.Instance.AssetMetaManager.RegAsset(ameta);

                        outSDF.SaveAssetTo(rn);

                        CreateSdfDebugMesh(PreviewViewport.World, outSDF).WaitCompletedAndDispose();

                        // test load sdf
                        Action action = async () =>
                        {
                            //await CreateSdfDebugMesh(PreviewViewport.World, outSDF);
                            var testSDF = await rn.GetAsset<DistanceField.TtSdfAsset>();
                        };
                        action();

                    }
                }
                MeshPropGrid.OnDraw(true, false, false, ImGuiWindowFlags_.ImGuiWindowFlags_NoScrollbar, 
                    ImGuiChildFlags_.ImGuiChildFlags_AlwaysAutoResize | ImGuiChildFlags_.ImGuiChildFlags_AutoResizeX | ImGuiChildFlags_.ImGuiChildFlags_AutoResizeY);
            }
            EGui.UIProxy.DockProxy.EndPanel(show);
        }
        bool ShowPreview = true;
        protected unsafe void DrawPreview()
        {
            var show = EGui.UIProxy.DockProxy.BeginPanel(mDockKeyClass, "Preview", ref ShowPreview, ImGuiWindowFlags_.ImGuiWindowFlags_None);
            if (show)
            {
                PreviewViewport.ViewportType = Graphics.Pipeline.TtViewportSlate.EViewportType.ChildWindow;
                PreviewViewport.OnDraw();
            }
            this.PreviewViewport.Visible = show;
            EGui.UIProxy.DockProxy.EndPanel(show);
        }

        #region QuarkDAG Debug Methods

        unsafe void BuildNaniteDAG()
        {
            if (Mesh == null || !Mesh.mCoreObject.IsValidPointer)
                return;

            var rc = TtEngine.Instance.GfxDevice.RenderContext;
            Mesh.mCoreObject.BuildNaniteDAGEx(rc.mCoreObject, (uint)mQuarkMaxGroupSize, (uint)mQuarkClusterSize);

            uint clusterCount = Mesh.mCoreObject.GetClusterCount();
            if (clusterCount == 0)
            {
                Profiler.Log.WriteLine<Profiler.TtGraphicsGategory>(Profiler.ELogTag.Warning, "BuildNaniteDAG produced 0 clusters");
                return;
            }

            Profiler.Log.WriteLine<Profiler.TtGraphicsGategory>(Profiler.ELogTag.Info,
                $"BuildNaniteDAG: {clusterCount} clusters, {Mesh.mCoreObject.GetDAGMipLevels()} mip levels");

            // Initialize visualization node
            InitQuarkVisNode();
            mQuarkDAGBuilt = true;
            mShowQuarkDAGPanel = true;
        }

        async void InitQuarkVisNode()
        {
            // Create a dedicated PreviewViewport for QuarkDAG visualization using standard deferred pipeline
            if (QuarkPreviewViewport == null)
            {
                var policyRName = TtEngine.Instance.Config.MainRPolicyName;
                QuarkPreviewViewport = new TtPreviewViewport();
                QuarkPreviewViewport.Title = "QuarkDAGViewport";
                QuarkPreviewViewport.PreviewAsset = policyRName;
                QuarkPreviewViewport.OnInitialize = Initialize_QuarkDAGHardwareRaster;
                QuarkPreviewViewport.OnDrawViewportUIAction = DrawQuarkStatsOverlay;
                await QuarkPreviewViewport.Initialize(TtEngine.Instance.GfxDevice.SlateApplication,
                    policyRName, 0.01f, 1000.0f);
            }

            mQuarkClusterCount = Mesh.mCoreObject.GetClusterCount();
            mQuarkMipLevels = Mesh.mCoreObject.GetDAGMipLevels();

            unsafe
            {
                // Store original mesh triangle count
                var atom = Mesh.mCoreObject.GetAtom(0, 0);
                mQuarkOrigTriCount = (atom != null) ? atom->NumPrimitives : 0;
                mQuarkLODLevel = 0;
                mQuarkLODLevelPrev = 0;
            }
        }

        /// <summary>
        /// Build a standard mesh from cluster VB/IB data and render with hardware rasterization.
        /// This verifies that the cluster data itself is correct before debugging software rasterizer.
        /// </summary>
        async Thread.Async.TtTask<bool> Initialize_QuarkDAGHardwareRaster(
            TtViewportSlate viewport, TtSlateApplication application,
            TtRenderPolicy policy, float zMin, float zMax)
        {
            viewport.RenderPolicy = policy;
            (viewport as TtPreviewViewport).CameraController.ControlCamera(policy.DefaultCamera);

            // Build mesh from cluster data (unsafe operations in separate method)
            var meshPrim = BuildClusterMeshFromDAG(mQuarkLODLevel);
            if (meshPrim == null)
                return false;

            // Load material and cache it for later LOD switches
            var config = TtEngine.Instance.ConfigManager.GetConfig<TtMeshPrimitiveEditorConfig>();
            mQuarkMaterial = await TtEngine.Instance.GfxDevice.MaterialManager.CreateMaterial(config.MaterialName);
            var materials = new TtMaterial[1];
            materials[0] = mQuarkMaterial;

            var renderMesh = new TtRenderMesh();
            renderMesh.Initialize(meshPrim, materials, Rtti.TtTypeDescGetter<TtMdfStaticMesh>.TypeDesc);

            // Add mesh node to viewport world
            mQuarkMeshNode = await GamePlay.Scene.TtMeshNode.AddMeshNode(
                viewport.World, viewport.World.Root,
                new GamePlay.Scene.TtMeshNode.TtMeshNodeData(),
                typeof(GamePlay.TtPlacement), renderMesh,
                DVector3.Zero, Vector3.One, Quaternion.Identity);
            mQuarkMeshNode.NodeData.Name = "ClusterMesh";
            mQuarkMeshNode.IsCastShadow = true;

            // Auto-zoom camera
            var aabb = Mesh.mCoreObject.mAABB;
            var daabb = new DBoundingBox(in aabb);
            policy.DefaultCamera.AutoZoom(in daabb, 0.0f, true);

            return true;
        }

        /// <summary>
        /// Rebuild the cluster mesh for a different LOD level and swap it in the viewport.
        /// </summary>
        async void RebuildQuarkLODMesh(int newLevel)
        {
            if (QuarkPreviewViewport == null || !QuarkPreviewViewport.IsInlitialized)
                return;
            if (mQuarkMaterial == null)
                return;

            var meshPrim = BuildClusterMeshFromDAG(newLevel);
            if (meshPrim == null)
                return;

            // Update LOD stats for overlay
            mQuarkLODTriCount = 0;
            mQuarkLODClusterCount = 0;
            var coreObj = Mesh.mCoreObject;
            uint cc = coreObj.GetClusterCount();
            for (uint i = 0; i < cc; i++)
            {
                if (newLevel < 0 || coreObj.GetClusterMipLevel((int)i) == newLevel)
                {
                    mQuarkLODClusterCount++;
                    var c = coreObj.GetCluster((int)i);
                    mQuarkLODTriCount += (uint)c.IndexCount / 3;
                }
            }

            // Remove old mesh node
            if (mQuarkMeshNode != null)
            {
                mQuarkMeshNode.Parent = null;
            }

            // Create new render mesh
            var materials = new TtMaterial[1];
            materials[0] = mQuarkMaterial;
            var renderMesh = new TtRenderMesh();
            renderMesh.Initialize(meshPrim, materials, Rtti.TtTypeDescGetter<TtMdfStaticMesh>.TypeDesc);

            // Add new mesh node
            var viewport = QuarkPreviewViewport;
            mQuarkMeshNode = await GamePlay.Scene.TtMeshNode.AddMeshNode(
                viewport.World, viewport.World.Root,
                new GamePlay.Scene.TtMeshNode.TtMeshNodeData(),
                typeof(GamePlay.TtPlacement), renderMesh,
                DVector3.Zero, Vector3.One, Quaternion.Identity);
            mQuarkMeshNode.NodeData.Name = "ClusterMesh";
            mQuarkMeshNode.IsCastShadow = true;
        }

        /// <summary>
        /// Unsafe helper: builds TtMeshPrimitives from cluster VB/IB data with per-cluster vertex colors.
        /// mipLevel: which LOD level to render (-1 = all levels)
        /// </summary>
        unsafe TtMeshPrimitives BuildClusterMeshFromDAG(int mipLevel)
        {
            var coreObj = Mesh.mCoreObject;
            uint clusterCount = coreObj.GetClusterCount();
            uint vbCount = coreObj.GetClustersVBCount();
            uint ibCount = coreObj.GetClustersIBCount();

            if (clusterCount == 0 || vbCount == 0 || ibCount == 0)
                return null;

            var vbPtr = coreObj.GetClustersVB(); // v3dxVector3*
            var ibPtr = coreObj.GetClustersIB(); // uint*

            // Collect clusters at the target mip level
            var activeClusters = new System.Collections.Generic.List<uint>();
            for (uint i = 0; i < clusterCount; i++)
            {
                if (mipLevel < 0 || coreObj.GetClusterMipLevel((int)i) == mipLevel)
                    activeClusters.Add(i);
            }
            if (activeClusters.Count == 0)
                return null;

            // Create MeshDataProvider with Position + Normal + Color + UV, 32-bit indices
            var meshBuilder = new TtMeshDataProvider();
            meshBuilder.AssetName = RName.GetRName("@QuarkDAGClusterMesh", RName.ERNameType.Transient);
            var builder = meshBuilder.mCoreObject;
            uint streams = (uint)((1 << (int)EVertexStreamType.VST_Position) |
                (1 << (int)EVertexStreamType.VST_Normal) |
                (1 << (int)EVertexStreamType.VST_Color) |
                (1 << (int)EVertexStreamType.VST_UV));
            builder.Init(streams, true, 1); // isIndex32 = true

            var aabb = coreObj.mAABB;
            builder.SetAABB(ref aabb);

            // For each active cluster: add its vertices (with cluster color) and triangles
            // We need to remap global vertex indices to our local builder indices
            var dummyNor = Vector3.UnitY;
            var dummyUV = Vector2.Zero;
            uint localVertBase = 0;
            uint totalTriangles = 0;

            for (int ci = 0; ci < activeClusters.Count; ci++)
            {
                uint clusterId = activeClusters[ci];
                var cluster = coreObj.GetCluster((int)clusterId);
                uint color = ClusterIDToColor(clusterId);

                int vStart = cluster.VertexStart;
                int vCount = cluster.VertexCount;
                int iStart = cluster.IndexStart;
                int iCount = cluster.IndexCount;

                // Add vertices for this cluster
                for (int v = 0; v < vCount; v++)
                {
                    int globalV = vStart + v;
                    var pos = new Vector3(vbPtr[globalV].X, vbPtr[globalV].Y, vbPtr[globalV].Z);
                    builder.AddVertex(in pos, in dummyNor, in dummyUV, color);
                }

                // Add triangles, remapping from global IB index to local vertex index
                uint triCount = (uint)iCount / 3;
                for (uint t = 0; t < triCount; t++)
                {
                    // Global indices in mClustersIB already point to global VB positions
                    // We need to remap: globalIndex - cluster.VertexStart = local offset within this cluster
                    uint i0 = ibPtr[iStart + t * 3 + 0] - (uint)vStart + localVertBase;
                    uint i1 = ibPtr[iStart + t * 3 + 1] - (uint)vStart + localVertBase;
                    uint i2 = ibPtr[iStart + t * 3 + 2] - (uint)vStart + localVertBase;
                    builder.AddTriangle(i0, i1, i2);
                }

                localVertBase += (uint)vCount;
                totalTriangles += triCount;
            }

            if (totalTriangles == 0)
                return null;

            // Push atom description
            var dpDesc = new FMeshAtomDesc();
            dpDesc.SetDefault();
            dpDesc.NumPrimitives = totalTriangles;
            builder.PushAtomLOD(0, &dpDesc);

            return meshBuilder.ToMesh();
        }

        static uint ClusterIDToColor(uint id)
        {
            // Generate distinct colors per cluster using a hash
            uint h = id * 2654435761u; // Knuth multiplicative hash
            byte r = (byte)((h >> 0) & 0xFF);
            byte g = (byte)((h >> 8) & 0xFF);
            byte b = (byte)((h >> 16) & 0xFF);
            // Ensure minimum brightness
            r = (byte)Math.Max(r, (byte)60);
            g = (byte)Math.Max(g, (byte)60);
            b = (byte)Math.Max(b, (byte)60);
            return (uint)(0xFF000000 | (b << 16) | (g << 8) | r); // ABGR format
        }

        bool mShowQuarkDAG = true;
        protected unsafe void DrawQuarkDAG()
        {
            if (!mShowQuarkDAGPanel)
                return;

            var show = EGui.UIProxy.DockProxy.BeginPanel(mDockKeyClass, "QuarkDAG", ref mShowQuarkDAG, ImGuiWindowFlags_.ImGuiWindowFlags_None);
            if (show)
            {
                if (!mQuarkDAGBuilt)
                {
                    ImGuiAPI.TextColored(new Vector4(1, 1, 0, 1), "Click [BuildNaniteDAG] to generate DAG data");
                }
                else
                {
                    ImGuiAPI.Text($"Clusters: {mQuarkClusterCount} | MipLevels: {mQuarkMipLevels}");

                    // MaxGroupSize input + ClusterSize input + Rebuild button
                    ImGuiAPI.SetNextItemWidth(160);
                    ImGuiAPI.InputInt("MaxGroupSize", ref mQuarkMaxGroupSize, 4, 8, ImGuiInputTextFlags_.ImGuiInputTextFlags_None);
                    if (mQuarkMaxGroupSize < 4) mQuarkMaxGroupSize = 4;
                    ImGuiAPI.SameLine(0, 10);
                    ImGuiAPI.SetNextItemWidth(160);
                    ImGuiAPI.InputInt("ClusterSize", ref mQuarkClusterSize, 16, 32, ImGuiInputTextFlags_.ImGuiInputTextFlags_None);
                    if (mQuarkClusterSize < 32) mQuarkClusterSize = 32;
                    if (mQuarkClusterSize > 256) mQuarkClusterSize = 256;
                    ImGuiAPI.SameLine(0, 10);
                    if (ImGuiAPI.Button("Rebuild DAG", in Vector2.Zero))
                    {
                        // Rebuild with new MaxGroupSize
                        BuildNaniteDAG();
                        mQuarkLODLevel = 0;
                        mQuarkLODLevelPrev = -1;
                    }
                    ImGuiAPI.SameLine(0, 20);
                    ImGuiAPI.Checkbox("Show Stats", ref mShowQuarkOverlay);

                    // LOD Level slider
                    if (mQuarkMipLevels > 1)
                    {
                        int maxLevel = (int)mQuarkMipLevels - 1;
                        if (ImGuiAPI.SliderInt("LOD Level", ref mQuarkLODLevel, 0, maxLevel, "%d", ImGuiSliderFlags_.ImGuiSliderFlags_None))
                        {
                            // Slider value changed
                        }

                        // Detect change and rebuild mesh
                        if (mQuarkLODLevel != mQuarkLODLevelPrev)
                        {
                            mQuarkLODLevelPrev = mQuarkLODLevel;
                            RebuildQuarkLODMesh(mQuarkLODLevel);
                        }
                    }

                    // --- Viewport (standard deferred pipeline handles everything) ---
                    if (QuarkPreviewViewport != null && QuarkPreviewViewport.IsInlitialized)
                    {
                        QuarkPreviewViewport.ViewportType = TtViewportSlate.EViewportType.ChildWindow;
                        QuarkPreviewViewport.OnDraw();
                    }
                }
            }
            if (QuarkPreviewViewport != null && QuarkPreviewViewport.IsInlitialized)
                QuarkPreviewViewport.Visible = show;
            EGui.UIProxy.DockProxy.EndPanel(show);
        }

        void TickQuarkDAGVisualize()
        {
            // Standard pipeline handles camera/lighting automatically, nothing to do here
        }

        Vector2 DrawQuarkStatsOverlay(in Vector2 startDrawPos)
        {
            if (!mShowQuarkOverlay)
                return Vector2.Zero;

            var cmdlst = ImGuiAPI.GetWindowDrawList();
            var textPos = new Vector2(startDrawPos.X + 4, startDrawPos.Y + 4);
            float lineH = 16;
            uint textColor = 0xFF00FFFF; // Yellow (ABGR)

            cmdlst.AddText(in textPos, textColor, $"OrigTris: {mQuarkOrigTriCount}", null);
            textPos.Y += lineH;
            cmdlst.AddText(in textPos, textColor, $"ClusterSize: {mQuarkClusterSize}", null);
            textPos.Y += lineH;
            cmdlst.AddText(in textPos, textColor, $"Clusters(total): {mQuarkClusterCount}", null);
            textPos.Y += lineH;
            cmdlst.AddText(in textPos, textColor, $"MipLevels: {mQuarkMipLevels}", null);
            textPos.Y += lineH;
            cmdlst.AddText(in textPos, textColor, $"LOD {mQuarkLODLevel}: {mQuarkLODClusterCount} clusters, {mQuarkLODTriCount} tris", null);
            textPos.Y += lineH;

            return new Vector2(200, textPos.Y - startDrawPos.Y);
        }

        #endregion
        #endregion
        public void OnEvent(in Bricks.Input.Event e)
        {
            //throw new NotImplementedException();
        }
        #region Tickable
        public override void TickLogic(float ellapse)
        {
            PreviewViewport.TickLogic(ellapse);
            if (QuarkPreviewViewport?.IsInlitialized == true)
                QuarkPreviewViewport.TickLogic(ellapse);

            TickQuarkDAGVisualize();

            // ESC 取消 Shape 选中
            if (SkeletonTreePanel.SelectedShape != null
                && TtEngine.Instance.InputSystem.IsKeyDown(Bricks.Input.Keycode.KEY_ESCAPE))
            {
                SkeletonTreePanel.SelectShape(null);
            }

            // Delete 删除选中的 Shape
            if (SkeletonTreePanel.SelectedShape != null
                && TtEngine.Instance.InputSystem.IsKeyPressed(Bricks.Input.Keycode.KEY_DELETE))
            {
                // 先从 PickedProxiableManager 中移除，清除描边效果
                var rp = PreviewViewport.RenderPolicy as Graphics.Pipeline.TtRenderPolicy;
                if (rp != null)
                    rp.PickedProxiableManager.Unselected(SkeletonTreePanel.SelectedShape);
                SkeletonTreePanel.DeleteSelectedShape();
            }

            // 检测 HitProxy 选中/取消 Shape 或骨骼（强制单选）
            var policy = PreviewViewport.RenderPolicy as Graphics.Pipeline.TtRenderPolicy;
            if (policy != null)
            {
                // 在 PickedProxies 中查找最新的 bone/shape（取最后一个，即最新点击的）
                Graphics.Pipeline.IProxiable latestPicked = null;
                foreach (var proxy in policy.PickedProxiableManager.PickedProxies)
                {
                    if (proxy is TtBoneHitProxy || proxy is Graphics.Mesh.PhysicsAsset.TtCollisionShape)
                        latestPicked = proxy;
                }

                // 强制单选：清除 PickedProxies 中除 latestPicked 外的所有 bone/shape
                for (int i = policy.PickedProxiableManager.PickedProxies.Count - 1; i >= 0; i--)
                {
                    var p = policy.PickedProxiableManager.PickedProxies[i];
                    if (p == latestPicked)
                        continue;
                    if (p is TtBoneHitProxy || p is Graphics.Mesh.PhysicsAsset.TtCollisionShape)
                        policy.PickedProxiableManager.Unselected(p);
                }

                // 选中对象发生变化时处理
                if (latestPicked != mLastPickedBoneOrShape)
                {
                    mLastPickedBoneOrShape = latestPicked;

                    if (latestPicked is TtBoneHitProxy pickedBone)
                    {
                        SkeletonTreePanel.TryHandleHitProxy(pickedBone);
                    }
                    else if (latestPicked is Graphics.Mesh.PhysicsAsset.TtCollisionShape pickedShape)
                    {
                        SkeletonTreePanel.SelectShape(pickedShape);
                    }
                    else
                    {
                        // 无选中 — 如有需要可清除
                    }
                }
            }

            SkeletonTreePanel.TickShapeProxy();
        }
        public override void TickRender(float ellapse)
        {
            PreviewViewport.TickRender(ellapse);

            if (IsDrawing == false)
                return;

            base.TickRender(ellapse);
        }
        public override void TickBeginFrame(float ellapse)
        {

        }
        public override void TickSync(float ellapse)
        {
            PreviewViewport.TickSync(ellapse);
            if (QuarkPreviewViewport?.IsInlitialized == true)
                QuarkPreviewViewport.TickSync(ellapse);
        }

        public string GetWindowsName()
        {
            return AssetName.Name;
        }
        #endregion
    }
}

namespace EngineNS.Graphics.Mesh
{
    [Editor.TtAssetEditor(EditorType = typeof(Editor.Forms.TtMeshPrimitiveEditor))]
    public partial class TtMeshPrimitives
    {
    }
}
