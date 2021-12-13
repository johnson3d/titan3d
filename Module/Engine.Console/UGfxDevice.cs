using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EngineNS.Graphics.Pipeline
{
    public class UGfxDeviceConsole : UGfxDevice
    {
        public override async System.Threading.Tasks.Task<bool> Initialize(UEngine engine)
        {
            await EngineNS.Thread.AsyncDummyClass.DummyFunc();
            if (await InitGPU(engine, 0, ERHIType.RHT_VirtualDevice, IntPtr.Zero, engine.Config.HasDebugLayer) == false)
            {
                return false;
            }
            return true;
        }
        public override void Tick(UEngine engine)
        {

        }
    }
}
