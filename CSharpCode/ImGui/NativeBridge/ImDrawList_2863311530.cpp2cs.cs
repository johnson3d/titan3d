//generated from legacy CppWeaving ImGui surface; native entrypoints routed through TitanImGuiBridge
using System;
using System.Runtime.InteropServices;
#if DEBUG
	using SuppressGC = EngineNS.Rtti.TtDummyAttribute;
#else
	using SuppressGC = System.Runtime.InteropServices.SuppressGCTransitionAttribute;
#endif


public unsafe partial struct ImDrawList : EngineNS.IPtrType
{
	[System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Explicit, Size = 224, Pack = 8)]
	public struct CppStructLayout
	{
		[System.Runtime.InteropServices.FieldOffset(48)]
		public int Flags;
		[System.Runtime.InteropServices.FieldOffset(52)]
		public uint _VtxCurrentIdx;
		[System.Runtime.InteropServices.FieldOffset(64)]
		public ImDrawVert* _VtxWritePtr;
		[System.Runtime.InteropServices.FieldOffset(72)]
		public ushort* _IdxWritePtr;
		[System.Runtime.InteropServices.FieldOffset(208)]
		public float _FringeScale;
		[System.Runtime.InteropServices.FieldOffset(216)]
		public sbyte* _OwnerName;
	}
	private void* mPointer;
	public CppStructLayout* UnsafeAsLayout { get => (CppStructLayout*)mPointer; }
	public ImDrawList(void* p) { mPointer = p; }
	public void UnsafeSetPointer(void* p) { mPointer = p; }
	public IntPtr NativePointer { get => (IntPtr)mPointer; set => mPointer = value.ToPointer(); }
	public ImDrawList* CppPointer { get => (ImDrawList*)mPointer; }
	public bool IsValidPointer { get => mPointer != (void*)0; }
	public static implicit operator ImDrawList* (ImDrawList v)
	{
		return (ImDrawList*)v.mPointer;
	}
	#region Constructor&Cast
	public static EngineNS.FRttiStruct GetTypeRtti()
	{
		return new EngineNS.FRttiStruct(TitanImGui_ImDrawList_Visitor_GetTypeRtti());
	}
	#endregion
	#region Fields
	public int Flags
	{
		get
		{
			return TitanImGui_ImDrawList_Visitor_FieldGet__Flags(mPointer);
		}
		set
		{
			TitanImGui_ImDrawList_Visitor_FieldSet__Flags(mPointer, value);
		}
	}
	public uint _VtxCurrentIdx
	{
		get
		{
			return TitanImGui_ImDrawList_Visitor_FieldGet___VtxCurrentIdx(mPointer);
		}
		set
		{
			TitanImGui_ImDrawList_Visitor_FieldSet___VtxCurrentIdx(mPointer, value);
		}
	}
	public ImDrawVert* _VtxWritePtr
	{
		get
		{
			return TitanImGui_ImDrawList_Visitor_FieldGet___VtxWritePtr(mPointer);
		}
		set
		{
			TitanImGui_ImDrawList_Visitor_FieldSet___VtxWritePtr(mPointer, value);
		}
	}
	public ushort* _IdxWritePtr
	{
		get
		{
			return TitanImGui_ImDrawList_Visitor_FieldGet___IdxWritePtr(mPointer);
		}
		set
		{
			TitanImGui_ImDrawList_Visitor_FieldSet___IdxWritePtr(mPointer, value);
		}
	}
	public float _FringeScale
	{
		get
		{
			return TitanImGui_ImDrawList_Visitor_FieldGet___FringeScale(mPointer);
		}
		set
		{
			TitanImGui_ImDrawList_Visitor_FieldSet___FringeScale(mPointer, value);
		}
	}
	public string _OwnerName
	{
		get
		{
			return EngineNS.Rtti.TtNativeCoreProvider.MarshalPtrUtf8((IntPtr)TitanImGui_ImDrawList_Visitor_FieldGet___OwnerName(mPointer));
		}
		set
		{
			TitanImGui_ImDrawList_Visitor_FieldSet___OwnerName(mPointer, value);
		}
	}
	#endregion
	#region Function
	public ImDrawCmd* GetCmdBuffer(int* size)
	{
		return TitanImGui_ImDrawList_Visitor_GetCmdBuffer_271826910(mPointer, size);
	}
	public ImDrawCmd* GetCmdBuffer( ref int size)
	{
		fixed(int* pinned_size = &size)
		{
			return GetCmdBuffer(pinned_size);
		}
	}
	public ushort* GetIdxBuffer(int* size)
	{
		return TitanImGui_ImDrawList_Visitor_GetIdxBuffer_4083488483(mPointer, size);
	}
	public ushort* GetIdxBuffer( ref int size)
	{
		fixed(int* pinned_size = &size)
		{
			return GetIdxBuffer(pinned_size);
		}
	}
	public ImDrawVert* GetVtxBuffer(int* size)
	{
		return TitanImGui_ImDrawList_Visitor_GetVtxBuffer_1699165095(mPointer, size);
	}
	public ImDrawVert* GetVtxBuffer( ref int size)
	{
		fixed(int* pinned_size = &size)
		{
			return GetVtxBuffer(pinned_size);
		}
	}
	public void PushClipRect(EngineNS.Vector2* clip_rect_min,EngineNS.Vector2* clip_rect_max,bool intersect_with_current_clip_rect)
	{
		TitanImGui_ImDrawList_Visitor_PushClipRect_1087775703(mPointer, clip_rect_min, clip_rect_max, intersect_with_current_clip_rect);
	}
	public void PushClipRect( in EngineNS.Vector2 clip_rect_min, in EngineNS.Vector2 clip_rect_max,bool intersect_with_current_clip_rect)
	{
		fixed(EngineNS.Vector2* pinned_clip_rect_min = &clip_rect_min)
		fixed(EngineNS.Vector2* pinned_clip_rect_max = &clip_rect_max)
		{
			PushClipRect(pinned_clip_rect_min, pinned_clip_rect_max, intersect_with_current_clip_rect);
		}
	}
	public void PushClipRectFullScreen()
	{
		TitanImGui_ImDrawList_Visitor_PushClipRectFullScreen_2960189489(mPointer);
	}
	public void PopClipRect()
	{
		TitanImGui_ImDrawList_Visitor_PopClipRect_2960189489(mPointer);
	}
	public void PushTexture(ImTextureRef tex_ref)
	{
		TitanImGui_ImDrawList_Visitor_PushTexture_2544618575(mPointer, tex_ref);
	}
	public void PopTexture()
	{
		TitanImGui_ImDrawList_Visitor_PopTexture_2960189489(mPointer);
	}
	public EngineNS.Vector2 GetClipRectMin()
	{
		return TitanImGui_ImDrawList_Visitor_GetClipRectMin_3443252160(mPointer);
	}
	public EngineNS.Vector2 GetClipRectMax()
	{
		return TitanImGui_ImDrawList_Visitor_GetClipRectMax_3443252160(mPointer);
	}
	public void AddLine(EngineNS.Vector2* p1,EngineNS.Vector2* p2,uint col,float thickness)
	{
		TitanImGui_ImDrawList_Visitor_AddLine_385528029(mPointer, p1, p2, col, thickness);
	}
	public void AddLine( in EngineNS.Vector2 p1, in EngineNS.Vector2 p2,uint col,float thickness)
	{
		fixed(EngineNS.Vector2* pinned_p1 = &p1)
		fixed(EngineNS.Vector2* pinned_p2 = &p2)
		{
			AddLine(pinned_p1, pinned_p2, col, thickness);
		}
	}
	public void AddRect(EngineNS.Vector2* p_min,EngineNS.Vector2* p_max,uint col,float rounding,ImDrawFlags_ flags,float thickness)
	{
		TitanImGui_ImDrawList_Visitor_AddRect_3863083696(mPointer, p_min, p_max, col, rounding, flags, thickness);
	}
	public void AddRect( in EngineNS.Vector2 p_min, in EngineNS.Vector2 p_max,uint col,float rounding,ImDrawFlags_ flags,float thickness)
	{
		fixed(EngineNS.Vector2* pinned_p_min = &p_min)
		fixed(EngineNS.Vector2* pinned_p_max = &p_max)
		{
			AddRect(pinned_p_min, pinned_p_max, col, rounding, flags, thickness);
		}
	}
	public void AddRectFilled(EngineNS.Vector2* p_min,EngineNS.Vector2* p_max,uint col,float rounding,ImDrawFlags_ flags)
	{
		TitanImGui_ImDrawList_Visitor_AddRectFilled_1833191196(mPointer, p_min, p_max, col, rounding, flags);
	}
	public void AddRectFilled( in EngineNS.Vector2 p_min, in EngineNS.Vector2 p_max,uint col,float rounding,ImDrawFlags_ flags)
	{
		fixed(EngineNS.Vector2* pinned_p_min = &p_min)
		fixed(EngineNS.Vector2* pinned_p_max = &p_max)
		{
			AddRectFilled(pinned_p_min, pinned_p_max, col, rounding, flags);
		}
	}
	public void AddRectFilledMultiColor(EngineNS.Vector2* p_min,EngineNS.Vector2* p_max,uint col_upr_left,uint col_upr_right,uint col_bot_right,uint col_bot_left)
	{
		TitanImGui_ImDrawList_Visitor_AddRectFilledMultiColor_924535741(mPointer, p_min, p_max, col_upr_left, col_upr_right, col_bot_right, col_bot_left);
	}
	public void AddRectFilledMultiColor( in EngineNS.Vector2 p_min, in EngineNS.Vector2 p_max,uint col_upr_left,uint col_upr_right,uint col_bot_right,uint col_bot_left)
	{
		fixed(EngineNS.Vector2* pinned_p_min = &p_min)
		fixed(EngineNS.Vector2* pinned_p_max = &p_max)
		{
			AddRectFilledMultiColor(pinned_p_min, pinned_p_max, col_upr_left, col_upr_right, col_bot_right, col_bot_left);
		}
	}
	public void AddQuad(EngineNS.Vector2* p1,EngineNS.Vector2* p2,EngineNS.Vector2* p3,EngineNS.Vector2* p4,uint col,float thickness)
	{
		TitanImGui_ImDrawList_Visitor_AddQuad_2574879389(mPointer, p1, p2, p3, p4, col, thickness);
	}
	public void AddQuad( in EngineNS.Vector2 p1, in EngineNS.Vector2 p2, in EngineNS.Vector2 p3, in EngineNS.Vector2 p4,uint col,float thickness)
	{
		fixed(EngineNS.Vector2* pinned_p1 = &p1)
		fixed(EngineNS.Vector2* pinned_p2 = &p2)
		fixed(EngineNS.Vector2* pinned_p3 = &p3)
		fixed(EngineNS.Vector2* pinned_p4 = &p4)
		{
			AddQuad(pinned_p1, pinned_p2, pinned_p3, pinned_p4, col, thickness);
		}
	}
	public void AddQuadFilled(EngineNS.Vector2* p1,EngineNS.Vector2* p2,EngineNS.Vector2* p3,EngineNS.Vector2* p4,uint col)
	{
		TitanImGui_ImDrawList_Visitor_AddQuadFilled_498068585(mPointer, p1, p2, p3, p4, col);
	}
	public void AddQuadFilled( in EngineNS.Vector2 p1, in EngineNS.Vector2 p2, in EngineNS.Vector2 p3, in EngineNS.Vector2 p4,uint col)
	{
		fixed(EngineNS.Vector2* pinned_p1 = &p1)
		fixed(EngineNS.Vector2* pinned_p2 = &p2)
		fixed(EngineNS.Vector2* pinned_p3 = &p3)
		fixed(EngineNS.Vector2* pinned_p4 = &p4)
		{
			AddQuadFilled(pinned_p1, pinned_p2, pinned_p3, pinned_p4, col);
		}
	}
	public void AddTriangle(EngineNS.Vector2* p1,EngineNS.Vector2* p2,EngineNS.Vector2* p3,uint col,float thickness)
	{
		TitanImGui_ImDrawList_Visitor_AddTriangle_4155471270(mPointer, p1, p2, p3, col, thickness);
	}
	public void AddTriangle( in EngineNS.Vector2 p1, in EngineNS.Vector2 p2, in EngineNS.Vector2 p3,uint col,float thickness)
	{
		fixed(EngineNS.Vector2* pinned_p1 = &p1)
		fixed(EngineNS.Vector2* pinned_p2 = &p2)
		fixed(EngineNS.Vector2* pinned_p3 = &p3)
		{
			AddTriangle(pinned_p1, pinned_p2, pinned_p3, col, thickness);
		}
	}
	public void AddTriangleFilled(EngineNS.Vector2* p1,EngineNS.Vector2* p2,EngineNS.Vector2* p3,uint col)
	{
		TitanImGui_ImDrawList_Visitor_AddTriangleFilled_3736051506(mPointer, p1, p2, p3, col);
	}
	public void AddTriangleFilled( in EngineNS.Vector2 p1, in EngineNS.Vector2 p2, in EngineNS.Vector2 p3,uint col)
	{
		fixed(EngineNS.Vector2* pinned_p1 = &p1)
		fixed(EngineNS.Vector2* pinned_p2 = &p2)
		fixed(EngineNS.Vector2* pinned_p3 = &p3)
		{
			AddTriangleFilled(pinned_p1, pinned_p2, pinned_p3, col);
		}
	}
	public void AddCircle(EngineNS.Vector2* center,float radius,uint col,int num_segments,float thickness)
	{
		TitanImGui_ImDrawList_Visitor_AddCircle_3914253043(mPointer, center, radius, col, num_segments, thickness);
	}
	public void AddCircle( in EngineNS.Vector2 center,float radius,uint col,int num_segments,float thickness)
	{
		fixed(EngineNS.Vector2* pinned_center = &center)
		{
			AddCircle(pinned_center, radius, col, num_segments, thickness);
		}
	}
	public void AddCircleFilled(EngineNS.Vector2* center,float radius,uint col,int num_segments)
	{
		TitanImGui_ImDrawList_Visitor_AddCircleFilled_23506951(mPointer, center, radius, col, num_segments);
	}
	public void AddCircleFilled( in EngineNS.Vector2 center,float radius,uint col,int num_segments)
	{
		fixed(EngineNS.Vector2* pinned_center = &center)
		{
			AddCircleFilled(pinned_center, radius, col, num_segments);
		}
	}
	public void AddNgon(EngineNS.Vector2* center,float radius,uint col,int num_segments,float thickness)
	{
		TitanImGui_ImDrawList_Visitor_AddNgon_3914253043(mPointer, center, radius, col, num_segments, thickness);
	}
	public void AddNgon( in EngineNS.Vector2 center,float radius,uint col,int num_segments,float thickness)
	{
		fixed(EngineNS.Vector2* pinned_center = &center)
		{
			AddNgon(pinned_center, radius, col, num_segments, thickness);
		}
	}
	public void AddNgonFilled(EngineNS.Vector2* center,float radius,uint col,int num_segments)
	{
		TitanImGui_ImDrawList_Visitor_AddNgonFilled_23506951(mPointer, center, radius, col, num_segments);
	}
	public void AddNgonFilled( in EngineNS.Vector2 center,float radius,uint col,int num_segments)
	{
		fixed(EngineNS.Vector2* pinned_center = &center)
		{
			AddNgonFilled(pinned_center, radius, col, num_segments);
		}
	}
	public void AddEllipse(EngineNS.Vector2* center,EngineNS.Vector2* radius,uint col,float rot,int num_segments,float thickness)
	{
		TitanImGui_ImDrawList_Visitor_AddEllipse_2337479130(mPointer, center, radius, col, rot, num_segments, thickness);
	}
	public void AddEllipse( in EngineNS.Vector2 center, in EngineNS.Vector2 radius,uint col,float rot,int num_segments,float thickness)
	{
		fixed(EngineNS.Vector2* pinned_center = &center)
		fixed(EngineNS.Vector2* pinned_radius = &radius)
		{
			AddEllipse(pinned_center, pinned_radius, col, rot, num_segments, thickness);
		}
	}
	public void AddEllipseFilled(EngineNS.Vector2* center,EngineNS.Vector2* radius,uint col,float rot,int num_segments)
	{
		TitanImGui_ImDrawList_Visitor_AddEllipseFilled_4133031662(mPointer, center, radius, col, rot, num_segments);
	}
	public void AddEllipseFilled( in EngineNS.Vector2 center, in EngineNS.Vector2 radius,uint col,float rot,int num_segments)
	{
		fixed(EngineNS.Vector2* pinned_center = &center)
		fixed(EngineNS.Vector2* pinned_radius = &radius)
		{
			AddEllipseFilled(pinned_center, pinned_radius, col, rot, num_segments);
		}
	}
	public void AddText(EngineNS.Vector2* pos,uint col,string text_begin,string text_end)
	{
		TitanImGui_ImDrawList_Visitor_AddText_374383570(mPointer, pos, col, text_begin, text_end);
	}
	public void AddText( in EngineNS.Vector2 pos,uint col,string text_begin,string text_end)
	{
		fixed(EngineNS.Vector2* pinned_pos = &pos)
		{
			AddText(pinned_pos, col, text_begin, text_end);
		}
	}
	public void AddText(ImFont font,float font_size,EngineNS.Vector2* pos,uint col,string text_begin,string text_end,float wrap_width,EngineNS.Vector4* cpu_fine_clip_rect)
	{
		TitanImGui_ImDrawList_Visitor_AddText_3346644164(mPointer, font, font_size, pos, col, text_begin, text_end, wrap_width, cpu_fine_clip_rect);
	}
	public void AddText(ImFont font,float font_size, in EngineNS.Vector2 pos,uint col,string text_begin,string text_end,float wrap_width, in EngineNS.Vector4 cpu_fine_clip_rect)
	{
		fixed(EngineNS.Vector2* pinned_pos = &pos)
		fixed(EngineNS.Vector4* pinned_cpu_fine_clip_rect = &cpu_fine_clip_rect)
		{
			AddText(font, font_size, pinned_pos, col, text_begin, text_end, wrap_width, pinned_cpu_fine_clip_rect);
		}
	}
	public void AddBezierCubic(EngineNS.Vector2* p1,EngineNS.Vector2* p2,EngineNS.Vector2* p3,EngineNS.Vector2* p4,uint col,float thickness,int num_segments)
	{
		TitanImGui_ImDrawList_Visitor_AddBezierCubic_184805550(mPointer, p1, p2, p3, p4, col, thickness, num_segments);
	}
	public void AddBezierCubic( in EngineNS.Vector2 p1, in EngineNS.Vector2 p2, in EngineNS.Vector2 p3, in EngineNS.Vector2 p4,uint col,float thickness,int num_segments)
	{
		fixed(EngineNS.Vector2* pinned_p1 = &p1)
		fixed(EngineNS.Vector2* pinned_p2 = &p2)
		fixed(EngineNS.Vector2* pinned_p3 = &p3)
		fixed(EngineNS.Vector2* pinned_p4 = &p4)
		{
			AddBezierCubic(pinned_p1, pinned_p2, pinned_p3, pinned_p4, col, thickness, num_segments);
		}
	}
	public void AddBezierQuadratic(EngineNS.Vector2* p1,EngineNS.Vector2* p2,EngineNS.Vector2* p3,uint col,float thickness,int num_segments)
	{
		TitanImGui_ImDrawList_Visitor_AddBezierQuadratic_1228503143(mPointer, p1, p2, p3, col, thickness, num_segments);
	}
	public void AddBezierQuadratic( in EngineNS.Vector2 p1, in EngineNS.Vector2 p2, in EngineNS.Vector2 p3,uint col,float thickness,int num_segments)
	{
		fixed(EngineNS.Vector2* pinned_p1 = &p1)
		fixed(EngineNS.Vector2* pinned_p2 = &p2)
		fixed(EngineNS.Vector2* pinned_p3 = &p3)
		{
			AddBezierQuadratic(pinned_p1, pinned_p2, pinned_p3, col, thickness, num_segments);
		}
	}
	public void AddPolyline(EngineNS.Vector2* points,int num_points,uint col,ImDrawFlags_ flags,float thickness)
	{
		TitanImGui_ImDrawList_Visitor_AddPolyline_432970084(mPointer, points, num_points, col, flags, thickness);
	}
	public void AddPolyline( in EngineNS.Vector2 points,int num_points,uint col,ImDrawFlags_ flags,float thickness)
	{
		fixed(EngineNS.Vector2* pinned_points = &points)
		{
			AddPolyline(pinned_points, num_points, col, flags, thickness);
		}
	}
	public void AddConvexPolyFilled(EngineNS.Vector2* points,int num_points,uint col)
	{
		TitanImGui_ImDrawList_Visitor_AddConvexPolyFilled_173088695(mPointer, points, num_points, col);
	}
	public void AddConvexPolyFilled( in EngineNS.Vector2 points,int num_points,uint col)
	{
		fixed(EngineNS.Vector2* pinned_points = &points)
		{
			AddConvexPolyFilled(pinned_points, num_points, col);
		}
	}
	public void AddConcavePolyFilled(EngineNS.Vector2* points,int num_points,uint col)
	{
		TitanImGui_ImDrawList_Visitor_AddConcavePolyFilled_173088695(mPointer, points, num_points, col);
	}
	public void AddConcavePolyFilled( in EngineNS.Vector2 points,int num_points,uint col)
	{
		fixed(EngineNS.Vector2* pinned_points = &points)
		{
			AddConcavePolyFilled(pinned_points, num_points, col);
		}
	}
	public void AddImage(ImTextureRef tex_ref,EngineNS.Vector2* p_min,EngineNS.Vector2* p_max,EngineNS.Vector2* uv_min,EngineNS.Vector2* uv_max,uint col)
	{
		TitanImGui_ImDrawList_Visitor_AddImage_2001392767(mPointer, tex_ref, p_min, p_max, uv_min, uv_max, col);
	}
	public void AddImage(ImTextureRef tex_ref, in EngineNS.Vector2 p_min, in EngineNS.Vector2 p_max, in EngineNS.Vector2 uv_min, in EngineNS.Vector2 uv_max,uint col)
	{
		fixed(EngineNS.Vector2* pinned_p_min = &p_min)
		fixed(EngineNS.Vector2* pinned_p_max = &p_max)
		fixed(EngineNS.Vector2* pinned_uv_min = &uv_min)
		fixed(EngineNS.Vector2* pinned_uv_max = &uv_max)
		{
			AddImage(tex_ref, pinned_p_min, pinned_p_max, pinned_uv_min, pinned_uv_max, col);
		}
	}
	public void AddImageQuad(ImTextureRef tex_ref,EngineNS.Vector2* p1,EngineNS.Vector2* p2,EngineNS.Vector2* p3,EngineNS.Vector2* p4,EngineNS.Vector2* uv1,EngineNS.Vector2* uv2,EngineNS.Vector2* uv3,EngineNS.Vector2* uv4,uint col)
	{
		TitanImGui_ImDrawList_Visitor_AddImageQuad_4052651007(mPointer, tex_ref, p1, p2, p3, p4, uv1, uv2, uv3, uv4, col);
	}
	public void AddImageQuad(ImTextureRef tex_ref, in EngineNS.Vector2 p1, in EngineNS.Vector2 p2, in EngineNS.Vector2 p3, in EngineNS.Vector2 p4, in EngineNS.Vector2 uv1, in EngineNS.Vector2 uv2, in EngineNS.Vector2 uv3, in EngineNS.Vector2 uv4,uint col)
	{
		fixed(EngineNS.Vector2* pinned_p1 = &p1)
		fixed(EngineNS.Vector2* pinned_p2 = &p2)
		fixed(EngineNS.Vector2* pinned_p3 = &p3)
		fixed(EngineNS.Vector2* pinned_p4 = &p4)
		fixed(EngineNS.Vector2* pinned_uv1 = &uv1)
		fixed(EngineNS.Vector2* pinned_uv2 = &uv2)
		fixed(EngineNS.Vector2* pinned_uv3 = &uv3)
		fixed(EngineNS.Vector2* pinned_uv4 = &uv4)
		{
			AddImageQuad(tex_ref, pinned_p1, pinned_p2, pinned_p3, pinned_p4, pinned_uv1, pinned_uv2, pinned_uv3, pinned_uv4, col);
		}
	}
	public void AddImageRounded(ImTextureRef tex_ref,EngineNS.Vector2* p_min,EngineNS.Vector2* p_max,EngineNS.Vector2* uv_min,EngineNS.Vector2* uv_max,uint col,float rounding,ImDrawFlags_ flags)
	{
		TitanImGui_ImDrawList_Visitor_AddImageRounded_7601998(mPointer, tex_ref, p_min, p_max, uv_min, uv_max, col, rounding, flags);
	}
	public void AddImageRounded(ImTextureRef tex_ref, in EngineNS.Vector2 p_min, in EngineNS.Vector2 p_max, in EngineNS.Vector2 uv_min, in EngineNS.Vector2 uv_max,uint col,float rounding,ImDrawFlags_ flags)
	{
		fixed(EngineNS.Vector2* pinned_p_min = &p_min)
		fixed(EngineNS.Vector2* pinned_p_max = &p_max)
		fixed(EngineNS.Vector2* pinned_uv_min = &uv_min)
		fixed(EngineNS.Vector2* pinned_uv_max = &uv_max)
		{
			AddImageRounded(tex_ref, pinned_p_min, pinned_p_max, pinned_uv_min, pinned_uv_max, col, rounding, flags);
		}
	}
	public void PathClear()
	{
		TitanImGui_ImDrawList_Visitor_PathClear_2960189489(mPointer);
	}
	public void PathLineTo(EngineNS.Vector2* pos)
	{
		TitanImGui_ImDrawList_Visitor_PathLineTo_2086025684(mPointer, pos);
	}
	public void PathLineTo( in EngineNS.Vector2 pos)
	{
		fixed(EngineNS.Vector2* pinned_pos = &pos)
		{
			PathLineTo(pinned_pos);
		}
	}
	public void PathLineToMergeDuplicate(EngineNS.Vector2* pos)
	{
		TitanImGui_ImDrawList_Visitor_PathLineToMergeDuplicate_2086025684(mPointer, pos);
	}
	public void PathLineToMergeDuplicate( in EngineNS.Vector2 pos)
	{
		fixed(EngineNS.Vector2* pinned_pos = &pos)
		{
			PathLineToMergeDuplicate(pinned_pos);
		}
	}
	public void PathFillConvex(uint col)
	{
		TitanImGui_ImDrawList_Visitor_PathFillConvex_3543463401(mPointer, col);
	}
	public void PathFillConcave(uint col)
	{
		TitanImGui_ImDrawList_Visitor_PathFillConcave_3543463401(mPointer, col);
	}
	public void PathStroke(uint col,ImDrawFlags_ flags,float thickness)
	{
		TitanImGui_ImDrawList_Visitor_PathStroke_3703851534(mPointer, col, flags, thickness);
	}
	public void PathArcTo(EngineNS.Vector2* center,float radius,float a_min,float a_max,int num_segments)
	{
		TitanImGui_ImDrawList_Visitor_PathArcTo_1172245843(mPointer, center, radius, a_min, a_max, num_segments);
	}
	public void PathArcTo( in EngineNS.Vector2 center,float radius,float a_min,float a_max,int num_segments)
	{
		fixed(EngineNS.Vector2* pinned_center = &center)
		{
			PathArcTo(pinned_center, radius, a_min, a_max, num_segments);
		}
	}
	public void PathArcToFast(EngineNS.Vector2* center,float radius,int a_min_of_12,int a_max_of_12)
	{
		TitanImGui_ImDrawList_Visitor_PathArcToFast_64231906(mPointer, center, radius, a_min_of_12, a_max_of_12);
	}
	public void PathArcToFast( in EngineNS.Vector2 center,float radius,int a_min_of_12,int a_max_of_12)
	{
		fixed(EngineNS.Vector2* pinned_center = &center)
		{
			PathArcToFast(pinned_center, radius, a_min_of_12, a_max_of_12);
		}
	}
	public void PathEllipticalArcTo(EngineNS.Vector2* center,EngineNS.Vector2* radius,float rot,float a_min,float a_max,int num_segments)
	{
		TitanImGui_ImDrawList_Visitor_PathEllipticalArcTo_1049904282(mPointer, center, radius, rot, a_min, a_max, num_segments);
	}
	public void PathEllipticalArcTo( in EngineNS.Vector2 center, in EngineNS.Vector2 radius,float rot,float a_min,float a_max,int num_segments)
	{
		fixed(EngineNS.Vector2* pinned_center = &center)
		fixed(EngineNS.Vector2* pinned_radius = &radius)
		{
			PathEllipticalArcTo(pinned_center, pinned_radius, rot, a_min, a_max, num_segments);
		}
	}
	public void PathBezierCubicCurveTo(EngineNS.Vector2* p2,EngineNS.Vector2* p3,EngineNS.Vector2* p4,int num_segments)
	{
		TitanImGui_ImDrawList_Visitor_PathBezierCubicCurveTo_1701830995(mPointer, p2, p3, p4, num_segments);
	}
	public void PathBezierCubicCurveTo( in EngineNS.Vector2 p2, in EngineNS.Vector2 p3, in EngineNS.Vector2 p4,int num_segments)
	{
		fixed(EngineNS.Vector2* pinned_p2 = &p2)
		fixed(EngineNS.Vector2* pinned_p3 = &p3)
		fixed(EngineNS.Vector2* pinned_p4 = &p4)
		{
			PathBezierCubicCurveTo(pinned_p2, pinned_p3, pinned_p4, num_segments);
		}
	}
	public void PathBezierQuadraticCurveTo(EngineNS.Vector2* p2,EngineNS.Vector2* p3,int num_segments)
	{
		TitanImGui_ImDrawList_Visitor_PathBezierQuadraticCurveTo_2431234604(mPointer, p2, p3, num_segments);
	}
	public void PathBezierQuadraticCurveTo( in EngineNS.Vector2 p2, in EngineNS.Vector2 p3,int num_segments)
	{
		fixed(EngineNS.Vector2* pinned_p2 = &p2)
		fixed(EngineNS.Vector2* pinned_p3 = &p3)
		{
			PathBezierQuadraticCurveTo(pinned_p2, pinned_p3, num_segments);
		}
	}
	public void PathRect(EngineNS.Vector2* rect_min,EngineNS.Vector2* rect_max,float rounding,ImDrawFlags_ flags)
	{
		TitanImGui_ImDrawList_Visitor_PathRect_1664922888(mPointer, rect_min, rect_max, rounding, flags);
	}
	public void PathRect( in EngineNS.Vector2 rect_min, in EngineNS.Vector2 rect_max,float rounding,ImDrawFlags_ flags)
	{
		fixed(EngineNS.Vector2* pinned_rect_min = &rect_min)
		fixed(EngineNS.Vector2* pinned_rect_max = &rect_max)
		{
			PathRect(pinned_rect_min, pinned_rect_max, rounding, flags);
		}
	}
	public unsafe delegate void FDelegate_ImDrawCallback(ImDrawList arg0,ImDrawCmd* arg1);
	public void AddCallback(FDelegate_ImDrawCallback callback,void* userdata,IntPtr userdata_size)
	{
		TitanImGui_ImDrawList_Visitor_AddCallback_933021736(mPointer, callback, userdata, userdata_size);
	}
	public void AddDrawCmd()
	{
		TitanImGui_ImDrawList_Visitor_AddDrawCmd_2960189489(mPointer);
	}
	public ImDrawList CloneOutput()
	{
		return new ImDrawList(TitanImGui_ImDrawList_Visitor_CloneOutput_4178288180(mPointer));
	}
	public void ChannelsSplit(int count)
	{
		TitanImGui_ImDrawList_Visitor_ChannelsSplit_4038704236(mPointer, count);
	}
	public void ChannelsMerge()
	{
		TitanImGui_ImDrawList_Visitor_ChannelsMerge_2960189489(mPointer);
	}
	public void ChannelsSetCurrent(int n)
	{
		TitanImGui_ImDrawList_Visitor_ChannelsSetCurrent_4038704236(mPointer, n);
	}
	public void PrimReserve(int idx_count,int vtx_count)
	{
		TitanImGui_ImDrawList_Visitor_PrimReserve_3539386109(mPointer, idx_count, vtx_count);
	}
	public void PrimUnreserve(int idx_count,int vtx_count)
	{
		TitanImGui_ImDrawList_Visitor_PrimUnreserve_3539386109(mPointer, idx_count, vtx_count);
	}
	public void PrimRect(EngineNS.Vector2* a,EngineNS.Vector2* b,uint col)
	{
		TitanImGui_ImDrawList_Visitor_PrimRect_2036564649(mPointer, a, b, col);
	}
	public void PrimRect( in EngineNS.Vector2 a, in EngineNS.Vector2 b,uint col)
	{
		fixed(EngineNS.Vector2* pinned_a = &a)
		fixed(EngineNS.Vector2* pinned_b = &b)
		{
			PrimRect(pinned_a, pinned_b, col);
		}
	}
	public void PrimRectUV(EngineNS.Vector2* a,EngineNS.Vector2* b,EngineNS.Vector2* uv_a,EngineNS.Vector2* uv_b,uint col)
	{
		TitanImGui_ImDrawList_Visitor_PrimRectUV_498068585(mPointer, a, b, uv_a, uv_b, col);
	}
	public void PrimRectUV( in EngineNS.Vector2 a, in EngineNS.Vector2 b, in EngineNS.Vector2 uv_a, in EngineNS.Vector2 uv_b,uint col)
	{
		fixed(EngineNS.Vector2* pinned_a = &a)
		fixed(EngineNS.Vector2* pinned_b = &b)
		fixed(EngineNS.Vector2* pinned_uv_a = &uv_a)
		fixed(EngineNS.Vector2* pinned_uv_b = &uv_b)
		{
			PrimRectUV(pinned_a, pinned_b, pinned_uv_a, pinned_uv_b, col);
		}
	}
	public void PrimQuadUV(EngineNS.Vector2* a,EngineNS.Vector2* b,EngineNS.Vector2* c,EngineNS.Vector2* d,EngineNS.Vector2* uv_a,EngineNS.Vector2* uv_b,EngineNS.Vector2* uv_c,EngineNS.Vector2* uv_d,uint col)
	{
		TitanImGui_ImDrawList_Visitor_PrimQuadUV_40166121(mPointer, a, b, c, d, uv_a, uv_b, uv_c, uv_d, col);
	}
	public void PrimQuadUV( in EngineNS.Vector2 a, in EngineNS.Vector2 b, in EngineNS.Vector2 c, in EngineNS.Vector2 d, in EngineNS.Vector2 uv_a, in EngineNS.Vector2 uv_b, in EngineNS.Vector2 uv_c, in EngineNS.Vector2 uv_d,uint col)
	{
		fixed(EngineNS.Vector2* pinned_a = &a)
		fixed(EngineNS.Vector2* pinned_b = &b)
		fixed(EngineNS.Vector2* pinned_c = &c)
		fixed(EngineNS.Vector2* pinned_d = &d)
		fixed(EngineNS.Vector2* pinned_uv_a = &uv_a)
		fixed(EngineNS.Vector2* pinned_uv_b = &uv_b)
		fixed(EngineNS.Vector2* pinned_uv_c = &uv_c)
		fixed(EngineNS.Vector2* pinned_uv_d = &uv_d)
		{
			PrimQuadUV(pinned_a, pinned_b, pinned_c, pinned_d, pinned_uv_a, pinned_uv_b, pinned_uv_c, pinned_uv_d, col);
		}
	}
	public void PrimWriteVtx(EngineNS.Vector2* pos,EngineNS.Vector2* uv,uint col)
	{
		TitanImGui_ImDrawList_Visitor_PrimWriteVtx_2036564649(mPointer, pos, uv, col);
	}
	public void PrimWriteVtx( in EngineNS.Vector2 pos, in EngineNS.Vector2 uv,uint col)
	{
		fixed(EngineNS.Vector2* pinned_pos = &pos)
		fixed(EngineNS.Vector2* pinned_uv = &uv)
		{
			PrimWriteVtx(pinned_pos, pinned_uv, col);
		}
	}
	public void PrimWriteIdx(ushort idx)
	{
		TitanImGui_ImDrawList_Visitor_PrimWriteIdx_2016656406(mPointer, idx);
	}
	public void PrimVtx(EngineNS.Vector2* pos,EngineNS.Vector2* uv,uint col)
	{
		TitanImGui_ImDrawList_Visitor_PrimVtx_2036564649(mPointer, pos, uv, col);
	}
	public void PrimVtx( in EngineNS.Vector2 pos, in EngineNS.Vector2 uv,uint col)
	{
		fixed(EngineNS.Vector2* pinned_pos = &pos)
		fixed(EngineNS.Vector2* pinned_uv = &uv)
		{
			PrimVtx(pinned_pos, pinned_uv, col);
		}
	}
	public void PushTextureID(ImTextureRef tex_ref)
	{
		TitanImGui_ImDrawList_Visitor_PushTextureID_2544618575(mPointer, tex_ref);
	}
	public void PopTextureID()
	{
		TitanImGui_ImDrawList_Visitor_PopTextureID_2960189489(mPointer);
	}
	public void _ResetForNewFrame()
	{
		TitanImGui_ImDrawList_Visitor__ResetForNewFrame_2960189489(mPointer);
	}
	public void _ClearFreeMemory()
	{
		TitanImGui_ImDrawList_Visitor__ClearFreeMemory_2960189489(mPointer);
	}
	public void _PopUnusedDrawCmd()
	{
		TitanImGui_ImDrawList_Visitor__PopUnusedDrawCmd_2960189489(mPointer);
	}
	public void _TryMergeDrawCmds()
	{
		TitanImGui_ImDrawList_Visitor__TryMergeDrawCmds_2960189489(mPointer);
	}
	public void _OnChangedClipRect()
	{
		TitanImGui_ImDrawList_Visitor__OnChangedClipRect_2960189489(mPointer);
	}
	public void _OnChangedTexture()
	{
		TitanImGui_ImDrawList_Visitor__OnChangedTexture_2960189489(mPointer);
	}
	public void _OnChangedVtxOffset()
	{
		TitanImGui_ImDrawList_Visitor__OnChangedVtxOffset_2960189489(mPointer);
	}
	public void _SetTexture(ImTextureRef tex_ref)
	{
		TitanImGui_ImDrawList_Visitor__SetTexture_2544618575(mPointer, tex_ref);
	}
	public int _CalcCircleAutoSegmentCount(float radius)
	{
		return TitanImGui_ImDrawList_Visitor__CalcCircleAutoSegmentCount_1740070591(mPointer, radius);
	}
	public void _PathArcToFastEx(EngineNS.Vector2* center,float radius,int a_min_sample,int a_max_sample,int a_step)
	{
		TitanImGui_ImDrawList_Visitor__PathArcToFastEx_792707599(mPointer, center, radius, a_min_sample, a_max_sample, a_step);
	}
	public void _PathArcToFastEx( in EngineNS.Vector2 center,float radius,int a_min_sample,int a_max_sample,int a_step)
	{
		fixed(EngineNS.Vector2* pinned_center = &center)
		{
			_PathArcToFastEx(pinned_center, radius, a_min_sample, a_max_sample, a_step);
		}
	}
	public void _PathArcToN(EngineNS.Vector2* center,float radius,float a_min,float a_max,int num_segments)
	{
		TitanImGui_ImDrawList_Visitor__PathArcToN_1172245843(mPointer, center, radius, a_min, a_max, num_segments);
	}
	public void _PathArcToN( in EngineNS.Vector2 center,float radius,float a_min,float a_max,int num_segments)
	{
		fixed(EngineNS.Vector2* pinned_center = &center)
		{
			_PathArcToN(pinned_center, radius, a_min, a_max, num_segments);
		}
	}
	#endregion
	#region Core SDK
	const string ModuleNC = EngineNS.CoreSDK.CoreModule;
	//Constructor&Cast
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	extern static EngineNS.FRttiStruct* TitanImGui_ImDrawList_Visitor_GetTypeRtti();
	//Fields
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static int TitanImGui_ImDrawList_Visitor_FieldGet__Flags(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImDrawList_Visitor_FieldSet__Flags(void* self, int value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static uint TitanImGui_ImDrawList_Visitor_FieldGet___VtxCurrentIdx(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImDrawList_Visitor_FieldSet___VtxCurrentIdx(void* self, uint value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static ImDrawVert* TitanImGui_ImDrawList_Visitor_FieldGet___VtxWritePtr(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImDrawList_Visitor_FieldSet___VtxWritePtr(void* self, ImDrawVert* value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static ushort* TitanImGui_ImDrawList_Visitor_FieldGet___IdxWritePtr(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImDrawList_Visitor_FieldSet___IdxWritePtr(void* self, ushort* value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static float TitanImGui_ImDrawList_Visitor_FieldGet___FringeScale(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImDrawList_Visitor_FieldSet___FringeScale(void* self, float value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static sbyte* TitanImGui_ImDrawList_Visitor_FieldGet___OwnerName(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImDrawList_Visitor_FieldSet___OwnerName(void* self, [MarshalAs(UnmanagedType.LPUTF8Str)] string value);
	//Functions
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	extern static ImDrawCmd* TitanImGui_ImDrawList_Visitor_GetCmdBuffer_271826910(void* Self, int* size);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	extern static ushort* TitanImGui_ImDrawList_Visitor_GetIdxBuffer_4083488483(void* Self, int* size);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	extern static ImDrawVert* TitanImGui_ImDrawList_Visitor_GetVtxBuffer_1699165095(void* Self, int* size);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	extern static void TitanImGui_ImDrawList_Visitor_PushClipRect_1087775703(void* Self, EngineNS.Vector2* clip_rect_min,EngineNS.Vector2* clip_rect_max,bool intersect_with_current_clip_rect);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	extern static void TitanImGui_ImDrawList_Visitor_PushClipRectFullScreen_2960189489(void* Self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	extern static void TitanImGui_ImDrawList_Visitor_PopClipRect_2960189489(void* Self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	extern static void TitanImGui_ImDrawList_Visitor_PushTexture_2544618575(void* Self, ImTextureRef tex_ref);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	extern static void TitanImGui_ImDrawList_Visitor_PopTexture_2960189489(void* Self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	extern static EngineNS.Vector2 TitanImGui_ImDrawList_Visitor_GetClipRectMin_3443252160(void* Self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	extern static EngineNS.Vector2 TitanImGui_ImDrawList_Visitor_GetClipRectMax_3443252160(void* Self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	extern static void TitanImGui_ImDrawList_Visitor_AddLine_385528029(void* Self, EngineNS.Vector2* p1,EngineNS.Vector2* p2,uint col,float thickness);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	extern static void TitanImGui_ImDrawList_Visitor_AddRect_3863083696(void* Self, EngineNS.Vector2* p_min,EngineNS.Vector2* p_max,uint col,float rounding,ImDrawFlags_ flags,float thickness);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	extern static void TitanImGui_ImDrawList_Visitor_AddRectFilled_1833191196(void* Self, EngineNS.Vector2* p_min,EngineNS.Vector2* p_max,uint col,float rounding,ImDrawFlags_ flags);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	extern static void TitanImGui_ImDrawList_Visitor_AddRectFilledMultiColor_924535741(void* Self, EngineNS.Vector2* p_min,EngineNS.Vector2* p_max,uint col_upr_left,uint col_upr_right,uint col_bot_right,uint col_bot_left);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	extern static void TitanImGui_ImDrawList_Visitor_AddQuad_2574879389(void* Self, EngineNS.Vector2* p1,EngineNS.Vector2* p2,EngineNS.Vector2* p3,EngineNS.Vector2* p4,uint col,float thickness);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	extern static void TitanImGui_ImDrawList_Visitor_AddQuadFilled_498068585(void* Self, EngineNS.Vector2* p1,EngineNS.Vector2* p2,EngineNS.Vector2* p3,EngineNS.Vector2* p4,uint col);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	extern static void TitanImGui_ImDrawList_Visitor_AddTriangle_4155471270(void* Self, EngineNS.Vector2* p1,EngineNS.Vector2* p2,EngineNS.Vector2* p3,uint col,float thickness);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	extern static void TitanImGui_ImDrawList_Visitor_AddTriangleFilled_3736051506(void* Self, EngineNS.Vector2* p1,EngineNS.Vector2* p2,EngineNS.Vector2* p3,uint col);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	extern static void TitanImGui_ImDrawList_Visitor_AddCircle_3914253043(void* Self, EngineNS.Vector2* center,float radius,uint col,int num_segments,float thickness);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	extern static void TitanImGui_ImDrawList_Visitor_AddCircleFilled_23506951(void* Self, EngineNS.Vector2* center,float radius,uint col,int num_segments);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	extern static void TitanImGui_ImDrawList_Visitor_AddNgon_3914253043(void* Self, EngineNS.Vector2* center,float radius,uint col,int num_segments,float thickness);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	extern static void TitanImGui_ImDrawList_Visitor_AddNgonFilled_23506951(void* Self, EngineNS.Vector2* center,float radius,uint col,int num_segments);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	extern static void TitanImGui_ImDrawList_Visitor_AddEllipse_2337479130(void* Self, EngineNS.Vector2* center,EngineNS.Vector2* radius,uint col,float rot,int num_segments,float thickness);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	extern static void TitanImGui_ImDrawList_Visitor_AddEllipseFilled_4133031662(void* Self, EngineNS.Vector2* center,EngineNS.Vector2* radius,uint col,float rot,int num_segments);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	extern static void TitanImGui_ImDrawList_Visitor_AddText_374383570(void* Self, EngineNS.Vector2* pos,uint col,[MarshalAs(UnmanagedType.LPUTF8Str)] string text_begin,[MarshalAs(UnmanagedType.LPUTF8Str)] string text_end);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	extern static void TitanImGui_ImDrawList_Visitor_AddText_3346644164(void* Self, ImFont font,float font_size,EngineNS.Vector2* pos,uint col,[MarshalAs(UnmanagedType.LPUTF8Str)] string text_begin,[MarshalAs(UnmanagedType.LPUTF8Str)] string text_end,float wrap_width,EngineNS.Vector4* cpu_fine_clip_rect);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	extern static void TitanImGui_ImDrawList_Visitor_AddBezierCubic_184805550(void* Self, EngineNS.Vector2* p1,EngineNS.Vector2* p2,EngineNS.Vector2* p3,EngineNS.Vector2* p4,uint col,float thickness,int num_segments);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	extern static void TitanImGui_ImDrawList_Visitor_AddBezierQuadratic_1228503143(void* Self, EngineNS.Vector2* p1,EngineNS.Vector2* p2,EngineNS.Vector2* p3,uint col,float thickness,int num_segments);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	extern static void TitanImGui_ImDrawList_Visitor_AddPolyline_432970084(void* Self, EngineNS.Vector2* points,int num_points,uint col,ImDrawFlags_ flags,float thickness);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	extern static void TitanImGui_ImDrawList_Visitor_AddConvexPolyFilled_173088695(void* Self, EngineNS.Vector2* points,int num_points,uint col);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	extern static void TitanImGui_ImDrawList_Visitor_AddConcavePolyFilled_173088695(void* Self, EngineNS.Vector2* points,int num_points,uint col);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	extern static void TitanImGui_ImDrawList_Visitor_AddImage_2001392767(void* Self, ImTextureRef tex_ref,EngineNS.Vector2* p_min,EngineNS.Vector2* p_max,EngineNS.Vector2* uv_min,EngineNS.Vector2* uv_max,uint col);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	extern static void TitanImGui_ImDrawList_Visitor_AddImageQuad_4052651007(void* Self, ImTextureRef tex_ref,EngineNS.Vector2* p1,EngineNS.Vector2* p2,EngineNS.Vector2* p3,EngineNS.Vector2* p4,EngineNS.Vector2* uv1,EngineNS.Vector2* uv2,EngineNS.Vector2* uv3,EngineNS.Vector2* uv4,uint col);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	extern static void TitanImGui_ImDrawList_Visitor_AddImageRounded_7601998(void* Self, ImTextureRef tex_ref,EngineNS.Vector2* p_min,EngineNS.Vector2* p_max,EngineNS.Vector2* uv_min,EngineNS.Vector2* uv_max,uint col,float rounding,ImDrawFlags_ flags);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	extern static void TitanImGui_ImDrawList_Visitor_PathClear_2960189489(void* Self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	extern static void TitanImGui_ImDrawList_Visitor_PathLineTo_2086025684(void* Self, EngineNS.Vector2* pos);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	extern static void TitanImGui_ImDrawList_Visitor_PathLineToMergeDuplicate_2086025684(void* Self, EngineNS.Vector2* pos);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	extern static void TitanImGui_ImDrawList_Visitor_PathFillConvex_3543463401(void* Self, uint col);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	extern static void TitanImGui_ImDrawList_Visitor_PathFillConcave_3543463401(void* Self, uint col);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	extern static void TitanImGui_ImDrawList_Visitor_PathStroke_3703851534(void* Self, uint col,ImDrawFlags_ flags,float thickness);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	extern static void TitanImGui_ImDrawList_Visitor_PathArcTo_1172245843(void* Self, EngineNS.Vector2* center,float radius,float a_min,float a_max,int num_segments);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	extern static void TitanImGui_ImDrawList_Visitor_PathArcToFast_64231906(void* Self, EngineNS.Vector2* center,float radius,int a_min_of_12,int a_max_of_12);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	extern static void TitanImGui_ImDrawList_Visitor_PathEllipticalArcTo_1049904282(void* Self, EngineNS.Vector2* center,EngineNS.Vector2* radius,float rot,float a_min,float a_max,int num_segments);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	extern static void TitanImGui_ImDrawList_Visitor_PathBezierCubicCurveTo_1701830995(void* Self, EngineNS.Vector2* p2,EngineNS.Vector2* p3,EngineNS.Vector2* p4,int num_segments);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	extern static void TitanImGui_ImDrawList_Visitor_PathBezierQuadraticCurveTo_2431234604(void* Self, EngineNS.Vector2* p2,EngineNS.Vector2* p3,int num_segments);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	extern static void TitanImGui_ImDrawList_Visitor_PathRect_1664922888(void* Self, EngineNS.Vector2* rect_min,EngineNS.Vector2* rect_max,float rounding,ImDrawFlags_ flags);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	extern static void TitanImGui_ImDrawList_Visitor_AddCallback_933021736(void* Self, FDelegate_ImDrawCallback callback,void* userdata,IntPtr userdata_size);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	extern static void TitanImGui_ImDrawList_Visitor_AddDrawCmd_2960189489(void* Self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	extern static ImDrawList* TitanImGui_ImDrawList_Visitor_CloneOutput_4178288180(void* Self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	extern static void TitanImGui_ImDrawList_Visitor_ChannelsSplit_4038704236(void* Self, int count);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	extern static void TitanImGui_ImDrawList_Visitor_ChannelsMerge_2960189489(void* Self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	extern static void TitanImGui_ImDrawList_Visitor_ChannelsSetCurrent_4038704236(void* Self, int n);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	extern static void TitanImGui_ImDrawList_Visitor_PrimReserve_3539386109(void* Self, int idx_count,int vtx_count);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	extern static void TitanImGui_ImDrawList_Visitor_PrimUnreserve_3539386109(void* Self, int idx_count,int vtx_count);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	extern static void TitanImGui_ImDrawList_Visitor_PrimRect_2036564649(void* Self, EngineNS.Vector2* a,EngineNS.Vector2* b,uint col);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	extern static void TitanImGui_ImDrawList_Visitor_PrimRectUV_498068585(void* Self, EngineNS.Vector2* a,EngineNS.Vector2* b,EngineNS.Vector2* uv_a,EngineNS.Vector2* uv_b,uint col);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	extern static void TitanImGui_ImDrawList_Visitor_PrimQuadUV_40166121(void* Self, EngineNS.Vector2* a,EngineNS.Vector2* b,EngineNS.Vector2* c,EngineNS.Vector2* d,EngineNS.Vector2* uv_a,EngineNS.Vector2* uv_b,EngineNS.Vector2* uv_c,EngineNS.Vector2* uv_d,uint col);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	extern static void TitanImGui_ImDrawList_Visitor_PrimWriteVtx_2036564649(void* Self, EngineNS.Vector2* pos,EngineNS.Vector2* uv,uint col);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	extern static void TitanImGui_ImDrawList_Visitor_PrimWriteIdx_2016656406(void* Self, ushort idx);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	extern static void TitanImGui_ImDrawList_Visitor_PrimVtx_2036564649(void* Self, EngineNS.Vector2* pos,EngineNS.Vector2* uv,uint col);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	extern static void TitanImGui_ImDrawList_Visitor_PushTextureID_2544618575(void* Self, ImTextureRef tex_ref);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	extern static void TitanImGui_ImDrawList_Visitor_PopTextureID_2960189489(void* Self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	extern static void TitanImGui_ImDrawList_Visitor__ResetForNewFrame_2960189489(void* Self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	extern static void TitanImGui_ImDrawList_Visitor__ClearFreeMemory_2960189489(void* Self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	extern static void TitanImGui_ImDrawList_Visitor__PopUnusedDrawCmd_2960189489(void* Self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	extern static void TitanImGui_ImDrawList_Visitor__TryMergeDrawCmds_2960189489(void* Self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	extern static void TitanImGui_ImDrawList_Visitor__OnChangedClipRect_2960189489(void* Self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	extern static void TitanImGui_ImDrawList_Visitor__OnChangedTexture_2960189489(void* Self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	extern static void TitanImGui_ImDrawList_Visitor__OnChangedVtxOffset_2960189489(void* Self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	extern static void TitanImGui_ImDrawList_Visitor__SetTexture_2544618575(void* Self, ImTextureRef tex_ref);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	extern static int TitanImGui_ImDrawList_Visitor__CalcCircleAutoSegmentCount_1740070591(void* Self, float radius);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	extern static void TitanImGui_ImDrawList_Visitor__PathArcToFastEx_792707599(void* Self, EngineNS.Vector2* center,float radius,int a_min_sample,int a_max_sample,int a_step);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	extern static void TitanImGui_ImDrawList_Visitor__PathArcToN_1172245843(void* Self, EngineNS.Vector2* center,float radius,float a_min,float a_max,int num_segments);
	//Cast
	#endregion
}
