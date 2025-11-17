using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.GamePlay.Scene
{
    [Bricks.CodeBuilder.ContextMenu("NodeHub", "NodeHub", TtNode.EditorKeyword)]
    [TtNode(NodeDataType = typeof(TtNodeData), DefaultNamePrefix = "NodeHub")]
    public class TtHubNode : TtLightWeightNodeBase
    {
        public override unsafe bool IsTreeContain(DVector3* localStart, DVector3* dir, DBoundingBox* pBox)
        {
            return true;
        }
    }
}
