using EngineNS.DesignMacross.Base.Description;
using EngineNS.DesignMacross.Base.Graph;
using EngineNS.DesignMacross.Base.Render;
using EngineNS.DesignMacross.Design.ConnectingLine;
using EngineNS.DesignMacross.Design.Expressions;
using EngineNS.DesignMacross.Design.Statement;
using EngineNS.DesignMacross.Editor;
using EngineNS.Rtti;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.DesignMacross.Design.Statements
{
    [ImGuiElementRender(typeof(TtGraphElementRender_StatementDescription))]
    public class TtGraphElement_CastStatementDescription : TtGraphElement_StatementDescription
    {
        public TtCastStatementDescription CastStatementDescription { get => Description as TtCastStatementDescription; }
        private TtGraphElement_ComboBox ComboBox = new TtGraphElement_ComboBox();
        public TtGraphElement_CastStatementDescription(IDescription description, IGraphElementStyle style) : base(description, style)
        {
            List<TtTypeDesc> types = new();
            foreach (var service in Rtti.TtTypeDescManager.Instance.Services.Values)
            {
                    types.AddRange(service.Types.Values);
            }
            ComboBox.Items = types;
            ComboBox.CurrentSelected = CastStatementDescription.TargetType;
            ComboBox.OnComboBoxSelect += OnComboBoxSelected;
        }
        
        void OnComboBoxSelected(object selectedItem)
        {
            CastStatementDescription.TargetType = (TtTypeDesc)selectedItem;
        }
        public override void ConstructElements(ref FGraphElementRenderingContext context)
        {
            ComboBox.CurrentSelected = CastStatementDescription.TargetType;
            LeftSidePinsStackPanel.Clear();
            {
                foreach (var execInPin in StatementDescription.ExecutionInPins)
                {
                    var execPinAttribute = GraphElementAttribute.GetAttributeWithSpecificClassType<TtGraphElement_ExecutionPin>(execInPin.GetType());
                    if (execPinAttribute != null)
                    {
                        var instance = TtDescriptionGraphElementsPoolManager.Instance.GetDescriptionGraphElement(execPinAttribute.ClassType, execInPin, context.GraphElementStyleManager.GetOrAdd(execInPin.Id)) as TtGraphElement_ExecutionPin;
                        instance.Parent = this;
                        instance.ConstructElements(ref context);
                        LeftSidePinsStackPanel.AddElement(instance);
                        context.DescriptionsElement.Add(execInPin.Id, instance);
                    }
                }
                LeftSidePinsStackPanel.AddElement(ComboBox);
                foreach (var dataPin in StatementDescription.DataInPins)
                {
                    var dataPinAttribute = GraphElementAttribute.GetAttributeWithSpecificClassType<TtGraphElement_DataPin>(dataPin.GetType());
                    if (dataPinAttribute != null)
                    {
                        var instance = TtDescriptionGraphElementsPoolManager.Instance.GetDescriptionGraphElement(dataPinAttribute.ClassType, dataPin, context.GraphElementStyleManager.GetOrAdd(dataPin.Id)) as TtGraphElement_DataPin;
                        instance.Parent = this;
                        instance.ConstructElements(ref context);
                        LeftSidePinsStackPanel.AddElement(instance);
                        context.DescriptionsElement.Add(dataPin.Id, instance);
                    }
                }
            }
            RightSidePinsStackPanel.Clear();
            {
                foreach (var execOutPin in StatementDescription.ExecutionOutPins)
                {
                    var execPinAttribute = GraphElementAttribute.GetAttributeWithSpecificClassType<TtGraphElement_ExecutionPin>(execOutPin.GetType());
                    if (execPinAttribute != null)
                    {
                        var instance = TtDescriptionGraphElementsPoolManager.Instance.GetDescriptionGraphElement(execPinAttribute.ClassType, execOutPin, context.GraphElementStyleManager.GetOrAdd(execOutPin.Id)) as TtGraphElement_ExecutionPin;
                        instance.Parent = this;
                        instance.ConstructElements(ref context);
                        RightSidePinsStackPanel.AddElement(instance);
                        context.DescriptionsElement.Add(execOutPin.Id, instance);
                    }
                }
                foreach (var dataPin in StatementDescription.DataOutPins)
                {
                    var dataPinAttribute = GraphElementAttribute.GetAttributeWithSpecificClassType<TtGraphElement_DataPin>(dataPin.GetType());
                    if (dataPinAttribute != null)
                    {
                        var instance = TtDescriptionGraphElementsPoolManager.Instance.GetDescriptionGraphElement(dataPinAttribute.ClassType, dataPin, context.GraphElementStyleManager.GetOrAdd(dataPin.Id)) as TtGraphElement_DataPin;
                        instance.Parent = this;
                        instance.ConstructElements(ref context);
                        RightSidePinsStackPanel.AddElement(instance);
                        context.DescriptionsElement.Add(dataPin.Id, instance);
                    }
                }
            }
        }
    }
}
