#pragma once
#include "NxGpuDevice.h"
#include "NxRHIDefine.h"
#include "../Base/xnd/vfxxnd.h"

NS_BEGIN

namespace NxRHI
{
	enum TR_ENUM(SV_EnumNoFlags)
		EShaderBindType
	{
		SBT_CBuffer,
			SBT_SRV,
			SBT_UAV,
			SBT_Sampler,
			SBT_Vertex,
			SBT_Index,
	};
	enum TR_ENUM(SV_EnumNoFlags)
		EShaderVarType
	{
		SVT_Float,
			SVT_Int,
			SVT_Texture,
			SVT_Sampler,
			SVT_Struct,
			SVT_Unknown,
	};
	struct TR_CLASS()
		FShaderVarDesc : public VIUnknownBase
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
		FShaderBinder : public VIUnknownBase
	{
		ENGINE_RTTI(FShaderBinder);

		FShaderBinder()
		{
			Space = 0;
			Slot = -1;
			Size = 0;
			DescriptorIndex = -1;
			IsStructuredBuffer = FALSE;
		}
		VNameString			Name;
		EShaderBindType		Type;
		int					Space = 0;
		int					Slot = 0;
		UINT				Size = 0;
		vBOOL				IsStructuredBuffer = FALSE;
		int					DescriptorIndex = -1;
		std::vector<AutoRef<FShaderVarDesc>>		Fields;
		const FShaderVarDesc* FindField(const char* name) const;

		void SaveXnd(XndAttribute* pAttr);
		bool LoadXnd(IGpuDevice* device, XndAttribute* pAttr);
	};
	class TR_CLASS()
		IShaderReflector : public VIUnknown
	{
	public:
		ENGINE_RTTI(IShaderReflector);
		const FShaderBinder* FindBinder(EShaderBindType type, const char* name) const;
		const FShaderBinder* FindBinder(EShaderBindType type, VNameString name) const;

		std::vector<AutoRef<FShaderBinder>>		CBuffers;
		std::vector<AutoRef<FShaderBinder>>		Uavs;
		std::vector<AutoRef<FShaderBinder>>		Srvs;
		std::vector<AutoRef<FShaderBinder>>		Samplers;
		void SaveXnd(XndAttribute* pAttr);
		bool LoadXnd(IGpuDevice * device, XndAttribute * pAttr);
	};
	enum TR_ENUM()
		EShaderType
	{
		SDT_Unknown = 0,
			SDT_VertexShader = 1,
			SDT_PixelShader = (1 << 1),
			SDT_ComputeShader = (1 << 2),

			SDT_AllStages = SDT_VertexShader | SDT_PixelShader | SDT_ComputeShader,
	};
	class TR_CLASS()
		IShader : public VIUnknown
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
	struct TR_CLASS()
		FShaderDesc : public VIUnknown
	{
		ENGINE_RTTI(FShaderDesc);
		FShaderDesc()
		{
			
		}
		EShaderType		Type;
		VNameString		FunctionName;
		std::vector<EVertexStreamType>	InputStreams;
		std::string		HLSL;
		std::vector<BYTE>	Dxbc;
		std::vector<BYTE>	DxIL;
		std::vector<BYTE>	SpirV;
		std::string		Es300Code;
		std::string		MetalCode;

		AutoRef<IShaderReflector>	DxbcReflector;
		AutoRef<IShaderReflector>	DxILReflector;
		AutoRef<IShaderReflector>	SpirvReflector;
		AutoRef<IShader>	Shader;
		const char* GetGLCode() const {
			return Es300Code.c_str();
		}
		void SetGLCode(const char* code) {
			Es300Code = code;
		}
		const char* GetMetalCode() const {
			return MetalCode.c_str();
		}
		void SetMetalCode(const char* code) {
			MetalCode = code;
		}

		void SetDXBC(const BYTE * pData, UINT size)
		{
			Dxbc.resize(size);
			memcpy(&Dxbc[0], pData, size);
		}
		void SetSpirV(const BYTE* pData, UINT size)
		{
			SpirV.resize(size);
			memcpy(&SpirV[0], pData, size);
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
		IShaderDefinitions : public VIUnknown
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
		FShaderCode : public VIUnknown
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
	typedef FShaderCode* (*FnGetShaderCodeStream)(TR_META(SV_NoStringConverter) const char* name);
	class TR_CLASS()
		FShaderCompiler : public VIUnknown
	{
		typedef FShaderCode* (*FnGetShaderCodeStream)(TR_META(SV_NoStringConverter) const char* name);
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
		FShaderCode* GetShaderCodeStream(const char* name);
		bool CompileShader(FShaderDesc* desc, const char* shader, const char* entry, EShaderType type, const char* sm, const IShaderDefinitions* defines, EShaderLanguage sl, bool bDebugShader);
	};
}

NS_END