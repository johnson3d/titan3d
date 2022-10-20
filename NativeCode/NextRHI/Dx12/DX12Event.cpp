#include "DX12Event.h"
#include "DX12GpuDevice.h"

#define new VNEW

NS_BEGIN

namespace NxRHI
{
	bool DX12Event::Init(DX12GpuDevice* pDevice, const FEventDesc& desc, const char* name)
	{
		Name = name;
		return true;
	}

	DX12Fence::DX12Fence()
	{
		mFence = nullptr;
	}
	DX12Fence::~DX12Fence()
	{
		Safe_Release(mFence);
	}
	bool DX12Fence::Init(DX12GpuDevice* pDevice, const FFenceDesc& desc, const char* name)
	{
		Desc = desc;
		Name = name;

		mEvent = MakeWeakRef(new DX12Event(Name.c_str()));
		AspectValue = desc.InitValue;
		if (S_OK != pDevice->mDevice->CreateFence(desc.InitValue, (D3D12_FENCE_FLAGS)desc.Type, __uuidof(ID3D12Fence), (void**)&mFence))
		{
			return false;
		}
		SetDebugName(name);
		return true;
	}
	UINT64 DX12Fence::GetCompletedValue()
	{
		return mFence->GetCompletedValue();
	}
	void DX12Fence::CpuSignal(UINT64 value)
	{
		mFence->Signal(value);
	}
	void DX12Fence::Signal(ICmdQueue* queue, UINT64 value)
	{
		if (AspectValue >= value)
		{
			ASSERT(false);
			return;
		}
		AspectValue = value;
		mFence->SetEventOnCompletion(value, mEvent->mHandle);
		((DX12CmdQueue*)queue)->mCmdQueue->Signal(mFence, value);
	}
	bool DX12Fence::Wait(UINT64 value, UINT timeOut)
	{
		/*while (mFence->GetCompletedValue() < value)
		{

		}*/
		if (mFence->GetCompletedValue() < value)
		{
			return mEvent->Wait(timeOut);
		}
		return true;
	}
	void DX12Fence::SetDebugName(const char* name)
	{
		std::wstring n = StringHelper::strtowstr(name);
		mFence->SetName(n.c_str());
	}
}

NS_END
