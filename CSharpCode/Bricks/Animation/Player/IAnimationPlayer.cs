using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EngineNS.Animation.Player
{
    public interface IAnimationPlayer
    {
        public void Update(float elapse);
        public void Evaluate();
    }

}
