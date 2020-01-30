#include "MTLShaderResourceView.h"
#include "../../Core/xnd/vfxxnd.h"
#include "../../3rd/Image.Shared/XImageDecoder.h"
#include "../../3rd/Image.Shared/XImageBuffer.h"
#include "../../Core/io/vfxfile.h"
#include "../../Core/r2m/file_2_memory.h"
#include "../../Core/debug/vfxdebug.h"

#include "MTLTexture2D.h"

extern "C"
{
	VFX_API EngineNS::XNDNode* XNDNode_New();
}

NS_BEGIN

MtlShaderResView::MtlShaderResView()
{
	m_refMtlCtx = nullptr;
	m_pTex2dFromFile = nullptr;
}

MtlShaderResView::~MtlShaderResView()
{
	if (m_pTex2dFromFile != nullptr)
	{
		m_pTex2dFromFile->Release();
		m_pTex2dFromFile = nullptr;
	}
}

bool MtlShaderResView::Init(MtlContext* pCtx, const IShaderResourceViewDesc* pDesc)
{
	if (pDesc->m_pTexture2D == nullptr)
		return false;

	m_refTexture2D.StrongRef(pDesc->m_pTexture2D);

	return true;
}

UINT32 GetRowBytes(EPixelFormat format, UINT32 width)
{
	return GetPixelByteWidth(format) * width;
}

bool MtlShaderResView::CreateSRVFromFile(MtlContext* pCtx, const char* FilePathName, bool ImmediateMode)
{
	mTextureFilePathName = FilePathName;
	m_refMtlCtx = pCtx;

	if (ImmediateMode)
	{
		int SuffixLength = strlen(".txpic");
		if (mTextureFilePathName.length() > SuffixLength && mTextureFilePathName.substr(mTextureFilePathName.length() - SuffixLength) == ".txpic")
		{
			VFile2Memory io;
			if (FALSE == io.Create(mTextureFilePathName.c_str()))
			{
				VFX_LTRACE(ELTT_Resource, "texture %s file is not found\r\n", FilePathName);
				return false;
			}

			BYTE* pImageBuffer = nullptr;
			size_t BufferSize = 0;

			AutoRef<XNDNode> node = XNDNode_New();
			node->Load(&io);

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

			XImageDecoder* pDecoder = nullptr;
			{
				attr = node->GetAttrib("PNG");
				if (attr != nullptr)
				{
					attr->BeginRead(__FILE__, __LINE__);
					BufferSize = attr->GetLength();
					pImageBuffer = new BYTE[BufferSize];
					attr->Read(pImageBuffer, BufferSize);
					attr->EndRead();
					pDecoder = XImageDecoder::MatchDecoder("a.png");
				}
			}
			node->TryReleaseHolder();
			io.Close();

			auto pImage = new XImageBuffer();
			auto bLoad = pDecoder->LoadImageX(*pImage, pImageBuffer, BufferSize);
			if (bLoad == false)
			{
				Safe_DeleteArray(pImageBuffer);
				return false;
			}
			pImage->ConvertToRGBA8();
			Safe_DeleteArray(pImageBuffer);
			pImage->FlipPixel4();

			int widthMipCount;
			int heightMipCount;
			int mipCount;
			bool isPowerOfTwo;
			pImage->CalculateMipCount(&widthMipCount, &heightMipCount, &mipCount, &isPowerOfTwo);

			EPixelFormat ImageFormat;
			switch (pImage->m_nBitCount)
			{
			case 32:
				ImageFormat = PXF_R8G8B8A8_UNORM;
				break;
			default:
				ASSERT(false);
				break;
			}

			m_pTex2dFromFile = new MtlTexture2D();
			ITexture2DDesc Tex2dDesc;
			Tex2dDesc.Width = pImage->m_nWidth;
			Tex2dDesc.Height = pImage->m_nHeight;
			Tex2dDesc.MipLevels = mipCount + 1;
			Tex2dDesc.Format = ImageFormat;
			Tex2dDesc.BindFlags = BF_SHADER_RES;
			if (m_pTex2dFromFile->Init(pCtx, &Tex2dDesc) == false)
			{
				AssertRHI(false);
			}

			UINT32 width = pImage->m_nWidth;
			UINT32 height = pImage->m_nHeight;
			XImageBuffer* pMipImage = pImage;
			std::vector<XImageBuffer*>	MipImagePtrArray;
			for (UINT32 idx = 0; idx < Tex2dDesc.MipLevels; idx++)
			{
				MipImagePtrArray.push_back(pMipImage);

				MTLRegion region = MTLRegionMake2D(0, 0, width, height);
				UINT32 BytesPerRow = GetRowBytes(ImageFormat, width);
				[m_pTex2dFromFile->m_pMtlTexture2D replaceRegion : region mipmapLevel : idx withBytes : pMipImage->GetImage() bytesPerRow : BytesPerRow];

				if (idx == Tex2dDesc.MipLevels - 1)
				{
					break;
				}

				width /= 2;
				if (width == 0)
				{
					width = 1;
				}
				height /= 2;
				if (height == 0)
				{
					height = 1;
				}
				pMipImage = pMipImage->BoxDownSampler(width, height, 0, 0);
			}

			m_refTexture2D.StrongRef(m_pTex2dFromFile);

			for (UINT32 idx = 0; idx < MipImagePtrArray.size(); idx++)
			{
				delete MipImagePtrArray[idx];
			}
			MipImagePtrArray.clear();
			pImage = nullptr;

			GetResourceState()->SetStreamState(SS_Valid);
		}
		else
		{
			return false;
		}
	}
	else
	{
		GetResourceState()->SetStreamState(SS_Invalid);
	}
	
	return true;
}

void MtlShaderResView::InvalidateResource()
{
	//

	GetResourceState()->SetResourceSize(0);
}

vBOOL MtlShaderResView::RestoreResource()
{
	if (m_refTexture2D != nullptr && ((MtlTexture2D*)(ITexture2D*)m_refTexture2D)->m_pMtlTexture2D != nil)
	{
		return TRUE;
	}

	if (m_refMtlCtx == nullptr || mTextureFilePathName.length() == 0)
	{
		return FALSE;
	}

	vBOOL ret_value = CreateSRVFromFile(m_refMtlCtx, mTextureFilePathName.c_str(), true) == true ? TRUE : FALSE;
	return ret_value;
}


NS_END