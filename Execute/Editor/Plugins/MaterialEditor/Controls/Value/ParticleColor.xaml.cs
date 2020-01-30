using System.Windows;
using System.Windows.Controls;
using CodeGenerateSystem.Base;

namespace MaterialEditor.Controls.Value
{
    [EngineNS.Rtti.MetaClass]
    public class ParticleColorConstructionParams : NodeControlConstructionBase { }
    /// <summary>
    /// Interaction logic for ParticleColor.xaml
    /// </summary>
    [CodeGenerateSystem.ShowInNodeList("Params/Particle/ParticleColor", "控制粒子颜色(ParticleColor)")]
    [CodeGenerateSystem.CustomConstructionParams(typeof(ParticleColorConstructionParams))]
    public partial class ParticleColor : BaseNodeControl, MaterialStreamRequire
    {
        public ParticleColor(CodeGenerateSystem.Base.ConstructionParams smParam)
            : base(smParam)
        {
            InitializeComponent();

            AddLinkPinInfo("TimeValueLink", TimeValueLink, TimeValueLink.BackBrush);
        }
        public static void InitNodePinTypes(CodeGenerateSystem.Base.ConstructionParams smParam)
        {
            CollectLinkPinInfo(smParam, "TimeValueLink", enLinkType.UInt4, enBezierType.Right, enLinkOpType.Start, true);
        }

        public override string GCode_GetValueName(CodeGenerateSystem.Base.LinkPinControl element, CodeGenerateSystem.Base.GenerateCodeContext_Method context)
        {
            return "input.vF4_1";
        }

        public override string GCode_GetTypeString(CodeGenerateSystem.Base.LinkPinControl element, CodeGenerateSystem.Base.GenerateCodeContext_Method context)
        {   
            return "uint4";
        }

        public string GetStreamRequire()
        {
            return "vF4_1";
        }
    }
}
