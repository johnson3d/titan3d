using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EngineNS.Animation.Pipeline
{
    public interface IAnimationCommand
    {
        public void Execute();
    }
}
