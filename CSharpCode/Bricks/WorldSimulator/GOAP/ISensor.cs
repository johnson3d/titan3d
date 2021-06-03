using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.WorldSimulator.GOAP
{
    public class ISensor
    {
        public virtual void OnSensor(string dataType, IEnvironment env)
        {//感知到dataType变化

        }
    }
}
