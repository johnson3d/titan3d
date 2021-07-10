using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Editor
{
    public partial class UPIEModule : UModule<UEngine>
    {
        public override void Tick(UEngine engine)
        {
            if (engine.GameInstance == null)
                return;
            engine.GameInstance.Tick(engine.ElapseTickCount);
        }
        public override void Cleanup(UEngine engine)
        {
            if (engine.GameInstance == null)
                return;

            engine.GameInstance.BeginDestroy();
            engine.GameInstance = null;
        }
    }
}

namespace EngineNS
{
    public partial class UEngine
    {
        public Editor.UPIEModule PIEModule { get; } = new Editor.UPIEModule();
        public virtual async System.Threading.Tasks.Task<bool> StartPlayInEditor(Graphics.Pipeline.USlateApplication application, Type rpType, RName main)
        {
            if (this.GameInstance != null)
                return false;

            var root = UEngine.Instance.FileManager.GetRoot(IO.FileManager.ERootDir.Current);
            UEngine.Instance.MacrossModule.ReloadAssembly(root + "/net5.0/GameProject.dll");

            this.GameInstance = new GamePlay.UGameBase();
            this.GameInstance.McObject.Name = main;

            var ret = await this.GameInstance.BeginPlay();
            var igame = this.GameInstance.McObject.Get();
            igame.WorldViewportSlate.Title = $"Game:{main.Name}";
            UEngine.Instance.TickableManager.AddTickable(igame);
            return ret;
        }
        public void EndPlayInEditor()
        {
            var wr =  EndPlayInEditor_Impl();
            if (wr == null)
                return;
            for (int i = 0; wr.IsAlive && (i < 10); i++)
            {
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }

            if (wr.IsAlive)
            {
                Profiler.Log.WriteLine(Profiler.ELogTag.Warning, "Core", "EndPIE: GameInstance is alive");
            }
            else
            {
                GC.Collect();
                GC.WaitForPendingFinalizers();

                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
        }
        private WeakReference EndPlayInEditor_Impl()
        {
            if (this.GameInstance == null)
                return null;

            var igame = this.GameInstance.McObject.Get();
            UEngine.Instance?.TickableManager.RemoveTickable(igame);
            this.GameInstance.BeginDestroy();
            var wr = new WeakReference(this.GameInstance);
            this.GameInstance = null;

            return wr;
        }
    }
}