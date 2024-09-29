namespace NS_tutorials.mc_cmd
{
    [EngineNS.Macross.TtMacross]
    public partial class do_assets : EngineNS.TtCommandMacross
    {
        EngineNS.Macross.TtMacrossStackFrame mFrame_DoCommand_3281139277 = new EngineNS.Macross.TtMacrossStackFrame(EngineNS.RName.GetRName("tutorials/mc_cmd/do_assets.macross", EngineNS.RName.ERNameType.Game));
        [EngineNS.Rtti.MetaAttribute]
        public override async EngineNS.Thread.Async.TtTask DoCommand(EngineNS.TtMcCommand host)
        {
            await EngineNS.Thread.TtAsyncDummyClass.DummyFunc();
            using(var guard_DoCommand = new EngineNS.Macross.TtMacrossStackGuard(mFrame_DoCommand_3281139277))
            {
                System.String tmp_r_FindArgument_2126302734 = default(System.String);
                tmp_r_FindArgument_2126302734 = host.FindArgument("Dir");
                this.IterateDirectory(tmp_r_FindArgument_2126302734,".srv",((ameta)=> {                 
                    EngineNS.Graphics.Pipeline.Shader.TtMaterial tmp_r_FindMaterial_771818083 = default(EngineNS.Graphics.Pipeline.Shader.TtMaterial);
                    EngineNS.Graphics.Pipeline.Shader.TtMaterialInstance tmp_r_CreateMaterialInstance_312885784 = default(EngineNS.Graphics.Pipeline.Shader.TtMaterialInstance);
                    System.Boolean tmp_r_SetSrv_1308319261 = default(System.Boolean);
                    EngineNS.RName tmp_r_GetRName_1223778736 = default(EngineNS.RName);
                    tmp_r_FindMaterial_771818083 = EngineNS.TtEngine.Instance.GfxDevice.MaterialManager.FindMaterial(EngineNS.RName.GetRName("project_factory/materials/m_common01.material", EngineNS.RName.ERNameType.Game));
                    tmp_r_CreateMaterialInstance_312885784 = EngineNS.TtEngine.Instance.GfxDevice.MaterialInstanceManager.CreateMaterialInstance(tmp_r_FindMaterial_771818083);
                    tmp_r_SetSrv_1308319261 = tmp_r_CreateMaterialInstance_312885784.SetSrv("DiffuseTexture",ameta.AssetName);
                    tmp_r_GetRName_1223778736 = EngineNS.TtEngine.GetRName((ameta.AssetName.Name + ameta.TypeExt),EngineNS.RName.ERNameType.Game);
                    tmp_r_CreateMaterialInstance_312885784.SaveAssetTo(tmp_r_GetRName_1223778736);
                    return default(System.Boolean);
                }),true);
            }
        }
    }
}
/* 杩欐槸涓€涓猟emo  锛岀敤鏉ュ睍绀洪€氳繃瀹忓浘鍒涘缓缂栬緫鍣ㄥ懡浠よ鑳藉姏
do_assets瀹忓浘锛屽埄鐢↖terateDirectory閬嶅巻鎸囧畾鐩綍鐨剆rv鏂囦欢
鍙屽嚮fun鍙傛暟杩涘叆鍥炴帀鍥?鍦ㄥ洖鎺変腑锛屽埄鐢–reateMaterialInstance鍒涘缓鏉愯川锛屾渶鍚庤皟鐢⊿aveAssetTo淇濆瓨
*/