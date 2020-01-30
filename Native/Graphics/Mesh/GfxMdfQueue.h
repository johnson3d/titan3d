#pragma once
#include "../GfxPreHead.h"

NS_BEGIN

class GfxMesh;
class GfxModifier;
class GfxMdfQueue : public VIUnknown
{
public:
	RTTI_DEF(GfxMdfQueue, 0x448ef9f85b0e52d5, true);
	GfxMdfQueue();
	~GfxMdfQueue();
	virtual void Cleanup() override;

	UINT GetMdfNumber() const;
	void AddModifier(GfxModifier* modifier);
	UINT FindModifier(const char* name);
	void RemoveModifier(UINT index);
	void ClearModifiers();
	GfxModifier* GetModifier(UINT index);

	virtual void TickLogic(IRenderContext* rc, GfxMesh* mesh, vTimeTick time);
protected:
	std::vector<GfxModifier*>	mModQueue;
};

NS_END