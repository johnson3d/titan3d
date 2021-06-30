#include "IStaticModifier.h"

#define new VNEW

NS_BEGIN

IStaticModifier::IStaticModifier()
{

}

void IStaticModifier::SetInputStreams(IMeshPrimitives* mesh, IVertexArray* vao)
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
}

void IStaticModifier::GetInputStreams(UINT* pOutStreams) 
{
	*pOutStreams |= ((1 << VST_Position) | (1 << VST_Normal) | (1 << VST_Tangent) | (1 << VST_UV) | (1 << VST_Color));
}

void IStaticModifier::GetProvideStreams(UINT* pOutStreams)
{
	*pOutStreams |= ((1 << VOT_Position) | (1 << VOT_Normal) | (1 << VOT_Tangent) | (1 << VOT_WorldPos) | (1 << VOT_UV) | (1 << VOT_Color));
}

NS_END
