#pragma once

#include "vfxGeomTypes.h"
#include "v3dxVector3.h"
#include "v3dxDVector3.h"

#pragma pack(push,1)

struct TR_CLASS(SV_LayoutStruct = 8)
VHitResult
{
	v3dxDVector3 Position;
	UINT HitFlags;

	v3dxVector3 Normal;	
	UINT GroupData;
	
	float U;
	float V;
	
	float Distance;	
	int FaceId;

	void* ExtData;
};

#pragma pack(pop)

