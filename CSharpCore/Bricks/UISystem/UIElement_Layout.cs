using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace EngineNS.UISystem
{
    public partial class UIElementInitializer
    {
        //protected bool mWidth_Auto = true;
        //[Browsable(false)]
        //[Rtti.MetaData]
        //public bool Width_Auto
        //{
        //    get => mWidth_Auto;
        //    set
        //    {
        //        mWidth_Auto = value;
        //        OnPropertyChanged("Width_Auto");
        //    }
        //}
        //protected int mMinWidth = 0;
        //[Rtti.MetaData]
        //public int MinWidth
        //{
        //    get => mMinWidth;
        //    set
        //    {
        //        mMinWidth = value;
        //        OnPropertyChanged("MinWidth");
        //    }
        //}
        //protected int mMaxWidth = int.MaxValue;
        //[Rtti.MetaData]
        //public int MaxWidth
        //{
        //    get => mMaxWidth;
        //    set
        //    {
        //        mMaxWidth = value;
        //        OnPropertyChanged("MaxWidth");
        //    }
        //}

        //protected bool mHeight_Auto = true;
        //[Browsable(false)]
        //[Rtti.MetaData]
        //public bool Height_Auto
        //{
        //    get => mHeight_Auto;
        //    set
        //    {
        //        mHeight_Auto = value;
        //        OnPropertyChanged("Height_Auto");
        //    }
        //}
        //protected int mMinHeight = 0;
        //[Rtti.MetaData]
        //public int MinHeight
        //{
        //    get => mMinHeight;
        //    set
        //    {
        //        mMinHeight = value;
        //        OnPropertyChanged("MinHeight");
        //    }
        //}
        //protected int mMaxHeight = int.MaxValue;
        //[Rtti.MetaData]
        //public int MaxHeight
        //{
        //    get => mMaxHeight;
        //    set
        //    {
        //        mMaxHeight = value;
        //        OnPropertyChanged("MaxHeight");
        //    }
        //}

        [Rtti.MetaData]
        public Controls.Containers.UIContainerSlot Slot
        {
            get;
            set;
        }
    }

    public partial class UIElement
    {
        [System.Flags]
        internal enum CoreFlags : uint
        {
            None = 0,
            MeasureDirty = 1 << 0,
            ArrangeDirty = 1 << 1,
            MeasureInProgress = 1 << 2,
            ArrangeInProgress = 1 << 3,
            NeverMeasured = 1 << 4,
            NeverArranged = 1 << 5,
            MeasureDuringArrange = 1 << 6,
            IsLayoutIslandRoot = 1 << 7,
        }
        private CoreFlags _flags;
        internal bool ReadFlag(CoreFlags field)
        {
            return (_flags & field) != 0;
        }
        internal void WriteFlag(CoreFlags field, bool value)
        {
            if (value)
                _flags |= field;
            else
                _flags &= ~field;
        }
        internal bool IsLayoutIslandRoot
        {
            get { return ReadFlag(CoreFlags.IsLayoutIslandRoot); }
            set { WriteFlag(CoreFlags.IsLayoutIslandRoot, value); }
        }
        internal bool MeasureDirty
        {
            get { return ReadFlag(CoreFlags.MeasureDirty); }
            set { WriteFlag(CoreFlags.MeasureDirty, value); }
        }
        internal bool ArrangeDirty
        {
            get { return ReadFlag(CoreFlags.ArrangeDirty); }
            set { WriteFlag(CoreFlags.ArrangeDirty, value); }
        }
        internal bool MeasureInProgress
        {
            get { return ReadFlag(CoreFlags.MeasureInProgress); }
            set { WriteFlag(CoreFlags.MeasureInProgress, value); }
        }
        internal bool ArrangeInProgress
        {
            get { return ReadFlag(CoreFlags.ArrangeInProgress); }
            set { WriteFlag(CoreFlags.ArrangeInProgress, value); }
        }
        internal bool NeverMeasured
        {
            get { return ReadFlag(CoreFlags.NeverMeasured); }
            set { WriteFlag(CoreFlags.NeverMeasured, value); }
        }
        internal bool NeverArranged
        {
            get { return ReadFlag(CoreFlags.NeverArranged); }
            set { WriteFlag(CoreFlags.NeverArranged, value); }
        }
        internal bool MeasureDuringArrange
        {
            get { return ReadFlag(CoreFlags.MeasureDuringArrange); }
            set { WriteFlag(CoreFlags.MeasureDuringArrange, value); }
        }
        [Browsable(false)]
        public bool IsArrangeValid { get => !ArrangeDirty; }
        [Browsable(false)]
        public bool IsMeasureValid { get => !MeasureDirty; }
        internal void InvalidateMeasureInternal() { MeasureDirty = true; }
        internal void InvalidateArrangeInternal() { ArrangeDirty = true; }

        internal Layout.LayoutQueue.Request MeasureRequest;
        internal Layout.LayoutQueue.Request ArrangeRequest;

        internal virtual UInt32 TreeLevel
        {
            get;
            set;
        } = 0;

        bool mTopMost = false;
        [EngineNS.Editor.UIEditor_BindingPropertyAttribute]
        [EngineNS.Editor.MacrossMember(EngineNS.Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public bool TopMost
        {
            get => mTopMost;
            set
            {
                if (mTopMost == value)
                    return;
                mTopMost = value;
                if (mTopMost)
                    MoveToTop();
                OnPropertyChanged("TopMost");
            }
        }
        public void MoveToTop()
        {
            var panel = Parent as Controls.Containers.Panel;
            if(panel != null)
            {
                panel.RemoveChild(this);
                panel.AddChild(this);
            }
        }

        //[Category("布局")]
        //[EngineNS.Editor.UIEditor_BindingPropertyAttribute]
        //[EngineNS.Editor.UIEditor_PropertysWithAutoSet]
        //public int Width
        //{
        //    get => (int)DesignRect.Width;
        //    set
        //    {
        //        var dr = DesignRect;
        //        if (System.Math.Abs(dr.Width - value) <= CoreDefine.Epsilon)
        //            return;

        //        dr.Width = value;
        //        //if (Slot != null)
        //        //    Slot.ProcessSetContentDesignRect(ref dr);
        //        //else
        //            mInitializer.DesignRect = dr;
        //        if (!Width_Auto)
        //        {
        //            this.Parent?.OnChildDesiredSizeChanged(this);
        //            this.UpdateLayout();
        //        }
        //        OnPropertyChanged("Width");
        //    }
        //}
        // 用于内部计算
        [Browsable(false)]
        internal float Width_Inner
        {
            get => DesignRect.Width;
        }
        //[Browsable(false)]
        //public bool Width_Auto
        //{
        //    get => mInitializer.Width_Auto;
        //    set
        //    {
        //        if (mInitializer.Width_Auto == value)
        //            return;
        //        mInitializer.Width_Auto = value;
        //        this.UpdateLayout();
        //        OnPropertyChanged("Width_Auto");
        //    }
        //}
        //[Category("布局")]
        //[EngineNS.Editor.UIEditor_BindingPropertyAttribute]
        //public int MinWidth
        //{
        //    get => mInitializer.MinWidth;
        //    set
        //    {
        //        mInitializer.MinWidth = value;
        //        UpdateLayout();
        //        OnPropertyChanged("MinWidth");
        //    }
        //}
        //[Category("布局")]
        //[EngineNS.Editor.UIEditor_BindingPropertyAttribute]
        //public int MaxWidth
        //{
        //    get => mInitializer.MaxWidth;
        //    set
        //    {
        //        mInitializer.MaxWidth = value;
        //        UpdateLayout();
        //        OnPropertyChanged("MaxWidth");
        //    }
        //}
        //[Category("布局")]
        //[EngineNS.Editor.UIEditor_BindingPropertyAttribute]
        //[EngineNS.Editor.UIEditor_PropertysWithAutoSet]
        //public int Height
        //{
        //    get => (int)DesignRect.Height;
        //    set
        //    {
        //        var dr = DesignRect;
        //        if (System.Math.Abs(dr.Height - value) <= CoreDefine.Epsilon)
        //            return;

        //        dr.Height = value;
        //        //if (Slot != null)
        //        //    Slot.ProcessSetContentDesignRect(ref dr);
        //        //else
        //            mInitializer.DesignRect = dr;
        //        if (!Height_Auto)
        //        {
        //            this.Parent?.OnChildDesiredSizeChanged(this);
        //            this.UpdateLayout();
        //        }
        //        OnPropertyChanged("Height");
        //    }
        //}
        // 用于内部计算
        [Browsable(false)]
        internal float Height_Inner
        {
            get => DesignRect.Height;
        }
        //[Browsable(false)]
        //public bool Height_Auto
        //{
        //    get => mInitializer.Height_Auto;
        //    set
        //    {
        //        if (mInitializer.Height_Auto == value)
        //            return;
        //        mInitializer.Height_Auto = value;
        //        this.UpdateLayout();
        //        OnPropertyChanged("Height_Auto");
        //    }
        //}

        //[Category("布局")]
        //[EngineNS.Editor.UIEditor_BindingPropertyAttribute]
        //public int MinHeight
        //{
        //    get => mInitializer.MinHeight;
        //    set
        //    {
        //        mInitializer.MinHeight = value;
        //        UpdateLayout();
        //        OnPropertyChanged("MinHeight");
        //    }
        //}
        //[Category("布局")]
        //[EngineNS.Editor.UIEditor_BindingPropertyAttribute]
        //public int MaxHeight
        //{
        //    get => mInitializer.MaxHeight;
        //    set
        //    {
        //        mInitializer.MaxHeight = value;
        //        UpdateLayout();
        //        OnPropertyChanged("MaxHeight");
        //    }
        //}
        [EngineNS.Editor.MacrossMember(EngineNS.Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        [EngineNS.Editor.Editor_ShowOnlyInnerProperties]
        [Category("布局")]
        public Controls.Containers.UIContainerSlot Slot
        {
            get => mInitializer?.Slot;
            set
            {
                if(mInitializer != null)
                    mInitializer.Slot = value;
                OnPropertyChanged("Slot");
            }
        }

        public virtual void OnChildDesiredSizeChanged(UIElement child)
        {
            if(IsMeasureValid && Slot != null)
            {
                if (Slot.NeedUpdateLayoutWhenChildDesiredSizeChanged(child))
                    InvalidateMeasure();
            }
            //if(IsMeasureValid)
            //{
            //    if(!(Width_Auto && Height_Auto))
            //        InvalidateMeasure();
            //}
        }
        public virtual void UpdateLayout()
        {
            if (Parent == null)
                return;
            InvalidateMeasure();
        }
        public UIElement GetUIParentWithinLayoutIsland()
        {
            var uiParent = this.Parent;
            if (uiParent != null && uiParent.IsLayoutIslandRoot)
                return null;
            return uiParent;
        }
        protected void InvalidateMeasure()
        {
            EngineNS.CEngine.Instance.EventPoster.RunOn(() =>
            {
                if ((!MeasureDirty && !MeasureInProgress))
                {
                    System.Diagnostics.Debug.Assert(MeasureRequest == null, "can't be clean and still have MeasureRequest");
                    EngineNS.CEngine.Instance.UILayoutManager.MeasureQueue.Add(this);
                    MeasureDirty = true;
                }
                return true;
            }, Thread.Async.EAsyncTarget.Logic);
        }
        protected void InvalidateArrange()
        {
            EngineNS.CEngine.Instance.EventPoster.RunOn(() =>
            {
                if (!ArrangeDirty && !ArrangeInProgress)
                {
                    if (Parent == null || !Parent.ArrangeDirty)
                    {
                        System.Diagnostics.Debug.Assert(ArrangeRequest == null, "can't be clean and still have MeasureRequest");
                        EngineNS.CEngine.Instance.UILayoutManager.ArrangeQueue.Add(this);
                    }
                    ArrangeDirty = true;
                }
                return true;
            }, Thread.Async.EAsyncTarget.Logic);
        }
        internal virtual void MarkTreeDirty()
        {
            InvalidateMeasureInternal();
            InvalidateArrangeInternal();
        }

        private void SwitchVisibilityIfNeeded(Visibility visibility)
        {
            switch(visibility)
            {
                case Visibility.Visible:
                    break;
                case Visibility.Hidden:
                    break;
                case Visibility.Collapsed:
                    break;
            }
        }
        private void EnsureVisible()
        {

        }

        EngineNS.SizeF mPreviousAvailableSize = EngineNS.SizeF.Infinity;
        public void ResetPreviousAvailableSize()
        {
            mPreviousAvailableSize = EngineNS.SizeF.Infinity;
        }
        internal EngineNS.SizeF PreviousAvailableSize => mPreviousAvailableSize;
        private EngineNS.SizeF mDesiredSize;
        internal EngineNS.SizeF DesiredSize
        {
            get
            {
                if (this.Visibility == Visibility.Collapsed)
                    return new SizeF(0, 0);
                else
                    return mDesiredSize;
            }
            set
            {
                mDesiredSize = value;
            }
        }
        EngineNS.SizeF mPrevDesiredSize;
        internal void Measure(ref EngineNS.SizeF availableSize)
        {
            try
            {
                if (FloatUtil.IsNaN(availableSize.Width) || FloatUtil.IsNaN(availableSize.Height))
                    throw new InvalidOperationException("Measure exception availableSize is NaN");

                var neverMeasured = NeverMeasured;
                if(neverMeasured)
                {

                }

                var isCloseToPreviousMeasure = mPreviousAvailableSize.Equals(ref availableSize);
                if(this.Visibility == Visibility.Collapsed)
                {
                    if (MeasureRequest != null)
                        EngineNS.CEngine.Instance.UILayoutManager.MeasureQueue.Remove(this);
                    if (isCloseToPreviousMeasure)
                        MeasureDirty = false;
                    else
                    {
                        InvalidateMeasureInternal();
                        mPreviousAvailableSize = availableSize;
                    }
                    return;
                }

                if (IsMeasureValid && !neverMeasured && isCloseToPreviousMeasure)
                    return;

                NeverMeasured = false;
                mPrevDesiredSize = mDesiredSize;
                InvalidateArrange();
                MeasureInProgress = true;
                var desiredSize = new EngineNS.SizeF(0, 0);
                bool gotException = true;
                try
                {
                    EngineNS.CEngine.Instance.UILayoutManager.EnterMeasure();
                    desiredSize = MeasureCore(ref availableSize);
                    gotException = false;
                }
                finally
                {
                    MeasureInProgress = false;
                    mPreviousAvailableSize = availableSize;
                    EngineNS.CEngine.Instance.UILayoutManager.ExitMeasure();
                    if(gotException)
                    {
                        if (EngineNS.CEngine.Instance.UILayoutManager.LastExceptionElement == null)
                            EngineNS.CEngine.Instance.UILayoutManager.LastExceptionElement = this;
                    }
                }

                // desiredSize必须为有效值
                if(float.IsPositiveInfinity(desiredSize.Width) || float.IsPositiveInfinity(desiredSize.Height))
                    throw new InvalidOperationException("Measure Exception: desiredSize IsPositiveInfinity");
                if(FloatUtil.IsNaN(desiredSize.Width) || FloatUtil.IsNaN(desiredSize.Height))
                    throw new InvalidOperationException("Measure Exception: desiredSize IsNaN");

                MeasureDirty = false;
                if (MeasureRequest != null)
                    EngineNS.CEngine.Instance.UILayoutManager.MeasureQueue.Remove(this);

                mDesiredSize = desiredSize;
                if(!MeasureDuringArrange && !mPrevDesiredSize.Equals(ref desiredSize))
                {
                    var p = this.Parent;
                    if (p != null && !p.MeasureInProgress)
                        p.OnChildDesiredSizeChanged(this);
                }
            }
            catch(System.Exception e)
            {
                EngineNS.Profiler.Log.WriteException(e);
            }
        }

        //internal struct MinMax
        //{
        //    internal MinMax(UIElement e)
        //    {
        //        maxHeight = e.MaxHeight;
        //        minHeight = e.MinHeight;
        //        float l = e.Height_Inner;

        //        //float height = (FloatUtil.IsNaN(l) ? Double.PositiveInfinity : l);
        //        float height = (e.Height_Auto || FloatUtil.IsNaN(l) ? float.PositiveInfinity : l);
        //        maxHeight = Math.Max(Math.Min(height, maxHeight), minHeight);

        //        //height = (FloatUtil.IsNaN(l) ? 0 : l);
        //        height = (e.Height_Auto || FloatUtil.IsNaN(l) ? 0 : l);
        //        minHeight = Math.Max(Math.Min(maxHeight, height), minHeight);

        //        maxWidth = e.MaxWidth;
        //        minWidth = e.MinWidth;
        //        l = e.Width_Inner;

        //        //float width = (FloatUtil.IsNaN(l) ? Double.PositiveInfinity : l);
        //        float width = (e.Width_Auto || FloatUtil.IsNaN(l) ? float.PositiveInfinity : l);
        //        maxWidth = Math.Max(Math.Min(width, maxWidth), minWidth);

        //        //width = (FloatUtil.IsNaN(l) ? 0 : l);
        //        width = (e.Width_Auto || FloatUtil.IsNaN(l) ? 0 : l);
        //        minWidth = Math.Max(Math.Min(maxWidth, width), minWidth);
        //    }

        //    internal float minWidth;
        //    internal float maxWidth;
        //    internal float minHeight;
        //    internal float maxHeight;
        //}
        EngineNS.SizeF mClippedSizeBox = EngineNS.SizeF.Empty;
        protected virtual EngineNS.SizeF MeasureCore(ref EngineNS.SizeF availableSize)
        {
            var frameworkAvailableSize = new EngineNS.SizeF(Math.Max(availableSize.Width, 0), Math.Max(availableSize.Height, 0));
            var desiredSize = frameworkAvailableSize;
            if (Slot != null)
            {
                desiredSize = Slot.Measure(ref frameworkAvailableSize);
            }
            else
            {
                desiredSize = MeasureOverride(ref frameworkAvailableSize);
            }

            return desiredSize;
        }

        public virtual EngineNS.SizeF MeasureOverride(ref EngineNS.SizeF availableSize)
        {
            return availableSize;
        }

        EngineNS.RectangleF mCurFinalRect;
        internal EngineNS.RectangleF PreviousArrangeRect => mCurFinalRect;
        public void Arrange(ref EngineNS.RectangleF finalRect)
        {
            try
            {
                if(float.IsPositiveInfinity(finalRect.Width) || float.IsPositiveInfinity(finalRect.Height) || FloatUtil.IsNaN(finalRect.Width) || FloatUtil.IsNaN(finalRect.Height))
                    throw new InvalidOperationException("Arrange Exception: finalRect illegal!");

                if(this.Visibility == Visibility.Collapsed)
                {
                    if (ArrangeRequest != null)
                        EngineNS.CEngine.Instance.UILayoutManager.ArrangeQueue.Remove(this);
                    mCurFinalRect = finalRect;
                    ArrangeDirty = false;
                    return;
                }

                if(MeasureDirty || NeverMeasured)
                {
                    try
                    {
                        MeasureDuringArrange = true;
                        if (NeverMeasured)
                        {
                            var size = finalRect.Size;
                            Measure(ref size);
                        }
                        else
                            Measure(ref mPreviousAvailableSize);
                    }
                    finally
                    {
                        MeasureDuringArrange = false;
                    }
                }

                if(!IsArrangeValid || NeverArranged || !(finalRect.Equals(ref mCurFinalRect) && mPrevDesiredSize.Equals(ref mDesiredSize)))
                {
                    NeverArranged = false;
                    ArrangeInProgress = true;

                    var oldSize = DesignRect.Size;
                    bool gotException = true;
                    try
                    {
                        EngineNS.CEngine.Instance.UILayoutManager.EnterArrange();
                        ArrangeCore(ref finalRect);
                        gotException = false;
                    }
                    finally
                    {
                        ArrangeInProgress = false;
                        EngineNS.CEngine.Instance.UILayoutManager.ExitArrange();
                        if(gotException)
                        {
                            if (EngineNS.CEngine.Instance.UILayoutManager.LastExceptionElement == null)
                                EngineNS.CEngine.Instance.UILayoutManager.LastExceptionElement = this;
                        }
                    }

                    mCurFinalRect = finalRect;
                    ArrangeDirty = false;
                    if (ArrangeRequest != null)
                        EngineNS.CEngine.Instance.UILayoutManager.ArrangeQueue.Remove(this);
                }
            }
            catch(System.Exception e)
            {
                System.Diagnostics.Debug.WriteLine(this.GetType().ToString() + " Arrange Exception: \r\n" + e.ToString());
            }
            finally
            {

            }
        }
        protected virtual void ArrangeCore(ref EngineNS.RectangleF finalRect)
        {
            var arrangeSize = finalRect;

            var oldRect = DesignRect;
            if (Slot != null)
                Slot.Arrange(ref arrangeSize);
            else
                ArrangeOverride(ref arrangeSize);

            if (!DesignRect.Equals(ref oldRect))
            {
                UpdateDesignClipRect();

                ForceUpdateDraw = true;
            }
            OnPropertyChanged("Width");
            OnPropertyChanged("Height");
        }
        public virtual void ArrangeOverride(ref EngineNS.RectangleF arrangeSize)
        {
            if (mInitializer == null)
                return;
            if (mInitializer.DesignRect.Equals(ref arrangeSize))
                return;
            DesignRect = arrangeSize;
            UpdateDesignClipRect();
        }
    }
}
