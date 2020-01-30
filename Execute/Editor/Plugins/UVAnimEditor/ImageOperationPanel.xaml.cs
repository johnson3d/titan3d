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

namespace UVAnimEditor
{
    /// <summary>
    /// ImageOperationPanel.xaml 的交互逻辑
    /// </summary>
    public partial class ImageOperationPanel : UserControl
    {
        public delegate void Delegate_OnSaveSource();
        public Delegate_OnSaveSource OnSaveSource;

        public delegate void Delegate_OnUpdateUVAnimFrames(EngineNS.UISystem.UVAnim uvAnim);
        public Delegate_OnUpdateUVAnimFrames OnUpdateUVAnimFrames;

        EngineNS.UISystem.UVAnim mUVAnim;
        EngineNS.UISystem.UVFrame mSelectedFrame;
        List<Rectangle> mUsedRectangleList = new List<Rectangle>();
        List<TextBlock> mUsedTextBlockList = new List<TextBlock>();
        int mWidth;
        int mHeight;
        //double mUVDeltaOffset = 0.05;   // UV数值修正，解决边缘裁剪问题
        double mUVDeltaOffset = 0;   // 此处改为0，如果是0.05会导致图片压缩，不清晰

        private class PixelData
        {
            public int x;
            public int y;
            public byte alpha;
        }

        enum enOperationMode
        {
            Unknow,
            AutoSelect,
            ManualSelect,
        }
        enOperationMode mOperationMode = enOperationMode.AutoSelect;

        bool mShowFrameRect = true;
        public bool ShowFrameRect
        {
            get { return mShowFrameRect; }
            set
            {
                mShowFrameRect = value;

                foreach (var rect in mUsedRectangleList)
                {
                    rect.Visibility = mShowFrameRect ? Visibility.Visible : Visibility.Hidden;
                }
            }
        }

        bool mShowFrameName = true;
        public bool ShowFrameName
        {
            get { return mShowFrameName; }
            set
            {
                mShowFrameName = value;

                foreach (var text in mUsedTextBlockList)
                {
                    text.Visibility = mShowFrameName ? Visibility.Visible : Visibility.Hidden;
                }
            }
        }

        public EngineNS.RName TextureFile
        {
            get { return (EngineNS.RName)GetValue(TextureFileProperty); }
            set { SetValue(TextureFileProperty, value); }
        }
        public static readonly DependencyProperty TextureFileProperty = DependencyProperty.Register("TextureFile", typeof(EngineNS.RName), typeof(ImageOperationPanel),
                                                        new FrameworkPropertyMetadata(EngineNS.RName.EmptyName, new PropertyChangedCallback(OnTextureFileChanged)));

        public static void OnTextureFileChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ImageOperationPanel control = d as ImageOperationPanel;

            control.Image_Pic.Source = null;

            var fileName = (EngineNS.RName)e.NewValue;
            if (fileName == null || fileName == EngineNS.RName.EmptyName)
                return;

            control.mTextureLoadSemaphore = EngineNS.Thread.ASyncSemaphore.CreateSemaphore(1);
            Action action = async () =>
            {
                var imgs = await EditorCommon.ImageInit.GetImage(fileName.Address);
                control.Image_Pic.Source = imgs[0];

                if (control.Image_Pic.Source != null)
                {
                    BitmapSource bitmap = control.Image_Pic.Source as BitmapSource;

                    control.mWidth = bitmap.PixelWidth;
                    control.mHeight = bitmap.PixelHeight;

                    control.Image_Pic.Width = bitmap.PixelWidth;
                    control.Image_Pic.Height = bitmap.PixelHeight;

                    control.ImageCanvas.Width = bitmap.PixelWidth;
                    control.ImageCanvas.Height = bitmap.PixelHeight;

                    control.ImageViewBox.Width = control.Image_Pic.Width;
                    control.ImageViewBox.Height = control.Image_Pic.Height;

                    Canvas.SetLeft(control.ImageViewBox, 0);
                    Canvas.SetTop(control.ImageViewBox, 0);

                    if(control.mSelectedFrame != null)
                    {
                        Canvas.SetLeft(control.Grid_SelectRect, control.mSelectedFrame.U * bitmap.PixelWidth);
                        Canvas.SetTop(control.Grid_SelectRect, control.mSelectedFrame.V * bitmap.PixelHeight);
                        control.Grid_SelectRect.Width = control.mSelectedFrame.SizeU * bitmap.PixelWidth;
                        control.Grid_SelectRect.Height = control.mSelectedFrame.SizeV * bitmap.PixelHeight;
                    }
                }

                control.mTextureLoadSemaphore.Release();
            };
            action();
        }

        public double SelectRectLeft
        {
            get { return (double)GetValue(SelectRectLeftProperty); }
            set { SetValue(SelectRectLeftProperty, value); }
        }
        public static readonly DependencyProperty SelectRectLeftProperty = DependencyProperty.Register("SelectRectLeft", typeof(double), typeof(ImageOperationPanel),
                                                        new FrameworkPropertyMetadata(0.0, new PropertyChangedCallback(OnSelectRectLeftChanged)));

        public static void OnSelectRectLeftChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ImageOperationPanel panel = d as ImageOperationPanel;
            if (panel.mSelectedFrame == null)
                return;

            var idx = panel.mUVAnim.Frames.IndexOf(panel.mSelectedFrame);
            if (idx < 0)
                return;

            var newValue = (double)e.NewValue;// +panel.mUVDeltaOffset;
            if (double.IsNaN(newValue) || newValue < 0)
                newValue = 0;

            Canvas.SetLeft(panel.mUsedRectangleList[idx], newValue);
            Canvas.SetLeft(panel.mUsedTextBlockList[idx], newValue);

            panel.mSelectedFrame.U = (float)(newValue / panel.mWidth);
            var noUse = panel.mSelectedFrame.UpdateScale9Infos();
        }

        public double SelectRectTop
        {
            get { return (double)GetValue(SelectRectTopProperty); }
            set { SetValue(SelectRectTopProperty, value); }
        }
        public static readonly DependencyProperty SelectRectTopProperty = DependencyProperty.Register("SelectRectTop", typeof(double), typeof(ImageOperationPanel),
                                                        new FrameworkPropertyMetadata(0.0, new PropertyChangedCallback(OnSelectRectTopChanged)));

        public static void OnSelectRectTopChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ImageOperationPanel panel = d as ImageOperationPanel;
            if (panel.mSelectedFrame == null)
                return;

            var idx = panel.mUVAnim.Frames.IndexOf(panel.mSelectedFrame);
            if (idx < 0)
                return;

            var newValue = (double)e.NewValue;// +panel.mUVDeltaOffset;
            if (double.IsNaN(newValue) || newValue < 0)
                newValue = 0;

            Canvas.SetTop(panel.mUsedRectangleList[idx], newValue);
            Canvas.SetTop(panel.mUsedTextBlockList[idx], newValue);
            panel.mSelectedFrame.V = (float)((newValue) / panel.mHeight);
            var noUse = panel.mSelectedFrame.UpdateScale9Infos();
        }

        public double SelectRectWidth
        {
            get { return (double)GetValue(SelectRectWidthProperty); }
            set { SetValue(SelectRectWidthProperty, value); }
        }
        public static readonly DependencyProperty SelectRectWidthProperty = DependencyProperty.Register("SelectRectWidth", typeof(double), typeof(ImageOperationPanel),
                                                        new FrameworkPropertyMetadata(0.0, new PropertyChangedCallback(OnSelectRectWidthChanged)));

        public static void OnSelectRectWidthChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ImageOperationPanel panel = d as ImageOperationPanel;
            if (panel.mSelectedFrame == null)
                return;

            var idx = panel.mUVAnim.Frames.IndexOf(panel.mSelectedFrame);
            if (idx < 0)
                return;

            var newValue = (double)e.NewValue;// -panel.mUVDeltaOffset;
            if (double.IsNaN(newValue))
                newValue = 0;
            if (newValue > panel.Image_Pic.Width)
                newValue = panel.Image_Pic.Width;

            if (newValue < 0)
                newValue = 0;
            panel.mUsedRectangleList[idx].Width = newValue;
            panel.mSelectedFrame.SizeU = (float)((newValue) / panel.mWidth);
            var noUse = panel.mSelectedFrame.UpdateScale9Infos();
        }

        public double SelectRectHeight
        {
            get { return (double)GetValue(SelectRectHeightProperty); }
            set { SetValue(SelectRectHeightProperty, value); }
        }
        public static readonly DependencyProperty SelectRectHeightProperty = DependencyProperty.Register("SelectRectHeight", typeof(double), typeof(ImageOperationPanel),
                                                        new FrameworkPropertyMetadata(0.0, new PropertyChangedCallback(OnSelectRectHeightChanged)));

        public static void OnSelectRectHeightChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ImageOperationPanel panel = d as ImageOperationPanel;
            if (panel.mSelectedFrame == null)
                return;

            var idx = panel.mUVAnim.Frames.IndexOf(panel.mSelectedFrame);
            if (idx < 0)
                return;

            var newValue = (double)e.NewValue;// -panel.mUVDeltaOffset;
            if (double.IsNaN(newValue))
                newValue = 0;
            if (newValue > panel.Image_Pic.Height)
                newValue = panel.Image_Pic.Height;

            if (newValue < 0)
                newValue = 0;
            panel.mUsedRectangleList[idx].Height = newValue;
            panel.mSelectedFrame.SizeV = (float)((newValue) / panel.mHeight);
            var noUse = panel.mSelectedFrame.UpdateScale9Infos();
        }

        #region 处理箭头缩放

        public double ViewBoxWidth
        {
            get { return (double)GetValue(ViewBoxWidthProperty); }
            set { SetValue(ViewBoxWidthProperty, value); }
        }
        public static readonly DependencyProperty ViewBoxWidthProperty = DependencyProperty.Register("ViewBoxWidth", typeof(double), typeof(ImageOperationPanel),
                                                        new FrameworkPropertyMetadata(0.0, new PropertyChangedCallback(OnViewBoxWidthChanged)));

        public static void OnViewBoxWidthChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ImageOperationPanel panel = d as ImageOperationPanel;

            double newValue = (double)e.NewValue;
            var delta = panel.Image_Pic.Width / newValue * 4;
            panel.Arrow_Left_L_Scale.ScaleX = delta;
            panel.Arrow_Right_L_Scale.ScaleX = delta;
            panel.Arrow_Left_R_Scale.ScaleX = delta;
            panel.Arrow_Right_R_Scale.ScaleX = delta;
            panel.Arrow_Up_U_Scale.ScaleX = delta;
            panel.Arrow_Bottom_U_Scale.ScaleX = delta;
            panel.Arrow_Up_B_Scale.ScaleX = delta;
            panel.Arrow_Bottom_B_Scale.ScaleX = delta;

            delta = panel.Image_Pic.Height / newValue * 2;
            foreach (var showText in panel.mUsedTextBlockList)
            {
                var scaleTransform = showText.RenderTransform as ScaleTransform;
                scaleTransform.ScaleX = delta;
            }
        }

        public double ViewBoxHeight
        {
            get { return (double)GetValue(ViewBoxHeightProperty); }
            set { SetValue(ViewBoxHeightProperty, value); }
        }
        public static readonly DependencyProperty ViewBoxHeightProperty = DependencyProperty.Register("ViewBoxHeight", typeof(double), typeof(ImageOperationPanel),
                                                        new FrameworkPropertyMetadata(0.0, new PropertyChangedCallback(OnViewBoxHeightChanged)));

        public static void OnViewBoxHeightChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ImageOperationPanel panel = d as ImageOperationPanel;

            double newValue = (double)e.NewValue;
            var delta = panel.Image_Pic.Height / newValue * 4;
            panel.Arrow_Left_L_Scale.ScaleY = delta;
            panel.Arrow_Right_L_Scale.ScaleY = delta;
            panel.Arrow_Left_R_Scale.ScaleY = delta;
            panel.Arrow_Right_R_Scale.ScaleY = delta;
            panel.Arrow_Up_U_Scale.ScaleY = delta;
            panel.Arrow_Bottom_U_Scale.ScaleY = delta;
            panel.Arrow_Up_B_Scale.ScaleY = delta;
            panel.Arrow_Bottom_B_Scale.ScaleY = delta;

            delta = panel.Image_Pic.Height / newValue * 2;
            foreach (var showText in panel.mUsedTextBlockList)
            {
                var scaleTransform = showText.RenderTransform as ScaleTransform;
                scaleTransform.ScaleY = delta;
            }
        }

        #endregion

        public ImageOperationPanel()
        {
            this.InitializeComponent();

            RenderOptions.SetBitmapScalingMode(Image_Pic, BitmapScalingMode.NearestNeighbor);

            BindingOperations.SetBinding(this, ViewBoxWidthProperty, new Binding("Width") { Source = ImageViewBox });
            BindingOperations.SetBinding(this, ViewBoxHeightProperty, new Binding("Height") { Source = ImageViewBox });
        }

        EngineNS.Thread.ASyncSemaphore mTextureLoadSemaphore;
        public async Task SetUVAnim(EngineNS.UISystem.UVAnim uvAnim)
        {
            mUVAnim = uvAnim;

            if (mUVAnim != null)
            {
                foreach (var rect in mUsedRectangleList)
                {
                    ImageCanvas.Children.Remove(rect);
                }
                foreach (var text in mUsedTextBlockList)
                {
                    ImageCanvas.Children.Remove(text);
                }

                //SetImageFile(mUVAnim.Texture);
                BindingOperations.ClearBinding(this, TextureFileProperty);
                TextureFile = mUVAnim.TextureRName;
                BindingOperations.SetBinding(this, TextureFileProperty, new Binding("TextureRName") { Source = mUVAnim, Mode = BindingMode.TwoWay, UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged });
                if (mTextureLoadSemaphore != null)
                    await mTextureLoadSemaphore.Await();

                //if (mUVAnim.TextureObject != null)
                {
                    mUsedRectangleList.Clear();
                    mUsedTextBlockList.Clear();
                    int i = 0;
                    foreach (var frame in mUVAnim.Frames)
                    {
                        Rectangle rect = new Rectangle()
                        {
                            Stroke = System.Windows.Media.Brushes.Yellow,
                            StrokeDashArray = new DoubleCollection(new Double[] { 2, 1 }),
                            Width = Image_Pic.Width * frame.SizeU,
                            Height = Image_Pic.Height * frame.SizeV,
                            IsHitTestVisible = false
                        };

                        double left = Image_Pic.Width * frame.U;
                        double top = Image_Pic.Height * frame.V;
                        Canvas.SetLeft(rect, left);
                        Canvas.SetTop(rect, top);

                        TextBlock text = new TextBlock()
                        {
                            Background = new SolidColorBrush(Color.FromArgb(120, 0, 0, 0)),
                            Text = MainControl.KeyFrameWord + i,
                            IsHitTestVisible = false,
                            RenderTransform = new ScaleTransform(),
                        };

                        Canvas.SetLeft(text, left);
                        Canvas.SetTop(text, top);

                        mUsedRectangleList.Add(rect);
                        mUsedTextBlockList.Add(text);
                        ImageCanvas.Children.Add(rect);
                        ImageCanvas.Children.Add(text);

                        i++;
                    }
                }
            }

            Grid_SelectRect.Visibility = System.Windows.Visibility.Collapsed;
        }

        //public void SetImageFile(string fileName)
        //{
        //    fileName = CSUtility.Support.IFileManager.Instance.Root + fileName;
        //    Image_Pic.Source = EditorCommon.ImageInit.GetImage(fileName);

        //    BitmapImage bitmap = Image_Pic.Source as BitmapImage;

        //    mWidth = bitmap.PixelWidth;
        //    mHeight = bitmap.PixelHeight;

        //    Image_Pic.Width = bitmap.PixelWidth;
        //    Image_Pic.Height = bitmap.PixelHeight;

        //    ImageCanvas.Width = bitmap.PixelWidth;
        //    ImageCanvas.Height = bitmap.PixelHeight;

        //    ImageViewBox.Width = Image_Pic.Width;
        //    ImageViewBox.Height = Image_Pic.Height;

        //    //Canvas_Container.Width = Image_Pic.Width;
        //    //Canvas_Container.Height = Image_Pic.Height;
        //    //Image_Pic.Width = Image_Pic.Source.wi
        //}

        public void AddFrame()
        {
            Rectangle rect = new Rectangle()
            {
                Stroke = System.Windows.Media.Brushes.Yellow,
                StrokeDashArray = new DoubleCollection(new Double[] { 2, 1 }),
                Width = 0,
                Height = 0,
                IsHitTestVisible = false
            };
            Canvas.SetLeft(rect, 0);
            Canvas.SetTop(rect, 0);
            mUsedRectangleList.Add(rect);

            var scaleTransform = new ScaleTransform();
            scaleTransform.ScaleX = Image_Pic.Height / ViewBoxWidth * 2;
            scaleTransform.ScaleY = Image_Pic.Width / ViewBoxHeight * 2;

            TextBlock text = new TextBlock()
            {
                Background = new SolidColorBrush(Color.FromArgb(120, 0, 0, 0)),
                Text = MainControl.KeyFrameWord + (mUsedRectangleList.Count - 1),
                IsHitTestVisible = false,
                RenderTransform = scaleTransform,
            };


            mUsedTextBlockList.Add(text);

            ImageCanvas.Children.Add(rect);
            ImageCanvas.Children.Add(text);
        }

        public void RemoveFrame(int idx)
        {
            if (idx < 0 || idx >= mUsedRectangleList.Count)
                return;

            var rect = mUsedRectangleList[idx];
            mUsedRectangleList.Remove(rect);
            ImageCanvas.Children.Remove(rect);

            var text = mUsedTextBlockList[idx];
            mUsedTextBlockList.Remove(text);
            ImageCanvas.Children.Remove(text);

            for (int i = 0; i < mUsedTextBlockList.Count; i++)
            {
                mUsedTextBlockList[i].Text = MainControl.KeyFrameWord + i;
            }

            Grid_SelectRect.Visibility = Visibility.Hidden;
        }

        public void SelectedFrame(int idx)
        {
            mSelectedFrame = null;

            if (mUVAnim == null)
                return;

            if (idx < 0 || idx >= mUVAnim.Frames.Count)
                return;

            Grid_SelectRect.Visibility = System.Windows.Visibility.Visible;
            mSelectedFrame = mUVAnim.Frames[idx];

            //Canvas.SetLeft(Grid_SelectRect, Canvas.GetLeft(mUsedRectangleList[idx]));
            SelectRectLeft = Canvas.GetLeft(mUsedRectangleList[idx]);
            //Canvas.SetTop(Grid_SelectRect, Canvas.GetTop(mUsedRectangleList[idx]));
            SelectRectTop = Canvas.GetTop(mUsedRectangleList[idx]);
            //Grid_SelectRect.Width = mUsedRectangleList[idx].Width;
            //Grid_SelectRect.Height = mUsedRectangleList[idx].Height;
            SelectRectWidth = mUsedRectangleList[idx].Width;
            SelectRectHeight = mUsedRectangleList[idx].Height;
        }

        bool mIsDragging = false;
        Point mLeftBtnDownPt = new Point();
        private void ImageCanvas_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                switch (mOperationMode)
                {
                    case enOperationMode.ManualSelect:
                        {
                            mLeftBtnDownPt = e.GetPosition(ImageCanvas);
                            mLeftBtnDownPt.X = (int)mLeftBtnDownPt.X;
                            mLeftBtnDownPt.Y = (int)mLeftBtnDownPt.Y;
                            Mouse.Capture(ImageCanvas);

                            //Canvas.SetLeft(Grid_SelectRect, mLeftBtnDownPt.X);
                            SelectRectLeft = mLeftBtnDownPt.X + mUVDeltaOffset;
                            //Canvas.SetTop(Grid_SelectRect, mLeftBtnDownPt.Y);
                            SelectRectTop = mLeftBtnDownPt.Y + mUVDeltaOffset;

                            //Grid_SelectRect.Width = 0;
                            //Grid_SelectRect.Height = 0;
                            SelectRectWidth = 0;
                            SelectRectHeight = 0;

                            Grid_SelectRect.Visibility = System.Windows.Visibility.Visible;

                            mIsDragging = true;
                        }
                        break;

                    case enOperationMode.AutoSelect:
                        {
                            mLeftBtnDownPt = e.GetPosition(ImageCanvas);

                            BitmapSource image = (BitmapSource)(Image_Pic.Source);
                            if (image == null)
                                break;

                            Int32Rect rect = new Int32Rect(0, 0, image.PixelWidth, image.PixelHeight);
                            var stridePerPixel = image.Format.BitsPerPixel / 8;
                            var stride = stridePerPixel * image.PixelWidth;
                            byte[] data = new byte[image.PixelHeight * stride];
                            // 保存检查过的值
                            bool[] checkData = new bool[image.PixelWidth * image.PixelHeight];
                            Queue<PixelData> needCheckQueue = new Queue<PixelData>();
                            image.CopyPixels(rect, data, stride, 0);

                            // 利用广度优先搜索点击位置图片的max xy和 min xy
                            int startX = (int)mLeftBtnDownPt.X;
                            int startY = (int)mLeftBtnDownPt.Y;
                            int left = 0, top = 0, right = image.PixelWidth, bottom = image.PixelHeight;

                            //int startLine = ((int)mLeftBtnDownPt.Y * image.PixelWidth) * stridePerPixel;
                            //int startPixel = ((int)mLeftBtnDownPt.Y * image.PixelWidth + (int)mLeftBtnDownPt.X) * stridePerPixel;


                            //for (int pix = startPixel; pix >= 0; pix -= stridePerPixel)
                            //{
                            //    if(CheckPixelColor(image, pix, data, 255, 0, 255, 255))
                            //    {
                            //        left = pix + stridePerPixel;
                            //        break;
                            //    }
                            //}

                            //for (int pix = startPixel; pix < startLine + stride; pix += stridePerPixel)
                            //{
                            //    if (CheckPixelColor(image, pix, data, 255, 0, 255, 255))
                            //    {
                            //        right = pix - stridePerPixel;
                            //        break;
                            //    }
                            //}

                            //for (int pix = startPixel; pix >= 0; pix -= stride)
                            //{
                            //    if (CheckPixelColor(image, pix, data, 255, 0, 255, 255))
                            //    {
                            //        top = pix + stride;
                            //        break;
                            //    }
                            //}

                            //for (int pix = startPixel; pix < image.PixelHeight * stride; pix += stride)
                            //{
                            //    if (CheckPixelColor(image, pix, data, 255, 0, 255, 255))
                            //    {
                            //        bottom = pix - stride;
                            //        break;
                            //    }
                            //}
                            //var leftPixel = (left / stridePerPixel);
                            //leftPixel = leftPixel - leftPixel / image.PixelWidth * image.PixelWidth;
                            //var topPixel = (top / stridePerPixel);
                            //topPixel = (int)(topPixel / image.PixelWidth);

                            //var widthPixel = (right - left) / stridePerPixel + 1;
                            //var heightPixel = (bottom - top) / stride + 1;

                            left = startX;
                            top = startY;
                            right = left;
                            bottom = top;

                            var pixelAlpha = GetPixelAlpha(image, startX, startY, data);

                            PixelData pixData = new PixelData()
                            {
                                x = startX,
                                y = startY,
                                alpha = pixelAlpha,
                            };
                            needCheckQueue.Enqueue(pixData);
                            checkData[startY * image.PixelWidth + startX] = true;

                            while (needCheckQueue.Count > 0)
                            {
                                pixData = needCheckQueue.Dequeue();

                                if (left > pixData.x)
                                    left = pixData.x;
                                if (top > pixData.y)
                                    top = pixData.y;
                                if (right < pixData.x)
                                    right = pixData.x;
                                if (bottom < pixData.y)
                                    bottom = pixData.y;

                                CheckAroundPixel(image, pixData.x, pixData.y, data, needCheckQueue, checkData);
                            }

                            var pixelWidth = right - left;
                            var pixelHeight = bottom - top;

                            if (pixelWidth > 0 && pixelHeight > 0)
                            {
                                //Canvas.SetLeft(Grid_SelectRect, leftPixel);
                                SelectRectLeft = left + mUVDeltaOffset;
                                //Canvas.SetTop(Grid_SelectRect, topPixel);
                                SelectRectTop = top + mUVDeltaOffset;
                                //Grid_SelectRect.Width = widthPixel;
                                //Grid_SelectRect.Height = heightPixel;
                                SelectRectWidth = pixelWidth + 1 - mUVDeltaOffset;
                                SelectRectHeight = pixelHeight + 1 - mUVDeltaOffset;

                                Grid_SelectRect.Visibility = System.Windows.Visibility.Visible;
                            }
                            else
                                Grid_SelectRect.Visibility = Visibility.Hidden;
                        }
                        break;
                }

            }
        }

        private bool CheckPixelColor(BitmapSource image, int pixIdx, byte[] data, byte r, byte g, byte b, byte a)
        {
            if (image.Format == PixelFormats.Bgra32)
            {
                if (data[pixIdx] == b &&
                   data[pixIdx + 1] == g &&
                   data[pixIdx + 2] == r &&
                   data[pixIdx + 3] == a)
                {
                    return true;
                }
            }
            else if (image.Format == PixelFormats.Pbgra32 ||
                     image.Format == PixelFormats.Prgba128Float ||
                     image.Format == PixelFormats.Prgba64 ||
                     image.Format == PixelFormats.Rgba128Float ||
                     image.Format == PixelFormats.Rgba64)
            {
                EditorCommon.MessageBox.Show("不支持" + image.Format.ToString() + "格式的文件");
            }

            return false;
        }

        private byte GetPixelAlpha(BitmapSource image, int x, int y, byte[] data)
        {
            if (x < 0 || x > image.PixelWidth ||
               y < 0 || y > image.PixelHeight)
                return 0;

            if (image.Format == PixelFormats.Bgra32)
            {
                var stridePerPixel = image.Format.BitsPerPixel / 8;
                int pixelIdx = (y * image.PixelWidth + x) * stridePerPixel;
                return data[pixelIdx + 3];
            }
            else if (image.Format == PixelFormats.Bgr32)
            {
                return 255;
            }
            else
            {
                EditorCommon.MessageBox.Show("不支持" + image.Format.ToString() + "格式的文件");
            }

            return 0;
        }

        private void CheckAroundPixel(BitmapSource image, int x, int y, byte[] data, Queue<PixelData> checkQueue, bool[] checkData)
        {
            // 将周围合法点加入待检测列表
            if ((x > 0) && (checkData[y * image.PixelWidth + x - 1] == false))
            {
                var alpha = GetPixelAlpha(image, x - 1, y, data);
                if (alpha > 0)
                {
                    PixelData pixData = new PixelData()
                    {
                        x = x - 1,
                        y = y,
                        alpha = alpha,
                    };
                    checkQueue.Enqueue(pixData);
                }

                checkData[y * image.PixelWidth + x - 1] = true;
            }

            if ((x < image.PixelWidth - 1) && (checkData[y * image.PixelWidth + x + 1] == false))
            {
                var alpha = GetPixelAlpha(image, x + 1, y, data);
                if (alpha > 0)
                {
                    PixelData pixData = new PixelData()
                    {
                        x = x + 1,
                        y = y,
                        alpha = alpha,
                    };
                    checkQueue.Enqueue(pixData);
                }

                checkData[y * image.PixelWidth + x + 1] = true;
            }

            if ((y > 0) && (checkData[(y - 1) * image.PixelWidth + x] == false))
            {
                var alpha = GetPixelAlpha(image, x, y - 1, data);
                if (alpha > 0)
                {
                    PixelData pixData = new PixelData()
                    {
                        x = x,
                        y = y - 1,
                        alpha = alpha,
                    };
                    checkQueue.Enqueue(pixData);
                }

                checkData[(y - 1) * image.PixelWidth + x] = true;
            }

            if ((y < image.PixelHeight - 1) && (checkData[(y + 1) * image.PixelWidth + x] == false))
            {
                var alpha = GetPixelAlpha(image, x, y + 1, data);
                if (alpha > 0)
                {
                    PixelData pixData = new PixelData()
                    {
                        x = x,
                        y = y + 1,
                        alpha = alpha,
                    };
                    checkQueue.Enqueue(pixData);
                }

                checkData[(y + 1) * image.PixelWidth + x] = true;
            }
        }

        private void ImageCanvas_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && mIsDragging)
            {
                var point = e.GetPosition(ImageCanvas);
                point.X = (int)point.X + 1;
                point.Y = (int)point.Y + 1;
                var deltaX = point.X - mLeftBtnDownPt.X;
                var deltaY = point.Y - mLeftBtnDownPt.Y;

                if (deltaX > 0)
                {
                    //Grid_SelectRect.Width = deltaX;
                    SelectRectWidth = deltaX - mUVDeltaOffset;
                }
                else
                {
                    //Grid_SelectRect.Width = -deltaX;
                    SelectRectWidth = SelectRectWidth - deltaX - mUVDeltaOffset;
                    //Canvas.SetLeft(Grid_SelectRect, point.X);
                    SelectRectLeft = point.X + mUVDeltaOffset;
                }

                if (deltaY > 0)
                {
                    //Grid_SelectRect.Height = deltaY;
                    SelectRectHeight = deltaY - mUVDeltaOffset;
                }
                else
                {
                    //Grid_SelectRect.Height = -deltaY;
                    SelectRectHeight = SelectRectHeight - deltaY - mUVDeltaOffset;
                    //Canvas.SetTop(Grid_SelectRect, point.Y);
                    SelectRectTop = point.Y + mUVDeltaOffset;
                }
            }
        }

        private void ImageCanvas_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Mouse.Capture(null);
            mIsDragging = false;
        }

        bool mIsSizeRectDragging = false;
        private void SizeRect_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                mLeftBtnDownPt = e.GetPosition((FrameworkElement)sender);
                mLeftBtnDownPt.X = (int)mLeftBtnDownPt.X;
                mLeftBtnDownPt.Y = (int)mLeftBtnDownPt.Y;
                Mouse.Capture((FrameworkElement)sender);
                e.Handled = true;
                mIsSizeRectDragging = true;
            }
        }

        private void SizeRect_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Mouse.Capture(null);
            mIsSizeRectDragging = false;
        }

        private void Rect_Left_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && mIsSizeRectDragging)
            {
                var point = e.GetPosition(Rect_Left);
                point.X = (int)point.X;
                var delta = point.X - mLeftBtnDownPt.X;

                //var left = Canvas.GetLeft(Grid_SelectRect);
                //Canvas.SetLeft(Grid_SelectRect, left + delta);
                SelectRectLeft += delta;

                //var tagWidth = Grid_SelectRect.Width - delta;
                var tagWidth = SelectRectWidth - delta;
                if (tagWidth > 0)
                {
                    //Grid_SelectRect.Width = tagWidth;
                    SelectRectWidth = tagWidth;
                }
            }
        }

        private void Rect_Right_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && mIsSizeRectDragging)
            {
                var point = e.GetPosition(Rect_Right);
                point.X = (int)point.X;
                var delta = point.X - mLeftBtnDownPt.X;

                //var tagWidth = Grid_SelectRect.Width + delta;
                var tagWidth = SelectRectWidth + delta;
                if (tagWidth > 0)
                {
                    //Grid_SelectRect.Width = tagWidth;
                    SelectRectWidth = tagWidth;
                }
            }
        }

        private void Rect_Top_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && mIsSizeRectDragging)
            {
                var point = e.GetPosition(Rect_Top);
                point.Y = (int)point.Y;
                var delta = point.Y - mLeftBtnDownPt.Y;

                //var top = Canvas.GetTop(Grid_SelectRect);
                //Canvas.SetTop(Grid_SelectRect, top + delta);
                SelectRectTop += delta;

                //var tagHeight = Grid_SelectRect.Height - delta;
                var tagHeight = SelectRectHeight - delta;
                if (tagHeight > 0)
                {
                    //Grid_SelectRect.Height = tagHeight;
                    SelectRectHeight = tagHeight;
                }
            }
        }

        private void Rect_Bottom_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && mIsSizeRectDragging)
            {
                var point = e.GetPosition(Rect_Bottom);
                point.Y = (int)point.Y;
                var delta = point.Y - mLeftBtnDownPt.Y;

                //var tagHeight = Grid_SelectRect.Height + delta;
                var tagHeight = SelectRectHeight + delta;
                if (tagHeight > 0)
                {
                    //Grid_SelectRect.Height = tagHeight;
                    SelectRectHeight = tagHeight;
                }
            }
        }

        private void Rect_LeftTop_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && mIsSizeRectDragging)
            {
                var point = e.GetPosition(Rect_LeftTop);
                var deltaX = (int)(point.X) - mLeftBtnDownPt.X;
                var deltaY = (int)(point.Y) - mLeftBtnDownPt.Y;

                //var left = Canvas.GetLeft(Grid_SelectRect);
                //Canvas.SetLeft(Grid_SelectRect, left + deltaX);
                SelectRectLeft += deltaX;

                //var tagWidth = Grid_SelectRect.Width - deltaX;
                var tagWidth = SelectRectWidth - deltaX;
                if (tagWidth > 0)
                {
                    //Grid_SelectRect.Width = tagWidth;
                    SelectRectWidth = tagWidth;
                }

                //var top = Canvas.GetTop(Grid_SelectRect);
                //Canvas.SetTop(Grid_SelectRect, top + deltaY);
                SelectRectTop += deltaY;

                //var tagHeight = Grid_SelectRect.Height - deltaY;
                var tagHeight = SelectRectHeight - deltaY;
                if (tagHeight > 0)
                {
                    //Grid_SelectRect.Height = tagHeight;
                    SelectRectHeight = tagHeight;
                }
            }
        }

        private void Rect_LeftBottom_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && mIsSizeRectDragging)
            {
                var point = e.GetPosition(Rect_LeftBottom);
                var deltaX = (int)(point.X) - mLeftBtnDownPt.X;
                var deltaY = (int)(point.Y) - mLeftBtnDownPt.Y;

                //var left = Canvas.GetLeft(Grid_SelectRect);
                //Canvas.SetLeft(Grid_SelectRect, left + deltaX);
                SelectRectLeft += deltaX;

                //var tagWidth = Grid_SelectRect.Width - deltaX;
                var tagWidth = SelectRectWidth - deltaX;
                if (tagWidth > 0)
                {
                    //Grid_SelectRect.Width = tagWidth;
                    SelectRectWidth = tagWidth;
                }

                //var tagHeight = Grid_SelectRect.Height + deltaY;
                var tagHeight = SelectRectHeight + deltaY;
                if (tagHeight > 0)
                {
                    //Grid_SelectRect.Height = tagHeight;
                    SelectRectHeight = tagHeight;
                }
            }
        }

        private void Rect_RightBottom_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && mIsSizeRectDragging)
            {
                var point = e.GetPosition(Rect_RightBottom);
                var deltaX = (int)(point.X) - mLeftBtnDownPt.X;
                var deltaY = (int)(point.Y) - mLeftBtnDownPt.Y;

                //var tagWidth = Grid_SelectRect.Width + deltaX;
                var tagWidth = SelectRectWidth + deltaX;
                if (tagWidth > 0)
                {
                    //Grid_SelectRect.Width = tagWidth;
                    SelectRectWidth = tagWidth;
                }

                //var tagHeight = Grid_SelectRect.Height + deltaY;
                var tagHeight = SelectRectHeight + deltaY;
                if (tagHeight > 0)
                {
                    //Grid_SelectRect.Height = tagHeight;
                    SelectRectHeight = tagHeight;
                }
            }
        }

        private void Rect_RightTop_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && mIsSizeRectDragging)
            {
                var point = e.GetPosition(Rect_RightTop);
                var deltaX = (int)(point.X) - mLeftBtnDownPt.X;
                var deltaY = (int)(point.Y) - mLeftBtnDownPt.Y;

                //var tagWidth = Grid_SelectRect.Width + deltaX;
                var tagWidth = SelectRectWidth + deltaX;
                if (tagWidth > 0)
                {
                    //Grid_SelectRect.Width = tagWidth;
                    SelectRectWidth = tagWidth;
                }

                //var top = Canvas.GetTop(Grid_SelectRect);
                //Canvas.SetTop(Grid_SelectRect, top + deltaY);
                SelectRectTop += deltaY;

                //var tagHeight = Grid_SelectRect.Height - deltaY;
                var tagHeight = SelectRectHeight - deltaY;
                if (tagHeight > 0)
                {
                    //Grid_SelectRect.Height = tagHeight;
                    SelectRectHeight = tagHeight;
                }
            }
        }

        public bool AutoSelect
        {
            get { return (bool)GetValue(AutoSelectProperty); }
            set { SetValue(AutoSelectProperty, value); }
        }
        public static readonly DependencyProperty AutoSelectProperty = DependencyProperty.Register("AutoSelect", typeof(bool), typeof(ImageOperationPanel), new FrameworkPropertyMetadata(true, new PropertyChangedCallback(OnAutoSelectChanged)));
        public static void OnAutoSelectChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var ctrl = d as ImageOperationPanel;
            var newVal = (bool)e.NewValue;
            if (newVal)
                ctrl.mOperationMode = enOperationMode.AutoSelect;
        }
        public bool ManualSelect
        {
            get { return (bool)GetValue(ManualSelectProperty); }
            set { SetValue(ManualSelectProperty, value); }
        }
        public static readonly DependencyProperty ManualSelectProperty = DependencyProperty.Register("ManualSelect", typeof(bool), typeof(ImageOperationPanel), new FrameworkPropertyMetadata(false, new PropertyChangedCallback(OnManualSelectChanged)));
        public static void OnManualSelectChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var ctrl = d as ImageOperationPanel;
            var newVal = (bool)e.NewValue;
            if (newVal)
                ctrl.mOperationMode = enOperationMode.ManualSelect;
        }

        private void Arrow_Left_L_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            SelectRectLeft--;
            SelectRectWidth++;

            var delta = (int)(ImageViewBox.Width / Image_Pic.Width);
            var pt = new ResourceLibrary.Win32.POINT();
            ResourceLibrary.Win32.GetCursorPos(ref pt);
            ResourceLibrary.Win32.SetCursorPos(pt.X - delta, pt.Y);

            e.Handled = true;
        }

        private void Arrow_Right_L_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (SelectRectWidth > 0)
                SelectRectWidth--;

            var delta = (int)(ImageViewBox.Width / Image_Pic.Width);
            var pt = new ResourceLibrary.Win32.POINT();
            ResourceLibrary.Win32.GetCursorPos(ref pt);
            ResourceLibrary.Win32.SetCursorPos(pt.X - delta, pt.Y);

            e.Handled = true;
        }

        private void Arrow_Left_R_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            SelectRectLeft++;
            if (SelectRectWidth > 0)
                SelectRectWidth--;

            var delta = (int)(ImageViewBox.Width / Image_Pic.Width);
            var pt = new ResourceLibrary.Win32.POINT();
            ResourceLibrary.Win32.GetCursorPos(ref pt);
            ResourceLibrary.Win32.SetCursorPos(pt.X + delta, pt.Y);

            e.Handled = true;
        }

        private void Arrow_Right_R_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            SelectRectWidth++;

            var delta = (int)(ImageViewBox.Width / Image_Pic.Width);
            var pt = new ResourceLibrary.Win32.POINT();
            ResourceLibrary.Win32.GetCursorPos(ref pt);
            ResourceLibrary.Win32.SetCursorPos(pt.X + delta, pt.Y);

            e.Handled = true;
        }

        private void Arrow_Up_U_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            SelectRectTop--;
            SelectRectHeight++;

            var delta = (int)(ImageViewBox.Width / Image_Pic.Width);
            var pt = new ResourceLibrary.Win32.POINT();
            ResourceLibrary.Win32.GetCursorPos(ref pt);
            ResourceLibrary.Win32.SetCursorPos(pt.X, pt.Y - delta);

            e.Handled = true;
        }

        private void Arrow_Bottom_U_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (SelectRectHeight > 0)
                SelectRectHeight--;

            var delta = (int)(ImageViewBox.Width / Image_Pic.Width);
            var pt = new ResourceLibrary.Win32.POINT();
            ResourceLibrary.Win32.GetCursorPos(ref pt);
            ResourceLibrary.Win32.SetCursorPos(pt.X, pt.Y - delta);

            e.Handled = true;
        }

        private void Arrow_Up_B_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            SelectRectTop++;
            if (SelectRectHeight > 0)
                SelectRectHeight--;

            var delta = (int)(ImageViewBox.Width / Image_Pic.Width);
            var pt = new ResourceLibrary.Win32.POINT();
            ResourceLibrary.Win32.GetCursorPos(ref pt);
            ResourceLibrary.Win32.SetCursorPos(pt.X, pt.Y + delta);

            e.Handled = true;
        }

        private void Arrow_Bottom_B_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            SelectRectHeight++;

            var delta = (int)(ImageViewBox.Width / Image_Pic.Width);
            var pt = new ResourceLibrary.Win32.POINT();
            ResourceLibrary.Win32.GetCursorPos(ref pt);
            ResourceLibrary.Win32.SetCursorPos(pt.X, pt.Y + delta);

            e.Handled = true;
        }

        private void Button_Save_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (OnSaveSource != null)
                OnSaveSource();
        }

        public void AutoGridOperation()
        {
            if (mUVAnim == null)
                return;

            AutoGridSetWindow win = new AutoGridSetWindow();
            if (win.ShowDialog() == true)
            {
                if (win.GridColumn == 0 || win.GridRow == 0)
                    return;

                mUVAnim.Frames.Clear();

                var columnWidth = (mWidth / win.GridColumn) * 1.0f / mWidth;
                var rowHeight = (mHeight / win.GridRow) * 1.0f / mHeight;

                for (UInt16 row = 0; row < win.GridRow; row++)
                {
                    for (UInt16 column = 0; column < win.GridColumn; column++)
                    {
                        var frame = new EngineNS.UISystem.UVFrame();

                        frame.U = column * columnWidth;
                        frame.V = row * rowHeight;
                        frame.SizeU = columnWidth;
                        frame.SizeV = rowHeight;

                        mUVAnim.Frames.Add(frame);
                    }
                }

                if (OnUpdateUVAnimFrames != null)
                    OnUpdateUVAnimFrames(mUVAnim);

                var noUse = SetUVAnim(mUVAnim);
            }
        }

        public void SelectAllOperation()
        {
            if (mUVAnim == null || mSelectedFrame == null || mUVAnim.TextureObject == null || Grid_SelectRect == null)
                return;

            mSelectedFrame.U = 0;
            mSelectedFrame.V = 0;
            mSelectedFrame.SizeU = 1f;
            mSelectedFrame.SizeV = 1f;

            var desc = mUVAnim.TextureObject.TxPicDesc;
            Grid_SelectRect.Height = desc.Height;
            Grid_SelectRect.Width = desc.Width;
            if (OnUpdateUVAnimFrames != null)
                OnUpdateUVAnimFrames(mUVAnim);

            var noUse = SetUVAnim(mUVAnim);
        }

    }
}
