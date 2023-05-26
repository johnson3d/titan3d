#pragma once

NS_BEGIN

#define OPNG_COMPR_LEVEL_MIN        1
#define OPNG_COMPR_LEVEL_MAX        9

#define OPNG_MEM_LEVEL_MIN          1
#define OPNG_MEM_LEVEL_MAX          9

#define OPNG_STRATEGY_MIN           0
#define OPNG_STRATEGY_MAX           4

#define OPNG_FILTER_MIN             0
#define OPNG_FILTER_MAX             5

#define OPNG_WINDOW_BIT_MIN         8
#define OPNG_WINDOW_BIT_MAX         15

struct XImagePngEncodeParames
{
	int			level;			//1-9
	int			mem_level;		//1-9
	int			strategy;		//1-3
	int			filter;			//0-5
	int			window;			//8-15
};

struct XImageBuffer;

class XIPngDecoder : public IImageDecoder
{
public:
	~XIPngDecoder();

	bool CheckFileExtention(const char * pszFileName, bool bSaveImage);
	bool CheckFileContent(const uint8_t * pMemory, size_t nLength);
	bool LoadImageX(XImageBuffer & image, const uint8_t * pMemory, size_t nLength);
	bool SaveImage(const XImageBuffer & image, XImageIO * pIO, XImageEncodeParames * ep);

	void GetFileExtention(const char ** pszExt);
	XDecoderType GetDecoderType() const;
};

NS_END