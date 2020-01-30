#pragma once
#include "../GfxPreHead.h"

NS_BEGIN

struct GfxModifierDesc
{

};

class GfxMesh;
class GfxModifier : public VIUnknown
{
public:
	RTTI_DEF(GfxModifier, 0x1578b3b35b1be440, true);
	GfxModifier();
	~GfxModifier();

	virtual void Cleanup() override;
	virtual Hash64 GetHash64() override;

	virtual bool Init(GfxModifierDesc* desc);

	virtual void TickLogic(IRenderContext* rc, GfxMesh* mesh, vTimeTick time) {}
	virtual void TickRender(IRenderContext* rc, vTimeTick time) {}
	virtual void TickSync(IRenderContext* rc, GfxMesh* mesh, vTimeTick time) {}

	virtual void Save2Xnd(XNDNode* node);
	virtual vBOOL LoadXnd(XNDNode* node);

	const char* GetName() const {
		return mName.c_str();
	}
	void SetName(const char* name);
	const char* GetShaderModuleName() const {
		return mShaderModule.c_str();
	}
	void SetShaderModuleName(const char* name) {
		mShaderModule = name;
	}
	virtual GfxModifier* CloneModifier(IRenderContext* rc);
public:
	std::string				mName;//csharp type name
	std::string				mShaderModule;//shader file name
};

NS_END