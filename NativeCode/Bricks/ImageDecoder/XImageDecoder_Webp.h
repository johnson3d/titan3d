#pragma once

NS_BEGIN

class XIWebpDecoder : public XImageDecoder
{
public:
	~XIWebpDecoder();

	bool CheckFileExtention(const char * pszFileName, bool bSaveImage);
	bool CheckFileContent(const uint8_t * pMemory, size_t nLength);
	bool LoadImageX(XImageBuffer & image, const uint8_t * pMemory, size_t nLength);

	void GetFileExtention(const char ** pszExt);
};

NS_END