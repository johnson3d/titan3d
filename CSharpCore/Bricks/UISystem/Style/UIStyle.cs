using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EngineNS.UISystem.Style
{
    [Rtti.MetaClass]
   
    public abstract class UIStyle : EngineNS.IO.Serializer.Serializer
    {
        public abstract Task Initialize(CRenderContext rc, UIElement hostElement);
    }
}
