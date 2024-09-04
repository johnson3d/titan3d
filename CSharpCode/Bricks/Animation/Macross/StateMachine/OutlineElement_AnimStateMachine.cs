using EngineNS.DesignMacross.Base.Render;
using EngineNS.DesignMacross.Base.Outline;
using EngineNS.DesignMacross.Base.Description;
using EngineNS.Bricks.StateMachine.Macross;
using EngineNS.Bricks.Animation.Macross.StateMachine.CompoundState;
using EngineNS.Bricks.Animation.Macross.StateMachine;

namespace EngineNS.Bricks.Animation.Macross
{
    #region OutlineElement_AnimStateMachine
    [ImGuiElementRender(typeof(TtOutlineElementRender_AnimStateMachine))]
    public class TtOutlineElement_AnimStateMachine : TtOutlineElement_TimedStateMachine
    {

    }
    public class TtOutlineElementRender_AnimStateMachine : IOutlineElementRender
    {
        public void Draw(IRenderableElement renderableElement, ref FOutlineElementRenderingContext context)
        {
            var stateMachineElement = renderableElement as TtOutlineElement_AnimStateMachine;
            var varibleName = stateMachineElement.Description.Name;
            Vector2 buttonSize = new Vector2(16, 16);

            var treeNodeIsOpen = ImGuiAPI.TreeNodeEx(varibleName, ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_AllowItemOverlap | ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_DefaultOpen | ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_OpenOnDoubleClick);
            var regionSize = ImGuiAPI.GetContentRegionAvail();
            ImGuiAPI.SameLine(regionSize.X , -1.0f);
            var isTreeNodeDoubleClicked = ImGuiAPI.IsItemDoubleClicked(ImGuiMouseButton_.ImGuiMouseButton_Left);
            var isTreeNodeIsItemClicked = ImGuiAPI.IsItemClicked(ImGuiMouseButton_.ImGuiMouseButton_Left);
            var keyName = $"Delete DesignableVariable {varibleName}?";
            if (EGui.UIProxy.CustomButton.ToolButton("x", in buttonSize, 0xFF0000FF, "DesignVar_X_" + varibleName))
            {
                EGui.UIProxy.MessageBox.Open(keyName);
            }
            bool bIsRemoved = false;
            EGui.UIProxy.MessageBox.Draw(keyName, $"Are you sure to delete {varibleName}?", EGui.UIProxy.MessageBox.EButtonType.YesNo,
            () =>
            {
                bIsRemoved = true;
                if (stateMachineElement.Parent is TtOutlineElement_List parent)
                {
                    parent.DescriptionsList.Remove(stateMachineElement.Description);
                }
            }, null);
            if (bIsRemoved)
            {
                if (stateMachineElement.Parent is TtOutlineElement_List parent)
                {
                    var desc = stateMachineElement.Description as IDescription;
                    context.CommandHistory.CreateAndExtuteCommand("RemoveStateMachine",
                       (data) => { parent.DescriptionsList.Remove(desc); ; },
                       (data) => { parent.DescriptionsList.Add(desc); });

                }
            }
            if (ImGuiAPI.BeginPopup("AnimStateMachineVariablesSelectPopup", ImGuiWindowFlags_.ImGuiWindowFlags_None))
            {
                var menuItemName = "AnimCompoundState";
                var drawList = ImGuiAPI.GetWindowDrawList();
                var menuData = new Support.TtAnyPointer();
                EGui.UIProxy.MenuItemProxy.MenuState newMethodMenuState = new EGui.UIProxy.MenuItemProxy.MenuState();
                newMethodMenuState.Reset();
                if (EGui.UIProxy.MenuItemProxy.MenuItem("New" + menuItemName, null, false, null, in drawList, in menuData, ref newMethodMenuState))
                {
                    var smDesc = stateMachineElement.Description as TtAnimStateMachineClassDescription;
                    var num = 0;
                    while (true)
                    {
                        var result = smDesc.CompoundStates.Find(child => child.Name == $"{menuItemName}_{num}");
                        if (result == null)
                        {
                            break;
                        }
                        num++;
                    }

                    var compoundStateDesc = new TtAnimCompoundStateClassDescription();
                    compoundStateDesc.Name = $"{menuItemName}_{num}";
                    context.CommandHistory.CreateAndExtuteCommand("AddAnimCompoundState",
                        (data) => { smDesc.AddCompoundState(compoundStateDesc); },
                        (data) => { smDesc.RemoveCompoundState(compoundStateDesc); });
                }
                ImGuiAPI.EndPopup();
            }
            if (treeNodeIsOpen)
            {
                if (isTreeNodeDoubleClicked)
                {
                    
                }
                else if (isTreeNodeIsItemClicked)
                {
                    stateMachineElement.OnSelected(ref context);
                }
                var elements = stateMachineElement.ConstructChildrenElements();
                foreach (var element in elements)
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
