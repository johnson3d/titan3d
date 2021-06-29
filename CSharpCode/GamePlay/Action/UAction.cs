using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.GamePlay.Action
{
    public class UAction
    {
        public string Name
        {
            get;
            set;
        }
        public static void OnChanged(object recorder, object host, string name, object oldValue, object newValue)
        {
            var rcdObj = recorder as IActionRecordable;
            var rcd = rcdObj?.ActionRecorder;
            if (rcd == null)
                return;
            if (rcd.IsPlayRecord)
                return;
            if (rcd.CurrentAction == null)
                return;

            if (rcd.IsRecordAction(host, name, oldValue, newValue) == false)
            {
                return;
            }

            var propChanged = new UPropertyModifier();
            propChanged.HostObject = host;
            propChanged.NewValue = newValue;
            propChanged.OldValue = oldValue;
            propChanged.PropertyName = name;
            rcd.CurrentAction.ModifyStack.Add(propChanged);
            rcd.OnChanged(propChanged);
        }
        public UActionRecorder Recorder;
        public class UPropertyModifier
        {
            public object HostObject;
            public string PropertyName;
            public object OldValue;
            public object NewValue;
        }
        public List<UPropertyModifier> ModifyStack { get; } = new List<UPropertyModifier>();
        public void Undo()
        {
            foreach (var i in ModifyStack)
            {
                i.HostObject.GetType().GetProperty(i.PropertyName).SetValue(i.HostObject, i.OldValue);
            }
        }
        public void Redo()
        {
            foreach (var i in ModifyStack)
            {
                i.HostObject.GetType().GetProperty(i.PropertyName).SetValue(i.HostObject, i.NewValue);
            }
        }
    }
}
