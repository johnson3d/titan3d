﻿namespace NS_utest.pgc
{
    [EngineNS.Macross.TtMacross]
    public partial class testprog : EngineNS.Bricks.Procedure.Node.UProgram
    {
        [EngineNS.Rtti.Meta]
        private System.Int32 Member_0 { get; set; }
        public EngineNS.Macross.TtMacrossBreak breaker_return_741093373 = new EngineNS.Macross.TtMacrossBreak("breaker_return_741093373");
        public EngineNS.Macross.TtMacrossBreak breaker_FindBuffer_227884761 = new EngineNS.Macross.TtMacrossBreak("breaker_FindBuffer_227884761");
        public EngineNS.Macross.TtMacrossBreak breaker_DispatchPixels_2032038249 = new EngineNS.Macross.TtMacrossBreak("breaker_DispatchPixels_2032038249");
        public EngineNS.Macross.TtMacrossBreak breaker_return_484331367 = new EngineNS.Macross.TtMacrossBreak("breaker_return_484331367");
        EngineNS.Macross.TtMacrossStackFrame mFrame_OnPerPixel = new EngineNS.Macross.TtMacrossStackFrame(EngineNS.RName.GetRName("utest/pgc/testprog.macross", EngineNS.RName.ERNameType.Game));
        public override void OnPerPixel(EngineNS.Bricks.Procedure.UPgcGraph graph,EngineNS.Bricks.Procedure.Node.UProgramNode node,EngineNS.Bricks.Procedure.UBufferComponent resuilt,System.Int32 x,System.Int32 y,System.Int32 z,System.Object tag)
        {
            using(var guard_OnPerPixel = new EngineNS.Macross.TtMacrossStackGuard(mFrame_OnPerPixel))
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
        EngineNS.Macross.TtMacrossStackFrame mFrame_OnProcedure = new EngineNS.Macross.TtMacrossStackFrame(EngineNS.RName.GetRName("utest/pgc/testprog.macross", EngineNS.RName.ERNameType.Game));
        public override System.Boolean OnProcedure(EngineNS.Bricks.Procedure.UPgcGraph graph,EngineNS.Bricks.Procedure.Node.UProgramNode node)
        {
            using(var guard_OnProcedure = new EngineNS.Macross.TtMacrossStackGuard(mFrame_OnProcedure))
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
