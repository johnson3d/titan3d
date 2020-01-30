#pragma once
#include "../IShaderResourceView.h"
#include "MTLRenderContext.h"
#include "MTLTexture2D.h"

NS_BEGIN

class MtlShaderResView : public IShaderResourceView
{
public:
	MtlShaderResView();
	~MtlShaderResView();

	virtual void InvalidateResource() override;
	virtual vBOOL RestoreResource() override;

public:
	bool Init(MtlContext* pCtx, const IShaderResourceViewDesc* pDesc);
	bool CreateSRVFromFile(MtlContext* pCtx, const char* FilePathName, bool ImmediateMode);

private:
	MtlContext* m_refMtlCtx;
	std::string mTextureFilePathName;
	MtlTexture2D* m_pTex2dFromFile;
};

NS_END