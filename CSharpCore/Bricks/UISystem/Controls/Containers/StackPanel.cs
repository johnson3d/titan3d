using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Threading.Tasks;

namespace EngineNS.UISystem.Controls.Containers
{
    [Rtti.MetaClass]
    public class StackPanelInitializer : PanelInitializer
    {
        public enum enOrientationType
        {
            LeftToRight,
            RightToLeft,
            TopToBottom,
            BottomToTop,
        }

        [Rtti.MetaData]
        public enOrientationType Orientation
        {
            get;
            set;
        } = enOrientationType.TopToBottom;
    }
    [Editor_UIControlInit(typeof(StackPanelInitializer))]
    [Editor_UIControl("容器.StackPanel", "", "VerticalBox.png")]
    public class StackPanel : Panel
    {
        [Category("布局")]
        public StackPanelInitializer.enOrientationType Orientation
        {
            get => ((StackPanelInitializer)Initializer).Orientation;
            set
            {
                var init = (StackPanelInitializer)Initializer;
                if (init.Orientation == value)
                    return;

                init.Orientation = value;
                for(int i=0; i<mChildrenUIElements.Count; i++)
                {
                    mChildrenUIElements[i].ResetPreviousAvailableSize();
                }
                UpdateLayout();
                OnPropertyChanged("Orientation");
            }
        }

        public override async Task<bool> Initialize(CRenderContext rc, UIElementInitializer init)
        {
            return await base.Initialize(rc, init);
        }

        public override void AddChild(UIElement element, bool updateLayout = true)
        {
            base.AddChild(element, updateLayout);
            if(element.Slot != null)
            {
                System.Diagnostics.Debug.Assert(element.Slot.GetType() == typeof(StackPanelSlot));
                element.Slot.Parent = this;
                element.Slot.Content = element;
            }
            else
            {
                var slot = new StackPanelSlot();
                slot.Parent = this;
                slot.Content = element;
                element.Slot = slot;
            }
        }
        public override void InsertChild(int index, UIElement element, bool updateLayout = true)
        {
            base.InsertChild(index, element, updateLayout);
            if (element.Slot != null)
            {
                System.Diagnostics.Debug.Assert(element.Slot.GetType() == typeof(StackPanelSlot));
                element.Slot.Parent = this;
                element.Slot.Content = element;
            }
            else
            {
                var slot = new StackPanelSlot();
                slot.Parent = this;
                slot.Content = element;
                element.Slot = slot;
            }
        }

        public override SizeF MeasureOverride(ref SizeF availableSize)
        {
            var retSize = SizeF.Empty;
            var restAvailableSize = availableSize;
            for(int i=0; i<mChildrenUIElements.Count; i++)
            {
                var child = mChildrenUIElements[i];
                child.Measure(ref restAvailableSize);

                switch(Orientation)
                {
                    case StackPanelInitializer.enOrientationType.TopToBottom:
                    case StackPanelInitializer.enOrientationType.BottomToTop:
                        {
                            retSize.Width = System.Math.Max(child.DesiredSize.Width, retSize.Width);
                            retSize.Height += child.DesiredSize.Height;
                            if (!float.IsInfinity(restAvailableSize.Height))
                                restAvailableSize.Height -= child.DesiredSize.Height;
                        }
                        break;
                    case StackPanelInitializer.enOrientationType.LeftToRight:
                    case StackPanelInitializer.enOrientationType.RightToLeft:
                        {
                            retSize.Width += child.DesiredSize.Width;
                            retSize.Height = System.Math.Max(child.DesiredSize.Height, retSize.Height);
                            if (!float.IsInfinity(restAvailableSize.Width))
                                restAvailableSize.Width -= child.DesiredSize.Width;
                        }
                        break;
                }
            }
            return retSize;
        }
        public override void ArrangeOverride(ref RectangleF arrangeSize)
        {
            if (mInitializer == null)
                return;

            if (!mInitializer.DesignRect.Equals(ref arrangeSize))
            {
                DesignRect = arrangeSize;
                UpdateDesignClipRect();
            }

            var restArrangeSize = arrangeSize;
            for(int i=0; i<mChildrenUIElements.Count; i++)
            {
                var child = mChildrenUIElements[i];
                switch(Orientation)
                {
                    case StackPanelInitializer.enOrientationType.TopToBottom:
                        {
                            var childDesiredSize = child.DesiredSize;
                            var childRect = new RectangleF(restArrangeSize.X, restArrangeSize.Y, arrangeSize.Width, childDesiredSize.Height);
                            child.Arrange(ref childRect);
                            restArrangeSize.Y += childDesiredSize.Height;
                        }
                        break;
                    case StackPanelInitializer.enOrientationType.BottomToTop:
                        {
                            var childDesiredSize = child.DesiredSize;
                            var childRect = new RectangleF(restArrangeSize.X, restArrangeSize.Y + restArrangeSize.Height - childDesiredSize.Height, arrangeSize.Width, childDesiredSize.Height);
                            child.Arrange(ref childRect);
                            restArrangeSize.Height -= childDesiredSize.Height;
                        }
                        break;
                    case StackPanelInitializer.enOrientationType.LeftToRight:
                        {
                            var childDesiredSize = child.DesiredSize;
                            var childRect = new RectangleF(restArrangeSize.X, restArrangeSize.Y, childDesiredSize.Width, arrangeSize.Height);
                            child.Arrange(ref childRect);
                            restArrangeSize.X += childDesiredSize.Width;
                        }
                        break;
                    case StackPanelInitializer.enOrientationType.RightToLeft:
                        {
                            var childDesiredSize = child.DesiredSize;
                            var childRect = new RectangleF(restArrangeSize.X + restArrangeSize.Width - childDesiredSize.Width, restArrangeSize.Y, childDesiredSize.Width, arrangeSize.Height);
                            child.Arrange(ref childRect);
                            restArrangeSize.Width -= childDesiredSize.Width;
                        }
                        break;
                }
            }
        }
    }
    [Rtti.MetaClass]
    public class StackPanelSlot : UIContainerSlot
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
                Parent?.UpdateLayout();
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
                Parent?.UpdateLayout();
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
                Parent?.UpdateLayout();
                Content?.UpdateLayout();
                OnPropertyChanged("Margin");
            }
        }

        // 不包含Margin的未裁剪的算出来的原始designSize
        //EngineNS.SizeF mUnClippedDesiredSizeBox = EngineNS.SizeF.Empty;
        public override SizeF Measure(ref SizeF availableSize)
        {
            if (Content == null)
                return availableSize;

            var parentStackPanel = Parent as StackPanel;

            var marginWidth = mMargin.Left + mMargin.Right;
            var marginHeight = mMargin.Top + mMargin.Bottom;

            var frameworkAvailableSize = new EngineNS.SizeF(Math.Max(availableSize.Width - marginWidth, 0), Math.Max(availableSize.Height - marginHeight, 0));
            var tempAvailableSize = frameworkAvailableSize;
            if (parentStackPanel.Orientation == StackPanelInitializer.enOrientationType.TopToBottom ||
               parentStackPanel.Orientation == StackPanelInitializer.enOrientationType.BottomToTop)
            {
                switch (HorizontalAlignment)
                {
                    case HorizontalAlignment.Center:
                    case HorizontalAlignment.Left:
                    case HorizontalAlignment.Right:
                        tempAvailableSize.Width = 0;
                        break;
                    case HorizontalAlignment.Stretch:
                        break;
                }
            }
            if (parentStackPanel.Orientation == StackPanelInitializer.enOrientationType.LeftToRight ||
               parentStackPanel.Orientation == StackPanelInitializer.enOrientationType.RightToLeft)
            {
                switch (VerticalAlignment)
                {
                    case VerticalAlignment.Center:
                    case VerticalAlignment.Bottom:
                    case VerticalAlignment.Top:
                        tempAvailableSize.Height = 0;
                        break;
                    case VerticalAlignment.Stretch:
                        break;
                }
            }
            var desiredSize = Content.MeasureOverride(ref tempAvailableSize);
            //mUnClippedDesiredSizeBox = desiredSize;
            //if (parentStackPanel.Orientation == StackPanelInitializer.enOrientationType.TopToBottom ||
            //   parentStackPanel.Orientation == StackPanelInitializer.enOrientationType.BottomToTop)
            //{
            //    desiredSize.Width = frameworkAvailableSize.Width;
            //}

            //if (parentStackPanel.Orientation == StackPanelInitializer.enOrientationType.LeftToRight ||
            //   parentStackPanel.Orientation == StackPanelInitializer.enOrientationType.RightToLeft)
            //{
            //    desiredSize.Height = frameworkAvailableSize.Height;
            //}

            var unclippedDesiredSize = desiredSize;
            //bool clipped = false;

            var clippedDesiredWidth = desiredSize.Width + marginWidth;
            var clippedDesiredHeight = desiredSize.Height + marginHeight;
            //if (clippedDesiredWidth > availableSize.Width)
            //{
            //    clippedDesiredWidth = availableSize.Width;
            //    clipped = true;
            //}
            //if (clippedDesiredHeight > availableSize.Height)
            //{
            //    clippedDesiredHeight = availableSize.Height;
            //    clipped = true;
            //}
            //if (clipped || clippedDesiredWidth < 0 || clippedDesiredHeight < 0)
            //{
            //    mUnClippedDesiredSizeBox.Width = unclippedDesiredSize.Width;
            //    mUnClippedDesiredSizeBox.Height = unclippedDesiredSize.Height;
            //}
            //else
            //{
            //    if (mUnClippedDesiredSizeBox != EngineNS.SizeF.Empty)
            //        mUnClippedDesiredSizeBox = EngineNS.SizeF.Empty;
            //}
            return new SizeF(Math.Max(0, clippedDesiredWidth), Math.Max(0, clippedDesiredHeight));
        }
        public override void Arrange(ref RectangleF finalRect)
        {
            // DesiredSize这里会带Margin数据
            if (Content == null)
                return;

            var parentStackPanel = Parent as StackPanel;

            var arrangeSize = finalRect;
            var marginWidth = mMargin.Left + mMargin.Right;
            var marginHeight = mMargin.Top + mMargin.Bottom;
            var desiredSize = Content.DesiredSize;
            var desiredNoMarginWidth = desiredSize.Width - marginWidth;
            var desiredNoMarginHeight = desiredSize.Height - marginHeight;

            if (parentStackPanel.Orientation == StackPanelInitializer.enOrientationType.TopToBottom ||
                parentStackPanel.Orientation == StackPanelInitializer.enOrientationType.BottomToTop)
            {
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
                arrangeSize.Height = Math.Max(finalRect.Height - marginHeight, 0);
                arrangeSize.Y = finalRect.Y + mMargin.Top;
            }
            if (parentStackPanel.Orientation == StackPanelInitializer.enOrientationType.LeftToRight ||
                parentStackPanel.Orientation == StackPanelInitializer.enOrientationType.RightToLeft)
            {
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
                arrangeSize.Width = Math.Max(finalRect.Width - marginWidth, 0);
                arrangeSize.X = finalRect.X + mMargin.Left;
            }


            //EngineNS.SizeF unclippedDesiredSize;
            //if (mUnClippedDesiredSizeBox == SizeF.Empty)
            //{
            //    unclippedDesiredSize = new SizeF(Math.Max(0, desiredNoMarginWidth),
            //                                              Math.Max(0, desiredNoMarginHeight));
            //}
            //else
            //{
            //    unclippedDesiredSize = new SizeF(mUnClippedDesiredSizeBox.Width, mUnClippedDesiredSizeBox.Height);
            //}

            //arrangeSize.Width = unclippedDesiredSize.Width;
            //arrangeSize.Height = unclippedDesiredSize.Height;

            Content.ArrangeOverride(ref arrangeSize);
        }

        public override void ProcessSetContentDesignRect(ref RectangleF tagRect)
        {
            var targetRect = RectangleF.Empty;
            var parentSP = Parent as StackPanel;
            var parentDesignRect = parentSP.DesignRect;

            var totalChildrenSize = SizeF.Empty;
            for(int i=0; i<parentSP.ChildrenUIElements.Count; i++)
            {
                totalChildrenSize.Width += parentSP.ChildrenUIElements[i].DesignRect.Width;
                totalChildrenSize.Height += parentSP.ChildrenUIElements[i].DesignRect.Height;
            }

            if(parentSP.Orientation != StackPanelInitializer.enOrientationType.LeftToRight &&
               parentSP.Orientation != StackPanelInitializer.enOrientationType.RightToLeft)
            {
                switch(HorizontalAlignment)
                {
                    case HorizontalAlignment.Center:
                    case HorizontalAlignment.Left:
                    case HorizontalAlignment.Right:
                        targetRect.Width = tagRect.Width;
                        break;
                    case HorizontalAlignment.Stretch:
                        targetRect.Width = parentDesignRect.Width;
                        break;
                }
                targetRect.Height = tagRect.Height;
                targetRect.X = parentDesignRect.X;
                if (parentSP.Orientation == StackPanelInitializer.enOrientationType.TopToBottom)
                    targetRect.Y = parentDesignRect.Y + totalChildrenSize.Height;
                else if (parentSP.Orientation == StackPanelInitializer.enOrientationType.BottomToTop)
                    targetRect.Y = parentDesignRect.Y + parentDesignRect.Height - totalChildrenSize.Height - targetRect.Height;
            }
            if(parentSP.Orientation != StackPanelInitializer.enOrientationType.TopToBottom &&
               parentSP.Orientation != StackPanelInitializer.enOrientationType.BottomToTop)
            {
                switch(VerticalAlignment)
                {
                    case VerticalAlignment.Center:
                    case VerticalAlignment.Top:
                    case VerticalAlignment.Bottom:
                        targetRect.Height = tagRect.Height;
                        break;
                    case VerticalAlignment.Stretch:
                        targetRect.Height = parentDesignRect.Height;
                        break;
                }
                targetRect.Width = tagRect.Width;
                targetRect.Y = parentDesignRect.Y;
                if (parentSP.Orientation == StackPanelInitializer.enOrientationType.LeftToRight)
                    targetRect.X = parentDesignRect.X + totalChildrenSize.Width;
                else if (parentSP.Orientation == StackPanelInitializer.enOrientationType.RightToLeft)
                    targetRect.X = parentDesignRect.X + parentDesignRect.Width - totalChildrenSize.Width - targetRect.Width;
            }
                        
            Content.SetDesignRect(ref targetRect, true);
            //mMargin = new Thickness(0);
            Content?.UpdateLayout();
            //OnPropertyChanged("Margin");
        }

        public override bool NeedUpdateLayoutWhenChildDesiredSizeChanged(UIElement child)
        {
            switch (HorizontalAlignment)
            {
                case HorizontalAlignment.Left:
                case HorizontalAlignment.Center:
                case HorizontalAlignment.Right:
                    return true;
            }
            switch (VerticalAlignment)
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
