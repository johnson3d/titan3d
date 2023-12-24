using EngineNS.UI.Controls;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.UI
{
    public class TtUIMacrossBase
    {
        TtUIElement HostElement;
        public virtual void InitializeEvents()
        {
            TtButton element = HostElement.FindElement("xx") as TtButton;
            element.Click += Element_Click;
        }

        private void Element_Click(object sender, TtRoutedEventArgs args)
        {
            throw new NotImplementedException();
        }
    }
}
