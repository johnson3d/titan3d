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
        public Rtti.UTypeDesc TypeDesc;
        public EditorDecoratorAttribute(Type decoratorType)
        {
            TypeDesc = Rtti.UTypeDesc.TypeOf(decoratorType);
        }
    }

    public interface IUIEditorDecorator
    {
        bool IsDirty { get; set; }
        Thread.Async.TtTask Initialize(TtUIEditor editor);
        Thread.Async.TtTask UpdateDecorator();
        void ProcessSelectElementDecorator();
        void DecoratorEventProcess(in Bricks.Input.Event e);
        bool IsInDecoratorOperation();
    }

    public partial class TtUIEditor
    {
        SelectedDecorator mSelectedDecoratorUIHost;
        internal TtUINode SelectedRect;
        internal TtUINode PointAtRect;
        internal UHitproxyNode HitProxyNode;
        Dictionary<Rtti.UTypeDesc, IUIEditorDecorator> mDecorators = new Dictionary<UTypeDesc, IUIEditorDecorator>();
        internal IUIEditorDecorator CurrentDecorator;

        async Thread.Async.TtTask InitializeDecorators()
        {
            mSelectedDecoratorUIHost = new SelectedDecorator();
            mSelectedDecoratorUIHost.DrawBrush.Color = Color.LightGreen;
            SelectedRect = await TtUINode.AddUINode(PreviewViewport.World, mUINode, new UNodeData(),
                typeof(GamePlay.UPlacement), mSelectedDecoratorUIHost, DVector3.Zero, Vector3.One, Quaternion.Identity);
            SelectedRect.Parent = null;

            var pointAtHost = new EditorUIHost(this);
            pointAtHost.DrawBrush.Color = Color.LightBlue;
            PointAtRect = await TtUINode.AddUINode(PreviewViewport.World, mUINode, new UNodeData(),
                typeof(GamePlay.UPlacement), pointAtHost, DVector3.Zero, Vector3.One, Quaternion.Identity);
            PointAtRect.Parent = null;

            HitProxyNode = PreviewViewport.RenderPolicy.FindFirstNode<Graphics.Pipeline.Common.UHitproxyNode>();
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
                    SelectedRect.UIHost.MeshDirty = true;
                }
            }

            if(SelectedRect.UIHost.MeshDirty && (SelectedRect.Parent != null))
            {
                await SelectedRect.UIHost.BuildMesh();
            }
            if(PointAtRect.UIHost.MeshDirty && (PointAtRect.Parent != null))
            {
                await PointAtRect.UIHost.BuildMesh();
            }

            CurrentDecorator?.UpdateDecorator();
        }

        TtUIElement mCurrentPointAtElement;
        public TtUIElement CurrentPointAtElement => mCurrentPointAtElement;
        internal void SetCurrentPointAtElement(TtUIElement element)
        {
            // debug //////////////////
            if (element != null)
                UEngine.Instance.UIManager.DebugPointatElement = element.Name + "(" + element.GetType().Name + ")";
            else
                UEngine.Instance.UIManager.DebugPointatElement = "";
            ///////////////////////////

            if (element != null)
            {
                PointAtRect.UIHost.SetDesignRect(element.DesignRect);
                if(PointAtRect.UIHost.TransformedElements.Count <= 0)
                    PointAtRect.UIHost.AddTransformedUIElement(PointAtRect.UIHost, 0);
                PointAtRect.UIHost.TransformedElements[0].SetMatrix(in mUIHost.TransformedElements[element.TransformIndex].Matrix);

                PointAtRect.UIHost.MeshDirty = true;
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
            CurrentDecorator = null;
            if (mSelectedElements.Count > 0)
            {
                mSelectedDecoratorUIHost.SelectedElements = mSelectedElements;
                mSelectedDecoratorUIHost.MeshDirty = true;
                //mSelectedRect.UIHost.SetDesignRect(element.DesignRect);
                //if (mSelectedRect.UIHost.TransformedElements.Count <= 0)
                //    mSelectedRect.UIHost.AddTransformedUIElement(mSelectedRect.UIHost, 0);
                //mSelectedRect.UIHost.TransformedElements[0].SetMatrix(in transMat);
                SelectedRect.UIHost.MeshDirty = true;
                SelectedRect.Parent = mUINode;

                UTypeDesc decalType;
                var parent = VisualTreeHelper.GetParent(mSelectedElements[0]);
                if(parent != null)
                {
                    var atts = parent.GetType().GetCustomAttributes(typeof(EditorDecoratorAttribute), true);
                    if (atts.Length > 0)
                    {
                        decalType = ((EditorDecoratorAttribute)atts[0]).TypeDesc;
                        if(!mDecorators.TryGetValue(decalType, out CurrentDecorator))
                        {
                            CurrentDecorator = UTypeDescManager.CreateInstance(decalType) as IUIEditorDecorator;
                            await CurrentDecorator.Initialize(this);
                            mDecorators[decalType] = CurrentDecorator;
                        }
                        // when update type, type is not equal
                        if(CurrentDecorator != null && !decalType.IsEqual(CurrentDecorator.GetType()))
                        {
                            CurrentDecorator = UTypeDescManager.CreateInstance(decalType) as IUIEditorDecorator;
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
