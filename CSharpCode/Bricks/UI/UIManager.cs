using EngineNS;
using EngineNS.EGui;
using EngineNS.UI.Controls;
using System;
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
        public bool RemoveUIFromDic(RName name, string keyName)
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
        public void RemoveUIFromDic(RName name)
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
        public TtUIHost GetFirstPointAtHost(in Point2f pt)
        {
            for(int i=mUserUIList.Count - 1; i >=0; i--)
            {
                var ui = mUserUIList[i];
                if (ui.IsPointIn(in pt))
                    return ui;
            }
            return null;
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