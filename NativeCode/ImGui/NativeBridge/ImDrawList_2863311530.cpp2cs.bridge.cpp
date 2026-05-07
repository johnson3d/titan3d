//generated from legacy CppWeaving ImGui surface; owned by TitanImGuiBridge
#include "../../../Core.Window/pch.h"
#if defined(HasModule_ImGui)
#include "../imgui.h"
#include "../TitanImGuiBridge.h"


#define new VNEW


namespace 
{
	struct ImDrawList_Visitor
	{
		static inline int FieldGet__Flags(ImDrawList* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<int>();
			}
			return (int)self->Flags;
		}
		static inline void FieldSet__Flags(ImDrawList* self, int value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->Flags = value;
		}
		static inline unsigned int FieldGet___VtxCurrentIdx(ImDrawList* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<unsigned int>();
			}
			return (unsigned int)self->_VtxCurrentIdx;
		}
		static inline void FieldSet___VtxCurrentIdx(ImDrawList* self, unsigned int value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->_VtxCurrentIdx = value;
		}
		static inline ImDrawVert* FieldGet___VtxWritePtr(ImDrawList* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<ImDrawVert*>();
			}
			return (ImDrawVert*)self->_VtxWritePtr;
		}
		static inline void FieldSet___VtxWritePtr(ImDrawList* self, ImDrawVert* value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->_VtxWritePtr = value;
		}
		static inline unsigned short* FieldGet___IdxWritePtr(ImDrawList* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<unsigned short*>();
			}
			return (unsigned short*)self->_IdxWritePtr;
		}
		static inline void FieldSet___IdxWritePtr(ImDrawList* self, unsigned short* value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->_IdxWritePtr = value;
		}
		static inline float FieldGet___FringeScale(ImDrawList* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<float>();
			}
			return (float)self->_FringeScale;
		}
		static inline void FieldSet___FringeScale(ImDrawList* self, float value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->_FringeScale = value;
		}
		static inline char* FieldGet___OwnerName(ImDrawList* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<char*>();
			}
			return (char*)self->_OwnerName;
		}
		static inline void FieldSet___OwnerName(ImDrawList* self, char* value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->_OwnerName = value;
		}
		static inline ImDrawCmd* GetCmdBuffer(ImDrawList* self, int* size)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<ImDrawCmd*>();
			}
			return (ImDrawCmd*)self->GetCmdBuffer(size);
		}
		static inline unsigned short* GetIdxBuffer(ImDrawList* self, int* size)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<unsigned short*>();
			}
			return (unsigned short*)self->GetIdxBuffer(size);
		}
		static inline ImDrawVert* GetVtxBuffer(ImDrawList* self, int* size)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<ImDrawVert*>();
			}
			return (ImDrawVert*)self->GetVtxBuffer(size);
		}
		static inline void PushClipRect(ImDrawList* self, const ImVec2* clip_rect_min,const ImVec2* clip_rect_max,bool intersect_with_current_clip_rect)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<void>();
			}
			return (void)self->PushClipRect(*clip_rect_min, *clip_rect_max, intersect_with_current_clip_rect);
		}
		static inline void PushClipRectFullScreen(ImDrawList* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<void>();
			}
			return (void)self->PushClipRectFullScreen();
		}
		static inline void PopClipRect(ImDrawList* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<void>();
			}
			return (void)self->PopClipRect();
		}
		static inline void PushTexture(ImDrawList* self, ImTextureRef tex_ref)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<void>();
			}
			return (void)self->PushTexture(tex_ref);
		}
		static inline void PopTexture(ImDrawList* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<void>();
			}
			return (void)self->PopTexture();
		}
		static inline ImVec2 GetClipRectMin(ImDrawList* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<ImVec2>();
			}
			return (ImVec2)self->GetClipRectMin();
		}
		static inline ImVec2 GetClipRectMax(ImDrawList* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<ImVec2>();
			}
			return (ImVec2)self->GetClipRectMax();
		}
		static inline void AddLine(ImDrawList* self, const ImVec2* p1,const ImVec2* p2,unsigned int col,float thickness)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<void>();
			}
			return (void)self->AddLine(*p1, *p2, col, thickness);
		}
		static inline void AddRect(ImDrawList* self, const ImVec2* p_min,const ImVec2* p_max,unsigned int col,float rounding,int flags,float thickness)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<void>();
			}
			return (void)self->AddRect(*p_min, *p_max, col, rounding, flags, thickness);
		}
		static inline void AddRectFilled(ImDrawList* self, const ImVec2* p_min,const ImVec2* p_max,unsigned int col,float rounding,int flags)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<void>();
			}
			return (void)self->AddRectFilled(*p_min, *p_max, col, rounding, flags);
		}
		static inline void AddRectFilledMultiColor(ImDrawList* self, const ImVec2* p_min,const ImVec2* p_max,unsigned int col_upr_left,unsigned int col_upr_right,unsigned int col_bot_right,unsigned int col_bot_left)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<void>();
			}
			return (void)self->AddRectFilledMultiColor(*p_min, *p_max, col_upr_left, col_upr_right, col_bot_right, col_bot_left);
		}
		static inline void AddQuad(ImDrawList* self, const ImVec2* p1,const ImVec2* p2,const ImVec2* p3,const ImVec2* p4,unsigned int col,float thickness)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<void>();
			}
			return (void)self->AddQuad(*p1, *p2, *p3, *p4, col, thickness);
		}
		static inline void AddQuadFilled(ImDrawList* self, const ImVec2* p1,const ImVec2* p2,const ImVec2* p3,const ImVec2* p4,unsigned int col)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<void>();
			}
			return (void)self->AddQuadFilled(*p1, *p2, *p3, *p4, col);
		}
		static inline void AddTriangle(ImDrawList* self, const ImVec2* p1,const ImVec2* p2,const ImVec2* p3,unsigned int col,float thickness)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<void>();
			}
			return (void)self->AddTriangle(*p1, *p2, *p3, col, thickness);
		}
		static inline void AddTriangleFilled(ImDrawList* self, const ImVec2* p1,const ImVec2* p2,const ImVec2* p3,unsigned int col)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<void>();
			}
			return (void)self->AddTriangleFilled(*p1, *p2, *p3, col);
		}
		static inline void AddCircle(ImDrawList* self, const ImVec2* center,float radius,unsigned int col,int num_segments,float thickness)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<void>();
			}
			return (void)self->AddCircle(*center, radius, col, num_segments, thickness);
		}
		static inline void AddCircleFilled(ImDrawList* self, const ImVec2* center,float radius,unsigned int col,int num_segments)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<void>();
			}
			return (void)self->AddCircleFilled(*center, radius, col, num_segments);
		}
		static inline void AddNgon(ImDrawList* self, const ImVec2* center,float radius,unsigned int col,int num_segments,float thickness)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<void>();
			}
			return (void)self->AddNgon(*center, radius, col, num_segments, thickness);
		}
		static inline void AddNgonFilled(ImDrawList* self, const ImVec2* center,float radius,unsigned int col,int num_segments)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<void>();
			}
			return (void)self->AddNgonFilled(*center, radius, col, num_segments);
		}
		static inline void AddEllipse(ImDrawList* self, const ImVec2* center,const ImVec2* radius,unsigned int col,float rot,int num_segments,float thickness)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<void>();
			}
			return (void)self->AddEllipse(*center, *radius, col, rot, num_segments, thickness);
		}
		static inline void AddEllipseFilled(ImDrawList* self, const ImVec2* center,const ImVec2* radius,unsigned int col,float rot,int num_segments)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<void>();
			}
			return (void)self->AddEllipseFilled(*center, *radius, col, rot, num_segments);
		}
		static inline void AddText(ImDrawList* self, const ImVec2* pos,unsigned int col,const char* text_begin,const char* text_end)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<void>();
			}
			return (void)self->AddText(*pos, col, text_begin, text_end);
		}
		static inline void AddText(ImDrawList* self, ImFont* font,float font_size,const ImVec2* pos,unsigned int col,const char* text_begin,const char* text_end,float wrap_width,const ImVec4* cpu_fine_clip_rect)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<void>();
			}
			return (void)self->AddText(font, font_size, *pos, col, text_begin, text_end, wrap_width, cpu_fine_clip_rect);
		}
		static inline void AddBezierCubic(ImDrawList* self, const ImVec2* p1,const ImVec2* p2,const ImVec2* p3,const ImVec2* p4,unsigned int col,float thickness,int num_segments)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<void>();
			}
			return (void)self->AddBezierCubic(*p1, *p2, *p3, *p4, col, thickness, num_segments);
		}
		static inline void AddBezierQuadratic(ImDrawList* self, const ImVec2* p1,const ImVec2* p2,const ImVec2* p3,unsigned int col,float thickness,int num_segments)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<void>();
			}
			return (void)self->AddBezierQuadratic(*p1, *p2, *p3, col, thickness, num_segments);
		}
		static inline void AddPolyline(ImDrawList* self, const ImVec2* points,int num_points,unsigned int col,int flags,float thickness)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<void>();
			}
			return (void)self->AddPolyline(points, num_points, col, flags, thickness);
		}
		static inline void AddConvexPolyFilled(ImDrawList* self, const ImVec2* points,int num_points,unsigned int col)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<void>();
			}
			return (void)self->AddConvexPolyFilled(points, num_points, col);
		}
		static inline void AddConcavePolyFilled(ImDrawList* self, const ImVec2* points,int num_points,unsigned int col)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<void>();
			}
			return (void)self->AddConcavePolyFilled(points, num_points, col);
		}
		static inline void AddImage(ImDrawList* self, ImTextureRef tex_ref,const ImVec2* p_min,const ImVec2* p_max,const ImVec2* uv_min,const ImVec2* uv_max,unsigned int col)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<void>();
			}
			return (void)self->AddImage(tex_ref, *p_min, *p_max, *uv_min, *uv_max, col);
		}
		static inline void AddImageQuad(ImDrawList* self, ImTextureRef tex_ref,const ImVec2* p1,const ImVec2* p2,const ImVec2* p3,const ImVec2* p4,const ImVec2* uv1,const ImVec2* uv2,const ImVec2* uv3,const ImVec2* uv4,unsigned int col)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<void>();
			}
			return (void)self->AddImageQuad(tex_ref, *p1, *p2, *p3, *p4, *uv1, *uv2, *uv3, *uv4, col);
		}
		static inline void AddImageRounded(ImDrawList* self, ImTextureRef tex_ref,const ImVec2* p_min,const ImVec2* p_max,const ImVec2* uv_min,const ImVec2* uv_max,unsigned int col,float rounding,int flags)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<void>();
			}
			return (void)self->AddImageRounded(tex_ref, *p_min, *p_max, *uv_min, *uv_max, col, rounding, flags);
		}
		static inline void PathClear(ImDrawList* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<void>();
			}
			return (void)self->PathClear();
		}
		static inline void PathLineTo(ImDrawList* self, const ImVec2* pos)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<void>();
			}
			return (void)self->PathLineTo(*pos);
		}
		static inline void PathLineToMergeDuplicate(ImDrawList* self, const ImVec2* pos)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<void>();
			}
			return (void)self->PathLineToMergeDuplicate(*pos);
		}
		static inline void PathFillConvex(ImDrawList* self, unsigned int col)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<void>();
			}
			return (void)self->PathFillConvex(col);
		}
		static inline void PathFillConcave(ImDrawList* self, unsigned int col)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<void>();
			}
			return (void)self->PathFillConcave(col);
		}
		static inline void PathStroke(ImDrawList* self, unsigned int col,int flags,float thickness)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<void>();
			}
			return (void)self->PathStroke(col, flags, thickness);
		}
		static inline void PathArcTo(ImDrawList* self, const ImVec2* center,float radius,float a_min,float a_max,int num_segments)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<void>();
			}
			return (void)self->PathArcTo(*center, radius, a_min, a_max, num_segments);
		}
		static inline void PathArcToFast(ImDrawList* self, const ImVec2* center,float radius,int a_min_of_12,int a_max_of_12)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<void>();
			}
			return (void)self->PathArcToFast(*center, radius, a_min_of_12, a_max_of_12);
		}
		static inline void PathEllipticalArcTo(ImDrawList* self, const ImVec2* center,const ImVec2* radius,float rot,float a_min,float a_max,int num_segments)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<void>();
			}
			return (void)self->PathEllipticalArcTo(*center, *radius, rot, a_min, a_max, num_segments);
		}
		static inline void PathBezierCubicCurveTo(ImDrawList* self, const ImVec2* p2,const ImVec2* p3,const ImVec2* p4,int num_segments)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<void>();
			}
			return (void)self->PathBezierCubicCurveTo(*p2, *p3, *p4, num_segments);
		}
		static inline void PathBezierQuadraticCurveTo(ImDrawList* self, const ImVec2* p2,const ImVec2* p3,int num_segments)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<void>();
			}
			return (void)self->PathBezierQuadraticCurveTo(*p2, *p3, num_segments);
		}
		static inline void PathRect(ImDrawList* self, const ImVec2* rect_min,const ImVec2* rect_max,float rounding,int flags)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<void>();
			}
			return (void)self->PathRect(*rect_min, *rect_max, rounding, flags);
		}
		static inline void AddCallback(ImDrawList* self, void (*callback)(const ImDrawList *, const ImDrawCmd *),void* userdata,size_t userdata_size)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<void>();
			}
			return (void)self->AddCallback(callback, userdata, userdata_size);
		}
		static inline void AddDrawCmd(ImDrawList* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<void>();
			}
			return (void)self->AddDrawCmd();
		}
		static inline ImDrawList* CloneOutput(ImDrawList* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<ImDrawList*>();
			}
			return (ImDrawList*)self->CloneOutput();
		}
		static inline void ChannelsSplit(ImDrawList* self, int count)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<void>();
			}
			return (void)self->ChannelsSplit(count);
		}
		static inline void ChannelsMerge(ImDrawList* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<void>();
			}
			return (void)self->ChannelsMerge();
		}
		static inline void ChannelsSetCurrent(ImDrawList* self, int n)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<void>();
			}
			return (void)self->ChannelsSetCurrent(n);
		}
		static inline void PrimReserve(ImDrawList* self, int idx_count,int vtx_count)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<void>();
			}
			return (void)self->PrimReserve(idx_count, vtx_count);
		}
		static inline void PrimUnreserve(ImDrawList* self, int idx_count,int vtx_count)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<void>();
			}
			return (void)self->PrimUnreserve(idx_count, vtx_count);
		}
		static inline void PrimRect(ImDrawList* self, const ImVec2* a,const ImVec2* b,unsigned int col)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<void>();
			}
			return (void)self->PrimRect(*a, *b, col);
		}
		static inline void PrimRectUV(ImDrawList* self, const ImVec2* a,const ImVec2* b,const ImVec2* uv_a,const ImVec2* uv_b,unsigned int col)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<void>();
			}
			return (void)self->PrimRectUV(*a, *b, *uv_a, *uv_b, col);
		}
		static inline void PrimQuadUV(ImDrawList* self, const ImVec2* a,const ImVec2* b,const ImVec2* c,const ImVec2* d,const ImVec2* uv_a,const ImVec2* uv_b,const ImVec2* uv_c,const ImVec2* uv_d,unsigned int col)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<void>();
			}
			return (void)self->PrimQuadUV(*a, *b, *c, *d, *uv_a, *uv_b, *uv_c, *uv_d, col);
		}
		static inline void PrimWriteVtx(ImDrawList* self, const ImVec2* pos,const ImVec2* uv,unsigned int col)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<void>();
			}
			return (void)self->PrimWriteVtx(*pos, *uv, col);
		}
		static inline void PrimWriteIdx(ImDrawList* self, unsigned short idx)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<void>();
			}
			return (void)self->PrimWriteIdx(idx);
		}
		static inline void PrimVtx(ImDrawList* self, const ImVec2* pos,const ImVec2* uv,unsigned int col)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<void>();
			}
			return (void)self->PrimVtx(*pos, *uv, col);
		}
		static inline void PushTextureID(ImDrawList* self, ImTextureRef tex_ref)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<void>();
			}
			return (void)self->PushTextureID(tex_ref);
		}
		static inline void PopTextureID(ImDrawList* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<void>();
			}
			return (void)self->PopTextureID();
		}
		static inline void _ResetForNewFrame(ImDrawList* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<void>();
			}
			return (void)self->_ResetForNewFrame();
		}
		static inline void _ClearFreeMemory(ImDrawList* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<void>();
			}
			return (void)self->_ClearFreeMemory();
		}
		static inline void _PopUnusedDrawCmd(ImDrawList* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<void>();
			}
			return (void)self->_PopUnusedDrawCmd();
		}
		static inline void _TryMergeDrawCmds(ImDrawList* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<void>();
			}
			return (void)self->_TryMergeDrawCmds();
		}
		static inline void _OnChangedClipRect(ImDrawList* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<void>();
			}
			return (void)self->_OnChangedClipRect();
		}
		static inline void _OnChangedTexture(ImDrawList* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<void>();
			}
			return (void)self->_OnChangedTexture();
		}
		static inline void _OnChangedVtxOffset(ImDrawList* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<void>();
			}
			return (void)self->_OnChangedVtxOffset();
		}
		static inline void _SetTexture(ImDrawList* self, ImTextureRef tex_ref)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<void>();
			}
			return (void)self->_SetTexture(tex_ref);
		}
		static inline int _CalcCircleAutoSegmentCount(ImDrawList* self, float radius)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<int>();
			}
			return (int)self->_CalcCircleAutoSegmentCount(radius);
		}
		static inline void _PathArcToFastEx(ImDrawList* self, const ImVec2* center,float radius,int a_min_sample,int a_max_sample,int a_step)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<void>();
			}
			return (void)self->_PathArcToFastEx(*center, radius, a_min_sample, a_max_sample, a_step);
		}
		static inline void _PathArcToN(ImDrawList* self, const ImVec2* center,float radius,float a_min,float a_max,int num_segments)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<void>();
			}
			return (void)self->_PathArcToN(*center, radius, a_min, a_max, num_segments);
		}
	};
}




extern "C" VFX_API EngineNS::FRttiStruct* TitanImGui_ImDrawList_Visitor_GetTypeRtti()
{
	return GetClassObject<ImDrawList>();
}


extern "C" VFX_API int TitanImGui_ImDrawList_Visitor_FieldGet__Flags(ImDrawList* self)
{
	return ImDrawList_Visitor::FieldGet__Flags(self);
}
extern "C" VFX_API void TitanImGui_ImDrawList_Visitor_FieldSet__Flags(ImDrawList* self, int value)
{
	ImDrawList_Visitor::FieldSet__Flags(self, value);
}
extern "C" VFX_API unsigned int TitanImGui_ImDrawList_Visitor_FieldGet___VtxCurrentIdx(ImDrawList* self)
{
	return ImDrawList_Visitor::FieldGet___VtxCurrentIdx(self);
}
extern "C" VFX_API void TitanImGui_ImDrawList_Visitor_FieldSet___VtxCurrentIdx(ImDrawList* self, unsigned int value)
{
	ImDrawList_Visitor::FieldSet___VtxCurrentIdx(self, value);
}
extern "C" VFX_API ImDrawVert* TitanImGui_ImDrawList_Visitor_FieldGet___VtxWritePtr(ImDrawList* self)
{
	return ImDrawList_Visitor::FieldGet___VtxWritePtr(self);
}
extern "C" VFX_API void TitanImGui_ImDrawList_Visitor_FieldSet___VtxWritePtr(ImDrawList* self, ImDrawVert* value)
{
	ImDrawList_Visitor::FieldSet___VtxWritePtr(self, value);
}
extern "C" VFX_API unsigned short* TitanImGui_ImDrawList_Visitor_FieldGet___IdxWritePtr(ImDrawList* self)
{
	return ImDrawList_Visitor::FieldGet___IdxWritePtr(self);
}
extern "C" VFX_API void TitanImGui_ImDrawList_Visitor_FieldSet___IdxWritePtr(ImDrawList* self, unsigned short* value)
{
	ImDrawList_Visitor::FieldSet___IdxWritePtr(self, value);
}
extern "C" VFX_API float TitanImGui_ImDrawList_Visitor_FieldGet___FringeScale(ImDrawList* self)
{
	return ImDrawList_Visitor::FieldGet___FringeScale(self);
}
extern "C" VFX_API void TitanImGui_ImDrawList_Visitor_FieldSet___FringeScale(ImDrawList* self, float value)
{
	ImDrawList_Visitor::FieldSet___FringeScale(self, value);
}
extern "C" VFX_API char* TitanImGui_ImDrawList_Visitor_FieldGet___OwnerName(ImDrawList* self)
{
	return ImDrawList_Visitor::FieldGet___OwnerName(self);
}
extern "C" VFX_API void TitanImGui_ImDrawList_Visitor_FieldSet___OwnerName(ImDrawList* self, char* value)
{
	ImDrawList_Visitor::FieldSet___OwnerName(self, value);
}


extern "C" VFX_API ImDrawCmd* TitanImGui_ImDrawList_Visitor_GetCmdBuffer_271826910(ImDrawList* self, int* size)
{
	return ImDrawList_Visitor::GetCmdBuffer(self, size);
}
extern "C" VFX_API unsigned short* TitanImGui_ImDrawList_Visitor_GetIdxBuffer_4083488483(ImDrawList* self, int* size)
{
	return ImDrawList_Visitor::GetIdxBuffer(self, size);
}
extern "C" VFX_API ImDrawVert* TitanImGui_ImDrawList_Visitor_GetVtxBuffer_1699165095(ImDrawList* self, int* size)
{
	return ImDrawList_Visitor::GetVtxBuffer(self, size);
}
extern "C" VFX_API void TitanImGui_ImDrawList_Visitor_PushClipRect_1087775703(ImDrawList* self, const ImVec2* clip_rect_min,const ImVec2* clip_rect_max,bool intersect_with_current_clip_rect)
{
	return ImDrawList_Visitor::PushClipRect(self, clip_rect_min, clip_rect_max, intersect_with_current_clip_rect);
}
extern "C" VFX_API void TitanImGui_ImDrawList_Visitor_PushClipRectFullScreen_2960189489(ImDrawList* self)
{
	return ImDrawList_Visitor::PushClipRectFullScreen(self);
}
extern "C" VFX_API void TitanImGui_ImDrawList_Visitor_PopClipRect_2960189489(ImDrawList* self)
{
	return ImDrawList_Visitor::PopClipRect(self);
}
extern "C" VFX_API void TitanImGui_ImDrawList_Visitor_PushTexture_2544618575(ImDrawList* self, ImTextureRef tex_ref)
{
	return ImDrawList_Visitor::PushTexture(self, tex_ref);
}
extern "C" VFX_API void TitanImGui_ImDrawList_Visitor_PopTexture_2960189489(ImDrawList* self)
{
	return ImDrawList_Visitor::PopTexture(self);
}
extern "C" VFX_API v3dVector2_t TitanImGui_ImDrawList_Visitor_GetClipRectMin_3443252160(ImDrawList* self)
{
	auto tmp_result = ImDrawList_Visitor::GetClipRectMin(self);
	return EngineNS::VReturnValueMarshal<ImVec2,v3dVector2_t>(tmp_result);
}
extern "C" VFX_API v3dVector2_t TitanImGui_ImDrawList_Visitor_GetClipRectMax_3443252160(ImDrawList* self)
{
	auto tmp_result = ImDrawList_Visitor::GetClipRectMax(self);
	return EngineNS::VReturnValueMarshal<ImVec2,v3dVector2_t>(tmp_result);
}
extern "C" VFX_API void TitanImGui_ImDrawList_Visitor_AddLine_385528029(ImDrawList* self, const ImVec2* p1,const ImVec2* p2,unsigned int col,float thickness)
{
	return ImDrawList_Visitor::AddLine(self, p1, p2, col, thickness);
}
extern "C" VFX_API void TitanImGui_ImDrawList_Visitor_AddRect_3863083696(ImDrawList* self, const ImVec2* p_min,const ImVec2* p_max,unsigned int col,float rounding,int flags,float thickness)
{
	return ImDrawList_Visitor::AddRect(self, p_min, p_max, col, rounding, flags, thickness);
}
extern "C" VFX_API void TitanImGui_ImDrawList_Visitor_AddRectFilled_1833191196(ImDrawList* self, const ImVec2* p_min,const ImVec2* p_max,unsigned int col,float rounding,int flags)
{
	return ImDrawList_Visitor::AddRectFilled(self, p_min, p_max, col, rounding, flags);
}
extern "C" VFX_API void TitanImGui_ImDrawList_Visitor_AddRectFilledMultiColor_924535741(ImDrawList* self, const ImVec2* p_min,const ImVec2* p_max,unsigned int col_upr_left,unsigned int col_upr_right,unsigned int col_bot_right,unsigned int col_bot_left)
{
	return ImDrawList_Visitor::AddRectFilledMultiColor(self, p_min, p_max, col_upr_left, col_upr_right, col_bot_right, col_bot_left);
}
extern "C" VFX_API void TitanImGui_ImDrawList_Visitor_AddQuad_2574879389(ImDrawList* self, const ImVec2* p1,const ImVec2* p2,const ImVec2* p3,const ImVec2* p4,unsigned int col,float thickness)
{
	return ImDrawList_Visitor::AddQuad(self, p1, p2, p3, p4, col, thickness);
}
extern "C" VFX_API void TitanImGui_ImDrawList_Visitor_AddQuadFilled_498068585(ImDrawList* self, const ImVec2* p1,const ImVec2* p2,const ImVec2* p3,const ImVec2* p4,unsigned int col)
{
	return ImDrawList_Visitor::AddQuadFilled(self, p1, p2, p3, p4, col);
}
extern "C" VFX_API void TitanImGui_ImDrawList_Visitor_AddTriangle_4155471270(ImDrawList* self, const ImVec2* p1,const ImVec2* p2,const ImVec2* p3,unsigned int col,float thickness)
{
	return ImDrawList_Visitor::AddTriangle(self, p1, p2, p3, col, thickness);
}
extern "C" VFX_API void TitanImGui_ImDrawList_Visitor_AddTriangleFilled_3736051506(ImDrawList* self, const ImVec2* p1,const ImVec2* p2,const ImVec2* p3,unsigned int col)
{
	return ImDrawList_Visitor::AddTriangleFilled(self, p1, p2, p3, col);
}
extern "C" VFX_API void TitanImGui_ImDrawList_Visitor_AddCircle_3914253043(ImDrawList* self, const ImVec2* center,float radius,unsigned int col,int num_segments,float thickness)
{
	return ImDrawList_Visitor::AddCircle(self, center, radius, col, num_segments, thickness);
}
extern "C" VFX_API void TitanImGui_ImDrawList_Visitor_AddCircleFilled_23506951(ImDrawList* self, const ImVec2* center,float radius,unsigned int col,int num_segments)
{
	return ImDrawList_Visitor::AddCircleFilled(self, center, radius, col, num_segments);
}
extern "C" VFX_API void TitanImGui_ImDrawList_Visitor_AddNgon_3914253043(ImDrawList* self, const ImVec2* center,float radius,unsigned int col,int num_segments,float thickness)
{
	return ImDrawList_Visitor::AddNgon(self, center, radius, col, num_segments, thickness);
}
extern "C" VFX_API void TitanImGui_ImDrawList_Visitor_AddNgonFilled_23506951(ImDrawList* self, const ImVec2* center,float radius,unsigned int col,int num_segments)
{
	return ImDrawList_Visitor::AddNgonFilled(self, center, radius, col, num_segments);
}
extern "C" VFX_API void TitanImGui_ImDrawList_Visitor_AddEllipse_2337479130(ImDrawList* self, const ImVec2* center,const ImVec2* radius,unsigned int col,float rot,int num_segments,float thickness)
{
	return ImDrawList_Visitor::AddEllipse(self, center, radius, col, rot, num_segments, thickness);
}
extern "C" VFX_API void TitanImGui_ImDrawList_Visitor_AddEllipseFilled_4133031662(ImDrawList* self, const ImVec2* center,const ImVec2* radius,unsigned int col,float rot,int num_segments)
{
	return ImDrawList_Visitor::AddEllipseFilled(self, center, radius, col, rot, num_segments);
}
extern "C" VFX_API void TitanImGui_ImDrawList_Visitor_AddText_374383570(ImDrawList* self, const ImVec2* pos,unsigned int col,const char* text_begin,const char* text_end)
{
	return ImDrawList_Visitor::AddText(self, pos, col, text_begin, text_end);
}
extern "C" VFX_API void TitanImGui_ImDrawList_Visitor_AddText_3346644164(ImDrawList* self, ImFont* font,float font_size,const ImVec2* pos,unsigned int col,const char* text_begin,const char* text_end,float wrap_width,const ImVec4* cpu_fine_clip_rect)
{
	return ImDrawList_Visitor::AddText(self, font, font_size, pos, col, text_begin, text_end, wrap_width, cpu_fine_clip_rect);
}
extern "C" VFX_API void TitanImGui_ImDrawList_Visitor_AddBezierCubic_184805550(ImDrawList* self, const ImVec2* p1,const ImVec2* p2,const ImVec2* p3,const ImVec2* p4,unsigned int col,float thickness,int num_segments)
{
	return ImDrawList_Visitor::AddBezierCubic(self, p1, p2, p3, p4, col, thickness, num_segments);
}
extern "C" VFX_API void TitanImGui_ImDrawList_Visitor_AddBezierQuadratic_1228503143(ImDrawList* self, const ImVec2* p1,const ImVec2* p2,const ImVec2* p3,unsigned int col,float thickness,int num_segments)
{
	return ImDrawList_Visitor::AddBezierQuadratic(self, p1, p2, p3, col, thickness, num_segments);
}
extern "C" VFX_API void TitanImGui_ImDrawList_Visitor_AddPolyline_432970084(ImDrawList* self, const ImVec2* points,int num_points,unsigned int col,int flags,float thickness)
{
	return ImDrawList_Visitor::AddPolyline(self, points, num_points, col, flags, thickness);
}
extern "C" VFX_API void TitanImGui_ImDrawList_Visitor_AddConvexPolyFilled_173088695(ImDrawList* self, const ImVec2* points,int num_points,unsigned int col)
{
	return ImDrawList_Visitor::AddConvexPolyFilled(self, points, num_points, col);
}
extern "C" VFX_API void TitanImGui_ImDrawList_Visitor_AddConcavePolyFilled_173088695(ImDrawList* self, const ImVec2* points,int num_points,unsigned int col)
{
	return ImDrawList_Visitor::AddConcavePolyFilled(self, points, num_points, col);
}
extern "C" VFX_API void TitanImGui_ImDrawList_Visitor_AddImage_2001392767(ImDrawList* self, ImTextureRef tex_ref,const ImVec2* p_min,const ImVec2* p_max,const ImVec2* uv_min,const ImVec2* uv_max,unsigned int col)
{
	return ImDrawList_Visitor::AddImage(self, tex_ref, p_min, p_max, uv_min, uv_max, col);
}
extern "C" VFX_API void TitanImGui_ImDrawList_Visitor_AddImageQuad_4052651007(ImDrawList* self, ImTextureRef tex_ref,const ImVec2* p1,const ImVec2* p2,const ImVec2* p3,const ImVec2* p4,const ImVec2* uv1,const ImVec2* uv2,const ImVec2* uv3,const ImVec2* uv4,unsigned int col)
{
	return ImDrawList_Visitor::AddImageQuad(self, tex_ref, p1, p2, p3, p4, uv1, uv2, uv3, uv4, col);
}
extern "C" VFX_API void TitanImGui_ImDrawList_Visitor_AddImageRounded_7601998(ImDrawList* self, ImTextureRef tex_ref,const ImVec2* p_min,const ImVec2* p_max,const ImVec2* uv_min,const ImVec2* uv_max,unsigned int col,float rounding,int flags)
{
	return ImDrawList_Visitor::AddImageRounded(self, tex_ref, p_min, p_max, uv_min, uv_max, col, rounding, flags);
}
extern "C" VFX_API void TitanImGui_ImDrawList_Visitor_PathClear_2960189489(ImDrawList* self)
{
	return ImDrawList_Visitor::PathClear(self);
}
extern "C" VFX_API void TitanImGui_ImDrawList_Visitor_PathLineTo_2086025684(ImDrawList* self, const ImVec2* pos)
{
	return ImDrawList_Visitor::PathLineTo(self, pos);
}
extern "C" VFX_API void TitanImGui_ImDrawList_Visitor_PathLineToMergeDuplicate_2086025684(ImDrawList* self, const ImVec2* pos)
{
	return ImDrawList_Visitor::PathLineToMergeDuplicate(self, pos);
}
extern "C" VFX_API void TitanImGui_ImDrawList_Visitor_PathFillConvex_3543463401(ImDrawList* self, unsigned int col)
{
	return ImDrawList_Visitor::PathFillConvex(self, col);
}
extern "C" VFX_API void TitanImGui_ImDrawList_Visitor_PathFillConcave_3543463401(ImDrawList* self, unsigned int col)
{
	return ImDrawList_Visitor::PathFillConcave(self, col);
}
extern "C" VFX_API void TitanImGui_ImDrawList_Visitor_PathStroke_3703851534(ImDrawList* self, unsigned int col,int flags,float thickness)
{
	return ImDrawList_Visitor::PathStroke(self, col, flags, thickness);
}
extern "C" VFX_API void TitanImGui_ImDrawList_Visitor_PathArcTo_1172245843(ImDrawList* self, const ImVec2* center,float radius,float a_min,float a_max,int num_segments)
{
	return ImDrawList_Visitor::PathArcTo(self, center, radius, a_min, a_max, num_segments);
}
extern "C" VFX_API void TitanImGui_ImDrawList_Visitor_PathArcToFast_64231906(ImDrawList* self, const ImVec2* center,float radius,int a_min_of_12,int a_max_of_12)
{
	return ImDrawList_Visitor::PathArcToFast(self, center, radius, a_min_of_12, a_max_of_12);
}
extern "C" VFX_API void TitanImGui_ImDrawList_Visitor_PathEllipticalArcTo_1049904282(ImDrawList* self, const ImVec2* center,const ImVec2* radius,float rot,float a_min,float a_max,int num_segments)
{
	return ImDrawList_Visitor::PathEllipticalArcTo(self, center, radius, rot, a_min, a_max, num_segments);
}
extern "C" VFX_API void TitanImGui_ImDrawList_Visitor_PathBezierCubicCurveTo_1701830995(ImDrawList* self, const ImVec2* p2,const ImVec2* p3,const ImVec2* p4,int num_segments)
{
	return ImDrawList_Visitor::PathBezierCubicCurveTo(self, p2, p3, p4, num_segments);
}
extern "C" VFX_API void TitanImGui_ImDrawList_Visitor_PathBezierQuadraticCurveTo_2431234604(ImDrawList* self, const ImVec2* p2,const ImVec2* p3,int num_segments)
{
	return ImDrawList_Visitor::PathBezierQuadraticCurveTo(self, p2, p3, num_segments);
}
extern "C" VFX_API void TitanImGui_ImDrawList_Visitor_PathRect_1664922888(ImDrawList* self, const ImVec2* rect_min,const ImVec2* rect_max,float rounding,int flags)
{
	return ImDrawList_Visitor::PathRect(self, rect_min, rect_max, rounding, flags);
}
extern "C" VFX_API void TitanImGui_ImDrawList_Visitor_AddCallback_933021736(ImDrawList* self, void (*callback)(const ImDrawList *, const ImDrawCmd *),void* userdata,size_t userdata_size)
{
	return ImDrawList_Visitor::AddCallback(self, callback, userdata, userdata_size);
}
extern "C" VFX_API void TitanImGui_ImDrawList_Visitor_AddDrawCmd_2960189489(ImDrawList* self)
{
	return ImDrawList_Visitor::AddDrawCmd(self);
}
extern "C" VFX_API ImDrawList* TitanImGui_ImDrawList_Visitor_CloneOutput_4178288180(ImDrawList* self)
{
	return ImDrawList_Visitor::CloneOutput(self);
}
extern "C" VFX_API void TitanImGui_ImDrawList_Visitor_ChannelsSplit_4038704236(ImDrawList* self, int count)
{
	return ImDrawList_Visitor::ChannelsSplit(self, count);
}
extern "C" VFX_API void TitanImGui_ImDrawList_Visitor_ChannelsMerge_2960189489(ImDrawList* self)
{
	return ImDrawList_Visitor::ChannelsMerge(self);
}
extern "C" VFX_API void TitanImGui_ImDrawList_Visitor_ChannelsSetCurrent_4038704236(ImDrawList* self, int n)
{
	return ImDrawList_Visitor::ChannelsSetCurrent(self, n);
}
extern "C" VFX_API void TitanImGui_ImDrawList_Visitor_PrimReserve_3539386109(ImDrawList* self, int idx_count,int vtx_count)
{
	return ImDrawList_Visitor::PrimReserve(self, idx_count, vtx_count);
}
extern "C" VFX_API void TitanImGui_ImDrawList_Visitor_PrimUnreserve_3539386109(ImDrawList* self, int idx_count,int vtx_count)
{
	return ImDrawList_Visitor::PrimUnreserve(self, idx_count, vtx_count);
}
extern "C" VFX_API void TitanImGui_ImDrawList_Visitor_PrimRect_2036564649(ImDrawList* self, const ImVec2* a,const ImVec2* b,unsigned int col)
{
	return ImDrawList_Visitor::PrimRect(self, a, b, col);
}
extern "C" VFX_API void TitanImGui_ImDrawList_Visitor_PrimRectUV_498068585(ImDrawList* self, const ImVec2* a,const ImVec2* b,const ImVec2* uv_a,const ImVec2* uv_b,unsigned int col)
{
	return ImDrawList_Visitor::PrimRectUV(self, a, b, uv_a, uv_b, col);
}
extern "C" VFX_API void TitanImGui_ImDrawList_Visitor_PrimQuadUV_40166121(ImDrawList* self, const ImVec2* a,const ImVec2* b,const ImVec2* c,const ImVec2* d,const ImVec2* uv_a,const ImVec2* uv_b,const ImVec2* uv_c,const ImVec2* uv_d,unsigned int col)
{
	return ImDrawList_Visitor::PrimQuadUV(self, a, b, c, d, uv_a, uv_b, uv_c, uv_d, col);
}
extern "C" VFX_API void TitanImGui_ImDrawList_Visitor_PrimWriteVtx_2036564649(ImDrawList* self, const ImVec2* pos,const ImVec2* uv,unsigned int col)
{
	return ImDrawList_Visitor::PrimWriteVtx(self, pos, uv, col);
}
extern "C" VFX_API void TitanImGui_ImDrawList_Visitor_PrimWriteIdx_2016656406(ImDrawList* self, unsigned short idx)
{
	return ImDrawList_Visitor::PrimWriteIdx(self, idx);
}
extern "C" VFX_API void TitanImGui_ImDrawList_Visitor_PrimVtx_2036564649(ImDrawList* self, const ImVec2* pos,const ImVec2* uv,unsigned int col)
{
	return ImDrawList_Visitor::PrimVtx(self, pos, uv, col);
}
extern "C" VFX_API void TitanImGui_ImDrawList_Visitor_PushTextureID_2544618575(ImDrawList* self, ImTextureRef tex_ref)
{
	return ImDrawList_Visitor::PushTextureID(self, tex_ref);
}
extern "C" VFX_API void TitanImGui_ImDrawList_Visitor_PopTextureID_2960189489(ImDrawList* self)
{
	return ImDrawList_Visitor::PopTextureID(self);
}
extern "C" VFX_API void TitanImGui_ImDrawList_Visitor__ResetForNewFrame_2960189489(ImDrawList* self)
{
	return ImDrawList_Visitor::_ResetForNewFrame(self);
}
extern "C" VFX_API void TitanImGui_ImDrawList_Visitor__ClearFreeMemory_2960189489(ImDrawList* self)
{
	return ImDrawList_Visitor::_ClearFreeMemory(self);
}
extern "C" VFX_API void TitanImGui_ImDrawList_Visitor__PopUnusedDrawCmd_2960189489(ImDrawList* self)
{
	return ImDrawList_Visitor::_PopUnusedDrawCmd(self);
}
extern "C" VFX_API void TitanImGui_ImDrawList_Visitor__TryMergeDrawCmds_2960189489(ImDrawList* self)
{
	return ImDrawList_Visitor::_TryMergeDrawCmds(self);
}
extern "C" VFX_API void TitanImGui_ImDrawList_Visitor__OnChangedClipRect_2960189489(ImDrawList* self)
{
	return ImDrawList_Visitor::_OnChangedClipRect(self);
}
extern "C" VFX_API void TitanImGui_ImDrawList_Visitor__OnChangedTexture_2960189489(ImDrawList* self)
{
	return ImDrawList_Visitor::_OnChangedTexture(self);
}
extern "C" VFX_API void TitanImGui_ImDrawList_Visitor__OnChangedVtxOffset_2960189489(ImDrawList* self)
{
	return ImDrawList_Visitor::_OnChangedVtxOffset(self);
}
extern "C" VFX_API void TitanImGui_ImDrawList_Visitor__SetTexture_2544618575(ImDrawList* self, ImTextureRef tex_ref)
{
	return ImDrawList_Visitor::_SetTexture(self, tex_ref);
}
extern "C" VFX_API int TitanImGui_ImDrawList_Visitor__CalcCircleAutoSegmentCount_1740070591(ImDrawList* self, float radius)
{
	return ImDrawList_Visitor::_CalcCircleAutoSegmentCount(self, radius);
}
extern "C" VFX_API void TitanImGui_ImDrawList_Visitor__PathArcToFastEx_792707599(ImDrawList* self, const ImVec2* center,float radius,int a_min_sample,int a_max_sample,int a_step)
{
	return ImDrawList_Visitor::_PathArcToFastEx(self, center, radius, a_min_sample, a_max_sample, a_step);
}
extern "C" VFX_API void TitanImGui_ImDrawList_Visitor__PathArcToN_1172245843(ImDrawList* self, const ImVec2* center,float radius,float a_min,float a_max,int num_segments)
{
	return ImDrawList_Visitor::_PathArcToN(self, center, radius, a_min, a_max, num_segments);
}
#endif//HasModule_ImGui
