#ifndef _BaseStructure_Box3_Compute_H_
#define _BaseStructure_Box3_Compute_H_
#include "../Math.cginc"

struct TtBox3
{
	float3 Minimum;
	float3 Maximum;

    void InitEmptyBox()
    {
        Minimum = float3(FLT_MAX, FLT_MAX, FLT_MAX);
        Maximum = float3(-FLT_MAX, -FLT_MAX, -FLT_MAX);
    }
    bool IsEmpty()
    {
        if (Minimum.x >= Maximum.x ||
            Minimum.y >= Maximum.y ||
            Minimum.z >= Maximum.z)
            return true;
        return false;
    }
    /// y  z
    /// | /
    /// |/
    /// -----x
    ///    0------1
    ///   /|     /|
    ///  / |3   / 2
    /// 4------5  /
    /// | /    | /
    /// |/     |/
    /// 7------6
    float3 GetCorners(int index)
    {
        switch (index)
        {
            case 0:
            {
                return float3(Minimum.x, Maximum.y, Maximum.z);
            }            
            case 1:
            {
                return float3(Maximum.x, Maximum.y, Maximum.z);
            }
            case 2:
            {
                return float3(Maximum.x, Minimum.y, Maximum.z);
            }
            case 3:
            {
                return float3(Minimum.x, Minimum.y, Maximum.z);
            }
            case 4:
            {
                return float3(Minimum.x, Maximum.y, Minimum.z);
            }
            case 5:
            {
                return float3(Maximum.x, Maximum.y, Minimum.z);
            }
            case 6:
            {
                return float3(Maximum.x, Minimum.y, Minimum.z);
            }
            case 7:
            {
                return float3(Minimum.x, Minimum.y, Minimum.z);
            }
            default:
                return float3(0, 0, 0);
        }
    }

    float3 GetCenter()
    {
        return (Maximum + Minimum) * 0.5f;
    }
    float3 GetSize()
    {
        return Maximum - Minimum;
    }
    float GetVolume()
    {
        float3 sz = GetSize();
        return sz.x * sz.y * sz.z;
    }
    float GetMaxSide()
    {
        float3 sz = GetSize();
        return max(max(sz.x, sz.y), sz.z);
    }
    static int Contains(in TtBox3 box1, in TtBox3 box2)
    {
        if (box1.Maximum.x < box2.Minimum.x || box1.Minimum.x > box2.Maximum.x)
            return ContainmentType_Disjoint;

        if (box1.Maximum.y < box2.Minimum.y || box1.Minimum.y > box2.Maximum.y)
            return ContainmentType_Disjoint;

        if (box1.Maximum.z < box2.Minimum.z || box1.Minimum.z > box2.Maximum.z)
            return ContainmentType_Disjoint;

        if (box1.Minimum.x <= box2.Minimum.x && box2.Maximum.x <= box1.Maximum.x && box1.Minimum.y <= box2.Minimum.y &&
            box2.Maximum.y <= box1.Maximum.y && box1.Minimum.z <= box2.Minimum.z && box2.Maximum.z <= box1.Maximum.z)
            return ContainmentType_Contains;

        return ContainmentType_Intersects;
    }
    static int Contains(in TtBox3 box, in float3 pos)
    {
        if (box.Minimum.x <= pos.x && pos.x <= box.Maximum.x && box.Minimum.y <= pos.y &&
            pos.y <= box.Maximum.y && box.Minimum.z <= pos.z && pos.z <= box.Maximum.z)
            return ContainmentType_Contains;

        return ContainmentType_Disjoint;
    }
    void Merge(in float3 pos)
    {
        Minimum = min(Minimum, pos);
        Maximum = max(Maximum, pos);
    }
    static TtBox3 Merge(in TtBox3 box1, in TtBox3 box2)
    {
        /*if (box1.IsEmpty() == TiTrue)
            return box2;
        else if (box2.IsEmpty() == TiTrue)
            return box1;
        else*/

        TtBox3 box;
        box.Minimum = min(box1.Minimum, box2.Minimum);
        box.Maximum = max(box1.Maximum, box2.Maximum);
        return box;
    }
    static bool Intersects(in TtBox3 box1, in TtBox3 box2)
    {
        if (box1.Maximum.x < box2.Minimum.x || box1.Minimum.x > box2.Maximum.x)
            return false;

        if (box1.Maximum.y < box2.Minimum.y || box1.Minimum.y > box2.Maximum.y)
            return false;

        return (box1.Maximum.z >= box2.Minimum.z && box1.Minimum.z <= box2.Maximum.z);
    }
};

#endif//_BaseStructure_Box3_Compute_H_