//generated from legacy CppWeaving ImGui surface; native entrypoints routed through TitanImGuiBridge
using System;
using System.Runtime.InteropServices;
#if DEBUG
	using SuppressGC = EngineNS.Rtti.TtDummyAttribute;
#else
	using SuppressGC = System.Runtime.InteropServices.SuppressGCTransitionAttribute;
#endif


[System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Explicit, Size = 64, Pack = 8)]
public unsafe partial struct ImGuiPayload : EngineNS.IPtrType
{
	#region StructLayout
	[System.Runtime.InteropServices.FieldOffset(0)]
	public void* m_Data;
	[System.Runtime.InteropServices.FieldOffset(8)]
	public int m_DataSize;
	[System.Runtime.InteropServices.FieldOffset(12)]
	public uint m_SourceId;
	[System.Runtime.InteropServices.FieldOffset(16)]
	public uint m_SourceParentId;
	[System.Runtime.InteropServices.FieldOffset(20)]
	public int m_DataFrameCount;
	[System.Runtime.InteropServices.FieldOffset(24)]
	public sbyte* m_DataType;
	[System.Runtime.InteropServices.FieldOffset(57)]
	public bool m_Preview;
	[System.Runtime.InteropServices.FieldOffset(58)]
	public bool m_Delivery;
	#endregion
	public IntPtr NativePointer { get => IntPtr.Zero; set {} }
	#region Constructor&Cast
	public void UnsafeCallConstructor()
	{
		fixed (ImGuiPayload* mPointer = &this)
		{
			TitanImGui_ImGuiPayload_Visitor_UnsafeCallConstructor_2960189489(mPointer);
		}
	}
	public void UnsafeCallDestructor()
	{
		fixed (ImGuiPayload* mPointer = &this)
		{
			TitanImGui_ImGuiPayload_Visitor_UnsafeCallDestructor(mPointer);
		}
	}
	#endregion
	#region Fields
	public void* Data
	{
		get
		{
			fixed (ImGuiPayload* mPointer = &this)
			{
				return TitanImGui_ImGuiPayload_Visitor_FieldGet__Data(mPointer);
			}
		}
		set
		{
			fixed (ImGuiPayload* mPointer = &this)
			{
				TitanImGui_ImGuiPayload_Visitor_FieldSet__Data(mPointer, value);
			}
		}
	}
	public int DataSize
	{
		get
		{
			fixed (ImGuiPayload* mPointer = &this)
			{
				return TitanImGui_ImGuiPayload_Visitor_FieldGet__DataSize(mPointer);
			}
		}
		set
		{
			fixed (ImGuiPayload* mPointer = &this)
			{
				TitanImGui_ImGuiPayload_Visitor_FieldSet__DataSize(mPointer, value);
			}
		}
	}
	public uint SourceId
	{
		get
		{
			fixed (ImGuiPayload* mPointer = &this)
			{
				return TitanImGui_ImGuiPayload_Visitor_FieldGet__SourceId(mPointer);
			}
		}
		set
		{
			fixed (ImGuiPayload* mPointer = &this)
			{
				TitanImGui_ImGuiPayload_Visitor_FieldSet__SourceId(mPointer, value);
			}
		}
	}
	public uint SourceParentId
	{
		get
		{
			fixed (ImGuiPayload* mPointer = &this)
			{
				return TitanImGui_ImGuiPayload_Visitor_FieldGet__SourceParentId(mPointer);
			}
		}
		set
		{
			fixed (ImGuiPayload* mPointer = &this)
			{
				TitanImGui_ImGuiPayload_Visitor_FieldSet__SourceParentId(mPointer, value);
			}
		}
	}
	public int DataFrameCount
	{
		get
		{
			fixed (ImGuiPayload* mPointer = &this)
			{
				return TitanImGui_ImGuiPayload_Visitor_FieldGet__DataFrameCount(mPointer);
			}
		}
		set
		{
			fixed (ImGuiPayload* mPointer = &this)
			{
				TitanImGui_ImGuiPayload_Visitor_FieldSet__DataFrameCount(mPointer, value);
			}
		}
	}
	public sbyte* DataType
	{
		get
		{
			fixed (ImGuiPayload* mPointer = &this)
			{
				return TitanImGui_ImGuiPayload_Visitor_FieldGet__DataType(mPointer);
			}
		}
		set
		{
			fixed (ImGuiPayload* mPointer = &this)
			{
				TitanImGui_ImGuiPayload_Visitor_FieldSet__DataType(mPointer, value);
			}
		}
	}
	public bool Preview
	{
		get
		{
			fixed (ImGuiPayload* mPointer = &this)
			{
				return TitanImGui_ImGuiPayload_Visitor_FieldGet__Preview(mPointer) == 0 ? false : true;
			}
		}
		set
		{
			fixed (ImGuiPayload* mPointer = &this)
			{
				TitanImGui_ImGuiPayload_Visitor_FieldSet__Preview(mPointer, value);
			}
		}
	}
	public bool Delivery
	{
		get
		{
			fixed (ImGuiPayload* mPointer = &this)
			{
				return TitanImGui_ImGuiPayload_Visitor_FieldGet__Delivery(mPointer) == 0 ? false : true;
			}
		}
		set
		{
			fixed (ImGuiPayload* mPointer = &this)
			{
				TitanImGui_ImGuiPayload_Visitor_FieldSet__Delivery(mPointer, value);
			}
		}
	}
	#endregion
	#region Function
	public void Clear()
	{
		fixed (ImGuiPayload* mPointer = &this)
		{
			TitanImGui_ImGuiPayload_Visitor_Clear_2960189489(mPointer);
		}
	}
	public bool IsDataType(string type)
	{
		fixed (ImGuiPayload* mPointer = &this)
		{
			return TitanImGui_ImGuiPayload_Visitor_IsDataType_1209308473(mPointer, type) == 0 ? false : true;
		}
	}
	public bool IsPreview()
	{
		fixed (ImGuiPayload* mPointer = &this)
		{
			return TitanImGui_ImGuiPayload_Visitor_IsPreview_82051314(mPointer) == 0 ? false : true;
		}
	}
	public bool IsDelivery()
	{
		fixed (ImGuiPayload* mPointer = &this)
		{
			return TitanImGui_ImGuiPayload_Visitor_IsDelivery_82051314(mPointer) == 0 ? false : true;
		}
	}
	#endregion
	#region Core SDK
	const string ModuleNC = EngineNS.CoreSDK.CoreModule;
	//Constructor&Cast
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	extern static void TitanImGui_ImGuiPayload_Visitor_UnsafeCallConstructor_2960189489(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	extern static void TitanImGui_ImGuiPayload_Visitor_UnsafeCallDestructor(void* self);
	//Fields
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void* TitanImGui_ImGuiPayload_Visitor_FieldGet__Data(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImGuiPayload_Visitor_FieldSet__Data(void* self, void* value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static int TitanImGui_ImGuiPayload_Visitor_FieldGet__DataSize(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImGuiPayload_Visitor_FieldSet__DataSize(void* self, int value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static uint TitanImGui_ImGuiPayload_Visitor_FieldGet__SourceId(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImGuiPayload_Visitor_FieldSet__SourceId(void* self, uint value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static uint TitanImGui_ImGuiPayload_Visitor_FieldGet__SourceParentId(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImGuiPayload_Visitor_FieldSet__SourceParentId(void* self, uint value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static int TitanImGui_ImGuiPayload_Visitor_FieldGet__DataFrameCount(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImGuiPayload_Visitor_FieldSet__DataFrameCount(void* self, int value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static sbyte* TitanImGui_ImGuiPayload_Visitor_FieldGet__DataType(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImGuiPayload_Visitor_FieldSet__DataType(void* self, sbyte* value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static sbyte TitanImGui_ImGuiPayload_Visitor_FieldGet__Preview(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImGuiPayload_Visitor_FieldSet__Preview(void* self, bool value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static sbyte TitanImGui_ImGuiPayload_Visitor_FieldGet__Delivery(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImGuiPayload_Visitor_FieldSet__Delivery(void* self, bool value);
	//Functions
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	extern static void TitanImGui_ImGuiPayload_Visitor_Clear_2960189489(void* Self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	extern static sbyte TitanImGui_ImGuiPayload_Visitor_IsDataType_1209308473(void* Self, [MarshalAs(UnmanagedType.LPUTF8Str)] string type);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	extern static sbyte TitanImGui_ImGuiPayload_Visitor_IsPreview_82051314(void* Self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	extern static sbyte TitanImGui_ImGuiPayload_Visitor_IsDelivery_82051314(void* Self);
	//Cast
	#endregion
}
