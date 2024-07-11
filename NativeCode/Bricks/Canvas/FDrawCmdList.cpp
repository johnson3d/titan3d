#include "FDrawCmdList.h"
#include "FCanvas.h"
#include "FTFont.h"
#include "../../Graphics/Mesh/MeshDataProvider.h"

#define new VNEW

NS_BEGIN

#define SMALLRADLINE 5
#define GETUVFROMPOS(pos) v3dxVector2(pos.X / size.X * uvRect.Width + uvRect.X, pos.Y / size.Y * uvRect.Height + uvRect.Y)

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
		pDefaultBrush->Name = VNameString("@MatInst:DefaultBrush");
		mBrushes.push(pDefaultBrush);
		// todo: default font
		// mFonts.push()
	}
	FDrawCmd* FCanvasDrawCmdList::GetOrNewDrawCmd(ICanvasBrush* brush, bool& isNewOne)
	{
		isNewOne = false;
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
		isNewOne = true;
		return NewDrawCmd(brush);
	}
	FDrawCmd* FCanvasDrawCmdList::NewDrawCmd(ICanvasBrush* brush)
	{
		auto tmp = MakeWeakRef(new FDrawCmd());
		tmp->mBrush = brush;
		tmp->Batch = Batch;
		mDrawCmds.push_back(tmp);
		mCurrentDrawCmd = tmp;
		return tmp;
	}
	void FCanvasDrawCmdList::TransformIndexToColor(const UInt16* index, FColor& color)
	{
		if (index == nullptr)
		{
			color.r = 0;
			color.a = 0;
		}
		else
		{
			color.r = (*index) >> 8;
			color.a = (*index) & 0xFF;
		}
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
		mClipRects.pop_back();
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
		bool isNewOne;
		return GetOrNewDrawCmd(GetCurrentBrush(), isNewOne);
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

	void FCanvasDrawCmdList::PushTransformIndex(const UInt16* index)
	{
		if (index == nullptr)
			return;
		mTransformIndexes.push(*index);
	}
	void FCanvasDrawCmdList::PopTransformIndex()
	{
		mTransformIndexes.pop();
	}
	const UInt16* FCanvasDrawCmdList::GetCurrentTransformIndex() const
	{
		if (mTransformIndexes.empty())
			return nullptr;
		return &mTransformIndexes.top();
	}
	void FCanvasDrawCmdList::ClearTransformIndex()
	{
		std::stack<UInt16>().swap(mTransformIndexes);
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

	void FCanvasDrawCmdList::AddText(const WCHAR* text, int charCount, float x, float y, const FDrawCmdInstanceData& insData, IBlobObject* pOutCmds)
	{
		if (charCount == 0)
		{
			charCount = (int)wcslen(text);
		}
		else
		{
			charCount = std::min((int)wcslen(text), charCount);
		}
		auto transIndex = GetCurrentTransformIndex();
		const auto& clip = GetCurrentClipRect();
		auto matrix = GetCurrentMatrix();
		FCanvasVertex vert[4];
		ICanvasBrush* prevBrush = nullptr;
		FDrawCmd* pCmd = nullptr;
		v3dxVector3 offset(x, y, 0);
		auto rightClip = clip.GetRight();
		v3dxMatrix4 moveMat, moveMat2;
		moveMat.moveMatrix(-offset.X, -offset.Y, 0.0f);
		moveMat2.moveMatrix(offset.X, offset.Y, 0.0f);
		moveMat = moveMat * (*matrix) * moveMat2;

		int oldCount = 0;
		char* dataPtr = (char*)pOutCmds->GetData();
		std::vector<FDrawCmd*> pOldCmds;
		if (pOutCmds != nullptr)
		{
			int ptrOffset = 0;
			memcpy(&oldCount, dataPtr, sizeof(int));
			ptrOffset += sizeof(int);
			int startIdx = oldCount;
			memcpy(dataPtr + ptrOffset, &startIdx, sizeof(int));
			ptrOffset += sizeof(int);
			for (int i = 0; i < oldCount; i++)
			{
				//ptrOffset += sizeof(void*);
				FDrawCmd* cmdPtr = (FDrawCmd*)(*(void**)(dataPtr + ptrOffset));
				int cmdIdx = 0;
				for (cmdIdx = 0; cmdIdx < pOldCmds.size(); cmdIdx++)
				{
					if (pOldCmds[cmdIdx] == cmdPtr)
						break;
				}
				if(cmdIdx >= pOldCmds.size())
					pOldCmds.push_back(cmdPtr);
				ptrOffset += sizeof(void*);
			}
			if (oldCount > 0)
			{
				pOutCmds->ReSize(pOutCmds->GetSize() - (sizeof(void*)));
			}
		}

		auto font = GetCurrentFont();
		for (int i = 0; i < charCount; i++)
		{
			auto c = text[i];
			auto word = font->GetWord(0, 0, c, *transIndex, vert);
			//ASSERT(word != nullptr);
			if (word == nullptr)
				continue;

			v3dxVector3 vMin(FLT_MAX, FLT_MAX, FLT_MAX), vMax(-FLT_MAX, -FLT_MAX, -FLT_MAX);
			for (int vIdx = 0; vIdx < 4; vIdx++)
			{
				auto tempPos = vert[vIdx].Pos + offset;
				v3dxVec3TransformCoord(&vert[vIdx].Pos, &tempPos, &moveMat);
				vMin.X = std::min(vMin.X, vert[vIdx].Pos.X);
				vMin.Y = std::min(vMin.Y, vert[vIdx].Pos.Y);
				vMin.Z = std::min(vMin.Z, vert[vIdx].Pos.Z);
				vMax.X = std::max(vMax.X, vert[vIdx].Pos.X);
				vMax.Y = std::max(vMax.Y, vert[vIdx].Pos.Y);
				vMax.Z = std::max(vMax.Z, vert[vIdx].Pos.Z);
			}
			
			FRectanglef wordRect(vMin.X, vMin.Y, vMax.X - vMin.X, vMax.Y - vMin.Y);
			if (clip.IsContain(wordRect))
			{
				if (prevBrush != word->Brush)
				{
					prevBrush = word->Brush;
					bool isNewOne = false;
					pCmd = GetOrNewDrawCmd(prevBrush, isNewOne);
					if(isNewOne)
						pCmd->SetInstanceData(insData);
					else if(!pCmd->InstanceData.IsEqual(insData))
					{
						pCmd = NewDrawCmd(prevBrush);
						pCmd->SetInstanceData(insData);
					}
					if (pOutCmds != nullptr)
					{
						//pOutCmds->PushData(&prevBrush, sizeof(prevBrush));
						pOutCmds->PushData(&pCmd, sizeof(pCmd));
						oldCount++;
					}
					bool isFind = false;
					for (auto oldCmd : pOldCmds)
					{
						if (oldCmd == pCmd)
						{
							isFind = true;
							break;
						}
					}
					if (!isFind)
					{
						pOldCmds.push_back(pCmd);
						pCmd->DrawCount++;
					}
				}
				pCmd->PushQuad(vert);
			}
			else if (clip.IsOverlap(wordRect) == false)
			{

			}
			else
			{
				if (prevBrush != word->Brush)
				{
					prevBrush = word->Brush;
					bool isNewOne = false;
					pCmd = GetOrNewDrawCmd(prevBrush, isNewOne);
					if(isNewOne)
						pCmd->SetInstanceData(insData);
					else if (!pCmd->InstanceData.IsEqual(insData))
					{
						pCmd = NewDrawCmd(prevBrush);
						pCmd->SetInstanceData(insData);
					}
					if (pOutCmds != nullptr)
					{
						//pOutCmds->PushData(&prevBrush, sizeof(prevBrush));
						pOutCmds->PushData(&pCmd, sizeof(pCmd));
						oldCount++;
					}
					bool isFind = false;
					for (auto oldCmd : pOldCmds)
					{
						if (oldCmd == pCmd)
						{
							isFind = true;
							break;
						}
					}
					if (!isFind)
					{
						pOldCmds.push_back(pCmd);
						pCmd->DrawCount++;
					}
				}
				auto rect = FRectanglef::And(clip, wordRect);
				float u = vert[Canvas::RCN_X0_Y0].UV.X;
				float u1 = (rect.X - wordRect.X) / wordRect.Width;
				float u2 = (rect.X + rect.Width - wordRect.X) / wordRect.Width;
				float us = vert[Canvas::RCN_X1_Y0].UV.X - u;
				float v = vert[Canvas::RCN_X0_Y0].UV.Y;
				float v1 = (rect.Y - wordRect.Y) / wordRect.Height;
				float v2 = (rect.Y + rect.Height - wordRect.Y) / wordRect.Height;
				float vs = vert[Canvas::RCN_X0_Y1].UV.Y - v;
				vert[Canvas::RCN_X0_Y0].UV.X = u + us * u1;
				vert[Canvas::RCN_X0_Y1].UV.X = u + us * u1;
				vert[Canvas::RCN_X0_Y0].UV.Y = v + vs * v1;
				vert[Canvas::RCN_X0_Y1].UV.Y = v + vs * v2;

				vert[Canvas::RCN_X1_Y0].UV.X = u + us * u2;
				vert[Canvas::RCN_X1_Y1].UV.X = u + us * u2;
				vert[Canvas::RCN_X1_Y0].UV.Y = v + vs * v1;
				vert[Canvas::RCN_X1_Y1].UV.Y = v + vs * v2;

				vert[Canvas::RCN_X0_Y0].Pos.setValue(rect.Get_X0_Y0());
				vert[Canvas::RCN_X1_Y0].Pos.setValue(rect.Get_X1_Y0());
				vert[Canvas::RCN_X0_Y1].Pos.setValue(rect.Get_X0_Y1());
				vert[Canvas::RCN_X1_Y1].Pos.setValue(rect.Get_X1_Y1());

				TransformIndexToColor(transIndex, vert[Canvas::RCN_X0_Y0].Index);
				TransformIndexToColor(transIndex, vert[Canvas::RCN_X1_Y0].Index);
				TransformIndexToColor(transIndex, vert[Canvas::RCN_X0_Y1].Index);
				TransformIndexToColor(transIndex, vert[Canvas::RCN_X1_Y1].Index);
				
				pCmd->PushQuad(vert);
			}
			
			offset.X += word->Advance.X;
			offset.Y += word->Advance.Y;
			//v3dxVector3 advance(word->Advance.x, word->Advance.y, 0.0f);
			//v3dxVec3TransformCoord(&advance, &advance, &matrix);
			//offset.x += advance.x;
			//offset.y += advance.y;
		}

		if (pOutCmds != nullptr)
		{
			void* temp = (void*)1;
			//pOutCmds->PushData(&temp, sizeof(void*));
			pOutCmds->PushData(&temp, sizeof(void*));
			pOutCmds->SetValueToOffset(0, oldCount);
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
		pCmd->DrawCount++;
		auto matrix =  GetCurrentMatrix();
		auto index = GetCurrentTransformIndex();

		FColor rgba = color;
		FCanvasVertex vert[4];
		auto pos = start - ext;
		auto& v1 = vert[RCN_X0_Y0];
		v1.Pos.setValue(pos.X, pos.Y, 0);
		if (matrix != nullptr)
			v3dxVec3TransformCoord(&v1.Pos, &v1.Pos, matrix);
		v1.Color = rgba;
		v1.UV = uv.Get_X0_Y0();
		TransformIndexToColor(index, v1.Index);

		pos = start + ext;
		auto& v2 = vert[RCN_X0_Y1];
		v2.Pos.setValue(pos.X, pos.Y, 0);
		if (matrix != nullptr)
			v3dxVec3TransformCoord(&v2.Pos, &v2.Pos, matrix);
		v2.Color = rgba;
		v2.UV = uv.Get_X0_Y1();
		TransformIndexToColor(index, v2.Index);

		pos = end + ext;
		auto& v3 = vert[RCN_X1_Y1];
		v3.Pos.setValue(pos.X, pos.Y, 0);
		if (matrix != nullptr)
			v3dxVec3TransformCoord(&v3.Pos, &v3.Pos, matrix);
		v3.Color = rgba;
		v3.UV = uv.Get_X1_Y1();
		TransformIndexToColor(index, v3.Index);

		pos = end - ext;
		auto& v4 = vert[RCN_X1_Y0];
		v4.Pos.setValue(pos.X, pos.Y, 0);
		if (matrix != nullptr)
			v3dxVec3TransformCoord(&v4.Pos, &v4.Pos, matrix);
		v4.Color = rgba;
		v4.UV = uv.Get_X1_Y0();
		TransformIndexToColor(index, v4.Index);

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
		auto index = GetCurrentTransformIndex();

		FColor rgba = color;
		FCanvasVertex vert[4];
		auto& v1 = vert[RCN_X0_Y0];
		v1.Pos.setValue(x, y, 0);
		if (matrix != nullptr)
			v3dxVec3TransformCoord(&v1.Pos, &v1.Pos, matrix);
		v1.Color = rgba;
		v1.UV = image->Rect.Get_X0_Y0();
		TransformIndexToColor(index, v1.Index);

		auto& v2 = (vert[RCN_X0_Y1]);
		v2.Pos.setValue(x, y + h, 0);
		if (matrix != nullptr)
			v3dxVec3TransformCoord(&v2.Pos, &v2.Pos, matrix);
		v2.Color = rgba;
		v2.UV = image->Rect.Get_X0_Y1();
		TransformIndexToColor(index, v2.Index);

		auto& v3 = (vert[RCN_X1_Y1]);
		v3.Pos.setValue(x + w, y + h, 0);
		if (matrix != nullptr)
			v3dxVec3TransformCoord(&v3.Pos, &v3.Pos, matrix);
		v3.Color = rgba;
		v3.UV = image->Rect.Get_X1_Y1();
		TransformIndexToColor(index, v3.Index);

		auto& v4 = (vert[RCN_X1_Y0]);
		v4.Pos.setValue(x + w, y, 0);
		if (matrix != nullptr)
			v3dxVec3TransformCoord(&v4.Pos, &v4.Pos, matrix);
		v4.Color = rgba;
		v4.UV = image->Rect.Get_X1_Y0();
		TransformIndexToColor(index, v4.Index);

		FRectanglef imgRect(x, y, w, h);
		if (clip.IsContain(imgRect))
		{
			bool isNewOne;
			auto pCmd = GetOrNewDrawCmd(image, isNewOne);
			pCmd->DrawCount++;
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
			bool isNewOne;
			auto pCmd = GetOrNewDrawCmd(image, isNewOne);
			pCmd->DrawCount++;
			auto rect = FRectanglef::And(clip, imgRect);
			float u = vert[Canvas::RCN_X0_Y0].UV.X;
			float u1 = ((rect.X - x) - u) / w;
			float u2 = ((rect.X + rect.Width - x) - u) / w;
			float us = vert[Canvas::RCN_X1_Y0].UV.X - u;
			float v = vert[Canvas::RCN_X0_Y0].UV.Y;
			float v1 = ((rect.Y - y) - v) / h;
			float v2 = ((rect.Y + rect.Height - y) - v) / h;
			float vs = vert[Canvas::RCN_X0_Y1].UV.Y - v;
			vert[Canvas::RCN_X0_Y0].UV.X = u + us * u1;
			vert[Canvas::RCN_X0_Y1].UV.X = u + us * u1;
			vert[Canvas::RCN_X0_Y0].UV.Y = v + vs * v1;
			vert[Canvas::RCN_X0_Y1].UV.Y = v + vs * v2;

			vert[Canvas::RCN_X1_Y0].UV.X = u + us * u2;
			vert[Canvas::RCN_X1_Y1].UV.X = u + us * u2;
			vert[Canvas::RCN_X1_Y0].UV.Y = v + vs * v1;
			vert[Canvas::RCN_X1_Y1].UV.Y = v + vs * v2;

			vert[Canvas::RCN_X0_Y0].Pos.setValue(rect.Get_X0_Y0());
			vert[Canvas::RCN_X1_Y0].Pos.setValue(rect.Get_X1_Y0());
			vert[Canvas::RCN_X0_Y1].Pos.setValue(rect.Get_X0_Y1());
			vert[Canvas::RCN_X1_Y1].Pos.setValue(rect.Get_X1_Y1());

			TransformIndexToColor(index, vert[Canvas::RCN_X0_Y0].Index);
			TransformIndexToColor(index, vert[Canvas::RCN_X1_Y0].Index);
			TransformIndexToColor(index, vert[Canvas::RCN_X0_Y1].Index);
			TransformIndexToColor(index, vert[Canvas::RCN_X1_Y1].Index);

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
		/*const auto& clip = GetCurrentClipRect();

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
		}*/
		v3dxVector4 cornerRadius(0);
		AddRectFill(s, e, cornerRadius, color, pOutCmd);
	}
	static void CalculateCornerVertices(int sliceCount, float radiusStart, float radius, const v3dxVector2& cornerCenter, int vectorStartIdx, std::vector<v3dxVector2>& vectices)
	{
		const float delta = (sliceCount > 0) ? Math::HALF_PI / sliceCount : 0.0f;
		for (int i = 0; i <= sliceCount; i++)
		{
			auto rad = delta * i + radiusStart;
			auto cosVal = std::cos(rad) * radius;
			auto sinVal = std::sin(rad) * radius;
			vectices[vectorStartIdx + i].setValue(cornerCenter.X + cosVal, cornerCenter.Y - sinVal);
		}

	}
	void FCanvasDrawCmdList::AddRectFill(const FRectanglef& rect, const v3dxVector4& cornerRadius, const FColor& color, FSubDrawCmd* pOutCmd)
	{
		AddRectFill(v3dxVector2(rect.X, rect.Y), v3dxVector2(rect.GetRight(), rect.GetBottom()), cornerRadius, color, pOutCmd);
	}
	void FCanvasDrawCmdList::AddRectFill(const v3dxVector2& s, const v3dxVector2& e, const v3dxVector4& cornerRadius, const FColor& color, FSubDrawCmd* pOutCmd)
	{
		const auto& clip = GetCurrentClipRect();
		v3dxVector2 min = v3dxVector2(std::min(s.X, e.X), std::min(s.Y, e.Y));
		v3dxVector2 max = v3dxVector2(std::max(s.X, e.X), std::max(s.Y, e.Y));
		v3dxVector2 size = max - min;

		if (clip.IsValid() && ((clip.X > max.X) || (clip.Y > max.Y) || (clip.GetRight() < min.X) || (clip.GetBottom() < min.Y)))
			return;

		int topLeftCornerSliceCount = cornerRadius.topleft != 0 ? (int)Math::Ceil(Math::HALF_PI / (std::asinf(SMALLRADLINE / cornerRadius.topleft) * 2)) : 0;
		int topRightCornerSliceCount = cornerRadius.topright != 0 ? (int)Math::Ceil(Math::HALF_PI / (std::asinf(SMALLRADLINE / cornerRadius.topright) * 2)) : 0;
		int bottomRightCornerSliceCount = cornerRadius.bottomright != 0 ? (int)Math::Ceil(Math::HALF_PI / (std::asinf(SMALLRADLINE / cornerRadius.bottomright) * 2)) : 0;
		int bottomLeftCornerSliceCount = cornerRadius.bottomleft != 0 ? (int)Math::Ceil(Math::HALF_PI / (std::asinf(SMALLRADLINE / cornerRadius.bottomleft) * 2)) : 0;

		std::vector<v3dxVector2> tempVertices;
		tempVertices.resize(topLeftCornerSliceCount + topRightCornerSliceCount + bottomRightCornerSliceCount + bottomLeftCornerSliceCount + 4);
		int tempVerticesStartIdx = 0;
		v3dxVector2 bottomLeftCenter(min.X + cornerRadius.bottomleft, max.Y - cornerRadius.bottomleft);
		CalculateCornerVertices(bottomLeftCornerSliceCount, Math::V3_PI, cornerRadius.bottomleft, bottomLeftCenter, tempVerticesStartIdx, tempVertices);
		tempVerticesStartIdx += bottomLeftCornerSliceCount + 1;
		v3dxVector2 bottomRightCenter(max.X - cornerRadius.bottomright, max.Y - cornerRadius.bottomright);
		CalculateCornerVertices(bottomRightCornerSliceCount, Math::HALF_PI * 3, cornerRadius.bottomright, bottomRightCenter, tempVerticesStartIdx, tempVertices);
		tempVerticesStartIdx += bottomRightCornerSliceCount + 1;
		v3dxVector2 topRightCenter(max.X - cornerRadius.topright, min.Y + cornerRadius.topright);
		CalculateCornerVertices(topRightCornerSliceCount, 0, cornerRadius.topright, topRightCenter, tempVerticesStartIdx, tempVertices);
		tempVerticesStartIdx += topRightCornerSliceCount + 1;
		v3dxVector2 topLeftCenter(min.X + cornerRadius.topleft, min.Y + cornerRadius.topleft);
		CalculateCornerVertices(topLeftCornerSliceCount, Math::HALF_PI, cornerRadius.topleft, topLeftCenter, tempVerticesStartIdx, tempVertices);

		std::vector<v3dxVector2> newVerts;
		std::vector<UINT> newInds;
		if (clip.IsValid())
		{
			FPathUtility::SutherlandHodgmanClip(clip, tempVertices.data(), (UINT)tempVertices.size(), newVerts);
		}
		else
		{
			newVerts = tempVertices;
		}
		FPathUtility::Triangulate(newVerts.data(), (UINT)newVerts.size(), newInds);

		auto brush = GetCurrentBrush();
		auto pCmd = GetTopBrushDrawCmd();
		pCmd->DrawCount++;
		auto matrix = GetCurrentMatrix();
		auto index = GetCurrentTransformIndex();
		const auto& uvRect = brush->Rect;
		auto& vertices = pCmd->mVertices;
		auto& indices = pCmd->mIndices;

		UINT vst = (UINT)vertices.size();
		vertices.resize(vst + newVerts.size());
		for (UINT i = 0; i < newVerts.size(); i++)
		{
			auto& vt = vertices[vst + i];
			vt.Pos.setValue(newVerts[i].X, newVerts[i].Y, 0.0f);
			vt.Color = color;
			vt.UV = GETUVFROMPOS(vt.Pos);
			TransformIndexToColor(index, vt.Index);
		}
		UINT ist = (UINT)indices.size();
		indices.resize(ist + newInds.size());
		for (UINT i = 0; i < newInds.size(); i++)
		{
			indices[ist + i] = vst + newInds[i];
		}
	}

	void FCanvasDrawCmdList::AddRectLine(const v3dxVector2& s, const v3dxVector2& e, float width, const FColor& color, FSubDrawCmd* pOutCmd)
	{
		v3dxThickness thickness(width);
		v3dxVector4 cornerRadius(0);
		AddRectLine(s, e, thickness, cornerRadius, color, pOutCmd);
	}
	void FCanvasDrawCmdList::AddRectLine(const FRectanglef& rect, const v3dxThickness& thickness, const v3dxVector4& cornerRadius, const FColor& color, FSubDrawCmd* pOutCmd)
	{
		AddRectLine(v3dxVector2(rect.X, rect.Y), v3dxVector2(rect.X + rect.Width, rect.Y + rect.Height), thickness, cornerRadius, color, pOutCmd);
	}
	static void CalculateCorner(
		int sliceCount,
		float radiusStart,
		BYTE cornerType,
		const FRectanglef& clip,
		const v3dxVector2& min,
		const v3dxVector2& max,
		const v3dxVector2& size,
		std::vector<FCanvasVertex>& vertices,
		std::vector<UINT>& indices,
		const EngineNS::FRectanglef& uvRect, 
		const v3dxThickness& thickness,
		const v3dxVector4& cornerRadius,
		const FColor& color,
		const UInt16* transformIndex)
	{
		float cRadius = 0;
		v3dxVector2 cornerInCenter;
		v3dxVector2 cornerOutCenter;
		auto verticesCount = 4 + sliceCount * 2;
		std::vector<v3dxVector2> tempVertices;
		tempVertices.resize(verticesCount);

		switch (cornerType)
		{
		case 0: // bottom left
			cRadius = cornerRadius.bottomleft;
			cornerInCenter.setValue(min.X + cRadius + thickness.left, max.Y - cRadius - thickness.bottom);
			cornerOutCenter.setValue(min.X + cRadius, max.Y - cRadius);
			tempVertices[0].setValue(min.X, max.Y - thickness.bottom - cRadius);
			tempVertices[sliceCount + 2].setValue(min.X + thickness.left + cRadius, max.Y);
			break;
		case 1: // bottom right
			cRadius = cornerRadius.bottomright;
			cornerInCenter.setValue(max.X - cRadius - thickness.right, max.Y - cRadius - thickness.bottom);
			cornerOutCenter.setValue(max.X - cRadius, max.Y - cRadius);
			tempVertices[0].setValue(max.X - thickness.right - cRadius, max.Y);
			tempVertices[sliceCount + 2].setValue(max.X, max.Y - thickness.bottom - cRadius);
			break;
		case 2:	// top right
			cRadius = cornerRadius.topright;
			cornerInCenter.setValue(max.X - cRadius - thickness.right, min.Y + cRadius + thickness.top);
			cornerOutCenter.setValue(max.X - cRadius, min.Y + cRadius);
			tempVertices[0].setValue(max.X, min.Y + thickness.top + cRadius);
			tempVertices[sliceCount + 2].setValue(max.X - thickness.right - cRadius, min.Y);
			break;
		case 3:	// top left
			cRadius = cornerRadius.topleft;
			cornerInCenter.setValue(min.X + cRadius + thickness.left, min.Y + cRadius + thickness.top);
			cornerOutCenter.setValue(min.X + cRadius, min.Y + cRadius);
			tempVertices[0].setValue(min.X + thickness.left + cRadius, min.Y);
			tempVertices[sliceCount + 2].setValue(min.X, min.Y + thickness.top + cRadius);
			break;
		}

		auto vs = 1;
		const float delta = (sliceCount > 0) ? Math::HALF_PI / sliceCount : 0.0f;
		for (int i = 0; i <= sliceCount; i++)
		{
			auto radIn = delta * i + radiusStart;
			auto cosVal = std::cos(radIn) * cRadius;
			auto sinVal = std::sin(radIn) * cRadius;
			tempVertices[vs].setValue(cornerInCenter.X + cosVal, cornerInCenter.Y - sinVal);
			tempVertices[verticesCount - vs].setValue(cornerOutCenter.X + cosVal, cornerOutCenter.Y  - sinVal);
			vs++;
		}
		std::vector<UINT> tempInds;
		tempInds.resize((2 + sliceCount * 2) * 3);
		tempInds[0] = 0;
		tempInds[1] = 1;
		tempInds[2] = verticesCount - 1;
		auto tvs = 1;
		auto tis = 3;
		for (int i = 0; i < sliceCount; i++)
		{
			tempInds[tis + 0] = tvs;
			tempInds[tis + 1] = tvs + 1;
			tempInds[tis + 2] = verticesCount - (tvs + 1);
			tempInds[tis + 3] = tvs;
			tempInds[tis + 4] = verticesCount - (tvs + 1);
			tempInds[tis + 5] = verticesCount - (tvs);
			tvs++;
			tis += 6;
		}
		tempInds[tis] = sliceCount + 1;
		tempInds[tis + 1] = sliceCount + 2;
		tempInds[tis + 2] = sliceCount + 3;

		std::vector<v3dxVector2> newVerts;
		std::vector<UINT> newInds;
		auto rect = FPathUtility::CalcBounds(tempVertices.data(), (UINT)tempVertices.size());
		if (clip.IsValid() && clip.IsOverlap(rect))
		{
			auto triNum = tempInds.size() / 3;
			std::vector<v3dxVector2> calTriVec;
			calTriVec.resize(3);
			for (size_t i = 0; i < triNum; i++)
			{
				calTriVec[0] = tempVertices[tempInds[i * 3]];
				calTriVec[1] = tempVertices[tempInds[i * 3 + 1]];
				calTriVec[2] = tempVertices[tempInds[i * 3 + 2]];
				const UINT startIdx = (UINT)newVerts.size();
				FPathUtility::SutherlandHodgmanClip(clip, calTriVec.data(), 3, newVerts);
				FPathUtility::Triangulate(newVerts.data() + startIdx, (UINT)(newVerts.size() - startIdx), newInds, startIdx);
			}
		}
		else
		{
			newVerts = tempVertices;
			newInds = tempInds;
		}
		UINT vst = (UINT)vertices.size();
		vertices.resize(vst + newVerts.size());
		for (UINT i = 0; i < newVerts.size(); i++)
		{
			auto& vt = vertices[vst + i];
			vt.Pos.setValue(newVerts[i].X, newVerts[i].Y, 0.0f);
			vt.Color = color;
			vt.UV = GETUVFROMPOS(vt.Pos);
			FCanvasDrawCmdList::TransformIndexToColor(transformIndex, vt.Index);
		}
		UINT ist = (UINT)indices.size();
		indices.resize(ist + newInds.size());
		for (UINT i = 0; i < newInds.size(); i++)
		{
			indices[ist + i] = vst + newInds[i];
		}	
	}
	static void CalculateRect(
		const FRectanglef& rect, 
		const FRectanglef& clip, 
		const v3dxVector2& size, 
		const EngineNS::FRectanglef& uvRect, 
		FDrawCmd* pCmd, 
		const FColor& color,
		const UInt16* transformIndex)
	{
		FRectanglef clippedRect = rect;
		if (clip.IsValid())
		{
			if (clip.IsOverlap(rect))
				clippedRect = FRectanglef::And(clip, rect);
			else
				return;
		}
		FCanvasVertex p[4];
		p[0].Pos.setValue(clippedRect.X, clippedRect.Y, 0.0f);
		p[0].Color = color;
		p[0].UV = GETUVFROMPOS(p[0].Pos);
		FCanvasDrawCmdList::TransformIndexToColor(transformIndex, p[0].Index);
		p[1].Pos.setValue(clippedRect.GetRight(), clippedRect.Y, 0.0f);
		p[1].Color = color;
		p[1].UV = GETUVFROMPOS(p[1].Pos);
		FCanvasDrawCmdList::TransformIndexToColor(transformIndex, p[1].Index);
		p[2].Pos.setValue(clippedRect.X, clippedRect.GetBottom(), 0.0f);
		p[2].Color = color;
		p[2].UV = GETUVFROMPOS(p[2].Pos);
		FCanvasDrawCmdList::TransformIndexToColor(transformIndex, p[2].Index);
		p[3].Pos.setValue(clippedRect.GetRight(), clippedRect.GetBottom(), 0.0f);
		p[3].Color = color;
		p[3].UV = GETUVFROMPOS(p[3].Pos);
		FCanvasDrawCmdList::TransformIndexToColor(transformIndex, p[3].Index);
		pCmd->PushQuad(p);
	}
	void FCanvasDrawCmdList::AddRectLine(const v3dxVector2& s, const v3dxVector2& e, const v3dxThickness& thickness, const v3dxVector4& cornerRadius, const FColor& color, FSubDrawCmd* pOutCmd)
	{
		const auto& clip = GetCurrentClipRect();
		v3dxVector2 min = v3dxVector2(std::min(s.X, e.X), std::min(s.Y, e.Y));
		v3dxVector2 max = v3dxVector2(std::max(s.X, e.X), std::max(s.Y, e.Y));
		v3dxVector2 size = max - min;

		if (clip.IsValid() && ((clip.X > max.X) || (clip.Y > max.Y) || (clip.GetRight() < min.X) || (clip.GetBottom() < min.Y)))
			return;

		int topLeftCornerSliceCount = cornerRadius.topleft != 0 ? (int)Math::Ceil(Math::HALF_PI / (std::asinf(SMALLRADLINE / cornerRadius.topleft) * 2)) : 0;
		int topRightCornerSliceCount = cornerRadius.topright != 0 ? (int)Math::Ceil(Math::HALF_PI / (std::asinf(SMALLRADLINE / cornerRadius.topright) * 2)) : 0;
		int bottomRightCornerSliceCount = cornerRadius.bottomright != 0 ? (int)Math::Ceil(Math::HALF_PI / (std::asinf(SMALLRADLINE / cornerRadius.bottomright) * 2)) : 0;
		int bottomLeftCornerSliceCount = cornerRadius.bottomleft != 0 ? (int)Math::Ceil(Math::HALF_PI / (std::asinf(SMALLRADLINE / cornerRadius.bottomleft) * 2)) : 0;

		auto brush = GetCurrentBrush();
		auto pCmd = GetTopBrushDrawCmd();
		pCmd->DrawCount++;
		auto matrix = GetCurrentMatrix();
		auto index = GetCurrentTransformIndex();
		const auto& uvRect = brush->Rect;
		auto& vertices = pCmd->mVertices;
		auto& indices = pCmd->mIndices;
		auto vtStart = (UINT)vertices.size();
		auto idxStart = (UINT)indices.size();

		// 示例：实际顶点顺序略有差异
		//        /p26 ----------- p24 \
		//    p28   |                |   p22
		//    /    p27 ----------- p25     \
		//  p30   p29                p23    p20
		//  /    p31                  p21     \  
		// p0 - p1                     p19 - p18 
		// |    |                       |     |
		// |    |                       |     |
		// |    |                       |     |
		// p2 - p3                     p17 - p16
		//  \    p5                   p15    /
		//  p4     p7                p13    p14
		//    \     p9 ---------- p11      / 
		//     p6    |              |   p12
		//        \ p8 ---------- p10 /  

		// left
		FRectanglef tileRect = FRectanglef(min.X, min.Y + thickness.top + cornerRadius.topleft, thickness.left, size.Y - thickness.top - cornerRadius.topleft - thickness.bottom - cornerRadius.bottomleft);
		CalculateRect(tileRect, clip, size, uvRect, pCmd, color, index);

		// bottom-left
		CalculateCorner(bottomLeftCornerSliceCount, Math::V3_PI, 0, clip,
			min, max, size, vertices, indices, uvRect,
			thickness, cornerRadius, color, index);
		
		// bottom
		tileRect = FRectanglef(min.X + thickness.left + cornerRadius.bottomleft, max.Y - thickness.bottom,
			size.X - thickness.left - thickness.right - cornerRadius.bottomleft - cornerRadius.bottomright,
			thickness.bottom);
		CalculateRect(tileRect, clip, size, uvRect, pCmd, color, index);

		// bottom right
		CalculateCorner(bottomRightCornerSliceCount, Math::HALF_PI * 3, 1, clip,
			min, max, size, vertices, indices, uvRect,
			thickness, cornerRadius, color, index);

		// right
		tileRect = FRectanglef(max.X - thickness.right, min.Y + thickness.top + cornerRadius.topright, thickness.right,
			size.Y - thickness.top - thickness.bottom - cornerRadius.topright - cornerRadius.bottomright);
		CalculateRect(tileRect, clip, size, uvRect, pCmd, color, index);

		// top right
		CalculateCorner(topRightCornerSliceCount, 0, 2, clip,
			min, max, size, vertices, indices, uvRect,
			thickness, cornerRadius, color, index);

		// top
		tileRect = FRectanglef(min.X + thickness.left + cornerRadius.topleft, min.Y,
			size.X - thickness.left - thickness.right - cornerRadius.topleft - cornerRadius.topright,
			thickness.top);
		CalculateRect(tileRect, clip, size, uvRect, pCmd, color, index);

		// top left
		CalculateCorner(topLeftCornerSliceCount, Math::HALF_PI, 3, clip,
			min, max, size, vertices, indices, uvRect,
			thickness, cornerRadius, color, index);

		if (pOutCmd)
		{
			pOutCmd->DrawCmd = pCmd;
			pOutCmd->VertexStart = vtStart;
			pOutCmd->IndexStart = idxStart;
			pOutCmd->Count = (UINT)pCmd->mVertices.size() - pOutCmd->VertexStart;
			pOutCmd->IndexCount = (UINT)pCmd->mIndices.size() - pOutCmd->IndexStart;
		}
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
		pDefaultBrush->Name = VNameString("@MatInst:DefaultBrush");
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
				mesh->AddVertex_Pos_UV_Color_Index(&j->mVertices[0], numOfVert, InvertY, ClientRect.Height);
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