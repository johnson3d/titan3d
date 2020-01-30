using System.Windows;
using System.Windows.Controls;
using CodeGenerateSystem.Base;

namespace MaterialEditor.Controls.Value
{
    [EngineNS.Rtti.MetaClass]
    public class ParticleLifeRateConstructionParams : NodeControlConstructionBase { }
    /// <summary>
    /// Interaction logic for ParticleLifeRate.xaml
    /// </summary>
    [CodeGenerateSystem.ShowInNodeList("Params/Particle/ParticleLifeRate", "生命的周期比率，0为出生，1为死亡(ParticleLifeRate)")]
    [CodeGenerateSystem.CustomConstructionParams(typeof(ParticleLifeRateConstructionParams))]
    public partial class ParticleLifeRate : BaseNodeControl, MaterialStreamRequire
    {
        public ParticleLifeRate(CodeGenerateSystem.Base.ConstructionParams smParam)
            : base(smParam)
        {
            InitializeComponent();

            AddLinkPinInfo("TimeValueLink", TimeValueLink, TimeValueLink.BackBrush);
        }
        public static void InitNodePinTypes(CodeGenerateSystem.Base.ConstructionParams smParam)
        {
            CollectLinkPinInfo(smParam, "TimeValueLink", enLinkType.Float1, enBezierType.Right, enLinkOpType.Start, true);
        }
        public override string GCode_GetValueName(CodeGenerateSystem.Base.LinkPinControl element, CodeGenerateSystem.Base.GenerateCodeContext_Method context)
        {
            return "mtl.mLightMapUV.w";
        }

        public override string GCode_GetTypeString(CodeGenerateSystem.Base.LinkPinControl element, CodeGenerateSystem.Base.GenerateCodeContext_Method context)
        {
            return "float";
        }

        public string GetStreamRequire()
        {
            return "LightMapUV";
        }
    }
}
