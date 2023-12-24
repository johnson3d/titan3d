using EngineNS.Bricks.CodeBuilder;
using EngineNS.Rtti;
using EngineNS.Thread.Async;
using EngineNS.UI.Controls;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using static EngineNS.Bricks.CodeBuilder.MacrossNode.UMacrossEditor;

namespace EngineNS.UI.Editor
{
    public partial class TtUIEditor
    {
        Macross.UMacrossGetter<TtUIMacrossBase> mMacrossGetter;

        async System.Threading.Tasks.Task InitMacrossEditor()
        {
            await UIAsset.MacrossEditor.Initialize();
            UIAsset.MacrossEditor.FormName = UIAsset.AssetName.Name;
            UIAsset.MacrossEditor.RootForm = this;
            UIAsset.MacrossEditor.LoadClassGraph(AssetName);
            UIAsset.MacrossEditor.DrawToolbarAction = DrawMacrossToolbar;

            //mMacrossGetter
        }
        STToolButtonData[] mToolBtnDatas = new STToolButtonData[7];
        void DrawMacrossToolbar(ImDrawList drawList)
        {
            int toolBarItemIdx = 0;
            var spacing = EGui.UIProxy.StyleConfig.Instance.ToolbarSeparatorThickness + EGui.UIProxy.StyleConfig.Instance.ItemSpacing.X * 2;
            EGui.UIProxy.Toolbar.BeginToolbar(in drawList);

            if(EGui.UIProxy.ToolbarIconButtonProxy.DrawButton(in drawList,
                ref mToolBtnDatas[toolBarItemIdx].IsMouseDown, ref mToolBtnDatas[toolBarItemIdx].IsMouseHover, null, "Show Designer "))
            {
                DrawType = enDrawType.Designer;
            }
            EGui.UIProxy.ToolbarSeparator.DrawSeparator(in drawList, in Support.UAnyPointer.Default);
            toolBarItemIdx++;
            if (EGui.UIProxy.ToolbarIconButtonProxy.DrawButton(in drawList,
                ref mToolBtnDatas[toolBarItemIdx].IsMouseDown, ref mToolBtnDatas[toolBarItemIdx].IsMouseHover, null, "Save"))
            {
                Save();
            }
            toolBarItemIdx++;
            EGui.UIProxy.ToolbarSeparator.DrawSeparator(in drawList, in Support.UAnyPointer.Default);
            if (EGui.UIProxy.ToolbarIconButtonProxy.DrawButton(in drawList,
                ref mToolBtnDatas[toolBarItemIdx].IsMouseDown, ref mToolBtnDatas[toolBarItemIdx].IsMouseHover, null, "GenCode", false, -1, 0, spacing))
            {
                UIAsset.MacrossEditor.GenerateCode();
                UIAsset.MacrossEditor.CompileCode();
            }

            toolBarItemIdx++;
            EGui.UIProxy.ToolbarSeparator.DrawSeparator(in drawList, in Support.UAnyPointer.Default);
            if (Macross.UMacrossDebugger.Instance.CurrrentBreak != null)
            {
                if (EGui.UIProxy.ToolbarIconButtonProxy.DrawButton(in drawList,
                    ref mToolBtnDatas[toolBarItemIdx].IsMouseDown, ref mToolBtnDatas[toolBarItemIdx].IsMouseHover, null, "Run", false, -1, 0, spacing))
                {
                    Macross.UMacrossDebugger.Instance.Run();
                }
            }

            EGui.UIProxy.Toolbar.EndToolbar();
        }
        public void OnDrawMacrossWindow()
        {
            ImGuiAPI.SetNextWindowDockID(DockId, DockCond);
            UIAsset.MacrossEditor.OnDraw();
        }
        public void AddEventMethod(Controls.TtUIElement element, string name, UTypeDesc eventType)
        {
            var elementName = GetValidName(element);
            if (elementName != element.Name)
            {
                element.Name = elementName;
            }
            var methodName = element.GetEventMethodName(name);
            var methodDesc = new UMethodDeclaration();
            methodDesc.GetDisplayNameFunc = element.GetEventMethodDisplayName;
            methodDesc.MethodName = methodName;
            var pams = eventType.GetMethod("Invoke").GetParameters();
            for(int i=0; i<pams.Length; i++)
            {
                methodDesc.Arguments.Add(new UMethodArgumentDeclaration()
                {
                    VariableName = pams[i].Name,
                    VariableType = new UTypeReference(pams[i].ParameterType),
                });
            }
            UIAsset.MacrossEditor.AddMethod(methodDesc);

            var initEvtMethod = UIAsset.MacrossEditor.DefClass.FindMethod("InitializeEvents");
            if(initEvtMethod == null)
            {
                initEvtMethod = new UMethodDeclaration()
                {
                    MethodName = "InitializeEvents",
                    IsOverride = true,
                };
                UIAsset.MacrossEditor.DefClass.AddMethod(initEvtMethod);
            }
            var varName = $"var_{element.GetType().Name}_{element.Id}";
            var findElementInvokeStatement = new UMethodInvokeStatement(
                    "FindElement",
                    new UVariableDeclaration()
                    {
                        VariableName = varName,
                        VariableType = new UTypeReference(element.GetType()),
                    },
                    new UVariableReferenceExpression("HostElement"),
                    new UMethodInvokeArgumentExpression(new UPrimitiveExpression(element.Id)))
                    {
                        ForceCastReturnType = true,
                    };
            if(initEvtMethod.MethodBody.FindStatement(findElementInvokeStatement) == null)
                initEvtMethod.MethodBody.Sequence.Add(findElementInvokeStatement);
            var ifStatement = new UIfStatement()
            {
                Condition = new UBinaryOperatorExpression()
                {
                    Left = new UVariableReferenceExpression(varName),
                    Right = new UNullValueExpression(),
                    Operation = UBinaryOperatorExpression.EBinaryOperation.NotEquality,
                },
                TrueStatement = new UExecuteSequenceStatement(
                    new UExpressionStatement(
                        new UBinaryOperatorExpression()
                        {
                            Operation = UBinaryOperatorExpression.EBinaryOperation.SubtractAssignment,
                            Left = new UVariableReferenceExpression()
                            {
                                Host = new UVariableReferenceExpression(varName),
                                VariableName = name,
                            },
                            Right = new UVariableReferenceExpression(methodName),
                        }),
                    new UExpressionStatement(
                        new UBinaryOperatorExpression()
                        {
                            Operation = UBinaryOperatorExpression.EBinaryOperation.AddAssignment,
                            Left = new UVariableReferenceExpression()
                            {
                                Host = new UVariableReferenceExpression(varName),
                                VariableName = name,
                            },
                            Right = new UVariableReferenceExpression(methodName),
                        })),
                };
            var tagIfStatement = initEvtMethod.MethodBody.FindStatement(ifStatement) as UIfStatement;
            if (tagIfStatement == null)
                initEvtMethod.MethodBody.Sequence.Add(ifStatement);
            else
            {
                var seqStatements = tagIfStatement.TrueStatement as UExecuteSequenceStatement;
                seqStatements.Sequence.Add(new UExpressionStatement(
                        new UBinaryOperatorExpression()
                        {
                            Operation = UBinaryOperatorExpression.EBinaryOperation.SubtractAssignment,
                            Left = new UVariableReferenceExpression()
                            {
                                Host = new UVariableReferenceExpression(varName),
                                VariableName = name,
                            },
                            Right = new UVariableReferenceExpression(methodName),
                        }));
                seqStatements.Sequence.Add(new UExpressionStatement(
                        new UBinaryOperatorExpression()
                        {
                            Operation = UBinaryOperatorExpression.EBinaryOperation.AddAssignment,
                            Left = new UVariableReferenceExpression()
                            {
                                Host = new UVariableReferenceExpression(varName),
                                VariableName = name,
                            },
                            Right = new UVariableReferenceExpression(methodName),
                        }));
            }
        }
    }
}
