#pragma once
#include "IConstantBuffer.h"

NS_BEGIN

struct TR_CLASS(SV_NameSpace = EngineNS, SV_UsingNS = EngineNS, SV_LayoutStruct = 8)
TSBindInfo
{
	TSBindInfo()
	{
		VSBindPoint = -1;
		PSBindPoint = -1;
		CSBindPoint = -1;
		BindCount = 0;
	}
	ECBufferRhiType		Type;
	UINT				VSBindPoint;
	UINT				PSBindPoint;
	UINT				CSBindPoint;
	UINT				BindCount;
	VNameString			Name;
};

class TR_CLASS(SV_NameSpace = EngineNS, SV_UsingNS = EngineNS)
ShaderReflector : public VIUnknown
{
public:
	std::vector<IConstantBufferDesc>	mCBDescArray;
	std::vector<TSBindInfo>				mTexBindInfoArray;
	std::vector<TSBindInfo>				mSamplerBindInfoArray;

	ShaderReflector();
	~ShaderReflector();

	TR_FUNCTION()
	UINT FindCBuffer(const char* name)
	{
		for (size_t i = 0; i < mCBDescArray.size(); i++)
		{
			if (mCBDescArray[i].Name == name)
				return (int)i;
		}
		return -1;
	}
	TR_FUNCTION()
	IConstantBufferDesc* GetCBuffer(UINT index)
	{
		if (index >= (UINT)mCBDescArray.size())
			return nullptr;
		return &mCBDescArray[index];
	}
	TR_FUNCTION()
	UINT FindSRV(const char* name)
	{
		for (size_t i = 0; i < mTexBindInfoArray.size(); i++)
		{
			if (mTexBindInfoArray[i].Name == name)
				return (int)i;
		}
		return -1;
	}
	TR_FUNCTION()
	TSBindInfo* GetSRV(UINT index)
	{
		if (index >= (UINT)mTexBindInfoArray.size())
			return nullptr;
		return &mTexBindInfoArray[index];
	}
	TR_FUNCTION()
	UINT FindSampler(const char* name)
	{
		for (size_t i = 0; i < mSamplerBindInfoArray.size(); i++)
		{
			if (mSamplerBindInfoArray[i].Name == name)
				return (int)i;
		}
		return -1;
	}
	TR_FUNCTION()
	TSBindInfo* GetSampler(UINT index)
	{
		if (index >= (UINT)mSamplerBindInfoArray.size())
			return nullptr;
		return &mSamplerBindInfoArray[index];
	}
};

NS_END