#include "IGLGeometryMesh.h"
#include "IGLPass.h"
#include "IGLCommandList.h"
#include "IGLRenderContext.h"
#include "IGLIndexBuffer.h"
#include "../../../Graphics/GfxEngine.h"

#define new VNEW

NS_BEGIN

IGLGeometryMesh::IGLGeometryMesh()
{
	
}

IGLGeometryMesh::~IGLGeometryMesh()
{
	
}

vBOOL IGLGeometryMesh::ApplyGeometry(ICommandList* cmd, IPass* pass, vBOOL bImm)
{
	return IGeometryMesh::ApplyGeometry(cmd, pass, bImm);
}

bool IGLGeometryMesh::Init(IGLRenderContext* rc)
{
	return true;
}

NS_END