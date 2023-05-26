#ifndef _BaseStructure_Box2_Compute_H_
#define _BaseStructure_Box2_Compute_H_
#include "../Math.cginc"

struct TtBox2
{
	float2 Minimum;
	float2 Maximum;

    void InitEmptyBox()
    {
        Minimum = float2(FLT_MAX, FLT_MAX);
        Maximum = float2(-FLT_MAX, -FLT_MAX);
    }
    bool IsEmpty()
    {
        if (Minimum.x >= Maximum.x ||
            Minimum.y >= Maximum.y)
            return true;
        return false;
    }
    /// 0------1
    /// |      |
    /// |      |
    /// 3------2
    float2 GetCorners(int index)
    {
        switch (index)
        {
            case 0:
            {
                return float2(Minimum.x, Maximum.y);
            }            
            case 1:
            {
                return float2(Maximum.x, Maximum.y);
            }
            case 2:
            {
                return float2(Maximum.x, Minimum.y);
            }
            case 3:
            {
                return float2(Minimum.x, Minimum.y);
            }
            default:
                return float2(0, 0, 0);
        }
    }

    float2 GetCenter()
    {
        return (Maximum + Minimum) * 0.5f;
    }
    float2 GetSize()
    {
        return Maximum - Minimum;
    }
    float GetVolume()
    {
        float2 sz = GetSize();
        return sz.x * sz.y;
    }
    float GetMaxSide()
    {
        float2 sz = GetSize();
        return max(sz.x, sz.y);
    }
    static int Contains(in TtBox2 box1, in TtBox2 box2)
    {
        if (box1.Maximum.x < box2.Minimum.x || box1.Minimum.x > box2.Maximum.x)
            return ContainmentType_Disjoint;

        if (box1.Maximum.y < box2.Minimum.y || box1.Minimum.y > box2.Maximum.y)
            return ContainmentType_Disjoint;

        if (box1.Minimum.x <= box2.Minimum.x && box2.Maximum.x <= box1.Maximum.x && 
            box1.Minimum.y <= box2.Minimum.y && box2.Maximum.y <= box1.Maximum.y)
            return ContainmentType_Contains;

        return ContainmentType_Intersects;
    }
    static int Contains(in TtBox2 box, in float2 pos)
    {
        if (box.Minimum.x <= pos.x && pos.x <= box.Maximum.x && 
            box.Minimum.y <= pos.y && pos.y <= box.Maximum.y)
            return ContainmentType_Contains;

        return ContainmentType_Disjoint;
    }
    void Merge(in float2 pos)
    {
        Minimum = min(Minimum, pos);
        Maximum = max(Maximum, pos);
    }
    static TtBox2 Merge(in TtBox2 box1, in TtBox2 box2)
    {
        /*if (box1.IsEmpty() == TiTrue)
            return box2;
        else if (box2.IsEmpty() == TiTrue)
            return box1;
        else*/

        TtBox2 box;
        box.Minimum = min(box1.Minimum, box2.Minimum);
        box.Maximum = max(box1.Maximum, box2.Maximum);
        return box;
    }
    static bool Intersects(in TtBox2 box1, in TtBox2 box2)
    {
        if (box1.Maximum.x < box2.Minimum.x || box1.Minimum.x > box2.Maximum.x)
            return false;

        if (box1.Maximum.y < box2.Minimum.y || box1.Minimum.y > box2.Maximum.y)
            return false;

        return true;
    }
};

struct TtBox2i
{
    int2 Minimum;
    int2 Maximum;

    void InitEmptyBox()
    {
        Minimum = int2(INT_MAX, INT_MAX);
        Maximum = int2(-INT_MAX, -INT_MAX);
    }
    bool IsEmpty()
    {
        if (Minimum.x >= Maximum.x ||
            Minimum.y >= Maximum.y)
            return true;
        return false;
    }
    /// 0------1
    /// |      |
    /// |      |
    /// 3------2
    int2 GetCorners(int index)
    {
        switch (index)
        {
        case 0:
        {
            return int2(Minimum.x, Maximum.y);
        }
        case 1:
        {
            return int2(Maximum.x, Maximum.y);
        }
        case 2:
        {
            return int2(Maximum.x, Minimum.y);
        }
        case 3:
        {
            return int2(Minimum.x, Minimum.y);
        }
        default:
            return int2(0, 0, 0);
        }
    }

    int2 GetCenter()
    {
        return (Maximum + Minimum) / 2;
    }
    int2 GetSize()
    {
        return Maximum - Minimum;
    }
    int GetVolume()
    {
        int2 sz = GetSize();
        return sz.x * sz.y;
    }
    int GetMaxSide()
    {
        int2 sz = GetSize();
        return max(sz.x, sz.y);
    }
    static int Contains(in TtBox2i box1, in TtBox2i box2)
    {
        if (box1.Maximum.x < box2.Minimum.x || box1.Minimum.x > box2.Maximum.x)
            return ContainmentType_Disjoint;

        if (box1.Maximum.y < box2.Minimum.y || box1.Minimum.y > box2.Maximum.y)
            return ContainmentType_Disjoint;

        if (box1.Minimum.x <= box2.Minimum.x && box2.Maximum.x <= box1.Maximum.x &&
            box1.Minimum.y <= box2.Minimum.y && box2.Maximum.y <= box1.Maximum.y)
            return ContainmentType_Contains;

        return ContainmentType_Intersects;
    }
    static int Contains(in TtBox2i box, in int2 pos)
    {
        if (box.Minimum.x <= pos.x && pos.x <= box.Maximum.x &&
            box.Minimum.y <= pos.y && pos.y <= box.Maximum.y)
            return ContainmentType_Contains;

        return ContainmentType_Disjoint;
    }
    void Merge(in int2 pos)
    {
        Minimum = min(Minimum, pos);
        Maximum = max(Maximum, pos);
    }
    static TtBox2i Merge(in TtBox2i box1, in TtBox2i box2)
    {
        /*if (box1.IsEmpty() == TiTrue)
            return box2;
        else if (box2.IsEmpty() == TiTrue)
            return box1;
        else*/

        TtBox2i box;
        box.Minimum = min(box1.Minimum, box2.Minimum);
        box.Maximum = max(box1.Maximum, box2.Maximum);
        return box;
    }
    static bool Intersects(in TtBox2i box1, in TtBox2i box2)
    {
        if (box1.Maximum.x < box2.Minimum.x || box1.Minimum.x > box2.Maximum.x)
            return false;

        if (box1.Maximum.y < box2.Minimum.y || box1.Minimum.y > box2.Maximum.y)
            return false;

        return true;
    }
};

#endif//_BaseStructure_Box2_Compute_H_