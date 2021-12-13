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
	
}

bool IGLShaderResourceView::UpdateBuffer(IRenderContext* rc, const IGpuBuffer* buffer)
{
	mBuffer.StrongRef((IGpuBuffer*)buffer);
	return true;
}

bool IGLShaderResourceView::Init(IGLRenderContext* rc, const IShaderResourceViewDesc* desc)
{
	mSrvDesc = *desc;
	mBuffer.StrongRef((IGpuBuffer*)desc->mGpuBuffer);
	return true;
}

void IGLShaderResourceView::InvalidateResource()
{
	auto gpuBuffer = mBuffer.UnsafeConvertTo<IGLGpuBuffer>();
	if (gpuBuffer != nullptr)
	{
		gpuBuffer->mBufferId.reset();
	}
	GetResourceState()->SetResourceSize(0);
}

vBOOL IGLShaderResourceView::RestoreResource()
{
	ASSERT(false);
	auto gpuBuffer = mBuffer.UnsafeConvertTo<IGLGpuBuffer>();
	if (gpuBuffer == nullptr)
	{
		return FALSE;
	}

	auto rc = mRenderContext.GetPtr();
	if (rc == nullptr)
		return FALSE;

	if (mResourceFile.length() == 0)
		return FALSE;

	return TRUE;
}

vBOOL IGLShaderResourceView::Save2Memory(IRenderContext* rc, IBlobObject* data, int Type)
{
	if (mSrvDesc.Type != ST_Texture2D)
		return FALSE;

	GLSdk* sdk = GLSdk::ImmSDK;

	auto pGLTexture = mBuffer.UnsafeConvertTo<IGLTexture2D>();
	sdk->BindTexture(GL_TEXTURE_2D, pGLTexture->mGlesTexture2D);
	INT rowBytesSize = GetRowBytesFromWidthAndFormat(pGLTexture->mTextureDesc.Width, pGLTexture->mTextureDesc.Format);
	int buffSize = rowBytesSize * pGLTexture->mTextureDesc.Height;
	data->ReSize(buffSize);
	void* pBuffer = nullptr;
	sdk->MapBuffer(&pBuffer, GL_TEXTURE_BUFFER, GL_MAP_READ_BIT);
	//sdk->MapBufferRange(&pBuffer, GL_TEXTURE_BUFFER, 0, buffSize, GL_MAP_READ_BIT);	
	if (pBuffer != nullptr)
	{
		XImageBuffer image;
		image.Create(pGLTexture->mTextureDesc.Width, pGLTexture->mTextureDesc.Height, 32);
		BYTE* row = (BYTE*)pBuffer;
		BYTE* dst = image.m_pImage;
		for (UINT i = 0; i < pGLTexture->mTextureDesc.Height; i++)
		{
			for (UINT j = 0; j < pGLTexture->mTextureDesc.Width; j++)
			{
				//DWORD color = ((DWORD*)mapped.pData)[i*desc.Width + j];
				//if (color != 0)
				//{
				//	dst[j] = 0;
				//}
				if (pGLTexture->mTextureDesc.Format == PXF_R8G8B8A8_UNORM)
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
	ASSERT(false);
	return FALSE;
	//if (mTexture2D == nullptr)
	//{
	//	return FALSE;
	//}

	//unsigned int CpuTexWidth = 1;
	//unsigned int CpuTexHeight = 1;

	//if (RectWidth < 1)
	//{
	//	CpuTexWidth = 1;
	//}
	//else
	//{
	//	CpuTexWidth = RectWidth;
	//}

	//if (RectHeight < 1)
	//{
	//	CpuTexHeight = 1;
	//}
	//else
	//{
	//	CpuTexHeight = RectHeight;
	//}

	//GLSdk* sdk = GLSdk::ImmSDK;
	//auto pGLTexture = mTexture2D.UnsafeConvertTo<IGLTexture2D>();
	//sdk->BindTexture(GL_TEXTURE_2D, pGLTexture->mGlesTexture2D);
	//INT rowBytesSize = GetRowBytesFromWidthAndFormat(RectWidth, mTexture2D->mDesc.Format);
	//int buffSize = rowBytesSize * RectHeight;
	//data->ReSize(buffSize);
	//void* pBuffer = nullptr;
	//sdk->MapBuffer(&pBuffer, GL_TEXTURE_BUFFER, GL_READ_ONLY);
	////sdk->MapBufferRange(&pBuffer, GL_TEXTURE_BUFFER, 0, buffSize, GL_MAP_READ_BIT);	
	//if (pBuffer != nullptr)
	//{
	//	UINT destBytesSize = (UINT)GetRowBytesFromWidthAndFormat(CpuTexWidth, mTexture2D->mDesc.Format);
	//	unsigned char* refGpuTex = (unsigned char*)pBuffer;
	//	unsigned char* refDestCpuTex = (unsigned char*)data->GetData();
	//	for (unsigned int y = 0; y < CpuTexHeight; y++)
	//	{
	//		memcpy(refDestCpuTex, refGpuTex, rowBytesSize);
	//		refDestCpuTex += destBytesSize;
	//		refGpuTex += rowBytesSize;
	//	}
	//	GLboolean ret;
	//	sdk->UnmapBuffer(&ret, GL_TEXTURE_BUFFER);
	//	sdk->BindTexture(GL_TEXTURE_BUFFER, 0);
	//	return TRUE;
	//}
	//sdk->BindTexture(GL_TEXTURE_2D, 0);
	//return FALSE;
}

std::shared_ptr<GLSdk::GLBufferId> IGLShaderResourceView::GetGLBufferId()
{
	if (mBuffer != nullptr)
	{
		return *(std::shared_ptr<GLSdk::GLBufferId>*)mBuffer->GetHWBuffer();
	}
	else
	{
		return nullptr;
	}
}

NS_END