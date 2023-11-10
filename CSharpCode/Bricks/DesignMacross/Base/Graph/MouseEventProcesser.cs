using EngineNS.DesignMacross.Base.Graph;
using NPOI.SS.Formula.Functions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace EngineNS.DesignMacross.Base.Graph
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
            if (element is IGraphElementSelectable selectableElement)
            {
                if (selectableElement.HitCheck(pos))
                {
                    //selectableElement.OnSelected();
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

        internal IContextMeunable mOpenedContextMenu = null;
        public Stack<IGraphElement> HitElementStack = new Stack<IGraphElement>();
        public IGraphElementSelectable LastHitElement { get; set; }
        public void Processing(IGraph graph, ref FGraphElementRenderingContext context)
        {
            if (ImGuiAPI.IsMouseDown(ImGuiMouseButton_.ImGuiMouseButton_Left) || ImGuiAPI.IsMouseDown(ImGuiMouseButton_.ImGuiMouseButton_Right))
            {             
                LastHitElement = ProcessSelectableElement(graph, context.ViewPort.ViewportInverseTransform(context.Camera.Location, ImGuiAPI.GetMousePos()));
            }
            if (ImGuiAPI.IsMouseClicked(ImGuiMouseButton_.ImGuiMouseButton_Left, false) && LastHitElement != null)
            {
                LastHitElement.OnSelected(ref context);
            }
            if (ImGuiAPI.IsMouseDragging(ImGuiMouseButton_.ImGuiMouseButton_Left, -1.0f) && LastHitElement != null)
            {
                var delta = ImGuiAPI.GetMouseDragDelta(ImGuiMouseButton_.ImGuiMouseButton_Left, -1.0f);
                if (LastHitElement is IGraphElementDraggable draggableEle)
                {
                    draggableEle.OnDragging(delta);
                    ImGuiAPI.ResetMouseDragDelta(ImGuiMouseButton_.ImGuiMouseButton_Left);
                }

            }
        }
    }
}
