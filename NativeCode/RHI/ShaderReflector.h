#pragma once
#include "IConstantBuffer.h"

NS_BEGIN

struct TR_CLASS(SV_LayoutStruct = 8)
	ShaderRViewBindInfo : public IShaderBinder
{
	ShaderRViewBindInfo()
	{
		SetDefault();
	}
	void SetDefault()
	{
		IShaderBinder::SetDefault();
		BindType = EShaderBindType::SBT_Srv;
		BufferType = EGpuBufferType::GBT_Unknown;
	}
};

struct TR_CLASS(SV_LayoutStruct = 8)
	SamplerBindInfo : public IShaderBinder
{
	SamplerBindInfo()
	{
		SetDefault();
	}
	void SetDefault()
	{
		IShaderBinder::SetDefault();
		BindType = EShaderBindType::SBT_Sampler;
		BufferType = EGpuBufferType::GBT_Unknown;
	}
};

struct TR_CLASS(SV_LayoutStruct = 8)
	UavBindInfo : public IShaderBinder
{
	UavBindInfo()
	{
		SetDefault();
	}
	void SetDefault()
	{
		IShaderBinder::SetDefault();
		BindType = EShaderBindType::SBT_Uav;
		BufferType = EGpuBufferType::GBT_UavBuffer;
	}
};

class TR_CLASS()
	ShaderReflector : public VIUnknown
{
public:
	std::vector<IConstantBufferDesc>	mCBDescArray;
	std::vector<UavBindInfo>			mUavBindArray;
	std::vector<ShaderRViewBindInfo>	mSrvBindArray;
	std::vector<SamplerBindInfo>		mSamplerBindArray;

	ShaderReflector();
	~ShaderReflector();

	void Reset()
	{
		mCBDescArray.clear();
		mUavBindArray.clear();
		mSrvBindArray.clear();
		mSamplerBindArray.clear();
	}
#if defined(USE_D11)
	TR_MEMBER(SV_NoBind=true)
	bool ReflectDXBC(const IShaderDesc* desc);
#endif

#if defined(USE_VK)
	TR_MEMBER(SV_NoBind = true)
	bool ReflectSpirV(const IShaderDesc * desc);
#endif
	void MergeShaderStage(EShaderType type, ShaderReflector* reflector);

	UINT FindShaderBinder(EShaderBindType type, const char* name) const
	{
		switch (type)
		{
		case EngineNS::SBT_CBuffer:
			for (size_t i = 0; i < mCBDescArray.size(); i++)
			{
				if (mCBDescArray[i].Name == name)
					return (int)i;
			}
			break;
		case EngineNS::SBT_Uav:
			for (size_t i = 0; i < mUavBindArray.size(); i++)
			{
				if (mUavBindArray[i].Name == name)
					return (int)i;
			}
			break;
		case EngineNS::SBT_Srv:
			for (size_t i = 0; i < mSrvBindArray.size(); i++)
			{
				if (mSrvBindArray[i].Name == name)
					return (int)i;
			}
			break;
		case EngineNS::SBT_Sampler:
			for (size_t i = 0; i < mSamplerBindArray.size(); i++)
			{
				if (mSamplerBindArray[i].Name == name)
					return (int)i;
			}
			break;
		default:
			break;
		}

		return -1;
	}
	UINT FindShaderBinderBySlot(EShaderBindType type, EShaderType shaderType, UINT slot) const
	{
		switch (type)
		{
		case EngineNS::SBT_CBuffer:
			for (size_t i = 0; i < mCBDescArray.size(); i++)
			{
				switch (shaderType)
				{
				case EngineNS::EST_UnknownShader:
					break;
				case EngineNS::EST_VertexShader:
					{
						if (mCBDescArray[i].VSBindPoint == slot)
							return (int)i;
					}
					break;
				case EngineNS::EST_PixelShader:
					{
						if (mCBDescArray[i].PSBindPoint == slot)
							return (int)i;
					}
					break;
				case EngineNS::EST_ComputeShader:
					{
						if (mCBDescArray[i].CSBindPoint == slot)
							return (int)i;
					}
					break;
				default:
					break;
				}
			}
			break;
		case EngineNS::SBT_Uav:
			for (size_t i = 0; i < mUavBindArray.size(); i++)
			{
				switch (shaderType)
				{
					case EngineNS::EST_UnknownShader:
						break;
					case EngineNS::EST_VertexShader:
					{
						if (mUavBindArray[i].VSBindPoint == slot)
							return (int)i;
					}
					break;
					case EngineNS::EST_PixelShader:
					{
						if (mUavBindArray[i].PSBindPoint == slot)
							return (int)i;
					}
					break;
					case EngineNS::EST_ComputeShader:
					{
						if (mUavBindArray[i].CSBindPoint == slot)
							return (int)i;
					}
					break;
					default:
						break;
				}
			}
			break;
		case EngineNS::SBT_Srv:
			for (size_t i = 0; i < mSrvBindArray.size(); i++)
			{
				switch (shaderType)
				{
					case EngineNS::EST_UnknownShader:
						break;
					case EngineNS::EST_VertexShader:
					{
						if (mSrvBindArray[i].VSBindPoint == slot)
							return (int)i;
					}
					break;
					case EngineNS::EST_PixelShader:
					{
						if (mSrvBindArray[i].PSBindPoint == slot)
							return (int)i;
					}
					break;
					case EngineNS::EST_ComputeShader:
					{
						if (mSrvBindArray[i].CSBindPoint == slot)
							return (int)i;
					}
					break;
					default:
						break;
				}
			}
			break;
		case EngineNS::SBT_Sampler:
			for (size_t i = 0; i < mSamplerBindArray.size(); i++)
			{
				switch (shaderType)
				{
					case EngineNS::EST_UnknownShader:
						break;
					case EngineNS::EST_VertexShader:
					{
						if (mSamplerBindArray[i].VSBindPoint == slot)
							return (int)i;
					}
					break;
					case EngineNS::EST_PixelShader:
					{
						if (mSamplerBindArray[i].PSBindPoint == slot)
							return (int)i;
					}
					break;
					case EngineNS::EST_ComputeShader:
					{
						if (mSamplerBindArray[i].CSBindPoint == slot)
							return (int)i;
					}
					break;
					default:
						break;
				}
			}
			break;
		default:
			break;
		}

		return -1;
	}
	UINT FindOrNewShaderBinder(EShaderBindType type, const char* name, bool& isNew)
	{
		isNew = false;
		switch (type)
		{
		case EngineNS::SBT_CBuffer:
			{
				for (size_t i = 0; i < mCBDescArray.size(); i++)
				{
					if (mCBDescArray[i].Name == name)
						return (int)i;
				}
				isNew = true;
				IConstantBufferDesc tmp;
				tmp.Name = name;
				tmp.CBufferLayout = MakeWeakRef(new IConstantBufferLayout());
				mCBDescArray.push_back(tmp);
				return (UINT)(mCBDescArray.size() - 1);
			}
		case EngineNS::SBT_Uav:
			{
				for (size_t i = 0; i < mUavBindArray.size(); i++)
				{
					if (mUavBindArray[i].Name == name)
						return (int)i;
				}
				isNew = true;
				UavBindInfo tmp;
				tmp.Name = name;
				mUavBindArray.push_back(tmp);
				return (UINT)(mUavBindArray.size() - 1);
			}
		case EngineNS::SBT_Srv:
			{
				for (size_t i = 0; i < mSrvBindArray.size(); i++)
				{
					if (mSrvBindArray[i].Name == name)
						return (int)i;
				}
				isNew = true;
				ShaderRViewBindInfo tmp;
				tmp.Name = name;
				mSrvBindArray.push_back(tmp);
				return (UINT)(mSrvBindArray.size() - 1);
			}
		case EngineNS::SBT_Sampler:
			{
				for (size_t i = 0; i < mSamplerBindArray.size(); i++)
				{
					if (mSamplerBindArray[i].Name == name)
						return (int)i;
				}
				isNew = true;
				SamplerBindInfo tmp;
				tmp.Name = name;
				mSamplerBindArray.push_back(tmp);
				return (UINT)(mSamplerBindArray.size() - 1);
			}
		default:
			return -1;
		}
	}
	const IShaderBinder* GetShaderBinder(EShaderBindType type, UINT index) const
	{
		switch (type)
		{
		case EngineNS::SBT_CBuffer:
			if (index >= (UINT)mCBDescArray.size())
				return nullptr;
			return &mCBDescArray[index];
		case EngineNS::SBT_Uav:
			if (index >= (UINT)mUavBindArray.size())
				return nullptr;
			return &mUavBindArray[index];
		case EngineNS::SBT_Srv:
			if (index >= (UINT)mSrvBindArray.size())
				return nullptr;
			return &mSrvBindArray[index];
		case EngineNS::SBT_Sampler:
			if (index >= (UINT)mSamplerBindArray.size())
				return nullptr;
			return &mSamplerBindArray[index];
		default:
			break;
		}

		return nullptr;
	}
	const IConstantBufferDesc* GetCBuffer(UINT index) const
	{
		if (index >= (UINT)mCBDescArray.size())
			return nullptr;
		return &mCBDescArray[index];
	}
	const IShaderBinder* GetShaderBinder(EShaderBindType type, const char* name) const
	{
		auto index = FindShaderBinder(type, name);
		if (index == -1)
		{
			//ASSERT(false);
			//VFX_LTRACE(ELTT_Graphics, "FindShaderBinder failed:(%s)\n", name);
			return nullptr;
		}
		return GetShaderBinder(type, index);
	}
};

NS_END