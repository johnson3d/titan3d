#pragma once
#include "IRenderResource.h"

NS_BEGIN

class XndAttribute;

class ICommandList;
class IRenderContext;
class ITextureBase;
class IShaderResourceView;

class IShaderProgram;

#define MaxConstBufferNum 8

struct TR_CLASS(SV_LayoutStruct = 8)
ConstantVarDesc
{
	ConstantVarDesc()
	{
		Dirty = TRUE;
	}
	~ConstantVarDesc()
	{

	}
	UINT GetArrayStride() const{
		return Size / Elements;
	}
	EShaderVarType	Type;
	UINT			Offset;
	UINT			Size;
	UINT            Elements;
	VNameString		Name;
	vBOOL			Dirty;
};

struct TR_CLASS(SV_LayoutStruct = 8)
IConstantBufferDesc : public IShaderBinder
{
	IConstantBufferDesc()
	{
		SetDefault();
	}
	TR_FUNCTION()
	void SetDefault()
	{
		IShaderBinder::SetDefault();
		BindType = EShaderBindType::SBT_CBuffer;
		Size = 0;		
		CPUAccess = 0;
	}
	UINT			Size;	
	UINT			CPUAccess;
	
	TR_DISCARD()
	std::vector<ConstantVarDesc>	Vars;
	
	UINT FindVar(const char* name) const{
		for (UINT i = 0; i < Vars.size(); i++)
		{
			if (Vars[i].Name == name)
			{
				return i;
			}
		}
		return -1;
	}
	const ConstantVarDesc* GetVar(UINT index) const {
		if (index >= Vars.size())
			return nullptr;
		return &Vars[index];
	}

	void Save2Xnd(XndAttribute* attr);
	void LoadXnd(XndAttribute* attr);
	vBOOL IsSameVars(const IConstantBufferDesc* desc);
};

class TR_CLASS()
IConstantBuffer : public IRenderResource
{
protected:
	bool							mDirty;
	bool							mHasPushed;
	void SetDirty()
	{
		if (mDirty == false)
		{
			mDirty = true;
		}
	}
public:
	IConstantBuffer();
	~IConstantBuffer();

	IConstantBufferDesc				Desc;
	std::vector<BYTE>				VarBuffer;
	
	TR_FUNCTION()
	const char* GetName() const {
		return Desc.Name.c_str();
	}
	TR_FUNCTION()
	UINT GetSize() const {
		return Desc.Size;
	}
	TR_FUNCTION()
	vBOOL IsSameVars(IShaderProgram* program, UINT cbIndex);

	virtual void DoSwap(IRenderContext* rc) override;
	virtual bool UpdateContent(ICommandList* cmd, void* pBuffer, UINT Size) = 0;
	TR_FUNCTION()
	virtual void UpdateDrawPass(ICommandList* cmd, vBOOL bImm);

	template<typename KVarType>
	inline bool SetVarValue(const char* name, KVarType value, UINT index) {
		auto desc = GetVarOffset(name);
		if (desc == nullptr)
			return false;
		desc->Dirty = TRUE;
		if (sizeof(value) != desc->Size)
			return false;
		auto ptr = GetVarPtr(desc) + index * desc->GetArrayStride();//GetShaderVarTypeSize(desc->Type);
		if (ptr == nullptr)
			return false;
		(*(KVarType*)ptr) = value;
		
		SetDirty();
		return true;
	}
	template<typename KVarType>
	inline bool GetVarValue(const char* name, KVarType& value, UINT index) {
		auto desc = GetVarOffset(name);
		if (desc == nullptr)
			return false;
		if (sizeof(value) != desc->Size)
			return false;
		auto ptr = GetVarPtr(desc) + index * desc->GetArrayStride();//GetShaderVarTypeSize(desc->Type);
		value = (*(KVarType*)ptr);
		return true;
	}

	template<typename KVarType>
	inline bool SetVarArrayValue(const char* name, KVarType* value, UINT count) {
		auto desc = GetVarOffset(name);
		if (desc == nullptr)
			return false;
		if (sizeof(KVarType)*count != desc->Size)
			return false;
		if (desc->Elements != count)
			return false;
		desc->Dirty = TRUE;
		auto ptr = GetVarPtr(desc);
		if (ptr == nullptr)
			return false;
		//(*(KVarType*)ptr) = value;
		memcpy(ptr, value, sizeof(KVarType)*count);
		
		SetDirty();
		return true;
	}

	void FlushContent(ICommandList* cmd) {
		if (VarBuffer.size() == 0)
			return;
		UpdateContent(cmd, &VarBuffer[0], (UINT)VarBuffer.size());
	}
	vBOOL FlushContent2(IRenderContext* ctx);
	TR_FUNCTION()
	inline int FindVar(const char* name)
	{
		for (size_t i = 0; i < Desc.Vars.size(); i++)
		{
			auto& var = Desc.Vars[i];
			if (var.Name == name)
			{
				return (int)i;
			}
		}
		return -1;
	}
	TR_FUNCTION()
	inline vBOOL GetVarDesc(int index, ConstantVarDesc* desc)
	{
		if (index < 0 || index >= (int)Desc.Vars.size())
			return FALSE;
		auto& var = Desc.Vars[index];
		desc->Type = var.Type;
		desc->Offset = var.Offset;
		desc->Size = var.Size;
		desc->Elements = var.Elements;
		return TRUE;
	}
	TR_FUNCTION()
	inline vBOOL SetVarValuePtr(int index, void* data, int len, UINT elementIndex)
	{
		if (index < 0 || index >= (int)Desc.Vars.size())
			return FALSE;
		auto var = &Desc.Vars[index];
		var->Dirty = TRUE;
		auto elementOffset = elementIndex * var->GetArrayStride();//GetShaderVarTypeSize(var->Type);
		/*if (var->Type == SVT_Struct)
		{
			elementOffset = elementIndex * (var->Size / var->Elements);
		}*/
		if (var->Offset + elementOffset + len > Desc.Size)
			return FALSE;
		BYTE* target = GetVarPtr(var) + elementOffset;
		memcpy(target, data, len);
		
		SetDirty();

		return TRUE;
	}
	TR_FUNCTION()
	inline void* GetVarValueAddress(int index, UINT elementIndex)
	{
		if (index < 0 || index >= (int)Desc.Vars.size())
			return FALSE;
		auto var = &Desc.Vars[index];
		var->Dirty = TRUE;
		auto elementOffset = elementIndex * var->GetArrayStride();//GetShaderVarTypeSize(var->Type);
		/*if (var->Type == SVT_Struct)
		{
			elementOffset = elementIndex * (var->Size / var->Elements);
		}*/
		BYTE* target = GetVarPtr(var) + elementOffset;
		
		return target;
	}
	TR_FUNCTION()
	inline BYTE* GetVarPtrToWrite(int index, int len)
	{
		if (index < 0 || index >= (int)Desc.Vars.size())
			return nullptr;
		auto var = &Desc.Vars[index];
		var->Dirty = TRUE;
		if (var->Offset + len > Desc.Size)
			return nullptr;
		BYTE* target = GetVarPtr(var);
		
		SetDirty();

		return target;
	}
private:
	inline ConstantVarDesc* GetVarOffset(const char* name) {
		for (size_t i = 0; i < Desc.Vars.size(); i++)
		{
			auto& var = Desc.Vars[i];
			if (var.Name == name)
			{
				return &Desc.Vars[i];
			}
		}
		return nullptr;
	}
	inline BYTE* GetVarPtr(const ConstantVarDesc* varDesc) {
		if (VarBuffer.size() == 0)
			VarBuffer.resize(Desc.Size);

		if (varDesc->Offset + varDesc->Size > VarBuffer.size())
			return nullptr;

		return &VarBuffer[varDesc->Offset];
	}
};

NS_END