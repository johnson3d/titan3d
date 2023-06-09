#include "DX11Event.h"
#include "DX11GpuDevice.h"
#include "DX11CommandList.h"

#define new VNEW

NS_BEGIN

namespace NxRHI
{
	bool DX11Event::Init(DX11GpuDevice* pDevice, const FEventDesc& desc, const char* name)
	{
		Name = name;
		return true;
	}

	DX11Fence::DX11Fence()
	{
		mFence = nullptr;
	}
	DX11Fence::~DX11Fence()
	{
		Safe_Release(mFence);
	}
	bool DX11Fence::Init(DX11GpuDevice* pDevice, const FFenceDesc& desc, const char* name)
	{
		Desc = desc;
		Name = name;

		if (pDevice->mDevice5 == nullptr)
			return false;

		ExpectValue = desc.InitValue;
		mEvent = MakeWeakRef(new DX11Event(Name.c_str()));
		if (S_OK != pDevice->mDevice5->CreateFence(desc.InitValue, (D3D11_FENCE_FLAG)desc.Type, __uuidof(ID3D11Fence), (void**)&mFence))
		{
			return false;
		}
		return true;
	}
	UINT64 DX11Fence::GetCompletedValue()
	{
		return mFence->GetCompletedValue();
	}
	void DX11Fence::CpuSignal(UINT64 value)
	{
		ASSERT(false);
		//mFence->Sig
	}
	void DX11Fence::Signal(ICmdQueue* queue, UINT64 value, EQueueType type)
	{
		mFence->SetEventOnCompletion(value, mEvent->mHandle);
		auto dx11Queue =((DX11CmdQueue*)queue);
		VAutoVSLLock locker(dx11Queue->mImmCmdListLocker);
		dx11Queue->mHardwareContext->mContext4->Signal(mFence, value);
	}
	UINT64 DX11Fence::Wait(UINT64 value, UINT timeOut)
	{
		/*while (mFence->GetCompletedValue() < value)
		{

		}*/
		auto completed = mFence->GetCompletedValue();
		while (completed < value)
		{
			if (completed == 0xffffffffffffffff)
			{
				//mDeviceRef.GetPtr()->OnDeviceRemoved();
			}
			mEvent->Wait(timeOut);
			completed = mFence->GetCompletedValue();
		}
		return completed;
	}
}

NS_END
