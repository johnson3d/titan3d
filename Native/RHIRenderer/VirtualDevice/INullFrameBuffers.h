#pragma once
#include "../IFrameBuffers.h"

NS_BEGIN

class INullFrameBuffers : public IFrameBuffers
{
public:
	INullFrameBuffers();
	~INullFrameBuffers();
};

NS_END