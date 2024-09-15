using EngineNS.Bricks.CodeBuilder;
using EngineNS.DesignMacross.Base.Description;
using EngineNS.DesignMacross.Design;
using EngineNS.DesignMacross.Design.ConnectingLine;
using EngineNS.Rtti;
using System;
using System.Collections.Generic;
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
    public class TtBlendTreeNodeClassDescription : TtDesignableVariableDescription
    {
        [Rtti.Meta]
        public List<TtDataInPinDescription> DataInPins { get; set; } = new();
        [Rtti.Meta]
        public List<TtPoseInPinDescription> PoseInPins { get; set; } = new();
        [Rtti.Meta]
        public List<TtPoseOutPinDescription> PoseOutPins { get; set; } = new();
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
        public List<TtPoseInPinDescription> GetDataInPins(TtTypeDesc typeDesc)
        {
            var pins = new List<TtPoseInPinDescription>();
            foreach (var pin in PoseInPins)
            {
                if (pin.TypeDesc == typeDesc)
                {
                    pins.Add(pin);
                }
            }
            return pins;
        }
        public List<TtPoseOutPinDescription> GetDataOutPins(TtTypeDesc typeDesc)
        {
            var pins = new List<TtPoseOutPinDescription>();
            foreach (var pin in PoseOutPins)
            {
                if (pin.TypeDesc == typeDesc)
                {
                    pins.Add(pin);
                }
            }
            return pins;
        }

        public virtual TtStatementBase BuildBlendTreeStatement(ref FBlendTreeBuildContext blendTreeBuildContext)
        {
            return null;
        }
    }
}
