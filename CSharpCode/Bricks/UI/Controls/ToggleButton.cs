using EngineNS.UI.Bind;
using EngineNS.UI.Controls;
using EngineNS.UI.Event;
using Microsoft.CodeAnalysis.Operations;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.UI.Controls
{
    public partial class TtToggleButton : TtButtonBase
    {
        public static readonly TtRoutedEvent CheckedEvent = TtEventManager.RegisterRoutedEvent("Checked", ERoutedType.Bubble, typeof(TtRoutedEventHandler), typeof(TtToggleButton));
        public event TtRoutedEventHandler Checked
        {
            add { AddHandler(CheckedEvent, value); }
            remove { RemoveHandler(CheckedEvent, value); }
        }

        public static readonly TtRoutedEvent UncheckedEvent = TtEventManager.RegisterRoutedEvent("Unchecked", ERoutedType.Bubble, typeof(TtRoutedEventHandler), typeof(TtToggleButton));
        public event TtRoutedEventHandler Unchecked
        {
            add { AddHandler(UncheckedEvent, value); }
            remove { RemoveHandler(UncheckedEvent, value);}
        }

        public static readonly TtRoutedEvent IndeterminateEvent = TtEventManager.RegisterRoutedEvent("Indeterminate", ERoutedType.Bubble, typeof(TtRoutedEventHandler), typeof(TtToggleButton));
        public event TtRoutedEventHandler Indeterminate
        {
            add { AddHandler(IndeterminateEvent, value); }
            remove { RemoveHandler(IndeterminateEvent, value); }
        }

        bool? mIsChecked = false;
        [BindProperty]
        public bool? IsChecked
        {
            get => mIsChecked;
            set
            {
                OnValueChange(value, mIsChecked);
                mIsChecked = value;

                if(value == true)
                {
                    var arg = UEngine.Instance.UIManager.QueryEventSync();
                    //arg.Host = RootUIHost;
                    arg.RoutedEvent = CheckedEvent;
                    arg.Source = this;
                    OnChecked(arg);
                    UEngine.Instance.UIManager.ReleaseEventSync(arg);
                }
                else if(value == false)
                {
                    var arg = UEngine.Instance.UIManager.QueryEventSync();
                    //arg.Host = RootUIHost;
                    arg.RoutedEvent = UncheckedEvent;
                    arg.Source = this;
                    OnUnchecked(arg);
                    UEngine.Instance.UIManager.ReleaseEventSync(arg);
                }
                else
                {
                    var arg = UEngine.Instance.UIManager.QueryEventSync();
                    //arg.Host = RootUIHost;
                    arg.RoutedEvent = IndeterminateEvent;
                    arg.Source = this;
                    OnIndeterminate(arg);
                    UEngine.Instance.UIManager.ReleaseEventSync(arg);
                }
            }
        }

        protected virtual void OnChecked(TtRoutedEventArgs e)
        {
            RaiseEvent(e);
        }
        protected virtual void OnUnchecked(TtRoutedEventArgs e)
        {
            RaiseEvent(e);
        }
        protected virtual void OnIndeterminate(TtRoutedEventArgs e)
        {
            RaiseEvent(e);
        }

        [BindProperty]
        public bool IsThreeState
        {
            get => GetValue<bool>();
            set => SetValue<bool>(value);
        }

        protected override void OnClick(TtRoutedEventArgs args)
        {
            OnToggle();
            base.OnClick(args);
        }

        protected virtual void OnToggle()
        {
            bool? isChecked;
            if (IsChecked == true)
                isChecked = IsThreeState ? (bool?)null : (bool?)false;
            else
                isChecked = IsChecked.HasValue;
            IsChecked = isChecked;
        }
    }
}
