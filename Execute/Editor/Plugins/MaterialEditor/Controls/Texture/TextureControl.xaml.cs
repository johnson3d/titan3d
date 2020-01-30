using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using CodeGenerateSystem.Base;
using System.Windows.Documents;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.ComponentModel;

namespace MaterialEditor.Controls
{
    public interface ITextureSampler
    {
        EngineNS.CSamplerStateDesc SamplerStateDesc { get; set; }
    }

    [EngineNS.Rtti.MetaClass]
    public class TextureControlConstructionParams : NodeControlConstructionBase { }
    /// <summary>
    /// TextureControl.xaml 的交互逻辑
    /// </summary>
    [CodeGenerateSystem.ShowInNodeList("Params/TextureSample", "贴图")]
    [EngineNS.Editor.Editor_DoNotShowBaseClassProperties]
    [CodeGenerateSystem.CustomConstructionParams(typeof(TextureControlConstructionParams))]
    public partial class TextureControl : BaseNodeControl_ShaderVar, INotifyPropertyChanged, ITextureSampler
    {
        #region INotifyPropertyChangedMembers
        public bool IgnoreOnPropertyChangedAction = false;
        public event PropertyChangedEventHandler _PropertyChanged;
        public Action<string, object, object> OnPropertyChangedAction;
        protected void OnPropertyChanged(string propertyName, object newValue, object oldValue)
        {
            if (HostNodesContainer == null)
                return;
            EngineNS.Editor.PropertyChangedUtility.PropertyChangedProcess(this, propertyName, _PropertyChanged);
            if (!IgnoreOnPropertyChangedAction)
            {
                var newVal = newValue;
                var oldVal = oldValue;
                if(HostNodesContainer.HostControl != null)
                {
                    EditorCommon.UndoRedo.UndoRedoManager.Instance.AddCommand(HostNodesContainer.HostControl.UndoRedoKey, null,
                                                        (obj) =>
                                                        {
                                                            IgnoreOnPropertyChangedAction = true;
                                                            var pro = GetType().GetProperty(propertyName);
                                                            if (pro != null)
                                                            {
                                                                pro.SetValue(this, newVal);
                                                            }
                                                            OnPropertyChangedAction?.Invoke(propertyName, newVal, oldVal);
                                                            IgnoreOnPropertyChangedAction = false;
                                                        }, null,
                                                        (obj) =>
                                                        {
                                                            IgnoreOnPropertyChangedAction = true;
                                                            var pro = GetType().GetProperty(propertyName);
                                                            if (pro != null)
                                                            {
                                                                pro.SetValue(this, oldVal);
                                                            }
                                                            OnPropertyChangedAction?.Invoke(propertyName, newVal, oldVal);
                                                            IgnoreOnPropertyChangedAction = false;
                                                        }, $"Set {"Texture"}.{propertyName}");
                }
            }
            else if (!IgnoreOnPropertyChangedAction)
                OnPropertyChangedAction?.Invoke(propertyName, newValue, oldValue);
        }
        #endregion

        // 临时类，用于选中后显示参数属性
        public override object GetShowPropertyObject()
        {
            return this;
        }

        [DisplayName("Name")]
        public string VarName
        {
            get { return NodeName; }
            set
            {
                NodeName = value;
                OnPropertyChanged("VarName");
            }
        }

        EngineNS.RName mTextureRName;
        [EngineNS.Editor.Editor_RNameTypeAttribute("Texture")]
        [DisplayName("Texture")]
        public EngineNS.RName TextureRName
        {
            get => mTextureRName;
            set
            {
                var oldVal = mTextureRName;
                mTextureRName = value;
                if (value != null)
                {
                    Action action = async () =>
                    {
                        var imgs = await EditorCommon.ImageInit.GetImage(value.Address);
                        if (imgs != null)
                        {
                            Image_Texture.Source = imgs[0];
                        }
                    };
                    action();

                    ShaderVarInfo.SetValueStr(value);
                }
                else
                {
                    ShaderVarInfo.SetValueStr(EngineNS.RName.EmptyName);
                }

                // 向连接到本节点的其他节点发送贴图设置的信息以便其他节点更新显示贴图
                if (TextureLink != null)
                {
                    var linkObj = TextureLink.GetLinkedObject(0, false);
                    if (linkObj is Texture.Tex2D)
                    {
                        ((Texture.Tex2D)linkObj).TexturePath = value;
                    }
                }

                IsDirty = true;
                OnPropertyChanged("TextureRName", value, oldVal);
            }
        }

        public EngineNS.CSamplerStateDesc mSamplerStateDesc = new EngineNS.CSamplerStateDesc();
        public EngineNS.CSamplerStateDesc SamplerStateDesc
        {
            get => mSamplerStateDesc;
            set
            {
                var oldValue = mSamplerStateDesc;
                mSamplerStateDesc = value;
                OnPropertyChanged("SamplerStateDesc", value, oldValue);
                IsDirty = true;
            }
        }

        // 用于CreateInstance创建
        public TextureControl()
            : base(null)
        {

        }
        public TextureControl(CodeGenerateSystem.Base.ConstructionParams smParam)
            : base(smParam)
        {
            InitializeComponent();

            IsInConstantBuffer = false;
            IsGeneric = true;
            mDropAdorner = new EditorCommon.DragDrop.DropAdorner(Image_Texture);

            ShaderVarInfo.EditorType = "Texture";
            //ShaderVarInfo.VarName = GetTextureName();

            TextureRName = EngineNS.CEngine.Instance.GameEditorInstance.Desc.DefaultTextureName;

            AddLinkPinInfo("TextureLink", TextureLink, TextureLink.BackBrush);
            AddLinkPinInfo("UVLink_2D", UVLink_2D, null);
            AddLinkPinInfo("RGBALink", RGBALink, RGBALink.BackBrush);
            AddLinkPinInfo("RGBLink", RGBLink, RGBLink.BackBrush);
            AddLinkPinInfo("RLink", RLink, RLink.BackBrush);
            AddLinkPinInfo("GLink", GLink, GLink.BackBrush);
            AddLinkPinInfo("BLink", BLink, BLink.BackBrush);
            AddLinkPinInfo("ALink", ALink, ALink.BackBrush);

            mSamplerStateDesc.SetDefault();
            InitializeShaderVarInfo();
        }

        public static void InitNodePinTypes(CodeGenerateSystem.Base.ConstructionParams smParam)
        {
            CollectLinkPinInfo(smParam, "TextureLink", enLinkType.Texture, enBezierType.Right, enLinkOpType.Start, true);
            CollectLinkPinInfo(smParam, "UVLink_2D", enLinkType.Float2 | enLinkType.Float3 | enLinkType.Float4, enBezierType.Left, enLinkOpType.End, false);
            CollectLinkPinInfo(smParam, "RGBALink", enLinkType.Float4, enBezierType.Right, enLinkOpType.Start, true);
            CollectLinkPinInfo(smParam, "RGBLink", enLinkType.Float3, enBezierType.Right, enLinkOpType.Start, true);
            CollectLinkPinInfo(smParam, "RLink", enLinkType.Float1, enBezierType.Right, enLinkOpType.Start, true);
            CollectLinkPinInfo(smParam, "GLink", enLinkType.Float1, enBezierType.Right, enLinkOpType.Start, true);
            CollectLinkPinInfo(smParam, "BLink", enLinkType.Float1, enBezierType.Right, enLinkOpType.Start, true);
            CollectLinkPinInfo(smParam, "ALink", enLinkType.Float1, enBezierType.Right, enLinkOpType.Start, true);
        }

        protected override void InitializeShaderVarInfo()
        {
            ShaderVarInfo.Initialize(GetTextureName(), TextureRName);
        }
        public override BaseNodeControl Duplicate(DuplicateParam param)
        {
            var copyedNode = base.Duplicate(param) as TextureControl;
            copyedNode.TextureRName = TextureRName;
            copyedNode.SamplerStateDesc = mSamplerStateDesc;
            return copyedNode;
        }
        public override void Save(EngineNS.IO.XndNode xndNode, bool newGuid)
        {
            var att = xndNode.AddAttrib("textureCtrlData");
            att.Version = 3;
            att.BeginWrite();
            att.Write(TextureRName);
            unsafe
            {
                var len = sizeof(EngineNS.CSamplerStateDesc);
                att.Write(len);
                fixed (EngineNS.CSamplerStateDesc* ptr = &mSamplerStateDesc)
                {
                    att.Write((IntPtr)ptr, len);
                }
            }
            att.EndWrite();
            base.Save(xndNode, newGuid);
        }

        public override async System.Threading.Tasks.Task Load(EngineNS.IO.XndNode xndNode)
        {
            var att = xndNode.FindAttrib("textureCtrlData");
            if(att != null)
            {
                switch(att.Version)
                {
                    case 0:
                        {
                            att.BeginRead();
                            string name;
                            att.Read(out name);
                            TextureRName = EngineNS.RName.GetRName(name);
                            att.EndRead();
                        }
                        break;
                    case 1:
                        {
                            att.BeginRead();
                            string name;
                            att.Read(out name);
                            TextureRName = EngineNS.RName.GetRName(name);
                            mSamplerStateDesc = (EngineNS.CSamplerStateDesc)att.ReadMetaObject(mSamplerStateDesc);
                            att.EndRead();
                        }
                        break;
                    case 2:
                        {
                            att.BeginRead();
                            EngineNS.RName name = EngineNS.RName.EmptyName;
                            att.Read(out name);
                            TextureRName = name;
                            mSamplerStateDesc = (EngineNS.CSamplerStateDesc)att.ReadMetaObject(mSamplerStateDesc);
                            att.EndRead();
                        }
                        break;
                    case 3:
                        {
                            att.BeginRead();
                            EngineNS.RName name = EngineNS.RName.EmptyName;
                            att.Read(out name);
                            TextureRName = name;
                            unsafe
                            {
                                int len = 0;
                                att.Read(out len);
                                fixed (EngineNS.CSamplerStateDesc* ptr = &mSamplerStateDesc)
                                {
                                    att.Read((IntPtr)ptr, len);
                                    SamplerStateDesc = mSamplerStateDesc;
                                }
                            }
                            att.EndRead();
                        }
                        break;
                }
            }

            await base.Load(xndNode);
            IsDirty = false;

            InitializeShaderVarInfo();
        }

        //private void MenuItem_LoadTexture_Click(object sender, RoutedEventArgs e)
        //{
        //    System.Windows.Forms.OpenFileDialog ofd = new System.Windows.Forms.OpenFileDialog();
        //    if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
        //    {
        //        var strFileName = EngineNS.CEngine.Instance.FileManager._GetRelativePathFromAbsPath(ofd.FileName);
        //        TexturePath = strFileName;

        //        // 向连接到本节点的其他节点发送贴图设置的信息以便其他节点更新显示贴图
        //        var linkOI = GetLinkPinInfo(TextureLink);
        //        var linkObj = linkOI.GetLinkObject(0, false);
        //        if (linkObj is Texture.Tex2D)
        //        {
        //            ((Texture.Tex2D)linkObj).TexturePath = TexturePath;
        //        }
        //    }
        //}

        /*/////////////////////////////////////////////////////////////////////////
        D3DTEXF_NONE            = 0,    // filtering disabled (valid for mip filter only)
        D3DTEXF_POINT           = 1,    // nearest
        D3DTEXF_LINEAR          = 2,    // linear interpolation
        D3DTEXF_ANISOTROPIC     = 3,    // anisotropic
        D3DTEXF_PYRAMIDALQUAD   = 6,    // 4-sample tent
        D3DTEXF_GAUSSIANQUAD    = 7,    // 4-sample gaussian
        /////////////////////////////////////////////////////////////////////////*/

        public override string GetValueDefine()
        {
            string retStr = "";

            retStr += "Texture2D " + GetTextureName() + ";\r\n";
            retStr += "SamplerState " + GetTextureSampName() + ";\r\n";

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
            if (element == TextureLink || element == null)
                return "sampler";
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
            if(element == TextureLink || element == null)
                return GetTextureSampName();
            else if (element == RGBALink)
            {
                return GetTextureName() + "_2D";
            }
            else if(element == RGBLink)
            {
                return GetTextureName() + "_2D.xyz";
            }
            else if(element == RLink)
            {
                return GetTextureName() + "_2D.x";
            }
            else if (element == GLink)
            {
                return GetTextureName() + "_2D.y";
            }
            else if (element == BLink)
            {
                return GetTextureName() + "_2D.z";
            }
            else if (element == ALink)
            {
                return GetTextureName() + "_2D.w";
            }

            return base.GCode_GetValueName(element, context);
        }
        
        public override void GCode_GenerateCode(ref string strDefinitionSegment, ref string strSegment, int nLayer, CodeGenerateSystem.Base.LinkPinControl element, CodeGenerateSystem.Base.GenerateCodeContext_Method context)
        {
            var strValueIdt = "float4 " + GetTextureName() + "_2D = float4(0,0,0,0);\r\n";
            if (!strDefinitionSegment.Contains(strValueIdt))
                strDefinitionSegment += "    " + strValueIdt;

            var strTab = GCode_GetTabString(nLayer);

            string uvName = "input.vUV";
            if (UVLink_2D.HasLink)
            {
                UVLink_2D.GetLinkedObject(0, true).GCode_GenerateCode(ref strDefinitionSegment, ref strSegment, nLayer, element, context);
                uvName = UVLink_2D.GetLinkedObject(0, true).GCode_GetValueName(UVLink_2D.GetLinkedPinControl(0, true), context) + ".xy";
            }
            var assignStr = $"{strTab}{GetTextureName()}_2D = {GetTextureName()}.Sample({GetTextureSampName()},{uvName});\r\n";
            // 这里先不做判断，连线中有if的情况下会导致问题
            //if (!strSegment.Contains(assignStr))
            if (!Program.IsSegmentContainString(strSegment.Length - 1, strSegment, assignStr))
                strSegment += assignStr;
           
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
            if(EditorCommon.DragDrop.DragDropManager.Instance.DragType.Equals("ResourceItem"))
            {
                var element = sender as FrameworkElement;
                if (element == null)
                    return;

                e.Handled = true;
                mDropAdorner.IsAllowDrop = false;

                switch(AllowResourceItemDrop(e))
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

                        TextureRName = resInfo.ResourceName;
                        break;
                    }
                }
            }
        }

        #endregion
    }
}
