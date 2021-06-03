#include "IVKGeometryMesh.h"

#define new VNEW

NS_BEGIN

vBOOL IVKGeometryMesh::ApplyGeometry(ICommandList* cmd, IDrawCall* pass, vBOOL bImm)
{
	return IGeometryMesh::ApplyGeometry(cmd, pass, bImm);
}

NS_END