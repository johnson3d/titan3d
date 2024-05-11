using EngineNS;
using EngineNS.Bricks.CodeBuilder;
using EngineNS.Bricks.CodeBuilder.MacrossNode;
using EngineNS.Profiler;
using EngineNS.UI.Editor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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
                await ProcMeshPrimitive();
                await ProcUMesh();
                await ProcMaterial();
                await ProcMaterialInstance();
                await ProcScene();
                await ProcAnimClip();
                await ProcUI();
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
                        case Type_MeshPrimitive:
                            {
                                await ProcMeshPrimitive();
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
                        case Type_AnimClip:
                            {
                                await ProcAnimClip();
                            }
                            break;
                        case Type_UI:
                            {
                                await ProcUI();
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
            var root = EngineNS.UEngine.Instance.FileManager.GetRoot(EngineNS.IO.TtFileManager.ERootDir.Game);
            var files = EngineNS.IO.TtFileManager.GetFiles(root, "*" + EngineNS.NxRHI.USrView.AssetExt, true);
            foreach (var i in files)
            {
                var rp = EngineNS.IO.TtFileManager.GetRelativePath(root, i);
                var rn = EngineNS.RName.GetRName(rp, EngineNS.RName.ERNameType.Game);
                var asset = await EngineNS.UEngine.Instance.GfxDevice.TextureManager.GetTexture(rn);
                //if (asset.PicDesc.DontCompress == false)
                //    asset.PicDesc.CompressFormat = EngineNS.UEngine.Instance.Config.CompressFormat;
                //else
                //    asset.PicDesc.CompressFormat = EngineNS.NxRHI.ETextureCompressFormat.TCF_None;
                asset.SaveAssetTo(rn);
            }

            root = EngineNS.UEngine.Instance.FileManager.GetRoot(EngineNS.IO.TtFileManager.ERootDir.Engine);
            files = EngineNS.IO.TtFileManager.GetFiles(root, "*" + EngineNS.NxRHI.USrView.AssetExt, true);
            foreach (var i in files)
            {
                var rp = EngineNS.IO.TtFileManager.GetRelativePath(root, i);
                var rn = EngineNS.RName.GetRName(rp, EngineNS.RName.ERNameType.Engine);
                var asset = await EngineNS.UEngine.Instance.GfxDevice.TextureManager.GetTexture(rn);
                //if (asset.PicDesc.DontCompress == false)
                //    asset.PicDesc.CompressFormat = EngineNS.UEngine.Instance.Config.CompressFormat;
                //else
                //    asset.PicDesc.CompressFormat = EngineNS.NxRHI.ETextureCompressFormat.TCF_None;
                asset.SaveAssetTo(rn);
            }
        }
        async System.Threading.Tasks.Task ProcUMesh()
        {
            var root = EngineNS.UEngine.Instance.FileManager.GetRoot(EngineNS.IO.TtFileManager.ERootDir.Game);
            var files = EngineNS.IO.TtFileManager.GetFiles(root, "*" + EngineNS.Graphics.Mesh.UMaterialMesh.AssetExt, true);
            foreach (var i in files)
            {
                var rp = EngineNS.IO.TtFileManager.GetRelativePath(root, i);
                var rn = EngineNS.RName.GetRName(rp, EngineNS.RName.ERNameType.Game);
                var asset = await EngineNS.UEngine.Instance.GfxDevice.MaterialMeshManager.GetMaterialMesh(rn);
                if (asset != null)
                {
                    asset.SaveAssetTo(rn); 
                }
                else
                {
                    EngineNS.Profiler.Log.WriteLineSingle($"GetMaterialMesh {rn} failed");
                }
            }

            root = EngineNS.UEngine.Instance.FileManager.GetRoot(EngineNS.IO.TtFileManager.ERootDir.Engine);
            files = EngineNS.IO.TtFileManager.GetFiles(root, "*" + EngineNS.Graphics.Mesh.UMaterialMesh.AssetExt, true);
            foreach (var i in files)
            {
                var rp = EngineNS.IO.TtFileManager.GetRelativePath(root, i);
                var rn = EngineNS.RName.GetRName(rp, EngineNS.RName.ERNameType.Engine);
                var asset = await EngineNS.UEngine.Instance.GfxDevice.MaterialMeshManager.GetMaterialMesh(rn);
                if (asset != null)
                {
                    asset.SaveAssetTo(rn); 
                }
                else
                {
                    EngineNS.Profiler.Log.WriteLineSingle($"GetMaterialMesh {rn} failed");
                }
            }
        }
        async System.Threading.Tasks.Task ProcMeshPrimitive()
        {
            var root = EngineNS.UEngine.Instance.FileManager.GetRoot(EngineNS.IO.TtFileManager.ERootDir.Game);
            var files = EngineNS.IO.TtFileManager.GetFiles(root, "*" + EngineNS.Graphics.Mesh.UMeshPrimitives.AssetExt, true);
            foreach (var i in files)
            {
                var rp = EngineNS.IO.TtFileManager.GetRelativePath(root, i);
                var rn = EngineNS.RName.GetRName(rp, EngineNS.RName.ERNameType.Game);
                var asset = await EngineNS.UEngine.Instance.GfxDevice.MeshPrimitiveManager.GetMeshPrimitive(rn);
                if (asset != null)
                {
                    asset.SaveAssetTo(rn);
                }
                else
                {
                    EngineNS.Profiler.Log.WriteLineSingle($"GetMeshPrimitive {rn} failed");
                }
            }

            root = EngineNS.UEngine.Instance.FileManager.GetRoot(EngineNS.IO.TtFileManager.ERootDir.Engine);
            files = EngineNS.IO.TtFileManager.GetFiles(root, "*" + EngineNS.Graphics.Mesh.UMeshPrimitives.AssetExt, true);
            foreach (var i in files)
            {
                var rp = EngineNS.IO.TtFileManager.GetRelativePath(root, i);
                var rn = EngineNS.RName.GetRName(rp, EngineNS.RName.ERNameType.Engine);
                var asset = await EngineNS.UEngine.Instance.GfxDevice.MeshPrimitiveManager.GetMeshPrimitive(rn);
                if (asset != null)
                {
                    asset.SaveAssetTo(rn);
                }
                else
                {
                    EngineNS.Profiler.Log.WriteLineSingle($"GetMeshPrimitive {rn} failed");
                }
            }
        }
        async System.Threading.Tasks.Task ProcAnimClip()
        {
            var root = EngineNS.UEngine.Instance.FileManager.GetRoot(EngineNS.IO.TtFileManager.ERootDir.Game);
            var files = EngineNS.IO.TtFileManager.GetFiles(root, "*" + EngineNS.Animation.Asset.TtAnimationClip.AssetExt, true);
            foreach (var i in files)
            {
                var rp = EngineNS.IO.TtFileManager.GetRelativePath(root, i);
                var rn = EngineNS.RName.GetRName(rp, EngineNS.RName.ERNameType.Game);
                var asset = await EngineNS.UEngine.Instance.AnimationModule.AnimationClipManager.GetAnimationClip(rn);
                if (asset != null)
                {
                    asset.SaveAssetTo(rn);
                }
                else
                {
                    EngineNS.Profiler.Log.WriteLineSingle($"GetAnimationClip {rn} failed");
                }
            }

            root = EngineNS.UEngine.Instance.FileManager.GetRoot(EngineNS.IO.TtFileManager.ERootDir.Engine);
            files = EngineNS.IO.TtFileManager.GetFiles(root, "*" + EngineNS.Animation.Asset.TtAnimationClip.AssetExt, true);
            foreach (var i in files)
            {
                var rp = EngineNS.IO.TtFileManager.GetRelativePath(root, i);
                var rn = EngineNS.RName.GetRName(rp, EngineNS.RName.ERNameType.Engine);
                var asset = await EngineNS.UEngine.Instance.AnimationModule.AnimationClipManager.GetAnimationClip(rn);
                if (asset != null)
                {
                    asset.SaveAssetTo(rn);
                }
                else
                {
                    EngineNS.Profiler.Log.WriteLineSingle($"GetAnimationClip {rn} failed");
                }
            }
        }
        async System.Threading.Tasks.Task ProcMaterial()
        {
            var root = EngineNS.UEngine.Instance.FileManager.GetRoot(EngineNS.IO.TtFileManager.ERootDir.Game);
            var files = EngineNS.IO.TtFileManager.GetFiles(root, "*" + EngineNS.Graphics.Pipeline.Shader.UMaterial.AssetExt, true);
            foreach (var i in files)
            {
                var rp = EngineNS.IO.TtFileManager.GetRelativePath(root, i);
                var rn = EngineNS.RName.GetRName(rp, EngineNS.RName.ERNameType.Game);
                var asset = await EngineNS.UEngine.Instance.GfxDevice.MaterialManager.GetMaterial(rn);
                if (asset != null)
                {
                    asset.SaveAssetTo(rn);
                }
                else
                {
                    EngineNS.Profiler.Log.WriteLineSingle($"GetMaterial {rn} failed");
                }
            }

            root = EngineNS.UEngine.Instance.FileManager.GetRoot(EngineNS.IO.TtFileManager.ERootDir.Engine);
            files = EngineNS.IO.TtFileManager.GetFiles(root, "*" + EngineNS.Graphics.Pipeline.Shader.UMaterial.AssetExt, true);
            foreach (var i in files)
            {
                var rp = EngineNS.IO.TtFileManager.GetRelativePath(root, i);
                var rn = EngineNS.RName.GetRName(rp, EngineNS.RName.ERNameType.Engine);
                var asset = await EngineNS.UEngine.Instance.GfxDevice.MaterialManager.GetMaterial(rn);
                if (asset != null)
                {
                    asset.SaveAssetTo(rn);
                }
                else
                {
                    EngineNS.Profiler.Log.WriteLineSingle($"GetMaterial {rn} failed");
                }
            }
        }
        async System.Threading.Tasks.Task ProcMaterialInstance()
        {
            var root = EngineNS.UEngine.Instance.FileManager.GetRoot(EngineNS.IO.TtFileManager.ERootDir.Game);
            var files = EngineNS.IO.TtFileManager.GetFiles(root, "*" + EngineNS.Graphics.Pipeline.Shader.UMaterialInstance.AssetExt, true);
            foreach (var i in files)
            {
                var rp = EngineNS.IO.TtFileManager.GetRelativePath(root, i);
                var rn = EngineNS.RName.GetRName(rp, EngineNS.RName.ERNameType.Game);
                var asset = await EngineNS.UEngine.Instance.GfxDevice.MaterialInstanceManager.GetMaterialInstance(rn);
                if (asset != null)
                {
                    asset.SaveAssetTo(rn);
                }
                else
                {
                    EngineNS.Profiler.Log.WriteLineSingle($"GetMaterialInstance {rn} failed");
                }
            }

            root = EngineNS.UEngine.Instance.FileManager.GetRoot(EngineNS.IO.TtFileManager.ERootDir.Engine);
            files = EngineNS.IO.TtFileManager.GetFiles(root, "*" + EngineNS.Graphics.Pipeline.Shader.UMaterialInstance.AssetExt, true);
            foreach (var i in files)
            {
                var rp = EngineNS.IO.TtFileManager.GetRelativePath(root, i);
                var rn = EngineNS.RName.GetRName(rp, EngineNS.RName.ERNameType.Engine);
                var asset = await EngineNS.UEngine.Instance.GfxDevice.MaterialInstanceManager.GetMaterialInstance(rn);
                if (asset != null)
                {
                    asset.SaveAssetTo(rn);
                }
                else
                {
                    EngineNS.Profiler.Log.WriteLineSingle($"GetMaterialInstance {rn} failed");
                }
            }
        }
        async System.Threading.Tasks.Task ProcScene()
        {
            var root = EngineNS.UEngine.Instance.FileManager.GetRoot(EngineNS.IO.TtFileManager.ERootDir.Game);
            var files = EngineNS.IO.TtFileManager.GetFiles(root, "*" + EngineNS.GamePlay.Scene.UScene.AssetExt, true);
            foreach (var i in files)
            {
                var rp = EngineNS.IO.TtFileManager.GetRelativePath(root, i);
                var rn = EngineNS.RName.GetRName(rp, EngineNS.RName.ERNameType.Game);
                var world = new EngineNS.GamePlay.UWorld(null);
                await world.InitWorld();
                var asset = await EngineNS.UEngine.Instance.SceneManager.GetScene(world, rn);
                if (asset != null)
                {
                    asset.SaveAssetTo(rn);
                }
                else
                {
                    EngineNS.Profiler.Log.WriteLineSingle($"GetScene {rn} failed");
                }
            }

            root = EngineNS.UEngine.Instance.FileManager.GetRoot(EngineNS.IO.TtFileManager.ERootDir.Engine);
            files = EngineNS.IO.TtFileManager.GetFiles(root, "*" + EngineNS.GamePlay.Scene.UScene.AssetExt, true);
            foreach (var i in files)
            {
                var rp = EngineNS.IO.TtFileManager.GetRelativePath(root, i);
                var rn = EngineNS.RName.GetRName(rp, EngineNS.RName.ERNameType.Engine);
                var world = new EngineNS.GamePlay.UWorld(null);
                await world.InitWorld();
                var asset = await EngineNS.UEngine.Instance.SceneManager.GetScene(world, rn);
                if (asset != null)
                {
                    asset.SaveAssetTo(rn);
                }
                else
                {
                    EngineNS.Profiler.Log.WriteLineSingle($"GetScene {rn} failed");
                }
            }
        }
        async System.Threading.Tasks.Task ProcUI()
        {
            var macrossEditor = new UMacrossEditor();
            await macrossEditor.Initialize();
            var root = EngineNS.UEngine.Instance.FileManager.GetRoot(EngineNS.IO.TtFileManager.ERootDir.Game);
            var files = new List<string>(EngineNS.IO.TtFileManager.GetDirectories(root, "*" + EngineNS.UI.TtUIAsset.AssetExt, true));
            foreach (var i in files)
            {
                try
                {
                    var rp = EngineNS.IO.TtFileManager.GetRelativePath(root, i);
                    var rn = EngineNS.RName.GetRName(rp, EngineNS.RName.ERNameType.Game);
                    var element = UEngine.Instance.UIManager.Load(rn);
                    UEngine.Instance.UIManager.Save(rn, element);
                    macrossEditor.LoadClassGraph(rn);
                    macrossEditor.SaveClassGraph(rn);
                }
                catch(Exception ex)
                {
                    Log.WriteLine(ELogTag.Error, "UI SaveAsLasted", ex.ToString());
                }
            }
            root = EngineNS.UEngine.Instance.FileManager.GetRoot(EngineNS.IO.TtFileManager.ERootDir.Engine);
            files = new List<string>(EngineNS.IO.TtFileManager.GetDirectories(root, "*" + EngineNS.UI.TtUIAsset.AssetExt, true));
            foreach (var i in files)
            {
                try
                {
                    var rp = EngineNS.IO.TtFileManager.GetRelativePath(root, i);
                    var rn = EngineNS.RName.GetRName(rp, EngineNS.RName.ERNameType.Engine);
                    var element = UEngine.Instance.UIManager.Load(rn);
                    UEngine.Instance.UIManager.Save(rn, element);
                    macrossEditor.LoadClassGraph(rn);
                    macrossEditor.SaveClassGraph(rn);
                }
                catch (Exception ex)
                {
                    Log.WriteLine(ELogTag.Error, "UI SaveAsLasted", ex.ToString());
                }
            }
        }
        async System.Threading.Tasks.Task ProcMacross()
        {
            var macrossEditor = new UMacrossEditor();
            await macrossEditor.Initialize();
            var root = EngineNS.UEngine.Instance.FileManager.GetRoot(EngineNS.IO.TtFileManager.ERootDir.Game);
            var files = new List<string>(EngineNS.IO.TtFileManager.GetFiles(root, "*" + UMacross.AssetExt, true));
            foreach (var i in files)
            {
                try
                {
                    var rp = EngineNS.IO.TtFileManager.GetRelativePath(root, i);
                    var rn = EngineNS.RName.GetRName(rp, EngineNS.RName.ERNameType.Game);
                    macrossEditor.LoadClassGraph(rn);
                    macrossEditor.SaveClassGraph(rn);
                }
                catch(Exception ex)
                {
                    Log.WriteLine(ELogTag.Error, "Macross SaveAsLasted", ex.ToString());
                }
            }
            root = EngineNS.UEngine.Instance.FileManager.GetRoot(EngineNS.IO.TtFileManager.ERootDir.Engine);
            files = new List<string>(EngineNS.IO.TtFileManager.GetFiles(root, "*" + UMacross.AssetExt, true));
            foreach (var i in files)
            {
                try
                {
                    var rp = EngineNS.IO.TtFileManager.GetRelativePath(root, i);
                    var rn = EngineNS.RName.GetRName(rp, EngineNS.RName.ERNameType.Engine);
                    macrossEditor.LoadClassGraph(rn);
                    macrossEditor.SaveClassGraph(rn);
                }
                catch(Exception ex)
                {
                    Log.WriteLine(ELogTag.Error, "Macross SaveAsLasted", ex.ToString());
                }
            }
        }
    }
}
