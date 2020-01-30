#include "IGLShaderResourceView.h"
#include "IGLRenderContext.h"
#include "IGLTexture2D.h"
#include "IGLUnorderedAccessView.h"
#include "../../3rd/Image.Shared/XImageDecoder.h"
#include "../../3rd/Image.Shared/XImageBuffer.h"
#include "../GfxTextureStreaming.h"
#include "../../Core/io/vfxfile.h"
#include "../../Core/r2m/VPakFile.h"
#include "../../Graphics/GfxEngine.h"

#define new VNEW

extern "C"
{
	VFX_API EngineNS::XNDNode* XNDNode_New();
}

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

class GLTextureStreaming : public GfxTextureStreaming
{
public:
	virtual void OnMipLoaded(IRenderContext* rc, IShaderResourceView* srv1, int mip) override
	{
		//auto sdk = GLSdk::ImmSDK;
		std::shared_ptr<GLSdk> sdk(new GLSdk(TRUE));
		OnMipLoaded_Impl(sdk, srv1, mip);

		GLSdk::ImmSDK->PushCommandDirect([=]()->void
		{
			sdk->Execute();
		}, "GLTextureStreaming::OnMipLoaded");
	}
	void OnMipLoaded_Impl(std::shared_ptr<GLSdk>& sdk, IShaderResourceView* srv1, int mip)
	{
		AutoRef<IGLShaderResourceView> srv;
		srv.StrongRef((IGLShaderResourceView*)srv1);
		
		UINT resourceSize = 0;
		std::shared_ptr<GLSdk::GLBufferId> textureId;
		if (mCompressMode == PCM_None)
		{
			GLint internalFormat;
			GLint format;
			GLenum type;
			FormatToGL(mFormat, internalFormat, format, type);

			textureId = sdk->GenTextures();
			sdk->BindTexture(GL_TEXTURE_2D, textureId);
			sdk->PixelStorei(GL_UNPACK_ALIGNMENT, 1);
			sdk->TexParameteri(GL_TEXTURE_2D, GL_TEXTURE_BASE_LEVEL, 0);
			sdk->TexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MAX_LEVEL, mip);

			for (int i = 0; i < mip + 1; ++i)
			{
				auto pRes = GetSubSource(mip - i);
				if (pRes == nullptr)
				{
					continue;
				}
				int width = GetSubSource(mip - i)->Width;
				int height = GetSubSource(mip - i)->Height;

				INT rowBytesSize = GetRowBytesFromWidthAndFormat(width, mFormat);
				GLint stride = GetStride(rowBytesSize);
				sdk->PixelStorei(GL_UNPACK_ALIGNMENT, stride);

				sdk->TexImage2D(GL_TEXTURE_2D, i, internalFormat, width, height, 0, format, type, GetMipMemData(mip - i));

				resourceSize += width * height * GetPixelByteWidth(mFormat);
			}

			sdk->BindTexture(GL_TEXTURE_2D, 0);
		}
		else if (mCompressMode == PCM_ETC2)
		{
			GLint internalFormat;
			GLint format;
			GLenum type;
			auto NumOfStride = FormatToGL_ETC((ETCFormat)srv->mTxDesc.EtcFormat, internalFormat, format, type);

			textureId = sdk->GenTextures();
#if _DEBUG
			textureId->DebugInfo.FromObject(srv1);
#endif
			sdk->BindTexture(GL_TEXTURE_2D, textureId);
			sdk->PixelStorei(GL_UNPACK_ALIGNMENT, 4);

			sdk->TexParameteri(GL_TEXTURE_2D, GL_TEXTURE_BASE_LEVEL, 0);

			auto mipCount = mip + 1;
			//Ī������MaxLevelҪ��ȥ��2�㣬����һ�£�������ΪETC��С�ߴ���4x4��
			//���Ǽ���mip����x2��x1���������ȻETCѹ����ʱ�����ǿ�ƴ����x4�ˣ�
			//����GL����������û�д������������Ȼ��������Ƿ���0�ˣ��Ҿ����ˣ�
			//sdk->TexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MAX_LEVEL, mipCount - 3);
			sdk->TexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MAX_LEVEL, mipCount - 1);
			sdk->TexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MAG_FILTER, GL_LINEAR);
			sdk->TexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MIN_FILTER, GL_LINEAR_MIPMAP_LINEAR);
			sdk->TexParameteri(GL_TEXTURE_2D, GL_TEXTURE_WRAP_S, GL_CLAMP_TO_EDGE);
			sdk->TexParameteri(GL_TEXTURE_2D, GL_TEXTURE_WRAP_T, GL_CLAMP_TO_EDGE);
			for (int i = 0; i < mipCount; i++)
			{
				auto subSrc = GetSubSource(mip - i);
				if(subSrc==nullptr)
					continue;

				GLvoid* pData = &subSrc->MemData[0];
				auto buffSize = (GLsizei)subSrc->MemData.size();
				
				sdk->CompressedTexImage2D(
					GL_TEXTURE_2D,
					i,
					internalFormat,
					subSrc->Width,//layer.Width,
					subSrc->Height,//layer.Height,
					0,
					buffSize,//layer.Size,
					pData
				);
				resourceSize += buffSize;

				float calc_size = ceil((float)subSrc->Width / 4) * ceil((float)subSrc->Height / 4) * NumOfStride;
				ASSERT((GLsizei)(calc_size) == buffSize);
			}

			sdk->BindTexture(GL_TEXTURE_2D, 0);
		}

		AutoRef<IGLTexture2D> pTex2dFromFile = new IGLTexture2D();
		pTex2dFromFile->mGlesTexture2D = textureId;
		srv->mTexture2D.StrongRef(pTex2dFromFile);

		auto pCurRes = GetSubSource(mip);
		if(pCurRes!=nullptr)
			srv->GetResourceState()->SetResourceSize(resourceSize);
	}
};

IGLShaderResourceView::IGLShaderResourceView()
{
	//mGlesTexture2D = 0;
	mImage = nullptr;
	mTexStreaming = new GLTextureStreaming();
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

bool IGLShaderResourceView::InitGLPointer(IGLRenderContext* rc, const char* file, bool isLoad)
{
	mRenderContext.FromObject(rc);
	mResourceFile = file;
	auto sdk = GLSdk::ImmSDK;
	std::string oriFile;
	//XImageDecoder * pDecoder = nullptr;
	if (isLoad)
	{
		//BYTE* pBuffer = nullptr;
		
		//size_t memLength = 0;

		//��ȡ.txpic RawImage����Etc2
		std::string txPic = file;
		static auto extLen = strlen(".txpic");
		if (txPic.length() > extLen && txPic.substr(txPic.length() - extLen) == ".txpic")
		{
			auto newName = txPic.substr(0, txPic.length() - extLen);
			newName += ".txpic";
			AutoRef<VRes2Memory> io(F2MManager::Instance.GetF2M(newName.c_str()));
			if (io == nullptr)
			{
				VFX_LTRACE(ELTT_Resource, "texture %s file is not found\r\n", file);
				return false;
			}
			AutoRef<XNDNode> node = XNDNode_New();
			node->Load(io);
			
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
	}
	else
	{
		this->GetResourceState()->SetStreamState(SS_Invalid);
	}
	
	return true;
}

void IGLShaderResourceView::InvalidateResource()
{
	auto pGLTexture = mTexture2D.UnsafeConvertTo<IGLTexture2D>();
	if (pGLTexture != nullptr && pGLTexture->mGlesTexture2D != nullptr)
	{
		pGLTexture->mGlesTexture2D.reset();
	}
	mTexStreaming->SetStreamMipLevel(0);
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

	return InitGLPointer(rc, mResourceFile.c_str(), true)?1:0;
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