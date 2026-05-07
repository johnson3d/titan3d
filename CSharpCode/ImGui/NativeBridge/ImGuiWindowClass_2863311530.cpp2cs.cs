//generated from legacy CppWeaving ImGui surface; native entrypoints routed through TitanImGuiBridge
using System;
using System.Runtime.InteropServices;
#if DEBUG
	using SuppressGC = EngineNS.Rtti.TtDummyAttribute;
#else
	using SuppressGC = System.Runtime.InteropServices.SuppressGCTransitionAttribute;
#endif


[System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Explicit, Size = 32, Pack = 4)]
public unsafe partial struct ImGuiWindowClass : EngineNS.IPtrType
{
	#region StructLayout
	[System.Runtime.InteropServices.FieldOffset(0)]
	public uint m_ClassId;
	[System.Runtime.InteropServices.FieldOffset(4)]
	public uint m_ParentViewportId;
	[System.Runtime.InteropServices.FieldOffset(8)]
	public uint m_FocusRouteParentWindowId;
	[System.Runtime.InteropServices.FieldOffset(12)]
	public ImGuiViewportFlags_ m_ViewportFlagsOverrideSet;
	[System.Runtime.InteropServices.FieldOffset(16)]
	public ImGuiViewportFlags_ m_ViewportFlagsOverrideClear;
	[System.Runtime.InteropServices.FieldOffset(20)]
	public ImGuiTabItemFlags_ m_TabItemFlagsOverrideSet;
	[System.Runtime.InteropServices.FieldOffset(24)]
	public ImGuiDockNodeFlags_ m_DockNodeFlagsOverrideSet;
	[System.Runtime.InteropServices.FieldOffset(28)]
	public bool m_DockingAlwaysTabBar;
	[System.Runtime.InteropServices.FieldOffset(29)]
	public bool m_DockingAllowUnclassed;
	#endregion
	public IntPtr NativePointer { get => IntPtr.Zero; set {} }
	#region Constructor&Cast
	public void UnsafeCallConstructor()
	{
		fixed (ImGuiWindowClass* mPointer = &this)
		{
			TitanImGui_ImGuiWindowClass_Visitor_UnsafeCallConstructor_2960189489(mPointer);
		}
	}
	public void UnsafeCallDestructor()
	{
		fixed (ImGuiWindowClass* mPointer = &this)
		{
			TitanImGui_ImGuiWindowClass_Visitor_UnsafeCallDestructor(mPointer);
		}
	}
	#endregion
	#region Fields
	public uint ClassId
	{
		get
		{
			fixed (ImGuiWindowClass* mPointer = &this)
			{
				return TitanImGui_ImGuiWindowClass_Visitor_FieldGet__ClassId(mPointer);
			}
		}
		set
		{
			fixed (ImGuiWindowClass* mPointer = &this)
			{
				TitanImGui_ImGuiWindowClass_Visitor_FieldSet__ClassId(mPointer, value);
			}
		}
	}
	public uint ParentViewportId
	{
		get
		{
			fixed (ImGuiWindowClass* mPointer = &this)
			{
				return TitanImGui_ImGuiWindowClass_Visitor_FieldGet__ParentViewportId(mPointer);
			}
		}
		set
		{
			fixed (ImGuiWindowClass* mPointer = &this)
			{
				TitanImGui_ImGuiWindowClass_Visitor_FieldSet__ParentViewportId(mPointer, value);
			}
		}
	}
	public uint FocusRouteParentWindowId
	{
		get
		{
			fixed (ImGuiWindowClass* mPointer = &this)
			{
				return TitanImGui_ImGuiWindowClass_Visitor_FieldGet__FocusRouteParentWindowId(mPointer);
			}
		}
		set
		{
			fixed (ImGuiWindowClass* mPointer = &this)
			{
				TitanImGui_ImGuiWindowClass_Visitor_FieldSet__FocusRouteParentWindowId(mPointer, value);
			}
		}
	}
	public ImGuiViewportFlags_ ViewportFlagsOverrideSet
	{
		get
		{
			fixed (ImGuiWindowClass* mPointer = &this)
			{
				return TitanImGui_ImGuiWindowClass_Visitor_FieldGet__ViewportFlagsOverrideSet(mPointer);
			}
		}
		set
		{
			fixed (ImGuiWindowClass* mPointer = &this)
			{
				TitanImGui_ImGuiWindowClass_Visitor_FieldSet__ViewportFlagsOverrideSet(mPointer, value);
			}
		}
	}
	public ImGuiViewportFlags_ ViewportFlagsOverrideClear
	{
		get
		{
			fixed (ImGuiWindowClass* mPointer = &this)
			{
				return TitanImGui_ImGuiWindowClass_Visitor_FieldGet__ViewportFlagsOverrideClear(mPointer);
			}
		}
		set
		{
			fixed (ImGuiWindowClass* mPointer = &this)
			{
				TitanImGui_ImGuiWindowClass_Visitor_FieldSet__ViewportFlagsOverrideClear(mPointer, value);
			}
		}
	}
	public ImGuiTabItemFlags_ TabItemFlagsOverrideSet
	{
		get
		{
			fixed (ImGuiWindowClass* mPointer = &this)
			{
				return TitanImGui_ImGuiWindowClass_Visitor_FieldGet__TabItemFlagsOverrideSet(mPointer);
			}
		}
		set
		{
			fixed (ImGuiWindowClass* mPointer = &this)
			{
				TitanImGui_ImGuiWindowClass_Visitor_FieldSet__TabItemFlagsOverrideSet(mPointer, value);
			}
		}
	}
	public ImGuiDockNodeFlags_ DockNodeFlagsOverrideSet
	{
		get
		{
			fixed (ImGuiWindowClass* mPointer = &this)
			{
				return TitanImGui_ImGuiWindowClass_Visitor_FieldGet__DockNodeFlagsOverrideSet(mPointer);
			}
		}
		set
		{
			fixed (ImGuiWindowClass* mPointer = &this)
			{
				TitanImGui_ImGuiWindowClass_Visitor_FieldSet__DockNodeFlagsOverrideSet(mPointer, value);
			}
		}
	}
	public bool DockingAlwaysTabBar
	{
		get
		{
			fixed (ImGuiWindowClass* mPointer = &this)
			{
				return TitanImGui_ImGuiWindowClass_Visitor_FieldGet__DockingAlwaysTabBar(mPointer) == 0 ? false : true;
			}
		}
		set
		{
			fixed (ImGuiWindowClass* mPointer = &this)
			{
				TitanImGui_ImGuiWindowClass_Visitor_FieldSet__DockingAlwaysTabBar(mPointer, value);
			}
		}
	}
	public bool DockingAllowUnclassed
	{
		get
		{
			fixed (ImGuiWindowClass* mPointer = &this)
			{
				return TitanImGui_ImGuiWindowClass_Visitor_FieldGet__DockingAllowUnclassed(mPointer) == 0 ? false : true;
			}
		}
		set
		{
			fixed (ImGuiWindowClass* mPointer = &this)
			{
				TitanImGui_ImGuiWindowClass_Visitor_FieldSet__DockingAllowUnclassed(mPointer, value);
			}
		}
	}
	#endregion
	#region Function
	#endregion
	#region Core SDK
	const string ModuleNC = EngineNS.CoreSDK.CoreModule;
	//Constructor&Cast
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	extern static void TitanImGui_ImGuiWindowClass_Visitor_UnsafeCallConstructor_2960189489(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	extern static void TitanImGui_ImGuiWindowClass_Visitor_UnsafeCallDestructor(void* self);
	//Fields
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static uint TitanImGui_ImGuiWindowClass_Visitor_FieldGet__ClassId(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImGuiWindowClass_Visitor_FieldSet__ClassId(void* self, uint value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static uint TitanImGui_ImGuiWindowClass_Visitor_FieldGet__ParentViewportId(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImGuiWindowClass_Visitor_FieldSet__ParentViewportId(void* self, uint value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static uint TitanImGui_ImGuiWindowClass_Visitor_FieldGet__FocusRouteParentWindowId(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImGuiWindowClass_Visitor_FieldSet__FocusRouteParentWindowId(void* self, uint value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static ImGuiViewportFlags_ TitanImGui_ImGuiWindowClass_Visitor_FieldGet__ViewportFlagsOverrideSet(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImGuiWindowClass_Visitor_FieldSet__ViewportFlagsOverrideSet(void* self, ImGuiViewportFlags_ value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static ImGuiViewportFlags_ TitanImGui_ImGuiWindowClass_Visitor_FieldGet__ViewportFlagsOverrideClear(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImGuiWindowClass_Visitor_FieldSet__ViewportFlagsOverrideClear(void* self, ImGuiViewportFlags_ value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static ImGuiTabItemFlags_ TitanImGui_ImGuiWindowClass_Visitor_FieldGet__TabItemFlagsOverrideSet(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImGuiWindowClass_Visitor_FieldSet__TabItemFlagsOverrideSet(void* self, ImGuiTabItemFlags_ value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static ImGuiDockNodeFlags_ TitanImGui_ImGuiWindowClass_Visitor_FieldGet__DockNodeFlagsOverrideSet(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImGuiWindowClass_Visitor_FieldSet__DockNodeFlagsOverrideSet(void* self, ImGuiDockNodeFlags_ value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static sbyte TitanImGui_ImGuiWindowClass_Visitor_FieldGet__DockingAlwaysTabBar(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImGuiWindowClass_Visitor_FieldSet__DockingAlwaysTabBar(void* self, bool value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static sbyte TitanImGui_ImGuiWindowClass_Visitor_FieldGet__DockingAllowUnclassed(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImGuiWindowClass_Visitor_FieldSet__DockingAllowUnclassed(void* self, bool value);
	//Functions
	//Cast
	#endregion
}
