#pragma once
#include "../IInputLayout.h"
#include "D11PreHead.h"

NS_BEGIN

class ID11RenderContext;
class ID11InputLayout : public IInputLayout
{
public:
	ID11InputLayout();
	~ID11InputLayout();
	inline ID3D11InputLayout* GetInnerLayout() {
		return mLayout;
	}
private:
	ID3D11InputLayout*			mLayout;
public:
	bool Init(ID11RenderContext* rc, const IInputLayoutDesc* desc);
};

NS_END