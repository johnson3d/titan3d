#include "IGLRasterizerState.h"
#include "GLPreHead.h"

#define new VNEW

NS_BEGIN

IGLRasterizerState::IGLRasterizerState()
{
}

IGLRasterizerState::~IGLRasterizerState()
{
	Cleanup();
}

void IGLRasterizerState::Cleanup()
{

}

void IGLRasterizerState::ApplyStates(GLSdk* sdk)
{
	sdk->FrontFace(GL_CW);
	switch (mDesc.CullMode)
	{
	case CMD_NONE:
		sdk->Disable(GL_CULL_FACE);
		break;
	case CMD_FRONT:
		sdk->Enable(GL_CULL_FACE);
		sdk->CullFace(GL_FRONT);
		break;
	case CMD_BACK:
		sdk->Enable(GL_CULL_FACE);
		sdk->CullFace(GL_BACK);
		break;
	}
#if defined(PLATFORM_WIN)
	//GLES没有这个设置了
	switch (mDesc.FillMode)
	{
	case FMD_WIREFRAME:
		sdk->PolygonMode(GL_FRONT_AND_BACK, GL_LINE);
		break;
	case FMD_SOLID:
		sdk->PolygonMode(GL_FRONT_AND_BACK, GL_FILL);
		break;
	}
#endif

	if (mDesc.DepthBias != 0 || mDesc.SlopeScaledDepthBias != 0.0f)
	{
		sdk->Enable(GL_POLYGON_OFFSET_FILL);
		//sdk->Enable(GL_POLYGON_OFFSET_LINE);//在835手机上，这个都会触发GLError，没有这个能力
		sdk->PolygonOffset(mDesc.SlopeScaledDepthBias, (float)mDesc.DepthBias);
	}
	else
	{
		sdk->Disable(GL_POLYGON_OFFSET_FILL);
		//sdk->Disable(GL_POLYGON_OFFSET_LINE);
	}
}

bool IGLRasterizerState::Init(IGLRenderContext* rc, const IRasterizerStateDesc* desc)
{
	mDesc = *desc;
	return true;
}

NS_END