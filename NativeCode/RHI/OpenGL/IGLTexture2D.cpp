#include "IGLTexture2D.h"
#include "IGLRenderContext.h"
#include "IGLCommandList.h"

#define new VNEW

NS_BEGIN

IGLTexture2D::IGLTexture2D()
{
	mIsReadable = false;
}

IGLTexture2D::~IGLTexture2D()
{
	Cleanup();
}

void IGLTexture2D::Cleanup()
{
	mGlesTexture2D.reset();
}

int GetRowBytesFromWidthAndFormat_Local(int width, EPixelFormat inFormat)
{
	return GetPixelByteWidth(inFormat) * width;
}

GLint GetStride_Local(INT rowBytesSize)
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

bool IGLTexture2D::Init(IGLRenderContext* pCtx, const ITexture2DDesc* pDesc)
{
	mTextureDesc = *pDesc;
	
	std::shared_ptr<GLSdk> sdk(new GLSdk(TRUE));
	//auto sdk = GLSdk::ImmSDK;
	GLint format = GL_DEPTH_STENCIL;
	GLint internalFormat = GL_DEPTH24_STENCIL8;
	GLenum type = GL_UNSIGNED_INT_24_8;

	mGlesTexture2D = sdk->GenTextures();
	sdk->BindTexture(GL_TEXTURE_2D, mGlesTexture2D);
	if (pDesc->Format == PXF_D24_UNORM_S8_UINT || pDesc->Format == PXF_D32_FLOAT || pDesc->Format == PXF_D16_UNORM)
	{
		switch (pDesc->Format)
		{
		case PXF_D24_UNORM_S8_UINT:
			format = GL_DEPTH_STENCIL;
			internalFormat = GL_DEPTH24_STENCIL8;
			type = GL_UNSIGNED_INT_24_8;
			break;
		case PXF_D32_FLOAT:
#if defined(PLATFORM_WIN)
			format = GL_DEPTH_COMPONENT;
			internalFormat = GL_DEPTH_COMPONENT32F;
#elif defined(PLATFORM_DROID)
			format = GL_DEPTH_COMPONENT;
			internalFormat = GL_DEPTH_COMPONENT32F;
#else
#endif
			type = GL_FLOAT;
			break;
		case PXF_D16_UNORM:
			format = GL_DEPTH_COMPONENT;
			internalFormat = GL_DEPTH_COMPONENT16;
			type = GL_UNSIGNED_INT;
			break;
		default:
			break;
		}
		sdk->TexImage2D(GL_TEXTURE_2D, 0, internalFormat, mTextureDesc.Width, mTextureDesc.Height, 0, format, type, 0);
		sdk->TexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MAG_FILTER, GL_NEAREST);
		sdk->TexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MIN_FILTER, GL_NEAREST);
		sdk->TexParameteri(GL_TEXTURE_2D, GL_TEXTURE_WRAP_S, GL_CLAMP_TO_EDGE);
		sdk->TexParameteri(GL_TEXTURE_2D, GL_TEXTURE_WRAP_T, GL_CLAMP_TO_EDGE);
		sdk->TexParameteri(GL_TEXTURE_2D, GL_TEXTURE_BASE_LEVEL, 0);
		sdk->TexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MAX_LEVEL, 0);
		sdk->BindTexture(GL_TEXTURE_2D, 0);
	}
	else
	{
		sdk->PixelStorei(GL_UNPACK_ALIGNMENT, 1);

		sdk->TexParameteri(GL_TEXTURE_2D, GL_TEXTURE_BASE_LEVEL, 0);

		sdk->TexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MAX_LEVEL, mTextureDesc.MipLevels - 1);

		sdk->TexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MAG_FILTER, GL_LINEAR);

		sdk->TexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MIN_FILTER, GL_LINEAR);

		sdk->TexParameterf(GL_TEXTURE_2D, GL_TEXTURE_LOD_BIAS, 0.f);

		UINT width = mTextureDesc.Width;
		UINT height = mTextureDesc.Height;

		for (UINT i = 0; i < mTextureDesc.MipLevels; i++)
		{
			INT rowBytesSize = GetRowBytesFromWidthAndFormat_Local(width, mTextureDesc.Format);
			GLint stride = GetStride_Local(rowBytesSize);
			sdk->PixelStorei(GL_UNPACK_ALIGNMENT, stride);


			FormatToGL(mTextureDesc.Format, internalFormat, format, type);
			if (pDesc->InitData != nullptr)
				sdk->TexImage2D(GL_TEXTURE_2D, (GLint)i, internalFormat, width, height, 0, format, type, pDesc->InitData[i].pSysMem);
			else
				sdk->TexImage2D(GL_TEXTURE_2D, (GLint)i, internalFormat, width, height, 0, format, type, nullptr);

			width = width / 2;
			height = height / 2;
		}

		sdk->BindTexture(GL_TEXTURE_2D, 0);
	}

	GLSdk::ImmSDK->PushCommandDirect([=]()->void
	{
		sdk->Execute();
	}, "IGLTexture2D::Init");

	return true;
}

vBOOL IGLTexture2D::MapMipmap(ICommandList* cmd, int MipLevel, void** ppData, UINT* pRowPitch, UINT* pDepthPitch)
{
	if (mIsReadable == false)
		return FALSE;
	if (mGlesTexture2D->BufferId == 0)
		return FALSE;
	cmd = cmd->GetContext()->GetImmCommandList();
	GLSdk* sdk = ((IGLCommandList*)cmd)->mCmdList;
	sdk->BindBuffer(GL_PIXEL_PACK_BUFFER, mGlesTexture2D);
	*pRowPitch = ((mTextureDesc.Width * GetPixelByteWidth(mTextureDesc.Format) + 3) / 4) * 4;
	UINT dataSize = (*pRowPitch) * mTextureDesc.Height;
	sdk->MapBufferRange(ppData, GL_PIXEL_PACK_BUFFER, 0, dataSize, GL_MAP_READ_BIT);
	return (*ppData)!=nullptr;
}

void IGLTexture2D::UnmapMipmap(ICommandList* cmd, int MipLevel)
{
	if (mIsReadable == false)
		return;

	cmd = cmd->GetContext()->GetImmCommandList();
	GLSdk* sdk = ((IGLCommandList*)cmd)->mCmdList;
	GLboolean ret;
	sdk->UnmapBuffer(&ret, GL_PIXEL_PACK_BUFFER);
	sdk->BindBuffer(GL_PIXEL_PACK_BUFFER, 0);
}

void IGLTexture2D::UpdateMipData(ICommandList* cmd, UINT level, void* pData, UINT width, UINT height, UINT Pitch)
{
	GLSdk* sdk = ((IGLCommandList*)cmd)->mCmdList;
	
	sdk->BindTexture(GL_TEXTURE_2D, mGlesTexture2D);

	/*sdk->PixelStorei(GL_UNPACK_ALIGNMENT, 1);
	sdk->TexParameteri(GL_TEXTURE_2D, GL_TEXTURE_BASE_LEVEL, 0);
	sdk->TexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MAX_LEVEL, mDesc.MipLevels - 1);
	sdk->TexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MAG_FILTER, GL_LINEAR);
	sdk->TexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MIN_FILTER, GL_LINEAR);*/

	INT rowBytesSize = GetRowBytesFromWidthAndFormat_Local(width, mTextureDesc.Format);
	GLint stride = GetStride_Local(rowBytesSize);
	sdk->PixelStorei(GL_UNPACK_ALIGNMENT, stride);

	GLint format = GL_DEPTH_STENCIL;
	GLint internalFormat = GL_DEPTH24_STENCIL8;
	GLenum type = GL_UNSIGNED_INT_24_8;
	FormatToGL(mTextureDesc.Format, internalFormat, format, type);
	sdk->TexImage2D(GL_TEXTURE_2D, (GLint)level, internalFormat, width, height, 0, format, type, pData);

	sdk->BindTexture(GL_TEXTURE_2D, 0);
}

NS_END