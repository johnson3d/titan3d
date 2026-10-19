namespace NS_tutorials.pgc
{
    [EngineNS.Macross.TtMacross]
    [EngineNS.Macross.TtMacrossSign(RName_Name = "tutorials/pgc/testspline.scene", RName_Type = EngineNS.RName.ERNameType.Game)]
    public partial class testspline : EngineNS.GamePlay.Scene.TtSceneMacrossBase
    {
        EngineNS.Macross.TtMacrossStackFrame mFrame_InitializeMacrossNodePropertyValues_3642530370 = new EngineNS.Macross.TtMacrossStackFrame(EngineNS.RName.GetRName("tutorials/pgc/testspline.scene", EngineNS.RName.ERNameType.Game));
        EngineNS.Macross.TtMacrossStackTracer mStack_InitializeMacrossNodePropertyValues_3642530370 = new EngineNS.Macross.TtMacrossStackTracer();
        public override void InitializeMacrossNodePropertyValues()
        {
            if (EngineNS.Macross.TtMacrossPatch.Active && EngineNS.Macross.TtMacrossPatch.TryInvoke(0xC962F1F060807423UL, this))
                return;
            using(var guard_InitializeMacrossNodePropertyValues = new EngineNS.Macross.TtMacrossStackGuard(mStack_InitializeMacrossNodePropertyValues_3642530370,mFrame_InitializeMacrossNodePropertyValues_3642530370))
            {
            }
        }
    }
}
