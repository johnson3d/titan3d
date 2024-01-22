#ifndef _BaseStructure_Quat_Compute_H_
#define _BaseStructure_Quat_Compute_H_

struct TtQuat
{
    float4 XYZW;
    float X()
    {
        return XYZW.x;
    }
    float Y()
    {
        return XYZW.y;
    }
    float Z()
    {
        return XYZW.z;
    }
    float W()
    {
        return XYZW.w;
    }
    
    void SetAxisAngel(float3 in_axis, float angle)
    {
        float3 axis = normalize(in_axis);
        
        float h = angle * 0.5f;
        float s = sin(h);
        float c = cos(h);

        XYZW.xyz = axis * s;
        XYZW.w = c;
    }
    float3 RotateVector3(float3 pt)
    {
        // http://people.csail.mit.edu/bkph/articles/Quaternions.pdf
		// V' = V + 2w(Q x V) + (2Q x (Q x V))
		// refactor:
		// V' = V + w(2(Q x V)) + (Q x (2(Q x V)))
		// T = 2(Q x V);
		// V' = V + w*(T) + (Q x T)

        float3 Q = XYZW.xyz;
        float3 T = cross(Q, pt) * 2.0f;
        float3 Result = pt + (T * XYZW.w) + cross(Q, T);
        return Result;
    }
    float3 UnrotateVector3(float3 vec)
    {
        TtQuat invQuat;
        invQuat.XYZW.x = -XYZW.x;
        invQuat.XYZW.y = -XYZW.y;
        invQuat.XYZW.z = -XYZW.z;
        invQuat.XYZW.w = XYZW.w;
        return invQuat.RotateVector3(vec);
    }
    static float4x4 Quat2Matrix(TtQuat quaternion)
    {
        float4x4 result;

        float3 xxyyzzww = quaternion.XYZW * quaternion.XYZW;
        float xx = xxyyzzww.x;
        float yy = xxyyzzww.y;
        float zz = xxyyzzww.z;
        
        float4 temp = quaternion.XYZW.xzzy * quaternion.XYZW.ywxw;
        float xy = temp.x;
        float zw = temp.y;
        float zx = temp.z;
        float yw = temp.w;
        
        temp.xy = quaternion.XYZW.yx * quaternion.XYZW.zw;
        float yz = temp.x;
        float xw = temp.y;
        
        result[0][0] = 1.0f - (2.0f * (yy + zz));
        result[0][1] = 2.0f * (xy + zw);
        result[0][2] = 2.0f * (zx - yw);
        result[0][3] = 0;

        result[1][0] = 2.0f * (xy - zw);
        result[1][1] = 1.0f - (2.0f * (zz + xx));
        result[1][2] = 2.0f * (yz + xw);
        result[1][3] = 0;

        result[2][0] = 2.0f * (zx + yw);
        result[2][1] = 2.0f * (yz - xw);
        result[2][2] = 1.0f - (2.0f * (yy + xx));
        result[2][3] = 0;

        result[3][0] = 0;
        result[3][1] = 0;
        result[3][2] = 0;
        result[3][3] = 1.0f;

        return result;
    }
    
    static TtQuat CreateQuat(float4 value)
    {
        TtQuat result;
        result.XYZW = value;
        return result;
    }
    
    static float3 TransformedBoxAABB(in float3 dims, in TtQuat quat)
    {
        float3x3 mat = Quat2Matrix(quat);
        float3 w = mat[0] * dims.x;
        float3 h = mat[1] * dims.y;
        float3 d = mat[2] * dims.z;
        float3 a = w + h, b = w - h;
        return max(max(abs(a + d), abs(a - d)),
               max(abs(b + d), abs(b - d)));
    }
};

#endif//_BaseStructure_Quat_Compute_H_