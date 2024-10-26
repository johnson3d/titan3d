namespace NS_tutorials.material
{
    [EngineNS.Macross.TtMacross]
    public partial class test_material : EngineNS.GamePlay.TtMacrossGame
    {
        [EngineNS.Rtti.Meta]
        internal EngineNS.GamePlay.Scene.TtMeshNode Member_0 { get; set; } = null;
        public EngineNS.Macross.TtMacrossBreak breaker_InitViewportSlateWithScene_1796944968 = new EngineNS.Macross.TtMacrossBreak("breaker_InitViewportSlateWithScene_1796944968");
        public EngineNS.Macross.TtMacrossBreak breaker_FindFirstChild_2034756304 = new EngineNS.Macross.TtMacrossBreak("breaker_FindFirstChild_2034756304");
        public EngineNS.Macross.TtMacrossBreak breaker_return_1924148349 = new EngineNS.Macross.TtMacrossBreak("breaker_return_1924148349");
        public EngineNS.Macross.TtMacrossBreak breaker_GetMaterial_1597294926 = new EngineNS.Macross.TtMacrossBreak("breaker_GetMaterial_1597294926");
        public EngineNS.Macross.TtMacrossBreak breaker_Sin_1732946206 = new EngineNS.Macross.TtMacrossBreak("breaker_Sin_1732946206");
        public EngineNS.Macross.TtMacrossBreak breaker_CreateColor3f_919645327 = new EngineNS.Macross.TtMacrossBreak("breaker_CreateColor3f_919645327");
        public EngineNS.Macross.TtMacrossBreak breaker_SetColor3_516167510 = new EngineNS.Macross.TtMacrossBreak("breaker_SetColor3_516167510");
        public EngineNS.Macross.TtMacrossBreak breaker_if_690337321 = new EngineNS.Macross.TtMacrossBreak("breaker_if_690337321");
        EngineNS.Macross.TtMacrossStackFrame mFrame_BeginPlay_1342966456 = new EngineNS.Macross.TtMacrossStackFrame(EngineNS.RName.GetRName("tutorials/material/test_material.macross", EngineNS.RName.ERNameType.Game));
        [EngineNS.Rtti.MetaAttribute]
        public override async System.Threading.Tasks.Task<System.Boolean> BeginPlay(EngineNS.GamePlay.TtGameInstance host)
        {
            using(var guard_BeginPlay = new EngineNS.Macross.TtMacrossStackGuard(mFrame_BeginPlay_1342966456))
            {
                System.Boolean ret_3983877425 = default(System.Boolean);
                mFrame_BeginPlay_1342966456.SetWatchVariable("host", host);
                EngineNS.GamePlay.Scene.TtScene tmp_r_InitViewportSlateWithScene_1796944968 = default(EngineNS.GamePlay.Scene.TtScene);
                EngineNS.GamePlay.Scene.TtMeshNode tmp_r_FindFirstChild_2034756304 = default(EngineNS.GamePlay.Scene.TtMeshNode);
                mFrame_BeginPlay_1342966456.SetWatchVariable("v_mapName_InitViewportSlateWithScene_1796944968", EngineNS.RName.GetRName("tutorials/material/test01.scene", EngineNS.RName.ERNameType.Game));
                mFrame_BeginPlay_1342966456.SetWatchVariable("v_zMin_InitViewportSlateWithScene_1796944968", 0f);
                mFrame_BeginPlay_1342966456.SetWatchVariable("v_zMax_InitViewportSlateWithScene_1796944968", 1f);
                mFrame_BeginPlay_1342966456.SetWatchVariable("v_bSetToWorld_InitViewportSlateWithScene_1796944968", true);
                breaker_InitViewportSlateWithScene_1796944968.TryBreak();
                tmp_r_InitViewportSlateWithScene_1796944968 = (EngineNS.GamePlay.Scene.TtScene)await host.InitViewportSlateWithScene(EngineNS.RName.GetRName("tutorials/material/test01.scene", EngineNS.RName.ERNameType.Game),0f,1f,true);
                mFrame_BeginPlay_1342966456.SetWatchVariable("tmp_r_InitViewportSlateWithScene_1796944968", tmp_r_InitViewportSlateWithScene_1796944968);
                mFrame_BeginPlay_1342966456.SetWatchVariable("v_name_FindFirstChild_2034756304", "TestMtl01");
                mFrame_BeginPlay_1342966456.SetWatchVariable("v_type_FindFirstChild_2034756304", typeof(EngineNS.GamePlay.Scene.TtMeshNode));
                mFrame_BeginPlay_1342966456.SetWatchVariable("v_bRecursive_FindFirstChild_2034756304", false);
                breaker_FindFirstChild_2034756304.TryBreak();
                tmp_r_FindFirstChild_2034756304 = (EngineNS.GamePlay.Scene.TtMeshNode)tmp_r_InitViewportSlateWithScene_1796944968.FindFirstChild("TestMtl01",typeof(EngineNS.GamePlay.Scene.TtMeshNode),false);
                mFrame_BeginPlay_1342966456.SetWatchVariable("tmp_r_FindFirstChild_2034756304", tmp_r_FindFirstChild_2034756304);
                Member_0 = ((EngineNS.GamePlay.Scene.TtMeshNode)tmp_r_FindFirstChild_2034756304);
                ret_3983877425 = true;
                mFrame_BeginPlay_1342966456.SetWatchVariable("ret_3983877425_1924148349", ret_3983877425);
                breaker_return_1924148349.TryBreak();
                return ret_3983877425;
                return ret_3983877425;
            }
        }
        EngineNS.Macross.TtMacrossStackFrame mFrame_Tick_63600741 = new EngineNS.Macross.TtMacrossStackFrame(EngineNS.RName.GetRName("tutorials/material/test_material.macross", EngineNS.RName.ERNameType.Game));
        [EngineNS.Rtti.MetaAttribute]
        public override void Tick(EngineNS.GamePlay.TtGameInstance host,System.Single elapsedMillisecond)
        {
            using(var guard_Tick = new EngineNS.Macross.TtMacrossStackGuard(mFrame_Tick_63600741))
            {
                mFrame_Tick_63600741.SetWatchVariable("host", host);
                mFrame_Tick_63600741.SetWatchVariable("elapsedMillisecond", elapsedMillisecond);
                EngineNS.Graphics.Pipeline.Shader.TtMaterial tmp_r_GetMaterial_1597294926 = default(EngineNS.Graphics.Pipeline.Shader.TtMaterial);
                System.Single tmp_r_Sin_1732946206 = default(System.Single);
                EngineNS.Color3f tmp_r_CreateColor3f_919645327 = default(EngineNS.Color3f);
                System.Boolean tmp_r_SetColor3_516167510 = default(System.Boolean);
                mFrame_Tick_63600741.SetWatchVariable("Condition0_690337321", (Member_0 != null));
                breaker_if_690337321.TryBreak();
                if ((Member_0 != null))
                {
                    mFrame_Tick_63600741.SetWatchVariable("v_subMesh_GetMaterial_1597294926", 0);
                    mFrame_Tick_63600741.SetWatchVariable("v_atom_GetMaterial_1597294926", 0);
                    breaker_GetMaterial_1597294926.TryBreak();
                    tmp_r_GetMaterial_1597294926 = Member_0.Mesh.GetMaterial(0,0);
                    mFrame_Tick_63600741.SetWatchVariable("tmp_r_GetMaterial_1597294926", tmp_r_GetMaterial_1597294926);
                    mFrame_Tick_63600741.SetWatchVariable("v_v_Sin_1732946206", EngineNS.TtEngine.Instance.TickCountSecond);
                    breaker_Sin_1732946206.TryBreak();
                    tmp_r_Sin_1732946206 = EngineNS.MathHelper.Sin(EngineNS.TtEngine.Instance.TickCountSecond);
                    mFrame_Tick_63600741.SetWatchVariable("tmp_r_Sin_1732946206", tmp_r_Sin_1732946206);
                    mFrame_Tick_63600741.SetWatchVariable("v_r_CreateColor3f_919645327", 1f);
                    mFrame_Tick_63600741.SetWatchVariable("v_g_CreateColor3f_919645327", tmp_r_Sin_1732946206);
                    mFrame_Tick_63600741.SetWatchVariable("v_b_CreateColor3f_919645327", 0f);
                    breaker_CreateColor3f_919645327.TryBreak();
                    tmp_r_CreateColor3f_919645327 = EngineNS.MathHelper.CreateColor3f(1f,tmp_r_Sin_1732946206,0f);
                    mFrame_Tick_63600741.SetWatchVariable("tmp_r_CreateColor3f_919645327", tmp_r_CreateColor3f_919645327);
                    mFrame_Tick_63600741.SetWatchVariable("v_name_SetColor3_516167510", "Color3_2");
                    mFrame_Tick_63600741.SetWatchVariable("tmp_r_CreateColor3f_919645327", tmp_r_CreateColor3f_919645327);
                    breaker_SetColor3_516167510.TryBreak();
                    tmp_r_SetColor3_516167510 = tmp_r_GetMaterial_1597294926.SetColor3("Color3_2",in tmp_r_CreateColor3f_919645327);
                    mFrame_Tick_63600741.SetWatchVariable("tmp_r_SetColor3_516167510", tmp_r_SetColor3_516167510);
                }
                else
                {
                }
            }
        }
    }
}
