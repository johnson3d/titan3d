namespace NS_utest.pgc
{
    [EngineNS.Macross.TtMacross]
    [EngineNS.Macross.TtMacrossSign(RName_Name = "utest/pgc/terraingen.macross", RName_Type = EngineNS.RName.ERNameType.Game)]
    public partial class terraingen : EngineNS.Bricks.Procedure.UPgcGraphProgram
    {
        public static EngineNS.Macross.TtMacrossBreak breaker_FindPgcNodeByName_1051611981 = new EngineNS.Macross.TtMacrossBreak("breaker_FindPgcNodeByName_1051611981");
        public static EngineNS.Macross.TtMacrossBreak breaker_if_2408442523 = new EngineNS.Macross.TtMacrossBreak("breaker_if_2408442523");
        EngineNS.Macross.TtMacrossStackFrame mFrame_OnNodeInitialized_1538527227 = new EngineNS.Macross.TtMacrossStackFrame(EngineNS.RName.GetRName("utest/pgc/terraingen.macross", EngineNS.RName.ERNameType.Game));
        EngineNS.Macross.TtMacrossStackTracer mStack_OnNodeInitialized_1538527227 = new EngineNS.Macross.TtMacrossStackTracer();
        public override System.Boolean OnNodeInitialized(EngineNS.Bricks.Procedure.UPgcGraph graph,EngineNS.Bricks.Procedure.UPgcNodeBase node)
        {
            #if !disable_macross_318c3ff6_c3c8_4e00_bb54_c3c2abaa73a2
            using(var guard_OnNodeInitialized = new EngineNS.Macross.TtMacrossStackGuard(mStack_OnNodeInitialized_1538527227,mFrame_OnNodeInitialized_1538527227))
            {
                System.Boolean ret_2298037646 = default(System.Boolean);
                mFrame_OnNodeInitialized_1538527227.SetWatchVariable("graph", graph);
                mFrame_OnNodeInitialized_1538527227.SetWatchVariable("node", node);
                EngineNS.Bricks.Procedure.Node.UNoisePerlin tmp_r_FindPgcNodeByName_1051611981 = default(EngineNS.Bricks.Procedure.Node.UNoisePerlin);
                mFrame_OnNodeInitialized_1538527227.SetWatchVariable("v_name_FindPgcNodeByName_1051611981", "NoisePerlin1");
                mFrame_OnNodeInitialized_1538527227.SetWatchVariable("v_type_FindPgcNodeByName_1051611981", typeof(EngineNS.Bricks.Procedure.Node.UNoisePerlin));
                breaker_FindPgcNodeByName_1051611981.TryBreak(mStack_OnNodeInitialized_1538527227, this);
                tmp_r_FindPgcNodeByName_1051611981 = (EngineNS.Bricks.Procedure.Node.UNoisePerlin)graph.FindPgcNodeByName("NoisePerlin1",typeof(EngineNS.Bricks.Procedure.Node.UNoisePerlin));
                mFrame_OnNodeInitialized_1538527227.SetWatchVariable("tmp_r_FindPgcNodeByName_1051611981", tmp_r_FindPgcNodeByName_1051611981);
                mFrame_OnNodeInitialized_1538527227.SetWatchVariable("Condition0_2408442523", (node.Name == "CopyRect"));
                breaker_if_2408442523.TryBreak(mStack_OnNodeInitialized_1538527227, this);
                if ((node.Name == "CopyRect"))
                {
                    ((EngineNS.Bricks.Procedure.Node.UCopyRect)node).X = (System.Int32)(tmp_r_FindPgcNodeByName_1051611981.Border);
                    ((EngineNS.Bricks.Procedure.Node.UCopyRect)node).Y = (System.Int32)(tmp_r_FindPgcNodeByName_1051611981.Border);
                }
                else
                {
                }
                return ret_2298037646;
            }
            #elif !(!disable_macross_318c3ff6_c3c8_4e00_bb54_c3c2abaa73a2)
            System.Boolean ret_2298037646 = default(System.Boolean);
            return ret_2298037646;
            #endif //!disable_macross_318c3ff6_c3c8_4e00_bb54_c3c2abaa73a2
        }
    }
}
