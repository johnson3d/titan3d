using EngineNS.Animation.Macross.BlendTree.Node;
using EngineNS.Bricks.CodeBuilder;
using EngineNS.DesignMacross;
using EngineNS.DesignMacross.Base.Description;
using EngineNS.DesignMacross.Base.Graph;
using EngineNS.DesignMacross.Design;
using EngineNS.DesignMacross.Design.ConnectingLine;
using EngineNS.DesignMacross.Design.Expressions;
using EngineNS.DesignMacross.Design.Statement;
using EngineNS.EGui.Controls;
using EngineNS.Rtti;
using SixLabors.Fonts;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace EngineNS.Animation.Macross.BlendTree
{
    public class TtBlendTreeGraphContextMenuUtil
    {
        public static void ConstructMenuItemsAboutContextMenuAttribute(ref FGraphElementRenderingContext context, TtPopupMenu popupMenu, TtGraph_BlendTree graph)
        {
            //Collect DMC Descriptions with  ContextMenuAttribute

            var graphDesc = graph.Description;
            var cmdHistory = context.CommandHistory;
            var graphElementStyleManager = context.GraphElementStyleManager;
            var tempContext = context;
            foreach (var service in Rtti.TtTypeDescManager.Instance.Services.Values)
            {
                foreach (var typeDesc in service.Types.Values)
                {
                    var contextMenuAttr = typeDesc.GetCustomAttribute<ContextMenuAttribute>(true);
                    if (contextMenuAttr != null)
                    {
                        if (contextMenuAttr.HasKeyString(UDesignMacross.MacrossAnimEditorKeyword) || contextMenuAttr.HasKeyString(UDesignMacross.MacrossScriptEditorKeyword))
                        {
                            if (typeDesc.IsSubclassOf(TtTypeDesc.TypeOf<TtBlendTreeNodeClassDescription>()))
                            {
                                TtMenuUtil.ConstructMenuItem(popupMenu.Menu, typeDesc, contextMenuAttr.MenuPaths, contextMenuAttr.FilterStrings,
                                                 (TtMenuItem item, object sender) =>
                                                 {
                                                     var popMenu = sender as TtPopupMenu;
                                                     var popedPosition = tempContext.CameraTransform(popMenu.PopedPosition);
                                                     if (Rtti.TtTypeDescManager.CreateInstance(typeDesc) is TtBlendTreeNodeClassDescription node)
                                                     {
                                                         node.Name = graph.GetValidNodeName(node.Name);
                                                         var style = graphElementStyleManager.GetOrAdd(node, popedPosition);
                                                         cmdHistory.CreateAndExtuteCommand("AddBlendTreeNode",
                                                             (data) => { graph.BlendTreeClassDescription.AddNode(node); },
                                                             (data) => { graph.BlendTreeClassDescription.RemoveNode(node); });
                                                     }
                                                 });
                            }

                            if (typeDesc.IsSubclassOf(TtTypeDesc.TypeOf<TtExpressionDescription>()))
                            {
                                TtMenuUtil.ConstructMenuItem(popupMenu.Menu, typeDesc, contextMenuAttr.MenuPaths, contextMenuAttr.FilterStrings,
                                                 (TtMenuItem item, object sender) =>
                                                 {
                                                     var popMenu = sender as TtPopupMenu;
                                                     var popedPosition = tempContext.CameraTransform(popMenu.PopedPosition);
                                                     if (Rtti.TtTypeDescManager.CreateInstance(typeDesc) is TtExpressionDescription expression)
                                                     {
                                                         var style = graphElementStyleManager.GetOrAdd(expression, popedPosition);
                                                         cmdHistory.CreateAndExtuteCommand("AddExpression",
                                                             (data) => { IExpressionOperator.AddExpression(graphDesc, expression); },
                                                             (data) => { IExpressionOperator.RemoveExpression(graphDesc, expression); });
                                                     }
                                                 });
                            }
                        }

                    }
                }
            }
        }
        public static void ConstructMenuItemsAboutDesignedClass(ref FGraphElementRenderingContext context, TtPopupMenu popupMenu, TtGraph_BlendTree graph)
        {
            var graphDesc = graph.Description;
            var cmdHistory = context.CommandHistory;
            var graphElementStyleManager = context.GraphElementStyleManager;
            var tempContext = context;
            foreach (var variable in context.DesignedClassDescription.Variables)
            {
                string[] getMenuPath = { "Self", "Variables", "Get" + variable.Name };
                var getTypeDesc = TtTypeDesc.TypeOf<TtVarGetDescription>();
                TtMenuUtil.ConstructMenuItem(popupMenu.Menu, getTypeDesc, getMenuPath, "",
                                                 (TtMenuItem item, object sender) =>
                                                 {
                                                     var popMenu = sender as TtPopupMenu;
                                                     var popedPosition = tempContext.CameraTransform(popMenu.PopedPosition);
                                                     if (Rtti.TtTypeDescManager.CreateInstance(getTypeDesc) is TtVarGetDescription expression)
                                                     {
                                                         expression.VariableId = variable.Id;
                                                         var style = graphElementStyleManager.GetOrAdd(expression, popedPosition);
                                                         cmdHistory.CreateAndExtuteCommand("AddVarGet",
                                                             (data) => { IExpressionOperator.AddExpression(graphDesc, expression); },
                                                             (data) => { IExpressionOperator.RemoveExpression(graphDesc, expression); });
                                                     }
                                                 });

            }
            string[] selfRefMenuPath = { "Self", "SelfReference" };
            var selfRefTypeDesc = TtTypeDesc.TypeOf<TtSelfReferenceDescription>();
            var superClassType = Rtti.TtTypeDescManager.Instance.GetTypeDescFromFullName(context.DesignedClassDescription.SupperClassNames[0]);
            TtMenuUtil.ConstructMenuItem(popupMenu.Menu, selfRefTypeDesc, selfRefMenuPath, "",
                                             (TtMenuItem item, object sender) =>
                                             {
                                                 var popMenu = sender as TtPopupMenu;
                                                 var popedPosition = tempContext.CameraTransform(popMenu.PopedPosition);
                                                 var expression = new TtSelfReferenceDescription(superClassType);
                                                 var style = graphElementStyleManager.GetOrAdd(expression, popedPosition);
                                                 cmdHistory.CreateAndExtuteCommand("AddSelfRef",
                                                        (data) => { IExpressionOperator.AddExpression(graphDesc, expression); },
                                                        (data) => { IExpressionOperator.RemoveExpression(graphDesc, expression); });
                                             });
            string[] centerDataMenuPath = { "Self", "CenterData" };
            var centerDataTypeDescription = TtTypeDesc.TypeOf<TtCenterDataReferenceDescription>();
            var centerDataTypeFullName = context.DesignedClassDescription.ClassFullName;
            var centerDataType = TtTypeDesc.TypeOfFullName(centerDataTypeFullName);
            TtMenuUtil.ConstructMenuItem(popupMenu.Menu, centerDataTypeDescription, centerDataMenuPath, "",
                                             (TtMenuItem item, object sender) =>
                                             {
                                                 var popMenu = sender as TtPopupMenu;
                                                 var popedPosition = tempContext.CameraTransform(popMenu.PopedPosition);
                                                 var expression = new TtCenterDataReferenceDescription(TtTypeDesc.TypeOfFullName(centerDataTypeFullName));
                                                 var style = graphElementStyleManager.GetOrAdd(expression, popedPosition);
                                                 cmdHistory.CreateAndExtuteCommand("AddCenterData",
                                                                (data) => { IExpressionOperator.AddExpression(graphDesc, expression); },
                                                                (data) => { IExpressionOperator.RemoveExpression(graphDesc, expression); });
                                             });
        }
        public static void ConstructMenuItemsAboutMetas(ref FGraphElementRenderingContext context, TtPopupMenu popupMenu, TtGraph_BlendTree graph)
        {
            var graphDesc = graph.Description;
            var cmdHistory = context.CommandHistory;
            var graphElementStyleManager = context.GraphElementStyleManager;
            var tempContext = context;
            foreach (var metaData in Rtti.TtClassMetaManager.Instance.Metas)
            {
                // create
                if (metaData.Value.MetaAttribute != null && !metaData.Value.MetaAttribute.IsNoMacrossCreate)
                {

                }
                // static method
                foreach (var methodMeta in metaData.Value.Methods)
                {
                    if (!methodMeta.IsStatic)
                        continue;
                    if (methodMeta.Meta.IsNoMacrossUseable)
                        continue;
                    string[] menuPath = methodMeta.Meta.MacrossDisplayPath;
                    if (menuPath == null)
                    {
                        menuPath = TtMenuUtil.GetContextPath(methodMeta.DeclaringType, methodMeta.MethodName);
                    }
                    var typeDesc = TtTypeDesc.TypeOf<TtPureMethodInvokeDescription>();
                    TtMenuUtil.ConstructMenuItem(popupMenu.Menu, typeDesc, menuPath, "",
                                                     (TtMenuItem item, object sender) =>
                                                     {
                                                         var popMenu = sender as TtPopupMenu;
                                                         var popedPosition = tempContext.CameraTransform(popMenu.PopedPosition);
                                                         var methodInvoke = TtPureMethodInvokeDescription.Create(methodMeta);
                                                         var style = graphElementStyleManager.GetOrAdd(methodInvoke, popedPosition);
                                                         cmdHistory.CreateAndExtuteCommand("AddStatement",
                                                             (data) => { IStatementOperator.AddStatement(graphDesc, methodInvoke); },
                                                             (data) => { IStatementOperator.RemoveStatement(graphDesc, methodInvoke); });

                                                     });

                }
            }
        }
    }

    public class TtBlendTreeGraphLinkedPinContextMenuUtil
    {
        //construct by startpin.TypeDesc 's properties and method
        public static void ConstructMenuItemsAboutPinClassPropertiesAndMethods(ref FGraphElementRenderingContext context, TtPopupMenu popupMenu, TtGraph_BlendTree graph)
        {
            var cmdHistory = context.CommandHistory;
            var graphElementStyleManager = context.GraphElementStyleManager;
            var graphDesc = graph.Description;

            if (graph.PreviewLine is TtGraphElement_PreviewDataLine previewDataLine)
            {
                if (previewDataLine.StartPin is TtSelfReferenceDataPin)
                {
                    return;
                }

                var classType = previewDataLine.StartPin.TypeDesc;
                if (classType == null)
                    return;

                if (previewDataLine.StartPin is TtDataInPinDescription)
                    return;

                var tempContext = context;
                foreach (var property in classType.GetProperties())
                {
                    //Get
                    {
                        string[] menuPath = { classType.Name, "Get" + property.Name };
                        var typeDesc = TtTypeDesc.TypeOf(property.PropertyType);
                        TtMenuUtil.ConstructMenuItem(popupMenu.Menu, typeDesc, menuPath, "",
                                                    (TtMenuItem.FMenuAction)((TtMenuItem item, object sender) =>
                                                    {
                                                        var popMenu = sender as TtPopupMenu;
                                                        var popedPosition = tempContext.CameraTransform(popMenu.PopedPosition);
                                                        TtPropertyGetDescription getExpression = new(classType, typeDesc);
                                                        getExpression.Name = property.Name;
                                                        getExpression.HostReferenceId = previewDataLine.StartPin.Parent.Id;
                                                        var line = new TtDataLineDescription { FromId = previewDataLine.StartPin.Id, ToId = getExpression.GetHostPin().Id };
                                                        var existLines = IDataLineOperator.GetExistSingleLinkDataLine(graphDesc, previewDataLine.StartPin as TtDataPinDescription);
                                                        var style = graphElementStyleManager.GetOrAdd(getExpression, popedPosition);
                                                        cmdHistory.CreateAndExtuteCommand("AddExpressionAndDataLink",
                                                                (data) =>
                                                                {
                                                                    IExpressionOperator.AddExpression(graphDesc, getExpression);
                                                                    IDataLineOperator.AddSingleDataLine(graphDesc, line, existLines);
                                                                },
                                                                (data) =>
                                                                {
                                                                    IExpressionOperator.RemoveExpression(graphDesc, getExpression);
                                                                    IDataLineOperator.RemoveSingleDataLine(graphDesc, line, existLines);
                                                                });
                                                    }));
                    }
                }
                foreach (var method in classType.GetMethods())
                {
                    if (!method.IsPublic || method.IsStatic)
                        continue;

                    string[] menuPath = { classType.Name, method.Name };
                    var typeDesc = TtTypeDesc.TypeOf<TtPureMethodInvokeDescription>();
                    var statement = TtPureMethodInvokeDescription.Create(method);
                    if (statement.GetHostPin() != null)
                    {
                        var line = new TtDataLineDescription { FromId = previewDataLine.StartPin.Id, ToId = statement.GetHostPin().Id };
                        TtMenuUtil.ConstructMenuItem(popupMenu.Menu, typeDesc, menuPath, "",
                                                                             (TtMenuItem.FMenuAction)((TtMenuItem item, object sender) =>
                                                                             {
                                                                                 var popMenu = sender as TtPopupMenu;
                                                                                 var popedPosition = tempContext.CameraTransform(popMenu.PopedPosition);
                                                                                 var style = graphElementStyleManager.GetOrAdd(statement, popedPosition);
                                                                                 var existLines = IDataLineOperator.GetExistSingleLinkDataLine(graphDesc, previewDataLine.StartPin as TtDataPinDescription);
                                                                                 cmdHistory.CreateAndExtuteCommand("AddStatementAndDataLink",
                                                                                          (data) =>
                                                                                          {
                                                                                              IStatementOperator.AddStatement(graphDesc, statement);
                                                                                              IDataLineOperator.AddSingleDataLine(graphDesc, line, existLines);
                                                                                          },
                                                                                          (data) =>
                                                                                          {
                                                                                              IStatementOperator.RemoveStatement(graphDesc, statement);
                                                                                              IDataLineOperator.RemoveSingleDataLine(graphDesc, line, existLines);
                                                                                          });

                                                                             }));
                    }
                }
            }
        }
        public static void ConstructMenuItemsAboutDesignedClass(ref FGraphElementRenderingContext context, TtPopupMenu popupMenu, TtGraph_BlendTree graph)
        {
            var cmdHistory = context.CommandHistory;
            var graphElementStyleManager = context.GraphElementStyleManager;
            var graphDesc = graph.Description;
            if (graph.PreviewLine is TtGraphElement_PreviewDataLine previewDataLine)
            {
                var tempContext = context;
                foreach (var variable in context.DesignedClassDescription.Variables)
                {
                    string[] getMenuPath = { "Self", "Variables", "Get" + variable.Name };
                    var getTypeDesc = TtTypeDesc.TypeOf<TtVarGetDescription>();
                    var expression = Rtti.TtTypeDescManager.CreateInstance(getTypeDesc) as TtVarGetDescription;
                    expression.VariableId = variable.Id;
                    FDescriptionUpdateContext descriptionUpdateContext = new();
                    descriptionUpdateContext.ClassDescription = tempContext.DesignedClassDescription;
                    expression.UpdateData(ref descriptionUpdateContext);
                    var canLink = IDataLineOperator.TryGetAvailableDataLineWithPin(expression, previewDataLine.StartPin as TtDataPinDescription, out var line);
                    if (canLink)
                    {
                        TtMenuUtil.ConstructMenuItem(popupMenu.Menu, getTypeDesc, getMenuPath, "",
                                                         (TtMenuItem item, object sender) =>
                                                         {
                                                             var popMenu = sender as TtPopupMenu;
                                                             var popedPosition = tempContext.CameraTransform(popMenu.PopedPosition);
                                                             if (canLink)
                                                             {
                                                                 var style = graphElementStyleManager.GetOrAdd(expression, popedPosition);
                                                                 var existLines = IDataLineOperator.GetExistSingleLinkDataLine(graphDesc, previewDataLine.StartPin as TtDataPinDescription);
                                                                 cmdHistory.CreateAndExtuteCommand("AddVarGet",
                                                                            (data) =>
                                                                            {
                                                                                IExpressionOperator.AddExpression(graphDesc, expression);
                                                                                IDataLineOperator.AddSingleDataLine(graphDesc, line, existLines);
                                                                            },
                                                                            (data) =>
                                                                            {
                                                                                IExpressionOperator.RemoveExpression(graphDesc, expression);
                                                                                IDataLineOperator.RemoveSingleDataLine(graphDesc, line, existLines);
                                                                            });
                                                             }
                                                         });
                    }
                }
                if (previewDataLine.StartPin is TtSelfReferenceDataPin)
                {

                    foreach (var superClassName in context.DesignedClassDescription.SupperClassNames)
                    {
                        var superClassType = Rtti.TtTypeDescManager.Instance.GetTypeDescFromFullName(superClassName);
                        foreach (var property in superClassType.GetProperties())
                        {
                            //Get
                            {
                                string[] menuPath = { "Self", "Get" + property.Name };
                                var typeDesc = TtTypeDesc.TypeOf(property.PropertyType);
                                TtPropertyGetDescription getExpression = new(superClassType, typeDesc);
                                getExpression.Name = property.Name;
                                getExpression.HostReferenceId = previewDataLine.StartPin.Parent.Id;
                                var canLink = IDataLineOperator.TryGetAvailableDataLineWithPin(getExpression, previewDataLine.StartPin as TtDataPinDescription, out var line);
                                if (canLink)
                                {
                                    TtMenuUtil.ConstructMenuItem(popupMenu.Menu, typeDesc, menuPath, "",
                                                                (TtMenuItem.FMenuAction)((TtMenuItem item, object sender) =>
                                                                {
                                                                    var popMenu = sender as TtPopupMenu;
                                                                    var popedPosition = tempContext.CameraTransform(popMenu.PopedPosition);
                                                                    var style = graphElementStyleManager.GetOrAdd(getExpression, popedPosition);
                                                                    var existLines = IDataLineOperator.GetExistSingleLinkDataLine(graphDesc, previewDataLine.StartPin as TtDataPinDescription);
                                                                    cmdHistory.CreateAndExtuteCommand("AddExpressionAndDataLink",
                                                                            (data) =>
                                                                            {
                                                                                IExpressionOperator.AddExpression(graphDesc, getExpression);
                                                                                IDataLineOperator.AddSingleDataLine(graphDesc, line, existLines);
                                                                            },
                                                                            (data) =>
                                                                            {
                                                                                IExpressionOperator.RemoveExpression(graphDesc, getExpression);
                                                                                IDataLineOperator.RemoveSingleDataLine(graphDesc, line, existLines);
                                                                            });
                                                                }));
                                }
                            }
                        }
                    }
                }
            }
        }
        public static void ConstructMenuItemsAboutContextMenuAttribute(ref FGraphElementRenderingContext context, TtPopupMenu popupMenu, TtGraph_BlendTree graph)
        {
            var cmdHistory = context.CommandHistory;
            var graphElementStyleManager = context.GraphElementStyleManager;
            var graphDesc = graph.Description;
            var tempContext = context;
            foreach (var service in Rtti.TtTypeDescManager.Instance.Services.Values)
            {
                foreach (var typeDesc in service.Types.Values)
                {
                    var att = typeDesc.GetCustomAttribute<ContextMenuAttribute>(true);
                    if (att != null)
                    {
                        if (att.HasKeyString(UDesignMacross.MacrossScriptEditorKeyword) || att.HasKeyString(UDesignMacross.MacrossAnimEditorKeyword))
                        {
                            if (typeDesc.IsSubclassOf(TtTypeDesc.TypeOf<TtExpressionDescription>()))
                            {
                                var expression = Rtti.TtTypeDescManager.CreateInstance(typeDesc) as TtExpressionDescription;
                                if (graph.PreviewLine is TtGraphElement_PreviewDataLine previewDataLine)
                                {
                                    var canLink = IDataLineOperator.TryGetAvailableDataLineWithPin(expression, previewDataLine.StartPin as TtDataPinDescription, out var line);
                                    if (canLink)
                                    {
                                        TtMenuUtil.ConstructMenuItem(popupMenu.Menu, typeDesc, att.MenuPaths, att.FilterStrings,
                                                                                             (TtMenuItem.FMenuAction)((TtMenuItem item, object sender) =>
                                                                                             {
                                                                                                 var popMenu = sender as TtPopupMenu;
                                                                                                 var popedPosition = tempContext.CameraTransform(popMenu.PopedPosition);
                                                                                                 var style = graphElementStyleManager.GetOrAdd(expression, popedPosition);
                                                                                                 var existLines = IDataLineOperator.GetExistSingleLinkDataLine(graphDesc, previewDataLine.StartPin as TtDataPinDescription);
                                                                                                 cmdHistory.CreateAndExtuteCommand("AddExpressionAndDataLink",
                                                                                                          (data) =>
                                                                                                          {
                                                                                                              IExpressionOperator.AddExpression(graphDesc, expression);
                                                                                                              IDataLineOperator.AddSingleDataLine(graphDesc, line, existLines);
                                                                                                          },
                                                                                                        (data) =>
                                                                                                        {
                                                                                                            IExpressionOperator.RemoveExpression(graphDesc, expression);
                                                                                                            IDataLineOperator.RemoveSingleDataLine(graphDesc, line, existLines);
                                                                                                        });
                                                                                             }));
                                    }
                                }
                            }
                            else if (typeDesc.IsSubclassOf(TtTypeDesc.TypeOf<TtBlendTreeNodeClassDescription>()))
                            {
                                if (graph.PreviewLine is TtGraphElement_PreviewPoseLine previewPoseLine)
                                {
                                    var node = Rtti.TtTypeDescManager.CreateInstance(typeDesc) as TtBlendTreeNodeClassDescription;
                                    var canLink = IPoseLineOperator.TryGetLinkedPoseLineWithPin(node, previewPoseLine.StartPin as TtPosePinDescription, out var line);
                                    if (canLink)
                                    {
                                        TtMenuUtil.ConstructMenuItem(popupMenu.Menu, typeDesc, att.MenuPaths, att.FilterStrings,
                                                                                             (TtMenuItem.FMenuAction)((TtMenuItem item, object sender) =>
                                                                                             {
                                                                                                 var popMenu = sender as TtPopupMenu;
                                                                                                 var popedPosition = tempContext.CameraTransform(popMenu.PopedPosition);
                                                                                                 var style = graphElementStyleManager.GetOrAdd(node, popedPosition);
                                                                                                 var existLines = IPoseLineOperator.GetExistSingleLinkPoseLine(graphDesc, previewPoseLine.StartPin as TtPosePinDescription);
                                                                                                 cmdHistory.CreateAndExtuteCommand("AddBlendTreeNodeAndPoseLink",
                                                                                                          (data) =>
                                                                                                          {
                                                                                                              graph.BlendTreeClassDescription.AddNode(node);
                                                                                                              IPoseLineOperator.AddSinglePoseLine(graphDesc, line, existLines);
                                                                                                          },
                                                                                                        (data) =>
                                                                                                        {
                                                                                                            graph.BlendTreeClassDescription.RemoveNode(node);
                                                                                                            IPoseLineOperator.RemoveSinglePoseLine(graphDesc, line, existLines);
                                                                                                        });
                                                                                             }));
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
        public static void ConstructMenuItemsAboutMetas(ref FGraphElementRenderingContext context, TtPopupMenu popupMenu, TtGraph_BlendTree graph)
        {
            var cmdHistory = context.CommandHistory;
            var graphElementStyleManager = context.GraphElementStyleManager;
            var graphDesc = graph.Description;
            var tempContext = context;
            foreach (var metaData in Rtti.TtClassMetaManager.Instance.Metas)
            {
                // create
                if (metaData.Value.MetaAttribute != null && !metaData.Value.MetaAttribute.IsNoMacrossCreate)
                {

                }
                // static method
                foreach (var methodMeta in metaData.Value.Methods)
                {
                    if (!methodMeta.IsStatic)
                        continue;
                    if (methodMeta.Meta.IsNoMacrossUseable)
                        continue;
                    var typeDesc = TtTypeDesc.TypeOf<TtPureMethodInvokeDescription>();
                    var statement = TtPureMethodInvokeDescription.Create(methodMeta);
                    string[] menuPath = methodMeta.Meta.MacrossDisplayPath;
                    if (menuPath == null)
                    {
                        menuPath = TtMenuUtil.GetContextPath(methodMeta.DeclaringType, methodMeta.MethodName);
                    }
                    if (graph.PreviewLine is TtGraphElement_PreviewDataLine previewDataLine)
                    {
                        var canLink = IDataLineOperator.TryGetAvailableDataLineWithPin(statement, previewDataLine.StartPin as TtDataPinDescription, out var line);
                        if (canLink)
                        {
                            TtMenuUtil.ConstructMenuItem(popupMenu.Menu, typeDesc, menuPath, "",
                                                                                 (TtMenuItem.FMenuAction)((TtMenuItem item, object sender) =>
                                                                                 {
                                                                                     var popMenu = sender as TtPopupMenu;
                                                                                     var popedPosition = tempContext.CameraTransform(popMenu.PopedPosition);
                                                                                     var style = graphElementStyleManager.GetOrAdd(statement, popedPosition);
                                                                                     var existLines = IDataLineOperator.GetExistSingleLinkDataLine(graphDesc, previewDataLine.StartPin as TtDataPinDescription);
                                                                                     cmdHistory.CreateAndExtuteCommand("AddStatementAndDataLink",
                                                                                                  (data) =>
                                                                                                  {
                                                                                                      IStatementOperator.AddStatement(graphDesc, statement);
                                                                                                      IDataLineOperator.AddSingleDataLine(graphDesc, line, existLines);
                                                                                                  },
                                                                                                    (data) =>
                                                                                                    {
                                                                                                        IStatementOperator.RemoveStatement(graphDesc, statement);
                                                                                                        IDataLineOperator.RemoveSingleDataLine(graphDesc, line, existLines);
                                                                                                    });
                                                                                 }));
                        }
                    }
                }
            }
        }
    }
}
