#pragma once
#include "../../Graphics/GfxPreHead.h"

#include "../../3rd/ShaderConductor/Include/ShaderConductor/ShaderConductor.hpp"

NS_BEGIN

class CscShaderConductor : public VIUnknown
{
public:
	static CscShaderConductor* GetInstance();
	struct Includer
	{
		std::string cbPerInstance_var;
		std::string dummy_gen;
	};
	bool CompileHLSL(IShaderDesc* desc, std::string incRoot, std::string hlsl, LPCSTR entry, std::string sm, 
				const IShaderDefinitions* defines, Includer* inc, bool hasGLSL, bool hasMetal);
};

NS_END
