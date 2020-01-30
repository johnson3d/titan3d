using System;
using System.Collections.Generic;
using System.CodeDom;
using Microsoft.CSharp;
using System.CodeDom.Compiler;
using System.Threading.Tasks;
using System.Text;
using static CodeDomNode.MethodInvokeParameterControl;
using System.Xml;

namespace Macross
{
    public class CodeGenerator
    {
        #region 生成类参考
        /*/ 生成类参考-------------------------------------------
        public class NormalClass
        {

        }
        public struct NormalStruct
        {

        }

        [EngineNS.Macross.MacrossTypeClassAttribute]
        public class MacrossClass : EngineNS.Macross.IMacrossType
        {
            public class DebugContext
            {
                public int DebugCollecterValue = 0;
            }
            [ThreadStatic]
            static DebugContext Debugger = new DebugContext();
            static DebugContext DebuggerUsed;


            public int Version
            {
                get;
                set;
            }
            static EngineNS.Hash64 mClassId = EngineNS.Hash64.Empty;
            static EngineNS.Hash64 ClassId
            {
                get
                {
                    if (mClassId == EngineNS.Hash64.Empty)
                    {
                        mClassId = new EngineNS.Hash64();
                        EngineNS.Hash64.CalcHash64(ref mClassId, "Macross_AActor");
                    }
                    return mClassId;
                }
            }

            public LinkedListNode<EngineNS.Macross.MacrossHandle> MacrossData
            {
                get;
                set;
            }
            public void InitMacrossData()
            {
            }
            public async System.Threading.Tasks.Task XXXFunction()
            {
                DebuggerUsed = Debugger;
#if MacrossDebug

#endif
                await EngineNS.CEngine.Instance.EventPoster.Post(() =>
                {
                    return true;
                }, EngineNS.Thread.Async.EAsyncTarget.AsyncIO);
            }
        }
        [EngineNS.Macross.MacrossTypeClassAttribute]
        public class Macross_AActor : EngineNS.GamePlay.Actor.GActor, EngineNS.Macross.IMacrossType
        {
            public int Version
            {
                get;
                set;
            }
            static EngineNS.Hash64 mClassId = EngineNS.Hash64.Empty;
            public static EngineNS.Hash64 ClassId
            {
                get
                {
                    if(mClassId == EngineNS.Hash64.Empty)
                    {
                        mClassId = new EngineNS.Hash64();
                        EngineNS.Hash64.CalcHash64(ref mClassId, "Macross_AActor");
                    }
                    return mClassId;
                }
            }
            // 再Macross类属性数量、类型、名称改变时版本号改变，用于类更新后的数据刷新
            public static readonly UInt32 FieldsVersion = 0;

            private LinkedListNode<EngineNS.Macross.MacrossHandle> mMacrossData;
            public LinkedListNode<EngineNS.Macross.MacrossHandle> MacrossData
            {
                get
                {
                    return mMacrossData;
                }
                set
                {
                    mMacrossData = value;
                    if (value != null)
                    {
                        mBytePtr = mMacrossData.Value.ByteDatas;
                        mStringPtr = mMacrossData.Value.StringDatas;
                    }
                    else
                    {
                        mBytePtr = null;
                        mStringPtr = null;
                    }
                }
            }
            [EngineNS.Macross.MacrossFieldAttribute(Offset=0, Size=4, Type=typeof(Int32), Name = "IntValue")]
            public Int32 IntValue
            {
                get
                {
                    unsafe
                    {
                        fixed (byte* pbyte = &mBytePtr[0])
                        {
                            return *(Int32*)(pbyte);
                        }
                    }
                }
                set
                {
                    unsafe
                    {
                        fixed(byte* pbyte = &mBytePtr[0])
                        {
                            *(Int32*)pbyte = value;
                        }
                    }
                }
            }
            [EngineNS.Macross.MacrossFieldAttribute(Offset = 0, Type = typeof(string), Name = "StrValue")]
            public string StrValue
            {
                get { return mStringPtr[0]; }
                set { mStringPtr[0] = value; }
            }

            // offset 4, size XX
            [EngineNS.Macross.MacrossFieldAttribute(Offset = 4, Size = 10, Type = typeof(NormalStruct), Name = "NStruct")]
            public NormalStruct NStruct
            {
                get
                {
                    unsafe
                    {
                        fixed (byte* pbyte = &mBytePtr[4])
                        {
                            return *(NormalStruct*)(pbyte);
                        }
                    }
                }
                set
                {
                    unsafe
                    {
                        fixed (byte* pbyte = &mBytePtr[4])
                        {
                            *(NormalStruct*)pbyte = value;
                        }
                    }
                }
            }
            [EngineNS.Macross.MacrossFieldAttribute(Type = typeof(NormalClass), Name = "NClass")]
            public NormalClass NClass
            {
                get;
                set;
            }
            [EngineNS.Macross.MacrossFieldAttribute(Type = typeof(MacrossClass), Name = "MCClass")]
            public MacrossClass MCClass
            {
                get;
                set;
            }
            ~Macross_AActor()
            {
                EngineNS.CEngine.Instance.MacrossDataManager.FreeMacrossData(ClassId, MacrossData);
            }
            private byte[] mBytePtr;
            private string[] mStringPtr;
            public void InitMacrossData()
            {
                if (MacrossData != null)
                    return;
                unsafe
                {
                    var dataValue = new EngineNS.Macross.MacrossHandle();
                    //dataValue.Host = new WeakReference<EngineNS.Macross.IMacrossType>(this);
                    int size = sizeof(int) + sizeof(NormalStruct);
                    dataValue.ByteDatas = new byte[size];
                    dataValue.StringDatas = new string[1];

                    MacrossData = EngineNS.CEngine.Instance.MacrossDataManager.AllocMacrossData(ClassId, this.GetType(), dataValue);
                }
            }
        }
        //------------------------------------------------------*/
        #endregion


        public List<string> mExt = new List<string>() { "*" + EngineNS.CEngineDesc.MacrossExtension, "*.animmacross", "*.componentmacross", "*" + EngineNS.CEngineDesc.MacrossEnumExtension };
        public async Task<string> GenerateMacrossCollectorCode(EngineNS.ECSType csType)
        {
            try
            {
                var codeNameSpace = "Macross.MacrossCollector";
                var nameSpace = new CodeNamespace(codeNameSpace);

                var option = new System.CodeDom.Compiler.CodeGeneratorOptions();
                option.BlankLinesBetweenMembers = false;
                option.BracingStyle = "C";
                option.IndentString = "    ";
                option.ElseOnClosing = false;
                option.VerbatimOrder = true;

                var macrossClass = new CodeTypeDeclaration("MacrossFactory");
                macrossClass.IsClass = true;
                macrossClass.BaseTypes.Add(typeof(EngineNS.Macross.MacrossFactory));
                nameSpace.Types.Add(macrossClass);

                // 参考代码
                // public override object CreateMacrossObject(RName name)
                // {
                //      if(name.Name == "AAA")
                //          return new AAA();
                // }
                var createMacrossObjectMethod = new CodeMemberMethod();
                createMacrossObjectMethod.Name = "CreateMacrossObject";
                createMacrossObjectMethod.Attributes = MemberAttributes.Override | MemberAttributes.Public;
                createMacrossObjectMethod.ReturnType = new CodeTypeReference(typeof(object));
                createMacrossObjectMethod.Parameters.Add(new CodeParameterDeclarationExpression(typeof(EngineNS.RName), "name"));
                createMacrossObjectMethod.Parameters.Add(new CodeParameterDeclarationExpression(typeof(EngineNS.Rtti.VAssembly), "assembly"));
                macrossClass.Members.Add(createMacrossObjectMethod);

                var getVersionMethod = new CodeMemberMethod();
                getVersionMethod.Name = "GetVersion";
                getVersionMethod.Attributes = MemberAttributes.Override | MemberAttributes.Public;
                getVersionMethod.ReturnType = new CodeTypeReference(typeof(int));
                getVersionMethod.Parameters.Add(new CodeParameterDeclarationExpression(typeof(EngineNS.RName), "name"));
                macrossClass.Members.Add(getVersionMethod);

                var getRNameMethod = new CodeMemberMethod();
                getRNameMethod.Name = "GetMacrossRName";
                getRNameMethod.Attributes = MemberAttributes.Override | MemberAttributes.Public;
                getRNameMethod.ReturnType = new CodeTypeReference(typeof(EngineNS.RName));
                getRNameMethod.Parameters.Add(new CodeParameterDeclarationExpression(typeof(Type), "macrossType"));
                macrossClass.Members.Add(getRNameMethod);

                var getTypeMethod = new CodeMemberMethod();
                getTypeMethod.Name = "GetMacrossType";
                getTypeMethod.Attributes = MemberAttributes.Override | MemberAttributes.Public;
                getTypeMethod.ReturnType = new CodeTypeReference(typeof(Type));
                getTypeMethod.Parameters.Add(new CodeParameterDeclarationExpression(typeof(EngineNS.RName), "macrossName"));
                macrossClass.Members.Add(getTypeMethod);

                //注册一个扩展名的列表，收集各种macross File
                List<string> files = new List<string>();
                foreach(var ext in mExt)
                {
                    var partFiles = EngineNS.CEngine.Instance.FileManager.GetFiles(EngineNS.CEngine.Instance.FileManager.ProjectContent, ext + EditorCommon.Program.ResourceInfoExt, System.IO.SearchOption.AllDirectories);
                    files.AddRange(partFiles);
                }
                
                foreach(var file in files)
                {
                    var macrossFile = EngineNS.CEngine.Instance.FileManager.RemoveExtension(file);
                    if (!EngineNS.CEngine.Instance.FileManager.DirectoryExists(macrossFile))
                        continue;

                    var macrossName = EngineNS.CEngine.Instance.FileManager.GetPureFileFromFullName(macrossFile, false);

                    var rInfo = new Macross.ResourceInfos.MacrossResourceInfo();
                    await rInfo.AsyncLoad(file);
                    if (rInfo.NotGenerateCode)
                        continue;

                    var relFile = EngineNS.CEngine.Instance.FileManager._GetRelativePathFromAbsPath(macrossFile, EngineNS.CEngine.Instance.FileManager.ProjectContent).ToLower();
                    var macrossNamespace = EngineNS.CEngine.Instance.FileManager.GetPathFromFullName(relFile, false).TrimEnd('/');
                    macrossNamespace = macrossNamespace.Replace("/", ".");

                    /*/ test only // 刷新命名空间 ///////////////////////////////
                    var tempFile = rInfo.ResourceName.Address + "/" + rInfo.ResourceName.PureName() + "_" + csType.ToString() + ".cs";
                    var encode = EngineNS.CEngine.Instance.FileManager.GetEncoding(tempFile);
                    var text = System.IO.File.ReadAllText(tempFile, encode);
                    text = text.Replace("namespace Macross.Generated", "namespace " + macrossNamespace);
                    System.IO.File.WriteAllText(tempFile, text, encode);
                    ///////////////////////////////////////////////////////////*/

                    var methodCond = new CodeConditionStatement(new CodeBinaryOperatorExpression(
                                                                                new CodeFieldReferenceExpression(new CodeVariableReferenceExpression("name"), "Name"),
                                                                                CodeBinaryOperatorType.ValueEquality,
                                                                                new CodePrimitiveExpression(relFile)));
                    //////methodCond.TrueStatements.Add(new CodeVariableDeclarationStatement(typeof(EngineNS.Rtti.VAssembly).FullName, "assembly", 
                    //////                                                new CodeSnippetExpression($"EngineNS.Rtti.RttiHelper.GetAssemblyFromDllFileName({csType.GetType().FullName}.{csType.ToString()}, EngineNS.CEngine.Instance.FileManager.Bin + dllName, \"\", true)")));
                    ////methodCond.TrueStatements.Add(new CodeVariableDeclarationStatement(typeof(Type).FullName, "type", new CodeSnippetExpression($"assembly.GetType(\"{macrossNamespace}.{macrossName}\")")));
                    //////methodCond.TrueStatements.Add(new CodeMethodReturnStatement(new CodeSnippetExpression($"EngineNS.CEngine.Instance.MacrossDataManager.NewObject(type)")));
                    ////methodCond.TrueStatements.Add(new CodeConditionStatement(new CodeBinaryOperatorExpression(new CodeVariableReferenceExpression("type"), CodeBinaryOperatorType.ValueEquality, new CodePrimitiveExpression(null)),
                    ////                                                         new CodeMethodReturnStatement(new CodePrimitiveExpression(null))));
                    ////methodCond.TrueStatements.Add(new CodeVariableDeclarationStatement(typeof(EngineNS.Macross.IMacrossType).FullName, "obj", new CodeGenerateSystem.CodeDom.CodeCastExpression(typeof(EngineNS.Macross.IMacrossType), new CodeSnippetExpression("System.Activator.CreateInstance(type)"))));
                    ////methodCond.TrueStatements.Add(new CodeConditionStatement(new CodeBinaryOperatorExpression(new CodeVariableReferenceExpression("obj"), CodeBinaryOperatorType.ValueEquality, new CodePrimitiveExpression(null)),
                    ////                                                         new CodeMethodReturnStatement(new CodePrimitiveExpression(null))));
                    ////methodCond.TrueStatements.Add(new CodeMethodInvokeExpression(new CodeVariableReferenceExpression("obj"), "InitMacrossData", new CodeExpression[0]));
                    ////methodCond.TrueStatements.Add(new CodeConditionStatement(new CodeBinaryOperatorExpression(new CodeFieldReferenceExpression(new CodeVariableReferenceExpression("obj"), "MacrossData"),
                    ////                                                                                          CodeBinaryOperatorType.IdentityInequality, new CodePrimitiveExpression(null)),
                    ////                                                         new CodeAssignStatement(new CodeSnippetExpression("obj.MacrossData.Value.Host"),
                    ////                                                                                 new CodeObjectCreateExpression(typeof(System.WeakReference<EngineNS.Macross.IMacrossType>), new CodeVariableReferenceExpression("obj")))));
                    ////methodCond.TrueStatements.Add(new CodeMethodReturnStatement(new CodeVariableReferenceExpression("obj")));
                    methodCond.TrueStatements.Add(new CodeMethodReturnStatement(new CodeObjectCreateExpression($"{macrossNamespace}.{macrossName}")));

                    createMacrossObjectMethod.Statements.Add(methodCond);

                    getVersionMethod.Statements.Add(new CodeConditionStatement(
                                                                    new CodeBinaryOperatorExpression(
                                                                                new CodeFieldReferenceExpression(new CodeVariableReferenceExpression("name"), "Name"),
                                                                                CodeBinaryOperatorType.ValueEquality,
                                                                                new CodePrimitiveExpression(relFile)),
                                                                    new CodeMethodReturnStatement(new CodePrimitiveExpression(rInfo.Version))
                                                    ));

                    //getRNameMethod.Statements.Add(new CodeConditionStatement(
                    //                                                new CodeBinaryOperatorExpression(
                    //                                                            new CodeFieldReferenceExpression(new CodeVariableReferenceExpression("macrossType"), "FullName"),
                    //                                                            CodeBinaryOperatorType.ValueEquality,
                    //                                                            new CodePrimitiveExpression($"{macrossNamespace}.{macrossName}")),
                    //                                                new CodeMethodReturnStatement(new CodeSnippetExpression($"EngineNS.RName.GetRName(\"{relFile}\")"))));
                    getRNameMethod.Statements.Add(new CodeConditionStatement(
                                                                    new CodeBinaryOperatorExpression(
                                                                                new CodeVariableReferenceExpression("macrossType"),
                                                                                CodeBinaryOperatorType.ValueEquality,
                                                                                new CodeTypeOfExpression($"{macrossNamespace}.{macrossName}")),
                                                                    new CodeMethodReturnStatement(new CodeSnippetExpression($"EngineNS.RName.GetRName(\"{relFile}\")"))));

                    getTypeMethod.Statements.Add(new CodeConditionStatement(
                                                                    new CodeBinaryOperatorExpression(
                                                                                new CodeFieldReferenceExpression(new CodeVariableReferenceExpression("macrossName"), "Name"),
                                                                                CodeBinaryOperatorType.ValueEquality,
                                                                                new CodePrimitiveExpression(relFile)),
                                                                    new CodeMethodReturnStatement(new CodeTypeOfExpression($"{macrossNamespace}.{macrossName}"))));
                }

                createMacrossObjectMethod.Statements.Add(new CodeMethodReturnStatement(new CodeGenerateSystem.CodeDom.CodeMethodInvokeExpression(new CodeBaseReferenceExpression(), "CreateMacrossObject", new CodeVariableReferenceExpression("name"), new CodeVariableReferenceExpression("assembly"))));
                getVersionMethod.Statements.Add(new CodeMethodReturnStatement(new CodeGenerateSystem.CodeDom.CodeMethodInvokeExpression(new CodeBaseReferenceExpression(), "GetVersion", new CodeVariableReferenceExpression("name"))));
                getRNameMethod.Statements.Add(new CodeMethodReturnStatement(new CodeGenerateSystem.CodeDom.CodeMethodInvokeExpression(new CodeBaseReferenceExpression(), "GetMacrossRName", new CodeVariableReferenceExpression("macrossType"))));
                getTypeMethod.Statements.Add(new CodeMethodReturnStatement(new CodeGenerateSystem.CodeDom.CodeMethodInvokeExpression(new CodeBaseReferenceExpression(), "GetMacrossType", new CodeVariableReferenceExpression("macrossName"))));

                var retTw = new System.IO.StringWriter();
                retTw.WriteLine("// Engine!\n");

                var codeProvider = new CodeGenerateSystem.CSharpCodeProvider();
                codeProvider.GenerateCodeFromNamespace(nameSpace, retTw, option);
                return retTw.ToString();
            }
            catch (System.Exception e)
            {
                EngineNS.Profiler.Log.WriteLine(EngineNS.Profiler.ELogTag.Error, "Macross", e.ToString());
                System.IO.TextWriter retTw = new System.IO.StringWriter();
                retTw.WriteLine("// Engine!\n");
                retTw.WriteLine($"// 名称:MacrossCollector");
                retTw.WriteLine($"#error MacrossCollector生成代码异常! \r\n{e.ToString()}");
                return retTw.ToString();
            }
        }
        public Action<string, Type, CodeMemberProperty, CodeTypeDeclaration> GenerateVariables_After_Action;
        public void GenerateVariables(IMacrossOperationContainer linkCtrl,CodeTypeDeclaration macrossClass)
        {
            // 遍历所有对象的类型，用于生成计算size的代码
            //var valueTypeTotalSize = 0;
            //int stringTotalCount = 0;
            var variables = linkCtrl.MacrossOpPanel.GetVariables();
            // test only ----------------- //
            //variables = new List<MacrossVariable>();
            //variables.Add(new MacrossVariable() { VariableName = "IntValue", VariableType = typeof(int) });
            //variables.Add(new MacrossVariable() { VariableName = "StrValue", VariableType = typeof(string) });
            //variables.Add(new MacrossVariable() { VariableName = "FloatValue", VariableType = typeof(float) });
            //variables.Add(new MacrossVariable() { VariableName = "TempClass", VariableType = typeof(EngineNS.BezierCalculate) });
            /////////////////////////////////

            foreach (var variable in variables)
            {
                var variablePro = variable.PropertyShowItem as VariableCategoryItemPropertys;
                if (variablePro == null)
                    continue;

                if(variablePro.VariableType.IsMacrossGetter)
                {
                    var varProperty = new CodeMemberProperty();
                    varProperty.Name = variablePro.VariableName;
                    varProperty.Type = new CodeTypeReference(variablePro.VariableType.GetActualType());
                    varProperty.Attributes = MemberAttributes.Public | MemberAttributes.Final;

                    var tempFieldName = "proTemp____" + variablePro.VariableName;
                    var tempField = new CodeMemberField(variablePro.VariableType.GetActualType(), tempFieldName);

                    macrossClass.Members.Add(tempField);

                    var getterFieldName = "proTempRNmame____" + variablePro.VariableName;
                    var getterField = new CodeMemberField(typeof(EngineNS.RName), getterFieldName);

                    macrossClass.Members.Add(getterField);

                    var getterProperty = new CodeMemberProperty();
                    getterProperty.Name = "RName_" + variablePro.VariableName;
                    getterProperty.Type = new CodeTypeReference(typeof(EngineNS.RName));
                    getterProperty.Attributes = MemberAttributes.Public | MemberAttributes.Final;

                    getterProperty.HasGet = true;
                    getterProperty.HasSet = true;
                    getterProperty.GetStatements.Add(new CodeMethodReturnStatement(new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), getterFieldName)));
                    getterProperty.SetStatements.Add(new CodeAssignStatement(
                                                                new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), getterFieldName),
                                                                new CodeVariableReferenceExpression("value")));
                    getterProperty.SetStatements.Add(new CodeAssignStatement(
                                                                new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), tempFieldName),
                                                                 new CodeSnippetExpression(" EngineNS.CEngine.Instance.MacrossDataManager.NewObjectGetter<" + variablePro.VariableType.MacrossClassType.Namespace + "." + variablePro.VariableType.MacrossClassType.Name + ">(value);")));

                    //给数据变量添加自定义标签
                    foreach (var attribute in variablePro.AttributeTypeProxys)
                    {
                        List<CodeAttributeDeclaration> infos = new List<CodeAttributeDeclaration>();
                        GenerateAttributes(attribute, infos);
                        foreach (var i in infos)
                        {
                            getterProperty.CustomAttributes.Add(i);
                        }
                    }

                    macrossClass.Members.Add(getterProperty);

                    //Getter get的object
                    var getterObj = new CodeMemberProperty();
                    getterObj.Name = "Object_" + variablePro.VariableName;
                    //variablePro.VariableType
                    getterObj.Type = new CodeTypeReference(variablePro.VariableType.MacrossClassType);
                    getterObj.Attributes = MemberAttributes.Public | MemberAttributes.Final;
                    getterObj.CustomAttributes.Add(new CodeAttributeDeclaration(
                                                                new CodeTypeReference(typeof(EngineNS.Macross.MacrossFieldAttribute).FullName, 0),
                                                                new CodeAttributeArgument("Type", new CodeTypeOfExpression(variablePro.VariableType.MacrossClassType)),
                                                                new CodeAttributeArgument("Name", new CodePrimitiveExpression(getterObj.Name))));
                    getterObj.CustomAttributes.Add(new CodeAttributeDeclaration(
                                        new CodeTypeReference(typeof(EngineNS.Editor.MacrossMemberAttribute).FullName, 0),
                                        new CodeAttributeArgument(new CodeSnippetExpression("EngineNS.Editor.MacrossMemberAttribute.enMacrossType.Callable"))));

                    getterObj.HasGet = true;
                    getterObj.GetStatements.Add(new CodeConditionStatement(
                        new CodeVariableReferenceExpression(tempFieldName + "==null"),
                        new CodeStatement[] { new CodeMethodReturnStatement(new CodeSnippetExpression("null")) }
                        ));
                    getterObj.GetStatements.Add(new CodeMethodReturnStatement(new CodeGenerateSystem.CodeDom.CodeMethodInvokeExpression(new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), tempFieldName), "Get")));
                    macrossClass.Members.Add(getterObj);

                    var SetRNameMethodStatment = new CodeMemberMethod();
                    SetRNameMethodStatment.Name = "SetMacrossGetterRName_" + variablePro.VariableName;
                    SetRNameMethodStatment.Attributes = MemberAttributes.Public;

                    macrossClass.Members.Add(SetRNameMethodStatment);
                    var param = new CodeParameterDeclarationExpression(typeof(EngineNS.RName), "gettervalue");

                    param.CustomAttributes.Add(new CodeAttributeDeclaration(
                                                        new CodeTypeReference(typeof(EngineNS.Editor.Editor_RNameMacrossType).FullName, 0),
                                                        new CodeAttributeArgument(new CodeTypeOfExpression(variablePro.VariableType.MacrossClassType))));
                    SetRNameMethodStatment.Parameters.Add(param);
                    SetRNameMethodStatment.Statements.Add(new CodeSnippetStatement(getterProperty.Name + "=gettervalue;"));

                    GenerateVariables_After_Action?.Invoke(variablePro.VariableName, variablePro.VariableType.GetActualType(), varProperty, macrossClass);
                }
                else
                {


                    var varProperty = new CodeMemberProperty();
                    varProperty.Name = variablePro.VariableName;
                    varProperty.Type = new CodeTypeReference(variablePro.VariableType.GetActualType());
                    varProperty.Attributes = MemberAttributes.Public | MemberAttributes.Final;

                    var varField = new CodeMemberField();
                    varField.Name = "m" + varProperty.Name;
                    varField.Type = varProperty.Type;
                    varField.Attributes = MemberAttributes.Private;
                    macrossClass.Members.Add(varField);

                    varProperty.CustomAttributes.Add(new CodeAttributeDeclaration(
                                        new CodeTypeReference(typeof(EngineNS.Editor.MacrossMemberAttribute).FullName, 0),
                                        new CodeAttributeArgument(new CodeSnippetExpression("EngineNS.Editor.MacrossMemberAttribute.enMacrossType.Callable"))));

                    varProperty.HasGet = true;
                    varProperty.HasSet = true;
                    varProperty.GetStatements.Add(new CodeMethodReturnStatement(new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), varField.Name)));
                    varProperty.SetStatements.Add(new CodeAssignStatement(new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), varField.Name), new CodeVariableReferenceExpression("value")));

                    //给数据变量添加自定义标签
                    foreach (var attribute in variablePro.AttributeTypeProxys)
                    {
                        List<CodeAttributeDeclaration> infos = new List<CodeAttributeDeclaration>();
                        GenerateAttributes(attribute, infos);
                        foreach (var i in infos)
                        {
                            varProperty.CustomAttributes.Add(i);
                        }
                    }
                    macrossClass.Members.Add(varProperty);

                    GenerateVariables_After_Action?.Invoke(variablePro.VariableName, variablePro.VariableType.GetActualType(), varProperty, macrossClass);
                }

                //if (variablePro.VariableType.IsValueType())
                //{
                //    var acturlType = variablePro.VariableType.GetActualType();
                //    int size = 0;
                //    if (acturlType.IsEnum)
                //    {
                //        unsafe
                //        {
                //            size = sizeof(int);
                //        }
                //    }
                //    else
                //    {
                //        size = System.Runtime.InteropServices.Marshal.SizeOf(variablePro.VariableType.GetActualType());
                //    }

                //    //varProperty.CustomAttributes.Add(new CodeAttributeDeclaration(
                //    //                                            new CodeTypeReference(typeof(EngineNS.Macross.MacrossFieldAttribute).FullName, 0),
                //    //                                            new CodeAttributeArgument("Offset", new CodePrimitiveExpression(valueTypeTotalSize)),
                //    //                                            new CodeAttributeArgument("Size", new CodePrimitiveExpression(size)),
                //    //                                            new CodeAttributeArgument("Type", new CodeTypeOfExpression(variablePro.VariableType.GetActualType())),
                //    //                                            new CodeAttributeArgument("Name", new CodePrimitiveExpression(variablePro.VariableName))));
                //    varProperty.CustomAttributes.Add(new CodeAttributeDeclaration(
                //                                                new CodeTypeReference(typeof(EngineNS.Editor.MacrossMemberAttribute).FullName, 0),
                //                                                new CodeAttributeArgument(new CodeSnippetExpression("EngineNS.Editor.MacrossMemberAttribute.enMacrossType.Callable"))));

                //    varProperty.HasGet = true;
                //    varProperty.HasSet = true;
                //    //varProperty.GetStatements.Add(new CodeSnippetStatement(
                //    //                                                        "                unsafe{\r\n" +
                //    //                                                    $"                    fixed(byte* pByte=&mBytePtr[{valueTypeTotalSize}])\r\n" +
                //    //                                                        "                    {\r\n" +
                //    //                                                    $"                        return *({variablePro.VariableType.GetActualType().FullName}*)(pByte);\r\n" +
                //    //                                                        "                    }\r\n" +
                //    //                                                        "                }"));
                //    //varProperty.SetStatements.Add(new CodeSnippetStatement(
                //    //                                                        "                unsafe{\r\n" +
                //    //                                                    $"                    fixed(byte* pByte=&mBytePtr[{valueTypeTotalSize}])\r\n" +
                //    //                                                        "                    {\r\n" +
                //    //                                                    $"                        *({variablePro.VariableType.GetActualType().FullName}*)(pByte) = value;\r\n" +
                //    //                                                        "                    }\r\n" +
                //    //                                                        "                }"));

                //    valueTypeTotalSize += size;
                //}
                //else if (variablePro.VariableType.GetActualType() == typeof(string))
                //{
                //    varProperty.CustomAttributes.Add(new CodeAttributeDeclaration(
                //                                                new CodeTypeReference(typeof(EngineNS.Macross.MacrossFieldAttribute).FullName, 0),
                //                                                new CodeAttributeArgument("Offset", new CodePrimitiveExpression(stringTotalCount)),
                //                                                new CodeAttributeArgument("Type", new CodeTypeOfExpression(variablePro.VariableType.GetActualType())),
                //                                                new CodeAttributeArgument("Name", new CodePrimitiveExpression(variablePro.VariableName))));
                //    varProperty.CustomAttributes.Add(new CodeAttributeDeclaration(
                //                                                new CodeTypeReference(typeof(EngineNS.Editor.MacrossMemberAttribute).FullName, 0),
                //                                                new CodeAttributeArgument(new CodeSnippetExpression("EngineNS.Editor.MacrossMemberAttribute.enMacrossType.Callable"))));
                //    varProperty.HasGet = true;
                //    varProperty.HasSet = true;
                //    varProperty.GetStatements.Add(new CodeMethodReturnStatement(new CodeArrayIndexerExpression(new CodeVariableReferenceExpression("mStringPtr"), new CodePrimitiveExpression(stringTotalCount))));
                //    varProperty.SetStatements.Add(new CodeAssignStatement(
                //                                                new CodeArrayIndexerExpression(new CodeVariableReferenceExpression("mStringPtr"), new CodePrimitiveExpression(stringTotalCount)),
                //                                                new CodeVariableReferenceExpression("value")));

                //    stringTotalCount++;
                //}
                //else
                //{
                //    var tempFieldName = "proTemp____" + variablePro.VariableName;
                //    var tempField = new CodeMemberField(variablePro.VariableType.GetActualType(), tempFieldName);

                //    macrossClass.Members.Add(tempField);

                //    //tempField.Attributes
                //    if (variablePro.VariableType.IsMacrossGetter)
                //    {
                //        var getterFieldName = "proTempRNmame____" + variablePro.VariableName;
                //        var getterField = new CodeMemberField(typeof(EngineNS.RName), getterFieldName);

                //        macrossClass.Members.Add(getterField); 

                //        var getterProperty = new CodeMemberProperty();
                //        getterProperty.Name = "RName_" + variablePro.VariableName;
                //        getterProperty.Type = new CodeTypeReference(typeof(EngineNS.RName));
                //        getterProperty.Attributes = MemberAttributes.Public | MemberAttributes.Final;
                //        getterProperty.CustomAttributes.Add(new CodeAttributeDeclaration(
                //                                                    new CodeTypeReference(typeof(EngineNS.Macross.MacrossFieldAttribute).FullName, 0),
                //                                                    new CodeAttributeArgument("Type", new CodeTypeOfExpression(variablePro.VariableType.GetActualType())),
                //                                                    new CodeAttributeArgument("Name", new CodePrimitiveExpression(variablePro.VariableName))));
                //        //getterProperty.CustomAttributes.Add(new CodeAttributeDeclaration(
                //        //                                            new CodeTypeReference(typeof(EngineNS.Editor.Editor_RNameMacrossType).FullName, 0),
                //        //                                            new CodeAttributeArgument(new CodeTypeOfExpression(variablePro.VariableType.MacrossClassType))));

                //        getterProperty.HasGet = true;
                //        getterProperty.HasSet = true;
                //        getterProperty.GetStatements.Add(new CodeMethodReturnStatement(new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), getterFieldName)));
                //        getterProperty.SetStatements.Add(new CodeAssignStatement(
                //                                                    new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), getterFieldName),
                //                                                    new CodeVariableReferenceExpression("value")));
                //        getterProperty.SetStatements.Add(new CodeAssignStatement(
                //                                                    new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), tempFieldName),
                //                                                     new CodeSnippetExpression(" EngineNS.CEngine.Instance.MacrossDataManager.NewObjectGetter<"+ variablePro.VariableType.MacrossClassType.Namespace + "." + variablePro.VariableType.MacrossClassType.Name + ">(value);")));

                //        //给数据变量添加自定义标签
                //        foreach (var attribute in variablePro.AttributeTypeProxys)
                //        {
                //            List<CodeAttributeDeclaration> infos = new List<CodeAttributeDeclaration>();
                //            GenerateAttributes(attribute, infos);
                //            foreach (var i in infos)
                //            {
                //                getterProperty.CustomAttributes.Add(i);
                //            }
                //        }

                //        macrossClass.Members.Add(getterProperty);

                //        //Getter get的object
                //        var getterObj = new CodeMemberProperty();
                //        getterObj.Name = "Object_" + variablePro.VariableName;
                //        //variablePro.VariableType
                //        getterObj.Type = new CodeTypeReference(variablePro.VariableType.MacrossClassType);
                //        getterObj.Attributes = MemberAttributes.Public | MemberAttributes.Final;
                //        getterObj.CustomAttributes.Add(new CodeAttributeDeclaration(
                //                                                    new CodeTypeReference(typeof(EngineNS.Macross.MacrossFieldAttribute).FullName, 0),
                //                                                    new CodeAttributeArgument("Type", new CodeTypeOfExpression(variablePro.VariableType.MacrossClassType)),
                //                                                    new CodeAttributeArgument("Name", new CodePrimitiveExpression(getterObj.Name))));
                //        getterObj.CustomAttributes.Add(new CodeAttributeDeclaration(
                //                            new CodeTypeReference(typeof(EngineNS.Editor.MacrossMemberAttribute).FullName, 0),
                //                            new CodeAttributeArgument(new CodeSnippetExpression("EngineNS.Editor.MacrossMemberAttribute.enMacrossType.Callable"))));

                //        getterObj.HasGet = true;
                //        getterObj.GetStatements.Add(new CodeConditionStatement(
                //            new CodeVariableReferenceExpression(tempFieldName + "==null"),
                //            new CodeStatement[] { new CodeMethodReturnStatement(new CodeSnippetExpression("null")) }
                //            ));
                //        getterObj.GetStatements.Add(new CodeMethodReturnStatement(new CodeMethodInvokeExpression(new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), tempFieldName), "Get")));
                //        macrossClass.Members.Add(getterObj);

                //        var SetRNameMethodStatment = new CodeMemberMethod();
                //        SetRNameMethodStatment.Name = "SetMacrossGetterRName_"+ variablePro.VariableName;
                //        SetRNameMethodStatment.Attributes = MemberAttributes.Public;

                //        macrossClass.Members.Add(SetRNameMethodStatment);
                //        var param = new CodeParameterDeclarationExpression(typeof(EngineNS.RName), "gettervalue");

                //        param.CustomAttributes.Add(new CodeAttributeDeclaration(
                //                                            new CodeTypeReference(typeof(EngineNS.Editor.Editor_RNameMacrossType).FullName, 0),
                //                                            new CodeAttributeArgument(new CodeTypeOfExpression(variablePro.VariableType.MacrossClassType))));
                //        SetRNameMethodStatment.Parameters.Add(param);

                //        SetRNameMethodStatment.Statements.Add(new CodeSnippetStatement(getterProperty.Name + "=gettervalue;"));
                //    }
                //    else
                //    {
                //        if (!variablePro.VariableType.NotCreateInInitialize && !variablePro.VariableType.IsMacrossGetter)
                //            tempField.InitExpression = new CodeObjectCreateExpression(variablePro.VariableType.GetActualType(), new CodeExpression[0]);


                //        varProperty.CustomAttributes.Add(new CodeAttributeDeclaration(
                //                                                    new CodeTypeReference(typeof(EngineNS.Macross.MacrossFieldAttribute).FullName, 0),
                //                                                    new CodeAttributeArgument("Type", new CodeTypeOfExpression(variablePro.VariableType.GetActualType())),
                //                                                    new CodeAttributeArgument("Name", new CodePrimitiveExpression(variablePro.VariableName))));
                //        varProperty.CustomAttributes.Add(new CodeAttributeDeclaration(
                //                            new CodeTypeReference(typeof(EngineNS.Editor.MacrossMemberAttribute).FullName, 0),
                //                            new CodeAttributeArgument(new CodeSnippetExpression("EngineNS.Editor.MacrossMemberAttribute.enMacrossType.Callable"))));

                //        varProperty.HasGet = true;
                //        varProperty.HasSet = true;
                //        varProperty.GetStatements.Add(new CodeMethodReturnStatement(new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), tempFieldName)));
                //        varProperty.SetStatements.Add(new CodeAssignStatement(
                //                                                    new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), tempFieldName),
                //                                                    new CodeVariableReferenceExpression("value")));

                //    }
                //}

                //if (!variablePro.VariableType.IsMacrossGetter)
                //{
                //    //给数据变量添加自定义标签
                //    foreach (var attribute in variablePro.AttributeTypeProxys)
                //    {
                //        List<CodeAttributeDeclaration> infos = new List<CodeAttributeDeclaration>();
                //        GenerateAttributes(attribute, infos);
                //        foreach (var i in infos)
                //        {
                //            varProperty.CustomAttributes.Add(i);
                //        }
                //    }
                //    macrossClass.Members.Add(varProperty);
                //}

                //GenerateVariables_After_Action?.Invoke(variablePro.VariableName, variablePro.VariableType.GetActualType(), varProperty, macrossClass);
            }

            //initMacrossDataMethodStatment.Statements.Add(new CodeAssignStatement(
            //                                                        new CodeFieldReferenceExpression(new CodeVariableReferenceExpression("dataValue"), "ByteDatas"),
            //                                                        new System.CodeDom.CodeArrayCreateExpression(typeof(byte), valueTypeTotalSize)));
            //initMacrossDataMethodStatment.Statements.Add(new CodeAssignStatement(
            //                                                        new CodeFieldReferenceExpression(new CodeVariableReferenceExpression("dataValue"), "StringDatas"),
            //                                                        new System.CodeDom.CodeArrayCreateExpression(typeof(string), stringTotalCount)));
            //initMacrossDataMethodStatment.Statements.Add(new CodeAssignStatement(
            //                                                        new CodePropertyReferenceExpression(new CodeThisReferenceExpression(), "MacrossData"),
            //                                                        new CodeMethodInvokeExpression(
            //                                                                new CodeSnippetExpression("EngineNS.CEngine.Instance.MacrossDataManager"), "AllocMacrossData",
            //                                                                new CodeDirectionExpression(FieldDirection.In, new CodeVariableReferenceExpression("ClassId")),
            //                                                                new CodeMethodInvokeExpression(new CodeThisReferenceExpression(), "GetType", new CodeExpression[0]),
            //                                                                new CodeVariableReferenceExpression("dataValue"))));

        }
        public void GenerateProperties(IMacrossOperationContainer linkCtrl, CodeTypeDeclaration macrossClass)
        {
            var properties = linkCtrl.MacrossOpPanel.GetProperties();
            foreach(var item in properties)
            {
                var itemPro = item.PropertyShowItem as CategoryItemProperty_Property;
                if (itemPro == null)
                    continue;

                var proProperty = new CodeMemberProperty();
                proProperty.Name = itemPro.PropertyName;
                proProperty.Type = new CodeTypeReference(itemPro.PropertyType.GetActualType());
                proProperty.Attributes = MemberAttributes.Public | MemberAttributes.Final;
                proProperty.CustomAttributes.Add(new CodeAttributeDeclaration(new CodeTypeReference(typeof(EngineNS.Editor.MacrossMemberAttribute)), new CodeAttributeArgument(new CodePrimitiveExpression(EngineNS.Editor.MacrossMemberAttribute.enMacrossType.Callable))));

                proProperty.HasGet = true;
                proProperty.HasSet = true;
                proProperty.GetStatements.Add(new CodeVariableDeclarationStatement(proProperty.Type, "tempValue"));
                proProperty.GetStatements.Add(new CodeExpressionStatement(new CodeGenerateSystem.CodeDom.CodeMethodInvokeExpression(new CodeThisReferenceExpression(), itemPro.GetMethodNodesKey.Name, new CodeDirectionExpression(FieldDirection.Out, new CodeVariableReferenceExpression("tempValue")))));
                proProperty.GetStatements.Add(new CodeMethodReturnStatement(new CodeVariableReferenceExpression("tempValue")));
                proProperty.SetStatements.Add(new CodeExpressionStatement(new CodeGenerateSystem.CodeDom.CodeMethodInvokeExpression(new CodeThisReferenceExpression(), itemPro.SetMethodNodesKey.Name, new CodeVariableReferenceExpression("value"))));

                macrossClass.Members.Add(proProperty);
            }
        }
        public List<string> GenerateMethodsCategoryNames = new List<string>() { MacrossPanel.GraphCategoryName, MacrossPanel.FunctionCategoryName };
        public virtual  async Task GenerateMethods(IMacrossOperationContainer linkCtrl,CodeTypeDeclaration macrossClass, CodeGenerateSystem.Base.GenerateCodeContext_Class codeClassContext)
        {
            // 生成各函数代码
            Category graphCategory;
            foreach(var categoryName in GenerateMethodsCategoryNames)
            {
                if (linkCtrl.MacrossOpPanel.CategoryDic.TryGetValue(categoryName, out graphCategory))
                {
                    for (int i = 0; i < graphCategory.Items.Count; i++)
                    {
                        var graph = graphCategory.Items[i];
                        var nodesContainer = await linkCtrl.GetNodesContainer(graph, true);
                        await nodesContainer.GenerateCode(macrossClass, codeClassContext);
                    }
                }
            }
            if(linkCtrl.MacrossOpPanel.CategoryDic.TryGetValue(MacrossPanel.PropertyCategoryName, out graphCategory))
            {
                for (int i = 0; i < graphCategory.Items.Count; i++)
                {
                    var graph = graphCategory.Items[i];
                    var pro = graph.PropertyShowItem as Macross.CategoryItemProperty_Property;

                    var getNodesContainer = await linkCtrl.GetNodesContainer(pro.GetMethodNodesKey, true);
                    await getNodesContainer.GenerateCode(macrossClass, codeClassContext);

                    var setNodesContainer = await linkCtrl.GetNodesContainer(pro.SetMethodNodesKey, true);
                    await setNodesContainer.GenerateCode(macrossClass, codeClassContext);
                }
            }

            await GenerateInputActions(macrossClass, linkCtrl);
        }

        public virtual async Task CollectMacrossResource(IMacrossOperationContainer linkCtrl, List<EngineNS.RName> rnames)
        {
            // 生成各函数代码
            foreach(var category in linkCtrl.MacrossOpPanel.CategoryDic.Values)
            {
                foreach(var item in category.Items)
                {
                    if (item.HasLinkedNodesContainer == false)
                        continue;
                    var nodesContainer = await linkCtrl.GetNodesContainer(item, true);
                    if(nodesContainer != null)
                        CollectMacrossResource(nodesContainer, rnames);
                }
            }
        }

        public void CollectMacrossResource(NodesControlAssist nodesContainer, List<EngineNS.RName> rnames)
        {
            // 生成各函数代码
            CollectInNodes(nodesContainer.NodesControl, rnames);           
            foreach(var subnode in nodesContainer.SubNodesContainers.Values)
            {
                CollectInNodes(subnode, rnames);
            }
        }

        public void CollectInNodes(CodeGenerateSystem.Controls.NodesContainerControl NodesControl, List<EngineNS.RName> rnames)
        {
            foreach (var node in NodesControl.CtrlNodeList)
            {
                var rc = node as CodeGenerateSystem.Base.IRNameContainer;
                if(rc != null)
                {
                    rc.CollectRefRNames(rnames);
                }
            }
        }

        public void GenerateAttributes(AttributeType attributetype, List<CodeAttributeDeclaration> infos)
        {
            List<Constructors> ConstructorParamers = attributetype.ConstructorParamers;
            for (int j = 0; j < ConstructorParamers.Count; j++)
            {
                if (ConstructorParamers[j].Enable)
                {

                    string name = attributetype.AttributeName;
                    if (ConstructorParamers[j].ParamsCount > 0)
                    {
                        List<AttributeConstructorParamer> Paramers = ConstructorParamers[j].Paramers;
                        CodeAttributeArgument[] cas = new CodeAttributeArgument[ConstructorParamers[j].ParamsCount];
                        for (int k = 0; k < Paramers.Count; k++)
                        {
                            if (Paramers[k].Value != null && Paramers[k].Value.GetType().Equals(typeof(string)))
                            {
                                 cas[Paramers[k].Index] = new CodeAttributeArgument(new CodePrimitiveExpression(Paramers[k].ToValueString()));
                            }
                            else
                            {
                                cas[Paramers[k].Index] = new CodeAttributeArgument(new CodeSnippetExpression(Paramers[k].ToValueString()));
                            }
                        }

                        //处理参数顺序问题
                        for (int k = 0; k < ConstructorParamers[j].ParamsCount; k++)
                        {
                            if (cas[k] == null)
                            {
                                cas[k] = new CodeAttributeArgument(new CodeSnippetExpression("null"));
                            }
                        }

                        infos.Add(new CodeAttributeDeclaration(name, cas));
                    }
                    else
                    {
                        infos.Add(new CodeAttributeDeclaration(name));
                    }

                    break;
                }
            }
        }
        public CodeAttributeDeclaration[] GenerateAttributes(IMacrossOperationContainer linkCtrl)
        {
            // 生成各函数代码
            Category category;
            List<CodeAttributeDeclaration> infos = new List<CodeAttributeDeclaration>();
            if (linkCtrl.MacrossOpPanel.CategoryDic.TryGetValue(MacrossPanel.AttributeCategoryName, out category))
            {
                for (int i = 0; i < category.Items.Count; i++)
                {
                    CategoryItem item = category.Items[i];
                    if (item.PropertyShowItem != null)
                    {
                        AttributeCategoryItemPropertys acip = item.PropertyShowItem as AttributeCategoryItemPropertys;
                        if (acip != null && acip.AttributeType != null)
                        {
                            GenerateAttributes(acip.AttributeType, infos);
                        }
                    }
                }
            }

            return infos.ToArray();
        }

        internal static string StrKey_BaseOnStart = "MacrossClassBaseOn Start";
        internal static string StrKey_BaseOnEnd = "MacrossClassBaseOn End";

        public async Task<string> GenerateCode(Macross.ResourceInfos.MacrossResourceInfo info, IMacrossOperationContainer linkCtrl)
        {
            return await GenerateCode(info, linkCtrl, null);
        }

        //1 : Macross.ResourceInfos.MacrossResourceInfo info
        public async Task<string> GenerateCode(Macross.ResourceInfos.MacrossResourceInfo info, IMacrossOperationContainer linkCtrl, Func<Macross.ResourceInfos.MacrossResourceInfo, IMacrossOperationContainer, CodeTypeDeclaration, CodeNamespace, Task> generateAction, CodeGenerateSystem.Base.GenerateCodeContext_Namespace nameSpaceContext, CodeNamespace nameSpace, string className, string baseclassname)
        {
            //var className = Program.GetClassName(info, linkCtrl.CSType);
            try
            {
              
                var option = new System.CodeDom.Compiler.CodeGeneratorOptions();
                option.BlankLinesBetweenMembers = false;
                option.BracingStyle = "C";
                option.IndentString = "    ";
                option.ElseOnClosing = false;
                option.VerbatimOrder = true;
                var macrossClass = new CodeGenerateSystem.CodeDom.CodeTypeDeclaration(className);
                if (info.BaseTypeIsMacross)
                {
                    var baseInfo = await Macross.ResourceInfos.MacrossResourceInfo.GetBaseMacrossResourceInfo(info);
                    macrossClass.Comments.Add(new CodeCommentStatement(StrKey_BaseOnStart));
                    macrossClass.Comments.Add(new CodeCommentStatement(baseInfo.ResourceName.Name));
                    macrossClass.Comments.Add(new CodeCommentStatement(StrKey_BaseOnEnd));
                }
                macrossClass.IsClass = true;
                macrossClass.IsPartial = true;
                macrossClass.CustomAttributes.Add(new CodeAttributeDeclaration(typeof(EngineNS.Macross.MacrossTypeClassAttribute).FullName));
                macrossClass.CustomAttributes.Add(new CodeAttributeDeclaration(typeof(EngineNS.Editor.Editor_MacrossClassAttribute).FullName,
                                                            new CodeAttributeArgument(new CodeSnippetExpression($"{typeof(EngineNS.ECSType).FullName}.{linkCtrl.CSType.ToString()}")),
                                                            new CodeAttributeArgument(new CodeSnippetExpression(
                                                                "EngineNS.Editor.Editor_MacrossClassAttribute.enMacrossType.Useable | EngineNS.Editor.Editor_MacrossClassAttribute.enMacrossType.Inheritable | EngineNS.Editor.Editor_MacrossClassAttribute.enMacrossType.Createable |  EngineNS.Editor.Editor_MacrossClassAttribute.enMacrossType.MacrossGetter"))));
                CodeAttributeDeclaration[] cds = GenerateAttributes(linkCtrl);
                if (cds.Length > 0)
                {
                    for (int i = 0; i < cds.Length; i++)
                    {
                        macrossClass.CustomAttributes.Add(cds[i]);
                    }
                }
                if (string.IsNullOrEmpty(baseclassname) == false)
                {
                    macrossClass.BaseTypes.Add(baseclassname);//(info.BaseType.FullName
                }
                macrossClass.BaseTypes.Add(typeof(EngineNS.Macross.IMacrossType).FullName);
                nameSpace.Types.Add(macrossClass);

                // scope
                var scopeField = new CodeMemberField(typeof(EngineNS.Profiler.TimeScope), "_mScope");
                scopeField.Attributes = MemberAttributes.Static | MemberAttributes.Public;
                scopeField.InitExpression = new CodeGenerateSystem.CodeDom.CodeMethodInvokeExpression(new CodeSnippetExpression(typeof(EngineNS.Profiler.TimeScopeManager).FullName), "GetTimeScope", new CodePrimitiveExpression(nameSpace.Name + "." + className));
                macrossClass.Members.Add(scopeField);

                // 默认构造函数
                var classConstructor = new CodeConstructor();
                classConstructor.Attributes = MemberAttributes.Public;
                //classConstructor.Statements.Add(new CodeMethodInvokeExpression(new CodeThisReferenceExpression(), "InitMacrossData", new CodeExpression[0]));
                //classConstructor.Statements.Add(new CodeConditionStatement(new CodeBinaryOperatorExpression(new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), "MacrossData"),
                //                                                                                            CodeBinaryOperatorType.IdentityInequality, new CodePrimitiveExpression(null)),
                //                                                               new CodeAssignStatement(new CodeSnippetExpression("MacrossData.Value.Host"),
                //                                                                                       new CodeObjectCreateExpression(typeof(System.WeakReference<EngineNS.Macross.IMacrossType>), new CodeThisReferenceExpression()))));
                // 类默认值
                linkCtrl.GenerateClassDefaultValues(classConstructor.Statements);
                macrossClass.Members.Add(classConstructor);

                // 特殊构造函数
                // XXXConstruce(bool init)
                var classParamConstructor = new CodeConstructor();
                classParamConstructor.Parameters.Add(new CodeParameterDeclarationExpression(typeof(bool), "init"));
                classParamConstructor.Attributes = MemberAttributes.Public;
                //classParamConstructor.Statements.Add(new CodeConditionStatement(new CodeBinaryOperatorExpression(new CodeVariableReferenceExpression("init"), CodeBinaryOperatorType.IdentityEquality, new CodePrimitiveExpression(true)),
                //                                                                new CodeExpressionStatement(new CodeMethodInvokeExpression(new CodeThisReferenceExpression(), "InitMacrossData", new CodeExpression[0])),
                //                                                                new CodeConditionStatement(new CodeBinaryOperatorExpression(new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), "MacrossData"),
                //                                                                                                CodeBinaryOperatorType.IdentityInequality, new CodePrimitiveExpression(null)),
                //                                                                                           new CodeAssignStatement(new CodeSnippetExpression("MacrossData.Value.Host"),
                //                                                                                           new CodeObjectCreateExpression(typeof(System.WeakReference<EngineNS.Macross.IMacrossType>), new CodeThisReferenceExpression())))));
                // 类默认值
                linkCtrl.GenerateClassDefaultValues(classParamConstructor.Statements);
                macrossClass.Members.Add(classParamConstructor);

                // 参考代码
                //public class DebugContext_XX
                //{
                //    public int XXX = 0;
                //}
                //[ThreadStatic]
                var debugContextClassName = "DebugContext_" + className;
                var debugContextClass = new CodeTypeDeclaration(debugContextClassName);
                debugContextClass.IsClass = true;
                nameSpace.Types.Add(debugContextClass);

                // 参考代码
                //[ThreadStaticAttribute]
                //static DebugContext_XX mDebuggerContext = new DebugContext();
                var debugContext = new CodeMemberProperty();
                debugContext.Name = "mDebuggerContext";
                debugContext.Type = new CodeTypeReference(debugContextClassName);
                debugContext.Attributes = MemberAttributes.Static | MemberAttributes.Private;
                debugContext.HasGet = true;

                debugContext.GetStatements.Add(new CodeConditionStatement(new CodeBinaryOperatorExpression(
                                                                            new CodeBinaryOperatorExpression(
                                                                                    new CodeVariableReferenceExpression("mDebugHolder"),
                                                                                    CodeBinaryOperatorType.ValueEquality,
                                                                                    new CodePrimitiveExpression(null)),
                                                                            CodeBinaryOperatorType.BooleanOr,
                                                                            new CodeBinaryOperatorExpression(
                                                                                    new CodePropertyReferenceExpression(new CodeVariableReferenceExpression("mDebugHolder"), "Context"),
                                                                                    CodeBinaryOperatorType.ValueEquality,
                                                                                    new CodePrimitiveExpression(null))),
                                                                        new CodeAssignStatement(
                                                                                new CodeVariableReferenceExpression("mDebugHolder"),
                                                                                new CodeObjectCreateExpression("EngineNS.Macross.MacrossDataManager.MacrossDebugContextHolder",
                                                                                        new CodeObjectCreateExpression($"DebugContext_{className}", new CodeExpression[0])))));
                debugContext.GetStatements.Add(new CodeMethodReturnStatement(
                                                    new CodeGenerateSystem.CodeDom.CodeCastExpression(
                                                                debugContextClassName, false,
                                                                new CodePropertyReferenceExpression(new CodeVariableReferenceExpression("mDebugHolder"), "Context"))));
                macrossClass.Members.Add(debugContext);

                var debugContext1 = new CodeMemberField(typeof(EngineNS.Macross.MacrossDataManager.MacrossDebugContextHolder), "mDebugHolder");
                debugContext1.Attributes = MemberAttributes.Static | MemberAttributes.Private;
                debugContext1.CustomAttributes.Add(new CodeAttributeDeclaration(typeof(ThreadStaticAttribute).FullName));
                macrossClass.Members.Add(debugContext1);

                // 参考代码
                //int Version
                //{
                //    get { return 0; }
                //}
                var versionProperty = new CodeMemberProperty();
                versionProperty.Name = "Version";
                versionProperty.Type = new CodeTypeReference(typeof(int));
                versionProperty.HasGet = true;
                versionProperty.HasSet = false;
                versionProperty.Attributes = MemberAttributes.Public;
                if (info.BaseTypeIsMacross)
                {
                    versionProperty.Attributes |= MemberAttributes.Override;
                }
                versionProperty.GetStatements.Add(new CodeMethodReturnStatement(new CodePrimitiveExpression(info.Version)));
                macrossClass.Members.Add(versionProperty);

                // 参考代码
                // static EngineNS.Hash64 mClassId = EngineNS.Hash64.Empty;
                //var classIdMember = new CodeMemberField(typeof(EngineNS.Hash64), "mClassId");
                //classIdMember.Attributes = MemberAttributes.Static | MemberAttributes.Private;
                //classIdMember.InitExpression = new CodeSnippetExpression("EngineNS.Hash64.Empty");
                //macrossClass.Members.Add(classIdMember);

                // 参考代码
                // public static EngineNS.Hash64 ClassId
                // {
                //     get
                //     {
                //         if(mClassId == EngineNS.Hash64.Empty)
                //         {
                //             mClassId = new EngineNS.Hash64();
                //             EngineNS.Hash64.CalcHash64(ref mClassId, "Macross_AActor");
                //         }
                //         return mClassId;
                //     }
                // }
                //var classIdProperty = new CodeMemberProperty();
                //classIdProperty.Name = "ClassId";
                //classIdProperty.Type = new CodeTypeReference(typeof(EngineNS.Hash64));
                //classIdProperty.HasGet = true;
                //classIdProperty.HasSet = false;
                //classIdProperty.Attributes = MemberAttributes.Public | MemberAttributes.Static | MemberAttributes.Final;
                //if (info.BaseTypeIsMacross)
                //{
                //    classIdProperty.Attributes |= MemberAttributes.New;
                //}
                //macrossClass.Members.Add(classIdProperty);

                //var classIdCondition = new CodeConditionStatement();
                //classIdCondition.Condition = new CodeBinaryOperatorExpression(
                //                                                new CodeVariableReferenceExpression("mClassId"),
                //                                                CodeBinaryOperatorType.ValueEquality,
                //                                                new CodeSnippetExpression("EngineNS.Hash64.Empty"));
                //classIdProperty.GetStatements.Add(classIdCondition);
                //var classIdAssign = new CodeAssignStatement(
                //                                                new CodeVariableReferenceExpression("mClassId"),
                //                                                new CodeObjectCreateExpression(typeof(EngineNS.Hash64), new CodeExpression[0]));
                //classIdCondition.TrueStatements.Add(classIdAssign);
                //var calcMethod = new CodeMethodInvokeExpression(new CodeSnippetExpression("EngineNS.Hash64"), "CalcHash64",
                //                                                               new CodeDirectionExpression(FieldDirection.Ref, new CodeVariableReferenceExpression("mClassId")),
                //                                                               new CodePrimitiveExpression(className));
                //classIdCondition.TrueStatements.Add(calcMethod);
                //var retStatement = new CodeMethodReturnStatement(new CodeVariableReferenceExpression("mClassId"));
                //classIdProperty.GetStatements.Add(retStatement);

                // 参考代码
                // 再Macross类属性数量、类型、名称改变时版本号改变，用于类更新后的数据刷新
                // public static readonly UInt32 FieldsVersion = 0;
                //var fieldsVersionMember = new CodeMemberField(new CodeTypeReference("readonly " + typeof(UInt32).FullName), "FieldsVersion");
                //fieldsVersionMember.Attributes = MemberAttributes.Public | MemberAttributes.Static;// | System.CodeDom.MemberAttributes.Const;
                //if (info.BaseTypeIsMacross)
                //    fieldsVersionMember.Attributes |= MemberAttributes.New;
                //fieldsVersionMember.InitExpression = new CodePrimitiveExpression(0);
                //macrossClass.Members.Add(fieldsVersionMember);

                // 参考代码
                // private LinkedListNode<EngineNS.Macross.MacrossHandle> mMacrossData;
                //var macrossDataMember = new CodeMemberField("System.Collections.Generic.LinkedListNode<EngineNS.Macross.MacrossHandle>", "mMacrossData");
                //macrossDataMember.Attributes = MemberAttributes.Private;
                //macrossClass.Members.Add(macrossDataMember);

                // 参考代码
                // public LinkedListNode<EngineNS.Macross.MacrossHandle> MacrossData
                // {
                //     get
                //     {
                //         return mMacrossData;
                //     }
                //     set
                //     {
                //         mMacrossData = value;
                //         if (value != null)
                //         {
                //             mBytePtr = mMacrossData.Value.ByteDatas;
                //             mStringPtr = mMacrossData.Value.StringDatas;
                //         }
                //         else
                //         {
                //             mBytePtr = null;
                //             mStringPtr = null;
                //         }
                //     }
                // }
                //var macrossDataProMember = new CodeMemberProperty();
                //macrossDataProMember.Name = "MacrossData";
                //macrossDataProMember.Type = new CodeTypeReference("System.Collections.Generic.LinkedListNode<EngineNS.Macross.MacrossHandle>");
                //macrossDataProMember.CustomAttributes.Add(new CodeAttributeDeclaration(new CodeTypeReference(typeof(System.ComponentModel.BrowsableAttribute)), new CodeAttributeArgument[] { new CodeAttributeArgument(new CodePrimitiveExpression(false)) }));
                //macrossDataProMember.Attributes = MemberAttributes.Public;
                //if (info.BaseTypeIsMacross)
                //    macrossDataProMember.Attributes |= MemberAttributes.Override;
                //macrossDataProMember.HasGet = true;
                //macrossDataProMember.HasSet = true;
                //macrossDataProMember.GetStatements.Add(new CodeMethodReturnStatement(new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), "mMacrossData")));

                //macrossDataProMember.SetStatements.Add(new CodeAssignStatement(
                //                                                    new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), "mMacrossData"),
                //                                                    new CodeVariableReferenceExpression("value")));
                //macrossDataProMember.SetStatements.Add(new CodeConditionStatement(
                //                                                    new CodeBinaryOperatorExpression(
                //                                                        new CodeVariableReferenceExpression("value"),
                //                                                        CodeBinaryOperatorType.IdentityInequality,
                //                                                        new CodePrimitiveExpression(null)),
                //                                                    new CodeStatement[]
                //                                                        {
                //                                                            new CodeAssignStatement(
                //                                                                new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), "mBytePtr"),
                //                                                                new CodeSnippetExpression("mMacrossData.Value.ByteDatas")),
                //                                                            new CodeAssignStatement(
                //                                                                new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), "mStringPtr"),
                //                                                                new CodeSnippetExpression("mMacrossData.Value.StringDatas"))
                //                                                        },
                //                                                    new CodeStatement[]
                //                                                        {
                //                                                            new CodeAssignStatement(
                //                                                                new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), "mBytePtr"),
                //                                                                new CodePrimitiveExpression(null)),
                //                                                            new CodeAssignStatement(
                //                                                                new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), "mStringPtr"),
                //                                                                new CodePrimitiveExpression(null))
                //                                                        }
                //                                                    ));
                //macrossClass.Members.Add(macrossDataProMember);

                //var destructing = new CodeSnippetTypeMember(
                //        $"        ~{className}()\r\n" +
                //        "        {\r\n" +
                //        "            EngineNS.CEngine.Instance.MacrossDataManager.FreeMacrossData(ClassId, MacrossData);\r\n" +
                //        "        }\r\n"
                //    );
                //macrossClass.Members.Add(destructing);

                // 参考代码
                // private byte[] mBytePtr;
                //var bytePtrMember = new CodeMemberField(new CodeTypeReference(typeof(byte).FullName, 1), "mBytePtr");
                //bytePtrMember.Attributes = MemberAttributes.Private;
                //macrossClass.Members.Add(bytePtrMember);

                // 参考代码
                // private string[] mStringPtr;
                //var stringPtrMember = new CodeMemberField(new CodeTypeReference(typeof(string).FullName, 1), "mStringPtr");
                //stringPtrMember.Attributes = MemberAttributes.Private;
                //macrossClass.Members.Add(stringPtrMember);

                // 参考代码
                //public void InitMacrossData()
                //{
                //    if (MacrossData != null)
                //        return;
                //    unsafe
                //    {
                //        var dataValue = new EngineNS.Macross.MacrossHandle();
                //        //dataValue.Host = new WeakReference<EngineNS.Macross.IMacrossType>(this);
                //        int size = sizeof(int) + sizeof(NormalStruct);
                //        dataValue.ByteDatas = new byte[size];
                //        dataValue.StringDatas = new string[1];

                //        MacrossData = EngineNS.CEngine.Instance.MacrossDataManager.AllocMacrossData(ClassId, this.GetType(), dataValue);
                //    }
                //}
                //var initMacrossDataMethodStatment = new CodeMemberMethod();
                //initMacrossDataMethodStatment.Name = "InitMacrossData";
                //initMacrossDataMethodStatment.Attributes = MemberAttributes.Public;
                //if (info.BaseTypeIsMacross)
                //    initMacrossDataMethodStatment.Attributes |= MemberAttributes.Override;
                //macrossClass.Members.Add(initMacrossDataMethodStatment);
                //initMacrossDataMethodStatment.Statements.Add(new CodeConditionStatement(
                //                                                    new CodeBinaryOperatorExpression(
                //                                                        new CodePropertyReferenceExpression(new CodeThisReferenceExpression(), "MacrossData"),
                //                                                        CodeBinaryOperatorType.IdentityInequality,
                //                                                        new CodePrimitiveExpression(null)),
                //                                                    new CodeMethodReturnStatement()));
                //initMacrossDataMethodStatment.Statements.Add(new CodeVariableDeclarationStatement(
                //                                                    typeof(EngineNS.Macross.MacrossHandle),
                //                                                    "dataValue",
                //                                                    new CodeObjectCreateExpression(typeof(EngineNS.Macross.MacrossHandle), new CodeExpression[0])));
                GenerateVariables(linkCtrl, macrossClass);
                GenerateProperties(linkCtrl, macrossClass);
                var codeClassContext = new CodeGenerateSystem.Base.GenerateCodeContext_Class(nameSpaceContext, macrossClass);
                codeClassContext.DebugContextClass = debugContextClass;
                await GenerateMethods(linkCtrl, macrossClass, codeClassContext);
                if (generateAction != null)
                    await generateAction.Invoke(info, linkCtrl, macrossClass, nameSpace);

                // 生成代码段
                var retTw = new System.IO.StringWriter();
                retTw.WriteLine("// Engine!\n");
                retTw.WriteLine($"// 名称:{className}");

                var codeProvider = new CodeGenerateSystem.CSharpCodeProvider();
                codeProvider.GenerateCodeFromNamespace(nameSpace, retTw, option);
                var codeStr = retTw.ToString();
                foreach (var ptrInvoke in codeClassContext.PtrInvokeParamNames)
                {
                    codeStr = codeStr.Replace(ptrInvoke + ".", ptrInvoke + "->");
                }
                return codeStr;
            }
            catch (System.Exception e)
            {
                EngineNS.Profiler.Log.WriteLine(EngineNS.Profiler.ELogTag.Error, "Macross", e.ToString());
                System.IO.TextWriter retTw = new System.IO.StringWriter();
                retTw.WriteLine("// Engine!\n");
                retTw.WriteLine($"// 名称:{className}");
                retTw.WriteLine($"#error Macross生成代码异常! Id={info.Id}, Name={className}\r\n{e.ToString()}");
                return retTw.ToString();
            }
        }

        public virtual async Task<string> GenerateCode(Macross.ResourceInfos.MacrossResourceInfo info, IMacrossOperationContainer linkCtrl, Func<Macross.ResourceInfos.MacrossResourceInfo, IMacrossOperationContainer, CodeTypeDeclaration, CodeNamespace, Task> generateAction)
        {
            var className = Program.GetClassName(info, linkCtrl.CSType);
            try
            {
                CodeDomNode.BreakPoint.ClearDebugValueFieldDic();
                //var codeNameSpace = "Macross.Generated";
                var codeNameSpace = Program.GetClassNamespace(info, linkCtrl.CSType);

                var nameSpace = new CodeNamespace(codeNameSpace);

                var nameSpaceContext = new CodeGenerateSystem.Base.GenerateCodeContext_Namespace(codeNameSpace, nameSpace);
                nameSpaceContext.NameSpaceID = info.Id;
                nameSpaceContext.Sign = className;
                return await GenerateCode(info, linkCtrl, generateAction, nameSpaceContext, nameSpace, className, info.BaseType.FullName);
          
            }
            catch (System.Exception e)
            {
                EngineNS.Profiler.Log.WriteLine(EngineNS.Profiler.ELogTag.Error, "Macross", e.ToString());
                System.IO.TextWriter retTw = new System.IO.StringWriter();
                retTw.WriteLine("// Engine!\n");
                retTw.WriteLine($"// 名称:{className}");
                retTw.WriteLine($"#error Macross生成代码异常! Id={info.Id}, Name={className}\r\n{e.ToString()}");
                return retTw.ToString();
            }
        }
        public List<string> CollectionMacrossProjectFiles(EngineNS.ECSType csType)
        {
            var codeFiles = EngineNS.CEngine.Instance.FileManager.GetFiles(EngineNS.CEngine.Instance.FileManager.ProjectContent, $"*_{csType.ToString()}.cs", System.IO.SearchOption.AllDirectories);
            var enumFiles = EngineNS.CEngine.Instance.FileManager.GetFiles(EngineNS.CEngine.Instance.FileManager.ProjectContent, "*.macross_enum.cs", System.IO.SearchOption.AllDirectories);

            List<string> files = new List<string>();
            files.AddRange(codeFiles);
            files.AddRange(enumFiles);
            return files;
        }
        public void GenerateMacrossProject(string[] macrossCodeFiles, EngineNS.ECSType csType)
        {
            var projFileName = EditorCommon.GameProjectConfig.Instance.MacrossGenerateProjItemsFileName;
            var projDir = EngineNS.CEngine.Instance.FileManager.GetPathFromFullName(projFileName);
            if(!EngineNS.CEngine.Instance.FileManager.FileExists(projFileName))
            {
                var srcFileName = EngineNS.CEngine.Instance.FileManager.EditorContent + "Macross/Macross.Generated.projitems";
                EngineNS.CEngine.Instance.FileManager.CopyFile(srcFileName, projFileName, true);
                var srcPJFileName = EngineNS.CEngine.Instance.FileManager.EditorContent + "Macross/Macross.Generated.shproj";
                var tagPJFileName = projDir + "Macross.Generated.shproj";
                EngineNS.CEngine.Instance.FileManager.CopyFile(srcPJFileName, tagPJFileName, true);
            }
            EngineNS.CEngine.Instance.FileManager.DeleteFilesInDirectory(projDir, "*.cs", System.IO.SearchOption.TopDirectoryOnly);
            foreach(var dir in EngineNS.CEngine.Instance.FileManager.GetDirectories(projDir))
            {
                EngineNS.CEngine.Instance.FileManager.DeleteDirectory(dir, true);
            }

            var xml = new XmlDocument();
            xml.Load(projFileName);

            var strWriter = new System.IO.StringWriter();
            var xmlTextWriter = new XmlTextWriter(strWriter);
            xml.WriteTo(xmlTextWriter);
            var oldContent = strWriter.ToString();

            var nsmgr = new XmlNamespaceManager(xml.NameTable);
            var nsUrl = "http://schemas.microsoft.com/developer/msbuild/2003";
            nsmgr.AddNamespace("xlns", nsUrl);
            var root = xml.DocumentElement;

            var itemGroupNode = root.SelectSingleNode("descendant::xlns:ItemGroup", nsmgr);
            if(itemGroupNode == null)
            {
                itemGroupNode = xml.CreateElement("ItemGroup", nsUrl);
                root.AppendChild(itemGroupNode);
            }
            var nodes = root.SelectNodes("descendant::xlns:ItemGroup/xlns:Compile", nsmgr);
            foreach (XmlNode node in nodes)
                node.ParentNode.RemoveChild(node);

            // 只处理在Content中的Macross，EngineContent及EditorContent中的不属于游戏逻辑
            foreach (var file in macrossCodeFiles)
            {
                var fileName = EngineNS.CEngine.Instance.FileManager._GetRelativePathFromAbsPath(file, EngineNS.CEngine.Instance.FileManager.ProjectContent);
                if(fileName.IndexOf("macross_enum") == -1)
                {
                    var path1 = EngineNS.CEngine.Instance.FileManager.GetPathFromFullName(file);
                    var path2 = EngineNS.CEngine.Instance.FileManager.GetPathFromFullName(path1);
                    path1 = path1.Replace(path2, "");
                    fileName = fileName.Replace(path1, "").Replace("/", "\\");
                }

                var node = xml.CreateElement("Compile", nsUrl);
                node.SetAttribute("Include", $"$(MSBuildThisFileDirectory){fileName}");
                itemGroupNode.AppendChild(node);

                var tagFile = projDir + fileName;
                var tagDir = EngineNS.CEngine.Instance.FileManager.GetPathFromFullName(tagFile);
                if (!EngineNS.CEngine.Instance.FileManager.DirectoryExists(tagDir))
                    EngineNS.CEngine.Instance.FileManager.CreateDirectory(tagDir);
                EngineNS.CEngine.Instance.FileManager.CopyFile(file, tagFile, true);
            }
            var colFile = EngineNS.CEngine.Instance.MacrossDataManager.GetCollectorCodeFileName(csType);
            var colFileName = EngineNS.CEngine.Instance.FileManager.GetPureFileFromFullName(colFile);
            var colTagFile = projDir + colFileName;
            EngineNS.CEngine.Instance.FileManager.CopyFile(colFile, colTagFile, true);
            var colNode = xml.CreateElement("Compile", nsUrl);
            colNode.SetAttribute("Include", $"$(MSBuildThisFileDirectory){colFileName}");
            itemGroupNode.AppendChild(colNode);

            strWriter = new System.IO.StringWriter();
            xmlTextWriter = new XmlTextWriter(strWriter);
            xml.WriteTo(xmlTextWriter);
            var tagContent = strWriter.ToString();
            if (tagContent != oldContent)
                xml.Save(projFileName);
        }

        public  CompilerResults CompileCode(string absDllFile, string[] codeFiles, string[] refAssemblys, string compilerOptions, bool isDebug)
        {
            var compiler = new CodeGenerateSystem.CSharpCodeProvider();
            var parameters = new CompilerParameters()
            {
                CompilerOptions = "/unsafe",
                GenerateExecutable = false,
                GenerateInMemory = false,
                IncludeDebugInformation = isDebug,
                OutputAssembly = absDllFile,
            };
            parameters.CompilerOptions += " " + compilerOptions;
            parameters.ReferencedAssemblies.AddRange(refAssemblys);
            var result = compiler.CompileAssemblyFromFile(parameters, codeFiles);

            var pureFile = EngineNS.CEngine.Instance.FileManager.RemoveExtension(absDllFile);
            if (EngineNS.CEngine.Instance.FileManager.FileExists(pureFile + ".pdb"))
            {
                EngineNS.CEngine.Instance.FileManager.CopyFile(pureFile + ".pdb", pureFile + ".vpdb", true);
                EngineNS.CEngine.Instance.FileManager.DeleteFile(pureFile + ".pdb");
            }
            return result;
        }

        public async Task<string> GenerateAndSaveMacrossCollector(EngineNS.ECSType csType)
        {
            var codeStr = await GenerateMacrossCollectorCode(csType);
            var codeFile = EngineNS.CEngine.Instance.MacrossDataManager.GetCollectorCodeFileName(csType);// $"{dir}/MacrossCollector_{csType.ToString()}.cs".Replace("/", "\\");
            var dir = EngineNS.CEngine.Instance.FileManager.GetPathFromFullName(codeFile);
            if (!EngineNS.CEngine.Instance.FileManager.DirectoryExists(dir))
                EngineNS.CEngine.Instance.FileManager.CreateDirectory(dir);
            using (var fs = new System.IO.FileStream(codeFile, System.IO.FileMode.Create, System.IO.FileAccess.ReadWrite, System.IO.FileShare.ReadWrite))
            {
                fs.Write(System.Text.Encoding.Default.GetBytes(codeStr), 0, Encoding.Default.GetByteCount(codeStr));
            }
            return codeFile;
        }
        public  async Task<System.CodeDom.Compiler.CompilerResults> CompileMacrossCollector(EngineNS.ECSType csType, string absDllName, string[] refAssemblys, string compilerOptions, bool isDebug)
        {
            var codeFile = await GenerateAndSaveMacrossCollector(csType);
            return CompileCode(absDllName, new string[] { codeFile }, refAssemblys, compilerOptions, isDebug);
        }

        public  async Task CollectInputActions(CodeGenerateSystem.Controls.NodesContainerControl nodesControl, List<CodeGenerateSystem.CodeDom.CodeMethodInvokeExpression> registerExps, List<CodeGenerateSystem.CodeDom.CodeMethodInvokeExpression> unRegisterExps)
        {
            await EngineNS.Thread.AsyncDummyClass.DummyFunc();
            foreach (var ctrl in nodesControl.CtrlNodeList)
            {
                if ((ctrl is CodeDomNode.InputActionMethodCustom))
                {
                    var csParam = ctrl.CSParam as CodeDomNode.InputActionMethodCustom.InputActionMethodCustomConstructParam;
                    if(csParam.MethodInfo.InParams.Count>1)
                    {
                        var registerMethodInvokeExp =
                            new CodeGenerateSystem.CodeDom.CodeMethodInvokeExpression(
                                new CodeVariableReferenceExpression("EngineNS.CEngine.Instance.InputServerInstance"), "RegisterInputMoveMapping",
                                new CodeExpression[] { new CodePrimitiveExpression(csParam.MethodName), new CodeMethodReferenceExpression(null, ctrl.NodeName) });
                        registerExps.Add(registerMethodInvokeExp);
                        var unRegisterMethodInvokeExp =
                            new CodeGenerateSystem.CodeDom.CodeMethodInvokeExpression(
                                new CodeVariableReferenceExpression("EngineNS.CEngine.Instance.InputServerInstance"), "UnRegisterInputMoveMapping",
                                new CodeExpression[] { new CodePrimitiveExpression(csParam.MethodName), new CodeMethodReferenceExpression(null, ctrl.NodeName) });
                        unRegisterExps.Add(unRegisterMethodInvokeExp);
                    }
                    else
                    {
                        var registerMethodInvokeExp =
                            new CodeGenerateSystem.CodeDom.CodeMethodInvokeExpression(
                                new CodeVariableReferenceExpression("EngineNS.CEngine.Instance.InputServerInstance"), "RegisterInputMapping",
                                new CodeExpression[] { new CodePrimitiveExpression(csParam.MethodName), new CodeMethodReferenceExpression(null, ctrl.NodeName) });
                        registerExps.Add(registerMethodInvokeExp);
                        var unRegisterMethodInvokeExp =
                            new CodeGenerateSystem.CodeDom.CodeMethodInvokeExpression(
                                new CodeVariableReferenceExpression("EngineNS.CEngine.Instance.InputServerInstance"), "UnRegisterInputMapping",
                                new CodeExpression[] { new CodePrimitiveExpression(csParam.MethodName), new CodeMethodReferenceExpression(null, ctrl.NodeName) });
                        unRegisterExps.Add(unRegisterMethodInvokeExp);
                    }
                }
            }
        }
        public async Task GenerateInputActions(CodeTypeDeclaration macrossClass, IMacrossOperationContainer linkCtrl)
        {

            bool needInputable = false;
            foreach(var baseType in macrossClass.BaseTypes)
            {
                var inputable = typeof(EngineNS.Input.IInputable);
                if(((System.CodeDom.CodeTypeReference)baseType).BaseType == inputable.FullName)
                {
                    needInputable = true;
                    break;
                }
                else
                {
                    var theType = EngineNS.Rtti.RttiHelper.GetTypeFromTypeFullName(((System.CodeDom.CodeTypeReference)baseType).BaseType);
                     var iface = theType.GetInterface(inputable.FullName);
                    if (iface != null)
                    {
                        needInputable = true;
                        break;
                    }
                }
            }
            if (!needInputable)
                return;
            List<CodeGenerateSystem.CodeDom.CodeMethodInvokeExpression> mInputRegisterMethodInvokeExps = new List<CodeGenerateSystem.CodeDom.CodeMethodInvokeExpression>();
            List<CodeGenerateSystem.CodeDom.CodeMethodInvokeExpression> mInputUnRegisterMethodInvokeExps = new List<CodeGenerateSystem.CodeDom.CodeMethodInvokeExpression>();
            Category graphCategory;
            if (linkCtrl.MacrossOpPanel.CategoryDic.TryGetValue(MacrossPanel.GraphCategoryName, out graphCategory))
            {
                for (int i = 0; i < graphCategory.Items.Count; i++)
                {
                    var graph = graphCategory.Items[i];
                    var nodesContainer = await linkCtrl.GetNodesContainer(graph, true);
                    foreach (var sub in nodesContainer.SubNodesContainers)
                    {
                        await CollectInputActions(sub.Value, mInputRegisterMethodInvokeExps, mInputUnRegisterMethodInvokeExps);
                    }
                    await CollectInputActions(nodesContainer.NodesControl, mInputRegisterMethodInvokeExps, mInputUnRegisterMethodInvokeExps);

                }
            }
            if (linkCtrl.MacrossOpPanel.CategoryDic.TryGetValue(MacrossPanel.FunctionCategoryName, out graphCategory))
            {
                for (int i = 0; i < graphCategory.Items.Count; i++)
                {
                    var graph = graphCategory.Items[i];
                    var nodesContainer = await linkCtrl.GetNodesContainer(graph, true);
                    foreach (var sub in nodesContainer.SubNodesContainers)
                    {
                        await CollectInputActions(sub.Value, mInputRegisterMethodInvokeExps, mInputUnRegisterMethodInvokeExps);
                    }
                    await CollectInputActions(nodesContainer.NodesControl, mInputRegisterMethodInvokeExps, mInputUnRegisterMethodInvokeExps);
                }
            }

            CodeGenerateSystem.CodeDom.CodeMemberMethod addMethod = null;
            CodeGenerateSystem.CodeDom.CodeMemberMethod removeMethod = null;
            foreach (var member in macrossClass.Members)
            {
                if (member is CodeGenerateSystem.CodeDom.CodeMemberMethod)
                {
                    var method = member as CodeGenerateSystem.CodeDom.CodeMemberMethod;
                    if (method.Name == "OnRegisterInput")
                        addMethod = method;
                    if (method.Name == "OnUnRegisterInput")
                        removeMethod = method;
                }
            }
            if (addMethod == null)
            {
                addMethod = new CodeGenerateSystem.CodeDom.CodeMemberMethod();
                addMethod.Name = "OnRegisterInput";
                addMethod.Attributes = MemberAttributes.Override | MemberAttributes.Public;
                macrossClass.Members.Add(addMethod);
            }
            if (removeMethod == null)
            {
                removeMethod = new CodeGenerateSystem.CodeDom.CodeMemberMethod();
                removeMethod.Name = "OnUnRegisterInput";
                removeMethod.Attributes = MemberAttributes.Override | MemberAttributes.Public;
                macrossClass.Members.Add(removeMethod);
            }
            foreach (var exp in mInputRegisterMethodInvokeExps)
            {
                addMethod.Statements.Add(exp);
            }
            foreach (var exp in mInputUnRegisterMethodInvokeExps)
            {
                removeMethod.Statements.Add(exp);
            }
        }
    }
}
