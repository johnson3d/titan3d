//generated from legacy CppWeaving ImGui surface; native entrypoints routed through TitanImGuiBridge
using System;
using System.Runtime.InteropServices;
#if DEBUG
	using SuppressGC = EngineNS.Rtti.TtDummyAttribute;
#else
	using SuppressGC = System.Runtime.InteropServices.SuppressGCTransitionAttribute;
#endif


[System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Explicit, Size = 72, Pack = 8)]
public unsafe partial struct ImDrawCmd : EngineNS.IPtrType
{
	#region StructLayout
	[System.Runtime.InteropServices.FieldOffset(0)]
	public EngineNS.Vector4 m_ClipRect;
	[System.Runtime.InteropServices.FieldOffset(16)]
	public ImTextureRef m_TexRef;
	[System.Runtime.InteropServices.FieldOffset(32)]
	public uint m_VtxOffset;
	[System.Runtime.InteropServices.FieldOffset(36)]
	public uint m_IdxOffset;
	[System.Runtime.InteropServices.FieldOffset(40)]
	public uint m_ElemCount;
	[System.Runtime.InteropServices.FieldOffset(48)]
	public IntPtr m_UserCallback;
	[System.Runtime.InteropServices.FieldOffset(56)]
	public void* m_UserCallbackData;
	[System.Runtime.InteropServices.FieldOffset(64)]
	public int m_UserCallbackDataSize;
	[System.Runtime.InteropServices.FieldOffset(68)]
	public int m_UserCallbackDataOffset;
	#endregion
	public IntPtr NativePointer { get => IntPtr.Zero; set {} }
	#region Constructor&Cast
	public void UnsafeCallConstructor()
	{
		fixed (ImDrawCmd* mPointer = &this)
		{
			TitanImGui_ImDrawCmd_Visitor_UnsafeCallConstructor_2960189489(mPointer);
		}
	}
	public void UnsafeCallDestructor()
	{
		fixed (ImDrawCmd* mPointer = &this)
		{
			TitanImGui_ImDrawCmd_Visitor_UnsafeCallDestructor(mPointer);
		}
	}
	#endregion
	#region Fields
	public EngineNS.Vector4 ClipRect
	{
		get
		{
			fixed (ImDrawCmd* mPointer = &this)
			{
				return TitanImGui_ImDrawCmd_Visitor_FieldGet__ClipRect(mPointer);
			}
		}
		set
		{
			fixed (ImDrawCmd* mPointer = &this)
			{
				TitanImGui_ImDrawCmd_Visitor_FieldSet__ClipRect(mPointer, value);
			}
		}
	}
	public ImTextureRef TexRef
	{
		get
		{
			fixed (ImDrawCmd* mPointer = &this)
			{
				return TitanImGui_ImDrawCmd_Visitor_FieldGet__TexRef(mPointer);
			}
		}
		set
		{
			fixed (ImDrawCmd* mPointer = &this)
			{
				TitanImGui_ImDrawCmd_Visitor_FieldSet__TexRef(mPointer, value);
			}
		}
	}
	public uint VtxOffset
	{
		get
		{
			fixed (ImDrawCmd* mPointer = &this)
			{
				return TitanImGui_ImDrawCmd_Visitor_FieldGet__VtxOffset(mPointer);
			}
		}
		set
		{
			fixed (ImDrawCmd* mPointer = &this)
			{
				TitanImGui_ImDrawCmd_Visitor_FieldSet__VtxOffset(mPointer, value);
			}
		}
	}
	public uint IdxOffset
	{
		get
		{
			fixed (ImDrawCmd* mPointer = &this)
			{
				return TitanImGui_ImDrawCmd_Visitor_FieldGet__IdxOffset(mPointer);
			}
		}
		set
		{
			fixed (ImDrawCmd* mPointer = &this)
			{
				TitanImGui_ImDrawCmd_Visitor_FieldSet__IdxOffset(mPointer, value);
			}
		}
	}
	public uint ElemCount
	{
		get
		{
			fixed (ImDrawCmd* mPointer = &this)
			{
				return TitanImGui_ImDrawCmd_Visitor_FieldGet__ElemCount(mPointer);
			}
		}
		set
		{
			fixed (ImDrawCmd* mPointer = &this)
			{
				TitanImGui_ImDrawCmd_Visitor_FieldSet__ElemCount(mPointer, value);
			}
		}
	}
	public unsafe delegate void FDelegate_UserCallback(ImDrawList arg0,ImDrawCmd* arg1);
	public FDelegate_UserCallback UserCallback
	{
		get
		{
			fixed (ImDrawCmd* mPointer = &this)
			{
				return TitanImGui_ImDrawCmd_Visitor_FieldGet__UserCallback(mPointer);
			}
		}
		set
		{
			fixed (ImDrawCmd* mPointer = &this)
			{
				TitanImGui_ImDrawCmd_Visitor_FieldSet__UserCallback(mPointer, value);
			}
		}
	}
	public void* UserCallbackData
	{
		get
		{
			fixed (ImDrawCmd* mPointer = &this)
			{
				return TitanImGui_ImDrawCmd_Visitor_FieldGet__UserCallbackData(mPointer);
			}
		}
		set
		{
			fixed (ImDrawCmd* mPointer = &this)
			{
				TitanImGui_ImDrawCmd_Visitor_FieldSet__UserCallbackData(mPointer, value);
			}
		}
	}
	public int UserCallbackDataSize
	{
		get
		{
			fixed (ImDrawCmd* mPointer = &this)
			{
				return TitanImGui_ImDrawCmd_Visitor_FieldGet__UserCallbackDataSize(mPointer);
			}
		}
		set
		{
			fixed (ImDrawCmd* mPointer = &this)
			{
				TitanImGui_ImDrawCmd_Visitor_FieldSet__UserCallbackDataSize(mPointer, value);
			}
		}
	}
	public int UserCallbackDataOffset
	{
		get
		{
			fixed (ImDrawCmd* mPointer = &this)
			{
				return TitanImGui_ImDrawCmd_Visitor_FieldGet__UserCallbackDataOffset(mPointer);
			}
		}
		set
		{
			fixed (ImDrawCmd* mPointer = &this)
			{
				TitanImGui_ImDrawCmd_Visitor_FieldSet__UserCallbackDataOffset(mPointer, value);
			}
		}
	}
	#endregion
	#region Function
	public ulong GetTexID()
	{
		fixed (ImDrawCmd* mPointer = &this)
		{
			return TitanImGui_ImDrawCmd_Visitor_GetTexID_2645814042(mPointer);
		}
	}
	#endregion
	#region Core SDK
	const string ModuleNC = EngineNS.CoreSDK.CoreModule;
	//Constructor&Cast
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	extern static void TitanImGui_ImDrawCmd_Visitor_UnsafeCallConstructor_2960189489(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	extern static void TitanImGui_ImDrawCmd_Visitor_UnsafeCallDestructor(void* self);
	//Fields
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static EngineNS.Vector4 TitanImGui_ImDrawCmd_Visitor_FieldGet__ClipRect(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImDrawCmd_Visitor_FieldSet__ClipRect(void* self, EngineNS.Vector4 value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static ImTextureRef TitanImGui_ImDrawCmd_Visitor_FieldGet__TexRef(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImDrawCmd_Visitor_FieldSet__TexRef(void* self, ImTextureRef value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static uint TitanImGui_ImDrawCmd_Visitor_FieldGet__VtxOffset(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImDrawCmd_Visitor_FieldSet__VtxOffset(void* self, uint value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static uint TitanImGui_ImDrawCmd_Visitor_FieldGet__IdxOffset(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImDrawCmd_Visitor_FieldSet__IdxOffset(void* self, uint value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static uint TitanImGui_ImDrawCmd_Visitor_FieldGet__ElemCount(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImDrawCmd_Visitor_FieldSet__ElemCount(void* self, uint value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static FDelegate_UserCallback TitanImGui_ImDrawCmd_Visitor_FieldGet__UserCallback(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImDrawCmd_Visitor_FieldSet__UserCallback(void* self, FDelegate_UserCallback value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void* TitanImGui_ImDrawCmd_Visitor_FieldGet__UserCallbackData(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImDrawCmd_Visitor_FieldSet__UserCallbackData(void* self, void* value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static int TitanImGui_ImDrawCmd_Visitor_FieldGet__UserCallbackDataSize(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImDrawCmd_Visitor_FieldSet__UserCallbackDataSize(void* self, int value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static int TitanImGui_ImDrawCmd_Visitor_FieldGet__UserCallbackDataOffset(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImDrawCmd_Visitor_FieldSet__UserCallbackDataOffset(void* self, int value);
	//Functions
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	extern static ulong TitanImGui_ImDrawCmd_Visitor_GetTexID_2645814042(void* Self);
	//Cast
	#endregion
}
