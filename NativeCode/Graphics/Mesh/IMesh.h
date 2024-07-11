#pragma once
#include "../../NextRHI/NxRHI.h"
#include "IMdfQueue.h"

NS_BEGIN

class TR_CLASS()
	IMesh : public IWeakReference
{
protected:
	AutoRef<NxRHI::FMeshPrimitives>		mGeoms;
	AutoRef<IMdfQueue>					mMdfQueue;
	AutoRef<NxRHI::FVertexArray>		mVertexArray;
public:
	IMesh();
	void Initialize(EngineNS::NxRHI::FMeshPrimitives* mesh, IMdfQueue* mdf);
	void SetInputStreams(EngineNS::NxRHI::FVertexArray* draw);

	IMdfQueue* GetMdfQuque() {
		return mMdfQueue;
	}

	IMesh* CloneMesh();

	static EngineNS::NxRHI::FInputLayoutDesc* CreateInputLayoutDesc(UINT streams);
};

NS_END