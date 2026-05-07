//generated from legacy CppWeaving ImGui surface; native entrypoints routed through TitanImGuiBridge
using System;
using System.Runtime.InteropServices;
#if DEBUG
	using SuppressGC = EngineNS.Rtti.TtDummyAttribute;
#else
	using SuppressGC = System.Runtime.InteropServices.SuppressGCTransitionAttribute;
#endif


namespace ImGui
{
	public unsafe partial struct ImGuiFileDialog : EngineNS.IPtrType, IDisposable
	{
		[System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Explicit, Size = 200, Pack = 8)]
		public struct CppStructLayout
		{
		}
		private void* mPointer;
		public CppStructLayout* UnsafeAsLayout { get => (CppStructLayout*)mPointer; }
		public ImGuiFileDialog(void* p) { mPointer = p; }
		public void UnsafeSetPointer(void* p) { mPointer = p; }
		public IntPtr NativePointer { get => (IntPtr)mPointer; set => mPointer = value.ToPointer(); }
		public ImGuiFileDialog* CppPointer { get => (ImGuiFileDialog*)mPointer; }
		public bool IsValidPointer { get => mPointer != (void*)0; }
		public static implicit operator ImGuiFileDialog* (ImGuiFileDialog v)
		{
			return (ImGuiFileDialog*)v.mPointer;
		}
		#region Constructor&Cast
		public static EngineNS.FRttiStruct GetTypeRtti()
		{
			return new EngineNS.FRttiStruct(TitanImGui_ImGuiFileDialog_Visitor_GetTypeRtti());
		}
		public static ImGui.ImGuiFileDialog CreateInstance()
		{
			return new ImGui.ImGuiFileDialog(TitanImGui_ImGuiFileDialog_Visitor_CreateInstance_2960189489());
		}
		public void Dispose()
		{
			TitanImGui_ImGuiFileDialog_Visitor_Dispose(mPointer);
		}
		#endregion
		#region Fields
		#endregion
		#region Function
		public void OpenDialog(string vKey,string vTitle,string vFilters,string vPath)
		{
			TitanImGui_ImGuiFileDialog_Visitor_OpenDialog_3146595389(mPointer, vKey, vTitle, vFilters, vPath);
		}
		public void OpenModal(string vKey,string vTitle,string vFilters,string vPath)
		{
			TitanImGui_ImGuiFileDialog_Visitor_OpenModal_3146595389(mPointer, vKey, vTitle, vFilters, vPath);
		}
		public void OpenModalWithMutiSelect(string vKey,string vTitle,string vFilters,string vPath,int vCountSelectionMax)
		{
			TitanImGui_ImGuiFileDialog_Visitor_OpenModalWithMutiSelect_1897522092(mPointer, vKey, vTitle, vFilters, vPath, vCountSelectionMax);
		}
		public bool DisplayDialog(string vKey)
		{
			return TitanImGui_ImGuiFileDialog_Visitor_DisplayDialog_1080422500(mPointer, vKey) == 0 ? false : true;
		}
		public void CloseDialog()
		{
			TitanImGui_ImGuiFileDialog_Visitor_CloseDialog_2960189489(mPointer);
		}
		public bool IsOk()
		{
			return TitanImGui_ImGuiFileDialog_Visitor_IsOk_1117990983(mPointer) == 0 ? false : true;
		}
		public bool WasKeyOpenedThisFrame(string vKey)
		{
			return TitanImGui_ImGuiFileDialog_Visitor_WasKeyOpenedThisFrame_1080422500(mPointer, vKey) == 0 ? false : true;
		}
		public bool WasOpenedThisFrame()
		{
			return TitanImGui_ImGuiFileDialog_Visitor_WasOpenedThisFrame_1117990983(mPointer) == 0 ? false : true;
		}
		public bool IsKeyOpened(string vCurrentOpenedKey)
		{
			return TitanImGui_ImGuiFileDialog_Visitor_IsKeyOpened_1080422500(mPointer, vCurrentOpenedKey) == 0 ? false : true;
		}
		public bool IsOpened()
		{
			return TitanImGui_ImGuiFileDialog_Visitor_IsOpened_1117990983(mPointer) == 0 ? false : true;
		}
		public int GetSelectedCount()
		{
			return TitanImGui_ImGuiFileDialog_Visitor_GetSelectedCount_2704135706(mPointer);
		}
		public string GetFilePathByIndex(int index)
		{
			return EngineNS.Rtti.TtNativeCoreProvider.MarshalPtrUtf8((IntPtr)TitanImGui_ImGuiFileDialog_Visitor_GetFilePathByIndex_4173149781(mPointer, index));
		}
		public string GetFilePathName()
		{
			return EngineNS.Rtti.TtNativeCoreProvider.MarshalPtrUtf8((IntPtr)TitanImGui_ImGuiFileDialog_Visitor_GetFilePathName_2396230038(mPointer));
		}
		public string GetCurrentFileName()
		{
			return EngineNS.Rtti.TtNativeCoreProvider.MarshalPtrUtf8((IntPtr)TitanImGui_ImGuiFileDialog_Visitor_GetCurrentFileName_2396230038(mPointer));
		}
		public string GetCurrentPath()
		{
			return EngineNS.Rtti.TtNativeCoreProvider.MarshalPtrUtf8((IntPtr)TitanImGui_ImGuiFileDialog_Visitor_GetCurrentPath_2396230038(mPointer));
		}
		public string GetCurrentFilter()
		{
			return EngineNS.Rtti.TtNativeCoreProvider.MarshalPtrUtf8((IntPtr)TitanImGui_ImGuiFileDialog_Visitor_GetCurrentFilter_2396230038(mPointer));
		}
		public void* GetUserDatas()
		{
			return TitanImGui_ImGuiFileDialog_Visitor_GetUserDatas_302642963(mPointer);
		}
		public void SetFileStyle(string vFilter,EngineNS.Vector4 vColor,string vIconText)
		{
			TitanImGui_ImGuiFileDialog_Visitor_SetFileStyle_2880679569(mPointer, vFilter, vColor, vIconText);
		}
		public bool GetFileStyle(string vFilter,EngineNS.Vector4* vOutColor,sbyte** vOutIconText)
		{
			return TitanImGui_ImGuiFileDialog_Visitor_GetFileStyle_786079400(mPointer, vFilter, vOutColor, vOutIconText) == 0 ? false : true;
		}
		public bool GetFileStyle(string vFilter, ref EngineNS.Vector4 vOutColor,sbyte** vOutIconText)
		{
			fixed(EngineNS.Vector4* pinned_vOutColor = &vOutColor)
			{
				return GetFileStyle(vFilter, pinned_vOutColor, vOutIconText);
			}
		}
		public void ClearFilesStyle()
		{
			TitanImGui_ImGuiFileDialog_Visitor_ClearFilesStyle_2960189489(mPointer);
		}
		#endregion
		#region Core SDK
		const string ModuleNC = EngineNS.CoreSDK.CoreModule;
		//Constructor&Cast
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static EngineNS.FRttiStruct* TitanImGui_ImGuiFileDialog_Visitor_GetTypeRtti();
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static ImGui.ImGuiFileDialog* TitanImGui_ImGuiFileDialog_Visitor_CreateInstance_2960189489();
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static void TitanImGui_ImGuiFileDialog_Visitor_Dispose(void* self);
		//Fields
		//Functions
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static void TitanImGui_ImGuiFileDialog_Visitor_OpenDialog_3146595389(void* Self, [MarshalAs(UnmanagedType.LPUTF8Str)] string vKey,[MarshalAs(UnmanagedType.LPUTF8Str)] string vTitle,[MarshalAs(UnmanagedType.LPUTF8Str)] string vFilters,[MarshalAs(UnmanagedType.LPUTF8Str)] string vPath);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static void TitanImGui_ImGuiFileDialog_Visitor_OpenModal_3146595389(void* Self, [MarshalAs(UnmanagedType.LPUTF8Str)] string vKey,[MarshalAs(UnmanagedType.LPUTF8Str)] string vTitle,[MarshalAs(UnmanagedType.LPUTF8Str)] string vFilters,[MarshalAs(UnmanagedType.LPUTF8Str)] string vPath);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static void TitanImGui_ImGuiFileDialog_Visitor_OpenModalWithMutiSelect_1897522092(void* Self, [MarshalAs(UnmanagedType.LPUTF8Str)] string vKey,[MarshalAs(UnmanagedType.LPUTF8Str)] string vTitle,[MarshalAs(UnmanagedType.LPUTF8Str)] string vFilters,[MarshalAs(UnmanagedType.LPUTF8Str)] string vPath,int vCountSelectionMax);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static sbyte TitanImGui_ImGuiFileDialog_Visitor_DisplayDialog_1080422500(void* Self, [MarshalAs(UnmanagedType.LPUTF8Str)] string vKey);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static void TitanImGui_ImGuiFileDialog_Visitor_CloseDialog_2960189489(void* Self);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static sbyte TitanImGui_ImGuiFileDialog_Visitor_IsOk_1117990983(void* Self);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static sbyte TitanImGui_ImGuiFileDialog_Visitor_WasKeyOpenedThisFrame_1080422500(void* Self, [MarshalAs(UnmanagedType.LPUTF8Str)] string vKey);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static sbyte TitanImGui_ImGuiFileDialog_Visitor_WasOpenedThisFrame_1117990983(void* Self);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static sbyte TitanImGui_ImGuiFileDialog_Visitor_IsKeyOpened_1080422500(void* Self, [MarshalAs(UnmanagedType.LPUTF8Str)] string vCurrentOpenedKey);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static sbyte TitanImGui_ImGuiFileDialog_Visitor_IsOpened_1117990983(void* Self);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static int TitanImGui_ImGuiFileDialog_Visitor_GetSelectedCount_2704135706(void* Self);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static sbyte* TitanImGui_ImGuiFileDialog_Visitor_GetFilePathByIndex_4173149781(void* Self, int index);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static sbyte* TitanImGui_ImGuiFileDialog_Visitor_GetFilePathName_2396230038(void* Self);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static sbyte* TitanImGui_ImGuiFileDialog_Visitor_GetCurrentFileName_2396230038(void* Self);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static sbyte* TitanImGui_ImGuiFileDialog_Visitor_GetCurrentPath_2396230038(void* Self);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static sbyte* TitanImGui_ImGuiFileDialog_Visitor_GetCurrentFilter_2396230038(void* Self);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static void* TitanImGui_ImGuiFileDialog_Visitor_GetUserDatas_302642963(void* Self);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static void TitanImGui_ImGuiFileDialog_Visitor_SetFileStyle_2880679569(void* Self, [MarshalAs(UnmanagedType.LPUTF8Str)] string vFilter,EngineNS.Vector4 vColor,[MarshalAs(UnmanagedType.LPUTF8Str)] string vIconText);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static sbyte TitanImGui_ImGuiFileDialog_Visitor_GetFileStyle_786079400(void* Self, [MarshalAs(UnmanagedType.LPUTF8Str)] string vFilter,EngineNS.Vector4* vOutColor,sbyte** vOutIconText);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static void TitanImGui_ImGuiFileDialog_Visitor_ClearFilesStyle_2960189489(void* Self);
		//Cast
		#endregion
	}
}
