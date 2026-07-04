using EngineNS.Bricks.CodeBuilder;
using EngineNS.DesignMacross.Base.Description;
using EngineNS.DesignMacross.Design.ConnectingLine;
using EngineNS.DesignMacross.Design.Expressions;
using EngineNS.Rtti;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;

namespace EngineNS.DesignMacross.Design.Statement
{
    [EGui.Controls.PropertyGrid.TtCategoryFilters(ExcludeFilters = new string[] { "Misc" })]
    public class TtStatementDescription : IStatementDescription, IDataPinOperator, IExecutionPinOperator
    {
        [Rtti.Meta("")]
        public Guid Id { get; set; } = Guid.NewGuid();
        [Rtti.Meta, Category("Option")]
        public virtual string Name { get; set; } = "StatementDescription";
        public virtual IDescription Parent { get; set; }
        [Rtti.Meta("")]
        public List<TtExecutionInPinDescription> ExecutionInPins { get; set; } = new();
        [Rtti.Meta("")]
        public List<TtExecutionOutPinDescription> ExecutionOutPins { get; set; } = new();
        public List<TtExecutionPinDescription> ExecutionPins
        {
            get
            {
                List<TtExecutionPinDescription> pins = new();
                pins.AddRange(ExecutionInPins);
                pins.AddRange(ExecutionOutPins);
                return pins; ;
            }
        }
        [Rtti.Meta("")]
        public List<TtDataInPinDescription> DataInPins { get; set; } = new();
        [Rtti.Meta("")]
        public List<TtDataOutPinDescription> DataOutPins { get; set; } = new();
        public List<TtDataPinDescription> DataPins
        {
            get
            {
                List<TtDataPinDescription> pins = new();
                pins.AddRange(DataInPins);
                pins.AddRange(DataOutPins);
                return pins; ;
            }
        }
        public virtual void UpdateData(ref FDescriptionUpdateContext updateContext)
        {
            foreach (var executionPin in ExecutionPins)
            {
                executionPin.UpdateData(ref updateContext);
            }
            foreach (var dataPin in DataPins)
            {
                dataPin.UpdateData(ref updateContext);
            }
        }
        public virtual TtStatementBase BuildStatement(ref FStatementBuildContext statementBuildContext)
        {
            return null;
        }
        public virtual TtExpressionBase BuildExpressionForOutPin(TtDataPinDescription pin) { return null; }
        #region IDataPinOperator
        public void AddDataInPin(TtDataInPinDescription pinDescription)
        {
            if (!DataInPins.Contains(pinDescription))
            {
                pinDescription.Parent = this;
                DataInPins.Add(pinDescription);
            }
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
        public List<TtDataInPinDescription> GetDataInPins(TtTypeDesc typeDesc)
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
            foreach (var dataPin in DataOutPins)
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
        public List<TtDataOutPinDescription> GetDataOutPins(TtTypeDesc typeDesc)
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
        public virtual bool IsDataPinsLinkable(TtDataPinDescription selfPin, TtDataPinDescription targetPin)
        {
            return selfPin.TypeDesc == targetPin.TypeDesc;
        }
        public virtual void OnDataPinConnected(TtDataPinDescription selfPin, TtDataPinDescription connectedPin, IDescription graphDescription)
        {

        }
        public virtual void OnDataPinDisConnected(TtDataPinDescription selfPin, TtDataPinDescription connectedPin, IDescription graphDescription)
        {

        }
        #endregion
        public virtual bool PinsChecking(TtPinsCheckContext pinsCheckContext)
        {
            bool isPinsCorrect = true;
            var graphDesc = pinsCheckContext.GraphDescription;
            foreach (var inDataPin in DataInPins)
            {
                if(graphDesc is IDataLineOperator lineOperator)
                {
                    var linkedPin = lineOperator.GetLinkedDataPin(inDataPin);
                    if (linkedPin != null)
                    {
                        if (linkedPin.TypeDesc == inDataPin.TypeDesc)
                        {
                            if (linkedPin.Parent is TtExpressionDescription linkedExpressionDesc)
                            {
                                isPinsCorrect = linkedExpressionDesc.PinsChecking(pinsCheckContext);
                            }
                        }
                        else
                        {
                            pinsCheckContext.ErrorDescriptions.Add(this);
                            isPinsCorrect = false;
                        }
                    }
                }
            }
            foreach(var outExecPin in ExecutionOutPins)
            {
                if (graphDesc is IExecutionLineOperator lineOperator)
                {
                    var linkedPin = lineOperator.GetLinkedExecutionPin(outExecPin);
                    if (linkedPin != null)
                    {
                        if (linkedPin.Parent is TtStatementDescription linkedDesc)
                        {
                            isPinsCorrect = linkedDesc.PinsChecking(pinsCheckContext);
                        }
                    }
                }
            }
            return isPinsCorrect;
        }

        #region IExecutionPinOperator
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

        public bool RemoveExecutionInPin(TtExecutionInPinDescription pinDescription)
        {
            pinDescription.Parent = null;
            return ExecutionInPins.Remove(pinDescription);
        }
        public bool RemoveExecutionOutPin(TtExecutionOutPinDescription pinDescription)
        {
            pinDescription.Parent = null;
            return ExecutionOutPins.Remove(pinDescription);
        }

        public int GetExecuteOutPinIndex(TtExecutionOutPinDescription pinDescription)
        {
            return ExecutionOutPins.IndexOf(pinDescription);
        }
        public bool TryGetExecutionPin(Guid pinId, out TtExecutionPinDescription execPin)
        {
            foreach (var executionPin in ExecutionInPins)
            {
                if (executionPin.Id == pinId)
                {
                    execPin = executionPin;
                    return true;
                }
            }
            foreach (var executionPin in ExecutionOutPins)
            {
                if (executionPin.Id == pinId)
                {
                    execPin = executionPin;
                    return true;
                }
            }

            execPin = null;
            return false;
        }
        public void InsertExecuteOutPin(int index, TtExecutionOutPinDescription pinDescription)
        {
            if (index < 0)
                return;
            if (!ExecutionOutPins.Contains(pinDescription))
            {
                pinDescription.Parent = this;
                if (index > ExecutionOutPins.Count)
                    ExecutionOutPins.Add(pinDescription);
                else
                    ExecutionOutPins.Insert(index, pinDescription);
            }
        }
        #endregion

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

        public void OnPropertyRead(object tagObject, string prop, bool fromXml)
        {
            
        }
        public void OnPostRead(object tagObject, object hostObject, bool fromXml) { }
        public void OnPropertyWrite(string prop, bool fromXml)
        {

        }
    }
}
