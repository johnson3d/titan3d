using System;
using System.Collections.Generic;
using EngineNS.GamePlay.Scene;
using EngineNS.Graphics.Pipeline;

namespace EngineNS.Bricks.GI.PRT
{
    public unsafe struct FProbeData
    {
        public Vector3 Position;
        public FSHCoefficients Coeffs;
    }
    public class TtPrtProbeVolume : TtVisual
    {
        //EngineNS.Graphics.Pipeline.GI.TtTetrahedron
        public TtGpuBuffer<FProbeData>[] ProbeBuffer;
    }
}
