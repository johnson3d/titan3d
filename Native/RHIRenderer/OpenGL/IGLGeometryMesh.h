#pragma once
#include "../IGeometryMesh.h"
#include "GLPreHead.h"

NS_BEGIN

class IGLRenderContext;
class IGLGeometryMesh : public IGeometryMesh
{
public:
	IGLGeometryMesh();
	~IGLGeometryMesh();
	virtual vBOOL ApplyGeometry(ICommandList* cmd, IPass* pass, vBOOL bImm) override;
public:
	bool Init(IGLRenderContext* rc);
};

NS_END