using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace EngineNS.Rtti
{
    // 类型重定向，用于类型更改后的版本兼容
    public class TypeRedirectionHelper
    {
        // string是TypeSavingString
        static Dictionary<string, string> mTypeRedirection = new Dictionary<string, string>();

        public static void AddRedirection(string oldType, string newType)
        {
            mTypeRedirection[oldType] = newType;
        }
        public static string GetRedirectedType(string typeStr)
        {
            string retStr;
            if (mTypeRedirection.TryGetValue(typeStr, out retStr))
                return retStr;
            return typeStr;
        }
        public static void Save(string absFileName)
        {
            // test only ///////////////////////////////////////////
            //mTypeRedirection["Client|Macross@CodeDomNode.VariableType"] = "Client|Macross@CodeDomNode.VariableType";
            ////////////////////////////////////////////////////////

            var xmlHolder = EngineNS.IO.XmlHolder.NewXMLHolder("TypeRedirection", "");
            var node = xmlHolder.RootNode.AddNode("TypeRDData", "", xmlHolder);
            foreach(var tr in mTypeRedirection)
            {
                var dataNode = node.AddNode("data", "", xmlHolder);
                dataNode.AddAttrib("Key", tr.Key);
                dataNode.AddAttrib("Value", tr.Value);
            }
            EngineNS.IO.XmlHolder.SaveXML(absFileName, xmlHolder);
        }
        public static void Load(string absFileName)
        {
            using (var xmlHolder = EngineNS.IO.XmlHolder.LoadXML(absFileName))
            {
                if (xmlHolder == null)
                {
                    Profiler.Log.WriteLine(Profiler.ELogTag.Error, "MetaData", $"TypeRedirection is null:{absFileName}");
                    return;
                }
                var node = xmlHolder.RootNode.FindNode("TypeRDData");
                if (node != null)
                {
                    var childNodes = node.GetNodes();
                    for (int i = 0; i < childNodes.Count; i++)
                    {
                        var keyAtt = childNodes[i].FindAttrib("Key");
                        if (keyAtt == null)
                            continue;
                        var valueAtt = childNodes[i].FindAttrib("Value");
                        if (valueAtt == null)
                            continue;
                        mTypeRedirection[keyAtt.Value] = valueAtt.Value;
                    }
                }
            }
        }
    }

    public class RttiHelper
    {
        [Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        [return: Editor.Editor_TypeChangeWithParam(0)]
        public static object CreateInstance(System.Type type)
        {
            return System.Activator.CreateInstance(type);
        }
        public static void Lock()
        {
            System.Threading.Monitor.Enter(mAnalyseAssemblyDic);
        }
        public static void UnLock()
        {
            System.Threading.Monitor.Exit(mAnalyseAssemblyDic);
        }
        #region Assemebly
        //class AssemblyPlatformData
        //{
        //    public EPlatformType Platform = EPlatformType.PLATFORM_WIN;

        //    Dictionary<string, VAssembly> mAssemblyPlatformMapping = new Dictionary<string, VAssembly>();
        //    public Dictionary<string, VAssembly> AssemblyPlatformMapping
        //    {
        //        get { return mAssemblyPlatformMapping; }
        //    }
        //}
        //static Dictionary<ECSType, Dictionary<EPlatformType, AssemblyPlatformData>> mAnalyseAssemblyDic = new Dictionary<ECSType, Dictionary<EPlatformType, AssemblyPlatformData>>();
        static Dictionary<ECSType, Dictionary<string, VAssembly>> mAnalyseAssemblyDic = new Dictionary<ECSType, Dictionary<string, VAssembly>>();
        static Dictionary<string, VAssembly> mAnalyseAssemblyFullNameDic = new Dictionary<string, VAssembly>();
        public static VAssembly GetAnalyseAssemblyByFullName(string fullName)
        {
            VAssembly assembly;
            if (mAnalyseAssemblyFullNameDic.TryGetValue(fullName, out assembly))
                return assembly;
            return null;
        }
        public static bool RegisterAnalyseAssembly(string keyName, VAssembly assembly)
        {
            if (assembly == null)
                return false;
            assembly.KeyName = keyName;
            Dictionary<string, VAssembly> dic;
            if(!mAnalyseAssemblyDic.TryGetValue(assembly.CSType, out dic))
            {
                dic = new Dictionary<string, VAssembly>();
                mAnalyseAssemblyDic.Add(assembly.CSType, dic);
            }
            dic.Add(keyName, assembly);
            mAnalyseAssemblyFullNameDic[assembly.FullName] = assembly;

            return true;
        }
        static void RemoveAssemblyTypes(VAssembly assembly)
        {
            if (assembly == null)
                return;
            Dictionary<EngineNS.EPlatformType, Dictionary<string, Type>> platFormDic;
            if (!mTypeNameDic.TryGetValue(assembly.CSType, out platFormDic))
                return;
            Dictionary<string, Type> typeDic;
            if (!platFormDic.TryGetValue(CIPlatform.Instance.PlatformType, out typeDic))
                return;
            foreach(var type in assembly.GetTypes())
            {
                typeDic.Remove(type.FullName);
            }
        }
        public static bool UnRegisterAnalyseAssembly(EngineNS.ECSType csType, string keyName)
        {
            if (string.IsNullOrEmpty(keyName))
                return false;
            mTypeNameDic.Clear();
            if (csType == ECSType.All)
            {
                foreach(var dic in mAnalyseAssemblyDic.Values)
                {
                    VAssembly assembly;
                    if(dic.TryGetValue(keyName, out assembly))
                    {
                        mAnalyseAssemblyFullNameDic.Remove(assembly.FullName);
                        RemoveAssemblyTypes(assembly);
                        dic.Remove(keyName);
                    }
                }
            }
            else
            {
                Dictionary<string, VAssembly> dic;
                if (mAnalyseAssemblyDic.TryGetValue(csType, out dic))
                {
                    VAssembly assembly;
                    if(dic.TryGetValue(keyName, out assembly))
                    {
                        mAnalyseAssemblyFullNameDic.Remove(assembly.FullName);
                        RemoveAssemblyTypes(assembly);
                        dic.Remove(keyName);
                    }
                }

            }
            return true;
        }
        public static VAssembly[] GetAnalyseAssemblys(ECSType csType)
        {
            List<VAssembly> retAssemblys = new List<VAssembly>();
            if (csType == ECSType.All || csType == ECSType.Common)
            {
                foreach (var asmDic in mAnalyseAssemblyDic.Values)
                {
                    retAssemblys.AddRange(asmDic.Values);
                }
            }
            else
            {
                Dictionary<string, VAssembly> dic;
                if (mAnalyseAssemblyDic.TryGetValue(csType, out dic))
                {
                    retAssemblys.AddRange(dic.Values);
                }
            }

            return retAssemblys.ToArray();
        }
        public static Type[] GetTypes()
        {
            List<Type> retList = new List<Type>();
            foreach (var asmDic in mAnalyseAssemblyDic.Values)
            {
                foreach (var assembly in asmDic.Values)
                {
                    retList.AddRange(assembly.GetTypes());
                }
            }

            return retList.ToArray();
        }
        public static Type[] GetTypes(ECSType csType)
        {
            List<Type> retList = new List<Type>();
            if (csType == ECSType.All || csType == ECSType.Common)
            {
                foreach (var assemDic in mAnalyseAssemblyDic.Values)
                {
                    foreach (var assembly in assemDic.Values)
                    {
                        retList.AddRange(assembly.GetTypes());
                    }
                }
            }
            else
            {
                Dictionary<string, VAssembly> assemDic;
                if (mAnalyseAssemblyDic.TryGetValue(csType, out assemDic))
                {
                    foreach (var assembly in assemDic.Values)
                    {
                        retList.AddRange(assembly.GetTypes());
                    }
                }
            }

            return retList.ToArray();
        }
        public static Type[] GetTypes(ECSType csType, string attributeTypeFullName, bool inherit)
        {
            List<Type> retList = new List<Type>();
            if (csType == ECSType.All || csType == ECSType.Common)
            {
                foreach (var assemDic in mAnalyseAssemblyDic.Values)
                {
                    foreach (var assembly in assemDic.Values)
                    {
                        foreach (var type in assembly.GetTypes())
                        {
                            var att = AttributeHelper.GetCustomAttribute(type, attributeTypeFullName, inherit);
                            if (att == null)
                                continue;

                            retList.Add(type);
                        }
                    }
                }
            }
            else
            {
                Dictionary<string, VAssembly> assemDic;
                if (mAnalyseAssemblyDic.TryGetValue(csType, out assemDic))
                {
                    foreach (var assembly in assemDic.Values)
                    {
                        foreach (var type in assembly.GetTypes())
                        {
                            var att = AttributeHelper.GetCustomAttribute(type, attributeTypeFullName, inherit);
                            if (att == null)
                                continue;

                            retList.Add(type);
                        }
                    }
                }
            }
            return retList.ToArray();
        }
        public static Type[] GetTypes(string attributeTypeFullName, bool inherit)
        {
            return GetTypes(ECSType.All, attributeTypeFullName, inherit);
        }
        public static ECSType GetAnalyseAssemblyCSType(System.Reflection.Assembly assembly)
        {
            foreach(var asmDic in mAnalyseAssemblyDic)
            {
                foreach(var asm in asmDic.Value)
                {
                    if (asm.Value.Assembly == assembly)
                        return asmDic.Key;
                }
            }
            return ECSType.Common;
        }
        public static VAssembly GetAnalyseAssembly(ECSType csType, string keyName)
        {
            lock (mAnalyseAssemblyDic)
            {
                if (csType == ECSType.All || csType == ECSType.Common)
                {
                    foreach (var asmDic in mAnalyseAssemblyDic.Values)
                    {
                        VAssembly assembly;
                        if (asmDic.TryGetValue(keyName, out assembly))
                        {
                            return assembly;
                        }
                    }
                }
                else
                {
                    Dictionary<string, VAssembly> asmDic;
                    if (mAnalyseAssemblyDic.TryGetValue(csType, out asmDic))
                    {
                        VAssembly assembly;
                        if (asmDic.TryGetValue(keyName, out assembly))
                            return assembly;
                    }
                }
            }
            return null;
        }

        static Dictionary<string, VAssembly> mRuntimeAssemblyCache = new Dictionary<string, VAssembly>();
        public static void ClearRuntimAssemblyCache()
        {
            mRuntimeAssemblyCache.Clear();
        }
        public static VAssembly GetAssemblyFromDllFileName(ECSType csType, string dllName, string relativePath = "", bool fromBytes = false, bool userCashe = true, bool useVPDB = true)
        {
            try
            {
                string dllFullname = dllName;
                dllName = EngineNS.CEngine.Instance.FileManager.GetPureFileFromFullName(dllName, false);
                var idx = dllFullname.LastIndexOf(".dll");
                if (idx < 0)
                    dllFullname = dllFullname + ".dll";
                if (userCashe)
                {
                    VAssembly retVal;
                    if (mRuntimeAssemblyCache.TryGetValue(dllName, out retVal))
                        return retVal;

                    var assem = GetAnalyseAssembly(csType, dllName);
                    if (assem != null)
                    {
                        mRuntimeAssemblyCache[assem.KeyName] = assem;
                        return assem;
                    }
                }


                //if (csType == ECSType.Server)
                //{
                //    System.Diagnostics.Debug.Assert(!dllFullname.Contains("Client.dll"));
                //    System.Diagnostics.Debug.Assert(!dllFullname.Contains("ClientCommon.dll"));
                //}
                //else if (csType == ECSType.Client)
                //{
                //    System.Diagnostics.Debug.Assert(!dllFullname.Contains("Server.dll"));
                //    System.Diagnostics.Debug.Assert(!dllFullname.Contains("Server.Windows.dll"));
                //}

                if (userCashe)
                {
                    var assemblys = AppDomain.CurrentDomain.GetAssemblies();
                    foreach (var assembly in assemblys)
                    {
                        if (assembly.GetName().Name + ".dll" == dllFullname)
                        {
                            var ra = VAssemblyManager.Instance.GetAssembly(assembly, csType, dllName);
                            mRuntimeAssemblyCache[ra.KeyName] = ra;
                            return ra;
                        }
                    }
                }

                if (!System.IO.Path.IsPathRooted(dllFullname))
                {
                    var pathString = "";
                    switch (csType)
                    {
                        case ECSType.Client:
                            pathString = CEngine.Instance.Desc.Client_Directory + "/";
                            dllFullname = CEngine.Instance.FileManager.EngineRoot + pathString + relativePath + dllFullname;
                            break;
                        case ECSType.Server:
                            pathString = CEngine.Instance.Desc.Server_Directory + "/";
                            dllFullname = CEngine.Instance.FileManager.EngineRoot + pathString + relativePath + dllFullname;
                            break;
                        case ECSType.Common:
                        case ECSType.All:
                            dllFullname = CEngine.Instance.FileManager.Bin + relativePath + dllFullname;
                            break;
                    }
                }

                var retAssembly = VAssemblyManager.Instance.LoadAssembly(dllFullname, csType, fromBytes, userCashe, dllName, useVPDB);
                if(retAssembly != null)
                {
                    mRuntimeAssemblyCache[retAssembly.KeyName] = retAssembly;
                }
                return retAssembly;
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.ToString());
            }

            return null;
        }
        #endregion
        //public static string GetAppTypeStringFromSaveString(string saveString)
        //{
        //    var type = GetTypeFromSaveString(saveString);
        //    return GetAppTypeString(type);
        //}
        public static string GetAppTypeString(Type type)
        {
            if (type == null)
                return "";

            if (type.IsGenericType)
            {
                string retValue = type.Namespace + "." + type.Name;
                var agTypes = type.GetGenericArguments();
                if (agTypes.Length == 0)
                    return retValue;

                retValue = retValue.Replace("`" + type.GetGenericArguments().Length, "");
                var agStr = "";
                for (int i = 0; i < agTypes.Length; i++)
                {
                    if (i == 0)
                        agStr = GetAppTypeString(agTypes[i]);
                    else
                        agStr += "," + GetAppTypeString(agTypes[i]);
                }
                retValue += "<" + agStr + ">";
                return retValue;
            }
            else if (type.IsGenericParameter)
                return type.Name;
            else
                return type.FullName.Replace("+", ".");
        }
        public static string GetTypeMetaHashString(Type type)
        {
            if (type == null)
                return "";
            if (type.IsGenericParameter)
            {
                if (type.DeclaringMethod != null && type.DeclaringMethod.IsGenericMethod && type.DeclaringMethod.IsGenericMethodDefinition)
                {
                    var classType = type.DeclaringMethod.DeclaringType;
                    var classTypeSaveString = GetTypeMetaHashString(classType);
                    return "genericMethodParameter@" + classTypeSaveString + ":" + type.DeclaringMethod.Name + ":" + type.Name;
                }
                else
                    throw new InvalidOperationException();
            }
            else
            {
                var assembly = VAssemblyManager.Instance.GetAssemblyFromType(type);
                var keyName = assembly.KeyName;
                if (string.IsNullOrEmpty(keyName))
                    keyName = type.Assembly.GetName().Name;// + ".dll@";
                else
                    keyName += "@";
                keyName = assembly.CSType.ToString() + "|" + keyName;
                if (type.IsGenericType)
                {
                    string retValue = keyName + type.Namespace + "." + type.Name;
                    var agTypes = type.GetGenericArguments();
                    if (agTypes.Length == 0)
                        return retValue;

                    var agTypeStr = "";
                    for (int i = 0; i < agTypes.Length; i++)
                    {
                        agTypeStr += "[" + GetTypeMetaHashString(agTypes[i]) + "]";
                    }

                    retValue += "[" + agTypeStr + "]";
                    return retValue;
                }
                else
                    return keyName + type.FullName;
            }
        }
        static Dictionary<Type, string> mTypeSaveStrDic = new Dictionary<Type, string>();
        public static string GetTypeSaveString(Type type)
        {
            if (type == null)
                return "";
            string retSaveStr;
            lock (mTypeSaveStrDic)
            {
                if (mTypeSaveStrDic.TryGetValue(type, out retSaveStr))
                    return retSaveStr;
                if (type.IsGenericParameter)
                {
                    if (type.DeclaringMethod != null && type.DeclaringMethod.IsGenericMethod && type.DeclaringMethod.IsGenericMethodDefinition)
                    {
                        var classType = type.DeclaringMethod.DeclaringType;
                        var classTypeSaveString = GetTypeSaveString(classType);
                        retSaveStr = "genericMethodParameter@" + classTypeSaveString + ":" + type.DeclaringMethod.Name + ":" + type.Name;
                        mTypeSaveStrDic.Add(type, retSaveStr);
                        return retSaveStr;
                    }
                    ////else if(type.DeclaringType != null && type.DeclaringType.IsGenericType && type.DeclaringType.IsGenericTypeDefinition)
                    ////{
                    ////    var classType = type.DeclaringType;
                    ////    var classTypeSaveString = GetTypeSaveString(classType);
                    ////    retSaveStr = "genericClassParameter@" + classTypeSaveString + ":" + type.Name;
                    ////    mTypeSaveStrDic.Add(type, retSaveStr);
                    ////    return retSaveStr;
                    ////}
                    else
                        throw new InvalidOperationException();
                }
                else
                {
                    var assembly = VAssemblyManager.Instance.GetAssemblyFromType(type);
                    var keyName = assembly.KeyName;
                    if(type.GetCustomAttributes(typeof(EngineNS.Macross.MacrossTypeClassAttribute), true).Length > 0)
                    {
                        // Macross类型
                        keyName = "MacrossScript@";
                    }
                    else
                    {
                        if (string.IsNullOrEmpty(keyName))
                            keyName = type.Assembly.GetName().Name;// + ".dll@";
                        else
                            keyName += "@";
                    }
                    keyName = assembly.CSType.ToString() + "|" + keyName;
                    if (type.IsGenericType)
                    {
                        string retValue = keyName + type.Namespace + "." + type.Name;
                        var agTypes = type.GetGenericArguments();
                        if (agTypes.Length == 0)
                        {
                            mTypeSaveStrDic.Add(type, retValue);
                            return retValue;
                        }

                        var agTypeStr = "";
                        for (int i = 0; i < agTypes.Length; i++)
                        {
                            agTypeStr += "[" + GetTypeSaveString(agTypes[i]) + "]";
                        }

                        retValue += "[" + agTypeStr + "]";
                        mTypeSaveStrDic.Add(type, retValue);
                        return retValue;
                    }
                    else
                    {
                        retSaveStr = keyName + type.FullName;
                        mTypeSaveStrDic.Add(type, retSaveStr);
                        return retSaveStr;
                    }
                }
            }
        }
        public static VAssembly GetTypeAssemblyFromSaveString(string str)
        {
            var idx = str.IndexOf('@');
            if (idx == -1)
                return null;
            var strDllKey = str.Substring(0, idx);
            var csIdx = strDllKey.IndexOf('|');
            if(csIdx < 0)
            {
                var assembly = GetAnalyseAssembly(ECSType.All, strDllKey);
                if (assembly != null)
                    return assembly;
                var assemblys = AppDomain.CurrentDomain.GetAssemblies();
                var dllName = strDllKey.Substring(csIdx + 1);
                if (dllName.LastIndexOf(".dll") < 0)
                    dllName += ".dll";
                foreach (var asm in assemblys)
                {
                    if (asm.GetName().Name + ".dll" == dllName)
                        return VAssemblyManager.Instance.GetAssembly(asm, ECSType.Common);
                }
            }
            else
            {
                var csStr = strDllKey.Substring(0, csIdx);
                var csType = (EngineNS.ECSType)EngineNS.Rtti.RttiHelper.EnumTryParse(typeof(EngineNS.ECSType), csStr);
                var dllName = strDllKey.Substring(csIdx + 1);
                if(dllName == "MacrossScript")
                {
                    return EngineNS.CEngine.Instance.MacrossDataManager.MacrossScripAssembly;
                }
                else
                    return GetAssemblyFromDllFileName(csType, dllName);
            }
            return null;
        }
        public static bool IsSubclassOf(System.Type type, string baseName)
        {
            foreach (var i in type.GetInterfaces())
            {
                if (i.FullName == baseName)
                    return true;
            }
            if (type.FullName == baseName)
                return true;
            else if (type.BaseType == null)
                return false;
            else
            {
                return IsSubclassOf(type.BaseType, baseName);
            }
        }
        private static int GetPairCharPosition(int startIdx, string str, char startChar, char endChar)
        {
            if (string.IsNullOrEmpty(str))
                return -1;

            if (startIdx < 0 || startIdx >= str.Length)
                return -1;
            if (str[startIdx] != startChar)
                return -1;

            int startCount = 0;
            for (int i = startIdx + 1; i < str.Length; i++)
            {
                if (str[i] == startChar)
                    startCount++;
                if (str[i] == endChar)
                {
                    if (startCount == 0)
                        return i;

                    startCount--;
                }
            }

            return -1;
        }
        //public static string GetTypeAppStringFromSaveString(string str, ECSType csType = ECSType.All, bool withAssembly = true)
        //{
        //    if (str.Contains("["))
        //    {
        //        var idxStart = str.IndexOf('[');
        //        var idxEnd = str.LastIndexOf(']');
        //        var typeStr = str.Substring(0, idxStart);
        //        if (typeStr.IndexOf('`') < 0)
        //        {
        //            // array
        //            var splits = str.Split('@');
        //            return splits[1];
        //        }
        //        else
        //        {
        //            // generic
        //            var argCount = System.Convert.ToInt32(typeStr.Substring(typeStr.IndexOf('`') + 1));
        //            typeStr = GetTypeAppStringFromSaveString(typeStr, csType, withAssembly);
        //            var argsStr = str.Substring(idxStart + 1, idxEnd - (idxStart + 1));
        //            int startIdx = 0;
        //            string argsAppStr = "";
        //            for (int i = 0; i < argCount; i++)
        //            {
        //                var endidx = GetPairCharPosition(startIdx, argsStr, '[', ']');
        //                if (endidx < 0)
        //                    return "";

        //                var subString = argsStr.Substring(startIdx + 1, endidx - (startIdx + 1));
        //                var dllKeyStr = subString.Substring(0, subString.IndexOf('@'));
        //                switch (dllKeyStr)
        //                {
        //                    case "genericMethodParameter":
        //                        {
        //                            var idx = subString.IndexOf('@') + 1;
        //                            var valueStr = subString.Substring(idx, subString.Length - idx);
        //                            if (i == 0)
        //                                argsAppStr += "";// valueStr;
        //                            else
        //                                argsAppStr += ",";// + valueStr;
        //                        }
        //                        break;
        //                    default:
        //                        {
        //                            var csIdx = dllKeyStr.IndexOf('|');
        //                            var assemStr = dllKeyStr;
        //                            if(csIdx >= 0)
        //                            {
        //                                var csStr = dllKeyStr.Substring(0, csIdx);
        //                                csType = (EngineNS.ECSType)EngineNS.Rtti.RttiHelper.EnumTryParse(typeof(EngineNS.ECSType), csStr);
        //                                assemStr = dllKeyStr.Substring(csIdx + 1);
        //                            }
        //                            var assem = GetAssemblyFromDllFileName(csType, assemStr);
        //                            if (assem == null)
        //                                return "";
        //                            if (i == 0)
        //                            {
        //                                if (withAssembly)
        //                                    argsAppStr += "[" + GetTypeAppStringFromSaveString(subString, csType, withAssembly) + ", " + assem.FullName + "]";
        //                                else
        //                                    argsAppStr += GetTypeAppStringFromSaveString(subString, csType, withAssembly);
        //                            }
        //                            else
        //                            {
        //                                if (withAssembly)
        //                                    argsAppStr += ",[" + GetTypeAppStringFromSaveString(subString, csType, withAssembly) + ", " + assem.FullName + "]";
        //                                else
        //                                    argsAppStr += "," + GetTypeAppStringFromSaveString(subString, csType, withAssembly);
        //                            }
        //                        }
        //                        break;
        //                }

        //                startIdx = endidx + 1;
        //            }

        //            typeStr += "[" + argsAppStr + "]";
        //        }

        //        return typeStr;
        //    }
        //    else
        //    {
        //        var splits = str.Split('@');
        //        return splits[1];
        //    }
        //}
        static Dictionary<string, Type> mTypeTable = new Dictionary<string, Type>();
        public static void ClearTypeTable()
        {
            mTypeTable.Clear();
        }
        public static Type GetTypeFromSaveString(string str)
        {
            bool isRedirection;
            return GetTypeFromSaveString(str, out isRedirection);
        }
        public static bool CacheCleanHistory = false;
        public static Dictionary<string, string> CanCleanHistoryMetaDatas = new Dictionary<string, string>();
        public static Type GetTypeFromSaveString(string str, out bool isRedirection)
        {
            if(CacheCleanHistory)
                CanCleanHistoryMetaDatas[str] = str;
            // 临时代码，将CoreClient替换为EngineCore
            //if (str.Contains("CoreClient@"))
            //{
            //    var newStr = str.Replace("CoreClient@", "EngineCore@");
            //    EngineNS.Rtti.TypeRedirectionHelper.AddRedirection(str, newStr);
            //    EngineNS.Rtti.TypeRedirectionHelper.Save(EngineNS.CEngine.Instance.FileManager.Content + "typeredirection.xml");
            //    var retType = GetTypeFromSaveString(newStr, out isRedirection);
            //    isRedirection = true;
            //    return retType;
            //}
            //else if(str.Contains("enginecore"))
            //{
            //    throw new InvalidOperationException();
            //}

            isRedirection = false;
            lock (mTypeTable)
            {
                if (str == null || str == "")
                    return null;
                Type outType = null;
                if (mTypeTable.TryGetValue(str, out outType))
                    return outType;

                var idx = str.IndexOf('@');
                if (idx == -1)
                    return null;
                var strDllKey = str.Substring(0, idx);
                switch (strDllKey)
                {
                    case "genericMethodParameter":
                        {
                            var typeDeclarer = str.Substring(idx + 1);
                            var splits = typeDeclarer.Split(':');
                            var classType = GetTypeFromSaveString(splits[0], out isRedirection);
                            var methods = classType.GetMethods();
                            for(int i=0; i<methods.Length; i++)
                            {
                                if(methods[i].Name == splits[1])
                                {
                                    if(methods[i].IsGenericMethod && methods[i].IsGenericMethodDefinition)
                                    {
                                        var args = methods[i].GetGenericArguments();
                                        for(int argIdx = 0; argIdx < args.Length; argIdx++)
                                        {
                                            if (args[argIdx].Name == splits[2])
                                                return args[argIdx];
                                        }
                                    }
                                }
                            }
                        }
                        break;
                    default:
                        {
                            var assem = GetTypeAssemblyFromSaveString(str);
                            if (assem == null)
                            {
                                isRedirection = true;
                                var rdType = TypeRedirectionHelper.GetRedirectedType(str);
                                if (rdType != str)
                                {
                                    return GetTypeFromSaveString(rdType);
                                }
                                return null;
                            }
                            else
                            {
                                if(str.Contains("["))
                                {
                                    // 泛型
                                    var idxStart = str.IndexOf('[');
                                    var idxEnd = str.LastIndexOf(']');
                                    var typeStr = str.Substring(0, idxStart);
                                    if(typeStr.IndexOf('`') < 0)
                                    {
                                        // array
                                        var splits = str.Split('@');
                                        outType = assem.GetType(splits[1]);
                                        if (outType != null)
                                            mTypeTable.Add(str, outType);
                                        else
                                        {
                                            isRedirection = true;
                                            var rdType = TypeRedirectionHelper.GetRedirectedType(str);
                                            if (rdType != str)
                                                return GetTypeFromSaveString(rdType);
                                        }
                                        return outType;
                                    }
                                    else
                                    {
                                        // generic
                                        var argCount = System.Convert.ToInt32(typeStr.Substring(typeStr.IndexOf('`') + 1));
                                        var type = GetTypeFromSaveString(typeStr);
                                        var argsStr = str.Substring(idxStart + 1, idxEnd - (idxStart + 1));
                                        int startIdx = 0;
                                        Type[] typeArgs = new Type[argCount];
                                        for(int i=0; i<argCount; i++)
                                        {
                                            var endIdx = GetPairCharPosition(startIdx, argsStr, '[', ']');
                                            if(endIdx < 0)
                                            {
                                                throw new InvalidOperationException();
                                            }

                                            var subString = argsStr.Substring(startIdx + 1, endIdx - (startIdx + 1));
                                            typeArgs[i] = GetTypeFromSaveString(subString);
                                            if (typeArgs[i] == null)
                                                throw new InvalidOperationException();
                                            startIdx = endIdx + 1;
                                        }
                                        outType = type.MakeGenericType(typeArgs);
                                        if (outType != null)
                                            mTypeTable.Add(str, outType);
                                        else
                                            throw new InvalidOperationException();
                                        return outType;
                                        //typeStr = Get
                                    }
                                }
                                else
                                {
                                    var splits = str.Split('@');
                                    outType = assem.GetType(splits[1]);
                                    if (outType != null)
                                    {
                                        mTypeTable.Add(str, outType);
                                    }
                                    else
                                    {
                                        isRedirection = true;
                                        var rdType = TypeRedirectionHelper.GetRedirectedType(str);
                                        if (rdType != str)
                                        {
                                            return GetTypeFromSaveString(rdType);
                                        }
                                    }
                                    return outType;
                                }
                            }
                        }
                        //break;
                }
                return null;
            }
        }

        // 类型名称对应类型字典表
        static Dictionary<EngineNS.ECSType, Dictionary<EngineNS.EPlatformType, Dictionary<string, Type>>> mTypeNameDic = new Dictionary<EngineNS.ECSType, Dictionary<EngineNS.EPlatformType, Dictionary<string, Type>>>();
        /// <summary>
        /// 从类型全名称获取类型（不包含Assembly内容）
        /// </summary>
        /// <param name="typeFullName">类型的全名称</param>
        /// <returns>根据名称取得的类型，没有找到则返回null</returns>
        public static Type GetTypeFromTypeFullName(string typeFullName, EngineNS.ECSType csType = EngineNS.ECSType.All)
        //public static Type GetTypeFromTypeFullName(string typeFullName, EngineNS.ECSType csType)
        {
            if (string.IsNullOrEmpty(typeFullName))
                return null;
            Dictionary<EngineNS.EPlatformType, Dictionary<string, Type>> platFormDic;
            if (!mTypeNameDic.TryGetValue(csType, out platFormDic))
            {
                platFormDic = new Dictionary<EngineNS.EPlatformType, Dictionary<string, Type>>();
                mTypeNameDic[csType] = platFormDic;
            }

            Dictionary<string, Type> typeDic;
            if (!platFormDic.TryGetValue(CIPlatform.Instance.PlatformType, out typeDic))
            {
                typeDic = new Dictionary<string, Type>();
                platFormDic[CIPlatform.Instance.PlatformType] = typeDic;
            }

            if (typeDic.ContainsKey(typeFullName))
                return typeDic[typeFullName];

            if (csType == EngineNS.ECSType.All)
            {
                foreach (var assemDic in mAnalyseAssemblyDic.Values)
                {
                    foreach (var assembly in assemDic.Values)
                    {
                        foreach (var type in assembly.GetTypes())
                        {
                            if (type.FullName.Equals(typeFullName))
                            {
                                typeDic[typeFullName] = type;
                                return type;
                            }
                        }
                    }
                }
            }
            else
            {
                Dictionary<string, VAssembly> assemDic;
                if (mAnalyseAssemblyDic.TryGetValue(csType, out assemDic))
                {
                    foreach (var assembly in assemDic.Values)
                    {
                        foreach (var type in assembly.GetTypes())
                        {
                            if (type.FullName.Equals(typeFullName))
                            {
                                typeDic[typeFullName] = type;
                                return type;
                            }
                        }
                    }
                }
            }

            var cmType = Type.GetType(typeFullName);
            if (cmType != null)
            {
                typeDic[typeFullName] = cmType;
                return cmType;
            }

            return null;
        }

        static public Guid GuidParse(string str)
        {
            if (!str.Contains("-"))
                return Guid.Empty;

            return new Guid(str);
        }
        static public Guid GuidTryParse(string str)
        {
            try
            {
                if (!str.Contains("-"))
                    return Guid.Empty;

                return new Guid(str);
            }
            catch (System.Exception)
            {
                //Log.FileLog.WriteLine(string.Format("Parse Guid Failed:{0}", str));
                return Guid.Empty;
            }
        }
        static public T EnumTryParse<T>(string str)
        {
            try
            {
                return (T)(System.Enum.Parse(typeof(T), str));
            }
            catch (System.Exception)
            {
                return default(T);
            }
        }

        static public object EnumTryParse(System.Type type, string str)
        {
            try
            {
                return System.Enum.Parse(type, str);
            }
            catch (System.Exception)
            {
                return null;
            }
        }

        // 类继承关系字典
        static Dictionary<EngineNS.ECSType, Dictionary<EngineNS.EPlatformType, Dictionary<string, List<Type>>>> mTypeInheritDic = new Dictionary<ECSType, Dictionary<EPlatformType, Dictionary<string, List<Type>>>>();
        /// <summary>
        /// 获取类的继承类列表
        /// </summary>
        /// <param name="parentTypeString">父类类型全名称，不包含Assembly信息</param>
        /// <returns></returns>
        public static List<Type> GetInheritTypesFromType(string parentTypeString, EngineNS.ECSType csType)
        {
            if (string.IsNullOrEmpty(parentTypeString))
                return new List<Type>();

            Dictionary<EngineNS.EPlatformType, Dictionary<string, List<Type>>> platFormDic;
            if (!mTypeInheritDic.TryGetValue(csType, out platFormDic))
            {
                platFormDic = new Dictionary<EngineNS.EPlatformType, Dictionary<string, List<Type>>>();
                mTypeInheritDic[csType] = platFormDic;
            }

            Dictionary<string, List<Type>> typeDic;
            if (!platFormDic.TryGetValue(CIPlatform.Instance.PlatformType, out typeDic))
            {
                typeDic = new Dictionary<string, List<Type>>();
                platFormDic[CIPlatform.Instance.PlatformType] = typeDic;
            }

            if (typeDic.ContainsKey(parentTypeString))
                return typeDic[parentTypeString];

            var parentType = GetTypeFromTypeFullName(parentTypeString, csType);
            if (parentType == null)
                return new List<Type>();

            List<Type> retValue = new List<Type>();
            if (csType == EngineNS.ECSType.All)
            {
                foreach (var assemDic in mAnalyseAssemblyDic.Values)
                {
                    foreach (var assembly in assemDic.Values)
                    {
                        if (parentType.IsInterface)
                        {
                            foreach (var type in assembly.GetTypes())
                            {
                                if (type.GetInterface(parentType.FullName) != null)
                                    retValue.Add(type);
                            }
                        }
                        else
                        {
                            foreach (var type in assembly.GetTypes())
                            {
                                if (type.IsSubclassOf(parentType))
                                    retValue.Add(type);
                            }
                        }
                    }
                }
            }
            else
            {
                Dictionary<string, VAssembly> assemDic;
                if (mAnalyseAssemblyDic.TryGetValue(csType, out assemDic))
                {
                    foreach (var assembly in assemDic.Values)
                    {
                        if (parentType.IsInterface)
                        {
                            foreach (var type in assembly.GetTypes())
                            {
                                if (type.GetInterface(parentType.FullName) != null)
                                    retValue.Add(type);
                            }
                        }
                        else
                        {
                            foreach (var type in assembly.GetTypes())
                            {
                                if (type.IsSubclassOf(parentType))
                                    retValue.Add(type);
                            }
                        }
                    }
                }
            }

            typeDic[parentTypeString] = retValue;
            return retValue;
        }

        public static RName GetRNameFromType(Type type, bool checkFileExist, RName.enRNameType rNameType = RName.enRNameType.Game)
        {
            var fullName = GetAppTypeString(type);
            var retVal = RName.GetRName(fullName.Replace(".", "/") + EngineNS.CEngineDesc.MacrossExtension, rNameType);
            if(checkFileExist)
            {
                if (!EngineNS.CEngine.Instance.FileManager.DirectoryExists(retVal.Address))
                    return null;
            }
            return retVal;
        }

        public static string ReplaceWholeWord(string original, string wordToFind, string replacement, RegexOptions regexOptions = RegexOptions.None)
        {
            string pattern = string.Format(@"\b{0}\b", wordToFind);
            string ret = Regex.Replace(original, pattern, replacement, regexOptions);
            return ret;
        }
    }
}
