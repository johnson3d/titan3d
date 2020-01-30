using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EngineNS.UISystem
{
    [EngineNS.Editor.Editor_MacrossClass(ECSType.Client, EngineNS.Editor.Editor_MacrossClassAttribute.enMacrossType.Useable)]
    public partial class UIManager
    {
        UISystemConfig mConfig = new UISystemConfig();
        public UISystemConfig Config => mConfig;

        public void Cleanup()
        {
            lock(UserUIs)
            {
                UserUIs.Clear();
            }
        }

        public struct CKeyName
        {
            public RName RName;
            public string KeyName;
            public CKeyName(RName rName, string keyName)
            {
                RName = rName;
                KeyName = keyName;
            }

            public bool Equals(CKeyName tag)
            {
                return (RName.Equals(tag.RName) && KeyName == tag.KeyName);
            }
            public override bool Equals(object obj)
            {
                var tag = (CKeyName)obj;
                return (RName.Equals(tag.RName) && KeyName == tag.KeyName);
            }
            public override int GetHashCode()
            {
                return (RName.Name + RName.RNameType.ToString() + KeyName).GetHashCode();
            }
            public class EqualityComparer : IEqualityComparer<CKeyName>
            {
                public bool Equals(CKeyName x, CKeyName y)
                {
                    return x.Equals(y);
                }

                public int GetHashCode(CKeyName obj)
                {
                    return obj.GetHashCode();
                }
            }
        }

        public Dictionary<CKeyName, UIElement> UserUIs
        {
            get;
        } = new Dictionary<CKeyName, UIElement>(new CKeyName.EqualityComparer());
        public bool RemoveUIFromDic(RName name, string keyName)
        {
            lock(UserUIs)
            {
                var key = new CKeyName(name, keyName);
                return UserUIs.Remove(key);
            }
        }
        public void RemoveUIFromDic(RName name)
        {
            List<CKeyName> keys = new List<CKeyName>(UserUIs.Count);
            lock(UserUIs)
            {
                using (var ite = UserUIs.GetEnumerator())
                {
                    while(ite.MoveNext())
                    {
                        var cur = ite.Current;
                        if (cur.Key.RName.Equals(name))
                            keys.Add(cur.Key);
                    }
                }
                for (int i = 0; i < keys.Count; i++)
                {
                    UserUIs.Remove(keys[i]);
                }
            }
        }
        [EngineNS.Editor.MacrossMember(EngineNS.Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public async Task<UIElement> GetUICloneAsync(
            [EngineNS.Editor.Editor_RNameMacrossType(typeof(EngineNS.UISystem.UIElement))]
            RName name, string keyName)
        {
            var ui = await GetUIAsync(name, keyName);
            var retUI = Rtti.RttiHelper.CreateInstance(ui.GetType()) as UIElement;
            var retUIArg =Rtti.RttiHelper.CreateInstance(ui.Initializer.GetType()) as UIElementInitializer; 
            await retUI.Initialize(EngineNS.CEngine.Instance.RenderContext, retUIArg);
            await retUI.CopyFromTemplate(EngineNS.CEngine.Instance.RenderContext, ui);
            return retUI;
        }
        public async Task RefreshUI(RName name, params string[] exclusiveKeyNames)
        {
            var rc = CEngine.Instance.RenderContext;
            List<UIElement> needRefreshUIs = new List<UIElement>(UserUIs.Count);
            lock(UserUIs)
            {
                using (var ite = UserUIs.GetEnumerator())
                {
                    while (ite.MoveNext())
                    {
                        var cur = ite.Current;
                        bool bFind = false;
                        for(int i=0; i<exclusiveKeyNames.Length; i++)
                        {
                            if (cur.Key.KeyName == exclusiveKeyNames[i])
                            {
                                bFind = true;
                                break;
                            }
                        }
                        if (bFind)
                            continue;
                        if (cur.Key.RName.Equals(name))
                        {
                            needRefreshUIs.Add(cur.Value);
                        }
                    }
                }
            }

            for (int i=0; i<needRefreshUIs.Count; i++)
            {
                await needRefreshUIs[i].Initialize(rc, needRefreshUIs[i].Initializer);
            }
        }
        [EngineNS.Editor.MacrossMember(EngineNS.Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public async Task<UIElement> GetUIAsync([EngineNS.Editor.Editor_RNameMacrossType(typeof(EngineNS.UISystem.UIElement))]RName name,
                                                            string keyName = "__default", bool forceLoad = false)
        {
            if (name == null || name.IsExtension(CEngineDesc.UIExtension) == false)
                return null;
            if (name.RNameType != RName.enRNameType.Game)
                throw new InvalidOperationException("暂不支持非游戏UI");
            if (string.IsNullOrEmpty(keyName))
                keyName = "__default";
            var rc = CEngine.Instance.RenderContext;
            var key = new CKeyName(name, keyName);
            UIElement result;

            bool bFound = true;
            lock (UserUIs)
            {
                if (UserUIs.TryGetValue(key, out result) == false)
                {
                    var nameSpace = name.RelativePath().Replace("/", ".");
                    var typeName = nameSpace + name.PureName();
                    var resultType = EngineNS.CEngine.Instance.MacrossDataManager.MacrossScripAssembly.GetType(typeName);
                    if (resultType == null)
                        return null;
                    result = System.Activator.CreateInstance(resultType) as UIElement;
                    bFound = false;
                    UserUIs.Add(key, result);
                }
            }

            if (bFound)
            {
                if (forceLoad)
                {
                    var atts = result.GetType().GetCustomAttributes(typeof(Editor_UIControlInitAttribute), false);
                    var att = atts[0] as Editor_UIControlInitAttribute;
                    var initType = att.InitializerType;
                    var init = System.Activator.CreateInstance(initType) as UIElementInitializer;
                    await result.Initialize(rc, init);
                }
            }
            else
            {
                var atts = result.GetType().GetCustomAttributes(typeof(Editor_UIControlInitAttribute), false);
                var att = atts[0] as Editor_UIControlInitAttribute;
                var initType = att.InitializerType;
                var init = System.Activator.CreateInstance(initType) as UIElementInitializer;
                await result.Initialize(rc, init);
            }
            return result;
        }

        [EngineNS.Editor.MacrossMember(EngineNS.Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        [return: EngineNS.Editor.Editor_TypeChangeWithParam(0)]
        public async Task<UIElement> GetUIAsyncAs(
                    Type elemType,
                    [EngineNS.Editor.Editor_RNameMacrossType(typeof(EngineNS.UISystem.UIElement))]
                    RName name,
                    string keyName = "__default", bool forceLoad = false)
        {
            return await GetUIAsync(name, keyName, forceLoad);
        }
        #region UIHosts

        Dictionary<string, UIHost> mUIHostList = new Dictionary<string, UIHost>();
        public static Profiler.TimeScope ScopeTickAutoMember = Profiler.TimeScopeManager.GetTimeScope(typeof(UIManager), nameof(Tick));
        public void Tick(CCommandList cmd)
        {
            ScopeTickAutoMember.Begin();
            var rc = EngineNS.CEngine.Instance.RenderContext;
            using (var ite = mUIHostList.GetEnumerator())
            {
                while (ite.MoveNext())
                {
                    ite.Current.Value.Commit(cmd);
                }
            }
            ScopeTickAutoMember.End();
        }

        public async Task<UIHost> RegisterHost(string name)
        {
            UIHost outVal;
            if(!mUIHostList.TryGetValue(name, out outVal))
            {
                var init = new UIElementInitializer();
                outVal = new UIHost();
                await outVal.Initialize(EngineNS.CEngine.Instance.RenderContext, init);
                mUIHostList[name] = outVal;
            }
            return outVal;
        }

        #endregion
    }

    public class UIManagerProcesser : CEngineAutoMemberProcessor
    {
        public override async Task<object> CreateObject()
        {
            await EngineNS.Thread.AsyncDummyClass.DummyFunc();
            return new UIManager();
        }
        public override void Tick(object obj)
        {
            var uiManager = obj as UIManager;
            uiManager?.Tick(CEngine.Instance.RenderContext.ImmCommandList);
        }
        public override void Cleanup(object obj)
        {
            var uiManager = obj as UIManager;
            uiManager?.Cleanup();
        }
        public override void ClearCache(object obj)
        {
            var uiManager = obj as UIManager;
            uiManager.Cleanup();
        }
        public override async Task<bool> OnGameStart(object obj)
        {
            await EngineNS.Thread.AsyncDummyClass.DummyFunc();
            var uiManager = obj as UIManager;
            uiManager.Cleanup();
            return true;
        }
    }
}

namespace EngineNS
{
    public partial class CEngine
    {
        [CEngineAutoMemberAttribute(typeof(EngineNS.UISystem.UIManagerProcesser))]
        [EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.PropReadOnly)]
        public UISystem.UIManager UIManager
        {
            get;
            set;
        }
    }
}