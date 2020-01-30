using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Input.Device
{
    [EngineNS.Editor.Editor_MacrossClass(ECSType.Client, EngineNS.Editor.Editor_MacrossClassAttribute.enMacrossType.AllFeatures)]
    public interface IDeviceEventArgs
    {
        [EngineNS.Editor.MacrossMember(EngineNS.Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        float PointX
        {
            get;
        }
        [EngineNS.Editor.MacrossMember(EngineNS.Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        float PointY
        {
            get;
        }
        [EngineNS.Editor.MacrossMember(EngineNS.Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        float DeltaX
        {
            get;
        }
        [EngineNS.Editor.MacrossMember(EngineNS.Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        float DeltaY
        {
            get;
        }
    }
    public enum DeviceType
    {
        None = 0,
        Keyboard,
        Mouse,
        //Android,    // 废弃
        //IOS,        // 废弃
        GamePad,
        Touch,
    }
    public enum InputMappingType
    {
        Action,
        Axis,
    }
    public class DeviceInputEventArgs
    {
        public DeviceType DeviceType;
    }
    [Rtti.MetaClass]
    public class InputMapping : EngineNS.IO.Serializer.Serializer
    {
        [Rtti.MetaData]
        public InputMappingType MappingType
        {
            get;
            set;
        }
        [Rtti.MetaData]
        public DeviceType DeviceType
        {
            get;
            set;
        }
        [Rtti.MetaData]
        public int KeyCode
        {
            get;
            set;
        }
        [Rtti.MetaData]
        public float Value
        {
            get;
            set;
        }
        public InputMapping()
        {
        }
        public InputMapping(DeviceType deviceType, InputMappingType mappingType, int keyCode, float scale)
        {
            DeviceType = deviceType;
            MappingType = mappingType;
            KeyCode = keyCode;
            Value = scale;
        }
    }
    [Rtti.MetaClass]
    public class InputValueMapping : InputMapping
    {
        public List<InputActionvalueFuntion> Funtions
        {
            get;
            set;
        } = new List<InputActionvalueFuntion>();
        public InputValueMapping(DeviceType deviceType, InputMappingType mappingType, int keyCode, float scale) : base(deviceType, mappingType, keyCode, scale)
        {

        }
    }
    [Rtti.MetaClass]
    public class InputMoveMapping : InputMapping
    {
        public List<InputActionMoveFuntion> Funtions
        {
            get;
            set;
        } = new List<InputActionMoveFuntion>();
        public InputMoveMapping()
        {
        }
        public InputMoveMapping(DeviceType deviceType, InputMappingType mappingType, int keyCode, float scale) : base(deviceType, mappingType, keyCode, scale)
        {

        }
    }
    [Rtti.MetaClass]
    public class InputBinding : EngineNS.IO.Serializer.Serializer
    {
        [Rtti.MetaData]
        public string BindingName
        {
            get;
            set;
        }
        [Rtti.MetaData]
        public InputMappingType MappingType
        {
            get;
            set;
        }
        [Rtti.MetaData]
        public List<InputMapping> Mappings
        {
            get;
            set;
        } = new List<InputMapping>();
        public InputBinding()
        {
        }
        public InputBinding(string name, InputMappingType mappingType)
        {
            BindingName = name;
            MappingType = mappingType;
        }
    }
    [Rtti.MetaClass]
    public class InputMoveBinding : InputBinding
    {
        public InputMoveBinding()
        {
        }
        public InputMoveBinding(string name, InputMappingType mappingType) : base(name, mappingType)
        {
        }
    }
    public class InputDevice
    {
        protected string mName = "InputDevice";
        public string Name { get => mName; set => mName = value; }
        protected DeviceType mType = DeviceType.None;
        public DeviceType Type { get => mType; set => mType = value; }
        protected Guid mID = Guid.Empty;
        public Guid ID { get => mID; set => mID = value; }
        public List<InputBinding> InputBindings
        {
            get;
            set;
        } = new List<InputBinding>();
        public virtual void InitDevice()
        {

        }
        public virtual void OnInputEvent(DeviceInputEventArgs e) { }
        public virtual void Tick() { }
        public void LoadConfig(InputConfiguration config)
        {
            foreach (var binding in config.InputBindings)
            {
                foreach (var mapping in binding.Mappings)
                {
                    if (mapping.DeviceType == Type)
                    {
                        AddMapping(binding.BindingName, binding.MappingType, mapping);
                    }
                }
            }
        }
        void AddMapping(string bindingName, InputMappingType type, InputMapping mapping)
        {
            var binding = InputBindings.Find((bind) =>
            {
                if (bind.BindingName == bindingName)
                    return true;
                return false;
            });
            if (binding == null)
            {
                binding = new InputBinding(bindingName, type);
                InputBindings.Add(binding);
            }
            if (!binding.Mappings.Contains(mapping))
            {
                binding.Mappings.Add(mapping);
            }
        }
        public virtual void RegisterInputMapping(string name, InputActionvalueFuntion fuction, Guid deviceID = default(Guid))
        {
            if (deviceID != Guid.Empty)
            {
                if (deviceID != ID)
                    return;
            }
            var binding = InputBindings.Find((bind) =>
            {
                if (bind.BindingName == name)
                    return true;
                return false;
            });
            if (binding != null)
            {
                foreach (var value in binding.Mappings)
                {
                    if (value is InputValueMapping)
                    {
                        var valueMapping = value as InputValueMapping;
                        if (!valueMapping.Funtions.Contains(fuction))
                            valueMapping.Funtions.Add(fuction);
                    }
                    if (value is InputValueMapping)
                    {
                        var valueMapping = value as InputValueMapping;
                        if (!valueMapping.Funtions.Contains(fuction))
                            valueMapping.Funtions.Add(fuction);
                    }
                }
            }
        }
        public void UnRegisterInputMapping(string name, InputActionvalueFuntion fuction, Guid deviceID = default(Guid))
        {
            if (deviceID != Guid.Empty)
            {
                if (deviceID != ID)
                    return;
            }
            var binding = InputBindings.Find((bind) =>
            {
                if (bind.BindingName == name)
                    return true;
                return false;
            });
            if (binding != null)
            {
                foreach (var value in binding.Mappings)
                {
                    if (value is InputValueMapping)
                    {
                        var valueMapping = value as InputValueMapping;
                        if (valueMapping.Funtions.Contains(fuction))
                            valueMapping.Funtions.Remove(fuction);
                    }
                }

            }
        }
        public virtual void RegisterInputMoveMapping(string name, InputActionMoveFuntion fuction, Guid deviceID = default(Guid))
        {
            if (deviceID != Guid.Empty)
            {
                if (deviceID != ID)
                    return;
            }
            var binding = InputBindings.Find((bind) =>
            {
                if (bind.BindingName == name)
                    return true;
                return false;
            });
            if (binding != null)
            {
                foreach (var value in binding.Mappings)
                {
                    if (value is InputMoveMapping)
                    {
                        var valueMapping = value as InputMoveMapping;
                        if (!valueMapping.Funtions.Contains(fuction))
                            valueMapping.Funtions.Add(fuction);
                    }
                }
            }
        }
        public void UnRegisterInputMoveMapping(string name, InputActionMoveFuntion fuction, Guid deviceID = default(Guid))
        {
            if (deviceID != Guid.Empty)
            {
                if (deviceID != ID)
                    return;
            }
            var binding = InputBindings.Find((bind) =>
            {
                if (bind.BindingName == name)
                    return true;
                return false;
            });
            if (binding != null)
            {
                foreach (var value in binding.Mappings)
                {
                    if (value is InputMoveMapping)
                    {
                        var valueMapping = value as InputMoveMapping;
                        if (valueMapping.Funtions.Contains(fuction))
                            valueMapping.Funtions.Remove(fuction);
                    }
                }

            }
        }
    }
}
