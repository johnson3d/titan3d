
float3 ParticleRotateVec(in float3 inPos, in float4 inQuat)
{
	float3 uv = cross(inQuat.xyz, inPos);
	float3 uuv = cross(inQuat.xyz, uv);
	uv = uv * (2.0f * inQuat.w);
	uuv *= 2.0f;
	
	return inPos + uv + uuv;
}

void DoParticleModifierVS(inout PS_INPUT vsOut, inout VS_INPUT vert)
{
	float3 Pos = vert.vInstPos.xyz + ParticleRotateVec(vert.vPosition * vert.vInstScale.xyz, vert.vInstQuat);
	
	vert.vPosition.xyz = Pos;
	vert.vNormal.xyz = ParticleRotateVec(vert.vNormal.xyz, vert.vInstQuat);
}
