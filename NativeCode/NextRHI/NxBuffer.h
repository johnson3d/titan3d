#pragma once
#include "NxGpuDevice.h"
#include "NxShader.h"
#include "NxRHIDefine.h"
#include "../Base/BlobObject.h"

NS_BEGIN

namespace NxRHI
{
	enum TR_ENUM()
		EGpuUsage
	{
		USAGE_DEFAULT = 0,
			USAGE_IMMUTABLE = 1,
			USAGE_DYNAMIC = 2,
			USAGE_STAGING = 3
	};
	enum TR_ENUM()
		ECpuAccess
	{
		CAS_DEFAULT = 0,
			CAS_WRITE = 0x10000,
			CAS_READ = 0x20000,
	};
	enum TR_ENUM()
		EResourceMiscFlag
	{
		RM_GENERATE_MIPS = 0x1,
			RM_SHARED = 0x2,
			RM_TEXTURECUBE = 0x4,
			RM_DRAWINDIRECT_ARGS = 0x10,
			RM_BUFFER_ALLOW_RAW_VIEWS = 0x20,
			RM_BUFFER_STRUCTURED = 0x40,
			RM_RESOURCE_CLAMP = 0x80,
			RM_SHARED_KEYEDMUTEX = 0x100,
			RM_GDI_COMPATIBLE = 0x200,
			RM_SHARED_NTHANDLE = 0x800,
			RM_RESTRICTED_CONTENT = 0x1000,
			RM_RESTRICT_SHARED_RESOURCE = 0x2000,
			RM_RESTRICT_SHARED_RESOURCE_DRIVER = 0x4000,
			RM_GUARDED = 0x8000,
			RM_TILE_POOL = 0x20000,
			RM_TILED = 0x40000,
			RM_HW_PROTECTED = 0x80000
	};
	enum TR_ENUM()
		EBufferType
	{
		BFT_NONE = 0,
			BFT_CBuffer = 1,
			BFT_UAV = (1 << 1),
			BFT_SRV = (1 << 2),
			BFT_Vertex = (1 << 3),
			BFT_Index = (1 << 4),
			BFT_IndirectArgs = (1 << 5),
			BFT_RTV = (1 << 6),
			BFT_DSV = (1 << 7),
			BFT_RAW = (1 << 8),
	};
	struct TR_CLASS(SV_LayoutStruct = 8)
		FBufferDesc
	{
		void SetDefault()
		{
			Type = BFT_CBuffer;
			Usage = USAGE_DEFAULT;
			CpuAccess = (ECpuAccess)0;
			MiscFlags = (EResourceMiscFlag)0;
			StructureStride = 0;
			Size = 0;
			RowPitch = 0;
			DepthPitch = 0;
			InitData = nullptr;
		}
		EBufferType	Type = BFT_CBuffer;
		EGpuUsage	Usage = USAGE_DEFAULT;
		ECpuAccess	CpuAccess = (ECpuAccess)0;
		EResourceMiscFlag	MiscFlags = (EResourceMiscFlag)0;
		UINT		StructureStride = 0;
		UINT		Size = 0;
		UINT		RowPitch = 0;
		UINT		DepthPitch = 0;
		void*		InitData = nullptr;
	};

	enum TR_ENUM()
		EGpuResourceState
	{
		GRS_Undefine = 0,
			GRS_SrvPS,
			GRS_GenericRead,
			GRS_Uav,
			GRS_UavIndirect,
			GRS_RenderTarget,
			GRS_DepthStencil,
			GRS_DepthRead,
			GRS_StencilRead,
			GRS_DepthStencilRead,
			GRS_CopySrc,
			GRS_CopyDst,
			GRS_Present,
	};

	class TR_CLASS()
		IGpuResource : public IResourceBase
	{
	public:
		ENGINE_RTTI(IGpuResource);
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
		virtual UINT GetFingerPrint() const{
			return 0;
		}
	};
	struct TR_CLASS(SV_LayoutStruct = 8)
		FMappedSubResource
	{
		void SetDefault() {
			pData = nullptr;
			RowPitch = 0;
			DepthPitch = 0;
		}
		void* pData = nullptr;
		UINT RowPitch = 0;
		UINT DepthPitch = 0;
	};
	class TR_CLASS()
		IGpuBufferData : public IGpuResource
	{
	public:
		ENGINE_RTTI(IGpuBufferData);
		virtual void Release() override{
			IGpuResource::Release();
		}
		virtual EGpuResourceState GetGpuResourceState() {
			return GpuState;
		}
		virtual void TransitionTo(ICommandList* cmd, EGpuResourceState state) {
			GpuState = state;
		}
		virtual bool FetchGpuData(ICommandList* cmd, UINT subRes, IBlobObject* blob) {
			return false;
		}
		virtual void UpdateGpuData(ICommandList* cmd, UINT subRes, void* pData, const FSubresourceBox* box, UINT rowPitch, UINT depthPitch) {

		}
		virtual bool Map(ICommandList* cmd, UINT index, FMappedSubResource* res, bool forRead) = 0;
		virtual void Unmap(ICommandList* cmd, UINT index) = 0;
	public:
		EGpuResourceState	GpuState = EGpuResourceState::GRS_Undefine;
	};
	class TR_CLASS()
		IBuffer : public IGpuBufferData
	{
	public:
		ENGINE_RTTI(IBuffer);
		virtual bool Map(ICommandList * cmd, UINT index, FMappedSubResource * res, bool forRead) = 0;
		virtual void Unmap(ICommandList * cmd, UINT index) = 0;
		virtual void Flush2Device(ICommandList* cmd, void* pBuffer, UINT Size) = 0;
		virtual bool FetchGpuData(ICommandList * cmd, UINT subRes, IBlobObject * blob) override;
		virtual void UpdateGpuData(ICommandList * cmd, UINT offset, void* pData, UINT size) = 0;
		virtual void SetDebugName(const char* name) {}
		template<class _T>
		void SetValue(const FShaderVarDesc& binder, const _T& v)
		{
			SetValue(binder, &v, sizeof(_T));
		}
		template<class _T>
		void SetValue(const FShaderVarDesc* binder, const _T& v)
		{
			SetValue(*binder, v);
		}
		void SetValue(const FShaderVarDesc& binder, const void* pData, int size)
		{
			if (MapBuffer.size() < Desc.Size)
			{
				MapBuffer.resize(Desc.Size);
			}
			memcpy(&MapBuffer[binder.Offset], pData, size);
			Dirty = true;
		}
		void SetArrrayValue(const FShaderVarDesc& binder, int index, const void* pData, int size)
		{
			ASSERT(size == binder.Size / binder.Elements);
			if (MapBuffer.size() < Desc.Size)
			{
				MapBuffer.resize(Desc.Size);
			}
			auto offset = binder.Offset + size * index;
			memcpy(&MapBuffer[offset], pData, size);
			Dirty = true;
		}
		void* GetVarPtrToWrite(const FShaderVarDesc& binder, UINT size)
		{
			if (binder.Size < size)
				return nullptr;
			if (MapBuffer.size() < Desc.Size)
			{
				MapBuffer.resize(Desc.Size);
			}
			Dirty = true;
			return &MapBuffer[binder.Offset];
		}
		void FlushDirty(ICommandList* cmd, bool clear = false)
		{
			if (Dirty == false)
				return;
			Dirty = false;
			if (MapBuffer.size() > 0)
				Flush2Device(cmd, &MapBuffer[0], (UINT)MapBuffer.size());
			if (clear)
				MapBuffer.clear();
		}
	public:
		FBufferDesc			Desc;
		std::vector<BYTE>	MapBuffer;
	private:
		bool				Dirty = true;
	};

	struct TR_CLASS(SV_LayoutStruct = 8)
		FSamplerMode
	{
		UINT Count = 1;
		UINT Quality = 0;
	};
	struct TR_CLASS(SV_LayoutStruct = 8)
		FPixelDesc
	{
		int Width;
		int Height;
		int Stride;
		EPixelFormat Format;
	};
	struct TR_CLASS(SV_LayoutStruct = 8)
		FTextureDesc
	{
		void SetDefault()
		{
			Width = 1;//1d
			Height = 1;//2d
			Depth = 0;//3d
			MipLevels = 1;
			ArraySize = 1;
			SamplerDesc.Count = 1;
			SamplerDesc.Quality = 0;
			Format = EPixelFormat::PXF_R8G8B8A8_UNORM;
			Usage = USAGE_DEFAULT;
			BindFlags = EBufferType::BFT_SRV;
			CpuAccess = (ECpuAccess)0;
			MiscFlags = (EResourceMiscFlag)0;
			InitData = nullptr;
		}
		UINT Width = 1;
		UINT Height = 1;
		UINT Depth = 0;
		UINT MipLevels = 1;
		UINT ArraySize = 1;
		FSamplerMode SamplerDesc;
		EPixelFormat Format = EPixelFormat::PXF_R8G8B8A8_UNORM;
		EGpuUsage Usage = USAGE_DEFAULT;
		EBufferType	BindFlags = EBufferType::BFT_SRV;
		ECpuAccess CpuAccess = (ECpuAccess)0;
		EResourceMiscFlag MiscFlags = (EResourceMiscFlag)0;
		FMappedSubResource* InitData;
	};
	struct TR_CLASS(SV_LayoutStruct = 8)
		FSubResourceFootPrint
	{
		EPixelFormat Format;
		int X = 0;
		int Y = 0;
		int Z = 0;

		UINT Width;
		UINT Height;
		UINT Depth;

		UINT RowPitch;
	};
	class TR_CLASS()
		ITexture : public IGpuBufferData
	{//1,2,3d
	public:
		ENGINE_RTTI(ITexture);

		int GetDimension() const {
			if (Desc.Height == 0 && Desc.Depth == 0)
				return 1;
			else if (Desc.Depth == 0)
				return 2;
			else
				return 3;
		}
		UINT GetSubResource(UINT mipIndex, UINT arrayIndex)
		{
			return mipIndex + arrayIndex * Desc.MipLevels;
		}
		virtual bool Map(ICommandList* cmd, UINT index, FMappedSubResource* res, bool forRead) = 0;
		virtual void Unmap(ICommandList* cmd, UINT index) = 0;
		static void BuildImage2DBlob(IBlobObject * blob, void* pData, UINT RowPitch, const FTextureDesc* desc);
		static void BuildImage2DBlob(IBlobObject* blob, IBlobObject* gpuData, const FTextureDesc* desc);
		virtual bool FetchGpuData(ICommandList * cmd, UINT subRes, IBlobObject * blob) override;
		virtual void SetDebugName(const char* name) {}
		virtual FResourceState* GetResourceState() override {
			return &mResourceState;
		}
		virtual IGpuBufferData* CreateBufferData(IGpuDevice* device, UINT mipIndex, ECpuAccess cpuAccess, FSubResourceFootPrint* outFootPrint) = 0;
	public:
		FTextureDesc		Desc;
		FResourceState		mResourceState;
	};

	struct TR_CLASS(SV_LayoutStruct = 8)
		FCbvDesc
	{
		FShaderBinder*	ShaderBinder;
	};
	class TR_CLASS()
		ICbView : public IGpuResource
	{
	public:
		ENGINE_RTTI(ICbView);
		IBuffer* GetBuffer() {
			return Buffer;
		}
		const FShaderBinder* GetShaderBinder() {
			return ShaderBinder;
		}
		template<class _T>
		void SetValue(const FShaderVarDesc& binder, const _T& v)
		{
			Buffer->SetValue(binder, &v, sizeof(_T));
		}
		template<class _T>
		void SetValue(const FShaderVarDesc* binder, const _T& v)
		{
			Buffer->SetValue(*binder, v);
		}
		void SetValue(const FShaderVarDesc& binder, const void* pData, int size)
		{
			Buffer->SetValue(binder, pData, size);
		}
		void SetArrrayValue(const FShaderVarDesc& binder, int index, const void* pData, int size)
		{
			Buffer->SetArrrayValue(binder, index, pData, size);
		}
		void* GetVarPtrToWrite(const FShaderVarDesc& binder, UINT size)
		{
			return Buffer->GetVarPtrToWrite(binder, size);
		}
		void FlushDirty(ICommandList* cmd, bool clear = false)
		{
			Buffer->FlushDirty(cmd, clear);
		}
	public:
		AutoRef<FShaderBinder>	ShaderBinder;
		AutoRef<IBuffer>	Buffer;
	};

	struct TR_CLASS(SV_LayoutStruct = 8)
		FVbvDesc
	{
		UINT		Stride = 0;
		UINT		Size = 0;
		EGpuUsage	Usage = EGpuUsage::USAGE_DEFAULT;
		ECpuAccess	CpuAccess = (ECpuAccess)0;
		void*		InitData = nullptr;
	};
	class TR_CLASS()
		IVbView : public IGpuResource
	{
	public:
		void UpdateGpuData(ICommandList * cmd, UINT offset, void* pData, UINT size)
		{
			Buffer->UpdateGpuData(cmd, offset, pData, size);
		}
	public:
		FVbvDesc			Desc{};
		AutoRef<IBuffer>	Buffer;
	};

	struct TR_CLASS(SV_LayoutStruct = 8)
		FIbvDesc
	{
		UINT		Stride = sizeof(UINT);
		UINT		Size = 0;
		EGpuUsage	Usage = EGpuUsage::USAGE_DEFAULT;
		ECpuAccess	CpuAccess = (ECpuAccess)0;
		void*		InitData = nullptr;
	};
	class TR_CLASS()
		IIbView : public IGpuResource
	{
	public:
		void UpdateGpuData(ICommandList * cmd, UINT offset, void* pData, UINT size)
		{
			Buffer->UpdateGpuData(cmd, offset, pData, size);
		}
	public:
		FIbvDesc			Desc{};
		AutoRef<IBuffer>	Buffer;
	};

	struct TR_CLASS(SV_LayoutStruct = 8)
		FBuffer_SRV
	{
		union
		{
			UINT FirstElement;
			UINT ElementOffset;
		};
		union
		{
			UINT NumElements;
			UINT ElementWidth;
		};
		UINT StructureByteStride;
		UINT Flags;
	};

	struct TR_CLASS(SV_LayoutStruct = 8)
		FTex1D_SRV
	{
		UINT MostDetailedMip;
		UINT MipLevels;
	};

	struct TR_CLASS(SV_LayoutStruct = 8)
		FTex1D_Array_SRV
	{
		UINT MostDetailedMip;
		UINT MipLevels;
		UINT FirstArraySlice;
		UINT ArraySize;
	};

	struct TR_CLASS(SV_LayoutStruct = 8)
		FTex2D_SRV
	{
		UINT MostDetailedMip;
		UINT MipLevels;
		UINT PlaneSlice;
		FLOAT ResourceMinLODClamp;
	};

	struct TR_CLASS(SV_LayoutStruct = 8)
		FTex2D_Array_SRV
	{
		UINT MostDetailedMip;
		UINT MipLevels;
		UINT FirstArraySlice;
		UINT ArraySize;
	};

	struct TR_CLASS(SV_LayoutStruct = 8)
		FTex2DMS_SRV
	{
		UINT UnusedField_NothingToDefine;
	};

	struct TR_CLASS(SV_LayoutStruct = 8)
		FTex2DMS_Array_SRV
	{
		UINT FirstArraySlice;
		UINT ArraySize;
	};

	struct TR_CLASS(SV_LayoutStruct = 8)
		FTex3D_SRV
	{
		UINT MostDetailedMip;
		UINT MipLevels;
	};

	struct TR_CLASS(SV_LayoutStruct = 8)
		FTexCube_SRV
	{
		UINT MostDetailedMip;
		UINT MipLevels;
	};

	struct TR_CLASS(SV_LayoutStruct = 8)
		FTexCube_Array_SRV
	{
		UINT MostDetailedMip;
		UINT MipLevels;
		UINT First2DArrayFace;
		UINT NumCubes;
	};

	enum TR_ENUM(SV_EnumNoFlags)
		ESrvType
	{
		ST_BufferSRV,
		ST_Texture1D,
		ST_Texture1DArray,
		ST_Texture2D,
		ST_Texture2DArray,
		ST_Texture2DMS,
		ST_Texture2DMSArray,
		ST_Texture3D,
		ST_TextureCube,
		ST_TextureCubeArray,
	};

	struct TR_CLASS(SV_LayoutStruct = 8)
		FSrvDesc
	{
		void SetTexture2D()
		{
			memset(this, 0, sizeof(FSrvDesc));
			Type = ST_Texture2D;
			Format = PXF_R8G8B8A8_UNORM;
			Texture2D.MipLevels = 0;
			Texture2D.ResourceMinLODClamp = 0;
			Texture2D.PlaneSlice = 0;
		}
		void SetTexture2DArray()
		{
			memset(this, 0, sizeof(FSrvDesc));
			Type = ST_Texture2DArray;
			Format = PXF_R8G8B8A8_UNORM;
			Texture2DArray.MipLevels = 0;
		}
		void SetBuffer(DWORD flags)
		{
			memset(this, 0, sizeof(FSrvDesc));
			Type = ST_BufferSRV;
			Format = PXF_UNKNOWN;
			Buffer.FirstElement = 0;
			Buffer.NumElements = 0;
			Buffer.StructureByteStride = 0;
			Buffer.Flags = flags;
		}
		ESrvType Type;
		EPixelFormat Format;
		union
		{
			FBuffer_SRV Buffer;
			FTex1D_SRV Texture1D;
			FTex1D_Array_SRV Texture1DArray;
			FTex2D_SRV Texture2D;
			FTex2D_Array_SRV Texture2DArray;
			FTex2DMS_SRV Texture2DMS;
			FTex2DMS_Array_SRV Texture2DMSArray;
			FTex3D_SRV Texture3D;
			FTexCube_SRV TextureCube;
			FTexCube_Array_SRV TextureCubeArray;
		};
	};
	class TR_CLASS()
		ISrView : public IGpuResource
	{
	public:
		ENGINE_RTTI(ISrView);
		bool IsBufferView()
		{
			if (Desc.Type == ST_BufferSRV)
				return true;
			return false;
		}
		virtual bool UpdateBuffer(IGpuDevice* device, IGpuBufferData* buffer)  = 0;
		IGpuBufferData* GetBuffer() {
			return Buffer;
		}
		ITexture* GetBufferAsTexture();
		IBuffer* GetBufferAsBuffer();
		virtual FResourceState* GetResourceState() override{
			return &ResourceState;
		}
	public:
		TR_MEMBER(SV_NoBind)
		FResourceState			ResourceState;
		AutoRef<IGpuBufferData>	Buffer;
		FSrvDesc		Desc;
	};

	struct TR_CLASS(SV_LayoutStruct = 8)
		FBufferUAV
	{
		UINT FirstElement;
		UINT NumElements;
		UINT StructureByteStride;
		UINT Flags;
	};

	struct TR_CLASS(SV_LayoutStruct = 8)
		FTex1DUAV
	{
		UINT MipSlice;
	};

	struct TR_CLASS(SV_LayoutStruct = 8)
		FTex1DArrayUAV
	{
		UINT MipSlice;
		UINT FirstArraySlice;
		UINT ArraySize;
	};

	struct TR_CLASS(SV_LayoutStruct = 8)
		FTex2DUAV
	{
		UINT MipSlice;
		UINT PlaneSlice;
	};

	struct TR_CLASS(SV_LayoutStruct = 8)
		FTex2DArrayUAV
	{
		UINT MipSlice;
		UINT FirstArraySlice;
		UINT ArraySize;
		UINT PlaneSlice;
	};

	struct TR_CLASS(SV_LayoutStruct = 8)
		FTex3DUAV
	{
		UINT MipSlice;
		UINT FirstWSlice;
		UINT WSize;
	};

	enum TR_ENUM(SV_EnumNoFlags)
		EDimensionUAV
	{
		UAV_DIMENSION_UNKNOWN = 0,
		UAV_DIMENSION_BUFFER = 1,
		UAV_DIMENSION_TEXTURE1D = 2,
		UAV_DIMENSION_TEXTURE1DARRAY = 3,
		UAV_DIMENSION_TEXTURE2D = 4,
		UAV_DIMENSION_TEXTURE2DARRAY = 5,
		UAV_DIMENSION_TEXTURE3D = 8,
	};
	struct TR_CLASS(SV_LayoutStruct = 8)
		FUavDesc
	{
		void SetBuffer(UINT flags)
		{
			memset(this, 0, sizeof(FUavDesc));
			ViewDimension = EDimensionUAV::UAV_DIMENSION_BUFFER;
			Buffer.FirstElement = 0;
			Buffer.StructureByteStride = 0;
			Buffer.Flags = flags;//UAV_FLAG_RAW
			if (flags == 1)
			{
				Format = EPixelFormat::PXF_R32_TYPELESS;
			}
			else
			{
				Format = EPixelFormat::PXF_UNKNOWN;
			}
		}
		void SetTexture2D()
		{
			memset(this, 0, sizeof(FUavDesc));
			Format = EPixelFormat::PXF_UNKNOWN;
			ViewDimension = EDimensionUAV::UAV_DIMENSION_TEXTURE2D;
			Texture2D.MipSlice = 0;
			Texture2D.PlaneSlice = 0;
		}
		EPixelFormat Format;
		EDimensionUAV ViewDimension;
		union
		{
			FBufferUAV Buffer;
			FTex1DUAV Texture1D;
			FTex1DArrayUAV Texture1DArray;
			FTex2DUAV Texture2D;
			FTex2DArrayUAV Texture2DArray;
			FTex3DUAV Texture3D;
		};
	};
	class TR_CLASS()
		IUaView : public IGpuResource
	{
	public:
		ENGINE_RTTI(IUaView);
		FUavDesc		Desc;
		AutoRef<IGpuBufferData>	Buffer;
	};
	
	enum TR_ENUM()
		ERtvType
	{
		RTV_Buffer,
			RTV_Texture1D,
			RTV_Texture1DArray,
			RTV_Texture2D,
			RTV_Texture2DArray,
			RTV_Texture2DMS,
			RTV_Texture2DMSArray,
			RTV_Texture3D,
	};

	struct TR_CLASS(SV_LayoutStruct = 8)
		FBufferRTV
	{
		union
		{
			UINT FirstElement;
			UINT ElementOffset;
		};
		union
		{
			UINT NumElements;
			UINT ElementWidth;
		};
	};

	struct TR_CLASS(SV_LayoutStruct = 8)
		FTex1DRTV
	{
		UINT MipSlice;
	};

	struct TR_CLASS(SV_LayoutStruct = 8)
		FTex1DArrayRTV
	{
		UINT MipSlice;
		UINT FirstArraySlice;
		UINT ArraySize;
	};

	struct TR_CLASS(SV_LayoutStruct = 8)
		FTex2DRTV
	{
		UINT MipSlice;
		UINT PlaneSlice;
	};

	struct TR_CLASS(SV_LayoutStruct = 8)
		FTex2DArrayRTV
	{
		UINT MipSlice;
		UINT FirstArraySlice;
		UINT ArraySize;
	};
	struct TR_CLASS(SV_LayoutStruct = 8)
		FTex2DMSRTV
	{
		UINT UnusedField_NothingToDefine;
	};
	struct TR_CLASS(SV_LayoutStruct = 8)
		FTex2DMSArrayRTV
	{
		UINT FirstArraySlice;
		UINT ArraySize;
	};
	struct TR_CLASS(SV_LayoutStruct = 8)
		FTex3DRTV
	{
		UINT MipSlice;
		UINT FirstWSlice;
		UINT WSize;
	};

	struct TR_CLASS(SV_LayoutStruct = 8)
		FRtvDesc
	{
		FRtvDesc()
		{
			SetTexture2D();
		}
		void SetTexture2D()
		{
			memset(this, 0, sizeof(FRtvDesc));
			Type = RTV_Texture2D;
			GpuBuffer = nullptr;
			Width = 0;
			Height = 0;
			Format = PXF_R8G8B8A8_UNORM;
			Texture2D.MipSlice = 0;
			Texture2D.PlaneSlice = 0;
		}
		ERtvType					Type = RTV_Texture2D;
		UINT						Width = 0;
		UINT						Height = 0;
		EPixelFormat				Format = PXF_R8G8B8A8_UNORM;
		IGpuResource*				GpuBuffer = nullptr;
		union
		{
			FBufferRTV Buffer;
			FTex1DRTV Texture1D;
			FTex1DArrayRTV Texture1DArray;
			FTex2DRTV Texture2D;
			FTex2DArrayRTV Texture2DArray;
			FTex2DMSRTV Texture2DMS;
			FTex2DMSArrayRTV Texture2DMSArray;
			FTex3DRTV Texture3D;
		};
	};

	class TR_CLASS()
		IRenderTargetView : public IGpuResource
	{
	public:
		ENGINE_RTTI(IRenderTargetView);

		ITexture* GetTexture() {
			return GpuResource;
		}
	public:
		FRtvDesc					Desc;
		AutoRef<ITexture>			GpuResource;
	};

	struct TR_CLASS(SV_LayoutStruct = 8)
		FDsvDesc
	{
		FDsvDesc()
		{
			SetDefault();
		}
		void SetDefault()
		{
			Width = 0;
			Height = 0;
			Format = PXF_D24_UNORM_S8_UINT;
			CPUAccess = 0;
			GpuBuffer = nullptr;
			MipLevel = 1;
		}

		UINT					Width;
		UINT					Height;
		EPixelFormat			Format;
		UINT					CPUAccess;
		UINT					MipLevel;
		IGpuResource*			GpuBuffer;
	};

	class TR_CLASS()
		IDepthStencilView : public IGpuResource
	{
	public:
		ENGINE_RTTI(IDepthStencilView);
		ITexture* GetTexture() {
			return GpuResource;
		}
	public:
		FDsvDesc					Desc;
		AutoRef<ITexture>			GpuResource;
	};
}

NS_END
