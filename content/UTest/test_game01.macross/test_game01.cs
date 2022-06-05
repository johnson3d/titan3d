namespace NS_utest
{
    [EngineNS.Macross.UMacross]
    public partial class test_game01 : EngineNS.GamePlay.UMacrossGame
    {
        [EngineNS.Rtti.Meta]
        public override void BeginDestroy(EngineNS.GamePlay.UGameInstance host)
        {
             host.FinalViewportSlate();
        }
        [EngineNS.Rtti.Meta]
        public override async System.Threading.Tasks.Task<System.Boolean> BeginPlay(EngineNS.GamePlay.UGameInstance host)
        {
             System.Boolean ret_2822948834 = default(System.Boolean);
            System.Boolean tmp_r_LoadScene_3770860681 = default(System.Boolean);
            await host.InitViewportSlate(EngineNS.RName.GetRName("utest/deferred.rpolicy", EngineNS.RName.ERNameType.Game),0,0);
            tmp_r_LoadScene_3770860681 = (System.Boolean)await host.LoadScene(EngineNS.RName.GetRName("utest/testscene.scene", EngineNS.RName.ERNameType.Game));
            return ret_2822948834;
        }
    }
}
