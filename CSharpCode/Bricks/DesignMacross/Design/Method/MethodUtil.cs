using EngineNS.Animation.Macross.BlendTree.Node;
using EngineNS.Bricks.CodeBuilder;
using EngineNS.DesignMacross.Base.Description;
using EngineNS.DesignMacross.Base.Graph;
using EngineNS.DesignMacross.Design.ConnectingLine;
using EngineNS.DesignMacross.Design.Expressions;
using EngineNS.DesignMacross.Design.Statement;
using EngineNS.EGui.Controls;
using EngineNS.Rtti;
using Jither.OpenEXR.Attributes;
using Org.BouncyCastle.Asn1.X509.Qualified;
using SixLabors.Fonts;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace EngineNS.DesignMacross.Design
{
    public class TtMethodGraphContextMenuUtil
    {
        public static void ConstructMenuItemsAboutContextMenuAttribute(ref FGraphElementRenderingContext context, TtPopupMenu popupMenu, TtGraph_Method methodGraph)
        {
            //Collect DMC Descriptions with  ContextMenuAttribute

            var methodDescription = methodGraph.MethodDescription;
            var cmdHistory = context.CommandHistory;
            var graphElementStyleManager = context.GraphElementStyleManager;
            var tempContext = context;
            foreach (var service in Rtti.TtTypeDescManager.Instance.Services.Values)
            {
                foreach (var typeDesc in service.Types.Values)
                {
                    var att = typeDesc.GetCustomAttribute<ContextMenuAttribute>(true);
                    if (att != null)
                    {
                        if (att.HasKeyString(UDesignMacross.MacrossScriptEditorKeyword))
                        {
                            TtMenuUtil.ConstructMenuItem(popupMenu.Menu, typeDesc, att.MenuPaths, att.FilterStrings,
                                                     (TtMenuItem.FMenuAction)((TtMenuItem item, object sender) =>
                                                     {
                                                         var popMenu = sender as TtPopupMenu;
                                                         var popedPosition = tempContext.CameraTransform(popMenu.PopedPosition);
                                                         if (Rtti.TtTypeDescManager.CreateInstance(typeDesc) is TtExpressionDescription expression)
                                                         {
                                                             var style = graphElementStyleManager.GetOrAdd(expression, popedPosition);
                                                             cmdHistory.CreateAndExtuteCommand("AddExpression",
                                                                 (data) => { methodDescription.AddExpression(expression); },
                                                                 (data) => { methodDescription.RemoveExpression(expression); });
                                                         }
                                                         if (Rtti.TtTypeDescManager.CreateInstance(typeDesc) is TtStatementDescription statement)
                                                         {
                                                             var style = graphElementStyleManager.GetOrAdd(statement, popedPosition);
                                                             cmdHistory.CreateAndExtuteCommand("AddStatement",
                                                                 (data) => { methodDescription.AddStatement(statement); },
                                                                 (data) => { methodDescription.RemoveStatement(statement); });
                                                         }
                                                     }));
                        }
                    }
                }
            }
        }
        public static void ConstructMenuItemsAboutDesignedClass(ref FGraphElementRenderingContext context, TtPopupMenu popupMenu, TtGraph_Method methodGraph)
        {
            var methodDescription = methodGraph.MethodDescription;
            var cmdHistory = context.CommandHistory;
            var graphElementStyleManager = context.GraphElementStyleManager;
            var tempContext = context;
            foreach (var variable in context.DesignedClassDescription.Variables)
            {
                string[] getMenuPath = { "Self", "Variables", "Get" + variable.Name };
                var getTypeDesc = TtTypeDesc.TypeOf<TtVarGetDescription>();
                TtMenuUtil.ConstructMenuItem(popupMenu.Menu, getTypeDesc, getMenuPath, "",
                                                 (TtMenuItem.FMenuAction)((TtMenuItem item, object sender) =>
                                                 {
                                                     var popMenu = sender as TtPopupMenu;
                                                     var popedPosition = tempContext.CameraTransform(popMenu.PopedPosition);
                                                     if (Rtti.TtTypeDescManager.CreateInstance(getTypeDesc) is TtVarGetDescription expression)
                                                     {
                                                         expression.VariableId = variable.Id;
                                                         var style = graphElementStyleManager.GetOrAdd(expression, popMenu.PopedPosition);
                                                         cmdHistory.CreateAndExtuteCommand("AddVarGet",
                                                             (data) => { methodDescription.AddExpression(expression); },
                                                             (data) => { methodDescription.RemoveExpression(expression); });
                                                     }
                                                 }));

                string[] setMenuPath = { "Self", "Variables", "Set" + variable.Name };
                var setTypeDesc = TtTypeDesc.TypeOf<TtVarSetDescription>();
                TtMenuUtil.ConstructMenuItem(popupMenu.Menu, setTypeDesc, setMenuPath, "",
                                                 (TtMenuItem.FMenuAction)((TtMenuItem item, object sender) =>
                                                 {
                                                     var popMenu = sender as TtPopupMenu;
                                                     var popedPosition = tempContext.CameraTransform(popMenu.PopedPosition);
                                                     var statement = new TtVarSetDescription(variable.Id, variable.VariableType.TypeDesc);
                                                     var style = graphElementStyleManager.GetOrAdd(statement, popedPosition);
                                                     cmdHistory.CreateAndExtuteCommand("AddVarSet",
                                                         (data) => { methodDescription.AddStatement(statement); },
                                                         (data) => { methodDescription.RemoveStatement(statement); });
                                                 }));

            }
            string[] selfRefMenuPath = { "Self", "SelfReference" };
            var selfRefTypeDesc = TtTypeDesc.TypeOf<TtSelfReferenceDescription>();
            var superClassType = Rtti.TtTypeDescManager.Instance.GetTypeDescFromFullName(context.DesignedClassDescription.SupperClassNames[0]);
            TtMenuUtil.ConstructMenuItem(popupMenu.Menu, selfRefTypeDesc, selfRefMenuPath, "",
                                             (TtMenuItem.FMenuAction)((TtMenuItem item, object sender) =>
                                             {
                                                 var popMenu = sender as TtPopupMenu;
                                                 var popedPosition = tempContext.CameraTransform(popMenu.PopedPosition);
                                                 var expression = new TtSelfReferenceDescription(superClassType);
                                                 var style = graphElementStyleManager.GetOrAdd(expression, popedPosition);
                                                 cmdHistory.CreateAndExtuteCommand("AddSelfRef",
                                                     (data) => { methodDescription.AddExpression(expression); },
                                                     (data) => { methodDescription.RemoveExpression(expression); });
                                             }));
            string[] centerDataMenuPath = { "Self", "CenterData" };
            var centerDataTypeDescription = TtTypeDesc.TypeOf<TtCenterDataReferenceDescription>();
            var centerDataTypeFullName = context.DesignedClassDescription.ClassFullName;
            var centerDataType = TtTypeDesc.TypeOfFullName(centerDataTypeFullName);

            TtMenuUtil.ConstructMenuItem(popupMenu.Menu, centerDataTypeDescription, centerDataMenuPath, "",
                                             (TtMenuItem.FMenuAction)((TtMenuItem item, object sender) =>
                                             {
                                                 var popMenu = sender as TtPopupMenu;
                                                 var popedPosition = tempContext.CameraTransform(popMenu.PopedPosition);
                                                 var expression = new TtCenterDataReferenceDescription(TtTypeDesc.TypeOfFullName(centerDataTypeFullName));
                                                 var style = graphElementStyleManager.GetOrAdd(expression, popedPosition);
                                                 cmdHistory.CreateAndExtuteCommand("AddCenterData",
                                                     (data) => { methodDescription.AddExpression(expression); },
                                                     (data) => { methodDescription.RemoveExpression(expression); });
                                             }));
        }
        public static void ConstructMenuItemsAboutMetas(ref FGraphElementRenderingContext context, TtPopupMenu popupMenu, TtGraph_Method methodGraph)
        {
            var methodDescription = methodGraph.MethodDescription;
            var cmdHistory = context.CommandHistory;
            var graphElementStyleManager = context.GraphElementStyleManager;
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
                    var typeDesc = TtTypeDesc.TypeOf<TtMethodInvokeDescription>();
                    var tempContext = context;
                    TtMenuUtil.ConstructMenuItem(popupMenu.Menu, typeDesc, menuPath, "",
                                                     (TtMenuItem.FMenuAction)((TtMenuItem item, object sender) =>
                                                     {
                                                         var popMenu = sender as TtPopupMenu;
                                                         var popedPosition = tempContext.CameraTransform(popMenu.PopedPosition);
                                                         var methodInvoke = TtMethodInvokeDescription.Create(methodMeta);
                                                         var style = graphElementStyleManager.GetOrAdd(methodInvoke, popedPosition);
                                                         cmdHistory.CreateAndExtuteCommand("AddStatement",
                                                             (data) => { methodDescription.AddStatement(methodInvoke); },
                                                             (data) => { methodDescription.RemoveStatement(methodInvoke); });

                                                     }));

                }
            }
        }
    }

    public class TtMethodGraphLinkedPinContextMenuUtil
    {
        public static void ConstructMenuItemsAboutClassPropertiesAndMethods_OutPin(ref FGraphElementRenderingContext context, TtPopupMenu popupMenu, TtGraph_Method methodGraph)
        {
            var cmdHistory = context.CommandHistory;
            var graphElementStyleManager = context.GraphElementStyleManager;
            var graphDesc = methodGraph.Description;

            if (methodGraph.PreviewLine is TtGraphElement_PreviewDataLine previewDataLine)
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
                    //Set
                    {
                        string[] menuPath = { classType.Name, "Set" + property.Name };
                        var typeDesc = TtTypeDesc.TypeOf(property.PropertyType);
                        TtMenuUtil.ConstructMenuItem(popupMenu.Menu, typeDesc, menuPath, "",
                                                    (TtMenuItem.FMenuAction)((TtMenuItem item, object sender) =>
                                                    {
                                                        var popMenu = sender as TtPopupMenu;
                                                        var popedPosition = tempContext.CameraTransform(popMenu.PopedPosition);
                                                        TtPropertySetDescription setStatement = new(classType, typeDesc);
                                                        setStatement.Name = property.Name;
                                                        setStatement.HostReferenceId = previewDataLine.StartPin.Parent.Id;
                                                        var line = new TtDataLineDescription { FromId = previewDataLine.StartPin.Id, ToId = setStatement.GetHostPin().Id };
                                                        var style = graphElementStyleManager.GetOrAdd(setStatement, popedPosition);
                                                        var existLines = IDataLineOperator.GetExistSingleLinkDataLine(graphDesc, previewDataLine.StartPin as TtDataPinDescription);
                                                        cmdHistory.CreateAndExtuteCommand("AddExpressionAndDataLink",
                                                                                 (data) =>
                                                                                 {
                                                                                     IStatementOperator.AddStatement(graphDesc, setStatement);
                                                                                     IDataLineOperator.AddSingleDataLine(graphDesc, line, existLines);
                                                                                 },
                                                                                   (data) =>
                                                                                   {
                                                                                       IStatementOperator.RemoveStatement(graphDesc, setStatement);
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
                    var typeDesc = TtTypeDesc.TypeOf<TtMethodInvokeDescription>();
                    var statement = TtMethodInvokeDescription.Create(method);
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
        public static void ConstructMenuItemsAboutDesignedClass(ref FGraphElementRenderingContext context, TtPopupMenu popupMenu, TtGraph_Method methodGraph)
        {
            var cmdHistory = context.CommandHistory;
            var graphElementStyleManager = context.GraphElementStyleManager;
            var graphDesc = methodGraph.Description;
            if (methodGraph.PreviewLine is TtGraphElement_PreviewDataLine previewDataLine)
            {
                if (previewDataLine.StartPin is TtSelfReferenceDataPin)
                {
                    var tempContext = context;
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
                            //Set
                            {
                                string[] menuPath = { "Self", "Set" + property.Name };
                                var typeDesc = TtTypeDesc.TypeOf(property.PropertyType);
                                TtPropertySetDescription setStatement = new(superClassType, typeDesc);
                                setStatement.Name = property.Name;
                                setStatement.HostReferenceId = previewDataLine.StartPin.Parent.Id;
                                var canLink = IDataLineOperator.TryGetAvailableDataLineWithPin(setStatement, previewDataLine.StartPin as TtDataPinDescription, out var line);
                                if (canLink)
                                {
                                    TtMenuUtil.ConstructMenuItem(popupMenu.Menu, typeDesc, menuPath, "",
                                                                (TtMenuItem.FMenuAction)((TtMenuItem item, object sender) =>
                                                                {
                                                                    var popMenu = sender as TtPopupMenu;
                                                                    var popedPosition = tempContext.CameraTransform(popMenu.PopedPosition);
                                                                    var style = graphElementStyleManager.GetOrAdd(setStatement, popedPosition);
                                                                    var existLines = IDataLineOperator.GetExistSingleLinkDataLine(graphDesc, previewDataLine.StartPin as TtDataPinDescription);
                                                                    cmdHistory.CreateAndExtuteCommand("AddStatementAndDataLink",
                                                                                 (data) =>
                                                                                 {
                                                                                     IStatementOperator.AddStatement(graphDesc, setStatement);
                                                                                     IDataLineOperator.AddSingleDataLine(graphDesc, line, existLines);
                                                                                 },
                                                                                   (data) =>
                                                                                   {
                                                                                       IStatementOperator.RemoveStatement(graphDesc, setStatement);
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
        public static void ConstructMenuItemsAboutContextMenuAttribute(ref FGraphElementRenderingContext context, TtPopupMenu popupMenu, TtGraph_Method methodGraph)
        {
            var cmdHistory = context.CommandHistory;
            var graphElementStyleManager = context.GraphElementStyleManager;
            var graphDesc = methodGraph.Description;
            var tempContext = context;
            foreach (var service in Rtti.TtTypeDescManager.Instance.Services.Values)
            {
                foreach (var typeDesc in service.Types.Values)
                {
                    var att = typeDesc.GetCustomAttribute<ContextMenuAttribute>(true);
                    if (att != null)
                    {
                        if (att.HasKeyString(UDesignMacross.MacrossScriptEditorKeyword))
                        {
                            if (Rtti.TtTypeDescManager.CreateInstance(typeDesc) is TtExpressionDescription expression)
                            {
                                if (methodGraph.PreviewLine is TtGraphElement_PreviewDataLine previewDataLine)
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
                                if (methodGraph.PreviewLine is TtGraphElement_PreviewExecutionLine previewExecutionLine)
                                {
                                    var canLink = IExecutionLineOperator.TryGetLinkedExecutionLineWithPin(expression, previewExecutionLine.StartPin as TtExecutionPinDescription, out var line);
                                    if (canLink)
                                    {
                                        TtMenuUtil.ConstructMenuItem(popupMenu.Menu, typeDesc, att.MenuPaths, att.FilterStrings,
                                                                                             (TtMenuItem.FMenuAction)((TtMenuItem item, object sender) =>
                                                                                             {
                                                                                                 var popMenu = sender as TtPopupMenu;
                                                                                                 var popedPosition = tempContext.CameraTransform(popMenu.PopedPosition);
                                                                                                 var style = graphElementStyleManager.GetOrAdd(expression, popedPosition);
                                                                                                 var existLines = IExecutionLineOperator.GetExistSingleLinkExecutionLine(graphDesc, previewExecutionLine.StartPin as TtExecutionPinDescription);
                                                                                                 cmdHistory.CreateAndExtuteCommand("AddExpressionAndExecutionLink",
                                                                                                          (data) =>
                                                                                                          {
                                                                                                              IExpressionOperator.AddExpression(graphDesc, expression);
                                                                                                              IExecutionLineOperator.AddSingleExecutionLine(graphDesc, line, existLines);
                                                                                                          },
                                                                                                        (data) =>
                                                                                                        {
                                                                                                            IExpressionOperator.RemoveExpression(graphDesc, expression);
                                                                                                            IExecutionLineOperator.RemoveSingleExecutionLine(graphDesc, line, existLines);
                                                                                                        });
                                                                                             }));
                                    }
                                }
                            }

                            if (Rtti.TtTypeDescManager.CreateInstance(typeDesc) is TtStatementDescription statement)
                            {
                                if (methodGraph.PreviewLine is TtGraphElement_PreviewDataLine previewDataLine)
                                {
                                    var canLink = IDataLineOperator.TryGetAvailableDataLineWithPin(statement, previewDataLine.StartPin as TtDataPinDescription, out var line);
                                    if (canLink)
                                    {
                                        TtMenuUtil.ConstructMenuItem(popupMenu.Menu, typeDesc, att.MenuPaths, att.FilterStrings,
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
                                if (methodGraph.PreviewLine is TtGraphElement_PreviewExecutionLine previewExecutionLine)
                                {
                                    var canLink = IExecutionLineOperator.TryGetLinkedExecutionLineWithPin(statement, previewExecutionLine.StartPin as TtExecutionPinDescription, out var line);
                                    if (canLink)
                                    {
                                        TtMenuUtil.ConstructMenuItem(popupMenu.Menu, typeDesc, att.MenuPaths, att.FilterStrings,
                                                                                             (TtMenuItem.FMenuAction)((TtMenuItem item, object sender) =>
                                                                                             {
                                                                                                 var popMenu = sender as TtPopupMenu;
                                                                                                 var popedPosition = tempContext.CameraTransform(popMenu.PopedPosition);
                                                                                                 var style = graphElementStyleManager.GetOrAdd(statement, popedPosition);
                                                                                                 var existLines = IExecutionLineOperator.GetExistSingleLinkExecutionLine(graphDesc, previewExecutionLine.StartPin as TtExecutionPinDescription);
                                                                                                 cmdHistory.CreateAndExtuteCommand("AddStatementAndExecLink",
                                                                                                          (data) =>
                                                                                                          {
                                                                                                              IStatementOperator.AddStatement(graphDesc, statement);
                                                                                                              IExecutionLineOperator.AddSingleExecutionLine(graphDesc, line, existLines);
                                                                                                          },
                                                                                                        (data) =>
                                                                                                        {
                                                                                                            IStatementOperator.RemoveStatement(graphDesc, statement);
                                                                                                            IExecutionLineOperator.RemoveSingleExecutionLine(graphDesc, line, existLines);
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
        public static void ConstructMenuItemsAboutMetas(ref FGraphElementRenderingContext context, TtPopupMenu popupMenu, TtGraph_Method methodGraph)
        {
            var cmdHistory = context.CommandHistory;
            var graphElementStyleManager = context.GraphElementStyleManager;
            var graphDesc = methodGraph.Description;
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
                    var typeDesc = TtTypeDesc.TypeOf<TtMethodInvokeDescription>();
                    var statement = TtMethodInvokeDescription.Create(methodMeta);
                    string[] menuPath = methodMeta.Meta.MacrossDisplayPath;
                    if (menuPath == null)
                    {
                        menuPath = TtMenuUtil.GetContextPath(methodMeta.DeclaringType, methodMeta.MethodName);
                    }
                    if (methodGraph.PreviewLine is TtGraphElement_PreviewDataLine previewDataLine)
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
                    if (methodGraph.PreviewLine is TtGraphElement_PreviewExecutionLine previewExecutionLine)
                    {
                        var canLink = IExecutionLineOperator.TryGetLinkedExecutionLineWithPin(statement, previewExecutionLine.StartPin as TtExecutionPinDescription, out var line);
                        if (canLink)
                        {
                            TtMenuUtil.ConstructMenuItem(popupMenu.Menu, typeDesc, menuPath, "",
                                                                                 (TtMenuItem.FMenuAction)((TtMenuItem item, object sender) =>
                                                                                 {
                                                                                     var popMenu = sender as TtPopupMenu;
                                                                                     var popedPosition = tempContext.CameraTransform(popMenu.PopedPosition);
                                                                                     var style = graphElementStyleManager.GetOrAdd(statement, popedPosition);
                                                                                     var existLines = IExecutionLineOperator.GetExistSingleLinkExecutionLine(graphDesc, previewExecutionLine.StartPin as TtExecutionPinDescription);
                                                                                     cmdHistory.CreateAndExtuteCommand("AddStatementAndExecLink",
                                                                                              (data) =>
                                                                                              {
                                                                                                  IStatementOperator.AddStatement(graphDesc, statement);
                                                                                                  IExecutionLineOperator.AddSingleExecutionLine(graphDesc, line, existLines);
                                                                                              },
                                                                                            (data) =>
                                                                                            {
                                                                                                IStatementOperator.RemoveStatement(graphDesc, statement);
                                                                                                IExecutionLineOperator.RemoveSingleExecutionLine(graphDesc, line, existLines);
                                                                                            });
                                                                                 }));
                        }
                    }
                }
            }
        }
    }
}
