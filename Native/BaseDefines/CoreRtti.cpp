#include "CoreRtti.h"
#include "../BaseHead.h"
#include "../CSharpAPI.h"
NS_BEGIN

AuxRttiStruct<void>		AuxRttiStruct<void>::Instance;
AuxRttiStruct<bool>		AuxRttiStruct<bool>::Instance;
AuxRttiStruct<char>		AuxRttiStruct<char>::Instance;
AuxRttiStruct<short>		AuxRttiStruct<short>::Instance;
AuxRttiStruct<int>			AuxRttiStruct<int>::Instance;
AuxRttiStruct<long long>	AuxRttiStruct<long long>::Instance;
AuxRttiStruct<unsigned char>		AuxRttiStruct<unsigned char>::Instance;
AuxRttiStruct<unsigned short>		AuxRttiStruct<unsigned short>::Instance;
AuxRttiStruct<unsigned int>			AuxRttiStruct<unsigned int>::Instance;
AuxRttiStruct<unsigned long long>	AuxRttiStruct<unsigned long long>::Instance;
AuxRttiStruct<float>			AuxRttiStruct<float>::Instance;
AuxRttiStruct<double>			AuxRttiStruct<double>::Instance;
AuxRttiStruct<std::string>		AuxRttiStruct<std::string>::Instance;

const char* EngineNSString = "Titan3D";
const char* EngineNSStringEx = "Titan3D::";

void RttiEnum::Init()
{
	RttiEnumManager::GetInstance()->RegEnumType(this->GetFullName().c_str(), this);
}

void RttiEnumManager::RegEnumType(const char* name, RttiEnum* type)
{
	std::string userName = name;
	auto pos = userName.find("EngineNS::");
	if (pos == 0)
	{
		userName = userName.replace(pos, strlen("EngineNS::"), EngineNSStringEx);
	}
	pos = type->NameSpace.find("EngineNS");
	if (pos == 0)
	{
		type->NameSpace = type->NameSpace.replace(pos, strlen("EngineNS"), EngineNSString);
	}
	EnumTyps.insert(std::make_pair(userName, type));
}

RttiEnumManager* RttiEnumManager::GetInstance() 
{
	static RttiEnumManager object;
	return &object;
}

void RttiEnumManager::BuildRtti()
{
	AllEnumTyps.clear();
	for (auto i : EnumTyps)
	{
		AllEnumTyps.push_back(i.second);
	}
}

void RttiEnumManager::Finalize()
{

}

RttiEnum* RttiEnumManager::FindEnum(const char* name)
{
	auto i = EnumTyps.find(name);
	if (i == EnumTyps.end())
		return nullptr;
	return i->second;
}

RttiStruct::MemberDesc* RttiStruct::PushMember(RttiStruct* type, unsigned int offset, unsigned int size, unsigned int arrayElements, const char* name, bool isPointer)
{
	MemberDesc desc;
	desc.MemberType = type;
	desc.MemberName = name;
	desc.Offset = offset;
	desc.Size = size;
	if (isPointer == false)
	{
		desc.ArrayElements = arrayElements;
	}
	else
	{
		desc.ArrayElements = 1;
	}
	desc.IsPointer = isPointer;
	Members.push_back(desc);
	return &Members[Members.size() - 1];
}


struct ABC : public VIUnknown
{

};

void RttiStruct::Init()
{
	//RttiStructManager::GetInstance()->RegStructType(GetFullName().c_str(), this);

	/*PushMethod<long>(&VIUnknown::AddRef);

	VMethod<long> desc;
	ABC object;
	auto fun = desc.Bind(&object);
	fun();*/
}

bool RttiStruct::IsA(RttiStruct* pTar)
{
	if (this == pTar)
		return true;
	auto pCur = ParentStructType;
	while (pCur != nullptr && pCur != &AuxRttiStruct<void>::Instance)
	{
		if (this == pTar)
			return true;

		pCur = pCur->ParentStructType;
	}
	return false;
}

void RttiStructManager::RegStructType(const char* name, RttiStruct* type)
{
	std::string userName = name;
	auto pos = userName.find("EngineNS::");
	if (pos == 0)
	{
		userName = userName.replace(pos, strlen("EngineNS::"), EngineNSStringEx);
	}
	pos = type->NameSpace.find("EngineNS");
	if (pos == 0)
	{
		type->NameSpace = type->NameSpace.replace(pos, strlen("EngineNS"), EngineNSString);
	}
	StructTyps.insert(std::make_pair(userName, type));
}

RttiStructManager* RttiStructManager::GetInstance() 
{
	static RttiStructManager object;
	return &object;
}

RttiStructManager::~RttiStructManager()
{
	Finalize();
}

struct Test_ConstantVarDesc
{
	Test_ConstantVarDesc()
	{
		Dirty = TRUE;
	}
	UINT			Offset;
	UINT			Size;
	UINT            Elements;
	std::string		Name;
	vBOOL			Dirty;
	std::string*	TestString;
	int				Test[5];
	void SetDirty(std::string d)
	{
		if (d == "true")
			Dirty = 1;
	}
	void SetDirty2(std::string d, int c)
	{
		if (d == "true")
			Dirty = 1;
	}
	void TestSetDirty(std::string d, int c)
	{
		if (d == "true")
			Dirty = 1;
	}
	void TestSetDirty(std::string d)
	{
		if (d == "true")
			Dirty = 1;
	}
};

StructBegin(Test_ConstantVarDesc, EngineNS)
	AddClassMetaInfo("abc");

	StructMember(Offset);
	AppendMemberMetaInfo("Offset info");

	StructMember(Size);
	StructMember(Elements);
	StructMember(Name);
	StructMember(TestString);
	StructMember(Test);

	StructMethod1(SetDirty, d);
	AppendMethodMetaInfo("SetDirty info");

	StructMethod2(SetDirty2, d, c);

	StructMethodEx1(TestSetDirty , void, std::string, d);
	StructMethodEx2(TestSetDirty, void, std::string, d, int, c);

	StructConstructor0();
	AppendConstructorMetaInfo("Test_ConstantVarDesc info");
	
StructEnd(void)

StructImpl(Test_ConstantVarDesc);

int FTestFuncTraits(int, int)
{
	return 0;
}

void TestReflection()
{
	auto rtti = RttiStructManager::GetInstance()->FindStruct("Titan3D::Test_ConstantVarDesc");
	auto method = rtti->FindMethod("SetDirty");
	ArgumentStream args;
	args << std::string("true");
	ArgumentStream result;
	result.Reset();

	auto constructor = rtti->FindConstructor(std::vector<RttiStruct*>());
	Test_ConstantVarDesc* tmp = (Test_ConstantVarDesc*)constructor->CreateInstance(ArgumentStream());
	method->Invoke(tmp, args, result);

	const RttiStruct::MemberDesc* pMemberName = rtti->FindMember("Name");
	std::string tt = *pMemberName->GetValueAddress<std::string>(tmp);
	pMemberName->SetValue(tmp, &std::string("bbb"));

	rtti->FindMember("TestString")->SetValue(tmp, (std::string*)nullptr);

	const RttiStruct::MemberDesc* pMember = rtti->FindMember("Size");
	assert(pMember->MemberType->Name == "UInt32");
	assert(pMember->Offset == 4);
	assert(pMember->MemberName == "Size");

	delete tmp;
}

void RttiStructManager::BuildRtti()
{
	AllStructTyps.clear();
	for (auto i : StructTyps)
	{
		i.second->Init();
		AllStructTyps.push_back(i.second);
	}

	TestReflection();
}

void RttiStructManager::Finalize()
{
	for (auto& i : StructTyps)
	{
		i.second->Cleanup();
	}
}

RttiStruct* RttiStructManager::FindStruct(const char* name)
{
	auto i = StructTyps.find(name);
	if (i == StructTyps.end())
		return nullptr;
	return i->second;
}

CoreRttiManager* CoreRttiManager::GetInstance() 
{
	static CoreRttiManager Instance;
	return &Instance;
}

CoreRtti::CoreRtti(const char* name, const char* super, 
	const vIID& id, unsigned int size, FConstructor fun, const char* file, int line)
{
	SuperClass = nullptr;
	ClassId = 0;
	Constructor = fun;
	ClassName = name;
	SuperClassName = super;
	ClassId = id;
	Size = size;

	auto pos = ClassName.find("EngineNS");
	if (pos != std::string::npos)
	{
		ClassName = ClassName.replace(pos, strlen("EngineNS"), EngineNSString);
	}

	pos = SuperClassName.find("EngineNS");
	if (pos != std::string::npos)
	{
		SuperClassName = SuperClassName.replace(pos, strlen("EngineNS"), EngineNSString);
	}

	AllocFile = file;
	AllocLine = line;

	auto elem = std::make_pair(ClassName, this);

	CoreRttiManager::GetInstance()->AllRttis.push_back(elem);
}

CoreRttiManager::CoreRttiManager()
{

}
CoreRttiManager::~CoreRttiManager()
{
	Finalize();
}
void CoreRttiManager::Finalize()
{
	if (AllRttis.size() == 0)
		return;
	AllRttis.clear();
	Classes.clear();
	Classes_Id.clear();
}
void CoreRttiManager::BuildRtti()
{
	for (auto i : AllRttis)
	{
		Classes[i.first] = i.second;
	}
	for (auto i : Classes)
	{
		auto super = (CoreRtti*)FindRtti(i.second->SuperClassName.c_str());
		i.second->SuperClass = super;
		Classes_Id[i.second->ClassId] = i.second;
	}
}

const CoreRtti* CoreRttiManager::FindRtti(const char* name)
{
	auto iter = Classes.find(name);
	if (iter == Classes.end())
		return nullptr;
	return iter->second;
}

const CoreRtti* CoreRttiManager::FindRtti(vIID id)
{
	auto iter = Classes_Id.find(id);
	if (iter == Classes_Id.end())
		return nullptr;
	return iter->second;
}

NS_END

using namespace EngineNS;

extern "C"
{
	VFX_API CoreRttiManager* SDK_CoreRttiManager_GetInstance()
	{
		return CoreRttiManager::GetInstance();
	}
	VFX_API RttiEnumManager* SDK_RttiEnumManager_GetInstance()
	{
		return RttiEnumManager::GetInstance();
	}
	VFX_API RttiStructManager* SDK_RttiStructManager_GetInstance()
	{
		return RttiStructManager::GetInstance();
	}
	VFX_API void SDK_CoreRttiManager_BuildRtti()
	{
		CoreRttiManager::GetInstance()->BuildRtti();
		RttiEnumManager::GetInstance()->BuildRtti();
		RttiStructManager::GetInstance()->BuildRtti();
	}

	VFX_API VIUnknown* SDK_CoreRttiManager_NewObjectByName(const char* name)
	{
		auto rtti = CoreRttiManager::GetInstance()->FindRtti(name);
		if (rtti == nullptr)
			return nullptr;
		if (rtti->Constructor == nullptr)
			return nullptr;
		auto obj = rtti->Constructor(rtti->AllocFile, rtti->AllocLine);
		return obj;
	}
	VFX_API VIUnknown* SDK_CoreRttiManager_NewObjectById(vIID id)
	{
		auto rtti = CoreRttiManager::GetInstance()->FindRtti(id);
		if (rtti == nullptr)
			return nullptr;
		if (rtti->Constructor == nullptr)
			return nullptr;
		return rtti->Constructor(rtti->AllocFile, rtti->AllocLine);
	}

//#if defined(GetClassName)
//	#undef GetClassName
//#endif

	CSharpReturnAPI0(unsigned int, , CoreRtti, GetSize);
	CSharpReturnAPI0(const char*, , CoreRtti, GetClassName);
	CSharpReturnAPI0(const char*, , CoreRtti, GetSuperClassName);
	CSharpReturnAPI0(vIID, , CoreRtti, GetClassId);
	CSharpReturnAPI2(VIUnknown*, , CoreRtti, CreateInstance, const char*, int);

	CSharpReturnAPI0(const char*, , RttiEnum, GetName);
	CSharpReturnAPI0(const char*, , RttiEnum, GetNameSpace);
	CSharpReturnAPI0(unsigned int, , RttiEnum, GetMemberNumber);
	CSharpReturnAPI1(const char*, , RttiEnum, GetMemberName, unsigned int);
	CSharpReturnAPI1(int, , RttiEnum, GetMemberValue, unsigned int);

	CSharpReturnAPI0(vBOOL, , RttiStruct, GetIsEnum);
	CSharpReturnAPI0(const char*, , RttiStruct, GetName);
	CSharpReturnAPI0(const char*, , RttiStruct, GetNameSpace);
	CSharpReturnAPI0(unsigned int, , RttiStruct, GetSize);
	CSharpReturnAPI0(unsigned int, , RttiStruct, GetMemberNumber);
	CSharpReturnAPI1(unsigned int, , RttiStruct, FindMemberIndex, const char*);
	CSharpReturnAPI1(RttiStruct*, , RttiStruct, GetMemberType, unsigned int);
	CSharpReturnAPI1(const char*, , RttiStruct, GetMemberName, unsigned int);

	CSharpReturnAPI1(const CoreRtti*, , CoreRttiManager, FindRtti, const char*);
	CSharpReturnAPI0(unsigned int, , CoreRttiManager, GetRttiNumber);
	CSharpReturnAPI1(const CoreRtti*, , CoreRttiManager, GetRtti, unsigned int);
	
	CSharpReturnAPI1(const RttiEnum*, , RttiEnumManager, FindEnum, const char*);
	CSharpReturnAPI0(unsigned int, , RttiEnumManager, GetEnumNumber);
	CSharpReturnAPI1(const RttiEnum*, , RttiEnumManager, GetEnum, unsigned int);

	CSharpReturnAPI1(const RttiStruct*, , RttiStructManager, FindStruct, const char*);
	CSharpReturnAPI0(unsigned int, , RttiStructManager, GetStructNumber);
	CSharpReturnAPI1(const RttiStruct*, , RttiStructManager, GetStruct, unsigned int);
}