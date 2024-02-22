#pragma once
#include "FBX.h"
#include "../../NextRHI/NxRHI.h"
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
				FBXNode = nullptr;
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
			FBXMeshImportDesc()
			{
				ReCalculateTangent = false;
				AsCollision = false;
				AsLocalSpace = false;
				HaveSkin = false;
				AsStaticMesh = false;
				RenderAtom = 0;
				TransformVertexToAbsolute = true;
				BakePivotInVertex = false;
			}
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
			FBXAnimImportDesc()
			{
				Duration = 0;
				SampleRate = 1.0f;
				AnimationType = EFBXAnimType::AT_Skeleton;
				AnimStack = nullptr;
				AnimLayer = nullptr;
			}
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
				AnimNum = 0;
			};
			~FBXFileImportDesc()
			{

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
			std::vector<FBXMeshImportDesc> Meshes;
			std::vector<FBXAnimImportDesc> Anims;
			//FBXMeshImportDesc** Meshes;
			//FBXAnimImportDesc** Anims;
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
			void ExtractFBXMeshesDescRecursive(fbxsdk::FbxNode * node, std::vector<FBXMeshImportDesc>&outFBXMeshImportDesces);
			void ExtractFBXAnimsDescRecursive(fbxsdk::FbxNode * node, FbxAnimStack * animStack, FbxAnimLayer * animLayer, std::vector<FBXAnimImportDesc>&outFBXAnimImportDesces);
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

