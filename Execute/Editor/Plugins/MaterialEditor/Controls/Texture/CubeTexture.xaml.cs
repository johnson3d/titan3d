using CodeGenerateSystem.Base;
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

namespace MaterialEditor.Controls.Texture
{
    [EngineNS.Rtti.MetaClass]
    public class CubeTextureConstructionParams : NodeControlConstructionBase { }
    /// <summary>
    /// CubeTexture.xaml 的交互逻辑
    /// </summary>
// 没实现完，先不显示
//    [CodeGenerateSystem.ShowInNodeList("Params/CubeTexture", "Cube贴图")]
    [CodeGenerateSystem.CustomConstructionParams(typeof(CubeTextureConstructionParams))]
    public partial class CubeTexture : BaseNodeControl_ShaderVar, MaterialStreamRequire
    {
        EngineNS.RName mTexturePath;
        public EngineNS.RName TexturePath
        {
            get { return mTexturePath; }
            set
            {
                mTexturePath = value;

                if (mTexturePath != null)
                {
                    //string strAbsPath = EngineNS.CEngine.Instance.FileManager.Root + mTexturePath;

                    //strAbsPath = strAbsPath.Replace("/\\", "\\");
                    Action action = async () =>
                    {
                        //var imgs = await EditorCommon.ImageInit.GetImage(strAbsPath);
                        var imgs = await EditorCommon.ImageInit.GetImage(mTexturePath.Address);
                        Image_Texture.Source = imgs[0];
                    };
                    action();

                    ShaderVarInfo.SetValueStr(mTexturePath);
                }
                else
                {
                    ShaderVarInfo.SetValueStr(EngineNS.RName.EmptyName);
                }

                IsDirty = true;
            }
        }

        public string GetStreamRequire()
        {
            if (UVLink_3D == null || !UVLink_3D.HasLink)
                return "LocalPos";

            return "";
        }

        public CubeTexture(CodeGenerateSystem.Base.ConstructionParams smParam)
            : base(smParam)
        {
            InitializeComponent();

            IsInConstantBuffer = false;
            IsGeneric = true;
            mDropAdorner = new EditorCommon.DragDrop.DropAdorner(Image_Texture);

            ShaderVarInfo.EditorType = "Texture";
            //ShaderVarInfo.VarName = GetTextureName();

            TexturePath = EngineNS.CEngine.Instance.GameEditorInstance.Desc.DefaultTextureName;

            AddLinkPinInfo("TextureLink", TextureLink, TextureLink.BackBrush);
            AddLinkPinInfo("UVLink_3D", UVLink_3D, null);
            AddLinkPinInfo("RGBALink", RGBALink, RGBALink.BackBrush);
            AddLinkPinInfo("RGBLink", RGBLink, RGBLink.BackBrush);
            AddLinkPinInfo("RLink", RLink, RLink.BackBrush);
            AddLinkPinInfo("GLink", GLink, GLink.BackBrush);
            AddLinkPinInfo("BLink", BLink, BLink.BackBrush);
            AddLinkPinInfo("ALink", ALink, ALink.BackBrush);

            InitializeShaderVarInfo();
        }

        public static void InitNodePinTypes(CodeGenerateSystem.Base.ConstructionParams smParam)
        {
            CollectLinkPinInfo(smParam, "TextureLink", enLinkType.Texture, enBezierType.Right, enLinkOpType.Start, true);
            CollectLinkPinInfo(smParam, "UVLink_3D", enLinkType.Float3 | enLinkType.Float4, enBezierType.Left, enLinkOpType.End, false);
            CollectLinkPinInfo(smParam, "RGBALink", enLinkType.Float4, enBezierType.Right, enLinkOpType.Start, true);
            CollectLinkPinInfo(smParam, "RGBLink", enLinkType.Float3, enBezierType.Right, enLinkOpType.Start, true);
            CollectLinkPinInfo(smParam, "RLink", enLinkType.Float1, enBezierType.Right, enLinkOpType.Start, true);
            CollectLinkPinInfo(smParam, "GLink", enLinkType.Float1, enBezierType.Right, enLinkOpType.Start, true);
            CollectLinkPinInfo(smParam, "BLink", enLinkType.Float1, enBezierType.Right, enLinkOpType.Start, true);
            CollectLinkPinInfo(smParam, "ALink", enLinkType.Float1, enBezierType.Right, enLinkOpType.Start, true);
        }

        protected override void InitializeShaderVarInfo()
        {
            ShaderVarInfo.Initialize(GetTextureName(), TexturePath);
        }
        public override BaseNodeControl Duplicate(DuplicateParam param)
        {
            var copyedNode = base.Duplicate(param) as CubeTexture;
            copyedNode.TexturePath = TexturePath;
            return copyedNode;
        }
        public override void Save(EngineNS.IO.XndNode xndNode, bool newGuid)
        {
            var att = xndNode.AddAttrib("cubeTexData");
            att.Version = 0;
            att.BeginWrite();
            att.Write(TexturePath.Name);
            att.EndWrite();
            base.Save(xndNode, newGuid);
        }

        public override async System.Threading.Tasks.Task Load(EngineNS.IO.XndNode xndNode)
        {
            var att = xndNode.FindAttrib("cubeTexData");
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

            InitializeShaderVarInfo();
        }

        public override string GetValueDefine()
        {
            string retStr = "";

            retStr += "TextureCube " + GetTextureName() + ";\r\n";
            retStr += "samplerCUBE " + GetTextureSampName() + " = sampler_state\r\n";
            //retStr += "{\r\n";
            //retStr += "\tTextureCube = <" + GetTextureName() + ">;\r\n";
            //retStr += "\tMipFilter = " + ((ComboBoxItem)MipFilterComboBox.SelectedItem).Content + ";\r\n";
            //retStr += "\tMinFilter = " + ((ComboBoxItem)MinFilterComboBox.SelectedItem).Content + ";\r\n";
            //retStr += "\tMagFilter = " + ((ComboBoxItem)MagFilterComboBox.SelectedItem).Content + ";\r\n";
            //retStr += "\tAddressU = " + ((ComboBoxItem)AddressUComboBox.SelectedItem).Content + ";\r\n";
            //retStr += "\tAddressV = " + ((ComboBoxItem)AddressVComboBox.SelectedItem).Content + ";\r\n";
            //retStr += "\tSRGBTexture = " + ((ComboBoxItem)SRGBTextureComboBox.SelectedItem).Content + ";\r\n";
            //retStr += "};\r\n\r\n";

            return retStr;
        }
        public string GetTextureName()
        {
            return Program.GetValidNodeName(this);
        }

        public string GetTextureSampName()
        {
            return "Samp_" + GetTextureName();
        }
        public override string GCode_GetTypeString(CodeGenerateSystem.Base.LinkPinControl element, CodeGenerateSystem.Base.GenerateCodeContext_Method context)
        {
            if (element == TextureLink)
                return "sampler2D";
            else if (element == RGBALink)
                return "float4";
            else if (element == RGBLink)
                return "float3";
            else if (element == RLink)
                return "float1";
            else if (element == GLink)
                return "float1";
            else if (element == BLink)
                return "float1";
            else if (element == ALink)
                return "float1";

            return base.GCode_GetTypeString(element, context);
        }

        public override string GCode_GetValueName(CodeGenerateSystem.Base.LinkPinControl element, CodeGenerateSystem.Base.GenerateCodeContext_Method context)
        {
            if (element == TextureLink)
                return GetTextureSampName();
            else if (element == RGBALink)
            {
                return GetTextureName() + "_CUBE";
            }
            else if (element == RGBLink)
            {
                return GetTextureName() + "_CUBE.xyz";
            }
            else if (element == RLink)
            {
                return GetTextureName() + "_CUBE.x";
            }
            else if (element == GLink)
            {
                return GetTextureName() + "_CUBE.y";
            }
            else if (element == BLink)
            {
                return GetTextureName() + "_CUBE.z";
            }
            else if (element == ALink)
            {
                return GetTextureName() + "_CUBE.w";
            }

            return base.GCode_GetValueName(element, context);
        }

        public override void GCode_GenerateCode(ref string strDefinitionSegment, ref string strSegment, int nLayer, CodeGenerateSystem.Base.LinkPinControl element, CodeGenerateSystem.Base.GenerateCodeContext_Method context)
        {
            var strValueIdt = "float4 " + GetTextureName() + "_CUBE = float4(0,0,0,0);\r\n";
            if (!strDefinitionSegment.Contains(strValueIdt))
                strDefinitionSegment += "    " + strValueIdt;

            var strTab = GCode_GetTabString(nLayer);

            string uvName = "mtl.mLocalPos.xyz";
            if (UVLink_3D.HasLink)
            {
                UVLink_3D.GetLinkedObject(0, true).GCode_GenerateCode(ref strDefinitionSegment, ref strSegment, nLayer, element, context);
                uvName = UVLink_3D.GetLinkedObject(0, true).GCode_GetValueName(UVLink_3D.GetLinkedPinControl(0, true), context) + ".xyz";
            }
            var assignStr = strTab + GetTextureName() + "_CUBE = vise_texCUBE(" + GetTextureName() + "," + GetTextureSampName() + "," + uvName + ");\r\n";
            // 这里先不做判断，连线中有if的情况下会导致问题
            //if (!strSegment.Contains(assignStr))
            if (!Program.IsSegmentContainString(strSegment.Length - 1, strSegment, assignStr))
                strSegment += assignStr;
        }

        private void Button_SetTexturePath_Click(object sender, RoutedEventArgs e)
        {
            var data = EditorCommon.PluginAssist.PropertyGridAssist.GetSelectedObjectData("Texture");
            if (data == null)
                return;

            if (data.Length <= 0)
                return;

            TexturePath = data[0] as EngineNS.RName;

            // 向连接到本节点的其他节点发送贴图设置的信息以便其他节点更新显示贴图
            var linkObj = TextureLink.GetLinkedObject(0, false);
            if (linkObj is Texture.Tex2D)
            {
                ((Texture.Tex2D)linkObj).TexturePath = TexturePath;
            }
        }

        private void ComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            IsDirty = true;
        }

        private void Button_SearchTexture_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var noUse = EditorCommon.Controls.ResourceBrowser.BrowserControl.ShowResource(TexturePath);
        }

        #region DragDrop

        EditorCommon.DragDrop.DropAdorner mDropAdorner;
        enum enDropResult
        {
            Denial_UnknowFormat,
            Denial_NoDragAbleObject,
            Allow,
        }
        // 是否允许拖放
        enDropResult AllowResourceItemDrop(System.Windows.DragEventArgs e)
        {
            var formats = e.Data.GetFormats();
            if (formats == null || formats.Length == 0)
                return enDropResult.Denial_UnknowFormat;

            var datas = e.Data.GetData(formats[0]) as EditorCommon.DragDrop.IDragAbleObject[];
            if (datas == null)
                return enDropResult.Denial_NoDragAbleObject;

            bool containMeshSource = false;
            foreach (var data in datas)
            {
                var resInfo = data as EditorCommon.Resources.ResourceInfo;
                if (resInfo.ResourceType == EngineNS.Editor.Editor_RNameTypeAttribute.Texture)
                {
                    containMeshSource = true;
                    break;
                }
            }

            if (!containMeshSource)
                return enDropResult.Denial_NoDragAbleObject;

            return enDropResult.Allow;
        }
        private void Image_Texture_DragEnter(object sender, DragEventArgs e)
        {
            if (EditorCommon.DragDrop.DragDropManager.Instance.DragType.Equals("ResourceItem"))
            {
                var element = sender as FrameworkElement;
                if (element == null)
                    return;

                e.Handled = true;
                mDropAdorner.IsAllowDrop = false;

                switch (AllowResourceItemDrop(e))
                {
                    case enDropResult.Allow:
                        {
                            EditorCommon.DragDrop.DragDropManager.Instance.InfoString = "设置贴图资源";

                            mDropAdorner.IsAllowDrop = true;
                            var pos = e.GetPosition(element);
                            if (pos.X > 0 && pos.X < element.ActualWidth &&
                               pos.Y > 0 && pos.Y < element.ActualHeight)
                            {
                                var layer = AdornerLayer.GetAdornerLayer(element);
                                layer.Add(mDropAdorner);
                            }
                        }
                        break;

                    case enDropResult.Denial_NoDragAbleObject:
                    case enDropResult.Denial_UnknowFormat:
                        {
                            EditorCommon.DragDrop.DragDropManager.Instance.InfoString = "拖动内容不是贴图资源";

                            mDropAdorner.IsAllowDrop = false;
                            var pos = e.GetPosition(element);
                            if (pos.X > 0 && pos.X < element.ActualWidth &&
                               pos.Y > 0 && pos.Y < element.ActualHeight)
                            {
                                var layer = AdornerLayer.GetAdornerLayer(element);
                                layer.Add(mDropAdorner);
                            }
                        }
                        break;
                }
            }
        }

        private void Image_Texture_DragLeave(object sender, DragEventArgs e)
        {
            var element = sender as FrameworkElement;
            if (element == null)
                return;

            if (EditorCommon.DragDrop.DragDropManager.Instance.DragType.Equals("ResourceItem"))
            {
                e.Handled = true;
                EditorCommon.DragDrop.DragDropManager.Instance.InfoString = "";
                var layer = AdornerLayer.GetAdornerLayer(element);
                layer.Remove(mDropAdorner);
            }
        }

        private void Image_Texture_DragOver(object sender, DragEventArgs e)
        {
            if (EditorCommon.DragDrop.DragDropManager.Instance.DragType.Equals("ResourceItem"))
            {
                e.Handled = true;
                if (AllowResourceItemDrop(e) == enDropResult.Allow)
                {
                    e.Effects = DragDropEffects.Move;
                }
                else
                {
                    e.Effects = DragDropEffects.None;
                }
            }
        }

        private void Image_Texture_Drop(object sender, DragEventArgs e)
        {
            var element = sender as FrameworkElement;
            if (element == null)
                return;

            if (EditorCommon.DragDrop.DragDropManager.Instance.DragType.Equals("ResourceItem"))
            {
                e.Handled = true;
                var layer = AdornerLayer.GetAdornerLayer(element);
                layer.Remove(mDropAdorner);

                if (AllowResourceItemDrop(e) == enDropResult.Allow)
                {
                    var formats = e.Data.GetFormats();
                    var datas = e.Data.GetData(formats[0]) as EditorCommon.DragDrop.IDragAbleObject[];
                    foreach (var data in datas)
                    {
                        var resInfo = data as EditorCommon.Resources.ResourceInfo;
                        if (resInfo == null)
                            continue;

                        TexturePath = resInfo.ResourceName;
                        break;
                    }
                }
            }
        }

        #endregion
    }
}
