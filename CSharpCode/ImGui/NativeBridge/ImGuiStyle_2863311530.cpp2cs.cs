//generated from legacy CppWeaving ImGui surface; native entrypoints routed through TitanImGuiBridge
using System;
using System.Runtime.InteropServices;
#if DEBUG
	using SuppressGC = EngineNS.Rtti.TtDummyAttribute;
#else
	using SuppressGC = System.Runtime.InteropServices.SuppressGCTransitionAttribute;
#endif


[System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Explicit, Size = 1340, Pack = 4)]
public unsafe partial struct ImGuiStyle : EngineNS.IPtrType
{
	#region StructLayout
	[System.Runtime.InteropServices.FieldOffset(0)]
	public float m_FontSizeBase;
	[System.Runtime.InteropServices.FieldOffset(4)]
	public float m_FontScaleMain;
	[System.Runtime.InteropServices.FieldOffset(8)]
	public float m_FontScaleDpi;
	[System.Runtime.InteropServices.FieldOffset(12)]
	public float m_Alpha;
	[System.Runtime.InteropServices.FieldOffset(16)]
	public float m_DisabledAlpha;
	[System.Runtime.InteropServices.FieldOffset(20)]
	public EngineNS.Vector2 m_WindowPadding;
	[System.Runtime.InteropServices.FieldOffset(28)]
	public float m_WindowRounding;
	[System.Runtime.InteropServices.FieldOffset(32)]
	public float m_WindowBorderSize;
	[System.Runtime.InteropServices.FieldOffset(36)]
	public float m_WindowBorderHoverPadding;
	[System.Runtime.InteropServices.FieldOffset(40)]
	public EngineNS.Vector2 m_WindowMinSize;
	[System.Runtime.InteropServices.FieldOffset(48)]
	public EngineNS.Vector2 m_WindowTitleAlign;
	[System.Runtime.InteropServices.FieldOffset(56)]
	public ImGuiDir m_WindowMenuButtonPosition;
	[System.Runtime.InteropServices.FieldOffset(60)]
	public float m_ChildRounding;
	[System.Runtime.InteropServices.FieldOffset(64)]
	public float m_ChildBorderSize;
	[System.Runtime.InteropServices.FieldOffset(68)]
	public float m_PopupRounding;
	[System.Runtime.InteropServices.FieldOffset(72)]
	public float m_PopupBorderSize;
	[System.Runtime.InteropServices.FieldOffset(76)]
	public EngineNS.Vector2 m_FramePadding;
	[System.Runtime.InteropServices.FieldOffset(84)]
	public float m_FrameRounding;
	[System.Runtime.InteropServices.FieldOffset(88)]
	public float m_FrameBorderSize;
	[System.Runtime.InteropServices.FieldOffset(92)]
	public EngineNS.Vector2 m_ItemSpacing;
	[System.Runtime.InteropServices.FieldOffset(100)]
	public EngineNS.Vector2 m_ItemInnerSpacing;
	[System.Runtime.InteropServices.FieldOffset(108)]
	public EngineNS.Vector2 m_CellPadding;
	[System.Runtime.InteropServices.FieldOffset(116)]
	public EngineNS.Vector2 m_TouchExtraPadding;
	[System.Runtime.InteropServices.FieldOffset(124)]
	public float m_IndentSpacing;
	[System.Runtime.InteropServices.FieldOffset(128)]
	public float m_ColumnsMinSpacing;
	[System.Runtime.InteropServices.FieldOffset(132)]
	public float m_ScrollbarSize;
	[System.Runtime.InteropServices.FieldOffset(136)]
	public float m_ScrollbarRounding;
	[System.Runtime.InteropServices.FieldOffset(140)]
	public float m_ScrollbarPadding;
	[System.Runtime.InteropServices.FieldOffset(144)]
	public float m_GrabMinSize;
	[System.Runtime.InteropServices.FieldOffset(148)]
	public float m_GrabRounding;
	[System.Runtime.InteropServices.FieldOffset(152)]
	public float m_LogSliderDeadzone;
	[System.Runtime.InteropServices.FieldOffset(156)]
	public float m_ImageRounding;
	[System.Runtime.InteropServices.FieldOffset(160)]
	public float m_ImageBorderSize;
	[System.Runtime.InteropServices.FieldOffset(164)]
	public float m_TabRounding;
	[System.Runtime.InteropServices.FieldOffset(168)]
	public float m_TabBorderSize;
	[System.Runtime.InteropServices.FieldOffset(172)]
	public float m_TabMinWidthBase;
	[System.Runtime.InteropServices.FieldOffset(176)]
	public float m_TabMinWidthShrink;
	[System.Runtime.InteropServices.FieldOffset(180)]
	public float m_TabCloseButtonMinWidthSelected;
	[System.Runtime.InteropServices.FieldOffset(184)]
	public float m_TabCloseButtonMinWidthUnselected;
	[System.Runtime.InteropServices.FieldOffset(188)]
	public float m_TabBarBorderSize;
	[System.Runtime.InteropServices.FieldOffset(192)]
	public float m_TabBarOverlineSize;
	[System.Runtime.InteropServices.FieldOffset(196)]
	public float m_TableAngledHeadersAngle;
	[System.Runtime.InteropServices.FieldOffset(200)]
	public EngineNS.Vector2 m_TableAngledHeadersTextAlign;
	[System.Runtime.InteropServices.FieldOffset(208)]
	public int m_TreeLinesFlags;
	[System.Runtime.InteropServices.FieldOffset(212)]
	public float m_TreeLinesSize;
	[System.Runtime.InteropServices.FieldOffset(216)]
	public float m_TreeLinesRounding;
	[System.Runtime.InteropServices.FieldOffset(220)]
	public float m_DragDropTargetRounding;
	[System.Runtime.InteropServices.FieldOffset(224)]
	public float m_DragDropTargetBorderSize;
	[System.Runtime.InteropServices.FieldOffset(228)]
	public float m_DragDropTargetPadding;
	[System.Runtime.InteropServices.FieldOffset(232)]
	public float m_ColorMarkerSize;
	[System.Runtime.InteropServices.FieldOffset(236)]
	public ImGuiDir m_ColorButtonPosition;
	[System.Runtime.InteropServices.FieldOffset(240)]
	public EngineNS.Vector2 m_ButtonTextAlign;
	[System.Runtime.InteropServices.FieldOffset(248)]
	public EngineNS.Vector2 m_SelectableTextAlign;
	[System.Runtime.InteropServices.FieldOffset(256)]
	public float m_SeparatorSize;
	[System.Runtime.InteropServices.FieldOffset(260)]
	public float m_SeparatorTextBorderSize;
	[System.Runtime.InteropServices.FieldOffset(264)]
	public EngineNS.Vector2 m_SeparatorTextAlign;
	[System.Runtime.InteropServices.FieldOffset(272)]
	public EngineNS.Vector2 m_SeparatorTextPadding;
	[System.Runtime.InteropServices.FieldOffset(280)]
	public EngineNS.Vector2 m_DisplayWindowPadding;
	[System.Runtime.InteropServices.FieldOffset(288)]
	public EngineNS.Vector2 m_DisplaySafeAreaPadding;
	[System.Runtime.InteropServices.FieldOffset(296)]
	public bool m_DockingNodeHasCloseButton;
	[System.Runtime.InteropServices.FieldOffset(300)]
	public float m_DockingSeparatorSize;
	[System.Runtime.InteropServices.FieldOffset(304)]
	public float m_MouseCursorScale;
	[System.Runtime.InteropServices.FieldOffset(308)]
	public bool m_AntiAliasedLines;
	[System.Runtime.InteropServices.FieldOffset(309)]
	public bool m_AntiAliasedLinesUseTex;
	[System.Runtime.InteropServices.FieldOffset(310)]
	public bool m_AntiAliasedFill;
	[System.Runtime.InteropServices.FieldOffset(312)]
	public float m_CurveTessellationTol;
	[System.Runtime.InteropServices.FieldOffset(316)]
	public float m_CircleTessellationMaxError;
	[System.Runtime.InteropServices.FieldOffset(320)]
	public EngineNS.Vector4* m_Colors;
	[System.Runtime.InteropServices.FieldOffset(1312)]
	public float m_HoverStationaryDelay;
	[System.Runtime.InteropServices.FieldOffset(1316)]
	public float m_HoverDelayShort;
	[System.Runtime.InteropServices.FieldOffset(1320)]
	public float m_HoverDelayNormal;
	[System.Runtime.InteropServices.FieldOffset(1324)]
	public int m_HoverFlagsForTooltipMouse;
	[System.Runtime.InteropServices.FieldOffset(1328)]
	public int m_HoverFlagsForTooltipNav;
	[System.Runtime.InteropServices.FieldOffset(1332)]
	public float m__MainScale;
	[System.Runtime.InteropServices.FieldOffset(1336)]
	public float m__NextFrameFontSizeBase;
	#endregion
	public IntPtr NativePointer { get => IntPtr.Zero; set {} }
	#region Constructor&Cast
	public void UnsafeCallConstructor()
	{
		fixed (ImGuiStyle* mPointer = &this)
		{
			TitanImGui_ImGuiStyle_Visitor_UnsafeCallConstructor_2960189489(mPointer);
		}
	}
	public void UnsafeCallDestructor()
	{
		fixed (ImGuiStyle* mPointer = &this)
		{
			TitanImGui_ImGuiStyle_Visitor_UnsafeCallDestructor(mPointer);
		}
	}
	#endregion
	#region Fields
	public float FontSizeBase
	{
		get
		{
			fixed (ImGuiStyle* mPointer = &this)
			{
				return TitanImGui_ImGuiStyle_Visitor_FieldGet__FontSizeBase(mPointer);
			}
		}
		set
		{
			fixed (ImGuiStyle* mPointer = &this)
			{
				TitanImGui_ImGuiStyle_Visitor_FieldSet__FontSizeBase(mPointer, value);
			}
		}
	}
	public float FontScaleMain
	{
		get
		{
			fixed (ImGuiStyle* mPointer = &this)
			{
				return TitanImGui_ImGuiStyle_Visitor_FieldGet__FontScaleMain(mPointer);
			}
		}
		set
		{
			fixed (ImGuiStyle* mPointer = &this)
			{
				TitanImGui_ImGuiStyle_Visitor_FieldSet__FontScaleMain(mPointer, value);
			}
		}
	}
	public float FontScaleDpi
	{
		get
		{
			fixed (ImGuiStyle* mPointer = &this)
			{
				return TitanImGui_ImGuiStyle_Visitor_FieldGet__FontScaleDpi(mPointer);
			}
		}
		set
		{
			fixed (ImGuiStyle* mPointer = &this)
			{
				TitanImGui_ImGuiStyle_Visitor_FieldSet__FontScaleDpi(mPointer, value);
			}
		}
	}
	public float Alpha
	{
		get
		{
			fixed (ImGuiStyle* mPointer = &this)
			{
				return TitanImGui_ImGuiStyle_Visitor_FieldGet__Alpha(mPointer);
			}
		}
		set
		{
			fixed (ImGuiStyle* mPointer = &this)
			{
				TitanImGui_ImGuiStyle_Visitor_FieldSet__Alpha(mPointer, value);
			}
		}
	}
	public float DisabledAlpha
	{
		get
		{
			fixed (ImGuiStyle* mPointer = &this)
			{
				return TitanImGui_ImGuiStyle_Visitor_FieldGet__DisabledAlpha(mPointer);
			}
		}
		set
		{
			fixed (ImGuiStyle* mPointer = &this)
			{
				TitanImGui_ImGuiStyle_Visitor_FieldSet__DisabledAlpha(mPointer, value);
			}
		}
	}
	public EngineNS.Vector2 WindowPadding
	{
		get
		{
			fixed (ImGuiStyle* mPointer = &this)
			{
				return TitanImGui_ImGuiStyle_Visitor_FieldGet__WindowPadding(mPointer);
			}
		}
		set
		{
			fixed (ImGuiStyle* mPointer = &this)
			{
				TitanImGui_ImGuiStyle_Visitor_FieldSet__WindowPadding(mPointer, value);
			}
		}
	}
	public float WindowRounding
	{
		get
		{
			fixed (ImGuiStyle* mPointer = &this)
			{
				return TitanImGui_ImGuiStyle_Visitor_FieldGet__WindowRounding(mPointer);
			}
		}
		set
		{
			fixed (ImGuiStyle* mPointer = &this)
			{
				TitanImGui_ImGuiStyle_Visitor_FieldSet__WindowRounding(mPointer, value);
			}
		}
	}
	public float WindowBorderSize
	{
		get
		{
			fixed (ImGuiStyle* mPointer = &this)
			{
				return TitanImGui_ImGuiStyle_Visitor_FieldGet__WindowBorderSize(mPointer);
			}
		}
		set
		{
			fixed (ImGuiStyle* mPointer = &this)
			{
				TitanImGui_ImGuiStyle_Visitor_FieldSet__WindowBorderSize(mPointer, value);
			}
		}
	}
	public float WindowBorderHoverPadding
	{
		get
		{
			fixed (ImGuiStyle* mPointer = &this)
			{
				return TitanImGui_ImGuiStyle_Visitor_FieldGet__WindowBorderHoverPadding(mPointer);
			}
		}
		set
		{
			fixed (ImGuiStyle* mPointer = &this)
			{
				TitanImGui_ImGuiStyle_Visitor_FieldSet__WindowBorderHoverPadding(mPointer, value);
			}
		}
	}
	public EngineNS.Vector2 WindowMinSize
	{
		get
		{
			fixed (ImGuiStyle* mPointer = &this)
			{
				return TitanImGui_ImGuiStyle_Visitor_FieldGet__WindowMinSize(mPointer);
			}
		}
		set
		{
			fixed (ImGuiStyle* mPointer = &this)
			{
				TitanImGui_ImGuiStyle_Visitor_FieldSet__WindowMinSize(mPointer, value);
			}
		}
	}
	public EngineNS.Vector2 WindowTitleAlign
	{
		get
		{
			fixed (ImGuiStyle* mPointer = &this)
			{
				return TitanImGui_ImGuiStyle_Visitor_FieldGet__WindowTitleAlign(mPointer);
			}
		}
		set
		{
			fixed (ImGuiStyle* mPointer = &this)
			{
				TitanImGui_ImGuiStyle_Visitor_FieldSet__WindowTitleAlign(mPointer, value);
			}
		}
	}
	public ImGuiDir WindowMenuButtonPosition
	{
		get
		{
			fixed (ImGuiStyle* mPointer = &this)
			{
				return TitanImGui_ImGuiStyle_Visitor_FieldGet__WindowMenuButtonPosition(mPointer);
			}
		}
		set
		{
			fixed (ImGuiStyle* mPointer = &this)
			{
				TitanImGui_ImGuiStyle_Visitor_FieldSet__WindowMenuButtonPosition(mPointer, value);
			}
		}
	}
	public float ChildRounding
	{
		get
		{
			fixed (ImGuiStyle* mPointer = &this)
			{
				return TitanImGui_ImGuiStyle_Visitor_FieldGet__ChildRounding(mPointer);
			}
		}
		set
		{
			fixed (ImGuiStyle* mPointer = &this)
			{
				TitanImGui_ImGuiStyle_Visitor_FieldSet__ChildRounding(mPointer, value);
			}
		}
	}
	public float ChildBorderSize
	{
		get
		{
			fixed (ImGuiStyle* mPointer = &this)
			{
				return TitanImGui_ImGuiStyle_Visitor_FieldGet__ChildBorderSize(mPointer);
			}
		}
		set
		{
			fixed (ImGuiStyle* mPointer = &this)
			{
				TitanImGui_ImGuiStyle_Visitor_FieldSet__ChildBorderSize(mPointer, value);
			}
		}
	}
	public float PopupRounding
	{
		get
		{
			fixed (ImGuiStyle* mPointer = &this)
			{
				return TitanImGui_ImGuiStyle_Visitor_FieldGet__PopupRounding(mPointer);
			}
		}
		set
		{
			fixed (ImGuiStyle* mPointer = &this)
			{
				TitanImGui_ImGuiStyle_Visitor_FieldSet__PopupRounding(mPointer, value);
			}
		}
	}
	public float PopupBorderSize
	{
		get
		{
			fixed (ImGuiStyle* mPointer = &this)
			{
				return TitanImGui_ImGuiStyle_Visitor_FieldGet__PopupBorderSize(mPointer);
			}
		}
		set
		{
			fixed (ImGuiStyle* mPointer = &this)
			{
				TitanImGui_ImGuiStyle_Visitor_FieldSet__PopupBorderSize(mPointer, value);
			}
		}
	}
	public EngineNS.Vector2 FramePadding
	{
		get
		{
			fixed (ImGuiStyle* mPointer = &this)
			{
				return TitanImGui_ImGuiStyle_Visitor_FieldGet__FramePadding(mPointer);
			}
		}
		set
		{
			fixed (ImGuiStyle* mPointer = &this)
			{
				TitanImGui_ImGuiStyle_Visitor_FieldSet__FramePadding(mPointer, value);
			}
		}
	}
	public float FrameRounding
	{
		get
		{
			fixed (ImGuiStyle* mPointer = &this)
			{
				return TitanImGui_ImGuiStyle_Visitor_FieldGet__FrameRounding(mPointer);
			}
		}
		set
		{
			fixed (ImGuiStyle* mPointer = &this)
			{
				TitanImGui_ImGuiStyle_Visitor_FieldSet__FrameRounding(mPointer, value);
			}
		}
	}
	public float FrameBorderSize
	{
		get
		{
			fixed (ImGuiStyle* mPointer = &this)
			{
				return TitanImGui_ImGuiStyle_Visitor_FieldGet__FrameBorderSize(mPointer);
			}
		}
		set
		{
			fixed (ImGuiStyle* mPointer = &this)
			{
				TitanImGui_ImGuiStyle_Visitor_FieldSet__FrameBorderSize(mPointer, value);
			}
		}
	}
	public EngineNS.Vector2 ItemSpacing
	{
		get
		{
			fixed (ImGuiStyle* mPointer = &this)
			{
				return TitanImGui_ImGuiStyle_Visitor_FieldGet__ItemSpacing(mPointer);
			}
		}
		set
		{
			fixed (ImGuiStyle* mPointer = &this)
			{
				TitanImGui_ImGuiStyle_Visitor_FieldSet__ItemSpacing(mPointer, value);
			}
		}
	}
	public EngineNS.Vector2 ItemInnerSpacing
	{
		get
		{
			fixed (ImGuiStyle* mPointer = &this)
			{
				return TitanImGui_ImGuiStyle_Visitor_FieldGet__ItemInnerSpacing(mPointer);
			}
		}
		set
		{
			fixed (ImGuiStyle* mPointer = &this)
			{
				TitanImGui_ImGuiStyle_Visitor_FieldSet__ItemInnerSpacing(mPointer, value);
			}
		}
	}
	public EngineNS.Vector2 CellPadding
	{
		get
		{
			fixed (ImGuiStyle* mPointer = &this)
			{
				return TitanImGui_ImGuiStyle_Visitor_FieldGet__CellPadding(mPointer);
			}
		}
		set
		{
			fixed (ImGuiStyle* mPointer = &this)
			{
				TitanImGui_ImGuiStyle_Visitor_FieldSet__CellPadding(mPointer, value);
			}
		}
	}
	public EngineNS.Vector2 TouchExtraPadding
	{
		get
		{
			fixed (ImGuiStyle* mPointer = &this)
			{
				return TitanImGui_ImGuiStyle_Visitor_FieldGet__TouchExtraPadding(mPointer);
			}
		}
		set
		{
			fixed (ImGuiStyle* mPointer = &this)
			{
				TitanImGui_ImGuiStyle_Visitor_FieldSet__TouchExtraPadding(mPointer, value);
			}
		}
	}
	public float IndentSpacing
	{
		get
		{
			fixed (ImGuiStyle* mPointer = &this)
			{
				return TitanImGui_ImGuiStyle_Visitor_FieldGet__IndentSpacing(mPointer);
			}
		}
		set
		{
			fixed (ImGuiStyle* mPointer = &this)
			{
				TitanImGui_ImGuiStyle_Visitor_FieldSet__IndentSpacing(mPointer, value);
			}
		}
	}
	public float ColumnsMinSpacing
	{
		get
		{
			fixed (ImGuiStyle* mPointer = &this)
			{
				return TitanImGui_ImGuiStyle_Visitor_FieldGet__ColumnsMinSpacing(mPointer);
			}
		}
		set
		{
			fixed (ImGuiStyle* mPointer = &this)
			{
				TitanImGui_ImGuiStyle_Visitor_FieldSet__ColumnsMinSpacing(mPointer, value);
			}
		}
	}
	public float ScrollbarSize
	{
		get
		{
			fixed (ImGuiStyle* mPointer = &this)
			{
				return TitanImGui_ImGuiStyle_Visitor_FieldGet__ScrollbarSize(mPointer);
			}
		}
		set
		{
			fixed (ImGuiStyle* mPointer = &this)
			{
				TitanImGui_ImGuiStyle_Visitor_FieldSet__ScrollbarSize(mPointer, value);
			}
		}
	}
	public float ScrollbarRounding
	{
		get
		{
			fixed (ImGuiStyle* mPointer = &this)
			{
				return TitanImGui_ImGuiStyle_Visitor_FieldGet__ScrollbarRounding(mPointer);
			}
		}
		set
		{
			fixed (ImGuiStyle* mPointer = &this)
			{
				TitanImGui_ImGuiStyle_Visitor_FieldSet__ScrollbarRounding(mPointer, value);
			}
		}
	}
	public float ScrollbarPadding
	{
		get
		{
			fixed (ImGuiStyle* mPointer = &this)
			{
				return TitanImGui_ImGuiStyle_Visitor_FieldGet__ScrollbarPadding(mPointer);
			}
		}
		set
		{
			fixed (ImGuiStyle* mPointer = &this)
			{
				TitanImGui_ImGuiStyle_Visitor_FieldSet__ScrollbarPadding(mPointer, value);
			}
		}
	}
	public float GrabMinSize
	{
		get
		{
			fixed (ImGuiStyle* mPointer = &this)
			{
				return TitanImGui_ImGuiStyle_Visitor_FieldGet__GrabMinSize(mPointer);
			}
		}
		set
		{
			fixed (ImGuiStyle* mPointer = &this)
			{
				TitanImGui_ImGuiStyle_Visitor_FieldSet__GrabMinSize(mPointer, value);
			}
		}
	}
	public float GrabRounding
	{
		get
		{
			fixed (ImGuiStyle* mPointer = &this)
			{
				return TitanImGui_ImGuiStyle_Visitor_FieldGet__GrabRounding(mPointer);
			}
		}
		set
		{
			fixed (ImGuiStyle* mPointer = &this)
			{
				TitanImGui_ImGuiStyle_Visitor_FieldSet__GrabRounding(mPointer, value);
			}
		}
	}
	public float LogSliderDeadzone
	{
		get
		{
			fixed (ImGuiStyle* mPointer = &this)
			{
				return TitanImGui_ImGuiStyle_Visitor_FieldGet__LogSliderDeadzone(mPointer);
			}
		}
		set
		{
			fixed (ImGuiStyle* mPointer = &this)
			{
				TitanImGui_ImGuiStyle_Visitor_FieldSet__LogSliderDeadzone(mPointer, value);
			}
		}
	}
	public float ImageRounding
	{
		get
		{
			fixed (ImGuiStyle* mPointer = &this)
			{
				return TitanImGui_ImGuiStyle_Visitor_FieldGet__ImageRounding(mPointer);
			}
		}
		set
		{
			fixed (ImGuiStyle* mPointer = &this)
			{
				TitanImGui_ImGuiStyle_Visitor_FieldSet__ImageRounding(mPointer, value);
			}
		}
	}
	public float ImageBorderSize
	{
		get
		{
			fixed (ImGuiStyle* mPointer = &this)
			{
				return TitanImGui_ImGuiStyle_Visitor_FieldGet__ImageBorderSize(mPointer);
			}
		}
		set
		{
			fixed (ImGuiStyle* mPointer = &this)
			{
				TitanImGui_ImGuiStyle_Visitor_FieldSet__ImageBorderSize(mPointer, value);
			}
		}
	}
	public float TabRounding
	{
		get
		{
			fixed (ImGuiStyle* mPointer = &this)
			{
				return TitanImGui_ImGuiStyle_Visitor_FieldGet__TabRounding(mPointer);
			}
		}
		set
		{
			fixed (ImGuiStyle* mPointer = &this)
			{
				TitanImGui_ImGuiStyle_Visitor_FieldSet__TabRounding(mPointer, value);
			}
		}
	}
	public float TabBorderSize
	{
		get
		{
			fixed (ImGuiStyle* mPointer = &this)
			{
				return TitanImGui_ImGuiStyle_Visitor_FieldGet__TabBorderSize(mPointer);
			}
		}
		set
		{
			fixed (ImGuiStyle* mPointer = &this)
			{
				TitanImGui_ImGuiStyle_Visitor_FieldSet__TabBorderSize(mPointer, value);
			}
		}
	}
	public float TabMinWidthBase
	{
		get
		{
			fixed (ImGuiStyle* mPointer = &this)
			{
				return TitanImGui_ImGuiStyle_Visitor_FieldGet__TabMinWidthBase(mPointer);
			}
		}
		set
		{
			fixed (ImGuiStyle* mPointer = &this)
			{
				TitanImGui_ImGuiStyle_Visitor_FieldSet__TabMinWidthBase(mPointer, value);
			}
		}
	}
	public float TabMinWidthShrink
	{
		get
		{
			fixed (ImGuiStyle* mPointer = &this)
			{
				return TitanImGui_ImGuiStyle_Visitor_FieldGet__TabMinWidthShrink(mPointer);
			}
		}
		set
		{
			fixed (ImGuiStyle* mPointer = &this)
			{
				TitanImGui_ImGuiStyle_Visitor_FieldSet__TabMinWidthShrink(mPointer, value);
			}
		}
	}
	public float TabCloseButtonMinWidthSelected
	{
		get
		{
			fixed (ImGuiStyle* mPointer = &this)
			{
				return TitanImGui_ImGuiStyle_Visitor_FieldGet__TabCloseButtonMinWidthSelected(mPointer);
			}
		}
		set
		{
			fixed (ImGuiStyle* mPointer = &this)
			{
				TitanImGui_ImGuiStyle_Visitor_FieldSet__TabCloseButtonMinWidthSelected(mPointer, value);
			}
		}
	}
	public float TabCloseButtonMinWidthUnselected
	{
		get
		{
			fixed (ImGuiStyle* mPointer = &this)
			{
				return TitanImGui_ImGuiStyle_Visitor_FieldGet__TabCloseButtonMinWidthUnselected(mPointer);
			}
		}
		set
		{
			fixed (ImGuiStyle* mPointer = &this)
			{
				TitanImGui_ImGuiStyle_Visitor_FieldSet__TabCloseButtonMinWidthUnselected(mPointer, value);
			}
		}
	}
	public float TabBarBorderSize
	{
		get
		{
			fixed (ImGuiStyle* mPointer = &this)
			{
				return TitanImGui_ImGuiStyle_Visitor_FieldGet__TabBarBorderSize(mPointer);
			}
		}
		set
		{
			fixed (ImGuiStyle* mPointer = &this)
			{
				TitanImGui_ImGuiStyle_Visitor_FieldSet__TabBarBorderSize(mPointer, value);
			}
		}
	}
	public float TabBarOverlineSize
	{
		get
		{
			fixed (ImGuiStyle* mPointer = &this)
			{
				return TitanImGui_ImGuiStyle_Visitor_FieldGet__TabBarOverlineSize(mPointer);
			}
		}
		set
		{
			fixed (ImGuiStyle* mPointer = &this)
			{
				TitanImGui_ImGuiStyle_Visitor_FieldSet__TabBarOverlineSize(mPointer, value);
			}
		}
	}
	public float TableAngledHeadersAngle
	{
		get
		{
			fixed (ImGuiStyle* mPointer = &this)
			{
				return TitanImGui_ImGuiStyle_Visitor_FieldGet__TableAngledHeadersAngle(mPointer);
			}
		}
		set
		{
			fixed (ImGuiStyle* mPointer = &this)
			{
				TitanImGui_ImGuiStyle_Visitor_FieldSet__TableAngledHeadersAngle(mPointer, value);
			}
		}
	}
	public EngineNS.Vector2 TableAngledHeadersTextAlign
	{
		get
		{
			fixed (ImGuiStyle* mPointer = &this)
			{
				return TitanImGui_ImGuiStyle_Visitor_FieldGet__TableAngledHeadersTextAlign(mPointer);
			}
		}
		set
		{
			fixed (ImGuiStyle* mPointer = &this)
			{
				TitanImGui_ImGuiStyle_Visitor_FieldSet__TableAngledHeadersTextAlign(mPointer, value);
			}
		}
	}
	public int TreeLinesFlags
	{
		get
		{
			fixed (ImGuiStyle* mPointer = &this)
			{
				return TitanImGui_ImGuiStyle_Visitor_FieldGet__TreeLinesFlags(mPointer);
			}
		}
		set
		{
			fixed (ImGuiStyle* mPointer = &this)
			{
				TitanImGui_ImGuiStyle_Visitor_FieldSet__TreeLinesFlags(mPointer, value);
			}
		}
	}
	public float TreeLinesSize
	{
		get
		{
			fixed (ImGuiStyle* mPointer = &this)
			{
				return TitanImGui_ImGuiStyle_Visitor_FieldGet__TreeLinesSize(mPointer);
			}
		}
		set
		{
			fixed (ImGuiStyle* mPointer = &this)
			{
				TitanImGui_ImGuiStyle_Visitor_FieldSet__TreeLinesSize(mPointer, value);
			}
		}
	}
	public float TreeLinesRounding
	{
		get
		{
			fixed (ImGuiStyle* mPointer = &this)
			{
				return TitanImGui_ImGuiStyle_Visitor_FieldGet__TreeLinesRounding(mPointer);
			}
		}
		set
		{
			fixed (ImGuiStyle* mPointer = &this)
			{
				TitanImGui_ImGuiStyle_Visitor_FieldSet__TreeLinesRounding(mPointer, value);
			}
		}
	}
	public float DragDropTargetRounding
	{
		get
		{
			fixed (ImGuiStyle* mPointer = &this)
			{
				return TitanImGui_ImGuiStyle_Visitor_FieldGet__DragDropTargetRounding(mPointer);
			}
		}
		set
		{
			fixed (ImGuiStyle* mPointer = &this)
			{
				TitanImGui_ImGuiStyle_Visitor_FieldSet__DragDropTargetRounding(mPointer, value);
			}
		}
	}
	public float DragDropTargetBorderSize
	{
		get
		{
			fixed (ImGuiStyle* mPointer = &this)
			{
				return TitanImGui_ImGuiStyle_Visitor_FieldGet__DragDropTargetBorderSize(mPointer);
			}
		}
		set
		{
			fixed (ImGuiStyle* mPointer = &this)
			{
				TitanImGui_ImGuiStyle_Visitor_FieldSet__DragDropTargetBorderSize(mPointer, value);
			}
		}
	}
	public float DragDropTargetPadding
	{
		get
		{
			fixed (ImGuiStyle* mPointer = &this)
			{
				return TitanImGui_ImGuiStyle_Visitor_FieldGet__DragDropTargetPadding(mPointer);
			}
		}
		set
		{
			fixed (ImGuiStyle* mPointer = &this)
			{
				TitanImGui_ImGuiStyle_Visitor_FieldSet__DragDropTargetPadding(mPointer, value);
			}
		}
	}
	public float ColorMarkerSize
	{
		get
		{
			fixed (ImGuiStyle* mPointer = &this)
			{
				return TitanImGui_ImGuiStyle_Visitor_FieldGet__ColorMarkerSize(mPointer);
			}
		}
		set
		{
			fixed (ImGuiStyle* mPointer = &this)
			{
				TitanImGui_ImGuiStyle_Visitor_FieldSet__ColorMarkerSize(mPointer, value);
			}
		}
	}
	public ImGuiDir ColorButtonPosition
	{
		get
		{
			fixed (ImGuiStyle* mPointer = &this)
			{
				return TitanImGui_ImGuiStyle_Visitor_FieldGet__ColorButtonPosition(mPointer);
			}
		}
		set
		{
			fixed (ImGuiStyle* mPointer = &this)
			{
				TitanImGui_ImGuiStyle_Visitor_FieldSet__ColorButtonPosition(mPointer, value);
			}
		}
	}
	public EngineNS.Vector2 ButtonTextAlign
	{
		get
		{
			fixed (ImGuiStyle* mPointer = &this)
			{
				return TitanImGui_ImGuiStyle_Visitor_FieldGet__ButtonTextAlign(mPointer);
			}
		}
		set
		{
			fixed (ImGuiStyle* mPointer = &this)
			{
				TitanImGui_ImGuiStyle_Visitor_FieldSet__ButtonTextAlign(mPointer, value);
			}
		}
	}
	public EngineNS.Vector2 SelectableTextAlign
	{
		get
		{
			fixed (ImGuiStyle* mPointer = &this)
			{
				return TitanImGui_ImGuiStyle_Visitor_FieldGet__SelectableTextAlign(mPointer);
			}
		}
		set
		{
			fixed (ImGuiStyle* mPointer = &this)
			{
				TitanImGui_ImGuiStyle_Visitor_FieldSet__SelectableTextAlign(mPointer, value);
			}
		}
	}
	public float SeparatorSize
	{
		get
		{
			fixed (ImGuiStyle* mPointer = &this)
			{
				return TitanImGui_ImGuiStyle_Visitor_FieldGet__SeparatorSize(mPointer);
			}
		}
		set
		{
			fixed (ImGuiStyle* mPointer = &this)
			{
				TitanImGui_ImGuiStyle_Visitor_FieldSet__SeparatorSize(mPointer, value);
			}
		}
	}
	public float SeparatorTextBorderSize
	{
		get
		{
			fixed (ImGuiStyle* mPointer = &this)
			{
				return TitanImGui_ImGuiStyle_Visitor_FieldGet__SeparatorTextBorderSize(mPointer);
			}
		}
		set
		{
			fixed (ImGuiStyle* mPointer = &this)
			{
				TitanImGui_ImGuiStyle_Visitor_FieldSet__SeparatorTextBorderSize(mPointer, value);
			}
		}
	}
	public EngineNS.Vector2 SeparatorTextAlign
	{
		get
		{
			fixed (ImGuiStyle* mPointer = &this)
			{
				return TitanImGui_ImGuiStyle_Visitor_FieldGet__SeparatorTextAlign(mPointer);
			}
		}
		set
		{
			fixed (ImGuiStyle* mPointer = &this)
			{
				TitanImGui_ImGuiStyle_Visitor_FieldSet__SeparatorTextAlign(mPointer, value);
			}
		}
	}
	public EngineNS.Vector2 SeparatorTextPadding
	{
		get
		{
			fixed (ImGuiStyle* mPointer = &this)
			{
				return TitanImGui_ImGuiStyle_Visitor_FieldGet__SeparatorTextPadding(mPointer);
			}
		}
		set
		{
			fixed (ImGuiStyle* mPointer = &this)
			{
				TitanImGui_ImGuiStyle_Visitor_FieldSet__SeparatorTextPadding(mPointer, value);
			}
		}
	}
	public EngineNS.Vector2 DisplayWindowPadding
	{
		get
		{
			fixed (ImGuiStyle* mPointer = &this)
			{
				return TitanImGui_ImGuiStyle_Visitor_FieldGet__DisplayWindowPadding(mPointer);
			}
		}
		set
		{
			fixed (ImGuiStyle* mPointer = &this)
			{
				TitanImGui_ImGuiStyle_Visitor_FieldSet__DisplayWindowPadding(mPointer, value);
			}
		}
	}
	public EngineNS.Vector2 DisplaySafeAreaPadding
	{
		get
		{
			fixed (ImGuiStyle* mPointer = &this)
			{
				return TitanImGui_ImGuiStyle_Visitor_FieldGet__DisplaySafeAreaPadding(mPointer);
			}
		}
		set
		{
			fixed (ImGuiStyle* mPointer = &this)
			{
				TitanImGui_ImGuiStyle_Visitor_FieldSet__DisplaySafeAreaPadding(mPointer, value);
			}
		}
	}
	public bool DockingNodeHasCloseButton
	{
		get
		{
			fixed (ImGuiStyle* mPointer = &this)
			{
				return TitanImGui_ImGuiStyle_Visitor_FieldGet__DockingNodeHasCloseButton(mPointer) == 0 ? false : true;
			}
		}
		set
		{
			fixed (ImGuiStyle* mPointer = &this)
			{
				TitanImGui_ImGuiStyle_Visitor_FieldSet__DockingNodeHasCloseButton(mPointer, value);
			}
		}
	}
	public float DockingSeparatorSize
	{
		get
		{
			fixed (ImGuiStyle* mPointer = &this)
			{
				return TitanImGui_ImGuiStyle_Visitor_FieldGet__DockingSeparatorSize(mPointer);
			}
		}
		set
		{
			fixed (ImGuiStyle* mPointer = &this)
			{
				TitanImGui_ImGuiStyle_Visitor_FieldSet__DockingSeparatorSize(mPointer, value);
			}
		}
	}
	public float MouseCursorScale
	{
		get
		{
			fixed (ImGuiStyle* mPointer = &this)
			{
				return TitanImGui_ImGuiStyle_Visitor_FieldGet__MouseCursorScale(mPointer);
			}
		}
		set
		{
			fixed (ImGuiStyle* mPointer = &this)
			{
				TitanImGui_ImGuiStyle_Visitor_FieldSet__MouseCursorScale(mPointer, value);
			}
		}
	}
	public bool AntiAliasedLines
	{
		get
		{
			fixed (ImGuiStyle* mPointer = &this)
			{
				return TitanImGui_ImGuiStyle_Visitor_FieldGet__AntiAliasedLines(mPointer) == 0 ? false : true;
			}
		}
		set
		{
			fixed (ImGuiStyle* mPointer = &this)
			{
				TitanImGui_ImGuiStyle_Visitor_FieldSet__AntiAliasedLines(mPointer, value);
			}
		}
	}
	public bool AntiAliasedLinesUseTex
	{
		get
		{
			fixed (ImGuiStyle* mPointer = &this)
			{
				return TitanImGui_ImGuiStyle_Visitor_FieldGet__AntiAliasedLinesUseTex(mPointer) == 0 ? false : true;
			}
		}
		set
		{
			fixed (ImGuiStyle* mPointer = &this)
			{
				TitanImGui_ImGuiStyle_Visitor_FieldSet__AntiAliasedLinesUseTex(mPointer, value);
			}
		}
	}
	public bool AntiAliasedFill
	{
		get
		{
			fixed (ImGuiStyle* mPointer = &this)
			{
				return TitanImGui_ImGuiStyle_Visitor_FieldGet__AntiAliasedFill(mPointer) == 0 ? false : true;
			}
		}
		set
		{
			fixed (ImGuiStyle* mPointer = &this)
			{
				TitanImGui_ImGuiStyle_Visitor_FieldSet__AntiAliasedFill(mPointer, value);
			}
		}
	}
	public float CurveTessellationTol
	{
		get
		{
			fixed (ImGuiStyle* mPointer = &this)
			{
				return TitanImGui_ImGuiStyle_Visitor_FieldGet__CurveTessellationTol(mPointer);
			}
		}
		set
		{
			fixed (ImGuiStyle* mPointer = &this)
			{
				TitanImGui_ImGuiStyle_Visitor_FieldSet__CurveTessellationTol(mPointer, value);
			}
		}
	}
	public float CircleTessellationMaxError
	{
		get
		{
			fixed (ImGuiStyle* mPointer = &this)
			{
				return TitanImGui_ImGuiStyle_Visitor_FieldGet__CircleTessellationMaxError(mPointer);
			}
		}
		set
		{
			fixed (ImGuiStyle* mPointer = &this)
			{
				TitanImGui_ImGuiStyle_Visitor_FieldSet__CircleTessellationMaxError(mPointer, value);
			}
		}
	}
	public EngineNS.Vector4* Colors
	{
		get
		{
			fixed (ImGuiStyle* mPointer = &this)
			{
				return TitanImGui_ImGuiStyle_Visitor_FieldGet__Colors(mPointer);
			}
		}
		set
		{
			fixed (ImGuiStyle* mPointer = &this)
			{
				TitanImGui_ImGuiStyle_Visitor_FieldSet__Colors(mPointer, value);
			}
		}
	}
	public float HoverStationaryDelay
	{
		get
		{
			fixed (ImGuiStyle* mPointer = &this)
			{
				return TitanImGui_ImGuiStyle_Visitor_FieldGet__HoverStationaryDelay(mPointer);
			}
		}
		set
		{
			fixed (ImGuiStyle* mPointer = &this)
			{
				TitanImGui_ImGuiStyle_Visitor_FieldSet__HoverStationaryDelay(mPointer, value);
			}
		}
	}
	public float HoverDelayShort
	{
		get
		{
			fixed (ImGuiStyle* mPointer = &this)
			{
				return TitanImGui_ImGuiStyle_Visitor_FieldGet__HoverDelayShort(mPointer);
			}
		}
		set
		{
			fixed (ImGuiStyle* mPointer = &this)
			{
				TitanImGui_ImGuiStyle_Visitor_FieldSet__HoverDelayShort(mPointer, value);
			}
		}
	}
	public float HoverDelayNormal
	{
		get
		{
			fixed (ImGuiStyle* mPointer = &this)
			{
				return TitanImGui_ImGuiStyle_Visitor_FieldGet__HoverDelayNormal(mPointer);
			}
		}
		set
		{
			fixed (ImGuiStyle* mPointer = &this)
			{
				TitanImGui_ImGuiStyle_Visitor_FieldSet__HoverDelayNormal(mPointer, value);
			}
		}
	}
	public int HoverFlagsForTooltipMouse
	{
		get
		{
			fixed (ImGuiStyle* mPointer = &this)
			{
				return TitanImGui_ImGuiStyle_Visitor_FieldGet__HoverFlagsForTooltipMouse(mPointer);
			}
		}
		set
		{
			fixed (ImGuiStyle* mPointer = &this)
			{
				TitanImGui_ImGuiStyle_Visitor_FieldSet__HoverFlagsForTooltipMouse(mPointer, value);
			}
		}
	}
	public int HoverFlagsForTooltipNav
	{
		get
		{
			fixed (ImGuiStyle* mPointer = &this)
			{
				return TitanImGui_ImGuiStyle_Visitor_FieldGet__HoverFlagsForTooltipNav(mPointer);
			}
		}
		set
		{
			fixed (ImGuiStyle* mPointer = &this)
			{
				TitanImGui_ImGuiStyle_Visitor_FieldSet__HoverFlagsForTooltipNav(mPointer, value);
			}
		}
	}
	public float _MainScale
	{
		get
		{
			fixed (ImGuiStyle* mPointer = &this)
			{
				return TitanImGui_ImGuiStyle_Visitor_FieldGet___MainScale(mPointer);
			}
		}
		set
		{
			fixed (ImGuiStyle* mPointer = &this)
			{
				TitanImGui_ImGuiStyle_Visitor_FieldSet___MainScale(mPointer, value);
			}
		}
	}
	public float _NextFrameFontSizeBase
	{
		get
		{
			fixed (ImGuiStyle* mPointer = &this)
			{
				return TitanImGui_ImGuiStyle_Visitor_FieldGet___NextFrameFontSizeBase(mPointer);
			}
		}
		set
		{
			fixed (ImGuiStyle* mPointer = &this)
			{
				TitanImGui_ImGuiStyle_Visitor_FieldSet___NextFrameFontSizeBase(mPointer, value);
			}
		}
	}
	#endregion
	#region Function
	public void ScaleAllSizes(float scale_factor)
	{
		fixed (ImGuiStyle* mPointer = &this)
		{
			TitanImGui_ImGuiStyle_Visitor_ScaleAllSizes_1759962673(mPointer, scale_factor);
		}
	}
	#endregion
	#region Core SDK
	const string ModuleNC = EngineNS.CoreSDK.CoreModule;
	//Constructor&Cast
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	extern static void TitanImGui_ImGuiStyle_Visitor_UnsafeCallConstructor_2960189489(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	extern static void TitanImGui_ImGuiStyle_Visitor_UnsafeCallDestructor(void* self);
	//Fields
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static float TitanImGui_ImGuiStyle_Visitor_FieldGet__FontSizeBase(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImGuiStyle_Visitor_FieldSet__FontSizeBase(void* self, float value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static float TitanImGui_ImGuiStyle_Visitor_FieldGet__FontScaleMain(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImGuiStyle_Visitor_FieldSet__FontScaleMain(void* self, float value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static float TitanImGui_ImGuiStyle_Visitor_FieldGet__FontScaleDpi(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImGuiStyle_Visitor_FieldSet__FontScaleDpi(void* self, float value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static float TitanImGui_ImGuiStyle_Visitor_FieldGet__Alpha(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImGuiStyle_Visitor_FieldSet__Alpha(void* self, float value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static float TitanImGui_ImGuiStyle_Visitor_FieldGet__DisabledAlpha(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImGuiStyle_Visitor_FieldSet__DisabledAlpha(void* self, float value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static EngineNS.Vector2 TitanImGui_ImGuiStyle_Visitor_FieldGet__WindowPadding(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImGuiStyle_Visitor_FieldSet__WindowPadding(void* self, EngineNS.Vector2 value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static float TitanImGui_ImGuiStyle_Visitor_FieldGet__WindowRounding(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImGuiStyle_Visitor_FieldSet__WindowRounding(void* self, float value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static float TitanImGui_ImGuiStyle_Visitor_FieldGet__WindowBorderSize(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImGuiStyle_Visitor_FieldSet__WindowBorderSize(void* self, float value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static float TitanImGui_ImGuiStyle_Visitor_FieldGet__WindowBorderHoverPadding(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImGuiStyle_Visitor_FieldSet__WindowBorderHoverPadding(void* self, float value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static EngineNS.Vector2 TitanImGui_ImGuiStyle_Visitor_FieldGet__WindowMinSize(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImGuiStyle_Visitor_FieldSet__WindowMinSize(void* self, EngineNS.Vector2 value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static EngineNS.Vector2 TitanImGui_ImGuiStyle_Visitor_FieldGet__WindowTitleAlign(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImGuiStyle_Visitor_FieldSet__WindowTitleAlign(void* self, EngineNS.Vector2 value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static ImGuiDir TitanImGui_ImGuiStyle_Visitor_FieldGet__WindowMenuButtonPosition(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImGuiStyle_Visitor_FieldSet__WindowMenuButtonPosition(void* self, ImGuiDir value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static float TitanImGui_ImGuiStyle_Visitor_FieldGet__ChildRounding(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImGuiStyle_Visitor_FieldSet__ChildRounding(void* self, float value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static float TitanImGui_ImGuiStyle_Visitor_FieldGet__ChildBorderSize(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImGuiStyle_Visitor_FieldSet__ChildBorderSize(void* self, float value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static float TitanImGui_ImGuiStyle_Visitor_FieldGet__PopupRounding(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImGuiStyle_Visitor_FieldSet__PopupRounding(void* self, float value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static float TitanImGui_ImGuiStyle_Visitor_FieldGet__PopupBorderSize(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImGuiStyle_Visitor_FieldSet__PopupBorderSize(void* self, float value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static EngineNS.Vector2 TitanImGui_ImGuiStyle_Visitor_FieldGet__FramePadding(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImGuiStyle_Visitor_FieldSet__FramePadding(void* self, EngineNS.Vector2 value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static float TitanImGui_ImGuiStyle_Visitor_FieldGet__FrameRounding(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImGuiStyle_Visitor_FieldSet__FrameRounding(void* self, float value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static float TitanImGui_ImGuiStyle_Visitor_FieldGet__FrameBorderSize(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImGuiStyle_Visitor_FieldSet__FrameBorderSize(void* self, float value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static EngineNS.Vector2 TitanImGui_ImGuiStyle_Visitor_FieldGet__ItemSpacing(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImGuiStyle_Visitor_FieldSet__ItemSpacing(void* self, EngineNS.Vector2 value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static EngineNS.Vector2 TitanImGui_ImGuiStyle_Visitor_FieldGet__ItemInnerSpacing(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImGuiStyle_Visitor_FieldSet__ItemInnerSpacing(void* self, EngineNS.Vector2 value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static EngineNS.Vector2 TitanImGui_ImGuiStyle_Visitor_FieldGet__CellPadding(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImGuiStyle_Visitor_FieldSet__CellPadding(void* self, EngineNS.Vector2 value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static EngineNS.Vector2 TitanImGui_ImGuiStyle_Visitor_FieldGet__TouchExtraPadding(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImGuiStyle_Visitor_FieldSet__TouchExtraPadding(void* self, EngineNS.Vector2 value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static float TitanImGui_ImGuiStyle_Visitor_FieldGet__IndentSpacing(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImGuiStyle_Visitor_FieldSet__IndentSpacing(void* self, float value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static float TitanImGui_ImGuiStyle_Visitor_FieldGet__ColumnsMinSpacing(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImGuiStyle_Visitor_FieldSet__ColumnsMinSpacing(void* self, float value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static float TitanImGui_ImGuiStyle_Visitor_FieldGet__ScrollbarSize(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImGuiStyle_Visitor_FieldSet__ScrollbarSize(void* self, float value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static float TitanImGui_ImGuiStyle_Visitor_FieldGet__ScrollbarRounding(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImGuiStyle_Visitor_FieldSet__ScrollbarRounding(void* self, float value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static float TitanImGui_ImGuiStyle_Visitor_FieldGet__ScrollbarPadding(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImGuiStyle_Visitor_FieldSet__ScrollbarPadding(void* self, float value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static float TitanImGui_ImGuiStyle_Visitor_FieldGet__GrabMinSize(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImGuiStyle_Visitor_FieldSet__GrabMinSize(void* self, float value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static float TitanImGui_ImGuiStyle_Visitor_FieldGet__GrabRounding(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImGuiStyle_Visitor_FieldSet__GrabRounding(void* self, float value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static float TitanImGui_ImGuiStyle_Visitor_FieldGet__LogSliderDeadzone(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImGuiStyle_Visitor_FieldSet__LogSliderDeadzone(void* self, float value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static float TitanImGui_ImGuiStyle_Visitor_FieldGet__ImageRounding(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImGuiStyle_Visitor_FieldSet__ImageRounding(void* self, float value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static float TitanImGui_ImGuiStyle_Visitor_FieldGet__ImageBorderSize(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImGuiStyle_Visitor_FieldSet__ImageBorderSize(void* self, float value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static float TitanImGui_ImGuiStyle_Visitor_FieldGet__TabRounding(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImGuiStyle_Visitor_FieldSet__TabRounding(void* self, float value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static float TitanImGui_ImGuiStyle_Visitor_FieldGet__TabBorderSize(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImGuiStyle_Visitor_FieldSet__TabBorderSize(void* self, float value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static float TitanImGui_ImGuiStyle_Visitor_FieldGet__TabMinWidthBase(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImGuiStyle_Visitor_FieldSet__TabMinWidthBase(void* self, float value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static float TitanImGui_ImGuiStyle_Visitor_FieldGet__TabMinWidthShrink(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImGuiStyle_Visitor_FieldSet__TabMinWidthShrink(void* self, float value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static float TitanImGui_ImGuiStyle_Visitor_FieldGet__TabCloseButtonMinWidthSelected(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImGuiStyle_Visitor_FieldSet__TabCloseButtonMinWidthSelected(void* self, float value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static float TitanImGui_ImGuiStyle_Visitor_FieldGet__TabCloseButtonMinWidthUnselected(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImGuiStyle_Visitor_FieldSet__TabCloseButtonMinWidthUnselected(void* self, float value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static float TitanImGui_ImGuiStyle_Visitor_FieldGet__TabBarBorderSize(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImGuiStyle_Visitor_FieldSet__TabBarBorderSize(void* self, float value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static float TitanImGui_ImGuiStyle_Visitor_FieldGet__TabBarOverlineSize(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImGuiStyle_Visitor_FieldSet__TabBarOverlineSize(void* self, float value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static float TitanImGui_ImGuiStyle_Visitor_FieldGet__TableAngledHeadersAngle(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImGuiStyle_Visitor_FieldSet__TableAngledHeadersAngle(void* self, float value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static EngineNS.Vector2 TitanImGui_ImGuiStyle_Visitor_FieldGet__TableAngledHeadersTextAlign(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImGuiStyle_Visitor_FieldSet__TableAngledHeadersTextAlign(void* self, EngineNS.Vector2 value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static int TitanImGui_ImGuiStyle_Visitor_FieldGet__TreeLinesFlags(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImGuiStyle_Visitor_FieldSet__TreeLinesFlags(void* self, int value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static float TitanImGui_ImGuiStyle_Visitor_FieldGet__TreeLinesSize(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImGuiStyle_Visitor_FieldSet__TreeLinesSize(void* self, float value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static float TitanImGui_ImGuiStyle_Visitor_FieldGet__TreeLinesRounding(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImGuiStyle_Visitor_FieldSet__TreeLinesRounding(void* self, float value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static float TitanImGui_ImGuiStyle_Visitor_FieldGet__DragDropTargetRounding(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImGuiStyle_Visitor_FieldSet__DragDropTargetRounding(void* self, float value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static float TitanImGui_ImGuiStyle_Visitor_FieldGet__DragDropTargetBorderSize(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImGuiStyle_Visitor_FieldSet__DragDropTargetBorderSize(void* self, float value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static float TitanImGui_ImGuiStyle_Visitor_FieldGet__DragDropTargetPadding(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImGuiStyle_Visitor_FieldSet__DragDropTargetPadding(void* self, float value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static float TitanImGui_ImGuiStyle_Visitor_FieldGet__ColorMarkerSize(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImGuiStyle_Visitor_FieldSet__ColorMarkerSize(void* self, float value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static ImGuiDir TitanImGui_ImGuiStyle_Visitor_FieldGet__ColorButtonPosition(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImGuiStyle_Visitor_FieldSet__ColorButtonPosition(void* self, ImGuiDir value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static EngineNS.Vector2 TitanImGui_ImGuiStyle_Visitor_FieldGet__ButtonTextAlign(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImGuiStyle_Visitor_FieldSet__ButtonTextAlign(void* self, EngineNS.Vector2 value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static EngineNS.Vector2 TitanImGui_ImGuiStyle_Visitor_FieldGet__SelectableTextAlign(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImGuiStyle_Visitor_FieldSet__SelectableTextAlign(void* self, EngineNS.Vector2 value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static float TitanImGui_ImGuiStyle_Visitor_FieldGet__SeparatorSize(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImGuiStyle_Visitor_FieldSet__SeparatorSize(void* self, float value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static float TitanImGui_ImGuiStyle_Visitor_FieldGet__SeparatorTextBorderSize(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImGuiStyle_Visitor_FieldSet__SeparatorTextBorderSize(void* self, float value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static EngineNS.Vector2 TitanImGui_ImGuiStyle_Visitor_FieldGet__SeparatorTextAlign(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImGuiStyle_Visitor_FieldSet__SeparatorTextAlign(void* self, EngineNS.Vector2 value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static EngineNS.Vector2 TitanImGui_ImGuiStyle_Visitor_FieldGet__SeparatorTextPadding(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImGuiStyle_Visitor_FieldSet__SeparatorTextPadding(void* self, EngineNS.Vector2 value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static EngineNS.Vector2 TitanImGui_ImGuiStyle_Visitor_FieldGet__DisplayWindowPadding(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImGuiStyle_Visitor_FieldSet__DisplayWindowPadding(void* self, EngineNS.Vector2 value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static EngineNS.Vector2 TitanImGui_ImGuiStyle_Visitor_FieldGet__DisplaySafeAreaPadding(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImGuiStyle_Visitor_FieldSet__DisplaySafeAreaPadding(void* self, EngineNS.Vector2 value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static sbyte TitanImGui_ImGuiStyle_Visitor_FieldGet__DockingNodeHasCloseButton(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImGuiStyle_Visitor_FieldSet__DockingNodeHasCloseButton(void* self, bool value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static float TitanImGui_ImGuiStyle_Visitor_FieldGet__DockingSeparatorSize(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImGuiStyle_Visitor_FieldSet__DockingSeparatorSize(void* self, float value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static float TitanImGui_ImGuiStyle_Visitor_FieldGet__MouseCursorScale(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImGuiStyle_Visitor_FieldSet__MouseCursorScale(void* self, float value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static sbyte TitanImGui_ImGuiStyle_Visitor_FieldGet__AntiAliasedLines(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImGuiStyle_Visitor_FieldSet__AntiAliasedLines(void* self, bool value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static sbyte TitanImGui_ImGuiStyle_Visitor_FieldGet__AntiAliasedLinesUseTex(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImGuiStyle_Visitor_FieldSet__AntiAliasedLinesUseTex(void* self, bool value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static sbyte TitanImGui_ImGuiStyle_Visitor_FieldGet__AntiAliasedFill(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImGuiStyle_Visitor_FieldSet__AntiAliasedFill(void* self, bool value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static float TitanImGui_ImGuiStyle_Visitor_FieldGet__CurveTessellationTol(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImGuiStyle_Visitor_FieldSet__CurveTessellationTol(void* self, float value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static float TitanImGui_ImGuiStyle_Visitor_FieldGet__CircleTessellationMaxError(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImGuiStyle_Visitor_FieldSet__CircleTessellationMaxError(void* self, float value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static EngineNS.Vector4* TitanImGui_ImGuiStyle_Visitor_FieldGet__Colors(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImGuiStyle_Visitor_FieldSet__Colors(void* self, EngineNS.Vector4* value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static float TitanImGui_ImGuiStyle_Visitor_FieldGet__HoverStationaryDelay(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImGuiStyle_Visitor_FieldSet__HoverStationaryDelay(void* self, float value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static float TitanImGui_ImGuiStyle_Visitor_FieldGet__HoverDelayShort(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImGuiStyle_Visitor_FieldSet__HoverDelayShort(void* self, float value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static float TitanImGui_ImGuiStyle_Visitor_FieldGet__HoverDelayNormal(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImGuiStyle_Visitor_FieldSet__HoverDelayNormal(void* self, float value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static int TitanImGui_ImGuiStyle_Visitor_FieldGet__HoverFlagsForTooltipMouse(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImGuiStyle_Visitor_FieldSet__HoverFlagsForTooltipMouse(void* self, int value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static int TitanImGui_ImGuiStyle_Visitor_FieldGet__HoverFlagsForTooltipNav(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImGuiStyle_Visitor_FieldSet__HoverFlagsForTooltipNav(void* self, int value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static float TitanImGui_ImGuiStyle_Visitor_FieldGet___MainScale(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImGuiStyle_Visitor_FieldSet___MainScale(void* self, float value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static float TitanImGui_ImGuiStyle_Visitor_FieldGet___NextFrameFontSizeBase(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImGuiStyle_Visitor_FieldSet___NextFrameFontSizeBase(void* self, float value);
	//Functions
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	extern static void TitanImGui_ImGuiStyle_Visitor_ScaleAllSizes_1759962673(void* Self, float scale_factor);
	//Cast
	#endregion
}
