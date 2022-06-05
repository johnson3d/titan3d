using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectCooker.Command
{
    class USaveAsLastest : UCookCommand
    {
        public override async System.Threading.Tasks.Task ExecuteCommand(string[] args)
        {
            var assetTypes = GetArguments(args, Param_Types);
            if (assetTypes == null)
            {
                //throw new Exception("AssetType error");
                //await ProcTextures();
                await ProcUMesh();
                await ProcMaterial();
                await ProcMaterialInstance();
                await ProcScene();
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
                            {
                                await ProcUMesh();
                            }
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
                        case Type_Scene:
                            {
                                await ProcScene();
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

            root = EngineNS.UEngine.Instance.FileManager.GetRoot(EngineNS.IO.FileManager.ERootDir.Engine);
            files = EngineNS.IO.FileManager.GetFiles(root, "*" + EngineNS.RHI.CShaderResourceView.AssetExt, true);
            foreach (var i in files)
            {
                var rp = EngineNS.IO.FileManager.GetRelativePath(root, i);
                var rn = EngineNS.RName.GetRName(rp, EngineNS.RName.ERNameType.Engine);
                var asset = await EngineNS.UEngine.Instance.GfxDevice.TextureManager.GetTexture(rn);
                asset.SaveAssetTo(rn);
            }
        }
        async System.Threading.Tasks.Task ProcUMesh()
        {
            var root = EngineNS.UEngine.Instance.FileManager.GetRoot(EngineNS.IO.FileManager.ERootDir.Game);
            var files = EngineNS.IO.FileManager.GetFiles(root, "*" + EngineNS.Graphics.Mesh.UMaterialMesh.AssetExt, true);
            foreach (var i in files)
            {
                var rp = EngineNS.IO.FileManager.GetRelativePath(root, i);
                var rn = EngineNS.RName.GetRName(rp, EngineNS.RName.ERNameType.Game);
                var asset = await EngineNS.UEngine.Instance.GfxDevice.MaterialMeshManager.GetMaterialMesh(rn);
                asset.SaveAssetTo(rn);
            }

            root = EngineNS.UEngine.Instance.FileManager.GetRoot(EngineNS.IO.FileManager.ERootDir.Engine);
            files = EngineNS.IO.FileManager.GetFiles(root, "*" + EngineNS.Graphics.Mesh.UMaterialMesh.AssetExt, true);
            foreach (var i in files)
            {
                var rp = EngineNS.IO.FileManager.GetRelativePath(root, i);
                var rn = EngineNS.RName.GetRName(rp, EngineNS.RName.ERNameType.Engine);
                var asset = await EngineNS.UEngine.Instance.GfxDevice.MaterialMeshManager.GetMaterialMesh(rn);
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

            root = EngineNS.UEngine.Instance.FileManager.GetRoot(EngineNS.IO.FileManager.ERootDir.Engine);
            files = EngineNS.IO.FileManager.GetFiles(root, "*" + EngineNS.Graphics.Pipeline.Shader.UMaterial.AssetExt, true);
            foreach (var i in files)
            {
                var rp = EngineNS.IO.FileManager.GetRelativePath(root, i);
                var rn = EngineNS.RName.GetRName(rp, EngineNS.RName.ERNameType.Engine);
                var asset = await EngineNS.UEngine.Instance.GfxDevice.MaterialManager.GetMaterial(rn);
                asset.SaveAssetTo(rn);
            }
        }
        async System.Threading.Tasks.Task ProcMaterialInstance()
        {
            var root = EngineNS.UEngine.Instance.FileManager.GetRoot(EngineNS.IO.FileManager.ERootDir.Game);
            var files = EngineNS.IO.FileManager.GetFiles(root, "*" + EngineNS.Graphics.Pipeline.Shader.UMaterialInstance.AssetExt, true);
            foreach (var i in files)
            {
                var rp = EngineNS.IO.FileManager.GetRelativePath(root, i);
                var rn = EngineNS.RName.GetRName(rp, EngineNS.RName.ERNameType.Game);
                var asset = await EngineNS.UEngine.Instance.GfxDevice.MaterialInstanceManager.GetMaterialInstance(rn);
                asset.SaveAssetTo(rn);
            }

            root = EngineNS.UEngine.Instance.FileManager.GetRoot(EngineNS.IO.FileManager.ERootDir.Engine);
            files = EngineNS.IO.FileManager.GetFiles(root, "*" + EngineNS.Graphics.Pipeline.Shader.UMaterialInstance.AssetExt, true);
            foreach (var i in files)
            {
                var rp = EngineNS.IO.FileManager.GetRelativePath(root, i);
                var rn = EngineNS.RName.GetRName(rp, EngineNS.RName.ERNameType.Engine);
                var asset = await EngineNS.UEngine.Instance.GfxDevice.MaterialInstanceManager.GetMaterialInstance(rn);
                asset.SaveAssetTo(rn);
            }
        }
        async System.Threading.Tasks.Task ProcScene()
        {
            var root = EngineNS.UEngine.Instance.FileManager.GetRoot(EngineNS.IO.FileManager.ERootDir.Game);
            var files = EngineNS.IO.FileManager.GetFiles(root, "*" + EngineNS.GamePlay.Scene.UScene.AssetExt, true);
            foreach (var i in files)
            {
                var rp = EngineNS.IO.FileManager.GetRelativePath(root, i);
                var rn = EngineNS.RName.GetRName(rp, EngineNS.RName.ERNameType.Game);
                var world = new EngineNS.GamePlay.UWorld(null);
                await world.InitWorld();
                var asset = await EngineNS.UEngine.Instance.SceneManager.GetScene(world, rn);
                asset.SaveAssetTo(rn);
            }

            root = EngineNS.UEngine.Instance.FileManager.GetRoot(EngineNS.IO.FileManager.ERootDir.Engine);
            files = EngineNS.IO.FileManager.GetFiles(root, "*" + EngineNS.GamePlay.Scene.UScene.AssetExt, true);
            foreach (var i in files)
            {
                var rp = EngineNS.IO.FileManager.GetRelativePath(root, i);
                var rn = EngineNS.RName.GetRName(rp, EngineNS.RName.ERNameType.Engine);
                var world = new EngineNS.GamePlay.UWorld(null);
                await world.InitWorld();
                var asset = await EngineNS.UEngine.Instance.SceneManager.GetScene(world, rn);
                asset.SaveAssetTo(rn);
            }
        }
    }
}
