using EngineNS.Bricks.CodeBuilder;
using EngineNS.Bricks.CodeBuilder.MacrossNode;
using EngineNS.Bricks.NodeGraph;
using EngineNS.DesignMacross.Description;
using EngineNS.DesignMacross.Editor.DeclarationPanel;
using EngineNS.DesignMacross.Graph;
using EngineNS.DesignMacross.Outline;
using EngineNS.DesignMacross.Render;
using EngineNS.DesignMacross.TimedStateMachine;
using EngineNS.Rtti;
using System;
using System.Collections.Generic;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace EngineNS.DesignMacross.Editor
{

    public class TtDeclarationEditPanel
    {
        public TtClassDescription ClassDesc { get; set; } = new TtClassDescription();
        public TtOutline Outline { get; set; } = new TtOutline();
        public List<TtVariableOutlinerElement> Variables { get; set; } = new List<TtVariableOutlinerElement>();
        public List<TtMethodOutlinerElement> Methods { get; set; } = new List<TtMethodOutlinerElement>();
        public List<IDesignableVariableOutlineElement> DesignableVariables { get; set; } = new List<IDesignableVariableOutlineElement>();

        public void Initialize()
        {
            Outline.Description = ClassDesc;
            Outline.Construct();
        }
        public void Draw(FDesignMacrossEditorRenderingContext context)
        {
            TtDeclarationPanelRender render = new TtDeclarationPanelRender();
            render.Draw(this, context);
        }
       
    }

    public class TtDeclarationPanelRender
    {
        public void Draw(TtDeclarationEditPanel classDeclarationPanel, FDesignMacrossEditorRenderingContext context)
        {
            //DrawVariables(classDeclarationPanel, context);
            // DrawMethods(classDeclarationPanel, context);
            DrawDesignableVars(classDeclarationPanel, context);
        }

        void DrawVariables(TtDeclarationEditPanel classDeclarationPanel, FDesignMacrossEditorRenderingContext context)
        {
            //ImGuiTreeNodeFlags_ flags = ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_Leaf | ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_NoTreePushOnOpen | ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_Bullet | ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_SpanFullWidth;// | ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_AllowItemOverlap;
            //Vector2 buttonSize = new Vector2(16, 16);
            //float buttonOffset = 16;
            //var sz = new Vector2(-1, 0);
            //var regionSize = ImGuiAPI.GetContentRegionAvail();

            //var variablesTreeNodeResult = ImGuiAPI.TreeNodeEx("Variables", ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_AllowItemOverlap | ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_DefaultOpen);
            //ImGuiAPI.SameLine(regionSize.X - buttonSize.X - buttonOffset, -1.0f);
            //if (EGui.UIProxy.CustomButton.ToolButton("+", in buttonSize, 0xFF00FF00))
            //{
            //    ImGuiAPI.OpenPopup("MacrossMemTypeSelPopup", ImGuiPopupFlags_.ImGuiPopupFlags_None);
            //    Type selectedType = typeof(int);
            //    if (selectedType != null)
            //    {
            //        var num = 0;
            //        while (true)
            //        {
            //            bool bFind = false;
            //            for (int i = 0; i < classDeclarationPanel.Variables.Count; i++)
            //            {
            //                if (classDeclarationPanel.Variables[i].VariableName == $"Member_{num}")
            //                {
            //                    num++;
            //                    bFind = true;
            //                    break;
            //                }
            //            }
            //            if (!bFind)
            //                break;
            //        }

            //        var mb = new UVariableDeclaration();
            //        mb.VariableType = new UTypeReference(selectedType);
            //        mb.VariableName = $"Member_{num}";
            //        mb.VisitMode = EVisisMode.Local;
            //        mb.InitValue = new UPrimitiveExpression(Rtti.UTypeDesc.TypeOf(selectedType), selectedType.IsValueType ? Rtti.UTypeDescManager.CreateInstance(selectedType) : null);
            //        mb.Comment = new UCommentStatement("");
            //        classDeclarationPanel.Variables.Add(mb);
            //    }
            //}
            //if (variablesTreeNodeResult)
            //{
            //    MemberVar DraggingMember = null;
            //    bool IsDraggingMember = false;
            //    if (DraggingMember != null && IsDraggingMember == false && ImGuiAPI.IsMouseDragging(ImGuiMouseButton_.ImGuiMouseButton_Left, 10))
            //    {
            //        IsDraggingMember = true;
            //    }
            //    var memRegionSize = ImGuiAPI.GetContentRegionAvail();
            //    for (int i = 0; i < classDeclarationPanel.Variables.Count; i++)
            //    {
            //        var mem = classDeclarationPanel.Variables[i];
            //        var memberTreeNodeResult = ImGuiAPI.TreeNodeEx(mem.VariableName, flags);
            //        var memberTreeNodeClicked = ImGuiAPI.IsItemClicked(ImGuiMouseButton_.ImGuiMouseButton_Left);
            //        ImGuiAPI.SameLine(regionSize.X - buttonSize.X - buttonOffset, -1.0f);
            //        if (EGui.UIProxy.CustomButton.ToolButton("x", in buttonSize, 0xFF0000FF, "mem_X_" + i))
            //        {
            //            // todo: 引用删除警告
            //            classDeclarationPanel.Variables.Remove(mem);
            //            break;
            //        }
            //        if (memberTreeNodeResult)
            //        {
            //            if (memberTreeNodeClicked)
            //            {
            //                //PGMember.Target = mem;
            //                //DraggingMember = MemberVar.NewMemberVar(classDeclared, mem.VariableName,
            //                //    UEngine.Instance.InputSystem.IsKeyDown(Bricks.Input.Keycode.KEY_LCTRL) ? false : true);
            //                //DraggingMember.UserData = this;
            //                //IsDraggingMember = false;
            //            }
            //        }
            //    }
            //    ImGuiAPI.TreePop();
            //}
        }
        void DrawMethods(TtDeclarationEditPanel classDeclarationPanel, FDesignMacrossEditorRenderingContext context)
        {
            //ImGuiTreeNodeFlags_ flags = ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_Leaf | ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_NoTreePushOnOpen | ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_Bullet | ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_SpanFullWidth;// | ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_AllowItemOverlap;
            //Vector2 buttonSize = new Vector2(16, 16);
            //float buttonOffset = 16;
            //var sz = new Vector2(-1, 0);
            //var regionSize = ImGuiAPI.GetContentRegionAvail();

            //var methodsTreeNodeResult = ImGuiAPI.TreeNodeEx("Methods", ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_AllowItemOverlap | ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_DefaultOpen);
            //ImGuiAPI.SameLine(regionSize.X - buttonSize.X - buttonOffset, -1.0f);
            //if (EGui.UIProxy.CustomButton.ToolButton("+", in buttonSize, 0xFF00FF00))
            //{
            //    ImGuiAPI.OpenPopup("MacrossMethodSelectPopup", ImGuiPopupFlags_.ImGuiPopupFlags_None);
            //}
            //if (ImGuiAPI.BeginPopup("MacrossMethodSelectPopup", ImGuiWindowFlags_.ImGuiWindowFlags_None))
            //{
            //    var drawList = ImGuiAPI.GetWindowDrawList();
            //    var menuData = new Support.UAnyPointer();
            //    EGui.UIProxy.MenuItemProxy.MenuState newMethodMenuState = new EGui.UIProxy.MenuItemProxy.MenuState();
            //    if (EGui.UIProxy.MenuItemProxy.MenuItem("New Method", null, false, null, in drawList, in menuData, ref newMethodMenuState))
            //    {
            //        var num = 0;
            //        while (true)
            //        {
            //            bool bFind = false;
            //            for (int i = 0; i < classDeclarationPanel.Methods.Count; i++)
            //            {
            //                if (classDeclarationPanel.Methods[i].MethodName == $"Method_{num}")
            //                {
            //                    num++;
            //                    bFind = true;
            //                    break;
            //                }
            //            }
            //            if (!bFind)
            //                break;
            //        }

            //        var f = new UMethodDeclaration()
            //        {
            //            MethodName = $"Method_{num}",
            //        };
            //        classDeclarationPanel.Methods.Add(f);

            //        //    var func = UMacrossMethodGraph.NewGraph(this, f);
            //        //    Methods.Add(func);
            //        //    for (int i = 0; i < OpenFunctions.Count; i++)
            //        //    {
            //        //        OpenFunctions[i].CanvasMenuDirty = true;
            //        //    }
            //        //}
            //        //if (EGui.UIProxy.MenuItemProxy.BeginMenuItem("Override Method", null, null, in drawList, in menuData, ref mOverrideMenuState))
            //        //{
            //        //    for (int i = 0; i < mOverrideMethodMenuItems.Count; i++)
            //        //    {
            //        //        mOverrideMethodMenuItems[i].OnDraw(in drawList, in menuData);
            //        //    }
            //        //    EGui.UIProxy.MenuItemProxy.EndMenuItem();
            //        //}

            //    }
            //    ImGuiAPI.EndPopup();
            //}
            //if (methodsTreeNodeResult)
            //{
            //    //var Methods = classDeclarationPanel.MethodGraphs;
            //    //var funcRegionSize = ImGuiAPI.GetContentRegionAvail();
            //    //for (int i = Methods.Count - 1; i >= 0; i--)
            //    //{
            //    //    var method = Methods[i];
            //    //    if (method.IsDelegateGraph())
            //    //        continue;
            //    //    var methodTreeNodeResult = ImGuiAPI.TreeNodeEx(method.Name, flags);
            //    //    ImGuiAPI.SameLine(0, EGui.UIProxy.StyleConfig.Instance.ItemSpacing.X);
            //    //    var methodTreeNodeDoubleClicked = ImGuiAPI.IsItemDoubleClicked(ImGuiMouseButton_.ImGuiMouseButton_Left);
            //    //    var methodTreeNodeIsItemClicked = ImGuiAPI.IsItemClicked(ImGuiMouseButton_.ImGuiMouseButton_Left);
            //    //    ImGuiAPI.SameLine(regionSize.X - buttonSize.X - buttonOffset, -1.0f);
            //    //    var keyName = $"Delete func {method.Name}?";
            //    //    if (EGui.UIProxy.CustomButton.ToolButton("x", in buttonSize, 0xFF0000FF, "func_X_" + i))
            //    //    {
            //    //        EGui.UIProxy.MessageBox.Open(keyName);
            //    //        break;
            //    //    }
            //    //    EGui.UIProxy.MessageBox.Draw(keyName, $"Are you sure to delete {method.Name}?", EGui.UIProxy.MessageBox.EButtonType.YesNo,
            //    //    () =>
            //    //    {
            //    //        //RemoveMethod(method);
            //    //    }, null);

            //    //    if (methodTreeNodeResult)
            //    //    {
            //    //        if (methodTreeNodeDoubleClicked)
            //    //        {
            //    //            //用 context 来获取GraphWindow来渲染Graph
            //    //            //mSettingCurrentFuncIndex = OpenFunctions.IndexOf(method);
            //    //            //if (mSettingCurrentFuncIndex < 0)
            //    //            //{
            //    //            //    method.VisibleInClassGraphTables = true;
            //    //            //    method.GraphRenderer.SetGraph(method);
            //    //            //    mSettingCurrentFuncIndex = OpenFunctions.Count;
            //    //            //    OpenFunctions.Add(method);
            //    //            //}
            //    //        }
            //    //        else if (methodTreeNodeIsItemClicked)
            //    //        {
            //    //            //用 context 来获取PGMember来渲染PG
            //    //            //PGMember.Target = method;
            //    //        }
            //    //    }
            //    //}
            //    ImGuiAPI.TreePop();
            //}
        }
      
        void DrawDesignableVars(TtDeclarationEditPanel classDeclarationPanel, FDesignMacrossEditorRenderingContext context)
        {
            var render = TtElementRenderDevice.CreateOutlineRender(classDeclarationPanel.Outline);
            var outlineContext = new FOutlineRenderingContext();
            outlineContext.CommandHistory = context.CommandHistory;
            outlineContext.EditorInteroperation = context.EditorInteroperation;
            render.Draw(classDeclarationPanel.Outline, ref outlineContext);
        }
    }
}
