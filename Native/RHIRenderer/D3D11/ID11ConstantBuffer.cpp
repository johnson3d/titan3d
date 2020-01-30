#include "ID11ConstantBuffer.h"
#include "ID11RenderContext.h"
#include "ID11CommandList.h"
#include "../../../Core/vfxSampCounter.h"

#define new VNEW

NS_BEGIN

ID11ConstantBuffer::ID11ConstantBuffer()
{
	mBuffer = nullptr;
}

ID11ConstantBuffer::~ID11ConstantBuffer()
{
	Safe_Release(mBuffer);
}

//bool ID11ConstantBuffer::MapMemory(ICommandList* cmd, void** ppBuffer)
//{
//	auto d11Cmd = (ID11CommandList*)cmd;
//	D3D11_MAPPED_SUBRESOURCE ms;
//	memset(&ms, 0, sizeof(ms));
//	auto hr = d11Cmd->mDeferredContext->Map(mBuffer, 0, D3D11_MAP_WRITE_DISCARD, 0, &ms);
//	if (FAILED(hr))
//		return false;
//	*ppBuffer = ms.pData;
//	return true;
//}
//
//void ID11ConstantBuffer::UnMapMemory(ICommandList* cmd, void* pBuffer)
//{
//	auto d11Cmd = (ID11CommandList*)cmd;
//	d11Cmd->mDeferredContext->Unmap(mBuffer, 0);
//}

/*void ID11ConstantBuffer::UpdateDrawPass(ICommandList* cmd)
{
	if (mDirty == false)
		return;
	mDirty = false;

	UpdateContent(cmd, &VarBuffer[0], (UINT)VarBuffer.size());
}*/

bool ID11ConstantBuffer::UpdateContent(ICommandList* cmd, void* pBuffer, UINT Size)
{
	AUTO_SAMP("Native.IConstantBuffer.UpdateContent");

	if (Size > Desc.Size || pBuffer == NULL)
		return false;
	RHI_ASSERT(Size == Desc.Size);

	auto refCmdList = (ID11CommandList*)cmd;
	//refCmdList->mDeferredContext->UpdateSubresource(mBuffer, 0, NULL, pBuffer, 0, 0);
	D3D11_MAPPED_SUBRESOURCE MappedSubresource;
	if (refCmdList->mDeferredContext->Map(mBuffer, 0, D3D11_MAP_WRITE_DISCARD, 0, &MappedSubresource) == S_OK)
	{
		memcpy(MappedSubresource.pData, pBuffer, Size);
		refCmdList->mDeferredContext->Unmap(mBuffer, 0);
	}
	
	return true;
}

bool ID11ConstantBuffer::Init(ID11RenderContext* rc, const IConstantBufferDesc* desc)
{
	D3D11_BUFFER_DESC BufferDesc;
	//memset(&bd, 0, sizeof(bd));
	BufferDesc.Usage = D3D11_USAGE_DYNAMIC;
	BufferDesc.ByteWidth = desc->Size;
	BufferDesc.BindFlags = D3D11_BIND_CONSTANT_BUFFER;
	BufferDesc.CPUAccessFlags = D3D11_CPU_ACCESS_WRITE;
	BufferDesc.MiscFlags = 0;
	BufferDesc.StructureByteStride = 0;
	auto hr = rc->mDevice->CreateBuffer(&BufferDesc, NULL, &mBuffer);
	if (FAILED(hr))
		return hr;
	Desc = *desc;

#ifdef _DEBUG
	mBuffer->SetPrivateData(WKPDID_D3DDebugObjectName, 0, NULL);
	static UINT UniqueId = 0;
	auto debuginfo = VStringA_FormatV("CB_%s_%u", desc->Name.c_str(), UniqueId++);
	mBuffer->SetPrivateData(WKPDID_D3DDebugObjectName, (UINT)debuginfo.length(), debuginfo.c_str());
#endif

	return true;
}

NS_END