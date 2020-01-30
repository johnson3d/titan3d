#pragma once
#include "../../BaseHead.h"

NS_BEGIN

class PlatformWindows : public IVPlatform
{
	static PlatformWindows	Object;
public:
	PlatformWindows();
	~PlatformWindows();

	static PlatformWindows* GetInstance() {
		return &Object;
	}
public:
	virtual EPlatformType GetPlatformType() override;
};

NS_END