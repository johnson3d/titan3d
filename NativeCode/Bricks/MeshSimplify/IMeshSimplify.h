#pragma once
#include "../../Base/IUnknown.h"
#include "../../Math/v3dxVector3.h"

NS_BEGIN

struct TR_CLASS(SV_LayoutStruct = 8)
	IMeshTriangle
{
	int A;
	int B;
	int C;
	int Material;
};

class TR_CLASS(SV_Dispose = delete self)
IMeshSimplify
{
public:
	static void SimplifyMesh(const v3dxVector3* posArray, int numOfPos, const IMeshTriangle* Indices, int numOfTri,
						v3dxVector3* outPosArray, int* outNumOfPos, IMeshTriangle* outIndices, int* outNumOfTri,
						int target_count, double agressiveness = 7, bool verbose = false);
};

NS_END
