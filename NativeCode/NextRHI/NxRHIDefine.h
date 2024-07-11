#pragma once
#include "../Base/IUnknown.h"
#include "../Base/thread/vfxcritical.h"
#include "../Math/v3dxVector2.h"
#include "../Math/v3dxVector3.h"
#include "../Math/v3dxQuaternion.h"
NS_BEGIN

namespace NxRHI
{
	class ITexture;
	enum TR_ENUM()
		EQueueType
	{
		QU_Unknown = 0,
			QU_Default = 1,
			QU_Compute = (1 << 1),
			QU_Transfer = (1 << 2),
			QU_ALL = 0xFFFFFFFF,
	};
	enum TR_ENUM(SV_EnumNoFlags = true)
		EColorSpace {
		SRGB_NONLINEAR = 0,
			EXTENDED_SRGB_LINEAR,
	};

	enum TR_ENUM(SV_EnumNoFlags = true)
		EPrimitiveType
	{
		EPT_PointList = 1,
			EPT_LineList = 2,
			EPT_LineStrip = 3,
			EPT_TriangleList = 4,
			EPT_TriangleStrip = 5,
			EPT_TriangleFan = 6,
	};
	inline void RgbaToColor4(UINT rgba, float color[4])
	{
		color[0] = (float)(rgba & 0xFF) / 255.0f;
		color[1] = (float)((rgba >> 8) & 0xFF) / 255.0f;
		color[2] = (float)((rgba >> 16) & 0xFF) / 255.0f;
		color[3] = (float)((rgba >> 24) & 0xFF) / 255.0f;
	}

	enum TR_ENUM(SV_EnumNoFlags = true)
		EComparisionMode
	{
		CMP_NEVER = 1,
			CMP_LESS = 2,
			CMP_EQUAL = 3,
			CMP_LESS_EQUAL = 4,
			CMP_GREATER = 5,
			CMP_NOT_EQUAL = 6,
			CMP_GREATER_EQUAL = 7,
			CMP_ALWAYS = 8
	};
	enum TR_ENUM(SV_EnumNoFlags = true)
		ETextureCompressFormat
	{
		TCF_None = 0,//no compress
			TCF_Dxt1,//rgb:5-6-5 a:0 = bc1
			TCF_Dxt1a,//rgb:5-6-5 a:1 = bc1
			TCF_Dxt3,//rgb:5-6-5 a:8 = bc2
			TCF_Dxt5,//rgba 8 = bc3
			TCF_BC4,//r channel
			TCF_BC5,//rg:8-8
			TCF_BC6,//hdr, Does not support negative values
			TCF_BC6_FLOAT,//signed float
			TCF_BC7_UNORM,//Very high Quality rgba or rgb encoding
			
			TCF_Etc2_RGB8,
			TCF_Etc2_RGBA1,
			TCF_Etc2_RGBA8,
			TCF_Etc2_R11,
			TCF_Etc2_SIGNED_R11,
			TCF_Etc2_RG11,
			TCF_Etc2_SIGNED_RG11,
			
			TCF_Astc_4x4,
			TCF_Astc_4x4_Float,
			TCF_Astc_5x4,
			TCF_Astc_5x4_Float,
			TCF_Astc_5x5,
			TCF_Astc_5x5_Float,
			TCF_Astc_6x5,
			TCF_Astc_6x5_Float,
			TCF_Astc_6x6,
			TCF_Astc_6x6_Float,
			TCF_Astc_8x5,
			TCF_Astc_8x5_Float,
			TCF_Astc_8x6,
			TCF_Astc_8x6_Float,
			TCF_Astc_8x8,
			TCF_Astc_8x8_Float,
			TCF_Astc_10x5,
			TCF_Astc_10x5_Float,
			TCF_Astc_10x6,
			TCF_Astc_10x6_Float,
			TCF_Astc_10x8,
			TCF_Astc_10x8_Float,
			TCF_Astc_10x10,
			TCF_Astc_10x10_Float,
			TCF_Astc_12x10,
			TCF_Astc_12x10_Float,
			TCF_Astc_12x12,
			TCF_Astc_12x12_Float,
	};
	enum TR_ENUM()
		ECubeFace
	{
		CBFC_Left = 1,
			CBFC_Right = (1 << 1),
			CBFC_Top = (1 << 2),
			CBFC_Bottom = (1 << 3),
			CBFC_Front = (1 << 4),
			CBFC_Bac = (1 << 5),
	};
	struct TR_CLASS(SV_LayoutStruct = 8)
		FPictureDesc
	{
		void SetDefault()
		{
			dwStructureSize = sizeof(FPictureDesc);
			CompressFormat = ETextureCompressFormat::TCF_None;
			MipLevel = 0;
			Width = 0;
			Height = 0;
			CubeFaces = 0;
			DontCompress = FALSE;
			Format = EPixelFormat::PXF_UNKNOWN;
			sRGB = FALSE;
			StripOriginSource = FALSE;

			BitNumRed = 8;
			BitNumGreen = 8;
			BitNumBlue = 8;
			BitNumAlpha = 8;
			Unused = 0;
		}
		UINT			dwStructureSize = sizeof(FPictureDesc);
		ETextureCompressFormat	CompressFormat = ETextureCompressFormat::TCF_None;
		int				MipLevel = 0;
		int				Width = 0;
		int				Height = 0;
		UINT			CubeFaces = 0;

		vBOOL			DontCompress = FALSE;
		EPixelFormat	Format = EPixelFormat::PXF_UNKNOWN;

		vBOOL			sRGB = FALSE;
		vBOOL			StripOriginSource = FALSE;

		BYTE			BitNumRed = 8;
		BYTE			BitNumGreen = 8;
		BYTE			BitNumBlue = 8;
		BYTE			BitNumAlpha = 8;
		UINT			Unused = 0;

		TR_FUNCTION()
		unsigned int PixelByteWidth()
		{
			return GetPixelByteWidth(Format);
		}

		TR_FUNCTION()
		unsigned int PixelChannelCount()
		{
			return GetPixelChannelCount(Format);
		}
		
	};
	enum TR_ENUM(SV_EnumNoFlags = true)
		EVertexStreamType : UINT
	{
		VST_Position,
			VST_Normal,
			VST_Tangent,
			VST_Color,
			VST_UV,
			VST_LightMap,
			VST_SkinIndex,
			VST_SkinWeight,
			VST_TerrainIndex,
			VST_TerrainGradient,//10
			VST_InstPos,
			VST_InstQuat,
			VST_InstScale,
			VST_F4_1,
			VST_F4_2,
			VST_F4_3,//16
			VST_Number,
			VST_FullMask = 0xffffffff,
	};
	enum TR_ENUM(SV_EnumNoFlags)
		EShaderVarType
	{
		SVT_Float,
			SVT_Int,
			SVT_Texture,
			SVT_Sampler,
			SVT_Struct,
			SVT_Unknown,
	};
	inline void GetVertexStreamInfo(EVertexStreamType type, UINT* stride = nullptr, UINT* element = nullptr, EShaderVarType* varType = nullptr)
	{
		switch (type)
		{
		case EngineNS::NxRHI::VST_Position:
			if (stride != nullptr)
				*stride = sizeof(v3dxVector3);
			if (element != nullptr)
				*element = 3;
			if (varType != nullptr)
				*varType = EShaderVarType::SVT_Float;
			break;
		case EngineNS::NxRHI::VST_Normal:
			if (stride != nullptr)
				*stride = sizeof(v3dxVector3);
			if (element != nullptr)
				*element = 3;
			if (varType != nullptr)
				*varType = EShaderVarType::SVT_Float;
			break;
		case EngineNS::NxRHI::VST_Tangent:
			if (stride != nullptr)
				*stride = sizeof(v3dxQuaternion);
			if (element != nullptr)
				*element = 4;
			if (varType != nullptr)
				*varType = EShaderVarType::SVT_Float;
			break;
		case EngineNS::NxRHI::VST_Color:
			if (stride != nullptr)
				*stride = sizeof(DWORD);
			if (element != nullptr)
				*element = 1;
			if (varType != nullptr)
				*varType = EShaderVarType::SVT_Int;
			break;
		case EngineNS::NxRHI::VST_UV:
			if (stride != nullptr)
				*stride = sizeof(v3dxVector2);
			if (element != nullptr)
				*element = 2;
			if (varType != nullptr)
				*varType = EShaderVarType::SVT_Float;
			break;
		case EngineNS::NxRHI::VST_LightMap:
			if (stride != nullptr)
				*stride = sizeof(v3dxVector2);
			if (element != nullptr)
				*element = 2;
			if (varType != nullptr)
				*varType = EShaderVarType::SVT_Float;
			break;
		case EngineNS::NxRHI::VST_SkinIndex:
			if (stride != nullptr)
				*stride = sizeof(DWORD);
			if (element != nullptr)
				*element = 1;
			if (varType != nullptr)
				*varType = EShaderVarType::SVT_Int;
			break;
		case EngineNS::NxRHI::VST_SkinWeight:
			if (stride != nullptr)
				*stride = sizeof(DWORD);
			if (element != nullptr)
				*element = 1;
			if (varType != nullptr)
				*varType = EShaderVarType::SVT_Int;
			break;
		case EngineNS::NxRHI::VST_TerrainIndex:
			if (stride != nullptr)
				*stride = sizeof(DWORD);
			if (element != nullptr)
				*element = 1;
			if (varType != nullptr)
				*varType = EShaderVarType::SVT_Int;
			break;
		case EngineNS::NxRHI::VST_TerrainGradient:
			if (stride != nullptr)
				*stride = sizeof(DWORD);
			if (element != nullptr)
				*element = 1;
			if (varType != nullptr)
				*varType = EShaderVarType::SVT_Int;
			break;
		case EngineNS::NxRHI::VST_InstPos:
			if (stride != nullptr)
				*stride = sizeof(v3dxVector3);
			if (element != nullptr)
				*element = 3;
			if (varType != nullptr)
				*varType = EShaderVarType::SVT_Float;
			break;
		case EngineNS::NxRHI::VST_InstQuat:
			if (stride != nullptr)
				*stride = sizeof(v3dxQuaternion);
			if (element != nullptr)
				*element = 4;
			if (varType != nullptr)
				*varType = EShaderVarType::SVT_Float;
			break;
		case EngineNS::NxRHI::VST_InstScale:
			if (stride != nullptr)
				*stride = sizeof(v3dxVector3);
			if (element != nullptr)
				*element = 3;
			if (varType != nullptr)
				*varType = EShaderVarType::SVT_Float;
			break;
		case EngineNS::NxRHI::VST_F4_1:
			if (stride != nullptr)
				*stride = sizeof(v3dxQuaternion);
			if (element != nullptr)
				*element = 4;
			if (varType != nullptr)
				*varType = EShaderVarType::SVT_Int;
			break;
		case EngineNS::NxRHI::VST_F4_2:
			if (stride != nullptr)
				*stride = sizeof(v3dxQuaternion);
			if (element != nullptr)
				*element = 4;
			if (varType != nullptr)
				*varType = EShaderVarType::SVT_Float;
			break;
		case EngineNS::NxRHI::VST_F4_3:
			if (stride != nullptr)
				*stride = sizeof(v3dxQuaternion);
			if (element != nullptr)
				*element = 4;
			if (varType != nullptr)
				*varType = EShaderVarType::SVT_Float;
			break;
		case EngineNS::NxRHI::VST_Number:
			break;
		case EngineNS::NxRHI::VST_FullMask:
			break;
		default:
			break;
		}
	}
	struct TR_CLASS(SV_LayoutStruct = 8)
		FSubresourceBox
	{
		void SetDefault() {
			Left = 0;
			Top = 0;
			Front = 0;
			Right = 0xFFFFFFFF;
			Bottom = 0xFFFFFFFF;
			Back = 0xFFFFFFFF;
		}
		void SetWhole(ITexture* texture);
		UINT Left;
		UINT Top;
		UINT Front;
		UINT Right;
		UINT Bottom;
		UINT Back;
	};

	class TR_CLASS()
		IGpuResource : public IResourceBase
	{
	public:
		ENGINE_RTTI(IGpuResource);
		~IGpuResource();
		virtual long AddRef() override
		{
			return IResourceBase::AddRef();
		}

		virtual void Release() override
		{
			IResourceBase::Release();
		}
		virtual void* GetHWBuffer() {
			return nullptr;
		}
		virtual UINT GetFingerPrint() const {
			return 0;
		}
		virtual void SetFingerPrint(UINT fp) {

		}
		virtual void SetDebugName(const char* name) {
			//TagName = name;
		}
		const char* GetDebugName() const {
			return "";
			//return TagName.c_str();
		}
		inline int UnsafeGetCmdRefCount() const {
			return CmdRefCount;
		}
		inline void AddCmdRefCount() {
			CmdRefCount++;
		}
		inline void ReleaseCmdRefCount() {
			CmdRefCount--;
		}
	public:
		std::atomic<int>			CmdRefCount;
	};

	class IGpuDevice;
	struct FGpuHeapSizedPool;
	struct IPagedGpuMemAllocator;
	struct IGpuHeap : public VIUnknown
	{
		virtual UINT64 GetGPUVirtualAddress() = 0;
		virtual void* GetHWBuffer() = 0;
	};
	struct FGpuMemory : public VIUnknown
	{
		IGpuHeap* GpuHeap = nullptr;
		UINT64 Offset = -1;
		//UINT64 Size = 0;

		inline UINT64 GetGPUVirtualAddress()
		{
			auto result = GpuHeap->GetGPUVirtualAddress() + Offset;
			ASSERT(result != 0);
			return result;
		}

		virtual void FreeMemory() = 0;
		virtual void* GetHWBuffer() {
			return GpuHeap->GetHWBuffer();
		}
	};
	struct FGpuMemHolder : public IGpuResource
	{
		AutoRef<FGpuMemory>	GpuMem;
		~FGpuMemHolder()
		{
			if (GpuMem != nullptr)
			{
				GpuMem->FreeMemory();
				GpuMem = nullptr;
			}
		}
		inline UINT64 GetGPUVirtualAddress()
		{
			return GpuMem->GetGPUVirtualAddress();
		}
		inline void* GetHWBuffer() {
			return GpuMem->GetHWBuffer();
		}
	};
	struct IGpuMemAllocator : public IWeakReference
	{
		virtual AutoRef<FGpuMemory> Alloc(IGpuDevice* device, UINT64 size, const char* name) = 0;
		virtual void Free(FGpuMemory* memory) = 0;
	};
	struct FPooledGpuMemory : public FGpuMemory
	{
		TWeakRefHandle<FGpuHeapSizedPool> HostPool;
		FPooledGpuMemory* Next = nullptr;
		virtual void FreeMemory() override;
	};
	struct FGpuHeapSizedPool : public IWeakReference
	{//pool for different size;
		~FGpuHeapSizedPool();
		TWeakRefHandle<IPagedGpuMemAllocator> HostAllocator;
		UINT64 ChunkSize;
		FPooledGpuMemory* FreePoint = nullptr;
		FGpuMemory* Alloc(IGpuDevice* device, IPagedGpuMemAllocator* allocator, UINT64 size, const char* name);
		void Free(FGpuMemory* memory);
	};
	struct IPagedGpuMemAllocator : public IGpuMemAllocator
	{
		VSLLock				mLocker;
		std::map<UINT64, AutoRef<FGpuHeapSizedPool>>	Pools;
		std::vector<AutoRef<IGpuHeap>>	mGpuHeaps;
	public:
		~IPagedGpuMemAllocator();
		virtual AutoRef<FGpuMemory> Alloc(IGpuDevice* device, UINT64 size, const char* name) override;
		virtual void Free(FGpuMemory* memory) override;

		inline FGpuMemHolder* AllocGpuMem(IGpuDevice* device, UINT64 size, const char* name)
		{
			auto result = new FGpuMemHolder();
			result->GpuMem = Alloc(device, size, name);
			return result;
		}

		virtual UINT GetBatchCount(UINT64 size) {
			return 64;
		}
		virtual IGpuHeap* CreateGpuHeap(IGpuDevice* device, UINT64 size, UINT count, const char* name) = 0;
	};

	struct FAddressRange
	{
		UINT64 Begin = 0;
		UINT64 End = 0;
		UINT64 GetSize() const {
			return End - Begin;
		}
	};
	struct FLinearGpuHeapPool;
	struct ILinearGpuMemAllocator;
	struct FLinearGpuMemory : public FGpuMemory
	{
		TWeakRefHandle<FLinearGpuHeapPool> HostPool;
		FAddressRange			AddressRange{};
		virtual void FreeMemory() override;
	};
	struct FLinearGpuHeapPool : public IWeakReference
	{
		~FLinearGpuHeapPool();
		TWeakRefHandle<ILinearGpuMemAllocator> HostAllocator;
		std::vector<FAddressRange>	FreeRanges;
		AutoRef<IGpuHeap>			GpuHeap;
		UINT64						MaxSize = 0;
		FLinearGpuMemory* Alloc(IGpuDevice* device, UINT64 size);
		void Free(FLinearGpuMemory* memory);
	};
	struct ILinearGpuMemAllocator : public IGpuMemAllocator
	{
		VSLLock				mLocker;
		std::vector<AutoRef<FLinearGpuHeapPool>>		Pools;
		UINT64				PoolSize = 1024 * 1204 * 8;//8 mbytes per block;
	public:
		~ILinearGpuMemAllocator();
		virtual AutoRef<FGpuMemory> Alloc(IGpuDevice* device, UINT64 size, const char* name) override;
		virtual void Free(FGpuMemory* memory) override;

		virtual IGpuHeap* CreateGpuHeap(IGpuDevice* device, UINT64 size, const char* name) {
			return nullptr;
		}

		static void TestAlloc();
	};
}

NS_END