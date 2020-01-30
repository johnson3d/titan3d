using System;
using System.CodeDom;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using EditorCommon.Resources;
using EngineNS;

namespace UIEditor
{
    public class ResourceCreateData : IResourceCreateData
    {
        public string ResourceName { get; set; }
        public ICustomCreateDialog HostDialog { get; set; }
        [Browsable(false)]
        public RName.enRNameType RNameType { get; set; }

        [Browsable(false)]
        public string Description { get; set; }
        public Type ClassType { get; set; }
    }

    [EngineNS.Rtti.MetaClass]
    [EditorCommon.Resources.ResourceInfoAttribute(ResourceInfoType = EngineNS.Editor.Editor_RNameTypeAttribute.UI, ResourceExts = new string[] { ".ui" })]
    public class UIResourceInfo : Macross.ResourceInfos.MacrossResourceInfo,
                                  EditorCommon.Resources.IResourceInfoEditor,
                                  EditorCommon.Resources.IResourceInfoCreateEmpty
    {
        #region ResourceInfo
        public override string ResourceTypeName => "UI";

        public override Brush ResourceTypeBrush => new SolidColorBrush(System.Windows.Media.Color.FromRgb(122, 126, 76));

        public override ImageSource ResourceIcon => new BitmapImage(new System.Uri("pack://application:,,,/ResourceLibrary;component/Icons/Icons/AssetIcons/WidgetBlueprint_64x.png", UriKind.Absolute));

        protected override async Task<bool> DeleteResourceOverride()
        {
            await EngineNS.Thread.AsyncDummyClass.DummyFunc();

            EngineNS.CEngine.Instance.UIManager.RemoveUIFromDic(this.ResourceName);
            if (EngineNS.CEngine.Instance.FileManager.FileExists(ResourceName.Address))
                EngineNS.CEngine.Instance.FileManager.DeleteFile(ResourceName.Address);

            if (EngineNS.CEngine.Instance.FileManager.DirectoryExists(ResourceName.Address))
                EngineNS.CEngine.Instance.FileManager.DeleteDirectory(ResourceName.Address, true);

            return true;
        }

        protected override async Task<bool> MoveToFolderOverride(string absFolder, RName currentResourceName)
        {
            await EngineNS.Thread.AsyncDummyClass.DummyFunc();
            //var absTagDir = absFolder + currentResourceName.PureName(true);
            //var newRName = RName.EditorOnly_GetRNameFromAbsFile(absTagDir);
            //// 拷贝文件
            //EngineNS.CEngine.Instance.FileManager.CopyDirectory(currentResourceName.Address, absTagDir);

            //// 更改cs文件内容
            //var oldTypeName = Macross.Program.GetClassNamespace(currentResourceName) + "." + Macross.Program.GetClassName(currentResourceName);
            //var newTypeName = Macross.Program.GetClassNamespace(newRName) + "." + Macross.Program.GetClassName(newRName);
            //var csFiles = EngineNS.CEngine.Instance.FileManager.GetFiles(absTagDir, "*.cs", System.IO.SearchOption.TopDirectoryOnly);
            //foreach(var file in csFiles)
            //{
            //    var encoding = EngineNS.CEngine.Instance.FileManager.GetEncoding(file);
            //    var content = System.IO.File.ReadAllText(file, encoding);
            //    var newContent = EngineNS.Rtti.RttiHelper.ReplaceWholeWord(content,
            //                                                        "namespace " + Macross.Program.GetClassNamespace(currentResourceName),
            //                                                        "namespace " + Macross.Program.GetClassNamespace(newRName));
            //    newContent = EngineNS.Rtti.RttiHelper.ReplaceWholeWord(newContent, oldTypeName, newTypeName);
            //    System.IO.File.WriteAllText(file, newContent, encoding);
            //}

            //// 设置类型重定向


            return true;
        }

        protected override Task OnReferencedRNameChangedOverride(ResourceInfo referencedResInfo, EngineNS.RName newRName, RName oldRName)
        {
            throw new NotImplementedException();
        }

        protected override Task<bool> RenameOverride(string absFolder, string newName)
        {
            throw new NotImplementedException();
        }
        #endregion
        #region IResourceInfoEditor
        public override string EditorTypeName => "UIEditor";

        public override async Task OpenEditor()
        {
            await EditorCommon.Program.OpenEditor(new ResourceEditorContext(EditorTypeName, this));
        }

        #endregion
        #region IResourceInfoCreateEmpty
        public override string GetValidName(string absFolder)
        {
            return EditorCommon.Program.GetValidName(absFolder, "UI", EngineNS.CEngineDesc.UIExtension);
        }
        //public override ValidationResult ResourceNameAvailable(string absFolder, string name)
        //{
        //    // 判断资源名称是否合法
        //    if (EditorCommon.Program.IsValidRName(name) == false)
        //    {
        //        return new ValidationResult(false, "名称不合法!");
        //    }

        //    return new ValidationResult(true, null);
        //}

        public override IResourceCreateData GetResourceCreateData(string absFolder)
        {
            return new ResourceCreateData();
        }
        public override ICustomCreateDialog GetCustomCreateDialogWindow()
        {
            var retVal = new UIMacross.CreateMacrossWindow();
            retVal.HostResourceInfo = this;
            return retVal;
        }
        public override async Task<ResourceInfo> CreateEmptyResource(string absFolder, string rootFolder, IResourceCreateData createData)
        {
            EditorCommon.Controls.ProcessProgressReportWin.Instance.Title = "正在创建UI，请稍后...";
            EditorCommon.Controls.ProcessProgressReportWin.Instance.ShowReportWin(true);

            var rc = EngineNS.CEngine.Instance.RenderContext;
            var result = new UIResourceInfo();
            var data = createData as ResourceCreateData;
            var rName = EngineNS.CEngine.Instance.FileManager._GetRelativePathFromAbsPath(absFolder + "/" + data.ResourceName, rootFolder);
            rName += EngineNS.CEngineDesc.UIExtension;
            result.ResourceName = EngineNS.RName.GetRName(rName, data.RNameType);
            result.ResourceType = EngineNS.Editor.Editor_RNameTypeAttribute.Macross;
            result.BaseTypeSaveName = EngineNS.Rtti.RttiHelper.GetTypeSaveString(data.ClassType);

            // 创建初始UI界面
            var atts = data.ClassType.GetCustomAttributes(typeof(EngineNS.UISystem.Editor_UIControlInitAttribute), false);
            var initType = ((EngineNS.UISystem.Editor_UIControlInitAttribute)atts[0]).InitializerType;
            var uiInit = System.Activator.CreateInstance(initType) as EngineNS.UISystem.UIElementInitializer;// new EngineNS.UISystem.Controls.UserControlInitializer();
            uiInit.Id = Guid.NewGuid().ToString().GetHashCode();
            var retCtrl = System.Activator.CreateInstance(data.ClassType) as EngineNS.UISystem.UIElement;// new EngineNS.UISystem.Controls.UserControl();
            await retCtrl.Initialize(rc, uiInit);
            var panel = retCtrl as EngineNS.UISystem.Controls.Containers.Panel;
            if(panel != null)
            {
                var canvasInit = new EngineNS.UISystem.Controls.Containers.CanvasPanelInitializer();
                canvasInit.Id = Guid.NewGuid().ToString().GetHashCode();
                var canvas = new EngineNS.UISystem.Controls.Containers.CanvasPanel();
                await canvas.Initialize(rc, canvasInit);
                panel.AddChild(canvas);
            }

            result.CurrentUI = retCtrl;

            // 创建时走一遍编译，保证当前UI能够取到this类型
            var ctrl = new Macross.MacrossLinkControl();
            ctrl.CurrentResourceInfo = this;
            ctrl.CSType = ECSType.Client;
            await result.GenerateCode(ctrl);

            result.CurrentUI = null;

            EditorCommon.Controls.ProcessProgressReportWin.Instance.ShowReportWin(false);
            //var userCtrl = await EngineNS.CEngine.Instance.UIManager.CreateUI(rc, result.ResourceName);
            //var xnd = EngineNS.IO.XndHolder.NewXNDHolder();
            //userCtrl.Save2Xnd(xnd.Node);
            //EngineNS.IO.XndHolder.SaveXND(result.ResourceName.Address, xnd);

            return result;
        }
        public override string CreateMenuPath => "UI/UI";
        public override bool IsBaseResource => false;

        #endregion

        public EngineNS.UISystem.UIElement CurrentUI;
        protected override async Task<ResourceInfo> CreateResourceInfoFromResourceOverride(RName resourceName)
        {
            await EngineNS.Thread.AsyncDummyClass.DummyFunc();
            var retValue = new UIResourceInfo();
            retValue.ResourceName = resourceName;
            retValue.ResourceType = EngineNS.Editor.Editor_RNameTypeAttribute.UI;
            return retValue;
        }
        protected override async Task<bool> InitializeContextMenuOverride(ContextMenu contextMenu)
        {
            await EngineNS.Thread.AsyncDummyClass.DummyFunc();
            var textSeparatorStyle = Application.Current.MainWindow.TryFindResource(new ComponentResourceKey(typeof(ResourceLibrary.CustomResources), "TextMenuSeparatorStyle")) as System.Windows.Style;
            contextMenu.Items.Add(new ResourceLibrary.Controls.Menu.TextSeparator()
            {
                Text = "Common",
                Style = textSeparatorStyle,
            });
            // <MenuItem Header="Add Feature or Content Pack..." menu:MenuAssist.Icon="/ResourceLibrary;component/Icons/Icons/icon_file_saveall_40x.png" Style="{DynamicResource {ComponentResourceKey ResourceId=MenuItem_Default, TypeInTargetAssembly={x:Type res:CustomResources}}}"/>
            var menuItemStyle = Application.Current.MainWindow.TryFindResource(new ComponentResourceKey(typeof(ResourceLibrary.CustomResources), "MenuItem_Default")) as System.Windows.Style;
            var menuItem = new MenuItem()
            {
                Name = "UIResInfo_Delete",
                Header = "删除",
                Style = menuItemStyle,
            };
            menuItem.Click += (object sender, RoutedEventArgs e) =>
            {
                var noUsed = DeleteResource();
            };
            ResourceLibrary.Controls.Menu.MenuAssist.SetIcon(menuItem, new BitmapImage(new System.Uri("pack://application:,,,/ResourceLibrary;component/Icons/Icons/icon_delete_16px.png", UriKind.Absolute)));
            contextMenu.Items.Add(menuItem);

            return true;
        }

        public async Task GenerateCode(Macross.IMacrossOperationContainer ctrl)
        {
            var csType = ECSType.Client;
            var codeGenerator = new Macross.CodeGenerator();
            codeGenerator.GenerateMethodsCategoryNames.Add(UIMacross.MacrossPanel.UIEventFuncCategoryName);
            codeGenerator.GenerateMethodsCategoryNames.Add(UIMacross.MacrossPanel.UIBindFuncCategoryName);
            codeGenerator.GenerateVariables_After_Action = (name, type, pro, cls) =>
            {
                //List<EngineNS.UISystem.VariableBindInfo> bindInfos;
                //if (!CurrentUI.VariableBindInfosDic.TryGetValue(name, out bindInfos))
                //    return;

                //foreach(var bindInfo in bindInfos)
                //{
                //    switch(bindInfo.BindMode)
                //    {
                //        case EngineNS.UISystem.enBindMode.OnWay:
                //        case EngineNS.UISystem.enBindMode.TwoWay:
                //            {
                //                pro.SetStatements.Add(new CodeMethodInvokeExpression(new CodeThisReferenceExpression(), bindInfo.FunctionName_Get, new CodeVariableReferenceExpression("value")));
                //            }
                //            break;
                //    }
                //}
                pro.SetStatements.Add(new CodeGenerateSystem.CodeDom.CodeMethodInvokeExpression(new CodeThisReferenceExpression(), "OnPropertyChanged", new CodePrimitiveExpression($"{pro.Name}")));
            };
            var codeStr = await codeGenerator.GenerateCode(this, ctrl, this.GenerateUIControlCustomCode);
            // 创建目录
            if (!EngineNS.CEngine.Instance.FileManager.DirectoryExists(this.ResourceName.Address))
                EngineNS.CEngine.Instance.FileManager.CreateDirectory(this.ResourceName.Address);
            var codeFile = $"{this.ResourceName.Address}/{this.ResourceName.PureName()}_{csType.ToString()}.cs";
            using (var fs = new System.IO.FileStream(codeFile, System.IO.FileMode.Create, System.IO.FileAccess.ReadWrite, System.IO.FileShare.ReadWrite))
            {
                fs.Write(System.Text.Encoding.Default.GetBytes(codeStr), 0, Encoding.Default.GetByteCount(codeStr));
            }
            await codeGenerator.GenerateAndSaveMacrossCollector(csType);
            var files = codeGenerator.CollectionMacrossProjectFiles(csType);
            codeGenerator.GenerateMacrossProject(files.ToArray(), csType);
            EditorCommon.Program.BuildGameDll(true);
        }
        async Task GenerateUIControlCustomCode(Macross.ResourceInfos.MacrossResourceInfo info, Macross.IMacrossOperationContainer linkCtrl, CodeTypeDeclaration codeClass, CodeNamespace nameSpace)
        {
            await EngineNS.Thread.AsyncDummyClass.DummyFunc();
            // UIInitializer
            var initClassName = info.ResourceName.PureName() + "_Initializer";
            var initClass = new CodeTypeDeclaration(initClassName);
            initClass.IsClass = true;
            initClass.IsPartial = true;
            initClass.CustomAttributes.Add(new CodeAttributeDeclaration(typeof(EngineNS.Rtti.MetaClassAttribute).FullName));

            var constructMethod = new CodeConstructor();
            constructMethod.Attributes = MemberAttributes.Public;
            initClass.Members.Add(constructMethod);
            var baseUICtrlAtts = info.BaseType.GetCustomAttributes(typeof(EngineNS.UISystem.Editor_UIControlInitAttribute), false);
            var currentInitType = ((EngineNS.UISystem.Editor_UIControlInitAttribute)baseUICtrlAtts[0]).InitializerType;
            var defaultInitValue = System.Activator.CreateInstance(currentInitType);
            foreach(var pro in currentInitType.GetProperties())
            {
                var initAtts = pro.GetCustomAttributes(typeof(EngineNS.Rtti.MetaDataAttribute), true);
                if (initAtts.Length == 0)
                    continue;
                // 控件本身的Slot不存
                if (pro.Name == "Slot")
                    continue;

                var proValue = pro.GetValue(CurrentUI.Initializer);
                // 与默认值相等则不设置
                var defaultValue = pro.GetValue(defaultInitValue);
                if (proValue == null && defaultValue == null)
                    continue;
                if (proValue != null && proValue.Equals(defaultValue))
                    continue;

                // 给属性设置数据的代码
                Macross.Program.GenerateSetValueCode(proValue, pro.PropertyType, new CodePropertyReferenceExpression(new CodeThisReferenceExpression(), pro.Name), constructMethod.Statements);
            }

            var atts = info.BaseType.GetCustomAttributes(typeof(EngineNS.UISystem.Editor_UIControlInitAttribute), false);
            var initBaseType = ((EngineNS.UISystem.Editor_UIControlInitAttribute)atts[0]).InitializerType;

            initClass.BaseTypes.Add(initBaseType.FullName);
            nameSpace.Types.Add(initClass);

            // Editor_UIControlInitAttribute
            codeClass.CustomAttributes.Add(new CodeAttributeDeclaration(typeof(EngineNS.UISystem.Editor_UIControlInitAttribute).FullName, new CodeAttributeArgument(new CodeTypeOfExpression(initClassName))));
            // Editor_UIControlAttribute
            codeClass.CustomAttributes.Add(new CodeAttributeDeclaration(typeof(EngineNS.UISystem.Editor_UIControlAttribute).FullName,
                                                                                                            new CodeAttributeArgument(new CodePrimitiveExpression("自定义控件." + EngineNS.CEngine.Instance.FileManager.RemoveExtension(info.ResourceName.Name).Replace("/", "."))),
                                                                                                            new CodeAttributeArgument(new CodePrimitiveExpression(info.Description)),
                                                                                                            new CodeAttributeArgument(new CodePrimitiveExpression("UserWidget.png"))));

            // InitControls
            var initCtrlsMethod = new CodeMemberMethod();
            initCtrlsMethod.Attributes = MemberAttributes.Family;
            initCtrlsMethod.Name = "InitControls";
            initCtrlsMethod.ReturnType = new CodeTypeReference("async System.Threading.Tasks.Task");
            initCtrlsMethod.Parameters.Add(new CodeParameterDeclarationExpression(typeof(EngineNS.CRenderContext), "rc"));

            // Event
            GenerateUIElementEventCode(CurrentUI, initCtrlsMethod.Statements, codeClass);
            // 参数绑定-----------------
            GeneratePorpertyBindCode(CurrentUI, initCtrlsMethod.Statements, codeClass);
            mBindInfosWithUIElement.Clear();
            GenerateVariableBindCode(CurrentUI, initCtrlsMethod.Statements, codeClass);
            // 计算mCurrentUI的子
            var panel = CurrentUI as EngineNS.UISystem.Controls.Containers.Panel;
            if(panel != null)
            {
                initCtrlsMethod.Statements.Add(new CodeExpressionStatement(new CodeSnippetExpression("mChildrenUIElements.Clear()")));
                foreach (var child in panel.ChildrenUIElements)
                {
                    GenerateUIElementCreateCode(child, new CodeThisReferenceExpression(), initCtrlsMethod.Statements, codeClass);
                }
            }

            foreach(var item in mBindInfosWithUIElement)
            {
                var uiElementNameStr = "ui_" + item.Key.Id.ToString().Replace("-", "_");
                CodeExpression uiElementNameExp;
                if (item.Key == CurrentUI)
                    uiElementNameExp = new CodeThisReferenceExpression();
                else
                    uiElementNameExp = new CodeVariableReferenceExpression(uiElementNameStr);
                var uiElementType = item.Key.GetType();

                var variableChangedCallbackFuncName = uiElementNameStr + "_VariableChangedCallback";
                initCtrlsMethod.Statements.Add(new CodeAttachEventStatement(uiElementNameExp, "UIPropertyChanged",
                                                                                            new CodeMethodReferenceExpression(new CodeThisReferenceExpression(), variableChangedCallbackFuncName)));

                var varChangedCBFunc = new CodeMemberMethod();
                varChangedCBFunc.Attributes = MemberAttributes.Private;
                varChangedCBFunc.Name = variableChangedCallbackFuncName;
                //varChangedCBFunc.Parameters.Add(new CodeParameterDeclarationExpression(typeof(object), "sender"));
                varChangedCBFunc.Parameters.Add(new CodeParameterDeclarationExpression(typeof(EngineNS.UISystem.UIElement.stUIPropertyChangedData), "e"));
                codeClass.Members.Add(varChangedCBFunc);

                foreach(var varBindInfo in item.Value)
                {
                    CodeExpression fromUIElementFieldExp;
                    if (varBindInfo.BindFromUIElementId == CurrentUI.Id)
                        fromUIElementFieldExp = new CodeThisReferenceExpression();
                    else
                        fromUIElementFieldExp = new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), $"mUI_ValueBind_{varBindInfo.BindFromUIElementId.ToString().Replace("-", "_")}");

                    var conditionStatement = new CodeConditionStatement();
                    varChangedCBFunc.Statements.Add(conditionStatement);
                    conditionStatement.Condition = new CodeBinaryOperatorExpression(new CodeFieldReferenceExpression(new CodeVariableReferenceExpression("e"), "PropertyName"),
                                                                                    CodeBinaryOperatorType.ValueEquality,
                                                                                    new CodePrimitiveExpression(varBindInfo.VariableName));
                    conditionStatement.TrueStatements.Add(new CodeVariableDeclarationStatement(uiElementType, "temp", new CodeGenerateSystem.CodeDom.CodeCastExpression(uiElementType, new CodeVariableReferenceExpression("e.SenderUI"))));
                    if(varBindInfo.BindFromPropertyType == varBindInfo.BindToVariableType)
                    {
                        var equalCondition = new CodeConditionStatement();
                        equalCondition.Condition = new CodeBinaryOperatorExpression(new CodePropertyReferenceExpression(new CodeVariableReferenceExpression("temp"), varBindInfo.VariableName),
                                                                                    CodeBinaryOperatorType.ValueEquality,
                                                                                    new CodePropertyReferenceExpression(fromUIElementFieldExp, varBindInfo.BindFromPropertyName));
                        equalCondition.TrueStatements.Add(new CodeMethodReturnStatement());
                        conditionStatement.TrueStatements.Add(equalCondition);
                    }
                    string outValueVarStr = "temp__outValue";
                    conditionStatement.TrueStatements.Add(new CodeVariableDeclarationStatement(varBindInfo.BindFromPropertyType, outValueVarStr));
                    conditionStatement.TrueStatements.Add(new CodeGenerateSystem.CodeDom.CodeMethodInvokeExpression(new CodeThisReferenceExpression(), varBindInfo.FunctionName_Get,
                                                                                                new CodeVariableReferenceExpression("temp"),
                                                                                                new CodeFieldReferenceExpression(new CodeVariableReferenceExpression("temp"), varBindInfo.VariableName),
                                                                                                new CodeSnippetExpression($"out {outValueVarStr}")));
                    conditionStatement.TrueStatements.Add(new CodeAssignStatement(new CodePropertyReferenceExpression(fromUIElementFieldExp, varBindInfo.BindFromPropertyName),
                                                                                  new CodeVariableReferenceExpression(outValueVarStr)));
                    conditionStatement.TrueStatements.Add(new CodeMethodReturnStatement());
                }
            }

            codeClass.Members.Add(initCtrlsMethod);

            // override Initialize
            var initMethod = new CodeMemberMethod();
            initMethod.Attributes = MemberAttributes.Override | MemberAttributes.Public;
            initMethod.Name = "Initialize";
            initMethod.ReturnType = new CodeTypeReference("async System.Threading.Tasks.Task<bool>");
            initMethod.Parameters.Add(new CodeParameterDeclarationExpression(typeof(EngineNS.CRenderContext), "rc"));
            initMethod.Parameters.Add(new CodeParameterDeclarationExpression(typeof(EngineNS.UISystem.UIElementInitializer), "init"));

            initMethod.Statements.Add(new CodeGenerateSystem.CodeDom.CodeMethodInvokeExpression(new CodeVariableReferenceExpression("await this"), "InitControls", new CodeVariableReferenceExpression("rc")));
            initMethod.Statements.Add(new CodeMethodReturnStatement(new CodeGenerateSystem.CodeDom.CodeMethodInvokeExpression(new CodeVariableReferenceExpression("await base"), "Initialize",
                                                                                                                new CodeVariableReferenceExpression("rc"),
                                                                                                                new CodeVariableReferenceExpression("init"))));
            codeClass.Members.Add(initMethod);
        }

        void GenerateUIElementCreateCode(EngineNS.UISystem.UIElement uiElement, CodeExpression parentParamExp, CodeStatementCollection codeStatements, CodeTypeDeclaration codeClass)
        {
            var uiIdStr = uiElement.Id.ToString().Replace("-", "_");
            var uiElementType = uiElement.GetType();
            var initerType = uiElement.Initializer.GetType();
            var defaultIniter = System.Activator.CreateInstance(initerType);
            // var initXX = new XX;
            var initParamName = "uiInit_" + uiIdStr;
            var newInitCode = new CodeVariableDeclarationStatement(initerType, initParamName, new CodeObjectCreateExpression(initerType));
            codeStatements.Add(newInitCode);
            foreach(var pro in initerType.GetProperties())
            {
                if (!pro.SetMethod.IsPublic)
                    continue;
                var atts = pro.GetCustomAttributes(typeof(EngineNS.Rtti.MetaDataAttribute), true);
                if (atts.Length == 0)
                    continue;

                var proValue = pro.GetValue(uiElement.Initializer);
                // 与默认值相等则不设置
                var defaultValue = pro.GetValue(defaultIniter);
                if (proValue == null && defaultValue == null)
                    continue;
                if (proValue != null && proValue.Equals(defaultValue))
                    continue;

                // 给属性设置数据的代码
                var proValueType = pro.PropertyType;
                if (proValue != null)
                    proValueType = proValue.GetType();
                if(proValueType.IsEnum)
                {
                    Macross.Program.GenerateSetValueCode(proValue, pro.PropertyType, new CodePropertyReferenceExpression(new CodeVariableReferenceExpression(initParamName), pro.Name), codeStatements);
                }
                else if((proValueType.IsClass && proValueType != typeof(string)) || (proValueType.IsValueType && !proValueType.IsPrimitive))
                {
                    var proName = pro.Name + "_" + uiIdStr;
                    var proNameExp = new CodeVariableReferenceExpression(proName);
                    codeStatements.Add(new CodeVariableDeclarationStatement(proValueType, proName));
                    Macross.Program.GenerateSetValueCode(proValue, pro.PropertyType, proNameExp, codeStatements);
                    codeStatements.Add(new CodeAssignStatement(new CodePropertyReferenceExpression(new CodeVariableReferenceExpression(initParamName), pro.Name), proNameExp));
                }
                else
                {
                    Macross.Program.GenerateSetValueCode(proValue, pro.PropertyType, new CodePropertyReferenceExpression(new CodeVariableReferenceExpression(initParamName), pro.Name), codeStatements);
                }
            }
            foreach(var field in initerType.GetFields())
            {
                var atts = field.GetCustomAttributes(typeof(EngineNS.Rtti.MetaDataAttribute), true);
                if (atts.Length == 0)
                    continue;

                var fieldValue = field.GetValue(uiElement.Initializer);
                // 与默认值相等则不设置
                var defaultValue = field.GetValue(defaultIniter);
                if (fieldValue == null && defaultValue == null)
                    continue;
                if (fieldValue != null && fieldValue.Equals(defaultValue))
                    continue;

                // 设置数据
                var fieldValueType = field.FieldType;
                if (fieldValue != null)
                    fieldValueType = fieldValue.GetType();
                if(fieldValueType.IsEnum)
                {
                    Macross.Program.GenerateSetValueCode(fieldValue, field.FieldType, new CodePropertyReferenceExpression(new CodeVariableReferenceExpression(initParamName), field.Name), codeStatements);
                }
                else if((fieldValueType.IsClass && fieldValueType != typeof(string)) || (fieldValueType.IsValueType && !fieldValueType.IsPrimitive))
                {
                    var fieldName = field.Name + "_" + uiIdStr;
                    var fieldNameExp = new CodeVariableReferenceExpression(fieldName);
                    codeStatements.Add(new CodeVariableDeclarationStatement(fieldValueType, fieldName));
                    Macross.Program.GenerateSetValueCode(fieldValue, field.FieldType, fieldNameExp, codeStatements);
                    codeStatements.Add(new CodeAssignStatement(new CodePropertyReferenceExpression(new CodeVariableReferenceExpression(initParamName), field.Name), fieldNameExp));
                }
                else
                {
                    Macross.Program.GenerateSetValueCode(fieldValue, field.FieldType, new CodePropertyReferenceExpression(new CodeVariableReferenceExpression(initParamName), field.Name), codeStatements);
                }
            }

            var uiElementNameStr = "ui_" + uiIdStr;
            codeStatements.Add(new CodeVariableDeclarationStatement(uiElementType, uiElementNameStr, new CodeObjectCreateExpression(uiElementType)));
            codeStatements.Add(new CodeGenerateSystem.CodeDom.CodeMethodInvokeExpression(new CodeVariableReferenceExpression("await " + uiElementNameStr), "Initialize",
                                                                        new CodeVariableReferenceExpression("rc"),
                                                                        new CodeVariableReferenceExpression(initParamName)));

            // Variable
            if(uiElement.Initializer.IsVariable)
            {
                codeStatements.Add(new CodeAssignStatement(new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), "mUI_" + uiElement.Initializer.Name), new CodeVariableReferenceExpression(uiElementNameStr)));

                var proField = new CodeMemberField(uiElement.GetType(), "mUI_" + uiElement.Initializer.Name);
                codeClass.Members.Add(proField);
                var proCode = new CodeMemberProperty();
                proCode.CustomAttributes.Add(new CodeAttributeDeclaration(new CodeTypeReference(typeof(BrowsableAttribute)), new CodeAttributeArgument(new CodePrimitiveExpression(false))));
                proCode.CustomAttributes.Add(new CodeAttributeDeclaration(new CodeTypeReference(typeof(EngineNS.Editor.MacrossMemberAttribute)), 
                                                                    new CodeAttributeArgument(new CodePrimitiveExpression(EngineNS.Editor.MacrossMemberAttribute.enMacrossType.PropReadOnly))));
                proCode.Attributes = MemberAttributes.Public;
                proCode.Type = new CodeTypeReference(uiElement.GetType());
                proCode.Name = uiElement.Initializer.Name;
                proCode.HasGet = true;
                proCode.GetStatements.Add(new CodeMethodReturnStatement(new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), "mUI_" + uiElement.Initializer.Name)));
                proCode.HasSet = false;
                codeClass.Members.Add(proCode);
            }

            // Event
            GenerateUIElementEventCode(uiElement, codeStatements, codeClass);
            // 参数绑定-----------------
            GeneratePorpertyBindCode(uiElement, codeStatements, codeClass);
            GenerateVariableBindCode(uiElement, codeStatements, codeClass);

            codeStatements.Add(new CodeGenerateSystem.CodeDom.CodeMethodInvokeExpression(parentParamExp, "AddChild", new CodeVariableReferenceExpression(uiElementNameStr), new CodePrimitiveExpression(false)));

            var panel = uiElement as EngineNS.UISystem.Controls.Containers.Panel;
            if(panel != null)
            {
                if(!((uiElement is EngineNS.UISystem.Controls.UserControl) && uiElement != CurrentUI))
                foreach(var child in panel.ChildrenUIElements)
                {
                    GenerateUIElementCreateCode(child, new CodeVariableReferenceExpression(uiElementNameStr), codeStatements, codeClass);
                }
            }
        }
        void GenerateUIElementEventCode(EngineNS.UISystem.UIElement uiElement, CodeStatementCollection codeStatements, CodeTypeDeclaration codeClass)
        {
            var uiElementType = uiElement.GetType();
            var uiIdStr = uiElement.Id.ToString().Replace("-", "_");
            CodeExpression uiElementNameExp;
            if (uiElement == CurrentUI)
                uiElementNameExp = new CodeThisReferenceExpression();
            else
                uiElementNameExp = new CodeVariableReferenceExpression("ui_" + uiIdStr);
            foreach (var field in uiElementType.GetFields())
            {
                var atts = field.GetCustomAttributes(typeof(EngineNS.UISystem.Editor_UIEvent), true);
                if (atts.Length == 0)
                    continue;
                if (!field.FieldType.IsSubclassOf(typeof(System.Delegate)))
                    continue;
                var key = new UIEventDicKey(uiElement.Id, field.Name);
                string functionName;
                if (!this.UIEventsDic.TryGetValue(key, out functionName))
                    continue;

                codeStatements.Add(new CodeAssignStatement(new CodeFieldReferenceExpression(uiElementNameExp, field.Name),
                                                           new CodeVariableReferenceExpression(functionName)));
            }
        }
        void GeneratePorpertyBindCode(EngineNS.UISystem.UIElement uiElement, CodeStatementCollection codeStatements, CodeTypeDeclaration codeClass)
        {
            var uiElementType = uiElement.GetType();
            var uiIdStr = uiElement.Id.ToString().Replace("-", "_");
            var uiElementNameStr = "ui_" + uiIdStr;
            CodeExpression uiElementNameExp;
            if (uiElement == CurrentUI)
                uiElementNameExp = new CodeThisReferenceExpression();
            else
                uiElementNameExp = new CodeVariableReferenceExpression(uiElementNameStr);

            // PropertyCustomBindFunctions
            codeStatements.Add(new CodeGenerateSystem.CodeDom.CodeMethodInvokeExpression(new CodeFieldReferenceExpression(uiElementNameExp, "PropertyBindFunctions"), "Clear"));
            if (uiElement.PropertyBindFunctions.Count > 0)
            {
                var propertyChangedCallbackFuncName = uiElementNameStr + "_PropertyChangedCallback";
                var proChangedCBFunc = new CodeMemberMethod();
                proChangedCBFunc.Attributes = MemberAttributes.Private;
                proChangedCBFunc.Name = propertyChangedCallbackFuncName;
                //proChangedCBFunc.Parameters.Add(new CodeParameterDeclarationExpression(typeof(object), "sender"));
                proChangedCBFunc.Parameters.Add(new CodeParameterDeclarationExpression(typeof(EngineNS.UISystem.UIElement.stUIPropertyChangedData), "e"));
                codeClass.Members.Add(proChangedCBFunc);


                foreach (var proFunc in uiElement.PropertyBindFunctions)
                {
                    codeStatements.Add(new CodeGenerateSystem.CodeDom.CodeMethodInvokeExpression(new CodeFieldReferenceExpression(uiElementNameExp, "PropertyBindFunctions"),
                                                                                       "Add",
                                                                                       new CodePrimitiveExpression(proFunc.Key),
                                                                                       new CodePrimitiveExpression(proFunc.Value)));
                    var conditionStat = new CodeConditionStatement();
                    proChangedCBFunc.Statements.Add(conditionStat);
                    conditionStat.Condition = new CodeBinaryOperatorExpression(new CodeFieldReferenceExpression(new CodeVariableReferenceExpression("e"), "PropertyName"),
                                                                               CodeBinaryOperatorType.ValueEquality,
                                                                               new CodePrimitiveExpression(proFunc.Key));
                    conditionStat.TrueStatements.Add(new CodeVariableDeclarationStatement(uiElementType, "temp", new CodeGenerateSystem.CodeDom.CodeCastExpression(uiElementType, new CodeVariableReferenceExpression("e.SenderUI"))));
                    conditionStat.TrueStatements.Add(new CodeGenerateSystem.CodeDom.CodeMethodInvokeExpression(new CodeThisReferenceExpression(), proFunc.Value,
                                                                                                new CodeVariableReferenceExpression("temp"),
                                                                                                new CodeFieldReferenceExpression(new CodeVariableReferenceExpression("temp"), proFunc.Key)));
                    conditionStat.TrueStatements.Add(new CodeMethodReturnStatement());
                }

                codeStatements.Add(new CodeAttachEventStatement(uiElementNameExp, "UIPropertyChanged",
                                                                new CodeMethodReferenceExpression(new CodeThisReferenceExpression(), propertyChangedCallbackFuncName)));

            }
        }

        Dictionary<EngineNS.UISystem.UIElement, List<EngineNS.UISystem.VariableBindInfo>> mBindInfosWithUIElement = new Dictionary<EngineNS.UISystem.UIElement, List<EngineNS.UISystem.VariableBindInfo>>();
        void GenerateVariableBindCode(EngineNS.UISystem.UIElement uiElement, CodeStatementCollection codeStatements, CodeTypeDeclaration codeClass)
        {
            var uiElementType = uiElement.GetType();
            var uiIdStr = uiElement.Id.ToString().Replace("-", "_");
            CodeExpression uiElementNameExp;
            if (uiElement == CurrentUI)
                uiElementNameExp = new CodeThisReferenceExpression();
            else
                uiElementNameExp = new CodeVariableReferenceExpression("ui_" + uiIdStr);

            // VariableBind
            codeStatements.Add(new CodeGenerateSystem.CodeDom.CodeMethodInvokeExpression(new CodeFieldReferenceExpression(uiElementNameExp, "VariableBindInfosDic"), "Clear"));
            if (uiElement.VariableBindInfosDic.Count > 0)
            {
                foreach (var varFunc in uiElement.VariableBindInfosDic)
                {
                    var listName = "list_" + Guid.NewGuid().ToString().Replace("-", "_");
                    var listType = typeof(System.Collections.Generic.List<EngineNS.UISystem.VariableBindInfo>);
                    codeStatements.Add(new CodeVariableDeclarationStatement(listType, listName, new CodeObjectCreateExpression(listType)));
                    foreach (var bindInfo in varFunc.Value)
                    {
                        var valName = "varBind_" + Guid.NewGuid().ToString().Replace("-", "_");
                        var valType = typeof(EngineNS.UISystem.VariableBindInfo);
                        codeStatements.Add(new CodeVariableDeclarationStatement(valType, valName, new CodeObjectCreateExpression(valType)));

                        codeStatements.Add(new CodeAssignStatement(new CodePropertyReferenceExpression(new CodeVariableReferenceExpression(valName), "BindFromUIElementId"), new CodePrimitiveExpression(bindInfo.BindFromUIElementId)));
                        codeStatements.Add(new CodeAssignStatement(new CodePropertyReferenceExpression(new CodeVariableReferenceExpression(valName), "BindFromPropertyName"), new CodePrimitiveExpression(bindInfo.BindFromPropertyName)));
                        codeStatements.Add(new CodeAssignStatement(new CodePropertyReferenceExpression(new CodeVariableReferenceExpression(valName), "BindFromPropertyType"), new CodeTypeOfExpression(bindInfo.BindFromPropertyType)));
                        codeStatements.Add(new CodeAssignStatement(new CodePropertyReferenceExpression(new CodeVariableReferenceExpression(valName), "BindToUIElementId"), new CodePrimitiveExpression(bindInfo.BindToUIElementId)));
                        codeStatements.Add(new CodeAssignStatement(new CodePropertyReferenceExpression(new CodeVariableReferenceExpression(valName), "BindToVariableType"), new CodeTypeOfExpression(bindInfo.BindToVariableType)));
                        codeStatements.Add(new CodeAssignStatement(new CodePropertyReferenceExpression(new CodeVariableReferenceExpression(valName), "VariableName"), new CodePrimitiveExpression(bindInfo.VariableName)));
                        codeStatements.Add(new CodeAssignStatement(new CodePropertyReferenceExpression(new CodeVariableReferenceExpression(valName), "BindMode"), new CodeSnippetExpression(EngineNS.Rtti.RttiHelper.GetAppTypeString(bindInfo.BindMode.GetType()) + "." + bindInfo.BindMode.ToString())));
                        codeStatements.Add(new CodeAssignStatement(new CodePropertyReferenceExpression(new CodeVariableReferenceExpression(valName), "FunctionName_Get"), new CodePrimitiveExpression(bindInfo.FunctionName_Get)));
                        codeStatements.Add(new CodeAssignStatement(new CodePropertyReferenceExpression(new CodeVariableReferenceExpression(valName), "FunctionName_Set"), new CodePrimitiveExpression(bindInfo.FunctionName_Set)));
                        codeStatements.Add(new CodeGenerateSystem.CodeDom.CodeMethodInvokeExpression(new CodeVariableReferenceExpression(listName), "Add", new CodeVariableReferenceExpression(valName)));

                        EngineNS.UISystem.UIElement targetUI;
                        if (CurrentUI.Id == bindInfo.BindToUIElementId)
                            targetUI = CurrentUI;
                        else
                            targetUI = CurrentUI.FindChildElement(bindInfo.BindToUIElementId);
                        List<EngineNS.UISystem.VariableBindInfo> infoValues;
                        if(!mBindInfosWithUIElement.TryGetValue(targetUI, out infoValues))
                        {
                            infoValues = new List<EngineNS.UISystem.VariableBindInfo>();
                            mBindInfosWithUIElement[targetUI] = infoValues;
                        }
                        infoValues.Add(bindInfo);
                    }
                    codeStatements.Add(new CodeGenerateSystem.CodeDom.CodeMethodInvokeExpression(new CodeFieldReferenceExpression(uiElementNameExp, "VariableBindInfosDic"),
                                                                                        "Add",
                                                                                        new CodePrimitiveExpression(varFunc.Key),
                                                                                        new CodeVariableReferenceExpression(listName)));
                }

                if(uiElement != CurrentUI)
                {
                    var uiValueBindFieldName = $"mUI_ValueBind_{uiIdStr}";
                    var uiFieldCode = new CodeMemberField(uiElementType, uiValueBindFieldName);
                    codeClass.Members.Add(uiFieldCode);

                    codeStatements.Add(new CodeAssignStatement(new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), uiValueBindFieldName), uiElementNameExp));
                }
            }
        }

        public class UIEventDicKey
        {
            public int UIElementId;
            public string EventName;

            public UIEventDicKey(int id, string eventName)
            {
                UIElementId = id;
                EventName = eventName;
            }
            public override bool Equals(object obj)
            {
                var key = obj as UIEventDicKey;
                if (key.UIElementId == UIElementId &&
                   key.EventName == EventName)
                    return true;
                return false;
            }
            public override int GetHashCode()
            {
                return (UIElementId.ToString() + EventName).GetHashCode();
            }
        }
        public Dictionary<UIEventDicKey, string> UIEventsDic
        {
            get;
            set;
        } = new Dictionary<UIEventDicKey, string>();

        public override async Task Save(bool withSnapshot = false)
        {
            Version++;

            ReferenceRNameList.Clear();
            RefreshReferenceRNames(CurrentUI);

            var saver = EngineNS.IO.XmlHolder.NewXMLHolder(this.GetType().FullName, "");
            WriteObjectXML(saver.RootNode);
            var dicNode = saver.RootNode.AddNode("UIEventDic", "", saver);
            foreach(var element in UIEventsDic)
            {
                var dataNode = dicNode.AddNode("data", "", saver);
                dataNode.AddAttrib("Value", element.Value);
                dataNode.AddAttrib("Id", element.Key.UIElementId.ToString());
                dataNode.AddAttrib("Name", element.Key.EventName);
            }
            
            EngineNS.IO.XmlHolder.SaveXML(AbsInfoFileName, saver);
            IsDirty = false;
            if (withSnapshot)
                await GetSnapshotImage(true);
        }
        public void RefreshReferenceRNames(EngineNS.UISystem.UIElement curUI)
        {
            if (curUI.CurrentBrush != null)
            {
                if (curUI.CurrentBrush.UVAnim != null)
                {
                    if(ReferenceRNameList.Contains(curUI.CurrentBrush.ImageName)==false)
                        ReferenceRNameList.Add(curUI.CurrentBrush.ImageName);
                }
            }

            var textBlock = curUI as EngineNS.UISystem.Controls.TextBlock;
            if (textBlock != null)
            {
                if (ReferenceRNameList.Contains(textBlock.Font) == false)
                    ReferenceRNameList.Add(textBlock.Font);
            }

            var button = curUI as EngineNS.UISystem.Controls.Button;
            if (button != null && button.ButtonStyle != null)
            {
                if (ReferenceRNameList.Contains(button.ButtonStyle.NormalBrush.ImageName) == false)
                    ReferenceRNameList.Add(button.ButtonStyle.NormalBrush.ImageName);
                if (ReferenceRNameList.Contains(button.ButtonStyle.PressedBrush.ImageName) == false)
                    ReferenceRNameList.Add(button.ButtonStyle.PressedBrush.ImageName);
                if (ReferenceRNameList.Contains(button.ButtonStyle.HoveredBrush.ImageName) == false)
                    ReferenceRNameList.Add(button.ButtonStyle.HoveredBrush.ImageName);
                if (ReferenceRNameList.Contains(button.ButtonStyle.DisabledBrush.ImageName) == false)
                    ReferenceRNameList.Add(button.ButtonStyle.DisabledBrush.ImageName);
            }

            var joystick = curUI as EngineNS.UISystem.Controls.Joysticks;
            if (joystick != null)
            {
                if (joystick.BackgroundBrush != null)
                {
                    if (ReferenceRNameList.Contains(joystick.BackgroundBrush.ImageName) == false)
                        ReferenceRNameList.Add(joystick.BackgroundBrush.ImageName);
                }
                if (joystick.ThumbStyle != null)
                {
                    if (ReferenceRNameList.Contains(joystick.ThumbStyle.NormalBrush.ImageName) == false)
                        ReferenceRNameList.Add(joystick.ThumbStyle.NormalBrush.ImageName);
                    if (ReferenceRNameList.Contains(joystick.ThumbStyle.PressedBrush.ImageName) == false)
                        ReferenceRNameList.Add(joystick.ThumbStyle.PressedBrush.ImageName);
                    if (ReferenceRNameList.Contains(joystick.ThumbStyle.HoveredBrush.ImageName) == false)
                        ReferenceRNameList.Add(joystick.ThumbStyle.HoveredBrush.ImageName);
                    if (ReferenceRNameList.Contains(joystick.ThumbStyle.DisabledBrush.ImageName) == false)
                        ReferenceRNameList.Add(joystick.ThumbStyle.DisabledBrush.ImageName);
                }
            }

            var container = curUI as EngineNS.UISystem.Controls.Containers.Panel;
            if(container!=null)
            {
                foreach (var i in container.ChildrenUIElements)
                {
                    RefreshReferenceRNames(i);
                }
            }
        }
        public override async Task<bool> AsyncLoad(string absFileName)
        {
            var holder = EngineNS.IO.XmlHolder.LoadXML(absFileName);
            if (holder == null)
                return false;

            mIsLoading = true;
            ReadObjectXML(holder.RootNode);
            var dicNode = holder.RootNode.FindNode("UIEventDic");
            if(dicNode != null)
            {
                foreach (var cNode in dicNode.FindNodes("data"))
                {
                    var att = cNode.FindAttrib("Id");
                    if (att == null)
                        continue;
                    int id = System.Convert.ToInt32(att.Value);
                    att = cNode.FindAttrib("Name");
                    if (att == null)
                        continue;
                    string eventName = att.Value;
                    var key = new UIEventDicKey(id, eventName);
                    att = cNode.FindAttrib("Value");
                    if (att == null)
                        continue;
                    var value = att.Value;
                    UIEventsDic[key] = value;
                }
            }
            absFileName = EngineNS.CEngine.Instance.FileManager.RemoveExtension(absFileName);
            ResourceName = EngineNS.RName.EditorOnly_GetRNameFromAbsFile(absFileName);

            // 有时需要直接读rinfo文件来做一些处理，不一定在界面中，所以这里如果ParentBorwser为空则不初始化菜单
            if (ParentBrowser != null)
                await InitializeContextMenu();

            mIsLoading = false;
            return true;
        }
    }
}
