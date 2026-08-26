using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace EngineNS.GamePlay.Scene
{
    // 贴花写哪几张 GBuffer RT 的模式 (EDecalMode) 定义在 TtMaterial 上 ——
    // 它是材质域属性 (对应 UE 的 DecalBlendMode), 见
    // Graphics.Pipeline.Shader.TtMaterial.EDecalMode 与 TtMaterial.DecalMode。

    /// <summary>
    /// 贴花投影盒的场景节点。
    ///
    /// 投影约定 (引擎是左手 Y-up, +Z 朝屏幕里):
    ///   - 盒体是以 Placement 为中心的单位立方体, 局域范围 [-0.5, 0.5]^3, 再乘 Placement.Scale
    ///     (即 Scale 直接等于盒体的三个边长, 单位: 米)。
    ///   - 投影方向 = 局域 +Z。落在盒内的场景像素按局域 XY 采样贴花材质
    ///     (材质图看到的 UV = (local.x + 0.5, 0.5 - local.y))。
    ///   - 因此"贴在地面上"的用法是把节点旋转成 +Z 朝下 (绕 X 轴 +90 度) ——
    ///     新建的贴花已经默认就是这个朝向, 见 <see cref="DefaultProjectionQuat"/>。
    ///
    /// 贴花的视觉全部由 <see cref="TtDecalNodeData.DecalMaterial"/> (RenderLayer=RL_Decal 的材质)
    /// 定义, 本节点只提供投影几何与淡出参数。本节点本身不产生 drawcall,
    /// 只把自己登记进可见节点列表 (IsForceGatherNode = true),
    /// 由 Graphics.Pipeline.Deferred.TtDecalPassNode 每帧收集并直写 GBuffer。
    /// </summary>
    [Bricks.CodeBuilder.ContextMenu("Decal", "Graphics\\Decal", TtNode.EditorKeyword)]
    [TtNode(NodeDataType = typeof(TtDecalNode.TtDecalNodeData), DefaultNamePrefix = "Decal")]
    [Rtti.Meta("")]
    public partial class TtDecalNode : TtVisual
    {
        public override void Dispose()
        {
            CoreSDK.DisposeObject(ref mDebugMesh);
            CoreSDK.DisposeObject(ref mProjectionMesh);
            CoreSDK.DisposeObject(ref mDirectionMesh);
            CoreSDK.DisposeObject(ref mDecalCBuffer);
            base.Dispose();
        }

        /// <summary>
        /// 新建贴花的默认朝向: 绕 X 轴 +90 度, 让投影方向 (局域 +Z) 指向世界 -Y (朝下)。
        ///
        /// 贴花绝大多数用在地面 / 地板上, 而 identity 朝向的投影方向是水平的 +Z ——
        /// 此时地面法线与投影方向夹角接近 90 度, 会被 <c>DecalPS.cginc</c> 的角度淡出
        /// (<see cref="TtDecalNodeData.AngleFadeDegree"/>, 默认 80 度) 整片 discard,
        /// 而且没有任何日志或报错 —— 是最难自查的一种坑。
        /// 默认朝下让"新建一个贴花贴到地上"开箱可用。
        /// </summary>
        public static readonly Quaternion DefaultProjectionQuat =
            Quaternion.RotationAxis(Vector3.Right, MathF.PI * 0.5f);

        #region 逐贴花 cbuffer
        NxRHI.TtCbView mDecalCBuffer;

        /// <summary>
        /// 逐贴花的 <c>cbDecal</c>。每个贴花有自己的 box mesh -> 自己的 atom -> 自己的 drawcall,
        /// 所以这份 cbuffer 天然满足 CodingGuidelines §1.3 (同帧多 drawcall 不能共享 CBV)。
        ///
        /// §1.1: 首次 CreateCBV 必须填满所有字段 + MarkDirty + FlushDirty。
        /// </summary>
        internal NxRHI.TtCbView GetOrCreateDecalCBuffer(NxRHI.FEffectBinder binder, float globalIntensity)
        {
            if (mDecalCBuffer == null)
            {
                mDecalCBuffer = TtEngine.Instance.GfxDevice.RenderContext.CreateCBV(binder);
                FillDecalCBuffer(mDecalCBuffer, globalIntensity);
                mDecalCBuffer.MarkDirty();
                mDecalCBuffer.FlushDirty();
                return mDecalCBuffer;
            }
            FillDecalCBuffer(mDecalCBuffer, globalIntensity);
            return mDecalCBuffer;
        }

        void FillDecalCBuffer(NxRHI.TtCbView cb, float globalIntensity)
        {
            var data = DecalData;
            if (data == null)
                return;

            // 颜色 / 法线 / 材质三组通道各自的混合权重。
            // 颜色与 PBR 参数本身来自贴花材质图 (mtl.mAlbedo / mRough / ...),
            // 这里的权重只控制"改多少", 与 UE 的 DecalBlendMode 权重语义一致。
            cb.SetValue("DecalColorWeight", Math.Clamp(data.ColorWeight, 0.0f, 1.0f));
            cb.SetValue("DecalNormalWeight", Math.Clamp(data.NormalWeight, 0.0f, 1.0f));
            cb.SetValue("DecalMaterialWeight", Math.Clamp(data.MaterialWeight, 0.0f, 1.0f));
            cb.SetValue("DecalGlobalIntensity", Math.Clamp(globalIntensity, 0.0f, 1.0f));

            cb.SetValue("DecalAngleFadeCos",
                MathF.Cos(Math.Clamp(data.AngleFadeDegree, 0.0f, 89.9f) * MathF.PI / 180.0f));
            cb.SetValue("DecalEdgeFade", Math.Clamp(data.EdgeFade, 0.0f, 0.5f));

            cb.SetValue("DecalFadeStart", data.FadeStartDistance);
            cb.SetValue("DecalFadeEnd", data.FadeEndDistance);
        }
        #endregion

        [Rtti.Meta("")]
        public class TtDecalNodeData : TtNodeData
        {
            internal TtDecalNode HostNode;

            #region Material
            RName mDecalMaterial;
            /// <summary>
            /// 贴花材质 (.material, RenderLayer 需为 RL_Decal)。颜色 / 法线 / PBR 参数全部由材质图产生,
            /// 对应 UE 的 MaterialDomain=Deferred Decal。为空时贴花不渲染。
            /// </summary>
            [Rtti.Meta("")]
            [Category("Material")]
            [RName.PGRName(FilterExts = Graphics.Pipeline.Shader.TtMaterial.AssetExt)]
            public RName DecalMaterial
            {
                get => mDecalMaterial;
                set
                {
                    if (mDecalMaterial == value)
                        return;
                    mDecalMaterial = value;
                    MaterialInstance = null;
                    mMaterialLoadState = EMaterialLoadState.Dirty;
                }
            }
            #endregion

            #region Blend
            /// <summary>base color 通道的混合权重 (0 = 不改颜色)。</summary>
            [Rtti.Meta("")]
            [Category("Blend")]
            public float ColorWeight { get; set; } = 1.0f;

            /// <summary>
            /// 法线通道的混合权重 (0 = 不改法线)。
            /// 仅在材质 <c>DecalMode == WithNormal</c> 时生效。
            /// </summary>
            [Rtti.Meta("")]
            [Category("Blend")]
            public float NormalWeight { get; set; } = 1.0f;

            /// <summary>Roughness / Metallic / Specular 通道的混合权重 (0 = 不改)。</summary>
            [Rtti.Meta("")]
            [Category("Blend")]
            public float MaterialWeight { get; set; } = 0.0f;

            /// <summary>
            /// 排序序号。同一像素上数值小的先投影、数值大的后投影 (后者盖在前者上面)。
            /// </summary>
            [Rtti.Meta("")]
            [Category("Blend")]
            public int SortOrder { get; set; } = 0;
            #endregion

            #region Fade
            /// <summary>
            /// 法线夹角淡出阈值 (角度)。表面法线与投影方向反向夹角超过该值的像素不接受贴花,
            /// 避免贴花在侧壁上被拉伸。
            /// </summary>
            [Rtti.Meta("")]
            [Category("Fade")]
            public float AngleFadeDegree { get; set; } = 80.0f;

            /// <summary>盒体边缘淡出比例 (0~0.5, 局域单位)。0 = 硬边。</summary>
            [Rtti.Meta("")]
            [Category("Fade")]
            public float EdgeFade { get; set; } = 0.1f;

            /// <summary>开始按距离淡出的距离 (米)。</summary>
            [Rtti.Meta("")]
            [Category("Fade")]
            public float FadeStartDistance { get; set; } = 60.0f;

            /// <summary>完全淡出的距离 (米)。</summary>
            [Rtti.Meta("")]
            [Category("Fade")]
            public float FadeEndDistance { get; set; } = 80.0f;
            #endregion

            #region Runtime material loading
            enum EMaterialLoadState
            {
                Dirty,
                Loading,
                Done,
            }
            EMaterialLoadState mMaterialLoadState = EMaterialLoadState.Dirty;

            internal Graphics.Pipeline.Shader.TtMaterial MaterialInstance;

            /// <summary>
            /// 由渲染节点每帧调用: 材质 RName 变化过就异步补加载一次。
            /// 加载完成前 ProjectionMesh 返回 null, 贴花暂不渲染。
            /// </summary>
            internal void EnsureMaterial()
            {
                if (mMaterialLoadState != EMaterialLoadState.Dirty)
                    return;
                if (mDecalMaterial == null)
                {
                    mMaterialLoadState = EMaterialLoadState.Done;
                    return;
                }
                mMaterialLoadState = EMaterialLoadState.Loading;
                LoadMaterial(mDecalMaterial).AddWaitTask();
            }

            async Thread.Async.TtTask LoadMaterial(RName request)
            {
                var material = await request.GetAsset<Graphics.Pipeline.Shader.TtMaterial>();
                if (material != null && material.RenderLayer != Graphics.Pipeline.ERenderLayer.RL_Decal)
                {
                    // 防错: 普通材质的 effect 没有 ENV_DECAL_WRITE_NORMAL 语义,
                    // 挂上来只会画出一个实心盒子
                    Profiler.Log.WriteLine<Profiler.TtGraphicsGategory>(Profiler.ELogTag.Warning,
                        $"[Decal] {request} 的 RenderLayer 不是 RL_Decal, 贴花不渲染");
                    material = null;
                }
                if (mDecalMaterial != request)
                {
                    // 加载期间 RName 又被换掉: 丢弃本次结果, 保持 Dirty 下一帧重载
                    mMaterialLoadState = EMaterialLoadState.Dirty;
                    return;
                }
                MaterialInstance = material;
                mMaterialLoadState = EMaterialLoadState.Done;
            }
            #endregion
        }

        /// <summary>
        /// 强类型 NodeData 访问器, 给引擎代码用 (渲染节点取材质 / FillDecalCBuffer 填参数)。
        ///
        /// 不在 Details 面板里显示: PropertyGrid 对这种嵌套对象属性只画出一行
        /// 不可展开编辑的值, 编辑器改参数一律走下面 #region 编辑器属性 的包装属性
        /// (见 CodingGuidelines §9)。
        /// </summary>
        [Browsable(false)]
        public TtDecalNodeData DecalData
        {
            get => GetNodeData<TtDecalNodeData>();
        }

        #region 编辑器属性 (转发到 TtDecalNodeData, 规范见 CodingGuidelines §9)
        // 这些属性只是 NodeData 字段在 Details 面板上的入口: 数据本体与 [Rtti.Meta]
        // 序列化标记都在 TtDecalNodeData 上, 这里 **不能** 再加 [Rtti.Meta], 否则同一份
        // 值会被序列化两遍。
        //
        // Blend / Fade 两组是纯转发 —— cbDecal 由 FillDecalCBuffer 每帧重灌,
        // SortOrder 由 TtDecalPassNode 每帧收集时重排, 所以改完下一帧自然生效,
        // 不需要额外的状态刷新。

        /// <summary>见 <see cref="TtDecalNodeData.DecalMaterial"/>。</summary>
        [Category("Material")]
        [RName.PGRName(FilterExts = Graphics.Pipeline.Shader.TtMaterial.AssetExt)]
        public RName DecalMaterial
        {
            get => DecalData.DecalMaterial;
            set
            {
                // 换材质需要的状态刷新已经在 NodeData 的 setter 里: 它把 MaterialInstance
                // 置空并标记 Dirty, 渲染节点下一帧 EnsureMaterial 补加载; ProjectionMesh
                // 发现材质对象变了会自己重建 mesh。这里只做转发。
                DecalData.DecalMaterial = value;
            }
        }

        /// <summary>见 <see cref="TtDecalNodeData.ColorWeight"/>。</summary>
        [Category("Blend")]
        public float ColorWeight
        {
            get => DecalData.ColorWeight;
            set => DecalData.ColorWeight = value;
        }

        /// <summary>见 <see cref="TtDecalNodeData.NormalWeight"/>。</summary>
        [Category("Blend")]
        public float NormalWeight
        {
            get => DecalData.NormalWeight;
            set => DecalData.NormalWeight = value;
        }

        /// <summary>见 <see cref="TtDecalNodeData.MaterialWeight"/>。</summary>
        [Category("Blend")]
        public float MaterialWeight
        {
            get => DecalData.MaterialWeight;
            set => DecalData.MaterialWeight = value;
        }

        /// <summary>见 <see cref="TtDecalNodeData.SortOrder"/>。</summary>
        [Category("Blend")]
        public int SortOrder
        {
            get => DecalData.SortOrder;
            set => DecalData.SortOrder = value;
        }

        /// <summary>见 <see cref="TtDecalNodeData.AngleFadeDegree"/>。</summary>
        [Category("Fade")]
        public float AngleFadeDegree
        {
            get => DecalData.AngleFadeDegree;
            set => DecalData.AngleFadeDegree = value;
        }

        /// <summary>见 <see cref="TtDecalNodeData.EdgeFade"/>。</summary>
        [Category("Fade")]
        public float EdgeFade
        {
            get => DecalData.EdgeFade;
            set => DecalData.EdgeFade = value;
        }

        /// <summary>见 <see cref="TtDecalNodeData.FadeStartDistance"/>。</summary>
        [Category("Fade")]
        public float FadeStartDistance
        {
            get => DecalData.FadeStartDistance;
            set => DecalData.FadeStartDistance = value;
        }

        /// <summary>见 <see cref="TtDecalNodeData.FadeEndDistance"/>。</summary>
        [Category("Fade")]
        public float FadeEndDistance
        {
            get => DecalData.FadeEndDistance;
            set => DecalData.FadeEndDistance = value;
        }
        #endregion

        public override void AddAssetReferences(IO.IAssetMeta ameta)
        {
            base.AddAssetReferences(ameta);
            var material = DecalData?.DecalMaterial;
            if (material != null)
                ameta.AddReferenceAsset(material);
        }

        protected override async Thread.Async.TtTask<bool> InitializeNode(GamePlay.TtWorld world, TtNodeData data, EBoundVolumeType bvType, Type placementType)
        {
            // Placement 为空 => 全新创建 (右键菜单 / AddDecalNode);
            // 非空 => 从场景资产反序列化, 存盘的旋转必须原样保留, 绝不能被默认朝向覆盖
            bool isNewCreate = (data == null || data.Placement == null);

            if (data == null)
            {
                data = new TtDecalNodeData();
            }

            var ret = await base.InitializeNode(world, data, bvType, placementType);
            GetNodeData<TtDecalNodeData>().HostNode = this;
            // 单位盒 (局域 [-0.5,0.5]^3), 实际尺寸由 Placement.Scale 决定
            this.BoundVolume.LocalAABB = new BoundingBox(Vector3.Zero, 1.0f);

            // 贴花不参与 mesh 剔除, 但必须进可见节点列表, 否则渲染节点收集不到
            this.IsForceGatherNode = true;

            if (isNewCreate && Placement != null)
            {
                // 只改朝向, 不动 base 刚设好的位置与缩放
                Placement.Quat = DefaultProjectionQuat;
            }
            return ret;
        }

        protected override async Thread.Async.TtTask OnPostInitNode(TtNode parent, object extArg)
        {
            await base.OnPostInitNode(parent, extArg);
            this.BoundVolume.LocalAABB = new BoundingBox(Vector3.Zero, 1.0f);
            InitDirectionMesh();
            UpdateAbsTransform();
        }

        public static async Thread.Async.TtTask<TtDecalNode> AddDecalNode(TtWorld world, TtNode parent, TtDecalNodeData data, DVector3 pos, Vector3 boxSize, Quaternion? quat = null)
        {
            var node = await GamePlay.Scene.TtNode.SpawnNode<TtDecalNode>(parent, null, data, EBoundVolumeType.Box, typeof(TtPlacement)) as TtDecalNode;
            if (node != null)
            {
                // 不传朝向就用默认的"+Z 朝下", 与右键新建的贴花保持一致
                var rot = quat ?? DefaultProjectionQuat;
                node.Placement.SetTransform(in pos, in boxSize, in rot);
            }
            return node;
        }

        Graphics.Mesh.TtRenderMesh mProjectionMesh;
        Graphics.Pipeline.Shader.TtMaterial mProjectionMeshMaterial;
        /// <summary>
        /// 投影盒的实体 mesh (局域 [-0.5,0.5]^3)。由贴花 pass 光栅化它来圈定影响范围。
        ///
        /// 材质就是 <see cref="TtDecalNodeData.DecalMaterial"/> (RL_Decal 域) —— 贴花的
        /// 颜色 / 法线 / PBR 参数全部由材质图产生, 对应 UE 的
        /// MaterialDomain=Deferred Decal。材质未就绪时返回 null, 贴花暂不渲染。
        ///
        /// 注意: 它 <b>不能</b> 进可见 mesh 列表 (<c>rp.AddVisibleMesh</c>) —— 否则 BasePass 会把它
        /// 当普通不透明物体画成一个实心盒子。贴花 pass 是通过 <c>VisibleNodes</c> 里的本节点
        /// 直接取这个 mesh 的。
        ///
        /// 同一 RName 的 GetAsset 返回缓存的同一材质对象, 编辑器里改材质保存只加 SerialId,
        /// atom 会自动重建 drawcall, 无需重建 mesh; 材质对象变化 (换材质) 才重建。
        /// </summary>
        public Graphics.Mesh.TtRenderMesh ProjectionMesh
        {
            get
            {
                var material = DecalData?.MaterialInstance;
                if (material == null)
                    return null;
                if (mProjectionMesh != null && mProjectionMeshMaterial == material)
                    return mProjectionMesh;

                var boxProvider = Graphics.Mesh.TtMeshDataProvider.MakeBox(
                    -0.5f, -0.5f, -0.5f, 1.0f, 1.0f, 1.0f);
                var cookedMesh = boxProvider.ToMesh();
                var materials = new Graphics.Pipeline.Shader.TtMaterial[1];
                materials[0] = material;
                var mesh = new Graphics.Mesh.TtRenderMesh();
                var ok = mesh.Initialize(cookedMesh, materials,
                    Rtti.TtTypeDescGetter<Graphics.Mesh.TtMdfStaticMesh>.TypeDesc);
                if (ok)
                {
                    mesh.IsAcceptShadow = false;
                    mesh.IsCastShadow = false;
                    mesh.IsDrawHitproxy = false;
                    // 贴花 pass 在 OnDrawCall 里通过 atom.RenderMesh.HostNode 回溯到本节点取参数
                    mesh.HostNode = this;
                    mProjectionMesh?.Dispose();
                    mProjectionMesh = mesh;
                    mProjectionMeshMaterial = material;

                    UpdateAbsTransform();
                }
                return mProjectionMesh;
            }
        }

        Graphics.Mesh.TtRenderMesh mDebugMesh;
        /// <summary>
        /// 编辑器里用于显示投影盒范围的线框, 不参与 hitproxy 与阴影。
        /// </summary>
        public Graphics.Mesh.TtRenderMesh DebugMesh
        {
            get
            {
                if (mDebugMesh == null)
                {
                    var wireProvider = Graphics.Mesh.TtMeshDataProvider.MakeBoxWireframe(
                        -0.5f, -0.5f, -0.5f, 1.0f, 1.0f, 1.0f, 0xff00ffff);
                    var cookedMesh = wireProvider.ToMesh();
                    var materials = new Graphics.Pipeline.Shader.TtMaterialInstance[1];
                    materials[0] = TtEngine.Instance.GfxDevice.MaterialInstanceManager.WireColorMateria.CloneMaterialInstance();
                    var mesh = new Graphics.Mesh.TtRenderMesh();
                    var ok = mesh.Initialize(cookedMesh, materials,
                        Rtti.TtTypeDescGetter<Graphics.Mesh.TtMdfStaticMesh>.TypeDesc);
                    if (ok)
                    {
                        mesh.IsAcceptShadow = false;
                        mesh.IsDrawHitproxy = false;
                        mesh.HostNode = this;
                        mDebugMesh = mesh;

                        UpdateAbsTransform();
                        UpdateAABB();
                        Parent?.UpdateAABB();
                    }
                }
                return mDebugMesh;
            }
        }

        Graphics.Mesh.TtRenderMesh mDirectionMesh;
        /// <summary>
        /// 投影方向指示箭头 (沿局域 +Z, 箭尖指向投影方向), 同时承载编辑器拾取的 hitproxy id。
        ///
        /// 一个 mesh 兼两职是有意的:
        ///   - 贴花没有可拾取的实体外观: 投影盒只在贴花 pass 里光栅化, 不进 hitproxy pass;
        ///     线框盒 (<see cref="DebugMesh"/>) 只有 1px 宽的边, 鼠标几乎不可能点中。
        ///   - 贴花最容易踩的坑是投影方向不对 (整片被角度淡出 discard, 且无任何报错),
        ///     所以最该显式画出来的就是这个方向。
        /// 让方向指示兼任拾取体, 既省一个 mesh, 又保证"能点中的地方"就是"方向所指的地方"。
        ///
        /// <b>不跟随 Placement.Scale</b>: 贴花常被压成薄片 (Scale.z 很小),
        /// 跟随缩放会把箭头一起压扁到点不中。
        /// </summary>
        void InitDirectionMesh()
        {
            if (mDirectionMesh != null)
                return;

            var arrowProvider = MakeDirectionArrow();
            if (arrowProvider == null)
                return;

            var mtl = TtEngine.Instance.GfxDevice.MaterialInstanceManager.WhiteColorMaterial?.CloneMaterialInstance();
            if (mtl == null)
                return;
            // 必须是实心的: WireColorMateria 是线框填充, 1px 的边同样点不中。
            // 双面 + 与线框盒同层, 保证在编辑器视口里可见
            var rast = mtl.Rasterizer;
            rast.CullMode = NxRHI.ECullMode.CMD_NONE;
            mtl.Rasterizer = rast;
            mtl.RenderLayer = Graphics.Pipeline.ERenderLayer.RL_Translucent;

            var materials = new Graphics.Pipeline.Shader.TtMaterial[1];
            materials[0] = mtl;
            var mesh = new Graphics.Mesh.TtRenderMesh();
            var ok = mesh.Initialize(arrowProvider.ToMesh(), materials,
                Rtti.TtTypeDescGetter<Graphics.Mesh.TtMdfStaticMesh>.TypeDesc);
            if (ok)
            {
                mesh.IsAcceptShadow = false;
                mesh.IsCastShadow = false;
                mDirectionMesh = mesh;
                mDirectionMesh.HostNode = this;

                // 必须等 mDirectionMesh 就绪后再设: setter 会 MapProxy 分配 id 并同步回调
                // OnHitProxyChanged, 那里要拿到 mesh 才能把 id 写进去
                this.HitproxyType = Graphics.Pipeline.TtHitProxy.EHitproxyType.Root;

                UpdateAbsTransform();
                UpdateAABB();
                Parent?.UpdateAABB();
            }
        }
        /// <summary>
        /// 拼一个沿 +Z、从盒体中心向外指的箭头: 细柱身 + 粗锥头, 合成单个 atom。
        ///
        /// <see cref="Graphics.Mesh.TtMeshDataProvider.MakeCylinder"/> 的主轴是 <b>Y 轴</b>
        /// (函数末尾把顶点 Y/Z 对调了), 而贴花的投影轴是局域 +Z, 所以两段都要
        /// 先 <c>RotationX(+90°)</c> 把主轴 Y → Z, 再平移到各自的区间 (柱身 z ∈ [0, 0.3],
        /// 锥头 z ∈ [0.3, 0.45], 尖端落在 +Z 端)。
        /// </summary>
        static Graphics.Mesh.TtMeshDataProvider MakeDirectionArrow()
        {
            const float shaftRadius = 0.02f;
            const float shaftLength = 0.3f;
            const float headRadius = 0.06f;
            const float headLength = 0.15f;
            const uint arrowColor = 0xff00ffff;

            // MakeCylinder 沿主轴居中, 所以转完轴还要把各段平移到自己的区间
            var yToZ = Matrix.RotationX(MathF.PI * 0.5f);

            var shaft = Graphics.Mesh.TtMeshDataProvider.MakeCylinder(
                shaftRadius, shaftRadius, shaftLength, 8, 1, arrowColor);
            if (shaft == null)
                return null;
            shaft.ApplyTransform(yToZ * Matrix.Translate(0.0f, 0.0f, shaftLength * 0.5f));

            var head = Graphics.Mesh.TtMeshDataProvider.MakeCylinder(
                headRadius, 0.0f, headLength, 12, 1, arrowColor);
            if (head == null)
                return shaft;
            head.ApplyTransform(yToZ * Matrix.Translate(0.0f, 0.0f, shaftLength + headLength * 0.5f));

            // appendToLastAtom: 柱身与锥头共用一个材质, 不能让它变成两个 atom ——
            // 否则 TtRenderMesh.Initialize 得接收两份材质数组。
            // shaft 是 MakeCylinder 出来的单 atom 单 LOD, 满足"最后一个 atom 收尾于索引末尾"的前提
            shaft.MergeFromMesh(head, true);
            return shaft;
        }

        public override void GetHitProxyDrawMesh(List<Graphics.Mesh.TtRenderMesh> meshes)
        {
            if (mDirectionMesh != null)
                meshes.Add(mDirectionMesh);
        }

        public override void OnGatherVisibleMeshes(TtWorld.TtVisParameter rp)
        {
            // 贴花必须无条件进可见节点列表, 否则 TtDecalPassNode 拿不到可见贴花集
            rp.AddVisibleNode(this);

            if ((rp.CullFilters & GamePlay.TtWorld.TtVisParameter.EVisCullFilter.LightDebug) != 0)
            {
                if (DebugMesh != null)
                    rp.AddVisibleMesh(mDebugMesh);
            }

            if ((rp.CullFilters & GamePlay.TtWorld.TtVisParameter.EVisCullFilter.UtilityEditor) != 0)
            {
                if (mDirectionMesh != null)
                    rp.AddVisibleMesh(mDirectionMesh);
            }
        }

        protected override void OnAbsTransformChanged()
        {
            var world = this.GetWorld();
            if (mDebugMesh != null)
                mDebugMesh.SetWorldTransform(in Placement.AbsTransform, world, false);
            if (mProjectionMesh != null)
                mProjectionMesh.SetWorldTransform(in Placement.AbsTransform, world, false);
            if (mDirectionMesh != null)
            {
                // 跟位置与朝向 (朝向正是它要指示的东西), 但不跟缩放:
                // isNoScale = true, 原因见 InitDirectionMesh 注释
                mDirectionMesh.SetWorldTransform(in Placement.AbsTransform, world, true);
            }
        }

        public override void OnHitProxyChanged()
        {
            if (mDirectionMesh == null)
                return;
            if (this.HitProxy == null)
            {
                mDirectionMesh.IsDrawHitproxy = false;
                return;
            }
            if (HitproxyType != Graphics.Pipeline.TtHitProxy.EHitproxyType.None)
            {
                mDirectionMesh.IsDrawHitproxy = true;
                var value = HitProxy.ConvertHitProxyIdToVector4();
                mDirectionMesh.SetHitproxy(in value);
            }
            else
            {
                mDirectionMesh.IsDrawHitproxy = false;
            }
        }

        public override bool IsAcceptShadow
        {
            get => false;
            set { }
        }
        public override bool IsCastShadow
        {
            get => false;
            set { }
        }
    }
}
