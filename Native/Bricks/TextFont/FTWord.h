#pragma once

#include "../../Graphics/GfxPreHead.h"
#include "../../3rd/FreeType/include/ft2build.h"
#include "../../3rd/FreeType/include/freetype.h"
#include "../../3rd/FreeType/include/ftglyph.h"

NS_BEGIN

struct FTWord;
class FTFont;
struct FTTextDrawContext;

struct FTVertex
{
	v3dxQuaternion		PosUV;
};

struct ITextTexture : public VIUnknown
{
	ITextTexture()
	{
		Dirty = false;
		TextureSize = 0;
		FontSize = 0;
		WordNumber = 0;
	}
	void Init(int size, int fontSize);
	AutoRef<IShaderResourceView>	RHITexture;
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
	void UpdateTexture(IRenderContext* rc, bool bflipV);
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

	void FillVertices(FTTextDrawContext* ctx, int x, int y, ITextTexture* texture, std::vector<FTVertex>& verts, bool flipV);

	FTWord* BuildAsSDF(int w, int h);
};

NS_END