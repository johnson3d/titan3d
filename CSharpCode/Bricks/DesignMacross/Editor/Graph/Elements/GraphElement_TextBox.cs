﻿using EngineNS.DesignMacross.Base.Graph;
using EngineNS.DesignMacross.Base.Render;

namespace EngineNS.DesignMacross.Editor
{
    public delegate void ValueChangeEvent(string oldValue, string newValue);
    [ImGuiElementRender(typeof(TtGraphElementRender_TextBox))]
    public class TtGraphElement_TextBox : TtWidgetGraphElement, ILayoutable
    {
        public ValueChangeEvent OnValueChange;
        public string Content { get; set; } = string.Empty;
        public EHorizontalAlignment HorizontalAlignment { get; set; } = EHorizontalAlignment.Left;
        public EVerticalAlignment VerticalAlignment { get; set; } = EVerticalAlignment.Top;
        public float FontScale { get; set; } = 1;
        public Color4f TextColor { get; set; } = new Color4f(0, 0, 0);
        public Color4f BackgroundColor { get; set; } = new Color4f(0, 0, 0, 0);
        public float Rounding { get; set; } = 5;
        public TtGraphElement_TextBox(string content = "TextBox", EVerticalAlignment verticalAlignment = EVerticalAlignment.Top, EHorizontalAlignment horizontalAlignment = EHorizontalAlignment.Left)
        {
            Content = content;
            VerticalAlignment = verticalAlignment;
            HorizontalAlignment = horizontalAlignment;
        }

        public override bool CanDrag()
        {
            return false;
        }

        public override bool HitCheck(Vector2 pos)
        {
            return false;
        }

        public override void OnDragging(Vector2 delta)
        {

        }


        public override void OnSelected(ref FGraphElementRenderingContext context)
        {
            
        }

        public override void OnUnSelected()
        {
            
        }   

        #region ILayoutable
        public FMargin Margin { get; set; } = FMargin.Default;
        public override SizeF Size
        {
            get
            {
                var oldScale = ImGuiAPI.GetFont().Scale;
                var font = ImGuiAPI.GetFont();
                font.Scale = FontScale;
                ImGuiAPI.PushFont(font);
                var size = ImGuiAPI.CalcTextSize(Content, false, 0);
                font.Scale = oldScale;
                ImGuiAPI.PopFont();
                base.Size = new SizeF(size.X, size.Y);
                return new SizeF(size.X, size.Y);
            }
            set
            {
                // nothing
            }
        }
        public SizeF Measuring(SizeF availableSize)
        {
            return new SizeF(Size.Width + Margin.Left + Margin.Right, Size.Height + Margin.Top + Margin.Bottom);
        }

        public SizeF Arranging(Rect finalRect)
        {
            HorizontalAligning(finalRect);
            VerticalAligning(finalRect);
            Location += new Vector2(Margin.Left, Margin.Top);
            return finalRect.Size;
        }
        public void HorizontalAligning(Rect finalRect)
        {
            float hLocation = 0;
            switch (HorizontalAlignment)
            {
                case EHorizontalAlignment.Left:
                    {
                        hLocation = 0;
                    }
                    break;
                case EHorizontalAlignment.Center:
                    {
                        hLocation = (finalRect.Width - Size.Width) / 2;
                    }
                    break;
                case EHorizontalAlignment.Right:
                    {
                        hLocation = finalRect.Width - Size.Width;
                    }
                    break;
                default:
                    break;
            }
            Location = new Vector2(hLocation + finalRect.X, Location.Y) ;
        }
        public void VerticalAligning(Rect finalRect)
        {
            float vLocation = 0;
            switch (VerticalAlignment)
            {
                case EVerticalAlignment.Top:
                    {
                        vLocation = 0;
                    }
                    break;
                case EVerticalAlignment.Center:
                    {
                        vLocation = (finalRect.Height - Size.Height) / 2;
                    }
                    break;
                case EVerticalAlignment.Bottom:
                    {
                        vLocation = (finalRect.Height - Size.Height);
                    }
                    break;
                default:
                    break;
            }
            Location = new Vector2(Location.X, vLocation + finalRect.Y);
        }
        #endregion ILayoutable

    }
    public class TtGraphElementRender_TextBox : IGraphElementRender
    {
        public void Draw(IRenderableElement renderableElement, ref FGraphElementRenderingContext context)
        {
            var textBox = renderableElement as TtGraphElement_TextBox;
            var cmd = ImGuiAPI.GetWindowDrawList();
            var start = context.ViewPortTransform(textBox.AbsLocation);
            ImGuiAPI.SetCursorScreenPos(in start);
            string inputValue = "";
            if(ImGuiAPI.InputText("##in_TextBox", ref inputValue))
            {
                textBox.OnValueChange(textBox.Content, inputValue);
            }
        }
    }
}