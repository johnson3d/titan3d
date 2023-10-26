using EngineNS.GamePlay.Scene;
using EngineNS.Graphics.Mesh;
using EngineNS.UI.Canvas;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.UI.Editor
{
    public partial class TtUIEditor
    {
        UMeshNode[] mOperatorNodes = new UMeshNode[8];

        async Thread.Async.TtTask InitializeDecorators()
        {
            var whiteColorMat = await UEngine.Instance.GfxDevice.MaterialInstanceManager.CreateMaterialInstance(RName.GetRName("material/whitecolor.uminst"));
            var meshProvider = UMeshDataProvider.MakeSphere(10.0f, 8, 8, 0xffffffff);
            var meshPrim = meshProvider.ToMesh();
            for(int i=0; i<8; i++)
            {
                var mesh = new UMesh();
                mesh.Initialize(meshPrim, new Graphics.Pipeline.Shader.UMaterial[] { whiteColorMat },
                    Rtti.UTypeDescGetter<Graphics.Mesh.UMdfStaticMesh>.TypeDesc);
                mOperatorNodes[i] = await UMeshNode.AddMeshNode(PreviewViewport.World, mUINode,
                    new GamePlay.Scene.UMeshNode.UMeshNodeData(), typeof(GamePlay.UPlacement),
                    mesh, DVector3.Zero, Vector3.One, Quaternion.Identity);
            }
        }
    }
}
