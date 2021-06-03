#include "v3dxConvex.h"
#include "../Base/CSharpAPI.h"

#define new VNEW

NS_BEGIN

NS_END

using namespace EngineNS;

extern "C"
{
	VFX_API v3dxConvex* SDK_v3dxConvex_New()
	{
		return new v3dxConvex();
	}
	VFX_API void SDK_v3dxConvex_Delete(v3dxConvex* p)
	{
		delete p;
	}
	CSharpReturnAPI1(vBOOL, EngineNS, v3dxConvex, IsContain, const v3dxVector3*);
}