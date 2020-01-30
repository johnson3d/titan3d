#pragma once
#include "../../../../Native/Graphics/GfxPreHead.h"
#include "../2019.2/include/fbxsdk.h"
#include "../../Graphics/Mesh/GfxMesh.h"
#include "../../BaseDefines/CoreRtti.h"

NS_BEGIN

class FBXManager;
class FBXAnalyzer : public VIUnknown
{
public:
	RTTI_DEF(FBXAnalyzer, 0x3ae9c0f75b1e933e, true);
	FBXAnalyzer();
	~FBXAnalyzer();
	virtual void Cleanup() override;
	bool Init(FBXManager* fbxManager,const char* pName);
	vBOOL LoadFile(FBXManager* fbxManager, const char* pName);
	vBOOL SetGeomtryMeshStream(IRenderContext* rc, GfxMeshPrimitives* primitivesMesh);
	int GetPrimitivesNum();
protected:
	FbxNode * GetFbxNodeByType(FbxNode *rootNode, FbxNodeAttribute::EType type);
	void* GetGeomtryMeshPosition(FbxMesh* pMesh,UINT& size, UINT& stride);
	void* GetGeomtryMeshNormal(FbxLayerElementNormal* layerElementNormal, UINT& size, UINT& stride);
	void* GetGeomtryMeshTangent(FbxLayerElementTangent* layerElementTangent, UINT& size, UINT& stride);
	void* GetGeomtryMeshBinormal(FbxLayerElementBinormal* layerElementBinormal, UINT& size, UINT& stride);
	void* GetGeomtryMeshUV(FbxMesh* pMesh,UINT& size, UINT& stride);
	void* GetGeomtryMeshIndex(FbxMesh* pMesh,UINT& size);
	vBOOL FillGeomtryVertexStream(IRenderContext* rc, GfxMeshPrimitives* primitivesMesh, EVertexSteamType stream, FbxMesh* pMesh);
	vBOOL FillGeomtryVertexIndex(IRenderContext* rc, GfxMeshPrimitives* primitivesMesh, EIndexBufferType type, FbxMesh* pMesh);
	
protected:
	FbxGeometryConverter * mGeometryConverter;
	FbxScene * mFbxScene;
	std::map<int, UINT16> mIndexMap;
};

NS_END
