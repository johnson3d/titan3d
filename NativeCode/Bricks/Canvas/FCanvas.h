#pragma once

#include "../../Base/IUnknown.h"
#include "../../Math/v3dxVector3.h"
#include "../../Math/v3dxVector2.h"
#include "../../NextRHI/NxFrameBuffers.h"
#include "../../NextRHI/NxGeomMesh.h"

NS_BEGIN

namespace Canvas
{
	struct FRect;
	class TR_CLASS()
		FUtility
	{
	public:
		static v3dxVector2 RotateVector(const v3dxVector2 & v, float angle = Math::HALF_PI);
		static bool LineLineIntersection(const v3dxVector2 & ps1, const v3dxVector2 & pe1, const v3dxVector2 & ps2, const v3dxVector2 & pe2, v3dxVector2 * pPoint);

		//sure s in rect
		static bool LineRectIntersection(const v3dxVector2 & s, const v3dxVector2 & e, const FRect & rect, v3dxVector2 * pPoint);
	};

	struct FColorAndFlags
	{
		union
		{
			struct 
			{
				BYTE Red;
				BYTE Green;
				BYTE Blue;
				BYTE Alpha : 6;
				BYTE ShadingMode : 2;//0:normal 1:SDF
			};
			DWORD Value;
		};
		FColorAndFlags operator =(const Rgba& rgba) {
			Red = rgba.R;
			Green = rgba.G;
			Blue = rgba.B;
			Alpha = rgba.A * 63 / 255;
			return *this;
		}
		FColorAndFlags()
		{
			Red = 255;
			Green = 255;
			Blue = 255;
			Alpha = 63;
			ShadingMode = 0;
		}
		FColorAndFlags(BYTE r, BYTE g, BYTE b, BYTE a, BYTE mode)
		{
			Red = r;
			Green = g;
			Blue = b;
			Alpha = a;
			ShadingMode = mode;
		}
		static FColorAndFlags MakeValue(BYTE r, BYTE g, BYTE b, BYTE a, BYTE mode) {
			return FColorAndFlags(r, g, b, a, mode);
		}
	};

	struct TR_CLASS(SV_LayoutStruct = 8)
		FCanvasVertex
	{
		v3dxVector3		Pos{};
		v3dxVector2		UV{};
		FColorAndFlags	Color{};
	};

	enum ERectCorner
	{
		RCN_X0_Y0 = 0,
		RCN_X1_Y0,
		RCN_X0_Y1,
		RCN_X1_Y1,
	};

	struct FRect
	{
		float X = 0;
		float Y = 0;
		float Width = 1.0f;
		float Height = 1.0f;
		FRect()
		{

		}
		FRect(float x, float y, float w, float h)
		{
			X = x; Y = y; Width = w; Height = h;
		}
		float GetRight() const {
			return X + Width;
		}
		float GetBottom() const {
			return Y + Height;
		}
		v3dxVector2 Get_X0_Y0() const{
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

		static FRect And(const FRect& r1, const FRect& r2) {
			FRect result;
			result.X = std::max(r1.X, r2.X);
			result.Y = std::max(r1.Y, r2.Y);
			result.Width = std::min(r1.GetRight(), r2.GetRight()) - result.X;
			result.Height = std::min(r1.GetBottom(), r2.GetBottom()) - result.Y;

			return result;
		}
		bool IsContain(const v3dxVector2& v) const
		{
			if (v.x<X || v.y<Y || v.x>X + Width || v.y>Y + Height)
				return false;
			return true;
		}
		bool ClipLine(const v3dxVector2& s, const v3dxVector2& e, v3dxVector2* os, v3dxVector2* oe) const
		{
			bool bs = IsContain(s);
			bool be = IsContain(s);
			if (bs && be)
			{
				*os = s;
				*oe = e;
				return true;
			}
			else if(bs && !be)
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

	class IImage : public VIUnknownBase
	{
	public:
		virtual NxRHI::ITexture* GetTextureRHI() = 0;
	};
	class IImageRect : public VIUnknownBase
	{
	public:
		FRect				Rect{};//[0-1]
		AutoRef<IImage>		Image;
	};
	class IFont : public VIUnknown
	{
	public:
		virtual IImage* GetWord(WCHAR c, FCanvasVertex vert[4], v3dxVector2* size) const = 0;

		v3dxVector2 GetTextSize(const WCHAR* text) const
		{
			v3dxVector2 result(0, 0);
			v3dxVector2 size;
			for (const WCHAR* c = text; c[0] != '\0'; c++)
			{
				GetWord(c[0], nullptr, &size);
				result.x += size.x;
				if (size.y > result.y)
					result.y = size.y;
			}
			return result;
		}
	};
	class FDrawBatch;
	class FCanvas;
	struct FDrawCmd : public VIUnknownBase
	{
		IImage*		Image = nullptr;
		FDrawBatch* Batch = nullptr;
		std::vector<FCanvasVertex>		mVertices;
		std::vector<UINT>				mIndices;

		void PushQuad(FCanvasVertex vert[4])
		{
			UINT vtStart = (UINT)mVertices.size();
			auto v = vert[ERectCorner::RCN_X0_Y0];
			mVertices.push_back(v);
			v = vert[ERectCorner::RCN_X1_Y0];
			mVertices.push_back(v);
			v = vert[ERectCorner::RCN_X0_Y1];
			mVertices.push_back(v);
			v = vert[ERectCorner::RCN_X1_Y1];
			mVertices.push_back(v);

			mIndices.push_back(vtStart);
			mIndices.push_back(vtStart + 1);
			mIndices.push_back(vtStart + 2);

			mIndices.push_back(vtStart);
			mIndices.push_back(vtStart + 2);
			mIndices.push_back(vtStart + 1);
		}
		void PushQuad(FCanvasVertex vert[4], const v3dxVector2& offset)
		{
			UINT vtStart = (UINT)mVertices.size();
			auto v = vert[ERectCorner::RCN_X0_Y0];
			v.Pos.x += offset.x;
			v.Pos.y += offset.y;
			mVertices.push_back(v);
			v = vert[ERectCorner::RCN_X1_Y0];
			v.Pos.x += offset.x;
			v.Pos.y += offset.y;
			mVertices.push_back(v);
			v = vert[ERectCorner::RCN_X0_Y1];
			v.Pos.x += offset.x;
			v.Pos.y += offset.y;
			mVertices.push_back(v);
			v = vert[ERectCorner::RCN_X1_Y1];
			v.Pos.x += offset.x;
			v.Pos.y += offset.y;
			mVertices.push_back(v);

			mIndices.push_back(vtStart);
			mIndices.push_back(vtStart + 1);
			mIndices.push_back(vtStart + 2);

			mIndices.push_back(vtStart);
			mIndices.push_back(vtStart + 2);
			mIndices.push_back(vtStart + 1);
		}
	};

	class TR_CLASS()
		FDrawCmdList : public VIUnknownBase
	{
	public:
		friend class FDrawBatch;
		friend class FCanvas;

		void NewDrawCmd() {
			mStopCmdIndex = (UINT)mDrawCmds.size();
		}
		void PushClip(const FRect& rect);
		void PopClip();

		void AddText(IFont* font, const WCHAR* text, float x, float y, const Rgba& color);
		void AddLine(const v3dxVector2& start, const v3dxVector2& end, float width, const Rgba& color, IImageRect* imgRect = nullptr);
		void AddLineStrips(const v3dxVector2* pPoints, UINT num, float width, const Rgba& color);
		void AddImage(IImageRect* image, float x, float y, float w, float h, const Rgba& color);
		const FRect& GetCurrentClipRect() const {
			ASSERT(mClipRects.size() > 0);
			return mClipRects[mClipRects.size() - 1];
		}

		void Reset();
	protected:
		FDrawCmd* GetOrNewDrawCmd(IImage* image);
	public:
		FDrawBatch* Batch = nullptr;
	protected:
		std::vector<FRect>				mClipRects;
		std::vector<AutoRef<FDrawCmd>>	mDrawCmds;
		size_t							mStopCmdIndex = 0;
	};

	class TR_CLASS()
		FDrawBatch : public VIUnknownBase
	{
	public:
		FDrawBatch()
		{
			Backgroud = MakeWeakRef(new FDrawCmdList());
			Backgroud->Batch = this;

			Middleground = MakeWeakRef(new FDrawCmdList());
			Middleground->Batch = this;

			Foregroud = MakeWeakRef(new FDrawCmdList());
			Foregroud->Batch = this;
		}
		FDrawCmdList* GetBackgroud() {
			return Backgroud;
		}
		FDrawCmdList* GetMiddleground() {
			return Middleground;
		}
		FDrawCmdList* GetForegroud() {
			return Foregroud;
		}
		void SetPosition(float x, float y) {
			ClientRect.X = x;
			ClientRect.Y = y;
		}
		void Begin(float w, float h);
		void End();
	protected:
		FRect						ClientRect{};
		AutoRef<FDrawCmdList>		Backgroud;
		AutoRef<FDrawCmdList>		Middleground;
		AutoRef<FDrawCmdList>		Foregroud;
	};

	class TR_CLASS()
		FCanvas : public VIUnknown
	{
	public:
		void PushBatch(AutoRef<FDrawBatch>&batch) {
			mBatches.push_back(batch);
		}
		void Begin(float w, float h);
		void End();
		void BuildMesh(NxRHI::FMeshDataProvider* mesh);
	protected:
		void PushCmdList(FDrawCmdList* cmdlist, NxRHI::FMeshDataProvider* mesh);

		void DemoDraw();
	protected:
		FRect								ClientRect{};
		AutoRef<FDrawCmdList>				Backgroud;
		std::vector<AutoRef<FDrawBatch>>	mBatches;
		AutoRef<FDrawCmdList>				Foregroud;
	};
}

NS_END