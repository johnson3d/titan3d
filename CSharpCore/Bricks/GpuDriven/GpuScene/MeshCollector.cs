using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EngineNS.Bricks.GpuDriven.GpuScene
{
    public class MeshCollector
    {
        public void AddMeshSource(Graphics.Mesh.CGfxMeshPrimitives meshSource)
        {
            //meshSource.GeometryMesh
        }
        public void RegisterMesh(SceneDataManager scene, Graphics.Mesh.CGfxMesh mesh)
        {
            for (int i = 0; i < mesh.MtlMeshArray.Length; i++)
            {
                var materailId = scene.GetOrAddMaterialId(mesh.MtlMeshArray[i].MtlInst);
            }
        }
        public void AddMeshInstance(SceneDataManager scene, Graphics.Mesh.CGfxMesh mesh, GamePlay.Component.GPlacementComponent placement)
        {

        }
    }
}
