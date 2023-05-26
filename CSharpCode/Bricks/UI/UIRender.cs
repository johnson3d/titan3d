using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.UI
{
    public class TtUIRender : ITickable
    {
        public int GetTickOrder()
        {
            return 0;
        }
        Canvas.TtCanvas mCanvas = new Canvas.TtCanvas();

        public void TickLogic(float ellapse)
        {
            throw new NotImplementedException();
        }

        public void TickRender(float ellapse)
        {
            throw new NotImplementedException();
        }
        public void TickBeginFrame(float ellapse)
        {

        }
        public void TickSync(float ellapse)
        {
            throw new NotImplementedException();
        }
    }
}
