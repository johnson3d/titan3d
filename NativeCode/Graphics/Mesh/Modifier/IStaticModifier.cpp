#include "IStaticModifier.h"

#define new VNEW

NS_BEGIN

IStaticModifier::IStaticModifier()
{

}

void IStaticModifier::SetInputStreams(NxRHI::FMeshPrimitives* mesh, NxRHI::FVertexArray* vao)
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
}

void IStaticModifier::GetInputStreams(UINT* pOutStreams) 
{
	*pOutStreams |= ((1 << NxRHI::VST_Position) | (1 << NxRHI::VST_Normal) | (1 << NxRHI::VST_Tangent) | (1 << NxRHI::VST_UV) | (1 << NxRHI::VST_Color));
}

void IStaticModifier::GetProvideStreams(UINT* pOutStreams)
{
	*pOutStreams |= ((1 << NxRHI::VOT_Position) | (1 << NxRHI::VOT_Normal) | (1 << NxRHI::VOT_Tangent) | (1 << NxRHI::VOT_WorldPos) | (1 << NxRHI::VOT_UV) | (1 << NxRHI::VOT_Color));
}

NS_END
