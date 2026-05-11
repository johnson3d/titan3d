#include "CsBinder.h"

#define new VNEW

NS_BEGIN

FNativeCoreProvider GNativeCoreProvider;

int FNativeCoreProvider::List_GetCount(FDNObjectHandle listHandle)
{
	return CoreSDK::NativeCoreListGetCount != nullptr ? CoreSDK::NativeCoreListGetCount(listHandle.Handle) : 0;
}

void FNativeCoreProvider::List_Add(FDNObjectHandle listHandle, TtAnyValue* value)
{
	if (CoreSDK::NativeCoreListAdd != nullptr)
		CoreSDK::NativeCoreListAdd(listHandle.Handle, value);
}

void FNativeCoreProvider::List_Clear(FDNObjectHandle listHandle)
{
	if (CoreSDK::NativeCoreListClear != nullptr)
		CoreSDK::NativeCoreListClear(listHandle.Handle);
}

void FNativeCoreProvider::List_RemoveAt(FDNObjectHandle listHandle, int index)
{
	if (CoreSDK::NativeCoreListRemoveAt != nullptr)
		CoreSDK::NativeCoreListRemoveAt(listHandle.Handle, index);
}

void FNativeCoreProvider::List_GetValue(FDNObjectHandle listHandle, int index, TtAnyValue* outValue)
{
	if (CoreSDK::NativeCoreListGetValue != nullptr)
		CoreSDK::NativeCoreListGetValue(listHandle.Handle, index, outValue);
}

void* FNativeCoreProvider::Array_PinElementAddress(FDNObjectHandle arrayHandle, int index)
{
	return CoreSDK::NativeCoreArrayPinElementAddress != nullptr ? CoreSDK::NativeCoreArrayPinElementAddress(arrayHandle.Handle, index) : nullptr;
}

FDNObjectHandle FNativeCoreProvider::PinGCHandle(FDNObjectHandle objectHandle)
{
	return FDNObjectHandle(CoreSDK::NativeCorePinGCHandle != nullptr ? CoreSDK::NativeCorePinGCHandle(objectHandle.Handle) : nullptr);
}

void FNativeCoreProvider::FreeGCHandle(FDNObjectHandle pinnedHandle)
{
	if (CoreSDK::NativeCoreFreeGCHandle != nullptr)
		CoreSDK::NativeCoreFreeGCHandle(pinnedHandle.Handle);
}

void FNativeCoreProvider::GetPropertyValue(FDNObjectHandle hostHandle, const char* propName, TtAnyValue* outValue)
{
	if (CoreSDK::NativeCoreGetPropertyValue != nullptr)
		CoreSDK::NativeCoreGetPropertyValue(hostHandle.Handle, propName, outValue);
}

void FNativeCoreProvider::SetPropertyValue(FDNObjectHandle hostHandle, const char* propName, TtAnyValue* value)
{
	if (CoreSDK::NativeCoreSetPropertyValue != nullptr)
		CoreSDK::NativeCoreSetPropertyValue(hostHandle.Handle, propName, value);
}

void TtManagedObjectBridge::InitializeNativeCoreProvider()
{
}

void TtManagedObjectBridge::FinalCleanupNativeCoreProvider()
{
}

FNativeCoreProvider* TtManagedObjectBridge::GetNativeCoreProvider()
{
	return &GNativeCoreProvider;
}

TtManagedObjectBridge::TtManagedObjectBridge()
{
	mCSharpHandle = nullptr;
}

TtManagedObjectBridge::~TtManagedObjectBridge()
{
	FreeManagedObjectGCHandle();
}

void TtManagedObjectBridge::CreateManagedObject(TtAnyValue* args, int NumOfArg, int retType)
{
	FreeManagedObjectGCHandle();

	mCSharpHandle = CoreSDK::CreateManagedObject(mCSFullName.c_str(), args, NumOfArg, retType);
}

void TtManagedObjectBridge::FreeManagedObjectGCHandle()
{
	if (mCSharpHandle != nullptr)
	{
		CoreSDK::FreeManagedObjectGCHandle(mCSharpHandle);
		mCSharpHandle = nullptr;
	}
}

void* TtManagedObjectBridge::GetManagedObject()
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

void TtManagedObjectBridge::UnitTest()
{
	if (CoreSDK::CreateManagedObject == nullptr || CoreSDK::NativeCoreListAdd == nullptr)
		return;

	TtManagedObjectBridge list_int;
	list_int.mCSFullName = "System.Collections.Generic.List<EngineNS.Matrix@EngineCore,>@Unknown";
	list_int.CreateManagedObject();
	if (list_int.mCSharpHandle != nullptr)
	{
		EngineNS::TtAnyValue v;
		v.SetStruct(v3dxMatrix4::IDENTITY, "EngineNS.Matrix@EngineCore");
		TtManagedObjectBridge::GetNativeCoreProvider()->List_Add(list_int.GetHandle(), &v);
		[[maybe_unused]] auto count = TtManagedObjectBridge::GetNativeCoreProvider()->List_GetCount(list_int.GetHandle());

		v.Dispose();
	}

	TtManagedObjectBridge list_matrix;
	list_matrix.mCSFullName = "EngineNS.Matrix[]@EngineCore";
	EngineNS::TtAnyValue elemNum;
	elemNum.SetI32(8);
	list_matrix.CreateManagedObject(&elemNum, 1);
	if (list_matrix.mCSharpHandle != nullptr)
	{
		auto pined = TtManagedObjectBridge::GetNativeCoreProvider()->PinGCHandle(list_matrix.GetHandle());
		auto pMatrix = (v3dxMatrix4*)TtManagedObjectBridge::GetNativeCoreProvider()->Array_PinElementAddress(list_matrix.GetHandle(), 0);
		if (pMatrix != nullptr)
		{
			pMatrix[2] = v3dxMatrix4::ZERO;
			pMatrix[2].m41 = 10;
		}
		TtManagedObjectBridge::GetNativeCoreProvider()->FreeGCHandle(pined);
	}
	elemNum.Dispose();
}

NS_END
