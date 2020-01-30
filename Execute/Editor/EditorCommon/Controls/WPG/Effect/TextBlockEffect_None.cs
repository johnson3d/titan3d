using System;
using System.Windows.Media.Effects;

namespace WPG.Effect
{
    public class TextBlockEffect_None : TextBlockEffect
    {
        public TextBlockEffect_None()
        {            
            PixelShader pixelShader = new PixelShader();
            pixelShader.UriSource = new Uri("/EditorCommon;component/Effect/TextBlockEffect_None.ps", UriKind.Relative);
            this.PixelShader = pixelShader;

            this.UpdateShaderValue(InputProperty);
            this.UpdateShaderValue(WProperty);
            this.UpdateShaderValue(HProperty);
            this.UpdateShaderValue(ColorLTProperty);
            this.UpdateShaderValue(ColorRTProperty);
            this.UpdateShaderValue(ColorLBProperty);
            this.UpdateShaderValue(ColorRBProperty);

            this.PaddingBottom = 4;
            this.PaddingLeft = 4;
            this.PaddingRight = 4;
            this.PaddingTop = 4;
            
            //System.Windows.Data.BindingOperations.SetBinding(this, WProperty,
            //    new System.Windows.Data.Binding("ActualWidth") { Source = txt, Mode = System.Windows.Data.BindingMode.OneWay });
            //System.Windows.Data.BindingOperations.SetBinding(this, HProperty,
            //    new System.Windows.Data.Binding("ActualHeight") { Source = txt, Mode = System.Windows.Data.BindingMode.OneWay });
            
        }
    }
}
