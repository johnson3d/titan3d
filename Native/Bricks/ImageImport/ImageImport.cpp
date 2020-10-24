#include "ImageImport.h"

#pragma warning(disable:4244)
#pragma warning(disable:4251)

#include "../../3rd/oiio/include/OpenImageIO/imagebuf.h"

#pragma comment(lib, "Half.lib")
#pragma comment(lib, "Iex-2_2.lib")
#pragma comment(lib, "IexMath-2_2.lib")
#pragma comment(lib, "IlmThread-2_2.lib")
#pragma comment(lib, "Imath-2_2.lib")

#pragma comment(lib, "IlmImf-2_2.lib")
#pragma comment(lib, "IlmImfUtil-2_2.lib")

#pragma comment(lib, "avcodec.lib")
#pragma comment(lib, "avutil.lib")
#pragma comment(lib, "postproc.lib")
#pragma comment(lib, "swresample.lib")
#pragma comment(lib, "swscale.lib")
#pragma comment(lib, "avformat.lib")
#pragma comment(lib, "avdevice.lib")
#pragma comment(lib, "avfilter.lib")
//

//
#pragma comment(lib, "OpenImageIO.lib")
#pragma comment(lib, "OpenImageIO_Util.lib")


#define new VNEW 

NS_BEGIN

RTTI_IMPL(EngineNS::ImageImport, EngineNS::VIUnknown);

void ImageImport::LoadTexture(const char* name, IBlobObject* blob)
{ 
	auto uftStr = VStringA_Gbk2Utf8(name);
	auto in = OIIO::ImageInput::open(uftStr.c_str());
	const OIIO::ImageSpec &spec = in->spec();

	w = spec.width;
	h = spec.height;
	channels = spec.nchannels;
	unsigned char* data = new unsigned char[w * h * channels];
	in->read_image(OIIO::TypeDesc::UINT8, &(data[0]));//spec.format

	blob->PushData(data, w * h * channels);

	delete[] data;
	in->close();
	OIIO::ImageInput::destroy(in.release());
	//mImageBlob.StrongRef(blob);
}

NS_END

using namespace EngineNS;

extern "C"
{
	Cpp2CS0(EngineNS, ImageImport, GetWidth);
	Cpp2CS0(EngineNS, ImageImport, GetHeight);
	Cpp2CS0(EngineNS, ImageImport, GetChannels);
	Cpp2CS2(EngineNS, ImageImport, LoadTexture);
}