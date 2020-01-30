using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using System.Threading.Tasks;
using EngineNS.GamePlay.Actor;


namespace EngineNS.Bricks.RecastRuntime
{
    [Editor.Editor_PlantAbleActor("Navigation", "Nav Modifier Volume")]
    public class NavModifierVolumeComponent : NavMeshBoundVolumeComponent
    {
        public NavModifierVolumeComponent()
        {
            RCAreaType = 0;//区域不可行走
        }

        //public override async Task<bool> SetInitializer(CRenderContext rc, GActor host, GComponentInitializer v)
        //{
        //    return await base.SetInitializer(rc, host, v);
           
        //}
    }
}