using EngineNS.Rtti;
using EngineNS.UI.Bind;
using EngineNS.UI.Controls.Containers;
using EngineNS.UI.Event;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace EngineNS.UI.Controls
{
    /// <summary>
    /// Specifies the expanding direction of a expansion.
    /// </summary>
    public enum EExpandDirection
    {
        /// <summary>
        /// Expander will expand to the down direction.
        /// </summary>
        Down = 0,
        /// <summary>
        /// Expander will expand to the up direction.
        /// </summary>
        Up = 1,
        /// <summary>
        /// Expander will expand to the left direction.
        /// </summary>
        Left = 2,
        /// <summary>
        /// Expander will expand to the right direction.
        /// </summary>
        Right = 3,
    }
    [Editor_UIControl("Controls.Expander", "Expander", "")]
    public partial class TtExpander : TtHeaderedContentsControl
    {
        static TtExpander()
        {
            // Expander system default template
            var expanderTemplate = new Template.TtControlTemplate()
            {
                TargetType = TtTypeDesc.TypeOf(typeof(TtExpander)),
            };
            TtUIManager.SystemDefaultTemplates[TtTypeDesc.TypeOf(typeof(TtExpander))] = expanderTemplate;

            var root = new Template.TtUIElementFactory(TtTypeDesc.TypeOf(typeof(TtStackPanel)));
            root.SetValue(TtStackPanel.NameProperty, "panel");
            root.SetValue(TtStackPanel.OrientationProperty, ELayout_Orientation.Vertical);
            root.SetTemplateBindingValue<TtBrush, TtBrush>(TtStackPanel.BackgroundProperty, "Background", "Background");
            root.SetTemplateBindingValue<TtBrush, TtBrush>(TtStackPanel.BorderBrushProperty, "BorderBrush", "BorderBrush");
            root.SetTemplateBindingValue<Thickness, Thickness>(TtStackPanel.BorderThicknessProperty, "BorderThickness", "BorderThickness");
            expanderTemplate.TemplateRoot = root;

            var headerBorder = new Template.TtUIElementFactory(TtTypeDesc.TypeOf(typeof(TtBorder)));
            headerBorder.SetValue(TtBorder.NameProperty, "headerBorder");
            headerBorder.SetValue(TtBorder.BackgroundProperty, new TtBrush(Color4b.Transparent, TtBrush.EBrushType.Rectangle));
            root.AppendChild(headerBorder);
            var headerToggle = new Template.TtUIElementFactory(TtTypeDesc.TypeOf(typeof(TtToggleButton)));
            headerToggle.SetValue(TtToggleButton.NameProperty, "headerToggle");
            headerToggle.SetValue(TtToggleButton.BackgroundProperty, new TtBrush(Color4b.Transparent, TtBrush.EBrushType.Rectangle));
            headerToggle.SetValue(TtToggleButton.SizeToContentProperty, ESizeToContent.WidthAndHeight);
            headerToggle.SetValue(TtBorder.HorizontalAlignmentProperty, HorizontalAlignment.Stretch);
            headerToggle.SetValue(TtBorder.VerticalAlignmentProperty, VerticalAlignment.Stretch);
            headerToggle.SetTemplateBindingValue<bool, bool>(TtToggleButton.IsCheckedProperty, "IsExpanded", "IsExpanded");
            headerBorder.AppendChild(headerToggle);
            var headerContent = new Template.TtUIElementFactory(TtTypeDesc.TypeOf(typeof(TtContentsPresenter)));
            headerContent.SetValue(TtContentsPresenter.NameProperty, "headerContentPresenter");
            headerContent.SetValue(TtContentsPresenter.ContentSourceProperty, "Header");
            headerBorder.AppendChild(headerContent);

            var content = new Template.TtUIElementFactory(TtTypeDesc.TypeOf(typeof(TtContentsPresenter)));
            content.SetValue(TtContentsPresenter.NameProperty, "contentPresenter");
            content.SetValue(TtContentsPresenter.ContentSourceProperty, "Content");
            content.SetValue(TtContentsPresenter.VisibilityProperty, Visibility.Collapsed);
            content.SetTemplateBindingValue<Thickness, Thickness>(TtContentsPresenter.MarginProperty, "Margin", "Padding");
            root.AppendChild(content);

            // direction trigger set orientation
            var proTrigger = new Trigger.TtUIPropertyTrigger();
            proTrigger.AddCondition(TtExpander.ExpandDirectionProperty, EExpandDirection.Right);
            proTrigger.AddTriggerValue(TtStackPanel.OrientationProperty, ELayout_Orientation.Horizontal, "panel");
            expanderTemplate.AddTrigger(proTrigger);

            var contentVisTrigger = new Trigger.TtUIPropertyTrigger();
            contentVisTrigger.AddCondition(TtExpander.IsExpandedProperty, true);
            contentVisTrigger.AddTriggerValue(TtContentsPresenter.VisibilityProperty, Visibility.Visible, "contentPresenter");
            expanderTemplate.AddTrigger(contentVisTrigger);
        }
        EExpandDirection mExpandDirection = EExpandDirection.Down;
        /// <summary>
        /// ExpandDirection specifies to which direction the content will expand
        /// </summary>
        [Rtti.Meta, BindProperty]
        [Category("Behavior")]
        public EExpandDirection ExpandDirection
        {
            get => mExpandDirection;
            set
            {
                OnValueChange(value, mExpandDirection);
                mExpandDirection = value;
            }
        }
        bool mIsExpanded = false;
        /// <summary>
        /// IsExpanded indicates whether the expander is currently expanded.
        /// </summary>
        [Rtti.Meta, BindProperty]
        [Category("Behavior")]
        public bool IsExpanded
        {
            get => mIsExpanded;
            set
            {
                OnValueChange(value, mIsExpanded);
                mIsExpanded = value;

                if(value == true)
                {
                    var arg = TtEngine.Instance.UIManager.QueryEventSync();
                    arg.RoutedEvent = ExpanderExpandedEvent;
                    arg.Source = this;
                    RaiseEvent(arg);
                    TtEngine.Instance.UIManager.ReleaseEventSync(arg);
                }
                else
                {
                    var arg = TtEngine.Instance.UIManager.QueryEventSync();
                    arg.RoutedEvent = ExpanderCollapsedEvent;
                    arg.Source = this;
                    RaiseEvent(arg);
                    TtEngine.Instance.UIManager.ReleaseEventSync(arg);
                }

                UpdateLayout();
            }
        }

        public static readonly TtRoutedEvent ExpanderExpandedEvent = TtEventManager.RegisterRoutedEvent("Expanded", ERoutedType.Bubble, typeof(TtRoutedEventHandler), typeof(TtUIElement));
        public event TtRoutedEventHandler Expanded
        {
            add { AddHandler(ExpanderExpandedEvent, value); }
            remove { RemoveHandler(ExpanderExpandedEvent, value); }
        }
        public static readonly TtRoutedEvent ExpanderCollapsedEvent = TtEventManager.RegisterRoutedEvent("Collapsed", ERoutedType.Bubble, typeof(TtRoutedEventHandler), typeof(TtUIElement));
        public event TtRoutedEventHandler Collapsed
        {
            add { AddHandler(ExpanderCollapsedEvent, value); }
            remove { RemoveHandler(ExpanderCollapsedEvent, value); }
        }

        public TtExpander()
        {
            Template = TtEngine.Instance.UIManager.GetDefaultTemplate(TtTypeDesc.TypeOf(typeof(TtExpander)));
            Template.Seal();
        }
    }
}
