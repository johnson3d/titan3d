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
#define VULKAN_HPP_NO_TO_STRING
#include "NsightAftermathShaderDatabase.h"

bool VKShaderDatabase::IsSpirV = false;
//*********************************************************
// ShaderDatabase implementation
//*********************************************************

VKShaderDatabase::VKShaderDatabase()
    : m_shaderBinaries()
    , m_shaderBinariesWithDebugInfo()
{
    //// Add shader binaries to database
    //AddShaderBinary("cube.vert.spirv");
    //AddShaderBinary("cube.frag.spirv");

    //// Add the not stripped shader binaries to the database, too.
    //AddShaderBinaryWithDebugInfo("cube.vert.spirv", "cube.vert.full.spirv");
    //AddShaderBinaryWithDebugInfo("cube.frag.spirv", "cube.frag.full.spirv");
}


void VKShaderDatabase::AddShaderBinary(const char* name, std::vector<uint8_t>& data)
{
    const GFSDK_Aftermath_SpirvCode shader{ data.data(), uint32_t(data.size()) };
	GFSDK_Aftermath_ShaderBinaryHash shaderHash;
    if (IsSpirV)
    {
		AFTERMATH_CHECK_ERROR(GFSDK_Aftermath_GetShaderHashSpirv(
			GFSDK_Aftermath_Version_API,
			&shader,
			&shaderHash));
    }
    else
    {
        const D3D12_SHADER_BYTECODE shader{ data.data(), data.size() };
        GFSDK_Aftermath_Result _result = GFSDK_Aftermath_GetShaderHash(
			GFSDK_Aftermath_Version_API,
			&shader,
			&shaderHash);
        ASSERT(_result == GFSDK_Aftermath_Result::GFSDK_Aftermath_Result_Success);
    }
	// Store the data for shader mapping when decoding GPU crash dumps.
	// cf. FindShaderBinary()
    FShaderCodeInfo tmp;
    tmp.ByteCode = data;
    tmp.DebugName = name;
	m_shaderBinaries[shaderHash] = tmp;
}

void VKShaderDatabase::AddShaderBinaryWithDebugInfo(const char* name, std::vector<uint8_t>& strippedData, std::vector<uint8_t>& data)
{
    GFSDK_Aftermath_ShaderDebugName debugName;
    if (IsSpirV)
    {
		const GFSDK_Aftermath_SpirvCode shader{ data.data(), uint32_t(data.size()) };
		const GFSDK_Aftermath_SpirvCode strippedShader{ strippedData.data(), uint32_t(strippedData.size()) };
		AFTERMATH_CHECK_ERROR(GFSDK_Aftermath_GetShaderDebugNameSpirv(
			GFSDK_Aftermath_Version_API,
			&shader,
			&strippedShader,
			&debugName));
		// Store the data for shader instruction address mapping when decoding GPU crash dumps.
	    // cf. FindShaderBinaryWithDebugData()
        m_shaderBinariesWithDebugInfo[debugName] = data;
    }
    else
    {
        const D3D12_SHADER_BYTECODE shader{ data.data(), uint32_t(data.size()) };
        GFSDK_Aftermath_ShaderDebugName debugName{};
        GFSDK_Aftermath_Result _result = GFSDK_Aftermath_GetShaderDebugName(
            GFSDK_Aftermath_Version_API,
            &shader,
            &debugName);
        
		// Store the data for shader instruction address mapping when decoding GPU crash dumps.
		// cf. FindShaderBinaryWithDebugData()
        if (_result != GFSDK_Aftermath_Result::GFSDK_Aftermath_Result_Success)
        {
            strncpy_s(debugName.name, name, sizeof(debugName.name) - 1);
            m_shaderBinariesWithDebugInfo[debugName] = data;
        }
        else
        {
            m_shaderBinariesWithDebugInfo[debugName] = data;
        }
    }
}

VKShaderDatabase::~VKShaderDatabase()
{
}

bool VKShaderDatabase::ReadFile(const char* filename, std::vector<uint8_t>& data)
{
    std::ifstream fs(filename, std::ios::in | std::ios::binary);
    if (!fs)
    {
        return false;
    }

    fs.seekg(0, std::ios::end);
    data.resize(fs.tellg());
    fs.seekg(0, std::ios::beg);
    fs.read(reinterpret_cast<char*>(data.data()), data.size());
    fs.close();

    return true;
}

void VKShaderDatabase::AddShaderBinary(const char* shaderFilePath)
{
    // Read the shader binary code from the file
    std::vector<uint8_t> data;
    if (!ReadFile(shaderFilePath, data))
    {
        return;
    }

   // Create shader hash for the shader
    const GFSDK_Aftermath_SpirvCode shader{ data.data(), uint32_t(data.size()) };
    GFSDK_Aftermath_ShaderBinaryHash shaderHash;
    AFTERMATH_CHECK_ERROR(GFSDK_Aftermath_GetShaderHashSpirv(
        GFSDK_Aftermath_Version_API,
        &shader,
        &shaderHash));

    // Store the data for shader mapping when decoding GPU crash dumps.
    // cf. FindShaderBinary()
    FShaderCodeInfo tmp;
    tmp.ByteCode = data;
    m_shaderBinaries[shaderHash] = tmp;
}

void VKShaderDatabase::AddShaderBinaryWithDebugInfo(const char* strippedShaderFilePath, const char* shaderFilePath)
{
    // Read the shader debug data from the file
    std::vector<uint8_t> data;
    if (!ReadFile(shaderFilePath, data))
    {
        return;
    }
    std::vector<uint8_t> strippedData;
    if (!ReadFile(strippedShaderFilePath, strippedData))
    {
        return;
    }

    // Generate shader debug name.
    GFSDK_Aftermath_ShaderDebugName debugName;
    const GFSDK_Aftermath_SpirvCode shader{ data.data(), uint32_t(data.size()) };
    const GFSDK_Aftermath_SpirvCode strippedShader{ strippedData.data(), uint32_t(strippedData.size()) };
    AFTERMATH_CHECK_ERROR(GFSDK_Aftermath_GetShaderDebugNameSpirv(
        GFSDK_Aftermath_Version_API,
        &shader,
        &strippedShader,
        &debugName));

    // Store the data for shader instruction address mapping when decoding GPU crash dumps.
    // cf. FindShaderBinaryWithDebugData()
    m_shaderBinariesWithDebugInfo[debugName].swap(data);
}

std::string TestCurrentShader;
// Find a shader binary by shader hash.
bool VKShaderDatabase::FindShaderBinary(const GFSDK_Aftermath_ShaderBinaryHash& shaderHash, std::vector<uint8_t>& shader, std::string& debugName) const
{
    // Find shader binary data for the shader hash
    auto i_shader = m_shaderBinaries.find(shaderHash);
    if (i_shader == m_shaderBinaries.end())
    {
        // Nothing found.   
        return false;
    }

    shader = i_shader->second.ByteCode;
    debugName = i_shader->second.DebugName;
    TestCurrentShader = i_shader->second.DebugName;
    return true;
}

// Find a shader binary with debug information by shader debug name.
bool VKShaderDatabase::FindShaderBinaryWithDebugData(const GFSDK_Aftermath_ShaderDebugName& shaderDebugName, std::vector<uint8_t>& shader) const
{
    // Find shader binary for the shader debug name.
    auto i_shader = m_shaderBinariesWithDebugInfo.find(shaderDebugName);
    if (i_shader == m_shaderBinariesWithDebugInfo.end())
    {
        // Nothing found.
        GFSDK_Aftermath_ShaderDebugName debugName{};
        strncpy_s(debugName.name, TestCurrentShader.c_str(), sizeof(debugName.name) - 1);
        i_shader = m_shaderBinariesWithDebugInfo.find(debugName);
        return false;
    }

    shader = i_shader->second;
    return true;
}
