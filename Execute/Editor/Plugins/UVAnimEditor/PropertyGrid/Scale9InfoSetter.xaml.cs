using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace UVAnimEditor.PropertyGrid
{
    /// <summary>
    /// Interaction logic for Scale9InfoSetter.xaml
    /// </summary>
    public partial class Scale9InfoSetter : UserControl, INotifyPropertyChanged
    {
        #region INotifyPropertyChangedMembers
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            EngineNS.Editor.PropertyChangedUtility.PropertyChangedProcess(this, propertyName, PropertyChanged);
        }
        #endregion

        public object BindInstance
        {
            get { return (object)GetValue(BindInstanceProperty); }
            set { SetValue(BindInstanceProperty, value); }
        }
        public static readonly DependencyProperty BindInstanceProperty =
                            DependencyProperty.Register("BindInstance", typeof(object), typeof(Scale9InfoSetter),
                            new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnBindInstanceChanged)));
        public static void OnBindInstanceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Scale9InfoSetter control = d as Scale9InfoSetter;
            var noUse = control.UpdateImageShow(e.NewValue as EngineNS.UISystem.UVFrame);
            //UISystem.UVFrame frame = e.NewValue as UISystem.UVFrame;
            //if (frame == null)
            //    return;
            //if(frame.ParentAnim == null)
            //    return;
            //if(frame.ParentAnim.TextureObject == null)
            //    return;

            //string strPath = CSUtility.Support.IFileManager.Instance.Root + frame.ParentAnim.Texture;
            //int left = (int)(frame.U * frame.ParentAnim.TextureObject.Width);
            //int top = (int)(frame.V * frame.ParentAnim.TextureObject.Height);
            //int width = (int)(frame.SizeU * frame.ParentAnim.TextureObject.Width);
            //int height = (int)(frame.SizeV * frame.ParentAnim.TextureObject.Height);

            //control.Image_Show.Source = EditorCommon.ImageInit.GetImage(strPath, new System.Windows.Int32Rect(left, top, width, height));
            
            //var bitmap = control.Image_Show.Source as BitmapSource;
            //if (bitmap != null)
            //{
            //    if (bitmap.PixelHeight > bitmap.PixelWidth)
            //        control.Grid_Show.Width = control.Grid_Show.Height * bitmap.PixelWidth / bitmap.PixelHeight;
            //    else
            //        control.Grid_Show.Height = control.Grid_Show.Width * bitmap.PixelHeight / bitmap.PixelWidth;
            //}
        }

        public EditorCommon.CustomPropertyDescriptor BindProperty
        {
            get { return (EditorCommon.CustomPropertyDescriptor)GetValue(BindPropertyProperty); }
            set { SetValue(BindPropertyProperty, value); }
        }
        public static readonly DependencyProperty BindPropertyProperty =
                            DependencyProperty.Register("BindProperty", typeof(EditorCommon.CustomPropertyDescriptor), typeof(Scale9InfoSetter), new UIPropertyMetadata(null));


        public EngineNS.Thickness Scale9Info
        {
            get { return (EngineNS.Thickness)GetValue(Scale9InfoProperty); }
            set { SetValue(Scale9InfoProperty, value); }
        }
        public static readonly DependencyProperty Scale9InfoProperty =
                DependencyProperty.Register("Scale9Info", typeof(EngineNS.Thickness), typeof(Scale9InfoSetter),
                new FrameworkPropertyMetadata(new EngineNS.Thickness(0), new PropertyChangedCallback(OnScale9InfoChanged)));

        public static void OnScale9InfoChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Scale9InfoSetter control = d as Scale9InfoSetter;

            EngineNS.Thickness newValue = (EngineNS.Thickness)e.NewValue;

            var left = newValue.Left * control.Grid_Show.Width;
            var top = newValue.Top * control.Grid_Show.Height;
            var right = newValue.Right * control.Grid_Show.Width;
            var bottom = newValue.Bottom * control.Grid_Show.Height;

            control.Path_Left.Margin = new Thickness(left - control.Path_Left.Width / 2, 0, 0, 0);
            control.Path_Right.Margin = new Thickness(0, 0, right - control.Path_Right.Width / 2, 0);
            control.Path_Top.Margin = new Thickness(0, top - control.Path_Top.Height / 2, 0, 0);
            control.Path_Bottom.Margin = new Thickness(0, 0, 0, bottom - control.Path_Bottom.Height / 2);
            control.Grid_Iner.Margin = new Thickness(left, top, right, bottom);
            control.Path_ShowLeft.Margin = new Thickness(left, 0, 0, 0);
            control.Path_ShowRight.Margin = new Thickness(0, 0, right, 0);
            control.Path_ShowTop.Margin = new Thickness(0, top, 0, 0);
            control.Path_ShowBottom.Margin = new Thickness(0, 0, 0, bottom);
        }

        //public bool LockAll
        //{
        //    get { return (bool)GetValue(LockAllProperty); }
        //    set { SetValue(LockAllProperty, value); }
        //}
        //public static readonly DependencyProperty LockAllProperty =
        //        DependencyProperty.Register("LockAll", typeof(bool), typeof(Scale9InfoSetter),
        //        new FrameworkPropertyMetadata(true, new PropertyChangedCallback(OnLockAllChanged)));

        //public static void OnLockAllChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        //{
        //    Scale9InfoSetter control = d as Scale9InfoSetter;
        //    bool newValue = (bool)e.NewValue;
        //    control.LockH = newValue;
        //    control.LockV = newValue;
        //}

        //public bool LockH
        //{
        //    get { return (bool)GetValue(LockHProperty); }
        //    set { SetValue(LockHProperty, value); }
        //}
        //public static readonly DependencyProperty LockHProperty =
        //        DependencyProperty.Register("LockH", typeof(bool), typeof(Scale9InfoSetter),
        //        new FrameworkPropertyMetadata(true, new PropertyChangedCallback(OnLockHChanged)));

        //public static void OnLockHChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        //{
        //    Scale9InfoSetter control = d as Scale9InfoSetter;
        //    bool newValue = (bool)e.NewValue;

        //    //        mLockH = value;
        //    //        if (mLockAll && !mLockH)
        //    //        {
        //    //            LockAll = false;
        //    //            LockV = true;
        //    //        }
        //}

        //public bool LockV
        //{
        //    get { return (bool)GetValue(LockVProperty); }
        //    set { SetValue(LockVProperty, value); }
        //}
        //public static readonly DependencyProperty LockVProperty =
        //        DependencyProperty.Register("LockV", typeof(bool), typeof(Scale9InfoSetter),
        //        new FrameworkPropertyMetadata(true, new PropertyChangedCallback(OnLockVChanged)));

        //public static void OnLockVChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        //{
        //    Scale9InfoSetter control = d as Scale9InfoSetter;
        //    bool newValue = (bool)e.NewValue;
        //}

        bool mLockAll = true;
        public bool LockAll
        {
            get { return mLockAll; }
            set
            {
                if(mLockAll == value)
                    return;

                mLockAll = value;
                LockH = value;
                LockV = value;

                OnPropertyChanged("LockAll");
            }
        }

        bool mLockH = true;
        public bool LockH
        {
            get { return mLockH; }
            set
            {
                if (mLockH == value)
                    return;

                mLockH = value;
                if (mLockAll && !mLockH)
                {
                    LockAll = false;
                    LockV = true;
                }
                else if (mLockH && mLockV)
                {
                    LockAll = true;
                }

                OnPropertyChanged("LockH");
            }
        }

        bool mLockV = true;
        public bool LockV
        {
            get { return mLockV; }
            set
            {
                if (mLockV == value)
                    return;

                mLockV = value;
                if (mLockAll && !mLockV)
                {
                    LockAll = false;
                    LockH = true;
                }
                else if (mLockV && mLockH)
                {
                    LockAll = true;
                }

                OnPropertyChanged("LockV");
            }
        }

        public Scale9InfoSetter()
        {
            InitializeComponent();
        }

        bool mDragging = false;
        Point mMouseLeftBtnDownPt;

        private void Path_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            mDragging = true;
            mMouseLeftBtnDownPt = e.GetPosition((UIElement)sender);
            Mouse.Capture((UIElement)sender);
        }

        private void Path_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            mDragging = false;
            Mouse.Capture(null);

            //var bitMapImage = Image_Show.Source as BitmapImage;

            var left = Grid_Iner.Margin.Left / Grid_Show.Width;
            var top = Grid_Iner.Margin.Top / Grid_Show.Height;
            var right = Grid_Iner.Margin.Right / Grid_Show.Width;
            var bottom = Grid_Iner.Margin.Bottom / Grid_Show.Height;
            //var x = System.Math.Round(Grid_Iner.Margin.Left / Grid_Show.Width * bitMapImage.PixelWidth);
            //var y = System.Math.Round(Grid_Iner.Margin.Top / Grid_Show.Height * bitMapImage.PixelHeight);
            //var width = System.Math.Round((Grid_Show.Width - Grid_Iner.Margin.Left - Grid_Iner.Margin.Right) / Grid_Show.Width * bitMapImage.PixelWidth);
            //var height = System.Math.Round((Grid_Show.Height - Grid_Iner.Margin.Top - Grid_Iner.Margin.Bottom) / Grid_Show.Height * bitMapImage.PixelHeight);

            //var tempValue = new CodeLinker.Scale9SpriteInfo();
            //tempValue.CenterRect = new Int32Rect((int)x, (int)y, (int)width, (int)height);
            //tempValue.ImageFileString = Value.ImageFileString;
            //Value = tempValue;

            //TextBlock_Info.Text = Value.ToString();
            Scale9Info = new EngineNS.Thickness(left, top, right, bottom);
        }

        private void PathLeft_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && mDragging)
            {
                var pos = e.GetPosition((UIElement)sender);
                var delta = pos.X - mMouseLeftBtnDownPt.X;
                var left = Grid_Iner.Margin.Left + delta;
                if (left < 0)
                    left = 0;
                if (left > Grid_Show.Width)
                    left = Grid_Show.Width;
                if (left > Grid_Show.Width - Grid_Iner.Margin.Right - 1)
                    left = Grid_Show.Width - Grid_Iner.Margin.Right - 1;

                left = (int)(left / mStep) * mStep;

                Path_Left.Margin = new Thickness(left - Path_Left.Width / 2, 0, 0, 0);
                if (mLockH)
                {
                    Grid_Iner.Margin = new Thickness(left, Grid_Iner.Margin.Top, left, Grid_Iner.Margin.Bottom);
                    Path_Right.Margin = new Thickness(0, 0, left - Path_Right.Width / 2, 0);
                    Path_ShowRight.Margin = new Thickness(0, 0, left, 0);
                }
                else
                    Grid_Iner.Margin = new Thickness(left, Grid_Iner.Margin.Top, Grid_Iner.Margin.Right, Grid_Iner.Margin.Bottom);
                Path_ShowLeft.Margin = new Thickness(left, 0, 0, 0);
            }
        }

        private void PathRight_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && mDragging)
            {
                var pos = e.GetPosition((UIElement)sender);
                var delta = pos.X - mMouseLeftBtnDownPt.X;
                var right = Grid_Iner.Margin.Right - delta;
                if (right < 0)
                    right = 0;
                if (right > Grid_Show.Width)
                    right = Grid_Show.Width;
                if (right > Grid_Show.Width - Grid_Iner.Margin.Left - 1)
                    right = Grid_Show.Width - Grid_Iner.Margin.Left - 1;

                right = (int)(right / mStep) * mStep;

                Path_Right.Margin = new Thickness(0, 0, right - Path_Right.Width / 2, 0);
                if (mLockH)
                {
                    Path_Left.Margin = new Thickness(right - Path_Left.Width / 2, 0, 0, 0);
                    Path_ShowLeft.Margin = new Thickness(right, 0, 0, 0);
                    Grid_Iner.Margin = new Thickness(right, Grid_Iner.Margin.Top, right, Grid_Iner.Margin.Bottom);
                }
                else
                    Grid_Iner.Margin = new Thickness(Grid_Iner.Margin.Left, Grid_Iner.Margin.Top, right, Grid_Iner.Margin.Bottom);
                Path_ShowRight.Margin = new Thickness(0, 0, right, 0);
            }
        }

        private void PathUp_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && mDragging)
            {
                var pos = e.GetPosition((UIElement)sender);
                var delta = pos.Y - mMouseLeftBtnDownPt.Y;
                var top = Grid_Iner.Margin.Top + delta;
                if (top < 0)
                    top = 0;
                if (top > Grid_Show.Height)
                    top = Grid_Show.Height;
                if (top > Grid_Show.Height - Grid_Iner.Margin.Bottom - 1)
                    top = Grid_Show.Height - Grid_Iner.Margin.Bottom - 1;

                top = (int)(top / mStep) * mStep;

                Path_Top.Margin = new Thickness(0, top - Path_Top.Height / 2, 0, 0);
                if (mLockV)
                {
                    Path_Bottom.Margin = new Thickness(0, 0, 0, top - Path_Bottom.Height / 2);
                    Path_ShowBottom.Margin = new Thickness(0, 0, 0, top);
                    Grid_Iner.Margin = new Thickness(Grid_Iner.Margin.Left, top, Grid_Iner.Margin.Right, top);
                }
                else
                    Grid_Iner.Margin = new Thickness(Grid_Iner.Margin.Left, top, Grid_Iner.Margin.Right, Grid_Iner.Margin.Bottom);
                Path_ShowTop.Margin = new Thickness(0, top, 0, 0);
            }
        }

        private void PathBottom_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && mDragging)
            {
                var pos = e.GetPosition((UIElement)sender);
                var delta = pos.Y - mMouseLeftBtnDownPt.Y;
                var bottom = Grid_Iner.Margin.Bottom - delta;
                if (bottom < 0)
                    bottom = 0;
                if (bottom > Grid_Show.Height)
                    bottom = Grid_Show.Height; 
                if (bottom > Grid_Show.Height - Grid_Iner.Margin.Top - 1)
                    bottom = Grid_Show.Height - Grid_Iner.Margin.Top - 1;

                bottom = (int)(bottom / mStep) * mStep;

                Path_Bottom.Margin = new Thickness(0, 0, 0, bottom - Path_Bottom.Height / 2);
                if (mLockV)
                {
                    Path_Top.Margin = new Thickness(0, bottom - Path_Top.Height / 2, 0, 0);
                    Path_ShowTop.Margin = new Thickness(0, bottom, 0, 0);
                    Grid_Iner.Margin = new Thickness(Grid_Iner.Margin.Left, bottom, Grid_Iner.Margin.Right, bottom);
                }
                else
                    Grid_Iner.Margin = new Thickness(Grid_Iner.Margin.Left, Grid_Iner.Margin.Top, Grid_Iner.Margin.Right, bottom);
                Path_ShowBottom.Margin = new Thickness(0, 0, 0, bottom);
            }
        }

        private void button_Reset_Click(object sender, RoutedEventArgs e)
        {
            Scale9Info = EngineNS.Thickness.Empty;
        }

        private void TextBlock_H_Left_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            TextBlock text = sender as TextBlock;

            var str = text.Text.Replace("%", "");
            var value = System.Convert.ToInt32(str) * 0.01f;

            if (LockH)
                Scale9Info = new EngineNS.Thickness(value, Scale9Info.Top, value, Scale9Info.Bottom);
            else
                Scale9Info = new EngineNS.Thickness(value, Scale9Info.Top, Scale9Info.Right, Scale9Info.Bottom);
        }

        private void TextBlock_H_Right_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            TextBlock text = sender as TextBlock;

            var str = text.Text.Replace("%", "");
            var value = System.Convert.ToInt32(str) * 0.01f;

            if (LockH)
                Scale9Info = new EngineNS.Thickness(value, Scale9Info.Top, value, Scale9Info.Bottom);
            else
                Scale9Info = new EngineNS.Thickness(Scale9Info.Left, Scale9Info.Top, value, Scale9Info.Bottom);
        }

        private void TextBlock_V_Top_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            TextBlock text = sender as TextBlock;

            var str = text.Text.Replace("%", "");
            var value = System.Convert.ToInt32(str) * 0.01f;

            if (LockV)
                Scale9Info = new EngineNS.Thickness(Scale9Info.Left, value, Scale9Info.Right, value);
            else
                Scale9Info = new EngineNS.Thickness(Scale9Info.Left, value, Scale9Info.Right, Scale9Info.Bottom);
        }

        private void TextBlock_V_Bottom_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            TextBlock text = sender as TextBlock;

            var str = text.Text.Replace("%", "");
            var value = System.Convert.ToInt32(str) * 0.01f;

            if (LockV)
                Scale9Info = new EngineNS.Thickness(Scale9Info.Left, value, Scale9Info.Right, value);
            else
                Scale9Info = new EngineNS.Thickness(Scale9Info.Left, Scale9Info.Top, Scale9Info.Right, value);
        }

        double mStep = 1.0;
        private async Task UpdateImageShow(EngineNS.UISystem.UVFrame frame)
        {
            if (frame == null)
                return;
            if (frame.ParentAnim == null)
                return;
            if (frame.ParentAnim.TextureObject == null)
                return;

            string strPath = frame.ParentAnim.TextureRName.Address;
            var texInfo = frame.ParentAnim.TextureObject.TxPicDesc;
            int left = (int)(frame.U * texInfo.Width);
            int top = (int)(frame.V * texInfo.Height);
            int width = (int)(frame.SizeU * texInfo.Width);
            int height = (int)(frame.SizeV * texInfo.Height);

            Image_Show.Source = await EditorCommon.ImageInit.GetImage(strPath, new System.Windows.Int32Rect(left, top, width, height));
            var bitmap = Image_Show.Source as BitmapSource;
            if (bitmap != null)
            {
                // 宽度不小于300, 高度不小于200
                if (bitmap.PixelHeight > bitmap.PixelWidth)
                {
                    //var delta = bitmap.PixelWidth * 1.0 / bitmap.PixelHeight;
                    //var tempWidth = Grid_Show.Height * delta;
                    ////if(tempWidth < 300)
                    ////{
                    ////    Grid_Show.Height = 300.0 / delta;
                    ////    tempWidth = Grid_Show.Height * delta;
                    ////}

                    //Grid_Show.Width = tempWidth;
                    Grid_Show.Height = 500;
                    Grid_Show.Width = Grid_Show.Height * bitmap.PixelWidth / bitmap.PixelHeight;
                }
                else
                {
                    //var delta = bitmap.PixelHeight * 1.0 / bitmap.PixelWidth;
                    //var tempHeight = Grid_Show.Width * delta;
                    ////if (tempHeight < 200)
                    ////{
                    ////    Grid_Show.Width = 200.0 / delta;
                    ////    tempHeight = Grid_Show.Width * delta;
                    ////}

                    //Grid_Show.Height = tempHeight;
                    Grid_Show.Width = 500;
                    Grid_Show.Height = Grid_Show.Width * bitmap.PixelHeight / bitmap.PixelWidth;
                }

                mStep = Grid_Show.Width / bitmap.PixelWidth;
            }
        }

        private void popup_Opened(object sender, System.EventArgs e)
        {
            // 更新图片显示
            var noUse = UpdateImageShow(BindInstance as EngineNS.UISystem.UVFrame);
        }
    }
}
