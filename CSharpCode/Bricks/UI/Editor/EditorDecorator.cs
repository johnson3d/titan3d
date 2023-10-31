using EngineNS.GamePlay.Scene;
using EngineNS.Graphics.Mesh;
using EngineNS.UI.Canvas;
using EngineNS.UI.Controls;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.UI.Editor
{
    public partial class TtUIEditor
    {
        UMeshNode[] mOperatorNodes = new UMeshNode[8];
        TtUINode mSelectedRect;
        TtUINode mPointAtRect;

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

            var selectHost = new EditorUIHost();
            selectHost.DrawBrush.Color = Color.LightGreen;
            mSelectedRect = await TtUINode.AddUINode(PreviewViewport.World, mUINode, new UNodeData(),
                typeof(GamePlay.UPlacement), selectHost, DVector3.Zero, Vector3.One, Quaternion.Identity);
            mSelectedRect.Parent = null;

            var pointAtHost = new EditorUIHost();
            pointAtHost.DrawBrush.Color = Color.LightBlue;
            mPointAtRect = await TtUINode.AddUINode(PreviewViewport.World, mUINode, new UNodeData(),
                typeof(GamePlay.UPlacement), pointAtHost, DVector3.Zero, Vector3.One, Quaternion.Identity);
            mPointAtRect.Parent = null;
        }

        async Thread.Async.TtTask UpdateDecorator()
        {
            if(mSelectedRect.UIHost.MeshDirty && (mSelectedRect.Parent != null))
            {
                await mSelectedRect.UIHost.BuildMesh();
            }
            if(mPointAtRect.UIHost.MeshDirty && (mSelectedRect.Parent != null))
            {
                await mPointAtRect.UIHost.BuildMesh();
            }
        }

        TtUIElement mCurrentPointAtElement;
        void SetCurrentPointAtElement(TtUIElement element)
        {
            // debug //////////////////
            if (element != null)
                UEngine.Instance.UIManager.DebugPointatElement = element.Name + "(" + element.GetType().Name + ")";
            else
                UEngine.Instance.UIManager.DebugPointatElement = "";
            ///////////////////////////

            if (element != null)
            {
                mPointAtRect.UIHost.SetDesignRect(element.DesignRect);
                if(mPointAtRect.UIHost.TransformedElements.Count <= 0)
                    mPointAtRect.UIHost.AddTransformedUIElement(mPointAtRect.UIHost, 0);
                mPointAtRect.UIHost.TransformedElements[0].SetMatrix(in mUIHost.TransformedElements[element.TransformIndex].Matrix);

                mPointAtRect.UIHost.MeshDirty = true;
                mPointAtRect.Parent = mUINode;
            }
            else
            {
                mPointAtRect.Parent = null;
            }

            mCurrentPointAtElement = element;
        }

        TtUIElement mSelectedElement;
        void SelectElement(TtUIElement element)
        {
            mSelectedElement = element;
            if(element != null)
            {

            }
            else
            {

            }
        }
    }
}
