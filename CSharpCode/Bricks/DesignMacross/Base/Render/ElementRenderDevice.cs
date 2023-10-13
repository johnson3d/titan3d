using EngineNS.DesignMacross.Base.Graph;
using EngineNS.DesignMacross.Base.Outline;
using EngineNS.Rtti;
using NPOI.SS.Formula.Functions;
using SixLabors.Fonts;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace EngineNS.DesignMacross.Base.Render
{
    public class TtElementRenderDevice
    {
        static Dictionary<Type, IOutlineRender> OutlineRenders = new Dictionary<Type, IOutlineRender>();
        static Dictionary<Type, IOutlineElementRender> OutlineElementRenders = new Dictionary<Type, IOutlineElementRender>();
        static Dictionary<Type, IGraphRender> GraphRenders = new Dictionary<Type, IGraphRender>();
        static Dictionary<Type, IGraphElementRender> GraphElementRenders = new Dictionary<Type, IGraphElementRender>();

        public static IElementRenderDeviceImplementation RenderDeviceImp = new TtImGuiElementRenderImplementation();
        public static IElementRender<U> CreateRender<U>(IRenderableElement element) where U : struct 
        {
            return RenderDeviceImp.CreateRender<U>(element);
        }
        public static IGraphElementRender CreateGraphElementRender(IGraphElement graphElement)
        {
            if(GraphElementRenders.ContainsKey(graphElement.GetType()))
            {
                return GraphElementRenders[graphElement.GetType()];
            }

            var render = RenderDeviceImp.CreateRender<FGraphElementRenderingContext>(graphElement) as IGraphElementRender;
            System.Diagnostics.Debug.Assert(render != null);
            GraphElementRenders.Add(graphElement.GetType(), render);
            return render;
        }
        public static IGraphRender CreateGraphRender(IGraph graph)
        {
            if(GraphRenders.ContainsKey(graph.GetType()))
            { 
                return GraphRenders[graph.GetType()];
            }

            var render = RenderDeviceImp.CreateRender<FGraphRenderingContext>(graph) as IGraphRender;
            System.Diagnostics.Debug.Assert(render != null);
            GraphRenders.Add(graph.GetType(), render);
            return render;
        }
        public static IOutlineElementRender CreateOutlineElementRender(IOutlineElement outlineElement)
        {
            if(OutlineElementRenders.ContainsKey(outlineElement.GetType()))
            {
                return OutlineElementRenders[outlineElement.GetType()];
            }

            var render = RenderDeviceImp.CreateRender<FOutlineElementRenderingContext>(outlineElement) as IOutlineElementRender;
            System.Diagnostics.Debug.Assert(render != null);
            OutlineElementRenders.Add(outlineElement.GetType(), render);
            return render;
        }
        //public static IOutlineElementsListRender CreateOutlineElementsListRender(IOutlineElementsList elementsList)
        //{
        //    var attr = elementsList.GetType().GetCustomAttribute<ImGuiElementRenderAttribute>();
        //    var instance = UTypeDescManager.CreateInstance(attr.RenderType) as IOutlineElementsListRender;
        //    instance.ElementsList = elementsList;
        //    return instance;
        //}
        public static IOutlineRender CreateOutlineRender(IOutline outline)
        {
            if(OutlineRenders.ContainsKey(outline.GetType()))
            {
                return OutlineRenders[outline.GetType()];
            }

            var render = CreateRender<FOutlineRenderingContext>(outline) as IOutlineRender;
            System.Diagnostics.Debug.Assert(render != null);
            OutlineRenders.Add(outline.GetType(), render);
            return render;
        }
    }

    public interface IElementRenderDeviceImplementation
    {
        public IElementRender<U> CreateRender<U>(IRenderableElement element) where U : struct;
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class ImGuiElementRenderAttribute : Attribute
    {
        public Type RenderType;
        public ImGuiElementRenderAttribute(Type type)
        {
            RenderType = type;
        }
    }
    public class TtImGuiElementRenderImplementation : IElementRenderDeviceImplementation
    {
        public IElementRender<U> CreateRender<U>(IRenderableElement element) where U : struct
        {
            if (element.GetType().GetCustomAttribute(typeof(ImGuiElementRenderAttribute)) is ImGuiElementRenderAttribute attr)
            {
                if (attr.RenderType.IsAssignableTo(typeof(IElementRender<U>)))
                {
                    return (IElementRender<U>)UTypeDescManager.CreateInstance(attr.RenderType);
                }
            }
            return null;
        }
    }
}
