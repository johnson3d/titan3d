#include "FTWord.h"
#include "FTFont.h"
#include "../../NextRHI/NxGpuDevice.h"
#include "../../NextRHI/NxCommandList.h"
#include "../../NextRHI/NxDrawcall.h"
#include "../../Base/sdf/sdf_8ssedt.h"
#include "../Canvas/FCanvas.h"
#include "FDrawCmdList.h"

#define new VNEW

NS_BEGIN

ENGINE_RTTI_IMPL(ITextBrush);

StructBegin(ITextBrush, EngineNS::Canvas)
StructEnd(Canvas::ITextBrush, Canvas::ICanvasBrush)

namespace Canvas
{
	void ITextBrush::Init(int sizeX, int sizeY, int fontSize)
	{
		TextureSizeX = sizeX;
		TextureSizeY = sizeY;
		FontSize = fontSize;
		ASSERT(sizeX >= FontSize && sizeY >= FontSize);
		Words.resize((sizeX / FontSize) * (sizeY / FontSize));
	}

	bool ITextBrush::PutWord(FTWord* word)
	{
		Words.push_back(word);
		return true;
	}

	void FTWord::FillCanvasVertices(int x, int y, UInt16 transformIndex, ITextBrush* texture, void* vert)
	{
		Canvas::FCanvasVertex* quatVerts = (Canvas::FCanvasVertex*)vert;
		x += PixelX;
		y += (PixelY);
		//auto step = (float)texture->TextureSize / (float)texture->CellNum;
		float osLeft = 0;
		//float osTop = 0;
		float osRight = 0;
		auto sv = (float)TexY;
		auto su = (float)TexX;
		auto texSizeX = (float)texture->TextureSizeX;
		auto texSizeY = (float)texture->TextureSizeY;

		//struct FTmpVertex
		//{
		//	v3dxVector3 Pos;
		//	v3dxVector2 UV;
		//	DWORD Color;
		//	DWORD Index;
		//};

		v3dxVector3 pos;
		quatVerts[Canvas::RCN_X0_Y0].Pos.z = 0;
		quatVerts[Canvas::RCN_X0_Y0].Pos.x = (float)x + osLeft;
		quatVerts[Canvas::RCN_X0_Y0].Pos.y = (float)y;
		quatVerts[Canvas::RCN_X0_Y0].UV.x = (su + osLeft) / texSizeX;//u
		quatVerts[Canvas::RCN_X0_Y0].UV.y = (sv) / texSizeY;//v
		quatVerts[Canvas::RCN_X0_Y0].Color.MakeValue(0xff, 0xff, 0xff, 0xff, 1);
		FCanvasDrawCmdList::TransformIndexToColor(&transformIndex, quatVerts[Canvas::RCN_X0_Y0].Index);

		quatVerts[Canvas::RCN_X1_Y0].Pos.z = 0;
		quatVerts[Canvas::RCN_X1_Y0].Pos.x = (float)x + PixelWidth - osRight;
		quatVerts[Canvas::RCN_X1_Y0].Pos.y = (float)y;
		quatVerts[Canvas::RCN_X1_Y0].UV.x = (su + PixelWidth - osRight) / texSizeX;//u
		quatVerts[Canvas::RCN_X1_Y0].UV.y = (sv) / texSizeY;//v
		quatVerts[Canvas::RCN_X1_Y0].Color.MakeValue(0xff, 0xff, 0xff, 0xff, 1);
		FCanvasDrawCmdList::TransformIndexToColor(&transformIndex, quatVerts[Canvas::RCN_X1_Y0].Index);

		quatVerts[Canvas::RCN_X1_Y1].Pos.z = 0;
		quatVerts[Canvas::RCN_X1_Y1].Pos.x = (float)x + PixelWidth - osRight;
		quatVerts[Canvas::RCN_X1_Y1].Pos.y = (float)y + PixelHeight;
		quatVerts[Canvas::RCN_X1_Y1].UV.x = (su + PixelWidth - osRight) / texSizeX;//u
		quatVerts[Canvas::RCN_X1_Y1].UV.y = (sv + PixelHeight) / texSizeY;//v
		quatVerts[Canvas::RCN_X1_Y1].Color.MakeValue(0xff, 0xff, 0xff, 0xff, 1);
		FCanvasDrawCmdList::TransformIndexToColor(&transformIndex, quatVerts[Canvas::RCN_X1_Y1].Index);

		quatVerts[Canvas::RCN_X0_Y1].Pos.z = 0;
		quatVerts[Canvas::RCN_X0_Y1].Pos.x = (float)x + osLeft;
		quatVerts[Canvas::RCN_X0_Y1].Pos.y = (float)y + PixelHeight;
		quatVerts[Canvas::RCN_X0_Y1].UV.x = (su + osLeft) / texSizeX;//u
		quatVerts[Canvas::RCN_X0_Y1].UV.y = (sv + PixelHeight) / texSizeY;//v
		quatVerts[Canvas::RCN_X0_Y1].Color.MakeValue(0xff, 0xff, 0xff, 0xff, 1);
		FCanvasDrawCmdList::TransformIndexToColor(&transformIndex, quatVerts[Canvas::RCN_X0_Y1].Index);
	}

	void FTWord::BuildMesh(int x, int y, UInt16 transformIndex, ITextBrush* texture, NxRHI::FMeshDataProvider* mesh)
	{
		x += PixelX;
		y += (PixelY);
		//auto step = (float)texture->TextureSize / (float)texture->CellNum;
		float osLeft = 0;
		//float osTop = 0;
		float osRight = 0;
		auto sv = (float)TexY;
		auto su = (float)TexX;
		auto texSizeX = (float)texture->TextureSizeX;
		auto texSizeY = (float)texture->TextureSizeY;

		//struct FTmpVertex
		//{
		//	v3dxVector3 Pos;
		//	v3dxVector2 UV;
		//	DWORD Color;
		//	DWORD Index;
		//};

		Canvas::FCanvasVertex verts[4];
		v3dxVector3 pos;
		verts[0].Pos.z = 0;
		verts[0].Pos.x = (float)x + osLeft;
		verts[0].Pos.y = (float)x + osLeft;
		verts[0].UV.x = (su + osLeft) / texSizeX;//u
		verts[0].UV.y = (sv) / texSizeY;//v
		verts[0].Color = FColor::White;// 0xffffffff;
		FCanvasDrawCmdList::TransformIndexToColor(&transformIndex, verts[0].Index);

		verts[1].Pos.z = 0;
		verts[1].Pos.x = (float)x + PixelWidth - osRight;
		verts[1].Pos.y = (float)y;
		verts[1].UV.x = (su + PixelWidth - osRight) / texSizeX;//u
		verts[1].UV.y = (sv) / texSizeY;//v
		verts[1].Color = FColor::White;//0xffffffff;
		FCanvasDrawCmdList::TransformIndexToColor(&transformIndex, verts[1].Index);

		verts[2].Pos.z = 0;
		verts[2].Pos.x = (float)x + PixelWidth - osRight;
		verts[2].Pos.y = (float)y + PixelHeight;
		verts[2].UV.x = (su + PixelWidth - osRight) / texSizeX;//u
		verts[2].UV.y = (sv + PixelHeight) / texSizeY;//v
		verts[2].Color = FColor::White;//0xffffffff;
		FCanvasDrawCmdList::TransformIndexToColor(&transformIndex, verts[2].Index);

		verts[3].Pos.z = 0;
		verts[3].Pos.x = (float)x + osLeft;
		verts[3].Pos.y = (float)y + PixelHeight;
		verts[3].UV.x = (su + osLeft) / texSizeX;//u
		verts[3].UV.y = (sv + PixelHeight) / texSizeY;//v
		verts[3].Color = FColor::White;//0xffffffff;
		FCanvasDrawCmdList::TransformIndexToColor(&transformIndex, verts[3].Index);

		auto start = mesh->GetVertexNumber();
		mesh->AddVertex_Pos_UV_Color_Index(verts, 4);

		UINT a, b, c;
		a = start;
		b = start + 1;
		c = start + 2;
		mesh->AddTriangle(a, b, c);
		a = start + 1;
		b = start + 2;
		c = start + 3;
		mesh->AddTriangle(a, b, c);
	}

	void FTWord::FillVertices(int x, int y, ITextBrush* texture, FTVertex verts[6], bool flipV)
	{
		x += PixelX;
		y += (PixelY);
		//auto step = (float)texture->TextureSize / (float)texture->CellNum;
		float osLeft = 0;
		//float osTop = 0;
		float osRight = 0;
		//float osBottom = 0;
		//if (ctx->ClipWidth > 0 && ctx->ClipHeight > 0)
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
		auto sv = (float)TexY;
		auto su = (float)TexX;
		auto texSizeX = (float)texture->TextureSizeX;
		auto texSizeY = (float)texture->TextureSizeY;

		FTVertex v;
		v.PosUV.x = (float)x + osLeft;
		v.PosUV.y = (float)y;
		v.PosUV.z = (su + osLeft) / texSizeX;//u
		v.PosUV.w = (sv) / texSizeY;//v
		verts[0] = v;

		v.PosUV.x = (float)x + PixelWidth - osRight;
		v.PosUV.y = (float)y;
		v.PosUV.z = (su + PixelWidth - osRight) / texSizeX;//u
		v.PosUV.w = (sv) / texSizeY;//v
		verts[1] = v;

		v.PosUV.x = (float)x + PixelWidth - osRight;
		v.PosUV.y = (float)y + PixelHeight;
		v.PosUV.z = (su + PixelWidth - osRight) / texSizeX;//u
		v.PosUV.w = (sv + PixelHeight) / texSizeY;//v
		verts[2] = v;

		v.PosUV.x = (float)x + osLeft;
		v.PosUV.y = (float)y;
		v.PosUV.z = (su + osLeft) / texSizeX;//u
		v.PosUV.w = (sv) / texSizeX;//v
		verts[3] = v;

		v.PosUV.x = (float)x + PixelWidth - osRight;
		v.PosUV.y = (float)y + PixelHeight;
		v.PosUV.z = (su + PixelWidth - osRight) / texSizeX;//u
		v.PosUV.w = (sv + PixelHeight) / texSizeY;//v
		verts[4] = v;

		v.PosUV.x = (float)x + osLeft;
		v.PosUV.y = (float)y + PixelHeight;
		v.PosUV.z = (su + osLeft) / texSizeX;//u
		v.PosUV.w = (sv + PixelHeight) / texSizeY;//v
		verts[5] = v;
	}

	void FTWord::SetDirty()
	{
		Dirty = true;
		if (Font != nullptr)
		{
			Font->SetDirty();
		}
	}

	void FTWord::Flush2Texture(NxRHI::IGpuDevice* device, NxRHI::ICommandList* cmdlist)
	{
		if (Dirty == false)
			return;
		Dirty = false;

		if (PixelHeight == 0 || PixelWidth == 0)
			return;

		NxRHI::FBufferDesc bfDesc{};
		bfDesc.SetDefault();
		bfDesc.Type = NxRHI::EBufferType::BFT_NONE;// NxRHI::EBufferType::BFT_SRV;
		bfDesc.Usage = NxRHI::USAGE_STAGING;
		bfDesc.CpuAccess = NxRHI::CAS_WRITE;
		bfDesc.InitData = nullptr;
		bfDesc.RowPitch = device->GetGpuResourceAlignment()->RoundupTexturePitch(PixelWidth * sizeof(BYTE));
		bfDesc.DepthPitch = bfDesc.RowPitch * PixelHeight;
		bfDesc.StructureStride = 0;
		bfDesc.Size = bfDesc.RowPitch * PixelHeight;
		auto buffer = MakeWeakRef(device->CreateBuffer(&bfDesc));
		NxRHI::FMappedSubResource mapped{};
		if (buffer->Map(0, &mapped, false))
		{
			auto pWrite = (BYTE*)mapped.pData;
			for (int i = 0; i < PixelHeight; i++)
			{
				memcpy(pWrite, &Pixels[i * PixelWidth], PixelWidth);
				pWrite += mapped.RowPitch;
			}
			buffer->Unmap(0);
		}
		
		if (0)
		{
			NxRHI::FSubResourceFootPrint footPrint{};
			footPrint.Format = EPixelFormat::PXF_A8_UNORM;
			footPrint.X = TexX;
			footPrint.Y = TexY;
			footPrint.Z = 0;
			footPrint.Width = PixelWidth;
			footPrint.Height = PixelHeight;
			footPrint.Depth = 1;
			footPrint.RowPitch = bfDesc.RowPitch;
			cmdlist->GetCmdRecorder()->UseResource(buffer);
			cmdlist->GetCmdRecorder()->UseResource(Brush->SrView->Buffer);
			cmdlist->CopyBufferToTexture(Brush->SrView->Buffer.UnsafeConvertTo<NxRHI::ITexture>(), 0, buffer, &footPrint);
		}
		else
		{
			AutoRef<NxRHI::ICopyDraw> cpDraw = MakeWeakRef(device->CreateCopyDraw());
			cpDraw->BindTextureDest(Brush->SrView->Buffer);
			cpDraw->BindBufferSrc(buffer);
			cpDraw->DestSubResource = 0;
			cpDraw->Mode = NxRHI::ECopyDrawMode::CDM_Buffer2Texture;
			cpDraw->FootPrint.Format = EPixelFormat::PXF_A8_UNORM;
			cpDraw->FootPrint.X = TexX;
			cpDraw->FootPrint.Y = TexY;
			cpDraw->FootPrint.Z = 0;
			cpDraw->FootPrint.Width = PixelWidth;
			cpDraw->FootPrint.Height = PixelHeight;
			cpDraw->FootPrint.Depth = 1;
			cpDraw->FootPrint.RowPitch = bfDesc.RowPitch;
			cpDraw->FootPrint.TotalSize = bfDesc.RowPitch * PixelHeight;

			cmdlist->PushGpuDraw(cpDraw);
		}
	}

	struct FSDFBuilder
	{
		FSDFBuilder(int maxDist, BYTE pxlClr = 127)
		{
			MAX_DISTANCE = maxDist;
			max_distance = -MAX_DISTANCE;
			min_distance = MAX_DISTANCE;
			PixelColored = pxlClr;
		}
		int MAX_DISTANCE = 512;
		BYTE PixelColored = 127;
		std::atomic<double> max_distance = -MAX_DISTANCE;
		std::atomic<double> min_distance = MAX_DISTANCE;
		VSLLock maxWriteLocker;
		VSLLock minWriteLocker;
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

				//max_distance.compare_exchange_strong(min, min);
				{
					VAutoVSLLock locker(maxWriteLocker);
					if (min > max_distance) {
						max_distance = min;
					}
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

				{
					VAutoVSLLock locker(minWriteLocker);
					if (-min < min_distance) {
						min_distance = -min;
					}
				}

				outBuffer[x + y * output_width] = -min;
			}
		}
	};

	FTWord* FTWord::BuildAsSDF(int fontSize, int w, int h)
	{
		auto MaxDistance = (int)sqrt(PixelWidth * PixelWidth + PixelHeight * PixelHeight);
		double* pSDFBuffer = new double[w * h];
		FSDFBuilder builder(MaxDistance, 127u);
#pragma omp for
		for (int oy = 0; oy < h; oy++)
		{
			for (int ox = 0; ox < w; ox++)
			{
				builder.SetPixel(ox, oy, PixelWidth, PixelHeight, w, h, &Pixels[0], pSDFBuffer);
			}
		}
		FTWord* result = new FTWord();
		result->SdfScale = (float)FontSize / (float)result->FontSize;
		result->UniCode = UniCode;
		result->FontSize = fontSize;
		result->PixelX = PixelX * FontSize / result->FontSize;
		result->PixelY = PixelY * FontSize / result->FontSize;
		result->Advance = Advance * (float)FontSize / (float)result->FontSize;
		result->PixelWidth = w;
		result->PixelHeight = h;
		result->Pixels.resize(w * h);
		double range = builder.max_distance - builder.min_distance;
#pragma omp for
		for (int oy = 0; oy < h; oy++)
		{
			for (int ox = 0; ox < w; ox++)
			{
				result->Pixels[oy * w + ox] = (BYTE)((pSDFBuffer[oy * w + ox] - builder.min_distance) * 255 / range);
			}
		}

		delete[] pSDFBuffer;
		return result;
	}

	bool FTWord::BuildAsSDFFast(FTWord* result, BYTE PixelColored, BYTE spread)
	{
		float maxDist = (float)sqrt((float)PixelWidth * PixelWidth + (float)PixelHeight * PixelHeight);
		SdfGrid grid1;
		SdfGrid grid2;
		grid1.InitGrid(PixelWidth, PixelHeight);
		grid2.InitGrid(PixelWidth, PixelHeight);
		for (int y = 0; y < PixelHeight; y++)
		{
			for (int x = 0; x < PixelWidth; x++)
			{
				auto source_is_inside = Pixels[x + y * PixelWidth] >= PixelColored;
				if (source_is_inside)
				{
					grid1.SetInside(x, y);
					grid2.SetEmpty(x, y);
				}
				else
				{
					grid1.SetEmpty(x, y);
					grid2.SetInside(x, y);
				}
			}
		}
		grid1.GenerateSDF();
		grid2.GenerateSDF();
		v3dxVector2 range1;
		v3dxVector2 range2;
		grid1.UpdateDistanceValues(range1.x, range1.y);
		grid2.UpdateDistanceValues(range2.x, range2.y);

		result->SdfScale = (float)result->FontSize / (float)FontSize;
		result->UniCode = UniCode;
		result->PixelX = PixelX * result->FontSize / FontSize;
		result->PixelY = PixelY * result->FontSize / FontSize;
		result->Advance = Advance * (float)result->FontSize / (float)FontSize;
		result->PixelWidth = PixelWidth * result->FontSize / FontSize;
		result->PixelHeight = PixelHeight * result->FontSize / FontSize;
		int w = result->PixelWidth;
		int h = result->PixelHeight;
		result->Pixels.resize(w * h);
		//range1.x = 0;
		//range1.y = 128;//size = 64
		float delta1 = range1.y - range1.x;
		float delta2 = range2.y - range2.x;

		int sx = FontSize / (result->FontSize * 2);
		int sy = FontSize / (result->FontSize * 2);
#pragma omp for
		for (int y = 0; y < h; y++)
		{
			auto oy = y * PixelHeight / h + sy;
			for (int x = 0; x < w; x++)
			{
				auto ox = x * PixelWidth / w + sx;
				const auto& slt1 = grid1.Get(ox, oy);
				const auto& slt2 = grid2.Get(ox, oy);

				auto source_is_inside = Pixels[ox + oy * PixelWidth] >= PixelColored;
				if (source_is_inside)
				{
					float dv = slt2.DistanceValue * result->SdfScale;
					dv = std::min(dv, (float)spread);
					//float d = slt2.DistanceValue * 127.0f / maxDist;
					float d = dv * 127.0f / spread;
					result->Pixels[y * w + x] = (BYTE)(d + 127.0f);
				}
				else
				{
					float dv = slt1.DistanceValue * result->SdfScale;
					dv = std::min(dv, (float)spread);
					float d = dv * 127.0f / spread;
					result->Pixels[y * w + x] = (BYTE)(127.0f - d);
				}
			}
		}
		return result;
	}

	void FTPagedWordCreator::Initialize(NxRHI::IGpuDevice* device, int fontSize, int textureW, int textureH)
	{
		mDeviceRef.FromObject(device);

		mTextureDesc.SetDefault();
		mTextureDesc.Width = textureW;
		mTextureDesc.Height = textureH;
		mTextureDesc.Format = PXF_A8_UNORM;

		mCellSizeX = fontSize;
		mCellSizeY = fontSize;
		mNumCellX = textureW / mCellSizeX;
		mNumCellY = textureH / mCellSizeY;
	}
	FTPagedWordCreator::PageType* FTPagedWordCreator::CreatePage(UINT pageSize)
	{
		auto result = new FTPagedWordCreator::PageType();

		auto device = mDeviceRef.GetPtr();
		result->Brush = MakeWeakRef(new ITextBrush());
		result->Brush->Init(mTextureDesc.Width, mTextureDesc.Height, mCellSizeY);
		auto pTexture = MakeWeakRef(device->CreateTexture(&mTextureDesc));
		NxRHI::FSrvDesc desc{};
		desc.SetTexture2D();
		desc.Format = mTextureDesc.Format;
		desc.Texture2D.MipLevels = 1;
		result->Brush->SrView = MakeWeakRef(device->CreateSRV(pTexture, &desc));

		return result;
	}
	FTPagedWordCreator::PagedObjectType* FTPagedWordCreator::CreatePagedObject(PageType* page, UINT index)
	{
		auto pAllocator = page->Allocator.GetCastPtr<FTPagedWordAllocator>();

		auto result = new FTPagedWord();
		auto pWord = MakeWeakRef(new FTWord());
		result->RealObject = pWord;
		int x = index % mNumCellX;
		int y = index / mNumCellX;
		pWord->TexX = x * mCellSizeX;
		pWord->TexY = y * mCellSizeY;
		pWord->Brush = ((FTWordPage*)page)->Brush;
		pWord->FontSize = mCellSizeY;
		
		return result;
	}
	void FTPagedWordCreator::OnAlloc(AllocatorType* pAllocator, PagedObjectType* obj)
	{

	}
	void FTPagedWordCreator::OnFree(AllocatorType* pAllocator, PagedObjectType* obj)
	{

	}
	void FTPagedWordCreator::FinalCleanup(MemAlloc::FPage<ObjectType>* page)
	{

	}

	void FTPagedWordAllocator::Update(NxRHI::IGpuDevice* device, bool bflipV)
	{
		for (auto& i : Pages)
		{
			auto pPage = i.UnsafeConvertTo<FTPagedWordCreator::PageType>();
			//pPage->Brush->Words
		}
	}
}


NS_END