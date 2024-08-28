using EngineNS.GamePlay.Scene;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.GamePlay.Player
{
    [Bricks.CodeBuilder.ContextMenu("PlayerStart", "PlayerStart", TtNode.EditorKeyword)]
    [TtNode(NodeDataType = typeof(TtPlayerStart.TtPlayerStartData), DefaultNamePrefix = "PlayerStart")]
    public partial class TtPlayerStart : TtSceneActorNode
    {
        public partial class TtPlayerStartData : TtNodeData
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
