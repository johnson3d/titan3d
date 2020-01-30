using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Input.Device
{
    public class Mouse : InputDevice
    {
        #region 数据结构定义
        public enum ButtonState
        {
            Down = 0,
            Up,
            Move,
            WheelScroll,
        }
        public enum MouseVirtualButtons
        {
            None = 0,
            Left = 1 << 20,
            Right = 1 << 21,
            Middle = 1 << 22,
            XButton1 = 1 << 23,
            XButton2 = 1 << 24,
            Wheel = 1 << 25,
            WheelScroll_Up = 1 << 26,
            WheelScroll_Down = 1 << 27,
            XAxis = 1 << 28,
            YAxis = 1 << 29,
            Move = 1 << 30,
        }
        public enum MouseButtons
        {
            None = 0,
            Left = 1 << 20,
            Right = 1 << 21,
            Middle = 1 << 22,
            XButton1 = 1 << 23,
            XButton2 = 1 << 24,
            Wheel = 1 << 25,
        }
        public struct MouseEventArgs : IDeviceEventArgs
        {
            [EngineNS.Editor.MacrossMember(EngineNS.Editor.MacrossMemberAttribute.enMacrossType.Callable | EngineNS.Editor.MacrossMemberAttribute.enMacrossType.ReadOnly)]
            public MouseButtons Button;
            [EngineNS.Editor.MacrossMember(EngineNS.Editor.MacrossMemberAttribute.enMacrossType.Callable | EngineNS.Editor.MacrossMemberAttribute.enMacrossType.ReadOnly)]
            public ButtonState State;
            [EngineNS.Editor.MacrossMember(EngineNS.Editor.MacrossMemberAttribute.enMacrossType.Callable | EngineNS.Editor.MacrossMemberAttribute.enMacrossType.ReadOnly)]
            public int Clicks;
            [EngineNS.Editor.MacrossMember(EngineNS.Editor.MacrossMemberAttribute.enMacrossType.Callable | EngineNS.Editor.MacrossMemberAttribute.enMacrossType.ReadOnly)]
            public int X;
            [EngineNS.Editor.MacrossMember(EngineNS.Editor.MacrossMemberAttribute.enMacrossType.Callable | EngineNS.Editor.MacrossMemberAttribute.enMacrossType.ReadOnly)]
            public int Y;
            [EngineNS.Editor.MacrossMember(EngineNS.Editor.MacrossMemberAttribute.enMacrossType.Callable | EngineNS.Editor.MacrossMemberAttribute.enMacrossType.ReadOnly)]
            public int Delta;
            public int OffsetX;
            public int OffsetY;
            [Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.ReadOnly)]
            public float PointX => X;
            [Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.ReadOnly)]
            public float PointY => Y;
            [Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.ReadOnly)]
            public float DeltaX => OffsetX;
            [Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.ReadOnly)]
            public float DeltaY => OffsetY;
        }
        public class MouseInputEventArgs : DeviceInputEventArgs
        {
            [Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.ReadOnly)]
            public MouseEventArgs MouseEvent;
        }
        #endregion
        public bool MouseAsTouch { get; set; } = false;
        //public event EventHandler<MouseEventArgs> OnMouseDown;
        //public event EventHandler<MouseEventArgs> OnMouseUp;
        //public event EventHandler<MouseEventArgs> OnMouseMove;
        //public event EventHandler<MouseEventArgs> OnMouseDrag;
        //public event EventHandler<MouseEventArgs> OnMouseHover;
        //public event EventHandler<MouseEventArgs> OnMouseWheel;
        #region InputDevice
        Vector2 mCurrentPoint = Vector2.Zero;
        Vector2 mLastPoint = Vector2.Zero;
        bool firstMouseMove = true;
        float mSensitivity = 0.05f;
        public override void OnInputEvent(DeviceInputEventArgs e)
        {
            if (e.DeviceType != Type)
                return;
            var inputEvent = e as MouseInputEventArgs;
            var mouseEvent = inputEvent.MouseEvent;
            mCurrentPoint.X = mouseEvent.X;
            mCurrentPoint.Y = mouseEvent.Y;
            if (firstMouseMove)
            {
                firstMouseMove = false;
                mLastPoint = mCurrentPoint;
            }
            switch (mouseEvent.State)
            {
                case ButtonState.Down:
                    {
                        mThePressKeys.Add((MouseVirtualButtons)mouseEvent.Button);
                        TriggerActionMapping((int)mouseEvent.Button, ButtonState.Down);
                    }
                    break;
                case ButtonState.Up:
                    {
                        mThePressKeys.Remove((MouseVirtualButtons)mouseEvent.Button);
                        TriggerActionMapping((int)mouseEvent.Button, ButtonState.Up);
                        PulseEndAxisMapping((int)mouseEvent.Button);
                    }
                    break;
                case ButtonState.Move:
                    {
                        zeroTimes = 0;
                        if ((mCurrentPoint - mLastPoint).Length() > MathHelper.Epsilon)
                        {
                            var delta = mCurrentPoint - mLastPoint;
                            TriggerActionMoveMapping((int)MouseVirtualButtons.Move, ButtonState.Move, mCurrentPoint.X, mCurrentPoint.Y, delta.X, delta.Y);
                        }
                    }
                    break;
                case ButtonState.WheelScroll:
                    {
                        if (mouseEvent.Delta > 0)
                            TriggerActionMapping((int)MouseVirtualButtons.WheelScroll_Up, ButtonState.WheelScroll);
                        else
                            TriggerActionMapping((int)MouseVirtualButtons.WheelScroll_Down, ButtonState.WheelScroll);
                    }
                    break;
            }
            //mLastPoint = mCurrentPoint;
        }
        public void TriggerActionMoveMapping(int keyCode, ButtonState state, float x, float y, float deltaX, float deltaY)
        {
            for (int i = 0; i < InputBindings.Count; ++i)
            {
                var binding = InputBindings[i];
                for (int j = 0; j < binding.Mappings.Count; ++j)
                {
                    var mapping = binding.Mappings[j];
                    if (mapping is InputMoveMapping)
                    {
                        var valueMapping = mapping as InputMoveMapping;
                        if (mapping.KeyCode == keyCode && mapping.MappingType == InputMappingType.Action && state == (ButtonState)mapping.Value)
                        {
                            for (int k = 0; k < valueMapping.Funtions.Count; ++k)
                            {
                                valueMapping.Funtions[k]?.Invoke(x, y, deltaX, deltaY);
                            }
                        }
                    }
                }
            }
        }
        public void TriggerActionMapping(int keyCode, ButtonState state)
        {
            for (int i = 0; i < InputBindings.Count; ++i)
            {
                var binding = InputBindings[i];
                for (int j = 0; j < binding.Mappings.Count; ++j)
                {
                    var mapping = binding.Mappings[j];
                    if (mapping is InputValueMapping)
                    {
                        var valueMapping = mapping as InputValueMapping;
                        if (mapping.KeyCode == keyCode && mapping.MappingType == InputMappingType.Action && state == (ButtonState)mapping.Value)
                        {
                            for (int k = 0; k < valueMapping.Funtions.Count; ++k)
                            {
                                valueMapping.Funtions[k]?.Invoke(mapping.Value);
                            }
                        }
                    }
                }
            }
        }
        public void PulseAxisMapping(int keyCode)
        {
            for (int i = 0; i < InputBindings.Count; ++i)
            {
                var binding = InputBindings[i];
                for (int j = 0; j < binding.Mappings.Count; ++j)
                {
                    var mapping = binding.Mappings[j];
                    if (mapping is InputValueMapping)
                    {
                        var valueMapping = mapping as InputValueMapping;
                        if (mapping.KeyCode == keyCode && mapping.MappingType == InputMappingType.Axis)
                        {
                            for (int k = 0; k < valueMapping.Funtions.Count; ++k)
                            {
                                valueMapping.Funtions[k]?.Invoke(mapping.Value);
                            }

                        }
                    }
                }
            }
        }
        public void PulseMouseMoveAxisMapping(int keyCode, float scale)
        {
            for (int i = 0; i < InputBindings.Count; ++i)
            {
                var binding = InputBindings[i];
                for (int j = 0; j < binding.Mappings.Count; ++j)
                {
                    var mapping = binding.Mappings[j];
                    if (mapping is InputValueMapping)
                    {
                        var valueMapping = mapping as InputValueMapping;
                        if (mapping.KeyCode == keyCode && mapping.MappingType == InputMappingType.Axis)
                        {
                            for (int k = 0; k < valueMapping.Funtions.Count; ++k)
                            {
                                valueMapping.Funtions[k]?.Invoke(mapping.Value * scale);
                            }

                        }
                    }
                }
            }
        }
        void PulseEndAxisMapping(int keyCode)
        {
            for (int i = 0; i < InputBindings.Count; ++i)
            {
                var binding = InputBindings[i];
                if (binding.MappingType == InputMappingType.Action)
                    continue;
                for (int j = 0; j < binding.Mappings.Count; ++j)
                {
                    var mapping = binding.Mappings[j];
                    if (mapping is InputValueMapping)
                    {
                        var valueMapping = mapping as InputValueMapping;
                        if (mapping.KeyCode == keyCode)
                        {
                            for (int k = 0; k < valueMapping.Funtions.Count; ++k)
                            {
                                valueMapping.Funtions[k]?.Invoke(0);
                            }

                        }
                    }
                }
            }
        }
        List<MouseVirtualButtons> mThePressKeys = new List<MouseVirtualButtons>();
        List<MouseVirtualButtons> mTheReleaseKeys = new List<MouseVirtualButtons>();
        int zeroTimes = 0;
        public override void Tick()
        {
            for (int i = 0; i < mThePressKeys.Count; ++i)
            {
                PulseAxisMapping((int)mThePressKeys[i]);
            }
            if ((mCurrentPoint - mLastPoint).Length() < MathHelper.Epsilon)
            {
                zeroTimes++;
            }
            if (zeroTimes < 20)
            {
                //if (MathHelper.Abs(mCurrentPoint.X - mLastPoint.X)> MathHelper.Epsilon)
                {
                    PulseMouseMoveAxisMapping((int)MouseVirtualButtons.XAxis, (mCurrentPoint.X - mLastPoint.X) * mSensitivity);
                }
                //if (MathHelper.Abs(mCurrentPoint.Y - mLastPoint.Y) > MathHelper.Epsilon)
                {
                    PulseMouseMoveAxisMapping((int)MouseVirtualButtons.YAxis, (mCurrentPoint.Y - mLastPoint.Y) * mSensitivity);
                }
            }
            mLastPoint = mCurrentPoint;
        }

        #endregion
        public Mouse()
        {
            mName = "Mouse";
            mType = DeviceType.Mouse;
            mID = Guid.NewGuid();
        }
    }
}
