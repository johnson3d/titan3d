#pragma once
#include "IRenderResource.h"

NS_BEGIN

class XNDAttrib;

class ICommandList;
class IRenderContext;
class ITextureBase;
class IShaderResourceView;

class IShaderProgram;

#define MaxConstBufferNum 8

struct ConstantVarDesc
{
	ConstantVarDesc()
	{
		Dirty = TRUE;
	}
	EShaderVarType	Type;
	UINT			Offset;
	UINT			Size;
	UINT            Elements;
	std::string		Name;
	vBOOL			Dirty;
};

StructBegin(ConstantVarDesc, EngineNS)
	StructMember(Type);
	StructMember(Offset);
	StructMember(Size);
	StructMember(Elements);
	StructMember(Name);
StructEnd(void)

enum ECBufferRhiType
{
	SIT_CBUFFER = 0,
	SIT_TBUFFER = (SIT_CBUFFER + 1),
	SIT_TEXTURE = (SIT_TBUFFER + 1),
	SIT_SAMPLER = (SIT_TEXTURE + 1),
	SIT_UAV_RWTYPED = (SIT_SAMPLER + 1),
	SIT_STRUCTURED = (SIT_UAV_RWTYPED + 1),
	SIT_UAV_RWSTRUCTURED = (SIT_STRUCTURED + 1),
	SIT_BYTEADDRESS = (SIT_UAV_RWSTRUCTURED + 1),
	SIT_UAV_RWBYTEADDRESS = (SIT_BYTEADDRESS + 1),
	SIT_UAV_APPEND_STRUCTURED = (SIT_UAV_RWBYTEADDRESS + 1),
	SIT_UAV_CONSUME_STRUCTURED = (SIT_UAV_APPEND_STRUCTURED + 1),
	SIT_UAV_RWSTRUCTURED_WITH_COUNTER = (SIT_UAV_CONSUME_STRUCTURED + 1),
};

struct IConstantBufferDesc
{
	IConstantBufferDesc()
	{
		Size = 0;
		VSBindPoint = -1;
		PSBindPoint = -1;
		CSBindPoint = -1;
		BindCount = 0;
		CPUAccess = 0;
		Type = SIT_CBUFFER;
	}
	void CopyBaseData(IConstantBufferDesc* desc) const 
	{
		desc->Type = Type;
		desc->Size = Size;
		desc->VSBindPoint = VSBindPoint;
		desc->PSBindPoint = PSBindPoint;
		desc->CSBindPoint = CSBindPoint;
		desc->BindCount = BindCount;
		desc->CPUAccess = CPUAccess;
		desc->BindCount = BindCount;
	}
	ECBufferRhiType	Type;
	UINT			Size;
	UINT			VSBindPoint;
	UINT			PSBindPoint;
	UINT			CSBindPoint;
	UINT			BindCount;
	UINT			CPUAccess;
	std::string		Name;
	std::vector<ConstantVarDesc>	Vars;
	void Save2Xnd(XNDAttrib* attr);
	void LoadXnd(XNDAttrib* attr);
	vBOOL IsSameVars(const IConstantBufferDesc* desc);
};

class IConstantBuffer : public IRenderResource
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
	
	const char* GetName() const {
		return Desc.Name.c_str();
	}
	UINT GetSize() const {
		return Desc.Size;
	}
	vBOOL IsSameVars(IShaderProgram* program, UINT cbIndex);

	virtual void DoSwap(IRenderContext* rc) override;
	virtual bool UpdateContent(ICommandList* cmd, void* pBuffer, UINT Size) = 0;
	virtual void UpdateDrawPass(ICommandList* cmd, vBOOL bImm);

	template<typename KVarType>
	inline bool SetVarValue(const char* name, KVarType value, UINT index) {
		auto desc = GetVarOffset(name);
		if (desc == nullptr)
			return false;
		desc->Dirty = TRUE;
		if (sizeof(value) != desc->Size)
			return false;
		auto ptr = GetVarPtr(desc) + index * GetShaderVarTypeSize(desc->Type);
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
		auto ptr = GetVarPtr(desc) + index * GetShaderVarTypeSize(desc->Type);
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
	inline vBOOL SetVarValuePtr(int index, void* data, int len, UINT elementIndex)
	{
		if (index < 0 || index >= (int)Desc.Vars.size())
			return FALSE;
		auto var = &Desc.Vars[index];
		var->Dirty = TRUE;
		auto elementOffset = elementIndex * GetShaderVarTypeSize(var->Type);
		if (var->Offset + elementOffset + len > Desc.Size)
			return FALSE;
		BYTE* target = GetVarPtr(var) + elementOffset;
		memcpy(target, data, len);
		
		SetDirty();

		return TRUE;
	}
	inline void* GetVarValueAddress(int index, UINT elementIndex)
	{
		if (index < 0 || index >= (int)Desc.Vars.size())
			return FALSE;
		auto var = &Desc.Vars[index];
		var->Dirty = TRUE;
		auto elementOffset = elementIndex * GetShaderVarTypeSize(var->Type);
		BYTE* target = GetVarPtr(var) + elementOffset;
		
		return target;
	}
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