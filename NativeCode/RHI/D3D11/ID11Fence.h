#pragma once
#include "../IFence.h"
#include "D11PreHead.h"

NS_BEGIN

class ID11Fence : public IFence
{
public:
	ID11Fence();
	~ID11Fence();
	virtual void Wait();
	virtual bool IsCompletion();

public:
	ID3D11Query*			mQuery;
	ID3D11DeviceContext*	mContext;
	bool Init(ID3D11Device* pDevice);
};

NS_END