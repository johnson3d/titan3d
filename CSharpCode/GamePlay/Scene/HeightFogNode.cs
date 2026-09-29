using System;
using System.ComponentModel;
using System.Threading.Tasks;

namespace EngineNS.GamePlay.Scene
{
    // 指数高度雾的"场景控制器"节点.
    // 管线里的雾参数在 RenderGraph 的 TtFogNode(rpolicy)上, 每次调都要改 rpolicy 很麻烦;
    // 本节点作为场景侧唯一数据源, 每帧 OnTickLogic 把参数推给当前 policy 的 FogNode,
    // 美术只需在 Outliner 里选中它、在 Details 面板调参即可, 无需改 rpolicy.
    [Bricks.CodeBuilder.ContextMenu("HeightFog", "Graphics\\HeightFog", TtNode.EditorKeyword)]
    [TtNode(NodeDataType = typeof(TtHeightFogNode.TtHeightFogNodeData), DefaultNamePrefix = "HeightFog")]
    [Rtti.Meta("")]
    public class TtHeightFogNode : TtVisual
    {
        [Rtti.Meta("")]
        public class TtHeightFogNodeData : TtNodeData
        {
            // 默认值与 tutorials/volumecloud/vcloud_scene.scene 保持一致, 作为新建场景的环境基线
            [Rtti.Meta("")] public bool Enable { get; set; } = true;

            [Rtti.Meta("")] public Color3f FogColor { get; set; } = new Color3f(0.5f, 0.6f, 0.7f);
            [Rtti.Meta("")] public float MinFogOpacity { get; set; } = 0.0f;
            [Rtti.Meta("")] public float FogDensity { get; set; } = 0.006f;
            [Rtti.Meta("")] public float FogEnd { get; set; } = 80.0f;
            [Rtti.Meta("")] public float FogHeightFalloff { get; set; } = 0.02f;
            [Rtti.Meta("")] public float StartDistance { get; set; } = 10.0f;

            [Rtti.Meta("")] public Color3f InscatterColor { get; set; } = new Color3f(1.0f, 0.9f, 0.7f);
            [Rtti.Meta("")] public float InscatteringExponent { get; set; } = 6.0f;
            [Rtti.Meta("")] public float InscatterStartDistance { get; set; } = 0.0f;
        }

        // §9.3: 强类型 NodeData 访问器给引擎代码用, 但对 Details 面板隐藏, 避免与逐字段包装属性重复
        [Browsable(false)]
        public TtHeightFogNodeData FogData => GetNodeData<TtHeightFogNodeData>();

        // ─────────────────────────────────────────────────────────────────────
        // §9.2 类型 A(纯转发): 值每帧在 OnTickLogic 里重新推给管线 FogNode, 下一帧自然生效.
        // 包装属性不加 [Rtti.Meta](数据本体已在 NodeData 上 Meta 过, 见 §9.1).
        // ─────────────────────────────────────────────────────────────────────
        [System.ComponentModel.Category("Fog")]
        [System.ComponentModel.DisplayName("Enable")]
        public bool FogEnable
        {
            get => FogData.Enable;
            set => FogData.Enable = value;
        }

        [System.ComponentModel.Category("Fog")]
        [EGui.Controls.PropertyGrid.TtColor3PickerEditor()]
        public Color3f FogColor
        {
            get => FogData.FogColor;
            set => FogData.FogColor = value;
        }

        [System.ComponentModel.Category("Fog")]
        [EGui.Controls.PropertyGrid.TtValueRange(0, 1000.0)]
        [EGui.Controls.PropertyGrid.TtValueChangeStep(0.0001f)]
        public float FogDensity
        {
            get => FogData.FogDensity;
            set => FogData.FogDensity = value;
        }

        [System.ComponentModel.Category("Fog")]
        [EGui.Controls.PropertyGrid.TtValueRange(0, 1000.0)]
        [EGui.Controls.PropertyGrid.TtValueChangeStep(0.0001f)]
        public float FogHeightFalloff
        {
            get => FogData.FogHeightFalloff;
            set => FogData.FogHeightFalloff = value;
        }

        // FogEnd 复用为高度雾的参考高度 h0(米)
        [System.ComponentModel.Category("Fog")]
        public float FogEnd
        {
            get => FogData.FogEnd;
            set => FogData.FogEnd = value;
        }

        [System.ComponentModel.Category("Fog")]
        public float StartDistance
        {
            get => FogData.StartDistance;
            set => FogData.StartDistance = value;
        }

        [System.ComponentModel.Category("Fog")]
        public float MinFogOpacity
        {
            get => FogData.MinFogOpacity;
            set => FogData.MinFogOpacity = value;
        }

        [System.ComponentModel.Category("Inscatter")]
        [EGui.Controls.PropertyGrid.TtColor3PickerEditor()]
        public Color3f InscatterColor
        {
            get => FogData.InscatterColor;
            set => FogData.InscatterColor = value;
        }

        [System.ComponentModel.Category("Inscatter")]
        public float InscatteringExponent
        {
            get => FogData.InscatteringExponent;
            set => FogData.InscatteringExponent = value;
        }

        [System.ComponentModel.Category("Inscatter")]
        public float InscatterStartDistance
        {
            get => FogData.InscatterStartDistance;
            set => FogData.InscatterStartDistance = value;
        }

        protected override async Thread.Async.TtTask<bool> InitializeNode(TtWorld world, TtNodeData data, EBoundVolumeType bvType, Type placementType)
        {
            if (data == null)
                data = new TtHeightFogNodeData();
            await base.InitializeNode(world, data, bvType, placementType);
            return true;
        }

        // 纯控制器节点, 不产生可见 mesh
        public override void OnGatherVisibleMeshes(TtWorld.TtVisParameter rp)
        {
        }

        public override bool OnTickLogic(TtNodeTickParameters args)
        {
            var policy = args.Policy;
            var fog = policy?.FogNode;
            if (fog != null)
            {
                if (FogData.Enable)
                {
                    // 只在状态变化时切 TypeFog: 切换会触发 shading env permutation 重编译, 不能每帧刷
                    if (policy.TypeFog != Graphics.Pipeline.TtRenderPolicy.ETypeFog.ExpHeight)
                        policy.TypeFog = Graphics.Pipeline.TtRenderPolicy.ETypeFog.ExpHeight;

                    // 把场景侧参数推给管线 FogNode(LightPosition 由管线自己的 SyncSunToFog 每帧维护, 这里不覆盖)
                    fog.FogColor = FogData.FogColor;
                    fog.MinFogOpacity = FogData.MinFogOpacity;
                    fog.FogDensity = FogData.FogDensity;
                    fog.FogEnd = FogData.FogEnd;
                    fog.FogHeightFalloff = FogData.FogHeightFalloff;
                    fog.StartDistance = FogData.StartDistance;
                    fog.InscatterColor = FogData.InscatterColor;
                    fog.InscatteringExponent = FogData.InscatteringExponent;
                    fog.InscatterStartDistance = FogData.InscatterStartDistance;
                }
                else
                {
                    if (policy.TypeFog != Graphics.Pipeline.TtRenderPolicy.ETypeFog.None)
                        policy.TypeFog = Graphics.Pipeline.TtRenderPolicy.ETypeFog.None;
                }
            }
            return base.OnTickLogic(args);
        }
    }
}
