using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Effects;

namespace EditorCommon.Effect
{
    public class ColorBlendEffect : ShaderEffect
    {
        // Shader Code
        /*
            sampler2D input : register(s0); 
            float4 blendColor : register(c0); 

            float4 main(float2 uv : TEXCOORD) : COLOR 
            { 
                float4 Color; 
                Color = tex2D(input, uv.xy);
	            Color = blendColor * Color;
                return Color;
            }
         */

        private static PixelShader mPixelShader = new PixelShader();

        public Brush Input
        {

            get { return (Brush)GetValue(InputProperty); }
            set { SetValue(InputProperty, value); }
        }

        public static readonly DependencyProperty InputProperty = ShaderEffect.RegisterPixelShaderSamplerProperty("input", typeof(ColorBlendEffect), 0);
        public Color BlendColor
        {
            get { return (Color)GetValue(BlendColorProperty); }
            set { SetValue(BlendColorProperty, value); }
        }
        public static readonly DependencyProperty BlendColorProperty = DependencyProperty.Register("BlendColor", typeof(Color), typeof(ColorBlendEffect), new UIPropertyMetadata(Colors.White, PixelShaderConstantCallback(0)));

        public Brush BlendBrush
        {
            get { return (Brush)GetValue(BlendBrushProperty); }
            set { SetValue(BlendBrushProperty, value); }
        }
        public static readonly DependencyProperty BlendBrushProperty = DependencyProperty.Register("BlendBrush", typeof(Brush), typeof(ColorBlendEffect), new UIPropertyMetadata(Brushes.White, OnBlendBrushPropertyChanged));
        private static void OnBlendBrushPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var ctrl = sender as ColorBlendEffect;
            var newBrush = e.NewValue as SolidColorBrush;
            if(newBrush != null)
            {
                ctrl.BlendColor = newBrush.Color;
            }
        }
        static ColorBlendEffect()
        {
            mPixelShader.UriSource = new Uri("pack://application:,,,/EditorCommon;component/Effect/RegularNode_color.ps", UriKind.Absolute);
            //{ UriSource = new Uri("pack://application:,,,/EditorCommon;component/Effect/RegularNode_color.ps", UriKind.Absolute) }
        }
        public ColorBlendEffect()
        {
            PixelShader = mPixelShader;
            UpdateShaderValue(InputProperty);
            UpdateShaderValue(BlendColorProperty);
        }
    }
}
