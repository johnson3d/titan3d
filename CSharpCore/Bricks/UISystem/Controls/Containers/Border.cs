using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Threading.Tasks;

namespace EngineNS.UISystem.Controls.Containers
{
    [Rtti.MetaClass]
    public class BorderInitializer : PanelInitializer
    {
        public BorderInitializer()
        {
            ContainChildType = enContainChildType.SingleChild;
        }
    }
    [Editor_UIControlInit(typeof(BorderInitializer))]
    [Editor_UIControl("容器.Border", "", "Border.png")]
    public class Border : Panel
    {
        [Browsable(false)]
        public UIElement Content
        {
            get
            {
                if (mChildrenUIElements.Count > 0)
                    return mChildrenUIElements[0];
                return null;
            }
            set
            {
                if(mChildrenUIElements.Count > 0)
                {
                    mChildrenUIElements[0].RemoveFromParent();
                }
                AddChild(value);
            }
        }
        
        BorderInitializer mCurIniter;
        public override Task<bool> Initialize(CRenderContext rc, UIElementInitializer init)
        {
            mCurIniter = init as BorderInitializer;
            return base.Initialize(rc, init);
        }

        #region Layout

        uint mTreeLevel = 0;
        internal override uint TreeLevel
        {
            get => mTreeLevel;
            set
            {
                mTreeLevel = value;
                var content = Content;
                if(content != null)
                    content.TreeLevel = mTreeLevel + 1;
            }
        }
        public override SizeF MeasureOverride(ref SizeF availableSize)
        {
            var retSize = SizeF.Empty;
            if(Content != null)
            {
                Content.Measure(ref availableSize);
                retSize.Width = System.Math.Max(retSize.Width, Content.DesiredSize.Width);
                retSize.Height = System.Math.Max(retSize.Height, Content.DesiredSize.Height);
            }
            return retSize;
        }
        public override void ArrangeOverride(ref RectangleF arrangeSize)
        {
            base.ArrangeOverride(ref arrangeSize);
        }

        #endregion

        public override void AddChild(UIElement element, bool updateLayout = true)
        {
            mChildrenUIElements.Clear();
            base.AddChild(element, updateLayout);
            if(element.Slot != null)
            {
                System.Diagnostics.Debug.Assert(element.Slot.GetType() == typeof(BorderSlot));
                element.Slot.Parent = this;
                element.Slot.Content = element;
            }
            else
            {
                var slot = new BorderSlot();
                slot.Parent = this;
                slot.Content = element;
                element.Slot = slot;
            }
        }
        public override void InsertChild(int index, UIElement element, bool updateLayout = true)
        {
            mChildrenUIElements.Clear();
            base.InsertChild(index, element, updateLayout);
            if (element.Slot != null)
            {
                System.Diagnostics.Debug.Assert(element.Slot.GetType() == typeof(BorderSlot));
                element.Slot.Parent = this;
                element.Slot.Content = element;
            }
            else
            {
                var slot = new BorderSlot();
                slot.Parent = this;
                slot.Content = element;
                element.Slot = slot;
            }
        }
    }
    [Rtti.MetaClass]
    public class BorderSlot : UIContainerSlot
    {
        HorizontalAlignment mHorizontalAlignment = HorizontalAlignment.Stretch;
        [Rtti.MetaData]
        [Category("布局")]
        [EngineNS.Editor.UIEditor_BindingPropertyAttribute]
        [EngineNS.Editor.Editor_PropertyGridDataTemplateAttribute("HorizontalAlignmentSetter")]
        public HorizontalAlignment HorizontalAlignment
        {
            get => mHorizontalAlignment;
            set
            {
                mHorizontalAlignment = value;
                Content?.UpdateLayout();
                OnPropertyChanged("HorizontalAlignment");
            }
        }

        VerticalAlignment mVerticalAlignment = VerticalAlignment.Stretch;
        [Category("布局")]
        [Rtti.MetaData]
        [EngineNS.Editor.UIEditor_BindingPropertyAttribute]
        [EngineNS.Editor.Editor_PropertyGridDataTemplateAttribute("VerticalAlignmentSetter")]
        public VerticalAlignment VerticalAlignment
        {
            get => mVerticalAlignment;
            set
            {
                mVerticalAlignment = value;
                Content?.UpdateLayout();
                OnPropertyChanged("VerticalAlignment");
            }
        }

        EngineNS.Thickness mMargin = EngineNS.Thickness.Empty;
        [Category("布局")]
        [Rtti.MetaData]
        [EngineNS.Editor.UIEditor_BindingPropertyAttribute]
        [EngineNS.Editor.Editor_UseCustomEditorAttribute]
        public Thickness Margin
        {
            get => mMargin;
            set
            {
                if (mMargin == value)
                    return;
                mMargin = value;
                Content?.UpdateLayout();
                OnPropertyChanged("Margin");
            }
        }

        // 不包含Margin的未裁剪的算出来的原始designSize
        EngineNS.SizeF mUnClippedDesiredSizeBox = EngineNS.SizeF.Empty;
        public override SizeF Measure(ref SizeF availableSize)
        {
            if (Content == null)
                return availableSize;

            var marginWidth = mMargin.Left + mMargin.Right;
            var marginHeight = mMargin.Top + mMargin.Bottom;

            var frameworkAvailableSize = new EngineNS.SizeF(Math.Max(availableSize.Width - marginWidth, 0), Math.Max(availableSize.Height - marginHeight, 0));
            switch (HorizontalAlignment)
            {
                case HorizontalAlignment.Center:
                case HorizontalAlignment.Left:
                case HorizontalAlignment.Right:
                    frameworkAvailableSize.Width = 0;
                    break;
                case HorizontalAlignment.Stretch:
                    break;
            }
            switch (VerticalAlignment)
            {
                case VerticalAlignment.Center:
                case VerticalAlignment.Bottom:
                case VerticalAlignment.Top:
                    frameworkAvailableSize.Height = 0;
                    break;
                case VerticalAlignment.Stretch:
                    break;
            }
            var desiredSize = Content.MeasureOverride(ref frameworkAvailableSize);

            var unclippedDesiredSize = desiredSize;
            bool clipped = false;

            var clippedDesiredWidth = desiredSize.Width + marginWidth;
            var clippedDesiredHeight = desiredSize.Height + marginHeight;
            if(clipped || clippedDesiredWidth < 0 || clippedDesiredHeight < 0)
            {
                mUnClippedDesiredSizeBox.Width = unclippedDesiredSize.Width;
                mUnClippedDesiredSizeBox.Height = unclippedDesiredSize.Height;
            }
            else
            {
                if (mUnClippedDesiredSizeBox != EngineNS.SizeF.Empty)
                    mUnClippedDesiredSizeBox = EngineNS.SizeF.Empty;
            }
            return new SizeF(Math.Max(0, clippedDesiredWidth), Math.Max(0, clippedDesiredHeight));
        }
        public override void Arrange(ref RectangleF finalRect)
        {
            // DesiredSize这里会带Margin数据
            if (Content == null)
                return;

            var arrangeSize = finalRect;
            var marginWidth = mMargin.Left + mMargin.Right;
            var marginHeight = mMargin.Top + mMargin.Bottom;
            var desiredSize = Content.DesiredSize;
            var desiredNoMarginWidth = desiredSize.Width - marginWidth;
            var desiredNoMarginHeight = desiredSize.Height - marginHeight;

            switch (HorizontalAlignment)
            {
                case HorizontalAlignment.Center:
                    {
                        arrangeSize.X = (finalRect.Width - desiredNoMarginWidth) * 0.5f + finalRect.Left + mMargin.Left - mMargin.Right;
                        arrangeSize.Width = Math.Max(desiredNoMarginWidth, 0);
                    }
                    break;
                case HorizontalAlignment.Left:
                    {
                        arrangeSize.X = finalRect.X + mMargin.Left;
                        arrangeSize.Width = Math.Max(desiredNoMarginWidth, 0);
                    }
                    break;
                case HorizontalAlignment.Right:
                    {
                        arrangeSize.X = finalRect.Width - desiredNoMarginWidth - mMargin.Right + finalRect.Left;
                        arrangeSize.Width = Math.Max(desiredNoMarginWidth, 0);
                    }
                    break;
                case HorizontalAlignment.Stretch:
                    {
                        arrangeSize.X = finalRect.X + mMargin.Left;
                        arrangeSize.Width = Math.Max(finalRect.Width - marginWidth, 0);
                    }
                    break;
            }
            switch (VerticalAlignment)
            {
                case VerticalAlignment.Center:
                    {
                        arrangeSize.Y = (finalRect.Height - desiredNoMarginHeight) * 0.5f + finalRect.Top + mMargin.Top - mMargin.Bottom;
                        arrangeSize.Height = Math.Max(desiredNoMarginHeight, 0);
                    }
                    break;
                case VerticalAlignment.Top:
                    {
                        arrangeSize.Y = finalRect.Y + mMargin.Top;
                        arrangeSize.Height = Math.Max(desiredNoMarginHeight, 0);
                    }
                    break;
                case VerticalAlignment.Bottom:
                    {
                        arrangeSize.Y = finalRect.Height - desiredNoMarginHeight - mMargin.Bottom + finalRect.Top;
                        arrangeSize.Height = Math.Max(desiredNoMarginHeight, 0);
                    }
                    break;
                case VerticalAlignment.Stretch:
                    {
                        arrangeSize.Y = finalRect.Y + mMargin.Top;
                        arrangeSize.Height = Math.Max(finalRect.Height - marginHeight, 0);
                    }
                    break;
            }


            EngineNS.SizeF unclippedDesiredSize;
            if(mUnClippedDesiredSizeBox == SizeF.Empty)
            {
                unclippedDesiredSize = new SizeF(Math.Max(0, desiredNoMarginWidth),
                                                          Math.Max(0, desiredNoMarginHeight));
            }
            else
            {
                unclippedDesiredSize = new SizeF(Math.Min(arrangeSize.Width, mUnClippedDesiredSizeBox.Width), 
                                                 Math.Min(arrangeSize.Height, mUnClippedDesiredSizeBox.Height));
            }

            arrangeSize.Width = unclippedDesiredSize.Width;
            arrangeSize.Height = unclippedDesiredSize.Height;

            Content.ArrangeOverride(ref arrangeSize);
        }

        public override void ProcessSetContentDesignRect(ref RectangleF tagRect)
        {
            Content.SetDesignRect(ref tagRect, true);
            var parentRect = this.Parent.DesignRect;
            var mgLeft = tagRect.Left - parentRect.Left;
            var mgTop = tagRect.Top - parentRect.Top;
            var mgRight = parentRect.Right - tagRect.Right;
            var mgBottom = parentRect.Bottom - tagRect.Bottom;
            float left = 0, top = 0, right = 0, bottom = 0;
            switch(HorizontalAlignment)
            {
                case HorizontalAlignment.Left:
                    left = mgLeft;
                    break;
                case HorizontalAlignment.Center:
                case HorizontalAlignment.Stretch:
                    left = mgLeft;
                    right = mgRight;
                    break;
                case HorizontalAlignment.Right:
                    right = mgRight;
                    break;
            }
            switch(VerticalAlignment)
            {
                case VerticalAlignment.Top:
                    top = mgTop;
                    break;
                case VerticalAlignment.Center:
                case VerticalAlignment.Stretch:
                    top = mgTop;
                    bottom = mgBottom;
                    break;
                case VerticalAlignment.Bottom:
                    bottom = mgBottom;
                    break;
            }
            mMargin = new Thickness(left, top, right, bottom);
            Content?.UpdateLayout();
            OnPropertyChanged("Margin");
        }

        public override bool NeedUpdateLayoutWhenChildDesiredSizeChanged(UIElement child)
        {
            switch(HorizontalAlignment)
            {
                case HorizontalAlignment.Left:
                case HorizontalAlignment.Center:
                case HorizontalAlignment.Right:
                    return true;
            }
            switch(VerticalAlignment)
            {
                case VerticalAlignment.Top:
                case VerticalAlignment.Center:
                case VerticalAlignment.Bottom:
                    return true;
            }
            return false;
        }
    }
}
