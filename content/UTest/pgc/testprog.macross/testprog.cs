namespace NS_utest.pgc
{
    [EngineNS.Macross.UMacross]
    public partial class testprog : EngineNS.Bricks.Procedure.Node.UProgram
    {
        [EngineNS.Rtti.Meta]
        private System.Int32 Member_0 { get; set; }
        [EngineNS.Rtti.Meta]
        public override void OnPerPixel(EngineNS.Bricks.Procedure.UPgcGraph graph,EngineNS.Bricks.Procedure.Node.UProgramNode node,EngineNS.Bricks.Procedure.UBufferConponent resuilt,System.Int32 x,System.Int32 y,System.Int32 z,System.Object tag)
        {
             EngineNS.Vector2 tmp_r_GetSuperPixelAddressEX_3986003977 = default(EngineNS.Vector2);
            tmp_r_GetSuperPixelAddressEX_3986003977 = (EngineNS.Vector2)resuilt.GetSuperPixelAddressEX(0,0,0,typeof(EngineNS.Vector2));
            return;
        }
        [EngineNS.Rtti.Meta]
        public override System.Boolean OnProcedure(EngineNS.Bricks.Procedure.UPgcGraph graph,EngineNS.Bricks.Procedure.Node.UProgramNode node)
        {
             System.Boolean ret_3421051548 = default(System.Boolean);
            EngineNS.Bricks.Procedure.UBufferConponent tmp_r_FindBuffer_4184920967 = default(EngineNS.Bricks.Procedure.UBufferConponent);
            tmp_r_FindBuffer_4184920967 = node.FindBuffer("Result");
            tmp_r_FindBuffer_4184920967.DispatchPixels(((___result,___x,___y,___z)=> {  }),false);
            ret_3421051548 = true;
            return ret_3421051548;
        }
    }
}
