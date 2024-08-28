namespace NS_tutorials.material
{
    [EngineNS.Macross.UMacross]
    public partial class test_material : EngineNS.GamePlay.UMacrossGame
    {
        [EngineNS.Rtti.Meta]
        internal EngineNS.GamePlay.Scene.TtMeshNode Member_0 { get; set; } = null;
        public EngineNS.Macross.UMacrossBreak breaker_InitViewportSlateWithScene_1114193444 = new EngineNS.Macross.UMacrossBreak("breaker_InitViewportSlateWithScene_1114193444");
        public EngineNS.Macross.UMacrossBreak breaker_FindFirstChild_1270059286 = new EngineNS.Macross.UMacrossBreak("breaker_FindFirstChild_1270059286");
        public EngineNS.Macross.UMacrossBreak breaker_return_3324266569 = new EngineNS.Macross.UMacrossBreak("breaker_return_3324266569");
        public EngineNS.Macross.UMacrossBreak breaker_GetMaterial_2979780793 = new EngineNS.Macross.UMacrossBreak("breaker_GetMaterial_2979780793");
        public EngineNS.Macross.UMacrossBreak breaker_Sin_2456722531 = new EngineNS.Macross.UMacrossBreak("breaker_Sin_2456722531");
        public EngineNS.Macross.UMacrossBreak breaker_CreateColor3f_494365951 = new EngineNS.Macross.UMacrossBreak("breaker_CreateColor3f_494365951");
        public EngineNS.Macross.UMacrossBreak breaker_SetColor3_927806693 = new EngineNS.Macross.UMacrossBreak("breaker_SetColor3_927806693");
        public EngineNS.Macross.UMacrossBreak breaker_if_1289412133 = new EngineNS.Macross.UMacrossBreak("breaker_if_1289412133");
        EngineNS.Macross.UMacrossStackFrame mFrame_BeginPlay = new EngineNS.Macross.UMacrossStackFrame(EngineNS.RName.GetRName("tutorials/material/test_material.macross", EngineNS.RName.ERNameType.Game));
        [EngineNS.Rtti.MetaAttribute]
        public override async System.Threading.Tasks.Task<System.Boolean> BeginPlay(EngineNS.GamePlay.UGameInstance host)
        {
            using(var guard_BeginPlay = new EngineNS.Macross.UMacrossStackGuard(mFrame_BeginPlay))
            {
                System.Boolean ret_358951701 = default(System.Boolean);
                mFrame_BeginPlay.SetWatchVariable("host", host);
                EngineNS.GamePlay.Scene.TtScene tmp_r_InitViewportSlateWithScene_1114193444 = default(EngineNS.GamePlay.Scene.TtScene);
                EngineNS.GamePlay.Scene.TtNode tmp_r_FindFirstChild_1270059286 = default(EngineNS.GamePlay.Scene.TtNode);
                mFrame_BeginPlay.SetWatchVariable("v_mapName_InitViewportSlateWithScene_1114193444", EngineNS.RName.GetRName("tutorials/material/test01.scene", EngineNS.RName.ERNameType.Game));
                mFrame_BeginPlay.SetWatchVariable("v_zMin_InitViewportSlateWithScene_1114193444", 0f);
                mFrame_BeginPlay.SetWatchVariable("v_zMax_InitViewportSlateWithScene_1114193444", 0f);
                mFrame_BeginPlay.SetWatchVariable("v_bSetToWorld_InitViewportSlateWithScene_1114193444", true);
                breaker_InitViewportSlateWithScene_1114193444.TryBreak();
                tmp_r_InitViewportSlateWithScene_1114193444 = (EngineNS.GamePlay.Scene.TtScene)await host.InitViewportSlateWithScene(EngineNS.RName.GetRName("tutorials/material/test01.scene", EngineNS.RName.ERNameType.Game),0f,0f,true);
                mFrame_BeginPlay.SetWatchVariable("tmp_r_InitViewportSlateWithScene_1114193444", tmp_r_InitViewportSlateWithScene_1114193444);
                mFrame_BeginPlay.SetWatchVariable("v_name_FindFirstChild_1270059286", "TestMtl01");
                mFrame_BeginPlay.SetWatchVariable("v_type_FindFirstChild_1270059286", typeof(EngineNS.GamePlay.Scene.TtNode));
                mFrame_BeginPlay.SetWatchVariable("v_bRecursive_FindFirstChild_1270059286", false);
                breaker_FindFirstChild_1270059286.TryBreak();
                tmp_r_FindFirstChild_1270059286 = tmp_r_InitViewportSlateWithScene_1114193444.FindFirstChild("TestMtl01",typeof(EngineNS.GamePlay.Scene.TtNode),false);
                mFrame_BeginPlay.SetWatchVariable("tmp_r_FindFirstChild_1270059286", tmp_r_FindFirstChild_1270059286);
                Member_0 = ((EngineNS.GamePlay.Scene.TtMeshNode)tmp_r_FindFirstChild_1270059286);
                ret_358951701 = true;
                mFrame_BeginPlay.SetWatchVariable("ret_358951701_3324266569", ret_358951701);
                breaker_return_3324266569.TryBreak();
                return ret_358951701;
                return ret_358951701;
            }
        }
        EngineNS.Macross.UMacrossStackFrame mFrame_Tick = new EngineNS.Macross.UMacrossStackFrame(EngineNS.RName.GetRName("tutorials/material/test_material.macross", EngineNS.RName.ERNameType.Game));
        [EngineNS.Rtti.MetaAttribute]
        public override void Tick(EngineNS.GamePlay.UGameInstance host,System.Single elapsedMillisecond)
        {
            using(var guard_Tick = new EngineNS.Macross.UMacrossStackGuard(mFrame_Tick))
            {
                mFrame_Tick.SetWatchVariable("host", host);
                mFrame_Tick.SetWatchVariable("elapsedMillisecond", elapsedMillisecond);
                EngineNS.Graphics.Pipeline.Shader.TtMaterial tmp_r_GetMaterial_2979780793 = default(EngineNS.Graphics.Pipeline.Shader.TtMaterial);
                System.Single tmp_r_Sin_2456722531 = default(System.Single);
                EngineNS.Color3f tmp_r_CreateColor3f_494365951 = default(EngineNS.Color3f);
                mFrame_Tick.SetWatchVariable("Condition0_1289412133", (Member_0 != null));
                breaker_if_1289412133.TryBreak();
                if ((Member_0 != null))
                {
                    mFrame_Tick.SetWatchVariable("v_subMesh_GetMaterial_2979780793", 0);
                    mFrame_Tick.SetWatchVariable("v_atom_GetMaterial_2979780793", 0);
                    breaker_GetMaterial_2979780793.TryBreak();
                    tmp_r_GetMaterial_2979780793 = Member_0.Mesh.GetMaterial(0,0);
                    mFrame_Tick.SetWatchVariable("tmp_r_GetMaterial_2979780793", tmp_r_GetMaterial_2979780793);
                    mFrame_Tick.SetWatchVariable("v_v_Sin_2456722531", EngineNS.TtEngine.Instance.TickCountSecond);
                    breaker_Sin_2456722531.TryBreak();
                    tmp_r_Sin_2456722531 = EngineNS.MathHelper.Sin(EngineNS.TtEngine.Instance.TickCountSecond);
                    mFrame_Tick.SetWatchVariable("tmp_r_Sin_2456722531", tmp_r_Sin_2456722531);
                    mFrame_Tick.SetWatchVariable("v_r_CreateColor3f_494365951", 1f);
                    mFrame_Tick.SetWatchVariable("v_g_CreateColor3f_494365951", tmp_r_Sin_2456722531);
                    mFrame_Tick.SetWatchVariable("v_b_CreateColor3f_494365951", 0f);
                    breaker_CreateColor3f_494365951.TryBreak();
                    tmp_r_CreateColor3f_494365951 = EngineNS.MathHelper.CreateColor3f(1f,tmp_r_Sin_2456722531,0f);
                    mFrame_Tick.SetWatchVariable("tmp_r_CreateColor3f_494365951", tmp_r_CreateColor3f_494365951);
                    mFrame_Tick.SetWatchVariable("v_name_SetColor3_927806693", "Color3_2");
                    mFrame_Tick.SetWatchVariable("tmp_r_CreateColor3f_494365951", tmp_r_CreateColor3f_494365951);
                    breaker_SetColor3_927806693.TryBreak();
                    tmp_r_GetMaterial_2979780793.SetColor3("Color3_2",in tmp_r_CreateColor3f_494365951);
                }
                else
                {
                }
            }
        }
    }
}
