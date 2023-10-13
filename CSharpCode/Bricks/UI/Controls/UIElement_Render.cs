using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace EngineNS.UI.Controls
{
    public partial class TtUIElement
    {
        [Browsable(false)]
        public virtual bool MeshDirty
        {
            get
            {
                if (RootUIHost != null)
                    return RootUIHost.MeshDirty;
                return false;
            }
            set
            {
                if (RootUIHost != null)
                    RootUIHost.MeshDirty = value;
            }
        }

        // 0~1
        Vector2 mRenderTransformCenter = new Vector2(0.5f, 0.5f);
        [Rtti.Meta]
        public Vector2 RenderTransformCenter
        {
            get => mRenderTransformCenter;
            set
            {
                mRenderTransformCenter = value;
                RenderTransformDirty = true;
            }
        }

        bool mRenderTransformDirty = false;
        [Browsable(false)]
        public virtual bool RenderTransformDirty
        {
            get
            {
                if (mRenderTransformDirty)
                    return true;
                if (Parent != null)
                    return Parent.RenderTransformDirty;
                return false;
            }
            set
            {
                mRenderTransformDirty = value;
                OnRenderTransformDirtyChanged(value);
            }
        }
        protected virtual void OnRenderTransformDirtyChanged(bool isDirty)
        {
            if (isDirty == false && Parent != null)
            {
                Parent.RenderTransformDirty = false;
            }
        }
        FTransform mRenderTransform = FTransform.Identity;
        [Rtti.Meta]
        public FTransform RenderTransform
        {
            get => mRenderTransform;
            set
            {
                mRenderTransform = value;
                RenderTransformDirty = true;
                if (CanTransformDirtyMesh())
                {
                    MeshDirty = true;
                }
            }
        }
        FTransform mAbsRenderTransform = new FTransform();
        [Browsable(false)]
        public virtual ref FTransform AbsRenderTransform
        {
            get
            {
                if (Parent == null)
                    return ref mRenderTransform;
                if (RenderTransformDirty)
                {
                    FTransform.Multiply(out mAbsRenderTransform, Parent.RenderTransform, mRenderTransform);
                    RenderTransformDirty = false;
                }
                return ref mAbsRenderTransform;
            }
        }

        protected UInt16 mTransformIndex = 0;
        [Browsable(false)]
        public UInt16 TransformIndex => mTransformIndex;
        bool CanTransformDirtyMesh()
        {
            if(GetType() == typeof(TtUIHost))
                return false;

            if(mTransformIndex == 0)
                return true;

            if(Parent != null && mTransformIndex == Parent.TransformIndex)
                return true;

            return false;
        }
        public virtual UInt16 UpdateTransformIndex(UInt16 parentTransformIdx)
        {
            if (mRenderTransform.IsIdentity)
                mTransformIndex = parentTransformIdx;
            else
                mTransformIndex = RootUIHost.AddTransformedUIElement(this, parentTransformIdx);

            return mTransformIndex;
        }

        private bool IsRenderable()
        {
            if (NeverMeasured || NeverArranged)
                return false;
            if (Visibility == Visibility.Collapsed)
                return false;
            return IsMeasureValid && IsArrangeValid;
        }

        internal void DrawInternal(Canvas.TtCanvas canvas, Canvas.TtCanvasDrawBatch batch)
        {
            //var clip = DesignClipRect;
            //batch.SetPosition(clip.Left, clip.Top);
            //batch.SetClientClip(clip.Width, clip.Height);

            Draw(canvas, batch);
        }
        public virtual void Draw(Canvas.TtCanvas canvas, Canvas.TtCanvasDrawBatch batch)
        {

        }

        public virtual bool IsReadyToDraw()
        {
            return true;
        }
    }
}
