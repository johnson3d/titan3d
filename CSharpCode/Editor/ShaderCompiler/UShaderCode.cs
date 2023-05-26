using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Editor.ShaderCompiler
{
    public class UShaderSourceCode : Graphics.Pipeline.Shader.IShaderCodeProvider
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
                    var rn = UShaderCodeManager.Instance.GetShaderCode(RName.GetRName(file, CodeName.RNameType));
                    if (rn != null)
                    {
                        if (!DependencyCodes.Contains(rn))
                            DependencyCodes.Add(rn);
                    }
                }
                cur = code.IndexOf("#", cur);
            }

            DependencyCodes.Sort((x, y) =>
            {
                return x.CodeName.CompareTo(y.CodeName);
            });

            string fullCode = code;
            foreach(var i in DependencyCodes)
            {
                fullCode += i.SourceCode.TextCode;
            }
            CodeHash = Hash160.CreateHash160(fullCode);
            IsDirty = false;
        }
        public bool IsDirty { get; set; } = true;
        public RName CodeName { get; set; }
        public NxRHI.UShaderCode DefineCode { get; private set; } = new NxRHI.UShaderCode();
        public NxRHI.UShaderCode SourceCode { get; private set; } = new NxRHI.UShaderCode();
        public Hash160 CodeHash { get; private set; }
        public List<UShaderSourceCode> DependencyCodes { get; } = new List<UShaderSourceCode>();
    }
    public class UShaderCodeManager
    {
        public static UShaderCodeManager Instance { get; } = new UShaderCodeManager();
        public Dictionary<RName, UShaderSourceCode> Codes { get; } = new Dictionary<RName, UShaderSourceCode>();
        ~UShaderCodeManager()
        {
            Cleanup();
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
                if (i.EndsWith(".vcxitems") || i.EndsWith(".filters"))
                    continue;

                var path = IO.TtFileManager.GetRelativePath(root, i);
                var rn = RName.GetRName(path, shaderDir.RNameType);
                var code = LoadCode(rn);
                Codes.Add(rn, code);
            }

            foreach(var i in Codes)
            {
                i.Value.CollectIncludes();
            }
        }
        public void Cleanup()
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
        public UShaderSourceCode GetShaderCode(RName name)
        {
            UShaderSourceCode result;
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
        protected UShaderSourceCode LoadCode(RName name)
        {
            if (IO.TtFileManager.FileExists(name.Address) == false)
                return null;

            var code = IO.TtFileManager.ReadAllText(name.Address);
            var result = new UShaderSourceCode();
            result.SourceCode.TextCode = code;
            result.CodeName = name;
            return result;
        }
    }
}
