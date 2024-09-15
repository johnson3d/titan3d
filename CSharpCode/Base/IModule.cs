using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS
{
    public class TtModuleHost<THost> where THost : class
    {
        protected List<TtModule<THost>> mModules = new List<TtModule<THost>>();
        protected virtual THost GetHost()
        {
            return null;
        }
        protected void GatherModules()
        {
            mModules.Clear();
            var props = this.GetType().GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Static);
            foreach (var i in props)
            {
                if (i.PropertyType.IsSubclassOf(typeof(TtModule<THost>)))
                {
                    var module = i.GetValue(this) as TtModule<THost>;
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
                if (i.IsModuleRun(TtEngine.Instance.PlayMode) == false)
                    continue;

                var t1 = Support.TtTime.HighPrecision_GetTickCount();
                if (false == await i.Initialize(host))
                {
                    Profiler.Log.WriteLine<Profiler.TtCoreGategory>(Profiler.ELogTag.Error, $"Module {i.GetType()} init failed");
                }
                var t2 = Support.TtTime.HighPrecision_GetTickCount();

                Profiler.Log.WriteLine<Profiler.TtCoreGategory>(Profiler.ELogTag.Info, $"InitializeModules {i.GetType().FullName}:{(t2 - t1) / 1000} ms");
            }

            foreach (var i in mModules)
            {
                if (i.IsModuleRun(TtEngine.Instance.PlayMode) == false)
                    continue;

                var t1 = Support.TtTime.HighPrecision_GetTickCount();
                if (false == await i.PostInitialize(host))
                {
                    Profiler.Log.WriteLine<Profiler.TtCoreGategory>(Profiler.ELogTag.Error, $"Module {i.GetType()} PostInit failed");
                }
                var t2 = Support.TtTime.HighPrecision_GetTickCount();

                Profiler.Log.WriteLine<Profiler.TtCoreGategory>(Profiler.ELogTag.Info, $"PostInitializeModules {i.GetType().FullName}:{(t2 - t1) / 1000} ms");
            }
        }
        protected void TickModules()
        {
            var host = GetHost();
            foreach (var i in mModules)
            {
                if (i.IsModuleRun(TtEngine.Instance.PlayMode) == false)
                    continue;
                i.TickModule(host);
            }
        }
        protected void TickLogicModules()
        {
            var host = GetHost();
            foreach (var i in mModules)
            {
                if (i.IsModuleRun(TtEngine.Instance.PlayMode) == false)
                    continue;
                i.TickLogic(host);
            }
        }
        protected void TickRenderModules()
        {
            var host = GetHost();
            foreach (var i in mModules)
            {
                if (i.IsModuleRun(TtEngine.Instance.PlayMode) == false)
                    continue;
                i.TickRender(host);
            }
        }
        protected void EndFrameModules()
        {
            var host = GetHost();
            foreach (var i in mModules)
            {
                if (i.IsModuleRun(TtEngine.Instance.PlayMode) == false)
                    continue;
                i.EndFrame(host);
            }
        }
        protected void CleanupModules()
        {
            var host = GetHost();
            foreach (var i in mModules)
            {
                if (i.IsModuleRun(TtEngine.Instance.PlayMode) == false)
                    continue;
                i.Cleanup(host);
            }
        }
    }
    public class TtModule<THost> where THost : class
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
            return 1;
        }
        public virtual async System.Threading.Tasks.Task<bool> Initialize(THost host)
        {
            await Thread.TtAsyncDummyClass.DummyFunc();
            return true;
        }
        public virtual async System.Threading.Tasks.Task<bool> PostInitialize(THost host)
        {
            await Thread.TtAsyncDummyClass.DummyFunc();
            return true;
        }
        public virtual void TickModule(THost host)
        {

        }
        public virtual void TickLogic(THost host)
        {
        }

        public virtual void TickRender(THost host)
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
