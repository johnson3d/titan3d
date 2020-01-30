#include "GfxFBXManager.h"
#define new VNEW
NS_BEGIN

RTTI_IMPL(EngineNS::GfxFBXManager, VIUnknown);

GfxFBXManager::GfxFBXManager()
{
	InitializeSdkManager();
}


GfxFBXManager::~GfxFBXManager()
{
	DestroySdkObjects(SdkManager,true);
}

FbxManager* GfxFBXManager::GetSDKManager()
{
	return SdkManager;
}

// Creates an instance of the SDK manager.
void GfxFBXManager::InitializeSdkManager()
{
	// Create the FBX SDK memory manager object.
	// The SDK Manager allocates and frees memory
	// for almost all the classes in the SDK.
	SdkManager = FbxManager::Create();

	// create an IOSettings object
	FbxIOSettings * ios = FbxIOSettings::Create(SdkManager, IOSROOT);
	SdkManager->SetIOSettings(ios);

}

void GfxFBXManager::DestroySdkObjects(FbxManager* pSdkManager, bool pExitStatus)
{
	// Delete the FBX SDK manager.
// All the objects that
// (1) have been allocated by the memory manager, AND that
// (2) have not been explicitly destroyed
// will be automatically destroyed.
	if (pSdkManager) pSdkManager->Destroy();
	if (pExitStatus) FBXSDK_printf("Program Success!\n");
}

const char * GfxFBXManager::GetReaderOFNFilters()
{
	int nbReaders =
		SdkManager->GetIOPluginRegistry()->GetReaderFormatCount();

	FbxString s;
	int i = 0;

	for (i = 0; i < nbReaders; i++)
	{
		s += SdkManager->GetIOPluginRegistry()->
			GetReaderFormatDescription(i);
		s += "|*.";
		s += SdkManager->GetIOPluginRegistry()->
			GetReaderFormatExtension(i);
		s += "|";
	}

	// replace | by \0
	int nbChar = int(strlen(s.Buffer())) + 1;
	char *filter = new char[nbChar];
	memset(filter, 0, nbChar);

	FBXSDK_strcpy(filter, nbChar, s.Buffer());

	for (i = 0; i < int(strlen(s.Buffer())); i++)
	{
		if (filter[i] == '|')
		{
			filter[i] = 0;
		}
	}

	// the caller must delete this allocated memory
	return filter;
}

const char * GfxFBXManager::GetWriterSFNFilters()
{
	int nbWriters =
		SdkManager->GetIOPluginRegistry()->GetWriterFormatCount();

	FbxString s;
	int i = 0;

	for (i = 0; i < nbWriters; i++)
	{
		s += SdkManager->GetIOPluginRegistry()->
			GetWriterFormatDescription(i);
		s += "|*.";
		s += SdkManager->GetIOPluginRegistry()->
			GetWriterFormatExtension(i);
		s += "|";
	}

	// replace | by \0
	int nbChar = int(strlen(s.Buffer())) + 1;
	char *filter = new char[nbChar];
	memset(filter, 0, nbChar);

	FBXSDK_strcpy(filter, nbChar, s.Buffer());

	for (i = 0; i < int(strlen(s.Buffer())); i++)
	{
		if (filter[i] == '|')
		{
			filter[i] = 0;
		}
	}

	// the caller must delete this allocated memory
	return filter;
}

const char * GfxFBXManager::GetFileFormatExt(const int pWriteFileFormat)
{
	char *buf = new char[10];
	memset(buf, 0, 10);

	// add a starting point .
	buf[0] = '.';
	const char * ext = SdkManager->GetIOPluginRegistry()->
		GetWriterFormatExtension(pWriteFileFormat);
	FBXSDK_strcat(buf, 10, ext);

	// the caller must delete this allocated memory
	return buf;
}


NS_END

