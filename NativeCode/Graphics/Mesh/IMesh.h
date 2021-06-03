#pragma once
#include "../../RHI/RHI.h"
#include "IMdfQueue.h"

NS_BEGIN

class TR_CLASS(SV_NameSpace = EngineNS, SV_UsingNS = EngineNS)
IMesh : public VIUnknown
{
protected:
	AutoRef<IMeshPrimitives>			mGeoms;
	AutoRef<IMdfQueue>					mMdfQueue;
	AutoRef<IVertexArray>				mVertexArray;
public:
	TR_CONSTRUCTOR()
	IMesh();
	TR_FUNCTION()
	void Initialize(IMeshPrimitives* mesh, IMdfQueue* mdf);
	TR_FUNCTION()
	void SetInputStreams(IVertexArray* draw);

	TR_FUNCTION()
	IMdfQueue* GetMdfQuque() {
		return mMdfQueue;
	}

	TR_FUNCTION()
	IMesh* CloneMesh();

	TR_FUNCTION()
	static IInputLayoutDesc* CreateInputLayoutDesc(UINT streams);
};

NS_END