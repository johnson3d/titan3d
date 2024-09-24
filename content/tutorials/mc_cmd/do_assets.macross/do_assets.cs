namespace NS_tutorials.mc_cmd
{
    [EngineNS.Macross.TtMacross]
    public partial class do_assets : EngineNS.TtCommandMacross
    {
        public EngineNS.Macross.TtMacrossBreak breaker_FindArgument_2126302734 = new EngineNS.Macross.TtMacrossBreak("breaker_FindArgument_2126302734");
        public EngineNS.Macross.TtMacrossBreak breaker_IterateDirectory_366745237 = new EngineNS.Macross.TtMacrossBreak("breaker_IterateDirectory_366745237");
        EngineNS.Macross.TtMacrossStackFrame mFrame_DoCommand_3281139277 = new EngineNS.Macross.TtMacrossStackFrame(EngineNS.RName.GetRName("tutorials/mc_cmd/do_assets.macross", EngineNS.RName.ERNameType.Game));
        [EngineNS.Rtti.MetaAttribute]
        public override async EngineNS.Thread.Async.TtTask DoCommand(EngineNS.TtMcCommand host)
        {
            await EngineNS.Thread.TtAsyncDummyClass.DummyFunc();
            using(var guard_DoCommand = new EngineNS.Macross.TtMacrossStackGuard(mFrame_DoCommand_3281139277))
            {
                mFrame_DoCommand_3281139277.SetWatchVariable("host", host);
                System.String tmp_r_FindArgument_2126302734 = default(System.String);
                mFrame_DoCommand_3281139277.SetWatchVariable("v_argName_FindArgument_2126302734", "Dir");
                breaker_FindArgument_2126302734.TryBreak();
                tmp_r_FindArgument_2126302734 = host.FindArgument("Dir");
                mFrame_DoCommand_3281139277.SetWatchVariable("tmp_r_FindArgument_2126302734", tmp_r_FindArgument_2126302734);
                mFrame_DoCommand_3281139277.SetWatchVariable("v_dir_IterateDirectory_366745237", tmp_r_FindArgument_2126302734);
                mFrame_DoCommand_3281139277.SetWatchVariable("v_ext_IterateDirectory_366745237", ".scene");
                mFrame_DoCommand_3281139277.SetWatchVariable("v_bAllDir_IterateDirectory_366745237", true);
                breaker_IterateDirectory_366745237.TryBreak();
                this.IterateDirectory(tmp_r_FindArgument_2126302734,".scene",((ameta)=> {                 
                    return default(System.Boolean);
                }),true);
            }
        }
    }
}
