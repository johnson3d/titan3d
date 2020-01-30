#pragma once
#include "IConstantBuffer.h"

NS_BEGIN

struct TextureBindInfo
{
	TextureBindInfo()
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
	std::string			Name;
};

struct SamplerBindInfo : public TextureBindInfo
{
	SamplerBindInfo& operator = (const TextureBindInfo& rh)
	{
		*(TextureBindInfo*)(this) = rh;
		return *this;
	}
};

class ShaderReflector : public RHIUnknown
{
public:
	std::vector<IConstantBufferDesc>	mCBDescArray;
	std::vector<TextureBindInfo>		mTexBindInfoArray;
	std::vector<SamplerBindInfo>		mSamplerBindInfoArray;

	ShaderReflector();
	~ShaderReflector();

	UINT FindCBuffer(const char* name)
	{
		for (size_t i = 0; i < mCBDescArray.size(); i++)
		{
			if (mCBDescArray[i].Name == name)
				return (int)i;
		}
		return -1;
	}
	IConstantBufferDesc* GetCBuffer(UINT index)
	{
		if (index >= (UINT)mCBDescArray.size())
			return nullptr;
		return &mCBDescArray[index];
	}
	UINT FindSRV(const char* name)
	{
		for (size_t i = 0; i < mTexBindInfoArray.size(); i++)
		{
			if (mTexBindInfoArray[i].Name == name)
				return (int)i;
		}
		return -1;
	}
	TextureBindInfo* GetSRV(UINT index)
	{
		if (index >= (UINT)mTexBindInfoArray.size())
			return nullptr;
		return &mTexBindInfoArray[index];
	}
	UINT FindSampler(const char* name)
	{
		for (size_t i = 0; i < mSamplerBindInfoArray.size(); i++)
		{
			if (mSamplerBindInfoArray[i].Name == name)
				return (int)i;
		}
		return -1;
	}
	SamplerBindInfo* GetSampler(UINT index)
	{
		if (index >= (UINT)mSamplerBindInfoArray.size())
			return nullptr;
		return &mSamplerBindInfoArray[index];
	}
};

NS_END