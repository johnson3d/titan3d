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
            if (InitGPU(engine.Config.AdaperId, engine.Config.RHIType, IntPtr.Zero, engine.Config.HasDebugLayer) == false)
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
