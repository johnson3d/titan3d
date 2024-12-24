#pragma once

#include "FTWord.h"
#include "../../NextRHI/NxGeomMesh.h"

NS_BEGIN

namespace Canvas
{
	class FTFontManager;
	class TR_CLASS()
		FTFont : public IWeakRefObject
	{
	public:
		FTWord* GetWord(int x, int y, UINT c, UInt16 transformIndex, Canvas::FCanvasVertex vert[4]) const;
		v3dxVector2 GetTextSize(const WCHAR * text) const
		{
			v3dxVector2 result(0, 0);
			for (const WCHAR* c = text; c[0] != '\0'; c++)
			{
				auto word = GetWord(0, 0, c[0], 0, nullptr);
				result.X += word->Advance.X;
				if (word->TexY + word->PixelHeight > result.Y)
					result.Y = (float)(word->TexY + word->PixelHeight);
			}
			return result;
		}
		UINT GetWordNum() const{
			return (UINT)mWordTable.size();
		}
		UINT GetTotalWords(UINT* pUnicodes, UINT Count);
	public:
		FTFont();
		~FTFont();
		virtual void Cleanup() override;
		bool Init(const char* name, NxRHI::IGpuDevice * rc, FTFontManager * ftMgr, XndHolder* xnd, int fontSize, int texSizeX, int texSizeY);
		bool LoadFtFaceFromFile(FTFontManager* manager, const char* font);
		bool LoadFtFaceFromBlob(FTFontManager* manager, IBlobObject* blob);
		bool InitForBuildFont(NxRHI::IGpuDevice* rc, FTFontManager* ftMgr, const char* name, int fontSize, 
			int SdfPixelSize, int SdfSpread, int SdfPixelColored);
		bool IsNeedSave() const {
			return NeedSave;
		}
		void SaveFontSDF(XndNode* node);
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
		const char* GetSourceFont() const{
			return mSdfSourceFont.c_str();
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
		std::vector<AutoRef<FWordHolder>>	mWordTable;

		bool								NeedSave = false;
		bool								Dirty = true;
		AutoRef<FTPagedWordAllocator>		mFTWordAllocator;
		
		AutoRef<XndHolder>					mSdfXnd;
		AutoRef<XndAttribute>				mWordBitmapAttr;
		
		FT_Face								mFtFace;
		FT_Byte*							mFtContent;
		TWeakRefHandle<FTFontManager>		mManager;
		AutoRef<IBlobObject>				mFontBlob;

		VSLLock mLocker;
	private:
		FT_Face LoadFtFaceFromFile(FT_Library ftlib, const char* font);
		FT_Face LoadFtFaceFromBlob(FT_Library ftlib, IBlobObject* blob);
		bool LoadChar(FT_Library ftlib, WCHAR unicode, int fontSize, int outline_type, int outline_thickness, FTWord* word);
	};

	class TR_CLASS()
		FTFontManager : public IWeakRefObject
	{
	public:
		ENGINE_RTTI(FTFontManager)
			FTFontManager();
		~FTFontManager();
		bool Init();
		virtual void Cleanup() override;

		FTFont* CreateFontSDF(const char* name, NxRHI::IGpuDevice* device, XndHolder* xnd, int fontSize, int texSizeX, int texSizeY);
	public:
		FT_Library				mFtlib;
	};
}

NS_END