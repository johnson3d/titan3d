#pragma once
#include "../../Base/IUnknown.h"
#include "../../Base/debug//vfxdebug.h"
#include "../../Math/v3dxVector3.h"
#include "../../Math/v3dxQuaternion.h"
#include "../../NextRHI/NxRHI.h"

NS_BEGIN

struct TR_CLASS(SV_LayoutStruct = 8)
	IBaseParticleState
{
	UINT			Flags;
	float			Life;

	bool IsDeath() const {
		return (Life <= 0);
	}
};

struct TR_CLASS(SV_LayoutStruct = 8)
	FVarAttribute
{
	VNameString		Name;
	NxRHI::EShaderVarType	Type;
	UINT			Columns;
	UINT			Offset;
};

class TR_CLASS()
	IParticleSystemAttribute
{
	std::vector<FVarAttribute>		NamedAttributes;

	void AddAttribute(const char* name, NxRHI::EShaderVarType type)
	{
		FVarAttribute tmp;
		tmp.Name = name;
		tmp.Type = type;
		NamedAttributes.push_back(tmp);
	}
	void BuildAttributes(UINT AlignSize = 16);

	int FindAttribute(const char* name) const
	{
		for (int i = 0; i < (int)NamedAttributes.size(); i++)
		{
			if (NamedAttributes[i].Name == name)
				return i;
		}
		return -1;
	}
	FVarAttribute* GetAttributeAddress(int index)
	{
		return &NamedAttributes[index];
	}
};

template<typename T>
struct FPingpongBuffer
{
	std::vector<T>	Buffers[2];
	UINT			CurIndex;
	FPingpongBuffer()
	{
		CurIndex = 0;
	}
	const std::vector<T>& GetCurBuffer() const{
		return Buffers[CurIndex];
	}
	std::vector<T>& GetCurBuffer() {
		return Buffers[CurIndex];
	}
	const std::vector<T>& GetBackBuffer() const {
		return Buffers[CurIndex ^ 1];
	}
	std::vector<T>& GetBackBuffer(){
		return Buffers[CurIndex ^ 1];
	}
	void Clear() {
		Buffers[0].clear();
		Buffers[1].clear();
		CurIndex = 0;
	}
	void Swap() {
		CurIndex ^= 1;
	}
};

class IEmitter;
class IParticlePool
{
public:
	UINT					mDataStride;
	std::vector<BYTE>		mParticleArray;
	std::queue<UINT>		mFreeParticles;//need be a queue
	
	FPingpongBuffer<UINT>	mAliveBuffer;

	bool					mChanged;
	VSLLock					mLocker;
public:
	IParticlePool()
	{
		mDataStride = sizeof(float);
	}
	bool InitPool(UINT dataStride, UINT maxNum);
	UINT Alloc(IEmitter* pEmitter, UINT num, UINT flags, float life);
	bool IsChanged() const {
		return mChanged;
	}
	void Recycle(IEmitter* pEmitter);

	inline BYTE* GetParticleAddress() {
		return mParticleArray.data();
	}
	inline UINT* GetCurrentAliveAddress() {
		return mAliveBuffer.GetCurBuffer().data();
	}
	inline UINT* GetBackendAliveAddress() {
		return mAliveBuffer.GetBackBuffer().data();
	}
	inline IBaseParticleState* GetParticle(UINT index) {
		return (IBaseParticleState*)&mParticleArray[(size_t)index * mDataStride];
	}
	inline std::vector<UINT>* GetCurAlives() {
		return &mAliveBuffer.GetCurBuffer();
	}
	inline UINT GetLiveNumber() const{
		return (UINT)mAliveBuffer.GetCurBuffer().size();
	}
	inline UINT GetBackendNumber() const {
		return (UINT)mAliveBuffer.GetBackBuffer().size();
	}
	inline IBaseParticleState* GetLiveParticle(UINT index)
	{
		auto addr = (mAliveBuffer.GetCurBuffer())[index];
		return (IBaseParticleState*)&mParticleArray[addr * mDataStride];
	}
};

NS_END