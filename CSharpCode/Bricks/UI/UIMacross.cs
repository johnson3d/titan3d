using EngineNS.Profiler;
using EngineNS.UI.Controls;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.UI
{
    [Bind.BindableObject]
    public partial class TtUIMacrossBase : Macross.AuxMacrossObject
    {
        public bool SimulateMode = false;
        public virtual void Initialize()
        {
            try
            {
                InitializeUIElementVariables();
                InitializeBindings();
                if (!SimulateMode)
                {
                    InitializeEvents();
                }
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

        [Rtti.Meta("")]
        public virtual TtUIElement FindElement(string name)
        {
            if (HostElement == null)
                return null;
            return HostElement.FindElement(name);
        }
        [Rtti.Meta("")]
        public virtual TtUIElement FindElement(UInt64 id)
        {
            if (HostElement == null)
                return null;
            return HostElement.FindElement(id);
        }
        [Rtti.Meta("")]
        public virtual TtUIElement FindElement(
            [Rtti.MetaParameter(FilterType = typeof(TtUIElement), ConvertOutArguments = Rtti.MetaParameterAttribute.EArgumentFilter.R)]
            System.Type rType,
            string name)
        {
            return FindElement(name);
        }
        [Rtti.Meta("")]
        public virtual TtUIElement FindElement(
            [Rtti.MetaParameter(FilterType = typeof(TtUIElement), ConvertOutArguments = Rtti.MetaParameterAttribute.EArgumentFilter.R)]
            System.Type rType,
            UInt64 id)
        {
            return FindElement(id);
        }
    }
}


#if TitanEngine_AutoGen_Macross
#region TitanEngine_AutoGen_Macross


namespace EngineNS.UI
{
	partial class TtUIMacrossBase
	{
		public unsafe TtUIElement macross_FindElement (EngineNS.Macross.TtMacrossStackTracer mcStack, string nodeName, string name) 
		{
			var stackframe = mcStack.TopFrame;
			{
				if(stackframe != null)
				{
				}
			}
			var _return_value = FindElement(name);
			return _return_value;
		}
		public unsafe TtUIElement macross_FindElement (EngineNS.Macross.TtMacrossStackTracer mcStack, string nodeName, UInt64 id) 
		{
			var stackframe = mcStack.TopFrame;
			{
				if(stackframe != null)
				{
				}
			}
			var _return_value = FindElement(id);
			return _return_value;
		}
		public unsafe TtUIElement macross_FindElement (EngineNS.Macross.TtMacrossStackTracer mcStack, string nodeName, System.Type rType, string name) 
		{
			var stackframe = mcStack.TopFrame;
			{
				if(stackframe != null)
				{
				}
			}
			var _return_value = FindElement(rType, name);
			return _return_value;
		}
		public unsafe TtUIElement macross_FindElement (EngineNS.Macross.TtMacrossStackTracer mcStack, string nodeName, System.Type rType, UInt64 id) 
		{
			var stackframe = mcStack.TopFrame;
			{
				if(stackframe != null)
				{
				}
			}
			var _return_value = FindElement(rType, id);
			return _return_value;
		}
	}
}
#endregion//TitanEngine_AutoGen_Macross
#endif//TitanEngine_AutoGen_Macross