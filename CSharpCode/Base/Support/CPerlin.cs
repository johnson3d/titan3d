using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Support
{
    public class URandom : AuxPtrType<vfxRandom>
    {
        public URandom()
        {
            mCoreObject = vfxRandom.CreateInstance();
        }
    }
    public class CPerlin : AuxPtrType<Perlin>
    {
        public CPerlin(int octaves, float freq, float amp, int seed, int samplerSize = 1024)
        {
            mCoreObject = Perlin.CreateInstance(octaves, freq, amp, seed, samplerSize);
        }
    }
}
