using EngineNS.GamePlay.Scene;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.GamePlay.Controller
{
    public class TtAIController : TtLightWeightNodeBase, IController
    {
        //Movement的在设计上应该保持简单，只管移动就好。
        //意图通过AIController调用Nav信息来指导 movement
    }
}
