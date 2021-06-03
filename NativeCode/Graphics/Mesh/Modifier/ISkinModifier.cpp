#include "ISkinModifier.h"
#include "../../../Bricks/Animation/Pose/IBonePose.h"

#define new VNEW

NS_BEGIN

void ISkinModifier::SetInputStreams(IMeshPrimitives* mesh, IVertexArray* vao)
{
	auto geom = mesh->GetGeomtryMesh();

	auto vb = geom->GetVertexBuffer(VST_Position);
	vao->BindVertexBuffer(VST_Position, vb);

	vb = geom->GetVertexBuffer(VST_Normal);
	vao->BindVertexBuffer(VST_Normal, vb);

	vb = geom->GetVertexBuffer(VST_Tangent);
	vao->BindVertexBuffer(VST_Tangent, vb);

	vb = geom->GetVertexBuffer(VST_UV);
	vao->BindVertexBuffer(VST_UV, vb);

	vb = geom->GetVertexBuffer(VST_Color);
	vao->BindVertexBuffer(VST_Color, vb);

	vb = geom->GetVertexBuffer(VST_SkinIndex);
	vao->BindVertexBuffer(VST_SkinIndex, vb);

	vb = geom->GetVertexBuffer(VST_SkinWeight);
	vao->BindVertexBuffer(VST_SkinWeight, vb);
}

void ISkinModifier::GetInputStreams(DWORD& pOutStreams)
{
	pOutStreams |= ((1 << VST_Position) | (1 << VST_Normal) | (1 << VST_Tangent) | (1 << VST_UV) | (1 << VST_Color) | (1 << VST_SkinIndex) | (1 << VST_SkinWeight));
}

void ISkinModifier::GetProvideStreams(DWORD& pOutStreams)
{
	pOutStreams |= ((1 << VOT_Position) | (1 << VOT_Normal) | (1 << VOT_Tangent) | (1 << VOT_WorldPos) | (1 << VOT_UV) | (1 << VOT_Color) | (1 << VST_SkinIndex) | (1 << VST_SkinWeight));
}

bool ISkinModifier::SetToRenderStream(IConstantBuffer* cb, int AbsBonePos, int AbsBoneQuat, ISkeletonPose* skeletonPose)
{
	if (mPartialSkeleton== nullptr)
		return FALSE;
	if (skeletonPose == nullptr)
		return FALSE;
	//shader buffer大小改成了 360个骨骼支持。Shaders\CoreShader\Modifier\SkinModifier.var
	v3dxQuaternion* absPos = (v3dxQuaternion*)cb->GetVarPtrToWrite(AbsBonePos, (int)mPartialSkeleton->GetBoneNumber() * sizeof(v3dxQuaternion));
	if (absPos == nullptr)
		return FALSE;
	v3dxQuaternion* absQuat = (v3dxQuaternion*)cb->GetVarPtrToWrite(AbsBoneQuat, (int)mPartialSkeleton->GetBoneNumber() * sizeof(v3dxQuaternion));
	if (absPos == nullptr)
		return FALSE;
	const auto& bones = mPartialSkeleton->GetBones();
	for (size_t i = 0; i < bones.size(); i++)
	{
		IBone* bone = bones[i];
		if (bone != nullptr)
		{
			//这里进行translateRetarget,？？？？根据选项选择用自己的shared，自己的transform等等
			v3dxTransform trans;
			const IBoneDesc* shared = nullptr;
			EngineNS::IBonePose* outBone = skeletonPose->FindBonePose(bone->Desc->Name);
			if (outBone)
			{
				trans = outBone->Transform;
				shared = bone->Desc;
			}
			if (shared == nullptr)
			{
				continue;
			}
			*((v3dxVector3*)absPos) = trans.Position + trans.Rotation * shared->InvPos;
			absPos->w = 0;
			*absQuat = shared->InvQuat * trans.Rotation;
	
		}
		absPos++;
		absQuat++;
	}
	return true;
}

ISkinModifier::ISkinModifier()
{

}

NS_END