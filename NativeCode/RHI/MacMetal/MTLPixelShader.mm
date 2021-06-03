#include "MTLPixelShader.h"

#define new VNEW

NS_BEGIN

MtlPixelShader::MtlPixelShader()
{
	m_pFragFunc = nil;
}

MtlPixelShader::~MtlPixelShader()
{
}

bool MtlPixelShader::Init(MtlContext* pCtx, const IShaderDesc* pDesc)
{
	NSError* pError = nil;
	MTLCompileOptions* pCompileOption = [[MTLCompileOptions alloc] init];
	pCompileOption.fastMathEnabled = true;

	NSString* pPSCode = [NSString stringWithFormat : @"%s", pDesc->MetalCode.c_str()];

	id<MTLLibrary> pPSLib = [pCtx->m_pDevice newLibraryWithSource : pPSCode options : pCompileOption error : &pError];
	if (pError != nil)
	{
		NSString* pDescStr = [pError localizedDescription];
		NSString* pReasonStr = [pError localizedFailureReason];
		if (pPSLib == nil)
		{
			AssertRHI(false);
		}
	}

	m_pFragFunc = [pPSLib newFunctionWithName : @"PS_Main"];
	if (m_pFragFunc == nil)
	{
		AssertRHI(false);
	}
	
	[pCompileOption release];
	pCompileOption = nil;
	return true;
}




NS_END