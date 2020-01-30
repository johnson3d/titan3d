#pragma once
#include "../../Graphics/GfxPreHead.h"
#include "../../Math/v3dxConvex.h"

NS_BEGIN

class GfxMeshPrimitives;

class TriMesh
{
public:
	vBOOL						mIsConvex;
	v3dxBox3					mBoundingBox;
	std::vector<v3dxVector3>			mPositions;
	std::vector<v3dxVector3>			mNormals;
	std::vector<v3dxVector2>			mUVs;
	std::vector<DWORD>				mUserFlags;
	std::vector<UINT>				mIndices;
	
	struct Convex
	{
		UINT StartIndex = 0;
		UINT NumTriangle = 0;
		float Volume = 0.0f;
		void BuildConvex(const TriMesh* mesh, v3dxConvex* convex) const;
	};
	std::vector<Convex>				mHulls;

	void Reset()
	{
		mPositions.clear();
		mNormals.clear();
		mIndices.clear();
		mUserFlags.clear();
	}
	void UpdateBox()
	{
		mBoundingBox.InitializeBox();
		for (auto i : mPositions)
		{
			mBoundingBox.OptimalVertex(i);
		}
	}
	UINT GetConvexNum() const {
		return (UINT)mHulls.size();
	}
	vBOOL BuildConvex(v3dxConvex** ppConvexes, UINT count) const;
	UINT GetVertexNumber() {
		return (UINT)mPositions.size();
	}
	void AddVertex(v3dxVector3* pos, v3dxVector3* nor, v3dxVector2* uv, DWORD flag)
	{
		mPositions.push_back(*pos);
		mNormals.push_back(*nor);
		mUVs.push_back(*uv);
		mUserFlags.push_back(flag);
	}
	void AddTriangle(UINT a, UINT b, UINT c)
	{
		mIndices.push_back(a);
		mIndices.push_back(b);
		mIndices.push_back(c);
	}
};

class SimGeom : public VIUnknown
{
public:
	class ConvexDecompDesc
	{
	public:
		ConvexDecompDesc(void)
		{
			mDepth = 8;
			mCpercent = 0.1;
			mPpercent = 30;
			mMaxVertices = 64;
			mSkinWidth = 0;
			mVolumeSplitPercent = 0.1;
		}
		// options
		unsigned int	mDepth;    // depth to split, a maximum of 10, generally not over 7.
		unsigned int	mMaxVertices; // maximum number of vertices in the output hull. Recommended 32 or less.
		double			mCpercent; // the concavity threshold percentage.  0=20 is reasonable.
		double			mPpercent; // the percentage volume conservation threshold to collapse hulls. 0-30 is reasonable.
		double			mVolumeSplitPercent;
		// hull output limits.
		double			mSkinWidth;   // a skin width to apply to the output hulls.
	};
	TriMesh				mTriMesh;
public:
	RTTI_DEF(SimGeom, 0x6b855fc85c399712, true);
	SimGeom();
	~SimGeom();
	vBOOL BuildTriMesh(IRenderContext* rc, GfxMeshPrimitives* mesh, ConvexDecompDesc* desc);
	GfxMeshPrimitives* CreateMesh(IRenderContext* rc);

	UINT GetConvexNum() const {
		return mTriMesh.GetConvexNum();
	}
	vBOOL BuildConvex(v3dxConvex** ppConvexes, UINT count) const {
		return mTriMesh.BuildConvex(ppConvexes, count);
	}
};

NS_END