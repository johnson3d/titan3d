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
                throw new Exception("AssetType error");
            }
            foreach(var i in assetTypes)
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
                        break;
                    case Type_MaterialInst:
                        break;
                }
            }
        }
        async System.Threading.Tasks.Task ProcTextures()
        {
            var root = EngineNS.UEngine.Instance.FileManager.GetRoot(EngineNS.IO.FileManager.ERootDir.Game);
            var files = EngineNS.IO.FileManager.GetFiles(root, EngineNS.RHI.CShaderResourceView.AssetExt, true);
            foreach (var i in files)
            {
                var rp = EngineNS.IO.FileManager.GetRelativePath(i, root);
                var rn = EngineNS.RName.GetRName(rp, EngineNS.RName.ERNameType.Game);
                var texture = await EngineNS.UEngine.Instance.GfxDevice.TextureManager.GetTexture(rn);
                texture.SaveAssetTo(rn);
            }
        }
    }
}
