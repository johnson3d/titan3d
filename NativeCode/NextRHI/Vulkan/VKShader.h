#pragma once
#include "../NxShader.h"
#include "VKPreHead.h"
#include "../../Base/allocator/PagedAllocator.h"

NS_BEGIN

namespace NxRHI
{
	class VKGpuDevice;
	class VKShader;
	
	class VKShader : public IShader
	{
	public:
		static bool CompileShader(FShaderCompiler* compiler, FShaderDesc* desc, const char* shader, const char* entry, EShaderType type, const char* sm, const IShaderDefinitions* defines, EShaderLanguage sl, bool bDebugShader, const char* extHlslVersion, const char* dxcArgs, IBlobObject* output, bool asModule);

		VKShader();
		~VKShader();
		bool Init(VKGpuDevice* device, FShaderDesc* desc);
		static bool Reflect(FShaderDesc* desc);
	public:
		TWeakRefHandle<VKGpuDevice>	mDeviceRef;
		VkShaderModule		mShader{};
		VNameString			mFunctionName;
	};
}

NS_END