//generated from legacy CppWeaving ImGui surface; native entrypoints routed through TitanImGuiBridge
using System;
using System.Runtime.InteropServices;
#if DEBUG
	using SuppressGC = EngineNS.Rtti.TtDummyAttribute;
#else
	using SuppressGC = System.Runtime.InteropServices.SuppressGCTransitionAttribute;
#endif


namespace EngineNS
{
	[System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Explicit, Size = 16, Pack = 8)]
	public unsafe partial struct ImGuiViewportDataSDL2 : EngineNS.IPtrType
	{
		#region StructLayout
		[System.Runtime.InteropServices.FieldOffset(0)]
		public void* m_Window;
		[System.Runtime.InteropServices.FieldOffset(8)]
		public uint m_WindowID;
		[System.Runtime.InteropServices.FieldOffset(12)]
		public bool m_WindowOwned;
		#endregion
		public IntPtr NativePointer { get => IntPtr.Zero; set {} }
		#region Constructor&Cast
		public void UnsafeCallConstructor()
		{
			fixed (ImGuiViewportDataSDL2* mPointer = &this)
			{
				TitanImGui_ImGuiViewportDataSDL2_Visitor_UnsafeCallConstructor_2960189489(mPointer);
			}
		}
		public void UnsafeCallDestructor()
		{
			fixed (ImGuiViewportDataSDL2* mPointer = &this)
			{
				TitanImGui_ImGuiViewportDataSDL2_Visitor_UnsafeCallDestructor(mPointer);
			}
		}
		#endregion
		#region Fields
		public void* Window
		{
			get
			{
				fixed (ImGuiViewportDataSDL2* mPointer = &this)
				{
					return TitanImGui_ImGuiViewportDataSDL2_Visitor_FieldGet__Window(mPointer);
				}
			}
			set
			{
				fixed (ImGuiViewportDataSDL2* mPointer = &this)
				{
					TitanImGui_ImGuiViewportDataSDL2_Visitor_FieldSet__Window(mPointer, value);
				}
			}
		}
		public uint WindowID
		{
			get
			{
				fixed (ImGuiViewportDataSDL2* mPointer = &this)
				{
					return TitanImGui_ImGuiViewportDataSDL2_Visitor_FieldGet__WindowID(mPointer);
				}
			}
			set
			{
				fixed (ImGuiViewportDataSDL2* mPointer = &this)
				{
					TitanImGui_ImGuiViewportDataSDL2_Visitor_FieldSet__WindowID(mPointer, value);
				}
			}
		}
		public bool WindowOwned
		{
			get
			{
				fixed (ImGuiViewportDataSDL2* mPointer = &this)
				{
					return TitanImGui_ImGuiViewportDataSDL2_Visitor_FieldGet__WindowOwned(mPointer) == 0 ? false : true;
				}
			}
			set
			{
				fixed (ImGuiViewportDataSDL2* mPointer = &this)
				{
					TitanImGui_ImGuiViewportDataSDL2_Visitor_FieldSet__WindowOwned(mPointer, value);
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
		extern static void TitanImGui_ImGuiViewportDataSDL2_Visitor_UnsafeCallConstructor_2960189489(void* self);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static void TitanImGui_ImGuiViewportDataSDL2_Visitor_UnsafeCallDestructor(void* self);
		//Fields
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		[SuppressGC]
		extern static void* TitanImGui_ImGuiViewportDataSDL2_Visitor_FieldGet__Window(void* self);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		[SuppressGC]
		extern static void TitanImGui_ImGuiViewportDataSDL2_Visitor_FieldSet__Window(void* self, void* value);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		[SuppressGC]
		extern static uint TitanImGui_ImGuiViewportDataSDL2_Visitor_FieldGet__WindowID(void* self);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		[SuppressGC]
		extern static void TitanImGui_ImGuiViewportDataSDL2_Visitor_FieldSet__WindowID(void* self, uint value);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		[SuppressGC]
		extern static sbyte TitanImGui_ImGuiViewportDataSDL2_Visitor_FieldGet__WindowOwned(void* self);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		[SuppressGC]
		extern static void TitanImGui_ImGuiViewportDataSDL2_Visitor_FieldSet__WindowOwned(void* self, bool value);
		//Functions
		//Cast
		#endregion
	}
}
