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

#pragma pack(push)
#pragma pack(1)
template <class Type>
struct PInvokePOD
{
	alignas(alignof(Type)) char MemData[sizeof(Type)];
};
#pragma pack(pop)

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

template<class _Type>
struct VTypeGetCSDesc
{
	static const char* GetCSTypeName() { return nullptr; }
};

#pragma pack(push)
#pragma pack(4)
struct UAnyValue
{
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
		Name,
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
		UINT mUI32Value;
		UINT64 mUI64Value;
		float mF32Value;
		double mF64Value;
		void* mPointer;
		int mNameIndex;
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
	void SetUI32(UINT v)
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
	void SetName(const VNameString& name)
	{
		Dispose();
		mValueType = EValueType::Name;
		mNameIndex = name.Index;
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

	template<class _Type>
	void GetValue(_Type& v)
	{
		ASSERT(mValueType == EValueType::ManagedHandle);
		ASSERT(mStruct.mStructSize == sizeof(_Type));
		CoreSDK::MemoryCopy(&v, mStruct.mStructPointer, sizeof(_Type));
	}
	void GetValue(char& v)
	{
		ASSERT(mValueType == EValueType::I8);
		v = mI8Value;
	}
	void GetValue(short& v)
	{
		ASSERT(mValueType == EValueType::I16);
		v = mI16Value;
	}
	void GetValue(int& v)
	{
		ASSERT(mValueType == EValueType::I32);
		v = mI32Value;
	}
	void GetValue(INT64& v)
	{
		ASSERT(mValueType == EValueType::I64);
		v = mI64Value;
	}
	void GetValue(BYTE& v)
	{
		ASSERT(mValueType == EValueType::UI8);
		v = mUI8Value;
	}
	void GetValue(UINT16& v)
	{
		ASSERT(mValueType == EValueType::UI16);
		v = mUI16Value;
	}
	void GetValue(UINT& v)
	{
		ASSERT(mValueType == EValueType::UI32);
		v = mUI32Value;
	}
	void GetValue(UINT64& v)
	{
		ASSERT(mValueType == EValueType::UI64);
		v = mUI64Value;
	}
	void GetValue(float& v)
	{
		ASSERT(mValueType == EValueType::F32);
		v = mF32Value;
	}
	void GetValue(double& v)
	{
		ASSERT(mValueType == EValueType::F64);
		v = mF64Value;
	}
	void GetValue(VNameString& v)
	{
		ASSERT(mValueType == EValueType::Name);
		v.Index = mNameIndex;
	}
	void GetValue(void*& v)
	{
		ASSERT(mValueType == EValueType::Ptr);
		v = mPointer;
	}
	void GetValue(FDNObjectHandle& v)
	{
		ASSERT(mValueType == EValueType::ManagedHandle);
		v = mPointer;
	}
	template<class _Type>
	void SetValue(const _Type& v)
	{
		SetStruct<_Type>(v, VTypeGetCSDesc<_Type>::GetCSTypeName());
	}
	void SetValue(const char v)
	{
		SetI8(v);
	}
	void SetValue(const short v)
	{
		SetI16(v);
	}
	void SetValue(const int v)
	{
		SetI32(v);
	}
	void SetValue(const INT64 v)
	{
		SetI64(v);
	}
	void SetValue(const BYTE v)
	{
		SetUI8(v);
	}
	void SetValue(const UINT16 v)
	{
		SetUI16(v);
	}
	void SetValue(const UINT v)
	{
		SetUI32(v);
	}
	void SetValue(const UINT64 v)
	{
		SetUI64(v);
	}
	void SetValue(const float v)
	{
		SetF32(v);
	}
	void SetValue(const double v)
	{
		SetF64(v);
	}
	void SetValue(const VNameString v)
	{
		SetName(v);
	}
	void SetValue(const v3dVector2_t v)
	{
		SetVector2(v);
	}
	void SetValue(const v3dVector3_t v)
	{
		SetVector3(v);
	}
	void SetValue(const v3dVector4_t v)
	{
		SetQuaternion(v);
	}
	void SetValue(const void* v)
	{
		SetPointer((void*)v);
	}
	void SetValue(const FDNObjectHandle& v)
	{
		SetManagedHandle(v);
	}
	friend constexpr bool operator ==(const UAnyValue& lh, const UAnyValue& rh)
	{
		if (lh.mValueType != rh.mValueType)
			return false;
		switch (lh.mValueType)
		{
			case Unknown:
			case ManagedHandle:
			case I8:
				return lh.mI8Value == rh.mI8Value;
			case I16:
				return lh.mI16Value == rh.mI16Value;
			case I32:
				return lh.mI32Value == rh.mI32Value;
			case I64:
				return lh.mI64Value == rh.mI64Value;
			case UI8:
				return lh.mUI8Value == rh.mUI8Value;
			case UI16:
				return lh.mUI16Value == rh.mUI16Value;
			case UI32:
				return lh.mUI32Value == rh.mUI32Value;
			case UI64:
				return lh.mUI64Value == rh.mUI64Value;
			case F32:
				return lh.mF32Value == rh.mF32Value;
			case F64:
				return lh.mF64Value == rh.mF64Value;
			case Name:
				return lh.mNameIndex == rh.mNameIndex;
			case Struct:
				{
					if (lh.mStruct.mStructSize != rh.mStruct.mStructSize)
						return false;
					return CoreSDK::MemoryCmp(lh.mStruct.mStructPointer, rh.mStruct.mStructPointer, lh.mStruct.mStructSize) == 0;
				}
			case Ptr:
				return lh.mPointer == rh.mPointer;
			case V2:
			case V3:
			case V4:
			default:
				break;
		}
		return false;
	}
	friend constexpr bool operator !=(const UAnyValue& lh, const UAnyValue& rh)
	{
		return !(lh == rh);
	}
	friend constexpr bool operator < (const UAnyValue& lh, const UAnyValue& rh)
	{
		if (lh.mValueType < rh.mValueType)
			return true;
		else if (lh.mValueType > rh.mValueType)
			return false;
		else
		{
			switch (lh.mValueType)
			{
				case Unknown:
				case ManagedHandle:
				case I8:
					return lh.mI8Value < rh.mI8Value;
				case I16:
					return lh.mI16Value < rh.mI16Value;
				case I32:
					return lh.mI32Value < rh.mI32Value;
				case I64:
					return lh.mI64Value < rh.mI64Value;
				case UI8:
					return lh.mUI8Value < rh.mUI8Value;
				case UI16:
					return lh.mUI16Value < rh.mUI16Value;
				case UI32:
					return lh.mUI32Value < rh.mUI32Value;
				case UI64:
					return lh.mUI64Value < rh.mUI64Value;
				case F32:
					return lh.mF32Value < rh.mF32Value;
				case F64:
					return lh.mF64Value < rh.mF64Value;
				case Name:
					return lh.mNameIndex < rh.mNameIndex;
				case Struct:
					{
						if (lh.mStruct.mStructSize < rh.mStruct.mStructSize)
							return true;
						else if (lh.mStruct.mStructSize < rh.mStruct.mStructSize)
							return false;
						else
							return CoreSDK::MemoryCmp(lh.mStruct.mStructPointer, rh.mStruct.mStructPointer, rh.mStruct.mStructSize) < 0;
					}
				case Ptr:
					return (UINT64)lh.mPointer < (UINT64)rh.mPointer;
				case V2:
				case V3:
				case V4:
				default:
					return lh.mUI64Value < rh.mUI64Value;
			}
		}
	}
	int GetValueSize()
	{
		switch (mValueType)
		{
		case Unknown:
			return 0;
		case ManagedHandle:
			return sizeof(void*);
		case I8:
			return sizeof(char);
		case I16:
			return sizeof(short);
		case I32:
			return sizeof(int);
		case I64:
			return sizeof(INT64);
		case UI8:
			return sizeof(BYTE);
		case UI16:
			return sizeof(UINT16);
		case UI32:
			return sizeof(UINT);
		case UI64:
			return sizeof(UINT64);
		case F32:
			return sizeof(float);
		case F64:
			return sizeof(double);
		case Name:
			return sizeof(int);
		case Struct:
			return mStruct.mStructSize;
		case Ptr:
			return sizeof(void*);
		case V2:
			return sizeof(v3dVector2_t);
		case V3:
			return sizeof(v3dVector3_t);
		case V4:
			return sizeof(v3dVector4_t);
		default:
			return 0;
		}
	}
};

struct UAnyValue_t
{
	static constexpr int StructSize = sizeof(UAnyValue);
	char MemData[StructSize];
};
#pragma pack(pop)

class TR_CLASS()
	FGlobalConfig
{
	struct FConfigValue
	{
		FConfigValue()
		{
			I32 = 0;
		}
		std::string Name;
		union 
		{
			int I32;
			UINT UI32;
			float F32;
		};
	};
	std::vector<FConfigValue>	Values;
	FConfigValue* GetOrCreateConfig(const char* name, UINT& outIndex);
	FConfigValue* GetConfig(UINT index);
public:
	static FGlobalConfig* GetInstance();
	const char* GetName(UINT handle);
	UINT GetConfigHandle(const char* name);

	UINT SetConfigValueI32(const char* name, int value);
	UINT SetConfigValueUI32(const char* name, UINT value);
	UINT SetConfigValueF32(const char* name, float value);

	void SetConfigValueI32(UINT handle, int value);
	void SetConfigValueUI32(UINT handle, UINT value);
	void SetConfigValueF32(UINT handle, float value);

	int GetConfigValueI32(const char* name);
	UINT GetConfigValueUI32(const char* name);
	float GetConfigValueF32(const char* name);

	int GetConfigValueI32(UINT handle);
	UINT GetConfigValueUI32(UINT handle);
	float GetConfigValueF32(UINT handle);
};

NS_END