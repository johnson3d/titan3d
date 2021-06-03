#include "GfxTextureStreaming.h"
#include "IShaderResourceView.h"
#include "IRenderContext.h"
#include "../../3rd/native/Image.Shared/XImageDecoder.h"
#include "../../3rd/native/Image.Shared/XImageBuffer.h"

#include "../Base/io/vfxfile.h"
#include "../Base/xnd/vfxxnd.h"

#define new VNEW

NS_BEGIN

//XImageDecoder*	GfxTextureStreaming::PngDecoder = nullptr;
//EPixelCompressMode GfxTextureStreaming::mTryCompressMode = PCM_None;
//
//GfxTextureStreaming::GfxTextureStreaming()
//{
//	mLoadMaxMip = 0;
//	mFormat = PXF_R8G8B8A8_UNORM;
//	mCompressMode = PCM_None;
//}
//
//GfxTextureStreaming::~GfxTextureStreaming()
//{
//	Cleanup();
//}
//
//void GfxTextureStreaming::Cleanup()
//{
//	mLoadMaxMip = 0;
//	mFormat = PXF_R8G8B8A8_UNORM;
//	mCompressMode = PCM_None;
//	mMipBuffers.clear();
//	//mSrcNode.Clear();
//}
//
//vBOOL GfxTextureStreaming::Init(IRenderContext* rc, IShaderResourceView* srv, const ITxPicDesc* txdesc, XndHolder* xnd)
//{
//	Cleanup();
//
//	mXnd.StrongRef(xnd);
//	auto node = mXnd->GetRootNode();
//	if(PngDecoder==nullptr)
//		PngDecoder = XImageDecoder::MatchDecoder("a.png");
//
//	if (rc->GetRHIType() == RHT_D3D11)
//	{
//		auto mipNode = node->TryGetChildNode("PngMips");
//		if (mipNode == nullptr)
//		{
//			mMipNode.StrongRef(mipNode);
//			mSrcNode.StrongRef(node);
//			mCompressMode = PCM_None;
//			SetStreamMipLevel(1);
//			return TRUE;//FALSE
//		}
//		mMipNode.StrongRef(mipNode);
//		mSrcNode.StrongRef(node);
//		mCompressMode = PCM_None;
//		SetStreamMipLevel((int)mipNode->GetNumOfNode());
//	}
//	else
//	{
//		const char* tryCompressFormat = "PngMips";
//		mCompressMode = mTryCompressMode;//PCM_None
//		switch (mCompressMode)
//		{
//			case PCM_None:
//				tryCompressFormat = "PngMips";
//				break;
//			case PCM_ETC2:
//				tryCompressFormat = "EtcMips";
//				break;
//			case PCM_ASTC:
//				tryCompressFormat = "AstcMips";
//				break;
//		}
//		auto mipNode = node->TryGetChildNode(tryCompressFormat);
//		if (mipNode == nullptr)
//		{
//			mipNode = node->TryGetChildNode("PngMips");
//			if (mipNode == nullptr)
//			{
//				VFX_LTRACE(ELTT_Graphics, "txpic don't have PngMips:%s\r\n", mXnd->GetResouce()->Name());
//				return FALSE;
//			}
//			mMipNode.StrongRef(mipNode);
//			mCompressMode = PCM_None;
//		}
//		else
//		{
//			mMipNode.StrongRef(mipNode);
//		}
//		mSrcNode.StrongRef(node);
//
//		SetStreamMipLevel((int)mipNode->GetNumOfNode());
//	}
//
//	return TRUE;
//}
//
//void GfxTextureStreaming::SetStreamMipLevel(int mip)
//{
//	if (mip < 0)
//		return;
//	mLoadMaxMip = 0;
//	mMipBuffers.clear();
//
//	mMipBuffers.resize(mip);
//}
//
//UINT GfxTextureStreaming::GetMipMemPitch(int mip)
//{
//	if (mip < 0 || mip >= (int)mMipBuffers.size())
//		return 0;
//	return mMipBuffers[mip].SysMemPitch;
//}
//
//UINT GfxTextureStreaming::GetMipSlicePitch(int mip)
//{
//	if (mip < 0 || mip >= (int)mMipBuffers.size())
//		return 0;
//	return mMipBuffers[mip].SysMemSlicePitch;
//}
//
//BYTE* GfxTextureStreaming::GetMipMemData(int mip)
//{
//	if (mip < 0 || mip >= (int)mMipBuffers.size())
//		return nullptr;
//	if (mMipBuffers[mip].MemData.size() == 0)
//		return nullptr;
//	return &mMipBuffers[mip].MemData[0];
//}
//
//vBOOL GfxTextureStreaming::LoadAllMips(IRenderContext* rc, IShaderResourceView* srv)
//{
//	ASSERT(false);//deprecated
//	if (mLoadMaxMip >= (int)mMipBuffers.size())
//		return TRUE;
//	for (int i = mLoadMaxMip; i < (int)mMipBuffers.size(); i++)
//	{
//		auto name = VStringA_FormatV("Mip_%d", i);
//		auto attr = mMipNode->TryGetAttribute(name.c_str());
//		if (attr == nullptr)
//			return FALSE;
//
//		attr->BeginRead();
//
//		auto memLength = (UINT)attr->GetReaderLength();
//		AutoPtr<BYTE,true> pBuffer(new BYTE[memLength]);
//		attr->Read(pBuffer, memLength);
//
//		AutoPtr<XImageBuffer> image(new XImageBuffer());
//		auto bLoad = PngDecoder->LoadImageX(*image.GetPtr(), pBuffer, memLength);
//		if (bLoad == false)
//		{
//			attr->EndRead();
//			mXnd->TryReleaseHolder();
//			return FALSE;
//		}
//		mMipBuffers[i].SysMemPitch = image->m_nStride;
//		mMipBuffers[i].SysMemSlicePitch = 0;
//		mMipBuffers[i].Width = image->m_nWidth;
//		mMipBuffers[i].Height = image->m_nHeight;
//		auto memSize = image->m_nStride * image->m_nHeight;
//		mMipBuffers[i].MemData.resize(memSize);
//		memcpy(&mMipBuffers[i].MemData[0], image->m_pImage, memSize);
//		attr->EndRead();
//	}
//	mXnd->TryReleaseHolder();
//
//	mLoadMaxMip = (int)mMipBuffers.size();
//	
//	OnMipLoaded(rc, srv, mLoadMaxMip - 1);
//
//	for (auto i : mMipBuffers)
//	{
//		i.Reset();
//	}
//
//	return TRUE;
//}
//
//vBOOL GfxTextureStreaming::LoadNextMip2(IRenderContext* rc, IShaderResourceView* srv)
//{
//	ASSERT(false);//deprecated
//	if (mLoadMaxMip >= (int)mMipBuffers.size())
//		return TRUE;
//
//	auto name = VStringA_FormatV("PNG", 0);
//	auto attr = mSrcNode->TryGetAttribute(name.c_str());
//	if (attr == nullptr)
//		return FALSE;
//
//	if (mCompressMode == PCM_None)
//	{
//		mLoadMaxMip = 0;
//		attr->BeginRead();
//		auto memLength = (UINT)attr->GetReaderLength();
//		AutoPtr<BYTE, true> pBuffer(new BYTE[memLength]);
//		attr->Read(pBuffer, memLength);
//
//		XImageBuffer* image = new XImageBuffer();
//		auto bLoad = PngDecoder->LoadImageX(*image, pBuffer, memLength);
//		if (bLoad == false)
//		{
//			attr->EndRead();
//			mXnd->TryReleaseHolder();
//			return FALSE;
//		}
//		image->ConvertToRGBA8();
//		image->FlipPixel4();
//		attr->EndRead();
//
//		int widthMipCount;
//		int heightMipCount;
//		int mipCount;
//		bool isPowerOfTwo;
//		image->CalculateMipCount(&widthMipCount, &heightMipCount, &mipCount, &isPowerOfTwo);
//		mMipBuffers.resize(mipCount + 1);
//		
//		int width = image->m_nWidth;
//		int height = image->m_nHeight;
//		for (UINT i = 0; i < (UINT)mipCount + 1; ++i)
//		{
//			mMipBuffers[mipCount - i].SysMemPitch = image->m_nStride;
//			mMipBuffers[mipCount - i].SysMemSlicePitch = 0;
//			mMipBuffers[mipCount - i].Width = image->m_nWidth;
//			mMipBuffers[mipCount - i].Height = image->m_nHeight;
//			auto memSize = image->m_nStride * image->m_nHeight;
//			mMipBuffers[mipCount - i].MemData.resize(memSize);
//			memcpy(&mMipBuffers[mipCount - i].MemData[0], image->m_pImage, memSize);
//
//			if (i == mipCount)
//				break;
//			
//			width /= 2;
//			if (width == 0)
//				width = 1;
//
//			height /= 2;
//			if (height == 0)
//				height = 1;
//			
//			XImageBuffer* mipImage = image->GenerateMip((UINT32)width, (UINT32)height);
//
//			delete image;
//			image = mipImage;
//		}
//		delete image;
//	}
//	
//	mXnd->TryReleaseHolder();
//
//	mLoadMaxMip = (int)mMipBuffers.size();
//	OnMipLoaded(rc, srv, mLoadMaxMip-1);
//
//	if (mLoadMaxMip >= (int)mMipBuffers.size())
//	{
//		for (auto i : mMipBuffers)
//		{
//			i.Reset();
//		}
//	}
//	return TRUE;
//}
//
//vBOOL GfxTextureStreaming::LoadNextMip(IRenderContext* rc, IShaderResourceView* srv)
//{
//	if(mLoadMaxMip >= (int)mMipBuffers.size())
//		return TRUE;
//
//	auto name = VStringA_FormatV("Mip_%d", mMipBuffers.size() - mLoadMaxMip - 1);
//	auto attr = mMipNode->TryGetAttribute(name.c_str());
//	if (attr == nullptr)
//		return FALSE;
//
//	if (mCompressMode == PCM_None)
//	{
//		attr->BeginRead();
//		auto memLength = (UINT)attr->GetReaderLength();
//		AutoPtr<BYTE, true> pBuffer(new BYTE[memLength]);
//		attr->Read(pBuffer, memLength);
//
//		AutoPtr<XImageBuffer> image(new XImageBuffer());
//		auto bLoad = PngDecoder->LoadImageX(*image.GetPtr(), pBuffer, memLength);
//		if (bLoad == false)
//		{
//			attr->EndRead();
//			mXnd->TryReleaseHolder();
//			return FALSE;
//		}
//		image->ConvertToRGBA8();
//		image->FlipPixel4();
//		mMipBuffers[mLoadMaxMip].SysMemPitch = image->m_nStride;
//		mMipBuffers[mLoadMaxMip].SysMemSlicePitch = 0;
//		mMipBuffers[mLoadMaxMip].Width = image->m_nWidth;
//		mMipBuffers[mLoadMaxMip].Height = image->m_nHeight;
//		auto memSize = image->m_nStride * image->m_nHeight;
//		mMipBuffers[mLoadMaxMip].MemData.resize(memSize);
//		memcpy(&mMipBuffers[mLoadMaxMip].MemData[0], image->m_pImage, memSize);
//		attr->EndRead();
//	}
//	else if (mCompressMode == PCM_ETC2)
//	{
//		attr->BeginRead();
//
//		IShaderResourceView::ETCLayer layer;
//		attr->Read(layer);
//		
//		int mipLvl = (int)(mMipBuffers.size() - mLoadMaxMip - 1);
//		mMipBuffers[mLoadMaxMip].Width = vfxMAX(1, srv->mTxDesc.Width >> mipLvl);
//		mMipBuffers[mLoadMaxMip].Height = vfxMAX(1, srv->mTxDesc.Height >> mipLvl);
//		mMipBuffers[mLoadMaxMip].MemData.resize(layer.Size);
//		attr->Read(&mMipBuffers[mLoadMaxMip].MemData[0], layer.Size);
//
//		attr->EndRead();
//	}
//
//	mXnd->TryReleaseHolder();
//
//	OnMipLoaded(rc, srv, mLoadMaxMip);
//	mLoadMaxMip++;
//
//	if (mLoadMaxMip >= (int)mMipBuffers.size())
//	{
//		for (auto i : mMipBuffers)
//		{
//			i.Reset();
//		}
//	}
//	return TRUE;
//}
//
//void GfxTextureStreaming::OnMipLoaded(IRenderContext* rc, IShaderResourceView* srv, int mip)
//{
//
//}

NS_END
