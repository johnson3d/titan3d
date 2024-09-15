using EngineNS.GamePlay;
using EngineNS.GamePlay.Scene;
using EngineNS.Graphics.Mesh;
using EngineNS.Graphics.Pipeline.Common;
using EngineNS.Rtti;
using EngineNS.UI.Canvas;
using EngineNS.UI.Controls;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.UI.Editor
{
    [AttributeUsage(AttributeTargets.Class)]
    public class EditorDecoratorAttribute : Attribute
    {
        public Rtti.TtTypeDesc TypeDesc;
        public EditorDecoratorAttribute(Type decoratorType)
        {
            TypeDesc = Rtti.TtTypeDesc.TypeOf(decoratorType);
        }
    }

    public interface IUIEditorDecorator
    {
        bool IsDirty { get; set; }
        Thread.Async.TtTask Initialize(TtUIEditor editor);
        Thread.Async.TtTask UpdateDecorator();
        void ProcessSelectElementDecorator();
        void ClearDecorator();
        void DecoratorEventProcess(in Bricks.Input.Event e);
        bool IsInDecoratorOperation();
    }

    public partial class TtUIEditor
    {
        SelectedDecorator mSelectedDecoratorUIHost;
        internal TtUINode SelectedRect;
        internal TtUINode PointAtRect;
        internal TtHitproxyNode HitProxyNode;
        Dictionary<Rtti.TtTypeDesc, IUIEditorDecorator> mDecorators = new Dictionary<TtTypeDesc, IUIEditorDecorator>();
        internal IUIEditorDecorator CurrentDecorator;

        async Thread.Async.TtTask InitializeDecorators()
        {
            mSelectedDecoratorUIHost = new SelectedDecorator();
            mSelectedDecoratorUIHost.DrawBrush.Color = Color4b.LightGreen;
            SelectedRect = await TtUINode.AddUINode(PreviewViewport.World, mUINode, new TtNodeData(),
                typeof(GamePlay.TtPlacement), mSelectedDecoratorUIHost, DVector3.Zero, Vector3.One, Quaternion.Identity);
            SelectedRect.Parent = null;

            var pointAtHost = new EditorUIHost();
            pointAtHost.DrawBrush.Color = Color4b.LightBlue;
            pointAtHost.PathWidth = 4.5f;
            PointAtRect = await TtUINode.AddUINode(PreviewViewport.World, mUINode, new TtNodeData(),
                typeof(GamePlay.TtPlacement), pointAtHost, DVector3.Zero, Vector3.One, Quaternion.Identity);
            PointAtRect.Parent = null;

            HitProxyNode = PreviewViewport.RenderPolicy.FindFirstNode<Graphics.Pipeline.Common.TtHitproxyNode>();
        }

        async Thread.Async.TtTask UpdateDecorator()
        {
            if (!mUIHost.IsReadyToDraw())
                return;
            if (mUIHost.TransformedElements.Count <= 0)
                return;
            for (int i=mSelectedElements.Count - 1; i >=0; i--)
            {
                if (mSelectedElements[i].MeshDirty)
                {
                    if(CurrentDecorator != null)
                        CurrentDecorator.IsDirty = true;
                    SelectedRect.GetUIHost(0).MeshDirty = true;
                }
            }

            if(SelectedRect.GetUIHost(0).MeshDirty && (SelectedRect.Parent != null))
            {
                var mesh = await SelectedRect.GetUIHost(0).BuildMesh();
                SelectedRect.AABB = new DBoundingBox(mesh.MaterialMesh.AABB);
            }
            if(PointAtRect.GetUIHost(0).MeshDirty && (PointAtRect.Parent != null))
            {
                var mesh = await PointAtRect.GetUIHost(0).BuildMesh();
                PointAtRect.AABB = new DBoundingBox(mesh.MaterialMesh.AABB);
            }

            CurrentDecorator?.UpdateDecorator();
        }

        TtUIElement mCurrentPointAtElement;
        public TtUIElement CurrentPointAtElement => mCurrentPointAtElement;
        internal void SetCurrentPointAtElement(TtUIElement element)
        {
            // debug //////////////////
            //if (element != null)
            //    TtEngine.Instance.UIManager.DebugPointatElement = element.Name + "(" + element.GetType().Name + ")";
            //else
            //    TtEngine.Instance.UIManager.DebugPointatElement = "";
            ///////////////////////////

            if (element != null)
            {
                PointAtRect.GetUIHost(0).SetDesignRect(element.DesignRect);
                if(PointAtRect.GetUIHost(0).TransformedElements.Count <= 0)
                    PointAtRect.GetUIHost(0).AddTransformedUIElement(PointAtRect.GetUIHost(0), 0);
                PointAtRect.GetUIHost(0).TransformedElements[0].SetMatrix(in mUIHost.TransformedElements[element.TransformIndex].Matrix);

                PointAtRect.GetUIHost(0).MeshDirty = true;
                PointAtRect.Parent = mUINode;
            }
            else
            {
                PointAtRect.Parent = null;
            }

            mCurrentPointAtElement = element;
        }

        async Thread.Async.TtTask ProcessSelectElementDecorator()
        {
            if (mSelectedDecoratorUIHost == null)
                return;
            if (CurrentDecorator != null)
                CurrentDecorator.ClearDecorator();
            CurrentDecorator = null;
            if (mSelectedElements.Count > 0)
            {
                mSelectedDecoratorUIHost.SelectedElements = mSelectedElements;
                mSelectedDecoratorUIHost.MeshDirty = true;
                //mSelectedRect.UIHost.SetDesignRect(element.DesignRect);
                //if (mSelectedRect.UIHost.TransformedElements.Count <= 0)
                //    mSelectedRect.UIHost.AddTransformedUIElement(mSelectedRect.UIHost, 0);
                //mSelectedRect.UIHost.TransformedElements[0].SetMatrix(in transMat);
                SelectedRect.GetUIHost(0).MeshDirty = true;
                SelectedRect.Parent = mUINode;

                TtTypeDesc decalType;
                var parent = VisualTreeHelper.GetParent(mSelectedElements[0]);
                if(parent != null)
                {
                    var atts = parent.GetType().GetCustomAttributes(typeof(EditorDecoratorAttribute), true);
                    if (atts.Length > 0)
                    {
                        decalType = ((EditorDecoratorAttribute)atts[0]).TypeDesc;
                        if(!mDecorators.TryGetValue(decalType, out CurrentDecorator))
                        {
                            CurrentDecorator = TtTypeDescManager.CreateInstance(decalType) as IUIEditorDecorator;
                            await CurrentDecorator.Initialize(this);
                            mDecorators[decalType] = CurrentDecorator;
                        }
                        // when update type, type is not equal
                        if(CurrentDecorator != null && !decalType.IsEqual(CurrentDecorator.GetType()))
                        {
                            CurrentDecorator = TtTypeDescManager.CreateInstance(decalType) as IUIEditorDecorator;
                            await CurrentDecorator.Initialize(this);
                            mDecorators[decalType] = CurrentDecorator;
                        }
                    }
                }
            }
            else
            {
                SelectedRect.Parent = null;
            }

            CurrentDecorator?.ProcessSelectElementDecorator();
        }

        void DecoratorEventProcess(in Bricks.Input.Event e)
        {
            CurrentDecorator?.DecoratorEventProcess(e);
        }
    }
}
