using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using System.Threading.Tasks;
using EngineNS.GamePlay.Actor;

namespace EngineNS.Bricks.RecastRuntime
{
    [Editor.Editor_PlantAbleActor("Navigation", "Nav Mesh Bound Volume")]
    public class NavMeshBoundVolumeComponent : EngineNS.LooseOctree.BoxComponent
    {
        public Byte RCAreaType = 64;
        public BoundingBox GetBox()
        {
            BoundingBox aabb = new BoundingBox();
            if (mHost == null)
                return aabb;
            mHost.GetAABB(ref aabb);

            return aabb;
        }

        public override async Task<bool> SetInitializer(CRenderContext rc, GActor host, GComponentInitializer v)
        {
            if (!await base.SetInitializer(rc, host, v))
                return false;

            host.IsNavgation = true;

            return true;
        }
    }

}
