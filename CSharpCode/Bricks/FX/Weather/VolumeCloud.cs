using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.FX.Weather
{
    public class CloudNoiseGenerator
    {
        public int resolution = 64;
        public Vector3 scale = Vector3.One;

        public int octaves = 4;
        public float frequency = 1.0f;
        public float lacunarity = 2.0f;
        public float gain = 0.5f;

        
        public float perlinWeight = 0.7f;
        public float worleyWeight = 0.3f;
        public float billowPower = 1.0f;

        public Support.IRemapCurve remapCurve = null;

        public NxRHI.TtTexture GenerateTexture()
        {
            int size = resolution;
            Color4f[] colors = new Color4f[size * size * size];

            float maxValue = float.MinValue;
            float minValue = float.MaxValue;

            // 预计算一些值以提高性能
            float[] perlinNoise = GenerateFractalPerlinNoise(size);
            float[] worleyNoise = GenerateFractalWorleyNoise(size);

            // 混合噪声
            for (int z = 0; z < size; z++)
            {
                for (int y = 0; y < size; y++)
                {
                    for (int x = 0; x < size; x++)
                    {
                        int index = x + y * size + z * size * size;

                        float perlin = perlinNoise[index];
                        float worley = worleyNoise[index];

                        // 混合Perlin和Worley噪声
                        float value = perlin * perlinWeight + worley * worleyWeight;

                        // 应用Billow效果
                        value = MathF.Pow(value, billowPower);

                        // 应用重映射曲线
                        if (remapCurve!=null)
                            value = remapCurve.Evaluate(value);

                        colors[index] = new Color4f(value, value, value, 1.0f);

                        maxValue = MathF.Max(maxValue, value);
                        minValue = MathF.Min(minValue, value);
                    }
                }
            }

            // 可选归一化
            if (MathF.Abs(maxValue - minValue) > 0.001f)
            {
                for (int i = 0; i < colors.Length; i++)
                {
                    float value = (colors[i].r - minValue) / (maxValue - minValue);
                    colors[i] = new Color4f(value, value, value, 1.0f);
                }
            }

            return null;
        }

        private float[] GenerateFractalPerlinNoise(int size)
        {
            float[] noise = new float[size * size * size];

            for (int z = 0; z < size; z++)
            {
                for (int y = 0; y < size; y++)
                {
                    for (int x = 0; x < size; x++)
                    {
                        float nx = (float)x / size * scale.x;
                        float ny = (float)y / size * scale.y;
                        float nz = (float)z / size * scale.z;

                        float value = FractalPerlin(nx, ny, nz);
                        noise[x + y * size + z * size * size] = value;
                    }
                }
            }

            return noise;
        }

        private float FractalPerlin(float x, float y, float z)
        {
            float value = 0;
            float amplitude = 1;
            float currentFrequency = frequency;

            for (int i = 0; i < octaves; i++)
            {
                float noiseValue = ImprovedPerlinNoise3D(
                    x * currentFrequency,
                    y * currentFrequency,
                    z * currentFrequency);

                value += noiseValue * amplitude;
                amplitude *= gain;
                currentFrequency *= lacunarity;
            }

            return MathHelper.Clamp(value * 0.5f + 0.5f, 0, 1);
        }

        private float ImprovedPerlinNoise3D(float x, float y, float z)
        {
            // 更高质量的3D Perlin噪声实现
            int X = MathHelper.FloorToInt(x) & 255;
            int Y = MathHelper.FloorToInt(y) & 255;
            int Z = MathHelper.FloorToInt(z) & 255;

            x -= MathHelper.Floor(x);
            y -= MathHelper.Floor(y);
            z -= MathHelper.Floor(z);

            float u = Fade(x);
            float v = Fade(y);
            float w = Fade(z);

            int A = p[X] + Y, AA = p[A] + Z, AB = p[A + 1] + Z;
            int B = p[X + 1] + Y, BA = p[B] + Z, BB = p[B + 1] + Z;

            return Lerp(w, Lerp(v, Lerp(u, Grad(p[AA], x, y, z),
                                           Grad(p[BA], x - 1, y, z)),
                                   Lerp(u, Grad(p[AB], x, y - 1, z),
                                           Grad(p[BB], x - 1, y - 1, z))),
                           Lerp(v, Lerp(u, Grad(p[AA + 1], x, y, z - 1),
                                           Grad(p[BA + 1], x - 1, y, z - 1)),
                                   Lerp(u, Grad(p[AB + 1], x, y - 1, z - 1),
                                           Grad(p[BB + 1], x - 1, y - 1, z - 1))));
        }

        private float Fade(float t) => t * t * t * (t * (t * 6 - 15) + 10);
        private float Lerp(float t, float a, float b) => a + t * (b - a);
        private float Grad(int hash, float x, float y, float z)
        {
            int h = hash & 15;
            float u = h < 8 ? x : y;
            float v = h < 4 ? y : (h == 12 || h == 14 ? x : z);
            return ((h & 1) == 0 ? u : -u) + ((h & 2) == 0 ? v : -v);
        }

        private static int[] p = {
        151,160,137,91,90,15,131,13,201,95,96,53,194,233,7,225,140,36,103,30,69,142,
        8,99,37,240,21,10,23,190,6,148,247,120,234,75,0,26,197,62,94,252,219,203,117,
        35,11,32,57,177,33,88,237,149,56,87,174,20,125,136,171,168,68,175,74,165,71,
        134,139,48,27,166,77,146,158,231,83,111,229,122,60,211,133,230,220,105,92,41,
        55,46,245,40,244,102,143,54,65,25,63,161,1,216,80,73,209,76,132,187,208,89,
        18,169,200,196,135,130,116,188,159,86,164,100,109,198,173,186,3,64,52,217,226,
        250,124,123,5,202,38,147,118,126,255,82,85,212,207,206,59,227,47,16,58,17,182,
        189,28,42,223,183,170,213,119,248,152,2,44,154,163,70,221,153,101,155,167,43,
        172,9,129,22,39,253,19,98,108,110,79,113,224,232,178,185,112,104,218,246,97,
        228,251,34,242,193,238,210,144,12,191,179,162,241,81,51,145,235,249,14,239,
        107,49,192,214,31,181,199,106,157,184,84,204,176,115,121,50,45,127,4,150,254,
        138,236,205,93,222,114,67,29,24,72,243,141,128,195,78,66,215,61,156,180
    };

        private float[] GenerateFractalWorleyNoise(int size)
        {
            float[] noise = new float[size * size * size];

            for (int z = 0; z < size; z++)
            {
                for (int y = 0; y < size; y++)
                {
                    for (int x = 0; x < size; x++)
                    {
                        float nx = (float)x / size * scale.x;
                        float ny = (float)y / size * scale.y;
                        float nz = (float)z / size * scale.z;

                        float value = FractalWorley(nx, ny, nz);
                        noise[x + y * size + z * size * size] = value;
                    }
                }
            }

            return noise;
        }

        private float FractalWorley(float x, float y, float z)
        {
            float value = 0;
            float amplitude = 1;
            float currentFrequency = frequency;

            for (int i = 0; i < octaves; i++)
            {
                float noiseValue = WorleyNoise3D(
                    x * currentFrequency,
                    y * currentFrequency,
                    z * currentFrequency);

                value += noiseValue * amplitude;
                amplitude *= gain;
                currentFrequency *= lacunarity;
            }

            return MathHelper.Clamp(value, 0, 1);
        }

        private float WorleyNoise3D(float x, float y, float z)
        {
            // 简化的Worley噪声实现
            int cellX = MathHelper.FloorToInt(x * 3);
            int cellY = MathHelper.FloorToInt(y * 3);
            int cellZ = MathHelper.FloorToInt(z * 3);

            float minDistance = float.MaxValue;

            for (int dx = -1; dx <= 1; dx++)
            {
                for (int dy = -1; dy <= 1; dy++)
                {
                    for (int dz = -1; dz <= 1; dz++)
                    {
                        Vector3 featurePoint = GetFeaturePoint(cellX + dx, cellY + dy, cellZ + dz);
                        Vector3 cellPos = new Vector3(cellX + dx, cellY + dy, cellZ + dz);
                        Vector3 pointPos = cellPos + featurePoint;

                        Vector3 samplePos = new Vector3(x * 3, y * 3, z * 3);
                        float distance = Vector3.Distance(samplePos, pointPos);

                        minDistance = MathF.Min(minDistance, distance);
                    }
                }
            }

            return 1.0f - MathHelper.Clamp(minDistance, 0, 1);
        }

        private Vector3 GetFeaturePoint(int cx, int cy, int cz)
        {
            // 使用哈希函数获取确定性的随机点
            float random(float seed)
            {
                return MathHelper.Repeat(MathF.Sin(seed * 12.9898f) * 43758.5453f, 1f);
            }

            float x = random(cx * 1.0f);
            float y = random(cy * 1.3f + 100);
            float z = random(cz * 1.7f + 200);

            return new Vector3(x, y, z);
        }
    }
    public class TtVolumeCloud
    {
    }
}
