#pragma once
#include "../IFrameBuffers.h"

NS_BEGIN

class ID11RenderContext;
class ID11FrameBuffers : public IFrameBuffers
{
public:
	ID11FrameBuffers();
	~ID11FrameBuffers();

public:
	bool Init(ID11RenderContext* rc, const IFrameBuffersDesc* desc);

};

NS_END