namespace NS_tutorials.material
{
    [EngineNS.Macross.TtMacross]
    [EngineNS.Macross.TtMacrossSign(RName_Name = "tutorials/material/test_material.macross", RName_Type = EngineNS.RName.ERNameType.Game)]
    public partial class test_material : EngineNS.GamePlay.TtMacrossGame
    {
        [EngineNS.Rtti.Meta]
        [System.ComponentModel.Category("Macross")]
        [System.ComponentModel.DisplayName("Member_0")]
        private EngineNS.GamePlay.Scene.TtMeshNode Member_0 { get; set; } = null;
        public static EngineNS.Macross.TtMacrossBreak breaker_InitViewportSlateWithScene_2890131646 = new EngineNS.Macross.TtMacrossBreak("breaker_InitViewportSlateWithScene_2890131646");
        public static EngineNS.Macross.TtMacrossBreak breaker_FindFirstChild_2034756304 = new EngineNS.Macross.TtMacrossBreak("breaker_FindFirstChild_2034756304");
        public static EngineNS.Macross.TtMacrossBreak breaker_return_1924148349 = new EngineNS.Macross.TtMacrossBreak("breaker_return_1924148349");
        public static EngineNS.Macross.TtMacrossBreak breaker_if_690337321 = new EngineNS.Macross.TtMacrossBreak("breaker_if_690337321");
        EngineNS.Macross.TtMacrossStackFrame mFrame_BeginPlay_1342966456 = new EngineNS.Macross.TtMacrossStackFrame(EngineNS.RName.GetRName("tutorials/material/test_material.macross", EngineNS.RName.ERNameType.Game));
        EngineNS.Macross.TtMacrossStackTracer mStack_BeginPlay_1342966456 = new EngineNS.Macross.TtMacrossStackTracer();
        [EngineNS.Rtti.MetaAttribute]
        public override async System.Threading.Tasks.Task<System.Boolean> BeginPlay(EngineNS.GamePlay.TtGameInstance host)
        {
            #if !disable_macross_9f463216_c26b_45b3_8f1a_30939daabeec
            using(var guard_BeginPlay = new EngineNS.Macross.TtMacrossStackGuard(mStack_BeginPlay_1342966456,mFrame_BeginPlay_1342966456))
            {
                System.Boolean ret_3983877425 = default(System.Boolean);
                mFrame_BeginPlay_1342966456.SetWatchVariable("host", host);
                EngineNS.GamePlay.Scene.TtScene tmp_r_InitViewportSlateWithScene_2890131646 = default(EngineNS.GamePlay.Scene.TtScene);
                EngineNS.GamePlay.Scene.TtMeshNode tmp_r_FindFirstChild_2034756304 = default(EngineNS.GamePlay.Scene.TtMeshNode);
                mFrame_BeginPlay_1342966456.SetWatchVariable("v_mapName_InitViewportSlateWithScene_2890131646", EngineNS.RName.GetRName("tutorials/material/test01.scene", EngineNS.RName.ERNameType.Game));
                mFrame_BeginPlay_1342966456.SetWatchVariable("v_zMin_InitViewportSlateWithScene_2890131646", 0f);
                mFrame_BeginPlay_1342966456.SetWatchVariable("v_zMax_InitViewportSlateWithScene_2890131646", 1f);
                mFrame_BeginPlay_1342966456.SetWatchVariable("v_bSetToWorld_InitViewportSlateWithScene_2890131646", true);
                breaker_InitViewportSlateWithScene_2890131646.TryBreak(mStack_BeginPlay_1342966456, this);
                tmp_r_InitViewportSlateWithScene_2890131646 = (EngineNS.GamePlay.Scene.TtScene)await host.InitViewportSlateWithScene(EngineNS.RName.GetRName("tutorials/material/test01.scene", EngineNS.RName.ERNameType.Game),0f,1f,true);
                mFrame_BeginPlay_1342966456.SetWatchVariable("tmp_r_InitViewportSlateWithScene_2890131646", tmp_r_InitViewportSlateWithScene_2890131646);
                mFrame_BeginPlay_1342966456.SetWatchVariable("v_name_FindFirstChild_2034756304", "TestMtl01");
                mFrame_BeginPlay_1342966456.SetWatchVariable("v_type_FindFirstChild_2034756304", typeof(EngineNS.GamePlay.Scene.TtMeshNode));
                mFrame_BeginPlay_1342966456.SetWatchVariable("v_bRecursive_FindFirstChild_2034756304", false);
                breaker_FindFirstChild_2034756304.TryBreak(mStack_BeginPlay_1342966456, this);
                tmp_r_FindFirstChild_2034756304 = (EngineNS.GamePlay.Scene.TtMeshNode)tmp_r_InitViewportSlateWithScene_2890131646.FindFirstChild("TestMtl01",typeof(EngineNS.GamePlay.Scene.TtMeshNode),false);
                mFrame_BeginPlay_1342966456.SetWatchVariable("tmp_r_FindFirstChild_2034756304", tmp_r_FindFirstChild_2034756304);
                Member_0 = ((EngineNS.GamePlay.Scene.TtMeshNode)tmp_r_FindFirstChild_2034756304);
                ret_3983877425 = true;
                mFrame_BeginPlay_1342966456.SetWatchVariable("ret_3983877425_1924148349", ret_3983877425);
                breaker_return_1924148349.TryBreak(mStack_BeginPlay_1342966456, this);
                return ret_3983877425;
            }
            #elif !(!disable_macross_9f463216_c26b_45b3_8f1a_30939daabeec)
            System.Boolean ret_3983877425 = default(System.Boolean);
            return ret_3983877425;
            #endif //!disable_macross_9f463216_c26b_45b3_8f1a_30939daabeec
        }
        EngineNS.Macross.TtMacrossStackFrame mFrame_Tick_63600741 = new EngineNS.Macross.TtMacrossStackFrame(EngineNS.RName.GetRName("tutorials/material/test_material.macross", EngineNS.RName.ERNameType.Game));
        EngineNS.Macross.TtMacrossStackTracer mStack_Tick_63600741 = new EngineNS.Macross.TtMacrossStackTracer();
        [EngineNS.Rtti.MetaAttribute]
        public override void Tick(EngineNS.GamePlay.TtGameInstance host,System.Single elapsedMillisecond)
        {
            #if !disable_macross_9f463216_c26b_45b3_8f1a_30939daabeec
            using(var guard_Tick = new EngineNS.Macross.TtMacrossStackGuard(mStack_Tick_63600741,mFrame_Tick_63600741))
            {
                mFrame_Tick_63600741.SetWatchVariable("host", host);
                mFrame_Tick_63600741.SetWatchVariable("elapsedMillisecond", elapsedMillisecond);
                mFrame_Tick_63600741.SetWatchVariable("Condition0_690337321", (Member_0 != null));
                breaker_if_690337321.TryBreak(mStack_Tick_63600741, this);
                if ((Member_0 != null))
                {
                }
                else
                {
                }
            }
            #endif //!disable_macross_9f463216_c26b_45b3_8f1a_30939daabeec
        }
    }
}
