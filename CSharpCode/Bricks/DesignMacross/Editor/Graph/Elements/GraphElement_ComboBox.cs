using EngineNS.DesignMacross.Base.Graph;
using EngineNS.DesignMacross.Base.Render;
using EngineNS.EGui.Controls;
using NPOI.SS.Formula.Functions;
using System.Collections;

namespace EngineNS.DesignMacross.Editor
{

    public delegate void ComboBoxSelectEvent(object selectedItem);
    [ImGuiElementRender(typeof(TtGraphElementRender_ComboBox))]
    public class TtGraphElement_ComboBox : TtWidgetGraphElement, ILayoutable
    {
        public ComboBoxSelectEvent OnComboBoxSelect;
        public string Content { get; set; } = string.Empty;
        public EHorizontalAlignment HorizontalAlignment { get; set; } = EHorizontalAlignment.Left;
        public EVerticalAlignment VerticalAlignment { get; set; } = EVerticalAlignment.Top;
        public float FontScale { get; set; } = 1;
        public Color4f TextColor { get; set; } = new Color4f(0, 0, 0);
        public Color4f BackgroundColor { get; set; } = new Color4f(0, 0, 0, 0);
        public float Rounding { get; set; } = 5;
        public IList Items { get; set; }
        public object CurrentSelected = null;
        public TtGraphElement_ComboBox(string content = "None", EVerticalAlignment verticalAlignment = EVerticalAlignment.Top, EHorizontalAlignment horizontalAlignment = EHorizontalAlignment.Left)
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
                var newSize = new SizeF(size.X + 40, size.Y + 10);
                base.Size = newSize;
                return newSize;
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
    public class TtGraphElementRender_ComboBox : IGraphElementRender
    {
        public void Draw(IRenderableElement renderableElement, ref FGraphElementRenderingContext context)
        {
            var comboBoxElement = renderableElement as TtGraphElement_ComboBox;
            var cmd = ImGuiAPI.GetWindowDrawList();
            var start = context.ViewPortTransform(comboBoxElement.AbsLocation);
            
            ImGuiAPI.SetCursorScreenPos(in start);
            //ImGuiAPI.PushStyleColor();
            if (EGui.UIProxy.ComboBox.BeginCombo("##DMCComboBoxe", comboBoxElement.CurrentSelected == null? "None" : comboBoxElement.CurrentSelected.ToString(), comboBoxElement.Size.Width))
            {
                var searchBar = TtEngine.Instance.UIProxyManager["SMDescSearchBar"] as EGui.UIProxy.SearchBarProxy;
                if (searchBar == null)
                {
                    searchBar = new EGui.UIProxy.SearchBarProxy()
                    {
                        InfoText = "Search macross base type",
                        Width = -1,
                    };
                    TtEngine.Instance.UIProxyManager["SMDescSearchBar"] = searchBar;
                }
                if (!ImGuiAPI.IsAnyItemActive() && !ImGuiAPI.IsMouseClicked(0, false))
                    ImGuiAPI.SetKeyboardFocusHere(0);
                searchBar.OnDraw(in cmd, in Support.TtAnyPointer.Default);
                bool bSelected = true;
                foreach(var item in comboBoxElement.Items)
                {
                    if (!string.IsNullOrEmpty(searchBar.SearchText) && !item.ToString().ToLower().Contains(searchBar.SearchText.ToLower()))
                        continue;
                    Vector2 selectableSize = new Vector2(200, 0);
                    if (ImGuiAPI.Selectable(item.ToString(), ref bSelected, ImGuiSelectableFlags_.ImGuiSelectableFlags_None, in selectableSize))
                    {
                        comboBoxElement.CurrentSelected = item;
                        comboBoxElement.OnComboBoxSelect?.Invoke(item);
                    }
                    if (ImGuiAPI.IsItemHovered(ImGuiHoveredFlags_.ImGuiHoveredFlags_None))
                    {
                        CtrlUtility.DrawHelper(item.ToString());
                    }
                }
                EGui.UIProxy.ComboBox.EndCombo();
            }
        }
    }
}