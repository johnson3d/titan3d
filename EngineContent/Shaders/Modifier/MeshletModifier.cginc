StructuredBuffer<FMeshlet> MeshletsBuffer DX_AUTOBIND;
ByteAddressBuffer VerticesBuffer DX_AUTOBIND;
ByteAddressBuffer TrianglesBuffer DX_AUTOBIND;

void DoMeshletModifierVS(inout PS_INPUT vsOut, inout VS_MODIFIER vert)
{
    FMeshlet meshlet = MeshletsBuffer[vert.vInstanceId];
    //vert.vVertexID
    //VerticesBuffer[meshlet.VertexOffset + vert.vVertexID]

}

//#define DO_VS_MODIFIER DoSkinModifierVS