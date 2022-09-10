#include "ISkinModifier.h"

#define new VNEW

NS_BEGIN

void ISkinModifier::SetInputStreams(NxRHI::FMeshPrimitives* mesh, NxRHI::FVertexArray* vao)
{
	auto geom = mesh->GetGeomtryMesh();

	auto vb = geom->GetVertexBuffer(NxRHI::VST_Position);
	vao->BindVB(NxRHI::VST_Position, vb);

	vb = geom->GetVertexBuffer(NxRHI::VST_Normal);
	vao->BindVB(NxRHI::VST_Normal, vb);

	vb = geom->GetVertexBuffer(NxRHI::VST_Tangent);
	vao->BindVB(NxRHI::VST_Tangent, vb);

	vb = geom->GetVertexBuffer(NxRHI::VST_UV);
	vao->BindVB(NxRHI::VST_UV, vb);

	vb = geom->GetVertexBuffer(NxRHI::VST_Color);
	vao->BindVB(NxRHI::VST_Color, vb);

	vb = geom->GetVertexBuffer(NxRHI::VST_SkinIndex);
	vao->BindVB(NxRHI::VST_SkinIndex, vb);

	vb = geom->GetVertexBuffer(NxRHI::VST_SkinWeight);
	vao->BindVB(NxRHI::VST_SkinWeight, vb);
}

void ISkinModifier::GetInputStreams(UINT* pOutStreams)
{
	*pOutStreams |= ((1 << NxRHI::VST_Position) | (1 << NxRHI::VST_Normal) | (1 << NxRHI::VST_Tangent) | (1 << NxRHI::VST_UV) | (1 << NxRHI::VST_Color) | (1 << NxRHI::VST_SkinIndex) | (1 << NxRHI::VST_SkinWeight));
}

void ISkinModifier::GetProvideStreams(UINT* pOutStreams)
{
	*pOutStreams |= ((1 << NxRHI::VOT_Position) | (1 << NxRHI::VOT_Normal) | (1 << NxRHI::VOT_Tangent) | (1 << NxRHI::VOT_WorldPos) | (1 << NxRHI::VOT_UV) | (1 << NxRHI::VOT_Color) | (1 << NxRHI::VST_SkinIndex) | (1 << NxRHI::VST_SkinWeight));
}

//bool ISkinModifier::FlushSkinPose(IConstantBuffer* cb, int AbsBonePos, int AbsBoneQuat, IPartialSkeleton* partialSkeleton,ISkeletonPose* skeletonPose)
//{
//	if (partialSkeleton == nullptr)
//		return FALSE;
//	if (skeletonPose == nullptr)
//		return FALSE;
//	//shader buffer
//	v3dxQuaternion* absPos = (v3dxQuaternion*)cb->GetVarPtrToWrite(AbsBonePos, (int)partialSkeleton->GetBonesNum() * sizeof(v3dxQuaternion));
//	if (absPos == nullptr)
//		return FALSE;
//	v3dxQuaternion* absQuat = (v3dxQuaternion*)cb->GetVarPtrToWrite(AbsBoneQuat, (int)partialSkeleton->GetBonesNum() * sizeof(v3dxQuaternion));
//	if (absPos == nullptr)
//		return FALSE;
//	const auto& bones = partialSkeleton->GetBones();
//	for (size_t i = 0; i < bones.size(); i++)
//	{
//		IBone* bone = bones[i];
//		if (bone != nullptr)
//		{
//			v3dxTransform trans;
//			const IBoneDesc* shared = nullptr;
//			EngineNS::IBonePose* outBone = skeletonPose->FindBonePose(bone->Desc.Name);
//			if (outBone)
//			{
//				trans = outBone->Transform;
//				shared = &bone->Desc;
//			}
//			if (shared == nullptr)
//			{
//				continue;
//			}
//			*((v3dxVector3*)absPos) = trans.Position + trans.Rotation * shared->InvPos;
//			absPos->w = 0;
//			*absQuat = shared->InvQuat * trans.Rotation;
//	
//		}
//		absPos++;
//		absQuat++;
//	}
//	return true;
//}

ISkinModifier::ISkinModifier()
{

}

NS_END