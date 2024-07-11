#include "FCanvas.h"
#include "FCanvasBrush.h"
#include "FDrawCmdList.h"
#include "../../Graphics/Mesh/MeshDataProvider.h"

#define new VNEW

NS_BEGIN

ENGINE_RTTI_IMPL(Canvas::ICanvasBrush);
StructBegin(ICanvasBrush, EngineNS::Canvas)
StructEnd(EngineNS::Canvas::ICanvasBrush, VIUnknown)

namespace Canvas
{	
	FCanvas::FCanvas()
	{
		Background = MakeWeakRef(new FCanvasDrawCmdList());
		Background->Batch = nullptr;

		Foreground = MakeWeakRef(new FCanvasDrawCmdList());
		Foreground->Batch = nullptr;
	}

	FCanvas::~FCanvas()
	{
	}
	void FCanvas::PushBatch(const AutoRef<FCanvasDrawBatch>& batch) 
	{
		mBatches.push_back(batch);
	}

	void FCanvas::PushBatch(FCanvasDrawBatch* batch) 
	{
		mBatches.push_back(batch);
	}
	void FCanvas::Reset()
	{
		mBatches.clear();
	}
	void FCanvas::BuildMesh(NxRHI::FMeshDataProvider* mesh)
	{
		mesh->Reset();
		PushCmdList(Background, mesh);
		for (auto& i : mBatches)
		{
			PushCmdList(i->GetBackgroud(), mesh);
			PushCmdList(i->GetMiddleground(), mesh);
			PushCmdList(i->GetForegroud(), mesh);
		}
		PushCmdList(Foreground, mesh);
	}

	void FCanvas::SetClientClip(float w, float h)
	{
		ClientRect.X = 0;
		ClientRect.Y = 0;
		ClientRect.Width = w;
		ClientRect.Height = h;

		Background->Reset(); //TODO
		Background->PushClip(ClientRect);

		Foreground->Reset(); //TODO
		Foreground->PushClip(ClientRect);
	}

	void FCanvas::DemoDraw(NxRHI::FMeshDataProvider* MeshProvider)
	{
		SetClientClip(800, 600);
		{
			auto win1 = MakeWeakRef(new FCanvasDrawBatch());
			win1->SetPosition(50, 50);
			win1->SetClientClip(800, 600);
			{
				//win1->GetMiddleground()->PushFont();
				//win1->AddLine_Background(v3dxVector2(1, 1), v3dxVector2(799, 599), 80.0, v3dxColor4(255, 0, 0, 255));
	/*			cmdlist = win1->GetMiddleground();
				{
					cmdlist->PushClip(FRect(20, 20, 60, 60));
					auto ImageRect = MakeWeakRef(new IFCanvasImageRect());
					cmdlist->AddLine(v3dxVector2(0, 0), v3dxVector2(1024, 1024), 20.0, Rgba(255, 255, 0, 255), ImageRect);
					cmdlist->PopClip();
				}*/
			}
			this->PushBatch(win1);
		}

		//auto mesh = MakeWeakRef(MeshProvider);
		this->BuildMesh(MeshProvider);
	}
}

NS_END
