using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Graphics.Pipeline.Common
{
    public class UPickedProxiableManager
    {
        public List<IProxiable> PickedProxies = new List<IProxiable>();
        public void Selected(IProxiable obj)
        {
            var node = obj as GamePlay.Scene.UNode;
            if (node != null && node.HasStyle(GamePlay.Scene.UNode.ENodeStyles.NoPickedDraw))
                return;
            if (PickedProxies.Contains(obj))
                return;
            PickedProxies.Add(obj);
        }
        public void Unselected(IProxiable obj)
        {
            var node = obj as GamePlay.Scene.UNode;
            if (node != null && node.HasStyle(GamePlay.Scene.UNode.ENodeStyles.NoPickedDraw))
                return;
            PickedProxies.Remove(obj);
        }
        public void ClearSelected()
        {
            PickedProxies.Clear();
        }
    }
}
