namespace NS_utest.pgc
{
    [EngineNS.Macross.UMacross]
    public partial class terrain_bz_height : EngineNS.Bricks.Procedure.Node.UProgram
    {
        [EngineNS.Rtti.Meta]
        public void dm_onPerPiexel_DispatchPixels_545367290(EngineNS.Bricks.Procedure.UBufferConponent result,System.Int32 x,System.Int32 y,System.Int32 z,EngineNS.Bricks.Procedure.Node.UBezier Value1)
        {
             EngineNS.Vector3 tmp_r_GetUVW_504025461 = default(EngineNS.Vector3);
            System.Single tmp_r_GetY_4154552476 = default(System.Single);
            tmp_r_GetUVW_504025461 = result.GetUVW(x,0,0);
            tmp_r_GetY_4154552476 = Value1.GetY(tmp_r_GetUVW_504025461.X);
            result.SetFloat1(x,y,z,tmp_r_GetY_4154552476);
        }
        [EngineNS.Rtti.Meta]
        public override void OnPerPixel(EngineNS.Bricks.Procedure.UPgcGraph graph,EngineNS.Bricks.Procedure.Node.UProgramNode node,EngineNS.Bricks.Procedure.UBufferConponent resuilt,System.Int32 x,System.Int32 y,System.Int32 z,System.Object tag)
        {
             EngineNS.Bricks.Procedure.Node.UBezier tmp_r_GetInputNodeByName_2129939616 = default(EngineNS.Bricks.Procedure.Node.UBezier);
            EngineNS.Vector3 tmp_r_GetUVW_3747548161 = default(EngineNS.Vector3);
            System.Single tmp_r_GetY_1647280972 = default(System.Single);
            tmp_r_GetInputNodeByName_2129939616 = (EngineNS.Bricks.Procedure.Node.UBezier)node.GetInputNodeByName(graph,"Curve",typeof(EngineNS.Bricks.Procedure.Node.UBezier));
            tmp_r_GetUVW_3747548161 = resuilt.GetUVW(x,0,0);
            tmp_r_GetY_1647280972 = tmp_r_GetInputNodeByName_2129939616.GetY(tmp_r_GetUVW_3747548161.X);
            resuilt.SetFloat1(x,y,z,tmp_r_GetY_1647280972);
        }
        [EngineNS.Rtti.Meta]
        public override System.Boolean OnProcedure(EngineNS.Bricks.Procedure.UPgcGraph graph,EngineNS.Bricks.Procedure.Node.UProgramNode node)
        {
             System.Boolean ret_989296408 = default(System.Boolean);
            EngineNS.Bricks.Procedure.Node.UBezier tmp_r_GetInputNodeByName_2473213923 = default(EngineNS.Bricks.Procedure.Node.UBezier);
            EngineNS.Bricks.Procedure.UBufferConponent tmp_r_FindBuffer_2587353212 = default(EngineNS.Bricks.Procedure.UBufferConponent);
            tmp_r_GetInputNodeByName_2473213923 = (EngineNS.Bricks.Procedure.Node.UBezier)node.GetInputNodeByName(graph,"Curve",typeof(EngineNS.Bricks.Procedure.Node.UBezier));
            tmp_r_FindBuffer_2587353212 = node.FindBuffer("Height");
            node.DispatchBuffer(graph,tmp_r_FindBuffer_2587353212,null,false);
            return ret_989296408;
        }
    }
}
