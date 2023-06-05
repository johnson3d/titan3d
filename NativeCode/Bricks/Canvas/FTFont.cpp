#include "FTFont.h"
#include "FTFont.h"
#include "../../NextRHI/NxCommandList.h"
#include FT_STROKER_H

#define new VNEW

NS_BEGIN

ENGINE_RTTI_IMPL(EngineNS::Canvas::FTFontManager)

namespace Canvas
{
	FTWord* FTFont::GetWord(int x, int y, UINT c, Canvas::FCanvasVertex vert[4]) const
	{
		auto pThis = (FTFont*)this;
		auto pWord = pThis->GetWord(c);

		//todo: fill vert[4] ...
		pWord->FillCanvasVertices(x, y, pWord->Brush, (void*)vert);

		return pWord;
	}

	FTFont::FTFont()
	{
		
	}

	FTFont::~FTFont()
	{
		Cleanup();
	}

	void FTFont::Cleanup()
	{
		mWords.clear();
	}

	bool FTFont::Init(NxRHI::IGpuDevice* rc, FTFontManager* ftMgr, const char* name, int fontSize, int texSizeX, int texSizeY, bool bUseSDF)
	{
		mUseSDF = bUseSDF;
		mManager.FromObject(ftMgr);
		mName = name;
		mFontSize = fontSize;
		mFTWordAllocator = MakeWeakRef(new FTPagedWordAllocator());
		
		if (mUseSDF)
		{
			mSdfXnd = MakeWeakRef(new XndHolder());
			mSdfXnd->LoadXnd(name);
			auto attr = mSdfXnd->GetRootNode()->TryGetAttribute("UniCode");
			if (attr == nullptr)
			{
				return false;
			}
			else
			{
				mWordBitmapAttr = mSdfXnd->GetRootNode()->TryGetAttribute("Words");
				if (mWordBitmapAttr == nullptr)
					return false;

				attr->BeginRead();
				attr->Read(mFontSize);
				int count;
				attr->Read(count);
				mSdfCodePairs.clear();
				for (int i = 0; i < count; i++)
				{
					UINT unicode;
					attr->Read(unicode);
					UINT64 offset;
					attr->Read(offset);
					mSdfCodePairs.push_back(std::make_pair(unicode, offset));
				}
				attr->EndRead();
			}
			//LoadFtFace(ftMgr->mFtlib, "F:/TitanEngine/enginecontent/fonts/Roboto-Regular.ttf");
		}
		else
		{
			LoadFtFace(ftMgr->mFtlib, name);
		}
		
		mFTWordAllocator->Creator.Initialize(rc, mFontSize, texSizeX, texSizeY);
		return true;
	}

	FT_Face FTFont::LoadFtFace(FT_Library ftlib, const char* font)
	{
		FT_Error err = FALSE;

		FILE* pRealFile = fopen(font, "rb");
		if (pRealFile)
		{
			fclose(pRealFile);
			pRealFile = nullptr;
			err = FT_New_Face(ftlib, font, 0, &mFtFace);
		}
		else
		{
			/*AutoRef<VRes2Memory> f2m = F2MManager::Instance.GetF2M(font);
			if (f2m != nullptr)
			{
				auto pMem = f2m->Ptr();
				if (pMem)
				{
					DWORD len = (DWORD)f2m->Length();
					BYTE* pData = new BYTE[len];
					memcpy(pData, pMem, len);
					err = FT_New_Memory_Face(ftlib, pData, (FT_Long)len, 0, &mFtFace);
					mMemFtData = std::shared_ptr<BYTE>(pData);
				}
				else
				{
					return NULL;
				}

				f2m->Free();
				f2m->TryReleaseHolder();
			}*/
			/*ViseFile io;
			if (io.Open(font, VFile::modeRead)==FALSE)
			{
				return nullptr;
			}
			BYTE* pData = new BYTE[io.GetLength()];
			io.Read(pData, io.GetLength());
			err = FT_New_Memory_Face(ftlib, pData, (FT_Long)io.GetLength(), 0, &mFtFace);
			mMemFtData = std::shared_ptr<BYTE>(pData);
			io.Close();*/
		}

		if (mFtFace == NULL)
			return NULL;
		//const int dpi = 64;
		err = FT_Select_Charmap(mFtFace, FT_ENCODING_UNICODE);
		if (err)
		{
			VFX_LTRACE(ELTT_Graphics, "FT_Select_Charmap failed = %s\r\n", font);
			return NULL;
		}

		mFtContent = NULL;

		return mFtFace;
	}

	UINT FTFont::GetFirstUnicode(UINT* index)
	{
		return FT_Get_First_Char(mFtFace, index);
	}
	UINT FTFont::GetNextUnicode(UINT code, UINT* index)
	{
		return FT_Get_Next_Char(mFtFace, code, index);
	}
	UINT FTFont::GetCharIndex(UINT uniCode)
	{
		return FT_Get_Char_Index(mFtFace, uniCode);
	}

	bool FTFont::LoadCharSDF(UINT unicode, FTWord* word)
	{
		auto it = std::lower_bound(
			mSdfCodePairs.begin(),
			mSdfCodePairs.end(),
			unicode,
			[](const std::pair<UINT, UINT64>& l, const UINT& r)
			{
				return l.first < r;
			});
		if (it == mSdfCodePairs.end())
		{
			return false;
		}
		if ((*it).first == unicode)
		{
			mWordBitmapAttr->BeginRead();
			mWordBitmapAttr->ReaderSeek((*it).second);
			UINT unicode = 0;
			mWordBitmapAttr->Read(unicode);
			mWordBitmapAttr->Read(word->PixelX);
			mWordBitmapAttr->Read(word->PixelY);
			mWordBitmapAttr->Read(word->PixelWidth);
			mWordBitmapAttr->Read(word->PixelHeight);
			mWordBitmapAttr->Read(word->Advance);
			mWordBitmapAttr->Read(word->SdfScale);
			word->Pixels.resize(word->PixelWidth * word->PixelHeight);
			if (word->Pixels.size() > 0)
			{
				UINT wSize = 0;
				mWordBitmapAttr->Read(wSize);
				auto pZstd = new BYTE[wSize];
				mWordBitmapAttr->Read(pZstd, wSize);
				auto dsize = (UINT)CoreSDK::Decompress_ZSTD(&word->Pixels[0], word->Pixels.size(), pZstd, wSize);
				ASSERT(dsize == word->Pixels.size());
				delete[] pZstd;
			}

			mWordBitmapAttr->EndRead();
			return true;
		}
		else
		{
			return false;
		}
	}
	bool FTFont::LoadChar(FT_Library ftlib, WCHAR unicode, int outline_type, int outline_thickness, FTWord* word)
	{
		if (mFtFace == NULL)
		{
			VFX_LTRACE(ELTT_Graphics, "LoadFtFace %s failed\r\n", mName.c_str());
			return false;
		}

		FT_Error err = FT_Set_Pixel_Sizes(mFtFace, 0, mFontSize);//FT_Set_Char_Size(face, 0, font_size * 64, 0, 0);
		if (err)
		{
			VFX_LTRACE(ELTT_Graphics, "FT_Set_Char_Size %s failed\r\n", mName.c_str());
			return false;
		}
		FT_Int32 flags = 0;
		flags |= FT_LOAD_NO_BITMAP;
		flags |= FT_LOAD_FORCE_AUTOHINT;

		err = FT_Load_Char(mFtFace, unicode, flags);
		if (err)
		{
			VFX_LTRACE(ELTT_Graphics, "FT_Load_Char error(ch:%d)\r\n", unicode);
			return false;
		}

		int pitch = 0;
		unsigned char* src_line = NULL;

		FT_Glyph ft_glyph;
		FT_Bitmap ft_bitmap;
		FT_BitmapGlyph ft_bitmap_glyph;
		/*FT_Stroker stroker;
		err = FT_Stroker_New(ftlib, &stroker);
		if (err)
		{
			VFX_LTRACE(ELTT_Graphics, "FT_Stroker_New %s failed\r\n", mName.c_str());
			return false;
		}
		FT_Stroker_Set(stroker,
			(int)(outline_thickness * 64),
			FT_STROKER_LINECAP_ROUND,
			FT_STROKER_LINEJOIN_ROUND,
			0);*/
		err = FT_Get_Glyph(mFtFace->glyph, &ft_glyph);
		if (err)
		{
			VFX_LTRACE(ELTT_Graphics, "FT_Get_Glyph %s failed\r\n", mName.c_str());
			return false;
		}

		/*if (outline_type == 1)
		{
			err = FT_Glyph_Stroke(&ft_glyph, stroker, 1);
		}
		else if (outline_type == 2)
		{
			err = FT_Glyph_StrokeBorder(&ft_glyph, stroker, 0, 1);
		}
		else if (outline_type == 3)
		{
			err = FT_Glyph_StrokeBorder(&ft_glyph, stroker, 1, 1);
		}
		if (err)
		{
			VFX_LTRACE(ELTT_Graphics, "FT_Glyph_Stroke %s failed\r\n", mName.c_str());
			return false;
		}*/

		err = FT_Glyph_To_Bitmap(&ft_glyph, FT_RENDER_MODE_NORMAL, 0, 1);
		if (err)
		{
			VFX_LTRACE(ELTT_Graphics, "FT_Glyph_To_Bitmap %s failed\r\n", mName.c_str());
			return false;
		}

		ft_bitmap_glyph = (FT_BitmapGlyph)ft_glyph;
		ft_bitmap = ft_bitmap_glyph->bitmap;

		word->PixelWidth = ft_bitmap.width;
		word->PixelHeight = ft_bitmap.rows;
		word->PixelX = ft_bitmap_glyph->left;
		word->PixelY = word->FontSize - ft_bitmap_glyph->top;
		//word->PixelX = 0;
		//word->PixelY = 0;
		word->Advance.x = (float)ft_bitmap_glyph->root.advance.x / 65536.0f;
		word->Advance.y = (float)ft_bitmap_glyph->root.advance.y / 65536.0f;
		//word->AdvanceX = word->PixelX + word->PixelWidth;//mFontSize;//
		/*if (word->AdvanceX == 0)
			word->AdvanceX = mFontSize / 2;*/

			//if (word->PixelWidth > mFontSize || word->PixelHeight > mFontSize)
			//{
			//	ASSERT(false);
			//	//FT_Stroker_Done(stroker);
			//	return false;
			//}

		pitch = ft_bitmap.pitch;
		src_line = ft_bitmap.buffer;

		int _bufferSize = word->PixelWidth * word->PixelHeight;
		word->Pixels.resize(_bufferSize);
		if (_bufferSize == 0)
		{
			return true;
		}
		auto dst_line = &word->Pixels[0];

		switch (ft_bitmap.pixel_mode)
		{
			case FT_PIXEL_MODE_GRAY:
			{
				for (int row_idx = 0; row_idx < word->PixelHeight; row_idx++)
				{
					auto writePtr = dst_line;
					for (int x = 0; x < word->PixelWidth; x++)
					{
						auto alpha = src_line[x];
						//writePtr[x] = 0x00FFFFFF | (((DWORD)alpha) << 24);
						writePtr[x] = (BYTE)alpha;
					}
					src_line += pitch;
					dst_line += word->PixelWidth;
				}
			}
			break;
			case FT_PIXEL_MODE_MONO:
			{
				for (int row_idx = 0; row_idx < word->PixelHeight; row_idx++)
				{
					auto writePtr = dst_line;
					for (int x = 0; x < word->PixelWidth; x++)
					{
						auto alpha = src_line[x];
						writePtr[x] = (BYTE)alpha;
						/*if ((alpha & 0x80) == alpha)
						{
							writePtr[x] = 0x00FFFFFF | (((DWORD)alpha) << 24);
						}
						else
						{
							writePtr[x] = 0x00FFFFFF | (((DWORD)alpha) << 24);
						}*/
					}
					src_line += pitch;
					dst_line += word->PixelWidth;
				}
			}
			break;
		}

		//FT_Stroker_Done(stroker);

		return true;
	}

	void FTFont::MeasureString(const WCHAR* text, int* width, int* height)
	{
		int x = 0;
		*width = 0;
		*height = mFontSize;
		auto len = wcslen(text);
		for (size_t i = 0; i < len; i++)
		{
			//if (text[i] == '\n')
			//{
			//	if (i != len - 1)
			//		*height += mFontSize;
			//	x = 0;
			//	continue;
			//}
			auto pWord = GetWord(text[i]);
			x += (int)pWord->Advance.x;
			auto wordHeight = mFontSize - pWord->PixelY + pWord->PixelHeight;
			if (*height < wordHeight)
				*height = wordHeight;
			if (x > *width)
				*width = x;
		}
	}

	int FTFont::CheckPointChar(const WCHAR* text, int x, int y, int* pos)
	{
		int width = 0;
		auto len = wcslen(text);
		for (size_t i = 0; i < len; i++)
		{
			auto pWord = GetWord(text[i]);
			width += (int)pWord->Advance.x;
			if (x < width)
			{
				if (pos != nullptr)
				{
					*pos = width - (int)pWord->Advance.x;
				}
				return (int)i;
			}
		}
		return -1;
	}

	int FTFont::CalculateWrap(const WCHAR* text, int* idxArray, int idxArraySize, int widthLimit, int* height)
	{
		int retLineCount = 1;
		auto len = wcslen(text);
		int curArrayIdx = 0;
		int currentLineWidth = 0;
		*height = 0;
		int lineMaxHeight = mFontSize;
		for (size_t i = 0; i < len; i++)
		{
			if (text[i] == '\n')
			{
				if (i != len - 1)
				{
					*height += mFontSize;
					(idxArray)[curArrayIdx] = (int)i;
					retLineCount++;
					curArrayIdx++;
					if (curArrayIdx == idxArraySize)
						return retLineCount;
				}
				currentLineWidth = 0;
				continue;
			}

			auto pWord = GetWord(text[i]);
			currentLineWidth += (int)pWord->Advance.x;
			if (currentLineWidth >= widthLimit)
			{
				if (currentLineWidth == pWord->Advance.x)
				{
					continue;
				}
				*height += lineMaxHeight;// mFontSize;
				(idxArray)[curArrayIdx] = (int)i;
				retLineCount++;
				curArrayIdx++;
				if (curArrayIdx == idxArraySize)
					return retLineCount;
				currentLineWidth = (int)pWord->Advance.x;
				lineMaxHeight = mFontSize;
			}
			int wordHeight = mFontSize - pWord->PixelY + pWord->PixelHeight;
			if (lineMaxHeight < wordHeight)
				lineMaxHeight = wordHeight;
		}
		*height += lineMaxHeight;
		return retLineCount;
	}
	int FTFont::CalculateWrapWithWord(const WCHAR* text, int* idxArray, int idxArraySize, int widthLimit, int* height)
	{
		int retLineCount = 1;
		auto len = wcslen(text);
		int curArrayIdx = 0;
		int currentLineWidth = 0;
		*height = 0;
		int lineMaxHeight = mFontSize;
		int lastWrapPos = -1;
		for (size_t i = 0; i < len; )
		{
			if (text[i] == '\n')
			{
				if (i != len - 1)
				{
					*height += mFontSize;
					(idxArray)[curArrayIdx] = (int)i;
					retLineCount++;
					curArrayIdx++;
					if (curArrayIdx == idxArraySize)
						return retLineCount;
				}
				currentLineWidth = 0;
				i++;
				continue;
			}

			auto pWord = GetWord(text[i]);
			if (((pWord->UniCode >= 65 && pWord->UniCode <= 90) ||
				(pWord->UniCode >= 97 && pWord->UniCode <= 122)))
			{
			}
			else
				lastWrapPos = (int)i;
			currentLineWidth += (int)pWord->Advance.x;
			if (currentLineWidth >= widthLimit)
			{
				if (currentLineWidth == (int)pWord->Advance.x)
				{
					i++;
				}
				else if (!((pWord->UniCode >= 65 && pWord->UniCode <= 90) ||
					(pWord->UniCode >= 97 && pWord->UniCode <= 122)))
				{
					*height += lineMaxHeight;
					(idxArray)[curArrayIdx] = (int)i;
					retLineCount++;
					curArrayIdx++;
					if (curArrayIdx == idxArraySize)
						return retLineCount;
					currentLineWidth = (int)pWord->Advance.x;
					lineMaxHeight = mFontSize;
					lastWrapPos = -1;
					i++;
					continue;
				}
				else if (lastWrapPos != -1)
				{
					*height += lineMaxHeight;
					(idxArray)[curArrayIdx] = lastWrapPos + 1;
					retLineCount++;
					curArrayIdx++;
					if (curArrayIdx == idxArraySize)
						return retLineCount;

					auto tempWord = GetWord(text[lastWrapPos + 1]);
					currentLineWidth = (int)tempWord->Advance.x;
					lineMaxHeight = mFontSize;
					i = lastWrapPos + 1;
					lastWrapPos = -1;
					continue;
				}
				else
					i++;
			}
			else
			{
				i++;
			}
			if (lastWrapPos == -1)
			{
				int wordHeight = mFontSize - pWord->PixelY + pWord->PixelHeight;
				if (lineMaxHeight < wordHeight)
					lineMaxHeight = wordHeight;
			}

		}
		*height += lineMaxHeight;
		return retLineCount;
	}

	UINT FTFont::GetWords(std::vector<FTWord*>& words, const WCHAR* text, UINT numOfChar)
	{
		if (numOfChar == 0)
		{
			numOfChar = (UINT)wcslen(text);
		}
		else
		{
			numOfChar = std::min((UINT)wcslen(text), numOfChar);
		}
		for (UINT i = 0; i < numOfChar; i++)
		{
			auto pWord = GetWord(text[i]);
			pWord->AddRef();
			words.push_back(pWord);
		}
		return (UINT)words.size();
	}

	UINT FTFont::GetWords(FTWord** pWords, UINT count, const WCHAR* text, UINT numOfChar)
	{
		if (numOfChar == 0)
		{
			numOfChar = (UINT)wcslen(text);
		}
		else
		{
			numOfChar = std::min((UINT)wcslen(text), numOfChar);
		}
		for (UINT i = 0; i < numOfChar; i++)
		{
			auto pWord = GetWord(text[i]);
			pWords[i] = pWord;
		}
		return numOfChar;
	}

	FTWord* FTFont::GetWord(UINT code)
	{
		auto i = mWords.find(code);
		if (i != mWords.end())
		{
			i->second->HitCount++;
			return i->second;
		}

		auto word = mFTWordAllocator->Alloc();
		mWords.insert(std::make_pair(code, word->RealObject));

		word->RealObject->HitCount = 1;
		word->RealObject->UniCode = code;
		auto mgr = mManager.GetPtr();
		if (mUseSDF)
		{
			if (LoadCharSDF(code, word->RealObject) == false)
				return nullptr;
		}
		else
		{
			if (LoadChar(mgr->mFtlib, code, 1, 0, word->RealObject) == false)
				return nullptr;
		}
		
		word->RealObject->Brush->PutWord(word->RealObject);
		word->RealObject->Font = this;
		word->RealObject->SetDirty();

		return word->RealObject;
	}

	void FTFont::ResetWords()
	{
		for (auto& i : mWords)
		{
			
		}
		mWords.clear();
	}

	void FTFont::Update(NxRHI::IGpuDevice* device, bool bflipV)
	{
		if (Dirty)
		{
			Dirty = false;
			NxRHI::FTransientCmd tsCmd(device, NxRHI::QU_Transfer);
			auto cmd = tsCmd.GetCmdList();
			for (auto& i : mWords)
			{
				i.second->Flush2Texture(device, cmd);
			}
		}
		//remove cold words
		
		mFTWordAllocator->Update(device, bflipV);
	}

	bool FTFontManager::Init()
	{
		FT_Error err;
		err = FT_Init_FreeType(&mFtlib);
		if (err)
		{
			VFX_LTRACE(ELTT_Graphics, "FreeType Init Failed\r\n");
			return false;
		}
		return true;
	}
	void FTFontManager::Cleanup()
	{
		mFonts.clear();

		if (mFtlib)
		{
			FT_Done_FreeType(mFtlib);
			mFtlib = NULL;
		}
	}

	FTFontManager::FTFontManager()
	{
		Init();
	}

	FTFontManager::~FTFontManager()
	{
		Cleanup();
	}

	FTFont* FTFontManager::GetFont(NxRHI::IGpuDevice* device, const char* file, int fontSize, int texSizeX, int texSizeY, bool bUseSDF)
	{
		if (fontSize > texSizeX || fontSize > texSizeY)
		{
			VFX_LTRACE(ELTT_Graphics, "FTFontManager::GetFont(%s,%d) FontSize>(%d,%d)", file, fontSize, texSizeX, texSizeY);
			return nullptr;
		}
		FontKey key;
		key.Name = file;
		key.FontSize = fontSize;
		auto iter = mFonts.find(key);
		if (iter != mFonts.end())
		{
			return iter->second;
		}

		auto font = MakeWeakRef(new FTFont());
		mFonts.insert(std::make_pair(key, font));

		font->Init(device, this, file, fontSize, texSizeX, texSizeY, bUseSDF);
		//font->AddRef();
		return font;
	}

	void FTFontManager::Update(NxRHI::IGpuDevice* rc, bool bflipV)
	{
		for (auto& i : mFonts)
		{
			i.second->Update(rc, bflipV);
		}
	}

}

NS_END
