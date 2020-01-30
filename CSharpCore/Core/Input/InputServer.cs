using EngineNS.Input.Device;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Input
{

    //[Rtti.MetaClass]
    //public class InputAction : EngineNS.IO.Serializer.Serializer
    //{
    //    [Rtti.MetaData]
    //    public InputDeviceType Type
    //    {
    //        get;
    //        set;
    //    }
    //    [Rtti.MetaData]
    //    public uint OperationType
    //    {
    //        get;
    //        set;
    //    }

    //    [Rtti.MetaData]
    //    public uint Value
    //    {
    //        get;
    //        set;
    //    }
    //    public List<InputActionFuntion> Funtions
    //    {
    //        get;
    //        set;
    //    } = new List<InputActionFuntion>();
    //    public InputAction()
    //    {
    //    }
    //    public InputAction(InputDeviceType type, uint keyValue, uint operationType)
    //    {
    //        Type = type;
    //        Value = keyValue;
    //        OperationType = operationType;
    //    }
    //}
    //public class InputAxisAction : EngineNS.IO.Serializer.Serializer
    //{
    //    [Rtti.MetaData]
    //    public InputDeviceType Type
    //    {
    //        get;
    //        set;
    //    }
    //    [Rtti.MetaData]
    //    public uint Value
    //    {
    //        get;
    //        set;
    //    }
    //    [Rtti.MetaData]
    //    public float Scale
    //    {
    //        get;
    //        set;
    //    }
    //    public List<InputAxisFuntion> Funtions
    //    {
    //        get;
    //        set;
    //    } = new List<InputAxisFuntion>();
    //    public InputAxisAction()
    //    {
    //    }
    //    public InputAxisAction(InputDeviceType type, uint keyValue, float scale)
    //    {
    //        Type = type;
    //        Value = keyValue;
    //        Scale = scale;
    //    }
    //}
    //[Rtti.MetaClass]
    //public class InputActionMapping : EngineNS.IO.Serializer.Serializer
    //{
    //    [Rtti.MetaData]
    //    public string BindingName
    //    {
    //        get;
    //        set;
    //    }
    //    [Rtti.MetaData]
    //    public List<InputAction> Actions
    //    {
    //        get;
    //        set;
    //    } = new List<InputAction>();
    //    public InputActionMapping()
    //    {
    //    }
    //    public InputActionMapping(string name)
    //    {
    //        BindingName = name;
    //    }
    //}
    //[Rtti.MetaClass]
    //public class InputAxisMapping : EngineNS.IO.Serializer.Serializer
    //{
    //    [Rtti.MetaData]
    //    public string BindingName
    //    {
    //        get;
    //        set;
    //    }
    //    [Rtti.MetaData]
    //    public List<InputAxisAction> Actions
    //    {
    //        get;
    //        set;
    //    } = new List<InputAxisAction>();
    //    public InputAxisMapping()
    //    {
    //    }
    //    public InputAxisMapping(string name)
    //    {
    //        BindingName = name;
    //    }
    //}
    [Rtti.MetaClass]
    public class InputConfiguration : EngineNS.IO.Serializer.Serializer
    {
        [Rtti.MetaData]
        public List<InputBinding> InputBindings
        {
            get;
            set;
        } = new List<InputBinding>();
    }
    public delegate void InputActionvalueFuntion(float scale);
    public delegate void InputActionMoveFuntion(float x, float y, float deltaX, float deltaY);
    //public delegate void InputAxisFuntion(float scale);

    public partial class InputServer
    {
        public static readonly int MaxMultiTouchNumber = 10;    // 最多支持的触控点数量

        Dictionary<Guid, InputDevice> mDevices = new Dictionary<Guid, InputDevice>();
        public List<InputDevice> GetDevive(DeviceType inputDeviceType)
        {
            List<InputDevice> temp = new List<InputDevice>();
            InputDevice[] tempArray = new InputDevice[mDevices.Count];
            mDevices.Values.CopyTo(tempArray, 0);
            temp.AddRange(tempArray);
            return temp.FindAll((device) =>
             {
                 return device.Type == inputDeviceType;
             });
        }
        partial void UIInputProcess(EngineNS.Input.Device.DeviceInputEventArgs e);
        public void OnInputEvnet( EngineNS.Input.Device.DeviceInputEventArgs e)
        {
            // 抛送到逻辑线程
            EngineNS.CEngine.Instance.EventPoster.RunOn(() =>
            {
                using (var it = mDevices.GetEnumerator())
                {
                    while (it.MoveNext())
                    {
                        it.Current.Value.OnInputEvent(e);
                    }
                }

                UIInputProcess(e);

                return true;
            }, Thread.Async.EAsyncTarget.Logic);
        }
        //public event EventHandler<ActionEvent> OnTouchDown;
        //public event EventHandler<ActionEvent> OnTouchUp;
        //public event EventHandler<ActionEvent> OnTouchMove;
        //public event EventHandler<ActionEvent> OnTouchDrag;
        //public event EventHandler<ActionEvent> OnTouchHover;
        //public event EventHandler<ActionEvent> OnMouseWheel;
        //public event EventHandler<KeyEvent> OnKeyDown;
        //public event EventHandler<KeyEvent> OnKeyUp;
        //Vector2 mPrevPos;
        //bool isFirstIn = true;
        //Vector2 deltaVector = Vector2.Zero;
        //float mSensitivity = 0.05f;
        //public void OnActionEvent(ref Input.ActionEvent e)
        //{
        //    if (isFirstIn)
        //    {
        //        mPrevPos.X = e.X;
        //        mPrevPos.Y = e.Y;
        //        e.XDelta = 0;
        //        e.YDelta = 0;
        //        isFirstIn = false;
        //    }
        //    deltaVector.X = e.X - mPrevPos.X;
        //    deltaVector.Y = e.Y - mPrevPos.Y;
        //    if (deltaVector.X == 1)
        //        deltaVector.X = 0;
        //    deltaVector.Normalize();

        //    e.XDelta = deltaVector.X * mSensitivity;
        //    e.YDelta = deltaVector.Y * mSensitivity;
        //    mPrevPos.X = e.X;
        //    mPrevPos.Y = e.Y;
        //    switch (e.MotionType)
        //    {
        //        case EMotionType.Down:
        //            {
        //                if (ContainsDeviceButton((uint)e.Flags) == null)
        //                    mDeviceButtonsDown.Add(new DeveceButton((uint)e.Flags, 1));
        //                OnTouchDown?.Invoke(this, e);
        //            }
        //            break;
        //        case EMotionType.Up:
        //            {
        //                RemoveDeviceButton((uint)e.Flags);
        //                OnTouchUp?.Invoke(this, e);
        //            }
        //            break;
        //        case EMotionType.Move:
        //            {
        //                if (ContainsDeviceButton((uint)MouseEventArgs.MouseButtons.XAxis) == null)
        //                    mDeviceButtonsDown.Add(new DeveceButton((uint)MouseEventArgs.MouseButtons.XAxis, e.XDelta));
        //                if (ContainsDeviceButton((uint)MouseEventArgs.MouseButtons.YAxis) == null)
        //                    mDeviceButtonsDown.Add(new DeveceButton((uint)MouseEventArgs.MouseButtons.YAxis, e.YDelta));
        //                OnTouchMove?.Invoke(this, e);
        //            }
        //            break;
        //        case EMotionType.Drag:
        //            {
        //                if (ContainsDeviceButton((uint)MouseEventArgs.MouseButtons.XAxis) == null)
        //                    mDeviceButtonsDown.Add(new DeveceButton((uint)MouseEventArgs.MouseButtons.XAxis, e.XDelta));
        //                if (ContainsDeviceButton((uint)MouseEventArgs.MouseButtons.YAxis) == null)
        //                    mDeviceButtonsDown.Add(new DeveceButton((uint)MouseEventArgs.MouseButtons.YAxis, e.YDelta));
        //                OnTouchDrag?.Invoke(this, e);
        //            }
        //            break;
        //        case EMotionType.Hover:
        //            OnTouchHover?.Invoke(this, e);
        //            break;
        //        case EMotionType.MouseWheel:
        //            OnMouseWheel?.Invoke(this, e);
        //            break;
        //    }
        //    if (e.XDelta == 0)
        //    {
        //        e.Flags = e.Flags & ~(uint)EngineNS.Input.MouseEventArgs.MouseButtons.XAxis;
        //        RemoveDeviceButton((uint)MouseEventArgs.MouseButtons.XAxis);
        //    }
        //    if (e.YDelta == 0)
        //    {
        //        e.Flags = e.Flags & ~(uint)EngineNS.Input.MouseEventArgs.MouseButtons.YAxis;
        //        RemoveDeviceButton((uint)MouseEventArgs.MouseButtons.YAxis);
        //    }
        //    FireMapping(e);
        //}
            
        InputConfiguration mInputConfiguration = null;
        public InputConfiguration InputConfiguration
        {
            get => mInputConfiguration;
            set => mInputConfiguration = value;
        }
        //Dictionary<int, InputActionFuntion> mActionMappingPair = new Dictionary<int, InputActionFuntion>();
        //Dictionary<int, InputAxisFuntion> mAxisMappingPair = new Dictionary<int, InputAxisFuntion>();

 
        public void RegisterInputMapping(string name, InputActionvalueFuntion fuction, Guid deviceID = default(Guid))
        {
           foreach(var device in mDevices)
            {
                device.Value.RegisterInputMapping(name, fuction, deviceID);
            }
        }
        public void UnRegisterInputMapping(string name, InputActionvalueFuntion fuction, Guid deviceID = default(Guid))
        {
            foreach (var device in mDevices)
            {
                device.Value.UnRegisterInputMapping(name, fuction, deviceID);
            }
        }
        public void RegisterInputMoveMapping(string name, InputActionMoveFuntion fuction, Guid deviceID = default(Guid))
        {
            foreach (var device in mDevices)
            {
                device.Value.RegisterInputMoveMapping(name, fuction, deviceID);
            }
        }
        public void UnRegisterInputMoveMapping(string name, InputActionMoveFuntion fuction, Guid deviceID = default(Guid))
        {
            foreach (var device in mDevices)
            {
                device.Value.UnRegisterInputMoveMapping(name, fuction, deviceID);
            }
        }
        //枚举输入设备
        public void EnumerateDevices()
        {
            //在这里检测新加入的设备，然后重新配置设备 
            var keyboard = new Keyboard();
            keyboard.LoadConfig(mInputConfiguration);
            mDevices.Add(keyboard.ID, keyboard);
            var mouse = new Mouse();
            mouse.MouseAsTouch = true;
            mouse.LoadConfig(mInputConfiguration);
            mDevices.Add(mouse.ID, mouse);
        }
        public void CreateDefaultConfig()
        {
            mInputConfiguration = new InputConfiguration();
            var horizonAxis = new InputBinding("HorizontalAxis", InputMappingType.Axis);
            var positiveAction = new InputValueMapping(DeviceType.Keyboard, InputMappingType.Axis, (int)EngineNS.Input.Device.Keyboard.Keys.D, 5);
            var negativeAction = new InputValueMapping(DeviceType.Keyboard, InputMappingType.Axis, (int)EngineNS.Input.Device.Keyboard.Keys.A, -5);
            horizonAxis.Mappings.Add(positiveAction);
            horizonAxis.Mappings.Add(negativeAction);
            mInputConfiguration.InputBindings.Add(horizonAxis);
            var verticalAxis = new InputBinding("VerticalAxis", InputMappingType.Axis);
            var positiveVerticalAction = new InputValueMapping(DeviceType.Keyboard, InputMappingType.Axis, (int)EngineNS.Input.Device.Keyboard.Keys.W, 5);
            var negativeVerticalAction = new InputValueMapping(DeviceType.Keyboard, InputMappingType.Axis, (int)EngineNS.Input.Device.Keyboard.Keys.S, -5);
            verticalAxis.Mappings.Add(positiveVerticalAction);
            verticalAxis.Mappings.Add(negativeVerticalAction);
            mInputConfiguration.InputBindings.Add(verticalAxis);

            var turnAction = new InputBinding("Turn", InputMappingType.Axis);
            var hor = new InputValueMapping(DeviceType.Mouse, InputMappingType.Axis,(int)Mouse.MouseVirtualButtons.XAxis, 0.5f);
            turnAction.Mappings.Add(hor);
            mInputConfiguration.InputBindings.Add(turnAction);

            var LookUpAction = new InputBinding("LookUp", InputMappingType.Axis);
            var ver = new InputValueMapping(DeviceType.Mouse, InputMappingType.Axis,(int)Mouse.MouseVirtualButtons.YAxis, 0.5f);
            LookUpAction.Mappings.Add(ver);
            mInputConfiguration.InputBindings.Add(LookUpAction);

            var TouchMoveAction = new InputMoveBinding("TouchMove", InputMappingType.Action);
            var tma = new InputMoveMapping(DeviceType.Mouse, InputMappingType.Action, (int)Mouse.MouseVirtualButtons.Move,(int)Mouse.ButtonState.Move);
            TouchMoveAction.Mappings.Add(tma);
            mInputConfiguration.InputBindings.Add(TouchMoveAction);

            CreateInputActionMapping(DeviceType.Mouse,"Touched", (int)EngineNS.Input.Device.Mouse.MouseVirtualButtons.Right);
            CreateInputActionMapping(DeviceType.Mouse, "MouseLeftButton", (int)EngineNS.Input.Device.Mouse.MouseVirtualButtons.Left);
            CreateInputActionMapping(DeviceType.Mouse, "MouseRightButton", (int)EngineNS.Input.Device.Mouse.MouseVirtualButtons.Right);
            CreateInputActionMapping(DeviceType.Keyboard, "Crouch", (int)EngineNS.Input.Device.Keyboard.Keys.C);
            CreateInputActionMapping(DeviceType.Keyboard,"Skill_NormalAttack", (int)EngineNS.Input.Device.Keyboard.Keys.F);
            CreateInputActionMapping(DeviceType.Keyboard,"Skill_HeavyAttack", (int)EngineNS.Input.Device.Keyboard.Keys.R);
            CreateInputActionMapping(DeviceType.Keyboard,"Skill_Dash", (int)EngineNS.Input.Device.Keyboard.Keys.Q);
            CreateInputActionMapping(DeviceType.Keyboard,"Skill_Roll", (int)EngineNS.Input.Device.Keyboard.Keys.Space);
            CreateInputActionMapping(DeviceType.Keyboard,"Skill_Cloak", (int)EngineNS.Input.Device.Keyboard.Keys.E);
            CreateInputActionMapping(DeviceType.Keyboard,"Rush", (int)EngineNS.Input.Device.Keyboard.Keys.LShiftKey);
        }
        void CreateInputActionMapping(DeviceType device,string name, int keyCode)
        {
            var presseddAction = new InputBinding(name + "_Pressed", InputMappingType.Action);
            var press = new InputValueMapping(device, InputMappingType.Action, keyCode, (int)Mouse.ButtonState.Down);
            presseddAction.Mappings.Add(press);
            mInputConfiguration.InputBindings.Add(presseddAction);
            var releasedAction = new InputBinding(name + "_Released", InputMappingType.Action);
            var released = new InputValueMapping(device, InputMappingType.Action, keyCode, (int)Mouse.ButtonState.Up);
            releasedAction.Mappings.Add(released);
            mInputConfiguration.InputBindings.Add(releasedAction);
        }

    
        public void Tick()
        {
            using (var it = mDevices.GetEnumerator())
            {
                while (it.MoveNext())
                {
                    it.Current.Value.Tick();
                }
            }
        }
    }
    public class InputActionSystemProcessor : CEngineAutoMemberProcessor
    {
        public override async System.Threading.Tasks.Task<object> CreateObject()
        {
            await Thread.AsyncDummyClass.DummyFunc();
            var inputSys = new InputServer();
            inputSys.CreateDefaultConfig();
            inputSys.EnumerateDevices();
            return inputSys;
        }
        public override void Tick(object obj)
        {
            var inputSys = obj as InputServer;
            if (inputSys != null)
            {
                inputSys.Tick();
            }
        }
        public override void Cleanup(object obj)
        {

        }
    }
}

namespace EngineNS
{
    public partial class CEngine
    {
        [CEngineAutoMemberAttribute(typeof(EngineNS.Input.InputActionSystemProcessor))]
        [EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]// | Editor.MacrossMemberAttribute.enMacrossType.ReadOnly)]
        public EngineNS.Input.InputServer InputServerInstance
        {
            get;
            set;
        }
    }
}
