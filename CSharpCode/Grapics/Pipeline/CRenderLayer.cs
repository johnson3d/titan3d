using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Graphics.Pipeline
{
    public enum ERenderLayer
    {
        Opaque,
        CustomOpaque,
        Translucent,
        CustomTranslucent,
        //for editor to use;this layer should always be the last layer to send to renderer;
        Gizmos,
        Shadow,
        Sky,

        Num,
    }
    public class CRenderLayer : AuxPtrType<IRenderLayer>
    {
        public CRenderLayer()
        {
            mCoreObject = IRenderLayer.CreateInstance();
        }
        public ERenderLayer RLayer { get; set; } = ERenderLayer.Opaque;
    }
}
