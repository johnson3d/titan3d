//generated from legacy CppWeaving ImGui surface; native entrypoints routed through TitanImGuiBridge
using System;
using System.Runtime.InteropServices;
#if DEBUG
	using SuppressGC = EngineNS.Rtti.TtDummyAttribute;
#else
	using SuppressGC = System.Runtime.InteropServices.SuppressGCTransitionAttribute;
#endif


[System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Explicit, Size = 112, Pack = 8)]
public unsafe partial struct ImGuiViewport : EngineNS.IPtrType
{
	#region StructLayout
	[System.Runtime.InteropServices.FieldOffset(0)]
	public uint m_ID;
	[System.Runtime.InteropServices.FieldOffset(4)]
	public ImGuiViewportFlags_ m_Flags;
	[System.Runtime.InteropServices.FieldOffset(8)]
	public EngineNS.Vector2 m_Pos;
	[System.Runtime.InteropServices.FieldOffset(16)]
	public EngineNS.Vector2 m_Size;
	[System.Runtime.InteropServices.FieldOffset(24)]
	public EngineNS.Vector2 m_FramebufferScale;
	[System.Runtime.InteropServices.FieldOffset(32)]
	public EngineNS.Vector2 m_WorkPos;
	[System.Runtime.InteropServices.FieldOffset(40)]
	public EngineNS.Vector2 m_WorkSize;
	[System.Runtime.InteropServices.FieldOffset(48)]
	public float m_DpiScale;
	[System.Runtime.InteropServices.FieldOffset(52)]
	public uint m_ParentViewportId;
	[System.Runtime.InteropServices.FieldOffset(56)]
	public ImGuiViewport* m_ParentViewport;
	[System.Runtime.InteropServices.FieldOffset(64)]
	public ImDrawData* m_DrawData;
	[System.Runtime.InteropServices.FieldOffset(72)]
	public void* m_RendererUserData;
	[System.Runtime.InteropServices.FieldOffset(80)]
	public void* m_PlatformUserData;
	[System.Runtime.InteropServices.FieldOffset(88)]
	public void* m_PlatformHandle;
	[System.Runtime.InteropServices.FieldOffset(96)]
	public void* m_PlatformHandleRaw;
	[System.Runtime.InteropServices.FieldOffset(104)]
	public bool m_PlatformWindowCreated;
	[System.Runtime.InteropServices.FieldOffset(105)]
	public bool m_PlatformRequestMove;
	[System.Runtime.InteropServices.FieldOffset(106)]
	public bool m_PlatformRequestResize;
	[System.Runtime.InteropServices.FieldOffset(107)]
	public bool m_PlatformRequestClose;
	#endregion
	public IntPtr NativePointer { get => IntPtr.Zero; set {} }
	#region Constructor&Cast
	public void UnsafeCallConstructor()
	{
		fixed (ImGuiViewport* mPointer = &this)
		{
			TitanImGui_ImGuiViewport_Visitor_UnsafeCallConstructor_2960189489(mPointer);
		}
	}
	public void UnsafeCallDestructor()
	{
		fixed (ImGuiViewport* mPointer = &this)
		{
			TitanImGui_ImGuiViewport_Visitor_UnsafeCallDestructor(mPointer);
		}
	}
	#endregion
	#region Fields
	public uint ID
	{
		get
		{
			fixed (ImGuiViewport* mPointer = &this)
			{
				return TitanImGui_ImGuiViewport_Visitor_FieldGet__ID(mPointer);
			}
		}
		set
		{
			fixed (ImGuiViewport* mPointer = &this)
			{
				TitanImGui_ImGuiViewport_Visitor_FieldSet__ID(mPointer, value);
			}
		}
	}
	public ImGuiViewportFlags_ Flags
	{
		get
		{
			fixed (ImGuiViewport* mPointer = &this)
			{
				return TitanImGui_ImGuiViewport_Visitor_FieldGet__Flags(mPointer);
			}
		}
		set
		{
			fixed (ImGuiViewport* mPointer = &this)
			{
				TitanImGui_ImGuiViewport_Visitor_FieldSet__Flags(mPointer, value);
			}
		}
	}
	public EngineNS.Vector2 Pos
	{
		get
		{
			fixed (ImGuiViewport* mPointer = &this)
			{
				return TitanImGui_ImGuiViewport_Visitor_FieldGet__Pos(mPointer);
			}
		}
		set
		{
			fixed (ImGuiViewport* mPointer = &this)
			{
				TitanImGui_ImGuiViewport_Visitor_FieldSet__Pos(mPointer, value);
			}
		}
	}
	public EngineNS.Vector2 Size
	{
		get
		{
			fixed (ImGuiViewport* mPointer = &this)
			{
				return TitanImGui_ImGuiViewport_Visitor_FieldGet__Size(mPointer);
			}
		}
		set
		{
			fixed (ImGuiViewport* mPointer = &this)
			{
				TitanImGui_ImGuiViewport_Visitor_FieldSet__Size(mPointer, value);
			}
		}
	}
	public EngineNS.Vector2 FramebufferScale
	{
		get
		{
			fixed (ImGuiViewport* mPointer = &this)
			{
				return TitanImGui_ImGuiViewport_Visitor_FieldGet__FramebufferScale(mPointer);
			}
		}
		set
		{
			fixed (ImGuiViewport* mPointer = &this)
			{
				TitanImGui_ImGuiViewport_Visitor_FieldSet__FramebufferScale(mPointer, value);
			}
		}
	}
	public EngineNS.Vector2 WorkPos
	{
		get
		{
			fixed (ImGuiViewport* mPointer = &this)
			{
				return TitanImGui_ImGuiViewport_Visitor_FieldGet__WorkPos(mPointer);
			}
		}
		set
		{
			fixed (ImGuiViewport* mPointer = &this)
			{
				TitanImGui_ImGuiViewport_Visitor_FieldSet__WorkPos(mPointer, value);
			}
		}
	}
	public EngineNS.Vector2 WorkSize
	{
		get
		{
			fixed (ImGuiViewport* mPointer = &this)
			{
				return TitanImGui_ImGuiViewport_Visitor_FieldGet__WorkSize(mPointer);
			}
		}
		set
		{
			fixed (ImGuiViewport* mPointer = &this)
			{
				TitanImGui_ImGuiViewport_Visitor_FieldSet__WorkSize(mPointer, value);
			}
		}
	}
	public float DpiScale
	{
		get
		{
			fixed (ImGuiViewport* mPointer = &this)
			{
				return TitanImGui_ImGuiViewport_Visitor_FieldGet__DpiScale(mPointer);
			}
		}
		set
		{
			fixed (ImGuiViewport* mPointer = &this)
			{
				TitanImGui_ImGuiViewport_Visitor_FieldSet__DpiScale(mPointer, value);
			}
		}
	}
	public uint ParentViewportId
	{
		get
		{
			fixed (ImGuiViewport* mPointer = &this)
			{
				return TitanImGui_ImGuiViewport_Visitor_FieldGet__ParentViewportId(mPointer);
			}
		}
		set
		{
			fixed (ImGuiViewport* mPointer = &this)
			{
				TitanImGui_ImGuiViewport_Visitor_FieldSet__ParentViewportId(mPointer, value);
			}
		}
	}
	public ImGuiViewport* ParentViewport
	{
		get
		{
			fixed (ImGuiViewport* mPointer = &this)
			{
				return TitanImGui_ImGuiViewport_Visitor_FieldGet__ParentViewport(mPointer);
			}
		}
		set
		{
			fixed (ImGuiViewport* mPointer = &this)
			{
				TitanImGui_ImGuiViewport_Visitor_FieldSet__ParentViewport(mPointer, value);
			}
		}
	}
	public ImDrawData* DrawData
	{
		get
		{
			fixed (ImGuiViewport* mPointer = &this)
			{
				return TitanImGui_ImGuiViewport_Visitor_FieldGet__DrawData(mPointer);
			}
		}
		set
		{
			fixed (ImGuiViewport* mPointer = &this)
			{
				TitanImGui_ImGuiViewport_Visitor_FieldSet__DrawData(mPointer, value);
			}
		}
	}
	public void* RendererUserData
	{
		get
		{
			fixed (ImGuiViewport* mPointer = &this)
			{
				return TitanImGui_ImGuiViewport_Visitor_FieldGet__RendererUserData(mPointer);
			}
		}
		set
		{
			fixed (ImGuiViewport* mPointer = &this)
			{
				TitanImGui_ImGuiViewport_Visitor_FieldSet__RendererUserData(mPointer, value);
			}
		}
	}
	public void* PlatformUserData
	{
		get
		{
			fixed (ImGuiViewport* mPointer = &this)
			{
				return TitanImGui_ImGuiViewport_Visitor_FieldGet__PlatformUserData(mPointer);
			}
		}
		set
		{
			fixed (ImGuiViewport* mPointer = &this)
			{
				TitanImGui_ImGuiViewport_Visitor_FieldSet__PlatformUserData(mPointer, value);
			}
		}
	}
	public void* PlatformHandle
	{
		get
		{
			fixed (ImGuiViewport* mPointer = &this)
			{
				return TitanImGui_ImGuiViewport_Visitor_FieldGet__PlatformHandle(mPointer);
			}
		}
		set
		{
			fixed (ImGuiViewport* mPointer = &this)
			{
				TitanImGui_ImGuiViewport_Visitor_FieldSet__PlatformHandle(mPointer, value);
			}
		}
	}
	public void* PlatformHandleRaw
	{
		get
		{
			fixed (ImGuiViewport* mPointer = &this)
			{
				return TitanImGui_ImGuiViewport_Visitor_FieldGet__PlatformHandleRaw(mPointer);
			}
		}
		set
		{
			fixed (ImGuiViewport* mPointer = &this)
			{
				TitanImGui_ImGuiViewport_Visitor_FieldSet__PlatformHandleRaw(mPointer, value);
			}
		}
	}
	public bool PlatformWindowCreated
	{
		get
		{
			fixed (ImGuiViewport* mPointer = &this)
			{
				return TitanImGui_ImGuiViewport_Visitor_FieldGet__PlatformWindowCreated(mPointer) == 0 ? false : true;
			}
		}
		set
		{
			fixed (ImGuiViewport* mPointer = &this)
			{
				TitanImGui_ImGuiViewport_Visitor_FieldSet__PlatformWindowCreated(mPointer, value);
			}
		}
	}
	public bool PlatformRequestMove
	{
		get
		{
			fixed (ImGuiViewport* mPointer = &this)
			{
				return TitanImGui_ImGuiViewport_Visitor_FieldGet__PlatformRequestMove(mPointer) == 0 ? false : true;
			}
		}
		set
		{
			fixed (ImGuiViewport* mPointer = &this)
			{
				TitanImGui_ImGuiViewport_Visitor_FieldSet__PlatformRequestMove(mPointer, value);
			}
		}
	}
	public bool PlatformRequestResize
	{
		get
		{
			fixed (ImGuiViewport* mPointer = &this)
			{
				return TitanImGui_ImGuiViewport_Visitor_FieldGet__PlatformRequestResize(mPointer) == 0 ? false : true;
			}
		}
		set
		{
			fixed (ImGuiViewport* mPointer = &this)
			{
				TitanImGui_ImGuiViewport_Visitor_FieldSet__PlatformRequestResize(mPointer, value);
			}
		}
	}
	public bool PlatformRequestClose
	{
		get
		{
			fixed (ImGuiViewport* mPointer = &this)
			{
				return TitanImGui_ImGuiViewport_Visitor_FieldGet__PlatformRequestClose(mPointer) == 0 ? false : true;
			}
		}
		set
		{
			fixed (ImGuiViewport* mPointer = &this)
			{
				TitanImGui_ImGuiViewport_Visitor_FieldSet__PlatformRequestClose(mPointer, value);
			}
		}
	}
	#endregion
	#region Function
	public EngineNS.Vector2 GetCenter()
	{
		fixed (ImGuiViewport* mPointer = &this)
		{
			return TitanImGui_ImGuiViewport_Visitor_GetCenter_3443252160(mPointer);
		}
	}
	public EngineNS.Vector2 GetWorkCenter()
	{
		fixed (ImGuiViewport* mPointer = &this)
		{
			return TitanImGui_ImGuiViewport_Visitor_GetWorkCenter_3443252160(mPointer);
		}
	}
	public string GetDebugName()
	{
		fixed (ImGuiViewport* mPointer = &this)
		{
			return EngineNS.Rtti.TtNativeCoreProvider.MarshalPtrUtf8((IntPtr)TitanImGui_ImGuiViewport_Visitor_GetDebugName_721684103(mPointer));
		}
	}
	#endregion
	#region Core SDK
	const string ModuleNC = EngineNS.CoreSDK.CoreModule;
	//Constructor&Cast
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	extern static void TitanImGui_ImGuiViewport_Visitor_UnsafeCallConstructor_2960189489(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	extern static void TitanImGui_ImGuiViewport_Visitor_UnsafeCallDestructor(void* self);
	//Fields
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static uint TitanImGui_ImGuiViewport_Visitor_FieldGet__ID(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImGuiViewport_Visitor_FieldSet__ID(void* self, uint value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static ImGuiViewportFlags_ TitanImGui_ImGuiViewport_Visitor_FieldGet__Flags(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImGuiViewport_Visitor_FieldSet__Flags(void* self, ImGuiViewportFlags_ value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static EngineNS.Vector2 TitanImGui_ImGuiViewport_Visitor_FieldGet__Pos(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImGuiViewport_Visitor_FieldSet__Pos(void* self, EngineNS.Vector2 value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static EngineNS.Vector2 TitanImGui_ImGuiViewport_Visitor_FieldGet__Size(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImGuiViewport_Visitor_FieldSet__Size(void* self, EngineNS.Vector2 value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static EngineNS.Vector2 TitanImGui_ImGuiViewport_Visitor_FieldGet__FramebufferScale(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImGuiViewport_Visitor_FieldSet__FramebufferScale(void* self, EngineNS.Vector2 value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static EngineNS.Vector2 TitanImGui_ImGuiViewport_Visitor_FieldGet__WorkPos(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImGuiViewport_Visitor_FieldSet__WorkPos(void* self, EngineNS.Vector2 value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static EngineNS.Vector2 TitanImGui_ImGuiViewport_Visitor_FieldGet__WorkSize(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImGuiViewport_Visitor_FieldSet__WorkSize(void* self, EngineNS.Vector2 value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static float TitanImGui_ImGuiViewport_Visitor_FieldGet__DpiScale(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImGuiViewport_Visitor_FieldSet__DpiScale(void* self, float value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static uint TitanImGui_ImGuiViewport_Visitor_FieldGet__ParentViewportId(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImGuiViewport_Visitor_FieldSet__ParentViewportId(void* self, uint value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static ImGuiViewport* TitanImGui_ImGuiViewport_Visitor_FieldGet__ParentViewport(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImGuiViewport_Visitor_FieldSet__ParentViewport(void* self, ImGuiViewport* value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static ImDrawData* TitanImGui_ImGuiViewport_Visitor_FieldGet__DrawData(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImGuiViewport_Visitor_FieldSet__DrawData(void* self, ImDrawData* value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void* TitanImGui_ImGuiViewport_Visitor_FieldGet__RendererUserData(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImGuiViewport_Visitor_FieldSet__RendererUserData(void* self, void* value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void* TitanImGui_ImGuiViewport_Visitor_FieldGet__PlatformUserData(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImGuiViewport_Visitor_FieldSet__PlatformUserData(void* self, void* value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void* TitanImGui_ImGuiViewport_Visitor_FieldGet__PlatformHandle(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImGuiViewport_Visitor_FieldSet__PlatformHandle(void* self, void* value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void* TitanImGui_ImGuiViewport_Visitor_FieldGet__PlatformHandleRaw(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImGuiViewport_Visitor_FieldSet__PlatformHandleRaw(void* self, void* value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static sbyte TitanImGui_ImGuiViewport_Visitor_FieldGet__PlatformWindowCreated(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImGuiViewport_Visitor_FieldSet__PlatformWindowCreated(void* self, bool value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static sbyte TitanImGui_ImGuiViewport_Visitor_FieldGet__PlatformRequestMove(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImGuiViewport_Visitor_FieldSet__PlatformRequestMove(void* self, bool value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static sbyte TitanImGui_ImGuiViewport_Visitor_FieldGet__PlatformRequestResize(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImGuiViewport_Visitor_FieldSet__PlatformRequestResize(void* self, bool value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static sbyte TitanImGui_ImGuiViewport_Visitor_FieldGet__PlatformRequestClose(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImGuiViewport_Visitor_FieldSet__PlatformRequestClose(void* self, bool value);
	//Functions
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	extern static EngineNS.Vector2 TitanImGui_ImGuiViewport_Visitor_GetCenter_3443252160(void* Self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	extern static EngineNS.Vector2 TitanImGui_ImGuiViewport_Visitor_GetWorkCenter_3443252160(void* Self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	extern static sbyte* TitanImGui_ImGuiViewport_Visitor_GetDebugName_721684103(void* Self);
	//Cast
	#endregion
}
