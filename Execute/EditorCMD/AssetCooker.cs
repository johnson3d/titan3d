using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EngineNS;

namespace EditorCMD
{
    public class AssetCooker
    {
        public EngineNS.GamePlay.GWorld World;
        public HashSet<RName> SceneAssets = new HashSet<RName>();
        public HashSet<RName> MeshAssets = new HashSet<RName>();
        public HashSet<RName> MeshSourceAssets = new HashSet<RName>();
        public HashSet<RName> TextureAssets = new HashSet<RName>();
        public HashSet<RName> UVAnimAssets = new HashSet<RName>();
        public HashSet<RName> NotifyAssets = new HashSet<RName>();
        public HashSet<RName> SkeletonAssets = new HashSet<RName>();
        public HashSet<RName> AnimationAssets = new HashSet<RName>();
        public HashSet<RName> MaterialAssets = new HashSet<RName>();
        public HashSet<RName> MaterialInstanceAssets = new HashSet<RName>();
        public HashSet<RName> MacrossAssets = new HashSet<RName>();
        public HashSet<RName> PrefabAssets = new HashSet<RName>();
        public HashSet<RName> ClusterAssets = new HashSet<RName>();
        public HashSet<RName> PhyMtlAssets = new HashSet<RName>();
        public HashSet<RName> AnimationBlendSpace1DAssets = new HashSet<RName>();
        public HashSet<RName> AnimationBlendSpaceAssets = new HashSet<RName>();
        public HashSet<RName> AnimationAdditiveBlendSpace1DAssets = new HashSet<RName>();
        public HashSet<RName> AnimationAdditiveBlendSpaceAssets = new HashSet<RName>(); 
        public HashSet<RName> AnimationClipAssets = new HashSet<RName>();
        public HashSet<RName> VertexCloudAssets = new HashSet<RName>();
        public HashSet<RName> UIAssets = new HashSet<RName>();
        public HashSet<RName> FontAssets = new HashSet<RName>();
        public HashSet<RName> XlsAssets = new HashSet<RName>();
        
        public async System.Threading.Tasks.Task CollectAssets(RName gameEntry, string platform, bool copyRInfo, string[] sm)
        {
            var RInfoManager = CMDEngine.CMDEngineInstance.mInfoManager;

            World = new EngineNS.GamePlay.GWorld();
            World.Init();

            MacrossAssets.Add(gameEntry);
            await CollectImpl(gameEntry);

            {
                var engineDesc = new EngineNS.CEngineDesc();
                HashSet<object> visitedObjects = new HashSet<object>();
                visitedObjects.Add(CMDEngine.Instance);
                await CollectRNameFromObject(engineDesc, visitedObjects);

                var rn = RName.GetRName("ui/mi_ui_default.instmtl", RName.enRNameType.Engine);
                MaterialInstanceAssets.Add(rn);
                await CollectImpl(rn);

                rn = RName.GetRName("ui/uv_ui_default.uvanim", RName.enRNameType.Engine);
                UVAnimAssets.Add(rn);
                await CollectImpl(rn);

                rn = RName.GetRName("EngineAsset/Texture/default_envmap.txpic");
                TextureAssets.Add(rn);
                await CollectImpl(rn);

                rn = RName.GetRName("Texture/eyeenvmap0.txpic");
                TextureAssets.Add(rn);
                await CollectImpl(rn);

                rn = RName.GetRName("EngineAsset/Texture/default_vignette.txpic");
                TextureAssets.Add(rn);
                await CollectImpl(rn);

                rn = RName.GetRName("ui/mi_ui_defaultfont.instmtl", RName.enRNameType.Engine);
                MaterialInstanceAssets.Add(rn);
                await CollectImpl(rn);

                rn = RName.GetRName("ui/defbutton.txpic", RName.enRNameType.Engine);
                TextureAssets.Add(rn);

                //rn = RName.GetRName("tutorials/gameplay/dataset/students_table.xls");
                //XlsAssets.Add(rn);

                await CollectImpl(rn);
            }

            CookResourceSet(SceneAssets, platform, copyRInfo);
            CookResourceSet(MeshAssets, platform, copyRInfo);
            CookResourceSet(MeshSourceAssets, platform, copyRInfo);
            CookResourceSet(TextureAssets, platform, copyRInfo, null, ResourceCooker.CookTxPic);
            CookResourceSet(UVAnimAssets, platform, copyRInfo);
            CookResourceSet(NotifyAssets, platform, copyRInfo);
            CookResourceSet(SkeletonAssets, platform, copyRInfo);
            CookResourceSet(AnimationAssets, platform, copyRInfo);
            CookResourceSet(MaterialAssets, platform, copyRInfo, new string[] { ".code", ".link", ".var" });
            CookResourceSet(MaterialInstanceAssets, platform, copyRInfo);
            CookResourceSet(MacrossAssets, platform, copyRInfo);
            CookResourceSet(PrefabAssets, platform, copyRInfo);
            CookResourceSet(ClusterAssets, platform, copyRInfo);
            CookResourceSet(PhyMtlAssets, platform, copyRInfo); 
            CookResourceSet(AnimationBlendSpace1DAssets, platform, copyRInfo);
            CookResourceSet(AnimationBlendSpaceAssets, platform, copyRInfo);
            CookResourceSet(AnimationAdditiveBlendSpace1DAssets, platform, copyRInfo);
            CookResourceSet(AnimationAdditiveBlendSpaceAssets, platform, copyRInfo);
            CookResourceSet(AnimationClipAssets, platform, copyRInfo);
            CookResourceSet(VertexCloudAssets, platform, copyRInfo);
            CookResourceSet(UIAssets, platform, copyRInfo);
            CookResourceSet(FontAssets, platform, copyRInfo);
            CookResourceSet(XlsAssets, platform, copyRInfo, null, ResourceCooker.CookXls);
        }
        public  void DirectCopyFiles(string platform)
        {
            var cookDir = CEngine.Instance.FileManager.CookingRoot;

            var rn = RName.GetRName("metaclasses");
            var targetPath = cookDir + "content/metaclasses";
            CEngine.Instance.FileManager.CopyDirectory(rn.Address, targetPath);
            CMDEngine.CMDEngineInstance.GatherFolderInfos(targetPath, "*.*");
            
            rn = RName.GetRName("cenginedesc.cfg");
            targetPath = cookDir + "content/cenginedesc.cfg";
            CEngine.Instance.FileManager.CopyFile(rn.Address, targetPath, true);
            CMDEngine.CMDEngineInstance.AddAssetInfos(targetPath);

            rn = RName.GetRName("typeredirection.xml");
            targetPath = cookDir + "content/typeredirection.xml";
            CEngine.Instance.FileManager.CopyFile(rn.Address, targetPath, true);
            CMDEngine.CMDEngineInstance.AddAssetInfos(targetPath);

            targetPath = cookDir + "deriveddatacache";
            CMDEngine.CMDEngineInstance.GatherFolderInfos(targetPath, "*.*");
        }
        private string GetTargetPath(RName rn, string cookDir)
        {
            string targetPath = "";
            switch (rn.RNameType)
            {
                case RName.enRNameType.Engine:
                    targetPath = cookDir + "enginecontent/" + rn.Name;
                    break;
                case RName.enRNameType.Game:
                    targetPath = cookDir + "content/" + rn.Name;
                    break;
                case RName.enRNameType.Editor:
                    targetPath = cookDir + "editcontent/" + rn.Name;
                    break;
            }
            return targetPath;
        }
        public delegate bool FCookResource(RName rn, string targetPath, bool copyRInfo, bool isandroid);
        private void CookResourceSet(HashSet<RName> set, string platform, bool copyRInfo, string[] appendExts = null, FCookResource cooker = null)
        {
            var cookDir = CEngine.Instance.FileManager.CookingRoot;

            bool isandroid = platform == "android";

            foreach (var i in set)
            {
                string targetPath = GetTargetPath(i, cookDir);
                if (System.IO.Directory.Exists(i.Address))
                {
                    CEngine.Instance.FileManager.CopyDirectory(i.Address, targetPath);
                    CMDEngine.CMDEngineInstance.GatherFolderInfos(targetPath, "*.*");
                }
                else
                {
                    if (cooker != null)
                    {
                        cooker(i, targetPath, copyRInfo, isandroid);
                    }
                    else
                    {
                        CEngine.Instance.FileManager.CopyFile(i.Address, targetPath, true);
                        if (isandroid)
                        {
                            CMDEngine.CMDEngineInstance.AddAssetInfos(targetPath);
                        }
                    }
                }

                if (copyRInfo)
                {
                    CEngine.Instance.FileManager.CopyFile(i.Address + ".rinfo", targetPath + ".rinfo", true);

                    if(appendExts!=null)
                    {
                        foreach(var j in appendExts)
                        {
                            CEngine.Instance.FileManager.CopyFile(i.Address + j, targetPath + j, true);
                        }
                    }
                }
            }
        }
        private async System.Threading.Tasks.Task CollectImpl(RName name)
        {
            var ext = CEngine.Instance.FileManager.GetFileExtension(name.Address);
            var resTypeName = GetResourceTypeName(ext);
            if (resTypeName == "Scene")
            {
                var scene = await EngineNS.GamePlay.GGameInstance.LoadScene(CEngine.Instance.RenderContext, World, name);
                if (scene != null)
                {
                    foreach(var i in scene.Actors.Values)
                    {
                        await AnalysisActor(i);
                    }
                }
            }
            else if (resTypeName == "Prefab")
            {
                var prefabActor = await EngineNS.GamePlay.Actor.GActor.NewPrefabActorAsync(name);
                if (prefabActor != null)
                {
                    await AnalysisActor(prefabActor);
                }
            }
            else
            {
                var RInfoManager = CMDEngine.CMDEngineInstance.mInfoManager;
                var resInfo = RInfoManager.CreateResourceInfo(resTypeName);
                if (resInfo != null)
                {
                    var rinfoAddress = name.Address + ".rinfo";
                    resInfo.Load(rinfoAddress);

                    foreach (var i in resInfo.ReferenceRNameList)
                    {
                        ext = CEngine.Instance.FileManager.GetFileExtension(i.Address);
                        resTypeName = GetResourceTypeName(ext);
                        var hashSet = GetHashSet(resTypeName);
                        if (hashSet != null)
                        {
                            if (hashSet.Contains(i) == false)
                            {
                                hashSet.Add(i);
                                await CollectImpl(i);
                            }
                        }
                        else
                        {
                            System.Diagnostics.Debug.Assert(false);
                        }
                    }
                }
                else
                {
                    EngineNS.Profiler.Log.WriteLine(EngineNS.Profiler.ELogTag.Warning, "Cook", $"ResourceTypeName({resTypeName}) create failed");
                }
            }
        }
        private async System.Threading.Tasks.Task AnalysisActor(EngineNS.GamePlay.Actor.GActor actor)
        {
            HashSet<object> visitedObjects = new HashSet<object>();
            await CollectRNameFromObject(actor, visitedObjects);
            foreach (var i in actor.Components)
            {
                await CollectRNameFromObject(i, visitedObjects);
            }
            var children = actor.GetChildrenUnsafe();
            foreach (var i in children)
            {
                await AnalysisActor(i);
            }
        }
        private async System.Threading.Tasks.Task CollectRNameFromObject(object obj, HashSet<object> visitedObjects)
        {
            if (obj == null || obj.GetType().FullName == "System.RuntimeType")
                return;
            var props = obj.GetType().GetProperties();
            foreach(var i in props)
            {
                if(i.PropertyType == typeof(RName))
                {
                    var rn = i.GetValue(obj) as RName;
                    if (rn != null)
                    {
                        var ext = CEngine.Instance.FileManager.GetFileExtension(rn.Address);
                        if (string.IsNullOrEmpty(ext))
                            continue;

                        var resTypeName = GetResourceTypeName(ext);
                        var hashSet = GetHashSet(resTypeName);
                        if (hashSet != null)
                        {
                            if (hashSet.Contains(rn) == false)
                            {
                                hashSet.Add(rn);
                                await CollectImpl(rn);
                            }
                        }
                    }
                }
                else if(i.PropertyType.IsValueType==false)
                {
                    if (i.GetIndexParameters().Length != 0)
                        continue;
                    object member = null;
                    try
                    {
                        member = i.GetValue(obj);
                    }
                    catch(Exception ex)
                    {
                        EngineNS.Profiler.Log.WriteException(ex);
                    }
                    if(member!=null)
                    {
                        if (visitedObjects.Contains(member) == false)
                        {
                            visitedObjects.Add(member);

                            var lst = member as System.Collections.IList;
                            var dict = member as System.Collections.IDictionary;
                            if (lst != null)
                            {
                                foreach (var elem in lst)
                                {
                                    if (elem == null)
                                        continue;
                                    if (elem.GetType().IsValueType == false)
                                    {
                                        await CollectRNameFromObject(elem, visitedObjects);
                                    }
                                }
                            }
                            else if (dict != null)
                            {
                                foreach (var elem in dict.Values)
                                {
                                    if (elem == null)
                                        continue;
                                    if (elem.GetType().IsValueType == false)
                                    {
                                        await CollectRNameFromObject(elem, visitedObjects);
                                    }
                                }
                            }
                            else
                            {
                                await CollectRNameFromObject(member, visitedObjects);
                            }
                        }
                    }
                }
            }
        }
        protected string GetResourceTypeName(string ext)
        {
            switch(ext)
            {
                case "gms":
                    return "Mesh";
                case "vms":
                    return "MeshSource";
                case "instmtl":
                    return "MaterialInstance";
                case "material":
                    return "Material";
                case "txpic":
                    return "Texture";
                case "map":
                    return "Scene";
                case "macross":
                    return "Macross";
                case "prefab":
                    return "Prefab";
                case "skt":
                    return "Skeleton";
                case "uvanim":
                    return "UVAnim";
                case "cluster":
                    return "MeshCluster";
                case "phymtl":
                    return "PhyMaterial";
                case "vanimbs1d":
                    return "AnimationBlendSpace1D";
                case "vanimbs":
                    return "AnimationBlendSpace";
                case "vanimabs1d":
                    return "AnimationAdditiveBlendSpace1D";
                case "vanimabs":
                    return "AnimationAdditiveBlendSpace";
                case "anim":
                    return "AnimationClip";
                case "vtc":
                    return "VertexCloud";
                case "ui":
                    return "UI";
                case "ttf":
                    return "Font";
                case "xls":
                    return "Xls";
                default:
                    return "";
            }
        }
        protected HashSet<RName> GetHashSet(string resTypeName)
        {
            switch(resTypeName)
            {
                case "Macross":
                    return MacrossAssets;
                case "Scene":
                    return SceneAssets;
                case "MeshSource":
                    return MeshSourceAssets;
                case "Mesh":
                    return MeshAssets;
                case "Prefab":
                    return PrefabAssets;
                case "Material":
                    return MaterialAssets;
                case "MaterialInstance":
                    return MaterialInstanceAssets;
                case "Texture":
                    return TextureAssets;
                case "Skeleton":
                    return SkeletonAssets;
                case "UVAnim":
                    return UVAnimAssets;
                case "MeshCluster":
                    return ClusterAssets;
                case "PhyMaterial":
                    return PhyMtlAssets;
                case "AnimationBlendSpace1D":
                    return AnimationBlendSpace1DAssets;
                case "AnimationBlendSpace":
                    return AnimationBlendSpaceAssets;
                case "AnimationAdditiveBlendSpace1D":
                    return AnimationAdditiveBlendSpace1DAssets;
                case "AnimationAdditiveBlendSpace":
                    return AnimationAdditiveBlendSpaceAssets;
                case "AnimationClip":
                    return AnimationClipAssets;
                case "VertexCloud":
                    return VertexCloudAssets;
                case "UI":
                    return UIAssets;
                case "Font":
                    return FontAssets;
                case "Xls":
                    return XlsAssets;
                default:
                    return null;
            }
        }
    }
}
