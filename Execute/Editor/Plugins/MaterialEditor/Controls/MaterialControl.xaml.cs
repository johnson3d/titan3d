using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using CodeGenerateSystem.Base;
using EngineNS.IO;

namespace MaterialEditor.Controls
{
    [EngineNS.Rtti.MetaClass]
    public class MaterialControlConstructionParams : NodeControlConstructionBase { }
    /// <summary>
    /// MaterialControl.xaml 的交互逻辑
    /// </summary>
    [CodeGenerateSystem.CustomConstructionParams(typeof(MaterialControlConstructionParams))]
    public partial class MaterialControl : BaseNodeControl
    {
        class MaterialLinkData
        {
            public bool mCommon = false;
            public string mNickName;
            public string mStrName;
            public string mStrType;
            public string mDescription;
        }
        Dictionary<CodeGenerateSystem.Base.LinkPinControl, MaterialLinkData> mMaterialDataDic = new Dictionary<CodeGenerateSystem.Base.LinkPinControl, MaterialLinkData>();
        List<CodeGenerateSystem.Base.LinkPinControl> mInLinks_PS = new List<CodeGenerateSystem.Base.LinkPinControl>();
        List<CodeGenerateSystem.Base.LinkPinControl> mInLinks_VS = new List<CodeGenerateSystem.Base.LinkPinControl>();
        List<CodeGenerateSystem.Base.LinkPinControl> mOutLinks = new List<CodeGenerateSystem.Base.LinkPinControl>();

        public string TitleString
        {
            get { return (string)GetValue(TitleStringProperty); }
            set { SetValue(TitleStringProperty, value); }
        }
        public static readonly DependencyProperty TitleStringProperty = DependencyProperty.Register("TitleString", typeof(string), typeof(MaterialControl), new FrameworkPropertyMetadata(null));


        public MaterialControl(CodeGenerateSystem.Base.ConstructionParams smParam)
            : base(smParam)
        {
            InitializeComponent();

            IsDeleteable = false;
            
            mOutLinks.Clear();
            mInLinks_VS.Clear();
            mInLinks_PS.Clear();

            //AddSemanticHandle("Base Color", "vAlbedo", "float4", "Base Color, Albedo", true);
            //AddSemanticHandle("LightMapColor", "vLightMapColor", "float4", "LightMapColor", true);
            //AddSemanticHandle("Normal", "vNormal", "float4", "Normal", true);
            //AddSemanticHandle("BumpNormal", "vBumpNormal", "float4", "BumpNormal", true);
            //AddSemanticHandle("Specular", "vSpecular", "float4", "Specular", true);
            //AddSemanticHandle("Bloom", "vBloom", "float", "Bloom", true);
            //AddSemanticHandle("Metal", "vMetal", "float", "Metal", true);
            //AddSemanticHandle("Roughness", "vRoughness", "float", "Roughness", true);
            //AddSemanticHandle("LitMode", "vLitMode", "float", "光照模式，是皮肤处理，还是金属处理，等等", true);
            //AddSemanticHandle("Pixel Shader UV", "vUV", "float2", "Pixel Shader UV", true, true);

            AddSemanticHandle("Albedo", "mAlbedo", "float3", "color of material without light shading", true);
            AddSemanticHandle("Normal", "mNormal", "float3", "surface normal", true);
            AddSemanticHandle("Metallic", "mMetallic", "float", "the metal value of material", true);
            AddSemanticHandle("Smooth", "mRough", "float", "The smoothness of surface", true);
            AddSemanticHandle("AbsSpecular", "mAbsSpecular", "float", "Absolute specular", true);
            AddSemanticHandle("Transmit", "mTransmit", "float", "transimission", true);
            AddSemanticHandle("Emissive", "mEmissive", "float3", "Emissive", true);
            AddSemanticHandle("Fuzz", "mFuzz", "float", "fuzzy", true);
            AddSemanticHandle("Iridescence", "mIridescence", "float", "iridescence", true);
            AddSemanticHandle("Distortion", "mDistortion", "float", "distort the background", true);
            AddSemanticHandle("Alpha", "mAlpha", "float", "material alpha", true);
            AddSemanticHandle("AlphaTest", "mAlphaTest", "float", "alpha test threshold", true);
            AddSemanticHandle("VertexOffset", "mVertexOffset", "float3", "vertex animation", true, true);
            AddSemanticHandle("SubAlbedo", "mSubAlbedo", "float3", "subsurface albedo", true);

            AddSemanticHandle("AO", "mAO", "float", "AO", true);
            AddSemanticHandle("Mask", "mMask", "float", "mask", true);

            AddSemanticHandle("ShadowColor", "mShadowColor", "float3", "shadow color mainly for npr to use", true);
            AddSemanticHandle("DeepShadow", "mDeepShadow", "float", "make the shadow even deep for npr to use", true);
            AddSemanticHandle("MoodColor", "mMoodColor", "float3", "mood color for npr to use", true);


            mMtlMacros = new MtlMacros(this);
        }
        public override bool CanDuplicate()
        {
            return false;
        }

        public static void InitNodePinTypes(CodeGenerateSystem.Base.ConstructionParams smParam)
        {
            CollectLinkPinInfo(smParam, "mAlbedo", LinkPinControl.GetLinkTypeFromTypeString("float3"), enBezierType.Left, enLinkOpType.End, false);
            CollectLinkPinInfo(smParam, "mNormal", LinkPinControl.GetLinkTypeFromTypeString("float3"), enBezierType.Left, enLinkOpType.End, false);
            CollectLinkPinInfo(smParam, "mMetallic", LinkPinControl.GetLinkTypeFromTypeString("float"), enBezierType.Left, enLinkOpType.End, false);
            CollectLinkPinInfo(smParam, "mRough", LinkPinControl.GetLinkTypeFromTypeString("float"), enBezierType.Left, enLinkOpType.End, false);
            CollectLinkPinInfo(smParam, "mAbsSpecular", LinkPinControl.GetLinkTypeFromTypeString("float"), enBezierType.Left, enLinkOpType.End, false);
            CollectLinkPinInfo(smParam, "mTransmit", LinkPinControl.GetLinkTypeFromTypeString("float"), enBezierType.Left, enLinkOpType.End, false);
            CollectLinkPinInfo(smParam, "mEmissive", LinkPinControl.GetLinkTypeFromTypeString("float3"), enBezierType.Left, enLinkOpType.End, false);
            CollectLinkPinInfo(smParam, "mFuzz", LinkPinControl.GetLinkTypeFromTypeString("float"), enBezierType.Left, enLinkOpType.End, false);
            CollectLinkPinInfo(smParam, "mIridescence", LinkPinControl.GetLinkTypeFromTypeString("float"), enBezierType.Left, enLinkOpType.End, false);
            CollectLinkPinInfo(smParam, "mDistortion", LinkPinControl.GetLinkTypeFromTypeString("float"), enBezierType.Left, enLinkOpType.End, false);
            CollectLinkPinInfo(smParam, "mAlpha", LinkPinControl.GetLinkTypeFromTypeString("float"), enBezierType.Left, enLinkOpType.End, false);
            CollectLinkPinInfo(smParam, "mAlphaTest", LinkPinControl.GetLinkTypeFromTypeString("float"), enBezierType.Left, enLinkOpType.End, false);
            CollectLinkPinInfo(smParam, "mVertexOffset", LinkPinControl.GetLinkTypeFromTypeString("float3"), enBezierType.Left, enLinkOpType.End, false);
            CollectLinkPinInfo(smParam, "mSubAlbedo", LinkPinControl.GetLinkTypeFromTypeString("float3"), enBezierType.Left, enLinkOpType.End, false);
            CollectLinkPinInfo(smParam, "mAO", LinkPinControl.GetLinkTypeFromTypeString("float"), enBezierType.Left, enLinkOpType.End, false);
            CollectLinkPinInfo(smParam, "mMask", LinkPinControl.GetLinkTypeFromTypeString("float"), enBezierType.Left, enLinkOpType.End, false);

            CollectLinkPinInfo(smParam, "mShadowColor", LinkPinControl.GetLinkTypeFromTypeString("float3"), enBezierType.Left, enLinkOpType.End, false);
            CollectLinkPinInfo(smParam, "mDeepShadow", LinkPinControl.GetLinkTypeFromTypeString("float"), enBezierType.Left, enLinkOpType.End, false);
            CollectLinkPinInfo(smParam, "mMoodColor", LinkPinControl.GetLinkTypeFromTypeString("float3"), enBezierType.Left, enLinkOpType.End, false);

        }

        private void AddSemanticHandle(string nickName, string strName, string strType, string description, bool isCommon, bool vsProcess = false)
        {
            Grid grid = new Grid();
            grid.ToolTip = description;
            grid.Margin = new Thickness(8, 2, 8, 2);

            if (isCommon)
                SemanticStackPanel.Children.Add(grid);
            else
                UnusedSemanticStackPanel.Children.Add(grid);

            MaterialLinkData data = new MaterialLinkData();
            data.mNickName = nickName;
            data.mStrName = strName;
            data.mStrType = strType;
            data.mDescription = description;
            data.mCommon = isCommon;

            var ellipse = new CodeGenerateSystem.Controls.LinkInControl()
            {
                Height = 15,
                BackBrush = Program.GetBrushFromValueType(strType, this),
                HorizontalAlignment = HorizontalAlignment.Left,
                Margin = new Thickness(2,2,2,2),
                Direction = enBezierType.Left,
                NameString = nickName + "(" + strType + ")",
            };
            grid.Children.Add(ellipse);
            var linkObjInfo = AddLinkPinInfo(strName, ellipse, null);
            linkObjInfo.OnAddLinkInfo += LinkObjInfo_In_OnAddLinkInfo;
            linkObjInfo.OnDelLinkInfo += LinkObjInfo_In_OnDelLinkInfo;

            if (vsProcess)
                mInLinks_VS.Add(ellipse);
            else
                mInLinks_PS.Add(ellipse);
            mMaterialDataDic[ellipse] = data;
        }

        private void LinkObjInfo_In_OnAddLinkInfo(LinkInfo linkInfo)
        {
            var grid = linkInfo.m_linkToObjectInfo.Parent as Grid;
            if (grid == null)
                return;

            if(grid.Parent == UnusedSemanticStackPanel)
            {
                UnusedSemanticStackPanel.Children.Remove(grid);
                SemanticStackPanel.Children.Add(grid);
                mNeedLayoutUpdateLink = true;
            }
        }
        private void LinkObjInfo_In_OnDelLinkInfo(LinkInfo linkInfo)
        {
            var grid = linkInfo.m_linkToObjectInfo.Parent as Grid;
            if (grid == null)
                return;

            MaterialLinkData data;
            if(mMaterialDataDic.TryGetValue(linkInfo.m_linkToObjectInfo, out data))
            {
                if(!data.mCommon)
                {
                    if (grid.Parent == SemanticStackPanel)
                    {
                        SemanticStackPanel.Children.Remove(grid);
                        UnusedSemanticStackPanel.Children.Add(grid);
                        mNeedLayoutUpdateLink = true;
                    }
                }
            }

        }

        public void GCode_GenerateCode_VS(ref string strDefinitionSegment, ref string strSegment, int nLayer, CodeGenerateSystem.Base.LinkPinControl element)
        {
            string strTab = GCode_GetTabString(nLayer);

            foreach (var link in mInLinks_VS)
            {
                if (!link.HasLink)
                    continue;

                var context = new GenerateCodeContext_Method(null, null);
                link.GetLinkedObject(0, true).GCode_GenerateCode(ref strDefinitionSegment, ref strSegment, nLayer, link.GetLinkedPinControl(0, true), context);
                strSegment += strTab + "mtl." + mMaterialDataDic[link].mStrName + " = " + link.GetLinkedObject(0, true).GCode_GetValueName(link.GetLinkedPinControl(0, true), context) + ";\r\n";
            }
        }
        public void GCode_GenerateCode_PS(ref string strDefinitionSegment, ref string strSegment, int nLayer, CodeGenerateSystem.Base.LinkPinControl element)
        {
            string strTab = GCode_GetTabString(nLayer);

            foreach (var link in mInLinks_PS)
            {
                if (!link.HasLink)
                    continue;

                var context = new GenerateCodeContext_Method(null, null);
                link.GetLinkedObject(0, true).GCode_GenerateCode(ref strDefinitionSegment, ref strSegment, nLayer, link.GetLinkedPinControl(0, true), context);
                strSegment += strTab + "mtl." + mMaterialDataDic[link].mStrName + " = " + link.GetLinkedObject(0, true).GCode_GetValueName(link.GetLinkedPinControl(0, true), context) + ";\r\n";
            }
        }

        public override string GCode_GetValueName(CodeGenerateSystem.Base.LinkPinControl element, CodeGenerateSystem.Base.GenerateCodeContext_Method context)
        {
            var data = mMaterialDataDic[element];
            return "mtl." + data.mStrName;
        }

        public override string GCode_GetTypeString(CodeGenerateSystem.Base.LinkPinControl element, CodeGenerateSystem.Base.GenerateCodeContext_Method context)
        {
            var data = mMaterialDataDic[element];
            return data.mStrType;
        }

        #region MTL_MACROS
        [EngineNS.Rtti.MetaClass]
        public class MtlMacros
        {
            protected MaterialControl mOutClassRef;


            public enum eMtlID
            {
                Unlit,
                Common,
                Transmit,
                Hair,
                Skin,
                Eye,
                NprScene,
                
            }

            protected eMtlID mMaterialID = eMtlID.Common;
            [EngineNS.Rtti.MetaData]
            public eMtlID MaterialID
            {
                get
                {
                    return mMaterialID;
                }
                set
                {
                    mMaterialID = value;
                    mOutClassRef.IsDirty = true;
                }
            }

            [EngineNS.Rtti.MetaData]
            public bool UseAlphaTest
            {
                get;
                set;
            } = false;


            public MtlMacros(MaterialControl OutClassRef)
            {
                EngineNS.CEngine.Instance.MetaClassManager.GetMetaClass(this.GetType());
                mOutClassRef = OutClassRef;
            }
        }

        public MtlMacros mMtlMacros;
        
        public override object GetShowPropertyObject()
        {
            return mMtlMacros;
        }
        #endregion


        public override void Save(XndNode xndNode, bool newGuid)
        {
            var att = xndNode.AddAttrib("MtlMacros");

            att.BeginWrite();
            att.WriteMetaObject(mMtlMacros);
            att.EndWrite();
            base.Save(xndNode, newGuid);
        }
        public override async System.Threading.Tasks.Task Load(XndNode xndNode)
        {
            var att = xndNode.FindAttrib("MtlMacros");
            if (att != null)
            {
                att.BeginRead();
                att.ReadMetaObject(mMtlMacros);
                att.EndRead();
                this.IsDirty = false;
            }
            await base.Load(xndNode);
        }
    }
}
