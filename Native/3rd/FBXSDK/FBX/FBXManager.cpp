#include "FBXManager.h"
#include "..\..\CSharpAPI.h"

NS_BEGIN


#define new VNEW
#ifdef IOS_REF
#undef  IOS_REF
#define IOS_REF (*(mSDKManager->GetIOSettings()))
#endif

RTTI_IMPL(EngineNS::FBXManager, VIUnknown);

FBXManager::FBXManager()
{

}


FBXManager::~FBXManager()
{
	Cleanup();
}

void FBXManager::Cleanup()
{
	if (mSDKManager)
	{
		mSDKManager->Destroy();
		mSDKManager = NULL;
	}
}

int numTabs = 0;

void PrintTabs()  //打印tabs，造出像xml那样的效果  
{
	for (int i = 0; i < numTabs; i++)
	{
		printf("\t");
	}
}


/**
*根据节点属性的不同，返回字符串。就是返回节点属性的名字
**/
FbxString GetAttributeTypeName(FbxNodeAttribute::EType type)
{
	switch (type)
	{
	case FbxNodeAttribute::eUnknown: return "UnknownAttribute";
	case FbxNodeAttribute::eNull: return "Null";
	case FbxNodeAttribute::eMarker: return "marker";  //马克……  
	case FbxNodeAttribute::eSkeleton: return "Skeleton"; //骨骼  
	case FbxNodeAttribute::eMesh: return "Mesh"; //网格  
	case FbxNodeAttribute::eNurbs: return "Nurbs"; //曲线  
	case FbxNodeAttribute::ePatch: return "Patch"; //Patch面片  
	case FbxNodeAttribute::eCamera: return "Camera"; //摄像机  
	case FbxNodeAttribute::eCameraStereo: return "CameraStereo"; //立体  
	case FbxNodeAttribute::eCameraSwitcher: return "CameraSwitcher"; //切换器  
	case FbxNodeAttribute::eLight: return "Light"; //灯光  
	case FbxNodeAttribute::eOpticalReference: return "OpticalReference"; //光学基准  
	case FbxNodeAttribute::eOpticalMarker: return "OpticalMarker";
	case FbxNodeAttribute::eNurbsCurve: return "Nurbs Curve";//NURBS曲线  
	case FbxNodeAttribute::eTrimNurbsSurface: return "Trim Nurbs Surface"; //曲面剪切？  
	case FbxNodeAttribute::eBoundary: return "Boundary"; //边界  
	case FbxNodeAttribute::eNurbsSurface: return "Nurbs Surface"; //Nurbs曲面  
	case FbxNodeAttribute::eShape: return "Shape"; //形状  
	case FbxNodeAttribute::eLODGroup: return "LODGroup"; //  
	case FbxNodeAttribute::eSubDiv: return "SubDiv";
	default: return "UnknownAttribute";
	}
}



/**
*打印一个属性
**/
void PrintAttribute(FbxNodeAttribute* pattribute)
{
	if (!pattribute)
	{
		return;
	}
	FbxString typeName = GetAttributeTypeName(pattribute->GetAttributeType());
	FbxString attrName = pattribute->GetName();
	PrintTabs();

	//FbxString.Buffer() 才是我们需要的字符数组  
	printf("<attribute type='%s' name='%s'/>\n ", typeName.Buffer(), attrName.Buffer());
}


/**
*打印出一个节点的属性，并且递归打印出所有子节点的属性;
**/
void PrintNode(FbxNode* pnode)
{
	PrintTabs();

	const char* nodeName = pnode->GetName(); //获取节点名字  

	FbxDouble3 translation = pnode->LclTranslation.Get();//获取这个node的位置、旋转、缩放  
	FbxDouble3 rotation = pnode->LclRotation.Get();
	FbxDouble3 scaling = pnode->LclScaling.Get();

	//打印出这个node的概览属性  
	printf("<node name='%s' translation='(%f,%f,%f)' rotation='(%f,%f,%f)' scaling='(%f,%f,%f)'>\n",
		nodeName,
		translation[0], translation[1], translation[2],
		rotation[0], rotation[1], rotation[2],
		scaling[0], scaling[1], scaling[2]);

	numTabs++;

	//打印这个node 的所有属性  
	for (int i = 0; i < pnode->GetNodeAttributeCount(); i++)
	{
		PrintAttribute(pnode->GetNodeAttributeByIndex(i));
	}

	//递归打印所有子node的属性  
	for (int j = 0; j < pnode->GetChildCount(); j++)
	{
		PrintNode(pnode->GetChild(j));
	}

	numTabs--;
	PrintTabs();
	printf("</node>\n");
}


vBOOL FBXManager::Init()
{
	// Create the FBX SDK memory manager object.
	// The SDK Manager allocates and frees memory
	// for almost all the classes in the SDK.
	mSDKManager = FbxManager::Create();
	if (mSDKManager == NULL)
		return FALSE;
	// create an IOSettings object
	FbxIOSettings * ios = FbxIOSettings::Create(mSDKManager, IOSROOT);
	mSDKManager->SetIOSettings(ios);
	return TRUE;
}



NS_END

using namespace EngineNS;

extern "C"
{
	CSharpReturnAPI0(vBOOL, EngineNS, FBXManager, Init);

	//CSharpReturnAPI0(const char*, EngineNS, GfxMesh, GetGeomName);
	//CSharpReturnAPI0(UINT, EngineNS, GfxMesh, GetAtomNumber);

	//CSharpReturnAPI3(bool, EngineNS, GfxMesh, Init, const char*, GfxMeshPrimitives*, GfxMdfQueue*);
	//CSharpReturnAPI2(bool, EngineNS, GfxMesh, SetMaterial, UINT, GfxMaterialPrimitive*);
	//CSharpAPI1(EngineNS, GfxMesh, SetMeshPrimitives, GfxMeshPrimitives*);
	//CSharpAPI1(EngineNS, GfxMesh, SetGfxMdfQueue, GfxMdfQueue*);

	//CSharpAPI1(EngineNS, GfxMesh, Save2Xnd, XNDNode*);
	//CSharpReturnAPI1(bool, EngineNS, GfxMesh, LoadXnd, XNDNode*);
}