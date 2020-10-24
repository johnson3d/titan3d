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
	Cpp2CS2(EngineNS, IGpuBuffer, GetBufferData);
	Cpp2CS5(EngineNS, IGpuBuffer, Map);
	Cpp2CS2(EngineNS, IGpuBuffer, Unmap);
	Cpp2CS3(EngineNS, IGpuBuffer, UpdateBufferData);
}