using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MaterialEditor.Controls.Value
{
    [EngineNS.Rtti.MetaClass]
    public class PixelMaterialDataConstructionParams : NodeControlConstructionBase { }
    /// <summary>
    /// PixelMaterialData.xaml 的交互逻辑
    /// </summary>
    [CodeGenerateSystem.CustomConstructionParams(typeof(PixelMaterialDataConstructionParams))]
    public partial class PixelMaterialData : CodeGenerateSystem.Base.BaseNodeControl, MaterialStreamRequire
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

        public PixelMaterialData(CodeGenerateSystem.Base.ConstructionParams smParam)
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
            CollectLinkPinInfo(smParam, "OutLink", CodeGenerateSystem.Base.LinkPinControl.GetLinkTypeFromTypeString(valueType), CodeGenerateSystem.Base.enBezierType.Right, CodeGenerateSystem.Base.enLinkOpType.Start, true);
        }

        public string GetStreamRequire()
        {
            return Title.Substring(1);
        }

        public override string GCode_GetValueName(CodeGenerateSystem.Base.LinkPinControl element, CodeGenerateSystem.Base.GenerateCodeContext_Method context)
        {
            return "input." + Title;
        }

        public override string GCode_GetTypeString(CodeGenerateSystem.Base.LinkPinControl element, CodeGenerateSystem.Base.GenerateCodeContext_Method context)
        {
            return mValueType;
        }
    }
}
