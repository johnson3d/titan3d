#include "IRenderContext.h"
#include "ISwapChain.h"
#include "../CSharpAPI.h"
#include "IShaderProgram.h"

#include "../Core/io/vfxfile.h"
#include "../Graphics/GfxEngine.h"

#if defined(PLATFORM_WIN)
#include "D3D11/D11PreHead.h"
#include "../HLSLcc/include/hlslcc.h"
#include "../Platform/Windows/Glsl2Vulkan.h"
#include "../../Bricks/CrossShaderCompiler/CscShaderConductor.h"
#endif

#define new VNEW

NS_BEGIN

int IRenderContext::mChooseShaderModel = 5;

IRenderContext::IRenderContext()
{
	m_pSwapChain = nullptr;
#if defined(PLATFORM_WIN)
	ShInitialize();
#elif defined(PLATFORM_DROID)
#elif defined(PLATFORM_IOS)
#endif
}


IRenderContext::~IRenderContext()
{
	Safe_Release(m_pSwapChain);
#if defined(PLATFORM_WIN)
	ShFinalize();
#endif
}

//void IRenderContext::BindCurrentSwapChain(ISwapChain* pSwapChain)
//{
//	/*if (swapChain == m_pSwapChain)
//		return;*/
//	if (pSwapChain)
//	{
//		pSwapChain->AddRef();
//		//pSwapChain->BindCurrent();
//	}
//	Safe_Release(m_pSwapChain);
//	m_pSwapChain = pSwapChain;
//}

//void IRenderContext::Present(UINT SyncInterval, UINT Flags)
//{
//	if (m_pSwapChain != nullptr)
//	{
//		//bind current is only for gles;
//		m_pSwapChain->BindCurrent();
//		m_pSwapChain->Present(SyncInterval, Flags);
//	}
//}

IConstantBuffer* IRenderContext::CreateConstantBuffer(IShaderProgram* program, UINT index)
{
	auto cb = program->GetCBuffer(index);
	if (cb == nullptr)
		return nullptr;
	return this->CreateConstantBuffer(cb);
}

IConstantBuffer* IRenderContext::CreateConstantBuffer2(IShaderDesc* desc, UINT index)
{
	if (index >= (UINT)desc->Reflector->mCBDescArray.size())
		return nullptr;
	
	const auto& cb = desc->Reflector->mCBDescArray[index];
	return this->CreateConstantBuffer(&cb);
}

void WriteIncludeFile(std::string& io, const std::string& file)
{
	auto content = &GfxEngine::Instance->GetContentPath();
	auto engine = &GfxEngine::Instance->GetEnginePath();
	auto editor = &GfxEngine::Instance->GetEditorPath();

	//const std::string* path = nullptr;

	std::string fullName;
	auto pos = file.find("@Engine/", 0);
	if (pos == 0)
	{
		fullName = *engine + file.substr(strlen("@Engine/"));
	}
	else if ((pos = file.find("@Editor/", 0)) == 0)
	{
		fullName = *editor + file.substr(strlen("@Editor/"));
	}
	else
	{
		fullName = *content + file;
	}

	io += fullName + "\"\n";
	//io.Write(fullName.c_str(), fullName.length());
	//io.Write("\"\n", strlen("\"\n"));
}

void Write_Extra_Include(std::string& io, const IShaderDefinitions* defines)
{
	auto mtl = defines->FindDefine("EXTRA_Include");
	if (mtl != nullptr)
	{
		auto incFiles = mtl->Definition;
		size_t pos = std::string::npos;
		do
		{
			pos = incFiles.find_first_of(';');
			if (pos != std::string::npos)
			{
				auto fname = incFiles.substr(0, pos);
				if (fname.length() > 0)
				{
					//io.Write("#include \"", strlen("#include \""));
					io += "#include \"";
					WriteIncludeFile(io, fname);
				}
				incFiles = incFiles.substr(pos + 1);
			}
		} while (pos != std::string::npos);

		if (incFiles.length() > 0)
		{
			//io.Write("#include \"", strlen("#include \""));
			io += "#include \"";
			WriteIncludeFile(io, incFiles);
		}
	}
}

void Write_Extra_Defines(std::string& io, const IShaderDefinitions* defines)
{
	auto mtl = defines->FindDefine("EXTRA_Defines");
	if (mtl != nullptr)
	{
		auto incFiles = mtl->Definition;
		size_t pos = std::string::npos;
		do
		{
			pos = incFiles.find_first_of(';');
			if (pos != std::string::npos)
			{
				auto fname = incFiles.substr(0, pos);
				if (fname.length() > 0)
				{
					//io.Write("#include \"", strlen("#include \""));
					io += "#include \"";
					WriteIncludeFile(io, fname);
				}
				incFiles = incFiles.substr(pos + 1);
			}
		} while (pos != std::string::npos);

		if (incFiles.length() > 0)
		{
			//io.Write("#include \"", strlen("#include \""));
			io += "#include \"";
			WriteIncludeFile(io, incFiles);
		}
	}
}

#if defined(PLATFORM_WIN)
struct EngineInclude : public ID3DInclude
{
	~EngineInclude()
	{
		for (auto i : Data2FileMap)
		{
			if(i.first!=nullptr)
				delete[] (BYTE*)(i.first);
		}
		Data2FileMap.clear();
	}
	std::string cbPerInstance_var;
	std::string dummy_gen;
	std::map<LPCVOID, std::string> Data2FileMap;
	virtual COM_DECLSPEC_NOTHROW HRESULT STDMETHODCALLTYPE Open(D3D_INCLUDE_TYPE IncludeType, LPCSTR pFileName, LPCVOID pParentData, LPCVOID *ppData, UINT *pBytes)
	{
		std::string parent;
		bool isAbs = false;
		if (strlen(pFileName) > 2 && pFileName[1] == ':')
		{
			isAbs = true;
		}
		else
		{
			auto iter = Data2FileMap.find(pParentData);
			if (iter != Data2FileMap.end())
			{
				parent = iter->second;
			}
		}

		std::string root;
		if (isAbs)
		{
			root = pFileName;
		}
		else
		{
			root = parent + pFileName;
		}

		if (strcmp(pFileName, "../cbPerInstance.var")==0)
		{
			*pBytes = (UINT)cbPerInstance_var.size();
			*ppData = new BYTE[*pBytes + 1];
			((BYTE*)*ppData)[*pBytes] = NULL;
			memcpy((void*)(*ppData), &cbPerInstance_var[0], *pBytes);
		}
		else if (strcmp(pFileName, "dummy.gen") == 0)
		{
			*pBytes = (UINT)dummy_gen.size();
			*ppData = new BYTE[*pBytes + 1];
			((BYTE*)*ppData)[*pBytes] = NULL;
			memcpy((void*)(*ppData), &dummy_gen[0], *pBytes);
		}
		else
		{
			ViseFile io;
			if (io.Open(root.c_str(), ViseFile::modeRead) == FALSE)
				return E_FAIL;

			*pBytes = (UINT)io.GetLength();
			*ppData = new BYTE[*pBytes + 1];
			((BYTE*)*ppData)[*pBytes] = NULL;

			io.Read((void*)(*ppData), *pBytes);

			io.Close();
		}
		auto dir = GetPath(root);
		Data2FileMap.insert(std::make_pair(*ppData, dir));
		return S_OK;
	}
	virtual COM_DECLSPEC_NOTHROW HRESULT STDMETHODCALLTYPE Close(LPCVOID pData)
	{
		//delete[] pData;
		return S_OK;
	}
	std::string GetPath(std::string file)
	{
		for (size_t i = 0; i < file.size(); i++)
		{
			if (file[i] == '\\')
				file[i] = '/';
		}
		size_t pos = std::string::npos;
		for (int i = (int)file.size()-1; i >=0 ; i--)
		{
			if (file[i] == '/')
			{
				pos = i;
				break;
			}
		}
		std::string result = file.substr(0, pos + 1);
		return result;
	}
};

VCritical HlslCompileLocker;
IShaderDesc* IRenderContext::CompileHLSLFromFile(LPCSTR file, LPCSTR entry, LPCSTR sm, const IShaderDefinitions* defines, DWORD CrossPlatfoms, vBOOL bDebugShader)
{
	EngineInclude engineInc;
	std::vector<D3D10_SHADER_MACRO> d11macros;

	//HlslCompileLocker.Lock();
	ViseFile io_pre;
	std::string path = file;
	auto pos = path.find_last_of('/');
	if (pos != std::string::npos)
	{
		path = path.substr(0, pos+1);
	}
	if (defines != nullptr)
	{
		Write_Extra_Include(engineInc.dummy_gen, defines);

		Write_Extra_Defines(engineInc.cbPerInstance_var, defines);

		for (size_t i = 0 ; i<defines->Definitions.size(); i++)
		{
			D3D10_SHADER_MACRO tmp;
			tmp.Name = defines->Definitions[i].Name.c_str();
			tmp.Definition = defines->Definitions[i].Definition.c_str();
			d11macros.push_back(tmp);
		}
		D3D10_SHADER_MACRO end;
		end.Name = nullptr;
		end.Definition = nullptr;
		d11macros.push_back(end);
	}

	DWORD dwShaderFlags = D3DCOMPILE_ENABLE_STRICTNESS;

	if (bDebugShader)
	{
		dwShaderFlags |= D3DCOMPILE_DEBUG;
	}

	ID3DBlob* pBlob;
	ID3DBlob* pError;
	D3D10_SHADER_MACRO* pMacros = nullptr;
	if (d11macros.size() > 0)
		pMacros = &d11macros[0];

	engineInc.Data2FileMap.insert(std::make_pair(nullptr, engineInc.GetPath(file)));
	std::string code;
	{
		ViseFile io;
		if (io.Open(file, ViseFile::modeRead) == FALSE)
		{
			VFX_LTRACE(ELTT_Graphics, "CompileHLSLFromFile failed:%s\r\n", file);
			return false;
		}
		{
			int len = (int)io.GetLength();
			char* buffer = new char[len + 1];
			buffer[len] = '\0';
			io.Read(buffer, len);
			code = buffer;
			delete[] buffer;
		}
		io.Close();
	}


	auto hr = D3DCompile(code.c_str(), code.length(), file, pMacros, &engineInc, entry, sm, dwShaderFlags, 0, &pBlob, &pError);
	if (pError != NULL)
	{
		//ASSERT(false);
		VFX_LTRACE(ELTT_Graphics, (char*)pError->GetBufferPointer());
		pError->Release();
	}
	if (FAILED(hr))
	{
		ASSERT(false);
		return nullptr;
	}

	auto desc = new IShaderDesc(GetShaderTypeFrom(sm));
	desc->SetCodes((BYTE*)pBlob->GetBufferPointer(), (UINT)pBlob->GetBufferSize());
	
	ReflectShader(desc);

	bool hasGLSL = (CrossPlatfoms&PLTF_Android) ? true : false;
	bool hasMetal = (CrossPlatfoms&PLTF_AppleIOS) ? true : false;
	if (CrossPlatfoms != 1)
	{
		CscShaderConductor::Includer cscIncluder;
		cscIncluder.cbPerInstance_var = engineInc.cbPerInstance_var;
		cscIncluder.dummy_gen = engineInc.dummy_gen;
		
		if (false == CscShaderConductor::GetInstance()->CompileHLSL(desc, file, file, entry, sm, defines, &cscIncluder, hasGLSL, hasMetal))
		{
			//HLSLcc(desc);
			//ASSERT(false);
		}
	}
	
	return desc;
}

inline EShaderVarType D11ShaderVarType2DX(D3D_SHADER_VARIABLE_TYPE type, UINT col, UINT row)
{
	switch (type)
	{
		case D3D_SVT_VOID:
		{
			return SVT_Struct;
		}
		case D3D_SVT_FLOAT:
		{
			if (row == 1)
			{
				switch (col)
				{
				case 1:
					return SVT_Float1;
				case 2:
					return SVT_Float2;
				case 3:
					return SVT_Float3;
				case 4:
					return SVT_Float4;
				}
			}
			else if (row == 3 && col == 3)
			{
				return SVT_Matrix3x3;
			}
			else if (row == 4 && col == 4)
			{
				return SVT_Matrix4x4;
			}
			break;
		}
		case D3D_SVT_INT:
		{
			switch (col)
			{
			case 1:
				return SVT_Int1;
			case 2:
				return SVT_Int2;
			case 3:
				return SVT_Int3;
			case 4:
				return SVT_Int4;
			}
			break;
		}
		default:
			break;
	}
	return SVT_Unknown;
}

bool IRenderContext::ReflectShader(IShaderDesc* desc)
{
	if (desc->Reflector == nullptr)
	{
		desc->Reflector = new ShaderReflector();
	}
	else
	{
		desc->Reflector->mCBDescArray.clear();
		desc->Reflector->mTexBindInfoArray.clear();
		desc->Reflector->mSamplerBindInfoArray.clear();
	}
	auto Reflector = desc->Reflector;
	ID3D11ShaderReflection* pReflection;
	auto hr = D3DReflect(&desc->GetCodes()[0], desc->GetCodes().size(), IID_ID3D11ShaderReflection, (void**)&pReflection);
	if (FAILED(hr))
	{
		return false;
	}
	D3D11_SHADER_DESC shaderDesc;
	pReflection->GetDesc(&shaderDesc);

	for (UINT i = 0; i < shaderDesc.InputParameters; i++)
	{
		D3D11_SIGNATURE_PARAMETER_DESC desc;
		pReflection->GetInputParameterDesc(i, &desc);
		pReflection->GetInputParameterDesc(i, &desc);
	}

#define DescSetBindPoint(type, dest, src) do{\
		switch (type) \
		{\
				case EST_VertexShader:\
					dest.VSBindPoint = src.BindPoint;\
					break;\
				case EST_PixelShader:\
					dest.PSBindPoint = src.BindPoint;\
					break;\
				case EST_ComputeShader:\
					dest.CSBindPoint = src.BindPoint;\
					break;\
		}\
		dest.BindCount = src.BindCount;\
	}\
	while(0);

	for (UINT i = 0; i < shaderDesc.BoundResources; i++)
	{
		D3D11_SHADER_INPUT_BIND_DESC csibDesc;
		pReflection->GetResourceBindingDesc(i, &csibDesc);
		ID3D11ShaderReflectionVariable* pVari = pReflection->GetVariableByName(csibDesc.Name);
		D3D11_SHADER_VARIABLE_DESC varDesc;
		auto hr = pVari->GetDesc(&varDesc);
		if (SUCCEEDED(hr))
		{
			if ((varDesc.uFlags & D3D10_SVF_USED) == 0)
			{
				continue;
			}
		}
		switch (csibDesc.Type)
		{
			case D3D_SIT_CBUFFER:
			case D3D_SIT_UAV_RWTYPED:
			case D3D_SIT_UAV_RWSTRUCTURED:
			case D3D_SIT_BYTEADDRESS:
			case D3D_SIT_UAV_RWBYTEADDRESS:
			case D3D_SIT_UAV_APPEND_STRUCTURED:
			case D3D_SIT_UAV_CONSUME_STRUCTURED:
			case D3D_SIT_UAV_RWSTRUCTURED_WITH_COUNTER:
			{
				ID3D11ShaderReflectionConstantBuffer* pCBuffer = pReflection->GetConstantBufferByName(csibDesc.Name);
				D3D11_SHADER_BUFFER_DESC Desc;
				auto hr = pCBuffer->GetDesc(&Desc);

				IConstantBufferDesc tcbDesc;
				tcbDesc.Type = (ECBufferRhiType)csibDesc.Type;
				tcbDesc.Name = csibDesc.Name;// Desc.Name;
				DescSetBindPoint(desc->ShaderType, tcbDesc, csibDesc);
				if (hr == S_OK)
				{
					tcbDesc.Size = Desc.Size;
					for (UINT j = 0; j < Desc.Variables; j++)
					{
						ID3D11ShaderReflectionVariable* pVari = pCBuffer->GetVariableByIndex(j);
						D3D11_SHADER_VARIABLE_DESC varDesc;
						pVari->GetDesc(&varDesc);

						D3D11_SHADER_TYPE_DESC stDesc;
						pVari->GetType()->GetDesc(&stDesc);

						ConstantVarDesc tcvDesc;
						tcvDesc.Name = varDesc.Name;
						tcvDesc.Type = D11ShaderVarType2DX(stDesc.Type, stDesc.Columns, stDesc.Rows);
						tcvDesc.Offset = varDesc.StartOffset;
						tcvDesc.Size = varDesc.Size;
						if (stDesc.Elements == 0)
							tcvDesc.Elements = 1;
						else
							tcvDesc.Elements = stDesc.Elements;

						tcbDesc.Vars.push_back(tcvDesc);
					}
				}
				else
				{
					tcbDesc.Size = 0;
				}

				Reflector->mCBDescArray.push_back(tcbDesc);
			}
			break;
			case D3D_SIT_TBUFFER:
			case D3D_SIT_STRUCTURED:
			{
				TextureBindInfo tmp;
				tmp.Type = (ECBufferRhiType)csibDesc.Type;
				tmp.Name = csibDesc.Name;
				DescSetBindPoint(desc->ShaderType, tmp, csibDesc);

				Reflector->mTexBindInfoArray.push_back(tmp);
			}
			break;
			case D3D_SIT_TEXTURE: 
			{
				TextureBindInfo tmp;
				tmp.Type = (ECBufferRhiType)csibDesc.Type;
				tmp.Name = csibDesc.Name;
				DescSetBindPoint(desc->ShaderType, tmp, csibDesc);

				Reflector->mTexBindInfoArray.push_back(tmp);
			}
			break;
			case D3D_SIT_SAMPLER:
			{
				SamplerBindInfo tmp;
				tmp.Type = (ECBufferRhiType)csibDesc.Type;
				tmp.Name = csibDesc.Name;
				DescSetBindPoint(desc->ShaderType, tmp, csibDesc);

				Reflector->mSamplerBindInfoArray.push_back(tmp);
			}
			break;
			default:
				break;
		}
	}
	return true;
}

//void CompileVulkanSpirV(const char* exe, const char* glsl, std::vector<UINT>& bc)
//{
//	char cmd[] = "-V shader.tmp";
//	STARTUPINFOA si;
//	PROCESS_INFORMATION pi;
//	ZeroMemory(&si, sizeof(si));
//	si.cb = sizeof(si);
//	ZeroMemory(&pi, sizeof(pi));
//	auto ret = ::CreateProcessA(exe, cmd, NULL, NULL, FALSE, 0/*CREATE_NEW_CONSOLE*/, NULL, NULL, &si, &pi);
//	if (ret)
//	{
//		CloseHandle(pi.hThread);
//		CloseHandle(pi.hProcess);
//	}
//}

HLSLccReflection cb;

bool IRenderContext::HLSLcc(IShaderDesc* desc)
{
	GlExtensions ext;
	ext.ARB_explicit_attrib_location = 0;
	ext.ARB_explicit_uniform_location = 0;
	ext.ARB_shading_language_420pack = 0;
	GLSLCrossDependencyData cdd;
	HLSLccSamplerPrecisionInfo spi;

	GLSLShader outShader;
	//unsigned int transFlag = HLSLCC_FLAG_DISABLE_GLOBALS_STRUCT;// HLSLCC_FLAG_DISABLE_GLOBALS_STRUCT;// HLSLCC_FLAG_UNIFORM_BUFFER_OBJECT;
	unsigned int transFlag = HLSLCC_FLAG_INOUT_SEMANTIC_NAMES |
		//HLSLCC_FLAG_DISABLE_EXPLICIT_LOCATIONS |
		//HLSLCC_FLAG_WRAP_UBO |
		HLSLCC_FLAG_DISABLE_GLOBALS_STRUCT |
		HLSLCC_FLAG_GLOBAL_CONSTS_NEVER_IN_UBO |
		//HLSLCC_FLAG_REMOVE_UNUSED_GLOBALS |
		//HLSLCC_FLAG_TRANSLATE_MATRICES |
		HLSLCC_FLAG_UNIFORM_BUFFER_OBJECT |
		HLSLCC_FLAG_GLES31_IMAGE_QUALIFIERS;

	auto result = TranslateHLSLFromMem((const char*)&desc->GetCodes()[0],
		transFlag,
		LANG_ES_310,//LANG_METAL, //
		&ext,
		&cdd,
		spi,
		cb,
		&outShader);
	if (result == 0)
		return false;

	auto pos = outShader.sourceCode.find("#version 310 es");
	if (pos != std::string::npos)
	{
		outShader.sourceCode.replace(pos, pos + strlen("#version 310 es"), "#version 310 es\nprecision highp float;");
	}
	desc->Es300Code = outShader.sourceCode;

	//auto hCompiler = ShConstructCompiler(EShLangVertex, 0);
	ShaderCC::GlslangHelper glsHelper;
	auto controls = glsHelper.DeriveOptions(ShaderCC::Source::GLSL, ShaderCC::Semantics::OpenGL, ShaderCC::Target::AST);
	ShaderCC::GlslangResult glsResult = glsHelper.compileAndLink(EShLangVertex, desc->Es300Code, "VS", controls,
		glslang::EShTargetVulkan_1_0, false,
		EShTexSampTransKeep, false, true);

	std::ostringstream stream;
	glsHelper.outputResultToStream(&stream, glsResult, controls);

	result = TranslateHLSLFromMem((const char*)&desc->GetCodes()[0],
		transFlag,
		LANG_METAL,
		&ext,
		&cdd,
		spi,
		cb,
		&outShader);
	if (result == 0)
		return false;

	desc->MetalCode = outShader.sourceCode;
	return true;
}
#else
IShaderDesc* IRenderContext::CompileHLSLFromFile(LPCSTR file, LPCSTR entry, LPCSTR sm, const IShaderDefinitions* defines, DWORD CrossPlatfoms, vBOOL bDebugShader)
{
	return nullptr;
}
#endif


NS_END

using namespace EngineNS;

extern "C"
{
	CSharpReturnAPI0(int, EngineNS, IRenderContext, GetShaderModel);
	VFX_API void SDK_IRenderContext_ChooseShaderModel(int sm)
	{
		IRenderContext::mChooseShaderModel = sm;
	}
	CSharpReturnAPI0(ICommandList*, EngineNS, IRenderContext, GetImmCommandList);
	CSharpAPI0(EngineNS, IRenderContext, FlushImmContext);
	CSharpReturnAPI0(IPass*, EngineNS, IRenderContext, CreatePass);
	CSharpReturnAPI1(ISwapChain*, EngineNS, IRenderContext, CreateSwapChain, const ISwapChainDesc*);
	//CSharpAPI1(EngineNS, IRenderContext, BindCurrentSwapChain, EngineNS::ISwapChain*);
	//CSharpAPI2(EngineNS, IRenderContext, Present, UINT, UINT);
	CSharpReturnAPI1(IShaderResourceView*, EngineNS, IRenderContext, LoadShaderResourceView, const char*);
	CSharpReturnAPI1(ITexture2D*, EngineNS, IRenderContext, CreateTexture2D, const EngineNS::ITexture2DDesc*);
	CSharpReturnAPI1(IRenderTargetView*, EngineNS, IRenderContext, CreateRenderTargetView, const EngineNS::IRenderTargetViewDesc*);
	CSharpReturnAPI1(IDepthStencilView*, EngineNS, IRenderContext, CreateDepthRenderTargetView, const EngineNS::IDepthStencilViewDesc*);
	CSharpReturnAPI1(IShaderResourceView*, EngineNS, IRenderContext, CreateShaderResourceView, const IShaderResourceViewDesc*);
	CSharpReturnAPI2(IGpuBuffer*, EngineNS, IRenderContext, CreateGpuBuffer, const IGpuBufferDesc*, void*);
	CSharpReturnAPI2(IShaderResourceView*, EngineNS, IRenderContext, CreateShaderResourceViewFromBuffer, IGpuBuffer*, const ISRVDesc*);
	CSharpReturnAPI2(IUnorderedAccessView*, EngineNS, IRenderContext, CreateUnorderedAccessView, IGpuBuffer*, const IUnorderedAccessViewDesc*);
	CSharpReturnAPI1(IFrameBuffers*, EngineNS, IRenderContext, CreateFrameBuffers, const IFrameBuffersDesc*);
	CSharpReturnAPI1(ICommandList*, EngineNS, IRenderContext, CreateCommandList, const ICommandListDesc*);
	CSharpReturnAPI1(IShaderProgram*, EngineNS, IRenderContext, CreateShaderProgram, const IShaderProgramDesc*);
	CSharpReturnAPI6(IShaderDesc*, EngineNS, IRenderContext, CompileHLSLFromFile, LPCSTR, LPCSTR, LPCSTR, const IShaderDefinitions*, DWORD, vBOOL);
	CSharpReturnAPI1(IVertexShader*, EngineNS, IRenderContext, CreateVertexShader, const IShaderDesc*);
	CSharpReturnAPI1(IPixelShader*, EngineNS, IRenderContext, CreatePixelShader, const IShaderDesc*);
	CSharpReturnAPI1(IComputeShader*, EngineNS, IRenderContext, CreateComputeShader, const IShaderDesc*);
	CSharpReturnAPI1(IInputLayout*, EngineNS, IRenderContext, CreateInputLayout, const IInputLayoutDesc*);
	CSharpReturnAPI1(IRenderPipeline*, EngineNS, IRenderContext, CreateRenderPipeline, const IRenderPipelineDesc*);
	CSharpReturnAPI0(IGeometryMesh*, EngineNS, IRenderContext, CreateGeometryMesh);
	CSharpReturnAPI1(IVertexBuffer*, EngineNS, IRenderContext, CreateVertexBuffer, const IVertexBufferDesc*);
	CSharpReturnAPI1(IIndexBuffer*, EngineNS, IRenderContext, CreateIndexBuffer, const IIndexBufferDesc*);
	CSharpReturnAPI2(IIndexBuffer*, EngineNS, IRenderContext, CreateIndexBufferFromBuffer, const IIndexBufferDesc*, const IGpuBuffer*);
	CSharpReturnAPI2(IVertexBuffer*, EngineNS, IRenderContext, CreateVertexBufferFromBuffer, const IVertexBufferDesc*, const IGpuBuffer*);
	CSharpReturnAPI1(ISamplerState*, EngineNS, IRenderContext, CreateSamplerState, const ISamplerStateDesc*);
	CSharpReturnAPI1(IRasterizerState*, EngineNS, IRenderContext, CreateRasterizerState, const IRasterizerStateDesc*);
	CSharpReturnAPI1(IDepthStencilState*, EngineNS, IRenderContext, CreateDepthStencilState, const IDepthStencilStateDesc*);
	CSharpReturnAPI1(IBlendState*, EngineNS, IRenderContext, CreateBlendState, const IBlendStateDesc*);
	CSharpReturnAPI2(IConstantBuffer*, EngineNS, IRenderContext, CreateConstantBuffer, IShaderProgram*, int);
	CSharpReturnAPI2(IConstantBuffer*, EngineNS, IRenderContext, CreateConstantBuffer2, IShaderDesc*, int);
	CSharpAPI1(EngineNS, IRenderContext, GetRenderContextCaps, IRenderContextCaps*);
	CSharpAPI1(EngineNS, IRenderContext, UnsafeSetRenderContextCaps, IRenderContextCaps*);
}