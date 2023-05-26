using EngineNS.DesignMacross.Graph;
using NPOI.SS.Formula.Functions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace EngineNS.DesignMacross.Graph
{

    public class TtMouseEventProcesser
    {
        static TtMouseEventProcesser mInstance = null;
        public static TtMouseEventProcesser Instance 
        { 
            get
            {
                if(mInstance == null)
                    mInstance = new TtMouseEventProcesser();
                return mInstance;
            }
        }

        IGraphElement ProcessSelectableElement(IGraphElement element, Vector2 pos)
        {
            IGraphElement finalHit = null;
            if (element is ISelectable selectableElement)
            {
                if (selectableElement.HitCheck(pos))
                {
                    selectableElement.OnSelected();
                    finalHit = element;
                    HitElementStack.Push(element);
                    if (element is IEnumChild enumChild)
                    {
                        var children = enumChild.EnumerateChild<IGraphElement>();
                        foreach (var child in children)
                        {
                            var  hit = ProcessSelectableElement(child, pos);
                            if(hit != null)
                            {
                                finalHit = hit;
                                break;
                            }
                        }
                    }
                }
            }
            return finalHit;
        }

        IContextMeunable mOpenedContextMenu = null;
        public Stack<IGraphElement> HitElementStack = new Stack<IGraphElement>();
        public IGraphElement LastHitElement { get; set; }
        public void Processing(IGraph graph, FGraphRenderingContext context)
        {
            if (ImGuiAPI.IsMouseDown(ImGuiMouseButton_.ImGuiMouseButton_Left) || ImGuiAPI.IsMouseDown(ImGuiMouseButton_.ImGuiMouseButton_Right))
            {             
                LastHitElement = ProcessSelectableElement(graph, context.ViewPort.ViewPortInverseTransform(context.Camera.Location, ImGuiAPI.GetMousePos()));
            }
            if (ImGuiAPI.IsMouseClicked(ImGuiMouseButton_.ImGuiMouseButton_Left, false))
            {

            }
            if (ImGuiAPI.IsMouseDragging(ImGuiMouseButton_.ImGuiMouseButton_Left, -1.0f))
            {
                var delta = ImGuiAPI.GetMouseDragDelta(ImGuiMouseButton_.ImGuiMouseButton_Left, -1.0f);
                if (LastHitElement is IDraggable draggableEle)
                {
                    draggableEle.OnDragging(delta);
                    ImGuiAPI.ResetMouseDragDelta(ImGuiMouseButton_.ImGuiMouseButton_Left);
                }

            }
        }
    }
}
