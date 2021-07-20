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
			EFBXAnimType
		{
			AT_Skeleton = 0,
				AT_Property,
				AT_Both,
		};

		enum TR_ENUM(SV_EnumNoFlags)
			EFBXImportResult
		{
			FIR_Sucess = 0,
				FIR_Failed,
				FIR_Cancel,
		};


		struct TR_CLASS(SV_LayoutStruct = 8)
			FBXObjectImportDesc
		{
			friend class FBXImporter;
		public:
			FBXObjectImportDesc()
			{
				Type = FOT_Unknown;
				Scale = 1.f;
				Imported = true;
			}
			~FBXObjectImportDesc()
			{
				FBXNode = nullptr;
			}
		public:
			VNameString Name;
			EFBXObjectType Type;
			float Scale;
			bool Imported;
		public:
			FbxNode* GetFBXNode() const { return FBXNode; }
		protected:
			FbxNode* FBXNode;
		};

		struct TR_CLASS(SV_LayoutStruct = 8)
			FBXMeshImportDesc :public FBXObjectImportDesc
		{
			friend class FBXImporter;
		public:
			bool ReCalculateTangent;
			bool AsCollision;
			bool AsLocalSpace;
			bool HaveSkin;
			bool AsStaticMesh;
			UINT RenderAtom;
			bool TransformVertexToAbsolute;
			bool BakePivotInVertex;
		};


		struct TR_CLASS(SV_LayoutStruct = 8)
			FBXAnimImportDesc :public FBXObjectImportDesc
		{
			friend class FBXImporter;
		public:
			VNameString Name;
			float Duration;
			float SampleRate;
			EFBXAnimType AnimationType;
		public:
			FbxAnimStack* GetFBXAnimStack() const { return AnimStack; }
			FbxAnimLayer* GetFBXAnimLayer() const { return AnimLayer; }
		private:
			FbxAnimStack* AnimStack;
			FbxAnimLayer* AnimLayer;
		};

		struct TR_CLASS(SV_Dispose = delete self)
			FBXFileImportDesc
		{
			friend class FBXImporter;
		public:
			FBXFileImportDesc()
			{
				FileSystemUnit = SU_cm;
				ConvertSceneUnit = false;
				ScaleFactor = 1.f;
				MeshNum = 0;
				Meshes = nullptr;
				AnimNum = 0;
				Anims = nullptr;
			};
			~FBXFileImportDesc()
			{
				for (UINT i = 0; i < MeshNum; ++i)
				{
					Safe_Delete<FBXMeshImportDesc>(Meshes[i]);
				}
				Safe_DeleteArray<FBXMeshImportDesc*>(Meshes);
				for (UINT i = 0; i < MeshNum; ++i)
				{
					Safe_Delete<FBXAnimImportDesc>(Anims[i]);
				}
				Safe_DeleteArray<FBXAnimImportDesc*>(Anims);
			}
		public:
			VNameString FileName;
			VNameString Creater;
			SystemUnit FileSystemUnit = SU_cm;
			bool ConvertSceneUnit = false;
			float ScaleFactor = 1.f;
			UINT MeshNum = 0;
			UINT AnimNum = 0;
		protected:
			//FBXObjectImportDesc** Objects;
			FBXMeshImportDesc** Meshes;
			FBXAnimImportDesc** Anims;
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

