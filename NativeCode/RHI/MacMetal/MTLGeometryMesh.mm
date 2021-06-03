#include "MTLGeometryMesh.h"

#define new VNEW

NS_BEGIN

vBOOL MtlGeometryMesh::ApplyGeometry(ICommandList* pCmdList, IPass* pPass, vBOOL bImm)
{
	IGeometryMesh::ApplyGeometry(pCmdList, pPass, bImm);
	return TRUE;
}

NS_END