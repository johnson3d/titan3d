#pragma once

#include "ShaderConductor.hpp"

namespace ShaderConductor
{
    enum class SpirvResourceType : uint32_t
    {
        StageInput,
        StageOutput,
        UniformBuffer,
        SampledImage,
        SeparateImage,
        SeparateSampler,
        StorageBuffer,
        StorageImage,
        AtomicCounter,
        AccelerationStructures,
    };
    struct SpirvCallback
    {
        virtual bool Remapping(const char* name, SpirvResourceType type, uint32_t set, uint32_t binding, uint32_t& outSet,
                               uint32_t& outBinding) = 0;
    };

    SC_API std::string ProcessSpirv(const Compiler::ResultDesc& binaryResult, SpirvCallback* callback);
} // namespace ShaderConductor