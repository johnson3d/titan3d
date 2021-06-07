using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectCooker.Command
{
    class USaveAsLastest : UCookCommand
    {
        public const string Param_Types = "AssetType=";
        public const string Type_Texture = "Texture";
        public const string Type_Mesh = "Mesh";
        public const string Type_Material = "Material";
        public const string Type_MaterialInst = "MaterialInst";
        public override async System.Threading.Tasks.Task ExecuteCommand(string[] args)
        {
            await base.ExecuteCommand(args);

            var assetTypes = GetArguments(args, Param_Types);
            if (assetTypes == null)
            {
                //throw new Exception("AssetType error");
                await ProcTextures();
                await ProcMaterial();
                await ProcMaterialInstance();
            }
            else
            {
                foreach (var i in assetTypes)
                {
                    switch (i)
                    {
                        case Type_Texture:
                            {
                                await ProcTextures();
                            }
                            break;
                        case Type_Mesh:
                            break;
                        case Type_Material:
                            {
                                await ProcMaterial();
                            }
                            break;
                        case Type_MaterialInst:
                            {
                                await ProcMaterialInstance();
                            }
                            break;
                        default:
                            break;
                    }
                }
            }
        }
        async System.Threading.Tasks.Task ProcTextures()
        {
            var root = EngineNS.UEngine.Instance.FileManager.GetRoot(EngineNS.IO.FileManager.ERootDir.Game);
            var files = EngineNS.IO.FileManager.GetFiles(root, "*" + EngineNS.RHI.CShaderResourceView.AssetExt, true);
            foreach (var i in files)
            {
                var rp = EngineNS.IO.FileManager.GetRelativePath(root, i);
                var rn = EngineNS.RName.GetRName(rp, EngineNS.RName.ERNameType.Game);
                var asset = await EngineNS.UEngine.Instance.GfxDevice.TextureManager.GetTexture(rn);
                asset.SaveAssetTo(rn);
            }
        }
        async System.Threading.Tasks.Task ProcMaterial()
        {
            var root = EngineNS.UEngine.Instance.FileManager.GetRoot(EngineNS.IO.FileManager.ERootDir.Game);
            var files = EngineNS.IO.FileManager.GetFiles(root, "*" + EngineNS.Graphics.Pipeline.Shader.UMaterial.AssetExt, true);
            foreach (var i in files)
            {
                var rp = EngineNS.IO.FileManager.GetRelativePath(root, i);
                var rn = EngineNS.RName.GetRName(rp, EngineNS.RName.ERNameType.Game);
                var asset = await EngineNS.UEngine.Instance.GfxDevice.MaterialManager.GetMaterial(rn);
                asset.SaveAssetTo(rn);
            }
        }
        async System.Threading.Tasks.Task ProcMaterialInstance()
        {
            var root = EngineNS.UEngine.Instance.FileManager.GetRoot(EngineNS.IO.FileManager.ERootDir.Game);
            var files = EngineNS.IO.FileManager.GetFiles(root, "*" + EngineNS.Graphics.Pipeline.Shader.UMaterial.AssetExt, true);
            foreach (var i in files)
            {
                var rp = EngineNS.IO.FileManager.GetRelativePath(root, i);
                var rn = EngineNS.RName.GetRName(rp, EngineNS.RName.ERNameType.Game);
                var asset = await EngineNS.UEngine.Instance.GfxDevice.MaterialInstanceManager.GetMaterialInstance(rn);
                asset.SaveAssetTo(rn);
            }
        }
    }
}
