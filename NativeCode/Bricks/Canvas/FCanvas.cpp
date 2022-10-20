#include "FCanvas.h"

#define new VNEW

NS_BEGIN

namespace Canvas
{
	v3dxVector2 FUtility::RotateVector(const v3dxVector2& v, float angle)
	{
		//https://blog.csdn.net/hjq376247328/article/details/45113563
		float rotMatrix[2][2];
		float cosV = Math::Cos(angle);
		float sinV = Math::Sin(angle);
		rotMatrix[0][0] = cosV, rotMatrix[1][0] = -sinV;
		rotMatrix[0][1] = sinV, rotMatrix[1][1] = cosV;

		v3dxVector2 result;
		result.x = v.x * rotMatrix[0][0] + v.y * rotMatrix[1][0];
		result.y = v.x * rotMatrix[0][1] + v.y * rotMatrix[1][1];
		return result;
	}
	bool FUtility::LineLineIntersection(const v3dxVector2& ps1, const v3dxVector2& pe1, const v3dxVector2& ps2, const v3dxVector2& pe2, v3dxVector2* pPoint)
	{
		// Get A,B of first line - points : ps1 to pe1
		float A1 = pe1.y - ps1.y;
		float B1 = ps1.x - pe1.x;
		// Get A,B of second line - points : ps2 to pe2
		float A2 = pe2.y - ps2.y;
		float B2 = ps2.x - pe2.x;

		// Get delta and check if the lines are parallel
		float delta = A1 * B2 - A2 * B1;
		if (delta == 0) 
			return false;

		// Get C of first and second lines
		float C2 = A2 * ps2.x + B2 * ps2.y;
		float C1 = A1 * ps1.x + B1 * ps1.y;
		//invert delta to make division cheaper
		float invdelta = 1 / delta;
		// now return the Vector2 intersection point
		pPoint->x = (B2 * C1 - B1 * C2) * invdelta;
		pPoint->y = (A1 * C2 - A2 * C1) * invdelta;
		return true;
	}
	bool FUtility::LineRectIntersection(const v3dxVector2& s, const v3dxVector2& e, const FRect& rect, v3dxVector2* pPoint)
	{
		v3dxVector2 intersection;
		v3dxVector2 r00 = rect.Get_X0_Y0();
		v3dxVector2 r10 = rect.Get_X1_Y0();
		v3dxVector2 r11 = rect.Get_X1_Y1();
		v3dxVector2 r01 = rect.Get_X0_Y1();
		if (LineLineIntersection(s, e, r00, r10, &intersection))
			return true;
		if (LineLineIntersection(s, e, r00, r01, &intersection))
			return true;
		if (LineLineIntersection(s, e, r01, r11, &intersection))
			return true;
		if (LineLineIntersection(s, e, r11, r01, &intersection))
			return true;
		return false;
	}
	FDrawCmd* FDrawCmdList::GetOrNewDrawCmd(IImage* image)
	{
		if (mDrawCmds.size() > 0)
		{
			for (size_t i = mDrawCmds.size() - 1; i >= mStopCmdIndex; i--)
			{
				if (mDrawCmds[i]->Image == image)
					return mDrawCmds[i];
			}
		}
		auto tmp = MakeWeakRef(new FDrawCmd());
		tmp->Image = image;
		tmp->Batch = Batch;
		mDrawCmds.push_back(tmp);
		return tmp;
	}
	void FDrawCmdList::PushClip(const FRect& rect)
	{
		const auto& rc = GetCurrentClipRect();
		auto clip = FRect::And(rc, rect);
		mClipRects.push_back(clip);
	}
	void FDrawCmdList::PopClip()
	{
		mClipRects.erase(mClipRects.begin() + mClipRects.size());
	}

	void FDrawCmdList::AddText(IFont* font, const WCHAR* text, float x, float y, const Rgba& color)
	{
		const auto& clip = GetCurrentClipRect();
		FCanvasVertex vert[4];
		v3dxVector2 size;
		IImage* prevImage = nullptr;
		FDrawCmd* pCmd = nullptr;
		v3dxVector2 offset(x, y);
		auto rightClip = clip.GetRight();
		for (const WCHAR* c = text; c[0] != '\0'; c++)
		{
			auto image = font->GetWord(c[0], vert, &size);
			if (prevImage != image)
			{
				prevImage = image;
				pCmd = GetOrNewDrawCmd(prevImage);
			}
			if (offset.x + x > rightClip)
			{
				//todo: clip word
				break;
			}
			pCmd->PushQuad(vert, offset);
			
			offset.x += size.x;
		}
	}
	void FDrawCmdList::AddLine(const v3dxVector2& s, const v3dxVector2& e, float width, const Rgba& color, IImageRect* imgRect)
	{
		const auto& clip = GetCurrentClipRect();

		v3dxVector2 start, end;
		if (clip.ClipLine(s, e, &start, &end) == false)
			return;

		IImage* pImage = imgRect->Image;
		auto dir = end - start;
		dir = FUtility::RotateVector(dir);
		dir.normalize();

		auto halfWidth = width * 0.5f;
		auto ext = dir* halfWidth;
		auto pCmd = GetOrNewDrawCmd(pImage);
		
		FCanvasVertex vert[4];		
		auto pos = start - ext;
		auto& v = vert[RCN_X0_Y0];
		v.Pos.setValue(pos.x, pos.y, 0);
		v.Color = color;
		v.UV = imgRect->Rect.Get_X0_Y0();
		
		pos = start + ext;
		v = vert[RCN_X0_Y1];
		v.Pos.setValue(pos.x, pos.y, 0);
		v.Color = color;
		v.UV = imgRect->Rect.Get_X0_Y1();

		pos = start + ext;
		v = vert[RCN_X1_Y1];
		v.Pos.setValue(pos.x, pos.y, 0);
		v.Color = color;
		v.UV = imgRect->Rect.Get_X1_Y1();
		
		pos = start - ext;
		v = vert[RCN_X1_Y0];
		v.Pos.setValue(pos.x, pos.y, 0);
		v.Color = color;
		v.UV = imgRect->Rect.Get_X1_Y0();
		
		//todo: Clip quad with rect
		pCmd->PushQuad(vert);
	}
	void FDrawCmdList::AddLineStrips(const v3dxVector2* pPoints, UINT num, float width, const Rgba& color)
	{

	}
	void FDrawCmdList::AddImage(IImageRect* image, float x, float y, float w, float h, const Rgba& color)
	{
		const auto& clip = GetCurrentClipRect();
		IImage* pImage = image->Image;
		auto pCmd = GetOrNewDrawCmd(pImage);

		FCanvasVertex vert[4];
		auto& v = vert[RCN_X0_Y0];
		v.Pos.setValue(x, y, 0);
		v.Color = color;
		v.UV = image->Rect.Get_X0_Y0();

		v = vert[RCN_X0_Y1];
		v.Pos.setValue(x, y + h, 0);
		v.Color = color;
		v.UV = image->Rect.Get_X0_Y1();

		v = vert[RCN_X1_Y1];
		v.Pos.setValue(x + w, y + h, 0);
		v.Color = color;
		v.UV = image->Rect.Get_X1_Y1();

		v = vert[RCN_X1_Y0];
		v.Pos.setValue(x + w, y, 0);
		v.Color = color;
		v.UV = image->Rect.Get_X1_Y0();

		pCmd->PushQuad(vert);
	}

	void FDrawCmdList::Reset()
	{
		mClipRects.clear();
		mDrawCmds.clear();
		mStopCmdIndex = 0;
	}

	void FCanvas::PushCmdList(FDrawCmdList* cmdlist, NxRHI::FMeshDataProvider* mesh)
	{
		for (auto& j : cmdlist->mDrawCmds)
		{
			NxRHI::FMeshAtomDesc desc{};
			desc.BaseVertexIndex = mesh->GetVertexNumber();
			desc.StartIndex = mesh->GetPrimitiveNumber() * 3;
			desc.NumPrimitives = (UINT)j->mIndices.size() / 3;

			for (auto& k : j->mVertices)
			{
				mesh->AddVertex(&k.Pos, nullptr, &k.UV, k.Color.Value);
			}

			auto& pIndices = j->mIndices;
			for (UINT k = 0; k < (UINT)pIndices.size(); k += 3)
			{
				mesh->AddTriangle(pIndices[k], pIndices[k + 1], pIndices[k + 2]);
			}

			mesh->PushAtom(&desc, 1, j);
		}
	}

	void FDrawBatch::Begin(float w, float h)
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

	void FDrawBatch::End()
	{

	}

	void FCanvas::BuildMesh(NxRHI::FMeshDataProvider* mesh)
	{
		mesh->Reset();
		PushCmdList(Backgroud, mesh);
		for (auto& i : mBatches)
		{
			PushCmdList(i->GetBackgroud(), mesh);
			PushCmdList(i->GetMiddleground(), mesh);
			PushCmdList(i->GetForegroud(), mesh);
		}
		PushCmdList(Foregroud, mesh);
	}

	void FCanvas::Begin(float w, float h)
	{
		ClientRect.X = 0;
		ClientRect.Y = 0;
		ClientRect.Width = w;
		ClientRect.Height = h;

		Backgroud->Reset();
		Backgroud->PushClip(ClientRect);

		Foregroud->Reset();
		Foregroud->PushClip(ClientRect);
	}

	void FCanvas::End()
	{

	}

	void FCanvas::DemoDraw()
	{
		Begin(800, 600);
		{
			auto win1 = MakeWeakRef(new FDrawBatch());
			win1->SetPosition(50, 50);
			win1->Begin(300, 200);
			{
				auto cmdlist = win1->GetBackgroud();
				{
					cmdlist->AddLine(v3dxVector2(0, 0), v3dxVector2(100, 100), 5.0f, Rgba(255, 255, 255));
					//cmdlist->AddText();
				}
				cmdlist = win1->GetMiddleground();
				{
					cmdlist->PushClip(FRect(20, 20, 60, 60));
					cmdlist->AddLine(v3dxVector2(100, 0), v3dxVector2(0, 100), 5.0f, Rgba(255, 255, 255));
					cmdlist->PopClip();
				}
			}
			win1->End();
			this->PushBatch(win1);
		}
		End();

		auto mesh = MakeWeakRef(new NxRHI::FMeshDataProvider());
		this->BuildMesh(mesh);

		//mesh->Render
	}
}

NS_END
