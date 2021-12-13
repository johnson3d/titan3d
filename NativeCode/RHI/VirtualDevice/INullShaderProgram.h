#pragma once
#include "../IShaderProgram.h"

NS_BEGIN

class INullRenderContext;
class INullShaderProgram : public IShaderProgram
{
public:
	INullShaderProgram();
	~INullShaderProgram();

	virtual vBOOL LinkShaders(IRenderContext* rc) override;
	virtual void ApplyShaders(ICommandList* cmd) override;
public:
	
public:
	bool Init(INullRenderContext* rc, const IShaderProgramDesc* desc);
};

NS_END