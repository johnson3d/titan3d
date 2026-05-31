using System;
using System.ComponentModel;
using System.Security.Permissions;

namespace EngineNS.Editor
{
    public class TtPreviewStudioContext
    {
        public GamePlay.Scene.TtMeshNode FloorNode { get; internal set; }
        public GamePlay.Scene.TtGridNode GridNode { get; internal set; }
        public GamePlay.Scene.TtMeshNode BackdropNode { get; internal set; }
        public GamePlay.Scene.TtMeshNode SkyBackdropNode { get; internal set; }
        public GamePlay.Scene.TtMeshNode SkyboxNode { get; internal set; }
        public BoundingBox AssetBounds { get; internal set; }
        public float Radius { get; internal set; } = 1.0f;
        public float FloorY { get; internal set; } = 0;
        public float BackdropRadius { get; internal set; } = 1000.0f;
    }

    public class TtPreviewViewport : EGui.Slate.TtWorldViewportSlate
    {
        static TtPreviewViewport()
        {
            TtEngine.Instance.InteractiveModeManager.RegisterMode<TtPreviewViewport, TtPreviewViewportInteractiveMode>();
        }
        public TtPreviewViewport()
        {
        }
        ~TtPreviewViewport()
        {
            Dispose();
        }
        public override void Dispose()
        {
            PresentWindow?.UnregEventProcessor(this);
            RenderPolicy?.Dispose();
            RenderPolicy = null;
            if (mVisParameter != null)
            {
                mVisParameter.Reset();
                mVisParameter = null;
            }
            base.Dispose();
        }
        new protected async Thread.Async.TtTask<bool> Initialize_Default(Graphics.Pipeline.TtViewportSlate viewport, TtSlateApplication application, Graphics.Pipeline.TtRenderPolicy policy, float zMin, float zMax)
        {
            RenderPolicy = policy;

            //await RenderPolicy.Initialize(null);

            CameraController.ControlCamera(RenderPolicy.DefaultCamera);

            var materials = new Graphics.Pipeline.Shader.TtMaterial[1];
            materials[0] = await RName.GetRName("utest/ttt.material").GetAsset<Graphics.Pipeline.Shader.TtMaterial>();// await TtEngine.Instance.GfxDevice.MaterialManager.GetMaterial(RName.GetRName("utest/ttt.material"));
            if (materials[0] == null)
                return false;
            var mesh = new Graphics.Mesh.TtRenderMesh();
            var rect = Graphics.Mesh.TtMeshDataProvider.MakeBox(-0.5f, -0.5f, -0.5f, 1, 1, 1);
            var rectMesh = rect.ToMesh();
            var ok = mesh.Initialize(rectMesh, materials, Rtti.TtTypeDescGetter<Graphics.Mesh.TtMdfStaticMesh>.TypeDesc);
            if (ok)
            {
                var meshNode = await GamePlay .Scene.TtMeshNode.AddMeshNode(viewport.World, viewport.World.Root, new GamePlay.Scene.TtMeshNode.TtMeshNodeData(), typeof(GamePlay.TtPlacement), mesh, DVector3.Zero, Vector3.One, Quaternion.Identity);
                meshNode.HitproxyType = Graphics.Pipeline.TtHitProxy.EHitproxyType.Root;
                meshNode.NodeData.Name = "PreviewObject";
                meshNode.IsCastShadow = true;
            }

            var aabb = mesh.MaterialMesh.AABB;
            FrameStudioCamera(in aabb);
            await CreateStudioEnvironment(aabb);

            //this.RenderPolicy.GBuffers.SunLightColor = new Vector3(1, 1, 1);
            //this.RenderPolicy.GBuffers.SunLightDirection = new Vector3(1, 1, 1);
            //this.RenderPolicy.GBuffers.SkyLightColor = new Vector3(0.1f, 0.1f, 0.1f);
            //this.RenderPolicy.GBuffers.GroundLightColor = new Vector3(0.1f, 0.1f, 0.1f);
            //this.RenderPolicy.GBuffers.UpdateViewportCBuffer();

            return true;
        }
        public override async Thread.Async.TtTask<bool> Initialize(TtSlateApplication application, RName policyName, float zMin, float zMax)
        {
            Graphics.Pipeline.TtRenderPolicy policy = await TtEngine.Instance.EventPoster.Post((state) =>
            {
                return Bricks.RenderPolicyEditor.TtRenderPolicyAsset.CreateRenderPolicy(policyName, this);
            }, Thread.Async.EAsyncTarget.AsyncIO);
            
            await policy.Initialize(null);
            if (ClientSize.X == 0 || ClientSize.Y == 0)
            {
                policy.OnResize(1, 1);
            }
            else
            {
                policy.OnResize(ClientSize.X, ClientSize.Y);
            }

            await World.InitWorld();

            if (OnInitialize == null)
            {
                OnInitialize = this.Initialize_Default;
            }
            await OnInitialize(this, application, policy, zMin, zMax);

            //mDefaultHUD.RenderCamera = this.RenderPolicy.DefaultCamera;
            //this.PushHUD(mDefaultHUD);

            mAxis = new GamePlay.TtAxis();
            await mAxis.Initialize(this.World, CameraController);

            await ReCreateInteractiveModes();
            await InitParticlePolicy();
            // ReCreateInteractiveModes 沿继承链收集 mode, 因为基类 TtWorldViewportSlate
            // 自己也注册了 TtWorldViewportInteractiveMode, 所以 InteractiveModes 列表里
            // 同时有它和 TtPreviewViewport 自己注册的 TtPreviewViewportInteractiveMode;
            // 而 ReCreateInteractiveModes 默认把列表最后一个设为 CurrentIntercativeMode
            // (顺序不可控)。这里显式把缺省切回 PreviewViewport 自己的那个。
            SetDefaultInteractiveMode<TtPreviewViewportInteractiveMode>();

            IsInlitialized = true;
            StartTime = System.DateTime.Now;
            HasAssetSnap = IO.TtFileManager.FileExists(PreviewAsset.Address + ".snap");
            return true;
        }
        public bool HasAssetSnap = false;
        public System.DateTime StartTime;
        protected override void OnClientChanged(bool bSizeChanged)
        {
            base.OnClientChanged(bSizeChanged);
        }
        [Category("Option")]
        [ReadOnly(true)]
        public RName PreviewAsset { get; set; } = null;
        [Category("Studio")]
        public TtPreviewStudioContext StudioContext { get; private set; }
        private const string StudioSkyboxImageName = "editor/studio/sakura_prefiltered_env_12_blur.png";
        private static readonly object mStudioSkyImageLocker = new object();
        private static StbImageSharp.TtMemImage mStudioSkyImage;
        private static bool IsStudioNodeVisible(GamePlay.Scene.TtNode node)
        {
            return node != null && node.HasStyle(GamePlay.Scene.TtNode.ENodeStyles.Invisible) == false;
        }
        private static void SetStudioNodeVisible(GamePlay.Scene.TtNode node, bool visible)
        {
            if (node == null)
                return;
            if (visible)
                node.UnsetStyle(GamePlay.Scene.TtNode.ENodeStyles.Invisible);
            else
                node.SetStyle(GamePlay.Scene.TtNode.ENodeStyles.Invisible);
        }
        [Category("Studio")]
        public bool ShowStudioFloor
        {
            get
            {
                var floor = StudioContext?.FloorNode;
                return IsStudioNodeVisible(floor);
            }
            set
            {
                var floor = StudioContext?.FloorNode;
                SetStudioNodeVisible(floor, value);
            }
        }
        [Category("Studio")]
        public bool ShowStudioGrid
        {
            get
            {
                var grid = StudioContext?.GridNode;
                return IsStudioNodeVisible(grid);
            }
            set
            {
                var grid = StudioContext?.GridNode;
                SetStudioNodeVisible(grid, value);
            }
        }
        [Category("Studio")]
        public bool ShowStudioBackdrop
        {
            get
            {
                var backdrop = StudioContext?.BackdropNode;
                var skybox = StudioContext?.SkyboxNode ?? StudioContext?.SkyBackdropNode;
                return IsStudioNodeVisible(backdrop) || IsStudioNodeVisible(skybox);
            }
            set
            {
                var backdrop = StudioContext?.BackdropNode;
                var skybox = StudioContext?.SkyboxNode ?? StudioContext?.SkyBackdropNode;
                SetStudioNodeVisible(backdrop, value);
                SetStudioNodeVisible(skybox, value);
            }
        }
        public void ApplyStudioLighting()
        {
            if (World == null)
                return;

            var dirLight = World.DirectionLight;
            var lightDir = new Vector3(-0.24f, -0.92f, 0.29f);
            lightDir.Normalize();
            dirLight.Direction = lightDir;
            dirLight.SunLightColor = new Vector3(1.0f, 0.95f, 0.92f);
            dirLight.SunLightIntensity = 3.6f;
            dirLight.SkyLightColor = new Vector3(0.31f, 0.34f, 0.40f);
            dirLight.GroundLightColor = new Vector3(0.24f, 0.22f, 0.20f);
        }
        public void FrameStudioCamera(in BoundingBox assetBounds, float padding = 1.2f)
        {
            if (RenderPolicy?.DefaultCamera == null)
                return;

            DBoundingSphere sphere;
            sphere.Center = assetBounds.GetCenter().AsDVector();
            sphere.Radius = MathHelper.Max(assetBounds.GetMaxSide() * padding, 1.0f);
            RenderPolicy.DefaultCamera.AutoZoom(in sphere);
        }
        public async Thread.Async.TtTask<TtPreviewStudioContext> CreateStudioEnvironment(
            BoundingBox assetBounds,
            float floorScale = 5.0f,
            RName floorMaterialName = null,
            bool createFloor = true,
            bool createGrid = true,
            bool createBackdrop = true)
        {
            ApplyStudioLighting();
            if (RenderPolicy != null)
                RenderPolicy.LookNodeName = "DirLightingNode";

            var size = assetBounds.GetSize();
            var center = assetBounds.GetCenter();
            var radius = MathHelper.Max(assetBounds.GetMaxSide(), 1.0f);
            var floorSize = MathHelper.Max(MathHelper.Max(size.X, size.Z) * floorScale, radius * floorScale);
            floorSize = MathHelper.Max(floorSize, 6.0f);
            var floorY = center.Y - size.Y * 0.5f - 0.001f;

            var context = new TtPreviewStudioContext()
            {
                AssetBounds = assetBounds,
                Radius = radius,
                FloorY = floorY,
            };
            context.BackdropRadius = MathHelper.Max(radius * 80.0f, 1000.0f);

            if (createFloor)
            {
                context.FloorNode = await CreateStudioFloor(center, floorY, floorSize, floorMaterialName);
            }

            if (createGrid)
            {
                var gridNode = await GamePlay.Scene.TtGridNode.AddGridNode(World, World.Root);
                gridNode.ViewportSlate = this;
                gridNode.GridHeight = floorY + 0.006f;
                gridNode.GridFade = 0.24f;
                gridNode.GridRadius = MathHelper.Max(floorSize * 1.35f, 12.0f);
                context.GridNode = gridNode;
            }

            if (createBackdrop)
            {
                context.SkyboxNode = await CreateStudioSkybox(center, context.BackdropRadius);
                context.SkyBackdropNode = context.SkyboxNode;
            }

            StudioContext = context;
            return context;
        }
        public Thread.Async.TtTask<TtPreviewStudioContext> CreateStudioEnvironment(
            DBoundingBox assetBounds,
            float floorScale = 5.0f,
            RName floorMaterialName = null,
            bool createFloor = true,
            bool createGrid = true,
            bool createBackdrop = true)
        {
            return CreateStudioEnvironment(assetBounds.ToSingleAABB(), floorScale, floorMaterialName, createFloor, createGrid, createBackdrop);
        }
        private bool IsDefaultStudioFloorMaterial(RName floorMaterialName)
        {
            return floorMaterialName == null ||
                (floorMaterialName.RNameType == RName.ERNameType.Engine &&
                string.Equals(floorMaterialName.Name, "material/whitecolor.uminst", StringComparison.OrdinalIgnoreCase));
        }
        private async Thread.Async.TtTask<Graphics.Pipeline.Shader.TtMaterialInstance> CreateStudioColorMaterial(
            Vector4 color,
            Graphics.Pipeline.ERenderLayer renderLayer,
            bool cullNone = false,
            bool depthWrite = true)
        {
            var material = await RName.GetRName("material/sysdft_color.material", RName.ERNameType.Engine)
                .GetAsset<Graphics.Pipeline.Shader.TtMaterial>();
            if (material == null)
                return null;

            var materialInstance = Graphics.Pipeline.Shader.TtMaterialInstance.CreateMaterialInstance(material);
            materialInstance.RenderLayer = renderLayer;
            unsafe
            {
                if (cullNone)
                {
                    var rsState = materialInstance.Rasterizer;
                    rsState.CullMode = NxRHI.ECullMode.CMD_NONE;
                    materialInstance.Rasterizer = rsState;
                }

                if (depthWrite == false)
                {
                    var dsState = materialInstance.DepthStencil;
                    dsState.DepthWriteMask = 0;
                    materialInstance.DepthStencil = dsState;
                }
            }

            var colorVar = materialInstance.FindVar("clr4_0");
            if (colorVar != null)
            {
                colorVar.SetValue(color);
            }
            return materialInstance;
        }
        private static StbImageSharp.TtMemImage GetStudioSkyImage()
        {
            if (mStudioSkyImage != null)
                return mStudioSkyImage;

            lock (mStudioSkyImageLocker)
            {
                if (mStudioSkyImage != null)
                    return mStudioSkyImage;

                var gameRoot = TtEngine.Instance?.FileManager?.GetRoot2(RName.ERNameType.Game);
                var localAsset = string.IsNullOrEmpty(gameRoot) ? null :
                    System.IO.Path.Combine(gameRoot, StudioSkyboxImageName.Replace('/', System.IO.Path.DirectorySeparatorChar));
                string[] candidates =
                {
                    localAsset,
                    @"D:\Dev\sakura-engine\assets\ibl\prefiltered_env_12.png",
                    @"D:\Dev\sakura-engine\assets\ibl\prefiltered_env.png",
                };

                foreach (var candidate in candidates)
                {
                    if (string.IsNullOrEmpty(candidate) || System.IO.File.Exists(candidate) == false)
                        continue;

                    try
                    {
                        using (var stream = System.IO.File.OpenRead(candidate))
                        {
                            mStudioSkyImage = StbImageSharp.TtMemImage.FromStream(stream, StbImageSharp.ColorComponents.RedGreenBlueAlpha);
                        }
                        if (mStudioSkyImage != null)
                            return mStudioSkyImage;
                    }
                    catch
                    {
                    }
                }
            }

            return null;
        }
        private static uint PackStudioColor(float r, float g, float b, float a = 1.0f)
        {
            r = MathHelper.FClamp(r, 0.0f, 1.0f);
            g = MathHelper.FClamp(g, 0.0f, 1.0f);
            b = MathHelper.FClamp(b, 0.0f, 1.0f);
            a = MathHelper.FClamp(a, 0.0f, 1.0f);
            return new Color4b(
                (byte)Math.Round(r * 255.0f),
                (byte)Math.Round(g * 255.0f),
                (byte)Math.Round(b * 255.0f),
                (byte)Math.Round(a * 255.0f)).ToR8G8B8A8();
        }
        private static Color4b SampleStudioSkyPixel(StbImageSharp.TtMemImage image, int x, int y)
        {
            x = MathHelper.Clamp(x, 0, image.Width - 1);
            y = MathHelper.Clamp(y, 0, image.Height - 1);
            return image.GetPixel(x, y);
        }
        private static uint SampleStudioSkyColor(Vector3 direction)
        {
            if (direction.LengthSquared() <= 0.000001f)
                direction = new Vector3(0, 1, 0);
            direction.Normalize();

            var image = GetStudioSkyImage();
            if (image == null || image.Width <= 0 || image.Height <= 0)
            {
                var t = (float)Math.Pow(MathHelper.FClamp(direction.Y * 0.5f + 0.5f, 0.0f, 1.0f), 0.72f);
                return PackStudioColor(
                    0.58f + (0.26f - 0.58f) * t,
                    0.62f + (0.43f - 0.62f) * t,
                    0.70f + (0.68f - 0.70f) * t);
            }

            var phi = Math.Atan2(direction.Z, direction.X);
            var theta = Math.Acos(MathHelper.FClamp(direction.Y, -1.0f, 1.0f));
            var u = (float)(phi / (2.0 * Math.PI) + 0.5);
            var v = (float)(theta / Math.PI);
            u = u - (float)Math.Floor(u);
            v = MathHelper.FClamp(v, 0.0f, 1.0f);

            var x = u * image.Width;
            var y = v * (image.Height - 1);
            var x0 = (int)Math.Floor(x) % image.Width;
            var x1 = (x0 + 1) % image.Width;
            var y0 = MathHelper.Clamp((int)Math.Floor(y), 0, image.Height - 1);
            var y1 = MathHelper.Clamp(y0 + 1, 0, image.Height - 1);
            var tx = x - (float)Math.Floor(x);
            var ty = y - (float)Math.Floor(y);

            var c00 = SampleStudioSkyPixel(image, x0, y0);
            var c10 = SampleStudioSkyPixel(image, x1, y0);
            var c01 = SampleStudioSkyPixel(image, x0, y1);
            var c11 = SampleStudioSkyPixel(image, x1, y1);

            float Lerp(float a, float b, float t) => a + (b - a) * t;
            var r0 = Lerp(c00.R, c10.R, tx);
            var r1 = Lerp(c01.R, c11.R, tx);
            var g0 = Lerp(c00.G, c10.G, tx);
            var g1 = Lerp(c01.G, c11.G, tx);
            var b0 = Lerp(c00.B, c10.B, tx);
            var b1 = Lerp(c01.B, c11.B, tx);

            return PackStudioColor(
                Lerp(r0, r1, ty) / 255.0f,
                Lerp(g0, g1, ty) / 255.0f,
                Lerp(b0, b1, ty) / 255.0f);
        }
        private async Thread.Async.TtTask<Graphics.Pipeline.Shader.TtMaterialInstance> CreateStudioVertexColorMaterial(
            Graphics.Pipeline.ERenderLayer renderLayer,
            bool cullNone = false,
            bool depthWrite = true)
        {
            var material = await RName.GetRName("material/vfx_color.material", RName.ERNameType.Engine)
                .GetAsset<Graphics.Pipeline.Shader.TtMaterial>();
            if (material == null)
                return null;

            var materialInstance = Graphics.Pipeline.Shader.TtMaterialInstance.CreateMaterialInstance(material);
            materialInstance.RenderLayer = renderLayer;
            unsafe
            {
                var rsState = materialInstance.Rasterizer;
                rsState.FillMode = NxRHI.EFillMode.FMD_SOLID;
                if (cullNone)
                {
                    rsState.CullMode = NxRHI.ECullMode.CMD_NONE;
                }
                materialInstance.Rasterizer = rsState;

                if (depthWrite == false)
                {
                    var dsState = materialInstance.DepthStencil;
                    dsState.DepthWriteMask = 0;
                    materialInstance.DepthStencil = dsState;
                }
            }
            return materialInstance;
        }
        private async Thread.Async.TtTask<GamePlay.Scene.TtMeshNode> CreateStudioSkybox(
            Vector3 assetCenter,
            float radius)
        {
            var materialInstance = await CreateStudioVertexColorMaterial(
                Graphics.Pipeline.ERenderLayer.RL_Background, cullNone: true, depthWrite: false);
            if (materialInstance == null)
                return null;

            var skyboxMesh = new Graphics.Mesh.TtRenderMesh();
            var materials = new Graphics.Pipeline.Shader.TtMaterial[1];
            materials[0] = materialInstance;
            var provider = Graphics.Mesh.TtMeshDataProvider.MakeLatLongSphere(1.0f, 128, 64, SampleStudioSkyColor);
            if (provider == null)
                return null;

            skyboxMesh.Initialize(provider.ToMesh(), materials,
                Rtti.TtTypeDescGetter<Graphics.Mesh.TtMdfStaticMesh>.TypeDesc);
            skyboxMesh.IsUnlit = true;

            var skyboxNode = await GamePlay.Scene.TtMeshNode.AddMeshNode(World, World.Root,
                new GamePlay.Scene.TtMeshNode.TtMeshNodeData(), typeof(GamePlay.TtPlacement), skyboxMesh,
                new DVector3(assetCenter.X, assetCenter.Y, assetCenter.Z), new Vector3(radius), Quaternion.Identity);
            skyboxNode.HitproxyType = Graphics.Pipeline.TtHitProxy.EHitproxyType.None;
            skyboxNode.NodeData.Name = "StudioSkybox";
            skyboxNode.IsAcceptShadow = false;
            skyboxNode.IsCastShadow = false;
            skyboxNode.SetStyle(GamePlay.Scene.TtNode.ENodeStyles.VisibleAlways);

            return skyboxNode;
        }
        private async Thread.Async.TtTask<GamePlay.Scene.TtMeshNode> CreateStudioBackdrop(
            Vector3 assetCenter,
            float radius,
            Graphics.Pipeline.ERenderLayer renderLayer,
            string nodeName,
            Vector4 color)
        {
            var materialInstance = await CreateStudioColorMaterial(color, renderLayer, cullNone: true, depthWrite: false);
            if (materialInstance == null)
                return null;

            var backdropMesh = new Graphics.Mesh.TtRenderMesh();
            var materials = new Graphics.Pipeline.Shader.TtMaterial[1];
            materials[0] = materialInstance;
            backdropMesh.Initialize(Graphics.Mesh.TtMeshDataProvider.MakeSphere(1.0f, 32, 16, 0xffffffff).ToMesh(), materials,
                Rtti.TtTypeDescGetter<Graphics.Mesh.TtMdfStaticMesh>.TypeDesc);
            backdropMesh.IsUnlit = true;

            var backdropNode = await GamePlay.Scene.TtMeshNode.AddMeshNode(World, World.Root,
                new GamePlay.Scene.TtMeshNode.TtMeshNodeData(), typeof(GamePlay.TtPlacement), backdropMesh,
                new DVector3(assetCenter.X, assetCenter.Y, assetCenter.Z), new Vector3(radius), Quaternion.Identity);
            backdropNode.HitproxyType = Graphics.Pipeline.TtHitProxy.EHitproxyType.None;
            backdropNode.NodeData.Name = nodeName;
            backdropNode.IsAcceptShadow = false;
            backdropNode.IsCastShadow = false;
            backdropNode.SetStyle(GamePlay.Scene.TtNode.ENodeStyles.VisibleAlways);

            return backdropNode;
        }
        private async Thread.Async.TtTask<GamePlay.Scene.TtMeshNode> CreateStudioFloor(
            Vector3 assetCenter,
            float floorY,
            float floorSize,
            RName floorMaterialName)
        {
            Graphics.Pipeline.Shader.TtMaterial floorMaterial = null;
            if (IsDefaultStudioFloorMaterial(floorMaterialName))
            {
                floorMaterial = await CreateStudioColorMaterial(new Vector4(0.68f, 0.72f, 0.76f, 1.0f),
                    Graphics.Pipeline.ERenderLayer.RL_Opaque, cullNone: true);
            }
            else
            {
                floorMaterial = await floorMaterialName.GetAsset<Graphics.Pipeline.Shader.TtMaterialInstance>();
                if (floorMaterial == null)
                    floorMaterial = await floorMaterialName.GetAsset<Graphics.Pipeline.Shader.TtMaterial>();
            }
            if (floorMaterial == null)
                return null;

            var floorMesh = new Graphics.Mesh.TtRenderMesh();
            var materials = new Graphics.Pipeline.Shader.TtMaterial[1];
            materials[0] = floorMaterial;
            floorMesh.Initialize(Graphics.Mesh.TtMeshDataProvider.MakePlane(floorSize, floorSize).ToMesh(), materials,
                Rtti.TtTypeDescGetter<Graphics.Mesh.TtMdfStaticMesh>.TypeDesc);

            var floorNode = await GamePlay.Scene.TtMeshNode.AddMeshNode(World, World.Root,
                new GamePlay.Scene.TtMeshNode.TtMeshNodeData(), typeof(GamePlay.TtPlacement), floorMesh,
                new DVector3(assetCenter.X, floorY, assetCenter.Z), Vector3.One, Quaternion.Identity);
            floorNode.HitproxyType = Graphics.Pipeline.TtHitProxy.EHitproxyType.None;
            floorNode.NodeData.Name = "StudioShadowFloor";
            floorNode.IsAcceptShadow = true;
            floorNode.IsCastShadow = false;
            floorNode.SetStyle(GamePlay.Scene.TtNode.ENodeStyles.VisibleAlways);

            return floorNode;
        }
        public delegate Vector2 Delegate_OnDrawViewportUIAction(in Vector2 startDrawPos);
        public Delegate_OnDrawViewportUIAction OnDrawViewportUIAction;
        public override Vector2 OnDrawViewportUI(in Vector2 startDrawPos) 
        {
            var baseUsedSize = base.OnDrawViewportUI(in startDrawPos);
            var totalUsedSize = baseUsedSize;
            var adjustedStartPos = new Vector2(startDrawPos.X, startDrawPos.Y + baseUsedSize.Y);

            if (OnDrawViewportUIAction != null)
            {
                var actionUsedSize = OnDrawViewportUIAction.Invoke(adjustedStartPos);
                totalUsedSize.X = Math.Max(totalUsedSize.X, actionUsedSize.X);
                totalUsedSize.Y = baseUsedSize.Y + actionUsedSize.Y;
            }
            else
            {
                if (PreviewAsset != null)
                {
                    ImGuiAPI.SameLine(0, -1);
                    if (EGui.UIProxy.CustomButton.ToolButton("S", in Vector2.Zero))
                    {
                        Editor.TtSnapshot.Save(PreviewAsset, TtEngine.Instance.AssetMetaManager.GetAssetMeta(PreviewAsset), RenderPolicy.GetFinalShowRSV());
                    }
                    else if (HasAssetSnap == false)
                    {
                        var tm = System.DateTime.Now;
                        TimeSpan ts = tm.Subtract(StartTime);
                        if (ts.Seconds > 10)
                        {
                            Editor.TtSnapshot.Save(PreviewAsset, TtEngine.Instance.AssetMetaManager.GetAssetMeta(PreviewAsset), RenderPolicy.GetFinalShowRSV());
                            HasAssetSnap = true;
                        }
                    }
                }
            }

            var currentPos = ImGuiAPI.GetCursorScreenPos();
            totalUsedSize.X = Math.Max(totalUsedSize.X, currentPos.X - startDrawPos.X);
            totalUsedSize.Y = currentPos.Y - startDrawPos.Y;

            return totalUsedSize;
        }
        protected override ImTextureRef GetShowTexture()
        {
            var srv = RenderPolicy?.GetFinalShowRSV();
            if (srv == null)
                return new ImTextureRef();
            var result = new ImTextureRef();
            result.m__TexID = (ulong)srv.GetTextureHandle();
            return result;
        }
        #region CameraControl
        public bool FreezCameraControl = false;
        public delegate void Delegate_OnEvent(in Bricks.Input.Event e);
        public Delegate_OnEvent OnEventAction;
        public enum EViewportMotion
        {
            None = 0,
            Rotate,
            Zoom,
            Move,
            ChangeMoveSpeed,
        }
        EViewportMotion mViewportMotion = EViewportMotion.None;
        public EViewportMotion ViewportMotion
        {
            get => mViewportMotion;
            set => mViewportMotion = value;
        }
        #endregion

        GamePlay.TtWorld.TtVisParameter mVisParameter = new GamePlay.TtWorld.TtVisParameter();
        public GamePlay.TtWorld.TtVisParameter VisParameter
        {
            get => mVisParameter;
        }
        public void TickRender(float ellapse)
        {
            
        }
        public Action AfterTickSync;
        public override void TickSync(float ellapse)
        {
            if (IsInlitialized == false)
                return;
            //if (IsDrawing == false)
            //    return;
            RenderPolicy?.TickSync();
            UpdateStudioBackdrop();
            if (AfterTickSync != null)
                AfterTickSync();
        }
        private void UpdateStudioBackdrop()
        {
            var backdrop = StudioContext?.BackdropNode;
            var skyBackdrop = StudioContext?.SkyBackdropNode;
            var skybox = StudioContext?.SkyboxNode;
            var camera = RenderPolicy?.DefaultCamera;
            if (camera == null)
                return;

            var cameraPos = camera.GetPosition();
            if (backdrop != null)
            {
                backdrop.Placement.Position = cameraPos;
                backdrop.Placement.Scale = new Vector3(StudioContext.BackdropRadius);
            }
            if (skyBackdrop != null)
            {
                skyBackdrop.Placement.Position = cameraPos;
                skyBackdrop.Placement.Scale = new Vector3(StudioContext.BackdropRadius * 0.96f);
            }
            if (skybox != null && skybox != skyBackdrop)
            {
                skybox.Placement.Position = cameraPos;
                skybox.Placement.Scale = new Vector3(StudioContext.BackdropRadius * 0.96f);
            }
        }
    }
}
