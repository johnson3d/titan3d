using System.Windows.Media.Effects;
using System.Windows;
using System.Windows.Media;

namespace WPG.Effect
{
    public class TextBlockEffect : ShaderEffect
    {
        public static readonly DependencyProperty InputProperty = ShaderEffect.RegisterPixelShaderSamplerProperty("Input", typeof(TextBlockEffect), 0);
        public static readonly DependencyProperty WProperty = DependencyProperty.Register("W", typeof(double), typeof(TextBlockEffect), new PropertyMetadata(((double)(0D)), PixelShaderConstantCallback(0)));
        public static readonly DependencyProperty HProperty = DependencyProperty.Register("H", typeof(double), typeof(TextBlockEffect), new PropertyMetadata(((double)(0D)), PixelShaderConstantCallback(1)));

        public static readonly DependencyProperty ColorLTProperty = DependencyProperty.Register("ColorLT", typeof(Color), typeof(TextBlockEffect), new PropertyMetadata(Color.FromArgb(255, 0, 0, 0), PixelShaderConstantCallback(2)));
        public static readonly DependencyProperty ColorRTProperty = DependencyProperty.Register("ColorRT", typeof(Color), typeof(TextBlockEffect), new PropertyMetadata(Color.FromArgb(255, 0, 0, 0), PixelShaderConstantCallback(3)));
        public static readonly DependencyProperty ColorLBProperty = DependencyProperty.Register("ColorLB", typeof(Color), typeof(TextBlockEffect), new PropertyMetadata(Color.FromArgb(255, 0, 0, 0), PixelShaderConstantCallback(4)));
        public static readonly DependencyProperty ColorRBProperty = DependencyProperty.Register("ColorRB", typeof(Color), typeof(TextBlockEffect), new PropertyMetadata(Color.FromArgb(255, 0, 0, 0), PixelShaderConstantCallback(5)));

        //public static readonly DependencyProperty OPTypeProperty = DependencyProperty.Register("OPType", typeof(int), typeof(TextBlockEffect), new PropertyMetadata(0, PixelShaderConstantCallback(6)));
        public static readonly DependencyProperty OPThicknessProperty = DependencyProperty.Register("OPThickness", typeof(float), typeof(TextBlockEffect), new PropertyMetadata(0.0f, PixelShaderConstantCallback(7)));
        
        public TextBlockEffect()
        {
            //PixelShader pixelShader = new PixelShader();
            //pixelShader.UriSource = new Uri("/CodeLinker;component/Effect/TextBlockEffect.ps", UriKind.Relative);
            //this.PixelShader = pixelShader;

            //this.UpdateShaderValue(InputProperty);
            //this.UpdateShaderValue(WProperty);
            //this.UpdateShaderValue(HProperty);
            //this.UpdateShaderValue(ColorLTProperty);
            //this.UpdateShaderValue(ColorRTProperty);
            //this.UpdateShaderValue(ColorLBProperty);
            //this.UpdateShaderValue(ColorRBProperty);

            //this.PaddingBottom = 4;
            //this.PaddingLeft = 4;
            //this.PaddingRight = 4;
            //this.PaddingTop = 4;
            
            ////System.Windows.Data.BindingOperations.SetBinding(this, WProperty,
            ////    new System.Windows.Data.Binding("ActualWidth") { Source = txt, Mode = System.Windows.Data.BindingMode.OneWay });
            ////System.Windows.Data.BindingOperations.SetBinding(this, HProperty,
            ////    new System.Windows.Data.Binding("ActualHeight") { Source = txt, Mode = System.Windows.Data.BindingMode.OneWay });
            
        }

        public Brush Input
        {
            get
            {
                return ((Brush)(this.GetValue(InputProperty)));
            }
            set
            {
                this.SetValue(InputProperty, value);
            }
        }
        public double W
        {
            get
            {
                return ((double)(this.GetValue(WProperty)));
            }
            set
            {
                this.SetValue(WProperty, value);
            }
        }
        public double H
        {
            get
            {
                return ((double)(this.GetValue(HProperty)));
            }
            set
            {
                this.SetValue(HProperty, value);
            }
        }
        public Color ColorLT
        {
            get
            {
                return ((Color)(this.GetValue(ColorLTProperty)));
            }
            set
            {
                this.SetValue(ColorLTProperty, value);
            }
        }
        public Color ColorRT
        {
            get
            {
                return ((Color)(this.GetValue(ColorRTProperty)));
            }
            set
            {
                this.SetValue(ColorRTProperty, value);
            }
        }
        public Color ColorLB
        {
            get
            {
                return ((Color)(this.GetValue(ColorLBProperty)));
            }
            set
            {
                this.SetValue(ColorLBProperty, value);
            }
        }
        public Color ColorRB
        {
            get
            {
                return ((Color)(this.GetValue(ColorRBProperty)));
            }
            set
            {
                this.SetValue(ColorRBProperty, value);
            }
        }

        //public int OPType
        //{
        //    get
        //    {
        //        return ((int)(this.GetValue(OPTypeProperty)));
        //    }
        //    set
        //    {
        //        this.SetValue(OPTypeProperty, value);
        //    }         
        //}

        public float OPThickness
        {
            get
            {
                return ((float)(this.GetValue(OPThicknessProperty)));
            }
            set
            {
                this.SetValue(OPThicknessProperty, value);
            } 
        }
    }
}
