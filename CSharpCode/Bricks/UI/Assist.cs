using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.UI
{
    [AttributeUsage(AttributeTargets.Class)]
    public class Editor_BaseElementAttribute : Attribute
    {
        public string Icon;
        public Editor_BaseElementAttribute(string icon)
        {
            Icon = icon;
        }
    }

    public class Editor_UIControlAttribute : Attribute
    {
        public string Path;     // split with '.', use like parent.child.subChild
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
