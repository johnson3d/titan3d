﻿using EngineNS.Bricks.CodeBuilder;
using EngineNS.DesignMacross.Base.Description;
using EngineNS.DesignMacross.Design.ConnectingLine;
using EngineNS.Rtti;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;

namespace EngineNS.DesignMacross.Design.Expressions
{
    [EGui.Controls.PropertyGrid.PGCategoryFilters(ExcludeFilters = new string[] { "Misc" })]
    public class TtExpressionDescription : IExpressionDescription
    {
        [Rtti.Meta]
        public Guid Id { get; set; } = Guid.NewGuid();
        [Rtti.Meta, Category("Option")]
        public virtual string Name { get; set; } = "ExpressionDescription";
        public virtual IDescription Parent { get; set; }

        [Rtti.Meta]
        public List<TtExecutionInPinDescription> ExecutionInPins { get; set; } = new();
        [Rtti.Meta]
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
        [Rtti.Meta]
        public List<TtDataInPinDescription> DataInPins { get; set; } = new();
        [Rtti.Meta]
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

        public virtual void Initailize()
        {

        }
        public virtual TtExpressionBase BuildExpression(ref FExpressionBuildContext expressionBuildContext)
        {
            return null;
        }
        public void AddDataInPin(TtDataInPinDescription pinDescription)
        {
            if(!DataInPins.Contains(pinDescription))
            {
                pinDescription.Parent = this;
                DataInPins.Add(pinDescription);
            }
        }
        public void AddDataOutPin(TtDataOutPinDescription pinDescription)
        {
            if(!DataOutPins.Contains(pinDescription))
            {
                pinDescription.Parent = this;
                DataOutPins.Add(pinDescription);
            }
        }
        public virtual void OnPinConnected(TtDataPinDescription selfPin, TtDataPinDescription connectedPin, TtMethodDescription methodDescription)
        {

        }
        public virtual void OnPinDisConnected(TtDataPinDescription selfPin, TtDataPinDescription disConnectedPin, TtMethodDescription methodDescription)
        {

        }
        public virtual bool PinsChecking(TtPinsCheckContext pinsCheckContext)
        {
            bool isPinsCorrect = true;
            var methodDesc = pinsCheckContext.MethodDescription;
            foreach (var inPin in DataInPins)
            {
                var linkedPin = methodDesc.GetLinkedDataPin(inPin);
                if (linkedPin != null)
                {
                    if (linkedPin.TypeDesc == inPin.TypeDesc)
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
            return isPinsCorrect;
        }
        public void AddExecutionInPin(TtExecutionInPinDescription pinDescription)
        {
            if(!ExecutionInPins.Contains(pinDescription))
            {
                pinDescription.Parent = this;
                ExecutionInPins.Add(pinDescription);
            }
        }
        public void AddExecutionOutPin(TtExecutionOutPinDescription pinDescription)
        {
            if(!ExecutionOutPins.Contains(pinDescription))
            {
                pinDescription.Parent = this;
                ExecutionOutPins.Add(pinDescription);
            }
        }
        public bool TryGetDataPin(Guid pinId, out TtDataPinDescription pin)
        {
            foreach(var dataPin in DataInPins)
            {
                if(dataPin.Id == pinId)
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
        public bool TryGetDataPin(string pinName, out TtDataPinDescription pin)
        {
            foreach (var dataPin in DataInPins)
            {
                if (dataPin.Name == pinName)
                {
                    pin = dataPin;
                    return true;
                }
            }
            foreach (var dataPin in DataOutPins)
            {
                if (dataPin.Name == pinName)
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
            foreach(var executionPin in ExecutionInPins)
            {
                if(executionPin.Id == pinId)
                {
                    pin = executionPin;
                    return true;
                }
            }
            foreach (var executionPin in ExecutionOutPins)
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
        public virtual bool IsPinsLinkable(TtDataPinDescription selfPin, TtDataPinDescription targetPin)
        {
            return selfPin.TypeDesc == targetPin.TypeDesc;
        }
        public List<TtDataInPinDescription> GetDataInPins(TtTypeDesc typeDesc)
        {
            var pins = new List<TtDataInPinDescription>();
            foreach(var pin in DataInPins)
            {
                if(pin.TypeDesc == typeDesc || typeDesc == null)
                {
                    pins.Add(pin);
                }
            }
            return pins;
        }

        public virtual bool TryGetLinkableDataInPins(TtDataPinDescription targetPin, out List<TtDataInPinDescription> outLinkablePins)
        {
            var pins = new List<TtDataInPinDescription>();
            foreach (var pin in DataInPins)
            {
                if (IsPinsLinkable(pin, targetPin))
                {
                    pins.Add(pin);
                }
            }
            outLinkablePins = pins;
            if(pins.Count > 0)
            {
                return true;
            }
            return false;
        }
        public List<TtDataOutPinDescription> GetDataOutPins(TtTypeDesc typeDesc)
        {
            var pins = new List<TtDataOutPinDescription>();
            foreach (var pin in DataOutPins)
            {
                if (pin.TypeDesc == typeDesc || typeDesc == null)
                {
                    pins.Add(pin);
                }
            }
            return pins;
        }
        public virtual bool TryGetLinkableDataOutPins(TtDataPinDescription targetPin, out List<TtDataOutPinDescription> outLinkablePins)
        {
            var pins = new List<TtDataOutPinDescription>();
            foreach (var pin in DataOutPins)
            {
                if (IsPinsLinkable(pin, targetPin))
                {
                    pins.Add(pin);
                }
            }
            outLinkablePins = pins;
            if (pins.Count > 0)
            {
                return true;
            }
            return false;
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
