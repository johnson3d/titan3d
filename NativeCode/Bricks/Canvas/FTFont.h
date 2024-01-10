#pragma once

#include "FTWord.h"
#include "../../NextRHI/NxGeomMesh.h"

NS_BEGIN

namespace Canvas
{
	class FTFontManager;
	class TR_CLASS()
		FTFont : public IWeakReference
	{
	public:
		FTWord* GetWord(int x, int y, UINT c, UInt16 transformIndex, Canvas::FCanvasVertex vert[4]) const;
		v3dxVector2 GetTextSize(const WCHAR * text) const
		{
			v3dxVector2 result(0, 0);
			for (const WCHAR* c = text; c[0] != '\0'; c++)
			{
				auto word = GetWord(0, 0, c[0], 0, nullptr);
				result.x += word->Advance.x;
				if (word->TexY + word->PixelHeight > result.y)
					result.y = (float)(word->TexY + word->PixelHeight);
			}
			return result;
		}
	public:
		FTFont();
		~FTFont();
		virtual void Cleanup() override;
		bool Init(NxRHI::IGpuDevice * rc, FTFontManager * ftMgr, const char* name, int fontSize, int texSizeX, int texSizeY);
		bool InitForBuildFont(NxRHI::IGpuDevice* rc, FTFontManager* ftMgr, const char* name, int fontSize, 
			int SdfPixelSize, int SdfSpread, int SdfPixelColored);
		bool IsNeedSave() const {
			return NeedSave;
		}
		void SaveFontSDF(const char* name);
		FTWord* GetWord(UINT uniCode);
		void AddWordForBuild(UINT uniCode);
		void Update(NxRHI::IGpuDevice * rc, bool bflipV);
		UINT GetWords(std::vector<FTWord*>&words, const WCHAR * text, UINT numOfChar);
		UINT GetWords(FTWord * *pWords, UINT count, const WCHAR * text, UINT numOfChar);
		
		const char* GetName() const {
			return mName.c_str();
		}
		int GetFontSize() const {
			return mFontSize;
		}
		UINT GetFirstUnicode(UINT * index);
		UINT GetNextUnicode(UINT code, UINT * index);
		UINT GetCharIndex(UINT uniCode);
		void ResetWords();
		void SetDirty() {
			Dirty = true;
		}
	protected:
		std::string							mName;
		int									mFontSize = 64;

		std::string							mSdfSourceFont;
		int									mSdfPixelSize = 1024;
		int									mSdfSpread = 4;
		int									mSdfPixelColored = 127;

		using FtPagedWord = MemAlloc::FPagedObject<AutoRef<FTWord>>;
		struct FWordHolder : public VIUnknown
		{
			UINT Unicode;
			UINT64 Offset;
			AutoRef<FtPagedWord> FtWord;
			inline bool operator < (FWordHolder& rh) const {
				return Unicode < rh.Unicode;
			}
			inline bool operator == (FWordHolder& rh) const {
				return Unicode == rh.Unicode;
			}
			inline bool operator < (UINT rh) const {
				return Unicode < rh;
			}
			inline bool operator == (UINT rh) const {
				return Unicode == rh;
			}
			FtPagedWord* GetWord(FTFont* font);
			FtPagedWord* BuildWord(FTFont* font, bool bOnlyBuildFont);
			static void Load(XndAttribute* attr, FTWord* word, UINT Unicode);
			static void SaveTo(XndAttribute* attr, FTWord* word, UINT Unicode);
		};
		std::vector<AutoRef<FWordHolder>> mWordTable;

		bool								NeedSave = false;
		bool								Dirty = true;
		AutoRef<FTPagedWordAllocator>		mFTWordAllocator;
		
		AutoRef<XndHolder>					mSdfXnd;
		AutoRef<XndAttribute>				mWordBitmapAttr;
		
		FT_Face								mFtFace;
		FT_Byte*							mFtContent;
		TWeakRefHandle<FTFontManager>		mManager;
		std::shared_ptr<BYTE>				mMemFtData;

		VSLLock mLocker;
	private:
		FT_Face LoadFtFace(FT_Library ftlib, const char* font);
		bool LoadChar(FT_Library ftlib, WCHAR unicode, int fontSize, int outline_type, int outline_thickness, FTWord* word);
	};

	class TR_CLASS()
		FTFontManager : public IWeakReference
	{
	public:
		ENGINE_RTTI(FTFontManager)
			FTFontManager();
		~FTFontManager();
		bool Init();
		virtual void Cleanup() override;

		FTFont* GetFont(NxRHI::IGpuDevice* device, const char* file, int fontSize, int texSizeX, int texSizeY);
		void Update(NxRHI::IGpuDevice* device, bool bflipV);
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
		std::map<FontKey, AutoRef<FTFont>, FontKey::FontKeyLess>	mFonts;
	};
}

NS_END