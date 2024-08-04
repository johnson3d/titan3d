
void DoParticleModifierVS(inout PS_INPUT vsOut, inout VS_MODIFIER vert)
{
    float3 Pos = vert.vInstPos.xyz + QuatRotateVector(vert.vPosition * vert.vInstScale.xyz, vert.vInstQuat);
	
	vert.vPosition.xyz = Pos;
	vert.vNormal.xyz = ParticleRotateVec(vert.vNormal.xyz, vert.vInstQuat);
}
