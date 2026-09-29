namespace NS_utest.pgc
{
    [EngineNS.Macross.TtMacross]
    [EngineNS.Macross.TtMacrossSign(RName_Name = "utest/pgc/testprog.macross", RName_Type = EngineNS.RName.ERNameType.Game)]
    public partial class testprog : EngineNS.Bricks.Procedure.Node.UProgram
    {
        [EngineNS.Rtti.Meta]
        [System.ComponentModel.Category("Macross")]
        [System.ComponentModel.DisplayName("Member_0")]
        private System.Int32 Member_0 { get; set; }
    }
}
