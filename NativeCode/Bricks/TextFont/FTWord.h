#pragma once

#include "../../../3rd/native/FreeType/include/ft2build.h"
#include "../../../3rd/native/FreeType/include/freetype.h"
#include "../../../3rd/native/FreeType/include/ftglyph.h"
#include "../../Base/IUnknown.h"
#include "../../Math/v3dxQuaternion.h"
#include "../../NextRHI/NxBuffer.h"

#include "../Canvas/FCanvas.h"

NS_BEGIN

struct FTWord;
class FTFont;

struct FTVertex
{
	v3dxQuaternion		PosUV;
};

struct ITextTexture : public Canvas::IImage
{
	virtual NxRHI::ITexture* GetTextureRHI() override
	{
		return RHITexture;
	}
	ITextTexture()
	{
		Dirty = false;
		TextureSize = 0;
		FontSize = 0;
		WordNumber = 0;
	}
	void Init(int size, int fontSize);
	AutoRef<NxRHI::ITexture>	RHITexture;
	int							TextureSize;
	int							FontSize;
	int							CellNum;

	int							HitCount;
	std::vector<FTWord*>		Words;
	bool						Dirty;

	void WriteWord(int x, int y, FTWord* word, BYTE* pPixels);
	int GetEmptyNumber() const {
		return CellNum * CellNum - WordNumber;
	}
	int FindEmptySlot()
	{
		for (int i=0;i<(int)Words.size(); i++)
		{
			if (Words[i] == nullptr)
			{
				return i;
			}
		}
		return -1;
	}
	void SetWord(int index, FTWord* word);
	void UpdateTexture(NxRHI::IGpuDevice* rc, bool bflipV);
private:
	int							WordNumber;
};

struct FTWord : public VIUnknown
{
	FTWord()
	{
		UniCode = 0;
		FontSize = 0;
		HitCount = 0;
		PixelWidth = 0;
		PixelHeight = 0;
		IndexInTexture = 0;
	}
	WCHAR				UniCode;
	int					FontSize;
	std::vector<BYTE>	Pixels;
	int					PixelX;
	int					PixelY;
	int					PixelWidth;
	int					PixelHeight;
	int					AdvanceX;

	AutoRef<ITextTexture>	Texture;
	int					IndexInTexture;

	int					HitCount;

	void FillVertices(int x, int y, ITextTexture* texture, std::vector<FTVertex>& verts, bool flipV);

	FTWord* BuildAsSDF(int w, int h);
};

NS_END