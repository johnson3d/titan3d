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
        public ushort GetNextUInt16()
        {
            return (ushort)mCoreObject.NextValue16Bit();
        }
        public int GetNextInt32()
        {
            return mCoreObject.NextValue();
        }
        public float GetUnit()
        {
            return ((float)mCoreObject.NextValue16Bit()) / (float)UInt16.MaxValue;
        }
        //probability[0-1]
        public bool GetProbability(float probability)
        {
            var value = GetUnit();
            return value < probability;
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
