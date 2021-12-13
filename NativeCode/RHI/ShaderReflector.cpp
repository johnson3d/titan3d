#include "ShaderReflector.h"

#define new VNEW

NS_BEGIN

ShaderReflector::ShaderReflector()
{
}


ShaderReflector::~ShaderReflector()
{
}

void ShaderReflector::MergeShaderStage(EShaderType type, ShaderReflector* reflector)
{
	for (auto& i : reflector->mCBDescArray)
	{
		bool isNew = false;
		auto index = FindOrNewShaderBinder(SBT_CBuffer, i.Name, isNew);
		auto pDesc = (IConstantBufferDesc*)GetShaderBinder(SBT_CBuffer, index);
		if (isNew)
		{
			*pDesc = i;
		}
		else
		{
			ASSERT(pDesc->IsSameVars(&i));
			switch (type)
			{
			case EngineNS::EST_UnknownShader:
				break;
			case EngineNS::EST_VertexShader:
				pDesc->VSBindPoint = i.VSBindPoint;
				break;
			case EngineNS::EST_PixelShader:
				pDesc->PSBindPoint = i.PSBindPoint;
				break;
			case EngineNS::EST_ComputeShader:
				pDesc->CSBindPoint = i.CSBindPoint;
				break;
			default:
				break;
			}
		}
	}
	for (auto& i : reflector->mUavBindArray)
	{
		bool isNew = false;
		auto index = FindOrNewShaderBinder(SBT_Uav, i.Name, isNew);
		auto pDesc = (UavBindInfo*)GetShaderBinder(SBT_Uav, index);
		if (isNew)
		{
			*pDesc = i;
		}
		else
		{
			switch (type)
			{
			case EngineNS::EST_UnknownShader:
				break;
			case EngineNS::EST_VertexShader:
				pDesc->VSBindPoint = i.VSBindPoint;
				break;
			case EngineNS::EST_PixelShader:
				pDesc->PSBindPoint = i.PSBindPoint;
				break;
			case EngineNS::EST_ComputeShader:
				pDesc->CSBindPoint = i.CSBindPoint;
				break;
			default:
				break;
			}
		}
	}
	for (auto& i : reflector->mSrvBindArray)
	{
		bool isNew = false;
		auto index = FindOrNewShaderBinder(SBT_Srv, i.Name, isNew);
		auto pDesc = (ShaderRViewBindInfo*)GetShaderBinder(SBT_Srv, index);
		if (isNew)
		{
			*pDesc = i;
		}
		else
		{
			switch (type)
			{
			case EngineNS::EST_UnknownShader:
				break;
			case EngineNS::EST_VertexShader:
				pDesc->VSBindPoint = i.VSBindPoint;
				break;
			case EngineNS::EST_PixelShader:
				pDesc->PSBindPoint = i.PSBindPoint;
				break;
			case EngineNS::EST_ComputeShader:
				pDesc->CSBindPoint = i.CSBindPoint;
				break;
			default:
				break;
			}
		}
	}
	for (auto& i : reflector->mSamplerBindArray)
	{
		bool isNew = false;
		auto index = FindOrNewShaderBinder(SBT_Sampler, i.Name, isNew);
		auto pDesc = (SamplerBindInfo*)GetShaderBinder(SBT_Sampler, index);
		if (isNew)
		{
			*pDesc = i;
		}
		else
		{
			switch (type)
			{
			case EngineNS::EST_UnknownShader:
				break;
			case EngineNS::EST_VertexShader:
				pDesc->VSBindPoint = i.VSBindPoint;
				break;
			case EngineNS::EST_PixelShader:
				pDesc->PSBindPoint = i.PSBindPoint;
				break;
			case EngineNS::EST_ComputeShader:
				pDesc->CSBindPoint = i.CSBindPoint;
				break;
			default:
				break;
			}
		}
	}
}

NS_END