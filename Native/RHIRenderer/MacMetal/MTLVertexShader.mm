#include "MTLVertexShader.h"

#define new VNEW

NS_BEGIN

MtlVertexShader::MtlVertexShader()
{
	m_pVtxFunc = nil;
}

MtlVertexShader::~MtlVertexShader()
{
}

bool MtlVertexShader::Init(MtlContext* pCtx, const IShaderDesc* pDesc)
{
	NSError* pError = nil;
	MTLCompileOptions* pCompileOption = [[MTLCompileOptions alloc] init];
	pCompileOption.fastMathEnabled = true;

	NSString* pVSCode = [NSString stringWithFormat : @"%s", pDesc->MetalCode.c_str()];

	id<MTLLibrary> pVSLib = [pCtx->m_pDevice newLibraryWithSource : pVSCode options : pCompileOption error : &pError];
	if (pError != nil)
	{
		NSString* pDescStr = [pError localizedDescription];
		NSString* pReasonStr = [pError localizedFailureReason];
		if (pVSLib == nil)
		{
			AssertRHI(false);
		}
	}

	m_pVtxFunc = [pVSLib newFunctionWithName : @"VS_Main"];
	if (m_pVtxFunc == nil)
	{
		AssertRHI(false);
	}

	[pCompileOption release];
	pCompileOption = nil;

	return true;
}

NS_END