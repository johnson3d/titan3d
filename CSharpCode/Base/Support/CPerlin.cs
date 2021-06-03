using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Support
{
    public class CPerlin : AuxPtrType<Perlin>
    {
        public CPerlin(int octaves, float freq, float amp, int seed)
        {
            mCoreObject = Perlin.CreateInstance(octaves, freq, amp, seed);
        }
    }
}
