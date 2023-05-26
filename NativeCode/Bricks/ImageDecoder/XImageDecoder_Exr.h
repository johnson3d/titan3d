#pragma once

NS_BEGIN

class XIExrDecoder : public IImageDecoder
{
public:
	~XIExrDecoder();

	bool CheckFileExtention(const char* pszFileName, bool bSaveImage);
	bool CheckFileContent(const uint8_t* pMemory, size_t nLength);
	bool LoadImageX(XImageBuffer& image, const char* pszFileName);
	bool LoadImageX(XImageBuffer& image, const uint8_t* pMemory, size_t nLength);
	bool SaveImage(const XImageBuffer& image, XImageIO* pIO, XImageEncodeParames* ep);

	void GetFileExtention(const char** pszExt);
	XDecoderType GetDecoderType() const;
};

NS_END