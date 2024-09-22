namespace NS_utest.pgc
{
    [EngineNS.Macross.TtMacross]
    public partial class terraingen : EngineNS.Bricks.Procedure.UPgcGraphProgram
    {
        public EngineNS.Macross.TtMacrossBreak breaker_FindPgcNodeByName_1051611981 = new EngineNS.Macross.TtMacrossBreak("breaker_FindPgcNodeByName_1051611981");
        public EngineNS.Macross.TtMacrossBreak breaker_if_2408442523 = new EngineNS.Macross.TtMacrossBreak("breaker_if_2408442523");
        EngineNS.Macross.TtMacrossStackFrame mFrame_OnNodeInitialized = new EngineNS.Macross.TtMacrossStackFrame(EngineNS.RName.GetRName("utest/pgc/terraingen.macross", EngineNS.RName.ERNameType.Game));
        public override System.Boolean OnNodeInitialized(EngineNS.Bricks.Procedure.UPgcGraph graph,EngineNS.Bricks.Procedure.UPgcNodeBase node)
        {
            using(var guard_OnNodeInitialized = new EngineNS.Macross.TtMacrossStackGuard(mFrame_OnNodeInitialized))
            {
                System.Boolean ret_2298037646 = default(System.Boolean);
                mFrame_OnNodeInitialized.SetWatchVariable("graph", graph);
                mFrame_OnNodeInitialized.SetWatchVariable("node", node);
                EngineNS.Bricks.Procedure.Node.UNoisePerlin tmp_r_FindPgcNodeByName_1051611981 = default(EngineNS.Bricks.Procedure.Node.UNoisePerlin);
                mFrame_OnNodeInitialized.SetWatchVariable("v_name_FindPgcNodeByName_1051611981", "NoisePerlin1");
                mFrame_OnNodeInitialized.SetWatchVariable("v_type_FindPgcNodeByName_1051611981", typeof(EngineNS.Bricks.Procedure.Node.UNoisePerlin));
                breaker_FindPgcNodeByName_1051611981.TryBreak();
                tmp_r_FindPgcNodeByName_1051611981 = (EngineNS.Bricks.Procedure.Node.UNoisePerlin)graph.FindPgcNodeByName("NoisePerlin1",typeof(EngineNS.Bricks.Procedure.Node.UNoisePerlin));
                mFrame_OnNodeInitialized.SetWatchVariable("tmp_r_FindPgcNodeByName_1051611981", tmp_r_FindPgcNodeByName_1051611981);
                mFrame_OnNodeInitialized.SetWatchVariable("Condition0_2408442523", (node.Name == "CopyRect"));
                breaker_if_2408442523.TryBreak();
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
        }
    }
}
