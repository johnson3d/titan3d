using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.UI.Controls
{
    public partial class TtUIElement
    {
        public string GetEventMethodName(string eventName)
        {
            return "On_" + eventName + "_" + Id;
        }

        public string GetEventMethodDisplayName(Bricks.CodeBuilder.UMethodDeclaration desc)
        {
            var start = desc.MethodName.IndexOf('_') + 1;
            var end = desc.MethodName.IndexOf('_', start);
            var eventName = desc.MethodName.Substring(start, end - start);
            return $"On{eventName}({Name})";
        }
    }
}
