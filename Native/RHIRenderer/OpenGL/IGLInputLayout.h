#pragma once
#include "../IInputLayout.h"

NS_BEGIN

class IGLRenderContext;
class IGLInputLayout : public IInputLayout
{
public:
	IGLInputLayout();
	~IGLInputLayout();

	virtual void Cleanup() override;
public:
public:
	bool Init(IGLRenderContext* rc, const IInputLayoutDesc* desc);
};

NS_END