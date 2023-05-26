#pragma once
#include "FCanvasBrush.h"
#include "Utility.h"

NS_BEGIN

namespace Canvas
{
	class FCanvasDrawCmdList;
	struct FSubDrawCmd;

	enum TR_ENUM()
		EPathStrokeMode
	{
		Stroke_Normal,
			Stroke_Dash,
			Stroke_Dot,
	};

	enum TR_ENUM()
		EPathJoinMode
	{
		Join_None,
			Join_Miter,
			Join_Round,
			Join_Bevel,
	};

	enum TR_ENUM()
		EPathCapMode
	{
		Cap_Butt,
			Cap_Round,
			Cap_Square,
	};

	enum TR_ENUM()
		EPathCmdType
	{
		Cmd_Close,
			Cmd_MoveTo,
			Cmd_LineTo,
			Cmd_SCCWArcTo,
			Cmd_SCWArcTo,
			Cmd_LCCWArcTo,
			Cmd_LCWArcTo,
			Cmd_Number,
	};

	enum TR_ENUM()
		EPathPolyType
	{
		Poly_Convex,
			Poly_Concave,
	};

	enum TR_ENUM()
		EPathPaintMode
	{
		Paint_Center,
		Paint_Fit,
		Paint_Fill,
		Paint_Stretch,
		Paint_Free,
	};

	namespace Path
	{
		const UINT GPathCmdCoord[Cmd_Number] = {0, 2, 2, 3, 3, 3, 3};

		class TR_CLASS()
			FPathStyle : public VIUnknown
		{
		public:
			FPathStyle() {}
			float PathWidth = 10.0f;
			float MiterLimit = 1.03f;
			EPathStrokeMode StrokeMode = Stroke_Normal;
			EPathJoinMode JoinMode = Join_Miter;
			EPathCapMode CapMode = Cap_Butt;
			bool FillArea = true;
			bool UvAlongPath = true;
			EPathPaintMode PaintMode = Paint_Fit;
			v3dxVector4 PaintTiling = v3dxVector4(1, 1, 0, 0); // ScalarU, ScalarV, OffsetU, OffsetV

			float StrokeOffset = 0.f;
			std::vector<float> StrokePattern;
			bool StrokeAutoAlign = true;

			void ResetStrokePattern() {
				StrokePattern.clear();
			}
			void PushStrokePattern(float* pPattern, int num)
			{
				auto start = StrokePattern.size();
				StrokePattern.resize(StrokePattern.size() + num);
				memcpy(&StrokePattern[start], pPattern, sizeof(float) * num);
			}
		};

		/////////////////////////////////////////
		enum TR_ENUM()
			EPathPointFlag
		{
			Point_None,
				Point_Joint,
				Point_Start,
				Point_End,
				Point_Single,
		};

		struct TR_CLASS(SV_LayoutStruct = 8)
			FPathPoint
		{
			v3dxVector2 Position;
			v3dxVector2 Forward;
			float Distance;
			EPathPointFlag Flag = Point_None;

			FPathPoint() 
				: Position(0), Forward(0), Distance(0), Flag(Point_None)
			{}
			FPathPoint(const v3dxVector2& InPos, const v3dxVector2& InDir, float InDist, EPathPointFlag InFlag) 
				: Position(InPos), Forward(InDir), Distance(InDist), Flag(InFlag) 
			{}
			static FPathPoint Interplate(const FPathPoint& P0, const FPathPoint& P1, float Distance);
		};

		struct FPathPoly
		{
			std::vector<v3dxVector2> Vertices;
			std::vector<v3dxVector2> UVs;
			std::vector<UINT> Indices;
			EPathPolyType PolyType = Poly_Convex;

			UINT GetSize() const { return (UINT)Vertices.size(); }
			bool HasUvChannel() const { return UVs.size() != 0 && UVs.size() == Vertices.size(); }
			bool HasTriangulated() const { return Indices.size() != 0; }
			void Traingulate();
			void WindowClip(const FRectanglef& Rect);
			FRectanglef GetBounds() const;
		};		
		class TR_CLASS()
			FPath : public VIUnknown
		{
		public:
			FPath() {}
			void BeginPath();
			void PushCmd(EPathCmdType CmdType, const void* Args = nullptr, UINT Count = 0);
			void EndPath(Canvas::FCanvasDrawCmdList* DrawCmdList, Canvas::FSubDrawCmd* pOutCmd = nullptr);

			void MoveTo(const v3dxVector2& P);
			void LineTo(const v3dxVector2& P);
			void ArcTo(const v3dxVector2& P, float Radius, bool IsCCW = true, bool IsSmall = true);
			void Close();

			void PushRect(const v3dxVector2& Min, const v3dxVector2& Max, float Round /* Round corner radius */);
			void PushCircle(const v3dxVector2& Center, float Radius);

		private:
			void Clear();
			void BuildPath();
			void Fill();
			void Stroke();
			void Scissor(const FRectanglef& Rect);
			void Trianglute();

			void DrawLine(const std::vector<FPathPoint>::iterator& Begin, const std::vector<FPathPoint>::iterator& End);
			void DrawJoint(const FPathPoint& P0, const FPathPoint& P1);
			void DrawCap(const FPathPoint& P0);

			AutoRef<FPathStyle> Style;
			std::vector<float> Params;
			std::vector<EPathCmdType> CmdList;
			std::vector<FPathPoint> Points;

			struct FPathGroup
			{
				UINT Begin = 0;
				UINT End = 0;
				bool bLoop = false;
			};

			std::vector<FPathGroup> Groups;
			bool IsValidGroup(FPathGroup Group) 
			{
				return Group.Begin >= 0 && Group.End <= Points.size()
					&& Group.End > Group.Begin;
			}

			float PathLength = 0;
			FRectanglef Bounds;
			std::vector<FPathPoly> Polygons;

		private:
			bool LineToInternal(const v3dxVector2& P0, const v3dxVector2& P1, float& Distance, FPathGroup& Group);
			bool ArcToInternal(const v3dxVector2& P0, const v3dxVector2& P1, float R, bool IsCCW, bool IsSmall, float& Distance, FPathGroup& Group);
			
			void MiterJointInternal(const FPathPoint& P0, const FPathPoint& P1);
			void RoundJointInternal(const FPathPoint& P0, const FPathPoint& P1);
			void BevelJointInternal(const FPathPoint& P0, const FPathPoint& P1);
			void RoundCapInternal(const FPathPoint& P0);
			void SquareCapInternal(const FPathPoint& P0);

			void UpdateBounds();
			v3dxVector2 RemapUV(const FRectanglef& InBounds, const FRectanglef& InBrush, const v3dxVector4& InTiling, const v3dxVector2& InUV) const;
		};
	}
}

NS_END