using EngineNS.DesignMacross.Editor;
using EngineNS.DesignMacross.Graph;
using EngineNS.DesignMacross.Render;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.DesignMacross.Outline
{
    public struct FOutlineRenderingContext
    {
        public TtCommandHistory CommandHistory { get; set; }
        public TtEditorInteroperation EditorInteroperation { get; set; }
    }
    public struct FOutlineElementRenderingContext
    {
        public TtCommandHistory CommandHistory { get; set; }
        public TtEditorInteroperation EditorInteroperation { get; set; }
    }

    public interface IOutlineRender : IElementRender<FOutlineRenderingContext>
    {
    }

    public interface IOutlineElementRender : IElementRender<FOutlineElementRenderingContext>
    {
    }

    public interface IOutlineElementsListRender : IOutlineElementRender
    {
     
    }
}
