#pragma once

#include "FDrawCmdList.h"

NS_BEGIN

namespace Canvas
{
	class ICanvasBrush;

	class TR_CLASS()
		FCanvas : public IWeakReference
	{
	public:
		ENGINE_RTTI(FCanvas);
		FCanvas();
		~FCanvas();

		void PushBatch(const AutoRef<FCanvasDrawBatch>&batch);
		void PushBatch(FCanvasDrawBatch * batch);
		void Reset();
		void SetClientClip(float w, float h);
		
		void BuildMesh(NxRHI::FMeshDataProvider* mesh);

		void DemoDraw(NxRHI::FMeshDataProvider* MeshProvider);
	protected:
		void PushCmdList(FCanvasDrawCmdList* cmdlist, NxRHI::FMeshDataProvider* mesh);

		void DemoDraw();
	public:
		bool									InvertY = true;
	protected:
		FRectanglef								ClientRect{};
		AutoRef<FCanvasDrawCmdList>				Background;
		std::vector<AutoRef<FCanvasDrawBatch>>	mBatches;
		AutoRef<FCanvasDrawCmdList>				Foreground;
	};
}

NS_END