#pragma once
#include "../../Base/IUnknown.h"
#include "../../Base/BlobObject.h"
#include "../../Base/debug//vfxdebug.h"
#include "../../Math/v3dxVector3.h"
#include "../../Math/v3dxQuaternion.h"
#include "../../NextRHI/NxRHI.h"

NS_BEGIN

using RealType = float;

enum TR_ENUM() 
	EPhyEntityType
{
	Phy_Context,
	Phy_Scene,
	Phy_Material,
	Phy_Controller,
	Phy_Actor,
	Phy_Shape,
	Unknown
};

enum TR_ENUM() 
	EPhyFeatureFlag
{
	Articulations = (1 << 0),
	HeightFields = (1 << 1),
	Cloth = (1 << 2),
	Particles = (1 << 3)
};

enum TR_ENUM() 
	EPhyActorType
{
	PAT_Dynamic,
	PAT_Static,
};

enum TR_ENUM()
	EPhyQueryFlag
{
	eSTATIC = (1 << 0),	//!< Traverse static shapes
	eDYNAMIC = (1 << 1),	//!< Traverse dynamic shapes
	ePREFILTER = (1 << 2),	//!< Run the pre-intersection-test filter (see #PxQueryFilterCallback::preFilter())
	ePOSTFILTER = (1 << 3),	//!< Run the post-intersection-test filter (see #PxQueryFilterCallback::postFilter())
	eANY_HIT = (1 << 4),	//!< Abort traversal as soon as any hit is found and return it via callback.block.
									//!< Helps query performance. Both eTOUCH and eBLOCK hitTypes are considered hits with this flag.
	eNO_BLOCK = (1 << 5),	//!< All hits are reported as touching. Overrides eBLOCK returned from user filters with eTOUCH.
																	//!< This is also an optimization hint that may improve query performance.
	eRESERVED = (1 << 15)	//!< Reserved for internal use
};

struct TR_CLASS(SV_LayoutStruct = 8)
	FPhyFilterData
{
	FPhyFilterData()
	{
		SetDefault();
	}

	void SetDefault()
	{
		word0 = word1 = word2 = word3 = 1;
	}
	void operator = (const FPhyFilterData& fd)
	{
		word0 = fd.word0;
		word1 = fd.word1;
		word2 = fd.word2;
		word3 = fd.word3;
	}
	bool operator == (const FPhyFilterData& a) const
	{
		return a.word0 == word0 && a.word1 == word1 && a.word2 == word2 && a.word3 == word3;
	}
	bool operator != (const FPhyFilterData& a) const
	{
		return !(a == *this);
	}
	UINT word0;
	UINT word1;
	UINT word2;
	UINT word3;
};

class TR_CLASS()
	PhyEntity : public IWeakRefObject
{
public:
	void*			mCSharpHandle;
public:
	ENGINE_RTTI(PhyEntity)
	PhyEntity()
	{
		mCSharpHandle = nullptr;
		EntityType = Unknown;
	}
	~PhyEntity();
	EPhyEntityType EntityType;
	EPhyEntityType  GetEntityType() { return EntityType; };
};

NS_END