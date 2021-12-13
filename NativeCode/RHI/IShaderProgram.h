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

class ShaderReflector;

class IRenderContext;

struct TR_CLASS(SV_LayoutStruct = 8)
IShaderProgramDesc
{
	IInputLayout*	InputLayout;
	IVertexShader*	VertexShader;
	IPixelShader*	PixelShader;
};

class TR_CLASS()
IShaderProgram : public IRenderResource
{
protected:
	AutoRef<IInputLayout>		mInputLayout;
	AutoRef<IVertexShader>		mVertexShader;
	AutoRef<IPixelShader>		mPixelShader;
	AutoRef<ShaderReflector>	mReflector;
public:
	IShaderProgram();
	~IShaderProgram();

	TR_FUNCTION()
	virtual void BindVertexShader(IVertexShader* VertexShader);
	TR_FUNCTION()
	virtual void BindPixelShader(IPixelShader* PixelShader);
	TR_FUNCTION()
	virtual void BindInputLayout(IInputLayout* Layout);
	TR_FUNCTION()
	IVertexShader* GetVertexShader() {
		return mVertexShader;
	}
	TR_FUNCTION()
	IPixelShader* GetPixelShader() {
		return mPixelShader;
	}
	TR_FUNCTION()
	IInputLayout* GetInputLayout() {
		return mInputLayout;
	}

	TR_FUNCTION()
	virtual vBOOL LinkShaders(IRenderContext* rc) = 0;
	TR_FUNCTION()
	virtual void ApplyShaders(ICommandList* cmd) = 0;

	ShaderReflector* GetReflector() {
		return mReflector;
	}
};

NS_END