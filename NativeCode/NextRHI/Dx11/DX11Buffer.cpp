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
		Desc = desc;
		Desc.InitData = nullptr;
		
		D3D11_BUFFER_DESC BufferDesc;
		memset(&BufferDesc, 0, sizeof(BufferDesc));
		BufferDesc.Usage = (D3D11_USAGE)desc.Usage;
		BufferDesc.ByteWidth = desc.Size;
		BufferDesc.CPUAccessFlags = (UINT)desc.CpuAccess;// D3D11_CPU_ACCESS_WRITE;
		BufferDesc.MiscFlags = desc.MiscFlags;
		BufferDesc.StructureByteStride = desc.StructureStride;
		if (desc.Type & BFT_CBuffer)
		{
			BufferDesc.BindFlags |= D3D11_BIND_CONSTANT_BUFFER;
			//BufferDesc.CPUAccessFlags = D3D11_CPU_ACCESS_WRITE;
		}
		if (desc.Type & EBufferType::BFT_UAV)
		{
			BufferDesc.BindFlags |= D3D11_BIND_UNORDERED_ACCESS;
			BufferDesc.CPUAccessFlags = 0;
			BufferDesc.Usage = D3D11_USAGE_DEFAULT;
			BufferDesc.MiscFlags |= D3D11_RESOURCE_MISC_BUFFER_STRUCTURED;
		}
		if (desc.Type & EBufferType::BFT_SRV)
		{
			BufferDesc.BindFlags |= D3D11_BIND_SHADER_RESOURCE;
			BufferDesc.MiscFlags |= D3D11_RESOURCE_MISC_BUFFER_STRUCTURED;
		}
		if (desc.Type & EBufferType::BFT_Vertex)
		{
			BufferDesc.BindFlags |= D3D11_BIND_VERTEX_BUFFER;
		}
		if (desc.Type & EBufferType::BFT_Index)
		{
			BufferDesc.BindFlags |= D3D11_BIND_INDEX_BUFFER;
		}
		if (desc.Type & EBufferType::BFT_IndirectArgs)
		{
			BufferDesc.MiscFlags |= D3D11_RESOURCE_MISC_DRAWINDIRECT_ARGS;
		}
		if (desc.Type & EBufferType::BFT_RAW)
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

	void DX11Buffer::Flush2Device(ICommandList* cmd, void* pBuffer, UINT Size)
	{
		if (Size > Desc.Size || pBuffer == NULL)
			return;
		ASSERT(Size == Desc.Size);

		auto refCmdList = (DX11CommandList*)cmd;
		auto pContext = refCmdList->mContext;
		if (Size <= Desc.Size && (Desc.CpuAccess & ECpuAccess::CAS_WRITE))
		{
			D3D11_MAPPED_SUBRESOURCE MappedSubresource;
			if (pContext->Map(mBuffer, 0, D3D11_MAP_WRITE_DISCARD, 0, &MappedSubresource) == S_OK)
			{
				memcpy(MappedSubresource.pData, pBuffer, Size);
				pContext->Unmap(mBuffer, 0);
			}
		}
		else
		{
			pContext->UpdateSubresource(mBuffer, 0, nullptr, pBuffer, Size, 0);
		}
	}

	void DX11Buffer::UpdateGpuData(ICommandList* cmd, UINT subRes, void* pData, const FSubresourceBox* box, UINT rowPitch, UINT depthPitch)
	{
		auto refCmdList = (DX11CommandList*)cmd;
		//refCmdList->mContext->UpdateSubresource(mBuffer, mipIndex, (D3D11_BOX*)box, pData, rowPitch, depthPitch);
		auto pContext = refCmdList->mContext;
		//UINT subRes = D3D11CalcSubresource(mipIndex, arrayIndex, 1);
		if (rowPitch < Desc.Size && (Desc.CpuAccess & ECpuAccess::CAS_WRITE))
		{
			D3D11_MAPPED_SUBRESOURCE mapData;
			if (S_OK == pContext->Map(mBuffer, 0, D3D11_MAP_WRITE_DISCARD, 0, &mapData))
			{
				memcpy(mapData.pData, pData, rowPitch);
				pContext->Unmap(mBuffer, 0);
			}
		}
		else
		{
			pContext->UpdateSubresource(mBuffer, subRes, nullptr, pData, rowPitch, depthPitch);
		}
	}

	void DX11Buffer::UpdateGpuData(ICommandList* cmd, UINT offset, void* pData, UINT size)
	{
		if (offset == 0 && Desc.Usage == USAGE_DEFAULT)// && size == Desc.Size)
		{
			auto refCmdList = (DX11CommandList*)cmd;
			refCmdList->mContext->UpdateSubresource(mBuffer, 0, nullptr, pData, size, size);
			return;
		}
		FMappedSubResource subRes;
		memset(&subRes, 0, sizeof(subRes));
		if (Map(cmd, 0, &subRes, false))
		{
			memcpy((BYTE*)subRes.pData + offset, pData, size);
			Unmap(cmd, 0);
		}
	}

	bool DX11Buffer::Map(ICommandList* cmd, UINT index, FMappedSubResource* res, bool forRead)
	{
		D3D11_MAP flags = D3D11_MAP_READ;
		if (forRead)
		{
			ASSERT(cmd == ((DX11CmdQueue*)cmd->GetGpuDevice()->GetCmdQueue())->mHardwareContext);
			cmd->Flush();
			flags = D3D11_MAP_READ;
		}
		else
			flags = D3D11_MAP_WRITE_DISCARD;

		auto refCmdList = (DX11CommandList*)cmd;
		auto ret = (refCmdList->mContext->Map(mBuffer, index, flags, 0, (D3D11_MAPPED_SUBRESOURCE*)res) == S_OK);
		ASSERT(ret == true);
		return ret;
	}

	void DX11Buffer::Unmap(ICommandList* cmd, UINT index)
	{
		auto refCmdList = (DX11CommandList*)cmd;
		refCmdList->mContext->Unmap(mBuffer, index);
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

	bool DX11Texture::Init(DX11GpuDevice* device, const FTextureDesc& desc)
	{
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
	bool DX11Texture::Map(ICommandList* cmd, UINT subRes, FMappedSubResource* res, bool forRead)
	{
		D3D11_MAP flags = D3D11_MAP_READ;
		if (forRead)
		{
			ASSERT(cmd == ((DX11CmdQueue*)cmd->GetGpuDevice()->GetCmdQueue())->mHardwareContext);
			cmd->Flush();
			flags = D3D11_MAP_READ;
		}
		else
			flags = D3D11_MAP_WRITE_DISCARD;
		//UINT subRes = D3D11CalcSubresource(mipIndex, arrayIndex, Desc.MipLevels);
		auto refCmdList = (DX11CommandList*)cmd;
		auto hr = refCmdList->mContext->Map(mTexture1D, subRes, flags, 0, (D3D11_MAPPED_SUBRESOURCE*)res);
		ASSERT(hr == S_OK);
		return (hr == S_OK);
	}
	void DX11Texture::Unmap(ICommandList* cmd, UINT subRes)
	{
		//UINT subRes = D3D11CalcSubresource(mipIndex, arrayIndex, Desc.MipLevels);
		auto refCmdList = (DX11CommandList*)cmd;
		refCmdList->mContext->Unmap(mTexture1D, subRes);
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
	void DX11Texture::UpdateGpuData(ICommandList* cmd, UINT subRes, void* pData, const FSubresourceBox* box, UINT rowPitch, UINT depthPitch)
	{
		//UINT subRes = D3D11CalcSubresource(mipIndex, arrayIndex, Desc.MipLevels);
		auto refCmdList = (DX11CommandList*)cmd;
		refCmdList->mContext->UpdateSubresource(mTexture1D, subRes, (D3D11_BOX*)box, pData, rowPitch, depthPitch);
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
			bfDesc.Size = ShaderBinder->Size;
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
			ASSERT(false);
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
		if (Desc.Type == ESrvType::ST_Texture1D ||
			Desc.Type == ESrvType::ST_Texture2D ||
			Desc.Type == ESrvType::ST_Texture3D)
		{
			Desc.Texture2D.MipLevels = Buffer.UnsafeConvertTo<ITexture>()->Desc.MipLevels;
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
		}
		if (tmp.Buffer.Flags & D3D11_BUFFER_UAV_FLAG_RAW)
		{
			tmp.Format = DXGI_FORMAT::DXGI_FORMAT_R32_TYPELESS;
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