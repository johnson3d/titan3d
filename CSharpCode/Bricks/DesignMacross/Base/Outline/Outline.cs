using EngineNS.DesignMacross.Base.Description;
using EngineNS.DesignMacross.Base.Render;
using EngineNS.Rtti;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Text;

namespace EngineNS.DesignMacross.Base.Outline

{

    [ImGuiElementRender(typeof(TtOutlineRender))]
    public class TtOutline : IOutline
    {
        public string Name { get; set; }
        public List<IOutlineElement> Children { get; set; } = new List<IOutlineElement>();
        public IDescription Description { get; set; } = null;

        public void Construct()
        {
            foreach (var property in Description.GetType().GetProperties())
            {
                var outlinerElementAttribute = property.GetCustomAttribute<OutlineElementAttribute>();
                if (outlinerElementAttribute != null)
                {
                    var instance = UTypeDescManager.CreateInstance(outlinerElementAttribute.ClassType) as IOutlineElement;
                    var desc = property.GetValue(Description) as IDescription;
                    instance.Description = desc;
                    instance.Construct();
                    Children.Add(instance);
                }
                var outlinerElementTreeAttribute = property.GetCustomAttribute<OutlineElementsListAttribute>();
                if (outlinerElementTreeAttribute != null)
                {
                    var instance = UTypeDescManager.CreateInstance(outlinerElementTreeAttribute.ClassType) as IOutlineElementsList;
                    var list = property.GetValue(Description) as IList;
                    var listType = list.GetType();
                    if(listType.IsGenericType && listType.GenericTypeArguments[0] == typeof(IDesignableVariableDescription))
                    {
                        var descList = list.Cast<IDesignableVariableDescription>() as ObservableCollection<IDesignableVariableDescription>;
                        instance.NotifiableDescriptions = descList;
                    }
                    else if (listType.IsGenericType && listType.GenericTypeArguments[0] == typeof(IVariableDescription))
                    {
                        var descList = list.Cast<IVariableDescription>() as ObservableCollection<IVariableDescription>;
                        instance.NotifiableDescriptions = descList;
                    }
                    else if(listType.IsGenericType && listType.GenericTypeArguments[0] == typeof(IDesignableVariableDescription))
                    {
                        var descList = list.Cast<IMethodDescription>() as ObservableCollection<IMethodDescription>;
                        instance.NotifiableDescriptions = descList;
                    }
                    else
                    {
                        var descList = list.Cast<IDescription>() as ObservableCollection<IDescription>;
                        instance.NotifiableDescriptions = descList;
                    }
                    instance.Construct();
                    Children.Add(instance);
                }
            }
        }
    }

    public class TtOutlineRender : IOutlineRender
    {
        public void Draw(IRenderableElement renderableElement, ref FOutlineRenderingContext context)
        {
            var outline = renderableElement as IOutline;
            var elementContext = new FOutlineElementRenderingContext();
            elementContext.CommandHistory = context.CommandHistory;
            elementContext.EditorInteroperation = context.EditorInteroperation;
            foreach (var element in outline.Children)
            {
                var elementRender = TtElementRenderDevice.CreateOutlineElementRender(element);
                elementRender.Draw(element, ref elementContext);
            }
        }
    }
}
