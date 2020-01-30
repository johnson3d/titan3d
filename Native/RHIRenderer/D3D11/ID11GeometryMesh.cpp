#include "ID11GeometryMesh.h"

#define new VNEW

NS_BEGIN

vBOOL ID11GeometryMesh::ApplyGeometry(ICommandList* cmd, IPass* pass, vBOOL bImm)
{
	IGeometryMesh::ApplyGeometry(cmd, pass, bImm);
	return TRUE;
}

NS_END