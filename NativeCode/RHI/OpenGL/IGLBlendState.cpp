#include "IGLBlendState.h"

#define new VNEW

NS_BEGIN

IGLBlendState::IGLBlendState()
{
}


IGLBlendState::~IGLBlendState()
{
}

bool IGLBlendState::Init(IGLRenderContext* rc, const IBlendStateDesc* desc)
{
	mDesc = *desc;
	return true;
}

void IGLBlendState::ApplyStates(GLSdk* sdk)
{
	if(mDesc.RenderTarget[0].BlendEnable)
		sdk->Enable(GL_BLEND);
	else
		sdk->Disable(GL_BLEND);

	if (mDesc.RenderTarget[0].BlendEnable)
	{
		sdk->BlendFuncSeparate(BlendToGL(mDesc.RenderTarget[0].SrcBlend), BlendToGL(mDesc.RenderTarget[0].DestBlend),
								BlendToGL(mDesc.RenderTarget[0].SrcBlendAlpha), BlendToGL(mDesc.RenderTarget[0].DestBlendAlpha));
		
		sdk->BlendEquationSeparate(BlendOPToGL(mDesc.RenderTarget[0].BlendOp), BlendOPToGL(mDesc.RenderTarget[0].BlendOpAlpha));
		
		//OpengGL 4 support mrt blend set,but gles3 doesn't support them
		//glBlendFuncSeparatei
		//glBlendEquationSeparatei
	}
}

NS_END