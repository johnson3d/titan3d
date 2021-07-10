#pragma once
#include "FBX.h"
#include "../../RHI/PreHead.h"
#include "fbxsdk.h"

namespace AssetImportAndExport
{
	namespace FBX
	{
		class FBXMeshImporter;

		class TR_CLASS(SV_Dispose = delete self)
			FBXImporter
		{
		public:
			FBXImporter();
			FBXImporter(FbxManager * fBXSdkManager);
			~FBXImporter();
		public:
			bool CheckFileValidedAndInitialize(const char* filename);
			const FBXFileImportDesc* GetFileImportDesc() const;
			const FBXMeshImportDesc* GetFBXMeshDescs(UINT index) const;
			FBXMeshImporter* CreateMeshImporter(UINT meshIndex);
		protected:
			void ExtractFBXFileDesc(fbxsdk::FbxScene* scene, fbxsdk::FbxImporter * importer);
			void ExtractFBXOBjectDescs(fbxsdk::FbxScene * scene);
			void ExtractFBXObjectsDescRecursive(fbxsdk::FbxNode * node, const EFBXObjectType objectType, std::vector<FBXObjectImportDesc*>&outFileImportDescs);
			void ExtractFBXMeshesDescRecursive(fbxsdk::FbxNode * node, std::vector<FBXMeshImportDesc*>&outFBXMeshImportDesces);

		protected:
			std::string mFilename;
			FBXFileImportDesc* mFBXFileImportDesc = nullptr;
			bool CheckValided = false;

		public:
			fbxsdk::FbxManager* GetFBXSdkManager() { return mFBXSdkManager; }
		protected:
			fbxsdk::FbxManager* mFBXSdkManager = nullptr;
		};

	}
}

