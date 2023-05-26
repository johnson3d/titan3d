using EngineNS.DesignMacross.Outline;
using EngineNS.DesignMacross.TimedStateMachine;
using EngineNS.Rtti;
using NPOI.SS.Formula.Functions;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace EngineNS.DesignMacross.Render
{
    public interface IRenderableElement
    {

    }

    public interface IElementRender<T> where T : struct
    {
        public void Draw(IRenderableElement renderableElement, ref T context);
    }
}
