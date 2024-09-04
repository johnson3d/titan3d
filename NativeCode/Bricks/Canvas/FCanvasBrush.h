#pragma once
#include "FCanvasDefine.h"
#include "../../Base/csharp/CsBinder.h"
#include "../../NextRHI/NxBuffer.h"

NS_BEGIN

struct FRectanglef_t
{
	float X;
	float Y;
	float Width;
	float Height;
};
struct FRectanglef
{
	float X = 0;
	float Y = 0;
	float Width = 1.0f;
	float Height = 1.0f;
	FRectanglef()
	{

	}
	FRectanglef(float x, float y, float w, float h)
	{
		X = x; Y = y; Width = w; Height = h;
	}
	float GetRight() const {
		return X + Width;
	}
	float GetBottom() const {
		return Y + Height;
	}
	v3dxVector2 Get_X0_Y0() const {
		return v3dxVector2(X, Y);
	}
	v3dxVector2 Get_X1_Y0() const {
		return v3dxVector2(X + Width, Y);
	}
	v3dxVector2 Get_X1_Y1() const {
		return v3dxVector2(X + Width, Y + Height);
	}
	v3dxVector2 Get_X0_Y1() const {
		return v3dxVector2(X, Y + Height);
	}
	v3dxVector2 GetCenter() const {
		return v3dxVector2(X + Width * .5f, Y + Height * .5f);
	}
	v3dxVector2 GetExtent() const {
		return v3dxVector2(Width * .5f, Height * .5f);
	}
	static FRectanglef And(const FRectanglef& r1, const FRectanglef& r2) {
		FRectanglef result;
		result.X = std::max(r1.X, r2.X);
		result.Y = std::max(r1.Y, r2.Y);
		result.Width = std::min(r1.GetRight(), r2.GetRight()) - result.X;
		result.Height = std::min(r1.GetBottom(), r2.GetBottom()) - result.Y;

		return result;
	}
	static FRectanglef Or(const FRectanglef& r1, const FRectanglef& r2) {
		FRectanglef result;
		result.X = std::min(r1.X, r2.X);
		result.Y = std::min(r1.Y, r2.Y);
		result.Width = std::max(r1.GetRight(), r2.GetRight()) - result.X;
		result.Height = std::max(r1.GetBottom(), r2.GetBottom()) - result.Y;

		return result;
	}
	bool IsContain(const v3dxVector2& v) const
	{
		if (v.X<X || v.Y<Y || v.X>X + Width || v.Y>Y + Height)
			return false;
		return true;
	}
	bool IsContain(const FRectanglef& r) const
	{
		if (r.GetRight() > GetRight() || r.X < X || r.GetBottom() > GetBottom() || r.Y < Y)
			return false;
		return true;
	}
	bool IsOverlap(const FRectanglef& r) const
	{
		if (r.GetRight() <= X || r.X >= GetRight() || r.GetBottom() <= Y || r.Y >= GetBottom())
			return false;
		return true;
	}
	bool IsValid() const
	{
		return Width > 0.f && Height > 0.f;
	}
	bool IsDefault() const
	{
		return X == 0.f && Y == 0.f && Width == 1.f && Height == 1.f;
	}
	bool ClipLine(const v3dxVector2& s, const v3dxVector2& e, v3dxVector2* os, v3dxVector2* oe) const
	{
		bool bs = IsContain(s);
		bool be = IsContain(e);
		if (bs && be)
		{
			*os = s;
			*oe = e;
			return true;
		}
		else if (bs && !be)
		{
			*os = s;
			return FUtility::LineRectIntersection(s, e, *this, oe);
		}
		else if (!bs && be)
		{
			*oe = e;
			return FUtility::LineRectIntersection(e, s, *this, os);
		}
		else
		{
			v3dxVector2 intersection;
			v3dxVector2 r00 = Get_X0_Y0();
			v3dxVector2 r10 = Get_X1_Y0();
			v3dxVector2 r11 = Get_X1_Y1();
			v3dxVector2 r01 = Get_X0_Y1();
			int hitCount = 0;
			if (FUtility::LineLineIntersection(s, e, r00, r10, &intersection))
			{
				*os = intersection;
				hitCount++;
			}
			if (FUtility::LineLineIntersection(s, e, r00, r01, &intersection))
			{
				if (hitCount == 0)
				{
					*os = intersection;
					hitCount++;
				}
				else
				{
					*oe = intersection;
					return true;
				}
			}
			if (FUtility::LineLineIntersection(s, e, r01, r11, &intersection))
			{
				if (hitCount == 0)
				{
					*os = intersection;
					hitCount++;
				}
				else
				{
					*oe = intersection;
					return true;
				}
			}
			if (FUtility::LineLineIntersection(s, e, r11, r01, &intersection))
			{
				if (hitCount == 0)
				{
					*os = intersection;
					hitCount++;
				}
				else
				{
					*oe = intersection;
					return true;
				}
			}
			return false;
		}
	}
};

namespace Canvas
{
	class TR_CLASS()
		ICanvasBrush : public VIUnknown
	{
	public:
		ENGINE_RTTI(ICanvasBrush);
		ICanvasBrush()
		{

		}
		FRectanglef					Rect{};//[0-1]
		AutoRef<NxRHI::ISrView>		SrView;
		VNameString					Name;
		FColor						Color;
		std::map<VNameString, TtAnyValue>	ExtValues;
		bool						IsDirty;
		void SetUV(const v3dxVector2 & uv0, const v3dxVector2 & uv1)
		{
			Rect.X = uv0.X;
			Rect.Y = uv0.Y;
			Rect.Width = uv1.X - uv0.X;
			Rect.Height = uv1.Y - uv0.Y;
		}
		v3dxVector2 GetUV0() const {
			return v3dxVector2(Rect.X, Rect.Y);
		}
		v3dxVector2 GetUV1() const {
			return v3dxVector2(Rect.X + Rect.Width, Rect.Y + Rect.Height);
		}
		void SetSrv(NxRHI::ISrView * srv) {
			SrView = srv;
		}
		NxRHI::ISrView* GetSrv() {
			return SrView;
		}
		//just for GetOrNewDrawCmd
		virtual bool IsSameCmd(ICanvasBrush* other) const
		{
			if (GetRtti() != other->GetRtti())
				return false;
			if (Color != other->Color)
				return false;
			if (SrView == nullptr && other->SrView == nullptr)
				return this == other;
			return (SrView == other->SrView);
		}
		FColor GetColor() const {
			return Color;
		}
		void SetColor(const FColor& color)
		{
			Color = color;
			IsDirty = true;
		}
		bool GetIsDirty() const {
			return IsDirty;
		}
		void SetIsDirty(bool isDirty) {
			IsDirty = isDirty;
		}
		void SetValue(const char* name, const TtAnyValue& value)
		{
			VNameString nameStr(name);
			auto iter = ExtValues.find(nameStr);
			if (iter != ExtValues.end())
				iter->second = value;
			else
				ExtValues.insert(std::make_pair(nameStr, value));
			IsDirty = true;
		}
		bool GetValue(const char* name, TtAnyValue& value)
		{
			VNameString nameStr(name);
			auto iter = ExtValues.find(nameStr);
			if (iter == ExtValues.end())
				return false;

			value = iter->second;
			return true;
		}
		void SetValuesToCbView(NxRHI::ICbView& cbuffer, NxRHI::FCbvUpdater& updater)
		{
			for (auto data : ExtValues)
			{
				auto fld = cbuffer.ShaderBinder->FindField(data.first.c_str());
				if (fld != nullptr)
				{
					auto size = data.second.GetValueSize();
					cbuffer.SetValue(*fld, &(data.second.mI8Value), size, true, &updater);
				}
			}
		}
	};
}

NS_END