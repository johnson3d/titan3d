#pragma once
#include "../IInputLayout.h"

NS_BEGIN

class INullInputLayout : public IInputLayout
{
public:
	INullInputLayout();
	~INullInputLayout();
};

NS_END