using EngineNS.Rtti;
using EngineNS.UI.Bind;
using EngineNS.UI.Controls;
using EngineNS.UI.Controls.Containers;
using EngineNS.UI.Template;
using EngineNS.UI.Trigger;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.UI
{
    public partial class TtUIManager
    {
        Dictionary<UTypeDesc, TtUITemplate> mSystemDefaultTemplates = new Dictionary<UTypeDesc, TtUITemplate>();
        Dictionary<UTypeDesc, TtUITemplate> mDefaultTemplates = new Dictionary<UTypeDesc, TtUITemplate>();
        Dictionary<string, TtUITemplate> mTemplates = new Dictionary<string, TtUITemplate>();

        public void InitSystemDefaultTemplates()
        {
            mSystemDefaultTemplates.Clear();

            // Button
            var buttonTemplate = new TtControlTemplate()
            {
                TargetType = UTypeDesc.TypeOf(typeof(Controls.TtButton)),
            };
            buttonTemplate.DefaultValues.Add(new TtTemplateSimpleValue(new TtBrush(Color.White, TtBrush.EBrushType.Rectangle), TtButton.BackgroundProperty));
            buttonTemplate.DefaultValues.Add(new TtTemplateSimpleValue(new TtBrush(Color.Tomato, TtBrush.EBrushType.Border), TtButton.BorderBrushProperty));

            var buttonRoot = new TtUIElementFactory(UTypeDesc.TypeOf(typeof(Controls.Containers.TtBorder)));
            buttonRoot.SetValue(Controls.Containers.TtBorder.NameProperty, "border");
            buttonRoot.SetTemplateBindingValue<TtBrush, TtBrush>(Controls.Containers.TtBorder.BackgroundProperty, "Background", "Background");
            buttonRoot.SetTemplateBindingValue<TtBrush, TtBrush>(Controls.Containers.TtBorder.BorderBrushProperty, "BorderBrush", "BorderBrush");
            buttonRoot.SetTemplateBindingValue<Thickness, Thickness>(Controls.Containers.TtBorder.BorderThicknessProperty, "BorderThickness", "BorderThickness");
            buttonTemplate.TemplateRoot = buttonRoot;
            var content = new TtUIElementFactory(UTypeDesc.TypeOf(typeof(Controls.TtContentsPresenter)));
            content.SetValue(Controls.TtContentsPresenter.NameProperty, "contentPresenter");
            content.SetTemplateBindingValue<Thickness, Thickness>(Controls.TtContentsPresenter.MarginProperty, "Margin", "Padding");
            content.SetValue(Controls.Containers.TtBorder.HorizontalAlignmentProperty, Controls.HorizontalAlignment.Center);
            content.SetValue(Controls.Containers.TtBorder.VerticalAlignmentProperty, Controls.VerticalAlignment.Center);
            buttonRoot.AppendChild(content);
            mSystemDefaultTemplates[UTypeDesc.TypeOf(typeof(TtButton))] = buttonTemplate;

            var proTrigger = new TtUIPropertyTrigger();
            proTrigger.AddCondition(TtButton.IsEnabledProperty, false);
            proTrigger.AddTriggerValue(TtBorder.BackgroundProperty, new TtBrush(Color.Gray, TtBrush.EBrushType.Rectangle), "border");
            proTrigger.AddTriggerValue(TtBorder.BorderBrushProperty, new TtBrush(Color.DarkGray, TtBrush.EBrushType.Rectangle), "border");
            buttonTemplate.AddTrigger(proTrigger);

            var mouseOverTrigger = new TtUIPropertyTrigger();
            mouseOverTrigger.AddCondition(TtButton.IsMouseOverProperty, true);
            mouseOverTrigger.AddTriggerValue(TtBorder.BackgroundProperty, new TtBrush(Color.GreenYellow, TtBrush.EBrushType.Rectangle), "border");
            mouseOverTrigger.AddTriggerValue(TtBorder.BorderBrushProperty, new TtBrush(Color.Yellow, TtBrush.EBrushType.Rectangle), "border");
            buttonTemplate.AddTrigger(mouseOverTrigger);

            var pressedTrigger = new TtUIPropertyTrigger();
            pressedTrigger.AddCondition(TtButton.IsPressedProperty, true);
            pressedTrigger.AddTriggerValue(TtBorder.BackgroundProperty, new TtBrush(Color.Red, TtBrush.EBrushType.Rectangle), "border");
            pressedTrigger.AddTriggerValue(TtBorder.BorderBrushProperty, new TtBrush(Color.IndianRed, TtBrush.EBrushType.Rectangle), "border");
            buttonTemplate.AddTrigger(pressedTrigger);
        }
        public TtUITemplate GetDefaultTemplate(UTypeDesc type)
        {
            if (mDefaultTemplates.TryGetValue(type, out var defaultTemplate))
                return defaultTemplate;
            if (mSystemDefaultTemplates.TryGetValue(type, out var sysDefTemplate))
                return sysDefTemplate;
            return null;
        }
    }
}
