using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectCooker.Command
{
    class USerializerCode : EngineNS.Bricks.CodeBuilder.UCodeWriter
    {

    }
    class UBuildSerializer : UCookCommand
    {
        public static bool IsValueType(System.Type type)
        {
            return (type.IsValueType && type.IsEnum == false) || type == typeof(string);
            //return type.IsValueType || type == typeof(EngineNS.RName) || type == typeof(string);
        }
        public override async System.Threading.Tasks.Task ExecuteCommand(string[] args)
        {
            await EngineNS.Thread.AsyncDummyClass.DummyFunc();
            var path = FindArgument(args, "Serializer_Path=");

            var metas = EngineNS.Rtti.UClassMetaManager.Instance.Metas;
            foreach (var i in metas)
            {
                USerializerCode codeWriter = new USerializerCode();
                codeWriter.AddLine($"#if UseSerializerCodeGen");
                codeWriter.AddLine($"using System;");

                codeWriter.AddLine($"namespace {i.Value.ClassType.Namespace}");
                codeWriter.PushBrackets();
                {
                    var fixedName = i.Value.ClassType.Name;
                    if (i.Value.ClassType.FullName.Contains('+'))
                    {
                        fixedName = i.Value.ClassType.FullName.Substring(i.Value.ClassType.Namespace.Length);
                        if (fixedName.StartsWith('.'))
                            fixedName = fixedName.Substring(1);
                        fixedName = fixedName.Replace("+", "_CIC_");
                    }
                    codeWriter.AddLine($"public class {fixedName}_Serializer");
                    codeWriter.PushBrackets();
                    {
                        foreach (var j in i.Value.MetaVersions)
                        {
                            codeWriter.AddLine($"public static EngineNS.IO.SerializerHelper.Delegate_ReadMetaVersion mfn_Read_{j.Key} = Read_{j.Key};"); 
                            codeWriter.AddLine($"public static void Read_{j.Key}(EngineNS.IO.IReader ar, EngineNS.IO.ISerializer InHostObject)");
                            codeWriter.PushBrackets();
                            {
                                codeWriter.AddLine($"var hostObject = ({i.Value.ClassType.FullName.Replace('+', '.')})InHostObject;");
                                foreach (var k in j.Value.Propertys)
                                {
                                    if (k.FieldType.SystemType.GetInterface("ISerializer") != null)
                                    {
                                        codeWriter.AddLine($"EngineNS.IO.ISerializer tmp_{k.PropertyName};");
                                        codeWriter.AddLine($"ar.Read(out tmp_{k.PropertyName}, hostObject);");
                                        if (k.PropInfo != null && k.PropInfo.CanWrite && k.PropInfo.SetMethod.IsPublic)
                                        {
                                            codeWriter.AddLine($"hostObject.{k.PropertyName} = ({k.PropInfo.PropertyType.FullName.Replace('+', '.')})tmp_{k.PropertyName};");
                                        }
                                        else
                                        {
                                            codeWriter.PushBrackets();
                                            {
                                                codeWriter.AddLine($"var typeDef = EngineNS.Rtti.UTypeDesc.TypeOf(typeof({k.PropInfo.PropertyType.FullName.Replace('+','.')}));");                                           
                                                codeWriter.AddLine($"var meta = EngineNS.Rtti.UClassMetaManager.Instance.GetMeta(typeDef);");
                                                codeWriter.AddLine($"meta?.CopyObjectMetaField(hostObject.{k.PropertyName}, tmp_{k.PropertyName});");
                                            }
                                            codeWriter.PopBrackets();
                                        }
                                        continue;
                                    }
                                    else if (k.FieldType.SystemType.GetInterface(nameof(System.Collections.IList)) != null)
                                    {
                                        var innerType = k.FieldType.SystemType.GenericTypeArguments[0];
                                        var lstType = $"System.Collections.Generic.List<{innerType.FullName.Replace('+', '.')}>";
                                        if (k.PropInfo.CanWrite)
                                        {
                                            codeWriter.AddLine($"var tmp_{k.PropertyName} = new {lstType}();");
                                        }
                                        else
                                        {
                                            codeWriter.AddLine($"var tmp_{k.PropertyName} = hostObject.{k.PropertyName};");
                                            codeWriter.AddLine($"if(tmp_{k.PropertyName} == null)");
                                            codeWriter.PushBrackets();
                                            {
                                                codeWriter.AddLine($"tmp_{k.PropertyName} = new {lstType}();");
                                            }
                                            codeWriter.PopBrackets();
                                        }
                                        codeWriter.PushBrackets();
                                        {   
                                            codeWriter.AddLine($"bool isValueType;");
                                            codeWriter.AddLine($"ar.Read(out isValueType);");
                                            codeWriter.AddLine($"int count = 0;");
                                            codeWriter.AddLine($"ar.Read(out count);");
                                            //codeWriter.AddLine($"if (isValueType)");
                                            //codeWriter.PushBrackets();
                                            if (IsValueType(innerType))
                                            {
                                                codeWriter.AddLine($"string elemTypeStr;");
                                                codeWriter.AddLine($"ar.Read(out elemTypeStr);");
                                                codeWriter.AddLine($"var skipPoint = EngineNS.IO.SerializerHelper.GetSkipOffset(ar);");
                                                codeWriter.AddLine($"{innerType.FullName} elem_lst_{k.PropertyName};");
                                                codeWriter.AddLine($"for (int j = 0; j < count; j++)");
                                                codeWriter.PushBrackets();
                                                {
                                                    codeWriter.AddLine($"ar.Read(out elem_lst_{k.PropertyName});");
                                                    codeWriter.AddLine($"tmp_{k.PropertyName}.Add(elem_lst_{k.PropertyName});");
                                                }
                                                codeWriter.PopBrackets();
                                            }
                                            //codeWriter.PopBrackets();
                                            //codeWriter.AddLine($"else");
                                            //codeWriter.PushBrackets();
                                            else
                                            {
                                                codeWriter.AddLine($"for (int j = 0; j < count; j++)");
                                                codeWriter.PushBrackets();
                                                {
                                                    codeWriter.AddLine($"string elemTypeStr;");
                                                    codeWriter.AddLine($"ar.Read(out elemTypeStr);");
                                                    codeWriter.AddLine($"var skipPoint = EngineNS.IO.SerializerHelper.GetSkipOffset(ar);");
                                                    codeWriter.AddLine($"try");
                                                    codeWriter.PushBrackets();
                                                    {
                                                        codeWriter.AddLine($"var elemType = Rtti.UTypeDesc.TypeOf(elemTypeStr).SystemType;");
                                                        codeWriter.AddLine($"var e = ({innerType.FullName.Replace('+','.')})EngineNS.IO.SerializerHelper.ReadObject(ar, elemType, hostObject);");
                                                        codeWriter.AddLine($"tmp_{k.PropertyName}.Add(e);");
                                                    }
                                                    codeWriter.PopBrackets();
                                                    codeWriter.AddLine($"catch (Exception ex)");
                                                    codeWriter.PushBrackets();
                                                    {
                                                        codeWriter.AddLine($"Profiler.Log.WriteException(ex);");
                                                        codeWriter.AddLine($"ar.Seek(skipPoint);");
                                                    }
                                                    codeWriter.PopBrackets();
                                                }
                                                codeWriter.PopBrackets();
                                            }
                                            //codeWriter.PopBrackets();
                                        }
                                        codeWriter.PopBrackets();
                                    }
                                    else if (k.FieldType.SystemType.GetInterface(nameof(System.Collections.IDictionary)) != null)
                                    {
                                        var keyType = k.FieldType.SystemType.GenericTypeArguments[0];
                                        var valueType = k.FieldType.SystemType.GenericTypeArguments[1];
                                        var dictType = $"System.Collections.Generic.Dictionary<{keyType.FullName.Replace('+', '.')}, {valueType.FullName.Replace('+', '.')}>";
                                        if (k.PropInfo.CanWrite)
                                        {
                                            codeWriter.AddLine($"var tmp_{k.PropertyName} = new {dictType}();");
                                        }
                                        else
                                        {
                                            codeWriter.AddLine($"var tmp_{k.PropertyName} = hostObject.{k.PropertyName};");
                                            codeWriter.AddLine($"if(tmp_{k.PropertyName} == null)");
                                            codeWriter.PushBrackets();
                                            {
                                                codeWriter.AddLine($"tmp_{k.PropertyName} = new {dictType}();");
                                            }
                                            codeWriter.PopBrackets();
                                        }
                                        codeWriter.PushBrackets();
                                        {   
                                            codeWriter.AddLine($"bool isKeyValueType;");
                                            codeWriter.AddLine($"bool isValueValueType;");
                                            codeWriter.AddLine($"ar.Read(out isKeyValueType);");
                                            codeWriter.AddLine($"ar.Read(out isValueValueType);");
                                            codeWriter.AddLine($"int count;");
                                            codeWriter.AddLine($"ar.Read(out count);");
                                            //codeWriter.AddLine($"if (isKeyValueType && isValueValueType)");
                                            //codeWriter.PushBrackets();
                                            if (IsValueType(keyType) && IsValueType(valueType))
                                            {
                                                codeWriter.AddLine($"string elemKeyTypeStr;");
                                                codeWriter.AddLine($"ar.Read(out elemKeyTypeStr);");
                                                codeWriter.AddLine($"string elemValueTypeStr;");
                                                codeWriter.AddLine($"ar.Read(out elemValueTypeStr);");
                                                codeWriter.AddLine($"var skipPoint = EngineNS.IO.SerializerHelper.GetSkipOffset(ar);");

                                                codeWriter.AddLine($"{keyType.FullName} elem_key_{k.PropertyName};");
                                                codeWriter.AddLine($"{valueType.FullName} elem_value_{k.PropertyName};");
                                                codeWriter.AddLine($"for (int j = 0; j < count; j++)");
                                                codeWriter.PushBrackets();
                                                {
                                                    codeWriter.AddLine($"ar.Read(out elem_key_{k.PropertyName});");
                                                    codeWriter.AddLine($"ar.Read(out elem_value_{k.PropertyName});");
                                                    codeWriter.AddLine($"tmp_{k.PropertyName}[elem_key_{k.PropertyName}] = elem_value_{k.PropertyName};");
                                                }
                                                codeWriter.PopBrackets();
                                            }
                                            //codeWriter.PopBrackets();
                                            //codeWriter.AddLine($"else if (isKeyValueType && !isValueValueType)");
                                            //codeWriter.PushBrackets();
                                            else if(IsValueType(keyType) && !IsValueType(valueType))
                                            {
                                                codeWriter.AddLine($"string elemKeyTypeStr;");
                                                codeWriter.AddLine($"ar.Read(out elemKeyTypeStr);");
                                                codeWriter.AddLine($"var skipPoint = EngineNS.IO.SerializerHelper.GetSkipOffset(ar);");

                                                codeWriter.AddLine($"{keyType.FullName} elem_key_{k.PropertyName};");
                                                codeWriter.AddLine($"for (int j = 0; j < count; j++)");
                                                codeWriter.PushBrackets();
                                                {
                                                    codeWriter.AddLine($"string elemValueTypeStr;");
                                                    codeWriter.AddLine($"ar.Read(out elemValueTypeStr);");
                                                    codeWriter.AddLine($"var skipPoint1 = EngineNS.IO.SerializerHelper.GetSkipOffset(ar);");
                                                    codeWriter.AddLine($"var elemValueType = Rtti.UTypeDesc.TypeOf(elemValueTypeStr).SystemType;"); 

                                                    codeWriter.AddLine($"ar.Read(out elem_key_{k.PropertyName});");
                                                    codeWriter.AddLine($"var value = ({valueType.FullName.Replace('+', '.')})EngineNS.IO.SerializerHelper.ReadObject(ar, elemValueType, hostObject);");
                                                    codeWriter.AddLine($"tmp_{k.PropertyName}[elem_key_{k.PropertyName}] = value;");
                                                }
                                                codeWriter.PopBrackets();
                                            }
                                            //codeWriter.PopBrackets();
                                            //codeWriter.AddLine($"else if (!isKeyValueType && isValueValueType)");
                                            //codeWriter.PushBrackets();
                                            else if (!IsValueType(keyType) && IsValueType(valueType))
                                            {
                                                codeWriter.AddLine($"string elemValueTypeStr;");
                                                codeWriter.AddLine($"ar.Read(out elemValueTypeStr);");
                                                codeWriter.AddLine($"var skipPoint = EngineNS.IO.SerializerHelper.GetSkipOffset(ar);");

                                                codeWriter.AddLine($"{valueType.FullName} elem_value_{k.PropertyName};");
                                                codeWriter.AddLine($"for (int j = 0; j < count; j++)");
                                                codeWriter.PushBrackets();
                                                {
                                                    codeWriter.AddLine($"string elemKeyTypeStr;");
                                                    codeWriter.AddLine($"ar.Read(out elemKeyTypeStr);");
                                                    codeWriter.AddLine($"var skipPoint1 = EngineNS.IO.SerializerHelper.GetSkipOffset(ar);");
                                                    codeWriter.AddLine($"var elemKeyType = Rtti.UTypeDesc.TypeOf(elemKeyTypeStr).SystemType;");

                                                    codeWriter.AddLine($"var key = ({keyType.FullName.Replace('+', '.')})EngineNS.IO.SerializerHelper.ReadObject(ar, elemKeyType, hostObject);");
                                                    codeWriter.AddLine($"ar.Read(out elem_value_{k.PropertyName});");                                                    
                                                    codeWriter.AddLine($"tmp_{k.PropertyName}[key] = elem_value_{k.PropertyName};");
                                                }
                                                codeWriter.PopBrackets();
                                            }
                                            //codeWriter.PopBrackets();
                                            //codeWriter.AddLine($"else if (!isKeyValueType && !isValueValueType)");
                                            //codeWriter.PushBrackets();
                                            else if (!IsValueType(keyType) && !IsValueType(valueType))
                                            {
                                                codeWriter.AddLine($"for (int j = 0; j < count; j++)");
                                                codeWriter.PushBrackets();
                                                {
                                                    codeWriter.AddLine($"string elemKeyTypeStr;");
                                                    codeWriter.AddLine($"ar.Read(out elemKeyTypeStr);");
                                                    codeWriter.AddLine($"var elemKeyType = Rtti.UTypeDesc.TypeOf(elemKeyTypeStr).SystemType;");

                                                    codeWriter.AddLine($"string elemValueTypeStr;");
                                                    codeWriter.AddLine($"ar.Read(out elemValueTypeStr);");                                                    
                                                    codeWriter.AddLine($"var elemValueType = Rtti.UTypeDesc.TypeOf(elemValueTypeStr).SystemType;");
                                                    codeWriter.AddLine($"var skipPoint1 = EngineNS.IO.SerializerHelper.GetSkipOffset(ar);");

                                                    codeWriter.AddLine($"var key = ({keyType.FullName.Replace('+', '.')})EngineNS.IO.SerializerHelper.ReadObject(ar, elemKeyType, hostObject);");
                                                    codeWriter.AddLine($"var value = ({valueType.FullName.Replace('+', '.')})EngineNS.IO.SerializerHelper.ReadObject(ar, elemValueType, hostObject);");
                                                    codeWriter.AddLine($"tmp_{k.PropertyName}[key] = value;");
                                                }
                                                codeWriter.PopBrackets();
                                            }
                                            //codeWriter.PopBrackets();
                                        }
                                        codeWriter.PopBrackets();
                                    }
                                    else if (IsValueType(k.FieldType.SystemType) || k.FieldType.SystemType == typeof(string))
                                    {
                                        codeWriter.AddLine($"{k.FieldType.FullName.Replace('+', '.')} tmp_{k.PropertyName};");
                                        codeWriter.AddLine($"ar.Read(out tmp_{k.PropertyName});");
                                    }
                                    else
                                    {
                                        codeWriter.AddLine($"{k.FieldType.FullName.Replace('+', '.')} tmp_{k.PropertyName};");
                                        codeWriter.AddLine($"tmp_{k.PropertyName} = ({k.FieldType.FullName.Replace('+', '.')})EngineNS.IO.SerializerHelper.ReadObject(ar, typeof({k.FieldType.FullName.Replace('+', '.')}), hostObject);");
                                    }
                                    
                                    if (k.PropInfo != null && k.PropInfo.CanWrite && k.PropInfo.SetMethod.IsPublic)
                                    {
                                        codeWriter.AddLine($"hostObject.{k.PropertyName} = tmp_{k.PropertyName};");
                                    }
                                    else
                                    {
                                        codeWriter.AddLine($"//hostObject.{k.PropertyName} = tmp_{k.PropertyName};");
                                    }
                                }
                            }
                            codeWriter.PopBrackets();
                        }
                    }
                    codeWriter.PopBrackets();
                }
                codeWriter.PopBrackets();

                codeWriter.AddLine($"#endif");

                var file = EngineNS.IO.FileManager.CombinePath(path, i.Value.ClassType.FullName) + ".reader.cs";
                file = EngineNS.IO.FileManager.GetRegularPath(file);
                WritedFiles.Add(file);
                System.IO.File.WriteAllText(file, codeWriter.ClassCode);
            }
            MakeSharedProjectCSharp(path, "EngineSerializer.projitems");
        }
        List<string> WritedFiles = new List<string>();
        public void MakeSharedProjectCSharp(string genDir, string fileName)
        {
            System.Xml.XmlDocument myXmlDoc = new System.Xml.XmlDocument();
            myXmlDoc.Load(EngineNS.IO.FileManager.CombinePath(genDir, "Empty_CodeGenCSharp.projitems"));
            var root = myXmlDoc.LastChild;
            var compile = myXmlDoc.CreateElement("ItemGroup", root.NamespaceURI);
            root.AppendChild(compile);
            var allFiles = System.IO.Directory.GetFiles(genDir, "*.cs", System.IO.SearchOption.AllDirectories);
            foreach (var i in allFiles)
            {
                var file = EngineNS.IO.FileManager.GetRegularPath(i);
                if (!WritedFiles.Contains(file))
                {
                    System.IO.File.Delete(i);
                }
            }
            allFiles = System.IO.Directory.GetFiles(genDir, "*.cs", System.IO.SearchOption.AllDirectories);
            foreach (var i in allFiles)
            {
                var cs = myXmlDoc.CreateElement("Compile", root.NamespaceURI);
                var file = myXmlDoc.CreateAttribute("Include");
                file.Value = "$(MSBuildThisFileDirectory)" + GetRelativePath(i, genDir);
                cs.Attributes.Append(file);
                compile.AppendChild(cs);
            }

            var streamXml = new System.IO.MemoryStream();
            var writer = new System.Xml.XmlTextWriter(streamXml, Encoding.UTF8);
            writer.Formatting = System.Xml.Formatting.Indented;
            myXmlDoc.Save(writer);
            var reader = new System.IO.StreamReader(streamXml, Encoding.UTF8);
            streamXml.Position = 0;
            var content = reader.ReadToEnd();
            reader.Close();
            streamXml.Close();

            var projFile = EngineNS.IO.FileManager.CombinePath(genDir, fileName);
            if (System.IO.File.Exists(projFile))
            {
                string old_code = System.IO.File.ReadAllText(projFile);
                if (content == old_code)
                    return;
            }
            System.IO.File.WriteAllText(projFile, content);
        }

        static string NormalizePath(string path, out bool error)
        {
            error = false;

            path = path.Replace("\\", "/");

            path = path.Replace("../", "$/");

            path = path.Replace("./", "");

            //path = path.ToLower();

            int UpDirLength = "$/".Length;
            int startPos = path.LastIndexOf("$/");
            while (startPos >= 0)
            {
                int rmvNum = 1;
                var head = path.Substring(0, startPos);
                var tail = path.Substring(startPos + UpDirLength);
                while (head.Length > UpDirLength && head.EndsWith("$/"))
                {
                    rmvNum++;
                    head = head.Substring(0, head.Length - "$/".Length);
                }
                if (head.EndsWith('/'))
                    head = head.Substring(0, head.Length - 1);
                int discardPos = -1;
                for (int i = 0; i < rmvNum; i++)
                {
                    discardPos = head.LastIndexOf("/");
                    if (discardPos < 0)
                    {
                        error = true;
                        return null;
                    }
                    else
                    {
                        head = head.Substring(0, discardPos);
                    }
                }
                path = head + '/' + tail;

                startPos = path.LastIndexOf("$/");
            }

            return path;
        }
        static string GetRelativePath(string path, string parent)
        {
            bool error;
            path = NormalizePath(path, out error);
            parent = NormalizePath(parent, out error);
            if (path.StartsWith(parent))
            {
                return path.Substring(parent.Length);
            }
            else
            {
                return path;
            }
        }
    }
}
