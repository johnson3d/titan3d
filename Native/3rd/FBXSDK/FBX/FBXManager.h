#pragma once
#include "../../../../Native/Graphics/GfxPreHead.h"
#include "../2019.2/include/fbxsdk.h"
#include "../../Graphics/Mesh/GfxMesh.h"
#include "../../BaseDefines/CoreRtti.h"


NS_BEGIN

class FBXManager : public VIUnknown
{
public:
	RTTI_DEF(FBXManager, 0xfd3a83155b1d5a76, true);
	FBXManager();
	~FBXManager();
	virtual void Cleanup() override;
	vBOOL Init();
	FbxManager* GetFbxSDKManager() 
	{
		return mSDKManager;
	}
protected:
	
protected:

	FbxManager* mSDKManager;
};

NS_END