using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EngineNS;
using EngineNS.GamePlay.Actor;

namespace EditorCMD
{
    public class RNameModifier : AssetCooker
    {
        private string RootPath = CEngine.Instance.FileManager.ProjectContent;
        private RName.enRNameType CurRNameType = RName.enRNameType.Game;
        public void CollectRefObjects(RName rn)
        {
            RootPath = CEngine.Instance.FileManager.ProjectContent;
            CurRNameType = RName.enRNameType.Game;
            var files = CEngine.Instance.FileManager.GetFiles(RootPath, "*.rinfo");
            ProcFiles(files, rn);

            RootPath = CEngine.Instance.FileManager.EditorContent;
            CurRNameType = RName.enRNameType.Editor;
            files = CEngine.Instance.FileManager.GetFiles(RootPath, "*.rinfo");
            ProcFiles(files, rn);

            RootPath = CEngine.Instance.FileManager.EngineContent;
            CurRNameType = RName.enRNameType.Engine;
            files = CEngine.Instance.FileManager.GetFiles(RootPath, "*.rinfo");
            ProcFiles(files, rn);
        }
        public async System.Threading.Tasks.Task SaveRefObjects(RName rn, string newname)
        {
            string src = rn.Address;
            string saveName = rn.Name;
            var rc = EngineNS.CEngine.Instance.RenderContext;
            foreach (var i in MeshAssets)
            {
                var mesh = await EngineNS.CEngine.Instance.MeshManager.CreateMeshAsync(rc, i, false, true);
                if (mesh == null)
                    continue;
                rn.Name = newname;
                mesh.SaveMesh(i.Address + ".ren");

                var resInfo = CMDEngine.CMDEngineInstance.mInfoManager.CreateResourceInfo("Mesh") as EditorCommon.ResourceInfos.MeshResourceInfo;
                if (resInfo != null)
                {
                    resInfo.Load(i.Address + ".rinfo");
                    resInfo.RefreshReferenceRNames(mesh);
                }
                var t = resInfo.Save(i.Address + ".rinfo", false);

                rn.Name = saveName;
            }

            foreach (var i in UVAnimAssets)
            {
                var uvAnim = new EngineNS.UISystem.UVAnim();
                if (false == await uvAnim.LoadUVAnimAsync(EngineNS.CEngine.Instance.RenderContext, i))
                    continue;

                rn.Name = newname;
                uvAnim.Save2Xnd(i.Address + ".ren");

                var resInfo = CMDEngine.CMDEngineInstance.mInfoManager.CreateResourceInfo("UVAnim") as UVAnimEditor.UVAnimResourceInfo;
                if (resInfo != null)
                {
                    resInfo.Load(i.Address + ".rinfo");
                    resInfo.RefreshReferenceRNames(uvAnim);
                }
                var t = resInfo.Save(i.Address + ".rinfo", false);

                rn.Name = saveName;
            }

            foreach (var i in MaterialAssets)
            {
                var mtlInst = await EngineNS.CEngine.Instance.MaterialManager.GetMaterialAsync(rc, rn);
                if (mtlInst == null)
                    continue;

                rn.Name = newname;
                mtlInst.SaveMaterial(i.Address + ".ren");

                var resInfo = CMDEngine.CMDEngineInstance.mInfoManager.CreateResourceInfo("Material") as MaterialEditor.ResourceInfos.MaterialResourceInfo;
                if (resInfo != null)
                {
                    resInfo.Load(i.Address + ".rinfo");
                    resInfo.RefreshReferenceRNames(mtlInst);
                }
                var t = resInfo.Save(i.Address + ".rinfo", false);

                rn.Name = saveName;
            }

            foreach (var i in MaterialInstanceAssets)
            {
                var mtlInst = await EngineNS.CEngine.Instance.MaterialInstanceManager.GetMaterialInstanceAsync(rc, rn);
                if (mtlInst == null)
                    continue;

                rn.Name = newname;
                mtlInst.SaveMaterialInstance(i.Address + ".ren");

                var resInfo = CMDEngine.CMDEngineInstance.mInfoManager.CreateResourceInfo("MaterialInstance") as MaterialEditor.ResourceInfos.MaterialInstanceResourceInfo;
                if (resInfo != null)
                {
                    resInfo.Load(i.Address + ".rinfo");
                    resInfo.RefreshReferenceRNames(mtlInst);
                }
                var t = resInfo.Save(i.Address + ".rinfo", false);

                rn.Name = saveName;
            }

            foreach (var i in MacrossAssets)
            {

            }

            foreach (var i in PrefabAssets)
            {
                var prefab = await CEngine.Instance.PrefabManager.GetPrefab(rc, i, false);
                if (prefab == null)
                    continue;

                rn.Name = newname;
                prefab.SavePrefab(i.Address + ".ren");

                var resInfo = CMDEngine.CMDEngineInstance.mInfoManager.CreateResourceInfo("Prefab") as EditorCommon.ResourceInfos.PrefabResourceInfo;
                if (resInfo != null)
                {
                    resInfo.Load(i.Address + ".rinfo");
                    await resInfo.RefreshReferenceRNames(prefab);
                }
                var t = resInfo.Save(i.Address + ".rinfo", false);

                rn.Name = saveName;
            }

            FinishRefObjects();

            rn.Name = newname;
            CEngine.Instance.FileManager.CopyFile(src, rn.Address, true, true);
            CEngine.Instance.FileManager.CopyFile(src + ".rinfo", rn.Address + ".rinfo", true, true);

            CEngine.Instance.FileManager.DeleteFile(src);
            if (CEngine.Instance.FileManager.FileExists(src + ".snap"))
            {
                CEngine.Instance.FileManager.DeleteFile(src + ".snap");
            }
            CEngine.Instance.FileManager.DeleteFile(src + ".rinfo");
        }
        public void FinishRefObjects()
        {
            foreach (var i in MeshAssets)
            {
                CEngine.Instance.FileManager.CopyFile(i.Address + ".ren", i.Address, true, true);
                CEngine.Instance.FileManager.DeleteFile(i.Address + ".ren");
            }
            foreach (var i in UVAnimAssets)
            {
                CEngine.Instance.FileManager.CopyFile(i.Address + ".ren", i.Address, true, true);
                CEngine.Instance.FileManager.DeleteFile(i.Address + ".ren");
            }
            foreach (var i in MaterialAssets)
            {
                CEngine.Instance.FileManager.CopyFile(i.Address + ".ren", i.Address, true, true);
                CEngine.Instance.FileManager.DeleteFile(i.Address + ".ren");
            }
            foreach (var i in MaterialInstanceAssets)
            {
                CEngine.Instance.FileManager.CopyFile(i.Address + ".ren", i.Address, true, true);
                CEngine.Instance.FileManager.DeleteFile(i.Address + ".ren");
            }
            foreach (var i in MacrossAssets)
            {
                //CEngine.Instance.FileManager.CopyFile(i.Address + ".ren", i.Address, true, true);
                //CEngine.Instance.FileManager.DeleteFile(i.Address + ".ren");
            }
            foreach (var i in PrefabAssets)
            {
                CEngine.Instance.FileManager.CopyFile(i.Address + ".ren", i.Address, true, true);
                CEngine.Instance.FileManager.DeleteFile(i.Address + ".ren");
            }
        }
        private void ProcFiles(List<string> files, RName rn)
        {
            foreach (var i in files)
            {
                bool error;
                var sf = EngineNS.CEngine.Instance.FileManager.NormalizePath(i, out error);
                sf = sf.Substring(RootPath.Length);
                sf = sf.Substring(0, sf.Length - ".rinfo".Length);
                var rn1 = EngineNS.RName.GetRName(sf, CurRNameType);

                var rinfo = _CreateObjectFromXML(i);
                if (rinfo != null)
                {
                    foreach(var j in rinfo.ReferenceRNameList)
                    {
                        if(rn == j)
                        {
                            var hashSet = GetHashSet(rinfo.ResourceType);
                            if (hashSet != null)
                            {
                                if (hashSet.Contains(rn1) == false)
                                {
                                    hashSet.Add(rn1);
                                    break;
                                }
                            }
                        }
                    }
                }
            }
        }
        private EditorCommon.Resources.ResourceInfo _CreateObjectFromXML(string file)
        {
            string resType = "";
            using (var xml = EngineNS.IO.XmlHolder.LoadXML(file))
            {
                if (xml == null)
                {
                    EngineNS.Profiler.Log.WriteLine(EngineNS.Profiler.ELogTag.Error, "IO", $"LoadXML failed {file}");
                    return null;
                }
                var attr = xml.RootNode.FindAttrib("ResourceType");
                if (attr == null)
                    return null;
                resType = attr.Value;
            }
            var result = CMDEngine.CMDEngineInstance.mInfoManager.CreateResourceInfo(resType);
            if (result == null)
                return null;
            result.Load(file);
            return result;
        }
    }
}
