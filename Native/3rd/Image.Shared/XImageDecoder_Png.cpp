#if !defined(EXCLUDE_PNG_SUPPORT)

#include <string.h>
#include <tuple>

#include "XImageBuffer.h"
#include "XImageDecoder.h"
#include "XImageDecoder_Png.h"

extern "C"
{
#include "png/png.h"
#include "png/pngstruct.h"
#include "png/pngpriv.h"
#include "png/pngdebug.h"
}

#define max(a,b) (((a) > (b)) ? (a) : (b))
#define min(a,b) (((a) < (b)) ? (a) : (b))
#define ASSERT(p) 

#ifndef PLATFORM_WIN
extern void OutputDebugStringA(const char* lpOutputString);
#endif

#if !defined(PLATFORM_WIN)
#pragma GCC diagnostic ignored "-Wunused-value"
#pragma GCC diagnostic ignored "-Wunused-variable"
#pragma GCC diagnostic ignored "-Wdangling-else"
#pragma GCC diagnostic ignored "-Wunused-function"
#pragma GCC diagnostic ignored "-Wshift-negative-value"
#endif

//typedef unsigned long DWORD_PTR;

//===================================================================
static void __foo_png_read_data(png_structp png_ptr, png_bytep data, png_size_t length)
{
	memcpy(data, png_ptr->io_ptr, length);
	png_ptr->io_ptr = (uint8_t *)png_ptr->io_ptr + length;
}

static void __foo_png_encoder_read(png_structp png_ptr, png_bytep data, png_size_t length)
{
	((XImageIO*)png_ptr->io_ptr)->Read(data, length);
}
static void __foo_png_encoder_write(png_structp png_ptr, png_bytep data, png_size_t length)
{
	((XImageIO*)png_ptr->io_ptr)->Write(data, length);
}
static void __foo_png_encoder_flush(png_structp png_ptr)
{
	png_ptr;
}

static void __foo_png_error(png_structp png_ptr, png_const_charp message)
{
	png_ptr;
	::OutputDebugStringA(message);
	::OutputDebugStringA("\n");
	//throw (DWORD_PTR)message;
	ASSERT(false);
}
static void __foo_png_warning(png_structp png_ptr, png_const_charp message)
{
	png_ptr;
	message;
}

static XImageFormat vpic_LoadPNG_Memory(XImageBuffer & image, const uint8_t * pStart, intptr_t nSize)
{
	nSize;
	XImageFormat eFormat = XIF_MAX;
	png_struct * png_ptr = NULL;	// png Lib
	png_info * info_ptr = NULL;	// png Lib
	png_bytep* row_pointers = NULL;

	//try
	{
		// 分配PNG结构，并初始化
		png_ptr = png_create_read_struct(PNG_LIBPNG_VER_STRING, (void *)NULL, __foo_png_error, __foo_png_warning);
		if (png_ptr == NULL)
		{
			//throw DWORD_PTR(0);
			goto LineErrror;
		}	

		// 创建PNG信息头
		info_ptr = png_create_info_struct(png_ptr);
		if (info_ptr == NULL)
		{
			//throw DWORD_PTR(0);
			//ASSERT(false);
			goto LineErrror;
		}

		// 自定义读取函数
		png_set_read_fn(png_ptr, (png_voidp)pStart, __foo_png_read_data);
		png_ptr->unknown_default = PNG_HANDLE_CHUNK_ALWAYS;

		// 读png文件信息
		png_read_info(png_ptr, info_ptr);

		png_uint_32 color_type = png_get_color_type(png_ptr, info_ptr);
		if (color_type == PNG_COLOR_TYPE_PALETTE)
			png_set_palette_to_rgb(png_ptr);
		if (color_type == PNG_COLOR_TYPE_GRAY && info_ptr->bit_depth < 8)
			png_set_expand_gray_1_2_4_to_8(png_ptr);
		if (png_get_valid(png_ptr, info_ptr, PNG_INFO_tRNS))
			png_set_tRNS_to_alpha(png_ptr);
		// 16bits/channel == >8bits/channel
		if (info_ptr->bit_depth == 16)
			png_set_strip_16(png_ptr);
		if (color_type == PNG_COLOR_TYPE_GRAY || color_type == PNG_COLOR_TYPE_GRAY_ALPHA)
			png_set_gray_to_rgb(png_ptr);
		// 设置为BGR-order
		//png_set_bgr(png_ptr);
		png_read_update_info(png_ptr, info_ptr);

		png_size_t rowbytes = png_get_rowbytes(png_ptr, info_ptr);
		png_uint_32 channel = (png_uint_32)(rowbytes / info_ptr->width);

		// 创建DIB
		if (!image.Create(info_ptr->width, info_ptr->height, channel * 8))
		{
			//throw DWORD_PTR(0);
			//ASSERT(false);
			goto LineErrror;
		}
		if (channel == 4)
			eFormat = XIF_A8R8G8B8;//(color_type & PNG_COLOR_MASK_ALPHA) ? XIF_A8R8G8B8 : XIF_X8R8G8B8;
		else
			eFormat = XIF_X8R8G8B8;


		// 读取图像数据
		row_pointers = (png_bytep*)malloc(sizeof(png_bytep) * info_ptr->height);
#if defined WIN
		if (row_pointers == NULL)
		{
			::MessageBoxA(NULL, "Out Of Memory:vpic_LoadPNG_Memory", "Error", MB_OK);
		}
#endif
		for (png_uint_32 i = 0; i < info_ptr->height; ++i)
		{
			row_pointers[i] = image[i];
		}
		png_read_image(png_ptr, row_pointers);

		png_read_end(png_ptr, info_ptr);

		free(row_pointers);

		if ((png_ptr != NULL) || (info_ptr != NULL))
			png_destroy_read_struct(&png_ptr, &info_ptr, (png_infopp)NULL);
	}
	//catch (...)
	goto LineOk;
LineErrror:
	{
		eFormat = XIF_MAX;
		if ((png_ptr != NULL) || (info_ptr != NULL))
			png_destroy_read_struct(&png_ptr, &info_ptr, (png_infopp)NULL);
		if (row_pointers != NULL)
			free(row_pointers);
	}
LineOk:

	return eFormat;
}

XIPngDecoder::~XIPngDecoder()
{
}

const char * c_pPNGExtention = ".png";
static const uint8_t __png_signature[8] = { 137, 80, 78, 71, 13, 10, 26, 10 };

bool XIPngDecoder::CheckFileExtention(const char * pszFileName, bool bSaveImage)
{
	if (pszFileName == NULL)
		return false;
	size_t nLength = strlen(pszFileName);
	if (nLength < 4)
		return false;
	return strncmp(pszFileName + nLength - 4, c_pPNGExtention, 4) == 0;
}

bool XIPngDecoder::CheckFileContent(const uint8_t * pMemory, size_t nLength)
{
	if (pMemory == NULL || nLength <= 8)
		return false;
	if (memcmp(pMemory, __png_signature, 8) == 0)
		return true;
	return false;
}

bool XIPngDecoder::LoadImageX(XImageBuffer & xib, const uint8_t * pMemory, size_t nLength)
{
	if (pMemory == NULL)
		return false;
	XImageFormat eFormat;
	try
	{
		eFormat = vpic_LoadPNG_Memory(xib, pMemory, nLength);
	}
	catch(...)
	{
		return false;
	}
	
	if (XIF_MAX != eFormat)
	{
		return true;
	}
	return false;
}

void XIPngDecoder::GetFileExtention(const char * & pszExt)
{
	pszExt = c_pPNGExtention;
}

XDecoderType XIPngDecoder::GetDecoderType() const
{
	return XDecoderType::PNGDecoder;
}

/*
static const int filter_table[OPNG_FILTER_MAX + 1] =
{
PNG_FILTER_VALUE_NONE,   / * -f0 * /
PNG_FILTER_VALUE_SUB,    / * -f1 * /
PNG_FILTER_VALUE_UP,     / * -f2 * /
PNG_FILTER_VALUE_AVG,    / * -f3 * /
PNG_FILTER_VALUE_PAETH,  / * -f4 * /
PNG_FILTER_VALUE_LAST    / * -f5 * /
};
*/
static const int filter_table[OPNG_FILTER_MAX + 1] =
{
	PNG_FILTER_NONE,   /* -f0 */
	PNG_FILTER_SUB,    /* -f1 */
	PNG_FILTER_UP,     /* -f2 */
	PNG_FILTER_AVG,    /* -f3 */
	PNG_FILTER_PAETH,  /* -f4 */
	PNG_ALL_FILTERS    /* -f5 */
};

bool XIPngDecoder::SaveImage(const XImageBuffer & buffer, XImageIO * pIO, XImageEncodeParames * ep)
{
	if (buffer.m_pImage == NULL) return false;
	if (buffer.m_nBitCount <= 0 || ((1U << (buffer.m_nBitCount - 1)) & 0x8080008B) == 0)
		return false;

	XImageFormat eFormat = XIF_MAX;
	png_struct * png_ptr = NULL;	// png Lib
	png_info * info_ptr = NULL;	// png Lib

	//try
	{
		// 分配PNG结构，并初始化
		png_ptr = png_create_write_struct(PNG_LIBPNG_VER_STRING, (void *)NULL, __foo_png_error, __foo_png_warning);
		if (png_ptr == NULL)
		{
			//throw uint32_t(0);
			goto LineError;
		}

		// 创建PNG信息头
		info_ptr = png_create_info_struct(png_ptr);
		if (info_ptr == NULL)
		{
			//throw uint32_t(0);
			goto LineError;
		}

		// 自定义读取函数
		png_set_write_fn(png_ptr, (png_voidp)pIO, __foo_png_encoder_write, __foo_png_encoder_flush);

		const XImagePngEncodeParames * pep = (ep != nullptr) ? (XImagePngEncodeParames *)ep->pvEncodeParames : nullptr;
		png_set_compression_level(png_ptr, pep ? pep->level : OPNG_COMPR_LEVEL_MAX);	//OPNG_COMPR_LEVEL_MIN & OPNG_COMPR_LEVEL_MAX
		png_set_compression_mem_level(png_ptr, pep ? pep->mem_level : OPNG_MEM_LEVEL_MAX);	//OPNG_MEM_LEVEL_MIN & OPNG_MEM_LEVEL_MAX
		png_set_compression_strategy(png_ptr, pep ? pep->strategy : 0);	//OPNG_STRATEGY_MIN & OPNG_STRATEGY_MAX
		png_set_compression_window_bits(png_ptr, OPNG_WINDOW_BIT_MAX);
		int nFilter = pep ? pep->filter : 0;
		nFilter = min(max(nFilter, 0), OPNG_FILTER_MAX);
		png_set_filter(png_ptr, PNG_FILTER_TYPE_BASE, filter_table[nFilter]);	//OPNG_FILTER_MIN & OPNG_FILTER_MAX

		info_ptr->valid = 0;
		info_ptr->width = buffer.m_nWidth;
		info_ptr->height = buffer.m_nHeight < 0 ? -buffer.m_nHeight : buffer.m_nHeight;
		info_ptr->rowbytes = buffer.m_nStride < 0 ? -buffer.m_nStride : buffer.m_nStride;
		if (buffer.m_nBitCount == 32)
		{
			info_ptr->color_type = PNG_COLOR_TYPE_RGB_ALPHA;
			info_ptr->bit_depth = 8;
			info_ptr->channels = 4;
		}
		else if (buffer.m_nBitCount == 24)
		{
			info_ptr->color_type = PNG_COLOR_TYPE_RGB;
			info_ptr->bit_depth = 8;
			info_ptr->channels = 3;
		}
		else if (buffer.m_nBitCount == 8)
		{
			info_ptr->color_type = buffer.m_pPalette ? PNG_COLOR_TYPE_PALETTE : PNG_COLOR_TYPE_GRAY_ALPHA;
			info_ptr->bit_depth = 8;
			info_ptr->channels = 1;
		}
		else if (buffer.m_nBitCount == 4)
		{
			info_ptr->color_type = buffer.m_pPalette ? PNG_COLOR_TYPE_PALETTE : PNG_COLOR_TYPE_GRAY_ALPHA;
			info_ptr->bit_depth = 4;
			info_ptr->channels = 1;
		}
		else if (buffer.m_nBitCount == 2)
		{
			info_ptr->color_type = buffer.m_pPalette ? PNG_COLOR_TYPE_PALETTE : PNG_COLOR_TYPE_GRAY_ALPHA;
			info_ptr->bit_depth = 2;
			info_ptr->channels = 1;
		}
		else if (buffer.m_nBitCount == 1)
		{
			info_ptr->color_type = buffer.m_pPalette ? PNG_COLOR_TYPE_PALETTE : PNG_COLOR_TYPE_GRAY_ALPHA;
			info_ptr->bit_depth = 1;
			info_ptr->channels = 1;
		}
		else
		{
			//throw uint32_t(0);
			//ASSERT(false);
			goto LineError;
		}

		if (info_ptr->color_type & PNG_COLOR_MASK_PALETTE)
		{
			info_ptr->valid |= PNG_INFO_PLTE;

			if (buffer.m_nPalCount > 0)
				info_ptr->num_palette = buffer.m_nPalCount;
			else
				info_ptr->num_palette = png_uint_16(1U << buffer.m_nBitCount);

			XImagePixel * rgba = buffer.m_pPalette;

			info_ptr->palette = new png_color[info_ptr->num_palette];
			for (png_uint_16 i = 0; i < info_ptr->num_palette; ++i)
			{
				info_ptr->palette[i].red = rgba[i].b;
				info_ptr->palette[i].green = rgba[i].g;
				info_ptr->palette[i].blue = rgba[i].r;
			}
		}

		//png_set_bgr(png_ptr);
		png_write_info(png_ptr, info_ptr);

		png_bytepp rows = new png_bytep[info_ptr->height];
		for (png_uint_32 i = 0; i < info_ptr->height; ++i)
		{
			rows[i] = (png_bytep)buffer[i];
		}
		png_write_image(png_ptr, rows);
		png_write_end(png_ptr, info_ptr);
		png_write_flush(png_ptr);

		delete[] rows;
		if (info_ptr->palette)
		{
			delete[] info_ptr->palette;
			info_ptr->palette = NULL;
		}

		png_destroy_write_struct(&png_ptr, &info_ptr);

		return true;
	}
	//catch (...)
	goto LineOk;
LineError:
	{
		if ((png_ptr != NULL) || (info_ptr != NULL))
			png_destroy_write_struct(&png_ptr, &info_ptr);
	}
LineOk:

	return false;
}

#endif
