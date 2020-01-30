#pragma once
#include "IRenderResource.h"
#include "IShader.h"

NS_BEGIN

class IInputLayout;
class IVertexShader;
class IPixelShader;
class ICommandList;

struct ConstantVarDesc;
struct IConstantBufferDesc;
class IConstantBuffer;

struct TextureBindInfo;
struct SamplerBindInfo;

class IRenderContext;

struct IShaderProgramDesc
{

};

class IShaderProgram : public IRenderResource
{
protected:
	IInputLayout *	mInputLayout;
	IVertexShader * mVertexShader;
	IPixelShader*	mPixelShader;
public:
	IShaderProgram();
	~IShaderProgram();

	virtual void BindVertexShader(IVertexShader* VertexShader);
	virtual void BindPixelShader(IPixelShader* PixelShader);
	virtual void BindInputLayout(IInputLayout* Layout);
	IVertexShader* GetVertexShader() {
		return mVertexShader;
	}
	IPixelShader* GetPixelShader() {
		return mPixelShader;
	}
	IInputLayout* GetInputLayout() {
		return mInputLayout;
	}

	virtual vBOOL LinkShaders(IRenderContext* rc) = 0;
	virtual void ApplyShaders(ICommandList* cmd) = 0;

	virtual UINT FindCBuffer(const char* name) = 0;
	virtual UINT GetCBufferNumber() = 0;
	virtual IConstantBufferDesc* GetCBuffer(UINT index) = 0;

	vBOOL GetCBufferDesc(UINT bufferIndex, IConstantBufferDesc* desc);

	virtual UINT GetShaderResourceNumber() const = 0;
	virtual bool GetShaderResourceBindInfo(UINT Index, TextureBindInfo* info) const = 0;
	virtual UINT GetTextureBindSlotIndex(const char* name) = 0;

	vBOOL GetSRBindDesc(UINT Index, TextureBindInfo* desc);

	virtual UINT GetSamplerNumber() const = 0;
	virtual bool GetSamplerBindInfo(UINT Index, SamplerBindInfo* info) const = 0;
	virtual UINT GetSamplerBindSlotIndex(const char* name) = 0;

	vBOOL GetSampBindDesc(UINT Index, SamplerBindInfo* desc);
};

NS_END