using NPOI.HPSF;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Editor.ShaderCompiler
{
    public class TtShaderDefineAttribute : Attribute
    {
        public string ShaderName;
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
                    var file = IO.TtFileManager.CombinePath(curPath, inc);
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
        public NxRHI.UShaderCode DefineCode { get; private set; } = new NxRHI.UShaderCode();
        public NxRHI.UShaderCode SourceCode { get; private set; } = new NxRHI.UShaderCode();
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
                root = UEngine.Instance.FileManager.GetRoot(IO.TtFileManager.ERootDir.Engine);
            else
                root = UEngine.Instance.FileManager.GetRoot(IO.TtFileManager.ERootDir.Game);
            foreach (var i in files)
            {
                if (i.EndsWith(".vcxitems") || i.EndsWith(".filters") || i.EndsWith(".bak"))
                    continue;

                var path = IO.TtFileManager.GetRelativePath(root, i);
                var rn = RName.GetRName(path, shaderDir.RNameType);
                var code = LoadCode(rn);
                Codes.Add(rn, code);
            }

            {
                var rn = RName.GetRName("@engine_preprosessors.cginc", RName.ERNameType.Engine);
                var code = GetEnginePreprocessors(rn, true);
                Codes.Add(rn, code);
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
            if (type == typeof(Vector4))
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
            else
            {
                System.Diagnostics.Debug.Assert(false);
            }
            return memberType;
        }
        private TtShaderSourceCode GetEnginePreprocessors(RName name, bool bWriteFile)
        {
            var result = new TtShaderSourceCode();
            var codeBuilder = new Bricks.CodeBuilder.UHLSLCodeGenerator();
            string sourceCode = "";

            var ShaderEnums = new List<KeyValuePair<TtShaderDefineAttribute, Rtti.UTypeDesc>>();
            var ShaderStructs = new List<KeyValuePair<TtShaderDefineAttribute, Rtti.UTypeDesc>>();
            Rtti.UTypeDescManager.Instance.InterateTypes((Rtti.UTypeDesc type) =>
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
                        System.Diagnostics.Debug.Assert(i.Name[0] == 'm');
                        codeBuilder.AddLine($"{ToHLSLTypeString(i.FieldType)} {i.Name.Substring(1)};", ref sourceCode);
                    }
                }
                codeBuilder.PopSegment(ref sourceCode, true);
            }
            codeBuilder.AddLine($"#endif//define ENGINE_PREPROCESSORTS_INC", ref sourceCode);

            result.SourceCode.TextCode = sourceCode;
            result.CodeName = name;

            if (bWriteFile)
            {
                IO.TtFileManager.WriteAllText(UEngine.Instance.FileManager.GetPath(IO.TtFileManager.ERootDir.Cache, IO.TtFileManager.ESystemDir.DebugUtility)
                    + $"/engine_preprosessors.cginc", sourceCode);
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
            if (name.ExtName == Graphics.Pipeline.Shader.UMaterial.AssetExt)
            {
                var saveMode = Thread.Async.TtContextThreadManager.ImmidiateMode;
                Thread.Async.TtContextThreadManager.ImmidiateMode = true;
                var task = UEngine.Instance.GfxDevice.MaterialManager.GetMaterial(name);
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
