using EngineNS.DesignMacross.Base.Graph;
using EngineNS.DesignMacross.Base.Render;

namespace EngineNS.DesignMacross.Editor
{
    [ImGuiElementRender(typeof(TtGraphElementRender_RNameSelect))]
    public class TtGraphElement_RNameSelect : TtWidgetGraphElement, ILayoutable
    {
        public string Content { get; set; } = string.Empty;
        public EHorizontalAlignment HorizontalAlignment { get; set; } = EHorizontalAlignment.Left;
        public EVerticalAlignment VerticalAlignment { get; set; } = EVerticalAlignment.Top;
        public float FontScale { get; set; } = 1;
        public Color4f TextColor { get; set; } = new Color4f(0, 0, 0);
        public Color4f BackgroundColor { get; set; } = new Color4f(0, 0, 0, 0);
        public float Rounding { get; set; } = 0;
        public ERoundCornerType CornerType = ERoundCornerType.None;

        public Func<string> GetBrowserRNameValueFunc;
        public Action<string> SetBrowserRNameValueFunc;
        public Func<string> GetBrowserFilterExtsFunc;
        public Action<string> SetBrowserFilterExtsFunc;
        public Func<Rtti.TtTypeDesc> GetBrowserShowTypeFunc;
        public Action<Rtti.TtTypeDesc> SetBrowserShowTypeFunc;

        public Func<EGui.Controls.TtContentBrowser> GetContentBrowser;
        public Func<bool> GetBrowserVisibleFunc;
        public Action<bool> SetBrowserVisibleFunc;
        public Action<string, string> OnValueChange;
        public TtGraphElement_RNameSelect(string content = "TextBox", EVerticalAlignment verticalAlignment = EVerticalAlignment.Top, EHorizontalAlignment horizontalAlignment = EHorizontalAlignment.Left)
        {
            Content = content;
            VerticalAlignment = verticalAlignment;
            HorizontalAlignment = horizontalAlignment;
        }

        public override bool CanDrag()
        {
            return false;
        }

        public override bool HitCheck(ref FMouseEventContext context)
        {
            return false;
        }

        public override void OnDragging(Vector2 delta)
        {

        }


        public override void OnSelected(ref FMouseEventContext context)
        {
            
        }

        public override void OnUnSelected(ref FMouseEventContext context)
        {
            
        }   

        #region ILayoutable
        public FMargin Margin { get; set; } = FMargin.Default;
        public override SizeF MinSize { get; set; } = new SizeF(40,20);
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
                var finalSize = new SizeF(0, 0);
                if (GetBrowserRNameValueFunc() == null)
                {
                    size.X = Math.Max(MinSize.Width, size.X);
                    size.Y = Math.Max(MinSize.Height, size.Y);
                    finalSize = new SizeF(size.X + 20, size.Y);
                }
                else
                {
                    size.X = Math.Max(MinSize.Width, size.X);
                    size.Y = Math.Max(MinSize.Height, size.Y);
                    finalSize = new SizeF(size.X + 80, 80);
                }
                base.Size = finalSize;
                return finalSize;
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
    public class TtGraphElementRender_RNameSelect : IGraphElementRender
    {

        public unsafe void Draw(IRenderableElement renderableElement, ref FGraphElementRenderingContext context)
        {
            var element = renderableElement as TtGraphElement_RNameSelect;
            var cmd = ImGuiAPI.GetWindowDrawList();
            var start = context.ViewportTransform(element.AbsLocation);
            ImGuiAPI.SetCursorScreenPos(in start);
            //ImGuiAPI.Dummy(in Vector2.Zero);
            //ImGuiAPI.SetNextItemWidth(element.Size.Width);
            ImGuiAPI.PushItemWidth(element.Size.Width * context.Camera.Scale);
            if (element.GetBrowserRNameValueFunc() != null)
            {
                var iconSize = new Vector2(64, 64) * context.Camera.Scale;
                var end = start + iconSize;
                var cmdList = ImGuiAPI.GetWindowDrawList();
                var ameta = TtEngine.Instance.AssetMetaManager.GetAssetMeta(RName.ParseFrom(element.GetBrowserRNameValueFunc()));
                if(ameta == null ||!ameta.HasSnapshot)
                {
                    cmdList.AddRect(in start, in end, 0xFFFFFFFF, 5, ImDrawFlags_.ImDrawFlags_RoundCornersAll, 1);
                }
                else
                {
                    ameta.OnDrawSnapshot(cmdList, ref start, ref end);
                }

                var cmdListFont = ImGuiAPI.GetDrawListFont(cmdList);
                var textPos = new Vector2(start.X, end.Y);
                cmdList.AddText(cmdListFont, cmdListFont.FontSize, &textPos, 0xFFFFFFFF, element.GetBrowserRNameValueFunc(), null, 0.0f, null);
                ImGuiAPI.SetCursorScreenPos(new Vector2(start.X + iconSize.X, start.Y));
                ImGuiAPI.Dummy(in Vector2.Zero);
            }
            else
            {
                ImGuiAPI.Text("null");
                ImGuiAPI.SameLine(0, -1);
            }
            //ImGuiAPI.SameLine(0, -1);

            element.GetContentBrowser().ExtNames = element.GetBrowserFilterExtsFunc();
            element.GetContentBrowser().MacrossBase = element.GetBrowserShowTypeFunc();
            element.GetContentBrowser().SelectedAssets.Clear();
            if (ImGuiAPI.Button("+"))
            {
                element.GetContentBrowser().Visible = true;
                ImGuiAPI.OpenPopup($"RName: {element.Description.Id} {element.Name}", ImGuiPopupFlags_.ImGuiPopupFlags_None);
                element.SetBrowserVisibleFunc(true);
            }
            ImGuiAPI.SetNextWindowSize(new Vector2(400, 500), ImGuiCond_.ImGuiCond_FirstUseEver);
            var browserVisible = element.GetBrowserVisibleFunc();
            if (browserVisible)
            {
                if (ImGuiAPI.BeginPopupModal($"RName: {element.Description.Id} {element.Name}", ref browserVisible, ImGuiWindowFlags_.ImGuiWindowFlags_None))
                {
                    element.GetContentBrowser().OnDraw();
                    ImGuiAPI.EndPopup();
                }
            }
            if (element.GetContentBrowser().SelectedAssets.Count > 0 &&
                    element.GetContentBrowser().SelectedAssets[0].GetAssetName().ToString() != element.GetBrowserRNameValueFunc())
            {
                element.SetBrowserRNameValueFunc(element.GetContentBrowser().SelectedAssets[0].GetAssetName().ToString());
            }
            ImGuiAPI.PopItemWidth();
        }
    }
}