using Assimp;
using EngineNS.DesignMacross.Base.Graph;
using EngineNS.DesignMacross.Base.Render;
using EngineNS.EGui.Controls.PropertyGrid;
using SixLabors.Fonts;
using static EngineNS.EGui.Controls.PropertyGrid.TtPGCustomValueEditorAttribute;

namespace EngineNS.DesignMacross.Editor
{
    public delegate void PinValueChangeEvent(string oldValue, string newValue);
    [ImGuiElementRender(typeof(TtGraphElementRender_PinEditableBox))]
    public class TtGraphElement_PinEditableBox : TtWidgetGraphElement, ILayoutable
    {
        public event PinValueChangeEvent OnValueChange;
        public string Content { get; set; } = "";
        public EHorizontalAlignment HorizontalAlignment { get; set; } = EHorizontalAlignment.Left;
        public EVerticalAlignment VerticalAlignment { get; set; } = EVerticalAlignment.Top;
        public float FontScale { get; set; } = 1;
        public Color4f TextColor { get; set; } = new Color4f(0, 0, 0);
        public Color4f BackgroundColor { get; set; } = new Color4f(0, 0, 0, 0);
        public float Rounding { get; set; } = 0;
        public ERoundCornerType CornerType = ERoundCornerType.None;
        private float BoxThickness = 4;

        public Rtti.TtTypeDesc ValueType { get; set; }
        public TtGraphElement_PinEditableBox(string content = "TextBox", EVerticalAlignment verticalAlignment = EVerticalAlignment.Top, EHorizontalAlignment horizontalAlignment = EHorizontalAlignment.Left)
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
            return true;
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

        public void ValueChange(string oldValue, string newValue)
        {
            OnValueChange(oldValue, newValue);
        }

        #region ILayoutable
        public FMargin Margin { get; set; } = FMargin.Default;
        public override SizeF MinSize { get; set; } = new SizeF(30, 5);
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
                size.X = Math.Max(MinSize.Width, size.X + BoxThickness);
                size.Y = Math.Max(MinSize.Height, size.Y + BoxThickness);
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
            Location = new Vector2(hLocation + finalRect.X, Location.Y);
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
    public class TtGraphElementRender_PinEditableBox : IGraphElementRender
    {
        public void Draw(IRenderableElement renderableElement, ref FGraphElementRenderingContext context)
        {
            var pinEditableBox = renderableElement as TtGraphElement_PinEditableBox;
            var cmd = ImGuiAPI.GetWindowDrawList();
            var start = context.ViewportTransform(pinEditableBox.AbsLocation);
            ImGuiAPI.SetCursorScreenPos(in start);
            //ImGuiAPI.Dummy(in Vector2.Zero);
            var oldScale = ImGuiAPI.GetFont().Scale;
            var font = ImGuiAPI.GetFont();
            font.Scale = pinEditableBox.FontScale * context.Camera.Scale;
            ImGuiAPI.PushFont(font);
            DrawEditableValue(pinEditableBox, ref context);
            font.Scale = oldScale;
            ImGuiAPI.PopFont();

        }

        public unsafe void DrawEditableValue(TtGraphElement_PinEditableBox pinEditableBox, ref FGraphElementRenderingContext context)
        {
            var valueType = pinEditableBox.ValueType;
            var controlWidth = pinEditableBox.Size.Width * context.Camera.Scale;
            var boxId = pinEditableBox.Id.ToString();
            if (valueType.SystemType == typeof(bool))
            {
                ImGuiAPI.PushItemWidth(controlWidth);
                var v = CustomConvert.ConvertStringToBoolean(pinEditableBox.Content);
                var saved = v;
                EngineNS.EGui.UIProxy.CheckBox.DrawCheckBox($"##{boxId}", ref v);
                pinEditableBox.Content = v.ToString();
                if (saved != v)
                {
                    pinEditableBox?.ValueChange(pinEditableBox.Content, v.ToString());
                }
                ImGuiAPI.PopItemWidth();
            }
            else if (valueType.SystemType == typeof(SByte) ||
                valueType.SystemType == typeof(Int16) ||
                valueType.SystemType == typeof(Int32))
            {
                ImGuiAPI.PushItemWidth(controlWidth);
                var v = CustomConvert.ConvertStringToInt32(pinEditableBox.Content);
                var saved = v;
                ImGuiAPI.InputInt($"##{boxId}", ref v, -1, 100, ImGuiInputTextFlags_.ImGuiInputTextFlags_None);
                pinEditableBox.Content = v.ToString();
                if (saved != v)
                {
                    pinEditableBox.ValueChange(pinEditableBox.Content, v.ToString());
                }
                ImGuiAPI.PopItemWidth();
            }
            else if (valueType.SystemType == typeof(byte) ||
                valueType.SystemType == typeof(UInt16) ||
                valueType.SystemType == typeof(UInt32) ||
                valueType.SystemType == typeof(UInt64) ||
                valueType.SystemType == typeof(Int64))
            {
                unsafe
                {
                    ImGuiAPI.PushItemWidth(controlWidth);
                    var v = CustomConvert.ConvertStringToInt32(pinEditableBox.Content);
                    var saved = v;
                    ImGuiAPI.InputInt($"##{boxId}", ref v, -1, 100, ImGuiInputTextFlags_.ImGuiInputTextFlags_None);
                    pinEditableBox.Content = v.ToString();
                    if (saved != v)
                    {
                        pinEditableBox?.ValueChange(pinEditableBox.Content, v.ToString());
                    }
                    ImGuiAPI.PopItemWidth();
                }
            }
            else if (valueType.SystemType == typeof(float))
            {
                ImGuiAPI.PushItemWidth(controlWidth);
                var v = CustomConvert.ConvertStringToSingle(pinEditableBox.Content);
                var saved = v;
                ImGuiAPI.InputFloat($"##{boxId}", ref v, 0, 0, "%.3f", ImGuiInputTextFlags_.ImGuiInputTextFlags_None);
                pinEditableBox.Content = v.ToString();
                if (saved != v)
                {
                    pinEditableBox?.ValueChange(pinEditableBox.Content, v.ToString());
                }
                ImGuiAPI.PopItemWidth();
            }
            else if (valueType.SystemType == typeof(double))
            {
                ImGuiAPI.PushItemWidth(controlWidth);
                var v = CustomConvert.ConvertStringToDouble(pinEditableBox.Content);
                var saved = v;
                ImGuiAPI.InputDouble($"##{boxId}", ref v, 0, 0, "%.3f", ImGuiInputTextFlags_.ImGuiInputTextFlags_None);
                pinEditableBox.Content = v.ToString();
                if (saved != v)
                {
                    pinEditableBox?.ValueChange(pinEditableBox.Content, v.ToString());
                }
                ImGuiAPI.PopItemWidth();
            }
            else if (valueType.SystemType == typeof(Vector2))
            {
                ImGuiAPI.PushItemWidth(controlWidth);
                var v = Vector2.FromString(pinEditableBox.Content);
                var saved = v;
                ImGuiAPI.InputFloat2($"##{boxId}", (float*)&v, "%.3f", ImGuiInputTextFlags_.ImGuiInputTextFlags_None);
                pinEditableBox.Content = v.ToString();
                if (saved != v)
                {
                    pinEditableBox?.ValueChange(pinEditableBox.Content, v.ToString());
                }
                ImGuiAPI.PopItemWidth();
            }
            else if (valueType.SystemType == typeof(Vector3))
            {
                ImGuiAPI.PushItemWidth(controlWidth);
                var v = Vector3.FromString(pinEditableBox.Content);
                var saved = v;
                ImGuiAPI.InputFloat3($"##{boxId}", (float*)&v, "%.3f", ImGuiInputTextFlags_.ImGuiInputTextFlags_None);
                pinEditableBox.Content = v.ToString();
                if (saved != v)
                {
                    pinEditableBox?.ValueChange(pinEditableBox.Content, v.ToString());
                }
                ImGuiAPI.PopItemWidth();
            }
            else if (valueType.SystemType == typeof(Vector4))
            {
                ImGuiAPI.PushItemWidth(controlWidth);
                var v = Vector4.FromString(pinEditableBox.Content);
                var saved = v;
                ImGuiAPI.InputFloat4($"##{boxId}", (float*)&v, "%.3f", ImGuiInputTextFlags_.ImGuiInputTextFlags_None);
                pinEditableBox.Content = v.ToString();
                if (saved != v)
                {
                    pinEditableBox?.ValueChange(pinEditableBox.Content, v.ToString());
                }
                ImGuiAPI.PopItemWidth();
            }
            else if (valueType.SystemType == typeof(string))
            {
                unsafe
                {
                    ImGuiAPI.PushItemWidth(controlWidth);
                    var v = pinEditableBox.Content != null ? pinEditableBox.Content.ToString() : "";
                    bool nameChanged = ImGuiAPI.InputText($"##{boxId}", ref v);
                    pinEditableBox.Content = v;
                    if (nameChanged)
                    {
                        pinEditableBox?.ValueChange(pinEditableBox.Content, v.ToString());
                    }
                    ImGuiAPI.PopItemWidth();
                }
            }
            else if (valueType.SystemType == typeof(Color4b))
            {
                EditorInfo info = new EditorInfo();
                var v = Color4b.FromString(pinEditableBox.Content);
                info.Name = pinEditableBox.Id.ToString() + (string.IsNullOrEmpty(pinEditableBox.Description.Parent.Name) ? "" : pinEditableBox.Description.Parent.Name);
                var objV = (object)v;
                info.Value = objV;
                info.Type = valueType;
                if (TtColor4PickerEditorAttribute.OnDrawStatic(in info, out objV))
                {
                    var color4bStr = ((Color4b)objV).ToString();
                    pinEditableBox.Content = color4bStr;
                    pinEditableBox?.ValueChange(pinEditableBox.Content, color4bStr);
                }
            }
        }
    }
}