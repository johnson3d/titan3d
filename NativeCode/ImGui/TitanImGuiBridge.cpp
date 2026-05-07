#include "../../Core.Window/pch.h"

#if defined(HasModule_ImGui)

#include <cstddef>

#include "imgui.h"
#include "imgui_internal.h"
#include "imgui_binding.h"
#include "TitanImGuiBridge.h"

static_assert(sizeof(ImGuiIO) == 3096, "ImGuiIO layout changed; update CSharpCode/ImGui/NativeBridge bindings.");
static_assert(offsetof(ImGuiIO, ConfigFlags) == 0, "ImGuiIO.ConfigFlags offset changed.");
static_assert(offsetof(ImGuiIO, WantCaptureMouse) == 184, "ImGuiIO.WantCaptureMouse offset changed.");
static_assert(offsetof(ImGuiIO, MouseWheel) == 248, "ImGuiIO.MouseWheel offset changed.");

static_assert(sizeof(ImDrawData) == 72, "ImDrawData layout changed; update CSharpCode/ImGui/NativeBridge bindings.");
static_assert(offsetof(ImDrawData, CmdListsCount) == 4, "ImDrawData.CmdListsCount offset changed.");
static_assert(offsetof(ImDrawData, DisplayPos) == 32, "ImDrawData.DisplayPos offset changed.");

static_assert(sizeof(ImGuiViewport) == 112, "ImGuiViewport layout changed; update CSharpCode/ImGui/NativeBridge bindings.");
static_assert(offsetof(ImGuiViewport, PlatformUserData) == 80, "ImGuiViewport.PlatformUserData offset changed.");
static_assert(offsetof(ImGuiViewport, PlatformHandle) == 88, "ImGuiViewport.PlatformHandle offset changed.");

static_assert(sizeof(ImGuiPlatformIO) == 328, "ImGuiPlatformIO layout changed; update CSharpCode/ImGui/NativeBridge bindings.");

#endif // HasModule_ImGui
