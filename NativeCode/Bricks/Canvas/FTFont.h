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
		bool Init(NxRHI::IGpuDevice * rc, FTFontManager * ftMgr, const char* name, int fontSize, int texSizeX, int texSizeY, bool bUseSDF);
		FTWord* GetWord(UINT uniCode);
		void Update(NxRHI::IGpuDevice * rc, bool bflipV);
		UINT GetWords(std::vector<FTWord*>&words, const WCHAR * text, UINT numOfChar);
		UINT GetWords(FTWord * *pWords, UINT count, const WCHAR * text, UINT numOfChar);
		void MeasureString(const WCHAR * text, int* width, int* height);
		int CheckPointChar(const WCHAR * text, int x, int y, int* pos = nullptr);
		int CalculateWrap(const WCHAR * text, int* idxArray, int idxArraySize, int widthLimit, int* height);
		int CalculateWrapWithWord(const WCHAR * text, int* idxArray, int idxArraySize, int widthLimit, int* height);

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
	private:
		FT_Face LoadFtFace(FT_Library ftlib, const char* font);
		bool LoadChar(FT_Library ftlib, WCHAR unicode, int outline_type, int outline_thickness, FTWord * word);
		bool LoadCharSDF(UINT unicode, FTWord * word);
	protected:
		bool								Dirty = true;
		std::string							mName;
		int									mFontSize;
		AutoRef<FTPagedWordAllocator>		mFTWordAllocator;
		std::map<UINT, AutoRef<FTWord>>		mWords;

		bool								mUseSDF = true;
		AutoRef<XndHolder>					mSdfXnd;
		AutoRef<XndAttribute>				mWordBitmapAttr;
		std::vector<std::pair<UINT, UINT64>>	mSdfCodePairs;
		
		FT_Face								mFtFace;
		FT_Byte*							mFtContent;
		TWeakRefHandle<FTFontManager>		mManager;

		std::shared_ptr<BYTE>				mMemFtData;
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

		FTFont* GetFont(NxRHI::IGpuDevice* device, const char* file, int fontSize, int texSizeX, int texSizeY, bool bUseSDF);
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