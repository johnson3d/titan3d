#include "CsBinder.h"
#include "../../../codegen/Cs2Cpp/cpp/EngineNS.UTest.UTestCs2CppBuilder.cs2cpp.h"
#include "../../../codegen/Cs2Cpp/cpp/EngineNS.Rtti.UNativeCoreProvider.cs2cpp.h"

#define new VNEW

NS_BEGIN

template<>
AuxRttiStruct<UAnyValue>		AuxRttiStruct<UAnyValue>::Instance;

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

void UCs2CppBase::CreateManagedObject(UAnyValue* args, int NumOfArg, int retType)
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

void UAnyValue::FreeManagedHandle()
{
	if (mGCHandle.Handle != nullptr)
	{
		CoreSDK::FreeManagedObjectGCHandle(mGCHandle.Handle);
		mGCHandle.Handle = nullptr;
	}
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
		auto pObject = mCs2CppBuilder.GetManagedObject();
		int b = 2;
		auto r = mCs2CppBuilder.Func0(1.0, &b);
		auto r2 = mCs2CppBuilder.Func1(5);
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
			EngineNS::UAnyValue v;
			//v.SetI32(8);

			v.SetStruct(v3dxMatrix4::IDENTITY, "EngineNS.Matrix@EngineCore");
			UCs2CppBase::GetNativeCoreProvider()->List_Add(list_int.GetHandle(), &v);
			auto count = UCs2CppBase::GetNativeCoreProvider()->List_GetCount(list_int.GetHandle());

			v.Dispose();
		}

		UCs2CppBase list_matrix;
		list_matrix.mCSFullName = "EngineNS.Matrix[]@EngineCore";
		EngineNS::UAnyValue elemNum;
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