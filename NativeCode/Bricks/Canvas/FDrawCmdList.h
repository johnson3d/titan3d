#pragma once
#include "FCanvasDefine.h"
#include "Path.h"
#include "FCanvasBrush.h"

NS_BEGIN

namespace Canvas
{
	class FCanvasDrawBatch;
	class FCanvas;
	class FCanvasDrawCmdList;
	class ICanvasBrush;
	class FTFont;
	
	struct TR_CLASS()
		FDrawCmd : public VIUnknown
	{
		AutoRef<ICanvasBrush>			mBrush;
		FCanvasDrawBatch*				Batch = nullptr;
		std::vector<FCanvasVertex>		mVertices;
		std::vector<UINT>				mIndices;

		void PushQuad(FCanvasVertex vert[4])
		{
			UINT vtStart = (UINT)mVertices.size();
			mVertices.resize(vtStart + 4);
			mVertices[vtStart + RCN_X0_Y0] = vert[ERectCorner::RCN_X0_Y0];
			mVertices[vtStart + RCN_X1_Y0] = vert[ERectCorner::RCN_X1_Y0];
			mVertices[vtStart + RCN_X0_Y1] = vert[ERectCorner::RCN_X0_Y1];
			mVertices[vtStart + RCN_X1_Y1] = vert[ERectCorner::RCN_X1_Y1];

			UINT idxStart = (UINT)mIndices.size();
			mIndices.resize(idxStart + 6);
			mIndices[idxStart + 0] = vtStart;
			mIndices[idxStart + 1] = vtStart + RCN_X0_Y1;
			mIndices[idxStart + 2] = vtStart + RCN_X1_Y1;

			mIndices[idxStart + 3] = vtStart;
			mIndices[idxStart + 4] = vtStart + RCN_X1_Y1;
			mIndices[idxStart + 5] = vtStart + RCN_X1_Y0;
		}
		void PushQuad(FCanvasVertex vert[4], const v3dxVector3 & offset)
		{
			UINT vtStart = (UINT)mVertices.size();
			mVertices.resize(vtStart + 4);
			mVertices[vtStart + RCN_X0_Y0] = vert[ERectCorner::RCN_X0_Y0];
			mVertices[vtStart + RCN_X0_Y0].Pos += offset;
			mVertices[vtStart + RCN_X1_Y0] = vert[ERectCorner::RCN_X1_Y0];
			mVertices[vtStart + RCN_X1_Y0].Pos += offset;
			mVertices[vtStart + RCN_X0_Y1] = vert[ERectCorner::RCN_X0_Y1];
			mVertices[vtStart + RCN_X0_Y1].Pos += offset;
			mVertices[vtStart + RCN_X1_Y1] = vert[ERectCorner::RCN_X1_Y1];
			mVertices[vtStart + RCN_X1_Y1].Pos += offset;

			UINT idxStart = (UINT)mIndices.size();
			mIndices.resize(idxStart + 6);
			mIndices[idxStart + 0] = vtStart;
			mIndices[idxStart + 1] = vtStart + RCN_X0_Y1;
			mIndices[idxStart + 2] = vtStart + RCN_X1_Y1;

			mIndices[idxStart + 3] = vtStart;
			mIndices[idxStart + 4] = vtStart + RCN_X1_Y1;
			mIndices[idxStart + 5] = vtStart + RCN_X1_Y0;
			/*UINT vtStart = (UINT)mVertices.size();
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

			mIndices.push_back(vtStart + ERectCorner::RCN_X0_Y0);
			mIndices.push_back(vtStart + ERectCorner::RCN_X0_Y1);
			mIndices.push_back(vtStart + ERectCorner::RCN_X1_Y1);

			mIndices.push_back(vtStart + ERectCorner::RCN_X0_Y0);
			mIndices.push_back(vtStart + ERectCorner::RCN_X1_Y1);
			mIndices.push_back(vtStart + ERectCorner::RCN_X1_Y0);*/
		}

		ICanvasBrush* GetBrush();
	};

	struct TR_CLASS(SV_LayoutStruct = 8)
		FSubDrawCmd
	{
		int VertexStart = 0;
		int Count = 0;
		int IndexStart = 0;
		int IndexCount = 0;
		FDrawCmd* DrawCmd = nullptr;
	};

	class TR_CLASS()
		FCanvasDrawCmdList : public VIUnknown
	{
	public:
		friend class FCanvasDrawBatch;
		friend class FCanvas;

		ENGINE_RTTI(FCanvasDrawCmdList);
		FCanvasDrawCmdList();
		FDrawCmd* GetCurrentDrawCmd() {
			return mCurrentDrawCmd;
		}
		void NewDrawCmd() {
			mStopCmdIndex = (int)mDrawCmds.size();
		}
		void SetCurrentDrawCmd(ICanvasBrush * brush)
		{
			GetOrNewDrawCmd(brush);
		}
		FDrawCmd* GetTopBrushDrawCmd();
		void PushClip(const FRectanglef & rect);
		void PopClip();
		const FRectanglef& GetCurrentClipRect() const {
			ASSERT(mClipRects.size() > 0);
			return mClipRects[mClipRects.size() - 1];
		}
		bool ShouldClip() const {
			return mShouldClip && mClipRects.size() > 0;
		}

		void PushBrush(ICanvasBrush * brush);
		void PopBrush();
		ICanvasBrush* GetCurrentBrush();

		void PushFont(FTFont * font);
		void PopFont();
		FTFont* GetCurrentFont();

		void PushMatrix(const v3dxMatrix4 * matrix);
		void PopMatrix();
		const v3dxMatrix4* GetCurrentMatrix() const;

		void PushPathStyle(Canvas::Path::FPathStyle* PathStyle);
		void PopPathStyle();
		AutoRef<Path::FPathStyle> GetCurrentPathStyle() const
		{
			return mPathStyles.top();
		}
		
		void Reset();

		void AddText(const WCHAR * text, int charCount, float x, float y, const FColor & color, IBlobObject* pOutCmds = nullptr);
		void AddLine(const v3dxVector2 & start, const v3dxVector2 & end, float width, const FColor & color, FSubDrawCmd* pOutCmd = nullptr);
		void AddLineStrips(const v3dxVector2 * pPoints, UINT num, float width, const FColor & color, bool loop, FSubDrawCmd* pOutCmd = nullptr);
		void AddImage(ICanvasBrush * image, float x, float y, float w, float h, const FColor & color, FSubDrawCmd* pOutCmd = nullptr);
		void AddRectLine(const v3dxVector2 & s, const v3dxVector2 & e, float width, const FColor & color, FSubDrawCmd* pOutCmd = nullptr);
		void AddRectFill(const v3dxVector2 & s, const v3dxVector2 & e, const FColor & color, FSubDrawCmd* pOutCmd = nullptr);
	protected:
		FDrawCmd* GetOrNewDrawCmd(ICanvasBrush* brush);
	public:
		FCanvasDrawBatch* Batch = nullptr;
	protected:
		std::vector<FRectanglef>					mClipRects;
		bool								mShouldClip = true;
		std::vector<AutoRef<FDrawCmd>>		mDrawCmds;
		AutoRef<FDrawCmd>					mCurrentDrawCmd;
		int									mStopCmdIndex = 0;

		// push pop stack
		std::stack<AutoRef<FTFont>>			mFonts;
		std::stack<v3dxMatrix4>				mMatrixes;
		std::stack<AutoRef<ICanvasBrush>>	mBrushes;
		std::stack<AutoRef<Path::FPathStyle>> mPathStyles;
	};

	class TR_CLASS()
		FCanvasDrawBatch : public VIUnknown
	{
	public:
		ENGINE_RTTI(FCanvasDrawBatch);
		FCanvasDrawBatch()
		{
			Backgroud = MakeWeakRef(new FCanvasDrawCmdList());
			Backgroud->Batch = this;

			Middleground = MakeWeakRef(new FCanvasDrawCmdList());
			Middleground->Batch = this;

			Foregroud = MakeWeakRef(new FCanvasDrawCmdList());
			Foregroud->Batch = this;
		}
		FCanvasDrawCmdList* GetBackgroud()
		{
			return Backgroud;
		}
		FCanvasDrawCmdList* GetMiddleground()
		{
			return Middleground;
		}
		FCanvasDrawCmdList* GetForegroud()
		{
			return Foregroud;
		}
		void Reset()
		{
			Backgroud->Reset();
			Middleground->Reset();
			Foregroud->Reset();
		}
		void SetClientClip(float w, float h)
		{
			ClientRect.Width = w;
			ClientRect.Height = h;

			Backgroud->Reset();
			Backgroud->PushClip(ClientRect);

			Middleground->Reset();
			Middleground->PushClip(ClientRect);

			Foregroud->Reset();
			Foregroud->PushClip(ClientRect);
		}

		void SetPosition(float x, float y)
		{
			ClientRect.X = x;
			ClientRect.Y = y;
		}
	protected:
		FRectanglef					ClientRect{};
		AutoRef<FCanvasDrawCmdList>		Backgroud;
		AutoRef<FCanvasDrawCmdList>		Middleground;
		AutoRef<FCanvasDrawCmdList>		Foregroud;
	};
}

NS_END