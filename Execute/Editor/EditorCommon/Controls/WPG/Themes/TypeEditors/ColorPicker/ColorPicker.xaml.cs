using System;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Runtime.InteropServices;
using System.Drawing;


namespace WPG.Themes.TypeEditors
{
    public partial class ColorPicker
    {
        public ColorPicker()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty ColorProperty =
            DependencyProperty.Register("Color", typeof(System.Windows.Media.Color), typeof(ColorPicker),
                                        new FrameworkPropertyMetadata(new System.Windows.Media.Color(), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public System.Windows.Media.Color Color
        {
            get { return (System.Windows.Media.Color)GetValue(ColorProperty); }
            set { SetValue(ColorProperty, value); }
        }

        public static readonly DependencyProperty HProperty =
            DependencyProperty.Register("H", typeof(int), typeof(ColorPicker));

        public int H
        {
            get { return (int)GetValue(HProperty); }
            set { SetValue(HProperty, value); }
        }

        public static readonly DependencyProperty SProperty =
            DependencyProperty.Register("S", typeof(int), typeof(ColorPicker));

        public int S
        {
            get { return (int)GetValue(SProperty); }
            set { SetValue(SProperty, value); }
        }

        public static readonly DependencyProperty VProperty =
            DependencyProperty.Register("V", typeof(int), typeof(ColorPicker));

        public int V
        {
            get { return (int)GetValue(VProperty); }
            set { SetValue(VProperty, value); }
        }

        public static readonly DependencyProperty RProperty =
            DependencyProperty.Register("R", typeof(byte), typeof(ColorPicker));

        public byte R
        {
            get { return (byte)GetValue(RProperty); }
            set { SetValue(RProperty, value); }
        }

        public static readonly DependencyProperty GProperty =
            DependencyProperty.Register("G", typeof(byte), typeof(ColorPicker));

        public byte G
        {
            get { return (byte)GetValue(GProperty); }
            set { SetValue(GProperty, value); }
        }

        public static readonly DependencyProperty BProperty =
            DependencyProperty.Register("B", typeof(byte), typeof(ColorPicker));

        public byte B
        {
            get { return (byte)GetValue(BProperty); }
            set { SetValue(BProperty, value); }
        }

        public static readonly DependencyProperty AProperty =
            DependencyProperty.Register("A", typeof(byte), typeof(ColorPicker));

        public byte A
        {
            get { return (byte)GetValue(AProperty); }
            set { SetValue(AProperty, value); }
        }

        public static readonly DependencyProperty HexProperty =
            DependencyProperty.Register("Hex", typeof(string), typeof(ColorPicker));

        public string Hex
        {
            get { return (string)GetValue(HexProperty); }
            set { SetValue(HexProperty, value); }
        }

        public static readonly DependencyProperty HueColorProperty =
            DependencyProperty.Register("HueColor", typeof(System.Windows.Media.Color), typeof(ColorPicker));

        public System.Windows.Media.Color HueColor
        {
            get { return (System.Windows.Media.Color)GetValue(HueColorProperty); }
            set { SetValue(HueColorProperty, value); }
        }

        bool updating;

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);

            if (updating) return;
            updating = true;
            try
            {
                if (e.Property == ColorProperty)
                {
                    UpdateSource(ColorSource.Hsv);
                    UpdateRest(ColorSource.Hsv);
                }
                else if (e.Property == HProperty || e.Property == SProperty || e.Property == VProperty)
                {
                    var c = ColorHelper.ColorFromHsv(H, S / 100.0, V / 100.0);
                    c.A = A;
                    Color = c;
                    UpdateRest(ColorSource.Hsv);
                }
                else if (e.Property == RProperty || e.Property == GProperty || e.Property == BProperty || e.Property == AProperty)
                {
                    Color = System.Windows.Media.Color.FromArgb(A, R, G, B);
                    UpdateRest(ColorSource.Rgba);
                }
                else if (e.Property == HexProperty)
                {
                    Color = ColorHelper.ColorFromString(Hex);
                    UpdateRest(ColorSource.Hex);
                }
            }
            finally
            {
                updating = false;
            }
        }

        void UpdateRest(ColorSource source)
        {
            HueColor = ColorHelper.ColorFromHsv(H, 1, 1);
            UpdateSource((ColorSource)(((int)source + 1) % 3));
            UpdateSource((ColorSource)(((int)source + 2) % 3));
        }

        void UpdateSource(ColorSource source)
        {
            if (source == ColorSource.Hsv)
            {
                double h, s, v;
                ColorHelper.HsvFromColor(Color, out h, out s, out v);

                H = (int)h;
                S = (int)(s * 100);
                V = (int)(v * 100);
            }
            else if (source == ColorSource.Rgba)
            {
                R = Color.R;
                G = Color.G;
                B = Color.B;
                A = Color.A;
            }
            else
            {
                Hex = ColorHelper.StringFromColor(Color);
            }
        }


        public static Bitmap GetScreenSnapShot()
        {
            Rectangle rc =System.Windows.Forms.SystemInformation.VirtualScreen;
            var screenmap = new Bitmap(rc.Width, rc.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            using (Graphics g = Graphics.FromImage(screenmap))
            {
                g.CopyFromScreen(rc.X, rc.Y, 0, 0, rc.Size, CopyPixelOperation.SourceCopy);
            }
            return screenmap;
        }
        public static ImageSource SnapShotImage(Bitmap bmp)
        {
            ImageSource returnSource;
            try
            {
                returnSource = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(bmp.GetHbitmap(),System.IntPtr.Zero,Int32Rect.Empty,BitmapSizeOptions.FromEmptyOptions());
            }
            catch
            {
                returnSource = null;
            }
            return returnSource;
        }

        private Cursor cursorBak;
        public Window ColorCheckWindow;

            public void PixelColorDraw()
            {                
                ColorCheckWindow = new Window();
                ColorCheckWindow.AllowsTransparency = true;
                ColorCheckWindow.Background = new ImageBrush
                {
                ImageSource = SnapShotImage(GetScreenSnapShot())
                };
                ColorCheckWindow.WindowStyle = WindowStyle.None;
                ColorCheckWindow.WindowState = WindowState.Normal;
                ColorCheckWindow.WindowStyle = WindowStyle.None;
                ColorCheckWindow.ResizeMode = ResizeMode.NoResize;
                ColorCheckWindow.Topmost = true;
                ColorCheckWindow.Left = 0.0;
                ColorCheckWindow.Top = 0.0;
            //ColorCheckWindow.Width = 800;
            //ColorCheckWindow.Height = 800;
            ColorCheckWindow.Width = System.Windows.SystemParameters.VirtualScreenWidth;
            ColorCheckWindow.Height = System.Windows.SystemParameters.VirtualScreenHeight;
            cursorBak = Mouse.OverrideCursor;
                try
                {
                Mouse.OverrideCursor=Cursors.Pen;
                }
                catch
                {
                Mouse.OverrideCursor = cursorBak;
                }                
                ColorCheckWindow.Show();
                ColorCheckWindow.MouseLeftButtonUp += Window_MouseLeftButtonUp;                                
            }

        private void Window_MouseLeftButtonUp(object sender, RoutedEventArgs e)
        {
            MainWindow win = new MainWindow();
            Color = win.GetPixelColor();
            ColorCheckWindow.Close();
            isDrawPixelToggleButton.IsChecked = false;
            Mouse.OverrideCursor = cursorBak;
        }        

        private void DrawPixelToggleButton_Checked(object sender, RoutedEventArgs e)
        {          
            isDrawPixelToggleButton.IsChecked = true;
            PixelColorDraw();  
        }
        private void DrawPixelToggleButton_UnChecked(object sender, RoutedEventArgs e)
        {
            isDrawPixelToggleButton.IsChecked = false;
        }


        enum ColorSource
        {
            Hsv, Rgba, Hex
        }
    }
    public partial class MainWindow : Window
    {
        [DllImport("gdi32")]
        private static extern int GetPixel(int hdc, int nXPos, int nYPos);
        [DllImport("user32")]
        private static extern int GetWindowDC(int hwnd);
        [DllImport("user32")]
        private static extern int ReleaseDC(int hWnd, int hDC);
        [DllImport("user32")]
        private static extern bool GetCursorPos(out System.Windows.Point pos);
        public System.Windows.Media.Color GetPixelColor()
        {
           // System.Windows.Point mousePos;
            //GetCursorPos(out mousePos);
            var mousePos = System.Windows.Forms.Cursor.Position;
            var win = GetWindowDC(0);
            int pixelColor = GetPixel(win, (int)mousePos.X, (int)mousePos.Y);
            ReleaseDC(0, win);
            byte b = (byte)((pixelColor >> 0x10) & 0xffL);
            byte g = (byte)((pixelColor >> 8) & 0xffL);
            byte r = (byte)(pixelColor & 0xffL);
            return System.Windows.Media.Color.FromArgb(255, r, g, b);
        }
    }

    class HexTextBox : System.Windows.Controls.TextBox
    {
        protected override void OnKeyDown(System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                var b = BindingOperations.GetBindingExpressionBase(this, TextProperty);
                if (b != null)
                {
                    b.UpdateTarget();
                }
                SelectAll();
            }
        }
    }
  
}
