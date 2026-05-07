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
	public unsafe partial struct FCodeEditor : EngineNS.IPtrType
	{
		[System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Explicit, Size = 1528, Pack = 8)]
		public struct CppStructLayout
		{
		}
		private void* mPointer;
		public CppStructLayout* UnsafeAsLayout { get => (CppStructLayout*)mPointer; }
		public FCodeEditor(void* p) { mPointer = p; }
		public void UnsafeSetPointer(void* p) { mPointer = p; }
		public IntPtr NativePointer { get => (IntPtr)mPointer; set => mPointer = value.ToPointer(); }
		public FCodeEditor* CppPointer { get => (FCodeEditor*)mPointer; }
		public bool IsValidPointer { get => mPointer != (void*)0; }
		public static implicit operator FCodeEditor* (FCodeEditor v)
		{
			return (FCodeEditor*)v.mPointer;
		}
		#region Constructor&Cast
		public static EngineNS.FRttiStruct GetTypeRtti()
		{
			return new EngineNS.FRttiStruct(TitanImGui_FCodeEditor_Visitor_GetTypeRtti());
		}
		public static EngineNS.FCodeEditor CreateInstance()
		{
			return new EngineNS.FCodeEditor(TitanImGui_FCodeEditor_Visitor_CreateInstance_2960189489());
		}
		public EngineNS.VIUnknown CastSuper()
		{
			return new EngineNS.VIUnknown(TitanImGui_FCodeEditor_Visitor_CastTo_EngineNS_VIUnknown(mPointer));
		}
		public EngineNS.VIUnknown NativeSuper
		{
			get { return CastSuper(); }
		}
		#endregion
		#region Fields
		#endregion
		#region Function
		public void SetLanguage(string type)
		{
			TitanImGui_FCodeEditor_Visitor_SetLanguage_2602414842(mPointer, type);
		}
		public void PushPreprocIdentifier(string name,string value)
		{
			TitanImGui_FCodeEditor_Visitor_PushPreprocIdentifier_568371421(mPointer, name, value);
		}
		public void PushIdentifier(string name,string value)
		{
			TitanImGui_FCodeEditor_Visitor_PushIdentifier_568371421(mPointer, name, value);
		}
		public void ApplyLangDefine()
		{
			TitanImGui_FCodeEditor_Visitor_ApplyLangDefine_2960189489(mPointer);
		}
		public void PushErrorMarker(int index,string info)
		{
			TitanImGui_FCodeEditor_Visitor_PushErrorMarker_85734681(mPointer, index, info);
		}
		public void ApplyErrorMarkers()
		{
			TitanImGui_FCodeEditor_Visitor_ApplyErrorMarkers_2960189489(mPointer);
		}
		public void Render(string aTitle,EngineNS.Vector2* aSize,bool aBorder)
		{
			TitanImGui_FCodeEditor_Visitor_Render_1399928045(mPointer, aTitle, aSize, aBorder);
		}
		public void Render(string aTitle, in EngineNS.Vector2 aSize,bool aBorder)
		{
			fixed(EngineNS.Vector2* pinned_aSize = &aSize)
			{
				Render(aTitle, pinned_aSize, aBorder);
			}
		}
		public void SetText(string aText)
		{
			TitanImGui_FCodeEditor_Visitor_SetText_2602414842(mPointer, aText);
		}
		public void GetText(EngineNS.IBlobObject blob)
		{
			TitanImGui_FCodeEditor_Visitor_GetText_2413885944(mPointer, blob);
		}
		public string GetTextPointer()
		{
			return EngineNS.Rtti.TtNativeCoreProvider.MarshalPtrUtf8((IntPtr)TitanImGui_FCodeEditor_Visitor_GetTextPointer_2396230038(mPointer));
		}
		public void Undo()
		{
			TitanImGui_FCodeEditor_Visitor_Undo_2960189489(mPointer);
		}
		public void Redo()
		{
			TitanImGui_FCodeEditor_Visitor_Redo_2960189489(mPointer);
		}
		public void Copy()
		{
			TitanImGui_FCodeEditor_Visitor_Copy_2960189489(mPointer);
		}
		public void Cut()
		{
			TitanImGui_FCodeEditor_Visitor_Cut_2960189489(mPointer);
		}
		public void Delete()
		{
			TitanImGui_FCodeEditor_Visitor_Delete_2960189489(mPointer);
		}
		public void Paste()
		{
			TitanImGui_FCodeEditor_Visitor_Paste_2960189489(mPointer);
		}
		public void SelectAll()
		{
			TitanImGui_FCodeEditor_Visitor_SelectAll_2960189489(mPointer);
		}
		public void SetViewStyle(string view)
		{
			TitanImGui_FCodeEditor_Visitor_SetViewStyle_2602414842(mPointer, view);
		}
		#endregion
		#region Core SDK
		const string ModuleNC = EngineNS.CoreSDK.CoreModule;
		//Constructor&Cast
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static EngineNS.FRttiStruct* TitanImGui_FCodeEditor_Visitor_GetTypeRtti();
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static EngineNS.FCodeEditor* TitanImGui_FCodeEditor_Visitor_CreateInstance_2960189489();
		//Fields
		//Functions
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static void TitanImGui_FCodeEditor_Visitor_SetLanguage_2602414842(void* Self, [MarshalAs(UnmanagedType.LPUTF8Str)] string type);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static void TitanImGui_FCodeEditor_Visitor_PushPreprocIdentifier_568371421(void* Self, [MarshalAs(UnmanagedType.LPUTF8Str)] string name,[MarshalAs(UnmanagedType.LPUTF8Str)] string value);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static void TitanImGui_FCodeEditor_Visitor_PushIdentifier_568371421(void* Self, [MarshalAs(UnmanagedType.LPUTF8Str)] string name,[MarshalAs(UnmanagedType.LPUTF8Str)] string value);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static void TitanImGui_FCodeEditor_Visitor_ApplyLangDefine_2960189489(void* Self);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static void TitanImGui_FCodeEditor_Visitor_PushErrorMarker_85734681(void* Self, int index,[MarshalAs(UnmanagedType.LPUTF8Str)] string info);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static void TitanImGui_FCodeEditor_Visitor_ApplyErrorMarkers_2960189489(void* Self);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static void TitanImGui_FCodeEditor_Visitor_Render_1399928045(void* Self, [MarshalAs(UnmanagedType.LPUTF8Str)] string aTitle,EngineNS.Vector2* aSize,bool aBorder);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static void TitanImGui_FCodeEditor_Visitor_SetText_2602414842(void* Self, [MarshalAs(UnmanagedType.LPUTF8Str)] string aText);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static void TitanImGui_FCodeEditor_Visitor_GetText_2413885944(void* Self, EngineNS.IBlobObject blob);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static sbyte* TitanImGui_FCodeEditor_Visitor_GetTextPointer_2396230038(void* Self);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static void TitanImGui_FCodeEditor_Visitor_Undo_2960189489(void* Self);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static void TitanImGui_FCodeEditor_Visitor_Redo_2960189489(void* Self);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static void TitanImGui_FCodeEditor_Visitor_Copy_2960189489(void* Self);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static void TitanImGui_FCodeEditor_Visitor_Cut_2960189489(void* Self);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static void TitanImGui_FCodeEditor_Visitor_Delete_2960189489(void* Self);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static void TitanImGui_FCodeEditor_Visitor_Paste_2960189489(void* Self);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static void TitanImGui_FCodeEditor_Visitor_SelectAll_2960189489(void* Self);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static void TitanImGui_FCodeEditor_Visitor_SetViewStyle_2602414842(void* Self, [MarshalAs(UnmanagedType.LPUTF8Str)] string view);
		//Cast
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static EngineNS.VIUnknown* TitanImGui_FCodeEditor_Visitor_CastTo_EngineNS_VIUnknown(void* self);
		#endregion
	}
}
