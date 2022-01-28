#include "IntegerType.h"
#include "v3dxMatrix4.h"
#include "v3dxVector2.h"
#include "v3dxVector3.h"
#include "v3dxColor4.h"
#include "v3dxFrustum.h"
#include "v3dxQuaternion.h"
#include "v3dxTransform.h"


NS_BEGIN

struct ThisRttiBuilder
{
	ThisRttiBuilder()
	{
		auto pRtti = GetClassObject<v3dxMatrix4>();
		pRtti->BuildClassInfo<v3dxMatrix4>("v3dxMatrix4", nullptr);
		pRtti = GetClassObject<v3dxBox3>();
		pRtti->BuildClassInfo<v3dxBox3>("v3dxBox3", nullptr);
		pRtti = GetClassObject<v3dxVector2>();
		pRtti->BuildClassInfo<v3dxVector2>("v3dxVector2", nullptr);
		pRtti = GetClassObject<v3dxVector3>();
		pRtti->BuildClassInfo<v3dxVector3>("v3dxVector3", nullptr); 
		pRtti = GetClassObject<v3dxColor4>();
		pRtti->BuildClassInfo<v3dxColor4>("v3dxColor4", nullptr);
		pRtti = GetClassObject<v3dxPlane3>();
		pRtti->BuildClassInfo<v3dxPlane3>("v3dxPlane3", nullptr);
		pRtti = GetClassObject<v3dxQuaternion>();
		pRtti->BuildClassInfo<v3dxQuaternion>("v3dxQuaternion", nullptr);
		pRtti = GetClassObject<v3dxTransform>();
		pRtti->BuildClassInfo<v3dxTransform>("v3dxTransform", nullptr);
	}
} GThisRttiBuilder;
NS_END

v3dUInt32_4 v3dUInt32_4::GetVar(int x, int y, int z, int w)
{
	v3dUInt32_4 result;
	result.x = x;
	result.y = y;
	result.z = z;
	result.w = w;
	return result;
}
v3dUInt32_4 v3dUInt32_4::Zero = GetVar(0,0,0,0);