#include "FBX.h"
#include "FBXDataConverter.h"
#include "fbxsdk/core/fbxsystemunit.h"
#include "FBXImporter.h"

#define  new VNEW

namespace AssetImportAndExport::FBX
{
	FBXFactory::FBXFactory()
	{
		InitializeSdkManager();
	}

	FBXFactory::~FBXFactory()
	{
		DestroySdkObjects();
	}


	void FBXFactory::InitializeSdkManager()
	{ 
		// Create the FBX SDK memory manager object.
		// The SDK Manager allocates and frees memory
		// for almost all the classes in the SDK.
		mFBXSdkManager = FbxManager::Create();
		// create an IOSettings object
		FbxIOSettings* ios = FbxIOSettings::Create(mFBXSdkManager, IOSROOT);
		mFBXSdkManager->SetIOSettings(ios);

	}

	void FBXFactory::DestroySdkObjects()
	{
		// Delete the FBX SDK manager.
		// All the objects that
		// (1) have been allocated by the memory manager, AND that
		// (2) have not been explicitly destroyed
		// will be automatically destroyed.
		if (mFBXSdkManager)
		{
			mFBXSdkManager->Destroy();
			mFBXSdkManager = nullptr;
		}

	}

	FBXImporter* FBXFactory::CreateImporter()
	{
		return new FBXImporter(mFBXSdkManager);
	}

}