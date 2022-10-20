#pragma once

#include "FTWord.h"
#include "../../NextRHI/NxGeomMesh.h"
#include "../Canvas/FCanvas.h"

NS_BEGIN

class FTFontManager;
class TR_CLASS()
	FTFont : public Canvas::IFont
{
public:
	virtual Canvas::IImage* GetWord(WCHAR c, Canvas::FCanvasVertex vert[4], v3dxVector2 * size) const override;
public:
	FTFont();
	~FTFont();
	virtual void Cleanup() override;
	void Init(NxRHI::IGpuDevice* rc, FTFontManager* ftMgr, const char* name, int fontSize, int cacheTexSize, int texSize);
	FTWord* GetWord(WCHAR code);
	void UpdateHotWords();
	UINT FindBestTexture();
	void GetWords(std::vector<FTWord*>& words, const WCHAR* text);
	void MeasureString(const WCHAR* text, int* width, int* height);
	int CheckPointChar(const WCHAR* text, int x, int y, int* pos=nullptr);
	int CalculateWrap(const WCHAR* text, int* idxArray, int idxArraySize, int widthLimit, int* height);
	int CalculateWrapWithWord(const WCHAR* text, int* idxArray, int idxArraySize, int widthLimit, int* height);

	const char* GetName() const {
		return mName.c_str();
	}
	int GetFontSize() const {
		return mFontSize;
	}
	UINT GetVersion() const {
		return mVersion;
	}
private:
	FT_Face LoadFtFace(FT_Library ftlib, const char* font);
	bool LoadChar(FT_Library ftlib, WCHAR unicode, int outline_type, int outline_thickness, FTWord* word);
protected:
	UINT								mVersion;
	std::string							mName;
	int									mFontSize;
	std::vector<AutoRef<ITextTexture>>	mTextures;
	std::map<WCHAR, FTWord*>			mWords;

	int									mCacheTextureSize;
	int									mTextureSize;
	AutoRef<ITextTexture>				mHotWordTexture;

	FT_Face								mFtFace;
	FT_Byte*							mFtContent;
	TObjectHandle<FTFontManager>		mManager;

	std::shared_ptr<BYTE>				mMemFtData;
};

class TR_CLASS()
	FTFontManager : public VIUnknown
{
public:
	ENGINE_RTTI(FTFontManager)
	FTFontManager();
	~FTFontManager();
	bool Init();
	virtual void Cleanup() override;

	FTFont* GetFont(const char* file, int fontSize, int cacheTexSize, int texSize);
public:
	FT_Library				mFtlib;
	struct FontKey
	{
		std::string			Name;
		int					FontSize;
		struct FontKeyLess
		{
			bool operator()(const FontKey& left, const FontKey& right) const
			{
				auto cmp = strcmp(left.Name.c_str(), right.Name.c_str());
				if (cmp < 0)
					return true;
				else if (cmp > 0)
					return false;
				else
				{
					return left.FontSize < right.FontSize;
				}
			}
		};
	};
	std::map<FontKey, FTFont*, FontKey::FontKeyLess>	mFonts;
};

NS_END