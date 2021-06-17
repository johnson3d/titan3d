#pragma once
#include "FBX.h"
#include "../../RHI/PreHead.h"
#include "fbxsdk.h"

namespace AssetImportAndExport
{
	namespace FBX
	{
		class FBXMeshImporter;
		class FBXAnimImporter;

		enum TR_ENUM(SV_EnumNoFlags)
			EFBXImportResult
		{
			FIR_Sucess = 0,
				FIR_Failed,
				FIR_Cancel,
		};

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
			const FBXAnimImportDesc* GetFBXAnimDesc(UINT index) const;
			FBXAnimImporter* CreateAnimImporter(UINT meshIndex);
		protected:
			void ExtractFBXFileDesc(fbxsdk::FbxScene* scene, fbxsdk::FbxImporter * importer);
			void ExtractFBXOBjectDescs(fbxsdk::FbxScene * scene);
			//void ExtractFBXObjectsDescRecursive(fbxsdk::FbxNode * node, const EFBXObjectType objectType, std::vector<FBXObjectImportDesc*>&outFileImportDescs);
			void ExtractFBXMeshesDescRecursive(fbxsdk::FbxNode * node, std::vector<FBXMeshImportDesc*>&outFBXMeshImportDesces);
			void ExtractFBXAnimsDescRecursive(fbxsdk::FbxNode * node, FbxAnimStack * animStack, FbxAnimLayer * animLayer, std::vector<FBXAnimImportDesc*>&outFBXAnimImportDesces);

			bool IsHaveAnimCurve(FbxNode * node, FbxAnimLayer * animLayer);
			bool IsHaveAnimCurve(FbxNodeAttribute * nodeAtt, FbxAnimLayer * animLayer);
			bool IsSkeletonHaveAnimCurve(FbxNode * node, FbxAnimLayer * animLayer);

		protected:
			std::string mFilename;
			FBXFileImportDesc* mFBXFileImportDesc;
			bool CheckValided = false;

		public:
			fbxsdk::FbxManager* GetFBXSdkManager() { return mFBXSdkManager; }
		protected:
			fbxsdk::FbxManager* mFBXSdkManager;
		};

	}
}

