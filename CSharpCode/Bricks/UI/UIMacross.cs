using EngineNS.Profiler;
using EngineNS.UI.Controls;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.UI
{
    [Bind.BindableObject]
    public partial class TtUIMacrossBase
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
		private static EngineNS.Macross.TtMacrossBreak macross_break_FindElement_107167771 = new EngineNS.Macross.TtMacrossBreak("EngineNS.UI.TtUIMacrossBase->TtUIElement FindElement(string name)");
		public unsafe TtUIElement macross_FindElement (string nodeName, string name) 
		{
			using(var stackframe = EngineNS.Macross.TtMacrossStackTracer.CurrentFrame)
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":name", name);
				}
			}
			var _return_value = FindElement(name);
			macross_break_FindElement_107167771.TryBreak();
			return _return_value;
		}
		private static EngineNS.Macross.TtMacrossBreak macross_break_FindElement_2697509680 = new EngineNS.Macross.TtMacrossBreak("EngineNS.UI.TtUIMacrossBase->TtUIElement FindElement(UInt64 id)");
		public unsafe TtUIElement macross_FindElement (string nodeName, UInt64 id) 
		{
			using(var stackframe = EngineNS.Macross.TtMacrossStackTracer.CurrentFrame)
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":id", id);
				}
			}
			var _return_value = FindElement(id);
			macross_break_FindElement_2697509680.TryBreak();
			return _return_value;
		}
		private static EngineNS.Macross.TtMacrossBreak macross_break_FindElement_4080205626 = new EngineNS.Macross.TtMacrossBreak("EngineNS.UI.TtUIMacrossBase->TtUIElement FindElement(System.Type rType, string name)");
		public unsafe TtUIElement macross_FindElement (string nodeName, System.Type rType, string name) 
		{
			using(var stackframe = EngineNS.Macross.TtMacrossStackTracer.CurrentFrame)
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":rType", rType);
					stackframe.SetWatchVariable(nodeName + ":name", name);
				}
			}
			var _return_value = FindElement(rType, name);
			macross_break_FindElement_4080205626.TryBreak();
			return _return_value;
		}
		private static EngineNS.Macross.TtMacrossBreak macross_break_FindElement_2174374925 = new EngineNS.Macross.TtMacrossBreak("EngineNS.UI.TtUIMacrossBase->TtUIElement FindElement(System.Type rType, UInt64 id)");
		public unsafe TtUIElement macross_FindElement (string nodeName, System.Type rType, UInt64 id) 
		{
			using(var stackframe = EngineNS.Macross.TtMacrossStackTracer.CurrentFrame)
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":rType", rType);
					stackframe.SetWatchVariable(nodeName + ":id", id);
				}
			}
			var _return_value = FindElement(rType, id);
			macross_break_FindElement_2174374925.TryBreak();
			return _return_value;
		}
	}
}
#endregion//TitanEngine_AutoGen_Macross
#endif//TitanEngine_AutoGen_Macross