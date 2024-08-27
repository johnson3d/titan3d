#include "DX11Buffer.h"
#include "DX11CommandList.h"
#include "DX11GpuDevice.h"

#define new VNEW

NS_BEGIN

namespace NxRHI
{
	DX11Buffer::DX11Buffer()
	{
		mBuffer = nullptr;
	}

	DX11Buffer::~DX11Buffer()
	{
		Safe_Release(mBuffer);
	}

	bool DX11Buffer::Init(DX11GpuDevice* device, const FBufferDesc& desc)
	{
		mDeviceRef.FromObject(device);

		Desc = desc;
		Desc.InitData = nullptr;
		
		D3D11_BUFFER_DESC BufferDesc;
		memset(&BufferDesc, 0, sizeof(BufferDesc));
		BufferDesc.Usage = (D3D11_USAGE)desc.Usage;
		BufferDesc.ByteWidth = desc.Size;
		BufferDesc.CPUAccessFlags = (UINT)desc.CpuAccess;// D3D11_CPU_ACCESS_WRITE;
		/*if (desc.CpuAccess & ECpuAccess::CAS_WRITE)
		{
			BufferDesc.CPUAccessFlags |= ECpuAccess::CAS_READ;
		}*/
		BufferDesc.MiscFlags = desc.MiscFlags;
		BufferDesc.StructureByteStride = desc.StructureStride;
		EBufferType descType = desc.Type;
		if (BufferDesc.Usage == D3D11_USAGE::D3D11_USAGE_STAGING)
		{
			BufferDesc.MiscFlags = 0;
			descType = EBufferType::BFT_NONE;
		}
		
		if (descType & BFT_CBuffer)
		{
			BufferDesc.BindFlags |= D3D11_BIND_CONSTANT_BUFFER;
			//BufferDesc.CPUAccessFlags = D3D11_CPU_ACCESS_WRITE;
		}
		if (descType & EBufferType::BFT_UAV)
		{
			BufferDesc.BindFlags |= D3D11_BIND_UNORDERED_ACCESS;
			BufferDesc.CPUAccessFlags = 0;
			BufferDesc.Usage = D3D11_USAGE_DEFAULT;
			if ((descType & EBufferType::BFT_IndirectArgs) == 0)
				BufferDesc.MiscFlags |= D3D11_RESOURCE_MISC_BUFFER_STRUCTURED;
		}
		if (descType & EBufferType::BFT_SRV)
		{
			BufferDesc.BindFlags |= D3D11_BIND_SHADER_RESOURCE;
			if (BufferDesc.StructureByteStride != 0)
				BufferDesc.MiscFlags |= D3D11_RESOURCE_MISC_BUFFER_STRUCTURED;
		}
		if (descType & EBufferType::BFT_Vertex)
		{
			BufferDesc.BindFlags |= D3D11_BIND_VERTEX_BUFFER;
		}
		if (descType & EBufferType::BFT_Index)
		{
			BufferDesc.BindFlags |= D3D11_BIND_INDEX_BUFFER;
		}
		if (descType & EBufferType::BFT_IndirectArgs)
		{
			BufferDesc.MiscFlags |= D3D11_RESOURCE_MISC_DRAWINDIRECT_ARGS;
		}
		if (descType & EBufferType::BFT_RAW)
		{
			BufferDesc.MiscFlags &= ~D3D11_RESOURCE_MISC_BUFFER_STRUCTURED;
			BufferDesc.MiscFlags |= D3D11_RESOURCE_MISC_BUFFER_ALLOW_RAW_VIEWS;
		}
		if (desc.CpuAccess == ECpuAccess::CAS_READ)
		{
			BufferDesc.Usage = D3D11_USAGE_STAGING;
			BufferDesc.BindFlags = 0;
			//BufferDesc.MiscFlags = 0;
			BufferDesc.CPUAccessFlags = D3D11_CPU_ACCESS_WRITE | D3D11_CPU_ACCESS_READ;
		}
		if (BufferDesc.MiscFlags & D3D11_RESOURCE_MISC_BUFFER_ALLOW_RAW_VIEWS)
		{
			BufferDesc.MiscFlags &= ~D3D11_RESOURCE_MISC_BUFFER_STRUCTURED;
		}

		ASSERT((BufferDesc.MiscFlags & (D3D11_RESOURCE_MISC_BUFFER_ALLOW_RAW_VIEWS | D3D11_RESOURCE_MISC_BUFFER_STRUCTURED)) != (D3D11_RESOURCE_MISC_BUFFER_ALLOW_RAW_VIEWS | D3D11_RESOURCE_MISC_BUFFER_STRUCTURED));
		HRESULT hr = S_OK;
		if (desc.InitData != nullptr)
		{
			D3D11_SUBRESOURCE_DATA data;
			data.pSysMem = desc.InitData;
			data.SysMemPitch = desc.Size;
			data.SysMemSlicePitch = 0;
			hr = device->mDevice->CreateBuffer(&BufferDesc, &data, &mBuffer);
		}
		else
		{
			hr = device->mDevice->CreateBuffer(&BufferDesc, nullptr, &mBuffer);
		}
		if (FAILED(hr))
			return false;

		return true;
	}

	void DX11Buffer::UpdateGpuData(ICommandList* cmd1, UINT subRes, void* pData, const FSubResourceFootPrint* footPrint)
	{
		if (Desc.Usage == USAGE_DEFAULT)
		{
			auto cmd = (DX11CommandList*)cmd1;

			auto device = mDeviceRef.GetPtr();
			//auto cmd = ((DX11CmdQueue*)device->GetCmdQueue())->mHardwareContext;
			D3D11_BOX box{};
			box.left = footPrint->X;
			box.right = footPrint->X + footPrint->Width;
			box.top = footPrint->Y;
			box.bottom = footPrint->Y + footPrint->Height;
			box.front = footPrint->Z;
			box.back = footPrint->Z + footPrint->Depth;
			((DX11CommandList*)cmd)->mContext4->UpdateSubresource1(mBuffer, subRes, &box, pData, footPrint->RowPitch, footPrint->TotalSize, D3D11_COPY_NO_OVERWRITE);
			if (cmd->mCmdRecorder != nullptr)
				cmd->mCmdRecorder->mDirectDrawNum++;
		}
		else
		{
			auto device = mDeviceRef.GetPtr();

			auto cmd = (DX11CommandList*)cmd1;
			/*auto cmdQueue = ((DX11CmdQueue*)device->GetCmdQueue());
			auto cmd = cmdQueue->mHardwareContext;
			VAutoVSLLock locker(cmdQueue->mImmCmdListLocker);*/

			D3D11_MAP flags = D3D11_MAP_WRITE;
			if (Desc.Usage == USAGE_DYNAMIC)
				flags = D3D11_MAP_WRITE_DISCARD;
			else
			{
				ASSERT(false);
				flags = D3D11_MAP_WRITE;
			}

			D3D11_MAPPED_SUBRESOURCE mapData;
			auto ret = cmd->mContext->Map(mBuffer, subRes, flags, 0, (D3D11_MAPPED_SUBRESOURCE*)&mapData);
			if (ret == S_OK)
			{
				memcpy(mapData.pData, pData, footPrint->TotalSize);
				cmd->mContext->Unmap(mBuffer, subRes);
			}
			else
			{
				ASSERT(false);
			}
			if (cmd->mCmdRecorder != nullptr)
				cmd->mCmdRecorder->mDirectDrawNum++;
			/*FMappedSubResource mapData;
			if (true == this->Map(0, &mapData, false))
			{
				memcpy(mapData.pData, pData, footPrint->TotalSize);
				this->Unmap(0);
			}*/
		}
	}

	void DX11Buffer::UpdateGpuData(UINT subRes, void* pData, const FSubResourceFootPrint* footPrint)
	{
		auto device = mDeviceRef.GetPtr();

		if (Desc.Usage == USAGE_DEFAULT)
		{
			auto device = mDeviceRef.GetPtr();
			auto cmd = ((DX11CmdQueue*)device->GetCmdQueue())->mHardwareContext;
			((DX11CommandList*)cmd)->mContext->UpdateSubresource(mBuffer, subRes, nullptr, pData, footPrint->RowPitch, footPrint->TotalSize);
			if (cmd->mCmdRecorder != nullptr)
				cmd->mCmdRecorder->mDirectDrawNum++;
		}
		else
		{
			auto cmdQueue = ((DX11CmdQueue*)device->GetCmdQueue());
			auto cmd = cmdQueue->mHardwareContext;
			VAutoVSLLock locker(cmdQueue->mImmCmdListLocker);
			D3D11_MAP flags = D3D11_MAP_WRITE;
			if (Desc.Usage == USAGE_DYNAMIC)
				flags = D3D11_MAP_WRITE_DISCARD;
			else
				flags = D3D11_MAP_WRITE;

			D3D11_MAPPED_SUBRESOURCE mapData;
			auto ret = cmd->mContext->Map(mBuffer, subRes, flags, 0, (D3D11_MAPPED_SUBRESOURCE*)&mapData);
			if (ret == S_OK)
			{
				memcpy(mapData.pData, pData, footPrint->TotalSize);
				cmd->mContext->Unmap(mBuffer, subRes);
			}
			else
			{
				ASSERT(false);
			}
			if (cmd->mCmdRecorder != nullptr)
				cmd->mCmdRecorder->mDirectDrawNum++;
		}
	}
	bool DX11Buffer::Map(UINT index, FMappedSubResource* res, bool forRead)
	{
		ASSERT(mMappingCmdList == nullptr);
		auto device = mDeviceRef.GetPtr();
		D3D11_MAP flags = D3D11_MAP_READ;
		if (forRead)
		{
			if (this->Desc.Usage == USAGE_DEFAULT)
				return false;
			mMappingCmdList = ((DX11CmdQueue*)device->GetCmdQueue())->mHardwareContext;
			//cmd->Flush();
			flags = D3D11_MAP_READ;
		}
		else
		{
			if (Desc.Usage == USAGE_DYNAMIC)
			{//If you call Map on a deferred context, you can only pass D3D11_MAP_WRITE_DISCARD,
				mMappingCmdList = (DX11CommandList*)device->GetCmdQueue()->GetIdleCmdlist();// ((DX11CmdQueue*)device->GetCmdQueue())->mHardwareContext;
				mMappingCmdList->BeginCommand();
				flags = D3D11_MAP_WRITE_DISCARD;
			}
			else
			{
				mMappingCmdList = ((DX11CmdQueue*)device->GetCmdQueue())->mHardwareContext;
				flags = D3D11_MAP_WRITE;
			}
		}

		auto ret = mMappingCmdList->mContext->Map(mBuffer, index, flags, 0, (D3D11_MAPPED_SUBRESOURCE*)res);
		res->RowPitch = Desc.RowPitch;
		res->DepthPitch = Desc.DepthPitch;
		//ASSERT(ret == S_OK);
		return ret == S_OK;
	}

	void DX11Buffer::Unmap(UINT index)
	{
		ASSERT(mMappingCmdList != nullptr);
		mMappingCmdList->mContext->Unmap(mBuffer, index);
		if (mMappingCmdList->IsImmContext == false)
		{
			auto device = mDeviceRef.GetPtr();
			mMappingCmdList->FlushDraws();
			mMappingCmdList->EndCommand();
			device->GetCmdQueue()->ExecuteCommandListSingle(mMappingCmdList, EQueueType::QU_Default);
			device->GetCmdQueue()->ReleaseIdleCmdlist(mMappingCmdList);
			mMappingCmdList = nullptr;
		}
		else
		{
			mMappingCmdList = nullptr;
		}
	}

	void DX11Buffer::SetDebugName(const char* name)
	{
		mBuffer->SetPrivateData(WKPDID_D3DDebugObjectName, 0, NULL);
		std::string debuginfo = name;
		mBuffer->SetPrivateData(WKPDID_D3DDebugObjectName, (UINT)debuginfo.length(), debuginfo.c_str());
	}
	
	DX11Texture::DX11Texture()
	{
		mTexture1D = nullptr;
	}

	DX11Texture::~DX11Texture()
	{
		Safe_Release(mTexture1D);
	}
	
	UINT BufferTypeToDXBindFlags(EBufferType type)
	{
		UINT flags = 0;
		if (type & EBufferType::BFT_CBuffer)
		{
			flags |= D3D11_BIND_VERTEX_BUFFER;
		}
		if (type & EBufferType::BFT_IndirectArgs)
		{
			
		}
		if (type & EBufferType::BFT_Index)
		{
			flags |= D3D11_BIND_INDEX_BUFFER;
		}
		if (type & EBufferType::BFT_RAW)
		{
			//flags |= D3D11_BIND_INDEX_BUFFER;
		}
		if (type & EBufferType::BFT_SRV)
		{
			flags |= D3D11_BIND_SHADER_RESOURCE;
		}
		if (type & EBufferType::BFT_UAV)
		{
			flags |= D3D11_BIND_UNORDERED_ACCESS;
		}
		if (type & EBufferType::BFT_RTV)
		{
			flags |= D3D11_BIND_RENDER_TARGET;
		}
		if (type & EBufferType::BFT_DSV)
		{
			flags |= D3D11_BIND_DEPTH_STENCIL;
		}
		return flags;
	}
	void* DX11Texture::GetSharedHandle()
	{
		//device->mDevice->OpenSharedResource(,IID_IDXGIResource, )
		IDXGIResource* dxgiRes;
		auto hr = mTexture1D->QueryInterface(IID_IDXGIResource, (void**) & dxgiRes);
		if (FAILED(hr))
			return nullptr;
		void* result;
		hr = dxgiRes->GetSharedHandle(&result);
		dxgiRes->Release();
		if (FAILED(hr))
		{	
			return nullptr;
		}
		return result;
	}
	bool DX11Texture::Init(DX11GpuDevice* device, void* pSharedObject)
	{
		mDeviceRef.FromObject(device);
		IDXGIResource* dxgiRes;
		auto hr = device->mDevice->OpenSharedResource(pSharedObject, IID_IDXGIResource, (void**)&dxgiRes);
		if (FAILED(hr))
			return false;

		
		hr = dxgiRes->QueryInterface(__uuidof(ID3D11Texture1D), (void**)(&mTexture1D));
		if (FAILED(hr))
		{
			hr = dxgiRes->QueryInterface(__uuidof(ID3D11Texture2D), (void**)(&mTexture2D));
			if (FAILED(hr))
			{
				hr = dxgiRes->QueryInterface(__uuidof(ID3D11Texture3D), (void**)(&mTexture3D));
				dxgiRes->Release();
				if (FAILED(hr))
				{
					return false;
				}
				else
				{
					D3D11_TEXTURE3D_DESC td;
					mTexture3D->GetDesc(&td);
					dxgiRes->Release();

					Desc.Width = td.Width;
					Desc.Height = td.Height;
					Desc.Depth = td.Depth;
					Desc.Format = DXFormatToFormat(td.Format);
					Desc.ArraySize = 1;
					//Desc.BindFlags = BufferTypeToDXBindFlags(desc.BindFlags);
					Desc.CpuAccess = (ECpuAccess)td.CPUAccessFlags;
					Desc.MipLevels = td.MipLevels;
					Desc.Usage = (EGpuUsage)td.Usage;
					Desc.MiscFlags = (EResourceMiscFlag)td.MiscFlags;
				}
			}
			else
			{
				D3D11_TEXTURE2D_DESC td;
				mTexture2D->GetDesc(&td);
				dxgiRes->Release();

				Desc.Width = td.Width;
				Desc.Height = td.Height;
				Desc.Depth = 0;
				Desc.Format = DXFormatToFormat(td.Format);
				Desc.ArraySize = 1;
				//Desc.BindFlags = BufferTypeToDXBindFlags(desc.BindFlags);
				Desc.CpuAccess = (ECpuAccess)td.CPUAccessFlags;
				Desc.MipLevels = td.MipLevels;
				Desc.Usage = (EGpuUsage)td.Usage;
				Desc.MiscFlags = (EResourceMiscFlag)td.MiscFlags;
			}
		}
		else
		{
			D3D11_TEXTURE1D_DESC td;
			mTexture1D->GetDesc(&td);
			dxgiRes->Release();

			Desc.Width = td.Width;
			Desc.Height = 0;
			Desc.Depth = 0;
			Desc.Format = DXFormatToFormat(td.Format);
			Desc.ArraySize = 1;
			//Desc.BindFlags = BufferTypeToDXBindFlags(desc.BindFlags);
			Desc.CpuAccess = (ECpuAccess)td.CPUAccessFlags;
			Desc.MipLevels = td.MipLevels;
			Desc.Usage = (EGpuUsage)td.Usage;
			Desc.MiscFlags = (EResourceMiscFlag)td.MiscFlags;
		}
		
		return true;
	}
	bool DX11Texture::Init(DX11GpuDevice* device, const FTextureDesc& desc)
	{
		mDeviceRef.FromObject(device);
		Desc = desc;
		if (Desc.CpuAccess & ECpuAccess::CAS_READ)
		{
			ASSERT(Desc.Usage == EGpuUsage::USAGE_STAGING);
		}
		switch (GetDimension())
		{
			case 1:
			{
				D3D11_TEXTURE1D_DESC td;
				memset(&td, 0, sizeof(td));
				td.Width = desc.Width;
				td.Format = FormatToDXFormat(desc.Format);
				td.ArraySize = desc.ArraySize;
				td.BindFlags = BufferTypeToDXBindFlags(desc.BindFlags);
				td.CPUAccessFlags = desc.CpuAccess;
				td.MipLevels = desc.MipLevels;
				td.Usage = (D3D11_USAGE)desc.Usage;
				td.MiscFlags = desc.MiscFlags;
				if (desc.BindFlags & EBufferType::BFT_RAW)
				{
					td.MiscFlags |= D3D11_RESOURCE_MISC_BUFFER_ALLOW_RAW_VIEWS;
				}
				if (desc.InitData == nullptr)
				{
					auto hr = device->mDevice->CreateTexture1D(&td, nullptr, &mTexture1D);
					if (hr != S_OK)
						return false;
				}
				else
				{
					auto hr = device->mDevice->CreateTexture1D(&td, (D3D11_SUBRESOURCE_DATA*)desc.InitData, &mTexture1D);
					if (hr != S_OK)
						return false;
				}
			}
			break;
			case 2:
			{
				D3D11_TEXTURE2D_DESC td{};
				td.Width = desc.Width;
				td.Height = desc.Height;
				td.SampleDesc.Count = desc.SamplerDesc.Count;
				td.SampleDesc.Quality = desc.SamplerDesc.Quality;
				td.Format = FormatToDXFormat(desc.Format);
				if (desc.BindFlags & EBufferType::BFT_DSV)
				{
					switch (desc.Format)
					{
						case EPixelFormat::PXF_D24_UNORM_S8_UINT:
							td.Format = DXGI_FORMAT_R24G8_TYPELESS;
							break;
						case EPixelFormat::PXF_D32_FLOAT:
							td.Format = DXGI_FORMAT_R32_TYPELESS;
							break;
						case EPixelFormat::PXF_D16_UNORM:
							td.Format = DXGI_FORMAT_R16_TYPELESS;
							break;
						case EPixelFormat::PXF_UNKNOWN:
							td.Format = DXGI_FORMAT_R16_TYPELESS;
							break;
						default:
							break;
					}
				}
				td.ArraySize = desc.ArraySize;
				td.BindFlags = BufferTypeToDXBindFlags(desc.BindFlags);
				td.CPUAccessFlags = desc.CpuAccess;
				td.MipLevels = desc.MipLevels;
				td.Usage = (D3D11_USAGE)desc.Usage;
				td.MiscFlags = desc.MiscFlags;
				if (desc.BindFlags & EBufferType::BFT_RAW)
				{
					td.MiscFlags |= D3D11_RESOURCE_MISC_BUFFER_ALLOW_RAW_VIEWS;
				}
				if (desc.InitData == nullptr)
				{
					auto hr = device->mDevice->CreateTexture2D(&td, nullptr, &mTexture2D);
					if (hr != S_OK)
						return false;
				}
				else
				{
					auto hr = device->mDevice->CreateTexture2D(&td, (D3D11_SUBRESOURCE_DATA*)desc.InitData, &mTexture2D);
					if (hr != S_OK)
						return false;
				}
			}
			break;
			case 3:
			{
				D3D11_TEXTURE3D_DESC td;
				td.Width = desc.Width;
				td.Height = desc.Height;
				td.Format = FormatToDXFormat(desc.Format);
				td.BindFlags = BufferTypeToDXBindFlags(desc.BindFlags);
				td.CPUAccessFlags = desc.CpuAccess;
				td.MipLevels = desc.MipLevels;
				td.Usage = (D3D11_USAGE)desc.Usage;
				td.MiscFlags = desc.MiscFlags;
				if (desc.BindFlags & EBufferType::BFT_RAW)
				{
					td.MiscFlags |= D3D11_RESOURCE_MISC_BUFFER_ALLOW_RAW_VIEWS;
				}
				if (desc.InitData == nullptr)
				{
					auto hr = device->mDevice->CreateTexture3D(&td, nullptr, &mTexture3D);
					if (hr != S_OK)
						return false;
				}
				else
				{
					auto hr = device->mDevice->CreateTexture3D(&td, (D3D11_SUBRESOURCE_DATA*)desc.InitData, &mTexture3D);
					if (hr != S_OK)
						return false;
				}
			}
			break;
			default:
				return false;
		}
		return true;
	}
	bool DX11Texture::Map(UINT subRes, FMappedSubResource* res, bool forRead)
	{
		ASSERT(mMappingCmdList == nullptr);
		auto device = mDeviceRef.GetPtr();
		D3D11_MAP flags = D3D11_MAP_READ;
		if (forRead)
		{
			mMappingCmdList = ((DX11CmdQueue*)device->GetCmdQueue())->mHardwareContext;
			//cmd->Flush();
			flags = D3D11_MAP_READ;
		}
		else
		{
			if (Desc.Usage == USAGE_DYNAMIC)
			{//If you call Map on a deferred context, you can only pass D3D11_MAP_WRITE_DISCARD,
				mMappingCmdList = (DX11CommandList*)device->GetCmdQueue()->GetIdleCmdlist();// ((DX11CmdQueue*)device->GetCmdQueue())->mHardwareContext;
				mMappingCmdList->BeginCommand();
				flags = D3D11_MAP_WRITE_DISCARD;
			}
			else
			{
				mMappingCmdList = ((DX11CmdQueue*)device->GetCmdQueue())->mHardwareContext;
				flags = D3D11_MAP_WRITE;
			}
		}
		auto hr = mMappingCmdList->mContext->Map(mTexture1D, subRes, flags, 0, (D3D11_MAPPED_SUBRESOURCE*)res);
		ASSERT(hr == S_OK);
		return (hr == S_OK);
	}
	void DX11Texture::Unmap(UINT subRes)
	{
		ASSERT(mMappingCmdList != nullptr);
		mMappingCmdList->mContext->Unmap(mTexture1D, subRes);
		if (mMappingCmdList->IsImmContext == false)
		{
			auto device = mDeviceRef.GetPtr();
			mMappingCmdList->FlushDraws();
			mMappingCmdList->EndCommand();
			device->GetCmdQueue()->ExecuteCommandListSingle(mMappingCmdList, EQueueType::QU_Default);
			device->GetCmdQueue()->ReleaseIdleCmdlist(mMappingCmdList);
			mMappingCmdList = nullptr;
		}
		else
		{
			mMappingCmdList = nullptr;
		}
	}
	IGpuBufferData* DX11Texture::CreateBufferData(IGpuDevice* device, UINT mipIndex, ECpuAccess cpuAccess, FSubResourceFootPrint* outFootPrint)
	{
		FTextureDesc desc{};
		desc.CpuAccess = cpuAccess;
		desc.BindFlags = EBufferType::BFT_SRV;
		if (cpuAccess & ECpuAccess::CAS_READ)
		{
			desc.Usage = EGpuUsage::USAGE_STAGING;
			desc.BindFlags = EBufferType::BFT_NONE;
		}
		else if (cpuAccess & ECpuAccess::CAS_WRITE)
			desc.Usage = EGpuUsage::USAGE_DYNAMIC;
		else
			desc.Usage = EGpuUsage::USAGE_DEFAULT;		
		desc.Width = Desc.Width;
		desc.Height = Desc.Height;
		desc.Depth = Desc.Depth;
		desc.Format = Desc.Format;
		desc.MipLevels = 1;
		desc.ArraySize = 1;
		
		auto result = device->CreateTexture(&desc);

		outFootPrint->X = 0;
		outFootPrint->Y = 0;
		outFootPrint->Z = 0;
		outFootPrint->Width = Desc.Width;
		outFootPrint->Height = Desc.Height;
		if (outFootPrint->Height == 0)
			outFootPrint->Height = 1;
		outFootPrint->Depth = Desc.Depth;
		if (outFootPrint->Depth == 0)
			outFootPrint->Depth = 1;
		outFootPrint->Format = Desc.Format;
		outFootPrint->RowPitch = GetPixelByteWidth(Desc.Format) * outFootPrint->Height;

		auto alignment = device->GetGpuResourceAlignment()->TexturePitchAlignment;
		if (outFootPrint->RowPitch % alignment > 0)
		{
			outFootPrint->RowPitch = (outFootPrint->RowPitch / alignment + 1) * alignment;
		}

		return result;
	}
	void DX11Texture::UpdateGpuData(ICommandList* cmd1, UINT subRes, void* pData, const FSubResourceFootPrint* footPrint)
	{
		/*auto device = mDeviceRef.GetPtr();
		auto cmd = (DX11CommandList*)cmd1;
		D3D11_BOX box{};
		box.left = footPrint->X;
		box.top = footPrint->Y;
		box.front = footPrint->Z;
		box.right = box.left + footPrint->Width;
		box.bottom = box.top + footPrint->Height;
		box.back = box.front + footPrint->Depth;
		cmd->mContext->UpdateSubresource(mTexture1D, subRes, &box, pData, footPrint->RowPitch, footPrint->TotalSize);
		if (cmd->mCmdRecorder != nullptr)
			cmd->mCmdRecorder->mDirectDrawNum++;*/

		auto cmd = (DX11CommandList*)cmd1;
		if (Desc.Usage == USAGE_DEFAULT)
		{
			auto device = mDeviceRef.GetPtr();
			//FTransientCmd tsCmd(device, QU_Transfer);
			//auto cmd = (DX11CommandList*)tsCmd.GetCmdList();
			//auto cmd = ((DX11CmdQueue*)device->GetCmdQueue())->mHardwareContext;
			
			D3D11_BOX box{};
			box.left = footPrint->X;
			box.top = footPrint->Y;
			box.front = footPrint->Z;
			box.right = box.left + footPrint->Width;
			box.bottom = box.top + footPrint->Height;
			box.back = box.front + footPrint->Depth;
			cmd->mContext4->UpdateSubresource1(mTexture1D, subRes, &box, pData, footPrint->RowPitch, footPrint->TotalSize, D3D11_COPY_NO_OVERWRITE);
			if (cmd->mCmdRecorder != nullptr)
				cmd->mCmdRecorder->mDirectDrawNum++;
		}
		else
		{
			auto device = mDeviceRef.GetPtr();
			/*FTransientCmd tsCmd(device, QU_Transfer, "Texture.Update");
			auto cmd = (DX11CommandList*)tsCmd.GetCmdList();*/
			
			D3D11_MAP flags = D3D11_MAP_READ;
			if (Desc.Usage == USAGE_DYNAMIC)
				flags = D3D11_MAP_WRITE_DISCARD;
			else
			{
				ASSERT(false);
				flags = D3D11_MAP_WRITE;
			}

			D3D11_MAPPED_SUBRESOURCE mapData;
			auto ret = cmd->mContext->Map(mTexture1D, subRes, flags, 0, (D3D11_MAPPED_SUBRESOURCE*)&mapData);
			if (ret == S_OK)
			{
				memcpy(mapData.pData, pData, footPrint->TotalSize);
				cmd->mContext->Unmap(mTexture1D, subRes);
			}
			if (cmd->mCmdRecorder != nullptr)
				cmd->mCmdRecorder->mDirectDrawNum++;
			/*FMappedSubResource mapData;
			if (true == this->Map(0, &mapData, false))
			{
				memcpy(mapData.pData, pData, footPrint->TotalSize);
				this->Unmap(0);
			}*/
		}
	}
	void DX11Texture::UpdateGpuData(UINT subRes, void* pData, const FSubResourceFootPrint* footPrint)
	{
		if (Desc.Usage == USAGE_DEFAULT)
		{
			auto device = mDeviceRef.GetPtr();
			//FTransientCmd tsCmd(device, QU_Transfer);
			//auto cmd = (DX11CommandList*)tsCmd.GetCmdList();
			auto cmd = ((DX11CmdQueue*)device->GetCmdQueue())->mHardwareContext;

			D3D11_BOX box{};
			box.left = footPrint->X;
			box.top = footPrint->Y;
			box.front = footPrint->Z;
			box.right = box.left + footPrint->Width;
			box.bottom = box.top + footPrint->Height;
			box.back = box.front + footPrint->Depth;
			cmd->mContext4->UpdateSubresource1(mTexture1D, subRes, &box, pData, footPrint->RowPitch, footPrint->TotalSize, D3D11_COPY_NO_OVERWRITE);
			if (cmd->mCmdRecorder != nullptr)
				cmd->mCmdRecorder->mDirectDrawNum++;
		}
		else
		{
			auto device = mDeviceRef.GetPtr();

			auto cmdQueue = ((DX11CmdQueue*)device->GetCmdQueue());
			auto cmd = cmdQueue->mHardwareContext;
			VAutoVSLLock locker(cmdQueue->mImmCmdListLocker);

			D3D11_MAP flags = D3D11_MAP_READ;
			if (Desc.Usage == USAGE_DYNAMIC)
				flags = D3D11_MAP_WRITE_DISCARD;
			else
				flags = D3D11_MAP_WRITE;

			D3D11_MAPPED_SUBRESOURCE mapData;
			auto ret = cmd->mContext->Map(mTexture1D, subRes, flags, 0, (D3D11_MAPPED_SUBRESOURCE*)&mapData);
			if (ret == S_OK)
			{
				memcpy(mapData.pData, pData, footPrint->TotalSize);
				cmd->mContext->Unmap(mTexture1D, subRes);
			}
			if (cmd->mCmdRecorder != nullptr)
				cmd->mCmdRecorder->mDirectDrawNum++;
		}
	}
	void DX11Texture::SetDebugName(const char* name)
	{
		mTexture2D->SetPrivateData(WKPDID_D3DDebugObjectName, 0, NULL);
		std::string debuginfo = name;
		mTexture2D->SetPrivateData(WKPDID_D3DDebugObjectName, (UINT)debuginfo.length(), debuginfo.c_str());
	}
	bool DX11CbView::Init(DX11GpuDevice* device, IBuffer* pBuffer, const FCbvDesc& desc)
	{
		ShaderBinder = desc.ShaderBinder;
		if (pBuffer == nullptr)
		{
			FBufferDesc bfDesc{};
			bfDesc.SetDefault();
			if (ShaderBinder)
				bfDesc.Size = ShaderBinder->Size;
			else
				bfDesc.Size = desc.BufferSize;
			//bfDesc.StructureStride = ShaderBinder->;
			//bfDesc.InitData = desc->InitData;
			bfDesc.Type = EBufferType::BFT_CBuffer;
			bfDesc.Usage = EGpuUsage::USAGE_DYNAMIC;
			bfDesc.CpuAccess = ECpuAccess::CAS_WRITE;
			Buffer = MakeWeakRef(device->CreateBuffer(&bfDesc));
			ASSERT(Buffer != nullptr);
		}
		else
		{
			Buffer = pBuffer;
		}
		
		return true;
	}
	bool DX11VbView::Init(DX11GpuDevice* device, IBuffer* pBuffer, const FVbvDesc* desc)
	{
		Desc = *desc;
		Desc.InitData = nullptr;
		if (pBuffer == nullptr)
		{
			FBufferDesc bfDesc{};
			bfDesc.SetDefault();
			bfDesc.Size = desc->Size;
			bfDesc.StructureStride = desc->Stride;
			bfDesc.InitData = desc->InitData;
			bfDesc.Type = EBufferType::BFT_Vertex;
			bfDesc.Usage = desc->Usage;
			bfDesc.CpuAccess = desc->CpuAccess;
			Buffer = MakeWeakRef(device->CreateBuffer(&bfDesc));
			ASSERT(Buffer != nullptr);
		}
		else
		{
			Buffer = pBuffer;
		}
		return true;
	}

	bool DX11IbView::Init(DX11GpuDevice* device, IBuffer* pBuffer, const FIbvDesc* desc)
	{
		Desc = *desc;
		Desc.InitData = nullptr;
		if (pBuffer == nullptr)
		{
			FBufferDesc bfDesc{};
			bfDesc.SetDefault();
			bfDesc.Size = desc->Size;
			bfDesc.StructureStride = desc->Stride;
			bfDesc.InitData = desc->InitData;
			bfDesc.Type = EBufferType::BFT_Index;
			bfDesc.Usage = desc->Usage;
			bfDesc.CpuAccess = desc->CpuAccess;
			Buffer = MakeWeakRef(device->CreateBuffer(&bfDesc));
			ASSERT(Buffer != nullptr);
		}
		else
		{
			Buffer = pBuffer;
		}
		return true;
	}


	DX11SrView::DX11SrView()
	{
		mView = nullptr;
	}

	DX11SrView::~DX11SrView()
	{
		Safe_Release(mView);
	}

	void SrvDesc2DX(D3D11_SHADER_RESOURCE_VIEW_DESC* tar, const FSrvDesc* src)
	{
		memset(tar, 0, sizeof(D3D11_SHADER_RESOURCE_VIEW_DESC));
		tar->Format = FormatToDXFormat(src->Format);
		switch (src->Type)
		{
		case ST_BufferSRV:
		{
			if (src->Buffer.Flags != 0)
			{
				tar->ViewDimension = D3D11_SRV_DIMENSION::D3D_SRV_DIMENSION_BUFFEREX;
				tar->BufferEx.FirstElement = src->Buffer.FirstElement;
				tar->BufferEx.NumElements = src->Buffer.NumElements;
				tar->BufferEx.Flags = src->Buffer.Flags;
			}
			else
			{
				tar->ViewDimension = D3D11_SRV_DIMENSION::D3D10_1_SRV_DIMENSION_BUFFER;
				tar->Buffer.FirstElement = src->Buffer.FirstElement;
				tar->Buffer.NumElements = src->Buffer.NumElements;
			}
		}
		break;
		case ST_Texture1D:
		{
			tar->ViewDimension = D3D11_SRV_DIMENSION::D3D10_1_SRV_DIMENSION_TEXTURE1D;
			tar->Texture1D.MipLevels = src->Texture1D.MipLevels;
			tar->Texture1D.MostDetailedMip = src->Texture1D.MostDetailedMip;
		}
		break;
		case ST_Texture1DArray:
		{
			tar->ViewDimension = D3D11_SRV_DIMENSION::D3D_SRV_DIMENSION_TEXTURE1DARRAY;
			tar->Texture1DArray.MipLevels = src->Texture1DArray.MipLevels;
			tar->Texture1DArray.MostDetailedMip = src->Texture1DArray.MostDetailedMip;
			tar->Texture1DArray.ArraySize = src->Texture1DArray.ArraySize;
			tar->Texture1DArray.FirstArraySlice = src->Texture1DArray.FirstArraySlice;
		}
		break;
		case ST_Texture2D:
		{
			tar->ViewDimension = D3D11_SRV_DIMENSION::D3D_SRV_DIMENSION_TEXTURE2D;
			tar->Texture2D.MipLevels = src->Texture2D.MipLevels;
			tar->Texture2D.MostDetailedMip = src->Texture2D.MostDetailedMip;
		}
		break;
		case ST_Texture2DArray:
		{
			tar->ViewDimension = D3D11_SRV_DIMENSION::D3D_SRV_DIMENSION_TEXTURE2DARRAY;
			tar->Texture2DArray.ArraySize = src->Texture2DArray.ArraySize;
			tar->Texture2DArray.FirstArraySlice = src->Texture2DArray.FirstArraySlice;
			tar->Texture2DArray.MipLevels = src->Texture2DArray.MipLevels;
			tar->Texture2DArray.MostDetailedMip = src->Texture2DArray.MostDetailedMip;
		}
		break;
		case ST_Texture2DMS:
		{
			tar->ViewDimension = D3D11_SRV_DIMENSION::D3D_SRV_DIMENSION_TEXTURE2DMS;
			ASSERT(false);
		}
		break;
		case ST_Texture2DMSArray:
		{
			tar->ViewDimension = D3D11_SRV_DIMENSION::D3D_SRV_DIMENSION_TEXTURE2DMSARRAY;
			tar->Texture2DMSArray.ArraySize = src->Texture2DMSArray.ArraySize;
			tar->Texture2DMSArray.FirstArraySlice = src->Texture2DMSArray.FirstArraySlice;
			ASSERT(false);
		}
		break;
		case ST_Texture3D:
		{
			tar->ViewDimension = D3D11_SRV_DIMENSION::D3D_SRV_DIMENSION_TEXTURE3D;
			tar->Texture3D.MipLevels = src->Texture3D.MipLevels;
			tar->Texture3D.MostDetailedMip = src->Texture3D.MostDetailedMip;
			ASSERT(false);
		}
		break;
		case ST_TextureCube:
		{
			tar->ViewDimension = D3D11_SRV_DIMENSION::D3D10_1_SRV_DIMENSION_TEXTURECUBE;
			tar->TextureCube.MipLevels = src->TextureCube.MipLevels;
			tar->TextureCube.MostDetailedMip = src->TextureCube.MostDetailedMip;
		}
		break;
		case ST_TextureCubeArray:
		{
			tar->ViewDimension = D3D11_SRV_DIMENSION::D3D10_1_SRV_DIMENSION_TEXTURECUBEARRAY;
			tar->TextureCubeArray.First2DArrayFace = src->TextureCubeArray.First2DArrayFace;
			tar->TextureCubeArray.MipLevels = src->TextureCubeArray.MipLevels;
			tar->TextureCubeArray.MostDetailedMip = src->TextureCubeArray.MostDetailedMip;
			tar->TextureCubeArray.NumCubes = src->TextureCubeArray.NumCubes;
			ASSERT(false);
		}
		break;
		default:
			ASSERT(false);
			break;
		}

		switch (tar->Format)
		{
		case DXGI_FORMAT_D24_UNORM_S8_UINT:
		case DXGI_FORMAT_R24G8_TYPELESS:
			tar->Format = DXGI_FORMAT_R24_UNORM_X8_TYPELESS;
			break;
		case DXGI_FORMAT_D32_FLOAT:
		case DXGI_FORMAT_R32_TYPELESS:
			{
				if (src->Type != ESrvType::ST_BufferSRV)
				{
					tar->Format = DXGI_FORMAT_R32_FLOAT;
				}
			}
			break;
		case DXGI_FORMAT_D16_UNORM:
		case DXGI_FORMAT_R16_TYPELESS:
			tar->Format = DXGI_FORMAT_R16_UNORM;
			break;
		}
	}

	bool DX11SrView::Init(DX11GpuDevice* device, IGpuBufferData* pBuffer, const FSrvDesc& desc)
	{
		Desc = desc;
		Buffer = pBuffer;
		D3D11_SHADER_RESOURCE_VIEW_DESC		mDX11SRVDesc;
		SrvDesc2DX(&mDX11SRVDesc, &desc);

		auto hr = device->mDevice->CreateShaderResourceView((ID3D11Resource*)pBuffer->GetHWBuffer(), &mDX11SRVDesc, &mView);
		if (FAILED(hr))
			return false;
		/*if (desc.Type == ST_BufferSRV || desc.Type == ST_BufferEx)
		{
			auto hr = device->mDevice->CreateShaderResourceView(((DX11Buffer*)pBuffer)->mBuffer, &mDX11SRVDesc, &mView);
			if (FAILED(hr))
				return false;
		}
		else
		{
			auto hr = device->mDevice->CreateShaderResourceView(((DX11Texture*)pBuffer)->mTexture1D, &mDX11SRVDesc, &mView);
			if (FAILED(hr))
				return false;
		}*/

		//mView->SetPrivateData(WKPDID_D3DDebugObjectName, (UINT)strlen("ShaderResource"), "ShaderResource");
		return true;
	}
	bool DX11SrView::UpdateBuffer(IGpuDevice* device, IGpuBufferData* pBuffer)
	{
		Buffer = pBuffer;
		D3D11_SHADER_RESOURCE_VIEW_DESC		mDX11SRVDesc;
		switch (Desc.Type)
		{
			case ESrvType::ST_Texture1D:
				Desc.Texture1D.MipLevels = Buffer.UnsafeConvertTo<ITexture>()->Desc.MipLevels;
				break;
			case ESrvType::ST_Texture1DArray:
				Desc.Texture1DArray.MipLevels = Buffer.UnsafeConvertTo<ITexture>()->Desc.MipLevels;
				break;
			case ESrvType::ST_Texture2D:
				Desc.Texture2D.MipLevels = Buffer.UnsafeConvertTo<ITexture>()->Desc.MipLevels;
				break;
			case ESrvType::ST_Texture2DArray:
				Desc.Texture2DArray.MipLevels = Buffer.UnsafeConvertTo<ITexture>()->Desc.MipLevels;
				break;
			case ESrvType::ST_Texture3D:
				Desc.Texture3D.MipLevels = Buffer.UnsafeConvertTo<ITexture>()->Desc.MipLevels;
				break;
			case ESrvType::ST_TextureCube:
				Desc.TextureCube.MipLevels = Buffer.UnsafeConvertTo<ITexture>()->Desc.MipLevels;
				break;
			case ESrvType::ST_TextureCubeArray:
				Desc.TextureCubeArray.MipLevels = Buffer.UnsafeConvertTo<ITexture>()->Desc.MipLevels;
				break;
			default:
				break;
		}
		
		SrvDesc2DX(&mDX11SRVDesc, &Desc);

		Safe_Release(mView);
		auto hr = ((DX11GpuDevice*)device)->mDevice->CreateShaderResourceView((ID3D11Resource*)pBuffer->GetHWBuffer(), &mDX11SRVDesc, &mView);
		if (FAILED(hr))
			return false;
		return true;
	}

	DX11UaView::DX11UaView()
	{
		mView = nullptr;
	}

	DX11UaView::~DX11UaView()
	{
		Safe_Release(mView);
	}
	static void UavDesc2DX(IGpuResource* pBuffer, D3D11_UNORDERED_ACCESS_VIEW_DESC* tar, const FUavDesc* src)
	{
		//memset(tar, 0, sizeof(D3D12_SHADER_RESOURCE_VIEW_DESC));
		tar->Format = FormatToDXFormat(src->Format);
		switch (src->ViewDimension)
		{
			case EDimensionUAV::UAV_DIMENSION_BUFFER:
			{
				tar->ViewDimension = D3D11_UAV_DIMENSION::D3D11_UAV_DIMENSION_BUFFER;
				tar->Buffer.FirstElement = src->Buffer.FirstElement;
				tar->Buffer.NumElements = src->Buffer.NumElements;
				tar->Buffer.Flags = src->Buffer.Flags;
			}
			break;
			case EDimensionUAV::UAV_DIMENSION_TEXTURE1D:
			{
				tar->ViewDimension = D3D11_UAV_DIMENSION::D3D11_UAV_DIMENSION_TEXTURE1D;
				tar->Texture1D.MipSlice = src->Texture1D.MipSlice;
			}
			break;
			case EDimensionUAV::UAV_DIMENSION_TEXTURE1DARRAY:
			{
				tar->ViewDimension = D3D11_UAV_DIMENSION::D3D11_UAV_DIMENSION_TEXTURE1DARRAY;
				tar->Texture1DArray.ArraySize = src->Texture1DArray.ArraySize;
				tar->Texture1DArray.FirstArraySlice = src->Texture1DArray.FirstArraySlice;
				tar->Texture1DArray.MipSlice = src->Texture1DArray.MipSlice;
			}
			break;
			case EDimensionUAV::UAV_DIMENSION_TEXTURE2D:
			{
				tar->ViewDimension = D3D11_UAV_DIMENSION::D3D11_UAV_DIMENSION_TEXTURE2D;
				tar->Texture2D.MipSlice = src->Texture2D.MipSlice;
			}
			break;
			case EDimensionUAV::UAV_DIMENSION_TEXTURE2DARRAY:
			{
				tar->ViewDimension = D3D11_UAV_DIMENSION::D3D11_UAV_DIMENSION_TEXTURE2DARRAY;
				tar->Texture2DArray.ArraySize = src->Texture2DArray.ArraySize;
				tar->Texture2DArray.FirstArraySlice = src->Texture2DArray.FirstArraySlice;
				tar->Texture2DArray.MipSlice = src->Texture2DArray.MipSlice;
			}
			break;
			case EDimensionUAV::UAV_DIMENSION_TEXTURE3D:
			{
				tar->ViewDimension = D3D11_UAV_DIMENSION::D3D11_UAV_DIMENSION_TEXTURE3D;
				tar->Texture3D.MipSlice = src->Texture3D.MipSlice;
				tar->Texture3D.WSize = src->Texture3D.WSize;
				tar->Texture3D.FirstWSlice = src->Texture3D.FirstWSlice;
			}
			break;
			default:
				ASSERT(false);
				break;
		}
	}
	bool DX11UaView::Init(DX11GpuDevice* device, IGpuBufferData* pBuffer, const FUavDesc& desc)
	{
		Desc = desc;
		Buffer = pBuffer;

		D3D11_UNORDERED_ACCESS_VIEW_DESC tmp{};
		UavDesc2DX(pBuffer, &tmp, &desc);
		if (desc.ViewDimension == EDimensionUAV::UAV_DIMENSION_BUFFER)
		{
			tmp.Format = DXGI_FORMAT::DXGI_FORMAT_UNKNOWN;
			if (tmp.Buffer.Flags & D3D11_BUFFER_UAV_FLAG_RAW)
			{
				tmp.Format = DXGI_FORMAT::DXGI_FORMAT_R32_TYPELESS;
			}
		}
		
		//tmp.Format = FormatToDXFormat(desc.Format);
		//if (desc.ViewDimension == UAV_DIMENSION_BUFFER)
		//{
		//	if (desc.Buffer.Flags & D3D11_BUFFER_UAV_FLAG_RAW)
		//	{
		//		if ((pBuffer->Desc.Type & EBufferType::BFT_RAW) == 0)
		//		{
		//			//ASSERT(false);
		//		}
		//	}
		//}

		auto hr = device->mDevice->CreateUnorderedAccessView((ID3D11Resource*)pBuffer->GetHWBuffer(), &tmp, &mView);
		if (FAILED(hr))
			return false;

		//mView->SetPrivateData(WKPDID_D3DDebugObjectName, (UINT)strlen("ShaderResource"), "ShaderResource");
		return true;
	}
	
	DX11RenderTargetView::DX11RenderTargetView()
	{
		mView = nullptr;
	}

	DX11RenderTargetView::~DX11RenderTargetView()
	{
		Safe_Release(mView);
	}

	void RtvDesc2DX(D3D11_RENDER_TARGET_VIEW_DESC* tar, const FRtvDesc* src)
	{
		memset(tar, 0, sizeof(D3D11_RENDER_TARGET_VIEW_DESC));
		tar->Format = FormatToDXFormat(src->Format);
		switch (src->Type)
		{
			case ERtvType::RTV_Buffer:
			{
				tar->Buffer = *(D3D11_BUFFER_RTV*)&src->Buffer;
				tar->ViewDimension = D3D11_RTV_DIMENSION_BUFFER;
			}
			break;
			case ERtvType::RTV_Texture1D:
			{
				tar->ViewDimension = D3D11_RTV_DIMENSION_TEXTURE1D;
				tar->Texture1D.MipSlice = src->Texture1D.MipSlice;
			}
			break;
			case ERtvType::RTV_Texture2D:
			{
				tar->ViewDimension = D3D11_RTV_DIMENSION_TEXTURE2D;
				tar->Texture2D.MipSlice = src->Texture2D.MipSlice;
			}
			break;
			case ERtvType::RTV_Texture3D:
			{
				tar->ViewDimension = D3D11_RTV_DIMENSION_TEXTURE3D;
				memcpy(&tar->Texture3D, &src->Texture3D, sizeof(src->Texture3D));
			}
			break;
			case ERtvType::RTV_Texture1DArray:
			{
				tar->ViewDimension = D3D11_RTV_DIMENSION_TEXTURE1DARRAY;
				tar->Texture1DArray.MipSlice = src->Texture1DArray.MipSlice;
				tar->Texture1DArray.ArraySize = src->Texture1DArray.ArraySize;
				tar->Texture1DArray.FirstArraySlice = src->Texture1DArray.FirstArraySlice;
			}
			break;
			case ERtvType::RTV_Texture2DArray:
			{
				tar->ViewDimension = D3D11_RTV_DIMENSION_TEXTURE2DARRAY;
				tar->Texture2DArray.MipSlice = src->Texture2DArray.MipSlice;
				tar->Texture2DArray.ArraySize = src->Texture2DArray.ArraySize;
				tar->Texture2DArray.FirstArraySlice = src->Texture2DArray.FirstArraySlice;
			}
			break;
			case ERtvType::RTV_Texture2DMS:
			{
				tar->ViewDimension = D3D11_RTV_DIMENSION_TEXTURE2DMS;
			}
			break;
			case ERtvType::RTV_Texture2DMSArray:
			{
				tar->ViewDimension = D3D11_RTV_DIMENSION_TEXTURE2DMSARRAY;
			}
			break;
		}

		switch (tar->Format)
		{
		case DXGI_FORMAT_R24G8_TYPELESS:
			tar->Format = DXGI_FORMAT_R24_UNORM_X8_TYPELESS;
			break;
		case DXGI_FORMAT_R32_TYPELESS:
			tar->Format = DXGI_FORMAT_R32_FLOAT;
			break;
		case DXGI_FORMAT_R16_TYPELESS:
			tar->Format = DXGI_FORMAT_R16_UNORM;
			break;
		}
	}
	bool DX11RenderTargetView::Init(DX11GpuDevice* device, ITexture* pBuffer, const FRtvDesc& desc)
	{
		Desc = desc;
		GpuResource = pBuffer;

		D3D11_RENDER_TARGET_VIEW_DESC	mDesc;
		RtvDesc2DX(&mDesc, &desc);

		if (pBuffer != nullptr)
		{
			auto hr = device->mDevice->CreateRenderTargetView((ID3D11Resource*)pBuffer->GetHWBuffer(), &mDesc, &mView);
			if (FAILED(hr))
			{
				return false;
			}

#ifdef _DEBUG
			mView->SetPrivateData(WKPDID_D3DDebugObjectName, 0, NULL);
			static UINT UniqueId = 0;
			auto debuginfo = VStringA_FormatV("RTV_%u", UniqueId++);
			mView->SetPrivateData(WKPDID_D3DDebugObjectName, (UINT)debuginfo.size(), debuginfo.c_str());
#endif
			return true;
		}

		return false;
	}

	DX11DepthStencilView::DX11DepthStencilView()
	{
		mView = nullptr;
	}

	DX11DepthStencilView::~DX11DepthStencilView()
	{
		Safe_Release(mView);
	}

	bool DX11DepthStencilView::Init(DX11GpuDevice* device, ITexture* pBuffer, const FDsvDesc& desc)
	{
		Desc = desc;
		auto dxPixelFormat = FormatToDXFormat(desc.Format);
		GpuResource = pBuffer;
		
		D3D11_DEPTH_STENCIL_VIEW_DESC	DSVDesc;
		memset(&DSVDesc, 0, sizeof(DSVDesc));
		switch (dxPixelFormat)
		{
		case DXGI_FORMAT_D24_UNORM_S8_UINT:
		case DXGI_FORMAT_D32_FLOAT:
		case DXGI_FORMAT_D16_UNORM:
			DSVDesc.Format = dxPixelFormat;
			break;
		default:
			DSVDesc.Format = DXGI_FORMAT_D32_FLOAT;
			break;
		}
		DSVDesc.ViewDimension = D3D11_DSV_DIMENSION_TEXTURE2D;
		
		auto hr = device->mDevice->CreateDepthStencilView((ID3D11Resource*)pBuffer->GetHWBuffer(), &DSVDesc, &mView);
		if (FAILED(hr))
		{
			return false;
		}

#ifdef _DEBUG
		mView->SetPrivateData(WKPDID_D3DDebugObjectName, 0, NULL);
		static UINT UniqueId = 0;
		auto debuginfo = VStringA_FormatV("DSView_%u", UniqueId++);
		mView->SetPrivateData(WKPDID_D3DDebugObjectName, (UINT)debuginfo.length(), debuginfo.c_str());
#endif

		return true;
	}
}
NS_END