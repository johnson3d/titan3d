using System;
using System.Collections.Generic;
using System.ComponentModel;
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

namespace TextureSourceEditor
{
    /// <summary>
    /// MainControl.xaml 的交互逻辑
    /// </summary>
    [EditorCommon.PluginAssist.EditorPlugin(PluginType = "TextureSourceEditor")]
    [Guid("F1688591-C56E-4362-AB36-FD4C891C0CA3")]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public partial class MainControl : UserControl, EditorCommon.PluginAssist.IEditorPlugin
    {
        public string PluginName
        {
            get { return "贴图资源编辑器"; }
        }
        public string Version
        {
            get { return "1.0.0"; }
        }

        System.Windows.UIElement mInstructionControl = new System.Windows.Controls.TextBlock()
        {
            Text = "贴图资源编辑器",
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch
        };
        public System.Windows.UIElement InstructionControl
        {
            get { return mInstructionControl; }
        }

        public string Title
        {
            get { return (string)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }
        public static readonly DependencyProperty TitleProperty = DependencyProperty.Register("Title", typeof(string), typeof(MainControl), new FrameworkPropertyMetadata(null));

        public ImageSource Icon => new BitmapImage(new System.Uri("pack://application:,,,/ResourceLibrary;component/Icons/Icons/AssetIcons/Texture2D_64x.png", UriKind.Absolute));

        public Brush IconBrush
        {
            get { return (Brush)GetValue(IconBrushProperty); }
            set { SetValue(IconBrushProperty, value); }
        }
        public static readonly DependencyProperty IconBrushProperty = DependencyProperty.Register("IconBrush", typeof(Brush), typeof(MainControl), new FrameworkPropertyMetadata(null));

        public bool IsShowing { get; set; }
        public bool IsActive { get; set; }

        public string KeyValue => PluginName;

        public int Index { get; set; }

        public string DockGroup => "";

        public bool OnActive()
        {
            return true;
        }
        public bool OnDeactive()
        {
            return true;
        }

        public bool ShowRChannel
        {
            get { return (bool)GetValue(ShowRChannelProperty); }
            set { SetValue(ShowRChannelProperty, value); }
        }
        public static readonly DependencyProperty ShowRChannelProperty = DependencyProperty.Register("ShowRChannel", typeof(bool), typeof(MainControl), new FrameworkPropertyMetadata(true, new PropertyChangedCallback(OnShowChannelChangedCallback)));
        static void OnShowChannelChangedCallback(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var ctrl = sender as MainControl;
            ctrl.UpdateChannelShow();
        }
        public bool ShowGChannel
        {
            get { return (bool)GetValue(ShowGChannelProperty); }
            set { SetValue(ShowGChannelProperty, value); }
        }
        public static readonly DependencyProperty ShowGChannelProperty = DependencyProperty.Register("ShowGChannel", typeof(bool), typeof(MainControl), new FrameworkPropertyMetadata(true, new PropertyChangedCallback(OnShowChannelChangedCallback)));
        public bool IsEnableGChannel
        {
            get { return (bool)GetValue(IsEnableGChannelProperty); }
            set { SetValue(IsEnableGChannelProperty, value); }
        }
        public static readonly DependencyProperty IsEnableGChannelProperty = DependencyProperty.Register("IsEnableGChannel", typeof(bool), typeof(MainControl), new FrameworkPropertyMetadata(true));


        public bool ShowBChannel
        {
            get { return (bool)GetValue(ShowBChannelProperty); }
            set { SetValue(ShowBChannelProperty, value); }
        }
        public static readonly DependencyProperty ShowBChannelProperty = DependencyProperty.Register("ShowBChannel", typeof(bool), typeof(MainControl), new FrameworkPropertyMetadata(true, new PropertyChangedCallback(OnShowChannelChangedCallback)));
        public bool IsEnableBChannel
        {
            get { return (bool)GetValue(IsEnableBChannelProperty); }
            set { SetValue(IsEnableBChannelProperty, value); }
        }
        public static readonly DependencyProperty IsEnableBChannelProperty = DependencyProperty.Register("IsEnableBChannel", typeof(bool), typeof(MainControl), new FrameworkPropertyMetadata(true));


        public bool ShowAChannel
        {
            get { return (bool)GetValue(ShowAChannelProperty); }
            set { SetValue(ShowAChannelProperty, value); }
        }
        public static readonly DependencyProperty ShowAChannelProperty = DependencyProperty.Register("ShowAChannel", typeof(bool), typeof(MainControl), new FrameworkPropertyMetadata(true, new PropertyChangedCallback(OnShowChannelChangedCallback)));
        public bool IsEnableAChannel
        {
            get { return (bool)GetValue(IsEnableAChannelProperty); }
            set { SetValue(IsEnableAChannelProperty, value); }
        }
        public static readonly DependencyProperty IsEnableAChannelProperty = DependencyProperty.Register("IsEnableAChannel", typeof(bool), typeof(MainControl), new FrameworkPropertyMetadata(true));

        public object[] GetObjects(object[] param)
        {
            return null;
        }

        public bool RemoveObjects(object[] param)
        {
            return false;
        }

        public void Tick()
        {
        }

        public MainControl()
        {
            InitializeComponent();
            EditorCommon.Resources.ResourceInfoManager.Instance.RegResourceInfo(typeof(TextureResourceInfo));
        }

        enum enShowChannelType
        {
            Single,
            Two,
            Three,
            Four,
        }

        void UpdateChannelShow()
        {
            if (mCurrentBitmapImage == null)
                return;

            //var showChannelType = enShowChannelType.Four;
            var pixelWidth = mCurrentBitmapImage.PixelWidth;
            var pixelHeight = mCurrentBitmapImage.PixelHeight;
            var bitsPerPixel = mCurrentBitmapImage.Format.BitsPerPixel;
            //WriteableBitmap bitMap;
            //if(ShowRChannel && ShowGChannel && ShowBChannel && ShowAChannel)
            //{
            //    showChannelType = enShowChannelType.Four;
            //    bitMap = new WriteableBitmap(pixelWidth, pixelHeight, mCurrentBitmapImage.DpiX, mCurrentBitmapImage.DpiY, PixelFormats.Bgra32, mCurrentBitmapImage.Palette);
            //}
            //else if ((ShowRChannel && !ShowGChannel && !ShowBChannel && !ShowAChannel) ||
            //        (!ShowRChannel && ShowGChannel && !ShowBChannel && !ShowAChannel) ||
            //        (!ShowRChannel && !ShowGChannel && ShowBChannel && ShowAChannel) ||
            //        (!ShowRChannel && !ShowGChannel && !ShowBChannel && ShowAChannel))
            //{
            //    showChannelType = enShowChannelType.Single;
            //    bitMap = new WriteableBitmap(pixelWidth, )
            //}

            if (mCurrentBitmapImage.Format == PixelFormats.Bgr32 ||
               mCurrentBitmapImage.Format == PixelFormats.Bgra32)
            {
                if((ShowRChannel && !ShowGChannel && !ShowBChannel && !ShowAChannel) ||
                   (!ShowRChannel && ShowGChannel && !ShowBChannel && !ShowAChannel) ||
                   (!ShowRChannel && !ShowGChannel && ShowBChannel && !ShowAChannel) ||
                   (!ShowRChannel && !ShowGChannel && !ShowBChannel && ShowAChannel))
                {
                    var bitMap = new WriteableBitmap(pixelWidth, pixelHeight, mCurrentBitmapImage.DpiX, mCurrentBitmapImage.DpiY, PixelFormats.Gray8, mCurrentBitmapImage.Palette);
                    var srcStride = pixelWidth * bitsPerPixel / 8;
                    var destStride = pixelWidth;
                    byte[] pixelsSrc = new byte[pixelHeight * srcStride];
                    mCurrentBitmapImage.CopyPixels(pixelsSrc, srcStride, 0);

                    var pixelDest = new byte[pixelHeight * destStride];
                    for(int x = 0; x < pixelWidth; ++x)
                    {
                        for(int y = 0; y < pixelHeight; ++y)
                        {
                            int srcIdx = (x + y * pixelWidth) * bitsPerPixel / 8;
                            int destIdx = (x + y * pixelWidth);
                            if(ShowBChannel)
                            {
                                pixelDest[destIdx] = pixelsSrc[srcIdx];
                            }
                            else if(ShowGChannel)
                            {
                                pixelDest[destIdx] = pixelsSrc[srcIdx + 1];
                            }
                            else if(ShowRChannel)
                            {
                                pixelDest[destIdx] = pixelsSrc[srcIdx + 2];
                            }
                            else if(mCurrentBitmapImage.Format == PixelFormats.Bgra32 && ShowAChannel)
                            {
                                pixelDest[destIdx] = pixelsSrc[srcIdx + 3];
                            }
                        }
                    }
                    bitMap.WritePixels(new Int32Rect(0, 0, pixelWidth, pixelHeight), pixelDest, destStride, 0);
                    Image_Texture.Source = bitMap;
                }
                else
                {
                    var bitMap = new WriteableBitmap(pixelWidth, pixelHeight, mCurrentBitmapImage.DpiX, mCurrentBitmapImage.DpiY, mCurrentBitmapImage.Format, mCurrentBitmapImage.Palette);
                    var stride = pixelWidth * bitsPerPixel / 8;
                    byte[] pixelsSrc = new byte[pixelHeight * stride];
                    mCurrentBitmapImage.CopyPixels(pixelsSrc, stride, 0);

                    byte[] pixelDest = new byte[pixelHeight * stride];
                    for (int x = 0; x < pixelWidth; ++x)
                    {
                        for (int y = 0; y < pixelHeight; ++y)
                        {
                            int indexOfPixel = (x + y * pixelWidth) * bitsPerPixel / 8;
                            if (ShowBChannel)
                                pixelDest[indexOfPixel] = pixelsSrc[indexOfPixel];
                            else
                                pixelDest[indexOfPixel] = 0;
                            if (ShowGChannel)
                                pixelDest[indexOfPixel + 1] = pixelsSrc[indexOfPixel + 1];
                            else
                                pixelDest[indexOfPixel + 1] = 0;
                            if (ShowRChannel)
                                pixelDest[indexOfPixel + 2] = pixelsSrc[indexOfPixel + 2];
                            else
                                pixelDest[indexOfPixel + 2] = 0;
                            if (mCurrentBitmapImage.Format == PixelFormats.Bgra32)
                            {
                                if (ShowAChannel)
                                    pixelDest[indexOfPixel + 3] = pixelsSrc[indexOfPixel + 3];
                                else
                                    pixelDest[indexOfPixel + 3] = 255;
                            }
                        }
                    }
                    bitMap.WritePixels(new Int32Rect(0, 0, pixelWidth, pixelHeight), pixelDest, stride, 0);
                    Image_Texture.Source = bitMap;
                }
            }
            else if(mCurrentBitmapImage.Format == PixelFormats.Gray8)
            {
                Image_Texture.Source = mCurrentBitmapImage;
            }
            else
            {
                throw new InvalidOperationException($"不支持的像素格式{mCurrentBitmapImage.Format}");
            }
        }

        TextureResourceInfo mCurrentTextureResInfo;
        BitmapImage mCurrentBitmapImage;
        public async Task SetObjectToEdit(ResourceEditorContext context)
        {
            mCurrentTextureResInfo = context.ResInfo as TextureResourceInfo;
            SetBinding(TitleProperty, new Binding("ResourceName") { Source = context.ResInfo, Converter = new EditorCommon.Converter.RNameConverter_PureName() });

            //mCurrentDesc = new TextureDesc();
            await TextureResourceInfo.GetTextureDesc(mCurrentTextureResInfo.ResourceName.Address, mCurrentTextureResInfo.mCurrentDesc);

            mCurrentBitmapImage = new System.Windows.Media.Imaging.BitmapImage();
            mCurrentBitmapImage.BeginInit();
            mCurrentBitmapImage.StreamSource = new System.IO.MemoryStream(mCurrentTextureResInfo.mCurrentDesc.RawData);
            mCurrentBitmapImage.EndInit();
            mCurrentBitmapImage.CacheOption = BitmapCacheOption.OnLoad;

            Image_Texture.Source = mCurrentBitmapImage;
            TextBlock_Info.Text = $"{mCurrentBitmapImage.PixelWidth}X{mCurrentBitmapImage.PixelHeight} {mCurrentBitmapImage.Format}";

            mCurrentTextureResInfo.Dimensions = mCurrentBitmapImage.PixelWidth + "X" + mCurrentBitmapImage.PixelHeight;
            mCurrentTextureResInfo.PixelFormat = mCurrentBitmapImage.Format;

            PG_TextureInfo.Instance = mCurrentTextureResInfo.mCurrentDesc;
            mCurrentTextureResInfo.mCurrentDesc.PropertyChanged += (object sender, PropertyChangedEventArgs e) =>
            {
                mCurrentTextureResInfo.IsDirty = true;
            };

            if (mCurrentBitmapImage.Format == PixelFormats.Gray8)
            {
                ShowGChannel = false;
                IsEnableGChannel = false;
                ShowBChannel = false;
                IsEnableBChannel = false;
                ShowAChannel = false;
                IsEnableAChannel = false;
            }
        }

        public void SaveElement(XmlNode node, XmlHolder holder)
        {
        }

        public IDockAbleControl LoadElement(XmlNode node)
        {
            return null;
        }

        public void StartDrag()
        {
        }

        public void EndDrag()
        {
        }

        public bool? CanClose()
        {
            if(mCurrentTextureResInfo.IsDirty)
            {
                var result = EditorCommon.MessageBox.Show("该贴图还有未保存的更改，是否保存后退出？\r\n(点否后会丢失所有未保存的更改)", EditorCommon.MessageBox.enMessageBoxButton.YesNoCancel);
                switch(result)
                {
                    case EditorCommon.MessageBox.enMessageBoxResult.Yes:
                        var noUse = Save();
                        return true;
                    case EditorCommon.MessageBox.enMessageBoxResult.No:
                        mCurrentTextureResInfo.IsDirty = false;
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

        async Task Save()
        {
            TextureResourceInfo.SaveTXPicToFile(mCurrentTextureResInfo.ResourceName.Address, ref mCurrentTextureResInfo.mCurrentDesc.PicDesc, mCurrentTextureResInfo.mCurrentDesc.RawData, mCurrentTextureResInfo.mCurrentDesc);
            await mCurrentTextureResInfo.Save();
            mCurrentTextureResInfo.IsDirty = false;
        }
        private void IconTextBtn_Save_Click(object sender, RoutedEventArgs e)
        {
            var noUse = Save();
        }

        private void ToggleButton_R_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            ShowRChannel = true;
            ShowGChannel = false;
            ShowBChannel = false;
            ShowAChannel = false;
        }

        private void ToggleButton_G_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            ShowRChannel = false;
            ShowGChannel = true;
            ShowBChannel = false;
            ShowAChannel = false;
        }

        private void ToggleButton_B_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            ShowRChannel = false;
            ShowGChannel = false;
            ShowBChannel = true;
            ShowAChannel = false;
        }

        private void ToggleButton_A_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            ShowRChannel = false;
            ShowGChannel = false;
            ShowBChannel = false;
            ShowAChannel = true;
        }

        private void Button_GenerateSrcImage_Click(object sender, RoutedEventArgs e)
        {
            //var noUse = GenerateSrcImage();
            var noUse = RefreshETCImg();
        }
        async Task RefreshETCImg()
        {
            var files = EngineNS.CEngine.Instance.FileManager.GetFiles(EngineNS.CEngine.Instance.FileManager.ProjectContent, "*" + EngineNS.CEngineDesc.TextureExtension, System.IO.SearchOption.AllDirectories);
            int index = 0;
            foreach(var file in files)
            {
                //var file = @"E:\Engine\Content\Test\TexTest\eyediffuse.txpic";
                System.Diagnostics.Debug.WriteLine($"-------------{System.DateTime.Now}---{index}/{files.Count} {file}");

                var resInfo = await EditorCommon.Resources.ResourceInfoManager.Instance.CreateResourceInfoFromFile(file + EditorCommon.Program.ResourceInfoExt, null) as TextureResourceInfo;
                if (resInfo == null)
                {
                    System.Diagnostics.Debugger.Break();
                }
                
                await TextureResourceInfo.GetTextureDesc(resInfo.ResourceName.Address, resInfo.mCurrentDesc);
                TextureResourceInfo.SaveTXPicToFile(file, ref resInfo.mCurrentDesc.PicDesc, resInfo.mCurrentDesc.RawData, resInfo.mCurrentDesc);

                await resInfo.Save();

                System.GC.Collect();
                index++;
            }
        }
        async Task GenerateSrcImage()
        {
            // 根据txpic生成原始png并刷新ResInfo文件
            var files = EngineNS.CEngine.Instance.FileManager.GetFiles(EngineNS.CEngine.Instance.FileManager.ProjectContent, "*" + EngineNS.CEngineDesc.TextureExtension, System.IO.SearchOption.AllDirectories);
            foreach(var file in files)
            {
                var resInfo = await EditorCommon.Resources.ResourceInfoManager.Instance.CreateResourceInfoFromFile(file + EditorCommon.Program.ResourceInfoExt, null) as TextureResourceInfo;
                if(resInfo == null)
                {
                    System.Diagnostics.Debugger.Break();
                }
                if (!string.IsNullOrEmpty(resInfo.LinkedFile))
                    continue;

                var xnd = await EngineNS.IO.XndHolder.LoadXND(file);
                var att = xnd.Node.FindAttrib("PNG");
                byte[] rawData;
                att.BeginRead();
                att.Read(out rawData, (int)att.Length);
                att.EndRead();

                var bitMapImage = new BitmapImage();
                bitMapImage.BeginInit();
                bitMapImage.StreamSource = new System.IO.MemoryStream(rawData);
                bitMapImage.EndInit();

                var bitmap = new System.Drawing.Bitmap(new System.IO.MemoryStream(rawData));
                var tagBitmap = bitmap.Clone(new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height), System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                tagBitmap.Save(file.Replace(".txpic", ".png"), System.Drawing.Imaging.ImageFormat.Png);

                resInfo.LinkedFile = resInfo.ResourceName.PureName() + ".png";
                await resInfo.Save();
            }
        }
    }
}
