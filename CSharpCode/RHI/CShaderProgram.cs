using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.RHI
{
    public class CShaderProgram : AuxPtrType<IShaderProgram>
    {
        public class ShaderVarAttribute : Attribute
        {
            public string VarType;
            public string CBuffer;
        }
        public class IShaderVarIndexer
        {
            public void UpdateIndex(CShaderProgram program)
            {
                var members = this.GetType().GetFields();
                foreach (var i in members)
                {
                    var attrs = i.GetCustomAttributes(typeof(CShaderProgram.ShaderVarAttribute), true);
                    if (attrs.Length == 0)
                    {
                        continue;
                    }

                    var varAttr = attrs[0] as ShaderVarAttribute;
                    if (varAttr.VarType == "Texture")
                    {
                        var index = program.mCoreObject.GetTextureBindSlotIndex(i.Name);
                        i.SetValue(this, index);
                    }
                    else if (varAttr.VarType == "CBuffer")
                    {
                        var index = program.mCoreObject.FindCBuffer(i.Name);
                        i.SetValue(this, index);
                    }
                    else if (varAttr.VarType == "Var")
                    {
                        var index = program.mCoreObject.FindCBufferVar(varAttr.CBuffer, i.Name);
                        i.SetValue(this, index);
                    }
                }
            }
        }
    }
}
