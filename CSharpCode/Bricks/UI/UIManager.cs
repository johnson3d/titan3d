using EngineNS;
using EngineNS.EGui;
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
    public partial class TtUIManager : UModule<UEngine>
    {
        TtUIConfig mConfig = new TtUIConfig();
        public TtUIConfig Config => mConfig;
        SizeF mScreenSize;
        public SizeF ScreenSize
        {
            get => mScreenSize;
            set
            {
                mScreenSize = value;
                if (mScreenSpaceUIHost != null)
                    mScreenSpaceUIHost.WindowSize = value;
            }
        }

        public TtUIManager()
        {
            UIManagerConstruct_Msg();
            InitSystemDefaultTemplates();
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

        TtUIHost mScreenSpaceUIHost;
        public TtUIHost ScreenSpaceUIHost
        {
            get => mScreenSpaceUIHost;
            set
            {
                mScreenSpaceUIHost = value;
                if(mScreenSpaceUIHost != null)
                {
                    mScreenSpaceUIHost.WindowSize = ScreenSize;
                }
            }
        }
        List<TtUIHost> mUserUIList = new List<TtUIHost>();
        Dictionary<UIKeyName, TtUIHost> mUserUIs = new Dictionary<UIKeyName, TtUIHost>(new UIKeyName.EqualityComparer());
        public void AddUI(RName fileName, string key, TtUIHost ui)
        {
            lock(mUserUIs)
            {
                var keyName = new UIKeyName(fileName, key);
                mUserUIList.Add(ui);
                mUserUIs[keyName] = ui;
            }
        }
        public void BringToTop(in UIKeyName key)
        {
            lock(mUserUIs)
            {
                if(mUserUIs.TryGetValue(key, out var ui))
                {
                    mUserUIList.Remove(ui);
                    mUserUIList.Add(ui);
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
                    mUserUIList.Remove(ui);
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
                        mUserUIList.Remove(ui);
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
                            distance = (result.Position - ray.Position).LengthSquared();
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
        public override void TickLogic(UEngine host)
        {
            for(int i=mUserUIList.Count - 1; i>=0; i--)
            {
                _ = mUserUIList[i].BuildMesh();
            }

            var elapsedSecond = UEngine.Instance.ElapsedSecond;
            for (int i=mTickUIElements.Count - 1; i >= 0; i--)
            {
                mTickUIElements[i].Tick(elapsedSecond);
            }
        }

        public void Save(RName name, TtUIElement element)
        {
            if (element == null)
                return;
            var typeStr = Rtti.UTypeDescManager.Instance.GetTypeStringFromType(element.GetType());
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
                xnd.SaveXnd(name.Address);
            }
        }
        public TtUIElement Load(RName name)
        {
            using (var xnd = IO.TtXndHolder.LoadXnd(name.Address))
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
                    }
                    catch(Exception ex)
                    {
                        Profiler.Log.WriteException(ex);
                    }
                    return element;
                }
            }
        }
    }
}

namespace EngineNS
{
    public partial class UEngine
    {
        UI.TtUIManager mUIManager;
        internal UI.TtUIManager UIManager 
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