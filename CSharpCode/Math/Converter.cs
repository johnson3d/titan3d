using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS
{
    public partial class Converter
    {
        [Rtti.Meta]
        public static string ToString(float value)
        {
            return System.Convert.ToString(value);
        }
        [Rtti.Meta]
        public static string ToString(double value)
        {
            return System.Convert.ToString(value);
        }
    }
}
