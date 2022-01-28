#include "IConstantBuffer.h"
#include "ITextureBase.h"
#include "IRenderContext.h"
#include "IShaderProgram.h"
#include "ShaderReflector.h"
#include "ICommandList.h"
#include "../Base/xnd/vfxxnd.h"

#define new VNEW

NS_BEGIN

void IConstantBufferDesc::Save2Xnd(XndAttribute* attr)
{
	attr->Write(Size);
	attr->Write(VSBindPoint);
	attr->Write(PSBindPoint);
	attr->Write(CSBindPoint);
	attr->Write(BindCount);
	attr->Write(Name);
	attr->Write((UINT)CBufferLayout->Vars.size());
	for (size_t i = 0; i < CBufferLayout->Vars.size(); i++)
	{
		ConstantVarDesc* var = &CBufferLayout->Vars[i];
		attr->Write(var->Type);
		attr->Write(var->Offset);
		attr->Write(var->Size);
		attr->Write(var->Elements);
		attr->Write(var->Name);
	}
}
void IConstantBufferDesc::LoadXnd(XndAttribute* attr)
{
	attr->Read(Size);
	attr->Read(VSBindPoint);
	attr->Read(PSBindPoint);
	attr->Read(CSBindPoint);
	attr->Read(BindCount);
	attr->Read(Name);
	UINT count;
	attr->Read(count);
	CBufferLayout->Vars.resize(count);
	for (size_t i = 0; i < CBufferLayout->Vars.size(); i++)
	{
		ConstantVarDesc* var = &CBufferLayout->Vars[i];
		attr->Read(var->Type);
		attr->Read(var->Offset);
		attr->Read(var->Size);
		attr->Read(var->Elements);
		attr->Read(var->Name);
	}
}

IConstantBuffer::IConstantBuffer()
{
	mDirty = false;
	mHasPushed = false;
}

IConstantBuffer::~IConstantBuffer()
{
	
}

vBOOL IConstantBuffer::IsSameVars(IShaderProgram* program, UINT cbIndex)
{
	auto cbDesc = program->GetReflector()->GetCBuffer(cbIndex);
	if (cbDesc == nullptr)
		return FALSE;
	return Desc.IsSameVars(cbDesc);
}

void IConstantBuffer::UpdateDrawPass(ICommandList* cmd, vBOOL bImm)
{
	if (mDirty == false)
		return;
	if (VarBuffer.size() == 0)
		return;
	if (bImm)
	{
		FlushContent(cmd);
		mDirty = false;
	}
	else
	{
		if (mHasPushed)
			return;
		mHasPushed = true;
		cmd->GetContext()->PushFrameResource(this);
	}
}

void IConstantBuffer::OnFrameEnd(IRenderContext* rc)
{
	if (FlushContent2(rc)!=FALSE)
	{
		mDirty = false;
		mHasPushed = false;
	}
}

vBOOL IConstantBuffer::FlushContent2(IRenderContext* ctx)
{
	if (mDirty == false)
		return TRUE;
	if (VarBuffer.size() == 0)
		return TRUE;
	return UpdateContent(ctx->GetImmCommandList(), &VarBuffer[0], (UINT)VarBuffer.size()) ? TRUE : FALSE;
}


NS_END
