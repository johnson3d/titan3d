#ifndef _COMMON_FUNCTION_
#define _COMMON_FUNCTION_
#include "Math.cginc"

float4 SampleLevel2D(Texture2D tex, SamplerState samp, float2 uv, float level, out float3 outRgb)
{
	float4 clr = tex.SampleLevel(samp, uv, level);
	outRgb = clr.rgb;
	return clr;
}

float MipLevel(float2 uv, float SUB_TEXTURE_SIZE, float SUB_TEXTURE_MIPCOUNT)
{
	float2 dx = ddx(uv * SUB_TEXTURE_SIZE);
	float2 dy = ddy(uv * SUB_TEXTURE_SIZE);
	float d = max(dot(dx, dx), dot(dy, dy));

	// Clamp the value to the max mip level counts
	const float rangeClamp = pow(2, (SUB_TEXTURE_MIPCOUNT - 1) * 2);
	d = clamp(d, 1.0, rangeClamp);

	float mipLevel = 0.5 * log2(d);
	mipLevel = floor(mipLevel);

	return mipLevel;
}

void Clamp( half x, half min, half max, out half ret )
{
	ret = clamp( x, min, max );
}

void FMod( half x, half y, out half ret )
{
	ret = (half)fmod(x, y);
}

void FMod4D( half4 x, half4 y, out half4 ret )
{
	ret = (half)fmod(x, y);
}

void ModF( half x, out half ip, out half fp )
{
	fp = (half)modf(x, ip);
}

void ACos( half x, out half ret )
{
	ret = (half)acos(x);
}

void ASin( half x, out half ret )
{
	ret = (half)asin(x);
}

void ATan( half x, out half ret )
{
	ret = (half)atan(x);
}

void ATan2( half x, half y, out half ret )
{
	ret = (half)atan2(x, y);
}

void Ceil( half x, out half ret )
{
	ret = (half)ceil(x);
}

void Cos( half x, out half ret )
{
	ret = (half)cos(x);
}

void Sin( half x, out half ret )
{
	ret = (half)sin(x);
}

void Tan( half x, out half ret )
{
	ret = (half)tan(x);
}

void SinCos( half x, out half sin, out half cos )
{
	sincos(x,sin,cos);
}

void Sqrt( half x, out half ret )
{
	ret = (half)sqrt(x);
}

void RSqrt( half x, out half ret )
{
	ret = (half)rsqrt(x);
}

void Reflect3D( half3 v1, half3 normal, out half3 ret )
{
	ret = (half3)reflect(v1, normal);
}

void Cross3D( half3 v1, half3 v2, out half3 ret )
{
	ret = (half3)cross(v1, v2);
}

void Dot2D( half2 v1, half2 v2, out half ret )
{
	ret = (half)dot(v1, v2);
}

void Dot3D( half3 v1, half3 v2, out half ret )
{
	ret = (half)dot(v1, v2);
}

void Exp( half x, out half ret )
{
	ret = (half)exp(x);
}

void Exp2( half x, out half ret )
{
	ret = (half)exp2(x);
}

void Floor( half x, out half ret )
{
	ret = (half)floor(x);
}

void Frac( half x, out half ret )
{
	ret = (half)frac(x);
}

void Pow( half v1, half v2, out half ret )
{
	ret = (half)pow(v1,v2);
}

void Lerp( half v1, half v2, half s , out half ret )
{
	ret = (half)lerp(v1, v2,s);
}

void Lerp2D( half2 v1, half2 v2, half2 s , out half2 ret )
{
	ret = (half2)lerp(v1, v2,s);
}

void Lerp3D( half3 v1, half3 v2, half3 s , out half3 ret )
{
	ret = (half3)lerp(v1, v2,s);
}

void Saturate( half x, out half ret )
{
	ret = (half)saturate(x);
}

void SmoothStep( half minvalue, half maxvalue, half s , out half ret )
{
	ret = (half)smoothstep(minvalue, maxvalue,s);
}


void Luminance3( half3 color , out half ret  )
{
	const half3 RGB_TO_LUM = half3(0.2126f, 0.7152f, 0.0722f);
	ret = (half)dot(color, RGB_TO_LUM);
}

void AnimateUV( float time, float duration , float2 frameCount, float2 uv, out float2 finalUV )
{
    float2 FrameStep = 1/frameCount;

    float timeElapsedPercent = frac(time/duration);//fmod(time/duration,1);
//    float timeElapsedPercent = fmod(time, duration)/duration;
	float2 currFrame;
	float fCurrFrame = timeElapsedPercent * frameCount.x * frameCount.y;
	currFrame.x = (int)( modf( fCurrFrame / frameCount.x, currFrame.y )*frameCount.x );
	finalUV = (currFrame+uv) * FrameStep;
}

void SubUV( half index, half frameCount, half2 uv, out half2 finalUV )
{
    half FrameStep = 1/frameCount;

	half currFrame;
	half percent = index/frameCount;
	half temp;
	percent = (half)modf( percent, temp ); 
//	percent = fmod(index, frameCount);
	currFrame = percent * frameCount;
	finalUV.x = (currFrame+uv.x) * FrameStep;
	finalUV.y = uv.y;
}

void UnpackNormal( half3 packedNormal, out half3 normal )
{
	normal = packedNormal.xyz * 2 - 1;
	// normalmap�洢�ķ����������ǳ���Z�ģ� ����ת���ɳ���y
	//normal = half3( temp.x, temp.z, temp.y );
}

void ScreenPos( float4 projPos, out float2 screenPos )
{
  screenPos = projPos.xy / projPos.w;
  screenPos = (screenPos+1)*0.5;


  screenPos.y = 1-screenPos.y;   // uv�����෴

  screenPos.x += 1.0f* gViewportSizeAndRcp.z*0.5f;
  screenPos.y += 1.0f* gViewportSizeAndRcp.w*0.5f;
}

void Distortion( float4 localPos, float4 localNorm, float4 viewPos, float4 projPos, float3 localCameraPos, float strength, float transparency, float distortionOffset, out  float2 distortionUV, out float distortionAlpha) 
{
	float3 localViewDir = normalize(localCameraPos - localPos.xyz);
	float NdotV = dot(localNorm.xyz, localViewDir);
	float depth = viewPos.z + 1;

	float distortion = abs((NdotV / depth)*strength);
	distortionAlpha = distortion / transparency;

	// ���㵱ǰ��������ĻTexture�ϵ�uv����
	float2 screenPos;
	ScreenPos(projPos, screenPos);
	distortionUV = screenPos + distortionOffset * distortion; // �Ŷ�uv
}

void Distortion2( float4 localPos, float4 localNorm, float4 viewPos, float4 projPos, float strength, float transparency, float distortionOffset, out  float4 distortionColor) 
{
	float3 localViewDir = normalize(CameraPositionInModel - localPos.xyz);
	float NdotV = dot(localNorm.xyz, localViewDir);
	float depth = viewPos.z + 1;

	float distortion = abs((NdotV / depth)*strength);

	// ���㵱ǰ��������ĻTexture�ϵ�uv����
	float2 screenPos;
	ScreenPos(projPos, screenPos);
	float2 distortionUV = screenPos + distortionOffset * distortion; // �Ŷ�uv

	distortionColor.rgb = PreFrameBuffer.Sample(SampPreFrameBuffer,distortionUV).rgb;
	distortionColor.a = distortion / transparency;
}

float4	CalcWorldPosition(float4 PosProj, float vDepth)
{
	// Position
	float4 VPos = PosProj;
	VPos.xy /= VPos.ww;
	VPos.z = vDepth;
	VPos.w = 1.0f;
	// Inverse ViewProjection Matrix
	VPos = mul(VPos, ViewPrjInvMtx);
	VPos.xyz /= VPos.www;
	VPos.w = 1.0f;
	return VPos;
}

void PreFrameWorldYBias(float4 projPos, out float worldYBias)
{
	float2 screenPos;
	ScreenPos(projPos, screenPos);
	float sceneDepth = PreFrameDepth.Sample(SampPreFrameDepth, screenPos).r;

	float3 worldPos1 = mul(projPos, ViewPrjInvMtx).xyz;
	float3 worldPos2 = CalcWorldPosition(projPos, sceneDepth).xyz;

	worldYBias = worldPos1.y - worldPos2.y; // ��ǰ����ͳ�����worldY��}
}

float4	CalcViewPosition(float4 PosProj, float vDepth)
{
	// Position
	float4 VPos = PosProj;
	VPos.xy /= VPos.ww;
	VPos.z = vDepth;
	VPos.w = 1.0f;
	// Inverse Projection Matrix
	VPos = mul(VPos, PrjInvMtx);
	VPos.xyz /= VPos.www;
	VPos.w = 1.0f;
	return VPos;
}

void PreFrameDepthBias(float4 projPos, out float depthBias)
{
  float2 screenPos;
  ScreenPos(projPos, screenPos);
  float sceneDepth = PreFrameDepth.Sample(SampPreFrameDepth, screenPos).x;

  float3 viewPos1 = mul(projPos, PrjInvMtx).xyz;

#if defined(GLES)
  sceneDepth = sceneDepth * 2 - 1;
#endif
  float3 viewPos2 = CalcViewPosition(projPos, sceneDepth).xyz;

  depthBias = viewPos1.z - viewPos2.z;
}



void ZDisableDepthBiasAlpha(float4 projPos, float alpha, float dist, out float outAlpha)
{
	float depthBias = 0;
	PreFrameDepthBias(projPos, depthBias);

	outAlpha = 0;
	if(depthBias<=dist)
	{
		outAlpha = alpha;
	}
}

void DepthBiasAlpha( float4 projPos, float alphaDistance, out float alpha )
{
	/*
	alpha = 1;
	float depthBias = 0;
	PreFrameDepthBias(projPos, depthBias);

	if(depthBias>=0)
	{
		alpha = 1;
	}
	else
	{
		alpha = smoothstep(0, 1, abs(depthBias) / alphaDistance);
	}
	*/

	float depthBias = 0;
	PreFrameDepthBias(projPos, depthBias);
	depthBias = abs(depthBias);
	float s = alphaDistance - depthBias;
	if( s <= 0 )
	{
		alpha = 1.0f;
	}
	else
	{
		if(alphaDistance == 0)
			alpha = 1.0f;
		else
			alpha = lerp(0.0f, 1.0f, depthBias / alphaDistance);
	}

}

void Panner(float2 uv, float time, float2 speed, float2 scale, out float2 outUV)
{
	float2 uvTrans = time * speed;

	//matrix <half, 4, 4> scaleM = {
	//half4x4 scaleM = {
	//	1.0f, 0.0f, 0.0f, 0.0f, // row 1
	//	0.0f, 1.0f, 0.0f, 0.0f, // row 2
	//	0.0f, 0.0f, 1.0f, 0.0f, // row 3
	//	0.0f, 0.0f, 0.0f, 1.0f, // row 4
	//};

	if(scale.x==0 || scale.y==0)
	{
		outUV = uv;
	}
	else
	{

		//scaleM[0][0] = 1 / scale.x;
		//scaleM[1][1] = 1 / scale.y;
		// //Skip matrix concat since first matrix update
		//scaleM[3][0] = (-0.5f * scaleM[0][0]) + 0.5f;
		//scaleM[3][1] = (-0.5f * scaleM[1][1]) + 0.5f;

		//matrix <half, 4, 4> trans = {
		//half4x4 trans = {
		//	1.0f, 0.0f, 0.0f, 0.0f, // row 1
		//	0.0f, 1.0f, 0.0f, 0.0f, // row 2
		//	0.0f, 0.0f, 1.0f, 0.0f, // row 3
		//	0.0f, 0.0f, 0.0f, 1.0f, // row 4
		//};

		//trans[3][0] = uvTrans.x;
		//trans[3][1] = uvTrans.y;

		//half4 inUV = { uv.x, uv.y, 1.0f, 1.0f };
		//inUV = mul(inUV, scaleM);
		//outUV = mul(inUV, trans);
		//scale from the texture center;

		float2 UVScale = float2(1.0f / scale.x, 1.0f / scale.y);
		
		outUV.x = uv.x * UVScale.x + ( -0.5f * UVScale.x + 0.5f) + uvTrans.x;
		outUV.y = uv.y * UVScale.y + ( -0.5f * UVScale.y + 0.5f) + uvTrans.y;
	}
}

void Rotator(float2 uv, float time, float2 center, float2 scale, float speed, out float2 outUV)
{
	float angle = time * speed;

	//matrix <float, 4, 4> scaleM = {
	float4x4 scaleM = {
		1.0f, 0.0f, 0.0f, 0.0f, // row 1
		0.0f, 1.0f, 0.0f, 0.0f, // row 2
		0.0f, 0.0f, 1.0f, 0.0f, // row 3
		0.0f, 0.0f, 0.0f, 1.0f, // row 4
	};

	scaleM[0][0] = 1 / scale.x;
	scaleM[1][1] = 1 / scale.y;
	// Skip matrix concat since first matrix update
	scaleM[3][0] = (-0.5f * scaleM[0][0]) + 0.5f;
	scaleM[3][1] = (-0.5f * scaleM[1][1]) + 0.5f;

	//matrix <float, 4, 4> trans = {
	float4x4 trans = {
		1.0f, 0.0f, 0.0f, 0.0f, // row 1
		0.0f, 1.0f, 0.0f, 0.0f, // row 2
		0.0f, 0.0f, 1.0f, 0.0f, // row 3
		0.0f, 0.0f, 0.0f, 1.0f, // row 4
	};

	trans[3][0] = center.x;
	trans[3][1] = center.y;

	//matrix <float, 4, 4> rot = { 
	float4x4 rot = { 
		1.0f, 0.0f, 0.0f, 0.0f, // row 1
		0.0f, 1.0f, 0.0f, 0.0f, // row 2
		0.0f, 0.0f, 1.0f, 0.0f, // row 3
		0.0f, 0.0f, 0.0f, 1.0f, // row 4
	};

	float theta = radians(angle);
	float cosTheta = cos(theta);
	float sinTheta = sin(theta);


	rot[0][0] = cosTheta;
	rot[1][0] = -sinTheta;
	rot[0][1] = sinTheta;
	rot[1][1] = cosTheta;

	rot[3][0] = 0.5f + ((-0.5f * cosTheta) - (-0.5f * sinTheta));
	rot[3][1] = 0.5f + ((-0.5f * sinTheta) + (-0.5f * cosTheta));


	float4 inUV = { uv.x, uv.y, 1.0f, 1.0f };
	inUV = mul(inUV, scaleM);
	inUV = mul(inUV, trans);
	outUV = mul(inUV, rot).xy;
}

void RimLight( half3 localPos, half3 localNormal, half rimStart, half rimEnd, half4 rimColor, half rimMultiply, out half4 outColor )
{
	half l = (half)length(localNormal);
	if(l==0)
	{
		outColor = half4(0,0,0,0);
		return;
	}
    half3 N = (half3)normalize(localNormal);
    half3 V = (half3)normalize(CameraPositionInModel - localPos);
    half rim = (half)smoothstep(rimStart, rimEnd, 1- dot(N,V));

    outColor = rim* rimMultiply * rimColor;
}

void Luminance4( half4 color , out half ret  )
{
	const half3 RGB_TO_LUM = half3(0.2126f, 0.7152f, 0.0722f);
	half3 color3 = color.rgb;
	ret = dot(color3, RGB_TO_LUM);
}

void RimLightBloom( half3 localPos, half3 localNormal, half rimStart, half rimEnd, half4 rimColor, half rimMultiply, half4 objColor, int isRimBloom, out half4 outColor, out half outBloom )
{
	half l = (half)length(localNormal);
	half3 N = (half3)normalize(localNormal);
    half3 V = (half3)normalize(CameraPositionInModel - localPos);
    
	half rim = (half)smoothstep(rimStart, rimEnd, 1- dot(N,V));

    half4 borderColor = rim* rimMultiply * rimColor;

	outBloom = 0;
	if(isRimBloom>0)
	{
		half lum = 0;
		Luminance4( borderColor, lum );
		outBloom = lum;

		outColor = borderColor + objColor;
	}
	else
	{
		half objLum = 0;
		Luminance4( objColor, objLum );
		half t = objLum;
		//if(t<0.07)
		//	t = 0.07 + t;
		outColor = borderColor * t + objColor;
	}
}

void VecMultiplyQuat(half3 vec, half4 quat, out half3 outVector)
{
	half3 uv = (half3)cross(quat.xyz, vec);
	half3 uuv = (half3)cross(quat.xyz, uv);
	uv = uv * ((half)2.0f * quat.w);
	uuv *= (half)2.0f;
	
	outVector = vec + uv + uuv;
}

void VecMulMatrix(half4 vec, half4x4 mat, out half4 outVector)
{
	outVector = mul(vec, mat);
}

void NormalMap(half3 Nt, half4 Tw, half3 Nw, out half3 UnpackedNormal)
{
	half3 Bw = half3(0.0h, 0.0h, 0.0h);
	if (Tw.w > 0.0h)
	{
		Bw = -cross(Tw.xyz, Nw);
	}
	else
	{
		Bw = cross(Tw.xyz, Nw);
	}
	half3x3 TBN = half3x3(Tw.xyz, Bw, Nw);

	Nt.xy = Nt.xy * 2.0h - 1.0h;
	Nt.z = sqrt(saturate(1.0h - dot(Nt.xy, Nt.xy)));
	
	UnpackedNormal = mul(Nt, TBN);
}

void BlingSpec(half4 flo4 , half intensity , half powIn , half4 worldNorm , half4 worldPos , half3 LP ,  out half BlingSpec)
{
	half texlum;
	Luminance4(flo4 , texlum);
	half3 LightVec= LP - worldPos.xyz;
	half3 ViewVec = (half3)CameraPositionInModel - worldPos.xyz ;
	half3 VLn = (half3)normalize(ViewVec + LightVec);
	half VLdN;
	VLdN = (half)dot(VLn, worldNorm.xyz);
	BlingSpec = (half)pow(VLdN , powIn) * texlum ;
}

half3 smoothstep3D(half3 InColor )
{	
	half3 OutColor ;
	OutColor.x = (half)smoothstep( 0 , 1 , InColor.x );
	OutColor.y = (half)smoothstep( 0 , 1 , InColor.y );
	OutColor.z = (half)smoothstep( 0 , 1 , InColor.z );
	return OutColor ;
}

half3 floor3D(half3 InColor)
{
	half3 OutColor;
	OutColor.x = (half)floor(InColor.x);
	OutColor.y = (half)floor(InColor.y);
	OutColor.z = (half)floor(InColor.z);
	return OutColor;
}

//void CartoonColorFliter(half4 InColor, half steps, half threshold , out half4 OutColor )
//{		
//	InColor = (InColor+1)/2 ;
//	half3 colorRGB; 
//	colorRGB = smoothstep3D ( InColor.xyz );
//	half3 toon = floor3D((colorRGB * steps) / steps);
//	OutColor = (lerp( colorRGB , toon , threshold ),InColor.w);
//}

void PolarCoodP2D(half2 uv,out half2 polar)
{
	half a,b,x,y;
	x = uv.x ;
	y = 1 - uv.y ;
	a = (half)(0.5-y*0.5/1*sin(x*2*3.1415926/1));
	b = (half)(0.5+y*0.5/1*cos(x*2*3.1415926/1));
	polar.x = a;
	polar.y = b;
}

void PolarCoodD2P(half2 uv,out half2 polar)
{
	half pi;
	pi = 3.1415926;
	half alpha;
	half a,b,x,y,x1,y1,r1,r2;
	x = uv.x ;
	y = 1 - uv.y ;
	a = y - 0.5;
	b = 0.5 - x;
	alpha = (half)atan(b/a);
	if( a<=0 )
		alpha = alpha + pi;
	if( a>0 && b<=0 )
		alpha = alpha + 2*pi;
	r1 = (half)(b/sin(alpha)) ;
	y1 = r1*2;
	x1 = alpha * 1 /( 2*pi);
	x = x1;
	y = y1;
	polar.x = x;
	polar.y = y;
}

void Vortex(half2 uv, half degree , out half2 VortexUV)
{
	half pi;
	pi = 3.1415926;
	half alpha;
	half a,b,x,y,x1,y1,r;
	x = uv.x ;
	y = 1 - uv.y ;
	a = y - 0.5;
	b = 0.5 - x;
	if(a==0)
	{
		a = 0+0.0001;
		alpha = (half)atan(b/a);
	}
	if(a<0)
		alpha = alpha + pi;
	if(a>0)
		alpha = pi/2;
	r = (half)sqrt(a*a+b*b);
	alpha = alpha + r/degree;
	x = r * (half)cos(degree);
	y = r * (half)sin(degree);
	x = x + 0.5;
	y = 0.5 - y;
	VortexUV.x = x;
	VortexUV.y = y;
}

void FlattenNormal(half3 normal, half flatness, out half3 ret)
{
	half3 z = half3(0,0,1);
	ret = (half)lerp(normal, z, flatness);
}

struct MaterialAttribute
{
	half3 baseColor;
	half specular;
	half metallic;
	half roughness;
	half3 normal;
};
half4x4 _EncodeMaterialAttribute(MaterialAttribute ma)
{
	half4x4 m = (half4x4)0.0h;
	m[0].xyz = ma.baseColor.xyz;
	m[1].x = ma.specular;
	m[1].y = ma.metallic;
	m[1].z = ma.roughness;
	m[2].xyz = ma.normal.xyz;
	//m[0] = half4(ma.baseColor, 0.0h);
	//m[1] = half4(ma.specular, ma.metallic, ma.roughness, 0.0h);
	//m[2] = half4(ma.normal, 0.0h);
	return m;
}
MaterialAttribute _DecodeMaterialAttribute(half4x4 m)
{
	MaterialAttribute ma = (MaterialAttribute)0.0h;
	ma.baseColor = m[0].xyz;
	ma.specular = m[1].x;
	ma.metallic = m[1].y; 
	ma.roughness = m[1].z;
	ma.normal = m[2].xyz;
	return ma;
}
void MakeMaterialAttribute(half3 baseColor, half specular, half metallic, half roughness, half3 normal, out half4x4 m)
{
	MaterialAttribute ma = (MaterialAttribute)0;
	ma.baseColor = baseColor;
	ma.specular = specular;
	ma.metallic = metallic;
	ma.roughness = roughness;
	ma.normal = normal;

	m = _EncodeMaterialAttribute(ma);
}
void BreakMaterialAttribute(half4x4 m, out half3 baseColor, out half specular, out half metallic, out half roughness, out half3 normal)
{
	MaterialAttribute ma = _DecodeMaterialAttribute(m);

	baseColor = ma.baseColor;
	specular = ma.specular;
	metallic = ma.metallic;
	roughness = ma.roughness;
	normal = ma.normal;
}
void LerpMaterial(half4x4 m1, half4x4 m2, half alpha, out half4x4 m)
{
	MaterialAttribute ma = (MaterialAttribute)0;
	MaterialAttribute ma1 = _DecodeMaterialAttribute(m1);
	MaterialAttribute ma2 = _DecodeMaterialAttribute(m2);
	ma.baseColor = (half)lerp(ma1.baseColor, ma2.baseColor, alpha);
	ma.specular = (half)lerp(ma1.specular, ma2.specular, alpha);
	ma.metallic = (half)lerp(ma1.metallic, ma2.metallic, alpha);
	ma.roughness = (half)lerp(ma1.roughness, ma2.roughness, alpha);
	ma.normal = (half3)lerp(ma1.normal, ma2.normal, alpha);
	m = _EncodeMaterialAttribute(ma);
}
void M_BaseColorTintAndConstrast(half3 baseColor, half mask, half constrast, half3 tint, out half3 ret)
{
	half a1 = (half)lerp(0.5, 1, constrast);
	half3 c = (half3)clamp(lerp(0.65, baseColor, a1),0,1) * tint;
	ret = (half3)lerp(baseColor, c, mask);
}
void M_RoughnessAdjustment(half baseRoughness, half baseRoughnessMin, half baseRoughnessMax, half grimeRoughness, half grimeMask, half scratchRoughness, half scratchMask, out half ret)
{
	ret = (half)lerp(baseRoughnessMin, baseRoughnessMax, baseRoughness);
	ret = (half)lerp(ret, grimeRoughness, grimeMask);
	ret = (half)lerp(ret, scratchRoughness, scratchMask);
}
void ConstantBiasScale(half3 v, half bias, half scale, half3 ret)
{
	ret = (v + bias)*scale;
}
void BlendAngleCorrectedNormals(half3 base, half3 additional, out half3 ret)
{
	half3 t = (base+1)*0.5 * half3(2, 2, 2) + half3(-1, -1, 0);
	half3 u = (additional + 1)*0.5 * half3(-2, -2, 2) + half3(1, 1, -1);
	ret = (half3)normalize(t*dot(t, u) - u*t.z);

	t = base * half3(2, 2, 2) + half3(-1, -1, 0);
	u = additional*0.5 * half3(-2, -2, 2) + half3(1, 1, -1);
	ret = (half3)normalize(t*dot(t, u) / t.z - u);

	ret = (half3)normalize(half3(base.xy + additional.xy, base.z));
}

void MetalicShading(half3 baseColor, half3 worldNormal, half3 worldPos, out half3 ret)
{
	half3 V = (half3)normalize(CameraPosition - worldPos);
	half NoV = clamp(dot(worldNormal, V), 0, 1);
	ret = (half3)(baseColor * (pow(1 - NoV, 4) + lerp(0.3, 0.9, pow(NoV, 2))));
}


void CartesianToLatLongTexcoord(float3 dir, out float2 uv)
{
	float u = (1.0 + atan2(dir.x, -dir.z) / 3.1415926);
	float v = acos(dir.y) / 3.1415926;
	uv = float2(u * 0.5, v);
}

void CalcParallaxRefractionPBR(half3 N, half3 V, half IOR, half IrisDepth, half ParallaxScale, out half2 UVOffset)
{
	IOR = IOR * max(dot(N, V), 0.0h);
	half3 Vrefracted = normalize(-N * IOR - V);
	//half c1 = sqrt(1.0h + (c0 - IOR) * (c0 + IOR));
	//half3 Vrefracted = normalize((c0 - c1) * N - IOR * V);
	//half3 OffsetW = IrisDepth / dot(EyeDir, -Vrefracted) * Vrefracted;
	//half3 OffsetW = IrisDepth / dot(half3(0.0h, 0.0h, -1.0h), -Vrefracted) * Vrefracted;
	//UVOffset = mul(half4(OffsetW, 0.0h), WorldMatrixInverse).xy;
	UVOffset = (half2)mul(half4(Vrefracted, 0.0h), WorldMatrixInverse).xy * IrisDepth * ParallaxScale;
	UVOffset.y = -UVOffset.y;
}

void EyeParallaxMapping(half3 V, half IrisDepth, half ParallaxScale, out half2 UVOffset)
{
	half2 Vlocal = (half2)mul(half4(V, 0.0h), WorldMatrixInverse).xy;
	UVOffset = IrisDepth * Vlocal * ParallaxScale;
	UVOffset.x = -UVOffset.x;
}

void Normalize(half3 Vec3, out half3 Vec3Normalized)
{
	Vec3Normalized = normalize(Vec3);
}

void Min(half x, half y, out half MinValue)
{
	MinValue = min(x, y);
}

void Max(half x, half y, out half MaxValue)
{
	MaxValue = max(x, y);
}

void UInt32x4_Colume0_To_UInt4(uint4 src, out uint4 tar)
{
	tar[0] = src[0] & 0xff;
	tar[1] = src[1] & 0xff;
	tar[2] = src[2] & 0xff;
	tar[3] = src[3] & 0xff;
}

void UInt32x4_Colume1_To_UInt4(uint4 src, out uint4 tar)
{
	tar[0] = (src[0] >> 8) & 0xff;
	tar[1] = (src[1] >> 8) & 0xff;
	tar[2] = (src[2] >> 8) & 0xff;
	tar[3] = (src[3] >> 8) & 0xff;
}

void UInt32x4_Colume2_To_UInt4(uint4 src, out uint4 tar)
{
	tar[0] = (src[0] >> 16) & 0xff;
	tar[1] = (src[1] >> 16) & 0xff;
	tar[2] = (src[2] >> 16) & 0xff;
	tar[3] = (src[3] >> 16) & 0xff;
}

void UInt32x4_Colume3_To_UInt4(uint4 src, out uint4 tar)
{
	tar[0] = (src[0] >> 24) & 0xff;
	tar[1] = (src[1] >> 24) & 0xff;
	tar[2] = (src[2] >> 24) & 0xff;
	tar[3] = (src[3] >> 24) & 0xff;
}

void UInt4_To_Color4(uint4 src, out half4 tar)
{
	tar[0] = (half)src[0] / 255.0h;
	tar[1] = (half)src[1] / 255.0h;
	tar[2] = (half)src[2] / 255.0h;
	tar[3] = (half)src[3] / 255.0h;
}

#endif