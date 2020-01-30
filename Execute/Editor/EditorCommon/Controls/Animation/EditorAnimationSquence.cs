using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EditorCommon.Controls.Animation
{
    public class EditorAnimationSquence : EngineNS.Graphics.Mesh.Animation.CGfxAnimationSequence
    {
        public EditorAnimationSquence()
        {
        }

        public override void Update(long time)
        {
            //no update
        }
        public void ManualUpdate(long time)
        {
            base.Update(time);
        }
    }
}
