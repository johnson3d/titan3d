#include "IGLDepthStencilState.h"

#define new VNEW

NS_BEGIN

IGLDepthStencilState::IGLDepthStencilState()
{
}


IGLDepthStencilState::~IGLDepthStencilState()
{
	Cleanup();
}

void IGLDepthStencilState::Cleanup()
{

}

bool IGLDepthStencilState::Init(IGLRenderContext* rc, const IDepthStencilStateDesc* desc)
{
	mDesc = *desc;
	return true;
}

void IGLDepthStencilState::ApplyStates(GLSdk* sdk)
{
	if(mDesc.DepthEnable)
		sdk->Enable(GL_DEPTH_TEST);
	else
		sdk->Disable(GL_DEPTH_TEST);
	
	if (mDesc.StencilEnable)
		sdk->Enable(GL_STENCIL_TEST);
	else
		sdk->Disable(GL_STENCIL_TEST);

	sdk->DepthFunc(ComparisionModeToGL(mDesc.DepthFunc));
}

NS_END