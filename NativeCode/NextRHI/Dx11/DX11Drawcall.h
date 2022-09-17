#pragma once
#include "../NxDrawcall.h"

NS_BEGIN

namespace NxRHI
{
	class DX11GraphicDraw : public IGraphicDraw
	{
	public:
		virtual void Commit(ICommandList* cmdlist) override;
	};
}

NS_END