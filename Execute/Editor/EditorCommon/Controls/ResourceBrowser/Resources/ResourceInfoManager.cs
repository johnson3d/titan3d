using System;
using System.Collections.Generic;
using System.Linq;

namespace EditorCommon.Resources
{
    public class ResourceInfoManager : EngineNS.Editor.IEditorInstanceObject
    {
        public static ResourceInfoManager Instance
        {
            get
            {
                var name = typeof(ResourceInfoManager).FullName;
                if (EngineNS.CEngine.Instance.GameEditorInstance[name] == null)
                {
                    var rManager = new ResourceInfoManager();
                    EngineNS.IO.FileManager.OnTrySaveRInfo = rManager.TrySaveRInfo;
                    EngineNS.CEngine.Instance.GameEditorInstance[name] = rManager;
                }
                return EngineNS.CEngine.Instance.GameEditorInstance[name];
            }
        }

        public void TrySaveRInfo(string resType, EngineNS.RName name, object obj)
        {
            var rInfo = this.CreateResourceInfo(resType);
            if (rInfo != null)
            {
                var noused = rInfo.Save(name.Address + ".rinfo", false);
            }
        }

        public ResourceInfoManager()
        {

        }

        public void FinalCleanup()
        {

        }

        Dictionary<string, ResourceInfoMetaData> mResourceInfoStrDic = new Dictionary<string, ResourceInfoMetaData>();
        Dictionary<Type, ResourceInfoMetaData> mResourceInfoTypeDic = new Dictionary<Type, ResourceInfoMetaData>();

        public ResourceInfoMetaData GetResourceInfoMetaData(Type resourceInfoType)
        {
            ResourceInfoMetaData retValue;
            if (mResourceInfoTypeDic.TryGetValue(resourceInfoType, out retValue))
                return retValue;
            return null;
        }
        public ResourceInfoMetaData GetResourceInfoMetaData(string resourceInfoStr)
        {
            ResourceInfoMetaData retValue;
            if (mResourceInfoStrDic.TryGetValue(resourceInfoStr, out retValue))
                return retValue;
            return null;
        }

        private void RegResourceInfo(ResourceInfoMetaData metaData)
        {
            if(mResourceInfoStrDic.ContainsKey(metaData.ResourceInfoTypeStr))
                return;
            mResourceInfoStrDic[metaData.ResourceInfoTypeStr] = metaData;
            mResourceInfoTypeDic[metaData.ResourceInfoType] = metaData;
        }

        public void RegResourceInfo(Type resourceInfoType)
        {
            var atts = resourceInfoType.GetCustomAttributes(typeof(ResourceInfoAttribute), false);
            if (atts == null || atts.Length <= 0)
                return;

            var att = atts[0] as EditorCommon.Resources.ResourceInfoAttribute;
            var metaData = new ResourceInfoMetaData()
            {
                ResourceInfoTypeStr = att.ResourceInfoType,
                ResourceExts = att.ResourceExts,
                ResourceInfoType = resourceInfoType,
            };
            RegResourceInfo(metaData);
        }
        public void RegResourceInfo(System.Reflection.Assembly assembly)
        {
            if (assembly == null)
                return;

            foreach(var type in assembly.GetTypes())
            {
                RegResourceInfo(type);
            }
        }

        public void UnRegResourceInfo(Type resourceInfoType)
        {
            ResourceInfoMetaData data;
            if(mResourceInfoTypeDic.TryGetValue(resourceInfoType, out data))
            {
                mResourceInfoStrDic.Remove(data.ResourceInfoTypeStr);
                mResourceInfoTypeDic.Remove(data.ResourceInfoType);
            }
        }
        public void UnRegResourceInfo(string resourceTypeStr)
        {
            ResourceInfoMetaData data;
            if (mResourceInfoStrDic.TryGetValue(resourceTypeStr, out data))
            {
                mResourceInfoStrDic.Remove(data.ResourceInfoTypeStr);
                mResourceInfoTypeDic.Remove(data.ResourceInfoType);
            }
        }

        public async System.Threading.Tasks.Task<ResourceInfo> CreateResourceInfoAsync(string resourceTypeStr)
        {
            ResourceInfoMetaData data;
            if(mResourceInfoStrDic.TryGetValue(resourceTypeStr, out data))
            {
                return await EngineNS.CEngine.Instance.EventPoster.Post(()=>
                {
                    return System.Activator.CreateInstance(data.ResourceInfoType) as ResourceInfo;
                }, EngineNS.Thread.Async.EAsyncTarget.Main);
            }
            return null;
        }
        public ResourceInfo CreateResourceInfo(string resourceTypeStr)
        {
            ResourceInfoMetaData data;
            if (mResourceInfoStrDic.TryGetValue(resourceTypeStr, out data))
            {
                return System.Activator.CreateInstance(data.ResourceInfoType) as ResourceInfo;
            }
            return null;
        }

        Dictionary<string, ResourceInfo> mResourceInfoDic = new Dictionary<string, ResourceInfo>();
        public void ClearResourceInfoSnapshot()
        {
            foreach(var i in mResourceInfoDic.Values)
            {
                if (i != null)
                    i.Snapshot = null;
            }
        }
        public void RemoveFromResourceInfoDic(string absResInfoFileName)
        {
            mResourceInfoDic.Remove(absResInfoFileName);
        }
        public void SetToResourceInfoDic(string absResInfoFileName, ResourceInfo resInfo)
        {
            mResourceInfoDic[absResInfoFileName] = resInfo;
        }

        public async System.Threading.Tasks.Task<ResourceInfo> CreateResourceInfoFromFile(string absResInfoFileName, EditorCommon.Controls.ResourceBrowser.IContentControlHost parentBrowser, bool forceLoad = false)
        {
            ResourceInfo retInfo;
            if(!forceLoad)
            {
                if (mResourceInfoDic.TryGetValue(absResInfoFileName, out retInfo))
                    return retInfo;
            }

            var info = new CommonResourceInfo();
            if (await info.AsyncLoad(absResInfoFileName) == false)
                return null;
            retInfo = await CreateResourceInfoAsync(info.ResourceType);
            if(retInfo != null)
            {
                retInfo.ParentBrowser = parentBrowser;
                if(await retInfo.AsyncLoad(absResInfoFileName))
                {
                    mResourceInfoDic[absResInfoFileName] = retInfo;
                    return retInfo;
                }
            }
            return null;
        }

        public async System.Threading.Tasks.Task<ResourceInfo> CreateResourceInfoFromResource(string resourceAbsFile)
        {
            if (EngineNS.CEngine.Instance.FileManager.FileExists(resourceAbsFile + EditorCommon.Program.ResourceInfoExt))
                return await CreateResourceInfoFromFile(resourceAbsFile + EditorCommon.Program.ResourceInfoExt, null);
            if(EngineNS.CEngine.Instance.FileManager.DirectoryExists(resourceAbsFile))
            {
                // 文件夹
                ResourceInfoMetaData data;
                if (mResourceInfoStrDic.TryGetValue("Folder", out data))
                {
                    var retVal = await data.ResInfo.CreateResourceInfoFromResource(resourceAbsFile);
                    mResourceInfoDic[resourceAbsFile + EditorCommon.Program.ResourceInfoExt] = retVal;
                    return retVal;
                }
                return null;
            }
            var resInfoArray = mResourceInfoStrDic.ToArray();
            var ext = "." + EngineNS.CEngine.Instance.FileManager.GetFileExtension(resourceAbsFile);
            foreach (var data in resInfoArray)
            {
                if (data.Value.ResourceExts == null)
                    continue;

                if(data.Value.ResourceExts.Length > 0)
                {
                    foreach(var resExt in data.Value.ResourceExts)
                    {
                        if(resExt.Equals(ext, StringComparison.OrdinalIgnoreCase))
                        {
                            var retVal = await data.Value.ResInfo.CreateResourceInfoFromResource(resourceAbsFile);
                            mResourceInfoDic[resourceAbsFile + EditorCommon.Program.ResourceInfoExt] = retVal;
                            return retVal;
                        }
                    }
                }
                else
                {
                    var retVal = await data.Value.ResInfo.CreateResourceInfoFromResource(resourceAbsFile);
                    mResourceInfoDic[resourceAbsFile + EditorCommon.Program.ResourceInfoExt] = retVal;
                    return retVal;
                }
            }

            return null;
        }

        public bool CheckFileExtAvaliable(string[] files)
        {
            var resInfoArray = mResourceInfoStrDic.ToArray();
            foreach (var file in files)
            {
                var fileExt = "." + EngineNS.CEngine.Instance.FileManager.GetFileExtension(file);
                foreach(var resInfo in resInfoArray)
                {
                    if(resInfo.Value.ResourceExts != null)
                    {
                        foreach(var ext in resInfo.Value.ResourceExts)
                        {
                            if (fileExt.Equals(ext, StringComparison.OrdinalIgnoreCase))
                                return true;
                        }
                    }
                }
            }

            return false;
        }

        public List<ResourceInfoMetaData> GetConfirmTypeResources(Type infoType)
        {
            List<ResourceInfoMetaData> retValue = new List<ResourceInfoMetaData>();
            var resInfoArray = mResourceInfoStrDic.ToArray();
            foreach(var info in resInfoArray)
            {
                if(info.Value.ResourceInfoType.GetInterface(infoType.FullName) != null)
                {
                    retValue.Add(info.Value);
                }
            }

            return retValue;
        }

        public List<ResourceInfoMetaData> GetAllResourceInfoDatas()
        {
            return mResourceInfoStrDic.Values.ToList();
        }
    }
}
