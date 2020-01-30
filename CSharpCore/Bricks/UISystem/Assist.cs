using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.UISystem
{
    [AttributeUsage(AttributeTargets.Class)]
    public class Editor_BaseElementAttribute : EngineNS.Editor.Editor_BaseAttribute
    {
        public string Icon;
        public override object[] GetConstructParams()
        {
            return null;
        }
        public Editor_BaseElementAttribute(string icon)
        {
            Icon = icon;
        }
    }
    [AttributeUsage(AttributeTargets.Class)]
    public class Editor_UIControlInitAttribute : Attribute
    {
        public Type InitializerType;
        public Editor_UIControlInitAttribute(Type initerType)
        {
            InitializerType = initerType;
        }
    }

    public class Editor_UIControlAttribute : Attribute
    {
        public string Path;
        public string Description;
        public string Icon;
        public Editor_UIControlAttribute(string path, string description, string icon)
        {
            Path = path;
            Description = description;
            Icon = icon;
        }
    }

    public class Editor_UIEvent : Attribute
    {
        public string Description;
        public Editor_UIEvent(string description)
        {
            Description = description;
        }
    }
}
