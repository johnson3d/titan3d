#pragma once
#include "../Base/IUnknown.h"
#include "../Base/thread/vfxcritical.h"

NS_BEGIN

namespace NxRHI
{
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
		EEtcFormat
	{
		UNKNOWN,
			//
			ETC1,
			//
			// ETC2 formats
			RGB8,
			SRGB8,
			RGBA8,
			SRGBA8,
			R11,
			SIGNED_R11,
			RG11,
			SIGNED_RG11,
			RGB8A1,
			SRGB8A1,
			//
			FORMATS,
			//
			DEFAULT = SRGB8
	};
	struct TR_CLASS(SV_LayoutStruct = 8)
		FPictureDesc
	{
		void SetDefault()
		{
			sRGB = 1;
			EtcFormat = EEtcFormat::RGBA8;//rgba8
			MipLevel = 0;
			Width = 0;
			Height = 0;
		}
		int				sRGB = 1;
		EEtcFormat		EtcFormat = EEtcFormat::RGBA8;
		int				MipLevel = 0;
		int				Width = 0;
		int				Height = 0;
	};
	enum TR_ENUM(SV_EnumNoFlags = true)
		EVertexStreamType : UINT32
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
	struct TR_CLASS(SV_LayoutStruct = 8)
		FSubresourceBox
	{
		UINT Left;
		UINT Top;
		UINT Front;
		UINT Right;
		UINT Bottom;
		UINT Back;
	};

	class IGpuDevice;
	struct FGpuHeapSizedPool;
	struct IGpuPooledMemAllocator;
	struct IGpuHeap : public VIUnknownBase
	{
		virtual UINT64 GetGPUVirtualAddress() = 0;
		virtual void* GetHWBuffer() = 0;
	};
	struct FGpuMemory : public VIUnknownBase
	{
		IGpuHeap* GpuHeap = nullptr;
		UINT64 Offset = -1;
		//UINT64 Size = 0;

		UINT64 GetGPUVirtualAddress()
		{
			return GpuHeap->GetGPUVirtualAddress() + Offset;
		}

		virtual void FreeMemory() = 0;
		virtual void* GetHWBuffer() {
			return GpuHeap->GetHWBuffer();
		}
	};
	struct IGpuMemAllocator : public VIUnknown
	{
		virtual AutoRef<FGpuMemory> Alloc(IGpuDevice* device, UINT64 size) = 0;
		virtual void Free(FGpuMemory* memory) = 0;
	};
	struct FPooledGpuMemory : public FGpuMemory
	{
		TObjectHandle<FGpuHeapSizedPool> HostPool;
		FPooledGpuMemory* Next = nullptr;
		virtual void FreeMemory() override;
	};
	struct FGpuHeapSizedPool : public VIUnknown
	{//pool for different size;
		~FGpuHeapSizedPool();
		TObjectHandle<IGpuPooledMemAllocator> HostAllocator;
		UINT64 ChunkSize;
		FPooledGpuMemory* FreePoint = nullptr;
		FGpuMemory* Alloc(IGpuDevice* device, IGpuPooledMemAllocator* allocator, UINT64 size);
		void Free(FGpuMemory* memory);
	};
	struct IGpuPooledMemAllocator : public IGpuMemAllocator
	{
		VSLLock				mLocker;
		std::map<UINT64, AutoRef<FGpuHeapSizedPool>>	Pools;
		std::vector<AutoRef<IGpuHeap>>	mGpuHeaps;
	public:
		~IGpuPooledMemAllocator();
		virtual AutoRef<FGpuMemory> Alloc(IGpuDevice* device, UINT64 size) override;
		virtual void Free(FGpuMemory* memory) override;

		virtual UINT GetBatchCount(UINT64 size) {
			return 64;
		}
		virtual IGpuHeap* CreateGpuHeap(IGpuDevice* device, UINT64 size, UINT count) = 0;
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
	struct IGpuLinearMemAllocator;
	struct FLinearGpuMemory : public FGpuMemory
	{
		TObjectHandle<FLinearGpuHeapPool> HostPool;
		FAddressRange			AddressRange{};
		virtual void FreeMemory() override;
	};
	struct FLinearGpuHeapPool : public VIUnknown
	{
		~FLinearGpuHeapPool();
		TObjectHandle<IGpuLinearMemAllocator> HostAllocator;
		std::vector<FAddressRange>	FreeRanges;
		AutoRef<IGpuHeap>			GpuHeap;
		UINT64						MaxSize = 0;
		FLinearGpuMemory* Alloc(IGpuDevice* device, UINT64 size);
		void Free(FLinearGpuMemory* memory);
	};
	struct IGpuLinearMemAllocator : public IGpuMemAllocator
	{
		VSLLock				mLocker;
		std::vector<AutoRef<FLinearGpuHeapPool>>		Pools;
		UINT64				PoolSize = 1024 * 1204 * 8;//8 mbytes per block;
	public:
		~IGpuLinearMemAllocator();
		virtual AutoRef<FGpuMemory> Alloc(IGpuDevice* device, UINT64 size) override;
		virtual void Free(FGpuMemory* memory) override;

		virtual IGpuHeap* CreateGpuHeap(IGpuDevice* device, UINT64 size) {
			return nullptr;
		}

		static void TestAlloc();
	};
}

NS_END