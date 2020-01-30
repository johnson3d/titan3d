#pragma once
#include "../../RHIRenderer/PreHead.h"
#include <algorithm>
#include <sstream>

#define NOMINMAX
#undef max
#undef min

#include "../../3rd/glslang/glslang/Public/ShaderLang.h"
#include "../../3rd/glslang/SPIRV/GlslangToSpv.h"
#include "../../3rd/glslang/SPIRV/disassemble.h"
#include "../../3rd/glslang/SPIRV/doc.h"
//#include "../../3rd/glslang/SPIRV/SPVRemapper.h"

namespace ShaderCC
{
	struct ShaderResult {
		EShLanguage shaderName;
		std::string output;
		std::string error;
	};

	struct GlslangResult {
		std::vector<ShaderResult> shaderResults;
		std::string linkingOutput;
		std::string linkingError;
		std::string spirvWarningsErrors;
		std::string spirv;  // Optional SPIR-V disassembly text.
	};

	enum class Source {
		GLSL,
		HLSL,
	};

	// Enum for shader compilation semantics.
	enum class Semantics {
		OpenGL,
		Vulkan
	};

	// Enum for compilation target.
	enum class Target {
		AST,
		Spv,
		BothASTAndSpv,
	};
	class GlslangHelper
	{
		int defaultVersion;
		EProfile defaultProfile;
		bool forceVersionProfile;
		bool isForwardCompatible;
	public:
		GlslangHelper()
		{
			defaultVersion = 100;
			defaultProfile = ENoProfile;
			forceVersionProfile = false;
			isForwardCompatible = false;
		}
		EShMessages DeriveOptions(Source source, Semantics semantics, Target target);
		GlslangResult compileAndLink(
			EShLanguage stage, const std::string& code,
			const std::string& entryPointName, EShMessages controls,
			glslang::EShTargetClientVersion clientTargetVersion,
			bool flattenUniformArrays = false,
			EShTextureSamplerTransformMode texSampTransMode = EShTexSampTransKeep,
			bool enableOptimizer = false,
			bool automap = true);

		bool compile(glslang::TShader* shader, const std::string& code,
			const std::string& entryPointName, EShMessages controls,
			const TBuiltInResource* resources);

		void outputResultToStream(std::ostringstream* stream,
			const GlslangResult& result,
			EShMessages controls);
	};
}