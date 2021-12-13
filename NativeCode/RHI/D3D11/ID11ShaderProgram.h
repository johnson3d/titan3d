#pragma once
#include "../IShaderProgram.h"
#include "D11PreHead.h"

NS_BEGIN

class ID11RenderContext;

class ID11ShaderProgram : public IShaderProgram
{
public:
	ID11ShaderProgram();
	~ID11ShaderProgram();

	virtual vBOOL LinkShaders(IRenderContext* rc) override;
	virtual void ApplyShaders(ICommandList* cmd) override;
public:
	bool Init(ID11RenderContext* rc, const IShaderProgramDesc* desc);
};

NS_END