#pragma once
#include "../IRenderResource.h"

class XImageDecoder;

NS_BEGIN

class XndNode;
class XndHolder;
class IShaderResourceView;
struct ITxPicDesc;

//enum EPixelCompressMode
//{
//	PCM_None,
//	PCM_ETC2,
//	PCM_ASTC,
//};
//
//struct SubSourceData
//{
//	int Width;
//	int Height;
//	UINT SysMemPitch;
//	UINT SysMemSlicePitch;
//	std::vector<BYTE> MemData;
//	void Reset() {
//		MemData.clear();
//	}
//};

//class GfxTextureStreaming : public VIUnknown
//{
//public:
//	GfxTextureStreaming();
//	~GfxTextureStreaming();
//	virtual void Cleanup() override;
//	vBOOL Init(IRenderContext* rc, IShaderResourceView* srv, const ITxPicDesc* desc, XndHolder* xnd);
//	void SetStreamMipLevel(int mip);
//	vBOOL LoadAllMips(IRenderContext* rc, IShaderResourceView* srv);
//	vBOOL LoadNextMip(IRenderContext* rc, IShaderResourceView* srv);
//	vBOOL LoadNextMip2(IRenderContext* rc, IShaderResourceView* srv);
//	int GetFullMipCount() {
//		return (int)mMipBuffers.size();
//	}
//	int GetLoadedMipCount() {
//		return mLoadMaxMip;
//	}
//	UINT GetMipMemPitch(int mip);
//	UINT GetMipSlicePitch(int mip);
//	BYTE* GetMipMemData(int mip);
//
//	SubSourceData* GetSubSource(int mip) {
//		if (mip < 0 || mip >= (int)mMipBuffers.size())
//			return nullptr;
//		return &mMipBuffers[mip];
//	}
//
//	virtual void OnMipLoaded(IRenderContext* rc, IShaderResourceView* srv, int mip);
//public:
//	EPixelCompressMode		mCompressMode;
//	EPixelFormat			mFormat;
//	std::vector<SubSourceData>	mMipBuffers;
//	int						mLoadMaxMip;
//
//	AutoRef<XndHolder>		mXnd;
//	AutoRef<XndNode>		mSrcNode;
//	AutoRef<XndNode>		mMipNode;
//
//	static XImageDecoder*	PngDecoder;
//	static EPixelCompressMode	mTryCompressMode;
//};

NS_END