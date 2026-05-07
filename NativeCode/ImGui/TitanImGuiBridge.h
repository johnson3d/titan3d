#pragma once

#include "imgui.h"

// Stable C ABI bridge for C# ImGui bindings. The exported functions are emitted
// from NativeBridge/*.bridge.cpp with the TitanImGui_* prefix.

#ifndef TITAN_IMGUI_BRIDGE_API
#define TITAN_IMGUI_BRIDGE_API VFX_API
#endif

struct ImTextureRef_PodType
{
	constexpr static int StructSize = sizeof(ImTextureRef);
	char MemData[StructSize];
};
