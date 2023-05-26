#include "FDrawCmdList.h"
#include "FCanvas.h"
#include "FTFont.h"

#define new VNEW

NS_BEGIN

namespace Canvas
{
	ICanvasBrush* FDrawCmd::GetBrush()
	{
		return mBrush;
	}
	FCanvasDrawCmdList::FCanvasDrawCmdList() 
	{
		auto pDefaultBrush = MakeWeakRef(new ICanvasBrush());
		pDefaultBrush->Rect = FRectanglef(0, 0, 1, 1);
		pDefaultBrush->Name = VNameString("DefaultBrush");
		mBrushes.push(pDefaultBrush);
		// todo: default font
		// mFonts.push()
	}
	FDrawCmd* FCanvasDrawCmdList::GetOrNewDrawCmd(ICanvasBrush* brush)
	{
		if (mCurrentDrawCmd != nullptr && mCurrentDrawCmd->mBrush->IsSameCmd(brush))
		{
			return mCurrentDrawCmd;
		}
		if (mDrawCmds.size() > 0)
		{
			for (int i = (int)mDrawCmds.size() - 1; i >= mStopCmdIndex; i--)
			{
				if (mDrawCmds[i]->mBrush->IsSameCmd(brush))
				{
					mCurrentDrawCmd = mDrawCmds[i];
					return mDrawCmds[i];
				}
			}
		}
		auto tmp = MakeWeakRef(new FDrawCmd());
		tmp->mBrush = brush;
		tmp->Batch = Batch;
		mDrawCmds.push_back(tmp);
		mCurrentDrawCmd = tmp;
		return tmp;
	}
	void FCanvasDrawCmdList::PushClip(const FRectanglef& rect)
	{
		if (mClipRects.size() > 0)
		{
			const auto& rc = GetCurrentClipRect();
			auto clip = FRectanglef::And(rc, rect);
			mClipRects.push_back(clip);
		}
		else
		{
			mClipRects.push_back(rect);
		}
	}
	void FCanvasDrawCmdList::PopClip()
	{
		//mClipRects.erase(mClipRects.begin() + mClipRects.size()); //TODO
	}
	void FCanvasDrawCmdList::PushBrush(ICanvasBrush* brush)
	{
		if (brush == nullptr)
			return;
		mBrushes.push(brush);
	}
	void FCanvasDrawCmdList::PopBrush()
	{
		mBrushes.pop();
	}
	ICanvasBrush* FCanvasDrawCmdList::GetCurrentBrush()
	{
		if (mBrushes.empty())
			return nullptr;
		return mBrushes.top();
	}
	FDrawCmd* FCanvasDrawCmdList::GetTopBrushDrawCmd() 
	{
		return GetOrNewDrawCmd(GetCurrentBrush());
	}
	void FCanvasDrawCmdList::PushFont(FTFont* font)
	{
		if (font == nullptr)
			return;
		mFonts.push(font);
	}
	void FCanvasDrawCmdList::PopFont()
	{
		mFonts.pop();
	}
	FTFont* FCanvasDrawCmdList::GetCurrentFont()
	{
		if (mFonts.empty())
			return nullptr;
		return mFonts.top();
	}

	void FCanvasDrawCmdList::PushMatrix(const v3dxMatrix4* matrix)
	{
		if (matrix == nullptr)
			return;
		mMatrixes.push(*matrix);
	}
	void FCanvasDrawCmdList::PopMatrix()
	{
		mMatrixes.pop();
	}
	const v3dxMatrix4* FCanvasDrawCmdList::GetCurrentMatrix() const
	{
		if (mMatrixes.empty())
			return nullptr;
		return &mMatrixes.top();
	}

	void FCanvasDrawCmdList::PushPathStyle(Path::FPathStyle* PathStyle)
	{
		if (PathStyle == nullptr)
			return;
		mPathStyles.push(PathStyle);
	}
	void FCanvasDrawCmdList::PopPathStyle()
	{
		return mPathStyles.pop();
	}

	void FCanvasDrawCmdList::AddText(const WCHAR* text, int charCount, float x, float y, const FColor& color, IBlobObject* pOutCmds)
	{
		if (charCount == 0)
		{
			charCount = (int)wcslen(text);
		}
		else
		{
			charCount = std::min((int)wcslen(text), charCount);
		}
		const auto& clip = GetCurrentClipRect();
		FCanvasVertex vert[4];
		ICanvasBrush* prevBrush = nullptr;
		FDrawCmd* pCmd = nullptr;
		v3dxVector3 offset(x, y, 0);
		auto rightClip = clip.GetRight();

		auto font = GetCurrentFont();
		FColor rgba = color;
		for (int i = 0; i < charCount; i++)
		{
			auto c = text[i];
			auto word = font->GetWord(0, 0, c, vert);
			ASSERT(word != nullptr);
			if (word == nullptr)
				continue;
			
			auto PixelY = (word->PixelY);
			FRectanglef wordRect(offset.x + word->PixelX, offset.y + PixelY, (float)word->PixelWidth, (float)word->PixelHeight);
			if (clip.IsContain(wordRect))
			{
				if (prevBrush != word->Brush)
				{
					prevBrush = word->Brush;
					pCmd = GetOrNewDrawCmd(prevBrush);
				}
				pCmd->PushQuad(vert, offset);
			}
			else if (clip.IsOverlap(wordRect) == false)
			{

			}
			else
			{
				if (prevBrush != word->Brush)
				{
					prevBrush = word->Brush;
					pCmd = GetOrNewDrawCmd(prevBrush);
				}
				auto rect = FRectanglef::And(clip, wordRect);
				float u = vert[Canvas::RCN_X0_Y0].UV.x;
				float u1 = ((rect.X - offset.x - word->PixelX) - u) / word->PixelWidth;
				float u2 = ((rect.X + rect.Width - offset.x - word->PixelX) - u) / word->PixelWidth;
				float us = vert[Canvas::RCN_X1_Y0].UV.x - u;
				float v = vert[Canvas::RCN_X0_Y0].UV.y;
				float v1 = ((rect.Y - offset.y - word->PixelY) - v) / word->PixelHeight;
				float v2 = ((rect.Y + rect.Height - offset.y - word->PixelY) - v) / word->PixelHeight;
				float vs = vert[Canvas::RCN_X0_Y1].UV.y - v;
				vert[Canvas::RCN_X0_Y0].UV.x = u + us * u1;
				vert[Canvas::RCN_X0_Y1].UV.x = u + us * u1;
				vert[Canvas::RCN_X0_Y0].UV.y = v + vs * v1;
				vert[Canvas::RCN_X0_Y1].UV.y = v + vs * v2;

				vert[Canvas::RCN_X1_Y0].UV.x = u + us * u2;
				vert[Canvas::RCN_X1_Y1].UV.x = u + us * u2;
				vert[Canvas::RCN_X1_Y0].UV.y = v + vs * v1;
				vert[Canvas::RCN_X1_Y1].UV.y = v + vs * v2;

				vert[Canvas::RCN_X0_Y0].Pos.setValue(rect.Get_X0_Y0());
				vert[Canvas::RCN_X1_Y0].Pos.setValue(rect.Get_X1_Y0());
				vert[Canvas::RCN_X0_Y1].Pos.setValue(rect.Get_X0_Y1());
				vert[Canvas::RCN_X1_Y1].Pos.setValue(rect.Get_X1_Y1());
				
				pCmd->PushQuad(vert);
			}
			
			offset.x += word->Advance.x;
		}
	}
	void FCanvasDrawCmdList::AddLine(const v3dxVector2& s, const v3dxVector2& e, float width, const FColor& color, FSubDrawCmd* pOutCmd)
	{
		const auto& clip = GetCurrentClipRect();

		v3dxVector2 start, end;
		if (clip.ClipLine(s, e, &start, &end) == false)
			return;

		auto brush = GetCurrentBrush();
		auto& uv = brush->Rect;

		auto dir = end - start;
		dir = FUtility::RotateVector(dir, 1.5707963267949f);
		dir.normalize();

		auto halfWidth = width * 0.5f;
		auto ext = dir * halfWidth;

		auto pCmd = GetTopBrushDrawCmd();
		auto matrix =  GetCurrentMatrix();

		FColor rgba = color;
		FCanvasVertex vert[4];
		auto pos = start - ext;
		auto& v1 = vert[RCN_X0_Y0];
		v1.Pos.setValue(pos.x, pos.y, 0);
		if (matrix != nullptr)
			v3dxVec3TransformCoord(&v1.Pos, &v1.Pos, matrix);
		v1.Color = rgba;
		v1.UV = uv.Get_X0_Y0();

		pos = start + ext;
		auto& v2 = vert[RCN_X0_Y1];
		v2.Pos.setValue(pos.x, pos.y, 0);
		if (matrix != nullptr)
			v3dxVec3TransformCoord(&v2.Pos, &v2.Pos, matrix);
		v2.Color = rgba;
		v2.UV = uv.Get_X0_Y1();

		pos = end + ext;
		auto& v3 = vert[RCN_X1_Y1];
		v3.Pos.setValue(pos.x, pos.y, 0);
		if (matrix != nullptr)
			v3dxVec3TransformCoord(&v3.Pos, &v3.Pos, matrix);
		v3.Color = rgba;
		v3.UV = uv.Get_X1_Y1();

		pos = end - ext;
		auto& v4 = vert[RCN_X1_Y0];
		v4.Pos.setValue(pos.x, pos.y, 0);
		if (matrix != nullptr)
			v3dxVec3TransformCoord(&v4.Pos, &v4.Pos, matrix);
		v4.Color = rgba;
		v4.UV = uv.Get_X1_Y0();

		//todo: Clip quad with rect
		if (pOutCmd)
		{
			pOutCmd->DrawCmd = pCmd;
			pOutCmd->VertexStart = (UINT)pCmd->mVertices.size();
			pOutCmd->IndexStart = (UINT)pCmd->mIndices.size();

			pCmd->PushQuad(vert);

			pOutCmd->Count = (UINT)pCmd->mVertices.size() - pOutCmd->VertexStart;
			pOutCmd->IndexCount = (UINT)pCmd->mIndices.size() - pOutCmd->IndexStart;
		}
		else
		{
			pCmd->PushQuad(vert);
		}
	}
	void FCanvasDrawCmdList::AddLineStrips(const v3dxVector2* pPoints, UINT num, float width, const FColor& color, bool loop, FSubDrawCmd* pOutCmd)
	{
		// Check num > 1 TODO..
		for (int i = 0; i < (int)num - 1; i++)
		{
			AddLine(pPoints[i], pPoints[i + 1], width, color);
		}

		if (loop)
		{
			AddLine(pPoints[num - 1], pPoints[0], width, color);
		}
	}
	void FCanvasDrawCmdList::AddImage(ICanvasBrush* image, float x, float y, float w, float h, const FColor& color, FSubDrawCmd* pOutCmd)
	{
		const auto& clip = GetCurrentClipRect();
		auto matrix = GetCurrentMatrix();

		FColor rgba = color;
		FCanvasVertex vert[4];
		auto& v1 = vert[RCN_X0_Y0];
		v1.Pos.setValue(x, y, 0);
		if (matrix != nullptr)
			v3dxVec3TransformCoord(&v1.Pos, &v1.Pos, matrix);
		v1.Color = rgba;
		v1.UV = image->Rect.Get_X0_Y0();

		auto& v2 = (vert[RCN_X0_Y1]);
		v2.Pos.setValue(x, y + h, 0);
		if (matrix != nullptr)
			v3dxVec3TransformCoord(&v2.Pos, &v2.Pos, matrix);
		v2.Color = rgba;
		v2.UV = image->Rect.Get_X0_Y1();

		auto& v3 = (vert[RCN_X1_Y1]);
		v3.Pos.setValue(x + w, y + h, 0);
		if (matrix != nullptr)
			v3dxVec3TransformCoord(&v3.Pos, &v3.Pos, matrix);
		v3.Color = rgba;
		v3.UV = image->Rect.Get_X1_Y1();

		auto& v4 = (vert[RCN_X1_Y0]);
		v4.Pos.setValue(x + w, y, 0);
		if (matrix != nullptr)
			v3dxVec3TransformCoord(&v4.Pos, &v4.Pos, matrix);
		v4.Color = rgba;
		v4.UV = image->Rect.Get_X1_Y0();

		FRectanglef imgRect(x, y, w, h);
		if (clip.IsContain(imgRect))
		{
			auto pCmd = GetOrNewDrawCmd(image);
			if (pOutCmd)
			{
				pOutCmd->DrawCmd = pCmd;
				pOutCmd->VertexStart = (UINT)pCmd->mVertices.size();
				pOutCmd->IndexStart = (UINT)pCmd->mIndices.size();
				pCmd->PushQuad(vert);
				pOutCmd->Count = (UINT)pCmd->mVertices.size() - pOutCmd->VertexStart;
				pOutCmd->IndexCount = (UINT)pCmd->mIndices.size() - pOutCmd->IndexStart;
			}
			else
			{
				pCmd->PushQuad(vert);
			}
		}
		else if (clip.IsOverlap(imgRect) == false)
		{
			pOutCmd->DrawCmd = nullptr;
		}
		else
		{
			auto pCmd = GetOrNewDrawCmd(image);
			auto rect = FRectanglef::And(clip, imgRect);
			float u = vert[Canvas::RCN_X0_Y0].UV.x;
			float u1 = ((rect.X - x) - u) / w;
			float u2 = ((rect.X + rect.Width - x) - u) / w;
			float us = vert[Canvas::RCN_X1_Y0].UV.x - u;
			float v = vert[Canvas::RCN_X0_Y0].UV.y;
			float v1 = ((rect.Y - y) - v) / h;
			float v2 = ((rect.Y + rect.Height - y) - v) / h;
			float vs = vert[Canvas::RCN_X0_Y1].UV.y - v;
			vert[Canvas::RCN_X0_Y0].UV.x = u + us * u1;
			vert[Canvas::RCN_X0_Y1].UV.x = u + us * u1;
			vert[Canvas::RCN_X0_Y0].UV.y = v + vs * v1;
			vert[Canvas::RCN_X0_Y1].UV.y = v + vs * v2;

			vert[Canvas::RCN_X1_Y0].UV.x = u + us * u2;
			vert[Canvas::RCN_X1_Y1].UV.x = u + us * u2;
			vert[Canvas::RCN_X1_Y0].UV.y = v + vs * v1;
			vert[Canvas::RCN_X1_Y1].UV.y = v + vs * v2;

			vert[Canvas::RCN_X0_Y0].Pos.setValue(rect.Get_X0_Y0());
			vert[Canvas::RCN_X1_Y0].Pos.setValue(rect.Get_X1_Y0());
			vert[Canvas::RCN_X0_Y1].Pos.setValue(rect.Get_X0_Y1());
			vert[Canvas::RCN_X1_Y1].Pos.setValue(rect.Get_X1_Y1());

			if (pOutCmd)
			{
				pOutCmd->DrawCmd = pCmd;
				pOutCmd->VertexStart = (UINT)pCmd->mVertices.size();
				pOutCmd->IndexStart = (UINT)pCmd->mIndices.size();
				pCmd->PushQuad(vert);
				pOutCmd->Count = (UINT)pCmd->mVertices.size() - pOutCmd->VertexStart;
				pOutCmd->IndexCount = (UINT)pCmd->mIndices.size() - pOutCmd->IndexStart;
			}
			else
			{
				pCmd->PushQuad(vert);
			}
		}
	}

	void FCanvasDrawCmdList::AddRectFill(const v3dxVector2& s, const v3dxVector2& e, const FColor& color, FSubDrawCmd* pOutCmd)
	{
		const auto& clip = GetCurrentClipRect();

		v3dxVector2 start, end;
		//Need clip TODO..
		start = s;
		end = e;

		auto brush = GetCurrentBrush();
		auto& uv = brush->Rect;

		auto pCmd = GetTopBrushDrawCmd();
		auto matrix = GetCurrentMatrix();

		FColor rgba = color;
		FCanvasVertex vert[4];

		auto pos = start;
		auto& v1 = vert[RCN_X0_Y0];
		v1.Pos.setValue(pos.x, pos.y, 0);
		if (matrix != nullptr)
			v3dxVec3TransformCoord(&v1.Pos, &v1.Pos, matrix);
		v1.Color = rgba;
		v1.UV = uv.Get_X0_Y0();

		pos = v3dxVector2(end.x, start.y);
		auto& v2 = vert[RCN_X0_Y1];
		v2.Pos.setValue(pos.x, pos.y, 0);
		if (matrix != nullptr)
			v3dxVec3TransformCoord(&v2.Pos, &v2.Pos, matrix);
		v2.Color = rgba;
		v2.UV = uv.Get_X0_Y1();

		pos = end;
		auto& v3 = vert[RCN_X1_Y1];
		v3.Pos.setValue(pos.x, pos.y, 0);
		if (matrix != nullptr)
			v3dxVec3TransformCoord(&v3.Pos, &v3.Pos, matrix);
		v3.Color = rgba;
		v3.UV = uv.Get_X1_Y1();

		pos = v3dxVector2(start.x, end.y);
		auto& v4 = vert[RCN_X1_Y0];
		v4.Pos.setValue(pos.x, pos.y, 0);
		if (matrix != nullptr)
			v3dxVec3TransformCoord(&v4.Pos, &v4.Pos, matrix);
		v4.Color = rgba;
		v4.UV = uv.Get_X1_Y0();

		//todo: Clip quad with rect
		if (pOutCmd)
		{
			pOutCmd->DrawCmd = pCmd;
			pOutCmd->VertexStart = (UINT)pCmd->mVertices.size();
			pOutCmd->IndexStart = (UINT)pCmd->mIndices.size();

			pCmd->PushQuad(vert);

			pOutCmd->Count = (UINT)pCmd->mVertices.size() - pOutCmd->VertexStart;
			pOutCmd->IndexCount = (UINT)pCmd->mIndices.size() - pOutCmd->IndexStart;
		}
		else
		{
			pCmd->PushQuad(vert);
		}
	}

	void FCanvasDrawCmdList::AddRectLine(const v3dxVector2& s, const v3dxVector2& e, float width, const FColor& color, FSubDrawCmd* pOutCmd)
	{
		v3dxVector2 start, end;
		//Need clip TODO..
		start = s;
		end = e;

		float halfwidth = width * 0.5f;
		AddLine(v3dxVector2(start.x - halfwidth, start.y), v3dxVector2(end.x + halfwidth, start.y), width, color, pOutCmd);
		AddLine(v3dxVector2(end.x, start.y), end, width, color, pOutCmd);
		AddLine(v3dxVector2(end.x + halfwidth, end.y), v3dxVector2(start.x - halfwidth, end.y), width, color, pOutCmd);
		AddLine(v3dxVector2(start.x, end.y), start, width, color, pOutCmd);
	}

	void FCanvasDrawCmdList::Reset()
	{
		mClipRects.clear();
		mDrawCmds.clear();
		mStopCmdIndex = 0;

		mCurrentDrawCmd = nullptr;
		mFonts = std::stack<AutoRef<FTFont>>();
		mMatrixes = std::stack<v3dxMatrix4>();
		mBrushes = std::stack<AutoRef<ICanvasBrush>>();
		mPathStyles = std::stack<AutoRef<Path::FPathStyle>>();

		auto pDefaultBrush = MakeWeakRef(new ICanvasBrush());
		pDefaultBrush->Rect = FRectanglef(0, 0, 1, 1);
		pDefaultBrush->Name = VNameString("DefaultBrush");
		mBrushes.push(pDefaultBrush);
	}

	void FCanvas::PushCmdList(FCanvasDrawCmdList* cmdlist, NxRHI::FMeshDataProvider* mesh)
	{
		for (auto& j : cmdlist->mDrawCmds)
		{
			NxRHI::FMeshAtomDesc desc{};
			desc.BaseVertexIndex = mesh->GetVertexNumber();
			desc.StartIndex = mesh->GetPrimitiveNumber() * 3;
			desc.NumPrimitives = (UINT)j->mIndices.size() / 3;

			//todo: AddVertex_Pos_UV_Color optimize
			UINT VertexNumber = 0;
			/*for (auto& k : j->mVertices)
			{
				mesh->AddVertex(&k.Pos, nullptr, &k.UV, k.Color.Value);
			}
			VertexNumber += (UINT)j->mVertices.size();*/
			UINT numOfVert = (UINT)j->mVertices.size();
			if (numOfVert > 0)
			{
				mesh->AddVertex_Pos_UV_Color(&j->mVertices[0], numOfVert, InvertY, ClientRect.Height);
				VertexNumber += numOfVert;
			}

			auto& pIndices = j->mIndices;
			/*for (UINT k = 0; k < (UINT)pIndices.size(); k += 3)
			{
				mesh->AddTriangle(pIndices[k], pIndices[k + 1], pIndices[k + 2]);
			}*/
			UINT numOfIndices = (UINT)pIndices.size();
			if (numOfIndices > 0)
			{
				mesh->AddTriangle(&pIndices[0], numOfIndices / 3);
			}

			if (pIndices.size() > 0 && VertexNumber > 0)
			{
				mesh->PushAtom(&desc, 1, j);
			}
		}
	}
}

NS_END