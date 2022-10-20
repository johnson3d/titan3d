#include "NxBuffer.h"
#include "../../Base/float16/float16.h"
#include "../../../3rd/native/Image.Shared/XImageDecoder.h"
#include "../../../3rd/native/Image.Shared/XImageBuffer.h"

#define new VNEW

NS_BEGIN

namespace NxRHI
{
	bool IBuffer::FetchGpuData(ICommandList* cmd, UINT index, IBlobObject* blob)
	{
		FMappedSubResource subRes;
		if (Map(cmd, index, &subRes, true))
		{
			blob->PushData(&subRes.RowPitch, sizeof(UINT));
			blob->PushData(&subRes.DepthPitch, sizeof(UINT));
			blob->PushData(subRes.pData, this->Desc.Size);
			Unmap(cmd, index);
			return true;
		}
		return false;
	}
	bool ITexture::FetchGpuData(ICommandList* cmd, UINT index, IBlobObject* blob)
	{
		FMappedSubResource subRes;
		if (Map(cmd, index, &subRes, true))
		{	
			blob->PushData(&subRes.RowPitch, sizeof(UINT));
			blob->PushData(&subRes.DepthPitch, sizeof(UINT));
			blob->PushData(subRes.pData, subRes.DepthPitch);
			Unmap(cmd, index);
			return true;
		}
		return false;
	}
	void ITexture::BuildImage2DBlob(IBlobObject* blob, void* pData, UINT RowPitch, const FTextureDesc* desc)
	{
		int nStride = (desc->Width * 32 + 7) / 8;
		int nPixelBufferSize = nStride * desc->Height;
		if (blob->GetSize() != sizeof(FPixelDesc) + nPixelBufferSize)
		{
			blob->ReSize(sizeof(FPixelDesc) + nPixelBufferSize);
		}
		FPixelDesc* pOutDesc = (FPixelDesc*)blob->GetData();
		pOutDesc->Width = desc->Width;
		pOutDesc->Height = desc->Height;
		pOutDesc->Stride = nStride;
		pOutDesc->Format = PXF_R8G8B8A8_UNORM;

		XImageBuffer image;
		image.m_pImage = (uint8_t*)(pOutDesc + 1);
		image.m_nWidth = desc->Width;
		image.m_nHeight = desc->Height;
		image.m_nBitCount = 32;
		image.m_nStride = nStride;
		BYTE* row = (BYTE*)pData;
		BYTE* dst = image.m_pImage;
		UINT copySize = desc->Width * 4;
		if (desc->Format == PXF_R8G8B8A8_UNORM)
		{
			for (UINT i = 0; i < desc->Height; i++)
			{
				memcpy(dst, row, copySize);
				/*for (UINT j = 0; j < mDesc.Width; j++)
				{
					if (mDesc.Format == PXF_R8G8B8A8_UNORM)
					{
						auto color = *(DWORD*)(&row[j * 4]);
						if (color != 0)
						{
							dst[j * 4] = color;
						}
						dst[j * 4] = row[j * 4];
						dst[j * 4 + 1] = row[j * 4 + 1];
						dst[j * 4 + 2] = row[j * 4 + 2];
						dst[j * 4 + 3] = row[j * 4 + 3];
					}
					else
					{
						dst[j] = 0;
					}
				}*/
				row += RowPitch;
				dst += image.m_nStride;
			}
		}
		else if (desc->Format == PXF_B8G8R8A8_UNORM)
		{
			for (UINT i = 0; i < desc->Height; i++)
			{
				for (UINT j = 0; j < desc->Width; j++)
				{
					//auto color = *(DWORD*)(&row[j * 4]);
					dst[j * 4 + 3] = row[j * 4];
					dst[j * 4 + 2] = row[j * 4 + 1];
					dst[j * 4 + 1] = row[j * 4 + 2];
					dst[j * 4 + 0] = row[j * 4 + 3];
				}
				row += RowPitch;
				dst += image.m_nStride;
			}
		}
		else if (desc->Format == PXF_R10G10B10A2_UNORM)
		{
			for (UINT i = 0; i < desc->Height; i++)
			{
				for (UINT j = 0; j < desc->Width; j++)
				{
					auto color = *(DWORD*)(&row[j * 4]);
					DWORD channel = (color & 0x3FF);
					dst[j * 4] = (BYTE)(channel * 255 / 1024);

					channel = ((color >> 10) & 0x3FF);
					dst[j * 4 + 1] = (BYTE)(channel * 255 / 1024);

					channel = ((color >> 20) & 0x3FF);
					dst[j * 4 + 2] = (BYTE)(channel * 255 / 1024);

					channel = ((color >> 30) & 0x3);
					dst[j * 4 + 3] = (BYTE)(channel * 255 / 4);
				}
				row += RowPitch;
				dst += image.m_nStride;
			}
		}
		else if (desc->Format == PXF_R16G16B16A16_FLOAT)
		{
			for (UINT i = 0; i < desc->Height; i++)
			{
				auto color = (float16*)row;
				for (UINT j = 0; j < desc->Width; j++)
				{
					auto f1 = (float)color[j * 4];
					auto f2 = (float)color[j * 4 + 1];
					auto f3 = (float)color[j * 4 + 2];
					auto f4 = (float)color[j * 4 + 3];
					if (f1 > 1.0f)
						dst[j * 4] = 255;
					else
						dst[j * 4] = (BYTE)(f1 * 255.0f);
					if (f2 > 1.0f)
						dst[j * 4] = 255;
					else
						dst[j * 4 + 1] = (BYTE)(f2 * 255.0f);
					if (f3 > 1.0f)
						dst[j * 4] = 255;
					else
						dst[j * 4 + 2] = (BYTE)(f3 * 255.0f);
					if (f4 > 1.0f)
						dst[j * 4] = 255;
					else
						dst[j * 4 + 3] = (BYTE)(f4 * 255.0f);
				}
				row += RowPitch;
				dst += image.m_nStride;
			}
		}

		/*switch (Desc::Instance->mRHIType)
		{
		case RHT_OGL:
		{
			image.FlipPixel4();
		}
		break;
		default:
		{
			break;
		}
		}*/

		image.m_pImage = nullptr;
	}
	void ITexture::BuildImage2DBlob(IBlobObject* blob, IBlobObject* gpuData, const FTextureDesc* desc)
	{
		MemStreamReader reader;
		reader.ProxyPointer((BYTE*)gpuData->GetData(), gpuData->GetSize());
		UINT rowPitch;
		UINT depthPitch;
		reader.Read(rowPitch);
		reader.Read(depthPitch);
		BuildImage2DBlob(blob, reader.GetPointer() + reader.Tell(), rowPitch, desc);
	}
	/*bool ITexture::FetchGpuDataAsImage2DBlob(ICommandList* cmd, UINT subRes, IBlobObject* blob)
	{
		IBlobObject tmp;
		if (false == this->FetchGpuData(cmd, subRes, &tmp))
			return false;
		
		MemStreamReader reader;
		reader.ProxyPointer((BYTE*)tmp.GetData(), tmp.GetSize());
		UINT rowPitch;
		UINT depthPitch;
		reader.Read(rowPitch);
		reader.Read(depthPitch);
		BuildImage2DBlob(blob, reader.GetPointer() + reader.Tell(), rowPitch, &Desc);
		return true;
	}*/
	ITexture* ISrView::GetBufferAsTexture() 
	{
		return Buffer.As<ITexture>();
	}
	IBuffer* ISrView::GetBufferAsBuffer() 
	{
		return Buffer.As<IBuffer>();
	}
}

NS_END