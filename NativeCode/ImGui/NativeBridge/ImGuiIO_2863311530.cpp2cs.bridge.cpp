//generated from legacy CppWeaving ImGui surface; owned by TitanImGuiBridge
#include "../../../Core.Window/pch.h"
#if defined(HasModule_ImGui)
#include "../imgui.h"
#include "../TitanImGuiBridge.h"


#define new VNEW


namespace 
{
	struct ImGuiIO_Visitor
	{
		static inline ImGuiIO* CreateInstance()
		{
			return new ImGuiIO();
		}
		static inline int FieldGet__ConfigFlags(ImGuiIO* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<int>();
			}
			return (int)self->ConfigFlags;
		}
		static inline void FieldSet__ConfigFlags(ImGuiIO* self, int value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->ConfigFlags = value;
		}
		static inline int FieldGet__BackendFlags(ImGuiIO* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<int>();
			}
			return (int)self->BackendFlags;
		}
		static inline void FieldSet__BackendFlags(ImGuiIO* self, int value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->BackendFlags = value;
		}
		static inline ImVec2 FieldGet__DisplaySize(ImGuiIO* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<ImVec2>();
			}
			return (ImVec2)self->DisplaySize;
		}
		static inline void FieldSet__DisplaySize(ImGuiIO* self, ImVec2 value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->DisplaySize = value;
		}
		static inline ImVec2 FieldGet__DisplayFramebufferScale(ImGuiIO* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<ImVec2>();
			}
			return (ImVec2)self->DisplayFramebufferScale;
		}
		static inline void FieldSet__DisplayFramebufferScale(ImGuiIO* self, ImVec2 value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->DisplayFramebufferScale = value;
		}
		static inline float FieldGet__DeltaTime(ImGuiIO* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<float>();
			}
			return (float)self->DeltaTime;
		}
		static inline void FieldSet__DeltaTime(ImGuiIO* self, float value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->DeltaTime = value;
		}
		static inline float FieldGet__IniSavingRate(ImGuiIO* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<float>();
			}
			return (float)self->IniSavingRate;
		}
		static inline void FieldSet__IniSavingRate(ImGuiIO* self, float value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->IniSavingRate = value;
		}
		static inline char* FieldGet__IniFilename(ImGuiIO* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<char*>();
			}
			return (char*)self->IniFilename;
		}
		static inline void FieldSet__IniFilename(ImGuiIO* self, char* value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->IniFilename = value;
		}
		static inline char* FieldGet__LogFilename(ImGuiIO* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<char*>();
			}
			return (char*)self->LogFilename;
		}
		static inline void FieldSet__LogFilename(ImGuiIO* self, char* value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->LogFilename = value;
		}
		static inline void* FieldGet__UserData(ImGuiIO* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<void*>();
			}
			return (void*)self->UserData;
		}
		static inline void FieldSet__UserData(ImGuiIO* self, void* value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->UserData = value;
		}
		static inline ImFontAtlas* FieldGet__Fonts(ImGuiIO* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<ImFontAtlas*>();
			}
			return (ImFontAtlas*)self->Fonts;
		}
		static inline void FieldSet__Fonts(ImGuiIO* self, ImFontAtlas* value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->Fonts = value;
		}
		static inline ImFont* FieldGet__FontDefault(ImGuiIO* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<ImFont*>();
			}
			return (ImFont*)self->FontDefault;
		}
		static inline void FieldSet__FontDefault(ImGuiIO* self, ImFont* value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->FontDefault = value;
		}
		static inline bool FieldGet__FontAllowUserScaling(ImGuiIO* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<bool>();
			}
			return (bool)self->FontAllowUserScaling;
		}
		static inline void FieldSet__FontAllowUserScaling(ImGuiIO* self, bool value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->FontAllowUserScaling = value;
		}
		static inline bool FieldGet__ConfigNavSwapGamepadButtons(ImGuiIO* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<bool>();
			}
			return (bool)self->ConfigNavSwapGamepadButtons;
		}
		static inline void FieldSet__ConfigNavSwapGamepadButtons(ImGuiIO* self, bool value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->ConfigNavSwapGamepadButtons = value;
		}
		static inline bool FieldGet__ConfigNavMoveSetMousePos(ImGuiIO* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<bool>();
			}
			return (bool)self->ConfigNavMoveSetMousePos;
		}
		static inline void FieldSet__ConfigNavMoveSetMousePos(ImGuiIO* self, bool value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->ConfigNavMoveSetMousePos = value;
		}
		static inline bool FieldGet__ConfigNavCaptureKeyboard(ImGuiIO* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<bool>();
			}
			return (bool)self->ConfigNavCaptureKeyboard;
		}
		static inline void FieldSet__ConfigNavCaptureKeyboard(ImGuiIO* self, bool value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->ConfigNavCaptureKeyboard = value;
		}
		static inline bool FieldGet__ConfigNavEscapeClearFocusItem(ImGuiIO* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<bool>();
			}
			return (bool)self->ConfigNavEscapeClearFocusItem;
		}
		static inline void FieldSet__ConfigNavEscapeClearFocusItem(ImGuiIO* self, bool value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->ConfigNavEscapeClearFocusItem = value;
		}
		static inline bool FieldGet__ConfigNavEscapeClearFocusWindow(ImGuiIO* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<bool>();
			}
			return (bool)self->ConfigNavEscapeClearFocusWindow;
		}
		static inline void FieldSet__ConfigNavEscapeClearFocusWindow(ImGuiIO* self, bool value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->ConfigNavEscapeClearFocusWindow = value;
		}
		static inline bool FieldGet__ConfigNavCursorVisibleAuto(ImGuiIO* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<bool>();
			}
			return (bool)self->ConfigNavCursorVisibleAuto;
		}
		static inline void FieldSet__ConfigNavCursorVisibleAuto(ImGuiIO* self, bool value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->ConfigNavCursorVisibleAuto = value;
		}
		static inline bool FieldGet__ConfigNavCursorVisibleAlways(ImGuiIO* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<bool>();
			}
			return (bool)self->ConfigNavCursorVisibleAlways;
		}
		static inline void FieldSet__ConfigNavCursorVisibleAlways(ImGuiIO* self, bool value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->ConfigNavCursorVisibleAlways = value;
		}
		static inline bool FieldGet__ConfigDockingNoSplit(ImGuiIO* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<bool>();
			}
			return (bool)self->ConfigDockingNoSplit;
		}
		static inline void FieldSet__ConfigDockingNoSplit(ImGuiIO* self, bool value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->ConfigDockingNoSplit = value;
		}
		static inline bool FieldGet__ConfigDockingNoDockingOver(ImGuiIO* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<bool>();
			}
			return (bool)self->ConfigDockingNoDockingOver;
		}
		static inline void FieldSet__ConfigDockingNoDockingOver(ImGuiIO* self, bool value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->ConfigDockingNoDockingOver = value;
		}
		static inline bool FieldGet__ConfigDockingWithShift(ImGuiIO* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<bool>();
			}
			return (bool)self->ConfigDockingWithShift;
		}
		static inline void FieldSet__ConfigDockingWithShift(ImGuiIO* self, bool value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->ConfigDockingWithShift = value;
		}
		static inline bool FieldGet__ConfigDockingAlwaysTabBar(ImGuiIO* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<bool>();
			}
			return (bool)self->ConfigDockingAlwaysTabBar;
		}
		static inline void FieldSet__ConfigDockingAlwaysTabBar(ImGuiIO* self, bool value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->ConfigDockingAlwaysTabBar = value;
		}
		static inline bool FieldGet__ConfigDockingTransparentPayload(ImGuiIO* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<bool>();
			}
			return (bool)self->ConfigDockingTransparentPayload;
		}
		static inline void FieldSet__ConfigDockingTransparentPayload(ImGuiIO* self, bool value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->ConfigDockingTransparentPayload = value;
		}
		static inline bool FieldGet__ConfigViewportsNoAutoMerge(ImGuiIO* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<bool>();
			}
			return (bool)self->ConfigViewportsNoAutoMerge;
		}
		static inline void FieldSet__ConfigViewportsNoAutoMerge(ImGuiIO* self, bool value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->ConfigViewportsNoAutoMerge = value;
		}
		static inline bool FieldGet__ConfigViewportsNoTaskBarIcon(ImGuiIO* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<bool>();
			}
			return (bool)self->ConfigViewportsNoTaskBarIcon;
		}
		static inline void FieldSet__ConfigViewportsNoTaskBarIcon(ImGuiIO* self, bool value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->ConfigViewportsNoTaskBarIcon = value;
		}
		static inline bool FieldGet__ConfigViewportsNoDecoration(ImGuiIO* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<bool>();
			}
			return (bool)self->ConfigViewportsNoDecoration;
		}
		static inline void FieldSet__ConfigViewportsNoDecoration(ImGuiIO* self, bool value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->ConfigViewportsNoDecoration = value;
		}
		static inline bool FieldGet__ConfigViewportsNoDefaultParent(ImGuiIO* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<bool>();
			}
			return (bool)self->ConfigViewportsNoDefaultParent;
		}
		static inline void FieldSet__ConfigViewportsNoDefaultParent(ImGuiIO* self, bool value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->ConfigViewportsNoDefaultParent = value;
		}
		static inline bool FieldGet__ConfigViewportsPlatformFocusSetsImGuiFocus(ImGuiIO* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<bool>();
			}
			return (bool)self->ConfigViewportsPlatformFocusSetsImGuiFocus;
		}
		static inline void FieldSet__ConfigViewportsPlatformFocusSetsImGuiFocus(ImGuiIO* self, bool value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->ConfigViewportsPlatformFocusSetsImGuiFocus = value;
		}
		static inline bool FieldGet__ConfigDpiScaleFonts(ImGuiIO* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<bool>();
			}
			return (bool)self->ConfigDpiScaleFonts;
		}
		static inline void FieldSet__ConfigDpiScaleFonts(ImGuiIO* self, bool value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->ConfigDpiScaleFonts = value;
		}
		static inline bool FieldGet__ConfigDpiScaleViewports(ImGuiIO* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<bool>();
			}
			return (bool)self->ConfigDpiScaleViewports;
		}
		static inline void FieldSet__ConfigDpiScaleViewports(ImGuiIO* self, bool value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->ConfigDpiScaleViewports = value;
		}
		static inline bool FieldGet__MouseDrawCursor(ImGuiIO* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<bool>();
			}
			return (bool)self->MouseDrawCursor;
		}
		static inline void FieldSet__MouseDrawCursor(ImGuiIO* self, bool value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->MouseDrawCursor = value;
		}
		static inline bool FieldGet__ConfigMacOSXBehaviors(ImGuiIO* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<bool>();
			}
			return (bool)self->ConfigMacOSXBehaviors;
		}
		static inline void FieldSet__ConfigMacOSXBehaviors(ImGuiIO* self, bool value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->ConfigMacOSXBehaviors = value;
		}
		static inline bool FieldGet__ConfigInputTrickleEventQueue(ImGuiIO* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<bool>();
			}
			return (bool)self->ConfigInputTrickleEventQueue;
		}
		static inline void FieldSet__ConfigInputTrickleEventQueue(ImGuiIO* self, bool value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->ConfigInputTrickleEventQueue = value;
		}
		static inline bool FieldGet__ConfigInputTextCursorBlink(ImGuiIO* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<bool>();
			}
			return (bool)self->ConfigInputTextCursorBlink;
		}
		static inline void FieldSet__ConfigInputTextCursorBlink(ImGuiIO* self, bool value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->ConfigInputTextCursorBlink = value;
		}
		static inline bool FieldGet__ConfigInputTextEnterKeepActive(ImGuiIO* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<bool>();
			}
			return (bool)self->ConfigInputTextEnterKeepActive;
		}
		static inline void FieldSet__ConfigInputTextEnterKeepActive(ImGuiIO* self, bool value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->ConfigInputTextEnterKeepActive = value;
		}
		static inline bool FieldGet__ConfigDragClickToInputText(ImGuiIO* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<bool>();
			}
			return (bool)self->ConfigDragClickToInputText;
		}
		static inline void FieldSet__ConfigDragClickToInputText(ImGuiIO* self, bool value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->ConfigDragClickToInputText = value;
		}
		static inline bool FieldGet__ConfigWindowsResizeFromEdges(ImGuiIO* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<bool>();
			}
			return (bool)self->ConfigWindowsResizeFromEdges;
		}
		static inline void FieldSet__ConfigWindowsResizeFromEdges(ImGuiIO* self, bool value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->ConfigWindowsResizeFromEdges = value;
		}
		static inline bool FieldGet__ConfigWindowsMoveFromTitleBarOnly(ImGuiIO* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<bool>();
			}
			return (bool)self->ConfigWindowsMoveFromTitleBarOnly;
		}
		static inline void FieldSet__ConfigWindowsMoveFromTitleBarOnly(ImGuiIO* self, bool value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->ConfigWindowsMoveFromTitleBarOnly = value;
		}
		static inline bool FieldGet__ConfigWindowsCopyContentsWithCtrlC(ImGuiIO* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<bool>();
			}
			return (bool)self->ConfigWindowsCopyContentsWithCtrlC;
		}
		static inline void FieldSet__ConfigWindowsCopyContentsWithCtrlC(ImGuiIO* self, bool value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->ConfigWindowsCopyContentsWithCtrlC = value;
		}
		static inline bool FieldGet__ConfigScrollbarScrollByPage(ImGuiIO* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<bool>();
			}
			return (bool)self->ConfigScrollbarScrollByPage;
		}
		static inline void FieldSet__ConfigScrollbarScrollByPage(ImGuiIO* self, bool value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->ConfigScrollbarScrollByPage = value;
		}
		static inline float FieldGet__ConfigMemoryCompactTimer(ImGuiIO* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<float>();
			}
			return (float)self->ConfigMemoryCompactTimer;
		}
		static inline void FieldSet__ConfigMemoryCompactTimer(ImGuiIO* self, float value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->ConfigMemoryCompactTimer = value;
		}
		static inline float FieldGet__MouseDoubleClickTime(ImGuiIO* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<float>();
			}
			return (float)self->MouseDoubleClickTime;
		}
		static inline void FieldSet__MouseDoubleClickTime(ImGuiIO* self, float value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->MouseDoubleClickTime = value;
		}
		static inline float FieldGet__MouseDoubleClickMaxDist(ImGuiIO* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<float>();
			}
			return (float)self->MouseDoubleClickMaxDist;
		}
		static inline void FieldSet__MouseDoubleClickMaxDist(ImGuiIO* self, float value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->MouseDoubleClickMaxDist = value;
		}
		static inline float FieldGet__MouseDragThreshold(ImGuiIO* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<float>();
			}
			return (float)self->MouseDragThreshold;
		}
		static inline void FieldSet__MouseDragThreshold(ImGuiIO* self, float value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->MouseDragThreshold = value;
		}
		static inline float FieldGet__KeyRepeatDelay(ImGuiIO* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<float>();
			}
			return (float)self->KeyRepeatDelay;
		}
		static inline void FieldSet__KeyRepeatDelay(ImGuiIO* self, float value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->KeyRepeatDelay = value;
		}
		static inline float FieldGet__KeyRepeatRate(ImGuiIO* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<float>();
			}
			return (float)self->KeyRepeatRate;
		}
		static inline void FieldSet__KeyRepeatRate(ImGuiIO* self, float value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->KeyRepeatRate = value;
		}
		static inline bool FieldGet__ConfigErrorRecovery(ImGuiIO* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<bool>();
			}
			return (bool)self->ConfigErrorRecovery;
		}
		static inline void FieldSet__ConfigErrorRecovery(ImGuiIO* self, bool value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->ConfigErrorRecovery = value;
		}
		static inline bool FieldGet__ConfigErrorRecoveryEnableAssert(ImGuiIO* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<bool>();
			}
			return (bool)self->ConfigErrorRecoveryEnableAssert;
		}
		static inline void FieldSet__ConfigErrorRecoveryEnableAssert(ImGuiIO* self, bool value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->ConfigErrorRecoveryEnableAssert = value;
		}
		static inline bool FieldGet__ConfigErrorRecoveryEnableDebugLog(ImGuiIO* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<bool>();
			}
			return (bool)self->ConfigErrorRecoveryEnableDebugLog;
		}
		static inline void FieldSet__ConfigErrorRecoveryEnableDebugLog(ImGuiIO* self, bool value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->ConfigErrorRecoveryEnableDebugLog = value;
		}
		static inline bool FieldGet__ConfigErrorRecoveryEnableTooltip(ImGuiIO* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<bool>();
			}
			return (bool)self->ConfigErrorRecoveryEnableTooltip;
		}
		static inline void FieldSet__ConfigErrorRecoveryEnableTooltip(ImGuiIO* self, bool value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->ConfigErrorRecoveryEnableTooltip = value;
		}
		static inline bool FieldGet__ConfigDebugIsDebuggerPresent(ImGuiIO* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<bool>();
			}
			return (bool)self->ConfigDebugIsDebuggerPresent;
		}
		static inline void FieldSet__ConfigDebugIsDebuggerPresent(ImGuiIO* self, bool value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->ConfigDebugIsDebuggerPresent = value;
		}
		static inline bool FieldGet__ConfigDebugHighlightIdConflicts(ImGuiIO* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<bool>();
			}
			return (bool)self->ConfigDebugHighlightIdConflicts;
		}
		static inline void FieldSet__ConfigDebugHighlightIdConflicts(ImGuiIO* self, bool value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->ConfigDebugHighlightIdConflicts = value;
		}
		static inline bool FieldGet__ConfigDebugHighlightIdConflictsShowItemPicker(ImGuiIO* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<bool>();
			}
			return (bool)self->ConfigDebugHighlightIdConflictsShowItemPicker;
		}
		static inline void FieldSet__ConfigDebugHighlightIdConflictsShowItemPicker(ImGuiIO* self, bool value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->ConfigDebugHighlightIdConflictsShowItemPicker = value;
		}
		static inline bool FieldGet__ConfigDebugBeginReturnValueOnce(ImGuiIO* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<bool>();
			}
			return (bool)self->ConfigDebugBeginReturnValueOnce;
		}
		static inline void FieldSet__ConfigDebugBeginReturnValueOnce(ImGuiIO* self, bool value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->ConfigDebugBeginReturnValueOnce = value;
		}
		static inline bool FieldGet__ConfigDebugBeginReturnValueLoop(ImGuiIO* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<bool>();
			}
			return (bool)self->ConfigDebugBeginReturnValueLoop;
		}
		static inline void FieldSet__ConfigDebugBeginReturnValueLoop(ImGuiIO* self, bool value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->ConfigDebugBeginReturnValueLoop = value;
		}
		static inline bool FieldGet__ConfigDebugIgnoreFocusLoss(ImGuiIO* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<bool>();
			}
			return (bool)self->ConfigDebugIgnoreFocusLoss;
		}
		static inline void FieldSet__ConfigDebugIgnoreFocusLoss(ImGuiIO* self, bool value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->ConfigDebugIgnoreFocusLoss = value;
		}
		static inline bool FieldGet__ConfigDebugIniSettings(ImGuiIO* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<bool>();
			}
			return (bool)self->ConfigDebugIniSettings;
		}
		static inline void FieldSet__ConfigDebugIniSettings(ImGuiIO* self, bool value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->ConfigDebugIniSettings = value;
		}
		static inline char* FieldGet__BackendPlatformName(ImGuiIO* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<char*>();
			}
			return (char*)self->BackendPlatformName;
		}
		static inline void FieldSet__BackendPlatformName(ImGuiIO* self, char* value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->BackendPlatformName = value;
		}
		static inline char* FieldGet__BackendRendererName(ImGuiIO* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<char*>();
			}
			return (char*)self->BackendRendererName;
		}
		static inline void FieldSet__BackendRendererName(ImGuiIO* self, char* value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->BackendRendererName = value;
		}
		static inline void* FieldGet__BackendPlatformUserData(ImGuiIO* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<void*>();
			}
			return (void*)self->BackendPlatformUserData;
		}
		static inline void FieldSet__BackendPlatformUserData(ImGuiIO* self, void* value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->BackendPlatformUserData = value;
		}
		static inline void* FieldGet__BackendRendererUserData(ImGuiIO* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<void*>();
			}
			return (void*)self->BackendRendererUserData;
		}
		static inline void FieldSet__BackendRendererUserData(ImGuiIO* self, void* value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->BackendRendererUserData = value;
		}
		static inline void* FieldGet__BackendLanguageUserData(ImGuiIO* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<void*>();
			}
			return (void*)self->BackendLanguageUserData;
		}
		static inline void FieldSet__BackendLanguageUserData(ImGuiIO* self, void* value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->BackendLanguageUserData = value;
		}
		static inline bool FieldGet__WantCaptureMouse(ImGuiIO* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<bool>();
			}
			return (bool)self->WantCaptureMouse;
		}
		static inline void FieldSet__WantCaptureMouse(ImGuiIO* self, bool value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->WantCaptureMouse = value;
		}
		static inline bool FieldGet__WantCaptureKeyboard(ImGuiIO* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<bool>();
			}
			return (bool)self->WantCaptureKeyboard;
		}
		static inline void FieldSet__WantCaptureKeyboard(ImGuiIO* self, bool value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->WantCaptureKeyboard = value;
		}
		static inline bool FieldGet__WantTextInput(ImGuiIO* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<bool>();
			}
			return (bool)self->WantTextInput;
		}
		static inline void FieldSet__WantTextInput(ImGuiIO* self, bool value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->WantTextInput = value;
		}
		static inline bool FieldGet__WantSetMousePos(ImGuiIO* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<bool>();
			}
			return (bool)self->WantSetMousePos;
		}
		static inline void FieldSet__WantSetMousePos(ImGuiIO* self, bool value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->WantSetMousePos = value;
		}
		static inline bool FieldGet__WantSaveIniSettings(ImGuiIO* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<bool>();
			}
			return (bool)self->WantSaveIniSettings;
		}
		static inline void FieldSet__WantSaveIniSettings(ImGuiIO* self, bool value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->WantSaveIniSettings = value;
		}
		static inline bool FieldGet__NavActive(ImGuiIO* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<bool>();
			}
			return (bool)self->NavActive;
		}
		static inline void FieldSet__NavActive(ImGuiIO* self, bool value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->NavActive = value;
		}
		static inline bool FieldGet__NavVisible(ImGuiIO* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<bool>();
			}
			return (bool)self->NavVisible;
		}
		static inline void FieldSet__NavVisible(ImGuiIO* self, bool value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->NavVisible = value;
		}
		static inline float FieldGet__Framerate(ImGuiIO* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<float>();
			}
			return (float)self->Framerate;
		}
		static inline void FieldSet__Framerate(ImGuiIO* self, float value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->Framerate = value;
		}
		static inline int FieldGet__MetricsRenderVertices(ImGuiIO* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<int>();
			}
			return (int)self->MetricsRenderVertices;
		}
		static inline void FieldSet__MetricsRenderVertices(ImGuiIO* self, int value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->MetricsRenderVertices = value;
		}
		static inline int FieldGet__MetricsRenderIndices(ImGuiIO* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<int>();
			}
			return (int)self->MetricsRenderIndices;
		}
		static inline void FieldSet__MetricsRenderIndices(ImGuiIO* self, int value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->MetricsRenderIndices = value;
		}
		static inline int FieldGet__MetricsRenderWindows(ImGuiIO* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<int>();
			}
			return (int)self->MetricsRenderWindows;
		}
		static inline void FieldSet__MetricsRenderWindows(ImGuiIO* self, int value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->MetricsRenderWindows = value;
		}
		static inline int FieldGet__MetricsActiveWindows(ImGuiIO* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<int>();
			}
			return (int)self->MetricsActiveWindows;
		}
		static inline void FieldSet__MetricsActiveWindows(ImGuiIO* self, int value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->MetricsActiveWindows = value;
		}
		static inline ImVec2 FieldGet__MouseDelta(ImGuiIO* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<ImVec2>();
			}
			return (ImVec2)self->MouseDelta;
		}
		static inline void FieldSet__MouseDelta(ImGuiIO* self, ImVec2 value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->MouseDelta = value;
		}
		static inline ImVec2 FieldGet__MousePos(ImGuiIO* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<ImVec2>();
			}
			return (ImVec2)self->MousePos;
		}
		static inline void FieldSet__MousePos(ImGuiIO* self, ImVec2 value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->MousePos = value;
		}
		static inline bool* FieldGet__MouseDown(ImGuiIO* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<bool*>();
			}
			return (bool*)self->MouseDown;
		}
		static inline void FieldSet__MouseDown(ImGuiIO* self, bool* value)
		{
			if(self==nullptr)
			{
				return;
			}
			for (int i = 0; i < 5; i++)
			{
				self->MouseDown[i] = value[i];
			}
		}
		static inline float FieldGet__MouseWheel(ImGuiIO* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<float>();
			}
			return (float)self->MouseWheel;
		}
		static inline void FieldSet__MouseWheel(ImGuiIO* self, float value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->MouseWheel = value;
		}
		static inline float FieldGet__MouseWheelH(ImGuiIO* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<float>();
			}
			return (float)self->MouseWheelH;
		}
		static inline void FieldSet__MouseWheelH(ImGuiIO* self, float value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->MouseWheelH = value;
		}
		static inline ImGuiMouseSource FieldGet__MouseSource(ImGuiIO* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<ImGuiMouseSource>();
			}
			return (ImGuiMouseSource)self->MouseSource;
		}
		static inline void FieldSet__MouseSource(ImGuiIO* self, ImGuiMouseSource value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->MouseSource = value;
		}
		static inline unsigned int FieldGet__MouseHoveredViewport(ImGuiIO* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<unsigned int>();
			}
			return (unsigned int)self->MouseHoveredViewport;
		}
		static inline void FieldSet__MouseHoveredViewport(ImGuiIO* self, unsigned int value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->MouseHoveredViewport = value;
		}
		static inline bool FieldGet__KeyCtrl(ImGuiIO* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<bool>();
			}
			return (bool)self->KeyCtrl;
		}
		static inline void FieldSet__KeyCtrl(ImGuiIO* self, bool value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->KeyCtrl = value;
		}
		static inline bool FieldGet__KeyShift(ImGuiIO* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<bool>();
			}
			return (bool)self->KeyShift;
		}
		static inline void FieldSet__KeyShift(ImGuiIO* self, bool value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->KeyShift = value;
		}
		static inline bool FieldGet__KeyAlt(ImGuiIO* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<bool>();
			}
			return (bool)self->KeyAlt;
		}
		static inline void FieldSet__KeyAlt(ImGuiIO* self, bool value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->KeyAlt = value;
		}
		static inline bool FieldGet__KeySuper(ImGuiIO* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<bool>();
			}
			return (bool)self->KeySuper;
		}
		static inline void FieldSet__KeySuper(ImGuiIO* self, bool value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->KeySuper = value;
		}
		static inline int FieldGet__KeyMods(ImGuiIO* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<int>();
			}
			return (int)self->KeyMods;
		}
		static inline void FieldSet__KeyMods(ImGuiIO* self, int value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->KeyMods = value;
		}
		static inline bool FieldGet__WantCaptureMouseUnlessPopupClose(ImGuiIO* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<bool>();
			}
			return (bool)self->WantCaptureMouseUnlessPopupClose;
		}
		static inline void FieldSet__WantCaptureMouseUnlessPopupClose(ImGuiIO* self, bool value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->WantCaptureMouseUnlessPopupClose = value;
		}
		static inline ImVec2 FieldGet__MousePosPrev(ImGuiIO* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<ImVec2>();
			}
			return (ImVec2)self->MousePosPrev;
		}
		static inline void FieldSet__MousePosPrev(ImGuiIO* self, ImVec2 value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->MousePosPrev = value;
		}
		static inline ImVec2* FieldGet__MouseClickedPos(ImGuiIO* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<ImVec2*>();
			}
			return (ImVec2*)self->MouseClickedPos;
		}
		static inline void FieldSet__MouseClickedPos(ImGuiIO* self, ImVec2* value)
		{
			if(self==nullptr)
			{
				return;
			}
			for (int i = 0; i < 5; i++)
			{
				self->MouseClickedPos[i] = value[i];
			}
		}
		static inline double* FieldGet__MouseClickedTime(ImGuiIO* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<double*>();
			}
			return (double*)self->MouseClickedTime;
		}
		static inline void FieldSet__MouseClickedTime(ImGuiIO* self, double* value)
		{
			if(self==nullptr)
			{
				return;
			}
			for (int i = 0; i < 5; i++)
			{
				self->MouseClickedTime[i] = value[i];
			}
		}
		static inline bool* FieldGet__MouseClicked(ImGuiIO* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<bool*>();
			}
			return (bool*)self->MouseClicked;
		}
		static inline void FieldSet__MouseClicked(ImGuiIO* self, bool* value)
		{
			if(self==nullptr)
			{
				return;
			}
			for (int i = 0; i < 5; i++)
			{
				self->MouseClicked[i] = value[i];
			}
		}
		static inline bool* FieldGet__MouseDoubleClicked(ImGuiIO* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<bool*>();
			}
			return (bool*)self->MouseDoubleClicked;
		}
		static inline void FieldSet__MouseDoubleClicked(ImGuiIO* self, bool* value)
		{
			if(self==nullptr)
			{
				return;
			}
			for (int i = 0; i < 5; i++)
			{
				self->MouseDoubleClicked[i] = value[i];
			}
		}
		static inline unsigned short* FieldGet__MouseClickedCount(ImGuiIO* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<unsigned short*>();
			}
			return (unsigned short*)self->MouseClickedCount;
		}
		static inline void FieldSet__MouseClickedCount(ImGuiIO* self, unsigned short* value)
		{
			if(self==nullptr)
			{
				return;
			}
			for (int i = 0; i < 5; i++)
			{
				self->MouseClickedCount[i] = value[i];
			}
		}
		static inline unsigned short* FieldGet__MouseClickedLastCount(ImGuiIO* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<unsigned short*>();
			}
			return (unsigned short*)self->MouseClickedLastCount;
		}
		static inline void FieldSet__MouseClickedLastCount(ImGuiIO* self, unsigned short* value)
		{
			if(self==nullptr)
			{
				return;
			}
			for (int i = 0; i < 5; i++)
			{
				self->MouseClickedLastCount[i] = value[i];
			}
		}
		static inline bool* FieldGet__MouseReleased(ImGuiIO* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<bool*>();
			}
			return (bool*)self->MouseReleased;
		}
		static inline void FieldSet__MouseReleased(ImGuiIO* self, bool* value)
		{
			if(self==nullptr)
			{
				return;
			}
			for (int i = 0; i < 5; i++)
			{
				self->MouseReleased[i] = value[i];
			}
		}
		static inline double* FieldGet__MouseReleasedTime(ImGuiIO* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<double*>();
			}
			return (double*)self->MouseReleasedTime;
		}
		static inline void FieldSet__MouseReleasedTime(ImGuiIO* self, double* value)
		{
			if(self==nullptr)
			{
				return;
			}
			for (int i = 0; i < 5; i++)
			{
				self->MouseReleasedTime[i] = value[i];
			}
		}
		static inline bool* FieldGet__MouseDownOwned(ImGuiIO* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<bool*>();
			}
			return (bool*)self->MouseDownOwned;
		}
		static inline void FieldSet__MouseDownOwned(ImGuiIO* self, bool* value)
		{
			if(self==nullptr)
			{
				return;
			}
			for (int i = 0; i < 5; i++)
			{
				self->MouseDownOwned[i] = value[i];
			}
		}
		static inline bool* FieldGet__MouseDownOwnedUnlessPopupClose(ImGuiIO* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<bool*>();
			}
			return (bool*)self->MouseDownOwnedUnlessPopupClose;
		}
		static inline void FieldSet__MouseDownOwnedUnlessPopupClose(ImGuiIO* self, bool* value)
		{
			if(self==nullptr)
			{
				return;
			}
			for (int i = 0; i < 5; i++)
			{
				self->MouseDownOwnedUnlessPopupClose[i] = value[i];
			}
		}
		static inline bool FieldGet__MouseWheelRequestAxisSwap(ImGuiIO* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<bool>();
			}
			return (bool)self->MouseWheelRequestAxisSwap;
		}
		static inline void FieldSet__MouseWheelRequestAxisSwap(ImGuiIO* self, bool value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->MouseWheelRequestAxisSwap = value;
		}
		static inline bool FieldGet__MouseCtrlLeftAsRightClick(ImGuiIO* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<bool>();
			}
			return (bool)self->MouseCtrlLeftAsRightClick;
		}
		static inline void FieldSet__MouseCtrlLeftAsRightClick(ImGuiIO* self, bool value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->MouseCtrlLeftAsRightClick = value;
		}
		static inline float* FieldGet__MouseDownDuration(ImGuiIO* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<float*>();
			}
			return (float*)self->MouseDownDuration;
		}
		static inline void FieldSet__MouseDownDuration(ImGuiIO* self, float* value)
		{
			if(self==nullptr)
			{
				return;
			}
			for (int i = 0; i < 5; i++)
			{
				self->MouseDownDuration[i] = value[i];
			}
		}
		static inline float* FieldGet__MouseDownDurationPrev(ImGuiIO* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<float*>();
			}
			return (float*)self->MouseDownDurationPrev;
		}
		static inline void FieldSet__MouseDownDurationPrev(ImGuiIO* self, float* value)
		{
			if(self==nullptr)
			{
				return;
			}
			for (int i = 0; i < 5; i++)
			{
				self->MouseDownDurationPrev[i] = value[i];
			}
		}
		static inline ImVec2* FieldGet__MouseDragMaxDistanceAbs(ImGuiIO* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<ImVec2*>();
			}
			return (ImVec2*)self->MouseDragMaxDistanceAbs;
		}
		static inline void FieldSet__MouseDragMaxDistanceAbs(ImGuiIO* self, ImVec2* value)
		{
			if(self==nullptr)
			{
				return;
			}
			for (int i = 0; i < 5; i++)
			{
				self->MouseDragMaxDistanceAbs[i] = value[i];
			}
		}
		static inline float* FieldGet__MouseDragMaxDistanceSqr(ImGuiIO* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<float*>();
			}
			return (float*)self->MouseDragMaxDistanceSqr;
		}
		static inline void FieldSet__MouseDragMaxDistanceSqr(ImGuiIO* self, float* value)
		{
			if(self==nullptr)
			{
				return;
			}
			for (int i = 0; i < 5; i++)
			{
				self->MouseDragMaxDistanceSqr[i] = value[i];
			}
		}
		static inline float FieldGet__PenPressure(ImGuiIO* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<float>();
			}
			return (float)self->PenPressure;
		}
		static inline void FieldSet__PenPressure(ImGuiIO* self, float value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->PenPressure = value;
		}
		static inline bool FieldGet__AppFocusLost(ImGuiIO* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<bool>();
			}
			return (bool)self->AppFocusLost;
		}
		static inline void FieldSet__AppFocusLost(ImGuiIO* self, bool value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->AppFocusLost = value;
		}
		static inline bool FieldGet__AppAcceptingEvents(ImGuiIO* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<bool>();
			}
			return (bool)self->AppAcceptingEvents;
		}
		static inline void FieldSet__AppAcceptingEvents(ImGuiIO* self, bool value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->AppAcceptingEvents = value;
		}
		static inline ImWchar FieldGet__InputQueueSurrogate(ImGuiIO* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<ImWchar>();
			}
			return (ImWchar)self->InputQueueSurrogate;
		}
		static inline void FieldSet__InputQueueSurrogate(ImGuiIO* self, ImWchar value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->InputQueueSurrogate = value;
		}
		static inline float FieldGet__FontGlobalScale(ImGuiIO* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<float>();
			}
			return (float)self->FontGlobalScale;
		}
		static inline void FieldSet__FontGlobalScale(ImGuiIO* self, float value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->FontGlobalScale = value;
		}
		static inline void* FieldGet__GetClipboardTextFn(ImGuiIO* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<void*>();
			}
			return (void*)self->GetClipboardTextFn;
		}
		static inline void FieldSet__GetClipboardTextFn(ImGuiIO* self, char* (*GetClipboardTextFn)(void* arg0))
		{
			if(self==nullptr)
			{
				return;
			}
			*(void**)&(self->GetClipboardTextFn) = (void*)GetClipboardTextFn;
		}
		static inline void* FieldGet__SetClipboardTextFn(ImGuiIO* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<void*>();
			}
			return (void*)self->SetClipboardTextFn;
		}
		static inline void FieldSet__SetClipboardTextFn(ImGuiIO* self, void (*SetClipboardTextFn)(void* arg0,char* arg1))
		{
			if(self==nullptr)
			{
				return;
			}
			*(void**)&(self->SetClipboardTextFn) = (void*)SetClipboardTextFn;
		}
		static inline void* FieldGet__ClipboardUserData(ImGuiIO* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<void*>();
			}
			return (void*)self->ClipboardUserData;
		}
		static inline void FieldSet__ClipboardUserData(ImGuiIO* self, void* value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->ClipboardUserData = value;
		}
		static inline void AddKeyEvent(ImGuiIO* self, ImGuiKey key,bool down)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<void>();
			}
			return (void)self->AddKeyEvent(key, down);
		}
		static inline void AddKeyAnalogEvent(ImGuiIO* self, ImGuiKey key,bool down,float v)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<void>();
			}
			return (void)self->AddKeyAnalogEvent(key, down, v);
		}
		static inline void AddMousePosEvent(ImGuiIO* self, float x,float y)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<void>();
			}
			return (void)self->AddMousePosEvent(x, y);
		}
		static inline void AddMouseButtonEvent(ImGuiIO* self, int button,bool down)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<void>();
			}
			return (void)self->AddMouseButtonEvent(button, down);
		}
		static inline void AddMouseWheelEvent(ImGuiIO* self, float wheel_x,float wheel_y)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<void>();
			}
			return (void)self->AddMouseWheelEvent(wheel_x, wheel_y);
		}
		static inline void AddMouseSourceEvent(ImGuiIO* self, ImGuiMouseSource source)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<void>();
			}
			return (void)self->AddMouseSourceEvent(source);
		}
		static inline void AddMouseViewportEvent(ImGuiIO* self, unsigned int id)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<void>();
			}
			return (void)self->AddMouseViewportEvent(id);
		}
		static inline void AddFocusEvent(ImGuiIO* self, bool focused)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<void>();
			}
			return (void)self->AddFocusEvent(focused);
		}
		static inline void AddInputCharacter(ImGuiIO* self, unsigned int c)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<void>();
			}
			return (void)self->AddInputCharacter(c);
		}
		static inline void AddInputCharacterUTF16(ImGuiIO* self, ImWchar c)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<void>();
			}
			return (void)self->AddInputCharacterUTF16(c);
		}
		static inline void AddInputCharactersUTF8(ImGuiIO* self, const char* str)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<void>();
			}
			return (void)self->AddInputCharactersUTF8(str);
		}
		static inline void SetKeyEventNativeData(ImGuiIO* self, ImGuiKey key,int native_keycode,int native_scancode,int native_legacy_index)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<void>();
			}
			return (void)self->SetKeyEventNativeData(key, native_keycode, native_scancode, native_legacy_index);
		}
		static inline void SetAppAcceptingEvents(ImGuiIO* self, bool accepting_events)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<void>();
			}
			return (void)self->SetAppAcceptingEvents(accepting_events);
		}
		static inline void ClearEventsQueue(ImGuiIO* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<void>();
			}
			return (void)self->ClearEventsQueue();
		}
		static inline void ClearInputKeys(ImGuiIO* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<void>();
			}
			return (void)self->ClearInputKeys();
		}
		static inline void ClearInputMouse(ImGuiIO* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<void>();
			}
			return (void)self->ClearInputMouse();
		}
	};
}


extern "C" VFX_API ImGuiIO* TitanImGui_ImGuiIO_Visitor_CreateInstance_2960189489()
{
	return ImGuiIO_Visitor::CreateInstance();
}


extern "C" VFX_API EngineNS::FRttiStruct* TitanImGui_ImGuiIO_Visitor_GetTypeRtti()
{
	return GetClassObject<ImGuiIO>();
}


extern "C" VFX_API int TitanImGui_ImGuiIO_Visitor_FieldGet__ConfigFlags(ImGuiIO* self)
{
	return ImGuiIO_Visitor::FieldGet__ConfigFlags(self);
}
extern "C" VFX_API void TitanImGui_ImGuiIO_Visitor_FieldSet__ConfigFlags(ImGuiIO* self, int value)
{
	ImGuiIO_Visitor::FieldSet__ConfigFlags(self, value);
}
extern "C" VFX_API int TitanImGui_ImGuiIO_Visitor_FieldGet__BackendFlags(ImGuiIO* self)
{
	return ImGuiIO_Visitor::FieldGet__BackendFlags(self);
}
extern "C" VFX_API void TitanImGui_ImGuiIO_Visitor_FieldSet__BackendFlags(ImGuiIO* self, int value)
{
	ImGuiIO_Visitor::FieldSet__BackendFlags(self, value);
}
extern "C" VFX_API v3dVector2_t TitanImGui_ImGuiIO_Visitor_FieldGet__DisplaySize(ImGuiIO* self)
{
	auto tmp_result = ImGuiIO_Visitor::FieldGet__DisplaySize(self);
	return EngineNS::VReturnValueMarshal<ImVec2,v3dVector2_t>(tmp_result);
}
extern "C" VFX_API void TitanImGui_ImGuiIO_Visitor_FieldSet__DisplaySize(ImGuiIO* self, ImVec2 value)
{
	ImGuiIO_Visitor::FieldSet__DisplaySize(self, value);
}
extern "C" VFX_API v3dVector2_t TitanImGui_ImGuiIO_Visitor_FieldGet__DisplayFramebufferScale(ImGuiIO* self)
{
	auto tmp_result = ImGuiIO_Visitor::FieldGet__DisplayFramebufferScale(self);
	return EngineNS::VReturnValueMarshal<ImVec2,v3dVector2_t>(tmp_result);
}
extern "C" VFX_API void TitanImGui_ImGuiIO_Visitor_FieldSet__DisplayFramebufferScale(ImGuiIO* self, ImVec2 value)
{
	ImGuiIO_Visitor::FieldSet__DisplayFramebufferScale(self, value);
}
extern "C" VFX_API float TitanImGui_ImGuiIO_Visitor_FieldGet__DeltaTime(ImGuiIO* self)
{
	return ImGuiIO_Visitor::FieldGet__DeltaTime(self);
}
extern "C" VFX_API void TitanImGui_ImGuiIO_Visitor_FieldSet__DeltaTime(ImGuiIO* self, float value)
{
	ImGuiIO_Visitor::FieldSet__DeltaTime(self, value);
}
extern "C" VFX_API float TitanImGui_ImGuiIO_Visitor_FieldGet__IniSavingRate(ImGuiIO* self)
{
	return ImGuiIO_Visitor::FieldGet__IniSavingRate(self);
}
extern "C" VFX_API void TitanImGui_ImGuiIO_Visitor_FieldSet__IniSavingRate(ImGuiIO* self, float value)
{
	ImGuiIO_Visitor::FieldSet__IniSavingRate(self, value);
}
extern "C" VFX_API char* TitanImGui_ImGuiIO_Visitor_FieldGet__IniFilename(ImGuiIO* self)
{
	return ImGuiIO_Visitor::FieldGet__IniFilename(self);
}
extern "C" VFX_API void TitanImGui_ImGuiIO_Visitor_FieldSet__IniFilename(ImGuiIO* self, char* value)
{
	ImGuiIO_Visitor::FieldSet__IniFilename(self, value);
}
extern "C" VFX_API char* TitanImGui_ImGuiIO_Visitor_FieldGet__LogFilename(ImGuiIO* self)
{
	return ImGuiIO_Visitor::FieldGet__LogFilename(self);
}
extern "C" VFX_API void TitanImGui_ImGuiIO_Visitor_FieldSet__LogFilename(ImGuiIO* self, char* value)
{
	ImGuiIO_Visitor::FieldSet__LogFilename(self, value);
}
extern "C" VFX_API void* TitanImGui_ImGuiIO_Visitor_FieldGet__UserData(ImGuiIO* self)
{
	return ImGuiIO_Visitor::FieldGet__UserData(self);
}
extern "C" VFX_API void TitanImGui_ImGuiIO_Visitor_FieldSet__UserData(ImGuiIO* self, void* value)
{
	ImGuiIO_Visitor::FieldSet__UserData(self, value);
}
extern "C" VFX_API ImFontAtlas* TitanImGui_ImGuiIO_Visitor_FieldGet__Fonts(ImGuiIO* self)
{
	return ImGuiIO_Visitor::FieldGet__Fonts(self);
}
extern "C" VFX_API void TitanImGui_ImGuiIO_Visitor_FieldSet__Fonts(ImGuiIO* self, ImFontAtlas* value)
{
	ImGuiIO_Visitor::FieldSet__Fonts(self, value);
}
extern "C" VFX_API ImFont* TitanImGui_ImGuiIO_Visitor_FieldGet__FontDefault(ImGuiIO* self)
{
	return ImGuiIO_Visitor::FieldGet__FontDefault(self);
}
extern "C" VFX_API void TitanImGui_ImGuiIO_Visitor_FieldSet__FontDefault(ImGuiIO* self, ImFont* value)
{
	ImGuiIO_Visitor::FieldSet__FontDefault(self, value);
}
extern "C" VFX_API char TitanImGui_ImGuiIO_Visitor_FieldGet__FontAllowUserScaling(ImGuiIO* self)
{
	auto tmp_result = ImGuiIO_Visitor::FieldGet__FontAllowUserScaling(self);
	return EngineNS::VReturnValueMarshal<bool,char>(tmp_result);
}
extern "C" VFX_API void TitanImGui_ImGuiIO_Visitor_FieldSet__FontAllowUserScaling(ImGuiIO* self, bool value)
{
	ImGuiIO_Visitor::FieldSet__FontAllowUserScaling(self, value);
}
extern "C" VFX_API char TitanImGui_ImGuiIO_Visitor_FieldGet__ConfigNavSwapGamepadButtons(ImGuiIO* self)
{
	auto tmp_result = ImGuiIO_Visitor::FieldGet__ConfigNavSwapGamepadButtons(self);
	return EngineNS::VReturnValueMarshal<bool,char>(tmp_result);
}
extern "C" VFX_API void TitanImGui_ImGuiIO_Visitor_FieldSet__ConfigNavSwapGamepadButtons(ImGuiIO* self, bool value)
{
	ImGuiIO_Visitor::FieldSet__ConfigNavSwapGamepadButtons(self, value);
}
extern "C" VFX_API char TitanImGui_ImGuiIO_Visitor_FieldGet__ConfigNavMoveSetMousePos(ImGuiIO* self)
{
	auto tmp_result = ImGuiIO_Visitor::FieldGet__ConfigNavMoveSetMousePos(self);
	return EngineNS::VReturnValueMarshal<bool,char>(tmp_result);
}
extern "C" VFX_API void TitanImGui_ImGuiIO_Visitor_FieldSet__ConfigNavMoveSetMousePos(ImGuiIO* self, bool value)
{
	ImGuiIO_Visitor::FieldSet__ConfigNavMoveSetMousePos(self, value);
}
extern "C" VFX_API char TitanImGui_ImGuiIO_Visitor_FieldGet__ConfigNavCaptureKeyboard(ImGuiIO* self)
{
	auto tmp_result = ImGuiIO_Visitor::FieldGet__ConfigNavCaptureKeyboard(self);
	return EngineNS::VReturnValueMarshal<bool,char>(tmp_result);
}
extern "C" VFX_API void TitanImGui_ImGuiIO_Visitor_FieldSet__ConfigNavCaptureKeyboard(ImGuiIO* self, bool value)
{
	ImGuiIO_Visitor::FieldSet__ConfigNavCaptureKeyboard(self, value);
}
extern "C" VFX_API char TitanImGui_ImGuiIO_Visitor_FieldGet__ConfigNavEscapeClearFocusItem(ImGuiIO* self)
{
	auto tmp_result = ImGuiIO_Visitor::FieldGet__ConfigNavEscapeClearFocusItem(self);
	return EngineNS::VReturnValueMarshal<bool,char>(tmp_result);
}
extern "C" VFX_API void TitanImGui_ImGuiIO_Visitor_FieldSet__ConfigNavEscapeClearFocusItem(ImGuiIO* self, bool value)
{
	ImGuiIO_Visitor::FieldSet__ConfigNavEscapeClearFocusItem(self, value);
}
extern "C" VFX_API char TitanImGui_ImGuiIO_Visitor_FieldGet__ConfigNavEscapeClearFocusWindow(ImGuiIO* self)
{
	auto tmp_result = ImGuiIO_Visitor::FieldGet__ConfigNavEscapeClearFocusWindow(self);
	return EngineNS::VReturnValueMarshal<bool,char>(tmp_result);
}
extern "C" VFX_API void TitanImGui_ImGuiIO_Visitor_FieldSet__ConfigNavEscapeClearFocusWindow(ImGuiIO* self, bool value)
{
	ImGuiIO_Visitor::FieldSet__ConfigNavEscapeClearFocusWindow(self, value);
}
extern "C" VFX_API char TitanImGui_ImGuiIO_Visitor_FieldGet__ConfigNavCursorVisibleAuto(ImGuiIO* self)
{
	auto tmp_result = ImGuiIO_Visitor::FieldGet__ConfigNavCursorVisibleAuto(self);
	return EngineNS::VReturnValueMarshal<bool,char>(tmp_result);
}
extern "C" VFX_API void TitanImGui_ImGuiIO_Visitor_FieldSet__ConfigNavCursorVisibleAuto(ImGuiIO* self, bool value)
{
	ImGuiIO_Visitor::FieldSet__ConfigNavCursorVisibleAuto(self, value);
}
extern "C" VFX_API char TitanImGui_ImGuiIO_Visitor_FieldGet__ConfigNavCursorVisibleAlways(ImGuiIO* self)
{
	auto tmp_result = ImGuiIO_Visitor::FieldGet__ConfigNavCursorVisibleAlways(self);
	return EngineNS::VReturnValueMarshal<bool,char>(tmp_result);
}
extern "C" VFX_API void TitanImGui_ImGuiIO_Visitor_FieldSet__ConfigNavCursorVisibleAlways(ImGuiIO* self, bool value)
{
	ImGuiIO_Visitor::FieldSet__ConfigNavCursorVisibleAlways(self, value);
}
extern "C" VFX_API char TitanImGui_ImGuiIO_Visitor_FieldGet__ConfigDockingNoSplit(ImGuiIO* self)
{
	auto tmp_result = ImGuiIO_Visitor::FieldGet__ConfigDockingNoSplit(self);
	return EngineNS::VReturnValueMarshal<bool,char>(tmp_result);
}
extern "C" VFX_API void TitanImGui_ImGuiIO_Visitor_FieldSet__ConfigDockingNoSplit(ImGuiIO* self, bool value)
{
	ImGuiIO_Visitor::FieldSet__ConfigDockingNoSplit(self, value);
}
extern "C" VFX_API char TitanImGui_ImGuiIO_Visitor_FieldGet__ConfigDockingNoDockingOver(ImGuiIO* self)
{
	auto tmp_result = ImGuiIO_Visitor::FieldGet__ConfigDockingNoDockingOver(self);
	return EngineNS::VReturnValueMarshal<bool,char>(tmp_result);
}
extern "C" VFX_API void TitanImGui_ImGuiIO_Visitor_FieldSet__ConfigDockingNoDockingOver(ImGuiIO* self, bool value)
{
	ImGuiIO_Visitor::FieldSet__ConfigDockingNoDockingOver(self, value);
}
extern "C" VFX_API char TitanImGui_ImGuiIO_Visitor_FieldGet__ConfigDockingWithShift(ImGuiIO* self)
{
	auto tmp_result = ImGuiIO_Visitor::FieldGet__ConfigDockingWithShift(self);
	return EngineNS::VReturnValueMarshal<bool,char>(tmp_result);
}
extern "C" VFX_API void TitanImGui_ImGuiIO_Visitor_FieldSet__ConfigDockingWithShift(ImGuiIO* self, bool value)
{
	ImGuiIO_Visitor::FieldSet__ConfigDockingWithShift(self, value);
}
extern "C" VFX_API char TitanImGui_ImGuiIO_Visitor_FieldGet__ConfigDockingAlwaysTabBar(ImGuiIO* self)
{
	auto tmp_result = ImGuiIO_Visitor::FieldGet__ConfigDockingAlwaysTabBar(self);
	return EngineNS::VReturnValueMarshal<bool,char>(tmp_result);
}
extern "C" VFX_API void TitanImGui_ImGuiIO_Visitor_FieldSet__ConfigDockingAlwaysTabBar(ImGuiIO* self, bool value)
{
	ImGuiIO_Visitor::FieldSet__ConfigDockingAlwaysTabBar(self, value);
}
extern "C" VFX_API char TitanImGui_ImGuiIO_Visitor_FieldGet__ConfigDockingTransparentPayload(ImGuiIO* self)
{
	auto tmp_result = ImGuiIO_Visitor::FieldGet__ConfigDockingTransparentPayload(self);
	return EngineNS::VReturnValueMarshal<bool,char>(tmp_result);
}
extern "C" VFX_API void TitanImGui_ImGuiIO_Visitor_FieldSet__ConfigDockingTransparentPayload(ImGuiIO* self, bool value)
{
	ImGuiIO_Visitor::FieldSet__ConfigDockingTransparentPayload(self, value);
}
extern "C" VFX_API char TitanImGui_ImGuiIO_Visitor_FieldGet__ConfigViewportsNoAutoMerge(ImGuiIO* self)
{
	auto tmp_result = ImGuiIO_Visitor::FieldGet__ConfigViewportsNoAutoMerge(self);
	return EngineNS::VReturnValueMarshal<bool,char>(tmp_result);
}
extern "C" VFX_API void TitanImGui_ImGuiIO_Visitor_FieldSet__ConfigViewportsNoAutoMerge(ImGuiIO* self, bool value)
{
	ImGuiIO_Visitor::FieldSet__ConfigViewportsNoAutoMerge(self, value);
}
extern "C" VFX_API char TitanImGui_ImGuiIO_Visitor_FieldGet__ConfigViewportsNoTaskBarIcon(ImGuiIO* self)
{
	auto tmp_result = ImGuiIO_Visitor::FieldGet__ConfigViewportsNoTaskBarIcon(self);
	return EngineNS::VReturnValueMarshal<bool,char>(tmp_result);
}
extern "C" VFX_API void TitanImGui_ImGuiIO_Visitor_FieldSet__ConfigViewportsNoTaskBarIcon(ImGuiIO* self, bool value)
{
	ImGuiIO_Visitor::FieldSet__ConfigViewportsNoTaskBarIcon(self, value);
}
extern "C" VFX_API char TitanImGui_ImGuiIO_Visitor_FieldGet__ConfigViewportsNoDecoration(ImGuiIO* self)
{
	auto tmp_result = ImGuiIO_Visitor::FieldGet__ConfigViewportsNoDecoration(self);
	return EngineNS::VReturnValueMarshal<bool,char>(tmp_result);
}
extern "C" VFX_API void TitanImGui_ImGuiIO_Visitor_FieldSet__ConfigViewportsNoDecoration(ImGuiIO* self, bool value)
{
	ImGuiIO_Visitor::FieldSet__ConfigViewportsNoDecoration(self, value);
}
extern "C" VFX_API char TitanImGui_ImGuiIO_Visitor_FieldGet__ConfigViewportsNoDefaultParent(ImGuiIO* self)
{
	auto tmp_result = ImGuiIO_Visitor::FieldGet__ConfigViewportsNoDefaultParent(self);
	return EngineNS::VReturnValueMarshal<bool,char>(tmp_result);
}
extern "C" VFX_API void TitanImGui_ImGuiIO_Visitor_FieldSet__ConfigViewportsNoDefaultParent(ImGuiIO* self, bool value)
{
	ImGuiIO_Visitor::FieldSet__ConfigViewportsNoDefaultParent(self, value);
}
extern "C" VFX_API char TitanImGui_ImGuiIO_Visitor_FieldGet__ConfigViewportsPlatformFocusSetsImGuiFocus(ImGuiIO* self)
{
	auto tmp_result = ImGuiIO_Visitor::FieldGet__ConfigViewportsPlatformFocusSetsImGuiFocus(self);
	return EngineNS::VReturnValueMarshal<bool,char>(tmp_result);
}
extern "C" VFX_API void TitanImGui_ImGuiIO_Visitor_FieldSet__ConfigViewportsPlatformFocusSetsImGuiFocus(ImGuiIO* self, bool value)
{
	ImGuiIO_Visitor::FieldSet__ConfigViewportsPlatformFocusSetsImGuiFocus(self, value);
}
extern "C" VFX_API char TitanImGui_ImGuiIO_Visitor_FieldGet__ConfigDpiScaleFonts(ImGuiIO* self)
{
	auto tmp_result = ImGuiIO_Visitor::FieldGet__ConfigDpiScaleFonts(self);
	return EngineNS::VReturnValueMarshal<bool,char>(tmp_result);
}
extern "C" VFX_API void TitanImGui_ImGuiIO_Visitor_FieldSet__ConfigDpiScaleFonts(ImGuiIO* self, bool value)
{
	ImGuiIO_Visitor::FieldSet__ConfigDpiScaleFonts(self, value);
}
extern "C" VFX_API char TitanImGui_ImGuiIO_Visitor_FieldGet__ConfigDpiScaleViewports(ImGuiIO* self)
{
	auto tmp_result = ImGuiIO_Visitor::FieldGet__ConfigDpiScaleViewports(self);
	return EngineNS::VReturnValueMarshal<bool,char>(tmp_result);
}
extern "C" VFX_API void TitanImGui_ImGuiIO_Visitor_FieldSet__ConfigDpiScaleViewports(ImGuiIO* self, bool value)
{
	ImGuiIO_Visitor::FieldSet__ConfigDpiScaleViewports(self, value);
}
extern "C" VFX_API char TitanImGui_ImGuiIO_Visitor_FieldGet__MouseDrawCursor(ImGuiIO* self)
{
	auto tmp_result = ImGuiIO_Visitor::FieldGet__MouseDrawCursor(self);
	return EngineNS::VReturnValueMarshal<bool,char>(tmp_result);
}
extern "C" VFX_API void TitanImGui_ImGuiIO_Visitor_FieldSet__MouseDrawCursor(ImGuiIO* self, bool value)
{
	ImGuiIO_Visitor::FieldSet__MouseDrawCursor(self, value);
}
extern "C" VFX_API char TitanImGui_ImGuiIO_Visitor_FieldGet__ConfigMacOSXBehaviors(ImGuiIO* self)
{
	auto tmp_result = ImGuiIO_Visitor::FieldGet__ConfigMacOSXBehaviors(self);
	return EngineNS::VReturnValueMarshal<bool,char>(tmp_result);
}
extern "C" VFX_API void TitanImGui_ImGuiIO_Visitor_FieldSet__ConfigMacOSXBehaviors(ImGuiIO* self, bool value)
{
	ImGuiIO_Visitor::FieldSet__ConfigMacOSXBehaviors(self, value);
}
extern "C" VFX_API char TitanImGui_ImGuiIO_Visitor_FieldGet__ConfigInputTrickleEventQueue(ImGuiIO* self)
{
	auto tmp_result = ImGuiIO_Visitor::FieldGet__ConfigInputTrickleEventQueue(self);
	return EngineNS::VReturnValueMarshal<bool,char>(tmp_result);
}
extern "C" VFX_API void TitanImGui_ImGuiIO_Visitor_FieldSet__ConfigInputTrickleEventQueue(ImGuiIO* self, bool value)
{
	ImGuiIO_Visitor::FieldSet__ConfigInputTrickleEventQueue(self, value);
}
extern "C" VFX_API char TitanImGui_ImGuiIO_Visitor_FieldGet__ConfigInputTextCursorBlink(ImGuiIO* self)
{
	auto tmp_result = ImGuiIO_Visitor::FieldGet__ConfigInputTextCursorBlink(self);
	return EngineNS::VReturnValueMarshal<bool,char>(tmp_result);
}
extern "C" VFX_API void TitanImGui_ImGuiIO_Visitor_FieldSet__ConfigInputTextCursorBlink(ImGuiIO* self, bool value)
{
	ImGuiIO_Visitor::FieldSet__ConfigInputTextCursorBlink(self, value);
}
extern "C" VFX_API char TitanImGui_ImGuiIO_Visitor_FieldGet__ConfigInputTextEnterKeepActive(ImGuiIO* self)
{
	auto tmp_result = ImGuiIO_Visitor::FieldGet__ConfigInputTextEnterKeepActive(self);
	return EngineNS::VReturnValueMarshal<bool,char>(tmp_result);
}
extern "C" VFX_API void TitanImGui_ImGuiIO_Visitor_FieldSet__ConfigInputTextEnterKeepActive(ImGuiIO* self, bool value)
{
	ImGuiIO_Visitor::FieldSet__ConfigInputTextEnterKeepActive(self, value);
}
extern "C" VFX_API char TitanImGui_ImGuiIO_Visitor_FieldGet__ConfigDragClickToInputText(ImGuiIO* self)
{
	auto tmp_result = ImGuiIO_Visitor::FieldGet__ConfigDragClickToInputText(self);
	return EngineNS::VReturnValueMarshal<bool,char>(tmp_result);
}
extern "C" VFX_API void TitanImGui_ImGuiIO_Visitor_FieldSet__ConfigDragClickToInputText(ImGuiIO* self, bool value)
{
	ImGuiIO_Visitor::FieldSet__ConfigDragClickToInputText(self, value);
}
extern "C" VFX_API char TitanImGui_ImGuiIO_Visitor_FieldGet__ConfigWindowsResizeFromEdges(ImGuiIO* self)
{
	auto tmp_result = ImGuiIO_Visitor::FieldGet__ConfigWindowsResizeFromEdges(self);
	return EngineNS::VReturnValueMarshal<bool,char>(tmp_result);
}
extern "C" VFX_API void TitanImGui_ImGuiIO_Visitor_FieldSet__ConfigWindowsResizeFromEdges(ImGuiIO* self, bool value)
{
	ImGuiIO_Visitor::FieldSet__ConfigWindowsResizeFromEdges(self, value);
}
extern "C" VFX_API char TitanImGui_ImGuiIO_Visitor_FieldGet__ConfigWindowsMoveFromTitleBarOnly(ImGuiIO* self)
{
	auto tmp_result = ImGuiIO_Visitor::FieldGet__ConfigWindowsMoveFromTitleBarOnly(self);
	return EngineNS::VReturnValueMarshal<bool,char>(tmp_result);
}
extern "C" VFX_API void TitanImGui_ImGuiIO_Visitor_FieldSet__ConfigWindowsMoveFromTitleBarOnly(ImGuiIO* self, bool value)
{
	ImGuiIO_Visitor::FieldSet__ConfigWindowsMoveFromTitleBarOnly(self, value);
}
extern "C" VFX_API char TitanImGui_ImGuiIO_Visitor_FieldGet__ConfigWindowsCopyContentsWithCtrlC(ImGuiIO* self)
{
	auto tmp_result = ImGuiIO_Visitor::FieldGet__ConfigWindowsCopyContentsWithCtrlC(self);
	return EngineNS::VReturnValueMarshal<bool,char>(tmp_result);
}
extern "C" VFX_API void TitanImGui_ImGuiIO_Visitor_FieldSet__ConfigWindowsCopyContentsWithCtrlC(ImGuiIO* self, bool value)
{
	ImGuiIO_Visitor::FieldSet__ConfigWindowsCopyContentsWithCtrlC(self, value);
}
extern "C" VFX_API char TitanImGui_ImGuiIO_Visitor_FieldGet__ConfigScrollbarScrollByPage(ImGuiIO* self)
{
	auto tmp_result = ImGuiIO_Visitor::FieldGet__ConfigScrollbarScrollByPage(self);
	return EngineNS::VReturnValueMarshal<bool,char>(tmp_result);
}
extern "C" VFX_API void TitanImGui_ImGuiIO_Visitor_FieldSet__ConfigScrollbarScrollByPage(ImGuiIO* self, bool value)
{
	ImGuiIO_Visitor::FieldSet__ConfigScrollbarScrollByPage(self, value);
}
extern "C" VFX_API float TitanImGui_ImGuiIO_Visitor_FieldGet__ConfigMemoryCompactTimer(ImGuiIO* self)
{
	return ImGuiIO_Visitor::FieldGet__ConfigMemoryCompactTimer(self);
}
extern "C" VFX_API void TitanImGui_ImGuiIO_Visitor_FieldSet__ConfigMemoryCompactTimer(ImGuiIO* self, float value)
{
	ImGuiIO_Visitor::FieldSet__ConfigMemoryCompactTimer(self, value);
}
extern "C" VFX_API float TitanImGui_ImGuiIO_Visitor_FieldGet__MouseDoubleClickTime(ImGuiIO* self)
{
	return ImGuiIO_Visitor::FieldGet__MouseDoubleClickTime(self);
}
extern "C" VFX_API void TitanImGui_ImGuiIO_Visitor_FieldSet__MouseDoubleClickTime(ImGuiIO* self, float value)
{
	ImGuiIO_Visitor::FieldSet__MouseDoubleClickTime(self, value);
}
extern "C" VFX_API float TitanImGui_ImGuiIO_Visitor_FieldGet__MouseDoubleClickMaxDist(ImGuiIO* self)
{
	return ImGuiIO_Visitor::FieldGet__MouseDoubleClickMaxDist(self);
}
extern "C" VFX_API void TitanImGui_ImGuiIO_Visitor_FieldSet__MouseDoubleClickMaxDist(ImGuiIO* self, float value)
{
	ImGuiIO_Visitor::FieldSet__MouseDoubleClickMaxDist(self, value);
}
extern "C" VFX_API float TitanImGui_ImGuiIO_Visitor_FieldGet__MouseDragThreshold(ImGuiIO* self)
{
	return ImGuiIO_Visitor::FieldGet__MouseDragThreshold(self);
}
extern "C" VFX_API void TitanImGui_ImGuiIO_Visitor_FieldSet__MouseDragThreshold(ImGuiIO* self, float value)
{
	ImGuiIO_Visitor::FieldSet__MouseDragThreshold(self, value);
}
extern "C" VFX_API float TitanImGui_ImGuiIO_Visitor_FieldGet__KeyRepeatDelay(ImGuiIO* self)
{
	return ImGuiIO_Visitor::FieldGet__KeyRepeatDelay(self);
}
extern "C" VFX_API void TitanImGui_ImGuiIO_Visitor_FieldSet__KeyRepeatDelay(ImGuiIO* self, float value)
{
	ImGuiIO_Visitor::FieldSet__KeyRepeatDelay(self, value);
}
extern "C" VFX_API float TitanImGui_ImGuiIO_Visitor_FieldGet__KeyRepeatRate(ImGuiIO* self)
{
	return ImGuiIO_Visitor::FieldGet__KeyRepeatRate(self);
}
extern "C" VFX_API void TitanImGui_ImGuiIO_Visitor_FieldSet__KeyRepeatRate(ImGuiIO* self, float value)
{
	ImGuiIO_Visitor::FieldSet__KeyRepeatRate(self, value);
}
extern "C" VFX_API char TitanImGui_ImGuiIO_Visitor_FieldGet__ConfigErrorRecovery(ImGuiIO* self)
{
	auto tmp_result = ImGuiIO_Visitor::FieldGet__ConfigErrorRecovery(self);
	return EngineNS::VReturnValueMarshal<bool,char>(tmp_result);
}
extern "C" VFX_API void TitanImGui_ImGuiIO_Visitor_FieldSet__ConfigErrorRecovery(ImGuiIO* self, bool value)
{
	ImGuiIO_Visitor::FieldSet__ConfigErrorRecovery(self, value);
}
extern "C" VFX_API char TitanImGui_ImGuiIO_Visitor_FieldGet__ConfigErrorRecoveryEnableAssert(ImGuiIO* self)
{
	auto tmp_result = ImGuiIO_Visitor::FieldGet__ConfigErrorRecoveryEnableAssert(self);
	return EngineNS::VReturnValueMarshal<bool,char>(tmp_result);
}
extern "C" VFX_API void TitanImGui_ImGuiIO_Visitor_FieldSet__ConfigErrorRecoveryEnableAssert(ImGuiIO* self, bool value)
{
	ImGuiIO_Visitor::FieldSet__ConfigErrorRecoveryEnableAssert(self, value);
}
extern "C" VFX_API char TitanImGui_ImGuiIO_Visitor_FieldGet__ConfigErrorRecoveryEnableDebugLog(ImGuiIO* self)
{
	auto tmp_result = ImGuiIO_Visitor::FieldGet__ConfigErrorRecoveryEnableDebugLog(self);
	return EngineNS::VReturnValueMarshal<bool,char>(tmp_result);
}
extern "C" VFX_API void TitanImGui_ImGuiIO_Visitor_FieldSet__ConfigErrorRecoveryEnableDebugLog(ImGuiIO* self, bool value)
{
	ImGuiIO_Visitor::FieldSet__ConfigErrorRecoveryEnableDebugLog(self, value);
}
extern "C" VFX_API char TitanImGui_ImGuiIO_Visitor_FieldGet__ConfigErrorRecoveryEnableTooltip(ImGuiIO* self)
{
	auto tmp_result = ImGuiIO_Visitor::FieldGet__ConfigErrorRecoveryEnableTooltip(self);
	return EngineNS::VReturnValueMarshal<bool,char>(tmp_result);
}
extern "C" VFX_API void TitanImGui_ImGuiIO_Visitor_FieldSet__ConfigErrorRecoveryEnableTooltip(ImGuiIO* self, bool value)
{
	ImGuiIO_Visitor::FieldSet__ConfigErrorRecoveryEnableTooltip(self, value);
}
extern "C" VFX_API char TitanImGui_ImGuiIO_Visitor_FieldGet__ConfigDebugIsDebuggerPresent(ImGuiIO* self)
{
	auto tmp_result = ImGuiIO_Visitor::FieldGet__ConfigDebugIsDebuggerPresent(self);
	return EngineNS::VReturnValueMarshal<bool,char>(tmp_result);
}
extern "C" VFX_API void TitanImGui_ImGuiIO_Visitor_FieldSet__ConfigDebugIsDebuggerPresent(ImGuiIO* self, bool value)
{
	ImGuiIO_Visitor::FieldSet__ConfigDebugIsDebuggerPresent(self, value);
}
extern "C" VFX_API char TitanImGui_ImGuiIO_Visitor_FieldGet__ConfigDebugHighlightIdConflicts(ImGuiIO* self)
{
	auto tmp_result = ImGuiIO_Visitor::FieldGet__ConfigDebugHighlightIdConflicts(self);
	return EngineNS::VReturnValueMarshal<bool,char>(tmp_result);
}
extern "C" VFX_API void TitanImGui_ImGuiIO_Visitor_FieldSet__ConfigDebugHighlightIdConflicts(ImGuiIO* self, bool value)
{
	ImGuiIO_Visitor::FieldSet__ConfigDebugHighlightIdConflicts(self, value);
}
extern "C" VFX_API char TitanImGui_ImGuiIO_Visitor_FieldGet__ConfigDebugHighlightIdConflictsShowItemPicker(ImGuiIO* self)
{
	auto tmp_result = ImGuiIO_Visitor::FieldGet__ConfigDebugHighlightIdConflictsShowItemPicker(self);
	return EngineNS::VReturnValueMarshal<bool,char>(tmp_result);
}
extern "C" VFX_API void TitanImGui_ImGuiIO_Visitor_FieldSet__ConfigDebugHighlightIdConflictsShowItemPicker(ImGuiIO* self, bool value)
{
	ImGuiIO_Visitor::FieldSet__ConfigDebugHighlightIdConflictsShowItemPicker(self, value);
}
extern "C" VFX_API char TitanImGui_ImGuiIO_Visitor_FieldGet__ConfigDebugBeginReturnValueOnce(ImGuiIO* self)
{
	auto tmp_result = ImGuiIO_Visitor::FieldGet__ConfigDebugBeginReturnValueOnce(self);
	return EngineNS::VReturnValueMarshal<bool,char>(tmp_result);
}
extern "C" VFX_API void TitanImGui_ImGuiIO_Visitor_FieldSet__ConfigDebugBeginReturnValueOnce(ImGuiIO* self, bool value)
{
	ImGuiIO_Visitor::FieldSet__ConfigDebugBeginReturnValueOnce(self, value);
}
extern "C" VFX_API char TitanImGui_ImGuiIO_Visitor_FieldGet__ConfigDebugBeginReturnValueLoop(ImGuiIO* self)
{
	auto tmp_result = ImGuiIO_Visitor::FieldGet__ConfigDebugBeginReturnValueLoop(self);
	return EngineNS::VReturnValueMarshal<bool,char>(tmp_result);
}
extern "C" VFX_API void TitanImGui_ImGuiIO_Visitor_FieldSet__ConfigDebugBeginReturnValueLoop(ImGuiIO* self, bool value)
{
	ImGuiIO_Visitor::FieldSet__ConfigDebugBeginReturnValueLoop(self, value);
}
extern "C" VFX_API char TitanImGui_ImGuiIO_Visitor_FieldGet__ConfigDebugIgnoreFocusLoss(ImGuiIO* self)
{
	auto tmp_result = ImGuiIO_Visitor::FieldGet__ConfigDebugIgnoreFocusLoss(self);
	return EngineNS::VReturnValueMarshal<bool,char>(tmp_result);
}
extern "C" VFX_API void TitanImGui_ImGuiIO_Visitor_FieldSet__ConfigDebugIgnoreFocusLoss(ImGuiIO* self, bool value)
{
	ImGuiIO_Visitor::FieldSet__ConfigDebugIgnoreFocusLoss(self, value);
}
extern "C" VFX_API char TitanImGui_ImGuiIO_Visitor_FieldGet__ConfigDebugIniSettings(ImGuiIO* self)
{
	auto tmp_result = ImGuiIO_Visitor::FieldGet__ConfigDebugIniSettings(self);
	return EngineNS::VReturnValueMarshal<bool,char>(tmp_result);
}
extern "C" VFX_API void TitanImGui_ImGuiIO_Visitor_FieldSet__ConfigDebugIniSettings(ImGuiIO* self, bool value)
{
	ImGuiIO_Visitor::FieldSet__ConfigDebugIniSettings(self, value);
}
extern "C" VFX_API char* TitanImGui_ImGuiIO_Visitor_FieldGet__BackendPlatformName(ImGuiIO* self)
{
	return ImGuiIO_Visitor::FieldGet__BackendPlatformName(self);
}
extern "C" VFX_API void TitanImGui_ImGuiIO_Visitor_FieldSet__BackendPlatformName(ImGuiIO* self, char* value)
{
	ImGuiIO_Visitor::FieldSet__BackendPlatformName(self, value);
}
extern "C" VFX_API char* TitanImGui_ImGuiIO_Visitor_FieldGet__BackendRendererName(ImGuiIO* self)
{
	return ImGuiIO_Visitor::FieldGet__BackendRendererName(self);
}
extern "C" VFX_API void TitanImGui_ImGuiIO_Visitor_FieldSet__BackendRendererName(ImGuiIO* self, char* value)
{
	ImGuiIO_Visitor::FieldSet__BackendRendererName(self, value);
}
extern "C" VFX_API void* TitanImGui_ImGuiIO_Visitor_FieldGet__BackendPlatformUserData(ImGuiIO* self)
{
	return ImGuiIO_Visitor::FieldGet__BackendPlatformUserData(self);
}
extern "C" VFX_API void TitanImGui_ImGuiIO_Visitor_FieldSet__BackendPlatformUserData(ImGuiIO* self, void* value)
{
	ImGuiIO_Visitor::FieldSet__BackendPlatformUserData(self, value);
}
extern "C" VFX_API void* TitanImGui_ImGuiIO_Visitor_FieldGet__BackendRendererUserData(ImGuiIO* self)
{
	return ImGuiIO_Visitor::FieldGet__BackendRendererUserData(self);
}
extern "C" VFX_API void TitanImGui_ImGuiIO_Visitor_FieldSet__BackendRendererUserData(ImGuiIO* self, void* value)
{
	ImGuiIO_Visitor::FieldSet__BackendRendererUserData(self, value);
}
extern "C" VFX_API void* TitanImGui_ImGuiIO_Visitor_FieldGet__BackendLanguageUserData(ImGuiIO* self)
{
	return ImGuiIO_Visitor::FieldGet__BackendLanguageUserData(self);
}
extern "C" VFX_API void TitanImGui_ImGuiIO_Visitor_FieldSet__BackendLanguageUserData(ImGuiIO* self, void* value)
{
	ImGuiIO_Visitor::FieldSet__BackendLanguageUserData(self, value);
}
extern "C" VFX_API char TitanImGui_ImGuiIO_Visitor_FieldGet__WantCaptureMouse(ImGuiIO* self)
{
	auto tmp_result = ImGuiIO_Visitor::FieldGet__WantCaptureMouse(self);
	return EngineNS::VReturnValueMarshal<bool,char>(tmp_result);
}
extern "C" VFX_API void TitanImGui_ImGuiIO_Visitor_FieldSet__WantCaptureMouse(ImGuiIO* self, bool value)
{
	ImGuiIO_Visitor::FieldSet__WantCaptureMouse(self, value);
}
extern "C" VFX_API char TitanImGui_ImGuiIO_Visitor_FieldGet__WantCaptureKeyboard(ImGuiIO* self)
{
	auto tmp_result = ImGuiIO_Visitor::FieldGet__WantCaptureKeyboard(self);
	return EngineNS::VReturnValueMarshal<bool,char>(tmp_result);
}
extern "C" VFX_API void TitanImGui_ImGuiIO_Visitor_FieldSet__WantCaptureKeyboard(ImGuiIO* self, bool value)
{
	ImGuiIO_Visitor::FieldSet__WantCaptureKeyboard(self, value);
}
extern "C" VFX_API char TitanImGui_ImGuiIO_Visitor_FieldGet__WantTextInput(ImGuiIO* self)
{
	auto tmp_result = ImGuiIO_Visitor::FieldGet__WantTextInput(self);
	return EngineNS::VReturnValueMarshal<bool,char>(tmp_result);
}
extern "C" VFX_API void TitanImGui_ImGuiIO_Visitor_FieldSet__WantTextInput(ImGuiIO* self, bool value)
{
	ImGuiIO_Visitor::FieldSet__WantTextInput(self, value);
}
extern "C" VFX_API char TitanImGui_ImGuiIO_Visitor_FieldGet__WantSetMousePos(ImGuiIO* self)
{
	auto tmp_result = ImGuiIO_Visitor::FieldGet__WantSetMousePos(self);
	return EngineNS::VReturnValueMarshal<bool,char>(tmp_result);
}
extern "C" VFX_API void TitanImGui_ImGuiIO_Visitor_FieldSet__WantSetMousePos(ImGuiIO* self, bool value)
{
	ImGuiIO_Visitor::FieldSet__WantSetMousePos(self, value);
}
extern "C" VFX_API char TitanImGui_ImGuiIO_Visitor_FieldGet__WantSaveIniSettings(ImGuiIO* self)
{
	auto tmp_result = ImGuiIO_Visitor::FieldGet__WantSaveIniSettings(self);
	return EngineNS::VReturnValueMarshal<bool,char>(tmp_result);
}
extern "C" VFX_API void TitanImGui_ImGuiIO_Visitor_FieldSet__WantSaveIniSettings(ImGuiIO* self, bool value)
{
	ImGuiIO_Visitor::FieldSet__WantSaveIniSettings(self, value);
}
extern "C" VFX_API char TitanImGui_ImGuiIO_Visitor_FieldGet__NavActive(ImGuiIO* self)
{
	auto tmp_result = ImGuiIO_Visitor::FieldGet__NavActive(self);
	return EngineNS::VReturnValueMarshal<bool,char>(tmp_result);
}
extern "C" VFX_API void TitanImGui_ImGuiIO_Visitor_FieldSet__NavActive(ImGuiIO* self, bool value)
{
	ImGuiIO_Visitor::FieldSet__NavActive(self, value);
}
extern "C" VFX_API char TitanImGui_ImGuiIO_Visitor_FieldGet__NavVisible(ImGuiIO* self)
{
	auto tmp_result = ImGuiIO_Visitor::FieldGet__NavVisible(self);
	return EngineNS::VReturnValueMarshal<bool,char>(tmp_result);
}
extern "C" VFX_API void TitanImGui_ImGuiIO_Visitor_FieldSet__NavVisible(ImGuiIO* self, bool value)
{
	ImGuiIO_Visitor::FieldSet__NavVisible(self, value);
}
extern "C" VFX_API float TitanImGui_ImGuiIO_Visitor_FieldGet__Framerate(ImGuiIO* self)
{
	return ImGuiIO_Visitor::FieldGet__Framerate(self);
}
extern "C" VFX_API void TitanImGui_ImGuiIO_Visitor_FieldSet__Framerate(ImGuiIO* self, float value)
{
	ImGuiIO_Visitor::FieldSet__Framerate(self, value);
}
extern "C" VFX_API int TitanImGui_ImGuiIO_Visitor_FieldGet__MetricsRenderVertices(ImGuiIO* self)
{
	return ImGuiIO_Visitor::FieldGet__MetricsRenderVertices(self);
}
extern "C" VFX_API void TitanImGui_ImGuiIO_Visitor_FieldSet__MetricsRenderVertices(ImGuiIO* self, int value)
{
	ImGuiIO_Visitor::FieldSet__MetricsRenderVertices(self, value);
}
extern "C" VFX_API int TitanImGui_ImGuiIO_Visitor_FieldGet__MetricsRenderIndices(ImGuiIO* self)
{
	return ImGuiIO_Visitor::FieldGet__MetricsRenderIndices(self);
}
extern "C" VFX_API void TitanImGui_ImGuiIO_Visitor_FieldSet__MetricsRenderIndices(ImGuiIO* self, int value)
{
	ImGuiIO_Visitor::FieldSet__MetricsRenderIndices(self, value);
}
extern "C" VFX_API int TitanImGui_ImGuiIO_Visitor_FieldGet__MetricsRenderWindows(ImGuiIO* self)
{
	return ImGuiIO_Visitor::FieldGet__MetricsRenderWindows(self);
}
extern "C" VFX_API void TitanImGui_ImGuiIO_Visitor_FieldSet__MetricsRenderWindows(ImGuiIO* self, int value)
{
	ImGuiIO_Visitor::FieldSet__MetricsRenderWindows(self, value);
}
extern "C" VFX_API int TitanImGui_ImGuiIO_Visitor_FieldGet__MetricsActiveWindows(ImGuiIO* self)
{
	return ImGuiIO_Visitor::FieldGet__MetricsActiveWindows(self);
}
extern "C" VFX_API void TitanImGui_ImGuiIO_Visitor_FieldSet__MetricsActiveWindows(ImGuiIO* self, int value)
{
	ImGuiIO_Visitor::FieldSet__MetricsActiveWindows(self, value);
}
extern "C" VFX_API v3dVector2_t TitanImGui_ImGuiIO_Visitor_FieldGet__MouseDelta(ImGuiIO* self)
{
	auto tmp_result = ImGuiIO_Visitor::FieldGet__MouseDelta(self);
	return EngineNS::VReturnValueMarshal<ImVec2,v3dVector2_t>(tmp_result);
}
extern "C" VFX_API void TitanImGui_ImGuiIO_Visitor_FieldSet__MouseDelta(ImGuiIO* self, ImVec2 value)
{
	ImGuiIO_Visitor::FieldSet__MouseDelta(self, value);
}
extern "C" VFX_API v3dVector2_t TitanImGui_ImGuiIO_Visitor_FieldGet__MousePos(ImGuiIO* self)
{
	auto tmp_result = ImGuiIO_Visitor::FieldGet__MousePos(self);
	return EngineNS::VReturnValueMarshal<ImVec2,v3dVector2_t>(tmp_result);
}
extern "C" VFX_API void TitanImGui_ImGuiIO_Visitor_FieldSet__MousePos(ImGuiIO* self, ImVec2 value)
{
	ImGuiIO_Visitor::FieldSet__MousePos(self, value);
}
extern "C" VFX_API bool* TitanImGui_ImGuiIO_Visitor_FieldGet__MouseDown(ImGuiIO* self)
{
	return ImGuiIO_Visitor::FieldGet__MouseDown(self);
}
extern "C" VFX_API void TitanImGui_ImGuiIO_Visitor_FieldSet__MouseDown(ImGuiIO* self, bool* value)
{
	ImGuiIO_Visitor::FieldSet__MouseDown(self, value);
}
extern "C" VFX_API float TitanImGui_ImGuiIO_Visitor_FieldGet__MouseWheel(ImGuiIO* self)
{
	return ImGuiIO_Visitor::FieldGet__MouseWheel(self);
}
extern "C" VFX_API void TitanImGui_ImGuiIO_Visitor_FieldSet__MouseWheel(ImGuiIO* self, float value)
{
	ImGuiIO_Visitor::FieldSet__MouseWheel(self, value);
}
extern "C" VFX_API float TitanImGui_ImGuiIO_Visitor_FieldGet__MouseWheelH(ImGuiIO* self)
{
	return ImGuiIO_Visitor::FieldGet__MouseWheelH(self);
}
extern "C" VFX_API void TitanImGui_ImGuiIO_Visitor_FieldSet__MouseWheelH(ImGuiIO* self, float value)
{
	ImGuiIO_Visitor::FieldSet__MouseWheelH(self, value);
}
extern "C" VFX_API ImGuiMouseSource TitanImGui_ImGuiIO_Visitor_FieldGet__MouseSource(ImGuiIO* self)
{
	return ImGuiIO_Visitor::FieldGet__MouseSource(self);
}
extern "C" VFX_API void TitanImGui_ImGuiIO_Visitor_FieldSet__MouseSource(ImGuiIO* self, ImGuiMouseSource value)
{
	ImGuiIO_Visitor::FieldSet__MouseSource(self, value);
}
extern "C" VFX_API unsigned int TitanImGui_ImGuiIO_Visitor_FieldGet__MouseHoveredViewport(ImGuiIO* self)
{
	return ImGuiIO_Visitor::FieldGet__MouseHoveredViewport(self);
}
extern "C" VFX_API void TitanImGui_ImGuiIO_Visitor_FieldSet__MouseHoveredViewport(ImGuiIO* self, unsigned int value)
{
	ImGuiIO_Visitor::FieldSet__MouseHoveredViewport(self, value);
}
extern "C" VFX_API char TitanImGui_ImGuiIO_Visitor_FieldGet__KeyCtrl(ImGuiIO* self)
{
	auto tmp_result = ImGuiIO_Visitor::FieldGet__KeyCtrl(self);
	return EngineNS::VReturnValueMarshal<bool,char>(tmp_result);
}
extern "C" VFX_API void TitanImGui_ImGuiIO_Visitor_FieldSet__KeyCtrl(ImGuiIO* self, bool value)
{
	ImGuiIO_Visitor::FieldSet__KeyCtrl(self, value);
}
extern "C" VFX_API char TitanImGui_ImGuiIO_Visitor_FieldGet__KeyShift(ImGuiIO* self)
{
	auto tmp_result = ImGuiIO_Visitor::FieldGet__KeyShift(self);
	return EngineNS::VReturnValueMarshal<bool,char>(tmp_result);
}
extern "C" VFX_API void TitanImGui_ImGuiIO_Visitor_FieldSet__KeyShift(ImGuiIO* self, bool value)
{
	ImGuiIO_Visitor::FieldSet__KeyShift(self, value);
}
extern "C" VFX_API char TitanImGui_ImGuiIO_Visitor_FieldGet__KeyAlt(ImGuiIO* self)
{
	auto tmp_result = ImGuiIO_Visitor::FieldGet__KeyAlt(self);
	return EngineNS::VReturnValueMarshal<bool,char>(tmp_result);
}
extern "C" VFX_API void TitanImGui_ImGuiIO_Visitor_FieldSet__KeyAlt(ImGuiIO* self, bool value)
{
	ImGuiIO_Visitor::FieldSet__KeyAlt(self, value);
}
extern "C" VFX_API char TitanImGui_ImGuiIO_Visitor_FieldGet__KeySuper(ImGuiIO* self)
{
	auto tmp_result = ImGuiIO_Visitor::FieldGet__KeySuper(self);
	return EngineNS::VReturnValueMarshal<bool,char>(tmp_result);
}
extern "C" VFX_API void TitanImGui_ImGuiIO_Visitor_FieldSet__KeySuper(ImGuiIO* self, bool value)
{
	ImGuiIO_Visitor::FieldSet__KeySuper(self, value);
}
extern "C" VFX_API int TitanImGui_ImGuiIO_Visitor_FieldGet__KeyMods(ImGuiIO* self)
{
	return ImGuiIO_Visitor::FieldGet__KeyMods(self);
}
extern "C" VFX_API void TitanImGui_ImGuiIO_Visitor_FieldSet__KeyMods(ImGuiIO* self, int value)
{
	ImGuiIO_Visitor::FieldSet__KeyMods(self, value);
}
extern "C" VFX_API char TitanImGui_ImGuiIO_Visitor_FieldGet__WantCaptureMouseUnlessPopupClose(ImGuiIO* self)
{
	auto tmp_result = ImGuiIO_Visitor::FieldGet__WantCaptureMouseUnlessPopupClose(self);
	return EngineNS::VReturnValueMarshal<bool,char>(tmp_result);
}
extern "C" VFX_API void TitanImGui_ImGuiIO_Visitor_FieldSet__WantCaptureMouseUnlessPopupClose(ImGuiIO* self, bool value)
{
	ImGuiIO_Visitor::FieldSet__WantCaptureMouseUnlessPopupClose(self, value);
}
extern "C" VFX_API v3dVector2_t TitanImGui_ImGuiIO_Visitor_FieldGet__MousePosPrev(ImGuiIO* self)
{
	auto tmp_result = ImGuiIO_Visitor::FieldGet__MousePosPrev(self);
	return EngineNS::VReturnValueMarshal<ImVec2,v3dVector2_t>(tmp_result);
}
extern "C" VFX_API void TitanImGui_ImGuiIO_Visitor_FieldSet__MousePosPrev(ImGuiIO* self, ImVec2 value)
{
	ImGuiIO_Visitor::FieldSet__MousePosPrev(self, value);
}
extern "C" VFX_API ImVec2* TitanImGui_ImGuiIO_Visitor_FieldGet__MouseClickedPos(ImGuiIO* self)
{
	return ImGuiIO_Visitor::FieldGet__MouseClickedPos(self);
}
extern "C" VFX_API void TitanImGui_ImGuiIO_Visitor_FieldSet__MouseClickedPos(ImGuiIO* self, ImVec2* value)
{
	ImGuiIO_Visitor::FieldSet__MouseClickedPos(self, value);
}
extern "C" VFX_API double* TitanImGui_ImGuiIO_Visitor_FieldGet__MouseClickedTime(ImGuiIO* self)
{
	return ImGuiIO_Visitor::FieldGet__MouseClickedTime(self);
}
extern "C" VFX_API void TitanImGui_ImGuiIO_Visitor_FieldSet__MouseClickedTime(ImGuiIO* self, double* value)
{
	ImGuiIO_Visitor::FieldSet__MouseClickedTime(self, value);
}
extern "C" VFX_API bool* TitanImGui_ImGuiIO_Visitor_FieldGet__MouseClicked(ImGuiIO* self)
{
	return ImGuiIO_Visitor::FieldGet__MouseClicked(self);
}
extern "C" VFX_API void TitanImGui_ImGuiIO_Visitor_FieldSet__MouseClicked(ImGuiIO* self, bool* value)
{
	ImGuiIO_Visitor::FieldSet__MouseClicked(self, value);
}
extern "C" VFX_API bool* TitanImGui_ImGuiIO_Visitor_FieldGet__MouseDoubleClicked(ImGuiIO* self)
{
	return ImGuiIO_Visitor::FieldGet__MouseDoubleClicked(self);
}
extern "C" VFX_API void TitanImGui_ImGuiIO_Visitor_FieldSet__MouseDoubleClicked(ImGuiIO* self, bool* value)
{
	ImGuiIO_Visitor::FieldSet__MouseDoubleClicked(self, value);
}
extern "C" VFX_API unsigned short* TitanImGui_ImGuiIO_Visitor_FieldGet__MouseClickedCount(ImGuiIO* self)
{
	return ImGuiIO_Visitor::FieldGet__MouseClickedCount(self);
}
extern "C" VFX_API void TitanImGui_ImGuiIO_Visitor_FieldSet__MouseClickedCount(ImGuiIO* self, unsigned short* value)
{
	ImGuiIO_Visitor::FieldSet__MouseClickedCount(self, value);
}
extern "C" VFX_API unsigned short* TitanImGui_ImGuiIO_Visitor_FieldGet__MouseClickedLastCount(ImGuiIO* self)
{
	return ImGuiIO_Visitor::FieldGet__MouseClickedLastCount(self);
}
extern "C" VFX_API void TitanImGui_ImGuiIO_Visitor_FieldSet__MouseClickedLastCount(ImGuiIO* self, unsigned short* value)
{
	ImGuiIO_Visitor::FieldSet__MouseClickedLastCount(self, value);
}
extern "C" VFX_API bool* TitanImGui_ImGuiIO_Visitor_FieldGet__MouseReleased(ImGuiIO* self)
{
	return ImGuiIO_Visitor::FieldGet__MouseReleased(self);
}
extern "C" VFX_API void TitanImGui_ImGuiIO_Visitor_FieldSet__MouseReleased(ImGuiIO* self, bool* value)
{
	ImGuiIO_Visitor::FieldSet__MouseReleased(self, value);
}
extern "C" VFX_API double* TitanImGui_ImGuiIO_Visitor_FieldGet__MouseReleasedTime(ImGuiIO* self)
{
	return ImGuiIO_Visitor::FieldGet__MouseReleasedTime(self);
}
extern "C" VFX_API void TitanImGui_ImGuiIO_Visitor_FieldSet__MouseReleasedTime(ImGuiIO* self, double* value)
{
	ImGuiIO_Visitor::FieldSet__MouseReleasedTime(self, value);
}
extern "C" VFX_API bool* TitanImGui_ImGuiIO_Visitor_FieldGet__MouseDownOwned(ImGuiIO* self)
{
	return ImGuiIO_Visitor::FieldGet__MouseDownOwned(self);
}
extern "C" VFX_API void TitanImGui_ImGuiIO_Visitor_FieldSet__MouseDownOwned(ImGuiIO* self, bool* value)
{
	ImGuiIO_Visitor::FieldSet__MouseDownOwned(self, value);
}
extern "C" VFX_API bool* TitanImGui_ImGuiIO_Visitor_FieldGet__MouseDownOwnedUnlessPopupClose(ImGuiIO* self)
{
	return ImGuiIO_Visitor::FieldGet__MouseDownOwnedUnlessPopupClose(self);
}
extern "C" VFX_API void TitanImGui_ImGuiIO_Visitor_FieldSet__MouseDownOwnedUnlessPopupClose(ImGuiIO* self, bool* value)
{
	ImGuiIO_Visitor::FieldSet__MouseDownOwnedUnlessPopupClose(self, value);
}
extern "C" VFX_API char TitanImGui_ImGuiIO_Visitor_FieldGet__MouseWheelRequestAxisSwap(ImGuiIO* self)
{
	auto tmp_result = ImGuiIO_Visitor::FieldGet__MouseWheelRequestAxisSwap(self);
	return EngineNS::VReturnValueMarshal<bool,char>(tmp_result);
}
extern "C" VFX_API void TitanImGui_ImGuiIO_Visitor_FieldSet__MouseWheelRequestAxisSwap(ImGuiIO* self, bool value)
{
	ImGuiIO_Visitor::FieldSet__MouseWheelRequestAxisSwap(self, value);
}
extern "C" VFX_API char TitanImGui_ImGuiIO_Visitor_FieldGet__MouseCtrlLeftAsRightClick(ImGuiIO* self)
{
	auto tmp_result = ImGuiIO_Visitor::FieldGet__MouseCtrlLeftAsRightClick(self);
	return EngineNS::VReturnValueMarshal<bool,char>(tmp_result);
}
extern "C" VFX_API void TitanImGui_ImGuiIO_Visitor_FieldSet__MouseCtrlLeftAsRightClick(ImGuiIO* self, bool value)
{
	ImGuiIO_Visitor::FieldSet__MouseCtrlLeftAsRightClick(self, value);
}
extern "C" VFX_API float* TitanImGui_ImGuiIO_Visitor_FieldGet__MouseDownDuration(ImGuiIO* self)
{
	return ImGuiIO_Visitor::FieldGet__MouseDownDuration(self);
}
extern "C" VFX_API void TitanImGui_ImGuiIO_Visitor_FieldSet__MouseDownDuration(ImGuiIO* self, float* value)
{
	ImGuiIO_Visitor::FieldSet__MouseDownDuration(self, value);
}
extern "C" VFX_API float* TitanImGui_ImGuiIO_Visitor_FieldGet__MouseDownDurationPrev(ImGuiIO* self)
{
	return ImGuiIO_Visitor::FieldGet__MouseDownDurationPrev(self);
}
extern "C" VFX_API void TitanImGui_ImGuiIO_Visitor_FieldSet__MouseDownDurationPrev(ImGuiIO* self, float* value)
{
	ImGuiIO_Visitor::FieldSet__MouseDownDurationPrev(self, value);
}
extern "C" VFX_API ImVec2* TitanImGui_ImGuiIO_Visitor_FieldGet__MouseDragMaxDistanceAbs(ImGuiIO* self)
{
	return ImGuiIO_Visitor::FieldGet__MouseDragMaxDistanceAbs(self);
}
extern "C" VFX_API void TitanImGui_ImGuiIO_Visitor_FieldSet__MouseDragMaxDistanceAbs(ImGuiIO* self, ImVec2* value)
{
	ImGuiIO_Visitor::FieldSet__MouseDragMaxDistanceAbs(self, value);
}
extern "C" VFX_API float* TitanImGui_ImGuiIO_Visitor_FieldGet__MouseDragMaxDistanceSqr(ImGuiIO* self)
{
	return ImGuiIO_Visitor::FieldGet__MouseDragMaxDistanceSqr(self);
}
extern "C" VFX_API void TitanImGui_ImGuiIO_Visitor_FieldSet__MouseDragMaxDistanceSqr(ImGuiIO* self, float* value)
{
	ImGuiIO_Visitor::FieldSet__MouseDragMaxDistanceSqr(self, value);
}
extern "C" VFX_API float TitanImGui_ImGuiIO_Visitor_FieldGet__PenPressure(ImGuiIO* self)
{
	return ImGuiIO_Visitor::FieldGet__PenPressure(self);
}
extern "C" VFX_API void TitanImGui_ImGuiIO_Visitor_FieldSet__PenPressure(ImGuiIO* self, float value)
{
	ImGuiIO_Visitor::FieldSet__PenPressure(self, value);
}
extern "C" VFX_API char TitanImGui_ImGuiIO_Visitor_FieldGet__AppFocusLost(ImGuiIO* self)
{
	auto tmp_result = ImGuiIO_Visitor::FieldGet__AppFocusLost(self);
	return EngineNS::VReturnValueMarshal<bool,char>(tmp_result);
}
extern "C" VFX_API void TitanImGui_ImGuiIO_Visitor_FieldSet__AppFocusLost(ImGuiIO* self, bool value)
{
	ImGuiIO_Visitor::FieldSet__AppFocusLost(self, value);
}
extern "C" VFX_API char TitanImGui_ImGuiIO_Visitor_FieldGet__AppAcceptingEvents(ImGuiIO* self)
{
	auto tmp_result = ImGuiIO_Visitor::FieldGet__AppAcceptingEvents(self);
	return EngineNS::VReturnValueMarshal<bool,char>(tmp_result);
}
extern "C" VFX_API void TitanImGui_ImGuiIO_Visitor_FieldSet__AppAcceptingEvents(ImGuiIO* self, bool value)
{
	ImGuiIO_Visitor::FieldSet__AppAcceptingEvents(self, value);
}
extern "C" VFX_API ImWchar16 TitanImGui_ImGuiIO_Visitor_FieldGet__InputQueueSurrogate(ImGuiIO* self)
{
	auto tmp_result = ImGuiIO_Visitor::FieldGet__InputQueueSurrogate(self);
	return EngineNS::VReturnValueMarshal<ImWchar,ImWchar16>(tmp_result);
}
extern "C" VFX_API void TitanImGui_ImGuiIO_Visitor_FieldSet__InputQueueSurrogate(ImGuiIO* self, ImWchar value)
{
	ImGuiIO_Visitor::FieldSet__InputQueueSurrogate(self, value);
}
extern "C" VFX_API float TitanImGui_ImGuiIO_Visitor_FieldGet__FontGlobalScale(ImGuiIO* self)
{
	return ImGuiIO_Visitor::FieldGet__FontGlobalScale(self);
}
extern "C" VFX_API void TitanImGui_ImGuiIO_Visitor_FieldSet__FontGlobalScale(ImGuiIO* self, float value)
{
	ImGuiIO_Visitor::FieldSet__FontGlobalScale(self, value);
}
extern "C" VFX_API void* TitanImGui_ImGuiIO_Visitor_FieldGet__GetClipboardTextFn(ImGuiIO* self)
{
	return ImGuiIO_Visitor::FieldGet__GetClipboardTextFn(self);
}
extern "C" VFX_API void TitanImGui_ImGuiIO_Visitor_FieldSet__GetClipboardTextFn(ImGuiIO* self, char* (*GetClipboardTextFn)(void* arg0))
{
	ImGuiIO_Visitor::FieldSet__GetClipboardTextFn(self, GetClipboardTextFn);
}
extern "C" VFX_API void* TitanImGui_ImGuiIO_Visitor_FieldGet__SetClipboardTextFn(ImGuiIO* self)
{
	return ImGuiIO_Visitor::FieldGet__SetClipboardTextFn(self);
}
extern "C" VFX_API void TitanImGui_ImGuiIO_Visitor_FieldSet__SetClipboardTextFn(ImGuiIO* self, void (*SetClipboardTextFn)(void* arg0,char* arg1))
{
	ImGuiIO_Visitor::FieldSet__SetClipboardTextFn(self, SetClipboardTextFn);
}
extern "C" VFX_API void* TitanImGui_ImGuiIO_Visitor_FieldGet__ClipboardUserData(ImGuiIO* self)
{
	return ImGuiIO_Visitor::FieldGet__ClipboardUserData(self);
}
extern "C" VFX_API void TitanImGui_ImGuiIO_Visitor_FieldSet__ClipboardUserData(ImGuiIO* self, void* value)
{
	ImGuiIO_Visitor::FieldSet__ClipboardUserData(self, value);
}


extern "C" VFX_API void TitanImGui_ImGuiIO_Visitor_AddKeyEvent_2200478491(ImGuiIO* self, ImGuiKey key,bool down)
{
	return ImGuiIO_Visitor::AddKeyEvent(self, key, down);
}
extern "C" VFX_API void TitanImGui_ImGuiIO_Visitor_AddKeyAnalogEvent_2648809663(ImGuiIO* self, ImGuiKey key,bool down,float v)
{
	return ImGuiIO_Visitor::AddKeyAnalogEvent(self, key, down, v);
}
extern "C" VFX_API void TitanImGui_ImGuiIO_Visitor_AddMousePosEvent_996365349(ImGuiIO* self, float x,float y)
{
	return ImGuiIO_Visitor::AddMousePosEvent(self, x, y);
}
extern "C" VFX_API void TitanImGui_ImGuiIO_Visitor_AddMouseButtonEvent_2814434660(ImGuiIO* self, int button,bool down)
{
	return ImGuiIO_Visitor::AddMouseButtonEvent(self, button, down);
}
extern "C" VFX_API void TitanImGui_ImGuiIO_Visitor_AddMouseWheelEvent_996365349(ImGuiIO* self, float wheel_x,float wheel_y)
{
	return ImGuiIO_Visitor::AddMouseWheelEvent(self, wheel_x, wheel_y);
}
extern "C" VFX_API void TitanImGui_ImGuiIO_Visitor_AddMouseSourceEvent_4119589844(ImGuiIO* self, ImGuiMouseSource source)
{
	return ImGuiIO_Visitor::AddMouseSourceEvent(self, source);
}
extern "C" VFX_API void TitanImGui_ImGuiIO_Visitor_AddMouseViewportEvent_2252480719(ImGuiIO* self, unsigned int id)
{
	return ImGuiIO_Visitor::AddMouseViewportEvent(self, id);
}
extern "C" VFX_API void TitanImGui_ImGuiIO_Visitor_AddFocusEvent_2077628183(ImGuiIO* self, bool focused)
{
	return ImGuiIO_Visitor::AddFocusEvent(self, focused);
}
extern "C" VFX_API void TitanImGui_ImGuiIO_Visitor_AddInputCharacter_1961468199(ImGuiIO* self, unsigned int c)
{
	return ImGuiIO_Visitor::AddInputCharacter(self, c);
}
extern "C" VFX_API void TitanImGui_ImGuiIO_Visitor_AddInputCharacterUTF16_1265636733(ImGuiIO* self, ImWchar c)
{
	return ImGuiIO_Visitor::AddInputCharacterUTF16(self, c);
}
extern "C" VFX_API void TitanImGui_ImGuiIO_Visitor_AddInputCharactersUTF8_2602414842(ImGuiIO* self, const char* str)
{
	return ImGuiIO_Visitor::AddInputCharactersUTF8(self, str);
}
extern "C" VFX_API void TitanImGui_ImGuiIO_Visitor_SetKeyEventNativeData_1655477210(ImGuiIO* self, ImGuiKey key,int native_keycode,int native_scancode,int native_legacy_index)
{
	return ImGuiIO_Visitor::SetKeyEventNativeData(self, key, native_keycode, native_scancode, native_legacy_index);
}
extern "C" VFX_API void TitanImGui_ImGuiIO_Visitor_SetAppAcceptingEvents_2077628183(ImGuiIO* self, bool accepting_events)
{
	return ImGuiIO_Visitor::SetAppAcceptingEvents(self, accepting_events);
}
extern "C" VFX_API void TitanImGui_ImGuiIO_Visitor_ClearEventsQueue_2960189489(ImGuiIO* self)
{
	return ImGuiIO_Visitor::ClearEventsQueue(self);
}
extern "C" VFX_API void TitanImGui_ImGuiIO_Visitor_ClearInputKeys_2960189489(ImGuiIO* self)
{
	return ImGuiIO_Visitor::ClearInputKeys(self);
}
extern "C" VFX_API void TitanImGui_ImGuiIO_Visitor_ClearInputMouse_2960189489(ImGuiIO* self)
{
	return ImGuiIO_Visitor::ClearInputMouse(self);
}
#endif//HasModule_ImGui
