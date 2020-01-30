using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Windows.Media;

namespace CodeDomNode
{
    // 类成员数据
    class ClassMemberData
    {
        /// <summary>
        /// 类类型
        /// </summary>
        public Type ClassType
        {
            get;
            private set;
        }
        /// <summary>
        /// 属性信息
        /// </summary>
        public List<PropertyInfo> PropertyInfos
        {
            get;
            private set;
        } = new List<PropertyInfo>();
        /// <summary>
        /// 函数信息
        /// </summary>
        public List<MethodInfo> MethodInfos
        {
            get;
            private set;
        } = new List<MethodInfo>();
        /// <summary>
        /// 继承类
        /// </summary>
        public Dictionary<Type, Type> InheritanceClasses
        {
            get;
            private set;
        } = new Dictionary<Type, Type>();

        public ClassMemberData(Type type, PropertyInfo[] propertyInfos, MethodInfo[] methodInfos)
        {
            ClassType = type;
            PropertyInfos = new List<PropertyInfo>(propertyInfos);
            MethodInfos = new List<MethodInfo>(methodInfos);
        }

        /// <summary>
        /// 计算classType在assemblys中的父类，并将其加入ClassMemberData中
        /// </summary>
        /// <param name="classType"></param>
        /// <param name="assemblys"></param>
        /// <param name="dataDictionary"></param>
        public static void CalculateInheritanceClasses(Type classType, List<EngineNS.Rtti.VAssembly> assemblys, Dictionary<string, CodeDomNode.ClassMemberData> dataDictionary)
        {
            if (classType == null)
                return;

            var baseType = classType.BaseType;
            var vAssem = EngineNS.Rtti.VAssemblyManager.Instance.GetAssemblyFromType(baseType);
            if (baseType != null && assemblys.Contains(vAssem))
            {
                if (baseType.FullName == null)
                    return;
                CodeDomNode.ClassMemberData data;
                if (dataDictionary.TryGetValue(baseType.FullName, out data))
                {
                    data.InheritanceClasses[classType] = classType;
                }

                CalculateInheritanceClasses(baseType, assemblys, dataDictionary);
            }

            var interfaceTypes = classType.GetInterfaces();
            foreach (var infType in interfaceTypes)
            {
                var vInfAssem = EngineNS.Rtti.VAssemblyManager.Instance.GetAssemblyFromType(infType);
                if (assemblys.Contains(vInfAssem))
                {
                    CodeDomNode.ClassMemberData data;
                    if (dataDictionary.TryGetValue(infType.FullName, out data))
                    {
                        data.InheritanceClasses[classType] = classType;
                    }

                    CalculateInheritanceClasses(infType, assemblys, dataDictionary);
                }
            }
        }
    }

    public class AIEditorExtendData
    {
        public Guid FSMID;
        public string ClassName;
    }

    public partial class Program
    {
        public static string GetPropertyKeyword(PropertyInfo prop)
        {
            return prop.Name + "%" + EngineNS.Rtti.RttiHelper.GetTypeSaveString(prop.PropertyType);
        }
        public static string GetMethodKeyword(MethodInfo method)
        {
            string strRet = method.Name + "%";

            System.Reflection.ParameterInfo[] parInfos = method.GetParameters();
            if (parInfos.Length > 0)
            {
                foreach (var pInfo in parInfos)
                {
                    var parameterTypeString = EngineNS.Rtti.RttiHelper.GetTypeSaveString(pInfo.ParameterType);
                    string strFlag = "";
                    if (pInfo.IsOut)
                    {
                        strFlag = ":Out";
                        parameterTypeString = parameterTypeString.Remove(parameterTypeString.Length - 1);
                    }
                    else if (pInfo.ParameterType.IsByRef)
                    {
                        strFlag = ":Ref";
                        parameterTypeString = parameterTypeString.Remove(parameterTypeString.Length - 1);
                    }
                    strRet += pInfo.Name + ":" + parameterTypeString + strFlag + "/";
                }
                strRet = strRet.Remove(strRet.Length - 1);   // 去除最后一个"/"
            }

            strRet += "%" + EngineNS.Rtti.RttiHelper.GetTypeSaveString(method.ReturnType);

            return strRet;
        }

        // 添加函数节点
        public static CodeGenerateSystem.Controls.NodeListAttributeClass AddMethodNode(CodeGenerateSystem.Controls.NodesContainerControl.ContextMenuFilterData filterData, System.Reflection.MethodInfo methodInfo, Type parentClassType, CodeGenerateSystem.Controls.NodeListControl hostNodesList, EngineNS.ECSType csType, string attributeName, CodeDomNode.MethodInfoAssist.enHostType hostType)
        {
            //"CSUtility.Event.Attribute.AllowMember"
            //

            CodeGenerateSystem.Controls.NodeListAttributeClass attribute = null;
            var att = EngineNS.Rtti.AttributeHelper.GetCustomAttribute(methodInfo, attributeName, true);
            if (att == null)
                return attribute;

            var path = EngineNS.Rtti.AttributeHelper.GetCustomAttributePropertyValue(att, "Path")?.ToString();
            if (string.IsNullOrEmpty(path))
                path = EngineNS.Rtti.RttiHelper.GetAppTypeString(methodInfo.ReflectedType) + "." + methodInfo.Name;
            var description = EngineNS.Rtti.AttributeHelper.GetCustomAttributePropertyValue(att, "Description").ToString();

            //var refTypeStr = EngineNS.Rtti.RttiHelper.GetAppTypeString(methodInfo.ReflectedType);
            //var methodInfoString = GetParamFromMethodInfo(methodInfo, path);
            //methodInfoString += "," + csType.ToString() + "," + methodInfo.IsGenericMethodDefinition;
            //if (usefulMemberData != null)
            //{
            //    var usefulMemberStr = usefulMemberData.ToString().Replace(usefulMemberData.ClassTypeFullName, refTypeStr);
            //    methodInfoString += ";" + usefulMemberStr;
            //    if (!string.IsNullOrEmpty(usefulMemberData.MemberHostName))
            //    {
            //        path = usefulMemberData.MemberHostName + "." + path;
            //    }
            //    if (usefulMemberData.HostControl != null)
            //    {
            //        path = GetNodeName(usefulMemberData) + "." + path;
            //    }
            //}
            //path += "(" + refTypeStr + ")";
            var csParam = new CodeDomNode.MethodInvokeNode.MethodNodeConstructionParams()
            {
                CSType = csType,
                ConstructParam = "",
                MethodInfo = GetMethodInfoAssistFromMethodInfo(methodInfo, parentClassType, hostType, path),
            };

            var displayatt = methodInfo.GetCustomAttributes(typeof(EngineNS.Editor.DisplayParamNameAttribute), true);
            if (displayatt.Length > 0)
            {
                csParam.DisplayName = ((EngineNS.Editor.DisplayParamNameAttribute)displayatt[0]).DisplayName;
            }

            var attrs = methodInfo.GetCustomAttributes(typeof(EngineNS.Editor.MacrossPanelPathAttribute), true);
            if (attrs.Length == 0)
            {
                path = path.Replace('.', '/');
            }
            else
            {
                path = ((EngineNS.Editor.MacrossPanelPathAttribute)attrs[0]).Path;
            }

            attribute = hostNodesList.AddNodesFromType(filterData, typeof(CodeDomNode.MethodInvokeNode), path, csParam, description, "", hostNodesList.TryFindResource("Icon_Function") as ImageSource);
            return attribute;
        }

        //public static string GetNodeName(CodeGenerateSystem.Base.UsefulMemberHostData usefulMemberData)
        //{
        //    if (usefulMemberData != null && usefulMemberData.HostControl != null)
        //    {
        //        var name = usefulMemberData.HostControl.NodeName;
        //        var index = name.IndexOf("(");
        //        if (index > 0)
        //        {
        //            name = name.Substring(0, index);
        //        }
        //        return name.Replace(".", "-");
        //    }
        //    return string.Empty;
        //}

        // 添加属性节点
        public static CodeGenerateSystem.Controls.NodeListAttributeClass AddPropertyNode(CodeGenerateSystem.Controls.NodesContainerControl.ContextMenuFilterData filterData, PropertyInfo propertyInfo, Type parentClassType, CodeGenerateSystem.Controls.NodeListControl hostNodesList, CodeDomNode.PropertyInfoAssist.enDirection direction, EngineNS.ECSType csType, string attributeName, CodeDomNode.MethodInfoAssist.enHostType hostType)
        {
            // "CSUtility.Event.Attribute.AllowMember"
            // "CSUtility.AISystem.Attribute.AllowMember"
            var att = EngineNS.Rtti.AttributeHelper.GetCustomAttribute(propertyInfo, attributeName, true);
            if (att == null)
                return null;
            
            var description = EngineNS.Rtti.AttributeHelper.GetCustomAttributePropertyValue(att, "Description").ToString();

            string path = EngineNS.Rtti.AttributeHelper.GetCustomAttributePropertyValue(att, "Path")?.ToString();
            if (string.IsNullOrEmpty(path))
                path = EngineNS.Rtti.RttiHelper.GetAppTypeString(propertyInfo.ReflectedType) + "." + propertyInfo.Name;

            var csParam = new CodeDomNode.PropertyNode.PropertyNodeConstructionParams()
            {
                CSType = csType,
                ConstructParam = "",
                PropertyInfo = CodeDomNode.PropertyNode.GetAssistFromPropertyInfo(propertyInfo, parentClassType, direction, path, hostType),
            };

            var mpattr = propertyInfo.GetCustomAttributes(typeof(EngineNS.Editor.MacrossPanelPathAttribute), false);
            if (mpattr.Length > 0)
            {
                path = ((EngineNS.Editor.MacrossPanelPathAttribute)mpattr[0]).Path;
            }
            else
            {
                path = path.Replace('.', '/');
            }

            var displayattr = propertyInfo.GetCustomAttributes(typeof(System.ComponentModel.DisplayNameAttribute), false);
            if (displayattr.Length > 0)
            {
                csParam.DisplayName = ((System.ComponentModel.DisplayNameAttribute)displayattr[0]).DisplayName;
            }
            var attribute = hostNodesList.AddNodesFromType(filterData, typeof(CodeDomNode.PropertyNode), $"{path}({PropertyNode.GetParamPreInfo(direction)})", csParam, description);

            return attribute;
        }
        public static CodeGenerateSystem.Controls.NodeListAttributeClass AddFieldNode(CodeGenerateSystem.Controls.NodesContainerControl.ContextMenuFilterData filterData, FieldInfo fieldInfo, Type parentClassType, CodeGenerateSystem.Controls.NodeListControl hostNodesList, CodeDomNode.PropertyInfoAssist.enDirection direction, EngineNS.ECSType csType, string attributeName, CodeDomNode.MethodInfoAssist.enHostType hostType)
        {
            var att = EngineNS.Rtti.AttributeHelper.GetCustomAttribute(fieldInfo, attributeName, true);
            if (att == null)
                return null;

            var path = EngineNS.Rtti.AttributeHelper.GetCustomAttributePropertyValue(att, "Path")?.ToString();
            if (string.IsNullOrEmpty(path))
                path = EngineNS.Rtti.RttiHelper.GetAppTypeString(fieldInfo.ReflectedType) + "." + fieldInfo.Name;
            var description = EngineNS.Rtti.AttributeHelper.GetCustomAttributePropertyValue(att, "Description").ToString();

            var csParam = new CodeDomNode.PropertyNode.PropertyNodeConstructionParams()
            {
                CSType = csType,
                ConstructParam = "",
                PropertyInfo = CodeDomNode.PropertyNode.GetAssistFromFieldInfo(fieldInfo, parentClassType, direction, path, hostType),
            };

            var mpattr = fieldInfo.GetCustomAttributes(typeof(EngineNS.Editor.MacrossPanelPathAttribute), false);
            if (mpattr.Length > 0)
            {
                path = ((EngineNS.Editor.MacrossPanelPathAttribute)mpattr[0]).Path;
            }
            else
            {
                path = path.Replace('.', '/');
            }

            var displayattr = fieldInfo.GetCustomAttributes(typeof(System.ComponentModel.DisplayNameAttribute), false);
            if (displayattr.Length > 0)
            {
                csParam.DisplayName = ((System.ComponentModel.DisplayNameAttribute)displayattr[0]).DisplayName;
            }
            return hostNodesList.AddNodesFromType(filterData, typeof(CodeDomNode.PropertyNode), $"{path}({PropertyNode.GetParamPreInfo(direction)})", csParam, description);
        }

        //public static void InitializeUnionNodes(CodeGenerateSystem.Controls.NodeListControl nodesListCtrl, EngineNS.ECSType csType)
        //{
        //    var dir = CSUtility.Support.IFileConfig.DefaultMethodDirectory;
        //    var absDir = CSUtility.Support.IFileManager.Instance.Root + dir;

        //    if (!System.IO.Directory.Exists(absDir))
        //    {
        //        return;
        //    }

        //    System.IO.DirectoryInfo folder = new System.IO.DirectoryInfo(absDir);
        //    System.IO.DirectoryInfo[] childFloders = folder.GetDirectories();
        //    foreach(var childFloder in childFloders)
        //    {
        //        System.IO.FileSystemInfo[] files = childFloder.GetFileSystemInfos();

        //        string strPath = null;
        //        foreach (var file in files)
        //        {
        //            if (file.FullName.EndsWith("_Path" + ".xml"))
        //            {
        //                // 保存节点路径
        //                var xmlPathHolder = CSUtility.Support.XmlHolder.LoadXML(file.FullName);
        //                var pathAtt = xmlPathHolder.RootNode.FindAttrib("Path");
        //                if (pathAtt != null)
        //                    strPath = pathAtt.Value;
        //            }
        //        }

        //        foreach (var file in files)
        //        {
        //            if (file.FullName.EndsWith("_" + csType.ToString() + ".xml"))
        //            {
        //                var xmlHolder = CSUtility.Support.XmlHolder.LoadXML(file.FullName);
        //                foreach (var xmlNode in xmlHolder.RootNode.GetNodes())
        //                {
        //                    var Id = Guid.Parse(xmlNode.FindAttrib("ID").Value);

        //                    var paramAtt = xmlNode.FindAttrib("Params");
        //                    string strParam = null;
        //                    if (paramAtt != null)
        //                        strParam = paramAtt.Value;

        //                    CodeGenerateSystem.Controls.NodeListAttributeClass attribute = nodesListCtrl.AddNodesFromType(typeof(CodeDomNode.MethodInvokeUnionNode), strPath, strParam, "", null, file.FullName);
        //                }
        //            }
                   
        //        }
        //    }
        //}

        public class MacrossMemberCollectData
        {
            public CodeGenerateSystem.Controls.NodeListControl NodesListCtrl;
            public Type ClassType;
            public string AttributeName;
            public EngineNS.ECSType CSType;
            public CodeDomNode.MethodInfoAssist.enHostType HostType;
            public bool IgnoreStatic = false;
        }
        public static void InitializeMacrossMembers(CodeGenerateSystem.Controls.NodesContainerControl.ContextMenuFilterData filterData, MacrossMemberCollectData data)
        {
            var memberClass = data.ClassType;
            if(data.ClassType.IsPointer)
            {
                var typeName = data.ClassType.FullName.Remove(data.ClassType.FullName.Length - 1);
                memberClass = data.ClassType.Assembly.GetType(typeName);
            }

            FieldInfo[] fields;
            PropertyInfo[] propertys;
            MethodInfo[] methods;
            if (data.HostType == MethodInfoAssist.enHostType.Static)
            {
                fields = memberClass.GetFields(BindingFlags.Public | BindingFlags.Static);
                propertys = memberClass.GetProperties(BindingFlags.Public | BindingFlags.Static);
                methods = memberClass.GetMethods(BindingFlags.Public | BindingFlags.Static);
            }
            else
            {
                fields = memberClass.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
                propertys = memberClass.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
                methods = memberClass.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
            }
            //var memberData = new CodeGenerateSystem.Base.UsefulMemberHostData()
            //{
            //    ClassTypeFullName = classType.FullName,
            //    HostType = CodeGenerateSystem.Base.UsefulMemberHostData.enHostType.This,
            //};
            //var staticMemberData = new CodeGenerateSystem.Base.UsefulMemberHostData()
            //{
            //    ClassTypeFullName = classType.FullName,
            //    HostType = CodeGenerateSystem.Base.UsefulMemberHostData.enHostType.Static,
            //};
            
            foreach (var fieldInfo in fields)
            {
                bool readOnly = false;
                var att = EngineNS.Rtti.AttributeHelper.GetCustomAttribute(fieldInfo, data.AttributeName, true);
                if (att != null)
                {
                    var mt = (EngineNS.Editor.MacrossMemberAttribute.enMacrossType)EngineNS.Rtti.AttributeHelper.GetCustomAttributePropertyValue(att, "MacrossType");
                    readOnly = EngineNS.Editor.MacrossMemberAttribute.HasType(mt, EngineNS.Editor.MacrossMemberAttribute.enMacrossType.ReadOnly);
                }

                if (fieldInfo.IsStatic)
                {
                    if(!data.IgnoreStatic)
                    {
                        if (!readOnly)
                            AddFieldNode(filterData, fieldInfo, data.ClassType, data.NodesListCtrl, PropertyInfoAssist.enDirection.Set, data.CSType, data.AttributeName, MethodInfoAssist.enHostType.Static);
                        AddFieldNode(filterData, fieldInfo, data.ClassType, data.NodesListCtrl, PropertyInfoAssist.enDirection.Get, data.CSType, data.AttributeName, MethodInfoAssist.enHostType.Static);
                    }
                }
                else
                {
                    if(!readOnly)
                        AddFieldNode(filterData, fieldInfo, data.ClassType, data.NodesListCtrl, PropertyInfoAssist.enDirection.Set, data.CSType, data.AttributeName, data.HostType);
                    AddFieldNode(filterData, fieldInfo, data.ClassType, data.NodesListCtrl, PropertyInfoAssist.enDirection.Get, data.CSType, data.AttributeName, data.HostType);
                }
            }
            foreach(var proInfo in propertys)
            {
                bool readOnly = false;
                var att = EngineNS.Rtti.AttributeHelper.GetCustomAttribute(proInfo, data.AttributeName, true);
                if(att != null)
                {
                    var mt = (EngineNS.Editor.MacrossMemberAttribute.enMacrossType)EngineNS.Rtti.AttributeHelper.GetCustomAttributePropertyValue(att, "MacrossType");
                    readOnly = EngineNS.Editor.MacrossMemberAttribute.HasType(mt, EngineNS.Editor.MacrossMemberAttribute.enMacrossType.ReadOnly);
                }

                var setMethod = proInfo.GetSetMethod(false);
                if (setMethod != null && !readOnly)
                {
                    if (setMethod.IsStatic)
                    {
                        if(!data.IgnoreStatic)
                            AddPropertyNode(filterData, proInfo, data.ClassType, data.NodesListCtrl, PropertyInfoAssist.enDirection.Set, data.CSType, data.AttributeName, MethodInfoAssist.enHostType.Static);
                    }
                    else
                        AddPropertyNode(filterData, proInfo, data.ClassType, data.NodesListCtrl, PropertyInfoAssist.enDirection.Set, data.CSType, data.AttributeName, data.HostType);
                }
                var getMethod = proInfo.GetGetMethod(false);
                if (getMethod != null)
                {
                    if (getMethod.IsStatic)
                    {
                        if(!data.IgnoreStatic)
                            AddPropertyNode(filterData, proInfo, data.ClassType, data.NodesListCtrl, PropertyInfoAssist.enDirection.Get, data.CSType, data.AttributeName, MethodInfoAssist.enHostType.Static);
                    }
                    else
                        AddPropertyNode(filterData, proInfo, data.ClassType, data.NodesListCtrl, PropertyInfoAssist.enDirection.Get, data.CSType, data.AttributeName, data.HostType);
                }
            }
            foreach(var methodInfo in methods)
            {
                if(!methodInfo.IsPrivate)
                {
                    if (methodInfo.IsStatic)
                    {
                        if(!data.IgnoreStatic)
                            AddMethodNode(filterData, methodInfo, data.ClassType, data.NodesListCtrl, data.CSType, data.AttributeName, MethodInfoAssist.enHostType.Static);
                    }
                    else
                        AddMethodNode(filterData, methodInfo, data.ClassType, data.NodesListCtrl, data.CSType, data.AttributeName, data.HostType);
                }
            }
        }
        private static CodeGenerateSystem.Controls.NodeListAttributeClass GetNodeListAttClassRoot(CodeGenerateSystem.Controls.NodeListAttributeClass attClass)
        {
            if (attClass == null)
                return null;
            if (attClass.Parent == null)
                return attClass;
            return GetNodeListAttClassRoot(attClass.Parent);
        }
        class CTypeTree
        {
            public Type CurrentType = null;
            public string CurrentTypeFullName = "";
            public Dictionary<string, CTypeTree> ChildrenType = new Dictionary<string, CTypeTree>();
            public CTypeTree Parent = null;
        }
        private static void CheckToAddTree(CTypeTree parent, Type type)
        {
            var tree = new CTypeTree()
            {
                CurrentType = type,
                CurrentTypeFullName = type.FullName,
                Parent = parent,
            };

            List<string> removeTypeList = new List<string>();
            foreach (var child in parent.ChildrenType)
            {
                if (child.Value.CurrentType == null)
                    continue;

                if (type.IsSubclassOf(child.Value.CurrentType))
                {
                    CheckToAddTree(child.Value, type);
                    return;
                }
                else if (child.Value.CurrentType.IsSubclassOf(type))
                {
                    removeTypeList.Add(child.Key);
                    CheckToAddTree(tree, child.Value.CurrentType);
                }
            }

            if (removeTypeList.Count > 0)
            {
                foreach (var rt in removeTypeList)
                {
                    parent.ChildrenType.Remove(rt);
                }
            }
            parent.ChildrenType[type.FullName] = tree;
        }
        public class ClassKeyWord
        {
            public Type ClassType;
            public string KeyWord;
            public ClassKeyWord(Type type, string keyWord)
            {
                ClassType = type;
                KeyWord = keyWord;
            }
        }
        //public class UsefulMemberClass
        //{
        //    public CodeGenerateSystem.Base.UsefulMemberHostData Umhd = null;
        //    public List<PropertyInfo> SetPropertyInfos = new List<PropertyInfo>();
        //    public List<PropertyInfo> GetPropertyInfos = new List<PropertyInfo>();
        //    public List<MethodInfo> MethodInfos = new List<MethodInfo>();
        //}

        public static bool IsContainsKeyWords(List<ClassKeyWord> classKeyWords, Type type, string keyWord)
        {
            foreach (var item in classKeyWords)
            {
                if (item.ClassType.IsInterface)
                {
                    if (item.KeyWord == keyWord && type.GetInterface(item.ClassType.FullName) != null)
                        return true;
                }
                else
                {
                    if (item.KeyWord == keyWord && type.IsSubclassOf(item.ClassType))
                    {
                        return true;
                    }
                }
            }
            return false;
        }
    }
}
