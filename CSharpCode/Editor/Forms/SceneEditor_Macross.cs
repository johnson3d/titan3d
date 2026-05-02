using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Xml.Linq;
using EngineNS.Bricks.CodeBuilder;
using EngineNS.Bricks.CodeBuilder.MacrossNode;
using EngineNS.EGui.Controls;
using EngineNS.GamePlay;
using EngineNS.GamePlay.Scene;
using EngineNS.Graphics.Mesh;
using EngineNS.Graphics.Pipeline;
using EngineNS.Macross;
using EngineNS.Rtti;
using EngineNS.Thread;
using EngineNS.Thread.Async;

namespace EngineNS.Editor.Forms
{
    public partial class TtSceneEditor
    {
        void InitializeMacrossEditor()
        {
            Scene.MacrossEditor.AssetName = AssetName;
            Scene.MacrossEditor.FormName = "Macross:" + AssetName.Name;
            Scene.MacrossEditor.HostForm = this;
            Scene.MacrossEditor.DockPostName = "SceneMacrossEditor";
            Scene.MacrossEditor.LoadClassGraph(AssetName);
            Scene.MacrossEditor.DrawToolbarAction = DrawMacrossToolbar;
            Scene.MacrossEditor.BeforeGenerateCode = OnBeforeGenerateCode;
            Scene.MacrossEditor.AfterCompileCode = OnAfterCompileCode;

        }
        void DrawMacrossToolbar(ImDrawList drawList)
        {
            //int toolBarItemIdx = 0;
            var spacing = EGui.UIProxy.StyleConfig.Instance.ToolbarSeparatorThickness + EGui.UIProxy.StyleConfig.Instance.ItemSpacing.X * 2;
            EGui.UIProxy.Toolbar.BeginToolbar(in drawList);

            //if (EGui.UIProxy.CustomButton.ToolButton("Show Scene", in Vector2.Zero,
            //    EGui.UIProxy.StyleConfig.Instance.ToolButtonTextColor,
            //    EGui.UIProxy.StyleConfig.Instance.ToolButtonTextColor_Press,
            //    EGui.UIProxy.StyleConfig.Instance.ToolButtonTextColor_Hover,
            //    EGui.UIProxy.StyleConfig.Instance.PGCreateButtonBGColor,
            //    EGui.UIProxy.StyleConfig.Instance.PGCreateButtonBGActiveColor,
            //    EGui.UIProxy.StyleConfig.Instance.PGCreateButtonBGHoverColor
            //    ))
            //{
            //    //DrawType = enDrawType.Designer;
            //}

            EGui.UIProxy.Toolbar.EndToolbar();
        }
        void GenMacrossNodeCode(TtNode node, TtExecuteSequenceStatement methodBody)
        {
            if(node is ISceneNodeMacrossInterface)
            {
                var macrossNode = node as ISceneNodeMacrossInterface;

                var tempName = "val_" + (uint)(node.NodeId.GetHashCode());
                var findNodeMethod = new TtMethodInvokeStatement()
                {
                    DeclarationReturnValue = true,
                    MethodName = "FindSceneNode",
                    ReturnValue = new TtVariableDeclaration()
                    {
                        VariableType = new TtTypeReference(typeof(TtNode)),
                        VariableName = tempName,
                    }
                };
                var nodeVarName = tempName + "_cast";
                var castDec = new TtVariableDeclaration()
                {
                    VariableType = new TtTypeReference(node.GetType()),
                    VariableName = nodeVarName,
                    InitValue = new TtCastExpression()
                    {
                        TargetType = new TtTypeReference(node.GetType()),
                        SourceType = new TtTypeReference(typeof(TtNode)),
                        Expression = new TtVariableReferenceExpression(tempName)
                    }
                };

                // stackalloc byte
                var guidBytesVarName = nodeVarName + "_id_bytes";
                var nodeIdBytes = node.NodeId.ToByteArray();
                var nodeIdByteExps = new TtExpressionBase[nodeIdBytes.Length];
                for(int i=0; i<nodeIdBytes.Length; i++)
                {
                    nodeIdByteExps[i] = new TtPrimitiveExpression(nodeIdBytes[i]);
                }
                var saSt = new TtStackallocStatement(new TtTypeReference(typeof(byte)), nodeIdByteExps)
                {
                    VarName = guidBytesVarName
                };
                methodBody.Sequence.Add(saSt);

                var guidVarName = nodeVarName + "_id";
                var guidVar = new TtVariableDeclaration()
                {
                    VariableName = guidVarName,
                    VariableType = new TtTypeReference(typeof(Guid)),
                    InitValue = new TtCreateObjectExpression(typeof(Guid).FullName, new TtVariableReferenceExpression(guidBytesVarName))
                };
                methodBody.Sequence.Add(guidVar);

                //var getIdMethod = new TtMethodInvokeStatement()
                //{
                //    DeclarationReturnValue = true,
                //    MethodName = "Parse",
                //    Host = new TtVariableReferenceExpression("System.Guid"),
                //    ReturnValue = new TtVariableDeclaration()
                //    {
                //        VariableName = guidVarName,
                //        VariableType = new TtTypeReference(typeof(Guid))
                //    }
                //};
                //getIdMethod.Arguments.Add(new TtMethodInvokeArgumentExpression(new TtPrimitiveExpression(node.NodeId.ToString())));
                //methodBody.Sequence.Add(getIdMethod);
                
                findNodeMethod.Arguments.Add(new TtMethodInvokeArgumentExpression(new TtVariableReferenceExpression(guidVarName), EMethodArgumentAttribute.In));
                methodBody.Sequence.Add(findNodeMethod);
                methodBody.Sequence.Add(castDec);
                var conditionStatement = new TtIfStatement();
                conditionStatement.Condition = new TtBinaryOperatorExpression()
                {
                    Operation = TtBinaryOperatorExpression.EBinaryOperation.NotEquality,
                    Left = new TtVariableReferenceExpression(nodeVarName),
                    Right = new TtNullValueExpression()
                };
                var trueStatement = new TtExecuteSequenceStatement();
                conditionStatement.TrueStatement = trueStatement;
                methodBody.Sequence.Add(conditionStatement);

                var pros = macrossNode.CollectionMacrossProperties();
                //node.FindNode()
                foreach (var prop in pros)
                {
                    var propExp = macrossNode.GetPropertyExpression(prop);
                    if (propExp == null)
                        continue;

                    var proSetMethodSt = new TtMethodInvokeStatement()
                    {
                        MethodName = "SetPropertyValue",
                        Host = new TtVariableReferenceExpression(nodeVarName)
                    };
                    proSetMethodSt.Arguments.Add(new TtMethodInvokeArgumentExpression()
                    {
                        Expression = new TtPrimitiveExpression(Standart.Hash.xxHash.xxHash64.ComputeHash(prop.Name)),
                    });
                    var expArg = new TtMethodInvokeArgumentExpression()
                    {
                        //OperationType = EMethodArgumentAttribute.In,
                        Expression = propExp
                    };
                    if(propExp is TtVariableReferenceExpression)
                    {
                        expArg.OperationType = EMethodArgumentAttribute.In;
                    }
                    proSetMethodSt.Arguments.Add(expArg);
                    trueStatement.Sequence.Add(proSetMethodSt);

                    //var propAssign = new TtAssignOperatorStatement();
                    //propAssign.From = propExp;
                    //propAssign.To = new TtVariableReferenceExpression(prop.Name, new TtVariableReferenceExpression(nodeVarName));
                    //trueStatement.Sequence.Add(propAssign);
                }
            }
            foreach (var child in node.Children)
            {
                GenMacrossNodeCode(child, methodBody);
            }
        }
        void OnBeforeGenerateCode(TtClassDeclaration cls)
        {
            // Gen set node val data
            var method = new TtMethodDeclaration()
            {
                IsOverride = true,
                MethodName = "InitializeMacrossNodePropertyValues"
            };
            cls.Methods.Remove(method);
            cls.Methods.Add(method);

            GenMacrossNodeCode(Scene, method.MethodBody);
        }
        void OnAfterCompileCode(TtMacrossEditor editor)
        {

        }
    }
}