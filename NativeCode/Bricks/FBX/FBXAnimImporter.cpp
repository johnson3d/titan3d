#include "FBXAnimImporter.h"


namespace AssetImportAndExport
{
	namespace FBX
	{

		FBXAnimImporter::FBXAnimImporter(FBXImporter* hostFBXImporter, UINT animIndex)
		{
			mHostFBXImporter = hostFBXImporter;
			mAnimIndex = animIndex;
		}

		FBXAnimImporter::~FBXAnimImporter()
		{
			mHostFBXImporter = nullptr;
		}

	}
}