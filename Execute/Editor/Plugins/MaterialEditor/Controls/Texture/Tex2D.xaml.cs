using System;
using System.Windows;
using System.Windows.Controls;
using CodeGenerateSystem.Base;

namespace MaterialEditor.Controls.Texture
{
    [EngineNS.Rtti.MetaClass]
    public class Tex2DConstructionParams : NodeControlConstructionBase { }
    /// <summary>
    /// Tex2D.xaml 的交互逻辑 SM1,2,3,4
    /// </summary>
    //[CodeGenerateSystem.ShowInNodeList("Params.Tex2D", "2D贴图的数据")]
    [CodeGenerateSystem.CustomConstructionParams(typeof(Tex2DConstructionParams))]
    public partial class Tex2D : BaseNodeControl
    {
        EngineNS.RName m_strTexturePath;
        public EngineNS.RName TexturePath
        {
            get { return m_strTexturePath; }
            set
            {
                m_strTexturePath = value;

                if (m_strTexturePath == null)
                {
                 
                    Action action = async () =>
                    {
                        var imgs = await EditorCommon.ImageInit.GetImage(m_strTexturePath.Address);
                        image_Texture.Source = imgs[0];
                    };
                    action();
                    //// 判断扩展名
                    //int nExtIdx = m_strTexturePath.LastIndexOf('.');
                    //string strExt = m_strTexturePath.Substring(nExtIdx);

                    ////string strAbsPath = EngineNS.CEngine.Instance.FileManager._GetAbsPathFromRelativePath(m_strTexturePath);
                    //string strAbsPath = EngineNS.CEngine.Instance.FileManager.Root + m_strTexturePath;
                    //strAbsPath = strAbsPath.Replace("/\\", "\\");

                    //switch (strExt)
                    //{
                    //    case ".dds":
                    //    case ".tga":
                    //        image_Texture.Source = FrameSet.Assist.DDSConverter.Convert(strAbsPath);
                    //        break;

                    //    default:
                    //        image_Texture.Source = new BitmapImage(new Uri(strAbsPath));
                    //        break;
                    //}
                }
            }
        }

        public Tex2D(CodeGenerateSystem.Base.ConstructionParams smParam)
            : base(smParam)
        {
            InitializeComponent();

            var linkObj = AddLinkPinInfo("TextureLink", TextureLink, null);
            linkObj.OnAddLinkInfo += new LinkPinControl.Delegate_OnOperateLinkInfo(TextureLink_OnAddLinkInfo);
            linkObj = AddLinkPinInfo("UVLink", UVLink, null);
            linkObj.OnAddLinkInfo += new LinkPinControl.Delegate_OnOperateLinkInfo(UVLink_OnAddLinkInfo);
            AddLinkPinInfo("RGBALink", RGBALink, RGBALink.BackBrush);
            AddLinkPinInfo("RGBLink", RGBLink, RGBLink.BackBrush);
            AddLinkPinInfo("RLink", RLink, RLink.BackBrush);
            AddLinkPinInfo("GLink", GLink, GLink.BackBrush);
            AddLinkPinInfo("BLink", BLink, BLink.BackBrush);
            AddLinkPinInfo("ALink", ALink, ALink.BackBrush);
        }

        public static void InitNodePinTypes(CodeGenerateSystem.Base.ConstructionParams smParam)
        {
            CollectLinkPinInfo(smParam, "TextureLink", enLinkType.Texture, enBezierType.Left, enLinkOpType.End, false);
            CollectLinkPinInfo(smParam, "UVLink", enLinkType.Float2, enBezierType.Left, enLinkOpType.End, false);
            CollectLinkPinInfo(smParam, "RGBALink", enLinkType.Float4, enBezierType.Right, enLinkOpType.Start, true);
            CollectLinkPinInfo(smParam, "RGBLink", enLinkType.Float3, enBezierType.Right, enLinkOpType.Start, true);
            CollectLinkPinInfo(smParam, "RLink", enLinkType.Float1, enBezierType.Right, enLinkOpType.Start, true);
            CollectLinkPinInfo(smParam, "GLink", enLinkType.Float1, enBezierType.Right, enLinkOpType.Start, true);
            CollectLinkPinInfo(smParam, "BLink", enLinkType.Float1, enBezierType.Right, enLinkOpType.Start, true);
            CollectLinkPinInfo(smParam, "ALink", enLinkType.Float1, enBezierType.Right, enLinkOpType.Start, true);
        }
        public override BaseNodeControl Duplicate(DuplicateParam param)
        {
            var copyedNode = base.Duplicate(param) as Tex2D;
            copyedNode.TexturePath = TexturePath;
            return copyedNode;
        }
        public override void Save(EngineNS.IO.XndNode xndNode, bool newGuid)
        {
            var att = xndNode.AddAttrib("tex2Ddata");
            att.Version = 0;
            att.BeginWrite();
            att.Write(TexturePath.Name);
            att.EndWrite();
            base.Save(xndNode, newGuid);
        }

        public override async System.Threading.Tasks.Task Load(EngineNS.IO.XndNode xndNode)
        {
            var att = xndNode.FindAttrib("tex2Ddata");
            if(att != null)
            {
                switch(att.Version)
                {
                    case 0:
                        {
                            att.BeginRead();
                            string name;
                            att.Read(out name);
                            TexturePath = EngineNS.RName.GetRName(name);
                            att.EndRead();
                        }
                        break;
                }
            }
            await base.Load(xndNode);
        }

        void TextureLink_OnAddLinkInfo(LinkInfo linkInfo)
        {
            if (!linkInfo.m_linkFromObjectInfo.mIsLoadingLinks && !linkInfo.m_linkToObjectInfo.mIsLoadingLinks)
            {
                TextureControl tCtrl = linkInfo.m_linkFromObjectInfo.HostNodeControl as TextureControl;
                if(tCtrl != null)
                    TexturePath = tCtrl.TextureRName;
            }
        }

        void UVLink_OnAddLinkInfo(LinkInfo linkInfo)
        {
            // todo 增加UV的移动和动画效果
        }

        private string GetVarName()
        {
            return "Text2D_" + EngineNS.Editor.Assist.GetValuedGUIDString(Id);
        }

        public override void GCode_GenerateCode(ref string strDefinitionSegment, ref string strSegment, int nLayer, CodeGenerateSystem.Base.LinkPinControl element, CodeGenerateSystem.Base.GenerateCodeContext_Method context)
        {
            var strValueIdt = "float4 " + GetVarName() + " = float4(0,0,0,0);\r\n";
            if (!strDefinitionSegment.Contains(strValueIdt))
                strDefinitionSegment += "    " + strValueIdt;

            var strTab = GCode_GetTabString(nLayer);

            string uvName = "input.vUV";
            if (UVLink.HasLink)
            {
                UVLink.GetLinkedObject(0, true).GCode_GenerateCode(ref strDefinitionSegment, ref strSegment, nLayer, element, context);
                uvName = UVLink.GetLinkedObject(0, true).GCode_GetValueName(UVLink.GetLinkedPinControl(0, true), context);
            }

            if (TextureLink.HasLink)
            {
                TextureLink.GetLinkedObject(0, true).GCode_GenerateCode(ref strDefinitionSegment, ref strSegment, nLayer, element, context);
                var ctrl = TextureLink.GetLinkedObject(0, true);
                string texSampName = "";
                string texName = "";
                if (ctrl is TextureControl)
                {
                    texSampName = ((TextureControl)ctrl).GetTextureSampName();
                    texName = ((TextureControl)ctrl).GetTextureName();
                }
                else if (ctrl is ShaderAutoData)
                {
                    texSampName = ((ShaderAutoData)ctrl).GCode_GetValueName(null, context);
                    texName = texSampName;
                }
                var assignStr = strTab + GetVarName() + " = vise_tex2D(" + texName + ", " + texSampName + ", " + uvName + ");\r\n";
                // 这里先不做判断，连线中有if的情况下会导致问题
                //if (!strSegment.Contains(assignStr))
                if(!Program.IsSegmentContainString(strSegment.Length - 1, strSegment, assignStr))
                    strSegment += assignStr;
            }
        }

        public override string GCode_GetValueName(CodeGenerateSystem.Base.LinkPinControl element, CodeGenerateSystem.Base.GenerateCodeContext_Method context)
        {
            if(element == RGBALink)
            {
                return GetVarName();
            }
            else if(element == RGBLink)
            {
                return GetVarName() + ".xyz";
            }
            else if(element == RLink)
            {
                return GetVarName() + ".x";
            }
            else if(element == GLink)
            {
                return GetVarName() + ".y";
            }
            else if(element == BLink)
            {
                return GetVarName() + ".z";
            }
            else if(element == ALink)
            {
                return GetVarName() + ".w";
            }

            return base.GCode_GetValueName(element, context);
        }

        public override string GCode_GetTypeString(CodeGenerateSystem.Base.LinkPinControl element, CodeGenerateSystem.Base.GenerateCodeContext_Method context)
        {
            if (element == null || element == RGBALink)
                return "float4";
            else if (element == RGBLink)
                return "float3";
            else if (element == RLink ||
                     element == GLink ||
                     element == BLink ||
                     element == ALink)
                return "float";

            return base.GCode_GetTypeString(element, context);
        }
    }
}
