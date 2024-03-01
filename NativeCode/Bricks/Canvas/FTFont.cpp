#include "FTFont.h"
#include "FTFont.h"
#include "../../NextRHI/NxCommandList.h"
#include FT_STROKER_H

#define new VNEW

NS_BEGIN

ENGINE_RTTI_IMPL(EngineNS::Canvas::FTFontManager)

namespace Canvas
{
	FTWord* FTFont::GetWord(int x, int y, UINT c, UInt16 transformIndex, Canvas::FCanvasVertex vert[4]) const
	{
		auto pThis = (FTFont*)this;
		auto pWord = pThis->GetWord(c);
		if (pWord == nullptr)
		{
			ASSERT(false);
			return nullptr;
		}

		//todo: fill vert[4] ...
		pWord->FillCanvasVertices(x, y, transformIndex, pWord->Brush, (void*)vert);

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
		mWordTable.clear();
	}

	bool FTFont::InitForBuildFont(NxRHI::IGpuDevice* rc, FTFontManager* ftMgr, const char* name, int fontSize, 
		int SdfPixelSize, int SdfSpread, int SdfPixelColored)
	{
		mManager.FromObject(ftMgr);
		//mName = name;
		mFontSize = fontSize;
		mSdfPixelSize = SdfPixelSize;
		mSdfSpread = SdfSpread;
		mSdfPixelColored = SdfPixelColored;
		
		mSdfSourceFont = name;
		auto pos = mSdfSourceFont.find_last_of('/');
		if (pos != std::string::npos)
		{
			mSdfSourceFont = mSdfSourceFont.substr(pos + 1);
		}
		LoadFtFace(ftMgr->mFtlib, name);

		mFTWordAllocator = MakeWeakRef(new FTPagedWordAllocator());
		mFTWordAllocator->Creator.Initialize(rc, SdfPixelSize, SdfPixelSize, SdfPixelSize);
		return true;
	}

	bool FTFont::Init(NxRHI::IGpuDevice* rc, FTFontManager* ftMgr, const char* name, int fontSize, int texSizeX, int texSizeY)
	{
		mManager.FromObject(ftMgr);
		mName = name;
		mFontSize = fontSize;
		mFTWordAllocator = MakeWeakRef(new FTPagedWordAllocator());
		mFTWordAllocator->Creator.Initialize(rc, mFontSize, texSizeX, texSizeY);

		{
			mSdfXnd = MakeWeakRef(new XndHolder());
			mSdfXnd->LoadXnd(name);
			auto attr = mSdfXnd->GetRootNode()->TryGetAttribute("SdfDesc");
			if (attr != nullptr)
			{
				attr->BeginRead();
				attr->Read(mSdfSourceFont);
				attr->Read(mFontSize);
				attr->Read(mSdfPixelSize);
				attr->Read(mSdfSpread);
				attr->Read(mSdfPixelColored);
				attr->EndRead();
			}
			attr = mSdfXnd->GetRootNode()->TryGetAttribute("UniCode");
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

				mWordTable.resize(count);
				for (int i = 0; i < count; i++)
				{
					mWordTable[i] = MakeWeakRef(new FWordHolder());
					attr->Read(mWordTable[i]->Unicode);
					attr->Read(mWordTable[i]->Offset);
				}
				attr->EndRead();
			}
		}
		
		auto fdPos = mName.find_last_of('/');
		if (fdPos != std::string::npos)
		{
			auto ftf = mName.substr(0, fdPos + 1);
			ftf += mSdfSourceFont;
			LoadFtFace(ftMgr->mFtlib, ftf.c_str());
		}

		NeedSave = false;
		return true;
	}
	void FTFont::SaveFontSDF(const char* name)
	{
		auto xnd = MakeWeakRef(new XndHolder());
		auto root = MakeWeakRef(xnd->NewNode("FontSDF", 0, 0));
		xnd->SetRootNode(root);

		auto attr = MakeWeakRef(xnd->NewAttribute("SdfDesc", 0, 0));
		xnd->GetRootNode()->AddAttribute(attr);
		if (attr != nullptr)
		{
			attr->BeginWrite();
			attr->Write(mSdfSourceFont);
			attr->Write(mFontSize);
			attr->Write(mSdfPixelSize);
			attr->Write(mSdfSpread);
			attr->Write(mSdfPixelColored);
			attr->EndWrite();
		}
		attr = MakeWeakRef(xnd->NewAttribute("UniCode", 0, 0));
		auto attrWords = MakeWeakRef(xnd->NewAttribute("Words", 0, 0));
		xnd->GetRootNode()->AddAttribute(attr);
		xnd->GetRootNode()->AddAttribute(attrWords);
		if (attr != nullptr)
		{
			attr->BeginWrite();
			attrWords->BeginWrite();
			attr->Write(mFontSize);
			attr->Write((int)mWordTable.size());
			for (auto& i : mWordTable)
			{
				attr->Write(i->Unicode);
				attr->Write(attrWords->GetWriterPosition());
				if (i->FtWord != nullptr)
				{
					i->SaveTo(attrWords, i->FtWord->RealObject, i->Unicode);
				}
				else
				{
					auto FtWord = MakeWeakRef(new FTWord());
					mWordBitmapAttr->BeginRead();
					mWordBitmapAttr->ReaderSeek(i->Offset);
					FWordHolder::Load(mWordBitmapAttr, FtWord, i->Unicode);
					mWordBitmapAttr->EndRead();

					i->SaveTo(attrWords, FtWord, i->Unicode);
				}
			}
			attrWords->EndWrite();
			attr->EndWrite();
		}

		if (mSdfXnd != nullptr)
			mSdfXnd->TryReleaseHolder();
		xnd->SaveXnd(name);
		xnd->TryReleaseHolder();

		NeedSave = false;
		if (mName == name)
		{
			ResetWords();
			
			mSdfXnd = MakeWeakRef(new XndHolder());
			mSdfXnd->LoadXnd(name);
			auto attr = mSdfXnd->GetRootNode()->TryGetAttribute("SdfDesc");
			if (attr != nullptr)
			{
				attr->BeginRead();
				attr->Read(mSdfSourceFont);
				attr->Read(mFontSize);
				attr->Read(mSdfPixelSize);
				attr->Read(mSdfSpread);
				attr->Read(mSdfPixelColored);
				attr->EndRead();
			}
			attr = mSdfXnd->GetRootNode()->TryGetAttribute("UniCode");
			{
				mWordBitmapAttr = mSdfXnd->GetRootNode()->TryGetAttribute("Words");
				if (mWordBitmapAttr == nullptr)
					return;

				attr->BeginRead();
				attr->Read(mFontSize);
				int count;
				attr->Read(count);

				mWordTable.resize(count);
				for (int i = 0; i < count; i++)
				{
					mWordTable[i] = MakeWeakRef(new FWordHolder());
					attr->Read(mWordTable[i]->Unicode);
					attr->Read(mWordTable[i]->Offset);
				}
				attr->EndRead();
			}
		}
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

	bool FTFont::LoadChar(FT_Library ftlib, WCHAR unicode, int fontSize, int outline_type, int outline_thickness, FTWord* word)
	{
		word->FontSize = fontSize;
		if (mFtFace == NULL)
		{
			VFX_LTRACE(ELTT_Graphics, "LoadFtFace %s failed\r\n", mName.c_str());
			return false;
		}

		FT_Error err = FT_Set_Pixel_Sizes(mFtFace, 0, fontSize);//FT_Set_Char_Size(face, 0, font_size * 64, 0, 0);
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
		word->Advance.X = (float)ft_bitmap_glyph->root.advance.x / 65536.0f;
		word->Advance.Y = (float)ft_bitmap_glyph->root.advance.y / 65536.0f;
		
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
	void FTFont::FWordHolder::SaveTo(XndAttribute* attr, FTWord* word, UINT Unicode)
	{
		attr->Write(Unicode);
		attr->Write(word->PixelX);
		attr->Write(word->PixelY);
		attr->Write(word->PixelWidth);
		attr->Write(word->PixelHeight);
		attr->Write(word->Advance);
		attr->Write(word->SdfScale);

		//auto dsize = (UINT)CoreSDK::Compress_ZSTD(nullptr, 0, &word->Pixels[0], word->Pixels.size(), 1);
		std::vector<BYTE>	Pixels;
		if (word->Pixels.size() > 0)
		{
			Pixels.resize(CoreSDK::CompressBound_ZSTD(word->Pixels.size()));
			auto dsize = (UINT)CoreSDK::Compress_ZSTD(&Pixels[0], Pixels.size(), &word->Pixels[0], word->Pixels.size(), 1);
			attr->Write(dsize);
			attr->Write(&Pixels[0], dsize);
		}
		else
		{
			attr->Write((UINT)0);
		}
	}
	void FTFont::FWordHolder::Load(XndAttribute* attr, FTWord* word, UINT Unicode)
	{
		UINT code;
		attr->Read(code);
		ASSERT(code == Unicode);
		attr->Read(word->PixelX);
		attr->Read(word->PixelY);
		attr->Read(word->PixelWidth);
		attr->Read(word->PixelHeight);
		attr->Read(word->Advance);
		attr->Read(word->SdfScale);

		word->Pixels.resize(word->PixelWidth * word->PixelHeight);
		if (word->Pixels.size() > 0)
		{
			UINT wSize = 0;
			attr->Read(wSize);
			auto pZstd = new BYTE[wSize];
			attr->Read(pZstd, wSize);
			auto dsize = (UINT)CoreSDK::Decompress_ZSTD(&word->Pixels[0], word->Pixels.size(), pZstd, wSize);
			ASSERT(dsize == word->Pixels.size());
			delete[] pZstd;
		}
	}
	FTFont::FtPagedWord* FTFont::FWordHolder::GetWord(FTFont* font)
	{
		if (FtWord != nullptr)
		{
			FtWord->RealObject->HitCount++;
		}
		else
		{
			FtWord = font->mFTWordAllocator->Alloc();
			FtWord->RealObject->HitCount = 1;
			FtWord->RealObject->UniCode = Unicode;

			auto mWordBitmapAttr = font->mWordBitmapAttr;
			mWordBitmapAttr->BeginRead();
			mWordBitmapAttr->ReaderSeek(Offset);
			Load(mWordBitmapAttr, FtWord->RealObject, Unicode);
			mWordBitmapAttr->EndRead();

			FtWord->RealObject->Brush->PutWord(FtWord->RealObject);
			FtWord->RealObject->Font = font;
			FtWord->RealObject->SetDirty();
		}

		return FtWord;
	}

	FTFont::FtPagedWord* FTFont::FWordHolder::BuildWord(FTFont* font, bool bOnlyBuildFont)
	{
		auto word = font->mFTWordAllocator->Alloc();
		word->RealObject->HitCount = 1;
		word->RealObject->UniCode = Unicode;
		word->RealObject->FontSize = font->mFontSize;
		word->RealObject->Font = font;
		auto mgr = font->mManager.GetPtr();
		auto srcWord = MakeWeakRef(new FTWord());
		srcWord->UniCode = Unicode;
		if (font->LoadChar(mgr->mFtlib, Unicode, font->mSdfPixelSize, 1, 0, srcWord) == false)
		{
			return nullptr;
		}
		srcWord->BuildAsSDFFast(word->RealObject, font->mSdfPixelColored, font->mSdfSpread);
		FtWord = word;

		if (bOnlyBuildFont != true)
		{
			FtWord->RealObject->Brush->PutWord(FtWord->RealObject);
			FtWord->RealObject->SetDirty();
		}
		return FtWord;
	}

	void FTFont::AddWordForBuild(UINT code)
	{
		auto mgr = mManager.GetPtr();

		auto wh = MakeWeakRef(new FWordHolder());
		wh->Unicode = code;
		wh->Offset = 0;
		auto result = wh->BuildWord(this, true);
		if (result != nullptr)
		{
			VAutoVSLLock lk(mLocker);

			auto it = std::lower_bound(
				mWordTable.begin(),
				mWordTable.end(),
				code,
				[](const AutoRef<FWordHolder>& l, const UINT& r)
				{
					return l->Unicode < r;
				});
			if (it != mWordTable.end())
			{
				auto& wordHolder = (*it);
				if (wordHolder->Unicode == code)
				{
					if (wordHolder->FtWord != nullptr)
						return;
				}
			}
			mWordTable.insert(it, wh);
		}
	}

	FTWord* FTFont::GetWord(UINT code)
	{
		auto mgr = mManager.GetPtr();
		
		auto it = std::lower_bound(
			mWordTable.begin(),
			mWordTable.end(),
			code,
			[](const AutoRef<FWordHolder>& l, const UINT& r)
			{
				return l->Unicode < r;
			});
		if (it != mWordTable.end())
		{
			auto& wordHolder = (*it);
			if (wordHolder->Unicode == code)
			{
				return wordHolder->GetWord(this)->RealObject;
			}
		}
		auto wh = MakeWeakRef(new FWordHolder());
		wh->Unicode = code;
		wh->Offset = 0;
		auto result = wh->BuildWord(this, false);
		if (result != nullptr)
		{
			NeedSave = true;
			mWordTable.insert(it, wh);
		}
		return result->RealObject;
	}

	void FTFont::ResetWords()
	{
		for (auto& i : mWordTable)
		{
			if (i->FtWord != nullptr)
			{
				mFTWordAllocator->Free(i->FtWord);
				i->FtWord = nullptr;
			}
		}
		mWordTable.clear();
	}

	void FTFont::Update(NxRHI::IGpuDevice* device, bool bflipV)
	{		
		bool temp = false;
		if (temp)
		{
			for (auto& i : mWordTable)
			{
				if (i->FtWord != nullptr && i->Unicode == 'o')
				{
					i->FtWord->RealObject->SetDirty();
				}
			}
		}
		if (Dirty)
		{
			Dirty = false;
			NxRHI::FTransientCmd tsCmd(device, NxRHI::QU_Transfer, "Font.Update");
			auto cmd = tsCmd.GetCmdList();
			int NumOfDirty = 0;
			int NumOfFlush = 0;
			for (auto& i : mWordTable)
			{
				if (i->FtWord != nullptr)
				{
					if (i->FtWord->RealObject->Dirty)
					{
						NumOfDirty++;
						if (NumOfFlush < 24)
						{
							NumOfFlush++;
							i->FtWord->RealObject->Flush2Texture(device, cmd);
						}
					}
				}	
			}
			cmd->FlushDraws();
			if (NumOfFlush < NumOfDirty)
				Dirty = true;
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

	FTFont* FTFontManager::GetFont(NxRHI::IGpuDevice* device, const char* file, int fontSize, int texSizeX, int texSizeY)
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

		font->Init(device, this, file, fontSize, texSizeX, texSizeY);
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
