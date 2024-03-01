using NPOI.SS.Formula.Functions;
using System;
using System.Collections.Generic;
using System.Text;

using EngineNS.Graphics.Pipeline;


namespace EngineNS.Bricks.VirtualShadowMaps
{
    public struct TtVirtualShadowMapArrayFrameData
    {
        public TtAttachBuffer PageTable;
        public TtAttachBuffer PageFlags;

        public TtAttachBuffer ProjectionData;
        public TtAttachBuffer PageRectBounds;

        public TtAttachBuffer PhysicalPageMetaData;

        // Need HZBPhysical TODO..
        //TMap<int32, FVirtualShadowMapHZBMetadata> HZBMetadata; TODO..

        // Maybe need GetGPUSizeBytes..
    };

    public class VirtualShadowMapArrayCacheManager
    {
        public UInt32 MaxStatFrames = 512*1024U;
    }
}
