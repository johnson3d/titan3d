#include "ID11ShaderResourceView.h"
#include "ID11Texture2D.h"
#include "ID11RenderContext.h"
#include "ID11CommandList.h"
#include "ID11GpuBuffer.h"
#include "../../../3rd/native/Image.Shared/XImageDecoder.h"
#include "../../../3rd/native/Image.Shared/XImageBuffer.h"
//#include "../../Graphics/GfxEngine.h"
#include "../../Base/io/vfxfile.h"
#include "../../Base/xnd/vfxxnd.h"
#include "../../Base/r2m/F2MManager.h"
#include "../Utility/GfxTextureStreaming.h"

#define new VNEW

NS_BEGIN

ID11ShaderResourceView::ID11ShaderResourceView()
{
	m_pDX11SRV = nullptr;
}

ID11ShaderResourceView::~ID11ShaderResourceView()
{
}

void SrvDesc2DX(D3D11_SHADER_RESOURCE_VIEW_DESC* tar, const IShaderResourceViewDesc* src)
{
	memset(tar, 0, sizeof(D3D11_SHADER_RESOURCE_VIEW_DESC));
	tar->Format = FormatToDXFormat(src->Format);
	tar->ViewDimension = *(D3D11_SRV_DIMENSION*)&src->ViewDimension;
	switch (src->Type)
	{
		case ST_BufferSRV:
		{
			tar->Buffer = *(D3D11_BUFFER_SRV*)&src->Buffer;
		}
		break;
		case ST_Texture1D:
		{
			tar->Texture1D.MipLevels = src->Texture1D.MipLevels;
			tar->Texture1D.MostDetailedMip = src->Texture1D.MostDetailedMip;
		}
		break;
		case ST_Texture1DArray:
		{
			tar->Texture1DArray.MipLevels = src->Texture1DArray.MipLevels;
			tar->Texture1DArray.MostDetailedMip = src->Texture1DArray.MostDetailedMip;
			tar->Texture1DArray.ArraySize = src->Texture1DArray.ArraySize;
			tar->Texture1DArray.FirstArraySlice = src->Texture1DArray.FirstArraySlice;
		}
		break;
		case ST_Texture2D:
		{
			tar->Texture2D.MipLevels = src->Texture2D.MipLevels;
			tar->Texture2D.MostDetailedMip = src->Texture2D.MostDetailedMip;
		}
		break;
		case ST_Texture2DArray:
		{
			tar->Texture2DArray.MipLevels = src->Texture2DArray.MipLevels;
			tar->Texture2DArray.MostDetailedMip = src->Texture2DArray.MostDetailedMip;
		}
		break;
		case ST_Texture2DMS:
		{

		}
		break;
		case ST_Texture2DMSArray:
		{

		}
		break;
		case ST_Texture3D:
		{

		}
		break;
		case ST_Texture3DArray:
		{

		}
		break;
		case ST_TextureCube:
		{

		}
		break;
		case ST_TextureCubeArray:
		{

		}
		break;
		case ST_BufferEx:
		{

		}
		break;
		default:
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
			tar->Format = DXGI_FORMAT_R32_FLOAT;
			break;
		case DXGI_FORMAT_D16_UNORM:
		case DXGI_FORMAT_R16_TYPELESS:
			tar->Format = DXGI_FORMAT_R16_UNORM;
			break;
	}
}

bool ID11ShaderResourceView::UpdateBuffer(IRenderContext* rc, const IGpuBuffer* buffer)
{
	mBuffer.StrongRef((IGpuBuffer*)buffer);

	if (buffer == nullptr)
	{
		m_pDX11SRV = nullptr;
		return true;
	}

	if (mSrvDesc.Type == ST_Texture2D)
	{
		ID3D11ShaderResourceView* pSrv = nullptr;
		auto hr = ((ID11RenderContext*)rc)->mDevice->CreateShaderResourceView((ID3D11Resource*)buffer->GetHWBuffer(), nullptr, &pSrv);
		if (FAILED(hr))
			return false;
		m_pDX11SRV = pSrv;
	}
	else
	{
		D3D11_SHADER_RESOURCE_VIEW_DESC		mDX11SRVDesc;
		SrvDesc2DX(&mDX11SRVDesc, &mSrvDesc);

		ID3D11ShaderResourceView* pSrv = nullptr;
		auto hr = ((ID11RenderContext*)rc)->mDevice->CreateShaderResourceView((ID3D11Resource*)buffer->GetHWBuffer(), nullptr, &pSrv);
		if (FAILED(hr))
			return false;
		m_pDX11SRV = pSrv;
	}
	//m_pDX11SRV->SetPrivateData(WKPDID_D3DDebugObjectName, (UINT)strlen("ShaderResource"), "ShaderResource");
	return true;
}

bool ID11ShaderResourceView::Init(ID11RenderContext* rc, const IShaderResourceViewDesc* pDesc)
{
	mSrvDesc = *pDesc;
	mBuffer.StrongRef(pDesc->mGpuBuffer);

	D3D11_SHADER_RESOURCE_VIEW_DESC		mDX11SRVDesc;
	SrvDesc2DX(&mDX11SRVDesc, &mSrvDesc);

	ID3D11ShaderResourceView* pSrv = nullptr;
	auto hr = rc->mDevice->CreateShaderResourceView((ID3D11Resource*)pDesc->mGpuBuffer->GetHWBuffer(), &mDX11SRVDesc, &pSrv);
	if (FAILED(hr))
		return false;

	m_pDX11SRV = pSrv;
	//m_pDX11SRV.StrongRef(pSrv);
	//m_pDX11SRV->SetPrivateData(WKPDID_D3DDebugObjectName, (UINT)strlen("ShaderResource"), "ShaderResource");
	return true;
}

//bool ID11ShaderResourceView::SetDepthStencilTexture(ID11Texture2D* texture)
//{
//	m_refTexture2D.StrongRef(texture);
//	return true;
//}

void ID11ShaderResourceView::RefreshResource()
{
	if (m_pDX11SRV!=nullptr)
	{
		m_pDX11SRV = nullptr;

		RestoreResource();
	}
}

void ID11ShaderResourceView::InvalidateResource()
{
	m_pDX11SRV = nullptr;
	GetResourceState()->SetResourceSize(0);
}

vBOOL ID11ShaderResourceView::RestoreResource()
{
	if (m_pDX11SRV != nullptr)
		return TRUE;
	auto rc = mRenderContext.GetPtr();
	if (rc == nullptr)
		return FALSE;

	return TRUE;
}

vBOOL ID11ShaderResourceView::Save2Memory(IRenderContext* rc, IBlobObject* data, int Type)
{
	if (mSrvDesc.Type != ST_Texture2D)
	{
		return FALSE;
	}
	ID3D11Texture2D* d11Texture = nullptr;
	//auto d11Texture = ((ID11Texture2D*)texture)->mTexture2D;
	m_pDX11SRV->GetResource((ID3D11Resource**)&d11Texture);
	D3D11_TEXTURE2D_DESC desc;
	d11Texture->GetDesc(&desc);
	desc.BindFlags = 0;
	desc.Usage = D3D11_USAGE_STAGING;
	desc.CPUAccessFlags = D3D11_CPU_ACCESS_WRITE | D3D11_CPU_ACCESS_READ;

	auto pDevice = ((ID11RenderContext*)rc)->mDevice;
	auto pContext = ((ID11RenderContext*)rc)->mHardwareContext;
	ID3D11Texture2D* memTexture;
	auto hr = pDevice->CreateTexture2D(&desc, nullptr, &memTexture);
	if (FAILED(hr))
	{
		d11Texture->Release();
		return FALSE;
	}

	pContext->CopyResource(memTexture, d11Texture);
	d11Texture->Release();

	D3D11_MAPPED_SUBRESOURCE mapped;
	if (pContext->Map(memTexture, 0, D3D11_MAP_READ, 0, &mapped) == S_OK)
	{
		XImageBuffer image;
		image.Create(desc.Width, desc.Height, 32);
		BYTE* row = (BYTE*)mapped.pData;
		BYTE* dst = image.m_pImage;
		for (UINT i = 0; i < desc.Height; i++)
		{
			for (UINT j = 0; j < desc.Width; j++)
			{
				//DWORD color = ((DWORD*)mapped.pData)[i*desc.Width + j];
				//if (color != 0)
				//{
				//	dst[j] = 0;
				//}
				if (desc.Format == DXGI_FORMAT_R8G8B8A8_UNORM)
				{
					dst[j * 4] = row[j * 4];
					dst[j * 4 + 1] = row[j * 4 + 1];
					dst[j * 4 + 2] = row[j * 4 + 2];
					dst[j * 4 + 3] = row[j * 4 + 3];
				}
				else
				{
					dst[j] = 0;
				}
			}
			row += mapped.RowPitch;
			dst += image.m_nStride;
		}
		pContext->Unmap(memTexture, 0);

		struct PixelDesc
		{
			int Width;
			int Height;
			int Stride;
			EPixelFormat Format;
		} saveDesc;
		saveDesc.Width = image.m_nWidth;
		saveDesc.Height = image.m_nHeight;
		saveDesc.Stride = image.m_nStride;
		saveDesc.Format = PXF_R8G8B8A8_UNORM;

		data->PushData((BYTE*)&saveDesc, sizeof(PixelDesc));
		data->PushData(image.m_pImage, saveDesc.Height * saveDesc.Stride);
		
		return TRUE;
	}
	else
	{
		return FALSE;
	}
	/*auto d11Ctx = ((ID11RenderContext*)rc)->mImmContext->mDeferredContext;
	ID3D11Texture2D* pTexture; 
	m_pDX11SRV->GetResource((ID3D11Resource **)&pTexture);
	D3D11_TEXTURE2D_DESC descDepth;
	pTexture->GetDesc(&descDepth);
	ID3D10Blob* d3dBlob;
	auto hr = D3DX11SaveTextureToMemory(d11Ctx, pTexture, (D3DX11_IMAGE_FILE_FORMAT)Type, &d3dBlob, 0);
	data->mDatas.resize(d3dBlob->GetBufferSize());
	memcpy(&data->mDatas[0], d3dBlob->GetBufferPointer(), d3dBlob->GetBufferSize());
	d3dBlob->Release();
	return hr == S_OK;*/
}

inline int GetDXFormatStride(DXGI_FORMAT fmt)
{
	switch (fmt)
	{
	case DXGI_FORMAT_R16_FLOAT:
		return 2;
	case DXGI_FORMAT_R16_UINT:
		return 2;
	case DXGI_FORMAT_R16_SINT:
		return 2;
	case DXGI_FORMAT_R16_UNORM:
		return 2;
	case DXGI_FORMAT_R16_SNORM:
		return 2;
	case DXGI_FORMAT_R32_UINT:
		return 4;
	case DXGI_FORMAT_R32_SINT:
		return 4;
	case DXGI_FORMAT_R32_FLOAT:
		return 4;
		/*case PXF_R8G8B8:
		case PXF_R8G8B8_UNORM:
		case PXF_R8G8B8_SNORM:*/
	case DXGI_FORMAT_R8G8B8A8_UNORM:
		assert(false);
		return 4;
	case DXGI_FORMAT_R8G8B8A8_SINT:
		return 4;
	case DXGI_FORMAT_R8G8B8A8_UINT:
		return 4;
	case DXGI_FORMAT_R8G8B8A8_SNORM:
		return 4;
	case DXGI_FORMAT_R16G16_UINT:
		return 4;
	case DXGI_FORMAT_R16G16_SINT:
		return 4;
	case DXGI_FORMAT_R16G16_FLOAT:
		return 4;
	case DXGI_FORMAT_R16G16_UNORM:
		return 4;
	case DXGI_FORMAT_R16G16_SNORM:
		return 4;
	case DXGI_FORMAT_R16G16B16A16_UINT:
		return 8;
	case DXGI_FORMAT_R16G16B16A16_SINT:
		return 8;
	case DXGI_FORMAT_R16G16B16A16_FLOAT:
		return 8;
	case DXGI_FORMAT_R16G16B16A16_UNORM:
		return 8;
	case DXGI_FORMAT_R16G16B16A16_SNORM:
		return 8;
	case DXGI_FORMAT_R32G32B32A32_UINT:
		return 16;
	case DXGI_FORMAT_R32G32B32A32_SINT:
		return 16;
	case DXGI_FORMAT_R32G32B32A32_FLOAT:
		return 16;
	case DXGI_FORMAT_R32G32B32_UINT:
		return 12;
	case DXGI_FORMAT_R32G32B32_SINT:
		return 12;
	case DXGI_FORMAT_R32G32B32_FLOAT:
		return 12;
	case DXGI_FORMAT_R32G32_UINT:
		return 8;
	case DXGI_FORMAT_R32G32_SINT:
		return 8;
	case DXGI_FORMAT_R32G32_FLOAT:
		return 8;
	case DXGI_FORMAT_D24_UNORM_S8_UINT:
		return 4;
	case DXGI_FORMAT_D32_FLOAT:
		return 4;
	case DXGI_FORMAT_D32_FLOAT_S8X24_UINT:
		return 8;
	case DXGI_FORMAT_D16_UNORM:
		return 2;
	default:
		break;
	}
	return 4;
}

static unsigned int GetPerPixelSize_Byte(DXGI_FORMAT Format)
{
	unsigned int BytesPerPixel = 4;

	switch (Format)
	{
	case DXGI_FORMAT_R16_TYPELESS:
		BytesPerPixel = 2;
		break;
	case DXGI_FORMAT_R8G8B8A8_TYPELESS:
	case DXGI_FORMAT_R8G8B8A8_UNORM:
	case DXGI_FORMAT_R8G8B8A8_UNORM_SRGB:
	case DXGI_FORMAT_R8G8B8A8_UINT:
	case DXGI_FORMAT_B8G8R8A8_TYPELESS:
	case DXGI_FORMAT_B8G8R8A8_UNORM:
	case DXGI_FORMAT_B8G8R8A8_UNORM_SRGB:
	case DXGI_FORMAT_R24G8_TYPELESS:
	case DXGI_FORMAT_R10G10B10A2_UNORM:
	case DXGI_FORMAT_R11G11B10_FLOAT:
	case DXGI_FORMAT_R16G16_UNORM:
	case DXGI_FORMAT_R32_UINT:
		BytesPerPixel = 4;
		break;
	case DXGI_FORMAT_R16G16B16A16_FLOAT:
	case DXGI_FORMAT_R16G16B16A16_UNORM:
		BytesPerPixel = 8;
		break;
	case DXGI_FORMAT_R32G32B32A32_FLOAT:
		BytesPerPixel = 16;
		break;
	case DXGI_FORMAT_R32G32_FLOAT:
		BytesPerPixel = 8;
		break;
	case DXGI_FORMAT_R8_TYPELESS:
	case DXGI_FORMAT_R8_UNORM:
	case DXGI_FORMAT_R8_UINT:
	case DXGI_FORMAT_R8_SNORM:
	case DXGI_FORMAT_R8_SINT:
		BytesPerPixel = 1;
		break;
	}

	return BytesPerPixel;
}


vBOOL ID11ShaderResourceView::GetTexture2DData(IRenderContext* rc, IBlobObject* pDestCpuTex, int level,int RectWidth, int RectHeight)
{
	unsigned int CpuTexWidth = 1;
	unsigned int CpuTexHeight = 1;

	if (RectWidth < 1)
	{
		CpuTexWidth = 1;
	}
	else
	{
		CpuTexWidth = RectWidth;
	}

	if (RectHeight < 1)
	{
		CpuTexHeight = 1;
	}
	else
	{
		CpuTexHeight = RectHeight;
	}

	auto d11rc = (ID11RenderContext*)rc;

	ID3D11Texture2D* pTexture = nullptr;
	m_pDX11SRV->GetResource((ID3D11Resource**)&pTexture);
	if (pTexture == nullptr)
		return FALSE;

	D3D11_TEXTURE2D_DESC srcDesc;
	pTexture->GetDesc(&srcDesc);

	D3D11_TEXTURE2D_DESC dstDesc;
	ZeroMemory(&dstDesc, sizeof(dstDesc));
	dstDesc = srcDesc;
	dstDesc.BindFlags = 0;
	dstDesc.Usage = D3D11_USAGE_STAGING;
	dstDesc.CPUAccessFlags = D3D11_CPU_ACCESS_WRITE | D3D11_CPU_ACCESS_READ;
	
	ID3D11Texture2D*			memTexture;

	auto hr = d11rc->mDevice->CreateTexture2D(&dstDesc, nullptr, &memTexture);
	if (FAILED(hr))
	{
		pTexture->Release();
		return FALSE;
	}

	d11rc->mImmCmdList->mDeferredContext->CopyResource(memTexture, pTexture);
	pTexture->Release();

	unsigned int BytesPerPixel = GetPerPixelSize_Byte(srcDesc.Format);
	
	/*if (0 != d11rc->mImmContext->mLocker.TryLock())
	{
		return FALSE;
	}*/
	D3D11_MAPPED_SUBRESOURCE GpuTex;
	if (S_OK == d11rc->mImmCmdList->mDeferredContext->Map(memTexture, level, D3D11_MAP_READ, 0, &GpuTex))
	{
		/*int buffSize = mappedRes.RowPitch * srcDesc.Height;
		data->mDatas.resize(buffSize);
		memcpy(data->GetData(), mappedRes.pData, buffSize);

		d11rc->mImmContext->mDeferredContext->Unmap(memTexture, level);*/

		unsigned int BytesPerRow = BytesPerPixel * CpuTexWidth;

		unsigned char* refGpuTex = (unsigned char*)GpuTex.pData;

		int BufferSize = CpuTexWidth * CpuTexHeight * BytesPerPixel;
		if (pDestCpuTex->GetSize() != BufferSize)
		{
			pDestCpuTex->ReSize(BufferSize);
		}
		
		unsigned char* refDestCpuTex = (unsigned char*)pDestCpuTex->GetData();

		for (unsigned int y = 0; y < CpuTexHeight; y++)
		{
			memcpy(refDestCpuTex, refGpuTex, BytesPerRow);
			refDestCpuTex += BytesPerRow;
			refGpuTex += GpuTex.RowPitch;
		}
		
		d11rc->mImmCmdList->mDeferredContext->Unmap(memTexture, level);
		memTexture->Release();
		//d11rc->mImmContext->mLocker.Unlock();
		return TRUE;
	}
	else
	{
		memTexture->Release();
		//d11rc->mImmContext->mLocker.Unlock();
		return FALSE;
	}
}

NS_END
