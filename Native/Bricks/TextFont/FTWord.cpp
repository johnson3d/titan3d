#include "FTWord.h"
#include "FTFont.h"

#define new VNEW

NS_BEGIN

void ITextTexture::Init(int size, int fontSize)
{
	TextureSize = size;
	FontSize = fontSize;
	CellNum = TextureSize / FontSize;
	Words.resize(CellNum*CellNum);
	WordNumber = 0;
	Dirty = false;
}

void ITextTexture::SetWord(int index, FTWord* word) 
{
	word->IndexInTexture = index;
	Words[index] = word;
	Dirty = true;
}

void ITextTexture::WriteWord(int x, int y, FTWord* word, BYTE* pPixels)
{
	BYTE* pTarget = &pPixels[(y * FontSize) * TextureSize + x * FontSize];
	for (int i = 0; i < word->PixelHeight; i++)
	{
		auto src = &word->Pixels[i*word->PixelWidth];
		memcpy(pTarget, src, sizeof(BYTE)*word->PixelWidth);
		pTarget += TextureSize;
	}
}

void ITextTexture::UpdateTexture(IRenderContext* rc, bool bflipV)
{
	if (Dirty == false)
		return;

	AutoRef<IBlobObject> pixelBlob = new IBlobObject();
	pixelBlob->ReSize(sizeof(BYTE) * TextureSize * TextureSize);
	BYTE* pPixels = (BYTE*)&pixelBlob->mDatas[0];
	memset(pPixels, 0, TextureSize * TextureSize * sizeof(BYTE));
	
	for(size_t i = 0; i<Words.size(); i++)
	{
		if(Words[i]==nullptr)
			continue;
		auto row = (int)(i / CellNum);
		auto col = (int)(i % CellNum);
		WriteWord(col, row, Words[i], pPixels);
	}

	if (bflipV) 
	{
		for (int i = 0; i < TextureSize/2; i++)
		{
			//DWORD* t = &pPixels[i*TextureSize];
			//DWORD* b = &pPixels[(TextureSize-1-i)*TextureSize];
			for (int j = 0; j < TextureSize; j++)
			{
				/*auto alpha = (BYTE)(t[j] >> 24);
				t[j] = alpha | (alpha << 8) | (alpha << 16) | (alpha << 24);

				alpha = (BYTE)(b[j] >> 24);
				b[j] = alpha | (alpha << 8) | (alpha << 16) | (alpha << 24);*/

				//std::swap(t[j], b[j]);
			}
		}
	}
	
	//if (RHITexture == nullptr)
	{
		ITexture2DDesc texDesc;
		texDesc.Format = PXF_R8_UNORM;
		texDesc.Width = TextureSize;
		texDesc.Height = TextureSize;
		texDesc.InitData.pSysMem = pixelBlob;
		texDesc.InitData.SysMemPitch = TextureSize * sizeof(BYTE);
		texDesc.InitData.SysMemSlicePitch = 1;
		AutoRef<ITexture2D> tex2D = rc->CreateTexture2D(&texDesc);
		IShaderResourceViewDesc srvDesc;
		srvDesc.m_pTexture2D = tex2D;
		RHITexture = rc->CreateShaderResourceView(&srvDesc);

		tex2D->mDesc.InitData.pSysMem = nullptr;
	}
	Dirty = false;
	//delete[] pPixels;
}

void FTWord::FillVertices(FTTextDrawContext* ctx, int x, int y, ITextTexture* texture, std::vector<FTVertex>& verts, bool flipV)
{
	x += PixelX;
	y += (FontSize - PixelY);
	//auto step = (float)texture->TextureSize / (float)texture->CellNum;
	float osLeft = 0;
	//float osTop = 0;
	float osRight = 0;
	//float osBottom = 0;
	if (ctx->ClipWidth > 0 && ctx->ClipHeight > 0)
	{
		//if (x + AdvanceX <= ctx->ClipX ||
		//	y + FontSize <= ctx->ClipY ||
		//	x >= ctx->ClipX + ctx->ClipWidth ||
		//	y >= ctx->ClipY + ctx->ClipHeight)
		//{
		//	return;
		//}
		//else if (x > ctx->ClipX &&
		//	y > ctx->ClipY &&
		//	x + AdvanceX < ctx->ClipX + ctx->ClipWidth &&
		//	y + FontSize < ctx->ClipY + ctx->ClipHeight
		//	)
		//{

		//}
		//else
		//{
		//	if (ctx->ClipX > x)
		//	{
		//		osLeft = ctx->ClipX - x;
		//	}
		//	if (ctx->ClipX + ctx->ClipWidth < x + PixelWidth)
		//	{
		//		osRight = ctx->ClipX + ctx->ClipWidth - (x + PixelWidth);
		//	}
		//	/*if (ctx->ClipY > y)
		//	{
		//		osTop = ctx->ClipY - y;
		//	}*/
		//}
	}
	auto sv = (float)(IndexInTexture / texture->CellNum) * FontSize;
	auto su = (float)(IndexInTexture % texture->CellNum) * FontSize;
	auto texSize = (float)texture->TextureSize;

	FTVertex v;
	v.PosUV.x = (float)x + osLeft;
	v.PosUV.y = (float)y;
	v.PosUV.z = (su + osLeft) / texSize;//u
	v.PosUV.w = (sv) / texSize;//v
	verts.push_back(v);
	v.PosUV.x = (float)x + PixelWidth - osRight;
	v.PosUV.y = (float)y;
	v.PosUV.z = (su + PixelWidth - osRight) / texSize;//u
	v.PosUV.w = (sv) / texSize;//v
	verts.push_back(v);
	v.PosUV.x = (float)x + PixelWidth - osRight;
	v.PosUV.y = (float)y + PixelHeight;
	v.PosUV.z = (su + PixelWidth - osRight) / texSize;//u
	v.PosUV.w = (sv + PixelHeight) / texSize;//v
	verts.push_back(v);

	v.PosUV.x = (float)x + osLeft;
	v.PosUV.y = (float)y;
	v.PosUV.z = (su + osLeft) / texSize;//u
	v.PosUV.w = (sv) / texSize;//v
	verts.push_back(v);
	v.PosUV.x = (float)x + PixelWidth - osRight;
	v.PosUV.y = (float)y + PixelHeight;
	v.PosUV.z = (su + PixelWidth - osRight) / texSize;//u
	v.PosUV.w = (sv + PixelHeight) / texSize;//v
	verts.push_back(v);
	v.PosUV.x = (float)x + osLeft;
	v.PosUV.y = (float)y + PixelHeight;
	v.PosUV.z = (su + osLeft) / texSize;//u
	v.PosUV.w = (sv + PixelHeight) / texSize;//v
	verts.push_back(v);
}

const int MAX_DISTANCE = 512;//�����ٷ�Χ�ڵ�����
const BYTE PixelColored = 127;
double max_distance = -MAX_DISTANCE;
double min_distance = MAX_DISTANCE;

void SetPixel(int x, int y, int input_width, int input_height, int output_width, int output_height, BYTE* inBuffer, double* outBuffer)
{
	int source_is_inside, target_is_inside, cx, cy, ix, iy, dx, dy, im;
	int minx, miny, maxx, maxy;
	double min, distance;

	cx = (x * input_width) / output_width;
	cy = (y * input_height) / output_height;

	min = MAX_DISTANCE;

	minx = cx - MAX_DISTANCE;
	if (minx < 0) {
		minx = 0;
	}
	miny = cy - MAX_DISTANCE;
	if (miny < 0) {
		miny = 0;
	}
	maxx = cx + MAX_DISTANCE;
	if (maxx > (int)input_width) {
		maxx = input_width;
	}
	maxy = cy + MAX_DISTANCE;
	if (maxy > (int)input_height) {
		maxy = input_height;
	}

	source_is_inside = inBuffer[cx + cy * input_width] > PixelColored;
	if (source_is_inside) {
		for (iy = miny; iy < maxy; iy++) {
			dy = iy - cy;
			dy *= dy;
			im = iy * input_width;
			for (ix = minx; ix < maxx; ix++) {
				target_is_inside = inBuffer[ix + im] > PixelColored;
				if (target_is_inside) {
					continue;
				}
				dx = ix - cx;
				distance = sqrt(dx * dx + dy);
				if (distance < min) {
					min = distance;
				}
			}
		}

		if (min > max_distance) {
			max_distance = min;
		}

		outBuffer[x + y * output_width] = min;
	}
	else {
		for (iy = miny; iy < maxy; iy++) {
			dy = iy - cy;
			dy *= dy;
			im = iy * input_width;
			for (ix = minx; ix < maxx; ix++) {
				target_is_inside = inBuffer[ix + im] > PixelColored;
				if (!target_is_inside) {
					continue;
				}
				dx = ix - cx;
				distance = sqrt(dx * dx + dy);
				if (distance < min) {
					min = distance;
				}
			}
		}

		if (-min < min_distance) {
			min_distance = -min;
		}

		outBuffer[x + y * output_width] = -min;
	}
}

FTWord* FTWord::BuildAsSDF(int w, int h)
{
	//ʹ��open mp����for���٣���˵�Ѿ��õ��󲿷�ϵͳ�ͱ�������֧�֣�����ndk��
#pragma omp for
	for (int oy = 0; oy < 32; oy++) {
		for (int ox = 0; ox < 32; ox++) {
			SetPixel(ox, oy, 1024, 1024, 32, 32, nullptr, nullptr);
		}
	}
	return nullptr;
}

NS_END