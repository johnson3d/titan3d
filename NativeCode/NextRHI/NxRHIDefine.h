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
		ETextureCompressFormat
	{
		TCF_None = 0,//no compress
			TCF_Dxt1,//rgb:5-6-5 a:0
			TCF_Dxt1a,//rgb:5-6-5 a:1
			TCF_Dxt3,//rgb:5-6-5 a:8
			TCF_Dxt5,//rg:8-8
			TCF_Etc1,
			TCF_Etc2_RGB8,
			TCF_Etc2_RGBA8,
			TCF_Etc2_R11,
			TCF_Etc2_SIGNED_R11,
			TCF_Etc2_RG11,
			TCF_Etc2_SIGNED_RG11,
			TCF_Etc2_RGBA1,
			TCF_Astc,
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