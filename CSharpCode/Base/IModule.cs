using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS
{
    public class UModuleHost<THost> where THost : class
    {
        protected List<UModule<THost>> mModules = new List<UModule<THost>>();
        protected virtual THost GetHost()
        {
            return null;
        }
        protected void GatherModules()
        {
            mModules.Clear();
            var props = this.GetType().GetProperties();
            foreach (var i in props)
            {
                if (i.PropertyType.IsSubclassOf(typeof(UModule<THost>)))
                {
                    var module = i.GetValue(this) as UModule<THost>;
                    if (module == null)
                        continue;
                    mModules.Add(module);
                }
            }
            mModules.Sort((x, y) => x.GetOrder().CompareTo(y.GetOrder()));
        }
        protected async System.Threading.Tasks.Task InitializeModules()
        {
            var host = GetHost();
            foreach (var i in mModules)
            {
                if (i.IsModuleRun(UEngine.Instance.PlayMode) == false)
                    continue;
                await i.Initialize(host);
            }
        }
        protected void TickModules()
        {
            var host = GetHost();
            foreach (var i in mModules)
            {
                if (i.IsModuleRun(UEngine.Instance.PlayMode) == false)
                    continue;
                i.Tick(host);
            }
        }
        protected void EndFrameModules()
        {
            var host = GetHost();
            foreach (var i in mModules)
            {
                if (i.IsModuleRun(UEngine.Instance.PlayMode) == false)
                    continue;
                i.EndFrame(host);
            }
        }
        protected void CleanupModules()
        {
            var host = GetHost();
            foreach (var i in mModules)
            {
                if (i.IsModuleRun(UEngine.Instance.PlayMode) == false)
                    continue;
                i.Cleanup(host);
            }
        }
    }
    public class UModule<THost> where THost : class
    {
        [Flags]
        public enum EModuleFlags : uint
        {
            RunClient = 1 << EPlayMode.Game,
            RunServer = 1 << EPlayMode.Server,
            RunEditor = 1 << EPlayMode.Editor,
            RunCook = 1 << EPlayMode.Cook,

            FullStyles = 0xFFFFFFFF,
        }
        public virtual EModuleFlags ModuleFlags
        {
            get
            {
                return EModuleFlags.FullStyles;
            }
        }
        public bool IsModuleRun(EPlayMode mode)
        {
            return ((int)ModuleFlags & (1 << (int)mode)) != 0;
        }
        public virtual int GetOrder()
        {
            return 0;
        }
        public virtual async System.Threading.Tasks.Task<bool> Initialize(THost host)
        {
            await Thread.AsyncDummyClass.DummyFunc();
            return true;
        }
        public virtual void Tick(THost host)
        {

        }
        public virtual void EndFrame(THost host)
        {

        }
        public virtual void Cleanup(THost host)
        {

        }
    }
}
