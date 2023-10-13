using EngineNS.Bricks.CodeBuilder;
using EngineNS.Bricks.CodeBuilder.MacrossNode;
using EngineNS.Bricks.NodeGraph;
using EngineNS.DesignMacross.Editor;
using EngineNS.DesignMacross.Base.Graph;
using EngineNS.DesignMacross.Base.Render;
using EngineNS.DesignMacross.Base.Outline;
using EngineNS.DesignMacross.TimedStateMachine;
using EngineNS.Rtti;
using Org.BouncyCastle.Crypto.Agreement;
using SixLabors.Fonts;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using EngineNS.Bricks.StateMachine.TimedSM;
using System.Collections.ObjectModel;
using MathNet.Numerics.Statistics;
using System.Collections.Specialized;
using EngineNS.DesignMacross.Base.Description;

namespace EngineNS.DesignMacross.TimedStateMachine
{
    #region OutlineElement_TimedStateMachine
    [ImGuiElementRender(typeof(TtOutlineElementRender_TimedStateMachine))]
    public class TtOutlineElement_TimedStateMachine : IDesignableVariableOutlineElement
    {
        public string Name { get; set; } // { get => VariableDeclaration.VariableName; set => VariableDeclaration.VariableName = value; }
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
                    if(list is INotifyCollectionChanged notifyCollection)
                    {
                        instance.NotifiableDescriptions = notifyCollection;
                        instance.Construct();
                        instance.Description = Description;
                        Children.Add(instance);
                    }

                }
            }
        }
    }
    public class TtOutlineElementRender_TimedStateMachine : IOutlineElementRender
    {
        public void Draw(IRenderableElement renderableElement, ref FOutlineElementRenderingContext context)
        {
            TtOutlineElement_TimedStateMachine stateMachineElement = renderableElement as TtOutlineElement_TimedStateMachine;
            var varibleName = "StateMachine_" + stateMachineElement.Description.Name;
            Vector2 buttonSize = new Vector2(16, 16);

            var treeNodeResult = ImGuiAPI.TreeNodeEx(varibleName, ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_AllowItemOverlap | ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_DefaultOpen | ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_OpenOnDoubleClick);
            var regionSize = ImGuiAPI.GetContentRegionAvail();
            ImGuiAPI.SameLine(regionSize.X , -1.0f);
            var keyName = $"Delete DesignableVariable {varibleName}?";
            if (EGui.UIProxy.CustomButton.ToolButton("x", in buttonSize, 0xFF0000FF, "DesignVar_X_" + varibleName))
            {
                EGui.UIProxy.MessageBox.Open(keyName);
            }

            EGui.UIProxy.MessageBox.Draw(keyName, $"Are you sure to delete {varibleName}?", EGui.UIProxy.MessageBox.EButtonType.YesNo,
            () =>
            {
                
            }, null);
            if (ImGuiAPI.BeginPopup("StateMachineVariablesSelectPopup", ImGuiWindowFlags_.ImGuiWindowFlags_None))
            {
                var menuItemName = "StateHub";
                var drawList = ImGuiAPI.GetWindowDrawList();
                var menuData = new Support.UAnyPointer();
                EGui.UIProxy.MenuItemProxy.MenuState newMethodMenuState = new EGui.UIProxy.MenuItemProxy.MenuState();
                newMethodMenuState.Reset();
                if (EGui.UIProxy.MenuItemProxy.MenuItem("New" + menuItemName, null, false, null, in drawList, in menuData, ref newMethodMenuState))
                {
                    var num = 0;
                    while (true)
                    {
                        var result = stateMachineElement.Children.Find(chile => chile.Description.Name == $"{menuItemName}_{num}");
                        if (result == null)
                        {
                            break;
                        }
                        num++;
                    }
                    var smDesc = stateMachineElement.Description as TtTimedStateMachineClassDescription;
                    var hubDesc = new TtTimedStatesHubClassDescription();
                    hubDesc.StateMachineClassDescription = smDesc;
                    hubDesc.Name = $"{menuItemName}_{num}";
                    smDesc.Hubs.Add(hubDesc);
                }
                ImGuiAPI.EndPopup();
            }
            if (treeNodeResult)
            {
                foreach (var element in stateMachineElement.Children)
                {
                    var elementRender = TtElementRenderDevice.CreateOutlineElementRender(element);
                    if (elementRender != null)
                    {
                        elementRender.Draw(element, ref context);
                    }
                }
                ImGuiAPI.TreePop();
            }
        }


    }
    #endregion OutlineElement_TimedStateMachine

}
