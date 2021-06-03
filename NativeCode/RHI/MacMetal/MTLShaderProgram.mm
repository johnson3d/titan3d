#include "MTLShaderProgram.h"
#include "MTLVertexShader.h"
#include "MTLPixelShader.h"
#include "MTLInputLayout.h"
#include "../IConstantBuffer.h"
#include "MTLCommandList.h"

#define new VNEW

NS_BEGIN


	
		


	

EShaderVarType TanslateMtlShaderDataType2RHI(MTLDataType mtl_data_type)
{
	switch (mtl_data_type)
	{
	case MTLDataTypeStruct:
		return SVT_Struct;
		break;
	case MTLDataTypeFloat:
		return SVT_Float1;
		break;
	case MTLDataTypeFloat2:
		return SVT_Float2;
		break;
	case MTLDataTypeFloat3:
		return SVT_Float3;
		break;
	case MTLDataTypeFloat4:
		return SVT_Float4;
		break;
	case MTLDataTypeInt:
		return SVT_Int1;
		break;
	case MTLDataTypeInt2:
		return SVT_Int2;
		break;
	case MTLDataTypeInt3:
		return SVT_Int3;
		break;
	case MTLDataTypeInt4:
		return SVT_Int4;
		break;
	case MTLDataTypeFloat3x3:
		return SVT_Matrix3x3;
		break; 
	case MTLDataTypeFloat4x4:
		return SVT_Matrix4x4;
		break;

	case MTLDataTypeUInt:
	case MTLDataTypeUInt2:
	case	MTLDataTypeUInt3:
	case MTLDataTypeUInt4:
	case MTLDataTypeShort:
	case MTLDataTypeShort2:
	case MTLDataTypeShort3:
	case MTLDataTypeShort4:
	case MTLDataTypeUShort:
	case MTLDataTypeUShort2:
	case MTLDataTypeUShort3:
	case MTLDataTypeUShort4:
	case MTLDataTypeChar:
	case MTLDataTypeChar2:
	case MTLDataTypeChar3:
	case MTLDataTypeChar4:
	case MTLDataTypeUChar:
	case MTLDataTypeUChar2:
	case MTLDataTypeUChar3:
	case MTLDataTypeUChar4:
	case MTLDataTypeBool:
	case MTLDataTypeBool2:
	case MTLDataTypeBool3:
	case MTLDataTypeBool4:
	case MTLDataTypeHalf:
	case MTLDataTypeHalf2:
	case MTLDataTypeHalf3:
	case MTLDataTypeHalf4:
	case MTLDataTypeHalf2x2:
	case MTLDataTypeHalf2x3:
	case MTLDataTypeHalf2x4:
	case MTLDataTypeHalf3x2:
	case MTLDataTypeHalf3x3:
	case MTLDataTypeHalf3x4:
	case MTLDataTypeHalf4x2:
	case MTLDataTypeHalf4x3:
	case MTLDataTypeHalf4x4:
	case	MTLDataTypeNone:
	case MTLDataTypeArray:
	case MTLDataTypeFloat2x2:
	case	MTLDataTypeFloat2x3:
	case	MTLDataTypeFloat2x4:
	case	MTLDataTypeFloat3x2:
	case	MTLDataTypeFloat3x4:
	case	MTLDataTypeFloat4x2:
	case	MTLDataTypeFloat4x3:
		return SVT_Unknown;
		break;
	default:
		return SVT_Unknown;
		break;
	}
}


MtlGpuProgram::MtlGpuProgram()
{
}

MtlGpuProgram::~MtlGpuProgram()
{
}

void MtlGpuProgram::LinkShaders(IRenderContext* pCtx)
{
	//to generate const buffer,texture and sampler desc in this strange function...
	MTLRenderPipelineDescriptor* pPipelineDesc = [[MTLRenderPipelineDescriptor alloc] init];
	pPipelineDesc.vertexFunction = ((MtlVertexShader*)mVertexShader)->m_pVtxFunc;
	pPipelineDesc.fragmentFunction = ((MtlPixelShader*)mPixelShader)->m_pFragFunc;
	pPipelineDesc.vertexDescriptor =   ((MtlInputLayout*)mInputLayout)->m_pVtxDesc;
	pPipelineDesc.colorAttachments[0].pixelFormat = MTLPixelFormatBGRA8Unorm;

	NSError* pError = nil;
	MTLPipelineOption PipelineOption = MTLPipelineOptionBufferTypeInfo | MTLPipelineOptionArgumentInfo;
	MTLRenderPipelineReflection* pShaderReflectionInfo = nil;
	id<MTLRenderPipelineState> pTempPipelineState = [((MtlContext*)pCtx)->m_pDevice newRenderPipelineStateWithDescriptor : pPipelineDesc  options: PipelineOption 
		reflection: &pShaderReflectionInfo  error : &pError];

	if (pError != nil)
	{
		AssertRHI(false);
	}

	//vertex shader arguments;
	for (UINT32 idx = 0; idx < pShaderReflectionInfo.vertexArguments.count; idx++)
	{
		//const buffer;
		if (pShaderReflectionInfo.vertexArguments[idx].type == MTLArgumentTypeBuffer)
		{
			if ((UINT)pShaderReflectionInfo.vertexArguments[idx].index > 7 && (UINT)pShaderReflectionInfo.vertexArguments[idx].index < 24)
			{
				//this is stage in vtx buffer(0-15);
				continue;
			}

			IConstantBufferDesc ConstBufferDesc;
			ConstBufferDesc.Type = SIT_CBUFFER;
			ConstBufferDesc.Name = [pShaderReflectionInfo.vertexArguments[idx].name UTF8String];
			ConstBufferDesc.Size = (UINT)pShaderReflectionInfo.vertexArguments[idx].bufferDataSize;
			ConstBufferDesc.VSBindPoint = (UINT)pShaderReflectionInfo.vertexArguments[idx].index;
			ConstBufferDesc.BindCount = 1;
			if (pShaderReflectionInfo.vertexArguments[idx].bufferDataType == MTLDataTypeStruct)
			{
				for (UINT32 idx_mem = 0; idx_mem < pShaderReflectionInfo.vertexArguments[idx].bufferStructType.members.count; idx_mem++)
				{
					ConstantVarDesc ConstBufferMemVarDesc;
					ConstBufferMemVarDesc.Name = [pShaderReflectionInfo.vertexArguments[idx].bufferStructType.members[idx_mem].name UTF8String];
					if (pShaderReflectionInfo.vertexArguments[idx].bufferStructType.members[idx_mem].dataType == MTLDataTypeArray)
					{
						ConstBufferMemVarDesc.Type = TanslateMtlShaderDataType2RHI(pShaderReflectionInfo.vertexArguments[idx].bufferStructType.members[idx_mem].arrayType.elementType);
						ConstBufferMemVarDesc.Elements = UINT(pShaderReflectionInfo.vertexArguments[idx].bufferStructType.members[idx_mem].arrayType.arrayLength);
					}
					else
					{
						ConstBufferMemVarDesc.Type = TanslateMtlShaderDataType2RHI(pShaderReflectionInfo.vertexArguments[idx].bufferStructType.members[idx_mem].dataType);
						ConstBufferMemVarDesc.Elements = 1;
					}
					
					ConstBufferMemVarDesc.Offset = (UINT)pShaderReflectionInfo.vertexArguments[idx].bufferStructType.members[idx_mem].offset;
					UINT32 next_mem_idx = idx_mem + 1;
					if (next_mem_idx < pShaderReflectionInfo.vertexArguments[idx].bufferStructType.members.count)
					{
						ConstBufferMemVarDesc.Size = UINT(pShaderReflectionInfo.vertexArguments[idx].bufferStructType.members[next_mem_idx].offset - 
							pShaderReflectionInfo.vertexArguments[idx].bufferStructType.members[idx_mem].offset);
					}
					else
					{
						ConstBufferMemVarDesc.Size = UINT(pShaderReflectionInfo.vertexArguments[idx].bufferDataSize - pShaderReflectionInfo.vertexArguments[idx].bufferStructType.
							members[idx_mem].offset);
					}
					
					ConstBufferDesc.Vars.push_back(ConstBufferMemVarDesc);
				}
			}
			mConstBufferDescArray.push_back(ConstBufferDesc);
		}

		//texture;
		if (pShaderReflectionInfo.vertexArguments[idx].type == MTLArgumentTypeTexture)
		{
			TextureBindInfo TexBindInfoInst;
			TexBindInfoInst.Name = [pShaderReflectionInfo.vertexArguments[idx].name UTF8String];
			TexBindInfoInst.VSBindPoint = (UINT)pShaderReflectionInfo.vertexArguments[idx].index;
			TexBindInfoInst.BindCount = 1;
			mTexBindInfoArray.push_back(TexBindInfoInst);
		}

		//sampler;
		if (pShaderReflectionInfo.vertexArguments[idx].type == MTLArgumentTypeSampler)
		{
			SamplerBindInfo SampBindInfoInst;
			SampBindInfoInst.Name = [pShaderReflectionInfo.vertexArguments[idx].name UTF8String];
			SampBindInfoInst.VSBindPoint = (UINT)pShaderReflectionInfo.vertexArguments[idx].index;
			mSampBindInfoArray.push_back(SampBindInfoInst);
		}
	}

	//fragment shader arguments;
	for (UINT32 idx = 0; idx < pShaderReflectionInfo.fragmentArguments.count; idx++)
	{
		//const buffer;
		if (pShaderReflectionInfo.fragmentArguments[idx].type == MTLArgumentTypeBuffer)
		{
			bool ExistInVS = false;
			IConstantBufferDesc ConstBufferDesc;
			ConstBufferDesc.Name = [pShaderReflectionInfo.fragmentArguments[idx].name UTF8String];
			for (UINT32 j = 0; j < mConstBufferDescArray.size(); j++)
			{
				if (mConstBufferDescArray[j].Name == ConstBufferDesc.Name)
				{
					ExistInVS = true;
					mConstBufferDescArray[j].PSBindPoint = (UINT)pShaderReflectionInfo.fragmentArguments[idx].index;
					break;
				}
			}

			if (ExistInVS == false)
			{
				ConstBufferDesc.Type = SIT_CBUFFER;
				ConstBufferDesc.Size = (UINT)pShaderReflectionInfo.fragmentArguments[idx].bufferDataSize;
				ConstBufferDesc.PSBindPoint = (UINT)pShaderReflectionInfo.fragmentArguments[idx].index;
				ConstBufferDesc.BindCount = 1;
				if (pShaderReflectionInfo.fragmentArguments[idx].bufferDataType == MTLDataTypeStruct)
				{
					for (UINT32 idx_mem = 0; idx_mem < pShaderReflectionInfo.fragmentArguments[idx].bufferStructType.members.count; idx_mem++)
					{
						ConstantVarDesc ConstBufferMemVarDesc;
						ConstBufferMemVarDesc.Name = [pShaderReflectionInfo.fragmentArguments[idx].bufferStructType.members[idx_mem].name UTF8String];
						if (pShaderReflectionInfo.fragmentArguments[idx].bufferStructType.members[idx_mem].dataType == MTLDataTypeArray)
						{
							ConstBufferMemVarDesc.Type = TanslateMtlShaderDataType2RHI(pShaderReflectionInfo.fragmentArguments[idx].bufferStructType.members[idx_mem].arrayType.elementType);
							ConstBufferMemVarDesc.Elements = UINT(pShaderReflectionInfo.fragmentArguments[idx].bufferStructType.members[idx_mem].arrayType.arrayLength);
						}
						else
						{
							ConstBufferMemVarDesc.Type = TanslateMtlShaderDataType2RHI(pShaderReflectionInfo.fragmentArguments[idx].bufferStructType.members[idx_mem].dataType);
							ConstBufferMemVarDesc.Elements = 1;
						}

						ConstBufferMemVarDesc.Offset = (UINT)pShaderReflectionInfo.fragmentArguments[idx].bufferStructType.members[idx_mem].offset;
						UINT32 next_mem_idx = idx_mem + 1;
						if (next_mem_idx < pShaderReflectionInfo.fragmentArguments[idx].bufferStructType.members.count)
						{
							ConstBufferMemVarDesc.Size = UINT(pShaderReflectionInfo.fragmentArguments[idx].bufferStructType.members[next_mem_idx].offset -
								pShaderReflectionInfo.fragmentArguments[idx].bufferStructType.members[idx_mem].offset);
						}
						else
						{
							ConstBufferMemVarDesc.Size = UINT(pShaderReflectionInfo.fragmentArguments[idx].bufferDataSize - pShaderReflectionInfo.fragmentArguments[idx].bufferStructType.
								members[idx_mem].offset);
						}

						ConstBufferDesc.Vars.push_back(ConstBufferMemVarDesc);
					}
				}
				mConstBufferDescArray.push_back(ConstBufferDesc);
			}
		}

		//texture;
		if (pShaderReflectionInfo.fragmentArguments[idx].type == MTLArgumentTypeTexture)
		{
			bool ExistInVS = false;
			TextureBindInfo TexBindInfoInst;
			TexBindInfoInst.Name = [pShaderReflectionInfo.fragmentArguments[idx].name UTF8String];
			for (UINT32 j = 0; j < mTexBindInfoArray.size(); j++)
			{
				if (mTexBindInfoArray[j].Name == TexBindInfoInst.Name)
				{
					ExistInVS = true;
					mTexBindInfoArray[j].PSBindPoint = (UINT)pShaderReflectionInfo.fragmentArguments[idx].index;
					break;
				}
			}
			
			if (ExistInVS == false)
			{
				TexBindInfoInst.PSBindPoint = (UINT)pShaderReflectionInfo.fragmentArguments[idx].index;
				TexBindInfoInst.BindCount = 1;
				mTexBindInfoArray.push_back(TexBindInfoInst);
			}
		}

		//sampler;
		if (pShaderReflectionInfo.fragmentArguments[idx].type == MTLArgumentTypeSampler)
		{
			bool ExistInVS = false;
			SamplerBindInfo SampBindInfoInst;
			SampBindInfoInst.Name = [pShaderReflectionInfo.fragmentArguments[idx].name UTF8String];
			for (UINT32 j = 0; j < mSampBindInfoArray.size(); j++)
			{
				if (SampBindInfoInst.Name == mSampBindInfoArray[j].Name)
				{
					ExistInVS = true;
					mSampBindInfoArray[j].PSBindPoint = (UINT)pShaderReflectionInfo.fragmentArguments[idx].index;
					break;
				}
			}
			if (ExistInVS == false)
			{
				SampBindInfoInst.PSBindPoint = (UINT)pShaderReflectionInfo.fragmentArguments[idx].index;
				SampBindInfoInst.BindCount = 1;
				mSampBindInfoArray.push_back(SampBindInfoInst);
			}
		}
	}
	[pPipelineDesc release];
	pPipelineDesc = nil;
}

void MtlGpuProgram::ApplyShaders(ICommandList* pCmdList)
{
	MtlCmdList* refCmdList = (MtlCmdList*)pCmdList;
	refCmdList->m_pMtlRenderPipelineDesc.vertexDescriptor = ((MtlInputLayout*)mInputLayout)->m_pVtxDesc;
	refCmdList->m_pMtlRenderPipelineDesc.vertexFunction = ((MtlVertexShader*)mVertexShader)->m_pVtxFunc;
	refCmdList->m_pMtlRenderPipelineDesc.fragmentFunction = ((MtlPixelShader*)mPixelShader)->m_pFragFunc;
}

UINT MtlGpuProgram::FindCBuffer(const char* pName)
{
	for (UINT idx = 0; idx < mConstBufferDescArray.size(); idx++)
	{
		if (mConstBufferDescArray[idx].Name == (std::string)pName)
		{
			return idx;
		}
	}
	return -1;
}

UINT MtlGpuProgram::GetCBufferNumber()
{
	return (UINT)mConstBufferDescArray.size();
}

IConstantBufferDesc* MtlGpuProgram::GetCBuffer(UINT index)
{
	if (index >= (UINT)mConstBufferDescArray.size())
	{
		return nullptr;
	}
	return &mConstBufferDescArray[index];
}

UINT MtlGpuProgram::GetShaderResourceNumber() const
{
	return (UINT)mTexBindInfoArray.size();
}

bool MtlGpuProgram::GetShaderResourceBindInfo(UINT index, TextureBindInfo* pInfo) const
{
	if (index >= (UINT)mTexBindInfoArray.size())
	{
		return false;
	}
	*pInfo = mTexBindInfoArray[index];
	return true;
}

UINT MtlGpuProgram::GetTextureBindSlotIndex(const char* pName)
{
	for (UINT idx = 0; idx < mTexBindInfoArray.size(); idx++)
	{
		if (mTexBindInfoArray[idx].Name == (std::string)pName)
		{
			return idx;
		}
	}
	return -1;
}

UINT MtlGpuProgram::GetSamplerNumber() const
{
	return (UINT)mSampBindInfoArray.size();
}

bool MtlGpuProgram::GetSamplerBindInfo(UINT index, SamplerBindInfo* pInfo) const
{
	if (index >= (UINT)mSampBindInfoArray.size())
	{
		return false;
	}
	*pInfo = mSampBindInfoArray[index];
	return true;
}

UINT MtlGpuProgram::GetSamplerBindSlotIndex(const char* pName)
{
	for (UINT idx = 0; idx < mSampBindInfoArray.size(); idx++)
	{
		if (mSampBindInfoArray[idx].Name == (std::string)pName)
		{
			return idx;
		}
	}
	return -1;
}


bool MtlGpuProgram::Init(MtlContext* pCtx, const IShaderProgramDesc* desc)
{
	return true;
}

NS_END