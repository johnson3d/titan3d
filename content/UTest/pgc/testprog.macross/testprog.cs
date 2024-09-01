namespace NS_utest.pgc
{
    [EngineNS.Macross.UMacross]
    public partial class testprog : EngineNS.Bricks.Procedure.Node.UProgram
    {
        [EngineNS.Rtti.Meta]
        private System.Int32 Member_0 { get; set; }
        public EngineNS.Macross.UMacrossBreak breaker_return_741093373 = new EngineNS.Macross.UMacrossBreak("breaker_return_741093373");
        public EngineNS.Macross.UMacrossBreak breaker_FindBuffer_227884761 = new EngineNS.Macross.UMacrossBreak("breaker_FindBuffer_227884761");
        public EngineNS.Macross.UMacrossBreak breaker_DispatchPixels_2032038249 = new EngineNS.Macross.UMacrossBreak("breaker_DispatchPixels_2032038249");
        public EngineNS.Macross.UMacrossBreak breaker_return_484331367 = new EngineNS.Macross.UMacrossBreak("breaker_return_484331367");
        EngineNS.Macross.UMacrossStackFrame mFrame_OnPerPixel = new EngineNS.Macross.UMacrossStackFrame(EngineNS.RName.GetRName("utest/pgc/testprog.macross", EngineNS.RName.ERNameType.Game));
        public override void OnPerPixel(EngineNS.Bricks.Procedure.UPgcGraph graph,EngineNS.Bricks.Procedure.Node.UProgramNode node,EngineNS.Bricks.Procedure.UBufferConponent resuilt,System.Int32 x,System.Int32 y,System.Int32 z,System.Object tag)
        {
            using(var guard_OnPerPixel = new EngineNS.Macross.UMacrossStackGuard(mFrame_OnPerPixel))
            {
                mFrame_OnPerPixel.SetWatchVariable("graph", graph);
                mFrame_OnPerPixel.SetWatchVariable("node", node);
                mFrame_OnPerPixel.SetWatchVariable("resuilt", resuilt);
                mFrame_OnPerPixel.SetWatchVariable("x", x);
                mFrame_OnPerPixel.SetWatchVariable("y", y);
                mFrame_OnPerPixel.SetWatchVariable("z", z);
                mFrame_OnPerPixel.SetWatchVariable("tag", tag);
                breaker_return_741093373.TryBreak();
                return;
            }
        }
        EngineNS.Macross.UMacrossStackFrame mFrame_OnProcedure = new EngineNS.Macross.UMacrossStackFrame(EngineNS.RName.GetRName("utest/pgc/testprog.macross", EngineNS.RName.ERNameType.Game));
        public override System.Boolean OnProcedure(EngineNS.Bricks.Procedure.UPgcGraph graph,EngineNS.Bricks.Procedure.Node.UProgramNode node)
        {
            using(var guard_OnProcedure = new EngineNS.Macross.UMacrossStackGuard(mFrame_OnProcedure))
            {
                System.Boolean ret_3421051548 = default(System.Boolean);
                mFrame_OnProcedure.SetWatchVariable("graph", graph);
                mFrame_OnProcedure.SetWatchVariable("node", node);
                EngineNS.Bricks.Procedure.UBufferComponent tmp_r_FindBuffer_227884761 = default(EngineNS.Bricks.Procedure.UBufferComponent);
                mFrame_OnProcedure.SetWatchVariable("v_name_FindBuffer_227884761", "");
                breaker_FindBuffer_227884761.TryBreak();
                tmp_r_FindBuffer_227884761 = node.FindBuffer("");
                mFrame_OnProcedure.SetWatchVariable("tmp_r_FindBuffer_227884761", tmp_r_FindBuffer_227884761);
                mFrame_OnProcedure.SetWatchVariable("v_bMultThread_DispatchPixels_2032038249", false);
                breaker_DispatchPixels_2032038249.TryBreak();
                tmp_r_FindBuffer_227884761.DispatchPixels(((___result,___x,___y,___z)=> {  }),false);
                ret_3421051548 = true;
                mFrame_OnProcedure.SetWatchVariable("ret_3421051548_484331367", ret_3421051548);
                breaker_return_484331367.TryBreak();
                return ret_3421051548;
                return ret_3421051548;
            }
        }
    }
}
