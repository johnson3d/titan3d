#pragma once
// Mac 胶水和 IntelliSense 的独立入口；不依赖 Core.Window 的 PCH。
#include "../NativeCode/Base/BaseHead.h"
#include "../NativeCode/Base/CoreRtti.h"
#include "../NativeCode/Base/CSharpAPI.h"
#include "../NativeCode/Base/xnd/vfxxnd.h"
#define IMGUI_DEFINE_MATH_OPERATORS
#include "../NativeCode/ImGui/imgui.h"
#include "../NativeCode/Base/debug/vfxnew.h"
#include "../NativeCode/Math/vfxgeometry.h"
#include "../NativeCode/Base/csharp/CsBinder.h"
#include "../NativeCode/NextRHI/NxRHI.h"
using namespace EngineNS;
