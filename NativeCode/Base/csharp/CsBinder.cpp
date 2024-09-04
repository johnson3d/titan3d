#include "CsBinder.h"
#include "../../../codegen/Cs2Cpp/cpp/EngineNS.UTest.UTestCs2CppBuilder.cs2cpp.h"
#include "../../../codegen/Cs2Cpp/cpp/EngineNS.Rtti.UNativeCoreProvider.cs2cpp.h"

#define new VNEW

NS_BEGIN

EngineNS::Rtti::UNativeCoreProvider GNativeCoreProvider;
void UCs2CppBase::InitializeNativeCoreProvider()
{
	GNativeCoreProvider.mCSFullName = "EngineNS.Rtti.UNativeCoreProvider@EngineCore";
	GNativeCoreProvider.CreateManagedObject();
}

void UCs2CppBase::FinalCleanupNativeCoreProvider()
{
	GNativeCoreProvider.FreeManagedObjectGCHandle();
}

EngineNS::Rtti::UNativeCoreProvider* UCs2CppBase::GetNativeCoreProvider()
{
	if (GNativeCoreProvider.mCSharpHandle == nullptr)
		return nullptr;
	return &GNativeCoreProvider;
}

UCs2CppBase::UCs2CppBase()
{
	mCSharpHandle = nullptr;
}

UCs2CppBase::~UCs2CppBase()
{
	FreeManagedObjectGCHandle();
}

void UCs2CppBase::CreateManagedObject(TtAnyValue* args, int NumOfArg, int retType)
{
	FreeManagedObjectGCHandle();

	mCSharpHandle = CoreSDK::CreateManagedObject(mCSFullName.c_str(), args, NumOfArg, retType);
}

void UCs2CppBase::FreeManagedObjectGCHandle()
{
	if (mCSharpHandle != nullptr)
	{
		CoreSDK::FreeManagedObjectGCHandle(mCSharpHandle);
		mCSharpHandle = nullptr;
	}
}

void* UCs2CppBase::GetManagedObject()
{
	return CoreSDK::GetManagedObjectFromGCHandle(mCSharpHandle);
}

void TtAnyValue::FreeManagedHandle()
{
	if (mGCHandle.Handle != nullptr)
	{
		CoreSDK::FreeManagedObjectGCHandle(mGCHandle.Handle);
		mGCHandle.Handle = nullptr;
	}
}

FGlobalConfig* FGlobalConfig::GetInstance()
{
	static FGlobalConfig obj;
	return &obj;
}
const char* FGlobalConfig::GetName(UINT handle)
{
	auto pConfig = GetConfig(handle);
	return (pConfig != nullptr) ? pConfig->Name.c_str() : nullptr;
}
UINT FGlobalConfig::GetConfigHandle(const char* name)
{
	for (size_t i = 0; i < Values.size(); i++)
	{
		if (Values[i].Name == name)
			return (UINT)i;
	}
	return 0xFFFFFFFF;
}
FGlobalConfig::FConfigValue* FGlobalConfig::GetOrCreateConfig(const char* name, UINT& index)
{
	index = GetConfigHandle(name);
	if (index == 0xFFFFFFFF)
	{
		index = (UINT)Values.size();
		FConfigValue v;
		v.Name = name;
		Values.push_back(v);
	}
	return &Values[index];
}
FGlobalConfig::FConfigValue* FGlobalConfig::GetConfig(UINT index)
{
	if (index >= Values.size())
		return nullptr;
	return &Values[index];
}
UINT FGlobalConfig::SetConfigValueI32(const char* name, int value)
{
	UINT index;
	auto pConfig = GetOrCreateConfig(name, index);
	pConfig->I32 = value;
	return index;
}
UINT FGlobalConfig::SetConfigValueUI32(const char* name, UINT value)
{
	UINT index;
	auto pConfig = GetOrCreateConfig(name, index);
	pConfig->UI32 = value;
	return index;
}
UINT FGlobalConfig::SetConfigValueF32(const char* name, float value)
{
	UINT index;
	auto pConfig = GetOrCreateConfig(name, index);
	pConfig->F32 = value;
	return index;
}

void FGlobalConfig::SetConfigValueI32(UINT handle, int value)
{
	auto pConfig = GetConfig(handle);
	pConfig->I32 = value;
}
void FGlobalConfig::SetConfigValueUI32(UINT handle, UINT value)
{
	auto pConfig = GetConfig(handle);
	pConfig->UI32 = value;
}
void FGlobalConfig::SetConfigValueF32(UINT handle, float value)
{
	auto pConfig = GetConfig(handle);
	pConfig->F32 = value;
}

int FGlobalConfig::GetConfigValueI32(const char* name)
{
	auto handle = GetConfigHandle(name);
	return GetConfigValueI32(handle);
}
UINT FGlobalConfig::GetConfigValueUI32(const char* name)
{
	auto handle = GetConfigHandle(name);
	return GetConfigValueUI32(handle);
}
float FGlobalConfig::GetConfigValueF32(const char* name)
{
	auto handle = GetConfigHandle(name);
	return GetConfigValueF32(handle);
}

int FGlobalConfig::GetConfigValueI32(UINT handle)
{
	auto pConfig = GetConfig(handle);
	return (pConfig != nullptr)? pConfig->I32 : 0;
}
UINT FGlobalConfig::GetConfigValueUI32(UINT handle)
{
	auto pConfig = GetConfig(handle);
	return (pConfig != nullptr) ? pConfig->UI32 : 0;
}
float FGlobalConfig::GetConfigValueF32(UINT handle)
{
	auto pConfig = GetConfig(handle);
	return (pConfig != nullptr) ? pConfig->F32 : 0;
}

void UCs2CppBase::UnitTest()
{
	///test code
	EngineNS::UTest::UTestCs2CppBuilder mCs2CppBuilder;
	//EngineNS::Rtti::UNativeCoreProvider mNativeCoreProvider;

	mCs2CppBuilder.mCSFullName = "EngineNS.UTest.UTestCs2CppBuilder@EngineCore";
	mCs2CppBuilder.CreateManagedObject();
	if (mCs2CppBuilder.mCSharpHandle != nullptr)
	{
		//[[maybe_unused]] auto pObject = mCs2CppBuilder.GetManagedObject();
		int b = 2;
		[[maybe_unused]] auto r = mCs2CppBuilder.Func0(1.0, &b);
		[[maybe_unused]] auto r2 = mCs2CppBuilder.Func1(5);
	}

	/*mNativeCoreProvider.mCSFullName = "EngineNS.Rtti.UNativeCoreProvider@EngineCore";
	mNativeCoreProvider.CreateManagedObject();
	if (mNativeCoreProvider.mCSharpHandle != nullptr)*/
	{
		UCs2CppBase list_int;
		//list_int.mCSFullName = "System.Collections.Generic.List<System.Int32@Unknown,>@Unknown";
		list_int.mCSFullName = "System.Collections.Generic.List<EngineNS.Matrix@EngineCore,>@Unknown";
		list_int.CreateManagedObject();
		if (list_int.mCSharpHandle != nullptr)
		{
			EngineNS::TtAnyValue v;
			//v.SetI32(8);

			v.SetStruct(v3dxMatrix4::IDENTITY, "EngineNS.Matrix@EngineCore");
			UCs2CppBase::GetNativeCoreProvider()->List_Add(list_int.GetHandle(), &v);
			[[maybe_unused]] auto count = UCs2CppBase::GetNativeCoreProvider()->List_GetCount(list_int.GetHandle());

			v.Dispose();
		}

		UCs2CppBase list_matrix;
		list_matrix.mCSFullName = "EngineNS.Matrix[]@EngineCore";
		EngineNS::TtAnyValue elemNum;
		elemNum.SetI32(8);
		list_matrix.CreateManagedObject(&elemNum, 1);
		if (list_int.mCSharpHandle != nullptr)
		{
			auto pined = UCs2CppBase::GetNativeCoreProvider()->PinGCHandle(list_matrix.mCSharpHandle);
			auto pMatrix = (v3dxMatrix4*)UCs2CppBase::GetNativeCoreProvider()->Array_PinElementAddress(list_matrix.GetHandle(), 0);
			pMatrix[2] = v3dxMatrix4::ZERO;
			pMatrix[2].m41 = 10;
			UCs2CppBase::GetNativeCoreProvider()->FreeGCHandle(pined);
		}
		elemNum.Dispose();
	}
}

NS_END