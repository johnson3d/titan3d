#pragma once

#include "../../../3rd/native/FreeType/include/ft2build.h"
#include "../../../3rd/native/FreeType/include/freetype/freetype.h"
#include "../../../3rd/native/FreeType/include/freetype/ftglyph.h"
#include "../../Base/IUnknown.h"
#include "../../Math/v3dxQuaternion.h"
#include "../../NextRHI/NxBuffer.h"
#include "../../NextRHI/NxGeomMesh.h"
#include "../../Base/allocator/PagedAllocator.h"
#include "FCanvasBrush.h"

NS_BEGIN

namespace Canvas
{
	struct FTWord;
	class FTFont;

	struct FTVertex
	{
		v3dxQuaternion		PosUV;
	};

	struct TR_CLASS()
		ITextBrush : public ICanvasBrush
	{
		ENGINE_RTTI(ITextBrush);
		ITextBrush()
		{
			FontSize = 0;
			Name = VNameString("@Text:");
		}
		void Init(int sizeX, int sizeY, int fontSize);
		int							TextureSizeX = 0;
		int							TextureSizeY = 0;
		int							FontSize = 0;
		
		int							HitCount = 0;
		std::vector<FTWord*>		Words;
		
		bool PutWord(FTWord * word);
	};

	struct TR_CLASS()
		FTWord : public VIUnknown
	{
		FTWord()
		{
			UniCode = 0;
			FontSize = 0;
			HitCount = 0;
			PixelWidth = 0;
			PixelHeight = 0;
			Dirty = true;
		}
		UINT				UniCode = 0;
		int					FontSize = 0;
		std::vector<BYTE>	Pixels;
		int					PixelX = 0;
		int					PixelY = 0;
		int					PixelWidth = 0;
		int					PixelHeight = 0;
		v3dxVector2			Advance;
		float				SdfScale = 1.0f;

		int					HitCount = 0;
		AutoRef<ITextBrush>	Brush;
		FTFont*				Font = nullptr;
		bool				Dirty = true;
		int					TexX = -1;
		int					TexY = -1;
		void SetDirty();
		void Flush2Texture(NxRHI::IGpuDevice* device, NxRHI::ICommandList* cmdlist);
		BYTE* GetBufferPtr() {
			if (Pixels.size() == 0)
				return nullptr;
			return &Pixels[0];
		}
		void FillVertices(int x, int y, ITextBrush * texture, FTVertex verts[6], bool flipV);
		void FillCanvasVertices(int x, int y, UInt16 transformIndex, ITextBrush * texture, void* quatVerts);
		void BuildMesh(int x, int y, UInt16 transformIndex, ITextBrush * texture, NxRHI::FMeshDataProvider * mesh);

		FTWord* BuildAsSDF(int fontSize, int w, int h);
		FTWord* BuildAsSDFFast(int fontSize, int w, int h, BYTE PixelColored, BYTE spread);
	};

	struct FTPagedWord : public MemAlloc::FPagedObject<AutoRef<FTWord>>
	{

	};

	struct FTPagedWordCreator
	{
		struct FTWordPage : public MemAlloc::FPage<AutoRef<FTWord>>
		{
			AutoRef<ITextBrush>		Brush;
		};

		using ObjectType = AutoRef<FTWord>;
		using PagedObjectType = MemAlloc::FPagedObject<ObjectType>;
		using PageType = FTWordPage;// MemAlloc::FPage<ObjectType>;
		using AllocatorType = MemAlloc::FAllocatorBase<ObjectType>;

		void Initialize(NxRHI::IGpuDevice* device, int fontSize, int textureW, int textureH);

		UINT GetPageSize() const {
			return mNumCellX * mNumCellY;
		}
		PageType* CreatePage(UINT pageSize);
		PagedObjectType* CreatePagedObject(PageType* page, UINT index);
		void OnAlloc(AllocatorType* pAllocator, PagedObjectType* obj);
		void OnFree(AllocatorType* pAllocator, PagedObjectType* obj);
		void FinalCleanup(MemAlloc::FPage<ObjectType>* page);
	protected:
		UINT									mCellSizeX = 0;
		UINT									mCellSizeY = 0;
		UINT									mNumCellX = 0;
		UINT									mNumCellY = 0;
		NxRHI::FTextureDesc						mTextureDesc{};
		TWeakRefHandle<NxRHI::IGpuDevice>		mDeviceRef;
	};

	struct FTPagedWordAllocator : public MemAlloc::FPagedObjectAllocator<FTPagedWordCreator::ObjectType, FTPagedWordCreator>
	{
		void Update(NxRHI::IGpuDevice* device, bool bflipV);
	};
}

NS_END