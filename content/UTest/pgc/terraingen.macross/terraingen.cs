namespace NS_utest.pgc
{
    [EngineNS.Macross.UMacross]
    public partial class terraingen : EngineNS.Bricks.Procedure.UPgcGraphProgram
    {
        [EngineNS.Rtti.Meta]
        public override System.Boolean OnNodeInitialized(EngineNS.Bricks.Procedure.UPgcGraph graph,EngineNS.Bricks.Procedure.UPgcNodeBase node)
        {
             System.Boolean ret_2298037646 = default(System.Boolean);
            EngineNS.Bricks.Procedure.Node.UNoisePerlin tmp_r_FindPgcNodeByName_1051611981 = default(EngineNS.Bricks.Procedure.Node.UNoisePerlin);
            tmp_r_FindPgcNodeByName_1051611981 = (EngineNS.Bricks.Procedure.Node.UNoisePerlin)graph.FindPgcNodeByName("NoisePerlin1",typeof(EngineNS.Bricks.Procedure.Node.UNoisePerlin));
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
