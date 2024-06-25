using EngineNS.DistanceField;
using EngineNS.GamePlay.Camera;
using EngineNS.Graphics.Mesh;
using NPOI.OpenXmlFormats.Dml;
using System;
using System.Collections.Generic;
using System.Text;

//using EngineNS.Graphics.Canvas;

namespace EngineNS
{
    public partial class UEngineConfig
    {
        [Rtti.Meta]
        public Editor.Forms.UMeshPrimitiveEditorConfig MeshPrimitiveEditorConfig
        {
            get; set;
        } = new Editor.Forms.UMeshPrimitiveEditorConfig();
    }
}

namespace EngineNS.Editor.Forms
{
    public class UMeshPrimitiveEditorConfig : IO.BaseSerializer
    {
        public UMeshPrimitiveEditorConfig()
        {
            MaterialName = RName.GetRName("material/sysdft.material", RName.ERNameType.Engine);
            PlaneMaterialName = RName.GetRName("material/whitecolor.uminst", RName.ERNameType.Engine);
        }
        public RName MaterialName { get; set; }
        public RName PlaneMaterialName { get; set; }
    }
    public class UMeshPrimitiveEditor : Editor.IAssetEditor, ITickable, IRootForm
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

        public Graphics.Mesh.UMeshPrimitives Mesh;
        public Editor.UPreviewViewport PreviewViewport = new Editor.UPreviewViewport();
        public EGui.Controls.PropertyGrid.PropertyGrid MeshPropGrid = new EGui.Controls.PropertyGrid.PropertyGrid();
        public EGui.Controls.PropertyGrid.PropertyGrid EditorPropGrid = new EGui.Controls.PropertyGrid.PropertyGrid();
        EngineNS.GamePlay.Scene.UMeshNode mCurrentMeshNode;
        EngineNS.GamePlay.Scene.UMeshNode mArrowMeshNode;
        float mCurrentMeshRadius = 1.0f;
        public float PlaneScale = 5.0f;
        EngineNS.GamePlay.Scene.UMeshNode PlaneMeshNode;
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

        ~UMeshPrimitiveEditor()
        {
            Dispose();
        }
        public void Dispose()
        {
            Mesh = null;
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
        protected async System.Threading.Tasks.Task Initialize_PreviewMaterialInstance(Graphics.Pipeline.UViewportSlate viewport, USlateApplication application, Graphics.Pipeline.URenderPolicy policy, float zMin, float zMax)
        {
            viewport.RenderPolicy = policy;

            await viewport.World.InitWorld();

            (viewport as Editor.UPreviewViewport).CameraController.ControlCamera(viewport.RenderPolicy.DefaultCamera);

            var mtl = await UEngine.Instance.GfxDevice.MaterialManager.GetMaterial(UEngine.Instance.Config.MeshPrimitiveEditorConfig.MaterialName);
            var materials = new Graphics.Pipeline.Shader.UMaterial[Mesh.mCoreObject.GetAtomNumber()];
            for (int i = 0; i < materials.Length; i++)
            {
                materials[i] = mtl;
            }
            var mesh = new Graphics.Mesh.TtMesh();
            var meshNodeData = new GamePlay.Scene.UMeshNode.UMeshNodeData();
            if (Mesh.PartialSkeleton != null)
            {
                mesh.Initialize(Mesh, materials, Rtti.UTypeDescGetter<Graphics.Mesh.UMdfSkinMesh>.TypeDesc);
                meshNodeData.MdfQueueType = EngineNS.Rtti.UTypeDesc.TypeStr(typeof(EngineNS.Graphics.Mesh.UMdfSkinMesh));
            }
            else
            {
                mesh.Initialize(Mesh, materials, Rtti.UTypeDescGetter<Graphics.Mesh.UMdfStaticMesh>.TypeDesc);
                meshNodeData.MdfQueueType = EngineNS.Rtti.UTypeDesc.TypeStr(typeof(EngineNS.Graphics.Mesh.UMdfStaticMesh));
            }
            meshNodeData.MeshName = Mesh.AssetName;
            var meshNode = await GamePlay.Scene.UMeshNode.AddMeshNode(viewport.World, viewport.World.Root, meshNodeData, typeof(GamePlay.UPlacement), mesh,
                        DVector3.Zero, Vector3.One, Quaternion.Identity);
            meshNode.HitproxyType = Graphics.Pipeline.UHitProxy.EHitproxyType.Root;
            meshNode.NodeData.Name = "PreviewObject";
            meshNode.IsAcceptShadow = true;
            meshNode.IsCastShadow = true;

            mCurrentMeshNode = meshNode;

            var aabb = mesh.MaterialMesh.AABB;
            mCurrentMeshRadius = aabb.GetMaxSide();
            BoundingSphere sphere;
            sphere.Center = aabb.GetCenter();
            sphere.Radius = mCurrentMeshRadius;
            policy.DefaultCamera.AutoZoom(ref sphere);

            {
                var arrowMaterialMesh = await UEngine.Instance.GfxDevice.MaterialMeshManager.GetMaterialMesh(RName.GetRName("mesh/base/arrow.ums", RName.ERNameType.Engine));
                var arrowMesh = new Graphics.Mesh.TtMesh();
                var ok = arrowMesh.Initialize(arrowMaterialMesh, Rtti.UTypeDescGetter<Graphics.Mesh.UMdfStaticMesh>.TypeDesc);
                if (ok)
                {
                    mArrowMeshNode = await GamePlay.Scene.UMeshNode.AddMeshNode(viewport.World, viewport.World.Root, new GamePlay.Scene.UMeshNode.UMeshNodeData(), typeof(GamePlay.UPlacement), arrowMesh, DVector3.UnitX * 3, Vector3.One, Quaternion.Identity);
                    mArrowMeshNode.HitproxyType = Graphics.Pipeline.UHitProxy.EHitproxyType.Root;
                    mArrowMeshNode.NodeData.Name = "PreviewArrow";
                    mArrowMeshNode.IsAcceptShadow = false;
                    mArrowMeshNode.IsCastShadow = false;
                }
            }

            {
                var meshCenter = aabb.GetCenter();
                var meshSize = aabb.GetSize() * PlaneScale;
                var maxLength = MathHelper.Max(meshSize.X, meshSize.Z);
                meshSize.X = meshSize.Z = maxLength;
                meshSize.Y = aabb.GetSize().Y * 0.05f;
                var boxStart = meshCenter - meshSize * 0.5f;
                boxStart.Y -= aabb.GetSize().Y * 0.5f + meshSize.Y * 0.5f + 0.001f;
                var box = Graphics.Mesh.UMeshDataProvider.MakePlane(meshSize.X, meshSize.Z).ToMesh();

                var PlaneMesh = new Graphics.Mesh.TtMesh();
                var tMaterials = new Graphics.Pipeline.Shader.UMaterial[1];
                tMaterials[0] = await UEngine.Instance.GfxDevice.MaterialInstanceManager.GetMaterialInstance(UEngine.Instance.Config.MeshPrimitiveEditorConfig.PlaneMaterialName);
                PlaneMesh.Initialize(box, tMaterials,
                    Rtti.UTypeDescGetter<Graphics.Mesh.UMdfStaticMesh>.TypeDesc);
                PlaneMeshNode = await GamePlay.Scene.UMeshNode.AddMeshNode(viewport.World, viewport.World.Root, new GamePlay.Scene.UMeshNode.UMeshNodeData(), typeof(GamePlay.UPlacement), PlaneMesh, new DVector3(0, -0.0001f, 0), Vector3.One, Quaternion.Identity);
                PlaneMeshNode.HitproxyType = Graphics.Pipeline.UHitProxy.EHitproxyType.None;
                PlaneMeshNode.NodeData.Name = "Plane";
                PlaneMeshNode.IsAcceptShadow = true;
                PlaneMeshNode.IsCastShadow = false;
            }

            var gridNode = await GamePlay.Scene.UGridNode.AddGridNode(viewport.World, viewport.World.Root);
            gridNode.ViewportSlate = this.PreviewViewport;
        }
        public Graphics.Mesh.TtMesh SdfDebugMesh;
        EngineNS.GamePlay.Scene.UMeshNode SdfMeshNode;
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
        public async System.Threading.Tasks.Task CreateSdfDebugMesh(GamePlay.UWorld world, DistanceField.TtSdfAsset sdfAsset)
        {
            if (sdfAsset==null || sdfAsset.Mips.Count < 0)
                return;

            if(SdfMeshNode==null)
            {
                var material = await UEngine.Instance.GfxDevice.MaterialInstanceManager.CreateMaterialInstance(RName.GetRName("material/sdfcolor.uminst", RName.ERNameType.Engine));
                SdfDebugMesh = new Graphics.Mesh.TtMesh();
                var rect = Graphics.Mesh.UMeshDataProvider.MakeBox(-0.5f, -0.5f, -0.5f, 1, 1, 1, 0xffffffff);
                var rectMesh = rect.ToMesh();
                var materials = new Graphics.Pipeline.Shader.UMaterial[1];
                materials[0] = material;
                SdfDebugMesh.Initialize(rectMesh, materials, Rtti.UTypeDescGetter<Graphics.Mesh.UMdfInstanceStaticMesh>.TypeDesc);
                SdfDebugMesh.MdfQueue.MdfDatas = this;

                var meshNode = await GamePlay.Scene.UMeshNode.AddMeshNode(world, world.Root, new GamePlay.Scene.UMeshNode.UMeshNodeData(), typeof(GamePlay.UPlacement), SdfDebugMesh, DVector3.Zero, Vector3.One, Quaternion.Identity);
                meshNode.SetStyle(GamePlay.Scene.UNode.ENodeStyles.VisibleFollowParent);
                meshNode.NodeData.Name = "Debug_SdfMeshNode";
                meshNode.IsAcceptShadow = false;
                meshNode.IsCastShadow = false;
                meshNode.HitproxyType = Graphics.Pipeline.UHitProxy.EHitproxyType.None;

                SdfMeshNode = meshNode;
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

            var instanceMdf = SdfDebugMesh.MdfQueue as Graphics.Mesh.UMdfInstanceStaticMesh;
            instanceMdf.InstanceModifier.InstanceBuffers.ResetInstance();
            instanceMdf.InstanceModifier.SetCapacity((uint)OutVoxelPositions.Count, false);
            for ( int i = 0; i < OutVoxelPositions.Count; ++i )
            {
                var voxelPos = OutVoxelPositions[i];
                var QuantizedDistance = OutVoxelDistance[i];
                var instance = new Graphics.Mesh.Modifier.FVSInstanceData();
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
        public async Thread.Async.TtTask<bool> OpenEditor(UMainEditorApplication mainEditor, RName name, object arg)
        {
            AssetName = name;
            Mesh = arg as Graphics.Mesh.UMeshPrimitives;
            if (Mesh == null)
            {
                Mesh = await UEngine.Instance.GfxDevice.MeshPrimitiveManager.CreateMeshPrimitive(name);
                if (Mesh == null)
                    return false;
            }

            PreviewViewport.PreviewAsset = AssetName;
            PreviewViewport.Title = $"Mesh:{name}";
            PreviewViewport.OnInitialize = Initialize_PreviewMaterialInstance;
            await PreviewViewport.Initialize(UEngine.Instance.GfxDevice.SlateApplication, UEngine.Instance.Config.MainRPolicyName, 0, 1);

            MeshPropGrid.Target = Mesh;
            EditorPropGrid.Target = this;
            UEngine.Instance.TickableManager.AddTickable(this);
            return true;
        }
        public void OnCloseEditor()
        {
            UEngine.Instance.TickableManager.RemoveTickable(this);
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
            IsDrawing = EGui.UIProxy.DockProxy.BeginMainForm(AssetName.Name, this, ImGuiWindowFlags_.ImGuiWindowFlags_NoSavedSettings);
            if (IsDrawing)
            {
                if (ImGuiAPI.IsWindowFocused(ImGuiFocusedFlags_.ImGuiFocusedFlags_RootAndChildWindows))
                {
                    var mainEditor = UEngine.Instance.GfxDevice.SlateApplication as Editor.UMainEditorApplication;
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
            ImGuiAPI.DockBuilderSplitNode(rightId, ImGuiDir_.ImGuiDir_Left, 0.8f, ref middleId, ref rightId);
            ImGuiAPI.DockBuilderSplitNode(rightId, ImGuiDir_.ImGuiDir_Down, 0.5f, ref rightDownId, ref rightUpId);
            ImGuiAPI.DockBuilderSplitNode(middleId, ImGuiDir_.ImGuiDir_Down, 0.3f, ref downId, ref middleId);
            ImGuiAPI.DockBuilderSplitNode(middleId, ImGuiDir_.ImGuiDir_Left, 0.2f, ref leftId, ref middleId);

            ImGuiAPI.DockBuilderDockWindow(EGui.UIProxy.DockProxy.GetDockWindowName("Preview", mDockKeyClass), middleId);
            ImGuiAPI.DockBuilderDockWindow(EGui.UIProxy.DockProxy.GetDockWindowName("MeshDetails", mDockKeyClass), rightDownId);
            ImGuiAPI.DockBuilderDockWindow(EGui.UIProxy.DockProxy.GetDockWindowName("EditorDetails", mDockKeyClass), rightDownId);

            ImGuiAPI.DockBuilderFinish(id);
        }
        protected unsafe void DrawToolBar()
        {
            var btSize = Vector2.Zero;
            if (EGui.UIProxy.CustomButton.ToolButton("Save", in btSize))
            {
                Mesh.SaveAssetTo(Mesh.AssetName);
                //var unused = UEngine.Instance.GfxDevice.MaterialInstanceManager.ReloadMaterialInstance(Mesh.AssetName);

                //USnapshot.Save(Mesh.AssetName, Mesh.GetAMeta(), PreviewViewport.RenderPolicy.GetFinalShowRSV(), UEngine.Instance.GfxDevice.RenderContext.mCoreObject.GetImmCommandList());
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
            ImGuiAPI.SameLine(0, -1);
            if (EGui.UIProxy.CustomButton.ToolButton("BuildCluster", in btSize))
            {
                var meshMeta = Mesh.GetAMeta() as EngineNS.Graphics.Mesh.UMeshPrimitivesAMeta;
                meshMeta.IsClustered = true;
                meshMeta.AddReferenceAsset(RName.GetRName(Mesh.AssetName + ".clusteremesh", Mesh.AssetName.RNameType));
                meshMeta.SaveAMeta();
                Mesh.BuildClusteredMesh();
            }
            ImGuiAPI.SameLine(0, -1);
            if (EGui.UIProxy.CustomButton.ToolButton("LoadCluster", in btSize))
            {
                Mesh.LoadClusterMesh();
            }
        }

        bool ShowEditorPropGrid = true;
        protected void DrawEditorDetails()
        {
            var sz = new Vector2(-1);
            var show = EGui.UIProxy.DockProxy.BeginPanel(mDockKeyClass, "EditorDetails", ref ShowEditorPropGrid, ImGuiWindowFlags_.ImGuiWindowFlags_None);
            if (show)
            {
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
                MeshPropGrid.OnDraw(true, false, false, ImGuiWindowFlags_.ImGuiWindowFlags_NoScrollbar | ImGuiWindowFlags_.ImGuiWindowFlags_AlwaysAutoResize);
                if (ImGuiAPI.Button("Build SDF"))
                {
                    UMeshDataProvider meshProvider = new UMeshDataProvider();
                    if (meshProvider.InitFrom(Mesh))
                    {
                        var sdfConfig = new DistanceField.DistanceFieldConfig();
                        var outSDF = MeshSdfAsset;
                        DistanceField.UMeshUtilities.GenerateSignedDistanceFieldVolumeData(Mesh.AssetName.ToString(), meshProvider, sdfConfig, 1.0f, false, ref outSDF);

                        var noExtName = Mesh.AssetName.Name.Substring(0, Mesh.AssetName.Name.Length - Mesh.AssetName.ExtName.Length);
                        var rn = RName.GetRName(noExtName + DistanceField.TtSdfAsset.AssetExt, Mesh.AssetName.RNameType);
                        var ameta = new DistanceField.TtSdfAssetAMeta();
                        ameta.SetAssetName(rn);
                        ameta.AssetId = Guid.NewGuid();
                        ameta.TypeStr = Rtti.UTypeDescManager.Instance.GetTypeStringFromType(typeof(DistanceField.TtSdfAssetAMeta));
                        ameta.Description = $"This is a {typeof(DistanceField.TtSdfAssetAMeta).FullName}\n";
                        ameta.SaveAMeta();
                        UEngine.Instance.AssetMetaManager.RegAsset(ameta);

                        outSDF.SaveAssetTo(rn);

                        CreateSdfDebugMesh(PreviewViewport.World, outSDF);

                        // test load sdf
                        Action action = async () =>
                        {
                            var testSDF = await UEngine.Instance.SdfAssetManager.GetSdfAsset(rn);
                        };
                        action();

                    }
                }
            }
            EGui.UIProxy.DockProxy.EndPanel(show);
        }
        bool ShowPreview = true;
        protected unsafe void DrawPreview()
        {
            var show = EGui.UIProxy.DockProxy.BeginPanel(mDockKeyClass, "Preview", ref ShowPreview, ImGuiWindowFlags_.ImGuiWindowFlags_None);
            if (show)
            {
                PreviewViewport.ViewportType = Graphics.Pipeline.UViewportSlate.EViewportType.ChildWindow;
                PreviewViewport.OnDraw();
            }
            this.PreviewViewport.Visible = show;
            EGui.UIProxy.DockProxy.EndPanel(show);
        }
        #endregion
        public void OnEvent(in Bricks.Input.Event e)
        {
            //throw new NotImplementedException();
        }
        #region Tickable
        public void TickLogic(float ellapse)
        {
            PreviewViewport.TickLogic(ellapse);
        }
        //[Category("Light")]
        [EGui.Controls.PropertyGrid.PGValueRange(-3.1416f, 3.1416f)]
        [EGui.Controls.PropertyGrid.PGValueChangeStep(3.1416f / 100.0f)]
        public float Yaw { get; set; } = -0.955047f;
        //[Category("Light")]
        //[EGui.Controls.PropertyGrid.PGValueRange(-3.1416f, 3.1416f)]
        //[EGui.Controls.PropertyGrid.PGValueChangeStep(3.1416f / 100.0f)]
        //public float Pitch { get; set; }
        //[Category("Light")]
        [EGui.Controls.PropertyGrid.PGValueRange(-3.1416f, 3.1416f)]
        [EGui.Controls.PropertyGrid.PGValueChangeStep(3.1416f / 100.0f)]
        public float Roll { get; set; } = -0.552922f;

        public void TickRender(float ellapse)
        {
            PreviewViewport.TickRender(ellapse);

            if (IsDrawing == false)
                return;

            if (ImGuiAPI.IsMouseDragging(ImGuiMouseButton_.ImGuiMouseButton_Left, -1) || ImGuiAPI.IsMouseDragging(ImGuiMouseButton_.ImGuiMouseButton_Right, -1))
            {
                if (UEngine.Instance.InputSystem.IsKeyDown(EngineNS.Bricks.Input.Keycode.KEY_l))
                {
                    var delta = ImGuiAPI.GetMouseDragDelta(ImGuiMouseButton_.ImGuiMouseButton_Left, -1);
                    var delta2 = ImGuiAPI.GetMouseDragDelta(ImGuiMouseButton_.ImGuiMouseButton_Right, -1);
                    delta.X = Math.Max(delta.X, delta.X);
                    delta.Y = Math.Max(delta.Y, delta.Y);

                    var step = 3.1416f / 500.0f;
                    Yaw -= delta.X * step;
                    Roll += delta.Y * step;
                    ImGuiAPI.ResetMouseDragDelta(ImGuiMouseButton_.ImGuiMouseButton_Left);
                    ImGuiAPI.ResetMouseDragDelta(ImGuiMouseButton_.ImGuiMouseButton_Right);
                    mArrowMeshNode.Placement.Scale = new Vector3(Math.Min(mCurrentMeshRadius * 0.5f, 2.0f));
                }
            }
            else
            {
                mArrowMeshNode.Placement.Scale = Vector3.Zero;
            }


            var quat = EngineNS.Quaternion.RotationYawPitchRoll(Yaw, 0, Roll);
            PreviewViewport.World.DirectionLight.Direction = quat * Vector3.UnitX;

            var arrowPos = -mCurrentMeshRadius * PreviewViewport.World.DirectionLight.Direction;
            mArrowMeshNode.Placement.Position = new DVector3(arrowPos.X, arrowPos.Y, arrowPos.Z);
            mArrowMeshNode.Placement.Quat = quat;
        }
        public void TickBeginFrame(float ellapse)
        {

        }
        public void TickSync(float ellapse)
        {
            PreviewViewport.TickSync(ellapse);
        }
        #endregion
    }
}

namespace EngineNS.Graphics.Mesh
{
    [Editor.UAssetEditor(EditorType = typeof(Editor.Forms.UMeshPrimitiveEditor))]
    public partial class UMeshPrimitives
    {
    }
}