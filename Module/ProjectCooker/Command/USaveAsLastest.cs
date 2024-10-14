using Assimp;
using EngineNS;
using EngineNS.Bricks.CodeBuilder;
using EngineNS.Bricks.CodeBuilder.MacrossNode;
using EngineNS.Profiler;
using EngineNS.UI.Editor;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ProjectCooker.Command
{
    class USaveAsLastest : UCookCommand
    {
        public Dictionary<string, Type> AssetTypes = new Dictionary<string, Type>();
        public override async System.Threading.Tasks.Task ExecuteCommand(string[] args)
        {
            AssetTypes.Clear();
            System.Console.WriteLine("Begin AssetType");
            EngineNS.Rtti.TtTypeDescManager.Instance.InterateTypes((cb) =>
            {
                if (cb.SystemType.IsSubclassOf(typeof(EngineNS.IO.IAssetMeta)))
                {
                    var ameta = EngineNS.Rtti.TtTypeDescManager.CreateInstance(cb) as EngineNS.IO.IAssetMeta;
                    AssetTypes[ameta.GetAssetTypeName()] = cb.SystemType;
                    System.Console.WriteLine(ameta.GetAssetTypeName());
                }
            });
            System.Console.WriteLine("End AssetType");
            var assetTypes = GetArguments(args, Param_Types);
            if (assetTypes == null)
            {
                //throw new Exception("AssetType error");
                //await ProcTextures();
                await ProcUVAnim();
                await ProcMeshPrimitive();
                await ProcUMesh();
                await ProcMaterial();
                await ProcMaterialInstance();
                await ProcScene();
                await ProcPrefab();
                await ProcAnimClip();
                await ProcUI();
                await ProcMacross();
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
                        case Type_UVAnim:
                            {
                                await ProcUVAnim();
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
                        case Type_Prefab:
                            {
                                await ProcPrefab();
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
                        case Type_Macross:
                            {
                                await ProcMacross();
                            }
                            break;
                        default:
                            {
                                if (AssetTypes.TryGetValue(i, out var assetType))
                                {
                                    await ProcAssets(assetType);
                                }
                            }
                            break;
                    }
                }
            }
        }
        async System.Threading.Tasks.Task ProcAssets(System.Type type, bool bOnlyAMeta = false)
        {
            var ameta = EngineNS.Rtti.TtTypeDescManager.CreateInstance(type) as EngineNS.IO.IAssetMeta;
            var extType = ameta.TypeExt;
            for (var t = EngineNS.IO.TtFileManager.ERootDir.Game; t <= EngineNS.IO.TtFileManager.ERootDir.Editor; t++)
            {
                var root = EngineNS.TtEngine.Instance.FileManager.GetRoot(t);
                var files = EngineNS.IO.TtFileManager.GetFiles(root, "*" + extType, true);
                foreach (var i in files)
                {
                    var rp = EngineNS.IO.TtFileManager.GetRelativePath(root, i);
                    var rn = EngineNS.RName.GetRName(rp, EngineNS.RName.ERNameType.Game);
                    ameta = EngineNS.TtEngine.Instance.AssetMetaManager.GetAssetMeta(rn);
                    if (ameta == null)
                    {
                        EngineNS.Profiler.Log.WriteLine<EngineNS.Profiler.TtCookGategory>(ELogTag.Warning, $"GetAssetMeta {rn} failed");
                        continue;
                    }
                    var asset = await ameta.LoadAsset();
                    if (asset != null)
                    {
                        if (bOnlyAMeta)
                        {
                            ameta.SaveAMeta(asset);
                        }
                        else
                        {
                            asset.SaveAssetTo(rn);
                        }
                    }
                    else
                    {
                        EngineNS.Profiler.Log.WriteLineSingle($"LoadAsset {rn} failed");
                    }
                }
            }
        }
        async System.Threading.Tasks.Task ProcUVAnim()
        {
            var root = EngineNS.TtEngine.Instance.FileManager.GetRoot(EngineNS.IO.TtFileManager.ERootDir.Game);
            var files = EngineNS.IO.TtFileManager.GetFiles(root, "*" + EngineNS.EGui.TtUVAnim.AssetExt, true);
            foreach (var i in files)
            {
                var rp = EngineNS.IO.TtFileManager.GetRelativePath(root, i);
                var rn = EngineNS.RName.GetRName(rp, EngineNS.RName.ERNameType.Game);
                var asset = await EngineNS.TtEngine.Instance.GfxDevice.UvAnimManager.GetUVAnim(rn);
                if (asset != null)
                {
                    asset.SaveAssetTo(rn);
                }
                else
                {
                    EngineNS.Profiler.Log.WriteLineSingle($"GetUVAnim {rn} failed");
                }
            }

            root = EngineNS.TtEngine.Instance.FileManager.GetRoot(EngineNS.IO.TtFileManager.ERootDir.Engine);
            files = EngineNS.IO.TtFileManager.GetFiles(root, "*" + EngineNS.EGui.TtUVAnim.AssetExt, true);
            foreach (var i in files)
            {
                var rp = EngineNS.IO.TtFileManager.GetRelativePath(root, i);
                var rn = EngineNS.RName.GetRName(rp, EngineNS.RName.ERNameType.Engine);
                var asset = await EngineNS.TtEngine.Instance.GfxDevice.UvAnimManager.GetUVAnim(rn);
                if (asset != null)
                {
                    asset.SaveAssetTo(rn);
                }
                else
                {
                    EngineNS.Profiler.Log.WriteLineSingle($"GetUVAnim {rn} failed");
                }
            }
        }
        async System.Threading.Tasks.Task ProcTextures()
        {
            var root = EngineNS.TtEngine.Instance.FileManager.GetRoot(EngineNS.IO.TtFileManager.ERootDir.Game);
            var files = EngineNS.IO.TtFileManager.GetFiles(root, "*" + EngineNS.NxRHI.TtSrView.AssetExt, true);
            foreach (var i in files)
            {
                var rp = EngineNS.IO.TtFileManager.GetRelativePath(root, i);
                var rn = EngineNS.RName.GetRName(rp, EngineNS.RName.ERNameType.Game);
                var asset = await EngineNS.TtEngine.Instance.GfxDevice.TextureManager.GetTexture(rn);
                if (asset == null)
                    continue;
                //if (asset.PicDesc.DontCompress == false)
                //    asset.PicDesc.CompressFormat = EngineNS.TtEngine.Instance.Config.CompressFormat;
                //else
                //    asset.PicDesc.CompressFormat = EngineNS.NxRHI.ETextureCompressFormat.TCF_None;


                //asset.SaveAssetTo(rn);
                asset.GetAMeta().SaveAMeta(asset);
            }

            root = EngineNS.TtEngine.Instance.FileManager.GetRoot(EngineNS.IO.TtFileManager.ERootDir.Engine);
            files = EngineNS.IO.TtFileManager.GetFiles(root, "*" + EngineNS.NxRHI.TtSrView.AssetExt, true);
            foreach (var i in files)
            {
                var rp = EngineNS.IO.TtFileManager.GetRelativePath(root, i);
                var rn = EngineNS.RName.GetRName(rp, EngineNS.RName.ERNameType.Engine);
                var asset = await EngineNS.TtEngine.Instance.GfxDevice.TextureManager.GetTexture(rn);
                if (asset == null)
                    continue;
                //if (asset.PicDesc.DontCompress == false)
                //    asset.PicDesc.CompressFormat = EngineNS.TtEngine.Instance.Config.CompressFormat;
                //else
                //    asset.PicDesc.CompressFormat = EngineNS.NxRHI.ETextureCompressFormat.TCF_None;

                //asset.SaveAssetTo(rn);
                asset.GetAMeta().SaveAMeta(asset);
            }
        }
        async System.Threading.Tasks.Task ProcUMesh()
        {
            var root = EngineNS.TtEngine.Instance.FileManager.GetRoot(EngineNS.IO.TtFileManager.ERootDir.Game);
            var files = EngineNS.IO.TtFileManager.GetFiles(root, "*" + EngineNS.Graphics.Mesh.TtMaterialMesh.AssetExt, true);
            foreach (var i in files)
            {
                var rp = EngineNS.IO.TtFileManager.GetRelativePath(root, i);
                var rn = EngineNS.RName.GetRName(rp, EngineNS.RName.ERNameType.Game);
                var asset = await EngineNS.TtEngine.Instance.GfxDevice.MaterialMeshManager.GetMaterialMesh(rn);
                if (asset != null)
                {
                    asset.SaveAssetTo(rn); 
                }
                else
                {
                    EngineNS.Profiler.Log.WriteLineSingle($"GetMaterialMesh {rn} failed");
                }
            }

            root = EngineNS.TtEngine.Instance.FileManager.GetRoot(EngineNS.IO.TtFileManager.ERootDir.Engine);
            files = EngineNS.IO.TtFileManager.GetFiles(root, "*" + EngineNS.Graphics.Mesh.TtMaterialMesh.AssetExt, true);
            foreach (var i in files)
            {
                var rp = EngineNS.IO.TtFileManager.GetRelativePath(root, i);
                var rn = EngineNS.RName.GetRName(rp, EngineNS.RName.ERNameType.Engine);
                var asset = await EngineNS.TtEngine.Instance.GfxDevice.MaterialMeshManager.GetMaterialMesh(rn);
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
            var root = EngineNS.TtEngine.Instance.FileManager.GetRoot(EngineNS.IO.TtFileManager.ERootDir.Game);
            var files = EngineNS.IO.TtFileManager.GetFiles(root, "*" + EngineNS.Graphics.Mesh.TtMeshPrimitives.AssetExt, true);
            foreach (var i in files)
            {
                var rp = EngineNS.IO.TtFileManager.GetRelativePath(root, i);
                var rn = EngineNS.RName.GetRName(rp, EngineNS.RName.ERNameType.Game);
                var asset = await EngineNS.TtEngine.Instance.GfxDevice.MeshPrimitiveManager.GetMeshPrimitive(rn);
                if (asset != null)
                {
                    asset.SaveAssetTo(rn);
                }
                else
                {
                    EngineNS.Profiler.Log.WriteLineSingle($"GetMeshPrimitive {rn} failed");
                }
            }

            root = EngineNS.TtEngine.Instance.FileManager.GetRoot(EngineNS.IO.TtFileManager.ERootDir.Engine);
            files = EngineNS.IO.TtFileManager.GetFiles(root, "*" + EngineNS.Graphics.Mesh.TtMeshPrimitives.AssetExt, true);
            foreach (var i in files)
            {
                var rp = EngineNS.IO.TtFileManager.GetRelativePath(root, i);
                var rn = EngineNS.RName.GetRName(rp, EngineNS.RName.ERNameType.Engine);
                var asset = await EngineNS.TtEngine.Instance.GfxDevice.MeshPrimitiveManager.GetMeshPrimitive(rn);
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
            var root = EngineNS.TtEngine.Instance.FileManager.GetRoot(EngineNS.IO.TtFileManager.ERootDir.Game);
            var files = EngineNS.IO.TtFileManager.GetFiles(root, "*" + EngineNS.Animation.Asset.TtAnimationClip.AssetExt, true);
            foreach (var i in files)
            {
                var rp = EngineNS.IO.TtFileManager.GetRelativePath(root, i);
                var rn = EngineNS.RName.GetRName(rp, EngineNS.RName.ERNameType.Game);
                var asset = await EngineNS.TtEngine.Instance.AnimationModule.AnimationClipManager.GetAnimationClip(rn);
                if (asset != null)
                {
                    asset.SaveAssetTo(rn);
                }
                else
                {
                    EngineNS.Profiler.Log.WriteLineSingle($"GetAnimationClip {rn} failed");
                }
            }

            root = EngineNS.TtEngine.Instance.FileManager.GetRoot(EngineNS.IO.TtFileManager.ERootDir.Engine);
            files = EngineNS.IO.TtFileManager.GetFiles(root, "*" + EngineNS.Animation.Asset.TtAnimationClip.AssetExt, true);
            foreach (var i in files)
            {
                var rp = EngineNS.IO.TtFileManager.GetRelativePath(root, i);
                var rn = EngineNS.RName.GetRName(rp, EngineNS.RName.ERNameType.Engine);
                var asset = await EngineNS.TtEngine.Instance.AnimationModule.AnimationClipManager.GetAnimationClip(rn);
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
            var root = EngineNS.TtEngine.Instance.FileManager.GetRoot(EngineNS.IO.TtFileManager.ERootDir.Game);
            var files = EngineNS.IO.TtFileManager.GetFiles(root, "*" + EngineNS.Graphics.Pipeline.Shader.TtMaterial.AssetExt, true);
            foreach (var i in files)
            {
                var rp = EngineNS.IO.TtFileManager.GetRelativePath(root, i);
                var rn = EngineNS.RName.GetRName(rp, EngineNS.RName.ERNameType.Game);
                var asset = await EngineNS.TtEngine.Instance.GfxDevice.MaterialManager.GetMaterial(rn);
                if (asset != null)
                {
                    var editor = new EngineNS.Bricks.CodeBuilder.ShaderNode.TtMaterialEditor();
                    editor.MaterialGraph.ShaderEditor = editor;
                    var xml = EngineNS.IO.TtFileManager.LoadXmlFromString(asset.GraphXMLString);
                    if (xml != null)
                    {
                        object pThis = editor;
                        EngineNS.IO.SerializerHelper.ReadObjectMetaFields(editor, xml.LastChild as System.Xml.XmlElement, ref pThis, null);
                        UHLSLCodeGenerator mHLSLCodeGen = new UHLSLCodeGenerator();
                        var MaterialGraph = editor.MaterialGraph;
                        var MaterialOutput = new EngineNS.Bricks.CodeBuilder.ShaderNode.TtMaterialOutput();
                        MaterialOutput = MaterialGraph.FindNode(editor.OutputNodeId) as EngineNS.Bricks.CodeBuilder.ShaderNode.TtMaterialOutput;
                        EngineNS.Graphics.Pipeline.Shader.TtMaterial.GenMateralGraphCode(asset, mHLSLCodeGen, MaterialGraph, MaterialOutput);
                    }
                    else
                    {
                        EngineNS.Profiler.Log.WriteLine<TtCookGategory>(ELogTag.Warning, $"Material({rn} GraphXML Parse Error!)");
                    }
                    asset.SaveAssetTo(rn);
                }
                else
                {
                    EngineNS.Profiler.Log.WriteLineSingle($"GetMaterial {rn} failed");
                }
            }

            root = EngineNS.TtEngine.Instance.FileManager.GetRoot(EngineNS.IO.TtFileManager.ERootDir.Engine);
            files = EngineNS.IO.TtFileManager.GetFiles(root, "*" + EngineNS.Graphics.Pipeline.Shader.TtMaterial.AssetExt, true);
            foreach (var i in files)
            {
                var rp = EngineNS.IO.TtFileManager.GetRelativePath(root, i);
                var rn = EngineNS.RName.GetRName(rp, EngineNS.RName.ERNameType.Engine);
                var asset = await EngineNS.TtEngine.Instance.GfxDevice.MaterialManager.GetMaterial(rn);
                if (asset != null)
                {
                    var editor = new EngineNS.Bricks.CodeBuilder.ShaderNode.TtMaterialEditor();
                    editor.MaterialGraph.ShaderEditor = editor;
                    var xml = EngineNS.IO.TtFileManager.LoadXmlFromString(asset.GraphXMLString);
                    if (xml != null)
                    {
                        object pThis = editor;
                        EngineNS.IO.SerializerHelper.ReadObjectMetaFields(editor, xml.LastChild as System.Xml.XmlElement, ref pThis, null);
                        UHLSLCodeGenerator mHLSLCodeGen = new UHLSLCodeGenerator();
                        var MaterialGraph = editor.MaterialGraph;
                        var MaterialOutput = new EngineNS.Bricks.CodeBuilder.ShaderNode.TtMaterialOutput();
                        MaterialOutput = MaterialGraph.FindNode(editor.OutputNodeId) as EngineNS.Bricks.CodeBuilder.ShaderNode.TtMaterialOutput;
                        EngineNS.Graphics.Pipeline.Shader.TtMaterial.GenMateralGraphCode(asset, mHLSLCodeGen, MaterialGraph, MaterialOutput);
                    }
                    else
                    {
                        EngineNS.Profiler.Log.WriteLine<TtCookGategory>(ELogTag.Warning, $"Material({rn} GraphXML Parse Error!)");
                    }
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
            var root = EngineNS.TtEngine.Instance.FileManager.GetRoot(EngineNS.IO.TtFileManager.ERootDir.Game);
            var files = EngineNS.IO.TtFileManager.GetFiles(root, "*" + EngineNS.Graphics.Pipeline.Shader.TtMaterialInstance.AssetExt, true);
            foreach (var i in files)
            {
                var rp = EngineNS.IO.TtFileManager.GetRelativePath(root, i);
                var rn = EngineNS.RName.GetRName(rp, EngineNS.RName.ERNameType.Game);
                var asset = await EngineNS.TtEngine.Instance.GfxDevice.MaterialInstanceManager.GetMaterialInstance(rn);
                if (asset != null)
                {
                    asset.SaveAssetTo(rn);
                }
                else
                {
                    EngineNS.Profiler.Log.WriteLineSingle($"GetMaterialInstance {rn} failed");
                }
            }

            root = EngineNS.TtEngine.Instance.FileManager.GetRoot(EngineNS.IO.TtFileManager.ERootDir.Engine);
            files = EngineNS.IO.TtFileManager.GetFiles(root, "*" + EngineNS.Graphics.Pipeline.Shader.TtMaterialInstance.AssetExt, true);
            foreach (var i in files)
            {
                var rp = EngineNS.IO.TtFileManager.GetRelativePath(root, i);
                var rn = EngineNS.RName.GetRName(rp, EngineNS.RName.ERNameType.Engine);
                var asset = await EngineNS.TtEngine.Instance.GfxDevice.MaterialInstanceManager.GetMaterialInstance(rn);
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
            var root = EngineNS.TtEngine.Instance.FileManager.GetRoot(EngineNS.IO.TtFileManager.ERootDir.Game);
            var files = EngineNS.IO.TtFileManager.GetFiles(root, "*" + EngineNS.GamePlay.Scene.TtScene.AssetExt, true);
            foreach (var i in files)
            {
                var rp = EngineNS.IO.TtFileManager.GetRelativePath(root, i);
                var rn = EngineNS.RName.GetRName(rp, EngineNS.RName.ERNameType.Game);
                var world = new EngineNS.GamePlay.TtWorld(null);
                await world.InitWorld();
                var asset = await EngineNS.TtEngine.Instance.SceneManager.GetScene(world, rn);
                if (asset != null)
                {
                    asset.SaveAssetTo(rn);
                }
                else
                {
                    EngineNS.Profiler.Log.WriteLineSingle($"GetScene {rn} failed");
                }
            }

            root = EngineNS.TtEngine.Instance.FileManager.GetRoot(EngineNS.IO.TtFileManager.ERootDir.Engine);
            files = EngineNS.IO.TtFileManager.GetFiles(root, "*" + EngineNS.GamePlay.Scene.TtScene.AssetExt, true);
            foreach (var i in files)
            {
                var rp = EngineNS.IO.TtFileManager.GetRelativePath(root, i);
                var rn = EngineNS.RName.GetRName(rp, EngineNS.RName.ERNameType.Engine);
                var world = new EngineNS.GamePlay.TtWorld(null);
                await world.InitWorld();
                var asset = await EngineNS.TtEngine.Instance.SceneManager.GetScene(world, rn);
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
        async System.Threading.Tasks.Task ProcPrefab()
        {
            var root = EngineNS.TtEngine.Instance.FileManager.GetRoot(EngineNS.IO.TtFileManager.ERootDir.Game);
            var files = EngineNS.IO.TtFileManager.GetFiles(root, "*" + EngineNS.GamePlay.Scene.TtPrefab.AssetExt, true);
            foreach (var i in files)
            {
                var rp = EngineNS.IO.TtFileManager.GetRelativePath(root, i);
                var rn = EngineNS.RName.GetRName(rp, EngineNS.RName.ERNameType.Game);
                var world = new EngineNS.GamePlay.TtWorld(null);
                await world.InitWorld();
                var asset = await EngineNS.TtEngine.Instance.PrefabManager.GetPrefab(rn);
                if (asset != null)
                {
                    asset.SaveAssetTo(rn);
                }
                else
                {
                    EngineNS.Profiler.Log.WriteLineSingle($"GetPrefab {rn} failed");
                }
            }

            root = EngineNS.TtEngine.Instance.FileManager.GetRoot(EngineNS.IO.TtFileManager.ERootDir.Engine);
            files = EngineNS.IO.TtFileManager.GetFiles(root, "*" + EngineNS.GamePlay.Scene.TtPrefab.AssetExt, true);
            foreach (var i in files)
            {
                var rp = EngineNS.IO.TtFileManager.GetRelativePath(root, i);
                var rn = EngineNS.RName.GetRName(rp, EngineNS.RName.ERNameType.Engine);
                var world = new EngineNS.GamePlay.TtWorld(null);
                await world.InitWorld();
                var asset = await EngineNS.TtEngine.Instance.PrefabManager.GetPrefab(rn);
                if (asset != null)
                {
                    asset.SaveAssetTo(rn);
                }
                else
                {
                    EngineNS.Profiler.Log.WriteLineSingle($"GetPrefab {rn} failed");
                }
            }
        }
        async System.Threading.Tasks.Task ProcUI()
        {
            var macrossEditor = new UMacrossEditor();
            await macrossEditor.Initialize();
            var root = EngineNS.TtEngine.Instance.FileManager.GetRoot(EngineNS.IO.TtFileManager.ERootDir.Game);
            var files = new List<string>(EngineNS.IO.TtFileManager.GetDirectories(root, "*" + EngineNS.UI.TtUIAsset.AssetExt, true));
            foreach (var i in files)
            {
                try
                {
                    var rp = EngineNS.IO.TtFileManager.GetRelativePath(root, i);
                    var rn = EngineNS.RName.GetRName(rp, EngineNS.RName.ERNameType.Game);
                    var element = TtEngine.Instance.UIManager.Load(rn);
                    TtEngine.Instance.UIManager.Save(rn, element);
                    macrossEditor.LoadClassGraph(rn);
                    macrossEditor.SaveClassGraph(rn);
                }
                catch(Exception ex)
                {
                    Log.WriteLine<TtCookGategory>(ELogTag.Error, "UI SaveAsLasted", ex.ToString());
                }
            }
            root = EngineNS.TtEngine.Instance.FileManager.GetRoot(EngineNS.IO.TtFileManager.ERootDir.Engine);
            files = new List<string>(EngineNS.IO.TtFileManager.GetDirectories(root, "*" + EngineNS.UI.TtUIAsset.AssetExt, true));
            foreach (var i in files)
            {
                try
                {
                    var rp = EngineNS.IO.TtFileManager.GetRelativePath(root, i);
                    var rn = EngineNS.RName.GetRName(rp, EngineNS.RName.ERNameType.Engine);
                    var element = TtEngine.Instance.UIManager.Load(rn);
                    TtEngine.Instance.UIManager.Save(rn, element);
                    macrossEditor.LoadClassGraph(rn);
                    macrossEditor.SaveClassGraph(rn);
                }
                catch (Exception ex)
                {
                    Log.WriteLine<TtCookGategory>(ELogTag.Error, "UI SaveAsLasted", ex.ToString());
                }
            }
        }
        async System.Threading.Tasks.Task ProcMacross()
        {
            var root = EngineNS.TtEngine.Instance.FileManager.GetRoot(EngineNS.IO.TtFileManager.ERootDir.Game);
            var files = new List<string>(EngineNS.IO.TtFileManager.GetDirectories(root, "*" + TtMacross.AssetExt, true));
            foreach (var i in files)
            {
                try
                {
                    var macrossEditor = new UMacrossEditor();
                    await macrossEditor.Initialize();
                    var rp = EngineNS.IO.TtFileManager.GetRelativePath(root, i);
                    var rn = EngineNS.RName.GetRName(rp, EngineNS.RName.ERNameType.Game);
                    macrossEditor.LoadClassGraph(rn);
                    try
                    {
                        macrossEditor.GenerateCode();
                    }
                    catch
                    {

                    }
                    macrossEditor.SaveClassGraph(rn);
                }
                catch(Exception ex)
                {
                    Log.WriteLine<TtCookGategory>(ELogTag.Error, "Macross SaveAsLasted", ex.ToString());
                }
            }
            root = EngineNS.TtEngine.Instance.FileManager.GetRoot(EngineNS.IO.TtFileManager.ERootDir.Engine);
            files = new List<string>(EngineNS.IO.TtFileManager.GetDirectories(root, "*" + TtMacross.AssetExt, true));
            foreach (var i in files)
            {
                try
                {
                    var macrossEditor = new UMacrossEditor();
                    await macrossEditor.Initialize();
                    var rp = EngineNS.IO.TtFileManager.GetRelativePath(root, i);
                    var rn = EngineNS.RName.GetRName(rp, EngineNS.RName.ERNameType.Engine);
                    macrossEditor.LoadClassGraph(rn);
                    try
                    {
                        macrossEditor.GenerateCode();
                    }
                    catch
                    {

                    }
                    macrossEditor.SaveClassGraph(rn);
                }
                catch(Exception ex)
                {
                    Log.WriteLine<TtCookGategory>(ELogTag.Error, "Macross SaveAsLasted", ex.ToString());
                }
            }
        }
    }
}
