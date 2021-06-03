#include "IntegerType.h"
#include "v3dxMatrix4.h"
#include "v3dxVector2.h"
#include "v3dxVector3.h"
#include "v3dxColor4.h"
#include "v3dxFrustum.h"
#include "v3dxQuaternion.h"
#include "v3dxTransform.h"


NS_BEGIN
AuxRttiStruct<v3dxMatrix4>		AuxRttiStruct<v3dxMatrix4>::Instance;
AuxRttiStruct<v3dxBox3>			AuxRttiStruct<v3dxBox3>::Instance;
AuxRttiStruct<v3dxVector2>		AuxRttiStruct<v3dxVector2>::Instance;
AuxRttiStruct<v3dxVector3>		AuxRttiStruct<v3dxVector3>::Instance;
AuxRttiStruct<v3dxColor4>		AuxRttiStruct<v3dxColor4>::Instance;
AuxRttiStruct<v3dxPlane3>		AuxRttiStruct<v3dxPlane3>::Instance;
AuxRttiStruct<v3dxQuaternion>	AuxRttiStruct<v3dxQuaternion>::Instance;
AuxRttiStruct<v3dxTransform>	AuxRttiStruct<v3dxTransform>::Instance;
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