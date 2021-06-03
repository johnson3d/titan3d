#include "IGLShaderResourceView.h"
#include "IGLRenderContext.h"
#include "IGLTexture2D.h"
#include "IGLUnorderedAccessView.h"
#include "../../../3rd/native/Image.Shared/XImageDecoder.h"
#include "../../../3rd/native/Image.Shared/XImageBuffer.h"
#include "../Utility/GfxTextureStreaming.h"
#include "../../Base/io/vfxfile.h"
#include "../../Base/xnd/vfxxnd.h"
#include "../../Base/r2m/F2MManager.h"

#define new VNEW

NS_BEGIN

int GetRowBytesFromWidthAndFormat(int width, EPixelFormat inFormat)
{
	return GetPixelByteWidth(inFormat) * width;
}

GLint GetStride(INT rowBytesSize)
{
	if (rowBytesSize % 8 == 0)
		return 8;
	else if (rowBytesSize % 4 == 0)
		return 4;
	else if (rowBytesSize % 2 == 0)
		return 2;
	else
		return 1;
}

IGLShaderResourceView::IGLShaderResourceView()
{
	//mGlesTexture2D = 0;
	mImage = nullptr;
}

IGLShaderResourceView::~IGLShaderResourceView()
{
	Safe_Delete(mImage);
	
	Cleanup();
}

void IGLShaderResourceView::Cleanup()
{
	mTexture2D = nullptr;
}

bool IGLShaderResourceView::UpdateTexture2D(IRenderContext* rc, const ITexture2D* pTexture2D)
{
	mTexture2D.StrongRef((ITexture2D*)pTexture2D);
	return true;
}

bool IGLShaderResourceView::Init(IGLRenderContext* rc, const IGLGpuBuffer* pBuffer, const ISRVDesc* desc)
{
	mSrvDesc = *desc;
	mBuffer.StrongRef((IGLGpuBuffer*)pBuffer);
	return true;
}

bool IGLShaderResourceView::Init(IGLRenderContext* rc, const IShaderResourceViewDesc* pDesc)
{
	if (pDesc->m_pTexture2D == nullptr)
		return false;

	mTexture2D.StrongRef(pDesc->m_pTexture2D);

	return true;
}

void IGLShaderResourceView::InvalidateResource()
{
	auto pGLTexture = mTexture2D.UnsafeConvertTo<IGLTexture2D>();
	if (pGLTexture != nullptr && pGLTexture->mGlesTexture2D != nullptr)
	{
		pGLTexture->mGlesTexture2D.reset();
	}
	GetResourceState()->SetResourceSize(0);
}

vBOOL IGLShaderResourceView::RestoreResource()
{
	auto pGLTexture = mTexture2D.UnsafeConvertTo<IGLTexture2D>();
	if (pGLTexture != nullptr && pGLTexture->mGlesTexture2D != 0)
		return TRUE;

	auto rc = mRenderContext.GetPtr();
	if (rc == nullptr)
		return FALSE;

	if (mResourceFile.length() == 0)
		return FALSE;

	return TRUE;
}

vBOOL IGLShaderResourceView::Save2Memory(IRenderContext* rc, IBlobObject* data, int Type)
{
	GLSdk* sdk = GLSdk::ImmSDK;

	auto pGLTexture = mTexture2D.UnsafeConvertTo<IGLTexture2D>();
	sdk->BindTexture(GL_TEXTURE_2D, pGLTexture->mGlesTexture2D);
	INT rowBytesSize = GetRowBytesFromWidthAndFormat(mTexture2D->mDesc.Width, mTexture2D->mDesc.Format);
	int buffSize = rowBytesSize * mTexture2D->mDesc.Height;
	data->ReSize(buffSize);
	void* pBuffer = nullptr;
	sdk->MapBuffer(&pBuffer, GL_TEXTURE_BUFFER, GL_MAP_READ_BIT);
	//sdk->MapBufferRange(&pBuffer, GL_TEXTURE_BUFFER, 0, buffSize, GL_MAP_READ_BIT);	
	if (pBuffer != nullptr)
	{
		XImageBuffer image;
		image.Create(mTexture2D->mDesc.Width, mTexture2D->mDesc.Height, 32);
		BYTE* row = (BYTE*)pBuffer;
		BYTE* dst = image.m_pImage;
		for (UINT i = 0; i < mTexture2D->mDesc.Height; i++)
		{
			for (UINT j = 0; j < mTexture2D->mDesc.Width; j++)
			{
				//DWORD color = ((DWORD*)mapped.pData)[i*desc.Width + j];
				//if (color != 0)
				//{
				//	dst[j] = 0;
				//}
				if (mTexture2D->mDesc.Format == PXF_R8G8B8A8_UNORM)
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
			row += rowBytesSize;
			dst += image.m_nStride;
		}

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

		GLboolean ret;
		sdk->UnmapBuffer(&ret, GL_TEXTURE_BUFFER);
		sdk->BindBuffer(GL_TEXTURE_BUFFER, 0);
	}
	return FALSE;
}

vBOOL IGLShaderResourceView::GetTexture2DData(IRenderContext* rc, IBlobObject* data, int level, int RectWidth, int RectHeight)
{
	if (mTexture2D == nullptr)
	{
		return FALSE;
	}

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

	GLSdk* sdk = GLSdk::ImmSDK;
	auto pGLTexture = mTexture2D.UnsafeConvertTo<IGLTexture2D>();
	sdk->BindTexture(GL_TEXTURE_2D, pGLTexture->mGlesTexture2D);
	INT rowBytesSize = GetRowBytesFromWidthAndFormat(RectWidth, mTexture2D->mDesc.Format);
	int buffSize = rowBytesSize * RectHeight;
	data->ReSize(buffSize);
	void* pBuffer = nullptr;
	sdk->MapBuffer(&pBuffer, GL_TEXTURE_BUFFER, GL_READ_ONLY);
	//sdk->MapBufferRange(&pBuffer, GL_TEXTURE_BUFFER, 0, buffSize, GL_MAP_READ_BIT);	
	if (pBuffer != nullptr)
	{
		UINT destBytesSize = (UINT)GetRowBytesFromWidthAndFormat(CpuTexWidth, mTexture2D->mDesc.Format);
		unsigned char* refGpuTex = (unsigned char*)pBuffer;
		unsigned char* refDestCpuTex = (unsigned char*)data->GetData();
		for (unsigned int y = 0; y < CpuTexHeight; y++)
		{
			memcpy(refDestCpuTex, refGpuTex, rowBytesSize);
			refDestCpuTex += destBytesSize;
			refGpuTex += rowBytesSize;
		}
		GLboolean ret;
		sdk->UnmapBuffer(&ret, GL_TEXTURE_BUFFER);
		sdk->BindTexture(GL_TEXTURE_BUFFER, 0);
		return TRUE;
	}
	sdk->BindTexture(GL_TEXTURE_2D, 0);
	return FALSE;
}

std::shared_ptr<GLSdk::GLBufferId> IGLShaderResourceView::GetGLBufferId()
{
	if (mTexture2D != nullptr)
	{
		auto pGLTexture = mTexture2D.UnsafeConvertTo<IGLTexture2D>();
		return pGLTexture->mGlesTexture2D;
	}
	else if (mBuffer != nullptr)
	{
		IGLGpuBuffer* pGLBuffer = mBuffer.UnsafeConvertTo<IGLGpuBuffer>();
		return pGLBuffer->mBufferId;
	}
	else
	{
		return nullptr;
	}
}

NS_END