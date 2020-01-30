#include "IUnorderedAccessView.h"

#define new VNEW

NS_BEGIN

IUnorderedAccessView::IUnorderedAccessView()
{
}


IUnorderedAccessView::~IUnorderedAccessView()
{
}

NS_END

using namespace EngineNS;
extern "C"
{
	CSharpReturnAPI2(vBOOL, EngineNS, IGpuBuffer, GetBufferData, IRenderContext*, IBlobObject*);
	CSharpReturnAPI5(vBOOL, EngineNS, IGpuBuffer, Map, IRenderContext*, UINT, EGpuMAP, UINT, IMappedSubResource*);
	CSharpAPI2(EngineNS, IGpuBuffer, Unmap, IRenderContext*, UINT);
	CSharpReturnAPI3(vBOOL, EngineNS, IGpuBuffer, UpdateBufferData, ICommandList*, void*, UINT);
}