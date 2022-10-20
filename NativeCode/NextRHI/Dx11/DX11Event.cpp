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

		AspectValue = desc.InitValue;
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
	void DX11Fence::Signal(ICmdQueue* queue, UINT64 value)
	{
		if (AspectValue >= value)
		{
			ASSERT(false);
			return;
		}
		AspectValue = value;
		mFence->SetEventOnCompletion(value, mEvent->mHandle);
		((DX11CmdQueue*)queue)->mHardwareContext->mContext4->Signal(mFence, value);
	}
	bool DX11Fence::Wait(UINT64 value, UINT timeOut)
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
}

NS_END
