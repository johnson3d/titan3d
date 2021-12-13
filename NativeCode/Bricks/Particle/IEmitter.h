#pragma once
#include "IEffector.h"

NS_BEGIN

class TR_CLASS()
	IEmitter : public VIUnknownBase
{
public:
	std::vector<IEffector*>	mEffectors;
	IParticlePool			mPool;

public:
	IEmitter()
	{
		
	}
	~IEmitter();
	void InitEmitter(UINT dataStide, UINT maxParticle);
	void PushEffector(IEffector* effector);
	
	UINT Spawn(UINT num, UINT flags, float life);
	void Update(float elapsed);

	void UpdateLife(float elapsed);
	void Recycle() {
		mPool.Recycle(this);
	}
	bool IsChanged() const {
		return mPool.IsChanged();
	}
	inline BYTE* GetParticleAddress() {
		return mPool.GetParticleAddress();
	}
	inline UINT* GetCurrentAliveAddress() {
		return mPool.GetCurrentAliveAddress();
	}
	inline UINT* GetBackendAliveAddress() {
		return mPool.GetBackendAliveAddress();
	}
	inline UINT GetLiveNumber() const {
		return mPool.GetLiveNumber();
	}
	inline UINT GetBackendNumber() const {
		return mPool.GetBackendNumber();
	}
	virtual void OnInitParticle(IBaseParticleState* state)
	{

	}
};


//
//class TR_CLASS(SV_BaseFunction = true)
//	ISimpleParticleEmitter : public IEmitter<ISimpleParticleState>
//{
//	virtual void OnInitParticle(ISimpleParticleState * particle)
//	{
//		particle->Location.setValue(0, 10, 0);
//	}
//};
//
//class TR_CLASS(SV_BaseFunction = true)
//	ISimpleParticleAcceleratedEffector : public IEffector<ISimpleParticleState>
//{
//public:
//	v3dxVector3 mAcceleration;
//	ISimpleParticleAcceleratedEffector()
//	{
//		mAcceleration.setValue(0, -9.8f, 0);
//	}
//	virtual void DoEffect(VIUnknownBase* emitter, ISimpleParticleState* particle) override
//	{
//		particle->Location += mAcceleration;
//	}
//};

NS_END

