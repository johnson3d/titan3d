//generated from legacy CppWeaving ImGui surface; native entrypoints routed through TitanImGuiBridge
using System;
using System.Runtime.InteropServices;
#if DEBUG
	using SuppressGC = EngineNS.Rtti.TtDummyAttribute;
#else
	using SuppressGC = System.Runtime.InteropServices.SuppressGCTransitionAttribute;
#endif


public unsafe partial struct ImGuiPlatformIO : EngineNS.IPtrType
{
	[System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Explicit, Size = 328, Pack = 8)]
	public struct CppStructLayout
	{
		[System.Runtime.InteropServices.FieldOffset(0)]
		public IntPtr Platform_GetClipboardTextFn;
		[System.Runtime.InteropServices.FieldOffset(8)]
		public IntPtr Platform_SetClipboardTextFn;
		[System.Runtime.InteropServices.FieldOffset(16)]
		public void* Platform_ClipboardUserData;
		[System.Runtime.InteropServices.FieldOffset(24)]
		public IntPtr Platform_OpenInShellFn;
		[System.Runtime.InteropServices.FieldOffset(32)]
		public void* Platform_OpenInShellUserData;
		[System.Runtime.InteropServices.FieldOffset(40)]
		public IntPtr Platform_SetImeDataFn;
		[System.Runtime.InteropServices.FieldOffset(48)]
		public void* Platform_ImeUserData;
		[System.Runtime.InteropServices.FieldOffset(56)]
		public Wchar16 Platform_LocaleDecimalPoint;
		[System.Runtime.InteropServices.FieldOffset(60)]
		public int Renderer_TextureMaxWidth;
		[System.Runtime.InteropServices.FieldOffset(64)]
		public int Renderer_TextureMaxHeight;
		[System.Runtime.InteropServices.FieldOffset(72)]
		public void* Renderer_RenderState;
		[System.Runtime.InteropServices.FieldOffset(80)]
		public IntPtr Platform_CreateWindow;
		[System.Runtime.InteropServices.FieldOffset(88)]
		public IntPtr Platform_DestroyWindow;
		[System.Runtime.InteropServices.FieldOffset(96)]
		public IntPtr Platform_ShowWindow;
		[System.Runtime.InteropServices.FieldOffset(104)]
		public IntPtr Platform_SetWindowPos;
		[System.Runtime.InteropServices.FieldOffset(112)]
		public IntPtr Platform_GetWindowPos;
		[System.Runtime.InteropServices.FieldOffset(120)]
		public IntPtr Platform_SetWindowSize;
		[System.Runtime.InteropServices.FieldOffset(128)]
		public IntPtr Platform_GetWindowSize;
		[System.Runtime.InteropServices.FieldOffset(136)]
		public IntPtr Platform_GetWindowFramebufferScale;
		[System.Runtime.InteropServices.FieldOffset(144)]
		public IntPtr Platform_SetWindowFocus;
		[System.Runtime.InteropServices.FieldOffset(152)]
		public IntPtr Platform_GetWindowFocus;
		[System.Runtime.InteropServices.FieldOffset(160)]
		public IntPtr Platform_GetWindowMinimized;
		[System.Runtime.InteropServices.FieldOffset(168)]
		public IntPtr Platform_SetWindowTitle;
		[System.Runtime.InteropServices.FieldOffset(176)]
		public IntPtr Platform_SetWindowAlpha;
		[System.Runtime.InteropServices.FieldOffset(184)]
		public IntPtr Platform_UpdateWindow;
		[System.Runtime.InteropServices.FieldOffset(192)]
		public IntPtr Platform_RenderWindow;
		[System.Runtime.InteropServices.FieldOffset(200)]
		public IntPtr Platform_SwapBuffers;
		[System.Runtime.InteropServices.FieldOffset(208)]
		public IntPtr Platform_GetWindowDpiScale;
		[System.Runtime.InteropServices.FieldOffset(216)]
		public IntPtr Platform_OnChangedViewport;
		[System.Runtime.InteropServices.FieldOffset(224)]
		public IntPtr Platform_GetWindowWorkAreaInsets;
		[System.Runtime.InteropServices.FieldOffset(232)]
		public IntPtr Platform_CreateVkSurface;
		[System.Runtime.InteropServices.FieldOffset(240)]
		public IntPtr Renderer_CreateWindow;
		[System.Runtime.InteropServices.FieldOffset(248)]
		public IntPtr Renderer_DestroyWindow;
		[System.Runtime.InteropServices.FieldOffset(256)]
		public IntPtr Renderer_SetWindowSize;
		[System.Runtime.InteropServices.FieldOffset(264)]
		public IntPtr Renderer_RenderWindow;
		[System.Runtime.InteropServices.FieldOffset(272)]
		public IntPtr Renderer_SwapBuffers;
	}
	private void* mPointer;
	public CppStructLayout* UnsafeAsLayout { get => (CppStructLayout*)mPointer; }
	public ImGuiPlatformIO(void* p) { mPointer = p; }
	public void UnsafeSetPointer(void* p) { mPointer = p; }
	public IntPtr NativePointer { get => (IntPtr)mPointer; set => mPointer = value.ToPointer(); }
	public ImGuiPlatformIO* CppPointer { get => (ImGuiPlatformIO*)mPointer; }
	public bool IsValidPointer { get => mPointer != (void*)0; }
	public static implicit operator ImGuiPlatformIO* (ImGuiPlatformIO v)
	{
		return (ImGuiPlatformIO*)v.mPointer;
	}
	#region Constructor&Cast
	public static EngineNS.FRttiStruct GetTypeRtti()
	{
		return new EngineNS.FRttiStruct(TitanImGui_ImGuiPlatformIO_Visitor_GetTypeRtti());
	}
	public static ImGuiPlatformIO CreateInstance()
	{
		return new ImGuiPlatformIO(TitanImGui_ImGuiPlatformIO_Visitor_CreateInstance_2960189489());
	}
	#endregion
	#region Fields
	public unsafe delegate sbyte* FDelegate_Platform_GetClipboardTextFn();
	public FDelegate_Platform_GetClipboardTextFn Platform_GetClipboardTextFn
	{
		get
		{
			return TitanImGui_ImGuiPlatformIO_Visitor_FieldGet__Platform_GetClipboardTextFn(mPointer);
		}
		set
		{
			TitanImGui_ImGuiPlatformIO_Visitor_FieldSet__Platform_GetClipboardTextFn(mPointer, value);
		}
	}
	public unsafe delegate void FDelegate_Platform_SetClipboardTextFn();
	public FDelegate_Platform_SetClipboardTextFn Platform_SetClipboardTextFn
	{
		get
		{
			return TitanImGui_ImGuiPlatformIO_Visitor_FieldGet__Platform_SetClipboardTextFn(mPointer);
		}
		set
		{
			TitanImGui_ImGuiPlatformIO_Visitor_FieldSet__Platform_SetClipboardTextFn(mPointer, value);
		}
	}
	public void* Platform_ClipboardUserData
	{
		get
		{
			return TitanImGui_ImGuiPlatformIO_Visitor_FieldGet__Platform_ClipboardUserData(mPointer);
		}
		set
		{
			TitanImGui_ImGuiPlatformIO_Visitor_FieldSet__Platform_ClipboardUserData(mPointer, value);
		}
	}
	public unsafe delegate bool FDelegate_Platform_OpenInShellFn();
	public FDelegate_Platform_OpenInShellFn Platform_OpenInShellFn
	{
		get
		{
			return TitanImGui_ImGuiPlatformIO_Visitor_FieldGet__Platform_OpenInShellFn(mPointer);
		}
		set
		{
			TitanImGui_ImGuiPlatformIO_Visitor_FieldSet__Platform_OpenInShellFn(mPointer, value);
		}
	}
	public void* Platform_OpenInShellUserData
	{
		get
		{
			return TitanImGui_ImGuiPlatformIO_Visitor_FieldGet__Platform_OpenInShellUserData(mPointer);
		}
		set
		{
			TitanImGui_ImGuiPlatformIO_Visitor_FieldSet__Platform_OpenInShellUserData(mPointer, value);
		}
	}
	public unsafe delegate void FDelegate_Platform_SetImeDataFn();
	public FDelegate_Platform_SetImeDataFn Platform_SetImeDataFn
	{
		get
		{
			return TitanImGui_ImGuiPlatformIO_Visitor_FieldGet__Platform_SetImeDataFn(mPointer);
		}
		set
		{
			TitanImGui_ImGuiPlatformIO_Visitor_FieldSet__Platform_SetImeDataFn(mPointer, value);
		}
	}
	public void* Platform_ImeUserData
	{
		get
		{
			return TitanImGui_ImGuiPlatformIO_Visitor_FieldGet__Platform_ImeUserData(mPointer);
		}
		set
		{
			TitanImGui_ImGuiPlatformIO_Visitor_FieldSet__Platform_ImeUserData(mPointer, value);
		}
	}
	public Wchar16 Platform_LocaleDecimalPoint
	{
		get
		{
			return TitanImGui_ImGuiPlatformIO_Visitor_FieldGet__Platform_LocaleDecimalPoint(mPointer);
		}
		set
		{
			TitanImGui_ImGuiPlatformIO_Visitor_FieldSet__Platform_LocaleDecimalPoint(mPointer, value);
		}
	}
	public int Renderer_TextureMaxWidth
	{
		get
		{
			return TitanImGui_ImGuiPlatformIO_Visitor_FieldGet__Renderer_TextureMaxWidth(mPointer);
		}
		set
		{
			TitanImGui_ImGuiPlatformIO_Visitor_FieldSet__Renderer_TextureMaxWidth(mPointer, value);
		}
	}
	public int Renderer_TextureMaxHeight
	{
		get
		{
			return TitanImGui_ImGuiPlatformIO_Visitor_FieldGet__Renderer_TextureMaxHeight(mPointer);
		}
		set
		{
			TitanImGui_ImGuiPlatformIO_Visitor_FieldSet__Renderer_TextureMaxHeight(mPointer, value);
		}
	}
	public void* Renderer_RenderState
	{
		get
		{
			return TitanImGui_ImGuiPlatformIO_Visitor_FieldGet__Renderer_RenderState(mPointer);
		}
		set
		{
			TitanImGui_ImGuiPlatformIO_Visitor_FieldSet__Renderer_RenderState(mPointer, value);
		}
	}
	public unsafe delegate void FDelegate_Platform_CreateWindow(ImGuiViewport* arg0);
	public FDelegate_Platform_CreateWindow Platform_CreateWindow
	{
		get
		{
			return TitanImGui_ImGuiPlatformIO_Visitor_FieldGet__Platform_CreateWindow(mPointer);
		}
		set
		{
			TitanImGui_ImGuiPlatformIO_Visitor_FieldSet__Platform_CreateWindow(mPointer, value);
		}
	}
	public unsafe delegate void FDelegate_Platform_DestroyWindow(ImGuiViewport* arg0);
	public FDelegate_Platform_DestroyWindow Platform_DestroyWindow
	{
		get
		{
			return TitanImGui_ImGuiPlatformIO_Visitor_FieldGet__Platform_DestroyWindow(mPointer);
		}
		set
		{
			TitanImGui_ImGuiPlatformIO_Visitor_FieldSet__Platform_DestroyWindow(mPointer, value);
		}
	}
	public unsafe delegate void FDelegate_Platform_ShowWindow(ImGuiViewport* arg0);
	public FDelegate_Platform_ShowWindow Platform_ShowWindow
	{
		get
		{
			return TitanImGui_ImGuiPlatformIO_Visitor_FieldGet__Platform_ShowWindow(mPointer);
		}
		set
		{
			TitanImGui_ImGuiPlatformIO_Visitor_FieldSet__Platform_ShowWindow(mPointer, value);
		}
	}
	public unsafe delegate void FDelegate_Platform_SetWindowPos(ImGuiViewport* arg0,EngineNS.Vector2 arg1);
	public FDelegate_Platform_SetWindowPos Platform_SetWindowPos
	{
		get
		{
			return TitanImGui_ImGuiPlatformIO_Visitor_FieldGet__Platform_SetWindowPos(mPointer);
		}
		set
		{
			TitanImGui_ImGuiPlatformIO_Visitor_FieldSet__Platform_SetWindowPos(mPointer, value);
		}
	}
	public unsafe delegate EngineNS.Vector2 FDelegate_Platform_GetWindowPos(ImGuiViewport* arg0);
	public FDelegate_Platform_GetWindowPos Platform_GetWindowPos
	{
		get
		{
			return TitanImGui_ImGuiPlatformIO_Visitor_FieldGet__Platform_GetWindowPos(mPointer);
		}
		set
		{
			TitanImGui_ImGuiPlatformIO_Visitor_FieldSet__Platform_GetWindowPos(mPointer, value);
		}
	}
	public unsafe delegate void FDelegate_Platform_SetWindowSize(ImGuiViewport* arg0,EngineNS.Vector2 arg1);
	public FDelegate_Platform_SetWindowSize Platform_SetWindowSize
	{
		get
		{
			return TitanImGui_ImGuiPlatformIO_Visitor_FieldGet__Platform_SetWindowSize(mPointer);
		}
		set
		{
			TitanImGui_ImGuiPlatformIO_Visitor_FieldSet__Platform_SetWindowSize(mPointer, value);
		}
	}
	public unsafe delegate EngineNS.Vector2 FDelegate_Platform_GetWindowSize(ImGuiViewport* arg0);
	public FDelegate_Platform_GetWindowSize Platform_GetWindowSize
	{
		get
		{
			return TitanImGui_ImGuiPlatformIO_Visitor_FieldGet__Platform_GetWindowSize(mPointer);
		}
		set
		{
			TitanImGui_ImGuiPlatformIO_Visitor_FieldSet__Platform_GetWindowSize(mPointer, value);
		}
	}
	public unsafe delegate EngineNS.Vector2 FDelegate_Platform_GetWindowFramebufferScale(ImGuiViewport* arg0);
	public FDelegate_Platform_GetWindowFramebufferScale Platform_GetWindowFramebufferScale
	{
		get
		{
			return TitanImGui_ImGuiPlatformIO_Visitor_FieldGet__Platform_GetWindowFramebufferScale(mPointer);
		}
		set
		{
			TitanImGui_ImGuiPlatformIO_Visitor_FieldSet__Platform_GetWindowFramebufferScale(mPointer, value);
		}
	}
	public unsafe delegate void FDelegate_Platform_SetWindowFocus(ImGuiViewport* arg0);
	public FDelegate_Platform_SetWindowFocus Platform_SetWindowFocus
	{
		get
		{
			return TitanImGui_ImGuiPlatformIO_Visitor_FieldGet__Platform_SetWindowFocus(mPointer);
		}
		set
		{
			TitanImGui_ImGuiPlatformIO_Visitor_FieldSet__Platform_SetWindowFocus(mPointer, value);
		}
	}
	public unsafe delegate bool FDelegate_Platform_GetWindowFocus(ImGuiViewport* arg0);
	public FDelegate_Platform_GetWindowFocus Platform_GetWindowFocus
	{
		get
		{
			return TitanImGui_ImGuiPlatformIO_Visitor_FieldGet__Platform_GetWindowFocus(mPointer);
		}
		set
		{
			TitanImGui_ImGuiPlatformIO_Visitor_FieldSet__Platform_GetWindowFocus(mPointer, value);
		}
	}
	public unsafe delegate bool FDelegate_Platform_GetWindowMinimized(ImGuiViewport* arg0);
	public FDelegate_Platform_GetWindowMinimized Platform_GetWindowMinimized
	{
		get
		{
			return TitanImGui_ImGuiPlatformIO_Visitor_FieldGet__Platform_GetWindowMinimized(mPointer);
		}
		set
		{
			TitanImGui_ImGuiPlatformIO_Visitor_FieldSet__Platform_GetWindowMinimized(mPointer, value);
		}
	}
	public unsafe delegate void FDelegate_Platform_SetWindowTitle(ImGuiViewport* arg0,sbyte* arg1);
	public FDelegate_Platform_SetWindowTitle Platform_SetWindowTitle
	{
		get
		{
			return TitanImGui_ImGuiPlatformIO_Visitor_FieldGet__Platform_SetWindowTitle(mPointer);
		}
		set
		{
			TitanImGui_ImGuiPlatformIO_Visitor_FieldSet__Platform_SetWindowTitle(mPointer, value);
		}
	}
	public unsafe delegate void FDelegate_Platform_SetWindowAlpha(ImGuiViewport* arg0,float arg1);
	public FDelegate_Platform_SetWindowAlpha Platform_SetWindowAlpha
	{
		get
		{
			return TitanImGui_ImGuiPlatformIO_Visitor_FieldGet__Platform_SetWindowAlpha(mPointer);
		}
		set
		{
			TitanImGui_ImGuiPlatformIO_Visitor_FieldSet__Platform_SetWindowAlpha(mPointer, value);
		}
	}
	public unsafe delegate void FDelegate_Platform_UpdateWindow(ImGuiViewport* arg0);
	public FDelegate_Platform_UpdateWindow Platform_UpdateWindow
	{
		get
		{
			return TitanImGui_ImGuiPlatformIO_Visitor_FieldGet__Platform_UpdateWindow(mPointer);
		}
		set
		{
			TitanImGui_ImGuiPlatformIO_Visitor_FieldSet__Platform_UpdateWindow(mPointer, value);
		}
	}
	public unsafe delegate void FDelegate_Platform_RenderWindow(ImGuiViewport* arg0,void* arg1);
	public FDelegate_Platform_RenderWindow Platform_RenderWindow
	{
		get
		{
			return TitanImGui_ImGuiPlatformIO_Visitor_FieldGet__Platform_RenderWindow(mPointer);
		}
		set
		{
			TitanImGui_ImGuiPlatformIO_Visitor_FieldSet__Platform_RenderWindow(mPointer, value);
		}
	}
	public unsafe delegate void FDelegate_Platform_SwapBuffers(ImGuiViewport* arg0,void* arg1);
	public FDelegate_Platform_SwapBuffers Platform_SwapBuffers
	{
		get
		{
			return TitanImGui_ImGuiPlatformIO_Visitor_FieldGet__Platform_SwapBuffers(mPointer);
		}
		set
		{
			TitanImGui_ImGuiPlatformIO_Visitor_FieldSet__Platform_SwapBuffers(mPointer, value);
		}
	}
	public unsafe delegate float FDelegate_Platform_GetWindowDpiScale(ImGuiViewport* arg0);
	public FDelegate_Platform_GetWindowDpiScale Platform_GetWindowDpiScale
	{
		get
		{
			return TitanImGui_ImGuiPlatformIO_Visitor_FieldGet__Platform_GetWindowDpiScale(mPointer);
		}
		set
		{
			TitanImGui_ImGuiPlatformIO_Visitor_FieldSet__Platform_GetWindowDpiScale(mPointer, value);
		}
	}
	public unsafe delegate void FDelegate_Platform_OnChangedViewport(ImGuiViewport* arg0);
	public FDelegate_Platform_OnChangedViewport Platform_OnChangedViewport
	{
		get
		{
			return TitanImGui_ImGuiPlatformIO_Visitor_FieldGet__Platform_OnChangedViewport(mPointer);
		}
		set
		{
			TitanImGui_ImGuiPlatformIO_Visitor_FieldSet__Platform_OnChangedViewport(mPointer, value);
		}
	}
	public unsafe delegate EngineNS.Vector4 FDelegate_Platform_GetWindowWorkAreaInsets(ImGuiViewport* arg0);
	public FDelegate_Platform_GetWindowWorkAreaInsets Platform_GetWindowWorkAreaInsets
	{
		get
		{
			return TitanImGui_ImGuiPlatformIO_Visitor_FieldGet__Platform_GetWindowWorkAreaInsets(mPointer);
		}
		set
		{
			TitanImGui_ImGuiPlatformIO_Visitor_FieldSet__Platform_GetWindowWorkAreaInsets(mPointer, value);
		}
	}
	public unsafe delegate int FDelegate_Platform_CreateVkSurface(ImGuiViewport* arg0,ulong arg1,void* arg2,ulong* arg3);
	public FDelegate_Platform_CreateVkSurface Platform_CreateVkSurface
	{
		get
		{
			return TitanImGui_ImGuiPlatformIO_Visitor_FieldGet__Platform_CreateVkSurface(mPointer);
		}
		set
		{
			TitanImGui_ImGuiPlatformIO_Visitor_FieldSet__Platform_CreateVkSurface(mPointer, value);
		}
	}
	public unsafe delegate void FDelegate_Renderer_CreateWindow(ImGuiViewport* arg0);
	public FDelegate_Renderer_CreateWindow Renderer_CreateWindow
	{
		get
		{
			return TitanImGui_ImGuiPlatformIO_Visitor_FieldGet__Renderer_CreateWindow(mPointer);
		}
		set
		{
			TitanImGui_ImGuiPlatformIO_Visitor_FieldSet__Renderer_CreateWindow(mPointer, value);
		}
	}
	public unsafe delegate void FDelegate_Renderer_DestroyWindow(ImGuiViewport* arg0);
	public FDelegate_Renderer_DestroyWindow Renderer_DestroyWindow
	{
		get
		{
			return TitanImGui_ImGuiPlatformIO_Visitor_FieldGet__Renderer_DestroyWindow(mPointer);
		}
		set
		{
			TitanImGui_ImGuiPlatformIO_Visitor_FieldSet__Renderer_DestroyWindow(mPointer, value);
		}
	}
	public unsafe delegate void FDelegate_Renderer_SetWindowSize(ImGuiViewport* arg0,EngineNS.Vector2 arg1);
	public FDelegate_Renderer_SetWindowSize Renderer_SetWindowSize
	{
		get
		{
			return TitanImGui_ImGuiPlatformIO_Visitor_FieldGet__Renderer_SetWindowSize(mPointer);
		}
		set
		{
			TitanImGui_ImGuiPlatformIO_Visitor_FieldSet__Renderer_SetWindowSize(mPointer, value);
		}
	}
	public unsafe delegate void FDelegate_Renderer_RenderWindow(ImGuiViewport* arg0,void* arg1);
	public FDelegate_Renderer_RenderWindow Renderer_RenderWindow
	{
		get
		{
			return TitanImGui_ImGuiPlatformIO_Visitor_FieldGet__Renderer_RenderWindow(mPointer);
		}
		set
		{
			TitanImGui_ImGuiPlatformIO_Visitor_FieldSet__Renderer_RenderWindow(mPointer, value);
		}
	}
	public unsafe delegate void FDelegate_Renderer_SwapBuffers(ImGuiViewport* arg0,void* arg1);
	public FDelegate_Renderer_SwapBuffers Renderer_SwapBuffers
	{
		get
		{
			return TitanImGui_ImGuiPlatformIO_Visitor_FieldGet__Renderer_SwapBuffers(mPointer);
		}
		set
		{
			TitanImGui_ImGuiPlatformIO_Visitor_FieldSet__Renderer_SwapBuffers(mPointer, value);
		}
	}
	#endregion
	#region Function
	public void ClearPlatformHandlers()
	{
		TitanImGui_ImGuiPlatformIO_Visitor_ClearPlatformHandlers_2960189489(mPointer);
	}
	public void ClearRendererHandlers()
	{
		TitanImGui_ImGuiPlatformIO_Visitor_ClearRendererHandlers_2960189489(mPointer);
	}
	#endregion
	#region Core SDK
	const string ModuleNC = EngineNS.CoreSDK.CoreModule;
	//Constructor&Cast
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	extern static EngineNS.FRttiStruct* TitanImGui_ImGuiPlatformIO_Visitor_GetTypeRtti();
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	extern static ImGuiPlatformIO* TitanImGui_ImGuiPlatformIO_Visitor_CreateInstance_2960189489();
	//Fields
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static FDelegate_Platform_GetClipboardTextFn TitanImGui_ImGuiPlatformIO_Visitor_FieldGet__Platform_GetClipboardTextFn(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImGuiPlatformIO_Visitor_FieldSet__Platform_GetClipboardTextFn(void* self, FDelegate_Platform_GetClipboardTextFn value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static FDelegate_Platform_SetClipboardTextFn TitanImGui_ImGuiPlatformIO_Visitor_FieldGet__Platform_SetClipboardTextFn(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImGuiPlatformIO_Visitor_FieldSet__Platform_SetClipboardTextFn(void* self, FDelegate_Platform_SetClipboardTextFn value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void* TitanImGui_ImGuiPlatformIO_Visitor_FieldGet__Platform_ClipboardUserData(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImGuiPlatformIO_Visitor_FieldSet__Platform_ClipboardUserData(void* self, void* value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static FDelegate_Platform_OpenInShellFn TitanImGui_ImGuiPlatformIO_Visitor_FieldGet__Platform_OpenInShellFn(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImGuiPlatformIO_Visitor_FieldSet__Platform_OpenInShellFn(void* self, FDelegate_Platform_OpenInShellFn value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void* TitanImGui_ImGuiPlatformIO_Visitor_FieldGet__Platform_OpenInShellUserData(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImGuiPlatformIO_Visitor_FieldSet__Platform_OpenInShellUserData(void* self, void* value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static FDelegate_Platform_SetImeDataFn TitanImGui_ImGuiPlatformIO_Visitor_FieldGet__Platform_SetImeDataFn(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImGuiPlatformIO_Visitor_FieldSet__Platform_SetImeDataFn(void* self, FDelegate_Platform_SetImeDataFn value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void* TitanImGui_ImGuiPlatformIO_Visitor_FieldGet__Platform_ImeUserData(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImGuiPlatformIO_Visitor_FieldSet__Platform_ImeUserData(void* self, void* value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static Wchar16 TitanImGui_ImGuiPlatformIO_Visitor_FieldGet__Platform_LocaleDecimalPoint(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImGuiPlatformIO_Visitor_FieldSet__Platform_LocaleDecimalPoint(void* self, Wchar16 value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static int TitanImGui_ImGuiPlatformIO_Visitor_FieldGet__Renderer_TextureMaxWidth(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImGuiPlatformIO_Visitor_FieldSet__Renderer_TextureMaxWidth(void* self, int value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static int TitanImGui_ImGuiPlatformIO_Visitor_FieldGet__Renderer_TextureMaxHeight(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImGuiPlatformIO_Visitor_FieldSet__Renderer_TextureMaxHeight(void* self, int value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void* TitanImGui_ImGuiPlatformIO_Visitor_FieldGet__Renderer_RenderState(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImGuiPlatformIO_Visitor_FieldSet__Renderer_RenderState(void* self, void* value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static FDelegate_Platform_CreateWindow TitanImGui_ImGuiPlatformIO_Visitor_FieldGet__Platform_CreateWindow(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImGuiPlatformIO_Visitor_FieldSet__Platform_CreateWindow(void* self, FDelegate_Platform_CreateWindow value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static FDelegate_Platform_DestroyWindow TitanImGui_ImGuiPlatformIO_Visitor_FieldGet__Platform_DestroyWindow(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImGuiPlatformIO_Visitor_FieldSet__Platform_DestroyWindow(void* self, FDelegate_Platform_DestroyWindow value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static FDelegate_Platform_ShowWindow TitanImGui_ImGuiPlatformIO_Visitor_FieldGet__Platform_ShowWindow(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImGuiPlatformIO_Visitor_FieldSet__Platform_ShowWindow(void* self, FDelegate_Platform_ShowWindow value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static FDelegate_Platform_SetWindowPos TitanImGui_ImGuiPlatformIO_Visitor_FieldGet__Platform_SetWindowPos(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImGuiPlatformIO_Visitor_FieldSet__Platform_SetWindowPos(void* self, FDelegate_Platform_SetWindowPos value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static FDelegate_Platform_GetWindowPos TitanImGui_ImGuiPlatformIO_Visitor_FieldGet__Platform_GetWindowPos(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImGuiPlatformIO_Visitor_FieldSet__Platform_GetWindowPos(void* self, FDelegate_Platform_GetWindowPos value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static FDelegate_Platform_SetWindowSize TitanImGui_ImGuiPlatformIO_Visitor_FieldGet__Platform_SetWindowSize(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImGuiPlatformIO_Visitor_FieldSet__Platform_SetWindowSize(void* self, FDelegate_Platform_SetWindowSize value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static FDelegate_Platform_GetWindowSize TitanImGui_ImGuiPlatformIO_Visitor_FieldGet__Platform_GetWindowSize(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImGuiPlatformIO_Visitor_FieldSet__Platform_GetWindowSize(void* self, FDelegate_Platform_GetWindowSize value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static FDelegate_Platform_GetWindowFramebufferScale TitanImGui_ImGuiPlatformIO_Visitor_FieldGet__Platform_GetWindowFramebufferScale(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImGuiPlatformIO_Visitor_FieldSet__Platform_GetWindowFramebufferScale(void* self, FDelegate_Platform_GetWindowFramebufferScale value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static FDelegate_Platform_SetWindowFocus TitanImGui_ImGuiPlatformIO_Visitor_FieldGet__Platform_SetWindowFocus(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImGuiPlatformIO_Visitor_FieldSet__Platform_SetWindowFocus(void* self, FDelegate_Platform_SetWindowFocus value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static FDelegate_Platform_GetWindowFocus TitanImGui_ImGuiPlatformIO_Visitor_FieldGet__Platform_GetWindowFocus(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImGuiPlatformIO_Visitor_FieldSet__Platform_GetWindowFocus(void* self, FDelegate_Platform_GetWindowFocus value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static FDelegate_Platform_GetWindowMinimized TitanImGui_ImGuiPlatformIO_Visitor_FieldGet__Platform_GetWindowMinimized(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImGuiPlatformIO_Visitor_FieldSet__Platform_GetWindowMinimized(void* self, FDelegate_Platform_GetWindowMinimized value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static FDelegate_Platform_SetWindowTitle TitanImGui_ImGuiPlatformIO_Visitor_FieldGet__Platform_SetWindowTitle(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImGuiPlatformIO_Visitor_FieldSet__Platform_SetWindowTitle(void* self, FDelegate_Platform_SetWindowTitle value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static FDelegate_Platform_SetWindowAlpha TitanImGui_ImGuiPlatformIO_Visitor_FieldGet__Platform_SetWindowAlpha(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImGuiPlatformIO_Visitor_FieldSet__Platform_SetWindowAlpha(void* self, FDelegate_Platform_SetWindowAlpha value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static FDelegate_Platform_UpdateWindow TitanImGui_ImGuiPlatformIO_Visitor_FieldGet__Platform_UpdateWindow(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImGuiPlatformIO_Visitor_FieldSet__Platform_UpdateWindow(void* self, FDelegate_Platform_UpdateWindow value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static FDelegate_Platform_RenderWindow TitanImGui_ImGuiPlatformIO_Visitor_FieldGet__Platform_RenderWindow(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImGuiPlatformIO_Visitor_FieldSet__Platform_RenderWindow(void* self, FDelegate_Platform_RenderWindow value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static FDelegate_Platform_SwapBuffers TitanImGui_ImGuiPlatformIO_Visitor_FieldGet__Platform_SwapBuffers(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImGuiPlatformIO_Visitor_FieldSet__Platform_SwapBuffers(void* self, FDelegate_Platform_SwapBuffers value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static FDelegate_Platform_GetWindowDpiScale TitanImGui_ImGuiPlatformIO_Visitor_FieldGet__Platform_GetWindowDpiScale(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImGuiPlatformIO_Visitor_FieldSet__Platform_GetWindowDpiScale(void* self, FDelegate_Platform_GetWindowDpiScale value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static FDelegate_Platform_OnChangedViewport TitanImGui_ImGuiPlatformIO_Visitor_FieldGet__Platform_OnChangedViewport(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImGuiPlatformIO_Visitor_FieldSet__Platform_OnChangedViewport(void* self, FDelegate_Platform_OnChangedViewport value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static FDelegate_Platform_GetWindowWorkAreaInsets TitanImGui_ImGuiPlatformIO_Visitor_FieldGet__Platform_GetWindowWorkAreaInsets(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImGuiPlatformIO_Visitor_FieldSet__Platform_GetWindowWorkAreaInsets(void* self, FDelegate_Platform_GetWindowWorkAreaInsets value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static FDelegate_Platform_CreateVkSurface TitanImGui_ImGuiPlatformIO_Visitor_FieldGet__Platform_CreateVkSurface(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImGuiPlatformIO_Visitor_FieldSet__Platform_CreateVkSurface(void* self, FDelegate_Platform_CreateVkSurface value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static FDelegate_Renderer_CreateWindow TitanImGui_ImGuiPlatformIO_Visitor_FieldGet__Renderer_CreateWindow(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImGuiPlatformIO_Visitor_FieldSet__Renderer_CreateWindow(void* self, FDelegate_Renderer_CreateWindow value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static FDelegate_Renderer_DestroyWindow TitanImGui_ImGuiPlatformIO_Visitor_FieldGet__Renderer_DestroyWindow(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImGuiPlatformIO_Visitor_FieldSet__Renderer_DestroyWindow(void* self, FDelegate_Renderer_DestroyWindow value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static FDelegate_Renderer_SetWindowSize TitanImGui_ImGuiPlatformIO_Visitor_FieldGet__Renderer_SetWindowSize(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImGuiPlatformIO_Visitor_FieldSet__Renderer_SetWindowSize(void* self, FDelegate_Renderer_SetWindowSize value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static FDelegate_Renderer_RenderWindow TitanImGui_ImGuiPlatformIO_Visitor_FieldGet__Renderer_RenderWindow(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImGuiPlatformIO_Visitor_FieldSet__Renderer_RenderWindow(void* self, FDelegate_Renderer_RenderWindow value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static FDelegate_Renderer_SwapBuffers TitanImGui_ImGuiPlatformIO_Visitor_FieldGet__Renderer_SwapBuffers(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImGuiPlatformIO_Visitor_FieldSet__Renderer_SwapBuffers(void* self, FDelegate_Renderer_SwapBuffers value);
	//Functions
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	extern static void TitanImGui_ImGuiPlatformIO_Visitor_ClearPlatformHandlers_2960189489(void* Self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	extern static void TitanImGui_ImGuiPlatformIO_Visitor_ClearRendererHandlers_2960189489(void* Self);
	//Cast
	#endregion
}
