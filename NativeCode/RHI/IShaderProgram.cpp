#include "IShaderProgram.h"
#include "IVertexShader.h"
#include "IPixelShader.h"
#include "IInputLayout.h"
#include "IConstantBuffer.h"
#include "ShaderReflector.h"

#define new VNEW

NS_BEGIN

IShaderProgram::IShaderProgram()
{
	mReflector.WeakRef(new ShaderReflector());
}

IShaderProgram::~IShaderProgram()
{

}

void IShaderProgram::BindVertexShader(IVertexShader* VertexShader)
{
	mVertexShader.StrongRef(VertexShader);
}

void IShaderProgram::BindPixelShader(IPixelShader* PixelShader)
{
	mPixelShader.StrongRef(PixelShader);
}

void IShaderProgram::BindInputLayout(IInputLayout* Layout)
{
	mInputLayout.StrongRef(Layout);
}

NS_END
