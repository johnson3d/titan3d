#include "FTFont.h"
#include FT_STROKER_H

#include "../../Graphics/Mesh/GfxMesh.h"
#include "../../Graphics/Mesh/GfxMeshPrimitives.h"
#include "../../Graphics/Mesh/GfxMdfQueue.h"
#include "../../Graphics/Mesh/GfxMaterialPrimitive.h"
#include "../../Graphics/GfxMaterial.h"
#include "../../Core/r2m/VPakFile.h"


#define new VNEW

NS_BEGIN

RTTI_IMPL(EngineNS::FTFontManager, EngineNS::VIUnknown)
RTTI_IMPL(EngineNS::FTTextDrawContext, EngineNS::VIUnknown)

vBOOL FTTextDrawContext::IsValidVersion(FTFont* font)
{
	return mVersion == font->GetVersion();
}

void FTTextDrawContext::RebuildContext(FTFont* font)
{
	mDrawCalls.clear();
	for (auto i : mTexts)
	{
		font->Draw2TextContext(i.Text.c_str(), i.X, i.Y, this, false);
	}
	mVersion = font->GetVersion();
}

void FTTextDrawContext::ResetContext()
{
	mDrawCalls.clear();
	mTexts.clear();
}

GfxMeshPrimitives* FTTextDrawContext::BuildMesh(IRenderContext* rc, IShaderResourceView** ppRSV, bool bFlipV)
{	
	GfxMeshPrimitives* meshSource = new GfxMeshPrimitives();

	std::vector<FTVertex> vbData;
	vbData.reserve(GetAllTextCharNumber() * 2);
	meshSource->Init(rc, "FTText", (UINT)mDrawCalls.size());
	UINT index = 0;
	for (auto i : mDrawCalls)
	{
		auto strDC = i.second;
		
		DrawPrimitiveDesc dcDesc;
		dcDesc.BaseVertexIndex = (UINT)vbData.size();
		dcDesc.NumPrimitives = (UINT)strDC->Vertices.size() / 3;
		dcDesc.StartIndex = 0xFFFFFFFF;
		//dcDesc.NumPrimitives = 2;
		//dcDesc.NumInstances = (UINT)strDC->Vertices.size() / 6;

		vbData.insert(vbData.begin(), strDC->Vertices.begin(), strDC->Vertices.end());
		meshSource->PushAtomLOD(index, &dcDesc);
		AutoRef<GfxMaterialPrimitive> mtlDC = new GfxMaterialPrimitive();
		
		strDC->Texture->UpdateTexture(rc, bFlipV);
		ppRSV[index] = strDC->Texture->RHITexture;

		index++;
	}

	meshSource->SetGeomtryMeshStream(rc, VST_SkinWeight, &vbData[0], (UINT)vbData.size()* sizeof(FTVertex), sizeof(FTVertex), 0);
	meshSource->GetGeomtryMesh()->SetIsDirty(TRUE);
	meshSource->GetResourceState()->SetStreamState(SS_Valid);

	return meshSource;
}

FTFont::FTFont()
{
	mVersion = 1;
}

FTFont::~FTFont()
{
	Cleanup();
}

void FTFont::Cleanup()
{
	for (auto i : mWords)
	{
		i.second->Release();
	}
	mWords.clear();
}

void FTFont::Init(IRenderContext* rc, FTFontManager* ftMgr, const char* name, int fontSize, int cacheTexSize, int texSize)
{
	mManager.FromObject(ftMgr);
	mName = name;
	mFontSize = fontSize;
	mCacheTextureSize = cacheTexSize;
	mTextureSize = texSize;
	//mHotWordTexture = new ITextTexture();
	//mHotWordTexture->Init(1024, mFontSize);

	LoadFtFace(ftMgr->mFtlib, name);
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
		AutoRef<VRes2Memory> f2m = F2MManager::Instance.GetF2M(font);
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
		}
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

	err = FT_Glyph_To_Bitmap(&ft_glyph, FT_RENDER_MODE_NORMAL, 0, 1);//���ﲻ���ͷ�ô��
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
	word->PixelY = ft_bitmap_glyph->top;
	//word->PixelX = 0;
	//word->PixelY = 0;
	word->AdvanceX = word->PixelX + word->PixelWidth;//mFontSize;//
	if (word->AdvanceX == 0)
		word->AdvanceX = mFontSize / 2;

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
		x += pWord->AdvanceX;
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
		width += pWord->AdvanceX;
		if (x < width)
		{
			if (pos != nullptr)
			{
				*pos = width - pWord->AdvanceX;
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
		currentLineWidth += pWord->AdvanceX;
		if (currentLineWidth >= widthLimit)
		{
			if (currentLineWidth == pWord->AdvanceX)
			{
				continue;
			}
			*height += lineMaxHeight;// mFontSize;
			(idxArray)[curArrayIdx] = (int)i;
			retLineCount++;
			curArrayIdx++;
			if (curArrayIdx == idxArraySize)
				return retLineCount;
			currentLineWidth = pWord->AdvanceX;
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
		// ���㻻�д�
		if (((pWord->UniCode >= 65 && pWord->UniCode <= 90) ||
			 (pWord->UniCode >= 97 && pWord->UniCode <= 122)))
		{
		}
		else
			lastWrapPos = (int)i;
		currentLineWidth += pWord->AdvanceX;
		if (currentLineWidth >= widthLimit)
		{
			if (currentLineWidth == pWord->AdvanceX)
			{
				// ÿ��������һ����
				i++;
			}
			else if (!((pWord->UniCode >= 65 && pWord->UniCode <= 90) ||
				(pWord->UniCode >= 97 && pWord->UniCode <= 122)))
			{
				// ��Ӣ����ĸ���ɻ���
				*height += lineMaxHeight;
				(idxArray)[curArrayIdx] = (int)i;
				retLineCount++;
				curArrayIdx++;
				if (curArrayIdx == idxArraySize)
					return retLineCount;
				currentLineWidth = pWord->AdvanceX;
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
				currentLineWidth = tempWord->AdvanceX;
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

void FTFont::GetWords(std::vector<FTWord*>& words, const WCHAR* text)
{
	auto len = wcslen(text);
	for (size_t i = 0; i < len; i++)
	{
		auto pWord = GetWord(text[i]);
		pWord->AddRef();
		words.push_back(pWord);
	}
}

FTWord* FTFont::GetWord(WCHAR code)
{
	auto i = mWords.find(code);
	if (i != mWords.end())
	{
		i->second->HitCount++;
		return i->second;
	}

	FTWord* word = new FTWord();
	word->FontSize = mFontSize;
	mWords.insert(std::make_pair(code, word));

	word->HitCount = 1;
	word->UniCode = code;
	auto mgr = mManager.GetPtr();
	LoadChar(mgr->mFtlib, code, 1, 0, word);
	auto texId = FindBestTexture();
	word->Texture = mTextures[texId];
	auto index = word->Texture->FindEmptySlot();
	ASSERT(index >= 0);
	word->Texture->SetWord(index, word);
	
	//word->IndexInTexture;

	return word;
}

UINT FTFont::FindBestTexture()
{
	int curSmall = 100000;
	UINT best = -1;
	if (mTextures.size()==0)
	{
		if (mHotWordTexture == nullptr)
		{
			mHotWordTexture = new ITextTexture();
			mHotWordTexture->Init(mCacheTextureSize, mFontSize);
			mTextures.push_back(mHotWordTexture);
		}
	}
	for (size_t i = 0; i < mTextures.size(); i++)
	{
		int remain = mTextures[i]->GetEmptyNumber();
		if(remain==0)
			continue;
		if (curSmall > remain)
		{
			curSmall = remain;
			best = (UINT)i;
		}
	}
	if (best == -1)
	{
		best = (UINT)mTextures.size();
		AutoRef<ITextTexture> tex = new ITextTexture();
		tex->Init(mTextureSize, mFontSize);

		mTextures.push_back(tex);
	}
	return best;
}

void FTFont::UpdateHotWords()
{
	for (auto i : mTextures)
	{
		i->HitCount = 0;
		auto& refWords = i->Words;
		for (auto j = refWords.begin(); j != refWords.end(); )
		{
			if ((*j)->HitCount == 0)
			{//��̭�ܾ�û���õ�
				auto rmv = mWords.find((*j)->UniCode);
				if (rmv != mWords.end())
				{
					rmv->second->Release();
					mWords.erase(rmv);
				}
				j = refWords.erase(j);
			}
			else
			{
				j++;
				i->HitCount += (*j)->HitCount;
			}
		}
	}
	//auto maxCached = mHotWordTexture->CellNum * mHotWordTexture->CellNum;
	
	//sort and mark mWords

	//swap hot words

	mVersion++;
}

void FTFont::Draw2TextContext(const WCHAR* text, int x, int y, FTTextDrawContext* drawCtx, bool flipV)
{
	FTTextDrawContext::TextString tmp;
	tmp.Text = text;
	tmp.X = x;
	tmp.Y = y;
	drawCtx->mTexts.push_back(tmp);

	if (drawCtx->mVersion == 0)
	{
		drawCtx->mVersion = mVersion;
	}

	int sx = x;
	auto len = wcslen(text);
	for (size_t i = 0; i < len; i++)
	{
		if (text[i] == '\n')
		{
			if(i != len - 1)
				y += mFontSize;
			x = sx;
			continue;
		}
		auto pWord = GetWord(text[i]);
		AutoRef<FTStringDC> dc;
		auto iter = drawCtx->mDrawCalls.find(pWord->Texture);
		if (iter == drawCtx->mDrawCalls.end())
		{
			AutoRef<FTStringDC> ndc = new FTStringDC();
			dc = ndc;
			dc->Texture = pWord->Texture;
			drawCtx->mDrawCalls[pWord->Texture] = dc;
		}
		else
		{
			dc = iter->second;
		}
		pWord->FillVertices(drawCtx, x, y, dc->Texture, dc->Vertices, flipV);
		x += pWord->AdvanceX;
	}
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
	for (auto i : mFonts)
	{
		i.second->Release();
	}
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

FTFont* FTFontManager::GetFont(const char* file, int fontSize, int cacheTexSize, int texSize)
{
	if (fontSize > texSize)
	{
		VFX_LTRACE(ELTT_Graphics, "FTFontManager::GetFont(%s,%d) FontSize>%d", file, fontSize, texSize);
		return nullptr;
	}
	FontKey key;
	key.Name = file;
	key.FontSize = fontSize;
	auto iter = mFonts.find(key);
	if (iter != mFonts.end())
	{
		iter->second->AddRef();
		return iter->second;
	}

	auto font = new FTFont();
	mFonts.insert(std::make_pair(key, font));

	font->Init(nullptr, this, file, fontSize, cacheTexSize, texSize);
	//font->AddRef();
	return font;
}

NS_END

using namespace EngineNS;

extern "C"
{
	CSharpReturnAPI4(FTFont*, EngineNS, FTFontManager, GetFont, const char*, int, int, int);
	CSharpReturnAPI0(const char*, EngineNS, FTFont, GetName);
	CSharpReturnAPI0(int, EngineNS, FTFont, GetFontSize);
	CSharpAPI5(EngineNS, FTFont, Draw2TextContext, const WCHAR*, int, int, FTTextDrawContext*, bool);
	CSharpAPI3(EngineNS, FTFont, MeasureString, const WCHAR*, int*, int*);
	CSharpReturnAPI4(int, EngineNS, FTFont, CheckPointChar, const WCHAR*, int, int, int*);
	CSharpAPI0(EngineNS, FTFont, UpdateHotWords);
	CSharpReturnAPI5(int, EngineNS, FTFont, CalculateWrap, const WCHAR*, int*, int, int, int*);
	CSharpReturnAPI5(int, EngineNS, FTFont, CalculateWrapWithWord, const WCHAR*, int*, int, int, int*);

	CSharpReturnAPI0(UINT, EngineNS, FTTextDrawContext, GetDrawCall);
	CSharpReturnAPI3(GfxMeshPrimitives*, EngineNS, FTTextDrawContext, BuildMesh, IRenderContext*, IShaderResourceView**, bool);
	CSharpReturnAPI1(vBOOL, EngineNS, FTTextDrawContext, IsValidVersion, FTFont*);
	CSharpAPI1(EngineNS, FTTextDrawContext, RebuildContext, FTFont*);
	CSharpAPI0(EngineNS, FTTextDrawContext, ResetContext);
	CSharpAPI4(EngineNS, FTTextDrawContext, SetClip, int, int, int, int);
}