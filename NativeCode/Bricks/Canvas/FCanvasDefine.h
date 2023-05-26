#pragma once
#include "../../Base/IUnknown.h"
#include "../../Math/v3dxVector4.h"
#include "../../Math/v3dxVector3.h"
#include "../../Math/v3dxVector2.h"
#include "../../NextRHI/NxFrameBuffers.h"
#include "../../NextRHI/NxGeomMesh.h"
#include "../../NextRHI/NxGeomMesh.h"

NS_BEGIN

struct FRectanglef;
class TR_CLASS()
	FUtility
{
public:
	static v3dxVector2 RotateVector(const v3dxVector2 & v, float angle = Math::HALF_PI);
	static bool LineLineIntersection(const v3dxVector2 & ps1, const v3dxVector2 & pe1, const v3dxVector2 & ps2, const v3dxVector2 & pe2, v3dxVector2 * pPoint);

	//sure s in rect
	static bool LineRectIntersection(const v3dxVector2 & s, const v3dxVector2 & e, const FRectanglef & rect, v3dxVector2 * pPoint);
};

namespace Canvas
{
	enum ERectCorner
	{
		RCN_X0_Y0 = 0,
		RCN_X1_Y0,
		RCN_X0_Y1,
		RCN_X1_Y1,
	};

	struct FColorAndFlags
	{
		union
		{
			struct
			{
				BYTE Red;
				BYTE Green;
				BYTE Blue;
				BYTE Alpha : 6;
				BYTE ShadingMode : 2;//0:normal 1:SDF
			};
			DWORD Value;
		};
		FColorAndFlags operator =(const FColor& rgba) {
			Red = rgba.R;
			Green = rgba.G;
			Blue = rgba.B;
			Alpha = rgba.A * 63 / 255;
			return *this;
		}
		FColorAndFlags()
		{
			Red = 255;
			Green = 255;
			Blue = 255;
			Alpha = 63;
			ShadingMode = 0;
		}
		FColorAndFlags(BYTE r, BYTE g, BYTE b, BYTE a, BYTE mode)
		{
			Red = r;
			Green = g;
			Blue = b;
			Alpha = a;
			ShadingMode = mode;
		}
		static FColorAndFlags MakeValue(BYTE r, BYTE g, BYTE b, BYTE a, BYTE mode) {
			return FColorAndFlags(r, g, b, a, mode);
		}
	};

	struct TR_CLASS(SV_LayoutStruct = 8)
		FCanvasVertex
	{
		v3dxVector3		Pos{};
		v3dxVector2		UV{};
		FColorAndFlags	Color{};
	};
}
NS_END

