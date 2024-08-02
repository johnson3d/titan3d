using EngineNS.Bricks.CodeBuilder.MacrossNode;
using Org.BouncyCastle.Asn1.Mozilla;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Graphics.Pipeline.Shader
{
    #region Modifier Permutations
    public class TtMdfPermutation
    {
        public virtual void BuildPermutationCode(ref string codeString, Bricks.CodeBuilder.Backends.UHLSLCodeGenerator builder)
        {
        }
    }

    public class TtMdfPermutationsBase
    {
        public List<Rtti.UTypeDesc> Permutations { get; } = new List<Rtti.UTypeDesc>();
        internal static string GetPermutationsName(List<Rtti.UTypeDesc> lst)
        {
            string result = "";
            foreach (var i in lst)
            {
                result += "@" + i.FullName;
            }
            return result;
        }
        internal static void SortPermutations(List<Rtti.UTypeDesc> lst)
        {
            lst.Sort((x, y) =>
            {
                return x.FullName.CompareTo(y.FullName);
            });
        }
        public void BuildPermutationCode(ref string codeString, Bricks.CodeBuilder.Backends.UHLSLCodeGenerator builder)
        {
            foreach (var i in Permutations)
            {
                var po = Rtti.UTypeDescManager.CreateInstance(i) as TtMdfPermutation;
                po.BuildPermutationCode(ref codeString, builder);
            }
        }
        public Rtti.UTypeDesc AddPermutation<T>() where T : TtMdfPermutation
        {
            var nps = new List<Rtti.UTypeDesc>(Permutations);
            nps.Remove(Rtti.UTypeDescGetter<T>.TypeDesc);
            nps.Add(Rtti.UTypeDescGetter<T>.TypeDesc);
            SortPermutations(nps);

            return TtMdfPermutationsTypeManager.Instance.FindType(GetPermutationsName(nps));
        }
        public Rtti.UTypeDesc RemovePermutation<Tar>() where Tar : TtMdfPermutation
        {
            var nps = new List<Rtti.UTypeDesc>(Permutations);
            nps.Remove(Rtti.UTypeDescGetter<Tar>.TypeDesc);
            SortPermutations(nps);

            return TtMdfPermutationsTypeManager.Instance.FindType(GetPermutationsName(nps));
        }
        public Rtti.UTypeDesc ReplacePermutation<Tar, Src>() where Tar : TtMdfPermutation
            where Src : TtMdfPermutation
        {
            var nps = new List<Rtti.UTypeDesc>(Permutations);
            nps.Remove(Rtti.UTypeDescGetter<Tar>.TypeDesc);
            nps.Add(Rtti.UTypeDescGetter<Src>.TypeDesc);
            SortPermutations(nps);

            return TtMdfPermutationsTypeManager.Instance.FindType(GetPermutationsName(nps));
        }
    }

    public class TtMdfPermutations1<T0> : TtMdfPermutationsBase where T0 : TtMdfPermutation
    {
        public TtMdfPermutations1()
        {
            Permutations.Add(Rtti.UTypeDescGetter<T0>.TypeDesc);
            SortPermutations(Permutations);
        }
    }

    public class TtMdfPermutations2<T0, T1> : TtMdfPermutationsBase where T0 : TtMdfPermutation
        where T1 : TtMdfPermutation
    {
        public TtMdfPermutations2()
        {
            Permutations.Add(Rtti.UTypeDescGetter<T0>.TypeDesc);
            Permutations.Add(Rtti.UTypeDescGetter<T1>.TypeDesc);
            SortPermutations(Permutations);
        }
    }

    public class TtMdfPermutationsTypeManager
    {
        static TtMdfPermutationsTypeManager mInstance;
        public static TtMdfPermutationsTypeManager Instance
        {
            get
            {
                if (mInstance == null)
                {
                    mInstance = new TtMdfPermutationsTypeManager();
                }
                return mInstance;
            }
        }
        protected Dictionary<string, Rtti.UTypeDesc> MdfPermutationTypes = new Dictionary<string, Rtti.UTypeDesc>();
        public TtMdfPermutationsTypeManager()
        {
            List<Rtti.UTypeDesc> types = new List<Rtti.UTypeDesc>();
            Rtti.UTypeDescManager.Instance.GetInheritTypes(Rtti.UTypeDescGetter<TtMdfPermutationsBase>.TypeDesc, types);
            foreach(var i in types)
            {
                if(i.SystemType.IsGenericType)
                {
                    var argTypes = new List<Type>(i.SystemType.GenericTypeArguments);
                    argTypes.Sort((x, y) =>
                    {
                        return x.FullName.CompareTo(y.FullName);
                    });
                    string name = "";
                    foreach (var j in argTypes)
                    {
                        name += "@" + j.FullName;
                    }
                    MdfPermutationTypes.Add(name, i);
                }
            }
        }
        public Rtti.UTypeDesc FindType(string name)
        {
            if (MdfPermutationTypes.TryGetValue(name, out var result))
                return result;
            return null;
        }
    }
    #endregion

    public interface IMeshModifier : IDisposable
    {
        public string ModifierNameVS { get; }
        public string ModifierNamePS { get; }
        public RName SourceName { get; }
        public NxRHI.EVertexStreamType[] GetNeedStreams();
        public EPixelShaderInput[] GetPSNeedInputs();
        public unsafe NxRHI.FShaderCode* GetHLSLCode(string includeName, string includeOriName);
        public string GetUniqueText();
        public void Initialize(Graphics.Mesh.UMaterialMesh materialMesh);
        public void OnDrawCall(TtMdfQueueBase mdfQueue, NxRHI.ICommandList cmd, NxRHI.UGraphicDraw drawcall, Graphics.Pipeline.URenderPolicy policy, Graphics.Mesh.TtMesh.TtAtom atom);
    }

    public abstract class TtMdfQueueBase : AuxPtrType<IMdfQueue>, IShaderCodeProvider
    {
        public List<IMeshModifier> Modifiers { get; } = new List<IMeshModifier>();
        public RName CodeName { get; set; }
        public NxRHI.UShaderCode DefineCode { get; protected set; }
        public NxRHI.UShaderCode SourceCode { get; protected set; }
        public object MdfDatas;
        public override void Dispose()
        {
            foreach(var i in Modifiers)
            {
                i.Dispose();
            }
            Modifiers.Clear();
            base.Dispose();
        }
        public virtual void Initialize(Graphics.Mesh.UMaterialMesh materialMesh)
        {
            foreach (var i in Modifiers)
            {
                i.Initialize(materialMesh);
            }
        }
        public virtual NxRHI.EVertexStreamType[] GetNeedStreams()
        {
            var result = new List<NxRHI.EVertexStreamType>();
            foreach (var i in Modifiers)
            {
                var needs = i.GetNeedStreams();
                if (needs == null)
                    continue;
                result.AddRange(i.GetNeedStreams());
            }
            return result.ToArray();
        }
        public virtual EPixelShaderInput[] GetPSNeedInputs()
        {
            var result = new List<EPixelShaderInput>();
            foreach (var i in Modifiers)
            {
                var needs = i.GetPSNeedInputs();
                if (needs == null)
                    continue;
                result.AddRange(i.GetPSNeedInputs());
            }
            return result.ToArray();
        }
        public virtual unsafe NxRHI.FShaderCode* GetHLSLCode(string includeName, string oriInc)
        {
            foreach (var i in Modifiers)
            {
                var code = i.GetHLSLCode(includeName, oriInc);
                if (code != (NxRHI.FShaderCode*)0)
                {
                    return code;
                }
            }
            return (NxRHI.FShaderCode*)0;
        }
        protected virtual void BuildMdfFunctions(ref string codeString, Bricks.CodeBuilder.Backends.UHLSLCodeGenerator codeBuilder)
        {
            string hashCode = "";
            foreach (var i in Modifiers)
            {
                if (i.SourceName != null)
                    codeBuilder.AddLine($"#include \"{i.SourceName.Address}\"", ref codeString);
            }
            codeBuilder.AddLine("void MdfQueueDoModifiers(inout PS_INPUT output, VS_MODIFIER input)", ref codeString);
            codeBuilder.PushSegment(ref codeString);
            {
                foreach (var i in Modifiers)
                {
                    if (i.SourceName == null || i.ModifierNameVS == null)
                        continue;
                    codeBuilder.AddLine($"{i.ModifierNameVS}(output, input);", ref codeString);

                    var mdfCode = Editor.ShaderCompiler.UShaderCodeManager.Instance.GetShaderCodeProvider(i.SourceName);
                    hashCode += mdfCode.SourceCode.TextCode;
                }
            }
            codeBuilder.PopSegment(ref codeString);

            codeBuilder.AddLine("void MdfQueueDoModifiersPS(inout PS_INPUT input, inout MTL_OUTPUT mtl)", ref codeString);
            codeBuilder.PushSegment(ref codeString);
            {
                foreach (var i in Modifiers)
                {
                    if (i.SourceName == null || i.ModifierNamePS == null)
                        continue;
                    codeBuilder.AddLine($"{i.ModifierNamePS}(input, mtl);", ref codeString);

                    var mdfCode = Editor.ShaderCompiler.UShaderCodeManager.Instance.GetShaderCodeProvider(i.SourceName);
                    hashCode += mdfCode.SourceCode.TextCode;
                }
            }
            codeBuilder.PopSegment(ref codeString);

            codeBuilder.AddLine("#define MDFQUEUE_FUNCTION", ref codeString);
            codeBuilder.AddLine("#define MDFQUEUE_FUNCTION_PS", ref codeString);
            codeBuilder.AddLine($"//Hash for :{UniHash32.APHash(hashCode)}", ref codeString);
        }

        protected virtual void PreBuildMdfFunctions(ref string codeString, Bricks.CodeBuilder.Backends.UHLSLCodeGenerator codeBuilder)
        {

        }
        protected void UpdateShaderCode()
        {
            var codeBuilder = new Bricks.CodeBuilder.Backends.UHLSLCodeGenerator();
            string codeString = "";

            PreBuildMdfFunctions(ref codeString, codeBuilder);
            BuildMdfFunctions(ref codeString, codeBuilder);

            SourceCode = new NxRHI.UShaderCode();
            SourceCode.TextCode = codeString;
        }

        public override string ToString()
        {
            return Rtti.UTypeDescManager.Instance.GetTypeStringFromType(this.GetType());
            //string result = $"Var: {DefineCode?.AsText}\n";
            //result += $"Code: {SourceCode.AsText}\n";
            //return result;
        }
        protected Hash160 mMdfQueueHash;
        public virtual Hash160 MdfQueueHash
        {
            get
            {
                return mMdfQueueHash;
            }
            set
            {
                mMdfQueueHash = value;
            }
        }
        public Hash160 GetHash()
        {
            if (mMdfQueueHash == Hash160.Emtpy)
            {
                string result = DefineCode?.TextCode;
                result += SourceCode.TextCode;
                foreach (var i in Modifiers)
                {
                    if (i.SourceName == null)
                        continue;
                    var shadingCode = Editor.ShaderCompiler.UShaderCodeManager.Instance.GetShaderCode(i.SourceName);
                    result += shadingCode;
                    result += i.GetUniqueText();
                }
                mMdfQueueHash = Hash160.CreateHash160(result);
            }
            return mMdfQueueHash;
        }
        public TtMdfQueueBase()
        {
            mCoreObject = IMdfQueue.CreateInstance();
        }
        
        public virtual void CopyFrom(TtMdfQueueBase mdf)
        {
            OnDrawCallCallback = mdf.OnDrawCallCallback;
        }
        
        public delegate void FOnDrawCall(NxRHI.UGraphicDraw drawcall, URenderPolicy policy, Mesh.TtMesh.TtAtom atom);
        public FOnDrawCall OnDrawCallCallback = null;
        public virtual void OnDrawCall(NxRHI.ICommandList cmd, NxRHI.UGraphicDraw drawcall, URenderPolicy policy, Mesh.TtMesh.TtAtom atom)
        {
            if (OnDrawCallCallback != null)
                OnDrawCallCallback(drawcall, policy, atom);
            foreach(var i in Modifiers)
            {
                i.OnDrawCall(this, cmd, drawcall, policy, atom);
            }
        }
    }

    public class TtMdfQueue1<T0> : TtMdfQueueBase where T0 : IMeshModifier, new()
    {
        public TtMdfQueue1()
        {
            Modifiers.Add(new T0());

            UpdateShaderCode();
        }
    }

    public class TtMdfQueue2<T0, T1> : TtMdfQueueBase where T0 : IMeshModifier, new()
        where T1 : IMeshModifier, new()
    {
        public TtMdfQueue2()
        {
            Modifiers.Add(new T0());
            Modifiers.Add(new T1());

            UpdateShaderCode();
        }
    }

}
