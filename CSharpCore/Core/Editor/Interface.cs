using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EngineNS.Editor
{
    public class PlantableItemCreateActorParam
    {
        public Graphics.View.CGfxSceneView View;
        public EngineNS.Vector3 Location;
    }

    // 可以在种植面板显示的
    public interface IPlantable
    {
        Task<GamePlay.Actor.GActor> CreateActor(PlantableItemCreateActorParam param);
    }
}
