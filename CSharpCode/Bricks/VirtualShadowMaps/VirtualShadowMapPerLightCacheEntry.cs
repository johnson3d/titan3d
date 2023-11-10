using System;
using System.Collections;
using System.Diagnostics;
using System.Collections.Generic;
using System.Text;
using NPOI.SS.Formula.Functions;

namespace EngineNS.Bricks.VirtualShadowMaps
{
    public struct TtInstanceRange
    {
        public Int32 InstanceSceneDataOffset;
        public Int32 NumInstanceSceneDataEntries;
        public bool bInvalidateStaticPage;
    };

    public class TtVirtualShadowMapPerLightCacheEntry
    {
        public bool bPrevIsDistantLight = false;
        public Int32 PrevRenderedFrameNumber = -1;
        public Int32 PrevScheduledFrameNumber = -1;

        public bool bCurrentIsDistantLight = false;
        public Int32 CurrentRenderedFrameNumber = -1;
        public Int32 CurrenScheduledFrameNumber = -1;//TODO..

        // Primitives that have been rendered (not culled) the previous frame, when a primitive transitions from being culled to not it must be rendered into the VSM
        // Key culling reasons are small size or distance cutoff.
        public BitArray RenderedPrimitives;

        // Primitives that have been rendered (not culled) _some_ previous frame, tracked so we can invalidate when they move/are removed (and not otherwise).
        public BitArray CachedPrimitives;

        public List<TtVirtualShadowMapCacheEntry> ShadowMapEntries = new List<TtVirtualShadowMapCacheEntry>();

        public List<TtInstanceRange> PrimitiveInstancesToInvalidate = new List<TtInstanceRange>();

        public TtVirtualShadowMapPerLightCacheEntry(Int32 MaxPersistentScenePrimitiveIndex)
        {

            RenderedPrimitives = new BitArray(MaxPersistentScenePrimitiveIndex);
            RenderedPrimitives.SetAll(false);

            CachedPrimitives = new BitArray(MaxPersistentScenePrimitiveIndex);
            CachedPrimitives.SetAll(false);
        }

        public TtVirtualShadowMapCacheEntry FindCreateShadowMapEntry(Int32 Index)
        {
            Debug.Assert(Index >= 0);

            if (ShadowMapEntries.Count > Index)
            {
                for (int i = 0; i < (Index - ShadowMapEntries.Count); i++)
                {
                    ShadowMapEntries.Add(new TtVirtualShadowMapCacheEntry());
                }
            }

            return ShadowMapEntries[Index];
        }
    }
}
