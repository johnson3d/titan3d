using EngineNS.GamePlay;
using EngineNS.GamePlay.Scene;
using EngineNS.Graphics.Mesh;
using EngineNS.Graphics.Pipeline;
using EngineNS.Graphics.Pipeline.Common;
using EngineNS.Graphics.Pipeline.Shader;
using EngineNS.NxRHI;
using System;
using System.ComponentModel;

namespace EngineNS.Bricks.FX.Water
{
    /// <summary>
    /// GPU 浅水方程水面场景节点。
    /// 内部持有一个迷你 RenderPolicy (只含 TtSWEComputeNode), 每帧 dispatch 一次
    /// 浅水方程 Compute Shader 更新高度图; 渲染用一个平整平面网格, 材质中通过
    /// mVertexOffset 采样高度图做顶点位移。
    /// </summary>
    [Bricks.CodeBuilder.ContextMenu("SWEWater", "Graphics\\SWEWater", TtNode.EditorKeyword)]
    [TtNode(NodeDataType = typeof(TtSWEWaterNodeData), DefaultNamePrefix = "SWEWater")]
    public class TtSWEWaterNode : GamePlay.Scene.TtVisual
    {
        // ---------- NodeData ----------
        [Rtti.Meta]
        public class TtSWEWaterNodeData : TtNodeData
        {
            [Rtti.Meta]
            [Category("SWE")]
            public uint SimResolution { get; set; } = 128;

            [Rtti.Meta]
            [Category("SWE")]
            public float DomainSize { get; set; } = 128.0f;

            [Rtti.Meta]
            [Category("SWE")]
            public float Gravity { get; set; } = 9.81f;

            [Rtti.Meta]
            [Category("SWE")]
            public float Damping { get; set; } = 0.995f;

            [Rtti.Meta]
            [Category("SWE")]
            public ESWEBoundaryMode BoundaryMode { get; set; } = ESWEBoundaryMode.Open;

            [Rtti.Meta]
            [Category("SWE")]
            public float BaseWaterHeight { get; set; } = 1.0f;

            /// <summary>
            /// 动态 SRV 注册名。材质采样节点的 DynamicSrvName 填写此值即可采样 HeightMap。
            /// </summary>
            [Rtti.Meta]
            [Category("SWE")]
            public string HeightMapSrvName { get; set; } = "SWEHeightMap";

            [Rtti.Meta]
            [Category("Material")]
            [RName.PGRName(FilterExts = Graphics.Pipeline.Shader.TtMaterial.AssetExt)]
            public RName WaterMaterialName { get; set; }
        }

        // ---------- Internal ----------
        TtRenderPolicy mSWEPolicy;
        TtSWEComputeNode mComputeNode;
        TtRCmdQueue mCmdQueue;
        TtRenderMesh mMesh;
        bool mInitialized = false;
        /// <summary>
        /// 外部获取当前高度图 SRV, 用于材质参数绑定
        /// </summary>
        public TtSrView HeightMapSrv => mComputeNode?.HeightMapSrv;

        public override void Dispose()
        {
            // 从动态 SRV 注册表注销
            var sweData = NodeData as TtSWEWaterNodeData;
            if (sweData != null && !string.IsNullOrEmpty(sweData.HeightMapSrvName))
                TtEngine.Instance.GfxDevice.DynamicSrvRegistry.Unregister(sweData.HeightMapSrvName);

            CoreSDK.DisposeObject(ref mMesh);
            if (mSWEPolicy != null)
            {
                mSWEPolicy.Dispose();
                mSWEPolicy = null;
            }
            mComputeNode = null;
            mCmdQueue = null;
            base.Dispose();
        }

        protected override async Thread.Async.TtTask<bool> InitializeNode(
            TtWorld world, TtNodeData data, EBoundVolumeType bvType, Type placementType)
        {
            if (data as TtSWEWaterNodeData == null)
                data = new TtSWEWaterNodeData();

            if (await base.InitializeNode(world, data, bvType, placementType) == false)
                return false;

            var sweData = data as TtSWEWaterNodeData;

            this.IsAcceptShadow = false;
            this.IsCastShadow = false;

            await InitSWEPolicy(sweData);
            BuildPlaneMesh(sweData);
            await ApplyMaterial(sweData);

            mInitialized = true;
            return true;
        }

        // =====================================================================
        // 迷你 RenderPolicy 构造 (不走 .rpolicy 资产)
        // =====================================================================
        async Thread.Async.TtTask InitSWEPolicy(TtSWEWaterNodeData sweData)
        {
            mSWEPolicy = new TtRenderPolicy();
            mCmdQueue = new TtRCmdQueue();
            mSWEPolicy.CmdQueue = mCmdQueue;

            // 创建 compute 节点
            mComputeNode = new TtSWEComputeNode();
            mComputeNode.InitNodePins();
            mComputeNode.SimResolution = sweData.SimResolution;
            mComputeNode.DomainSize = sweData.DomainSize;
            mComputeNode.Gravity = sweData.Gravity;
            mComputeNode.Damping = sweData.Damping;
            mComputeNode.BoundaryMode = sweData.BoundaryMode;
            mComputeNode.BaseWaterHeight = sweData.BaseWaterHeight;

            // 创建 ending 节点 (RenderGraph 必须有 RootNode)
            // 用 TtAssitRootNode (IsMainRoot=false 不影响, 它有 SrcPinIn 可以接 linker)
            var endingNode = new TtAssitRootNode();
            endingNode.InitNodePins();
            endingNode.Name = "SWEEnding";

            // 注册节点到 RenderGraph
            mSWEPolicy.RegRenderNode2("SWECompute", mComputeNode);
            mSWEPolicy.RegRenderNode2("SWEEnding", endingNode);

            // AddLinker: ComputeNode.ResultPinOut -> EndingNode.SrcPinIn
            // 否则 BuildGraph 从 RootNode 回溯找不到 ComputeNode, IsUsed=false, Tick 被跳过
            mSWEPolicy.AddLinker(mComputeNode.ResultPinOut, endingNode.SrcPinIn);
            mSWEPolicy.RootNode = endingNode;

            bool hasInputError = false;
            mSWEPolicy.BuildGraph(ref hasInputError);

            // 初始化所有节点 (会触发 TtSWEComputeNode.Initialize, 创建 shading + 纹理)
            foreach (var kvp in mSWEPolicy.GraphNodes)
            {
                await kvp.Value.Initialize(mSWEPolicy, kvp.Value.Name);
            }

            // 向全局动态 SRV 注册表注册 HeightMap, 材质采样节点通过 DynamicSrvName 查找
            if (!string.IsNullOrEmpty(sweData.HeightMapSrvName) && mComputeNode.HeightMapSrv != null)
            {
                TtEngine.Instance.GfxDevice.DynamicSrvRegistry.Register(
                    sweData.HeightMapSrvName,
                    mComputeNode.HeightMapSrv,
                    sweData.SimResolution, sweData.SimResolution,
                    EPixelFormat.PXF_R32G32B32A32_FLOAT,
                    this);
            }
        }

        // =====================================================================
        // 创建平整平面网格 (顶点位移由材质 mVertexOffset 驱动)
        // =====================================================================
        unsafe void BuildPlaneMesh(TtSWEWaterNodeData sweData)
        {
            uint resolution = sweData.SimResolution;
            float domainSize = sweData.DomainSize;

            var meshBuilder = new TtMeshDataProvider();
            meshBuilder.AssetName = RName.GetRName("@SWEWaterPlane", RName.ERNameType.Transient);
            var builder = meshBuilder.mCoreObject;

            uint streams = (uint)((1 << (int)EVertexStreamType.VST_Position) |
                (1 << (int)EVertexStreamType.VST_Normal) |
                (1 << (int)EVertexStreamType.VST_Color) |
                (1 << (int)EVertexStreamType.VST_UV));
            builder.Init(streams, false, 1);

            // 生成顶点: resolution x resolution 的平面网格
            for (uint row = 0; row < resolution; row++)
            {
                for (uint col = 0; col < resolution; col++)
                {
                    float x = (float)col / (resolution - 1) * domainSize - domainSize * 0.5f;
                    float z = (float)row / (resolution - 1) * domainSize - domainSize * 0.5f;
                    var position = new Vector3(x, 0, z);
                    var normal = Vector3.Up;
                    var uv = new Vector2((float)col / (resolution - 1), (float)row / (resolution - 1));
                    builder.AddVertex(position, normal, uv, 0xFFFFFFFF);
                }
            }

            // 生成三角形索引
            for (uint row = 0; row < resolution - 1; row++)
            {
                for (uint col = 0; col < resolution - 1; col++)
                {
                    uint topLeft = row * resolution + col;
                    uint topRight = topLeft + 1;
                    uint bottomLeft = (row + 1) * resolution + col;
                    uint bottomRight = bottomLeft + 1;

                    builder.AddTriangle(topLeft, bottomLeft, topRight);
                    builder.AddTriangle(topRight, bottomLeft, bottomRight);
                }
            }

            var dpDesc = new FMeshAtomDesc();
            dpDesc.SetDefault();
            dpDesc.NumPrimitives = (resolution - 1) * (resolution - 1) * 2;
            builder.PushAtomLOD(0, &dpDesc);
            builder.CalcAABB();

            var cookedMesh = meshBuilder.ToMesh();
            var materials = new Graphics.Pipeline.Shader.TtMaterialInstance[1];
            materials[0] = TtEngine.Instance.GfxDevice.MaterialInstanceManager.WireColorMateria;

            mMesh = new TtRenderMesh();
            mMesh.Initialize(cookedMesh, materials,
                Rtti.TtTypeDescGetter<TtMdfStaticMesh>.TypeDesc);
            mMesh.IsAcceptShadow = false;
        }

        async Thread.Async.TtTask ApplyMaterial(TtSWEWaterNodeData sweData)
        {
            if (sweData.WaterMaterialName == null)
                return;

            var material = await sweData.WaterMaterialName.GetAsset<Graphics.Pipeline.Shader.TtMaterial>();
            if (material == null)
                return;

            if (mMesh != null && mMesh.MaterialMesh != null && mMesh.MaterialMesh.SubMeshes.Count > 0)
            {
                var matInstance = Graphics.Pipeline.Shader.TtMaterialInstance.CreateMaterialInstance(material);
                if (matInstance != null)
                {
                    mMesh.MaterialMesh.SubMeshes[0].Materials[0] = matInstance;
                }
            }
        }

        // =====================================================================
        // Tick: 刷新 SWE + 同步 mesh transform
        // =====================================================================
        public override bool OnTickLogic(TtNodeTickParameters args)
        {
            if (!mInitialized || mSWEPolicy == null || mComputeNode == null)
                return true;

            // 驱动迷你 RenderPolicy 做一次 SWE compute dispatch
            // SRV 注册也放在 RenderThread 里, 在 dispatch 完成、ping-pong 交换后立即更新,
            // 避免逻辑线程和渲染线程之间 HeightMapSrv 指向不一致导致闪烁
            TtEngine.Instance.ThreadRender.QueueRenderAction("SWEUpdate", static (in Thread.TtThreadRender.FRenderAction RAct) =>
            {
                var This = (RAct.Arg as TtSWEWaterNode);
                This.mSWEPolicy.QueueCmd((TtRCmdQueue queue, ref FRCmdInfo info) =>
                {
                    TtEngine.Instance.GfxDevice.RenderContext.GpuQueue.BeginEvent($"SWEUpdate");
                }, "Begin:SWEUpdate");

                This.mSWEPolicy?.BeginTick(null);
                This.mSWEPolicy?.Tick(null, null);
                This.mSWEPolicy?.EndTick(null);

                This.mSWEPolicy.QueueCmd((TtRCmdQueue queue, ref FRCmdInfo info) =>
                {
                    TtEngine.Instance.GfxDevice.RenderContext.GpuQueue.EndEvent($"SWEUpdate");
                }, "End:SWEUpdate");
                This.mSWEPolicy.ExecuteCmdQueue(false);

                // dispatch 完成后更新动态 SRV 注册表 (此时 ping-pong 已交换, HeightMapSrv 指向最新结果)
                var sweData = This.NodeData as TtSWEWaterNodeData;
                if (sweData != null && !string.IsNullOrEmpty(sweData.HeightMapSrvName) && This.mComputeNode.HeightMapSrv != null)
                {
                    TtEngine.Instance.GfxDevice.DynamicSrvRegistry.Register(
                        sweData.HeightMapSrvName,
                        This.mComputeNode.HeightMapSrv,
                        sweData.SimResolution, sweData.SimResolution,
                        EPixelFormat.PXF_R32G32B32A32_FLOAT,
                        This);
                }
            }, this);

            // 同步 mesh 的世界变换
            if (mMesh != null)
            {
                var world = this.GetWorld();
                if (world != null)
                    mMesh.SetWorldTransform(in Placement.AbsTransform, world, false);
                else
                    mMesh.SetWorldTransform(in Placement.AbsTransform, null, false);
            }

            return true;
        }

        public override Graphics.Mesh.TtRenderMesh RenderMesh
        {
            get => mMesh;
            set => mMesh = value;
        }
        public override void OnGatherVisibleMeshes(GamePlay.TtWorld.TtVisParameter rp)
        {
            if (mMesh == null)
                return;

            this.CheckDirty();

            rp.AddVisibleMesh(mMesh);
        }
        // =====================================================================
        // 公共 API: 外部注入扰动 (如物体落水)
        // =====================================================================

        /// <summary>
        /// 在水面指定世界坐标位置注入一次扰动
        /// </summary>
        /// <param name="worldX">世界空间 X 坐标</param>
        /// <param name="worldZ">世界空间 Z 坐标</param>
        /// <param name="radius">扰动半径 (米)</param>
        /// <param name="strength">扰动强度 (正=凸起, 负=凹陷)</param>
        public void AddDisturbance(float worldX, float worldZ, float radius, float strength)
        {
            if (mComputeNode == null)
                return;

            var sweData = NodeData as TtSWEWaterNodeData;
            if (sweData == null)
                return;

            float domainSize = sweData.DomainSize;
            uint resolution = sweData.SimResolution;

            // 世界坐标 -> 像素坐标: 水面中心在世界原点
            // 需要考虑节点自身的世界位移
            var nodeWorldPos = Placement.AbsTransform.Position;
            float localX = worldX - (float)nodeWorldPos.X;
            float localZ = worldZ - (float)nodeWorldPos.Z;

            float pixelX = (localX + domainSize * 0.5f) / domainSize * resolution;
            float pixelZ = (localZ + domainSize * 0.5f) / domainSize * resolution;
            float pixelRadius = radius / domainSize * resolution;

            mComputeNode.DisturbPosX = pixelX;
            mComputeNode.DisturbPosZ = pixelZ;
            mComputeNode.DisturbRadius = pixelRadius;
            mComputeNode.DisturbStrength = strength;
            mComputeNode.PendingDisturb = true;
        }

        // =====================================================================
        // 测试用: Detail 面板按钮, 随机位置/半径注入扰动
        // =====================================================================
        public class TtTestDisturbCmd
        {
            internal TtSWEWaterNode HostNode;
            static readonly Random sRandom = new Random();

            public class TtValueEditorAttribute : EGui.Controls.PropertyGrid.TtPGCustomValueEditorAttribute
            {
                public unsafe override bool OnDraw(in EditorInfo info, out object newValue)
                {
                    newValue = info.Value;
                    var cmd = newValue as TtTestDisturbCmd;
                    if (cmd?.HostNode == null)
                        return false;

                    if (ImGuiAPI.Button("Random Disturb"))
                    {
                        var sweData = cmd.HostNode.NodeData as TtSWEWaterNodeData;
                        if (sweData != null)
                        {
                            float Scale = 1.0f;
                            float halfDomain = sweData.DomainSize * 0.5f;
                            var pos = cmd.HostNode.Placement.AbsTransform.Position;
                            float worldX = (float)pos.X + (float)(sRandom.NextDouble() * 2.0 - 1.0) * halfDomain * 0.8f;
                            float worldZ = (float)pos.Z + (float)(sRandom.NextDouble() * 2.0 - 1.0) * halfDomain * 0.8f;
                            float radius = 2.0f + (float)sRandom.NextDouble() * 6.0f * Scale;
                            float strength = 0.5f + (float)sRandom.NextDouble() * 2.0f * Scale;
                            cmd.HostNode.AddDisturbance(worldX, worldZ, radius, strength);
                        }
                    }
                    return false;
                }
            }
        }

        // =====================================================================
        // NodeData 属性包装 — 直接在 Node Detail 面板上设置
        // =====================================================================
        [Category("SWE")]
        public uint SimResolution
        {
            get => (NodeData as TtSWEWaterNodeData)?.SimResolution ?? 128;
            set { var d = NodeData as TtSWEWaterNodeData; if (d != null) d.SimResolution = value; }
        }

        [Category("SWE")]
        public float DomainSize
        {
            get => (NodeData as TtSWEWaterNodeData)?.DomainSize ?? 128.0f;
            set { var d = NodeData as TtSWEWaterNodeData; if (d != null) d.DomainSize = value; }
        }

        [Category("SWE")]
        public float Gravity
        {
            get => (NodeData as TtSWEWaterNodeData)?.Gravity ?? 9.81f;
            set { var d = NodeData as TtSWEWaterNodeData; if (d != null) d.Gravity = value; }
        }

        [Category("SWE")]
        public float Damping
        {
            get => (NodeData as TtSWEWaterNodeData)?.Damping ?? 0.995f;
            set { var d = NodeData as TtSWEWaterNodeData; if (d != null) d.Damping = value; }
        }

        [Category("SWE")]
        public ESWEBoundaryMode BoundaryMode
        {
            get => (NodeData as TtSWEWaterNodeData)?.BoundaryMode ?? ESWEBoundaryMode.Open;
            set { var d = NodeData as TtSWEWaterNodeData; if (d != null) d.BoundaryMode = value; }
        }

        [Category("SWE")]
        public float BaseWaterHeight
        {
            get => (NodeData as TtSWEWaterNodeData)?.BaseWaterHeight ?? 1.0f;
            set { var d = NodeData as TtSWEWaterNodeData; if (d != null) d.BaseWaterHeight = value; }
        }

        [Category("SWE")]
        public string HeightMapSrvName
        {
            get => (NodeData as TtSWEWaterNodeData)?.HeightMapSrvName ?? "SWEHeightMap";
            set { var d = NodeData as TtSWEWaterNodeData; if (d != null) d.HeightMapSrvName = value; }
        }

        [Category("Material")]
        [RName.PGRName(FilterExts = Graphics.Pipeline.Shader.TtMaterial.AssetExt)]
        public RName WaterMaterialName
        {
            get
            {
                var sweData = NodeData as TtSWEWaterNodeData;
                return sweData?.WaterMaterialName;
            }
            set
            {
                var sweData = NodeData as TtSWEWaterNodeData;
                if (sweData == null)
                    return;
                sweData.WaterMaterialName = value;
                ApplyMaterial(sweData).AddWaitTask();
            }
        }

        TtTestDisturbCmd mTestDisturbCmd;
        [TtTestDisturbCmd.TtValueEditor]
        [Category("Debug")]
        public TtTestDisturbCmd TestDisturb
        {
            get
            {
                if (mTestDisturbCmd == null)
                {
                    mTestDisturbCmd = new TtTestDisturbCmd();
                    mTestDisturbCmd.HostNode = this;
                }
                return mTestDisturbCmd;
            }
        }
    }
}
