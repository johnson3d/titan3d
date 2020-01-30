#pragma once
#include "../IGeometryMesh.h"
#include "D11PreHead.h"

NS_BEGIN

class ID11GeometryMesh : public IGeometryMesh
{
public:
	virtual vBOOL ApplyGeometry(ICommandList* cmd, IPass* pass, vBOOL bImm) override;
};

NS_END