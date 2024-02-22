using EngineNS.Bricks.CodeBuilder;
using EngineNS.IO;
using EngineNS.UI.Event;
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

        public string GetPropertyBindMethodName(string propertyName, bool isSet)
        {
            if (isSet)
                return "Set_" + propertyName + "_" + Id;
            else
                return "Get_" + propertyName + "_" + Id;
        }

        public string GetEventMethodName(string eventName)
        {
            return "On_" + eventName + "_" + Id;
        }
        public class MacrossMethodData : BaseSerializer
        {
            public TtUIElement HostElement;
            public bool DisplayNameDirty = true;

            public override void OnPreRead(object tagObject, object hostObject, bool fromXml)
            {
                HostElement = hostObject as TtUIElement;
            }

            public virtual string GetDisplayName(Bricks.CodeBuilder.UMethodDeclaration desc)
            {
                if (desc == null)
                    return "none";
                return desc.MethodName;
            }
        }

        public class MacrossEventMethodData : MacrossMethodData
        {
            public Bricks.CodeBuilder.UMethodDeclaration Desc;
            [Rtti.Meta]
            public string EventName { get; set; }
            string mDisplayName;
            public override string GetDisplayName(UMethodDeclaration desc)
            {
                if(DisplayNameDirty)
                {
                    mDisplayName = $"On {EventName}({HostElement.Name})";
                    DisplayNameDirty = false;
                }
                return mDisplayName;
            }
        }

        public class MacrossPropertyBindMethodData : MacrossMethodData
        {
            [Rtti.Meta]
            public string PropertyName { get; set; }

            public Bricks.CodeBuilder.UMethodDeclaration GetDesc;
            public Bricks.CodeBuilder.UMethodDeclaration SetDesc;
            string mGetMethodDisplayName;
            string mSetMethodDisplayName;
            public override string GetDisplayName(UMethodDeclaration desc)
            {
                if (DisplayNameDirty)
                {
                    mSetMethodDisplayName = $"Set {PropertyName}({HostElement.Name})";
                    mGetMethodDisplayName = $"Get {PropertyName}({HostElement.Name})";
                    DisplayNameDirty = false;
                }
                if (desc == GetDesc)
                    return mGetMethodDisplayName;
                else if(desc == SetDesc)
                    return mSetMethodDisplayName;
                return "none";
            }
        }

        internal Dictionary<Bricks.CodeBuilder.UMethodDeclaration, MacrossMethodData> mMethodDisplayNames = new Dictionary<Bricks.CodeBuilder.UMethodDeclaration, MacrossMethodData>();
        public string GetMethodDisplayName(Bricks.CodeBuilder.UMethodDeclaration desc)
        {
            MacrossMethodData val = null;
            if(mMethodDisplayNames.TryGetValue(desc, out val))
            {
                return val.GetDisplayName(desc);
            }
            return desc.MethodName;
        }
        void SetMethodDisplayNamesDirty()
        {
            foreach(var name in mMethodDisplayNames)
            {
                name.Value.DisplayNameDirty = true;
            }
        }

        Dictionary<string, MacrossMethodData> mMacrossMethods = new Dictionary<string, MacrossMethodData>();
        [Rtti.Meta]
        [Browsable(false)]
        public Dictionary<string, MacrossMethodData> MacrossMethods
        {
            get => mMacrossMethods;
            set
            {
                mMacrossMethods = value;
            }
        }
        public void SetEventBindMethod(string eventName, Bricks.CodeBuilder.UMethodDeclaration desc)
        {
            var data = new TtUIElement.MacrossEventMethodData()
            {
                EventName = eventName,
                Desc = desc,
                HostElement = this,
            };
            MacrossMethods[eventName] = data;
            mMethodDisplayNames[desc] = data;
        }
        public bool HasMethod(string keyName, out string methodDisplayName)
        {
            MacrossMethodData data;
            if (MacrossMethods.TryGetValue(keyName, out data))
            {
                methodDisplayName = data.GetDisplayName(null);
                return true;
            }
            methodDisplayName = "";
            return false;
        }

        public void SetPropertyBindMethod(string propertyName, Bricks.CodeBuilder.UMethodDeclaration desc, bool isSet)
        {
            MacrossMethodData data;
            if(MacrossMethods.TryGetValue(propertyName, out data))
            {
                var pbData = data as MacrossPropertyBindMethodData;
                if (isSet)
                    pbData.SetDesc = desc;
                else
                    pbData.GetDesc = desc;
            }
            else
            {
                var pbData = new TtUIElement.MacrossPropertyBindMethodData()
                {
                    PropertyName = propertyName,
                    HostElement = this,
                };
                if (isSet)
                    pbData.SetDesc = desc;
                else
                    pbData.GetDesc = desc;
                MacrossMethods[propertyName] = data;
            }
            mMethodDisplayNames[desc] = data;
        }

        public string GetVariableDisplayName(Bricks.CodeBuilder.UVariableDeclaration desc)
        {
            return this.Name;
        }
    }
}
