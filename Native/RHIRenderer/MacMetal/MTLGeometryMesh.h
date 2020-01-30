#pragma once
#include "../IGeometryMesh.h"

NS_BEGIN

class MtlGeometryMesh : public IGeometryMesh
{
public:
	virtual vBOOL ApplyGeometry(ICommandList* pCmdList, IPass* pPass, vBOOL bImm) override;
};

NS_END