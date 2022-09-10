namespace NS_utest.pgc
{
    [EngineNS.Macross.UMacross]
    public partial class terrain_bz_height : EngineNS.Bricks.Procedure.Node.UProgram
    {
        EngineNS.Macross.UMacrossBreak breaker_GetUVW_504025461 = new EngineNS.Macross.UMacrossBreak("breaker_GetUVW_504025461");
        EngineNS.Macross.UMacrossBreak breaker_GetY_4154552476 = new EngineNS.Macross.UMacrossBreak("breaker_GetY_4154552476");
        EngineNS.Macross.UMacrossBreak breaker_SetFloat1_3734322301 = new EngineNS.Macross.UMacrossBreak("breaker_SetFloat1_3734322301");
        EngineNS.Macross.UMacrossBreak breaker_GetInputNodeByName_2129939616 = new EngineNS.Macross.UMacrossBreak("breaker_GetInputNodeByName_2129939616");
        EngineNS.Macross.UMacrossBreak breaker_GetUVW_3747548161 = new EngineNS.Macross.UMacrossBreak("breaker_GetUVW_3747548161");
        EngineNS.Macross.UMacrossBreak breaker_GetY_1647280972 = new EngineNS.Macross.UMacrossBreak("breaker_GetY_1647280972");
        EngineNS.Macross.UMacrossBreak breaker_SetFloat1_1829751520 = new EngineNS.Macross.UMacrossBreak("breaker_SetFloat1_1829751520");
        EngineNS.Macross.UMacrossBreak breaker_GetInputNodeByName_2473213923 = new EngineNS.Macross.UMacrossBreak("breaker_GetInputNodeByName_2473213923");
        EngineNS.Macross.UMacrossBreak breaker_FindBuffer_2587353212 = new EngineNS.Macross.UMacrossBreak("breaker_FindBuffer_2587353212");
        EngineNS.Macross.UMacrossBreak breaker_DispatchPixels_545367290 = new EngineNS.Macross.UMacrossBreak("breaker_DispatchPixels_545367290");
        EngineNS.Macross.UMacrossStackFrame mFrame_dm_onPerPiexel_DispatchPixels_545367290 = new EngineNS.Macross.UMacrossStackFrame(EngineNS.RName.GetRName("utest/pgc/terrain_bz_height.macross", EngineNS.RName.ERNameType.Game));
        [EngineNS.Rtti.Meta]
        public void dm_onPerPiexel_DispatchPixels_545367290(EngineNS.Bricks.Procedure.UBufferConponent result,System.Int32 x,System.Int32 y,System.Int32 z,EngineNS.Bricks.Procedure.Node.UBezier Value1)
        {
            using(var guard_dm_onPerPiexel_DispatchPixels_545367290 = new EngineNS.Macross.UMacrossStackGuard(mFrame_dm_onPerPiexel_DispatchPixels_545367290))
            {
                mFrame_dm_onPerPiexel_DispatchPixels_545367290.SetWatchVariable("result", result);
                mFrame_dm_onPerPiexel_DispatchPixels_545367290.SetWatchVariable("x", x);
                mFrame_dm_onPerPiexel_DispatchPixels_545367290.SetWatchVariable("y", y);
                mFrame_dm_onPerPiexel_DispatchPixels_545367290.SetWatchVariable("z", z);
                mFrame_dm_onPerPiexel_DispatchPixels_545367290.SetWatchVariable("Value1", Value1);
                EngineNS.Vector3 tmp_r_GetUVW_504025461 = default(EngineNS.Vector3);
                System.Single tmp_r_GetY_4154552476 = default(System.Single);
                mFrame_dm_onPerPiexel_DispatchPixels_545367290.SetWatchVariable("v_x_GetUVW_504025461", x);
                mFrame_dm_onPerPiexel_DispatchPixels_545367290.SetWatchVariable("v_y_GetUVW_504025461", 0);
                mFrame_dm_onPerPiexel_DispatchPixels_545367290.SetWatchVariable("v_z_GetUVW_504025461", 0);
                breaker_GetUVW_504025461.TryBreak();
                tmp_r_GetUVW_504025461 = result.GetUVW(x,0,0);
                mFrame_dm_onPerPiexel_DispatchPixels_545367290.SetWatchVariable("tmp_r_GetUVW_504025461", tmp_r_GetUVW_504025461);
                mFrame_dm_onPerPiexel_DispatchPixels_545367290.SetWatchVariable("v_x_GetY_4154552476", tmp_r_GetUVW_504025461.X);
                breaker_GetY_4154552476.TryBreak();
                tmp_r_GetY_4154552476 = Value1.GetY(tmp_r_GetUVW_504025461.X);
                mFrame_dm_onPerPiexel_DispatchPixels_545367290.SetWatchVariable("tmp_r_GetY_4154552476", tmp_r_GetY_4154552476);
                mFrame_dm_onPerPiexel_DispatchPixels_545367290.SetWatchVariable("v_x_SetFloat1_3734322301", x);
                mFrame_dm_onPerPiexel_DispatchPixels_545367290.SetWatchVariable("v_y_SetFloat1_3734322301", y);
                mFrame_dm_onPerPiexel_DispatchPixels_545367290.SetWatchVariable("v_z_SetFloat1_3734322301", z);
                mFrame_dm_onPerPiexel_DispatchPixels_545367290.SetWatchVariable("v_v_SetFloat1_3734322301", tmp_r_GetY_4154552476);
                breaker_SetFloat1_3734322301.TryBreak();
                result.SetFloat1(x,y,z,tmp_r_GetY_4154552476);
            }
        }
        EngineNS.Macross.UMacrossStackFrame mFrame_OnPerPixel = new EngineNS.Macross.UMacrossStackFrame(EngineNS.RName.GetRName("utest/pgc/terrain_bz_height.macross", EngineNS.RName.ERNameType.Game));
        [EngineNS.Rtti.Meta]
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
                EngineNS.Bricks.Procedure.Node.UBezier tmp_r_GetInputNodeByName_2129939616 = default(EngineNS.Bricks.Procedure.Node.UBezier);
                EngineNS.Vector3 tmp_r_GetUVW_3747548161 = default(EngineNS.Vector3);
                System.Single tmp_r_GetY_1647280972 = default(System.Single);
                mFrame_OnPerPixel.SetWatchVariable("v_graph_GetInputNodeByName_2129939616", graph);
                mFrame_OnPerPixel.SetWatchVariable("v_pinName_GetInputNodeByName_2129939616", "Curve");
                mFrame_OnPerPixel.SetWatchVariable("v_retType_GetInputNodeByName_2129939616", typeof(EngineNS.Bricks.Procedure.Node.UBezier));
                breaker_GetInputNodeByName_2129939616.TryBreak();
                tmp_r_GetInputNodeByName_2129939616 = (EngineNS.Bricks.Procedure.Node.UBezier)node.GetInputNodeByName(graph,"Curve",typeof(EngineNS.Bricks.Procedure.Node.UBezier));
                mFrame_OnPerPixel.SetWatchVariable("tmp_r_GetInputNodeByName_2129939616", tmp_r_GetInputNodeByName_2129939616);
                mFrame_OnPerPixel.SetWatchVariable("v_x_GetUVW_3747548161", x);
                mFrame_OnPerPixel.SetWatchVariable("v_y_GetUVW_3747548161", 0);
                mFrame_OnPerPixel.SetWatchVariable("v_z_GetUVW_3747548161", 0);
                breaker_GetUVW_3747548161.TryBreak();
                tmp_r_GetUVW_3747548161 = resuilt.GetUVW(x,0,0);
                mFrame_OnPerPixel.SetWatchVariable("tmp_r_GetUVW_3747548161", tmp_r_GetUVW_3747548161);
                mFrame_OnPerPixel.SetWatchVariable("v_x_GetY_1647280972", tmp_r_GetUVW_3747548161.X);
                breaker_GetY_1647280972.TryBreak();
                tmp_r_GetY_1647280972 = tmp_r_GetInputNodeByName_2129939616.GetY(tmp_r_GetUVW_3747548161.X);
                mFrame_OnPerPixel.SetWatchVariable("tmp_r_GetY_1647280972", tmp_r_GetY_1647280972);
                mFrame_OnPerPixel.SetWatchVariable("v_x_SetFloat1_1829751520", x);
                mFrame_OnPerPixel.SetWatchVariable("v_y_SetFloat1_1829751520", y);
                mFrame_OnPerPixel.SetWatchVariable("v_z_SetFloat1_1829751520", z);
                mFrame_OnPerPixel.SetWatchVariable("v_v_SetFloat1_1829751520", tmp_r_GetY_1647280972);
                breaker_SetFloat1_1829751520.TryBreak();
                resuilt.SetFloat1(x,y,z,tmp_r_GetY_1647280972);
            }
        }
        EngineNS.Macross.UMacrossStackFrame mFrame_OnProcedure = new EngineNS.Macross.UMacrossStackFrame(EngineNS.RName.GetRName("utest/pgc/terrain_bz_height.macross", EngineNS.RName.ERNameType.Game));
        [EngineNS.Rtti.Meta]
        public override System.Boolean OnProcedure(EngineNS.Bricks.Procedure.UPgcGraph graph,EngineNS.Bricks.Procedure.Node.UProgramNode node)
        {
            using(var guard_OnProcedure = new EngineNS.Macross.UMacrossStackGuard(mFrame_OnProcedure))
            {
                System.Boolean ret_989296408 = default(System.Boolean);
                mFrame_OnProcedure.SetWatchVariable("graph", graph);
                mFrame_OnProcedure.SetWatchVariable("node", node);
                EngineNS.Bricks.Procedure.Node.UBezier tmp_r_GetInputNodeByName_2473213923 = default(EngineNS.Bricks.Procedure.Node.UBezier);
                EngineNS.Bricks.Procedure.UBufferConponent tmp_r_FindBuffer_2587353212 = default(EngineNS.Bricks.Procedure.UBufferConponent);
                mFrame_OnProcedure.SetWatchVariable("v_graph_GetInputNodeByName_2473213923", graph);
                mFrame_OnProcedure.SetWatchVariable("v_pinName_GetInputNodeByName_2473213923", "Curve");
                mFrame_OnProcedure.SetWatchVariable("v_retType_GetInputNodeByName_2473213923", typeof(EngineNS.Bricks.Procedure.Node.UBezier));
                breaker_GetInputNodeByName_2473213923.TryBreak();
                tmp_r_GetInputNodeByName_2473213923 = (EngineNS.Bricks.Procedure.Node.UBezier)node.GetInputNodeByName(graph,"Curve",typeof(EngineNS.Bricks.Procedure.Node.UBezier));
                mFrame_OnProcedure.SetWatchVariable("tmp_r_GetInputNodeByName_2473213923", tmp_r_GetInputNodeByName_2473213923);
                mFrame_OnProcedure.SetWatchVariable("v_name_FindBuffer_2587353212", "Height");
                breaker_FindBuffer_2587353212.TryBreak();
                tmp_r_FindBuffer_2587353212 = node.FindBuffer("Height");
                mFrame_OnProcedure.SetWatchVariable("tmp_r_FindBuffer_2587353212", tmp_r_FindBuffer_2587353212);
                mFrame_OnProcedure.SetWatchVariable("v_bMultThread_DispatchPixels_545367290", false);
                breaker_DispatchPixels_545367290.TryBreak();
                tmp_r_FindBuffer_2587353212.DispatchPixels(((___result,___x,___y,___z)=> { dm_onPerPiexel_DispatchPixels_545367290(___result,___x,___y,___z,tmp_r_GetInputNodeByName_2473213923); }),false);
                return ret_989296408;
            }
        }
    }
}
