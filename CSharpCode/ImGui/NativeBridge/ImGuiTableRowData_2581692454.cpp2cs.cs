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
	[System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Explicit, Size = 56, Pack = 8)]
	public unsafe partial struct ImGuiTableRowData : EngineNS.IPtrType
	{
		#region StructLayout
		[System.Runtime.InteropServices.FieldOffset(0)]
		public ulong m_IndentTextureId;
		[System.Runtime.InteropServices.FieldOffset(8)]
		public float m_MinHeight;
		[System.Runtime.InteropServices.FieldOffset(12)]
		public float m_CellPaddingYEnd;
		[System.Runtime.InteropServices.FieldOffset(16)]
		public float m_CellPaddingYBegin;
		[System.Runtime.InteropServices.FieldOffset(20)]
		public float m_IndentImageWidth;
		[System.Runtime.InteropServices.FieldOffset(24)]
		public EngineNS.Vector2 m_IndentTextureUVMin;
		[System.Runtime.InteropServices.FieldOffset(32)]
		public EngineNS.Vector2 m_IndentTextureUVMax;
		[System.Runtime.InteropServices.FieldOffset(40)]
		public uint m_IndentColor;
		[System.Runtime.InteropServices.FieldOffset(44)]
		public uint m_HoverColor;
		[System.Runtime.InteropServices.FieldOffset(48)]
		public ImGuiTableRowFlags_ m_Flags;
		#endregion
		public IntPtr NativePointer { get => IntPtr.Zero; set {} }
		#region Constructor&Cast
		public void UnsafeCallConstructor()
		{
			fixed (ImGuiTableRowData* mPointer = &this)
			{
				TitanImGui_ImGuiTableRowData_Visitor_UnsafeCallConstructor_2960189489(mPointer);
			}
		}
		public void UnsafeCallDestructor()
		{
			fixed (ImGuiTableRowData* mPointer = &this)
			{
				TitanImGui_ImGuiTableRowData_Visitor_UnsafeCallDestructor(mPointer);
			}
		}
		#endregion
		#region Fields
		public ulong IndentTextureId
		{
			get
			{
				fixed (ImGuiTableRowData* mPointer = &this)
				{
					return TitanImGui_ImGuiTableRowData_Visitor_FieldGet__IndentTextureId(mPointer);
				}
			}
			set
			{
				fixed (ImGuiTableRowData* mPointer = &this)
				{
					TitanImGui_ImGuiTableRowData_Visitor_FieldSet__IndentTextureId(mPointer, value);
				}
			}
		}
		public float MinHeight
		{
			get
			{
				fixed (ImGuiTableRowData* mPointer = &this)
				{
					return TitanImGui_ImGuiTableRowData_Visitor_FieldGet__MinHeight(mPointer);
				}
			}
			set
			{
				fixed (ImGuiTableRowData* mPointer = &this)
				{
					TitanImGui_ImGuiTableRowData_Visitor_FieldSet__MinHeight(mPointer, value);
				}
			}
		}
		public float CellPaddingYEnd
		{
			get
			{
				fixed (ImGuiTableRowData* mPointer = &this)
				{
					return TitanImGui_ImGuiTableRowData_Visitor_FieldGet__CellPaddingYEnd(mPointer);
				}
			}
			set
			{
				fixed (ImGuiTableRowData* mPointer = &this)
				{
					TitanImGui_ImGuiTableRowData_Visitor_FieldSet__CellPaddingYEnd(mPointer, value);
				}
			}
		}
		public float CellPaddingYBegin
		{
			get
			{
				fixed (ImGuiTableRowData* mPointer = &this)
				{
					return TitanImGui_ImGuiTableRowData_Visitor_FieldGet__CellPaddingYBegin(mPointer);
				}
			}
			set
			{
				fixed (ImGuiTableRowData* mPointer = &this)
				{
					TitanImGui_ImGuiTableRowData_Visitor_FieldSet__CellPaddingYBegin(mPointer, value);
				}
			}
		}
		public float IndentImageWidth
		{
			get
			{
				fixed (ImGuiTableRowData* mPointer = &this)
				{
					return TitanImGui_ImGuiTableRowData_Visitor_FieldGet__IndentImageWidth(mPointer);
				}
			}
			set
			{
				fixed (ImGuiTableRowData* mPointer = &this)
				{
					TitanImGui_ImGuiTableRowData_Visitor_FieldSet__IndentImageWidth(mPointer, value);
				}
			}
		}
		public EngineNS.Vector2 IndentTextureUVMin
		{
			get
			{
				fixed (ImGuiTableRowData* mPointer = &this)
				{
					return TitanImGui_ImGuiTableRowData_Visitor_FieldGet__IndentTextureUVMin(mPointer);
				}
			}
			set
			{
				fixed (ImGuiTableRowData* mPointer = &this)
				{
					TitanImGui_ImGuiTableRowData_Visitor_FieldSet__IndentTextureUVMin(mPointer, value);
				}
			}
		}
		public EngineNS.Vector2 IndentTextureUVMax
		{
			get
			{
				fixed (ImGuiTableRowData* mPointer = &this)
				{
					return TitanImGui_ImGuiTableRowData_Visitor_FieldGet__IndentTextureUVMax(mPointer);
				}
			}
			set
			{
				fixed (ImGuiTableRowData* mPointer = &this)
				{
					TitanImGui_ImGuiTableRowData_Visitor_FieldSet__IndentTextureUVMax(mPointer, value);
				}
			}
		}
		public uint IndentColor
		{
			get
			{
				fixed (ImGuiTableRowData* mPointer = &this)
				{
					return TitanImGui_ImGuiTableRowData_Visitor_FieldGet__IndentColor(mPointer);
				}
			}
			set
			{
				fixed (ImGuiTableRowData* mPointer = &this)
				{
					TitanImGui_ImGuiTableRowData_Visitor_FieldSet__IndentColor(mPointer, value);
				}
			}
		}
		public uint HoverColor
		{
			get
			{
				fixed (ImGuiTableRowData* mPointer = &this)
				{
					return TitanImGui_ImGuiTableRowData_Visitor_FieldGet__HoverColor(mPointer);
				}
			}
			set
			{
				fixed (ImGuiTableRowData* mPointer = &this)
				{
					TitanImGui_ImGuiTableRowData_Visitor_FieldSet__HoverColor(mPointer, value);
				}
			}
		}
		public ImGuiTableRowFlags_ Flags
		{
			get
			{
				fixed (ImGuiTableRowData* mPointer = &this)
				{
					return TitanImGui_ImGuiTableRowData_Visitor_FieldGet__Flags(mPointer);
				}
			}
			set
			{
				fixed (ImGuiTableRowData* mPointer = &this)
				{
					TitanImGui_ImGuiTableRowData_Visitor_FieldSet__Flags(mPointer, value);
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
		extern static void TitanImGui_ImGuiTableRowData_Visitor_UnsafeCallConstructor_2960189489(void* self);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static void TitanImGui_ImGuiTableRowData_Visitor_UnsafeCallDestructor(void* self);
		//Fields
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		[SuppressGC]
		extern static ulong TitanImGui_ImGuiTableRowData_Visitor_FieldGet__IndentTextureId(void* self);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		[SuppressGC]
		extern static void TitanImGui_ImGuiTableRowData_Visitor_FieldSet__IndentTextureId(void* self, ulong value);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		[SuppressGC]
		extern static float TitanImGui_ImGuiTableRowData_Visitor_FieldGet__MinHeight(void* self);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		[SuppressGC]
		extern static void TitanImGui_ImGuiTableRowData_Visitor_FieldSet__MinHeight(void* self, float value);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		[SuppressGC]
		extern static float TitanImGui_ImGuiTableRowData_Visitor_FieldGet__CellPaddingYEnd(void* self);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		[SuppressGC]
		extern static void TitanImGui_ImGuiTableRowData_Visitor_FieldSet__CellPaddingYEnd(void* self, float value);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		[SuppressGC]
		extern static float TitanImGui_ImGuiTableRowData_Visitor_FieldGet__CellPaddingYBegin(void* self);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		[SuppressGC]
		extern static void TitanImGui_ImGuiTableRowData_Visitor_FieldSet__CellPaddingYBegin(void* self, float value);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		[SuppressGC]
		extern static float TitanImGui_ImGuiTableRowData_Visitor_FieldGet__IndentImageWidth(void* self);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		[SuppressGC]
		extern static void TitanImGui_ImGuiTableRowData_Visitor_FieldSet__IndentImageWidth(void* self, float value);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		[SuppressGC]
		extern static EngineNS.Vector2 TitanImGui_ImGuiTableRowData_Visitor_FieldGet__IndentTextureUVMin(void* self);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		[SuppressGC]
		extern static void TitanImGui_ImGuiTableRowData_Visitor_FieldSet__IndentTextureUVMin(void* self, EngineNS.Vector2 value);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		[SuppressGC]
		extern static EngineNS.Vector2 TitanImGui_ImGuiTableRowData_Visitor_FieldGet__IndentTextureUVMax(void* self);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		[SuppressGC]
		extern static void TitanImGui_ImGuiTableRowData_Visitor_FieldSet__IndentTextureUVMax(void* self, EngineNS.Vector2 value);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		[SuppressGC]
		extern static uint TitanImGui_ImGuiTableRowData_Visitor_FieldGet__IndentColor(void* self);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		[SuppressGC]
		extern static void TitanImGui_ImGuiTableRowData_Visitor_FieldSet__IndentColor(void* self, uint value);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		[SuppressGC]
		extern static uint TitanImGui_ImGuiTableRowData_Visitor_FieldGet__HoverColor(void* self);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		[SuppressGC]
		extern static void TitanImGui_ImGuiTableRowData_Visitor_FieldSet__HoverColor(void* self, uint value);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		[SuppressGC]
		extern static ImGuiTableRowFlags_ TitanImGui_ImGuiTableRowData_Visitor_FieldGet__Flags(void* self);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		[SuppressGC]
		extern static void TitanImGui_ImGuiTableRowData_Visitor_FieldSet__Flags(void* self, ImGuiTableRowFlags_ value);
		//Functions
		//Cast
		#endregion
	}
}
