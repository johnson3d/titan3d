#pragma once

#include <stdint.h>

#ifdef IOS
    #include <stddef.h>
#endif

namespace EngineNS
{
	struct IBlobObject;
}

enum XImageFormat
{
	XIF_A8R8G8B8,
	XIF_X8R8G8B8,
	XIF_R5G6B5,
	XIF_PA8R8G8B8,

	XIF_MAX,
};

union XImagePixel
{
	struct
	{
		uint8_t b, g, r, a;
	};
	uint32_t c;
};

enum XImagePixelFormat
{
	XIPF_Unknow,
	XIPF_R8G8B8A8,
	XIPF_R16G16B16A16F,
	XIPF_R32G32B32A32F,
};

struct XImageBuffer
{
	uint8_t* _AllocSharedImageData(size_t nLength);
	void _FreeSharedImageData(uint8_t* pImage);

	XImagePixelFormat	mPixelFormat;
	union
	{
		uint8_t *		m_pImage;				//???????.
		unsigned short*	m_pImageFloat16;
		float*			m_pImageFloat32;
	};

	int				m_nWidth;				
	int				m_nHeight;				
	int				m_nBitCount;			
	int				m_nStride;				
											

	XImagePixel *	m_pPalette;				
	int				m_nPalCount;			

	XImageBuffer();
	~XImageBuffer();

	XImageBuffer(XImageBuffer && _Right);
	XImageBuffer & operator = (XImageBuffer && _Right);

	XImageBuffer(const XImageBuffer & _Right) = delete;
	XImageBuffer & operator = (const XImageBuffer & _Right) = delete;

	XImageBuffer* DownSampler();
	XImageBuffer* BoxDownSampler(int targetWidth, int targetHeight, unsigned int widthMipLevel, unsigned int heightMipLevel);
	XImageBuffer* GenerateMip(unsigned int MipWidth, unsigned int MipHeight);

	XImageBuffer* ConvertToFloatImage();
	void ConvertToRGBA8();

	bool Create(int nWidth, int nHeight, int nBitCount);
	
	bool CopyFrom(const XImageBuffer & _Right);
	void Cleanup();							
	void FlipPixel4();
	void SwapRB();

	void SetGrayPalette();
	void SetPalette(int nStart, int nCount, XImagePixel * pPalette);

	void CalculateMipCount(int * pWidthMipCount, int * pHeightMipCount, int * mipCount, bool * pIsPowerOfTwo) const;

    XImageBuffer* Bit24ToBit32();
	
#if IOS
    XImageBuffer* UpsideDown();
#endif
    
    inline void * GetImage(){ return (void *)m_pImage; }
	inline const void * GetImage() const { return (void *)m_pImage; }

	inline uint8_t GetR(uint8_t * pPixel) { return *pPixel; }
	inline uint8_t GetG(uint8_t * pPixel) { return *(pPixel + 1); }
	inline uint8_t GetB(uint8_t * pPixel) { return *(pPixel + 2); }
	inline uint8_t GetA(uint8_t * pPixel) { return *(pPixel + 3); }

	inline uint8_t * operator [] (intptr_t line){
		return m_pImage + m_nStride * (m_nHeight - 1 - line);
	}
	inline const uint8_t * operator [] (intptr_t line) const{
		return m_pImage + m_nStride * (m_nHeight - 1 - line);
	}
};
