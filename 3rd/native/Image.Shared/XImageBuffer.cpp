#include <string.h>
#include <algorithm>
#include "XImageBuffer.h"
#include "../../../NativeCode/Base/float16/float16.h"
#include "../../../NativeCode/Math/v3dxColor4.h"

using namespace EngineNS;

#define new VNEW

uint8_t* XImageBuffer::_AllocSharedImageData(size_t nLength)
{
	auto ret = (uint8_t *)_vfxMemoryNew(nLength + 16, __FILE__, __LINE__);
#if defined WIN
	if (ret == NULL)
	{
		::MessageBoxA(NULL, "Out Of Memory:_AllocSharedImageData", "Error", MB_OK);
	}
#endif
	return ret;
}

void XImageBuffer::_FreeSharedImageData(uint8_t* pImage)
{
	if (pImage != NULL)
		_vfxMemoryDelete(pImage, __FILE__, __LINE__);
}

//--------------------------------------------------------------------------------------------------

XImageBuffer::XImageBuffer()
{
	mPixelFormat = XIPF_Unknow;
	memset(this, 0, sizeof(XImageBuffer));
}

XImageBuffer::~XImageBuffer()
{
	_FreeSharedImageData(m_pImage);
	_FreeSharedImageData((uint8_t*)m_pPalette);
}

XImageBuffer::XImageBuffer(XImageBuffer && _Right)
{
	memcpy(this, &_Right, sizeof(XImageBuffer));
	memset(&_Right, 0, sizeof(XImageBuffer));
}

XImageBuffer & XImageBuffer::operator = (XImageBuffer && _Right)
{
	if (this != &_Right)
	{
		std::swap(this->m_pImage, _Right.m_pImage);
		std::swap(this->m_pPalette, _Right.m_pPalette);
		std::swap(this->m_nWidth, _Right.m_nWidth);
		std::swap(this->m_nHeight, _Right.m_nHeight);
		std::swap(this->m_nBitCount, _Right.m_nBitCount);
		std::swap(this->m_nStride, _Right.m_nStride);
		std::swap(this->m_nPalCount, _Right.m_nPalCount);
	}

	return *this;
}

XImageBuffer* XImageBuffer::Bit24ToBit32()
{
    if (m_nBitCount != 24)
        return NULL;
    
    XImageBuffer* image = new XImageBuffer();
    image->Create(m_nWidth, m_nHeight, 32);
    for (int i = 0; i < m_nHeight; i++)
    {
        auto tar = (*image)[i];
        auto src = (*this)[i];
        for (int j = 0; j < m_nWidth; j++)
        {
            memcpy(tar, src, 3);
            tar[3] = 0xFF;
            tar += 4;
            src += 3;
        }
    }
    return image;
}

#if IOS
XImageBuffer* XImageBuffer::UpsideDown()
{
    //4 bytes per pixel for metal to use;
    XImageBuffer* pImgBuffer = new XImageBuffer();
    pImgBuffer->Create(m_nWidth, m_nHeight, 32);
    for (int h = 0; h < m_nHeight; h++)
    {
        auto refDstRowData = (*pImgBuffer)[h];
        auto refSrcRowData = (*this)[(m_nHeight-1)-h];
        
        memcpy(refDstRowData, refSrcRowData, 4*m_nWidth);
    }
    return pImgBuffer;
}
#endif


XImageBuffer* XImageBuffer::DownSampler()
{
	XImageBuffer* image = new XImageBuffer();

	int hW = m_nWidth / 2;
	if (hW == 0)
		hW = 1;

	int hH = m_nHeight / 2;
	if (hH == 0)
		hH = 1;

	int pixelWidth = m_nBitCount / 8;
	image->Create(hW, hH, m_nBitCount);
	for (int i = 0; i < hH; i++)
	{
		auto tar = (*image)[i];
		auto src = (*this)[i*2];
		for (int j = 0; j < hW; j++)
		{
			memcpy(tar, src, pixelWidth);
			tar += pixelWidth;
			src += (pixelWidth*2);
		}
	}
	return image;
}

int sampler[3][3] = {
	{ 10, 20, 10},
	{ 20, 80, 20},
	{ 10, 20, 10},
};

DWORD GetSamplerStride4(BYTE* pBuffer, int w, int h, int x, int y)
{
	x = x - 1;
	y = y - 1;

	UINT tb = 0;
	UINT tg = 0;
	UINT tr = 0;
	UINT ta = 0;
	UINT value = 0;
	UINT stride = w * 4;
	for (int i = 0; i < 3; i++)
	{
		int sy = y + i;
		if(sy<0 || sy>=h)
			continue;
		for (int j = 0; j < 3; j++)
		{
			int sx = x + j;
			if (sx < 0 || sx >= w)
				continue;
			int start = sy * stride + sx * 4;
			int b = pBuffer[start];
			int g = pBuffer[start + 1];
			int r = pBuffer[start + 2];
			int a = pBuffer[start + 3];

			tb += b * sampler[i][j];
			tg += g * sampler[i][j];
			tr += r * sampler[i][j];
			ta += a * sampler[i][j];
			value += sampler[i][j];
		}
	}
	if (value == 0)
		return 0;
	tb = tb / value;
	tg = tg / value;
	tr = tr / value;
	ta = ta / value;

	DWORD color = (tb & 0xFF) | ((tg & 0xFF) << 8) | ((tr & 0xFF) << 16) | ((ta & 0xFF) << 24);
	return color;
}



DWORD GenerateOnePixel(BYTE* pOriginalBuffer, UINT32 OriginalWidth, UINT32 OriginalHeight, UINT32 PosW, UINT32 PosH)
{
	static const UINT32 Kernel[2][2] = 
	{
		{ 70, 10},
		{ 10, 10},
	};

	UINT Ob = 0;
	UINT Og = 0;
	UINT Or = 0;
	UINT Oa = 0;
	UINT SumKernel = 0;
	UINT stride = OriginalWidth * 4;
	for (UINT32 h = 0; h < 2; h++)
	{
		UINT32 NowH = PosH + h;
		if (NowH > OriginalHeight -1)
			continue;
		for (UINT32 w = 0; w < 2; w++)
		{
			UINT32 NowW = PosW + w;
			if (NowW > OriginalWidth - 1)
				continue;
			UINT32 StartIdx = NowH * stride + NowW * 4;
			UINT32 a = pOriginalBuffer[StartIdx];
			UINT32 b = pOriginalBuffer[StartIdx + 1];
			UINT32 g = pOriginalBuffer[StartIdx + 2];
			UINT32 r = pOriginalBuffer[StartIdx + 3];

			Oa += a * Kernel[h][w];
			Ob += b * Kernel[h][w];
			Og += g * Kernel[h][w];
			Or += r * Kernel[h][w];
			SumKernel += Kernel[h][w];
		}
	}
	if (SumKernel == 0)
		return 0;
	Oa /= SumKernel;
	Ob /= SumKernel;
	Og /= SumKernel;
	Or /= SumKernel;

	DWORD color = (Oa & 0xFF) | ((Ob & 0xFF) << 8) | ((Og & 0xFF) << 16) | ((Or & 0xFF) << 24);
	return color;
}

void XImageBuffer::ConvertToRGBA8()
{
	if (m_nBitCount == 32)
		return;

	int nStride = m_nWidth * 4;
	if (m_nBitCount == 24)
	{
		BYTE* pPixel32 = _AllocSharedImageData(m_nWidth * 4 * m_nHeight);
		BYTE* tar = pPixel32;
		BYTE* src = m_pImage;
		for (int i = 0; i < m_nHeight; i++)
		{
			for (int j = 0; j < m_nWidth; j++)
			{
				tar[j * 4 + 0] = src[j * 3 + 0];
				tar[j * 4 + 1] = src[j * 3 + 1];
				tar[j * 4 + 2] = src[j * 3 + 2];
				tar[j * 4 + 0] = 0xFF;
			}
			tar += nStride;
			src += m_nStride;
		}
		m_nStride = nStride;
		_FreeSharedImageData(m_pImage);
		m_pImage = pPixel32;
		mPixelFormat = XIPF_R8G8B8A8;
		m_nBitCount = 32;
	}
}

XImageBuffer* XImageBuffer::ConvertToFloatImage()
{
	ASSERT(m_nBitCount == 32);

	XImageBuffer* image = new XImageBuffer();
	image->mPixelFormat = XIPF_R32G32B32A32F;
	image->m_pImageFloat32 = new float[m_nWidth * m_nHeight * 4];
	image->m_nWidth = m_nWidth;
	image->m_nHeight = m_nHeight;
	image->m_nStride = m_nWidth * sizeof(float) * 4;
	image->m_nBitCount = sizeof(float) * 4 * 8;

	BYTE* src = m_pImage;
	float* tar = image->m_pImageFloat32;
	for (int i = 0; i < m_nHeight; i++)
	{
		for (int j = 0; j < m_nWidth; j++)
		{
			auto index = j * 4;
			auto b = (float)src[index + 0];
			auto g = (float)src[index + 1];
			auto r = (float)src[index + 2];
			auto a = (float)src[index + 3];

			tar[index + 0] = b / 255.0f;
			tar[index + 1] = g / 255.0f;
			tar[index + 2] = r / 255.0f;
			tar[index + 3] = a / 255.0f;
		}
		src += m_nStride;
		tar += 4 * m_nWidth;
	}
	return image;
}

XImageBuffer* XImageBuffer::BoxDownSampler(int targetWidth, int targetHeight, unsigned int widthMipLevel, unsigned int heightMipLevel)
{
	XImageBuffer* image = new XImageBuffer();
	int hW = targetWidth;
	int hH = targetHeight;
	image->Create(hW, hH, m_nBitCount);

	auto tar = (BYTE*)image->m_pImage;

	float scaleX = (float)m_nWidth / (float)hW;
	float scaleY = (float)m_nHeight / (float)hH;

	for (int i = 0; i < hH; i++)
	{
		for (int j = 0; j < hW; j++)
		{
			DWORD color = GetSamplerStride4(m_pImage, m_nWidth, m_nHeight, (int)((float)j*scaleX), (int)((float)i*scaleY));
			((DWORD*)tar)[j] = color;
		}
		tar += image->m_nStride;
	}

	return image;
}

XImageBuffer* XImageBuffer::GenerateMip(unsigned int MipWidth, unsigned int MipHeight)
{
	XImageBuffer* pMipBuffer = new XImageBuffer();
	pMipBuffer->Create(MipWidth, MipHeight, m_nBitCount);

	auto pMipDataByteForm = (BYTE*)pMipBuffer->m_pImage;

	float ScaleW = (float)m_nWidth / (float)MipWidth;
	float ScaleH = (float)m_nHeight / (float)MipHeight;

	for (unsigned int h = 0; h < MipHeight; h++)
	{
		for (unsigned int w = 0; w < MipWidth; w++)
		{
			DWORD color = GenerateOnePixel(m_pImage, m_nWidth, m_nHeight, (int)((float) w * ScaleW), (int)((float)h * ScaleH));
			((DWORD*)pMipDataByteForm)[w] = color;
		}
		pMipDataByteForm += pMipBuffer->m_nStride;
	}

	return pMipBuffer;
}

//{
//	XImageBuffer* image = new XImageBuffer();
//	int hW = targetWidth;
//	int hH = targetHeight;
//	int pixelWidth = m_nBitCount / 8;
//	image->Create(hW, hH, m_nBitCount);
//
//	int widthStride = 1 << widthMipLevel;
//	int widthStrideMinusOne = widthStride - 1;
//
//	int heightStride = 1 << heightMipLevel;
//	int heightStrideMinusOne = heightStride - 1;
//
//	uint8_t rgba[4];
//
//	for (int i = 0; i < hH; i++)
//	{
//		auto tar = (*image)[i];
//
//		auto srcN = (*this)[i * heightStride];
//		auto srcNPlusStride = (*this)[i * heightStride + heightStrideMinusOne];
//
//		for (int j = 0; j < hW; j++)
//		{
//			rgba[0] = (GetR(srcN) + GetR(srcN + pixelWidth * widthStrideMinusOne) + GetR(srcNPlusStride) + GetR(srcNPlusStride + pixelWidth * widthStrideMinusOne)) / 4;
//			rgba[1] = (GetG(srcN) + GetG(srcN + pixelWidth * widthStrideMinusOne) + GetG(srcNPlusStride) + GetG(srcNPlusStride + pixelWidth * widthStrideMinusOne)) / 4;
//			rgba[2] = (GetB(srcN) + GetB(srcN + pixelWidth * widthStrideMinusOne) + GetB(srcNPlusStride) + GetB(srcNPlusStride + pixelWidth * widthStrideMinusOne)) / 4;
//
//			if (pixelWidth == 4)
//				rgba[3] = (GetA(srcN) + GetA(srcN + pixelWidth * widthStrideMinusOne) + GetA(srcNPlusStride) + GetA(srcNPlusStride + pixelWidth * widthStrideMinusOne)) / 4;
//
//			memcpy(tar, &rgba, pixelWidth);
//
//			tar += pixelWidth;
//
//			srcN += (pixelWidth * widthStride);
//			srcNPlusStride += (pixelWidth * widthStride);
//		}
//	}
//	return image;
//}

bool XImageBuffer::CopyFrom(const XImageBuffer & _Right)
{
	if (!Create(_Right.m_nWidth, _Right.m_nHeight, _Right.m_nBitCount))
		return false;

	int nCount = m_nBitCount / 8 * m_nWidth;
	for (int y = 0; y < m_nHeight; ++y)
		memcpy(this->operator[](y), _Right[y], nCount);
	if (m_pPalette && _Right.m_pPalette)
		memcpy(m_pPalette, _Right.m_pPalette, sizeof(XImagePixel) * _Right.m_nPalCount);

	m_nPalCount = _Right.m_nPalCount;

	return true;
}

bool XImageBuffer::Create(int nWidth, int nHeight, int nBitCount)
{
	Cleanup();

	mPixelFormat = XIPF_R8G8B8A8;
	m_nWidth = nWidth;
	m_nHeight = nHeight < 0 ? -nHeight : nHeight;
	m_nBitCount = nBitCount;
	//m_nStride = (m_nWidth * m_nBitCount + 31) / 32 * 4;
	//m_nStride = m_nWidth * m_nBitCount / 8;
	m_nStride = (m_nWidth * m_nBitCount + 7) / 8;
	m_pImage = _AllocSharedImageData(m_nStride * m_nHeight);
	if (m_pImage == NULL)
		return false;
	if (m_nBitCount <= 8)
	{
		uint32_t nCount = uint32_t(1) << m_nBitCount;
		m_nPalCount = nCount;
		m_pPalette = (XImagePixel *)_AllocSharedImageData(nCount * sizeof(XImagePixel));
		if (m_pPalette == NULL)
		{
			_FreeSharedImageData(m_pImage);
			m_pImage = NULL;
			return false;
		}
	}

	return true;
}

void XImageBuffer::Cleanup(void)
{
	_FreeSharedImageData(m_pImage);
	_FreeSharedImageData((uint8_t*)m_pPalette);
	memset(this, 0, sizeof(XImageBuffer));
}

void XImageBuffer::SetGrayPalette()
{
	if (m_pPalette == NULL) return;
	uint32_t nCount = uint32_t(1) << m_nBitCount;
	for (uint32_t i = 0; i < nCount; ++i)
	{
		m_pPalette[i].c = i | (i << 8) | (i << 16) | 0xff000000;
	}
}

void XImageBuffer::SetPalette(int nStart, int nCount, XImagePixel * pPalette)
{
	if (m_pPalette == NULL || pPalette == NULL) return;
	int nPaletteCount = int(1) << m_nBitCount;
	if (nStart + nCount > nPaletteCount) nCount = nPaletteCount - nStart;
	memcpy(m_pPalette + nStart, pPalette, nCount * sizeof(XImagePixel));
}

void XImageBuffer::CalculateMipCount(int * pWidthMipCount, int * pHeightMipCount, int * mipCount, bool * pIsPowerOfTwo) const
{
	int value = 1;
	int exponentForWidth = 0;

	while ((value << exponentForWidth) <= m_nWidth)
		++exponentForWidth;

	*pWidthMipCount = exponentForWidth - 1;

	int exponentForHeight = 0;

	while ((value << exponentForHeight) <= m_nHeight)
		++exponentForHeight;

	*pHeightMipCount = exponentForHeight - 1;

	*mipCount = *pWidthMipCount > *pHeightMipCount ? *pWidthMipCount : *pHeightMipCount;

	if (value << (exponentForWidth - 1) == m_nWidth&&
		value << (exponentForHeight - 1) == m_nHeight)
	{
		*pIsPowerOfTwo = true;
	}
	else
		*pIsPowerOfTwo = false;
}

void XImageBuffer::FlipPixel4()
{
	const int xCount = m_nStride / 4;

	for (int i = 0, j = m_nHeight - 1; i < j; ++i, --j)
	{
		DWORD * pi = (DWORD *)this->operator[](i);
		DWORD * pj = (DWORD *)this->operator[](j);
		for (int x = 0; x < xCount; ++x)
			std::swap(pi[x], pj[x]);
	}
}

void XImageBuffer::SwapRB()
{
	if (m_pPalette)
	{
		XImagePixel * pend = m_pPalette + m_nPalCount;
		for (XImagePixel * p = m_pPalette; p < pend; ++p)
			std::swap(p->b, p->r);
	}
	if (m_nBitCount == 24)
	{
		for (int y = m_nHeight - 1; y >= 0; --y)
		{
			uint8_t * pixel = this->operator[](y);
			for (int x = m_nWidth - 1; x >= 0; --x)
			{
				std::swap(pixel[0], pixel[2]);
				pixel += 3;
			}
		}
	}
	else if (m_nBitCount == 32)
	{
		for (int y = m_nHeight - 1; y >= 0; --y)
		{
			uint32_t * pixel = (uint32_t *)this->operator[](y);
			for (int x = m_nWidth - 1; x >= 0; --x)
			{
				uint32_t c = *pixel;
				*pixel++ = (c & 0xff00ff00) | ((c & 0x00ff0000) >> 16) | ((c & 0x000000ff) << 16);
			}
		}
	}
}
