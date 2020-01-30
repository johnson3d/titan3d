#pragma once
#include "../../Graphics/GfxPreHead.h"
#include "../../3rd/EtcLib/Etc.h"
#include "../../3rd/EtcLib/EtcFilter.h"
#include "../../3rd/EtcLib/EtcConfig.h"
#include "../../3rd/EtcLib/EtcImage.h"
#include "../../3rd/EtcLib/EtcColor.h"
#include "../../3rd/EtcLib/EtcErrorMetric.h"

NS_BEGIN

class TexCompressor : public VIUnknown
{
public:
	RTTI_DEF(TexCompressor, 0x7b0df0e55c6a3901, true);
	vBOOL EncodePng2ETC(void* ptr, UINT size, Etc::Image::Format fmt, int mipmap,
					IBlobObject* blob);

	enum EDxtCompressMode
	{
		DCM_NORMAL    = 0,
		DCM_DITHER    = (1<<0),   // use dithering. dubious win. never use for normal maps and the like!
		DCM_HIGHQUAL  = (1<<1),
	};
	vBOOL EncodePng2DXT(void* ptr, UINT size, vBOOL bAlpha, EDxtCompressMode mode, OUT int* mipmap,
		IBlobObject* blob);
};

NS_END