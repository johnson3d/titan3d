#pragma once
#include "../IGeometryMesh.h"

NS_BEGIN

class INullGeometryMesh : public IGeometryMesh
{
public:
	virtual vBOOL ApplyGeometry(ICommandList* cmd, IPass* pass, vBOOL bImm) override;
};

NS_END