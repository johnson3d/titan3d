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
            if (PickedProxies.Contains(obj))
                return;
            PickedProxies.Add(obj);
        }
        public void Unselected(IProxiable obj)
        {
            PickedProxies.Remove(obj);
        }
        public void ClearSelected()
        {
            PickedProxies.Clear();
        }
    }
}
