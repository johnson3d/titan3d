#include "INullGeometryMesh.h"

#define new VNEW

NS_BEGIN

vBOOL INullGeometryMesh::ApplyGeometry(ICommandList* cmd, IPass* pass, vBOOL bImm)
{
	return IGeometryMesh::ApplyGeometry(cmd, pass, bImm);
}

NS_END