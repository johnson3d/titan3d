using CodeGenerateSystem.Base;
using System.Windows;
using System.Windows.Controls;

namespace MaterialEditor.Controls.Value
{
    [EngineNS.Rtti.MetaClass]
    public class ParticleFrameConstructionParams : NodeControlConstructionBase { }
    /// <summary>
    /// Interaction logic for ParticleFrame.xaml
    /// </summary>
    [CodeGenerateSystem.ShowInNodeList("Params/Particle/ParticleFrame", "控制粒子帧动画关键帧(ParticleFrame)")]
    [CodeGenerateSystem.CustomConstructionParams(typeof(ParticleFrameConstructionParams))]
    public partial class ParticleFrame : BaseNodeControl, MaterialStreamRequire
    {
        public ParticleFrame(CodeGenerateSystem.Base.ConstructionParams smParam)
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
            return "mtl.mLocalBinorm.w";
        }

        public override string GCode_GetTypeString(CodeGenerateSystem.Base.LinkPinControl element, CodeGenerateSystem.Base.GenerateCodeContext_Method context)
        {
            return "float";
        }

        public string GetStreamRequire()
        {
            return "LocalBinorm";
        }
    }
}
