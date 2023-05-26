#pragma once
#include "FCanvasDefine.h"

NS_BEGIN

namespace Canvas
{
	class FPathUtility
	{
	public:
		static constexpr float PI = 3.1415926535897932f;
		static constexpr float TWO_PI = 6.28318530717f;

		static void SafeNormalize(const v3dxVector2& In, v3dxVector2& Out, float& Length);
		static v3dxVector2 Normalize(const v3dxVector2& In);
		static v3dxVector2 Lerp(const v3dxVector2& A, const v3dxVector2& B, float Alpha);
		static float DotProduct(const v3dxVector2& V0, const v3dxVector2& V1);
		static float CrossProduct(const v3dxVector2& V0, const v3dxVector2& V1);
		static v3dxVector2 GetRight(const v3dxVector2& V);
		static float GetDistance(const v3dxVector2& P0, const v3dxVector2& P1);
		static bool IsInsideTriangle(const v3dxVector2& A, const v3dxVector2& B, const v3dxVector2& C, const v3dxVector2& P);
		static bool IsInsideRect(const FRectanglef& Rect, const v3dxVector2& P);
		static float GetTriangleArea(const v3dxVector2& A, const v3dxVector2& B, const v3dxVector2& C);
		
		static FRectanglef CalcBounds(const void* InVerts, UINT NumVerts);
		static void Triangulate(const void* InVerts, UINT NumVerts, std::vector<UINT>& OutInds, UINT IndOffset = 0, bool bConvex = true);

		// Clip polygon by a rect window.
		// It is supposed to be non self-intersect.
		// https://en.wikipedia.org/wiki/Sutherland¨CHodgman_algorithm
		static void SutherlandHodgmanClip(const FRectanglef& Rect,
			const void* InVerts, UINT NumVerts,
			std::vector<v3dxVector2>& OutVerts,
			const void* InUVs = nullptr,
			std::vector<v3dxVector2>* OutUVs = nullptr);
	};

	class FArcLUT
	{
	public:
		static constexpr UINT NUM_FASTLUT = 64;
		static constexpr float DELTA = FPathUtility::TWO_PI / NUM_FASTLUT;
		v3dxVector2 LUT[NUM_FASTLUT];

		FArcLUT();
		UINT GetLUTIndex(const v3dxVector2& Orient, bool InCCW = true);
		UINT GetLUTIndex(const v3dxVector2& Orient, bool InCCW, float& OutArcToIndex);
	};

	extern FArcLUT GArcLUT;
}

NS_END