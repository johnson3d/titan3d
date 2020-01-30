using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Runtime.InteropServices;
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
using DockControl;
using EditorCommon.Resources;
using EngineNS.IO;

namespace UVAnimEditor
{
    /// <summary>
    /// MainControl.xaml 的交互逻辑
    /// </summary>
    [EditorCommon.PluginAssist.EditorPlugin(PluginType = "UVAnimEditor")]
    [Guid("AB0096CF-9621-49D8-8EE9-ADA36F223809")]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public partial class MainControl : UserControl, EditorCommon.PluginAssist.IEditorPlugin
    {
        #region IEditorPlugin
        public string PluginName => "UI图元编辑器";

        public string Version => "1.0.0";

        public string Title
        {
            get { return (string)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }
        public static readonly DependencyProperty TitleProperty = DependencyProperty.Register("Title", typeof(string), typeof(MainControl), new FrameworkPropertyMetadata(null));

        public ImageSource Icon => new BitmapImage(new System.Uri("pack://application:,,,/ResourceLibrary;component/Icons/Icons/AssetIcons/PaperSprite_64x.png", UriKind.Absolute));

        public Brush IconBrush
        {
            get { return (Brush)GetValue(IconBrushProperty); }
            set { SetValue(IconBrushProperty, value); }
        }
        public static readonly DependencyProperty IconBrushProperty = DependencyProperty.Register("IconBrush", typeof(Brush), typeof(MainControl), new FrameworkPropertyMetadata(null));

        public UIElement InstructionControl => new System.Windows.Controls.TextBlock()
        {
            Text = PluginName,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch
        };

        public bool IsShowing { get; set; }
        public bool IsActive { get; set; }

        public string KeyValue => PluginName;

        public int Index { get; set; }

        public string DockGroup => "";

        public bool? CanClose()
        {
            if(mCurrentResInfo.IsDirty)
            {
                var result = EditorCommon.MessageBox.Show("该贴图还有未保存的更改，是否保存后退出？\r\n(点否后会丢失所有未保存的更改)", EditorCommon.MessageBox.enMessageBoxButton.YesNoCancel);
                switch (result)
                {
                    case EditorCommon.MessageBox.enMessageBoxResult.Yes:
                        var noUse = Save();
                        return true;
                    case EditorCommon.MessageBox.enMessageBoxResult.No:
                        mCurrentResInfo.IsDirty = false;
                        return true;
                    case EditorCommon.MessageBox.enMessageBoxResult.Cancel:
                        return false;
                }
            }

            return true;
        }

        public void Closed()
        {
        }

        public void StartDrag()
        {
        }
        public void EndDrag()
        {
        }

        public void SaveElement(XmlNode node, XmlHolder holder)
        {
        }
        public IDockAbleControl LoadElement(XmlNode node)
        {
            return null;
        }

        public bool OnActive()
        {
            return true;
        }

        public bool OnDeactive()
        {
            return true;
        }

        public static readonly string KeyFrameWord = "关键帧";
        UVAnimResourceInfo mCurrentResInfo;
        public async Task SetObjectToEdit(ResourceEditorContext context)
        {
            await Viewport.WaitInitComplated();

            mCurrentResInfo = context.ResInfo as UVAnimResourceInfo;

            var rc = EngineNS.CEngine.Instance.RenderContext;
            mUVAnim = await EngineNS.CEngine.Instance.UVAnimManager.GetUVAnimAsync(rc, mCurrentResInfo.ResourceName);
            mUVAnim.PropertyChanged += UVAnim_PropertyChanged;
            await ImagePanel.SetUVAnim(mUVAnim);

            PG_UVAnim.Instance = mUVAnim;

            ListBox_Frames.Items.Clear();
            for(int i=0; i<mUVAnim.Frames.Count; i++)
            {
                ListBox_Frames.Items.Add(KeyFrameWord + i);

                mUVAnim.Frames[i].PropertyChanged += Frame_PropertyChanged;
            }
            if (ListBox_Frames.Items.Count > 0)
                ListBox_Frames.SelectedIndex = 0;
            else
                ListBox_Frames.SelectedIndex = -1;

            mImage2D = await EngineNS.Graphics.Mesh.CGfxImage2D.CreateImage2D(rc, mUVAnim.MaterialInstanceRName, 0, 0, 0, 1, 1);
            var texture = EngineNS.CEngine.Instance.TextureManager.GetShaderRView(rc, mUVAnim.TextureRName);
            mImage2D.SetTexture("texture", texture);
            mImage2D.RenderMatrix = EngineNS.Matrix.Scaling(Viewport.GetViewPortWidth(), Viewport.GetViewPortHeight(), 1);
            //var list = new EngineNS.Support.NativeList<EngineNS.Vector2>();
            //list.Add(new EngineNS.Vector2(0.5f, 0.5f));
            //list.Add(new EngineNS.Vector2(1, 0.5f));
            //list.Add(new EngineNS.Vector2(1, 1));
            //list.Add(new EngineNS.Vector2(0.5f, 1));
            //mImage2D.SetUV(list, rc, false);

            Viewport.RPolicy.OnDrawUI += (cmd, view) =>
            {
                var mtlmesh = mImage2D.Mesh.MtlMeshArray[0];
                var pass = mImage2D.GetPass();
                pass.ViewPort = view.Viewport;
                if (pass.RenderPipeline == null)
                {
                    var rplDesc = new EngineNS.CRenderPipelineDesc();
                    pass.RenderPipeline = rc.CreateRenderPipeline(rplDesc);
                }
                pass.RenderPipeline.RasterizerState = mtlmesh.MtlInst.CustomRasterizerState;
                pass.RenderPipeline.DepthStencilState = mtlmesh.MtlInst.CustomDepthStencilState;
                pass.RenderPipeline.BlendState = mtlmesh.MtlInst.CustomBlendState;
                //pass.ShaderSamplerBinder = mtlmesh.GetSamplerBinder(rc, pass.Effect.ShaderProgram);

                if (pass.Effect.PreUse())
                {
                    pass.BindCBuffer(pass.Effect.ShaderProgram, pass.Effect.CacheData.CBID_View, view.ScreenViewCB);
                    pass.ShadingEnv.BindResources(mImage2D.Mesh, pass);
                    cmd.PushPass(pass);
                }
            };
        }

        private void UVAnim_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (mUVAnim == null || mImage2D == null)
                return;
            var rc = EngineNS.CEngine.Instance.RenderContext;
            switch (e.PropertyName)
            {
                case "TextureRName":
                    {
                        if(mUVAnim.TextureRName != null && mUVAnim.TextureRName != EngineNS.RName.EmptyName)
                        {
                            var texture = EngineNS.CEngine.Instance.TextureManager.GetShaderRView(rc, mUVAnim.TextureRName);
                            mImage2D.SetTexture(mUVAnim.TextureShaderVarName, texture);
                        }
                    }
                    break;
                case "MaterialInstanceRName":
                    {
                        var noUse = mImage2D.SetMaterialInstance(rc, mUVAnim.MaterialInstanceRName);
                        if (mUVAnim.TextureRName != null && mUVAnim.TextureRName != EngineNS.RName.EmptyName)
                        {
                            var texture = EngineNS.CEngine.Instance.TextureManager.GetShaderRView(rc, mUVAnim.TextureRName);
                            mImage2D.SetTexture(mUVAnim.TextureShaderVarName, texture);
                        }
                    }
                    break;
            }
        }

        private void Frame_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            mForceUpdateFrame = true;
        }

        #endregion

        EngineNS.Graphics.Mesh.CGfxImage2D mImage2D;
        EngineNS.UISystem.UVAnim mUVAnim;

        public MainControl()
        {
            InitializeComponent();
            EditorCommon.Resources.ResourceInfoManager.Instance.RegResourceInfo(typeof(UVAnimResourceInfo));
            var template = TryFindResource("Scale9Setter") as DataTemplate;
            WPG.Program.RegisterDataTemplate("Scale9Setter", template);

            Viewport.AfterInitializedAction = InitViewPort;
            Viewport.SizeChanged += Viewport_SizeChanged;
        }

        private void Viewport_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (mImage2D != null)
                mImage2D.RenderMatrix = EngineNS.Matrix.Scaling(Viewport.GetViewPortWidth(), Viewport.GetViewPortHeight(), 1);
            mForceUpdateFrame = true;
        }

        EngineNS.Graphics.RenderPolicy.CGfxRP_EditorMobile mRP_EditorMobile;
        bool mViewPortInited = false;
        async Task InitViewPort(EditorCommon.ViewPort.ViewPortControl vpCtrl)
        {
            if (mViewPortInited)
                return;
            mViewPortInited = true;
            var rc = EngineNS.CEngine.Instance.RenderContext;
            mRP_EditorMobile = new EngineNS.Graphics.RenderPolicy.CGfxRP_EditorMobile();
            var width = (uint)vpCtrl.GetViewPortWidth();
            var height = (uint)vpCtrl.GetViewPortHeight();
            await mRP_EditorMobile.Init(rc, width, height, Viewport.Camera, vpCtrl.DrawHandle);
            vpCtrl.RPolicy = mRP_EditorMobile;

            mRP_EditorMobile.mHitProxy.mEnabled = false;

            vpCtrl.TickLogicEvent = Viewport_TickLogic;
        }
        bool mForceUpdateFrame = false;
        void Viewport_TickLogic(EditorCommon.ViewPort.ViewPortControl vpc)
        {
            var rc = EngineNS.CEngine.Instance.RenderContext;
            if (vpc.World != null)
            {
                vpc.World.CheckVisible(rc.ImmCommandList, vpc.Camera);
                vpc.World.Tick();
            }
            vpc.RPolicy.TickLogic(null, rc);

            var designRect = new EngineNS.RectangleF(0, 0, Viewport.GetViewPortWidth(), Viewport.GetViewPortHeight());
            var tempClipRect = designRect;

            if (mUVAnim != null && mImage2D != null)
            {
                mUVAnim.CheckAndAutoReferenceFromTemplateUVAnim();
                bool frameChanged;
                var frame = mUVAnim.GetUVFrame(EngineNS.Support.Time.GetTickCount(), out frameChanged);
                if(frameChanged || mForceUpdateFrame)
                {
                    using (var posData = EngineNS.Support.NativeListProxy<EngineNS.Vector3>.CreateNativeList())
                    using (var uvData = EngineNS.Support.NativeListProxy<EngineNS.Vector2>.CreateNativeList())
                    {
                        frame.UpdateVertexes(posData, ref designRect, ref tempClipRect);
                        frame.UpdateUVs(uvData, ref designRect, ref tempClipRect);
                        mImage2D.RenderMatrix = EngineNS.Matrix.Scaling(tempClipRect.Width, tempClipRect.Height, 1) * EngineNS.Matrix.Translate(tempClipRect.Left, tempClipRect.Top, 0.0f);
                        mImage2D.SetUV(uvData, rc.ImmCommandList);
                        mImage2D.SetVertexBuffer(posData, rc.ImmCommandList);

                        mForceUpdateFrame = false;
                    }   
                }
            }
        }

        private void ListBox_Frames_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (mUVAnim == null)
                return;

            if (ListBox_Frames.SelectedIndex < 0 || ListBox_Frames.SelectedIndex >= mUVAnim.Frames.Count)
                return;

            Property_Frame.Instance = mUVAnim.Frames[ListBox_Frames.SelectedIndex];
            ImagePanel.SelectedFrame(ListBox_Frames.SelectedIndex);
        }

        private async Task Save()
        {
            mUVAnim.Save2Xnd(mCurrentResInfo.ResourceName);

            // Reference
            mCurrentResInfo.ReferenceRNameList.Clear();
            mCurrentResInfo.ReferenceRNameList.Add(mUVAnim.TextureRName);
            mCurrentResInfo.ReferenceRNameList.Add(mUVAnim.MaterialInstanceRName);
            await mCurrentResInfo.Save(true);
        }

        private void IconTextBtn_Save_Click(object sender, RoutedEventArgs e)
        {
            var noUse = Save();
        }
        private void IconTextBtn_AutoGrid_Click(object sender, RoutedEventArgs e)
        {
            ImagePanel.AutoGridOperation();
            ListBox_Frames.Items.Clear();
            for(int i=0; i<mUVAnim.Frames.Count; i++)
            {
                ListBox_Frames.Items.Add(KeyFrameWord + i);
            }
            Property_Frame.Instance = null;
            if (ListBox_Frames.Items.Count > 0)
                ListBox_Frames.SelectedIndex = 0;
            else
                ListBox_Frames.SelectedIndex = 1;
        }
        private void IconTextBtn_SelectAll_Click(object sender, RoutedEventArgs e)
        {
            ImagePanel.SelectAllOperation();
        }

        private void Button_Add_Click(object sender, RoutedEventArgs e)
        {
            if (mUVAnim == null)
                return;

            mUVAnim.AddFrame();
            ListBox_Frames.Items.Add(KeyFrameWord + (mUVAnim.Frames.Count - 1));
            ImagePanel.AddFrame();
        }
        private void Button_Del_Click(object sender, RoutedEventArgs e)
        {
            if (mUVAnim == null)
                return;
            // 至少有一帧
            if (mUVAnim.Frames.Count <= 1)
                return;

            if (ListBox_Frames.SelectedIndex < 0 || ListBox_Frames.SelectedIndex >= mUVAnim.Frames.Count)
                return;

            ImagePanel.RemoveFrame(ListBox_Frames.SelectedIndex);
            mUVAnim.DelFrame(ListBox_Frames.SelectedIndex);
            ListBox_Frames.Items.RemoveAt(ListBox_Frames.SelectedIndex);

            for(int i=0; i<ListBox_Frames.Items.Count; i++)
            {
                ListBox_Frames.Items[i] = KeyFrameWord + i;
            }
        }
    }
}
