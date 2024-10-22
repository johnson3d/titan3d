namespace NS_tutorials.character
{
    [EngineNS.Macross.TtMacross]
    public partial class charactertestgame : EngineNS.GamePlay.UMacrossGame
    {
        public EngineNS.Macross.TtMacrossBreak breaker_FinalViewportSlate_196399066 = new EngineNS.Macross.TtMacrossBreak("breaker_FinalViewportSlate_196399066");
        public EngineNS.Macross.TtMacrossBreak breaker_InitViewportSlateWithScene_4202790171 = new EngineNS.Macross.TtMacrossBreak("breaker_InitViewportSlateWithScene_4202790171");
        public EngineNS.Macross.TtMacrossBreak breaker_CreateCharacterFromPrefab_4154512876 = new EngineNS.Macross.TtMacrossBreak("breaker_CreateCharacterFromPrefab_4154512876");
        public EngineNS.Macross.TtMacrossBreak breaker_return_2566250119 = new EngineNS.Macross.TtMacrossBreak("breaker_return_2566250119");
        EngineNS.Macross.TtMacrossStackFrame mFrame_BeginDestroy_2487905154 = new EngineNS.Macross.TtMacrossStackFrame(EngineNS.RName.GetRName("tutorials/character/charactertestgame.macross", EngineNS.RName.ERNameType.Game));
        [EngineNS.Rtti.MetaAttribute]
        public override void BeginDestroy(EngineNS.GamePlay.UGameInstance host)
        {
            using(var guard_BeginDestroy = new EngineNS.Macross.TtMacrossStackGuard(mFrame_BeginDestroy_2487905154))
            {
                mFrame_BeginDestroy_2487905154.SetWatchVariable("host", host);
                breaker_FinalViewportSlate_196399066.TryBreak();
                host.FinalViewportSlate();
            }
        }
        EngineNS.Macross.TtMacrossStackFrame mFrame_BeginPlay_2115264093 = new EngineNS.Macross.TtMacrossStackFrame(EngineNS.RName.GetRName("tutorials/character/charactertestgame.macross", EngineNS.RName.ERNameType.Game));
        [EngineNS.Rtti.MetaAttribute]
        public override async System.Threading.Tasks.Task<System.Boolean> BeginPlay(EngineNS.GamePlay.UGameInstance host)
        {
            using(var guard_BeginPlay = new EngineNS.Macross.TtMacrossStackGuard(mFrame_BeginPlay_2115264093))
            {
                System.Boolean ret_3332842155 = default(System.Boolean);
                mFrame_BeginPlay_2115264093.SetWatchVariable("host", host);
                EngineNS.GamePlay.Scene.TtScene tmp_r_InitViewportSlateWithScene_4202790171 = default(EngineNS.GamePlay.Scene.TtScene);
                mFrame_BeginPlay_2115264093.SetWatchVariable("v_mapName_InitViewportSlateWithScene_4202790171", EngineNS.RName.GetRName("tutorials/character/charactertest.scene", EngineNS.RName.ERNameType.Game));
                mFrame_BeginPlay_2115264093.SetWatchVariable("v_zMin_InitViewportSlateWithScene_4202790171", 0f);
                mFrame_BeginPlay_2115264093.SetWatchVariable("v_zMax_InitViewportSlateWithScene_4202790171", 1f);
                mFrame_BeginPlay_2115264093.SetWatchVariable("v_bSetToWorld_InitViewportSlateWithScene_4202790171", true);
                breaker_InitViewportSlateWithScene_4202790171.TryBreak();
                tmp_r_InitViewportSlateWithScene_4202790171 = (EngineNS.GamePlay.Scene.TtScene)await host.InitViewportSlateWithScene(EngineNS.RName.GetRName("tutorials/character/charactertest.scene", EngineNS.RName.ERNameType.Game),0f,1f,true);
                mFrame_BeginPlay_2115264093.SetWatchVariable("tmp_r_InitViewportSlateWithScene_4202790171", tmp_r_InitViewportSlateWithScene_4202790171);
                mFrame_BeginPlay_2115264093.SetWatchVariable("v_scene_CreateCharacterFromPrefab_4154512876", tmp_r_InitViewportSlateWithScene_4202790171);
                mFrame_BeginPlay_2115264093.SetWatchVariable("v_prefabName_CreateCharacterFromPrefab_4154512876", EngineNS.RName.GetRName("project_factory/belica/prefab_belica.prefab", EngineNS.RName.ERNameType.Game));
                breaker_CreateCharacterFromPrefab_4154512876.TryBreak();
                await host.CreateCharacterFromPrefab(tmp_r_InitViewportSlateWithScene_4202790171,EngineNS.RName.GetRName("project_factory/belica/prefab_belica.prefab", EngineNS.RName.ERNameType.Game));
                ret_3332842155 = true;
                mFrame_BeginPlay_2115264093.SetWatchVariable("ret_3332842155_2566250119", ret_3332842155);
                breaker_return_2566250119.TryBreak();
                return ret_3332842155;
                return ret_3332842155;
            }
        }
    }
}
