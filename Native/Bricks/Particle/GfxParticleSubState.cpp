#include "GfxParticleSubState.h"

#define  new VNEW

NS_BEGIN

void GfxParticleSubState::SetShapeType(GfxParticleEmitterShape* shape)
{
	mShape.StrongRef(shape);
}

void GfxParticleSubState::SetShapeData(GfxParticleState* sub)
{
	mShape->GenEmissionPose(sub);
	//sub->mPose.mPosition += mPosition;
	/*sub->mPose.mPosition += mPosition;
	sub->mPose.mRotation.Multiply(&mEmitRotation);

	sub->mPose.mVelocity.x += mEmitVelocity;
	sub->mPose.mVelocity.y += mEmitVelocity;
	sub->mPose.mVelocity.z += mEmitVelocity;
	
	sub->mPose.mScale += mScale;

	sub->mPose.mColor.r += mColor.r;
	sub->mPose.mColor.g += mColor.g;
	sub->mPose.mColor.b += mColor.b;
	sub->mPose.mColor.a += mColor.a;*/
	//mParticleFrame;
}

void GfxParticleSubState::Simulate(float elaspedTime)
{
	for (auto i = mParticles.begin(); i != mParticles.end();)
	{
		auto p = *i;
		if (p->Host->IsAlive() == FALSE)
		{
			i = mParticles.erase(i);
		}
		else
		{
			i++;
		}
	}
}

NS_END

using namespace EngineNS;

template <>
struct Type2TypeConverter<v3dxVector3>
{
	typedef v3dVector3_t		TarType;
};

extern "C"
{
	Cpp2CS2(EngineNS, GfxParticleSubState, GetNewBorns);
	Cpp2CS0(EngineNS, GfxParticleSubState, PushNewBorns);
	Cpp2CS2(EngineNS, GfxParticleSubState, GetParticles);
	Cpp2CS1(EngineNS, GfxParticleSubState, SetShapeType);
	Cpp2CS0(EngineNS, GfxParticleSubState, GetShapeType);
	Cpp2CS1(EngineNS, GfxParticleSubState, Simulate);

	Cpp2CS0(EngineNS, GfxParticleSubState, GetPosition);
	Cpp2CS1(EngineNS, GfxParticleSubState, SetPosition);
	Cpp2CS0(EngineNS, GfxParticleSubState, GetDirection);
	Cpp2CS1(EngineNS, GfxParticleSubState, SetDirection);

	VFX_API int Inner_TestCall(int a, float b);

	VFX_API void SDK_TestCall(int num, int a, float b)
	{
		for (int i = 0; i < num; i++)
		{
			Inner_TestCall(a, b);
		}
	}
}