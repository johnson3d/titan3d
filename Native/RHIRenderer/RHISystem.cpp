//#include "RHISystem.h"
//
//#include "../Math/vfxgeometry.h"
//#include "../Core/io/vfxfile.h"
//#include "../Graphics/GfxPreHead.h"
//#include "../CSharpAPI.h"
//
//#define new VNEW
//
//extern unsigned char glewExperimental;
//
//NS_BEGIN
//
//std::string AssetsPath = "E:/Program Files/VulkanSDK/1.1.73.0/Demos/NewRHI/RHIRenderer/Assets/";
//
//struct SimpleVertex
//{
//	v3dxVector3 Pos;
//	v3dxVector2 Tex;
//};
//
//SimpleVertex vertices[] =
//{
//{ v3dxVector3(-1.0f, 1.0f, -1.0f), v3dxVector2(0.0f, 0.0f) },
//{ v3dxVector3(1.0f, 1.0f, -1.0f), v3dxVector2(1.0f, 0.0f) },
//{ v3dxVector3(1.0f, 1.0f, 1.0f), v3dxVector2(1.0f, 1.0f) },
//{ v3dxVector3(-1.0f, 1.0f, 1.0f), v3dxVector2(0.0f, 1.0f) },
//
//{ v3dxVector3(-1.0f, -1.0f, -1.0f), v3dxVector2(0.0f, 0.0f) },
//{ v3dxVector3(1.0f, -1.0f, -1.0f), v3dxVector2(1.0f, 0.0f) },
//{ v3dxVector3(1.0f, -1.0f, 1.0f), v3dxVector2(1.0f, 1.0f) },
//{ v3dxVector3(-1.0f, -1.0f, 1.0f), v3dxVector2(0.0f, 1.0f) },
//
//{ v3dxVector3(-1.0f, -1.0f, 1.0f), v3dxVector2(0.0f, 0.0f) },
//{ v3dxVector3(-1.0f, -1.0f, -1.0f), v3dxVector2(1.0f, 0.0f) },
//{ v3dxVector3(-1.0f, 1.0f, -1.0f), v3dxVector2(1.0f, 1.0f) },
//{ v3dxVector3(-1.0f, 1.0f, 1.0f), v3dxVector2(0.0f, 1.0f) },
//
//{ v3dxVector3(1.0f, -1.0f, 1.0f), v3dxVector2(0.0f, 0.0f) },
//{ v3dxVector3(1.0f, -1.0f, -1.0f), v3dxVector2(1.0f, 0.0f) },
//{ v3dxVector3(1.0f, 1.0f, -1.0f), v3dxVector2(1.0f, 1.0f) },
//{ v3dxVector3(1.0f, 1.0f, 1.0f), v3dxVector2(0.0f, 1.0f) },
//
//{ v3dxVector3(-1.0f, -1.0f, -1.0f), v3dxVector2(0.0f, 0.0f) },
//{ v3dxVector3(1.0f, -1.0f, -1.0f), v3dxVector2(1.0f, 0.0f) },
//{ v3dxVector3(1.0f, 1.0f, -1.0f), v3dxVector2(1.0f, 1.0f) },
//{ v3dxVector3(-1.0f, 1.0f, -1.0f), v3dxVector2(0.0f, 1.0f) },
//
//{ v3dxVector3(-1.0f, -1.0f, 1.0f), v3dxVector2(0.0f, 0.0f) },
//{ v3dxVector3(1.0f, -1.0f, 1.0f), v3dxVector2(1.0f, 0.0f) },
//{ v3dxVector3(1.0f, 1.0f, 1.0f), v3dxVector2(1.0f, 1.0f) },
//{ v3dxVector3(-1.0f, 1.0f, 1.0f), v3dxVector2(0.0f, 1.0f) },
//};
//
//v3dxVector3 vertices_pos[] =
//{
//v3dxVector3(-1.0f, 1.0f, -1.0f),
//v3dxVector3(1.0f, 1.0f, -1.0f),
//v3dxVector3(1.0f, 1.0f, 1.0f),
//v3dxVector3(-1.0f, 1.0f, 1.0f),
//
//v3dxVector3(-1.0f, -1.0f, -1.0f),
//v3dxVector3(1.0f, -1.0f, -1.0f),
//v3dxVector3(1.0f, -1.0f, 1.0f),
//v3dxVector3(-1.0f, -1.0f, 1.0f),
//
//v3dxVector3(-1.0f, -1.0f, 1.0f),
//v3dxVector3(-1.0f, -1.0f, -1.0f),
//v3dxVector3(-1.0f, 1.0f, -1.0f),
//v3dxVector3(-1.0f, 1.0f, 1.0f),
//
//v3dxVector3(1.0f, -1.0f, 1.0f),
//v3dxVector3(1.0f, -1.0f, -1.0f),
//v3dxVector3(1.0f, 1.0f, -1.0f),
//v3dxVector3(1.0f, 1.0f, 1.0f),
//
//v3dxVector3(-1.0f, -1.0f, -1.0f),
//v3dxVector3(1.0f, -1.0f, -1.0f),
//v3dxVector3(1.0f, 1.0f, -1.0f),
//v3dxVector3(-1.0f, 1.0f, -1.0f),
//
//v3dxVector3(-1.0f, -1.0f, 1.0f),
//v3dxVector3(1.0f, -1.0f, 1.0f),
//v3dxVector3(1.0f, 1.0f, 1.0f),
//v3dxVector3(-1.0f, 1.0f, 1.0f)
//};
//
//v3dxVector2 vertices_uv[] =
//{
//{ v3dxVector2(0.0f, 0.0f) },
//{ v3dxVector2(1.0f, 0.0f) },
//{ v3dxVector2(1.0f, 1.0f) },
//{ v3dxVector2(0.0f, 1.0f) },
//
//{ v3dxVector2(0.0f, 0.0f) },
//{ v3dxVector2(1.0f, 0.0f) },
//{ v3dxVector2(1.0f, 1.0f) },
//{ v3dxVector2(0.0f, 1.0f) },
//
//{ v3dxVector2(0.0f, 0.0f) },
//{ v3dxVector2(1.0f, 0.0f) },
//{ v3dxVector2(1.0f, 1.0f) },
//{ v3dxVector2(0.0f, 1.0f) },
//
//{ v3dxVector2(0.0f, 0.0f) },
//{ v3dxVector2(1.0f, 0.0f) },
//{ v3dxVector2(1.0f, 1.0f) },
//{ v3dxVector2(0.0f, 1.0f) },
//
//{ v3dxVector2(0.0f, 0.0f) },
//{ v3dxVector2(1.0f, 0.0f) },
//{ v3dxVector2(1.0f, 1.0f) },
//{ v3dxVector2(0.0f, 1.0f) },
//
//{ v3dxVector2(0.0f, 0.0f) },
//{ v3dxVector2(1.0f, 0.0f) },
//{ v3dxVector2(1.0f, 1.0f) },
//{ v3dxVector2(0.0f, 1.0f) },
//};
//
//WORD indices[] =
//{
//	3,1,0,
//	2,1,3,
//
//	6,4,5,
//	7,4,6,
//
//	11,9,8,
//	10,9,11,
//
//	14,12,13,
//	15,12,14,
//
//	19,17,16,
//	18,17,19,
//
//	22,20,21,
//	23,20,22
//};
//
//struct CBNeverChanges
//{
//	v3dxMatrix4 mView;
//};
//
//struct CBChangeOnResize
//{
//	v3dxMatrix4 mProjection;
//};
//
//struct CBChangesEveryFrame
//{
//	v3dxMatrix4 mWorld;
//	v3dxQuaternion vMeshColor;
//};
//
//
//RHISystem::RHISystem()
//{
//}
//
//
//RHISystem::~RHISystem()
//{
//	Cleanup();
//}
//
//void RHISystem::Cleanup()
//{
//	cb_update_frame.Clear();
//	my_pass.Clear();
//	my_cmd_list.Clear();
//	my_drt.Clear();
//	my_rt.Clear();
//	my_swapChain.Clear();
//	my_rc.Clear();
//}
//
//std::string RHISystem::LoadTextFromFile(const char* file)
//{
//	std::string text;
//	ViseFile io;
//	if (io.Open(file, VFile::modeRead) == FALSE)
//		return "Error File";
//	auto len = io.GetLength();
//	text.resize(len + 1);
//	io.Read(&text[0], len);
//	text[len] = '\0';
//	return text;
//}
//
//extern "C" VFX_API IRenderSystem* SDK_CreateRenderSystem(ERHIType type, const IRenderSystemDesc* desc);
//
//void RHISystem::Init(const ISwapChainDesc& scDesc)
//{
//	static ERHIType RHIType = RHT_D3D11;
////#define USE_D11
////#define USE_GL
//	AssetsPath = __FILE__;
//	AssetsPath = AssetsPath.substr(0, AssetsPath.length() - strlen("RHISystem.cpp"));
//	AssetsPath += "Assets/";
//
//#if defined(PLATFORM_DROID)
//	AssetsPath = "/sdcard/NewRHI/Assets/";
//	glewExperimental = 1;
//#endif
//
//	IRenderSystemDesc rsysDesc;
//	rsysDesc.WindowHandle = scDesc.WindowHandle;
//	AutoRef<IRenderSystem> rs = SDK_CreateRenderSystem(RHIType, &rsysDesc);
//	UINT32 curContext = 0;
//	IRenderContextDesc rcDesc;
//	for (; curContext < rs->GetContextNumber(); curContext++)
//	{
//		rs->GetContextDesc(curContext, &rcDesc);
//	}
//	curContext = 0;
//	rs->GetContextDesc(curContext, &rcDesc);
//
//	AutoRef<IRenderContext> rc = rs->CreateContext(&rcDesc);
//	my_rc = rc;
//
//	AutoRef<ISwapChain> swapChain = rc->CreateSwapChain(&scDesc);
//	my_swapChain = swapChain;
//
//	rc->BindCurrentSwapChain(swapChain);
//
//	IRenderTargetViewDesc rtDesc;
//	IShaderResourceViewDesc srsDesc;
//	srsDesc.Texture = swapChain->GetTexture2D();
//	AutoRef<IShaderResourceView> swapChainTexture = rc->CreateShaderResourceView(&srsDesc);
//	rtDesc.Target = swapChainTexture;
//	AutoRef<IRenderTargetView> rt = rc->CreateRenderTargetView(&rtDesc);
//	IDepthStencilViewDesc drtDesc;
//	drtDesc.Width = scDesc.Width;
//	drtDesc.Height = scDesc.Height;
//	AutoRef<IDepthStencilView> drt = rc->CreateDepthRenderTargetView(&drtDesc);
//	my_rt = rt;
//	my_drt = drt;
//
//	IFrameBuffersDesc frmbDesc;
//	frmbDesc.IsSwapChainBuffer = true;
//	AutoRef<IFrameBuffers> frmb = rc->CreateFrameBuffers(&frmbDesc);
//	frmb->BindRenderTargetView(0, my_rt);
//	frmb->BindDepthStencilView(my_drt);
//	my_frmb = frmb;
//
//	ICommandListDesc clDesc;
//	AutoRef<ICommandList> cmd_list = rc->CreateCommandList(&clDesc);
//	my_cmd_list = cmd_list;
//
//	AutoRef<IViewPort> vp = new IViewPort();
//	vp->Width = (float)scDesc.Width;
//	vp->Height = (float)scDesc.Height;
//	AutoRef<IScissorRect> sr = new IScissorRect();
//	sr->MaxX = scDesc.Width;
//	sr->MaxY = scDesc.Height;
//	
//	IVertexBufferDesc vbDesc;
//	vbDesc.CPUAccess = 0;
//	vbDesc.ByteWidth = sizeof(vertices);
//	vbDesc.Stride = sizeof(SimpleVertex);
//	vbDesc.InitData = vertices;
//	AutoRef<IVertexBuffer> vb = rc->CreateVertexBuffer(&vbDesc);
//	AutoRef<IVertexBuffer> vb_pos;
//	AutoRef<IVertexBuffer> vb_uv;
//	{
//		vbDesc.CPUAccess = 0;
//		vbDesc.ByteWidth = sizeof(vertices_pos);
//		vbDesc.Stride = sizeof(v3dxVector3);
//		vbDesc.InitData = vertices_pos;
//		vb_pos = rc->CreateVertexBuffer(&vbDesc);
//
//		vbDesc.CPUAccess = 0;
//		vbDesc.ByteWidth = sizeof(vertices_uv);
//		vbDesc.Stride = sizeof(v3dxVector2);
//		vbDesc.InitData = vertices_uv;
//		vb_uv = rc->CreateVertexBuffer(&vbDesc);
//	}
//
//	IIndexBufferDesc ibDesc;
//	ibDesc.CPUAccess = 0;
//	ibDesc.ByteWidth = sizeof(indices);
//	ibDesc.InitData = indices;
//	AutoRef<IIndexBuffer> ib = rc->CreateIndexBuffer(&ibDesc);
//
//	AutoRef<IShaderDesc> vsDesc;
//	AutoRef<IShaderDesc> psDesc;
//	switch (RHIType)
//	{
//	case RHT_D3D11:
//		{
//			//auto shaderFile = AssetsPath + "Shaders/Tutorial07.fx";
//			auto shaderFile = AssetsPath + "Shaders/FSBase.shadingenv";
//
//			vsDesc = rc->CompileHLSLFromFile(shaderFile.c_str(), "VS_Main", "vs_4_0", nullptr);
//			psDesc = rc->CompileHLSLFromFile(shaderFile.c_str(), "PS_Main", "ps_4_0", nullptr);
//		}
//		break;
//	case RHT_VULKAN:
//		break;
//	case RHT_OGL:
//		{
//			auto shaderFile = AssetsPath + "Shaders/Test1.vert";
//			vsDesc = new IShaderDesc(EST_VertexShader);
//			vsDesc->Es300Code = LoadTextFromFile(shaderFile.c_str());
//
//			shaderFile = AssetsPath + "Shaders/Test1.frag";
//			psDesc = new IShaderDesc(EST_PixelShader);
//			psDesc->Es300Code = LoadTextFromFile(shaderFile.c_str());
//		}
//		break;
//	}
//
//	AutoRef<IVertexShader> vs = rc->CreateVertexShader(vsDesc);
//	AutoRef<IPixelShader> ps = rc->CreatePixelShader(psDesc);
//	
//	AutoRef<IInputLayoutDesc> layoutDesc = new IInputLayoutDesc();
//	GetEngineVertexLayout(layoutDesc->Layouts);
//	layoutDesc->ShaderDesc = vsDesc;
//	AutoRef<IInputLayout> layout = rc->CreateInputLayout(layoutDesc);
//
//	IRasterizerStateDesc rsDesc;
//	rsDesc.CullMode = CMD_NONE;
//	AutoRef<IRasterizerState> rss = rc->CreateRasterizerState(&rsDesc);
//
//	IDepthStencilStateDesc dssDesc;
//	AutoRef<IDepthStencilState> dss = rc->CreateDepthStencilState(&dssDesc);
//
//	AutoRef<IPass> pass = rc->CreatePass();
//	{
//		pass->BindViewPort(vp);
//		pass->BindScissor(sr);
//		IRenderPipelineDesc rplDesc;
//		AutoRef<IRenderPipeline> rpl = rc->CreateRenderPipeline(&rplDesc);
//		IShaderProgramDesc sdpDesc;
//		AutoRef<IShaderProgram> program = rc->CreateShaderProgram(&sdpDesc);
//		program->BindVertexShader(vs);
//		program->BindPixelShader(ps);
//		program->BindInputLayout(layout);
//		program->LinkShaders();
//		rpl->BindShaderProgram(program);
//		rpl->BindRasterizerState(rss);
//		rpl->BindDepthStencilState(dss);
//		pass->BindPipeline(rpl);
//
//		AutoRef<IGeometryMesh> mesh = rc->CreateGeometryMesh();
//		mesh->BindIndexBuffer(ib);
//		//mesh->BindVertexBuffer(0, vb);
//		mesh->BindVertexBuffer(VST_Position, vb_pos);
//		mesh->BindVertexBuffer(VST_UV, vb_uv);
//		pass->BindGeometry(mesh);
//
//		{
//			int index = program->FindCBuffer("cbPerCamera");
//			AutoRef<IConstantBuffer> cb0 = rc->CreateConstantBuffer(program, index);
//			{
//				v3dxVector3 Eye(0.0f, 3.0f, -6.0f);
//				v3dxVector3 At(0.0f, 1.0f, 0.0f);
//				v3dxVector3 Up(0.0f, 1.0f, 0.0f);
//				CBNeverChanges data;
//				v3dxMatrixLookAtLH(&data.mView, &Eye, &At, &Up);
//				cb0->SetVarValue("View", data.mView, 0);
//			}
//			pass->BindCBufferVS(program->GetCBuffer(index)->VSBindPoint, cb0);
//			index = program->FindCBuffer("cbPerViewport");
//			AutoRef<IConstantBuffer> cb1 = rc->CreateConstantBuffer(program, index);
//			{
//				CBChangeOnResize data;
//				v3dxMatrix4 g_Projection;
//				v3dxMatrix4Perspective(&g_Projection, V_PI / 4, (float)scDesc.Width / (float)scDesc.Height, 0.01f, 100.0f);
//				v3dxTransposeMatrix4(&data.mProjection, &g_Projection);
//				cb1->SetVarValue("Projection", data.mProjection, 0);
//				//data.mProjection =  XMMatrixTranspose(g_Projection);
//				//cb1->UpdateContent(cmd_list, &data, sizeof(data));
//				//cb1->UpdateContent(rc->GetImmCommandList(), &data, sizeof(data));
//			}
//			pass->BindCBufferVS(program->GetCBuffer(index)->VSBindPoint, cb1);
//			index = program->FindCBuffer("cbPerInstance");
//			cb_update_frame = rc->CreateConstantBuffer(program, index);
//			pass->BindCBufferVS(program->GetCBuffer(index)->VSBindPoint, cb_update_frame);
//		}
//
//		{
//			auto index = program->FindCBuffer("cbPerInstance");
//			pass->BindCBufferPS(program->GetCBuffer(index)->PSBindPoint, cb_update_frame);
//		}
//
//		AutoRef<IShaderResources> textures = new IShaderResources();
//		{
//			//auto imageFile = AssetsPath + "Texture/seafloor.dds";
//			auto imageFile = AssetsPath + "Texture/testpic.png";
//			AutoRef<IShaderResourceView> srv = rc->LoadShaderResourceView(imageFile.c_str());
//			for (UINT i = 0; i < program->GetShaderResourceNumber(); i++)
//			{
//				TextureBindInfo info;
//				program->GetShaderResourceBindInfo(i, &info);
//				textures->PSBindTexture(info.PSBindPoint, srv);
//			}
//		}
//		pass->BindShaderResouces(textures);
//
//		AutoRef<IShaderSamplers> samplers = new IShaderSamplers();
//		{
//			ISamplerStateDesc sampDesc;
//			sampDesc.Filter = SPF_MIN_MAG_MIP_POINT;
//			AutoRef<ISamplerState> samp = rc->CreateSamplerState(&sampDesc);
//
//			for (UINT i = 0; i < program->GetSamplerNumber(); i++)
//			{
//				SamplerBindInfo info;
//				program->GetSamplerBindInfo(i, &info);
//				samplers->PSBindSampler(info.PSBindPoint, samp);
//			}
//		}
//		pass->BindShaderSamplers(samplers);
//
//		DrawPrimitiveDesc dpDesc;
//		dpDesc.NumPrimitives = 12;
//		pass->BindDrawPrimitive(&dpDesc);
//
//		pass->BuildPass(cmd_list);
//	}
//	my_pass = pass;
//	
//	//Build CommandList
//	if(0)
//	{
//		cmd_list->BeginCommand();
//
//		cmd_list->SetRenderTargets(my_frmb);
//
//		std::vector<std::pair<BYTE, DWORD>> clrColors;
//		clrColors.push_back(std::make_pair(0, 0xFFFF0000));
//		cmd_list->ClearMRT(&clrColors[0], (int)clrColors.size(), true, 1.0F, true, 0);
//
//		cmd_list->PushPass(pass);
//
//		cmd_list->EndCommand();
//	}
//}
//
//void RHISystem::Tick()
//{
//	VDefferedDeleteManager::GetInstance()->Tick(10);
//	my_rc->BindCurrentSwapChain(my_swapChain);
//	//my_swapChain->BindCurrent();
//
//	AutoRef<ICommandList> cmd;
//	static bool useImmCmdList = false;
//	if (useImmCmdList)
//	{
//		auto cmd1 = my_rc->GetImmCommandList();
//		cmd.StrongRef(cmd1);
//	}
//	else
//	{
//		cmd = my_cmd_list;
//	}
//
//	CBChangesEveryFrame cb;
//	static float t = 0.0f;
//	t += (float)V_PI * 0.0125f;
//	v3dxMatrix4 g_World;
//	v3dxMatrix4RotationY(&g_World, t);
//
//	v3dxTransposeMatrix4(&cb.mWorld, &g_World);
//	cb.vMeshColor.set(0.7f, 0.7f, 0.7f, 1.0f);
//	
//	cb_update_frame->SetVarValuePtr(0, &cb.mWorld, sizeof(v3dxMatrix4), 0);
//	cb_update_frame->SetVarValuePtr(1, &cb.vMeshColor, sizeof(v3dxQuaternion), 0);
//	cb_update_frame->FlushContent(cmd);
//	//cb_update_frame->UpdateContent(cmd, &cb, sizeof(cb));
//	if (true)// useImmCmdList && true)
//	{
//		cmd->BeginCommand();
//		cmd->ClearPasses();
//
//		cmd->SetRenderTargets(my_frmb);
//
//		std::vector<std::pair<BYTE, DWORD>> clrColors;
//		clrColors.push_back(std::make_pair(0, 0xFFFF8080));
//		cmd->ClearMRT(&clrColors[0], 1, true, 1.0F, true, 0);
//
//		cmd->PushPass(my_pass);
//
//		cmd->EndCommand();
//
//		cmd->Execute(my_rc);
//	}
//	else
//	{
//		cmd->Execute(my_rc);
//	}
//
//	my_rc->Present(0, 0);
//}
//
//void RHISystem::OnPause()
//{
//	if(my_swapChain!=nullptr)
//		my_swapChain->OnLost();
//}
//
//vBOOL RHISystem::OnResume(void* window)
//{
//	if (my_swapChain != nullptr)
//	{
//		my_swapChain->mDesc.WindowHandle = window;
//		return my_swapChain->OnRestore(&my_swapChain->mDesc);
//	}
//	return TRUE;
//}
//
//NS_END
//
//using namespace EngineNS;
//extern "C"
//{
//#if defined(PLATFORM_DROID)
//	void* Android_ANWinFromSurface(JNIEnv* jniEnv, jobject surface)
//	{
//		return (void*)ANativeWindow_fromSurface(jniEnv, (jobject)surface);
//	}
//#endif
//	EngineNS::RHISystem rhiSys;
//	void RHISystem_Init(void* window, UINT width, UINT height)
//	{
//		ISwapChainDesc desc;
//		desc.Format = PXF_R8G8B8A8_UINT;
//		desc.Width = width;
//		desc.Height = height;
//		desc.WindowHandle = window;
//		rhiSys.Init(desc);
//	}
//
//	void RHISystem_Tick()
//	{
//		//SDK_RHISystem_Tick(nullptr);
//		//SDK_RHISystem_OnResume(nullptr, nullptr);
//		rhiSys.Tick();
//	}
//
//	void RHISystem_OnPause()
//	{
//		rhiSys.OnPause();
//	}
//
//	vBOOL RHISystem_OnResume(void* window)
//	{
//		return rhiSys.OnResume(window);
//	}
//}
