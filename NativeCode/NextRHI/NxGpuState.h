#pragma once
#include "NxBuffer.h"

NS_BEGIN

namespace NxRHI
{
	class IGraphicsEffect;
	enum TR_ENUM(SV_EnumNoFlags = true)
		ESamplerFilter
	{
		SPF_MIN_MAG_MIP_POINT = 0,
			SPF_MIN_MAG_POINT_MIP_LINEAR = 0x1,
			SPF_MIN_POINT_MAG_LINEAR_MIP_POINT = 0x4,
			SPF_MIN_POINT_MAG_MIP_LINEAR = 0x5,
			SPF_MIN_LINEAR_MAG_MIP_POINT = 0x10,
			SPF_MIN_LINEAR_MAG_POINT_MIP_LINEAR = 0x11,
			SPF_MIN_MAG_LINEAR_MIP_POINT = 0x14,
			SPF_MIN_MAG_MIP_LINEAR = 0x15,
			SPF_ANISOTROPIC = 0x55,
			SPF_COMPARISON_MIN_MAG_MIP_POINT = 0x80,
			SPF_COMPARISON_MIN_MAG_POINT_MIP_LINEAR = 0x81,
			SPF_COMPARISON_MIN_POINT_MAG_LINEAR_MIP_POINT = 0x84,
			SPF_COMPARISON_MIN_POINT_MAG_MIP_LINEAR = 0x85,
			SPF_COMPARISON_MIN_LINEAR_MAG_MIP_POINT = 0x90,
			SPF_COMPARISON_MIN_LINEAR_MAG_POINT_MIP_LINEAR = 0x91,
			SPF_COMPARISON_MIN_MAG_LINEAR_MIP_POINT = 0x94,
			SPF_COMPARISON_MIN_MAG_MIP_LINEAR = 0x95,
			SPF_COMPARISON_ANISOTROPIC = 0xd5,
			SPF_MINIMUM_MIN_MAG_MIP_POINT = 0x100,
			SPF_MINIMUM_MIN_MAG_POINT_MIP_LINEAR = 0x101,
			SPF_MINIMUM_MIN_POINT_MAG_LINEAR_MIP_POINT = 0x104,
			SPF_MINIMUM_MIN_POINT_MAG_MIP_LINEAR = 0x105,
			SPF_MINIMUM_MIN_LINEAR_MAG_MIP_POINT = 0x110,
			SPF_MINIMUM_MIN_LINEAR_MAG_POINT_MIP_LINEAR = 0x111,
			SPF_MINIMUM_MIN_MAG_LINEAR_MIP_POINT = 0x114,
			SPF_MINIMUM_MIN_MAG_MIP_LINEAR = 0x115,
			SPF_MINIMUM_ANISOTROPIC = 0x155,
			SPF_MAXIMUM_MIN_MAG_MIP_POINT = 0x180,
			SPF_MAXIMUM_MIN_MAG_POINT_MIP_LINEAR = 0x181,
			SPF_MAXIMUM_MIN_POINT_MAG_LINEAR_MIP_POINT = 0x184,
			SPF_MAXIMUM_MIN_POINT_MAG_MIP_LINEAR = 0x185,
			SPF_MAXIMUM_MIN_LINEAR_MAG_MIP_POINT = 0x190,
			SPF_MAXIMUM_MIN_LINEAR_MAG_POINT_MIP_LINEAR = 0x191,
			SPF_MAXIMUM_MIN_MAG_LINEAR_MIP_POINT = 0x194,
			SPF_MAXIMUM_MIN_MAG_MIP_LINEAR = 0x195,
			SPF_MAXIMUM_ANISOTROPIC = 0x1d5
	};
	enum TR_ENUM(SV_EnumNoFlags = true)
		EAddressMode
	{
		ADM_WRAP = 1,
			ADM_MIRROR,
			ADM_CLAMP,
			ADM_BORDER,
			ADM_MIRROR_ONCE,
	};
	struct TR_CLASS(SV_LayoutStruct = 8)
		FSamplerDesc
	{
		void SetDefault()
		{
			Filter = SPF_MIN_MAG_MIP_LINEAR;
			CmpMode = CMP_NEVER;
			AddressU = ADM_WRAP;
			AddressV = ADM_WRAP;
			AddressW = ADM_WRAP;
			MaxAnisotropy = 0;
			MipLODBias = 0;
			RgbaToColor4(0, BorderColor);
			MinLOD = 0;
			MaxLOD = 3.402823466e+38f;
		}
		ESamplerFilter		Filter;
		EComparisionMode	CmpMode;
		EAddressMode		AddressU;
		EAddressMode		AddressV;
		EAddressMode		AddressW;
		UINT				MaxAnisotropy;
		float				MipLODBias;
		float				BorderColor[4];
		float				MinLOD;
		float				MaxLOD;
	};
	class TR_CLASS()
		ISampler : public IGpuResource
	{
	public:
		ENGINE_RTTI(ISampler);
		FSamplerDesc	Desc;
	};

	enum TR_ENUM(SV_EnumNoFlags = true)
		EFillMode
	{
		FMD_WIREFRAME = 2,
			FMD_SOLID = 3
	};

	enum TR_ENUM(SV_EnumNoFlags = true)
		ECullMode
	{
		CMD_NONE = 1,
			CMD_FRONT = 2,
			CMD_BACK = 3
	};
	struct TR_CLASS(SV_LayoutStruct = 8)
		FRasterizerDesc
	{
		FRasterizerDesc()
		{
			SetDefault();
		}
		void SetDefault()
		{
			FillMode = FMD_SOLID;
			CullMode = CMD_BACK;
			FrontCounterClockwise = FALSE;
			DepthBias = 0;
			DepthBiasClamp = 0;
			SlopeScaledDepthBias = 0;
			DepthClipEnable = FALSE;
			ScissorEnable = FALSE;
			MultisampleEnable = FALSE;
			AntialiasedLineEnable = FALSE;
		}
		EFillMode FillMode;
		ECullMode CullMode;
		vBOOL FrontCounterClockwise;
		INT DepthBias;
		FLOAT DepthBiasClamp;
		FLOAT SlopeScaledDepthBias;
		vBOOL DepthClipEnable;
		vBOOL ScissorEnable;
		vBOOL MultisampleEnable;
		vBOOL AntialiasedLineEnable;
	};

	enum TR_ENUM(SV_EnumNoFlags = true)
		EDepthWriteMask
	{
		DSWM_ZERO = 0,
			DSWM_ALL = 1
	};

	enum TR_ENUM(SV_EnumNoFlags = true)
		EStencilOp
	{
		STOP_KEEP = 1,
			STOP_ZERO = 2,
			STOP_REPLACE = 3,
			STOP_INCR_SAT = 4,
			STOP_DECR_SAT = 5,
			STOP_INVERT = 6,
			STOP_INCR = 7,
			STOP_DECR = 8
	};
	struct TR_CLASS(SV_LayoutStruct = 8)
		FStencilOpDesc
	{
		EStencilOp StencilFailOp;
		EStencilOp StencilDepthFailOp;
		EStencilOp StencilPassOp;
		EComparisionMode StencilFunc;
	};
	struct TR_CLASS(SV_LayoutStruct = 8)
		FDepthStencilDesc
	{
		FDepthStencilDesc()
		{
			SetDefault(false);
		}
		void SetDefault(bool bInverseZ)
		{
			DepthEnable = TRUE;
			DepthWriteMask = DSWM_ALL;
			DepthFunc = bInverseZ ? CMP_GREATER_EQUAL :  CMP_LESS_EQUAL;
			StencilEnable = FALSE;
			StencilReadMask = 0xFF;
			StencilWriteMask = 0xFF;

			FrontFace.StencilDepthFailOp = STOP_KEEP;
			FrontFace.StencilFailOp = STOP_KEEP;
			FrontFace.StencilFunc = CMP_NEVER;

			BackFace.StencilDepthFailOp = STOP_KEEP;
			BackFace.StencilFailOp = STOP_KEEP;
			BackFace.StencilFunc = CMP_NEVER;

			StencilRef = 0;
		}
		vBOOL DepthEnable;
		EDepthWriteMask DepthWriteMask;
		EComparisionMode DepthFunc;
		vBOOL StencilEnable;
		BYTE StencilReadMask;
		BYTE StencilWriteMask;
		FStencilOpDesc FrontFace;
		FStencilOpDesc BackFace;

		UINT					StencilRef;
	};
	enum TR_ENUM(SV_EnumNoFlags = true)
		EBlend
	{
		BLD_ZERO = 1,
			BLD_ONE = 2,
			BLD_SRC_COLOR = 3,
			BLD_INV_SRC_COLOR = 4,
			BLD_SRC_ALPHA = 5,
			BLD_INV_SRC_ALPHA = 6,
			BLD_DEST_ALPHA = 7,
			BLD_INV_DEST_ALPHA = 8,
			BLD_DEST_COLOR = 9,
			BLD_INV_DEST_COLOR = 10,
			BLD_SRC_ALPHA_SAT = 11,
			BLD_BLEND_FACTOR = 14,
			BLD_INV_BLEND_FACTOR = 15,
			BLD_SRC1_COLOR = 16,
			BLD_INV_SRC1_COLOR = 17,
			BLD_SRC1_ALPHA = 18,
			BLD_INV_SRC1_ALPHA = 19
	};

	enum TR_ENUM(SV_EnumNoFlags = true)
		EBlendOp
	{
		BLDOP_ADD = 1,
			BLDOP_SUBTRACT = 2,
			BLDOP_REV_SUBTRACT = 3,
			BLDOP_MIN = 4,
			BLDOP_MAX = 5
	};
	struct TR_CLASS(SV_LayoutStruct = 8)
		FRenderTargetBlendDesc
	{
		FRenderTargetBlendDesc()
		{
			SetDefault();
		}
		void SetDefault()
		{
			BlendEnable = FALSE;
			SrcBlend = BLD_SRC_ALPHA;
			DestBlend = BLD_INV_SRC_ALPHA;
			BlendOp = BLDOP_ADD;
			SrcBlendAlpha = BLD_SRC_ALPHA;
			DestBlendAlpha = BLD_INV_SRC_ALPHA;
			BlendOpAlpha = BLDOP_ADD;
			RenderTargetWriteMask = 0x0F;
		}
		vBOOL BlendEnable;
		EBlend SrcBlend;
		EBlend DestBlend;
		EBlendOp BlendOp;
		EBlend SrcBlendAlpha;
		EBlend DestBlendAlpha;
		EBlendOp BlendOpAlpha;
		UINT8 RenderTargetWriteMask;
	};
	struct TR_CLASS(SV_LayoutStruct = 8)
		FBlendDesc
	{
		FBlendDesc()
		{
			SetDefault();
		}
		void SetDefault()
		{
			AlphaToCoverageEnable = FALSE;
			IndependentBlendEnable = FALSE;
			//NumOfRT = 1;
			for (int i = 0; i < 8; i++)
			{
				RenderTarget[i].SetDefault();
			}
		}
		//UINT NumOfRT;
		vBOOL AlphaToCoverageEnable;
		vBOOL IndependentBlendEnable;
		FRenderTargetBlendDesc RenderTarget[8];
	};

	struct TR_CLASS(SV_LayoutStruct = 8)
		FGpuPipelineDesc
	{
		void SetDefault(bool bReverseZ) {
			Rasterizer.SetDefault();
			DepthStencil.SetDefault(bReverseZ);
			Blend.SetDefault();
			StencilRef = 0;
			SampleMask = 0xff;
			BlendFactors[0] = 0;
			BlendFactors[1] = 0;
			BlendFactors[2] = 0;
			BlendFactors[3] = 0;
		}
		FRasterizerDesc Rasterizer;
		FDepthStencilDesc DepthStencil;
		FBlendDesc Blend;
		UINT StencilRef;
		UINT SampleMask;
		float BlendFactors[4];
	};
	class TR_CLASS()
		IGpuPipeline : public IGpuResource
	{
	public:
	public:
		FGpuPipelineDesc		Desc;
	};
	
	class IGpuDrawState : public VIUnknown
	{
	public:
		EPrimitiveType			TopologyType = EPrimitiveType::EPT_TriangleList;
		AutoRef<IGpuPipeline>	Pipeline;
		AutoRef<IGraphicsEffect>	ShaderEffect;
		AutoRef<IRenderPass>	RenderPass;
		virtual bool BuildState(IGpuDevice* device) {
			return true;
		}
	};

	class FGpuPipelineManager : public IWeakReference
	{
	public:
		const IGpuDrawState* GetOrCreate(IGpuDevice* device, IRenderPass * rpass, IGraphicsEffect* effect, IGpuPipeline* pipeline, EPrimitiveType topology);
	public:
		std::map<UINT64, AutoRef<IGpuDrawState>>		GpuPipelineCache;
	};
}

NS_END