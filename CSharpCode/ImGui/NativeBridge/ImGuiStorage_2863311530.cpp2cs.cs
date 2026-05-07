//generated from legacy CppWeaving ImGui surface; native entrypoints routed through TitanImGuiBridge
using System;
using System.Runtime.InteropServices;
#if DEBUG
	using SuppressGC = EngineNS.Rtti.TtDummyAttribute;
#else
	using SuppressGC = System.Runtime.InteropServices.SuppressGCTransitionAttribute;
#endif


public unsafe partial struct ImGuiStorage : EngineNS.IPtrType
{
	[System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Explicit, Size = 16, Pack = 8)]
	public struct CppStructLayout
	{
	}
	private void* mPointer;
	public CppStructLayout* UnsafeAsLayout { get => (CppStructLayout*)mPointer; }
	public ImGuiStorage(void* p) { mPointer = p; }
	public void UnsafeSetPointer(void* p) { mPointer = p; }
	public IntPtr NativePointer { get => (IntPtr)mPointer; set => mPointer = value.ToPointer(); }
	public ImGuiStorage* CppPointer { get => (ImGuiStorage*)mPointer; }
	public bool IsValidPointer { get => mPointer != (void*)0; }
	public static implicit operator ImGuiStorage* (ImGuiStorage v)
	{
		return (ImGuiStorage*)v.mPointer;
	}
	#region Constructor&Cast
	public static EngineNS.FRttiStruct GetTypeRtti()
	{
		return new EngineNS.FRttiStruct(TitanImGui_ImGuiStorage_Visitor_GetTypeRtti());
	}
	#endregion
	#region Fields
	#endregion
	#region Function
	public void Clear()
	{
		TitanImGui_ImGuiStorage_Visitor_Clear_2960189489(mPointer);
	}
	public int GetInt(uint key,int default_val)
	{
		return TitanImGui_ImGuiStorage_Visitor_GetInt_1250669178(mPointer, key, default_val);
	}
	public void SetInt(uint key,int val)
	{
		TitanImGui_ImGuiStorage_Visitor_SetInt_3129034236(mPointer, key, val);
	}
	public bool GetBool(uint key,bool default_val)
	{
		return TitanImGui_ImGuiStorage_Visitor_GetBool_2657485946(mPointer, key, default_val) == 0 ? false : true;
	}
	public void SetBool(uint key,bool val)
	{
		TitanImGui_ImGuiStorage_Visitor_SetBool_1053065717(mPointer, key, val);
	}
	public float GetFloat(uint key,float default_val)
	{
		return TitanImGui_ImGuiStorage_Visitor_GetFloat_3416669274(mPointer, key, default_val);
	}
	public void SetFloat(uint key,float val)
	{
		TitanImGui_ImGuiStorage_Visitor_SetFloat_3128544667(mPointer, key, val);
	}
	public void* GetVoidPtr(uint key)
	{
		return TitanImGui_ImGuiStorage_Visitor_GetVoidPtr_2257549434(mPointer, key);
	}
	public void SetVoidPtr(uint key,void* val)
	{
		TitanImGui_ImGuiStorage_Visitor_SetVoidPtr_3144869721(mPointer, key, val);
	}
	public int* GetIntRef(uint key,int default_val)
	{
		return TitanImGui_ImGuiStorage_Visitor_GetIntRef_1484994661(mPointer, key, default_val);
	}
	public bool* GetBoolRef(uint key,bool default_val)
	{
		return TitanImGui_ImGuiStorage_Visitor_GetBoolRef_1816378645(mPointer, key, default_val);
	}
	public float* GetFloatRef(uint key,float default_val)
	{
		return TitanImGui_ImGuiStorage_Visitor_GetFloatRef_26904213(mPointer, key, default_val);
	}
	public void** GetVoidPtrRef(uint key,void* default_val)
	{
		return TitanImGui_ImGuiStorage_Visitor_GetVoidPtrRef_949922101(mPointer, key, default_val);
	}
	public void BuildSortByKey()
	{
		TitanImGui_ImGuiStorage_Visitor_BuildSortByKey_2960189489(mPointer);
	}
	public void SetAllInt(int val)
	{
		TitanImGui_ImGuiStorage_Visitor_SetAllInt_4038704236(mPointer, val);
	}
	#endregion
	#region Core SDK
	const string ModuleNC = EngineNS.CoreSDK.CoreModule;
	//Constructor&Cast
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	extern static EngineNS.FRttiStruct* TitanImGui_ImGuiStorage_Visitor_GetTypeRtti();
	//Fields
	//Functions
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	extern static void TitanImGui_ImGuiStorage_Visitor_Clear_2960189489(void* Self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	extern static int TitanImGui_ImGuiStorage_Visitor_GetInt_1250669178(void* Self, uint key,int default_val);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	extern static void TitanImGui_ImGuiStorage_Visitor_SetInt_3129034236(void* Self, uint key,int val);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	extern static sbyte TitanImGui_ImGuiStorage_Visitor_GetBool_2657485946(void* Self, uint key,bool default_val);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	extern static void TitanImGui_ImGuiStorage_Visitor_SetBool_1053065717(void* Self, uint key,bool val);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	extern static float TitanImGui_ImGuiStorage_Visitor_GetFloat_3416669274(void* Self, uint key,float default_val);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	extern static void TitanImGui_ImGuiStorage_Visitor_SetFloat_3128544667(void* Self, uint key,float val);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	extern static void* TitanImGui_ImGuiStorage_Visitor_GetVoidPtr_2257549434(void* Self, uint key);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	extern static void TitanImGui_ImGuiStorage_Visitor_SetVoidPtr_3144869721(void* Self, uint key,void* val);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	extern static int* TitanImGui_ImGuiStorage_Visitor_GetIntRef_1484994661(void* Self, uint key,int default_val);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	extern static bool* TitanImGui_ImGuiStorage_Visitor_GetBoolRef_1816378645(void* Self, uint key,bool default_val);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	extern static float* TitanImGui_ImGuiStorage_Visitor_GetFloatRef_26904213(void* Self, uint key,float default_val);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	extern static void** TitanImGui_ImGuiStorage_Visitor_GetVoidPtrRef_949922101(void* Self, uint key,void* default_val);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	extern static void TitanImGui_ImGuiStorage_Visitor_BuildSortByKey_2960189489(void* Self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	extern static void TitanImGui_ImGuiStorage_Visitor_SetAllInt_4038704236(void* Self, int val);
	//Cast
	#endregion
}
