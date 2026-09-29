namespace NS_utest.pgc
{
    [EngineNS.Macross.TtMacross]
    [EngineNS.Macross.TtMacrossSign(RName_Name = "utest/pgc/terraingen.macross", RName_Type = EngineNS.RName.ERNameType.Game)]
    public partial class terraingen : EngineNS.Bricks.Procedure.UPgcGraphProgram
    {
        public static EngineNS.Macross.TtMacrossBreak breaker_FindPgcNodeByName_3185294158 = new EngineNS.Macross.TtMacrossBreak("breaker_FindPgcNodeByName_3185294158");
        public static EngineNS.Macross.TtMacrossBreak breaker_if_4184480612 = new EngineNS.Macross.TtMacrossBreak("breaker_if_4184480612");
        EngineNS.Macross.TtMacrossStackFrame mFrame_OnNodeInitialized_2263807525 = new EngineNS.Macross.TtMacrossStackFrame(EngineNS.RName.GetRName("utest/pgc/terraingen.macross", EngineNS.RName.ERNameType.Game));
        EngineNS.Macross.TtMacrossStackTracer mStack_OnNodeInitialized_2263807525 = new EngineNS.Macross.TtMacrossStackTracer();
        [EngineNS.Rtti.MetaAttribute]
        public override System.Boolean OnNodeInitialized(EngineNS.Bricks.Procedure.TtPgcGraph graph,EngineNS.Bricks.Procedure.TtPgcNodeBase node)
        {
            #if !disable_macross_318c3ff6_c3c8_4e00_bb54_c3c2abaa73a2
            using(var guard_OnNodeInitialized = new EngineNS.Macross.TtMacrossStackGuard(mStack_OnNodeInitialized_2263807525,mFrame_OnNodeInitialized_2263807525))
            {
                System.Boolean ret_542370630 = default(System.Boolean);
                mFrame_OnNodeInitialized_2263807525.SetWatchVariable("graph", graph);
                mFrame_OnNodeInitialized_2263807525.SetWatchVariable("node", node);
                EngineNS.Bricks.Procedure.Node.TtCopyRect tmp_r_FindPgcNodeByName_3185294158 = default(EngineNS.Bricks.Procedure.Node.TtCopyRect);
                mFrame_OnNodeInitialized_2263807525.SetWatchVariable("v_name_FindPgcNodeByName_3185294158", "");
                mFrame_OnNodeInitialized_2263807525.SetWatchVariable("v_type_FindPgcNodeByName_3185294158", typeof(EngineNS.Bricks.Procedure.Node.TtCopyRect));
                breaker_FindPgcNodeByName_3185294158.TryBreak(mStack_OnNodeInitialized_2263807525, this);
                tmp_r_FindPgcNodeByName_3185294158 = (EngineNS.Bricks.Procedure.Node.TtCopyRect)graph.FindPgcNodeByName("",typeof(EngineNS.Bricks.Procedure.Node.TtCopyRect));
                mFrame_OnNodeInitialized_2263807525.SetWatchVariable("tmp_r_FindPgcNodeByName_3185294158", tmp_r_FindPgcNodeByName_3185294158);
                mFrame_OnNodeInitialized_2263807525.SetWatchVariable("Condition0_4184480612", (Name == "CopyRect"));
                breaker_if_4184480612.TryBreak(mStack_OnNodeInitialized_2263807525, this);
                if ((Name == "CopyRect"))
                {
                }
                else
                {
                }
                tmp_r_FindPgcNodeByName_3185294158.X = 0;
                tmp_r_FindPgcNodeByName_3185294158.Y = 0;
                return ret_542370630;
            }
            #elif !(!disable_macross_318c3ff6_c3c8_4e00_bb54_c3c2abaa73a2)
            System.Boolean ret_542370630 = default(System.Boolean);
            return ret_542370630;
            #endif //!disable_macross_318c3ff6_c3c8_4e00_bb54_c3c2abaa73a2
        }
    }
}
