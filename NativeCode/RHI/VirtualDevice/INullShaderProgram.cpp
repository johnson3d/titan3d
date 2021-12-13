#include "INullShaderProgram.h"
#include "INullVertexShader.h"
#include "INullPixelShader.h"
#include "INullInputLayout.h"
#include "INullRenderContext.h"
#include "INullConstantBuffer.h"
#include "../ShaderReflector.h"

#define new VNEW

NS_BEGIN

INullShaderProgram::INullShaderProgram()
{
}

INullShaderProgram::~INullShaderProgram()
{
}

vBOOL INullShaderProgram::LinkShaders(IRenderContext* rc)
{
	return TRUE;
}

void INullShaderProgram::ApplyShaders(ICommandList* cmd)
{

}

bool INullShaderProgram::Init(INullRenderContext* rc, const IShaderProgramDesc* desc)
{
	return true;
}

NS_END