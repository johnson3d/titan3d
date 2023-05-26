#pragma once
#include "../IUnknown.h"
#include "../TypeUtility.h"

NS_BEGIN

//https://zhuanlan.zhihu.com/p/25102683
struct TR_CLASS(SV_LayoutStruct = 8)
	SdfPoint
{
	SdfPoint()
	{
		dx = 0;
		dy = 0;
	}
	SdfPoint(int x, int y)
	{
		dx = x;
		dy = y;
	}
	int dx, dy;
	float DistanceValue = 0;
	int DistSq() const {
		return dx * dx + dy * dy;
	}
	void UpdateDistanceValue()
	{
		DistanceValue = sqrtf((float)DistSq());
	}
	float Distance() const{
		return sqrtf((float)DistSq());
	}
	bool operator ==(const SdfPoint& rh) const {
		return dx == rh.dx && dy == rh.dy;
	}
	bool operator !=(const SdfPoint& rh) const {
		return dx != rh.dx || dy != rh.dy;
	}
	static SdfPoint Empty;
	static SdfPoint Inside;
};

class TR_CLASS()
	SdfGrid : public IWeakReference
{
public:
	std::vector<SdfPoint> mGrid;
	int Width;
	int Height;
public:
	SdfGrid()
	{
		Width = 0;
		Height = 0;
	}
	void InitGrid(int w, int h)
	{
		Width = w;
		Height = h;
		mGrid.resize(w * h);
	}
	const SdfPoint& GetRef(int x, int y) const
	{
		if (x >= 0 && y >= 0 && x < Width && y < Height)
			return mGrid[y * Width + x];
		else
			return SdfPoint::Empty;
	}
	SdfPoint Get(int x, int y) const
	{
		if (x >= 0 && y >= 0 && x < Width && y < Height)
			return mGrid[y * Width + x];
		else
			return SdfPoint::Empty;
	}
	void Put(int x, int y, const SdfPoint & p)
	{
		mGrid[y * Width + x] = p;
	}
	void SetInside(int x, int y)
	{
		Put(x, y, SdfPoint::Inside);
	}
	void SetEmpty(int x, int y)
	{
		Put(x, y, SdfPoint::Empty);
	}

	void Compare(SdfPoint & p, int x, int y, int offsetx, int offsety)
	{
		auto other = Get(x + offsetx, y + offsety);
		other.dx += offsetx;
		other.dy += offsety;

		if (other.DistSq() < p.DistSq())
			p = other;
	}
	void GenerateSDF()
	{
		// Pass 0
		for (int y = 0; y < Height; y++)
		{
			for (int x = 0; x < Width; x++)
			{
				auto p = Get(x, y);
				Compare(p, x, y, -1, 0);
				Compare(p, x, y, 0, -1);
				Compare(p, x, y, -1, -1);
				Compare(p, x, y, 1, -1);
				Put(x, y, p);
			}

			for (int x = Width - 1; x >= 0; x--)
			{
				auto p = Get(x, y);
				Compare(p, x, y, 1, 0);
				Put(x, y, p);
			}
		}

		// Pass 1
		for (int y = Height - 1; y >= 0; y--)
		{
			for (int x = Width - 1; x >= 0; x--)
			{
				auto p = Get(x, y);
				Compare(p, x, y, 1, 0);
				Compare(p, x, y, 0, 1);
				Compare(p, x, y, -1, 1);
				Compare(p, x, y, 1, 1);
				Put(x, y, p);
			}

			for (int x = 0; x < Width; x++)
			{
				auto p = Get(x, y);
				Compare(p, x, y, -1, 0);
				Put(x, y, p);
			}
		}
	}

	void UpdateDistanceValues(float& minValue, float& maxValue)
	{
		minValue = FLT_MAX;
		maxValue = -FLT_MAX;
		for (auto& i : mGrid)
		{
			i.UpdateDistanceValue();
			if (i.DistanceValue > maxValue)
			{
				maxValue = i.DistanceValue;
			}
			if (i.DistanceValue < minValue)
			{
				minValue = i.DistanceValue;
			}
		}
	}

	static void Generate_SDF(BYTE* pTargetBuffer, int targetPitch, BYTE* pSrcBuffer, int w, int h, int srcW, int srcH, int downScale, int spread = 5);
	static std::vector<float> Generate_SDF2(const std::vector<bool>& in_filled, int width);
};

NS_END
