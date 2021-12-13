#pragma once

#include "../../Base/IUnknown.h"
#include "../../Math/v3dxBox3.h"

NS_BEGIN

enum TR_ENUM()
	ENodeStyle
{
	NDS_LB = 1,
	NDS_LT = (1 << 1),
	NDS_RT = (1 << 2),
	NDS_RB = (1 << 3),
};

struct TR_CLASS()
	QNodeBase
{
	QNodeBase()
	{
		NodeStyles = (ENodeStyle)0;
		X = Z = -1;
		Level = -1;
	}
	virtual QNodeBase* GetChild(ENodeStyle style) {
		return nullptr;
	}
	void SetChild(ENodeStyle style, QNodeBase* node) {

	}
	virtual bool IsLeaf() {
		return false;
	}
	int Level;
	unsigned int X;
	unsigned int Z;

	ENodeStyle	NodeStyles;
	v3dxBox3	AABB;
};

class TR_CLASS()
	QNode : public QNodeBase
{
public:
	QNode()
	{
		LBNode = nullptr;
		LTNode = nullptr;
		RBNode = nullptr;
		RTNode = nullptr;
	}
	QNodeBase* LBNode;
	QNodeBase* LTNode;
	QNodeBase* RBNode;
	QNodeBase* RTNode;
	virtual bool IsLeaf() {
		return false;
	}
	virtual QNodeBase* GetChild(ENodeStyle style) 
	{
		switch (style)
		{
			case EngineNS::NDS_LB:
				return LBNode;
			case EngineNS::NDS_LT:
				return LTNode;
			case EngineNS::NDS_RT:
				return RTNode;
			case EngineNS::NDS_RB:
				return RBNode;
			default:
				break;
		}
		return nullptr;
	}
	void SetChild(ENodeStyle style, QNodeBase* node) 
	{
		switch (style)
		{
			case EngineNS::NDS_LB:
				LBNode = node;
			case EngineNS::NDS_LT:
				LTNode = node;
			case EngineNS::NDS_RT:
				RTNode = node;
			case EngineNS::NDS_RB:
				RBNode = node;
			default:
				break;
		}
	}

	template <typename _Function>
	void IterateNode(_Function func)
	{
		if (IsLeaf())
		{
			func(this, true);
		}
		else
		{
			func(this, false);
			((QNode*)LBNode)->IterateNode();
			((QNode*)LTNode)->IterateNode();
			((QNode*)RBNode)->IterateNode();
			((QNode*)RTNode)->IterateNode();
		}
	}
};

class TR_CLASS()
	QLeaf : public QNodeBase
{
public:
	QLeaf()
	{
		TagObject = nullptr;
	}
	virtual bool IsLeaf() {
		return true;
	}
	void*		TagObject;
};

class TR_CLASS()
	QTree : public VIUnknown
{
public:
	QTree();
	~QTree();
	void Cleanup();

	//64 * 16 = 1024 meters
	bool Initialize(int MipLevels = 4, float patchSize = 64.0f);

	QLeaf* GetLeaf(int x, int z);
	inline int GetMipLevels() {
		return MipLevel;
	}
	inline int GetPatchSide() {
		return PatchSide;
	}
	inline float GetPatchSize() {
		return PatchSize;
	}
protected:
	void BuildTree(QNode * node);
protected:
	int						MipLevel;
	int						PatchSide;
	float					PatchSize;
	std::vector<QLeaf*>		mLeafs;
	std::vector<QNode*>		mNodes;
	QNode*					mRoot;
};


NS_END