namespace NS_tutorials.mc_cmd
{
    [EngineNS.Macross.TtMacross]
    public partial class do_assets : EngineNS.TtCommandMacross
    {
        public EngineNS.Macross.TtMacrossBreak breaker_FindArgument_2126302734 = new EngineNS.Macross.TtMacrossBreak("breaker_FindArgument_2126302734");
        public EngineNS.Macross.TtMacrossBreak breaker_FindMaterial_771818083 = new EngineNS.Macross.TtMacrossBreak("breaker_FindMaterial_771818083");
        public EngineNS.Macross.TtMacrossBreak breaker_CreateMaterialInstance_312885784 = new EngineNS.Macross.TtMacrossBreak("breaker_CreateMaterialInstance_312885784");
        public EngineNS.Macross.TtMacrossBreak breaker_SetSrv_1308319261 = new EngineNS.Macross.TtMacrossBreak("breaker_SetSrv_1308319261");
        public EngineNS.Macross.TtMacrossBreak breaker_GetRName_1223778736 = new EngineNS.Macross.TtMacrossBreak("breaker_GetRName_1223778736");
        public EngineNS.Macross.TtMacrossBreak breaker_SaveAssetTo_2390019430 = new EngineNS.Macross.TtMacrossBreak("breaker_SaveAssetTo_2390019430");
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
                mFrame_DoCommand_3281139277.SetWatchVariable("v_ext_IterateDirectory_366745237", ".srv");
                mFrame_DoCommand_3281139277.SetWatchVariable("v_bAllDir_IterateDirectory_366745237", true);
                breaker_IterateDirectory_366745237.TryBreak();
                this.IterateDirectory(tmp_r_FindArgument_2126302734,".srv",((ameta)=> {                 
                    EngineNS.Graphics.Pipeline.Shader.TtMaterial tmp_r_FindMaterial_771818083 = default(EngineNS.Graphics.Pipeline.Shader.TtMaterial);
                    EngineNS.Graphics.Pipeline.Shader.TtMaterialInstance tmp_r_CreateMaterialInstance_312885784 = default(EngineNS.Graphics.Pipeline.Shader.TtMaterialInstance);
                    System.Boolean tmp_r_SetSrv_1308319261 = default(System.Boolean);
                    EngineNS.RName tmp_r_GetRName_1223778736 = default(EngineNS.RName);
                    mFrame_DoCommand_3281139277.SetWatchVariable("v_rn_FindMaterial_771818083", EngineNS.RName.GetRName("project_factory/materials/m_common01.material", EngineNS.RName.ERNameType.Game));
                    breaker_FindMaterial_771818083.TryBreak();
                    tmp_r_FindMaterial_771818083 = EngineNS.TtEngine.Instance.GfxDevice.MaterialManager.FindMaterial(EngineNS.RName.GetRName("project_factory/materials/m_common01.material", EngineNS.RName.ERNameType.Game));
                    mFrame_DoCommand_3281139277.SetWatchVariable("tmp_r_FindMaterial_771818083", tmp_r_FindMaterial_771818083);
                    mFrame_DoCommand_3281139277.SetWatchVariable("v_mtl_CreateMaterialInstance_312885784", tmp_r_FindMaterial_771818083);
                    breaker_CreateMaterialInstance_312885784.TryBreak();
                    tmp_r_CreateMaterialInstance_312885784 = EngineNS.TtEngine.Instance.GfxDevice.MaterialInstanceManager.CreateMaterialInstance(tmp_r_FindMaterial_771818083);
                    mFrame_DoCommand_3281139277.SetWatchVariable("tmp_r_CreateMaterialInstance_312885784", tmp_r_CreateMaterialInstance_312885784);
                    mFrame_DoCommand_3281139277.SetWatchVariable("v_name_SetSrv_1308319261", "DiffuseTexture");
                    mFrame_DoCommand_3281139277.SetWatchVariable("v_srv_SetSrv_1308319261", ameta.AssetName);
                    breaker_SetSrv_1308319261.TryBreak();
                    tmp_r_SetSrv_1308319261 = tmp_r_CreateMaterialInstance_312885784.SetSrv("DiffuseTexture",ameta.AssetName);
                    mFrame_DoCommand_3281139277.SetWatchVariable("tmp_r_SetSrv_1308319261", tmp_r_SetSrv_1308319261);
                    mFrame_DoCommand_3281139277.SetWatchVariable("v_name_GetRName_1223778736", (ameta.AssetName.Name + ameta.TypeExt));
                    mFrame_DoCommand_3281139277.SetWatchVariable("v_type_GetRName_1223778736", EngineNS.RName.ERNameType.Game);
                    breaker_GetRName_1223778736.TryBreak();
                    tmp_r_GetRName_1223778736 = EngineNS.TtEngine.GetRName((ameta.AssetName.Name + ameta.TypeExt),EngineNS.RName.ERNameType.Game);
                    mFrame_DoCommand_3281139277.SetWatchVariable("tmp_r_GetRName_1223778736", tmp_r_GetRName_1223778736);
                    mFrame_DoCommand_3281139277.SetWatchVariable("v_name_SaveAssetTo_2390019430", tmp_r_GetRName_1223778736);
                    breaker_SaveAssetTo_2390019430.TryBreak();
                    tmp_r_CreateMaterialInstance_312885784.SaveAssetTo(tmp_r_GetRName_1223778736);
                    return default(System.Boolean);
                }),true);
            }
        }
    }
}
