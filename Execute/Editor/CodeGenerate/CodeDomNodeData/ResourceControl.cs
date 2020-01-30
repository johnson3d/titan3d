using System;
using System.Collections.Generic;
using System.Text;

namespace CodeDomNode
{
    [EngineNS.Rtti.MetaClass]
    public class ResourceControlConstructParam : CodeGenerateSystem.Base.ConstructionParams
    {

    }
    [CodeGenerateSystem.CustomConstructionParams(typeof(ResourceControlConstructParam))]
    public partial class ResourceControl : CodeGenerateSystem.Base.BaseNodeControl
    {
        partial void InitConstruction();
        public ResourceControl(CodeGenerateSystem.Base.ConstructionParams csParam)
            : base(csParam)
        {
            InitConstruction();
        }

        public static void InitNodePinTypes(CodeGenerateSystem.Base.ConstructionParams smParam)
        {

        }
    }
}
