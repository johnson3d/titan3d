#if !defined(EXCLUDE_JPG_SUPPORT)

#include <string.h>
#include <algorithm>
#include "XImageBuffer.h"
#include "XImageDecoder.h"
#include "XImageDecoder_Jpg.h"
#include "jpeg-9a/jpeglib.h"

#if !defined(PLATFORM_WIN)
#pragma GCC diagnostic ignored "-Wunused-variable"
#pragma GCC diagnostic ignored "-Wunused-value"
#pragma GCC diagnostic ignored "-Wdangling-else"
#endif

#define ASSERT(p) 

struct VJepgErrorHandle : public jpeg_error_mgr{
	static void ErrorExit(j_common_ptr cinfo);
	static void ErrorEmpty(j_common_ptr cinfo);
	static void EmitMessage(j_common_ptr cinfo, int msg_level){ cinfo; msg_level; }
	static void OutputMessage(j_common_ptr cinfo){ cinfo; }
	static void FormatMessage(j_common_ptr cinfo, char * buffer){ cinfo; buffer; }
	static void ResetErrorMgr(j_common_ptr cinfo);
public:
	VJepgErrorHandle();

	int exist_msg_code;
};
VJepgErrorHandle::VJepgErrorHandle()
{
	error_exit = ErrorExit;
	emit_message = EmitMessage;
	output_message = OutputMessage;
	format_message = FormatMessage;
	reset_error_mgr = ResetErrorMgr;

	trace_level = 0;
	num_warnings = 0;
	msg_code = 0;
	exist_msg_code = 0;

	/* Initialize message table pointers */
	jpeg_message_table = NULL;
	last_jpeg_message = 0;

	addon_message_table = NULL;
	first_addon_message = 0;
	last_addon_message = 0;
}

void VJepgErrorHandle::ErrorEmpty(j_common_ptr cinfo)
{
	((VJepgErrorHandle*)cinfo->err)->exist_msg_code = cinfo->err->msg_code;
}

void VJepgErrorHandle::ErrorExit(j_common_ptr cinfo)
{
	int msg_code = cinfo->err->msg_code;
	//throw int(msg_code);
	ASSERT(false);
}
void VJepgErrorHandle::ResetErrorMgr(j_common_ptr cinfo)
{
	cinfo->err->num_warnings = 0;
	cinfo->err->msg_code = 0;
}

class VJpegReadStream : public jpeg_source_mgr
{
public:
	VJpegReadStream(uint8_t * pStart, intptr_t iFileSize)
	{
		next_input_byte = NULL;
		bytes_in_buffer = 0;							// base class member
		m_pBase = pStart;
		m_iFileSize = iFileSize;

		m_END[0] = (JOCTET)0xFF;
		m_END[1] = (JOCTET)JPEG_EOI;
		fill_input_buffer = FillInputBuffer;
		skip_input_data = SkipInputData;
		resync_to_restart = jpeg_resync_to_restart;	// use default method
		init_source = InitSource;
		term_source = TermSource;						// none use
	}
	static boolean FillInputBuffer(j_decompress_ptr cinfo)
	{
		VJpegReadStream	* pSrc = (VJpegReadStream *)cinfo->src;
		pSrc->next_input_byte = (pSrc->m_iFileSize > 0) ? pSrc->m_pBase : pSrc->m_END;
		pSrc->bytes_in_buffer = (pSrc->m_iFileSize > 0) ? pSrc->m_iFileSize : 2; // fake EOI marker
		for (size_t i = 0; i < pSrc->bytes_in_buffer; ++i)
			if (pSrc->next_input_byte[i] == 0x06)
				break;
		return true;
	}
	static void SkipInputData(j_decompress_ptr cinfo, long num_bytes)
	{
		VJpegReadStream	* pSrc = (VJpegReadStream *)cinfo->src;
		if (num_bytes > 0)
			if (pSrc->bytes_in_buffer > (size_t)num_bytes)
			{
				pSrc->next_input_byte += num_bytes;
				pSrc->bytes_in_buffer -= num_bytes;
			}
			else
			{
				pSrc->next_input_byte = pSrc->m_END;
				pSrc->bytes_in_buffer = 2;
			}
	}
	static void TermSource(j_decompress_ptr cinfo) { cinfo; }
	static void InitSource(j_decompress_ptr cinfo) { cinfo; }
public:
	uint8_t		* m_pBase;
	intptr_t		m_iFileSize;
	uint8_t		m_END[2];
};

inline int __fast_div_255(int x)
{
	return ((x + ((x + 257) >> 8)) >> 8);
}

__inline void __SwapRB(uint8_t * p, uint32_t w, uint32_t c)
{
	uint8_t * e = p + w * c;
	uint8_t t;
	for (; p < e; p += c)
	{
		t = p[0];
		p[0] = p[2];
		p[2] = t;
	}
}
__inline void __CMYK2RGB(uint8_t * p, uint32_t w)
{
	uint8_t * e = p + w * 4;
	for (; p < e; p += 4)
	{
		uint32_t c = p[0];
		uint32_t m = p[1];
		uint32_t y = p[2];
		uint32_t k = p[3];

		p[2] = uint8_t(__fast_div_255(y * k));
		p[1] = uint8_t(__fast_div_255(m * k));
		p[0] = uint8_t(__fast_div_255(c * k));
		p[3] = 255;
	}
}
__inline void __YCbCr2RGB_24(uint8_t * p, uint32_t w)
{
	uint8_t * e = p + w * 3;
	for (; p < e; p += 3)
	{
		float Y = p[0];
		float Cb = p[1];
		float Cr = p[2];

		p[2] = (uint8_t)(Y + 1.40200f * (Cr - 128.0f));
		p[1] = (uint8_t)(Y - 0.34414f * (Cb - 128.0f) - 0.71414f * (Cr - 128.0f));
		p[0] = (uint8_t)(Y + 1.77200f * (Cb - 128.0f));
	}
}

__inline void __RGB2ARGB(uint8_t * pIn, uint32_t w, uint8_t* pOut)
{
	for (uint32_t i = 0; i < w; ++i)
	{
		pOut[i * 4 + 2] = pIn[i * 3 + 0];
		pOut[i * 4 + 1] = pIn[i * 3 + 1];
		pOut[i * 4 + 0] = pIn[i * 3 + 2];
		pOut[i * 4 + 3] = 255;
	}
}

__inline void __BGR2ARGB(uint8_t * pIn, uint32_t w, uint8_t* pOut)
{
	for (uint32_t i = 0; i < w; ++i)
	{
		pOut[i * 4 + 0] = pIn[i * 3 + 0];
		pOut[i * 4 + 1] = pIn[i * 3 + 1];
		pOut[i * 4 + 2] = pIn[i * 3 + 2];
		pOut[i * 4 + 3] = 255;
	}
}

__inline void __ARGB2RGB(uint8_t * pIn, uint32_t w, uint8_t* pOut)
{
	for (uint32_t i = 0; i < w; ++i)
	{
		pOut[i * 3 + 2] = pIn[i * 4 + 0];
		pOut[i * 3 + 1] = pIn[i * 4 + 1];
		pOut[i * 3 + 0] = pIn[i * 4 + 2];
	}
}

__inline void __ARGB2BGR(uint8_t * pIn, uint32_t w, uint8_t* pOut)
{
	for (uint32_t i = 0; i < w; ++i)
	{
		pOut[i * 3 + 2] = pIn[i * 4 + 2];
		pOut[i * 3 + 1] = pIn[i * 4 + 1];
		pOut[i * 3 + 0] = pIn[i * 4 + 0];
	}
}

__inline void __CMYK2ARGB(uint8_t * pIn, uint32_t w, uint8_t * pOut)
{
	for (uint32_t i = 0; i < w; ++i)
	{
		uint32_t c = pIn[i * 4 + 0];
		uint32_t m = pIn[i * 4 + 1];
		uint32_t y = pIn[i * 4 + 2];
		uint32_t k = pIn[i * 4 + 3];

		pOut[i * 4 + 2] = uint8_t(__fast_div_255(y * k));
		pOut[i * 4 + 1] = uint8_t(__fast_div_255(m * k));
		pOut[i * 4 + 0] = uint8_t(__fast_div_255(c * k));
		pOut[i * 4 + 3] = 255;
	}
}
__inline void __YCbCr2ARGB(uint8_t * pIn, uint32_t w, uint8_t * pOut)
{
	for (uint32_t i = 0; i < w; ++i)
	{
		float Y = pIn[i * 3 + 0];
		float Cb = pIn[i * 3 + 1];
		float Cr = pIn[i * 3 + 2];


		pOut[i * 4 + 2] = (uint8_t)(Y + 1.40200f * (Cr - 128.0f));
		pOut[i * 4 + 1] = (uint8_t)(Y - 0.34414f * (Cb - 128.0f) - 0.71414f * (Cr - 128.0f));
		pOut[i * 4 + 0] = (uint8_t)(Y + 1.77200f * (Cb - 128.0f));
		pOut[i * 4 + 3] = 255;
	}
}

__inline void __Gray2ARGB(uint8_t * pIn, uint32_t w, uint8_t* pOut)
{
	for (uint32_t i = 0; i < w; ++i)
	{
		pOut[i * 4 + 0] = pIn[i];
		pOut[i * 4 + 1] = pIn[i];
		pOut[i * 4 + 2] = pIn[i];
		pOut[i * 4 + 3] = 255;
	}
}

XIJpgDecoder::~XIJpgDecoder()
{

}

bool XIJpgDecoder::CheckFileExtention(const char * pszFileName, bool bSaveImage)
{
	if (pszFileName == NULL)
		return false;
	size_t nLength = strlen(pszFileName);
	return (nLength >= 4 && strncmp(pszFileName + nLength - 4, ".jpg", 4) == 0) ||
		(nLength >= 5 && strncmp(pszFileName + nLength - 5, ".jpeg", 5) == 0);
}

bool XIJpgDecoder::CheckFileContent(const uint8_t * pMemory, size_t nLength)
{
	if (pMemory == NULL || nLength < 4) return false;
	if (pMemory[0] != 0xff || pMemory[1] != 0xd8) return false;
	//if (pMemory[nLength - 2] != 0xff || pMemory[nLength - 1] != 0xd9) return false;

	jpeg_decompress_struct cinfo;

	VJepgErrorHandle error_handle;
	cinfo.err = &error_handle;
	cinfo.err->error_exit = &VJepgErrorHandle::ErrorEmpty;

	do
	{
		jpeg_create_decompress(&cinfo);

		VJpegReadStream	stream_src((uint8_t *)pMemory, nLength);
		cinfo.src = &stream_src;

		(void)jpeg_read_header(&cinfo, false);
		if (((VJepgErrorHandle*)cinfo.err)->exist_msg_code) break;

		jpeg_destroy_decompress(&cinfo);
		// validate the parameter
		if ((cinfo.image_width == 0) || (cinfo.image_height == 0))
			return false;
		if (cinfo.out_color_space == JCS_UNKNOWN || cinfo.out_color_space == JCS_YCCK)
			return false;
		return true;
	} while (0);

	jpeg_destroy_decompress(&cinfo);
	return false;
}

bool XIJpgDecoder::LoadImageX(XImageBuffer & image, const uint8_t * pMemory, size_t nLength)
{
	jpeg_decompress_struct cinfo;

	VJepgErrorHandle	error_handle;
	cinfo.err = &error_handle;

	try
	{
		jpeg_create_decompress(&cinfo);

		VJpegReadStream	 stream_src((uint8_t *)pMemory, nLength);
		cinfo.src = &stream_src;

		(void)jpeg_read_header(&cinfo, true);
		// validate the parameter
		if ((cinfo.image_width == 0) || (cinfo.image_height == 0) || (cinfo.out_color_space == JCS_UNKNOWN))
		{
			jpeg_destroy_decompress(&cinfo);
			return false;
		}

		// create DIB
		if (!image.Create(cinfo.image_width, cinfo.image_height, 32))
		{
			jpeg_destroy_decompress(&cinfo);
			return  false;
		}

		jpeg_start_decompress(&cinfo);

		// 输出行的步长    
		int row_stride = cinfo.output_width * cinfo.output_components;
		uint8_t* pBuffer = new uint8_t[row_stride];

		switch (cinfo.out_color_space)
		{
		case JCS_GRAYSCALE:
			while (cinfo.output_scanline < cinfo.output_height)
			{
				jpeg_read_scanlines(&cinfo, &pBuffer, 1);
				__Gray2ARGB(pBuffer, cinfo.image_width, image[cinfo.output_scanline - 1]);
			}
			break;
		case JCS_RGB:
			while (cinfo.output_scanline < cinfo.output_height)
			{
				jpeg_read_scanlines(&cinfo, &pBuffer, 1);
				//if(cinfo.jpeg_color_space == JCS_RGB)
				__BGR2ARGB(pBuffer, cinfo.image_width, image[cinfo.output_scanline - 1]);
				//else
				//	__RGB2ARGB(pBuffer,cinfo.image_width,image[cinfo.output_scanline-1]);
			}
			break;
		case JCS_YCbCr:	/* Y/Cb/Cr (also known as YUV) */
			while (cinfo.output_scanline < cinfo.output_height)
			{
				jpeg_read_scanlines(&cinfo, &pBuffer, 1);
				__YCbCr2ARGB(pBuffer, cinfo.image_width, image[cinfo.output_scanline - 1]);
			}
			break;
		case JCS_CMYK:	/* C/M/Y/K */
			while (cinfo.output_scanline < cinfo.output_height)
			{

				jpeg_read_scanlines(&cinfo, &pBuffer, 1);
				__CMYK2ARGB(pBuffer, cinfo.image_width, image[cinfo.output_scanline - 1]);
			}
			break;
		case JCS_YCCK:	/* Y/Cb/Cr/K */
		default:
		{
			jpeg_destroy_decompress(&cinfo);
			return  false;
		}
		break;
		}

		delete[] pBuffer;

		(void)jpeg_finish_decompress(&cinfo);
		jpeg_destroy_decompress(&cinfo);

		return true;
	}
	catch (...)
	{
		/* Let the memory manager delete any temp files before we die */
		image.Cleanup();
		jpeg_destroy_decompress(&cinfo);
		return false;
	}
}

void XIJpgDecoder::GetFileExtention(const char * & pszExt)
{
	pszExt = ".jpg";
}

XDecoderType XIJpgDecoder::GetDecoderType() const
{
	return XDecoderType::JPGDecoder;
}


class VJpegWriteStream : public jpeg_destination_mgr
{
	enum{ BUFF_SIZE = 4096 };
	XImageIO *		outfile;		/* target stream */
	uint8_t *		buffer;			/* start of buffer */

	static boolean EmptyOutputBuffer(j_compress_ptr cinfo);
	static void TermDestination(j_compress_ptr cinfo);
	static void InitDestination(j_compress_ptr){};
public:
	VJpegWriteStream(j_compress_ptr cinfo, XImageIO * file);
	~VJpegWriteStream(){
		delete[] buffer;
	}
};

VJpegWriteStream::VJpegWriteStream(j_compress_ptr cinfo, XImageIO * file)
{
	buffer = new uint8_t[BUFF_SIZE];
	outfile = file;
	init_destination = InitDestination;
	empty_output_buffer = EmptyOutputBuffer;
	term_destination = TermDestination;
	next_output_byte = buffer;
	free_in_buffer = BUFF_SIZE;
	cinfo->dest = this;
}

boolean VJpegWriteStream::EmptyOutputBuffer(j_compress_ptr cinfo)
{
	VJpegWriteStream * desc = (VJpegWriteStream *)cinfo->dest;
	desc->outfile->Write(desc->buffer, BUFF_SIZE);
	desc->next_output_byte = desc->buffer;
	desc->free_in_buffer = BUFF_SIZE;
	return true;
}

void VJpegWriteStream::TermDestination(j_compress_ptr cinfo)
{
	VJpegWriteStream * dest = (VJpegWriteStream *)cinfo->dest;
	size_t datacount = BUFF_SIZE - dest->free_in_buffer;

	if (datacount > 0)
		dest->outfile->Write(dest->buffer, datacount);
}

bool vpic_WriteJPG(const XImageBuffer & image, XImageIO * pIO, XImageEncodeParames * ep)
{
	if (image.m_nBitCount != 24)
		return false;

	struct   jpeg_compress_struct cinfo;
	struct   jpeg_error_mgr       jerr;
	//-- Allocate and Initialize Jpeg Structures -----------------------------
	memset(&cinfo, 0, sizeof(struct jpeg_compress_struct));
	memset(&jerr, 0, sizeof(struct jpeg_error_mgr));

	//-- Initialize the JPEG compression object with default error handling --
	VJepgErrorHandle error_handle;
	cinfo.err = &error_handle;

	try{
		//-- Specify data destination for compression ----------------------------

		//jpeg_create_compress(&cinfo);
		VJpegWriteStream stream(&cinfo, pIO);

		//-- Initialize JPEG parameters ------------------------------------------
		cinfo.in_color_space = JCS_RGB;
		cinfo.image_width = image.m_nWidth;
		cinfo.image_height = image.m_nHeight;
		cinfo.input_components = 3;

		jpeg_set_defaults(&cinfo);
		cinfo.data_precision = 8;
		cinfo.arith_code = 0;
		cinfo.optimize_coding = true;
		cinfo.CCIR601_sampling = 0;
		cinfo.smoothing_factor = 0;

		bool bForceBaseLine = true;
		int nQuality = ep ? ep->nQuality : 80;
		jpeg_set_quality(&cinfo, nQuality, bForceBaseLine);
		jpeg_default_colorspace(&cinfo);

		//-- Start compressor
		jpeg_start_compress(&cinfo, true);

		//-- Process data
#if 1
		JSAMPROW * row_pointer = new JSAMPROW[cinfo.image_height];
		for (JDIMENSION i = 0; i < cinfo.image_height; ++i)
			row_pointer[i] = (uint8_t *)image[i];
		jpeg_write_scanlines(&cinfo, row_pointer, cinfo.image_height);
		delete[] row_pointer;
#else
		JSAMPROW * row_pointer[1];
		while (cinfo.next_scanline < cinfo.image_height)
		{
			row_pointer[0] = (uint8_t *)image[cinfo.next_scanline];
			jpeg_write_scanlines(&cinfo, row_pointer, 1);
		}
#endif

		//-- Finish compression and release memory
		jpeg_finish_compress(&cinfo);
		jpeg_destroy_compress(&cinfo);

		return true;
	}
	catch (...){
		jpeg_destroy_compress(&cinfo);
		return false;
	}
}

bool XIJpgDecoder::SaveImage(const XImageBuffer & xib, XImageIO * pIO, XImageEncodeParames * ep)
{
	if (!xib.m_pImage) return false;
	return vpic_WriteJPG(xib, pIO, ep);
}

#endif
