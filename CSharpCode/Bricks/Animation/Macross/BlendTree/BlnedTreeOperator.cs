using EngineNS.DesignMacross.Base.Description;
using EngineNS.DesignMacross.Design.ConnectingLine;
using EngineNS.DesignMacross.Design.Expressions;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Animation.Macross.BlendTree
{
    public interface IPosePinOperator
    {

        public List<TtPoseInPinDescription> PoseInPins { get; } 

        public List<TtPoseOutPinDescription> PoseOutPins { get; }
    }
    public interface IPoseLineOperator
    {
        public List<TtPoseLineDescription> PoseLines { get; }
        public void AddPoseLine(TtPoseLineDescription poseLine);

        public bool RemovePoseLine(TtPoseLineDescription poseLine);
        public TtPoseLineDescription GetPoseLineWithPin(TtPosePinDescription dataPin);
        public TtPosePinDescription GetLinkedPosePin(TtPosePinDescription posePin);
        public bool ContainsPoseLineBetweenPins(TtPosePinDescription pinA, TtPosePinDescription pinB);
        public bool ContainsPoseLineBetweenPins(Guid pinAId, Guid pinBId);

        public static void AddPoseLine(IDescription operatorDescription, TtPoseLineDescription poseLine)
        {
            if (operatorDescription is IPoseLineOperator poseLineOperator)
            {
                poseLineOperator.AddPoseLine(poseLine);
            }
            else
            {
                System.Diagnostics.Debug.Assert(false, $"Operator {operatorDescription.Name} does not implement IPoseLineOperator");
            }
        }
        public static bool RemovePoseLine(IDescription operatorDescription, TtPoseLineDescription poseLine)
        {
            if (operatorDescription is IPoseLineOperator poseLineOperator)
            {
                return poseLineOperator.RemovePoseLine(poseLine);
            }
            else
            {
                System.Diagnostics.Debug.Assert(false, $"Operator {operatorDescription.Name} does not implement IPoseLineOperator");
                return false;
            }
        }
        public static TtPoseLineDescription GetPoseLineWithPin(IDescription operatorDescription, TtPosePinDescription dataPin)
        {
            if (operatorDescription is IPoseLineOperator poseLineOperator)
            {
                return poseLineOperator.GetPoseLineWithPin(dataPin);
            }
            else
            {
                System.Diagnostics.Debug.Assert(false, $"Operator {operatorDescription.Name} does not implement IPoseLineOperator");
                return null;
            }
        }
        public static TtPosePinDescription GetLinkedPosePin(IDescription operatorDescription, TtPosePinDescription posePin)
        {
            if (operatorDescription is IPoseLineOperator poseLineOperator)
            {
                return poseLineOperator.GetLinkedPosePin(posePin);
            }
            else
            {
                System.Diagnostics.Debug.Assert(false, $"Operator {operatorDescription.Name} does not implement IPoseLineOperator");
                return null;
            }
        }
        public static bool ContainsPoseLineBetweenPins(IDescription operatorDescription, TtPosePinDescription pinA, TtPosePinDescription pinB)
        {
            if (operatorDescription is IPoseLineOperator poseLineOperator)
            {
                return poseLineOperator.ContainsPoseLineBetweenPins(pinA, pinB);
            }
            else
            {
                System.Diagnostics.Debug.Assert(false, $"Operator {operatorDescription.Name} does not implement IPoseLineOperator");
                return false;
            }
        }
        public static bool ContainsPoseLineBetweenPins(IDescription operatorDescription, Guid pinAId, Guid pinBId)
        {
            if (operatorDescription is IPoseLineOperator poseLineOperator)
            {
                return poseLineOperator.ContainsPoseLineBetweenPins(pinAId, pinBId);
            }
            else
            {
                System.Diagnostics.Debug.Assert(false, $"Operator {operatorDescription.Name} does not implement IPoseLineOperator");
                return false;
            }
        }

        public static (TtPoseLineDescription fromPinLine, TtPoseLineDescription toPinLine) GetExistSingleLinkPoseLine(IDescription graphDesc, TtPosePinDescription fromPin, TtPosePinDescription toPin = null)
        {
            (TtPoseLineDescription fromPinLine, TtPoseLineDescription toPinLine) existLinePair;
            existLinePair.fromPinLine = null;
            existLinePair.toPinLine = null;

            if (fromPin != null && !fromPin.CanMutilLink)
            {
                existLinePair.fromPinLine = GetPoseLineWithPin(graphDesc, fromPin);
            }
            if (toPin != null && !toPin.CanMutilLink)
            {
                existLinePair.toPinLine = GetPoseLineWithPin(graphDesc, toPin);
            }
            return existLinePair;
        }
        public static void AddSinglePoseLine(IDescription graphDesc, TtPoseLineDescription lineToBeAdded, (TtPoseLineDescription fromPinLine, TtPoseLineDescription toPinLine) existLines)
        {
            if (existLines.fromPinLine != null)
            {
                RemovePoseLine(graphDesc, existLines.fromPinLine);
            }
            if (existLines.toPinLine != null)
            {
                RemovePoseLine(graphDesc, existLines.toPinLine);
            }
            AddPoseLine(graphDesc, lineToBeAdded);
        }
        public static void RemoveSinglePoseLine(IDescription graphDesc, TtPoseLineDescription lineToBeRemoved, (TtPoseLineDescription fromPinLine, TtPoseLineDescription toPinLine) existLines)
        {
            if (existLines.fromPinLine != null)
            {
                AddPoseLine(graphDesc, existLines.fromPinLine);
            }
            if (existLines.toPinLine != null)
            {
                AddPoseLine(graphDesc, existLines.toPinLine);
            }
            RemovePoseLine(graphDesc, lineToBeRemoved);
        }

        public static bool TryGetLinkedPoseLineWithPin(IPosePinOperator pinOperator, TtPosePinDescription startPin, out TtPoseLineDescription line)
        {
            var fromId = Guid.Empty;
            var fromDescName = "";
            var toId = Guid.Empty;
            var toDescName = "";
            bool existLink = false;
            if (startPin is TtPoseInPinDescription)
            {
                var pins = pinOperator.PoseOutPins;
                if (pins.Count > 0)
                {
                    var firstPin = pins[0];
                    fromId = firstPin.Id;
                    fromDescName = firstPin.Parent.Name;
                    toId = startPin.Id;
                    toDescName = startPin.Parent.Name;
                    existLink = true;
                }
            }
            else
            {
                var pins = pinOperator.PoseInPins;
                if (pins.Count > 0)
                {
                    var firstPin = pins[0];
                    fromId = startPin.Id;
                    fromDescName = startPin.Parent.Name;
                    toId = firstPin.Id;
                    toDescName = firstPin.Parent.Name;
                    existLink = true;
                }
            }
            if (existLink)
            {
                line = new TtPoseLineDescription() { Name = "Exec_" + fromDescName + "_To_" + toDescName, FromId = fromId, ToId = toId };
                return true;
            }
            else
            {
                line = null;
                return false;
            }
        }
    }
}
