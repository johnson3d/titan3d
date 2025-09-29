#ifndef __SHCOEFFICIENTS_H__
#define __SHCOEFFICIENTS_H__
#include "../../Inc/Math.cginc"

struct TtSHCoefficient
{
    float2 Hammersley(uint idx, uint num)
    {
        uint bits = idx;
        bits = (bits << 16) | (bits >> 16);
        bits = ((bits & 0x55555555u) << 1) | ((bits & 0xAAAAAAAAu) >> 1);
        bits = ((bits & 0x33333333u) << 2) | ((bits & 0xCCCCCCCCu) >> 2);
        bits = ((bits & 0x0F0F0F0Fu) << 4) | ((bits & 0xF0F0F0F0u) >> 4);
        bits = ((bits & 0x00FF00FFu) << 8) | ((bits & 0xFF00FF00u) >> 8);
        float radicalInverse_VdC = (float) (((double) bits) * 2.3283064365386963e-10); // / 0x100000000

        return float2((float) (idx) / (float) (num), radicalInverse_VdC);
    }
    float3 UniformSampleSphere(float2 u)
    {
        float theta = 2.0f * Pi * u.x;
        
        float phi = acos(2.0f * u.y - 1.0f);
        
        float sinPhi = sin(phi);
        float x = sinPhi * cos(theta);
        float y = sinPhi * sin(theta);
        float z = cos(phi);

        return float3(x, y, z);
    }
    float4 CosineSampleHemisphere(float2 E)
    {
        float Phi = 2 * Pi * E.x;
        float CosTheta = sqrt(E.y);
        float SinTheta = sqrt(1 - CosTheta * CosTheta);

        float4 H;
        H.x = SinTheta * cos(Phi);
        H.y = SinTheta * sin(Phi);
        H.z = CosTheta;

        H.w = CosTheta * (1.0f / Pi);

        return H;
    }
    void SHEval3(out float shBasis[9], float3 dir)
    {
        float3 d = normalize(dir);
        float x = d.x, y = d.y, z = d.z;
        
        shBasis[0] = 0.2820947918f; // Y00: 1/(2*sqrt(¦Ð))
        
        shBasis[1] = -0.4886025119f * y; // Y1-1
        shBasis[2] = 0.4886025119f * z; // Y10
        shBasis[3] = -0.4886025119f * x; // Y11
        
        shBasis[4] = 1.0925484306f * x * y; // Y2-2
        shBasis[5] = -1.0925484306f * y * z; // Y2-1
        shBasis[6] = 0.3153915652f * (3.0f * z * z - 1.0f); // Y20
        shBasis[7] = -1.0925484306f * x * z; // Y21
        shBasis[8] = 0.5462742153f * (x * x - y * y); // Y22
    }
    float EvaluateSH(float coefficients[9], float3 normal)
    {
        float basis[9];
        SHEval3(basis, normal);
        float result = 0.0f;

        for (int i = 0; i < 9; i++)
        {
            result += coefficients[i] * basis[i];
        }

        return result;
    }
};

#endif//__SHCOEFFICIENTS_H__