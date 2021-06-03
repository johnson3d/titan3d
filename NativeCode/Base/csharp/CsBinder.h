#pragma once

#include "../IUnknown.h"
#include "../string/vfxstring.h"
#include "../../Math/v3dxVector2.h"
#include "../../Math/v3dxVector3.h"
#include "../../Math/v3dxQuaternion.h"

namespace EngineNS::Rtti
{
	struct UNativeCoreProvider;
}

NS_BEGIN

struct FDNObjectHandle
{
	FDNObjectHandle(void* p) 
	: Handle(p){

	}
	void* Handle;
};

struct UAnyValue;

struct TR_CLASS(SV_LayoutStruct = 8)
UCs2CppBase
{
	UCs2CppBase();
	~UCs2CppBase();
	void* mCSharpHandle;
	inline FDNObjectHandle GetHandle() {
		return FDNObjectHandle(mCSharpHandle);
	}
	VNameString mCSFullName;
	void CreateManagedObject(UAnyValue* args = nullptr, int NumOfArg = 0, int retType = 2);
	void FreeManagedObjectGCHandle();
	void* GetManagedObject();

	static void UnitTest();

	static void InitializeNativeCoreProvider();
	static void FinalCleanupNativeCoreProvider();
	static EngineNS::Rtti::UNativeCoreProvider* GetNativeCoreProvider();
};

#pragma pack(push)
#pragma pack(4)
struct UAnyValue
{
	//和C#内要完全对应，不要乱改定义
	enum EValueType : char
	{
		Unknown,
		ManagedHandle,
		I8,
		I16,
		I32,
		I64,
		UI8,
		UI16,
		UI32,
		UI64,
		F32,
		F64,
		Struct,
		Ptr,
		V2,
		V3,
		V4,
	};
	struct FStructDesc
	{
		void*	mStructPointer;
		int		mStructSize;
		int		mTypeNameIndex;
		template <class T>
		void SetStruct(const T& v, const char* typeName)
		{
			mStructSize = sizeof(T);
			mStructPointer = CoreSDK::Alloc(mStructSize, nullptr, 0);
			CoreSDK::MemoryCopy(mStructPointer, (void*)&v, mStructSize);
			mTypeNameIndex = VNameString(typeName).Index;
		}
		void Dispose()
		{
			if (mStructPointer != nullptr)
			{
				CoreSDK::Free(mStructPointer);
				mStructPointer = nullptr;
			}
			mStructSize = 0;
		}
	};
	UAnyValue()
	{
		/*mValueType = EValueType::Unknown;
		mStructSize = 0;*/
		memset(this, 0, sizeof(UAnyValue));
	}
	char mValueType;
	char mUnused[3];
	union
	{
		char mI8Value;
		short mI16Value;
		int mI32Value;
		INT64 mI64Value;
		BYTE mUI8Value;
		UINT16 mUI16Value;
		UINT32 mUI32Value;
		UINT64 mUI64Value;
		float mF32Value;
		double mF64Value;
		void* mPointer;
		FStructDesc mStruct;
		FDNObjectHandle mGCHandle;
		v3dVector2_t mV2;
		v3dVector3_t mV3;
		v3dVector4_t mV4;
	};
	void Dispose()
	{
		if (mValueType == EValueType::Struct)
		{
			mStruct.Dispose();
		}
		else if (mValueType == EValueType::ManagedHandle)
		{
			FreeManagedHandle();
		}
		mValueType = EValueType::Unknown;
	}
	void FreeManagedHandle();
	template<class T>
	void SetStruct(const T& v, const char* typeName)
	{
		Dispose();
		mValueType = EValueType::Struct;
		mStruct.SetStruct(v, typeName);
	}
	void SetPointer(void* v)
	{
		Dispose();
		mValueType = EValueType::Ptr;
		mPointer = v;
	}
	void SetManagedHandle(FDNObjectHandle gcHandle)
	{
		Dispose();
		mValueType = EValueType::ManagedHandle;
		mGCHandle = gcHandle;
	}
	void SetI8(char v)
	{
		Dispose();
		mValueType = EValueType::I8;
		mI8Value = v;
	}
	void SetI16(short v)
	{
		Dispose();
		mValueType = EValueType::I16;
		mI16Value = v;
	}
	void SetI32(int v)
	{
		Dispose();
		mValueType = EValueType::I32;
		mI32Value = v;
	}
	void SetI64(INT64 v)
	{
		Dispose();
		mValueType = EValueType::I64;
		mI64Value = v;
	}
	void SetUI8(BYTE v)
	{
		Dispose();
		mValueType = EValueType::UI8;
		mUI8Value = v;
	}
	void SetUI16(UINT16 v)
	{
		Dispose();
		mValueType = EValueType::UI16;
		mUI16Value = v;
	}
	void SetUI32(UINT32 v)
	{
		Dispose();
		mValueType = EValueType::UI32;
		mUI32Value = v;
	}
	void SetUI64(UINT64 v)
	{
		Dispose();
		mValueType = EValueType::UI64;
		mUI64Value = v;
	}
	void SetF32(float v)
	{
		Dispose();
		mValueType = EValueType::F32;
		mF32Value = v;
	}
	void SetF64(double v)
	{
		Dispose();
		mValueType = EValueType::F64;
		mF64Value = v;
	}
	void SetVector2(v3dVector2_t v)
	{
		Dispose();
		mValueType = EValueType::V2;
		mV2 = v;
	}
	void SetVector3(v3dVector3_t v)
	{
		Dispose();
		mValueType = EValueType::V3;
		mV3 = v;
	}
	void SetQuaternion(v3dVector4_t v)
	{
		Dispose();
		mValueType = EValueType::V4;
		mV4 = v;
	}
};
#pragma pack(pop)

NS_END