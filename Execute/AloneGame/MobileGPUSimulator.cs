using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AloneGame
{
    class MobileGPUSimulator
    {
        public static MobileGPUSimulator Instance = new MobileGPUSimulator();
        EngineNS.CRenderContextCaps MaliG76 = new EngineNS.CRenderContextCaps();//es3.2
        EngineNS.CRenderContextCaps MaliT860MP2 = new EngineNS.CRenderContextCaps();//es3.1
        EngineNS.CRenderContextCaps Adreno540 = new EngineNS.CRenderContextCaps();//es3.2
        EngineNS.CRenderContextCaps Adreno330 = new EngineNS.CRenderContextCaps();//es3.0

        public string Gpu = "Adreno540";
        public MobileGPUSimulator()
        {
            MaliG76.SetDefault();
            MaliG76.MaxVertexShaderStorageBlocks = 0;

            MaliT860MP2.SetDefault();
            MaliT860MP2.MaxVertexShaderStorageBlocks = 0;
            MaliT860MP2.ShaderModel = 4;
            Adreno330.SupportFloatRT = 0;
            Adreno330.SupportHalfRT = 0;

            Adreno540.SetDefault();

            Adreno330.SetDefault();
            Adreno330.MaxVertexShaderStorageBlocks = 0;
            Adreno330.ShaderModel = 3;
            Adreno330.SupportFloatRT = 0;
            Adreno330.SupportHalfRT = 0;
        }
        public EngineNS.CRenderContextCaps GetGPUCaps(string gpu)
        {
            switch (gpu)
            {
                case "MaliG76":
                    return MaliG76;
                case "MaliT860MP2":
                    return MaliT860MP2;
                case "Adreno540":
                    return Adreno540;
                case "Adreno330":
                    return Adreno330;
                default:
                    return new EngineNS.CRenderContextCaps();
            }
        }
        public void SimulateGPU(string gpu)
        {
            var rc = EngineNS.CEngine.Instance.RenderContext;
            unsafe
            {
                switch (gpu)
                {
                    case "MaliG76":
                        fixed (EngineNS.CRenderContextCaps* p = &MaliG76)
                        {//有cs，无ssbo in vs，sm5
                            rc.UnsafeSetRenderContextCaps(p);
                        }
                        break;
                    case "MaliT860MP2":
                        fixed (EngineNS.CRenderContextCaps* p = &MaliT860MP2)
                        {//有cs，无ssbo in vs，sm4
                            rc.UnsafeSetRenderContextCaps(p);
                        }
                        break;
                    case "Adreno540":
                        fixed (EngineNS.CRenderContextCaps* p = &Adreno540)
                        {//有cs，有ssbo in vs，sm5
                            rc.UnsafeSetRenderContextCaps(p);
                        }
                        break;
                    case "Adreno330":
                        fixed (EngineNS.CRenderContextCaps* p = &Adreno330)
                        {//无cs，无ssbo in vs，sm3
                            rc.UnsafeSetRenderContextCaps(p);
                        }
                        break;
                }
            }
        }
    }
}
