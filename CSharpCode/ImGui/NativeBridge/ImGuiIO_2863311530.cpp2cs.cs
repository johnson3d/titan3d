//generated from legacy CppWeaving ImGui surface; native entrypoints routed through TitanImGuiBridge
using System;
using System.Runtime.InteropServices;
#if DEBUG
	using SuppressGC = EngineNS.Rtti.TtDummyAttribute;
#else
	using SuppressGC = System.Runtime.InteropServices.SuppressGCTransitionAttribute;
#endif


public unsafe partial struct ImGuiIO : EngineNS.IPtrType
{
	[System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Explicit, Size = 3096, Pack = 8)]
	public struct CppStructLayout
	{
		[System.Runtime.InteropServices.FieldOffset(0)]
		public ImGuiConfigFlags_ ConfigFlags;
		[System.Runtime.InteropServices.FieldOffset(4)]
		public ImGuiBackendFlags_ BackendFlags;
		[System.Runtime.InteropServices.FieldOffset(8)]
		public EngineNS.Vector2 DisplaySize;
		[System.Runtime.InteropServices.FieldOffset(16)]
		public EngineNS.Vector2 DisplayFramebufferScale;
		[System.Runtime.InteropServices.FieldOffset(24)]
		public float DeltaTime;
		[System.Runtime.InteropServices.FieldOffset(28)]
		public float IniSavingRate;
		[System.Runtime.InteropServices.FieldOffset(32)]
		public sbyte* IniFilename;
		[System.Runtime.InteropServices.FieldOffset(40)]
		public sbyte* LogFilename;
		[System.Runtime.InteropServices.FieldOffset(48)]
		public void* UserData;
		[System.Runtime.InteropServices.FieldOffset(56)]
		public ImFontAtlas* Fonts;
		[System.Runtime.InteropServices.FieldOffset(64)]
		public ImFont* FontDefault;
		[System.Runtime.InteropServices.FieldOffset(72)]
		public bool FontAllowUserScaling;
		[System.Runtime.InteropServices.FieldOffset(73)]
		public bool ConfigNavSwapGamepadButtons;
		[System.Runtime.InteropServices.FieldOffset(74)]
		public bool ConfigNavMoveSetMousePos;
		[System.Runtime.InteropServices.FieldOffset(75)]
		public bool ConfigNavCaptureKeyboard;
		[System.Runtime.InteropServices.FieldOffset(76)]
		public bool ConfigNavEscapeClearFocusItem;
		[System.Runtime.InteropServices.FieldOffset(77)]
		public bool ConfigNavEscapeClearFocusWindow;
		[System.Runtime.InteropServices.FieldOffset(78)]
		public bool ConfigNavCursorVisibleAuto;
		[System.Runtime.InteropServices.FieldOffset(79)]
		public bool ConfigNavCursorVisibleAlways;
		[System.Runtime.InteropServices.FieldOffset(80)]
		public bool ConfigDockingNoSplit;
		[System.Runtime.InteropServices.FieldOffset(81)]
		public bool ConfigDockingNoDockingOver;
		[System.Runtime.InteropServices.FieldOffset(82)]
		public bool ConfigDockingWithShift;
		[System.Runtime.InteropServices.FieldOffset(83)]
		public bool ConfigDockingAlwaysTabBar;
		[System.Runtime.InteropServices.FieldOffset(84)]
		public bool ConfigDockingTransparentPayload;
		[System.Runtime.InteropServices.FieldOffset(85)]
		public bool ConfigViewportsNoAutoMerge;
		[System.Runtime.InteropServices.FieldOffset(86)]
		public bool ConfigViewportsNoTaskBarIcon;
		[System.Runtime.InteropServices.FieldOffset(87)]
		public bool ConfigViewportsNoDecoration;
		[System.Runtime.InteropServices.FieldOffset(88)]
		public bool ConfigViewportsNoDefaultParent;
		[System.Runtime.InteropServices.FieldOffset(89)]
		public bool ConfigViewportsPlatformFocusSetsImGuiFocus;
		[System.Runtime.InteropServices.FieldOffset(90)]
		public bool ConfigDpiScaleFonts;
		[System.Runtime.InteropServices.FieldOffset(91)]
		public bool ConfigDpiScaleViewports;
		[System.Runtime.InteropServices.FieldOffset(92)]
		public bool MouseDrawCursor;
		[System.Runtime.InteropServices.FieldOffset(93)]
		public bool ConfigMacOSXBehaviors;
		[System.Runtime.InteropServices.FieldOffset(94)]
		public bool ConfigInputTrickleEventQueue;
		[System.Runtime.InteropServices.FieldOffset(95)]
		public bool ConfigInputTextCursorBlink;
		[System.Runtime.InteropServices.FieldOffset(96)]
		public bool ConfigInputTextEnterKeepActive;
		[System.Runtime.InteropServices.FieldOffset(97)]
		public bool ConfigDragClickToInputText;
		[System.Runtime.InteropServices.FieldOffset(98)]
		public bool ConfigWindowsResizeFromEdges;
		[System.Runtime.InteropServices.FieldOffset(99)]
		public bool ConfigWindowsMoveFromTitleBarOnly;
		[System.Runtime.InteropServices.FieldOffset(100)]
		public bool ConfigWindowsCopyContentsWithCtrlC;
		[System.Runtime.InteropServices.FieldOffset(101)]
		public bool ConfigScrollbarScrollByPage;
		[System.Runtime.InteropServices.FieldOffset(104)]
		public float ConfigMemoryCompactTimer;
		[System.Runtime.InteropServices.FieldOffset(108)]
		public float MouseDoubleClickTime;
		[System.Runtime.InteropServices.FieldOffset(112)]
		public float MouseDoubleClickMaxDist;
		[System.Runtime.InteropServices.FieldOffset(116)]
		public float MouseDragThreshold;
		[System.Runtime.InteropServices.FieldOffset(120)]
		public float KeyRepeatDelay;
		[System.Runtime.InteropServices.FieldOffset(124)]
		public float KeyRepeatRate;
		[System.Runtime.InteropServices.FieldOffset(128)]
		public bool ConfigErrorRecovery;
		[System.Runtime.InteropServices.FieldOffset(129)]
		public bool ConfigErrorRecoveryEnableAssert;
		[System.Runtime.InteropServices.FieldOffset(130)]
		public bool ConfigErrorRecoveryEnableDebugLog;
		[System.Runtime.InteropServices.FieldOffset(131)]
		public bool ConfigErrorRecoveryEnableTooltip;
		[System.Runtime.InteropServices.FieldOffset(132)]
		public bool ConfigDebugIsDebuggerPresent;
		[System.Runtime.InteropServices.FieldOffset(133)]
		public bool ConfigDebugHighlightIdConflicts;
		[System.Runtime.InteropServices.FieldOffset(134)]
		public bool ConfigDebugHighlightIdConflictsShowItemPicker;
		[System.Runtime.InteropServices.FieldOffset(135)]
		public bool ConfigDebugBeginReturnValueOnce;
		[System.Runtime.InteropServices.FieldOffset(136)]
		public bool ConfigDebugBeginReturnValueLoop;
		[System.Runtime.InteropServices.FieldOffset(137)]
		public bool ConfigDebugIgnoreFocusLoss;
		[System.Runtime.InteropServices.FieldOffset(138)]
		public bool ConfigDebugIniSettings;
		[System.Runtime.InteropServices.FieldOffset(144)]
		public sbyte* BackendPlatformName;
		[System.Runtime.InteropServices.FieldOffset(152)]
		public sbyte* BackendRendererName;
		[System.Runtime.InteropServices.FieldOffset(160)]
		public void* BackendPlatformUserData;
		[System.Runtime.InteropServices.FieldOffset(168)]
		public void* BackendRendererUserData;
		[System.Runtime.InteropServices.FieldOffset(176)]
		public void* BackendLanguageUserData;
		[System.Runtime.InteropServices.FieldOffset(184)]
		public bool WantCaptureMouse;
		[System.Runtime.InteropServices.FieldOffset(185)]
		public bool WantCaptureKeyboard;
		[System.Runtime.InteropServices.FieldOffset(186)]
		public bool WantTextInput;
		[System.Runtime.InteropServices.FieldOffset(187)]
		public bool WantSetMousePos;
		[System.Runtime.InteropServices.FieldOffset(188)]
		public bool WantSaveIniSettings;
		[System.Runtime.InteropServices.FieldOffset(189)]
		public bool NavActive;
		[System.Runtime.InteropServices.FieldOffset(190)]
		public bool NavVisible;
		[System.Runtime.InteropServices.FieldOffset(192)]
		public float Framerate;
		[System.Runtime.InteropServices.FieldOffset(196)]
		public int MetricsRenderVertices;
		[System.Runtime.InteropServices.FieldOffset(200)]
		public int MetricsRenderIndices;
		[System.Runtime.InteropServices.FieldOffset(204)]
		public int MetricsRenderWindows;
		[System.Runtime.InteropServices.FieldOffset(208)]
		public int MetricsActiveWindows;
		[System.Runtime.InteropServices.FieldOffset(212)]
		public EngineNS.Vector2 MouseDelta;
		[System.Runtime.InteropServices.FieldOffset(232)]
		public EngineNS.Vector2 MousePos;
		[System.Runtime.InteropServices.FieldOffset(240)]
		public bool* MouseDown;
		[System.Runtime.InteropServices.FieldOffset(248)]
		public float MouseWheel;
		[System.Runtime.InteropServices.FieldOffset(252)]
		public float MouseWheelH;
		[System.Runtime.InteropServices.FieldOffset(256)]
		public ImGuiMouseSource MouseSource;
		[System.Runtime.InteropServices.FieldOffset(260)]
		public uint MouseHoveredViewport;
		[System.Runtime.InteropServices.FieldOffset(264)]
		public bool KeyCtrl;
		[System.Runtime.InteropServices.FieldOffset(265)]
		public bool KeyShift;
		[System.Runtime.InteropServices.FieldOffset(266)]
		public bool KeyAlt;
		[System.Runtime.InteropServices.FieldOffset(267)]
		public bool KeySuper;
		[System.Runtime.InteropServices.FieldOffset(268)]
		public int KeyMods;
		[System.Runtime.InteropServices.FieldOffset(2752)]
		public bool WantCaptureMouseUnlessPopupClose;
		[System.Runtime.InteropServices.FieldOffset(2756)]
		public EngineNS.Vector2 MousePosPrev;
		[System.Runtime.InteropServices.FieldOffset(2764)]
		public EngineNS.Vector2* MouseClickedPos;
		[System.Runtime.InteropServices.FieldOffset(2808)]
		public double* MouseClickedTime;
		[System.Runtime.InteropServices.FieldOffset(2848)]
		public bool* MouseClicked;
		[System.Runtime.InteropServices.FieldOffset(2853)]
		public bool* MouseDoubleClicked;
		[System.Runtime.InteropServices.FieldOffset(2858)]
		public ushort* MouseClickedCount;
		[System.Runtime.InteropServices.FieldOffset(2868)]
		public ushort* MouseClickedLastCount;
		[System.Runtime.InteropServices.FieldOffset(2878)]
		public bool* MouseReleased;
		[System.Runtime.InteropServices.FieldOffset(2888)]
		public double* MouseReleasedTime;
		[System.Runtime.InteropServices.FieldOffset(2928)]
		public bool* MouseDownOwned;
		[System.Runtime.InteropServices.FieldOffset(2933)]
		public bool* MouseDownOwnedUnlessPopupClose;
		[System.Runtime.InteropServices.FieldOffset(2938)]
		public bool MouseWheelRequestAxisSwap;
		[System.Runtime.InteropServices.FieldOffset(2939)]
		public bool MouseCtrlLeftAsRightClick;
		[System.Runtime.InteropServices.FieldOffset(2940)]
		public float* MouseDownDuration;
		[System.Runtime.InteropServices.FieldOffset(2960)]
		public float* MouseDownDurationPrev;
		[System.Runtime.InteropServices.FieldOffset(2980)]
		public EngineNS.Vector2* MouseDragMaxDistanceAbs;
		[System.Runtime.InteropServices.FieldOffset(3020)]
		public float* MouseDragMaxDistanceSqr;
		[System.Runtime.InteropServices.FieldOffset(3040)]
		public float PenPressure;
		[System.Runtime.InteropServices.FieldOffset(3044)]
		public bool AppFocusLost;
		[System.Runtime.InteropServices.FieldOffset(3045)]
		public bool AppAcceptingEvents;
		[System.Runtime.InteropServices.FieldOffset(3046)]
		public Wchar16 InputQueueSurrogate;
		[System.Runtime.InteropServices.FieldOffset(3064)]
		public float FontGlobalScale;
		[System.Runtime.InteropServices.FieldOffset(3072)]
		public IntPtr GetClipboardTextFn;
		[System.Runtime.InteropServices.FieldOffset(3080)]
		public IntPtr SetClipboardTextFn;
		[System.Runtime.InteropServices.FieldOffset(3088)]
		public void* ClipboardUserData;
	}
	private void* mPointer;
	public CppStructLayout* UnsafeAsLayout { get => (CppStructLayout*)mPointer; }
	public ImGuiIO(void* p) { mPointer = p; }
	public void UnsafeSetPointer(void* p) { mPointer = p; }
	public IntPtr NativePointer { get => (IntPtr)mPointer; set => mPointer = value.ToPointer(); }
	public ImGuiIO* CppPointer { get => (ImGuiIO*)mPointer; }
	public bool IsValidPointer { get => mPointer != (void*)0; }
	public static implicit operator ImGuiIO* (ImGuiIO v)
	{
		return (ImGuiIO*)v.mPointer;
	}
	#region Constructor&Cast
	public static EngineNS.FRttiStruct GetTypeRtti()
	{
		return new EngineNS.FRttiStruct(TitanImGui_ImGuiIO_Visitor_GetTypeRtti());
	}
	public static ImGuiIO CreateInstance()
	{
		return new ImGuiIO(TitanImGui_ImGuiIO_Visitor_CreateInstance_2960189489());
	}
	#endregion
	#region Fields
	public ImGuiConfigFlags_ ConfigFlags
	{
		get
		{
			return TitanImGui_ImGuiIO_Visitor_FieldGet__ConfigFlags(mPointer);
		}
		set
		{
			TitanImGui_ImGuiIO_Visitor_FieldSet__ConfigFlags(mPointer, value);
		}
	}
	public ImGuiBackendFlags_ BackendFlags
	{
		get
		{
			return TitanImGui_ImGuiIO_Visitor_FieldGet__BackendFlags(mPointer);
		}
		set
		{
			TitanImGui_ImGuiIO_Visitor_FieldSet__BackendFlags(mPointer, value);
		}
	}
	public EngineNS.Vector2 DisplaySize
	{
		get
		{
			return TitanImGui_ImGuiIO_Visitor_FieldGet__DisplaySize(mPointer);
		}
		set
		{
			TitanImGui_ImGuiIO_Visitor_FieldSet__DisplaySize(mPointer, value);
		}
	}
	public EngineNS.Vector2 DisplayFramebufferScale
	{
		get
		{
			return TitanImGui_ImGuiIO_Visitor_FieldGet__DisplayFramebufferScale(mPointer);
		}
		set
		{
			TitanImGui_ImGuiIO_Visitor_FieldSet__DisplayFramebufferScale(mPointer, value);
		}
	}
	public float DeltaTime
	{
		get
		{
			return TitanImGui_ImGuiIO_Visitor_FieldGet__DeltaTime(mPointer);
		}
		set
		{
			TitanImGui_ImGuiIO_Visitor_FieldSet__DeltaTime(mPointer, value);
		}
	}
	public float IniSavingRate
	{
		get
		{
			return TitanImGui_ImGuiIO_Visitor_FieldGet__IniSavingRate(mPointer);
		}
		set
		{
			TitanImGui_ImGuiIO_Visitor_FieldSet__IniSavingRate(mPointer, value);
		}
	}
	public string IniFilename
	{
		get
		{
			return EngineNS.Rtti.TtNativeCoreProvider.MarshalPtrUtf8((IntPtr)TitanImGui_ImGuiIO_Visitor_FieldGet__IniFilename(mPointer));
		}
		set
		{
			TitanImGui_ImGuiIO_Visitor_FieldSet__IniFilename(mPointer, value);
		}
	}
	public string LogFilename
	{
		get
		{
			return EngineNS.Rtti.TtNativeCoreProvider.MarshalPtrUtf8((IntPtr)TitanImGui_ImGuiIO_Visitor_FieldGet__LogFilename(mPointer));
		}
		set
		{
			TitanImGui_ImGuiIO_Visitor_FieldSet__LogFilename(mPointer, value);
		}
	}
	public void* UserData
	{
		get
		{
			return TitanImGui_ImGuiIO_Visitor_FieldGet__UserData(mPointer);
		}
		set
		{
			TitanImGui_ImGuiIO_Visitor_FieldSet__UserData(mPointer, value);
		}
	}
	public ImFontAtlas Fonts
	{
		get
		{
			return new ImFontAtlas(TitanImGui_ImGuiIO_Visitor_FieldGet__Fonts(mPointer));
		}
		set
		{
			TitanImGui_ImGuiIO_Visitor_FieldSet__Fonts(mPointer, value);
		}
	}
	public ImFont FontDefault
	{
		get
		{
			return new ImFont(TitanImGui_ImGuiIO_Visitor_FieldGet__FontDefault(mPointer));
		}
		set
		{
			TitanImGui_ImGuiIO_Visitor_FieldSet__FontDefault(mPointer, value);
		}
	}
	public bool FontAllowUserScaling
	{
		get
		{
			return TitanImGui_ImGuiIO_Visitor_FieldGet__FontAllowUserScaling(mPointer) == 0 ? false : true;
		}
		set
		{
			TitanImGui_ImGuiIO_Visitor_FieldSet__FontAllowUserScaling(mPointer, value);
		}
	}
	public bool ConfigNavSwapGamepadButtons
	{
		get
		{
			return TitanImGui_ImGuiIO_Visitor_FieldGet__ConfigNavSwapGamepadButtons(mPointer) == 0 ? false : true;
		}
		set
		{
			TitanImGui_ImGuiIO_Visitor_FieldSet__ConfigNavSwapGamepadButtons(mPointer, value);
		}
	}
	public bool ConfigNavMoveSetMousePos
	{
		get
		{
			return TitanImGui_ImGuiIO_Visitor_FieldGet__ConfigNavMoveSetMousePos(mPointer) == 0 ? false : true;
		}
		set
		{
			TitanImGui_ImGuiIO_Visitor_FieldSet__ConfigNavMoveSetMousePos(mPointer, value);
		}
	}
	public bool ConfigNavCaptureKeyboard
	{
		get
		{
			return TitanImGui_ImGuiIO_Visitor_FieldGet__ConfigNavCaptureKeyboard(mPointer) == 0 ? false : true;
		}
		set
		{
			TitanImGui_ImGuiIO_Visitor_FieldSet__ConfigNavCaptureKeyboard(mPointer, value);
		}
	}
	public bool ConfigNavEscapeClearFocusItem
	{
		get
		{
			return TitanImGui_ImGuiIO_Visitor_FieldGet__ConfigNavEscapeClearFocusItem(mPointer) == 0 ? false : true;
		}
		set
		{
			TitanImGui_ImGuiIO_Visitor_FieldSet__ConfigNavEscapeClearFocusItem(mPointer, value);
		}
	}
	public bool ConfigNavEscapeClearFocusWindow
	{
		get
		{
			return TitanImGui_ImGuiIO_Visitor_FieldGet__ConfigNavEscapeClearFocusWindow(mPointer) == 0 ? false : true;
		}
		set
		{
			TitanImGui_ImGuiIO_Visitor_FieldSet__ConfigNavEscapeClearFocusWindow(mPointer, value);
		}
	}
	public bool ConfigNavCursorVisibleAuto
	{
		get
		{
			return TitanImGui_ImGuiIO_Visitor_FieldGet__ConfigNavCursorVisibleAuto(mPointer) == 0 ? false : true;
		}
		set
		{
			TitanImGui_ImGuiIO_Visitor_FieldSet__ConfigNavCursorVisibleAuto(mPointer, value);
		}
	}
	public bool ConfigNavCursorVisibleAlways
	{
		get
		{
			return TitanImGui_ImGuiIO_Visitor_FieldGet__ConfigNavCursorVisibleAlways(mPointer) == 0 ? false : true;
		}
		set
		{
			TitanImGui_ImGuiIO_Visitor_FieldSet__ConfigNavCursorVisibleAlways(mPointer, value);
		}
	}
	public bool ConfigDockingNoSplit
	{
		get
		{
			return TitanImGui_ImGuiIO_Visitor_FieldGet__ConfigDockingNoSplit(mPointer) == 0 ? false : true;
		}
		set
		{
			TitanImGui_ImGuiIO_Visitor_FieldSet__ConfigDockingNoSplit(mPointer, value);
		}
	}
	public bool ConfigDockingNoDockingOver
	{
		get
		{
			return TitanImGui_ImGuiIO_Visitor_FieldGet__ConfigDockingNoDockingOver(mPointer) == 0 ? false : true;
		}
		set
		{
			TitanImGui_ImGuiIO_Visitor_FieldSet__ConfigDockingNoDockingOver(mPointer, value);
		}
	}
	public bool ConfigDockingWithShift
	{
		get
		{
			return TitanImGui_ImGuiIO_Visitor_FieldGet__ConfigDockingWithShift(mPointer) == 0 ? false : true;
		}
		set
		{
			TitanImGui_ImGuiIO_Visitor_FieldSet__ConfigDockingWithShift(mPointer, value);
		}
	}
	public bool ConfigDockingAlwaysTabBar
	{
		get
		{
			return TitanImGui_ImGuiIO_Visitor_FieldGet__ConfigDockingAlwaysTabBar(mPointer) == 0 ? false : true;
		}
		set
		{
			TitanImGui_ImGuiIO_Visitor_FieldSet__ConfigDockingAlwaysTabBar(mPointer, value);
		}
	}
	public bool ConfigDockingTransparentPayload
	{
		get
		{
			return TitanImGui_ImGuiIO_Visitor_FieldGet__ConfigDockingTransparentPayload(mPointer) == 0 ? false : true;
		}
		set
		{
			TitanImGui_ImGuiIO_Visitor_FieldSet__ConfigDockingTransparentPayload(mPointer, value);
		}
	}
	public bool ConfigViewportsNoAutoMerge
	{
		get
		{
			return TitanImGui_ImGuiIO_Visitor_FieldGet__ConfigViewportsNoAutoMerge(mPointer) == 0 ? false : true;
		}
		set
		{
			TitanImGui_ImGuiIO_Visitor_FieldSet__ConfigViewportsNoAutoMerge(mPointer, value);
		}
	}
	public bool ConfigViewportsNoTaskBarIcon
	{
		get
		{
			return TitanImGui_ImGuiIO_Visitor_FieldGet__ConfigViewportsNoTaskBarIcon(mPointer) == 0 ? false : true;
		}
		set
		{
			TitanImGui_ImGuiIO_Visitor_FieldSet__ConfigViewportsNoTaskBarIcon(mPointer, value);
		}
	}
	public bool ConfigViewportsNoDecoration
	{
		get
		{
			return TitanImGui_ImGuiIO_Visitor_FieldGet__ConfigViewportsNoDecoration(mPointer) == 0 ? false : true;
		}
		set
		{
			TitanImGui_ImGuiIO_Visitor_FieldSet__ConfigViewportsNoDecoration(mPointer, value);
		}
	}
	public bool ConfigViewportsNoDefaultParent
	{
		get
		{
			return TitanImGui_ImGuiIO_Visitor_FieldGet__ConfigViewportsNoDefaultParent(mPointer) == 0 ? false : true;
		}
		set
		{
			TitanImGui_ImGuiIO_Visitor_FieldSet__ConfigViewportsNoDefaultParent(mPointer, value);
		}
	}
	public bool ConfigViewportsPlatformFocusSetsImGuiFocus
	{
		get
		{
			return TitanImGui_ImGuiIO_Visitor_FieldGet__ConfigViewportsPlatformFocusSetsImGuiFocus(mPointer) == 0 ? false : true;
		}
		set
		{
			TitanImGui_ImGuiIO_Visitor_FieldSet__ConfigViewportsPlatformFocusSetsImGuiFocus(mPointer, value);
		}
	}
	public bool ConfigDpiScaleFonts
	{
		get
		{
			return TitanImGui_ImGuiIO_Visitor_FieldGet__ConfigDpiScaleFonts(mPointer) == 0 ? false : true;
		}
		set
		{
			TitanImGui_ImGuiIO_Visitor_FieldSet__ConfigDpiScaleFonts(mPointer, value);
		}
	}
	public bool ConfigDpiScaleViewports
	{
		get
		{
			return TitanImGui_ImGuiIO_Visitor_FieldGet__ConfigDpiScaleViewports(mPointer) == 0 ? false : true;
		}
		set
		{
			TitanImGui_ImGuiIO_Visitor_FieldSet__ConfigDpiScaleViewports(mPointer, value);
		}
	}
	public bool MouseDrawCursor
	{
		get
		{
			return TitanImGui_ImGuiIO_Visitor_FieldGet__MouseDrawCursor(mPointer) == 0 ? false : true;
		}
		set
		{
			TitanImGui_ImGuiIO_Visitor_FieldSet__MouseDrawCursor(mPointer, value);
		}
	}
	public bool ConfigMacOSXBehaviors
	{
		get
		{
			return TitanImGui_ImGuiIO_Visitor_FieldGet__ConfigMacOSXBehaviors(mPointer) == 0 ? false : true;
		}
		set
		{
			TitanImGui_ImGuiIO_Visitor_FieldSet__ConfigMacOSXBehaviors(mPointer, value);
		}
	}
	public bool ConfigInputTrickleEventQueue
	{
		get
		{
			return TitanImGui_ImGuiIO_Visitor_FieldGet__ConfigInputTrickleEventQueue(mPointer) == 0 ? false : true;
		}
		set
		{
			TitanImGui_ImGuiIO_Visitor_FieldSet__ConfigInputTrickleEventQueue(mPointer, value);
		}
	}
	public bool ConfigInputTextCursorBlink
	{
		get
		{
			return TitanImGui_ImGuiIO_Visitor_FieldGet__ConfigInputTextCursorBlink(mPointer) == 0 ? false : true;
		}
		set
		{
			TitanImGui_ImGuiIO_Visitor_FieldSet__ConfigInputTextCursorBlink(mPointer, value);
		}
	}
	public bool ConfigInputTextEnterKeepActive
	{
		get
		{
			return TitanImGui_ImGuiIO_Visitor_FieldGet__ConfigInputTextEnterKeepActive(mPointer) == 0 ? false : true;
		}
		set
		{
			TitanImGui_ImGuiIO_Visitor_FieldSet__ConfigInputTextEnterKeepActive(mPointer, value);
		}
	}
	public bool ConfigDragClickToInputText
	{
		get
		{
			return TitanImGui_ImGuiIO_Visitor_FieldGet__ConfigDragClickToInputText(mPointer) == 0 ? false : true;
		}
		set
		{
			TitanImGui_ImGuiIO_Visitor_FieldSet__ConfigDragClickToInputText(mPointer, value);
		}
	}
	public bool ConfigWindowsResizeFromEdges
	{
		get
		{
			return TitanImGui_ImGuiIO_Visitor_FieldGet__ConfigWindowsResizeFromEdges(mPointer) == 0 ? false : true;
		}
		set
		{
			TitanImGui_ImGuiIO_Visitor_FieldSet__ConfigWindowsResizeFromEdges(mPointer, value);
		}
	}
	public bool ConfigWindowsMoveFromTitleBarOnly
	{
		get
		{
			return TitanImGui_ImGuiIO_Visitor_FieldGet__ConfigWindowsMoveFromTitleBarOnly(mPointer) == 0 ? false : true;
		}
		set
		{
			TitanImGui_ImGuiIO_Visitor_FieldSet__ConfigWindowsMoveFromTitleBarOnly(mPointer, value);
		}
	}
	public bool ConfigWindowsCopyContentsWithCtrlC
	{
		get
		{
			return TitanImGui_ImGuiIO_Visitor_FieldGet__ConfigWindowsCopyContentsWithCtrlC(mPointer) == 0 ? false : true;
		}
		set
		{
			TitanImGui_ImGuiIO_Visitor_FieldSet__ConfigWindowsCopyContentsWithCtrlC(mPointer, value);
		}
	}
	public bool ConfigScrollbarScrollByPage
	{
		get
		{
			return TitanImGui_ImGuiIO_Visitor_FieldGet__ConfigScrollbarScrollByPage(mPointer) == 0 ? false : true;
		}
		set
		{
			TitanImGui_ImGuiIO_Visitor_FieldSet__ConfigScrollbarScrollByPage(mPointer, value);
		}
	}
	public float ConfigMemoryCompactTimer
	{
		get
		{
			return TitanImGui_ImGuiIO_Visitor_FieldGet__ConfigMemoryCompactTimer(mPointer);
		}
		set
		{
			TitanImGui_ImGuiIO_Visitor_FieldSet__ConfigMemoryCompactTimer(mPointer, value);
		}
	}
	public float MouseDoubleClickTime
	{
		get
		{
			return TitanImGui_ImGuiIO_Visitor_FieldGet__MouseDoubleClickTime(mPointer);
		}
		set
		{
			TitanImGui_ImGuiIO_Visitor_FieldSet__MouseDoubleClickTime(mPointer, value);
		}
	}
	public float MouseDoubleClickMaxDist
	{
		get
		{
			return TitanImGui_ImGuiIO_Visitor_FieldGet__MouseDoubleClickMaxDist(mPointer);
		}
		set
		{
			TitanImGui_ImGuiIO_Visitor_FieldSet__MouseDoubleClickMaxDist(mPointer, value);
		}
	}
	public float MouseDragThreshold
	{
		get
		{
			return TitanImGui_ImGuiIO_Visitor_FieldGet__MouseDragThreshold(mPointer);
		}
		set
		{
			TitanImGui_ImGuiIO_Visitor_FieldSet__MouseDragThreshold(mPointer, value);
		}
	}
	public float KeyRepeatDelay
	{
		get
		{
			return TitanImGui_ImGuiIO_Visitor_FieldGet__KeyRepeatDelay(mPointer);
		}
		set
		{
			TitanImGui_ImGuiIO_Visitor_FieldSet__KeyRepeatDelay(mPointer, value);
		}
	}
	public float KeyRepeatRate
	{
		get
		{
			return TitanImGui_ImGuiIO_Visitor_FieldGet__KeyRepeatRate(mPointer);
		}
		set
		{
			TitanImGui_ImGuiIO_Visitor_FieldSet__KeyRepeatRate(mPointer, value);
		}
	}
	public bool ConfigErrorRecovery
	{
		get
		{
			return TitanImGui_ImGuiIO_Visitor_FieldGet__ConfigErrorRecovery(mPointer) == 0 ? false : true;
		}
		set
		{
			TitanImGui_ImGuiIO_Visitor_FieldSet__ConfigErrorRecovery(mPointer, value);
		}
	}
	public bool ConfigErrorRecoveryEnableAssert
	{
		get
		{
			return TitanImGui_ImGuiIO_Visitor_FieldGet__ConfigErrorRecoveryEnableAssert(mPointer) == 0 ? false : true;
		}
		set
		{
			TitanImGui_ImGuiIO_Visitor_FieldSet__ConfigErrorRecoveryEnableAssert(mPointer, value);
		}
	}
	public bool ConfigErrorRecoveryEnableDebugLog
	{
		get
		{
			return TitanImGui_ImGuiIO_Visitor_FieldGet__ConfigErrorRecoveryEnableDebugLog(mPointer) == 0 ? false : true;
		}
		set
		{
			TitanImGui_ImGuiIO_Visitor_FieldSet__ConfigErrorRecoveryEnableDebugLog(mPointer, value);
		}
	}
	public bool ConfigErrorRecoveryEnableTooltip
	{
		get
		{
			return TitanImGui_ImGuiIO_Visitor_FieldGet__ConfigErrorRecoveryEnableTooltip(mPointer) == 0 ? false : true;
		}
		set
		{
			TitanImGui_ImGuiIO_Visitor_FieldSet__ConfigErrorRecoveryEnableTooltip(mPointer, value);
		}
	}
	public bool ConfigDebugIsDebuggerPresent
	{
		get
		{
			return TitanImGui_ImGuiIO_Visitor_FieldGet__ConfigDebugIsDebuggerPresent(mPointer) == 0 ? false : true;
		}
		set
		{
			TitanImGui_ImGuiIO_Visitor_FieldSet__ConfigDebugIsDebuggerPresent(mPointer, value);
		}
	}
	public bool ConfigDebugHighlightIdConflicts
	{
		get
		{
			return TitanImGui_ImGuiIO_Visitor_FieldGet__ConfigDebugHighlightIdConflicts(mPointer) == 0 ? false : true;
		}
		set
		{
			TitanImGui_ImGuiIO_Visitor_FieldSet__ConfigDebugHighlightIdConflicts(mPointer, value);
		}
	}
	public bool ConfigDebugHighlightIdConflictsShowItemPicker
	{
		get
		{
			return TitanImGui_ImGuiIO_Visitor_FieldGet__ConfigDebugHighlightIdConflictsShowItemPicker(mPointer) == 0 ? false : true;
		}
		set
		{
			TitanImGui_ImGuiIO_Visitor_FieldSet__ConfigDebugHighlightIdConflictsShowItemPicker(mPointer, value);
		}
	}
	public bool ConfigDebugBeginReturnValueOnce
	{
		get
		{
			return TitanImGui_ImGuiIO_Visitor_FieldGet__ConfigDebugBeginReturnValueOnce(mPointer) == 0 ? false : true;
		}
		set
		{
			TitanImGui_ImGuiIO_Visitor_FieldSet__ConfigDebugBeginReturnValueOnce(mPointer, value);
		}
	}
	public bool ConfigDebugBeginReturnValueLoop
	{
		get
		{
			return TitanImGui_ImGuiIO_Visitor_FieldGet__ConfigDebugBeginReturnValueLoop(mPointer) == 0 ? false : true;
		}
		set
		{
			TitanImGui_ImGuiIO_Visitor_FieldSet__ConfigDebugBeginReturnValueLoop(mPointer, value);
		}
	}
	public bool ConfigDebugIgnoreFocusLoss
	{
		get
		{
			return TitanImGui_ImGuiIO_Visitor_FieldGet__ConfigDebugIgnoreFocusLoss(mPointer) == 0 ? false : true;
		}
		set
		{
			TitanImGui_ImGuiIO_Visitor_FieldSet__ConfigDebugIgnoreFocusLoss(mPointer, value);
		}
	}
	public bool ConfigDebugIniSettings
	{
		get
		{
			return TitanImGui_ImGuiIO_Visitor_FieldGet__ConfigDebugIniSettings(mPointer) == 0 ? false : true;
		}
		set
		{
			TitanImGui_ImGuiIO_Visitor_FieldSet__ConfigDebugIniSettings(mPointer, value);
		}
	}
	public string BackendPlatformName
	{
		get
		{
			return EngineNS.Rtti.TtNativeCoreProvider.MarshalPtrUtf8((IntPtr)TitanImGui_ImGuiIO_Visitor_FieldGet__BackendPlatformName(mPointer));
		}
		set
		{
			TitanImGui_ImGuiIO_Visitor_FieldSet__BackendPlatformName(mPointer, value);
		}
	}
	public string BackendRendererName
	{
		get
		{
			return EngineNS.Rtti.TtNativeCoreProvider.MarshalPtrUtf8((IntPtr)TitanImGui_ImGuiIO_Visitor_FieldGet__BackendRendererName(mPointer));
		}
		set
		{
			TitanImGui_ImGuiIO_Visitor_FieldSet__BackendRendererName(mPointer, value);
		}
	}
	public void* BackendPlatformUserData
	{
		get
		{
			return TitanImGui_ImGuiIO_Visitor_FieldGet__BackendPlatformUserData(mPointer);
		}
		set
		{
			TitanImGui_ImGuiIO_Visitor_FieldSet__BackendPlatformUserData(mPointer, value);
		}
	}
	public void* BackendRendererUserData
	{
		get
		{
			return TitanImGui_ImGuiIO_Visitor_FieldGet__BackendRendererUserData(mPointer);
		}
		set
		{
			TitanImGui_ImGuiIO_Visitor_FieldSet__BackendRendererUserData(mPointer, value);
		}
	}
	public void* BackendLanguageUserData
	{
		get
		{
			return TitanImGui_ImGuiIO_Visitor_FieldGet__BackendLanguageUserData(mPointer);
		}
		set
		{
			TitanImGui_ImGuiIO_Visitor_FieldSet__BackendLanguageUserData(mPointer, value);
		}
	}
	public bool WantCaptureMouse
	{
		get
		{
			return TitanImGui_ImGuiIO_Visitor_FieldGet__WantCaptureMouse(mPointer) == 0 ? false : true;
		}
		set
		{
			TitanImGui_ImGuiIO_Visitor_FieldSet__WantCaptureMouse(mPointer, value);
		}
	}
	public bool WantCaptureKeyboard
	{
		get
		{
			return TitanImGui_ImGuiIO_Visitor_FieldGet__WantCaptureKeyboard(mPointer) == 0 ? false : true;
		}
		set
		{
			TitanImGui_ImGuiIO_Visitor_FieldSet__WantCaptureKeyboard(mPointer, value);
		}
	}
	public bool WantTextInput
	{
		get
		{
			return TitanImGui_ImGuiIO_Visitor_FieldGet__WantTextInput(mPointer) == 0 ? false : true;
		}
		set
		{
			TitanImGui_ImGuiIO_Visitor_FieldSet__WantTextInput(mPointer, value);
		}
	}
	public bool WantSetMousePos
	{
		get
		{
			return TitanImGui_ImGuiIO_Visitor_FieldGet__WantSetMousePos(mPointer) == 0 ? false : true;
		}
		set
		{
			TitanImGui_ImGuiIO_Visitor_FieldSet__WantSetMousePos(mPointer, value);
		}
	}
	public bool WantSaveIniSettings
	{
		get
		{
			return TitanImGui_ImGuiIO_Visitor_FieldGet__WantSaveIniSettings(mPointer) == 0 ? false : true;
		}
		set
		{
			TitanImGui_ImGuiIO_Visitor_FieldSet__WantSaveIniSettings(mPointer, value);
		}
	}
	public bool NavActive
	{
		get
		{
			return TitanImGui_ImGuiIO_Visitor_FieldGet__NavActive(mPointer) == 0 ? false : true;
		}
		set
		{
			TitanImGui_ImGuiIO_Visitor_FieldSet__NavActive(mPointer, value);
		}
	}
	public bool NavVisible
	{
		get
		{
			return TitanImGui_ImGuiIO_Visitor_FieldGet__NavVisible(mPointer) == 0 ? false : true;
		}
		set
		{
			TitanImGui_ImGuiIO_Visitor_FieldSet__NavVisible(mPointer, value);
		}
	}
	public float Framerate
	{
		get
		{
			return TitanImGui_ImGuiIO_Visitor_FieldGet__Framerate(mPointer);
		}
		set
		{
			TitanImGui_ImGuiIO_Visitor_FieldSet__Framerate(mPointer, value);
		}
	}
	public int MetricsRenderVertices
	{
		get
		{
			return TitanImGui_ImGuiIO_Visitor_FieldGet__MetricsRenderVertices(mPointer);
		}
		set
		{
			TitanImGui_ImGuiIO_Visitor_FieldSet__MetricsRenderVertices(mPointer, value);
		}
	}
	public int MetricsRenderIndices
	{
		get
		{
			return TitanImGui_ImGuiIO_Visitor_FieldGet__MetricsRenderIndices(mPointer);
		}
		set
		{
			TitanImGui_ImGuiIO_Visitor_FieldSet__MetricsRenderIndices(mPointer, value);
		}
	}
	public int MetricsRenderWindows
	{
		get
		{
			return TitanImGui_ImGuiIO_Visitor_FieldGet__MetricsRenderWindows(mPointer);
		}
		set
		{
			TitanImGui_ImGuiIO_Visitor_FieldSet__MetricsRenderWindows(mPointer, value);
		}
	}
	public int MetricsActiveWindows
	{
		get
		{
			return TitanImGui_ImGuiIO_Visitor_FieldGet__MetricsActiveWindows(mPointer);
		}
		set
		{
			TitanImGui_ImGuiIO_Visitor_FieldSet__MetricsActiveWindows(mPointer, value);
		}
	}
	public EngineNS.Vector2 MouseDelta
	{
		get
		{
			return TitanImGui_ImGuiIO_Visitor_FieldGet__MouseDelta(mPointer);
		}
		set
		{
			TitanImGui_ImGuiIO_Visitor_FieldSet__MouseDelta(mPointer, value);
		}
	}
	public EngineNS.Vector2 MousePos
	{
		get
		{
			return TitanImGui_ImGuiIO_Visitor_FieldGet__MousePos(mPointer);
		}
		set
		{
			TitanImGui_ImGuiIO_Visitor_FieldSet__MousePos(mPointer, value);
		}
	}
	public bool* MouseDown
	{
		get
		{
			return TitanImGui_ImGuiIO_Visitor_FieldGet__MouseDown(mPointer);
		}
		set
		{
			TitanImGui_ImGuiIO_Visitor_FieldSet__MouseDown(mPointer, value);
		}
	}
	public float MouseWheel
	{
		get
		{
			return TitanImGui_ImGuiIO_Visitor_FieldGet__MouseWheel(mPointer);
		}
		set
		{
			TitanImGui_ImGuiIO_Visitor_FieldSet__MouseWheel(mPointer, value);
		}
	}
	public float MouseWheelH
	{
		get
		{
			return TitanImGui_ImGuiIO_Visitor_FieldGet__MouseWheelH(mPointer);
		}
		set
		{
			TitanImGui_ImGuiIO_Visitor_FieldSet__MouseWheelH(mPointer, value);
		}
	}
	public ImGuiMouseSource MouseSource
	{
		get
		{
			return TitanImGui_ImGuiIO_Visitor_FieldGet__MouseSource(mPointer);
		}
		set
		{
			TitanImGui_ImGuiIO_Visitor_FieldSet__MouseSource(mPointer, value);
		}
	}
	public uint MouseHoveredViewport
	{
		get
		{
			return TitanImGui_ImGuiIO_Visitor_FieldGet__MouseHoveredViewport(mPointer);
		}
		set
		{
			TitanImGui_ImGuiIO_Visitor_FieldSet__MouseHoveredViewport(mPointer, value);
		}
	}
	public bool KeyCtrl
	{
		get
		{
			return TitanImGui_ImGuiIO_Visitor_FieldGet__KeyCtrl(mPointer) == 0 ? false : true;
		}
		set
		{
			TitanImGui_ImGuiIO_Visitor_FieldSet__KeyCtrl(mPointer, value);
		}
	}
	public bool KeyShift
	{
		get
		{
			return TitanImGui_ImGuiIO_Visitor_FieldGet__KeyShift(mPointer) == 0 ? false : true;
		}
		set
		{
			TitanImGui_ImGuiIO_Visitor_FieldSet__KeyShift(mPointer, value);
		}
	}
	public bool KeyAlt
	{
		get
		{
			return TitanImGui_ImGuiIO_Visitor_FieldGet__KeyAlt(mPointer) == 0 ? false : true;
		}
		set
		{
			TitanImGui_ImGuiIO_Visitor_FieldSet__KeyAlt(mPointer, value);
		}
	}
	public bool KeySuper
	{
		get
		{
			return TitanImGui_ImGuiIO_Visitor_FieldGet__KeySuper(mPointer) == 0 ? false : true;
		}
		set
		{
			TitanImGui_ImGuiIO_Visitor_FieldSet__KeySuper(mPointer, value);
		}
	}
	public int KeyMods
	{
		get
		{
			return TitanImGui_ImGuiIO_Visitor_FieldGet__KeyMods(mPointer);
		}
		set
		{
			TitanImGui_ImGuiIO_Visitor_FieldSet__KeyMods(mPointer, value);
		}
	}
	public bool WantCaptureMouseUnlessPopupClose
	{
		get
		{
			return TitanImGui_ImGuiIO_Visitor_FieldGet__WantCaptureMouseUnlessPopupClose(mPointer) == 0 ? false : true;
		}
		set
		{
			TitanImGui_ImGuiIO_Visitor_FieldSet__WantCaptureMouseUnlessPopupClose(mPointer, value);
		}
	}
	public EngineNS.Vector2 MousePosPrev
	{
		get
		{
			return TitanImGui_ImGuiIO_Visitor_FieldGet__MousePosPrev(mPointer);
		}
		set
		{
			TitanImGui_ImGuiIO_Visitor_FieldSet__MousePosPrev(mPointer, value);
		}
	}
	public EngineNS.Vector2* MouseClickedPos
	{
		get
		{
			return TitanImGui_ImGuiIO_Visitor_FieldGet__MouseClickedPos(mPointer);
		}
		set
		{
			TitanImGui_ImGuiIO_Visitor_FieldSet__MouseClickedPos(mPointer, value);
		}
	}
	public double* MouseClickedTime
	{
		get
		{
			return TitanImGui_ImGuiIO_Visitor_FieldGet__MouseClickedTime(mPointer);
		}
		set
		{
			TitanImGui_ImGuiIO_Visitor_FieldSet__MouseClickedTime(mPointer, value);
		}
	}
	public bool* MouseClicked
	{
		get
		{
			return TitanImGui_ImGuiIO_Visitor_FieldGet__MouseClicked(mPointer);
		}
		set
		{
			TitanImGui_ImGuiIO_Visitor_FieldSet__MouseClicked(mPointer, value);
		}
	}
	public bool* MouseDoubleClicked
	{
		get
		{
			return TitanImGui_ImGuiIO_Visitor_FieldGet__MouseDoubleClicked(mPointer);
		}
		set
		{
			TitanImGui_ImGuiIO_Visitor_FieldSet__MouseDoubleClicked(mPointer, value);
		}
	}
	public ushort* MouseClickedCount
	{
		get
		{
			return TitanImGui_ImGuiIO_Visitor_FieldGet__MouseClickedCount(mPointer);
		}
		set
		{
			TitanImGui_ImGuiIO_Visitor_FieldSet__MouseClickedCount(mPointer, value);
		}
	}
	public ushort* MouseClickedLastCount
	{
		get
		{
			return TitanImGui_ImGuiIO_Visitor_FieldGet__MouseClickedLastCount(mPointer);
		}
		set
		{
			TitanImGui_ImGuiIO_Visitor_FieldSet__MouseClickedLastCount(mPointer, value);
		}
	}
	public bool* MouseReleased
	{
		get
		{
			return TitanImGui_ImGuiIO_Visitor_FieldGet__MouseReleased(mPointer);
		}
		set
		{
			TitanImGui_ImGuiIO_Visitor_FieldSet__MouseReleased(mPointer, value);
		}
	}
	public double* MouseReleasedTime
	{
		get
		{
			return TitanImGui_ImGuiIO_Visitor_FieldGet__MouseReleasedTime(mPointer);
		}
		set
		{
			TitanImGui_ImGuiIO_Visitor_FieldSet__MouseReleasedTime(mPointer, value);
		}
	}
	public bool* MouseDownOwned
	{
		get
		{
			return TitanImGui_ImGuiIO_Visitor_FieldGet__MouseDownOwned(mPointer);
		}
		set
		{
			TitanImGui_ImGuiIO_Visitor_FieldSet__MouseDownOwned(mPointer, value);
		}
	}
	public bool* MouseDownOwnedUnlessPopupClose
	{
		get
		{
			return TitanImGui_ImGuiIO_Visitor_FieldGet__MouseDownOwnedUnlessPopupClose(mPointer);
		}
		set
		{
			TitanImGui_ImGuiIO_Visitor_FieldSet__MouseDownOwnedUnlessPopupClose(mPointer, value);
		}
	}
	public bool MouseWheelRequestAxisSwap
	{
		get
		{
			return TitanImGui_ImGuiIO_Visitor_FieldGet__MouseWheelRequestAxisSwap(mPointer) == 0 ? false : true;
		}
		set
		{
			TitanImGui_ImGuiIO_Visitor_FieldSet__MouseWheelRequestAxisSwap(mPointer, value);
		}
	}
	public bool MouseCtrlLeftAsRightClick
	{
		get
		{
			return TitanImGui_ImGuiIO_Visitor_FieldGet__MouseCtrlLeftAsRightClick(mPointer) == 0 ? false : true;
		}
		set
		{
			TitanImGui_ImGuiIO_Visitor_FieldSet__MouseCtrlLeftAsRightClick(mPointer, value);
		}
	}
	public float* MouseDownDuration
	{
		get
		{
			return TitanImGui_ImGuiIO_Visitor_FieldGet__MouseDownDuration(mPointer);
		}
		set
		{
			TitanImGui_ImGuiIO_Visitor_FieldSet__MouseDownDuration(mPointer, value);
		}
	}
	public float* MouseDownDurationPrev
	{
		get
		{
			return TitanImGui_ImGuiIO_Visitor_FieldGet__MouseDownDurationPrev(mPointer);
		}
		set
		{
			TitanImGui_ImGuiIO_Visitor_FieldSet__MouseDownDurationPrev(mPointer, value);
		}
	}
	public EngineNS.Vector2* MouseDragMaxDistanceAbs
	{
		get
		{
			return TitanImGui_ImGuiIO_Visitor_FieldGet__MouseDragMaxDistanceAbs(mPointer);
		}
		set
		{
			TitanImGui_ImGuiIO_Visitor_FieldSet__MouseDragMaxDistanceAbs(mPointer, value);
		}
	}
	public float* MouseDragMaxDistanceSqr
	{
		get
		{
			return TitanImGui_ImGuiIO_Visitor_FieldGet__MouseDragMaxDistanceSqr(mPointer);
		}
		set
		{
			TitanImGui_ImGuiIO_Visitor_FieldSet__MouseDragMaxDistanceSqr(mPointer, value);
		}
	}
	public float PenPressure
	{
		get
		{
			return TitanImGui_ImGuiIO_Visitor_FieldGet__PenPressure(mPointer);
		}
		set
		{
			TitanImGui_ImGuiIO_Visitor_FieldSet__PenPressure(mPointer, value);
		}
	}
	public bool AppFocusLost
	{
		get
		{
			return TitanImGui_ImGuiIO_Visitor_FieldGet__AppFocusLost(mPointer) == 0 ? false : true;
		}
		set
		{
			TitanImGui_ImGuiIO_Visitor_FieldSet__AppFocusLost(mPointer, value);
		}
	}
	public bool AppAcceptingEvents
	{
		get
		{
			return TitanImGui_ImGuiIO_Visitor_FieldGet__AppAcceptingEvents(mPointer) == 0 ? false : true;
		}
		set
		{
			TitanImGui_ImGuiIO_Visitor_FieldSet__AppAcceptingEvents(mPointer, value);
		}
	}
	public Wchar16 InputQueueSurrogate
	{
		get
		{
			return TitanImGui_ImGuiIO_Visitor_FieldGet__InputQueueSurrogate(mPointer);
		}
		set
		{
			TitanImGui_ImGuiIO_Visitor_FieldSet__InputQueueSurrogate(mPointer, value);
		}
	}
	public float FontGlobalScale
	{
		get
		{
			return TitanImGui_ImGuiIO_Visitor_FieldGet__FontGlobalScale(mPointer);
		}
		set
		{
			TitanImGui_ImGuiIO_Visitor_FieldSet__FontGlobalScale(mPointer, value);
		}
	}
	public unsafe delegate sbyte* FDelegate_GetClipboardTextFn(void* arg0);
	public FDelegate_GetClipboardTextFn GetClipboardTextFn
	{
		get
		{
			return TitanImGui_ImGuiIO_Visitor_FieldGet__GetClipboardTextFn(mPointer);
		}
		set
		{
			TitanImGui_ImGuiIO_Visitor_FieldSet__GetClipboardTextFn(mPointer, value);
		}
	}
	public unsafe delegate void FDelegate_SetClipboardTextFn(void* arg0,sbyte* arg1);
	public FDelegate_SetClipboardTextFn SetClipboardTextFn
	{
		get
		{
			return TitanImGui_ImGuiIO_Visitor_FieldGet__SetClipboardTextFn(mPointer);
		}
		set
		{
			TitanImGui_ImGuiIO_Visitor_FieldSet__SetClipboardTextFn(mPointer, value);
		}
	}
	public void* ClipboardUserData
	{
		get
		{
			return TitanImGui_ImGuiIO_Visitor_FieldGet__ClipboardUserData(mPointer);
		}
		set
		{
			TitanImGui_ImGuiIO_Visitor_FieldSet__ClipboardUserData(mPointer, value);
		}
	}
	#endregion
	#region Function
	public void AddKeyEvent(ImGuiKey key,bool down)
	{
		TitanImGui_ImGuiIO_Visitor_AddKeyEvent_2200478491(mPointer, key, down);
	}
	public void AddKeyAnalogEvent(ImGuiKey key,bool down,float v)
	{
		TitanImGui_ImGuiIO_Visitor_AddKeyAnalogEvent_2648809663(mPointer, key, down, v);
	}
	public void AddMousePosEvent(float x,float y)
	{
		TitanImGui_ImGuiIO_Visitor_AddMousePosEvent_996365349(mPointer, x, y);
	}
	public void AddMouseButtonEvent(int button,bool down)
	{
		TitanImGui_ImGuiIO_Visitor_AddMouseButtonEvent_2814434660(mPointer, button, down);
	}
	public void AddMouseWheelEvent(float wheel_x,float wheel_y)
	{
		TitanImGui_ImGuiIO_Visitor_AddMouseWheelEvent_996365349(mPointer, wheel_x, wheel_y);
	}
	public void AddMouseSourceEvent(ImGuiMouseSource source)
	{
		TitanImGui_ImGuiIO_Visitor_AddMouseSourceEvent_4119589844(mPointer, source);
	}
	public void AddMouseViewportEvent(uint id)
	{
		TitanImGui_ImGuiIO_Visitor_AddMouseViewportEvent_2252480719(mPointer, id);
	}
	public void AddFocusEvent(bool focused)
	{
		TitanImGui_ImGuiIO_Visitor_AddFocusEvent_2077628183(mPointer, focused);
	}
	public void AddInputCharacter(uint c)
	{
		TitanImGui_ImGuiIO_Visitor_AddInputCharacter_1961468199(mPointer, c);
	}
	public void AddInputCharacterUTF16(Wchar16 c)
	{
		TitanImGui_ImGuiIO_Visitor_AddInputCharacterUTF16_1265636733(mPointer, c);
	}
	public void AddInputCharactersUTF8(sbyte* str)
	{
		TitanImGui_ImGuiIO_Visitor_AddInputCharactersUTF8_2602414842(mPointer, str);
	}
	public void AddInputCharactersUTF8( in sbyte str)
	{
		fixed(sbyte* pinned_str = &str)
		{
			AddInputCharactersUTF8(pinned_str);
		}
	}
	public void SetKeyEventNativeData(ImGuiKey key,int native_keycode,int native_scancode,int native_legacy_index)
	{
		TitanImGui_ImGuiIO_Visitor_SetKeyEventNativeData_1655477210(mPointer, key, native_keycode, native_scancode, native_legacy_index);
	}
	public void SetAppAcceptingEvents(bool accepting_events)
	{
		TitanImGui_ImGuiIO_Visitor_SetAppAcceptingEvents_2077628183(mPointer, accepting_events);
	}
	public void ClearEventsQueue()
	{
		TitanImGui_ImGuiIO_Visitor_ClearEventsQueue_2960189489(mPointer);
	}
	public void ClearInputKeys()
	{
		TitanImGui_ImGuiIO_Visitor_ClearInputKeys_2960189489(mPointer);
	}
	public void ClearInputMouse()
	{
		TitanImGui_ImGuiIO_Visitor_ClearInputMouse_2960189489(mPointer);
	}
	#endregion
	#region Core SDK
	const string ModuleNC = EngineNS.CoreSDK.CoreModule;
	//Constructor&Cast
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	extern static EngineNS.FRttiStruct* TitanImGui_ImGuiIO_Visitor_GetTypeRtti();
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	extern static ImGuiIO* TitanImGui_ImGuiIO_Visitor_CreateInstance_2960189489();
	//Fields
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static ImGuiConfigFlags_ TitanImGui_ImGuiIO_Visitor_FieldGet__ConfigFlags(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImGuiIO_Visitor_FieldSet__ConfigFlags(void* self, ImGuiConfigFlags_ value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static ImGuiBackendFlags_ TitanImGui_ImGuiIO_Visitor_FieldGet__BackendFlags(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImGuiIO_Visitor_FieldSet__BackendFlags(void* self, ImGuiBackendFlags_ value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static EngineNS.Vector2 TitanImGui_ImGuiIO_Visitor_FieldGet__DisplaySize(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImGuiIO_Visitor_FieldSet__DisplaySize(void* self, EngineNS.Vector2 value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static EngineNS.Vector2 TitanImGui_ImGuiIO_Visitor_FieldGet__DisplayFramebufferScale(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImGuiIO_Visitor_FieldSet__DisplayFramebufferScale(void* self, EngineNS.Vector2 value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static float TitanImGui_ImGuiIO_Visitor_FieldGet__DeltaTime(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImGuiIO_Visitor_FieldSet__DeltaTime(void* self, float value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static float TitanImGui_ImGuiIO_Visitor_FieldGet__IniSavingRate(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImGuiIO_Visitor_FieldSet__IniSavingRate(void* self, float value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static sbyte* TitanImGui_ImGuiIO_Visitor_FieldGet__IniFilename(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImGuiIO_Visitor_FieldSet__IniFilename(void* self, [MarshalAs(UnmanagedType.LPUTF8Str)] string value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static sbyte* TitanImGui_ImGuiIO_Visitor_FieldGet__LogFilename(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImGuiIO_Visitor_FieldSet__LogFilename(void* self, [MarshalAs(UnmanagedType.LPUTF8Str)] string value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void* TitanImGui_ImGuiIO_Visitor_FieldGet__UserData(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImGuiIO_Visitor_FieldSet__UserData(void* self, void* value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static ImFontAtlas* TitanImGui_ImGuiIO_Visitor_FieldGet__Fonts(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImGuiIO_Visitor_FieldSet__Fonts(void* self, ImFontAtlas* value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static ImFont* TitanImGui_ImGuiIO_Visitor_FieldGet__FontDefault(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImGuiIO_Visitor_FieldSet__FontDefault(void* self, ImFont* value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static sbyte TitanImGui_ImGuiIO_Visitor_FieldGet__FontAllowUserScaling(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImGuiIO_Visitor_FieldSet__FontAllowUserScaling(void* self, bool value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static sbyte TitanImGui_ImGuiIO_Visitor_FieldGet__ConfigNavSwapGamepadButtons(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImGuiIO_Visitor_FieldSet__ConfigNavSwapGamepadButtons(void* self, bool value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static sbyte TitanImGui_ImGuiIO_Visitor_FieldGet__ConfigNavMoveSetMousePos(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImGuiIO_Visitor_FieldSet__ConfigNavMoveSetMousePos(void* self, bool value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static sbyte TitanImGui_ImGuiIO_Visitor_FieldGet__ConfigNavCaptureKeyboard(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImGuiIO_Visitor_FieldSet__ConfigNavCaptureKeyboard(void* self, bool value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static sbyte TitanImGui_ImGuiIO_Visitor_FieldGet__ConfigNavEscapeClearFocusItem(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImGuiIO_Visitor_FieldSet__ConfigNavEscapeClearFocusItem(void* self, bool value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static sbyte TitanImGui_ImGuiIO_Visitor_FieldGet__ConfigNavEscapeClearFocusWindow(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImGuiIO_Visitor_FieldSet__ConfigNavEscapeClearFocusWindow(void* self, bool value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static sbyte TitanImGui_ImGuiIO_Visitor_FieldGet__ConfigNavCursorVisibleAuto(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImGuiIO_Visitor_FieldSet__ConfigNavCursorVisibleAuto(void* self, bool value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static sbyte TitanImGui_ImGuiIO_Visitor_FieldGet__ConfigNavCursorVisibleAlways(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImGuiIO_Visitor_FieldSet__ConfigNavCursorVisibleAlways(void* self, bool value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static sbyte TitanImGui_ImGuiIO_Visitor_FieldGet__ConfigDockingNoSplit(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImGuiIO_Visitor_FieldSet__ConfigDockingNoSplit(void* self, bool value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static sbyte TitanImGui_ImGuiIO_Visitor_FieldGet__ConfigDockingNoDockingOver(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImGuiIO_Visitor_FieldSet__ConfigDockingNoDockingOver(void* self, bool value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static sbyte TitanImGui_ImGuiIO_Visitor_FieldGet__ConfigDockingWithShift(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImGuiIO_Visitor_FieldSet__ConfigDockingWithShift(void* self, bool value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static sbyte TitanImGui_ImGuiIO_Visitor_FieldGet__ConfigDockingAlwaysTabBar(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImGuiIO_Visitor_FieldSet__ConfigDockingAlwaysTabBar(void* self, bool value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static sbyte TitanImGui_ImGuiIO_Visitor_FieldGet__ConfigDockingTransparentPayload(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImGuiIO_Visitor_FieldSet__ConfigDockingTransparentPayload(void* self, bool value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static sbyte TitanImGui_ImGuiIO_Visitor_FieldGet__ConfigViewportsNoAutoMerge(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImGuiIO_Visitor_FieldSet__ConfigViewportsNoAutoMerge(void* self, bool value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static sbyte TitanImGui_ImGuiIO_Visitor_FieldGet__ConfigViewportsNoTaskBarIcon(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImGuiIO_Visitor_FieldSet__ConfigViewportsNoTaskBarIcon(void* self, bool value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static sbyte TitanImGui_ImGuiIO_Visitor_FieldGet__ConfigViewportsNoDecoration(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImGuiIO_Visitor_FieldSet__ConfigViewportsNoDecoration(void* self, bool value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static sbyte TitanImGui_ImGuiIO_Visitor_FieldGet__ConfigViewportsNoDefaultParent(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImGuiIO_Visitor_FieldSet__ConfigViewportsNoDefaultParent(void* self, bool value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static sbyte TitanImGui_ImGuiIO_Visitor_FieldGet__ConfigViewportsPlatformFocusSetsImGuiFocus(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImGuiIO_Visitor_FieldSet__ConfigViewportsPlatformFocusSetsImGuiFocus(void* self, bool value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static sbyte TitanImGui_ImGuiIO_Visitor_FieldGet__ConfigDpiScaleFonts(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImGuiIO_Visitor_FieldSet__ConfigDpiScaleFonts(void* self, bool value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static sbyte TitanImGui_ImGuiIO_Visitor_FieldGet__ConfigDpiScaleViewports(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImGuiIO_Visitor_FieldSet__ConfigDpiScaleViewports(void* self, bool value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static sbyte TitanImGui_ImGuiIO_Visitor_FieldGet__MouseDrawCursor(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImGuiIO_Visitor_FieldSet__MouseDrawCursor(void* self, bool value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static sbyte TitanImGui_ImGuiIO_Visitor_FieldGet__ConfigMacOSXBehaviors(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImGuiIO_Visitor_FieldSet__ConfigMacOSXBehaviors(void* self, bool value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static sbyte TitanImGui_ImGuiIO_Visitor_FieldGet__ConfigInputTrickleEventQueue(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImGuiIO_Visitor_FieldSet__ConfigInputTrickleEventQueue(void* self, bool value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static sbyte TitanImGui_ImGuiIO_Visitor_FieldGet__ConfigInputTextCursorBlink(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImGuiIO_Visitor_FieldSet__ConfigInputTextCursorBlink(void* self, bool value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static sbyte TitanImGui_ImGuiIO_Visitor_FieldGet__ConfigInputTextEnterKeepActive(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImGuiIO_Visitor_FieldSet__ConfigInputTextEnterKeepActive(void* self, bool value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static sbyte TitanImGui_ImGuiIO_Visitor_FieldGet__ConfigDragClickToInputText(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImGuiIO_Visitor_FieldSet__ConfigDragClickToInputText(void* self, bool value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static sbyte TitanImGui_ImGuiIO_Visitor_FieldGet__ConfigWindowsResizeFromEdges(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImGuiIO_Visitor_FieldSet__ConfigWindowsResizeFromEdges(void* self, bool value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static sbyte TitanImGui_ImGuiIO_Visitor_FieldGet__ConfigWindowsMoveFromTitleBarOnly(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImGuiIO_Visitor_FieldSet__ConfigWindowsMoveFromTitleBarOnly(void* self, bool value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static sbyte TitanImGui_ImGuiIO_Visitor_FieldGet__ConfigWindowsCopyContentsWithCtrlC(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImGuiIO_Visitor_FieldSet__ConfigWindowsCopyContentsWithCtrlC(void* self, bool value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static sbyte TitanImGui_ImGuiIO_Visitor_FieldGet__ConfigScrollbarScrollByPage(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImGuiIO_Visitor_FieldSet__ConfigScrollbarScrollByPage(void* self, bool value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static float TitanImGui_ImGuiIO_Visitor_FieldGet__ConfigMemoryCompactTimer(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImGuiIO_Visitor_FieldSet__ConfigMemoryCompactTimer(void* self, float value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static float TitanImGui_ImGuiIO_Visitor_FieldGet__MouseDoubleClickTime(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImGuiIO_Visitor_FieldSet__MouseDoubleClickTime(void* self, float value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static float TitanImGui_ImGuiIO_Visitor_FieldGet__MouseDoubleClickMaxDist(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImGuiIO_Visitor_FieldSet__MouseDoubleClickMaxDist(void* self, float value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static float TitanImGui_ImGuiIO_Visitor_FieldGet__MouseDragThreshold(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImGuiIO_Visitor_FieldSet__MouseDragThreshold(void* self, float value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static float TitanImGui_ImGuiIO_Visitor_FieldGet__KeyRepeatDelay(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImGuiIO_Visitor_FieldSet__KeyRepeatDelay(void* self, float value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static float TitanImGui_ImGuiIO_Visitor_FieldGet__KeyRepeatRate(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImGuiIO_Visitor_FieldSet__KeyRepeatRate(void* self, float value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static sbyte TitanImGui_ImGuiIO_Visitor_FieldGet__ConfigErrorRecovery(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImGuiIO_Visitor_FieldSet__ConfigErrorRecovery(void* self, bool value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static sbyte TitanImGui_ImGuiIO_Visitor_FieldGet__ConfigErrorRecoveryEnableAssert(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImGuiIO_Visitor_FieldSet__ConfigErrorRecoveryEnableAssert(void* self, bool value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static sbyte TitanImGui_ImGuiIO_Visitor_FieldGet__ConfigErrorRecoveryEnableDebugLog(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImGuiIO_Visitor_FieldSet__ConfigErrorRecoveryEnableDebugLog(void* self, bool value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static sbyte TitanImGui_ImGuiIO_Visitor_FieldGet__ConfigErrorRecoveryEnableTooltip(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImGuiIO_Visitor_FieldSet__ConfigErrorRecoveryEnableTooltip(void* self, bool value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static sbyte TitanImGui_ImGuiIO_Visitor_FieldGet__ConfigDebugIsDebuggerPresent(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImGuiIO_Visitor_FieldSet__ConfigDebugIsDebuggerPresent(void* self, bool value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static sbyte TitanImGui_ImGuiIO_Visitor_FieldGet__ConfigDebugHighlightIdConflicts(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImGuiIO_Visitor_FieldSet__ConfigDebugHighlightIdConflicts(void* self, bool value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static sbyte TitanImGui_ImGuiIO_Visitor_FieldGet__ConfigDebugHighlightIdConflictsShowItemPicker(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImGuiIO_Visitor_FieldSet__ConfigDebugHighlightIdConflictsShowItemPicker(void* self, bool value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static sbyte TitanImGui_ImGuiIO_Visitor_FieldGet__ConfigDebugBeginReturnValueOnce(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImGuiIO_Visitor_FieldSet__ConfigDebugBeginReturnValueOnce(void* self, bool value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static sbyte TitanImGui_ImGuiIO_Visitor_FieldGet__ConfigDebugBeginReturnValueLoop(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImGuiIO_Visitor_FieldSet__ConfigDebugBeginReturnValueLoop(void* self, bool value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static sbyte TitanImGui_ImGuiIO_Visitor_FieldGet__ConfigDebugIgnoreFocusLoss(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImGuiIO_Visitor_FieldSet__ConfigDebugIgnoreFocusLoss(void* self, bool value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static sbyte TitanImGui_ImGuiIO_Visitor_FieldGet__ConfigDebugIniSettings(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImGuiIO_Visitor_FieldSet__ConfigDebugIniSettings(void* self, bool value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static sbyte* TitanImGui_ImGuiIO_Visitor_FieldGet__BackendPlatformName(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImGuiIO_Visitor_FieldSet__BackendPlatformName(void* self, [MarshalAs(UnmanagedType.LPUTF8Str)] string value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static sbyte* TitanImGui_ImGuiIO_Visitor_FieldGet__BackendRendererName(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImGuiIO_Visitor_FieldSet__BackendRendererName(void* self, [MarshalAs(UnmanagedType.LPUTF8Str)] string value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void* TitanImGui_ImGuiIO_Visitor_FieldGet__BackendPlatformUserData(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImGuiIO_Visitor_FieldSet__BackendPlatformUserData(void* self, void* value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void* TitanImGui_ImGuiIO_Visitor_FieldGet__BackendRendererUserData(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImGuiIO_Visitor_FieldSet__BackendRendererUserData(void* self, void* value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void* TitanImGui_ImGuiIO_Visitor_FieldGet__BackendLanguageUserData(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImGuiIO_Visitor_FieldSet__BackendLanguageUserData(void* self, void* value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static sbyte TitanImGui_ImGuiIO_Visitor_FieldGet__WantCaptureMouse(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImGuiIO_Visitor_FieldSet__WantCaptureMouse(void* self, bool value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static sbyte TitanImGui_ImGuiIO_Visitor_FieldGet__WantCaptureKeyboard(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImGuiIO_Visitor_FieldSet__WantCaptureKeyboard(void* self, bool value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static sbyte TitanImGui_ImGuiIO_Visitor_FieldGet__WantTextInput(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImGuiIO_Visitor_FieldSet__WantTextInput(void* self, bool value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static sbyte TitanImGui_ImGuiIO_Visitor_FieldGet__WantSetMousePos(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImGuiIO_Visitor_FieldSet__WantSetMousePos(void* self, bool value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static sbyte TitanImGui_ImGuiIO_Visitor_FieldGet__WantSaveIniSettings(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImGuiIO_Visitor_FieldSet__WantSaveIniSettings(void* self, bool value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static sbyte TitanImGui_ImGuiIO_Visitor_FieldGet__NavActive(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImGuiIO_Visitor_FieldSet__NavActive(void* self, bool value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static sbyte TitanImGui_ImGuiIO_Visitor_FieldGet__NavVisible(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImGuiIO_Visitor_FieldSet__NavVisible(void* self, bool value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static float TitanImGui_ImGuiIO_Visitor_FieldGet__Framerate(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImGuiIO_Visitor_FieldSet__Framerate(void* self, float value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static int TitanImGui_ImGuiIO_Visitor_FieldGet__MetricsRenderVertices(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImGuiIO_Visitor_FieldSet__MetricsRenderVertices(void* self, int value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static int TitanImGui_ImGuiIO_Visitor_FieldGet__MetricsRenderIndices(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImGuiIO_Visitor_FieldSet__MetricsRenderIndices(void* self, int value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static int TitanImGui_ImGuiIO_Visitor_FieldGet__MetricsRenderWindows(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImGuiIO_Visitor_FieldSet__MetricsRenderWindows(void* self, int value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static int TitanImGui_ImGuiIO_Visitor_FieldGet__MetricsActiveWindows(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImGuiIO_Visitor_FieldSet__MetricsActiveWindows(void* self, int value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static EngineNS.Vector2 TitanImGui_ImGuiIO_Visitor_FieldGet__MouseDelta(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImGuiIO_Visitor_FieldSet__MouseDelta(void* self, EngineNS.Vector2 value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static EngineNS.Vector2 TitanImGui_ImGuiIO_Visitor_FieldGet__MousePos(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImGuiIO_Visitor_FieldSet__MousePos(void* self, EngineNS.Vector2 value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static bool* TitanImGui_ImGuiIO_Visitor_FieldGet__MouseDown(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImGuiIO_Visitor_FieldSet__MouseDown(void* self, bool* value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static float TitanImGui_ImGuiIO_Visitor_FieldGet__MouseWheel(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImGuiIO_Visitor_FieldSet__MouseWheel(void* self, float value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static float TitanImGui_ImGuiIO_Visitor_FieldGet__MouseWheelH(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImGuiIO_Visitor_FieldSet__MouseWheelH(void* self, float value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static ImGuiMouseSource TitanImGui_ImGuiIO_Visitor_FieldGet__MouseSource(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImGuiIO_Visitor_FieldSet__MouseSource(void* self, ImGuiMouseSource value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static uint TitanImGui_ImGuiIO_Visitor_FieldGet__MouseHoveredViewport(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImGuiIO_Visitor_FieldSet__MouseHoveredViewport(void* self, uint value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static sbyte TitanImGui_ImGuiIO_Visitor_FieldGet__KeyCtrl(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImGuiIO_Visitor_FieldSet__KeyCtrl(void* self, bool value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static sbyte TitanImGui_ImGuiIO_Visitor_FieldGet__KeyShift(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImGuiIO_Visitor_FieldSet__KeyShift(void* self, bool value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static sbyte TitanImGui_ImGuiIO_Visitor_FieldGet__KeyAlt(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImGuiIO_Visitor_FieldSet__KeyAlt(void* self, bool value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static sbyte TitanImGui_ImGuiIO_Visitor_FieldGet__KeySuper(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImGuiIO_Visitor_FieldSet__KeySuper(void* self, bool value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static int TitanImGui_ImGuiIO_Visitor_FieldGet__KeyMods(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImGuiIO_Visitor_FieldSet__KeyMods(void* self, int value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static sbyte TitanImGui_ImGuiIO_Visitor_FieldGet__WantCaptureMouseUnlessPopupClose(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImGuiIO_Visitor_FieldSet__WantCaptureMouseUnlessPopupClose(void* self, bool value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static EngineNS.Vector2 TitanImGui_ImGuiIO_Visitor_FieldGet__MousePosPrev(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImGuiIO_Visitor_FieldSet__MousePosPrev(void* self, EngineNS.Vector2 value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static EngineNS.Vector2* TitanImGui_ImGuiIO_Visitor_FieldGet__MouseClickedPos(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImGuiIO_Visitor_FieldSet__MouseClickedPos(void* self, EngineNS.Vector2* value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static double* TitanImGui_ImGuiIO_Visitor_FieldGet__MouseClickedTime(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImGuiIO_Visitor_FieldSet__MouseClickedTime(void* self, double* value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static bool* TitanImGui_ImGuiIO_Visitor_FieldGet__MouseClicked(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImGuiIO_Visitor_FieldSet__MouseClicked(void* self, bool* value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static bool* TitanImGui_ImGuiIO_Visitor_FieldGet__MouseDoubleClicked(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImGuiIO_Visitor_FieldSet__MouseDoubleClicked(void* self, bool* value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static ushort* TitanImGui_ImGuiIO_Visitor_FieldGet__MouseClickedCount(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImGuiIO_Visitor_FieldSet__MouseClickedCount(void* self, ushort* value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static ushort* TitanImGui_ImGuiIO_Visitor_FieldGet__MouseClickedLastCount(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImGuiIO_Visitor_FieldSet__MouseClickedLastCount(void* self, ushort* value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static bool* TitanImGui_ImGuiIO_Visitor_FieldGet__MouseReleased(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImGuiIO_Visitor_FieldSet__MouseReleased(void* self, bool* value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static double* TitanImGui_ImGuiIO_Visitor_FieldGet__MouseReleasedTime(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImGuiIO_Visitor_FieldSet__MouseReleasedTime(void* self, double* value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static bool* TitanImGui_ImGuiIO_Visitor_FieldGet__MouseDownOwned(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImGuiIO_Visitor_FieldSet__MouseDownOwned(void* self, bool* value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static bool* TitanImGui_ImGuiIO_Visitor_FieldGet__MouseDownOwnedUnlessPopupClose(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImGuiIO_Visitor_FieldSet__MouseDownOwnedUnlessPopupClose(void* self, bool* value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static sbyte TitanImGui_ImGuiIO_Visitor_FieldGet__MouseWheelRequestAxisSwap(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImGuiIO_Visitor_FieldSet__MouseWheelRequestAxisSwap(void* self, bool value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static sbyte TitanImGui_ImGuiIO_Visitor_FieldGet__MouseCtrlLeftAsRightClick(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImGuiIO_Visitor_FieldSet__MouseCtrlLeftAsRightClick(void* self, bool value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static float* TitanImGui_ImGuiIO_Visitor_FieldGet__MouseDownDuration(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImGuiIO_Visitor_FieldSet__MouseDownDuration(void* self, float* value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static float* TitanImGui_ImGuiIO_Visitor_FieldGet__MouseDownDurationPrev(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImGuiIO_Visitor_FieldSet__MouseDownDurationPrev(void* self, float* value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static EngineNS.Vector2* TitanImGui_ImGuiIO_Visitor_FieldGet__MouseDragMaxDistanceAbs(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImGuiIO_Visitor_FieldSet__MouseDragMaxDistanceAbs(void* self, EngineNS.Vector2* value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static float* TitanImGui_ImGuiIO_Visitor_FieldGet__MouseDragMaxDistanceSqr(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImGuiIO_Visitor_FieldSet__MouseDragMaxDistanceSqr(void* self, float* value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static float TitanImGui_ImGuiIO_Visitor_FieldGet__PenPressure(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImGuiIO_Visitor_FieldSet__PenPressure(void* self, float value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static sbyte TitanImGui_ImGuiIO_Visitor_FieldGet__AppFocusLost(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImGuiIO_Visitor_FieldSet__AppFocusLost(void* self, bool value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static sbyte TitanImGui_ImGuiIO_Visitor_FieldGet__AppAcceptingEvents(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImGuiIO_Visitor_FieldSet__AppAcceptingEvents(void* self, bool value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static Wchar16 TitanImGui_ImGuiIO_Visitor_FieldGet__InputQueueSurrogate(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImGuiIO_Visitor_FieldSet__InputQueueSurrogate(void* self, Wchar16 value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static float TitanImGui_ImGuiIO_Visitor_FieldGet__FontGlobalScale(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImGuiIO_Visitor_FieldSet__FontGlobalScale(void* self, float value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static FDelegate_GetClipboardTextFn TitanImGui_ImGuiIO_Visitor_FieldGet__GetClipboardTextFn(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImGuiIO_Visitor_FieldSet__GetClipboardTextFn(void* self, FDelegate_GetClipboardTextFn value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static FDelegate_SetClipboardTextFn TitanImGui_ImGuiIO_Visitor_FieldGet__SetClipboardTextFn(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImGuiIO_Visitor_FieldSet__SetClipboardTextFn(void* self, FDelegate_SetClipboardTextFn value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void* TitanImGui_ImGuiIO_Visitor_FieldGet__ClipboardUserData(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImGuiIO_Visitor_FieldSet__ClipboardUserData(void* self, void* value);
	//Functions
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	extern static void TitanImGui_ImGuiIO_Visitor_AddKeyEvent_2200478491(void* Self, ImGuiKey key,bool down);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	extern static void TitanImGui_ImGuiIO_Visitor_AddKeyAnalogEvent_2648809663(void* Self, ImGuiKey key,bool down,float v);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	extern static void TitanImGui_ImGuiIO_Visitor_AddMousePosEvent_996365349(void* Self, float x,float y);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	extern static void TitanImGui_ImGuiIO_Visitor_AddMouseButtonEvent_2814434660(void* Self, int button,bool down);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	extern static void TitanImGui_ImGuiIO_Visitor_AddMouseWheelEvent_996365349(void* Self, float wheel_x,float wheel_y);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	extern static void TitanImGui_ImGuiIO_Visitor_AddMouseSourceEvent_4119589844(void* Self, ImGuiMouseSource source);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	extern static void TitanImGui_ImGuiIO_Visitor_AddMouseViewportEvent_2252480719(void* Self, uint id);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	extern static void TitanImGui_ImGuiIO_Visitor_AddFocusEvent_2077628183(void* Self, bool focused);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	extern static void TitanImGui_ImGuiIO_Visitor_AddInputCharacter_1961468199(void* Self, uint c);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	extern static void TitanImGui_ImGuiIO_Visitor_AddInputCharacterUTF16_1265636733(void* Self, Wchar16 c);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	extern static void TitanImGui_ImGuiIO_Visitor_AddInputCharactersUTF8_2602414842(void* Self, sbyte* str);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	extern static void TitanImGui_ImGuiIO_Visitor_SetKeyEventNativeData_1655477210(void* Self, ImGuiKey key,int native_keycode,int native_scancode,int native_legacy_index);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	extern static void TitanImGui_ImGuiIO_Visitor_SetAppAcceptingEvents_2077628183(void* Self, bool accepting_events);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	extern static void TitanImGui_ImGuiIO_Visitor_ClearEventsQueue_2960189489(void* Self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	extern static void TitanImGui_ImGuiIO_Visitor_ClearInputKeys_2960189489(void* Self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	extern static void TitanImGui_ImGuiIO_Visitor_ClearInputMouse_2960189489(void* Self);
	//Cast
	#endregion
}
