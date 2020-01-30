using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Threading.Tasks;
using EngineNS.Graphics.View;
using EngineNS.Profiler;

namespace EngineNS.UISystem.Controls
{
    [Rtti.MetaClass]
    public class JoysticksInitializer : UIElementInitializer
    {
        [Rtti.MetaData]
        public bool Enable { get; set; } = true;
        [Rtti.MetaData]
        public EngineNS.SizeF BackgroundSize { get; set; } = new SizeF(100, 100);
        [Rtti.MetaData]
        public bool BackgroundUseImageSize { get; set; } = true;
        [Rtti.MetaData]
        public Brush BackgroundBrush { get; set; } = new Brush();
        [Rtti.MetaData]
        public bool ShowBackground { get; set; } = true;
        [Rtti.MetaData]
        public HorizontalAlignment BackgroundHorizontalAlignment { get; set; } = HorizontalAlignment.Center;
        [Rtti.MetaData]
        public VerticalAlignment BackgroundVerticalAlignment { get; set; } = VerticalAlignment.Center;
        [Rtti.MetaData]
        public Thickness BackgroundMargin { get; set; }

        [Rtti.MetaData]
        public EngineNS.SizeF ThumbSize { get; set; } = new SizeF(50, 50);
        [Rtti.MetaData]
        public bool ThumbUseImageSize { get; set; } = true;
        [Rtti.MetaData]
        public Style.ButtonStyle ThumbStyle { get; set; } = new Style.ButtonStyle();
        [Rtti.MetaData]
        public float ThumbMoveRadius { get; set; } = 100.0f;
        public enum enPosType
        {
            Freeze, // 固定位置
            Manual, // 根据点击计算位置
        }
        [Rtti.MetaData]
        public enPosType PosType { get; set; } = enPosType.Freeze;

        public JoysticksInitializer()
        {
            BackgroundBrush.ImageName = EngineNS.RName.GetRName("ui/uv_ui_joysticks_background.uvanim", RName.enRNameType.Engine);
            ThumbStyle.NormalBrush.ImageName = EngineNS.RName.GetRName("ui/uv_ui_joysticks_thumb.uvanim", RName.enRNameType.Engine);
            ThumbStyle.HoveredBrush.ImageName = EngineNS.RName.GetRName("ui/uv_ui_joysticks_thumb.uvanim", RName.enRNameType.Engine);
            ThumbStyle.PressedBrush.ImageName = EngineNS.RName.GetRName("ui/uv_ui_joysticks_thumb.uvanim", RName.enRNameType.Engine);
            ThumbStyle.DisabledBrush.ImageName = EngineNS.RName.GetRName("ui/uv_ui_joysticks_thumb.uvanim", RName.enRNameType.Engine);
        }
    }
    public class JoystickControl : UIElement
    {
        public Brush BackgroundBrush;
        public Brush ThumbBrush;
        public Joysticks HostJoystick;

        // -1.0f -- 1.0f
        float mXValue = 0.0f;
        public float XValue
        {
            get => mXValue;
            set
            {
                mXValue = value;
                HostJoystick?.OnValueChangedCallback(mXValue, mYValue);
                OnPropertyChanged("XValue");
            }
        }
        // -1.0f -- 1.0f
        float mYValue = 0.0f;
        public float YValue
        {
            get => mYValue;
            set
            {
                mYValue = value;
                HostJoystick?.OnValueChangedCallback(mXValue, mYValue);
                OnPropertyChanged("YValue");
            }
        }
        RectangleF mBackgroundRect = new RectangleF(0, 0, 100, 100);
        public RectangleF BackgroundRect => mBackgroundRect;
        RectangleF mThumbRect = new RectangleF(0, 0, 100, 100);
        public RectangleF ThumbRect => mThumbRect;

        public override async Task<bool> Initialize(CRenderContext rc, UIElementInitializer init)
        {
            if (await base.Initialize(rc, init) == false)
                return false;
            return true;
        }
        public override void UpdateLayout()
        {

        }
        public override SizeF MeasureOverride(ref SizeF availableSize)
        {
            var init = Initializer as JoysticksInitializer;
            mBackgroundRect.Size = init.BackgroundSize;
            if (init.BackgroundUseImageSize)
            {
                var brush = init.BackgroundBrush;
                if (brush == null)
                    mBackgroundRect.Size = SizeF.Empty;
                var imgSize = brush.ImageSize;
                mBackgroundRect.Size = new SizeF(imgSize.X, imgSize.Y);
            }
            mThumbRect.Size = init.ThumbSize;
            if (init.ThumbUseImageSize)
            {
                var brush = init.ThumbStyle.NormalBrush;
                if (brush == null)
                    mThumbRect.Size = SizeF.Empty;
                var imgSize = brush.ImageSize;
                mThumbRect.Size = new SizeF(imgSize.X, imgSize.Y);
            }
            return new SizeF(Math.Max(availableSize.Width, Math.Max(mBackgroundRect.Width, init.ThumbMoveRadius * 2)),
                             Math.Max(availableSize.Height, Math.Max(mBackgroundRect.Height, init.ThumbMoveRadius * 2)));
        }
        public PointF CenterOffset = PointF.Empty;
        public override void ArrangeOverride(ref RectangleF arrangeSize)
        {
            var init = Initializer as JoysticksInitializer;

            var center = new PointF(arrangeSize.X + arrangeSize.Width * 0.5f + CenterOffset.X, arrangeSize.Y + arrangeSize.Height * 0.5f + CenterOffset.Y);
            mBackgroundRect.X = center.X - mBackgroundRect.Width * 0.5f;
            mBackgroundRect.Y = center.Y - mBackgroundRect.Height * 0.5f;

            var vec = new Vector2(XValue, YValue);
            var len = vec.Length();
            if (len > 1 || len < -1)
            {
                vec.Normalize();
            }
            var delta = vec * init.ThumbMoveRadius;
            mThumbRect.X = center.X - mThumbRect.Width * 0.5f + delta.X;
            mThumbRect.Y = center.Y - mThumbRect.Height * 0.5f + delta.Y;
        }
        public override bool Commit(CCommandList cmd, ref Matrix parentTransformMatrix, float dpiScale)
        {
            if (IsRenderable() == false)
                return false;

            var init = Initializer as JoysticksInitializer;
            if(init.ShowBackground)
                BackgroundBrush?.CommitUVAnim(cmd, ref mBackgroundRect, ref mBackgroundRect, ref parentTransformMatrix, dpiScale);
            ThumbBrush?.CommitUVAnim(cmd, ref mThumbRect, ref mThumbRect, ref parentTransformMatrix, dpiScale);
            return true;
        }
        public override bool Draw(CRenderContext rc, CCommandList cmd, CGfxScreenView view)
        {
            if (IsRenderable() == false)
                return false;

            var init = Initializer as JoysticksInitializer;
            if(init.ShowBackground)
                BackgroundBrush?.Draw(rc, cmd, view);
            ThumbBrush?.Draw(rc, cmd, view);
            return true;
        }
    }
    [EngineNS.Editor.Editor_MacrossClass(ECSType.Client, EngineNS.Editor.Editor_MacrossClassAttribute.enMacrossType.AllFeatures)]
    [Editor_UIControlInit(typeof(JoysticksInitializer))]
    [Editor_UIControl("通用.摇杆", "摇杆控件", "Joysticks.png")]
    public class Joysticks : UIElement
    {
        [EngineNS.Editor.UIEditor_BindingPropertyAttribute]
        [EngineNS.Editor.MacrossMember(EngineNS.Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public bool Enable
        {
            get => ((JoysticksInitializer)mInitializer).Enable;
            set
            {
                var init = mInitializer as JoysticksInitializer;
                init.Enable = value;
                UpdateCurrentBrush();
                OnPropertyChanged("Enable");
            }
        }

        [Category("背景")]
        [EngineNS.Editor.UIEditor_BindingPropertyAttribute]
        [EngineNS.Editor.MacrossMember(EngineNS.Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public EngineNS.SizeF BackgroundSize
        {
            get => ((JoysticksInitializer)mInitializer).BackgroundSize;
            set
            {
                var init = mInitializer as JoysticksInitializer;
                init.BackgroundSize = value;
                UpdateJoystickControlLayout();
                OnPropertyChanged("BackgroundSize");
            }
        }
        [Category("背景")]
        [EngineNS.Editor.UIEditor_BindingPropertyAttribute]
        [EngineNS.Editor.MacrossMember(EngineNS.Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public bool BackgroundUseImageSize
        {
            get => ((JoysticksInitializer)mInitializer).BackgroundUseImageSize;
            set
            {
                ((JoysticksInitializer)mInitializer).BackgroundUseImageSize = value;
                UpdateJoystickControlLayout();
                OnPropertyChanged("BackgroundUseImageSize");
            }
        }
        [Category("背景")]
        [EngineNS.Editor.UIEditor_BindingPropertyAttribute]
        [EngineNS.Editor.MacrossMember(EngineNS.Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public Brush BackgroundBrush
        {
            get => ((JoysticksInitializer)mInitializer).BackgroundBrush;
        }
        [Category("背景")]
        [EngineNS.Editor.UIEditor_BindingPropertyAttribute]
        [EngineNS.Editor.MacrossMember(EngineNS.Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public bool ShowBackground
        {
            get => ((JoysticksInitializer)mInitializer).ShowBackground;
            set
            {
                ((JoysticksInitializer)mInitializer).ShowBackground = value;
                OnPropertyChanged("ShowBackground");
            }
        }
        [Category("背景")]
        [EngineNS.Editor.UIEditor_BindingPropertyAttribute]
        [EngineNS.Editor.MacrossMember(EngineNS.Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        [EngineNS.Editor.Editor_PropertyGridDataTemplateAttribute("HorizontalAlignmentSetter")]
        public HorizontalAlignment BackgroundHorizontalAlignment
        {
            get => ((JoysticksInitializer)mInitializer).BackgroundHorizontalAlignment;
            set
            {
                ((JoysticksInitializer)mInitializer).BackgroundHorizontalAlignment = value;
                if (mJoysticksControlSlot != null)
                    mJoysticksControlSlot.HorizontalAlignment = value;
                UpdateJoystickControlLayout();
                OnPropertyChanged("BackgroundHorizontalAlignment");
            }
        }
        [Category("背景")]
        [EngineNS.Editor.UIEditor_BindingPropertyAttribute]
        [EngineNS.Editor.MacrossMember(EngineNS.Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        [EngineNS.Editor.Editor_PropertyGridDataTemplateAttribute("VerticalAlignmentSetter")]
        public VerticalAlignment BackgroundVerticalAlignment
        {
            get => ((JoysticksInitializer)mInitializer).BackgroundVerticalAlignment;
            set
            {
                ((JoysticksInitializer)mInitializer).BackgroundVerticalAlignment = value;
                if (mJoysticksControlSlot != null)
                    mJoysticksControlSlot.VerticalAlignment = value;
                UpdateJoystickControlLayout();
                OnPropertyChanged("BackgroundVerticalAlignment");
            }
        }
        [Category("背景")]
        [EngineNS.Editor.UIEditor_BindingPropertyAttribute]
        [EngineNS.Editor.MacrossMember(EngineNS.Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        [EngineNS.Editor.Editor_UseCustomEditorAttribute]
        public Thickness BackgroundMargin
        {
            get => ((JoysticksInitializer)mInitializer).BackgroundMargin;
            set
            {
                ((JoysticksInitializer)mInitializer).BackgroundMargin = value;
                if (mJoysticksControlSlot != null)
                    mJoysticksControlSlot.Margin = value;
                UpdateJoystickControlLayout();
                OnPropertyChanged("BackgroundMargin");
            }
        }

        [Category("滑块")]
        [EngineNS.Editor.UIEditor_BindingPropertyAttribute]
        [EngineNS.Editor.MacrossMember(EngineNS.Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public EngineNS.SizeF ThumbSize
        {
            get => ((JoysticksInitializer)mInitializer).ThumbSize;
            set
            {
                ((JoysticksInitializer)mInitializer).ThumbSize = value;
                UpdateJoystickControlLayout();
                OnPropertyChanged("ThumbSize");
            }
        }
        [Category("滑块")]
        [EngineNS.Editor.UIEditor_BindingPropertyAttribute]
        [EngineNS.Editor.MacrossMember(EngineNS.Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public bool ThumbUseImageSize
        {
            get => ((JoysticksInitializer)mInitializer).ThumbUseImageSize;
            set
            {
                ((JoysticksInitializer)mInitializer).ThumbUseImageSize = value;
                UpdateJoystickControlLayout();
                OnPropertyChanged("ThumbUseImageSize");
            }
        }
        [Category("滑块")]
        [EngineNS.Editor.UIEditor_BindingPropertyAttribute]
        [EngineNS.Editor.MacrossMember(EngineNS.Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public Style.ButtonStyle ThumbStyle
        {
            get => ((JoysticksInitializer)mInitializer).ThumbStyle;
            set
            {
                ((JoysticksInitializer)mInitializer).ThumbStyle = value;
                OnPropertyChanged("ThumbStyle");
            }
        }
        [Category("滑块")]
        [EngineNS.Editor.UIEditor_BindingPropertyAttribute]
        [EngineNS.Editor.MacrossMember(EngineNS.Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public float ThumbMoveRadius
        {
            get => ((JoysticksInitializer)mInitializer).ThumbMoveRadius;
            set
            {
                if (value <= 0)
                    return;
                ((JoysticksInitializer)mInitializer).ThumbMoveRadius = value;
                OnPropertyChanged("ThumbMoveRadius");
            }
        }
        [EngineNS.Editor.UIEditor_BindingPropertyAttribute]
        [EngineNS.Editor.MacrossMember(EngineNS.Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public JoysticksInitializer.enPosType PosType
        {
            get => ((JoysticksInitializer)mInitializer).PosType;
            set
            {
                ((JoysticksInitializer)mInitializer).PosType = value;
                if(mJoystickControl != null)
                    mJoystickControl.CenterOffset = PointF.Empty;
                OnPropertyChanged("PosType");
            }
        }

        [EngineNS.Editor.UIEditor_BindingPropertyAttribute]
        [EngineNS.Editor.MacrossMember(EngineNS.Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public float XValue
        {
            get
            {
                if(mJoystickControl != null)
                    return mJoystickControl.XValue;
                return 0.0f;
            }
            set
            {
                if(mJoystickControl != null)
                    mJoystickControl.XValue = value;
                UpdateJoystickControlLayout();
                OnPropertyChanged("XValue");
            }
        }
        [EngineNS.Editor.UIEditor_BindingPropertyAttribute]
        [EngineNS.Editor.MacrossMember(EngineNS.Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public float YValue
        {
            get
            {
                if(mJoystickControl != null)
                    return mJoystickControl.YValue;
                return 0.0f;
            }
            set
            {
                if(mJoystickControl != null)
                    mJoystickControl.YValue = value;
                UpdateJoystickControlLayout();
                OnPropertyChanged("YValue");
            }
        }

        Controls.Containers.BorderSlot mJoysticksControlSlot;
        JoystickControl mJoystickControl;
        public override async Task<bool> Initialize(CRenderContext rc, UIElementInitializer init)
        {
            if (!await base.Initialize(rc, init))
                return false;

            mJoystickControl = new JoystickControl();
            mJoystickControl.HostJoystick = this;
            await mJoystickControl.Initialize(rc, init);

            var joystickInit = init as JoysticksInitializer;
            mJoystickControl.BackgroundBrush = joystickInit.BackgroundBrush;
            await mJoystickControl.BackgroundBrush.Initialize(rc, this);
            await joystickInit.ThumbStyle.Initialize(rc, this);
            mJoystickControl.ThumbBrush = joystickInit.ThumbStyle.NormalBrush;

            mJoysticksControlSlot = new Containers.BorderSlot();
            mJoysticksControlSlot.HorizontalAlignment = BackgroundHorizontalAlignment;
            mJoysticksControlSlot.VerticalAlignment = BackgroundVerticalAlignment;
            mJoysticksControlSlot.Margin = BackgroundMargin;
            mJoysticksControlSlot.Parent = this;
            mJoysticksControlSlot.Content = mJoystickControl;
            mJoystickControl.SetParent(this);

            if(CIPlatform.Instance.PlayMode != CIPlatform.enPlayMode.Editor)
            {
                EngineNS.CEngine.Instance.TickManager.AddTickInfo(this);
            }

            return true;
        }
        public override void Cleanup()
        {
            EngineNS.CEngine.Instance.TickManager.RemoveTickInfo(this);
            base.Cleanup();
        }
        void UpdateJoystickControlLayout()
        {
            if (mJoysticksControlSlot == null)
                return;
            var init = mInitializer as JoysticksInitializer;
            var designRect = init.DesignRect;
            var designSize = designRect.Size;
            mJoystickControl.DesiredSize = mJoysticksControlSlot.Measure(ref designSize);
            mJoysticksControlSlot.Arrange(ref designRect);
        }
        public override SizeF MeasureOverride(ref SizeF availableSize)
        {
            if (mJoysticksControlSlot == null)
                return availableSize;
            var size = mJoysticksControlSlot.Measure(ref availableSize);
            mJoystickControl.DesiredSize = size;
            size.Width = Math.Max(size.Width, availableSize.Width);
            size.Height = Math.Max(size.Height, availableSize.Height);
            return size;
        }
        public override void ArrangeOverride(ref RectangleF arrangeSize)
        {
            if (mJoysticksControlSlot != null)
                mJoysticksControlSlot.Arrange(ref arrangeSize);
            base.ArrangeOverride(ref arrangeSize);
        }

        protected override bool IsRenderable()
        {
            if (mJoystickControl == null)
                return false;
            return base.IsRenderable();
        }
        public override bool Commit(CCommandList cmd, ref Matrix parentTransformMatrix, float dpiScale)
        {
            if (IsRenderable() == false)
                return false;
            return mJoystickControl.Commit(cmd, ref parentTransformMatrix, dpiScale);
        }
        public override bool Draw(CRenderContext rc, CCommandList cmd, CGfxScreenView view)
        {
            if (IsRenderable() == false)
                return false;
            return mJoystickControl.Draw(rc, cmd, view);
        }
        void UpdateCurrentBrush()
        {
            if (mJoystickControl == null)
                return;
            var init = mInitializer as JoysticksInitializer;
            if(!init.Enable)
            {
                mJoystickControl.ThumbBrush = init.ThumbStyle.DisabledBrush;
                return;
            }
            if(mHasMouseDown || mHasTouchDown)
            {
                mJoystickControl.ThumbBrush = init.ThumbStyle.PressedBrush;
                return;
            }
            if(mIsHover)
            {
                mJoystickControl.ThumbBrush = init.ThumbStyle.HoveredBrush;
                return;
            }
            mJoystickControl.ThumbBrush = init.ThumbStyle.NormalBrush;
        }
        bool mIsHover = false;
        public override void ProcessMouseEnter(RoutedEventArgs eventArgs)
        {
            mIsHover = true;
            if (mJoystickControl != null)
                mJoystickControl.ProcessMouseEnter(eventArgs);
        }
        public override void ProcessTouchEnter(RoutedEventArgs eventArgs)
        {
            mIsHover = true;
            if (mJoystickControl != null)
                mJoystickControl.ProcessTouchEnter(eventArgs);
        }
        public override void ProcessMouseLeave(RoutedEventArgs eventArgs)
        {
            mIsHover = false;
            if (mJoystickControl != null)
                mJoystickControl.ProcessMouseLeave(eventArgs);
        }
        public override void ProcessTouchLeave(RoutedEventArgs eventArgs)
        {
            mIsHover = false;
            if(mJoystickControl != null)
                mJoystickControl.ProcessTouchLeave(eventArgs);
        }

        bool mHasMouseDown = false;
        bool mHasTouchDown = false;
        Vector2 mDownPos;
        void ProcessDown(int fingerIdx, Vector2 mousePos, UIHost uiHost)
        {
            mHasMouseDown = true;
            mDownPos = mousePos;
            uiHost.CaptureTouch(fingerIdx, this);
            UpdateCurrentBrush();

            switch(PosType)
            {
                case JoysticksInitializer.enPosType.Manual:
                    var pos = mJoystickControl.RootUIHost.GetPointWith2DSpacePoint(ref mDownPos);
                    mJoystickControl.CenterOffset.X = pos.X - mJoystickControl.BackgroundRect.X + mJoystickControl.CenterOffset.X - mJoystickControl.BackgroundRect.Width * 0.5f;
                    mJoystickControl.CenterOffset.Y = pos.Y - mJoystickControl.BackgroundRect.Y + mJoystickControl.CenterOffset.Y - mJoystickControl.BackgroundRect.Height * 0.5f;
                    break;
            }
        }
        void ProcessUp(int fingerIdx, UIHost uiHost)
        {
            mHasMouseDown = false;
            uiHost.CaptureTouch(fingerIdx, null);
            UpdateCurrentBrush();
            XValue = 0;
            YValue = 0;
        }
        void ProcessMove(Vector2 mousePos)
        {
            var curPos = new Vector2(mousePos.X, mousePos.Y);
            var temp = curPos - mDownPos;
            var len = temp.Length();
            if (len > ThumbMoveRadius)
            {
                temp.Normalize();
                XValue = temp.X;
                YValue = temp.Y;
            }
            else
            {
                XValue = temp.X / ThumbMoveRadius;
                YValue = temp.Y / ThumbMoveRadius;
            }
        }
        public override void ProcessMessage(RoutedEventArgs eventArgs)
        {
            switch(eventArgs.DeviceType)
            {
                case Input.Device.DeviceType.Mouse:
                    {
                        var mouseArg = (Input.Device.Mouse.MouseEventArgs)eventArgs.DeviceEventArgs;
                        switch(mouseArg.State)
                        {
                            case Input.Device.Mouse.ButtonState.Down:
                                ProcessDown(Input.InputServer.MaxMultiTouchNumber, new Vector2(mouseArg.X,  mouseArg.Y), eventArgs.UIHost);
                                break;
                            case Input.Device.Mouse.ButtonState.Up:
                                ProcessUp(Input.InputServer.MaxMultiTouchNumber, eventArgs.UIHost);
                                break;
                            case Input.Device.Mouse.ButtonState.Move:
                                if(mouseArg.Button == Input.Device.Mouse.MouseButtons.Left && mHasMouseDown)
                                {
                                    ProcessMove(new Vector2(mouseArg.X, mouseArg.Y));
                                }
                                break;
                        }
                    }
                    break;
                case Input.Device.DeviceType.Touch:
                    {
                        var touchArg = (Input.Device.TouchDevice.TouchEventArgs)eventArgs.DeviceEventArgs;
                        switch(touchArg.State)
                        {
                            case Input.Device.TouchDevice.enTouchState.Down:
                                ProcessDown(touchArg.FingerIdx, new Vector2(touchArg.PosX, touchArg.PosY), eventArgs.UIHost);
                                break;
                            case Input.Device.TouchDevice.enTouchState.Up:
                                ProcessUp(touchArg.FingerIdx, eventArgs.UIHost);
                                break;
                            case Input.Device.TouchDevice.enTouchState.Move:
                                if(mHasMouseDown)
                                {
                                    ProcessMove(new Vector2(touchArg.PosX, touchArg.PosY));
                                }
                                break;
                        }
                    }
                    break;
            }
            base.ProcessMessage(eventArgs);
        }

        #region Msg

        public delegate void Delegate_OnValueChanged(float x, float y);
        [Editor_UIEvent("摇杆拖动时调用方法")]
        public Delegate_OnValueChanged OnValueChanged;
        internal void OnValueChangedCallback(float x, float y)
        {
            OnValueChanged?.Invoke(x, y);
        }

        public delegate void Delegate_ProcessValue(float x, float y, float deltaX, float deltaY);
        [Editor_UIEvent("每帧调用，x,y为Value值，deltaX,deltaY为与上一帧比较的变化量")]
        public Delegate_ProcessValue OnProcessValue;

        #endregion

        #region Tick

        public static Profiler.TimeScope JoysticksScopeTickLogic = Profiler.TimeScopeManager.GetTimeScope(typeof(Joysticks), nameof(TickLogic));
        public override TimeScope GetLogicTimeScope()
        {
            return JoysticksScopeTickLogic;
        }

        float mOldXValue = 0;
        float mOldYValue = 0;
        public override void TickLogic()
        {
            var xVal = XValue;
            var yVal = YValue;
            if (CIPlatform.Instance.PlayMode != CIPlatform.enPlayMode.Editor)
            {
                OnProcessValue?.Invoke(xVal, yVal, xVal - mOldXValue, yVal - mOldYValue);
            }
            mOldXValue = xVal;
            mOldYValue = yVal;
        }

        #endregion
    }
}
