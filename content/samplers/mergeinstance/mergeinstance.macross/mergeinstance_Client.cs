// Engine!

// 名称:mergeinstance
namespace samplers.mergeinstance
{
    
    [EngineNS.Macross.MacrossTypeClassAttribute()]
    [EngineNS.Editor.Editor_MacrossClassAttribute(EngineNS.ECSType.Client, EngineNS.Editor.Editor_MacrossClassAttribute.enMacrossType.Useable | EngineNS.Editor.Editor_MacrossClassAttribute.enMacrossType.Inheritable | EngineNS.Editor.Editor_MacrossClassAttribute.enMacrossType.Createable |  EngineNS.Editor.Editor_MacrossClassAttribute.enMacrossType.MacrossGetter)]
    public partial class mergeinstance : EngineNS.GamePlay.McGameInstance, EngineNS.Macross.IMacrossType
    {
        public static EngineNS.Profiler.TimeScope _mScope = EngineNS.Profiler.TimeScopeManager.GetTimeScope("samplers.mergeinstance.mergeinstance");
        public mergeinstance()
        {
            this.SceneData = null;
            this.ListActors = new System.Collections.Generic.List<EngineNS.GamePlay.Actor.GActor>();
            this.MainActor = null;
            this.NewVar_1 = null;
        }
        public mergeinstance(bool init)
        {
            this.SceneData = null;
            this.ListActors = new System.Collections.Generic.List<EngineNS.GamePlay.Actor.GActor>();
            this.MainActor = null;
            this.NewVar_1 = null;
        }
        private static DebugContext_mergeinstance mDebuggerContext
        {
            get
            {
                if (((mDebugHolder == null) 
                            || (mDebugHolder.Context == null)))
                {
                    mDebugHolder = new EngineNS.Macross.MacrossDataManager.MacrossDebugContextHolder(new DebugContext_mergeinstance());
                }
                return ((mDebugHolder.Context) as DebugContext_mergeinstance);
            }
        }
        [System.ThreadStaticAttribute()]
        private static EngineNS.Macross.MacrossDataManager.MacrossDebugContextHolder mDebugHolder;
        public virtual int Version
        {
            get
            {
                return 162;
            }
        }
        private EngineNS.Bricks.GpuDriven.GpuScene.SceneDataManager mSceneData;
        [EngineNS.Editor.MacrossMemberAttribute(EngineNS.Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public EngineNS.Bricks.GpuDriven.GpuScene.SceneDataManager SceneData
        {
            get
            {
                return this.mSceneData;
            }
            set
            {
                this.mSceneData = value;
            }
        }
        private System.Collections.Generic.List<EngineNS.GamePlay.Actor.GActor> mListActors;
        [EngineNS.Editor.MacrossMemberAttribute(EngineNS.Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public System.Collections.Generic.List<EngineNS.GamePlay.Actor.GActor> ListActors
        {
            get
            {
                return this.mListActors;
            }
            set
            {
                this.mListActors = value;
            }
        }
        private EngineNS.GamePlay.Actor.GActor mMainActor;
        [EngineNS.Editor.MacrossMemberAttribute(EngineNS.Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public EngineNS.GamePlay.Actor.GActor MainActor
        {
            get
            {
                return this.mMainActor;
            }
            set
            {
                this.mMainActor = value;
            }
        }
        private samplers.tpsgame.tpscenterdata mNewVar_1;
        [EngineNS.Editor.MacrossMemberAttribute(EngineNS.Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public samplers.tpsgame.tpscenterdata NewVar_1
        {
            get
            {
                return this.mNewVar_1;
            }
            set
            {
                this.mNewVar_1 = value;
            }
        }
// OverrideStart OnGameStart In,EngineNS.GamePlay.GGameInstance
#pragma warning disable 1998
        [EngineNS.Editor.MacrossMemberAttribute(EngineNS.Editor.MacrossMemberAttribute.enMacrossType.Callable|EngineNS.Editor.MacrossMemberAttribute.enMacrossType.Overrideable)]
        public override async System.Threading.Tasks.Task<bool> OnGameStart(EngineNS.GamePlay.GGameInstance game)
        {
            System.Type param_50e553de_eb51_4a64_a758_c2f3e96e6e29_type = typeof(samplers.mergeinstance.ui_main);
            EngineNS.RName param_50e553de_eb51_4a64_a758_c2f3e96e6e29_name = EngineNS.CEngine.Instance.FileManager.GetRName("samplers/mergeinstance/ui_main.macross", EngineNS.RName.enRNameType.Game);
            samplers.mergeinstance.ui_main methodReturnValue_50e553de_eb51_4a64_a758_c2f3e96e6e29 = null;
            samplers.tpsgame.tpscenterdata param_0255cf1a_fb5d_40fd_a2ec_482529cacd92_InParams_0 = null;
            EngineNS.GamePlay.GGameInstance param_9c785c51_782a_45d8_8187_c84bd0209952_game = null;
            EngineNS.RName param_a1c18c85_8e5e_4569_91af_055cc977cb3f_clusterName = EngineNS.CEngine.Instance.FileManager.GetRName("samplers/mergeinstance/cluster/sphere.vms.cluster", EngineNS.RName.enRNameType.Game);
            EngineNS.GamePlay.Actor.GActor param_a1c18c85_8e5e_4569_91af_055cc977cb3f_actor = null;
            EngineNS.RName param_83f93c60_16cf_41c5_9a3f_85830393a8c5_clusterName = EngineNS.CEngine.Instance.FileManager.GetRName("samplers/mergeinstance/cluster/box.vms.cluster", EngineNS.RName.enRNameType.Game);
            EngineNS.GamePlay.Actor.GActor param_83f93c60_16cf_41c5_9a3f_85830393a8c5_actor = null;
            bool compare_a2274167_7262_4435_946b_a37c7dd504ac;
            string param_f204aef2_6c97_4536_8586_f2091eb93c09_name = "";
            EngineNS.Graphics.Mesh.CGfxMesh methodReturnValue_f204aef2_6c97_4536_8586_f2091eb93c09 = null;
            EngineNS.RName.enRNameType param_7852684b_6294_43a2_906d_a7cf0aed783f_rNameType = EngineNS.RName.enRNameType.Game;
            string param_7852684b_6294_43a2_906d_a7cf0aed783f_name = "editor\\basemesh\\box.gms";
            EngineNS.RName methodReturnValue_7852684b_6294_43a2_906d_a7cf0aed783f = null;
            EngineNS.GamePlay.GGameInstance param_714046c8_aa97_42d9_ace5_bacd20c3bf50_game = null;
            EngineNS.GamePlay.Actor.GActor param_92feeffa_d1fd_42f5_94e7_302259ab9a94_actor = null;
            EngineNS.Vector3 param_40a8b59c_85b1_4cff_88f5_5565c4d07e1d_scale = new EngineNS.Vector3(0F, 0F, 0F);
            EngineNS.Quaternion param_40a8b59c_85b1_4cff_88f5_5565c4d07e1d_quaternion = new EngineNS.Quaternion(0F, 0F, 0F, 0F);
            EngineNS.Vector3 param_40a8b59c_85b1_4cff_88f5_5565c4d07e1d_location = new EngineNS.Vector3();
            EngineNS.GamePlay.SceneGraph.GSceneGraph param_40a8b59c_85b1_4cff_88f5_5565c4d07e1d_scene = null;
            EngineNS.RName param_40a8b59c_85b1_4cff_88f5_5565c4d07e1d_prefabname = EngineNS.CEngine.Instance.FileManager.GetRName("samplers/tpsgame/character/titanrobot/pf_titan.prefab", EngineNS.RName.enRNameType.Game);
            EngineNS.GamePlay.Actor.GActor methodReturnValue_40a8b59c_85b1_4cff_88f5_5565c4d07e1d = null;
            int param_f80c6ddc_9de2_4340_8fb8_9aa609f4fc6d_index = 0;
            EngineNS.Vector3 methodReturnValue_f80c6ddc_9de2_4340_8fb8_9aa609f4fc6d = new EngineNS.Vector3();
            bool param_c6b19e6f_8dd1_4b31_b606_143708162aea_useEditorScene = false;
            bool param_c6b19e6f_8dd1_4b31_b606_143708162aea_clearWorld = false;
            EngineNS.RName param_c6b19e6f_8dd1_4b31_b606_143708162aea_name = EngineNS.CEngine.Instance.FileManager.GetRName("samplers/mergeinstance/mginstmap.map", EngineNS.RName.enRNameType.Game);
            EngineNS.CRenderContext param_c6b19e6f_8dd1_4b31_b606_143708162aea_rc = null;
            EngineNS.GamePlay.SceneGraph.GSceneGraph methodReturnValue_c6b19e6f_8dd1_4b31_b606_143708162aea = null;
            bool param_31c01579_1b6b_4c9e_a26f_d2a12f8db9fa_isSM3 = true;
            EngineNS.Graphics.CGfxMaterialInstance param_31c01579_1b6b_4c9e_a26f_d2a12f8db9fa_MtlInst = null;
            EngineNS.CRenderContext param_31c01579_1b6b_4c9e_a26f_d2a12f8db9fa_rc = null;
            EngineNS.Bricks.GpuDriven.GpuScene.SceneDataManager createItem_d47e7262_51dc_48a9_b56b_90a7f71f4c6c = new EngineNS.Bricks.GpuDriven.GpuScene.SceneDataManager();
            samplers.tpsgame.tpscenterdata NewVar_0 = null;
            try
            {
                _mScope.Begin();
#if MacrossDebug
                mDebuggerContext.ParamPin_ede4eede_8dc7_4512_afb3_ac69c8cbf227 = game;
                if (BreakEnable_b594976f_0b93_4761_a3c3_80bf4de022ab)
                {
                    EngineNS.Editor.Runner.RunnerManager.BreakContext breakContext = new EngineNS.Editor.Runner.RunnerManager.BreakContext();
                    breakContext.ThisObject = this;
                    breakContext.DebuggerId = EngineNS.Rtti.RttiHelper.GuidTryParse("3df25d67-48f9-4dd1-bd6a-efa385295e3c");
                    breakContext.BreakId = EngineNS.Rtti.RttiHelper.GuidTryParse("b594976f-0b93-4761-a3c3-80bf4de022ab");
                    breakContext.ClassName = "mergeinstance";
                    breakContext.ValueContext = mDebuggerContext;
                    EngineNS.Editor.Runner.RunnerManager.Instance.Break(breakContext);
                    game = ((mDebuggerContext.ParamPin_ede4eede_8dc7_4512_afb3_ac69c8cbf227) as EngineNS.GamePlay.GGameInstance);
                }
#endif
#if MacrossDebug
                mDebuggerContext.ValueInHandle_85fc5145_9ccd_492d_8765_1ff9f3143348 = this.SceneData;
                if (BreakEnable_85fc5145_9ccd_492d_8765_1ff9f3143348)
                {
                    EngineNS.Editor.Runner.RunnerManager.BreakContext breakContext = new EngineNS.Editor.Runner.RunnerManager.BreakContext();
                    breakContext.ThisObject = this;
                    breakContext.DebuggerId = EngineNS.Rtti.RttiHelper.GuidTryParse("3df25d67-48f9-4dd1-bd6a-efa385295e3c");
                    breakContext.BreakId = EngineNS.Rtti.RttiHelper.GuidTryParse("85fc5145-9ccd-492d-8765-1ff9f3143348");
                    breakContext.ClassName = "mergeinstance";
                    breakContext.ValueContext = mDebuggerContext;
                    EngineNS.Editor.Runner.RunnerManager.Instance.Break(breakContext);
                }
#endif
                this.SceneData = ((createItem_d47e7262_51dc_48a9_b56b_90a7f71f4c6c) as EngineNS.Bricks.GpuDriven.GpuScene.SceneDataManager);
#if MacrossDebug
                mDebuggerContext.ValueInHandle_85fc5145_9ccd_492d_8765_1ff9f3143348 = this.SceneData;
#endif
#if MacrossDebug
                mDebuggerContext.ValueOutHandle_85fc5145_9ccd_492d_8765_1ff9f3143348 = this.SceneData;
#endif
#if MacrossDebug
                mDebuggerContext.ParamPin_610c7def_3544_4d63_86ab_f5b9446df709 = param_31c01579_1b6b_4c9e_a26f_d2a12f8db9fa_rc;
                mDebuggerContext.ParamPin_16cb3a6e_0098_42d2_adb6_cfe00ce358ff = param_31c01579_1b6b_4c9e_a26f_d2a12f8db9fa_MtlInst;
                mDebuggerContext.ParamPin_ed1452c8_94f4_4d2c_b634_e25be300cd1e = param_31c01579_1b6b_4c9e_a26f_d2a12f8db9fa_isSM3;
                if (BreakEnable_31c01579_1b6b_4c9e_a26f_d2a12f8db9fa)
                {
                    EngineNS.Editor.Runner.RunnerManager.BreakContext breakContext = new EngineNS.Editor.Runner.RunnerManager.BreakContext();
                    breakContext.ThisObject = this;
                    breakContext.DebuggerId = EngineNS.Rtti.RttiHelper.GuidTryParse("3df25d67-48f9-4dd1-bd6a-efa385295e3c");
                    breakContext.BreakId = EngineNS.Rtti.RttiHelper.GuidTryParse("31c01579-1b6b-4c9e-a26f-d2a12f8db9fa");
                    breakContext.ClassName = "mergeinstance";
                    breakContext.ValueContext = mDebuggerContext;
                    EngineNS.Editor.Runner.RunnerManager.Instance.Break(breakContext);
                    param_31c01579_1b6b_4c9e_a26f_d2a12f8db9fa_rc = ((mDebuggerContext.ParamPin_610c7def_3544_4d63_86ab_f5b9446df709) as EngineNS.CRenderContext);
                    param_31c01579_1b6b_4c9e_a26f_d2a12f8db9fa_MtlInst = ((mDebuggerContext.ParamPin_16cb3a6e_0098_42d2_adb6_cfe00ce358ff) as EngineNS.Graphics.CGfxMaterialInstance);
                    param_31c01579_1b6b_4c9e_a26f_d2a12f8db9fa_isSM3 = ((System.Boolean)(mDebuggerContext.ParamPin_ed1452c8_94f4_4d2c_b634_e25be300cd1e));
                }
#endif
                await this.SceneData.InitPass(param_31c01579_1b6b_4c9e_a26f_d2a12f8db9fa_rc, param_31c01579_1b6b_4c9e_a26f_d2a12f8db9fa_MtlInst, param_31c01579_1b6b_4c9e_a26f_d2a12f8db9fa_isSM3);
#if MacrossDebug
                mDebuggerContext.ParamPin_34799286_fb44_45ed_9b6e_ab811eda84c4 = param_c6b19e6f_8dd1_4b31_b606_143708162aea_rc;
                mDebuggerContext.ParamPin_48dea2b4_12d2_4b37_a66d_d721a289a51a = param_c6b19e6f_8dd1_4b31_b606_143708162aea_name;
                mDebuggerContext.ParamPin_9fcc6259_431f_434a_86e7_1d8743693f5d = param_c6b19e6f_8dd1_4b31_b606_143708162aea_clearWorld;
                mDebuggerContext.ParamPin_ee5efc0c_4110_437e_a642_c175e1e99d69 = param_c6b19e6f_8dd1_4b31_b606_143708162aea_useEditorScene;
                if (BreakEnable_c6b19e6f_8dd1_4b31_b606_143708162aea)
                {
                    EngineNS.Editor.Runner.RunnerManager.BreakContext breakContext = new EngineNS.Editor.Runner.RunnerManager.BreakContext();
                    breakContext.ThisObject = this;
                    breakContext.DebuggerId = EngineNS.Rtti.RttiHelper.GuidTryParse("3df25d67-48f9-4dd1-bd6a-efa385295e3c");
                    breakContext.BreakId = EngineNS.Rtti.RttiHelper.GuidTryParse("c6b19e6f-8dd1-4b31-b606-143708162aea");
                    breakContext.ClassName = "mergeinstance";
                    breakContext.ValueContext = mDebuggerContext;
                    EngineNS.Editor.Runner.RunnerManager.Instance.Break(breakContext);
                    param_c6b19e6f_8dd1_4b31_b606_143708162aea_rc = ((mDebuggerContext.ParamPin_34799286_fb44_45ed_9b6e_ab811eda84c4) as EngineNS.CRenderContext);
                    param_c6b19e6f_8dd1_4b31_b606_143708162aea_name = ((mDebuggerContext.ParamPin_48dea2b4_12d2_4b37_a66d_d721a289a51a) as EngineNS.RName);
                    param_c6b19e6f_8dd1_4b31_b606_143708162aea_clearWorld = ((System.Boolean)(mDebuggerContext.ParamPin_9fcc6259_431f_434a_86e7_1d8743693f5d));
                    param_c6b19e6f_8dd1_4b31_b606_143708162aea_useEditorScene = ((System.Boolean)(mDebuggerContext.ParamPin_ee5efc0c_4110_437e_a642_c175e1e99d69));
                }
#endif
                methodReturnValue_c6b19e6f_8dd1_4b31_b606_143708162aea = ((await game.LoadScene(param_c6b19e6f_8dd1_4b31_b606_143708162aea_rc, param_c6b19e6f_8dd1_4b31_b606_143708162aea_name, param_c6b19e6f_8dd1_4b31_b606_143708162aea_clearWorld, param_c6b19e6f_8dd1_4b31_b606_143708162aea_useEditorScene)) as EngineNS.GamePlay.SceneGraph.GSceneGraph);
#if MacrossDebug
                mDebuggerContext.returnLink_c6b19e6f_8dd1_4b31_b606_143708162aea = methodReturnValue_c6b19e6f_8dd1_4b31_b606_143708162aea;
#endif
#if MacrossDebug
                mDebuggerContext.ParamPin_a3673d73_2431_4fc0_87c9_beb423565e07 = param_f80c6ddc_9de2_4340_8fb8_9aa609f4fc6d_index;
                if (BreakEnable_f80c6ddc_9de2_4340_8fb8_9aa609f4fc6d)
                {
                    EngineNS.Editor.Runner.RunnerManager.BreakContext breakContext = new EngineNS.Editor.Runner.RunnerManager.BreakContext();
                    breakContext.ThisObject = this;
                    breakContext.DebuggerId = EngineNS.Rtti.RttiHelper.GuidTryParse("3df25d67-48f9-4dd1-bd6a-efa385295e3c");
                    breakContext.BreakId = EngineNS.Rtti.RttiHelper.GuidTryParse("f80c6ddc-9de2-4340-8fb8-9aa609f4fc6d");
                    breakContext.ClassName = "mergeinstance";
                    breakContext.ValueContext = mDebuggerContext;
                    EngineNS.Editor.Runner.RunnerManager.Instance.Break(breakContext);
                    param_f80c6ddc_9de2_4340_8fb8_9aa609f4fc6d_index = ((System.Int32)(mDebuggerContext.ParamPin_a3673d73_2431_4fc0_87c9_beb423565e07));
                }
#endif
                methodReturnValue_f80c6ddc_9de2_4340_8fb8_9aa609f4fc6d = ((EngineNS.Vector3)(methodReturnValue_c6b19e6f_8dd1_4b31_b606_143708162aea.GetPlayerStartLocation(param_f80c6ddc_9de2_4340_8fb8_9aa609f4fc6d_index)));
#if MacrossDebug
                mDebuggerContext.returnLink_f80c6ddc_9de2_4340_8fb8_9aa609f4fc6d = methodReturnValue_f80c6ddc_9de2_4340_8fb8_9aa609f4fc6d;
#endif
                param_40a8b59c_85b1_4cff_88f5_5565c4d07e1d_scene = ((methodReturnValue_c6b19e6f_8dd1_4b31_b606_143708162aea) as EngineNS.GamePlay.SceneGraph.GSceneGraph);
                param_40a8b59c_85b1_4cff_88f5_5565c4d07e1d_location = ((EngineNS.Vector3)(methodReturnValue_f80c6ddc_9de2_4340_8fb8_9aa609f4fc6d));
#if MacrossDebug
                mDebuggerContext.ParamPin_c4600f9c_4472_4d5c_b3dd_557cc8758eef = param_40a8b59c_85b1_4cff_88f5_5565c4d07e1d_prefabname;
                mDebuggerContext.ParamPin_3b6c8d5f_4acc_465b_95ac_c2adf5685150 = param_40a8b59c_85b1_4cff_88f5_5565c4d07e1d_scene;
                mDebuggerContext.ParamPin_9726a714_b714_4888_a571_460b819b2768 = param_40a8b59c_85b1_4cff_88f5_5565c4d07e1d_location;
                mDebuggerContext.ParamPin_3e292893_be46_4300_b2fe_38c912539863 = param_40a8b59c_85b1_4cff_88f5_5565c4d07e1d_quaternion;
                mDebuggerContext.ParamPin_eea7cb7c_73f2_49f7_82f2_0d3167af8fbe = param_40a8b59c_85b1_4cff_88f5_5565c4d07e1d_scale;
                if (BreakEnable_40a8b59c_85b1_4cff_88f5_5565c4d07e1d)
                {
                    EngineNS.Editor.Runner.RunnerManager.BreakContext breakContext = new EngineNS.Editor.Runner.RunnerManager.BreakContext();
                    breakContext.ThisObject = this;
                    breakContext.DebuggerId = EngineNS.Rtti.RttiHelper.GuidTryParse("3df25d67-48f9-4dd1-bd6a-efa385295e3c");
                    breakContext.BreakId = EngineNS.Rtti.RttiHelper.GuidTryParse("40a8b59c-85b1-4cff-88f5-5565c4d07e1d");
                    breakContext.ClassName = "mergeinstance";
                    breakContext.ValueContext = mDebuggerContext;
                    EngineNS.Editor.Runner.RunnerManager.Instance.Break(breakContext);
                    param_40a8b59c_85b1_4cff_88f5_5565c4d07e1d_prefabname = ((mDebuggerContext.ParamPin_c4600f9c_4472_4d5c_b3dd_557cc8758eef) as EngineNS.RName);
                    param_40a8b59c_85b1_4cff_88f5_5565c4d07e1d_scene = ((mDebuggerContext.ParamPin_3b6c8d5f_4acc_465b_95ac_c2adf5685150) as EngineNS.GamePlay.SceneGraph.GSceneGraph);
                    param_40a8b59c_85b1_4cff_88f5_5565c4d07e1d_location = ((EngineNS.Vector3)(mDebuggerContext.ParamPin_9726a714_b714_4888_a571_460b819b2768));
                    param_40a8b59c_85b1_4cff_88f5_5565c4d07e1d_quaternion = ((EngineNS.Quaternion)(mDebuggerContext.ParamPin_3e292893_be46_4300_b2fe_38c912539863));
                    param_40a8b59c_85b1_4cff_88f5_5565c4d07e1d_scale = ((EngineNS.Vector3)(mDebuggerContext.ParamPin_eea7cb7c_73f2_49f7_82f2_0d3167af8fbe));
                }
#endif
                methodReturnValue_40a8b59c_85b1_4cff_88f5_5565c4d07e1d = ((await EngineNS.GamePlay.Actor.GActor.NewPrefabActorTo(param_40a8b59c_85b1_4cff_88f5_5565c4d07e1d_prefabname, param_40a8b59c_85b1_4cff_88f5_5565c4d07e1d_scene, param_40a8b59c_85b1_4cff_88f5_5565c4d07e1d_location, param_40a8b59c_85b1_4cff_88f5_5565c4d07e1d_quaternion, param_40a8b59c_85b1_4cff_88f5_5565c4d07e1d_scale)) as EngineNS.GamePlay.Actor.GActor);
#if MacrossDebug
                mDebuggerContext.returnLink_40a8b59c_85b1_4cff_88f5_5565c4d07e1d = methodReturnValue_40a8b59c_85b1_4cff_88f5_5565c4d07e1d;
#endif
                param_92feeffa_d1fd_42f5_94e7_302259ab9a94_actor = ((methodReturnValue_40a8b59c_85b1_4cff_88f5_5565c4d07e1d) as EngineNS.GamePlay.Actor.GActor);
#if MacrossDebug
                mDebuggerContext.ParamPin_9146aee1_1f54_43b4_b6b4_443a804082db = param_92feeffa_d1fd_42f5_94e7_302259ab9a94_actor;
                if (BreakEnable_92feeffa_d1fd_42f5_94e7_302259ab9a94)
                {
                    EngineNS.Editor.Runner.RunnerManager.BreakContext breakContext = new EngineNS.Editor.Runner.RunnerManager.BreakContext();
                    breakContext.ThisObject = this;
                    breakContext.DebuggerId = EngineNS.Rtti.RttiHelper.GuidTryParse("3df25d67-48f9-4dd1-bd6a-efa385295e3c");
                    breakContext.BreakId = EngineNS.Rtti.RttiHelper.GuidTryParse("92feeffa-d1fd-42f5-94e7-302259ab9a94");
                    breakContext.ClassName = "mergeinstance";
                    breakContext.ValueContext = mDebuggerContext;
                    EngineNS.Editor.Runner.RunnerManager.Instance.Break(breakContext);
                    param_92feeffa_d1fd_42f5_94e7_302259ab9a94_actor = ((mDebuggerContext.ParamPin_9146aee1_1f54_43b4_b6b4_443a804082db) as EngineNS.GamePlay.Actor.GActor);
                }
#endif
                game.SetCameraByActor(param_92feeffa_d1fd_42f5_94e7_302259ab9a94_actor);
#if MacrossDebug
                mDebuggerContext.ValueInHandle_7aeea6c8_fe4b_4217_b9f6_f52089613010 = this.MainActor;
                if (BreakEnable_7aeea6c8_fe4b_4217_b9f6_f52089613010)
                {
                    EngineNS.Editor.Runner.RunnerManager.BreakContext breakContext = new EngineNS.Editor.Runner.RunnerManager.BreakContext();
                    breakContext.ThisObject = this;
                    breakContext.DebuggerId = EngineNS.Rtti.RttiHelper.GuidTryParse("3df25d67-48f9-4dd1-bd6a-efa385295e3c");
                    breakContext.BreakId = EngineNS.Rtti.RttiHelper.GuidTryParse("7aeea6c8-fe4b-4217-b9f6-f52089613010");
                    breakContext.ClassName = "mergeinstance";
                    breakContext.ValueContext = mDebuggerContext;
                    EngineNS.Editor.Runner.RunnerManager.Instance.Break(breakContext);
                }
#endif
                this.MainActor = ((methodReturnValue_40a8b59c_85b1_4cff_88f5_5565c4d07e1d) as EngineNS.GamePlay.Actor.GActor);
#if MacrossDebug
                mDebuggerContext.ValueInHandle_7aeea6c8_fe4b_4217_b9f6_f52089613010 = this.MainActor;
#endif
                param_714046c8_aa97_42d9_ace5_bacd20c3bf50_game = ((game) as EngineNS.GamePlay.GGameInstance);
#if MacrossDebug
                mDebuggerContext.ParamPin_315d1ac5_dc38_4032_ab30_64548b6f32e9 = param_714046c8_aa97_42d9_ace5_bacd20c3bf50_game;
                if (BreakEnable_714046c8_aa97_42d9_ace5_bacd20c3bf50)
                {
                    EngineNS.Editor.Runner.RunnerManager.BreakContext breakContext = new EngineNS.Editor.Runner.RunnerManager.BreakContext();
                    breakContext.ThisObject = this;
                    breakContext.DebuggerId = EngineNS.Rtti.RttiHelper.GuidTryParse("3df25d67-48f9-4dd1-bd6a-efa385295e3c");
                    breakContext.BreakId = EngineNS.Rtti.RttiHelper.GuidTryParse("714046c8-aa97-42d9-ace5-bacd20c3bf50");
                    breakContext.ClassName = "mergeinstance";
                    breakContext.ValueContext = mDebuggerContext;
                    EngineNS.Editor.Runner.RunnerManager.Instance.Break(breakContext);
                    param_714046c8_aa97_42d9_ace5_bacd20c3bf50_game = ((mDebuggerContext.ParamPin_315d1ac5_dc38_4032_ab30_64548b6f32e9) as EngineNS.GamePlay.GGameInstance);
                }
#endif
                await this.CreateMergedMeshActors(param_714046c8_aa97_42d9_ace5_bacd20c3bf50_game);
#if MacrossDebug
                mDebuggerContext.ValueOutHandle_f6701d27_f00d_434f_a131_0a4b15ebe982 = this.ListActors;
#endif
                var param_da806233_d514_4267_b93c_d2e85412d639 = this.ListActors;
#if MacrossDebug
                if (BreakEnable_da806233_d514_4267_b93c_d2e85412d639)
                {
                    EngineNS.Editor.Runner.RunnerManager.BreakContext breakContext = new EngineNS.Editor.Runner.RunnerManager.BreakContext();
                    breakContext.ThisObject = this;
                    breakContext.DebuggerId = EngineNS.Rtti.RttiHelper.GuidTryParse("3df25d67-48f9-4dd1-bd6a-efa385295e3c");
                    breakContext.BreakId = EngineNS.Rtti.RttiHelper.GuidTryParse("da806233-d514-4267-b93c-d2e85412d639");
                    breakContext.ClassName = "mergeinstance";
                    breakContext.ValueContext = mDebuggerContext;
                    EngineNS.Editor.Runner.RunnerManager.Instance.Break(breakContext);
                }
#endif
        for (int index_da806233_d514_4267_b93c_d2e85412d639 = 0; index_da806233_d514_4267_b93c_d2e85412d639 < param_da806233_d514_4267_b93c_d2e85412d639.Count; index_da806233_d514_4267_b93c_d2e85412d639++)        {
                EngineNS.GamePlay.Actor.GActor current_da806233_d514_4267_b93c_d2e85412d639 = param_da806233_d514_4267_b93c_d2e85412d639[index_da806233_d514_4267_b93c_d2e85412d639];
#if MacrossDebug
                mDebuggerContext.ParamPin_42422c54_c8c4_4659_a7fc_6b1342994b1c = param_7852684b_6294_43a2_906d_a7cf0aed783f_name;
                mDebuggerContext.ParamPin_3590197f_e550_4a7c_a6e9_204f6ca357bd = param_7852684b_6294_43a2_906d_a7cf0aed783f_rNameType;
                if (BreakEnable_7852684b_6294_43a2_906d_a7cf0aed783f)
                {
                    EngineNS.Editor.Runner.RunnerManager.BreakContext breakContext = new EngineNS.Editor.Runner.RunnerManager.BreakContext();
                    breakContext.ThisObject = this;
                    breakContext.DebuggerId = EngineNS.Rtti.RttiHelper.GuidTryParse("3df25d67-48f9-4dd1-bd6a-efa385295e3c");
                    breakContext.BreakId = EngineNS.Rtti.RttiHelper.GuidTryParse("7852684b-6294-43a2-906d-a7cf0aed783f");
                    breakContext.ClassName = "mergeinstance";
                    breakContext.ValueContext = mDebuggerContext;
                    EngineNS.Editor.Runner.RunnerManager.Instance.Break(breakContext);
                    param_7852684b_6294_43a2_906d_a7cf0aed783f_name = ((mDebuggerContext.ParamPin_42422c54_c8c4_4659_a7fc_6b1342994b1c) as System.String);
                    param_7852684b_6294_43a2_906d_a7cf0aed783f_rNameType = ((EngineNS.RName.enRNameType)(mDebuggerContext.ParamPin_3590197f_e550_4a7c_a6e9_204f6ca357bd));
                }
#endif
                methodReturnValue_7852684b_6294_43a2_906d_a7cf0aed783f = ((EngineNS.RName.GetRName(param_7852684b_6294_43a2_906d_a7cf0aed783f_name, param_7852684b_6294_43a2_906d_a7cf0aed783f_rNameType)) as EngineNS.RName);
#if MacrossDebug
                mDebuggerContext.returnLink_7852684b_6294_43a2_906d_a7cf0aed783f = methodReturnValue_7852684b_6294_43a2_906d_a7cf0aed783f;
#endif
#if MacrossDebug
                mDebuggerContext.ParamPin_0c8467c6_5bc9_43b3_a849_b0666264d231 = param_f204aef2_6c97_4536_8586_f2091eb93c09_name;
                if (BreakEnable_f204aef2_6c97_4536_8586_f2091eb93c09)
                {
                    EngineNS.Editor.Runner.RunnerManager.BreakContext breakContext = new EngineNS.Editor.Runner.RunnerManager.BreakContext();
                    breakContext.ThisObject = this;
                    breakContext.DebuggerId = EngineNS.Rtti.RttiHelper.GuidTryParse("3df25d67-48f9-4dd1-bd6a-efa385295e3c");
                    breakContext.BreakId = EngineNS.Rtti.RttiHelper.GuidTryParse("f204aef2-6c97-4536-8586-f2091eb93c09");
                    breakContext.ClassName = "mergeinstance";
                    breakContext.ValueContext = mDebuggerContext;
                    EngineNS.Editor.Runner.RunnerManager.Instance.Break(breakContext);
                    param_f204aef2_6c97_4536_8586_f2091eb93c09_name = ((mDebuggerContext.ParamPin_0c8467c6_5bc9_43b3_a849_b0666264d231) as System.String);
                }
#endif
                methodReturnValue_f204aef2_6c97_4536_8586_f2091eb93c09 = ((current_da806233_d514_4267_b93c_d2e85412d639.GetComponentMesh(param_f204aef2_6c97_4536_8586_f2091eb93c09_name)) as EngineNS.Graphics.Mesh.CGfxMesh);
#if MacrossDebug
                mDebuggerContext.returnLink_f204aef2_6c97_4536_8586_f2091eb93c09 = methodReturnValue_f204aef2_6c97_4536_8586_f2091eb93c09;
#endif
#if MacrossDebug
                mDebuggerContext.ValueInHandle_83422356_fd9b_48cb_b4ca_3657e5f2512b = NewVar_0;
                if (BreakEnable_83422356_fd9b_48cb_b4ca_3657e5f2512b)
                {
                    EngineNS.Editor.Runner.RunnerManager.BreakContext breakContext = new EngineNS.Editor.Runner.RunnerManager.BreakContext();
                    breakContext.ThisObject = this;
                    breakContext.DebuggerId = EngineNS.Rtti.RttiHelper.GuidTryParse("3df25d67-48f9-4dd1-bd6a-efa385295e3c");
                    breakContext.BreakId = EngineNS.Rtti.RttiHelper.GuidTryParse("83422356-fd9b-48cb-b4ca-3657e5f2512b");
                    breakContext.ClassName = "mergeinstance";
                    breakContext.ValueContext = mDebuggerContext;
                    EngineNS.Editor.Runner.RunnerManager.Instance.Break(breakContext);
                }
#endif
                NewVar_0 = null;
#if MacrossDebug
                mDebuggerContext.ValueInHandle_83422356_fd9b_48cb_b4ca_3657e5f2512b = NewVar_0;
#endif
#if MacrossDebug
                mDebuggerContext.ValueOutHandle_4c779de7_3298_4ef1_b663_ef95c466dba3 = methodReturnValue_f204aef2_6c97_4536_8586_f2091eb93c09.Name;
#endif
                compare_a2274167_7262_4435_946b_a37c7dd504ac = (methodReturnValue_f204aef2_6c97_4536_8586_f2091eb93c09.Name == methodReturnValue_7852684b_6294_43a2_906d_a7cf0aed783f);
#if MacrossDebug
                mDebuggerContext.resultHandle_a2274167_7262_4435_946b_a37c7dd504ac = compare_a2274167_7262_4435_946b_a37c7dd504ac;
#endif
                bool condition_a4b93072_16fc_4194_a828_9a76a990dead = compare_a2274167_7262_4435_946b_a37c7dd504ac;
#if MacrossDebug
                mDebuggerContext.userControl_a4b93072_16fc_4194_a828_9a76a990dead = condition_a4b93072_16fc_4194_a828_9a76a990dead;
                if (BreakEnable_cdc078c5_122c_46db_9e91_141713cd1ebf)
                {
                    EngineNS.Editor.Runner.RunnerManager.BreakContext breakContext = new EngineNS.Editor.Runner.RunnerManager.BreakContext();
                    breakContext.ThisObject = this;
                    breakContext.DebuggerId = EngineNS.Rtti.RttiHelper.GuidTryParse("3df25d67-48f9-4dd1-bd6a-efa385295e3c");
                    breakContext.BreakId = EngineNS.Rtti.RttiHelper.GuidTryParse("cdc078c5-122c-46db-9e91-141713cd1ebf");
                    breakContext.ClassName = "mergeinstance";
                    breakContext.ValueContext = mDebuggerContext;
                    EngineNS.Editor.Runner.RunnerManager.Instance.Break(breakContext);
                    condition_a4b93072_16fc_4194_a828_9a76a990dead = ((System.Boolean)(mDebuggerContext.userControl_a4b93072_16fc_4194_a828_9a76a990dead));
                }
#endif
                if (condition_a4b93072_16fc_4194_a828_9a76a990dead)
                {
#if MacrossDebug
                    mDebuggerContext.ValueOutHandle_05a19218_8af3_4f1d_84da_3cfddc02506e = this.SceneData;
#endif
                    param_83f93c60_16cf_41c5_9a3f_85830393a8c5_actor = ((current_da806233_d514_4267_b93c_d2e85412d639) as EngineNS.GamePlay.Actor.GActor);
#if MacrossDebug
                    mDebuggerContext.ParamPin_936c9d80_b656_445d_8bec_7e271e9a3450 = param_83f93c60_16cf_41c5_9a3f_85830393a8c5_actor;
                    mDebuggerContext.ParamPin_88ef3469_646d_4fdd_8184_401a8c60490d = param_83f93c60_16cf_41c5_9a3f_85830393a8c5_clusterName;
                    if (BreakEnable_83f93c60_16cf_41c5_9a3f_85830393a8c5)
                    {
                        EngineNS.Editor.Runner.RunnerManager.BreakContext breakContext = new EngineNS.Editor.Runner.RunnerManager.BreakContext();
                        breakContext.ThisObject = this;
                        breakContext.DebuggerId = EngineNS.Rtti.RttiHelper.GuidTryParse("3df25d67-48f9-4dd1-bd6a-efa385295e3c");
                        breakContext.BreakId = EngineNS.Rtti.RttiHelper.GuidTryParse("83f93c60-16cf-41c5-9a3f-85830393a8c5");
                        breakContext.ClassName = "mergeinstance";
                        breakContext.ValueContext = mDebuggerContext;
                        EngineNS.Editor.Runner.RunnerManager.Instance.Break(breakContext);
                        param_83f93c60_16cf_41c5_9a3f_85830393a8c5_actor = ((mDebuggerContext.ParamPin_936c9d80_b656_445d_8bec_7e271e9a3450) as EngineNS.GamePlay.Actor.GActor);
                        param_83f93c60_16cf_41c5_9a3f_85830393a8c5_clusterName = ((mDebuggerContext.ParamPin_88ef3469_646d_4fdd_8184_401a8c60490d) as EngineNS.RName);
                    }
#endif
                    this.SceneData.AddMeshInstance(param_83f93c60_16cf_41c5_9a3f_85830393a8c5_actor, param_83f93c60_16cf_41c5_9a3f_85830393a8c5_clusterName);
                }
                else
                {
#if MacrossDebug
                    mDebuggerContext.ValueOutHandle_05a19218_8af3_4f1d_84da_3cfddc02506e = this.SceneData;
#endif
                    param_a1c18c85_8e5e_4569_91af_055cc977cb3f_actor = ((current_da806233_d514_4267_b93c_d2e85412d639) as EngineNS.GamePlay.Actor.GActor);
#if MacrossDebug
                    mDebuggerContext.ParamPin_828b9a26_60a5_4643_902c_734985c52ce3 = param_a1c18c85_8e5e_4569_91af_055cc977cb3f_actor;
                    mDebuggerContext.ParamPin_6807e3ae_4f7e_4889_aa82_163ab85cf1ee = param_a1c18c85_8e5e_4569_91af_055cc977cb3f_clusterName;
                    if (BreakEnable_a1c18c85_8e5e_4569_91af_055cc977cb3f)
                    {
                        EngineNS.Editor.Runner.RunnerManager.BreakContext breakContext = new EngineNS.Editor.Runner.RunnerManager.BreakContext();
                        breakContext.ThisObject = this;
                        breakContext.DebuggerId = EngineNS.Rtti.RttiHelper.GuidTryParse("3df25d67-48f9-4dd1-bd6a-efa385295e3c");
                        breakContext.BreakId = EngineNS.Rtti.RttiHelper.GuidTryParse("a1c18c85-8e5e-4569-91af-055cc977cb3f");
                        breakContext.ClassName = "mergeinstance";
                        breakContext.ValueContext = mDebuggerContext;
                        EngineNS.Editor.Runner.RunnerManager.Instance.Break(breakContext);
                        param_a1c18c85_8e5e_4569_91af_055cc977cb3f_actor = ((mDebuggerContext.ParamPin_828b9a26_60a5_4643_902c_734985c52ce3) as EngineNS.GamePlay.Actor.GActor);
                        param_a1c18c85_8e5e_4569_91af_055cc977cb3f_clusterName = ((mDebuggerContext.ParamPin_6807e3ae_4f7e_4889_aa82_163ab85cf1ee) as EngineNS.RName);
                    }
#endif
                    this.SceneData.AddMeshInstance(param_a1c18c85_8e5e_4569_91af_055cc977cb3f_actor, param_a1c18c85_8e5e_4569_91af_055cc977cb3f_clusterName);
                }
        }
#if MacrossDebug
                if (BreakEnable_f4cf9495_2797_4c62_a014_3def88f0fdaa)
                {
                    EngineNS.Editor.Runner.RunnerManager.BreakContext breakContext = new EngineNS.Editor.Runner.RunnerManager.BreakContext();
                    breakContext.ThisObject = this;
                    breakContext.DebuggerId = EngineNS.Rtti.RttiHelper.GuidTryParse("3df25d67-48f9-4dd1-bd6a-efa385295e3c");
                    breakContext.BreakId = EngineNS.Rtti.RttiHelper.GuidTryParse("f4cf9495-2797-4c62-a014-3def88f0fdaa");
                    breakContext.ClassName = "mergeinstance";
                    breakContext.ValueContext = mDebuggerContext;
                    EngineNS.Editor.Runner.RunnerManager.Instance.Break(breakContext);
                }
#endif
                createItem_d47e7262_51dc_48a9_b56b_90a7f71f4c6c.UpdateGpuBuffer();
#if MacrossDebug
                mDebuggerContext.ValueOutHandle_85fc5145_9ccd_492d_8765_1ff9f3143348 = this.SceneData;
#endif
                param_9c785c51_782a_45d8_8187_c84bd0209952_game = ((game) as EngineNS.GamePlay.GGameInstance);
#if MacrossDebug
                mDebuggerContext.ParamPin_e1fe5550_46f7_4ec3_b0b8_a0a86b2981d7 = param_9c785c51_782a_45d8_8187_c84bd0209952_game;
                if (BreakEnable_9c785c51_782a_45d8_8187_c84bd0209952)
                {
                    EngineNS.Editor.Runner.RunnerManager.BreakContext breakContext = new EngineNS.Editor.Runner.RunnerManager.BreakContext();
                    breakContext.ThisObject = this;
                    breakContext.DebuggerId = EngineNS.Rtti.RttiHelper.GuidTryParse("3df25d67-48f9-4dd1-bd6a-efa385295e3c");
                    breakContext.BreakId = EngineNS.Rtti.RttiHelper.GuidTryParse("9c785c51-782a-45d8-8187-c84bd0209952");
                    breakContext.ClassName = "mergeinstance";
                    breakContext.ValueContext = mDebuggerContext;
                    EngineNS.Editor.Runner.RunnerManager.Instance.Break(breakContext);
                    param_9c785c51_782a_45d8_8187_c84bd0209952_game = ((mDebuggerContext.ParamPin_e1fe5550_46f7_4ec3_b0b8_a0a86b2981d7) as EngineNS.GamePlay.GGameInstance);
                }
#endif
                this.SceneData.BindDrawGpuScene(param_9c785c51_782a_45d8_8187_c84bd0209952_game);
#if MacrossDebug
                mDebuggerContext.ValueOutHandle_b65d0be7_26d4_4655_aa40_ec82f5d2fb75 = this.NewVar_1;
#endif
                param_0255cf1a_fb5d_40fd_a2ec_482529cacd92_InParams_0 = ((this.NewVar_1) as samplers.tpsgame.tpscenterdata);
#if MacrossDebug
                mDebuggerContext.ParamPin_c8647cbd_dec2_4801_8b84_2a391b5cab88 = param_0255cf1a_fb5d_40fd_a2ec_482529cacd92_InParams_0;
                if (BreakEnable_0255cf1a_fb5d_40fd_a2ec_482529cacd92)
                {
                    EngineNS.Editor.Runner.RunnerManager.BreakContext breakContext = new EngineNS.Editor.Runner.RunnerManager.BreakContext();
                    breakContext.ThisObject = this;
                    breakContext.DebuggerId = EngineNS.Rtti.RttiHelper.GuidTryParse("3df25d67-48f9-4dd1-bd6a-efa385295e3c");
                    breakContext.BreakId = EngineNS.Rtti.RttiHelper.GuidTryParse("0255cf1a-fb5d-40fd-a2ec-482529cacd92");
                    breakContext.ClassName = "mergeinstance";
                    breakContext.ValueContext = mDebuggerContext;
                    EngineNS.Editor.Runner.RunnerManager.Instance.Break(breakContext);
                    param_0255cf1a_fb5d_40fd_a2ec_482529cacd92_InParams_0 = ((mDebuggerContext.ParamPin_c8647cbd_dec2_4801_8b84_2a391b5cab88) as samplers.tpsgame.tpscenterdata);
                }
#endif
                this.NewFunction_0(param_0255cf1a_fb5d_40fd_a2ec_482529cacd92_InParams_0);
#if MacrossDebug
                mDebuggerContext.ValueOutHandle_78836361_0c19_43d9_ae81_17b411876125 = game.UIHost;
#endif
#if MacrossDebug
                mDebuggerContext.ParamPin_53882fea_aafa_4cf3_9fb1_7c238e2cc85a = param_50e553de_eb51_4a64_a758_c2f3e96e6e29_name;
                mDebuggerContext.ParamPin_77537554_3bcc_4e86_93fd_dc70088d2569 = param_50e553de_eb51_4a64_a758_c2f3e96e6e29_type;
                if (BreakEnable_50e553de_eb51_4a64_a758_c2f3e96e6e29)
                {
                    EngineNS.Editor.Runner.RunnerManager.BreakContext breakContext = new EngineNS.Editor.Runner.RunnerManager.BreakContext();
                    breakContext.ThisObject = this;
                    breakContext.DebuggerId = EngineNS.Rtti.RttiHelper.GuidTryParse("3df25d67-48f9-4dd1-bd6a-efa385295e3c");
                    breakContext.BreakId = EngineNS.Rtti.RttiHelper.GuidTryParse("50e553de-eb51-4a64-a758-c2f3e96e6e29");
                    breakContext.ClassName = "mergeinstance";
                    breakContext.ValueContext = mDebuggerContext;
                    EngineNS.Editor.Runner.RunnerManager.Instance.Break(breakContext);
                    param_50e553de_eb51_4a64_a758_c2f3e96e6e29_name = ((mDebuggerContext.ParamPin_53882fea_aafa_4cf3_9fb1_7c238e2cc85a) as EngineNS.RName);
                    param_50e553de_eb51_4a64_a758_c2f3e96e6e29_type = ((mDebuggerContext.ParamPin_77537554_3bcc_4e86_93fd_dc70088d2569) as System.Type);
                }
#endif
                methodReturnValue_50e553de_eb51_4a64_a758_c2f3e96e6e29 = ((await game.UIHost.AddChildWithRNameWithReturn(param_50e553de_eb51_4a64_a758_c2f3e96e6e29_name, param_50e553de_eb51_4a64_a758_c2f3e96e6e29_type)) as samplers.mergeinstance.ui_main);
#if MacrossDebug
                mDebuggerContext.returnLink_50e553de_eb51_4a64_a758_c2f3e96e6e29 = methodReturnValue_50e553de_eb51_4a64_a758_c2f3e96e6e29;
#endif
#if MacrossDebug
                mDebuggerContext.ParamPin_791144e8_f0c7_4d82_b32b_6251e111d65e = true;
                if (BreakEnable_abe157d2_57e6_49f6_a358_837c4077bf96)
                {
                    EngineNS.Editor.Runner.RunnerManager.BreakContext breakContext = new EngineNS.Editor.Runner.RunnerManager.BreakContext();
                    breakContext.ThisObject = this;
                    breakContext.DebuggerId = EngineNS.Rtti.RttiHelper.GuidTryParse("3df25d67-48f9-4dd1-bd6a-efa385295e3c");
                    breakContext.BreakId = EngineNS.Rtti.RttiHelper.GuidTryParse("abe157d2-57e6-49f6-a358-837c4077bf96");
                    breakContext.ClassName = "mergeinstance";
                    breakContext.ValueContext = mDebuggerContext;
                    EngineNS.Editor.Runner.RunnerManager.Instance.Break(breakContext);
                }
#endif
                return ((System.Boolean)(true));
            }
            catch (System.Exception ex_b594976f_0b93_4761_a3c3_80bf4de022ab)
            {
                EngineNS.Profiler.Log.WriteException(ex_b594976f_0b93_4761_a3c3_80bf4de022ab, "Macross异常");
            }
            finally
            {
                _mScope.End();
            }
            return false;
        }
#pragma warning restore 1998
// OverrideEnd OnGameStart
        public static bool BreakEnable_b594976f_0b93_4761_a3c3_80bf4de022ab = false;
        public static bool BreakEnable_85fc5145_9ccd_492d_8765_1ff9f3143348 = false;
        public static bool BreakEnable_31c01579_1b6b_4c9e_a26f_d2a12f8db9fa = false;
        public static bool BreakEnable_c6b19e6f_8dd1_4b31_b606_143708162aea = false;
        public static bool BreakEnable_f80c6ddc_9de2_4340_8fb8_9aa609f4fc6d = false;
        public static bool BreakEnable_40a8b59c_85b1_4cff_88f5_5565c4d07e1d = false;
        public static bool BreakEnable_92feeffa_d1fd_42f5_94e7_302259ab9a94 = false;
        public static bool BreakEnable_7aeea6c8_fe4b_4217_b9f6_f52089613010 = false;
        public static bool BreakEnable_714046c8_aa97_42d9_ace5_bacd20c3bf50 = false;
        public static bool BreakEnable_da806233_d514_4267_b93c_d2e85412d639 = false;
        public static bool BreakEnable_7852684b_6294_43a2_906d_a7cf0aed783f = false;
        public static bool BreakEnable_f204aef2_6c97_4536_8586_f2091eb93c09 = false;
        public static bool BreakEnable_83422356_fd9b_48cb_b4ca_3657e5f2512b = false;
        public static bool BreakEnable_cdc078c5_122c_46db_9e91_141713cd1ebf = false;
        public static bool BreakEnable_83f93c60_16cf_41c5_9a3f_85830393a8c5 = false;
        public static bool BreakEnable_a1c18c85_8e5e_4569_91af_055cc977cb3f = false;
        public static bool BreakEnable_f4cf9495_2797_4c62_a014_3def88f0fdaa = false;
        public static bool BreakEnable_9c785c51_782a_45d8_8187_c84bd0209952 = false;
        public static bool BreakEnable_0255cf1a_fb5d_40fd_a2ec_482529cacd92 = false;
        public static bool BreakEnable_50e553de_eb51_4a64_a758_c2f3e96e6e29 = false;
        public static bool BreakEnable_abe157d2_57e6_49f6_a358_837c4077bf96 = false;
#pragma warning disable 1998
        [EngineNS.Editor.MacrossMemberAttribute(EngineNS.Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        [EngineNS.Editor.Editor_Guid("66971ad5-13b9-4799-a407-4f4d9dcc717f")]
        public async System.Threading.Tasks.Task CreateMergedMeshActors(EngineNS.GamePlay.GGameInstance game)
        {
            System.Guid param_2ab2c220_15de_4dd4_b98b_e76cd1611918_sceneId = new System.Guid();
            EngineNS.GamePlay.Actor.GActor param_2ab2c220_15de_4dd4_b98b_e76cd1611918_actor = null;
            EngineNS.GamePlay.GGameInstance param_2ab2c220_15de_4dd4_b98b_e76cd1611918_game = null;
            bool methodReturnValue_2ab2c220_15de_4dd4_b98b_e76cd1611918 = new bool();
            int arithmetic_6c1ab948_655c_487c_a816_1e8249b2809a_Value2 = new int();
            float arithmeticResult_6c1ab948_655c_487c_a816_1e8249b2809a = new float();
            EngineNS.Vector3 vec_7f65ecbf_c5bf_4fa0_a38a_ea9b66d108c9 = new EngineNS.Vector3();
            EngineNS.RName param_f125aa98_94ff_4619_b60d_72d384437db7_gms = EngineNS.CEngine.Instance.FileManager.GetRName("editor/basemesh/box.gms", EngineNS.RName.enRNameType.Game);
            EngineNS.GamePlay.Actor.GActor methodReturnValue_f125aa98_94ff_4619_b60d_72d384437db7 = null;
            System.Guid param_99540365_05b6_44cc_9063_c591f8b65e3b_sceneId = new System.Guid();
            EngineNS.GamePlay.Actor.GActor param_99540365_05b6_44cc_9063_c591f8b65e3b_actor = null;
            EngineNS.GamePlay.GGameInstance param_99540365_05b6_44cc_9063_c591f8b65e3b_game = null;
            bool methodReturnValue_99540365_05b6_44cc_9063_c591f8b65e3b = new bool();
            int arithmetic_3efb1891_acc6_4575_970f_f833bfec28f8_Value2 = new int();
            float arithmeticResult_3efb1891_acc6_4575_970f_f833bfec28f8 = new float();
            EngineNS.Vector3 vec_2bb15ac0_fd6e_4c43_9e96_0c70cb17f91a = new EngineNS.Vector3();
            EngineNS.RName param_52934b5a_9d31_484d_b135_cc3f35f5b04a_gms = EngineNS.CEngine.Instance.FileManager.GetRName("editor/basemesh/sphere.gms", EngineNS.RName.enRNameType.Game);
            EngineNS.GamePlay.Actor.GActor methodReturnValue_52934b5a_9d31_484d_b135_cc3f35f5b04a = null;
            try
            {
                _mScope.Begin();
#if MacrossDebug
                mDebuggerContext.ParamPin_86b2347b_c593_4631_965d_17c37f162f38 = game;
                if (BreakEnable_26d76ec1_e51c_4ef8_a147_3e476175c384)
                {
                    EngineNS.Editor.Runner.RunnerManager.BreakContext breakContext = new EngineNS.Editor.Runner.RunnerManager.BreakContext();
                    breakContext.ThisObject = this;
                    breakContext.DebuggerId = EngineNS.Rtti.RttiHelper.GuidTryParse("66971ad5-13b9-4799-a407-4f4d9dcc717f");
                    breakContext.BreakId = EngineNS.Rtti.RttiHelper.GuidTryParse("26d76ec1-e51c-4ef8-a147-3e476175c384");
                    breakContext.ClassName = "mergeinstance";
                    breakContext.ValueContext = mDebuggerContext;
                    EngineNS.Editor.Runner.RunnerManager.Instance.Break(breakContext);
                    game = ((mDebuggerContext.ParamPin_86b2347b_c593_4631_965d_17c37f162f38) as EngineNS.GamePlay.GGameInstance);
                }
#endif
                for (int index_9fc1646d_6b2a_4e47_b1ad_e340464694fd = 0; (index_9fc1646d_6b2a_4e47_b1ad_e340464694fd <= 5); index_9fc1646d_6b2a_4e47_b1ad_e340464694fd = (index_9fc1646d_6b2a_4e47_b1ad_e340464694fd + 1))
                {
#if MacrossDebug
                    mDebuggerContext.ParamPin_0de3fd4f_5e1a_4586_a22f_549ca50ca939 = param_52934b5a_9d31_484d_b135_cc3f35f5b04a_gms;
                    if (BreakEnable_52934b5a_9d31_484d_b135_cc3f35f5b04a)
                    {
                        EngineNS.Editor.Runner.RunnerManager.BreakContext breakContext = new EngineNS.Editor.Runner.RunnerManager.BreakContext();
                        breakContext.ThisObject = this;
                        breakContext.DebuggerId = EngineNS.Rtti.RttiHelper.GuidTryParse("66971ad5-13b9-4799-a407-4f4d9dcc717f");
                        breakContext.BreakId = EngineNS.Rtti.RttiHelper.GuidTryParse("52934b5a-9d31-484d-b135-cc3f35f5b04a");
                        breakContext.ClassName = "mergeinstance";
                        breakContext.ValueContext = mDebuggerContext;
                        EngineNS.Editor.Runner.RunnerManager.Instance.Break(breakContext);
                        param_52934b5a_9d31_484d_b135_cc3f35f5b04a_gms = ((mDebuggerContext.ParamPin_0de3fd4f_5e1a_4586_a22f_549ca50ca939) as EngineNS.RName);
                    }
#endif
                    methodReturnValue_52934b5a_9d31_484d_b135_cc3f35f5b04a = ((await EngineNS.GamePlay.Actor.GActor.NewMeshActorAsync(param_52934b5a_9d31_484d_b135_cc3f35f5b04a_gms)) as EngineNS.GamePlay.Actor.GActor);
#if MacrossDebug
                    mDebuggerContext.returnLink_52934b5a_9d31_484d_b135_cc3f35f5b04a = methodReturnValue_52934b5a_9d31_484d_b135_cc3f35f5b04a;
#endif
#if MacrossDebug
                    mDebuggerContext.ValueOutHandle_5022c11a_fc6f_4921_997e_f7b9ca1ac521 = methodReturnValue_52934b5a_9d31_484d_b135_cc3f35f5b04a.Placement;
#endif
#if MacrossDebug
                    mDebuggerContext.ValueInHandle_1d40409c_1cff_4d80_8ae4_6e5f60a8c20f = methodReturnValue_52934b5a_9d31_484d_b135_cc3f35f5b04a.Placement.Location;
                    if (BreakEnable_1d40409c_1cff_4d80_8ae4_6e5f60a8c20f)
                    {
                        EngineNS.Editor.Runner.RunnerManager.BreakContext breakContext = new EngineNS.Editor.Runner.RunnerManager.BreakContext();
                        breakContext.ThisObject = this;
                        breakContext.DebuggerId = EngineNS.Rtti.RttiHelper.GuidTryParse("66971ad5-13b9-4799-a407-4f4d9dcc717f");
                        breakContext.BreakId = EngineNS.Rtti.RttiHelper.GuidTryParse("1d40409c-1cff-4d80-8ae4-6e5f60a8c20f");
                        breakContext.ClassName = "mergeinstance";
                        breakContext.ValueContext = mDebuggerContext;
                        EngineNS.Editor.Runner.RunnerManager.Instance.Break(breakContext);
                    }
#endif
                    vec_2bb15ac0_fd6e_4c43_9e96_0c70cb17f91a = new EngineNS.Vector3(0F, 2F, 0F);
                    arithmetic_3efb1891_acc6_4575_970f_f833bfec28f8_Value2 = index_9fc1646d_6b2a_4e47_b1ad_e340464694fd;
#if MacrossDebug
                    mDebuggerContext.Value1_3efb1891_acc6_4575_970f_f833bfec28f8 = 1.5F;
                    mDebuggerContext.Value2_3efb1891_acc6_4575_970f_f833bfec28f8 = arithmetic_3efb1891_acc6_4575_970f_f833bfec28f8_Value2;
                    if (BreakEnable_3efb1891_acc6_4575_970f_f833bfec28f8)
                    {
                        EngineNS.Editor.Runner.RunnerManager.BreakContext breakContext = new EngineNS.Editor.Runner.RunnerManager.BreakContext();
                        breakContext.ThisObject = this;
                        breakContext.DebuggerId = EngineNS.Rtti.RttiHelper.GuidTryParse("66971ad5-13b9-4799-a407-4f4d9dcc717f");
                        breakContext.BreakId = EngineNS.Rtti.RttiHelper.GuidTryParse("3efb1891-acc6-4575-970f-f833bfec28f8");
                        breakContext.ClassName = "mergeinstance";
                        breakContext.ValueContext = mDebuggerContext;
                        EngineNS.Editor.Runner.RunnerManager.Instance.Break(breakContext);
                        arithmetic_3efb1891_acc6_4575_970f_f833bfec28f8_Value2 = ((System.Int32)(mDebuggerContext.Value2_3efb1891_acc6_4575_970f_f833bfec28f8));
                    }
#endif
                    arithmeticResult_3efb1891_acc6_4575_970f_f833bfec28f8 = (1.5F * arithmetic_3efb1891_acc6_4575_970f_f833bfec28f8_Value2);
#if MacrossDebug
                    mDebuggerContext.ResultLink_3efb1891_acc6_4575_970f_f833bfec28f8 = arithmeticResult_3efb1891_acc6_4575_970f_f833bfec28f8;
#endif
                    vec_2bb15ac0_fd6e_4c43_9e96_0c70cb17f91a.X = ((System.Single)(arithmeticResult_3efb1891_acc6_4575_970f_f833bfec28f8));
                    methodReturnValue_52934b5a_9d31_484d_b135_cc3f35f5b04a.Placement.Location = ((EngineNS.Vector3)(vec_2bb15ac0_fd6e_4c43_9e96_0c70cb17f91a));
#if MacrossDebug
                    mDebuggerContext.ValueInHandle_1d40409c_1cff_4d80_8ae4_6e5f60a8c20f = methodReturnValue_52934b5a_9d31_484d_b135_cc3f35f5b04a.Placement.Location;
#endif
#if MacrossDebug
                    mDebuggerContext.ValueOutHandle_c22cf83c_dc82_44da_9c1c_0fe0b94c8550 = this.ListActors;
#endif
                    this.ListActors.Add(methodReturnValue_52934b5a_9d31_484d_b135_cc3f35f5b04a);
#if MacrossDebug
                    mDebuggerContext.ValueInHandle_c2e33d88_6a4b_45a7_83af_cb316ee57af0 = methodReturnValue_52934b5a_9d31_484d_b135_cc3f35f5b04a.Visible;
                    if (BreakEnable_c2e33d88_6a4b_45a7_83af_cb316ee57af0)
                    {
                        EngineNS.Editor.Runner.RunnerManager.BreakContext breakContext = new EngineNS.Editor.Runner.RunnerManager.BreakContext();
                        breakContext.ThisObject = this;
                        breakContext.DebuggerId = EngineNS.Rtti.RttiHelper.GuidTryParse("66971ad5-13b9-4799-a407-4f4d9dcc717f");
                        breakContext.BreakId = EngineNS.Rtti.RttiHelper.GuidTryParse("c2e33d88-6a4b-45a7-83af-cb316ee57af0");
                        breakContext.ClassName = "mergeinstance";
                        breakContext.ValueContext = mDebuggerContext;
                        EngineNS.Editor.Runner.RunnerManager.Instance.Break(breakContext);
                    }
#endif
                    methodReturnValue_52934b5a_9d31_484d_b135_cc3f35f5b04a.Visible = false;
#if MacrossDebug
                    mDebuggerContext.ValueInHandle_c2e33d88_6a4b_45a7_83af_cb316ee57af0 = methodReturnValue_52934b5a_9d31_484d_b135_cc3f35f5b04a.Visible;
#endif
                    param_99540365_05b6_44cc_9063_c591f8b65e3b_game = ((game) as EngineNS.GamePlay.GGameInstance);
                    param_99540365_05b6_44cc_9063_c591f8b65e3b_actor = ((methodReturnValue_52934b5a_9d31_484d_b135_cc3f35f5b04a) as EngineNS.GamePlay.Actor.GActor);
#if MacrossDebug
                    mDebuggerContext.ValueOutHandle_4bfd1ca6_4bcc_4f88_950f_018ff336efbb = EngineNS.CEngine.Instance.GameInstance.World.FindActor(EngineNS.Rtti.RttiHelper.GuidTryParse("26f8d140-27da-417f-a188-a8786b82fd5e")).Scene.SceneId;
#endif
                    param_99540365_05b6_44cc_9063_c591f8b65e3b_sceneId = ((System.Guid)(EngineNS.CEngine.Instance.GameInstance.World.FindActor(EngineNS.Rtti.RttiHelper.GuidTryParse("26f8d140-27da-417f-a188-a8786b82fd5e")).Scene.SceneId));
#if MacrossDebug
                    mDebuggerContext.ParamPin_3a0f1461_adab_4a86_89fb_29712153086d = param_99540365_05b6_44cc_9063_c591f8b65e3b_game;
                    mDebuggerContext.ParamPin_5345e78b_0343_4869_ac9b_1316053bd3a5 = param_99540365_05b6_44cc_9063_c591f8b65e3b_actor;
                    mDebuggerContext.ParamPin_8ccf88e3_dbcd_4ae1_80a5_c608045154b7 = param_99540365_05b6_44cc_9063_c591f8b65e3b_sceneId;
                    if (BreakEnable_99540365_05b6_44cc_9063_c591f8b65e3b)
                    {
                        EngineNS.Editor.Runner.RunnerManager.BreakContext breakContext = new EngineNS.Editor.Runner.RunnerManager.BreakContext();
                        breakContext.ThisObject = this;
                        breakContext.DebuggerId = EngineNS.Rtti.RttiHelper.GuidTryParse("66971ad5-13b9-4799-a407-4f4d9dcc717f");
                        breakContext.BreakId = EngineNS.Rtti.RttiHelper.GuidTryParse("99540365-05b6-44cc-9063-c591f8b65e3b");
                        breakContext.ClassName = "mergeinstance";
                        breakContext.ValueContext = mDebuggerContext;
                        EngineNS.Editor.Runner.RunnerManager.Instance.Break(breakContext);
                        param_99540365_05b6_44cc_9063_c591f8b65e3b_game = ((mDebuggerContext.ParamPin_3a0f1461_adab_4a86_89fb_29712153086d) as EngineNS.GamePlay.GGameInstance);
                        param_99540365_05b6_44cc_9063_c591f8b65e3b_actor = ((mDebuggerContext.ParamPin_5345e78b_0343_4869_ac9b_1316053bd3a5) as EngineNS.GamePlay.Actor.GActor);
                        param_99540365_05b6_44cc_9063_c591f8b65e3b_sceneId = ((System.Guid)(mDebuggerContext.ParamPin_8ccf88e3_dbcd_4ae1_80a5_c608045154b7));
                    }
#endif
                    methodReturnValue_99540365_05b6_44cc_9063_c591f8b65e3b = ((System.Boolean)(EngineNS.GamePlay.McGameInstance.AddActor2Scene(param_99540365_05b6_44cc_9063_c591f8b65e3b_game, param_99540365_05b6_44cc_9063_c591f8b65e3b_actor, param_99540365_05b6_44cc_9063_c591f8b65e3b_sceneId)));
                }
                for (int index_e5925a95_2210_4fe5_b7da_0662a63de055 = 0; (index_e5925a95_2210_4fe5_b7da_0662a63de055 <= 5); index_e5925a95_2210_4fe5_b7da_0662a63de055 = (index_e5925a95_2210_4fe5_b7da_0662a63de055 + 1))
                {
#if MacrossDebug
                    mDebuggerContext.ParamPin_c643fefd_fb74_41e2_a70e_a9192b514b05 = param_f125aa98_94ff_4619_b60d_72d384437db7_gms;
                    if (BreakEnable_f125aa98_94ff_4619_b60d_72d384437db7)
                    {
                        EngineNS.Editor.Runner.RunnerManager.BreakContext breakContext = new EngineNS.Editor.Runner.RunnerManager.BreakContext();
                        breakContext.ThisObject = this;
                        breakContext.DebuggerId = EngineNS.Rtti.RttiHelper.GuidTryParse("66971ad5-13b9-4799-a407-4f4d9dcc717f");
                        breakContext.BreakId = EngineNS.Rtti.RttiHelper.GuidTryParse("f125aa98-94ff-4619-b60d-72d384437db7");
                        breakContext.ClassName = "mergeinstance";
                        breakContext.ValueContext = mDebuggerContext;
                        EngineNS.Editor.Runner.RunnerManager.Instance.Break(breakContext);
                        param_f125aa98_94ff_4619_b60d_72d384437db7_gms = ((mDebuggerContext.ParamPin_c643fefd_fb74_41e2_a70e_a9192b514b05) as EngineNS.RName);
                    }
#endif
                    methodReturnValue_f125aa98_94ff_4619_b60d_72d384437db7 = ((await EngineNS.GamePlay.Actor.GActor.NewMeshActorAsync(param_f125aa98_94ff_4619_b60d_72d384437db7_gms)) as EngineNS.GamePlay.Actor.GActor);
#if MacrossDebug
                    mDebuggerContext.returnLink_f125aa98_94ff_4619_b60d_72d384437db7 = methodReturnValue_f125aa98_94ff_4619_b60d_72d384437db7;
#endif
#if MacrossDebug
                    mDebuggerContext.ValueOutHandle_9181e402_e58f_49ef_900d_e16e3c0a7340 = methodReturnValue_f125aa98_94ff_4619_b60d_72d384437db7.Placement;
#endif
#if MacrossDebug
                    mDebuggerContext.ValueInHandle_63734710_55c8_4e65_a397_695290fc3690 = methodReturnValue_f125aa98_94ff_4619_b60d_72d384437db7.Placement.Location;
                    if (BreakEnable_63734710_55c8_4e65_a397_695290fc3690)
                    {
                        EngineNS.Editor.Runner.RunnerManager.BreakContext breakContext = new EngineNS.Editor.Runner.RunnerManager.BreakContext();
                        breakContext.ThisObject = this;
                        breakContext.DebuggerId = EngineNS.Rtti.RttiHelper.GuidTryParse("66971ad5-13b9-4799-a407-4f4d9dcc717f");
                        breakContext.BreakId = EngineNS.Rtti.RttiHelper.GuidTryParse("63734710-55c8-4e65-a397-695290fc3690");
                        breakContext.ClassName = "mergeinstance";
                        breakContext.ValueContext = mDebuggerContext;
                        EngineNS.Editor.Runner.RunnerManager.Instance.Break(breakContext);
                    }
#endif
                    vec_7f65ecbf_c5bf_4fa0_a38a_ea9b66d108c9 = new EngineNS.Vector3(0F, 2F, 0F);
                    arithmetic_6c1ab948_655c_487c_a816_1e8249b2809a_Value2 = index_e5925a95_2210_4fe5_b7da_0662a63de055;
#if MacrossDebug
                    mDebuggerContext.Value1_6c1ab948_655c_487c_a816_1e8249b2809a = 1.5F;
                    mDebuggerContext.Value2_6c1ab948_655c_487c_a816_1e8249b2809a = arithmetic_6c1ab948_655c_487c_a816_1e8249b2809a_Value2;
                    if (BreakEnable_6c1ab948_655c_487c_a816_1e8249b2809a)
                    {
                        EngineNS.Editor.Runner.RunnerManager.BreakContext breakContext = new EngineNS.Editor.Runner.RunnerManager.BreakContext();
                        breakContext.ThisObject = this;
                        breakContext.DebuggerId = EngineNS.Rtti.RttiHelper.GuidTryParse("66971ad5-13b9-4799-a407-4f4d9dcc717f");
                        breakContext.BreakId = EngineNS.Rtti.RttiHelper.GuidTryParse("6c1ab948-655c-487c-a816-1e8249b2809a");
                        breakContext.ClassName = "mergeinstance";
                        breakContext.ValueContext = mDebuggerContext;
                        EngineNS.Editor.Runner.RunnerManager.Instance.Break(breakContext);
                        arithmetic_6c1ab948_655c_487c_a816_1e8249b2809a_Value2 = ((System.Int32)(mDebuggerContext.Value2_6c1ab948_655c_487c_a816_1e8249b2809a));
                    }
#endif
                    arithmeticResult_6c1ab948_655c_487c_a816_1e8249b2809a = (1.5F * arithmetic_6c1ab948_655c_487c_a816_1e8249b2809a_Value2);
#if MacrossDebug
                    mDebuggerContext.ResultLink_6c1ab948_655c_487c_a816_1e8249b2809a = arithmeticResult_6c1ab948_655c_487c_a816_1e8249b2809a;
#endif
                    vec_7f65ecbf_c5bf_4fa0_a38a_ea9b66d108c9.Y = ((System.Single)(arithmeticResult_6c1ab948_655c_487c_a816_1e8249b2809a));
                    methodReturnValue_f125aa98_94ff_4619_b60d_72d384437db7.Placement.Location = ((EngineNS.Vector3)(vec_7f65ecbf_c5bf_4fa0_a38a_ea9b66d108c9));
#if MacrossDebug
                    mDebuggerContext.ValueInHandle_63734710_55c8_4e65_a397_695290fc3690 = methodReturnValue_f125aa98_94ff_4619_b60d_72d384437db7.Placement.Location;
#endif
#if MacrossDebug
                    mDebuggerContext.ValueOutHandle_c22cf83c_dc82_44da_9c1c_0fe0b94c8550 = this.ListActors;
#endif
                    this.ListActors.Add(methodReturnValue_f125aa98_94ff_4619_b60d_72d384437db7);
#if MacrossDebug
                    mDebuggerContext.ValueInHandle_47048462_97dd_429b_b0f8_6dca6ceb4df9 = methodReturnValue_f125aa98_94ff_4619_b60d_72d384437db7.Visible;
                    if (BreakEnable_47048462_97dd_429b_b0f8_6dca6ceb4df9)
                    {
                        EngineNS.Editor.Runner.RunnerManager.BreakContext breakContext = new EngineNS.Editor.Runner.RunnerManager.BreakContext();
                        breakContext.ThisObject = this;
                        breakContext.DebuggerId = EngineNS.Rtti.RttiHelper.GuidTryParse("66971ad5-13b9-4799-a407-4f4d9dcc717f");
                        breakContext.BreakId = EngineNS.Rtti.RttiHelper.GuidTryParse("47048462-97dd-429b-b0f8-6dca6ceb4df9");
                        breakContext.ClassName = "mergeinstance";
                        breakContext.ValueContext = mDebuggerContext;
                        EngineNS.Editor.Runner.RunnerManager.Instance.Break(breakContext);
                    }
#endif
                    methodReturnValue_f125aa98_94ff_4619_b60d_72d384437db7.Visible = false;
#if MacrossDebug
                    mDebuggerContext.ValueInHandle_47048462_97dd_429b_b0f8_6dca6ceb4df9 = methodReturnValue_f125aa98_94ff_4619_b60d_72d384437db7.Visible;
#endif
                    param_2ab2c220_15de_4dd4_b98b_e76cd1611918_game = ((game) as EngineNS.GamePlay.GGameInstance);
                    param_2ab2c220_15de_4dd4_b98b_e76cd1611918_actor = ((methodReturnValue_f125aa98_94ff_4619_b60d_72d384437db7) as EngineNS.GamePlay.Actor.GActor);
#if MacrossDebug
                    mDebuggerContext.ValueOutHandle_4bfd1ca6_4bcc_4f88_950f_018ff336efbb = EngineNS.CEngine.Instance.GameInstance.World.FindActor(EngineNS.Rtti.RttiHelper.GuidTryParse("26f8d140-27da-417f-a188-a8786b82fd5e")).Scene.SceneId;
#endif
                    param_2ab2c220_15de_4dd4_b98b_e76cd1611918_sceneId = ((System.Guid)(EngineNS.CEngine.Instance.GameInstance.World.FindActor(EngineNS.Rtti.RttiHelper.GuidTryParse("26f8d140-27da-417f-a188-a8786b82fd5e")).Scene.SceneId));
#if MacrossDebug
                    mDebuggerContext.ParamPin_72ae126c_e3b3_4c85_961b_95cf875b239a = param_2ab2c220_15de_4dd4_b98b_e76cd1611918_game;
                    mDebuggerContext.ParamPin_f5ae7b99_f684_4eec_904a_49f932a9d1a0 = param_2ab2c220_15de_4dd4_b98b_e76cd1611918_actor;
                    mDebuggerContext.ParamPin_dda0ceb2_ed24_428c_bba9_9e8066d48e35 = param_2ab2c220_15de_4dd4_b98b_e76cd1611918_sceneId;
                    if (BreakEnable_2ab2c220_15de_4dd4_b98b_e76cd1611918)
                    {
                        EngineNS.Editor.Runner.RunnerManager.BreakContext breakContext = new EngineNS.Editor.Runner.RunnerManager.BreakContext();
                        breakContext.ThisObject = this;
                        breakContext.DebuggerId = EngineNS.Rtti.RttiHelper.GuidTryParse("66971ad5-13b9-4799-a407-4f4d9dcc717f");
                        breakContext.BreakId = EngineNS.Rtti.RttiHelper.GuidTryParse("2ab2c220-15de-4dd4-b98b-e76cd1611918");
                        breakContext.ClassName = "mergeinstance";
                        breakContext.ValueContext = mDebuggerContext;
                        EngineNS.Editor.Runner.RunnerManager.Instance.Break(breakContext);
                        param_2ab2c220_15de_4dd4_b98b_e76cd1611918_game = ((mDebuggerContext.ParamPin_72ae126c_e3b3_4c85_961b_95cf875b239a) as EngineNS.GamePlay.GGameInstance);
                        param_2ab2c220_15de_4dd4_b98b_e76cd1611918_actor = ((mDebuggerContext.ParamPin_f5ae7b99_f684_4eec_904a_49f932a9d1a0) as EngineNS.GamePlay.Actor.GActor);
                        param_2ab2c220_15de_4dd4_b98b_e76cd1611918_sceneId = ((System.Guid)(mDebuggerContext.ParamPin_dda0ceb2_ed24_428c_bba9_9e8066d48e35));
                    }
#endif
                    methodReturnValue_2ab2c220_15de_4dd4_b98b_e76cd1611918 = ((System.Boolean)(EngineNS.GamePlay.McGameInstance.AddActor2Scene(param_2ab2c220_15de_4dd4_b98b_e76cd1611918_game, param_2ab2c220_15de_4dd4_b98b_e76cd1611918_actor, param_2ab2c220_15de_4dd4_b98b_e76cd1611918_sceneId)));
                }
            }
            catch (System.Exception ex_26d76ec1_e51c_4ef8_a147_3e476175c384)
            {
                EngineNS.Profiler.Log.WriteException(ex_26d76ec1_e51c_4ef8_a147_3e476175c384, "Macross异常");
            }
            finally
            {
                _mScope.End();
            }
        }
#pragma warning restore 1998
        public static bool BreakEnable_26d76ec1_e51c_4ef8_a147_3e476175c384 = false;
        public static bool BreakEnable_52934b5a_9d31_484d_b135_cc3f35f5b04a = false;
        public static bool BreakEnable_1d40409c_1cff_4d80_8ae4_6e5f60a8c20f = false;
        public static bool BreakEnable_3efb1891_acc6_4575_970f_f833bfec28f8 = false;
        public static bool BreakEnable_c2e33d88_6a4b_45a7_83af_cb316ee57af0 = false;
        public static bool BreakEnable_99540365_05b6_44cc_9063_c591f8b65e3b = false;
        public static bool BreakEnable_f125aa98_94ff_4619_b60d_72d384437db7 = false;
        public static bool BreakEnable_63734710_55c8_4e65_a397_695290fc3690 = false;
        public static bool BreakEnable_6c1ab948_655c_487c_a816_1e8249b2809a = false;
        public static bool BreakEnable_47048462_97dd_429b_b0f8_6dca6ceb4df9 = false;
        public static bool BreakEnable_2ab2c220_15de_4dd4_b98b_e76cd1611918 = false;
#pragma warning disable 1998
        [EngineNS.Editor.MacrossMemberAttribute(EngineNS.Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        [EngineNS.Editor.Editor_Guid("7a0945cb-f6c3-4578-828d-27b6baec9bf2")]
        public void NewFunction_0(samplers.tpsgame.tpscenterdata InParams_0)
        {
            try
            {
                _mScope.Begin();
#if MacrossDebug
                mDebuggerContext.ParamPin_9df7c360_fdfa_4db4_b9d8_b90fc2ac0c49 = InParams_0;
                if (BreakEnable_f78bb1d3_f253_4afc_9e9e_b297820093a1)
                {
                    EngineNS.Editor.Runner.RunnerManager.BreakContext breakContext = new EngineNS.Editor.Runner.RunnerManager.BreakContext();
                    breakContext.ThisObject = this;
                    breakContext.DebuggerId = EngineNS.Rtti.RttiHelper.GuidTryParse("7a0945cb-f6c3-4578-828d-27b6baec9bf2");
                    breakContext.BreakId = EngineNS.Rtti.RttiHelper.GuidTryParse("f78bb1d3-f253-4afc-9e9e-b297820093a1");
                    breakContext.ClassName = "mergeinstance";
                    breakContext.ValueContext = mDebuggerContext;
                    EngineNS.Editor.Runner.RunnerManager.Instance.Break(breakContext);
                    InParams_0 = ((mDebuggerContext.ParamPin_9df7c360_fdfa_4db4_b9d8_b90fc2ac0c49) as samplers.tpsgame.tpscenterdata);
                }
#endif
            }
            catch (System.Exception ex_f78bb1d3_f253_4afc_9e9e_b297820093a1)
            {
                EngineNS.Profiler.Log.WriteException(ex_f78bb1d3_f253_4afc_9e9e_b297820093a1, "Macross异常");
            }
            finally
            {
                _mScope.End();
            }
        }
#pragma warning restore 1998
        public static bool BreakEnable_f78bb1d3_f253_4afc_9e9e_b297820093a1 = false;
    }
    public class DebugContext_mergeinstance
    {
        public EngineNS.GamePlay.GGameInstance ParamPin_ede4eede_8dc7_4512_afb3_ac69c8cbf227;
        public EngineNS.Bricks.GpuDriven.GpuScene.SceneDataManager ValueInHandle_85fc5145_9ccd_492d_8765_1ff9f3143348;
        public EngineNS.Bricks.GpuDriven.GpuScene.SceneDataManager ValueOutHandle_85fc5145_9ccd_492d_8765_1ff9f3143348;
        public EngineNS.CRenderContext ParamPin_610c7def_3544_4d63_86ab_f5b9446df709;
        public EngineNS.Graphics.CGfxMaterialInstance ParamPin_16cb3a6e_0098_42d2_adb6_cfe00ce358ff;
        public bool ParamPin_ed1452c8_94f4_4d2c_b634_e25be300cd1e;
        public EngineNS.CRenderContext ParamPin_34799286_fb44_45ed_9b6e_ab811eda84c4;
        public EngineNS.RName ParamPin_48dea2b4_12d2_4b37_a66d_d721a289a51a;
        public bool ParamPin_9fcc6259_431f_434a_86e7_1d8743693f5d;
        public bool ParamPin_ee5efc0c_4110_437e_a642_c175e1e99d69;
        public EngineNS.GamePlay.SceneGraph.GSceneGraph returnLink_c6b19e6f_8dd1_4b31_b606_143708162aea;
        public int ParamPin_a3673d73_2431_4fc0_87c9_beb423565e07;
        public EngineNS.Vector3 returnLink_f80c6ddc_9de2_4340_8fb8_9aa609f4fc6d;
        public EngineNS.RName ParamPin_c4600f9c_4472_4d5c_b3dd_557cc8758eef;
        public EngineNS.GamePlay.SceneGraph.GSceneGraph ParamPin_3b6c8d5f_4acc_465b_95ac_c2adf5685150;
        public EngineNS.Vector3 ParamPin_9726a714_b714_4888_a571_460b819b2768;
        public EngineNS.Quaternion ParamPin_3e292893_be46_4300_b2fe_38c912539863;
        public EngineNS.Vector3 ParamPin_eea7cb7c_73f2_49f7_82f2_0d3167af8fbe;
        public EngineNS.GamePlay.Actor.GActor returnLink_40a8b59c_85b1_4cff_88f5_5565c4d07e1d;
        public EngineNS.GamePlay.Actor.GActor ParamPin_9146aee1_1f54_43b4_b6b4_443a804082db;
        public EngineNS.GamePlay.Actor.GActor ValueInHandle_7aeea6c8_fe4b_4217_b9f6_f52089613010;
        public EngineNS.GamePlay.GGameInstance ParamPin_315d1ac5_dc38_4032_ab30_64548b6f32e9;
        public System.Collections.Generic.List<EngineNS.GamePlay.Actor.GActor> ValueOutHandle_f6701d27_f00d_434f_a131_0a4b15ebe982;
        public string ParamPin_42422c54_c8c4_4659_a7fc_6b1342994b1c;
        public EngineNS.RName.enRNameType ParamPin_3590197f_e550_4a7c_a6e9_204f6ca357bd;
        public EngineNS.RName returnLink_7852684b_6294_43a2_906d_a7cf0aed783f;
        public string ParamPin_0c8467c6_5bc9_43b3_a849_b0666264d231;
        public EngineNS.Graphics.Mesh.CGfxMesh returnLink_f204aef2_6c97_4536_8586_f2091eb93c09;
        public samplers.tpsgame.tpscenterdata ValueInHandle_83422356_fd9b_48cb_b4ca_3657e5f2512b;
        public EngineNS.RName ValueOutHandle_4c779de7_3298_4ef1_b663_ef95c466dba3;
        public bool resultHandle_a2274167_7262_4435_946b_a37c7dd504ac;
        public bool userControl_a4b93072_16fc_4194_a828_9a76a990dead;
        public EngineNS.Bricks.GpuDriven.GpuScene.SceneDataManager ValueOutHandle_05a19218_8af3_4f1d_84da_3cfddc02506e;
        public EngineNS.GamePlay.Actor.GActor ParamPin_936c9d80_b656_445d_8bec_7e271e9a3450;
        public EngineNS.RName ParamPin_88ef3469_646d_4fdd_8184_401a8c60490d;
        public EngineNS.GamePlay.Actor.GActor ParamPin_828b9a26_60a5_4643_902c_734985c52ce3;
        public EngineNS.RName ParamPin_6807e3ae_4f7e_4889_aa82_163ab85cf1ee;
        public EngineNS.GamePlay.GGameInstance ParamPin_e1fe5550_46f7_4ec3_b0b8_a0a86b2981d7;
        public samplers.tpsgame.tpscenterdata ValueOutHandle_b65d0be7_26d4_4655_aa40_ec82f5d2fb75;
        public samplers.tpsgame.tpscenterdata ParamPin_c8647cbd_dec2_4801_8b84_2a391b5cab88;
        public EngineNS.UISystem.UIHost ValueOutHandle_78836361_0c19_43d9_ae81_17b411876125;
        public EngineNS.RName ParamPin_53882fea_aafa_4cf3_9fb1_7c238e2cc85a;
        public System.Type ParamPin_77537554_3bcc_4e86_93fd_dc70088d2569;
        public samplers.mergeinstance.ui_main returnLink_50e553de_eb51_4a64_a758_c2f3e96e6e29;
        public bool ParamPin_791144e8_f0c7_4d82_b32b_6251e111d65e;
        public EngineNS.GamePlay.GGameInstance ParamPin_86b2347b_c593_4631_965d_17c37f162f38;
        public EngineNS.RName ParamPin_0de3fd4f_5e1a_4586_a22f_549ca50ca939;
        public EngineNS.GamePlay.Actor.GActor returnLink_52934b5a_9d31_484d_b135_cc3f35f5b04a;
        public EngineNS.GamePlay.Component.GPlacementComponent ValueOutHandle_5022c11a_fc6f_4921_997e_f7b9ca1ac521;
        public EngineNS.Vector3 ValueInHandle_1d40409c_1cff_4d80_8ae4_6e5f60a8c20f;
        public float Value1_3efb1891_acc6_4575_970f_f833bfec28f8;
        public int Value2_3efb1891_acc6_4575_970f_f833bfec28f8;
        public float ResultLink_3efb1891_acc6_4575_970f_f833bfec28f8;
        public System.Collections.Generic.List<EngineNS.GamePlay.Actor.GActor> ValueOutHandle_c22cf83c_dc82_44da_9c1c_0fe0b94c8550;
        public bool ValueInHandle_c2e33d88_6a4b_45a7_83af_cb316ee57af0;
        public System.Guid ValueOutHandle_4bfd1ca6_4bcc_4f88_950f_018ff336efbb;
        public EngineNS.GamePlay.GGameInstance ParamPin_3a0f1461_adab_4a86_89fb_29712153086d;
        public EngineNS.GamePlay.Actor.GActor ParamPin_5345e78b_0343_4869_ac9b_1316053bd3a5;
        public System.Guid ParamPin_8ccf88e3_dbcd_4ae1_80a5_c608045154b7;
        public EngineNS.RName ParamPin_c643fefd_fb74_41e2_a70e_a9192b514b05;
        public EngineNS.GamePlay.Actor.GActor returnLink_f125aa98_94ff_4619_b60d_72d384437db7;
        public EngineNS.GamePlay.Component.GPlacementComponent ValueOutHandle_9181e402_e58f_49ef_900d_e16e3c0a7340;
        public EngineNS.Vector3 ValueInHandle_63734710_55c8_4e65_a397_695290fc3690;
        public float Value1_6c1ab948_655c_487c_a816_1e8249b2809a;
        public int Value2_6c1ab948_655c_487c_a816_1e8249b2809a;
        public float ResultLink_6c1ab948_655c_487c_a816_1e8249b2809a;
        public bool ValueInHandle_47048462_97dd_429b_b0f8_6dca6ceb4df9;
        public EngineNS.GamePlay.GGameInstance ParamPin_72ae126c_e3b3_4c85_961b_95cf875b239a;
        public EngineNS.GamePlay.Actor.GActor ParamPin_f5ae7b99_f684_4eec_904a_49f932a9d1a0;
        public System.Guid ParamPin_dda0ceb2_ed24_428c_bba9_9e8066d48e35;
        public samplers.tpsgame.tpscenterdata ParamPin_9df7c360_fdfa_4db4_b9d8_b90fc2ac0c49;
    }
}
