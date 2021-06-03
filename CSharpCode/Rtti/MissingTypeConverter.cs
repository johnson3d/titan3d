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
        public UMissingTypeManager()
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

            var tmp = new MissingTypeConverter();
            tmp.TypeStr = "System.Collections.Generic.List<EngineNS.Graphics.Pipeline.Shader.UMaterial.StringPair@EngineCore,>@mscorlib";
            tmp.ConvertType = Rtti.UTypeDescGetter<List<StringPair>>.TypeDesc;
            MissingTypes.Add(tmp.TypeStr, tmp);

            tmp = new MissingTypeConverter();
            tmp.TypeStr = "System.Collections.Generic.List<EngineNS.Graphics.Pipeline.Shader.UMaterial.StringPair@EngineCore,>@System.Private.CoreLib";
            tmp.ConvertType = Rtti.UTypeDescGetter<List<StringPair>>.TypeDesc;
            MissingTypes.Add(tmp.TypeStr, tmp);

            tmp = new MissingTypeConverter();
            tmp.TypeStr = "EngineNS.Graphics.Pipeline.Shader.UMaterial.StringPair@EngineCore";
            tmp.ConvertType = Rtti.UTypeDescGetter<StringPair>.TypeDesc;
            MissingTypes.Add(tmp.TypeStr, tmp);
        }
        public Rtti.UTypeDesc GetConvertType(string typeStr)
        {
            MissingTypeConverter result;
            if (MissingTypes.TryGetValue(typeStr, out result))
                return result.ConvertType;
            return null;
        }

        #region TmpClass
        public class StringPair
        {
        }
        #endregion
    }
}
