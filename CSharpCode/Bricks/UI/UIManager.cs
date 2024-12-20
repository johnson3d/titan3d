using EngineNS;
using EngineNS.EGui;
using EngineNS.Macross;
using EngineNS.UI.Controls;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Threading.Tasks;

namespace EngineNS.UI
{
    [Rtti.Meta]
    public partial class TtUIAssistFunctions
    {
        [Rtti.Meta(MacrossDisplayPath = new string[] { "UI", "UIManager" })]
        public static TtUIManager UIManager
        {
            get => TtEngine.Instance.UIManager;
        }
    }

    public partial class TtUIManager : TtModule<TtEngine>
    {
        TtUIConfig mConfig = new TtUIConfig();
        public TtUIConfig Config => mConfig;
        public TtUIManager()
        {
            UIManagerConstruct_Msg();
        }

        public override void Cleanup(TtEngine host)
        {
            ClearTemplates();
            mBindObjects.Clear();
            mBindingTargets.Clear();
            base.Cleanup(host);
        }

        public struct UIKeyName
        {
            public RName RName;
            public string KeyName;
            public UIKeyName(RName rName, string keyName)
            {
                RName = rName;
                KeyName = keyName;
            }
            public bool Equals(UIKeyName other)
            {
                return RName.Equals(other.RName) && (KeyName == other.KeyName);
            }
            public override bool Equals([NotNullWhen(true)] object obj)
            {
                var tag = (UIKeyName)obj;
                return Equals(tag);
            }
            public override int GetHashCode()
            {
                return (RName.Name + RName.RNameType.ToString() + KeyName).GetHashCode();
            }
            public class EqualityComparer : IEqualityComparer<UIKeyName>
            {
                public bool Equals(UIKeyName x, UIKeyName y)
                {
                    return x.Equals(y);
                }

                public int GetHashCode(UIKeyName obj)
                {
                    return obj.GetHashCode();
                }
            }
        }
        List<TtUIHost> mUserUIList = new List<TtUIHost>();
        public void AddActivedUI(TtUIHost userUI)
        {
            mUserUIList.Add(userUI);
        }
        public void RemoveActivedUI(TtUIHost userUI)
        {
            mUserUIList.Remove(userUI);
        }

        Dictionary<UIKeyName, TtUIHost> mUserUIs = new Dictionary<UIKeyName, TtUIHost>(new UIKeyName.EqualityComparer());
        public void AddUI(RName fileName, string key, TtUIHost ui)
        {
            lock(mUserUIs)
            {
                var keyName = new UIKeyName(fileName, key);
                AddActivedUI(ui);
                mUserUIs[keyName] = ui;
            }
        }
        public void BringToTop(in UIKeyName key)
        {
            lock(mUserUIs)
            {
                if(mUserUIs.TryGetValue(key, out var ui))
                {
                    RemoveActivedUI(ui);
                    AddActivedUI(ui);
                }
            }
        }
        public bool RemoveUI(RName name, string keyName)
        {
            lock (mUserUIs)
            {
                var key = new UIKeyName(name, keyName);
                if(mUserUIs.TryGetValue(key, out var ui))
                {
                    RemoveActivedUI(ui);
                    return mUserUIs.Remove(key);
                }
                return false;
            }
        }
        public void RemoveUI(RName name)
        {
            List<UIKeyName> keys = new List<UIKeyName>(mUserUIs.Count);
            lock (mUserUIs)
            {
                using (var ite = mUserUIs.GetEnumerator())
                {
                    while (ite.MoveNext())
                    {
                        var cur = ite.Current;
                        if (cur.Key.RName.Equals(name))
                            keys.Add(cur.Key);
                    }
                }
                for (int i = 0; i < keys.Count; i++)
                {
                    if(mUserUIs.TryGetValue(keys[i], out var ui))
                    {
                        RemoveActivedUI(ui);
                        mUserUIs.Remove(keys[i]);
                    }
                }
            }
        }

        public TtUIHost GetFirstIntersectHost(in Ray ray)
        {
            float minDistance = float.MaxValue;
            TtUIHost intersectHost = null;
            for(int i=mUserUIList.Count - 1; i>=0; i--)
            {
                var ui = mUserUIList[i];
                float distance;
                if(Ray.Intersects(ray, ui.BoundingBox, out distance))
                {
                    if(ui.Has3DElement)
                    {
                        var end = ray.Position + ray.Direction * 100.0f;
                        VHitResult result = new VHitResult();
                        if(ui.OnLineCheckTriangle(ray.Position, end, ref result))
                        {
                            distance = (float)(result.Position - ray.Position).LengthSquared();
                            if(distance < minDistance)
                            {
                                minDistance = distance;
                                intersectHost = ui;
                            }
                        }
                    }
                    else
                    {
                        if(distance < minDistance)
                        {
                            minDistance = distance;
                            intersectHost = ui;
                        }
                    }
                }
            }

            return intersectHost;
        }

        List<TtUIElement> mTickUIElements = new List<TtUIElement>();
        public void RegisterTickElement(TtUIElement element)
        {
            if (mTickUIElements.IndexOf(element) == -1)
                mTickUIElements.Add(element);
        }
        public void UnregisterTickElement(TtUIElement element)
        {
            mTickUIElements.Remove(element);
        }
        public override void TickLogic(TtEngine host)
        {
            for(int i=mUserUIList.Count - 1; i>=0; i--)
            {
                TtEngine.Instance.TaskCollector.AddWaitTask(mUserUIList[i].BuildMesh());
            }

            var elapsedSecond = TtEngine.Instance.ElapsedSecond;
            for (int i=mTickUIElements.Count - 1; i >= 0; i--)
            {
                mTickUIElements[i].Tick(elapsedSecond);
            }

            TickTimeline(elapsedSecond);
        }

        public void Save(RName name, TtUIElement element)
        {
            if (element == null)
                return;
            var typeStr = Rtti.TtTypeDescManager.Instance.GetTypeStringFromType(element.GetType());
            using (var xnd = new IO.TtXndHolder(typeStr, 0, 0))
            {
                using (var attr = xnd.NewAttribute("UI", 0, 0))
                {
                    using (var ar = attr.GetWriter(512))
                    {
                        ar.Write(element);
                    }
                    xnd.RootNode.AddAttribute(attr);
                }
                var fileName = name.Address + "/" + name.PureName + name.ExtName;
                xnd.SaveXnd(fileName);
                TtEngine.Instance.SourceControlModule.AddFile(fileName);
            }
        }
        [Rtti.Meta]
        public TtUIElement Load(
            [RName.PGRName(FilterExts = TtUIAsset.AssetExt)]
            RName name)
        {
            using (var xnd = IO.TtXndHolder.LoadXnd(name.Address + "/" + name.PureName + name.ExtName))
            {
                if (xnd == null)
                    return null;

                var attr = xnd.RootNode.TryGetAttribute("UI");
                if (attr.NativePointer == IntPtr.Zero)
                    return null;

                using(var ar = attr.GetReader(null))
                {
                    TtUIElement element = null;
                    try
                    {
                        ar.ReadObject(out element);

                        element.MacrossGetter = TtMacrossGetter<TtUIMacrossBase>.NewInstance();
                        element.MacrossGetter.Name = name;
                        var mc = element.MacrossGetter.Get();
                        if(mc != null)
                        {
                            mc.HostElement = element;
                            mc.Initialize();
                        }
                    }
                    catch(Exception ex)
                    {
                        Profiler.Log.WriteException(ex);
                    }

                    return element;
                }
            }
        }

        private void Btn_DeviceDown(object sender, TtRoutedEventArgs args)
        {
            throw new NotImplementedException();
        }

        Dictionary<Bind.BindPropertyAttribute, List<Bind.IBindableObject>> mBindObjects = new Dictionary<Bind.BindPropertyAttribute, List<Bind.IBindableObject>>();
        Dictionary<Bind.BindPropertyAttribute, List<Bind.IBindableObject>> mBindingTargets = new Dictionary<Bind.BindPropertyAttribute, List<Bind.IBindableObject>>();

        public void ClearBindObjects(Bind.BindPropertyAttribute attr)
        {
            if(mBindObjects.ContainsKey(attr))
                mBindObjects.Remove(attr);
        }
        public void ClearBindingTargets(Bind.BindPropertyAttribute attr)
        {
            if (mBindingTargets.ContainsKey(attr))
                mBindingTargets.Remove(attr);
        }
        public bool GetBindObjects(Bind.BindPropertyAttribute attr, out List<Bind.IBindableObject> list)
        {
            return mBindObjects.TryGetValue(attr, out list);
        }
        public void SetBindObjects(Bind.BindPropertyAttribute attr, List<Bind.IBindableObject> list)
        {
            mBindObjects[attr] = list;
        }
        public int BindObjectsCount(Bind.BindPropertyAttribute attr)
        {
            List<Bind.IBindableObject> bindObjects;
            if (!mBindObjects.TryGetValue(attr, out bindObjects))
                return 0;
            return bindObjects.Count;
        }
        public bool GetBindingTargets(Bind.BindPropertyAttribute attr, out List<Bind.IBindableObject> list)
        {
            return mBindingTargets.TryGetValue(attr, out list);
        }
        public void SetBindingTargets(Bind.BindPropertyAttribute attr, List<Bind.IBindableObject> list)
        {
            mBindingTargets[attr] = list;
        }
        public int BindingTargetsCount(Bind.BindPropertyAttribute attr)
        {
            List<Bind.IBindableObject> bindTargets;
            if (!mBindingTargets.TryGetValue(attr, out bindTargets))
                return 0;
            return bindTargets.Count;
        }
    }
}

namespace EngineNS
{
    public partial class TtEngine
    {
        UI.TtUIManager mUIManager;
        [Rtti.Meta]
        public UI.TtUIManager UIManager 
        { 
            get
            {
                if (mUIManager == null)
                    mUIManager = new UI.TtUIManager();
                return mUIManager;
            }
        }
    }
}



#if TitanEngine_AutoGen_Macross
#region TitanEngine_AutoGen_Macross


namespace EngineNS.UI
{
	partial class TtUIManager
	{
		private static EngineNS.Macross.TtMacrossBreak macross_break_Load_2037383663 = new EngineNS.Macross.TtMacrossBreak("EngineNS.UI.TtUIManager->TtUIElement Load(RName name)");
		public unsafe TtUIElement macross_Load (string nodeName, RName name) 
		{
			using(var stackframe = EngineNS.Macross.TtMacrossStackTracer.CurrentFrame)
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":name", name);
				}
			}
			var _return_value = Load(name);
			macross_break_Load_2037383663.TryBreak();
			return _return_value;
		}
	}
}
#endregion//TitanEngine_AutoGen_Macross
#endif//TitanEngine_AutoGen_Macross