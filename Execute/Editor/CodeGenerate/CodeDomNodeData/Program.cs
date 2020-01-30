using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Linq;
using System.Collections;

namespace CodeDomNode
{
    public partial class Program
    {
        //public static string GetParamFromParamInfo(System.Reflection.ParameterInfo pInfo)
        //{
        //    var parameterTypeString = EngineNS.Rtti.RttiHelper.GetTypeSaveString(pInfo.ParameterType);
        //    string strFlag = ":";
        //    if (pInfo.IsOut)
        //    {
        //        strFlag = ":Out";
        //        parameterTypeString = parameterTypeString.Remove(parameterTypeString.Length - 1);
        //    }
        //    else if (pInfo.ParameterType.IsByRef)
        //    {
        //        strFlag = ":Ref";
        //        parameterTypeString = parameterTypeString.Remove(parameterTypeString.Length - 1);
        //    }
        //    else
        //    {
        //        var atts = pInfo.GetCustomAttributes(typeof(System.ParamArrayAttribute), false);
        //        if (atts.Length > 0)
        //        {
        //            strFlag = ":Params";
        //        }
        //    }

        //    return pInfo.Name + ":" + parameterTypeString + strFlag;
        //}

        public static MethodParamInfoAssist GetMethodParamInfoAssistFromParamInfo(System.Reflection.ParameterInfo pInfo)
        {
            var retValue = new MethodParamInfoAssist();
            retValue.ParamName = pInfo.Name;
            if (pInfo.IsOut)
            {
                retValue.FieldDirection = System.CodeDom.FieldDirection.Out;
                var paramFullName = pInfo.ParameterType.FullName;
                retValue.ParameterType = pInfo.ParameterType.Assembly.GetType(paramFullName.Remove(paramFullName.Length - 1));
            }
            else if (pInfo.ParameterType.IsByRef)
            {
                retValue.FieldDirection = System.CodeDom.FieldDirection.Ref;
                var paramFullName = pInfo.ParameterType.FullName;
                retValue.ParameterType = pInfo.ParameterType.Assembly.GetType(paramFullName.Remove(paramFullName.Length - 1));
            }
            else
            {
                retValue.ParameterType = pInfo.ParameterType;
                retValue.FieldDirection = System.CodeDom.FieldDirection.In;
                var atts = pInfo.GetCustomAttributes(typeof(System.ParamArrayAttribute), false);
                if (atts.Length > 0)
                    retValue.IsParamsArray = true;
            }

            var typeAtts = pInfo.GetCustomAttributes(typeof(EngineNS.Editor.Editor_MacrossMethodParamTypeAttribute), true);
            if(typeAtts.Length > 0)
            {
                retValue.ParameterDisplayType = ((EngineNS.Editor.Editor_MacrossMethodParamTypeAttribute)typeAtts[0]).ParamType;
            }

            var attrs = pInfo.GetCustomAttributes(typeof(System.ComponentModel.DisplayNameAttribute), true);
            if (attrs.Length > 0)
            {
                retValue.UIDisplayParamName = ((System.ComponentModel.DisplayNameAttribute)attrs[0]).DisplayName;
            }

            attrs = pInfo.GetCustomAttributes(typeof(EngineNS.Editor.Editor_TypeChangeWithParamAttribute), true);
            if(attrs.Length > 0)
            {
                retValue.TypeLinkIndex = ((EngineNS.Editor.Editor_TypeChangeWithParamAttribute)attrs[0]).Index;
            }

            return retValue;
        }

        //public static string GetParamFromMethodInfo(System.Reflection.MethodInfo info, string path)
        //{
        //    string strRet = path + "," + EngineNS.Rtti.RttiHelper.GetTypeSaveString(info.ReflectedType) + "," + info.Name + ",";

        //    System.Reflection.ParameterInfo[] parInfos = info.GetParameters();
        //    if (parInfos.Length > 0)
        //    {
        //        foreach (var pInfo in parInfos)
        //        {
        //            strRet += GetParamFromParamInfo(pInfo) + "/";
        //        }
        //        strRet = strRet.Remove(strRet.Length - 1);   // 去除最后一个"/"
        //    }

        //    strRet += "," + EngineNS.Rtti.RttiHelper.GetTypeSaveString(info.ReturnType);

        //    return strRet;
        //}
        public static MethodInfoAssist GetMethodInfoAssistFromMethodInfo(CodeDomNode.CustomMethodInfo info, Type parentClassType, CodeDomNode.MethodInfoAssist.enHostType hostType, string path)
        {
            var retVal = new MethodInfoAssist();
            retVal.Path = path;
            retVal.ParentClassType = parentClassType;
            retVal.MethodName = info.MethodName;
            //for (int i = 0; i < info.Attributes.Count; i++)
            //{
            //    var attr = info.Attributes[i] as EngineNS.Editor.DisplayParamNameAttribute;
            //    if (attr != null)
            //    {
            //        retVal.ShowMethodName = attr.DisplayName;
            //        break;
            //    }
            //}

            if (string.IsNullOrEmpty(retVal.ShowMethodName) && string.IsNullOrEmpty(info.Tooltip) == false)
            {
                retVal.ShowMethodName = info.Tooltip;
            }

            foreach (var pInfo in info.InParams)
            {
                var paramInfo = GetMethodParamInfoAssistFromParamInfo(pInfo, FieldDirection.In);
                retVal.Params.Add(paramInfo);
            }
            foreach(var pInfo in info.OutParams)
            {
                var paramInfo = GetMethodParamInfoAssistFromParamInfo(pInfo, FieldDirection.Out);
                retVal.Params.Add(paramInfo);
            }
            if (info.IsAsync)
                retVal.ReturnType = typeof(System.Threading.Tasks.Task);
            else
                retVal.ReturnType = typeof(void);
            retVal.IsPublic = true;
            retVal.HostType = hostType;

       //     for (int i = 0; i < info.Attributes.Count; i++)
        //    {
       //         var attr = info.Attributes[i] as System.ComponentModel.DisplayNameAttribute;
       //         if (attr != null)
        //        {
       //             //retVal.ShowMethodName = attr.DisplayName;
        //            break;
       //         }
       //     }

            return retVal;
        }
        public static MethodParamInfoAssist GetMethodParamInfoAssistFromParamInfo(CustomMethodInfo.FunctionParam pInfo, FieldDirection fieldDir)
        {
            var retValue = new MethodParamInfoAssist();
            retValue.ParamName = pInfo.ParamName;
            retValue.FieldDirection = fieldDir;
            retValue.ParameterType = pInfo.ParamType.GetActualType();
            foreach(var att in pInfo.Attributes)
            {
                if(att is EngineNS.Editor.Editor_MacrossMethodParamTypeAttribute)
                {
                    retValue.ParameterDisplayType = ((EngineNS.Editor.Editor_MacrossMethodParamTypeAttribute)att).ParamType;
                }
            }

            for (int i = 0; i < pInfo.Attributes.Count; i++)
            {
                var attr = pInfo.Attributes[i] as System.ComponentModel.DisplayNameAttribute;
                if (attr != null)
                {
                   retValue.UIDisplayParamName = attr.DisplayName;
                    break;
                }
            }

            return retValue;
        }

        public static MethodInfoAssist GetMethodInfoAssistFromMethodInfo(System.Reflection.MethodInfo info, Type parentClassType, CodeDomNode.MethodInfoAssist.enHostType hostType, string path)
        {
            var retVal = new MethodInfoAssist();
            retVal.Path = path;
            retVal.ParentClassType = parentClassType;
            retVal.MethodName = info.Name;
            var attrs = info.GetCustomAttributes(typeof(EngineNS.Editor.DisplayParamNameAttribute), true);
            if (attrs.Length > 0)
            {
                retVal.ShowMethodName = ((EngineNS.Editor.DisplayParamNameAttribute)attrs[0]).DisplayName;
            }

            var parInfos = info.GetParameters();
            if(parInfos.Length > 0)
            {
                foreach(var pInfo in parInfos)
                {
                    var paramInfo = GetMethodParamInfoAssistFromParamInfo(pInfo);
                    retVal.Params.Add(paramInfo);
                }
            }

            if(info.ReturnTypeCustomAttributes.IsDefined(typeof(EngineNS.Editor.Editor_TypeChangeWithParamAttribute), true))
            {
                var att = (EngineNS.Editor.Editor_TypeChangeWithParamAttribute)(info.ReturnTypeCustomAttributes.GetCustomAttributes(typeof(EngineNS.Editor.Editor_TypeChangeWithParamAttribute), true)[0]);
                retVal.ReturnTypeLinkIndex = att.Index;
            }
            retVal.ReturnType = info.ReturnType;
            retVal.IsFamily = info.IsFamily;
            retVal.IsFamilyAndAssembly = info.IsFamilyAndAssembly;
            retVal.IsFamilyOrAssembly = info.IsFamilyOrAssembly;
            retVal.IsPublic = info.IsPublic;
            retVal.HostType = hostType;

            var mmAtt = info.GetCustomAttribute(typeof(EngineNS.Editor.MacrossMemberAttribute), false) as EngineNS.Editor.MacrossMemberAttribute;
            if(mmAtt != null)
            {
                retVal.MC_Callable = mmAtt.HasType(EngineNS.Editor.MacrossMemberAttribute.enMacrossType.Callable);
                retVal.MC_Overrideable = mmAtt.HasType(EngineNS.Editor.MacrossMemberAttribute.enMacrossType.Overrideable);
            }

            return retVal;
        }
        public static MethodInfo GetMethodInfoFromMethodInfoAssist(MethodInfoAssist assist)
        {
            Type[] paramTypes = new Type[assist.Params.Count];
            for (int i = 0; i < paramTypes.Length; i++)
            {
                switch (assist.Params[i].FieldDirection)
                {
                    case System.CodeDom.FieldDirection.In:
                        if (assist.Params[i].IsParamsArray)
                            throw new InvalidOperationException("未实现");
                        else
                            paramTypes[i] = assist.Params[i].ParameterType;
                        break;
                    case System.CodeDom.FieldDirection.Out:
                    case System.CodeDom.FieldDirection.Ref:
                        if (assist.Params[i].IsParamsArray)
                            throw new InvalidOperationException("未实现");
                        else
                            paramTypes[i] = assist.Params[i].ParameterType.MakeByRefType();
                        break;
                }
                if (paramTypes[i] == null)
                {
                    return null;
                }
            }
            return assist.ParentClassType?.GetMethod(assist.MethodName, paramTypes);
        }

        //static Dictionary<EngineNS.ECSType, Dictionary<string, System.Reflection.MethodInfo>> mMethodInfoWithParamKeyDic = new Dictionary<EngineNS.ECSType, Dictionary<string, System.Reflection.MethodInfo>>();
        //public static System.Reflection.MethodInfo GetMethodInfoFromParam(string param)
        //{
        //    try
        //    {
        //        if (string.IsNullOrEmpty(param))
        //            return null;

        //        bool isGenericMethodDefinition = false;
        //        var splits = param.Split(',');
        //        EngineNS.ECSType csType = EngineNS.ECSType.All;
        //        if (splits.Length > 5)
        //            csType = (EngineNS.ECSType)EngineNS.Rtti.RttiHelper.EnumTryParse(typeof(EngineNS.ECSType), splits[5]);

        //        Dictionary<string, System.Reflection.MethodInfo> methodDic = null;
        //        if (mMethodInfoWithParamKeyDic.TryGetValue(csType, out methodDic))
        //        {
        //            System.Reflection.MethodInfo retInfo = null;
        //            if (methodDic.TryGetValue(param, out retInfo))
        //                return retInfo;
        //        }
        //        else
        //        {
        //            methodDic = new Dictionary<string, System.Reflection.MethodInfo>();
        //            mMethodInfoWithParamKeyDic[csType] = methodDic;
        //        }

        //        if (splits.Length > 6)
        //            isGenericMethodDefinition = System.Convert.ToBoolean(splits[6]);
        //        var path = splits[0];
        //        var classType = EngineNS.Rtti.RttiHelper.GetTypeFromSaveString(splits[1]);
        //        if (classType == null)
        //            return null;
        //        var methodName = splits[2];

        //        if (isGenericMethodDefinition)
        //        {
        //            var methods = classType.GetMethods();
        //            foreach (var method in methods)
        //            {
        //                var methodParam = GetParamFromMethodInfo(method, path) + "," + csType.ToString() + "," + method.IsGenericMethodDefinition;
        //                if (string.Equals(methodParam, param))
        //                {
        //                    methodDic[param] = method;
        //                    return method;
        //                }
        //            }
        //        }
        //        if (!string.IsNullOrEmpty(splits[3]))
        //        {
        //            var paramSplits = splits[3].Split('/');
        //            Type[] paramTypes = new Type[paramSplits.Length];
        //            for (int i = 0; i < paramSplits.Length; i++)
        //            {
        //                var tempSplits = paramSplits[i].Split(':');
        //                if (tempSplits.Length > 2)
        //                {
        //                    switch (tempSplits[2])
        //                    {
        //                        case "Ref":
        //                        case "Out":
        //                            tempSplits[1] += "&";
        //                            break;
        //                    }
        //                }
        //                paramTypes[i] = EngineNS.Rtti.RttiHelper.GetTypeFromSaveString(tempSplits[1]);
        //            }

        //            var retValue = classType.GetMethod(methodName, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic, null, paramTypes, null);
        //            methodDic[param] = retValue;
        //            return retValue;
        //        }
        //        else
        //        {
        //            var retValue = classType.GetMethod(methodName, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
        //            methodDic[param] = retValue;
        //            return retValue;
        //        }
        //    }
        //    catch (System.Exception e)
        //    {
        //        EngineNS.Profiler.Log.WriteLine(EngineNS.Profiler.ELogTag.Error, "Macross", e.ToString());
        //    }

        //    return null;
        //}

        public static CodeGenerateSystem.Base.CustomPropertyInfo GetFromParamInfo(MethodParamInfoAssist info)
        {
            if (info == null)
                return null;

            var retInfo = new CodeGenerateSystem.Base.CustomPropertyInfo();
            retInfo.PropertyName = info.ParamName;
            //if (info.FieldDirection == System.CodeDom.FieldDirection.Out || info.FieldDirection == System.CodeDom.FieldDirection.Ref)
            //    retInfo.PropertyType = EngineNS.Rtti.RttiHelper.GetTypeFromTypeFullName(info.ParameterType.FullName.Remove(info.ParameterType.FullName.Length - 1));
            //else
                retInfo.PropertyType = info.ParameterType;

            // todo: CustomAttributes

            retInfo.DefaultValue = CodeGenerateSystem.Program.GetDefaultValueFromType(retInfo.PropertyType);
            retInfo.CurrentValue = retInfo.DefaultValue;
            retInfo.PropertyAttributes.Add(new EngineNS.Rtti.MetaDataAttribute());
            return retInfo;
        }
        public static CodeGenerateSystem.Base.CustomPropertyInfo GetFromParamInfo(ParameterInfo info)
        {
            if (info == null)
                return null;

            var retInfo = new CodeGenerateSystem.Base.CustomPropertyInfo();
            retInfo.PropertyName = info.Name;
            retInfo.PropertyType = info.ParameterType;
            retInfo.PropertyAttributes.AddRange(info.GetCustomAttributes());

            retInfo.DefaultValue = CodeGenerateSystem.Program.GetDefaultValueFromType(retInfo.PropertyType);
            retInfo.CurrentValue = retInfo.DefaultValue;
            return retInfo;
        }

        // todo: Execute\Editor\CodeGenerate\CodeGenerate\Program.cs 中GenerateClassInitializeCode、GenerateArrayInitializeCode、GenerateGenericInitializeCode迁移
        public static void GenerateClassInitializeCode(CodeStatementCollection codeStatementCollection, Type classType, object classInstance, string paramName, bool isDeclaration)
        {
            try
            {
                CodeStatement statement = null;
                if (classInstance == null)
                {
                    // classType class = null;
                    if (isDeclaration)
                    {
                        statement = new CodeVariableDeclarationStatement(classType, paramName, new CodePrimitiveExpression(null));
                    }
                    else
                    {
                        statement = new CodeAssignStatement(new CodeVariableReferenceExpression(paramName), new CodePrimitiveExpression(null));
                    }
                    codeStatementCollection.Add(statement);
                }
                else
                {
                    // classType class = new classType();
                    if (isDeclaration)
                        statement = new CodeVariableDeclarationStatement(classType, paramName, new CodeObjectCreateExpression(classType, new CodeExpression[] { }));
                    else
                        statement = new CodeAssignStatement(new CodeVariableReferenceExpression(paramName), new CodeObjectCreateExpression(classType, new CodeExpression[] { }));
                    codeStatementCollection.Add(statement);

                    object initializeValue = null;
                    if(classType != typeof(Type))
                        initializeValue = System.Activator.CreateInstance(classType);

                    foreach (var property in classType.GetProperties())
                    {
                        if (!property.CanWrite)
                            continue;

                        var propertyValue = property.GetValue(classInstance);
                        var initValue = property.GetValue(initializeValue);
                        if (propertyValue == initValue)
                            continue;
                        var left = new CodePropertyReferenceExpression(new CodeVariableReferenceExpression(paramName), property.Name);
                        GenerateAssignCode(codeStatementCollection, left, property.PropertyType, propertyValue);
                    }
                    //foreach (var field in classType.GetFields())
                    //{
                    //    var value = field.GetValue(classInstance);
                    //    var initValue = field.GetValue(initializeValue);
                    //    if (value == initValue)
                    //        continue;
                    //    var left = new CodeFieldReferenceExpression(new CodeVariableReferenceExpression(paramName), field.Name);
                    //    GenerateAssignCode(codeStatementCollection, left, field.FieldType, value);
                    //}
                }
            }
            catch (System.Exception ex)
            {
                EngineNS.Profiler.Log.WriteLine(EngineNS.Profiler.ELogTag.Error, CodeGenerateSystem.Program.MessageCategory, "代码生成：" + ex.ToString());
            }
        }
        public static void GenerateGenericInitializeCode(CodeStatementCollection codeStatementCollection, Type genericType, object genericValue, string paramName, bool isDeclaration)
        {
            try
            {
                if (genericValue == null)
                {
                    CodeStatement statement;
                    if (isDeclaration)
                        statement = new CodeVariableDeclarationStatement(genericType, paramName, new CodePrimitiveExpression(null));
                    else
                        statement = new CodeAssignStatement(new CodeVariableReferenceExpression(paramName), new CodePrimitiveExpression(null));
                    codeStatementCollection.Add(statement);
                }
                else
                {
                    if (genericType.IsValueType)
                    {
                        var proKey = genericType.GetProperty("Key");
                        var proValue = genericType.GetProperty("Value");
                        var key = proKey.GetValue(genericValue, null);
                        var value = proValue.GetValue(genericValue, null);
                        if (key != null && value != null)
                        {
                            var keyName = paramName + "_Key";
                            CodeExpression keyExpression = null, valueExpression = null;
                            var keyType = key.GetType();
                            if (keyType.IsGenericType)
                            {
                                GenerateGenericInitializeCode(codeStatementCollection, keyType, key, keyName, isDeclaration);
                                keyExpression = new CodeArgumentReferenceExpression(keyName);
                            }
                            else if (keyType.IsArray)
                            {
                                GenerateArrayInitializeCode(codeStatementCollection, keyType, key, keyName, isDeclaration);
                                keyExpression = new CodeArgumentReferenceExpression(keyName);
                            }
                            else if (keyType == typeof(string))
                            {
                                keyExpression = new CodePrimitiveExpression(key);
                            }
                            else if (keyType.IsClass)
                            {
                                GenerateClassInitializeCode(codeStatementCollection, keyType, key, keyName, isDeclaration);
                                keyExpression = new CodeArgumentReferenceExpression(keyName);
                            }
                            else if (keyType == typeof(Guid))
                            {
                                keyExpression = new CodeSnippetExpression("CSUtility.Support.IHelper.GuidTryParse(\"" + key.ToString() + "\")");
                            }
                            else
                                keyExpression = new CodePrimitiveExpression(key);

                            var valueName = paramName + "_Value";
                            var valueType = value.GetType();
                            if (valueType.IsGenericType)
                            {
                                GenerateGenericInitializeCode(codeStatementCollection, valueType, value, valueName, isDeclaration);
                                valueExpression = new CodeArgumentReferenceExpression(valueName);
                            }
                            else if (valueType.IsArray)
                            {
                                GenerateArrayInitializeCode(codeStatementCollection, valueType, value, valueName, isDeclaration);
                                valueExpression = new CodeArgumentReferenceExpression(valueName);
                            }
                            else if (valueType == typeof(string))
                            {
                                valueExpression = new CodePrimitiveExpression(value);
                            }
                            else if (valueType.IsClass)
                            {
                                GenerateClassInitializeCode(codeStatementCollection, valueType, value, valueName, isDeclaration);
                                valueExpression = new CodeArgumentReferenceExpression(valueName);
                            }
                            else if (valueType == typeof(Guid))
                            {
                                valueExpression = new CodeSnippetExpression("CSUtility.Support.IHelper.GuidTryParse(\"" + value.ToString() + "\")");
                            }
                            else
                                valueExpression = new CodePrimitiveExpression(value);

                            CodeStatement statement;
                            // genericType param = new genericType(key, value);
                            if (isDeclaration)
                            {
                                statement = new CodeVariableDeclarationStatement(genericType, paramName, new CodeObjectCreateExpression(genericType, new CodeExpression[] {
                                                                    keyExpression,
                                                                    valueExpression}));
                            }
                            else
                            {
                                statement = new CodeAssignStatement(new CodeVariableReferenceExpression(paramName), new CodeObjectCreateExpression(genericType, new CodeExpression[] {
                                                                    keyExpression,
                                                                    valueExpression}));
                            }
                            codeStatementCollection.Add(statement);
                        }
                    }
                    else
                    {
                        var genericTypeAppString = EngineNS.Rtti.RttiHelper.GetAppTypeString(genericType);
                        CodeStatement statement;
                        if (isDeclaration)
                            statement = new CodeExpressionStatement(new CodeSnippetExpression(genericTypeAppString + " " + paramName + " = new " + genericTypeAppString + "()"));
                        else
                            statement = new CodeExpressionStatement(new CodeSnippetExpression(paramName + " = new " + genericTypeAppString + "()"));
                        //var statement = new CodeVariableDeclarationStatement(genericTypeAppString, paramName, new CodeObjectCreateExpression(genericTypeAppString, new CodeExpression[] { }));
                        codeStatementCollection.Add(statement);
                        var proCount = genericType.GetProperty("Count");
                        var enumerableValue = genericValue as IEnumerable;
                        int i = 0;
                        foreach (var item in enumerableValue)
                        {
                            if (item == null)
                            {
                                var sm = new CodeGenerateSystem.CodeDom.CodeMethodInvokeExpression(new CodeMethodReferenceExpression(new CodeArgumentReferenceExpression(paramName), "Add"),
                                                               new CodeExpression[] { new CodePrimitiveExpression(null) });
                                codeStatementCollection.Add(sm);
                            }
                            else
                            {
                                var itemName = paramName + "_" + i;

                                CodeExpression addParamExp = null;
                                var itemType = item.GetType();
                                if (itemType.IsGenericType)
                                {
                                    GenerateGenericInitializeCode(codeStatementCollection, itemType, item, itemName, isDeclaration);
                                    addParamExp = new CodeArgumentReferenceExpression(itemName);
                                }
                                else if (itemType.IsArray)
                                {
                                    GenerateArrayInitializeCode(codeStatementCollection, itemType, item, itemName, isDeclaration);
                                    addParamExp = new CodeArgumentReferenceExpression(itemName);
                                }
                                else if (itemType == typeof(string))
                                {
                                    addParamExp = new CodePrimitiveExpression(item);
                                }
                                else if (itemType.IsClass)
                                {
                                    GenerateClassInitializeCode(codeStatementCollection, itemType, item, itemName, isDeclaration);
                                    addParamExp = new CodeArgumentReferenceExpression(itemName);
                                }
                                else if (itemType == typeof(Guid))
                                {
                                    addParamExp = new CodeSnippetExpression("CSUtility.Support.IHelper.GuidTryParse(\"" + item.ToString() + "\")");
                                }
                                else
                                    addParamExp = new CodePrimitiveExpression(item);

                                // genericClass.Add(Value);
                                var sm = new CodeGenerateSystem.CodeDom.CodeMethodInvokeExpression(new CodeMethodReferenceExpression(new CodeArgumentReferenceExpression(paramName), "Add"),
                                                                new CodeExpression[] { addParamExp });
                                codeStatementCollection.Add(sm);
                            }

                            i++;
                        }
                    }
                }
            }
            catch (System.Exception ex)
            {
                EngineNS.Profiler.Log.WriteLine(EngineNS.Profiler.ELogTag.Error, CodeGenerateSystem.Program.MessageCategory, "代码生成：" + ex.ToString());
            }
        }

        public static void GenerateArrayInitializeCode(CodeStatementCollection codeStatementCollection, Type arrayType, object arrayValue, string paramName, bool isDeclaration)
        {
            try
            {
                if (arrayValue == null)
                {
                    CodeStatement statement;
                    if (isDeclaration)
                        statement = new CodeVariableDeclarationStatement(arrayType, paramName, new CodePrimitiveExpression(null));
                    else
                        statement = new CodeAssignStatement(new CodeVariableReferenceExpression(paramName), new CodePrimitiveExpression(null));

                    codeStatementCollection.Add(statement);
                }
                else
                {
                    var array = arrayValue as System.Array;
                    if (array != null)
                    {
                        CodeStatement statement;
                        if (isDeclaration)
                            statement = new CodeVariableDeclarationStatement(arrayType, paramName, new CodeArrayCreateExpression(arrayType, array.Length));
                        else
                            statement = new CodeAssignStatement(new CodeVariableReferenceExpression(paramName), new CodeArrayCreateExpression(arrayType, array.Length));
                        codeStatementCollection.Add(statement);
                        int i = 0;
                        foreach (var item in array)
                        {
                            if (item == null)
                            {
                                var sm = new CodeAssignStatement(new CodeArrayIndexerExpression(new CodeVariableReferenceExpression(paramName), new CodePrimitiveExpression(i)),
                                                                                                new CodePrimitiveExpression(null));
                                codeStatementCollection.Add(sm);
                            }
                            else
                            {
                                CodeExpression itemExp = null;
                                var itemName = paramName + "_" + i;
                                var itemType = item.GetType();
                                if (itemType.IsGenericType)
                                {
                                    GenerateGenericInitializeCode(codeStatementCollection, itemType, item, itemName, isDeclaration);
                                    itemExp = new CodeArgumentReferenceExpression(itemName);
                                }
                                else if (itemType.IsArray)
                                {
                                    GenerateArrayInitializeCode(codeStatementCollection, itemType, item, itemName, isDeclaration);
                                    itemExp = new CodeArgumentReferenceExpression(itemName);
                                }
                                else if (itemType == typeof(string))
                                {
                                    itemExp = new CodePrimitiveExpression(item);
                                }
                                else if (itemType.IsClass)
                                {
                                    GenerateClassInitializeCode(codeStatementCollection, itemType, item, itemName, isDeclaration);
                                    itemExp = new CodeArgumentReferenceExpression(itemName);
                                }
                                else if (itemType == typeof(Guid))
                                {
                                    itemExp = new CodeSnippetExpression("CSUtility.Support.IHelper.GuidTryParse(\"" + item.ToString() + "\")");
                                }
                                else
                                    itemExp = new CodePrimitiveExpression(item);

                                var sm = new CodeAssignStatement(new CodeArrayIndexerExpression(new CodeVariableReferenceExpression(paramName), new CodePrimitiveExpression(i)),
                                                                                                itemExp);
                                codeStatementCollection.Add(sm);
                            }

                            i++;
                        }
                    }
                }
            }
            catch (System.Exception ex)
            {
                EngineNS.Profiler.Log.WriteLine(EngineNS.Profiler.ELogTag.Error, CodeGenerateSystem.Program.MessageCategory, "代码生成：" + ex.ToString());
            }
        }

        public static CodeExpression GetValueCode(CodeStatementCollection codeStatementCollection, Type type, object value)
        {
            if (value == null)
            {
                return new CodePrimitiveExpression(null);
            }
            else if (type.IsPointer)
            {
                return new CodePrimitiveExpression(null);
            }
            else if (type == typeof(IntPtr))
            {

            }
            else if(type == typeof(System.Type))
            {
                var val = (Type)value;
                return new CodeTypeOfExpression(val);
            }
            else if (type.FullName == typeof(EngineNS.RName).FullName)
            {
                var rNameValue = value as EngineNS.RName;
                return new CodeSnippetExpression($"EngineNS.CEngine.Instance.FileManager.GetRName(\"{rNameValue.Name}\", {EngineNS.Rtti.RttiHelper.GetAppTypeString(typeof(EngineNS.RName.enRNameType))}.{rNameValue.RNameType.ToString()})");
            }
            else if (type.IsGenericType)
            {
                var tempName = "val_" + EngineNS.Editor.Assist.GetValuedGUIDString(Guid.NewGuid());
                GenerateGenericInitializeCode(codeStatementCollection, type, value, tempName, true);
                return new CodeVariableReferenceExpression(tempName);
            }
            else if (type.IsGenericParameter)
            {
                throw new InvalidOperationException("未实现");
            }
            else if (type.IsArray)
            {
                var tempName = "val_" + EngineNS.Editor.Assist.GetValuedGUIDString(Guid.NewGuid());
                GenerateArrayInitializeCode(codeStatementCollection, type, value, tempName, true);
                return new CodeVariableReferenceExpression(tempName);
            }
            else if (type == typeof(string))
            {
                return new CodePrimitiveExpression(value);
            }
            else if (type.IsClass)
            {
                var tempName = "val_" + EngineNS.Editor.Assist.GetValuedGUIDString(Guid.NewGuid());
                GenerateClassInitializeCode(codeStatementCollection, type, value, tempName, true);
                return new CodeVariableReferenceExpression(tempName);
            }
            else if (type == typeof(EngineNS.Vector2))
            {
                var val = (EngineNS.Vector2)value;
                return new CodeObjectCreateExpression(type,
                                                new CodePrimitiveExpression(val.X),
                                                new CodePrimitiveExpression(val.Y));
            }
            else if (type == typeof(EngineNS.Vector3))
            {
                var val = (EngineNS.Vector3)value;
                return new CodeObjectCreateExpression(type,
                                                new CodePrimitiveExpression(val.X),
                                                new CodePrimitiveExpression(val.Y),
                                                new CodePrimitiveExpression(val.Z));
            }
            else if (type == typeof(EngineNS.Vector4))
            {
                var val = (EngineNS.Vector4)value;
                return new CodeObjectCreateExpression(type,
                                                new CodePrimitiveExpression(val.X),
                                                new CodePrimitiveExpression(val.Y),
                                                new CodePrimitiveExpression(val.Z),
                                                new CodePrimitiveExpression(val.W));
            }
            else if (type == typeof(EngineNS.Quaternion))
            {
                var val = (EngineNS.Quaternion)value;
                return new CodeObjectCreateExpression(type,
                                                new CodePrimitiveExpression(val.X),
                                                new CodePrimitiveExpression(val.Y),
                                                new CodePrimitiveExpression(val.Z),
                                                new CodePrimitiveExpression(val.W));
            }
            else if (type == typeof(EngineNS.Matrix))
            {
                var val = (EngineNS.Matrix)value;
                var tempName = "val_" + EngineNS.Editor.Assist.GetValuedGUIDString(Guid.NewGuid());
                var statment = new CodeVariableDeclarationStatement(typeof(EngineNS.Matrix), tempName, new CodeObjectCreateExpression(typeof(EngineNS.Matrix), new CodeExpression[0]));
                codeStatementCollection.Add(statment);
                var left = new CodeVariableReferenceExpression(tempName);
                var assignStatement = new CodeAssignStatement(new CodeFieldReferenceExpression(left, "M11"), new CodePrimitiveExpression(val.M11));
                codeStatementCollection.Add(assignStatement);
                assignStatement = new CodeAssignStatement(new CodeFieldReferenceExpression(left, "M12"), new CodePrimitiveExpression(val.M12));
                codeStatementCollection.Add(assignStatement);
                assignStatement = new CodeAssignStatement(new CodeFieldReferenceExpression(left, "M13"), new CodePrimitiveExpression(val.M13));
                codeStatementCollection.Add(assignStatement);
                assignStatement = new CodeAssignStatement(new CodeFieldReferenceExpression(left, "M14"), new CodePrimitiveExpression(val.M14));
                codeStatementCollection.Add(assignStatement);
                assignStatement = new CodeAssignStatement(new CodeFieldReferenceExpression(left, "M21"), new CodePrimitiveExpression(val.M21));
                codeStatementCollection.Add(assignStatement);
                assignStatement = new CodeAssignStatement(new CodeFieldReferenceExpression(left, "M22"), new CodePrimitiveExpression(val.M22));
                codeStatementCollection.Add(assignStatement);
                assignStatement = new CodeAssignStatement(new CodeFieldReferenceExpression(left, "M23"), new CodePrimitiveExpression(val.M23));
                codeStatementCollection.Add(assignStatement);
                assignStatement = new CodeAssignStatement(new CodeFieldReferenceExpression(left, "M24"), new CodePrimitiveExpression(val.M24));
                codeStatementCollection.Add(assignStatement);
                assignStatement = new CodeAssignStatement(new CodeFieldReferenceExpression(left, "M31"), new CodePrimitiveExpression(val.M31));
                codeStatementCollection.Add(assignStatement);
                assignStatement = new CodeAssignStatement(new CodeFieldReferenceExpression(left, "M32"), new CodePrimitiveExpression(val.M32));
                codeStatementCollection.Add(assignStatement);
                assignStatement = new CodeAssignStatement(new CodeFieldReferenceExpression(left, "M33"), new CodePrimitiveExpression(val.M33));
                codeStatementCollection.Add(assignStatement);
                assignStatement = new CodeAssignStatement(new CodeFieldReferenceExpression(left, "M34"), new CodePrimitiveExpression(val.M34));
                codeStatementCollection.Add(assignStatement);
                assignStatement = new CodeAssignStatement(new CodeFieldReferenceExpression(left, "M41"), new CodePrimitiveExpression(val.M41));
                codeStatementCollection.Add(assignStatement);
                assignStatement = new CodeAssignStatement(new CodeFieldReferenceExpression(left, "M42"), new CodePrimitiveExpression(val.M42));
                codeStatementCollection.Add(assignStatement);
                assignStatement = new CodeAssignStatement(new CodeFieldReferenceExpression(left, "M43"), new CodePrimitiveExpression(val.M43));
                codeStatementCollection.Add(assignStatement);
                assignStatement = new CodeAssignStatement(new CodeFieldReferenceExpression(left, "M44"), new CodePrimitiveExpression(val.M44));
                codeStatementCollection.Add(assignStatement);
                return left;
            }
            else if (type == typeof(EngineNS.Color4))
            {
                var val = (EngineNS.Color4)value;
                return new CodeObjectCreateExpression(type,
                                                new CodePrimitiveExpression(val.Alpha),
                                                new CodePrimitiveExpression(val.Red),
                                                new CodePrimitiveExpression(val.Green),
                                                new CodePrimitiveExpression(val.Blue));
            }
            else if (type == typeof(EngineNS.Color3))
            {
                var val = (EngineNS.Color3)value;
                return new CodeObjectCreateExpression(type,
                                                new CodePrimitiveExpression(val.Red),
                                                new CodePrimitiveExpression(val.Green),
                                                new CodePrimitiveExpression(val.Blue));
            }
            else if (type == typeof(EngineNS.Color))
            {
                var val = (EngineNS.Color)value;
                return new CodeMethodInvokeExpression(new CodeSnippetExpression(nameof(EngineNS.Color)), "FromArgb",
                                                                new CodePrimitiveExpression(val.A),
                                                                new CodePrimitiveExpression(val.R),
                                                                new CodePrimitiveExpression(val.G),
                                                                new CodePrimitiveExpression(val.B));
            }
            else if (type == typeof(Guid))
            {
                return new System.CodeDom.CodeSnippetExpression("EngineNS.Rtti.RttiHelper.GuidTryParse(\"" + value.ToString() + "\")");
            }
            else if (type.IsEnum)
            {
                return new CodePrimitiveExpression(value);
            }
            else if (type.IsValueType)
            {
                if (type.IsPrimitive)
                {
                    return new CodePrimitiveExpression(value);
                }
                else
                {
                    var tempName = "val_" + EngineNS.Editor.Assist.GetValuedGUIDString(Guid.NewGuid());

                    GenerateClassInitializeCode(codeStatementCollection, type, value, tempName, true);
                    return new CodeVariableReferenceExpression(tempName);
                }
            }
            else
            {
                throw new InvalidOperationException("未实现");
            }
            return null;
        }
        public static CodeAssignStatement GenerateAssignCode(CodeStatementCollection codeStatementCollection, CodeExpression left, Type type, object value)
        {
            var exp = GetValueCode(codeStatementCollection, type, value);
            var retStatement = new CodeAssignStatement(left, exp);
            codeStatementCollection.Add(retStatement);
            return retStatement;
            //if(value == null)
            //{
            //    var retStatment = new CodeAssignStatement(left, new CodePrimitiveExpression(null));
            //    codeStatementCollection.Add(retStatment);
            //    return retStatment;
            //}
            //else if (type.IsPointer)
            //{

            //}
            //else if(type == typeof(IntPtr))
            //{

            //}
            //else if(type.FullName == typeof(EngineNS.RName).FullName)
            //{
            //    var rNameValue = value as EngineNS.RName;
            //    var retStatement = new CodeAssignStatement(left, new CodeSnippetExpression($"EngineNS.CEngine.Instance.FileManager.GetRName(\"{rNameValue.Name}\", {EngineNS.Rtti.RttiHelper.GetAppTypeString(typeof(EngineNS.RName.enRNameType))}.{rNameValue.RNameType.ToString()})"));
            //    codeStatementCollection.Add(retStatement);
            //    return retStatement;
            //}
            //else if(type.IsGenericType)
            //{
            //    var tempName = "val_" + EngineNS.Editor.Assist.GetValuedGUIDString(Guid.NewGuid());
            //    GenerateGenericInitializeCode(codeStatementCollection, type, value, tempName, true);
            //    var retStatment = new CodeAssignStatement(left, new CodeVariableReferenceExpression(tempName));
            //    codeStatementCollection.Add(retStatment);
            //    return retStatment;
            //}
            //else if(type.IsGenericParameter)
            //{
            //    throw new InvalidOperationException("未实现");
            //}
            //else if(type.IsArray)
            //{
            //    var tempName = "val_" + EngineNS.Editor.Assist.GetValuedGUIDString(Guid.NewGuid());
            //    GenerateArrayInitializeCode(codeStatementCollection, type, value, tempName, true);
            //    var retStatment = new CodeAssignStatement(left, new CodeVariableReferenceExpression(tempName));
            //    codeStatementCollection.Add(retStatment);
            //    return retStatment;
            //}
            //else if(type == typeof(string))
            //{
            //    var retStatement = new CodeAssignStatement(left, new CodePrimitiveExpression(value));
            //    codeStatementCollection.Add(retStatement);
            //    return retStatement;
            //}
            //else if(type.IsClass)
            //{
            //    var tempName = "val_" + EngineNS.Editor.Assist.GetValuedGUIDString(Guid.NewGuid());
            //    GenerateClassInitializeCode(codeStatementCollection, type, value, tempName, true);
            //    var retStatment = new CodeAssignStatement(left, new CodeVariableReferenceExpression(tempName));
            //    codeStatementCollection.Add(retStatment);
            //    return retStatment;
            //}
            //else if(type == typeof(EngineNS.Vector2))
            //{
            //    var val = (EngineNS.Vector2)value;
            //    var retStatment = new CodeAssignStatement(left, new CodeObjectCreateExpression(type,
            //                                                                new CodePrimitiveExpression(val.X),
            //                                                                new CodePrimitiveExpression(val.Y)));
            //    codeStatementCollection.Add(retStatment);
            //    return retStatment;
            //}
            //else if(type == typeof(EngineNS.Vector3))
            //{
            //    var val = (EngineNS.Vector3)value;
            //    var retStatment = new CodeAssignStatement(left, new CodeObjectCreateExpression(type,
            //                                                                new CodePrimitiveExpression(val.X),
            //                                                                new CodePrimitiveExpression(val.Y),
            //                                                                new CodePrimitiveExpression(val.Z)));
            //    codeStatementCollection.Add(retStatment);
            //    return retStatment;
            //}
            //else if(type == typeof(EngineNS.Vector4))
            //{
            //    var val = (EngineNS.Vector4)value;
            //    var retStatment = new CodeAssignStatement(left, new CodeObjectCreateExpression(type,
            //                                                                new CodePrimitiveExpression(val.X),
            //                                                                new CodePrimitiveExpression(val.Y),
            //                                                                new CodePrimitiveExpression(val.Z),
            //                                                                new CodePrimitiveExpression(val.W)));
            //    codeStatementCollection.Add(retStatment);
            //    return retStatment;
            //}
            //else if(type == typeof(EngineNS.Quaternion))
            //{
            //    var val = (EngineNS.Quaternion)value;
            //    var retStatment = new CodeAssignStatement(left, new CodeObjectCreateExpression(type,
            //                                                                new CodePrimitiveExpression(val.X),
            //                                                                new CodePrimitiveExpression(val.Y),
            //                                                                new CodePrimitiveExpression(val.Z),
            //                                                                new CodePrimitiveExpression(val.W)));
            //    codeStatementCollection.Add(retStatment);
            //    return retStatment;
            //}
            //else if (type == typeof(EngineNS.Matrix))
            //{
            //    var val = (EngineNS.Matrix)value;
            //    var statement = new CodeAssignStatement(left, new CodeObjectCreateExpression(type, new CodeExpression[] { }));
            //    codeStatementCollection.Add(statement);
            //    var assignStatement = new CodeAssignStatement(new CodeFieldReferenceExpression(left, "M11"), new CodePrimitiveExpression(val.M11));
            //    codeStatementCollection.Add(assignStatement);
            //    assignStatement = new CodeAssignStatement(new CodeFieldReferenceExpression(left, "M12"), new CodePrimitiveExpression(val.M12));
            //    codeStatementCollection.Add(assignStatement);
            //    assignStatement = new CodeAssignStatement(new CodeFieldReferenceExpression(left, "M13"), new CodePrimitiveExpression(val.M13));
            //    codeStatementCollection.Add(assignStatement);
            //    assignStatement = new CodeAssignStatement(new CodeFieldReferenceExpression(left, "M14"), new CodePrimitiveExpression(val.M14));
            //    codeStatementCollection.Add(assignStatement);
            //    assignStatement = new CodeAssignStatement(new CodeFieldReferenceExpression(left, "M21"), new CodePrimitiveExpression(val.M21));
            //    codeStatementCollection.Add(assignStatement);
            //    assignStatement = new CodeAssignStatement(new CodeFieldReferenceExpression(left, "M22"), new CodePrimitiveExpression(val.M22));
            //    codeStatementCollection.Add(assignStatement);
            //    assignStatement = new CodeAssignStatement(new CodeFieldReferenceExpression(left, "M23"), new CodePrimitiveExpression(val.M23));
            //    codeStatementCollection.Add(assignStatement);
            //    assignStatement = new CodeAssignStatement(new CodeFieldReferenceExpression(left, "M24"), new CodePrimitiveExpression(val.M24));
            //    codeStatementCollection.Add(assignStatement);
            //    assignStatement = new CodeAssignStatement(new CodeFieldReferenceExpression(left, "M31"), new CodePrimitiveExpression(val.M31));
            //    codeStatementCollection.Add(assignStatement);
            //    assignStatement = new CodeAssignStatement(new CodeFieldReferenceExpression(left, "M32"), new CodePrimitiveExpression(val.M32));
            //    codeStatementCollection.Add(assignStatement);
            //    assignStatement = new CodeAssignStatement(new CodeFieldReferenceExpression(left, "M33"), new CodePrimitiveExpression(val.M33));
            //    codeStatementCollection.Add(assignStatement);
            //    assignStatement = new CodeAssignStatement(new CodeFieldReferenceExpression(left, "M34"), new CodePrimitiveExpression(val.M34));
            //    codeStatementCollection.Add(assignStatement);
            //    assignStatement = new CodeAssignStatement(new CodeFieldReferenceExpression(left, "M41"), new CodePrimitiveExpression(val.M41));
            //    codeStatementCollection.Add(assignStatement);
            //    assignStatement = new CodeAssignStatement(new CodeFieldReferenceExpression(left, "M42"), new CodePrimitiveExpression(val.M42));
            //    codeStatementCollection.Add(assignStatement);
            //    assignStatement = new CodeAssignStatement(new CodeFieldReferenceExpression(left, "M43"), new CodePrimitiveExpression(val.M43));
            //    codeStatementCollection.Add(assignStatement);
            //    assignStatement = new CodeAssignStatement(new CodeFieldReferenceExpression(left, "M44"), new CodePrimitiveExpression(val.M44));
            //    codeStatementCollection.Add(assignStatement);
            //    return statement;
            //}
            //else if(type == typeof(Guid))
            //{
            //    var retStatment = new CodeAssignStatement(left, new System.CodeDom.CodeSnippetExpression("EngineNS.Rtti.RttiHelper.GuidTryParse(\"" + value.ToString() + "\")"));
            //    codeStatementCollection.Add(retStatment);
            //    return retStatment;
            //}
            //else if(type.IsEnum)
            //{
            //    var retStatment = new CodeAssignStatement(left, new CodeArgumentReferenceExpression(EngineNS.Rtti.RttiHelper.GetAppTypeString(value.GetType()) + "." + value.ToString()));
            //    codeStatementCollection.Add(retStatment);
            //    return retStatment;
            //}
            //else if(type.IsValueType)
            //{
            //    if(type.IsPrimitive)
            //    {
            //        var retStatment = new CodeAssignStatement(left, new CodePrimitiveExpression(value));
            //        codeStatementCollection.Add(retStatment);
            //        return retStatment;
            //    }
            //    else
            //    {
            //        var tempName = "val_" + EngineNS.Editor.Assist.GetValuedGUIDString(Guid.NewGuid());

            //        GenerateClassInitializeCode(codeStatementCollection, type, value, tempName, true);
            //        var retStatment = new CodeAssignStatement(left, new CodeVariableReferenceExpression(tempName));
            //        codeStatementCollection.Add(retStatment);
            //        return retStatment;
            //    }
            //}
            //else
            //{
            //    throw new InvalidOperationException("未实现");
            //}
            //return null;
        }

        public static bool IsTypeValidInPropertyGridShow(Type type)
        {
            if (type.IsEnum)
                return true;
            if (type.IsValueType && type.IsPrimitive)
                return true;
            if (type == typeof(string))
                return true;
            if (type == typeof(EngineNS.RName))
                return true;
            if (type == typeof(EngineNS.Vector2))
                return true;
            if (type == typeof(EngineNS.Vector3))
                return true;
            if (type == typeof(EngineNS.Vector4))
                return true;
            if (type == typeof(EngineNS.Quaternion))
                return true;
            if (type == typeof(Type))
                return true;
            //if (type.IsClass)
            //    return false;
            return false;
        }
    }
}
