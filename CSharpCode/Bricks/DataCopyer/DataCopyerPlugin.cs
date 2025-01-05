using EngineNS.Bricks.DataCopyer;
using EngineNS.IO;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.DataCopyer
{
    public abstract class TtDataCopyer : AssemblyLoader.IPlugin
    {
        public virtual void OnLoadedPlugin()
        {

        }
        public virtual void OnUnloadPlugin()
        {

        }
        public delegate void FCopy(object tar, object src);
        public class TtClassCopyer
        {
            public FCopy CurrentVersion;
            public Dictionary<UInt64, FCopy> VersionCopyers = new Dictionary<UInt64, FCopy>();
            public void RegVersion(UInt64 v, FCopy fn)
            {
                VersionCopyers[v] = fn;
            }
            public FCopy FindVersion(UInt64 hash)
            {
                if (hash == 0)
                {
                    return CurrentVersion;
                }
                if (VersionCopyers.TryGetValue(hash, out FCopy fn))
                    return fn;
                return null;
            }
        }
        public Dictionary<string, TtClassCopyer> ClassCopyer = new Dictionary<string, TtClassCopyer>();
        public TtClassCopyer GetClassCopyer(string typeStr)
        {
            TtClassCopyer result;
            if (ClassCopyer.TryGetValue(typeStr, out result))
                return result;
            result = new TtClassCopyer();
            ClassCopyer.Add(typeStr, result);
            return result;
        }
        public FCopy FindCopyer(string typeStr, UInt64 version = 0)
        {
            if (ClassCopyer.TryGetValue(typeStr, out var result))
            {
                return result.FindVersion(version);
            }
            return null;
        }
        public abstract Hash160 GetVersionHash();
        public Hash160 CalcVersionHash()
        {
            var metas = Rtti.TtClassMetaManager.Instance.Metas.Values.ToList();
            metas.Sort((x, y) =>
            {
                return x.ClassMetaName.CompareTo(y.ClassMetaName);
            });
            string hashStr = "";
            foreach (var met in metas)
            {
                hashStr += met.ClassMetaName;
                var vers = met.MetaVersions.Values.ToList();
                vers.Sort((x, y) =>
                {
                    return x.MetaHash.CompareTo(y.MetaHash);
                });
                foreach (var v in vers)
                {
                    hashStr += v;
                }
            }
            return Hash160.CreateHash160(hashStr);
        }
        public bool DataCopy(object target, object src, UInt64 version = 0)
        {
            if (target.GetType().IsSubclassOf(src.GetType()) == false && target.GetType() != src.GetType())
                return false;
            
            var fn = FindCopyer(Rtti.TtTypeDesc.TypeStr(src.GetType()), version);
            if (fn == null)
                return false;
            fn(target, src);
            return true;
        }
        public class TtCodeWriter : CodeBuilder.TtCodeCreator
        {
            public TtCodeWriter()
            {
                mSegmentStartStr = "{";
                mSegmentEndStr = "}";
                mIndentStr = "\t";
            }
        }
        
        public string GenCode(Hash160 hash)
        {
            string code = "";
            var creator = new TtCodeWriter();
            creator.AddLine("//Gen by engine", ref code);
            List<Rtti.TtClassMeta> metas = Rtti.TtClassMetaManager.Instance.Metas.Values.ToList();
            metas.Sort((x, y) =>
            {
                return x.ClassMetaName.CompareTo(y.ClassMetaName);
            });
            creator.AddLine($"namespace EngineNS.Plugins.DataCopyer", ref code);
            creator.PushSegment(ref code);
            {
                var klsCodes = new string[metas.Count];
                var numTask = TtEngine.Instance.EventPoster.NumOfPool;
                TtEngine.Instance.EventPoster.ParrallelFor(numTask, (i, arg1, arg2, state) =>
                {
                    int stride = (int)metas.Count / (int)state.UserArguments.NumOfParrallelFor + 1;
                    var start = i * stride;
                    for (int n = 0; n < stride; n++)
                    {
                        var nn = start + n;
                        if (nn >= metas.Count)
                            break;
                        var met = metas[nn];
                        if (met.ClassType.IsValueType)
                            continue;
                        string klsCode = "";
                        var klsCreator = new TtCodeWriter();
                        klsCreator.IntentCount = creator.IntentCount;
                        GenCode(met, klsCreator, ref klsCode);
                        klsCodes[nn] = klsCode;
                    }
                }, null);
                foreach(var c in klsCodes)
                {
                    if (c == null)
                        continue;
                    code += c;
                }
                //foreach (var met in metas)
                //{
                //    if (met.ClassType.IsValueType)
                //        continue;
                //    string klsCode = "";
                //    GenCode(met, creator, ref klsCode);
                //    code += klsCode;
                //}
            }
            creator.PopSegment(ref code);
            
            creator.AddLine($"namespace EngineNS.Plugins.DataCopyer", ref code);
            creator.PushSegment(ref code);
            {
                creator.AddLine($"partial class TtDataCopyerPlugin", ref code);
                creator.PushSegment(ref code);
                {
                    creator.AddLine($"internal TtDataCopyerPlugin()", ref code);
                    creator.PushSegment(ref code);
                    {
                        foreach (var met in metas)
                        {
                            if (met.ClassType.IsValueType)
                                continue;
                            string klsCode = "";
                            var klsCreator = new TtCodeWriter();
                            klsCreator.IntentCount = creator.IntentCount;

                            klsCreator.PushSegment(ref klsCode);
                            {
                                var name = met.ClassType.FullName.Replace('.', '_');
                                name = name.Replace('+', '_');
                                klsCreator.AddLine($"var kls = this.GetClassCopyer(\"{met.ClassMetaName}\");", ref klsCode);
                                klsCreator.AddLine($"kls.CurrentVersion = {name}.Copy_{met.CurrentVersion.MetaHash};", ref klsCode);
                                foreach (var v in met.MetaVersions)
                                {
                                    klsCreator.AddLine($"kls.RegVersion({v.Key}, {name}.Copy_{v.Key});", ref klsCode);
                                }
                            }
                            klsCreator.PopSegment(ref klsCode);
                            code += klsCode;
                        }

                        creator.AddLine($"this.VersionHash = EngineNS.Hash160.Parse(\"{hash.ToString()}\");", ref code);
                    }
                    creator.PopSegment(ref code);
                }
                creator.PopSegment(ref code);
            }
            creator.PopSegment(ref code);

            return code;
        }
        public void GenCode(Rtti.TtClassMeta meta, TtCodeWriter creator, ref string code)
        {
            var vers = meta.MetaVersions.Values.ToList();
            vers.Sort((x, y) =>
            {
                return x.MetaHash.CompareTo(y.MetaHash);
            });
            var name = meta.ClassType.FullName.Replace('.', '_');
            name = name.Replace('+', '_');
            creator.AddLine($"static class {name}", ref code);
            creator.PushSegment(ref code);
            {
                foreach (var i in vers)
                {
                    creator.AddLine($"internal static {typeof(FCopy).FullName.Replace('+', '.')} Copy_{i.MetaHash} = (object tar, object src)=>", ref code);
                    creator.PushSegment(ref code);
                    {
                        creator.AddLine($"var tarObj = tar as {meta.ClassType.FullName.Replace('+', '.')};", ref code);
                        creator.AddLine($"var srcObj = src as {meta.ClassType.FullName.Replace('+', '.')};", ref code);
                        foreach (var j in i.Propertys)
                        {
                            if (j.PropInfo != null && j.PropInfo.CanWrite && j.PropInfo.GetSetMethod() != null && j.PropInfo.GetSetMethod().IsPublic)
                            {
                                if (j.PropInfo.PropertyType.IsValueType ||
                                    j.PropInfo.PropertyType == typeof(string) || 
                                    j.PropInfo.PropertyType == typeof(RName) ||
                                    j.PropInfo.PropertyType == typeof(Rtti.TtTypeDesc))
                                {
                                    creator.AddLine($"tarObj.{j.PropertyName} = srcObj.{j.PropertyName};", ref code);
                                }
                                else
                                {
                                    if (j.PropInfo.PropertyType.IsClass &&
                                        !j.PropInfo.PropertyType.IsGenericType &&
                                        !j.PropInfo.PropertyType.IsArray &&
                                        j.PropInfo.PropertyType.GetInterface("IList") == null &&
                                        j.PropInfo.PropertyType.GetInterface("IDictionary") == null &&
                                        j.PropInfo.GetGetMethod() != null)
                                    {
                                        creator.AddLine($"if (srcObj.{j.PropertyName} != null)", ref code);
                                        creator.PushSegment(ref code);
                                        {
                                            if(j.PropInfo.GetSetMethod() != null)
                                            {
                                                if (j.PropInfo.PropertyType.GetConstructor(new Type[0]) != null)
                                                {
                                                    creator.AddLine($"if (tarObj.{j.PropertyName} == null || tarObj.{j.PropertyName}.GetType() != srcObj.{j.PropertyName}.GetType())", ref code);
                                                    creator.PushSegment(ref code);
                                                    {
                                                        creator.AddLine($"tarObj.{j.PropertyName} = EngineNS.Rtti.TtTypeDescManager.CreateInstance(srcObj.{j.PropertyName}.GetType()) as {j.PropInfo.PropertyType.FullName.Replace('+', '.')};", ref code);
                                                    }
                                                    creator.PopSegment(ref code);
                                                }
                                            }
                                                
                                            creator.AddLine($"if (tarObj.{j.PropertyName} != null)", ref code);
                                            creator.PushSegment(ref code);
                                            {
                                                creator.AddLine($"var fn = EngineNS.TtEngine.Instance.DataCopyer.FindCopyer(Rtti.TtTypeDescGetter<{j.PropInfo.PropertyType.FullName.Replace('+', '.')}>.TypeDesc.TypeString);", ref code);
                                                creator.AddLine($"if (fn != null)", ref code);
                                                creator.PushSegment(ref code);
                                                {
                                                    creator.AddLine($"fn(tarObj.{j.PropertyName}, srcObj.{j.PropertyName});", ref code);
                                                }
                                                creator.PopSegment(ref code);
                                            }
                                            creator.PopSegment(ref code);
                                        }
                                        creator.PopSegment(ref code);
                                        creator.AddLine($"else if (srcObj.{j.PropertyName} == null)", ref code);
                                        creator.PushSegment(ref code);
                                        {
                                            creator.AddLine($"tarObj.{j.PropertyName} = null;", ref code);
                                        }
                                        creator.PopSegment(ref code);
                                    }
                                    else if (j.PropInfo.PropertyType.IsGenericType && j.PropInfo.PropertyType.GetInterface("IList") != null)
                                    {
                                        creator.AddLine($"if (srcObj.{j.PropertyName} != null)", ref code);
                                        creator.PushSegment(ref code);
                                        {
                                            if (j.PropInfo.GetSetMethod() != null)
                                            {
                                                if (j.PropInfo.PropertyType.GetConstructor(new Type[0]) != null)
                                                {
                                                    creator.AddLine($"if (tarObj.{j.PropertyName} == null)", ref code);
                                                    creator.PushSegment(ref code);
                                                    {
                                                        creator.AddLine($"tarObj.{j.PropertyName} = new();", ref code);
                                                    }
                                                    creator.PopSegment(ref code);
                                                }
                                            }

                                            creator.AddLine($"if (tarObj.{j.PropertyName} != null)", ref code);
                                            creator.PushSegment(ref code);
                                            {
                                                var type = j.PropInfo.PropertyType.GetGenericArguments()[0];
                                                var typeName = type.FullName.Replace('+', '.');
                                                creator.AddLine($"var Tarlst = tarObj.{j.PropertyName} as System.Collections.Generic.List<{typeName}>;", ref code);
                                                creator.AddLine($"var Srclst = srcObj.{j.PropertyName} as System.Collections.Generic.List<{typeName}>;", ref code);
                                                bool isValue = type.IsValueType || type == typeof(string) || type == typeof(RName) || type == typeof(Rtti.TtTypeDesc);
                                                creator.AddLine($"Tarlst.Clear();", ref code);
                                                creator.AddLine($"for (int i = 0; i < Srclst.Count; i++)", ref code);
                                                creator.PushSegment(ref code);
                                                {
                                                    if (isValue)
                                                    {
                                                        creator.AddLine($"Tarlst.Add(Srclst[i]);", ref code);
                                                    }
                                                    else
                                                    {
                                                        creator.AddLine($"{typeName} tmp = Rtti.TtClassMeta.CloneProperty(Srclst[i]) as {typeName};", ref code);
                                                        creator.AddLine($"Tarlst.Add(tmp);", ref code);
                                                    }
                                                }
                                                creator.PopSegment(ref code);
                                            }
                                            creator.PopSegment(ref code);
                                        }
                                        creator.PopSegment(ref code);
                                    }
                                }
                            }
                        }
                    }
                    creator.PopSegment(ref code, true);
                }
            }
            creator.PopSegment(ref code);
        }
    }
}
namespace EngineNS
{
    partial class TtEngine
    {
        Bricks.DataCopyer.TtDataCopyer mDataCopyer;
        public Bricks.DataCopyer.TtDataCopyer DataCopyer
        {
            get
            {
                if (mDataCopyer == null)
                {
                    var serverPlugin = TtEngine.Instance.PluginModuleManager.GetPluginModule("DataCopyer");
                    if (serverPlugin != null)
                    {
                        mDataCopyer = serverPlugin.GetPluginObject<TtDataCopyer>();
                        var hash = mDataCopyer.CalcVersionHash();
                        if (hash != mDataCopyer.GetVersionHash())
                        {
                            var code = mDataCopyer.GenCode(hash);
                            var file = this.FileManager.GetRoot(IO.TtFileManager.ERootDir.PluginSource) + "DataCopyer/DataCopyer/Copyer.gen.cs";
                            TtFileManager.WriteAllText(file, code);

                            Profiler.Log.WriteLine<Profiler.TtIOCategory>(Profiler.ELogTag.Warning, $"Plugin DataCopyer need build");
                            //rebuild plugin
                        }
                    }
                }
                return mDataCopyer;
            }
        }
    }
}
