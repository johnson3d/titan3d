namespace NS_tutorials.helloword
{
    [EngineNS.Macross.UMacross]
    public partial class helloword : EngineNS.GamePlay.UMacrossGame
    {
        public EngineNS.Macross.UMacrossBreak breaker_InitViewportSlate_3038162489 = new EngineNS.Macross.UMacrossBreak("breaker_InitViewportSlate_3038162489");
        public EngineNS.Macross.UMacrossBreak breaker_LoadScene_3113493897 = new EngineNS.Macross.UMacrossBreak("breaker_LoadScene_3113493897");
        public EngineNS.Macross.UMacrossBreak breaker_Load_2325772087 = new EngineNS.Macross.UMacrossBreak("breaker_Load_2325772087");
        public EngineNS.Macross.UMacrossBreak breaker_Add_1937078671 = new EngineNS.Macross.UMacrossBreak("breaker_Add_1937078671");
        public EngineNS.Macross.UMacrossBreak breaker_return_1529912777 = new EngineNS.Macross.UMacrossBreak("breaker_return_1529912777");
        EngineNS.Macross.UMacrossStackFrame mFrame_BeginPlay = new EngineNS.Macross.UMacrossStackFrame(EngineNS.RName.GetRName("tutorials/helloword/helloword.macross", EngineNS.RName.ERNameType.Game));
        public override async System.Threading.Tasks.Task<System.Boolean> BeginPlay(EngineNS.GamePlay.UGameInstance host)
        {
            using(var guard_BeginPlay = new EngineNS.Macross.UMacrossStackGuard(mFrame_BeginPlay))
            {
                System.Boolean ret_4129711059 = default(System.Boolean);
                mFrame_BeginPlay.SetWatchVariable("host", host);
                EngineNS.GamePlay.Scene.UScene tmp_r_LoadScene_3113493897 = default(EngineNS.GamePlay.Scene.UScene);
                EngineNS.UI.Controls.TtUIElement tmp_r_Load_2325772087 = default(EngineNS.UI.Controls.TtUIElement);
                mFrame_BeginPlay.SetWatchVariable("v_rPolicy_InitViewportSlate_3038162489", EngineNS.RName.GetRName("utest/deferred.rpolicy", EngineNS.RName.ERNameType.Game));
                mFrame_BeginPlay.SetWatchVariable("v_zMin_InitViewportSlate_3038162489", 0f);
                mFrame_BeginPlay.SetWatchVariable("v_zMax_InitViewportSlate_3038162489", 0f);
                breaker_InitViewportSlate_3038162489.TryBreak();
                await host.InitViewportSlate(EngineNS.RName.GetRName("utest/deferred.rpolicy", EngineNS.RName.ERNameType.Game),0f,0f);
                mFrame_BeginPlay.SetWatchVariable("v_mapName_LoadScene_3113493897", EngineNS.RName.GetRName("tutorials/helloword/map01.scene", EngineNS.RName.ERNameType.Game));
                breaker_LoadScene_3113493897.TryBreak();
                tmp_r_LoadScene_3113493897 = (EngineNS.GamePlay.Scene.UScene)await host.LoadScene(EngineNS.RName.GetRName("tutorials/helloword/map01.scene", EngineNS.RName.ERNameType.Game));
                mFrame_BeginPlay.SetWatchVariable("tmp_r_LoadScene_3113493897", tmp_r_LoadScene_3113493897);
                mFrame_BeginPlay.SetWatchVariable("v_name_Load_2325772087", EngineNS.RName.GetRName("tutorials/helloword/hello.ui", EngineNS.RName.ERNameType.Game));
                breaker_Load_2325772087.TryBreak();
                tmp_r_Load_2325772087 = EngineNS.UEngine.Instance.UIManager.Load(EngineNS.RName.GetRName("tutorials/helloword/hello.ui", EngineNS.RName.ERNameType.Game));
                mFrame_BeginPlay.SetWatchVariable("tmp_r_Load_2325772087", tmp_r_Load_2325772087);
                mFrame_BeginPlay.SetWatchVariable("v_item_Add_1937078671", tmp_r_Load_2325772087);
                breaker_Add_1937078671.TryBreak();
                host.WorldViewportSlate.DefaultHUD.Children.Add(tmp_r_Load_2325772087);
                ret_4129711059 = true;
                mFrame_BeginPlay.SetWatchVariable("ret_4129711059_1529912777", ret_4129711059);
                breaker_return_1529912777.TryBreak();
                return ret_4129711059;
            }
        }
    }
}
