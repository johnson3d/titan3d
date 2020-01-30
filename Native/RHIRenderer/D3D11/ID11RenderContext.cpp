#include "ID11RenderContext.h"
#include "ID11CommandList.h"
#include "ID11RenderPipeline.h"
#include "ID11ConstantBuffer.h"
#include "ID11VertexBuffer.h"
#include "ID11IndexBuffer.h"
#include "ID11RenderTargetView.h"
#include "ID11DepthStencilView.h"
#include "ID11ShaderResourceView.h"
#include "ID11UnorderedAccessView.h"
#include "ID11VertexShader.h"
#include "ID11PixelShader.h"
#include "ID11ComputeShader.h"
#include "ID11SwapChain.h"
#include "ID11RenderSystem.h"
#include "ID11Texture2D.h"
#include "ID11VertexShader.h"
#include "ID11PixelShader.h"
#include "ID11InputLayout.h"
#include "ID11SamplerState.h"
#include "ID11RasterizerState.h"
#include "ID11DepthStencilState.h"
#include "ID11BlendState.h"
#include "ID11FrameBuffers.h"
#include "ID11ShaderProgram.h"
#include "ID11GeometryMesh.h"
#include "ID11Pass.h"

#include "../../HLSLcc/include/hlslcc.h"
#include "../../Platform/Windows/Glsl2Vulkan.h"

#define new VNEW

NS_BEGIN

ID11RenderContext* ID11RenderContext::DefaultRenderContext = nullptr;

ID11RenderContext::ID11RenderContext()
{
	mDevice = nullptr;
	mD3dDebug = nullptr;
	mFeatureLevel = D3D_FEATURE_LEVEL_11_0;
	mImmCmdList = nullptr;
	mHardwareContext = nullptr;
	//mDXGIDevice = nullptr;
	mSystem = nullptr;

	DefaultRenderContext = this;
}


ID11RenderContext::~ID11RenderContext()
{
	DefaultRenderContext = nullptr;

	Safe_Release(mHardwareContext);
	Safe_Release(mImmCmdList);

	if (mD3dDebug != nullptr)
	{
		HRESULT hr = hr = mD3dDebug->ReportLiveDeviceObjects(D3D11_RLDO_DETAIL);
		mD3dDebug->Release();
	}
	Safe_Release(mDevice);
	//Safe_Release(mDXGIDevice);
	Safe_Release(mSystem);
}

bool ID11RenderContext::Init(ID11RenderSystem* sys, const IRenderContextDesc* desc)
{
	if (sys)
		sys->AddRef();
	Safe_Release(mSystem);
	mSystem = sys;
	UINT createDeviceFlags = 0;
	if(desc->CreateDebugLayer!=FALSE)
		createDeviceFlags |= D3D11_CREATE_DEVICE_DEBUG;
	D3D_FEATURE_LEVEL featureLevels[] =
	{
		D3D_FEATURE_LEVEL_11_0,
		D3D_FEATURE_LEVEL_11_1,
		D3D_FEATURE_LEVEL_10_0,
	};
	UINT numFeatureLevels = ARRAYSIZE(featureLevels);

	auto hr = D3D11CreateDevice(NULL, D3D_DRIVER_TYPE_HARDWARE, NULL/*(HMODULE)desc->AppHandle*/, createDeviceFlags,
		featureLevels, numFeatureLevels, D3D11_SDK_VERSION, &mDevice, &mFeatureLevel, &mHardwareContext);
	if (FAILED(hr))
		return false;

	ID3D11DeviceContext* pImmContext;
	hr = mDevice->CreateDeferredContext(0, &pImmContext);

	mImmCmdList = new ID11CommandList();
	mImmCmdList->InitD11Point(this, pImmContext);
	pImmContext->Release();

	
	//mDevice->QueryInterface(IID_IDXGIDevice, (void**)&mDXGIDevice);

	hr = mDevice->QueryInterface(__uuidof(ID3D11Debug), reinterpret_cast<void**>(&mD3dDebug));
	if (SUCCEEDED(hr))
	{
		
	}
	return true;
}

void ID11RenderContext::FlushImmContext()
{
	mImmCmdList->EndCommand();
	mImmCmdList->Commit(this);
}

ISwapChain* ID11RenderContext::CreateSwapChain(const ISwapChainDesc* desc)
{
	auto swapchain = new ID11SwapChain();
	if (swapchain->Init(this, desc) == false)
	{
		swapchain->Release();
		return nullptr;
	}
	return swapchain;
}

ICommandList* ID11RenderContext::CreateCommandList(const ICommandListDesc* desc)
{
	auto cmd_list = new ID11CommandList();
	if (cmd_list->Init(this, desc) == false)
	{
		cmd_list->Release();
		return nullptr;
	}
	return cmd_list;
}

IPass* ID11RenderContext::CreatePass()
{
	auto pass = new ID11Pass();
	return pass;
}

IRenderPipeline* ID11RenderContext::CreateRenderPipeline(const IRenderPipelineDesc* desc)
{
	auto rpl = new ID11RenderPipeline();
	return rpl;
}

//������Ϣ
IVertexBuffer* ID11RenderContext::CreateVertexBuffer(const IVertexBufferDesc* desc)
{
	auto vb = new ID11VertexBuffer();
	if (vb->Init(this, desc) == false)
	{
		vb->Release();
		return nullptr;
	}
	return vb;
}

IIndexBuffer* ID11RenderContext::CreateIndexBuffer(const IIndexBufferDesc* desc)
{
	auto ib = new ID11IndexBuffer();
	if (ib->Init(this, desc) == false)
	{
		ib->Release();
		return nullptr;
	}
	return ib;
}

IIndexBuffer* ID11RenderContext::CreateIndexBufferFromBuffer(const IIndexBufferDesc* desc, const IGpuBuffer* pBuffer)
{
	auto ib = new ID11IndexBuffer();
	if (ib->Init(this, desc, pBuffer) == false)
	{
		ib->Release();
		return nullptr;
	}
	return ib;
}

IVertexBuffer* ID11RenderContext::CreateVertexBufferFromBuffer(const IVertexBufferDesc* desc, const IGpuBuffer* pBuffer)
{
	auto vb = new ID11VertexBuffer();
	if (vb->Init(this, desc, pBuffer) == false)
	{
		vb->Release();
		return nullptr;
	}
	return vb;
}

IGeometryMesh* ID11RenderContext::CreateGeometryMesh()
{
	return new ID11GeometryMesh();
}
//��Դ��Ϣ
IFrameBuffers* ID11RenderContext::CreateFrameBuffers(const IFrameBuffersDesc* desc)
{
	auto rt = new ID11FrameBuffers();
	if (rt->Init(this, desc) == false)
	{
		rt->Release();
		return nullptr;
	}
	return rt;
}
IRenderTargetView* ID11RenderContext::CreateRenderTargetView(const IRenderTargetViewDesc* desc)
{
	auto rt = new ID11RenderTargetView();
	if (rt->Init(this, desc) == false)
	{
		rt->Release();
		return nullptr;
	}
	return rt;
}

IDepthStencilView* ID11RenderContext::CreateDepthRenderTargetView(const IDepthStencilViewDesc* desc)
{
	auto drt = new ID11DepthStencilView();
	if (drt->Init(this, desc) == false)
	{
		drt->Release();
		return nullptr;
	}
	return drt;
}

ITexture2D* ID11RenderContext::CreateTexture2D(const ITexture2DDesc* desc)
{
	auto texture = new ID11Texture2D();
	if (texture->Init(this, desc) == false)
	{
		texture->Release();
		return nullptr;
	}
	return texture;
}

IShaderResourceView* ID11RenderContext::CreateShaderResourceView(const IShaderResourceViewDesc* desc)
{
	auto view = new ID11ShaderResourceView();
	if (view->Init(this, desc) == false)
	{
		view->Release();
		return nullptr;
	}
	view->GetResourceState()->SetStreamState(SS_Valid);
	return view;
}

IGpuBuffer* ID11RenderContext::CreateGpuBuffer(const IGpuBufferDesc* desc, void* pInitData)
{
	ID3D11Buffer* pBuffer;
	if (pInitData)
	{
		D3D11_SUBRESOURCE_DATA InitData;
		InitData.pSysMem = pInitData;
		auto hr = mDevice->CreateBuffer((D3D11_BUFFER_DESC*)desc, &InitData, &pBuffer);
		if (hr != S_OK)
			return nullptr;
	}
	else
	{
		auto hr = mDevice->CreateBuffer((D3D11_BUFFER_DESC*)desc, NULL, &pBuffer);
		if (hr != S_OK)
			return nullptr;
	}

	auto result = new ID11GpuBuffer();
	result->mBuffer = pBuffer;
	result->mDesc = *((D3D11_BUFFER_DESC*)desc);

	result->GetResourceState()->SetStreamState(SS_Valid);
	result->GetResourceState()->SetResourceSize(desc->ByteWidth);
	return result;
}

IShaderResourceView* ID11RenderContext::CreateShaderResourceViewFromBuffer(IGpuBuffer* pBuffer, const ISRVDesc* desc)
{
	auto view = new ID11ShaderResourceView();
	if (view->Init(this, (ID11GpuBuffer*)pBuffer, desc) == false)
	{
		view->Release();
		return nullptr;
	}
	view->GetResourceState()->SetStreamState(SS_Valid);
	view->GetResourceState()->SetResourceSize(pBuffer->GetResourceState()->GetResourceSize());
	return view;
}

IUnorderedAccessView* ID11RenderContext::CreateUnorderedAccessView(IGpuBuffer* pBuffer, const IUnorderedAccessViewDesc* desc)
{
	auto view = new ID11UnorderedAccessView();
	if (view->Init(this, ((ID11GpuBuffer*)pBuffer)->mBuffer, desc) == false)
	{
		view->Release();
		return nullptr;
	}
	return view;
}

ISamplerState* ID11RenderContext::CreateSamplerState(const ISamplerStateDesc* desc)
{
	auto sampler = new ID11SamplerState();
	if (sampler->Init(this, desc) == false)
	{
		sampler->Release();
		return nullptr;
	}
	return sampler;
}
IRasterizerState* ID11RenderContext::CreateRasterizerState(const IRasterizerStateDesc* desc)
{
	auto state = new ID11RasterizerState();
	if (state->Init(this, desc) == false)
	{
		state->Release();
		return nullptr;
	}
	return state;
}
IDepthStencilState* ID11RenderContext::CreateDepthStencilState(const IDepthStencilStateDesc* desc)
{
	auto state = new ID11DepthStencilState();
	if (state->Init(this, desc) == false)
	{
		state->Release();
		return nullptr;
	}
	return state;
}
IBlendState* ID11RenderContext::CreateBlendState(const IBlendStateDesc* desc)
{
	auto state = new ID11BlendState();
	if (state->Init(this, desc) == false)
	{
		state->Release();
		return nullptr;
	}
	return state;
}
IShaderResourceView* ID11RenderContext::LoadShaderResourceView(const char* file)
{
	auto view = new ID11ShaderResourceView();
	if (false == view->InitD11View(this, file, false))
	{
		view->Release();
		return nullptr;
	}return view;
}
//shader
IShaderProgram* ID11RenderContext::CreateShaderProgram(const IShaderProgramDesc* desc)
{
	auto program = new ID11ShaderProgram();
	if (program->Init(this, desc) == false)
	{
		program->Release();
		return nullptr;
	}
	return program;
}

IVertexShader* ID11RenderContext::CreateVertexShader(const IShaderDesc* desc)
{
	auto vs = new ID11VertexShader();
	if (vs->Init(this, desc) == false)
	{
		vs->Release();
		return nullptr;
	}
	if (desc->Reflector == nullptr)
	{
		ReflectShader((IShaderDesc*)desc);
	}
	return vs;
}

IPixelShader* ID11RenderContext::CreatePixelShader(const IShaderDesc* desc)
{
	auto ps = new ID11PixelShader();
	if (ps->Init(this, desc) == false)
	{
		ps->Release();
		return nullptr;
	}
	/*if (desc->Reflector != nullptr)
	{
		
	}*/
	ReflectShader((IShaderDesc*)desc);
	return ps;
}

IComputeShader* ID11RenderContext::CreateComputeShader(const IShaderDesc* desc)
{
	auto cs = new ID11ComputeShader();
	if (cs->Init(this, desc) == false)
	{
		cs->Release();
		return nullptr;
	}
	if (desc->Reflector == nullptr)
	{
		ReflectShader((IShaderDesc*)desc);
	}
	return cs;
}

IInputLayout* ID11RenderContext::CreateInputLayout(const IInputLayoutDesc* desc)
{
	auto layout = new ID11InputLayout();
	if (layout->Init(this, desc) == false)
	{
		layout->Release();
		return nullptr;
	}
	return layout;
}

IConstantBuffer* ID11RenderContext::CreateConstantBuffer(const IConstantBufferDesc* desc)
{
	auto cb = new ID11ConstantBuffer();
	if (cb->Init(this, desc) == false)
	{
		cb->Release();
		return nullptr;
	}
	return cb;
}

IShaderDesc* ID11RenderContext::CompileHLSL(std::string hlsl, LPCSTR entry, LPCSTR sm, const IShaderDefinitions* defines)
{
	std::vector<D3D10_SHADER_MACRO> d11macros;
	if (defines!=nullptr)
	{
		for (auto i : defines->Definitions)
		{
			D3D10_SHADER_MACRO tmp;
			tmp.Name = i.Name.c_str();
			tmp.Definition = i.Definition.c_str();
			d11macros.push_back(tmp);
		}
	}
	DWORD dwShaderFlags = D3DCOMPILE_ENABLE_STRICTNESS;
#if defined( DEBUG ) || defined( _DEBUG )
	dwShaderFlags |= D3DCOMPILE_DEBUG;
#endif
	ID3DBlob* pBlob;
	ID3DBlob* pError;
	//auto hr = D3DX11CompileFromMemory(hlsl.c_str(), hlsl.size(), "", &d11macros[0], nullptr, entry, sm, dwShaderFlags, 0, NULL, &pBlob, &pError, NULL);
	auto hr = D3DCompile(hlsl.c_str(), hlsl.size(), "", &d11macros[0], nullptr, entry, sm, dwShaderFlags, 0, &pBlob, &pError);
	if (FAILED(hr))
	{
		if (pError != NULL)
			RHI_Trace((char*)pError->GetBufferPointer());
		return nullptr;
	}

	auto desc = new IShaderDesc(GetShaderTypeFrom(sm));
	desc->SetCodes((BYTE*)pBlob->GetBufferPointer(), (UINT)pBlob->GetBufferSize());
	//����HLSLcc�����glsl��metal

	ReflectShader(desc);
	return desc;
}
//
//IShaderDesc* ID11RenderContext::CompileHLSLFromFile(LPCSTR file, LPCSTR entry, LPCSTR sm, const IShaderDefinitions* defines)
//{
//	std::vector<D3D10_SHADER_MACRO> d11macros;
//	
//	if (defines != nullptr)
//	{
//		for (auto i : defines->Definitions)
//		{
//			D3D10_SHADER_MACRO tmp;
//			tmp.Name = i.Name.c_str();
//			tmp.Definition = i.Definition.c_str();
//			d11macros.push_back(tmp);
//		}
//	}
//	DWORD dwShaderFlags = D3DCOMPILE_ENABLE_STRICTNESS;
//#if defined( DEBUG ) || defined( _DEBUG )
//	dwShaderFlags |= D3DCOMPILE_DEBUG;
//#endif
//	ID3DBlob* pBlob;
//	ID3DBlob* pError;
//	D3D10_SHADER_MACRO* pMacros = nullptr;
//	if (d11macros.size() > 0)
//		pMacros = &d11macros[0];
//	auto hr = D3DX11CompileFromFileA(file, pMacros, nullptr, entry, sm, dwShaderFlags, 0, NULL, &pBlob, &pError, NULL);
//	if (FAILED(hr))
//	{
//		if (pError != NULL)
//		{
//			RHI_Trace((char*)pError->GetBufferPointer());
//			pError->Release();
//		}
//		return nullptr;
//	}
//
//	auto desc = new IShaderDesc(GetShaderTypeFrom(sm));
//	desc->SetCodes((BYTE*)pBlob->GetBufferPointer(), (UINT)pBlob->GetBufferSize());
//	//����HLSLcc�����glsl��metal
//
//	ReflectShader(desc);
//	return desc;
//}
//
//inline EShaderVarType D11ShaderVarType2DX(D3D_SHADER_VARIABLE_TYPE type, UINT col, UINT row)
//{
//	switch (type)
//	{
//		case D3D_SVT_FLOAT:
//		{
//			if (row==1)
//			{
//				switch (col)
//				{
//				case 1:
//					return SVT_Float1;
//				case 2:
//					return SVT_Float2;
//				case 3:
//					return SVT_Float3;
//				case 4:
//					return SVT_Float4;
//				}
//			}
//			else if (row == 3 && col == 3)
//			{
//				return SVT_Matrix3x3;
//			}
//			else if (row == 4 && col == 4)
//			{
//				return SVT_Matrix4x4;
//			}
//			break;
//		}
//		case D3D_SVT_INT:
//		{
//			switch (col)
//			{
//			case 1:
//				return SVT_Int1;
//			case 2:
//				return SVT_Int2;
//			case 3:
//				return SVT_Int3;
//			case 4:
//				return SVT_Int4;
//			}
//			break;
//		}
//	}
//	return SVT_Unknown;
//}
//
//bool ID11RenderContext::ReflectShader(IShaderDesc* desc)
//{
//	if (desc->Reflector == nullptr)
//	{
//		desc->Reflector = new ShaderReflector();
//	}
//	auto Reflector = desc->Reflector;
//	ID3D11ShaderReflection* pReflection;
//	auto hr = D3DReflect(&desc->GetCodes()[0], desc->GetCodes().size(), IID_ID3D11ShaderReflection, (void**)&pReflection);
//	if (FAILED(hr))
//	{
//		return false;
//	}
//	D3D11_SHADER_DESC shaderDesc;
//	pReflection->GetDesc(&shaderDesc);
//	for (UINT i = 0; i < shaderDesc.ConstantBuffers; i++) 
//	{
//		ID3D11ShaderReflectionConstantBuffer* pCBuffer = pReflection->GetConstantBufferByIndex(i);
//		D3D11_SHADER_BUFFER_DESC Desc;
//		pCBuffer->GetDesc(&Desc);
//
//		D3D11_SHADER_INPUT_BIND_DESC csibDesc;
//		pReflection->GetResourceBindingDescByName(Desc.Name, &csibDesc);
//
//		IConstantBufferDesc tcbDesc;
//		tcbDesc.Name = Desc.Name;
//		tcbDesc.Size = Desc.Size;
//		switch (desc->ShaderType)
//		{
//		case EST_VertexShader:
//			tcbDesc.VSBindPoint = csibDesc.BindPoint;
//			break;
//		case EST_PixelShader:
//			tcbDesc.PSBindPoint = csibDesc.BindPoint;
//			break;
//		}
//		tcbDesc.BindCount = csibDesc.BindCount;
//		
//		for (UINT j = 0; j < Desc.Variables; j++)
//		{
//			ID3D11ShaderReflectionVariable* pVari = pCBuffer->GetVariableByIndex(j);
//			D3D11_SHADER_VARIABLE_DESC varDesc;
//			pVari->GetDesc(&varDesc);
//			/*if ((varDesc.uFlags & D3D10_SVF_USED) == 0)
//			{
//				continue;
//			}*/
//
//			D3D11_SHADER_TYPE_DESC stDesc;
//			pVari->GetType()->GetDesc(&stDesc);
//
//			ConstantVarDesc tcvDesc;
//			tcvDesc.Name = varDesc.Name;
//			tcvDesc.Type = D11ShaderVarType2DX(stDesc.Type, stDesc.Columns, stDesc.Rows);
//			tcvDesc.Offset = varDesc.StartOffset;
//			tcvDesc.Size = varDesc.Size;
//			if (stDesc.Elements == 0)
//				tcvDesc.Elements = 1;
//			else
//				tcvDesc.Elements = stDesc.Elements;
//
//			tcbDesc.Vars.push_back(tcvDesc);
//		}
//
//		Reflector->CBuffers.push_back(tcbDesc);
//	}
//
//	for (UINT i = 0; i < shaderDesc.InputParameters; i++)
//	{
//		D3D11_SIGNATURE_PARAMETER_DESC desc;
//		pReflection->GetInputParameterDesc(i, &desc);
//		pReflection->GetInputParameterDesc(i, &desc);
//	}
//
//	for (UINT i = 0; i < shaderDesc.BoundResources; i++)
//	{
//		D3D11_SHADER_INPUT_BIND_DESC csibDesc;
//		pReflection->GetResourceBindingDesc(i, &csibDesc);
//		ID3D11ShaderReflectionVariable* pVari = pReflection->GetVariableByName(csibDesc.Name);
//		D3D11_SHADER_VARIABLE_DESC varDesc;
//		auto hr = pVari->GetDesc(&varDesc);
//		if (SUCCEEDED(hr))
//		{
//			if ((varDesc.uFlags & D3D10_SVF_USED) == 0)
//			{
//				continue;
//			}
//		}
//		if (csibDesc.Type == D3D_SIT_TEXTURE)
//		{
//			/*D3D11_SHADER_TYPE_DESC stDesc;
//			pVari->GetType()->GetDesc(&stDesc);*/
//
//			TextureBindInfo tmp;
//			tmp.Name = csibDesc.Name;
//			switch (desc->ShaderType)
//			{
//			case EST_VertexShader:
//				tmp.VSBindPoint = csibDesc.BindPoint;
//				break;
//			case EST_PixelShader:
//				tmp.PSBindPoint = csibDesc.BindPoint;
//				break;
//			}
//			
//			tmp.BindCount = csibDesc.BindCount;
//			Reflector->Textures.push_back(tmp);
//		}
//		else if (csibDesc.Type == D3D_SIT_SAMPLER)
//		{
//			SamplerBindInfo tmp;
//			tmp.Name = csibDesc.Name;
//			switch (desc->ShaderType)
//			{
//			case EST_VertexShader:
//				tmp.VSBindPoint = csibDesc.BindPoint;
//				break;
//			case EST_PixelShader:
//				tmp.PSBindPoint = csibDesc.BindPoint;
//				break;
//			}
//			tmp.BindCount = csibDesc.BindCount;
//			Reflector->Samplers.push_back(tmp);
//		}
//	}
//	
//	return HLSLcc(desc);
//}
//
////void CompileVulkanSpirV(const char* exe, const char* glsl, std::vector<UINT>& bc)
////{
////	char cmd[] = "-V shader.tmp";
////	STARTUPINFOA si;
////	PROCESS_INFORMATION pi;
////	ZeroMemory(&si, sizeof(si));
////	si.cb = sizeof(si);
////	ZeroMemory(&pi, sizeof(pi));
////	auto ret = ::CreateProcessA(exe, cmd, NULL, NULL, FALSE, 0/*CREATE_NEW_CONSOLE*/, NULL, NULL, &si, &pi);
////	if (ret)
////	{
////		CloseHandle(pi.hThread);
////		CloseHandle(pi.hProcess);
////	}
////}
//
//HLSLccReflection cb;
//
//bool ID11RenderContext::HLSLcc(IShaderDesc* desc)
//{
//	GlExtensions ext;
//	ext.ARB_explicit_attrib_location = 0;
//	ext.ARB_explicit_uniform_location = 0;
//	ext.ARB_shading_language_420pack = 0;
//	GLSLCrossDependencyData cdd;
//	HLSLccSamplerPrecisionInfo spi;
//
//	GLSLShader outShader;
//	//unsigned int transFlag = HLSLCC_FLAG_DISABLE_GLOBALS_STRUCT;// HLSLCC_FLAG_DISABLE_GLOBALS_STRUCT;// HLSLCC_FLAG_UNIFORM_BUFFER_OBJECT;
//	unsigned int transFlag = HLSLCC_FLAG_INOUT_SEMANTIC_NAMES |
//		//HLSLCC_FLAG_DISABLE_EXPLICIT_LOCATIONS |
//		//HLSLCC_FLAG_WRAP_UBO |
//		HLSLCC_FLAG_DISABLE_GLOBALS_STRUCT |
//		HLSLCC_FLAG_GLOBAL_CONSTS_NEVER_IN_UBO |
//		//HLSLCC_FLAG_REMOVE_UNUSED_GLOBALS |
//		//HLSLCC_FLAG_TRANSLATE_MATRICES |
//		HLSLCC_FLAG_UNIFORM_BUFFER_OBJECT|
//		HLSLCC_FLAG_GLES31_IMAGE_QUALIFIERS;
//
//	auto result = TranslateHLSLFromMem((const char*)&desc->GetCodes()[0],
//		transFlag,
//		LANG_ES_310,//LANG_METAL, //
//		&ext,
//		&cdd,
//		spi,
//		cb,
//		&outShader);
//	if (result == 0)
//		return false;
//
//	auto pos = outShader.sourceCode.find_first_of("#version 310 es");
//	if (pos != std::string::npos)
//	{
//		outShader.sourceCode.replace(pos, pos + strlen("#version 310 es"), "#version 310 es\nprecision highp float;");
//	}
//	desc->Es300Code = outShader.sourceCode;
//
//	//auto hCompiler = ShConstructCompiler(EShLangVertex, 0);
//	ShaderCC::GlslangHelper glsHelper;
//	auto controls = glsHelper.DeriveOptions(ShaderCC::Source::GLSL, ShaderCC::Semantics::OpenGL, ShaderCC::Target::AST);
//	ShaderCC::GlslangResult glsResult = glsHelper.compileAndLink(EShLangVertex, desc->Es300Code, "VS", controls, 
//		glslang::EShTargetVulkan_1_0, false,
//		EShTexSampTransKeep, false, true);
//
//	std::ostringstream stream;
//	glsHelper.outputResultToStream(&stream, glsResult, controls);
//
//	result = TranslateHLSLFromMem((const char*)&desc->GetCodes()[0],
//		transFlag,
//		LANG_METAL,
//		&ext,
//		&cdd,
//		spi,
//		cb,
//		&outShader);
//	if (result == 0)
//		return false;
//
//	desc->MetalCode = outShader.sourceCode;
//	return true;
//}

ICommandList* ID11RenderContext::GetImmCommandList()
{
	return mImmCmdList;
}

NS_END