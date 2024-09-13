using EngineNS.Bricks.CodeBuilder;
using EngineNS.DesignMacross.Base.Description;
using EngineNS.DesignMacross.Design.ConnectingLine;
using EngineNS.Rtti;
using System.Diagnostics;
using System.Reflection;

namespace EngineNS.DesignMacross.Design.Statement
{
    [EGui.Controls.PropertyGrid.PGCategoryFilters(ExcludeFilters = new string[] { "Misc" })]
    public class TtStatementDescription : IStatementDescription
    {
        [Rtti.Meta]
        public Guid Id { get; set; } = Guid.NewGuid();
        [Rtti.Meta]
        public virtual string Name { get; set; } = "StatementDescription";
        public IDescription Parent { get; set; }
        [Rtti.Meta]
        public List<TtExecutionInPinDescription> ExecutionInPins { get; set; } = new();
        [Rtti.Meta]
        public List<TtExecutionOutPinDescription> ExecutionOutPins { get; set; } = new();
        [Rtti.Meta]
        public List<TtDataInPinDescription> DataInPins { get; set; } = new();
        [Rtti.Meta]
        public List<TtDataOutPinDescription> DataOutPins { get; set; } = new();
        public virtual  TtStatementBase BuildStatement(ref FStatementBuildContext statementBuildContext)
        {
            return null;
        }
        public void AddDataInPin(TtDataInPinDescription pinDescription)
        {
            if (!DataInPins.Contains(pinDescription))
            {
                pinDescription.Parent = this;
                DataInPins.Add(pinDescription);
            }
        }
        public void ClearDataInPin()
        {
            foreach(var pin in DataInPins)
            {
                pin.Parent = null;
            }
            DataInPins.Clear();
        }
        public bool RemoveDataInPin(TtDataInPinDescription pinDescription)
        {
            if (DataInPins.Contains(pinDescription))
            {
                pinDescription.Parent = null;
                DataInPins.Remove(pinDescription);
                return true;
            }
            return false;
        }
        public void AddDataOutPin(TtDataOutPinDescription pinDescription)
        {
            if (!DataOutPins.Contains(pinDescription))
            {
                pinDescription.Parent = this;
                DataOutPins.Add(pinDescription);
            }
        }
        public void ClearDataOutPin()
        {
            foreach (var pin in DataOutPins)
            {
                pin.Parent = null;
            }
            DataOutPins.Clear();
        }
        public bool RemoveDataOutPin(TtDataOutPinDescription pinDescription)
        {
            if (DataOutPins.Contains(pinDescription))
            {
                pinDescription.Parent = null;
                DataOutPins.Remove(pinDescription);
                return true;
            }
            return false;
        }
        public bool RemoveDataOutPin(string name)
        {
            foreach(var pin in DataOutPins)
            {
                if (pin.Name == name)
                {
                    RemoveDataOutPin(pin);
                    return true;
                }
            }
            return false;
        }
        public void AddExecutionInPin(TtExecutionInPinDescription pinDescription)
        {
            if (!ExecutionInPins.Contains(pinDescription))
            {
                pinDescription.Parent = this;
                ExecutionInPins.Add(pinDescription);
            }
        }
        public void AddExecutionOutPin(TtExecutionOutPinDescription pinDescription)
        {
            if (!ExecutionOutPins.Contains(pinDescription))
            {
                pinDescription.Parent = this;
                ExecutionOutPins.Add(pinDescription);
            }
        }
        public bool TryGetDataPin(Guid pinId, out TtDataPinDescription pin)
        {
            foreach (var dataPin in DataInPins)
            {
                if (dataPin.Id == pinId)
                {
                    pin = dataPin;
                    return true;
                }
            }
            pin = null;
            return false;
        }
        public bool TryGetExecutePin(Guid pinId, out TtExecutionPinDescription pin)
        {
            foreach (var executionPin in ExecutionInPins)
            {
                if (executionPin.Id == pinId)
                {
                    pin = executionPin;
                    return true;
                }
            }
            pin = null;
            return false;
        }
        public List<TtDataInPinDescription> GetDataInPins(UTypeDesc typeDesc)
        {
            var pins = new List<TtDataInPinDescription>();
            foreach (var pin in DataInPins)
            {
                if (pin.TypeDesc == typeDesc)
                {
                    pins.Add(pin);
                }
            }
            return pins;
        }
        public List<TtDataOutPinDescription> GetDataOutPins(UTypeDesc typeDesc)
        {
            var pins = new List<TtDataOutPinDescription>();
            foreach (var pin in DataOutPins)
            {
                if (pin.TypeDesc == typeDesc)
                {
                    pins.Add(pin);
                }
            }
            return pins;
        }
        public List<TtExecutionInPinDescription> GetExecutionInPins()
        {
            return ExecutionInPins;
        }
        public List<TtExecutionOutPinDescription> GetExecutionOutPins()
        {
            return ExecutionOutPins;
        }
        public void OnPreRead(object tagObject, object hostObject, bool fromXml)
        {
            if (hostObject is IDescription parentDescription)
            {
                Parent = parentDescription;
            }
            else
            {
                Debug.Assert(false);
            }
        }

        public void OnPropertyRead(object tagObject, PropertyInfo prop, bool fromXml)
        {
            
        }
    }
}
