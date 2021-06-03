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

enum TR_ENUM(SV_NameSpace = EngineNS, SV_UsingNS = EngineNS)
EShaderType
{
	EST_UnknownShader,
	EST_VertexShader,
	EST_PixelShader,
	EST_ComputeShader,
};

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

class TR_CLASS(SV_NameSpace = EngineNS, SV_UsingNS = EngineNS)
IShaderDesc : public VIUnknown
{
private:
	std::vector<BYTE>		Codes;
	Hash64					HashCode;
public:
	std::string				Es300Code;
	std::string				MetalCode;
public:
	EShaderType				ShaderType;
	ShaderReflector*		Reflector;
public:
	RTTI_DEF(IShaderDesc, 0x965458df5b039800, true);
	TR_CONSTRUCTOR()
	IShaderDesc()
	{
		ShaderType = EST_UnknownShader;
		Reflector = nullptr;
	}
	TR_CONSTRUCTOR()
	IShaderDesc(EShaderType type)
	{
		ShaderType = type;
		Reflector = nullptr;
	}
	~IShaderDesc();
	void SetGLCode(const char* code){
		Es300Code = code;
	}
	void SetMetalCode(const char* code) {
		MetalCode = code;
	}
	TR_FUNCTION()
	const char* GetGLCode() const {
		return Es300Code.c_str();
	}
	TR_FUNCTION()
	const char* GetMetalCode() const {
		return MetalCode.c_str();
	}
	TR_FUNCTION()
	void SetShaderType(EShaderType type)
	{
		ShaderType = type;
	}
	TR_FUNCTION()
	EShaderType GetShaderType()
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
	
	TR_FUNCTION()
	void Save2Xnd(XndNode* node, DWORD platforms);
	TR_FUNCTION()
	vBOOL LoadXnd(XndNode* node);

	TR_FUNCTION()
	UINT GetCBufferNum();
	TR_FUNCTION()
	UINT GetSRVNum();
	TR_FUNCTION()
	UINT GetSamplerNum();
	TR_FUNCTION()
	vBOOL GetCBufferDesc(UINT index, IConstantBufferDesc* info);
	TR_FUNCTION()
	vBOOL GetSRVDesc(UINT index, TSBindInfo* info);
	TR_FUNCTION()
	vBOOL GetSamplerDesc(UINT index, TSBindInfo* info);
	TR_FUNCTION()
	UINT FindCBufferDesc(const char* name);
	TR_FUNCTION()
	UINT FindSRVDesc(const char* name);
	TR_FUNCTION()
	UINT FindSamplerDesc(const char* name);
};

class TR_CLASS(SV_NameSpace = EngineNS, SV_UsingNS = EngineNS)
IShader : public IRenderResource
{
protected:
	AutoRef<IShaderDesc>		mDesc;
public:
	IShader();
	~IShader();

	TR_FUNCTION()
	IShaderDesc* GetDesc() {
		return mDesc;
	}
	
	TR_FUNCTION()
	UINT32 GetConstantBufferNumber() const;
	TR_FUNCTION()
	bool GetConstantBufferDesc(UINT32 Index, IConstantBufferDesc* desc);
	TR_FUNCTION()
	const IConstantBufferDesc* FindCBufferDesc(const char* name);
	
	TR_FUNCTION()
	UINT GetShaderResourceNumber() const;
	TR_FUNCTION()
	bool GetShaderResourceBindInfo(UINT Index, TSBindInfo* info) const;
	TR_FUNCTION()
	const TSBindInfo* FindTextureBindInfo(const char* name);

	TR_FUNCTION()
	UINT GetSamplerNumber() const;
	TR_FUNCTION()
	bool GetSamplerBindInfo(UINT Index, TSBindInfo* info) const;
	TR_FUNCTION()
	const TSBindInfo* FindSamplerBindInfo(const char* name);
};

class TR_CLASS(SV_NameSpace = EngineNS, SV_UsingNS = EngineNS)
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