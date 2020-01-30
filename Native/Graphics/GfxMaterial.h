#pragma once
#include "GfxPreHead.h"

class XNDNode;

NS_BEGIN

//enum EGfxVarType
//{
//	GVT_F,
//	GVT_D,
//	GVT_I8,
//	GVT_I16,
//	GVT_I32,
//	GVT_I64,
//	GVT_uI8,
//	GVT_uI16,
//	GVT_uI32,
//	GVT_uI64,
//	GVT_V2,
//	GVT_V3,
//	GVT_V4,
//	GVT_Mat4,
//	GVT_Unknown,
//};
//
//struct GfxVar
//{
//	union
//	{
//		float F;
//		double D;
//		INT8 I8;
//		INT16 I16;
//		INT32 I32;
//		INT64 I64;
//		UINT8 uI8;
//		UINT16 uI16;
//		UINT32 uI32;
//		UINT64 uI64;
//		v3dVector2_t V2;
//		v3dVector3_t V3;
//		v3dVector4_t V4;
//		v3dMatrix4_t Mat4;
//	};
//
//	EGfxVarType Type;
//
//	GfxVar()
//	{
//		memset(&F, 0, sizeof(GfxVar)-sizeof(EGfxVarType));
//		Type = GVT_Unknown;
//	}
//};

class GfxMaterial : public VIUnknown
{
public:
	GfxMaterial();
	~GfxMaterial();

	RTTI_DEF(GfxMaterial, 0xa8bd732d5b0df950, true);

	virtual void Cleanup() override;

	vBOOL Init(const char* name);

	//VDef_ReadOnly(Version);
};

class GfxMaterialInstance : public VIUnknown
{
protected:
	//AutoRef<GfxMaterial>	mMaterial;
	//AutoRef<IRasterizerState>	mRasterizerState;
	//AutoRef<IDepthStencilState>	mDepthStencilState;
	//AutoRef<IBlendState>		mBlendState;
public:
	RTTI_DEF(GfxMaterialInstance, 0x944e35a15b0e3b4b, true);
	GfxMaterialInstance();
	~GfxMaterialInstance();
	//virtual void Cleanup() override;
	//vBOOL Init(GfxMaterial* material, const char* name);
	//vBOOL SetMaterial(GfxMaterial* material, bool doClear);
	//void SetRasterizerState(IRasterizerState* rs);
	//void SetDepthStencilState(IDepthStencilState* dss);
	//void SetBlendState(IBlendState* bs);
	//IRasterizerState* GetRasterizerState();
	//IDepthStencilState* GetDepthStencilState();

	//UINT GetVarNumber() const {
	//	return (UINT)mVars.size();
	//}
	//const char* GetVarName(UINT index) const;
	//UINT FindVarIndex(const char* name);
	//vBOOL GetVarValue(UINT index, UINT elementIndex, GfxVar* definition, BYTE* pValue);
	//vBOOL SetVarValue(UINT index, UINT elementIndex, const BYTE* pValue);
	//vBOOL AddVar(const char* name, UINT type, UINT elements);
	//vBOOL RemoveVar(UINT index);

	//void Save2Xnd(XNDNode* node);
	//vBOOL LoadXnd(XNDNode* node);

	//GfxVarValue* FindVar(const char* name);
	//std::vector<GfxVarValue*>		mVars;

	//SRV* FindSRV(const char* name);
	//UINT FindSRVIndex(const char* name);
	//void SetSRV(UINT index, IShaderResourceView* rsv);
	//IShaderResourceView* GetSRV(UINT index) {
	//	if (index >= mSRViews.size())
	//		return nullptr;
	//	return mSRViews[index]->RSView;
	//}
	//const char* GetSRVShaderName(UINT index) {
	//	if (index >= mSRViews.size())
	//		return nullptr;
	//	return mSRViews[index]->ShaderName.c_str();
	//}
	//UINT GetSRVNumber() const{
	//	return (UINT)mSRViews.size();
	//}
	//vBOOL AddSRV(const char* name);
	//vBOOL RemoveSRV(UINT index);
	//std::vector<SRV*>					mSRViews;

	//std::vector<ISamplerStateDesc>			mSamplerStateDescArray;

	//IRasterizerStateDesc			mRSDesc;
	//IDepthStencilStateDesc			mDSDesc;
	//IBlendStateDesc					mBLDDesc;
	//void GetRSDesc(IRasterizerStateDesc* desc)
	//{
	//	*desc = mRSDesc;
	//}
	//void GetDSDesc(IDepthStencilStateDesc* desc)
	//{
	//	*desc = mDSDesc;
	//}
	//void GetBLDDesc(IBlendStateDesc* desc)
	//{
	//	*desc = mBLDDesc;
	//}
};

NS_END