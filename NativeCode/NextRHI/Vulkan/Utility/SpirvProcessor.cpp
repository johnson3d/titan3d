#include "SpirvProcessor.h"
#include <shaderc/shaderc.hpp>

NS_BEGIN

void FSpirvProcessor::GlslToSpirv(const char* glsl, NxRHI::EShaderType shaderStage, std::vector<unsigned char>& data)
{
    /*shaderc::Compiler shader_compiler;
    shaderc::CompileOptions options;
    options.SetTargetEnvironment(shaderc_target_env_vulkan,
        shaderc_env_version_vulkan_1_2);

    shaderc::SpvCompilationResult result = shader_compiler.CompileGlslToSpv(
        glsl, shaderc_glsl_vertex_shader, nullptr, options);*/
}

NS_END