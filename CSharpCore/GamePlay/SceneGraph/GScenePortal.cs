using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.GamePlay.SceneGraph
{
    public class GScenePortal
    {
        public ISceneNode PosSide;
        public ISceneNode NegSide;
        public Vector3[] GeomVerts;
        //
        public CPass ShapePass;
    }
}
