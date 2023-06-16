using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Rtti
{
    public class MissingTypeConverter
    {
        public string TypeStr;
        public Rtti.UTypeDesc ConvertType;
    }
    public class UMissingTypeManager
    {
        public static UMissingTypeManager Instance { get; } = new UMissingTypeManager();
        public Dictionary<string, MissingTypeConverter> MissingTypes { get; } = new Dictionary<string, MissingTypeConverter>();
        public void Initialize()
        {
            //var tmp = new MissingTypeConverter();
            //tmp.TypeStr = "System.String@mscorlib";
            //tmp.ConvertType = typeof(string);
            //MissingTypes.Add(tmp.TypeStr, tmp);

            //tmp = new MissingTypeConverter();
            //tmp.TypeStr = "System.Guid@mscorlib";
            //tmp.ConvertType = typeof(System.Guid);
            //MissingTypes.Add(tmp.TypeStr, tmp);

            //tmp = new MissingTypeConverter();
            //tmp.TypeStr = "System.Boolean@mscorlib";
            //tmp.ConvertType = typeof(System.Guid);
            //MissingTypes.Add(tmp.TypeStr, tmp); 

            //var tmp = new MissingTypeConverter();
            //tmp.TypeStr = "System.Collections.Generic.List<EngineNS.Graphics.Pipeline.Shader.UMaterial.StringPair@EngineCore,>@mscorlib";
            //tmp.ConvertType = Rtti.UTypeDescGetter<List<StringPair>>.TypeDesc;
            //MissingTypes.Add(tmp.TypeStr, tmp);

            //tmp = new MissingTypeConverter();
            //tmp.TypeStr = "System.Collections.Generic.List<EngineNS.Graphics.Pipeline.Shader.UMaterial.StringPair@EngineCore,>@System.Private.CoreLib";
            //tmp.ConvertType = Rtti.UTypeDescGetter<List<StringPair>>.TypeDesc;
            //MissingTypes.Add(tmp.TypeStr, tmp);

            //tmp = new MissingTypeConverter();
            //tmp.TypeStr = "EngineNS.Graphics.Pipeline.Shader.UMaterial.StringPair@EngineCore";
            //tmp.ConvertType = Rtti.UTypeDescGetter<StringPair>.TypeDesc;
            //MissingTypes.Add(tmp.TypeStr, tmp);

            //tmp = new MissingTypeConverter();
            //tmp.TypeStr = "EngineNS.RHI.CShaderResourceView@EngineCore";
            //tmp.ConvertType = Rtti.UTypeDescGetter<NxRHI.USrView>.TypeDesc;
            //MissingTypes.Add(tmp.TypeStr, tmp);

            //tmp = new MissingTypeConverter();
            //tmp.TypeStr = "EngineNS.RHI.CShaderResourceViewAMeta@EngineCore";
            //tmp.ConvertType = Rtti.UTypeDescGetter<NxRHI.USrViewAMeta>.TypeDesc;
            //MissingTypes.Add(tmp.TypeStr, tmp); 

            //tmp = new MissingTypeConverter();
            //tmp.TypeStr = "EngineNS.Graphics.Mesh.CMeshPrimitives@EngineCore";
            //tmp.ConvertType = Rtti.UTypeDescGetter<Graphics.Mesh.UMeshPrimitives>.TypeDesc;
            //MissingTypes.Add(tmp.TypeStr, tmp);

            //tmp = new MissingTypeConverter();
            //tmp.TypeStr = "EngineNS.Graphics.Mesh.CMeshPrimitivesAMeta@EngineCore";
            //tmp.ConvertType = Rtti.UTypeDescGetter<Graphics.Mesh.UMeshPrimitivesAMeta>.TypeDesc;
            //MissingTypes.Add(tmp.TypeStr, tmp);

            //tmp = new MissingTypeConverter();
            //tmp.TypeStr = "EngineNS.IBlendStateDesc@EngineCore";
            //tmp.ConvertType = Rtti.UTypeDescGetter<EngineNS.NxRHI.FBlendDesc>.TypeDesc;
            //MissingTypes.Add(tmp.TypeStr, tmp);

            //tmp = new MissingTypeConverter();
            //tmp.TypeStr = "EngineNS.IDepthStencilStateDesc@EngineCore";
            //tmp.ConvertType = Rtti.UTypeDescGetter<EngineNS.NxRHI.FDepthStencilDesc>.TypeDesc;
            //MissingTypes.Add(tmp.TypeStr, tmp);

            //tmp = new MissingTypeConverter();
            //tmp.TypeStr = "EngineNS.IRasterizerStateDesc@EngineCore";
            //tmp.ConvertType = Rtti.UTypeDescGetter<EngineNS.NxRHI.FRasterizerDesc>.TypeDesc;
            //MissingTypes.Add(tmp.TypeStr, tmp);

            //tmp = new MissingTypeConverter();
            //tmp.TypeStr = "EngineNS.ISamplerStateDesc@EngineCore";
            //tmp.ConvertType = Rtti.UTypeDescGetter<EngineNS.NxRHI.FSamplerDesc>.TypeDesc;
            //MissingTypes.Add(tmp.TypeStr, tmp); 
        }
        public Rtti.UTypeDesc GetConvertType(string typeStr)
        {
            MissingTypeConverter result;
            if (MissingTypes.TryGetValue(typeStr, out result))
                return result.ConvertType;

            return Rtti.UTypeDescManager.Instance.FindNameAlias(typeStr);
        }

        #region TmpClass
        public class StringPair
        {
        }
        #endregion
    }
}
