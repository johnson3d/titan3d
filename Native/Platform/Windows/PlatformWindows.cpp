#include "PlatformWindows.h"

#define new VNEW

NS_BEGIN

PlatformWindows	PlatformWindows::Object;

PlatformWindows::PlatformWindows()
{
}


PlatformWindows::~PlatformWindows()
{
}

EPlatformType PlatformWindows::GetPlatformType()
{
	return PLTF_Windows;
}

NS_END