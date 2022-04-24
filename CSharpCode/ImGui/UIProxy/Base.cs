using System;
using System.Collections.Generic;
using System.Text;
using System.Dynamic;

namespace EngineNS.EGui.UIProxy
{
    public interface IUIProxyBase
    {
        void Cleanup();
        System.Threading.Tasks.Task<bool> Initialize();
        bool OnDraw(in ImDrawList drawList, in Support.UAnyPointer drawData);
    }
    public class UIManager : UModule<UEngine>
    {
        Dictionary<string, IUIProxyBase> mDic = new Dictionary<string, IUIProxyBase>();

        public int Count => mDic.Count;

        public IUIProxyBase this[string key]
        {
            get
            {
                IUIProxyBase item = null;
                mDic.TryGetValue(key, out item);
                return item;
            }
            set
            {
                lock(mDic)
                {
                    mDic[key] = value;
                }
            }
        }

        public override void Cleanup(UEngine host)
        {
            foreach(var item in mDic.Values)
            {
                item.Cleanup();
            }
            mDic.Clear();
        }
    }
}

namespace EngineNS
{
    public partial class UEngine
    {
        public EGui.UIProxy.UIManager UIManager { get; } = new EGui.UIProxy.UIManager();
    }
}