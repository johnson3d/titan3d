#include "VKShader.h"
#include "VKGpuDevice.h"
#include "VKBuffer.h"
#include "VKGpuState.h"
#include "../NxRHIDefine.h"
#include "../../Bricks/CrossShaderCompiler/IShaderConductor.h"
#include "../../../3rd/native/SpirvCross/spirv_cross_c.h"
//#include <spirv_cross/spirv_cross_c.h>//link problem

#include <spirv-tools/libspirv.h>
#include <spirv-tools/libspirv.hpp>
#include <spirv-tools/optimizer.hpp>
#include <glslang/Public/ShaderLang.h>
#include <glslang/SPIRV/GlslangToSpv.h>
#include <shaderc/shaderc.hpp>

#if defined(HasModule_GpuDump)
#include "../../Bricks/GpuDump/NvAftermath.h"
#endif

#if defined(PLATFORM_WIN)
//__declspec(dllexport) double __imp_exp2(double x) {
//	return std::exp2(x);
//}
	#pragma comment(lib, "SPIRV.lib")
	#pragma comment(lib, "glslang.lib")
	
	#pragma comment(lib, "shaderc.lib")
	#pragma comment(lib, "shaderc_combined.lib")
	#pragma comment(lib, "shaderc_shared.lib")
	#pragma comment(lib, "shaderc_util.lib")

	#pragma comment(lib, "SPIRV-Tools.lib")
	#pragma comment(lib, "SPIRV-Tools-diff.lib")
	#pragma comment(lib, "SPIRV-Tools-link.lib")
	#pragma comment(lib, "SPIRV-Tools-lint.lib")
	#pragma comment(lib, "SPIRV-Tools-opt.lib")
	#pragma comment(lib, "SPIRV-Tools-reduce.lib")
	#pragma comment(lib, "SPIRV-Tools-shared.lib")

	/*#pragma comment(lib, "spirv-cross-c.lib")
	#pragma comment(lib, "spirv-cross-core.lib")
	#pragma comment(lib, "spirv-cross-cpp.lib")
	#pragma comment(lib, "spirv-cross-c-shared.lib")
	#pragma comment(lib, "spirv-cross-glsl.lib")
	#pragma comment(lib, "spirv-cross-hlsl.lib")
	#pragma comment(lib, "spirv-cross-msl.lib")
	#pragma comment(lib, "spirv-cross-reflect.lib")
	#pragma comment(lib, "spirv-cross-util.lib")*/
#endif

#define new VNEW

NS_BEGIN
namespace NxRHI
{
	bool VKShader::CompileShader(FShaderCompiler* compiler, FShaderDesc* desc, const char* shader, const char* entry, EShaderType type, const char* sm, const IShaderDefinitions* defines, EShaderLanguage sl, bool bDebugShader, const char* extHlslVersion, const char* dxcArgs, IBlobObject* output, bool asModule)
	{
		desc->FunctionName = entry;
		return IShaderConductor::GetInstance()->CompileShader(compiler, desc, shader, entry, type, sm, defines, bDebugShader, sl, extHlslVersion, dxcArgs, output, asModule);
	}

	VKShader::VKShader()
	{
		
	}
	VKShader::~VKShader()
	{
		auto device = mDeviceRef.GetPtr();
		if (device == nullptr)
			return;

		if (mShader)
		{
			vkDestroyShaderModule(device->mDevice, mShader, device->GetVkAllocCallBacks());
			mShader = nullptr;
		}
	}
	bool VKShader::Init(VKGpuDevice* device, FShaderDesc* desc)
	{
		Desc = desc;
		if (Desc->SpirV.size() == 0)
			return false;

		//Reflect(desc);
		Reflector = desc->SpirvReflector;

		VkShaderModuleCreateInfo createInfo{};
		createInfo.sType = VK_STRUCTURE_TYPE_SHADER_MODULE_CREATE_INFO;
		createInfo.codeSize = desc->SpirV.size();
		createInfo.pCode = reinterpret_cast<const uint32_t*>(&desc->SpirV[0]);

		if (vkCreateShaderModule(device->mDevice, &createInfo, device->GetVkAllocCallBacks(), &mShader) != VK_SUCCESS)
		{
			return false;
		}

#if defined(HasModule_GpuDump)
		GpuDump::NvAftermath::RegByteCode(desc->DebugName.c_str(), &desc->SpirV[0], (UINT)createInfo.codeSize);
#endif

		return true;
	}
	class VKSpirvOptimizer
	{

	public:
		static spv_result_t modify_binding_callback(
			void* user_data,
			const spv_parsed_instruction_t* inst) {

			// 获取用户数据（包含绑定修改信息）
			uint32_t* bindings = static_cast<uint32_t*>(user_data);
			uint32_t old_binding = bindings[0];
			uint32_t new_binding = bindings[1];

			// 检查是否是 OpDecorate 指令
			if (inst->opcode == SPV_OPERAND_TYPE_DECORATION) {
				
			}

			return SPV_SUCCESS;
		}
		void PostSpirvIR(const std::vector<BYTE>& src, std::vector<BYTE>& target)
		{
			spv_context context = spvContextCreate(SPV_ENV_VULKAN_1_2);

			spv_diagnostic diagnostic = nullptr;
			spv_result_t result = spvBinaryParse(
				context,
				this, // 用户数据
				(UINT*)src.data(),
				src.size()/sizeof(UINT),
				nullptr, // 头部回调
				modify_binding_callback, // 指令回调
				&diagnostic
			);
		}
	};

	class FGLSlangUtility
	{
	public:
		static bool initGlslang() {
			if (!glslang::InitializeProcess()) {
				std::cerr << "Failed to initialize glslang" << std::endl;
				return false;
			}
			return true;
		}
		static bool compileGLSL(const char* shaderCode, const char* entry, EShLanguage stage, std::vector<uint32_t>& spirv) {
			// 1. 创建着色器对象
			glslang::TShader shader(stage);
			shader.setStrings(&shaderCode, 1);
			shader.setEntryPoint(entry);
			shader.setSourceEntryPoint(entry);

			// 2. 设置编译选项
			const TBuiltInResource* resources = nullptr;// GetDefaultResources();
			EShMessages messages = (EShMessages)(EShMsgSpvRules | EShMsgVulkanRules); // 目标规则

			// 3. 预处理 + 解析
			if (!shader.parse(resources, 450, false, messages)) { // 450 = GLSL版本
				std::cerr << "Parse failed:\n" << shader.getInfoLog() << std::endl;
				return false;
			}

			// 4. 链接到程序
			glslang::TProgram program;
			program.addShader(&shader);
			if (!program.link(messages)) {
				std::cerr << "Link failed:\n" << program.getInfoLog() << std::endl;
				return false;
			}

			// 5. 生成 SPIR-V
			glslang::SpvOptions spvOptions;
			glslang::GlslangToSpv(*program.getIntermediate(stage), spirv, &spvOptions);
			return true;
		}
		static void finalizeGlslang() {
			glslang::FinalizeProcess();
		}
		static void CompileShaderToSpirv(
			const char* shaderCode, const char* entry,
			EShLanguage stage,
			std::vector<uint32_t>& spirv)
		{
			if (!initGlslang()) {
				return;
			}
			if (!compileGLSL(shaderCode, entry, stage, spirv)) {
				finalizeGlslang();
				return;
			}
			finalizeGlslang();
		}
	};

	bool VKShader::Reflect(FShaderDesc* desc)
	{
		desc->SpirvReflector = MakeWeakRef(new IShaderReflector());

		auto Reflector = desc->SpirvReflector;

		spvc_context context = NULL;
		spvc_parsed_ir ir = NULL;
		spvc_compiler compiler_glsl = NULL;
		spvc_compiler_options options = NULL;
		spvc_resources resources = NULL;
		const spvc_reflected_resource* list = NULL;
		const char* result = NULL;
		size_t count;
		size_t i;

		const SpvId* spirv = (const SpvId*)& desc->SpirV[0];
		ASSERT(desc->SpirV.size() % sizeof(SpvId) == 0)
		size_t word_count = desc->SpirV.size() / sizeof(SpvId);

		// Create context.
		spvc_context_create(&context);

		// Set debug callback.
		//spvc_context_set_error_callback(context, error_callback, userdata);

		// Parse the SPIR-V.
		spvc_context_parse_spirv(context, spirv, word_count, &ir);

		// Hand it off to a compiler instance and give it ownership of the IR.
		spvc_context_create_compiler(context, SPVC_BACKEND_GLSL, ir, SPVC_CAPTURE_MODE_TAKE_OWNERSHIP, &compiler_glsl);

		spvc_set active_variables = nullptr;
		spvc_compiler_get_active_interface_variables(compiler_glsl, &active_variables);

		const int typeMaxBinding = IShaderConductor::typeMaxBinding;

		// Do some basic reflection.
		spvc_compiler_create_shader_resources_for_active_variables(compiler_glsl, &resources, active_variables);
		spvc_resources_get_resource_list_for_type(resources, SPVC_RESOURCE_TYPE_STAGE_INPUT, &list, &count);
		for (i = 0; i < count; i++)
		{
			auto spv_type = spvc_compiler_get_type_handle(compiler_glsl, list[i].base_type_id);
			auto name = spvc_compiler_get_name(compiler_glsl, list[i].id);
			auto descriptorSet = spvc_compiler_get_decoration(compiler_glsl, list[i].id, SpvDecorationDescriptorSet);
			auto binding = spvc_compiler_get_decoration(compiler_glsl, list[i].id, SpvDecorationLocation);
			auto type = GetStreamTypeByVKBinding(binding);
			if (type != VST_Number)
				desc->InputStreams.push_back(type);

			/*const spvc_reflected_builtin_resource* pInputs;
			size_t inputCount = 0;
			spvc_resources_get_builtin_resource_list_for_type(resources, SPVC_BUILTIN_RESOURCE_TYPE_STAGE_INPUT, &pInputs, &inputCount);
			for (int j = 0; j < inputCount; j++)
			{
				auto type = GetStreamType(pInputs[i].resource.id);
				if (type != VST_Number)
					desc->InputStreams.push_back(type);
				if (spvc_compiler_has_active_builtin(compiler_glsl, pInputs[j].builtin, SpvStorageClass::SpvStorageClassInput))
				{
					int xxx = 0;
				}
				else
				{
					int xxx = 0;
				}
			}*/
		}
		spvc_resources_get_resource_list_for_type(resources, SPVC_RESOURCE_TYPE_UNIFORM_BUFFER, &list, &count);
		ASSERT(count < typeMaxBinding);
		for (i = 0; i < count; i++)
		{
			auto spv_type = spvc_compiler_get_type_handle(compiler_glsl, list[i].base_type_id);
			std::string name = spvc_compiler_get_name(compiler_glsl, list[i].id);
			auto descriptorSet = spvc_compiler_get_decoration(compiler_glsl, list[i].id, SpvDecorationDescriptorSet);
			auto binding = spvc_compiler_get_decoration(compiler_glsl, list[i].id, SpvDecorationBinding);

			//spvc_compiler_set_decoration(compiler_glsl, list[i].id, SpvDecorationBinding, 100);

			size_t sz;
			spvc_compiler_get_declared_struct_size(compiler_glsl, spv_type, &sz);

			auto binder = MakeWeakRef(new FShaderBinder(desc->Type));
			binder->Name = name;
			binder->Space = descriptorSet;
			binder->Size = (UINT)sz;
			binder->Slot = binding;
			binder->Type = EShaderBindType::SBT_CBV;
			
			UINT NumOfMember = spvc_type_get_num_member_types(spv_type);
			for (UINT idx = 0; idx < NumOfMember; ++idx)
			{
				auto v = MakeWeakRef(new FShaderVarDesc());
				auto memberName = spvc_compiler_get_member_name(compiler_glsl, list[i].base_type_id, idx);
				size_t varSize;
				spvc_compiler_get_declared_struct_member_size(compiler_glsl, spv_type, idx, &varSize);
				spvc_compiler_type_struct_member_offset(compiler_glsl, spv_type, idx, (UINT*)&v->Offset);
				v->Name = memberName;
				v->Size = (UINT)varSize;

				auto member_id = spvc_type_get_member_type(spv_type, idx);
				auto member_type = spvc_compiler_get_type_handle(compiler_glsl, member_id);
				auto numOfDim = spvc_type_get_num_array_dimensions(member_type);
				if (numOfDim > 0)
				{
					v->Elements = 0;
					for (UINT j = 0; j < numOfDim; j++)
					{
						v->Elements += spvc_type_get_array_dimension(member_type, j);
					}
				}
				else
				{
					v->Elements = 1;
				}
				
				//UINT varStride = 0;
				//auto ok = spvc_compiler_type_struct_member_array_stride(compiler_glsl, spv_type, idx, &varStride);
				//if (SPVC_SUCCESS == ok)
				//	v->Elements = (USHORT)(varSize / varStride);//spvc_type_get_vector_size
				//else
				//	v->Elements = 1;
				
				auto baseType = spvc_type_get_basetype(member_type);
				auto vectorSize = spvc_type_get_columns(member_type);
				//ASSERT(v->Elements == vectorSize);
				auto cols = spvc_type_get_vector_size(member_type);
				v->Type = EShaderVarType::SVT_Unknown;
				v->Columns = (USHORT)cols;
				switch (baseType)
				{
				case SPVC_BASETYPE_UNKNOWN:
					break;
				case SPVC_BASETYPE_VOID:
					break;
				case SPVC_BASETYPE_BOOLEAN:
					break;
				case SPVC_BASETYPE_INT8:
					break;
				case SPVC_BASETYPE_UINT8:
					break;
				case SPVC_BASETYPE_INT16:
					break;
				case SPVC_BASETYPE_UINT16:
					break;
				case SPVC_BASETYPE_INT32:
					v->Type = EShaderVarType::SVT_Int;
					break;
				case SPVC_BASETYPE_UINT32:
					break;
				case SPVC_BASETYPE_INT64:
					break;
				case SPVC_BASETYPE_UINT64:
					break;
				case SPVC_BASETYPE_ATOMIC_COUNTER:
					break;
				case SPVC_BASETYPE_FP16:
					break;
				case SPVC_BASETYPE_FP32:
					v->Type = EShaderVarType::SVT_Float;
					break;
				case SPVC_BASETYPE_FP64:
					break;
				case SPVC_BASETYPE_STRUCT:
					break;
				case SPVC_BASETYPE_IMAGE:
					v->Type = EShaderVarType::SVT_Texture;
					break;
				case SPVC_BASETYPE_SAMPLED_IMAGE:
					v->Type = EShaderVarType::SVT_Texture;
					break;
				case SPVC_BASETYPE_SAMPLER:
					v->Type = EShaderVarType::SVT_Sampler;
					break;
				case SPVC_BASETYPE_ACCELERATION_STRUCTURE:
					break;
				case SPVC_BASETYPE_INT_MAX:
					break;
				default:
					break;
				}
				binder->Fields.push_back(v);
			}
			Reflector->CBuffers.push_back(binder);
		}

		spvc_resources_get_resource_list_for_type(resources, SPVC_RESOURCE_TYPE_STORAGE_BUFFER, &list, &count);
		ASSERT(count < typeMaxBinding);
		for (i = 0; i < count; i++)
		{//UAV buffer:rwstructuredbuffer
			auto spv_type = spvc_compiler_get_type_handle(compiler_glsl, list[i].base_type_id);
			std::string decl_block_name = spvc_compiler_get_remapped_declared_block_name(compiler_glsl, list[i].id);
			auto name = spvc_compiler_get_name(compiler_glsl, list[i].id);
			auto descriptorSet = spvc_compiler_get_decoration(compiler_glsl, list[i].id, SpvDecorationDescriptorSet);
			auto binding = spvc_compiler_get_decoration(compiler_glsl, list[i].id, SpvDecorationBinding);
			/*const SpvDecoration* decoration[10];
			size_t num = 0;
			spvc_compiler_get_buffer_block_decorations(compiler_glsl, list[i].id, decoration, &num);*/
			auto constant = spvc_compiler_get_decoration(compiler_glsl, list[i].id, SpvDecorationNonWritable);

			auto binder = MakeWeakRef(new FShaderBinder(desc->Type));
			binder->Name = name;
			binder->Space = descriptorSet;
			binder->Size = 0;
			binder->Slot = binding;
			binder->IsStructuredBuffer = TRUE;

			if (decl_block_name.rfind("type.RWStructuredBuffer") == 0 ||
				decl_block_name.rfind("type.RWByteAddressBuffer") == 0)
			{
				binder->Type = EShaderBindType::SBT_UAV;
				Reflector->Uavs.push_back(binder);
			}
			else if (decl_block_name.rfind("type.StructuredBuffer") == 0 || 
				decl_block_name.rfind("type.ByteAddressBuffer") == 0)
			{
				binder->Type = EShaderBindType::SBT_SRV;
				Reflector->Srvs.push_back(binder);
			}
			else
			{
				ASSERT(false);
			}

			UINT NumOfMember = spvc_type_get_num_member_types(spv_type);
			for (UINT idx = 0; idx < NumOfMember; ++idx)
			{
				auto v = MakeWeakRef(new FShaderVarDesc());
				auto memberName = spvc_compiler_get_member_name(compiler_glsl, list[i].base_type_id, idx);
				size_t varSize;
				spvc_compiler_get_declared_struct_member_size(compiler_glsl, spv_type, idx, &varSize);
				spvc_compiler_type_struct_member_offset(compiler_glsl, spv_type, idx, (UINT*)&v->Offset);
				v->Name = memberName;
				v->Size = (UINT)varSize;

				UINT varStride = 0;
				auto ok = spvc_compiler_type_struct_member_array_stride(compiler_glsl, spv_type, idx, &varStride);
				if (SPVC_SUCCESS == ok)
					v->Elements = (USHORT)(varSize / varStride);//spvc_type_get_vector_size
				else
					v->Elements = 1;
				auto member_id = spvc_type_get_member_type(spv_type, idx);
				auto member_type = spvc_compiler_get_type_handle(compiler_glsl, member_id);
				auto baseType = spvc_type_get_basetype(member_type);
				auto vectorSize = spvc_type_get_columns(member_type);
				auto cols = spvc_type_get_vector_size(member_type);
				v->Type = EShaderVarType::SVT_Unknown;
				v->Columns = (USHORT)vectorSize;
				switch (baseType)
				{
				case SPVC_BASETYPE_UNKNOWN:
					break;
				case SPVC_BASETYPE_VOID:
					break;
				case SPVC_BASETYPE_BOOLEAN:
					break;
				case SPVC_BASETYPE_INT8:
					break;
				case SPVC_BASETYPE_UINT8:
					break;
				case SPVC_BASETYPE_INT16:
					break;
				case SPVC_BASETYPE_UINT16:
					break;
				case SPVC_BASETYPE_INT32:
					v->Type = EShaderVarType::SVT_Int;
					break;
				case SPVC_BASETYPE_UINT32:
					break;
				case SPVC_BASETYPE_INT64:
					break;
				case SPVC_BASETYPE_UINT64:
					break;
				case SPVC_BASETYPE_ATOMIC_COUNTER:
					break;
				case SPVC_BASETYPE_FP16:
					break;
				case SPVC_BASETYPE_FP32:
					v->Type = EShaderVarType::SVT_Float;
					break;
				case SPVC_BASETYPE_FP64:
					break;
				case SPVC_BASETYPE_STRUCT:
					break;
				case SPVC_BASETYPE_IMAGE:
					v->Type = EShaderVarType::SVT_Texture;
					break;
				case SPVC_BASETYPE_SAMPLED_IMAGE:
					v->Type = EShaderVarType::SVT_Texture;
					break;
				case SPVC_BASETYPE_SAMPLER:
					v->Type = EShaderVarType::SVT_Sampler;
					break;
				case SPVC_BASETYPE_ACCELERATION_STRUCTURE:
					break;
				case SPVC_BASETYPE_INT_MAX:
					break;
				default:
					break;
				}
				binder->Fields.push_back(v);
			}
		}

		spvc_resources_get_resource_list_for_type(resources, SPVC_RESOURCE_TYPE_STORAGE_IMAGE, &list, &count);
		ASSERT(count < typeMaxBinding);
		for (i = 0; i < count; i++)
		{//UAV texture:rwtexture
			std::string decl_block_name = spvc_compiler_get_remapped_declared_block_name(compiler_glsl, list[i].id);
			std::string name = spvc_compiler_get_name(compiler_glsl, list[i].id);
			auto descriptorSet = spvc_compiler_get_decoration(compiler_glsl, list[i].id, SpvDecorationDescriptorSet);
			auto binding = spvc_compiler_get_decoration(compiler_glsl, list[i].id, SpvDecorationBinding);
			auto constant = spvc_compiler_get_decoration(compiler_glsl, list[i].id, SpvDecorationConstant);
			auto spv_type = spvc_compiler_get_type_handle(compiler_glsl, list[i].base_type_id);
			SpvAccessQualifier access = spvc_type_get_image_access_qualifier(spv_type);

			auto binder = MakeWeakRef(new FShaderBinder(desc->Type));
			
			binder->Name = name;
			binder->Space = descriptorSet;
			binder->Size = 0;
			binder->Slot = binding;
			binder->Type = EShaderBindType::SBT_UAV;
			binder->IsStructuredBuffer = FALSE;

			Reflector->Uavs.push_back(binder);
		}

		spvc_resources_get_resource_list_for_type(resources, SPVC_RESOURCE_TYPE_SAMPLED_IMAGE, &list, &count);
		ASSERT(count < typeMaxBinding);
		for (i = 0; i < count; i++)
		{//For GL:combine sampler&texture
			auto name = spvc_compiler_get_name(compiler_glsl, list[i].id);
			auto descriptorSet = spvc_compiler_get_decoration(compiler_glsl, list[i].id, SpvDecorationDescriptorSet);
			auto binding = spvc_compiler_get_decoration(compiler_glsl, list[i].id, SpvDecorationBinding);

			{
				auto binder = MakeWeakRef(new FShaderBinder(desc->Type));
				binder->Name = name;
				binder->Space = descriptorSet;
				binder->Size = 0;
				binder->Slot = binding;
				binder->Type = EShaderBindType::SBT_SRV;
				binder->IsStructuredBuffer = FALSE;
				Reflector->Srvs.push_back(binder);
			}
			{
				auto binder = MakeWeakRef(new FShaderBinder(desc->Type));
				binder->Name = name;
				binder->Space = descriptorSet;
				binder->Size = 0;
				binder->Slot = binding;
				binder->Type = EShaderBindType::SBT_Sampler;
				Reflector->Samplers.push_back(binder);
			}
		}

		spvc_resources_get_resource_list_for_type(resources, SPVC_RESOURCE_TYPE_SEPARATE_IMAGE, &list, &count);
		ASSERT(count < typeMaxBinding);
		for (i = 0; i < count; i++)
		{//srv:texture 
			std::string name = spvc_compiler_get_name(compiler_glsl, list[i].id);
			auto descriptorSet = spvc_compiler_get_decoration(compiler_glsl, list[i].id, SpvDecorationDescriptorSet);
			auto binding = spvc_compiler_get_decoration(compiler_glsl, list[i].id, SpvDecorationBinding);

			auto binder = MakeWeakRef(new FShaderBinder(desc->Type));
			binder->Name = name;
			binder->Space = descriptorSet;
			binder->Size = 0;
			binder->Slot = binding;
			binder->Type = EShaderBindType::SBT_SRV;
			binder->IsStructuredBuffer = FALSE;
			Reflector->Srvs.push_back(binder);
		}

		spvc_resources_get_resource_list_for_type(resources, SPVC_RESOURCE_TYPE_SEPARATE_SAMPLERS, &list, &count);
		ASSERT(count < typeMaxBinding);
		for (i = 0; i < count; i++)
		{//samplers
			auto name = spvc_compiler_get_name(compiler_glsl, list[i].id);
			auto descriptorSet = spvc_compiler_get_decoration(compiler_glsl, list[i].id, SpvDecorationDescriptorSet);
			auto binding = spvc_compiler_get_decoration(compiler_glsl, list[i].id, SpvDecorationBinding);

			auto binder = MakeWeakRef(new FShaderBinder(desc->Type));
			binder->Name = name;
			binder->Space = descriptorSet;
			binder->Size = 0;
			binder->Slot = binding;
			binder->Type = EShaderBindType::SBT_Sampler;
			binder->IsStructuredBuffer = FALSE;
			Reflector->Samplers.push_back(binder);
		}

		const char* output_str = NULL;
		spvc_compiler_compile(compiler_glsl, &output_str);
		if (output_str)
		{
			auto len = strlen(output_str);
		}

		std::vector<uint32_t> spirvBinary;
		EShLanguage lang;
		switch (desc->Type)
		{
		case EShaderType::SDT_VertexShader:
			lang = EShLanguage::EShLangVertex;
			break;
		case EShaderType::SDT_PixelShader:
			lang = EShLanguage::EShLangFragment;
			break;
		case EShaderType::SDT_AmplificationShader:
			lang = EShLanguage::EShLangTask;
			break;
		case EShaderType::SDT_MeshShader:
			lang = EShLanguage::EShLangMesh;
			break;
		case EShaderType::SDT_ComputeShader:
			lang = EShLanguage::EShLangCompute;
			break;
		case EShaderType::SDT_RayTracing:
		default:
			ASSERT(false);
			break;
		}



		spvc_context_destroy(context);

		return true;
	}
}
NS_END