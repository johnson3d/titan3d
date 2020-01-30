#pragma once

#include "../../Graphics/GfxPreHead.h"

NS_BEGIN

class ImageImport : public VIUnknown
{
private:
	int w;
	int h;
	int channels;

	//AutoRef<IBlobObject> mImageBlob;
public:
	RTTI_DEF(ImageImport, 0xb13bafda5c6e5fd9, true);
	ImageImport() : w(0), h(0), channels(0){}
	void LoadTexture(const char* name, IBlobObject* blob);
	
	inline int GetWidth() { return w; }
	inline int GetHeight() { return h; }
	inline int GetChannels() { return channels; }
	//inline unsigned char* GetData() { return data; }
};

NS_END