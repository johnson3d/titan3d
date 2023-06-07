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
		mDeviceRef.FromObject(pDevice);

		mEvent = MakeWeakRef(new DX12Event(Name.c_str()));
		
		if (S_OK != pDevice->mDevice->CreateFence(desc.InitValue, (D3D12_FENCE_FLAGS)desc.Type, __uuidof(ID3D12Fence), (void**)&mFence))
		{
			return false;
		}
		ExpectValue = desc.InitValue;
		SetDebugName(name);
		return true;
	}
	UINT64 DX12Fence::GetCompletedValue()
	{
		auto result = mFence->GetCompletedValue();
		if (result == 0xffffffffffffffff)
		{
			mDeviceRef.GetPtr()->OnDeviceRemoved();
		}
		return result;
	}
	void DX12Fence::CpuSignal(UINT64 value)
	{
		mFence->Signal(value);
	}
	void DX12Fence::Signal(ICmdQueue* queue, UINT64 value, EQueueType type)
	{
		mFence->SetEventOnCompletion(value, mEvent->mHandle);
		auto dx12Queue = ((DX12CmdQueue*)queue);
		VAutoVSLLock locker(dx12Queue->mQueueLocker);
		auto hr = dx12Queue->mCmdQueue->Signal(mFence, value);
		if (hr == DXGI_ERROR_DEVICE_REMOVED)
		{
			mDeviceRef.GetPtr()->OnDeviceRemoved();
		}
	}
	UINT64 DX12Fence::Wait(UINT64 value, UINT timeOut)
	{
		/*while (mFence->GetCompletedValue() < value)
		{

		}*/
		auto completed = mFence->GetCompletedValue();
		while (completed < value)
		{
			if (completed == 0xffffffffffffffff)
			{
				mDeviceRef.GetPtr()->OnDeviceRemoved();
			}
			mEvent->Wait(timeOut);
			completed = mFence->GetCompletedValue();
		}
		return completed;
	}
	void DX12Fence::SetDebugName(const char* name)
	{
		std::wstring n = StringHelper::strtowstr(name);
		mFence->SetName(n.c_str());
	}
}

NS_END
