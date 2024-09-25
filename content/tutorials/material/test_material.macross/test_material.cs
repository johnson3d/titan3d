namespace NS_tutorials.material
{
    [EngineNS.Macross.TtMacross]
    public partial class test_material : EngineNS.GamePlay.UMacrossGame
    {
        [EngineNS.Rtti.Meta]
        internal EngineNS.GamePlay.Scene.TtMeshNode Member_0 { get; set; } = null;
        public EngineNS.Macross.TtMacrossBreak breaker_GetMaterial_2979780793 = new EngineNS.Macross.TtMacrossBreak("breaker_GetMaterial_2979780793");
        public EngineNS.Macross.TtMacrossBreak breaker_Sin_2456722531 = new EngineNS.Macross.TtMacrossBreak("breaker_Sin_2456722531");
        public EngineNS.Macross.TtMacrossBreak breaker_CreateColor3f_494365951 = new EngineNS.Macross.TtMacrossBreak("breaker_CreateColor3f_494365951");
        public EngineNS.Macross.TtMacrossBreak breaker_SetColor3_927806693 = new EngineNS.Macross.TtMacrossBreak("breaker_SetColor3_927806693");
        public EngineNS.Macross.TtMacrossBreak breaker_if_1289412133 = new EngineNS.Macross.TtMacrossBreak("breaker_if_1289412133");
        EngineNS.Macross.TtMacrossStackFrame mFrame_Tick_1665648463 = new EngineNS.Macross.TtMacrossStackFrame(EngineNS.RName.GetRName("tutorials/material/test_material.macross", EngineNS.RName.ERNameType.Game));
        [EngineNS.Rtti.MetaAttribute]
        public override void Tick(EngineNS.GamePlay.UGameInstance host,System.Single elapsedMillisecond)
        {
            using(var guard_Tick = new EngineNS.Macross.TtMacrossStackGuard(mFrame_Tick_1665648463))
            {
                mFrame_Tick_1665648463.SetWatchVariable("host", host);
                mFrame_Tick_1665648463.SetWatchVariable("elapsedMillisecond", elapsedMillisecond);
                EngineNS.Graphics.Pipeline.Shader.TtMaterial tmp_r_GetMaterial_2979780793 = default(EngineNS.Graphics.Pipeline.Shader.TtMaterial);
                System.Single tmp_r_Sin_2456722531 = default(System.Single);
                EngineNS.Color3f tmp_r_CreateColor3f_494365951 = default(EngineNS.Color3f);
                mFrame_Tick_1665648463.SetWatchVariable("Condition0_1289412133", (Member_0 != null));
                breaker_if_1289412133.TryBreak();
                if ((Member_0 != null))
                {
                    mFrame_Tick_1665648463.SetWatchVariable("v_subMesh_GetMaterial_2979780793", 0);
                    mFrame_Tick_1665648463.SetWatchVariable("v_atom_GetMaterial_2979780793", 0);
                    breaker_GetMaterial_2979780793.TryBreak();
                    tmp_r_GetMaterial_2979780793 = Member_0.Mesh.GetMaterial(0,0);
                    mFrame_Tick_1665648463.SetWatchVariable("tmp_r_GetMaterial_2979780793", tmp_r_GetMaterial_2979780793);
                    mFrame_Tick_1665648463.SetWatchVariable("v_v_Sin_2456722531", EngineNS.TtEngine.Instance.TickCountSecond);
                    breaker_Sin_2456722531.TryBreak();
                    tmp_r_Sin_2456722531 = EngineNS.MathHelper.Sin(EngineNS.TtEngine.Instance.TickCountSecond);
                    mFrame_Tick_1665648463.SetWatchVariable("tmp_r_Sin_2456722531", tmp_r_Sin_2456722531);
                    mFrame_Tick_1665648463.SetWatchVariable("v_r_CreateColor3f_494365951", 1f);
                    mFrame_Tick_1665648463.SetWatchVariable("v_g_CreateColor3f_494365951", tmp_r_Sin_2456722531);
                    mFrame_Tick_1665648463.SetWatchVariable("v_b_CreateColor3f_494365951", 0f);
                    breaker_CreateColor3f_494365951.TryBreak();
                    tmp_r_CreateColor3f_494365951 = EngineNS.MathHelper.CreateColor3f(1f,tmp_r_Sin_2456722531,0f);
                    mFrame_Tick_1665648463.SetWatchVariable("tmp_r_CreateColor3f_494365951", tmp_r_CreateColor3f_494365951);
                    mFrame_Tick_1665648463.SetWatchVariable("v_name_SetColor3_927806693", "Color3_2");
                    mFrame_Tick_1665648463.SetWatchVariable("tmp_r_CreateColor3f_494365951", tmp_r_CreateColor3f_494365951);
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
