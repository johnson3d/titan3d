using EngineNS.IO;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Text;

namespace EngineNS.UI.Controls
{
    public partial class TtUIElement
    {
        public Macross.UMacrossGetter<TtUIMacrossBase> MacrossGetter;

        public string GetEventMethodName(string eventName)
        {
            return "On_" + eventName + "_" + Id;
        }

        public class MacrossMethodData : BaseSerializer
        {
            [Rtti.Meta]
            public string EventName { get; set; }
            string mDisplayName;
            public string DisplayName
            {
                get
                {
                    if(DisplayNameDirty)
                    {
                        mDisplayName = $"On {EventName}({HostElement.Name})";
                        DisplayNameDirty = false;
                    }
                    return mDisplayName;
                }
            }
            public Bricks.CodeBuilder.UMethodDeclaration Desc;
            public TtUIElement HostElement;
            public bool DisplayNameDirty = true;

            public override void OnPreRead(object tagObject, object hostObject, bool fromXml)
            {
                HostElement = hostObject as TtUIElement;
            }
        }

        internal Dictionary<Bricks.CodeBuilder.UMethodDeclaration, MacrossMethodData> mEventMethodDisplayNames = new Dictionary<Bricks.CodeBuilder.UMethodDeclaration, MacrossMethodData>();
        public string GetEventMethodDisplayName(Bricks.CodeBuilder.UMethodDeclaration desc)
        {
            MacrossMethodData val = null;
            if(mEventMethodDisplayNames.TryGetValue(desc, out val))
            {
                return val.DisplayName;
            }
            return desc.MethodName;
        }
        void SetMethodDisplayNamesDirty()
        {
            foreach(var name in mEventMethodDisplayNames)
            {
                name.Value.DisplayNameDirty = true;
            }
        }

        Dictionary<string, MacrossMethodData> mMacrossEvents = new Dictionary<string, MacrossMethodData>();
        [Rtti.Meta]
        [Browsable(false)]
        public Dictionary<string, MacrossMethodData> MacrossEvents
        {
            get => mMacrossEvents;
            set
            {
                mMacrossEvents = value;
            }
        }
        public void SetEventBindMethod(string name, Bricks.CodeBuilder.UMethodDeclaration desc)
        {
            var data = new TtUIElement.MacrossMethodData()
            {
                EventName = name,
                Desc = desc,
                HostElement = this,
            };
            MacrossEvents[name] = data;
            mEventMethodDisplayNames[desc] = data;
        }
        public bool HasEventMethod(string name, out string methodDisplayName)
        {
            MacrossMethodData data;
            if (MacrossEvents.TryGetValue(name, out data))
            {
                methodDisplayName = data.DisplayName;
                return true;
            }
            methodDisplayName = "";
            return false;
        }
    }
}
