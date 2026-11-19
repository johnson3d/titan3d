#pragma once
#include "NxGpuDevice.h"
#include "NxShader.h"
#include "NxRHIDefine.h"
#include "../Base/BlobObject.h"
#include <memory>

NS_BEGIN

namespace NxRHI
{
	class FMeshPrimitives;
	enum TR_ENUM(SV_EnumNoFlags)
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
		EBarrierAccess
	{
		BAS_Default = 0,
			BAS_IndirectRead = 0x00000001,
			BAS_IndexRead = 0x00000002,
			BAS_VertexRead = 0x00000004,
			BAS_CBufferRead = 0x00000008,
			BAS_InputStreamRead = 0x00000010,
			BAS_ShaderRead = 0x00000020,
			BAS_ShaderWrite = 0x00000040,
			BAS_RenderTargetRead = 0x00000080,
			BAS_RenderTargetWrite = 0x00000100,
			BAS_DepthStencilRead = 0x00000200,
			BAS_DepthStencilWrite = 0x00000400,
			BAS_CopyRead = 0x00000800,
			BAS_CopyWrite = 0x00001000,
			BAS_CpuRead = 0x00002000,
			BAS_CpuWrite = 0x00004000,
			BAS_MemoryRead = 0x00008000,
			BAS_MemoryWrite = 0x00010000,
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
			RM_COPY_SRC = 0x80000,
			RM_READ_ONLY = 0x100000,
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
			BFT_RTAS = (1 << 9),
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
	struct TR_CLASS(SV_LayoutStruct = 8)
		FBufferDesc
	{
		void SetDefault(bool isRaw = false, EBufferType types = BFT_CBuffer)
		{
			Type = types;
			
			Usage = USAGE_DEFAULT;
			CpuAccess = (ECpuAccess)0;
			if (isRaw)
				MiscFlags = EResourceMiscFlag::RM_BUFFER_ALLOW_RAW_VIEWS; 
			else
				MiscFlags = (EResourceMiscFlag)0;
			if (Type & EBufferType::BFT_IndirectArgs)
			{
				ASSERT(isRaw);
				MiscFlags = (EResourceMiscFlag)(MiscFlags | EResourceMiscFlag::RM_DRAWINDIRECT_ARGS);
			}
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
		FMappedSubResource*		InitData = nullptr;
	};

	enum TR_ENUM()
		EGpuResourceState
	{
		GRS_Undefine = 0,
			GRS_SrvPS,
			GRS_SrvNonPS,
			GRS_GenericRead,
			GRS_Uav,
			GRS_Indirect,
			GRS_RenderTarget,
			GRS_DepthStencil,
			GRS_DepthRead,
			GRS_StencilRead,
			GRS_DepthStencilRead,
			GRS_CopySrc,
			GRS_CopyDst,
			GRS_Present,
			//GRS_RTAS,
	};

	struct TR_CLASS(SV_LayoutStruct = 8)
		FSubResourceFootPrint
	{
		void SetDefault()
		{
			Format = EPixelFormat::PXF_UNKNOWN;
			X = 0;
			Y = 0;
			Z = 0;

			Width = 0;
			Height = 0;
			Depth = 0;

			RowPitch = 0;
			TotalSize = 0;
		}
		EPixelFormat Format = EPixelFormat::PXF_UNKNOWN;
		int X = 0;
		int Y = 0;
		int Z = 0;

		UINT Width = 0;
		UINT Height = 0;
		UINT Depth = 0;

		UINT RowPitch = 0;

		UINT TotalSize = 0;

		UINT GetOffset() const{
			return GetPixelByteWidth(Format) * X;
		}
		/*UINT GetSize() const{
			return GetPixelByteWidth(Format) * Width * Height * Depth;
		}*/
	};

	// Count 取此值表示从 Base 到末尾的全部 mip / layer
	constexpr UINT SubresourceRemaining = 0xFFFFFFFF;
	// 子资源范围 (mip 与 array layer 二维), 缺省构造即整资源
	struct TR_CLASS(SV_LayoutStruct = 8)
		FSubresourceRange
	{
		UINT BaseMip = 0;
		UINT MipCount = SubresourceRemaining;
		UINT BaseLayer = 0;
		UINT LayerCount = SubresourceRemaining;
		void SetWhole()
		{
			BaseMip = 0;
			MipCount = SubresourceRemaining;
			BaseLayer = 0;
			LayerCount = SubresourceRemaining;
		}
		void SetRange(UINT baseMip, UINT mipCount, UINT baseLayer, UINT layerCount)
		{
			BaseMip = baseMip;
			MipCount = mipCount;
			BaseLayer = baseLayer;
			LayerCount = layerCount;
		}
		// 按 ITexture::GetSubResource 的线性序号 (mip + layer * mipLevels) 取单个子资源
		void SetSubresource(UINT subResource, UINT mipLevels)
		{
			if (mipLevels == 0)
				mipLevels = 1;
			SetRange(subResource % mipLevels, 1, subResource / mipLevels, 1);
		}
		bool IsWhole() const
		{
			return BaseMip == 0 && MipCount == SubresourceRemaining && BaseLayer == 0 && LayerCount == SubresourceRemaining;
		}
		UINT GetMipEnd(UINT mipLevels) const
		{
			if (MipCount == SubresourceRemaining || BaseMip + MipCount > mipLevels)
				return mipLevels;
			return BaseMip + MipCount;
		}
		UINT GetLayerEnd(UINT arraySize) const
		{
			if (LayerCount == SubresourceRemaining || BaseLayer + LayerCount > arraySize)
				return arraySize;
			return BaseLayer + LayerCount;
		}
		bool Covers(UINT mipLevels, UINT arraySize) const
		{
			return BaseMip == 0 && GetMipEnd(mipLevels) == mipLevels && BaseLayer == 0 && GetLayerEnd(arraySize) == arraySize;
		}
		bool Overlaps(const FSubresourceRange& other, UINT mipLevels, UINT arraySize) const
		{
			return BaseMip < other.GetMipEnd(mipLevels) && other.BaseMip < GetMipEnd(mipLevels)
				&& BaseLayer < other.GetLayerEnd(arraySize) && other.BaseLayer < GetLayerEnd(arraySize);
		}
	};

	// view 被使用时对底层资源的要求: Range 内的子资源应处于 State; State 为 GRS_Undefine 表示不做自动转换
	struct TR_CLASS(SV_LayoutStruct = 8)
		FViewLayout
	{
		EGpuResourceState State = EGpuResourceState::GRS_Undefine;
		FSubresourceRange Range;
	};

	enum TR_ENUM(SV_EnumNoFlags)
		ELayoutTrackMode
	{
		LTM_Whole = 0,
		LTM_PerSubresource,
	};

	// 资源 layout 跟踪: 缺省 Whole 只记一个状态, 任何范围都按整资源转换;
	// PerSubresource 由创建期 FTextureDesc::LayoutTrack 声明 (如 HZB 不同 mip 同时作 SRV / UAV), 按 mip + layer * MipLevels 逐个记录
	class TR_CLASS()
		FLayoutTracker
	{
	public:
		// tracker 内嵌于资源, 托管端只能借用查询, 不能独立创建或释放
		TR_FUNCTION(SV_NoBind)
		FLayoutTracker() = default;
		TR_FUNCTION(SV_NoBind)
		void SetPerSubresource(UINT mipLevels, UINT arraySize)
		{
			// 逐子资源状态放堆上: 绝大多数资源是 Whole, 不能让 tracker 撑大 buffer 对象 (DX12Buffer / VKBuffer 须留在 small_alloc)
			Sub = std::make_unique<FSubStates>();
			Sub->MipLevels = mipLevels > 0 ? mipLevels : 1;
			Sub->ArraySize = arraySize > 0 ? arraySize : 1;
			Sub->States.assign(Sub->MipLevels * Sub->ArraySize, WholeState);
		}
		bool IsPerSubresource() const
		{
			return Sub != nullptr;
		}
		// 固定状态资源 (如 UPLOAD/READBACK heap 上的 DYNAMIC/STAGING buffer) 锁定后任何转换请求都忽略
		TR_FUNCTION(SV_NoBind)
		void LockState(EGpuResourceState state)
		{
			SetState(state);
			Locked = true;
		}
		bool IsLocked() const
		{
			return Locked;
		}
		UINT GetMipLevels() const
		{
			return Sub != nullptr ? Sub->MipLevels : 1;
		}
		UINT GetArraySize() const
		{
			return Sub != nullptr ? Sub->ArraySize : 1;
		}
		EGpuResourceState GetState() const
		{
			return GetState(FSubresourceRange{});
		}
		// 范围内状态一致时返回该状态, 否则返回 GRS_Undefine
		// range 必须非空; 托管端整资源查询用无参重载, 或显式 range.SetWhole()
		EGpuResourceState GetState(const FSubresourceRange& range) const
		{
			if (IsPerSubresource() == false)
				return WholeState;
			UINT mipBegin, mipEnd, layerBegin, layerEnd;
			Resolve(range, mipBegin, mipEnd, layerBegin, layerEnd);
			auto result = At(mipBegin, layerBegin);
			for (UINT layer = layerBegin; layer < layerEnd; layer++)
			{
				for (UINT mip = mipBegin; mip < mipEnd; mip++)
				{
					if (At(mip, layer) != result)
						return EGpuResourceState::GRS_Undefine;
				}
			}
			return result;
		}
		// 只记账不发 barrier
		TR_FUNCTION(SV_NoBind)
		void SetState(EGpuResourceState state, const FSubresourceRange& range = FSubresourceRange{})
		{
			if (IsPerSubresource() == false)
			{
				WholeState = state;
				return;
			}
			UINT mipBegin, mipEnd, layerBegin, layerEnd;
			Resolve(range, mipBegin, mipEnd, layerBegin, layerEnd);
			for (UINT layer = layerBegin; layer < layerEnd; layer++)
			{
				for (UINT mip = mipBegin; mip < mipEnd; mip++)
					At(mip, layer) = state;
			}
		}
		bool NeedTransition(const FSubresourceRange& range, EGpuResourceState toState) const
		{
			if (Locked)
				return false;
			if (IsPerSubresource() == false)
				return WholeState != toState;
			UINT mipBegin, mipEnd, layerBegin, layerEnd;
			Resolve(range, mipBegin, mipEnd, layerBegin, layerEnd);
			for (UINT layer = layerBegin; layer < layerEnd; layer++)
			{
				for (UINT mip = mipBegin; mip < mipEnd; mip++)
				{
					if (At(mip, layer) != toState)
						return true;
				}
			}
			return false;
		}
		// 先枚举范围内需要转换的段 onSegment(const FSubresourceRange& seg, EGpuResourceState from), 再把范围推进到 toState
		// Whole 模式或整资源状态一致时只产生一段整资源 (FSubresourceRange{}); 否则每个 layer 内按连续同源状态的 mip 合并
		template<class _Fn>
		TR_FUNCTION(SV_NoBind)
		void Transition(const FSubresourceRange& range, EGpuResourceState toState, _Fn&& onSegment)
		{
			if (Locked)
				return;
			if (IsPerSubresource() == false)
			{
				if (WholeState != toState)
				{
					onSegment(FSubresourceRange{}, WholeState);
					WholeState = toState;
				}
				return;
			}
			UINT mipBegin, mipEnd, layerBegin, layerEnd;
			Resolve(range, mipBegin, mipEnd, layerBegin, layerEnd);
			if (range.Covers(Sub->MipLevels, Sub->ArraySize))
			{
				auto cur = GetState();
				if (cur != EGpuResourceState::GRS_Undefine)
				{
					if (cur != toState)
					{
						onSegment(FSubresourceRange{}, cur);
						Sub->States.assign(Sub->States.size(), toState);
					}
					return;
				}
			}
			for (UINT layer = layerBegin; layer < layerEnd; layer++)
			{
				UINT mip = mipBegin;
				while (mip < mipEnd)
				{
					auto from = At(mip, layer);
					UINT runEnd = mip + 1;
					while (runEnd < mipEnd && At(runEnd, layer) == from)
						runEnd++;
					if (from != toState)
					{
						FSubresourceRange seg;
						seg.SetRange(mip, runEnd - mip, layer, 1);
						onSegment(seg, from);
						for (UINT i = mip; i < runEnd; i++)
							At(i, layer) = toState;
					}
					mip = runEnd;
				}
			}
		}
	private:
		struct FSubStates
		{
			UINT							MipLevels = 1;
			UINT							ArraySize = 1;
			std::vector<EGpuResourceState>	States;
		};
		EGpuResourceState& At(UINT mip, UINT layer)
		{
			return Sub->States[mip + layer * Sub->MipLevels];
		}
		EGpuResourceState At(UINT mip, UINT layer) const
		{
			return Sub->States[mip + layer * Sub->MipLevels];
		}
		void Resolve(const FSubresourceRange& range, UINT& mipBegin, UINT& mipEnd, UINT& layerBegin, UINT& layerEnd) const
		{
			mipEnd = range.GetMipEnd(Sub->MipLevels);
			layerEnd = range.GetLayerEnd(Sub->ArraySize);
			mipBegin = range.BaseMip < mipEnd ? range.BaseMip : mipEnd;
			layerBegin = range.BaseLayer < layerEnd ? range.BaseLayer : layerEnd;
			// 越界的 view 范围在 PerSubresource 下会漏转换
			ASSERT(mipBegin < mipEnd && layerBegin < layerEnd);
		}
		TR_MEMBER(SV_NoBind)
		EGpuResourceState				WholeState = EGpuResourceState::GRS_Undefine;
		TR_MEMBER(SV_NoBind)
		bool							Locked = false;
		TR_MEMBER(SV_NoBind)
		std::unique_ptr<FSubStates>		Sub;
	};

	class TR_CLASS()
		IGpuBufferDataBase : public IGpuResource
	{
	public:
		ENGINE_RTTI(IGpuBufferDataBase);
		virtual void Release() const override{
			IGpuResource::Release();
		}
		// 返回资源内嵌 tracker 的借用指针, 仅在资源存活期间有效
		FLayoutTracker* GetLayoutTracker()
		{
			return &LayoutTracker;
		}
		// native 内部查询整资源; PerSubresource 状态混合时返回 GRS_Undefine
		TR_FUNCTION(SV_NoBind)
		EGpuResourceState GetGpuResourceState() const {
			return LayoutTracker.GetState();
		}
		// 只登记 tracker 不发 GPU 命令: 创建期初始化或接管外部资源 (如交换链 back buffer) 时使用
		TR_FUNCTION(SV_NoBind)
		void SetGpuResourceState(EGpuResourceState state) {
			LayoutTracker.SetState(state);
		}
		// 只给 native 内部必然的转换使用; 外部通过 view 绑定由 drawcall/cmdlist 自动决定转换
		TR_FUNCTION(SV_NoBind)
		void TransitionTo(ICommandList* cmd, EGpuResourceState state) {
			TransitionTo(cmd, FSubresourceRange{}, state);
		}
		TR_FUNCTION(SV_NoBind)
		void TransitionTo(ICommandList* cmd, const FSubresourceRange& range, EGpuResourceState state) {
			LayoutTracker.Transition(range, state, [&](const FSubresourceRange& seg, EGpuResourceState from) {
				EmitTransition(cmd, seg, from, state);
			});
		}
		// 只发 GPU barrier 不改 tracker, 由 TransitionTo / FTransitionScope / IBarriersDraw 在 tracker 记账后调用; from != to
		TR_FUNCTION(SV_NoBind)
		virtual void EmitTransition(ICommandList* cmd, const FSubresourceRange& range, EGpuResourceState from, EGpuResourceState to) {
		}
		virtual bool FetchGpuData(IGpuDevice* device, UINT subRes, IBlobObject* blob) = 0;
		virtual IBuffer* CreateReadable(IGpuDevice* device, int subRes, ICopyDraw* cpDraw) = 0;
		virtual void UpdateGpuData(UINT subRes, void* pData, const FSubResourceFootPrint* footPrint) {
			ASSERT(false);
		}
		virtual void UpdateGpuData(ICommandList* cmd, UINT subRes, void* pData, const FSubResourceFootPrint* footPrint) {
			ASSERT(false);
		}
		void UpdateGpuDataSimple(ICommandList* cmd, UINT offset, void* pData, UINT size, UINT subRes = 0) {
			FSubResourceFootPrint fp{};
			fp.SetDefault();
			fp.Format = EPixelFormat::PXF_A8_UNORM;
			fp.Width = size;
			fp.Height = 1;
			fp.Depth = 1;
			fp.X = offset;
			fp.RowPitch = size;
			fp.TotalSize = size;
			UpdateGpuData(cmd, subRes, pData, &fp);
		}
		void UpdateGpuDataSimple(UINT offset, void* pData, UINT size, UINT subRes = 0) {
			FSubResourceFootPrint fp{};
			fp.SetDefault();
			fp.Format = EPixelFormat::PXF_A8_UNORM;
			fp.Width = size;
			fp.Height = 1;
			fp.Depth = 1;
			fp.X = offset;
			fp.RowPitch = size;
			fp.TotalSize = size;
			UpdateGpuData(subRes, pData, &fp);
		}
		virtual bool Map(UINT index, FMappedSubResource* res, bool forRead) = 0;
		virtual void Unmap(UINT index) = 0;
	public:
		TR_MEMBER(SV_NoBind)
		FLayoutTracker		LayoutTracker;
		bool IsAutoTransition = true;
	};
	class TR_CLASS(SV_Dispose = self->Release())
		IBuffer : public IGpuBufferDataBase
	{
	public:
		ENGINE_RTTI(IBuffer);
		virtual bool Map(UINT index, FMappedSubResource * res, bool forRead) override = 0;
		virtual void Unmap(UINT index) override = 0;
		void Flush2Device(ICommandList* cmd, void* pBuffer, UINT Size)
		{
			UpdateGpuDataSimple(cmd, 0, pBuffer, Size, 0);
		}
		void Flush2Device(void* pBuffer, UINT Size)
		{
			UpdateGpuDataSimple(0, pBuffer, Size, 0);
		}
		//virtual bool FetchGpuData(UINT subRes, IBlobObject * blob) override;
		virtual void UpdateGpuData(ICommandList* cmd, UINT subRes, void* pData, const FSubResourceFootPrint * footPrint) override = 0;
		virtual void SetDebugName(const char* name) override {}
		virtual bool FetchGpuData(IGpuDevice* device, UINT index, IBlobObject* blob) override;
		virtual IBuffer* CreateReadable(IGpuDevice* device, int subRes, ICopyDraw* cpDraw) override;
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
		bool SetValue(const FShaderVarDesc& binder, const void* pData, int size)
		{
			if (MapBuffer.size() < Desc.Size)
			{
				MapBuffer.resize(Desc.Size);
			}
			if (memcmp(&MapBuffer[binder.Offset], pData, size) == 0)
			{
				return false;
			}
			memcpy(&MapBuffer[binder.Offset], pData, size);
			DirtyState = EDirtyState::Dirty;
			return true;
		}
		bool SetArrrayValue(const FShaderVarDesc& binder, int index, const void* pData, int size)
		{
			auto stride = (binder.Size / binder.Elements);
			ASSERT((UINT)size <= stride);
			if (MapBuffer.size() < Desc.Size)
			{
				MapBuffer.resize(Desc.Size);
			}
			auto offset = binder.Offset + stride * (UINT)index;
			if (memcmp(&MapBuffer[offset], pData, size) == 0)
			{
				return false;
			}
			memcpy(&MapBuffer[offset], pData, size);
			DirtyState = EDirtyState::Dirty;
			return true;
		}
		void* GetVarPtrToWrite(const FShaderVarDesc& binder, UINT size)
		{
			if (binder.Size < size)
				return nullptr;
			if (MapBuffer.size() < Desc.Size)
			{
				MapBuffer.resize(Desc.Size);
			}
			DirtyState = EDirtyState::Dirty;
			return &MapBuffer[binder.Offset];
		}
		void FlushDirty(ICommandList* cmd, bool clear = false)
		{
			if (DirtyState == EDirtyState::NotDirty)
				return;
			DirtyState = EDirtyState::NotDirty;
			if (MapBuffer.size() > 0)
				Flush2Device(cmd, &MapBuffer[0], (UINT)MapBuffer.size());
			if (clear)
				MapBuffer.clear();
		}
		void FlushDirty(bool clear = false)
		{
			if (DirtyState == EDirtyState::NotDirty)
				return;
			DirtyState = EDirtyState::NotDirty;
			if (MapBuffer.size() > 0)
				Flush2Device(&MapBuffer[0], (UINT)MapBuffer.size());
			if (clear)
				MapBuffer.clear();
		}
		enum EDirtyState : char
		{
			Dirty = 0,
			WaitFlush,
			NotDirty,
		};
		inline bool IsDirty() const {
			return DirtyState == EDirtyState::Dirty;
		}
		inline void MarkDirty() {
			DirtyState = EDirtyState::Dirty;
		}
	public:
		FBufferDesc			Desc;
		std::vector<BYTE>	MapBuffer;
	private:
		EDirtyState			DirtyState = EDirtyState::Dirty;
	};

	class TR_CLASS()
		FUploadBuffer : public VIUnknown
	{
		AutoRef<IBuffer> Buffer;
		void* Ptr;
		UINT Size;
	public:
		FUploadBuffer(IBuffer* buffer)
		{
			Buffer = buffer;
			FMappedSubResource mapped{};
			if (Buffer->Map(0, &mapped, false))
			{
				Ptr = mapped.pData;
				Size = mapped.DepthPitch;
			}
		}
		~FUploadBuffer()
		{
			Dispose();
		}
		void Dispose() 
		{
			if (Ptr != nullptr)
			{
				Ptr = nullptr;
				Buffer->Unmap(0);
			}
		}
		IBuffer* GetBuffer() { return Buffer; }
		void* GetPtr() { return Ptr; }
		UINT GetSize() { return Size; }
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
			LayoutTrack = ELayoutTrackMode::LTM_Whole;
		}
		void SetShared() {
			MiscFlags = (EResourceMiscFlag)(MiscFlags | EResourceMiscFlag::RM_SHARED);
		}
		bool IsShared() const {
			return (MiscFlags & EResourceMiscFlag::RM_SHARED) != 0;
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
		// 只有不同子资源会同时处于不同 layout 的纹理 (如 HZB 逐级 mip 建 SRV/UAV) 才需 LTM_PerSubresource
		ELayoutTrackMode LayoutTrack = ELayoutTrackMode::LTM_Whole;
	};
	class TR_CLASS(SV_Dispose = self->Release())
		ITexture : public IGpuBufferDataBase
	{
	protected:
		static std::atomic<int> AliveCount;
		static std::atomic<int> AliveAttachBufferCount;
	public:
		ENGINE_RTTI(ITexture);
		static int GetAliveCount() {
			return AliveCount;
		}
		static int GetAliveAttachBufferCount() {
			return AliveAttachBufferCount;
		}
		ITexture()
		{
			AliveCount++;
		}
		~ITexture()
		{
			AliveCount--;
		}

		int GetDimension() const {
			if (Desc.Height == 0 && Desc.Depth == 0)
				return 1;
			else if (Desc.Depth == 0)
				return 2;
			else
				return 3;
		}
		UINT GetSubResource(UINT mipIndex, UINT arrayIndex = 0, UINT planeIndex = 0)
		{
			return mipIndex + arrayIndex * Desc.MipLevels;
		}
		// 各后端 Init 在设置 Desc 之后, 初始状态登记之前调用
		TR_FUNCTION(SV_NoBind)
		void InitLayoutTracker()
		{
			if (Desc.LayoutTrack == ELayoutTrackMode::LTM_PerSubresource)
				LayoutTracker.SetPerSubresource(Desc.MipLevels, Desc.ArraySize);
		}
		virtual void* GetSharedHandle() { return nullptr; }
		virtual bool Map(UINT index, FMappedSubResource* res, bool forRead) override = 0;
		virtual void Unmap(UINT index) override = 0;
		static void BuildImage2DBlob(IBlobObject * blob, void* pData, UINT RowPitch, const FTextureDesc* desc);
		static void BuildImage2DBlob(IBlobObject* blob, IBlobObject* gpuData, const FTextureDesc* desc);
		//virtual bool FetchGpuData(UINT subRes, IBlobObject * blob) override;
		virtual void SetDebugName(const char* name) override{}
		virtual FResourceState* GetResourceState() override {
			return &mResourceState;
		}
		virtual IGpuBufferDataBase* CreateBufferData(IGpuDevice* device, UINT mipIndex, ECpuAccess cpuAccess, FSubResourceFootPrint* outFootPrint) = 0;
		virtual bool FetchGpuData(IGpuDevice* device, UINT index, IBlobObject* blob) override
		{
			FMappedSubResource subRes;
			if (Map(index, &subRes, true))
			{
				blob->PushData(&subRes.RowPitch, sizeof(UINT));
				blob->PushData(&subRes.DepthPitch, sizeof(UINT));
				blob->PushData(subRes.pData, subRes.DepthPitch);
				Unmap(index);
				return true;
			}
			return false;
		}
		virtual IBuffer* CreateReadable(IGpuDevice* device, int subRes, ICopyDraw* cpDraw) override;
		virtual bool GetFootprint(FSubResourceFootPrint* fp, UINT64* rowSize, UINT64* totalSize, UINT subRes, UINT64 offset) { 
			ASSERT(false);
			return false; 
		}
	public:
		FTextureDesc		Desc{};
		FResourceState		mResourceState{};
		std::string			DebugName;
	};

	struct TR_CLASS(SV_LayoutStruct = 8)
		FCbvDesc
	{
		FShaderBinder*	ShaderBinder;
		UINT			BufferSize = 0;
		// 仅在 CBV 自建 buffer (pBuffer == nullptr) 时生效, 只接受 DYNAMIC / DEFAULT:
		// DYNAMIC: upload heap, CPU 直接 Map 写入, 单份内存会与 in-flight 帧竞争;
		// DEFAULT: 显存, 更新经 staging 拷贝在 GPU 队列中排队, 适合 cbPerCamera 这类跨帧一致性敏感的数据
		EGpuUsage		Usage = EGpuUsage::USAGE_DYNAMIC;
	};

	class TR_CLASS()
		IGpuViewBase : public IGpuResource
	{
	public:
		ENGINE_RTTI(IGpuViewBase);
		virtual IGpuBufferDataBase* GetBufferDataBase() = 0;
		// view 被 drawcall 绑定使用时底层资源哪些子资源应处的状态, 由 FTransitionScope::TryAutoTransition(cmd, view, ...) 自动转换
		// State 为 GRS_Undefine 表示不做自动转换 (如 CBV: DYNAMIC 在 upload heap 上不能转换)
		virtual FViewLayout GetViewLayout() const {
			return FViewLayout{};
		}
	};

	class ICbView;
	class TR_CLASS()
		FCbvUpdater : public VIUnknown
	{
		std::vector<AutoRef<ICbView>> mCBVs;
		VSLLock			mLocker;
	public:
		FCbvUpdater() {}
		inline void PushCbv(ICbView* cbv)
		{
			VAutoVSLLock lk(mLocker);
			mCBVs.push_back(cbv);
		}
		void UpdateCBVs();
	};
	class TR_CLASS()
		ICbView : public IGpuViewBase
	{
	public:
		ENGINE_RTTI(ICbView);
		IBuffer* GetBuffer() {
			return Buffer;
		}
		virtual IGpuBufferDataBase* GetBufferDataBase() override {
			return Buffer;
		}
		const FShaderBinder* GetShaderBinder() {
			return ShaderBinder;
		}
		template<class _T>
		void SetValue(const FShaderVarDesc& binder, const _T& v, bool bFlush, FCbvUpdater* pUpdater)
		{
			SetValue(binder, &v, sizeof(_T), bFlush, pUpdater);
		}
		template<class _T>
		void SetValue(const FShaderVarDesc* binder, const _T& v, bool bFlush, FCbvUpdater* pUpdater)
		{
			if (binder == nullptr)
			{
				ASSERT(false);
				return;
			}
			SetValue(*binder, v, bFlush, pUpdater);
		}
		template<class _T>
		void SetValue(const FShaderVarDesc* binder, int index, const _T& v, bool bFlush, FCbvUpdater* pUpdater)
		{
			if (binder == nullptr)
			{
				ASSERT(false);
				return;
			}
			SetArrrayValue(*binder, index, &v, sizeof(v), bFlush, pUpdater);
		}
		void SetMatrix(const FShaderVarDesc* binder, const v3dxMatrix4& v, bool bTranspose, bool bFlush, FCbvUpdater* pUpdater)
		{
			if (binder == nullptr)
			{
				ASSERT(false);
				return;
			}
			if (bTranspose)
			{
				v3dxMatrix4 tempM;
				v3dxTransposeMatrix4(&tempM, &v);
				SetValue(*binder, tempM, bFlush, pUpdater);
			}
			else
			{
				SetValue(*binder, v, bFlush, pUpdater);
			}
		}
		bool SetValue(const FShaderVarDesc& binder, const void* pData, int size, bool bFlush, FCbvUpdater* pUpdater)
		{
			if (Buffer->SetValue(binder, pData, size))
			{
				FlushWrite(bFlush, pUpdater);
				
				return true;
			}
			return false;
		}
		bool SetArrrayValue(const FShaderVarDesc& binder, int index, const void* pData, int size, bool bFlush, FCbvUpdater* pUpdater)
		{
			if (Buffer->SetArrrayValue(binder, index, pData, size))
			{
				FlushWrite(bFlush, pUpdater);
			}
			return false;
		}
		void* GetVarPtrToWrite(const FShaderVarDesc& binder, UINT size)
		{
			return Buffer->GetVarPtrToWrite(binder, size);
		}
		void FlushWrite(bool bFlush, FCbvUpdater* pUpdater)
		{
			if (bFlush)
			{
				if (pUpdater != nullptr)
				{
					if (Updater == nullptr)
					{
						Updater = pUpdater;
						Updater->PushCbv(this);
					}
				}
				else
				{
					this->FlushDirty(false);
				}
			}
		}
		void FlushDirty(ICommandList* cmd, bool clear = false)
		{
			Buffer->FlushDirty(cmd, clear);
		}
		void FlushDirty(bool clear = false)
		{
			Buffer->FlushDirty(clear);
		}
		inline bool IsDirty() const {
			return Buffer->IsDirty();
		}
		inline void MarkDirty() {
			Buffer->MarkDirty();
		}
	public:
		FCbvUpdater*		Updater = nullptr;
		AutoRef<FShaderBinder>	ShaderBinder;
		AutoRef<IBuffer>	Buffer;
	};

	struct TR_CLASS(SV_LayoutStruct = 8)
		FVbvDesc
	{
		UINT64		Offset = 0;
		UINT		Stride = 0;
		UINT		Size = 0;
		EGpuUsage	Usage = EGpuUsage::USAGE_DEFAULT;
		ECpuAccess	CpuAccess = (ECpuAccess)0;
		FMappedSubResource*		InitData = nullptr;
		void SetDefault()
		{
			Offset = 0;
			Stride = 0;
			Size = 0;
			Usage = EGpuUsage::USAGE_DEFAULT;
			CpuAccess = (ECpuAccess)0;
			InitData = nullptr;
		}
	};
	class TR_CLASS()
		IVbView : public IGpuViewBase
	{
	public:
		virtual IGpuBufferDataBase* GetBufferDataBase() override {
			return Buffer;
		}
		void UpdateGpuData(ICommandList* cmd, UINT offset, void* pData, UINT size)
		{
			Buffer->UpdateGpuDataSimple(cmd, (UINT)Desc.Offset + offset, pData, size, 0);
		}
		void UpdateGpuData(UINT offset, void* pData, UINT size)
		{
			Buffer->UpdateGpuDataSimple((UINT)Desc.Offset + offset, pData, size, 0);
		}
	public:
		FVbvDesc			Desc{};
		AutoRef<IBuffer>	Buffer;
	};

	struct TR_CLASS(SV_LayoutStruct = 8)
		FIbvDesc
	{
		UINT64		Offset = 0;
		UINT		Stride = sizeof(UINT);
		UINT		Size = 0;
		EGpuUsage	Usage = EGpuUsage::USAGE_DEFAULT;
		ECpuAccess	CpuAccess = (ECpuAccess)0;
		FMappedSubResource*		InitData = nullptr;

		void SetDefault()
		{
			Offset = 0;
			Stride = 0;
			Size = 0;
			Usage = EGpuUsage::USAGE_DEFAULT;
			CpuAccess = (ECpuAccess)0;
			InitData = nullptr;
		}
	};
	class TR_CLASS()
		IIbView : public IGpuViewBase
	{
	public:
		virtual IGpuBufferDataBase* GetBufferDataBase() override {
			return Buffer;
		}
		void UpdateGpuData(ICommandList* cmd, UINT offset, void* pData, UINT size)
		{
			Buffer->UpdateGpuDataSimple(cmd, (UINT)Desc.Offset + offset, pData, size, 0);
		}
		void UpdateGpuData(UINT offset, void* pData, UINT size)
		{
			Buffer->UpdateGpuDataSimple((UINT)Desc.Offset + offset, pData, size, 0);
		}
	public:
		FIbvDesc			Desc{};
		AutoRef<IBuffer>	Buffer;
	};

	// 把 buffer 作为 Indirect Draw/Dispatch 参数使用的 view
	// drawcall 只记录 view, 由 view 决定 Commit 时 buffer 应转换到的状态
	class TR_CLASS()
		IIndirectArgView : public IGpuViewBase
	{
	public:
		virtual IGpuBufferDataBase* GetBufferDataBase() override {
			return Buffer;
		}
		virtual FViewLayout GetViewLayout() const override {
			FViewLayout result;
			result.State = EGpuResourceState::GRS_Indirect;
			return result;
		}
		IBuffer* GetBuffer() {
			return Buffer;
		}
	public:
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
		ST_RTAS,
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
		void SetTexture3D()
		{
			memset(this, 0, sizeof(FSrvDesc));
			Type = ST_Texture3D;
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
		void SetBuffer(bool isRaw)
		{
			memset(this, 0, sizeof(FSrvDesc));
			Type = ST_BufferSRV;
			Buffer.FirstElement = 0;
			Buffer.NumElements = 0;
			Buffer.StructureByteStride = 0;

			if (isRaw)
			{
				Format = EPixelFormat::PXF_R32_TYPELESS;
				Buffer.Flags = 1;
			}
			else
			{
				Format = EPixelFormat::PXF_UNKNOWN;
				Buffer.Flags = 0;
			}
		}
		void SetRTAS()
		{
			memset(this, 0, sizeof(FSrvDesc));
			Type = ST_RTAS;
			RTASLocation = 0;
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
			UINT64 RTASLocation;
		};
	};
	class TR_CLASS()
		ISrView : public IGpuViewBase
	{
	public:
		ENGINE_RTTI(ISrView);
		bool IsBufferView()
		{
			if (Desc.Type == ST_BufferSRV)
				return true;
			return false;
		}
		//must run on render thread: render thread reads the view while recording/binding descriptors
		virtual bool UpdateBuffer_OnRenderThread(IGpuDevice* device, IGpuBufferDataBase* buffer)  = 0;
		IGpuBufferDataBase* GetBuffer() {
			return Buffer;
		}
		virtual IGpuBufferDataBase* GetBufferDataBase() override {
			return Buffer;
		}
		virtual FViewLayout GetViewLayout() const override;
		ITexture* GetBufferAsTexture();
		IBuffer* GetBufferAsBuffer();
		virtual FResourceState* GetResourceState() override{
			return &ResourceState;
		}
		virtual int AddRef() const override;
		virtual void Release() const override;
		
	public:
		TR_MEMBER(SV_NoBind)
		FResourceState			ResourceState;
		AutoRef<IGpuBufferDataBase>	Buffer;
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
		void SetBuffer(bool isRaw)
		{
			memset(this, 0, sizeof(FUavDesc));
			ViewDimension = EDimensionUAV::UAV_DIMENSION_BUFFER;
			Buffer.FirstElement = 0;
			Buffer.StructureByteStride = 0;
			if (isRaw)
			{
				Format = EPixelFormat::PXF_R32_TYPELESS;
				Buffer.Flags = 1;
			}
			else
			{
				Format = EPixelFormat::PXF_UNKNOWN;
				Buffer.Flags = 0;
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
		IUaView : public IGpuViewBase
	{
	public:
		ENGINE_RTTI(IUaView);
		virtual FViewLayout GetViewLayout() const override;
		virtual IGpuBufferDataBase* GetBufferDataBase() override {
			return Buffer;
		}
	public:
		FUavDesc		Desc;
		AutoRef<IGpuBufferDataBase>	Buffer;
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
		IRenderTargetView : public IGpuViewBase
	{
	public:
		ENGINE_RTTI(IRenderTargetView);
		virtual FViewLayout GetViewLayout() const override;

		ITexture* GetTexture() {
			return GpuResource;
		}
		virtual IGpuBufferDataBase* GetBufferDataBase() override {
			return GpuResource;
		}
	public:
		FRtvDesc					Desc;
		AutoRef<ITexture>			GpuResource;
	};

	enum TR_ENUM()
		EDsvType
	{
		DSV_Buffer,
			DSV_Texture1D,
			DSV_Texture1DArray,
			DSV_Texture2D,
			DSV_Texture2DArray,
			DSV_Texture2DMS,
			DSV_Texture2DMSArray,
			DSV_Texture3D,
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
			Type = DSV_Texture2D;
			Width = 0;
			Height = 0;
			Format = PXF_D24_UNORM_S8_UINT;
			CPUAccess = 0;
			GpuBuffer = nullptr;
			ArrayIndex = 0xFFFFFFFF;
			MipLevel = 1;
		}
		EDsvType				Type = DSV_Texture2D;
		UINT					Width;
		UINT					Height;
		EPixelFormat			Format;
		UINT					CPUAccess;
		UINT					ArrayIndex;	
		UINT					MipLevel;
		IGpuResource*			GpuBuffer;
	};

	class TR_CLASS()
		IDepthStencilView : public IGpuViewBase
	{
	public:
		ENGINE_RTTI(IDepthStencilView);
		virtual FViewLayout GetViewLayout() const override;
		ITexture* GetTexture() {
			return GpuResource;
		}
		virtual IGpuBufferDataBase* GetBufferDataBase() override {
			return GpuResource;
		}
	public:
		FDsvDesc					Desc;
		AutoRef<ITexture>			GpuResource;
	};

	enum ERayTracingGeomFlags
	{
		RTG_NONE = 0,
		RTG_OPAQUE = 0x1,
		RTG_NO_DUPLICATE_ANYHIT_INVOCATION = 0x2
	};

	struct TR_CLASS(SV_LayoutStruct = 8)
		FAccelerationStructureDesc
	{
		void SetDefault()
		{
			GeometryCount = 0;
			Geometries = nullptr;
			GeomFlags = nullptr;
		}
		//normally, we only need one geometry,other wise, we need to build a complex SBT and manage the InstanceContributionToHitGroupIndex
		UINT 					GeometryCount = 0;
		FMeshPrimitives**		Geometries = nullptr;
		ERayTracingGeomFlags*	GeomFlags = nullptr;
	};

	class TR_CLASS()
		IAccelerationStructure : public IGpuResource
	{
	public:
		ENGINE_RTTI(IAccelerationStructure);
	public:
		AutoRef<IBuffer>		mGpuBuffer;
		UINT					mScratchSize = 0;
	};

	enum TR_ENUM()
		ERayTracingInstanceFlags : uint32
	{
		RTI_FLAG_NONE = 0,
		RTI_FLAG_TRIANGLE_CULL_DISABLE = 0x1,
		RTI_FLAG_TRIANGLE_FRONT_COUNTERCLOCKWISE = 0x2,
		RTI_FLAG_FORCE_OPAQUE = 0x4,
		RTI_FLAG_FORCE_NON_OPAQUE = 0x8
	};

	struct TR_CLASS(SV_LayoutStruct = 8)
		FAStructureInstanceDesc
	{
		void SetDefault()
		{
			InstanceContributionToHitGroupIndex = 0;
			InstanceID = 0;
			InstanceMask = 0;
			Flags = ERayTracingInstanceFlags::RTI_FLAG_NONE;
			Matrix = Matrix.IDENTITY;
		}
		UINT								InstanceContributionToHitGroupIndex = 0;
		UINT								InstanceID = 0;
		UINT								InstanceMask = 0;//Bit and with HLSL TraceRay(,,Mask,,,,)
		ERayTracingInstanceFlags			Flags = ERayTracingInstanceFlags::RTI_FLAG_NONE;
		v3dxMatrix4							Matrix = Matrix.IDENTITY;
	};

	class TR_CLASS()
		IAStructureInstance : public IGpuResource
	{
	public:
		FAStructureInstanceDesc				mDesc{};
		AutoRef<IAccelerationStructure>		mAStructure;
		FAStructureInstanceDesc* GetDescPtr() {
			return &mDesc;
		}
		void SetMatrix(const v3dxMatrix4* pMatrix) {
			mDesc.Matrix = *pMatrix;
		}
	};

	struct TR_CLASS(SV_LayoutStruct = 8)
		FTopAccelerationStructureDesc
	{
		void SetDefault()
		{
			
		}
	};
	class TR_CLASS()
		ITopAccelerationStructure : public IGpuResource
	{
	public:
		ENGINE_RTTI(ITopAccelerationStructure);
	public:
		AutoRef<IBuffer>		mGpuScratchBuffer;
		AutoRef<IBuffer>		mGpuBuffer;
		AutoRef<ISrView>		mGpuBufferSRV;
		AutoRef<IUaView>		mGpuBufferUAV;
		AutoRef<FUploadBuffer>	mInstanceGpuBuffer;
		UINT					mScratchSize = 0;
		IBuffer* GetGpuBuffer() {
			return mGpuBuffer;
		}
		ISrView* GetGpuBufferSRV() {
			return mGpuBufferSRV;
		}
		IUaView* GetGpuBufferUAV() {
			return mGpuBufferUAV;
		}
		FUploadBuffer* GetInstanceGpuBuffer() {
			return mInstanceGpuBuffer;
		}
		std::vector<AutoRef<IAStructureInstance>>	mBottomASInstances;
		UINT GetBLASInstanceCount() const
		{
			return (UINT)mBottomASInstances.size();
		}
		IAStructureInstance* GetBLASInstance(UINT index)
		{
			return mBottomASInstances[index];
		}
		void AddBLASInstance(IAStructureInstance* instance)
		{
			mBottomASInstances.push_back(instance);
		}
		virtual bool BuildAcclerationStruture() = 0;
	};

	class TR_CLASS()
		IBindless : public IGpuResource
	{
	public:
		static const UINT MaxBindless = 4096;
		static UINT GetMaxBindless() {
			return IBindless::MaxBindless;
		}
		bool mIsCopyNull = false;
		EShaderBindType mBindType;
		UINT GetResourceCount() const {
			return (UINT)mResources.size();
		}
		bool SetResource(UINT index, IGpuResource* resource) {
			if (index >= GetResourceCount())
				return false;

			auto& saved = mResources[index];
			if (resource == saved.Resource)
				return true;
			saved.SetResource(resource);
			OnBind(index, resource);
			return true;
		}

		virtual void OnBind(UINT index, IGpuResource* resource) = 0;

		void BindResources() {
			for (size_t i = 0; i < mResources.size(); i++)
			{
				if (mIsCopyNull == false && mResources[i].Resource == nullptr)
					continue;
				OnBind((UINT)i, mResources[i].Resource);
			}
		}
		//dont call it
		void CheckResourceFingerPrint() {
			for (size_t i = 0; i < mResources.size(); i++)
			{
				if (mResources[i].Resource == nullptr)
				{
					continue;
				}
				if (mResources[i].FingerPrint != mResources[i].Resource->GetFingerPrint())
				{
					OnBind((UINT)i, mResources[i].Resource);
					mResources[i].FingerPrint = mResources[i].Resource->GetFingerPrint();
				}
			}
		}
		std::vector<FBindResource>	mResources;
	};
}

NS_END
