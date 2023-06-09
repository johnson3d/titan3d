#pragma once
#include "../NxBuffer.h"
#include "NullPreHead.h"

NS_BEGIN

namespace NxRHI
{
	class NullGpuDevice;
	class NullBuffer : public IBuffer
	{
	public:
		NullBuffer();
		~NullBuffer();
		bool Init(NullGpuDevice* device, const FBufferDesc* desc);

		virtual void* GetHWBuffer() override{
			return nullptr;
		}
		virtual bool Map(UINT index, FMappedSubResource* res, bool forRead) override;
		virtual void Unmap(UINT index) override;
		virtual void UpdateGpuData(UINT subRes, void* pData, const FSubResourceFootPrint* footPrint) override;
	public:
		std::vector<BYTE>		mBuffer;
	};

	class NullTexture : public ITexture
	{
	public:
		NullTexture();
		~NullTexture();
		bool Init(NullGpuDevice* device, const FTextureDesc* desc);
		virtual void* GetHWBuffer() override {
			return nullptr;
		}
		virtual bool Map(UINT subRes, FMappedSubResource* res, bool forRead) override;
		virtual void Unmap(UINT subRes) override;
		virtual void UpdateGpuData(UINT subRes, void* pData, const FSubResourceFootPrint* footPrint) override;
		virtual IGpuBufferData* CreateBufferData(IGpuDevice* device, UINT mipIndex, ECpuAccess cpuAccess, FSubResourceFootPrint* outFootPrint) override;
	public:
	};

	class NullCbView : public ICbView
	{
	public:
		bool Init(NullGpuDevice* device, IBuffer* pBuffer, const FCbvDesc* desc);
	};

	class NullVbView : public IVbView
	{
	public:
		bool Init(NullGpuDevice* device, IBuffer* pBuffer, const FVbvDesc* desc);
	};

	class NullIbView : public IIbView
	{
	public:
		bool Init(NullGpuDevice* device, IBuffer* pBuffer, const FIbvDesc* desc);
	};

	class NullSrView : public ISrView
	{
	public:
		NullSrView();
		~NullSrView();
		virtual void* GetHWBuffer() override {
			return nullptr;
		}
		bool Init(NullGpuDevice* device, IGpuBufferData* pBffer, const FSrvDesc* desc);
		virtual bool UpdateBuffer(IGpuDevice* device, IGpuBufferData* buffer) override;
	public:
	};

	class NullUaView : public IUaView
	{
	public:
		NullUaView();
		~NullUaView();
		virtual void* GetHWBuffer() override {
			return nullptr;
		}
		bool Init(NullGpuDevice* device, IGpuResource* pBuffer, const FUavDesc* desc);
	public:
	};

	class NullRenderTargetView : public IRenderTargetView
	{
	public:
		NullRenderTargetView();
		~NullRenderTargetView();
		virtual void* GetHWBuffer() override {
			return nullptr;
		}
		bool Init(NullGpuDevice* device, IGpuResource* pBuffer, const FRtvDesc* desc);
	public:
	};

	class NullDepthStencilView : public IDepthStencilView
	{
	public:
		NullDepthStencilView();
		~NullDepthStencilView();
		virtual void* GetHWBuffer() override {
			return nullptr;
		}
		bool Init(NullGpuDevice* device, IGpuResource* pBuffer, const FDsvDesc* desc);
	public:
	};
}

NS_END
