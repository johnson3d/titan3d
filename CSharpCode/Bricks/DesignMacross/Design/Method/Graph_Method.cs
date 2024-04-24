using EngineNS.Bricks.CodeBuilder;
using EngineNS.Bricks.CodeBuilder.MacrossNode;
using EngineNS.Bricks.NodeGraph;
using EngineNS.DesignMacross.Base.Description;
using EngineNS.DesignMacross.Base.Graph;
using EngineNS.DesignMacross.Base.Render;
using EngineNS.DesignMacross.Design;
using EngineNS.EGui.Controls;
using EngineNS.EGui.Controls.PropertyGrid;
using NPOI.POIFS.Properties;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.DesignMacross.Design
{
    public class TtTransitionMacrossHolder : IMacrossMethodHolder
    {
        public MemberVar DraggingMember { get; set; }
        public bool IsDraggingMember { get; set; }
        public UClassDeclaration DefClass { get; set; }
        UCSharpCodeGenerator mCSCodeGen = new UCSharpCodeGenerator();
        public UCSharpCodeGenerator CSCodeGen => mCSCodeGen;
        List<UMacrossMethodGraph> mMethods = new List<UMacrossMethodGraph>();
        public List<UMacrossMethodGraph> Methods => mMethods;

        public PropertyGrid PGMember { get; set; }

        public void RemoveMethod(UMacrossMethodGraph method)
        {

        }

        public void SetConfigUnionNode(IUnionNode node)
        {

        }
    }

    public class TtDesignMacrossMethodGraph : UMacrossMethodGraph
    {
        public override void UpdateCanvasMenus()
        {
            base.UpdateCanvasMenus();
            //if(MethodDescription != null)
            //{
            //    var classDesc = MethodDescription.Parent as TtClassDescription;
            //    var selfMenu = GetMenu("Self");
            //    var macrossHolder = MacrossEditor as TtTransitionMacrossHolder;
            //    FClassBuildContext classBuildContext = new() { MainClassDescription = classDesc };
            //    var classDec = TtDescriptionASTBuildUtil.BuildDefaultPartForClassDeclaration( classDesc, ref classBuildContext);
            //    foreach (var variable in classDesc.Variables)
            //    {
            //        if(variable.VisitMode == EVisisMode.Public)
            //        {
            //            var menuPath = new string[] { variable.Name };
            //            selfMenu.AddMenuItem("Get " + variable.Name, variable.VariableName, null,
            //                    (UMenuItem item, object sender) =>
            //                    {
            //                        var node = MemberVar.NewMemberVar(classDec, variable.VariableName, true);
            //                        AddNode(node);
            //                    });
            //            selfMenu.AddMenuItem("Set " + variable.Name, variable.Name, null,
            //                    (UMenuItem item, object sender) =>
            //                    {
            //                        var node = MemberVar.NewMemberVar(classDec, variable.VariableName, false);
            //                        SetDefaultActionForNode(node);
            //                        AddNode(node);
            //                    });
            //        }
            //    }
            //}
        }
        TtMenuItem GetMenu(string menuName)
        {
            foreach(var sub in CanvasMenus.SubMenuItems)
            {
                if(sub.Text == menuName)
                {
                    return sub;
                }
            }
            return null;
        }
        public static TtDesignMacrossMethodGraph CreateGraph(IMacrossMethodHolder kls, UMethodDeclaration method = null)
        {
            var result = new TtDesignMacrossMethodGraph();
            result.MacrossEditor = kls;
            result.Initialize();
            //result.FunctionName = funName;
            //if (result.Function == null)
            //    return null;
            if (method != null)
            {
                var methodData = MethodData.CreateFromMethod(result, method);
                result.MethodDatas.Add(methodData);
                result.AddNode(methodData.StartNode);
                result.GraphName = method.MethodName;
            }
            return result;
        }
        public void SetMacrossEditor(IMacrossMethodHolder macrossEditor)
        {
            MacrossEditor = macrossEditor;
        }
    }

    [ImGuiElementRender(typeof(TtGraph_MethodRender))]
    public class TtGraph_Method : TtGraph, IContextMeunable
    {
        public TtMethodDescription MethodDescription { get => Description as TtMethodDescription; }
        int VariableCount = 0;
        int MethodCount = 0;
        public TtGraph_Method(IDescription description) : base(description)
        {

        }

        public override void ConstructElements(ref FGraphRenderingContext context)
        {
            var classDesc = MethodDescription.Parent as TtClassDescription;
            if (MethodDescription.MethodGraph == null)
            {
                var macrossHolder = new TtTransitionMacrossHolder();
                FClassBuildContext classBuildContext = new() { MainClassDescription = classDesc };
                var classDec = TtDescriptionASTBuildUtil.BuildDefaultPartForClassDeclaration(classDesc, ref classBuildContext);
                macrossHolder.DefClass = classDec;
                MethodDescription.MethodGraph = TtDesignMacrossMethodGraph.CreateGraph(macrossHolder, MethodDescription.BuildMethodDeclaration(ref classBuildContext));
                MethodDescription.MethodGraph.GraphName = MethodDescription.Name;
            }
            if (classDesc.Variables.Count != VariableCount)
            {
                VariableCount = classDesc.Variables.Count;
                MethodDescription.MethodGraph.CanvasMenuDirty = true;
            }
            if (classDesc.Methods.Count != MethodCount)
            {
                MethodCount = classDesc.Methods.Count;
                MethodDescription.MethodGraph.CanvasMenuDirty = true;
            }
            if (MethodDescription.MethodGraph.MacrossEditor == null)
            {
                var macrossHolder = new TtTransitionMacrossHolder();
                FClassBuildContext classBuildContext = new() { MainClassDescription = classDesc };
                var classDec = TtDescriptionASTBuildUtil.BuildDefaultPartForClassDeclaration(classDesc, ref classBuildContext);
                macrossHolder.DefClass = classDec;
                MethodDescription.MethodGraph.SetMacrossEditor(macrossHolder);
            }
        }
    }
    public class TtGraph_MethodRender : IGraphRender
    {
        UGraphRenderer mGraphRender;


        public void Draw(IRenderableElement renderableElement, ref FGraphRenderingContext context)
        {
            var graph = renderableElement as TtGraph_Method;
            if (graph == null)
                return;
            
            if (mGraphRender == null)
            {
                mGraphRender = new UGraphRenderer();
                mGraphRender.SetGraph(graph.MethodDescription.MethodGraph);
                mGraphRender.DrawInherit = false;
            }

            mGraphRender.OnDraw();
        }
    }
}
