#include "IConstantBuffer.h"
#include "ITextureBase.h"
#include "IRenderContext.h"
#include "IShaderProgram.h"
#include "../Core/xnd/vfxxnd.h"

#define new VNEW

NS_BEGIN

StructImpl(ConstantVarDesc);

vBOOL IConstantBufferDesc::IsSameVars(const IConstantBufferDesc* desc)
{
	if (Vars.size() != desc->Vars.size())
		return FALSE;

	for (size_t i = 0; i < Vars.size(); i++)
	{
		if (Vars[i].Type != desc->Vars[i].Type)
			return FALSE;
		if (Vars[i].Offset != desc->Vars[i].Offset)
			return FALSE;
		if (Vars[i].Elements != desc->Vars[i].Elements)
			return FALSE;
		if (Vars[i].Name != desc->Vars[i].Name)
			return FALSE;
	}

	return TRUE;
}

void IConstantBufferDesc::Save2Xnd(XNDAttrib* attr)
{
	attr->Write(Size);
	attr->Write(VSBindPoint);
	attr->Write(PSBindPoint);
	attr->Write(CSBindPoint);
	attr->Write(BindCount);
	attr->WriteText(Name.c_str());
	attr->Write((UINT)Vars.size());
	for (size_t i = 0; i < Vars.size(); i++)
	{
		ConstantVarDesc* var = &Vars[i];
		attr->Write(var->Type);
		attr->Write(var->Offset);
		attr->Write(var->Size);
		attr->Write(var->Elements);
		attr->WriteText(var->Name.c_str());
	}
}
void IConstantBufferDesc::LoadXnd(XNDAttrib* attr)
{
	attr->Read(Size);
	attr->Read(VSBindPoint);
	attr->Read(PSBindPoint);
	attr->Read(CSBindPoint);
	attr->Read(BindCount);
	attr->ReadText(Name);
	UINT count;
	attr->Read(count);
	Vars.resize(count);
	for (size_t i = 0; i < Vars.size(); i++)
	{
		ConstantVarDesc* var = &Vars[i];
		attr->Read(var->Type);
		attr->Read(var->Offset);
		attr->Read(var->Size);
		attr->Read(var->Elements);
		attr->ReadText(var->Name);
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
	auto cbDesc = program->GetCBuffer(cbIndex);
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
		RResourceSwapChain::GetInstance()->PushResource(this);
	}
}

void IConstantBuffer::DoSwap(IRenderContext* rc)
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

using namespace EngineNS;

extern "C"
{
	CSharpReturnAPI0(const char*, EngineNS, IConstantBuffer, GetName);
	CSharpReturnAPI0(UINT, EngineNS, IConstantBuffer, GetSize);
	CSharpReturnAPI2(vBOOL, EngineNS, IConstantBuffer, IsSameVars, IShaderProgram*, UINT);
	CSharpReturnAPI1(int, EngineNS, IConstantBuffer, FindVar, const char*);
	CSharpReturnAPI2(vBOOL, EngineNS, IConstantBuffer, GetVarDesc, int, ConstantVarDesc*);
	CSharpReturnAPI4(vBOOL, EngineNS, IConstantBuffer, SetVarValuePtr, int, void*, int, UINT);
	CSharpReturnAPI2(void*, EngineNS, IConstantBuffer, GetVarValueAddress, int, UINT);
	CSharpAPI1(EngineNS, IConstantBuffer, FlushContent, ICommandList*);
	CSharpReturnAPI1(vBOOL, EngineNS, IConstantBuffer, FlushContent2, IRenderContext*);
}