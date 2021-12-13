using EngineNS.GamePlay.Scene;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.GamePlay.Controller
{
    public class UNPCController : UNode, IController
    {
        //UE NavMesh 寻路接口放在Movement里面，我觉得有些不合理，
        //Movement的在设计上应该保持简单，只管移动就好。
        //意图通过NPCController调用Nav信息来指导 movement
    }
}
