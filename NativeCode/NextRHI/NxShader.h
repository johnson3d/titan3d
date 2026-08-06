#pragma once
#include "NxGpuDevice.h"
#include "NxRHIDefine.h"
#include "../Base/xnd/vfxxnd.h"

NS_BEGIN

struct IBlobObject;

namespace NxRHI
{
	enum TR_ENUM(SV_EnumNoFlags)
		EShaderBindType
	{
		SBT_CBV,
			SBT_SRV,
			SBT_UAV,
			SBT_Sampler,
			SBT_VBV,
			SBT_IBV,
	};

	//what kind of resource a SBT_SRV/SBT_UAV binder binds, each RHI backend maps it to its own descriptor type
	//SBRT_Image(0)/SBRT_Buffer(1) keep the binary compatibility with the legacy IsStructuredBuffer(vBOOL) in cooked shaders
	enum TR_ENUM(SV_EnumNoFlags)
		EShaderBindResourceType
	{
		SBRT_Image = 0,
			SBRT_Buffer = 1,
			SBRT_AccelerationStructure = 2,
	};

	enum TR_ENUM()
		EShaderType
	{
		SDT_Unknown = 0,
			SDT_VertexShader = 1,
			SDT_PixelShader = (1 << 1),
			SDT_ComputeShader = (1 << 2),
			SDT_AmplificationShader = (1 << 3),
			SDT_MeshShader = (1 << 4),
			SDT_RayTracing = (1 << 5),

			SDT_AllStages = SDT_VertexShader | SDT_PixelShader | SDT_ComputeShader | SDT_AmplificationShader | SDT_MeshShader | SDT_RayTracing,
	};
	
	struct TR_CLASS()
		FShaderVarDesc : public VIUnknown
	{
		ENGINE_RTTI(FShaderVarDesc);
		int GetShaderVarTypeSize(EShaderVarType type)
		{
			switch (type)
			{
			case EShaderVarType::SVT_Float:
				return 4;
			case EShaderVarType::SVT_Int:
				return 4;
			default:
				return -1;
			}
		}
		VNameString			Name;
		EShaderVarType		Type;
		USHORT				Columns;
		USHORT				Elements;
		int					Offset;
		UINT				Size;
	};
	struct TR_CLASS()
		FShaderBinder : public VIUnknown
	{
		ENGINE_RTTI(FShaderBinder);

		FShaderBinder()
		{
			ASSERT(false);
			Space = 0;
			Slot = -1;
			BindCount = 1;
			Size = 0;
			DescriptorIndex = -1;
			ResourceType = EShaderBindResourceType::SBRT_Image;
			ShaderStage = EShaderType::SDT_Unknown;
		}
		FShaderBinder(EShaderType stage)
		{
			Space = 0;
			Slot = -1;
			BindCount = 1;
			Size = 0;
			DescriptorIndex = -1;
			ResourceType = EShaderBindResourceType::SBRT_Image;
			ShaderStage = stage;
		}
		EShaderType ShaderStage = EShaderType::SDT_Unknown;
		VNameString			Name;
		EShaderBindType		Type;
		int					Space = 0;
		int					Slot = 0;
		int					BindCount = 1;
		UINT				Size = 0;
		EShaderBindResourceType	ResourceType = EShaderBindResourceType::SBRT_Image;
		int					DescriptorIndex = -1;
		bool IsBindless() const { 
			return BindCount == 0;
		}
		bool IsStructuredBuffer() const {
			return ResourceType == EShaderBindResourceType::SBRT_Buffer;
		}
		std::vector<AutoRef<FShaderVarDesc>>		Fields;
		const FShaderVarDesc* FindField(const char* name) const;

		void SaveXnd(XndAttribute* pAttr);
		bool LoadXnd(IGpuDevice* device, XndAttribute* pAttr);
	};
	class TR_CLASS()
		IShaderReflector : public IWeakRefObject
	{
	public:
		ENGINE_RTTI(IShaderReflector);
		const FShaderBinder* FindBinder(EShaderBindType type, const char* name) const;
		const FShaderBinder* FindBinder(EShaderBindType type, VNameString name) const;
		const FShaderBinder* FindBinder(const char* name) const
		{
			auto result = FindBinder(EShaderBindType::SBT_CBV, name);
			if (result)
				return result;
			result = FindBinder(EShaderBindType::SBT_SRV, name);
			if (result)
				return result;
			result = FindBinder(EShaderBindType::SBT_UAV, name);
			if (result)
				return result;
			result = FindBinder(EShaderBindType::SBT_Sampler, name);
			if (result)
				return result;

			return nullptr;
		}

		std::vector<AutoRef<const FShaderBinder>>		CBuffers;
		std::vector<AutoRef<const FShaderBinder>>		Uavs;
		std::vector<AutoRef<const FShaderBinder>>		Srvs;
		std::vector<AutoRef<const FShaderBinder>>		Samplers;
		void SaveXnd(XndAttribute* pAttr);
		bool LoadXnd(IGpuDevice * device, XndAttribute * pAttr, EShaderType stage);
	};
	class TR_CLASS()
		IShader : public IWeakRefObject
	{
	public:
		ENGINE_RTTI(IShader);
		AutoRef<FShaderDesc>		Desc;
		AutoRef<IShaderReflector>	Reflector;

		FShaderDesc* GetDesc() {
			return Desc;
		}
		IShaderReflector* GetReflector() {
			return Reflector;
		}
	};
	enum TR_ENUM() 
		EShaderLanguage
	{
		SL_DXBC = 1,
		SL_DXIL = (1 << 1),
		SL_GLSL = (1 << 2),
		SL_SPIRV = (1 << 3),
		SL_METAL = (1 << 4)
	};
	struct TR_CLASS()
		FShaderDesc : public IWeakRefObject
	{
		ENGINE_RTTI(FShaderDesc);
		FShaderDesc()
		{
			
		}
		EShaderType		Type = EShaderType::SDT_Unknown;
		EShaderLanguage	Language = EShaderLanguage::SL_DXBC;
		VNameString		DebugName;
		VNameString		FunctionName;
		std::vector<EVertexStreamType>	InputStreams;
		std::string		HLSL;
		std::vector<BYTE>	RhiData;

		AutoRef<IShaderReflector>	Reflector;
		AutoRef<IShader>	Shader;

		inline BYTE* GetRHIData() {
			if (RhiData.size() == 0)
				return nullptr;
			return &RhiData[0];
		}
		inline UINT GetRHIDataSize() {
			return (UINT)RhiData.size();
		}
		void SetRhiData(const BYTE* pData, UINT size)
		{
			RhiData.resize(size);
			if (size > 0)
				memcpy(&RhiData[0], pData, size);
		}
		const BYTE* GetRhiData() const {
			if (RhiData.empty())
				return nullptr;
			return &RhiData[0];
		}
		UINT GetRhiDataSize() const {
			return (UINT)RhiData.size();
		}
		const char* GetRhiDataAsText() const {
			if (RhiData.empty())
				return "";
			return (const char*)&RhiData[0];
		}
		void SetRhiDataFromText(const char* code) {
			size_t len = strlen(code);
			RhiData.resize(len);
			if (len > 0)
				memcpy(&RhiData[0], code, len);
		}

		void SaveXnd(XndNode* node);
		bool LoadXnd(IGpuDevice* device, XndNode* node);

		const AutoRef<IShader>& GetOrCreateShader(IGpuDevice* pDevice);
	};
	struct FMacroDefine
	{
		FMacroDefine(const char* name, const char* definition)
		{
			Name = name;
			Definition = definition;
		}
		VNameString Name;
		VNameString Definition;
		const char* GetName() const {
			return Name.c_str();
		}
		const char* GetDefinition() const {
			return Definition.c_str();
		}
	};
	class TR_CLASS()
		IShaderDefinitions : public IWeakRefObject
	{
	public:
		ENGINE_RTTI(IShaderDefinitions);
		IShaderDefinitions() 
		{

		}
		std::vector<FMacroDefine>	Definitions;
		
		std::string ToDefineString() const
		{
			std::string					DefineCode;
			for (const auto& i : Definitions)
			{
				DefineCode = DefineCode + i.Name.c_str() + "=" + i.Definition.c_str();
			}
			return DefineCode.c_str();
		}
		UINT GetDefineCount() {
			return (UINT)Definitions.size();
		}
		const char* GetName(UINT index)
		{
			return Definitions[index].Name.c_str();
		}
		const char* GetValue(UINT index)
		{
			return Definitions[index].Definition.c_str();
		}
		void AddDefine(const char* name, const char* value);
		const FMacroDefine* FindDefine(const char* name) const;
		void ClearDefines();
		void RemoveDefine(const char* name);
		void MergeDefinitions(IShaderDefinitions* def);
	};
	struct TR_CLASS()
		FShaderCode : public IWeakRefObject
	{
		ENGINE_RTTI(FShaderCode);
		FShaderCode()
		{

		}
		std::string Name;
		std::string SourceCode;
		UINT GetSize()
		{
			return (UINT)SourceCode.size();
		}
		void SetSourceCode(const char* code) {
			SourceCode = code;
		}
		const char* GetSourceCode() const;
	};
	TR_CALLBACK(SV_CallConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, SV_NameSpace = EngineNS)
	typedef NxRHI::FShaderCode* (*FnGetShaderCodeStream)(TR_META(SV_NoStringConverter) const char* name, TR_META(SV_NoStringConverter) const char* oriName);
	class TR_CLASS()
		FShaderCompiler : public IWeakRefObject
	{
		typedef NxRHI::FShaderCode* (*FnGetShaderCodeStream)(TR_META(SV_NoStringConverter) const char* name, TR_META(SV_NoStringConverter) const char* oriName);
		FnGetShaderCodeStream			GetShaderCodeStreamPtr;
	public:
		ENGINE_RTTI(FShaderCompiler);
		FShaderCompiler()
		{	
			GetShaderCodeStreamPtr = nullptr;
		}
		void SetCallback(FnGetShaderCodeStream fn)
		{
			GetShaderCodeStreamPtr = fn;
		}
		FShaderCode* GetShaderCodeStream(const char* name, const char* oriName);
		bool CompileShader(FShaderDesc* desc, const char* shader, const char* entry, EShaderType type, const char* sm, const IShaderDefinitions* defines, EShaderLanguage sl, bool bDebugShader, const char* extHlslVersion, const char* dxcArgs, IBlobObject* output, bool asModule);
	};
}

NS_END