#include "IGLInputLayout.h"

#define new VNEW

NS_BEGIN

IGLInputLayout::IGLInputLayout()
{
}


IGLInputLayout::~IGLInputLayout()
{
	Cleanup();
}

void IGLInputLayout::Cleanup()
{

}

bool IGLInputLayout::Init(IGLRenderContext* rc, const IInputLayoutDesc* desc)
{
	mDesc.StrongRef((IInputLayoutDesc*)desc);
	return true;
}

NS_END