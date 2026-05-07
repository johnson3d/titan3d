//generated from legacy CppWeaving ImGui surface; native entrypoints routed through TitanImGuiBridge
using System;
using System.Runtime.InteropServices;
#if DEBUG
	using SuppressGC = EngineNS.Rtti.TtDummyAttribute;
#else
	using SuppressGC = System.Runtime.InteropServices.SuppressGCTransitionAttribute;
#endif


[System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Explicit, Size = 16, Pack = 8)]
public unsafe partial struct ImTextureRef : EngineNS.IPtrType
{
	#region StructLayout
	[System.Runtime.InteropServices.FieldOffset(8)]
	public ulong m__TexID;
	#endregion
	public IntPtr NativePointer { get => IntPtr.Zero; set {} }
	#region Constructor&Cast
	public void UnsafeCallConstructor()
	{
		fixed (ImTextureRef* mPointer = &this)
		{
			TitanImGui_ImTextureRef_Visitor_UnsafeCallConstructor_2960189489(mPointer);
		}
	}
	public void UnsafeCallConstructor(ulong tex_id)
	{
		fixed (ImTextureRef* mPointer = &this)
		{
			TitanImGui_ImTextureRef_Visitor_UnsafeCallConstructor_3315747347(mPointer, tex_id);
		}
	}
	public void UnsafeCallConstructor(void* tex_id)
	{
		fixed (ImTextureRef* mPointer = &this)
		{
			TitanImGui_ImTextureRef_Visitor_UnsafeCallConstructor_3034592143(mPointer, tex_id);
		}
	}
	public void UnsafeCallDestructor()
	{
		fixed (ImTextureRef* mPointer = &this)
		{
			TitanImGui_ImTextureRef_Visitor_UnsafeCallDestructor(mPointer);
		}
	}
	#endregion
	#region Fields
	public ulong _TexID
	{
		get
		{
			fixed (ImTextureRef* mPointer = &this)
			{
				return TitanImGui_ImTextureRef_Visitor_FieldGet___TexID(mPointer);
			}
		}
		set
		{
			fixed (ImTextureRef* mPointer = &this)
			{
				TitanImGui_ImTextureRef_Visitor_FieldSet___TexID(mPointer, value);
			}
		}
	}
	#endregion
	#region Function
	public ulong GetTexID()
	{
		fixed (ImTextureRef* mPointer = &this)
		{
			return TitanImGui_ImTextureRef_Visitor_GetTexID_2645814042(mPointer);
		}
	}
	#endregion
	#region Core SDK
	const string ModuleNC = EngineNS.CoreSDK.CoreModule;
	//Constructor&Cast
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	extern static void TitanImGui_ImTextureRef_Visitor_UnsafeCallConstructor_2960189489(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	extern static void TitanImGui_ImTextureRef_Visitor_UnsafeCallConstructor_3315747347(void* self, ulong tex_id);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	extern static void TitanImGui_ImTextureRef_Visitor_UnsafeCallConstructor_3034592143(void* self, void* tex_id);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	extern static void TitanImGui_ImTextureRef_Visitor_UnsafeCallDestructor(void* self);
	//Fields
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static ulong TitanImGui_ImTextureRef_Visitor_FieldGet___TexID(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImTextureRef_Visitor_FieldSet___TexID(void* self, ulong value);
	//Functions
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	extern static ulong TitanImGui_ImTextureRef_Visitor_GetTexID_2645814042(void* Self);
	//Cast
	#endregion
}
