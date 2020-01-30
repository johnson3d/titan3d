#pragma once
#include "../GfxPreHead.h"
#include "../GfxRenderPolicy.h"

NS_BEGIN

enum ERenderLayer
{
	RL_Opaque,
	RL_CustomOpaque,
	RL_Translucent,
	RL_CustomTranslucent,
	//for editor to use;this layer should always be the last layer to send to renderer;
	RL_Gizmos,
	RL_Shadow,
	RL_Sky,

	RL_Num,
};
class GfxSceneRenderLayer;
class GfxSceneView : public VIUnknown
{
public:
	RTTI_DEF(GfxSceneView, 0x51fe91c35b90d1af, true);
	GfxSceneView();
	~GfxSceneView();

	virtual void Cleanup() override;

	vBOOL Init(IRenderContext* rc, UINT width, UINT height);

	/*void ClearMRT(ICommandList* pCmdList, const std::pair<BYTE, DWORD>* ClearColors, int ColorNum,
		bool bClearDepth, float Depth, bool bClearStencil, UINT32 Stencil);*/

	void SetViewport(IViewPort* vp);
	void ResizeViewport(UINT TopLeftX, UINT TopLeftY, UINT width, UINT height);

	void SetFrameBuffers(IFrameBuffers* fb);
	void BindConstBuffer(IConstantBuffer* cbuffer);

	void ClearSpecRenderLayerData(ERenderLayer channel);

	void SendPassToCorrectRenderLayer(ERenderLayer channel, IPass* pass);
	void PushSpecRenderLayerDataToRHI(ICommandList* cmd, ERenderLayer index);

	UINT GetRenderLayerSize() const {
		return (UINT)mSceneRenderLayer.size();
	}

	/*std::vector<GfxSceneRenderLayer*>& GetDrawPipes() {
		return mSceneRenderLayer;
	}*/
	

	auto GetFrameBuffer()
	{
		return mFrameBuffer;
	}

	auto GetViewport()
	{
		return mViewport;
	}

	IConstantBuffer* GetCBuffer() {
		return mConstBuffer;
	}

protected:
	void UpdateConstBufferData();

	AutoRef<IViewPort>									mViewport;
	AutoRef<IConstantBuffer>						mConstBuffer;
	AutoRef<IFrameBuffers>							mFrameBuffer;
	/*std::vector<std::pair<BYTE, DWORD>>   mClearColors;
	bool						mClearDepth;
	float						mDepth;
	bool						mClearStencil;
	int						mStencil;*/
	
	std::vector<GfxSceneRenderLayer*>	mSceneRenderLayer;

	UINT						mViewportPosId;
};

NS_END

