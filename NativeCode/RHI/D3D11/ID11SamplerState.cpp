#include "ID11SamplerState.h"
#include "ID11RenderContext.h"

#define new VNEW

NS_BEGIN

ID11SamplerState::ID11SamplerState()
{
	mSampler = nullptr;
}

ID11SamplerState::~ID11SamplerState()
{
	Safe_Release(mSampler);
}

bool ID11SamplerState::Init(ID11RenderContext* rc, const ISamplerStateDesc* desc)
{
	mDesc = *desc;

	mD11Desc.Filter = FilterToDX(desc->Filter);
	mD11Desc.AddressU = AddressModeToDX(desc->AddressU);
	mD11Desc.AddressV = AddressModeToDX(desc->AddressV);
	mD11Desc.AddressW = AddressModeToDX(desc->AddressW);
	mD11Desc.ComparisonFunc = CmpModeToDX(desc->CmpMode);
	mD11Desc.MinLOD = desc->MinLOD;
	mD11Desc.MaxLOD = desc->MaxLOD;
	memcpy(mD11Desc.BorderColor, desc->BorderColor, sizeof(desc->BorderColor));
	mD11Desc.MaxAnisotropy = desc->MaxAnisotropy;
	mD11Desc.MipLODBias = desc->MipLODBias;
	auto hr = rc->mDevice->CreateSamplerState(&mD11Desc, &mSampler);
	if (FAILED(hr))
	{
		return false;
	}
#ifdef _DEBUG
	static UINT UniqueId = 0;
	auto debuginfo = VStringA_FormatV("Sampler_%u", UniqueId++);
	mSampler->SetPrivateData(WKPDID_D3DDebugObjectName, (UINT)debuginfo.size(), debuginfo.c_str());
#endif
	
	return true;
}

NS_END