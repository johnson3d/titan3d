using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace EngineNS.UISystem.Controls.Containers
{
    [Rtti.MetaClass]
   
    public class CanvasSlot : UIContainerSlot
    {
        string mAnchorType = "";
        [Category("布局")]
        [DisplayName("锚点")]
        [EngineNS.Editor.Editor_PropertyGridDataTemplateAttribute("AnchorSelector")]
        public string AnchorType
        {
            get => mAnchorType;
            set
            {
                if (Content == null)
                    return;
                mAnchorType = value;

                var tagDesignRect = Content.DesignRect;

                switch (mAnchorType)
                {
                    case "LT":
                        mMinimum = new EngineNS.Vector2(0.0f, 0.0f);
                        mMaximum = new EngineNS.Vector2(0.0f, 0.0f);
                        break;
                    case "CT":
                        mMinimum = new EngineNS.Vector2(0.5f, 0.0f);
                        mMaximum = new EngineNS.Vector2(0.5f, 0.0f);
                        break;
                    case "RT":
                        mMinimum = new EngineNS.Vector2(1.0f, 0.0f);
                        mMaximum = new EngineNS.Vector2(1.0f, 0.0f);
                        break;
                    case "T":
                        mMinimum = new EngineNS.Vector2(0.0f, 0.0f);
                        mMaximum = new EngineNS.Vector2(1.0f, 0.0f);
                        break;
                    case "CL":
                        mMinimum = new EngineNS.Vector2(0.0f, 0.5f);
                        mMaximum = new EngineNS.Vector2(0.0f, 0.5f);
                        break;
                    case "Center":
                        mMinimum = new EngineNS.Vector2(0.5f, 0.5f);
                        mMaximum = new EngineNS.Vector2(0.5f, 0.5f);
                        break;
                    case "CR":
                        mMinimum = new EngineNS.Vector2(1.0f, 0.5f);
                        mMaximum = new EngineNS.Vector2(1.0f, 0.5f);
                        break;
                    case "CH":
                        mMinimum = new EngineNS.Vector2(0.0f, 0.5f);
                        mMaximum = new EngineNS.Vector2(1.0f, 0.5f);
                        break;
                    case "LB":
                        mMinimum = new EngineNS.Vector2(0.0f, 1.0f);
                        mMaximum = new EngineNS.Vector2(0.0f, 1.0f);
                        break;
                    case "CB":
                        mMinimum = new EngineNS.Vector2(0.5f, 1.0f);
                        mMaximum = new EngineNS.Vector2(0.5f, 1.0f);
                        break;
                    case "RB":
                        mMinimum = new EngineNS.Vector2(1.0f, 1.0f);
                        mMaximum = new EngineNS.Vector2(1.0f, 1.0f);
                        break;
                    case "B":
                        mMinimum = new EngineNS.Vector2(0.0f, 1.0f);
                        mMaximum = new EngineNS.Vector2(1.0f, 1.0f);
                        break;
                    case "L":
                        mMinimum = new EngineNS.Vector2(0.0f, 0.0f);
                        mMaximum = new EngineNS.Vector2(0.0f, 1.0f);
                        break;
                    case "CV":
                        mMinimum = new EngineNS.Vector2(0.5f, 0.0f);
                        mMaximum = new EngineNS.Vector2(0.5f, 1.0f);
                        break;
                    case "R":
                        mMinimum = new EngineNS.Vector2(1.0f, 0.0f);
                        mMaximum = new EngineNS.Vector2(1.0f, 1.0f);
                        break;
                    case "Fill":
                        mMinimum = new EngineNS.Vector2(0.0f, 0.0f);
                        mMaximum = new EngineNS.Vector2(1.0f, 1.0f);
                        break;
                }
                OnPropertyChanged("Minimum");
                OnPropertyChanged("Maximum");
                ProcessSetContentDesignRect(ref tagDesignRect);
            }
        }

        [Rtti.MetaData]
        public EngineNS.Vector2 mMinimum = new Vector2(0, 0);
        [Category("布局")]
        [DisplayName("最小值")]
        [EngineNS.Editor.MacrossMember(EngineNS.Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public EngineNS.Vector2 Minimum
        {
            get => mMinimum;
            set
            {
                mMinimum = value;
                if (mMinimum.X > mMaximum.X)
                    mMinimum.X = mMaximum.X;
                if (mMinimum.Y > mMaximum.Y)
                    mMinimum.Y = mMaximum.Y;
                Content?.UpdateLayout();
                OnPropertyChanged("Minimum");
            }
        }
        [Rtti.MetaData]
        public EngineNS.Vector2 mMaximum = new Vector2(0, 0);
        [Category("布局")]
        [DisplayName("最大值")]
        [EngineNS.Editor.MacrossMember(EngineNS.Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public EngineNS.Vector2 Maximum
        {
            get => mMaximum;
            set
            {
                mMaximum = value;
                if (mMaximum.X < mMinimum.X)
                    mMaximum.X = mMinimum.X;
                if (mMaximum.Y < mMinimum.Y)
                    mMaximum.Y = mMinimum.Y;
                Content?.UpdateLayout();
                OnPropertyChanged("Maximum");
            }
        }
        [EngineNS.Editor.Editor_MacrossClass(ECSType.Client, EngineNS.Editor.Editor_MacrossClassAttribute.enMacrossType.Useable)]
        class CanvasSlotPropertyUIProvider : EngineNS.Editor.Editor_PropertyGridUIProvider
        {
            public override string GetName(object arg)
            {
                var item = arg as CanvasSlotPropertyUIProviderItem;

                if((item.HostSlot.Maximum.X - item.HostSlot.Minimum.X) < EngineNS.CoreDefine.Epsilon)
                {
                    switch(item.NameType)
                    {
                        case CanvasSlotPropertyUIProviderItem.enNameType.X1:
                            return "位置X";
                        case CanvasSlotPropertyUIProviderItem.enNameType.X2:
                            return "尺寸X";
                    }
                }
                else
                {
                    switch(item.NameType)
                    {
                        case CanvasSlotPropertyUIProviderItem.enNameType.X1:
                            return "偏移左侧";
                        case CanvasSlotPropertyUIProviderItem.enNameType.X2:
                            return "偏移右侧";
                    }
                }

                if((item.HostSlot.Maximum.Y - item.HostSlot.Minimum.Y) < EngineNS.CoreDefine.Epsilon)
                {
                    switch(item.NameType)
                    {
                        case CanvasSlotPropertyUIProviderItem.enNameType.Y1:
                            return "位置Y";
                        case CanvasSlotPropertyUIProviderItem.enNameType.Y2:
                            return "尺寸Y";
                    }
                }
                else
                {
                    switch(item.NameType)
                    {
                        case CanvasSlotPropertyUIProviderItem.enNameType.Y1:
                            return "偏移顶部";
                        case CanvasSlotPropertyUIProviderItem.enNameType.Y2:
                            return "偏移底部";
                    }
                }

                return item.NameType.ToString();
            }
            public override Type GetUIType(object arg)
            {
                return typeof(float);
            }
            public override object GetValue(object arg)
            {
                var item = arg as CanvasSlotPropertyUIProviderItem;
                switch(item.NameType)
                {
                    case CanvasSlotPropertyUIProviderItem.enNameType.X1:
                        return item.HostSlot.X1;
                    case CanvasSlotPropertyUIProviderItem.enNameType.Y1:
                        return item.HostSlot.Y1;
                    case CanvasSlotPropertyUIProviderItem.enNameType.X2:
                        return item.HostSlot.X2;
                    case CanvasSlotPropertyUIProviderItem.enNameType.Y2:
                        return item.HostSlot.Y2;
                }
                return 0.0f;
            }
            public override void SetValue(object arg, object val)
            {
                var item = arg as CanvasSlotPropertyUIProviderItem;
                switch(item.NameType)
                {
                    case CanvasSlotPropertyUIProviderItem.enNameType.X1:
                        item.HostSlot.X1 = (float)val;
                        break;
                    case CanvasSlotPropertyUIProviderItem.enNameType.Y1:
                        item.HostSlot.Y1 = (float)val;
                        break;
                    case CanvasSlotPropertyUIProviderItem.enNameType.X2:
                        item.HostSlot.X2 = (float)val;
                        break;
                    case CanvasSlotPropertyUIProviderItem.enNameType.Y2:
                        item.HostSlot.Y2 = (float)val;
                        break;
                }
            }
        }
        class CanvasSlotPropertyUIProviderItem
        {
            public enum enNameType
            {
                X1,
                Y1,
                X2,
                Y2,
            }

            public CanvasSlot HostSlot;
            public enNameType NameType;
        }

        float mX1 = 0;
        [Browsable(false)]
        [Rtti.MetaData]
        [EngineNS.Editor.MacrossMember(EngineNS.Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public float X1
        {
            get => mX1;
            set
            {
                mX1 = value;
                Content?.UpdateLayout();
            }
        }
        [EngineNS.Editor.Editor_PropertyGridUIShowProvider(typeof(CanvasSlotPropertyUIProvider))]
        [Category("布局")]
        public object X1_Value
        {
            get
            {
                var elem = new CanvasSlotPropertyUIProviderItem()
                {
                    HostSlot = this,
                    NameType = CanvasSlotPropertyUIProviderItem.enNameType.X1
                };
                return elem;
            }
        }
        float mY1 = 0;
        [Browsable(false)]
        [Rtti.MetaData]
        [EngineNS.Editor.MacrossMember(EngineNS.Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public float Y1
        {
            get => mY1;
            set
            {
                mY1 = value;
                Content?.UpdateLayout();
            }
        }
        [EngineNS.Editor.Editor_PropertyGridUIShowProvider(typeof(CanvasSlotPropertyUIProvider))]
        [Category("布局")]
        public object Y1_Value
        {
            get
            {
                var elem = new CanvasSlotPropertyUIProviderItem()
                {
                    HostSlot = this,
                    NameType = CanvasSlotPropertyUIProviderItem.enNameType.Y1
                };
                return elem;
            }
        }
        float mX2 = 100;
        [Browsable(false)]
        [Rtti.MetaData]
        [EngineNS.Editor.MacrossMember(EngineNS.Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public float X2
        {
            get => mX2;
            set
            {
                mX2 = value;
                Content?.UpdateLayout();
            }
        }
        [EngineNS.Editor.Editor_PropertyGridUIShowProvider(typeof(CanvasSlotPropertyUIProvider))]
        [Category("布局")]
        public object X2_Value
        {
            get
            {
                var elem = new CanvasSlotPropertyUIProviderItem()
                {
                    HostSlot = this,
                    NameType = CanvasSlotPropertyUIProviderItem.enNameType.X2
                };
                return elem;
            }
        }
        float mY2 = 100;
        [Browsable(false)]
        [Rtti.MetaData]
        [EngineNS.Editor.MacrossMember(EngineNS.Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public float Y2
        {
            get => mY2;
            set
            {
                mY2 = value;
                Content?.UpdateLayout();
            }
        }
        [EngineNS.Editor.Editor_PropertyGridUIShowProvider(typeof(CanvasSlotPropertyUIProvider))]
        [Category("布局")]
        public object Y2_Value
        {
            get
            {
                var elem = new CanvasSlotPropertyUIProviderItem()
                {
                    HostSlot = this,
                    NameType = CanvasSlotPropertyUIProviderItem.enNameType.Y2
                };
                return elem;
            }
        }

        bool mSizeToContent = false;
        [Rtti.MetaData]
        [Category("布局")]
        public bool SizeToContent
        {
            get => mSizeToContent;
            set
            {
                mSizeToContent = value;
                Content?.UpdateLayout();
                OnPropertyChanged("SizeToContent");
            }
        }

        int mZOrder = 0;
        [Rtti.MetaData]
        [Category("布局")]
        [Browsable(false)]  // 功能没有，暂时不显示
        public int ZOrder
        {
            get => mZOrder;
            set
            {
                mZOrder = value;
                OnPropertyChanged("ZOrder");
            }
        }

        SizeF mOrigionSize;
        public override SizeF Measure(ref SizeF totalSize)
        {
            var widthDelta = Maximum.X - Minimum.X;
            var heightDelta = Maximum.Y - Minimum.Y;

            var availableSize = new SizeF(totalSize.Width * widthDelta, totalSize.Height * heightDelta);
            var frameworkAvailableSize = EngineNS.SizeF.Empty;
            if (widthDelta < EngineNS.CoreDefine.Epsilon)
            {
                // 位置与大小
                frameworkAvailableSize.Width = X2;
            }
            else
            {
                // 偏移
                var marginWidth = X1 + X2;
                frameworkAvailableSize.Width = Math.Max(availableSize.Width - marginWidth, 0);
            }
            if(heightDelta < EngineNS.CoreDefine.Epsilon)
            {
                frameworkAvailableSize.Height = Y2;
            }
            else
            {
                var marginHeight = Y1 + Y2;
                frameworkAvailableSize.Height = Math.Max(availableSize.Height - marginHeight, 0);
            }

            var desiredSize = Content.MeasureOverride(ref frameworkAvailableSize);
            mOrigionSize = desiredSize;//frameworkAvailableSize;
            if (widthDelta >= EngineNS.CoreDefine.Epsilon)
            {
                if (SizeToContent)
                    desiredSize.Width = desiredSize.Width + mX1 + mX2;
                else
                    desiredSize.Width = availableSize.Width;// Parent.DesignRect.Width - mX1 - mX2;
            }
            else
            {
                if (SizeToContent)
                    desiredSize.Width = desiredSize.Width + mX1;
                else
                    desiredSize.Width = mX1 + mX2;
            }
            if (heightDelta >= EngineNS.CoreDefine.Epsilon)
            {
                if (SizeToContent)
                    desiredSize.Height = desiredSize.Height + mY1 + mY2;
                else
                    desiredSize.Height = availableSize.Height;// Parent.DesignRect.Height - mY1 - mY2;
            }
            else
            {
                if (SizeToContent)
                    desiredSize.Height = desiredSize.Height + mY1;
                else
                    desiredSize.Height = mY1 + mY2;
            }

            return desiredSize;
        }
        public override void Arrange(ref EngineNS.RectangleF finalRect)
        {
            var arrangeSize = finalRect;

            var desiredSize = Content.DesiredSize;
            var left = finalRect.Width * Minimum.X + finalRect.Left + X1;
            var top = finalRect.Height * Minimum.Y + finalRect.Top + Y1;
            arrangeSize.X = left;
            arrangeSize.Y = top;
            if(SizeToContent)
            {
                arrangeSize.Width = mOrigionSize.Width;
                arrangeSize.Height = mOrigionSize.Height;
            }
            else
            {
                var widthDelta = Maximum.X - Minimum.X;
                var heightDelta = Maximum.Y - Minimum.Y;

                if (widthDelta < EngineNS.CoreDefine.Epsilon)
                    arrangeSize.Width = X2;     // X2为大小
                else
                    arrangeSize.Width = Math.Max(finalRect.Width - (X1 + X2), 0);   // X2为偏移
                if (heightDelta < EngineNS.CoreDefine.Epsilon)
                    arrangeSize.Height = Y2;    // Y2为大小
                else
                    arrangeSize.Height = Math.Max(finalRect.Height - (Y1 + Y2), 0); // Y2为偏移
            }

            Content.ArrangeOverride(ref arrangeSize);
        }
        public override void ProcessSetContentDesignRect(ref RectangleF tagRect)
        {
            var designRect = Content.DesignRect;
            var parentDesignRect = Parent.DesignRect;
            var left = mMinimum.X * parentDesignRect.Width + parentDesignRect.Left;
            var top = mMinimum.Y * parentDesignRect.Height + parentDesignRect.Top;
            var width = (mMaximum.X - mMinimum.X) * parentDesignRect.Width;
            var height = (mMaximum.Y - mMinimum.Y) * parentDesignRect.Height;
            if((mMaximum.X - mMinimum.X) < EngineNS.CoreDefine.Epsilon)
            {
                mX1 = tagRect.Left - left;
                OnPropertyChanged("X1_Value");
                mX2 = tagRect.Width;
                OnPropertyChanged("X2_Value");
            }
            else
            {
                mX1 = tagRect.Left - left;
                OnPropertyChanged("X1_Value");
                mX2 = left + width - tagRect.Right;
                OnPropertyChanged("X2_Value");
            }
            if ((mMaximum.Y - mMinimum.Y) < EngineNS.CoreDefine.Epsilon)
            {
                mY1 = tagRect.Top - top;
                OnPropertyChanged("Y1_Value");
                mY2 = tagRect.Height;
                OnPropertyChanged("Y2_Value");
            }
            else
            {
                mY1 = tagRect.Top - top;
                OnPropertyChanged("Y1_Value");
                mY2 = top + height - tagRect.Bottom;
                OnPropertyChanged("Y2_Value");
            }
            Content?.SetDesignRect(ref tagRect, true);
        }
        public override bool NeedUpdateLayoutWhenChildDesiredSizeChanged(UIElement child)
        {
            return true;
        }

        public override Type GetSlotOperatorType()
        {
            return typeof(CanvasSlotOperator);
        }
    }
}
