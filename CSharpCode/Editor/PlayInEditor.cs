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
        public virtual async System.Threading.Tasks.Task<bool> StartPlayInEditor(RName main)
        {
            if (this.GameInstance != null)
                return false;
            this.GameInstance = new GamePlay.UGameBase();
            this.GameInstance.McObject.Name = main;

            return await this.GameInstance.BeginPlay();
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
        }
        private WeakReference EndPlayInEditor_Impl()
        {
            if (this.GameInstance == null)
                return null;

            this.GameInstance.BeginDestroy();
            var wr = new WeakReference(this.GameInstance);
            this.GameInstance = null;

            return wr;
        }
    }
}