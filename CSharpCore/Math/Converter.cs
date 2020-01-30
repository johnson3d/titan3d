using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS
{
    [EngineNS.Editor.Editor_MacrossClass(ECSType.Common, Editor.Editor_MacrossClassAttribute.enMacrossType.Useable)]
    public class Converter
    {
        [EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public static string Tostring(float value)
        {
            return System.Convert.ToString(value);
        }
        [EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public static string Tostring(double value)
        {
            return System.Convert.ToString(value);
        }
    }
}
