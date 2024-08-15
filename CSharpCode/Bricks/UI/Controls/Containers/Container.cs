using EngineNS.IO;
using EngineNS.UI.Bind;
using EngineNS.UI.Canvas;
using NPOI.SS.Formula.PTG;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace EngineNS.UI.Controls.Containers
{
    public partial class TtUIElementCollection : IList<TtUIElement>
    {
        internal List<TtUIElement> mChildren = new List<TtUIElement>();

        TtContainer mVisualParent;
        public TtContainer VisualParent => mVisualParent;
        TtContainer mLogicalParent;
        public TtContainer LogicalParent => mLogicalParent;
        public TtUIElementCollection(TtContainer visualParent, TtContainer logicalParent)
        {
            if (visualParent == null)
                throw new ArgumentNullException("visualParent can not be null");
            mVisualParent = visualParent;
            mLogicalParent = logicalParent;
        }

        public int Count
        {
            get
            {
                if (mVisualParent.TemplateInternal != null)
                {
                    if (mLogicalParent.HasTemplateGeneratedSubTree)
                    {
                        if (mVisualParent.ChildContentsPresenter != null)
                            return mVisualParent.ChildContentsPresenter.Children.Count;
                        else
                            return 0;
                    }
                    else
                        return mChildrenHolder.Count;
                }
                else
                    return mChildren.Count;
            }
        }

        public bool IsReadOnly => false;

        // template parent default get logical tree item, other get it's child
        public TtUIElement this[int index] 
        {
            get
            {
                //if (!TtEngine.Instance.EventPoster.IsThread(Thread.Async.EAsyncTarget.Logic))
                //{
                //    throw new InvalidOperationException("need be called in logic thread");
                //}

                if (mVisualParent.TemplateInternal != null)
                {
                    if (mLogicalParent.HasTemplateGeneratedSubTree)
                    {
                        if (mVisualParent.ChildContentsPresenter != null)
                            return mVisualParent.ChildContentsPresenter.Children[index];
                        else
                            return null;
                    }
                    else
                        return mChildrenHolder[index];
                }
                else
                    return mChildren[index];
            }
            set
            {
                //if (!TtEngine.Instance.EventPoster.IsThread(Thread.Async.EAsyncTarget.Logic))
                //{
                //    throw new InvalidOperationException("need be called in logic thread");
                //}

                if(mVisualParent.TemplateInternal != null)
                {
                    if(mLogicalParent.HasTemplateGeneratedSubTree)
                    {
                        if (mVisualParent.ChildContentsPresenter != null)
                        {
                            var children = mVisualParent.ChildContentsPresenter.Children;
                            children[index] = value;
                        }
                    }
                    else
                        mChildrenHolder[index] = value;
                }
                else
                {
                    if(mChildren[index] != value)
                    {
                        var c = mChildren[index];
                        ClearParent(c);
                    }
                    mChildren[index] = value;
                    SetParent(value);
                    mVisualParent.InvalidateMeasure();
                    if (value != null && value is TtContentsPresenter)
                    {
                        mVisualParent.ChildIsContentsPresenter = true;
                    }
                }
            }
        }
        //public TtUIElement GetVisualChild(int index)
        //{
        //    if (mVisualParent.ChildIsContentsPresenter)
        //        return mLogicalParent.mLogicContentsPresenter.Children[index];
        //    return mChildren[index];
        //}
        protected void ClearParent(TtUIElement element)
        {
            if (mLogicalParent != null)
            {
                if (mLogicalParent.IsLogicalChildrenIterationInProgress)
                    throw new InvalidOperationException("Can not modify logical children during three walk");
                bool hasLogicChildren = false;
                foreach(var lcp in mLogicalParent.mLogicContentsPresenters)
                {
                    hasLogicChildren = hasLogicChildren || (lcp.Value.Children.Count > 0);
                }
                mLogicalParent.HasLogicalChildren = hasLogicChildren;
            }
            if(element != null)
            {
                if(element.mParent != null)
                    element.mParent.Children.Remove(element);
                element.mParent = null;
                element.RootUIHost = null; 
                element.mVisualParent = null;
            }
        }
        protected void SetParent(TtUIElement element)
        {
            if (mLogicalParent != null)
            {
                if (mLogicalParent.IsLogicalChildrenIterationInProgress)
                    throw new InvalidOperationException("Can not modify logical children during three walk");
                mLogicalParent.HasLogicalChildren = true;
                // todo: fire trigger
            }
            if(element != null)
            {
                if(element.mVisualParent != null)
                {
                    if (element.mVisualParent is TtContentsPresenter)
                        element.RemoveAttachedProperties(element.mVisualParent.mVisualParent.GetType());
                    else
                        element.RemoveAttachedProperties(element.mVisualParent.GetType());
                }
                element.mParent = mLogicalParent;
                element.RootUIHost = mVisualParent.RootUIHost;
                element.mVisualParent = mVisualParent;
                if(mVisualParent != null)
                {
                    if (element.mVisualParent is TtContentsPresenter)
                        mVisualParent.mVisualParent.SetAttachedProperties(element);
                    else
                        mVisualParent.SetAttachedProperties(element);
                }
            }
        }

        bool HasContentsPresenter()
        {
            for(int i=0; i<mChildren.Count; i++)
            {
                if (mChildren[i] is TtContentsPresenter)
                    return true;
            }
            return false;
        }

        public int IndexOf(TtUIElement item)
        {
            //if (!TtEngine.Instance.EventPoster.IsThread(Thread.Async.EAsyncTarget.Logic))
            //{
            //    throw new InvalidOperationException("need be called in logic thread");
            //}

            if (mVisualParent.TemplateInternal != null)
            {
                if (mLogicalParent.HasTemplateGeneratedSubTree)
                {
                    if (mVisualParent.ChildContentsPresenter != null)
                    {
                        var children = mVisualParent.ChildContentsPresenter.Children;
                        return children.IndexOf(item);
                    }
                    else
                        return -1;
                }
                else
                    return mChildrenHolder.IndexOf(item);
            }
            else
                return mChildren.IndexOf(item);
        }

        public void Insert(int index, TtUIElement item)
        {
            //if (!TtEngine.Instance.EventPoster.IsThread(Thread.Async.EAsyncTarget.Logic))
            //{
            //    throw new InvalidOperationException("need be called in logic thread");
            //}
            if (mVisualParent.TemplateInternal != null)
            {
                if (mLogicalParent.HasTemplateGeneratedSubTree)
                {
                    if (mVisualParent.ChildContentsPresenter != null)
                    {
                        var children = mVisualParent.ChildContentsPresenter.Children;
                        children.Insert(index, item);
                    }
                }
                else
                {
                    ClearParent(item);
                    mChildrenHolder.Insert(index, item);
                }
            }
            else
            {
                if (item is TtContentsPresenter)
                {
                    mVisualParent.ChildIsContentsPresenter = true;
                }
                ClearParent(item);
                SetParent(item);
                mChildren.Insert(index, item);
                mVisualParent.InvalidateMeasure();
            }
        }

        public void RemoveAt(int index)
        {
            //if (!TtEngine.Instance.EventPoster.IsThread(Thread.Async.EAsyncTarget.Logic))
            //{
            //    throw new InvalidOperationException("need be called in logic thread");
            //}
            if(mVisualParent.TemplateInternal != null)
            {
                if (mLogicalParent.HasTemplateGeneratedSubTree)
                {
                    if (mVisualParent.ChildContentsPresenter != null)
                    {
                        var children = mVisualParent.ChildContentsPresenter.Children;
                        children.RemoveAt(index);
                    }
                }
                else
                {
                    ClearParent(mChildrenHolder[index]);
                    mChildrenHolder.RemoveAt(index);
                }
            }
            else
            {
                var e = mChildren[index];
                mChildren.RemoveAt(index);
                ClearParent(e);
                mVisualParent.InvalidateMeasure();
                mVisualParent.ChildIsContentsPresenter = HasContentsPresenter();
            }
        }
        public bool Remove(TtUIElement item)
        {
            //if (!TtEngine.Instance.EventPoster.IsThread(Thread.Async.EAsyncTarget.Logic))
            //{
            //    throw new InvalidOperationException("need be called in logic thread");
            //}
            bool returnValue = false;
            if (mVisualParent.TemplateInternal != null)
            {
                if (mLogicalParent.HasTemplateGeneratedSubTree)
                {
                    if (mVisualParent.ChildContentsPresenter != null)
                    {
                        var children = mVisualParent.ChildContentsPresenter.Children;
                        returnValue = children.Remove(item);
                    }
                    else
                        returnValue = false;
                }
                else
                {
                    ClearParent(item);
                    return mChildrenHolder.Remove(item);
                }
            }
            else
            {
                if (mChildren.Remove(item))
                {
                    ClearParent(item);
                    mVisualParent.InvalidateMeasure();
                    mVisualParent.ChildIsContentsPresenter = HasContentsPresenter();
                    returnValue = true;
                }
            }
            return returnValue;
        }
        [Rtti.Meta]
        public void Add(TtUIElement item)
        {
            //if (!TtEngine.Instance.EventPoster.IsThread(Thread.Async.EAsyncTarget.Logic))
            //{
            //    throw new InvalidOperationException("need be called in logic thread");
            //}
            if (item == null)
                throw new ArgumentNullException("item is null");
            if(mVisualParent.TemplateInternal != null)
            {
                if(mLogicalParent.HasTemplateGeneratedSubTree)
                {
                    if (mVisualParent.ChildContentsPresenter != null)
                    {
                        var children = mVisualParent.ChildContentsPresenter.Children;
                        children.Add(item);
                    }
                }
                else
                {
                    ClearParent(item);
                    SetParent(item);
                    mChildrenHolder.Add(item);
                }
            }
            else
            {
                if(item is TtContentsPresenter)
                {
                    mVisualParent.ChildIsContentsPresenter = true;
                    mVisualParent.mVisualParent.InvalidateMeasure();
                }
                else
                    mVisualParent.InvalidateMeasure();

                ClearParent(item);
                SetParent(item);
                mChildren.Add(item);
            }
        }

        public void Clear()
        {
            //if (!TtEngine.Instance.EventPoster.IsThread(Thread.Async.EAsyncTarget.Logic))
            //{
            //    throw new InvalidOperationException("need be called in logic thread");
            //}
            // todo: mChildrenHolder process
            if (mVisualParent.TemplateInternal != null)
            {
                if (mLogicalParent.HasTemplateGeneratedSubTree)
                {
                    if (mVisualParent.ChildContentsPresenter != null)
                    {
                        var children = mVisualParent.ChildContentsPresenter.Children;
                        children.Clear();
                    }
                }
                else
                {
                    for(int i=mChildrenHolder.Count - 1; i>=0; i--)
                    {
                        ClearParent(mChildrenHolder[i]);
                        mChildrenHolder[i].Cleanup();
                    }
                    mChildrenHolder.Clear();
                }
            }
            else
            {
                var count = mChildren.Count;
                if(count > 0)
                {
                    for(int i = mChildren.Count - 1; i>=0; i--)
                    {
                        if (mChildren[i] != null)
                        {
                            ClearParent(mChildren[i]);
                            mChildren[i].Cleanup();
                        }
                    }
                    mChildren.Clear();
                    mVisualParent.InvalidateMeasure();
                }
            }

            mVisualParent.ChildIsContentsPresenter = false;
        }

        public bool Contains(TtUIElement item)
        {
            //if (!TtEngine.Instance.EventPoster.IsThread(Thread.Async.EAsyncTarget.Logic))
            //{
            //    throw new InvalidOperationException("need be called in logic thread");
            //}
            if(mVisualParent.TemplateInternal != null)
            {
                if (mLogicalParent.HasTemplateGeneratedSubTree)
                {
                    if (mVisualParent.ChildContentsPresenter != null)
                    {
                        var children = mVisualParent.ChildContentsPresenter.Children;
                        return children.Contains(item);
                    }
                    else
                        return false;
                }
                else
                    return mChildrenHolder.Contains(item);
            }
            else
                return mChildren.Contains(item);
        }

        public void CopyTo(TtUIElement[] array, int arrayIndex)
        {
            //if (!TtEngine.Instance.EventPoster.IsThread(Thread.Async.EAsyncTarget.Logic))
            //{
            //    throw new InvalidOperationException("need be called in logic thread");
            //}
            if(mVisualParent.TemplateInternal != null)
            {
                if (mLogicalParent.HasTemplateGeneratedSubTree)
                {
                    if (mVisualParent.ChildContentsPresenter != null)
                    {
                        var children = mVisualParent.ChildContentsPresenter.Children;
                        children.CopyTo(array, arrayIndex);
                    }
                }
                else
                    mChildrenHolder.CopyTo(array, arrayIndex);
            }
            else
                mChildren.CopyTo(array, arrayIndex);
        }

        public IEnumerator<TtUIElement> GetEnumerator()
        {
            //if (!TtEngine.Instance.EventPoster.IsThread(Thread.Async.EAsyncTarget.Logic))
            //{
            //    throw new InvalidOperationException("need be called in logic thread");
            //}
            if(mVisualParent.TemplateInternal != null)
            {
                if (mLogicalParent.HasTemplateGeneratedSubTree)
                {
                    if (mVisualParent.ChildContentsPresenter != null)
                    {
                        var children = mVisualParent.ChildContentsPresenter.Children;
                        return children.GetEnumerator();
                    }
                    else
                        return null;
                }
                else
                    return mChildrenHolder.GetEnumerator();
            }
            else
                return mChildren.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            //if (!TtEngine.Instance.EventPoster.IsThread(Thread.Async.EAsyncTarget.Logic))
            //{
            //    throw new InvalidOperationException("need be called in logic thread");
            //}
            if(mVisualParent.TemplateInternal != null)
            {
                if (mLogicalParent.HasTemplateGeneratedSubTree)
                {
                    if (mVisualParent.ChildContentsPresenter != null)
                    {
                        var children = mVisualParent.ChildContentsPresenter.Children;
                        return children.GetEnumerator();
                    }
                    else
                        return null;
                }
                else
                    return mChildrenHolder.GetEnumerator();
            }
            else
                return mChildren.GetEnumerator();
        }

        List<TtUIElement> mChildrenHolder = new List<TtUIElement>();
        public void OnApplyTemplate()
        {
            if (!mLogicalParent.HasTemplateGeneratedSubTree)
                throw new InvalidOperationException("OnApplyTemplate need generated subtree");
            if (mVisualParent.ChildContentsPresenter != null)
            {
                var children = mVisualParent.ChildContentsPresenter.Children;
                for (int i = 0; i < mChildrenHolder.Count; i++)
                {
                    children.Add(mChildrenHolder[i]);
                }
            }
            mChildrenHolder.Clear();
        }
    }

    public partial class TtTemplateContainer : TtContainer
    {
        public TtTemplateContainer()
        {
            IsSelectedable = false;
        }

        public override string GetEditorShowName()
        {
            return Name;
        }

        protected override SizeF MeasureOverride(in SizeF availableSize)
        {
            return SizeF.Empty;
        }
        protected override void ArrangeOverride(in RectangleF arrangeSize)
        {
        }
    }

    public abstract partial class TtContainer : TtUIElement
    {
        internal TtUIElementCollection mChildren;
        public class ElementCollectionSaverAttribute : IO.UCustomSerializerAttribute
        {
            public override void Save(IWriter ar, object host, string propName)
            {
                System.Diagnostics.Debug.Assert(propName == "Children");
                var container = host as TtContainer;
                var count = container.mChildren.Count;
                ar.Write(count);
                for(int i=0; i<count; i++)
                {
                    var offset = SerializerHelper.WriteSkippable(ar);
                    ar.Write(container.mChildren[i]);
                    SerializerHelper.SureSkippable(ar, offset);
                }
            }
            public override object Load(IReader ar, object host, string propName)
            {
                System.Diagnostics.Debug.Assert(propName == "Children");
                var container = host as TtContainer;
                int count;
                ar.Read(out count);
                for(int i=0; i<count; i++)
                {
                    var skipPoint = SerializerHelper.GetSkipOffset(ar);
                    try
                    {
                        IO.ISerializer serial;
                        ar.Read(out serial, host);
                        var element = serial as TtUIElement;
                        container.mChildren.Add(element);
                        element.SetLoadedAttachedValues();
                    }
                    catch(Exception)
                    {
                        ar.Seek(skipPoint);
                    }
                }
                return null;
            }
        }
        [Browsable(false), ElementCollectionSaver, Rtti.Meta]
        public TtUIElementCollection Children
        {
            get => mChildren;
            private set { ; }
        }

        [Browsable(false)]
        public bool ChildIsContentsPresenter
        {
            get => ReadInternalFlag(eInternalFlags.ChildIsContentsPresenter);
            set => WriteInternalFlag(eInternalFlags.ChildIsContentsPresenter, value);
        }

        internal TtContentsPresenter ChildContentsPresenter;

        public override void Cleanup()
        {
            mChildren.Clear();
        }

        //protected TtBrush mBrush;
        //[Rtti.Meta, Bind.BindProperty]
        //public TtBrush Brush
        //{
        //    get => mBrush;
        //    set
        //    {
        //        OnValueChange(value, mBrush);
        //        mBrush = value;
        //        mBrush.HostElement = this;
        //    }
        //}
        protected Thickness mBorderThickness = Thickness.Empty;
        [Rtti.Meta, Bind.BindProperty]
        public Thickness BorderThickness
        {
            get => mBorderThickness;
            set
            {
                OnValueChange(value, mBorderThickness);
                mBorderThickness = value;
                UpdateLayout();
            }
        }
        protected Thickness mPadding = Thickness.Empty;
        [Rtti.Meta, Bind.BindProperty]
        public Thickness Padding
        {
            get => mPadding;
            set
            {
                OnValueChange(value, mPadding);
                mPadding = value;
                UpdateLayout();
            }
        }
        protected TtBrush mBorderBrush;
        [Rtti.Meta, Bind.BindProperty]
        public TtBrush BorderBrush
        {
            get => mBorderBrush;
            set
            {
                OnValueChange(value, mBorderBrush);
                mBorderBrush = value;
                //mBorderBrush.HostElement = this;
                MeshDirty = true;
            }
        }
        protected TtBrush mBackground;
        [Rtti.Meta, Bind.BindProperty]
        public TtBrush Background
        {
            get => mBackground;
            set
            {
                OnValueChange(value, mBackground);
                mBackground = value;
                //mBackground.HostElement = this;
                MeshDirty = true;
            }
        }

        public enum ESizeToContent
        {
            None,
            Width,
            Height,
            WidthAndHeight,
        }
        protected ESizeToContent mSizeToContent = ESizeToContent.None;
        [Rtti.Meta, Bind.BindProperty]
        public ESizeToContent SizeToContent
        {
            get => mSizeToContent;
            set
            {
                OnValueChange(value, mSizeToContent);
                mSizeToContent = value;
                UpdateLayout();
            }
        }

        public TtContainer()
        {
            mChildren = new TtUIElementCollection(this, this);
            BorderBrush = new TtBrush();
            mBorderBrush.BrushType = TtBrush.EBrushType.Border;
            Background = new TtBrush();
        }
        public TtContainer(TtContainer parent)
            : base(parent)
        {
            mChildren = new TtUIElementCollection(this, parent);
            BorderBrush = new TtBrush();
            mBorderBrush.BrushType = TtBrush.EBrushType.Border;
            Background = new TtBrush();
            mBackground.HostElement = this;
        }
        // pt位置相对于linecheck到的element
        public override TtUIElement GetPointAtElement(in Vector2 pt, out Vector2 pointOffset, bool onlyClipped = true)
        {
            // todo: inv transform
            pointOffset = Vector2.Zero;
            if (onlyClipped)
            {
                if (!DesignRect.Contains(in pt))
                    return null;
                for (int i = mChildren.Count - 1; i >= 0; i--)
                {
                    var child = mChildren[i];
                    if (child.Is3D)
                        continue;
                    if (!child.DesignRect.Contains(in pt))
                        continue;

                    var container = child as TtContainer;
                    if (container != null)
                    {
                        var retVal = container.GetPointAtElement(in pt, out pointOffset, onlyClipped);
                        if (retVal != null)
                            return retVal;
                    }
                    else
                        return child;
                }
                return this;
            }
            else
            {
                for (int i = mChildren.Count - 1; i >= 0; i--)
                {
                    var child = mChildren[i];
                    if (child.Is3D)
                        continue;
                    var container = child as TtContainer;
                    if (container != null)
                    {
                        var retVal = container.GetPointAtElement(in pt, out pointOffset, onlyClipped);
                        if (retVal != null)
                            return retVal;
                    }
                    else if(child.DesignRect.Contains(in pt))
                        return child;
                }
                if (DesignRect.Contains(in pt))
                    return this;
                return null;
            }
        }
        public virtual bool NeedUpdateLayoutWhenChildDesiredSizeChanged(TtUIElement child)
        {
            return true;
        }
        public virtual void OnChildDesiredSizeChanged(TtUIElement child)
        {
            if (IsMeasureValid)
            {
                if (NeedUpdateLayoutWhenChildDesiredSizeChanged(child))
                    InvalidateMeasure();
            }
        }

        public override void Draw(TtCanvas canvas, TtCanvasDrawBatch batch)
        {
            var count = VisualTreeHelper.GetChildrenCount(this);
            for (int i = 0; i < count; i++)
            {
                var child = VisualTreeHelper.GetChild(this, i);
                child.Draw(canvas, batch);
            }
        }

        protected override void OnApplyTemplate()
        {
            if (!HasTemplateGeneratedSubTree)
                throw new InvalidOperationException("Template need generated sub tree");
            mChildren.OnApplyTemplate();
        }

        internal bool IsLogicalChildrenIterationInProgress
        {
            get => ReadInternalFlag(eInternalFlags.IsLogicalChildrenIterationInProgress);
            set => WriteInternalFlag(eInternalFlags.IsLogicalChildrenIterationInProgress, value);
        }
        protected internal Dictionary<string, TtContentsPresenter> mLogicContentsPresenters = new Dictionary<string, TtContentsPresenter>();

        protected override SizeF MeasureOverride(in SizeF availableSize)
        {
            var count = VisualTreeHelper.GetChildrenCount(this);
            var tempSize = SizeF.Empty;
            for(int i=0; i<count; i++)
            {
                var childUI = VisualTreeHelper.GetChild(this, i);
                childUI.Measure(in availableSize);
                var childSize = childUI.DesiredSize;
                tempSize.Width = Math.Max(tempSize.Width, childSize.Width);
                tempSize.Height = Math.Max(tempSize.Height, childSize.Height);
            }
            switch (mSizeToContent)
            {
                case ESizeToContent.None:
                    return availableSize;
                case ESizeToContent.Width:
                    return new SizeF(tempSize.Width, availableSize.Height);
                case ESizeToContent.Height:
                    return new SizeF(availableSize.Width, tempSize.Height);
                case ESizeToContent.WidthAndHeight:
                    return tempSize;
            }
            return availableSize;
        }
        protected override void ArrangeOverride(in RectangleF arrangeSize)
        {
            var count = VisualTreeHelper.GetChildrenCount(this);
            for(int i=0; i<count; i++)
            {
                var childUI = VisualTreeHelper.GetChild(this, i);
                childUI.Arrange(in arrangeSize);
            }
        }

        public override bool QueryElements<T>(Delegate_QueryProcess<T> queryAction, ref T queryData)
        {
            for(int i=0; i<mChildren.Count; i++)
            {
                if (mChildren[i].QueryElements(queryAction, ref queryData))
                    return true;
            }

            return base.QueryElements(queryAction, ref queryData);
        }

        public override bool IsReadyToDraw()
        {
            var count = VisualTreeHelper.GetChildrenCount(this);
            for(int i=0; i<count; i++)
            {
                var child = VisualTreeHelper.GetChild(this, i);
                if (!child.IsReadyToDraw())
                    return false;
            }
            return true;
        }


        protected override void OnRenderTransformDirtyChanged(bool isDirty)
        {
            base.OnRenderTransformDirtyChanged(isDirty);
            if(isDirty)
            {
                var count = VisualTreeHelper.GetChildrenCount(this);
                for(int i=0; i<count; i++)
                {
                    var child = VisualTreeHelper.GetChild(this, i);
                    child.RenderTransformDirty = true;
                }
            }
        }
        public override UInt16 UpdateTransformIndex(ushort parentTransformIdx)
        {
            var idx = base.UpdateTransformIndex(parentTransformIdx);
            var count = VisualTreeHelper.GetChildrenCount(this);
            for (int i = 0; i < count; i++)
            {
                var child = VisualTreeHelper.GetChild(this, i);
                child?.UpdateTransformIndex(idx);
            }
            return idx;
        }

        public virtual bool CanAddChild(Rtti.UTypeDesc childType)
        {
            return true;
        }
        public virtual void ProcessNewAddChild(TtUIElement element, in Vector2 offset, in Vector2 size)
        {

        }

        public override TtUIElement FindElement(string name)
        {
            var retVal = base.FindElement(name);
            if (retVal != null)
                return retVal;

            for(int i=0; i<Children.Count; i++)
            {
                retVal = Children[i].FindElement(name);
                if (retVal != null)
                    return retVal;
            }

            return null;
        }
        public override TtUIElement FindElement(ulong id)
        {
            var retVal = base.FindElement(id);
            if (retVal != null)
                return retVal;

            for(int i=0; i<Children.Count; i++)
            {
                retVal = Children[i].FindElement(id);
                if (retVal != null)
                    return retVal;
            }

            return null;
        }
        public override IBindableObject FindBindObject(ulong id)
        {
            var retVal = base.FindBindObject(id);
            if (retVal != null)
                return retVal;

            if (BorderBrush != null)
            {
                retVal = BorderBrush.FindBindObject(id);
                if (retVal != null)
                    return retVal;
            }
            if (Background != null)
            {
                retVal = Background.FindBindObject(id);
                if (retVal != null)
                    return retVal;
            }

            for(int i=0; i<Children.Count; i++)
            {
                retVal = Children[i].FindBindObject(id);
                if (retVal != null)
                    return retVal;
            }

            return null;
        }
    }
}
