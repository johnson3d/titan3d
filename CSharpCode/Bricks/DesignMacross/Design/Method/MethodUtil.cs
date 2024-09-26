using System;
using System.Collections.Generic;
using System.Text;
using EngineNS.Bricks.CodeBuilder;
using EngineNS.DesignMacross.Base.Description;
using EngineNS.DesignMacross.Base.Graph;
using EngineNS.DesignMacross.Design.ConnectingLine;
using EngineNS.DesignMacross.Design.Expressions;
using EngineNS.DesignMacross.Design.Statement;
using EngineNS.EGui.Controls;
using EngineNS.Rtti;

namespace EngineNS.DesignMacross.Design
{
    public class TtMethodUtil
    {
        public static bool TryGetLinkedDataLineWithPin(TtDataPinDescription startPin, TtExpressionDescription expression, out TtDataLineDescription line)
        {
            var fromId = Guid.Empty;
            var fromDescName = "";
            var toId = Guid.Empty;
            var toDescName = "";
            bool existLink = false;
            if (startPin is TtDataInPinDescription)
            {
                var pins = expression.GetDataOutPins(startPin.TypeDesc);
                if (pins.Count > 0)
                {
                    var firstPin = pins[0];
                    fromId = firstPin.Id;
                    fromDescName = firstPin.Parent.Name;
                    toId = startPin.Id;
                    toDescName = startPin.Parent.Name;
                    existLink = true;
                }
            }
            else
            {
                var pins = expression.GetDataInPins(startPin.TypeDesc);
                if (pins.Count > 0)
                {
                    var firstPin = pins[0];
                    fromId = startPin.Id;
                    fromDescName = startPin.Parent.Name;
                    toId = firstPin.Id;
                    toDescName = firstPin.Parent.Name;
                    existLink = true;
                }
            }
            if (existLink)
            {
                line = new TtDataLineDescription() { Name = "Data_" + fromDescName + "_To_" + toDescName, FromId = fromId, ToId = toId };
                return true;
            }
            else
            {
                line = null;
                return false;
            }
        }
        public static bool TryGetLinkedDataLineWithPin(TtDataPinDescription startPin, TtStatementDescription statement, out TtDataLineDescription line)
        {
            var fromId = Guid.Empty;
            var fromDescName = "";
            var toId = Guid.Empty;
            var toDescName = "";
            bool existLink = false;
            if (startPin is TtDataInPinDescription)
            {
                var pins = statement.GetDataOutPins(startPin.TypeDesc);
                if (pins.Count > 0)
                {
                    var firstPin = pins[0];
                    fromId = firstPin.Id;
                    fromDescName = firstPin.Parent.Name;
                    toId = startPin.Id;
                    toDescName = startPin.Parent.Name;
                    existLink = true;
                }
            }
            else
            {
                var pins = statement.GetDataInPins(startPin.TypeDesc);
                if (pins.Count > 0)
                {
                    var firstPin = pins[0];
                    fromId = startPin.Id;
                    fromDescName = startPin.Parent.Name;
                    toId = firstPin.Id;
                    toDescName = firstPin.Parent.Name;
                    existLink = true;
                }
            }
            if (existLink)
            {
                line = new TtDataLineDescription() { Name = "Data_" + fromDescName + "_To_" + toDescName, FromId = fromId, ToId = toId };
                return true;
            }
            else
            {
                line = null;
                return false;
            }
        }
        public static bool TryGetLinkedExecutionLineWithPin(TtExecutionPinDescription startPin, TtExpressionDescription expression, out TtExecutionLineDescription line)
        {
            var fromId = Guid.Empty;
            var fromDescName = "";
            var toId = Guid.Empty;
            var toDescName = "";
            bool existLink = false;
            if (startPin is TtExecutionInPinDescription)
            {
                var pins = expression.GetExecutionOutPins();
                if (pins.Count > 0)
                {
                    var firstPin = pins[0];
                    fromId = firstPin.Id;
                    fromDescName = firstPin.Parent.Name;
                    toId = startPin.Id;
                    toDescName = startPin.Parent.Name;
                    existLink = true;
                }
            }
            else
            {
                var pins = expression.GetExecutionInPins();
                if (pins.Count > 0)
                {
                    var firstPin = pins[0];
                    fromId = startPin.Id;
                    fromDescName = startPin.Parent.Name;
                    toId = firstPin.Id;
                    toDescName = firstPin.Parent.Name;
                    existLink = true;
                }
            }
            if (existLink)
            {
                line = new TtExecutionLineDescription() { Name = "Exec_" + fromDescName + "_To_" + toDescName, FromId = fromId, ToId = toId };
                return true;
            }
            else
            {
                line = null;
                return false;
            }
        }
        public static bool TryGetLinkedExecutionLineWithPin(TtExecutionPinDescription startPin, TtStatementDescription statement, out TtExecutionLineDescription line)
        {
            var fromId = Guid.Empty;
            var fromDescName = "";
            var toId = Guid.Empty;
            var toDescName = "";
            bool existLink = false;
            if (startPin is TtExecutionInPinDescription)
            {
                var pins = statement.GetExecutionOutPins();
                if (pins.Count > 0)
                {
                    var firstPin = pins[0];
                    fromId = firstPin.Id;
                    fromDescName = firstPin.Parent.Name;
                    toId = startPin.Id;
                    toDescName = startPin.Parent.Name;
                    existLink = true;
                }
            }
            else
            {
                var pins = statement.GetExecutionInPins();
                if (pins.Count > 0)
                {
                    var firstPin = pins[0];
                    fromId = startPin.Id;
                    fromDescName = startPin.Parent.Name;
                    toId = firstPin.Id;
                    toDescName = firstPin.Parent.Name;
                    existLink = true;
                }
            }
            if (existLink)
            {
                line = new TtExecutionLineDescription() { Name = "Exec_" + fromDescName + "_To_" + toDescName, FromId = fromId, ToId = toId };
                return true;
            }
            else
            {
                line = null;
                return false;
            }
        }
    }
    public class TtMethodGraphContextMenuUtil
    {
        public static void ConstructMenuItemsAboutAssembly(ref FGraphElementRenderingContext context, TtPopupMenu popupMenu, TtGraph_Method methodGraph)
        {
            //Collect DMC Descriptions with  ContextMenuAttribute

            var methodDescription = methodGraph.MethodDescription;
            var cmdHistory = context.CommandHistory;
            var graphElementStyleManager = context.GraphElementStyleManager;
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
                                                     (TtMenuItem item, object sender) =>
                                                     {
                                                         var popMenu = sender as TtPopupMenu;
                                                         if (Rtti.TtTypeDescManager.CreateInstance(typeDesc) is TtExpressionDescription expression)
                                                         {
                                                             var style = graphElementStyleManager.GetOrAdd(expression.Id, popMenu.PopedPosition);
                                                             cmdHistory.CreateAndExtuteCommand("AddExpression",
                                                                 (data) => { methodDescription.AddExpression(expression); },
                                                                 (data) => { methodDescription.RemoveExpression(expression); });
                                                         }
                                                         if (Rtti.TtTypeDescManager.CreateInstance(typeDesc) is TtStatementDescription statement)
                                                         {
                                                             var style = graphElementStyleManager.GetOrAdd(statement.Id, popMenu.PopedPosition);
                                                             cmdHistory.CreateAndExtuteCommand("AddStatement",
                                                                 (data) => { methodDescription.AddStatement(statement); },
                                                                 (data) => { methodDescription.RemoveStatement(statement); });
                                                         }
                                                     });
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
            foreach (var variable in context.DesignedClassDescription.Variables)
            {
                string[] getMenuPath = { "Self", "Variables", "Get" + variable.Name };
                var getTypeDesc = TtTypeDesc.TypeOf<TtVarGetDescription>();
                TtMenuUtil.ConstructMenuItem(popupMenu.Menu, getTypeDesc, getMenuPath, "",
                                                 (TtMenuItem item, object sender) =>
                                                 {
                                                     var popMenu = sender as TtPopupMenu;
                                                     if (Rtti.TtTypeDescManager.CreateInstance(getTypeDesc) is TtVarGetDescription expression)
                                                     {
                                                         expression.VariableDescription = variable;
                                                         var style = graphElementStyleManager.GetOrAdd(expression.Id, popMenu.PopedPosition);
                                                         cmdHistory.CreateAndExtuteCommand("AddVarGet",
                                                             (data) => { methodDescription.AddExpression(expression); },
                                                             (data) => { methodDescription.RemoveExpression(expression); });
                                                     }
                                                 });

                string[] setMenuPath = { "Self", "Variables", "Set" + variable.Name };
                var setTypeDesc = TtTypeDesc.TypeOf<TtVarSetDescription>();
                TtMenuUtil.ConstructMenuItem(popupMenu.Menu, setTypeDesc, setMenuPath, "",
                                                 (TtMenuItem item, object sender) =>
                                                 {
                                                     var popMenu = sender as TtPopupMenu;
                                                     if (Rtti.TtTypeDescManager.CreateInstance(setTypeDesc) is TtVarSetDescription expression)
                                                     {
                                                         expression.VariableDescription = variable;
                                                         var style = graphElementStyleManager.GetOrAdd(expression.Id, popMenu.PopedPosition);
                                                         cmdHistory.CreateAndExtuteCommand("AddVarSet",
                                                             (data) => { methodDescription.AddExpression(expression); },
                                                             (data) => { methodDescription.RemoveExpression(expression); });
                                                     }
                                                 });

            }
            string[] selfRefMenuPath = { "Self", "SelfReference"};
            var selfRefTypeDesc = TtTypeDesc.TypeOf<TtSelfReferenceDescription>();
            var superClassType = Rtti.TtTypeDescManager.Instance.GetTypeDescFromFullName(context.DesignedClassDescription.SupperClassNames[0]);
            TtMenuUtil.ConstructMenuItem(popupMenu.Menu, selfRefTypeDesc, selfRefMenuPath, "",
                                             (TtMenuItem item, object sender) =>
                                             {
                                                 var popMenu = sender as TtPopupMenu;
                                                 var expression = new TtSelfReferenceDescription(superClassType);
                                                 var style = graphElementStyleManager.GetOrAdd(expression.Id, popMenu.PopedPosition);
                                                 cmdHistory.CreateAndExtuteCommand("AddSelfRef",
                                                     (data) => { methodDescription.AddExpression(expression); },
                                                     (data) => { methodDescription.RemoveExpression(expression); });
                                             });
        }
        public static void ConstructMenuItemsAboutReflection(ref FGraphElementRenderingContext context, TtPopupMenu popupMenu, TtGraph_Method methodGraph)
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
                    var typeDesc = TtTypeDesc.TypeOf<TtMethodInvokeReflectedDescription>();
                    TtMenuUtil.ConstructMenuItem(popupMenu.Menu, typeDesc, menuPath, "",
                                                     (TtMenuItem item, object sender) =>
                                                     {
                                                         var popMenu = sender as TtPopupMenu;
                                                         if (Rtti.TtTypeDescManager.CreateInstance(typeDesc) is TtMethodInvokeReflectedDescription desc)
                                                         {
                                                             desc.SetMethodMeta(methodMeta);
                                                             var style = graphElementStyleManager.GetOrAdd(desc.Id, popMenu.PopedPosition);
                                                             cmdHistory.CreateAndExtuteCommand("AddStatement",
                                                                 (data) => { methodDescription.AddStatement(desc); },
                                                                 (data) => { methodDescription.RemoveStatement(desc); });
                                                         }
                                                     });

                }
            }
        }
    }

    public class TtMethodGraphLinkedPinContextMenuUtil
    {
        public static void ConstructMenuItemsAboutClassPropertiesAndMethods(ref FGraphElementRenderingContext context, TtPopupMenu popupMenu, TtGraph_Method methodGraph)
        {
            var cmdHistory = context.CommandHistory;
            var graphElementStyleManager = context.GraphElementStyleManager;
            var methodDescription = methodGraph.MethodDescription;
            var previewDataLine = methodGraph.PreviewDataLine;
            var previewExecutionLine = methodGraph.PreviewExecutionLine;
            if (previewDataLine != null)
            {
                if (previewDataLine.StartPin is TtSelfReferenceDataPin)
                {
                    return;
                }

                var classType = previewDataLine.StartPin.TypeDesc;
                if (classType == null)
                    return;

                foreach (var property in classType.GetProperties())
                {
                    //Get
                    {
                        string[] menuPath = { classType.Name, "Get" + property.Name };
                        var typeDesc = TtTypeDesc.TypeOf(property.PropertyType);
                        TtPropertyGetDescription getExpression = new(classType, typeDesc);
                        getExpression.Name = property.Name;
                        getExpression.HostReferenceId = previewDataLine.StartPin.Parent.Id;
                        var canLink = TtMethodUtil.TryGetLinkedDataLineWithPin(previewDataLine.StartPin, getExpression, out var line);
                        if (canLink)
                        {
                            TtMenuUtil.ConstructMenuItem(popupMenu.Menu, typeDesc, menuPath, "",
                                                        (TtMenuItem item, object sender) =>
                                                        {
                                                            var popMenu = sender as TtPopupMenu;
                                                            var style = graphElementStyleManager.GetOrAdd(getExpression.Id, popMenu.PopedPosition);
                                                            cmdHistory.CreateAndExtuteCommand("AddExpressionAndDataLink",
                                                                    (data) => { methodDescription.AddExpression(getExpression); methodDescription.AddDataLine(line); },
                                                                    (data) => { methodDescription.RemoveExpression(getExpression); methodDescription.RemoveDataLine(line); });
                                                        });
                        }
                    }
                    //Set
                    {
                        string[] menuPath = { classType.Name, "Set" + property.Name };
                        var typeDesc = TtTypeDesc.TypeOf(property.PropertyType);
                        TtPropertySetDescription setExpression = new(classType, typeDesc);
                        setExpression.Name = property.Name;
                        setExpression.HostReferenceId = previewDataLine.StartPin.Parent.Id;
                        var canLink = TtMethodUtil.TryGetLinkedDataLineWithPin(previewDataLine.StartPin, setExpression, out var line);
                        if (canLink)
                        {
                            TtMenuUtil.ConstructMenuItem(popupMenu.Menu, typeDesc, menuPath, "",
                                                        (TtMenuItem item, object sender) =>
                                                        {
                                                            var popMenu = sender as TtPopupMenu;
                                                            var style = graphElementStyleManager.GetOrAdd(setExpression.Id, popMenu.PopedPosition);
                                                            cmdHistory.CreateAndExtuteCommand("AddExpressionAndDataLink",
                                                                    (data) => { methodDescription.AddExpression(setExpression); methodDescription.AddDataLine(line); },
                                                                    (data) => { methodDescription.RemoveExpression(setExpression); methodDescription.RemoveDataLine(line); });
                                                        });
                        }
                    }

                }
                foreach (var method in classType.GetMethods())
                {
                    string[] menuPath = { classType.Name, method.Name };

                }
            }
        }
        public static void ConstructMenuItemsAboutDesignedClass(ref FGraphElementRenderingContext context, TtPopupMenu popupMenu, TtGraph_Method methodGraph)
        {
            var cmdHistory = context.CommandHistory;
            var graphElementStyleManager = context.GraphElementStyleManager;
            var methodDescription = methodGraph.MethodDescription;
            var previewDataLine = methodGraph.PreviewDataLine;
            var previewExecutionLine = methodGraph.PreviewExecutionLine;
            if (previewDataLine != null)
            {
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
                                var canLink = TtMethodUtil.TryGetLinkedDataLineWithPin(previewDataLine.StartPin, getExpression, out var line);
                                if (canLink)
                                {
                                    TtMenuUtil.ConstructMenuItem(popupMenu.Menu, typeDesc, menuPath, "",
                                                                (TtMenuItem item, object sender) =>
                                                                {
                                                                    var popMenu = sender as TtPopupMenu;
                                                                    var style = graphElementStyleManager.GetOrAdd(getExpression.Id, popMenu.PopedPosition);
                                                                    cmdHistory.CreateAndExtuteCommand("AddExpressionAndDataLink",
                                                                            (data) => { methodDescription.AddExpression(getExpression); methodDescription.AddDataLine(line); },
                                                                            (data) => { methodDescription.RemoveExpression(getExpression); methodDescription.RemoveDataLine(line); });
                                                                });
                                }
                            }
                            //Set
                            {
                                string[] menuPath = { "Self", "Set" + property.Name };
                                var typeDesc = TtTypeDesc.TypeOf(property.PropertyType);
                                TtPropertySetDescription setExpression = new(superClassType, typeDesc);
                                setExpression.Name = property.Name;
                                setExpression.HostReferenceId = previewDataLine.StartPin.Parent.Id;
                                var canLink = TtMethodUtil.TryGetLinkedDataLineWithPin(previewDataLine.StartPin, setExpression, out var line);
                                if (canLink)
                                {
                                    TtMenuUtil.ConstructMenuItem(popupMenu.Menu, typeDesc, menuPath, "",
                                                                (TtMenuItem item, object sender) =>
                                                                {
                                                                    var popMenu = sender as TtPopupMenu;
                                                                    var style = graphElementStyleManager.GetOrAdd(setExpression.Id, popMenu.PopedPosition);
                                                                    cmdHistory.CreateAndExtuteCommand("AddExpressionAndDataLink",
                                                                            (data) => { methodDescription.AddExpression(setExpression); methodDescription.AddDataLine(line); },
                                                                            (data) => { methodDescription.RemoveExpression(setExpression); methodDescription.RemoveDataLine(line); });
                                                                });
                                }
                            }
                        }
                    }
                }
            }
        }
        public static void ConstructMenuItemsAboutAssembly(ref FGraphElementRenderingContext context, TtPopupMenu popupMenu, TtGraph_Method methodGraph)
        {
            var cmdHistory = context.CommandHistory;
            var graphElementStyleManager = context.GraphElementStyleManager;
            var methodDescription = methodGraph.MethodDescription;
            var previewDataLine = methodGraph.PreviewDataLine;
            var previewExecutionLine = methodGraph.PreviewExecutionLine;
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
                                if (previewDataLine != null)
                                {            
                                    var canLink = TtMethodUtil.TryGetLinkedDataLineWithPin(previewDataLine.StartPin, expression, out var line);
                                    if (canLink)
                                    {
                                        TtMenuUtil.ConstructMenuItem(popupMenu.Menu, typeDesc, att.MenuPaths, att.FilterStrings,
                                                                                             (TtMenuItem item, object sender) =>
                                                                                             {
                                                                                                 var popMenu = sender as TtPopupMenu;
                                                                                                 var style = graphElementStyleManager.GetOrAdd(expression.Id, popMenu.PopedPosition);
                                                                                                 cmdHistory.CreateAndExtuteCommand("AddExpressionAndDataLink",
                                                                                                            (data) => { methodDescription.AddExpression(expression); methodDescription.AddDataLine(line); },
                                                                                                            (data) => { methodDescription.RemoveExpression(expression); methodDescription.RemoveDataLine(line); });
                                                                                             });
                                    }
                                }
                                if (previewExecutionLine != null)
                                {
                                    var canLink = TtMethodUtil.TryGetLinkedExecutionLineWithPin(previewExecutionLine.StartPin, expression, out var line);
                                    if (canLink)
                                    {
                                        TtMenuUtil.ConstructMenuItem(popupMenu.Menu, typeDesc, att.MenuPaths, att.FilterStrings,
                                                                                             (TtMenuItem item, object sender) =>
                                                                                             {
                                                                                                 var popMenu = sender as TtPopupMenu;
                                                                                                 var style = graphElementStyleManager.GetOrAdd(expression.Id, popMenu.PopedPosition);
                                                                                                 cmdHistory.CreateAndExtuteCommand("AddExpressionAndExecLink",
                                                                                                            (data) => { methodDescription.AddExpression(expression); methodDescription.AddExecutionLine(line); },
                                                                                                            (data) => { methodDescription.RemoveExpression(expression); methodDescription.RemoveExecutionLine(line); });
                                                                                             });
                                    }
                                }
                            }

                            if (Rtti.TtTypeDescManager.CreateInstance(typeDesc) is TtStatementDescription statement)
                            {
                                if (previewDataLine != null)
                                {
                                    var canLink = TtMethodUtil.TryGetLinkedDataLineWithPin(previewDataLine.StartPin, statement, out var line);
                                    if (canLink)
                                    {
                                        TtMenuUtil.ConstructMenuItem(popupMenu.Menu, typeDesc, att.MenuPaths, att.FilterStrings,
                                                                                             (TtMenuItem item, object sender) =>
                                                                                             {
                                                                                                 var popMenu = sender as TtPopupMenu;
                                                                                                 var style = graphElementStyleManager.GetOrAdd(statement.Id, popMenu.PopedPosition);
                                                                                                 cmdHistory.CreateAndExtuteCommand("AddStatementAndDataLink",
                                                                                                            (data) => { methodDescription.AddStatement(statement); methodDescription.AddDataLine(line); },
                                                                                                            (data) => { methodDescription.RemoveStatement(statement); methodDescription.RemoveDataLine(line); });
                                                                                             });
                                    }
                                }
                                if (previewExecutionLine != null)
                                {
                                    var canLink = TtMethodUtil.TryGetLinkedExecutionLineWithPin(previewExecutionLine.StartPin, statement, out var line);
                                    if (canLink)
                                    {
                                        TtMenuUtil.ConstructMenuItem(popupMenu.Menu, typeDesc, att.MenuPaths, att.FilterStrings,
                                                                                             (TtMenuItem item, object sender) =>
                                                                                             {
                                                                                                 var popMenu = sender as TtPopupMenu;
                                                                                                 var style = graphElementStyleManager.GetOrAdd(statement.Id, popMenu.PopedPosition);
                                                                                                 cmdHistory.CreateAndExtuteCommand("AddStatementAndExecLink",
                                                                                                            (data) => { methodDescription.AddStatement(statement); methodDescription.AddExecutionLine(line); },
                                                                                                            (data) => { methodDescription.RemoveStatement(statement); methodDescription.RemoveExecutionLine(line); });
                                                                                             });
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
        public static void ConstructMenuItemsAboutReflection(ref FGraphElementRenderingContext context, TtPopupMenu popupMenu, TtGraph_Method methodGraph)
        {
            var cmdHistory = context.CommandHistory;
            var graphElementStyleManager = context.GraphElementStyleManager;
            var methodDescription = methodGraph.MethodDescription;
            var previewDataLine = methodGraph.PreviewDataLine;
            var previewExecutionLine = methodGraph.PreviewExecutionLine;
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
                    var typeDesc = TtTypeDesc.TypeOf<TtMethodInvokeReflectedDescription>();
                    if (Rtti.TtTypeDescManager.CreateInstance(typeDesc) is TtMethodInvokeReflectedDescription statement)
                    {
                        statement.SetMethodMeta(methodMeta);
                        string[] menuPath = methodMeta.Meta.MacrossDisplayPath;
                        if (menuPath == null)
                        {
                            menuPath = TtMenuUtil.GetContextPath(methodMeta.DeclaringType, methodMeta.MethodName);
                        }
                        if (previewDataLine != null)
                        {
                            var canLink = TtMethodUtil.TryGetLinkedDataLineWithPin(previewDataLine.StartPin, statement, out var line);
                            if (canLink)
                            {
                                TtMenuUtil.ConstructMenuItem(popupMenu.Menu, typeDesc, menuPath, "",
                                                                                     (TtMenuItem item, object sender) =>
                                                                                     {
                                                                                         var popMenu = sender as TtPopupMenu;
                                                                                         var style = graphElementStyleManager.GetOrAdd(statement.Id, popMenu.PopedPosition);
                                                                                         cmdHistory.CreateAndExtuteCommand("AddStatementAndDataLink",
                                                                                                    (data) => { methodDescription.AddStatement(statement); methodDescription.AddDataLine(line); },
                                                                                                    (data) => { methodDescription.RemoveStatement(statement); methodDescription.RemoveDataLine(line); });
                                                                                     });
                            }
                        }
                        if (previewExecutionLine != null)
                        {
                            var canLink = TtMethodUtil.TryGetLinkedExecutionLineWithPin(previewExecutionLine.StartPin, statement, out var line);
                            if (canLink)
                            {
                                TtMenuUtil.ConstructMenuItem(popupMenu.Menu, typeDesc, menuPath, "",
                                                                                     (TtMenuItem item, object sender) =>
                                                                                     {
                                                                                         var popMenu = sender as TtPopupMenu;
                                                                                         var style = graphElementStyleManager.GetOrAdd(statement.Id, popMenu.PopedPosition);
                                                                                         cmdHistory.CreateAndExtuteCommand("AddStatementAndExecLink",
                                                                                                    (data) => { methodDescription.AddStatement(statement); methodDescription.AddExecutionLine(line); },
                                                                                                    (data) => { methodDescription.RemoveStatement(statement); methodDescription.RemoveExecutionLine(line); });
                                                                                     });
                            }
                        }
                    }
                }
            }
        }
    }
}
