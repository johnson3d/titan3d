using System;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows;

using WPG.Data;
using System.ComponentModel.Composition;

namespace WPG
{
    public class PropertyTemplateSelector : DataTemplateSelector
	{
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
		{
			Property property = item as Property;
			if (property == null)
			{
				throw new ArgumentException("item must be of type Property");
			}
			FrameworkElement element = container as FrameworkElement;
			if (element == null)
			{
				return base.SelectTemplate(property.Value, container);
			}
			DataTemplate template = FindDataTemplate(property, element);
			return template;
		}		

		private DataTemplate FindDataTemplate(Property property, FrameworkElement element)
		{
			Type propertyType = property.PropertyType;
            var attributes = property.ValueAttributes;

            foreach (var attr in attributes)
            {
                if (attr is EngineNS.Editor.Editor_ColorPicker)
                {
                    return TryFindDataTemplate(element, "ColorPicker");
                }
                else if (attr is EngineNS.Editor.Editor_Color4Picker)
                {
                    return TryFindDataTemplate(element, "Color4Picker");
                }
                else if (attr is EngineNS.Editor.UIEditor_PropertysWithAutoSet)
                {
                    return TryFindDataTemplate(element, "IntWithAutoSet");
                }
                else if (attr is EngineNS.Editor.OpenFileEditorAttribute)
                {
                    return TryFindDataTemplate(element, "FileEditor");
                }
                else if (attr is EngineNS.Editor.OpenFolderEditorAttribute)
                {
                    return TryFindDataTemplate(element, "FileEditor");
                }
                else if (attr is EngineNS.Editor.Editor_ValueWithRange)
                {
                    return TryFindDataTemplate(element, "ValueWithRange");
                }
                else if (attr is EngineNS.Editor.Editor_VectorEditor)
                {
                    return TryFindDataTemplate(element, "VectorEditor");
                }
                else if (attr is EngineNS.Editor.Editor_Angle360Setter)
                {
                    return TryFindDataTemplate(element, "Angle360Setter");
                }
                else if (attr is EngineNS.Editor.Editor_Angle180Setter)
                {
                    return TryFindDataTemplate(element, "Angle180Setter");
                }
                else if (attr is EngineNS.Editor.Editor_HotKeySetter)
                {
                    return TryFindDataTemplate(element, "HotKeySetter");
                }
                else if(attr is EngineNS.Editor.Editor_FlagsEnumSetter)
                {
                    return TryFindDataTemplate(element, "FlagsEnumEditor");
                }
                else if (attr is EngineNS.Editor.Editor_PropertyGridDataTemplateAttribute)
                {
                    var dtAtt = attr as EngineNS.Editor.Editor_PropertyGridDataTemplateAttribute;
                    var retValue = Program.GetRegisterDataTemplate(dtAtt.DataTemplateType);
                    if (retValue != null)
                        return retValue;
                }
                else if(attr is EngineNS.Editor.Editor_RNameTypeAttribute ||
                        attr is EngineNS.Editor.Editor_RNameMacrossType ||
                        attr is EngineNS.Editor.Editor_RNameMExcelType)
                {
                    return TryFindDataTemplate(element, "RNameEditor");
                }
                else if(attr is EngineNS.Editor.Editor_InputWithErrorCheckAttribute)
                {
                    return TryFindDataTemplate(element, "InputWithErrorCheck");
                }
                else if(attr is EngineNS.Editor.Editor_SocketSelectAttribute)
                {
                    return TryFindDataTemplate(element, "SocketSelecter");
                }
                else if (attr is EngineNS.Editor.Editor_LAGraphBonePoseSelectAttribute)
                {
                    return TryFindDataTemplate(element, "LAGraphBonePoseSelecter");
                }
                else if (attr is EngineNS.Editor.Editor_ClassPropertySelectAttributeAttribute)
                {
                    return TryFindDataTemplate(element, "ClassPropertySelecter");
                }
            }

            //if (!(property.PropertyType is String) && property.PropertyType is IEnumerable)
            //    propertyType = typeof(List<object>);
            if (property.PropertyType.IsGenericType)// is IEnumerable)
                propertyType = typeof(List<object>);
            
			DataTemplate template = TryFindDataTemplate(element, propertyType);

    		while (template == null && propertyType.BaseType != null)
			{
				propertyType = propertyType.BaseType;
				template = TryFindDataTemplate(element, propertyType);
			}
			if (template == null)
			{
				template = TryFindDataTemplate(element, "default");
			}
			return template;
		}

		private static DataTemplate TryFindDataTemplate(FrameworkElement element, object dataTemplateKey)
		{
			object dataTemplate = element.TryFindResource(dataTemplateKey);
			if (dataTemplate == null)
			{
				dataTemplateKey = new ComponentResourceKey(typeof(PropertyGrid), dataTemplateKey);
				dataTemplate = element.TryFindResource(dataTemplateKey);
			}
			return dataTemplate as DataTemplate;
		}
	}
}
