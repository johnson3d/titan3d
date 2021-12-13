#include "QTree.h"

#define new VNEW

NS_BEGIN

QTree::QTree()
{
	MipLevel = 0;
	PatchSide = 0;
	PatchSize = 0;
	mRoot = nullptr;
}

QTree::~QTree()
{
	Cleanup();
}

void QTree::Cleanup()
{
	PatchSide = 0;
	mRoot = nullptr;
	for (auto i : mLeafs)
	{
		delete i;
	}
	mLeafs.clear();

	for (auto i : mNodes)
	{
		delete i;
	}
	mNodes.clear();
}

bool QTree::Initialize(int mipLevels, float patchSize)
{
	Cleanup();

	if (mipLevels < 1)
		return false;

	MipLevel = mipLevels;
	PatchSize = patchSize;
	PatchSide = 1;
	for (int i = 0; i < mipLevels; i++)
	{
		PatchSide *= 2;		
	}

	for (int z = 0; z < PatchSide; z++)
	{
		for (int x = 0; x < PatchSide; x++)
		{
			auto leaf = new QLeaf();
			leaf->X = x;
			leaf->Z = z;
			leaf->AABB.minbox.x = x * PatchSize;
			leaf->AABB.maxbox.x = leaf->AABB.minbox.x + PatchSize;
			leaf->AABB.minbox.y = FLT_MAX;
			leaf->AABB.maxbox.y = -FLT_MAX;
			leaf->AABB.minbox.z = z * PatchSize;
			leaf->AABB.maxbox.z = leaf->AABB.minbox.z + PatchSize;

			mLeafs.push_back(leaf);
		}
	}
	/*mLeafs.resize((size_t)(PatchSide * PatchSide));
	for (size_t i = 0; i < mLeafs.size(); i++)
	{
		mLeafs[i] = new QLeaf();
	}*/

	mRoot = new QNode();
	mRoot->X = 0;
	mRoot->Z = 0;
	mRoot->Level = 0;
	mRoot->AABB.minbox.x = 0;
	mRoot->AABB.maxbox.x = patchSize * PatchSide;
	mRoot->AABB.minbox.y = FLT_MAX;
	mRoot->AABB.maxbox.y = -FLT_MAX;
	mRoot->AABB.minbox.z = 0;
	mRoot->AABB.maxbox.z = patchSize * PatchSide;

	BuildTree(mRoot);
	return true;
}

void QTree::BuildTree(QNode* node)
{
	mNodes.push_back(node);
	float sz_x = node->AABB.GetWidth();
	float sz_z = node->AABB.GetHeight();

	if (node->Level == MipLevel - 1)
	{
		node->LBNode = GetLeaf(node->X * 2, node->Z * 2);
		node->LBNode->NodeStyles = NDS_LB;
		node->LBNode->Level = node->Level + 1;
		ASSERT(node->LBNode->X == node->X * 2);
		ASSERT(node->LBNode->Z == node->Z * 2);
		auto curNode = node->LBNode;
		curNode->AABB.minbox.x = node->AABB.minbox.x;
		curNode->AABB.maxbox.x = curNode->AABB.minbox.x + sz_x * 0.5f;
		curNode->AABB.minbox.y = FLT_MAX;
		curNode->AABB.maxbox.y = -FLT_MAX;
		curNode->AABB.minbox.z = node->AABB.minbox.z;
		curNode->AABB.maxbox.z = curNode->AABB.minbox.z + sz_z * 0.5f;

		node->LTNode = GetLeaf(node->X * 2, node->Z * 2 + 1);
		node->LTNode->NodeStyles = NDS_LT;
		node->LTNode->Level = node->Level + 1;
		ASSERT(node->LTNode->X == node->X * 2);
		ASSERT(node->LTNode->Z == node->Z * 2 + 1);
		curNode = node->LTNode;
		curNode->AABB.minbox.x = node->AABB.minbox.x;
		curNode->AABB.maxbox.x = curNode->AABB.minbox.x + sz_x * 0.5f;
		curNode->AABB.minbox.y = FLT_MAX;
		curNode->AABB.maxbox.y = -FLT_MAX;
		curNode->AABB.minbox.z = node->AABB.minbox.z + sz_z * 0.5f;
		curNode->AABB.maxbox.z = curNode->AABB.minbox.z + sz_z * 0.5f;

		node->RTNode = GetLeaf(node->X * 2 + 1, node->Z * 2 + 1);
		node->RTNode->NodeStyles = NDS_RT;
		node->RTNode->Level = node->Level + 1;
		ASSERT(node->RTNode->X == node->X * 2 + 1);
		ASSERT(node->RTNode->Z == node->Z * 2 + 1);
		curNode = node->RTNode;
		curNode->AABB.minbox.x = node->AABB.minbox.x + sz_x * 0.5f;
		curNode->AABB.maxbox.x = curNode->AABB.minbox.x + sz_x * 0.5f;
		curNode->AABB.minbox.y = FLT_MAX;
		curNode->AABB.maxbox.y = -FLT_MAX;
		curNode->AABB.minbox.z = node->AABB.minbox.z + sz_z * 0.5f;
		curNode->AABB.maxbox.z = curNode->AABB.minbox.z + sz_z * 0.5f;

		node->RBNode = GetLeaf(node->X * 2 + 1, node->Z * 2);
		node->RBNode->NodeStyles = NDS_RB;
		node->RBNode->Level = node->Level + 1;
		node->RBNode->NodeStyles = NDS_RB;
		node->RBNode->Level = node->Level + 1;
		ASSERT(node->RBNode->X == node->X * 2 + 1);
		ASSERT(node->RBNode->Z == node->Z * 2);
		curNode = node->RBNode;
		curNode->AABB.minbox.x = node->AABB.minbox.x + sz_x * 0.5f;
		curNode->AABB.maxbox.x = curNode->AABB.minbox.x + sz_x * 0.5f;
		curNode->AABB.minbox.y = FLT_MAX;
		curNode->AABB.maxbox.y = -FLT_MAX;
		curNode->AABB.minbox.z = node->AABB.minbox.z;
		curNode->AABB.maxbox.z = curNode->AABB.minbox.z + sz_z * 0.5f;
	}
	else
	{
		node->LBNode = new QNode();
		node->LBNode->NodeStyles = NDS_LB;
		node->LBNode->Level = node->Level + 1;
		node->LBNode->X = node->X * 2;
		node->LBNode->Z = node->Z * 2;

		auto curNode = node->LBNode;
		curNode->AABB.minbox.x = node->AABB.minbox.x;
		curNode->AABB.maxbox.x = curNode->AABB.minbox.x + sz_x * 0.5f;
		curNode->AABB.minbox.y = FLT_MAX;
		curNode->AABB.maxbox.y = -FLT_MAX;
		curNode->AABB.minbox.z = node->AABB.minbox.z;
		curNode->AABB.maxbox.z = curNode->AABB.minbox.z + sz_z * 0.5f;

		node->LTNode = new QNode();
		node->LTNode->NodeStyles = NDS_LT;
		node->LTNode->Level = node->Level + 1;
		node->LTNode->X = node->X * 2;
		node->LTNode->Z = node->Z * 2 + 1;

		curNode = node->LTNode;
		curNode->AABB.minbox.x = node->AABB.minbox.x;
		curNode->AABB.maxbox.x = curNode->AABB.minbox.x + sz_x * 0.5f;
		curNode->AABB.minbox.y = FLT_MAX;
		curNode->AABB.maxbox.y = -FLT_MAX;
		curNode->AABB.minbox.z = node->AABB.minbox.z + sz_z * 0.5f;
		curNode->AABB.maxbox.z = curNode->AABB.minbox.z + sz_z * 0.5f;

		node->RBNode = new QNode();
		node->RBNode->NodeStyles = NDS_RB;
		node->RBNode->Level = node->Level + 1;
		node->RBNode->X = node->X * 2 + 1;
		node->RBNode->Z = node->Z * 2;

		curNode = node->RBNode;
		curNode->AABB.minbox.x = node->AABB.minbox.x + sz_x * 0.5f;
		curNode->AABB.maxbox.x = curNode->AABB.minbox.x + sz_x * 0.5f;
		curNode->AABB.minbox.y = FLT_MAX;
		curNode->AABB.maxbox.y = -FLT_MAX;
		curNode->AABB.minbox.z = node->AABB.minbox.z;
		curNode->AABB.maxbox.z = curNode->AABB.minbox.z + sz_z * 0.5f;

		node->RTNode = new QNode();
		node->RTNode->NodeStyles = NDS_RT;
		node->RTNode->Level = node->Level + 1;
		node->RTNode->X = node->X * 2 + 1;
		node->RTNode->Z = node->Z * 2 + 1;

		curNode = node->RTNode;
		curNode->AABB.minbox.x = node->AABB.minbox.x + sz_x * 0.5f;
		curNode->AABB.maxbox.x = curNode->AABB.minbox.x + sz_x * 0.5f;
		curNode->AABB.minbox.y = FLT_MAX;
		curNode->AABB.maxbox.y = -FLT_MAX;
		curNode->AABB.minbox.z = node->AABB.minbox.z + sz_z * 0.5f;
		curNode->AABB.maxbox.z = curNode->AABB.minbox.z + sz_z * 0.5f;

		BuildTree((QNode*)node->LBNode);
		BuildTree((QNode*)node->LTNode);
		BuildTree((QNode*)node->RBNode);
		BuildTree((QNode*)node->RTNode);
	}
}

QLeaf* QTree::GetLeaf(int x, int z)
{
	if (x < 0 || x >= PatchSide)
		return nullptr;
	if (z < 0 || z >= PatchSide)
		return nullptr;
	return mLeafs[z * PatchSide + x];
}

NS_END
