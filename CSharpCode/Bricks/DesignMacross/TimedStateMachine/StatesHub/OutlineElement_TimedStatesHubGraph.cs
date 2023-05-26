using EngineNS.Bricks.CodeBuilder;
using EngineNS.DesignMacross.Description;
using EngineNS.DesignMacross.Outline;
using EngineNS.DesignMacross.Render;
using EngineNS.Rtti;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Reflection;
using System.Text;

namespace EngineNS.DesignMacross.TimedStateMachine
{
    public struct FTimedStatesHubGraphOutlineElementContext
    {
        public TtOutlineElement_TimedStateMachine Parent { get; set; }
    }
    [ImGuiElementRender(typeof(TtOutlineElementRender_TimedStatesHubGraph))]
    public class TtOutlineElement_TimedStatesHubGraph : IOutlineElement
    {
        public TtOutlineElement_TimedStateMachine StateMachineDeclaration;
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
                    var descList = list.Cast<IDescription>() as ObservableCollection<IDescription>;
                    instance.NotifiableDescriptions = descList;
                    instance.Construct();
                    Children.Add(instance);
                }
            }
        }
    }
    public class TtOutlineElementRender_TimedStatesHubGraph : IOutlineElementRender
    {
        public void Draw(IRenderableElement renderableElement, ref FOutlineElementRenderingContext context)
        {
            TtOutlineElement_TimedStatesHubGraph hubGraph = renderableElement as TtOutlineElement_TimedStatesHubGraph; ;
            Vector2 buttonSize = new Vector2(16, 16);
            float buttonOffset = 16;
            var sz = new Vector2(-1, 0);
            var regionSize = ImGuiAPI.GetContentRegionAvail();

            var treeNodeIsOpen = ImGuiAPI.TreeNodeEx(hubGraph.Description.Name, ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_OpenOnArrow | ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_DefaultOpen);
            ImGuiAPI.SameLine(0, EGui.UIProxy.StyleConfig.Instance.ItemSpacing.X);
            var designVarTreeNodeDoubleClicked = ImGuiAPI.IsItemDoubleClicked(ImGuiMouseButton_.ImGuiMouseButton_Left);
            var designVarTreeNodeIsItemClicked = ImGuiAPI.IsItemClicked(ImGuiMouseButton_.ImGuiMouseButton_Left);
            ImGuiAPI.SameLine(regionSize.X, -1.0f);
            var keyName = $"Delete DesignableVariable {hubGraph.Description.Name}?";
            if (EGui.UIProxy.CustomButton.ToolButton("x", in buttonSize, 0xFF0000FF, "DesignVar_X_" + hubGraph.Description.Name))
            {
                EGui.UIProxy.MessageBox.Open(keyName);
            }
            EGui.UIProxy.MessageBox.Draw(keyName, $"Are you sure to delete {hubGraph.Description.Name}?", EGui.UIProxy.MessageBox.EButtonType.YesNo,
            () =>
            {
                //bIsRemoved = true;
            }, null);
            if (treeNodeIsOpen)
            {
                if (designVarTreeNodeDoubleClicked)
                {
                    context.EditorInteroperation.DefinitionGraphEditPanel.EditDefinitionGraph(hubGraph.Description);
                }
                else if (designVarTreeNodeIsItemClicked)
                {
                    //PGMember.Target = method;
                }
                foreach (var state in hubGraph.Children)
                {
                    var stateRender = TtElementRenderDevice.CreateOutlineElementRender(state);
                    stateRender.Draw(state, ref context);
                }
                ImGuiAPI.TreePop();
            }
        }
    }

    #region OutlineElementsListRender_TimedStatesHubs
    [ImGuiElementRender(typeof(TtOutlineElementsListRender_TimedStatesHubs))]
    public class TtOutlineElementsList_TimedStatesHubs : IOutlineElementsList
    {
        public ObservableCollection<TtTimedStatesHubClassDescription> HubDescriptions { get => NotifiableDescriptions as ObservableCollection<TtTimedStatesHubClassDescription>; }
        INotifyCollectionChanged mNotifiableDescriptions = null;
        public INotifyCollectionChanged NotifiableDescriptions
        {
            get
            {
                return mNotifiableDescriptions;
            }
            set
            {
                mNotifiableDescriptions = value;
                mNotifiableDescriptions.CollectionChanged -= Descriptions_CollectionChanged;
                mNotifiableDescriptions.CollectionChanged += Descriptions_CollectionChanged;
            }
        }
        public string Name { get; set; }
        public List<IOutlineElement> Children { get; set; } = new List<IOutlineElement>();
        public IDescription Description { get; set; } = null;
        public TtTimedStateMachineClassDescription StateMachineClassDescription { get => Description as TtTimedStateMachineClassDescription; }
        public TtOutlineElementsList_TimedStatesHubs()
        {

        }

        private void Descriptions_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
                    {
                        
                    }
                    break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:
                    break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Replace:
                    break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Move:
                    break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Reset:
                    break;
            }
            Construct();
        }

        public void Construct()
        {
            Children.Clear();
            var list = (NotifiableDescriptions as IList).Cast<TtTimedStatesHubClassDescription>().ToList();
            foreach (var description in HubDescriptions)
            {
                var outlinerElementAttribute = description.GetType().GetCustomAttribute<OutlineElementAttribute>();
                if (outlinerElementAttribute != null)
                {
                    var instance = UTypeDescManager.CreateInstance(outlinerElementAttribute.ClassType) as IOutlineElement;
                    instance.Description = description;
                    instance.Construct();
                    Children.Add(instance);
                }
                var outlinerElementTreeAttribute = description.GetType().GetCustomAttribute<OutlineElementsListAttribute>();
                if (outlinerElementTreeAttribute != null)
                {
                    System.Diagnostics.Debug.Assert(false);
                }
            }

        }
    }
    public class TtOutlineElementsListRender_TimedStatesHubs : IOutlineElementsListRender
    {
        public void Draw(IRenderableElement renderableElement, ref FOutlineElementRenderingContext context)
        {
            TtOutlineElementsList_TimedStatesHubs hubsList = renderableElement as TtOutlineElementsList_TimedStatesHubs;
            Vector2 buttonSize = new Vector2(16, 16);
            float buttonOffset = 16;
            var sz = new Vector2(-1, 0);
            var regionSize = ImGuiAPI.GetContentRegionAvail();

            var treeNodeResult = ImGuiAPI.TreeNodeEx("StateHubs", ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_AllowItemOverlap | ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_DefaultOpen | ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_OpenOnDoubleClick);
            ImGuiAPI.SameLine(regionSize.X - buttonSize.X - buttonOffset, -1.0f);
            if (EGui.UIProxy.CustomButton.ToolButton("+", in buttonSize, 0xFF00FF00))
            {
                ImGuiAPI.OpenPopup("StateMachineVariablesSelectPopup", ImGuiPopupFlags_.ImGuiPopupFlags_None);
            }
            if (ImGuiAPI.BeginPopup("StateMachineVariablesSelectPopup", ImGuiWindowFlags_.ImGuiWindowFlags_None))
            {
                var hubVarName = "StateHub";
                var drawList = ImGuiAPI.GetWindowDrawList();
                var menuData = new Support.UAnyPointer();
                EGui.UIProxy.MenuItemProxy.MenuState newMethodMenuState = new EGui.UIProxy.MenuItemProxy.MenuState();
                newMethodMenuState.Reset();
                if (EGui.UIProxy.MenuItemProxy.MenuItem("New" + hubVarName, null, false, null, in drawList, in menuData, ref newMethodMenuState))
                {
                    var num = 0;
                    while (true)
                    {
                        var result = hubsList.Children.Find(chile => chile.Description.Name == $"{hubVarName}_{num}");
                        if (result == null)
                        {
                            break;
                        }
                        num++;
                    }
                    var hubDesc = new TtTimedStatesHubClassDescription();
                    hubDesc.Name = $"{hubVarName}_{num}";
                    hubDesc.StateMachineClassDescription = hubsList.StateMachineClassDescription;
                    context.CommandHistory.CreateAndExtuteCommand("AddStateHub",
                        () => { hubsList.HubDescriptions.Add(hubDesc); },
                        () => { hubsList.HubDescriptions.Remove(hubDesc); });

                }
                ImGuiAPI.EndPopup();
            }
            if (treeNodeResult)
            {
                foreach (var element in hubsList.Children)
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

    #endregion OutlineElementsListRender_TimedStatesHubs
}
