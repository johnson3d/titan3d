using EngineNS.Bricks.CodeBuilder;
using EngineNS.Bricks.CodeBuilder.MacrossNode;
using EngineNS.Bricks.NodeGraph;
using EngineNS.DesignMacross.Base.Description;
using EngineNS.DesignMacross.Base.Graph;
using EngineNS.DesignMacross.Base.Render;
using EngineNS.DesignMacross.Design;
using EngineNS.EGui.Controls.PropertyGrid;
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
        public TtMethodDescription MethodDescription { get; set; } = null;
        public override void UpdateCanvasMenus()
        {
            base.UpdateCanvasMenus();
            if(MethodDescription != null)
            {
                var classDesc = MethodDescription.Parent as TtClassDescription;
                var selfMenu = GetMenu("Self");
                var macrossHolder = MacrossEditor as TtTransitionMacrossHolder;
                FClassBuildContext classBuildContext = new() { MainClassDescription = classDesc };
                var classDec = TtDescriptionASTBuildUtil.BuildDefaultPartForClassDeclaration( classDesc, ref classBuildContext);
                foreach (var variable in classDesc.Variables)
                {
                    if(variable.VisitMode == EVisisMode.Public)
                    {
                        var menuPath = new string[] { variable.Name };
                        selfMenu.AddMenuItem("Get " + variable.Name, variable.VariableName, null,
                                (UMenuItem item, object sender) =>
                                {
                                    var node = MemberVar.NewMemberVar(classDec, variable.VariableName, true);
                                    AddNode(node);
                                });
                        selfMenu.AddMenuItem("Set " + variable.Name, variable.Name, null,
                                (UMenuItem item, object sender) =>
                                {
                                    var node = MemberVar.NewMemberVar(classDec, variable.VariableName, false);
                                    SetDefaultActionForNode(node);
                                    AddNode(node);
                                });
                    }
                }
            }
        }
        UMenuItem GetMenu(string menuName)
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
    }


    [ImGuiElementRender(typeof(TtGraph_MethodRender))]
    public class TtGraph_Method : TtGraph, IContextMeunable
    {
        public TtMethodDescription MethodDescription { get => Description as TtMethodDescription; }
        TtDesignMacrossMethodGraph mGraph;
        public TtDesignMacrossMethodGraph Graph => mGraph;

        TtTransitionMacrossHolder mMacrossHolder;

        public TtGraph_Method(IDescription description) : base(description)
        {
            var classDesc = MethodDescription.Parent as TtClassDescription;
            mMacrossHolder = new TtTransitionMacrossHolder();
            FClassBuildContext classBuildContext = new() { MainClassDescription = classDesc };
            var classDec = TtDescriptionASTBuildUtil.BuildDefaultPartForClassDeclaration(classDesc, ref classBuildContext);
            mMacrossHolder.DefClass = classDec;
            mGraph = TtDesignMacrossMethodGraph.CreateGraph(mMacrossHolder, MethodDescription.BuildMethodDeclaration(ref classBuildContext));
            mGraph.MethodDescription = MethodDescription;
            mGraph.GraphName = description.Name;
        }

        public override void ConstructContextMenu(ref FGraphElementRenderingContext context, TtPopupMenu PopupMenu)
        {
            PopupMenu.Reset();
        }

        public override void ConstructElements(ref FGraphRenderingContext context)
        {
            
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

            if(mGraphRender == null)
            {
                mGraphRender = new UGraphRenderer();
                mGraphRender.SetGraph(graph.Graph);
                mGraphRender.DrawInherit = false;
            }

            mGraphRender.OnDraw();
        }
    }
}
