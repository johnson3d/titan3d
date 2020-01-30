using System;
using System.Collections.Generic;
using System.Text;
using EngineNS.GamePlay.Actor;
using EngineNS.GamePlay.Component;
using EngineNS.Graphics;

namespace EngineNS.GamePlay.SceneGraph
{
    [Rtti.MetaClass]
    public class GPvsCell : IO.Serializer.Serializer
    {
        [Rtti.MetaData]
        public BoundingBox BoundVolume;
        [Rtti.MetaData]
        public int PvsIndex;

        public GPvsSet HostSet;

        public CGfxCamera.CFrustum.CONTAIN_TYPE CheckContain(CGfxCamera.CFrustum frustum)
        {
            return frustum.WhichContainType(ref BoundVolume, true);
        }

        public void OnCheckVisible(CCommandList cmd, GSceneGraph scene, CGfxCamera camera, CheckVisibleParam param)
        {
            if (HostSet == null)
                throw new InvalidOperationException("");
            var bitSet = HostSet.PvsData[PvsIndex];
            if(scene.PVSActors != null && scene.PVSActors.Length == bitSet.BitCount)
            {
                for(int i=0; i<bitSet.BitCount; i++)
                {
                    if (!bitSet.IsBit(i))
                        continue;

                    Actor.GActor actor = scene.PVSActors[i];
                    if (actor.NeedCheckVisible(param) == false)
                        continue;
                    if (param.FrustumCulling && actor.CheckContain(camera.CullingFrustum, true) == CGfxCamera.CFrustum.CONTAIN_TYPE.CONTAIN_TEST_OUTER)
                        continue;

                    actor.OnCheckVisible(cmd, scene, camera, param);
                }
            }
        }
    }
    [Rtti.MetaClass]
    public class GPvsSet : IO.Serializer.Serializer
    {
        [Rtti.MetaData]
        public List<Support.BitSet> PvsData;
        [Rtti.MetaData]
        public List<GPvsCell> PvsCells;
        [Rtti.MetaData]
        public Matrix WorldMatrix;
        [Rtti.MetaData]
        public Matrix WorldMatrixInv;
    }
}
