//generated from legacy CppWeaving ImGui surface; native entrypoints routed through TitanImGuiBridge
using System;
using System.Runtime.InteropServices;
#if DEBUG
	using SuppressGC = EngineNS.Rtti.TtDummyAttribute;
#else
	using SuppressGC = System.Runtime.InteropServices.SuppressGCTransitionAttribute;
#endif


[System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Explicit, Size = 48, Pack = 8)]
public unsafe partial struct ImGuiPlatformMonitor : EngineNS.IPtrType
{
	#region StructLayout
	[System.Runtime.InteropServices.FieldOffset(0)]
	public EngineNS.Vector2 m_MainPos;
	[System.Runtime.InteropServices.FieldOffset(8)]
	public EngineNS.Vector2 m_MainSize;
	[System.Runtime.InteropServices.FieldOffset(16)]
	public EngineNS.Vector2 m_WorkPos;
	[System.Runtime.InteropServices.FieldOffset(24)]
	public EngineNS.Vector2 m_WorkSize;
	[System.Runtime.InteropServices.FieldOffset(32)]
	public float m_DpiScale;
	[System.Runtime.InteropServices.FieldOffset(40)]
	public void* m_PlatformHandle;
	#endregion
	public IntPtr NativePointer { get => IntPtr.Zero; set {} }
	#region Constructor&Cast
	public void UnsafeCallConstructor()
	{
		fixed (ImGuiPlatformMonitor* mPointer = &this)
		{
			TitanImGui_ImGuiPlatformMonitor_Visitor_UnsafeCallConstructor_2960189489(mPointer);
		}
	}
	public void UnsafeCallDestructor()
	{
		fixed (ImGuiPlatformMonitor* mPointer = &this)
		{
			TitanImGui_ImGuiPlatformMonitor_Visitor_UnsafeCallDestructor(mPointer);
		}
	}
	#endregion
	#region Fields
	public EngineNS.Vector2 MainPos
	{
		get
		{
			fixed (ImGuiPlatformMonitor* mPointer = &this)
			{
				return TitanImGui_ImGuiPlatformMonitor_Visitor_FieldGet__MainPos(mPointer);
			}
		}
		set
		{
			fixed (ImGuiPlatformMonitor* mPointer = &this)
			{
				TitanImGui_ImGuiPlatformMonitor_Visitor_FieldSet__MainPos(mPointer, value);
			}
		}
	}
	public EngineNS.Vector2 MainSize
	{
		get
		{
			fixed (ImGuiPlatformMonitor* mPointer = &this)
			{
				return TitanImGui_ImGuiPlatformMonitor_Visitor_FieldGet__MainSize(mPointer);
			}
		}
		set
		{
			fixed (ImGuiPlatformMonitor* mPointer = &this)
			{
				TitanImGui_ImGuiPlatformMonitor_Visitor_FieldSet__MainSize(mPointer, value);
			}
		}
	}
	public EngineNS.Vector2 WorkPos
	{
		get
		{
			fixed (ImGuiPlatformMonitor* mPointer = &this)
			{
				return TitanImGui_ImGuiPlatformMonitor_Visitor_FieldGet__WorkPos(mPointer);
			}
		}
		set
		{
			fixed (ImGuiPlatformMonitor* mPointer = &this)
			{
				TitanImGui_ImGuiPlatformMonitor_Visitor_FieldSet__WorkPos(mPointer, value);
			}
		}
	}
	public EngineNS.Vector2 WorkSize
	{
		get
		{
			fixed (ImGuiPlatformMonitor* mPointer = &this)
			{
				return TitanImGui_ImGuiPlatformMonitor_Visitor_FieldGet__WorkSize(mPointer);
			}
		}
		set
		{
			fixed (ImGuiPlatformMonitor* mPointer = &this)
			{
				TitanImGui_ImGuiPlatformMonitor_Visitor_FieldSet__WorkSize(mPointer, value);
			}
		}
	}
	public float DpiScale
	{
		get
		{
			fixed (ImGuiPlatformMonitor* mPointer = &this)
			{
				return TitanImGui_ImGuiPlatformMonitor_Visitor_FieldGet__DpiScale(mPointer);
			}
		}
		set
		{
			fixed (ImGuiPlatformMonitor* mPointer = &this)
			{
				TitanImGui_ImGuiPlatformMonitor_Visitor_FieldSet__DpiScale(mPointer, value);
			}
		}
	}
	public void* PlatformHandle
	{
		get
		{
			fixed (ImGuiPlatformMonitor* mPointer = &this)
			{
				return TitanImGui_ImGuiPlatformMonitor_Visitor_FieldGet__PlatformHandle(mPointer);
			}
		}
		set
		{
			fixed (ImGuiPlatformMonitor* mPointer = &this)
			{
				TitanImGui_ImGuiPlatformMonitor_Visitor_FieldSet__PlatformHandle(mPointer, value);
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
	extern static void TitanImGui_ImGuiPlatformMonitor_Visitor_UnsafeCallConstructor_2960189489(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	extern static void TitanImGui_ImGuiPlatformMonitor_Visitor_UnsafeCallDestructor(void* self);
	//Fields
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static EngineNS.Vector2 TitanImGui_ImGuiPlatformMonitor_Visitor_FieldGet__MainPos(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImGuiPlatformMonitor_Visitor_FieldSet__MainPos(void* self, EngineNS.Vector2 value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static EngineNS.Vector2 TitanImGui_ImGuiPlatformMonitor_Visitor_FieldGet__MainSize(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImGuiPlatformMonitor_Visitor_FieldSet__MainSize(void* self, EngineNS.Vector2 value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static EngineNS.Vector2 TitanImGui_ImGuiPlatformMonitor_Visitor_FieldGet__WorkPos(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImGuiPlatformMonitor_Visitor_FieldSet__WorkPos(void* self, EngineNS.Vector2 value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static EngineNS.Vector2 TitanImGui_ImGuiPlatformMonitor_Visitor_FieldGet__WorkSize(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImGuiPlatformMonitor_Visitor_FieldSet__WorkSize(void* self, EngineNS.Vector2 value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static float TitanImGui_ImGuiPlatformMonitor_Visitor_FieldGet__DpiScale(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImGuiPlatformMonitor_Visitor_FieldSet__DpiScale(void* self, float value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void* TitanImGui_ImGuiPlatformMonitor_Visitor_FieldGet__PlatformHandle(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImGuiPlatformMonitor_Visitor_FieldSet__PlatformHandle(void* self, void* value);
	//Functions
	//Cast
	#endregion
}
