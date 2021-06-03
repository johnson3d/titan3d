#pragma once
#include "../IRenderSystem.h"
#include "../IRenderContext.h"
#include "GLPreHead.h"

NS_BEGIN

#if defined(PLATFORM_DROID)
struct EGLConfigParms
{
	/** Whether this is a valid configuration or not */
	int validConfig;
	/** The number of bits requested for the red component */
	int redSize;
	/** The number of bits requested for the green component */
	int greenSize;
	/** The number of bits requested for the blue component */
	int blueSize;
	/** The number of bits requested for the alpha component */
	int alphaSize;
	/** The number of bits requested for the depth component */
	int depthSize;
	/** The number of bits requested for the stencil component */
	int stencilSize;
	/** The number of multisample buffers requested */
	int sampleBuffers;
	/** The number of samples requested */
	int sampleSamples;

	EGLConfigParms();
	EGLConfigParms(const EGLConfigParms& Parms);
};
#endif

class IGLRenderSystem : public IRenderSystem
{
public:
	IGLRenderSystem();
	~IGLRenderSystem();

	virtual void Cleanup() override;

	virtual bool Init(const IRenderSystemDesc* desc) override;

	virtual UINT32 GetContextNumber() override {
		return mDeviceNumber;
	}
	virtual vBOOL GetContextDesc(UINT32 index, IRenderContextDesc* desc) override;
	virtual IRenderContext* CreateContext(const IRenderContextDesc* desc) override;
public:
	UINT32							mDeviceNumber;
#if defined(PLATFORM_WIN)
	HGLRC							mContext;
#elif defined(PLATFORM_DROID)
	IRenderContextDesc				mContextDesc;
#endif
};

NS_END