#include "CoreRtti.h"
#include "BaseHead.h"
#include "CSharpAPI.h"
#include "IUnknown.h"
#include "../Math/v3dxVector3.h"

NS_BEGIN

//void __FieldSet__Platform_CreateWindow(void (*value)(void*))
//{
//
//}

static_assert(sizeof(bool)==1,"");
//
//template<>
//RttiStruct AuxRttiStruct<void>::Instance;
//template<>
//RttiStruct AuxRttiStruct<bool>::Instance;
//template<>
//RttiStruct AuxRttiStruct<char>::Instance;
//template<>
//RttiStruct AuxRttiStruct<short>::Instance;
//template<>
//RttiStruct AuxRttiStruct<int>::Instance;
//template<>
//RttiStruct AuxRttiStruct<long>::Instance;
//template<>
//RttiStruct AuxRttiStruct<long long>::Instance;
//template<>
//RttiStruct AuxRttiStruct<unsigned char>::Instance;
//template<>
//RttiStruct AuxRttiStruct<unsigned short>::Instance;
//template<>
//RttiStruct AuxRttiStruct<unsigned int>::Instance;
//template<>
//RttiStruct AuxRttiStruct<unsigned long>::Instance;
//template<>
//RttiStruct AuxRttiStruct<unsigned long long>::Instance;
//template<>
//RttiStruct AuxRttiStruct<float>::Instance;
//template<>
//RttiStruct AuxRttiStruct<double>::Instance;
//template<>
//RttiStruct AuxRttiStruct<std::string>::Instance;
//template<>
//RttiStruct AuxRttiStruct<VIUnknownBase>::Instance;
//template<>
//RttiStruct AuxRttiStruct<VIUnknown>::Instance;

RttiStruct::RttiStruct()
{
	IsEnum = false;
	EnumDesc = nullptr;	
	Assignment = nullptr;
	Size = 0;
	RttiStructManager::GetInstance()->RegStructType(this);
}

void MemberDesc::SetValue(void* pHost, const void* pValueAddress) const
{
	auto pAddr = GetValueAddress(pHost);
	MemberType.SetValue(pAddr, pValueAddress);
	if (DeclareClass != nullptr)
	{
		auto pIUnknown = (VIUnknownBase*)DeclareClass->CastSuper(pHost, GetClassObject<VIUnknownBase>());
		if (pIUnknown != nullptr)
		{
			pIUnknown->OnPropertyChanged(this->MemberName, MemberType);
		}
	}
}

void RttiType::SetValue(void* pTar, const void* pSrc) const
{
	if (NumOfPointer > 0)
	{
		memcpy(pTar, pSrc, sizeof(void*));
	}
	else
	{
		Type->Assignment(pTar, pSrc);
	}
}

AuxRttiBuilderBase::AuxRttiBuilderBase()
{
	RttiStructManager::GetInstance()->RegStructBuilder(this);
}

const RttiStruct* VIUnknownBase::GetRtti() const
{
	return AuxRttiStruct<VIUnknownBase>::GetClassObject();
}

VIUnknownBase* VIUnknownBase::CastTo(RttiStruct* type)
{
	return (VIUnknownBase*)type->DownCast(this, GetRtti());
}

void RttiEnum::Init()
{
	RttiEnumManager::GetInstance()->RegEnumType(this->GetFullName().c_str(), this);
}

void RttiEnumManager::RegEnumType(const char* name, RttiEnum* type)
{
	AllEnumTyps.push_back(type);
}

RttiEnumManager* RttiEnumManager::GetInstance() 
{
	static RttiEnumManager object;
	return &object;
}

void RttiEnumManager::BuildRtti()
{
	/*for (auto i : AllEnumTyps)
	{
		
	}*/
}

void RttiEnumManager::FinalCleanup()
{

}

RttiEnum* RttiEnumManager::FindEnum(const char* name)
{
	for (auto i : AllEnumTyps)
	{
		if (i->GetFullName() == name)
			return i;
	}
	return nullptr;
}

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
	for (auto i : BaseTypes)
	{
		if (i.ClassType->IsA(pTar))
			return true;
	}	
	return false;
}

void RttiStructManager::RegStructBuilder(AuxRttiBuilderBase* builder)
{
	StructBuilders.push_back(builder);
}

void RttiStructManager::RegStructType(RttiStruct* type)
{
	AllStructTyps.push_back(type);
	
	//static_assert(TypePointerCounter<int***>::Value == 3, "");
}

RttiStructManager* RttiStructManager::GetInstance() 
{
	static RttiStructManager object;
	return &object;
}

RttiStructManager::~RttiStructManager()
{
	FinalCleanup();
}

struct Test_ConstantVarDesc : public VIUnknown, public v3dxVector3
{
	Test_ConstantVarDesc()
	{
		Dirty = TRUE;
		setValue(1,1,1);
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
	void TestSetDirty(std::string d)
	{
		if (d == "true")
			Dirty = 1;
	}
	static void TestStaticFunction(std::string d, int c)
	{

	}
private:
	typedef void (Test_ConstantVarDesc::MemberCall)(Test_ConstantVarDesc*);
	friend struct AuxRttiBuilder<Test_ConstantVarDesc>;
	void AAAA(Test_ConstantVarDesc*)
	{

	}
	void TestSetDirty(std::string d, int c)
	{
		std::function<Test_ConstantVarDesc::MemberCall> a;
		a = std::bind(&Test_ConstantVarDesc::AAAA, new Test_ConstantVarDesc(), std::placeholders::_1);
		if (d == "true")
			Dirty = 1;
	}
};

struct FTestMeta : public FMetaBase
{
	FTestMeta(const char* Desc)
		: DescString(Desc)
	{

	}
	ENGINE_RTTI(FTestMeta);
	const char* DescString;
};

StructBegin(FTestMeta, EngineNS)
{
}
StructEnd(FTestMeta)

StructBegin(Test_ConstantVarDesc, EngineNS)
{
	AppendClassMetaInfo(FTestMeta, "Test Class Meta");

	StructMember(Offset);
	{
		AppendMemberMetaInfo(FTestMeta, "Test Member Meta: Offset");
	}

	StructMember(Size);
	StructMember(Elements);
	StructMember(Name);
	StructMember(TestString);
	StructMember(Test);

	Struct_Method(SetDirty);
	{
		AppendMethodMetaInfo(FTestMeta, "Test Method Meta: SetDirty info");
	}

	Struct_MethodEX(TestSetDirty, void, std::string);
	Struct_MethodEX(TestSetDirty, void, std::string, int);

	StructConstructor();
	{
		AppendConstructorMetaInfo(FTestMeta, "Test_ConstantVarDesc info");
	}
}
StructEnd(Test_ConstantVarDesc, v3dxVector3, VIUnknown)

void TestReflection()
{
	AutoRef<VIUnknown> a = MakeWeakRef(new VIUnknown());
	AutoRef<VIUnknownBase> b = MakeWeakRef(new VIUnknown());
	b = a;
	{
		typedef VTypeList<std::string, float> TestTypeList_base;
		typedef VTypeList_PushBack<TestTypeList_base, char>::ResultType TestTypeList;
		if (TestTypeList::Size == 3)
		{
			int MyInt = 8;
			typedef VTypeList_ReplaceFront<TestTypeList, int* >::ResultType TL2;
			[[maybe_unused]] VTypeList_GetAt<0, TL2>::ResultType a = &MyInt;
			[[maybe_unused]] VTypeList_GetAt<1, TL2>::ResultType b = 0.5f;
			[[maybe_unused]] VTypeList_GetAt<2, TL2>::ResultType c = 'c';
			//VTypeList_GetAt<3, TL2>::ResultType d = 'c';
			int* p = a;

			typedef VTypeList_SetAt<2, int*, TestTypeList>::ResultType TL3;
			//typedef VTypeList<float*, TestTypeList> TL4;
			[[maybe_unused]] VTypeList_GetAt<0, TL3>::ResultType a2 = "A";
			[[maybe_unused]] VTypeList_GetAt<2, TL3>::ResultType c2 = p;
		}
		
		int AA = 5;
		int BB[100] = {};
		typedef int (FnTest)();
		std::function<FnTest> Assignment;
		//std::apply(Assignment, std::make_tuple(0));
		//std::invoke(Assignment);
		auto fnPtr = [AA,BB]()
		{
			auto result = AA;
			for (int i = 0; i < 100; i++)
			{
				result += BB[i];
			}
			return result;
		};
		auto sz1 = sizeof(fnPtr);
		Assignment = fnPtr;
		auto sz2 = sizeof(Assignment);
		if (sz1 == sz2)
		{
			sz1 = sz2;
		}
	}
	auto rtti = RttiStructManager::GetInstance()->FindStruct("EngineNS::Test_ConstantVarDesc");
	[[maybe_unused]] auto testMeta = rtti->GetFirstMeta<FTestMeta>();

	auto method = rtti->FindMethod("SetDirty");
	FArgumentStream args;
	args << std::string("true");
	FArgumentStream result;
	result.Reset();
	
	auto constructor = rtti->FindConstructor(std::vector<RttiType>());
	FArgumentStream createArgs;
	Test_ConstantVarDesc* tmp = (Test_ConstantVarDesc*)constructor->CreateInstance(createArgs);
	method->Invoke(tmp, args, result);

	auto pMemberName = rtti->FindMember("Name");
	std::string tt = *pMemberName->GetValueAddress<std::string>(tmp);
	std::string strTemp("bbb");
	pMemberName->SetValue(tmp, strTemp);

	rtti->FindMember("TestString")->SetValue(tmp, (std::string*)nullptr);

	auto pMember = rtti->FindMember("Size");
	ASSERT(pMember->MemberType.Type->Name == "UInt32");
	ASSERT(pMember->Offset == __vsizeof(Test_ConstantVarDesc, Size));
	ASSERT(pMember->MemberName == "Size");

	bool isIUnknown = rtti->IsA(GetClassObject<VIUnknown>());
	ASSERT(isIUnknown);
	auto pCastVector3 = (v3dxVector3*)rtti->CastSuper(tmp, GetClassObject<v3dxVector3>());
	ASSERT(pCastVector3->x==1 && pCastVector3->y == 1 && pCastVector3->z == 1);
	auto pCastIUnknown = (VIUnknown*)rtti->CastSuper(tmp, GetClassObject<VIUnknown>());
	ASSERT(pCastIUnknown!=nullptr);

	[[maybe_unused]] auto pDownCastTest = (Test_ConstantVarDesc*)GetClassObject<Test_ConstantVarDesc>()->DownCast(pCastVector3, GetClassObject<v3dxVector3>());
	[[maybe_unused]] auto pDownCastTest2 = (Test_ConstantVarDesc*)GetClassObject<Test_ConstantVarDesc>()->DownCast(pCastIUnknown, GetClassObject<VIUnknown>());
	tmp->Release();
}

void RttiStructManager::BuildRtti()
{
	auto pRtti = AuxRttiStruct<void>::GetClassObject();
	pRtti->Name = "void";
	pRtti = GetClassObject<bool>();
	pRtti->BuildClassInfo<bool>("bool", nullptr);
	pRtti = GetClassObject<char>();
	pRtti->BuildClassInfo<char>("char", nullptr);
	pRtti = GetClassObject<short>();
	pRtti->BuildClassInfo<short>("short", nullptr);
	pRtti = GetClassObject<int>();
	pRtti->BuildClassInfo<int>("int", nullptr);
	pRtti = GetClassObject<long>();
	pRtti->BuildClassInfo<long>("int32", nullptr);
	pRtti = GetClassObject<long long>();
	pRtti->BuildClassInfo<long long>("int64", nullptr);
	pRtti = GetClassObject<unsigned char>();
	pRtti->BuildClassInfo<unsigned char>("uint8", nullptr);
	pRtti = GetClassObject<unsigned short>();
	pRtti->BuildClassInfo<unsigned short>("uint16", nullptr);
	pRtti = GetClassObject<unsigned int>();
	pRtti->BuildClassInfo<unsigned int>("uint32", nullptr);
	pRtti = GetClassObject<unsigned long>();
	pRtti->BuildClassInfo<unsigned long>("uint32", nullptr);
	pRtti = GetClassObject<unsigned long long>();
	pRtti->BuildClassInfo<unsigned long long>("uint64", nullptr);
	pRtti = GetClassObject<float>();
	pRtti->BuildClassInfo<float>("float", nullptr);
	pRtti = GetClassObject<double>();
	pRtti->BuildClassInfo<double>("double", nullptr);
	pRtti = GetClassObject<std::string>();
	pRtti->BuildClassInfo<std::string>("string", "std");	
	pRtti = GetClassObject<VIUnknownBase>();
	pRtti->BuildClassInfo<VIUnknownBase>("VIUnknownBase", "EngineNS");
	pRtti = GetClassObject<VIUnknown>();
	pRtti->BuildClassInfo<VIUnknown>("VIUnknown", "EngineNS");

	for (auto i : StructBuilders)
	{
		i->BuildRtti();
	}

	RttiEnumManager::GetInstance()->BuildRtti();
	TestReflection();
}

void RttiStructManager::FinalCleanup()
{
	for (auto i : AllStructTyps)
	{
		i->Cleanup();
	}
	RttiEnumManager::GetInstance()->FinalCleanup();
}

RttiStruct* RttiStructManager::FindStruct(const char* name)
{
	for (auto i : AllStructTyps)
	{
		if (i->GetFullName() == name)
			return i;
	}
	return nullptr;
}

NS_END
