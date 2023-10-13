using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.VirtualShadowMaps
{
    public class TTVirtualShadowMapCacheManager
    {
        public struct TtVSMClipmapInfo
        {
            public Matrix WorldToLight;
            public double ViewCenterZ;
            public double ViewRadiusZ;

            // Previous frame data
            public Point PrevClipmapCornerOffset;

            // Current frame data
            public Point CurrentClipmapCornerOffset;

            //public TtVSMClipmapInfo()
            //{
            //}
        }

        public class TtVirtualShadowMapCacheEntry
        {
            // Previous frame data
            public Point PrevPageSpaceLocation = new Point(0, 0);
            public Int32 PrevVirtualShadowMapId = -1;

            // Current frame data
            public Point CurrentPageSpaceLocation = new Point(0, 0);
            public Int32 CurrentVirtualShadowMapId = -1;

            public TtVSMClipmapInfo Clipmap = new TtVSMClipmapInfo();

            public TtVirtualShadowMapCacheEntry()
            {
            }
        }
    }
}
