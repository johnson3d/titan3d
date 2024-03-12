using EngineNS.Profiler;
using EngineNS.UI.Controls;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.UI
{
    public partial class TtUIMacrossBase
    {
        public TtUIElement HostElement;
        public virtual void Initialize()
        {
            try
            {
                InitializeEvents();
                InitializeUIElementVariables();
                InitializeBindings();
            }
            catch(System.Exception ex)
            {
                Log.WriteException(ex);
            }
        }
        public virtual void InitializeEvents()
        {
            //TtButton element = HostObject.FindElement("xx") as TtButton;
            //element.Click += Element_Click;
        }

        public virtual void InitializeUIElementVariables()
        {

        }

        public virtual void InitializeBindings()
        {

        }

        //private void Element_Click(object sender, TtRoutedEventArgs args)
        //{
        //    throw new NotImplementedException();
        //}

        [Rtti.Meta]
        public virtual TtUIElement FindElement(string name)
        {
            if (HostElement == null)
                return null;
            return HostElement.FindElement(name);
        }
        [Rtti.Meta]
        public virtual TtUIElement FindElement(UInt64 id)
        {
            if (HostElement == null)
                return null;
            return HostElement.FindElement(id);
        }
        [Rtti.Meta]
        public virtual TtUIElement FindElement(
            [Rtti.MetaParameter(FilterType = typeof(TtUIElement), ConvertOutArguments = Rtti.MetaParameterAttribute.EArgumentFilter.R)]
            System.Type rType,
            string name)
        {
            return FindElement(name);
        }
        [Rtti.Meta]
        public virtual TtUIElement FindElement(
            [Rtti.MetaParameter(FilterType = typeof(TtUIElement), ConvertOutArguments = Rtti.MetaParameterAttribute.EArgumentFilter.R)]
            System.Type rType,
            UInt64 id)
        {
            return FindElement(id);
        }
    }
}
