using NPOI.SS.Formula.Functions;
using Org.BouncyCastle.Math.EC;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace EngineNS.Bricks.VirtualShadowMaps
{
    public struct TtVirtualShadowMapHZBMetadata
    {
        public Matrix ViewMatrices; // TODO
        public Rect ViewRect;
        public Int32 TargetLayerIndex;
    }

    public class TtVirtualShadowMap
    {

	    public static UInt32 PageSize = 128U;
	    public static UInt32 Level0DimPagesXY = 128U;

	    public static UInt32 PageSizeMask = PageSize - 1U;
	    public static UInt32 Log2PageSize = MathHelper.ILog2Const(PageSize);
        public static UInt32 Log2Level0DimPagesXY = MathHelper.ILog2Const(Level0DimPagesXY);
        public static UInt32 MaxMipLevels = Log2Level0DimPagesXY + 1U;

	    public static UInt32 PageTableSize = CalcVirtualShadowMapLevelOffsets(MaxMipLevels, Log2Level0DimPagesXY);

        public static UInt32 VirtualMaxResolutionXY = Level0DimPagesXY* PageSize;

        public static UInt32 PhysicalPageAddressBits = 16U;
	    public static UInt32 MaxPhysicalTextureDimPages = (UInt32)(1 << (Int32)PhysicalPageAddressBits);
	    public static UInt32 MaxPhysicalTextureDimTexels = MaxPhysicalTextureDimPages* PageSize;

        public static UInt32 NumHZBLevels = Log2PageSize;

	    public static UInt32 RasterWindowPages = 4u;
	


        public Int32 ID;
        public bool bIsSinglePageSM;

        //public TTVirtualShadowMapCacheManager.TtVirtualShadowMapCacheEntry[] VirtualShadowMapCacheEntry; //TODO..

        public TtVirtualShadowMap()
        {
            ID = -1;
            bIsSinglePageSM = false;
            
        }

        public static UInt32 CalcVirtualShadowMapLevelOffsets(UInt32 Level, UInt32 Log2Level0DimPagesXY)
        {
            UInt32 NumBits = Level << 1;
            UInt32 StartBit = (2U * Log2Level0DimPagesXY + 2U) - NumBits;
            UInt32 Mask = (UInt32)(((1 << (int)NumBits) - 1) << (int)StartBit);
            return 0x55555555U & Mask;
        }
    };

    public class UVirtualShadowMapArray
    {
    }
}
