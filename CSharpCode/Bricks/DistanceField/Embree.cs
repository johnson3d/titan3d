using EngineNS.Graphics.Mesh;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.DistanceField
{
    public class UEmbreeScene : AuxPtrType<EngineNS.FEmbreeScene>
    {
        public UEmbreeScene()
        {
            mCoreObject = EngineNS.FEmbreeScene.CreateInstance();
        }
    }

    public class UEmbreeManager : AuxPtrType<EngineNS.EmbreeManager>
    {
        public UEmbreeManager()
        {
            mCoreObject = EngineNS.EmbreeManager.CreateInstance();
        }
        public void SetupEmbreeScene(string name, UMeshDataProvider meshProvider, float DistanceFieldResolutionScale, UEmbreeScene embreeScene)
        {
            mCoreObject.SetupEmbreeScene(VNameString.FromString(name), meshProvider.mCoreObject, DistanceFieldResolutionScale, embreeScene.mCoreObject);
        }
        public void DeleteEmbreeScene(UEmbreeScene embreeScene)
        {
            mCoreObject.DeleteEmbreeScene(embreeScene.mCoreObject);
        }
    }
}
