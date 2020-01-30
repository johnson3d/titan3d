#include "Glsl2Vulkan.h"

#define new VNEW

//#pragma comment(lib, "glslang.lib")
//#pragma comment(lib, "HLSL.lib")
//#pragma comment(lib, "OGLCompiler.lib")
//#pragma comment(lib, "OSDependent.lib")
//#pragma comment(lib, "SPIRV.lib")
//#pragma comment(lib, "SPIRV-Tools.lib")
//#pragma comment(lib, "SPIRV-Tools-opt.lib")
//#pragma comment(lib, "SPVRemapper.lib")

namespace ShaderCC
{
	const TBuiltInResource DefaultTBuiltInResource = {
		/* .MaxLights = */ 32,
		/* .MaxClipPlanes = */ 6,
		/* .MaxTextureUnits = */ 32,
		/* .MaxTextureCoords = */ 32,
		/* .MaxVertexAttribs = */ 64,
		/* .MaxVertexUniformComponents = */ 4096,
		/* .MaxVaryingFloats = */ 64,
		/* .MaxVertexTextureImageUnits = */ 32,
		/* .MaxCombinedTextureImageUnits = */ 80,
		/* .MaxTextureImageUnits = */ 32,
		/* .MaxFragmentUniformComponents = */ 4096,
		/* .MaxDrawBuffers = */ 32,
		/* .MaxVertexUniformVectors = */ 128,
		/* .MaxVaryingVectors = */ 8,
		/* .MaxFragmentUniformVectors = */ 16,
		/* .MaxVertexOutputVectors = */ 16,
		/* .MaxFragmentInputVectors = */ 15,
		/* .MinProgramTexelOffset = */ -8,
		/* .MaxProgramTexelOffset = */ 7,
		/* .MaxClipDistances = */ 8,
		/* .MaxComputeWorkGroupCountX = */ 65535,
		/* .MaxComputeWorkGroupCountY = */ 65535,
		/* .MaxComputeWorkGroupCountZ = */ 65535,
		/* .MaxComputeWorkGroupSizeX = */ 1024,
		/* .MaxComputeWorkGroupSizeY = */ 1024,
		/* .MaxComputeWorkGroupSizeZ = */ 64,
		/* .MaxComputeUniformComponents = */ 1024,
		/* .MaxComputeTextureImageUnits = */ 16,
		/* .MaxComputeImageUniforms = */ 8,
		/* .MaxComputeAtomicCounters = */ 8,
		/* .MaxComputeAtomicCounterBuffers = */ 1,
		/* .MaxVaryingComponents = */ 60,
		/* .MaxVertexOutputComponents = */ 64,
		/* .MaxGeometryInputComponents = */ 64,
		/* .MaxGeometryOutputComponents = */ 128,
		/* .MaxFragmentInputComponents = */ 128,
		/* .MaxImageUnits = */ 8,
		/* .MaxCombinedImageUnitsAndFragmentOutputs = */ 8,
		/* .MaxCombinedShaderOutputResources = */ 8,
		/* .MaxImageSamples = */ 0,
		/* .MaxVertexImageUniforms = */ 0,
		/* .MaxTessControlImageUniforms = */ 0,
		/* .MaxTessEvaluationImageUniforms = */ 0,
		/* .MaxGeometryImageUniforms = */ 0,
		/* .MaxFragmentImageUniforms = */ 8,
		/* .MaxCombinedImageUniforms = */ 8,
		/* .MaxGeometryTextureImageUnits = */ 16,
		/* .MaxGeometryOutputVertices = */ 256,
		/* .MaxGeometryTotalOutputComponents = */ 1024,
		/* .MaxGeometryUniformComponents = */ 1024,
		/* .MaxGeometryVaryingComponents = */ 64,
		/* .MaxTessControlInputComponents = */ 128,
		/* .MaxTessControlOutputComponents = */ 128,
		/* .MaxTessControlTextureImageUnits = */ 16,
		/* .MaxTessControlUniformComponents = */ 1024,
		/* .MaxTessControlTotalOutputComponents = */ 4096,
		/* .MaxTessEvaluationInputComponents = */ 128,
		/* .MaxTessEvaluationOutputComponents = */ 128,
		/* .MaxTessEvaluationTextureImageUnits = */ 16,
		/* .MaxTessEvaluationUniformComponents = */ 1024,
		/* .MaxTessPatchComponents = */ 120,
		/* .MaxPatchVertices = */ 32,
		/* .MaxTessGenLevel = */ 64,
		/* .MaxViewports = */ 16,
		/* .MaxVertexAtomicCounters = */ 0,
		/* .MaxTessControlAtomicCounters = */ 0,
		/* .MaxTessEvaluationAtomicCounters = */ 0,
		/* .MaxGeometryAtomicCounters = */ 0,
		/* .MaxFragmentAtomicCounters = */ 8,
		/* .MaxCombinedAtomicCounters = */ 8,
		/* .MaxAtomicCounterBindings = */ 1,
		/* .MaxVertexAtomicCounterBuffers = */ 0,
		/* .MaxTessControlAtomicCounterBuffers = */ 0,
		/* .MaxTessEvaluationAtomicCounterBuffers = */ 0,
		/* .MaxGeometryAtomicCounterBuffers = */ 0,
		/* .MaxFragmentAtomicCounterBuffers = */ 1,
		/* .MaxCombinedAtomicCounterBuffers = */ 1,
		/* .MaxAtomicCounterBufferSize = */ 16384,
		/* .MaxTransformFeedbackBuffers = */ 4,
		/* .MaxTransformFeedbackInterleavedComponents = */ 64,
		/* .MaxCullDistances = */ 8,
		/* .MaxCombinedClipAndCullDistances = */ 8,
		/* .MaxSamples = */ 4,
		/* .limits = */{
		/* .nonInductiveForLoops = */ 1,
		/* .whileLoops = */ 1,
		/* .doWhileLoops = */ 1,
		/* .generalUniformIndexing = */ 1,
		/* .generalAttributeMatrixVectorIndexing = */ 1,
		/* .generalVaryingIndexing = */ 1,
		/* .generalSamplerIndexing = */ 1,
		/* .generalVariableIndexing = */ 1,
		/* .generalConstantMatrixVectorIndexing = */ 1,
	} };
	EShMessages GlslangHelper::DeriveOptions(Source source, Semantics semantics, Target target)
	{
		EShMessages result = EShMsgCascadingErrors;

		switch (source) {
		case Source::GLSL:
			break;
		case Source::HLSL:
			result = static_cast<EShMessages>(result | EShMsgReadHlsl);
			break;
		}

		switch (target) {
		case Target::AST:
			result = static_cast<EShMessages>(result | EShMsgAST);
			break;
		case Target::Spv:
			result = static_cast<EShMessages>(result | EShMsgSpvRules);
			result = static_cast<EShMessages>(result | EShMsgKeepUncalled);
			break;
		case Target::BothASTAndSpv:
			result = static_cast<EShMessages>(result | EShMsgSpvRules | EShMsgAST);
			result = static_cast<EShMessages>(result | EShMsgKeepUncalled);
			break;
		};

		switch (semantics) {
		case Semantics::OpenGL:
			break;
		case Semantics::Vulkan:
			result = static_cast<EShMessages>(result | EShMsgVulkanRules | EShMsgSpvRules);
			break;
		}

		result = static_cast<EShMessages>(result | EShMsgHlslLegalization);

		return result;
	}

	GlslangResult GlslangHelper::compileAndLink(
		EShLanguage stage, const std::string& code,
		const std::string& entryPointName, EShMessages controls,
		glslang::EShTargetClientVersion clientTargetVersion,
		bool flattenUniformArrays,
		EShTextureSamplerTransformMode texSampTransMode,
		bool enableOptimizer,
		bool automap)
	{
		glslang::TShader shader(stage);
		if (automap) {
			shader.setAutoMapLocations(true);
			shader.setAutoMapBindings(true);
		}
		shader.setTextureSamplerTransformMode(texSampTransMode);
		shader.setFlattenUniformArrays(flattenUniformArrays);

		if (controls & EShMsgSpvRules) {
			if (controls & EShMsgVulkanRules) {
				shader.setEnvInput((controls & EShMsgReadHlsl) ? glslang::EShSourceHlsl
					: glslang::EShSourceGlsl,
					stage, glslang::EShClientVulkan, 100);
				shader.setEnvClient(glslang::EShClientVulkan, clientTargetVersion);
				shader.setEnvTarget(glslang::EShTargetSpv,
					clientTargetVersion == glslang::EShTargetVulkan_1_1 ? glslang::EShTargetSpv_1_3
					: glslang::EShTargetSpv_1_0);
			}
			else {
				shader.setEnvInput((controls & EShMsgReadHlsl) ? glslang::EShSourceHlsl
					: glslang::EShSourceGlsl,
					stage, glslang::EShClientOpenGL, 100);
				shader.setEnvClient(glslang::EShClientOpenGL, clientTargetVersion);
				shader.setEnvTarget(glslang::EshTargetSpv, glslang::EShTargetSpv_1_0);
			}
		}

		bool success = compile(&shader, code, entryPointName, controls, &DefaultTBuiltInResource);

		glslang::TProgram program;
		program.addShader(&shader);
		success &= program.link(controls);

		spv::SpvBuildLogger logger;

		if (success && (controls & EShMsgSpvRules)) {
			std::vector<uint32_t> spirv_binary;
			glslang::SpvOptions options;
			options.disableOptimizer = !enableOptimizer;
			glslang::GlslangToSpv(*program.getIntermediate(stage),
				spirv_binary, &logger, &options);

			std::ostringstream disassembly_stream;
			spv::Parameterize();
			spv::Disassemble(disassembly_stream, spirv_binary);
			return { { { stage, shader.getInfoLog(), shader.getInfoDebugLog() }, },
				program.getInfoLog(), program.getInfoDebugLog(),
				logger.getAllMessages(), disassembly_stream.str() };
		}
		else {
			return { { { stage, shader.getInfoLog(), shader.getInfoDebugLog() }, },
				program.getInfoLog(), program.getInfoDebugLog(), "", "" };
		}
	}

	bool GlslangHelper::compile(glslang::TShader* shader, const std::string& code,
		const std::string& entryPointName, EShMessages controls,
		const TBuiltInResource* resources)
	{
		const char* shaderStrings = code.data();
		const int shaderLengths = static_cast<int>(code.size());

		shader->setStringsWithLengths(&shaderStrings, &shaderLengths, 1);
		if (!entryPointName.empty()) shader->setEntryPoint(entryPointName.c_str());
		return shader->parse(
			resources, //(resources ? resources : &glslang::DefaultTBuiltInResource),
			defaultVersion, isForwardCompatible, controls);
	}

	void GlslangHelper::outputResultToStream(std::ostringstream* stream,
		const GlslangResult& result,
		EShMessages controls)
	{
		const auto outputIfNotEmpty = [&stream](const std::string& str) {
			if (!str.empty()) *stream << str << "\n";
		};

		for (const auto& shaderResult : result.shaderResults) {
			*stream << shaderResult.shaderName << "\n";
			outputIfNotEmpty(shaderResult.output);
			outputIfNotEmpty(shaderResult.error);
		}
		outputIfNotEmpty(result.linkingOutput);
		outputIfNotEmpty(result.linkingError);
		*stream << result.spirvWarningsErrors;

		if (controls & EShMsgSpvRules) {
			*stream
				<< (result.spirv.empty()
					? "SPIR-V is not generated for failed compile or link\n"
					: result.spirv);
		}
	}
}