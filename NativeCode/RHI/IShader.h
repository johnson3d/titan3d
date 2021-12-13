#pragma once
#include "IRenderResource.h"

NS_BEGIN

class XndNode;
class IRenderContext;
class ICommandList;
struct IConstantBufferDesc;
class IConstantBuffer;
struct TSBindInfo;
class ShaderReflector;

inline EShaderType GetShaderTypeFrom(std::string sm)
{
	if (sm == "vs_4_0" || sm == "vs_5_0")
		return EST_VertexShader;
	else if (sm == "ps_4_0" || sm == "ps_5_0")
		return EST_PixelShader;
	else if (sm == "cs_4_0" || sm == "cs_5_0")
		return EST_ComputeShader;
	ASSERT(false);
	return EST_VertexShader;
}

class TR_CLASS()
IShaderDesc : public VIUnknown
{
private:
	std::vector<BYTE>		Codes;
	Hash64					HashCode;
public:
	std::string				Es300Code;
	std::string				MetalCode;
	std::vector<UINT>		SpirV;
public:
	EShaderType				ShaderType;
private:
	ShaderReflector*		Reflector;
public:
	RTTI_DEF(IShaderDesc, 0x965458df5b039800, true);
	
	IShaderDesc();	
	IShaderDesc(EShaderType type);
	~IShaderDesc();
	ShaderReflector* GetReflector() {
		return Reflector;
	}
	void SetGLCode(const char* code){
		Es300Code = code;
	}
	void SetMetalCode(const char* code) {
		MetalCode = code;
	}
	
	const char* GetGLCode() const {
		return Es300Code.c_str();
	}
	
	const char* GetMetalCode() const {
		return MetalCode.c_str();
	}
	
	void SetShaderType(EShaderType type)
	{
		ShaderType = type;
	}
	
	EShaderType GetShaderType() const
	{
		return ShaderType;
	}
	void SetDXBC(BYTE* ptr, int count)
	{
		Codes.resize(count);
		memcpy(&Codes[0], ptr, count);
		HashCode = HashHelper::CalcHash64(ptr, count);
	}
	virtual Hash64 GetHash64() override
	{
		return HashCode;
	}
	inline const std::vector<BYTE>& GetCodes() const {
		return Codes;
	}
	
	void Save2Xnd(XndNode* node, DWORD platforms);
	
	vBOOL LoadXnd(XndNode* node);	
};

class TR_CLASS()
IShader : public IRenderResource
{
protected:
	AutoRef<IShaderDesc>		mDesc;
public:
	IShader();
	~IShader();

	IShaderDesc* GetDesc() {
		return mDesc;
	}

	ShaderReflector* GetReflector() {
		return mDesc->GetReflector();
	}
};

class TR_CLASS(SV_Dispose=self->Release())
IShaderDefinitions : public VIUnknown
{
public:
	RTTI_DEF(IShaderDefinitions, 0x807c2f725b0bf4db, true);
	std::vector<MacroDefine>	Definitions;
	
	TR_CONSTRUCTOR()
	IShaderDefinitions()
	{

	}
	TR_FUNCTION()
	void AddDefine(const char* name, const char* value);
	TR_FUNCTION()
	const MacroDefine* FindDefine(const char* name) const;
	TR_FUNCTION()
	void ClearDefines();
	TR_FUNCTION()
	void RemoveDefine(const char* name);
	TR_FUNCTION()
	void MergeDefinitions(IShaderDefinitions* def);
};

NS_END