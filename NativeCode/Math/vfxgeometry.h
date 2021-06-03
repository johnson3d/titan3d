#pragma once
#include "v3dxVector2.h"
#include "v3dxVector3.h"
#include "v3dxQuaternion.h"
#include "v3dxMatrix3.h"
#include "v3dxMatrix4.h"
#include "v3dxBox3.h"
#include "v3dxColor4.h"
#include "v3dxBezier.h"
#include "v3dxCylinder.h"
#include "v3dxFrustum.h"
#include "v3dxLine.h"
#include "v3dxOBB.h"
#include "v3dxPlane3.h"
#include "v3dxPoly3.h"
#include "v3dxSegment3.h"
#include "v3dxSphere.h"
#include "v3dxSpline.h"
#include "v3dxPoint.h"
#include "v3dxRect.h"
#include "v3dxSize.h"
#include "v3dxFrustum.h"

#include "../Base/CoreRtti.h"

NS_BEGIN
template<>
struct AuxRttiStruct<v3dxMatrix4> : public RttiStruct
{
	static AuxRttiStruct<v3dxMatrix4>		Instance;
	AuxRttiStruct()
	{
		Size = sizeof(v3dxMatrix4);
		Name = "Matrix";
		NameSpace = "Global";
		RttiStructManager::GetInstance()->RegStructType(GetFullName().c_str(), this);
	}
};

template<>
struct AuxRttiStruct<v3dxBox3> : public RttiStruct
{
	static AuxRttiStruct<v3dxBox3>		Instance;
	AuxRttiStruct()
	{
		Size = sizeof(v3dxBox3);
		Name = "BoudingBox";
		NameSpace = "Global";
		RttiStructManager::GetInstance()->RegStructType(GetFullName().c_str(), this);
	}
};

template<>
struct AuxRttiStruct<v3dxVector2> : public RttiStruct
{
	static AuxRttiStruct<v3dxVector2>		Instance;
	AuxRttiStruct()
	{
		Size = sizeof(v3dxVector2);
		Name = "v3dxVector2";
		NameSpace = "Global";
		RttiStructManager::GetInstance()->RegStructType(GetFullName().c_str(), this);
	}
};

template<>
struct AuxRttiStruct<v3dxVector3> : public RttiStruct
{
	static AuxRttiStruct<v3dxVector3>		Instance;
	AuxRttiStruct()
	{
		Size = sizeof(v3dxVector3);
		Name = "v3dxVector3";
		NameSpace = "Global";
		RttiStructManager::GetInstance()->RegStructType(GetFullName().c_str(), this);
	}
};

template<>
struct AuxRttiStruct<v3dxColor4> : public RttiStruct
{
	static AuxRttiStruct<v3dxColor4>		Instance;
	AuxRttiStruct()
	{
		Size = sizeof(v3dxColor4);
		Name = "v3dxColor4";
		NameSpace = "Global";
		RttiStructManager::GetInstance()->RegStructType(GetFullName().c_str(), this);
	}
};

template<>
struct AuxRttiStruct<v3dxPlane3> : public RttiStruct
{
	static AuxRttiStruct<v3dxPlane3>		Instance;
	AuxRttiStruct()
	{
		Size = sizeof(v3dxPlane3);
		Name = "v3dxPlane3";
		NameSpace = "Global";
		RttiStructManager::GetInstance()->RegStructType(GetFullName().c_str(), this);
	}
}; 
NS_END