using NPOI.SS.Formula.Functions;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace EngineNS.Graphics.Pipeline.GI
{
    public struct FCubemapResult
    {
        public NxRHI.ECubeFace Face;
        public Vector2 UV;

        public FCubemapResult(NxRHI.ECubeFace face, Vector2 uv)
        {
            Face = face;
            UV = uv;
        }
        public static FCubemapResult DirectionToCubemap(Vector3 direction)
        {
            // 标准化方向向量
            //direction = Vector3.Normalize(in direction);

            float absX = Math.Abs(direction.x);
            float absY = Math.Abs(direction.y);
            float absZ = Math.Abs(direction.z);

            NxRHI.ECubeFace face;
            Vector2 uv;

            // 确定主要轴和对应的面
            if (absX >= absY && absX >= absZ)
            {
                // X轴为主要轴
                if (direction.x > 0)
                {
                    // Positive X (Right face)
                    face = NxRHI.ECubeFace.CBFC_Right;
                    uv.X = -direction.z / direction.x;
                    uv.Y = -direction.y / direction.x;
                }
                else
                {
                    // Negative X (Left face)
                    face = NxRHI.ECubeFace.CBFC_Left;
                    uv.X = direction.z / (-direction.x);
                    uv.Y = -direction.y / (-direction.x);
                }
            }
            else if (absY >= absX && absY >= absZ)
            {
                // Y轴为主要轴
                if (direction.y > 0)
                {
                    // Positive Y (Top face)
                    face = NxRHI.ECubeFace.CBFC_Top;
                    uv.X = direction.x / direction.y;
                    uv.Y = direction.z / direction.y;
                }
                else
                {
                    // Negative Y (Bottom face)
                    face = NxRHI.ECubeFace.CBFC_Bottom;
                    uv.X = direction.x / (-direction.y);
                    uv.Y = -direction.z / (-direction.y);
                }
            }
            else
            {
                // Z轴为主要轴
                if (direction.z > 0)
                {
                    // Positive Z (Front face)
                    face = NxRHI.ECubeFace.CBFC_Bac;
                    uv.X = direction.x / direction.z;
                    uv.Y = -direction.y / direction.z;
                }
                else
                {
                    // Negative Z (Back face)
                    face = NxRHI.ECubeFace.CBFC_Front;
                    uv.X = -direction.x / (-direction.z);
                    uv.Y = -direction.y / (-direction.z);
                }
            }

            // 将UV坐标从[-1,1]范围转换到[0,1]范围
            uv.X = (uv.X + 1.0f) * 0.5f;
            uv.Y = (uv.Y + 1.0f) * 0.5f;

            // 确保UV坐标在有效范围内
            uv.X = Math.Max(0.0f, Math.Min(1.0f, uv.X));
            uv.Y = Math.Max(0.0f, Math.Min(1.0f, uv.Y));

            return new FCubemapResult(face, uv);
        }
    }
    public class TtSHCoefficient
    {
        public static Vector2 Hammersley(uint idx, uint num)
        {
            uint bits = idx;
            bits = (bits << 16) | (bits >> 16);
            bits = ((bits & 0x55555555u) << 1) | ((bits & 0xAAAAAAAAu) >> 1);
            bits = ((bits & 0x33333333u) << 2) | ((bits & 0xCCCCCCCCu) >> 2);
            bits = ((bits & 0x0F0F0F0Fu) << 4) | ((bits & 0xF0F0F0F0u) >> 4);
            bits = ((bits & 0x00FF00FFu) << 8) | ((bits & 0xFF00FF00u) >> 8);
            float radicalInverse_VdC = (float)(((double)bits) * 2.3283064365386963e-10); // / 0x100000000

            return new Vector2((float)(idx) / (float)(num), radicalInverse_VdC);
        }
        public static Vector3 UniformSampleSphere(Vector2 u)
        {
            float theta = 2.0f * MathF.PI * u.X;

            // 计算极角 φ ∈ [0, π]
            // 关键步骤：通过反余弦确保均匀分布
            float phi = MathF.Acos(2.0f * u.Y - 1.0f);

            // 转换为笛卡尔坐标
            float sinPhi = MathF.Sin(phi);
            float x = sinPhi * MathF.Cos(theta);
            float y = sinPhi * MathF.Sin(theta);
            float z = MathF.Cos(phi);

            return new Vector3(x, y, z);
        }
        public static Vector4 CosineSampleHemisphere(Vector2 E)
        {
            float Phi = 2 * MathF.PI * E.X;
            float CosTheta = MathF.Sqrt(E.Y);
            float SinTheta = MathF.Sqrt(1 - CosTheta * CosTheta);

            Vector4 H;
            H.X = SinTheta * MathF.Cos(Phi);
            H.Y = SinTheta * MathF.Sin(Phi);
            H.Z = CosTheta;

            H.W = CosTheta * (1.0f / MathF.PI);

            return H;
        }
        public static float[] EvaluateSHBasis(Vector3 normal)
        {
            Vector3 n = Vector3.Normalize(normal);
            float x = n.X;
            float y = n.Y;
            float z = n.Z;
            float[] basis = new float[9];

            // l=0
            basis[0] = 0.5f * MathF.Sqrt(1.0f / MathF.PI);

            // l=1
            float sqrt3Over4Pi = MathF.Sqrt(3.0f / (4.0f * MathF.PI));
            basis[1] = sqrt3Over4Pi * y;  // m=-1
            basis[2] = sqrt3Over4Pi * z;  // m=0
            basis[3] = sqrt3Over4Pi * x;  // m=+1

            // l=2
            float sqrt15OverPi = MathF.Sqrt(15.0f / MathF.PI);
            float sqrt5Over16Pi = MathF.Sqrt(5.0f / (16.0f * MathF.PI));
            float sqrt15Over16Pi = MathF.Sqrt(15.0f / (16.0f * MathF.PI));

            basis[4] = 0.5f * sqrt15OverPi * x * y;      // m=-2
            basis[5] = 0.5f * sqrt15OverPi * y * z;      // m=-1
            basis[6] = sqrt5Over16Pi * (3.0f * z * z - 1.0f); // m=0
            basis[7] = 0.5f * sqrt15OverPi * x * z;      // m=+1
            basis[8] = sqrt15Over16Pi * (x * x - y * y); // m=+2

            return basis;
        }
        public static unsafe void SHEval3(float[] inBasis, in Vector3 dir)
        {
            System.Diagnostics.Debug.Assert(inBasis.Length >= 9);

            fixed(float* shBasis = &inBasis[0])
            {
                SHEval3(shBasis, in dir);
            }
        }
        public static unsafe void SHEval3(float* shBasis, in Vector3 dir)
        {
            // 归一化方向
            Vector3 d = dir;
            d.Normalize();
            float x = d.x, y = d.y, z = d.z;

            // 第0阶 (l=0)
            shBasis[0] = 0.2820947918f; // Y00: 1/(2*sqrt(π))

            // 第1阶 (l=1)
            shBasis[1] = -0.4886025119f * y; // Y1-1
            shBasis[2] = 0.4886025119f * z; // Y10
            shBasis[3] = -0.4886025119f * x; // Y11

            // 第2阶 (l=2)
            shBasis[4] = 1.0925484306f * x * y; // Y2-2
            shBasis[5] = -1.0925484306f * y * z; // Y2-1
            shBasis[6] = 0.3153915652f * (3.0f * z * z - 1.0f); // Y20
            shBasis[7] = -1.0925484306f * x * z; // Y21
            shBasis[8] = 0.5462742153f * (x * x - y * y); // Y22

            // 第3阶 (l=3) 可根据需要扩展
        }
        public static float[] PrecomputeSHCoefficients(Func<Vector3, float> sampleEnvironment, uint sampleCount = 100000)
        {
            float[] coefficients = new float[9];

            float[] basis = new float[9];
            for (uint i = 0; i < sampleCount; i++)
            {
                // 生成均匀分布的随机方向（蒙特卡洛采样）
                Vector3 dir = UniformSampleSphere(Hammersley(i, sampleCount));//CosineSampleHemisphere
                //System.Diagnostics.Debug.Assert(Vector3.GreatEqual(dir, Vector3.Zero).All());
                //System.Diagnostics.Debug.Assert(dir.Z>0);

                // 采样环境光照颜色（返回值为float，范围[0,1]）
                float radiance = sampleEnvironment(dir);

                // 计算球谐基函数值
                SHEval3(basis, in dir);

                // 累加到系数
                for (int j = 0; j < 9; j++)
                {
                    coefficients[j] += radiance * basis[j];
                }
            }

            // 应用蒙特卡洛积分权重（4π / sampleCount）
            float weight = 4.0f * MathF.PI / sampleCount;
            for (int j = 0; j < 9; j++)
            {
                coefficients[j] *= weight;
            }
            return coefficients;
        }
        public static unsafe float EvaluateSH(float[] coefficients, in Vector3 normal)
        {
            if (coefficients.Length != 9)
                throw new ArgumentException("Coefficients must have 9 elements for 3rd-order SH.");

            fixed(float* p = &coefficients[0])
            {
                return EvaluateSH(p, in normal);
            }
        }
        public static unsafe float EvaluateSH(float* coefficients, in Vector3 normal)
        {
            float* basis = stackalloc float[9];
            SHEval3(basis, normal);
            float result = 0.0f;

            for (int i = 0; i < 9; i++)
            {
                result += coefficients[i] * basis[i];
            }

            return result;
        }
    }
}
