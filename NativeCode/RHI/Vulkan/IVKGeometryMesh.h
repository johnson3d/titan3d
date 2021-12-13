#pragma once
#include "../Utility/IGeometryMesh.h"

NS_BEGIN

class IVKGeometryMesh : public IGeometryMesh
{
public:
	virtual vBOOL ApplyGeometry(ICommandList* cmd, IDrawCall* pass, vBOOL bImm) override;
};

NS_END