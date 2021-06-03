#pragma once

class XIJpgDecoder : public XImageDecoder
{
public:
	~XIJpgDecoder();

	bool CheckFileExtention(const char * pszFileName, bool bSaveImage);
	bool CheckFileContent(const uint8_t * pMemory, size_t nLength);
	bool LoadImageX(XImageBuffer & image, const uint8_t * pMemory, size_t nLength);
	bool SaveImage(const XImageBuffer & image, XImageIO * pIO, XImageEncodeParames * ep);

	void GetFileExtention(const char * & pszExt);
	XDecoderType GetDecoderType() const;
};
