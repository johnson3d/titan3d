
struct FVoxelDebugger
{
	float3		Position;	
	float		Scale;

	float3		Color;
	float		Pad0;
};

StructuredBuffer<FVoxelDebugger> VxDebugInstanceSRV DX_AUTOBIND;//: register(t13);
void DoVoxelDebugMeshVS(inout PS_INPUT vsOut, inout VS_MODIFIER vert)
{
	FVoxelDebugger vx = VxDebugInstanceSRV[vert.vInstanceId];
	vsOut.vPosition.xyz = vert.vPosition * vx.Scale + vx.Position;
	//vsOut.vWorldPos = vert.vPosition * vx.Scale + vx.Position;

	vsOut.psCustomUV1.xyz = vx.Color.xyz;
}

void DoVoxelDebugMeshPS(inout PS_INPUT input, inout MTL_OUTPUT mtl)
{
	mtl.mAlbedo = input.psCustomUV1.rgb;//float3(0.5,0.5,0.5);
}