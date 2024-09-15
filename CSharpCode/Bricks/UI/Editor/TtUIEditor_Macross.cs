using EngineNS.Bricks.CodeBuilder;
using EngineNS.Bricks.CodeBuilder.MacrossNode;
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
        //Macross.UMacrossGetter<TtUIMacrossBase> mMacrossGetter;

        public struct MacrossEditorRemoveMethodQueryData
        {
            public TtMethodDeclaration Desc;
            public bool RemoveSuccess;
        };

        bool OnMacrossEditorRemoveMethod(Bricks.CodeBuilder.MacrossNode.UMacrossMethodGraph method)
        {
            MacrossEditorRemoveMethodQueryData data = new MacrossEditorRemoveMethodQueryData();
            data.RemoveSuccess = true;
            for (int methodIdx = 0; methodIdx < method.MethodDatas.Count; methodIdx++)
            {
                data.Desc = method.MethodDatas[methodIdx].MethodDec;
                mUIHost.QueryElements(ElementOnRemoveMacrossMethod, ref data);
            }

            return data.RemoveSuccess;
        }
        bool ElementOnRemoveMacrossMethod(TtUIElement element, ref MacrossEditorRemoveMethodQueryData data)
        {
            element.OnRemoveMacrossMethod(ref data);
            //List<string> needDeletes = new List<string>();
            //foreach(var method in element.MacrossMethods)
            //{
            //    if(method.Value is TtUIElement.MacrossEventMethodData)
            //    {
            //        // remove event bind method
            //        var evd = method.Value as TtUIElement.MacrossEventMethodData;
            //        if (evd.Desc.Equals(data.Desc))
            //        {
            //            needDeletes.Add(method.Key);
            //            element.MethodDisplayNames.Remove(data.Desc);
            //        }
            //    }
            //    else if(method.Value is TtUIElement.MacrossPropertyBindMethodData)
            //    {
            //        // remove property bind method
            //        var pvd = method.Value as TtUIElement.MacrossPropertyBindMethodData;
            //        if(pvd.SetDesc.Equals(data.Desc) ||
            //           pvd.GetDesc.Equals(data.Desc))
            //        {
            //            needDeletes.Add(method.Key);
            //            element.MethodDisplayNames.Remove(data.Desc);
            //            element.BindingDatas.Remove(method.Key);
            //        }
            //    }
            //}
            //for(int i=0; i<needDeletes.Count; i++)
            //{
            //    element.MacrossMethods.Remove(needDeletes[i]);
            //}
            return false;
        }
        bool OnMacrossEditorAddMember(Bricks.CodeBuilder.TtVariableDeclaration variable)
        {
            // 默认UI的class所有member增加bind attribute
            var bpaType = TtTypeDesc.TypeOf(typeof(UI.Bind.BindPropertyAttribute));
            //if (!variable.HasAttribute(bpaType))
            //{
            //    variable.Attributes.Add(new TtAttribute()
            //    {
            //        AttributeType = new UTypeReference(bpaType),
            //    });
            //}
            variable.IsBindable = true;

            return true;
        }
        bool OnMacrossEditorRemoveMember(Bricks.CodeBuilder.TtVariableDeclaration variable)
        {
            mUIHost.QueryElements(ElementOnRemoveMacrossMember, ref variable);
            return true;
        }
        List<string> mNeedDeletes = new List<string>();
        bool ElementOnRemoveMacrossMember(TtUIElement element, ref TtVariableDeclaration desc)
        {
            if(desc.VariableName == GetUIElementMacrossVariableName(element))
            {
                element.IsVariable = false;
                return true;
            }

            mNeedDeletes.Clear();
            foreach (var bindData in element.BindingDatas)
            {
                var sp = bindData.Value as TtUIElement.BindingData_SelfProperty;
                if(sp != null)
                {
                    if(sp.PropertyName == desc.VariableName)
                        mNeedDeletes.Add(bindData.Key);
                }
            }
            for(int i=0; i<mNeedDeletes.Count; i++)
            {
                element.BindingDatas.Remove(mNeedDeletes[i]);
            }

            return false;
        }
        async System.Threading.Tasks.Task InitMacrossEditor()
        {
            await UIAsset.MacrossEditor.Initialize();
            UIAsset.MacrossEditor.FormName = UIAsset.AssetName.Name;
            UIAsset.MacrossEditor.RootForm = this;
            UIAsset.MacrossEditor.LoadClassGraph(AssetName);
            UIAsset.MacrossEditor.DrawToolbarAction = DrawMacrossToolbar;
            UIAsset.MacrossEditor.OnRemoveMethod = OnMacrossEditorRemoveMethod;
            UIAsset.MacrossEditor.OnAddMember = OnMacrossEditorAddMember;
            UIAsset.MacrossEditor.OnRemoveMember = OnMacrossEditorRemoveMember;
            UIAsset.MacrossEditor.BeforeGenerateCode = OnBeforeGenerateCode;
            UIAsset.MacrossEditor.AfterCompileCode = OnAfterCompileCode;

            //mMacrossGetter
            int temp = 0;
            this.mUIHost.QueryElements(ElementBindMacross, ref temp);
        }
        bool ElementBindMacross(TtUIElement element, ref int temp)
        {
            //List<string> needDeletes = new List<string>();
            //foreach(var evt in element.MacrossMethods)
            //{
            //    if(evt.Value is TtUIElement.MacrossEventMethodData)
            //    {
            //        var data = evt.Value as TtUIElement.MacrossEventMethodData;
            //        var eventName = data.EventName;
            //        var methodDesc = UIAsset.MacrossEditor.DefClass.FindMethod(element.GetEventMethodName(eventName));
            //        if(methodDesc == null)
            //        {
            //            // method已删除或不存在
            //            needDeletes.Add(evt.Key);
            //        }
            //        else
            //        {
            //            data.Desc = methodDesc;
            //            methodDesc.GetDisplayNameFunc = element.GetMethodDisplayName;
            //            element.mMethodDisplayNames[methodDesc] = data;
            //        }
            //    }
            //    else if(evt.Value is TtUIElement.MacrossPropertyBindMethodData)
            //    {
            //        var data = evt.Value as TtUIElement.MacrossPropertyBindMethodData;
            //        var propertyName = data.PropertyName;
            //        var setMethodDesc = UIAsset.MacrossEditor.DefClass.FindMethod(element.GetPropertyBindMethodName(propertyName, true));
            //        var getMethodDesc = UIAsset.MacrossEditor.DefClass.FindMethod(element.GetPropertyBindMethodName(propertyName, false));
            //        if(setMethodDesc == null && getMethodDesc == null)
            //        {
            //            // method已删除或不存在
            //            needDeletes.Add(evt.Key);
            //        }
            //        else
            //        {
            //            if(getMethodDesc != null)
            //            {
            //                data.GetDesc = getMethodDesc;
            //                getMethodDesc.GetDisplayNameFunc = element.GetMethodDisplayName;
            //                element.mMethodDisplayNames[getMethodDesc] = data;
            //            }
            //            if(setMethodDesc != null)
            //            {
            //                data.SetDesc = setMethodDesc;
            //                setMethodDesc.GetDisplayNameFunc = element.GetMethodDisplayName;
            //                element.mMethodDisplayNames[setMethodDesc] = data;
            //            }
            //        }
            //    }
            //}

            //for(int i = 0; i<needDeletes.Count; i++)
            //{
            //    element.MacrossMethods.Remove(needDeletes[i]);
            //}
            element.BindMacross(ref temp, UIAsset.MacrossEditor.DefClass);

            if (element.IsVariable)
            {
                var variable = UIAsset.MacrossEditor.DefClass.FindMember(GetUIElementMacrossVariableName(element));
                if(variable != null)
                {
                    variable.GetDisplayNameFunc = element.GetVariableDisplayName;
                }
            }

            return false;
        }
        STToolButtonData[] mToolBtnDatas = new STToolButtonData[7];
        void DrawMacrossToolbar(ImDrawList drawList)
        {
            int toolBarItemIdx = 0;
            var spacing = EGui.UIProxy.StyleConfig.Instance.ToolbarSeparatorThickness + EGui.UIProxy.StyleConfig.Instance.ItemSpacing.X * 2;
            EGui.UIProxy.Toolbar.BeginToolbar(in drawList);

            //if(EGui.UIProxy.ToolbarIconButtonProxy.DrawButton(in drawList,
            //    ref mToolBtnDatas[toolBarItemIdx].IsMouseDown, ref mToolBtnDatas[toolBarItemIdx].IsMouseHover, null, "Show Designer "))
            //{
            //    DrawType = enDrawType.Designer;
            //}
            if (EGui.UIProxy.CustomButton.ToolButton("Show Designer", in Vector2.Zero,
                EGui.UIProxy.StyleConfig.Instance.ToolButtonTextColor,
                EGui.UIProxy.StyleConfig.Instance.ToolButtonTextColor_Press,
                EGui.UIProxy.StyleConfig.Instance.ToolButtonTextColor_Hover,
                EGui.UIProxy.StyleConfig.Instance.PGCreateButtonBGColor,
                EGui.UIProxy.StyleConfig.Instance.PGCreateButtonBGActiveColor,
                EGui.UIProxy.StyleConfig.Instance.PGCreateButtonBGHoverColor
                ))
            {
                DrawType = enDrawType.Designer;
            }
            EGui.UIProxy.ToolbarSeparator.DrawSeparator(in drawList, in Support.TtAnyPointer.Default);
            toolBarItemIdx++;
            if (EGui.UIProxy.ToolbarIconButtonProxy.DrawButton(in drawList,
                ref mToolBtnDatas[toolBarItemIdx].IsMouseDown, ref mToolBtnDatas[toolBarItemIdx].IsMouseHover, null, "Save"))
            {
                Save();
            }
            toolBarItemIdx++;
            EGui.UIProxy.ToolbarSeparator.DrawSeparator(in drawList, in Support.TtAnyPointer.Default);
            if (EGui.UIProxy.ToolbarIconButtonProxy.DrawButton(in drawList,
                ref mToolBtnDatas[toolBarItemIdx].IsMouseDown, ref mToolBtnDatas[toolBarItemIdx].IsMouseHover, null, "GenCode", false, -1, 0, spacing))
            {
                UIAsset.MacrossEditor.GenerateCode();
                UIAsset.MacrossEditor.CompileCode();
            }

            toolBarItemIdx++;
            EGui.UIProxy.ToolbarSeparator.DrawSeparator(in drawList, in Support.TtAnyPointer.Default);
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
        public void AddEventMethod(Controls.TtUIElement element, string name, TtTypeDesc eventType)
        {
            var elementName = GetValidName(element);
            if (elementName != element.Name)
            {
                element.Name = elementName;
            }
            var methodName = element.GetEventMethodName(name);
            var methodDesc = new TtMethodDeclaration();
            methodDesc.GetDisplayNameFunc = element.GetMethodDisplayName;
            methodDesc.MethodName = methodName;
            var pams = eventType.GetMethod("Invoke").GetParameters();
            for(int i=0; i<pams.Length; i++)
            {
                methodDesc.Arguments.Add(new TtMethodArgumentDeclaration()
                {
                    VariableName = pams[i].Name,
                    VariableType = new TtTypeReference(pams[i].ParameterType),
                });
            }
            var graph = UIAsset.MacrossEditor.AddMethod(methodDesc);
            element.SetEventBindMethod(name, methodDesc);

            UIAsset.MacrossEditor.OpenMethodGraph(graph);
            DrawType = enDrawType.Macross;
        }

        public void JumpToEventMethod(string methodDisplayName)
        {
            for(int i=0; i<UIAsset.MacrossEditor.Methods.Count; i++)
            {
                if(UIAsset.MacrossEditor.Methods[i].DisplayName == methodDisplayName)
                {
                    UIAsset.MacrossEditor.OpenMethodGraph(UIAsset.MacrossEditor.Methods[i]);
                    break;
                }
            }

            DrawType = enDrawType.Macross;
        }
        string GetUIElementMacrossVariableName(TtUIElement element)
        {
            if (element == null)
                return null;
            return "ElementVar_" + element.Id;
        }
        void GenericEventBindMethodCode(TtUIElement.MacrossEventMethodData data, TtUIElement element, ref TtClassDeclaration cls)
        {
            var methodName = element.GetEventMethodName(data.EventName);
            var initEvtMethod = UIAsset.MacrossEditor.DefClass.FindMethod("InitializeEvents");
            var varName = $"var_{element.GetType().Name}_{element.Id}";
            var findElementInvokeStatement = new TtMethodInvokeStatement(
                    "FindElement",
                    new TtVariableDeclaration()
                    {
                        VariableName = varName,
                        VariableType = new TtTypeReference(element.GetType()),
                    },
                    new TtVariableReferenceExpression("HostObject"),
                    new TtMethodInvokeArgumentExpression(new TtPrimitiveExpression(element.Id)))
            {
                DeclarationReturnValue = true,
                ForceCastReturnType = true,
            };
            if (initEvtMethod.MethodBody.FindStatement(findElementInvokeStatement) == null)
                initEvtMethod.MethodBody.Sequence.Add(findElementInvokeStatement);
            TtIfStatement ifStatement = null;
            for (int i = 0; i < initEvtMethod.MethodBody.Sequence.Count; i++)
            {
                var seq = initEvtMethod.MethodBody.Sequence[i];
                var ifSt = seq as TtIfStatement;
                if (ifSt != null)
                {
                    var cond = ifSt.Condition as TtBinaryOperatorExpression;
                    if (cond != null)
                    {
                        var varRef = cond.Left as TtVariableReferenceExpression;
                        if (varRef != null)
                        {
                            if (varRef.VariableName == varName && cond.Operation == TtBinaryOperatorExpression.EBinaryOperation.NotEquality)
                            {
                                ifStatement = ifSt;
                                break;
                            }
                        }
                    }
                }
            }
            if (ifStatement == null)
            {
                ifStatement = new TtIfStatement()
                {
                    Condition = new TtBinaryOperatorExpression()
                    {
                        Left = new TtVariableReferenceExpression(varName),
                        Right = new TtNullValueExpression(),
                        Operation = TtBinaryOperatorExpression.EBinaryOperation.NotEquality,
                    },
                    TrueStatement = new TtExecuteSequenceStatement(),
                };
                initEvtMethod.MethodBody.Sequence.Add(ifStatement);
            }
            var seqStatements = ifStatement.TrueStatement as TtExecuteSequenceStatement;
            var subAssigStatement = new TtExpressionStatement(
                    new TtBinaryOperatorExpression()
                    {
                        Operation = TtBinaryOperatorExpression.EBinaryOperation.SubtractAssignment,
                        Left = new TtVariableReferenceExpression()
                        {
                            Host = new TtVariableReferenceExpression(varName),
                            VariableName = data.EventName,
                        },
                        Right = new TtVariableReferenceExpression(methodName),
                        Cell = false,
                    });
            var addAssigStatement = new TtExpressionStatement(
                    new TtBinaryOperatorExpression()
                    {
                        Operation = TtBinaryOperatorExpression.EBinaryOperation.AddAssignment,
                        Left = new TtVariableReferenceExpression()
                        {
                            Host = new TtVariableReferenceExpression(varName),
                            VariableName = data.EventName,
                        },
                        Right = new TtVariableReferenceExpression(methodName),
                        Cell = false,
                    });
            if (seqStatements.FindStatement(subAssigStatement) == null)
                seqStatements.Sequence.Add(subAssigStatement);
            if (seqStatements.FindStatement(addAssigStatement) == null)
                seqStatements.Sequence.Add(addAssigStatement);
        }

        bool GenericElementVariableCode(TtUIElement element, ref TtClassDeclaration cls)
        {
            foreach(var data in element.MacrossMethods)
            {
                if(data.Value is TtUIElement.MacrossEventMethodData)
                {
                    var bindData = data.Value as TtUIElement.MacrossEventMethodData;
                    GenericEventBindMethodCode(bindData, element, ref cls);
                }
                else if(data.Value is TtUIElement.MacrossPropertyBindMethodData)
                {
                    // 这里不需要生成
                }
            }

            var bindInitMethod = UIAsset.MacrossEditor.DefClass.FindMethod("InitializeBindings");
            element.GenerateBindingDataStatement(this, bindInitMethod.MethodBody.Sequence);
            //foreach(var data in element.BindingDatas)
            //{
            //    data.Value.GenerateStatement(this, bindInitMethod.MethodBody.Sequence);
            //}
            if (element.IsVariable)
            {
                var initMethod = UIAsset.MacrossEditor.DefClass.FindMethod("InitializeUIElementVariables");
                var findElementInvokeStatement = new TtMethodInvokeStatement(
                        "FindElement",
                        new TtVariableDeclaration()
                        {
                            VariableName = GetUIElementMacrossVariableName(element),
                            VariableType = new TtTypeReference(element.GetType()),
                        },
                        new TtVariableReferenceExpression("HostObject"),
                        new TtMethodInvokeArgumentExpression(new TtPrimitiveExpression(element.Id)))
                {
                    DeclarationReturnValue = false,
                    ForceCastReturnType = true,
                };
                initMethod.MethodBody.Sequence.Add(findElementInvokeStatement);
            }
            return false;
        }
        void OnBeforeGenerateCode(TtClassDeclaration cls)
        {
            var initEvtMethod = UIAsset.MacrossEditor.DefClass.FindMethod("InitializeEvents");
            if (initEvtMethod == null)
            {
                initEvtMethod = new TtMethodDeclaration()
                {
                    MethodName = "InitializeEvents",
                    IsOverride = true,
                };
                UIAsset.MacrossEditor.DefClass.AddMethod(initEvtMethod);
            }
            initEvtMethod.MethodBody.Sequence.Clear();

            var bindInitMethod = UIAsset.MacrossEditor.DefClass.FindMethod("InitializeBindings");
            if (bindInitMethod == null)
            {
                bindInitMethod = new TtMethodDeclaration()
                {
                    MethodName = "InitializeBindings",
                    IsOverride = true,
                };
                UIAsset.MacrossEditor.DefClass.AddMethod(bindInitMethod);
            }
            bindInitMethod.MethodBody.Sequence.Clear();

            var initMethod = UIAsset.MacrossEditor.DefClass.FindMethod("InitializeUIElementVariables");
            if (initMethod == null)
            {
                initMethod = new TtMethodDeclaration()
                {
                    MethodName = "InitializeUIElementVariables",
                    IsOverride = true,
                };
                UIAsset.MacrossEditor.DefClass.AddMethod(initMethod);
            }
            initMethod.MethodBody.Sequence.Clear();

            mUIHost.QueryElements(GenericElementVariableCode, ref cls);
        }
        void OnAfterCompileCode(UMacrossEditor editor)
        {

        }
    }
}
