using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.EGui.Controls.PropertyGrid
{
    public class PGHideBaseClassPropertiesAttribute : Attribute { }
    public class PGShowInPropertyGridAttribute : Attribute { }
    public class PGValueRange : Attribute
    {
        public double Max;
        public double Min;
        public PGValueRange(double min, double max)
        {
            Max = max;
            Min = min;
        }
    }
    public class PGValueChangeStep : Attribute
    {
        public float Step = 1.0f;
        public PGValueChangeStep(float step)
        {
            Step = step;
        }
    }
    public class PGValueFormat : Attribute
    {
        public string Format = null;
        public PGValueFormat(string format)
        {
            Format = format;
        }
    }
}
