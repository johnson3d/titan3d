#pragma once

#include "vfxGeomTypes.h"
#include "v3dxVector3.h"

#pragma pack(push,1)

struct VHitResult
{
	DWORD HitFlags;
	DWORD GroupData;
	
	DWORD Reserved;
	float Distance;
	
	int FaceId;
	float U;
	float V;
	
	v3dxVector3 Position;
	v3dxVector3 Normal;
	
	void* ExtData;
};

#pragma pack(pop)

