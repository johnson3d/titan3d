#include "NxBuffer.h"
#include "NxDrawcall.h"
#include "NxCommandList.h"
#include "../../Base/float16/float16.h"
//#include "../Bricks/ImageDecoder/XImageDecoder.h"
#include "../Bricks/ImageDecoder/XImageBuffer.h"

#define new VNEW

NS_BEGIN

template <>
struct FTypeAssignment<NxRHI::ISrView>
{
	static void Assign(NxRHI::ISrView& left, const NxRHI::ISrView& right)
	{
		ASSERT(false);
	}
};

ENGINE_RTTI_IMPL(ISrView);
StructBegin(ISrView, EngineNS::NxRHI)
StructEnd(EngineNS::NxRHI::ISrView, VIUnknown)

namespace NxRHI
{
	std::atomic<int> ITexture::AliveCount;
	std::atomic<int> ITexture::AliveAttachBufferCount;
	bool IBuffer::FetchGpuData(IGpuDevice* device, UINT index, IBlobObject* blob)
	{
		FMappedSubResource subRes;
		if (Map(index, &subRes, true))
		{
			blob->PushData(&subRes.RowPitch, sizeof(UINT));
			blob->PushData(&subRes.DepthPitch, sizeof(UINT));
			blob->PushData(subRes.pData, this->Desc.Size);
			Unmap(index);
			return true;
		}
		else
		{
			if (device == nullptr)
				return false;

			auto desc = Desc;
			desc.Type = EBufferType::BFT_NONE;
			desc.CpuAccess = ECpuAccess::CAS_READ;
			desc.Usage = EGpuUsage::USAGE_STAGING;
			desc.RowPitch = Desc.Size;
			desc.DepthPitch = Desc.Size;
			auto cpBuffer = MakeWeakRef(device->CreateBuffer(&desc, __FILE__, __LINE__));
			auto cpDraw = MakeWeakRef(device->CreateCopyDraw(__FILE__, __LINE__));
			
			cpDraw->Mode = ECopyDrawMode::CDM_Buffer2Buffer;
			cpDraw->FootPrint.Format = PXF_UNKNOWN;
			cpDraw->FootPrint.X = 0;
			cpDraw->FootPrint.Y = 0;
			cpDraw->FootPrint.Z = 0;
			cpDraw->FootPrint.Width = Desc.Size;
			cpDraw->FootPrint.Height = 1;
			cpDraw->FootPrint.Depth = 1;
			cpDraw->FootPrint.RowPitch = desc.RowPitch;
			cpDraw->FootPrint.TotalSize = desc.RowPitch;
			cpDraw->BindBufferSrc(this);
			cpDraw->BindBufferDest(cpBuffer);
			cpDraw->SrcSubResource = 0;
			cpDraw->DestSubResource = 0;

			{
				FTransientCmd cmd(device, EQueueType::QU_Transfer, "FetchBuffer");
				cmd.GetCmdList()->PushGpuDraw(cpDraw.GetPtr());
			}
			device->GetCmdQueue()->Flush(EQueueType::QU_Transfer);
			return cpBuffer->FetchGpuData(device, index, blob);
		}
	}
	IBuffer* IBuffer::CreateReadable(IGpuDevice* device, int subRes, ICopyDraw* cpDraw)
	{
		auto cpDesc = this->Desc;
		cpDesc.Usage = USAGE_STAGING;
		cpDesc.CpuAccess = CAS_READ;
		cpDesc.Type = EBufferType::BFT_NONE;
		auto cpBuffer = device->CreateBuffer(&cpDesc, __FILE__, __LINE__);
		if (cpDraw != nullptr)
		{
			cpDraw->BindBufferDest(cpBuffer);
			cpDraw->BindBufferSrc(this);
			cpDraw->DestSubResource = 0;
			cpDraw->Mode = NxRHI::ECopyDrawMode::CDM_Buffer2Buffer;
		}
		return cpBuffer;
	}
	IBuffer* ITexture::CreateReadable(IGpuDevice* device, int subRes, ICopyDraw* cpDraw)
	{
		FBufferDesc cpDesc;
		cpDesc.SetDefault();
		cpDesc.Type = NxRHI::EBufferType::BFT_NONE;
		cpDesc.Usage = USAGE_STAGING;
		cpDesc.CpuAccess = CAS_READ;
		auto blockSize = GetCompressBlockByteSize(Desc.Format);
		if (blockSize == 0)
		{
			cpDesc.RowPitch = device->GetGpuResourceAlignment()->RoundupTexturePitch(Desc.Width * GetPixelByteWidth(Desc.Format));
			cpDesc.Size = cpDesc.RowPitch * Desc.Height;
		}
		else
		{
			unsigned int blockW, blockH;
			GetCompressBlockDimensions(Desc.Format, blockW, blockH);
			auto numBlocksWide = (Desc.Width + blockW - 1) / blockW;
			auto numBlocksHigh = (Desc.Height + blockH - 1) / blockH;
			cpDesc.RowPitch = device->GetGpuResourceAlignment()->RoundupTexturePitch(numBlocksWide * blockSize);
			cpDesc.Size = cpDesc.RowPitch * numBlocksHigh;
		}
		auto cpBuffer = device->CreateBuffer(&cpDesc, __FILE__, __LINE__);
		if (cpDraw != nullptr)
		{
			cpDraw->BindBufferDest(cpBuffer);
			cpDraw->BindTextureSrc(this);
			cpDraw->SrcSubResource = subRes;
			cpDraw->Mode = NxRHI::ECopyDrawMode::CDM_Texture2Buffer;

			cpDraw->FootPrint.SetDefault();
			cpDraw->FootPrint.Format = Desc.Format;
			cpDraw->FootPrint.RowPitch = cpDesc.RowPitch;
			cpDraw->FootPrint.TotalSize = cpDesc.Size;
			cpDraw->FootPrint.Width = Desc.Width;
			cpDraw->FootPrint.Height = Desc.Height;
			cpDraw->FootPrint.Depth = 1;
		}
		//cmdlist->PushGpuDraw(cpDraw);
		return cpBuffer;
	}

	long ISrView::AddRef() const
	{
		/*if (Buffer != nullptr)
			Buffer->AddRef();*/
		return IGpuResource::AddRef();
	}
	void ISrView::Release() const
	{
		/*if (Buffer != nullptr)
			Buffer->Release();*/
		IGpuResource::Release();
	}
	/*bool IBuffer::FetchGpuData(ICommandList* cmd, UINT index, IBlobObject* blob)
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
	}*/
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

	void FCbvUpdater::UpdateCBVs()
	{
		VAutoVSLLock lk(mLocker);
		for (auto& i : mCBVs)
		{
			i->FlushDirty(false);
			i->Updater = nullptr;
		}
		mCBVs.clear();
	}
}

NS_END