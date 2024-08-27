using EngineNS.GamePlay.Scene;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.GamePlay.Player
{
    [Bricks.CodeBuilder.ContextMenu("PlayerStart", "PlayerStart", UNode.EditorKeyword)]
    [UNode(NodeDataType = typeof(TtPlayerStart.TtPlayerStartData), DefaultNamePrefix = "PlayerStart")]
    public partial class TtPlayerStart : USceneActorNode
    {
        public partial class TtPlayerStartData : UNodeData
        {

        }
        public TtPlayerStartData PlayerStartData
        {
            get
            {
                return NodeData as TtPlayerStartData;
            }
        }
    }
}
