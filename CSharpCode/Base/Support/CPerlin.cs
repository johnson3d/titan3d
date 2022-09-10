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

    public class UPerlin
    {
        public UPerlin(int seed, int rdPoolSize = 255)
        {
            var rd = new URandom();
            rd.mCoreObject.SetSeed(seed);
            mPerm = new int[rdPoolSize + 1];
            for (int i = 0; i < rdPoolSize + 1; i++)
            {
                mPerm[i] = rd.GetNextUInt16() % 255;
            }
        }
        public enum EFbmMode
        {
            Classic,
            Turbulence,
            Ridge,
            RidgeSharp,
        }
        public float RidgeOffset = 1;
        public double GetPerlinValue(EFbmMode mode, DVector2 coord, int terms, float freq, float amp, float lacumarity = 2.0f, float gain = 0.5f, float rotAngel = 0.0f)
        {
            double result = 0.0f;

            coord = coord * freq;

            for (int i = 0; i < terms; i++)
            {
                switch (mode)
                {
                    case EFbmMode.Classic:
                        result += Noise(coord) * amp;
                        break;
                    case EFbmMode.Turbulence:
                        result += CoreDefine.Abs(Noise(coord) * amp);
                        break;
                    case EFbmMode.Ridge:
                        {
                            var n = CoreDefine.Abs(Noise(coord));
                            n = RidgeOffset - n;
                            result += amp * n;
                        }
                        break;
                    case EFbmMode.RidgeSharp:
                        {
                            var n = CoreDefine.Abs(Noise(coord));
                            n = RidgeOffset - n;
                            n = n * n;
                            result += amp * n;
                        }
                        break;
                }

                if (rotAngel != 0)
                {
                    var rot = DMatrix3x3.RotationZ(rotAngel);
                    coord = DVector2.TransformCoordinate(in coord, in rot);

                    rotAngel *= gain;
                }
                coord *= lacumarity;
                amp *= gain;
            }

            return result;
        }

        public double GetPerlinValue1(DVector2 coord, int terms, float freq, float amp)
        {
            double result = 0.0f;

            coord = coord * freq;

            for (int i = 0; i < terms; i++)
            {
                result += CoreDefine.Abs(Noise(coord)) * amp;
                coord *= 2.0f;

                amp *= 0.5f;
            }

            return result;
            //double f = 1.98;  // could be 2.0
            //double s = 0.49;  // could be 0.5
            //double a = 0.0;
            //double b = 0.5;
            //var d = new DVector3(0.0);
            //var m = DMatrix.Identity;
            //for (int i = 0; i < octaves; i++)
            //{
            //    var n = Noise(x);
            //    a += b * n;          // accumulate values
            //    //d += b * m * n.yzw;      // accumulate derivatives
            //    b *= s;
            //    x = f * m3 * x;
            //    m = f * m3i * m;
            //}
            //return vec4(a, d);
        }
        //https://thebookofshaders.com/13/?lan=ch
        //https://iquilezles.org/articles/morenoise/
        public double GetPerlinValue2(DVector2 coord, int terms, float freq, float amp)
        {
            double result = 0.0f;
            coord = coord * freq;

            for (int i = 0; i < terms; i++)
            {
                var n = Noise(coord);
                n = CoreDefine.Abs(n) * amp;
                n = n * n;
                result += n;
                
                coord *= 2.0f;
                amp *= 0.5f;
            }

            return result;
        }

        #region Noise functions
        private int ClampPermIndex(int v)
        {
            return (int)((uint)v % (uint)(mPerm.Length - 1));
        }

        public double Noise(double x)
        {
            //var X = CoreDefine.FloorToInt(x) & 0xff;
            var X = ClampPermIndex(CoreDefine.FloorToInt(x));
            x -= CoreDefine.Floor(x);
            var u = Fade(x);
            return Lerp(u, Grad(mPerm[X], x), Grad(mPerm[X + 1], x - 1)) * 2;
        }

        public double Noise(double x, double y)
        {
            //var X = CoreDefine.FloorToInt(x) & 0xff;
            //var Y = CoreDefine.FloorToInt(y) & 0xff;
            var X = ClampPermIndex(CoreDefine.FloorToInt(x));
            var Y = ClampPermIndex(CoreDefine.FloorToInt(y));
            x -= CoreDefine.Floor(x);
            y -= CoreDefine.Floor(y);
            var u = Fade(x);
            var v = Fade(y);
            //var A = (mPerm[X] + Y) & 0xff;
            //var B = (mPerm[X + 1] + Y) & 0xff;
            var A = ClampPermIndex(mPerm[X] + Y);
            var B = ClampPermIndex(mPerm[X + 1] + Y);
            return Lerp(v, Lerp(u, Grad(mPerm[A], x, y), Grad(mPerm[B], x - 1, y)),
                           Lerp(u, Grad(mPerm[A + 1], x, y - 1), Grad(mPerm[B + 1], x - 1, y - 1)));
        }

        public double Noise(DVector2 coord)
        {
            return Noise(coord.X, coord.Y);
        }

        public double Noise(double x, double y, double z)
        {
            //var X = CoreDefine.FloorToInt(x) & 0xff;
            //var Y = CoreDefine.FloorToInt(y) & 0xff;
            //var Z = CoreDefine.FloorToInt(z) & 0xff;
            var X = ClampPermIndex(CoreDefine.FloorToInt(x));
            var Y = ClampPermIndex(CoreDefine.FloorToInt(y));
            var Z = ClampPermIndex(CoreDefine.FloorToInt(z));
            x -= CoreDefine.Floor(x);
            y -= CoreDefine.Floor(y);
            z -= CoreDefine.Floor(z);
            var u = Fade(x);
            var v = Fade(y);
            var w = Fade(z);
            //var A = (mPerm[X] + Y) & 0xff;
            //var B = (mPerm[X + 1] + Y) & 0xff;
            //var AA = (mPerm[A] + Z) & 0xff;
            //var BA = (mPerm[B] + Z) & 0xff;
            //var AB = (mPerm[A + 1] + Z) & 0xff;
            //var BB = (mPerm[B + 1] + Z) & 0xff;

            var A = ClampPermIndex(mPerm[X] + Y);
            var B = ClampPermIndex(mPerm[X + 1] + Y);
            var AA = ClampPermIndex(mPerm[A] + Z);
            var BA = ClampPermIndex(mPerm[B] + Z);
            var AB = ClampPermIndex(mPerm[A + 1] + Z);
            var BB = ClampPermIndex(mPerm[B + 1] + Z);
            return Lerp(w, Lerp(v, Lerp(u, Grad(mPerm[AA], x, y, z), Grad(mPerm[BA], x - 1, y, z)),
                                   Lerp(u, Grad(mPerm[AB], x, y - 1, z), Grad(mPerm[BB], x - 1, y - 1, z))),
                           Lerp(v, Lerp(u, Grad(mPerm[AA + 1], x, y, z - 1), Grad(mPerm[BA + 1], x - 1, y, z - 1)),
                                   Lerp(u, Grad(mPerm[AB + 1], x, y - 1, z - 1), Grad(mPerm[BB + 1], x - 1, y - 1, z - 1))));
        }

        public double Noise(DVector3 coord)
        {
            return Noise(coord.X, coord.Y, coord.Z);
        }

        #endregion

        #region fBm functions

        public double Fbm(double x, int octave)
        {
            double f = 0.0f;
            double w = 0.5f;
            for (var i = 0; i < octave; i++)
            {
                f += w * Noise(x);
                x *= 2.0f;
                w *= 0.5f;
            }
            return f;
        }

        public double Fbm(DVector2 coord, int octave)
        {
            double f = 0.0f;
            double w = 0.5f;
            for (var i = 0; i < octave; i++)
            {
                f += w * Noise(coord);
                coord *= 2.0f;
                w *= 0.5f;
            }
            return f;
        }

        public double Fbm(double x, double y, int octave)
        {
            return Fbm(new DVector2(x, y), octave);
        }

        public double Fbm(DVector3 coord, int octave)
        {
            double f = 0.0f;
            double w = 0.5f;
            for (var i = 0; i < octave; i++)
            {
                f += w * Noise(coord);
                coord *= 2.0f;
                w *= 0.5f;
            }
            return f;
        }

        public double Fbm(double x, double y, double z, int octave)
        {
            return Fbm(new DVector3(x, y, z), octave);
        }

        #endregion

        #region Private functions

        static double Fade(double t)
        {
            return t * t * t * (t * (t * 6 - 15) + 10);
        }

        static double Lerp(double t, double a, double b)
        {
            return a + t * (b - a);
        }

        static double Grad(int hash, double x)
        {
            return (hash & 1) == 0 ? x : -x;
        }

        static double Grad(int hash, double x, double y)
        {
            return ((hash & 1) == 0 ? x : -x) + ((hash & 2) == 0 ? y : -y);
        }

        static double Grad(int hash, double x, double y, double z)
        {
            var h = hash & 15;
            var u = h < 8 ? x : y;
            var v = h < 4 ? y : (h == 12 || h == 14 ? x : z);
            return ((h & 1) == 0 ? u : -u) + ((h & 2) == 0 ? v : -v);
        }

        int[] mPerm = {
        151,160,137,91,90,15,
        131,13,201,95,96,53,194,233,7,225,140,36,103,30,69,142,8,99,37,240,21,10,23,
        190, 6,148,247,120,234,75,0,26,197,62,94,252,219,203,117,35,11,32,57,177,33,
        88,237,149,56,87,174,20,125,136,171,168, 68,175,74,165,71,134,139,48,27,166,
        77,146,158,231,83,111,229,122,60,211,133,230,220,105,92,41,55,46,245,40,244,
        102,143,54, 65,25,63,161, 1,216,80,73,209,76,132,187,208, 89,18,169,200,196,
        135,130,116,188,159,86,164,100,109,198,173,186, 3,64,52,217,226,250,124,123,
        5,202,38,147,118,126,255,82,85,212,207,206,59,227,47,16,58,17,182,189,28,42,
        223,183,170,213,119,248,152, 2,44,154,163, 70,221,153,101,155,167, 43,172,9,
        129,22,39,253, 19,98,108,110,79,113,224,232,178,185, 112,104,218,246,97,228,
        251,34,242,193,238,210,144,12,191,179,162,241, 81,51,145,235,249,14,239,107,
        49,192,214, 31,181,199,106,157,184, 84,204,176,115,121,50,45,127, 4,150,254,
        138,236,205,93,222,114,67,29,24,72,243,141,128,195,78,66,215,61,156,180,
        151};

        #endregion
    }
}
