using CodeGenerateSystem.Base;
using System.Windows;
using System.Windows.Controls;

namespace MaterialEditor.Controls
{
    [EngineNS.Rtti.MetaClass]
    public class ShaderAutoDataConstructionParams : NodeControlConstructionBase { }
    /// <summary>
    /// Interaction logic for ShaderAutoData.xaml
    /// </summary>
    [CodeGenerateSystem.CustomConstructionParams(typeof(ShaderAutoDataConstructionParams))]
    public partial class ShaderAutoData : BaseNodeControl
    {
        string mTitle = "";
        public string Title
        {
            get { return mTitle; }
            set
            {
                mTitle = value;
                OnPropertyChanged("Title");
            }
        }

        string mValueType = "";

        string mDescription = "";
        public string Description
        {
            get { return mDescription; }
            set
            {
                mDescription = value;
                OnPropertyChanged("Describe");
            }
        }

        public ShaderAutoData(CodeGenerateSystem.Base.ConstructionParams smParam)
            : base(smParam)
        {
            InitializeComponent();

            var splits = smParam.ConstructParam.Split(',');
            mValueType = splits[0];
            Title = splits[1];
            Description = splits[2] + "(" + mValueType + ")";
            OutLink.BackBrush = Program.GetBrushFromValueType(mValueType, this);
            AddLinkPinInfo("OutLink", OutLink, OutLink.BackBrush);
        }
        public static void InitNodePinTypes(CodeGenerateSystem.Base.ConstructionParams smParam)
        {
            var splits = smParam.ConstructParam.Split(',');
            var valueType = splits[0];
            CollectLinkPinInfo(smParam, "OutLink", LinkPinControl.GetLinkTypeFromTypeString(valueType), enBezierType.Right, enLinkOpType.Start, true);
        }

        //public override string GetValueDefine()
        //{
        //    return mValueType + " " + GCode_GetValueName(null)+ ";\r\n";// + " : " + Title + ";\r\n";
        //}

        public override string GCode_GetValueName(CodeGenerateSystem.Base.LinkPinControl element, CodeGenerateSystem.Base.GenerateCodeContext_Method context)
        {
            return Title;// + "_" + EngineNS.Editor.Assist.GetValuedGUIDString(this.Id);
        }

        public override string GCode_GetTypeString(CodeGenerateSystem.Base.LinkPinControl element, CodeGenerateSystem.Base.GenerateCodeContext_Method context)
        {
            return mValueType;
        }
    }
}
