using NPOI.SS.Formula.Functions;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.VirtualShadowMaps
{
    public struct TtVSMSceneData
    {
        public TtVirtualShadowMap VirtualShadowMap;
        public Matrix ViewToClip;
        public Vector3 WorldCenter;
        //Offset from (0,0) to clipmap corner, in level radii
        public Point CornerOffsetTile;
        public Point CornerOffsetPosition;

        //Offset from LastLevel-snapped WorldCenter to clipmap corner, in level radii
        public Point RelativeCornerOffset;
    };

    public class VirtualShadowMapClipmap
    {
        /** Origin of the clipmap in world space
        * Usually aligns with the camera position from which it was created.
        * Note that the centers of each of the levels can be different as they are snapped to page alignment at their respective scales
        * */
       public Vector3 WorldOrigin;

        /** Directional light rotation matrix (no translation) */
        public Matrix WorldToLightViewRotationMatrix;

        public Int32 FirstLevel;
        public float ResolutionLodBias;
        public float MaxRadius;

        public List<TtVSMSceneData> LevelData =new List<TtVSMSceneData>();

        public BoundingSphere BoundingSphere;
        public TtConvexVolume ViewFrustumBounds;

        public TtVirtualShadowMapPerLightCacheEntry PerLightCacheEntry ;
    }
}
