#pragma once

#include "FTWord.h"

NS_BEGIN

class GfxMesh;
class GfxMaterialInstance;
class GfxMeshPrimitives;
struct FTStringDC : public VIUnknown
{
	FTStringDC()
	{
	}
	AutoRef<ITextTexture>	Texture;
	std::vector<FTVertex>	Vertices;
};

struct FTTextDrawContext : public VIUnknown
{
	RTTI_DEF(FTTextDrawContext, 0x4f48def45b3ef01d, true);
	FTTextDrawContext()
	{
		mVersion = 0;
		ClipX = 0;
		ClipY = 0;
		ClipWidth = 0;
		ClipHeight = 0;
	}
	std::map<ITextTexture*, AutoRef<FTStringDC>>	mDrawCalls;
	
	struct TextString
	{
		std::wstring		Text;
		int					X;
		int					Y;
	};

	int ClipX;
	int ClipY;
	int ClipWidth;
	int ClipHeight;
	UINT							mVersion;
	std::vector<TextString>			mTexts;
	UINT GetAllTextCharNumber() {
		UINT len = 0;
		for (auto i : mTexts)
		{
			len += (UINT)i.Text.length();
		}
		return len;
	}
	UINT GetDrawCall() const {
		return (UINT)mDrawCalls.size();
	}
	void ResetContext();
	GfxMeshPrimitives* BuildMesh(IRenderContext* rc, IShaderResourceView** ppRSV, bool bFlipV);
	vBOOL IsValidVersion(FTFont* font);
	void RebuildContext(FTFont* font);
	void SetClip(int x, int y, int w, int h)
	{
		ClipX = x;
		ClipY = y;
		ClipWidth = w;
		ClipHeight = h;
	}
};

class FTFontManager;
class FTFont : public VIUnknown
{
public:
	FTFont();
	~FTFont();
	virtual void Cleanup() override;
	void Init(IRenderContext* rc, FTFontManager* ftMgr, const char* name, int fontSize, int cacheTexSize, int texSize);
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
	void Draw2TextContext(const WCHAR* text, int x, int y, FTTextDrawContext* drawCtx, bool flipV);
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

class FTFontManager : public VIUnknown
{
public:
	RTTI_DEF(FTFontManager, 0x8a35bb2a5b3e2f1d, true)
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