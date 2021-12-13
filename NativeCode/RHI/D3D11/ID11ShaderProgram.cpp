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
	mReflector->Reset();
	mReflector->MergeShaderStage(EST_VertexShader, mVertexShader->GetReflector());
	mReflector->MergeShaderStage(EST_PixelShader, mPixelShader->GetReflector());

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

bool ID11ShaderProgram::Init(ID11RenderContext* rc, const IShaderProgramDesc* desc)
{
	BindInputLayout(desc->InputLayout);
	BindVertexShader(desc->VertexShader);
	BindPixelShader(desc->PixelShader);
	return true;
}

NS_END