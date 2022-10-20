#pragma once

#include "../BaseHead.h"

NS_BEGIN

#define CRNLIB_PIXEL_FMT_FOURCC(a, b, c, d) ((a) | ((b) << 8U) | ((c) << 16U) | ((d) << 24U))

namespace TextureCompress
{
	enum pixel_format
	{
		PIXEL_FMT_INVALID = 0,

		PIXEL_FMT_DXT1 = CRNLIB_PIXEL_FMT_FOURCC('D', 'X', 'T', '1'),
		PIXEL_FMT_DXT2 = CRNLIB_PIXEL_FMT_FOURCC('D', 'X', 'T', '2'),
		PIXEL_FMT_DXT3 = CRNLIB_PIXEL_FMT_FOURCC('D', 'X', 'T', '3'),
		PIXEL_FMT_DXT4 = CRNLIB_PIXEL_FMT_FOURCC('D', 'X', 'T', '4'),
		PIXEL_FMT_DXT5 = CRNLIB_PIXEL_FMT_FOURCC('D', 'X', 'T', '5'),
		PIXEL_FMT_3DC = CRNLIB_PIXEL_FMT_FOURCC('A', 'T', 'I', '2'), // DXN_YX
		PIXEL_FMT_DXN = CRNLIB_PIXEL_FMT_FOURCC('A', '2', 'X', 'Y'), // DXN_XY
		PIXEL_FMT_DXT5A = CRNLIB_PIXEL_FMT_FOURCC('A', 'T', 'I', '1'), // ATI1N, http://developer.amd.com/media/gpu_assets/Radeon_X1x00_Programming_Guide.pdf

		// Non-standard, crnlib-specific pixel formats (some of these are supported by ATI's Compressonator)
		PIXEL_FMT_DXT5_CCxY = CRNLIB_PIXEL_FMT_FOURCC('C', 'C', 'x', 'Y'),
		PIXEL_FMT_DXT5_xGxR = CRNLIB_PIXEL_FMT_FOURCC('x', 'G', 'x', 'R'),
		PIXEL_FMT_DXT5_xGBR = CRNLIB_PIXEL_FMT_FOURCC('x', 'G', 'B', 'R'),
		PIXEL_FMT_DXT5_AGBR = CRNLIB_PIXEL_FMT_FOURCC('A', 'G', 'B', 'R'),

		PIXEL_FMT_DXT1A = CRNLIB_PIXEL_FMT_FOURCC('D', 'X', '1', 'A'),
		PIXEL_FMT_ETC1 = CRNLIB_PIXEL_FMT_FOURCC('E', 'T', 'C', '1'),

		PIXEL_FMT_R8G8B8 = CRNLIB_PIXEL_FMT_FOURCC('R', 'G', 'B', 'x'),
		PIXEL_FMT_L8 = CRNLIB_PIXEL_FMT_FOURCC('L', 'x', 'x', 'x'),
		PIXEL_FMT_A8 = CRNLIB_PIXEL_FMT_FOURCC('x', 'x', 'x', 'A'),
		PIXEL_FMT_A8L8 = CRNLIB_PIXEL_FMT_FOURCC('L', 'x', 'x', 'A'),
		PIXEL_FMT_A8R8G8B8 = CRNLIB_PIXEL_FMT_FOURCC('R', 'G', 'B', 'A')
	};

	const UINT cDDSMaxImageDimensions = 8192U;

	// Total size of header is sizeof(uint32)+cDDSSizeofDDSurfaceDesc2;
	const UINT cDDSSizeofDDSurfaceDesc2 = 124;

	// "DDS "
	const UINT cDDSFileSignature = 0x20534444;

	struct TR_CLASS(SV_LayoutStruct = 8)
		DDCOLORKEY
	{
		UINT dwUnused0;
		UINT dwUnused1;
	};

	struct TR_CLASS(SV_LayoutStruct = 8)
		DDPIXELFORMAT
	{
		UINT dwSize;
		UINT dwFlags;
		UINT dwFourCC;
		UINT dwRGBBitCount;     // ATI compressonator and crnlib will place a FOURCC code here for swizzled/cooked DXTn formats
		UINT dwRBitMask;
		UINT dwGBitMask;
		UINT dwBBitMask;
		UINT dwRGBAlphaBitMask;
	};

	struct TR_CLASS(SV_LayoutStruct = 8)
		DDSCAPS2
	{
		UINT dwCaps;
		UINT dwCaps2;
		UINT dwCaps3;
		UINT dwCaps4;
	};

	struct TR_CLASS(SV_LayoutStruct = 8)
		DDSURFACEDESC2
	{
		UINT dwSize;
		UINT dwFlags;
		UINT dwHeight;
		UINT dwWidth;
		union
		{
			INT lPitch;
			UINT dwLinearSize;
		};
		UINT dwBackBufferCount;
		UINT dwMipMapCount;
		UINT dwAlphaBitDepth;
		UINT dwUnused0;
		UINT lpSurface;
		DDCOLORKEY unused0;
		DDCOLORKEY unused1;
		DDCOLORKEY unused2;
		DDCOLORKEY unused3;
		DDPIXELFORMAT ddpfPixelFormat;
		DDSCAPS2 ddsCaps;
		UINT dwUnused1;
	};

	const UINT DDSD_CAPS = 0x00000001;
	const UINT DDSD_HEIGHT = 0x00000002;
	const UINT DDSD_WIDTH = 0x00000004;
	const UINT DDSD_PITCH = 0x00000008;

	const UINT DDSD_BACKBUFFERCOUNT = 0x00000020;
	const UINT DDSD_ZBUFFERBITDEPTH = 0x00000040;
	const UINT DDSD_ALPHABITDEPTH = 0x00000080;

	const UINT DDSD_LPSURFACE = 0x00000800;

	const UINT DDSD_PIXELFORMAT = 0x00001000;
	const UINT DDSD_CKDESTOVERLAY = 0x00002000;
	const UINT DDSD_CKDESTBLT = 0x00004000;
	const UINT DDSD_CKSRCOVERLAY = 0x00008000;

	const UINT DDSD_CKSRCBLT = 0x00010000;
	const UINT DDSD_MIPMAPCOUNT = 0x00020000;
	const UINT DDSD_REFRESHRATE = 0x00040000;
	const UINT DDSD_LINEARSIZE = 0x00080000;

	const UINT DDSD_TEXTURESTAGE = 0x00100000;
	const UINT DDSD_FVF = 0x00200000;
	const UINT DDSD_SRCVBHANDLE = 0x00400000;
	const UINT DDSD_DEPTH = 0x00800000;

	const UINT DDSD_ALL = 0x00fff9ee;

	const UINT DDPF_ALPHAPIXELS = 0x00000001;
	const UINT DDPF_ALPHA = 0x00000002;
	const UINT DDPF_FOURCC = 0x00000004;
	const UINT DDPF_PALETTEINDEXED8 = 0x00000020;
	const UINT DDPF_RGB = 0x00000040;
	const UINT DDPF_LUMINANCE = 0x00020000;

	const UINT DDSCAPS_COMPLEX = 0x00000008;
	const UINT DDSCAPS_TEXTURE = 0x00001000;
	const UINT DDSCAPS_MIPMAP = 0x00400000;

	const UINT DDSCAPS2_CUBEMAP = 0x00000200;
	const UINT DDSCAPS2_CUBEMAP_POSITIVEX = 0x00000400;
	const UINT DDSCAPS2_CUBEMAP_NEGATIVEX = 0x00000800;

	const UINT DDSCAPS2_CUBEMAP_POSITIVEY = 0x00001000;
	const UINT DDSCAPS2_CUBEMAP_NEGATIVEY = 0x00002000;
	const UINT DDSCAPS2_CUBEMAP_POSITIVEZ = 0x00004000;
	const UINT DDSCAPS2_CUBEMAP_NEGATIVEZ = 0x00008000;

	const UINT DDSCAPS2_VOLUME = 0x00200000;

} // namespace crnlib

NS_END
