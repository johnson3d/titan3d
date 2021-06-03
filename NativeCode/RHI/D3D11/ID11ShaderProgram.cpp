#include "ID11ShaderProgram.h"
#include "ID11VertexShader.h"
#include "ID11PixelShader.h"
#include "ID11InputLayout.h"
#include "ID11CommandList.h"
#include "../Utility/GraphicsProfiler.h"

#define new VNEW

NS_BEGIN

ID11ShaderProgram::ID11ShaderProgram()
{
	
}

ID11ShaderProgram::~ID11ShaderProgram()
{
	
}

vBOOL ID11ShaderProgram::LinkShaders(IRenderContext* rc)
{
	for (UINT i = 0; i < mVertexShader->GetConstantBufferNumber(); i++)
	{
		IConstantBufferDesc desc;
		mVertexShader->GetConstantBufferDesc(i, &desc);
		auto index = FindCBuffer(desc.Name.c_str());
		if (index == -1)
		{
			IConstantBufferDesc d11Desc;
			d11Desc = desc;
			d11Desc.VSBindPoint = desc.VSBindPoint;
			mCBuffers.push_back(d11Desc);
		}
		else
		{
			auto pDesc = GetCBuffer(index);
			pDesc->VSBindPoint = desc.VSBindPoint;
		}
	}
	for (UINT i = 0; i < mPixelShader->GetConstantBufferNumber(); i++)
	{
		IConstantBufferDesc desc;
		mPixelShader->GetConstantBufferDesc(i, &desc);
		auto index = FindCBuffer(desc.Name.c_str());
		if (index == -1)
		{
			IConstantBufferDesc d11Desc;
			d11Desc = desc;
			d11Desc.PSBindPoint = desc.PSBindPoint;
			mCBuffers.push_back(d11Desc);
		}
		else
		{
			auto pDesc = GetCBuffer(index);
			pDesc->PSBindPoint = desc.PSBindPoint;
		}
	}

	for (UINT i = 0; i < mVertexShader->GetShaderResourceNumber(); i++)
	{
		TSBindInfo desc;
		mVertexShader->GetShaderResourceBindInfo(i, &desc);
		auto index = GetTextureBindSlotIndex(desc.Name.c_str());
		if (index == -1)
		{
			TSBindInfo d11Desc;
			d11Desc = desc;
			d11Desc.VSBindPoint = desc.VSBindPoint;
			mTextures.push_back(d11Desc);
		}
		else
		{
			mTextures[index].VSBindPoint = desc.VSBindPoint;
		}
	}
	for (UINT i = 0; i < mPixelShader->GetShaderResourceNumber(); i++)
	{
		TSBindInfo desc;
		mPixelShader->GetShaderResourceBindInfo(i, &desc);
		int index = GetTextureBindSlotIndex(desc.Name.c_str());
		if (index == -1)
		{
			TSBindInfo d11Desc;
			d11Desc = desc;
			d11Desc.PSBindPoint = desc.PSBindPoint;
			mTextures.push_back(d11Desc);
		}
		else
		{
			mTextures[index].PSBindPoint = desc.PSBindPoint;
		}
	}

	for (UINT i = 0; i < mVertexShader->GetSamplerNumber(); i++)
	{
		TSBindInfo desc;
		mVertexShader->GetSamplerBindInfo(i, &desc);
		auto index = GetSamplerBindSlotIndex(desc.Name.c_str());
		if (index == -1)
		{
			TSBindInfo d11Desc;
			d11Desc = desc;
			d11Desc.VSBindPoint = desc.VSBindPoint;
			mSamplers.push_back(d11Desc);
		}
		else
		{
			mSamplers[index].VSBindPoint = desc.VSBindPoint;
		}
	}
	for (UINT i = 0; i < mPixelShader->GetSamplerNumber(); i++)
	{
		TSBindInfo desc;
		mPixelShader->GetSamplerBindInfo(i, &desc);
		auto index = GetSamplerBindSlotIndex(desc.Name.c_str());
		if (index == -1)
		{
			TSBindInfo d11Desc;
			d11Desc = desc;
			d11Desc.PSBindPoint = desc.PSBindPoint;
			mSamplers.push_back(d11Desc);
		}
		else
		{
			mSamplers[index].PSBindPoint = desc.PSBindPoint;
		}
	}

	return TRUE;
}

void ID11ShaderProgram::ApplyShaders(ICommandList* cmd)
{
	/*if (cmd->IsDoing() == false)
		return;*/
	auto d11Cmd = (ID11CommandList*)cmd;

	if (cmd->mCurrentState.TrySet_InputLayout(mInputLayout))
	{
		d11Cmd->mDeferredContext->IASetInputLayout((mInputLayout.UnsafeConvertTo<ID11InputLayout>())->GetInnerLayout());
	}
	if (cmd->mCurrentState.TrySet_VertexShader(mVertexShader))
	{
		d11Cmd->mDeferredContext->VSSetShader(mVertexShader.UnsafeConvertTo<ID11VertexShader>()->mShader, nullptr, 0);
	}
	if (cmd->mProfiler != nullptr && cmd->mProfiler->mNoPixelShader)
	{
		IPixelShader* ps = cmd->mProfiler->mPSEmpty;
		if (cmd->mCurrentState.TrySet_PixelShader(ps))
		{
			d11Cmd->mDeferredContext->PSSetShader(((ID11PixelShader*)ps)->mShader, nullptr, 0);
		}
	}
	else
	{
		if (cmd->mCurrentState.TrySet_PixelShader(mPixelShader))
		{
			d11Cmd->mDeferredContext->PSSetShader((mPixelShader.UnsafeConvertTo<ID11PixelShader>())->mShader, nullptr, 0);
		}
	}
}

UINT ID11ShaderProgram::FindCBuffer(const char* name)
{
	for (size_t i = 0; i < mCBuffers.size(); i++)
	{
		if (mCBuffers[i].Name == name)
			return (int)i;
	}
	return -1;
}

UINT ID11ShaderProgram::GetCBufferNumber()
{
	return (UINT)mCBuffers.size();
}

IConstantBufferDesc* ID11ShaderProgram::GetCBuffer(UINT index)
{
	if (index >= (UINT)mCBuffers.size())
		return nullptr;
	return &mCBuffers[index];
}

UINT ID11ShaderProgram::GetShaderResourceNumber() const
{
	return (UINT)mTextures.size();
}

bool ID11ShaderProgram::GetShaderResourceBindInfo(UINT Index, TSBindInfo* info, int dataSize) const
{
	if (Index >= (UINT)mTextures.size())
		return false;
	if (dataSize >= sizeof(TSBindInfo))
		*info = mTextures[Index];
	else
		memcpy(info, &mTextures[Index], dataSize);
	return true;
}

UINT ID11ShaderProgram::GetTextureBindSlotIndex(const char* name)
{
	for (size_t i = 0; i < mTextures.size(); i++)
	{
		if (mTextures[i].Name == name)
			return (int)i;
	}
	return -1;
}

UINT ID11ShaderProgram::GetSamplerNumber() const
{
	return (UINT)mSamplers.size();
}

bool ID11ShaderProgram::GetSamplerBindInfo(UINT Index, TSBindInfo* info, int dataSize) const
{
	if (Index >= (UINT)mSamplers.size())
		return false;
	if (dataSize >= sizeof(TSBindInfo))
		*info = mSamplers[Index];
	else
		memcpy(info, &mSamplers[Index], dataSize);
	return true;
}
UINT ID11ShaderProgram::GetSamplerBindSlotIndex(const char* name)
{
	for (size_t i = 0; i < mSamplers.size(); i++)
	{
		if (mSamplers[i].Name == name)
			return (int)i;
	}
	return -1;
}


bool ID11ShaderProgram::Init(ID11RenderContext* rc, const IShaderProgramDesc* desc)
{
	BindInputLayout(desc->InputLayout);
	BindVertexShader(desc->VertexShader);
	BindPixelShader(desc->PixelShader);
	return true;
}

NS_END