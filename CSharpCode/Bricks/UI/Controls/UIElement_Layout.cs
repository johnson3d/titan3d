using EngineNS;
using EngineNS.UI.Bind;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection.PortableExecutable;
using System.Text;

namespace EngineNS.UI.Controls
{
    public partial class TtUIElement
    {
        [Flags]
        internal enum eLayoutFlags : uint
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
            UseRounding = 1 << 8,           // aligns to pixel boundaries
            BypassLayoutPolicies = 1 << 9,
        }
        private eLayoutFlags mLayoutFlags;
        internal bool ReadFlag(eLayoutFlags flag)
        {
            return (mLayoutFlags & flag) != 0;
        }
        internal void WriteFlag(eLayoutFlags flag, bool value)
        {
            if (value)
                mLayoutFlags |= flag;
            else
                mLayoutFlags &= ~flag;
        }
        [Browsable(false)]
        internal bool IsLayoutIslandRoot
        {
            get { return ReadFlag(eLayoutFlags.IsLayoutIslandRoot); }
            set { WriteFlag(eLayoutFlags.IsLayoutIslandRoot, value); }
        }
        [Browsable(false)]
        internal bool MeasureDirty
        {
            get { return ReadFlag(eLayoutFlags.MeasureDirty); }
            set { WriteFlag(eLayoutFlags.MeasureDirty, value); }
        }
        [Browsable(false)]
        internal bool ArrangeDirty
        {
            get { return ReadFlag(eLayoutFlags.ArrangeDirty); }
            set { WriteFlag(eLayoutFlags.ArrangeDirty, value); }
        }
        [Browsable(false)]
        internal bool MeasureInProgress
        {
            get { return ReadFlag(eLayoutFlags.MeasureInProgress); }
            set { WriteFlag(eLayoutFlags.MeasureInProgress, value); }
        }
        [Browsable(false)]
        internal bool ArrangeInProgress
        {
            get { return ReadFlag(eLayoutFlags.ArrangeInProgress); }
            set { WriteFlag(eLayoutFlags.ArrangeInProgress, value); }
        }
        [Browsable(false)]
        internal bool NeverMeasured
        {
            get { return ReadFlag(eLayoutFlags.NeverMeasured); }
            set { WriteFlag(eLayoutFlags.NeverMeasured, value); }
        }
        [Browsable(false)]
        internal bool NeverArranged
        {
            get { return ReadFlag(eLayoutFlags.NeverArranged); }
            set { WriteFlag(eLayoutFlags.NeverArranged, value); }
        }
        [Browsable(false)]
        internal bool MeasureDuringArrange
        {
            get { return ReadFlag(eLayoutFlags.MeasureDuringArrange); }
            set { WriteFlag(eLayoutFlags.MeasureDuringArrange, value); }
        }
        [BindProperty(DefaultValue = false)]
        public bool UseRounding
        {
            get { return ReadFlag(eLayoutFlags.UseRounding); }
            set 
            { 
                WriteFlag(eLayoutFlags.UseRounding, value);
                OnValueChange(value);
            }
        }
        protected bool BypassLayoutPolicies
        {
            get => ReadFlag(eLayoutFlags.BypassLayoutPolicies);
            set => WriteFlag(eLayoutFlags.BypassLayoutPolicies, value);
        }
        [Browsable(false)]
        public bool IsArrangeValid { get => !ArrangeDirty; }
        [Browsable(false)]
        public bool IsMeasureValid { get => !MeasureDirty; }
        internal void InvalidateMeasureInternal() { MeasureDirty = true; }
        internal void InvalidateArrangeInternal() { ArrangeDirty = true; }

        internal Layout.TtLayoutQueue.Request MeasureRequest;
        internal Layout.TtLayoutQueue.Request ArrangeRequest;

        [Browsable(false)]
        internal virtual UInt32 TreeLevel
        {
            get;
            set;
        } = 0;

        public class MarginEditorAttribute : EGui.Controls.PropertyGrid.PGCustomValueEditorAttribute
        {
            public unsafe override bool OnDraw(in EditorInfo info, out object newValue)
            {
                this.Expandable = true;
                bool retValue = false;
                newValue = info.Value;
                var index = ImGuiAPI.TableGetColumnIndex();
                var width = ImGuiAPI.GetColumnWidth(index);
                ImGuiAPI.SetNextItemWidth(width - EGui.UIProxy.StyleConfig.Instance.PGCellPadding.X);
                var minValue = float.MinValue;
                var maxValue = float.MaxValue;
                var multiValue = info.Value as EGui.Controls.PropertyGrid.PropertyMultiValue;
                if (multiValue != null && multiValue.HasDifferentValue())
                {
                    ImGuiAPI.Text(multiValue.MultiValueString);
                    if (multiValue.DrawVector<Vector4>(in info, "Left", "Top", "Right", "Bottom") && !info.Readonly)
                    {
                        newValue = multiValue;
                        retValue = true;
                    }
                }
                else
                {
                    var v = (Vector4)info.Value;
                    float speed = 0.1f;
                    var format = "%.6f";
                    if (info.HostProperty != null)
                    {
                        var vR = info.HostProperty.GetAttribute<EGui.Controls.PropertyGrid.PGValueRange>();
                        if (vR != null)
                        {
                            minValue = (float)vR.Min;
                            maxValue = (float)vR.Max;
                        }
                        var vStep = info.HostProperty.GetAttribute<EGui.Controls.PropertyGrid.PGValueChangeStep>();
                        if (vStep != null)
                        {
                            speed = vStep.Step;
                        }
                        var vFormat = info.HostProperty.GetAttribute<EGui.Controls.PropertyGrid.PGValueFormat>();
                        if (vFormat != null)
                            format = vFormat.Format;
                    }
                    var changed = ImGuiAPI.DragScalarN2(TName.FromString2("##", info.Name).ToString(), ImGuiDataType_.ImGuiDataType_Float, (float*)&v, 4, speed, &minValue, &maxValue, format, ImGuiSliderFlags_.ImGuiSliderFlags_None);
                    if (changed && !info.Readonly)//(v != saved)
                    {
                        newValue = v;
                        retValue = true;
                    }
                    if(Vector4.Vector4EditorAttribute.OnDrawVectorValue<Vector4>(in info, ref v, ref v, "Left", "Top", "Right", "Bottom") && !info.Readonly)
                    {
                        newValue = v;
                        retValue = true;
                    }
                }
                return retValue;
            }
        }

        Vector4 mMargin = Vector4.Zero;
        [BindProperty, MarginEditor, Category("Layout")]
        public Vector4 Margin
        {
            get => mMargin;
            set
            {
                mMargin = value;
                OnValueChange(value);
                UpdateLayout();
            }
        }

        float mMinWidth = 0.0f;
        [BindProperty, Category("Layout")]
        public float MinWidth
        {
            get => mMinWidth;
            set
            {
                mMinWidth = value;
                OnValueChange(value);
                UpdateLayout();
            }
        }
        float mMinHeight = 0.0f;
        [BindProperty, Category("Layout")]
        public float MinHeight
        {
            get => mMinHeight;
            set
            {
                mMinHeight = value;
                OnValueChange(value);
                UpdateLayout();
            }
        }
        float mMaxWidth = float.MaxValue;
        [BindProperty, Category("Layout")]
        public float MaxWidth
        {
            get => mMaxWidth;
            set
            {
                mMaxWidth = value;
                OnValueChange(value);
                UpdateLayout();
            }
        }
        float mMaxHeight = float.MaxValue;
        [BindProperty, Category("Layout")]
        public float MaxHeight
        {
            get => mMaxHeight;
            set
            {
                mMaxHeight = value;
                OnValueChange(value);
                UpdateLayout();
            }
        }

        public virtual void UpdateLayout()
        {
            if (Parent == null)
                return;

            InvalidateMeasure();
        }
        public TtUIElement GetUIParentWithinLayoutIsland()
        {
            var uiParent = this.Parent;
            if (uiParent != null && uiParent.IsLayoutIslandRoot)
                return null;
            return uiParent;
        }
        protected void InvalidateMeasure()
        {
            UEngine.Instance.EventPoster.RunOn(static (state) =>
            {
                var This = (TtUIElement)state.UserArguments;
                if (!This.MeasureDirty && !This.MeasureInProgress)
                {
                    System.Diagnostics.Debug.Assert(This.MeasureRequest == null, "can't be clean and still have MeasureRequest");
                    EngineNS.UEngine.Instance.UILayoutManager.MeasureQueue.Add(This);
                    This.MeasureDirty = true;
                }
                return true;
            }, Thread.Async.EAsyncTarget.Logic, this);
        }
        protected void InvalidateArrange()
        {
            UEngine.Instance.EventPoster.RunOn(static (state) =>
            {
                var This = (TtUIElement)state.UserArguments;
                if (!This.ArrangeDirty && !This.ArrangeInProgress)
                {
                    if (This.Parent == null || !This.Parent.ArrangeDirty)
                    {
                        System.Diagnostics.Debug.Assert(This.ArrangeRequest == null, "can't be clean and still have MeasureRequest");
                        UEngine.Instance.UILayoutManager.ArrangeQueue.Add(This);
                    }
                    This.ArrangeDirty = true;
                }
                return true;
            }, Thread.Async.EAsyncTarget.Logic, this);
        }
        internal virtual void MarkTreeDirty()
        {
            InvalidateMeasureInternal();
            InvalidateArrangeInternal();
        }
        EngineNS.SizeF mPreviousAvailableSize = EngineNS.SizeF.Infinity;
        public void ResetPreviousAvailableSize()
        {
            mPreviousAvailableSize = EngineNS.SizeF.Infinity;
        }

        static float RoundValue(float value, float dpiScale)
        {
            if (Math.Abs(dpiScale - 1.0f) >= MathHelper.Epsilon)
            {
                float newValue = (float)Math.Round(value * dpiScale) / dpiScale;
                if (float.IsNaN(newValue) || float.IsInfinity(newValue) || (Math.Abs(newValue - float.MaxValue) < MathHelper.Epsilon))
                    return value;
                return newValue;
            }
            else
                return (float)Math.Round(value);
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
        public void Measure(in EngineNS.SizeF availableSize)
        {
            try
            {
                if (FloatUtil.IsNaN(availableSize.Width) || FloatUtil.IsNaN(availableSize.Height))
                    throw new InvalidOperationException("Measure exception availableSize is NaN");

                var neverMeasured = NeverMeasured;
                if (neverMeasured)
                {

                }

                var isCloseToPreviousMeasure = mPreviousAvailableSize.Equals(in availableSize);
                if (this.Visibility == Visibility.Collapsed)
                {
                    if (MeasureRequest != null)
                        EngineNS.UEngine.Instance.UILayoutManager.MeasureQueue.Remove(this);
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
                    EngineNS.UEngine.Instance.UILayoutManager.EnterMeasure();
                    desiredSize = MeasureCore(in availableSize);
                    gotException = false;
                }
                finally
                {
                    MeasureInProgress = false;
                    mPreviousAvailableSize = availableSize;
                    EngineNS.UEngine.Instance.UILayoutManager.ExitMeasure();
                    if (gotException)
                    {
                        if (EngineNS.UEngine.Instance.UILayoutManager.LastExceptionElement == null)
                            EngineNS.UEngine.Instance.UILayoutManager.LastExceptionElement = this;
                    }
                }

                // desiredSize必须为有效值
                if (float.IsPositiveInfinity(desiredSize.Width) || float.IsPositiveInfinity(desiredSize.Height))
                    throw new InvalidOperationException("Measure Exception: desiredSize IsPositiveInfinity");
                if (FloatUtil.IsNaN(desiredSize.Width) || FloatUtil.IsNaN(desiredSize.Height))
                    throw new InvalidOperationException("Measure Exception: desiredSize IsNaN");

                MeasureDirty = false;
                if (MeasureRequest != null)
                    EngineNS.UEngine.Instance.UILayoutManager.MeasureQueue.Remove(this);

                mDesiredSize = desiredSize;
                if (!MeasureDuringArrange && !mPrevDesiredSize.Equals(in desiredSize))
                {
                    var p = this.Parent;
                    if (p != null && !p.MeasureInProgress)
                        p.OnChildDesiredSizeChanged(this);
                }
            }
            catch (System.Exception e)
            {
                EngineNS.Profiler.Log.WriteException(e);
            }
        }
        //public Controls.Containers.TtUIContainerSlot Slot { get; set; }

        protected virtual EngineNS.SizeF MeasureCore(in EngineNS.SizeF availableSize)
        {
            var frameworkAvailableSize = new EngineNS.SizeF(Math.Max(availableSize.Width, 0), Math.Max(availableSize.Height, 0));
            float dpiScale = 1.0f;
            if(RootUIHost != null)
                dpiScale = RootUIHost.DPIScale;
            ApplyTemplate();
            
            if(BypassLayoutPolicies)
                return MeasureOverride(in frameworkAvailableSize);
            else
            {
                var margin = Margin;
                var marginWidth = margin.X + margin.Z;
                var marginHeight = margin.Y + margin.W;
                var finaleSize = new SizeF(
                    Math.Max(availableSize.Width - marginWidth, 0.0f),
                    Math.Max(availableSize.Height - marginHeight, 0.0f));
                var minWidth = Math.Min(MinWidth, finaleSize.Width);
                var minHeight = Math.Min(MinHeight, finaleSize.Height);
                var maxWidth = Math.Max(MaxWidth, finaleSize.Width);
                var maxHeight = Math.Max(MaxHeight, finaleSize.Height);
                finaleSize.Width = Math.Max(minWidth, Math.Min(finaleSize.Width, maxWidth));
                finaleSize.Height = Math.Max(minHeight, Math.Min(finaleSize.Height, maxHeight));
                if(UseRounding)
                {
                    finaleSize.Width = RoundValue(finaleSize.Width, dpiScale);
                    finaleSize.Height = RoundValue(finaleSize.Height, dpiScale);
                }
                return MeasureOverride(in finaleSize);
            }
        }

        protected virtual EngineNS.SizeF MeasureOverride(in EngineNS.SizeF availableSize)
        {
            return availableSize;
        }

        protected EngineNS.RectangleF mCurFinalRect;
        internal EngineNS.RectangleF PreviousArrangeRect => mCurFinalRect;
        public void Arrange(in EngineNS.RectangleF finalRect)
        {
            try
            {
                if (float.IsPositiveInfinity(finalRect.Width) || float.IsPositiveInfinity(finalRect.Height) || FloatUtil.IsNaN(finalRect.Width) || FloatUtil.IsNaN(finalRect.Height))
                    throw new InvalidOperationException("Arrange Exception: finalRect illegal!");

                if (this.Visibility == Visibility.Collapsed)
                {
                    if (ArrangeRequest != null)
                        EngineNS.UEngine.Instance.UILayoutManager.ArrangeQueue.Remove(this);
                    mCurFinalRect = finalRect;
                    ArrangeDirty = false;
                    return;
                }

                if (MeasureDirty || NeverMeasured)
                {
                    try
                    {
                        MeasureDuringArrange = true;
                        if (NeverMeasured)
                        {
                            var size = finalRect.Size;
                            Measure(in size);
                        }
                        else
                            Measure(in mPreviousAvailableSize);
                    }
                    finally
                    {
                        MeasureDuringArrange = false;
                    }
                }

                if (!IsArrangeValid || NeverArranged || !(finalRect.Equals(in mCurFinalRect) && mPrevDesiredSize.Equals(in mDesiredSize)))
                {
                    var firstArrange = NeverArranged;
                    NeverArranged = false;
                    ArrangeInProgress = true;

                    var oldSize = DesignRect.Size;
                    bool gotException = true;
                    try
                    {
                        EngineNS.UEngine.Instance.UILayoutManager.EnterArrange();
                        ArrangeCore(in finalRect);
                        gotException = false;
                    }
                    finally
                    {
                        ArrangeInProgress = false;
                        EngineNS.UEngine.Instance.UILayoutManager.ExitArrange();
                        if (gotException)
                        {
                            if (EngineNS.UEngine.Instance.UILayoutManager.LastExceptionElement == null)
                                EngineNS.UEngine.Instance.UILayoutManager.LastExceptionElement = this;
                        }
                    }

                    mCurFinalRect = finalRect;
                    ArrangeDirty = false;
                    if (ArrangeRequest != null)
                        EngineNS.UEngine.Instance.UILayoutManager.ArrangeQueue.Remove(this);

                    if(firstArrange && IsRenderable())
                    {
                        // render update
                        RootUIHost.MeshDirty = true;
                    }
                }
            }
            catch (System.Exception e)
            {
                System.Diagnostics.Debug.WriteLine(this.GetType().ToString() + " Arrange Exception: \r\n" + e.ToString());
            }
            finally
            {
                RootUIHost.MeshDirty = true;
            }
        }
        protected virtual void ArrangeCore(in EngineNS.RectangleF finalRect)
        {
            DesignRect = finalRect;
            ArrangeOverride(in finalRect);

            if (mClipRectDirty)
            {
                UpdateDesignClipRect();
            }
        }
        protected virtual void ArrangeOverride(in EngineNS.RectangleF arrangeSize)
        {
        }

    }
}
