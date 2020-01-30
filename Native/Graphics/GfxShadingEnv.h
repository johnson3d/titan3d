#pragma once
#include "GfxPreHead.h"

NS_BEGIN

class GfxShadingEnv : public VIUnknown
{
public:
	RTTI_DEF(GfxShadingEnv, 0xc37f97365b0f5b90, true);
	GfxShadingEnv();
	~GfxShadingEnv();

	virtual void Cleanup() override;
	//virtual Hash64 GetHash64() override;

	//vBOOL Init(const char* name, const char* shader);
	
	//VDef_ReadOnly(Version);

	//RName GetRName() const {
	//	return mName;
	//}
	//const char* GetName() const {
	//	return mName.Name.c_str();
	//}
	//const char* GetShaderName() const;

	//void AddVar(const char* name, EShaderVarType type, UINT elements);
	//void RemoveVar(const char* name);
	
	//void AddSRV(const char* name);
	//void RemoveSRV(const char* name);
	//void SetSRV(const char* name, IShaderResourceView* srv);
	//IShaderResourceView* GetSRV(const char* name);

	//void Save2Xnd(XNDNode* node);
	//vBOOL LoadXnd(XNDNode* node);
protected:
	//struct lessString
	//{
	//	bool operator()(const std::string& _Left, const std::string& _Right) const {
	//		return strcmp(_Left.c_str(), _Right.c_str()) < 0;
	//	}
	//};

	//RName					mName;
	//std::string				mShaderName;
	//UINT					mVersion;
	//std::map<std::string, GfxVarValue*, lessString>		mVars;

	//struct SRV
	//{
	//	SRV()
	//	{
	//		RSView = nullptr;
	//	}
	//	~SRV()
	//	{
	//		Safe_Release(RSView);
	//	}
	//	std::string				ShaderName;
	//	RName					RName;
	//	IShaderResourceView*	RSView;
	//};
	//std::map<std::string, SRV*, lessString>		mSRViews;
};

NS_END