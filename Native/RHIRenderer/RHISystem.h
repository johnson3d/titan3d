#pragma once

#include "IRenderSystem.h"
#include "IRenderContext.h"
#include "ICommandList.h"
#include "IVertexBuffer.h"
#include "IIndexBuffer.h"
#include "IRenderTargetView.h"
#include "IDepthStencilView.h"
#include "IConstantBuffer.h"
#include "IPass.h"
#include "ISwapChain.h"
#include "IShaderResourceView.h"
#include "IInputLayout.h"
#include "ISamplerState.h"
#include "IRasterizerState.h"
#include "IDepthStencilState.h"
#include "IBlendState.h"
#include "IFrameBuffers.h"
#include "ShaderReflector.h"
#include "IShaderProgram.h"
//
//NS_BEGIN
//
//class RHISystem
//{
//public:
//	RHISystem();
//	~RHISystem();
//
//	void Cleanup();
//
//	void Init(const ISwapChainDesc& scDesc);
//	void Tick();
//
//	void OnPause();
//	vBOOL OnResume(void* window);
//
//	std::string LoadTextFromFile(const char* file);
//public:
//	AutoRef<IRenderContext> my_rc;
//	AutoRef<ISwapChain> my_swapChain;
//	AutoRef<IFrameBuffers> my_frmb;
//	AutoRef<IRenderTargetView> my_rt;
//	AutoRef<IDepthStencilView> my_drt;
//	AutoRef<ICommandList> my_cmd_list;
//	AutoRef<IPass> my_pass;
//	AutoRef<IConstantBuffer> cb_update_frame;
//};
//
//NS_END