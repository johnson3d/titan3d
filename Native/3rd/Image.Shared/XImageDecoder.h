#pragma once

#if !defined(EXCLUDE_DECODER_SUPPORT)

struct XImageBuffer;

#if !defined(DEF_XIMAGE_IO)
#define DEF_XIMAGE_IO
struct XImageIO
{
	XImageIO();
	virtual ~XImageIO();

	virtual bool Close() = 0;
	virtual bool EOS() = 0;
	virtual bool Read(void* pv, size_t cb) = 0;
	virtual bool Write(const void* pv, size_t cb) = 0;
	virtual bool SetPos(size_t offset) = 0;
	virtual bool GetPos(size_t & offset) = 0;
};
#endif

struct XImageEncodeParames
{
	int		nQuality;		//0-100. 0表示质量最差， 100表示质量最好
	int		nSizeCustomData;
	void *	pvCustomData;
	void *	pvEncodeParames;
};

enum XDecoderType
{
	UnknownDecoder,
	JPGDecoder,
	PNGDecoder,
	VIMGADecoder,
};

class XImageDecoder
{
public:
	virtual ~XImageDecoder();
	virtual bool CheckFileExtention(const char * pszFileName, bool bSaveImage = false) = 0;
	virtual bool CheckFileContent(const uint8_t * pMemory, size_t nLength) = 0;
	virtual bool LoadImageX(XImageBuffer & image, const uint8_t * pMemory, size_t nLength) = 0;
	virtual bool SaveImage(const XImageBuffer & buffer, XImageIO * pIO, XImageEncodeParames * ep);

	virtual void GetFileExtention(const char * & pszExt) = 0;
	virtual XDecoderType GetDecoderType() const = 0;

	//返回的Decoder不需要删除
	static XImageDecoder * MatchDecoder(const char * pszFileName);
};

#endif
