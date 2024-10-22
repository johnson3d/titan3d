using NPOI.HPSF;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace EngineNS.Editor.ShaderCompiler
{
    [Flags]
    public enum EShaderDefine : uint
    {
        HasGet = 1,
        HasSet = 1 << 1,
    }
    public class TtShaderDefineAttribute : Attribute
    {
        public EShaderDefine Flags = (EShaderDefine)0;
        public string ShaderName;
        public string Condition;
        public string Semantic;
        public string Binder;
        public int Order = 0;
    }

    public class TtShaderSourceCode : Graphics.Pipeline.Shader.IShaderCodeProvider
    {
        public void Cleanup()
        {
            DefineCode = null;
            SourceCode = null;
        }
        public void CollectIncludes()
        {
            var code = Support.UTextUtility.RemoveCStyleComments(SourceCode.TextCode);
            int cur = 0;
            cur = code.IndexOf("#", cur);
            while (cur >=0 && cur < code.Length)
            {
                cur++;
                var token = Support.UTextUtility.GetTokenString(ref cur, code);
                if (token == "include")
                {
                    Support.UTextUtility.SkipBlank(ref cur, code);
                    string inc = "";
                    if (code[cur] == '"')
                    {
                        var start = cur + 1;
                        Support.UTextUtility.SkipString(ref cur, code);
                        inc = code.Substring(start, cur - start);
                    }
                    else if (code[cur] == '<')
                    {
                        var start = cur + 1;
                        cur = code.IndexOf('>', start);
                        inc = code.Substring(start, cur - start);
                    }

                    var curPath = IO.TtFileManager.GetParentPathName(CodeName.Name);
                    string file;
                    if (!inc.StartsWith("@"))
                    {
                        file = IO.TtFileManager.CombinePath(curPath, inc);
                    }
                    else
                    {
                        file = inc;
                    }
                    
                    var rn = TtShaderCodeManager.Instance.GetShaderCode(RName.GetRName(file, CodeName.RNameType));
                    if (rn != null)
                    {
                        if (!DependencyCodes.Contains(rn))
                            DependencyCodes.Add(rn);
                    }
                    else
                    {
                        if (inc.Contains('.'))
                        {
                            //int xx = 0;
                        }
                    }
                }
                cur = code.IndexOf("#", cur);
            }

            DependencyCodes.Sort((x, y) =>
            {
                return x.CodeName.CompareTo(y.CodeName);
            });
        }
        public void GatherDependencyTree(List<TtShaderSourceCode> depends)
        {
            foreach (var i in DependencyCodes)
            {
                if (depends.Contains(i) == false)
                {
                    depends.Add(i);
                    i.GatherDependencyTree(depends);
                }
            }
        }
        public void UpdateCodeHash()
        {
            var depends = new List<TtShaderSourceCode>();
            GatherDependencyTree(depends);
            depends.Sort((x, y) =>
            {
                return x.CodeName.CompareTo(y.CodeName);
            });
            var fullCode = SourceCode.TextCode;
            foreach (var i in depends)
            {
                fullCode += i.SourceCode.TextCode;
            }
            CodeHash = Hash160.CreateHash160(fullCode);
        }
        public RName CodeName { get; set; }

/* 项目“Engine.Android”的未合并的更改
在此之前:
        public NxRHI.UShaderCode DefineCode { get; private set; } = new NxRHI.UShaderCode();
        public NxRHI.UShaderCode SourceCode { get; private set; } = new NxRHI.UShaderCode();
        public Hash160 CodeHash { get; private set; }
在此之后:
        public NxRHI.TtShaderCode DefineCode { get; private set; } = new NxRHI.UShaderCode();
        public NxRHI.TtShaderCode SourceCode { get; private set; } = new NxRHI.UShaderCode();
        public Hash160 CodeHash { get; private set; }
*/
        public NxRHI.TtShaderCode DefineCode { get; private set; } = new NxRHI.TtShaderCode();
        public NxRHI.TtShaderCode SourceCode { get; private set; } = new NxRHI.TtShaderCode();
        public Hash160 CodeHash { get; private set; }
        public List<TtShaderSourceCode> DependencyCodes { get; } = new List<TtShaderSourceCode>();
    }
    public class TtShaderCodeManager : IDisposable
    {
        public static TtShaderCodeManager Instance { get; } = new TtShaderCodeManager();
        public Dictionary<RName, TtShaderSourceCode> Codes { get; } = new Dictionary<RName, TtShaderSourceCode>();
        ~TtShaderCodeManager()
        {
            Dispose();
        }
        public void Initialize(RName shaderDir)
        {
            var files = IO.TtFileManager.GetFiles(shaderDir.Address, "*.*", true);
            string root;
            if(shaderDir.RNameType == RName.ERNameType.Engine)
                root = TtEngine.Instance.FileManager.GetRoot(IO.TtFileManager.ERootDir.Engine);
            else
                root = TtEngine.Instance.FileManager.GetRoot(IO.TtFileManager.ERootDir.Game);
            foreach (var i in files)
            {
                if (i.EndsWith(".vcxitems") || i.EndsWith(".filters") || i.EndsWith(".bak"))
                    continue;

                var path = IO.TtFileManager.GetRelativePath(root, i);
                var rn = RName.GetRName(path, shaderDir.RNameType);
                var code = LoadCode(rn);
                Codes.Add(rn, code);
            }

            try
            {
                var rn = RName.GetRName("@engine_preprosessors.cginc", RName.ERNameType.Engine);
                var code = GetEnginePreprocessors(rn, true);
                Codes.Add(rn, code);
            }
            catch (Exception ex)
            {
                Profiler.Log.WriteException(ex);
            }

            foreach (var i in Codes)
            {
                i.Value.CollectIncludes();
            }

            foreach (var i in Codes)
            {
                i.Value.UpdateCodeHash();
            }
        }
        public static string ToHLSLTypeString(System.Type type)
        {
            string memberType = "";
            if (type == typeof(Vector4) || type == typeof(Quaternion))
            {
                memberType = "float4";
            }
            else if (type == typeof(Vector3) || type == typeof(FRotator))
            {
                memberType = "float3";
            }
            else if (type == typeof(Vector2))
            {
                memberType = "float2";
            }
            else if (type == typeof(float))
            {
                memberType = "float";
            }
            else if (type == typeof(Vector4i))
            {
                memberType = "int4";
            }
            else if (type == typeof(Vector3i))
            {
                memberType = "int3";
            }
            else if (type == typeof(Vector2i))
            {
                memberType = "int2";
            }
            else if (type == typeof(int))
            {
                memberType = "int";
            }
            else if (type == typeof(Vector4ui))
            {
                memberType = "uint4";
            }
            else if (type == typeof(Vector3ui))
            {
                memberType = "uint3";
            }
            else if (type == typeof(Vector2ui))
            {
                memberType = "uint2";
            }
            else if (type == typeof(uint))
            {
                memberType = "uint";
            }
            else if (type == typeof(Color4f))
            {
                memberType = "float4";
            }
            else if (type == typeof(bool))
            {
                memberType = "bool";
            }
            else if (type == typeof(void))
            {
                memberType = "void";
            }
            else
            {
                var attr = type.GetCustomAttribute<TtShaderDefineAttribute>(false);
                if (attr != null)
                {
                    return attr.ShaderName;
                }
                System.Diagnostics.Debug.Assert(false);
            }
            return memberType;
        }
        private TtShaderSourceCode GetEnginePreprocessors(RName name, bool bWriteFile)
        {
            var result = new TtShaderSourceCode();
            var codeBuilder = new Bricks.CodeBuilder.UHLSLCodeGenerator();
            string sourceCode = "";

            var ShaderEnums = new List<KeyValuePair<TtShaderDefineAttribute, Rtti.TtTypeDesc>>();
            var ShaderStructs = new List<KeyValuePair<TtShaderDefineAttribute, Rtti.TtTypeDesc>>();
            Rtti.TtTypeDescManager.Instance.InterateTypes((Rtti.TtTypeDesc type) =>
            {
                var attrs = type.SystemType.GetCustomAttributes(typeof(TtShaderDefineAttribute), false);
                if (attrs.Length != 1)
                {
                    return;
                }
                var sdAttr = attrs[0] as TtShaderDefineAttribute;
                if (type.IsEnum)
                {
                    ShaderEnums.Add(KeyValuePair.Create(sdAttr, type));
                }
                else if (type.IsValueType)
                {
                    ShaderStructs.Add(KeyValuePair.Create(sdAttr, type));
                }
            });

            ShaderEnums.Sort((x, y) =>
            {
                return x.Key.ShaderName.CompareTo(y.Key.ShaderName);
            });
            ShaderStructs.Sort((x, y) =>
            {
                if (x.Key.Order.CompareTo(y.Key.Order) < 0)
                    return -1;
                else if (x.Key.Order.CompareTo(y.Key.Order) > 0)
                    return 1;
                else
                    return x.Key.ShaderName.CompareTo(y.Key.ShaderName);
            });
            codeBuilder.AddLine($"#ifndef ENGINE_PREPROCESSORTS_INC", ref sourceCode);
            codeBuilder.AddLine($"#define ENGINE_PREPROCESSORTS_INC", ref sourceCode);
            foreach (var kv in ShaderEnums)
            {
                codeBuilder.AddLine($"//Begin enum {kv.Key.ShaderName}", ref sourceCode);
                var values = Enum.GetValues(kv.Value.SystemType);
                var names = Enum.GetNames(kv.Value.SystemType);
                for (int i = 0; i < names.Length; i++)
                {
                    object r;
                    Enum.TryParse(kv.Value.SystemType, values.GetValue(i).ToString(), out r);
                    codeBuilder.AddLine($"#define {kv.Key.ShaderName}_{names[i]} {(uint)r}", ref sourceCode);
                }
                codeBuilder.AddLine($"//End enum {kv.Key.ShaderName}", ref sourceCode);
                codeBuilder.AddLine("\n", ref sourceCode);
            }

            foreach (var kv in ShaderStructs)
            {
                codeBuilder.AddLine($"struct {kv.Key.ShaderName}", ref sourceCode);
                codeBuilder.PushSegment(ref sourceCode);
                {
                    var members = kv.Value.SystemType.GetFields();
                    foreach (var i in members)
                    {
                        var typeStr = ToHLSLTypeString(i.FieldType);
                        var attr = i.GetCustomAttribute<TtShaderDefineAttribute>(false);
                        if (attr != null && attr.ShaderName != null)
                        {
                            string prefix = "";
                            string suffix = "";
                            if (attr.Binder != null)
                            {
                                prefix = attr.Binder + " ";
                            }
                            if (attr.Semantic != null)
                            {
                                suffix = " : " + attr.Semantic;
                            }
                            if (attr.Condition != null)
                            {
                                codeBuilder.AddLine($"#if {attr.Condition}", ref sourceCode);
                            }
                            codeBuilder.AddLine($"{prefix}{typeStr} {attr.ShaderName}{suffix};", ref sourceCode);
                            if (attr.Condition != null)
                            {
                                codeBuilder.AddLine($"#endif//{attr.Condition}", ref sourceCode);
                            }
                            if ((attr.Flags & EShaderDefine.HasGet) != 0)
                            {
                                codeBuilder.AddLine($"{typeStr} Get_{attr.ShaderName}()", ref sourceCode);
                                codeBuilder.PushSegment(ref sourceCode);
                                {
                                    if (attr.Condition != null)
                                    {
                                        codeBuilder.AddLine($"#if {attr.Condition}", ref sourceCode);
                                    }

                                    codeBuilder.AddLine($"return {attr.ShaderName};", ref sourceCode);

                                    if (attr.Condition != null)
                                    {
                                        codeBuilder.AddLine($"#else", ref sourceCode);
                                        codeBuilder.AddLine($"return ({typeStr})0;", ref sourceCode);
                                        codeBuilder.AddLine($"#endif//{attr.Condition}", ref sourceCode);
                                    }
                                }
                                codeBuilder.PopSegment(ref sourceCode);
                            }
                            if ((attr.Flags & EShaderDefine.HasSet) != 0)
                            {
                                codeBuilder.AddLine($"void Set_{attr.ShaderName}({typeStr} v)", ref sourceCode);
                                codeBuilder.PushSegment(ref sourceCode);
                                {
                                    if (attr.Condition != null)
                                    {
                                        codeBuilder.AddLine($"#if {attr.Condition}", ref sourceCode);
                                    }

                                    codeBuilder.AddLine($"{attr.ShaderName} = v;", ref sourceCode);

                                    if (attr.Condition != null)
                                    {
                                        codeBuilder.AddLine($"#endif//{attr.Condition}", ref sourceCode);
                                    }
                                }
                                codeBuilder.PopSegment(ref sourceCode);
                            }
                        }
                        else
                        {
                            System.Diagnostics.Debug.Assert(i.Name[0] == 'm');
                            codeBuilder.AddLine($"{typeStr} {i.Name.Substring(1)};", ref sourceCode);
                        }
                    }

                    codeBuilder.AddLine($"// Method declaration", ref sourceCode);
                    var methods = kv.Value.SystemType.GetMethods();
                    foreach (var i in methods)
                    {
                        var attr = i.GetCustomAttribute<TtShaderDefineAttribute>(false);
                        if (attr == null)
                            continue;
                        var retType = ToHLSLTypeString(i.ReturnType);
                        var pmter = i.GetParameters();
                        string callArg = "";
                        foreach (var j in pmter)
                        {
                            var argType = ToHLSLTypeString(j.ParameterType);
                            callArg += $"{argType} {j.Name},";
                        }
                        if (pmter.Length > 0)
                        {
                            callArg = callArg.Substring(0, callArg.Length - 1);
                        }
                        codeBuilder.AddLine($"{retType} {attr.ShaderName}({callArg});", ref sourceCode);
                    }
                }
                codeBuilder.PopSegment(ref sourceCode, true);
            }

            Graphics.Pipeline.Shader.VS_MODIFIER.VSInput_2_VSModifier(codeBuilder, ref sourceCode);
            Graphics.Pipeline.Shader.PS_INPUT.VSModifier_2_PSInput(codeBuilder, ref sourceCode);

            codeBuilder.AddLine($"#endif//define ENGINE_PREPROCESSORTS_INC", ref sourceCode);

            result.SourceCode.TextCode = sourceCode;
            result.CodeName = name;

            if (bWriteFile)
            {
                var file = TtEngine.Instance.FileManager.GetPath(IO.TtFileManager.ERootDir.Cache, IO.TtFileManager.ESystemDir.DebugUtility) + $"/engine_preprosessors.cginc";
                var code = IO.TtFileManager.ReadAllText(file);
                if (code != sourceCode)
                {
                    IO.TtFileManager.WriteAllText(file, sourceCode);
                }
            }
            return result;
        }
        public void Dispose()
        {
            foreach (var i in Codes)
            {
                i.Value.Cleanup();
            }
            Codes.Clear();
        }
        public Graphics.Pipeline.Shader.IShaderCodeProvider GetShaderCodeProvider(RName name)
        {
            if (name.ExtName == Graphics.Pipeline.Shader.TtMaterial.AssetExt)
            {
                var saveMode = Thread.Async.TtContextThreadManager.ImmidiateMode;
                Thread.Async.TtContextThreadManager.ImmidiateMode = true;
                var task = TtEngine.Instance.GfxDevice.MaterialManager.GetMaterial(name);
                task.Wait();
                Thread.Async.TtContextThreadManager.ImmidiateMode = saveMode;
                var material = task.Result;
                if (material != null)
                    return material;
                return null;
            }
            else
            {
                return GetShaderCode(name);
            }
        }
        public TtShaderSourceCode GetShaderCode(RName name)
        {
            TtShaderSourceCode result;
            if (Codes.TryGetValue(name, out result))
                return result;

            result = LoadCode(name);
            if (result != null)
            {
                Codes.Add(name, result);
                return result;
            }
            return null;
        }
        protected TtShaderSourceCode LoadCode(RName name)
        {
            if (IO.TtFileManager.FileExists(name.Address) == false)
                return null;

            var code = IO.TtFileManager.ReadAllText(name.Address);
            var result = new TtShaderSourceCode();
            result.SourceCode.TextCode = code;
            result.CodeName = name;
            return result;
        }
    }
}
