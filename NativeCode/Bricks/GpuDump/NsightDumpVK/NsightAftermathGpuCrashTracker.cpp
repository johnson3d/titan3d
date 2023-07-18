//*********************************************************
//
// Copyright (c) 2019-2022, NVIDIA CORPORATION. All rights reserved.
//
//  Permission is hereby granted, free of charge, to any person obtaining a
//  copy of this software and associated documentation files (the "Software"),
//  to deal in the Software without restriction, including without limitation
//  the rights to use, copy, modify, merge, publish, distribute, sublicense,
//  and/or sell copies of the Software, and to permit persons to whom the
//  Software is furnished to do so, subject to the following conditions:
//
//  The above copyright notice and this permission notice shall be included in
//  all copies or substantial portions of the Software.
//
//  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//  IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//  FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.  IN NO EVENT SHALL
//  THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//  LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
//  FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
//  DEALINGS IN THE SOFTWARE.
//
//*********************************************************
#include "../../../Base/IUnknown.h"
#include <fstream>
#include <iomanip>
#include <string>
#include <array>
#define VULKAN_HPP_NO_TO_STRING
#include "NsightAftermathGpuCrashTracker.h"

#include "../NvAftermath.h"
#ifdef HasModule_Dx12
#include "../../NextRHI/Dx12/DX12PreHead.h"
#endif

//*********************************************************
// GpuCrashTracker implementation
//*********************************************************

VKGpuCrashTracker::VKGpuCrashTracker(const MarkerMap& markerMap)
    : m_initialized(false)
    , m_mutex()
    , m_shaderDebugInfo()
    , m_shaderDatabase()
    , m_markerMap(markerMap)
{
}

VKGpuCrashTracker::~VKGpuCrashTracker()
{
    // If initialized, disable GPU crash dumps
    if (m_initialized)
    {
        GFSDK_Aftermath_DisableGpuCrashDumps();
    }
}

// Initialize the GPU Crash Dump Tracker
void VKGpuCrashTracker::Initialize(GFSDK_Aftermath_GpuCrashDumpWatchedApiFlags api)
{
    // Enable GPU crash dumps and set up the callbacks for crash dump notifications,
    // shader debug information notifications, and providing additional crash
    // dump description data.Only the crash dump callback is mandatory. The other two
    // callbacks are optional and can be omitted, by passing nullptr, if the corresponding
    // functionality is not used.
    // The DeferDebugInfoCallbacks flag enables caching of shader debug information data
    // in memory. If the flag is set, ShaderDebugInfoCallback will be called only
    // in the event of a crash, right before GpuCrashDumpCallback. If the flag is not set,
    // ShaderDebugInfoCallback will be called for every shader that is compiled.
    GFSDK_Aftermath_Version ver = GFSDK_Aftermath_Version_API;
    GFSDK_Aftermath_Result _result = GFSDK_Aftermath_EnableGpuCrashDumps(
        ver,
        api,
        GFSDK_Aftermath_GpuCrashDumpFeatureFlags_DeferDebugInfoCallbacks, // Let the Nsight Aftermath library cache shader debug information.
        GpuCrashDumpCallback,                                             // Register callback for GPU crash dumps.
        ShaderDebugInfoCallback,                                          // Register callback for shader debug information.
        CrashDumpDescriptionCallback,                                     // Register callback for GPU crash dump description.
        ResolveMarkerCallback,                                            // Register callback for resolving application-managed markers.
        this);                                                           // Set the GpuCrashTracker object as user data for the above callbacks.
    ASSERT(_result == GFSDK_Aftermath_Result::GFSDK_Aftermath_Result_Success);
    m_initialized = true;
}

// Handler for GPU crash dump callbacks from Nsight Aftermath
void VKGpuCrashTracker::OnCrashDump(const void* pGpuCrashDump, const uint32_t gpuCrashDumpSize)
{
    // Make sure only one thread at a time...
    std::lock_guard<std::mutex> lock(m_mutex);

    GFSDK_Aftermath_PageFaultInformation FaultInformation;
    auto Result = GFSDK_Aftermath_GetPageFaultInformation(&FaultInformation);
    if (Result == GFSDK_Aftermath_Result_Success)
    {
        VFX_LTRACE(ELTT_Graphics, ("NVAftermath Fault address: 0x%016llx\r\n"), FaultInformation.faultingGpuVA);
        VFX_LTRACE(ELTT_Graphics, ("NVAftermath Fault resource dims: %d x %d x %d\r\n"), FaultInformation.resourceDesc.width, FaultInformation.resourceDesc.height, FaultInformation.resourceDesc.depth);
        VFX_LTRACE(ELTT_Graphics, ("NVAftermath Fault result size: %llu bytes\r\n"), FaultInformation.resourceDesc.size);
        VFX_LTRACE(ELTT_Graphics, ("NVAftermath Fault resource mips: %d\r\n"), FaultInformation.resourceDesc.mipLevels);

#if defined(HasModule_Dx12) || defined(HasModule_Dx11)
        if (EngineNS::GpuDump::NvAftermath::GetAfterMathRhiType() == EngineNS::NxRHI::ERhiType::RHI_D3D12||
            EngineNS::GpuDump::NvAftermath::GetAfterMathRhiType() == EngineNS::NxRHI::ERhiType::RHI_D3D11)
        {
            if (EngineNS::GpuDump::NvAftermath::GetAfterMathRhiType() == EngineNS::NxRHI::ERhiType::RHI_D3D12)
            {
				auto pdxResource = (ID3D12Resource*)FaultInformation.resourceDesc.pAppResource;
                /*wchar_t name[128] = {};
				UINT size = sizeof(name);
				pdxResource->GetPrivateData(WKPDID_D3DDebugObjectNameW, &size, name);
                auto nameStr = StringHelper::wstrtostr(name);
                VFX_LTRACE(ELTT_Graphics, ("NVAftermath Fault Name: %s"), nameStr.c_str());*/
                auto pInfo = EngineNS::NxRHI::DX12ResourceDebugMapper::Get()->FindDebugInfo(pdxResource);
                if (pInfo != nullptr)
                {
                    VFX_LTRACE(ELTT_Graphics, ("NVAftermath Fault Name: %s"), pInfo->Name.c_str());
                }
                else
                {
                    VFX_LTRACE(ELTT_Graphics, ("NVAftermath Fault Name: None"));
                }
            }

            auto ResourceFormat = (DXGI_FORMAT)FaultInformation.resourceDesc.format;
            auto fmt = EngineNS::NxRHI::DX12FormatToFormat(ResourceFormat);
            VFX_LTRACE(ELTT_Graphics, ("NVAftermath Fault resource format: %s (0x%x)"), GetPixelFormatString(fmt), (UINT)ResourceFormat);
        }
#elif defined(HasModule_Vulkan)
        if (EngineNS::GpuDump::NvAftermath::GetAfterMathRhiType() == EngineNS::NxRHI::ERhiType::RHI_VK)
        {
            auto ResourceFormat = (VK_FORMAT)FaultInformation.resourceDesc.format;
        }
#endif
    }

    VFX_LTRACE(ELTT_Graphics, "Gpu crash dump:begin\r\n");
    // Write to file for later in-depth analysis with Nsight Graphics.
    WriteGpuCrashDumpToFile(pGpuCrashDump, gpuCrashDumpSize);
    VFX_LTRACE(ELTT_Graphics, "Gpu crash dump:end\r\n");
}

// Handler for shader debug information callbacks
void VKGpuCrashTracker::OnShaderDebugInfo(const void* pShaderDebugInfo, const uint32_t shaderDebugInfoSize)
{
    // Make sure only one thread at a time...
    std::lock_guard<std::mutex> lock(m_mutex);

    // Get shader debug information identifier
    GFSDK_Aftermath_ShaderDebugInfoIdentifier identifier = {};
    AFTERMATH_CHECK_ERROR(GFSDK_Aftermath_GetShaderDebugInfoIdentifier(
        GFSDK_Aftermath_Version_API,
        pShaderDebugInfo,
        shaderDebugInfoSize,
        &identifier));

    // Store information for decoding of GPU crash dumps with shader address mapping
    // from within the application.
    std::vector<uint8_t> data((uint8_t*)pShaderDebugInfo, (uint8_t*)pShaderDebugInfo + shaderDebugInfoSize);
    m_shaderDebugInfo[identifier].swap(data);

    // Write to file for later in-depth analysis of crash dumps with Nsight Graphics
    WriteShaderDebugInformationToFile(identifier, pShaderDebugInfo, shaderDebugInfoSize);
}

// Handler for GPU crash dump description callbacks
void VKGpuCrashTracker::OnDescription(PFN_GFSDK_Aftermath_AddGpuCrashDumpDescription addDescription)
{
    // Add some basic description about the crash. This is called after the GPU crash happens, but before
    // the actual GPU crash dump callback. The provided data is included in the crash dump and can be
    // retrieved using GFSDK_Aftermath_GpuCrashDump_GetDescription().
    addDescription(GFSDK_Aftermath_GpuCrashDumpDescriptionKey_ApplicationName, "Titan3D");
    addDescription(GFSDK_Aftermath_GpuCrashDumpDescriptionKey_ApplicationVersion, "v1.0");
    addDescription(GFSDK_Aftermath_GpuCrashDumpDescriptionKey_UserDefined, "This is a TitanEngine GPU crash dump.");
    addDescription(GFSDK_Aftermath_GpuCrashDumpDescriptionKey_UserDefined + 1, "Engine State: Rendering.");
    addDescription(GFSDK_Aftermath_GpuCrashDumpDescriptionKey_UserDefined + 2, "More user-defined information...");
}

// Handler for app-managed marker resolve callback
void VKGpuCrashTracker::OnResolveMarker(const void* pMarker, void** resolvedMarkerData, uint32_t* markerSize)
{
    // Important: the pointer passed back via resolvedMarkerData must remain valid after this function returns
    // using references for all of the m_markerMap accesses ensures that the pointers refer to the persistent data
    for (auto& map : m_markerMap)
    {
        const auto& foundMarker = map.find((uint64_t)pMarker);
        if (foundMarker != map.end())
        {
            const std::string& markerData = foundMarker->second;
            // std::string::data() will return a valid pointer until the string is next modified
            // we don't modify the string after calling data() here, so the pointer should remain valid
            *resolvedMarkerData = (void*)markerData.data();
            *markerSize = (uint32_t)markerData.length();
            return;
        }
    }
}

// Helper for writing a GPU crash dump to a file
void VKGpuCrashTracker::WriteGpuCrashDumpToFile(const void* pGpuCrashDump, const uint32_t gpuCrashDumpSize)
{
    // Create a GPU crash dump decoder object for the GPU crash dump.
    GFSDK_Aftermath_GpuCrashDump_Decoder decoder = {};
    AFTERMATH_CHECK_ERROR(GFSDK_Aftermath_GpuCrashDump_CreateDecoder(
        GFSDK_Aftermath_Version_API,
        pGpuCrashDump,
        gpuCrashDumpSize,
        &decoder));

    // Use the decoder object to read basic information, like application
    // name, PID, etc. from the GPU crash dump.
    GFSDK_Aftermath_GpuCrashDump_BaseInfo baseInfo = {};
    AFTERMATH_CHECK_ERROR(GFSDK_Aftermath_GpuCrashDump_GetBaseInfo(decoder, &baseInfo));

    // Use the decoder object to query the application name that was set
    // in the GPU crash dump description.
    uint32_t applicationNameLength = 0;
    AFTERMATH_CHECK_ERROR(GFSDK_Aftermath_GpuCrashDump_GetDescriptionSize(
        decoder,
        GFSDK_Aftermath_GpuCrashDumpDescriptionKey_ApplicationName,
        &applicationNameLength));

    std::vector<char> applicationName(applicationNameLength, '\0');

    AFTERMATH_CHECK_ERROR(GFSDK_Aftermath_GpuCrashDump_GetDescription(
        decoder,
        GFSDK_Aftermath_GpuCrashDumpDescriptionKey_ApplicationName,
        uint32_t(applicationName.size()),
        applicationName.data()));

    // Create a unique file name for writing the crash dump data to a file.
    // Note: due to an Nsight Aftermath bug (will be fixed in an upcoming
    // driver release) we may see redundant crash dumps. As a workaround,
    // attach a unique count to each generated file name.
    static int count = 0;
    const std::string baseFileName =
        std::string(applicationName.data())
        + "-"
        + std::to_string(baseInfo.pid)
        + "-"
        + std::to_string(++count);

    // Write the crash dump data to a file using the .nv-gpudmp extension
    // registered with Nsight Graphics.
    const std::string crashDumpFileName = baseFileName + ".nv-gpudmp";
    std::ofstream dumpFile(crashDumpFileName, std::ios::out | std::ios::binary);
    if (dumpFile)
    {
        dumpFile.write((const char*)pGpuCrashDump, gpuCrashDumpSize);
        dumpFile.close();
    }

    // Decode the crash dump to a JSON string.
    // Step 1: Generate the JSON and get the size.
    uint32_t jsonSize = 0;
    AFTERMATH_CHECK_ERROR(GFSDK_Aftermath_GpuCrashDump_GenerateJSON(
        decoder,
        GFSDK_Aftermath_GpuCrashDumpDecoderFlags_ALL_INFO,
        GFSDK_Aftermath_GpuCrashDumpFormatterFlags_NONE,
        ShaderDebugInfoLookupCallback,
        ShaderLookupCallback,
        ShaderSourceDebugInfoLookupCallback,
        this,
        &jsonSize));
    // Step 2: Allocate a buffer and fetch the generated JSON.
    std::vector<char> json(jsonSize);
    AFTERMATH_CHECK_ERROR(GFSDK_Aftermath_GpuCrashDump_GetJSON(
        decoder,
        uint32_t(json.size()),
        json.data()));

    // Write the crash dump data as JSON to a file.
    const std::string jsonFileName = crashDumpFileName + ".json";
    std::ofstream jsonFile(jsonFileName, std::ios::out | std::ios::binary);
    if (jsonFile)
    {
       // Write the JSON to the file (excluding string termination)
       jsonFile.write(json.data(), json.size() - 1);
       jsonFile.close();
    }

    // Destroy the GPU crash dump decoder object.
    AFTERMATH_CHECK_ERROR(GFSDK_Aftermath_GpuCrashDump_DestroyDecoder(decoder));
}

// Helper for writing shader debug information to a file
void VKGpuCrashTracker::WriteShaderDebugInformationToFile(
    GFSDK_Aftermath_ShaderDebugInfoIdentifier identifier,
    const void* pShaderDebugInfo,
    const uint32_t shaderDebugInfoSize)
{
    // Create a unique file name.
    const std::string filePath = "shader-" + std::to_string(identifier) + ".nvdbg";

    std::ofstream f(filePath, std::ios::out | std::ios::binary);
    if (f)
    {
        f.write((const char*)pShaderDebugInfo, shaderDebugInfoSize);
    }
}

// Handler for shader debug information lookup callbacks.
// This is used by the JSON decoder for mapping shader instruction
// addresses to SPIR-V IL lines or GLSL source lines.
void VKGpuCrashTracker::OnShaderDebugInfoLookup(
    const GFSDK_Aftermath_ShaderDebugInfoIdentifier& identifier,
    PFN_GFSDK_Aftermath_SetData setShaderDebugInfo) const
{
    // Search the list of shader debug information blobs received earlier.
    auto i_debugInfo = m_shaderDebugInfo.find(identifier);
    if (i_debugInfo == m_shaderDebugInfo.end())
    {
        // Early exit, nothing found. No need to call setShaderDebugInfo.
        return;
    }

    // Let the GPU crash dump decoder know about the shader debug information
    // that was found.
    setShaderDebugInfo(i_debugInfo->second.data(), uint32_t(i_debugInfo->second.size()));
}

// Handler for shader lookup callbacks.
// This is used by the JSON decoder for mapping shader instruction
// addresses to SPIR-V IL lines or GLSL source lines.
// NOTE: If the application loads stripped shader binaries (ie; --strip-all in spirv-remap),
// Aftermath will require access to both the stripped and the not stripped
// shader binaries.
void VKGpuCrashTracker::OnShaderLookup(
    const GFSDK_Aftermath_ShaderBinaryHash& shaderHash,
    PFN_GFSDK_Aftermath_SetData setShaderBinary) const
{
    // Find shader binary data for the shader hash in the shader database.
    std::vector<uint8_t> shaderBinary;
    std::string debugName;
    if (!m_shaderDatabase.FindShaderBinary(shaderHash, shaderBinary, debugName))
    {
        // Early exit, nothing found. No need to call setShaderBinary.
        return;
    }
    VFX_LTRACE(ELTT_Graphics, "Shader(%s) in crash dump\r\n", debugName.c_str());
    // Let the GPU crash dump decoder know about the shader data
    // that was found.
    setShaderBinary(shaderBinary.data(), uint32_t(shaderBinary.size()));
}

// Handler for shader source debug info lookup callbacks.
// This is used by the JSON decoder for mapping shader instruction addresses to
// GLSL source lines, if the shaders used by the application were compiled with
// separate debug info data files.
void VKGpuCrashTracker::OnShaderSourceDebugInfoLookup(
    const GFSDK_Aftermath_ShaderDebugName& shaderDebugName,
    PFN_GFSDK_Aftermath_SetData setShaderBinary) const
{
    // Find source debug info for the shader DebugName in the shader database.
    std::vector<uint8_t> shaderBinary;
    if (!m_shaderDatabase.FindShaderBinaryWithDebugData(shaderDebugName, shaderBinary))
    {
        // Early exit, nothing found. No need to call setShaderBinary.
        return;
    }

    // Let the GPU crash dump decoder know about the shader debug data that was
    // found.
    setShaderBinary(shaderBinary.data(), uint32_t(shaderBinary.size()));
}

// Static callback wrapper for OnCrashDump
void VKGpuCrashTracker::GpuCrashDumpCallback(
    const void* pGpuCrashDump,
    const uint32_t gpuCrashDumpSize,
    void* pUserData)
{
    VKGpuCrashTracker* pGpuCrashTracker = reinterpret_cast<VKGpuCrashTracker*>(pUserData);
    pGpuCrashTracker->OnCrashDump(pGpuCrashDump, gpuCrashDumpSize);
}

// Static callback wrapper for OnShaderDebugInfo
void VKGpuCrashTracker::ShaderDebugInfoCallback(
    const void* pShaderDebugInfo,
    const uint32_t shaderDebugInfoSize,
    void* pUserData)
{
    VKGpuCrashTracker* pGpuCrashTracker = reinterpret_cast<VKGpuCrashTracker*>(pUserData);
    pGpuCrashTracker->OnShaderDebugInfo(pShaderDebugInfo, shaderDebugInfoSize);
}

// Static callback wrapper for OnDescription
void VKGpuCrashTracker::CrashDumpDescriptionCallback(
    PFN_GFSDK_Aftermath_AddGpuCrashDumpDescription addDescription,
    void* pUserData)
{
    VKGpuCrashTracker* pGpuCrashTracker = reinterpret_cast<VKGpuCrashTracker*>(pUserData);
    pGpuCrashTracker->OnDescription(addDescription);
}

// Static callback wrapper for OnResolveMarker
void VKGpuCrashTracker::ResolveMarkerCallback(
    const void* pMarker,
    void* pUserData,
    void** resolvedMarkerData,
    uint32_t* markerSize)
{
    VKGpuCrashTracker* pGpuCrashTracker = reinterpret_cast<VKGpuCrashTracker*>(pUserData);
    pGpuCrashTracker->OnResolveMarker(pMarker, resolvedMarkerData, markerSize);
}

// Static callback wrapper for OnShaderDebugInfoLookup
void VKGpuCrashTracker::ShaderDebugInfoLookupCallback(
    const GFSDK_Aftermath_ShaderDebugInfoIdentifier* pIdentifier,
    PFN_GFSDK_Aftermath_SetData setShaderDebugInfo,
    void* pUserData)
{
    VKGpuCrashTracker* pGpuCrashTracker = reinterpret_cast<VKGpuCrashTracker*>(pUserData);
    pGpuCrashTracker->OnShaderDebugInfoLookup(*pIdentifier, setShaderDebugInfo);
}

// Static callback wrapper for OnShaderLookup
void VKGpuCrashTracker::ShaderLookupCallback(
    const GFSDK_Aftermath_ShaderBinaryHash* pShaderHash,
    PFN_GFSDK_Aftermath_SetData setShaderBinary,
    void* pUserData)
{
    VKGpuCrashTracker* pGpuCrashTracker = reinterpret_cast<VKGpuCrashTracker*>(pUserData);
    pGpuCrashTracker->OnShaderLookup(*pShaderHash, setShaderBinary);
}

// Static callback wrapper for OnShaderSourceDebugInfoLookup
void VKGpuCrashTracker::ShaderSourceDebugInfoLookupCallback(
    const GFSDK_Aftermath_ShaderDebugName* pShaderDebugName,
    PFN_GFSDK_Aftermath_SetData setShaderBinary,
    void* pUserData)
{
    VKGpuCrashTracker* pGpuCrashTracker = reinterpret_cast<VKGpuCrashTracker*>(pUserData);
    pGpuCrashTracker->OnShaderSourceDebugInfoLookup(*pShaderDebugName, setShaderBinary);
}
