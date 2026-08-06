using EngineNS.Bricks.CodeBuilder;
using EngineNS.DesignMacross.Base.Description;
using EngineNS.DesignMacross.Design;
using EngineNS.DesignMacross.Design.ConnectingLine;
using EngineNS.Rtti;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EngineNS.Animation.Macross.BlendTree.Node
{
    public struct FBlendTreeBuildContext
    {
        public TtBlendTreeClassDescription BlendTreeDescription { get; set; }
        public TtExecuteSequenceStatement ExecuteSequenceStatement { get; set; }
        public void AddStatement(TtStatementBase statement)
        {
            if (ExecuteSequenceStatement == null)
                return;
            ExecuteSequenceStatement.Sequence.Add(statement);
        }
    }
    public class TtBlendTreeNodeClassDescription : TtDesignableVariableDescription, IDataPinOperator, IPosePinOperator, IAnimMacrossClassDescription
    {
        [Rtti.Meta("")]
        public List<TtDataInPinDescription> DataInPins { get; set; } = new();
        [Rtti.Meta("")]
        public List<TtDataOutPinDescription> DataOutPins { get; set; } = new();
        [Rtti.Meta("")]
        public List<TtPoseInPinDescription> PoseInPins { get; set; } = new();
        [Rtti.Meta("")]
        public List<TtPoseOutPinDescription> PoseOutPins { get; set; } = new();
        public List<TtPosePinDescription> PosePins
        {
            get
            {
                List<TtPosePinDescription> pins = new();
                pins.AddRange(PoseInPins);
                pins.AddRange(PoseOutPins);
                return pins; ;
            }
        }
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
        public List<TtDataInPinDescription> GetDataInPins(TtTypeDesc typeDesc)
        {
            var pins = new List<TtDataInPinDescription>();
            foreach (var pin in DataInPins)
            {
                if (pin.TypeDesc == typeDesc || typeDesc == null)
                {
                    pins.Add(pin);
                }
            }
            return pins;
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
        public virtual bool IsDataPinsLinkable(TtDataPinDescription selfPin, TtDataPinDescription targetPin)
        {
            return selfPin.TypeDesc == targetPin.TypeDesc;
        }
        public virtual void OnDataPinConnected(TtDataPinDescription selfPin, TtDataPinDescription connectedPin, IDescription graphDescription)
        {

        }
        public virtual void OnDataPinDisConnected(TtDataPinDescription selfPin, TtDataPinDescription disConnectedPin, IDescription graphDescription)
        {

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
        public bool TryGetDataPin(string pinName, out TtPosePinDescription pin)
        {
            foreach (var dataPin in PoseInPins)
            {
                if (dataPin.Name == pinName)
                {
                    pin = dataPin;
                    return true;
                }
            }
            foreach (var dataPin in PoseOutPins)
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

        public void AddPoseInPin(TtPoseInPinDescription pinDescription)
        {
            if (!PoseInPins.Contains(pinDescription))
            {
                pinDescription.Parent = this;
                PoseInPins.Add(pinDescription);
            }
        }
        public void AddPoseOutPin(TtPoseOutPinDescription pinDescription)
        {
            if (!PoseOutPins.Contains(pinDescription))
            {
                pinDescription.Parent = this;
                PoseOutPins.Add(pinDescription);
            }
        }
        public bool TryGetPosePin(Guid pinId, out TtPosePinDescription pin)
        {
            foreach (var dataPin in PoseInPins)
            {
                if (dataPin.Id == pinId)
                {
                    pin = dataPin;
                    return true;
                }
            }
            foreach (var dataPin in PoseOutPins)
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

        public virtual TtStatementBase BuildBlendTreeStatement(ref FBlendTreeBuildContext blendTreeBuildContext)
        {
            return null;
        }
    }
}
