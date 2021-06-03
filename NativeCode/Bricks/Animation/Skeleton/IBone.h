#pragma once
#include "../../../RHI/PreHead.h"
#include "../../../Base/string/vfxstring.h"
#include "../../../Math/v3dxMatrix4.h"


namespace EngineNS
{
#pragma pack(push,4)
	class  v3dxIndexInSkeleton
	{
	public:
		inline v3dxIndexInSkeleton()
		{
			Value = -1;
		}
		inline v3dxIndexInSkeleton(int index)
		{
			Value = index;
		}
		int ToInt() const { return Value; };
	private:
		int Value;
	};
#pragma pack(pop)

	struct TR_CLASS(SV_LayoutStruct = 8)
		IBoneDesc
	{
		VNameString			Name;
		UINT				NameHash;
		VNameString			ParentName;
		UINT				ParentHash;
		//VNameString			GrantParent;
		//UINT				GrantParentHash;
		//float				GrantWeight;
		//Constraint			TransitionConstraint; 
		//Constraint			RotationConstraint;

		v3dxMatrix4			InitMatrix;
		v3dxMatrix4			InvInitMatrix;
		v3dxVector3			InvPos;
		v3dxVector3			InvScale;
		v3dxQuaternion		InvQuat;
	};

	class TR_CLASS(SV_Dispose = delete self)
		IBone : public VIUnknown
	{
	public:
		static IBone* Create(const IBoneDesc * desc);
		IBone(const IBoneDesc * desc);
	public:
		int GetChildNumber() const {
			return (int)Children.size();
		} 
		v3dxIndexInSkeleton GetChild(int index) const {
			if (index > Children.size())
				return -1;
			return Children[index];
		}
		void AddChild(v3dxIndexInSkeleton childIndex) {
			Children.push_back(childIndex);
		}
		void ClearChildren()
		{
			Children.clear();
		}
	public:
		v3dxIndexInSkeleton						ParentIndex;
		v3dxIndexInSkeleton						Index;
		std::vector<v3dxIndexInSkeleton>		Children;
		//std::vector<USHORT>			GrantChildren;
		const IBoneDesc* Desc;
	};


}

