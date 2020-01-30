#include "ID11ShaderResourceView.h"
#include "ID11Texture2D.h"
#include "ID11RenderContext.h"
#include "ID11CommandList.h"
#include "ID11UnorderedAccessView.h"
#include "../../3rd/Image.Shared/XImageDecoder.h"
#include "../../3rd/Image.Shared/XImageBuffer.h"
#include "../../Graphics/GfxEngine.h"
#include "../../Core/io/vfxfile.h"
#include "../../Core/r2m/VPakFile.h"
#include "../GfxTextureStreaming.h"

#define new VNEW

extern "C"
{
	VFX_API EngineNS::XNDNode* XNDNode_New();
}

NS_BEGIN

class D11TextureStreaming : public GfxTextureStreaming
{
public:
	virtual void OnMipLoaded(IRenderContext* rc, IShaderResourceView* srv1, int mip) override
	{
		AutoRef<ID11ShaderResourceView> srv;
		srv.StrongRef((ID11ShaderResourceView*)srv1);

		D3D11_TEXTURE2D_DESC desc;
		memset(&desc, 0, sizeof(desc));
		desc.Width = GetSubSource(mip)->Width;
		desc.Height = GetSubSource(mip)->Height;
		desc.MipLevels = mip + 1;
		desc.ArraySize = 1;
		desc.Format = FormatToDXFormat(mFormat);
		desc.SampleDesc.Count = 1;
		desc.SampleDesc.Quality = 0;
		desc.Usage = D3D11_USAGE_DEFAULT;
		desc.BindFlags = D3D11_BIND_SHADER_RESOURCE;
		desc.CPUAccessFlags = 0;
		desc.MiscFlags = 0;
		std::vector<D3D11_SUBRESOURCE_DATA> resData;
		resData.resize(desc.MipLevels* desc.ArraySize);

		UINT resourceSize = 0;
		for (UINT i = 0; i < (UINT)desc.MipLevels; ++i)
		{
			resData[desc.MipLevels - i - 1].pSysMem = GetMipMemData(i);
			resData[desc.MipLevels - i - 1].SysMemPitch = GetMipMemPitch(i);
			resData[desc.MipLevels - i - 1].SysMemSlicePitch = GetMipSlicePitch(i);

			resourceSize += (UINT)GetSubSource(i)->MemData.size();
		}

		ID3D11Texture2D* d11Tex = nullptr;
		auto d11Device = ((ID11RenderContext*)rc)->mDevice;
		auto hr = d11Device->CreateTexture2D(&desc, &resData[0], &d11Tex);
		if (hr == S_OK)
		{
			D3D11_SHADER_RESOURCE_VIEW_DESC	 srvDesc;
			memset(&srvDesc, 0, sizeof(srvDesc));
			srvDesc.Format = desc.Format;
			srvDesc.ViewDimension = D3D11_SRV_DIMENSION_TEXTURE2D;
			srvDesc.Texture2D.MipLevels = desc.MipLevels;

			ID3D11ShaderResourceView* pSrv;
			hr = d11Device->CreateShaderResourceView(d11Tex, &srvDesc, &pSrv);
			if (FAILED(hr))
			{
				VFX_LTRACE(ELTT_Graphics, "RHI", "CreateShaderResourceView failed(%d):%s", hr, srv->mResourceFile.c_str());
			}
#ifdef _DEBUG
			pSrv->SetPrivateData(WKPDID_D3DDebugObjectName, (UINT)srv->mResourceFile.length(), srv->mResourceFile.c_str());
#endif
			AutoRef<ID3D11ShaderResourceView> pD11Srv = pSrv;
			AutoRef<ID11Texture2D> tex2d = new ID11Texture2D();
			tex2d->InitD11Texture2D(d11Tex);
			tex2d->mDesc.Width = desc.Width;
			tex2d->mDesc.Height = desc.Height;
			tex2d->mDesc.MipLevels = desc.MipLevels;
			tex2d->mDesc.Format = mFormat;
			tex2d->mDesc.BindFlags = desc.BindFlags;
			tex2d->mDesc.CPUAccess = desc.CPUAccessFlags;
			
			d11Tex->Release();

			srv->m_pDX11SRV = pD11Srv;
			srv->mDX11SRVDesc = srvDesc;
			srv->mTexture2D.StrongRef(tex2d);

			srv->GetResourceState()->SetResourceSize(resourceSize);
		}
	}
};

ID11ShaderResourceView::ID11ShaderResourceView()
{
	m_pDX11SRV = nullptr;
	mTexStreaming = new D11TextureStreaming();
}

ID11ShaderResourceView::~ID11ShaderResourceView()
{
}

bool ID11ShaderResourceView::Init(ID11RenderContext* rc, const IShaderResourceViewDesc* pDesc)
{
	mTexture2D.StrongRef(pDesc->m_pTexture2D);

	auto d11Texture = (ID11Texture2D*)pDesc->m_pTexture2D;
	D3D11_TEXTURE2D_DESC Tex2dDesc;
	d11Texture->m_pDX11Texture2D->GetDesc(&Tex2dDesc);
	memset(&mDX11SRVDesc, 0, sizeof(mDX11SRVDesc));
	mDX11SRVDesc.Format = Tex2dDesc.Format;
	mDX11SRVDesc.ViewDimension = D3D11_SRV_DIMENSION_TEXTURE2D;
	mDX11SRVDesc.Texture2D.MipLevels = Tex2dDesc.MipLevels;
	mDX11SRVDesc.Texture2D.MostDetailedMip = 0;
	
	switch (mDX11SRVDesc.Format)
	{
	case DXGI_FORMAT_R24G8_TYPELESS:
		mDX11SRVDesc.Format = DXGI_FORMAT_R24_UNORM_X8_TYPELESS;
		break;
	case DXGI_FORMAT_R32_TYPELESS:
		mDX11SRVDesc.Format = DXGI_FORMAT_R32_FLOAT;
		break;
	case DXGI_FORMAT_R16_TYPELESS:
		mDX11SRVDesc.Format = DXGI_FORMAT_R16_UNORM;
		break;
	}

	ID3D11ShaderResourceView* pSrv = nullptr;
	auto hr = rc->mDevice->CreateShaderResourceView(d11Texture->m_pDX11Texture2D, &mDX11SRVDesc, &pSrv);
	if (FAILED(hr))
		return false;

	m_pDX11SRV = pSrv;
	//m_pDX11SRV->SetPrivateData(WKPDID_D3DDebugObjectName, (UINT)strlen("ShaderResource"), "ShaderResource");
	return true;
}

bool ID11ShaderResourceView::Init(ID11RenderContext* rc, ID11GpuBuffer* pBuffer, const ISRVDesc* desc)
{
	mSrvDesc = *desc;
	D3D11_SHADER_RESOURCE_VIEW_DESC temp = *(D3D11_SHADER_RESOURCE_VIEW_DESC*)desc;
	temp.Format = FormatToDXFormat(desc->Format);
	/*D3D11_BUFFER_DESC descBuf;
	ZeroMemory(&descBuf, sizeof(descBuf));
	pBuffer->mBuffer->GetDesc(&descBuf);*/
	ID3D11ShaderResourceView* pSrv = nullptr;
	auto hr = rc->mDevice->CreateShaderResourceView(pBuffer->mBuffer, &temp, &pSrv);
	if (FAILED(hr))
		return false;
	m_pDX11SRV = pSrv;
	return true;
}

//bool ID11ShaderResourceView::SetDepthStencilTexture(ID11Texture2D* texture)
//{
//	m_refTexture2D.StrongRef(texture);
//	return true;
//}

bool ID11ShaderResourceView::InitD11View(ID11RenderContext* rc, const char* file, bool isLoad)
{
	mResourceFile = file;
	mRenderContext.FromObject(rc);

	if (isLoad)
	{
		static auto extLen = strlen(".txpic");
		if (mResourceFile.length() > extLen && mResourceFile.substr(mResourceFile.length() - extLen) == ".txpic")
		{
			/*auto newName = mResourceFile.substr(0, mResourceFile.length() - extLen);
			newName += ".txpic";*/
			AutoRef<VRes2Memory> io(F2MManager::Instance.GetF2M(file));
			if (io == nullptr)
			{
				VFX_LTRACE(ELTT_Resource, "texture %s file is not found\r\n", file);
				return false;
			}
			BYTE* pBuffer = nullptr;
			
			AutoRef<XNDNode> node = XNDNode_New();
			node->Load(io);
						
			std::string oriFile;
			auto attr = node->GetAttrib("Desc");
			if (attr != nullptr)
			{
				attr->BeginRead(__FILE__, __LINE__);
				if (attr->GetVersion() == 1)
				{
					attr->ReadText(oriFile);
					attr->Read(mTxDesc);
				}
				else if (attr->GetVersion() == 2)
				{
					attr->Read(mTxDesc.sRGB);
				}
				else if (attr->GetVersion() == 3)
				{
					attr->Read(mTxDesc);
				}
				attr->EndRead();
			}

			if (mTexStreaming->Init(rc, this, &mTxDesc, node))
			{
				node->TryReleaseHolder();
				return true;
			}
			return false;
		}
		else
		{
			return false;
		}
		
		if (m_pDX11SRV == nullptr)
		{
			GetResourceState()->SetStreamState(SS_Invalid);
			return false;
		}
		GetResourceState()->SetStreamState(SS_Valid);
#ifdef _DEBUG
		m_pDX11SRV->SetPrivateData(WKPDID_D3DDebugObjectName, (UINT)mResourceFile.length(), mResourceFile.c_str());
#endif
	}
	else
	{
		GetResourceState()->SetStreamState(SS_Invalid);
	}

	return true;
}

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
	mTexStreaming->SetStreamMipLevel(0);
	GetResourceState()->SetResourceSize(0);
}

vBOOL ID11ShaderResourceView::RestoreResource()
{
	if (m_pDX11SRV != nullptr)
		return TRUE;
	auto rc = mRenderContext.GetPtr();
	if (rc == nullptr)
		return FALSE;

	if (mResourceFile.length()==0)
	{
		return FALSE;
	}
	return InitD11View(rc, mResourceFile.c_str(), true)?1:0;
}

vBOOL ID11ShaderResourceView::Save2Memory(IRenderContext* rc, IBlobObject* data, int Type)
{
	ITexture2D* texture = mTexture2D;
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

using namespace EngineNS;

extern "C"
{
	/*VFX_API vBOOL SDK_ImageEncoder_ConvertImage(ID11RenderContext* rc, BYTE* src, UINT size, D3DX11_IMAGE_FILE_FORMAT format, IBlobObject* tar)
	{
		D3DX11_IMAGE_LOAD_INFO info;
		ID3D11Resource* texture;
		auto hr = D3DX11CreateTextureFromMemory(rc->mDevice, src, size, &info, nullptr, &texture, nullptr);
		if (hr != S_OK)
			return FALSE;
		ID3D10Blob* blob;
		hr = D3DX11SaveTextureToMemory(rc->mImmContext->mDeferredContext, texture, format, &blob, 0);
		texture->Release();
		if (hr != S_OK)
			return FALSE;
		tar->mDatas.resize(blob->GetBufferSize());
		memcpy(&tar->mDatas[0], blob->GetBufferPointer(), blob->GetBufferSize());
		blob->Release();
		
		return true;
	}*/
}